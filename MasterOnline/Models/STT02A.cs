namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT02A
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public STT02A()
        {
            STT02B = new HashSet<STT02B>();
        }

        [Key]
        [Column(Order = 0)]
        public byte JENIS_FORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string NOBUK { get; set; }

        public DateTime TGL { get; set; }

        [StringLength(1)]
        public string JREF { get; set; }

        [StringLength(15)]
        public string REF { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        public bool AMBIL_PENUH { get; set; }

        [StringLength(255)]
        public string KET { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [StringLength(15)]
        public string WORK_CENTER { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<STT02B> STT02B { get; set; }
    }
}
