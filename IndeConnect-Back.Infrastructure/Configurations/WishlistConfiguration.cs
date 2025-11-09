using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        // Primary Key
        builder.HasKey(w => w.Id);
        
        // Relation One-to-One with User
        builder.HasOne(w => w.User)
            .WithOne(u => u.Wishlist)
            .HasForeignKey<Wishlist>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        // Relation One-to-Many with WishlistItems
        builder.HasMany(w => w.Items)
            .WithOne(wi => wi.Wishlist)
            .HasForeignKey(wi => wi.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(w => w.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Wishlist_UniqueUserWishlist");
    }
}