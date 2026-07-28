using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BeverageWebsite.Models;

namespace BeverageWebsite.DAL
{
    /// <summary>
    /// Data access layer for Shipment operations using ADO.NET and the shared DataProvider.
    /// </summary>
    public class ShipmentDAL
    {
        private const int ShippingProviderMaxLength = 100;
        private const int TrackingNumberMaxLength = 100;
        private const int ShipmentStatusMaxLength = 50;
        private const byte ShipmentDateScale = 7;

        private static readonly HashSet<string> AllowedShipmentStatuses =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "Pending",
                "Packed",
                "Shipping",
                "Delivered",
                "Cancelled"
            };

        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShipmentDAL"/> class.
        /// </summary>
        public ShipmentDAL()
        {
            _dataProvider = new DataProvider();
        }

        /// <summary>
        /// Retrieves all shipment records.
        /// </summary>
        /// <returns>A list of <see cref="Shipment"/> objects ordered by identifier in descending order.</returns>
        public List<Shipment> GetAll()
        {
            var shipments = new List<Shipment>();
            const string sql = @"SELECT ShipmentId, OrderId, ShippingProvider, TrackingNumber, ShipmentStatus, ShippedAt, DeliveredAt
                                 FROM dbo.Shipment
                                 ORDER BY ShipmentId DESC";

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text))
                {
                    while (reader.Read())
                    {
                        shipments.Add(MapShipment(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve shipments.", ex);
            }

            return shipments;
        }

        /// <summary>
        /// Retrieves a shipment by its identifier.
        /// </summary>
        /// <param name="shipmentId">The shipment identifier.</param>
        /// <returns>A <see cref="Shipment"/> object if found; otherwise, null.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="shipmentId"/> is not greater than zero.
        /// </exception>
        public Shipment GetById(int shipmentId)
        {
            ValidateShipmentId(shipmentId);

            const string sql = @"SELECT ShipmentId, OrderId, ShippingProvider, TrackingNumber, ShipmentStatus, ShippedAt, DeliveredAt
                                 FROM dbo.Shipment
                                 WHERE ShipmentId = @ShipmentId";
            var parameters = new[]
            {
                new SqlParameter("@ShipmentId", SqlDbType.Int) { Value = shipmentId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapShipment(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve the shipment.", ex);
            }

            return null;
        }

        /// <summary>
        /// Retrieves the shipment associated with an order.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns>A <see cref="Shipment"/> object if found; otherwise, null.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="orderId"/> is not greater than zero.
        /// </exception>
        public Shipment GetByOrderId(int orderId)
        {
            ValidateOrderId(orderId);

            const string sql = @"SELECT ShipmentId, OrderId, ShippingProvider, TrackingNumber, ShipmentStatus, ShippedAt, DeliveredAt
                                 FROM dbo.Shipment
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
                        return MapShipment(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve the shipment for the order.", ex);
            }

            return null;
        }

        /// <summary>
        /// Retrieves a shipment by its exact tracking number.
        /// </summary>
        /// <param name="trackingNumber">The tracking number.</param>
        /// <returns>A <see cref="Shipment"/> object if found; otherwise, null.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="trackingNumber"/> is null, empty, whitespace, or longer than the database column.
        /// </exception>
        public Shipment GetByTrackingNumber(string trackingNumber)
        {
            ValidateRequiredString(
                trackingNumber,
                nameof(trackingNumber),
                TrackingNumberMaxLength);

            const string sql = @"SELECT ShipmentId, OrderId, ShippingProvider, TrackingNumber, ShipmentStatus, ShippedAt, DeliveredAt
                                 FROM dbo.Shipment
                                 WHERE TrackingNumber = @TrackingNumber
                                 ORDER BY ShipmentId DESC";
            var parameters = new[]
            {
                new SqlParameter("@TrackingNumber", SqlDbType.NVarChar, TrackingNumberMaxLength)
                {
                    Value = trackingNumber.Trim()
                }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapShipment(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve the shipment by tracking number.", ex);
            }

            return null;
        }

        /// <summary>
        /// Inserts a new shipment into the database.
        /// </summary>
        /// <param name="shipment">The shipment to insert.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shipment"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when shipment data violates the database constraints.</exception>
        public int Insert(Shipment shipment)
        {
            if (shipment == null)
            {
                throw new ArgumentNullException(nameof(shipment));
            }

            ValidateShipment(shipment);

            const string sql = @"INSERT INTO dbo.Shipment
                                     (OrderId, ShippingProvider, TrackingNumber, ShipmentStatus, ShippedAt, DeliveredAt)
                                 VALUES
                                     (@OrderId, @ShippingProvider, @TrackingNumber, @ShipmentStatus, @ShippedAt, @DeliveredAt)";
            var parameters = CreateWriteParameters(shipment);

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to insert the shipment.", ex);
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The shipment insert did not affect exactly one record.");
            }

            return affectedRows;
        }

        /// <summary>
        /// Updates an existing shipment while retaining its associated order.
        /// </summary>
        /// <param name="shipment">The shipment to update.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shipment"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when shipment data violates the database constraints.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the shipment or order identifier is not greater than zero.
        /// </exception>
        public int Update(Shipment shipment)
        {
            if (shipment == null)
            {
                throw new ArgumentNullException(nameof(shipment));
            }

            ValidateShipmentId(shipment.ShipmentId);
            ValidateShipment(shipment);

            const string sql = @"UPDATE dbo.Shipment
                                 SET ShippingProvider = @ShippingProvider,
                                     TrackingNumber = @TrackingNumber,
                                     ShipmentStatus = @ShipmentStatus,
                                     ShippedAt = @ShippedAt,
                                     DeliveredAt = @DeliveredAt
                                 WHERE ShipmentId = @ShipmentId
                                   AND OrderId = @OrderId";
            var writeParameters = CreateWriteParameters(shipment);
            var parameters = new SqlParameter[writeParameters.Length + 1];
            Array.Copy(writeParameters, parameters, writeParameters.Length);
            parameters[writeParameters.Length] =
                new SqlParameter("@ShipmentId", SqlDbType.Int) { Value = shipment.ShipmentId };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update the shipment.", ex);
            }

            if (affectedRows == 0)
            {
                throw new InvalidOperationException(
                    "The shipment was not found or does not match the expected order.");
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The shipment update did not affect exactly one record.");
            }

            return affectedRows;
        }

        /// <summary>
        /// Updates the status of a shipment.
        /// </summary>
        /// <param name="shipmentId">The shipment identifier.</param>
        /// <param name="status">The new shipment status.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="shipmentId"/> is not greater than zero.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="status"/> is null, empty, whitespace, or longer than the database column.
        /// </exception>
        public int UpdateStatus(int shipmentId, string status)
        {
            ValidateShipmentId(shipmentId);
            ValidateShipmentStatus(status, nameof(status));

            const string sql = @"UPDATE dbo.Shipment
                                 SET ShipmentStatus = @ShipmentStatus
                                 WHERE ShipmentId = @ShipmentId";
            var parameters = new[]
            {
                new SqlParameter("@ShipmentId", SqlDbType.Int) { Value = shipmentId },
                new SqlParameter("@ShipmentStatus", SqlDbType.NVarChar, ShipmentStatusMaxLength)
                {
                    Value = status.Trim()
                }
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update the shipment status.", ex);
            }

            if (affectedRows == 0)
            {
                throw new InvalidOperationException("The shipment was not found.");
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The shipment status update did not affect exactly one record.");
            }

            return affectedRows;
        }

        /// <summary>
        /// Deletes a shipment by its identifier.
        /// </summary>
        /// <param name="shipmentId">The shipment identifier.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="shipmentId"/> is not greater than zero.
        /// </exception>
        public int Delete(int shipmentId)
        {
            ValidateShipmentId(shipmentId);

            const string sql = @"DELETE FROM dbo.Shipment WHERE ShipmentId = @ShipmentId";
            var parameters = new[]
            {
                new SqlParameter("@ShipmentId", SqlDbType.Int) { Value = shipmentId }
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to delete the shipment.", ex);
            }

            if (affectedRows == 0)
            {
                throw new InvalidOperationException("The shipment was not found.");
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The shipment delete did not affect exactly one record.");
            }

            return affectedRows;
        }

        private static Shipment MapShipment(SqlDataReader reader)
        {
            var shipmentIdOrdinal = reader.GetOrdinal("ShipmentId");
            var orderIdOrdinal = reader.GetOrdinal("OrderId");
            var shippingProviderOrdinal = reader.GetOrdinal("ShippingProvider");
            var trackingNumberOrdinal = reader.GetOrdinal("TrackingNumber");
            var shipmentStatusOrdinal = reader.GetOrdinal("ShipmentStatus");
            var shippedAtOrdinal = reader.GetOrdinal("ShippedAt");
            var deliveredAtOrdinal = reader.GetOrdinal("DeliveredAt");

            return new Shipment
            {
                ShipmentId = reader.IsDBNull(shipmentIdOrdinal) ? 0 : reader.GetInt32(shipmentIdOrdinal),
                OrderId = reader.IsDBNull(orderIdOrdinal) ? 0 : reader.GetInt32(orderIdOrdinal),
                ShippingProvider = reader.IsDBNull(shippingProviderOrdinal)
                    ? null
                    : reader.GetString(shippingProviderOrdinal),
                TrackingNumber = reader.IsDBNull(trackingNumberOrdinal)
                    ? null
                    : reader.GetString(trackingNumberOrdinal),
                ShipmentStatus = reader.IsDBNull(shipmentStatusOrdinal)
                    ? null
                    : reader.GetString(shipmentStatusOrdinal),
                ShippedAt = reader.IsDBNull(shippedAtOrdinal)
                    ? (DateTime?)null
                    : reader.GetDateTime(shippedAtOrdinal),
                DeliveredAt = reader.IsDBNull(deliveredAtOrdinal)
                    ? (DateTime?)null
                    : reader.GetDateTime(deliveredAtOrdinal)
            };
        }

        private static SqlParameter[] CreateWriteParameters(Shipment shipment)
        {
            return new[]
            {
                new SqlParameter("@OrderId", SqlDbType.Int) { Value = shipment.OrderId },
                CreateOptionalStringParameter(
                    "@ShippingProvider",
                    shipment.ShippingProvider,
                    ShippingProviderMaxLength),
                CreateOptionalStringParameter(
                    "@TrackingNumber",
                    shipment.TrackingNumber,
                    TrackingNumberMaxLength),
                new SqlParameter("@ShipmentStatus", SqlDbType.NVarChar, ShipmentStatusMaxLength)
                {
                    Value = shipment.ShipmentStatus.Trim()
                },
                CreateNullableDateTimeParameter("@ShippedAt", shipment.ShippedAt),
                CreateNullableDateTimeParameter("@DeliveredAt", shipment.DeliveredAt)
            };
        }

        private static SqlParameter CreateOptionalStringParameter(
            string parameterName,
            string value,
            int maximumLength)
        {
            return new SqlParameter(parameterName, SqlDbType.NVarChar, maximumLength)
            {
                Value = string.IsNullOrWhiteSpace(value)
                    ? (object)DBNull.Value
                    : value.Trim()
            };
        }

        private static SqlParameter CreateNullableDateTimeParameter(
            string parameterName,
            DateTime? value)
        {
            return new SqlParameter(parameterName, SqlDbType.DateTime2)
            {
                Scale = ShipmentDateScale,
                Value = value.HasValue ? (object)value.Value : DBNull.Value
            };
        }

        private static void ValidateShipment(Shipment shipment)
        {
            ValidateOrderId(shipment.OrderId);
            ValidateOptionalString(
                shipment.ShippingProvider,
                nameof(shipment.ShippingProvider),
                ShippingProviderMaxLength);
            ValidateOptionalString(
                shipment.TrackingNumber,
                nameof(shipment.TrackingNumber),
                TrackingNumberMaxLength);
            ValidateShipmentStatus(
                shipment.ShipmentStatus,
                nameof(shipment.ShipmentStatus));

            if (shipment.ShippedAt.HasValue
                && shipment.DeliveredAt.HasValue
                && shipment.DeliveredAt.Value < shipment.ShippedAt.Value)
            {
                throw new ArgumentException(
                    "Delivery date cannot be earlier than the shipping date.",
                    nameof(shipment));
            }
        }

        private static void ValidateShipmentStatus(string value, string parameterName)
        {
            ValidateRequiredString(value, parameterName, ShipmentStatusMaxLength);

            if (!AllowedShipmentStatuses.Contains(value.Trim()))
            {
                throw new ArgumentException("Shipment status is invalid.", parameterName);
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

        private static void ValidateOptionalString(string value, string parameterName, int maximumLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var trimmedValue = value.Trim();
            if (trimmedValue.Length > maximumLength)
            {
                throw new ArgumentException(
                    $"{parameterName} cannot exceed {maximumLength} characters.",
                    parameterName);
            }
        }

        private static void ValidateShipmentId(int shipmentId)
        {
            if (shipmentId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(shipmentId),
                    "Shipment identifier must be greater than zero.");
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
