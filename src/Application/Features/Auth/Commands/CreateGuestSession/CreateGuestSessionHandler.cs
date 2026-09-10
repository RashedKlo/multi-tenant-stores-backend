// Application/Auth/Commands/CreateGuestSession/CreateGuestSessionHandler.cs
using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.CreateGuestSession;

public sealed class CreateGuestSessionHandler(
    IGuestSessionRepository guestSessions,
    IJwtTokenService jwt)
    : IRequestHandler<CreateGuestSessionCommand, Result<GuestSessionDto>>
{
    public async Task<Result<GuestSessionDto>> Handle(
        CreateGuestSessionCommand request, CancellationToken ct)
    {
        var rawToken = jwt.GenerateOpaqueToken();
        var hash = jwt.HashToken(rawToken);
        var expiresAt = DateTime.UtcNow.AddDays(30);

        var createResult = GuestSession.Create(hash, expiresAt);
        if (createResult.IsFailure)
            return Result<GuestSessionDto>.Failure(createResult.Errors);

        await guestSessions.AddAsync(createResult.Value!, ct);
        await guestSessions.SaveChangesAsync(ct);

        return Result<GuestSessionDto>.Success(
            new GuestSessionDto(rawToken, expiresAt));
    }
}