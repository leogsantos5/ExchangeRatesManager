using MediatR;

namespace ExchangeRatesManager.Application.Features.ExchangeRates.Commands;

public class DeleteExchangeRateCommand : IRequest<Unit>
{
    public Guid Id { get; set; }

    public DeleteExchangeRateCommand(Guid id)
    {
        Id = id;
    }
}

