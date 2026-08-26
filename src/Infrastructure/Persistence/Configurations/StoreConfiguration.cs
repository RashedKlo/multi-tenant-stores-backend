using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class StoreConfiguration : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            builder.ToTable("stores", table =>
            {
                table.HasCheckConstraint(
                    "ck_stores_name_en_not_empty",
                    "length(btrim(name_en)) > 0");

                table.HasCheckConstraint(
                    "ck_stores_name_ar_not_empty",
                    "length(btrim(name_ar)) > 0");

                table.HasCheckConstraint(
                    "ck_stores_email_format",
                    "email IS NULL OR email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$'");

                table.HasCheckConstraint(
                    "ck_stores_latitude_range",
                    "latitude IS NULL OR (latitude BETWEEN -90 AND 90)");

                table.HasCheckConstraint(
                    "ck_stores_longitude_range",
                    "longitude IS NULL OR (longitude BETWEEN -180 AND 180)");

                table.HasCheckConstraint(
                    "ck_stores_rating_range",
                    "rating BETWEEN 0 AND 5");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.TenantId)
                .HasColumnName("tenant_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.ModuleId)
                .HasColumnName("module_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.NameEn)
                .HasColumnName("name_en")
                .HasColumnType("varchar(200)")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.NameAr)
                .HasColumnName("name_ar")
                .HasColumnType("varchar(200)")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.DescriptionEn)
                .HasColumnName("description_en")
                .HasColumnType("text")
                .IsRequired(false);

            builder.Property(x => x.DescriptionAr)
                .HasColumnName("description_ar")
                .HasColumnType("text")
                .IsRequired(false);

            builder.Property(x => x.LogoUrl)
                .HasColumnName("logo_url")
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.BannerUrl)
                .HasColumnName("banner_url")
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.Phone)
                .HasColumnName("phone")
                .HasColumnType("varchar(30)")
                .HasMaxLength(30)
                .IsRequired(false);

            builder.Property(x => x.Email)
                .HasColumnName("email")
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired(false);

            builder.Property(x => x.AddressEn)
                .HasColumnName("address_en")
                .HasColumnType("text")
                .IsRequired(false);

            builder.Property(x => x.AddressAr)
                .HasColumnName("address_ar")
                .HasColumnType("text")
                .IsRequired(false);

            builder.Property(x => x.Latitude)
                .HasColumnName("latitude")
                .HasColumnType("decimal(10,7)")
                .IsRequired(false);

            builder.Property(x => x.Longitude)
                .HasColumnName("longitude")
                .HasColumnType("decimal(10,7)")
                .IsRequired(false);

            builder.Property(x => x.Rating)
                .HasColumnName("rating")
                .HasColumnType("decimal(2,1)")
                .HasDefaultValue(0m)
                .IsRequired();

            builder.Property(x => x.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("jsonb")
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
            builder.HasIndex(x => x.TenantId)
                .HasDatabaseName("idx_stores_tenant_id");

            builder.HasIndex(x => x.ModuleId)
                .HasDatabaseName("idx_stores_module_id");

            // Relationships
            builder.HasOne(m=>m.Tenant)
                .WithMany(s=>s.Stores)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
  
            builder.HasOne(m=>m.Module)
                .WithMany(s=>s.Stores)
                .HasForeignKey(x => x.ModuleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}