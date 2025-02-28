using FluentValidation;

namespace ExchangeRatesManager.Application.Features.ExchangeRates.Commands;

public class AddExchangeRateCommandValidator : AbstractValidator<AddExchangeRateCommand>
{
    public AddExchangeRateCommandValidator()
    {
        RuleFor(x => x.FromCurrencyCode)
            .NotEmpty().WithMessage("FromCurrencyCode is required.")
            .Length(3).WithMessage("Currency codes must be 3 characters long.")
            .Matches("^[A-Z]{3}$").WithMessage("FromCurrencyCode must be uppercase letters (example: 'USD').");

        RuleFor(x => x.ToCurrencyCode)
            .NotEmpty().WithMessage("ToCurrencyCode is required.")
            .Length(3).WithMessage("Currency codes must be 3 characters long.")
            .Matches("^[A-Z]{3}$").WithMessage("ToCurrencyCode must be uppercase letters (example: 'EUR').")
            .NotEqual(x => x.FromCurrencyCode).WithMessage("In an Exchange Rate, currencies must be different.");

        RuleFor(x => x.Bid).GreaterThan(0).WithMessage("Bid price must be greater than zero.");

        RuleFor(x => x.Ask).GreaterThan(x => x.Bid).WithMessage("Ask price must be greater than Bid price.");
    }
}
