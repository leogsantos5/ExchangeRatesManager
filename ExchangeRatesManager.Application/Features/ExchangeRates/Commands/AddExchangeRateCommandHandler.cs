using ExchangeRatesManager.Domain.Repositories;
using ExchangeRatesManager.Domain.Models;
using MediatR;
using ExchangeRatesManager.Application.Services.RabbitMQ;
using ExchangeRatesManager.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace ExchangeRatesManager.Application.Features.ExchangeRates.Commands;

public class AddExchangeRateCommandHandler : IRequestHandler<AddExchangeRateCommand, Guid>
{
    private readonly ILogger<AddExchangeRateCommandHandler> _logger;
    private readonly IExchangeRateRepository _exchangeRateRepo;
    private readonly IExchangeRatePublisher _exchangeRatePublisher;

    public AddExchangeRateCommandHandler(ILogger<AddExchangeRateCommandHandler> logger, IExchangeRateRepository exchangeRateRepository, IExchangeRatePublisher exchangeRatePublisher)
    {
        _logger = logger;
        _exchangeRateRepo = exchangeRateRepository;
        _exchangeRatePublisher = exchangeRatePublisher;
    }

    public async Task<Guid> Handle(AddExchangeRateCommand request, CancellationToken cancellationToken)
    {
        string fromCurrencyCode = request.FromCurrencyCode;
        string toCurrencyCode = request.ToCurrencyCode;

        _logger.LogInformation("[HANDLER] Handling AddExchangeRateCommand for {From} -> {To} with Bid: {Bid}, Ask: {Ask}",
                               fromCurrencyCode, toCurrencyCode, request.Bid, request.Ask);

        var validator = new AddExchangeRateCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.Errors.Count != 0)
            throw new BadRequestException(validationResult);

        var exchangeRate = new ExchangeRate(fromCurrencyCode, toCurrencyCode, request.Bid, request.Ask);
        Guid exchangeRateId = await _exchangeRateRepo.CreateAsync(exchangeRate);

        _logger.LogInformation("[HANDLER] Exchange rate created successfully with ID {ExchangeRateId}", exchangeRateId);

        await _exchangeRatePublisher.PublishExchangeRateAddedEvent(fromCurrencyCode, toCurrencyCode, request.Bid, request.Ask);

        return exchangeRateId;
    }
}
