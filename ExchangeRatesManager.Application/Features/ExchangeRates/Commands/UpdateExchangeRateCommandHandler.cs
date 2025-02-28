using ExchangeRatesManager.Application.Exceptions;
using ExchangeRatesManager.Application.Features.ExchangeRates.Commands;
using ExchangeRatesManager.Domain.Models;
using ExchangeRatesManager.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

public class UpdateExchangeRateCommandHandler : IRequestHandler<UpdateExchangeRateCommand, Unit>
{
    private readonly ILogger<UpdateExchangeRateCommandHandler> _logger;
    private readonly IExchangeRateRepository _exchangeRateRepo;

    public UpdateExchangeRateCommandHandler(ILogger<UpdateExchangeRateCommandHandler> logger, IExchangeRateRepository exchangeRateRepository)
    {
        _logger = logger;
        _exchangeRateRepo = exchangeRateRepository;
    }

    public async Task<Unit> Handle(UpdateExchangeRateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[HANDLER] Handling UpdateExchangeRateCommand for ID {Id} with Bid: {Bid}, Ask: {Ask}", 
                                                                               request.Id, request.Bid, request.Ask);

        var validator = new UpdateExchangeRateCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.Errors.Count != 0)
            throw new BadRequestException(validationResult);

        var exchangeRate = await _exchangeRateRepo.GetByIdAsync(request.Id);
        if (exchangeRate == null)
            throw new NotFoundException(nameof(ExchangeRate), request.Id);

        exchangeRate.Bid = request.Bid;
        exchangeRate.Ask = request.Ask;

        await _exchangeRateRepo.UpdateAsync(exchangeRate);

        _logger.LogInformation("[HANDLER] ExchangeRate with ID {Id} updated successfully", request.Id);

        return Unit.Value;
    }
}
