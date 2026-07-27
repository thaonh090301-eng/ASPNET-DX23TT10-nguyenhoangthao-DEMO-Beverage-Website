using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BeverageWebsite.Models;

namespace BeverageWebsite.DAL
{
    /// <summary>
    /// Data access layer for User operations using ADO.NET and the shared DataProvider.
    /// </summary>
    public class UserDAL
    {
        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDAL"/> class.
        /// </summary>
        public UserDAL()
        {
            _dataProvider = new DataProvider();
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>A list of <see cref="User"/> objects.</returns>
        public List<User> GetAll()
        {
            var users = new List<User>();
            const string sql = @"SELECT UserId, UserName, Email, PasswordHash, FullName, Phone, Role, IsActive, CreatedAt FROM dbo.[User] ORDER BY UserName";

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql))
                {
                    while (reader.Read())
                    {
                        users.Add(MapUser(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve users. Details: {ex.Message}", ex);
            }

            return users;
        }

        /// <summary>
        /// Retrieves a user by its identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A <see cref="User"/> object if found; otherwise, null.</returns>
        public User GetById(int userId)
        {
            const string sql = @"SELECT UserId, UserName, Email, PasswordHash, FullName, Phone, Role, IsActive, CreatedAt FROM dbo.[User] WHERE UserId = @UserId";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapUser(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve user by id {userId}. Details: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Retrieves a user by email address.
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <returns>A <see cref="User"/> object if found; otherwise, null.</returns>
        public User GetByEmail(string email)
        {
            const string sql = @"SELECT UserId, UserName, Email, PasswordHash, FullName, Phone, Role, IsActive, CreatedAt FROM dbo.[User] WHERE Email = @Email";
            var parameters = new[]
            {
                new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = email ?? string.Empty }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapUser(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve user by email '{email}'. Details: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Checks whether a user with the specified email already exists.
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <returns>True if the user exists; otherwise, false.</returns>
        public bool ExistsByEmail(string email)
        {
            const string sql = @"SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.[User] WHERE Email = @Email) THEN 1 ELSE 0 END AS BIT)";
            var parameters = new[]
            {
                new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = email ?? string.Empty }
            };

            try
            {
                var result = _dataProvider.ExecuteScalar(sql, CommandType.Text, parameters);
                return result != null && Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to check user email existence '{email}'. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserts a new user into the database.
        /// </summary>
        /// <param name="user">The user to insert.</param>
        /// <returns>The number of rows affected.</returns>
        public int Insert(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            const string sql = @"INSERT INTO dbo.[User] (UserName, Email, PasswordHash, FullName, Phone, Role, IsActive) VALUES (@UserName, @Email, @PasswordHash, @FullName, @Phone, @Role, @IsActive)";
            var parameters = new[]
            {
                new SqlParameter("@UserName", SqlDbType.NVarChar, 100) { Value = user.UserName ?? string.Empty },
                new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = user.Email ?? string.Empty },
                new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 255) { Value = user.PasswordHash ?? string.Empty },
                new SqlParameter("@FullName", SqlDbType.NVarChar, 200) { Value = (object)user.FullName ?? DBNull.Value },
                new SqlParameter("@Phone", SqlDbType.NVarChar, 20) { Value = (object)user.Phone ?? DBNull.Value },
                new SqlParameter("@Role", SqlDbType.NVarChar, 20) { Value = user.Role ?? string.Empty },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = user.IsActive }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to insert user. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing user in the database.
        /// </summary>
        /// <param name="user">The user to update.</param>
        /// <returns>The number of rows affected.</returns>
        public int Update(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            const string sql = @"UPDATE dbo.[User] SET UserName = @UserName, Email = @Email, PasswordHash = @PasswordHash, FullName = @FullName, Phone = @Phone, Role = @Role, IsActive = @IsActive WHERE UserId = @UserId";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = user.UserId },
                new SqlParameter("@UserName", SqlDbType.NVarChar, 100) { Value = user.UserName ?? string.Empty },
                new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = user.Email ?? string.Empty },
                new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 255) { Value = user.PasswordHash ?? string.Empty },
                new SqlParameter("@FullName", SqlDbType.NVarChar, 200) { Value = (object)user.FullName ?? DBNull.Value },
                new SqlParameter("@Phone", SqlDbType.NVarChar, 20) { Value = (object)user.Phone ?? DBNull.Value },
                new SqlParameter("@Role", SqlDbType.NVarChar, 20) { Value = user.Role ?? string.Empty },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = user.IsActive }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update user. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes a user by its identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The number of rows affected.</returns>
        public int Delete(int userId)
        {
            const string sql = @"DELETE FROM dbo.[User] WHERE UserId = @UserId";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete user {userId}. Details: {ex.Message}", ex);
            }
        }

        private static User MapUser(SqlDataReader reader)
        {
            return new User
            {
                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserId")),
                UserName = reader.IsDBNull(reader.GetOrdinal("UserName")) ? null : reader.GetString(reader.GetOrdinal("UserName")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.IsDBNull(reader.GetOrdinal("PasswordHash")) ? null : reader.GetString(reader.GetOrdinal("PasswordHash")),
                FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? null : reader.GetString(reader.GetOrdinal("FullName")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                Role = reader.IsDBNull(reader.GetOrdinal("Role")) ? null : reader.GetString(reader.GetOrdinal("Role")),
                IsActive = reader.IsDBNull(reader.GetOrdinal("IsActive")) ? false : reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}
