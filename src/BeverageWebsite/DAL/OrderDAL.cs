using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BeverageWebsite.Models;

namespace BeverageWebsite.DAL
{
    /// <summary>
    /// Data access layer for order operations using ADO.NET and the shared DataProvider.
    /// </summary>
    public class OrderDAL
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

        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderDAL"/> class.
        /// </summary>
        public OrderDAL()
        {
            _dataProvider = new DataProvider();
        }

        /// <summary>
        /// Retrieves all orders, newest first.
        /// </summary>
        /// <returns>A list of <see cref="Order"/> objects.</returns>
        public List<Order> GetAll()
        {
            var orders = new List<Order>();
            const string sql = @"SELECT O.OrderId, O.UserId, O.AddressId, O.PromotionId, O.OrderDate,
                                        O.OrderStatus, O.TotalAmount, O.ShippingFee, O.FinalAmount
                                 FROM dbo.[Order] AS O
                                 ORDER BY O.OrderDate DESC, O.OrderId DESC";

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text))
                {
                    while (reader.Read())
                    {
                        orders.Add(MapOrder(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve orders.", ex);
            }

            return orders;
        }

        /// <summary>
        /// Retrieves an order by its identifier.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns>An <see cref="Order"/> object if found; otherwise, null.</returns>
        public Order GetById(int orderId)
        {
            if (orderId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(orderId), "Order identifier must be greater than zero.");
            }

            const string sql = @"SELECT O.OrderId, O.UserId, O.AddressId, O.PromotionId, O.OrderDate,
                                        O.OrderStatus, O.TotalAmount, O.ShippingFee, O.FinalAmount
                                 FROM dbo.[Order] AS O
                                 WHERE O.OrderId = @OrderId";
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
                        return MapOrder(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve the order.", ex);
            }

            return null;
        }

        /// <summary>
        /// Retrieves an order owned by a user.
        /// </summary>
        /// <param name="userId">The owning user identifier.</param>
        /// <param name="orderId">The requested order identifier.</param>
        /// <returns>The owned <see cref="Order"/> when found; otherwise, null.</returns>
        public Order GetById(int userId, int orderId)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User identifier must be greater than zero.");
            }

            if (orderId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(orderId), "Order identifier must be greater than zero.");
            }

            const string sql = @"SELECT O.OrderId, O.UserId, O.AddressId, O.PromotionId, O.OrderDate,
                                        O.OrderStatus, O.TotalAmount, O.ShippingFee, O.FinalAmount
                                 FROM dbo.[Order] AS O
                                 WHERE O.OrderId = @OrderId
                                   AND O.UserId = @UserId";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@OrderId", SqlDbType.Int) { Value = orderId }
            };

            using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
            {
                if (reader.Read())
                {
                    return MapOrder(reader);
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves all orders for a user, newest first.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A list of the user's <see cref="Order"/> objects.</returns>
        public List<Order> GetByUserId(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User identifier must be greater than zero.");
            }

            var orders = new List<Order>();
            const string sql = @"SELECT O.OrderId, O.UserId, O.AddressId, O.PromotionId, O.OrderDate,
                                        O.OrderStatus, O.TotalAmount, O.ShippingFee, O.FinalAmount
                                 FROM dbo.[Order] AS O
                                 WHERE O.UserId = @UserId
                                 ORDER BY O.OrderDate DESC, O.OrderId DESC";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    while (reader.Read())
                    {
                        orders.Add(MapOrder(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve orders for the user.", ex);
            }

            return orders;
        }

        /// <summary>
        /// Retrieves all items belonging to an order.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <returns>A list of <see cref="OrderItem"/> objects.</returns>
        public List<OrderItem> GetOrderItems(int orderId)
        {
            if (orderId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(orderId), "Order identifier must be greater than zero.");
            }

            var items = new List<OrderItem>();
            const string sql = @"SELECT OrderItemId, OrderId, ProductId, Quantity, UnitPrice, DiscountAmount, LineTotal
                                 FROM dbo.OrderItem
                                 WHERE OrderId = @OrderId
                                 ORDER BY OrderItemId";
            var parameters = new[]
            {
                new SqlParameter("@OrderId", SqlDbType.Int) { Value = orderId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    while (reader.Read())
                    {
                        items.Add(MapOrderItem(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve order items.", ex);
            }

            return items;
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
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User identifier must be greater than zero.");
            }

            if (orderId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(orderId), "Order identifier must be greater than zero.");
            }

            var items = new List<OrderItem>();
            const string sql = @"SELECT OI.OrderItemId, OI.OrderId, OI.ProductId, OI.Quantity,
                                        OI.UnitPrice, OI.DiscountAmount, OI.LineTotal
                                 FROM dbo.OrderItem AS OI
                                 INNER JOIN dbo.[Order] AS O
                                     ON O.OrderId = OI.OrderId
                                 WHERE OI.OrderId = @OrderId
                                   AND O.UserId = @UserId
                                 ORDER BY OI.OrderItemId";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@OrderId", SqlDbType.Int) { Value = orderId }
            };

            using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
            {
                while (reader.Read())
                {
                    items.Add(MapOrderItem(reader));
                }
            }

            return items;
        }

        /// <summary>
        /// Creates an order from all items in a cart using one atomic transaction.
        /// </summary>
        /// <param name="order">The order data to persist.</param>
        /// <param name="cartId">The cart identifier.</param>
        /// <returns>The identifier of the newly created order.</returns>
        public int CreateOrderFromCart(Order order, int cartId)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            if (order.UserId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(order.UserId), "User identifier must be greater than zero.");
            }

            if (cartId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cartId), "Cart identifier must be greater than zero.");
            }

            if (order.AddressId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(order.AddressId), "Address identifier must be greater than zero.");
            }

            if (order.ShippingFee < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(order.ShippingFee), "Shipping fee must be greater than or equal to zero.");
            }

            if (order.PromotionId.HasValue)
            {
                throw new InvalidOperationException("Promotion calculation is not implemented yet.");
            }

            try
            {
                return _dataProvider.ExecuteInTransaction<int>((connection, transaction) =>
                {
                    var cartUserId = GetCartUserId(connection, transaction, cartId);
                    if (!cartUserId.HasValue)
                    {
                        throw new InvalidOperationException("The cart does not exist.");
                    }

                    if (cartUserId.Value != order.UserId)
                    {
                        throw new InvalidOperationException("The cart does not belong to the specified user.");
                    }

                    ValidateAddressOwnership(
                        connection,
                        transaction,
                        order.AddressId,
                        order.UserId);

                    var cartItems = ReadCheckoutItems(connection, transaction, cartId);
                    if (cartItems.Count == 0)
                    {
                        throw new InvalidOperationException("The cart does not contain any items.");
                    }

                    var totalAmount = CalculateCartTotal(connection, transaction, cartId);
                    var checkoutTime = DateTime.UtcNow;
                    var orderId = InsertOrder(connection, transaction, order, totalAmount, checkoutTime);

                    foreach (var cartItem in cartItems)
                    {
                        InsertOrderItem(connection, transaction, orderId, cartItem);
                    }

                    foreach (var cartItem in cartItems)
                    {
                        DecreaseInventory(connection, transaction, cartItem.ProductId, cartItem.Quantity, checkoutTime);
                    }

                    DeleteCartItems(connection, transaction, cartId, cartItems.Count);
                    UpdateCartTimestamp(connection, transaction, cartId, checkoutTime);

                    return orderId;
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create the order from the cart.", ex);
            }
        }

        /// <summary>
        /// Updates the status of an order.
        /// </summary>
        /// <param name="orderId">The order identifier.</param>
        /// <param name="status">The new order status.</param>
        /// <returns>The number of rows affected.</returns>
        public int UpdateStatus(int orderId, string status)
        {
            if (orderId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(orderId), "Order identifier must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentException("Order status is required.", nameof(status));
            }

            var normalizedStatus = status.Trim();

            if (normalizedStatus.Length > OrderStatusMaxLength)
            {
                throw new ArgumentException("Order status exceeds the allowed maximum length.", nameof(status));
            }

            if (!AllowedOrderStatuses.Contains(normalizedStatus))
            {
                throw new ArgumentException("Order status is invalid.", nameof(status));
            }

            const string sql = @"UPDATE dbo.[Order]
                                 SET OrderStatus = @OrderStatus
                                 WHERE OrderId = @OrderId";
            var parameters = new[]
            {
                new SqlParameter("@OrderId", SqlDbType.Int) { Value = orderId },
                new SqlParameter("@OrderStatus", SqlDbType.NVarChar, OrderStatusMaxLength)
                {
                    Value = normalizedStatus
                }
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update the order status.", ex);
            }

            if (affectedRows == 0)
            {
                throw new InvalidOperationException("The order was not found.");
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The order status update did not affect exactly one record.");
            }

            return affectedRows;
        }

        private static int? GetCartUserId(SqlConnection connection, SqlTransaction transaction, int cartId)
        {
            const string sql = @"SELECT UserId
                                 FROM dbo.Cart WITH (UPDLOCK, HOLDLOCK)
                                 WHERE CartId = @CartId";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    var userIdOrdinal = reader.GetOrdinal("UserId");
                    return reader.IsDBNull(userIdOrdinal) ? (int?)null : reader.GetInt32(userIdOrdinal);
                }
            }
        }

        private static void ValidateAddressOwnership(
            SqlConnection connection,
            SqlTransaction transaction,
            int addressId,
            int userId)
        {
            const string sql = @"SELECT AddressId
                                 FROM dbo.Address WITH (UPDLOCK, HOLDLOCK)
                                 WHERE AddressId = @AddressId
                                   AND UserId = @UserId";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@AddressId", SqlDbType.Int) { Value = addressId });
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });

                var result = command.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "The selected address does not exist or does not belong to the specified user.");
                }
            }
        }

        private static List<OrderItem> ReadCheckoutItems(SqlConnection connection, SqlTransaction transaction, int cartId)
        {
            var items = new List<OrderItem>();
            const string sql = @"SELECT CI.CartItemId, CI.ProductId, CI.Quantity, CI.UnitPrice,
                                        P.ProductId AS ProductExists, P.IsActive,
                                        I.InventoryId AS InventoryExists, I.StockQuantity
                                 FROM dbo.CartItem AS CI WITH (UPDLOCK, HOLDLOCK)
                                 LEFT JOIN dbo.Product AS P WITH (UPDLOCK, HOLDLOCK) ON P.ProductId = CI.ProductId
                                 LEFT JOIN dbo.Inventory AS I WITH (UPDLOCK, HOLDLOCK) ON I.ProductId = CI.ProductId
                                 WHERE CI.CartId = @CartId
                                 ORDER BY CI.CartItemId";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });

                using (var reader = command.ExecuteReader())
                {
                    var cartItemIdOrdinal = reader.GetOrdinal("CartItemId");
                    var productIdOrdinal = reader.GetOrdinal("ProductId");
                    var quantityOrdinal = reader.GetOrdinal("Quantity");
                    var unitPriceOrdinal = reader.GetOrdinal("UnitPrice");
                    var productExistsOrdinal = reader.GetOrdinal("ProductExists");
                    var isActiveOrdinal = reader.GetOrdinal("IsActive");
                    var inventoryExistsOrdinal = reader.GetOrdinal("InventoryExists");
                    var stockQuantityOrdinal = reader.GetOrdinal("StockQuantity");

                    while (reader.Read())
                    {
                        var cartItemId = reader.IsDBNull(cartItemIdOrdinal) ? 0 : reader.GetInt32(cartItemIdOrdinal);
                        var productId = reader.IsDBNull(productIdOrdinal) ? 0 : reader.GetInt32(productIdOrdinal);

                        if (reader.IsDBNull(productExistsOrdinal))
                        {
                            throw new InvalidOperationException("A product in the cart does not exist.");
                        }

                        if (reader.IsDBNull(isActiveOrdinal) || !reader.GetBoolean(isActiveOrdinal))
                        {
                            throw new InvalidOperationException("A product in the cart is inactive.");
                        }

                        if (reader.IsDBNull(quantityOrdinal))
                        {
                            throw new InvalidOperationException("A cart item has an invalid quantity.");
                        }

                        var quantity = reader.GetInt32(quantityOrdinal);
                        if (quantity <= 0)
                        {
                            throw new InvalidOperationException("A cart item has an invalid quantity.");
                        }

                        if (reader.IsDBNull(unitPriceOrdinal))
                        {
                            throw new InvalidOperationException("A cart item has an invalid unit price.");
                        }

                        if (reader.IsDBNull(inventoryExistsOrdinal))
                        {
                            throw new InvalidOperationException("Inventory information is unavailable for a cart item.");
                        }

                        if (reader.IsDBNull(stockQuantityOrdinal))
                        {
                            throw new InvalidOperationException("Inventory information is unavailable for a cart item.");
                        }

                        var stockQuantity = reader.GetInt32(stockQuantityOrdinal);
                        if (stockQuantity < quantity)
                        {
                            throw new InvalidOperationException("Insufficient stock for a cart item.");
                        }

                        items.Add(new OrderItem
                        {
                            OrderItemId = cartItemId,
                            ProductId = productId,
                            Quantity = quantity,
                            UnitPrice = reader.GetDecimal(unitPriceOrdinal)
                        });
                    }
                }
            }

            return items;
        }

        private static decimal CalculateCartTotal(SqlConnection connection, SqlTransaction transaction, int cartId)
        {
            const string sql = @"SELECT CAST(ISNULL(SUM(CI.Quantity * CI.UnitPrice), 0) AS DECIMAL(12,2))
                                 FROM dbo.CartItem AS CI WITH (UPDLOCK, HOLDLOCK)
                                 WHERE CI.CartId = @CartId";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });

                var result = command.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    throw new InvalidOperationException("The cart total could not be calculated.");
                }

                return Convert.ToDecimal(result);
            }
        }

        private static int InsertOrder(
            SqlConnection connection,
            SqlTransaction transaction,
            Order order,
            decimal totalAmount,
            DateTime orderDate)
        {
            const string sql = @"INSERT INTO dbo.[Order]
                                     (UserId, AddressId, PromotionId, OrderDate, TotalAmount, ShippingFee, FinalAmount)
                                 VALUES
                                     (@UserId, @AddressId, @PromotionId, @OrderDate, @TotalAmount, @ShippingFee,
                                      CAST(@TotalAmount + @ShippingFee AS DECIMAL(12,2)));
                                 SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = order.UserId });
                command.Parameters.Add(new SqlParameter("@AddressId", SqlDbType.Int) { Value = order.AddressId });
                command.Parameters.Add(new SqlParameter("@PromotionId", SqlDbType.Int)
                {
                    Value = order.PromotionId.HasValue ? (object)order.PromotionId.Value : DBNull.Value
                });
                command.Parameters.Add(CreateDateTime2Parameter("@OrderDate", orderDate));
                command.Parameters.Add(CreateDecimalParameter("@TotalAmount", totalAmount));
                command.Parameters.Add(CreateDecimalParameter("@ShippingFee", order.ShippingFee));

                var result = command.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    throw new InvalidOperationException("The new order identifier was not returned.");
                }

                return Convert.ToInt32(result);
            }
        }

        private static void InsertOrderItem(
            SqlConnection connection,
            SqlTransaction transaction,
            int orderId,
            OrderItem cartItem)
        {
            const string sql = @"INSERT INTO dbo.OrderItem
                                     (OrderId, ProductId, Quantity, UnitPrice, LineTotal)
                                 VALUES
                                     (@OrderId, @ProductId, @Quantity, @UnitPrice,
                                      CAST(@Quantity * @UnitPrice AS DECIMAL(12,2)))";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@OrderId", SqlDbType.Int) { Value = orderId });
                command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = cartItem.ProductId });
                command.Parameters.Add(new SqlParameter("@Quantity", SqlDbType.Int) { Value = cartItem.Quantity });
                command.Parameters.Add(CreateDecimalParameter("@UnitPrice", cartItem.UnitPrice));

                if (command.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException("An order item could not be inserted.");
                }
            }
        }

        private static void DecreaseInventory(
            SqlConnection connection,
            SqlTransaction transaction,
            int productId,
            int quantity,
            DateTime lastUpdatedAt)
        {
            const string sql = @"UPDATE dbo.Inventory
                                 SET StockQuantity = StockQuantity - @Quantity,
                                     LastUpdatedAt = @LastUpdatedAt
                                 WHERE ProductId = @ProductId
                                   AND StockQuantity >= @Quantity";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });
                command.Parameters.Add(new SqlParameter("@Quantity", SqlDbType.Int) { Value = quantity });
                command.Parameters.Add(CreateDateTime2Parameter("@LastUpdatedAt", lastUpdatedAt));

                if (command.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException("Inventory could not be updated.");
                }
            }
        }

        private static void DeleteCartItems(
            SqlConnection connection,
            SqlTransaction transaction,
            int cartId,
            int expectedItemCount)
        {
            const string sql = @"DELETE FROM dbo.CartItem WHERE CartId = @CartId";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });

                if (command.ExecuteNonQuery() != expectedItemCount)
                {
                    throw new InvalidOperationException("Cart items could not be removed.");
                }
            }
        }

        private static void UpdateCartTimestamp(
            SqlConnection connection,
            SqlTransaction transaction,
            int cartId,
            DateTime updatedAt)
        {
            const string sql = @"UPDATE dbo.Cart
                                 SET UpdatedAt = @UpdatedAt
                                 WHERE CartId = @CartId";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add(new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });
                command.Parameters.Add(CreateDateTime2Parameter("@UpdatedAt", updatedAt));

                if (command.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException("The cart timestamp could not be updated.");
                }
            }
        }

        private static SqlParameter CreateDecimalParameter(string name, decimal value)
        {
            return new SqlParameter(name, SqlDbType.Decimal)
            {
                Precision = 12,
                Scale = 2,
                Value = value
            };
        }

        private static SqlParameter CreateDateTime2Parameter(
            string parameterName,
            DateTime value)
        {
            return new SqlParameter(parameterName, SqlDbType.DateTime2)
            {
                Scale = 7,
                Value = value
            };
        }

        private static Order MapOrder(SqlDataReader reader)
        {
            var orderIdOrdinal = reader.GetOrdinal("OrderId");
            var userIdOrdinal = reader.GetOrdinal("UserId");
            var addressIdOrdinal = reader.GetOrdinal("AddressId");
            var promotionIdOrdinal = reader.GetOrdinal("PromotionId");
            var orderDateOrdinal = reader.GetOrdinal("OrderDate");
            var orderStatusOrdinal = reader.GetOrdinal("OrderStatus");
            var totalAmountOrdinal = reader.GetOrdinal("TotalAmount");
            var shippingFeeOrdinal = reader.GetOrdinal("ShippingFee");
            var finalAmountOrdinal = reader.GetOrdinal("FinalAmount");

            return new Order
            {
                OrderId = reader.IsDBNull(orderIdOrdinal) ? 0 : reader.GetInt32(orderIdOrdinal),
                UserId = reader.IsDBNull(userIdOrdinal) ? 0 : reader.GetInt32(userIdOrdinal),
                AddressId = reader.IsDBNull(addressIdOrdinal) ? 0 : reader.GetInt32(addressIdOrdinal),
                PromotionId = reader.IsDBNull(promotionIdOrdinal) ? (int?)null : reader.GetInt32(promotionIdOrdinal),
                OrderDate = reader.IsDBNull(orderDateOrdinal) ? DateTime.MinValue : reader.GetDateTime(orderDateOrdinal),
                OrderStatus = reader.IsDBNull(orderStatusOrdinal) ? null : reader.GetString(orderStatusOrdinal),
                TotalAmount = reader.IsDBNull(totalAmountOrdinal) ? 0m : reader.GetDecimal(totalAmountOrdinal),
                ShippingFee = reader.IsDBNull(shippingFeeOrdinal) ? 0m : reader.GetDecimal(shippingFeeOrdinal),
                FinalAmount = reader.IsDBNull(finalAmountOrdinal) ? 0m : reader.GetDecimal(finalAmountOrdinal)
            };
        }

        private static OrderItem MapOrderItem(SqlDataReader reader)
        {
            var orderItemIdOrdinal = reader.GetOrdinal("OrderItemId");
            var orderIdOrdinal = reader.GetOrdinal("OrderId");
            var productIdOrdinal = reader.GetOrdinal("ProductId");
            var quantityOrdinal = reader.GetOrdinal("Quantity");
            var unitPriceOrdinal = reader.GetOrdinal("UnitPrice");
            var discountAmountOrdinal = reader.GetOrdinal("DiscountAmount");
            var lineTotalOrdinal = reader.GetOrdinal("LineTotal");

            return new OrderItem
            {
                OrderItemId = reader.IsDBNull(orderItemIdOrdinal) ? 0 : reader.GetInt32(orderItemIdOrdinal),
                OrderId = reader.IsDBNull(orderIdOrdinal) ? 0 : reader.GetInt32(orderIdOrdinal),
                ProductId = reader.IsDBNull(productIdOrdinal) ? 0 : reader.GetInt32(productIdOrdinal),
                Quantity = reader.IsDBNull(quantityOrdinal) ? 0 : reader.GetInt32(quantityOrdinal),
                UnitPrice = reader.IsDBNull(unitPriceOrdinal) ? 0m : reader.GetDecimal(unitPriceOrdinal),
                DiscountAmount = reader.IsDBNull(discountAmountOrdinal) ? 0m : reader.GetDecimal(discountAmountOrdinal),
                LineTotal = reader.IsDBNull(lineTotalOrdinal) ? 0m : reader.GetDecimal(lineTotalOrdinal)
            };
        }
    }
}
