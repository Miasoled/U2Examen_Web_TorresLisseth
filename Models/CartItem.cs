namespace NorthwindApp.Models
{
    public class CartItem
    {
        public short ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public float UnitPrice { get; set; }
        public int Quantity { get; set; }
        public float Subtotal => UnitPrice * Quantity;
    }
}