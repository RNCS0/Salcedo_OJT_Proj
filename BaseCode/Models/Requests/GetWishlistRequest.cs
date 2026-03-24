using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Requests
{
    public class GetWishlistRequest
    {
        public int BuyerId { get; set; }
        public string SessionKey { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}
