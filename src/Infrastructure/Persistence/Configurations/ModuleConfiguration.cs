using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.ToTable("modules", table =>
            {
                table.HasCheckConstraint("ck_modules_name_en_not_empty", "length(btrim(name_en)) > 0");
                table.HasCheckConstraint("ck_modules_name_ar_not_empty", "length(btrim(name_ar)) > 0");
                table.HasCheckConstraint("ck_modules_display_order_non_negative", "display_order >= 0");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.NameEn)
                .HasColumnName("name_en")
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(x => x.NameEn)
                .IsUnique();

            builder.Property(x => x.NameAr)
                .HasColumnName("name_ar")
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(x => x.NameAr)
                .IsUnique();

            builder.Property(x => x.IconUrl)
                .HasColumnName("icon_url")
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
        }
    }
}