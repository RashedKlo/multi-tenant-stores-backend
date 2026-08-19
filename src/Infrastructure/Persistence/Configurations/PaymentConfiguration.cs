using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("payments", table =>
            {
                table.HasCheckConstraint(
                    "ck_payments_amount_non_negative",
                    "amount >= 0");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.OrderId)
                .HasColumnName("order_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.Provider)
                .HasColumnName("provider")
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .HasDefaultValue("Stripe")
                .IsRequired();

            builder.Property(x => x.StripePaymentIntentId)
                .HasColumnName("stripe_payment_intent_id")
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasColumnType("payment_status")
                .HasConversion<string>()
                .HasDefaultValue(PaymentStatus.Pending)
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            builder.Property(x => x.Currency)
                .HasColumnName("currency")
                .HasColumnType("varchar(3)")
                .HasMaxLength(3)
                .HasDefaultValue("USD")
                .IsRequired();

            builder.Property(x => x.FailureReason)
                .HasColumnName("failure_reason")
                .HasColumnType("text")
                .IsRequired(false);

            builder.Property(x => x.ProviderMetadata)
                .HasColumnName("provider_metadata")
                .HasColumnType("jsonb")
                .IsRequired(false);

            builder.Property(x => x.PaidAt)
                .HasColumnName("paid_at")
                .HasColumnType("timestamptz")
                .IsRequired(false);

            builder.Property(x => x.RefundedAt)
                .HasColumnName("refunded_at")
                .HasColumnType("timestamptz")
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.HasIndex(x => x.OrderId)
                .IsUnique()
                .HasDatabaseName("uq_payments_order_id");

            builder.HasIndex(x => x.StripePaymentIntentId)
                .IsUnique()
                .HasDatabaseName("uq_payments_stripe_payment_intent_id");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("idx_payments_status");
builder.HasOne(p => p.Order)                    // use Payment.Order navigation
    .WithOne(o => o.Payment)                    // use Order.Payment navigation
    .HasForeignKey<Payment>(p => p.OrderId)     // Payment is the dependent
    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}