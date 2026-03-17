using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Requests
{
    public class UpdateCartItemRequest
    {
        public int BuyerId { get; set; }
        public string SessionKey { get; set; }
        public int CartId { get; set; }
        public int NewQuantity { get; set; }
    }
}
