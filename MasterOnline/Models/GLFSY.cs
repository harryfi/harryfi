namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFSYS")]
    public partial class GLFSY
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

        public short EditNilaiTukar { get; set; }

        public short Metode_UpdateRevaluasi { get; set; }

        [StringLength(12)]
        public string NoSeri_UpdateRevaluasi { get; set; }

        public int NoAkhir_UpdateRevaluasi { get; set; }

        public short ValidasiRekLawandiTransaksi { get; set; }

        public short ValidasiDeptdiTransaksi_Neraca { get; set; }

        public short ValidasiCCdiTransaksi_Neraca { get; set; }

        public short ValidasiDeptdiTransaksi_Pendapatan { get; set; }

        public short ValidasiCCdiTransaksi_Pendapatan { get; set; }

        public short ValidasiDeptdiTransaksi_Biaya { get; set; }

        public short ValidasiCCdiTransaksi_Biaya { get; set; }

        public short FORMAT_DEC_DETAIL { get; set; }

        public short FORMAT_DEC_CURRENCY { get; set; }

        public short FORMAT_DEC_BUDGET { get; set; }

        public short FORMAT_DEC_BALANCE { get; set; }

        public short FORMAT_DEC_GRAND { get; set; }

        public short FORMAT_DEC_GRID { get; set; }

        public bool SHOW_KDREK_DILAPORAN { get; set; }

        public bool SHOW_NAMAPERUSAHAAN_DILAPORAN { get; set; }

        public bool SHOW_ICON_PROMPT { get; set; }

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

        public int? KD_LEVEL_REK { get; set; }

        public int? KD_LEVEL_DEPT { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
