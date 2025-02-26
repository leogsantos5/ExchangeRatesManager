using ExchangeRatesManager.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRatesManager.Infrastructure.Persistence;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<ExchangeRate> ExchangeRates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExchangeRate>().Property(e => e.Bid).HasPrecision(20, 8); 
        modelBuilder.Entity<ExchangeRate>().Property(e => e.Ask).HasPrecision(20, 8);
    }
}

