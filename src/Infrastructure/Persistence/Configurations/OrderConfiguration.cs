using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders", table =>
            {
                table.HasCheckConstraint(
                    "ck_orders_delivery_latitude_range",
                    "delivery_latitude BETWEEN -90 AND 90");

                table.HasCheckConstraint(
                    "ck_orders_delivery_longitude_range",
                    "delivery_longitude BETWEEN -180 AND 180");

                table.HasCheckConstraint(
                    "ck_orders_subtotal_non_negative",
                    "subtotal >= 0");

                table.HasCheckConstraint(
                    "ck_orders_discount_total_non_negative",
                    "discount_total >= 0");

                table.HasCheckConstraint(
                    "ck_orders_total_non_negative",
                    "total >= 0");

                table.HasCheckConstraint(
                    "ck_orders_total_calculation",
                    "total = subtotal - discount_total");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.CustomerId)
                .HasColumnName("customer_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.StoreId)
                .HasColumnName("store_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.AddressId)
                .HasColumnName("address_id")
                .HasColumnType("uuid")
                .IsRequired(false);

            builder.Property(x => x.DeliveryName)
                .HasColumnName("delivery_name")
                .HasColumnType("varchar(200)")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.DeliveryPhone)
                .HasColumnName("delivery_phone")
                .HasColumnType("varchar(30)")
                .HasMaxLength(30)
                .IsRequired(false);

            builder.Property(x => x.DeliveryAddressText)
                .HasColumnName("delivery_address_text")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(x => x.DeliveryLatitude)
                .HasColumnName("delivery_latitude")
                .HasColumnType("decimal(10,7)")
                .IsRequired();

            builder.Property(x => x.DeliveryLongitude)
                .HasColumnName("delivery_longitude")
                .HasColumnType("decimal(10,7)")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasColumnType("order_status")
                .HasConversion<string>()
                .HasDefaultValue(OrderStatus.Pending)
                .IsRequired();

            builder.Property(x => x.Subtotal)
                .HasColumnName("subtotal")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            builder.Property(x => x.DiscountTotal)
                .HasColumnName("discount_total")
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0m)
                .IsRequired();

            builder.Property(x => x.Total)
                .HasColumnName("total")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

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

            builder.HasIndex(x => x.CustomerId)
                .HasDatabaseName("idx_orders_customer_id");

            builder.HasIndex(x => x.StoreId)
                .HasDatabaseName("idx_orders_store_id");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("idx_orders_status");

            builder.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Store>()
                .WithMany()
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Composite FK (customer_id, address_id) → customer_addresses
            builder.HasOne<CustomerAddress>()
                .WithMany()
                .HasForeignKey(x => new { x.CustomerId, x.AddressId })
                .HasPrincipalKey(x => new { x.CustomerId, x.Id })
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
        }
    }
}