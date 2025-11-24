using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class ProductGroupConfiguration : IEntityTypeConfiguration<ProductGroup>
{
    public void Configure(EntityTypeBuilder<ProductGroup> builder)
    {
        builder.HasKey(pg => pg.Id);
        
        builder.Property(pg => pg.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(pg => pg.BaseDescription)
            .IsRequired()
            .HasMaxLength(2000);
        
        // Relation avec Brand
        builder.HasOne(pg => pg.Brand)
            .WithMany()
            .HasForeignKey(pg => pg.BrandId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        // Relation avec Category
        builder.HasOne(pg => pg.Category)
            .WithMany()
            .HasForeignKey(pg => pg.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        // Relation avec Products
        builder.HasMany(pg => pg.Products)
            .WithOne(p => p.ProductGroup)
            .HasForeignKey(p => p.ProductGroupId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        
        builder.HasIndex(pg => pg.BrandId)
            .HasDatabaseName("IX_ProductGroup_BrandId");
        
        builder.HasIndex(pg => pg.CategoryId)
            .HasDatabaseName("IX_ProductGroup_CategoryId");
        
        builder.HasIndex(pg => pg.Name)
            .HasDatabaseName("IX_ProductGroup_Name");
        
        builder.ToTable("ProductGroups");
    }
}