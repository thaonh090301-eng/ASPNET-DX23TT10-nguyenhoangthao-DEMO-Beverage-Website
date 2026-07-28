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
                throw new InvalidOperationException("Failed to retrieve inventory records.", ex);
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
            if (inventoryId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inventoryId), "The inventory identifier must be greater than zero.");
            }

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
                throw new InvalidOperationException("Failed to retrieve the inventory record.", ex);
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
            if (productId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(productId), "The product identifier must be greater than zero.");
            }

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
                throw new InvalidOperationException("Failed to retrieve inventory for the product.", ex);
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

            if (inventory.ProductId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inventory.ProductId), "The product identifier must be greater than zero.");
            }

            if (inventory.StockQuantity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inventory.StockQuantity), "The stock quantity must be greater than or equal to zero.");
            }

            if (inventory.ReorderLevel < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inventory.ReorderLevel), "The reorder level must be greater than or equal to zero.");
            }

            const string sql = @"INSERT INTO dbo.Inventory (ProductId, StockQuantity, ReorderLevel, LastUpdatedAt) VALUES (@ProductId, @StockQuantity, @ReorderLevel, @LastUpdatedAt)";
            var parameters = new[]
            {
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = inventory.ProductId },
                new SqlParameter("@StockQuantity", SqlDbType.Int) { Value = inventory.StockQuantity },
                new SqlParameter("@ReorderLevel", SqlDbType.Int) { Value = inventory.ReorderLevel },
                CreateLastUpdatedAtParameter(inventory.LastUpdatedAt)
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to insert the inventory record.", ex);
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The inventory insert did not affect exactly one record.");
            }

            return affectedRows;
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
                CreateLastUpdatedAtParameter(inventory.LastUpdatedAt)
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update the inventory record.", ex);
            }

            if (affectedRows == 0)
            {
                throw new InvalidOperationException("The expected inventory record was not found.");
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The inventory update did not affect exactly one record.");
            }

            return affectedRows;
        }

        /// <summary>
        /// Updates only the stock quantity for the specified product.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <param name="quantity">The new stock quantity.</param>
        /// <returns>The number of rows affected.</returns>
        public int UpdateStock(int productId, int quantity)
        {
            if (productId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(productId), "The product identifier must be greater than zero.");
            }

            if (quantity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "The stock quantity must be greater than or equal to zero.");
            }

            const string sql = @"UPDATE dbo.Inventory SET StockQuantity = @Quantity, LastUpdatedAt = @LastUpdatedAt WHERE ProductId = @ProductId";
            var parameters = new[]
            {
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId },
                new SqlParameter("@Quantity", SqlDbType.Int) { Value = quantity },
                CreateLastUpdatedAtParameter(DateTime.UtcNow)
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update inventory stock.", ex);
            }

            if (affectedRows == 0)
            {
                throw new InvalidOperationException("No inventory record was found for the product.");
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The stock update did not affect exactly one record.");
            }

            return affectedRows;
        }

        /// <summary>
        /// Deletes an inventory record by its identifier.
        /// </summary>
        /// <param name="inventoryId">The inventory identifier.</param>
        /// <returns>The number of rows affected.</returns>
        public int Delete(int inventoryId)
        {
            if (inventoryId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inventoryId), "The inventory identifier must be greater than zero.");
            }

            const string sql = @"DELETE FROM dbo.Inventory WHERE InventoryId = @InventoryId";
            var parameters = new[]
            {
                new SqlParameter("@InventoryId", SqlDbType.Int) { Value = inventoryId }
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to delete the inventory record.", ex);
            }

            if (affectedRows == 0)
            {
                throw new InvalidOperationException("The inventory record was not found.");
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The inventory delete did not affect exactly one record.");
            }

            return affectedRows;
        }

        private static SqlParameter CreateLastUpdatedAtParameter(DateTime value)
        {
            return new SqlParameter("@LastUpdatedAt", SqlDbType.DateTime2)
            {
                Scale = 7,
                Value = value
            };
        }

        private static Inventory MapInventory(SqlDataReader reader)
        {
            var inventoryIdOrdinal = reader.GetOrdinal("InventoryId");
            var productIdOrdinal = reader.GetOrdinal("ProductId");
            var stockQuantityOrdinal = reader.GetOrdinal("StockQuantity");
            var reorderLevelOrdinal = reader.GetOrdinal("ReorderLevel");
            var lastUpdatedAtOrdinal = reader.GetOrdinal("LastUpdatedAt");

            return new Inventory
            {
                InventoryId = reader.IsDBNull(inventoryIdOrdinal) ? 0 : reader.GetInt32(inventoryIdOrdinal),
                ProductId = reader.IsDBNull(productIdOrdinal) ? 0 : reader.GetInt32(productIdOrdinal),
                StockQuantity = reader.IsDBNull(stockQuantityOrdinal) ? 0 : reader.GetInt32(stockQuantityOrdinal),
                ReorderLevel = reader.IsDBNull(reorderLevelOrdinal) ? 0 : reader.GetInt32(reorderLevelOrdinal),
                LastUpdatedAt = reader.IsDBNull(lastUpdatedAtOrdinal) ? DateTime.MinValue : reader.GetDateTime(lastUpdatedAtOrdinal)
            };
        }
    }
}
