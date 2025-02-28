using FluentValidation.Results;

namespace ExchangeRatesManager.Application.Exceptions;

public class BadRequestException : Exception
{
    public List<string>? ValidationErrors { get; set; }

    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(ValidationResult validationResult)
    {
        ValidationErrors = [];
        foreach (var error in validationResult.Errors)
            ValidationErrors.Add(error.ErrorMessage);
    }
}
