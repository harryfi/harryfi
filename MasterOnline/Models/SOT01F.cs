namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SOT01F
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(15)]
        public string No_Bukti { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int No_Urut { get; set; }

        [Key]
        [Column(Order = 2)]
        public int Urut { get; set; }

        [StringLength(20)]
        public string Brg { get; set; }

        public DateTime Tgl_Cancel { get; set; }

        public double? Qty { get; set; }

        [StringLength(1000)]
        public string Ket { get; set; }

        [StringLength(50)]
        public string USERNAME { get; set; }
    }
}
