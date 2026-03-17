using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseCode.Models.Tables;

namespace BaseCode.Models.Responses
{
    public class GetProductByCategoryResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<Product> ProductsList { get; set; }
        public int TotalCount { get; set; }
    }
}
