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
        private const int CategoryNameMaxLength = 100;
        private const int DescriptionMaxLength = 500;

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
                throw new InvalidOperationException("Failed to retrieve categories.", ex);
            }

            return categories;
        }

        /// <summary>
        /// Retrieves all active categories for the public catalog.
        /// </summary>
        /// <returns>A list of active <see cref="Category"/> objects.</returns>
        public List<Category> GetActive()
        {
            var categories = new List<Category>();
            const string sql = @"SELECT CategoryId, CategoryName, Description, IsActive FROM dbo.Category WHERE IsActive = 1 ORDER BY CategoryName";

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
                throw new InvalidOperationException("Failed to retrieve active categories.", ex);
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
            ValidateCategoryId(id, nameof(id));

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
                throw new InvalidOperationException("Failed to retrieve the category.", ex);
            }

            return null;
        }

        /// <summary>
        /// Retrieves an active category by its identifier for the public catalog.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <returns>An active <see cref="Category"/> object if found; otherwise, null.</returns>
        public Category GetActiveById(int id)
        {
            ValidateCategoryId(id, nameof(id));

            const string sql = @"SELECT CategoryId, CategoryName, Description, IsActive FROM dbo.Category WHERE CategoryId = @CategoryId AND IsActive = 1";
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
                throw new InvalidOperationException("Failed to retrieve the active category.", ex);
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

            var categoryName = NormalizeRequiredString(
                category.CategoryName,
                CategoryNameMaxLength,
                nameof(category.CategoryName));
            var description = NormalizeOptionalString(
                category.Description,
                DescriptionMaxLength,
                nameof(category.Description));

            const string sql = @"INSERT INTO dbo.Category (CategoryName, Description, IsActive) VALUES (@CategoryName, @Description, @IsActive)";
            var parameters = new[]
            {
                new SqlParameter("@CategoryName", SqlDbType.NVarChar, CategoryNameMaxLength) { Value = categoryName },
                new SqlParameter("@Description", SqlDbType.NVarChar, DescriptionMaxLength) { Value = (object)description ?? DBNull.Value },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = category.IsActive }
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to insert the category.", ex);
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The category insert did not affect exactly one record.");
            }

            return affectedRows;
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

            ValidateCategoryId(category.CategoryId, nameof(category.CategoryId));

            var categoryName = NormalizeRequiredString(
                category.CategoryName,
                CategoryNameMaxLength,
                nameof(category.CategoryName));
            var description = NormalizeOptionalString(
                category.Description,
                DescriptionMaxLength,
                nameof(category.Description));

            const string sql = @"UPDATE dbo.Category SET CategoryName = @CategoryName, Description = @Description, IsActive = @IsActive WHERE CategoryId = @CategoryId";
            var parameters = new[]
            {
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = category.CategoryId },
                new SqlParameter("@CategoryName", SqlDbType.NVarChar, CategoryNameMaxLength) { Value = categoryName },
                new SqlParameter("@Description", SqlDbType.NVarChar, DescriptionMaxLength) { Value = (object)description ?? DBNull.Value },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = category.IsActive }
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update the category.", ex);
            }

            if (affectedRows == 0)
            {
                throw new InvalidOperationException("The category was not found.");
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The category update did not affect exactly one record.");
            }

            return affectedRows;
        }

        /// <summary>
        /// Deletes a category by its identifier.
        /// </summary>
        /// <param name="id">The category identifier.</param>
        /// <returns>The number of rows affected.</returns>
        public int Delete(int id)
        {
            ValidateCategoryId(id, nameof(id));

            const string sql = @"DELETE FROM dbo.Category WHERE CategoryId = @CategoryId";
            var parameters = new[]
            {
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = id }
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to delete the category.", ex);
            }

            if (affectedRows == 0)
            {
                throw new InvalidOperationException("The category was not found.");
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The category delete did not affect exactly one record.");
            }

            return affectedRows;
        }

        /// <summary>
        /// Deletes a category only when no active or inactive product belongs to it.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <returns><c>true</c> when the category was deleted; otherwise, <c>false</c>.</returns>
        public bool DeleteIfEmpty(int categoryId)
        {
            ValidateCategoryId(categoryId, nameof(categoryId));

            return _dataProvider.ExecuteInTransaction(
                (connection, transaction) =>
                {
                    const string categoryExistsSql = @"
SELECT CASE WHEN EXISTS
(
    SELECT 1
    FROM dbo.Category WITH (UPDLOCK, HOLDLOCK)
    WHERE CategoryId = @CategoryId
)
THEN 1 ELSE 0 END;";

                    using (var existsCommand = new SqlCommand(
                        categoryExistsSql,
                        connection,
                        transaction))
                    {
                        existsCommand.Parameters.Add(
                            new SqlParameter("@CategoryId", SqlDbType.Int)
                            {
                                Value = categoryId
                            });

                        if (Convert.ToInt32(existsCommand.ExecuteScalar()) != 1)
                        {
                            return false;
                        }
                    }

                    const string productExistsSql = @"
SELECT CASE WHEN EXISTS
(
    SELECT 1
    FROM dbo.Product WITH (UPDLOCK, HOLDLOCK)
    WHERE CategoryId = @CategoryId
)
THEN 1 ELSE 0 END;";

                    using (var productCommand = new SqlCommand(
                        productExistsSql,
                        connection,
                        transaction))
                    {
                        productCommand.Parameters.Add(
                            new SqlParameter("@CategoryId", SqlDbType.Int)
                            {
                                Value = categoryId
                            });

                        if (Convert.ToInt32(productCommand.ExecuteScalar()) == 1)
                        {
                            return false;
                        }
                    }

                    const string deleteSql = @"
DELETE FROM dbo.Category
WHERE CategoryId = @CategoryId;";

                    using (var deleteCommand = new SqlCommand(
                        deleteSql,
                        connection,
                        transaction))
                    {
                        deleteCommand.Parameters.Add(
                            new SqlParameter("@CategoryId", SqlDbType.Int)
                            {
                                Value = categoryId
                            });

                        if (deleteCommand.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException(
                                "The category delete did not affect exactly one record.");
                        }
                    }

                    return true;
                },
                IsolationLevel.Serializable);
        }

        private static Category MapCategory(SqlDataReader reader)
        {
            var categoryIdOrdinal = reader.GetOrdinal("CategoryId");
            var categoryNameOrdinal = reader.GetOrdinal("CategoryName");
            var descriptionOrdinal = reader.GetOrdinal("Description");
            var isActiveOrdinal = reader.GetOrdinal("IsActive");

            return new Category
            {
                CategoryId = reader.IsDBNull(categoryIdOrdinal) ? 0 : reader.GetInt32(categoryIdOrdinal),
                CategoryName = reader.IsDBNull(categoryNameOrdinal) ? null : reader.GetString(categoryNameOrdinal),
                Description = reader.IsDBNull(descriptionOrdinal) ? null : reader.GetString(descriptionOrdinal),
                IsActive = reader.IsDBNull(isActiveOrdinal) ? false : reader.GetBoolean(isActiveOrdinal)
            };
        }

        private static void ValidateCategoryId(int categoryId, string parameterName)
        {
            if (categoryId <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "The category identifier must be greater than zero.");
            }
        }

        private static string NormalizeRequiredString(string value, int maxLength, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A required category value must be provided.", parameterName);
            }

            var normalizedValue = value.Trim();

            if (normalizedValue.Length > maxLength)
            {
                throw new ArgumentException("A category value exceeds the allowed maximum length.", parameterName);
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
                throw new ArgumentException("A category value exceeds the allowed maximum length.", parameterName);
            }

            return normalizedValue;
        }
    }
}
