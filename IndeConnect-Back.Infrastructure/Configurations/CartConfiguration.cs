using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        // Primary Key
        builder.HasKey(c => c.Id);
        
        // Properties 
        builder.Property(c => c.CreatedAt)
            .IsRequired();
        
        builder.Property(c => c.UpdatedAt)
            .IsRequired();
        
        // Relation One-to-One with User
        builder.HasOne(c => c.User)
            .WithOne(u => u.Cart)
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        // Relation One-to-Many with CartItems
        builder.HasMany(c => c.Items)
            .WithOne(ci => ci.Cart)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(c => c.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Cart_UniqueUserCart");
        
        builder.HasIndex(c => c.CreatedAt)
            .HasDatabaseName("IX_Cart_CreatedAt");
        
        builder.HasIndex(c => c.UpdatedAt)
            .HasDatabaseName("IX_Cart_UpdatedAt");
    }
}