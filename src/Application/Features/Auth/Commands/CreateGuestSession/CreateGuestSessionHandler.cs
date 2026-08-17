using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.CreateGuestSession;

public class CreateGuestSessionHandler(
    IGuestSessionRepository repository,
    IJwtTokenService tokenService)
    : IRequestHandler<CreateGuestSessionCommand, Result<GuestSessionDto>>
{
    private static readonly DateTime Ttl = DateTime.UtcNow.AddDays(30);

    public async Task<Result<GuestSessionDto>> Handle(
        CreateGuestSessionCommand request, CancellationToken cancellationToken)
    {
        // Raw token goes to the browser; only its hash is stored.
        var rawToken = tokenService.GenerateOpaqueToken();
        var session = GuestSession.Create(tokenService.HashToken(rawToken), Ttl);
          if(session is null)
            return Result<GuestSessionDto>.Failure(Error.Validation("GuestSession.Invalid", "Failed to create guest session."));
        repository.Add(session.Value!);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<GuestSessionDto>.Success(
            new GuestSessionDto(rawToken, session.Value!.ExpiresAt));
    }
}
