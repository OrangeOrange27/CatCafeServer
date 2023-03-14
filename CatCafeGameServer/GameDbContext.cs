using Microsoft.EntityFrameworkCore;
using SharedLibrary;

namespace CatCafeGameServer;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<User> Users { get; set; }
}