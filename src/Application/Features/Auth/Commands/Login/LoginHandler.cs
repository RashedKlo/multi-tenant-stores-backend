using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.Login;

public class LoginHandler(
    ICustomerRepository customerRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService tokenService)
    : IRequestHandler<LoginCommand, Result<AuthTokensDto>>
{
    private static readonly DateTime RefreshTokenTtl = DateTime.UtcNow.AddDays(30);

    public async Task<Result<AuthTokensDto>> Handle(
        LoginCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Same generic message whether the email doesn't exist or the
        // password is wrong — don't let this endpoint confirm which emails
        // have accounts.
        if (customer is null || customer.PasswordHash is null
            || !passwordHasher.Verify(request.Password, customer.PasswordHash))
        {
            return Result<AuthTokensDto>.Failure(
                Error.Conflict("Credentials", "Invalid email or password."));
        }

        if (!customer.IsEmailVerified)
        {
            return Result<AuthTokensDto>.Failure(
                Error.Conflict("Email", "Please verify your email before logging in."));
        }

        if (!customer.IsActive || customer.IsDeleted)
        {
            return Result<AuthTokensDto>.Failure(
                Error.Conflict("Account", "This account is inactive."));
        }

        var pair = tokenService.GenerateTokenPair(customer.Id, customer.Email);
        var refreshTokenHash = tokenService.HashToken(pair.RefreshToken);
       var refreshToken = Domain.Entities.RefreshToken.Create(customer.Id, refreshTokenHash, RefreshTokenTtl);
       if (refreshToken.IsFailure)
        {
            return Result<AuthTokensDto>.Failure(refreshToken.Errors);
        }
        refreshTokenRepository.Add(refreshToken.Value!);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);
 
        return Result<AuthTokensDto>.Success(
            new AuthTokensDto(pair.AccessToken, pair.RefreshToken, pair.AccessTokenExpiresAt));
    }
}
