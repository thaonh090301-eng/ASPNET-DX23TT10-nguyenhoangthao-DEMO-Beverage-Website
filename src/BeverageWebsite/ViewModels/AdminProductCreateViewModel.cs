using System.ComponentModel.DataAnnotations;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents product and initial inventory data submitted by an administrator.
    /// </summary>
    public class AdminProductCreateViewModel
    {
        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the selected category identifier.
        /// </summary>
        [Required]
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the product description.
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the selling price.
        /// </summary>
        [Required]
        [Range(typeof(decimal), "0", "9999999999.99")]
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the product image URL.
        /// </summary>
        [StringLength(500)]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product is available for sale.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the initial stock quantity.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        /// <summary>
        /// Gets or sets the stock level at which replenishment is needed.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; }
    }
}
