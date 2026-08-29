// Application/Features/Checkout/Commands/HandleStripeWebhook/HandleStripeWebhookCommand.cs
using Domain.Common;
using MediatR;

namespace Application.Features.Checkout.Commands.HandleStripeWebhook;

public sealed record HandleStripeWebhookCommand(
    string JsonPayload,
    string StripeSignatureHeader) : IRequest<Result>;