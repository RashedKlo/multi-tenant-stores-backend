using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

/// <summary>
/// Tells SignalR which claim is "the user id" for Clients.User(...).
/// Must match what CurrentUserService reads (the "sub" claim).
/// </summary>
public sealed class OrderTrackingUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => connection.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
}