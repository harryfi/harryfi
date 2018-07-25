namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT02B
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public POT02B()
        {
            POT02C = new HashSet<POT02C>();
        }

        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string POP_NO { get; set; }

        [StringLength(20)]
        public string ITEM_NO { get; set; }

        [StringLength(50)]
        public string ITEM_DESCRIPTION { get; set; }

        public byte? UNIT_CODE { get; set; }

        public double QUANTITY { get; set; }

        public double PO_QUANTITY { get; set; }

        [StringLength(250)]
        public string DESCRIPTION { get; set; }

        [Key]
        [Column(Order = 1)]
        public int NO_URUT { get; set; }

        public bool WRITE_KONFIG { get; set; }

        [StringLength(20)]
        public string USER_NAME { get; set; }

        public DateTime TGL_INPUT { get; set; }

        public double APPROVE_QTY { get; set; }

        [StringLength(10)]
        public string KODE_BUYER { get; set; }

        public DateTime? TGL_ALOKASI_BUYER { get; set; }

        public DateTime TGL_TERIMA { get; set; }

        public double APPROVE_QTY_2 { get; set; }

        public double APPROVE_QTY_3 { get; set; }

        [StringLength(200)]
        public string APPROVE_CATATAN_1 { get; set; }

        [StringLength(200)]
        public string APPROVE_CATATAN_2 { get; set; }

        [StringLength(200)]
        public string APPROVE_CATATAN_3 { get; set; }

        [StringLength(20)]
        public string APPROVE_OLEH { get; set; }

        public DateTime? APPROVE_TGL { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        public virtual POT02A POT02A { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<POT02C> POT02C { get; set; }
    }
}
