using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class CreateVoucherResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int VoucherId { get; set; }
        public string VoucherCode { get; set; }
    }
}
