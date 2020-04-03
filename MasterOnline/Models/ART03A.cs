using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ART03A
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ART03A()
        {
            ART03B = new HashSet<ART03B>();
            ART03C = new HashSet<ART03C>();
        }

        [Key]
        [StringLength(10)]
        public string BUKTI { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

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

        public double MUKA1 { get; set; }

        public double MUKA2 { get; set; }

        public double KONTAN { get; set; }

        public double TBAYAR { get; set; }

        public double TPOT { get; set; }

        [StringLength(30)]
        public string NCUST { get; set; }

        public double TOTAL_DEBET_GL { get; set; }

        public double TOTAL_KREDIT_GL { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        public DateTime? TGLINPUT { get; set; }

        //add by nurul 12/3/2020
        public double? TLEBIH_BAYAR { get; set; }
        //end add by nurul 12/3/2020

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ART03B> ART03B { get; set; }

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ART03C> ART03C { get; set; }
    }
}
