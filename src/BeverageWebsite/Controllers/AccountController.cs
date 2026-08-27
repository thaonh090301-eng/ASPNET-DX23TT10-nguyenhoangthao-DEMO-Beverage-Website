using System;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using BeverageWebsite.BLL;
using BeverageWebsite.Models;
using BeverageWebsite.ViewModels;

namespace BeverageWebsite.Controllers
{
    /// <summary>
    /// Provides public registration and Forms Authentication account actions.
    /// </summary>
    public class AccountController : Controller
    {
        private const string InvalidLoginMessage = "Email hoặc mật khẩu không đúng.";
        private const string RegistrationFailureMessage =
            "Không thể tạo tài khoản. Tên đăng nhập hoặc email có thể đã được sử dụng.";

        private readonly UserBLL _userBll;
        private readonly AddressBLL _addressBll;

        /// <summary>
        /// Initializes the controller for account operations.
        /// </summary>
        public AccountController()
        {
            _userBll = new UserBLL();
            _addressBll = new AddressBLL();
        }

        /// <summary>
        /// Displays the public login form without creating an authentication cookie.
        /// </summary>
        /// <param name="returnUrl">The optional local destination after login.</param>
        /// <returns>The login view with an empty input model.</returns>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        /// <summary>
        /// Authenticates valid credentials and redirects to a safe destination.
        /// </summary>
        /// <param name="model">The login form input.</param>
        /// <param name="returnUrl">The optional local destination after login.</param>
        /// <returns>The login view on failure or a redirect after successful login.</returns>
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _userBll.GetByEmailForAuthentication(model.Email);

            if (user == null
                || !user.IsActive
                || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, InvalidLoginMessage);
                return View(model);
            }

            var passwordIsValid = false;

            try
            {
                passwordIsValid = Crypto.VerifyHashedPassword(
                    user.PasswordHash,
                    model.Password);
            }
            catch (FormatException)
            {
                passwordIsValid = false;
            }

            if (!passwordIsValid)
            {
                ModelState.AddModelError(string.Empty, InvalidLoginMessage);
                return View(model);
            }

            FormsAuthentication.SetAuthCookie(user.Email, model.RememberMe);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Displays the public registration form.
        /// </summary>
        /// <returns>The registration view with an empty input model.</returns>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        /// <summary>
        /// Registers an active customer account using server-controlled account values.
        /// </summary>
        /// <param name="model">The registration form input.</param>
        /// <returns>The registration view on failure or the login page after success.</returns>
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_userBll.ExistsByEmail(model.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(model);
            }

