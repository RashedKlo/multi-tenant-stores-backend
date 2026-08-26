using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ModuleBannerConfiguration : IEntityTypeConfiguration<ModuleBanner>
    {
        public void Configure(EntityTypeBuilder<ModuleBanner> builder)
        {
            builder.ToTable("module_banners", table =>
            {
                table.HasCheckConstraint(
                    "ck_module_banners_image_url_not_empty",
                    "length(btrim(image_url)) > 0");

                table.HasCheckConstraint(
                    "ck_module_banners_display_order_non_negative",
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

            builder.Property(x => x.ImageUrl)
                .HasColumnName("image_url")
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.TitleEn)
                .HasColumnName("title_en")
                .HasColumnType("varchar(200)")
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(x => x.TitleAr)
                .HasColumnName("title_ar")
                .HasColumnType("varchar(200)")
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(x => x.ActionUrl)
                .HasColumnName("action_url")
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

            builder.HasIndex(x => x.ModuleId)
                .HasDatabaseName("idx_module_banners_module_id");

           builder.HasOne(b => b.Module)
    .WithMany(m => m.ModuleBanners)
    .HasForeignKey(b => b.ModuleId)
    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}