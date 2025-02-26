using MediatR;

namespace ExchangeRatesManager.Application.Commands;

public class UpdateExchangeRateCommand : IRequest<Unit>
{
    public Guid Id { get; }
    public decimal Bid { get; }
    public decimal Ask { get; }

    public UpdateExchangeRateCommand(Guid id, decimal bid, decimal ask)
    {
        Id = id;
        Bid = bid;
        Ask = ask;
    }
}
