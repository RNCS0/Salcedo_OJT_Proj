using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Requests
{
    public class CreateVoucherRequest
    {
        public int SellerId { get; set; } 
        public string SessionKey { get; set; }
        public string VoucherCode { get; set; }
        public string VoucherName { get; set; }
        public string VoucherDescription { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal MinimumSpend { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int MaxUses { get; set; }
    }
}
