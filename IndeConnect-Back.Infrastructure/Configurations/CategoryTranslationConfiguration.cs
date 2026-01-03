using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class CategoryTranslationConfiguration : IEntityTypeConfiguration<CategoryTranslation>
{
    public void Configure(EntityTypeBuilder<CategoryTranslation> builder)
    {
        // Primary Key
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.LanguageCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Relationship
        builder.HasOne(t => t.Category)
            .WithMany(c => c.Translations)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.LanguageCode)
            .HasDatabaseName("IX_CategoryTranslation_LanguageCode");

        builder.HasIndex(t => new { t.CategoryId, t.LanguageCode })
            .IsUnique()
            .HasDatabaseName("IX_CategoryTranslation_Unique_CategoryId_LanguageCode");

        builder.ToTable("category_translations");
    }
}
