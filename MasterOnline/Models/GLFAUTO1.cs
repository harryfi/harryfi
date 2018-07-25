namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFAUTO1
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LKS { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string BUKTI { get; set; }

        public short JENIS { get; set; }

        public int? NO_AKHIR { get; set; }

        [StringLength(2)]
        public string JURNAL { get; set; }

        public DateTime WKT_PROSES { get; set; }

        [StringLength(15)]
        public string BUKTI_JURNAL_GL { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
