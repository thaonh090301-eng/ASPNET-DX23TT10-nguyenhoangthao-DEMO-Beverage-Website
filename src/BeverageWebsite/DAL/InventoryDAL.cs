using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BeverageWebsite.Models;

namespace BeverageWebsite.DAL
{
    /// <summary>
    /// Data access layer for Inventory operations using ADO.NET and the shared DataProvider.
    /// </summary>
    public class InventoryDAL
    {
        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryDAL"/> class.
        /// </summary>
        public InventoryDAL()
        {
            _dataProvider = new DataProvider();
        }

        /// <summary>
        /// Retrieves all inventory records.
        /// </summary>
        /// <returns>A list of <see cref="Inventory"/> objects.</returns>
        public List<Inventory> GetAll()
        {
            var inventories = new List<Inventory>();
            const string sql = @"SELECT InventoryId, ProductId, StockQuantity, ReorderLevel, LastUpdatedAt FROM dbo.Inventory ORDER BY ProductId";

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql))
                {
                    while (reader.Read())
                    {
                        inventories.Add(MapInventory(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve inventories. Details: {ex.Message}", ex);
            }

            return inventories;
        }

        /// <summary>
        /// Retrieves an inventory record by its identifier.
        /// </summary>
        /// <param name="inventoryId">The inventory identifier.</param>
        /// <returns>An <see cref="Inventory"/> object if found; otherwise, null.</returns>
        public Inventory GetById(int inventoryId)
        {
            const string sql = @"SELECT InventoryId, ProductId, StockQuantity, ReorderLevel, LastUpdatedAt FROM dbo.Inventory WHERE InventoryId = @InventoryId";
            var parameters = new[]
            {
                new SqlParameter("@InventoryId", SqlDbType.Int) { Value = inventoryId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapInventory(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve inventory by id {inventoryId}. Details: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Retrieves an inventory record by product identifier.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <returns>An <see cref="Inventory"/> object if found; otherwise, null.</returns>
        public Inventory GetByProductId(int productId)
        {
            const string sql = @"SELECT InventoryId, ProductId, StockQuantity, ReorderLevel, LastUpdatedAt FROM dbo.Inventory WHERE ProductId = @ProductId";
            var parameters = new[]
            {
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapInventory(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve inventory by product id {productId}. Details: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Inserts a new inventory record into the database.
        /// </summary>
        /// <param name="inventory">The inventory to insert.</param>
        /// <returns>The number of rows affected.</returns>
        public int Insert(Inventory inventory)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            const string sql = @"INSERT INTO dbo.Inventory (ProductId, StockQuantity, ReorderLevel, LastUpdatedAt) VALUES (@ProductId, @StockQuantity, @ReorderLevel, @LastUpdatedAt)";
            var parameters = new[]
            {
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = inventory.ProductId },
                new SqlParameter("@StockQuantity", SqlDbType.Int) { Value = inventory.StockQuantity },
                new SqlParameter("@ReorderLevel", SqlDbType.Int) { Value = inventory.ReorderLevel },
                new SqlParameter("@LastUpdatedAt", SqlDbType.DateTime2) { Value = inventory.LastUpdatedAt }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to insert inventory. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing inventory record in the database.
        /// </summary>
        /// <param name="inventory">The inventory to update.</param>
        /// <returns>The number of rows affected.</returns>
        public int Update(Inventory inventory)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            if (inventory.InventoryId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inventory.InventoryId), "InventoryId must be greater than zero.");
            }

            if (inventory.ProductId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inventory.ProductId), "ProductId must be greater than zero.");
            }

            if (inventory.StockQuantity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inventory.StockQuantity), "StockQuantity must be greater than or equal to zero.");
            }

            if (inventory.ReorderLevel < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inventory.ReorderLevel), "ReorderLevel must be greater than or equal to zero.");
            }

            const string sql = @"UPDATE dbo.Inventory
SET StockQuantity = @StockQuantity,
    ReorderLevel = @ReorderLevel,
    LastUpdatedAt = @LastUpdatedAt
WHERE InventoryId = @InventoryId
  AND ProductId = @ProductId";
            var parameters = new[]
            {
                new SqlParameter("@InventoryId", SqlDbType.Int) { Value = inventory.InventoryId },
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = inventory.ProductId },
                new SqlParameter("@StockQuantity", SqlDbType.Int) { Value = inventory.StockQuantity },
                new SqlParameter("@ReorderLevel", SqlDbType.Int) { Value = inventory.ReorderLevel },
                new SqlParameter("@LastUpdatedAt", SqlDbType.DateTime2) { Scale = 7, Value = inventory.LastUpdatedAt }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update inventory. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates only the stock quantity for the specified product.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <param name="quantity">The new stock quantity.</param>
        /// <returns>The number of rows affected.</returns>
        public int UpdateStock(int productId, int quantity)
        {
            const string sql = @"UPDATE dbo.Inventory SET StockQuantity = @Quantity, LastUpdatedAt = @LastUpdatedAt WHERE ProductId = @ProductId";
            var parameters = new[]
            {
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId },
                new SqlParameter("@Quantity", SqlDbType.Int) { Value = quantity },
                new SqlParameter("@LastUpdatedAt", SqlDbType.DateTime2) { Value = DateTime.UtcNow }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update stock for product {productId}. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes an inventory record by its identifier.
        /// </summary>
        /// <param name="inventoryId">The inventory identifier.</param>
        /// <returns>The number of rows affected.</returns>
        public int Delete(int inventoryId)
        {
            const string sql = @"DELETE FROM dbo.Inventory WHERE InventoryId = @InventoryId";
            var parameters = new[]
            {
                new SqlParameter("@InventoryId", SqlDbType.Int) { Value = inventoryId }
            };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete inventory {inventoryId}. Details: {ex.Message}", ex);
            }
        }

        private static Inventory MapInventory(SqlDataReader reader)
        {
            return new Inventory
            {
                InventoryId = reader.IsDBNull(reader.GetOrdinal("InventoryId")) ? 0 : reader.GetInt32(reader.GetOrdinal("InventoryId")),
                ProductId = reader.IsDBNull(reader.GetOrdinal("ProductId")) ? 0 : reader.GetInt32(reader.GetOrdinal("ProductId")),
                StockQuantity = reader.IsDBNull(reader.GetOrdinal("StockQuantity")) ? 0 : reader.GetInt32(reader.GetOrdinal("StockQuantity")),
                ReorderLevel = reader.IsDBNull(reader.GetOrdinal("ReorderLevel")) ? 0 : reader.GetInt32(reader.GetOrdinal("ReorderLevel")),
                LastUpdatedAt = reader.IsDBNull(reader.GetOrdinal("LastUpdatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("LastUpdatedAt"))
            };
        }
    }
}
