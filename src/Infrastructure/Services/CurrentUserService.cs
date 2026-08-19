using System.Security.Claims;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

/// <summary>
/// Reads the current customer from the HTTP context claims.
/// Expects a claim named "sub" or ClaimTypes.NameIdentifier holding a Guid.
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public bool IsAuthenticated => CustomerId.HasValue;

    public Guid? CustomerId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            // Prefer "sub" (JWT standard), fall back to NameIdentifier
            var value =
                user.FindFirst("sub")
                ?? user.FindFirst(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value?.Value, out var id) ? id : null;
        }
    }

    public Guid? GuestSessionId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user is null)
                return null;

            var value =
                user.FindFirst("guest_session_id")
                ?? user.FindFirst("GuestSessionId")
                ?? user.FindFirst("guest_session");

            return Guid.TryParse(value?.Value, out var id) ? id : null;
        }
    }
}
