using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class GetProductsRequest
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }
        public int? SellerId { get; set; }
        public string Status { get; set; } = "ACTIVE";
        public string? SessionKey { get; set; }
        public int? UserId { get; set; }
        public string? UserType { get; set; } 
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}
