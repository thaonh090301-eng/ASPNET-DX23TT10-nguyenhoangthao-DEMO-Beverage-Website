using System;
using System.Collections.Generic;
using BeverageWebsite.DAL;
using BeverageWebsite.Models;

namespace BeverageWebsite.BLL
{
    /// <summary>
    /// Provides business operations for managing user carts and cart items.
    /// </summary>
    public class CartBLL
    {
        private readonly CartDAL _cartDal;

        /// <summary>
        /// Initializes a new instance of the <see cref="CartBLL"/> class.
        /// </summary>
        public CartBLL()
        {
            _cartDal = new CartDAL();
        }

        /// <summary>
        /// Retrieves the cart belonging to a user without creating one.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user's cart when found; otherwise, null.</returns>
        public Cart GetByUserId(int userId)
        {
            ValidateIdentifier(userId, nameof(userId));
            return _cartDal.GetCartByUserId(userId);
        }

        /// <summary>
        /// Creates a cart for a user using database-default timestamps.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The number of records affected.</returns>
        public int Create(int userId)
        {
            ValidateIdentifier(userId, nameof(userId));
            return _cartDal.CreateCart(userId);
        }

        /// <summary>
        /// Retrieves all items in a cart.
        /// </summary>
        /// <param name="cartId">The cart identifier.</param>
        /// <returns>The cart items returned by the data access layer.</returns>
        public List<CartItem> GetCartItems(int cartId)
        {
            ValidateIdentifier(cartId, nameof(cartId));
            return _cartDal.GetCartItems(cartId);
        }

        /// <summary>
        /// Adds a product to a user-owned cart using its current active product price
        /// loaded by the data access layer.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the cart.</param>
        /// <param name="cartId">The cart identifier.</param>
        /// <param name="productId">The product identifier.</param>
        /// <param name="quantity">The positive quantity to add.</param>
        /// <returns>The number of records affected.</returns>
        public int AddItem(int userId, int cartId, int productId, int quantity)
        {
            ValidateIdentifier(userId, nameof(userId));
            ValidateIdentifier(cartId, nameof(cartId));
            ValidateIdentifier(productId, nameof(productId));
            ValidateQuantity(quantity);
            return _cartDal.AddItem(userId, cartId, productId, quantity);
        }

        /// <summary>
        /// Replaces the quantity of an item in a user-owned cart.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the cart.</param>
        /// <param name="cartId">The cart identifier.</param>
        /// <param name="cartItemId">The cart-item identifier.</param>
        /// <param name="quantity">The new positive quantity.</param>
        /// <returns>The number of records affected.</returns>
        public int UpdateQuantity(
            int userId,
            int cartId,
            int cartItemId,
            int quantity)
        {
            ValidateIdentifier(userId, nameof(userId));
            ValidateIdentifier(cartId, nameof(cartId));
            ValidateIdentifier(cartItemId, nameof(cartItemId));
            ValidateQuantity(quantity);
            return _cartDal.UpdateQuantity(userId, cartId, cartItemId, quantity);
        }

        /// <summary>
        /// Removes an item from a user-owned cart.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the cart.</param>
        /// <param name="cartId">The cart identifier.</param>
        /// <param name="cartItemId">The cart-item identifier.</param>
        /// <returns>The number of records affected.</returns>
        public int RemoveItem(int userId, int cartId, int cartItemId)
        {
            ValidateIdentifier(userId, nameof(userId));
            ValidateIdentifier(cartId, nameof(cartId));
            ValidateIdentifier(cartItemId, nameof(cartItemId));
            return _cartDal.RemoveItem(userId, cartId, cartItemId);
        }

        /// <summary>
        /// Clears all items from a user-owned cart without deleting the cart.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the cart.</param>
        /// <param name="cartId">The cart identifier.</param>
        /// <returns>The number of records affected, including zero for an empty cart.</returns>
        public int ClearCart(int userId, int cartId)
        {
            ValidateIdentifier(userId, nameof(userId));
            ValidateIdentifier(cartId, nameof(cartId));
            return _cartDal.ClearCart(userId, cartId);
        }

        /// <summary>
        /// Retrieves the total monetary value of a cart.
        /// </summary>
        /// <param name="cartId">The cart identifier.</param>
        /// <returns>The total calculated by the data access layer.</returns>
        public decimal GetCartTotal(int cartId)
        {
            ValidateIdentifier(cartId, nameof(cartId));
            return _cartDal.GetCartTotal(cartId);
        }

        /// <summary>
        /// Retrieves the sum of item quantities in a cart.
        /// </summary>
        /// <param name="cartId">The cart identifier.</param>
        /// <returns>The total item quantity calculated by the data access layer.</returns>
        public int GetTotalItems(int cartId)
        {
            ValidateIdentifier(cartId, nameof(cartId));
            return _cartDal.GetTotalItems(cartId);
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

        private static void ValidateQuantity(int quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quantity),
                    "Quantity must be greater than zero.");
            }
        }
    }
}
