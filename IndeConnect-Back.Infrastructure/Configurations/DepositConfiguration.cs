using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class DepositConfiguration : IEntityTypeConfiguration<Deposit>
{
    public void Configure(EntityTypeBuilder<Deposit> builder)
    {
        builder.HasKey(d => d.Id);
        
        // Properties
        builder.Property(d => d.Id)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(d => d.Number)
            .IsRequired();
        
        builder.Property(d => d.Street)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(d => d.PostalCode)
            .IsRequired()
            .HasMaxLength(20);
        builder.Property(d => d.City)
            .IsRequired();
        builder.Property(d => d.Country)
            .IsRequired();
        // Relation with Brand
        builder.HasOne(d => d.Brand)
            .WithMany(b => b.Deposits)
            .HasForeignKey(d => d.BrandId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.HasIndex(d => new { d.BrandId, d.Id })
            .IsUnique()
            .HasDatabaseName("IX_Deposit_UniqueBrandId");
        
        builder.HasIndex(d => d.BrandId)
            .HasDatabaseName("IX_Deposit_BrandId");
        
        builder.HasIndex(d => d.PostalCode)
            .HasDatabaseName("IX_Deposit_PostalCode");
    }
}