namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POT02A
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public POT02A()
        {
            POT02B = new HashSet<POT02B>();
        }

        [Key]
        [StringLength(10)]
        public string POP_NO { get; set; }

        public DateTime POP_DATE { get; set; }

        [StringLength(10)]
        public string DIVISION { get; set; }

        [StringLength(255)]
        public string DESCRIPTION { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        public double PRINT_COPIES { get; set; }

        [StringLength(20)]
        public string USER_NAME { get; set; }

        public DateTime TGL_INPUT { get; set; }

        [StringLength(20)]
        public string APPROVE_OLEH { get; set; }

        public DateTime? APPROVE_TGL { get; set; }

        [StringLength(10)]
        public string BUYER_CODE { get; set; }

        public DateTime? TGL_TERIMA_BUYER { get; set; }

        public DateTime? TGL_TERIMA_SPP { get; set; }

        [StringLength(10)]
        public string PO_NO { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [StringLength(10)]
        public string GroupBrg { get; set; }

        [Required]
        [StringLength(20)]
        public string APPROVE_OLEH_2 { get; set; }

        [Required]
        [StringLength(20)]
        public string APPROVE_OLEH_3 { get; set; }

        public DateTime? APPROVE_TGL_2 { get; set; }

        public DateTime? APPROVE_TGL_3 { get; set; }

        [Required]
        [StringLength(20)]
        public string APPROVE_CATATAN_1 { get; set; }

        [Required]
        [StringLength(20)]
        public string APPROVE_CATATAN_2 { get; set; }

        [Required]
        [StringLength(20)]
        public string APPROVE_CATATAN_3 { get; set; }

        public bool? AUTOCREATE { get; set; }

        [StringLength(10)]
        public string GD { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<POT02B> POT02B { get; set; }

        public virtual POT02E POT02E { get; set; }
    }
}
