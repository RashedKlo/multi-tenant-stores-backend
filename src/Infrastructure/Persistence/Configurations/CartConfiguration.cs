using Domain.Aggregates.Cart;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Persistence.Configurations;
public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts", t =>
        {
            t.HasCheckConstraint("ck_carts_owner",
                "(customer_id IS NOT NULL AND guest_session_id IS NULL) OR " +
                "(customer_id IS NULL AND guest_session_id IS NOT NULL)");
        });

         builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.CustomerId).HasColumnName("customer_id").HasColumnType("uuid");
        builder.Property(x => x.GuestSessionId).HasColumnName("guest_session_id").HasColumnType("uuid");
        builder.Property(x => x.StoreId).HasColumnName("store_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz")
            .HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz")
            .HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(x => new { x.CustomerId, x.StoreId })
            .IsUnique().HasFilter("customer_id IS NOT NULL")
            .HasDatabaseName("uq_carts_customer_store");

        builder.HasIndex(x => new { x.GuestSessionId, x.StoreId })
            .IsUnique().HasFilter("guest_session_id IS NOT NULL")
            .HasDatabaseName("uq_carts_guest_store");
builder.HasMany(c => c.Items)
    .WithOne()
    .HasForeignKey(i => i.CartId)
    .OnDelete(DeleteBehavior.Cascade);

builder.Navigation(c => c.Items)
    .HasField("_items")
    .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}