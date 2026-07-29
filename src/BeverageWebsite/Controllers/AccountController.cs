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

        /// <summary>
        /// Initializes the controller for account operations.
        /// </summary>
        public AccountController()
        {
            _userBll = new UserBLL();
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
        /// Displays the authenticated user's read-only profile.
        /// Signs out stale or inactive identities.
        /// </summary>
        /// <returns>The profile view for a valid active user; otherwise, the login page.</returns>
        [Authorize]
        [HttpGet]
        [ActionName("Profile")]
        public ActionResult UserProfile()
        {
            var authenticatedEmail = User.Identity.Name;

            if (string.IsNullOrWhiteSpace(authenticatedEmail))
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", "Account");
            }

            var user = _userBll.GetByEmail(authenticatedEmail);

            if (user == null || !user.IsActive)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", "Account");
            }

            return View(user);
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
    }
}
