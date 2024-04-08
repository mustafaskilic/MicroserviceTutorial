using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Shared.Messages;
using StockAPI.Models.Entities;
using StockAPI.Services;

namespace StockAPI.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    { 
        IMongoCollection<Stock> _stockCollection;
        readonly ISendEndpointProvider _sendEndpointProvider;
        readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedEventConsumer(MongoDBService mongoDBService, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
        {
            _stockCollection = mongoDBService.GetCollection<Stock>();
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResult = new();
            foreach (OrderItemMessage orderItem in context.Message.OrderItems)
            {
                stockResult.Add((await _stockCollection.FindAsync(s => s.ProductId == orderItem.ProductId && s.Count >= orderItem.Count)).Any());
            }

            if(stockResult.TrueForAll(sr => sr))
            {
                //Gerekli sipariş işlemleri
                foreach (OrderItemMessage orderItem in context.Message.OrderItems)
                {
                   Stock stock = await (await _stockCollection.FindAsync(s=> s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                    stock.Count -= orderItem.Count;
                    await _stockCollection.FindOneAndReplaceAsync(s => s.ProductId == orderItem.ProductId, stock);
                }

                //Payment ...
                StockReservedEvent stockReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.Id,
                    TotalPrice = context.Message.TotalPrice
                };

               ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Payment_StockReservedEventQueue}"));
               await sendEndpoint.Send(stockReservedEvent);

                Console.Out.WriteLine("Stok islemleri basarili");
            }
            else
            {
                //Siparis tutarsiz/gecersiz
                StockNotReservedEvent stockNotReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.Id,
                    Message = "Siparise uygun stok bulunamadi"
                };

                await _publishEndpoint.Publish(stockNotReservedEvent);

                Console.Out.WriteLine("Stok islemleri basarisiz");
            }

             await Task.CompletedTask;
        }
    }
}
