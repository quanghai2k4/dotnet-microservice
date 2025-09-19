using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Models;
using OrderService.Data;
using OrderService.Services;
using Common.RabbitMQ;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _context;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<OrdersController> _logger;
    private readonly IPaymentService _paymentService;

    public OrdersController(OrderDbContext context, IMessagePublisher messagePublisher, ILogger<OrdersController> logger, IPaymentService paymentService)
    {
        _context = context;
        _messagePublisher = messagePublisher;
        _logger = logger;
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        return await _context.Orders.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }
        return order;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByUserId(int userId)
    {
        var orders = await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
        return orders;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CreateOrderRequest request)
    {
        var order = new Order
        {
            UserId = request.UserId,
            ProductName = request.ProductName,
            Quantity = request.Quantity,
            Price = request.Price,
            CreatedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            UserId = order.UserId,
            ProductName = order.ProductName,
            Quantity = order.Quantity,
            Price = order.Price,
            CreatedAt = order.CreatedAt
        };

        await _messagePublisher.PublishAsync("order.events", "order.created", orderCreatedEvent);
        
        _logger.LogInformation("Order created with ID: {OrderId}", order.Id);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = status;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Order {OrderId} status updated to {Status}", id, status);
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/payment")]
    public async Task<ActionResult<PaymentResult>> ProcessPayment(int id, PaymentRequest request)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        if (order.PaymentStatus == "Paid")
        {
            return BadRequest("Order is already paid");
        }

        _logger.LogInformation("Processing payment for Order {OrderId} with method {PaymentMethod}", id, request.PaymentMethod);

        var paymentResult = await _paymentService.ProcessPaymentAsync(order.Price, request);

        if (paymentResult.IsSuccess)
        {
            order.PaymentStatus = "Paid";
            order.PaymentMethod = request.PaymentMethod;
            order.PaidAt = DateTime.UtcNow;
            order.TransactionId = paymentResult.TransactionId;
            order.Status = "Paid";
        }
        else
        {
            order.PaymentStatus = "Failed";
            order.PaymentMethod = request.PaymentMethod;
        }

        await _context.SaveChangesAsync();

        var paymentEvent = new PaymentCompletedEvent
        {
            OrderId = order.Id,
            IsSuccess = paymentResult.IsSuccess,
            PaymentMethod = request.PaymentMethod,
            TransactionId = paymentResult.TransactionId,
            ProcessedAt = DateTime.UtcNow
        };

        await _messagePublisher.PublishAsync("payment.events", paymentResult.IsSuccess ? "payment.completed" : "payment.failed", paymentEvent);

        _logger.LogInformation("Payment {Status} for Order {OrderId}", paymentResult.IsSuccess ? "completed" : "failed", id);

        return paymentResult;
    }
}