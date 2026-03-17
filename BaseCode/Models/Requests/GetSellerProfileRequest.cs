using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class GetSellerProfileRequest
    {
        public int SellerId { get; set; }
        public string SessionKey { get; set; }
    }
}
