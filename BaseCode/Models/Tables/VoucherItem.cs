using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Tables
{
    public class VoucherItem
    {
        public int VoucherId { get; set; }
        public int SellerId { get; set; }
        public string VoucherCode { get; set; }
        public string VoucherName { get; set; }
        public string VoucherDescription { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal MinimumSpend { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int MaxUses { get; set; }
        public int UsedCount { get; set; }
        public string VoucherStatus { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}
