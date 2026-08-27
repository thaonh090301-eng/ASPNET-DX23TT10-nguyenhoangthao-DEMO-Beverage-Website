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
        private const int RecipientNameMaxLength = 200;
        private const int PhoneMaxLength = 20;
        private const int StreetMaxLength = 255;
        private const int WardMaxLength = 100;
        private const int DistrictMaxLength = 100;
        private const int CityMaxLength = 100;

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
                throw new InvalidOperationException("Failed to retrieve addresses.", ex);
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
            ValidateIdentifier(userId, nameof(userId));

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
                throw new InvalidOperationException("Failed to retrieve addresses for the user.", ex);
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
            ValidateIdentifier(addressId, nameof(addressId));

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
                throw new InvalidOperationException("Failed to retrieve the address.", ex);
            }

            return null;
        }

        /// <summary>
        /// Retrieves an address only when it belongs to the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the address owner.</param>
        /// <param name="addressId">The identifier of the address.</param>
        /// <returns>The matching owned address when found; otherwise, null.</returns>
        public Address GetByUserIdAndAddressId(int userId, int addressId)
        {
            ValidateIdentifier(userId, nameof(userId));
            ValidateIdentifier(addressId, nameof(addressId));

            const string sql = @"SELECT AddressId, UserId, RecipientName, Phone, Street, Ward, District, City, IsDefault
                                 FROM dbo.Address
                                 WHERE AddressId = @AddressId
                                   AND UserId = @UserId";
            var parameters = new[]
            {
                new SqlParameter("@AddressId", SqlDbType.Int) { Value = addressId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
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
                throw new InvalidOperationException("Failed to retrieve the address for the user.", ex);
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

            ValidateIdentifier(address.UserId, nameof(address.UserId));

            var recipientName = NormalizeRequiredString(
                address.RecipientName,
                RecipientNameMaxLength,
                nameof(address.RecipientName));
            var phone = NormalizeRequiredString(
                address.Phone,
                PhoneMaxLength,
                nameof(address.Phone));
            var street = NormalizeRequiredString(
                address.Street,
                StreetMaxLength,
                nameof(address.Street));
            var ward = NormalizeOptionalString(
                address.Ward,
                WardMaxLength,
                nameof(address.Ward));
            var district = NormalizeOptionalString(
                address.District,
                DistrictMaxLength,
                nameof(address.District));
            var city = NormalizeRequiredString(
                address.City,
                CityMaxLength,
                nameof(address.City));

            const string sql = @"INSERT INTO dbo.Address
                                     (UserId, RecipientName, Phone, Street, Ward, District, City, IsDefault)
                                 VALUES
                                     (@UserId, @RecipientName, @Phone, @Street, @Ward, @District, @City, @IsDefault)";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = address.UserId },
                new SqlParameter("@RecipientName", SqlDbType.NVarChar, RecipientNameMaxLength) { Value = recipientName },
                new SqlParameter("@Phone", SqlDbType.NVarChar, PhoneMaxLength) { Value = phone },
                new SqlParameter("@Street", SqlDbType.NVarChar, StreetMaxLength) { Value = street },
                new SqlParameter("@Ward", SqlDbType.NVarChar, WardMaxLength) { Value = (object)ward ?? DBNull.Value },
                new SqlParameter("@District", SqlDbType.NVarChar, DistrictMaxLength) { Value = (object)district ?? DBNull.Value },
                new SqlParameter("@City", SqlDbType.NVarChar, CityMaxLength) { Value = city },
                new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = address.IsDefault }
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to insert the address.", ex);
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The address insert did not affect exactly one record.");
            }

            return affectedRows;
        }

        /// <summary>
        /// Updates an existing address while preserving its user ownership.
        /// </summary>
        /// <param name="address">The address data, including its identifier and owning user identifier.</param>
        /// <returns>The number of rows affected.</returns>
        public int Update(Address address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            ValidateIdentifier(address.AddressId, nameof(address.AddressId));
            ValidateIdentifier(address.UserId, nameof(address.UserId));

            var recipientName = NormalizeRequiredString(
                address.RecipientName,
                RecipientNameMaxLength,
                nameof(address.RecipientName));
            var phone = NormalizeRequiredString(
                address.Phone,
                PhoneMaxLength,
                nameof(address.Phone));
            var street = NormalizeRequiredString(
                address.Street,
                StreetMaxLength,
                nameof(address.Street));
            var ward = NormalizeOptionalString(
                address.Ward,
                WardMaxLength,
                nameof(address.Ward));
            var district = NormalizeOptionalString(
                address.District,
                DistrictMaxLength,
                nameof(address.District));
            var city = NormalizeRequiredString(
                address.City,
                CityMaxLength,
                nameof(address.City));

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
                new SqlParameter("@RecipientName", SqlDbType.NVarChar, RecipientNameMaxLength) { Value = recipientName },
                new SqlParameter("@Phone", SqlDbType.NVarChar, PhoneMaxLength) { Value = phone },
                new SqlParameter("@Street", SqlDbType.NVarChar, StreetMaxLength) { Value = street },
                new SqlParameter("@Ward", SqlDbType.NVarChar, WardMaxLength) { Value = (object)ward ?? DBNull.Value },
                new SqlParameter("@District", SqlDbType.NVarChar, DistrictMaxLength) { Value = (object)district ?? DBNull.Value },
                new SqlParameter("@City", SqlDbType.NVarChar, CityMaxLength) { Value = city },
                new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = address.IsDefault }
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update the address.", ex);
            }

            if (affectedRows == 0)
            {
                throw new InvalidOperationException("The address was not found or is not owned by the user.");
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The address update did not affect exactly one record.");
            }

            return affectedRows;
        }

        /// <summary>
        /// Deletes an address owned by the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the address.</param>
        /// <param name="addressId">The address identifier.</param>
        /// <returns>The number of rows affected.</returns>
        public int Delete(int userId, int addressId)
        {
            ValidateIdentifier(userId, nameof(userId));
            ValidateIdentifier(addressId, nameof(addressId));

            const string sql = @"DELETE FROM dbo.Address
                                 WHERE AddressId = @AddressId
                                   AND UserId = @UserId";
            var parameters = new[]
            {
                new SqlParameter("@AddressId", SqlDbType.Int) { Value = addressId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to delete the address.", ex);
            }

            if (affectedRows == 0)
            {
                throw new InvalidOperationException("The address was not found or is not owned by the user.");
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The address delete did not affect exactly one record.");
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
                throw new ArgumentException("A required address value must be provided.", parameterName);
            }

            var normalizedValue = value.Trim();

            if (normalizedValue.Length > maxLength)
            {
                throw new ArgumentException("An address value exceeds the allowed maximum length.", parameterName);
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
                throw new ArgumentException("An address value exceeds the allowed maximum length.", parameterName);
            }

            return normalizedValue;
        }

        private static Address MapAddress(SqlDataReader reader)
        {
            var addressIdOrdinal = reader.GetOrdinal("AddressId");
            var userIdOrdinal = reader.GetOrdinal("UserId");
            var recipientNameOrdinal = reader.GetOrdinal("RecipientName");
            var phoneOrdinal = reader.GetOrdinal("Phone");
            var streetOrdinal = reader.GetOrdinal("Street");
            var wardOrdinal = reader.GetOrdinal("Ward");
            var districtOrdinal = reader.GetOrdinal("District");
            var cityOrdinal = reader.GetOrdinal("City");
            var isDefaultOrdinal = reader.GetOrdinal("IsDefault");

            return new Address
            {
                AddressId = reader.IsDBNull(addressIdOrdinal) ? 0 : reader.GetInt32(addressIdOrdinal),
                UserId = reader.IsDBNull(userIdOrdinal) ? 0 : reader.GetInt32(userIdOrdinal),
                RecipientName = reader.IsDBNull(recipientNameOrdinal) ? null : reader.GetString(recipientNameOrdinal),
                Phone = reader.IsDBNull(phoneOrdinal) ? null : reader.GetString(phoneOrdinal),
                Street = reader.IsDBNull(streetOrdinal) ? null : reader.GetString(streetOrdinal),
                Ward = reader.IsDBNull(wardOrdinal) ? null : reader.GetString(wardOrdinal),
                District = reader.IsDBNull(districtOrdinal) ? null : reader.GetString(districtOrdinal),
                City = reader.IsDBNull(cityOrdinal) ? null : reader.GetString(cityOrdinal),
                IsDefault = reader.IsDBNull(isDefaultOrdinal) ? false : reader.GetBoolean(isDefaultOrdinal)
            };
        }
    }
}
