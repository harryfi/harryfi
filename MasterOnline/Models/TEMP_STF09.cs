namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TEMP_STF09
    {
        [StringLength(20)]
        public string Brg { get; set; }

        [Key]
        [Column(Order = 0)]
        public DateTime Tgl { get; set; }

        [StringLength(10)]
        public string Bukti { get; set; }

        [StringLength(1)]
        public string MK { get; set; }

        [StringLength(30)]
        public string Ket { get; set; }

        [Key]
        [Column(Order = 1)]
        public double Qty { get; set; }

        [Key]
        [Column(Order = 2)]
        public double HPokok { get; set; }

        [Key]
        [Column(Order = 3)]
        public double HJual { get; set; }

        [StringLength(4)]
        public string GD { get; set; }

        [StringLength(10)]
        public string Ref { get; set; }

        [StringLength(2)]
        public string JTran { get; set; }

        [StringLength(10)]
        public string WO { get; set; }

        [Key]
        [Column(Order = 4)]
        public double Qty_Berat { get; set; }

        [StringLength(15)]
        public string No_Faktur { get; set; }

        [Key]
        [Column(Order = 5)]
        public int recnum { get; set; }
    }
}
