using System;
using System.Collections.Generic;
using BeverageWebsite.DAL;
using BeverageWebsite.Models;

namespace BeverageWebsite.BLL
{
    /// <summary>
    /// Provides business operations for managing stored payment records.
    /// </summary>
    public class PaymentBLL
    {
        private const int PaymentMethodMaxLength = 50;
        private const int PaymentStatusMaxLength = 50;
        private const int TransactionReferenceMaxLength = 255;

        private static readonly HashSet<string> AllowedPaymentMethods =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "Cash",
                "Card",
                "BankTransfer",
                "DigitalWallet"
            };

        private static readonly HashSet<string> AllowedPaymentStatuses =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "Pending",
                "Paid",
                "Failed",
                "Refunded"
            };

        private readonly PaymentDAL _paymentDal;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentBLL"/> class.
        /// </summary>
        public PaymentBLL()
        {
            _paymentDal = new PaymentDAL();
        }

        /// <summary>
        /// Retrieves all stored payment records.
        /// </summary>
        /// <returns>All payments returned by the data access layer.</returns>
        public List<Payment> GetAll()
        {
            return _paymentDal.GetAll();
        }

        /// <summary>
        /// Retrieves a payment by its identifier.
        /// </summary>
        /// <param name="paymentId">The payment identifier.</param>
        /// <returns>The matching payment when found; otherwise, null.</returns>
        public Payment GetById(int paymentId)
        {
            ValidateIdentifier(paymentId, nameof(paymentId));
            return _paymentDal.GetById(paymentId);
        }

        /// <summary>
        /// Retrieves the single payment associated with an order.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns>The order's payment when found; otherwise, null.</returns>
        public Payment GetByOrderId(int orderId)
        {
            ValidateIdentifier(orderId, nameof(orderId));
            return _paymentDal.GetByOrderId(orderId);
        }

        /// <summary>
        /// Validates, normalizes, and stores a payment record only.
        /// </summary>
        /// <param name="payment">The payment record data to create.</param>
        /// <returns>The number of records affected.</returns>
        public int Create(Payment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }

            ValidateIdentifier(payment.OrderId, nameof(payment.OrderId));

            var normalizedPayment = CreateNormalizedPayment(payment);
            return _paymentDal.Insert(normalizedPayment);
        }

        /// <summary>
        /// Validates, normalizes, and updates a stored payment record only.
        /// </summary>
        /// <param name="payment">The payment record data to update.</param>
        /// <returns>The number of records affected.</returns>
        public int Update(Payment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }

            ValidateIdentifier(payment.PaymentId, nameof(payment.PaymentId));
            ValidateIdentifier(payment.OrderId, nameof(payment.OrderId));

            var normalizedPayment = CreateNormalizedPayment(payment);
            normalizedPayment.PaymentId = payment.PaymentId;

            return _paymentDal.Update(normalizedPayment);
        }

        /// <summary>
        /// Updates only a payment status without changing its related order,
        /// timestamps, amount, or transaction reference automatically.
        /// </summary>
        /// <param name="paymentId">The payment identifier.</param>
        /// <param name="paymentStatus">The new payment status.</param>
        /// <returns>The number of records affected.</returns>
        public int UpdateStatus(int paymentId, string paymentStatus)
        {
            ValidateIdentifier(paymentId, nameof(paymentId));
            return _paymentDal.UpdateStatus(
                paymentId,
                NormalizePaymentStatus(
                    paymentStatus,
                    nameof(paymentStatus)));
        }

        /// <summary>
        /// Deletes a stored payment record without deleting its related order.
        /// </summary>
        /// <param name="paymentId">The payment identifier.</param>
        /// <returns>The number of records affected.</returns>
        public int Delete(int paymentId)
        {
            ValidateIdentifier(paymentId, nameof(paymentId));
            return _paymentDal.Delete(paymentId);
        }

        private static Payment CreateNormalizedPayment(Payment payment)
        {
            ValidatePaidAmount(
                payment.PaidAmount,
                nameof(payment.PaidAmount));

            return new Payment
            {
                OrderId = payment.OrderId,
                PaymentMethod = NormalizePaymentMethod(
                    payment.PaymentMethod,
                    nameof(payment.PaymentMethod)),
                PaymentStatus = NormalizePaymentStatus(
                    payment.PaymentStatus,
                    nameof(payment.PaymentStatus)),
                PaidAmount = payment.PaidAmount,
                PaidAt = payment.PaidAt,
                TransactionReference = NormalizeOptionalString(
                    payment.TransactionReference,
                    TransactionReferenceMaxLength,
                    nameof(payment.TransactionReference))
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

        private static void ValidatePaidAmount(
            decimal paidAmount,
            string parameterName)
        {
            if (paidAmount < 0m)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Paid amount must be greater than or equal to zero.");
            }
        }

        private static string NormalizePaymentMethod(
            string paymentMethod,
            string parameterName)
        {
            var normalizedMethod = NormalizeRequiredString(
                paymentMethod,
                PaymentMethodMaxLength,
                parameterName);

            if (!AllowedPaymentMethods.Contains(normalizedMethod))
            {
                throw new ArgumentException(
                    "Payment method is invalid.",
                    parameterName);
            }

            return normalizedMethod;
        }

        private static string NormalizePaymentStatus(
            string paymentStatus,
            string parameterName)
        {
            var normalizedStatus = NormalizeRequiredString(
                paymentStatus,
                PaymentStatusMaxLength,
                parameterName);

            if (!AllowedPaymentStatuses.Contains(normalizedStatus))
            {
                throw new ArgumentException(
                    "Payment status is invalid.",
                    parameterName);
            }

            return normalizedStatus;
        }

        private static string NormalizeRequiredString(
            string value,
            int maximumLength,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "A required payment value must be provided.",
                    parameterName);
            }

            var normalizedValue = value.Trim();

            if (normalizedValue.Length > maximumLength)
            {
                throw new ArgumentException(
                    $"The payment value cannot exceed {maximumLength} characters.",
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
                    $"The payment value cannot exceed {maximumLength} characters.",
                    parameterName);
            }

            return normalizedValue;
        }
    }
}
