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
        
        // Relation with ProductVariant
        builder.HasMany<ProductVariant>()
               .WithOne(pv => pv.Size)
               .HasForeignKey(pv => pv.SizeId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false); 
        
        builder.HasIndex(s => s.Name)
               .IsUnique()
               .HasDatabaseName("IX_Size_UniqueName");
        
        // Seed Data 
        builder.HasData(
            new { Id = 1L, Name = "XS" },
            new { Id = 2L, Name = "S" },
            new { Id = 3L, Name = "M" },
            new { Id = 4L, Name = "L" },
            new { Id = 5L, Name = "XL" },
            new { Id = 6L, Name = "XXL" },
            new { Id = 7L, Name = "XXXL" },
            // Tailles numériques chaussures
            new { Id = 10L, Name = "36" },
            new { Id = 11L, Name = "37" },
            new { Id = 12L, Name = "38" },
            new { Id = 13L, Name = "39" },
            new { Id = 14L, Name = "40" },
            new { Id = 15L, Name = "41" },
            new { Id = 16L, Name = "42" },
            new { Id = 17L, Name = "43" },
            new { Id = 18L, Name = "44" },
            new { Id = 19L, Name = "45" },
            // Taille unique
            new { Id = 99L, Name = "Unique" }
        );
    }
}
