using System.ComponentModel.DataAnnotations;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents editable product data in the protected administration area.
    /// </summary>
    public class AdminProductEditViewModel
    {
        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        public int ProductId { get; set; }

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
        /// Gets or sets the current selling price.
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
        /// Gets or sets a value indicating whether the product is active.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
