using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MasterOnline.Models;

namespace MasterOnline.ViewModels
{
    public class CourierViewModel
    {
        public Ekspedisi Ekspedisi { get; set; }
        public IList<Ekspedisi> ListEkspedisi { get; set; } = new List<Ekspedisi>();
    }
}