using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class BrandSubscriptionConfiguration : IEntityTypeConfiguration<BrandSubscription>
{
    public void Configure(EntityTypeBuilder<BrandSubscription> builder)
    {
        builder.HasKey(bs => bs.Id);
        builder.Property(bs => bs.Id).ValueGeneratedOnAdd();

        // Relation with User
        builder.HasOne(bs => bs.User)
            .WithMany(u => u.BrandSubscriptions)
            .HasForeignKey(bs => bs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relation with Brand
        builder.HasOne(bs => bs.Brand)
            .WithMany() 
            .HasForeignKey(bs => bs.BrandId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(bs => new { bs.UserId, bs.BrandId })
            .IsUnique()
            .HasDatabaseName("IX_BrandSubscription_UserId_BrandId");

        builder.Property(bs => bs.SubscribedAt)
            .IsRequired();
    }
}