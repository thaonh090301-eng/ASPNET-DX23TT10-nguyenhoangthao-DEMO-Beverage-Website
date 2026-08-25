namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents read-only item presentation data for an order viewed by an administrator.
    /// </summary>
    public class AdminOrderDetailItemViewModel
    {
        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the current product display name.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the current product image URL.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the quantity stored on the order item.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price stored on the order item.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the discount amount stored on the order item.
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the line total stored on the order item.
        /// </summary>
        public decimal LineTotal { get; set; }
    }
}
