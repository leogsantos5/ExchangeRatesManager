using MediatR;

namespace ExchangeRatesManager.Application.Features.ExchangeRates.Commands;

public class AddExchangeRateCommand : IRequest<Guid>
{
    public string FromCurrencyCode { get; }
    public string ToCurrencyCode { get; }
    public decimal Bid { get; }
    public decimal Ask { get; }

    public AddExchangeRateCommand(string fromCurrencyCode, string toCurrencyCode, decimal bid, decimal ask)
    {
        FromCurrencyCode = fromCurrencyCode;
        ToCurrencyCode = toCurrencyCode;
        Bid = bid;
        Ask = ask;
    }
}
