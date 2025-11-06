using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) {}
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Applique toutes tes classes de configuration
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
