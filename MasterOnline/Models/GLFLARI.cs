namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFLARIS")]
    public partial class GLFLARI
    {
        [Key]
        [StringLength(2)]
        public string KODE { get; set; }

        [StringLength(30)]
        public string KET { get; set; }

        [StringLength(10)]
        public string REK1 { get; set; }

        [StringLength(10)]
        public string REK2 { get; set; }

        [StringLength(10)]
        public string REK3 { get; set; }

        [StringLength(10)]
        public string REK4 { get; set; }

        [StringLength(10)]
        public string REK5 { get; set; }

        [StringLength(10)]
        public string REK6 { get; set; }

        [StringLength(10)]
        public string REK7 { get; set; }

        [StringLength(10)]
        public string REK8 { get; set; }

        [StringLength(10)]
        public string REK9 { get; set; }

        [StringLength(10)]
        public string REK10 { get; set; }

        [StringLength(15)]
        public string DR1 { get; set; }

        [StringLength(15)]
        public string DR2 { get; set; }

        [StringLength(15)]
        public string DR3 { get; set; }

        [StringLength(15)]
        public string DR4 { get; set; }

        [StringLength(15)]
        public string DR5 { get; set; }

        [StringLength(15)]
        public string DR6 { get; set; }

        [StringLength(15)]
        public string DR7 { get; set; }

        [StringLength(15)]
        public string DR8 { get; set; }

        [StringLength(15)]
        public string DR9 { get; set; }

        [StringLength(15)]
        public string DR10 { get; set; }

        [StringLength(15)]
        public string SD1 { get; set; }

        [StringLength(15)]
        public string SD2 { get; set; }

        [StringLength(15)]
        public string SD3 { get; set; }

        [StringLength(15)]
        public string SD4 { get; set; }

        [StringLength(15)]
        public string SD5 { get; set; }

        [StringLength(15)]
        public string SD6 { get; set; }

        [StringLength(15)]
        public string SD7 { get; set; }

        [StringLength(15)]
        public string SD8 { get; set; }

        [StringLength(15)]
        public string SD9 { get; set; }

        [StringLength(15)]
        public string SD10 { get; set; }
    }
}
