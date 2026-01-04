using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IndeConnect_Back.Domain.catalog.brand;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class BrandModerationHistoryConfiguration : IEntityTypeConfiguration<BrandModerationHistory>
{
    public void Configure(EntityTypeBuilder<BrandModerationHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Action)
            .HasConversion(new EnumToStringConverter<ModerationAction>())
            .IsRequired();

        builder.Property(h => h.Comment)
            .HasMaxLength(2000);

        builder.Property(h => h.CreatedAt)
            .IsRequired();

        // Relation avec Brand
        builder.HasOne(h => h.Brand)
            .WithMany(b => b.ModerationHistory)
            .HasForeignKey(h => h.BrandId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relation avec User (Moderator)
        builder.HasOne(h => h.ModeratorUser)
            .WithMany()
            .HasForeignKey(h => h.ModeratorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index pour performance
        builder.HasIndex(h => h.BrandId);
        builder.HasIndex(h => h.CreatedAt);
    }
}