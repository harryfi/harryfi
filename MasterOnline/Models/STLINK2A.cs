namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STLINK2A
    {
        [Key]
        [StringLength(5)]
        public string KLINK { get; set; }

        [StringLength(40)]
        public string KET { get; set; }

        [StringLength(2)]
        public string LKS { get; set; }

        [StringLength(10)]
        public string DEPT { get; set; }

        [StringLength(10)]
        public string COST { get; set; }

        [StringLength(2)]
        public string JURNAL { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
