using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class AddToCartResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int CartId { get; set; }
        public int TotalItems { get; set; }
        public decimal CartTotal { get; set; }
    }
}
