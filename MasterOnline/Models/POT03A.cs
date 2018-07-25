namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT03A
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public POT03A()
        {
            POT03C = new HashSet<POT03C>();
        }

        [Key]
        [StringLength(10)]
        public string MATRIX { get; set; }

        [StringLength(10)]
        public string BUYER_CODE { get; set; }

        [StringLength(10)]
        public string POP_NO { get; set; }

        [StringLength(10)]
        public string SUPP1 { get; set; }

        [StringLength(10)]
        public string SUPP2 { get; set; }

        [StringLength(10)]
        public string SUPP3 { get; set; }

        [StringLength(20)]
        public string APPROVE_OLEH { get; set; }

        public DateTime? TGL_APPROVE { get; set; }

        [StringLength(10)]
        public string SUPP_TERPILIH { get; set; }

        [StringLength(10)]
        public string PO { get; set; }

        public double TOTAL1 { get; set; }

        public double TOTAL2 { get; set; }

        public double TOTAL3 { get; set; }

        public double TDISC1 { get; set; }

        public double TDISC2 { get; set; }

        public double TDISC3 { get; set; }

        public double TBIAYA1 { get; set; }

        public double TBIAYA2 { get; set; }

        public double TBIAYA3 { get; set; }

        public double DPP1 { get; set; }

        public double DPP2 { get; set; }

        public double DPP3 { get; set; }

        public double PPN1 { get; set; }

        public double PPN2 { get; set; }

        public double PPN3 { get; set; }

        public bool AUTO_MATRIX { get; set; }

        [StringLength(5)]
        public string ALAMAT { get; set; }

        [StringLength(5)]
        public string ALASAN { get; set; }

        [StringLength(10)]
        public string SUPP4 { get; set; }

        [StringLength(10)]
        public string SUPP5 { get; set; }

        [StringLength(10)]
        public string SUPP6 { get; set; }

        public double TOTAL4 { get; set; }

        public double TOTAL5 { get; set; }

        public double TOTAL6 { get; set; }

        public double TDISC4 { get; set; }

        public double TDISC5 { get; set; }

        public double TDISC6 { get; set; }

        public double TBIAYA4 { get; set; }

        public double TBIAYA5 { get; set; }

        public double TBIAYA6 { get; set; }

        public double DPP4 { get; set; }

        public double DPP5 { get; set; }

        public double DPP6 { get; set; }

        public double PPN4 { get; set; }

        public double PPN5 { get; set; }

        public double PPN6 { get; set; }

        public double? TUKAR1 { get; set; }

        public double? TUKAR2 { get; set; }

        public double? TUKAR3 { get; set; }

        public double? TUKAR4 { get; set; }

        public double? TUKAR5 { get; set; }

        public double? TUKAR6 { get; set; }

        public double PPNBM1 { get; set; }

        public double PPNBM2 { get; set; }

        public double PPNBM3 { get; set; }

        public double PPNBM4 { get; set; }

        public double PPNBM5 { get; set; }

        public double PPNBM6 { get; set; }

        public double TYPE_BELI { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public double QTY_SUPP_1 { get; set; }

        public double QTY_SUPP_2 { get; set; }

        public double QTY_SUPP_3 { get; set; }

        public double QTY_SUPP_4 { get; set; }

        public double QTY_SUPP_5 { get; set; }

        public double QTY_SUPP_6 { get; set; }

        public bool SUPP_TERPILIH_1 { get; set; }

        public bool SUPP_TERPILIH_2 { get; set; }

        public bool SUPP_TERPILIH_3 { get; set; }

        public bool SUPP_TERPILIH_4 { get; set; }

        public bool SUPP_TERPILIH_5 { get; set; }

        public bool SUPP_TERPILIH_6 { get; set; }

        [Required]
        [StringLength(10)]
        public string PO_NO_1 { get; set; }

        [Required]
        [StringLength(10)]
        public string PO_NO_2 { get; set; }

        [Required]
        [StringLength(10)]
        public string PO_NO_3 { get; set; }

        [Required]
        [StringLength(10)]
        public string PO_NO_4 { get; set; }

        [Required]
        [StringLength(10)]
        public string PO_NO_5 { get; set; }

        [Required]
        [StringLength(10)]
        public string PO_NO_6 { get; set; }

        public DateTime? TGL_INPUT { get; set; }

        [StringLength(50)]
        public string USER_NAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<POT03C> POT03C { get; set; }
    }
}
