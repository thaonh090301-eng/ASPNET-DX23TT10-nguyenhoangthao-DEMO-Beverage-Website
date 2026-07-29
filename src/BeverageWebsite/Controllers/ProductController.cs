using System.Net;
using System.Web.Mvc;
using BeverageWebsite.BLL;

namespace BeverageWebsite.Controllers
{
    /// <summary>
    /// Provides public read-only access to products.
    /// </summary>
    public class ProductController : Controller
    {
        private readonly ProductBLL _productBll;

        /// <summary>
        /// Initializes the controller for product queries.
        /// </summary>
        public ProductController()
        {
            _productBll = new ProductBLL();
        }

        /// <summary>
        /// Displays all products.
        /// </summary>
        /// <returns>A view containing all products.</returns>
        [HttpGet]
        public ActionResult Index()
        {
            var products = _productBll.GetAll();
            return View(products);
        }

        /// <summary>
        /// Displays the product with the specified identifier.
        /// </summary>
        /// <param name="id">The product identifier.</param>
        /// <returns>
        /// A bad request when the identifier is invalid, not found when no product
        /// matches the identifier, or a view containing the product.
        /// </returns>
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = _productBll.GetById(id.Value);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }
    }
}
