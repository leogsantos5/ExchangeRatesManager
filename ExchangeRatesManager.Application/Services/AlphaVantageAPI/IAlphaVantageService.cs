using Refit;
using System.Text.Json.Serialization;

namespace ExchangeRatesManager.Application.Services.AlphaVantageAPI;

public interface IAlphaVantageService
{
    [Get("/query?function=CURRENCY_EXCHANGE_RATE&from_currency={fromCurrency}&to_currency={toCurrency}&apikey={apiKey}")]
    Task<AlphaVantageResponse> GetExchangeRateAsync(string fromCurrency, string toCurrency, string apiKey);
}

public class AlphaVantageResponse
{
    [JsonPropertyName("Realtime Currency Exchange Rate")]
    public required ExchangeRateData? ExchangeRateData { get; set; }
}

public class ExchangeRateData
{
    [JsonPropertyName("5. Exchange Rate")]
    public required string Rate { get; set; }

    [JsonPropertyName("8. Bid Price")]
    public required string Bid { get; set; }

    [JsonPropertyName("9. Ask Price")]
    public required string Ask { get; set; }
}

