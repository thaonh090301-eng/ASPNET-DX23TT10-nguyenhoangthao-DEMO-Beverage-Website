using System;
using System.Collections.Generic;
using BeverageWebsite.DAL;
using BeverageWebsite.Models;

namespace BeverageWebsite.BLL
{
    /// <summary>
    /// Provides business operations for managing users and stored credentials.
    /// </summary>
    public class UserBLL
    {
        private const int UserNameMaxLength = 100;
        private const int EmailMaxLength = 255;
        private const int PasswordHashMaxLength = 255;
        private const int FullNameMaxLength = 200;
        private const int PhoneMaxLength = 20;
        private const int RoleMaxLength = 20;

        private static readonly HashSet<string> AllowedRoles =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "Admin",
                "Customer",
                "Staff"
            };

        private readonly UserDAL _userDal;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserBLL"/> class.
        /// </summary>
        public UserBLL()
        {
            _userDal = new UserDAL();
        }

        /// <summary>
        /// Retrieves all users without credential data.
        /// </summary>
        /// <returns>All users returned by the general data access lookup.</returns>
        public List<User> GetAll()
        {
            return _userDal.GetAll();
        }

        /// <summary>
        /// Retrieves a user without credential data by identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The matching user when found; otherwise, null.</returns>
        public User GetById(int userId)
        {
            ValidateUserId(userId, nameof(userId));
            return _userDal.GetById(userId);
        }

        /// <summary>
        /// Retrieves a user without credential data by email address.
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <returns>The matching user when found; otherwise, null.</returns>
        public User GetByEmail(string email)
        {
            return _userDal.GetByEmail(
                NormalizeEmail(email, nameof(email)));
        }

        /// <summary>
        /// Retrieves credential data solely for authentication verification.
        /// </summary>
        /// <param name="email">The email address used for authentication lookup.</param>
        /// <returns>
        /// The matching user including the stored password hash when found; otherwise, null.
        /// </returns>
        public User GetByEmailForAuthentication(string email)
        {
            return _userDal.GetByEmailForAuthentication(
                NormalizeEmail(email, nameof(email)));
        }

        /// <summary>
        /// Determines whether a user exists for an email address.
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <returns>True when the email exists; otherwise, false.</returns>
        public bool ExistsByEmail(string email)
        {
            return _userDal.ExistsByEmail(
                NormalizeEmail(email, nameof(email)));
        }

        /// <summary>
        /// Validates, normalizes, and creates a user with an already-created password hash.
        /// </summary>
        /// <param name="user">The user data to create.</param>
        /// <returns>The number of records affected.</returns>
        public int Create(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            ValidatePasswordHash(
                user.PasswordHash,
                nameof(user.PasswordHash));

            var normalizedUser = new User
            {
                UserName = NormalizeRequiredString(
                    user.UserName,
                    UserNameMaxLength,
                    nameof(user.UserName)),
                Email = NormalizeEmail(user.Email, nameof(user.Email)),
                PasswordHash = user.PasswordHash,
                FullName = NormalizeOptionalString(
                    user.FullName,
                    FullNameMaxLength,
                    nameof(user.FullName)),
                Phone = NormalizeOptionalString(
                    user.Phone,
                    PhoneMaxLength,
                    nameof(user.Phone)),
                Role = NormalizeRole(user.Role, nameof(user.Role)),
                IsActive = user.IsActive
            };

            return _userDal.Insert(normalizedUser);
        }

        /// <summary>
        /// Updates a user profile without modifying the stored password hash.
        /// </summary>
        /// <param name="user">The user profile data to update.</param>
        /// <returns>The number of records affected.</returns>
        public int Update(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            ValidateUserId(user.UserId, nameof(user.UserId));

            var normalizedUser = new User
            {
                UserId = user.UserId,
                UserName = NormalizeRequiredString(
                    user.UserName,
                    UserNameMaxLength,
                    nameof(user.UserName)),
                Email = NormalizeEmail(user.Email, nameof(user.Email)),
                FullName = NormalizeOptionalString(
                    user.FullName,
                    FullNameMaxLength,
                    nameof(user.FullName)),
                Phone = NormalizeOptionalString(
                    user.Phone,
                    PhoneMaxLength,
                    nameof(user.Phone)),
                Role = NormalizeRole(user.Role, nameof(user.Role)),
                IsActive = user.IsActive
            };

            return _userDal.Update(normalizedUser);
        }

        /// <summary>
        /// Stores an already-created password hash for a user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="passwordHash">The already-created password hash to store unchanged.</param>
        /// <returns>The number of records affected.</returns>
        public int UpdatePasswordHash(int userId, string passwordHash)
        {
            ValidateUserId(userId, nameof(userId));
            ValidatePasswordHash(passwordHash, nameof(passwordHash));
            return _userDal.UpdatePasswordHash(userId, passwordHash);
        }

        /// <summary>
        /// Deletes a user by identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The number of records affected.</returns>
        public int Delete(int userId)
        {
            ValidateUserId(userId, nameof(userId));
            return _userDal.Delete(userId);
        }

        private static void ValidateUserId(int userId, string parameterName)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "User identifier must be greater than zero.");
            }
        }

        private static string NormalizeRequiredString(
            string value,
            int maximumLength,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A required user value must be provided.", parameterName);
            }

            var normalizedValue = value.Trim();

            if (normalizedValue.Length > maximumLength)
            {
                throw new ArgumentException(
                    $"The user value cannot exceed {maximumLength} characters.",
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
                    $"The user value cannot exceed {maximumLength} characters.",
                    parameterName);
            }

            return normalizedValue;
        }

        private static string NormalizeEmail(string email, string parameterName)
        {
            return NormalizeRequiredString(
                email,
                EmailMaxLength,
                parameterName);
        }

        private static void ValidatePasswordHash(
            string passwordHash,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Password hash is required.", parameterName);
            }

            if (passwordHash.Length > PasswordHashMaxLength)
            {
                throw new ArgumentException(
                    $"Password hash cannot exceed {PasswordHashMaxLength} characters.",
                    parameterName);
            }
        }

        private static string NormalizeRole(string role, string parameterName)
        {
            var normalizedRole = NormalizeRequiredString(
                role,
                RoleMaxLength,
                parameterName);

            if (!AllowedRoles.Contains(normalizedRole))
            {
                throw new ArgumentException("Role is invalid.", parameterName);
            }

            return normalizedRole;
        }
    }
}
