using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Tables
{
    public class ReviewItem
    {
        public int ReviewId { get; set; }
        public string BuyerName { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
