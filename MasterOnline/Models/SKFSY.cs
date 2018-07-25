namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SKFSYS")]
    public partial class SKFSY
    {
        [StringLength(1)]
        public string Satuan { get; set; }

        [Key]
        [Column(Order = 0)]
        public bool EDIT_H_Satuan { get; set; }

        [StringLength(10)]
        public string DEFAULT_ENTRY { get; set; }

        [StringLength(3)]
        public string KODE_INVOICE { get; set; }

        [StringLength(1)]
        public string TRANSAKSI_ENTRY { get; set; }

        [Key]
        [Column(Order = 1)]
        public bool ISI_KODE_BRG_BARU { get; set; }

        [StringLength(2)]
        public string JENIS_TRANS_MASUK { get; set; }

        [StringLength(2)]
        public string JENIS_TRANS_KELUAR { get; set; }

        [StringLength(3)]
        public string KD_SERI_MASUK { get; set; }

        [StringLength(3)]
        public string KD_SERI_KELUAR { get; set; }

        [StringLength(1)]
        public string METODA_PROSES_OB { get; set; }

        [StringLength(1)]
        public string HARGA_JUAL_ENTRY { get; set; }

        [StringLength(1)]
        public string Discount_Entry { get; set; }

        [Key]
        [Column(Order = 2)]
        public byte JML_SORT_OB { get; set; }

        [Key]
        [Column(Order = 3)]
        public bool CONVERSI_BRG { get; set; }

        [StringLength(1)]
        public string DISCOUNT_TABLE { get; set; }

        [StringLength(1)]
        public string PROTEK_QOH { get; set; }

        [Key]
        [Column(Order = 4)]
        public byte SORT_FIELD_SIZE { get; set; }

        [StringLength(1)]
        public string METODA_NO { get; set; }

        [StringLength(2)]
        public string NS_SK { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(1)]
        public string DEFAULT_HRG_SATUAN { get; set; }
    }
}
