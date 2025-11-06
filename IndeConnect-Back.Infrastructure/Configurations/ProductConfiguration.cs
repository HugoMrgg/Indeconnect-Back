using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IndeConnect_Back.Domain.catalog.product;
using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Primary Key
        builder.HasKey(p => p.Id);
        
        // Properties - General
        builder.Property(p => p.Name)
               .IsRequired()
               .HasMaxLength(200);
        
        builder.Property(p => p.Description)
               .IsRequired()
               .HasMaxLength(2000);
        
        builder.Property(p => p.Price)
               .IsRequired()
               .HasPrecision(18, 2);
        
        builder.Property(p => p.IsEnabled)
               .IsRequired()
               .HasDefaultValue(true);
        
        builder.Property(p => p.CreatedAt)
               .IsRequired();
        
        builder.Property(p => p.UpdatedAt)
               .IsRequired(false);
        
        builder.Property(p => p.Status)
               .HasConversion(new EnumToStringConverter<ProductStatus>())
               .IsRequired()
               .HasMaxLength(50)
               .HasDefaultValue(ProductStatus.Draft);
        
        // Relation with Brand
        builder.HasOne(p => p.Brand)
               .WithMany() 
               .HasForeignKey(p => p.BrandId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        
        // Relation with Category
        builder.HasOne(p => p.Category)
               .WithMany() 
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        
        // Relation with Sale
        builder.HasOne(p => p.Sale)
               .WithMany()
               .HasForeignKey(p => p.SaleId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);
        
        // Relation with ProductVariant
        builder.HasMany(p => p.Variants)
               .WithOne(pv => pv.Product)
               .HasForeignKey(pv => pv.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
        
        // Relation with ProductMedia
        builder.HasMany(p => p.Media)
               .WithOne(pm => pm.Product)
               .HasForeignKey(pm => pm.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
        
        // Relation with ProductKeyword
        builder.HasMany(p => p.Keywords)
               .WithOne(pk => pk.Product)
               .HasForeignKey(pk => pk.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
        
        // Relation with ProductDetail
        builder.HasMany(p => p.Details)
               .WithOne(pd => pd.Product)
               .HasForeignKey(pd => pd.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
        
        // Relation with ProductReview
        builder.HasMany(p => p.Reviews)
               .WithOne(pr => pr.Product)
               .HasForeignKey(pr => pr.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(p => p.BrandId)
               .HasDatabaseName("IX_Product_BrandId");
        
        builder.HasIndex(p => p.CategoryId)
               .HasDatabaseName("IX_Product_CategoryId");
        
        builder.HasIndex(p => p.Status)
               .HasDatabaseName("IX_Product_Status");
        
        builder.HasIndex(p => new { p.BrandId, p.IsEnabled, p.Status })
               .HasDatabaseName("IX_Product_BrandActiveStatus");
        
        builder.HasIndex(p => p.Price)
               .HasDatabaseName("IX_Product_Price");
        
        builder.HasIndex(p => p.CreatedAt)
               .HasDatabaseName("IX_Product_CreatedAt");
        
        builder.HasIndex(p => p.Name)
               .HasDatabaseName("IX_Product_Name");
    }
}
