using Microsoft.EntityFrameworkCore;
using IndeConnect_Back.Domain.catalog.product;

namespace IndeConnect_Back.Infrastructure.Configurations;

public static class SeedDataConfiguration
{
    public static void SeedData(this ModelBuilder modelBuilder)
    {
        // Seed Categories (si pas déjà fait ailleurs)
        modelBuilder.Entity<Category>().HasData(
            new { Id = 100L, Name = "T-Shirts" },
            new { Id = 101L, Name = "Jeans" },
            new { Id = 102L, Name = "Shoes" },
            new { Id = 103L, Name = "Accessories" },
            new { Id = 104L, Name = "Dresses" },
            new { Id = 105L, Name = "Jackets" },
            new { Id = 106L, Name = "Hoodies" },
            new { Id = 107L, Name = "Pants" },
            new { Id = 108L, Name = "Skirts" },
            new { Id = 109L, Name = "Swimwear" }
        );
        
        // Seed Sizes avec CategoryId et SortOrder
        modelBuilder.Entity<Size>().HasData(
            // T-Shirts (100), Hoodies (106), Jackets (105), Dresses (104) - Tailles lettres
            new { Id = 1L, Name = "XS", CategoryId = 100L, SortOrder = 1 },
            new { Id = 2L, Name = "S", CategoryId = 100L, SortOrder = 2 },
            new { Id = 3L, Name = "M", CategoryId = 100L, SortOrder = 3 },
            new { Id = 4L, Name = "L", CategoryId = 100L, SortOrder = 4 },
            new { Id = 5L, Name = "XL", CategoryId = 100L, SortOrder = 5 },
            new { Id = 6L, Name = "XXL", CategoryId = 100L, SortOrder = 6 },
            new { Id = 7L, Name = "XXXL", CategoryId = 100L, SortOrder = 7 },
            
            // Hoodies - mêmes tailles
            new { Id = 50L, Name = "XS", CategoryId = 106L, SortOrder = 1 },
            new { Id = 51L, Name = "S", CategoryId = 106L, SortOrder = 2 },
            new { Id = 52L, Name = "M", CategoryId = 106L, SortOrder = 3 },
            new { Id = 53L, Name = "L", CategoryId = 106L, SortOrder = 4 },
            new { Id = 54L, Name = "XL", CategoryId = 106L, SortOrder = 5 },
            new { Id = 55L, Name = "XXL", CategoryId = 106L, SortOrder = 6 },
            
            // Jackets - mêmes tailles
            new { Id = 60L, Name = "XS", CategoryId = 105L, SortOrder = 1 },
            new { Id = 61L, Name = "S", CategoryId = 105L, SortOrder = 2 },
            new { Id = 62L, Name = "M", CategoryId = 105L, SortOrder = 3 },
            new { Id = 63L, Name = "L", CategoryId = 105L, SortOrder = 4 },
            new { Id = 64L, Name = "XL", CategoryId = 105L, SortOrder = 5 },
            new { Id = 65L, Name = "XXL", CategoryId = 105L, SortOrder = 6 },
            
            // Dresses - mêmes tailles
            new { Id = 70L, Name = "XS", CategoryId = 104L, SortOrder = 1 },
            new { Id = 71L, Name = "S", CategoryId = 104L, SortOrder = 2 },
            new { Id = 72L, Name = "M", CategoryId = 104L, SortOrder = 3 },
            new { Id = 73L, Name = "L", CategoryId = 104L, SortOrder = 4 },
            new { Id = 74L, Name = "XL", CategoryId = 104L, SortOrder = 5 },
            new { Id = 75L, Name = "XXL", CategoryId = 104L, SortOrder = 6 },
            
            // Jeans (101) et Pants (107) - Tailles chiffres
            new { Id = 20L, Name = "28", CategoryId = 101L, SortOrder = 1 },
            new { Id = 21L, Name = "30", CategoryId = 101L, SortOrder = 2 },
            new { Id = 22L, Name = "32", CategoryId = 101L, SortOrder = 3 },
            new { Id = 23L, Name = "34", CategoryId = 101L, SortOrder = 4 },
            new { Id = 24L, Name = "36", CategoryId = 101L, SortOrder = 5 },
            new { Id = 25L, Name = "38", CategoryId = 101L, SortOrder = 6 },
            
            // Pants - mêmes tailles
            new { Id = 80L, Name = "28", CategoryId = 107L, SortOrder = 1 },
            new { Id = 81L, Name = "30", CategoryId = 107L, SortOrder = 2 },
            new { Id = 82L, Name = "32", CategoryId = 107L, SortOrder = 3 },
            new { Id = 83L, Name = "34", CategoryId = 107L, SortOrder = 4 },
            new { Id = 84L, Name = "36", CategoryId = 107L, SortOrder = 5 },
            new { Id = 85L, Name = "38", CategoryId = 107L, SortOrder = 6 },
            
            // Shoes (102) - Pointures
            new { Id = 10L, Name = "36", CategoryId = 102L, SortOrder = 1 },
            new { Id = 11L, Name = "37", CategoryId = 102L, SortOrder = 2 },
            new { Id = 12L, Name = "38", CategoryId = 102L, SortOrder = 3 },
            new { Id = 13L, Name = "39", CategoryId = 102L, SortOrder = 4 },
            new { Id = 14L, Name = "40", CategoryId = 102L, SortOrder = 5 },
            new { Id = 15L, Name = "41", CategoryId = 102L, SortOrder = 6 },
            new { Id = 16L, Name = "42", CategoryId = 102L, SortOrder = 7 },
            new { Id = 17L, Name = "43", CategoryId = 102L, SortOrder = 8 },
            new { Id = 18L, Name = "44", CategoryId = 102L, SortOrder = 9 },
            new { Id = 19L, Name = "45", CategoryId = 102L, SortOrder = 10 },
            
            // Accessories (103), Skirts (108), Swimwear (109) - Taille unique
            new { Id = 99L, Name = "Unique", CategoryId = 103L, SortOrder = 1 },
            new { Id = 100L, Name = "Unique", CategoryId = 108L, SortOrder = 1 },
            new { Id = 101L, Name = "Unique", CategoryId = 109L, SortOrder = 1 }
        );
    }
}
