using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class CreateWalletRequest
    {
        public int UserId { get; set; }
        public string SessionKey { get; set; }
        public string UserType { get; set; }
        public string Email { get; set; }
        public string VerificationCode { get; set; }
    }
}