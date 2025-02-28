using ExchangeRatesManager.Application.Exceptions;
using ExchangeRatesManager.Domain.Models;
using ExchangeRatesManager.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ExchangeRatesManager.Application.Features.ExchangeRates.Commands;

public class DeleteExchangeRateCommandHandler : IRequestHandler<DeleteExchangeRateCommand, Unit>
{
    private readonly ILogger<DeleteExchangeRateCommandHandler> _logger;
    private readonly IExchangeRateRepository _exchangeRateRepo;

    public DeleteExchangeRateCommandHandler(ILogger<DeleteExchangeRateCommandHandler> logger, IExchangeRateRepository exchangeRateRepository)
    {
        _logger = logger;
        _exchangeRateRepo = exchangeRateRepository;
    }

    public async Task<Unit> Handle(DeleteExchangeRateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[HANDLER] Handling DeleteExchangeRateCommand for ID {Id}", request.Id);

        var exchangeRate = await _exchangeRateRepo.GetByIdAsync(request.Id);
        if (exchangeRate == null)
            throw new NotFoundException(nameof(ExchangeRate), request.Id);

        await _exchangeRateRepo.DeleteAsync(exchangeRate);

        _logger.LogInformation("[HANDLER] ExchangeRate with ID {Id} deleted successfully", request.Id);

        return Unit.Value;
    }
}
