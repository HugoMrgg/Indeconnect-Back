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
        builder.Property(bq => bq.Status).IsRequired();
        builder.Property(bq => bq.CreatedAt).IsRequired();
        builder.Property(bq => bq.SubmittedAt).IsRequired(false);
        builder.Property(bq => bq.ReviewedAt).IsRequired(false);
        builder.Property(bq => bq.ReviewerAdminUserId).IsRequired(false);
        builder.Property(bq => bq.RejectionReason).IsRequired(false).HasMaxLength(1000);
        
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
        
        builder.HasIndex(bq => bq.Status)
               .HasDatabaseName("IX_BrandQuestionnaire_Status");
        
        builder.HasIndex(bq => new { bq.BrandId, bq.SubmittedAt })
               .HasDatabaseName("IX_BrandQuestionnaire_BrandHistory");
        
        builder.HasIndex(bq => new { bq.BrandId, bq.Status })
               .HasDatabaseName("IX_BrandQuestionnaire_BrandStatus");
        
    }
}
