using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class GetWalletTransactionsRequest
    {
        public int UserId { get; set; }
        public string SessionKey { get; set; }
        public string UserType { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string TransactionType { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
