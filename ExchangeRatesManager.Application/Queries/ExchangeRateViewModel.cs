namespace ExchangeRatesManager.Application.Queries;

public class ExchangeRateViewModel
{
    public required string FromCurrency { get; set; }
    public required string ToCurrency { get; set; }
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
}
