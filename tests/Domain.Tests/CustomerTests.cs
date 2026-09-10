using Domain.Common;
using Domain.Entities;
using FluentAssertions;

namespace Domain.Tests;

public class CustomerTests
{
    [Fact]
    public void CreateWithPassword_WhenValid_ProducesActiveCustomer()
    {
        var result = Customer.CreateWithPassword(
            "Jane",
            "Doe",
            "jane@example.com",
            "Pa55word!");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FirstName.Should().Be("Jane");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Email.Should().Be("jane@example.com");
        result.Value.IsActive.Should().BeTrue();
        result.Value.IsEmailVerified.Should().BeFalse();
    }

    [Fact]
    public void UpdateProfile_WhenLastNameIsWhitespace_ReturnsValidationError()
    {
        var customer = Customer.CreateWithPassword(
            "Jane",
            "Doe",
            "jane@example.com",
            "Pa55word!").Value!;

        var result = customer.UpdateProfile("Jane", " ");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Customer.LastName.Required");
    }
}
