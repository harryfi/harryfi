namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT02B
    {
        [Key]
        [Column(Order = 0)]
        public byte JENIS_FORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string NOBUK { get; set; }

        [StringLength(20)]
        public string KOBAR { get; set; }

        [Key]
        [Column(Order = 2)]
        public int NO_URUT { get; set; }

        [StringLength(1)]
        public string SATUAN { get; set; }

        public double QTY { get; set; }

        public double QTY_AMBIL { get; set; }

        [StringLength(15)]
        public string NO_WO { get; set; }

        [Column(TypeName = "text")]
        public string CATATAN { get; set; }

        [StringLength(4)]
        public string DR_GD { get; set; }

        [StringLength(4)]
        public string KE_GD { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        public virtual STT02A STT02A { get; set; }
    }
}
