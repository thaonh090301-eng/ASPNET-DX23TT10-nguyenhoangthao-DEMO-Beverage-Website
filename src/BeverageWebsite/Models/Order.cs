using System;

namespace BeverageWebsite.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int AddressId { get; set; }
        public int? PromotionId { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal FinalAmount { get; set; }
    }
}
