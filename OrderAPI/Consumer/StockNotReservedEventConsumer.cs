using MassTransit;
using OrderAPI.Models.Entities;
using OrderAPI.Models;
using Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace OrderAPI.Consumer
{
    public class StockNotReservedEventConsumer : IConsumer<StockNotReservedEvent>
    {
        readonly OrderAPIDbContext _orderAPIDbContext;

        public StockNotReservedEventConsumer(OrderAPIDbContext orderAPIDbContext)
        {
            _orderAPIDbContext = orderAPIDbContext;
        }

        public async Task Consume(ConsumeContext<StockNotReservedEvent> context)
        {
            Order? order = await _orderAPIDbContext.Orders.FirstOrDefaultAsync(o => o.Id == context.Message.OrderId);
            order.Status = Models.Enums.OrderStatus.Failed;

            await _orderAPIDbContext.SaveChangesAsync();
        }
    }
}
