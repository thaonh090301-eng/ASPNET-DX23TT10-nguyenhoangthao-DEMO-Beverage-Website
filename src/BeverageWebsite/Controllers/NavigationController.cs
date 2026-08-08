using System;
using System.Web.Mvc;
using BeverageWebsite.BLL;

namespace BeverageWebsite.Controllers
{
    /// <summary>
    /// Provides shared navigation fragments based on the authenticated account.
    /// </summary>
    public class NavigationController : Controller
    {
        private readonly UserBLL _userBll;

        /// <summary>
        /// Initializes the controller for navigation queries.
        /// </summary>
        public NavigationController()
        {
            _userBll = new UserBLL();
        }

        /// <summary>
        /// Renders the administration navigation item for an active Admin account.
        /// </summary>
        /// <returns>A partial view indicating whether the Admin link is visible.</returns>
        [ChildActionOnly]
        public ActionResult AdminNavigation()
        {
            var isAdmin = false;

            if (User != null
                && User.Identity != null
                && User.Identity.IsAuthenticated
                && !string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                try
                {
                    var user = _userBll.GetByEmail(User.Identity.Name);

                    isAdmin = user != null
                        && user.IsActive
                        && string.Equals(
                            user.Role,
                            "Admin",
                            StringComparison.Ordinal);
                }
                catch (ArgumentException)
                {
                    isAdmin = false;
                }
                catch (InvalidOperationException)
                {
                    isAdmin = false;
                }
            }

            return PartialView("_AdminNavigation", isAdmin);
        }
    }
}
