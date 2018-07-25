namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFRLA2
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(2)]
        public string KODE_BARIS { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(2)]
        public string KODE { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(1)]
        public string CETAK_RL { get; set; }

        [StringLength(1)]
        public string JR { get; set; }

        [StringLength(1)]
        public string HIT_KEL { get; set; }

        [StringLength(1)]
        public string CETAK_KEL { get; set; }

        [StringLength(30)]
        public string NAKEM { get; set; }

        [StringLength(1)]
        public string K_PJL { get; set; }

        [StringLength(30)]
        public string NARL { get; set; }

        [StringLength(1)]
        public string NOLKAN { get; set; }

        [StringLength(30)]
        public string NASUB { get; set; }

        public bool KONSOLIDASI { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }
    }
}
