﻿using ExchangeRatesManager.Application.Commands;
using ExchangeRatesManager.Application.Exceptions;
using ExchangeRatesManager.Domain.Models;
using ExchangeRatesManager.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UpdateExchangingRateCommandHandler : IRequestHandler<UpdateExchangeRateCommand, Unit>
{
    private readonly IExchangeRateRepository _exchangeRateRepo;

    public UpdateExchangingRateCommandHandler(IExchangeRateRepository repository)
    {
        _exchangeRateRepo = repository;
    }

    public async Task<Unit> Handle(UpdateExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var exchangeRate = await _exchangeRateRepo.GetByIdAsync(request.Id);
        if (exchangeRate == null)
            throw new NotFoundException(nameof(ExchangeRate), request.Id);

        exchangeRate.Bid = request.Bid;
        exchangeRate.Ask = request.Ask;

        await _exchangeRateRepo.UpdateAsync(exchangeRate);

        return Unit.Value;
    }
}
