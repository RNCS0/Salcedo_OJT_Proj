using BaseCode.Models.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class GetVouchersResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public List<VoucherItem> Vouchers { get; set; }
        public int TotalCount { get; set; }
    }
}
