using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class KeywordConfiguration : IEntityTypeConfiguration<Keyword>
{
    public void Configure(EntityTypeBuilder<Keyword> builder)
    {
        // Primary Key
        builder.HasKey(k => k.Id);
        
        // Properties
        builder.Property(k => k.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        // Relation with ProductKeyword
        builder.HasMany<ProductKeyword>()
            .WithOne(pk => pk.Keyword)
            .HasForeignKey(pk => pk.KeywordId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(k => k.Name)
            .IsUnique()
            .HasDatabaseName("IX_Keyword_UniqueName");
        
    }
}