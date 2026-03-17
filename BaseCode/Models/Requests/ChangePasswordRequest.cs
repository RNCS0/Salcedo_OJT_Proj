using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class ChangePasswordRequest
    {
        public int UserId { get; set; }
        public string SessionKey { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
