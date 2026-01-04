using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class SizeTranslationConfiguration : IEntityTypeConfiguration<SizeTranslation>
{
    public void Configure(EntityTypeBuilder<SizeTranslation> builder)
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
        builder.HasOne(t => t.Size)
            .WithMany(s => s.Translations)
            .HasForeignKey(t => t.SizeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.LanguageCode)
            .HasDatabaseName("IX_SizeTranslation_LanguageCode");

        builder.HasIndex(t => new { t.SizeId, t.LanguageCode })
            .IsUnique()
            .HasDatabaseName("IX_SizeTranslation_Unique_SizeId_LanguageCode");

        builder.ToTable("size_translations");
    }
}
