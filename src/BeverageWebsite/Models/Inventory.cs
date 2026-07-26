using System;

namespace BeverageWebsite.Models
{
    public class Inventory
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
