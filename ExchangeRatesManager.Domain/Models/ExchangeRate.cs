namespace ExchangeRatesManager.Domain.Models;

public class ExchangeRate 
{
    public Guid Id { get; private set; }
    public string FromCurrency { get; private set; } 
    public string ToCurrency { get; private set; }  
    public decimal Bid { get; private set; }
    public decimal Ask { get; private set; }

    public ExchangeRate(string fromCurrency, string toCurrency, decimal bid, decimal ask)
    {
        FromCurrency = fromCurrency;
        ToCurrency = toCurrency;
        Bid = bid;
        Ask = ask;
    }
}


