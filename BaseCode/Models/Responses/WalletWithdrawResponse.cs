using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCode.Models.Responses
{
    public class WalletWithdrawResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public decimal NewBalance { get; set; }
        public decimal AmountWithdrawn { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
