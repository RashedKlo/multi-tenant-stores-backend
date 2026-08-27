using Application.Common.Interfaces;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Middleware;

/// <summary>
/// Resolves an opaque guest-session token from the X-Guest-Session header,
/// looks up the active GuestSession by hash, and stores its Id in HttpContext.Items
/// so ICurrentUserService.GuestSessionId can read it.
/// </summary>
public class GuestSessionMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Guest-Session";
    public const string ItemsKey = "GuestSessionId";

    public async Task InvokeAsync(
        HttpContext context,
        IGuestSessionRepository repo,
        IJwtTokenService tokenService)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var values))
        {
            var raw = values.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(raw))
            {
                var hash = tokenService.HashToken(raw);
                var session = await repo.GetByTokenHashAsync(hash, context.RequestAborted);

                if (session is not null)
                {
                    context.Items[ItemsKey] = session.Id;
                }
            }
        }

        await next(context);
    }
}