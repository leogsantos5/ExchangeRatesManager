using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ExchangeRatesManager.Application.Services.RabbitMQ;

public class ExchangeRateConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<ExchangeRateConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public ExchangeRateConsumer(IConfiguration configuration, ILogger<ExchangeRateConsumer> logger)
    {
        _config = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = _config["RabbitMQ:QueueName"]!;
        var hostName = _config["RabbitMQ:Host"]!;

        var factory = new ConnectionFactory { HostName = hostName };
        try
        {
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false,
                                             autoDelete: false, arguments: null);

            _logger.LogInformation("🐇 [CONSUMER] RabbitMQ Consumer started...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [CONSUMER] Failed to connect to RabbitMQ. The API will continue running without RabbitMQ.");
            return; 
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, eventArgs) =>
        {
            try
            {
                _logger.LogInformation("🔄 [CONSUMER] Received a message from RabbitMQ...");

                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var exchangeRate = JsonSerializer.Deserialize<ExchangeRateMessage>(message);
                if (exchangeRate == null)
                    throw new Exception("❌ [CONSUMER] Deserialization resulted in null.");

                _logger.LogInformation("📩 [CONSUMER] New rate received: {FromCurrency} to {ToCurrency} - Bid: {Bid}, Ask: {Ask}",
                                       exchangeRate?.FromCurrency, exchangeRate?.ToCurrency, exchangeRate?.Bid, exchangeRate?.Ask);

                // Note: At first, to properly test Message Queuing behaviour, try changing the ConsumerAck value 
                // on the RabbitMQ section of the appsettings.json file to "false", temporarily.
                // On RabbitMQ UI, on Queues and Streams section, select the queue and you'll see the Queued messages graph
                // and the Message rates graph. When it is "false", you'll see the Queued messages graph changing 
                // when a new ExchangeRate is added. This is due to the message not being ACKed and therefore, becoming queued.
                // Stop the API. Then, set it to "true", run the API again. Consequently, the consumer starts as well.
                // The previously qeued message is ACKed right away and the Queued messages graph goes back to 0.
                // Link for the ExchangeRateQueue RabbitMQ UI: //localhost:15672/#/queues/%2F/exchangeRateQueue ⬇️⬇️⬇️

                if (bool.Parse(_config["RabbitMQ:ConsumerAck"]!)) // Change this flag to "false" to test the message queuing behaviour
                {
                    await _channel.BasicAckAsync(eventArgs.DeliveryTag, false);
                    _logger.LogInformation("✅ [CONSUMER] Message acknowledged with tag: {DeliveryTag}", eventArgs.DeliveryTag);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [CONSUMER] Error processing RabbitMQ message.");
            }
        };

        _logger.LogInformation("✅ [CONSUMER] Listening to queue: {QueueName}", queueName);

        await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

        await Task.Delay(Timeout.Infinite); 
    }
}
