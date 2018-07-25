namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFMDL2
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GLFMDL2()
        {
            GLFMDL3 = new HashSet<GLFMDL3>();
        }

        [Key]
        [Column(Order = 0)]
        [StringLength(2)]
        public string KODE { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(2)]
        public string KODE_BARIS { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        [StringLength(1)]
        public string JENIS { get; set; }

        [StringLength(1)]
        public string JSALDO { get; set; }

        [StringLength(1)]
        public string CETAK_TT { get; set; }

        [StringLength(30)]
        public string NAMA_TT { get; set; }

        [StringLength(1)]
        public string AKTIVA { get; set; }

        [StringLength(1)]
        public string CETAK_AKHIR { get; set; }

        [StringLength(30)]
        public string NAMA_AKHIR { get; set; }

        public bool KONSOLIDASI { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GLFMDL3> GLFMDL3 { get; set; }
    }
}
