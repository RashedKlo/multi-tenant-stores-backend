using Domain.Enums;

namespace Application.Features.Orders.DTOs;

public sealed record ChangeOrderStatusRequest(OrderStatus NewStatus, string? Note = null);