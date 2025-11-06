using IndeConnect_Back.Domain;
using Microsoft.EntityFrameworkCore;
using IndeConnect_Back.Domain.catalog.brand;
using IndeConnect_Back.Domain.catalog.product;
using IndeConnect_Back.Domain.order;
using IndeConnect_Back.Domain.payment;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Infrastructure;

public class AppDbContext : DbContext
{
    // User Domain
    public DbSet<BrandSubscription> BrandSubscriptions { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Delivery> Deliveries { get; set; }
    // DeliveryStatus is an enum in the Domain layer and must NOT be mapped as an entity/DbSet.
    // Enums are stored as strings on their owning entities via ValueConverters.
    public DbSet<Role> Roles { get; set; }
    public DbSet<ShippingAddress> ShippingAddresses { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }

    // Catalog - Brand Domain
    public DbSet<Brand> Brands { get; set; }
    public DbSet<BrandEthicTag> BrandEthicTags { get; set; }
    public DbSet<BrandPolicy> BrandPolicies { get; set; }
    public DbSet<BrandQuestionnaire> BrandQuestionnaires { get; set; }
    public DbSet<BrandQuestionResponse> BrandQuestionResponses { get; set; }
    public DbSet<BrandStatistics> BrandStatistics { get; set; }
    public DbSet<Deposit> Depots { get; set; }
    public DbSet<EthicsQuestion> EthicsCriteria { get; set; }
    public DbSet<EthicsOption> EthicsOptions { get; set; }

    // Catalog - Product Domain
    public DbSet<Category> Categories { get; set; }
    public DbSet<Color> Colors { get; set; }
    public DbSet<Detail> Details { get; set; }
    public DbSet<Keyword> Keywords { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductColor> ProductColors { get; set; }
    public DbSet<ProductDetail> ProductDetails { get; set; }
    public DbSet<ProductKeyword> ProductKeywords { get; set; }
    public DbSet<ProductMedia> ProductMedias { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }
    public DbSet<ProductSize> ProductSizes { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<Size> Sizes { get; set; }

    // Order Domain
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<ReturnRequest> ReturnRequests { get; set; }

    // Payment Domain
    public DbSet<Payment> Payments { get; set; }
    public DbSet<UserPaymentMethod> UserPaymentMethods { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) {}
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Applique toutes tes classes de configuration
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
