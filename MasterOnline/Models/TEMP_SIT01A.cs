namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TEMP_SIT01A
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENIS_FORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(15)]
        public string NO_BUKTI { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime TGL { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        [StringLength(1)]
        public string ST_POSTING { get; set; }

        public DateTime? TGL_KIRIM { get; set; }

        [StringLength(15)]
        public string NO_REF { get; set; }

        [StringLength(15)]
        public string NO_SO { get; set; }

        [StringLength(10)]
        public string CUST { get; set; }

        [StringLength(40)]
        public string NAMA_CUST { get; set; }

        [StringLength(5)]
        public string KODE_ALAMAT { get; set; }

        [StringLength(20)]
        public string NO_KENDARAAN { get; set; }

        [StringLength(20)]
        public string SOPIR { get; set; }

        [Column(TypeName = "text")]
        public string KET { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        public double? TUKAR { get; set; }

        public double? TUKAR_PPN { get; set; }

        [StringLength(5)]
        public string KODE_SALES { get; set; }

        [StringLength(5)]
        public string KODE_WIL { get; set; }

        [StringLength(25)]
        public string NO_F_PAJAK { get; set; }

        public DateTime? TGL_F_PAJAK { get; set; }

        [StringLength(10)]
        public string KODE_PROYEK { get; set; }

        public double? BRUTO { get; set; }

        public double? DISCOUNT { get; set; }

        public double? NILAI_DISC { get; set; }

        public double? PPN { get; set; }

        public double? NILAI_PPN { get; set; }

        public double? PPNBM { get; set; }

        public double? NILAI_PPNBM { get; set; }

        public double? MATERAI { get; set; }

        public double? NETTO { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        [Key]
        [Column(Order = 3)]
        public DateTime TGLINPUT { get; set; }

        [StringLength(15)]
        public string NO_MK { get; set; }

        [StringLength(1)]
        public string JENIS_RETUR { get; set; }

        public short? PRINT_COUNT { get; set; }

        [Key]
        [Column(Order = 4)]
        public bool RETUR_PENUH { get; set; }

        [StringLength(3)]
        public string JTRAN { get; set; }

        [StringLength(40)]
        public string AL3 { get; set; }

        [StringLength(40)]
        public string AL2 { get; set; }

        [StringLength(40)]
        public string AL1 { get; set; }

        [StringLength(40)]
        public string AL { get; set; }

        public double? U_MUKA { get; set; }

        public double? TERM { get; set; }

        [Key]
        [Column(Order = 5)]
        public bool PPN_ditangguhkan { get; set; }

        public byte? PPN_Bln_Lapor { get; set; }

        public short? PPN_Thn_Lapor { get; set; }

        [StringLength(1)]
        public string JENIS { get; set; }

        [StringLength(10)]
        public string CUST_QQ { get; set; }

        [StringLength(40)]
        public string NAMA_CUST_QQ { get; set; }

        public DateTime? TGL_JT_TEMPO { get; set; }

        [Key]
        [Column(Order = 6)]
        public bool KIRIM_PENUH { get; set; }

        [StringLength(10)]
        public string NO_FAKTUR_PPN_AR { get; set; }

        [Key]
        [Column(Order = 7)]
        public double U_MUKA_FA { get; set; }

        [StringLength(10)]
        public string NO_FAKTUR_LAMA { get; set; }

        [Key]
        [Column(Order = 8)]
        public bool BATAL { get; set; }

        [Key]
        [Column(Order = 9)]
        public bool SJ_ADA_FAKTUR { get; set; }

        [StringLength(50)]
        public string STATUS_LOADING { get; set; }

        [Key]
        [Column(Order = 10)]
        [StringLength(10)]
        public string NO_FA_OUTLET { get; set; }

        [StringLength(30)]
        public string NO_PO_CUST { get; set; }
    }
}
