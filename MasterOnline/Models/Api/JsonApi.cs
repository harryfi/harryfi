using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models.Api
{
    public class JsonApi
    {
        public int code { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }
}