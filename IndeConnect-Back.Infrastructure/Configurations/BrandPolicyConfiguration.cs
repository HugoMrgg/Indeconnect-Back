using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class BrandPolicyConfiguration : IEntityTypeConfiguration<BrandPolicy>
{
    public void Configure(EntityTypeBuilder<BrandPolicy> builder)
    {
        // Primary Key
        builder.HasKey(bp => bp.Id);
        
        // Properties
        builder.Property(bp => bp.Type)
               .HasConversion(new EnumToStringConverter<PolicyType>())
               .IsRequired()
               .HasMaxLength(50); 
        
        builder.Property(bp => bp.Content)
               .IsRequired()
               .HasMaxLength(10000); 
        
        builder.Property(bp => bp.Language)
               .HasMaxLength(10) 
               .IsRequired(false); 
        
        builder.Property(bp => bp.PublishedAt)
               .IsRequired();
        
        builder.Property(bp => bp.IsActive)
               .IsRequired()
               .HasDefaultValue(true);
        
        // Relation with Brand 
        builder.HasOne(bp => bp.Brand)
               .WithMany(b => b.Policies)
               .HasForeignKey(bp => bp.BrandId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
        
        // Enable the fact that a brand could have two policy about the same subject
        // Like more than one policy for the return policy
        builder.HasIndex(bp => new { bp.BrandId, bp.Type, bp.IsActive })
               .HasFilter("[IsActive] = 1") 
               .IsUnique()
               .HasDatabaseName("IX_BrandPolicy_UniqueActivePerType");
        
        // Type's filter Index
        builder.HasIndex(bp => bp.Type)
               .HasDatabaseName("IX_BrandPolicy_Type");
        
        // Active's filter Index
        builder.HasIndex(bp => new { bp.BrandId, bp.IsActive })
               .HasDatabaseName("IX_BrandPolicy_BrandActive");
        
        // Language's filter Index
        builder.HasIndex(bp => bp.Language)
               .HasDatabaseName("IX_BrandPolicy_Language");
    }
}
