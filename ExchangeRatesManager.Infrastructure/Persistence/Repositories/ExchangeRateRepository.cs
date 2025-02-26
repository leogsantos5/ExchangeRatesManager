using ExchangeRatesManager.Domain.Models;
using ExchangeRatesManager.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRatesManager.Infrastructure.Persistence.Repositories;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly DatabaseContext _context;

    public ExchangeRateRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateAsync(ExchangeRate exchangeRate)
    {
        await _context.ExchangeRates.AddAsync(exchangeRate);
        await _context.SaveChangesAsync();
        return exchangeRate.Id;
    }

    public async Task<ExchangeRate?> GetByIdAsync(Guid id)
    {
        return await _context.ExchangeRates.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id);
    }
    public async Task<ExchangeRate?> GetByCurrencyPairAsync(string fromCurrency, string toCurrency)
    {
        return await _context.ExchangeRates.AsNoTracking().FirstOrDefaultAsync(e => e.FromCurrency == fromCurrency && e.ToCurrency == toCurrency);
    }

    public async Task<List<ExchangeRate>?> GetAllAsync()
    {
        return await _context.ExchangeRates.AsNoTracking().ToListAsync();
    }

    public async Task UpdateAsync(ExchangeRate exchangeRate)
    {
        _context.ExchangeRates.Entry(exchangeRate).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ExchangeRate exchangeRate)
    {
        _context.ExchangeRates.Remove(exchangeRate);
        await _context.SaveChangesAsync();
    }
}
