namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents a product row on the administration product list.
    /// </summary>
    public class AdminProductItemViewModel
    {
        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the current selling price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product is active.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
