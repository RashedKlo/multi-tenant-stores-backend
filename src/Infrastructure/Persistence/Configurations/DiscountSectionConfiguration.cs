using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class DiscountSectionConfiguration : IEntityTypeConfiguration<DiscountSection>
    {
        public void Configure(EntityTypeBuilder<DiscountSection> builder)
        {
            builder.ToTable("discount_sections");

            builder.HasKey(x => new { x.DiscountId, x.SectionId });

            builder.Property(x => x.DiscountId)
                .HasColumnName("discount_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(x => x.SectionId)
                .HasColumnName("section_id")
                .HasColumnType("uuid")
                .IsRequired();

            builder.HasIndex(x => x.SectionId)
                .HasDatabaseName("idx_discount_sections_section_id");

            builder.HasOne<Discount>()
                .WithMany()
                .HasForeignKey(x => x.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<StoreSection>()
                .WithMany()
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}