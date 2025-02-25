using ExchangeRatesManager.Domain.Models;

namespace ExchangeRatesManager.Domain.Repositories;

public interface IExchangeRateRepository
{
    Task<Guid> AddAsync(ExchangeRate exchangeRate);
    Task<ExchangeRate?> GetByCurrencyPairAsync(string fromCurrency, string toCurrency);
    Task<List<ExchangeRate>> GetAllAsync();
}