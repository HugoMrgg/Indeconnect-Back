using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class ProductMediaConfiguration : IEntityTypeConfiguration<ProductMedia>
{
    public void Configure(EntityTypeBuilder<ProductMedia> builder)
    {
        // Primary Key
        builder.HasKey(pm => pm.Id);

        // Properties
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

        builder.Property(pm => pm.AltText)
            .HasMaxLength(200)
            .IsRequired(false);

        // Relation with Product
        builder.HasOne(pm => pm.Product)
            .WithMany(p => p.Media)
            .HasForeignKey(pm => pm.ProductId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasIndex(pm => new { pm.ProductId, pm.DisplayOrder })
            .HasDatabaseName("IX_ProductMedia_ProductOrder");

        builder.HasIndex(pm => pm.Type)
            .HasDatabaseName("IX_ProductMedia_Type");
    }
}