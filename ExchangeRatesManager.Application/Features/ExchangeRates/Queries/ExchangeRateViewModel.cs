namespace ExchangeRatesManager.Application.Features.ExchangeRates.Queries;

public class ExchangeRateViewModel
{
    public Guid Id { get; set; }
    public required string FromCurrency { get; set; }
    public required string ToCurrency { get; set; }
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
}
