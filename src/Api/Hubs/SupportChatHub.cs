// src/Api/Hubs/SupportChatHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

// No client-invokable methods — both sides only listen. Targeting is done
// server-side via Clients.User(...) in SupportChatNotifier, same as OrderTrackingHub.
[Authorize]
public sealed class SupportChatHub : Hub
{
}