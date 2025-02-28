using FluentValidation;

namespace ExchangeRatesManager.Application.Features.ExchangeRates.Queries;

public class GetExchangeRateQueryValidator : AbstractValidator<GetExchangeRateQuery>
{
    public GetExchangeRateQueryValidator()
    {
        RuleFor(x => x.FromCurrencyCode)
            .NotEmpty().WithMessage("FromCurrencyCode is required.")
            .Length(3).WithMessage("Currency codes must be 3 characters long.")
            .Must(x => x.ToUpper() == x).WithMessage("FromCurrencyCode must be uppercase letters (example: 'USD').");

        RuleFor(x => x.ToCurrencyCode)
            .NotEmpty().WithMessage("ToCurrencyCode is required.")
            .Length(3).WithMessage("Currency codes must be 3 characters long.")
            .NotEqual(x => x.FromCurrencyCode).WithMessage("In an Exchange Rate, currencies must be different.")
            .Must(x => x.ToUpper() == x).WithMessage("FromCurrencyCode must be uppercase letters (example: 'USD').");
    }
}
