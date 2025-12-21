using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class CatalogVersionConfiguration : IEntityTypeConfiguration<CatalogVersion>
{
    public void Configure(EntityTypeBuilder<CatalogVersion> builder)
    {
        // Primary Key
        builder.HasKey(cv => cv.Id);

        // Properties
        builder.Property(cv => cv.VersionNumber)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(cv => cv.CreatedAt).IsRequired();
        builder.Property(cv => cv.PublishedAt).IsRequired(false);
        builder.Property(cv => cv.IsActive).IsRequired();
        builder.Property(cv => cv.IsDraft).IsRequired();

        // Unique constraint on VersionNumber
        builder.HasIndex(cv => cv.VersionNumber)
               .IsUnique()
               .HasDatabaseName("IX_CatalogVersion_VersionNumber");

        // Index on IsActive for quick lookup of active version
        builder.HasIndex(cv => cv.IsActive)
               .HasDatabaseName("IX_CatalogVersion_IsActive");

        // Index on IsDraft
        builder.HasIndex(cv => cv.IsDraft)
               .HasDatabaseName("IX_CatalogVersion_IsDraft");

        // Relation with Questions
        builder.HasMany(cv => cv.Questions)
               .WithOne(q => q.CatalogVersion)
               .HasForeignKey(q => q.CatalogVersionId)
               .OnDelete(DeleteBehavior.Cascade);

        // Relation with Questionnaires
        builder.HasMany(cv => cv.Questionnaires)
               .WithOne(bq => bq.CatalogVersion)
               .HasForeignKey(bq => bq.CatalogVersionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
