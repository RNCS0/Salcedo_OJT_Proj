using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCode.Models.Responses
{
    public class GetWalletBalanceResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public string UserType { get; set; }
        public string UserName { get; set; }
        public decimal Balance { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
