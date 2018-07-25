namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF16C
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(3)]
        public string KDHARGA { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime TGL1 { get; set; }

        [Key]
        [Column(Order = 3)]
        public DateTime TGL2 { get; set; }

        [StringLength(10)]
        public string Sort1 { get; set; }

        [StringLength(10)]
        public string Sort2 { get; set; }

        [StringLength(10)]
        public string Sort3 { get; set; }

        [StringLength(10)]
        public string Sort4 { get; set; }

        [StringLength(10)]
        public string Sort5 { get; set; }

        public double H_Besar { get; set; }

        public double H_Kecil { get; set; }
    }
}
