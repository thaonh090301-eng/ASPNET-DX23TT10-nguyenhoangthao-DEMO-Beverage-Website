using System;
using System.Collections.Generic;
using BeverageWebsite.DAL;
using BeverageWebsite.Models;

namespace BeverageWebsite.BLL
{
    /// <summary>
    /// Provides business operations for managing user addresses.
    /// </summary>
    public class AddressBLL
    {
        private const int RecipientNameMaxLength = 200;
        private const int PhoneMaxLength = 20;
        private const int StreetMaxLength = 255;
        private const int WardMaxLength = 100;
        private const int DistrictMaxLength = 100;
        private const int CityMaxLength = 100;

        private readonly AddressDAL _addressDal;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressBLL"/> class.
        /// </summary>
        public AddressBLL()
        {
            _addressDal = new AddressDAL();
        }

        /// <summary>
        /// Retrieves all address records.
        /// </summary>
        /// <returns>All addresses returned by the data access layer.</returns>
        public List<Address> GetAll()
        {
            return _addressDal.GetAll();
        }

        /// <summary>
        /// Retrieves all addresses owned by a user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user's addresses returned by the data access layer.</returns>
        public List<Address> GetByUserId(int userId)
        {
            ValidateIdentifier(userId, nameof(userId));
            return _addressDal.GetByUserId(userId);
        }

        /// <summary>
        /// Retrieves an address by its identifier.
        /// </summary>
        /// <param name="addressId">The address identifier.</param>
        /// <returns>The matching address when found; otherwise, null.</returns>
        public Address GetById(int addressId)
        {
            ValidateIdentifier(addressId, nameof(addressId));
            return _addressDal.GetById(addressId);
        }

        /// <summary>
        /// Validates, normalizes, and creates an address.
        /// </summary>
        /// <param name="address">The address data to create.</param>
        /// <returns>The number of records affected.</returns>
        public int Create(Address address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            ValidateIdentifier(address.UserId, nameof(address.UserId));

            var normalizedAddress = CreateNormalizedAddress(address);
            return _addressDal.Insert(normalizedAddress);
        }

        /// <summary>
        /// Updates an address while preserving its owning user relationship.
        /// </summary>
        /// <param name="address">
        /// The address data, including its identifier and owning user identifier.
        /// </param>
        /// <returns>The number of records affected.</returns>
        public int Update(Address address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            ValidateIdentifier(address.AddressId, nameof(address.AddressId));
            ValidateIdentifier(address.UserId, nameof(address.UserId));

            var normalizedAddress = CreateNormalizedAddress(address);
            normalizedAddress.AddressId = address.AddressId;

            return _addressDal.Update(normalizedAddress);
        }

        /// <summary>
        /// Deletes an address owned by the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the address.</param>
        /// <param name="addressId">The address identifier.</param>
        /// <returns>The number of records affected.</returns>
        public int Delete(int userId, int addressId)
        {
            ValidateIdentifier(userId, nameof(userId));
            ValidateIdentifier(addressId, nameof(addressId));
            return _addressDal.Delete(userId, addressId);
        }

        private static Address CreateNormalizedAddress(Address address)
        {
            return new Address
            {
                UserId = address.UserId,
                RecipientName = NormalizeRequiredString(
                    address.RecipientName,
                    RecipientNameMaxLength,
                    nameof(address.RecipientName)),
                Phone = NormalizeRequiredString(
                    address.Phone,
                    PhoneMaxLength,
                    nameof(address.Phone)),
                Street = NormalizeRequiredString(
                    address.Street,
                    StreetMaxLength,
                    nameof(address.Street)),
                Ward = NormalizeOptionalString(
                    address.Ward,
                    WardMaxLength,
                    nameof(address.Ward)),
                District = NormalizeOptionalString(
                    address.District,
                    DistrictMaxLength,
                    nameof(address.District)),
                City = NormalizeRequiredString(
                    address.City,
                    CityMaxLength,
                    nameof(address.City)),
                IsDefault = address.IsDefault
            };
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

        private static string NormalizeRequiredString(
            string value,
            int maximumLength,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "A required address value must be provided.",
                    parameterName);
            }

            var normalizedValue = value.Trim();

            if (normalizedValue.Length > maximumLength)
            {
                throw new ArgumentException(
                    $"The address value cannot exceed {maximumLength} characters.",
                    parameterName);
            }

            return normalizedValue;
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
                    $"The address value cannot exceed {maximumLength} characters.",
                    parameterName);
            }

            return normalizedValue;
        }
    }
}
