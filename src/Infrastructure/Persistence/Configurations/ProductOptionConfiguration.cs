using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ProductOptionConfiguration : IEntityTypeConfiguration<ProductOption>
    {
        public void Configure(EntityTypeBuilder<ProductOption> builder)
        {
            builder.ToTable("product_options", table =>
            {
                table.HasCheckConstraint(
                    "ck_product_options_name_en_not_empty",
                    "length(btrim(name_en)) > 0");

                table.HasCheckConstraint(
                    "ck_product_options_name_ar_not_empty",
                    "length(btrim(name_ar)) > 0");

                table.HasCheckConstraint(
                    "ck_product_options_display_order_non_negative",
                    "display_order >= 0");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.OptionGroupId)
                .HasColumnName("option_group_id")
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

            builder.Property(x => x.PriceAdjustment)
                .HasColumnName("price_adjustment")
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0m)
                .IsRequired();

            builder.Property(x => x.IsDefault)
                .HasColumnName("is_default")
                .HasColumnType("boolean")
                .HasDefaultValue(false)
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

            builder.HasIndex(x => x.OptionGroupId)
                .HasDatabaseName("idx_product_options_option_group_id");

            // Partial unique index – only one default per group
            builder.HasIndex(x => x.OptionGroupId)
                .IsUnique()
                .HasFilter("is_default = true")
                .HasDatabaseName("uq_product_options_one_default");

            builder.HasOne<ProductOptionGroup>()
                .WithMany()
                .HasForeignKey(x => x.OptionGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}