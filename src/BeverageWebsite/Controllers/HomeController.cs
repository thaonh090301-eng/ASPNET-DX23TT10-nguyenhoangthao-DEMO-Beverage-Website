using System.Linq;
using System.Web.Mvc;
using BeverageWebsite.BLL;
using BeverageWebsite.Models;
using BeverageWebsite.ViewModels;

namespace BeverageWebsite
{
    /// <summary>
    /// Provides the public storefront landing and informational pages.
    /// </summary>
    public class HomeController : Controller
    {
        private const int FeaturedProductCount = 8;

        private readonly CategoryBLL _categoryBll;
        private readonly ProductBLL _productBll;
        private readonly InventoryBLL _inventoryBll;

        /// <summary>
        /// Initializes the controller for storefront queries.
        /// </summary>
        public HomeController()
        {
            _categoryBll = new CategoryBLL();
            _productBll = new ProductBLL();
            _inventoryBll = new InventoryBLL();
        }

        /// <summary>
        /// Displays active categories and up to eight active featured products.
        /// </summary>
        /// <returns>The storefront home view.</returns>
        [HttpGet]
        public ActionResult Index()
        {
            var categories = _categoryBll.GetActive();
            var products = _productBll.GetFeaturedActive(FeaturedProductCount);

            if (products.Count == 0)
            {
                products = _productBll
                    .GetActive()
                    .Take(FeaturedProductCount)
                    .ToList();
            }

            var inventoryRecords = _inventoryBll.GetAll();
            var inventoryByProductId = inventoryRecords
                .ToDictionary(inventory => inventory.ProductId);
            var featuredProducts = products
                .Take(FeaturedProductCount)
                .Select(product =>
                {
                    Inventory inventory;
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
                        IsFeatured = product.IsFeatured,
                        BadgeType = product.BadgeType,
                        StockQuantity = stockQuantity
                    };
                })
                .ToList();
            var viewModel = new HomePageViewModel
            {
                Categories = categories,
                FeaturedProducts = featuredProducts
            };

            return View(viewModel);
        }

        /// <summary>
        /// Displays the static beverage journal page.
        /// </summary>
        /// <returns>The blog view.</returns>
        [HttpGet]
        public ActionResult Blog()
        {
            return View();
        }

        /// <summary>
        /// Displays the static store contact page.
        /// </summary>
        /// <returns>The contact view.</returns>
        [HttpGet]
        public ActionResult Contact()
        {
            return View();
        }
    }
}
