using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BeverageWebsite.BLL;
using BeverageWebsite.Filters;
using BeverageWebsite.Models;
using BeverageWebsite.ViewModels;

namespace BeverageWebsite.Controllers
{
    /// <summary>
    /// Provides protected administration dashboard and inventory operations.
    /// </summary>
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private readonly CategoryBLL _categoryBll;
        private readonly ProductBLL _productBll;
        private readonly InventoryBLL _inventoryBll;

        /// <summary>
        /// Initializes the controller for protected administration operations.
        /// </summary>
        public AdminController()
        {
            _categoryBll = new CategoryBLL();
            _productBll = new ProductBLL();
            _inventoryBll = new InventoryBLL();
        }

        /// <summary>
        /// Displays a read-only summary of the active public catalog.
        /// </summary>
        /// <returns>The administration dashboard view.</returns>
        [HttpGet]
        public ActionResult Index()
        {
            var categories = _categoryBll.GetActive();
            var products = _productBll.GetActive();
            var inventoryByProductId = _inventoryBll
                .GetAll()
                .ToDictionary(inventory => inventory.ProductId);
            var inStockProductCount = products.Count(product =>
            {
                Inventory inventory;
                return inventoryByProductId.TryGetValue(product.ProductId, out inventory)
                    && inventory.StockQuantity > 0;
            });

            var viewModel = new AdminDashboardViewModel
            {
                ActiveCategoryCount = categories.Count,
                ActiveProductCount = products.Count,
                InStockProductCount = inStockProductCount,
                OutOfStockProductCount = products.Count - inStockProductCount
            };

            return View(viewModel);
        }

        /// <summary>
        /// Displays inventory for all active products in the public catalog.
        /// </summary>
        /// <returns>The inventory management view.</returns>
        [HttpGet]
        public ActionResult Inventory()
        {
            var products = _productBll.GetActive();
            var categoryById = _categoryBll
                .GetActive()
                .ToDictionary(category => category.CategoryId);
            var inventoryByProductId = _inventoryBll
                .GetAll()
                .ToDictionary(inventory => inventory.ProductId);
            var viewModels = products.Select(product =>
            {
                Category category;
                Inventory inventory;
                var hasCategory = categoryById.TryGetValue(
                    product.CategoryId,
                    out category);
                var hasInventory = inventoryByProductId.TryGetValue(
                    product.ProductId,
                    out inventory);

                return new AdminInventoryItemViewModel
                {
                    ProductId = product.ProductId,
                    CategoryName = hasCategory
                        ? category.CategoryName
                        : "Không xác định",
                    ProductName = product.ProductName,
                    Price = product.Price,
                    HasInventory = hasInventory,
                    StockQuantity = hasInventory ? inventory.StockQuantity : 0,
                    ReorderLevel = hasInventory ? inventory.ReorderLevel : 0
                };
            })
            .OrderBy(item => item.CategoryName)
            .ThenBy(item => item.ProductName)
            .ToList();

            return View(viewModels);
        }

        /// <summary>
        /// Replaces the stock quantity for an active product.
        /// </summary>
        /// <param name="productId">The active product identifier.</param>
        /// <param name="quantity">The new absolute stock quantity.</param>
        /// <returns>A redirect to the inventory management page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStock(int productId, int quantity)
        {
            if (productId <= 0)
            {
                TempData["ErrorMessage"] = "Sản phẩm không hợp lệ.";
                return RedirectToAction("Inventory", "Admin");
            }

            if (quantity < 0)
            {
                TempData["ErrorMessage"] =
                    "Số lượng tồn kho không được nhỏ hơn 0.";
                return RedirectToAction("Inventory", "Admin");
            }

            try
            {
                var product = _productBll.GetActiveById(productId);

                if (product == null)
                {
                    TempData["ErrorMessage"] =
                        "Không tìm thấy sản phẩm đang kinh doanh.";
                    return RedirectToAction("Inventory", "Admin");
                }

                var inventory = _inventoryBll.GetByProductId(productId);

                if (inventory == null)
                {
                    TempData["ErrorMessage"] =
                        "Sản phẩm chưa có dữ liệu kho. Vui lòng kiểm tra dữ liệu.";
                    return RedirectToAction("Inventory", "Admin");
                }

                _inventoryBll.UpdateStock(productId, quantity);

                TempData["SuccessMessage"] = quantity == 0
                    ? "Đã cập nhật tồn kho. Món hiện được hiển thị là tạm hết."
                    : "Đã cập nhật tồn kho sản phẩm.";
            }
            catch (ArgumentException)
            {
                TempData["ErrorMessage"] =
                    "Không thể cập nhật tồn kho. Vui lòng kiểm tra dữ liệu và thử lại.";
            }
            catch (InvalidOperationException)
            {
                TempData["ErrorMessage"] =
                    "Không thể cập nhật tồn kho. Vui lòng kiểm tra dữ liệu và thử lại.";
            }

            return RedirectToAction("Inventory", "Admin");
        }

