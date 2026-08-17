using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<HomeBanner> HomeBanners => Set<HomeBanner>();
    public DbSet<ModuleBanner> ModuleBanners => Set<ModuleBanner>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StoreCategory> StoreCategories => Set<StoreCategory>();
    public DbSet<StoreBanner> StoreBanners => Set<StoreBanner>();
    public DbSet<StoreSection> StoreSections => Set<StoreSection>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductOptionGroup> ProductOptionGroups => Set<ProductOptionGroup>();
    public DbSet<ProductOption> ProductOptions => Set<ProductOption>();
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<DiscountProduct> DiscountProducts => Set<DiscountProduct>();
    public DbSet<DiscountSection> DiscountSections => Set<DiscountSection>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
public DbSet<CartItemOption> CartItemOptions => Set<CartItemOption>();

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemOption> OrderItemOptions => Set<OrderItemOption>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<FavoriteProduct> FavoriteProducts => Set<FavoriteProduct>();
    public DbSet<FavoriteStore> FavoriteStores => Set<FavoriteStore>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
public DbSet<GuestSession> GuestSessions => Set<GuestSession>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
        
    }
}