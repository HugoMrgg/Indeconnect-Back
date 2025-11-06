using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IndeConnect_Back.Domain.payment;
using IndeConnect_Back.Domain.order;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        // Primary Key
        builder.HasKey(p => p.Id);
        
        // Properties
        builder.Property(p => p.CreatedAt)
               .IsRequired();
        
        builder.Property(p => p.Currency)
               .IsRequired()
               .HasMaxLength(3)
               .HasDefaultValue("EUR");
        
        builder.Property(p => p.Amount)
               .IsRequired()
               .HasPrecision(18, 2);
        
        builder.Property(p => p.Status)
               .HasConversion(new EnumToStringConverter<PaymentStatus>())
               .IsRequired()
               .HasMaxLength(20)
               .HasDefaultValue(PaymentStatus.Pending);
        
        builder.Property(p => p.TransactionId)
               .HasMaxLength(200) 
               .IsRequired(false);
        
        builder.Property(p => p.RawPayload)
               .HasMaxLength(4000) 
               .IsRequired(false);
        
        // Relation with Order (One-to-One)
        builder.HasOne(p => p.Order)
               .WithOne(o => o.Payment)
               .HasForeignKey<Payment>(p => p.OrderId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        
        // Relation with PaymentProvider
        builder.HasOne(p => p.PaymentProvider)
               .WithMany()
               .HasForeignKey(p => p.PaymentProviderId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        
        builder.HasIndex(p => p.TransactionId)
               .IsUnique()
               .HasFilter("[TransactionId] IS NOT NULL")
               .HasDatabaseName("IX_Payment_UniqueTransactionId");
        
        builder.HasIndex(p => p.OrderId)
               .IsUnique() 
               .HasDatabaseName("IX_Payment_OrderId");
        
        builder.HasIndex(p => p.Status)
               .HasDatabaseName("IX_Payment_Status");
        
        builder.HasIndex(p => p.PaymentProviderId)
               .HasDatabaseName("IX_Payment_ProviderId");
        
        builder.HasIndex(p => new { p.CreatedAt, p.Status })
               .HasDatabaseName("IX_Payment_CreatedStatus");
    }
}
