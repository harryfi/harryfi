using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class PromoViewModel
    {
        public Promo Promo { get; set; }
        public List<Promo> ListPromo { get; set; } = new List<Promo>();
    }
}