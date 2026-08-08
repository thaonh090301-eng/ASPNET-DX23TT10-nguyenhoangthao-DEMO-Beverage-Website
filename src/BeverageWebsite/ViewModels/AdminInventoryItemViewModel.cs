namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents an active product and its inventory on the administration page.
    /// </summary>
    public class AdminInventoryItemViewModel
    {
        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the product price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the current stock quantity.
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Gets or sets the stock reorder level.
        /// </summary>
        public int ReorderLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether inventory data exists.
        /// </summary>
        public bool HasInventory { get; set; }

        /// <summary>
        /// Gets a value indicating whether inventory exists with positive stock.
        /// </summary>
        public bool IsInStock
        {
            get { return HasInventory && StockQuantity > 0; }
        }
    }
}
