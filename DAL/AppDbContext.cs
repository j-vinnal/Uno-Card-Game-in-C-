using Domain.Database;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public class AppDbContext: DbContext
{
    public DbSet<Domain.Database.Game> Games { get; set; } = default!;
    public DbSet<Domain.Database.Player> Players { get; set; } = default!;
    
    
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}