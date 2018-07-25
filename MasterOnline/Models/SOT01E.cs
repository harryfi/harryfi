namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SOT01E
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(15)]
        public string NO_BUKTI { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NO_URUT_UNIT { get; set; }

        [Key]
        [Column(Order = 2)]
        public int NO_URUT { get; set; }

        public DateTime TGL_TERIMA { get; set; }

        public double? QTY { get; set; }

        [StringLength(4)]
        public string GUDANG { get; set; }

        [Column(TypeName = "text")]
        public string KET { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
