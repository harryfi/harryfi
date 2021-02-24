namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF03
    {
        [StringLength(20)]
        public string Unit { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? No { get; set; }

        [StringLength(20)]
        public string Brg { get; set; }

        public double Qty { get; set; }

        [StringLength(4)]
        public string GD { get; set; }

        public bool MEMORY { get; set; }

        public bool KOMPONEN_PECAH { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        //ADD BY NURUL 5/10/2020, FITUR BUNDLING
        public double? HARGA { get; set; }
        public double? TOTALHARGA { get; set; }
        public DateTime? TGL_EDIT { get; set; }
        public double QTY_SIAPJUAL { get; set; }
        public double QTY_KOMPONEN { get; set; }
        //END ADD BY NURUL 5/10/2020, FITUR BUNDLING 

    }
}
