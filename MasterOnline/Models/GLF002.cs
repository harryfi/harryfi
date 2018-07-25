namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLF002
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short kode_cabang { get; set; }

        [StringLength(15)]
        public string Kode_rekening { get; set; }

        [StringLength(2)]
        public string kode_jurnal { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(15)]
        public string bukti { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? tanggal { get; set; }

        [StringLength(1)]
        public string dk { get; set; }

        public double nilai { get; set; }

        [StringLength(30)]
        public string keterangan { get; set; }

        [StringLength(1)]
        public string valid { get; set; }

        [Key]
        [Column(Order = 2)]
        public int no { get; set; }
    }
}
