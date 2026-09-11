using Domain.Aggregates.Cart;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Infrastructure.Persistence.Configurations;
public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items", t =>
        {
            t.HasCheckConstraint("ck_cart_items_quantity_positive", "quantity > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid")
            .ValueGeneratedNever();
        builder.Property(x => x.CartId).HasColumnName("cart_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.ProductId).HasColumnName("product_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("int").IsRequired();
        builder.Property(x => x.Notes).HasColumnName("notes").HasColumnType("varchar(500)").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz")
            .HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz")
            .HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(x => x.CartId).HasDatabaseName("idx_cart_items_cart_id");
        builder.HasIndex(x => x.ProductId).HasDatabaseName("idx_cart_items_product_id");

builder.HasMany(i => i.Options)
    .WithOne()
    .HasForeignKey(o => o.CartItemId)
    .OnDelete(DeleteBehavior.Cascade);

builder.Navigation(i => i.Options)
    .HasField("_options")
    .UsePropertyAccessMode(PropertyAccessMode.Field);

      
    }
}