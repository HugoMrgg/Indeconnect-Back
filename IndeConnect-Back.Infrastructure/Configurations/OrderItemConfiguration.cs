using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.order;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        // Primary Key
        builder.HasKey(oi => oi.Id);
        
        // Properties
        builder.Property(oi => oi.ProductName)
               .IsRequired()
               .HasMaxLength(200); 
        
        builder.Property(oi => oi.Quantity)
               .IsRequired();
        
        builder.Property(oi => oi.UnitPrice)
               .IsRequired()
               .HasPrecision(18, 2);
        
        // Relation with Order
        builder.HasOne(oi => oi.Order)
               .WithMany(o => o.Items)
               .HasForeignKey(oi => oi.OrderId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
        
        // Relation with Product 
        builder.HasOne(oi => oi.Product)
               .WithMany()
               .HasForeignKey(oi => oi.ProductId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        
        // Relation with ProductVariant
        builder.HasOne(oi => oi.Variant)
               .WithMany()
               .HasForeignKey(oi => oi.VariantId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);
        
        builder.HasIndex(oi => oi.OrderId)
               .HasDatabaseName("IX_OrderItem_OrderId");
        
        builder.HasIndex(oi => oi.ProductId)
               .HasDatabaseName("IX_OrderItem_ProductId");
        
        builder.HasIndex(oi => oi.VariantId)
               .HasDatabaseName("IX_OrderItem_VariantId");
    }
}
