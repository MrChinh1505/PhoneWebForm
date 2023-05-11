namespace MobilePhone.Models
{
    [Serializable]
    public class CartItem
    {
        public string? productID { get; set; }
        public string? productName { get; set; }
        public int? price { get; set; }
        public int? quantity { get; set; }
    }

}
