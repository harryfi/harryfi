namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF02A1
    {
        [Required]
        [StringLength(20)]
        public string BRG1 { get; set; }

        public bool CHK1 { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG2 { get; set; }

        public bool CHK2 { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG3 { get; set; }

        public bool CHK3 { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG4 { get; set; }

        public bool CHK4 { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG5 { get; set; }

        public bool CHK5 { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG6 { get; set; }

        public bool CHK6 { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG7 { get; set; }

        public bool CHK7 { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG8 { get; set; }

        public bool CHK8 { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG9 { get; set; }

        public bool CHK9 { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG10 { get; set; }

        public bool CHK10 { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG11 { get; set; }

        public bool CHK11 { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG12 { get; set; }

        public bool CHK12 { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG13 { get; set; }

        public bool CHK13 { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG14 { get; set; }

        public bool CHK14 { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG15 { get; set; }

        public bool CHK15 { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(30)]
        public string NAMA2 { get; set; }

        [StringLength(30)]
        public string NAMA3 { get; set; }

        [Required]
        [StringLength(1)]
        public string JENIS_SIZE { get; set; }

        [StringLength(10)]
        public string JENIS { get; set; }

        [StringLength(5)]
        public string STN { get; set; }

        public double HBELI { get; set; }

        public double MAXI { get; set; }

        public double MINI { get; set; }

        [StringLength(5)]
        public string STN2 { get; set; }

        public double ISI { get; set; }

        public double HPP { get; set; }

        public double HBESAR { get; set; }

        public double HKECIL { get; set; }

        [StringLength(1)]
        public string MVC { get; set; }

        [StringLength(10)]
        public string LKS { get; set; }

        [StringLength(10)]
        public string SUP { get; set; }

        [StringLength(10)]
        public string MEREK { get; set; }

        [Required]
        [StringLength(20)]
        public string PART1 { get; set; }

        [Required]
        [StringLength(20)]
        public string PART2 { get; set; }

        [Required]
        [StringLength(20)]
        public string PART3 { get; set; }

        [Required]
        [StringLength(20)]
        public string PART4 { get; set; }

        [Required]
        [StringLength(20)]
        public string PART5 { get; set; }

        [Required]
        [StringLength(20)]
        public string PART6 { get; set; }

        [Required]
        [StringLength(20)]
        public string PART7 { get; set; }

        [Required]
        [StringLength(20)]
        public string PART8 { get; set; }

        [Required]
        [StringLength(20)]
        public string PART9 { get; set; }

        [Required]
        [StringLength(20)]
        public string PART10 { get; set; }

        [Required]
        [StringLength(20)]
        public string PART11 { get; set; }

        [Required]
        [StringLength(20)]
        public string PART12 { get; set; }

        [Required]
        [StringLength(20)]
        public string PART13 { get; set; }

        [Required]
        [StringLength(20)]
        public string PART14 { get; set; }

        [Required]
        [StringLength(20)]
        public string PART15 { get; set; }

        [StringLength(1)]
        public string TYPE { get; set; }

        [StringLength(15)]
        public string KLINK { get; set; }

        public double HP_STD { get; set; }

        public double QPROD { get; set; }

        public DateTime Tgl_Input { get; set; }

        public DateTime TGL_KLR { get; set; }

        [StringLength(15)]
        public string WARNA { get; set; }

        public short LT { get; set; }

        public short OC { get; set; }

        public short SS { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string Sort1 { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string Sort2 { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string Sort3 { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(10)]
        public string Sort4 { get; set; }

        [StringLength(1)]
        public string Stn_berat { get; set; }

        [StringLength(1)]
        public string Metoda { get; set; }

        [StringLength(1)]
        public string Qty_berat { get; set; }

        public double QSALES { get; set; }

        [StringLength(40)]
        public string KET_SORT1 { get; set; }

        [StringLength(40)]
        public string KET_SORT2 { get; set; }

        [StringLength(40)]
        public string KET_SORT3 { get; set; }

        [StringLength(40)]
        public string KET_SORT4 { get; set; }

        [StringLength(5)]
        public string STN3 { get; set; }

        [StringLength(5)]
        public string STN4 { get; set; }

        [StringLength(20)]
        public string KET_STN2 { get; set; }

        [StringLength(20)]
        public string KET_STN3 { get; set; }

        [StringLength(20)]
        public string KET_STN4 { get; set; }

        public double ISI3 { get; set; }

        public double ISI4 { get; set; }

        [StringLength(20)]
        public string KET_STN { get; set; }

        public double TOLERANSI { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Column(TypeName = "image")]
        public byte[] Photo { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT5 { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT6 { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT7 { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT8 { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT9 { get; set; }

        [Required]
        [StringLength(10)]
        public string SORT10 { get; set; }

        [Required]
        [StringLength(40)]
        public string KET_SORT5 { get; set; }

        [Required]
        [StringLength(40)]
        public string KET_SORT6 { get; set; }

        [Required]
        [StringLength(40)]
        public string KET_SORT7 { get; set; }

        [Required]
        [StringLength(40)]
        public string KET_SORT8 { get; set; }

        [Required]
        [StringLength(40)]
        public string KET_SORT9 { get; set; }

        [Required]
        [StringLength(40)]
        public string KET_SORT10 { get; set; }
    }
}
