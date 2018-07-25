namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFBUK")]
    public partial class GLFBUK
    {
        [Key]
        [Column(Order = 0)]
        public int ID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LKS { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(15)]
        public string KODE_REK { get; set; }

        [Key]
        [Column(Order = 3, TypeName = "smalldatetime")]
        public DateTime TGL_BUKTI { get; set; }

        [StringLength(15)]
        public string NO_BUKTI { get; set; }

        [StringLength(75)]
        public string KET { get; set; }

        public double NILAI { get; set; }

        [StringLength(1)]
        public string DK { get; set; }

        [StringLength(2)]
        public string JURNAL { get; set; }

        [StringLength(15)]
        public string LAWAN { get; set; }

        [StringLength(10)]
        public string PROYEK { get; set; }

        [StringLength(10)]
        public string TBIAYA { get; set; }

        [StringLength(10)]
        public string DEPT { get; set; }

        [StringLength(5)]
        public string KURS { get; set; }

        public double TUKAR { get; set; }

        public double NILAI2 { get; set; }
    }
}
