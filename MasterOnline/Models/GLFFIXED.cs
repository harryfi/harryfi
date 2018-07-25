namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFFIXED")]
    public partial class GLFFIXED
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LKS { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string BRG { get; set; }

        [StringLength(30)]
        public string NBRG { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime TGL_BELI { get; set; }

        public double HARGA { get; set; }

        public double TAMBAH { get; set; }

        public short? UMUR { get; set; }

        public double SUSUT { get; set; }

        [StringLength(15)]
        public string REK1 { get; set; }

        [StringLength(15)]
        public string REK2 { get; set; }

        [StringLength(1)]
        public string J_SUSUT { get; set; }

        [StringLength(15)]
        public string REK3 { get; set; }

        [StringLength(10)]
        public string JENIS { get; set; }

        [StringLength(20)]
        public string LOKASI { get; set; }

        [StringLength(10)]
        public string KEL { get; set; }
    }
}
