using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [JsonObject(IsReference = true)]
    public partial class SIT01A
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SIT01A()
        {
            SIT01B = new HashSet<SIT01B>();
            SIT01B1 = new HashSet<SIT01B1>();
            SIT01C = new HashSet<SIT01C>();
        }

        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENIS_FORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(15)]
        public string NO_BUKTI { get; set; }

        [Column(TypeName = "date")]
        public DateTime TGL { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        [StringLength(1)]
        public string ST_POSTING { get; set; }

        public DateTime? TGL_KIRIM { get; set; }

        [StringLength(100)]
        public string NO_REF { get; set; }

        [StringLength(15)]
        public string NO_SO { get; set; }

        [StringLength(10)]
        public string CUST { get; set; }

        [StringLength(40)]
        public string NAMA_CUST { get; set; }

        [StringLength(5)]
        public string KODE_ALAMAT { get; set; }

        [StringLength(20)]
        public string NO_KENDARAAN { get; set; }

        [StringLength(20)]
        public string SOPIR { get; set; }

        [Column(TypeName = "text")]
        public string KET { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        public double? TUKAR { get; set; }

        public double? TUKAR_PPN { get; set; }

        [StringLength(5)]
        public string KODE_SALES { get; set; }

        [StringLength(5)]
        public string KODE_WIL { get; set; }

        [StringLength(25)]
        public string NO_F_PAJAK { get; set; }

        public DateTime? TGL_F_PAJAK { get; set; }

        [StringLength(10)]
        public string KODE_PROYEK { get; set; }

        public double? BRUTO { get; set; }

        public double? DISCOUNT { get; set; }

        public double? NILAI_DISC { get; set; }

        public double? PPN { get; set; }

        public double? NILAI_PPN { get; set; }

        public double? PPNBM { get; set; }

        public double? NILAI_PPNBM { get; set; }

        public double? MATERAI { get; set; }

        public double? NETTO { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        [Column(TypeName = "date")]
        public DateTime TGLINPUT { get; set; }

        [StringLength(15)]
        public string NO_MK { get; set; }

        [StringLength(1)]
        public string JENIS_RETUR { get; set; }

        public short? PRINT_COUNT { get; set; }

        public bool RETUR_PENUH { get; set; }

        [StringLength(3)]
        public string JTRAN { get; set; }

        //[StringLength(40)]
        public string AL3 { get; set; }

        //[StringLength(40)]
        public string AL2 { get; set; }

        //[StringLength(40)]
        public string AL1 { get; set; }

        //[StringLength(40)]
        public string AL { get; set; }

        public double? U_MUKA { get; set; }

        public double? TERM { get; set; }

        public bool PPN_ditangguhkan { get; set; }

        public byte? PPN_Bln_Lapor { get; set; }

        public short? PPN_Thn_Lapor { get; set; }

        [StringLength(1)]
        public string JENIS { get; set; }

        [StringLength(10)]
        public string CUST_QQ { get; set; }

        [StringLength(40)]
        public string NAMA_CUST_QQ { get; set; }

        public DateTime? TGL_JT_TEMPO { get; set; }

        public bool KIRIM_PENUH { get; set; }

        [StringLength(10)]
        public string NO_FAKTUR_PPN_AR { get; set; }

        public double U_MUKA_FA { get; set; }

        [StringLength(10)]
        public string NO_FAKTUR_LAMA { get; set; }

        public bool BATAL { get; set; }

        public bool SJ_ADA_FAKTUR { get; set; }

        [StringLength(50)]
        public string STATUS_LOADING { get; set; }

        [Required]
        [StringLength(10)]
        public string NO_FA_OUTLET { get; set; }

        [Required]
        [StringLength(10)]
        public string NO_LPB { get; set; }

        [StringLength(30)]
        public string NO_PO_CUST { get; set; }

        [Required]
        [StringLength(10)]
        public string GROUP_LIMIT { get; set; }

        [Required]
        [StringLength(5)]
        public string KODE_ANGKUTAN { get; set; }

        [Required]
        [StringLength(2)]
        public string JENIS_MOBIL { get; set; }

        public double NILAI_ANGKUTAN { get; set; }

        [StringLength(10)]
        public string PENGIRIM { get; set; }

        [StringLength(50)]
        public string NAMAPENGIRIM { get; set; }

        [StringLength(5)]
        public string ZONA { get; set; }

        public DateTime? JAMKIRIM { get; set; }

        [StringLength(5)]
        public string UCAPAN { get; set; }

        [StringLength(255)]
        public string N_UCAPAN { get; set; }

        [StringLength(10)]
        public string PEMESAN { get; set; }

        [StringLength(30)]
        public string NAMAPEMESAN { get; set; }

        public double? KOMISI { get; set; }

        public double? N_KOMISI { get; set; }

        public double? JML_VOUCHER { get; set; }

        [StringLength(15)]
        public string NO_SERI_VOUCHER { get; set; }

        public double? N_VOUCHER { get; set; }

        public bool APPROVAL { get; set; }

        public double? TOTAL_TITIPAN { get; set; }

        [StringLength(20)]
        public string SUPP { get; set; }

        public DateTime? TGL_POSTING { get; set; }

        [StringLength(20)]
        public string USERNAME_POSTING { get; set; }

        [StringLength(30)]
        public string USERNAME_APPROVAL { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        public int? WMS { get; set; }

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SIT01B> SIT01B { get; set; }

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SIT01B1> SIT01B1 { get; set; }

        [JsonIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SIT01C> SIT01C { get; set; }

        [JsonIgnore]
        public virtual SIT01F SIT01F { get; set; }
    }
}
