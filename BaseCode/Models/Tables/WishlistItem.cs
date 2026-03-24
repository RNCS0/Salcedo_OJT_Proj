using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Tables
{
    public class WishlistItem
    {
        public int WishlistId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public string Brand { get; set; }
        public string CategoryName { get; set; }
        public string Status { get; set; }
        public int AvailableStock { get; set; }
        public int SellerId { get; set; }
        public string StoreName { get; set; }
        public DateTime DateAdded { get; set; }
        public string ImageUrl { get; set; }
    }
}
