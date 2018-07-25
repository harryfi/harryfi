using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class PromosiViewModel
    {
        public Promosi Promosi { get; set; }
        public List<Promosi> ListPromosi { get; set; } = new List<Promosi>();
        public DetailPromosi PromosiDetail { get; set; }
        public List<DetailPromosi> ListPromosiDetail { get; set; } = new List<DetailPromosi>();
        public List<STF02> ListBarang { get; set; } = new List<STF02>();
        public List<ARF01> ListPelanggan { get; set; } = new List<ARF01>();
        public List<Marketplace> ListMarketplace { get; set; } = new List<Marketplace>();
        public List<String> Errors { get; set; } = new List<String>();
    }
}