using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Tables
{
    public class Product
    {
        public int ProductId { get; set; }
        public int SellerId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public string Status { get; set; }
        public string StoreName { get; set; }
        public string SellerName { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