        /// <summary>
        /// Displays all active and inactive products for administration.
        /// </summary>
        /// <returns>The product management view.</returns>
        [HttpGet]
        public ActionResult Products()
        {
            var products = _productBll.GetAll();
            var categoryById = _categoryBll
                .GetAll()
                .ToDictionary(category => category.CategoryId);
            var viewModels = products.Select(product =>
            {
                Category category;
                var hasCategory = categoryById.TryGetValue(
                    product.CategoryId,
                    out category);

                return new AdminProductItemViewModel
                {
                    ProductId = product.ProductId,
                    CategoryId = product.CategoryId,
                    CategoryName = hasCategory
                        ? category.CategoryName
                        : "Không xác định",
                    ProductName = product.ProductName,
                    Price = product.Price,
                    IsActive = product.IsActive
                };
            })
            .OrderBy(item => item.CategoryName)
            .ThenBy(item => item.ProductName)
            .ToList();

            return View(viewModels);
        }

        /// <summary>
        /// Displays the edit form for an active or inactive product.
        /// </summary>
        /// <param name="id">The product identifier.</param>
        /// <returns>The product edit view or an HTTP error result.</returns>
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = _productBll.GetById(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            var model = new AdminProductEditViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                CategoryId = product.CategoryId,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive
            };

            PopulateCategorySelectList(model.CategoryId);
            return View(model);
        }

        /// <summary>
        /// Updates editable fields for an existing product.
        /// </summary>
        /// <param name="model">The submitted product data.</param>
        /// <returns>The edit view on failure or a redirect on success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(AdminProductEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateCategorySelectList(model.CategoryId);
                return View(model);
            }

            try
            {
                if (model.ProductId <= 0)
                {
                    return HttpNotFound();
                }

                var existing = _productBll.GetById(model.ProductId);

                if (existing == null)
                {
                    return HttpNotFound();
                }

                Category category = null;

                if (model.CategoryId > 0)
                {
                    category = _categoryBll.GetById(model.CategoryId);
                }

                if (category == null)
                {
                    ModelState.AddModelError(
                        "CategoryId",
                        "Loại đồ uống không hợp lệ.");
                    PopulateCategorySelectList(model.CategoryId);
                    return View(model);
                }

                var product = new Product
                {
                    ProductId = existing.ProductId,
                    CategoryId = model.CategoryId,
                    ProductName = model.ProductName,
                    Description = model.Description,
                    Price = model.Price,
                    ImageUrl = model.ImageUrl,
                    IsActive = model.IsActive
                };

                _productBll.Update(product);
                TempData["SuccessMessage"] = "Đã cập nhật sản phẩm.";
                return RedirectToAction("Products", "Admin");
            }
            catch (ArgumentException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể cập nhật sản phẩm. Vui lòng kiểm tra dữ liệu.");
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể cập nhật sản phẩm. Vui lòng kiểm tra dữ liệu.");
            }

            PopulateCategorySelectList(model.CategoryId);
            return View(model);
        }

        /// <summary>
        /// Displays all active and inactive beverage categories.
        /// </summary>
        /// <returns>The category management view.</returns>
        [HttpGet]
        public ActionResult Categories()
        {
            var categories = _categoryBll.GetAll();
            return View(categories);
        }

        /// <summary>
        /// Displays the form for creating a beverage category.
        /// </summary>
        /// <returns>The category creation view.</returns>
        [HttpGet]
        public ActionResult CreateCategory()
        {
            return View(new AdminCategoryInputViewModel
            {
                IsActive = true
            });
        }

        /// <summary>
        /// Creates a beverage category.
        /// </summary>
        /// <param name="model">The submitted category data.</param>
        /// <returns>The creation view on failure or a redirect on success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCategory(AdminCategoryInputViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var category = new Category
                {
                    CategoryName = model.CategoryName,
                    Description = model.Description,
                    IsActive = model.IsActive
                };

                _categoryBll.Create(category);
                TempData["SuccessMessage"] = "Đã thêm loại đồ uống.";
                return RedirectToAction("Categories", "Admin");
            }
            catch (ArgumentException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể thêm loại đồ uống. Tên loại có thể đã tồn tại.");
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể thêm loại đồ uống. Tên loại có thể đã tồn tại.");
            }

            return View(model);
        }

        /// <summary>
        /// Displays the edit form for an active or inactive category.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <returns>The category edit view or an HTTP error result.</returns>
        [HttpGet]
        public ActionResult EditCategory(int id)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = _categoryBll.GetById(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            var model = new AdminCategoryInputViewModel
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                IsActive = category.IsActive
            };

            return View(model);
        }

        /// <summary>
        /// Updates an existing beverage category without changing its products.
        /// </summary>
        /// <param name="model">The submitted category data.</param>
        /// <returns>The edit view on failure or a redirect on success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(AdminCategoryInputViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (model.CategoryId <= 0)
                {
                    return HttpNotFound();
                }

                var existing = _categoryBll.GetById(model.CategoryId);

                if (existing == null)
                {
                    return HttpNotFound();
                }

                var category = new Category
                {
                    CategoryId = existing.CategoryId,
                    CategoryName = model.CategoryName,
                    Description = model.Description,
                    IsActive = model.IsActive
                };

                _categoryBll.Update(category);
                TempData["SuccessMessage"] = "Đã cập nhật loại đồ uống.";
                return RedirectToAction("Categories", "Admin");
            }
            catch (ArgumentException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể cập nhật loại đồ uống. Tên loại có thể đã tồn tại.");
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể cập nhật loại đồ uống. Tên loại có thể đã tồn tại.");
            }

            return View(model);
        }

        private void PopulateCategorySelectList(int selectedCategoryId)
        {
            var categories = _categoryBll.GetAll();
            ViewBag.Categories = new SelectList(
                categories,
                "CategoryId",
                "CategoryName",
                selectedCategoryId);
        }
    }
}
