using System;

namespace BeverageWebsite.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime? PaidAt { get; set; }
        public string TransactionReference { get; set; }
    }
}
