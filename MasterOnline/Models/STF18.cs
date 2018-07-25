namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF18
    {
        [Key]
        [StringLength(4)]
        public string Kode_Gudang { get; set; }

        [StringLength(30)]
        public string Nama_Gudang { get; set; }

        public bool QOH_SALES { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? ID { get; set; }

        [StringLength(40)]
        public string AL1 { get; set; }

        [StringLength(40)]
        public string AL2 { get; set; }

        [StringLength(40)]
        public string AL3 { get; set; }

        public bool GD_KONSINYASI { get; set; }

        [StringLength(20)]
        public string CUSTOMER { get; set; }

        [StringLength(20)]
        public string KD_HARGA_JUAL { get; set; }

        public bool DISCOUNT { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
