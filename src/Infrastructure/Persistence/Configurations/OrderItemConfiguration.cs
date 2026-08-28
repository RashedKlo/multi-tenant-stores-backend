using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace Infrastructure.Persistence.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("order_items", table =>
            {
                table.HasCheckConstraint(
                    "ck_order_items_unit_price_non_negative",
                    "unit_price_snapshot >= 0");

                table.HasCheckConstraint(
                    "ck_order_items_quantity_positive",
                    "quantity > 0");

                table.HasCheckConstraint(
                    "ck_order_items_line_total_non_negative",
                    "line_total >= 0");
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

            builder.Property(x => x.ProductId)
                .HasColumnName("product_id")
                .HasColumnType("uuid")
                .IsRequired(false);

            builder.Property(x => x.NameEnSnapshot)
                .HasColumnName("name_en_snapshot")
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.NameArSnapshot)
                .HasColumnName("name_ar_snapshot")
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.UnitPriceSnapshot)
                .HasColumnName("unit_price_snapshot")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            builder.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(x => x.LineTotal)
                .HasColumnName("line_total")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            builder.HasIndex(x => x.OrderId)
                .HasDatabaseName("idx_order_items_order_id");

            builder.HasIndex(x => x.ProductId)
                .HasDatabaseName("idx_order_items_product_id");

            builder.HasOne(x=>x.Order)
                .WithMany(x=>x.OrderItems)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x=>x.Product)
                .WithMany(x=>x.OrderItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}