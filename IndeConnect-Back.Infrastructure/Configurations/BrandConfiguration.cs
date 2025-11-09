using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        // Primary Key
        builder.HasKey(b => b.Id);
        
        // Properties
        builder.Property(b => b.Name)
               .IsRequired()
               .HasMaxLength(200);
        
        builder.Property(b => b.Status)
               .HasConversion(new EnumToStringConverter<BrandStatus>())
               .IsRequired();

        builder.Property(b => b.Description)
               .HasMaxLength(2000);

        builder.Property(b => b.AboutUs)
               .HasMaxLength(2000);

        builder.Property(b => b.WhereAreWe)
               .HasMaxLength(1000);

        builder.Property(b => b.OtherInfo)
               .HasMaxLength(1000);

        builder.Property(b => b.Contact)
               .HasMaxLength(500);

        // Relation with SuperVendor (User)
        builder.HasOne(b => b.SuperVendorUser)
               .WithMany()
               .HasForeignKey(b => b.SuperVendorUserId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

        // Relation Many-to-Many with Sellers (User)
        builder.HasMany(b => b.Sellers)
               .WithOne(s => s.Brand)
               .HasForeignKey(bs => bs.BrandId)
               .OnDelete(DeleteBehavior.Cascade);

        // Relation with Deposits
        builder.HasMany(b => b.Deposits)
               .WithOne(d => d.Brand)
               .HasForeignKey(d => d.BrandId)
               .OnDelete(DeleteBehavior.Cascade);

        // Relation with Policies
        builder.HasMany(b => b.Policies)
               .WithOne(bp => bp.Brand)
               .HasForeignKey(bp => bp.BrandId)
               .OnDelete(DeleteBehavior.Cascade);

        // Relation with EthicTags
        builder.HasMany(b => b.EthicTags)
               .WithOne(et => et.Brand)
               .HasForeignKey(et => et.BrandId)
               .OnDelete(DeleteBehavior.Cascade);

        // Relation with Questionnaires
        builder.HasMany(b => b.Questionnaires)
               .WithOne(q => q.Brand)
               .HasForeignKey(q => q.BrandId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
