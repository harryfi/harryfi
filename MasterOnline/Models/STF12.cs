namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF12
    {
        [Key]
        [StringLength(2)]
        public string KODE { get; set; }

        [StringLength(30)]
        public string KET { get; set; }

        [StringLength(15)]
        public string JUD1 { get; set; }

        [StringLength(15)]
        public string JUD2 { get; set; }

        [StringLength(15)]
        public string JUD3 { get; set; }

        [StringLength(15)]
        public string JUD4 { get; set; }

        [StringLength(15)]
        public string JUD5 { get; set; }

        [StringLength(15)]
        public string JUD6 { get; set; }

        [StringLength(15)]
        public string JUD7 { get; set; }

        [StringLength(15)]
        public string JUD8 { get; set; }

        [StringLength(15)]
        public string JUD9 { get; set; }

        [StringLength(15)]
        public string JUD10 { get; set; }

        [StringLength(2)]
        public string TRAN1 { get; set; }

        [StringLength(2)]
        public string TRAN2 { get; set; }

        [StringLength(2)]
        public string TRAN3 { get; set; }

        [StringLength(2)]
        public string TRAN4 { get; set; }

        [StringLength(2)]
        public string TRAN5 { get; set; }

        [StringLength(2)]
        public string TRAN6 { get; set; }

        [StringLength(2)]
        public string TRAN7 { get; set; }

        [StringLength(2)]
        public string TRAN8 { get; set; }

        [StringLength(2)]
        public string TRAN9 { get; set; }

        [StringLength(2)]
        public string TRAN10 { get; set; }
    }
}
