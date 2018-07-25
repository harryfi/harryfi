namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PBT01B
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PBT01B()
        {
            PBT01E = new HashSet<PBT01E>();
        }

        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENISFORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string INV { get; set; }

        [Required]
        [StringLength(10)]
        public string PO { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 2)]
        public int? NO { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG { get; set; }

        [StringLength(40)]
        public string NAMA_BRG { get; set; }

        [Required]
        [StringLength(4)]
        public string GD { get; set; }

        [Required]
        [StringLength(1)]
        public string BK { get; set; }

        public double QTY { get; set; }

        public double DISC2 { get; set; }

        public double NDISC2 { get; set; }

        public double HBELI { get; set; }

        public double THARGA { get; set; }

        [Required]
        [StringLength(10)]
        public string NOBUK { get; set; }

        [Required]
        [StringLength(2)]
        public string AUTO_LOAD { get; set; }

        public double QTY_RETUR { get; set; }

        public double BIAYA { get; set; }

        [Required]
        [StringLength(30)]
        public string USERNAME { get; set; }

        [Column(TypeName = "date")]
        public DateTime TGLINPUT { get; set; }

        public double TOTAL_LOT { get; set; }

        public double TOTAL_QTY { get; set; }

        public double DISCOUNT_1 { get; set; }

        public double DISCOUNT_2 { get; set; }

        public double DISCOUNT_3 { get; set; }

        public double NILAI_DISC_1 { get; set; }

        public double NILAI_DISC_2 { get; set; }

        public double NILAI_DISC_3 { get; set; }

        [Required]
        [StringLength(1000)]
        public string KET { get; set; }

        public double NO_URUT_PO { get; set; }

        public double PPNBM { get; set; }

        public double NILAI_PPNBM { get; set; }

        [Required]
        [StringLength(20)]
        public string BRG_ORIGINAL { get; set; }

        [Required]
        [StringLength(20)]
        public string LKU { get; set; }

        public virtual PBT01A PBT01A { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PBT01E> PBT01E { get; set; }
    }
}
