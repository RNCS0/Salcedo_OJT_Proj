using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class ClearCartResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int BuyerId { get; set; }
        public DateTime ClearedDate { get; set; }
    }
}
