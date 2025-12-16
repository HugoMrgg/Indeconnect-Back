using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IndeConnect_Back.Domain.order;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class BrandDeliveryConfiguration : IEntityTypeConfiguration<BrandDelivery>
{
    public void Configure(EntityTypeBuilder<BrandDelivery> builder)
    {
        // Primary Key
        builder.HasKey(bd => bd.Id);

        // Properties
        builder.Property(bd => bd.Description)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(bd => bd.TrackingNumber)
               .HasMaxLength(100)
               .IsRequired(false);

        builder.Property(bd => bd.CreatedAt)
               .IsRequired();

        builder.Property(bd => bd.UpdatedAt)
               .IsRequired();

        builder.Property(bd => bd.ShippedAt)
               .IsRequired(false);

        builder.Property(bd => bd.DeliveredAt)
               .IsRequired(false);

        builder.Property(bd => bd.EstimatedDelivery)
               .IsRequired(false);

        builder.Property(bd => bd.Status)
               .HasConversion(new EnumToStringConverter<DeliveryStatus>())
               .IsRequired()
               .HasMaxLength(50)
               .HasDefaultValue(DeliveryStatus.Pending);

        builder.Property(bd => bd.ShippingFee)
               .HasColumnType("decimal(18,2)")
               .IsRequired()
               .HasDefaultValue(0m);

        // Relation with Brand
        builder.HasOne(bd => bd.Brand)
               .WithMany()
               .HasForeignKey(bd => bd.BrandId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

        // Relation with BrandShippingMethod
        builder.HasOne(bd => bd.ShippingMethod)
               .WithMany()
               .HasForeignKey(bd => bd.ShippingMethodId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

        // Relation with Order
        builder.HasOne(bd => bd.Order)
               .WithMany(o => o.BrandDeliveries)
               .HasForeignKey(bd => bd.OrderId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

        // Relation with OrderItems
        builder.HasMany(bd => bd.Items)
               .WithOne(oi => oi.BrandDelivery)
               .HasForeignKey(oi => oi.BrandDeliveryId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);

        // Indexes
        builder.HasIndex(bd => bd.TrackingNumber)
               .IsUnique()
               .HasFilter("\"TrackingNumber\" IS NOT NULL")
               .HasDatabaseName("IX_BrandDelivery_UniqueTrackingNumber");

        builder.HasIndex(bd => bd.Status)
               .HasDatabaseName("IX_BrandDelivery_Status");

        builder.HasIndex(bd => new { bd.OrderId, bd.BrandId })
               .IsUnique()
               .HasDatabaseName("IX_BrandDelivery_OrderBrand");

        builder.HasIndex(bd => new { bd.OrderId, bd.Status })
               .HasDatabaseName("IX_BrandDelivery_OrderStatus");

        builder.HasIndex(bd => bd.ShippedAt)
               .HasDatabaseName("IX_BrandDelivery_ShippedAt");

        builder.HasIndex(bd => bd.DeliveredAt)
               .HasDatabaseName("IX_BrandDelivery_DeliveredAt");
    }
}
