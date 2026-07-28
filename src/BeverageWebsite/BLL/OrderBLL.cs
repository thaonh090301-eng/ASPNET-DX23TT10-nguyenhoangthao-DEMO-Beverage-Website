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
        /// Creates an order from a cart in one data access transaction.
        /// Cart and address ownership are verified by the data access operation.
        /// Product prices and totals are loaded or calculated there, and order-item
        /// creation, inventory updates, and cart cleanup occur in the same transaction.
        /// </summary>
        /// <param name="order">The writable order inputs for checkout.</param>
        /// <param name="cartId">The cart identifier.</param>
        /// <returns>The identifier of the newly created order.</returns>
        public int CreateOrderFromCart(Order order, int cartId)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            ValidateIdentifier(order.UserId, nameof(order.UserId));
            ValidateIdentifier(cartId, nameof(cartId));
            ValidateIdentifier(order.AddressId, nameof(order.AddressId));
            ValidateNullableIdentifier(
                order.PromotionId,
                nameof(order.PromotionId));
            ValidateShippingFee(
                order.ShippingFee,
                nameof(order.ShippingFee));

            var checkoutOrder = new Order
            {
                UserId = order.UserId,
                AddressId = order.AddressId,
                PromotionId = order.PromotionId,
                ShippingFee = order.ShippingFee
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

        private static void ValidateIdentifier(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "The identifier must be greater than zero.");
            }
        }

        private static void ValidateNullableIdentifier(
            int? value,
            string parameterName)
        {
            if (value.HasValue && value.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "The identifier must be greater than zero when supplied.");
            }
        }

        private static void ValidateShippingFee(
            decimal shippingFee,
            string parameterName)
        {
            if (shippingFee < 0m)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Shipping fee must be greater than or equal to zero.");
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
