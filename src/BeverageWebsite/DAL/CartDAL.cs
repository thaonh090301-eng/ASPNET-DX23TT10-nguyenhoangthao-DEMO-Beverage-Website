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
                throw new InvalidOperationException($"Failed to retrieve cart for user {userId}. Details: {ex.Message}", ex);
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
            const string sql = @"INSERT INTO dbo.Cart (UserId, CreatedAt, UpdatedAt) VALUES (@UserId, @CreatedAt, @UpdatedAt)";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = DateTime.UtcNow },
                new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = DateTime.UtcNow }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create cart for user {userId}. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves all items in the specified cart.
        /// </summary>
        /// <param name="cartId">The cart identifier.</param>
        /// <returns>A list of <see cref="CartItem"/> objects.</returns>
        public List<CartItem> GetCartItems(int cartId)
        {
            var items = new List<CartItem>();
            const string sql = @"SELECT CI.CartItemId, CI.CartId, CI.ProductId, CI.Quantity, CI.UnitPrice
                                 FROM dbo.CartItem CI
                                 WHERE CI.CartId = @CartId
                                 ORDER BY CI.CartItemId";
            var parameters = new[]
            {
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
                throw new InvalidOperationException($"Failed to retrieve cart items for cart {cartId}. Details: {ex.Message}", ex);
            }

            return items;
        }

        /// <summary>
        /// Adds a product to the cart with the specified quantity.
        /// </summary>
        /// <param name="cartId">The cart identifier.</param>
        /// <param name="productId">The product identifier.</param>
        /// <param name="quantity">The quantity to add.</param>
        /// <returns>The number of rows affected.</returns>
        public int AddItem(int cartId, int productId, int quantity)
        {
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
                            throw new InvalidOperationException("The product does not exist or is inactive.");
                        }

                        unitPrice = Convert.ToDecimal(productPrice);
                    }

                    const string cartSql = @"SELECT C.CartId
                                             FROM dbo.Cart C WITH (UPDLOCK, HOLDLOCK)
                                             WHERE C.CartId = @CartId";

                    using (var cartCommand = new SqlCommand(cartSql, connection, transaction))
                    {
                        cartCommand.Parameters.Add(
                            new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });

                        if (cartCommand.ExecuteScalar() == null)
                        {
                            throw new InvalidOperationException("The cart does not exist.");
                        }
                    }

                    const string cartItemSql = @"SELECT CI.CartItemId
                                                 FROM dbo.CartItem CI WITH (UPDLOCK, HOLDLOCK)
                                                 WHERE CI.CartId = @CartId
                                                   AND CI.ProductId = @ProductId";
                    object cartItemId;

                    using (var cartItemCommand = new SqlCommand(cartItemSql, connection, transaction))
                    {
                        cartItemCommand.Parameters.Add(
                            new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId });
                        cartItemCommand.Parameters.Add(
                            new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });
                        cartItemId = cartItemCommand.ExecuteScalar();
                    }

                    if (cartItemId != null && cartItemId != DBNull.Value)
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

                        return affectedRows;
                    }
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to add item to cart.", ex);
            }
        }

        /// <summary>
        /// Updates the quantity of a cart item.
        /// </summary>
        /// <param name="cartItemId">The cart item identifier.</param>
        /// <param name="quantity">The new quantity.</param>
        /// <returns>The number of rows affected.</returns>
        public int UpdateQuantity(int cartItemId, int quantity)
        {
            if (cartItemId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cartItemId), "Cart item identifier must be greater than zero.");
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
            }

            const string sql = @"UPDATE dbo.CartItem SET Quantity = @Quantity WHERE CartItemId = @CartItemId";
            var parameters = new[]
            {
                new SqlParameter("@CartItemId", SqlDbType.Int) { Value = cartItemId },
                new SqlParameter("@Quantity", SqlDbType.Int) { Value = quantity }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update quantity for cart item {cartItemId}. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Removes a cart item from the cart.
        /// </summary>
        /// <param name="cartItemId">The cart item identifier.</param>
        /// <returns>The number of rows affected.</returns>
        public int RemoveItem(int cartItemId)
        {
            const string sql = @"DELETE FROM dbo.CartItem WHERE CartItemId = @CartItemId";
            var parameters = new[]
            {
                new SqlParameter("@CartItemId", SqlDbType.Int) { Value = cartItemId }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to remove cart item {cartItemId}. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Clears all items from the specified cart.
        /// </summary>
        /// <param name="cartId">The cart identifier.</param>
        /// <returns>The number of rows affected.</returns>
        public int ClearCart(int cartId)
        {
            const string sql = @"DELETE FROM dbo.CartItem WHERE CartId = @CartId";
            var parameters = new[]
            {
                new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to clear cart {cartId}. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves the total amount of the cart.
        /// </summary>
        /// <param name="cartId">The cart identifier.</param>
        /// <returns>The total amount of the cart.</returns>
        public decimal GetCartTotal(int cartId)
        {
            const string sql = @"SELECT ISNULL(SUM(CI.Quantity * CI.UnitPrice), 0) FROM dbo.CartItem CI WHERE CI.CartId = @CartId";
            var parameters = new[]
            {
                new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId }
            };

            try
            {
                var result = _dataProvider.ExecuteScalar(sql, CommandType.Text, parameters);
                return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to calculate cart total for cart {cartId}. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves the total number of items in the cart.
        /// </summary>
        /// <param name="cartId">The cart identifier.</param>
        /// <returns>The total number of items in the cart.</returns>
        public int GetTotalItems(int cartId)
        {
            const string sql = @"SELECT ISNULL(SUM(CI.Quantity), 0) FROM dbo.CartItem CI WHERE CI.CartId = @CartId";
            var parameters = new[]
            {
                new SqlParameter("@CartId", SqlDbType.Int) { Value = cartId }
            };

            try
            {
                var result = _dataProvider.ExecuteScalar(sql, CommandType.Text, parameters);
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to calculate total items for cart {cartId}. Details: {ex.Message}", ex);
            }
        }

        private static Cart MapCart(SqlDataReader reader)
        {
            return new Cart
            {
                CartId = reader.IsDBNull(reader.GetOrdinal("CartId")) ? 0 : reader.GetInt32(reader.GetOrdinal("CartId")),
                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserId")),
                CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        private static CartItem MapCartItem(SqlDataReader reader)
        {
            return new CartItem
            {
                CartItemId = reader.IsDBNull(reader.GetOrdinal("CartItemId")) ? 0 : reader.GetInt32(reader.GetOrdinal("CartItemId")),
                CartId = reader.IsDBNull(reader.GetOrdinal("CartId")) ? 0 : reader.GetInt32(reader.GetOrdinal("CartId")),
                ProductId = reader.IsDBNull(reader.GetOrdinal("ProductId")) ? 0 : reader.GetInt32(reader.GetOrdinal("ProductId")),
                Quantity = reader.IsDBNull(reader.GetOrdinal("Quantity")) ? 0 : reader.GetInt32(reader.GetOrdinal("Quantity")),
                UnitPrice = reader.IsDBNull(reader.GetOrdinal("UnitPrice")) ? 0m : reader.GetDecimal(reader.GetOrdinal("UnitPrice"))
            };
        }
    }
}
