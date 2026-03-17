using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class AddReviewResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int ReviewId { get; set; }
    }
}
