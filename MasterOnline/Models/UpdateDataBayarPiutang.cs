using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class UpdateDataBayarPiutang
    {
        public string OrderId { get; set; }
        public double TotalFaktur { get; set; }
        public double TotalBayar { get; set; }
        public double TotalPotongan { get; set; }
        public string Tgl { get; set; }
        public string Cust { get; set; }
        public List<string> getRec { get; set; } = new List<string>();
        public List<double> getPot { get; set; } = new List<double>();
        public List<string> getFaktur { get; set; } = new List<string>();

        public double TotalLebih { get; set; }
    }
}