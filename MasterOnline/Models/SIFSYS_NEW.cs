namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIFSYS_NEW
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short BLN { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short THN { get; set; }

        [StringLength(1)]
        public string SATUAN { get; set; }

        [StringLength(4)]
        public string GUDANG { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        [StringLength(1)]
        public string METODA_SJ { get; set; }

        [StringLength(1)]
        public string METODA_NO { get; set; }

        [StringLength(1)]
        public string JENIS_SJ { get; set; }

        [StringLength(1)]
        public string PROTEK_QOH { get; set; }

        [StringLength(1)]
        public string PROTEK_LIMIT { get; set; }

        [StringLength(1)]
        public string PROTEK_HARGA { get; set; }

        [Key]
        [Column(Order = 2)]
        public double PPN { get; set; }

        [Key]
        [Column(Order = 3)]
        public double PPNBM { get; set; }

        [StringLength(2)]
        public string NS_SJ { get; set; }

        [StringLength(2)]
        public string NS_FA { get; set; }

        [StringLength(2)]
        public string NS_RT { get; set; }

        [StringLength(10)]
        public string NS_FP { get; set; }

        [StringLength(1)]
        public string METODA_SO { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(50)]
        public string LINK_SO { get; set; }

        [StringLength(50)]
        public string LINK_AR { get; set; }

        [StringLength(50)]
        public string LINK_ST { get; set; }

        [StringLength(1)]
        public string BHS { get; set; }

        [StringLength(30)]
        public string DBASE { get; set; }

        [StringLength(30)]
        public string DSN { get; set; }

        [StringLength(50)]
        public string USER { get; set; }

        [StringLength(30)]
        public string PSW { get; set; }

        [StringLength(30)]
        public string DRIVER { get; set; }

        [StringLength(1)]
        public string METODA { get; set; }

        [StringLength(3)]
        public string JT_RETUR { get; set; }

        [StringLength(40)]
        public string NAMA_PT { get; set; }

        [Column(TypeName = "ntext")]
        public string ALAMAT_PT { get; set; }

        [StringLength(20)]
        public string NPWP { get; set; }

        [StringLength(20)]
        public string SK { get; set; }

        [Key]
        [Column(Order = 4)]
        public DateTime TGL_SK { get; set; }

        [StringLength(1)]
        public string JENIS_PRINT { get; set; }

        [StringLength(1)]
        public string POSTING_STATUS { get; set; }

        [StringLength(1)]
        public string LINK_GL { get; set; }

        [StringLength(3)]
        public string JT_SJ { get; set; }

        [StringLength(3)]
        public string JT_FAKTUR { get; set; }

        [StringLength(1)]
        public string KONFIGURASI_BRG { get; set; }

        [StringLength(1)]
        public string H_JUAL_TERENDAH { get; set; }

        [StringLength(1)]
        public string METODA_LINK { get; set; }

        [StringLength(2)]
        public string NS_Faktur_PPN { get; set; }

        [StringLength(1)]
        public string TINGKAT_DISC { get; set; }

        [StringLength(1)]
        public string METODA_NDISC { get; set; }

        [StringLength(1)]
        public string TERBILANG_IN { get; set; }

        [StringLength(1)]
        public string METODA_HJUAL { get; set; }

        [StringLength(1)]
        public string UPDATE_QOH_SJ { get; set; }

        [StringLength(1)]
        public string TOTAL_DISC_per { get; set; }

        [StringLength(1)]
        public string EDIT_DISC_perItem { get; set; }

        [StringLength(1)]
        public string EDIT_DISC_perFaktur { get; set; }

        [StringLength(1)]
        public string DEFAULT_TBL_HJUAL { get; set; }

        public DateTime? TGLINPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        [StringLength(1)]
        public string METODA_LINK_CC { get; set; }
    }
}
