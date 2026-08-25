namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents the read-only summary displayed on the administration dashboard.
    /// </summary>
    public class AdminDashboardViewModel
    {
        /// <summary>
        /// Gets or sets the number of active categories.
        /// </summary>
        public int ActiveCategoryCount { get; set; }

        /// <summary>
        /// Gets or sets the number of active public products.
        /// </summary>
        public int ActiveProductCount { get; set; }

        /// <summary>
        /// Gets or sets the number of active products with available stock.
        /// </summary>
        public int InStockProductCount { get; set; }

        /// <summary>
        /// Gets or sets the number of active products without available stock.
        /// </summary>
        public int OutOfStockProductCount { get; set; }

        /// <summary>
        /// Gets or sets the number of orders awaiting confirmation.
        /// </summary>
        public int PendingOrderCount { get; set; }
    }
}
