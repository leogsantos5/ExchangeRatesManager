using ExchangeRatesManager.Domain.Models;
namespace ExchangeRatesManager.Domain.Repositories;

public interface IExchangeRateRepository
{
    Task<Guid> CreateAsync(ExchangeRate exchangeRate);
    Task<ExchangeRate?> GetByIdAsync(Guid id);
    Task<ExchangeRate?> GetByCurrencyPairAsync(string fromCurrency, string toCurrency);
    Task UpdateAsync(ExchangeRate exchangeRate);
    Task DeleteAsync(ExchangeRate exchangeRate);
}