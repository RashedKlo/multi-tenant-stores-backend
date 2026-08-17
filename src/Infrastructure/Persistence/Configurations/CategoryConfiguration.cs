using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("categories", table =>
            {
                table.HasCheckConstraint(
                    "ck_categories_name_en_not_empty",
                    "length(btrim(name_en)) > 0");

                table.HasCheckConstraint(
                    "ck_categories_name_ar_not_empty",
                    "length(btrim(name_ar)) > 0");

                table.HasCheckConstraint(
                    "ck_categories_display_order_non_negative",
                    "display_order >= 0");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.ModuleId)
                .HasColumnName("module_id")
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

            builder.Property(x => x.ImageUrl)
                .HasColumnName("image_url")
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsRequired(false);

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

            // Unique constraints
            builder.HasIndex(x => new { x.ModuleId, x.NameEn })
                .IsUnique()
                .HasDatabaseName("uq_categories_module_id_name_en");

            builder.HasIndex(x => new { x.ModuleId, x.NameAr })
                .IsUnique()
                .HasDatabaseName("uq_categories_module_id_name_ar");

            builder.HasIndex(x => x.ModuleId)
                .HasDatabaseName("idx_categories_module_id");

            builder.HasOne<Module>()
                .WithMany()
                .HasForeignKey(x => x.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}