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
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromDays(30);

    public async Task<Result<GuestSessionDto>> Handle(
        CreateGuestSessionCommand request, CancellationToken cancellationToken)
    {
        // Raw token goes to the browser exactly once; only its hash is persisted.
        var rawToken = tokenService.GenerateOpaqueToken();

        var session = GuestSession.Create(
            tokenService.HashToken(rawToken),
            DateTime.UtcNow.Add(SessionLifetime)); // adjust if Create takes a TimeSpan

        if (session.IsFailure)
            return Result<GuestSessionDto>.Failure(session.Errors);

        repository.Add(session.Value!);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<GuestSessionDto>.Success(
            new GuestSessionDto(rawToken, session.Value!.ExpiresAt));
    }
}
