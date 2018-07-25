namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT01G
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
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NO_URUT_PP { get; set; }

        [Required]
        [StringLength(10)]
        public string POP_NO { get; set; }

        public DateTime? POP_DATE { get; set; }

        [Required]
        [StringLength(10)]
        public string DIVISION { get; set; }

        public double PO_QUANTITY { get; set; }

        public double APPROVE_QTY { get; set; }

        public byte? UNIT_CODE { get; set; }

        [Required]
        [StringLength(4)]
        public string GD { get; set; }

        [Required]
        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
