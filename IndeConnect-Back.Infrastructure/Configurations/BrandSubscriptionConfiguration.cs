using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class BrandSubscriptionConfiguration : IEntityTypeConfiguration<BrandSubscription>
{
    public void Configure(EntityTypeBuilder<BrandSubscription> builder)
    {
        builder.HasKey(bs => bs.Id);
        builder.Property(bs => bs.Id).ValueGeneratedOnAdd();
    }
}
