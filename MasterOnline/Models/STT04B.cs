namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT04B
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? No { get; set; }

        public string Gud { get; set; }

        [Required]
        [StringLength(10)]
        public string NOBUK { get; set; }

        [StringLength(10)]
        public string LKS { get; set; }


        [StringLength(20)]
        public string Brg { get; set; }

        public double Qty { get; set; }

        public DateTime Tgl { get; set; }

        public double HPokok { get; set; }

        [StringLength(1)]
        public string BK { get; set; }

        [StringLength(5)]
        public string Stn { get; set; }

        [StringLength(10)]
        public string WO { get; set; }

        [StringLength(30)]
        public string Nama_Barang { get; set; }

        public double Qty_Berat { get; set; }

        public double QTY_KECIL { get; set; }

        public double QTY_BESAR { get; set; }

        public double QTY_3 { get; set; }

        public double QTY_4 { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        public virtual STT04A STT04A { get; set; }
    }
}
