using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class ValidateVoucherResponse
    {
        public bool isValid { get; set; }
        public string Message { get; set; }
        public string VoucherCode { get; set; }
        public string VoucherName { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal MinimumSpend { get; set; }
    }
}
