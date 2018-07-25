namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF02H
    {
        [Key]
        public int? RecNum { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG { get; set; }

        public int IDMARKET { get; set; }

        [Required]
        [StringLength(50)]
        public string AKUNMARKET { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        public double HJUAL { get; set; }

        [StringLength(50)]
        public string BRG_MP { get; set; }

        public bool DISPLAY { get; set; }

        [StringLength(10)]
        public string DeliveryTempElevenia { get; set; }
    }
}
