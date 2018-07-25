namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF03
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public POF03()
        {
            POF03A = new HashSet<POF03A>();
        }

        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string ITEM_NO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string SUPPLIER_CODE { get; set; }

        public DateTime? TGLINPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<POF03A> POF03A { get; set; }
    }
}
