using Microsoft.Extensions.DependencyInjection;

namespace Common.RabbitMQ;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services)
    {
        services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();
        services.AddScoped<IMessagePublisher, RabbitMQService>();
        services.AddScoped<IMessageConsumer, RabbitMQService>();
        services.AddScoped<RabbitMQService>();
        
        return services;
    }
}