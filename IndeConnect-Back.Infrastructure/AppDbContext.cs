using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Futurs DbSets

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
    }
}