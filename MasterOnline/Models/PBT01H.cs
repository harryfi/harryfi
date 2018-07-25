namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PBT01H
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENISFORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string INV { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(20)]
        public string BRG { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NO_URUT { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(10)]
        public string PREFIXNO { get; set; }

        [Key]
        [Column(Order = 5)]
        public int RECNUM { get; set; }

        [StringLength(50)]
        public string AssetNm { get; set; }

        [StringLength(50)]
        public string SerialNo { get; set; }

        [StringLength(50)]
        public string AssetTp { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(50)]
        public string Branch { get; set; }

        [StringLength(50)]
        public string Manufacture { get; set; }

        [StringLength(50)]
        public string Model { get; set; }

        [StringLength(50)]
        public string GRP1 { get; set; }

        [StringLength(50)]
        public string GRP2 { get; set; }

        [StringLength(50)]
        public string GRP3 { get; set; }

        public int? QTY { get; set; }

        [StringLength(50)]
        public string Dept { get; set; }

        [StringLength(50)]
        public string Respon { get; set; }

        public int? RoomNo { get; set; }

        [StringLength(50)]
        public string PONo { get; set; }

        public DateTime? PODate { get; set; }

        [StringLength(50)]
        public string Vlt { get; set; }

        [StringLength(50)]
        public string Supplier { get; set; }

        [StringLength(50)]
        public string Person { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        [Column(TypeName = "ntext")]
        public string Address { get; set; }

        public short? Ages { get; set; }

        [Column(TypeName = "ntext")]
        public string Comment { get; set; }

        [Column(TypeName = "image")]
        public byte[] Photo { get; set; }

        public short? Life { get; set; }

        [StringLength(50)]
        public string Method { get; set; }

        public DateTime? StartDate { get; set; }

        public double? Percent { get; set; }

        public double? BeginVal { get; set; }

        public double? AdjustVal { get; set; }

        [StringLength(50)]
        public string AssetAccNo { get; set; }

        [StringLength(50)]
        public string DeprAccNo { get; set; }

        [StringLength(50)]
        public string CostAccNo { get; set; }

        public double? BookValue { get; set; }

        [StringLength(50)]
        public string HarmonyID { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        public byte[] upsize_ts { get; set; }

        [StringLength(1000)]
        public string Photo_Path { get; set; }

        [StringLength(10)]
        public string DEPT_GL { get; set; }

        [StringLength(10)]
        public string COST_GL { get; set; }

        public int? LKS_GL { get; set; }

        public double? ORIGIN { get; set; }

        [Column(TypeName = "text")]
        public string PHOTO2 { get; set; }

        [StringLength(50)]
        public string USERNAME { get; set; }
    }
}
