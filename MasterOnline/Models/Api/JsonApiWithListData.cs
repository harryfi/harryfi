using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models.Api
{
    public class JsonApiWithListData
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<object> listdata { get; set; }
    }
}