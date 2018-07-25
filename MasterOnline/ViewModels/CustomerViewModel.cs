using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class CustomerViewModel
    {
        public ARF01 Customers { get; set; }
        public List<ARF01> ListCustomer { get; set; } = new List<ARF01>();
        public List<string> Errors { get; set; } = new List<string>();
        public List<Subscription> ListSubs { get; set; } = new List<Subscription>();
    }
}