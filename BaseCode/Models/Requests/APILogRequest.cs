using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class APILogRequest
    {
        public string ApiName { get; set; }
        public string RequestData { get; set; }
        public string ResponseData { get; set; }
        public string LogTime { get; set; }
    }
}
