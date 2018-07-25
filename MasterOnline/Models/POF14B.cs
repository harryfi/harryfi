namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF14B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string DIV { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string BRG { get; set; }

        [Key]
        [Column(Order = 2)]
        public double NO_URUT_BAPAK { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BULAN { get; set; }

        [Key]
        [Column(Order = 4)]
        public double TAHUN { get; set; }

        public double? QTY { get; set; }

        public double? NILAI { get; set; }

        [Required]
        [StringLength(20)]
        public string USERNAME { get; set; }

        public DateTime TGLINPUT { get; set; }
    }
}
