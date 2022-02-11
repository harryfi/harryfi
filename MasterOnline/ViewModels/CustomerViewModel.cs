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
        //add by Tri
        public string kodeCust { get; set; }
        public string marketplace { get; set; }

        //add by nurul 2/1/2022
        public List<MAPPING_GUDANG> ListMappingGudang { get; set; } = new List<MAPPING_GUDANG>();
        public List<STF18> ListGudang { get; set; } = new List<STF18>();
        public MAPPING_GUDANG MappingGudang { get; set; }
        public string MULTILOKASI { get; set; }
    }
}