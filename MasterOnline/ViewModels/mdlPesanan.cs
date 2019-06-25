using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Linq;
    using System.Web;
    using MasterOnline.Models;
    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Web;
    //using MasterOnline.Models;

    //namespace MasterOnline.ViewModels
    //{
    public class mdlPesanan
    {
        public int? RECNUM { get; set; }
        public string NOSO { get; set; }
        public DateTime? TGL { get; set; }
        public string MARKET { get; set; }
        public string PEMBELI { get; set; }
        public double TOTAL { get; set; }
        public string STATUS { get; set; }
        public string RESI { get; set; }
        public string FAKTUR { get; set; }
        //public DateTime? TGL_FAKTUR { get; set; }
        public System.Nullable<DateTime> TGL_FAKTUR { get; set; }
        public string NO_FAKTUR { get; set; }
        public string PERSO { get; set; }
        public string CUST { get; set; }
        public string REFERENSI { get; set; }
        public string POSTING { get; set; }
        public DateTime? TGLJTTEMPO { get; set; }
        public string PEMBAYARAN { get; set; }
        public string SUPPLIER { get; set; }
        
        //ADD BY CALVIN 19 JUNI 2019
        public string USER_NAME { get; set; }
        //END ADD BY CALVIN 19 JUNI 2019
        //ADD BY NURUL 20/6/2019, FLAG PRINT FAKTUR
        public string PRINT_FAKTUR { get; set; }
        //END ADD BY NURUL 20/6/2019

        //[Key]
        //[StringLength(15)]
        //public string NO_BUKTI { get; set; }

        //[Required]
        //public DateTime? TGL { get; set; }

        //public DateTime? TGL_KIRIM { get; set; }

        //[StringLength(1)]
        //public string STATUS { get; set; }

        //[StringLength(30)]
        //public string NO_PO_CUST { get; set; }

        //[Required]
        //[StringLength(10)]
        //public string CUST { get; set; }

        //[StringLength(40)]
        //public string NAMA_CUST { get; set; }

        //[StringLength(3)]
        //public string VLT { get; set; }

        //public double NILAI_TUKAR { get; set; }

        //[StringLength(5)]
        //public string KODE_SALES { get; set; }

        //[StringLength(5)]
        //public string KODE_WIL { get; set; }

        //[StringLength(5)]
        //public string KODE_ALAMAT { get; set; }

        //[Column(TypeName = "text")]
        //public string KET { get; set; }

        //public double DISCOUNT { get; set; }

        //public double NILAI_DISC { get; set; }

        //public double PPN { get; set; }

        //public double NILAI_PPN { get; set; }

        //public double BRUTO { get; set; }

        //public double NETTO { get; set; }

        //[StringLength(20)]
        //public string USER_NAME { get; set; }

        //public DateTime? TGL_INPUT { get; set; }

        //public int PRINT_COUNT { get; set; }

        //public bool KIRIM_PENUH { get; set; }

        //public bool RETUR_PENUH { get; set; }

        //[StringLength(40)]
        //public string AL { get; set; }

        //[StringLength(40)]
        //public string AL1 { get; set; }

        //[StringLength(40)]
        //public string AL2 { get; set; }

        //[StringLength(40)]
        //public string AL3 { get; set; }

        //[StringLength(50)]
        //public string AL_CUST { get; set; }

        //public double? U_MUKA { get; set; }

        //public double TERM { get; set; }

        //[StringLength(10)]
        //public string CUST_QQ { get; set; }

        //[StringLength(1)]
        //public string HARGA_FRANCO { get; set; }

        //[StringLength(1)]
        //public string Status_Approve { get; set; }

        //[StringLength(20)]
        //public string User_Approve { get; set; }

        //public DateTime? Date_Approve { get; set; }

        //[StringLength(10)]
        //public string NO_PENAWARAN { get; set; }

        //public bool INDENT { get; set; }

        //[StringLength(10)]
        //public string PENGIRIM { get; set; }

        //[StringLength(50)]
        //public string NAMAPENGIRIM { get; set; }

        //[StringLength(5)]
        //public string ZONA { get; set; }

        //public DateTime? JAMKIRIM { get; set; }

        //[StringLength(5)]
        //public string UCAPAN { get; set; }

        //[StringLength(255)]
        //public string N_UCAPAN { get; set; }

        //[StringLength(10)]
        //public string PEMESAN { get; set; }

        //[Required]
        //[StringLength(30)]
        //public string NAMAPEMESAN { get; set; }

        //public double? KOMISI { get; set; }

        //public double? N_KOMISI { get; set; }

        //public double? N_KOMISI1 { get; set; }

        //[Required]
        //[StringLength(10)]
        //public string EXPEDISI { get; set; }

        //public double? TIPE_KIRIM { get; set; }

        //public double? TOTAL_TITIPAN { get; set; }

        //[StringLength(20)]
        //public string SUPP { get; set; }

        //[StringLength(50)]
        //public string STATUS_TRANSAKSI { get; set; }

        //[Column(TypeName = "text")]
        //public string ALAMAT_KIRIM { get; set; }

        //[StringLength(50)]
        //public string PROPINSI { get; set; }

        //[StringLength(50)]
        //public string KOTA { get; set; }

        //[StringLength(50)]
        //public string KODE_POS { get; set; }

        //[StringLength(50)]
        //public string SHIPMENT { get; set; }

        //[StringLength(50)]
        //public string TRACKING_SHIPMENT { get; set; }

        //public double TOTAL_SEMUA { get; set; }

        //public double ONGKOS_KIRIM { get; set; }

        //[Required]
        //public DateTime? TGL_JTH_TEMPO { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int? RecNum { get; set; }

        //[StringLength(70)]
        //public string NO_REFERENSI { get; set; }

        //[JsonIgnore]
        //public virtual SOT01D SOT01D { get; set; }

    }
}
