namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT03A
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public STT03A()
        {
            STT03B = new HashSet<STT03B>();
        }

        [Key]
        [StringLength(10)]
        public string Nobuk { get; set; }

        public DateTime Tgl { get; set; }

        [StringLength(1)]
        public string Jenis { get; set; }

        [StringLength(30)]
        public string Ket { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<STT03B> STT03B { get; set; }
    }
}
