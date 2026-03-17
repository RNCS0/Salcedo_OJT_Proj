using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Requests
{
    public class DeleteProductRequest
    {
        public int SellerId { get; set; }
        public int ProductId { get; set; }
        public string SessionKey { get; set; }
    }
}
