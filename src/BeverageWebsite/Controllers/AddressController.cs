using System;
using System.Web.Mvc;
using System.Web.Security;
using BeverageWebsite.BLL;
using BeverageWebsite.Models;
using BeverageWebsite.ViewModels;

namespace BeverageWebsite.Controllers
{
    /// <summary>
    /// Provides authenticated shipping-address creation.
    /// </summary>
    public class AddressController : Controller
    {
        private readonly UserBLL _userBll;
        private readonly AddressBLL _addressBll;

        /// <summary>
        /// Initializes the controller for authenticated address operations.
        /// </summary>
        public AddressController()
        {
            _userBll = new UserBLL();
            _addressBll = new AddressBLL();
        }

        /// <summary>
        /// Displays the shipping-address creation form.
        /// </summary>
        /// <returns>The creation view or the login page for a stale identity.</returns>
        [Authorize]
        [HttpGet]
        public ActionResult Create()
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

            return View(new AddressInputViewModel());
        }

        /// <summary>
        /// Processes the authenticated shipping-address creation form.
        /// </summary>
        /// <param name="input">The submitted shipping-address input.</param>
        /// <returns>The creation view on failure or checkout after success.</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AddressInputViewModel input)
        {
            if (input == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Thông tin địa chỉ không hợp lệ.");
                return View(new AddressInputViewModel());
            }

            if (!ModelState.IsValid)
            {
                return View(input);
            }

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

            var address = new Address
            {
                UserId = user.UserId,
                RecipientName = input.RecipientName,
                Phone = input.Phone,
                Street = input.Street,
                Ward = input.Ward,
                District = input.District,
                City = input.City,
                IsDefault = input.IsDefault
            };

            try
            {
                _addressBll.Create(address);
            }
            catch (ArgumentException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể thêm địa chỉ giao hàng. Vui lòng kiểm tra thông tin và thử lại.");
                return View(input);
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể thêm địa chỉ giao hàng. Vui lòng kiểm tra thông tin và thử lại.");
                return View(input);
            }

            TempData["SuccessMessage"] = "Đã thêm địa chỉ giao hàng.";
            return RedirectToAction("Index", "Checkout");
        }
    }
}
