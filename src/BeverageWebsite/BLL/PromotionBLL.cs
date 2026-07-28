using System;
using System.Collections.Generic;
using BeverageWebsite.DAL;
using BeverageWebsite.Models;

namespace BeverageWebsite.BLL
{
    /// <summary>
    /// Provides business operations for managing promotions.
    /// </summary>
    public class PromotionBLL
    {
        private const int PromotionCodeMaxLength = 50;
        private const int PromotionNameMaxLength = 200;
        private const int DiscountTypeMaxLength = 50;

        private readonly PromotionDAL _promotionDal;

        /// <summary>
        /// Initializes a new instance of the <see cref="PromotionBLL"/> class.
        /// </summary>
        public PromotionBLL()
        {
            _promotionDal = new PromotionDAL();
        }

        /// <summary>
        /// Retrieves all promotions.
        /// </summary>
        /// <returns>All promotions returned by the data access layer.</returns>
        public List<Promotion> GetAll()
        {
            return _promotionDal.GetAll();
        }

        /// <summary>
        /// Retrieves a promotion by its identifier.
        /// </summary>
        /// <param name="promotionId">The promotion identifier.</param>
        /// <returns>The matching promotion when found; otherwise, null.</returns>
        public Promotion GetById(int promotionId)
        {
            ValidatePromotionId(promotionId);
            return _promotionDal.GetById(promotionId);
        }

        /// <summary>
        /// Retrieves a promotion by its normalized code.
        /// </summary>
        /// <param name="promotionCode">The promotion code.</param>
        /// <returns>The matching promotion when found; otherwise, null.</returns>
        public Promotion GetByCode(string promotionCode)
        {
            return _promotionDal.GetByCode(
                NormalizePromotionCode(promotionCode));
        }

        /// <summary>
        /// Retrieves promotions active at the supplied date and time.
        /// </summary>
        /// <param name="currentDate">The date and time used to identify active promotions.</param>
        /// <returns>The active promotions returned by the data access layer.</returns>
        public List<Promotion> GetActive(DateTime currentDate)
        {
            return _promotionDal.GetActive(currentDate);
        }

        /// <summary>
        /// Validates, normalizes, and creates a promotion.
        /// </summary>
        /// <param name="promotion">The promotion data to create.</param>
        /// <returns>The number of records affected.</returns>
        public int Create(Promotion promotion)
        {
            if (promotion == null)
            {
                throw new ArgumentNullException(nameof(promotion));
            }

            ValidateDiscountValue(promotion.DiscountValue);
            ValidateDateRange(promotion.StartDate, promotion.EndDate);

            var normalizedPromotion = new Promotion
            {
                PromotionCode = NormalizePromotionCode(promotion.PromotionCode),
                PromotionName = NormalizeRequiredText(
                    promotion.PromotionName,
                    PromotionNameMaxLength,
                    nameof(promotion.PromotionName)),
                DiscountType = NormalizeDiscountType(promotion.DiscountType),
                DiscountValue = promotion.DiscountValue,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                IsActive = promotion.IsActive
            };

            return _promotionDal.Insert(normalizedPromotion);
        }

        /// <summary>
        /// Validates, normalizes, and updates a promotion.
        /// </summary>
        /// <param name="promotion">The promotion data to update.</param>
        /// <returns>The number of records affected.</returns>
        public int Update(Promotion promotion)
        {
            if (promotion == null)
            {
                throw new ArgumentNullException(nameof(promotion));
            }

            ValidatePromotionId(promotion.PromotionId);
            ValidateDiscountValue(promotion.DiscountValue);
            ValidateDateRange(promotion.StartDate, promotion.EndDate);

            var normalizedPromotion = new Promotion
            {
                PromotionId = promotion.PromotionId,
                PromotionCode = NormalizePromotionCode(promotion.PromotionCode),
                PromotionName = NormalizeRequiredText(
                    promotion.PromotionName,
                    PromotionNameMaxLength,
                    nameof(promotion.PromotionName)),
                DiscountType = NormalizeDiscountType(promotion.DiscountType),
                DiscountValue = promotion.DiscountValue,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                IsActive = promotion.IsActive
            };

            return _promotionDal.Update(normalizedPromotion);
        }

        /// <summary>
        /// Deletes a promotion by its identifier.
        /// </summary>
        /// <param name="promotionId">The promotion identifier.</param>
        /// <returns>The number of records affected.</returns>
        public int Delete(int promotionId)
        {
            ValidatePromotionId(promotionId);
            return _promotionDal.Delete(promotionId);
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

        private static string NormalizePromotionCode(string promotionCode)
        {
            return NormalizeRequiredText(
                promotionCode,
                PromotionCodeMaxLength,
                nameof(promotionCode));
        }

        private static string NormalizeDiscountType(string discountType)
        {
            return NormalizeRequiredText(
                discountType,
                DiscountTypeMaxLength,
                nameof(discountType));
        }

        private static string NormalizeRequiredText(
            string value,
            int maximumLength,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A value is required.", parameterName);
            }

            var normalizedValue = value.Trim();

            if (normalizedValue.Length > maximumLength)
            {
                throw new ArgumentException(
                    $"The value cannot exceed {maximumLength} characters.",
                    parameterName);
            }

            return normalizedValue;
        }

        private static void ValidateDiscountValue(decimal discountValue)
        {
            if (discountValue < 0m)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(discountValue),
                    "Discount value must be greater than or equal to zero.");
            }
        }

        private static void ValidateDateRange(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                throw new ArgumentException(
                    "Promotion end date cannot be earlier than its start date.",
                    nameof(endDate));
            }
        }
    }
}
