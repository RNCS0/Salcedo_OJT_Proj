using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Requests
{
    public class ApplyVoucherRequest
    {
        public int UserId { get; set; }
        public string SessionKey { get; set; }
        public string UserType { get; set; }
        public string VoucherCode { get; set; }
        public decimal CartTotal { get; set; }
        public int? OrderId { get; set; }
    }
}
