
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCode.Models.Tables
{
    public class Wallet
    {
        public int WalletId { get; set; }
        public int UserId { get; set; }
        public string UserType { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }

    public class WalletTransaction
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public string UserType { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Remarks { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
