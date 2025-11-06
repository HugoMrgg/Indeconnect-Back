using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.payment;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class UserPaymentMethodConfiguration : IEntityTypeConfiguration<UserPaymentMethod>
{
    public void Configure(EntityTypeBuilder<UserPaymentMethod> builder)
    {
        // Primary Key
        builder.HasKey(upm => upm.Id);
        
        // Properties
        builder.Property(upm => upm.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        // Relation with User
        builder.HasOne(upm => upm.User)
            .WithMany(u => u.PaymentMethods)
            .HasForeignKey(upm => upm.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        // Relation with PaymentProvider
        builder.HasOne(upm => upm.PaymentProvider)
            .WithMany()
            .HasForeignKey(upm => upm.PaymentProviderId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        builder.HasIndex(upm => upm.UserId)
            .HasDatabaseName("IX_UserPaymentMethod_UserId");
        
        builder.HasIndex(upm => new { upm.UserId, upm.IsActive })
            .HasDatabaseName("IX_UserPaymentMethod_UserActive");
        
        builder.HasIndex(upm => upm.PaymentProviderId)
            .HasDatabaseName("IX_UserPaymentMethod_ProviderId");    
    }
}