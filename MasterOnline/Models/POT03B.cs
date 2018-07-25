namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT03B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string MATRIX { get; set; }

        [Key]
        [Column(Order = 1)]
        public int NO_URUT { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(20)]
        public string BRG { get; set; }

        [StringLength(80)]
        public string NAMA { get; set; }

        public double QTY { get; set; }

        [StringLength(1)]
        public string BK { get; set; }

        [StringLength(5)]
        public string STN { get; set; }

        public double H_STN1 { get; set; }

        public double DISC1 { get; set; }

        public double NDISC1 { get; set; }

        public double BIAYA_LAIN1 { get; set; }

        public double H_STN2 { get; set; }

        public double DISC2 { get; set; }

        public double NDISC2 { get; set; }

        public double BIAYA_LAIN2 { get; set; }

        public double H_STN3 { get; set; }

        public double DISC3 { get; set; }

        public double NDISC3 { get; set; }

        public double BIAYA_LAIN3 { get; set; }

        public double T_BIAYA1 { get; set; }

        public double T_BIAYA2 { get; set; }

        public double T_BIAYA3 { get; set; }

        public double T_HARGA1 { get; set; }

        public double T_HARGA2 { get; set; }

        public double T_HARGA3 { get; set; }

        public double HARGA1 { get; set; }

        public double HARGA2 { get; set; }

        public double HARGA3 { get; set; }

        [StringLength(10)]
        public string SUPP4 { get; set; }

        public double H_STN4 { get; set; }

        public double DISC4 { get; set; }

        public double T_HARGA4 { get; set; }

        [StringLength(80)]
        public string NAMA2 { get; set; }

        [StringLength(80)]
        public string NAMA3 { get; set; }

        [StringLength(255)]
        public string CATATAN1 { get; set; }

        [StringLength(255)]
        public string CATATAN2 { get; set; }

        [StringLength(255)]
        public string CATATAN3 { get; set; }

        [StringLength(80)]
        public string NAMA4 { get; set; }

        [StringLength(80)]
        public string NAMA5 { get; set; }

        [StringLength(80)]
        public string NAMA6 { get; set; }

        [StringLength(255)]
        public string CATATAN4 { get; set; }

        [StringLength(255)]
        public string CATATAN5 { get; set; }

        [StringLength(255)]
        public string CATATAN6 { get; set; }

        public double H_STN5 { get; set; }

        public double H_STN6 { get; set; }

        public double H_STN7 { get; set; }

        public double DISC5 { get; set; }

        public double DISC6 { get; set; }

        public double DISC7 { get; set; }

        public double NDISC4 { get; set; }

        public double NDISC5 { get; set; }

        public double NDISC6 { get; set; }

        public double BIAYA_LAIN4 { get; set; }

        public double BIAYA_LAIN5 { get; set; }

        public double BIAYA_LAIN6 { get; set; }

        public double T_BIAYA4 { get; set; }

        public double T_BIAYA5 { get; set; }

        public double T_BIAYA6 { get; set; }

        public double T_HARGA5 { get; set; }

        public double T_HARGA6 { get; set; }

        public double T_HARGA7 { get; set; }

        public double HARGA4 { get; set; }

        public double HARGA5 { get; set; }

        public double HARGA6 { get; set; }

        public double PPNBM1 { get; set; }

        public double PPNBM2 { get; set; }

        public double PPNBM3 { get; set; }

        public double PPNBM4 { get; set; }

        public double PPNBM5 { get; set; }

        public double PPNBM6 { get; set; }

        public double NPPNBM1 { get; set; }

        public double NPPNBM2 { get; set; }

        public double NPPNBM3 { get; set; }

        public double NPPNBM4 { get; set; }

        public double NPPNBM5 { get; set; }

        public double NPPNBM6 { get; set; }

        public DateTime? TGL_H_LAMA { get; set; }

        public double? Harsat_Lama { get; set; }

        public double? Persen_Plus_Minus { get; set; }

        public DateTime? TGL_H_LAMA2 { get; set; }

        public double? Harsat_Lama2 { get; set; }

        public double? Persen_Plus_Minus2 { get; set; }

        public DateTime? TGL_H_LAMA3 { get; set; }

        public double? Harsat_Lama3 { get; set; }

        public double? Persen_Plus_Minus3 { get; set; }

        public DateTime? TGL_H_LAMA4 { get; set; }

        public double? Harsat_Lama4 { get; set; }

        public double? Persen_Plus_Minus4 { get; set; }

        public DateTime? TGL_H_LAMA5 { get; set; }

        public double? Harsat_Lama5 { get; set; }

        public double? Persen_Plus_Minus5 { get; set; }

        public DateTime? TGL_H_LAMA6 { get; set; }

        public double? Harsat_Lama6 { get; set; }

        public double? Persen_Plus_Minus6 { get; set; }

        public double? Qty_PO1 { get; set; }

        public DateTime? Tgl_PO_Terakhir1 { get; set; }

        public double? Qty_PO2 { get; set; }

        public DateTime? Tgl_PO_Terakhir2 { get; set; }

        public double? Qty_PO3 { get; set; }

        public DateTime? Tgl_PO_Terakhir3 { get; set; }

        public double? Qty_PO4 { get; set; }

        public DateTime? Tgl_PO_Terakhir4 { get; set; }

        public double? Qty_PO5 { get; set; }

        public DateTime? Tgl_PO_Terakhir5 { get; set; }

        public double? Qty_PO6 { get; set; }

        public DateTime? Tgl_PO_Terakhir6 { get; set; }

        public double? Qty_Approve1 { get; set; }

        public double? Qty_Approve2 { get; set; }

        public double? Qty_Approve3 { get; set; }

        public double? Qty_Approve4 { get; set; }

        public double? Qty_Approve5 { get; set; }

        public double? Qty_Approve6 { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
