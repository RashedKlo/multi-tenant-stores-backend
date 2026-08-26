using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace Infrastructure.Persistence.Configurations
{
    public class DiscountProductConfiguration : IEntityTypeConfiguration<DiscountProduct>
    {
        public void Configure(EntityTypeBuilder<DiscountProduct> builder)
        {
            builder.ToTable("discount_products");

            builder.HasKey(x => new { x.DiscountId, x.ProductId });

            builder.Property(x => x.DiscountId)
                .HasColumnName("discount_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.ProductId)
                .HasColumnName("product_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.HasIndex(x => x.ProductId)
                .HasDatabaseName("idx_discount_products_product_id");

            builder.HasOne(x=>x.Discount)
                .WithMany(x=>x.DiscountProducts)
                .HasForeignKey(x => x.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x=>x.Product)
                .WithMany(x=>x.DiscountProducts)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}