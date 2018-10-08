namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ARF01
    {
        [Key]
        [StringLength(10)]
        public string CUST { get; set; }

        [Required]
        [StringLength(30)]
        public string NAMA { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string AL { get; set; }

        [StringLength(30)]
        public string AL2 { get; set; }

        [Required]
        [StringLength(30)]
        public string TLP { get; set; }

        [Required]
        [StringLength(30)]
        public string PERSO { get; set; }

        [StringLength(15)]
        public string NPWP { get; set; }

        public double TERM { get; set; }

        public double LIMIT { get; set; }

        [StringLength(100)]
        public string TOKEN { get; set; }

        [StringLength(100)]
        public string REFRESH_TOKEN { get; set; }

        [StringLength(50)]
        public string API_CLIENT_U { get; set; }

        public string API_CLIENT_P { get; set; }

        [StringLength(1)]
        public string PKP { get; set; }

        [StringLength(3)]
        public string WIL { get; set; }

        [StringLength(10)]
        public string KLINK { get; set; }

        public double KODE_CABANG { get; set; }

        [StringLength(30)]
        public string AL3 { get; set; }

        [StringLength(3)]
        public string SLM { get; set; }

        [StringLength(3)]
        public string VLT { get; set; }

        [StringLength(2)]
        public string KDHARGA { get; set; }

        [StringLength(30)]
        public string AL_KIRIM1 { get; set; }

        [StringLength(30)]
        public string AL_KIRIM2 { get; set; }

        [StringLength(30)]
        public string AL_KIRIM3 { get; set; }

        public double DISC_NOTA { get; set; }

        public double NDISC_NOTA { get; set; }

        public double DISC_ITEM { get; set; }

        public double NDISC_ITEM { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }

        [StringLength(1)]
        public string KD_ANALISA { get; set; }

        public double LABA { get; set; }

        public bool TIDAK_HIT_UANG_R { get; set; }

        [StringLength(10)]
        public string Sort1_Area { get; set; }

        [StringLength(10)]
        public string Sort2_Area { get; set; }

        [StringLength(10)]
        public string Sort3_Area { get; set; }

        [StringLength(10)]
        public string Sort4_Area { get; set; }

        [StringLength(10)]
        public string Sort5_Area { get; set; }

        [StringLength(10)]
        public string Sort1_Cust { get; set; }

        [StringLength(10)]
        public string Sort2_Cust { get; set; }

        [StringLength(10)]
        public string Sort3_Cust { get; set; }

        [StringLength(10)]
        public string Sort4_Cust { get; set; }

        [StringLength(10)]
        public string Sort5_Cust { get; set; }

        [StringLength(10)]
        public string Sort6_Cust { get; set; }

        [StringLength(10)]
        public string Attr1_Cust { get; set; }

        [StringLength(10)]
        public string Attr2_Cust { get; set; }

        [StringLength(10)]
        public string Attr3_Cust { get; set; }

        [StringLength(10)]
        public string Attr4_Cust { get; set; }

        [StringLength(10)]
        public string Attr1_Area { get; set; }

        [StringLength(10)]
        public string Attr2_Area { get; set; }

        [StringLength(10)]
        public string Attr3_Area { get; set; }

        [StringLength(10)]
        public string Attr4_Area { get; set; }

        [StringLength(10)]
        public string Attr5_Area { get; set; }

        //[StringLength(50)]
        public string Kode { get; set; }

        [StringLength(2)]
        public string No_Seri_Pajak { get; set; }

        public DateTime? TGL_INPUT { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }

        [StringLength(20)]
        public string PASSWORD { get; set; }

        [StringLength(10)]
        public string GD1 { get; set; }

        [Required]
        [StringLength(7)]
        public string KODEPOS { get; set; }

        [Required]
        [StringLength(50)]
        public string EMAIL { get; set; }

        [Required]
        [StringLength(4)]
        public string KODEKABKOT { get; set; }

        [Required]
        [StringLength(2)]
        public string KODEPROV { get; set; }

        public int TOP { get; set; }

        [StringLength(100)]
        public string API_KEY { get; set; }

        [StringLength(1)]
        public string STATUS_API { get; set; }
    }
}
