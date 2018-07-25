using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class PostingViewModel
    {
        public class DataPosting
        {
            public string UserID { get; set; }
			public string Month { get; set; }
			public string Year { get; set; }
			public string From { get; set; }
			public string To { get; set; }
        }
    }
}
