using System.Collections.Generic;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents an authenticated user's cart for presentation.
    /// </summary>
    public class CartViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CartViewModel"/> class.
        /// </summary>
        public CartViewModel()
        {
            Items = new List<CartItemViewModel>();
        }

        /// <summary>
        /// Gets or sets the cart items.
        /// </summary>
        public List<CartItemViewModel> Items { get; set; }

        /// <summary>
        /// Gets or sets the server-calculated cart total.
        /// </summary>
        public decimal CartTotal { get; set; }

        /// <summary>
        /// Gets or sets the server-calculated total item quantity.
        /// </summary>
        public int TotalItems { get; set; }
    }
}
