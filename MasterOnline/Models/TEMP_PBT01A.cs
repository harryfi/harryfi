namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TEMP_PBT01A
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENISFORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string INV { get; set; }

        public DateTime? TGL { get; set; }

        [StringLength(1)]
        public string JENIS { get; set; }

        [StringLength(10)]
        public string PO { get; set; }

        [Key]
        [Column(Order = 2)]
        public double DISC1 { get; set; }

        [Key]
        [Column(Order = 3)]
        public double NDISC1 { get; set; }

        [Key]
        [Column(Order = 4)]
        public double PPN { get; set; }

        [Key]
        [Column(Order = 5)]
        public double NPPN { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(1)]
        public string STATUS { get; set; }

        [Key]
        [Column(Order = 7)]
        [StringLength(1)]
        public string POSTING { get; set; }

        [Key]
        [Column(Order = 8)]
        [StringLength(1)]
        public string APP { get; set; }

        [Key]
        [Column(Order = 9)]
        [StringLength(10)]
        public string SUPP { get; set; }

        [Key]
        [Column(Order = 10)]
        [StringLength(40)]
        public string NAMA { get; set; }

        [Key]
        [Column(Order = 11)]
        [StringLength(5)]
        public string VLT { get; set; }

        [Key]
        [Column(Order = 12)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short TERM { get; set; }

        [Key]
        [Column(Order = 13)]
        [StringLength(25)]
        public string F_PAJAK { get; set; }

        public DateTime? T_PAJAK { get; set; }

        [Key]
        [Column(Order = 14)]
        public double TUKAR { get; set; }

        [Key]
        [Column(Order = 15)]
        public double BRUTO { get; set; }

        [Key]
        [Column(Order = 16)]
        public double BIAYA_LAIN { get; set; }

        [Key]
        [Column(Order = 17)]
        public double NETTO { get; set; }

        [Key]
        [Column(Order = 18)]
        [StringLength(10)]
        public string REF { get; set; }

        [Column(TypeName = "text")]
        public string KET { get; set; }

        [Key]
        [Column(Order = 19)]
        public bool RETUR_PENUH { get; set; }

        [Key]
        [Column(Order = 20)]
        [StringLength(20)]
        public string USERNAME { get; set; }

        public DateTime? TGLINPUT { get; set; }

        [Key]
        [Column(Order = 21)]
        public bool PPN_ditangguhkan { get; set; }

        [Key]
        [Column(Order = 22)]
        public byte PPN_Bln_Lapor { get; set; }

        [Key]
        [Column(Order = 23)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short PPN_Thn_Lapor { get; set; }

        [Key]
        [Column(Order = 24)]
        public double UANG_MUKA { get; set; }

        public DateTime? TGJT { get; set; }

        [Key]
        [Column(Order = 25)]
        public double TUKAR_PPN { get; set; }

        [Key]
        [Column(Order = 26)]
        public double NILAI_PPN { get; set; }

        [Key]
        [Column(Order = 27)]
        [StringLength(25)]
        public string NO_INVOICE_SUPP { get; set; }

        [Key]
        [Column(Order = 28)]
        public double PPNBM { get; set; }

        [Key]
        [Column(Order = 29)]
        public double TYPE_BELI { get; set; }

        [Key]
        [Column(Order = 30)]
        public double PPH_22 { get; set; }

        [Key]
        [Column(Order = 31)]
        public double NPPH_22 { get; set; }

        [Key]
        [Column(Order = 32)]
        public double U_MUKA { get; set; }

        [Key]
        [Column(Order = 33)]
        public double U_MUKA_INV { get; set; }

        [StringLength(15)]
        public string WO_SUBCON { get; set; }
    }
}
