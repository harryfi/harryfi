namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT01B
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public POT01B()
        {
            POT01C = new HashSet<POT01C>();
            POT01D = new HashSet<POT01D>();
        }

        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string PO_NO { get; set; }

        [StringLength(20)]
        public string ITEM_NO { get; set; }

        [StringLength(50)]
        public string ITEM_DESCRIPTION { get; set; }

        public byte? UNIT_CODE { get; set; }

        public double QUANTITY { get; set; }

        public double APRICE { get; set; }

        public double PRICE { get; set; }

        public double DISC_PERCENT { get; set; }

        public double DISC_VALUE { get; set; }

        public double TOTAL { get; set; }

        public double? ACCEPT_QUANTITY { get; set; }

        public DateTime? ACCEPT_DATE { get; set; }

        public double BACK_QUANTITY { get; set; }

        [StringLength(10)]
        public string POP_NO { get; set; }

        [Key]
        [Column(Order = 1)]
        public int NO_URUT { get; set; }

        public double? BIAYA { get; set; }

        public bool WRITE_KONFIG { get; set; }

        [StringLength(20)]
        public string USER_NAME { get; set; }

        public DateTime? TGL_INPUT { get; set; }

        [StringLength(250)]
        public string KET { get; set; }

        public double? DISC_PERCENT_1 { get; set; }

        public double? DISC_PERCENT_2 { get; set; }

        public double? DISC_PERCENT_3 { get; set; }

        public double? DISC_VALUE_1 { get; set; }

        public double? DISC_VALUE_2 { get; set; }

        public double? DISC_VALUE_3 { get; set; }

        public double PPNBM_PERCENT { get; set; }

        public double PPNBM_VALUE { get; set; }

        [Required]
        [StringLength(10)]
        public string PS_NO { get; set; }

        public double? QTY_PL { get; set; }

        public int NO_URUT_PP { get; set; }

        public double? TRANS_NO_URUT { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<POT01C> POT01C { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<POT01D> POT01D { get; set; }
    }
}
