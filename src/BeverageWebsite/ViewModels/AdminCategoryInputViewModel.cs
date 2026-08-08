using System.ComponentModel.DataAnnotations;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents category data entered in the protected administration area.
    /// </summary>
    public class AdminCategoryInputViewModel
    {
        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the category description.
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the category is active.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
