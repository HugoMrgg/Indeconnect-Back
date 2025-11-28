using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class ProductMediaConfiguration : IEntityTypeConfiguration<ProductMedia>
{
    public void Configure(EntityTypeBuilder<ProductMedia> builder)
    {
        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pm => pm.Type)
            .HasConversion(new EnumToStringConverter<MediaType>())
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue(MediaType.Image);

        builder.Property(pm => pm.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(pm => pm.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(pm => pm.Product)
            .WithMany(p => p.Media)
            .HasForeignKey(pm => pm.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pm => pm.ProductId)
            .HasDatabaseName("IX_ProductMedia_ProductId");
        
        builder.HasIndex(pm => new { pm.ProductId, pm.IsPrimary })
            .HasDatabaseName("IX_ProductMedia_ProductPrimary");
        
        builder.HasIndex(pm => new { pm.ProductId, pm.DisplayOrder })
            .HasDatabaseName("IX_ProductMedia_ProductOrder");

        builder.ToTable("ProductMedia");
    }
}