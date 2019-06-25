using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class SyncMenuViewModel
    {
        public List<string> Errors { get; set; } = new List<string>();
        public List<BindingCustomer> Customers { get; set; }
    }

    public class BindingCustomer
    {
        public string cust { get; set; }
        public string namaCust { get; set; }
        public string namaMarket { get; set; }
        public bool stat { get; set; }
    }
}