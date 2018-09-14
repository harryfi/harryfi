using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PBT01A
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PBT01A()
        {
            PBT01B = new HashSet<PBT01B>();
            PBT01B1 = new HashSet<PBT01B1>();
            PBT01C = new HashSet<PBT01C>();
            PBT01D = new HashSet<PBT01D>();
        }

        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENISFORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string INV { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TGL { get; set; }

        [StringLength(1)]
        public string JENIS { get; set; }

        [StringLength(10)]
        public string PO { get; set; }

        public double DISC1 { get; set; }

        public double NDISC1 { get; set; }

        public double PPN { get; set; }

        public double NPPN { get; set; }

        [Required]
        [StringLength(1)]
        public string STATUS { get; set; }

        [Required]
        [StringLength(1)]
        public string POSTING { get; set; }

        [Required]
        [StringLength(1)]
        public string APP { get; set; }

        [Required]
        [StringLength(10)]
        public string SUPP { get; set; }

        [Required]
        [StringLength(40)]
        public string NAMA { get; set; }

        [Required]
        [StringLength(5)]
        public string VLT { get; set; }

        public short TERM { get; set; }

        [StringLength(15)]
        public string KODE_REF_PESANAN { get; set; }

        public bool? DROPSHIPPER { get; set; }

        [StringLength(25)]
        public string F_PAJAK { get; set; }

        public DateTime? T_PAJAK { get; set; }

        public double TUKAR { get; set; }

        public double BRUTO { get; set; }

        public double BIAYA_LAIN { get; set; }

        public double NETTO { get; set; }

        [Required]
        [StringLength(10)]
        public string REF { get; set; }

        [Column(TypeName = "text")]
        public string KET { get; set; }

        public bool RETUR_PENUH { get; set; }

        [Required]
        [StringLength(30)]
        public string USERNAME { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TGLINPUT { get; set; }

        public bool PPN_ditangguhkan { get; set; }

        public byte PPN_Bln_Lapor { get; set; }

        public short PPN_Thn_Lapor { get; set; }

        public double UANG_MUKA { get; set; }

        public DateTime? TGJT { get; set; }

        public double TUKAR_PPN { get; set; }

        public double NILAI_PPN { get; set; }

        [Required]
        [StringLength(25)]
        public string NO_INVOICE_SUPP { get; set; }

        public double PPNBM { get; set; }

        public double TYPE_BELI { get; set; }

        public double PPH_22 { get; set; }

        public double NPPH_22 { get; set; }

        public double U_MUKA { get; set; }

        public double U_MUKA_INV { get; set; }

        [StringLength(15)]
        public string WO_SUBCON { get; set; }

        public double BERAT { get; set; }

        public double VOLUME { get; set; }

        public double HS_BERAT { get; set; }

        public double HS_VOLUME { get; set; }

        [StringLength(10)]
        public string CONSIGNEE { get; set; }

        [StringLength(1)]
        public string SHIPMENT_TYPE { get; set; }

        public DateTime? RECEIVED_DATE { get; set; }

        public DateTime? SUPP_INV_DATE { get; set; }

        [StringLength(20)]
        public string BL_NO { get; set; }

        public DateTime? BL_DATE { get; set; }

        public double FREIGHT { get; set; }

        public double INSURANCE { get; set; }

        public double OTHERS { get; set; }

        [StringLength(10)]
        public string SHIPPER { get; set; }

        [StringLength(50)]
        public string PORT { get; set; }

        public double PREPAID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PBT01B> PBT01B { get; set; }

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PBT01B1> PBT01B1 { get; set; }

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PBT01C> PBT01C { get; set; }

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PBT01D> PBT01D { get; set; }

        [JsonIgnore]
        public virtual PBT01F PBT01F { get; set; }
    }
}
