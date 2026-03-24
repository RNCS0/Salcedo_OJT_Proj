using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Requests
{
    public class GetSellerDashboardRequest
    {
        public int SellerId { get; set; }
        public string SessionKey { get; set; }
    }
}
