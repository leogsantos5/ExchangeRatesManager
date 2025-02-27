using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ExchangeRatesManager.Application.Services.RabbitMQ;

public class ExchangeRatePublisher(IConfiguration config, ILogger<ExchangeRatePublisher> logger)
{
    private readonly IConfiguration _config = config;
    private readonly ILogger<ExchangeRatePublisher> _logger = logger;

    public async Task PublishExchangeRateAddedEvent(string currencyFrom, string currencyTo, decimal bid, decimal ask)
    {
        string queueName = _config["RabbitMQ:QueueName"]!;
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"]!,
            UserName = _config["RabbitMQ:Username"]!,
            Password = _config["RabbitMQ:Password"]!
        };

        _logger.LogInformation("📤 [PUBLISHER] Connecting to RabbitMQ: {HostName}, Queue: {QueueName}", factory.HostName, queueName);

        try
        {
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false,
                                            autoDelete: false, arguments: null, passive: false);

            var exchangeRateMessage = new ExchangeRateMessage { FromCurrency = currencyFrom, ToCurrency = currencyTo,
                                                                Bid = bid, Ask = ask };

            var jsonMessage = JsonSerializer.Serialize(exchangeRateMessage);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body);

            _logger.LogInformation("✅ [PUBLISHER] Message published to queue: {QueueName}, From: {FromCurrency}, To: {ToCurrency}, Bid: {Bid}, Ask: {Ask}",
                                   queueName, currencyFrom, currencyTo, bid, ask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [PUBLISHER] Error publishing message to RabbitMQ.");
        }
    }
}

