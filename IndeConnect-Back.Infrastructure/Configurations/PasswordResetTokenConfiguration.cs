using IndeConnect_Back.Domain.user;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.ExpiresAt)
            .IsRequired();

        builder.Property(p => p.IsUsed)
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        // Foreign key
        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index pour recherche rapide par token
        builder.HasIndex(p => p.Token)
            .IsUnique();

        // Index pour recherche par UserId
        builder.HasIndex(p => p.UserId);
    }
}