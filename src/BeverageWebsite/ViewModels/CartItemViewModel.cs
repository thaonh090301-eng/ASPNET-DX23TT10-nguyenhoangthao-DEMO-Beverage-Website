namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents a cart item for presentation.
    /// </summary>
    public class CartItemViewModel
    {
        /// <summary>
        /// Gets or sets the cart-item identifier.
        /// </summary>
        public int CartItemId { get; set; }

        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the product image URL.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the item quantity.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets the total price for this cart item.
        /// </summary>
        public decimal LineTotal
        {
            get
            {
                return Quantity * UnitPrice;
            }
        }
    }
}
