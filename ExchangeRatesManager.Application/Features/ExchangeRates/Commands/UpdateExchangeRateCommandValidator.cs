using FluentValidation;

namespace ExchangeRatesManager.Application.Features.ExchangeRates.Commands;

public class UpdateExchangeRateCommandValidator : AbstractValidator<UpdateExchangeRateCommand>
{
    public UpdateExchangeRateCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Exchange rate ID is required.");

        RuleFor(x => x.Bid).GreaterThan(0).WithMessage("Bid price must be greater than zero.");

        RuleFor(x => x.Ask).GreaterThan(0).WithMessage("Ask price must be greater than zero.");

        RuleFor(x => x).Must(x => x.Ask > x.Bid).WithMessage("Ask price must be greater than Bid price.");
    }
}
