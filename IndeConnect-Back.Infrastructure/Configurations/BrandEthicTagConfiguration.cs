using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class BrandEthicTagConfiguration : IEntityTypeConfiguration<BrandEthicTag>
{
    public void Configure(EntityTypeBuilder<BrandEthicTag> builder)
    {
        // Primary Key
        builder.HasKey(bet => bet.Id);
        
        // Properties
        builder.Property(bet => bet.Category)
               .HasConversion(new EnumToStringConverter<EthicsCategory>())
               .IsRequired()
               .HasMaxLength(50); // Limite pour le string converti
        
        builder.Property(bet => bet.TagKey)
               .IsRequired()
               .HasMaxLength(100); // Limite raisonnable pour un tag
        
        // Relation with Brand
        // This configuration assure the two-way coherence.
        builder.HasOne(bet => bet.Brand)
               .WithMany(b => b.EthicTags)
               .HasForeignKey(bet => bet.BrandId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
        
        // Enabling the fact that one brand could have two tags in the same category
        builder.HasIndex(bet => new { bet.BrandId, bet.Category, bet.TagKey })
               .IsUnique()
               .HasDatabaseName("IX_BrandEthicTag_Unique");
        
        // Category's filter index
        builder.HasIndex(bet => bet.Category)
               .HasDatabaseName("IX_BrandEthicTag_Category");
        
        // Tag's filter index
        builder.HasIndex(bet => bet.TagKey)
               .HasDatabaseName("IX_BrandEthicTag_TagKey");
    }
}
