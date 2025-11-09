using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        // Compiste Key : WishlistId + ProductId
        builder.HasKey(wi => new { wi.WishlistId, wi.ProductId });
        
        // Properties
        builder.Property(wi => wi.AddedAt)
            .IsRequired();
        
        // Relation with Wishlist
        builder.HasOne(wi => wi.Wishlist)
            .WithMany(w => w.Items)
            .HasForeignKey(wi => wi.WishlistId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        // Relation with Product
        builder.HasOne(wi => wi.Product)
            .WithMany()
            .HasForeignKey(wi => wi.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        builder.HasIndex(wi => wi.ProductId)
            .HasDatabaseName("IX_WishlistItem_ProductId");
        
        builder.HasIndex(wi => wi.AddedAt)
            .HasDatabaseName("IX_WishlistItem_AddedAt");
        
        builder.HasIndex(wi => new { wi.ProductId, wi.AddedAt })
            .HasDatabaseName("IX_WishlistItem_ProductAdded");
    }
}