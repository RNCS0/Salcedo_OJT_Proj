using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class RecentOrderItem
    {
        public int OrderId { get; set; }
        public string BuyerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
