namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF18
    {
        [Key]
        [StringLength(10)]
        public string KODE { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(50)]
        public string ALAMAT { get; set; }

        [StringLength(50)]
        public string FIELD1 { get; set; }

        [StringLength(50)]
        public string FIELD2 { get; set; }

        [StringLength(50)]
        public string FIELD3 { get; set; }

        [StringLength(50)]
        public string FIELD4 { get; set; }

        [StringLength(50)]
        public string FIELD5 { get; set; }

        [StringLength(50)]
        public string FIELD6 { get; set; }

        [StringLength(50)]
        public string FIELD7 { get; set; }

        [StringLength(50)]
        public string FIELD8 { get; set; }

        [StringLength(50)]
        public string FIELD9 { get; set; }

        [StringLength(50)]
        public string FIELD10 { get; set; }
    }
}
