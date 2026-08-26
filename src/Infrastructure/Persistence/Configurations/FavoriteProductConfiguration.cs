using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace Infrastructure.Persistence.Configurations
{
    public class FavoriteProductConfiguration : IEntityTypeConfiguration<FavoriteProduct>
    {
        public void Configure(EntityTypeBuilder<FavoriteProduct> builder)
        {
            builder.ToTable("favorite_products");

            builder.HasKey(x => new { x.CustomerId, x.ProductId });

            builder.Property(x => x.CustomerId)
                .HasColumnName("customer_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.ProductId)
                .HasColumnName("product_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.HasIndex(x => x.ProductId)
                .HasDatabaseName("idx_favorite_products_product_id");

            builder.HasOne(x=>x.Customer)
                .WithMany(x=>x.FavoriteProducts)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x=>x.Product)
                .WithMany(x=>x.FavoriteProducts)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}