using ExchangeRatesManager.Application.Exceptions;
using ExchangeRatesManager.Domain.Models;
using ExchangeRatesManager.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRatesManager.Application.Commands;

public class DeleteExchangeRateCommandHandler : IRequestHandler<DeleteExchangeRateCommand, Unit>
{
    private readonly IExchangeRateRepository _exchangeRateRepo;

    public DeleteExchangeRateCommandHandler(IExchangeRateRepository repository)
    {
        _exchangeRateRepo = repository;
    }

    public async Task<Unit> Handle(DeleteExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var exchangeRate = await _exchangeRateRepo.GetByIdAsync(request.Id);
        if (exchangeRate == null)
            throw new NotFoundException(nameof(ExchangeRate), request.Id);

        await _exchangeRateRepo.DeleteAsync(exchangeRate);

        return Unit.Value;
    }
}
