using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class PenjualanBarang
    {
        public string KodeBrg { get; set; }
        public string NamaBrg { get; set; }
        public string Kategori { get; set; }
        public string Merk { get; set; }
        public double HJual { get; set; }
        public double Qty { get; set; }
        public bool Laku { get; set; }
    }
}