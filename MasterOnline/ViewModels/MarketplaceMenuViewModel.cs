using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class MarketplaceMenuViewModel
    {
        public Marketplace Marketplace { get; set; }
        public List<Marketplace> ListMarket { get; set; } = new List<Marketplace>(); // Need initialization with VM
    }
}