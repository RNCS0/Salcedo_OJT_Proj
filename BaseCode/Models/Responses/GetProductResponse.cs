using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseCode.Models.Tables;

namespace BaseCode.Models.Responses
{
    public class GetProductsResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public List<Product> ProductsList { get; set; }
        public int TotalCount { get; set; }
        public string? CategoryName { get; set; }
        public int? CategoryId { get; set; }
        public string? SellerName { get; set; }
        public int? SellerId { get; set; }
        public int? CurrentPage { get; set; }
        public int? PageSize { get; set; }
        public int? TotalPages { get; set; }
    }
}
