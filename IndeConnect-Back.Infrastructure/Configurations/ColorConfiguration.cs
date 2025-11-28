using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class ColorConfiguration : IEntityTypeConfiguration<Color>
{
    public void Configure(EntityTypeBuilder<Color> builder)
    {
        // Primary Key
        builder.HasKey(c => c.Id);
        
        // Properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50); 
        
        builder.Property(c => c.Hexa)
            .IsRequired()
            .HasMaxLength(7) 
            .IsFixedLength(false);
   
        builder.HasMany<Product>()
            .WithOne(p => p.PrimaryColor)
            .HasForeignKey(p => p.PrimaryColorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        
        // Indexes
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("IX_Color_UniqueName");
        
        builder.HasIndex(c => c.Hexa)
            .IsUnique()
            .HasDatabaseName("IX_Color_UniqueHexa");
        
        builder.ToTable("Colors");
    }
}