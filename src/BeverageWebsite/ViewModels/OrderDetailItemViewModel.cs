namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents read-only item presentation data for an authenticated customer's order details.
    /// </summary>
    public class OrderDetailItemViewModel
    {
        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the product image URL.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the ordered quantity.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the stored unit price.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the stored item discount amount.
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the stored line total.
        /// </summary>
        public decimal LineTotal { get; set; }
    }
}
