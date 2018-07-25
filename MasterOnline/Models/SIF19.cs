namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIF19
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NO { get; set; }

        [StringLength(10)]
        public string OldSort1 { get; set; }

        [StringLength(10)]
        public string OldSort2 { get; set; }

        [StringLength(10)]
        public string OldSort3 { get; set; }

        [StringLength(10)]
        public string OldSort4 { get; set; }

        [StringLength(10)]
        public string OldSort5 { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime TGL { get; set; }

        [StringLength(20)]
        public string KODE_BRG_BARU { get; set; }

        [StringLength(10)]
        public string NewSort1 { get; set; }

        [StringLength(10)]
        public string NewSort2 { get; set; }

        [StringLength(10)]
        public string NewSort3 { get; set; }

        [StringLength(10)]
        public string NewSort4 { get; set; }

        [StringLength(10)]
        public string NewSort5 { get; set; }

        [Key]
        [Column(Order = 2)]
        public bool STATUS_PROSES { get; set; }

        [StringLength(10)]
        public string NO_BUKTI_MASUK_GD { get; set; }

        [StringLength(10)]
        public string NO_BUKTI_KELUAR_GD { get; set; }

        [Key]
        [Column(Order = 3)]
        public bool UNPOST { get; set; }
    }
}
