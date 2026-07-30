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

        /// <summary>
        /// Initializes the controller for authenticated order history queries.
        /// </summary>
        public OrderController()
        {
            _userBll = new UserBLL();
            _orderBll = new OrderBLL();
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
    }
}
