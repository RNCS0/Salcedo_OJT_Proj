using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Requests
{
    public class ClearCartRequest
    {
        public int BuyerId { get; set; }
        public string SessionKey { get; set; }
    }
}
