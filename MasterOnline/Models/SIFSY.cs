namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SIFSYS")]
    public partial class SIFSY
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short BLN { get; set; }

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

        public double PPN { get; set; }

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

        [StringLength(40)]
        public string NAMA_PT { get; set; }

        [StringLength(255)]
        public string ALAMAT_PT { get; set; }

        [StringLength(20)]
        public string NPWP { get; set; }

        [StringLength(20)]
        public string SK { get; set; }

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

        [StringLength(3)]
        public string JT_RETUR { get; set; }

        [StringLength(1)]
        public string H_JUAL_TERENDAH { get; set; }

        [StringLength(1)]
        public string KONFIGURASI_BRG { get; set; }

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

        [StringLength(30)]
        public string USERNAME { get; set; }

        [StringLength(1)]
        public string METODA_LINK_CC { get; set; }

        [Required]
        [StringLength(1)]
        public string VALIDASI_U_MUKA { get; set; }

        [Required]
        [StringLength(1)]
        public string KODE_BRG_STYLE { get; set; }

        public int FAKTUR_OUTLET { get; set; }

        [StringLength(20)]
        public string DB { get; set; }

        [StringLength(2)]
        public string JTRAN_RETUR { get; set; }

        [StringLength(2)]
        public string JTRAN_RETUR_KONSINYASI { get; set; }

        [StringLength(1)]
        public string KODE_VALUTA { get; set; }

        [StringLength(1)]
        public string PROTEK_SO { get; set; }

        public double Toleransi_Hari_KreditLimit { get; set; }

        public bool EDIT_TOP { get; set; }

        [Required]
        [StringLength(1)]
        public string GUDANG_SALESMAN { get; set; }

        [Required]
        [StringLength(1)]
        public string BARANG_SAMA { get; set; }

        [Required]
        [StringLength(1)]
        public string EDIT_BONUS { get; set; }

        [Required]
        [StringLength(1)]
        public string VALIDASI_BRG_FAKTUR { get; set; }

        [Required]
        [StringLength(1)]
        public string METODA_DISCOUNT { get; set; }

        public bool APPROVAL { get; set; }

        public bool APPROVAL_HJUAL { get; set; }

        public int? HARSAT_SESUAI_STN { get; set; }

        public int? INPUT_SATUAN { get; set; }

        public int TITIPAN { get; set; }

        public int? PROTEK_OVER_DUEDATE { get; set; }

        public bool EDIT_BRG_FAKTUR { get; set; }

        [StringLength(20)]
        public string USERNAME_POSTING { get; set; }

        public int? JURNAL_SALES { get; set; }

        public int? TIPE_TOLERANSI { get; set; }

        public int? INQUERY_STOCK { get; set; }

        [StringLength(1)]
        public string TERBILANG { get; set; }

        public int? DIGIT_TERBILANG { get; set; }

        [StringLength(1)]
        public string MFDesimal { get; set; }

        [StringLength(1)]
        public string QFDesimal { get; set; }

        [StringLength(100)]
        public string DB_PATH { get; set; }

        [StringLength(1)]
        public string DIRECT_SALES { get; set; }

        [StringLength(30)]
        public string USER_NAME { get; set; }

        [StringLength(50)]
        public string BCA_API_KEY { get; set; }

        [StringLength(50)]
        public string BCA_API_SECRET { get; set; }

        [StringLength(50)]
        public string BCA_CLIENT_ID { get; set; }

        [StringLength(50)]
        public string BCA_CLIENT_SECRET { get; set; }

        [StringLength(50)]
        public string CORPORATE_ID { get; set; }

        [StringLength(50)]
        public string REKBCA { get; set; }

        [StringLength(50)]
        public string NAMAREKBCA { get; set; }
    }
}
