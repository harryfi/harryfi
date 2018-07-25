namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PBT02
    {
        [Key]
        [StringLength(25)]
        public string PIB { get; set; }

        public DateTime TGL { get; set; }

        [Required]
        [StringLength(30)]
        public string NMPKB { get; set; }

        [Required]
        [StringLength(20)]
        public string NPWP { get; set; }

        public double PPNVALUE { get; set; }

        [Required]
        [StringLength(10)]
        public string INV { get; set; }

        public double BLN_LAPOR { get; set; }

        public double THN_LAPOR { get; set; }

        public DateTime? Tgl_SSP { get; set; }

        public double PPN_Yang_Dibebaskan { get; set; }

        public double DPP { get; set; }

        public double PPnBM { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
