// Application/Auth/Commands/CreateGuestSession/CreateGuestSessionCommand.cs
using Application.Auth.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.CreateGuestSession;

public sealed record CreateGuestSessionCommand : IRequest<Result<GuestSessionDto>>;