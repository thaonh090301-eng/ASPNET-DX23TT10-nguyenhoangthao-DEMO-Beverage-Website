using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BeverageWebsite.Models;

namespace BeverageWebsite.DAL
{
    /// <summary>
    /// Data access layer for cart operations using ADO.NET and the shared DataProvider.
    /// </summary>
    public class CartDAL
    {
        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CartDAL"/> class.
        /// </summary>
        public CartDAL()
        {
            _dataProvider = new DataProvider();
        }

        /// <summary>
        /// Retrieves the cart for the specified user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A <see cref="Cart"/> object if found; otherwise, null.</returns>
        public Cart GetCartByUserId(int userId)
        {
            ValidateIdentifier(userId, nameof(userId));

            const string sql = @"SELECT CartId, UserId, CreatedAt, UpdatedAt FROM dbo.Cart WHERE UserId = @UserId";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapCart(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve the cart.", ex);
            }

            return null;
        }

        /// <summary>
        /// Creates a new cart for the specified user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The number of rows affected.</returns>
        public int CreateCart(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User identifier must be greater than zero.");
            }

            const string sql = @"INSERT INTO dbo.Cart (UserId)
                                 VALUES (@UserId)";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create the cart.", ex);
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The cart insert did not affect exactly one record.");
            }

            return affectedRows;
        }

        /// <summary>
        /// Retrieves all items in the specified user-owned cart.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the cart.</param>
        /// <param name="cartId">The cart identifier.</param>
        /// <returns>A list of <see cref="CartItem"/> objects.</returns>
        public List<CartItem> GetCartItems(int userId, int cartId)
        {
            ValidateIdentifier(userId, nameof(userId));
            ValidateIdentifier(cartId, nameof(cartId));

            var items = new List<CartItem>();
            const string sql = @"SELECT CI.CartItemId, CI.CartId, CI.ProductId, CI.Quantity, CI.UnitPrice
                                 FROM dbo.CartItem CI
                                 INNER JOIN dbo.Cart C ON C.CartId = CI.CartId
                                 WHERE CI.CartId = @CartId
                                   AND C.UserId = @UserId
                                 ORDER BY CI.CartItemId";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    while (reader.Read())
                    {
                        items.Add(MapCartItem(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve cart items.", ex);
            }

            return items;
        }

        /// <summary>
        /// Adds a product to the cart with the specified quantity.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cartId">The cart identifier.</param>
        /// <param name="productId">The product identifier.</param>
        /// <param name="quantity">The quantity to add.</param>
        /// <returns>The number of rows affected.</returns>
        public int AddItem(int userId, int cartId, int productId, int quantity)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User identifier must be greater than zero.");
            }

            if (cartId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cartId), "Cart identifier must be greater than zero.");
            }

            if (productId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(productId), "Product identifier must be greater than zero.");
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
            }

            try
            {
                return _dataProvider.ExecuteInTransaction((connection, transaction) =>
                {
                    ValidateCartOwnership(connection, transaction, cartId, userId);

                    const string productSql = @"SELECT P.Price
                                                FROM dbo.Product P WITH (UPDLOCK, HOLDLOCK)
                                                WHERE P.ProductId = @ProductId
                                                  AND P.IsActive = 1";
                    decimal unitPrice;

                    using (var productCommand = new SqlCommand(productSql, connection, transaction))
                    {
                        productCommand.Parameters.Add(
                            new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });

                        var productPrice = productCommand.ExecuteScalar();

                        if (productPrice == null || productPrice == DBNull.Value)
                        {
                            throw new CartValidationException(
                                "The product does not exist or is inactive.");
                        }

                        unitPrice = Convert.ToDecimal(productPrice);
                    }

                    const string cartItemSql = @"SELECT CI.Quantity
                                                 FROM dbo.CartItem CI WITH (UPDLOCK, HOLDLOCK)
                                                 WHERE CI.CartId = @CartId
                                                   AND CI.ProductId = @ProductId";
                    object existingQuantityValue;

                    using (var cartItemCommand = new SqlCommand(cartItemSql, connection, transaction))
                    {
                        cartItemCommand.Parameters.Add(
                            new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });
                        cartItemCommand.Parameters.Add(
                            new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });
                        existingQuantityValue = cartItemCommand.ExecuteScalar();
                    }

                    if (existingQuantityValue == DBNull.Value)
                    {
                        throw new CartValidationException(
                            "The cart item quantity is unavailable.");
                    }

                    var hasExistingItem = existingQuantityValue != null;
                    var existingQuantity = hasExistingItem
                        ? Convert.ToInt32(existingQuantityValue)
                        : 0;

                    if (hasExistingItem && existingQuantity <= 0)
                    {
                        throw new CartValidationException(
                            "The cart item quantity is unavailable.");
                    }

                    var finalQuantity = (long)existingQuantity + quantity;
                    var stockQuantity = GetLockedStockQuantity(
                        connection,
                        transaction,
                        productId);

                    ValidateStockAvailability(stockQuantity, finalQuantity);

                    if (hasExistingItem)
                    {
                        const string updateSql = @"UPDATE dbo.CartItem
                                                   SET Quantity = Quantity + @Quantity,
                                                       UnitPrice = @UnitPrice
                                                   WHERE CartId = @CartId
                                                     AND ProductId = @ProductId";

                        using (var updateCommand = new SqlCommand(updateSql, connection, transaction))
                        {
                            updateCommand.Parameters.Add(
                                new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });
                            updateCommand.Parameters.Add(
                                new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });
                            updateCommand.Parameters.Add(
                                new SqlParameter("@Quantity", SqlDbType.Int) { Value = quantity });
                            updateCommand.Parameters.Add(
                                new SqlParameter("@UnitPrice", SqlDbType.Decimal)
                                {
                                    Precision = 12,
                                    Scale = 2,
                                    Value = unitPrice
                                });

                            var affectedRows = updateCommand.ExecuteNonQuery();

                            if (affectedRows != 1)
                            {
                                throw new InvalidOperationException("The cart item could not be updated.");
                            }

                            UpdateCartTimestamp(
                                connection,
                                transaction,
                                userId,
                                cartId);

                            return affectedRows;
                        }
                    }

                    const string insertSql = @"INSERT INTO dbo.CartItem
                                               (CartId, ProductId, Quantity, UnitPrice)
                                               VALUES
                                               (@CartId, @ProductId, @Quantity, @UnitPrice)";

                    using (var insertCommand = new SqlCommand(insertSql, connection, transaction))
                    {
                        insertCommand.Parameters.Add(
                            new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });
                        insertCommand.Parameters.Add(
                            new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });
                        insertCommand.Parameters.Add(
                            new SqlParameter("@Quantity", SqlDbType.Int) { Value = quantity });
                        insertCommand.Parameters.Add(
                            new SqlParameter("@UnitPrice", SqlDbType.Decimal)
                            {
                                Precision = 12,
                                Scale = 2,
                                Value = unitPrice
                            });

                        var affectedRows = insertCommand.ExecuteNonQuery();

                        if (affectedRows != 1)
                        {
                            throw new InvalidOperationException("The cart item could not be inserted.");
                        }

                        UpdateCartTimestamp(
                            connection,
                            transaction,
                            userId,
                            cartId);

                        return affectedRows;
                    }
                });
            }
            catch (Exception ex)
            {
                var validationException = FindCartValidationException(ex);

                if (validationException != null)
                {
                    throw new InvalidOperationException(validationException.Message);
                }

                throw new InvalidOperationException("Failed to add item to cart.", ex);
            }
        }

        /// <summary>
        /// Updates the quantity of a cart item.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cartId">The cart identifier.</param>
        /// <param name="cartItemId">The cart item identifier.</param>
        /// <param name="quantity">The new quantity.</param>
        /// <returns>The number of rows affected.</returns>
        public int UpdateQuantity(int userId, int cartId, int cartItemId, int quantity)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User identifier must be greater than zero.");
            }

            if (cartId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cartId), "Cart identifier must be greater than zero.");
            }

            if (cartItemId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cartItemId), "Cart item identifier must be greater than zero.");
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
            }

            try
            {
                return _dataProvider.ExecuteInTransaction((connection, transaction) =>
                {
                    ValidateCartOwnership(connection, transaction, cartId, userId);

                    const string cartItemSql = @"SELECT CI.ProductId
                                                 FROM dbo.CartItem CI WITH (UPDLOCK, HOLDLOCK)
                                                 WHERE CI.CartItemId = @CartItemId
                                                   AND CI.CartId = @CartId";
                    object productIdValue;

                    using (var cartItemCommand = new SqlCommand(cartItemSql, connection, transaction))
                    {
                        cartItemCommand.Parameters.Add(
                            new SqlParameter("@CartItemId", SqlDbType.Int) { Value = cartItemId });
                        cartItemCommand.Parameters.Add(
                            new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });
                        productIdValue = cartItemCommand.ExecuteScalar();
                    }

                    if (productIdValue == null || productIdValue == DBNull.Value)
                    {
                        throw new CartValidationException(
                            "The cart item does not exist in the specified cart.");
                    }

                    var productId = Convert.ToInt32(productIdValue);
                    var stockQuantity = GetLockedStockQuantity(
                        connection,
                        transaction,
                        productId);

                    ValidateStockAvailability(stockQuantity, quantity);

                    const string sql = @"UPDATE dbo.CartItem
                                         SET Quantity = @Quantity
                                         WHERE CartItemId = @CartItemId
                                           AND CartId = @CartId";

                    using (var command = new SqlCommand(sql, connection, transaction))
                    {
                        command.Parameters.Add(
                            new SqlParameter("@CartItemId", SqlDbType.Int) { Value = cartItemId });
                        command.Parameters.Add(
                            new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });
                        command.Parameters.Add(
                            new SqlParameter("@Quantity", SqlDbType.Int) { Value = quantity });

                        var affectedRows = command.ExecuteNonQuery();

                        if (affectedRows == 0)
                        {
                            throw new CartValidationException(
                                "The cart item does not exist in the specified cart.");
                        }

                        if (affectedRows != 1)
                        {
                            throw new InvalidOperationException("The cart item could not be updated.");
                        }

                        UpdateCartTimestamp(
                            connection,
                            transaction,
                            userId,
                            cartId);

                        return affectedRows;
                    }
                });
            }
            catch (Exception ex)
            {
                var validationException = FindCartValidationException(ex);

                if (validationException != null)
                {
                    throw new InvalidOperationException(validationException.Message);
                }

                throw new InvalidOperationException("Failed to update the cart item quantity.", ex);
            }
        }

        /// <summary>
        /// Removes a cart item from the cart.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cartId">The cart identifier.</param>
        /// <param name="cartItemId">The cart item identifier.</param>
        /// <returns>The number of rows affected.</returns>
        public int RemoveItem(int userId, int cartId, int cartItemId)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User identifier must be greater than zero.");
            }

            if (cartId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cartId), "Cart identifier must be greater than zero.");
            }

            if (cartItemId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cartItemId), "Cart item identifier must be greater than zero.");
            }

            try
            {
                return _dataProvider.ExecuteInTransaction((connection, transaction) =>
                {
                    ValidateCartOwnership(connection, transaction, cartId, userId);

                    const string sql = @"DELETE FROM dbo.CartItem
                                         WHERE CartItemId = @CartItemId
                                           AND CartId = @CartId";

                    using (var command = new SqlCommand(sql, connection, transaction))
                    {
                        command.Parameters.Add(
                            new SqlParameter("@CartItemId", SqlDbType.Int) { Value = cartItemId });
                        command.Parameters.Add(
                            new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });

                        var affectedRows = command.ExecuteNonQuery();

                        if (affectedRows == 0)
                        {
                            throw new InvalidOperationException("The cart item does not exist in the specified cart.");
                        }

                        if (affectedRows != 1)
                        {
                            throw new InvalidOperationException("The cart item could not be removed.");
                        }

                        UpdateCartTimestamp(
                            connection,
                            transaction,
                            userId,
                            cartId);

                        return affectedRows;
                    }
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to remove the cart item.", ex);
            }
        }

        /// <summary>
        /// Clears all items from the specified cart.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cartId">The cart identifier.</param>
        /// <returns>The number of rows affected.</returns>
        public int ClearCart(int userId, int cartId)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "User identifier must be greater than zero.");
            }

            if (cartId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cartId), "Cart identifier must be greater than zero.");
            }

            try
            {
                return _dataProvider.ExecuteInTransaction((connection, transaction) =>
                {
                    ValidateCartOwnership(connection, transaction, cartId, userId);

                    const string sql = @"DELETE FROM dbo.CartItem
                                         WHERE CartId = @CartId";

                    using (var command = new SqlCommand(sql, connection, transaction))
                    {
                        command.Parameters.Add(
                            new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });

                        var affectedRows = command.ExecuteNonQuery();

                        if (affectedRows > 0)
                        {
                            UpdateCartTimestamp(
                                connection,
                                transaction,
                                userId,
                                cartId);
                        }

                        return affectedRows;
                    }
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to clear the cart.", ex);
            }
        }

        /// <summary>
        /// Retrieves the total amount of the specified user-owned cart.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the cart.</param>
        /// <param name="cartId">The cart identifier.</param>
        /// <returns>The total amount of the cart.</returns>
        public decimal GetCartTotal(int userId, int cartId)
        {
            ValidateIdentifier(userId, nameof(userId));
            ValidateIdentifier(cartId, nameof(cartId));

            const string sql = @"SELECT CAST(
                                     ISNULL(SUM(CI.Quantity * CI.UnitPrice), 0)
                                     AS DECIMAL(12,2))
                                 FROM dbo.CartItem AS CI
                                 INNER JOIN dbo.Cart AS C ON C.CartId = CI.CartId
                                 WHERE CI.CartId = @CartId
                                   AND C.UserId = @UserId";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId }
            };

            try
            {
                var result = _dataProvider.ExecuteScalar(sql, CommandType.Text, parameters);
                return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to calculate the cart total.", ex);
            }
        }

        /// <summary>
        /// Retrieves the total number of items in the specified user-owned cart.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the cart.</param>
        /// <param name="cartId">The cart identifier.</param>
        /// <returns>The total number of items in the cart.</returns>
        public int GetTotalItems(int userId, int cartId)
        {
            ValidateIdentifier(userId, nameof(userId));
            ValidateIdentifier(cartId, nameof(cartId));

            const string sql = @"SELECT ISNULL(SUM(CI.Quantity), 0)
                                 FROM dbo.CartItem CI
                                 INNER JOIN dbo.Cart C ON C.CartId = CI.CartId
                                 WHERE CI.CartId = @CartId
                                   AND C.UserId = @UserId";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId }
            };

            try
            {
                var result = _dataProvider.ExecuteScalar(sql, CommandType.Text, parameters);
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to calculate the cart item count.", ex);
            }
        }

        private static void ValidateIdentifier(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Identifier must be greater than zero.");
            }
        }

        private static void UpdateCartTimestamp(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int cartId)
        {
            const string sql = @"UPDATE dbo.Cart
                                 SET UpdatedAt = SYSUTCDATETIME()
                                 WHERE CartId = @CartId
                                   AND UserId = @UserId";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add(
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                command.Parameters.Add(
                    new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });

                if (command.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException(
                        "The cart timestamp could not be updated.");
                }
            }
        }

        private static int GetLockedStockQuantity(
            SqlConnection connection,
            SqlTransaction transaction,
            int productId)
        {
            const string sql = @"SELECT I.StockQuantity
                                 FROM dbo.Inventory I WITH (UPDLOCK, HOLDLOCK)
                                 WHERE I.ProductId = @ProductId";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add(
                    new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });

                var stockQuantityValue = command.ExecuteScalar();

                if (stockQuantityValue == null || stockQuantityValue == DBNull.Value)
                {
                    throw new CartValidationException(
                        "Không thể xác định tồn kho hiện có.");
                }

                var stockQuantity = Convert.ToInt32(stockQuantityValue);

                if (stockQuantity < 0)
                {
                    throw new CartValidationException(
                        "Không thể xác định tồn kho hiện có.");
                }

                return stockQuantity;
            }
        }

        private static void ValidateStockAvailability(
            int stockQuantity,
            long finalQuantity)
        {
            if (finalQuantity > stockQuantity)
            {
                throw new CartValidationException(
                    "Số lượng yêu cầu vượt quá tồn kho hiện có.");
            }
        }

        private static CartValidationException FindCartValidationException(
            Exception exception)
        {
            while (exception != null)
            {
                var validationException = exception as CartValidationException;

                if (validationException != null)
                {
                    return validationException;
                }

                exception = exception.InnerException;
            }

            return null;
        }

        private static void ValidateCartOwnership(
            SqlConnection connection,
            SqlTransaction transaction,
            int cartId,
            int userId)
        {
            const string sql = @"SELECT C.CartId
                                 FROM dbo.Cart C WITH (UPDLOCK, HOLDLOCK)
                                 WHERE C.CartId = @CartId
                                   AND C.UserId = @UserId";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add(
                    new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });
                command.Parameters.Add(
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });

                if (command.ExecuteScalar() == null)
                {
                    throw new CartValidationException(
                        "The cart does not exist or is not available to the specified user.");
                }
            }
        }

        private sealed class CartValidationException : Exception
        {
            public CartValidationException(string message)
                : base(message)
            {
            }
        }

        private static Cart MapCart(SqlDataReader reader)
        {
            var cartIdOrdinal = reader.GetOrdinal("CartId");
            var userIdOrdinal = reader.GetOrdinal("UserId");
            var createdAtOrdinal = reader.GetOrdinal("CreatedAt");
            var updatedAtOrdinal = reader.GetOrdinal("UpdatedAt");

            return new Cart
            {
                CartId = reader.IsDBNull(cartIdOrdinal) ? 0 : reader.GetInt32(cartIdOrdinal),
                UserId = reader.IsDBNull(userIdOrdinal) ? 0 : reader.GetInt32(userIdOrdinal),
                CreatedAt = reader.IsDBNull(createdAtOrdinal) ? DateTime.MinValue : reader.GetDateTime(createdAtOrdinal),
                UpdatedAt = reader.IsDBNull(updatedAtOrdinal) ? DateTime.MinValue : reader.GetDateTime(updatedAtOrdinal)
            };
        }

        private static CartItem MapCartItem(SqlDataReader reader)
        {
            var cartItemIdOrdinal = reader.GetOrdinal("CartItemId");
            var cartIdOrdinal = reader.GetOrdinal("CartId");
            var productIdOrdinal = reader.GetOrdinal("ProductId");
            var quantityOrdinal = reader.GetOrdinal("Quantity");
            var unitPriceOrdinal = reader.GetOrdinal("UnitPrice");

            return new CartItem
            {
                CartItemId = reader.IsDBNull(cartItemIdOrdinal) ? 0 : reader.GetInt32(cartItemIdOrdinal),
                CartId = reader.IsDBNull(cartIdOrdinal) ? 0 : reader.GetInt32(cartIdOrdinal),
                ProductId = reader.IsDBNull(productIdOrdinal) ? 0 : reader.GetInt32(productIdOrdinal),
                Quantity = reader.IsDBNull(quantityOrdinal) ? 0 : reader.GetInt32(quantityOrdinal),
                UnitPrice = reader.IsDBNull(unitPriceOrdinal) ? 0m : reader.GetDecimal(unitPriceOrdinal)
            };
        }
    }
}
