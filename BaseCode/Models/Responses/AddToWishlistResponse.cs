using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class AddToWishlistResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int WishlistId { get; set; }
        public int ProductId { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
