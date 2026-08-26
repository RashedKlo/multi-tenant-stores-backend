using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
    {
        public void Configure(EntityTypeBuilder<Discount> builder)
        {
            builder.ToTable("discounts", table =>
            {
                table.HasCheckConstraint(
                    "ck_discounts_title_en_not_empty",
                    "length(btrim(title_en)) > 0");

                table.HasCheckConstraint(
                    "ck_discounts_title_ar_not_empty",
                    "length(btrim(title_ar)) > 0");

                table.HasCheckConstraint(
                    "ck_discounts_value_positive",
                    "value > 0");

                table.HasCheckConstraint(
                    "ck_discounts_date_range",
                    "start_date IS NULL OR end_date IS NULL OR start_date < end_date");

                table.HasCheckConstraint(
                    "ck_discounts_percentage_max",
                    "type <> 'Percentage' OR value <= 100");
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

            builder.Property(x => x.TitleEn)
                .HasColumnName("title_en")
                .HasColumnType("varchar(150)")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.TitleAr)
                .HasColumnName("title_ar")
                .HasColumnType("varchar(150)")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Type)
                .HasColumnName("type")
                .HasColumnType("discount_type")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.Value)
                .HasColumnName("value")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            builder.Property(x => x.StartDate)
                .HasColumnName("start_date")
                .HasColumnType("timestamptz")
                .IsRequired(false);

            builder.Property(x => x.EndDate)
                .HasColumnName("end_date")
                .HasColumnType("timestamptz")
                .IsRequired(false);

            builder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .HasColumnType("boolean")
                .HasDefaultValue(true)
                .IsRequired();

            builder.HasIndex(x => x.StoreId)
                .HasDatabaseName("idx_discounts_store_id");

            builder.HasOne(x=>x.Store)
                .WithMany(x=>x.Discounts)
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}