using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class MenuSupport
    {
        public Account Account { get; set; }
        public ARF01 TokoMarketplaces { get; set; }
        public STF02H BarangMP { get; set; }

        public List<Account> ListAccount { get; set; } = new List<Account>();
        public List<ARF01> ListTokoMarketplaces { get; set; } = new List<ARF01>();
        public List<STF02H> ListBarangMP { get; set; } = new List<STF02H>();

        public List<String> Errors { get; set; } = new List<String>();
    }
}