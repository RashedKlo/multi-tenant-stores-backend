// Infrastructure/Services/StripePaymentService.cs
using Application.Checkout.DTOs;
using Application.Common.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Infrastructure.Services;

public sealed class StripePaymentService : IPaymentService
{
    private readonly StripeSettings _settings;

    public StripePaymentService(IOptions<StripeSettings> settings)
    {
        _settings = settings.Value;
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<CreateCheckoutSessionResult> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var unitAmount = (long)Math.Round(request.Amount * 100m, MidpointRounding.AwayFromZero);
        if (unitAmount < 1)
            throw new InvalidOperationException("Amount must be at least 0.01.");

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = string.IsNullOrWhiteSpace(request.SuccessUrl)
                ? _settings.SuccessUrl
                : request.SuccessUrl,
            CancelUrl = string.IsNullOrWhiteSpace(request.CancelUrl)
                ? _settings.CancelUrl
                : request.CancelUrl,
            ClientReferenceId = request.OrderId.ToString(),
            CustomerEmail = request.CustomerEmail,
            Metadata = new Dictionary<string, string>
            {
                ["order_id"] = request.OrderId.ToString(),
                ["customer_id"] = request.CustomerId.ToString(),
                ["store_id"] = request.StoreId.ToString()
            },
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = (request.Currency ?? _settings.Currency).ToLowerInvariant(),
                        UnitAmount = unitAmount,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = request.Description ?? $"Order {request.OrderId:N}"
                        }
                    }
                }
            ]
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return new CreateCheckoutSessionResult(
            session.Id,
            session.Url!,
            session.PaymentIntentId);
    }
}