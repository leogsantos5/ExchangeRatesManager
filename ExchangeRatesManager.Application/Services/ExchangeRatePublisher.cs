using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;

namespace ExchangeRatesManager.Application.Services;

public class ExchangeRatePublisher(IConfiguration config) 
{
    private readonly IConfiguration _config = config;

    public async Task PublishExchangeRateAddedEvent(string currencyFrom, string currencyTo, decimal bid, decimal ask)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"]!,
            UserName = _config["RabbitMQ:Username"]!,
            Password = _config["RabbitMQ:Password"]!
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        string queueName = _config["RabbitMQ:QueueName"]!;

        await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false,
                                        autoDelete: false, arguments: null, passive: true);

        var message = $"New rate added: From: {currencyFrom}, to: {currencyTo} - Bid: {bid}, Ask: {ask}";
        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body );
    }
}

