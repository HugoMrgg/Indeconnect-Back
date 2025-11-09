using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using IndeConnect_Back.Domain.catalog.product;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        // Primary Key
        builder.HasKey(pr => pr.Id);
        
        // Properties
        builder.Property(pr => pr.Rating)
               .IsRequired();
        
        builder.Property(pr => pr.Comment)
               .HasMaxLength(2000)
               .IsRequired(false);
        
        builder.Property(pr => pr.CreatedAt)
               .IsRequired();
        
        builder.Property(pr => pr.UpdatedAt)
               .IsRequired(false);
        
        builder.Property(pr => pr.Status)
               .HasConversion(new EnumToStringConverter<ReviewStatus>())
               .IsRequired()
               .HasMaxLength(20)
               .HasDefaultValue(ReviewStatus.Pending);
        
        // Relation with Product
        builder.HasOne(pr => pr.Product)
               .WithMany(p => p.Reviews)
               .HasForeignKey(pr => pr.ProductId)
               .OnDelete(DeleteBehavior.Restrict) 
               .IsRequired();
        
        // Relation with User
        builder.HasOne(pr => pr.User)
               .WithMany() 
               .HasForeignKey(pr => pr.UserId)
               .OnDelete(DeleteBehavior.Restrict) 
               .IsRequired();
        
        builder.HasIndex(pr => new { pr.ProductId, pr.UserId })
               .IsUnique()
               .HasDatabaseName("IX_ProductReview_UniqueUserProduct");
        
        builder.HasIndex(pr => pr.Status)
               .HasDatabaseName("IX_ProductReview_Status");
        
        builder.HasIndex(pr => new { pr.ProductId, pr.Status })
               .HasDatabaseName("IX_ProductReview_ProductStatus");
        
        builder.HasIndex(pr => pr.CreatedAt)
               .HasDatabaseName("IX_ProductReview_CreatedAt");
        
        builder.HasIndex(pr => pr.UserId)
               .HasDatabaseName("IX_ProductReview_UserId");
    }
}
