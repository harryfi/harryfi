namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFJTTAP")]
    public partial class GLFJTTAP
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LKS { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string BRG { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime TGL { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(1)]
        public string JENIS { get; set; }

        [StringLength(15)]
        public string REK1 { get; set; }

        [StringLength(15)]
        public string REK2 { get; set; }

        [StringLength(15)]
        public string REK3 { get; set; }

        [StringLength(15)]
        public string REK4 { get; set; }

        [StringLength(15)]
        public string REK5 { get; set; }

        public double NL1 { get; set; }

        public double NL2 { get; set; }

        public double NL3 { get; set; }

        public double NL4 { get; set; }

        public double NL5 { get; set; }
    }
}
