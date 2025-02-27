using MediatR;

namespace ExchangeRatesManager.Application.Features.ExchangeRates.Queries;

public class GetExchangeRateQuery : IRequest<ExchangeRateViewModel>
{
    public string FromCurrencyCode { get; }
    public string ToCurrencyCode { get; }

    public GetExchangeRateQuery(string fromCurrencyCode, string toCurrencyCode)
    {
        FromCurrencyCode = fromCurrencyCode;
        ToCurrencyCode = toCurrencyCode;
    }
}
