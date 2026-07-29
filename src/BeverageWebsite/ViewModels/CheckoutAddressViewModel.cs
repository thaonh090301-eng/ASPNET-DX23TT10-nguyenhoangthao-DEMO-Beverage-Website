namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents a selectable shipping address for checkout presentation.
    /// </summary>
    public class CheckoutAddressViewModel
    {
        /// <summary>
        /// Gets or sets the address identifier.
        /// </summary>
        public int AddressId { get; set; }

        /// <summary>
        /// Gets or sets the recipient name.
        /// </summary>
        public string RecipientName { get; set; }

        /// <summary>
        /// Gets or sets the recipient phone number.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the street address.
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Gets or sets the ward.
        /// </summary>
        public string Ward { get; set; }

        /// <summary>
        /// Gets or sets the district.
        /// </summary>
        public string District { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets whether the address is the default address.
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
