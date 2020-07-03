using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.DynamicData;

namespace MasterOnline.Models
{
    [Table("TEMP_UPLOADPESANAN")]
    public class TEMP_UPLOADPESANAN
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        [StringLength(70)]
        public string NO_REFERENSI { get; set; }

        public DateTime? TGL_PESANAN { get; set; }

        [StringLength(20)]
        public string MARKETPLACE { get; set; }

        [StringLength(100)]
        public string NAMA_PEMBELI { get; set; }

        [Column(TypeName = "text")]
        public string ALAMAT_KIRIM { get; set; }

        [StringLength(50)]
        public string KODE_KURIR { get; set; }

        [StringLength(50)]
        public string STATUS_PESANAN { get; set; }

        [StringLength(20)]
        public string KODE_BRG { get; set; }

        [StringLength(100)]
        public string NAMA_BRG { get; set; }
        
        public DateTime? TGL_JATUH_TEMPO { get; set; }

        [StringLength(200)]
        public string KETERANGAN { get; set; }

        public double TOP { get; set; }
        public double BRUTO { get; set; }
        public double? DISKON { get; set; }
        public double? PPN { get; set; }
        public double? NILAI_PPN { get; set; }
        public double ONGKIR { get; set; }
        public double? NETTO { get; set; }
        public double QTY { get; set; }
        public double HARGA_SATUAN { get; set; }
        public double? DISC1 { get; set; }
        public double? NDISC1 { get; set; }
        public double? DISC2 { get; set; }
        public double? NDISC2 { get; set; }
        public double? TOTAL { get; set; }
    }
}