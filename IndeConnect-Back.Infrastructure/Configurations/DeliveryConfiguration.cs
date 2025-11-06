using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IndeConnect_Back.Domain.order;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        // Primary Key
        builder.HasKey(d => d.Id);
        
        // Properties
        builder.Property(d => d.Description)
               .IsRequired()
               .HasMaxLength(500);
        
        builder.Property(d => d.TrackingNumber)
               .HasMaxLength(100)
               .IsRequired(false);
        
        builder.Property(d => d.ShippedAt)
               .IsRequired(false);
        
        builder.Property(d => d.Status)
               .HasConversion(new EnumToStringConverter<DeliveryStatus>())
               .IsRequired()
               .HasMaxLength(50)
               .HasDefaultValue(DeliveryStatus.Pending);
        
        // Relation with Order
        builder.HasOne(d => d.Order)
               .WithMany(o => o.Deliveries)
               .HasForeignKey(d => d.OrderId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        
        builder.HasIndex(d => d.TrackingNumber)
               .IsUnique()
               .HasFilter("[TrackingNumber] IS NOT NULL")
               .HasDatabaseName("IX_Delivery_UniqueTrackingNumber");
        
        builder.HasIndex(d => d.Status)
               .HasDatabaseName("IX_Delivery_Status");
        
        builder.HasIndex(d => new { d.OrderId, d.Status })
               .HasDatabaseName("IX_Delivery_OrderStatus");
        
        builder.HasIndex(d => d.ShippedAt)
               .HasDatabaseName("IX_Delivery_ShippedAt");
    }
}
