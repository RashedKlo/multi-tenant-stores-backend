using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class StoreCategoryConfiguration : IEntityTypeConfiguration<StoreCategory>
    {
        public void Configure(EntityTypeBuilder<StoreCategory> builder)
        {
            builder.ToTable("store_categories");

            builder.HasKey(x => new { x.StoreId, x.CategoryId });

            builder.Property(x => x.StoreId)
                .HasColumnName("store_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.CategoryId)
                .HasColumnName("category_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.HasIndex(x => x.CategoryId)
                .HasDatabaseName("idx_store_categories_category_id");

            builder.HasOne<Store>()
                .WithMany()
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Category>()
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}