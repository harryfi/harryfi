namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF09A
    {
        [StringLength(20)]
        public string Brg { get; set; }

        public DateTime Tgl { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string Bukti { get; set; }

        [StringLength(1)]
        public string MK { get; set; }

        [StringLength(30)]
        public string Ket { get; set; }

        public double Qty { get; set; }

        public double HPokok { get; set; }

        public double HJual { get; set; }

        [StringLength(4)]
        public string GD { get; set; }

        [StringLength(10)]
        public string Ref { get; set; }

        [StringLength(2)]
        public string JTran { get; set; }

        [StringLength(10)]
        public string WO { get; set; }

        public double Qty_Berat { get; set; }

        [StringLength(15)]
        public string No_Faktur { get; set; }

        [Key]
        [Column(Order = 1)]
        public int RECNUM { get; set; }

        [Key]
        [Column(Order = 2)]
        public double DetailNoUrut { get; set; }

        [StringLength(10)]
        public string Work_Center { get; set; }

        [StringLength(4)]
        public string DR_GD { get; set; }

        [StringLength(20)]
        public string BRG_UNIT { get; set; }
    }
}
