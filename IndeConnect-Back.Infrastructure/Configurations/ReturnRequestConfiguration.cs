using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IndeConnect_Back.Domain.order;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class ReturnRequestConfiguration : IEntityTypeConfiguration<ReturnRequest>
{
    public void Configure(EntityTypeBuilder<ReturnRequest> builder)
    {
        // Primary Key
        builder.HasKey(rr => rr.Id);
        
        // Properties
        builder.Property(rr => rr.RequestedAt)
               .IsRequired();
        
        builder.Property(rr => rr.Reason)
               .HasMaxLength(1000)
               .IsRequired(false); 
        
        builder.Property(rr => rr.ProcessedAt)
               .IsRequired(false); 
        
        builder.Property(rr => rr.Status)
               .HasConversion(new EnumToStringConverter<ReturnStatus>())
               .IsRequired()
               .HasMaxLength(20)
               .HasDefaultValue(ReturnStatus.Requested);
        
        // Relation with Order
        builder.HasOne(rr => rr.Order)
               .WithMany(o => o.Returns)
               .HasForeignKey(rr => rr.OrderId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        
        // Relation with User
        builder.HasOne(rr => rr.User)
               .WithMany()
               .HasForeignKey(rr => rr.UserId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        
        builder.HasIndex(rr => rr.OrderId)
               .HasDatabaseName("IX_ReturnRequest_OrderId");
        
        builder.HasIndex(rr => rr.UserId)
               .HasDatabaseName("IX_ReturnRequest_UserId");
        
        builder.HasIndex(rr => rr.Status)
               .HasDatabaseName("IX_ReturnRequest_Status");
        
        builder.HasIndex(rr => new { rr.UserId, rr.Status })
               .HasDatabaseName("IX_ReturnRequest_UserStatus");
        
        builder.HasIndex(rr => rr.RequestedAt)
               .HasDatabaseName("IX_ReturnRequest_RequestedAt");
    }
}
