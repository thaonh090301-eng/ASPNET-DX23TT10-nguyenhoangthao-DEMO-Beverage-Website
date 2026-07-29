using System.Collections.Generic;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents the presentation model for checkout.
    /// </summary>
    public class CheckoutViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckoutViewModel"/> class.
        /// </summary>
        public CheckoutViewModel()
        {
            Input = new CheckoutInputViewModel();
            Addresses = new List<CheckoutAddressViewModel>();
            Cart = new CartViewModel();
        }

        /// <summary>
        /// Gets or sets the checkout input.
        /// </summary>
        public CheckoutInputViewModel Input { get; set; }

        /// <summary>
        /// Gets or sets the selectable checkout addresses.
        /// </summary>
        public List<CheckoutAddressViewModel> Addresses { get; set; }

        /// <summary>
        /// Gets or sets the cart summary.
        /// </summary>
        public CartViewModel Cart { get; set; }
    }
}
