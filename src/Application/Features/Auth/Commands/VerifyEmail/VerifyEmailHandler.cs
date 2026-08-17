using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.VerifyEmail;

public class VerifyEmailHandler(
    ICustomerRepository customerRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IVerificationCodeStore codeStore,
    IJwtTokenService tokenService)
    : IRequestHandler<VerifyEmailCommand, Result<AuthTokensDto>>
{
    private static readonly DateTime RefreshTokenTtl = DateTime.UtcNow.AddDays(30);

    public async Task<Result<AuthTokensDto>> Handle(
        VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        if (!await codeStore.ValidateAndConsumeAsync(request.Email, request.Code, cancellationToken))
            return Result<AuthTokensDto>.Failure(
                Error.Conflict("Code", "Invalid or expired verification code."));

        var customer = await customerRepository.GetByEmailAsync(request.Email, cancellationToken);
          if (customer is null)
          return Result<AuthTokensDto>.Failure(
                Error.NotFound("Customer.NotFound", "Customer not found."));

        customer.VerifyEmail();
        customerRepository.Update(customer);
        await customerRepository.SaveChangesAsync(cancellationToken);

        var pair = tokenService.GenerateTokenPair(customer.Id, customer.Email);
        var refreshTokenHash = tokenService.HashToken(pair.RefreshToken);
        var refreshToken = Domain.Entities.RefreshToken.Create(customer.Id, refreshTokenHash, RefreshTokenTtl);
       if (refreshToken.IsFailure)
        {
            return Result<AuthTokensDto>.Failure(refreshToken.Errors);
        }
        refreshTokenRepository.Add(refreshToken.Value!);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return Result<AuthTokensDto>.Success(
            new AuthTokensDto(pair.AccessToken, pair.RefreshToken, pair.AccessTokenExpiresAt));
    }
}
