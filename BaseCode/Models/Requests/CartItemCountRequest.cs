using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Requests
{
    public class CartItemCountRequest
    {
        public int BuyerId { get; set; }
        public string SessionKey { get; set; }
    }
}
