namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PBT03B
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string NOBUK { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string BRG { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(30)]
        public string NAMA { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(30)]
        public string NAMA2 { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int STN { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int QTY { get; set; }

        [Key]
        [Column(Order = 6)]
        public double HARGA { get; set; }
    }
}
