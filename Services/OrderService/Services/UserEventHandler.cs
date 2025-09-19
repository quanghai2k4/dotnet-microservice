using OrderService.Models;
using Common.RabbitMQ;

namespace OrderService.Services;

public class UserEventHandler : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserEventHandler> _logger;

    public UserEventHandler(IServiceProvider serviceProvider, ILogger<UserEventHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var messageConsumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();

        await messageConsumer.ConsumeAsync<UserCreatedEvent>(
            "user.events", 
            "user.created", 
            "order.service.user.created",
            async (userCreatedEvent) =>
            {
                _logger.LogInformation("Received UserCreatedEvent for UserId: {UserId}", userCreatedEvent.UserId);
            });
    }
}