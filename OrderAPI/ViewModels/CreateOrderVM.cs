namespace OrderAPI.ViewModels
{
    public class CreateOrderVM
    {
        public Guid BuyerId { get; set; }
        public List<CreateOrderItemVM>? Items { get; set; }
    }
}
