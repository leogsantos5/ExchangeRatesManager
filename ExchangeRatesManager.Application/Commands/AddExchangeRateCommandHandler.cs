using ExchangeRatesManager.Domain.Repositories;
using ExchangeRatesManager.Domain.Models;
using MediatR;

namespace ExchangeRatesManager.Application.Commands;

public class AddExchangingRateCommandHandler : IRequestHandler<AddExchangeRateCommand, Guid>
{
    private readonly IExchangeRateRepository _exchangeRateRepo;

    public AddExchangingRateCommandHandler(IExchangeRateRepository repository)
    {
        _exchangeRateRepo = repository;
    }

    public async Task<Guid> Handle(AddExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var exchangeRate = new ExchangeRate(request.FromCurrencyCode, request.ToCurrencyCode, request.Bid, request.Ask);
        return await _exchangeRateRepo.CreateAsync(exchangeRate);  
    }
}
