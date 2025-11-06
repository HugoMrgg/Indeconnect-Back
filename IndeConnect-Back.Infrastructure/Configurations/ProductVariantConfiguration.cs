using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(pv => pv.Id);
        
        builder.Property(pv => pv.SKU)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(pv => pv.StockCount)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(pv => pv.PriceOverride)
            .HasPrecision(18, 2)
            .IsRequired(false);
        
        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.HasOne(pv => pv.Size)
            .WithMany()
            .HasForeignKey(pv => pv.SizeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        
        builder.HasOne(pv => pv.Color)
            .WithMany()
            .HasForeignKey(pv => pv.ColorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        
        builder.HasIndex(pv => pv.SKU)
            .IsUnique()
            .HasDatabaseName("IX_ProductVariant_UniqueSKU");
        
        builder.HasIndex(pv => pv.ProductId)
            .HasDatabaseName("IX_ProductVariant_ProductId");
        
        builder.ToTable("ProductVariants");
    }
}