namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFTRAN1TEMP
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short lks { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(15)]
        public string bukti { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime tgl { get; set; }

        [StringLength(2)]
        public string jurnal { get; set; }

        public double tdebet { get; set; }

        public double tkredit { get; set; }

        [StringLength(75)]
        public string urai { get; set; }

        [StringLength(1)]
        public string posting { get; set; }

        [StringLength(10)]
        public string pcost { get; set; }

        public double tukar { get; set; }

        [StringLength(20)]
        public string userid { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime tgl_input { get; set; }
    }
}
