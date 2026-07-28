using System;
using System.Collections.Generic;
using BeverageWebsite.DAL;
using BeverageWebsite.Models;

namespace BeverageWebsite.BLL
{
    /// <summary>
    /// Provides business operations for managing inventory records.
    /// </summary>
    public class InventoryBLL
    {
        private readonly InventoryDAL _inventoryDal;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryBLL"/> class.
        /// </summary>
        public InventoryBLL()
        {
            _inventoryDal = new InventoryDAL();
        }

        /// <summary>
        /// Retrieves all inventory records.
        /// </summary>
        /// <returns>All inventory records returned by the data access layer.</returns>
        public List<Inventory> GetAll()
        {
            return _inventoryDal.GetAll();
        }

        /// <summary>
        /// Retrieves an inventory record by its identifier.
        /// </summary>
        /// <param name="inventoryId">The inventory identifier.</param>
        /// <returns>The matching inventory record when found; otherwise, null.</returns>
        public Inventory GetById(int inventoryId)
        {
            ValidateIdentifier(inventoryId, nameof(inventoryId));
            return _inventoryDal.GetById(inventoryId);
        }

        /// <summary>
        /// Retrieves the inventory record for a product.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <returns>The product inventory record when found; otherwise, null.</returns>
        public Inventory GetByProductId(int productId)
        {
            ValidateIdentifier(productId, nameof(productId));
            return _inventoryDal.GetByProductId(productId);
        }

        /// <summary>
        /// Validates and creates an inventory record.
        /// </summary>
        /// <param name="inventory">The inventory data to create.</param>
        /// <returns>The number of records affected.</returns>
        public int Create(Inventory inventory)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            ValidateIdentifier(inventory.ProductId, nameof(inventory.ProductId));
            ValidateStockQuantity(
                inventory.StockQuantity,
                nameof(inventory.StockQuantity));
            ValidateReorderLevel(
                inventory.ReorderLevel,
                nameof(inventory.ReorderLevel));

            var normalizedInventory = new Inventory
            {
                ProductId = inventory.ProductId,
                StockQuantity = inventory.StockQuantity,
                ReorderLevel = inventory.ReorderLevel,
                LastUpdatedAt = inventory.LastUpdatedAt
            };

            return _inventoryDal.Insert(normalizedInventory);
        }

        /// <summary>
        /// Validates and updates an inventory record.
        /// </summary>
        /// <param name="inventory">The inventory data to update.</param>
        /// <returns>The number of records affected.</returns>
        public int Update(Inventory inventory)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            ValidateIdentifier(inventory.InventoryId, nameof(inventory.InventoryId));
            ValidateIdentifier(inventory.ProductId, nameof(inventory.ProductId));
            ValidateStockQuantity(
                inventory.StockQuantity,
                nameof(inventory.StockQuantity));
            ValidateReorderLevel(
                inventory.ReorderLevel,
                nameof(inventory.ReorderLevel));

            var normalizedInventory = new Inventory
            {
                InventoryId = inventory.InventoryId,
                ProductId = inventory.ProductId,
                StockQuantity = inventory.StockQuantity,
                ReorderLevel = inventory.ReorderLevel,
                LastUpdatedAt = inventory.LastUpdatedAt
            };

            return _inventoryDal.Update(normalizedInventory);
        }

        /// <summary>
        /// Replaces the stock quantity for a product.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <param name="quantity">The new absolute stock quantity.</param>
        /// <returns>The number of records affected.</returns>
        public int UpdateStock(int productId, int quantity)
        {
            ValidateIdentifier(productId, nameof(productId));
            ValidateStockQuantity(quantity, nameof(quantity));
            return _inventoryDal.UpdateStock(productId, quantity);
        }

        /// <summary>
        /// Deletes an inventory record by its identifier.
        /// </summary>
        /// <param name="inventoryId">The inventory identifier.</param>
        /// <returns>The number of records affected.</returns>
        public int Delete(int inventoryId)
        {
            ValidateIdentifier(inventoryId, nameof(inventoryId));
            return _inventoryDal.Delete(inventoryId);
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

        private static void ValidateStockQuantity(
            int stockQuantity,
            string parameterName)
        {
            if (stockQuantity < 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "The stock quantity must be greater than or equal to zero.");
            }
        }

        private static void ValidateReorderLevel(
            int reorderLevel,
            string parameterName)
        {
            if (reorderLevel < 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "The reorder level must be greater than or equal to zero.");
            }
        }
    }
}
