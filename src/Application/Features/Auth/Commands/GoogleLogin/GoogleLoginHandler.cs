using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.GoogleLogin;

public class GoogleLoginHandler(
    ICustomerRepository customerRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IGoogleTokenVerifier googleVerifier,
    IJwtTokenService tokenService)
    : IRequestHandler<GoogleLoginCommand, Result<AuthTokensDto>>
{
    private static readonly DateTime RefreshTokenTtl = DateTime.UtcNow.AddDays(30);

    public async Task<Result<AuthTokensDto>> Handle(
        GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        // Throws if the token is invalid/expired/wrong audience —
        // IGoogleTokenVerifier owns that validation.
        var googleUser = await googleVerifier.VerifyAsync(request.IdToken, cancellationToken);

        var customer = await customerRepository.GetByGoogleIdAsync(googleUser.GoogleId, cancellationToken);

        if (customer is null)
        {
            // Existing password account with same email gets Google linked
            // rather than creating a duplicate customer.
            customer = await customerRepository.GetByEmailAsync(googleUser.Email, cancellationToken);
            if (customer is not null)
            {
                customer.Update(customer.FirstName,customer.LastName,customer.Email,customer.PasswordHash,googleUser.GoogleId);
                customerRepository.Update(customer);
            }
            else
            {
              var newCustomer= Customer.Create(
                    googleUser.FirstName, googleUser.LastName, googleUser.Email, googleUser.GoogleId);
                    if(newCustomer.IsFailure)
                {
                    return Result<AuthTokensDto>.Failure(new Error("Customer.CreationFailed","Failed to create customer."));
                }
                customerRepository.Add(newCustomer.Value!);
            }
            await customerRepository.SaveChangesAsync(cancellationToken);
        }
  var pair = tokenService.GenerateTokenPair(customer.Id , customer.Email);
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
