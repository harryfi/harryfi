using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.ViewModels
{
    public class PartnerApiViewModel
    {
        public List<PARTNER_API> ListPartnerApi { get; set; } = new List<PARTNER_API>();

        public PARTNER_API partner_api { get; set; } = new PARTNER_API();

        public List<ARF01> ListCustomer { get; set; } = new List<ARF01>();
        public ARF01 Customer { get; set; } = new ARF01();
    }
}