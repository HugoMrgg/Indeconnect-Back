using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class SizeConfiguration : IEntityTypeConfiguration<Size>
{
    public void Configure(EntityTypeBuilder<Size> builder)
    {
        // Primary Key
        builder.HasKey(s => s.Id);
        
        // Properties
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(20);
               
        builder.Property(s => s.SortOrder)
            .IsRequired();
               
        builder.Property(s => s.CategoryId)
            .IsRequired();
        
        // Relation avec Category
        builder.HasOne(s => s.Category)
            .WithMany(c => c.Sizes)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relation avec ProductVariant
        builder.HasMany<ProductVariant>()
            .WithOne(pv => pv.Size)
            .HasForeignKey(pv => pv.SizeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        
        // Index unique sur Name + CategoryId (deux catégories peuvent avoir un "M" différent)
        builder.HasIndex(s => new { s.Name, s.CategoryId })
            .IsUnique()
            .HasDatabaseName("IX_Size_Name_Category");
               
        // Index pour améliorer les queries par CategoryId
        builder.HasIndex(s => s.CategoryId)
            .HasDatabaseName("IX_Size_CategoryId");
    }
}