using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BeverageWebsite.Models;

namespace BeverageWebsite.DAL
{
    /// <summary>
    /// Data access layer for address operations using ADO.NET and the shared DataProvider.
    /// </summary>
    public class AddressDAL
    {
        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressDAL"/> class.
        /// </summary>
        public AddressDAL()
        {
            _dataProvider = new DataProvider();
        }

        /// <summary>
        /// Retrieves all address records.
        /// </summary>
        /// <returns>A list of <see cref="Address"/> objects.</returns>
        public List<Address> GetAll()
        {
            var addresses = new List<Address>();
            const string sql = @"SELECT AddressId, UserId, RecipientName, Phone, Street, Ward, District, City, IsDefault
                                 FROM dbo.Address
                                 ORDER BY AddressId";

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text))
                {
                    while (reader.Read())
                    {
                        addresses.Add(MapAddress(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve addresses. Details: {ex.Message}", ex);
            }

            return addresses;
        }

        /// <summary>
        /// Retrieves all addresses belonging to a user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A list of the user's <see cref="Address"/> objects.</returns>
        public List<Address> GetByUserId(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User identifier must be greater than zero.");
            }

            var addresses = new List<Address>();
            const string sql = @"SELECT AddressId, UserId, RecipientName, Phone, Street, Ward, District, City, IsDefault
                                 FROM dbo.Address
                                 WHERE UserId = @UserId
                                 ORDER BY AddressId";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    while (reader.Read())
                    {
                        addresses.Add(MapAddress(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve addresses for user {userId}. Details: {ex.Message}", ex);
            }

            return addresses;
        }

        /// <summary>
        /// Retrieves an address by its identifier.
        /// </summary>
        /// <param name="addressId">The address identifier.</param>
        /// <returns>An <see cref="Address"/> object if found; otherwise, null.</returns>
        public Address GetById(int addressId)
        {
            if (addressId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(addressId), "Address identifier must be greater than zero.");
            }

            const string sql = @"SELECT AddressId, UserId, RecipientName, Phone, Street, Ward, District, City, IsDefault
                                 FROM dbo.Address
                                 WHERE AddressId = @AddressId";
            var parameters = new[]
            {
                new SqlParameter("@AddressId", SqlDbType.Int) { Value = addressId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapAddress(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve address by id {addressId}. Details: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Inserts a new address into the database.
        /// </summary>
        /// <param name="address">The address to insert.</param>
        /// <returns>The number of rows affected.</returns>
        public int Insert(Address address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (address.UserId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(address.UserId), "User identifier must be greater than zero.");
            }

            const string sql = @"INSERT INTO dbo.Address
                                     (UserId, RecipientName, Phone, Street, Ward, District, City, IsDefault)
                                 VALUES
                                     (@UserId, @RecipientName, @Phone, @Street, @Ward, @District, @City, @IsDefault)";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = address.UserId },
                new SqlParameter("@RecipientName", SqlDbType.NVarChar, 200) { Value = address.RecipientName ?? string.Empty },
                new SqlParameter("@Phone", SqlDbType.NVarChar, 20) { Value = address.Phone ?? string.Empty },
                new SqlParameter("@Street", SqlDbType.NVarChar, 255) { Value = address.Street ?? string.Empty },
                new SqlParameter("@Ward", SqlDbType.NVarChar, 100) { Value = (object)address.Ward ?? DBNull.Value },
                new SqlParameter("@District", SqlDbType.NVarChar, 100) { Value = (object)address.District ?? DBNull.Value },
                new SqlParameter("@City", SqlDbType.NVarChar, 100) { Value = address.City ?? string.Empty },
                new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = address.IsDefault }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to insert address. Details: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Updates an existing address in the database.
        /// </summary>
        /// <param name="address">The address to update.</param>
        /// <returns>The number of rows affected.</returns>
        public int Update(Address address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (address.AddressId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(address.AddressId), "Address identifier must be greater than zero.");
            }

            if (address.UserId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(address.UserId), "User identifier must be greater than zero.");
            }

            const string sql = @"UPDATE dbo.Address
                                 SET RecipientName = @RecipientName,
                                     Phone = @Phone,
                                     Street = @Street,
                                     Ward = @Ward,
                                     District = @District,
                                     City = @City,
                                     IsDefault = @IsDefault
                                 WHERE AddressId = @AddressId
                                   AND UserId = @UserId";
            var parameters = new[]
            {
                new SqlParameter("@AddressId", SqlDbType.Int) { Value = address.AddressId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = address.UserId },
                new SqlParameter("@RecipientName", SqlDbType.NVarChar, 200) { Value = address.RecipientName ?? string.Empty },
                new SqlParameter("@Phone", SqlDbType.NVarChar, 20) { Value = address.Phone ?? string.Empty },
                new SqlParameter("@Street", SqlDbType.NVarChar, 255) { Value = address.Street ?? string.Empty },
                new SqlParameter("@Ward", SqlDbType.NVarChar, 100) { Value = (object)address.Ward ?? DBNull.Value },
                new SqlParameter("@District", SqlDbType.NVarChar, 100) { Value = (object)address.District ?? DBNull.Value },
                new SqlParameter("@City", SqlDbType.NVarChar, 100) { Value = address.City ?? string.Empty },
                new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = address.IsDefault }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update address. Details: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Deletes an address by its identifier.
        /// </summary>
        /// <param name="addressId">The address identifier.</param>
        /// <returns>The number of rows affected.</returns>
        public int Delete(int addressId)
        {
            if (addressId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(addressId), "Address identifier must be greater than zero.");
            }

            const string sql = @"DELETE FROM dbo.Address WHERE AddressId = @AddressId";
            var parameters = new[]
            {
                new SqlParameter("@AddressId", SqlDbType.Int) { Value = addressId }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete address {addressId}. Details: {ex.Message}", ex);
            }
        }

        private static Address MapAddress(SqlDataReader reader)
        {
            return new Address
            {
                AddressId = reader.IsDBNull(reader.GetOrdinal("AddressId")) ? 0 : reader.GetInt32(reader.GetOrdinal("AddressId")),
                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserId")),
                RecipientName = reader.IsDBNull(reader.GetOrdinal("RecipientName")) ? null : reader.GetString(reader.GetOrdinal("RecipientName")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                Street = reader.IsDBNull(reader.GetOrdinal("Street")) ? null : reader.GetString(reader.GetOrdinal("Street")),
                Ward = reader.IsDBNull(reader.GetOrdinal("Ward")) ? null : reader.GetString(reader.GetOrdinal("Ward")),
                District = reader.IsDBNull(reader.GetOrdinal("District")) ? null : reader.GetString(reader.GetOrdinal("District")),
                City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City")),
                IsDefault = reader.IsDBNull(reader.GetOrdinal("IsDefault")) ? false : reader.GetBoolean(reader.GetOrdinal("IsDefault"))
            };
        }
    }
}
