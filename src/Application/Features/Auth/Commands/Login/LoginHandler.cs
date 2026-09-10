// Application/Auth/Commands/Login/LoginHandler.cs
using Application.Auth.Commands.VerifyEmail;
using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.Login;

public sealed class LoginHandler(
    ICustomerRepository customers,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwt)
    : IRequestHandler<LoginCommand, Result<AuthTokensDto>>
{
    public async Task<Result<AuthTokensDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var customer = await customers.GetByEmailAsync(email, ct);

        // Generic error — do not reveal which field is wrong
        if (customer is null || customer.IsDeleted || customer.PasswordHash is null)
            return Result<AuthTokensDto>.Failure(
                Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));

        if (!passwordHasher.Verify(request.Password, customer.PasswordHash))
            return Result<AuthTokensDto>.Failure(
                Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password."));

        if (!customer.IsActive)
            return Result<AuthTokensDto>.Failure(
                Error.Forbidden("Auth.Customer.Inactive", "Account is inactive."));

        if (!customer.IsEmailVerified)
            return Result<AuthTokensDto>.Failure(
                Error.Forbidden("Auth.Email.NotVerified", "Please verify your email before logging in."));

        return await VerifyEmailHandler.IssueTokensAsync(customer, refreshTokens, jwt, ct);
    }
}