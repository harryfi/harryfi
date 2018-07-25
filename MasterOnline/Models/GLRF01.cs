namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLRF01
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short URUT { get; set; }

        [StringLength(40)]
        public string KET { get; set; }

        public double X1 { get; set; }

        public double X2 { get; set; }

        [StringLength(15)]
        public string X3 { get; set; }

        [StringLength(15)]
        public string X4 { get; set; }

        public double X5 { get; set; }

        [StringLength(15)]
        public string X_HITUNG { get; set; }

        [StringLength(1)]
        public string X_PERIODE { get; set; }

        [StringLength(1)]
        public string X_BANDING { get; set; }

        [StringLength(1)]
        public string X_PERSEN { get; set; }

        public double Y1 { get; set; }

        public double Y2 { get; set; }

        [StringLength(15)]
        public string Y3 { get; set; }

        [StringLength(15)]
        public string Y4 { get; set; }

        public double Y5 { get; set; }

        [StringLength(15)]
        public string Y_HITUNG { get; set; }

        [StringLength(1)]
        public string Y_PERIODE { get; set; }

        [StringLength(1)]
        public string Y_BANDING { get; set; }

        [StringLength(1)]
        public string CETAK { get; set; }

        public short? GRUP { get; set; }

        public short? URUT2 { get; set; }
    }
}
