using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Common.RabbitMQ;

public interface IRabbitMQConnection
{
    IConnection Connection { get; }
    IChannel CreateChannel();
}

public class RabbitMQConnection : IRabbitMQConnection, IDisposable
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMQConnection> _logger;

    public RabbitMQConnection(IConfiguration configuration, ILogger<RabbitMQConnection> logger)
    {
        _logger = logger;
        
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:UserName"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest",
            VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        try
        {
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _logger.LogInformation("RabbitMQ connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish RabbitMQ connection");
            throw;
        }
    }

    public IConnection Connection => _connection;

    public IChannel CreateChannel()
    {
        return _connection.CreateChannelAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _connection?.CloseAsync().GetAwaiter().GetResult();
        _connection?.Dispose();
    }
}