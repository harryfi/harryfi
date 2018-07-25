namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TEMP_PBT01F
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENIS_FORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string NO_BUKTI { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(40)]
        public string CATATAN_1 { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(40)]
        public string CATATAN_2 { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(40)]
        public string CATATAN_3 { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(40)]
        public string CATATAN_4 { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(40)]
        public string CATATAN_5 { get; set; }

        [Key]
        [Column(Order = 7)]
        [StringLength(40)]
        public string CATATAN_6 { get; set; }

        [Key]
        [Column(Order = 8)]
        [StringLength(40)]
        public string CATATAN_7 { get; set; }

        [Key]
        [Column(Order = 9)]
        [StringLength(40)]
        public string CATATAN_8 { get; set; }

        [Key]
        [Column(Order = 10)]
        [StringLength(40)]
        public string CATATAN_9 { get; set; }

        [Key]
        [Column(Order = 11)]
        [StringLength(40)]
        public string CATATAN_10 { get; set; }
    }
}
