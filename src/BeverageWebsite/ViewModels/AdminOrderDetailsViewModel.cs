using System;
using System.Collections.Generic;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents complete read-only order details and available actions for an administrator.
    /// </summary>
    public class AdminOrderDetailsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminOrderDetailsViewModel"/> class.
        /// </summary>
        public AdminOrderDetailsViewModel()
        {
            Items = new List<AdminOrderDetailItemViewModel>();
        }

        /// <summary>
        /// Gets or sets the order identifier.
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the order was placed.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the exact order status stored in the database.
        /// </summary>
        public string OrderStatus { get; set; }

        /// <summary>
        /// Gets or sets the localized order status display name.
        /// </summary>
        public string OrderStatusDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the stored merchandise total before shipping.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the stored shipping fee.
        /// </summary>
        public decimal ShippingFee { get; set; }

        /// <summary>
        /// Gets or sets the stored final payment amount.
        /// </summary>
        public decimal FinalAmount { get; set; }

        /// <summary>
        /// Gets or sets the customer display name.
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the customer's stored email address.
        /// </summary>
        public string CustomerEmail { get; set; }

        /// <summary>
        /// Gets or sets the customer's stored phone number.
        /// </summary>
        public string CustomerPhone { get; set; }

        /// <summary>
        /// Gets or sets the delivery recipient's stored name.
        /// </summary>
        public string RecipientName { get; set; }

        /// <summary>
        /// Gets or sets the delivery recipient's stored phone number.
        /// </summary>
        public string RecipientPhone { get; set; }

        /// <summary>
        /// Gets or sets the stored delivery street address.
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Gets or sets the stored delivery ward.
        /// </summary>
        public string Ward { get; set; }

        /// <summary>
        /// Gets or sets the stored delivery district.
        /// </summary>
        public string District { get; set; }

        /// <summary>
        /// Gets or sets the stored delivery city.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the items included in the order.
        /// </summary>
        public List<AdminOrderDetailItemViewModel> Items { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the order can be confirmed.
        /// </summary>
        public bool CanConfirm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the order can move to processing.
        /// </summary>
        public bool CanProcess { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the order can be completed.
        /// </summary>
        public bool CanComplete { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the order can be cancelled.
        /// </summary>
        public bool CanCancel { get; set; }
    }
}
