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
            const string sql = @"SELECT UserId, UserName, Email, FullName, Phone, Role, IsActive, CreatedAt FROM dbo.[User] ORDER BY UserName";

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
                throw new InvalidOperationException("Failed to retrieve users.", ex);
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
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User identifier must be greater than zero.");
            }

            const string sql = @"SELECT UserId, UserName, Email, FullName, Phone, Role, IsActive, CreatedAt FROM dbo.[User] WHERE UserId = @UserId";
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
                throw new InvalidOperationException("Failed to retrieve user.", ex);
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
            var normalizedEmail = ValidateAndNormalizeEmail(email);
            const string sql = @"SELECT UserId, UserName, Email, FullName, Phone, Role, IsActive, CreatedAt FROM dbo.[User] WHERE Email = @Email";
            var parameters = new[]
            {
                new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = normalizedEmail }
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
                throw new InvalidOperationException("Failed to retrieve user by email.", ex);
            }

            return null;
        }

        /// <summary>
        /// Retrieves a user by exact email address together with credential data required for authentication.
        /// </summary>
        /// <param name="email">The email address used for the authentication lookup.</param>
        /// <returns>
        /// A <see cref="User"/> object including its password hash when found; otherwise, null.
        /// The returned credential data is intended only for authentication verification.
        /// </returns>
        public User GetByEmailForAuthentication(string email)
        {
            var normalizedEmail = ValidateAndNormalizeEmail(email);
            const string sql = @"SELECT UserId, UserName, Email, PasswordHash, FullName, Phone, Role, IsActive, CreatedAt FROM dbo.[User] WHERE Email = @Email";
            var parameters = new[]
            {
                new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = normalizedEmail }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapUserForAuthentication(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve authentication credentials.", ex);
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

            const string sql = @"UPDATE dbo.[User] SET UserName = @UserName, Email = @Email, FullName = @FullName, Phone = @Phone, Role = @Role, IsActive = @IsActive WHERE UserId = @UserId";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = user.UserId },
                new SqlParameter("@UserName", SqlDbType.NVarChar, 100) { Value = user.UserName ?? string.Empty },
                new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = user.Email ?? string.Empty },
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
        /// Updates the stored password hash for an existing user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="passwordHash">The already-created password hash to store.</param>
        /// <returns>The number of rows affected.</returns>
        public int UpdatePasswordHash(int userId, string passwordHash)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User identifier must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Password hash is required.", nameof(passwordHash));
            }

            if (passwordHash.Length > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(passwordHash), "Password hash must not exceed 255 characters.");
            }

            const string sql = @"UPDATE dbo.[User]
                                 SET PasswordHash = @PasswordHash
                                 WHERE UserId = @UserId";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 255) { Value = passwordHash }
            };

            try
            {
                var affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);

                if (affectedRows == 0)
                {
                    throw new InvalidOperationException("The user does not exist.");
                }

                if (affectedRows != 1)
                {
                    throw new InvalidOperationException("The password hash could not be updated.");
                }

                return affectedRows;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update the password hash.", ex);
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

        private static string ValidateAndNormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is required.", nameof(email));
            }

            var normalizedEmail = email.Trim();

            if (normalizedEmail.Length > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(email), "Email must not exceed 255 characters.");
            }

            return normalizedEmail;
        }

        private static User MapUser(SqlDataReader reader)
        {
            var userIdOrdinal = reader.GetOrdinal("UserId");
            var userNameOrdinal = reader.GetOrdinal("UserName");
            var emailOrdinal = reader.GetOrdinal("Email");
            var fullNameOrdinal = reader.GetOrdinal("FullName");
            var phoneOrdinal = reader.GetOrdinal("Phone");
            var roleOrdinal = reader.GetOrdinal("Role");
            var isActiveOrdinal = reader.GetOrdinal("IsActive");
            var createdAtOrdinal = reader.GetOrdinal("CreatedAt");

            return new User
            {
                UserId = reader.IsDBNull(userIdOrdinal) ? 0 : reader.GetInt32(userIdOrdinal),
                UserName = reader.IsDBNull(userNameOrdinal) ? null : reader.GetString(userNameOrdinal),
                Email = reader.IsDBNull(emailOrdinal) ? null : reader.GetString(emailOrdinal),
                FullName = reader.IsDBNull(fullNameOrdinal) ? null : reader.GetString(fullNameOrdinal),
                Phone = reader.IsDBNull(phoneOrdinal) ? null : reader.GetString(phoneOrdinal),
                Role = reader.IsDBNull(roleOrdinal) ? null : reader.GetString(roleOrdinal),
                IsActive = reader.IsDBNull(isActiveOrdinal) ? false : reader.GetBoolean(isActiveOrdinal),
                CreatedAt = reader.IsDBNull(createdAtOrdinal) ? DateTime.MinValue : reader.GetDateTime(createdAtOrdinal)
            };
        }

        private static User MapUserForAuthentication(SqlDataReader reader)
        {
            var passwordHashOrdinal = reader.GetOrdinal("PasswordHash");
            var user = MapUser(reader);
            user.PasswordHash = reader.IsDBNull(passwordHashOrdinal) ? null : reader.GetString(passwordHashOrdinal);
            return user;
        }
    }
}
