using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Common.RabbitMQ;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string exchange, string routingKey, T message);
    Task PublishAsync<T>(string queueName, T message);
}

public interface IMessageConsumer
{
    Task ConsumeAsync<T>(string queueName, Func<T, Task> handler);
    Task ConsumeAsync<T>(string exchange, string routingKey, string queueName, Func<T, Task> handler);
}

public class RabbitMQService : IMessagePublisher, IMessageConsumer, IDisposable
{
    private readonly IRabbitMQConnection _connection;
    private readonly ILogger<RabbitMQService> _logger;
    private IChannel? _channel;

    public RabbitMQService(IRabbitMQConnection connection, ILogger<RabbitMQService> logger)
    {
        _connection = connection;
        _logger = logger;
        _channel = _connection.CreateChannel();
    }

    public async Task PublishAsync<T>(string exchange, string routingKey, T message)
    {
        try
        {
            await _channel!.ExchangeDeclareAsync(exchange, ExchangeType.Direct, durable: true);
            
            var messageJson = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(messageJson);

            var properties = new BasicProperties
            {
                Persistent = true,
                MessageId = Guid.NewGuid().ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await _channel.BasicPublishAsync(exchange, routingKey, false, properties, body);

            _logger.LogInformation("Message published to exchange {Exchange} with routing key {RoutingKey}", 
                exchange, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to exchange {Exchange}", exchange);
            throw;
        }
    }

    public async Task PublishAsync<T>(string queueName, T message)
    {
        try
        {
            await _channel!.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            
            var messageJson = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(messageJson);

            var properties = new BasicProperties
            {
                Persistent = true,
                MessageId = Guid.NewGuid().ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await _channel.BasicPublishAsync(string.Empty, queueName, false, properties, body);

            _logger.LogInformation("Message published to queue {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to queue {QueueName}", queueName);
            throw;
        }
    }

    public async Task ConsumeAsync<T>(string queueName, Func<T, Task> handler)
    {
        try
        {
            await _channel!.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(messageJson);

                    if (message != null)
                    {
                        await handler(message);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        _logger.LogInformation("Message processed successfully from queue {QueueName}", queueName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue {QueueName}", queueName);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
            _logger.LogInformation("Started consuming messages from queue {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start consuming from queue {QueueName}", queueName);
            throw;
        }
    }

    public async Task ConsumeAsync<T>(string exchange, string routingKey, string queueName, Func<T, Task> handler)
    {
        try
        {
            await _channel!.ExchangeDeclareAsync(exchange, ExchangeType.Direct, durable: true);
            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            await _channel.QueueBindAsync(queue: queueName, exchange: exchange, routingKey: routingKey);
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(messageJson);

                    if (message != null)
                    {
                        await handler(message);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        _logger.LogInformation("Message processed successfully from exchange {Exchange}", exchange);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from exchange {Exchange}", exchange);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
            _logger.LogInformation("Started consuming messages from exchange {Exchange} with routing key {RoutingKey}", 
                exchange, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start consuming from exchange {Exchange}", exchange);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        _channel?.Dispose();
    }
}