using IndeConnect_Back.Domain.catalog.product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class ProductVariantMediaConfiguration : IEntityTypeConfiguration<ProductVariantMedia>
{
    public void Configure(EntityTypeBuilder<ProductVariantMedia> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Type)
            .IsRequired();

        builder.Property(m => m.DisplayOrder)
            .IsRequired();

        builder.Property(m => m.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(m => m.Variant)
            .WithMany(v => v.Media)
            .HasForeignKey(m => m.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.VariantId);
    }
}