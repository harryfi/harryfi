namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF08
    {
        [Key]
        [StringLength(5)]
        public string ALAMAT { get; set; }

        [Column(TypeName = "text")]
        public string KET { get; set; }

        [StringLength(4)]
        public string GUD { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        public DateTime TGLINPUT { get; set; }
    }
}
