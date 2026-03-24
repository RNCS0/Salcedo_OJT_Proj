using BaseCode.Models.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vonage.Common;


namespace BaseCode.Models.Responses
{
    public class GetAllCategoriesResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public List<CategoryItem> Categories { get; set; }
        public int TotalCount { get; set; }
    }
}
