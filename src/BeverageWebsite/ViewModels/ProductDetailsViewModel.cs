namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents public product details and current availability.
    /// </summary>
    public class ProductDetailsViewModel
    {
        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the product description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the product price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the product image URL.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the available stock quantity.
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Gets a value indicating whether the product is currently in stock.
        /// </summary>
        public bool IsInStock
        {
            get { return StockQuantity > 0; }
        }
    }
}
