namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFAUTO2
    {
        [StringLength(15)]
        public string KODE { get; set; }

        [StringLength(40)]
        public string URAIAN { get; set; }

        [StringLength(1)]
        public string DK { get; set; }

        public double NILAI { get; set; }

        [StringLength(15)]
        public string REK_LAWAN { get; set; }

        [StringLength(10)]
        public string DEPT { get; set; }

        [StringLength(10)]
        public string TBIAYA { get; set; }

        [StringLength(1)]
        public string ST { get; set; }

        public double PERSENTAGE { get; set; }

        [StringLength(10)]
        public string PROYEK { get; set; }

        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LKS { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(15)]
        public string BUKTI { get; set; }

        public double TUKAR { get; set; }

        [Key]
        [Column(Order = 2)]
        public int NO { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
