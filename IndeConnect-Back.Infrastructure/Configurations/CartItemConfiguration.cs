using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.user;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        // Composite key : CartId + ProductId
        builder.HasKey(ci => new { ci.CartId, ci.ProductId });
        
        // Properties
        builder.Property(ci => ci.Quantity)
            .IsRequired()
            .HasDefaultValue(1);
        
        builder.Property(ci => ci.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(ci => ci.AddedAt)
            .IsRequired();
        
        // Relation with Cart
        builder.HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        // Relation with Product
        builder.HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        builder.HasIndex(ci => ci.ProductId)
            .HasDatabaseName("IX_CartItem_ProductId");
        
        builder.HasIndex(ci => ci.AddedAt)
            .HasDatabaseName("IX_CartItem_AddedAt");
    }
}