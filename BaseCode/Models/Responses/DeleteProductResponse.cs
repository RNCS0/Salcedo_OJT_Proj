using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Responses
{
    public class DeleteProductResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int ProductId { get; set; }
        public string DeactivationDate { get; set; }
    }
}
