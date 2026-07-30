using System;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents an order in the authenticated customer's order history for presentation.
    /// </summary>
    public class OrderHistoryItemViewModel
    {
        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the order date.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the order status.
        /// </summary>
        public string OrderStatus { get; set; }

        /// <summary>
        /// Gets or sets the order total before shipping.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the shipping fee.
        /// </summary>
        public decimal ShippingFee { get; set; }

        /// <summary>
        /// Gets or sets the final order amount.
        /// </summary>
        public decimal FinalAmount { get; set; }
    }
}
