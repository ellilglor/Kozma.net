using Kozma.net.Models;
using Microsoft.EntityFrameworkCore;

namespace Kozma.net.Services;

public class KozmaDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Exchange> Exchange { get; set; }
    public DbSet<TradeLog> TradeLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Exchange>();
        modelBuilder.Entity<TradeLog>();
    }
}
