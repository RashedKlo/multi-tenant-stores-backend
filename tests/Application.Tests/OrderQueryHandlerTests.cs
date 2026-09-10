using Application.Common.Interfaces;
using Application.Features.Orders.Queries.GetOrderById;
using Application.Features.Orders.Queries.GetOrders;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace Application.Tests;

public class OrderQueryHandlerTests
{
    [Fact]
    public async Task GetOrders_WhenAuthenticated_ReturnsCustomerOrders()
    {
        var customerId = Guid.NewGuid();
        var order = Order.Create(
            customerId,
            Guid.NewGuid(),
            "Jane Doe",
            "123 Main St",
            40.0m,
            80.0m,
            [new OrderLineInput(
                Guid.NewGuid(),
                "Mouse",
                "ماوس",
                25m,
                1,
                [])],
            addressId: Guid.NewGuid()).Value!;

        var user = Substitute.For<ICurrentUserService>();
        user.IsAuthenticated.Returns(true);
        user.CustomerId.Returns(customerId);

        var orders = Substitute.For<IOrderRepository>();
        orders.GetPagedByCustomerAsync(customerId, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<Order> { order }, 1));

        var handler = new GetOrdersHandler(orders, user);

        var result = await handler.Handle(new GetOrdersQuery(null, 1, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        result.Value.Items[0].StoreId.Should().Be(order.StoreId);
    }

    [Fact]
    public async Task GetOrderById_WhenAuthenticatedAndOwned_ReturnsDetailDto()
    {
        var customerId = Guid.NewGuid();
        var order = Order.Create(
            customerId,
            Guid.NewGuid(),
            "Jane Doe",
            "123 Main St",
            40.0m,
            80.0m,
            [new OrderLineInput(
                Guid.NewGuid(),
                "Keyboard",
                "لوحة مفاتيح",
                60m,
                1,
                [new OrderOptionInput("Backlight", "إضاءة", 10m)])],
            addressId: Guid.NewGuid()).Value!;

        var user = Substitute.For<ICurrentUserService>();
        user.IsAuthenticated.Returns(true);
        user.CustomerId.Returns(customerId);

        var orders = Substitute.For<IOrderRepository>();
        orders.GetByIdForCustomerAsync(order.Id, customerId, Arg.Any<CancellationToken>())
            .Returns(order);

        var handler = new GetOrderByIdHandler(orders, user);

        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(order.Id);
        result.Value.Items.Should().ContainSingle();
        result.Value.Items[0].NameEn.Should().Be("Keyboard");
    }
}
