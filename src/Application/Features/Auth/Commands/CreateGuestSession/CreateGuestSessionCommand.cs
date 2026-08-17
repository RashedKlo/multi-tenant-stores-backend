using Application.Auth.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.CreateGuestSession;

public record CreateGuestSessionCommand : IRequest<Result<GuestSessionDto>>;
