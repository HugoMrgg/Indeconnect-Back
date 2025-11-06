using IndeConnect_Back.Domain.order;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(i => i.Amount)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(i => i.IssuedAt)
            .IsRequired();
        
        builder.HasOne(i => i.Order)
            .WithMany(o => o.Invoices)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(i => i.Brand)
            .WithMany()
            .HasForeignKey(i => i.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(i => i.InvoiceNumber)
            .IsUnique()
            .HasDatabaseName("IX_Invoice_UniqueNumber");
        
        builder.ToTable("Invoices");
    }
}
