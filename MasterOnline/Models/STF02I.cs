namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF02I
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }
        public string BRG { get; set; }
        public string CATEGORY_MO { get; set; }
        public string MP_CATEGORY_CODE { get; set; }
        public int LEVEL_VAR { get; set; }
        public string KODE_VAR { get; set; }
        public string MP_JUDUL_VAR { get; set; }
        public string MP_VALUE_VAR { get; set; }
        public string MARKET { get; set; }
        public string MP_VALUE_FC_VAR { get; set; }//SIMPAN VALUE FAMILY COLOUR BLIBLI
    }
}
