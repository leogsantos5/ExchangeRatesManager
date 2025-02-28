using ExchangeRatesManager.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeRatesManager.WebApi.Middleware;

internal sealed class BadRequestExceptionHandler : IExceptionHandler
{
    private readonly ILogger<BadRequestExceptionHandler> _logger;

    public BadRequestExceptionHandler(ILogger<BadRequestExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BadRequestException badRequestException)
            return false;

        var formattedErrorsString = string.Empty;
        if (badRequestException.ValidationErrors != null && badRequestException.ValidationErrors.Count != 0)
            formattedErrorsString = string.Join("; ", badRequestException.ValidationErrors.Select((error, index) => $"Error {index + 1}: {error}"));
        else
            formattedErrorsString += exception.Message;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = formattedErrorsString
        };

        _logger.LogError("[EXCEPTION] - BadRequestException: {ErrorMessage}", formattedErrorsString);

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
