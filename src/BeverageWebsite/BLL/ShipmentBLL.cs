using System;
using System.Collections.Generic;
using BeverageWebsite.DAL;
using BeverageWebsite.Models;

namespace BeverageWebsite.BLL
{
    /// <summary>
    /// Provides business operations for managing stored shipment records.
    /// </summary>
    public class ShipmentBLL
    {
        private const int ShippingProviderMaxLength = 100;
        private const int TrackingNumberMaxLength = 100;
        private const int ShipmentStatusMaxLength = 50;

        private static readonly HashSet<string> AllowedShipmentStatuses =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "Pending",
                "Packed",
                "Shipping",
                "Delivered",
                "Cancelled"
            };

        private readonly ShipmentDAL _shipmentDal;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShipmentBLL"/> class.
        /// </summary>
        public ShipmentBLL()
        {
            _shipmentDal = new ShipmentDAL();
        }

        /// <summary>
        /// Retrieves all stored shipment records.
        /// </summary>
        /// <returns>All shipments returned by the data access layer.</returns>
        public List<Shipment> GetAll()
        {
            return _shipmentDal.GetAll();
        }

        /// <summary>
        /// Retrieves a shipment by its identifier.
        /// </summary>
        /// <param name="shipmentId">The shipment identifier.</param>
        /// <returns>The matching shipment when found; otherwise, null.</returns>
        public Shipment GetById(int shipmentId)
        {
            ValidateIdentifier(shipmentId, nameof(shipmentId));
            return _shipmentDal.GetById(shipmentId);
        }

        /// <summary>
        /// Retrieves the single shipment associated with an order.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns>The order's shipment when found; otherwise, null.</returns>
        public Shipment GetByOrderId(int orderId)
        {
            ValidateIdentifier(orderId, nameof(orderId));
            return _shipmentDal.GetByOrderId(orderId);
        }

        /// <summary>
        /// Retrieves a shipment by its normalized tracking number.
        /// </summary>
        /// <param name="trackingNumber">The tracking number.</param>
        /// <returns>The matching shipment when found; otherwise, null.</returns>
        public Shipment GetByTrackingNumber(string trackingNumber)
        {
            return _shipmentDal.GetByTrackingNumber(
                NormalizeRequiredString(
                    trackingNumber,
                    TrackingNumberMaxLength,
                    nameof(trackingNumber)));
        }

        /// <summary>
        /// Validates, normalizes, and stores shipment record data only.
        /// </summary>
        /// <param name="shipment">The shipment record data to create.</param>
        /// <returns>The number of records affected.</returns>
        public int Create(Shipment shipment)
        {
            if (shipment == null)
            {
                throw new ArgumentNullException(nameof(shipment));
            }

            ValidateIdentifier(shipment.OrderId, nameof(shipment.OrderId));

            var normalizedShipment = CreateNormalizedShipment(shipment);
            return _shipmentDal.Insert(normalizedShipment);
        }

        /// <summary>
        /// Validates, normalizes, and updates stored shipment record data only.
        /// </summary>
        /// <param name="shipment">The shipment record data to update.</param>
        /// <returns>The number of records affected.</returns>
        public int Update(Shipment shipment)
        {
            if (shipment == null)
            {
                throw new ArgumentNullException(nameof(shipment));
            }

            ValidateIdentifier(shipment.ShipmentId, nameof(shipment.ShipmentId));
            ValidateIdentifier(shipment.OrderId, nameof(shipment.OrderId));

            var normalizedShipment = CreateNormalizedShipment(shipment);
            normalizedShipment.ShipmentId = shipment.ShipmentId;

            return _shipmentDal.Update(normalizedShipment);
        }

        /// <summary>
        /// Updates only shipment status without changing timestamps or the related order.
        /// </summary>
        /// <param name="shipmentId">The shipment identifier.</param>
        /// <param name="shipmentStatus">The new shipment status.</param>
        /// <returns>The number of records affected.</returns>
        public int UpdateStatus(int shipmentId, string shipmentStatus)
        {
            ValidateIdentifier(shipmentId, nameof(shipmentId));
            return _shipmentDal.UpdateStatus(
                shipmentId,
                NormalizeShipmentStatus(
                    shipmentStatus,
                    nameof(shipmentStatus)));
        }

        /// <summary>
        /// Deletes a stored shipment without deleting its related order.
        /// </summary>
        /// <param name="shipmentId">The shipment identifier.</param>
        /// <returns>The number of records affected.</returns>
        public int Delete(int shipmentId)
        {
            ValidateIdentifier(shipmentId, nameof(shipmentId));
            return _shipmentDal.Delete(shipmentId);
        }

        private static Shipment CreateNormalizedShipment(Shipment shipment)
        {
            return new Shipment
            {
                OrderId = shipment.OrderId,
                ShippingProvider = NormalizeOptionalString(
                    shipment.ShippingProvider,
                    ShippingProviderMaxLength,
                    nameof(shipment.ShippingProvider)),
                TrackingNumber = NormalizeOptionalString(
                    shipment.TrackingNumber,
                    TrackingNumberMaxLength,
                    nameof(shipment.TrackingNumber)),
                ShipmentStatus = NormalizeShipmentStatus(
                    shipment.ShipmentStatus,
                    nameof(shipment.ShipmentStatus)),
                ShippedAt = shipment.ShippedAt,
                DeliveredAt = shipment.DeliveredAt
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

        private static string NormalizeShipmentStatus(
            string shipmentStatus,
            string parameterName)
        {
            var normalizedStatus = NormalizeRequiredString(
                shipmentStatus,
                ShipmentStatusMaxLength,
                parameterName);

            if (!AllowedShipmentStatuses.Contains(normalizedStatus))
            {
                throw new ArgumentException(
                    "Shipment status is invalid.",
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
                    "A required shipment value must be provided.",
                    parameterName);
            }

            var normalizedValue = value.Trim();

            if (normalizedValue.Length > maximumLength)
            {
                throw new ArgumentException(
                    $"The shipment value cannot exceed {maximumLength} characters.",
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
                    $"The shipment value cannot exceed {maximumLength} characters.",
                    parameterName);
            }

            return normalizedValue;
        }
    }
}
