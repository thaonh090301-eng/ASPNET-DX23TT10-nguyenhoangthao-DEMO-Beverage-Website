using System.ComponentModel.DataAnnotations;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents the input required to check out a cart.
    /// </summary>
    public class CheckoutInputViewModel
    {
        /// <summary>
        /// Gets or sets the selected shipping-address identifier.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn địa chỉ giao hàng hợp lệ.")]
        public int AddressId { get; set; }
    }
}
