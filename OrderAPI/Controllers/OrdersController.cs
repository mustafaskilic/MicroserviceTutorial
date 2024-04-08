using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.Models;
using OrderAPI.Models.Entities;
using OrderAPI.ViewModels;
using Shared.Events;

namespace OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        readonly OrderAPIDbContext _context;
        readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(OrderAPIDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderVM createOrder)
        {
            Order order = new()
            {
                Id = Guid.NewGuid(),
                BuyerId = createOrder.BuyerId,
            };
            order.Items = createOrder.Items.Select(e => new OrderItem { Count = e.Count, Price = e.Price, ProductId = e.ProductId }).ToList();
            order.TotalPrice = createOrder.Items.Sum(e => e.Price * e.Count);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            OrderCreatedEvent orderCreatedEvent = new()
            {
                BuyerId = order.BuyerId,
                Id = order.Id,
                OrderItems = order.Items.Select(e => new Shared.Messages.OrderItemMessage() { ProductId = e.ProductId, Count = e.Count}).ToList(),
                TotalPrice = order.TotalPrice,
            };

            await _publishEndpoint.Publish(orderCreatedEvent);

            return Ok();
        }
    }
}
