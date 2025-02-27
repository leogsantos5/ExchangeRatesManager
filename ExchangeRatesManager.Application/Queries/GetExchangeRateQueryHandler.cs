using AutoMapper;
using ExchangeRatesManager.Application.Exceptions;
using ExchangeRatesManager.Application.Services.AlphaVantageAPI;
using ExchangeRatesManager.Application.Services.RabbitMQ;
using ExchangeRatesManager.Domain.Models;
using ExchangeRatesManager.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace ExchangeRatesManager.Application.Queries;

public class GetExchangeRateQueryHandler : IRequestHandler<GetExchangeRateQuery, ExchangeRateViewModel>
{
    private readonly IExchangeRateRepository _exchangeRateRepo;
    private readonly IAlphaVantageService _alphaVantageService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private readonly ExchangeRatePublisher _exchangeRatePublisher;

    public GetExchangeRateQueryHandler(IExchangeRateRepository exchangeRateRepo, IAlphaVantageService alphaVantageService, 
                                       IMapper mapper, IConfiguration config, ExchangeRatePublisher exchangeRatePublisher)
    {
        _exchangeRateRepo = exchangeRateRepo;
        _alphaVantageService = alphaVantageService;
        _mapper = mapper;
        _config = config;
        _exchangeRatePublisher = exchangeRatePublisher;
    }

    public async Task<ExchangeRateViewModel> Handle(GetExchangeRateQuery request, CancellationToken cancellationToken)
    {
        var exchangeRate = await _exchangeRateRepo.GetByCurrencyPairAsync(request.FromCurrencyCode, request.ToCurrencyCode);
        if (exchangeRate == null)
        {
            var apiKey = _config["AlphaVantage:ApiKey"]!;

            var externalRate = await _alphaVantageService.GetExchangeRateAsync(request.FromCurrencyCode, request.ToCurrencyCode, apiKey);
            if (externalRate == null || externalRate.ExchangeRateData == null)
                throw new NotFoundException(nameof(ExchangeRate), new { request.FromCurrencyCode, request.ToCurrencyCode });

            decimal bid = ParseStringToDecimal(externalRate.ExchangeRateData.Bid);
            decimal ask = ParseStringToDecimal(externalRate.ExchangeRateData.Ask);
            exchangeRate = new ExchangeRate(request.FromCurrencyCode, request.ToCurrencyCode, bid, ask);
            
            await _exchangeRateRepo.CreateAsync(exchangeRate);
            await _exchangeRatePublisher.PublishExchangeRateAddedEvent(request.FromCurrencyCode, request.ToCurrencyCode, bid, ask);
        }

        return _mapper.Map<ExchangeRateViewModel>(exchangeRate);
    }

    private static decimal ParseStringToDecimal(string value)
    {
        try
        {
            value = value.Trim();
            return decimal.Parse(value, CultureInfo.InvariantCulture);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException($"Unable to parse '{value}' as a decimal.", ex);
        }
    }
}

