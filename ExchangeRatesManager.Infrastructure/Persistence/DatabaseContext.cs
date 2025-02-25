using ExchangeRatesManager.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRatesManager.Infrastructure.Persistence;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<ExchangeRate> ExchangeRates { get; set; }
}
