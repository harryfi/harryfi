namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFMDL3
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NO_URUT { get; set; }

        [StringLength(75)]
        public string NAMA { get; set; }

        [StringLength(15)]
        public string KODE_DARI { get; set; }

        [StringLength(15)]
        public string KODE_SAMPAI { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(2)]
        public string KODE { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(2)]
        public string KODE_BARIS { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }

        public virtual GLFMDL2 GLFMDL2 { get; set; }
    }
}
