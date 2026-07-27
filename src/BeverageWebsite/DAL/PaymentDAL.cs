using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BeverageWebsite.Models;

namespace BeverageWebsite.DAL
{
    /// <summary>
    /// Data access layer for Payment operations using ADO.NET and the shared DataProvider.
    /// </summary>
    public class PaymentDAL
    {
        private const int PaymentMethodMaxLength = 50;
        private const int PaymentStatusMaxLength = 50;
        private const int TransactionReferenceMaxLength = 255;
        private const byte PaidAmountPrecision = 12;
        private const byte PaidAmountScale = 2;
        private const byte PaidAtScale = 7;

        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentDAL"/> class.
        /// </summary>
        public PaymentDAL()
        {
            _dataProvider = new DataProvider();
        }

        /// <summary>
        /// Retrieves all payment records.
        /// </summary>
        /// <returns>A list of <see cref="Payment"/> objects ordered by identifier in descending order.</returns>
        public List<Payment> GetAll()
        {
            var payments = new List<Payment>();
            const string sql = @"SELECT PaymentId, OrderId, PaymentMethod, PaymentStatus, PaidAmount, PaidAt, TransactionReference
                                 FROM dbo.Payment
                                 ORDER BY PaymentId DESC";

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text))
                {
                    while (reader.Read())
                    {
                        payments.Add(MapPayment(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve payments.", ex);
            }

            return payments;
        }

        /// <summary>
        /// Retrieves a payment by its identifier.
        /// </summary>
        /// <param name="paymentId">The payment identifier.</param>
        /// <returns>A <see cref="Payment"/> object if found; otherwise, null.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="paymentId"/> is not greater than zero.
        /// </exception>
        public Payment GetById(int paymentId)
        {
            ValidatePaymentId(paymentId);

            const string sql = @"SELECT PaymentId, OrderId, PaymentMethod, PaymentStatus, PaidAmount, PaidAt, TransactionReference
                                 FROM dbo.Payment
                                 WHERE PaymentId = @PaymentId";
            var parameters = new[]
            {
                new SqlParameter("@PaymentId", SqlDbType.Int) { Value = paymentId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapPayment(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve payment by identifier.", ex);
            }

            return null;
        }

        /// <summary>
        /// Retrieves the payment associated with an order.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns>A <see cref="Payment"/> object if found; otherwise, null.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="orderId"/> is not greater than zero.
        /// </exception>
        public Payment GetByOrderId(int orderId)
        {
            ValidateOrderId(orderId);

            const string sql = @"SELECT PaymentId, OrderId, PaymentMethod, PaymentStatus, PaidAmount, PaidAt, TransactionReference
                                 FROM dbo.Payment
                                 WHERE OrderId = @OrderId";
            var parameters = new[]
            {
                new SqlParameter("@OrderId", SqlDbType.Int) { Value = orderId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapPayment(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve payment by order identifier.", ex);
            }

            return null;
        }

        /// <summary>
        /// Inserts a new payment into the database.
        /// </summary>
        /// <param name="payment">The payment to insert.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="payment"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when payment data violates the database constraints.</exception>
        public int Insert(Payment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }

            ValidatePayment(payment);

            const string sql = @"INSERT INTO dbo.Payment
                                     (OrderId, PaymentMethod, PaymentStatus, PaidAmount, PaidAt, TransactionReference)
                                 VALUES
                                     (@OrderId, @PaymentMethod, @PaymentStatus, @PaidAmount, @PaidAt, @TransactionReference)";
            var parameters = CreateWriteParameters(payment);

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to insert payment.", ex);
            }
        }

        /// <summary>
        /// Updates an existing payment while retaining its associated order.
        /// </summary>
        /// <param name="payment">The payment to update.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="payment"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when payment data violates the database constraints.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the payment or order identifier is not greater than zero.
        /// </exception>
        public int Update(Payment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }

            ValidatePaymentId(payment.PaymentId);
            ValidatePayment(payment);

            const string sql = @"UPDATE dbo.Payment
                                 SET PaymentMethod = @PaymentMethod,
                                     PaymentStatus = @PaymentStatus,
                                     PaidAmount = @PaidAmount,
                                     PaidAt = @PaidAt,
                                     TransactionReference = @TransactionReference
                                 WHERE PaymentId = @PaymentId
                                   AND OrderId = @OrderId";
            var writeParameters = CreateWriteParameters(payment);
            var parameters = new SqlParameter[writeParameters.Length + 1];
            Array.Copy(writeParameters, parameters, writeParameters.Length);
            parameters[writeParameters.Length] =
                new SqlParameter("@PaymentId", SqlDbType.Int) { Value = payment.PaymentId };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update payment.", ex);
            }
        }

        /// <summary>
        /// Updates the status of a payment.
        /// </summary>
        /// <param name="paymentId">The payment identifier.</param>
        /// <param name="status">The new payment status.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="paymentId"/> is not greater than zero.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="status"/> is null, empty, whitespace, or longer than the database column.
        /// </exception>
        public int UpdateStatus(int paymentId, string status)
        {
            ValidatePaymentId(paymentId);
            ValidateRequiredString(status, nameof(status), PaymentStatusMaxLength);

            const string sql = @"UPDATE dbo.Payment
                                 SET PaymentStatus = @PaymentStatus
                                 WHERE PaymentId = @PaymentId";
            var parameters = new[]
            {
                new SqlParameter("@PaymentId", SqlDbType.Int) { Value = paymentId },
                new SqlParameter("@PaymentStatus", SqlDbType.NVarChar, PaymentStatusMaxLength)
                {
                    Value = status.Trim()
                }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update payment status.", ex);
            }
        }

        /// <summary>
        /// Deletes a payment by its identifier.
        /// </summary>
        /// <param name="paymentId">The payment identifier.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="paymentId"/> is not greater than zero.
        /// </exception>
        public int Delete(int paymentId)
        {
            ValidatePaymentId(paymentId);

            const string sql = @"DELETE FROM dbo.Payment WHERE PaymentId = @PaymentId";
            var parameters = new[]
            {
                new SqlParameter("@PaymentId", SqlDbType.Int) { Value = paymentId }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to delete payment.", ex);
            }
        }

        private static Payment MapPayment(SqlDataReader reader)
        {
            var paymentIdOrdinal = reader.GetOrdinal("PaymentId");
            var orderIdOrdinal = reader.GetOrdinal("OrderId");
            var paymentMethodOrdinal = reader.GetOrdinal("PaymentMethod");
            var paymentStatusOrdinal = reader.GetOrdinal("PaymentStatus");
            var paidAmountOrdinal = reader.GetOrdinal("PaidAmount");
            var paidAtOrdinal = reader.GetOrdinal("PaidAt");
            var transactionReferenceOrdinal = reader.GetOrdinal("TransactionReference");

            return new Payment
            {
                PaymentId = reader.IsDBNull(paymentIdOrdinal) ? 0 : reader.GetInt32(paymentIdOrdinal),
                OrderId = reader.IsDBNull(orderIdOrdinal) ? 0 : reader.GetInt32(orderIdOrdinal),
                PaymentMethod = reader.IsDBNull(paymentMethodOrdinal) ? null : reader.GetString(paymentMethodOrdinal),
                PaymentStatus = reader.IsDBNull(paymentStatusOrdinal) ? null : reader.GetString(paymentStatusOrdinal),
                PaidAmount = reader.IsDBNull(paidAmountOrdinal) ? 0m : reader.GetDecimal(paidAmountOrdinal),
                PaidAt = reader.IsDBNull(paidAtOrdinal) ? (DateTime?)null : reader.GetDateTime(paidAtOrdinal),
                TransactionReference = reader.IsDBNull(transactionReferenceOrdinal)
                    ? null
                    : reader.GetString(transactionReferenceOrdinal)
            };
        }

        private static SqlParameter[] CreateWriteParameters(Payment payment)
        {
            return new[]
            {
                new SqlParameter("@OrderId", SqlDbType.Int) { Value = payment.OrderId },
                new SqlParameter("@PaymentMethod", SqlDbType.NVarChar, PaymentMethodMaxLength)
                {
                    Value = payment.PaymentMethod.Trim()
                },
                new SqlParameter("@PaymentStatus", SqlDbType.NVarChar, PaymentStatusMaxLength)
                {
                    Value = payment.PaymentStatus.Trim()
                },
                CreateDecimalParameter("@PaidAmount", payment.PaidAmount),
                new SqlParameter("@PaidAt", SqlDbType.DateTime2)
                {
                    Scale = PaidAtScale,
                    Value = payment.PaidAt.HasValue ? (object)payment.PaidAt.Value : DBNull.Value
                },
                new SqlParameter("@TransactionReference", SqlDbType.NVarChar, TransactionReferenceMaxLength)
                {
                    Value = string.IsNullOrWhiteSpace(payment.TransactionReference)
                        ? (object)DBNull.Value
                        : payment.TransactionReference.Trim()
                }
            };
        }

        private static SqlParameter CreateDecimalParameter(string parameterName, decimal value)
        {
            return new SqlParameter(parameterName, SqlDbType.Decimal)
            {
                Precision = PaidAmountPrecision,
                Scale = PaidAmountScale,
                Value = value
            };
        }

        private static void ValidatePayment(Payment payment)
        {
            ValidateOrderId(payment.OrderId);
            ValidateRequiredString(
                payment.PaymentMethod,
                nameof(payment.PaymentMethod),
                PaymentMethodMaxLength);
            ValidateRequiredString(
                payment.PaymentStatus,
                nameof(payment.PaymentStatus),
                PaymentStatusMaxLength);

            if (payment.PaidAmount < 0m)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(payment.PaidAmount),
                    "Paid amount cannot be negative.");
            }

            if (!string.IsNullOrWhiteSpace(payment.TransactionReference))
            {
                var trimmedTransactionReference = payment.TransactionReference.Trim();
                if (trimmedTransactionReference.Length > TransactionReferenceMaxLength)
                {
                    throw new ArgumentException(
                        $"TransactionReference cannot exceed {TransactionReferenceMaxLength} characters.",
                        nameof(payment.TransactionReference));
                }
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

        private static void ValidatePaymentId(int paymentId)
        {
            if (paymentId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(paymentId),
                    "Payment identifier must be greater than zero.");
            }
        }

        private static void ValidateOrderId(int orderId)
        {
            if (orderId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(orderId),
                    "Order identifier must be greater than zero.");
            }
        }
    }
}
