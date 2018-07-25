namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFLAMP3
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NO_URUT { get; set; }

        [StringLength(75)]
        public string NAMA { get; set; }

        [StringLength(1)]
        public string N_RL { get; set; }

        [StringLength(15)]
        public string KODE_DARI { get; set; }

        [StringLength(15)]
        public string KODE_SAMPAI { get; set; }

        [StringLength(10)]
        public string DRDEPT { get; set; }

        [StringLength(10)]
        public string SDDEPT { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(2)]
        public string KODE_BARIS { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(2)]
        public string KODE { get; set; }

        [StringLength(2)]
        public string Kolom_Khusus { get; set; }

        [StringLength(10)]
        public string DRCOST_CENTER { get; set; }

        [StringLength(10)]
        public string SDCOST_CENTER { get; set; }
    }
}
