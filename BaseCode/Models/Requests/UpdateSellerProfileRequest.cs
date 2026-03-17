using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCode.Models.Requests
{
    public class UpdateSellerProfileRequest
    {
        public int SellerId { get; set; }
        public string SessionKey { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StoreName { get; set; }
        public string UserName { get; set; }
    }
}
