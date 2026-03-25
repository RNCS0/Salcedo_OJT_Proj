using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Responses
{
    public class GetPopularProductsResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public List<PopularProductItem> Products { get; set; }
        public int TotalCount { get; set; }
    }

    public class PopularProductItem
    {
        public int ProductId { get; set; }
        public int SellerId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Brand { get; set; }
        public string CategoryName { get; set; }
        public string StoreName { get; set; }
        public string Status { get; set; }
        public int TotalSold { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
}
