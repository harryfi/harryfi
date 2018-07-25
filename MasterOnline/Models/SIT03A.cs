namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIT03A
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string NO_BUKTI { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(5)]
        public string KD_KASIR { get; set; }

        [StringLength(10)]
        public string CUST { get; set; }

        [StringLength(30)]
        public string NAMA_CUST { get; set; }

        [StringLength(1)]
        public string STATUS_BAYAR { get; set; }

        public double? TUNAI { get; set; }

        public double? NILAI_CEK { get; set; }

        [StringLength(20)]
        public string NOMOR_CEK { get; set; }

        public DateTime? TGL_CEK { get; set; }

        [StringLength(20)]
        public string BANK_CEK { get; set; }

        public double? NILAI_DEBET { get; set; }

        [StringLength(20)]
        public string NOMOR_DEBET { get; set; }

        [StringLength(20)]
        public string BANK_DEBET { get; set; }

        public double? POTONGAN { get; set; }

        public double? PIUTANG { get; set; }

        public DateTime? tgl_transaksi { get; set; }

        [StringLength(1)]
        public string ST_POSTING { get; set; }

        [StringLength(15)]
        public string NoRetur { get; set; }

        public double? Nilai_Retur { get; set; }

        public int? STATUS_PRINT { get; set; }

        [StringLength(255)]
        public string Ket { get; set; }

        public double? MUKA { get; set; }

        public double? TOTAL_BAYAR { get; set; }

        public double? TOTAL_CARD { get; set; }

        public double? TOTAL_LUNAS { get; set; }

        [StringLength(5)]
        public string VLT { get; set; }

        public double? TUKAR { get; set; }

        public int? STATUS { get; set; }

        public double? KEMBALI { get; set; }

        public bool UANG_MUKA { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
