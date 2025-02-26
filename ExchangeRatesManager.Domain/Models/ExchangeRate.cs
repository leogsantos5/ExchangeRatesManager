namespace ExchangeRatesManager.Domain.Models;

public class ExchangeRate 
{
    public Guid Id { get; private set; }
    public string FromCurrency { get; set; } 
    public string ToCurrency { get; set; }  
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }

    public ExchangeRate(string fromCurrency, string toCurrency, decimal bid, decimal ask)
    {
        FromCurrency = fromCurrency;
        ToCurrency = toCurrency;
        Bid = bid;
        Ask = ask;
    }
}


