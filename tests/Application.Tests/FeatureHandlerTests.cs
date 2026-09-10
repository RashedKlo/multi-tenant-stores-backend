using Application.Addresses.Commands.CreateAddress;
using Application.Addresses.Commands.SetDefaultAddress;
using Application.Common.Interfaces;
using Application.Customers.Commands.ChangePassword;
using Application.Customers.Commands.UpdateProfile;
using Application.Customers.Queries.GetMe;
using Application.Favorites.Commands.AddFavoriteProduct;
using Application.Favorites.Commands.RemoveFavoriteProduct;
using Application.Features.Cart.Commands.AddCartItem;
using Application.Features.Cart.Commands.ClearCart;
using Application.Features.Cart.Commands.UpdateCartItem;
using Domain.Aggregates.Cart;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace Application.Tests;

public class CustomerFeatureHandlerTests
{
    [Fact]
    public async Task UpdateProfile_WhenAuthenticated_UpdatesCustomerProfile()
    {
        var customer = Customer.CreateWithPassword(
            "Jane",
            "Doe",
            "jane@example.com",
            "hashed-password").Value!;

        var customers = Substitute.For<ICustomerRepository>();
        customers.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        var user = Substitute.For<ICurrentUserService>();
        user.CustomerId.Returns(customer.Id);

        var handler = new UpdateProfileHandler(customers, user);

        var result = await handler.Handle(
            new UpdateProfileCommand("Janet", "Smith"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var value = result.Value;
        value.Should().NotBeNull();
        value!.FirstName.Should().Be("Janet");
        value.LastName.Should().Be("Smith");
        await customers.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMe_WhenAuthenticated_ReturnsCustomerProfile()
    {
        var customer = Customer.CreateWithPassword(
            "Jane",
            "Doe",
            "jane@example.com",
            "hashed-password").Value!;

        var customers = Substitute.For<ICustomerRepository>();
        customers.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        var user = Substitute.For<ICurrentUserService>();
        user.CustomerId.Returns(customer.Id);

        var handler = new GetMeQueryHandler(customers, user);

        var result = await handler.Handle(new GetMeQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task ChangePassword_WhenCurrentPasswordIsIncorrect_ReturnsValidationFailure()
    {
        var customer = Customer.CreateWithPassword(
            "Jane",
            "Doe",
            "jane@example.com",
            "hashed-password").Value!;

        var customers = Substitute.For<ICustomerRepository>();
        customers.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        var user = Substitute.For<ICurrentUserService>();
        user.CustomerId.Returns(customer.Id);

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify("wrong-password", customer.PasswordHash!).Returns(false);

        var handler = new ChangePasswordHandler(customers, user, passwordHasher);

        var result = await handler.Handle(
            new ChangePasswordCommand("wrong-password", "NewPass!123"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Customer.Password.Invalid");
    }

    [Fact]
    public async Task ChangePassword_WhenUnauthenticated_ReturnsUnauthorized()
    {
        var customers = Substitute.For<ICustomerRepository>();
        var user = Substitute.For<ICurrentUserService>();
        user.CustomerId.Returns((Guid?)null);

        var handler = new ChangePasswordHandler(customers, user, Substitute.For<IPasswordHasher>());

        var result = await handler.Handle(
            new ChangePasswordCommand("Password123!", "NewPass!123"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Customer.Unauthorized");
    }
}

public class AddressFeatureHandlerTests
{
    [Fact]
    public async Task CreateAddress_WhenValidRequest_AddsAddressForCustomer()
    {
        var customerId = Guid.NewGuid();
        var user = Substitute.For<ICurrentUserService>();
        user.CustomerId.Returns(customerId);

        var addresses = Substitute.For<ICustomerAddressRepository>();
        addresses.GetDefaultForCustomerAsync(customerId, Arg.Any<CancellationToken>()).Returns((CustomerAddress?)null);

        var handler = new CreateAddressHandler(addresses, user);

        var result = await handler.Handle(
            new CreateAddressCommand("Home", 12.5m, 45.9m, "123 Main St", true),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var value = result.Value ?? throw new InvalidOperationException("Expected a created address result.");
        value.Label.Should().Be("Home");
        value.IsDefault.Should().BeTrue();
        await addresses.Received(1).AddAsync(
            Arg.Is<CustomerAddress>(a => a.CustomerId == customerId && a.IsDefault),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetDefaultAddress_WhenAddressAlreadyDefault_ReturnsSuccessWithoutChanges()
    {
        var customerId = Guid.NewGuid();
        var existing = CustomerAddress.Create(customerId, "Home", 12.5m, 45.9m, "123 Main St", true).Value!;

        var user = Substitute.For<ICurrentUserService>();
        user.CustomerId.Returns(customerId);

        var addresses = Substitute.For<ICustomerAddressRepository>();
        addresses.GetByIdForCustomerAsync(existing.Id, customerId, Arg.Any<CancellationToken>()).Returns(existing);
        addresses.GetDefaultForCustomerAsync(customerId, Arg.Any<CancellationToken>()).Returns(existing);

        var handler = new SetDefaultAddressHandler(addresses, user);

        var result = await handler.Handle(
            new SetDefaultAddressCommand(existing.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var value = result.Value ?? throw new InvalidOperationException("Expected an address result.");
        value.Id.Should().Be(existing.Id);
        await addresses.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class CartFeatureHandlerTests
{
    [Fact]
    public async Task AddCartItem_WhenCartExists_AddsItemAndPersists()
    {
        var customerId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var cart = Cart.Create(storeId, customerId: customerId).Value!;

        var user = Substitute.For<ICurrentUserService>();
        user.CustomerId.Returns(customerId);

        var carts = Substitute.For<ICartRepository>();
        carts.GetByCustomerAndStoreAsync(customerId, storeId, Arg.Any<CancellationToken>()).Returns(cart);

        var handler = new AddCartItemHandler(carts, user);

        var result = await handler.Handle(
            new AddCartItemCommand(storeId, Guid.NewGuid(), 2, "gift wrap", new[] { Guid.NewGuid() }),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        cart.Items.Should().HaveCount(1);
        cart.Items.Single().Quantity.Should().Be(2);
        await carts.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCartItem_WhenCartDoesNotExist_ReturnsNotFound()
    {
        var customerId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var user = Substitute.For<ICurrentUserService>();
        user.CustomerId.Returns(customerId);

        var carts = Substitute.For<ICartRepository>();
        carts.GetByCustomerAndStoreAsync(customerId, storeId, Arg.Any<CancellationToken>()).Returns((Cart?)null);

        var handler = new UpdateCartItemHandler(carts, user);

        var result = await handler.Handle(
            new UpdateCartItemCommand(Guid.NewGuid(), storeId, 3),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Cart.NotFound");
    }

    [Fact]
    public async Task ClearCart_WhenCartExists_RemovesAllItems()
    {
        var customerId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var cart = Cart.Create(storeId, customerId: customerId).Value!;
        cart.AddItem(Guid.NewGuid(), 1);

        var user = Substitute.For<ICurrentUserService>();
        user.CustomerId.Returns(customerId);

        var carts = Substitute.For<ICartRepository>();
        carts.GetByCustomerAndStoreAsync(customerId, storeId, Arg.Any<CancellationToken>()).Returns(cart);

        var handler = new ClearCartHandler(carts, user);

        var result = await handler.Handle(
            new ClearCartCommand(storeId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        cart.Items.Should().BeEmpty();
        await carts.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

public class FavoriteFeatureHandlerTests
{
    [Fact]
    public async Task AddFavoriteProduct_WhenAlreadyExists_DoesNotDuplicate()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var user = Substitute.For<ICurrentUserService>();
        user.IsAuthenticated.Returns(true);
        user.CustomerId.Returns(customerId);

        var favorites = Substitute.For<IFavoriteProductRepository>();
        favorites.ExistsAsync(customerId, productId, Arg.Any<CancellationToken>()).Returns(true);

        var handler = new AddFavoriteProductHandler(favorites, user);

        var result = await handler.Handle(
            new AddFavoriteProductCommand(productId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await favorites.DidNotReceive().AddAsync(Arg.Any<FavoriteProduct>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveFavoriteProduct_WhenMissing_ReturnsSuccessWithoutRemoving()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var user = Substitute.For<ICurrentUserService>();
        user.IsAuthenticated.Returns(true);
        user.CustomerId.Returns(customerId);

        var favorites = Substitute.For<IFavoriteProductRepository>();
        favorites.ExistsAsync(customerId, productId, Arg.Any<CancellationToken>()).Returns(false);

        var handler = new RemoveFavoriteProductHandler(favorites, user);

        var result = await handler.Handle(
            new RemoveFavoriteProductCommand(productId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await favorites.DidNotReceive().RemoveAsync(customerId, productId, Arg.Any<CancellationToken>());
    }
}
