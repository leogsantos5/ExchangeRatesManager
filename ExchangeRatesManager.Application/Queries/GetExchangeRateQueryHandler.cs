using AutoMapper;
using ExchangeRatesManager.Application.Exceptions;
using ExchangeRatesManager.Domain.Models;
using ExchangeRatesManager.Domain.Repositories;
using MediatR;

namespace ExchangeRatesManager.Application.Queries;

public class GetExchangeRateQueryHandler : IRequestHandler<GetExchangeRateQuery, ExchangeRateViewModel>
{
    private readonly IMapper _mapper;
    private readonly IExchangeRateRepository _exchangingRateRepo;

    public GetExchangeRateQueryHandler(IMapper mapper, IExchangeRateRepository exchangingRateRepo)
    {
        _mapper = mapper;
        _exchangingRateRepo = exchangingRateRepo;
    }

    public async Task<ExchangeRateViewModel> Handle(GetExchangeRateQuery request, CancellationToken cancellationToken)
    {
        var exchangeRate = await _exchangingRateRepo.GetByCurrencyPairAsync(request.FromCurrencyCode, request.ToCurrencyCode);
        if (exchangeRate is null)
            throw new NotFoundException(nameof(ExchangeRate), new { request.FromCurrencyCode, request.ToCurrencyCode });

        return _mapper.Map<ExchangeRateViewModel>(exchangeRate);
    }
}
