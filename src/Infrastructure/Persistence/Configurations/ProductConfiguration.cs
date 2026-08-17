using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products", table =>
            {
                table.HasCheckConstraint(
                    "ck_products_name_en_not_empty",
                    "length(btrim(name_en)) > 0");

                table.HasCheckConstraint(
                    "ck_products_name_ar_not_empty",
                    "length(btrim(name_ar)) > 0");

                table.HasCheckConstraint(
                    "ck_products_price_non_negative",
                    "price >= 0");

                table.HasCheckConstraint(
                    "ck_products_compare_price_valid",
                    "compare_price IS NULL OR compare_price >= price");

                table.HasCheckConstraint(
                    "ck_products_stock_quantity_non_negative",
                    "stock_quantity >= 0");

                table.HasCheckConstraint(
                    "ck_products_weight_non_negative",
                    "weight IS NULL OR weight >= 0");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.SectionId)
                .HasColumnName("section_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.StoreId)
                .HasColumnName("store_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.NameEn)
                .HasColumnName("name_en")
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.NameAr)
                .HasColumnName("name_ar")
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.DescriptionEn)
                .HasColumnName("description_en")
                .HasColumnType("text")
                .IsRequired(false);

            builder.Property(x => x.DescriptionAr)
                .HasColumnName("description_ar")
                .HasColumnType("text")
                .IsRequired(false);

            builder.Property(x => x.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("jsonb")
                .IsRequired(false);

            builder.Property(x => x.Price)
                .HasColumnName("price")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            builder.Property(x => x.ComparePrice)
                .HasColumnName("compare_price")
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            builder.Property(x => x.Sku)
                .HasColumnName("sku")
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(x => x.Barcode)
                .HasColumnName("barcode")
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(x => x.TrackInventory)
                .HasColumnName("track_inventory")
                .HasColumnType("boolean")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.StockQuantity)
                .HasColumnName("stock_quantity")
                .HasColumnType("integer")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(x => x.Weight)
                .HasColumnName("weight")
                .HasColumnType("numeric(10,2)")
                .IsRequired(false);

            builder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .HasColumnType("boolean")
                .HasDefaultValue(true)
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

            builder.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("timestamptz")
                .IsRequired(false);

            // Indexes
            builder.HasIndex(x => x.SectionId)
                .HasDatabaseName("idx_products_section_id");

            builder.HasIndex(x => x.StoreId)
                .HasDatabaseName("idx_products_store_id");

            // Partial unique indexes
            builder.HasIndex(x => x.Sku)
                .IsUnique()
                .HasFilter("sku IS NOT NULL")
                .HasDatabaseName("uq_products_sku");

            builder.HasIndex(x => x.Barcode)
                .IsUnique()
                .HasFilter("barcode IS NOT NULL")
                .HasDatabaseName("uq_products_barcode");

            // Relationships
            builder.HasOne<StoreSection>()
                .WithMany()
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Store>()
                .WithMany()
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}