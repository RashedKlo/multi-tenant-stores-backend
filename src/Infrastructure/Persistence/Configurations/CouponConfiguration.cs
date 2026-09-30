using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("coupons");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid");
        builder.Property(x => x.StoreId).HasColumnName("store_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.Code).HasColumnName("code").HasColumnType("varchar(50)").HasMaxLength(50).IsRequired();
        builder.Property(x => x.DiscountType).HasColumnName("discount_type").HasColumnType("smallint").HasConversion<short>().IsRequired();
        builder.Property(x => x.DiscountValue).HasColumnName("discount_value").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(x => x.MaxDiscountAmount).HasColumnName("max_discount_amount").HasColumnType("numeric(12,2)");
        builder.Property(x => x.MinOrderAmount).HasColumnName("min_order_amount").HasColumnType("numeric(12,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(x => x.StartsAt).HasColumnName("starts_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at").HasColumnType("timestamptz");
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasColumnType("boolean").HasDefaultValue(true).IsRequired();
        builder.Property(x => x.UsageLimitTotal).HasColumnName("usage_limit_total").HasColumnType("integer");
        builder.Property(x => x.UsedCount).HasColumnName("used_count").HasColumnType("integer").HasDefaultValue(0).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz").HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(x => new { x.StoreId, x.Code }).IsUnique().HasDatabaseName("uq_coupons_store_code");
        builder.HasIndex(x => x.StoreId).HasDatabaseName("idx_coupons_store_id");
    }
}