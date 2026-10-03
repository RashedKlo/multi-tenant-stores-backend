// Infrastructure/Persistence/Repositories/PaymentRepository.cs
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;
    public PaymentRepository(AppDbContext context) => _context = context;

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Payments.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);

    // Webhook lookups: tracked on purpose, the handler updates the payment right after.
    public Task<Payment?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId, CancellationToken cancellationToken = default) =>
        _context.Payments.FirstOrDefaultAsync(p => p.StripePaymentIntentId == stripePaymentIntentId, cancellationToken);

    public Task<Payment?> GetByStripeSessionIdAsync(string stripeSessionId, CancellationToken cancellationToken = default) =>
        _context.Payments.FirstOrDefaultAsync(p => p.StripeSessionId == stripeSessionId, cancellationToken);

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default) =>await  _context.Payments.AddAsync(payment, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}