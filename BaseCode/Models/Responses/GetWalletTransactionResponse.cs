using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseCode.Models.Tables;

namespace BaseCode.Models.Responses
{
    public class GetWalletTransactionsResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public List<WalletTransactionItem> Transactions { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public decimal CurrentBalance { get; set; }
    }

    public class WalletTransactionItem
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Remarks { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
