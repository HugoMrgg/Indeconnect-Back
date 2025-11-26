using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        // MODIFIÉ : Composite key CartId + ProductVariantId (au lieu de ProductId)
        builder.HasKey(ci => new { ci.CartId, ci.ProductVariantId });
        
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
        
        // NOUVEAU : Relation with ProductVariant
        builder.HasOne(ci => ci.ProductVariant)
            .WithMany()
            .HasForeignKey(ci => ci.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        builder.HasIndex(ci => ci.ProductId)
            .HasDatabaseName("IX_CartItem_ProductId");
        
        builder.HasIndex(ci => ci.ProductVariantId)
            .HasDatabaseName("IX_CartItem_ProductVariantId");
        
        builder.HasIndex(ci => ci.AddedAt)
            .HasDatabaseName("IX_CartItem_AddedAt");
        
        builder.ToTable("CartItems");
    }
}