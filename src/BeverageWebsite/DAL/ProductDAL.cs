using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BeverageWebsite.Models;

namespace BeverageWebsite.DAL
{
    /// <summary>
    /// Data access layer for Product operations using ADO.NET and the shared DataProvider.
    /// </summary>
    public class ProductDAL
    {
        private const int ProductNameMaxLength = 200;
        private const int DescriptionMaxLength = 1000;
        private const int ImageUrlMaxLength = 500;
        private const int SearchKeywordMaxLength = DescriptionMaxLength;
        private const int EscapedSearchKeywordMaxLength =
            SearchKeywordMaxLength * 2;
        private const int SearchPatternMaxLength =
            EscapedSearchKeywordMaxLength + 2;

        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDAL"/> class.
        /// </summary>
        public ProductDAL()
        {
            _dataProvider = new DataProvider();
        }

        /// <summary>
        /// Retrieves all products.
        /// </summary>
        /// <returns>A list of <see cref="Product"/> objects.</returns>
        public List<Product> GetAll()
        {
            var products = new List<Product>();
            const string sql = @"SELECT ProductId, CategoryId, ProductName, Description, Price, ImageUrl, IsActive, CreatedAt FROM dbo.Product ORDER BY ProductName";

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql))
                {
                    while (reader.Read())
                    {
                        products.Add(MapProduct(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve products.", ex);
            }

            return products;
        }

        /// <summary>
        /// Retrieves active products in active categories for the public catalog.
        /// </summary>
        /// <returns>A list of active <see cref="Product"/> objects.</returns>
        public List<Product> GetActive()
        {
            var products = new List<Product>();
            const string sql = @"
SELECT
    P.ProductId,
    P.CategoryId,
    P.ProductName,
    P.Description,
    P.Price,
    P.ImageUrl,
    P.IsActive,
    P.CreatedAt
FROM dbo.Product AS P
INNER JOIN dbo.Category AS C
    ON C.CategoryId = P.CategoryId
WHERE P.IsActive = 1
  AND C.IsActive = 1
ORDER BY P.ProductName, P.ProductId";

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql))
                {
                    while (reader.Read())
                    {
                        products.Add(MapProduct(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve active products.", ex);
            }

            return products;
        }

        /// <summary>
        /// Retrieves a product by its identifier.
        /// </summary>
        /// <param name="id">The product identifier.</param>
        /// <returns>A <see cref="Product"/> object if found; otherwise, null.</returns>
        public Product GetById(int id)
        {
            ValidateIdentifier(id, nameof(id));

            const string sql = @"SELECT ProductId, CategoryId, ProductName, Description, Price, ImageUrl, IsActive, CreatedAt FROM dbo.Product WHERE ProductId = @ProductId";
            var parameters = new[]
            {
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = id }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapProduct(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve the product.", ex);
            }

            return null;
        }

        /// <summary>
        /// Retrieves an active product in an active category by its identifier.
        /// </summary>
        /// <param name="id">The product identifier.</param>
        /// <returns>An active <see cref="Product"/> object if found; otherwise, null.</returns>
        public Product GetActiveById(int id)
        {
            ValidateIdentifier(id, nameof(id));

            const string sql = @"
SELECT
    P.ProductId,
    P.CategoryId,
    P.ProductName,
    P.Description,
    P.Price,
    P.ImageUrl,
    P.IsActive,
    P.CreatedAt
FROM dbo.Product AS P
INNER JOIN dbo.Category AS C
    ON C.CategoryId = P.CategoryId
WHERE P.ProductId = @ProductId
  AND P.IsActive = 1
  AND C.IsActive = 1";
            var parameters = new[]
            {
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = id }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapProduct(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve the active product.", ex);
            }

            return null;
        }

        /// <summary>
        /// Retrieves products by category identifier.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <returns>A list of <see cref="Product"/> objects.</returns>
        public List<Product> GetByCategory(int categoryId)
        {
            ValidateIdentifier(categoryId, nameof(categoryId));

            var products = new List<Product>();
            const string sql = @"SELECT ProductId, CategoryId, ProductName, Description, Price, ImageUrl, IsActive, CreatedAt FROM dbo.Product WHERE CategoryId = @CategoryId ORDER BY ProductName";
            var parameters = new[]
            {
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    while (reader.Read())
                    {
                        products.Add(MapProduct(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve products for the category.", ex);
            }

            return products;
        }

        /// <summary>
        /// Retrieves active products in an active category for the public catalog.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <returns>A list of active <see cref="Product"/> objects.</returns>
        public List<Product> GetActiveByCategory(int categoryId)
        {
            ValidateIdentifier(categoryId, nameof(categoryId));

            var products = new List<Product>();
            const string sql = @"
SELECT
    P.ProductId,
    P.CategoryId,
    P.ProductName,
    P.Description,
    P.Price,
    P.ImageUrl,
    P.IsActive,
    P.CreatedAt
FROM dbo.Product AS P
INNER JOIN dbo.Category AS C
    ON C.CategoryId = P.CategoryId
WHERE P.CategoryId = @CategoryId
  AND P.IsActive = 1
  AND C.IsActive = 1
ORDER BY P.ProductName, P.ProductId";
            var parameters = new[]
            {
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    while (reader.Read())
                    {
                        products.Add(MapProduct(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve active products for the category.", ex);
            }

            return products;
        }

        /// <summary>
        /// Searches products by keyword in the product name or description.
        /// </summary>
        /// <param name="keyword">The keyword to search for.</param>
        /// <returns>A list of <see cref="Product"/> objects.</returns>
        public List<Product> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                throw new ArgumentException("A search keyword must be provided.", nameof(keyword));
            }

            var normalizedKeyword = keyword.Trim();

            if (normalizedKeyword.Length > SearchKeywordMaxLength)
            {
                throw new ArgumentException("The search keyword exceeds the allowed maximum length.", nameof(keyword));
            }

            var products = new List<Product>();
            const string sql = @"
SELECT
    P.ProductId,
    P.CategoryId,
    P.ProductName,
    P.Description,
    P.Price,
    P.ImageUrl,
    P.IsActive,
    P.CreatedAt
FROM dbo.Product AS P
WHERE (P.ProductName LIKE @Keyword ESCAPE N'\'
       OR P.Description LIKE @Keyword ESCAPE N'\')
ORDER BY P.ProductName, P.ProductId";
            var parameters = new[]
            {
                new SqlParameter("@Keyword", SqlDbType.NVarChar, SearchPatternMaxLength)
                {
                    Value = $"%{EscapeLikePattern(normalizedKeyword)}%"
                }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    while (reader.Read())
                    {
                        products.Add(MapProduct(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to search products.", ex);
            }

            return products;
        }

        /// <summary>
        /// Searches active products in active categories by a literal keyword.
        /// </summary>
        /// <param name="keyword">The keyword to search for.</param>
        /// <returns>A list of matching active <see cref="Product"/> objects.</returns>
        public List<Product> SearchActive(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                throw new ArgumentException("A search keyword must be provided.", nameof(keyword));
            }

            var normalizedKeyword = keyword.Trim();

            if (normalizedKeyword.Length > SearchKeywordMaxLength)
            {
                throw new ArgumentException("The search keyword exceeds the allowed maximum length.", nameof(keyword));
            }

            var products = new List<Product>();
            const string sql = @"
SELECT
    P.ProductId,
    P.CategoryId,
    P.ProductName,
    P.Description,
    P.Price,
    P.ImageUrl,
    P.IsActive,
    P.CreatedAt
FROM dbo.Product AS P
INNER JOIN dbo.Category AS C
    ON C.CategoryId = P.CategoryId
WHERE P.IsActive = 1
  AND C.IsActive = 1
  AND (P.ProductName LIKE @Keyword ESCAPE N'\'
       OR P.Description LIKE @Keyword ESCAPE N'\')
ORDER BY P.ProductName, P.ProductId";
            var parameters = new[]
            {
                new SqlParameter("@Keyword", SqlDbType.NVarChar, SearchPatternMaxLength)
                {
                    Value = $"%{EscapeLikePattern(normalizedKeyword)}%"
                }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    while (reader.Read())
                    {
                        products.Add(MapProduct(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to search active products.", ex);
            }

            return products;
        }

        private static string EscapeLikePattern(string value)
        {
            return value
                .Replace(@"\", @"\\")
                .Replace("%", @"\%")
                .Replace("_", @"\_")
                .Replace("[", @"\[");
        }

        /// <summary>
        /// Inserts a new product into the database.
        /// </summary>
        /// <param name="product">The product to insert.</param>
        /// <returns>The number of rows affected.</returns>
        public int Insert(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            ValidateIdentifier(product.CategoryId, nameof(product.CategoryId));

            if (product.Price < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(product.Price), "Product price must be greater than or equal to zero.");
            }

            var productName = NormalizeRequiredString(
                product.ProductName,
                ProductNameMaxLength,
                nameof(product.ProductName));
            var description = NormalizeOptionalString(
                product.Description,
                DescriptionMaxLength,
                nameof(product.Description));
            var imageUrl = NormalizeOptionalString(
                product.ImageUrl,
                ImageUrlMaxLength,
                nameof(product.ImageUrl));

            const string sql = @"INSERT INTO dbo.Product (CategoryId, ProductName, Description, Price, ImageUrl, IsActive) VALUES (@CategoryId, @ProductName, @Description, @Price, @ImageUrl, @IsActive)";
            var parameters = new[]
            {
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = product.CategoryId },
                new SqlParameter("@ProductName", SqlDbType.NVarChar, ProductNameMaxLength) { Value = productName },
                new SqlParameter("@Description", SqlDbType.NVarChar, DescriptionMaxLength) { Value = (object)description ?? DBNull.Value },
                CreatePriceParameter(product.Price),
                new SqlParameter("@ImageUrl", SqlDbType.NVarChar, ImageUrlMaxLength) { Value = (object)imageUrl ?? DBNull.Value },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = product.IsActive }
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to insert the product.", ex);
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The product insert did not affect exactly one record.");
            }

            return affectedRows;
        }

        /// <summary>
        /// Updates an existing product in the database.
        /// </summary>
        /// <param name="product">The product to update.</param>
        /// <returns>The number of rows affected.</returns>
        public int Update(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            ValidateIdentifier(product.ProductId, nameof(product.ProductId));
            ValidateIdentifier(product.CategoryId, nameof(product.CategoryId));

            if (product.Price < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(product.Price), "Product price must be greater than or equal to zero.");
            }

            var productName = NormalizeRequiredString(
                product.ProductName,
                ProductNameMaxLength,
                nameof(product.ProductName));
            var description = NormalizeOptionalString(
                product.Description,
                DescriptionMaxLength,
                nameof(product.Description));
            var imageUrl = NormalizeOptionalString(
                product.ImageUrl,
                ImageUrlMaxLength,
                nameof(product.ImageUrl));

            const string sql = @"UPDATE dbo.Product SET CategoryId = @CategoryId, ProductName = @ProductName, Description = @Description, Price = @Price, ImageUrl = @ImageUrl, IsActive = @IsActive WHERE ProductId = @ProductId";
            var parameters = new[]
            {
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = product.ProductId },
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = product.CategoryId },
                new SqlParameter("@ProductName", SqlDbType.NVarChar, ProductNameMaxLength) { Value = productName },
                new SqlParameter("@Description", SqlDbType.NVarChar, DescriptionMaxLength) { Value = (object)description ?? DBNull.Value },
                CreatePriceParameter(product.Price),
                new SqlParameter("@ImageUrl", SqlDbType.NVarChar, ImageUrlMaxLength) { Value = (object)imageUrl ?? DBNull.Value },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = product.IsActive }
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update the product.", ex);
            }

            if (affectedRows == 0)
            {
                throw new InvalidOperationException("The product was not found.");
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The product update did not affect exactly one record.");
            }

            return affectedRows;
        }

        /// <summary>
        /// Deletes a product by its identifier.
        /// </summary>
        /// <param name="id">The product identifier.</param>
        /// <returns>The number of rows affected.</returns>
        public int Delete(int id)
        {
            ValidateIdentifier(id, nameof(id));

            const string sql = @"DELETE FROM dbo.Product WHERE ProductId = @ProductId";
            var parameters = new[]
            {
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = id }
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to delete the product.", ex);
            }

            if (affectedRows == 0)
            {
                throw new InvalidOperationException("The product was not found.");
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The product delete did not affect exactly one record.");
            }

            return affectedRows;
        }

        private static void ValidateIdentifier(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "The identifier must be greater than zero.");
            }
        }

        private static string NormalizeRequiredString(string value, int maxLength, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A required product value must be provided.", parameterName);
            }

            var normalizedValue = value.Trim();

            if (normalizedValue.Length > maxLength)
            {
                throw new ArgumentException("A product value exceeds the allowed maximum length.", parameterName);
            }

            return normalizedValue;
        }

        private static string NormalizeOptionalString(string value, int maxLength, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalizedValue = value.Trim();

            if (normalizedValue.Length > maxLength)
            {
                throw new ArgumentException("A product value exceeds the allowed maximum length.", parameterName);
            }

            return normalizedValue;
        }

        private static SqlParameter CreatePriceParameter(decimal value)
        {
            return new SqlParameter("@Price", SqlDbType.Decimal)
            {
                Precision = 12,
                Scale = 2,
                Value = value
            };
        }

        private static Product MapProduct(SqlDataReader reader)
        {
            var productIdOrdinal = reader.GetOrdinal("ProductId");
            var categoryIdOrdinal = reader.GetOrdinal("CategoryId");
            var productNameOrdinal = reader.GetOrdinal("ProductName");
            var descriptionOrdinal = reader.GetOrdinal("Description");
            var priceOrdinal = reader.GetOrdinal("Price");
            var imageUrlOrdinal = reader.GetOrdinal("ImageUrl");
            var isActiveOrdinal = reader.GetOrdinal("IsActive");
            var createdAtOrdinal = reader.GetOrdinal("CreatedAt");

            return new Product
            {
                ProductId = reader.IsDBNull(productIdOrdinal) ? 0 : reader.GetInt32(productIdOrdinal),
                CategoryId = reader.IsDBNull(categoryIdOrdinal) ? 0 : reader.GetInt32(categoryIdOrdinal),
                ProductName = reader.IsDBNull(productNameOrdinal) ? null : reader.GetString(productNameOrdinal),
                Description = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal),
                Price = reader.IsDBNull(priceOrdinal) ? 0 : reader.GetDecimal(priceOrdinal),
                ImageUrl = reader.IsDBNull(imageUrlOrdinal) ? null : reader.GetString(imageUrlOrdinal),
                IsActive = reader.IsDBNull(isActiveOrdinal) ? false : reader.GetBoolean(isActiveOrdinal),
                CreatedAt = reader.IsDBNull(createdAtOrdinal) ? DateTime.MinValue : reader.GetDateTime(createdAtOrdinal)
            };
        }
    }
}
