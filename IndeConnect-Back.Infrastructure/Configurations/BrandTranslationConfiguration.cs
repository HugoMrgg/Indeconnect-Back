using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class BrandTranslationConfiguration : IEntityTypeConfiguration<BrandTranslation>
{
    public void Configure(EntityTypeBuilder<BrandTranslation> builder)
    {
        // Primary Key
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.LanguageCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.AboutUs)
            .HasMaxLength(2000);

        builder.Property(t => t.WhereAreWe)
            .HasMaxLength(2000);

        builder.Property(t => t.OtherInfo)
            .HasMaxLength(2000);

        // Relationship
        builder.HasOne(t => t.Brand)
            .WithMany(b => b.Translations)
            .HasForeignKey(t => t.BrandId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.LanguageCode)
            .HasDatabaseName("IX_BrandTranslation_LanguageCode");

        builder.HasIndex(t => new { t.BrandId, t.LanguageCode })
            .IsUnique()
            .HasDatabaseName("IX_BrandTranslation_Unique_BrandId_LanguageCode");

        builder.ToTable("brand_translations");
    }
}
