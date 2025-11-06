using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IndeConnect_Back.Domain.order;
using IndeConnect_Back.Domain.payment;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Primary Key
        builder.HasKey(o => o.Id);
        
        // Properties
        builder.Property(o => o.PlacedAt)
               .IsRequired();
        
        builder.Property(o => o.Currency)
               .IsRequired()
               .HasMaxLength(3) 
               .HasDefaultValue("EUR");
        
        builder.Property(o => o.TotalAmount)
               .IsRequired()
               .HasPrecision(18, 2);
        
        builder.Property(o => o.Status)
               .HasConversion(new EnumToStringConverter<OrderStatus>())
               .IsRequired()
               .HasMaxLength(20)
               .HasDefaultValue(OrderStatus.Pending);
        
        // Relation with User
        builder.HasOne(o => o.User)
               .WithMany(u => u.Orders)
               .HasForeignKey(o => o.UserId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        
        // Relation with ShippingAddress
        builder.HasOne(o => o.ShippingAddress)
               .WithMany()
               .HasForeignKey(o => o.ShippingAddressId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        
        // Relation with Payment 
        builder.HasOne(o => o.Payment)
               .WithOne(p => p.Order)
               .HasForeignKey<Payment>(p => p.OrderId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);
        
        // Relation with OrderItems (One-to-Many)
        builder.HasMany(o => o.Items)
               .WithOne(oi => oi.Order)
               .HasForeignKey(oi => oi.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
        
        // Relation with Invoices (One-to-Many)
        builder.HasMany(o => o.Invoices)
               .WithOne(i => i.Order)
               .HasForeignKey(i => i.OrderId)
               .OnDelete(DeleteBehavior.Restrict);
        
        // Relation with Deliveries (One-to-Many)
        builder.HasMany(o => o.Deliveries)
               .WithOne(d => d.Order)
               .HasForeignKey(d => d.OrderId)
               .OnDelete(DeleteBehavior.Restrict);
        
        // Relation with ReturnRequests (One-to-Many)
        builder.HasMany(o => o.Returns)
               .WithOne(rr => rr.Order)
               .HasForeignKey(rr => rr.OrderId)
               .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(o => o.UserId)
               .HasDatabaseName("IX_Order_UserId");
        
        builder.HasIndex(o => o.Status)
               .HasDatabaseName("IX_Order_Status");
        
        builder.HasIndex(o => new { o.UserId, o.Status })
               .HasDatabaseName("IX_Order_UserStatus");
        
        builder.HasIndex(o => o.PlacedAt)
               .HasDatabaseName("IX_Order_PlacedAt");
        
        builder.HasIndex(o => new { o.PlacedAt, o.Status })
               .HasDatabaseName("IX_Order_PlacedStatus");
    }
}
