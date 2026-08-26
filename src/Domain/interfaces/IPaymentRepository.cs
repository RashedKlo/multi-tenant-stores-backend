using Domain.Entities;

namespace Domain.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    // Looked up by the Stripe webhook handler using the intent id from the event payload.
    Task<Payment?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId, CancellationToken cancellationToken = default);

    void Add(Payment payment);
    void Update(Payment payment);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
