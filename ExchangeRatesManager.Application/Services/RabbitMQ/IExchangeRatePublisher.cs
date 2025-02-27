namespace ExchangeRatesManager.Application.Services.RabbitMQ;

public interface IExchangeRatePublisher
{
    Task PublishExchangeRateAddedEvent(string currencyFrom, string currencyTo, decimal bid, decimal ask);
}
