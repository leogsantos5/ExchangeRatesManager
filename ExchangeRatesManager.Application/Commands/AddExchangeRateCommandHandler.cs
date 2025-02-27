﻿using ExchangeRatesManager.Domain.Repositories;
using ExchangeRatesManager.Domain.Models;
using MediatR;
using ExchangeRatesManager.Application.Services.RabbitMQ;

namespace ExchangeRatesManager.Application.Commands;

public class AddExchangingRateCommandHandler : IRequestHandler<AddExchangeRateCommand, Guid>
{
    private readonly IExchangeRateRepository _exchangeRateRepo;
    private ExchangeRatePublisher _exchangeRatePublisher;

    public AddExchangingRateCommandHandler(IExchangeRateRepository repository, ExchangeRatePublisher exchangeRatePublisher)
    {
        _exchangeRateRepo = repository;
        _exchangeRatePublisher = exchangeRatePublisher;
    }

    public async Task<Guid> Handle(AddExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var exchangeRate = new ExchangeRate(request.FromCurrencyCode, request.ToCurrencyCode, request.Bid, request.Ask);
        Guid exchangeRateId = await _exchangeRateRepo.CreateAsync(exchangeRate);

        await _exchangeRatePublisher.PublishExchangeRateAddedEvent(request.FromCurrencyCode, request.ToCurrencyCode, request.Bid, request.Ask);

        return exchangeRateId;
    }
}
