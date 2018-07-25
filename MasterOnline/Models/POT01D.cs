namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT01D
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string PO_NO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NO_URUT_UNIT { get; set; }

        [Key]
        [Column(Order = 2)]
        public int NO_URUT { get; set; }

        public DateTime TGL_TERIMA { get; set; }

        public double QTY { get; set; }

        [StringLength(4)]
        public string GUD { get; set; }

        [Column(TypeName = "text")]
        public string KET { get; set; }

        public int UNIT_CODE { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        public virtual POT01B POT01B { get; set; }
    }
}
