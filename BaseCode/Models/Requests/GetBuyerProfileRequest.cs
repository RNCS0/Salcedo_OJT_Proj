using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class GetBuyerProfileRequest
    {
        public int BuyerId { get; set; }
        public string SessionKey { get; set; }
    }
}
