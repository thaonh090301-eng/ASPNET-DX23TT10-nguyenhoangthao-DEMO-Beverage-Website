using System.Collections.Generic;
using BeverageWebsite.Models;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents the active categories and featured products displayed on the home page.
    /// </summary>
    public class HomePageViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomePageViewModel"/> class.
        /// </summary>
        public HomePageViewModel()
        {
            Categories = new List<Category>();
            FeaturedProducts = new List<ProductCatalogItemViewModel>();
        }

        /// <summary>
        /// Gets or sets the active categories available in the public catalog.
        /// </summary>
        public List<Category> Categories { get; set; }

        /// <summary>
        /// Gets or sets the products featured on the home page.
        /// </summary>
        public List<ProductCatalogItemViewModel> FeaturedProducts { get; set; }
    }
}
