using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class RequestWalletCodeResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public string VerificationCode { get; set; }
    }
}
