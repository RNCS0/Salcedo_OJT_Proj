using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class SearchSellerRequest
    {
        public string Keyword { get; set; }
        public string Status { get; set; } = "ACTIVE";
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public bool ShowAll { get; set; } = false;
    }
}
