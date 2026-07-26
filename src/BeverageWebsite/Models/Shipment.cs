using System;

namespace BeverageWebsite.Models
{
    public class Shipment
    {
        public int ShipmentId { get; set; }
        public int OrderId { get; set; }
        public string ShippingProvider { get; set; }
        public string TrackingNumber { get; set; }
        public string ShipmentStatus { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }
}
