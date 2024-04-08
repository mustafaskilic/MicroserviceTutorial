using OrderAPI.Models.Enums;

namespace OrderAPI.Models.Entities
{
    public class Order
    {
        public Order()
        {
            Status = OrderStatus.Suspend;
            CreatedDate = DateTime.Now;
        }
        public Guid Id { get; set; }
        public Guid BuyerId { get; set; }
        public decimal TotalPrice { get; set; }
        public ICollection<OrderItem> Items { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
          
    }
}
