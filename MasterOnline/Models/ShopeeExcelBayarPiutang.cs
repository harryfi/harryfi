using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class ShopeeExcelBayarPiutang
    {
        public DateTime Tanggal { get; set; }
        public string PenerimaDana { get; set; }
        public double JumlahDana { get; set; }
        public string Deskripsi { get; set; }
        public string Status { get; set; }
        public double Saldo { get; set; }

        //public string Noref { get; set; }
    }
}