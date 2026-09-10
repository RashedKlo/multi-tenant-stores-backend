using Application.Auth.Commands.Login;
using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Api.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_WhenAuthenticationFails_ReturnsUnauthorizedProblemDetails()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<AuthTokensDto>.Failure(
                Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."))));

        var controller = new Api.Controllers.AuthController(mediator);

        var result = await controller.Login(new LoginCommand("jane@example.com", "WrongPass!"), CancellationToken.None);

        var actionResult = result.Result;
        actionResult.Should().NotBeNull();
        actionResult.Should().BeOfType<UnauthorizedObjectResult>();
    }
}
