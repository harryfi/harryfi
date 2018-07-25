namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFTRAN2
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short lks { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(15)]
        public string bukti { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 2)]
        public int? no { get; set; }

        [StringLength(15)]
        public string rek { get; set; }

        [StringLength(75)]
        public string urai { get; set; }

        [StringLength(1)]
        public string dk { get; set; }

        [StringLength(5)]
        public string kurs { get; set; }

        public double nilai { get; set; }

        [StringLength(15)]
        public string rek_lawan { get; set; }

        [StringLength(10)]
        public string dept { get; set; }

        [StringLength(10)]
        public string tbiaya { get; set; }

        [StringLength(10)]
        public string proyek { get; set; }

        public double tukar { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? Tgl_Input { get; set; }

        [StringLength(20)]
        public string UserId { get; set; }

        [StringLength(2)]
        public string GST_Type { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
