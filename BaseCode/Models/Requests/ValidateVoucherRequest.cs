using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Requests
{
    public class ValidateVoucherRequest
    {
        public string VoucherCode { get; set; }
        public decimal CartTotal { get; set; }
    }
}
