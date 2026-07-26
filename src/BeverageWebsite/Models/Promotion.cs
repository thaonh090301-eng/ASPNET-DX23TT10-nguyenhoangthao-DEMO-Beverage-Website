using System;

namespace BeverageWebsite.Models
{
    public class Promotion
    {
        public int PromotionId { get; set; }
        public string PromotionCode { get; set; }
        public string PromotionName { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
