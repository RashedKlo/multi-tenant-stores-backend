using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

/// <summary>
/// Reads the current customer from the HTTP context claims.
/// Expects a claim named "sub" (JWT standard) or ClaimTypes.NameIdentifier holding a Guid.
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

            
            var value =
                user.FindFirst(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value?.Value, out var id) ? id : null;
        }
    }

  public Guid? GuestSessionId
{
    get
    {
        var ctx = httpContextAccessor.HttpContext;
        if (ctx?.Items.TryGetValue("GuestSessionId", out var value) == true
            && value is Guid id)
            return id;
        return null;
    }
}
}