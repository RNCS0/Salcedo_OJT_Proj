using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Requests
{
    public class AddReviewRequest
    {
        public int ProductId { get; set; }
        public int BuyerId { get; set; }
        public string SessionKey { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
    }
}
