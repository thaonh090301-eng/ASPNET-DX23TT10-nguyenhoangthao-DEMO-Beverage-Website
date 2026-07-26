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
                throw new InvalidOperationException($"Failed to retrieve orders. Details: {ex.Message}", ex);
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
                throw new InvalidOperationException($"Failed to retrieve order by id {orderId}. Details: {ex.Message}", ex);
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
                throw new InvalidOperationException($"Failed to retrieve orders for user {userId}. Details: {ex.Message}", ex);
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
                throw new InvalidOperationException($"Failed to retrieve order items for order {orderId}. Details: {ex.Message}", ex);
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
                        throw new InvalidOperationException($"Cart {cartId} does not exist.");
                    }

                    if (cartUserId.Value != order.UserId)
                    {
                        throw new InvalidOperationException($"Cart {cartId} does not belong to the specified user.");
                    }

                    var cartItems = ReadCheckoutItems(connection, transaction, cartId);
                    if (cartItems.Count == 0)
                    {
                        throw new InvalidOperationException($"Cart {cartId} does not contain any items.");
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
                throw new InvalidOperationException($"Failed to create order from cart {cartId}. Details: {ex.Message}", ex);
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

            const string sql = @"UPDATE dbo.[Order]
                                 SET OrderStatus = @OrderStatus
                                 WHERE OrderId = @OrderId";
            var parameters = new[]
            {
                new SqlParameter("@OrderId", SqlDbType.Int) { Value = orderId },
                new SqlParameter("@OrderStatus", SqlDbType.NVarChar, 50) { Value = status }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update status for order {orderId}. Details: {ex.Message}", ex);
            }
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
                            throw new InvalidOperationException($"Product {productId} does not exist.");
                        }

                        if (reader.IsDBNull(isActiveOrdinal) || !reader.GetBoolean(isActiveOrdinal))
                        {
                            throw new InvalidOperationException($"Product {productId} is inactive.");
                        }

                        if (reader.IsDBNull(quantityOrdinal))
                        {
                            throw new InvalidOperationException($"Cart item {cartItemId} has no quantity.");
                        }

                        var quantity = reader.GetInt32(quantityOrdinal);
                        if (quantity <= 0)
                        {
                            throw new InvalidOperationException($"Cart item {cartItemId} has an invalid quantity.");
                        }

                        if (reader.IsDBNull(unitPriceOrdinal))
                        {
                            throw new InvalidOperationException($"Cart item {cartItemId} has no unit price.");
                        }

                        if (reader.IsDBNull(inventoryExistsOrdinal))
                        {
                            throw new InvalidOperationException($"Inventory for product {productId} does not exist.");
                        }

                        if (reader.IsDBNull(stockQuantityOrdinal))
                        {
                            throw new InvalidOperationException($"Inventory for product {productId} has no stock quantity.");
                        }

                        var stockQuantity = reader.GetInt32(stockQuantityOrdinal);
                        if (stockQuantity < quantity)
                        {
                            throw new InvalidOperationException($"Insufficient stock for product {productId}.");
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
                    throw new InvalidOperationException($"Unable to calculate the total for cart {cartId}.");
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
                                     (UserId, AddressId, PromotionId, OrderDate, OrderStatus, TotalAmount, ShippingFee, FinalAmount)
                                 VALUES
                                     (@UserId, @AddressId, @PromotionId, @OrderDate, @OrderStatus, @TotalAmount, @ShippingFee,
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
                command.Parameters.Add(new SqlParameter("@OrderDate", SqlDbType.DateTime2) { Value = orderDate });
                command.Parameters.Add(new SqlParameter("@OrderStatus", SqlDbType.NVarChar, 50)
                {
                    Value = (object)order.OrderStatus ?? DBNull.Value
                });
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
                    throw new InvalidOperationException($"Order item insert failed for order {orderId}.");
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
                command.Parameters.Add(new SqlParameter("@LastUpdatedAt", SqlDbType.DateTime2) { Value = lastUpdatedAt });

                if (command.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException($"Inventory update failed for product {productId}.");
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
                    throw new InvalidOperationException($"Cart item deletion count did not match the expected count for cart {cartId}.");
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
                command.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = updatedAt });

                if (command.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException($"Cart timestamp update failed for cart {cartId}.");
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

        private static Order MapOrder(SqlDataReader reader)
        {
            return new Order
            {
                OrderId = reader.IsDBNull(reader.GetOrdinal("OrderId")) ? 0 : reader.GetInt32(reader.GetOrdinal("OrderId")),
                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserId")),
                AddressId = reader.IsDBNull(reader.GetOrdinal("AddressId")) ? 0 : reader.GetInt32(reader.GetOrdinal("AddressId")),
                PromotionId = reader.IsDBNull(reader.GetOrdinal("PromotionId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("PromotionId")),
                OrderDate = reader.IsDBNull(reader.GetOrdinal("OrderDate")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("OrderDate")),
                OrderStatus = reader.IsDBNull(reader.GetOrdinal("OrderStatus")) ? null : reader.GetString(reader.GetOrdinal("OrderStatus")),
                TotalAmount = reader.IsDBNull(reader.GetOrdinal("TotalAmount")) ? 0m : reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                ShippingFee = reader.IsDBNull(reader.GetOrdinal("ShippingFee")) ? 0m : reader.GetDecimal(reader.GetOrdinal("ShippingFee")),
                FinalAmount = reader.IsDBNull(reader.GetOrdinal("FinalAmount")) ? 0m : reader.GetDecimal(reader.GetOrdinal("FinalAmount"))
            };
        }

        private static OrderItem MapOrderItem(SqlDataReader reader)
        {
            return new OrderItem
            {
                OrderItemId = reader.IsDBNull(reader.GetOrdinal("OrderItemId")) ? 0 : reader.GetInt32(reader.GetOrdinal("OrderItemId")),
                OrderId = reader.IsDBNull(reader.GetOrdinal("OrderId")) ? 0 : reader.GetInt32(reader.GetOrdinal("OrderId")),
                ProductId = reader.IsDBNull(reader.GetOrdinal("ProductId")) ? 0 : reader.GetInt32(reader.GetOrdinal("ProductId")),
                Quantity = reader.IsDBNull(reader.GetOrdinal("Quantity")) ? 0 : reader.GetInt32(reader.GetOrdinal("Quantity")),
                UnitPrice = reader.IsDBNull(reader.GetOrdinal("UnitPrice")) ? 0m : reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                DiscountAmount = reader.IsDBNull(reader.GetOrdinal("DiscountAmount")) ? 0m : reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),
                LineTotal = reader.IsDBNull(reader.GetOrdinal("LineTotal")) ? 0m : reader.GetDecimal(reader.GetOrdinal("LineTotal"))
            };
        }
    }
}
