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
        
        // Relation with EthicsOption
        builder.HasOne(br => br.Option)
               .WithMany()
               .HasForeignKey(br => br.OptionId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        
        builder.HasIndex(br => new { br.QuestionnaireId, CriterionId = br.QuestionId })
               .IsUnique()
               .HasDatabaseName("IX_BrandQuestionResponse_UniqueQuestionPerQuestionnaire");
        
        builder.HasIndex(br => br.QuestionnaireId)
               .HasDatabaseName("IX_BrandQuestionResponse_QuestionnaireId");
        
        builder.HasIndex(br => br.QuestionId)
               .HasDatabaseName("IX_BrandQuestionResponse_QuestionId");
        
        builder.HasIndex(br => br.OptionId)
               .HasDatabaseName("IX_BrandQuestionResponse_OptionId");
    }
}
