using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Requests
{
    public class AddToWishlistRequest
    {
        public int BuyerId { get; set; }
        public string SessionKey { get; set; }
        public int ProductId { get; set; }
    }
}
