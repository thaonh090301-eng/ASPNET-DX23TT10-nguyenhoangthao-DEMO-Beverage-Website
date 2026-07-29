using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using BeverageWebsite.BLL;
using BeverageWebsite.Models;

namespace BeverageWebsite.Controllers
{
    /// <summary>
    /// Provides public read-only access to products.
    /// </summary>
    public class ProductController : Controller
    {
        private const int SearchKeywordMaxLength = 1000;

        private readonly ProductBLL _productBll;

        /// <summary>
        /// Initializes the controller for product queries.
        /// </summary>
        public ProductController()
        {
            _productBll = new ProductBLL();
        }

        /// <summary>
        /// Displays all products or products matching a keyword.
        /// </summary>
        /// <param name="keyword">The optional product search keyword.</param>
        /// <returns>A view containing all products.</returns>
        [HttpGet]
        public ActionResult Index(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                var allProducts = _productBll.GetAll();
                ViewData["Keyword"] = string.Empty;
                return View(allProducts);
            }

            var normalizedKeyword = keyword.Trim();

            if (normalizedKeyword.Length > SearchKeywordMaxLength)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Từ khóa tìm kiếm không được vượt quá 1000 ký tự.");
                ViewData["Keyword"] = keyword;
                return View(new List<Product>());
            }

            var products = _productBll.Search(normalizedKeyword);
            ViewData["Keyword"] = normalizedKeyword;
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
