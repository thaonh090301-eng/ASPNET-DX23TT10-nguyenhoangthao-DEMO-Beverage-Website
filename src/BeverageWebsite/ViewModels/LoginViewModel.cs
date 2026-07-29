using System.ComponentModel.DataAnnotations;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents public login form input.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Gets or sets the email entered for login.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password entered for login.
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets whether the login should be remembered.
        /// </summary>
        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }
}
