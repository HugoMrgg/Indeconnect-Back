using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class BrandQuestionnaireConfiguration : IEntityTypeConfiguration<BrandQuestionnaire>
{
    public void Configure(EntityTypeBuilder<BrandQuestionnaire> builder)
    {
        // Primary Key
        builder.HasKey(bq => bq.Id);
        
        // Properties
        builder.Property(bq => bq.SubmittedAt)
               .IsRequired();
        
        builder.Property(bq => bq.IsApproved)
               .IsRequired()
               .HasDefaultValue(false);
        
        builder.Property(bq => bq.ApprovedAt)
               .IsRequired(false);
        
        // Relation with Brand
        builder.HasOne(bq => bq.Brand)
               .WithMany(b => b.Questionnaires)
               .HasForeignKey(bq => bq.BrandId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

        // Relation with Responses
        builder.HasMany(bq => bq.Responses)
               .WithOne(br => br.Questionnaire)
               .HasForeignKey(br => br.QuestionnaireId)
               .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(bq => bq.IsApproved)
               .HasDatabaseName("IX_BrandQuestionnaire_IsApproved");
        
        builder.HasIndex(bq => new { bq.BrandId, bq.SubmittedAt })
               .HasDatabaseName("IX_BrandQuestionnaire_BrandHistory");
        
        builder.HasIndex(bq => new { bq.BrandId, bq.IsApproved })
               .HasDatabaseName("IX_BrandQuestionnaire_BrandApproval");
        
    }
}
