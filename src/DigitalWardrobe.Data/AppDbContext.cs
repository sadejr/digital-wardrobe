using Microsoft.EntityFrameworkCore;

namespace DigitalWardrobe.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}