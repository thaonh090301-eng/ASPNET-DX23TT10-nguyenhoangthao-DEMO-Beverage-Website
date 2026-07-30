using System;
using System.Web.Mvc;
using System.Web.Security;
using BeverageWebsite.BLL;
using BeverageWebsite.ViewModels;

namespace BeverageWebsite.Controllers
{
    /// <summary>
    /// Provides authenticated access to order history presentation.
    /// </summary>
    public class OrderController : Controller
    {
        private readonly UserBLL _userBll;
        private readonly OrderBLL _orderBll;
        private readonly ProductBLL _productBll;

        /// <summary>
        /// Initializes the controller for authenticated order history queries.
        /// </summary>
        public OrderController()
        {
            _userBll = new UserBLL();
            _orderBll = new OrderBLL();
            _productBll = new ProductBLL();
        }

        /// <summary>
        /// Displays the authenticated customer's order history.
        /// </summary>
        /// <returns>The order history view or the login page for an invalid identity.</returns>
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

            var orders = _orderBll.GetByUserId(user.UserId);
            var viewModel = new OrderHistoryViewModel();

            if (orders != null)
            {
                foreach (var order in orders)
                {
                    viewModel.Orders.Add(new OrderHistoryItemViewModel
                    {
                        OrderId = order.OrderId,
                        OrderDate = order.OrderDate,
                        OrderStatus = order.OrderStatus,
                        TotalAmount = order.TotalAmount,
                        ShippingFee = order.ShippingFee,
                        FinalAmount = order.FinalAmount
                    });
                }
            }

            return View(viewModel);
        }

        /// <summary>
        /// Displays an order owned by the authenticated customer without disclosing ownership information.
        /// </summary>
        /// <param name="id">The requested order identifier.</param>
        /// <returns>The owned order-details view, or a not-found response when unavailable.</returns>
        [Authorize]
        [HttpGet]
        public ActionResult Details(int id)
        {
            if (id <= 0)
            {
                return HttpNotFound();
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

            var order = _orderBll.GetById(user.UserId, id);

            if (order == null)
            {
                return HttpNotFound();
            }

            var viewModel = new OrderDetailsViewModel
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                ShippingFee = order.ShippingFee,
                FinalAmount = order.FinalAmount
            };
            var items = _orderBll.GetOrderItems(user.UserId, id);

            if (items != null)
            {
                foreach (var item in items)
                {
                    var product = _productBll.GetById(item.ProductId);

                    viewModel.Items.Add(new OrderDetailItemViewModel
                    {
                        ProductName = product == null
                            ? "Sản phẩm không khả dụng"
                            : product.ProductName,
                        ImageUrl = product == null ? null : product.ImageUrl,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountAmount = item.DiscountAmount,
                        LineTotal = item.LineTotal
                    });
                }
            }

            return View(viewModel);
        }
    }
}
