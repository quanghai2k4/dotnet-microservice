using OrderService.Models;

namespace OrderService.Services;

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(decimal amount, PaymentRequest request);
}

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ILogger<PaymentService> logger)
    {
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, PaymentRequest request)
    {
        _logger.LogInformation("Processing payment simulation for amount: {Amount} with method: {PaymentMethod}", 
            amount, request.PaymentMethod);

        // Simulate processing delay
        await Task.Delay(Random.Shared.Next(500, 2000));

        // Simulate payment failure if requested
        if (request.SimulateFailure)
        {
            _logger.LogWarning("Payment simulation failed as requested");
            return new PaymentResult
            {
                IsSuccess = false,
                TransactionId = string.Empty,
                Message = "Payment failed: Simulated failure"
            };
        }

        // Random failure simulation (5% chance)
        if (Random.Shared.NextDouble() < 0.05)
        {
            _logger.LogWarning("Payment simulation failed randomly");
            return new PaymentResult
            {
                IsSuccess = false,
                TransactionId = string.Empty,
                Message = "Payment failed: Insufficient funds"
            };
        }

        var transactionId = $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{Random.Shared.Next(1000, 9999)}";
        
        _logger.LogInformation("Payment simulation successful with transaction ID: {TransactionId}", transactionId);
        
        return new PaymentResult
        {
            IsSuccess = true,
            TransactionId = transactionId,
            Message = "Payment processed successfully"
        };
    }
}