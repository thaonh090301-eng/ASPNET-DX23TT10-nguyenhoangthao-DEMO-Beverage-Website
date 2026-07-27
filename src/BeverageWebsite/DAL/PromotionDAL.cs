using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BeverageWebsite.Models;

namespace BeverageWebsite.DAL
{
    /// <summary>
    /// Data access layer for Promotion operations using ADO.NET and the shared DataProvider.
    /// </summary>
    public class PromotionDAL
    {
        private const int PromotionCodeMaxLength = 50;
        private const int PromotionNameMaxLength = 200;
        private const int DiscountTypeMaxLength = 50;
        private const byte DiscountValuePrecision = 12;
        private const byte DiscountValueScale = 2;
        private const byte DateTimeScale = 7;

        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PromotionDAL"/> class.
        /// </summary>
        public PromotionDAL()
        {
            _dataProvider = new DataProvider();
        }

        /// <summary>
        /// Retrieves all promotions.
        /// </summary>
        /// <returns>A list of <see cref="Promotion"/> objects ordered by identifier in descending order.</returns>
        public List<Promotion> GetAll()
        {
            var promotions = new List<Promotion>();
            const string sql = @"SELECT PromotionId, PromotionCode, PromotionName, DiscountType, DiscountValue, StartDate, EndDate, IsActive
                                 FROM dbo.Promotion
                                 ORDER BY PromotionId DESC";

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql))
                {
                    while (reader.Read())
                    {
                        promotions.Add(MapPromotion(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve promotions. Details: {ex.Message}", ex);
            }

            return promotions;
        }

        /// <summary>
        /// Retrieves a promotion by its identifier.
        /// </summary>
        /// <param name="promotionId">The promotion identifier.</param>
        /// <returns>A <see cref="Promotion"/> object if found; otherwise, null.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="promotionId"/> is not greater than zero.
        /// </exception>
        public Promotion GetById(int promotionId)
        {
            ValidatePromotionId(promotionId);

            const string sql = @"SELECT PromotionId, PromotionCode, PromotionName, DiscountType, DiscountValue, StartDate, EndDate, IsActive
                                 FROM dbo.Promotion
                                 WHERE PromotionId = @PromotionId";
            var parameters = new[]
            {
                new SqlParameter("@PromotionId", SqlDbType.Int) { Value = promotionId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapPromotion(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve promotion by identifier. Details: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Retrieves a promotion by its exact promotion code.
        /// </summary>
        /// <param name="code">The promotion code.</param>
        /// <returns>A <see cref="Promotion"/> object if found; otherwise, null.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="code"/> is null, empty, whitespace, or longer than the database column.
        /// </exception>
        public Promotion GetByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Promotion code is required.", nameof(code));
            }

            var trimmedCode = code.Trim();
            if (trimmedCode.Length > PromotionCodeMaxLength)
            {
                throw new ArgumentException(
                    $"Promotion code cannot exceed {PromotionCodeMaxLength} characters.",
                    nameof(code));
            }

            const string sql = @"SELECT PromotionId, PromotionCode, PromotionName, DiscountType, DiscountValue, StartDate, EndDate, IsActive
                                 FROM dbo.Promotion
                                 WHERE PromotionCode = @PromotionCode";
            var parameters = new[]
            {
                new SqlParameter("@PromotionCode", SqlDbType.NVarChar, PromotionCodeMaxLength)
                {
                    Value = trimmedCode
                }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapPromotion(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve promotion by code. Details: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Retrieves promotions that are active on the specified date and time.
        /// </summary>
        /// <param name="currentDate">The date and time used to determine whether a promotion is active.</param>
        /// <returns>A list of active <see cref="Promotion"/> objects.</returns>
        public List<Promotion> GetActive(DateTime currentDate)
        {
            var promotions = new List<Promotion>();
            const string sql = @"SELECT PromotionId, PromotionCode, PromotionName, DiscountType, DiscountValue, StartDate, EndDate, IsActive
                                 FROM dbo.Promotion
                                 WHERE IsActive = 1
                                   AND @CurrentDate >= StartDate
                                   AND @CurrentDate <= EndDate
                                 ORDER BY StartDate DESC, PromotionId DESC";
            var parameters = new[]
            {
                CreateDateTimeParameter("@CurrentDate", currentDate)
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    while (reader.Read())
                    {
                        promotions.Add(MapPromotion(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve active promotions. Details: {ex.Message}", ex);
            }

            return promotions;
        }

        /// <summary>
        /// Inserts a new promotion into the database.
        /// </summary>
        /// <param name="promotion">The promotion to insert.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="promotion"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when promotion data violates the database constraints.</exception>
        public int Insert(Promotion promotion)
        {
            if (promotion == null)
            {
                throw new ArgumentNullException(nameof(promotion));
            }

            ValidatePromotion(promotion);

            const string sql = @"INSERT INTO dbo.Promotion
                                     (PromotionCode, PromotionName, DiscountType, DiscountValue, StartDate, EndDate, IsActive)
                                 VALUES
                                     (@PromotionCode, @PromotionName, @DiscountType, @DiscountValue, @StartDate, @EndDate, @IsActive)";
            var parameters = CreateWriteParameters(promotion);

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to insert promotion. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing promotion in the database.
        /// </summary>
        /// <param name="promotion">The promotion to update.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="promotion"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when promotion data violates the database constraints.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the promotion identifier is not greater than zero.
        /// </exception>
        public int Update(Promotion promotion)
        {
            if (promotion == null)
            {
                throw new ArgumentNullException(nameof(promotion));
            }

            ValidatePromotionId(promotion.PromotionId);
            ValidatePromotion(promotion);

            const string sql = @"UPDATE dbo.Promotion
                                 SET PromotionCode = @PromotionCode,
                                     PromotionName = @PromotionName,
                                     DiscountType = @DiscountType,
                                     DiscountValue = @DiscountValue,
                                     StartDate = @StartDate,
                                     EndDate = @EndDate,
                                     IsActive = @IsActive
                                 WHERE PromotionId = @PromotionId";
            var writeParameters = CreateWriteParameters(promotion);
            var parameters = new SqlParameter[writeParameters.Length + 1];
            Array.Copy(writeParameters, parameters, writeParameters.Length);
            parameters[writeParameters.Length] =
                new SqlParameter("@PromotionId", SqlDbType.Int) { Value = promotion.PromotionId };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update promotion. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes a promotion by its identifier.
        /// </summary>
        /// <param name="promotionId">The promotion identifier.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="promotionId"/> is not greater than zero.
        /// </exception>
        public int Delete(int promotionId)
        {
            ValidatePromotionId(promotionId);

            const string sql = @"DELETE FROM dbo.Promotion WHERE PromotionId = @PromotionId";
            var parameters = new[]
            {
                new SqlParameter("@PromotionId", SqlDbType.Int) { Value = promotionId }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete promotion. Details: {ex.Message}", ex);
            }
        }

        private static Promotion MapPromotion(SqlDataReader reader)
        {
            var promotionIdOrdinal = reader.GetOrdinal("PromotionId");
            var promotionCodeOrdinal = reader.GetOrdinal("PromotionCode");
            var promotionNameOrdinal = reader.GetOrdinal("PromotionName");
            var discountTypeOrdinal = reader.GetOrdinal("DiscountType");
            var discountValueOrdinal = reader.GetOrdinal("DiscountValue");
            var startDateOrdinal = reader.GetOrdinal("StartDate");
            var endDateOrdinal = reader.GetOrdinal("EndDate");
            var isActiveOrdinal = reader.GetOrdinal("IsActive");

            return new Promotion
            {
                PromotionId = reader.IsDBNull(promotionIdOrdinal) ? 0 : reader.GetInt32(promotionIdOrdinal),
                PromotionCode = reader.IsDBNull(promotionCodeOrdinal) ? null : reader.GetString(promotionCodeOrdinal),
                PromotionName = reader.IsDBNull(promotionNameOrdinal) ? null : reader.GetString(promotionNameOrdinal),
                DiscountType = reader.IsDBNull(discountTypeOrdinal) ? null : reader.GetString(discountTypeOrdinal),
                DiscountValue = reader.IsDBNull(discountValueOrdinal) ? 0m : reader.GetDecimal(discountValueOrdinal),
                StartDate = reader.IsDBNull(startDateOrdinal) ? DateTime.MinValue : reader.GetDateTime(startDateOrdinal),
                EndDate = reader.IsDBNull(endDateOrdinal) ? DateTime.MinValue : reader.GetDateTime(endDateOrdinal),
                IsActive = !reader.IsDBNull(isActiveOrdinal) && reader.GetBoolean(isActiveOrdinal)
            };
        }

        private static SqlParameter[] CreateWriteParameters(Promotion promotion)
        {
            return new[]
            {
                new SqlParameter("@PromotionCode", SqlDbType.NVarChar, PromotionCodeMaxLength)
                {
                    Value = promotion.PromotionCode.Trim()
                },
                new SqlParameter("@PromotionName", SqlDbType.NVarChar, PromotionNameMaxLength)
                {
                    Value = promotion.PromotionName.Trim()
                },
                new SqlParameter("@DiscountType", SqlDbType.NVarChar, DiscountTypeMaxLength)
                {
                    Value = promotion.DiscountType.Trim()
                },
                CreateDecimalParameter("@DiscountValue", promotion.DiscountValue),
                CreateDateTimeParameter("@StartDate", promotion.StartDate),
                CreateDateTimeParameter("@EndDate", promotion.EndDate),
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = promotion.IsActive }
            };
        }

        private static SqlParameter CreateDecimalParameter(string parameterName, decimal value)
        {
            return new SqlParameter(parameterName, SqlDbType.Decimal)
            {
                Precision = DiscountValuePrecision,
                Scale = DiscountValueScale,
                Value = value
            };
        }

        private static SqlParameter CreateDateTimeParameter(string parameterName, DateTime value)
        {
            return new SqlParameter(parameterName, SqlDbType.DateTime2)
            {
                Scale = DateTimeScale,
                Value = value
            };
        }

        private static void ValidatePromotion(Promotion promotion)
        {
            ValidateRequiredString(
                promotion.PromotionCode,
                nameof(promotion.PromotionCode),
                PromotionCodeMaxLength);
            ValidateRequiredString(
                promotion.PromotionName,
                nameof(promotion.PromotionName),
                PromotionNameMaxLength);
            ValidateRequiredString(
                promotion.DiscountType,
                nameof(promotion.DiscountType),
                DiscountTypeMaxLength);

            if (promotion.DiscountValue < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(promotion.DiscountValue),
                    "Discount value cannot be negative.");
            }

            if (promotion.EndDate < promotion.StartDate)
            {
                throw new ArgumentException(
                    "Promotion end date cannot be earlier than its start date.",
                    nameof(promotion));
            }
        }

        private static void ValidateRequiredString(string value, string parameterName, int maximumLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{parameterName} is required.", parameterName);
            }

            var trimmedValue = value.Trim();
            if (trimmedValue.Length > maximumLength)
            {
                throw new ArgumentException(
                    $"{parameterName} cannot exceed {maximumLength} characters.",
                    parameterName);
            }
        }

        private static void ValidatePromotionId(int promotionId)
        {
            if (promotionId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(promotionId),
                    "Promotion identifier must be greater than zero.");
            }
        }
    }
}
