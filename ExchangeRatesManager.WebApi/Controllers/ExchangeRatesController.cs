using ExchangeRatesManager.Application.Commands;
using ExchangeRatesManager.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeRatesManager.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExchangeRatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExchangeRatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> AddExchangeRate([FromBody] AddExchangeRateCommand command)
    {
        var exchangeRateId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetExchangeRate), new { id = exchangeRateId }, null);
    }

    [HttpGet("{fromCurrency}/{toCurrency}")]
    public async Task<ActionResult<ExchangeRateViewModel>> GetExchangeRate(string fromCurrency, string toCurrency)
    {
        var query = new GetExchangeRateQuery(fromCurrency, toCurrency);
        var result = await _mediator.Send(query);
        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
