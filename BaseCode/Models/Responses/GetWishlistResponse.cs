using System;
using System.Collections.Generic;
using System.Text;
using BaseCode.Models.Tables;

namespace BaseCode.Models.Responses
{
    public class GetWishlistResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int BuyerId { get; set; }
        public int TotalItems { get; set; }
        public List<WishlistItem> WishlistItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}