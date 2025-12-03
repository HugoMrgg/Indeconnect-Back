using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class ProductDetailConfiguration : IEntityTypeConfiguration<ProductDetail>
{
    public void Configure(EntityTypeBuilder<ProductDetail> builder)
    {
        // Primary Key
        builder.HasKey(pd => pd.Id);
        
        builder.Property(pd => pd.Value)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(pd => pd.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);
        
        // Relation with Product
        builder.HasOne(pd => pd.Product)
            .WithMany(p => p.Details)
            .HasForeignKey(pd => pd.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.HasIndex(pd => new { pd.ProductId, pd.DisplayOrder })
            .HasDatabaseName("IX_ProductDetail_ProductOrder");
    }
}