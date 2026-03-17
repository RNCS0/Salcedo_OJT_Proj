using BaseCode.Models.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class GetProductCommentsResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public List<CommentItem> Comments { get; set; }
    }
}
