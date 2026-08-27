using System.Collections.Generic;
using BeverageWebsite.Models;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents the authenticated customer's account information and shipping-address editor.
    /// </summary>
    public class ProfileViewModel
    {
        /// <summary>
        /// Gets or sets the authenticated customer.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets all shipping addresses belonging to the customer.
        /// </summary>
        public List<Address> Addresses { get; set; }

        /// <summary>
        /// Gets or sets the address currently being edited, or a new address input.
        /// </summary>
        public AddressInputViewModel AddressInput { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the address being edited. Null represents a new address.
        /// </summary>
        public int? EditingAddressId { get; set; }

        /// <summary>
        /// Gets or sets whether the address editor is creating a new address.
        /// </summary>
        public bool IsAddingAddress { get; set; }
    }
}
