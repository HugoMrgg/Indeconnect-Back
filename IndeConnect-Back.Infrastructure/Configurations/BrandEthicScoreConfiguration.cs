using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class BrandEthicScoreConfiguration : IEntityTypeConfiguration<BrandEthicScore>
{
    public void Configure(EntityTypeBuilder<BrandEthicScore> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RawScore).IsRequired();
        builder.Property(x => x.FinalScore).IsRequired();
        builder.Property(x => x.IsOfficial).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Brand)
            .WithMany()
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Questionnaire)
            .WithMany()
            .HasForeignKey(x => x.QuestionnaireId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.BrandId, x.CategoryId, x.IsOfficial });
        builder.HasIndex(x => x.QuestionnaireId);
    }
}