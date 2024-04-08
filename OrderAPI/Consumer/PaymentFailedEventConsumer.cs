using MassTransit;
using OrderAPI.Models.Entities;
using OrderAPI.Models;
using Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace OrderAPI.Consumer
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        readonly OrderAPIDbContext _orderAPIDbContext;

        public PaymentFailedEventConsumer(OrderAPIDbContext orderAPIDbContext)
        {
            _orderAPIDbContext = orderAPIDbContext;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            Order? order = await _orderAPIDbContext.Orders.FirstOrDefaultAsync(o => o.Id == context.Message.OrderId);
            order.Status = Models.Enums.OrderStatus.Failed;

            await _orderAPIDbContext.SaveChangesAsync();
        }
    }
}
