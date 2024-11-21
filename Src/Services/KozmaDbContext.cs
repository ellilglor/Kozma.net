using Kozma.net.Src.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kozma.net.Src.Services;

public class KozmaDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Exchange> Exchange { get; set; }
    public DbSet<TradeLog> TradeLogs { get; set; }
    public DbSet<Command> Commands { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<BuyMute> BuyMutes { get; set; }
    public DbSet<SellMute> SellMutes { get; set; }
    public DbSet<Unbox> Boxes { get; set; }
    public DbSet<Gambler> Gamblers { get; set; }
    public DbSet<SearchedLog> SearchedLogs { get; set; }
    public DbSet<TimedTask> TimedTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Exchange>();
        modelBuilder.Entity<TradeLog>();
        modelBuilder.Entity<Command>();
        modelBuilder.Entity<User>();
        modelBuilder.Entity<BuyMute>();
        modelBuilder.Entity<SellMute>();
        modelBuilder.Entity<Unbox>();
        modelBuilder.Entity<Gambler>();
        modelBuilder.Entity<SearchedLog>();
        modelBuilder.Entity<TimedTask>();
    }
}
