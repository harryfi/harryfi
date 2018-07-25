namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT05A
    {
        [Key]
        [StringLength(10)]
        public string NOBUK { get; set; }

        public DateTime TGL { get; set; }

        [StringLength(1)]
        public string MK { get; set; }

        [StringLength(2)]
        public string DARI { get; set; }

        [StringLength(2)]
        public string KE { get; set; }

        [StringLength(1)]
        public string SATUAN { get; set; }

        [StringLength(30)]
        public string KET { get; set; }

        [StringLength(1)]
        public string JTRAN { get; set; }

        [StringLength(10)]
        public string REF { get; set; }

        [StringLength(1)]
        public string ST_CETAK { get; set; }
    }
}
