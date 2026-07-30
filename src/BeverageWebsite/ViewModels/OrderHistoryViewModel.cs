using System.Collections.Generic;

namespace BeverageWebsite.ViewModels
{
    /// <summary>
    /// Represents the authenticated customer's order history for presentation.
    /// </summary>
    public class OrderHistoryViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryViewModel"/> class.
        /// </summary>
        public OrderHistoryViewModel()
        {
            Orders = new List<OrderHistoryItemViewModel>();
        }

        /// <summary>
        /// Gets or sets the orders in the authenticated customer's order history.
        /// </summary>
        public List<OrderHistoryItemViewModel> Orders { get; set; }
    }
}
