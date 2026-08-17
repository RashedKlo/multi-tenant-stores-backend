using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ProductOptionGroupConfiguration : IEntityTypeConfiguration<ProductOptionGroup>
    {
        public void Configure(EntityTypeBuilder<ProductOptionGroup> builder)
        {
            builder.ToTable("product_option_groups", table =>
            {
                table.HasCheckConstraint(
                    "ck_product_option_groups_name_en_not_empty",
                    "length(btrim(name_en)) > 0");

                table.HasCheckConstraint(
                    "ck_product_option_groups_name_ar_not_empty",
                    "length(btrim(name_ar)) > 0");

                table.HasCheckConstraint(
                    "ck_product_option_groups_min_selection_non_negative",
                    "min_selection >= 0");

                table.HasCheckConstraint(
                    "ck_product_option_groups_max_selection_positive",
                    "max_selection >= 1");

                table.HasCheckConstraint(
                    "ck_product_option_groups_max_gte_min",
                    "max_selection >= min_selection");

                table.HasCheckConstraint(
                    "ck_product_option_groups_single_max_one",
                    "selection_type <> 'Single' OR max_selection = 1");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.ProductId)
                .HasColumnName("product_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.NameEn)
                .HasColumnName("name_en")
                .HasColumnType("varchar(150)")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.NameAr)
                .HasColumnName("name_ar")
                .HasColumnType("varchar(150)")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.SelectionType)
                .HasColumnName("selection_type")
                .HasColumnType("selection_type")
                .HasDefaultValue(SelectionType.Single)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.MinSelection)
                .HasColumnName("min_selection")
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(x => x.MaxSelection)
                .HasColumnName("max_selection")
                .HasColumnType("int")
                .HasDefaultValue(1)
                .IsRequired();

            builder.Property(x => x.DisplayOrder)
                .HasColumnName("display_order")
                .HasColumnType("int")
                .HasDefaultValue(0)
                .IsRequired();

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

            builder.HasIndex(x => x.ProductId)
                .HasDatabaseName("idx_product_option_groups_product_id");

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}