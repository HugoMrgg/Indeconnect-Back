using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class EthicsOptionConfiguration : IEntityTypeConfiguration<EthicsOption>
{
    public void Configure(EntityTypeBuilder<EthicsOption> builder)
    {
        // Primary Key
        builder.HasKey(eo => eo.Id);
        
        // Properties
        builder.Property(eo => eo.Key)
            .IsRequired()
            .HasMaxLength(100); 
        
        builder.Property(eo => eo.Label)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(eo => eo.Score)
            .IsRequired()
            .HasPrecision(5, 2); 
        
        // Relation with EthicsQuestion
        builder.HasOne(eo => eo.Question)
            .WithMany(eq => eq.Options)
            .HasForeignKey(eo => eo.QuestionId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.HasIndex(eo => new { QuesitonId = eo.QuestionId, eo.Key })
            .IsUnique()
            .HasDatabaseName("IX_EthicsOption_UniqueKeyPerQuestion");
        
        builder.HasIndex(eo => eo.Score)
            .HasDatabaseName("IX_EthicsOption_Score");
        
        builder.HasIndex(eo => eo.QuestionId)
            .HasDatabaseName("IX_EthicsOption_QuestionId");
    }
}