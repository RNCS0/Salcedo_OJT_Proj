using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class GetSellerDashboardResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }

        // Product Statistics
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public int SoldOutProducts { get; set; }

        // Order Statistics
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }

        // Sales Statistics
        public decimal TotalSales { get; set; }
        public decimal TodaySales { get; set; }
        public decimal ThisWeekSales { get; set; }
        public decimal ThisMonthSales { get; set; }

        public List<RecentOrderItem> RecentOrders { get; set; }

        public List<TopProductItem> TopProducts { get; set; }
    }
}
