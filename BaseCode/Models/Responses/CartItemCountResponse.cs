using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class CartItemCountResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int TotalItems { get; set; }
    }
}
