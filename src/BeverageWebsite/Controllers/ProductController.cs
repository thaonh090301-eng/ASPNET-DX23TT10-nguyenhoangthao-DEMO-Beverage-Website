using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BeverageWebsite.BLL;
using BeverageWebsite.Models;
using BeverageWebsite.ViewModels;

namespace BeverageWebsite.Controllers
{
    /// <summary>
    /// Provides public read-only access to products.
    /// </summary>
    public class ProductController : Controller
    {
        private const int SearchKeywordMaxLength = 1000;

        private readonly ProductBLL _productBll;
        private readonly CategoryBLL _categoryBll;
        private readonly InventoryBLL _inventoryBll;

        /// <summary>
        /// Initializes the controller for product queries.
        /// </summary>
        public ProductController()
        {
            _productBll = new ProductBLL();
            _categoryBll = new CategoryBLL();
            _inventoryBll = new InventoryBLL();
        }

        /// <summary>
        /// Displays all products or products matching a public catalog filter.
        /// </summary>
        /// <param name="keyword">The optional product search keyword.</param>
        /// <param name="categoryId">The optional category identifier.</param>
        /// <returns>A view containing all products.</returns>
        [HttpGet]
        public ActionResult Index(string keyword, int? categoryId)
        {
            var categories = _categoryBll.GetActive();
            var hasKeyword = !string.IsNullOrWhiteSpace(keyword);
            var normalizedKeyword = hasKeyword ? keyword.Trim() : string.Empty;
            var hasInvalidKeyword = normalizedKeyword.Length > SearchKeywordMaxLength;
            var hasInvalidCategory = categoryId.HasValue && categoryId.Value <= 0;

            ViewData["Keyword"] = hasInvalidKeyword ? keyword : normalizedKeyword;
            ViewData["CategoryId"] = categoryId;
            ViewData["Categories"] = categories;

            if (hasInvalidKeyword)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Từ khóa tìm kiếm không được vượt quá 1000 ký tự.");
            }

            if (hasInvalidCategory)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Danh mục sản phẩm không hợp lệ.");
            }

            if (hasInvalidKeyword || hasInvalidCategory)
            {
                return View(new List<ProductCatalogItemViewModel>());
            }

            if (hasKeyword && categoryId.HasValue)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Hiện chưa hỗ trợ kết hợp từ khóa và danh mục trong cùng một lần tìm kiếm.");
                return View(new List<ProductCatalogItemViewModel>());
            }

            List<Product> products;

            if (hasKeyword)
            {
                products = _productBll.SearchActive(normalizedKeyword);
            }
            else if (categoryId.HasValue)
            {
                var category = _categoryBll.GetActiveById(categoryId.Value);

                if (category == null)
                {
                    return HttpNotFound();
                }

                products = _productBll.GetActiveByCategory(categoryId.Value);
            }
            else
            {
                products = _productBll.GetActive();
            }

            var inventoryByProductId = _inventoryBll
                .GetAll()
                .ToDictionary(inventory => inventory.ProductId);
            var viewModels = products.Select(product =>
            {
                BeverageWebsite.Models.Inventory inventory;
                var stockQuantity = inventoryByProductId.TryGetValue(
                    product.ProductId,
                    out inventory)
                    ? inventory.StockQuantity
                    : 0;

                return new ProductCatalogItemViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    StockQuantity = stockQuantity
                };
            }).ToList();

            return View(viewModels);
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

            var product = _productBll.GetActiveById(id.Value);

            if (product == null)
            {
                return HttpNotFound();
            }

            var inventory = _inventoryBll.GetByProductId(product.ProductId);
            var viewModel = new ProductDetailsViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                StockQuantity = inventory != null ? inventory.StockQuantity : 0
            };

            return View(viewModel);
        }
    }
}
