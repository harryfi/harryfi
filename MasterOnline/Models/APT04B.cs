namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class APT04B
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Rec_Num { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string No_SPK { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime Tgl_SPK { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string Nobuk { get; set; }

        public double? Nilai { get; set; }
    }
}
