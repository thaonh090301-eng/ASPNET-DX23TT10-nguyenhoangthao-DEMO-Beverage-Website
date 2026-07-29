using System.ComponentModel.DataAnnotations;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents input used to create a shipping address.
    /// </summary>
    public class AddressInputViewModel
    {
        /// <summary>
        /// Gets or sets the recipient name.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận.")]
        [StringLength(200, ErrorMessage = "Tên người nhận không được vượt quá 200 ký tự.")]
        public string RecipientName { get; set; }

        /// <summary>
        /// Gets or sets the recipient phone number.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the street address.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ đường.")]
        [StringLength(255, ErrorMessage = "Địa chỉ đường không được vượt quá 255 ký tự.")]
        public string Street { get; set; }

        /// <summary>
        /// Gets or sets the optional ward.
        /// </summary>
        [StringLength(100, ErrorMessage = "Phường hoặc xã không được vượt quá 100 ký tự.")]
        public string Ward { get; set; }

        /// <summary>
        /// Gets or sets the optional district.
        /// </summary>
        [StringLength(100, ErrorMessage = "Quận hoặc huyện không được vượt quá 100 ký tự.")]
        public string District { get; set; }

        /// <summary>
        /// Gets or sets the city or province.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập tỉnh hoặc thành phố.")]
        [StringLength(100, ErrorMessage = "Tỉnh hoặc thành phố không được vượt quá 100 ký tự.")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets whether the address should be the default address.
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
