using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

            builder.HasOne<Discount>()
                .WithMany()
                .HasForeignKey(x => x.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}