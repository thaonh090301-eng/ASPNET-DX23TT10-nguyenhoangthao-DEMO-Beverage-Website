using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;
using BeverageWebsite.BLL;
using BeverageWebsite.ViewModels;

namespace BeverageWebsite.Controllers
{
    /// <summary>
    /// Provides authenticated access to checkout presentation.
    /// </summary>
    public class CheckoutController : Controller
    {
        private readonly CartBLL _cartBll;
        private readonly ProductBLL _productBll;
        private readonly UserBLL _userBll;
        private readonly AddressBLL _addressBll;
        private readonly OrderBLL _orderBll;

        /// <summary>
        /// Initializes the controller for authenticated checkout queries.
        /// </summary>
        public CheckoutController()
        {
            _cartBll = new CartBLL();
            _productBll = new ProductBLL();
            _userBll = new UserBLL();
            _addressBll = new AddressBLL();
            _orderBll = new OrderBLL();
        }

        /// <summary>
        /// Displays checkout information for the authenticated user's cart.
        /// </summary>
        /// <returns>The checkout view or a redirect when checkout cannot continue.</returns>
        [Authorize]
        [HttpGet]
        public ActionResult Index()
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

            var cart = _cartBll.GetByUserId(user.UserId);

            if (cart == null)
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn hiện đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            var cartItems = _cartBll.GetCartItems(user.UserId, cart.CartId);

            if (cartItems == null || cartItems.Count == 0)
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn hiện đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            var cartViewModel = new CartViewModel
            {
                CartTotal = _cartBll.GetCartTotal(user.UserId, cart.CartId),
                TotalItems = _cartBll.GetTotalItems(user.UserId, cart.CartId)
            };

            foreach (var item in cartItems)
            {
                var product = _productBll.GetById(item.ProductId);

                cartViewModel.Items.Add(new CartItemViewModel
                {
                    CartItemId = item.CartItemId,
                    ProductId = item.ProductId,
                    ProductName = product == null
                        ? "Sản phẩm không khả dụng"
                        : product.ProductName,
                    ImageUrl = product == null ? null : product.ImageUrl,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            var addresses = _addressBll.GetByUserId(user.UserId);
            var addressViewModels = new List<CheckoutAddressViewModel>();

            if (addresses != null)
            {
                foreach (var address in addresses)
                {
                    addressViewModels.Add(new CheckoutAddressViewModel
                    {
                        AddressId = address.AddressId,
                        RecipientName = address.RecipientName,
                        Phone = address.Phone,
                        Street = address.Street,
                        Ward = address.Ward,
                        District = address.District,
                        City = address.City,
                        IsDefault = address.IsDefault
                    });
                }
            }

            var viewModel = new CheckoutViewModel
            {
                Input = new CheckoutInputViewModel(),
                Addresses = addressViewModels,
                Cart = cartViewModel
            };

            CheckoutAddressViewModel selectedAddress = null;

            foreach (var address in addressViewModels)
            {
                if (address.IsDefault)
                {
                    selectedAddress = address;
                    break;
                }
            }

            if (selectedAddress == null && addressViewModels.Count > 0)
            {
                selectedAddress = addressViewModels[0];
            }

            if (selectedAddress != null)
            {
                viewModel.Input.AddressId = selectedAddress.AddressId;
            }

            return View(viewModel);
        }

        /// <summary>
        /// Places an order for the authenticated user's cart.
        /// </summary>
        /// <param name="input">The selected checkout input.</param>
        /// <returns>A redirect after the order attempt.</returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(CheckoutInputViewModel input)
        {
            if (input == null || !ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn địa chỉ giao hàng hợp lệ.";
                return RedirectToAction("Index", "Checkout");
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

            var cart = _cartBll.GetByUserId(user.UserId);

            if (cart == null)
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn hiện đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            var cartItems = _cartBll.GetCartItems(user.UserId, cart.CartId);

            if (cartItems == null || cartItems.Count == 0)
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn hiện đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            try
            {
                var orderId = _orderBll.CreateOrderFromCart(
                    user.UserId,
                    cart.CartId,
                    input.AddressId);

                if (orderId > 0)
                {
                    TempData["SuccessMessage"] = "Đặt hàng thành công.";
                    return RedirectToAction("Index", "Cart");
                }
            }
            catch (InvalidOperationException)
            {
                TempData["ErrorMessage"] =
                    "Không thể đặt hàng. Vui lòng kiểm tra giỏ hàng, địa chỉ và tồn kho rồi thử lại.";
                return RedirectToAction("Index", "Checkout");
            }

            TempData["ErrorMessage"] =
                "Không thể đặt hàng. Vui lòng kiểm tra giỏ hàng, địa chỉ và tồn kho rồi thử lại.";
            return RedirectToAction("Index", "Checkout");
        }
    }
}
