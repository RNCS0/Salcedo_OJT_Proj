using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Responses
{
    public class CreateWalletResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int WalletId { get; set; }
        public decimal Balance { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
