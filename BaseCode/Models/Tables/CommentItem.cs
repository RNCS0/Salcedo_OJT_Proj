using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Tables
{
    public class CommentItem
    {
        public int CommentId { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public string CommentText { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
