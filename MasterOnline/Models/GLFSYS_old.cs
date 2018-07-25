namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFSYS_old
    {
        [Key]
        public int AutoNumber { get; set; }

        [StringLength(40)]
        public string ID { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(30)]
        public string ALAMAT { get; set; }

        public byte? BL { get; set; }

        public short TH { get; set; }

        public short VALIDASI { get; set; }

        [StringLength(15)]
        public string RL { get; set; }

        [StringLength(2)]
        public string CASH { get; set; }

        [StringLength(2)]
        public string J_AKTIVA { get; set; }

        public byte? BLN_AKTIVA { get; set; }

        [StringLength(2)]
        public string TRM { get; set; }

        [StringLength(2)]
        public string KLR { get; set; }

        [StringLength(2)]
        public string J_UMUM { get; set; }

        [StringLength(1)]
        public string BB { get; set; }

        public short J_HAL { get; set; }

        [StringLength(1)]
        public string KODE { get; set; }

        [StringLength(2)]
        public string appl { get; set; }

        public byte? BHS { get; set; }

        [StringLength(5)]
        public string kurs { get; set; }

        [StringLength(1)]
        public string metoda { get; set; }

        public short P1 { get; set; }

        public short P2 { get; set; }

        public short P3 { get; set; }

        public short P4 { get; set; }

        public short P5 { get; set; }

        public short J1 { get; set; }

        public short J2 { get; set; }

        public short J3 { get; set; }

        public short J4 { get; set; }

        public short J5 { get; set; }

        [StringLength(15)]
        public string RekDiTahan { get; set; }

        [StringLength(12)]
        public string NoSeriLink { get; set; }

        public int NoAkhirLink { get; set; }

        public int EditNilaiTukar { get; set; }

        public short DP1 { get; set; }

        public short DP2 { get; set; }

        public short DP3 { get; set; }

        public short DP4 { get; set; }

        public short DP5 { get; set; }

        public short DJ1 { get; set; }

        public short DJ2 { get; set; }

        public short DJ3 { get; set; }

        public short DJ4 { get; set; }

        public short DJ5 { get; set; }

        public bool UseBatchNo { get; set; }

        public short UseGSTCode { get; set; }
    }
}
