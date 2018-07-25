using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class UpdateData
    {
        // Data Pesanan
        public string OrderId { get; set; }
        public double NilaiDisc { get; set; }
        public double OngkosKirim { get; set; }
        public int Ppn { get; set; }
        public double NilaiPpn { get; set; }
        public string Tgl { get; set; }
        public string Cust { get; set; }
        public double Term { get; set; }
        public string Exp { get; set; } //Ekspedisi
        public string Buyer { get; set; }
        public string Tempo { get; set; }
        public double Bruto { get; set; }

        // Data Stok
        public string NoBuktiStok { get; set; }
        public string TglInput { get; set; }

        //Invoice
        public string Supp { get; set; }
        public short TermInvoice { get; set; }
        public string KodeRefPesanan { get; set; }

        //Jurnal
        public double Debet { get; set; }
        public double Kredit { get; set; }

        //Password
        public string Username { get; set; }
        public string OldPass { get; set; }
        public string NewPass { get; set; }
        public bool WrongOldPass { get; set; } = false;

        //Promosi
        public int? RecNumPromosi { get; set; }
        public string NamaMarket { get; set; }
        public string TglMulai { get; set; }
        public string TglAkhir { get; set; }
    }
}