using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Requests
{
    public class AddCommentRequest
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string UserType { get; set; }
        public string SessionKey { get; set; }
        public string CommentText { get; set; }
    }
}
