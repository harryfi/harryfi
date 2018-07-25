namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFLAMP1
    {
        [Key]
        [StringLength(2)]
        public string KODE { get; set; }

        [StringLength(40)]
        public string JUDUL1 { get; set; }

        [StringLength(40)]
        public string JUDUL2 { get; set; }

        public short LKS { get; set; }

        [StringLength(10)]
        public string DEPT { get; set; }

        [StringLength(2)]
        public string KODE_BARIS { get; set; }

        [StringLength(2)]
        public string KODE_KOLOM { get; set; }

        [StringLength(1)]
        public string CNILAI { get; set; }

        [StringLength(1)]
        public string CASH { get; set; }

        [StringLength(1)]
        public string TXT { get; set; }

        public double TOTAL1 { get; set; }

        public double TOTAL2 { get; set; }

        public double TOTAL3 { get; set; }

        public double TOTAL4 { get; set; }

        public double TOTAL5 { get; set; }

        public double TOTAL6 { get; set; }

        public double TOTAL7 { get; set; }

        public double TOTAL8 { get; set; }

        public double TOTAL9 { get; set; }

        public double TOTAL10 { get; set; }

        public double TOTAL11 { get; set; }

        public double TOTAL12 { get; set; }

        public double TOTAL13 { get; set; }
    }
}
