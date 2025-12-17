using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class EthicsCategoryConfiguration : IEntityTypeConfiguration<EthicsCategoryEntity>
{
    public void Configure(EntityTypeBuilder<EthicsCategoryEntity> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Key).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Label).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Order).IsRequired().HasDefaultValue(0);
        builder.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasIndex(c => c.Key).IsUnique();
        builder.HasIndex(c => new { c.IsActive, c.Order });
    }
}