using Microsoft.EntityFrameworkCore;
using IndeConnect_Back.Domain.catalog.brand;
using IndeConnect_Back.Domain.catalog.product;
using IndeConnect_Back.Domain.order;
using IndeConnect_Back.Domain.payment;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Infrastructure;

public class AppDbContext : DbContext
{
    // USER DOMAIN
    public DbSet<User> Users { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }
    public DbSet<ShippingAddress> ShippingAddresses { get; set; }
    public DbSet<Delivery> Deliveries { get; set; }
    public DbSet<UserReview> UserReviews { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    // CATALOG - BRAND DOMAIN
    public DbSet<Brand> Brands { get; set; }
    public DbSet<BrandSeller> BrandSellers { get; set; } 
    public DbSet<BrandSubscription> BrandSubscriptions { get; set; }
    public DbSet<BrandEthicTag> BrandEthicTags { get; set; }
    public DbSet<BrandPolicy> BrandPolicies { get; set; }
    public DbSet<BrandQuestionnaire> BrandQuestionnaires { get; set; }
    public DbSet<BrandQuestionResponse> BrandQuestionResponses { get; set; }
    public DbSet<Deposit> Deposits { get; set; }
    public DbSet<EthicsQuestion> EthicsQuestions { get; set; } 
    public DbSet<EthicsOption> EthicsOptions { get; set; }
    
    // CATALOG - PRODUCT DOMAIN
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Size> Sizes { get; set; }
    public DbSet<Color> Colors { get; set; }
    public DbSet<Keyword> Keywords { get; set; }
    public DbSet<ProductKeyword> ProductKeywords { get; set; }
    public DbSet<ProductDetail> ProductDetails { get; set; }
    public DbSet<ProductMedia> ProductMedia { get; set; } 
    public DbSet<ProductReview> ProductReviews { get; set; }
    public DbSet<Sale> Sales { get; set; }
    
    // ORDER DOMAIN
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<ReturnRequest> ReturnRequests { get; set; }
    
    // PAYMENT DOMAIN
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentProvider> PaymentProviders { get; set; } 
    public DbSet<UserPaymentMethod> UserPaymentMethods { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
