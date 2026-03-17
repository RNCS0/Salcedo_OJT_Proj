using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseCode.Models.Tables;


namespace BaseCode.Models.Responses
{
    public class GetBuyerProfileResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int BuyerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public string Status { get; set; }
        public DateTime DateRegistered { get; set; }
        public int TotalOrders { get; set; }
    }
}
