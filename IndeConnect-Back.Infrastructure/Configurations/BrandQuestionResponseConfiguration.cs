using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class BrandQuestionResponseConfiguration : IEntityTypeConfiguration<BrandQuestionResponse>
{
    public void Configure(EntityTypeBuilder<BrandQuestionResponse> builder)
    {
        // Primary Key
        builder.HasKey(br => br.Id);
        
        // Relation with BrandQuestionnaire
        builder.HasOne(br => br.Questionnaire)
               .WithMany(q => q.Responses)
               .HasForeignKey(br => br.QuestionnaireId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
        
        // Relation with EthicsQuestion
        builder.HasOne(br => br.Question)
               .WithMany()
               .HasForeignKey(br => br.QuestionId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        
        builder.Property(br => br.CalculatedScore).IsRequired(false);
        
        builder.HasMany(br => br.SelectedOptions)
               .WithOne(x => x.Response)
               .HasForeignKey(x => x.ResponseId)
               .OnDelete(DeleteBehavior.Cascade);
        
        
        builder.HasIndex(br => new { br.QuestionnaireId, CriterionId = br.QuestionId })
               .IsUnique()
               .HasDatabaseName("IX_BrandQuestionResponse_UniqueQuestionPerQuestionnaire");
        
        builder.HasIndex(br => br.QuestionnaireId)
               .HasDatabaseName("IX_BrandQuestionResponse_QuestionnaireId");
        
        builder.HasIndex(br => br.QuestionId)
               .HasDatabaseName("IX_BrandQuestionResponse_QuestionId");
        
        
    }
}
