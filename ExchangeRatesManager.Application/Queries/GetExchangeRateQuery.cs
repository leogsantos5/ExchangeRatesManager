using MediatR;

namespace ExchangeRatesManager.Application.Queries;

public class GetExchangeRateQuery : IRequest<ExchangeRateViewModel>  // Returns a DTO
{
    public string FromCurrencyCode { get; }
    public string ToCurrencyCode { get; }

    public GetExchangeRateQuery(string fromCurrencyCode, string toCurrencyCode)
    {
        FromCurrencyCode = fromCurrencyCode;
        ToCurrencyCode = toCurrencyCode;
    }
}
