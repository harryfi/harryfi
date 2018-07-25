namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFREK_A
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LKS { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(15)]
        public string KODE { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(1)]
        public string JR { get; set; }

        [StringLength(1)]
        public string type { get; set; }

        [StringLength(5)]
        public string kurs { get; set; }

        [StringLength(15)]
        public string rek_konsol { get; set; }

        [StringLength(20)]
        public string UserId { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime TglInput { get; set; }

        [Key]
        [Column(Order = 2)]
        public int XXXXX { get; set; }
    }
}
