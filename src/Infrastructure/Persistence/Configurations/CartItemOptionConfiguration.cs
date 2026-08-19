using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CartItemOptionConfiguration : IEntityTypeConfiguration<CartItemOption>
    {
        public void Configure(EntityTypeBuilder<CartItemOption> builder)
        {
            builder.ToTable("cart_item_options");

            builder.HasKey(x => new { x.CartItemId, x.OptionId });

            builder.Property(x => x.CartItemId).HasColumnName("cart_item_id").HasColumnType("uuid").IsRequired();
            builder.Property(x => x.OptionId).HasColumnName("option_id").HasColumnType("uuid").IsRequired();

            builder.HasIndex(x => x.OptionId).HasDatabaseName("idx_cart_item_options_option_id");

            builder.HasOne(x => x.CartItem).WithMany().HasForeignKey(x => x.CartItemId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<ProductOption>().WithMany().HasForeignKey(x => x.OptionId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}