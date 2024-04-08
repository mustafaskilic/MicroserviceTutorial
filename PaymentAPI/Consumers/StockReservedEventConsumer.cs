using MassTransit;
using Shared.Events;

namespace PaymentAPI.Consumers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        readonly IPublishEndpoint _publishEndpoint;

        public StockReservedEventConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            if (true)
            {
                PaymentCompletedEvent paymentCompletedEvent = new()
                {
                    OrderId = context.Message.OrderId,
                };

                _publishEndpoint.Publish(paymentCompletedEvent);

                Console.Out.WriteLine("Odeme basarili");
            }
            else
            {
                PaymentFailedEvent paymentFailedEvent = new()
                {
                    OrderId = context.Message.OrderId,
                    Message = "Odeme esnasinda hata olustu"
                };

                _publishEndpoint.Publish(paymentFailedEvent);

                Console.Out.WriteLine("Odeme basarisiz");
            }
            return Task.CompletedTask;
        }
    }
}
