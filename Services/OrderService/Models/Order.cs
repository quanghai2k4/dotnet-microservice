namespace OrderService.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = "Pending";
    public string PaymentStatus { get; set; } = "Unpaid";
    public string? PaymentMethod { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? TransactionId { get; set; }
}

public class CreateOrderRequest
{
    public int UserId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class OrderCreatedEvent
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserCreatedEvent
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PaymentRequest
{
    public string PaymentMethod { get; set; } = string.Empty;
    public bool SimulateFailure { get; set; } = false;
}

public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class PaymentCompletedEvent
{
    public int OrderId { get; set; }
    public bool IsSuccess { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}