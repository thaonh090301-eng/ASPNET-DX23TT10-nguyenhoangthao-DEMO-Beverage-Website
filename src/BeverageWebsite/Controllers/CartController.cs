using System;
using System.Web.Mvc;
using System.Web.Security;
using BeverageWebsite.BLL;
using BeverageWebsite.ViewModels;

namespace BeverageWebsite.Controllers
{
    /// <summary>
    /// Provides authenticated read-only access to the shopping cart.
    /// </summary>
    public class CartController : Controller
    {
        private readonly CartBLL _cartBll;
        private readonly ProductBLL _productBll;
        private readonly UserBLL _userBll;

        /// <summary>
        /// Initializes the controller for authenticated cart queries.
        /// </summary>
        public CartController()
        {
            _cartBll = new CartBLL();
            _productBll = new ProductBLL();
            _userBll = new UserBLL();
        }

        /// <summary>
        /// Displays the authenticated user's cart.
        /// </summary>
        /// <returns>The cart view, or the login page for a stale identity.</returns>
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
                return View(new CartViewModel());
            }

            var cartItems = _cartBll.GetCartItems(user.UserId, cart.CartId);
            var viewModel = new CartViewModel
            {
                CartTotal = _cartBll.GetCartTotal(user.UserId, cart.CartId),
                TotalItems = _cartBll.GetTotalItems(user.UserId, cart.CartId)
            };

            foreach (var item in cartItems)
            {
                var product = _productBll.GetById(item.ProductId);

                viewModel.Items.Add(new CartItemViewModel
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

            return View(viewModel);
        }
    }
}
