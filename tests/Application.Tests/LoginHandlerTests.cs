using Application.Auth.Commands.Login;
using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace Application.Tests;

public class LoginHandlerTests
{
    [Fact]
    public async Task Handle_WhenCustomerDoesNotExist_ReturnsUnauthorized()
    {
        var customers = Substitute.For<ICustomerRepository>();
        customers.GetByEmailAsync("jane@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Customer?>(null));

        var refreshTokens = Substitute.For<IRefreshTokenRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var jwt = Substitute.For<IJwtTokenService>();

        var handler = new LoginHandler(customers, refreshTokens, passwordHasher, jwt);

        var result = await handler.Handle(
            new LoginCommand("jane@example.com", "Pa55word!"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_WhenCustomerIsUnverified_ReturnsForbidden()
    {
        var customer = Customer.CreateWithPassword(
            "Jane",
            "Doe",
            "jane@example.com",
            "hashed-password").Value!;

        var customers = Substitute.For<ICustomerRepository>();
        customers.GetByEmailAsync("jane@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Customer?>(customer));

        var refreshTokens = Substitute.For<IRefreshTokenRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify("Pa55word!", "hashed-password").Returns(true);

        var jwt = Substitute.For<IJwtTokenService>();
        jwt.GenerateTokenPair(customer.Id, customer.Email)
            .Returns(new TokenPair("access-token", "refresh-token", DateTimeOffset.UtcNow.AddMinutes(15)));

        var handler = new LoginHandler(customers, refreshTokens, passwordHasher, jwt);

        var result = await handler.Handle(
            new LoginCommand("jane@example.com", "Pa55word!"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Auth.Email.NotVerified");
    }
}
