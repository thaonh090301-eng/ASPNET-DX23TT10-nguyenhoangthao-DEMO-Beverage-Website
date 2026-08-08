using System;
using System.Collections.Generic;
using BeverageWebsite.DAL;
using BeverageWebsite.Models;

namespace BeverageWebsite.BLL
{
    /// <summary>
    /// Provides business operations for managing products.
    /// </summary>
    public class ProductBLL
    {
        private const int ProductNameMaxLength = 200;
        private const int DescriptionMaxLength = 1000;
        private const int ImageUrlMaxLength = 500;
        private const int SearchKeywordMaxLength = DescriptionMaxLength;

        private readonly ProductDAL _productDal;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductBLL"/> class.
        /// </summary>
        public ProductBLL()
        {
            _productDal = new ProductDAL();
        }

        /// <summary>
        /// Retrieves all products.
        /// </summary>
        /// <returns>All products returned by the data access layer.</returns>
        public List<Product> GetAll()
        {
            return _productDal.GetAll();
        }

        /// <summary>
        /// Retrieves active products in active categories for the public catalog.
        /// </summary>
        /// <returns>Active products returned by the data access layer.</returns>
        public List<Product> GetActive()
        {
            return _productDal.GetActive();
        }

        /// <summary>
        /// Retrieves a product by its identifier.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <returns>The matching product when found; otherwise, null.</returns>
        public Product GetById(int productId)
        {
            ValidateIdentifier(productId, nameof(productId));
            return _productDal.GetById(productId);
        }

        /// <summary>
        /// Retrieves an active product in an active category by its identifier.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <returns>The matching active product when found; otherwise, null.</returns>
        public Product GetActiveById(int productId)
        {
            ValidateIdentifier(productId, nameof(productId));
            return _productDal.GetActiveById(productId);
        }

        /// <summary>
        /// Retrieves all products in a category.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <returns>The products returned for the category.</returns>
        public List<Product> GetByCategory(int categoryId)
        {
            ValidateIdentifier(categoryId, nameof(categoryId));
            return _productDal.GetByCategory(categoryId);
        }

        /// <summary>
        /// Retrieves active products in an active category for the public catalog.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <returns>The active products returned for the category.</returns>
        public List<Product> GetActiveByCategory(int categoryId)
        {
            ValidateIdentifier(categoryId, nameof(categoryId));
            return _productDal.GetActiveByCategory(categoryId);
        }

        /// <summary>
        /// Searches products using a normalized literal keyword.
        /// </summary>
        /// <param name="keyword">The search keyword.</param>
        /// <returns>The matching products returned by the data access layer.</returns>
        public List<Product> Search(string keyword)
        {
            return _productDal.Search(NormalizeSearchKeyword(keyword));
        }

        /// <summary>
        /// Searches active products in active categories using a normalized literal keyword.
        /// </summary>
        /// <param name="keyword">The search keyword.</param>
        /// <returns>The matching active products returned by the data access layer.</returns>
        public List<Product> SearchActive(string keyword)
        {
            return _productDal.SearchActive(NormalizeSearchKeyword(keyword));
        }

        /// <summary>
        /// Validates, normalizes, and creates a product.
        /// </summary>
        /// <param name="product">The product data to create.</param>
        /// <returns>The number of records affected.</returns>
        public int Create(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            ValidateIdentifier(product.CategoryId, nameof(product.CategoryId));
            ValidatePrice(product.Price);

            var normalizedProduct = new Product
            {
                CategoryId = product.CategoryId,
                ProductName = NormalizeProductName(product.ProductName),
                Description = NormalizeOptionalString(
                    product.Description,
                    DescriptionMaxLength,
                    nameof(product.Description)),
                Price = product.Price,
                ImageUrl = NormalizeOptionalString(
                    product.ImageUrl,
                    ImageUrlMaxLength,
                    nameof(product.ImageUrl)),
                IsActive = product.IsActive
            };

            return _productDal.Insert(normalizedProduct);
        }

        /// <summary>
        /// Validates, normalizes, and updates a product.
        /// </summary>
        /// <param name="product">The product data to update.</param>
        /// <returns>The number of records affected.</returns>
        public int Update(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            ValidateIdentifier(product.ProductId, nameof(product.ProductId));
            ValidateIdentifier(product.CategoryId, nameof(product.CategoryId));
            ValidatePrice(product.Price);

            var normalizedProduct = new Product
            {
                ProductId = product.ProductId,
                CategoryId = product.CategoryId,
                ProductName = NormalizeProductName(product.ProductName),
                Description = NormalizeOptionalString(
                    product.Description,
                    DescriptionMaxLength,
                    nameof(product.Description)),
                Price = product.Price,
                ImageUrl = NormalizeOptionalString(
                    product.ImageUrl,
                    ImageUrlMaxLength,
                    nameof(product.ImageUrl)),
                IsActive = product.IsActive
            };

            return _productDal.Update(normalizedProduct);
        }

        /// <summary>
        /// Deletes a product by its identifier.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <returns>The number of records affected.</returns>
        public int Delete(int productId)
        {
            ValidateIdentifier(productId, nameof(productId));
            return _productDal.Delete(productId);
        }

        private static void ValidateIdentifier(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "The identifier must be greater than zero.");
            }
        }

        private static string NormalizeProductName(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                throw new ArgumentException("Product name is required.", nameof(productName));
            }

            var normalizedProductName = productName.Trim();

            if (normalizedProductName.Length > ProductNameMaxLength)
            {
                throw new ArgumentException(
                    $"Product name cannot exceed {ProductNameMaxLength} characters.",
                    nameof(productName));
            }

            return normalizedProductName;
        }

        private static string NormalizeOptionalString(
            string value,
            int maximumLength,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalizedValue = value.Trim();

            if (normalizedValue.Length > maximumLength)
            {
                throw new ArgumentException(
                    $"The value cannot exceed {maximumLength} characters.",
                    parameterName);
            }

            return normalizedValue;
        }

        private static void ValidatePrice(decimal price)
        {
            if (price < 0m)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(price),
                    "Product price must be greater than or equal to zero.");
            }
        }

        private static string NormalizeSearchKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                throw new ArgumentException("A search keyword must be provided.", nameof(keyword));
            }

            var normalizedKeyword = keyword.Trim();

            if (normalizedKeyword.Length > SearchKeywordMaxLength)
            {
                throw new ArgumentException(
                    $"The search keyword cannot exceed {SearchKeywordMaxLength} characters.",
                    nameof(keyword));
            }

            return normalizedKeyword;
        }
    }
}
