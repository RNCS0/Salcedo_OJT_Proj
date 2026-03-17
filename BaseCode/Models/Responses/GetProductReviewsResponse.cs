using BaseCode.Models.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class GetProductReviewsResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public List<ReviewItem> Reviews { get; set; }
        public double AverageRating { get; set; }
    }
}
