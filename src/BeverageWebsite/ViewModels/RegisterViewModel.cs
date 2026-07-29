using System.ComponentModel.DataAnnotations;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents public registration form input.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Gets or sets the requested user name.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(
            100,
            ErrorMessage = "Tên đăng nhập không được vượt quá 100 ký tự.")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the registration email.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password entered for registration.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the password confirmation.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [DataType(DataType.Password)]
        [Compare(
            "Password",
            ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Gets or sets the optional full name.
        /// </summary>
        [StringLength(
            200,
            ErrorMessage = "Họ và tên không được vượt quá 200 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the optional phone number.
        /// </summary>
        [StringLength(
            20,
            ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }
    }
}
