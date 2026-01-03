using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class ColorTranslationConfiguration : IEntityTypeConfiguration<ColorTranslation>
{
    public void Configure(EntityTypeBuilder<ColorTranslation> builder)
    {
        // Primary Key
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.LanguageCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);

        // Relationship
        builder.HasOne(t => t.Color)
            .WithMany(c => c.Translations)
            .HasForeignKey(t => t.ColorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.LanguageCode)
            .HasDatabaseName("IX_ColorTranslation_LanguageCode");

        builder.HasIndex(t => new { t.ColorId, t.LanguageCode })
            .IsUnique()
            .HasDatabaseName("IX_ColorTranslation_Unique_ColorId_LanguageCode");

        builder.ToTable("color_translations");
    }
}
