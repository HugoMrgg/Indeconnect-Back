using IndeConnect_Back.Domain.payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class PaymentProviderConfiguration : IEntityTypeConfiguration<PaymentProvider>
{
    public void Configure(EntityTypeBuilder<PaymentProvider> builder)
    {
        builder.HasKey(pp => pp.Id);
        
        builder.Property(pp => pp.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(pp => pp.Description)
            .HasMaxLength(500)
            .IsRequired(false);
        
        builder.Property(pp => pp.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(pp => pp.LogoUrl)
            .HasMaxLength(500)
            .IsRequired(false);
        
        builder.HasIndex(pp => pp.Name)
            .IsUnique()
            .HasDatabaseName("IX_PaymentProvider_UniqueName");
        
        builder.ToTable("PaymentProviders");
        
        // Seed Data
        builder.HasData(
            new { Id = 1L, Name = "Stripe", Description = "Paiement par carte bancaire via Stripe", IsEnabled = true },
            new { Id = 2L, Name = "PayPal", Description = "Paiement via compte PayPal", IsEnabled = true },
            new { Id = 3L, Name = "Bancontact", Description = "Paiement Bancontact (Belgique)", IsEnabled = true }
        );
    }
}
