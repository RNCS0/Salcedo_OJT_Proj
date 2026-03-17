using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Responses
{
    public class UpdateSellerProfileResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int SellerId { get; set; }
    }
}
