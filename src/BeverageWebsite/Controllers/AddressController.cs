using System.Web.Mvc;

namespace BeverageWebsite.Controllers
{
    /// <summary>
    /// Redirects legacy address URLs to the shipping-address section in the customer profile.
    /// </summary>
    public class AddressController : Controller
    {
        /// <summary>
        /// Redirects the former address list URL to the profile page.
        /// </summary>
        [Authorize]
        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("Profile", "Account");
        }

        /// <summary>
        /// Redirects the former address creation URL to the profile editor.
        /// </summary>
        [Authorize]
        [HttpGet]
        public ActionResult Create()
        {
            return RedirectToAction("Profile", "Account", new { addAddress = true });
        }

        /// <summary>
        /// Redirects the former address edit URL to the profile editor.
        /// </summary>
        [Authorize]
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            return RedirectToAction("Profile", "Account", new { addressId = id });
        }

        /// <summary>
        /// Redirects legacy create form posts to the profile page.
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public ActionResult CreatePost()
        {
            return RedirectToAction("Profile", "Account", new { addAddress = true });
        }

        /// <summary>
        /// Redirects legacy edit form posts to the profile editor.
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public ActionResult EditPost(int? id)
        {
            return RedirectToAction("Profile", "Account", new { addressId = id });
        }
    }
}
