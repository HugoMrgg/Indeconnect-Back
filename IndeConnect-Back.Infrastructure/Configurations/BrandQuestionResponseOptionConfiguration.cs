using IndeConnect_Back.Domain.catalog.brand;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class BrandQuestionResponseOptionConfiguration : IEntityTypeConfiguration<BrandQuestionResponseOption>
{
    public void Configure(EntityTypeBuilder<BrandQuestionResponseOption> builder)
    {
        builder.HasKey(x => new { x.ResponseId, x.OptionId });

        builder.HasOne(x => x.Option)
            .WithMany()
            .HasForeignKey(x => x.OptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OptionId);
    }
}