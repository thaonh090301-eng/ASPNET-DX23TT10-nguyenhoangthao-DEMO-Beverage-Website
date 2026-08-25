using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BeverageWebsite.BLL;
using BeverageWebsite.Filters;
using BeverageWebsite.Helpers;
using BeverageWebsite.Models;
using BeverageWebsite.ViewModels;

namespace BeverageWebsite.Controllers
{
    /// <summary>
    /// Provides protected catalog, inventory, and order administration operations.
    /// </summary>
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private const int MaxProductImageLength = 5 * 1024 * 1024;
        private const string ProductImageUploadVirtualDirectory =
            "~/Content/Uploads/Products/";
        private const string ProductImageWebPathPrefix =
            "/Content/Uploads/Products/";

        private static readonly HashSet<string> DangerousEmbeddedExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".asp",
                ".aspx",
                ".bat",
                ".cmd",
                ".com",
                ".config",
                ".cpl",
                ".cshtml",
                ".dll",
                ".exe",
                ".hta",
                ".htm",
                ".html",
                ".jar",
                ".js",
                ".jse",
                ".jsp",
                ".msi",
                ".php",
                ".pif",
                ".ps1",
                ".scr",
                ".svg",
                ".vbs",
                ".wsf",
                ".wsh"
            };

        private readonly CategoryBLL _categoryBll;
        private readonly ProductBLL _productBll;
        private readonly InventoryBLL _inventoryBll;
        private readonly OrderBLL _orderBll;
        private readonly UserBLL _userBll;
        private readonly AddressBLL _addressBll;

        /// <summary>
        /// Initializes the controller for protected administration operations.
        /// </summary>
        public AdminController()
        {
            _categoryBll = new CategoryBLL();
            _productBll = new ProductBLL();
            _inventoryBll = new InventoryBLL();
            _orderBll = new OrderBLL();
            _userBll = new UserBLL();
            _addressBll = new AddressBLL();
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
            var orders = _orderBll.GetAll();
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
                OutOfStockProductCount = products.Count - inStockProductCount,
                PendingOrderCount = orders.Count(order =>
                    string.Equals(
                        order.OrderStatus,
                        "Pending",
                        StringComparison.Ordinal))
            };

            return View(viewModel);
        }

        /// <summary>
        /// Displays customer orders for the selected Vietnam-date and status filters.
        /// </summary>
        /// <param name="dateFilter">The supported operational date filter.</param>
        /// <param name="status">The optional exact order status.</param>
        /// <returns>The order management view.</returns>
        [HttpGet]
        public ActionResult Orders(string dateFilter, string status)
        {
            var orders = _orderBll.GetAll();
            var normalizedDateFilter = NormalizeAdminOrderDateFilter(dateFilter);
            var normalizedStatus = NormalizeAdminOrderStatusFilter(status);
            var vietnamToday = VietnamTimeHelper.VietnamToday;
            var localizedOrders = orders.Select(order => new
            {
                Order = order,
                VietnamOrderDate = VietnamTimeHelper.FromUtc(order.OrderDate)
            })
            .ToList();

            var olderUnfinishedOrderCount = localizedOrders.Count(item =>
                item.VietnamOrderDate.Date < vietnamToday
                && IsUnfinishedOrderStatus(item.Order.OrderStatus));
            var filteredOrders = localizedOrders.AsEnumerable();

            switch (normalizedDateFilter)
            {
                case "yesterday":
                    var yesterday = vietnamToday.AddDays(-1);
                    filteredOrders = filteredOrders.Where(item =>
                        item.VietnamOrderDate.Date == yesterday);
                    break;
                case "last7days":
                    var firstIncludedDate = vietnamToday.AddDays(-6);
                    filteredOrders = filteredOrders.Where(item =>
                        item.VietnamOrderDate.Date >= firstIncludedDate
                        && item.VietnamOrderDate.Date <= vietnamToday);
                    break;
                case "all":
                    break;
                default:
                    filteredOrders = filteredOrders.Where(item =>
                        item.VietnamOrderDate.Date == vietnamToday);
                    break;
            }

            if (!string.IsNullOrEmpty(normalizedStatus))
            {
                filteredOrders = filteredOrders.Where(item =>
                    string.Equals(
                        item.Order.OrderStatus,
                        normalizedStatus,
                        StringComparison.Ordinal));
            }

            var userById = _userBll
                .GetAll()
                .ToDictionary(user => user.UserId);
            var viewModels = filteredOrders
                .OrderByDescending(item => item.VietnamOrderDate)
                .ThenByDescending(item => item.Order.OrderId)
                .Select(item =>
            {
                var order = item.Order;
                User customer;
                userById.TryGetValue(order.UserId, out customer);

                return new AdminOrderListItemViewModel
                {
                    OrderId = order.OrderId,
                    OrderDate = item.VietnamOrderDate,
                    CustomerName = GetCustomerName(customer),
                    CustomerEmail = GetPresentationValue(
                        customer == null ? null : customer.Email),
                    OrderStatus = order.OrderStatus,
                    OrderStatusDisplayName = GetOrderStatusDisplayName(
                        order.OrderStatus),
                    FinalAmount = order.FinalAmount
                };
            })
            .ToList();

            ViewBag.ActiveDateFilter = normalizedDateFilter;
            ViewBag.ActiveStatus = normalizedStatus;
            ViewBag.OlderUnfinishedOrderCount = olderUnfinishedOrderCount;

            return View(viewModels);
        }

        /// <summary>
        /// Displays complete customer, delivery, item, and status information for an order.
        /// </summary>
        /// <param name="id">The order identifier.</param>
        /// <returns>The order details view or an HTTP error result.</returns>
        [HttpGet]
        public ActionResult OrderDetails(int id)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = _orderBll.GetById(id);

            if (order == null)
            {
                return HttpNotFound();
            }

            var orderItems = _orderBll.GetOrderItems(id);
            var customer = _userBll.GetById(order.UserId);
            var address = _addressBll.GetById(order.AddressId);
            var productById = GetProductLookupForOrderPresentation();
            var pending = string.Equals(
                order.OrderStatus,
                "Pending",
                StringComparison.Ordinal);
            var confirmed = string.Equals(
                order.OrderStatus,
                "Confirmed",
                StringComparison.Ordinal);
            var processing = string.Equals(
                order.OrderStatus,
                "Processing",
                StringComparison.Ordinal);

            var viewModel = new AdminOrderDetailsViewModel
            {
                OrderId = order.OrderId,
                OrderDate = VietnamTimeHelper.FromUtc(order.OrderDate),
                OrderStatus = order.OrderStatus,
                OrderStatusDisplayName = GetOrderStatusDisplayName(
                    order.OrderStatus),
                TotalAmount = order.TotalAmount,
                ShippingFee = order.ShippingFee,
                FinalAmount = order.FinalAmount,
                CustomerName = GetCustomerName(customer),
                CustomerEmail = GetPresentationValue(
                    customer == null ? null : customer.Email),
                CustomerPhone = GetPresentationValue(
                    customer == null ? null : customer.Phone),
                RecipientName = GetPresentationValue(
                    address == null ? null : address.RecipientName),
                RecipientPhone = GetPresentationValue(
                    address == null ? null : address.Phone),
                Street = address == null ? null : address.Street,
                Ward = address == null ? null : address.Ward,
                District = address == null ? null : address.District,
                City = address == null ? null : address.City,
                CanConfirm = pending,
                CanProcess = confirmed,
                CanComplete = processing,
                CanCancel = pending || confirmed,
                Items = orderItems.Select(item =>
                {
                    Product product;
                    var hasProduct = productById.TryGetValue(
                        item.ProductId,
                        out product);
                    var hasProductName = hasProduct
                        && !string.IsNullOrWhiteSpace(product.ProductName);

                    return new AdminOrderDetailItemViewModel
                    {
                        ProductId = item.ProductId,
                        ProductName = hasProductName
                            ? product.ProductName
                            : "Sản phẩm không khả dụng",
                        ImageUrl = hasProduct
                            && !string.IsNullOrWhiteSpace(product.ImageUrl)
                                ? product.ImageUrl
                                : null,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountAmount = item.DiscountAmount,
                        LineTotal = item.LineTotal
                    };
                })
                .ToList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Applies one valid administrative order-status transition.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <param name="newStatus">The requested next order status.</param>
        /// <returns>A redirect to the order details view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrderStatus(int orderId, string newStatus)
        {
            const string errorMessage =
                "Không thể cập nhật trạng thái đơn hàng. Vui lòng tải lại đơn và kiểm tra trạng thái hiện tại.";

            if (orderId <= 0)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(
                    "OrderDetails",
                    "Admin",
                    new { id = orderId });
            }

            try
            {
                _orderBll.ChangeStatus(orderId, newStatus);

                var normalizedStatus = string.IsNullOrWhiteSpace(newStatus)
                    ? string.Empty
                    : newStatus.Trim();

                switch (normalizedStatus)
                {
                    case "Confirmed":
                        TempData["SuccessMessage"] = "Đã xác nhận đơn hàng.";
                        break;
                    case "Processing":
                        TempData["SuccessMessage"] =
                            "Đơn hàng đã chuyển sang trạng thái đang chuẩn bị.";
                        break;
                    case "Completed":
                        TempData["SuccessMessage"] = "Đã hoàn thành đơn hàng.";
                        break;
                    case "Cancelled":
                        TempData["SuccessMessage"] =
                            "Đã hủy đơn hàng và hoàn lại tồn kho.";
                        break;
                }
            }
            catch (ArgumentException)
            {
                TempData["ErrorMessage"] = errorMessage;
            }
            catch (InvalidOperationException)
            {
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction(
                "OrderDetails",
                "Admin",
                new { id = orderId });
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
                    IsActive = product.IsActive,
                    IsFeatured = product.IsFeatured,
                    BadgeType = product.BadgeType
                };
            })
            .OrderBy(item => item.CategoryName)
            .ThenBy(item => item.ProductName)
            .ToList();

            return View(viewModels);
        }

        /// <summary>
        /// Displays the form for creating a product and its initial inventory.
        /// </summary>
        /// <returns>The product creation view or the category management view.</returns>
        [HttpGet]
        public ActionResult CreateProduct()
        {
            var categories = _categoryBll.GetAll();

            if (categories.Count == 0)
            {
                TempData["ErrorMessage"] =
                    "Cần tạo loại đồ uống trước khi thêm sản phẩm.";
                return RedirectToAction("Categories", "Admin");
            }

            PopulateProductSelectLists(categories, 0, null);

            return View(new AdminProductCreateViewModel
            {
                IsActive = true,
                StockQuantity = 50,
                ReorderLevel = 10
            });
        }

        /// <summary>
        /// Creates a product and its initial inventory in one transaction.
        /// </summary>
        /// <param name="model">The submitted product and inventory data.</param>
        /// <param name="imageFile">The optional product image upload.</param>
        /// <returns>The creation view on failure or a redirect on success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProduct(
            AdminProductCreateViewModel model,
            HttpPostedFileBase imageFile)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ModelState.Remove("ImageUrl");
            model.ImageUrl = null;

            if (!ModelState.IsValid)
            {
                PopulateProductSelectLists(model.CategoryId, model.BadgeType);
                return View(model);
            }

            Category category = null;

            try
            {
                if (model.CategoryId > 0)
                {
                    category = _categoryBll.GetById(model.CategoryId);
                }
            }
            catch (ArgumentException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể thêm sản phẩm. Vui lòng kiểm tra dữ liệu.");
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể thêm sản phẩm. Vui lòng kiểm tra dữ liệu.");
            }

            if (!ModelState.IsValid)
            {
                PopulateProductSelectLists(model.CategoryId, model.BadgeType);
                return View(model);
            }

            if (category == null)
            {
                ModelState.AddModelError(
                    "CategoryId",
                    "Loại đồ uống không hợp lệ.");
                PopulateProductSelectLists(model.CategoryId, model.BadgeType);
                return View(model);
            }

            string imageExtension = null;
            string imageValidationError = null;
            var hasImage = HasProductImage(imageFile);

            if (hasImage
                && !TryValidateProductImage(
                    imageFile,
                    out imageExtension,
                    out imageValidationError))
            {
                ModelState.AddModelError("imageFile", imageValidationError);
                PopulateProductSelectLists(model.CategoryId, model.BadgeType);
                return View(model);
            }

            string uploadedImageUrl = null;

            if (hasImage
                && !TrySaveProductImage(
                    imageFile,
                    imageExtension,
                    out uploadedImageUrl))
            {
                ModelState.AddModelError(
                    "imageFile",
                    "Không thể lưu ảnh sản phẩm. Vui lòng thử lại.");
                PopulateProductSelectLists(model.CategoryId, model.BadgeType);
                return View(model);
            }

            var product = new Product
            {
                CategoryId = model.CategoryId,
                ProductName = model.ProductName,
                Description = model.Description,
                Price = model.Price,
                ImageUrl = uploadedImageUrl,
                IsActive = model.IsActive,
                IsFeatured = model.IsFeatured,
                BadgeType = model.BadgeType
            };

            try
            {
                _productBll.CreateWithInventory(
                    product,
                    model.StockQuantity,
                    model.ReorderLevel);

                TempData["SuccessMessage"] = "Đã thêm sản phẩm mới.";
                return RedirectToAction("Products", "Admin");
            }
            catch (Exception)
            {
                TryDeleteProductImage(uploadedImageUrl);
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể thêm sản phẩm. Vui lòng kiểm tra dữ liệu.");
            }

            PopulateProductSelectLists(model.CategoryId, model.BadgeType);
            return View(model);
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
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured,
                BadgeType = product.BadgeType
            };

            PopulateProductSelectLists(model.CategoryId, model.BadgeType);
            return View(model);
        }

        /// <summary>
        /// Updates editable fields for an existing product.
        /// </summary>
        /// <param name="model">The submitted product data.</param>
        /// <param name="imageFile">The optional replacement product image.</param>
        /// <returns>The edit view on failure or a redirect on success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(
            AdminProductEditViewModel model,
            HttpPostedFileBase imageFile)
        {
            if (model == null || model.ProductId <= 0)
            {
                return HttpNotFound();
            }

            ModelState.Remove("ImageUrl");
            model.ImageUrl = null;

            Product existing;

            try
            {
                existing = _productBll.GetById(model.ProductId);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể cập nhật sản phẩm. Vui lòng kiểm tra dữ liệu.");
                PopulateProductSelectLists(model.CategoryId, model.BadgeType);
                return View(model);
            }

            if (existing == null)
            {
                return HttpNotFound();
            }

            model.ImageUrl = existing.ImageUrl;

            if (!ModelState.IsValid)
            {
                PopulateProductSelectLists(model.CategoryId, model.BadgeType);
                return View(model);
            }

            Category category = null;

            try
            {
                if (model.CategoryId > 0)
                {
                    category = _categoryBll.GetById(model.CategoryId);
                }
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

            if (!ModelState.IsValid)
            {
                PopulateProductSelectLists(model.CategoryId, model.BadgeType);
                return View(model);
            }

            if (category == null)
            {
                ModelState.AddModelError(
                    "CategoryId",
                    "Loại đồ uống không hợp lệ.");
                PopulateProductSelectLists(model.CategoryId, model.BadgeType);
                return View(model);
            }

            string imageExtension = null;
            string imageValidationError = null;
            var hasNewImage = HasProductImage(imageFile);

            if (hasNewImage
                && !TryValidateProductImage(
                    imageFile,
                    out imageExtension,
                    out imageValidationError))
            {
                ModelState.AddModelError("imageFile", imageValidationError);
                PopulateProductSelectLists(model.CategoryId, model.BadgeType);
                return View(model);
            }

            string uploadedImageUrl = null;

            if (hasNewImage
                && !TrySaveProductImage(
                    imageFile,
                    imageExtension,
                    out uploadedImageUrl))
            {
                ModelState.AddModelError(
                    "imageFile",
                    "Không thể lưu ảnh sản phẩm. Vui lòng thử lại.");
                PopulateProductSelectLists(model.CategoryId, model.BadgeType);
                return View(model);
            }

            var product = new Product
            {
                ProductId = existing.ProductId,
                CategoryId = model.CategoryId,
                ProductName = model.ProductName,
                Description = model.Description,
                Price = model.Price,
                ImageUrl = uploadedImageUrl ?? existing.ImageUrl,
                IsActive = model.IsActive,
                IsFeatured = model.IsFeatured,
                BadgeType = model.BadgeType
            };

            try
            {
                _productBll.Update(product);
            }
            catch (Exception)
            {
                TryDeleteProductImage(uploadedImageUrl);
                model.ImageUrl = existing.ImageUrl;
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể cập nhật sản phẩm. Vui lòng kiểm tra dữ liệu.");
                PopulateProductSelectLists(model.CategoryId, model.BadgeType);
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(uploadedImageUrl)
                && !string.Equals(
                    uploadedImageUrl,
                    existing.ImageUrl,
                    StringComparison.OrdinalIgnoreCase))
            {
                TryDeleteProductImage(existing.ImageUrl);
            }

            TempData["SuccessMessage"] = "Đã cập nhật sản phẩm.";
            return RedirectToAction("Products", "Admin");
        }

        /// <summary>
        /// Permanently deletes an unused product and its inventory record.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <returns>A redirect to the product management view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProduct(int productId)
        {
            if (productId <= 0)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("Products", "Admin");
            }

            try
            {
                var product = _productBll.GetById(productId);

                if (product == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction("Products", "Admin");
                }

                if (_productBll.DeleteIfUnused(productId))
                {
                    TempData["SuccessMessage"] = "Đã xóa sản phẩm.";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        "Không thể xóa sản phẩm vì đã phát sinh dữ liệu. Hãy chuyển sản phẩm sang trạng thái Ngừng bán.";
                }
            }
            catch (ArgumentException)
            {
                TempData["ErrorMessage"] =
                    "Không thể xóa sản phẩm. Vui lòng thử lại.";
            }
            catch (InvalidOperationException)
            {
                TempData["ErrorMessage"] =
                    "Không thể xóa sản phẩm. Vui lòng thử lại.";
            }

            return RedirectToAction("Products", "Admin");
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

        /// <summary>
        /// Permanently deletes a category that contains no products.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <returns>A redirect to the category management view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCategory(int categoryId)
        {
            if (categoryId <= 0)
            {
                TempData["ErrorMessage"] = "Không tìm thấy loại đồ uống.";
                return RedirectToAction("Categories", "Admin");
            }

            try
            {
                var category = _categoryBll.GetById(categoryId);

                if (category == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy loại đồ uống.";
                    return RedirectToAction("Categories", "Admin");
                }

                if (_categoryBll.DeleteIfEmpty(categoryId))
                {
                    TempData["SuccessMessage"] = "Đã xóa loại đồ uống.";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        "Không thể xóa loại đồ uống vì vẫn còn sản phẩm thuộc loại này.";
                }
            }
            catch (ArgumentException)
            {
                TempData["ErrorMessage"] =
                    "Không thể xóa loại đồ uống. Vui lòng thử lại.";
            }
            catch (InvalidOperationException)
            {
                TempData["ErrorMessage"] =
                    "Không thể xóa loại đồ uống. Vui lòng thử lại.";
            }

            return RedirectToAction("Categories", "Admin");
        }

        private void PopulateProductSelectLists(
            int selectedCategoryId,
            string selectedBadgeType)
        {
            var categories = _categoryBll.GetAll();
            PopulateProductSelectLists(
                categories,
                selectedCategoryId,
                selectedBadgeType);
        }

        private void PopulateProductSelectLists(
            IEnumerable<Category> categories,
            int selectedCategoryId,
            string selectedBadgeType)
        {
            ViewBag.Categories = new SelectList(
                categories,
                "CategoryId",
                "CategoryName",
                selectedCategoryId);

            var badgeTypes = new[]
            {
                new SelectListItem
                {
                    Value = string.Empty,
                    Text = "Không có nhãn"
                },
                new SelectListItem
                {
                    Value = "Featured",
                    Text = "Món nổi bật"
                },
                new SelectListItem
                {
                    Value = "BestSeller",
                    Text = "Best seller"
                },
                new SelectListItem
                {
                    Value = "New",
                    Text = "Món mới"
                }
            };

            ViewBag.BadgeTypes = new SelectList(
                badgeTypes,
                "Value",
                "Text",
                selectedBadgeType);
        }

        private Dictionary<int, Product> GetProductLookupForOrderPresentation()
        {
            try
            {
                return _productBll
                    .GetAll()
                    .ToDictionary(product => product.ProductId);
            }
            catch (InvalidOperationException)
            {
                return new Dictionary<int, Product>();
            }
        }

        private static string GetCustomerName(User customer)
        {
            if (customer == null)
            {
                return "Không xác định";
            }

            if (!string.IsNullOrWhiteSpace(customer.FullName))
            {
                return customer.FullName;
            }

            return GetPresentationValue(customer.UserName);
        }

        private static string GetPresentationValue(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "Không xác định"
                : value;
        }

        private static string GetOrderStatusDisplayName(string orderStatus)
        {
            if (string.Equals(orderStatus, "Pending", StringComparison.Ordinal))
            {
                return "Chờ xác nhận";
            }

            if (string.Equals(orderStatus, "Confirmed", StringComparison.Ordinal))
            {
                return "Đã xác nhận";
            }

            if (string.Equals(orderStatus, "Processing", StringComparison.Ordinal))
            {
                return "Đang chuẩn bị";
            }

            if (string.Equals(orderStatus, "Completed", StringComparison.Ordinal))
            {
                return "Hoàn thành";
            }

            if (string.Equals(orderStatus, "Cancelled", StringComparison.Ordinal))
            {
                return "Đã hủy";
            }

            return "Không xác định";
        }

        private static string NormalizeAdminOrderDateFilter(string dateFilter)
        {
            if (string.IsNullOrWhiteSpace(dateFilter))
            {
                return "today";
            }

            switch (dateFilter.Trim().ToLowerInvariant())
            {
                case "today":
                    return "today";
                case "yesterday":
                    return "yesterday";
                case "last7days":
                    return "last7days";
                case "all":
                    return "all";
                default:
                    return "today";
            }
        }

        private static string NormalizeAdminOrderStatusFilter(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return string.Empty;
            }

            var normalizedStatus = status.Trim();
            var supportedStatuses = new[]
            {
                "Pending",
                "Confirmed",
                "Processing",
                "Completed",
                "Cancelled"
            };

            return supportedStatuses.FirstOrDefault(supportedStatus =>
                string.Equals(
                    supportedStatus,
                    normalizedStatus,
                    StringComparison.OrdinalIgnoreCase))
                ?? string.Empty;
        }

        private static bool IsUnfinishedOrderStatus(string status)
        {
            return string.Equals(status, "Pending", StringComparison.Ordinal)
                || string.Equals(status, "Confirmed", StringComparison.Ordinal)
                || string.Equals(status, "Processing", StringComparison.Ordinal);
        }

        // Returns true when an uploaded file was provided.
        private static bool HasProductImage(HttpPostedFileBase imageFile)
        {
            return imageFile != null && imageFile.ContentLength > 0;
        }

        // Validates image file size, extension, and magic bytes. Returns the extension (including dot) on success.
        private static bool TryValidateProductImage(
            HttpPostedFileBase imageFile,
            out string imageExtension,
            out string errorMessage)
        {
            imageExtension = null;
            errorMessage = null;

            if (imageFile == null)
            {
                errorMessage = "Không có file ảnh được gửi.";
                return false;
            }

            if (imageFile.ContentLength <= 0)
            {
                errorMessage = "File được chọn không hợp lệ.";
                return false;
            }

            if (imageFile.ContentLength > MaxProductImageLength)
            {
                errorMessage = "Ảnh sản phẩm không được vượt quá 5 MB.";
                return false;
            }

            var fileName = Path.GetFileName(imageFile.FileName ?? string.Empty);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                errorMessage = "File được chọn không hợp lệ.";
                return false;
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            // Reject SVG explicitly and require jpg/jpeg/png/webp
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            if (!allowed.Contains(ext))
            {
                errorMessage = "Chỉ hỗ trợ ảnh JPG, JPEG, PNG hoặc WEBP.";
                return false;
            }

            // Block suspicious filenames with embedded dangerous extensions (e.g. shell.php.jpg)
            var lowerName = fileName.ToLowerInvariant();
            foreach (var dangerousExt in DangerousEmbeddedExtensions)
            {
                // check for patterns like ".php." or filename ending with dangerous ext (no final extension)
                if (lowerName.Contains(dangerousExt + ".") || lowerName.EndsWith(dangerousExt, StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = "Tên file chứa phần mở rộng không an toàn.";
                    return false;
                }
            }

            // Read magic bytes
            try
            {
                var input = imageFile.InputStream;
                if (input == null)
                {
                    errorMessage = "File được chọn không hợp lệ.";
                    return false;
                }

                // Read up to 12 bytes to cover signatures for PNG/JPEG/WEBP
                var signature = new byte[12];
                var bytesRead = 0;
                try
                {
                    // if stream supports seek, remember and reset position after read
                    long originalPosition = 0;
                    if (input.CanSeek)
                    {
                        originalPosition = input.Position;
                        input.Position = 0;
                    }

                    bytesRead = input.Read(signature, 0, signature.Length);

                    if (input.CanSeek)
                    {
                        input.Position = originalPosition;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    errorMessage = "Không thể đọc file ảnh.";
                    return false;
                }

                // JPEG: FF D8 FF
                if (bytesRead >= 3 && signature[0] == 0xFF && signature[1] == 0xD8 && signature[2] == 0xFF)
                {
                    imageExtension = ext;
                    return true;
                }

                // PNG signature: 89 50 4E 47 0D 0A 1A 0A
                var pngSig = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
                if (bytesRead >= pngSig.Length)
                {
                    var ok = true;
                    for (int i = 0; i < pngSig.Length; i++)
                    {
                        if (signature[i] != pngSig[i])
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (ok)
                    {
                        imageExtension = ext;
                        return true;
                    }
                }

                // WEBP: "RIFF" at 0..3 and "WEBP" at 8..11
                if (bytesRead >= 12)
                {
                    if (signature[0] == (byte)'R' && signature[1] == (byte)'I' && signature[2] == (byte)'F' && signature[3] == (byte)'F'
                        && signature[8] == (byte)'W' && signature[9] == (byte)'E' && signature[10] == (byte)'B' && signature[11] == (byte)'P')
                    {
                        imageExtension = ext;
                        return true;
                    }
                }

                errorMessage = "File được chọn không phải là ảnh hợp lệ.";
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                errorMessage = "Đã xảy ra lỗi khi kiểm tra file ảnh.";
                return false;
            }
        }

        private bool TrySaveProductImage(
            HttpPostedFileBase imageFile,
            string imageExtension,
            out string uploadedImageUrl)
        {
            uploadedImageUrl = null;

            if (imageFile == null || string.IsNullOrWhiteSpace(imageExtension))
            {
                return false;
            }

            try
            {
                var uploadPhysicalDir = Server.MapPath(ProductImageUploadVirtualDirectory);

                if (string.IsNullOrWhiteSpace(uploadPhysicalDir))
                {
                    System.Diagnostics.Debug.WriteLine("Server.MapPath returned null or empty for upload directory.");
                    return false;
                }

                if (!Directory.Exists(uploadPhysicalDir))
                {
                    Directory.CreateDirectory(uploadPhysicalDir);
                }

                var fileName = Guid.NewGuid().ToString("N") + imageExtension.ToLowerInvariant();
                var physicalPath = Path.Combine(uploadPhysicalDir, fileName);

                // SaveAs will overwrite if exists - but name is GUID so collision is extremely unlikely
                imageFile.SaveAs(physicalPath);

                if (System.IO.File.Exists(physicalPath))
                {
                    uploadedImageUrl = ProductImageWebPathPrefix + fileName;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                uploadedImageUrl = null;
                return false;
            }
        }

        private void TryDeleteProductImage(string imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return;
                }

                // Only delete images inside configured web path prefix
                if (!imageUrl.StartsWith(ProductImageWebPathPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var fileName = Path.GetFileName(imageUrl);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return;
                }

                var physicalPath = Server.MapPath(ProductImageUploadVirtualDirectory + fileName);
                if (string.IsNullOrWhiteSpace(physicalPath))
                {
                    return;
                }

                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            catch (Exception ex)
            {
                // Log but do not propagate - deletion failure must not break update flow                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}
