using System;
using System.Collections.Generic;
using BeverageWebsite.DAL;
using BeverageWebsite.Models;

namespace BeverageWebsite.BLL
{
    /// <summary>
    /// Provides business operations for reading, creating, and updating orders.
    /// </summary>
    public class OrderBLL
    {
        private const int OrderStatusMaxLength = 50;

        private static readonly HashSet<string> AllowedOrderStatuses =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "Pending",
                "Confirmed",
                "Processing",
                "Completed",
                "Cancelled"
            };

        private readonly OrderDAL _orderDal;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderBLL"/> class.
        /// </summary>
        public OrderBLL()
        {
            _orderDal = new OrderDAL();
        }

        /// <summary>
        /// Retrieves all orders.
        /// </summary>
        /// <returns>All orders returned by the data access layer.</returns>
        public List<Order> GetAll()
        {
            return _orderDal.GetAll();
        }

        /// <summary>
        /// Retrieves an order by its identifier.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns>The matching order when found; otherwise, null.</returns>
        public Order GetById(int orderId)
        {
            ValidateIdentifier(orderId, nameof(orderId));
            return _orderDal.GetById(orderId);
        }

        /// <summary>
        /// Retrieves an order owned by a user.
        /// </summary>
        /// <param name="userId">The owning user identifier.</param>
        /// <param name="orderId">The requested order identifier.</param>
        /// <returns>The owned order when found; otherwise, null.</returns>
        public Order GetById(int userId, int orderId)
        {
            ValidateIdentifier(userId, nameof(userId));
            ValidateIdentifier(orderId, nameof(orderId));
            return _orderDal.GetById(userId, orderId);
        }

        /// <summary>
        /// Retrieves all orders belonging to a user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user's orders returned by the data access layer.</returns>
        public List<Order> GetByUserId(int userId)
        {
            ValidateIdentifier(userId, nameof(userId));
            return _orderDal.GetByUserId(userId);
        }

        /// <summary>
        /// Retrieves all items belonging to an order without recalculating amounts.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns>The order items returned by the data access layer.</returns>
        public List<OrderItem> GetOrderItems(int orderId)
        {
            ValidateIdentifier(orderId, nameof(orderId));
            return _orderDal.GetOrderItems(orderId);
        }

        /// <summary>
        /// Retrieves all items belonging to an order owned by a user.
        /// </summary>
        /// <param name="userId">The owning user identifier.</param>
        /// <param name="orderId">The requested order identifier.</param>
        /// <returns>
        /// The owned order items, or an empty list when the order is missing, not owned, or has no items.
        /// </returns>
        public List<OrderItem> GetOrderItems(int userId, int orderId)
        {
            ValidateIdentifier(userId, nameof(userId));
            ValidateIdentifier(orderId, nameof(orderId));
            return _orderDal.GetOrderItems(userId, orderId);
        }

        /// <summary>
        /// Creates an order from the specified user's cart.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cartId">The cart identifier.</param>
        /// <param name="addressId">The shipping-address identifier.</param>
        /// <returns>The identifier of the newly created order.</returns>
        public int CreateOrderFromCart(
            int userId,
            int cartId,
            int addressId)
        {
            ValidateIdentifier(userId, nameof(userId));
            ValidateIdentifier(cartId, nameof(cartId));
            ValidateIdentifier(addressId, nameof(addressId));

            var checkoutOrder = new Order
            {
                UserId = userId,
                AddressId = addressId,
                PromotionId = null,
                ShippingFee = 0m
            };

            return _orderDal.CreateOrderFromCart(checkoutOrder, cartId);
        }

        /// <summary>
        /// Updates an order to an exact status allowed by the database schema.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <param name="orderStatus">The new order status.</param>
        /// <returns>The number of records affected.</returns>
        public int UpdateStatus(int orderId, string orderStatus)
        {
            ValidateIdentifier(orderId, nameof(orderId));
            return _orderDal.UpdateStatus(
                orderId,
                NormalizeOrderStatus(orderStatus));
        }

        /// <summary>
        /// Changes an order status when the requested business transition is valid.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <param name="newStatus">The requested new order status.</param>
        /// <returns>The number of records affected.</returns>
        public int ChangeStatus(int orderId, string newStatus)
        {
            ValidateIdentifier(orderId, nameof(orderId));
            var normalizedStatus = NormalizeOrderStatus(newStatus);

            Order order;

            try
            {
                order = _orderDal.GetById(orderId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "The order status could not be changed.",
                    ex);
            }

            if (order == null)
            {
                throw new InvalidOperationException("The order was not found.");
            }

            if (!IsAllowedStatusTransition(order.OrderStatus, normalizedStatus))
            {
                throw new InvalidOperationException(
                    "The requested order status transition is invalid.");
            }

            try
            {
                return _orderDal.ChangeStatus(
                    orderId,
                    order.OrderStatus,
                    normalizedStatus);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "The order status could not be changed.",
                    ex);
            }
        }

        private static bool IsAllowedStatusTransition(
            string currentStatus,
            string newStatus)
        {
            if (string.Equals(currentStatus, "Pending", StringComparison.Ordinal))
            {
                return string.Equals(newStatus, "Confirmed", StringComparison.Ordinal)
                    || string.Equals(newStatus, "Cancelled", StringComparison.Ordinal);
            }

            if (string.Equals(currentStatus, "Confirmed", StringComparison.Ordinal))
            {
                return string.Equals(newStatus, "Processing", StringComparison.Ordinal)
                    || string.Equals(newStatus, "Cancelled", StringComparison.Ordinal);
            }

            if (string.Equals(currentStatus, "Processing", StringComparison.Ordinal))
            {
                return string.Equals(newStatus, "Completed", StringComparison.Ordinal);
            }

            return false;
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

        private static string NormalizeOrderStatus(string orderStatus)
        {
            if (string.IsNullOrWhiteSpace(orderStatus))
            {
                throw new ArgumentException(
                    "Order status is required.",
                    nameof(orderStatus));
            }

            var normalizedStatus = orderStatus.Trim();

            if (normalizedStatus.Length > OrderStatusMaxLength)
            {
                throw new ArgumentException(
                    $"Order status cannot exceed {OrderStatusMaxLength} characters.",
                    nameof(orderStatus));
            }

            if (!AllowedOrderStatuses.Contains(normalizedStatus))
            {
                throw new ArgumentException(
                    "Order status is invalid.",
                    nameof(orderStatus));
            }

            return normalizedStatus;
        }
    }
}
