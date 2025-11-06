using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class ShippingAddressConfiguration : IEntityTypeConfiguration<ShippingAddress>
{
    public void Configure(EntityTypeBuilder<ShippingAddress> builder)
    {
        // Primary Key
        builder.HasKey(sa => sa.Id);
        
        // Properties
        builder.Property(sa => sa.Street)
               .IsRequired()
               .HasMaxLength(200);
        
        builder.Property(sa => sa.Number)
               .IsRequired()
               .HasMaxLength(20); 
        
        builder.Property(sa => sa.PostalCode)
               .IsRequired()
               .HasMaxLength(20); 
        
        builder.Property(sa => sa.City)
               .IsRequired()
               .HasMaxLength(100);
        
        builder.Property(sa => sa.Country)
               .IsRequired()
               .HasMaxLength(2) 
               .HasDefaultValue("BE");
        
        builder.Property(sa => sa.Extra)
               .HasMaxLength(500)
               .IsRequired(false); 
        
        builder.Property(sa => sa.IsDefault)
               .IsRequired()
               .HasDefaultValue(false);
        
        // Relation with User
        builder.HasOne(sa => sa.User)
               .WithMany(u => u.ShippingAddresses)
               .HasForeignKey(sa => sa.UserId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
        
        builder.HasIndex(sa => sa.UserId)
               .HasDatabaseName("IX_ShippingAddress_UserId");
        
        builder.HasIndex(sa => new { sa.UserId, sa.IsDefault })
               .HasDatabaseName("IX_ShippingAddress_UserDefault");
        
        builder.HasIndex(sa => sa.PostalCode)
               .HasDatabaseName("IX_ShippingAddress_PostalCode");
    }
}
