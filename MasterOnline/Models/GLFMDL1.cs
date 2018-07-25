namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFMDL1
    {
        [Key]
        [StringLength(2)]
        public string KODE { get; set; }

        [StringLength(40)]
        public string JUDUL1 { get; set; }

        [StringLength(40)]
        public string JUDUL2 { get; set; }

        public short LKS { get; set; }

        [StringLength(2)]
        public string KODE_BARIS { get; set; }

        public short BLN1 { get; set; }

        public short BLN2 { get; set; }

        public short BLN3 { get; set; }

        public short BLN4 { get; set; }

        public short THN1 { get; set; }

        public short THN2 { get; set; }

        [StringLength(1)]
        public string CNILAI { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }
    }
}
