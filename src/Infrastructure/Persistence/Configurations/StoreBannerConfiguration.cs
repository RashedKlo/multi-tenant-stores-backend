using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class StoreBannerConfiguration : IEntityTypeConfiguration<StoreBanner>
    {
        public void Configure(EntityTypeBuilder<StoreBanner> builder)
        {
            builder.ToTable("store_banners", table =>
            {
                table.HasCheckConstraint(
                    "ck_store_banners_image_url_not_empty",
                    "length(btrim(image_url)) > 0");

                table.HasCheckConstraint(
                    "ck_store_banners_display_order_non_negative",
                    "display_order >= 0");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.StoreId)
                .HasColumnName("store_id")
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

            builder.HasIndex(x => x.StoreId)
                .HasDatabaseName("idx_store_banners_store_id");

            builder.HasOne(x=>x.Store)
                .WithMany(x=>x.StoreBanners)
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}