using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Responses
{
    public class SearchSellerResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public List<SellerSearchItem> Sellers { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class SellerSearchItem
    {
        public int SellerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public string StoreName { get; set; }
        public string Status { get; set; }
        public DateTime DateRegistered { get; set; }
        public int TotalProducts { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
}
