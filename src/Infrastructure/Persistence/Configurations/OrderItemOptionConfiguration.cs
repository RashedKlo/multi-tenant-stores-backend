using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class OrderItemOptionConfiguration : IEntityTypeConfiguration<OrderItemOption>
    {
        public void Configure(EntityTypeBuilder<OrderItemOption> builder)
        {
            builder.ToTable("order_item_options");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.OrderItemId)
                .HasColumnName("order_item_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.OptionNameEnSnapshot)
                .HasColumnName("option_name_en_snapshot")
                .HasColumnType("varchar(150)")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.OptionNameArSnapshot)
                .HasColumnName("option_name_ar_snapshot")
                .HasColumnType("varchar(150)")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.PriceAdjustmentSnapshot)
                .HasColumnName("price_adjustment_snapshot")
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0m)
                .IsRequired();

            builder.HasIndex(x => x.OrderItemId)
                .HasDatabaseName("idx_order_item_options_order_item_id");

            builder.HasOne(x=>x.OrderItem)
                .WithMany(x=>x.OrderItemOptions)
                .HasForeignKey(x => x.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}