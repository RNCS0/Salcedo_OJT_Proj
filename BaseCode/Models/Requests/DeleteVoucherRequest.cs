using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Requests
{
    public class DeleteVoucherRequest
    {
        public int SellerId { get; set; }
        public int VoucherId { get; set; }
        public string SessionKey { get; set; }
    }
}
