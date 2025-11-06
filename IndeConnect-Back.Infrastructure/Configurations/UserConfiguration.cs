using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.user;
using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Primary Key
        builder.HasKey(u => u.Id);
        
        // Properties - generales informations
        builder.Property(u => u.Email)
               .IsRequired()
               .HasMaxLength(255);
        
        builder.Property(u => u.FirstName)
               .IsRequired()
               .HasMaxLength(100);
        
        builder.Property(u => u.LastName)
               .IsRequired()
               .HasMaxLength(100);
        
        builder.Property(u => u.PasswordHash)
               .HasMaxLength(255)
               .IsRequired(false); 
        
        builder.Property(u => u.CreatedAt)
               .IsRequired();
        
        builder.Property(u => u.IsEnabled)
               .IsRequired()
               .HasDefaultValue(true);
        
        // Properties - Invitation
        builder.Property(u => u.InvitationTokenHash)
               .HasMaxLength(255)
               .IsRequired(false);
        
        builder.Property(u => u.InvitationExpiresAt)
               .IsRequired(false);
        
        builder.Ignore(u => u.IsInvitationPending);
        
        // Relation with Role
        builder.HasOne(u => u.Role)
               .WithMany(r => r.Users)
               .HasForeignKey(u => u.RoleId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        
        // Relation One-to-One with Cart
        builder.HasOne(u => u.Cart)
               .WithOne(c => c.User)
               .HasForeignKey<Cart>(c => c.UserId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
        
        // Relation One-to-One with Wishlist
        builder.HasOne(u => u.Wishlist)
               .WithOne(w => w.User)
               .HasForeignKey<Wishlist>(w => w.UserId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired(false);
        
        // Relation with ShippingAddresses
        builder.HasMany(u => u.ShippingAddresses)
               .WithOne(sa => sa.User)
               .HasForeignKey(sa => sa.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        
        // Relation with Orders
        builder.HasMany(u => u.Orders)
               .WithOne(o => o.User)
               .HasForeignKey(o => o.UserId)
               .OnDelete(DeleteBehavior.Restrict);
        
        // Relation with ReturnRequests
        builder.HasMany(u => u.Returns)
               .WithOne(rr => rr.User)
               .HasForeignKey(rr => rr.UserId)
               .OnDelete(DeleteBehavior.Restrict);
        
        // Relation with BrandSubscriptions
        builder.HasMany(u => u.BrandSubscriptions)
               .WithOne(bs => bs.User)
               .HasForeignKey(bs => bs.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        
        // Relation with PaymentMethods
        builder.HasMany(u => u.PaymentMethods)
               .WithOne(pm => pm.User)
               .HasForeignKey(pm => pm.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        
        // Relation Many-to-One : Brands as SuperVendor
        builder.HasMany(u => u.BrandsAsSuperVendor)
               .WithOne(b => b.SuperVendorUser)
               .HasForeignKey(b => b.SuperVendorUserId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);
        
        // Relation Many-to-Many : Brands as Seller
        builder.HasMany(u => u.BrandsAsSeller)
               .WithOne(bs => bs.Seller)
               .HasForeignKey(bs => bs.SellerId)
               .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(u => u.Email)
               .IsUnique()
               .HasDatabaseName("IX_User_UniqueEmail");
        
        builder.HasIndex(u => u.RoleId)
               .HasDatabaseName("IX_User_RoleId");
        
        builder.HasIndex(u => u.IsEnabled)
               .HasDatabaseName("IX_User_IsEnabled");
        
        builder.HasIndex(u => new { u.LastName, u.FirstName })
               .HasDatabaseName("IX_User_FullName");
        
        builder.HasIndex(u => u.CreatedAt)
               .HasDatabaseName("IX_User_CreatedAt");
    }
}
