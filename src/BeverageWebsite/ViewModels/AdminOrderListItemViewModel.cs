using System;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents read-only order summary data displayed in the administration order list.
    /// </summary>
    public class AdminOrderListItemViewModel
    {
        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the order was placed.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the customer display name.
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the customer's stored email address.
        /// </summary>
        public string CustomerEmail { get; set; }

        /// <summary>
        /// Gets or sets the exact order status stored in the database.
        /// </summary>
        public string OrderStatus { get; set; }

        /// <summary>
        /// Gets or sets the localized order status display name.
        /// </summary>
        public string OrderStatusDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the final amount paid for the order.
        /// </summary>
        public decimal FinalAmount { get; set; }
    }
}
