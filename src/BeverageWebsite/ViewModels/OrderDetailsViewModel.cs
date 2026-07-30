using System;
using System.Collections.Generic;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents read-only presentation data for an authenticated customer's order details.
    /// </summary>
    public class OrderDetailsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderDetailsViewModel"/> class.
        /// </summary>
        public OrderDetailsViewModel()
        {
            Items = new List<OrderDetailItemViewModel>();
        }

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

        /// <summary>
        /// Gets or sets the items in the authenticated customer's order details.
        /// </summary>
        public List<OrderDetailItemViewModel> Items { get; set; }
    }
}
