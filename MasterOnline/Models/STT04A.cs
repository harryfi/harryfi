using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [JsonObject(IsReference = true)]
    public partial class STT04A
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public STT04A()
        {
            STT04B = new HashSet<STT04B>();
            //STT04B1 = new HashSet<STT04B1>();
        }
        
        [Key]
        [Column(Order = 0)]
        public string GUD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string NOBUK { get; set; }

        [StringLength(30)]
        public string NAMA_GUDANG { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        [StringLength(1)]
        public string POSTING { get; set; }

        public DateTime? TGL { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? ID { get; set; }

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<STT04B> STT04B { get; set; }

    }
}
