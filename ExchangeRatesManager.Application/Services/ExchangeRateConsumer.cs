using ExchangeRatesManager.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class ExchangeRateConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IChannel? _channel;

    public ExchangeRateConsumer(IConfiguration configuration)
    {
        _config = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = _config["RabbitMQ:QueueName"]!;
        var hostName = _config["RabbitMQ:Host"]!;

        var factory = new ConnectionFactory { HostName = hostName };
        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken
        );

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var exchangeRate = JsonSerializer.Deserialize<ExchangeRateMessage>(message);

            Console.WriteLine($"📩 New rate received: {exchangeRate?.FromCurrency} to {exchangeRate?.ToCurrency} - Bid: {exchangeRate?.Bid}, Ask: {exchangeRate?.Ask}");

            await _channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken); // Keeps consumer alive
    }
}
