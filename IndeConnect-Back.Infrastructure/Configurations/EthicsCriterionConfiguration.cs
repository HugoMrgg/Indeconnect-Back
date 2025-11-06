using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class EthicsQuestionConfiguration : IEntityTypeConfiguration<EthicsQuestion>
{
    public void Configure(EntityTypeBuilder<EthicsQuestion> builder)
    {
        // Primary Key
        builder.HasKey(eq => eq.Id);
        
        // Properties
        builder.Property(eq => eq.Category)
               .HasConversion(new EnumToStringConverter<EthicsCategory>())
               .IsRequired()
               .HasMaxLength(50);
        
        builder.Property(eq => eq.Key)
               .IsRequired()
               .HasMaxLength(100); 
        
        builder.Property(eq => eq.Label)
               .IsRequired()
               .HasMaxLength(500); 
        
        builder.Property(eq => eq.Order)
               .IsRequired()
               .HasDefaultValue(0);
        
        // Relation with EthicsOption
        builder.HasMany(eq => eq.Options)
               .WithOne(eo => eo.Question)
               .HasForeignKey(eo => eo.QuestionId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
        
        builder.HasIndex(eq => eq.Key)
               .IsUnique()
               .HasDatabaseName("IX_EthicsQuestion_UniqueKey");
        
        builder.HasIndex(eq => eq.Category)
               .HasDatabaseName("IX_EthicsQuestion_Category");
        
        builder.HasIndex(eq => new { eq.Category, eq.Order })
               .HasDatabaseName("IX_EthicsQuestion_CategoryOrder");
    }
}
