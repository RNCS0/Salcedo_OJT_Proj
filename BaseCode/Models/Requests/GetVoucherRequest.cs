using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Requests
{
    public class GetVouchersRequest
    {
        public int? SellerId { get; set; }
        public string SessionKey { get; set; }
        public string Status { get; set; }
    }
}
