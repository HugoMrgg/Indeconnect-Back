using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Infrastructure.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Primary Key
        builder.HasKey(r => r.Id);
        
        // Properties
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(r => r.Description)
            .HasMaxLength(500)
            .IsRequired(false);
        
        // Relation with User
        builder.HasMany(r => r.Users)
            .WithOne(u => u.Role)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("IX_Role_UniqueName");
        
        // Seed Data
        builder.HasData(
            new { Id = 1L, Name = "Admin", Description = "Gestion de la gouvernance globale du système" },
            new { Id = 2L, Name = "Moderator", Description = "Responsable du contrôle qualité et modération" },
            new { Id = 3L, Name = "SuperVendor", Description = "Représente une marque et gère son image" },
            new { Id = 4L, Name = "Seller", Description = "Gère le catalogue produit de sa marque" },
            new { Id = 5L, Name = "User", Description = "Client inscrit pouvant passer commande" }
        );
    }
}