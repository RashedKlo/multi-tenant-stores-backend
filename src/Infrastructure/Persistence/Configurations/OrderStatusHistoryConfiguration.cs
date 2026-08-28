using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
        {
            builder.ToTable("order_status_history", table =>
            {
                table.HasCheckConstraint(
                    "ck_order_status_history_changed_by_type",
                    "changed_by_type IS NULL OR changed_by_type IN ('Customer','Tenant','System')");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()")
                .IsRequired();

            builder.Property(x => x.OrderId)
                .HasColumnName("order_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasColumnType("order_status")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.Note)
                .HasColumnName("note")
                .HasColumnType("varchar(500)")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.ChangedByType)
                .HasColumnName("changed_by_type")
                .HasColumnType("varchar(20)")
                .HasMaxLength(20)
                .HasConversion<string>()
                .IsRequired(false);

            builder.Property(x => x.ChangedById)
                .HasColumnName("changed_by_id")
                .HasColumnType("uuid")
                .IsRequired(false);

            builder.Property(x => x.ChangedAt)
                .HasColumnName("changed_at")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.HasIndex(x => new { x.OrderId, x.ChangedAt })
                .HasDatabaseName("idx_order_status_history_order_id");

            builder.HasOne(x=>x.Order)
                .WithMany(x=>x.OrderStatusHistories)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}