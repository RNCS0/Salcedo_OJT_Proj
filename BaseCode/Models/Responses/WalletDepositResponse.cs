using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCode.Models.Responses
{
    public class WalletDepositResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int TransactionId { get; set; }
        public decimal NewBalance { get; set; }
        public decimal AmountDeposited { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
