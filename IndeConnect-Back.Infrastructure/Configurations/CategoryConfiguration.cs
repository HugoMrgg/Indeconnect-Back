using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Primary Key
        builder.HasKey(c => c.Id);
        
        // Properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        // Relation avec Product
        builder.HasMany<Product>()
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relation avec Size (déjà configurée dans SizeConfiguration, mais on l'explicite ici aussi)
        builder.HasMany(c => c.Sizes)
            .WithOne(s => s.Category)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("IX_Category_UniqueName");
    }
}