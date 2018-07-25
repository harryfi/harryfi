namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFDRL")]
    public partial class GLFDRL
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LKS { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(2)]
        public string KODE { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short NO_URUT { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(10)]
        public string KODE_DARI { get; set; }

        [StringLength(10)]
        public string KODE_SAMPAI { get; set; }
    }
}
