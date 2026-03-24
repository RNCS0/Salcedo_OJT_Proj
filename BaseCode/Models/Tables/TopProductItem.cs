using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class TopProductItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int UnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
