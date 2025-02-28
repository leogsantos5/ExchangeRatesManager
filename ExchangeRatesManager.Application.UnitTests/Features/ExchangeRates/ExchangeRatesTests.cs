using AutoMapper;
using ExchangeRatesManager.Application.Exceptions;
using ExchangeRatesManager.Application.Features.ExchangeRates.Commands;
using ExchangeRatesManager.Application.Features.ExchangeRates.Queries;
using ExchangeRatesManager.Application.Services.AlphaVantageAPI;
using ExchangeRatesManager.Application.Services.RabbitMQ;
using ExchangeRatesManager.Domain.Models;
using ExchangeRatesManager.Domain.Repositories;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExchangeRatesManager.Application.UnitTests.Features.ExchangeRates;

public class ExchangeRateCommandHandlersTests
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IExchangeRateRepository> _mockRepo;
    private readonly Mock<IAlphaVantageService> _mockAlphaVantageService;
    private readonly Mock<IExchangeRatePublisher> _mockPublisher;
    private readonly GetExchangeRateQueryHandler _getHandler;
    private readonly AddExchangeRateCommandHandler _addHandler;
    private readonly UpdateExchangeRateCommandHandler _updateHandler;
    private readonly DeleteExchangeRateCommandHandler _deleteHandler;
    private readonly Mock<ILogger<GetExchangeRateQueryHandler>> _mockGetLogger;
    private readonly Mock<ILogger<AddExchangeRateCommandHandler>> _mockAddLogger;
    private readonly Mock<ILogger<UpdateExchangeRateCommandHandler>> _mockUpdateLogger;
    private readonly Mock<ILogger<DeleteExchangeRateCommandHandler>> _mockDeleteLogger;

    public ExchangeRateCommandHandlersTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockRepo = new Mock<IExchangeRateRepository>();

        _mockPublisher = new Mock<IExchangeRatePublisher>();
        _mockAlphaVantageService = new Mock<IAlphaVantageService>();

        _mockAddLogger = new Mock<ILogger<AddExchangeRateCommandHandler>>();
        _mockGetLogger = new Mock<ILogger<GetExchangeRateQueryHandler>>();
        _mockUpdateLogger = new Mock<ILogger<UpdateExchangeRateCommandHandler>>();
        _mockDeleteLogger = new Mock<ILogger<DeleteExchangeRateCommandHandler>>();

        _getHandler = new GetExchangeRateQueryHandler(_mockRepo.Object, _mockAlphaVantageService.Object, _mockGetLogger.Object,
                                                      _mockMapper.Object, new Mock<IConfiguration>().Object, _mockPublisher.Object);
        _addHandler = new AddExchangeRateCommandHandler(_mockAddLogger.Object, _mockRepo.Object, _mockPublisher.Object);
        _updateHandler = new UpdateExchangeRateCommandHandler(_mockUpdateLogger.Object, _mockRepo.Object);
        _deleteHandler = new DeleteExchangeRateCommandHandler(_mockDeleteLogger.Object, _mockRepo.Object);
    }

    [Fact]
    public async Task AddExchangeRateCommandHandler_ShouldCreateExchangeRate()
    {
        var command = new AddExchangeRateCommand("USD", "EUR", 1.20m, 1.25m);

        _mockRepo.Setup(repo => repo.CreateAsync(It.IsAny<ExchangeRate>())).ReturnsAsync(Guid.NewGuid());

        var result = await _addHandler.Handle(command, CancellationToken.None);

        _mockRepo.Verify(repo => repo.CreateAsync(It.IsAny<ExchangeRate>()), Times.Once);
        _mockPublisher.Verify(publisher => publisher.PublishExchangeRateAddedEvent("USD", "EUR", 1.20m, 1.25m), Times.Once);

        Assert.NotEqual(Guid.Empty, result); 
    }

    [Fact]
    public async Task AddExchangeRateCommandHandler_ShouldThrowBadRequestException_WhenValidationFails()
    {
        var invalidCommand = new AddExchangeRateCommand("USD", "EUR", 1.25m, 1.20m); // Bid higher than Ask price

        var validationResult = new ValidationResult(new[] {
            new ValidationFailure("Ask", "Ask price must be greater than Bid price.")
        });
        var mockValidator = new Mock<IValidator<AddExchangeRateCommand>>();
        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<AddExchangeRateCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationResult);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _addHandler.Handle(invalidCommand, CancellationToken.None));

        Assert.Contains("Ask price must be greater than Bid price.", exception.ValidationErrors!);
    }

    [Fact]
    public async Task GetExchangeRateQueryHandler_ShouldReturnExchangeRate_WhenFoundInDatabase()
    {
        var query = new GetExchangeRateQuery("USD", "EUR");
        var exchangeRate = new ExchangeRate("USD", "EUR", 1.20m, 1.25m);

        _mockRepo.Setup(repo => repo.GetByCurrencyPairAsync("USD", "EUR")).ReturnsAsync(exchangeRate);
        _mockMapper.Setup(mapper => mapper.Map<ExchangeRateViewModel>(It.IsAny<ExchangeRate>()))
                   .Returns(new ExchangeRateViewModel { FromCurrency = "USD", ToCurrency = "EUR", Bid = 1.20m, Ask = 1.25m });

        var result = await _getHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1.20m, result.Bid);
        Assert.Equal(1.25m, result.Ask);

        _mockAlphaVantageService.Verify(service => service.GetExchangeRateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetExchangeRateQueryHandler_FetchesFromAlphaVantageAPI_WhenNotFoundInDB()
    {
        var query = new GetExchangeRateQuery("USD", "EUR");

        var externalRate = new AlphaVantageResponse
        {
            ExchangeRateData = new ExchangeRateData { Rate = "1.20", Bid = "1.18", Ask = "1.22" }
        };

        _mockRepo.Setup(repo => repo.GetByCurrencyPairAsync("USD", "EUR")).ReturnsAsync((ExchangeRate?) null);
        _mockAlphaVantageService.Setup(service => service.GetExchangeRateAsync("USD", "EUR", It.IsAny<string>())).ReturnsAsync(externalRate);
        _mockMapper.Setup(mapper => mapper.Map<ExchangeRateViewModel>(It.IsAny<ExchangeRate>()))
                   .Returns(new ExchangeRateViewModel { FromCurrency = "USD", ToCurrency = "EUR", Bid = 1.18m, Ask = 1.22m });

        var result = await _getHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1.18m, result.Bid);
        Assert.Equal(1.22m, result.Ask);

        _mockRepo.Verify(repo => repo.CreateAsync(It.IsAny<ExchangeRate>()), Times.Once);
        _mockPublisher.Verify(publisher => publisher.PublishExchangeRateAddedEvent(It.IsAny<string>(), It.IsAny<string>(),
                                                                                   It.IsAny<decimal>(), It.IsAny<decimal>()), Times.Once);
    }

    [Fact]
    public async Task GetExchangeRateQueryHandler_ShouldThrowBadRequestException_WhenFromCurrencyCodeIsTooShort()
    {
        var query = new GetExchangeRateQuery("US", "EUR"); // From currency only has 2 chars

        var validationResult = new ValidationResult(new[] {
            new ValidationFailure("FromCurrencyCode", "Currency codes must be 3 characters long.") 
        });

        var mockValidator = new Mock<IValidator<GetExchangeRateQuery>>();
        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<GetExchangeRateQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationResult);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _getHandler.Handle(query, CancellationToken.None));

        Assert.Contains("Currency codes must be 3 characters long.", exception.ValidationErrors!);

        _mockRepo.Verify(repo => repo.GetByCurrencyPairAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockAlphaVantageService.Verify(service => service.GetExchangeRateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateExchangeRateCommandHandler_ShouldUpdateExchangeRate()
    {
        var command = new UpdateExchangeRateCommand(Guid.NewGuid(), 1.30m, 1.35m);
        var exchangeRate = new ExchangeRate("USD", "EUR", 1.20m, 1.25m);

        _mockRepo.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync(exchangeRate);

        var result = await _updateHandler.Handle(command, CancellationToken.None);

        _mockRepo.Verify(repo => repo.UpdateAsync(exchangeRate), Times.Once);

        Assert.Equal(Unit.Value, result);
    }

    [Fact]
    public async Task UpdateExchangeRateCommandHandler_ShouldThrowNotFoundException_WhenExchangeRateDoesNotExist()
    {
        var command = new UpdateExchangeRateCommand(Guid.NewGuid(), 1.30m, 1.35m);
        _mockRepo.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync((ExchangeRate?) null);

        await Assert.ThrowsAsync<NotFoundException>(() => _updateHandler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateExchangeRateCommandHandler_ShouldThrowBadRequestException_WhenBidIsLessThanOrEqualToZero()
    {
        var invalidCommand = new UpdateExchangeRateCommand(Guid.NewGuid(), -1.30m, 1.35m); // Bid has a negative value

        var validationResult = new ValidationResult(new[] {
            new ValidationFailure("Bid", "Bid price must be greater than zero.")
        });

        var mockValidator = new Mock<IValidator<UpdateExchangeRateCommand>>();
        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateExchangeRateCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(validationResult);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _updateHandler.Handle(invalidCommand, CancellationToken.None));

        Assert.Contains("Bid price must be greater than zero.", exception.ValidationErrors!);
    }

    [Fact]
    public async Task DeleteExchangeRateCommandHandler_ShouldDeleteExchangeRate()
    {
        var command = new DeleteExchangeRateCommand(Guid.NewGuid());
        var exchangeRate = new ExchangeRate("USD", "EUR", 1.20m, 1.25m);

        _mockRepo.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync(exchangeRate);

        var result = await _deleteHandler.Handle(command, CancellationToken.None);

        _mockRepo.Verify(repo => repo.DeleteAsync(exchangeRate), Times.Once);

        Assert.Equal(Unit.Value, result); 
    }

    [Fact]
    public async Task DeleteExchangeRateCommandHandler_ShouldThrowNotFoundException_WhenExchangeRateDoesNotExist()
    {
        var command = new DeleteExchangeRateCommand(Guid.NewGuid());

        _mockRepo.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync((ExchangeRate?) null);

        await Assert.ThrowsAsync<NotFoundException>(() => _deleteHandler.Handle(command, CancellationToken.None));
    }
}