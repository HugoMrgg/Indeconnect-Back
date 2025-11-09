    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using IndeConnect_Back.Domain.user;

    namespace IndeConnect_Back.Infrastructure.Configurations;

    public class UserReviewConfiguration : IEntityTypeConfiguration<UserReview>
    {
        public void Configure(EntityTypeBuilder<UserReview> builder)
        {
            builder.HasKey(r => r.Id);

            builder.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Brand)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BrandId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(r => r.Rating).IsRequired();
            builder.Property(r => r.CreatedAt).IsRequired();
        }
    }