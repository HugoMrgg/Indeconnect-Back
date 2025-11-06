using IndeConnect_Back.Domain.catalog.product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class ProductKeywordConfiguration : IEntityTypeConfiguration<ProductKeyword>
{
    public void Configure(EntityTypeBuilder<ProductKeyword> builder)
    {
        builder.HasKey(pk => new { pk.ProductId, pk.KeywordId });
        
        builder.HasOne(pk => pk.Product)
            .WithMany(p => p.Keywords)
            .HasForeignKey(pk => pk.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(pk => pk.Keyword)
            .WithMany()
            .HasForeignKey(pk => pk.KeywordId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.ToTable("ProductKeywords");
    }
}
