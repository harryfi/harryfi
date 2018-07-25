namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFREK")]
    public partial class GLFREK
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LKS { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(15)]
        public string KODE { get; set; }

        [StringLength(75)]
        public string NAMA { get; set; }

        [StringLength(1)]
        public string JR { get; set; }

        [StringLength(1)]
        public string type { get; set; }

        [StringLength(5)]
        public string kurs { get; set; }

        [StringLength(50)]
        public string rek_konsol { get; set; }

        [StringLength(50)]
        public string UserId { get; set; }

        public DateTime? TglInput { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        [StringLength(2)]
        public string KATEGORY { get; set; }
    }
}
