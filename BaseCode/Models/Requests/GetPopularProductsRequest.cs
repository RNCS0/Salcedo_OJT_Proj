using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class GetPopularProductsRequest
    {
        public int Limit { get; set; } = 10;
        public int? CategoryId { get; set; }
    }
}
