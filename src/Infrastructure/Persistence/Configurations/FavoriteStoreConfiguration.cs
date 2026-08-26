using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class FavoriteStoreConfiguration : IEntityTypeConfiguration<FavoriteStore>
    {
        public void Configure(EntityTypeBuilder<FavoriteStore> builder)
        {
            builder.ToTable("favorite_stores");

            builder.HasKey(x => new { x.CustomerId, x.StoreId });

            builder.Property(x => x.CustomerId)
                .HasColumnName("customer_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.StoreId)
                .HasColumnName("store_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.HasIndex(x => x.StoreId)
                .HasDatabaseName("idx_favorite_stores_store_id");

                   builder.HasOne(x=>x.Customer)
                .WithMany(x=>x.FavoriteStores)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x=>x.Store)
                .WithMany(x=>x.FavoriteStores)
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}