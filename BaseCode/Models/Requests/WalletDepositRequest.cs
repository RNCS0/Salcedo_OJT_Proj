using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class WalletDepositRequest
    {
        public int UserId { get; set; }
        public string SessionKey { get; set; }
        public string UserType { get; set; } 
        public decimal Amount { get; set; }
        public string ReferenceNumber { get; set; } 
        public string PaymentMethod { get; set; }
    }
}
