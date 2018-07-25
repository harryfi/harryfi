namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PBT03
    {
        [Key]
        public int NO_URUT { get; set; }

        [StringLength(10)]
        public string INV { get; set; }

        [StringLength(30)]
        public string NMPKB { get; set; }

        [StringLength(20)]
        public string NPWP { get; set; }

        public double? PPNVALUE { get; set; }

        public double? BLN_LAPOR { get; set; }

        public double? THN_LAPOR { get; set; }

        public DateTime? TGL_SSP { get; set; }

        public double? DPP { get; set; }

        public double? PPnBM { get; set; }
    }
}
