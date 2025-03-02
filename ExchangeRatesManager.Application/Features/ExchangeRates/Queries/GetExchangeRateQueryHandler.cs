using AutoMapper;
using ExchangeRatesManager.Application.Exceptions;
using ExchangeRatesManager.Application.Services.AlphaVantageAPI;
using ExchangeRatesManager.Application.Services.RabbitMQ;
using ExchangeRatesManager.Domain.Models;
using ExchangeRatesManager.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ExchangeRatesManager.Application.Features.ExchangeRates.Queries;

public class GetExchangeRateQueryHandler : IRequestHandler<GetExchangeRateQuery, ExchangeRateViewModel>
{
    private readonly IExchangeRateRepository _exchangeRateRepo;
    private readonly IAlphaVantageService _alphaVantageService;
    private readonly ILogger<GetExchangeRateQueryHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private readonly IExchangeRatePublisher _exchangeRatePublisher;

    public GetExchangeRateQueryHandler(IExchangeRateRepository exchangeRateRepository, IAlphaVantageService alphaVantageService,
                                       ILogger<GetExchangeRateQueryHandler> logger, IMapper mapper, IConfiguration config, IExchangeRatePublisher exchangeRatePublisher)
    {
        _exchangeRateRepo = exchangeRateRepository;
        _alphaVantageService = alphaVantageService;
        _logger = logger;
        _mapper = mapper;
        _config = config;
        _exchangeRatePublisher = exchangeRatePublisher;
    }

    public async Task<ExchangeRateViewModel> Handle(GetExchangeRateQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetExchangeRateQueryValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.Errors.Count != 0)
            throw new BadRequestException(validationResult);

        string fromCurrencyCode = request.FromCurrencyCode; 
        string toCurrencyCode = request.ToCurrencyCode;

        _logger.LogInformation("[HANDLER] Handling GetExchangeRateQuery for {From} -> {To}", fromCurrencyCode, toCurrencyCode);

        var exchangeRate = await _exchangeRateRepo.GetByCurrencyPairAsync(fromCurrencyCode, toCurrencyCode);
        if (exchangeRate == null)
        {
            _logger.LogWarning("[HANDLER] ExchangeRate not found in DB for {From} -> {To}. Fetching from AlphaVantage API...",
                                                                            fromCurrencyCode, toCurrencyCode);
            var apiKey = _config["AlphaVantage:ApiKey"]!;

            var externalRate = await _alphaVantageService.GetExchangeRateAsync(fromCurrencyCode, toCurrencyCode, apiKey);
            if (externalRate == null || externalRate.ExchangeRateData == null)
                throw new NotFoundException(nameof(ExchangeRate), new { fromCurrencyCode, toCurrencyCode });

            decimal bid = ParseStringToDecimal(externalRate.ExchangeRateData.Bid);
            decimal ask = ParseStringToDecimal(externalRate.ExchangeRateData.Ask);
            exchangeRate = new ExchangeRate(fromCurrencyCode, request.ToCurrencyCode, bid, ask);

            await _exchangeRateRepo.CreateAsync(exchangeRate);

            _logger.LogInformation("[HANDLER] Fetched and saved ExchangeRate from AlphaVantage API for {From} -> {To} with Bid: {Bid}, Ask: {Ask}",
                                                                                                      fromCurrencyCode, toCurrencyCode, bid, ask);

            await _exchangeRatePublisher.PublishExchangeRateAddedEvent(fromCurrencyCode, toCurrencyCode, bid, ask);
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

