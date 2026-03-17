using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class ApplyVoucherResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int VoucherId { get; set; }
        public string VoucherCode { get; set; }
        public string VoucherName { get; set; }
        public decimal OriginalTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalTotal { get; set; }
        public decimal MinimumSpend { get; set; }
    }
}
