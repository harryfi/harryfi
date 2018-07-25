namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ART02
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ART02()
        {
            ART02B = new HashSet<ART02B>();
            ART02C = new HashSet<ART02C>();
        }

        [Key]
        [StringLength(10)]
        public string NOBUK { get; set; }

        public DateTime TGL { get; set; }

        [StringLength(10)]
        public string CUST { get; set; }

        [Column(TypeName = "text")]
        public string KET { get; set; }

        [StringLength(1)]
        public string POSTING { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        public double TUKAR { get; set; }

        public double TKOREKSI { get; set; }

        public double TPPN { get; set; }

        [StringLength(30)]
        public string NCUST { get; set; }

        public double TOTAL_DEBET_GL { get; set; }

        public double TOTAL_KREDIT_GL { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        public DateTime? TGLINPUT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ART02B> ART02B { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ART02C> ART02C { get; set; }
    }
}
