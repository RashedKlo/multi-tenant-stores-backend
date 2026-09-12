using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

// No client-invokable methods — customers only listen.
// "Joining their own channel" happens automatically via user targeting below.
[Authorize]
public sealed class OrderTrackingHub : Hub
{
}