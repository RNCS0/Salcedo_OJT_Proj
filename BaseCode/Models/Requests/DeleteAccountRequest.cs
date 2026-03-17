using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class DeleteAccountRequest
    {
        public int UserId { get; set; }
        public string SessionKey { get; set; }
    }
}

