using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class MarketplaceViewModel
    {
        public STF02G MarketplaceAccount { get; set; }
        public IList<STF02G> ListMarketplaceAccount { get; set; }
    }
}