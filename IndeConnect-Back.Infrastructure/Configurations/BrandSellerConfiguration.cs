using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class BrandSellerConfiguration : IEntityTypeConfiguration<BrandSeller>
{
    public void Configure(EntityTypeBuilder<BrandSeller> builder)
    {
        // Clé composite
        builder.HasKey(bs => new { bs.BrandId, bs.SellerId });

        // Relation with Brand
        builder.HasOne(bs => bs.Brand)
            .WithMany(b => b.Sellers)
            .HasForeignKey(bs => bs.BrandId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relation with User
        builder.HasOne(bs => bs.Seller)
            .WithMany(u => u.BrandsAsSeller)
            .HasForeignKey(bs => bs.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Properties
        builder.Property(bs => bs.JoinedAt)
            .IsRequired();

        builder.Property(bs => bs.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(bs => bs.SellerId);
        builder.HasIndex(bs => bs.IsActive);
    }
}