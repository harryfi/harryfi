using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class PaymentMidtransViewModel
    {
        public string typeSubscription { get; set; }
        public string price { get; set; }
        public string subDesc { get; set; }
        public string tokenMidtrans { get; set; }
        public string urlView { get; set; }
    }
}