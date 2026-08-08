using System.Net;
using System.Web.Mvc;
using BeverageWebsite.BLL;

namespace BeverageWebsite.Controllers
{
    /// <summary>
    /// Provides public read-only access to product categories.
    /// </summary>
    public class CategoryController : Controller
    {
        private readonly CategoryBLL _categoryBll;

        /// <summary>
        /// Initializes the controller for category queries.
        /// </summary>
        public CategoryController()
        {
            _categoryBll = new CategoryBLL();
        }

        /// <summary>
        /// Displays all product categories.
        /// </summary>
        /// <returns>A view containing all categories.</returns>
        [HttpGet]
        public ActionResult Index()
        {
            var categories = _categoryBll.GetActive();
            return View(categories);
        }

        /// <summary>
        /// Displays the category with the specified identifier.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <returns>
        /// A bad request when the identifier is missing, not found when no category
        /// matches the identifier, or a view containing the category.
        /// </returns>
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = _categoryBll.GetActiveById(id.Value);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }
    }
}
