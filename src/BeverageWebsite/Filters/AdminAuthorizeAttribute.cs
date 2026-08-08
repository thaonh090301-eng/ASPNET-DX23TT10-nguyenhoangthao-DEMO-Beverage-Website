using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BeverageWebsite.BLL;

namespace BeverageWebsite.Filters
{
    /// <summary>
    /// Restricts administration actions to active users whose stored role is Admin.
    /// </summary>
    public sealed class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Determines whether the current authenticated account is an active Admin.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>True only for an authenticated, active Admin account.</returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null
                || httpContext.User == null
                || httpContext.User.Identity == null
                || !httpContext.User.Identity.IsAuthenticated
                || string.IsNullOrWhiteSpace(httpContext.User.Identity.Name))
            {
                return false;
            }

            try
            {
                var email = httpContext.User.Identity.Name;
                var userBll = new UserBLL();
                var user = userBll.GetByEmail(email);

                return user != null
                    && user.IsActive
                    && string.Equals(
                        user.Role,
                        "Admin",
                        StringComparison.Ordinal);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        /// <summary>
        /// Preserves the login flow for anonymous visitors and returns HTTP 403 for
        /// authenticated accounts that are not authorized as active Admins.
        /// </summary>
        /// <param name="filterContext">The authorization context.</param>
        protected override void HandleUnauthorizedRequest(
            AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            var identity = filterContext.HttpContext?.User?.Identity;

            if (identity == null || !identity.IsAuthenticated)
            {
                base.HandleUnauthorizedRequest(filterContext);
                return;
            }

            filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }
    }
}
