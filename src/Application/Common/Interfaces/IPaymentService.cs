// Application/Common/Interfaces/IPaymentService.cs
using Application.Checkout.DTOs;

namespace Application.Common.Interfaces;

public interface IPaymentService
{
    Task<CreateCheckoutSessionResult> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken = default);
}

