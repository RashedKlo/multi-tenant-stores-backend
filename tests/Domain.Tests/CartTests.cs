using Domain.Aggregates.Cart;
using FluentAssertions;

namespace Domain.Tests;

public class CartTests
{
    [Fact]
    public void Create_WhenCustomerAndGuestAreBothMissing_ReturnsValidationFailure()
    {
        var result = Cart.Create(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Cart.Owner.Required");
    }

    [Fact]
    public void AddItem_WhenSameProductAndSameOptions_CombinesQuantity()
    {
        var cart = Cart.Create(Guid.NewGuid(), customerId: Guid.NewGuid()).Value!;
        var productId = Guid.NewGuid();
        var optionId = Guid.NewGuid();

        var first = cart.AddItem(productId, 2, "note", new[] { optionId });
        var second = cart.AddItem(productId, 3, "note", new[] { optionId });

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        cart.Items.Should().HaveCount(1);
        cart.Items.Single().Quantity.Should().Be(5);
    }
}
