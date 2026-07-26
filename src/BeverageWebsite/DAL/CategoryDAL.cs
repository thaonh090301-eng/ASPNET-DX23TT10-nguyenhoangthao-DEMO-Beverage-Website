using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BeverageWebsite.Models;

namespace BeverageWebsite.DAL
{
    /// <summary>
    /// Data access layer for Category operations using ADO.NET and the shared DataProvider.
    /// </summary>
    public class CategoryDAL
    {
        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDAL"/> class.
        /// </summary>
        public CategoryDAL()
        {
            _dataProvider = new DataProvider();
        }

        /// <summary>
        /// Retrieves all active and inactive categories.
        /// </summary>
        /// <returns>A list of <see cref="Category"/> objects.</returns>
        public List<Category> GetAll()
        {
            var categories = new List<Category>();
            const string sql = @"SELECT CategoryId, CategoryName, Description, IsActive FROM dbo.Category ORDER BY CategoryName";

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql))
                {
                    while (reader.Read())
                    {
                        categories.Add(MapCategory(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve categories. Details: {ex.Message}", ex);
            }

            return categories;
        }

        /// <summary>
        /// Retrieves a category by its identifier.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <returns>A <see cref="Category"/> object if found; otherwise, null.</returns>
        public Category GetById(int id)
        {
            const string sql = @"SELECT CategoryId, CategoryName, Description, IsActive FROM dbo.Category WHERE CategoryId = @CategoryId";
            var parameters = new[]
            {
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = id }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapCategory(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve category by id {id}. Details: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Inserts a new category into the database.
        /// </summary>
        /// <param name="category">The category to insert.</param>
        /// <returns>The number of rows affected.</returns>
        public int Insert(Category category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            const string sql = @"INSERT INTO dbo.Category (CategoryName, Description, IsActive) VALUES (@CategoryName, @Description, @IsActive)";
            var parameters = new[]
            {
                new SqlParameter("@CategoryName", SqlDbType.NVarChar, 100) { Value = category.CategoryName ?? string.Empty },
                new SqlParameter("@Description", SqlDbType.NVarChar, 500) { Value = (object)category.Description ?? DBNull.Value },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = category.IsActive }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to insert category. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing category in the database.
        /// </summary>
        /// <param name="category">The category to update.</param>
        /// <returns>The number of rows affected.</returns>
        public int Update(Category category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            const string sql = @"UPDATE dbo.Category SET CategoryName = @CategoryName, Description = @Description, IsActive = @IsActive WHERE CategoryId = @CategoryId";
            var parameters = new[]
            {
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = category.CategoryId },
                new SqlParameter("@CategoryName", SqlDbType.NVarChar, 100) { Value = category.CategoryName ?? string.Empty },
                new SqlParameter("@Description", SqlDbType.NVarChar, 500) { Value = (object)category.Description ?? DBNull.Value },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = category.IsActive }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update category. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes a category by its identifier.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <returns>The number of rows affected.</returns>
        public int Delete(int id)
        {
            const string sql = @"DELETE FROM dbo.Category WHERE CategoryId = @CategoryId";
            var parameters = new[]
            {
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = id }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete category {id}. Details: {ex.Message}", ex);
            }
        }

        private static Category MapCategory(SqlDataReader reader)
        {
            return new Category
            {
                CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? 0 : reader.GetInt32(reader.GetOrdinal("CategoryId")),
                CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? null : reader.GetString(reader.GetOrdinal("CategoryName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                IsActive = reader.IsDBNull(reader.GetOrdinal("IsActive")) ? false : reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }
    }
}
