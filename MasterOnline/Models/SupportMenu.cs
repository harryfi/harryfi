using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class SupportMenu
    {
        public Account Account { get; set; }
        public ARF01 TokoMarketplaces { get; set; }
        public STF02H BarangMP { get; set; }

        public List<Account> ListAccount { get; set; } = new List<Account>();
        public List<ARF01> ListTokoMarketplaces { get; set; } = new List<ARF01>();
        public List<STF02H> ListBarangMP { get; set; } = new List<STF02H>();

        public List<String> Errors { get; set; } = new List<String>();

        public List<string> AccountList { get; set; }

        public List<ListMarketplaces> ListTokoMPCustomers { get; set; }
    }

    public class ListMarketplaces
    {
        public int cust { get; set; }
        public string namaCust { get; set; }
        public string namaMarket { get; set; }
        public bool stat { get; set; }
    }
}