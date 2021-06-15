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

    public class DatabaseListViewModel
    {
        public List<DatabaseList> ListDatabase { get; set; } = new List<DatabaseList>();
    }

    public class DatabaseList
    {
        public string trialEnd { get; set; }
        public bool expired { get; set; }
        public string bgColor { get; set; }
        public string licenseEnd { get; set; }
        public string alias { get; set; }
        public string logo { get; set; }
        public bool admin { get; set; }
        public int id { get; set; }
        public string accessibleUntil { get; set; }
        public bool sample { get; set; }
        public string logoUrl { get; set; }
        public bool trial { get; set; }
    }
}