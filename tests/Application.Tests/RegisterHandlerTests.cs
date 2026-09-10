using Application.Auth.Commands.Register;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace Application.Tests;

public class RegisterHandlerTests
{
    [Fact]
    public async Task Handle_WhenEmailExists_ReturnsConflict()
    {
        var customers = Substitute.For<ICustomerRepository>();
        customers.ExistsByEmailAsync("jane@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var passwordHasher = Substitute.For<IPasswordHasher>();
        var codeStore = Substitute.For<IVerificationCodeStore>();
        var emailService = Substitute.For<IEmailService>();

        var handler = new RegisterHandler(customers, passwordHasher, codeStore, emailService);

        var result = await handler.Handle(
            new RegisterCommand("Jane", "Doe", "jane@example.com", "Pa55word!"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Auth.Email.AlreadyExists");
        await customers.DidNotReceive().AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenValidRequest_CreatesCustomerAndSendsVerificationCode()
    {
        var customers = Substitute.For<ICustomerRepository>();
        customers.ExistsByEmailAsync("jane@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        customers.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Hash("Pa55word!").Returns("hashed-password");

        var codeStore = Substitute.For<IVerificationCodeStore>();
        var emailService = Substitute.For<IEmailService>();

        var handler = new RegisterHandler(customers, passwordHasher, codeStore, emailService);

        var result = await handler.Handle(
            new RegisterCommand("Jane", "Doe", "jane@example.com", "Pa55word!"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var value = result.Value ?? throw new InvalidOperationException("Expected a registered customer result.");
        value.Email.Should().Be("jane@example.com");
        await customers.Received(1).AddAsync(Arg.Is<Customer>(c => c.Email == "jane@example.com"), Arg.Any<CancellationToken>());
        await codeStore.Received(1).StoreCodeAsync(
            "jane@example.com",
            Arg.Any<string>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>());
        await emailService.Received(1).SendVerificationCodeAsync(
            "jane@example.com",
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }
}
