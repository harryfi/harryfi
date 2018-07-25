namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFNER3
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int KDN_URUT { get; set; }

        [StringLength(75)]
        public string KDN_NAMA { get; set; }

        [StringLength(1)]
        public string RL { get; set; }

        [StringLength(15)]
        public string KDN_DARI { get; set; }

        [StringLength(15)]
        public string KDN_SAMPAI { get; set; }

        [StringLength(1)]
        public string PERINCI { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(2)]
        public string KODE { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(2)]
        public string KDN { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }
    }
}
