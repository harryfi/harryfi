namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIT01G
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENIS_FORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(15)]
        public string NO_BUKTI { get; set; }

        [Key]
        [Column(Order = 2)]
        public int NO_URUT { get; set; }

        [StringLength(15)]
        public string NO_FAKTUR { get; set; }

        public double NPOTONG { get; set; }

        public DateTime TGLINPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        public double NFAKTUR { get; set; }
    }
}
