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
                throw new InvalidOperationException($"Failed to retrieve products. Details: {ex.Message}", ex);
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
                throw new InvalidOperationException($"Failed to retrieve product by id {id}. Details: {ex.Message}", ex);
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
                throw new InvalidOperationException($"Failed to retrieve products by category {categoryId}. Details: {ex.Message}", ex);
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
            var products = new List<Product>();
            const string sql = @"SELECT ProductId, CategoryId, ProductName, Description, Price, ImageUrl, IsActive, CreatedAt FROM dbo.Product WHERE ProductName LIKE @Keyword OR Description LIKE @Keyword ORDER BY ProductName";
            var parameters = new[]
            {
                new SqlParameter("@Keyword", SqlDbType.NVarChar, 200) { Value = $"%{keyword ?? string.Empty}%" }
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
                throw new InvalidOperationException($"Failed to search products with keyword '{keyword}'. Details: {ex.Message}", ex);
            }

            return products;
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

            const string sql = @"INSERT INTO dbo.Product (CategoryId, ProductName, Description, Price, ImageUrl, IsActive, CreatedAt) VALUES (@CategoryId, @ProductName, @Description, @Price, @ImageUrl, @IsActive, @CreatedAt)";
            var parameters = new[]
            {
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = product.CategoryId },
                new SqlParameter("@ProductName", SqlDbType.NVarChar, 200) { Value = product.ProductName ?? string.Empty },
                new SqlParameter("@Description", SqlDbType.NVarChar, 1000) { Value = (object)product.Description ?? DBNull.Value },
                new SqlParameter("@Price", SqlDbType.Decimal) { Value = product.Price },
                new SqlParameter("@ImageUrl", SqlDbType.NVarChar, 500) { Value = (object)product.ImageUrl ?? DBNull.Value },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = product.IsActive },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = product.CreatedAt }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to insert product. Details: {ex.Message}", ex);
            }
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

            const string sql = @"UPDATE dbo.Product SET CategoryId = @CategoryId, ProductName = @ProductName, Description = @Description, Price = @Price, ImageUrl = @ImageUrl, IsActive = @IsActive, CreatedAt = @CreatedAt WHERE ProductId = @ProductId";
            var parameters = new[]
            {
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = product.ProductId },
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = product.CategoryId },
                new SqlParameter("@ProductName", SqlDbType.NVarChar, 200) { Value = product.ProductName ?? string.Empty },
                new SqlParameter("@Description", SqlDbType.NVarChar, 1000) { Value = (object)product.Description ?? DBNull.Value },
                new SqlParameter("@Price", SqlDbType.Decimal) { Value = product.Price },
                new SqlParameter("@ImageUrl", SqlDbType.NVarChar, 500) { Value = (object)product.ImageUrl ?? DBNull.Value },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = product.IsActive },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = product.CreatedAt }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update product. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes a product by its identifier.
        /// </summary>
        /// <param name="id">The product identifier.</param>
        /// <returns>The number of rows affected.</returns>
        public int Delete(int id)
        {
            const string sql = @"DELETE FROM dbo.Product WHERE ProductId = @ProductId";
            var parameters = new[]
            {
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = id }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete product {id}. Details: {ex.Message}", ex);
            }
        }

        private static Product MapProduct(SqlDataReader reader)
        {
            return new Product
            {
                ProductId = reader.IsDBNull(reader.GetOrdinal("ProductId")) ? 0 : reader.GetInt32(reader.GetOrdinal("ProductId")),
                CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? 0 : reader.GetInt32(reader.GetOrdinal("CategoryId")),
                ProductName = reader.IsDBNull(reader.GetOrdinal("ProductName")) ? null : reader.GetString(reader.GetOrdinal("ProductName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Price")),
                ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? null : reader.GetString(reader.GetOrdinal("ImageUrl")),
                IsActive = reader.IsDBNull(reader.GetOrdinal("IsActive")) ? false : reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}
