using AutoMapper;
using ExchangeRatesManager.Application.Exceptions;
using ExchangeRatesManager.Application.Features.ExchangeRates.Commands;
using ExchangeRatesManager.Application.Features.ExchangeRates.Queries;
using ExchangeRatesManager.Application.Services.AlphaVantageAPI;
using ExchangeRatesManager.Application.Services.RabbitMQ;
using ExchangeRatesManager.Domain.Models;
using ExchangeRatesManager.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExchangeRatesManager.Application.UnitTests.Features.ExchangeRates;

public class ExchangeRateCommandHandlersTests
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IExchangeRateRepository> _mockRepo;
    private readonly AddExchangingRateCommandHandler _addHandler;
    private readonly DeleteExchangeRateCommandHandler _deleteHandler;
    private readonly UpdateExchangeRateCommandHandler _updateHandler;
    private readonly GetExchangeRateQueryHandler _getHandler;
    private readonly Mock<IAlphaVantageService> _mockAlphaVantageService;
    private readonly Mock<ILogger<IExchangeRatePublisher>> _mockLogger;
    private readonly Mock<IExchangeRatePublisher> _mockPublisher;

    public ExchangeRateCommandHandlersTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockRepo = new Mock<IExchangeRateRepository>();
        _mockLogger = new Mock<ILogger<IExchangeRatePublisher>>();

        _mockPublisher = new Mock<IExchangeRatePublisher>();
        _mockAlphaVantageService = new Mock<IAlphaVantageService>();

        _getHandler = new GetExchangeRateQueryHandler(_mockRepo.Object, _mockAlphaVantageService.Object,
                                                      _mockMapper.Object, new Mock<IConfiguration>().Object, _mockPublisher.Object);

        _addHandler = new AddExchangingRateCommandHandler(_mockRepo.Object, _mockPublisher.Object);
        _updateHandler = new UpdateExchangeRateCommandHandler(_mockRepo.Object);
        _deleteHandler = new DeleteExchangeRateCommandHandler(_mockRepo.Object);
    }

    [Fact]
    public async Task AddExchangingRateCommandHandler_ShouldCreateExchangeRate()
    {
        var command = new AddExchangeRateCommand("USD", "EUR", 1.20m, 1.25m);

        _mockRepo.Setup(repo => repo.CreateAsync(It.IsAny<ExchangeRate>())).ReturnsAsync(Guid.NewGuid());

        var result = await _addHandler.Handle(command, CancellationToken.None);

        _mockRepo.Verify(repo => repo.CreateAsync(It.IsAny<ExchangeRate>()), Times.Once);
        _mockPublisher.Verify(publisher => publisher.PublishExchangeRateAddedEvent("USD", "EUR", 1.20m, 1.25m), Times.Once);

        Assert.NotEqual(Guid.Empty, result); 
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

        _mockRepo.Setup(repo => repo.GetByCurrencyPairAsync("USD", "EUR")).ReturnsAsync((ExchangeRate?) null);

        var externalRate = new AlphaVantageResponse
        {
            ExchangeRateData = new ExchangeRateData { Rate = "1.20", Bid = "1.18", Ask = "1.22" }
        };

        _mockAlphaVantageService.Setup(service => service.GetExchangeRateAsync("USD", "EUR", It.IsAny<string>()))
                                .ReturnsAsync(externalRate);

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
        _mockRepo.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync((ExchangeRate?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _updateHandler.Handle(command, CancellationToken.None));
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