using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class BrandShippingMethodConfiguration : IEntityTypeConfiguration<BrandShippingMethod>
{
    public void Configure(EntityTypeBuilder<BrandShippingMethod> builder)
    {
        builder.ToTable("BrandShippingMethods");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProviderName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.Property(x => x.MaxWeight)
            .HasPrecision(10, 2);

        builder.Property(x => x.ProviderConfig)
            .HasMaxLength(2000);

        builder.Property(x => x.MethodType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.IsEnabled)
            .HasDefaultValue(true);

        // Index
        builder.HasIndex(x => x.BrandId);
        builder.HasIndex(x => new { x.BrandId, x.IsEnabled });

        // Relation avec Brand
        builder.HasOne(x => x.Brand)
            .WithMany(b => b.ShippingMethods)
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}