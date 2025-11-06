using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        // Primary Key
        builder.HasKey(s => s.Id);
        
        // Properties
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100); 
        
        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(s => s.DiscountPercentage)
            .IsRequired()
            .HasPrecision(5, 2);
        
        builder.Property(s => s.StartDate)
            .IsRequired();
        
        builder.Property(s => s.EndDate)
            .IsRequired();
        
        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        // Relation with Product
        builder.HasMany(s => s.Products)
            .WithOne(p => p.Sale)
            .HasForeignKey(p => p.SaleId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
        
        builder.HasIndex(s => s.IsActive)
            .HasDatabaseName("IX_Sale_IsActive");
        
        builder.HasIndex(s => new { s.IsActive, s.StartDate, s.EndDate })
            .HasDatabaseName("IX_Sale_ActiveDates");
        
        builder.HasIndex(s => s.StartDate)
            .HasDatabaseName("IX_Sale_StartDate");
    }
}