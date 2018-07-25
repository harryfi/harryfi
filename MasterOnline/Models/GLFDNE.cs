namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFDNE")]
    public partial class GLFDNE
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LKS { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(2)]
        public string KDN { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short KDN_URUT { get; set; }

        [StringLength(30)]
        public string KDN_NAMA { get; set; }

        [StringLength(1)]
        public string RL { get; set; }

        [StringLength(10)]
        public string KDN_DARI { get; set; }

        [StringLength(10)]
        public string KDN_SAMPAI { get; set; }
    }
}
