using BaseCode.Models.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class GetCartResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int BuyerId { get; set; }
        public int TotalItems { get; set; }  // This already exists
        public decimal Subtotal { get; set; }
        public List<CartItem> Items { get; set; }
    }
}
