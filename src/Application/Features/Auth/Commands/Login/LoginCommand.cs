using Application.Auth.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Auth.Commands.Login;

// StoreId + GuestToken are optional — only present when the customer was
// mid-checkout in a specific store's guest cart and logged in from there.
public record LoginCommand(
    string Email,
    string Password,
    Guid? StoreId,
    string? GuestToken) : IRequest<Result<AuthTokensDto>>;