            var passwordHash = Crypto.HashPassword(model.Password);
            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                PasswordHash = passwordHash,
                FullName = model.FullName,
                Phone = model.Phone,
                Role = "Customer",
                IsActive = true
            };

            try
            {
                _userBll.Create(user);
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError(string.Empty, RegistrationFailureMessage);
                return View(model);
            }

            TempData["SuccessMessage"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Displays the authenticated user's profile and shipping-address management section.
        /// Signs out stale or inactive identities.
        /// </summary>
        /// <returns>The profile view for a valid active user; otherwise, the login page.</returns>
        [Authorize]
        [HttpGet]
        [ActionName("Profile")]
        public ActionResult UserProfile(int? addressId, bool addAddress = false)
        {
            var user = GetAuthenticatedUser();

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(BuildProfileViewModel(user, addressId, addAddress, null));
        }

        /// <summary>
        /// Creates or updates a shipping address from the profile page.
        /// </summary>
        /// <param name="addressId">The owned address to update; null creates a new address.</param>
        /// <param name="input">The submitted address values.</param>
        /// <returns>The profile view on failure or the profile page after saving.</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveAddress(
            int? addressId,
            [Bind(Prefix = "AddressInput")] AddressInputViewModel input)
        {
            var user = GetAuthenticatedUser();

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (input == null || !ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Vui lòng kiểm tra lại thông tin địa chỉ giao hàng.");
                return View(
                    "Profile",
                    BuildProfileViewModel(user, addressId, !addressId.HasValue, input));
            }

            try
            {
                if (addressId.HasValue)
                {
                    var existingAddress = _addressBll.GetByUserIdAndAddressId(user.UserId, addressId.Value);

                    if (existingAddress == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy địa chỉ giao hàng cần chỉnh sửa.";
                        return RedirectToAction("Profile");
                    }

                    _addressBll.Update(new Address
                    {
                        AddressId = existingAddress.AddressId,
                        UserId = user.UserId,
                        RecipientName = input.RecipientName,
                        Phone = input.Phone,
                        Street = input.Street,
                        Ward = input.Ward,
                        District = input.District,
                        City = input.City,
                        IsDefault = existingAddress.IsDefault
                    });

                    TempData["SuccessMessage"] = "Đã cập nhật địa chỉ giao hàng.";
                    return RedirectToAction("Profile");
                }

                _addressBll.Create(new Address
                {
                    UserId = user.UserId,
                    RecipientName = input.RecipientName,
                    Phone = input.Phone,
                    Street = input.Street,
                    Ward = input.Ward,
                    District = input.District,
                    City = input.City,
                    IsDefault = input.IsDefault
                });

                TempData["SuccessMessage"] = "Đã thêm địa chỉ giao hàng.";
                return RedirectToAction("Profile");
            }
            catch (ArgumentException)
            {
                ModelState.AddModelError(string.Empty, "Không thể lưu địa chỉ giao hàng. Vui lòng kiểm tra thông tin và thử lại.");
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError(string.Empty, "Không thể lưu địa chỉ giao hàng. Vui lòng thử lại.");
            }

            return View(
                "Profile",
                BuildProfileViewModel(user, addressId, !addressId.HasValue, input));
        }

        /// <summary>
        /// Signs out the authenticated user through a protected POST request.
        /// </summary>
        /// <returns>A redirect to the public home page.</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        private User GetAuthenticatedUser()
        {
            var authenticatedEmail = User.Identity.Name;

            if (string.IsNullOrWhiteSpace(authenticatedEmail))
            {
                FormsAuthentication.SignOut();
                return null;
            }

            var user = _userBll.GetByEmail(authenticatedEmail);

            if (user == null || !user.IsActive)
            {
                FormsAuthentication.SignOut();
                return null;
            }

            return user;
        }

        private ProfileViewModel BuildProfileViewModel(
            User user,
            int? addressId,
            bool addAddress,
            AddressInputViewModel submittedInput)
        {
            var viewModel = new ProfileViewModel
            {
                User = user,
                Addresses = new System.Collections.Generic.List<Address>()
            };

            try
            {
                viewModel.Addresses = _addressBll.GetByUserId(user.UserId);
            }
            catch (InvalidOperationException)
            {
                TempData["ErrorMessage"] = "Không thể tải địa chỉ giao hàng. Vui lòng thử lại.";
            }

            if (viewModel.Addresses == null)
            {
                viewModel.Addresses = new System.Collections.Generic.List<Address>();
            }

            Address selectedAddress = null;

            if (addressId.HasValue && addressId.Value > 0)
            {
                selectedAddress = viewModel.Addresses.Find(address => address.AddressId == addressId.Value);

                if (selectedAddress == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy địa chỉ giao hàng cần chỉnh sửa.";
                }
            }

            if (selectedAddress != null)
            {
                viewModel.EditingAddressId = selectedAddress.AddressId;
                viewModel.AddressInput = submittedInput ?? new AddressInputViewModel
                {
                    RecipientName = selectedAddress.RecipientName,
                    Phone = selectedAddress.Phone,
                    Street = selectedAddress.Street,
                    Ward = selectedAddress.Ward,
                    District = selectedAddress.District,
                    City = selectedAddress.City,
                    IsDefault = selectedAddress.IsDefault
                };

                return viewModel;
            }

            if (addAddress)
            {
                viewModel.IsAddingAddress = true;
                viewModel.AddressInput = submittedInput ?? new AddressInputViewModel();
            }

            return viewModel;
        }
    }
}
