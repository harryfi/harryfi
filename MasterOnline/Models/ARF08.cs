namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ARF08
    {
        [Key]
        [StringLength(3)]
        public string SLM { get; set; }

        [StringLength(30)]
        public string NAMA { get; set; }

        public double PERSEN { get; set; }

        [StringLength(30)]
        public string Jabatan { get; set; }

        [StringLength(50)]
        public string Alamat1 { get; set; }

        [StringLength(50)]
        public string Alamat2 { get; set; }

        public DateTime? tgl_join { get; set; }

        public DateTime? tgl_berhenti { get; set; }

        [StringLength(30)]
        public string Telp { get; set; }

        [StringLength(30)]
        public string HP { get; set; }

        [StringLength(30)]
        public string Pager { get; set; }

        [StringLength(50)]
        public string KTP { get; set; }

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
        public string Sort1_Org { get; set; }

        [StringLength(10)]
        public string Sort2_Org { get; set; }

        [StringLength(10)]
        public string Sort3_Org { get; set; }

        [StringLength(10)]
        public string Sort4_Org { get; set; }

        [StringLength(10)]
        public string Sort5_Org { get; set; }

        [StringLength(10)]
        public string Sort6_Org { get; set; }

        [StringLength(10)]
        public string Sort7_Org { get; set; }

        [StringLength(10)]
        public string Attr1_Org { get; set; }

        [StringLength(10)]
        public string Attr2_Org { get; set; }

        [StringLength(10)]
        public string Attr3_Org { get; set; }

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

        public double Kredit_Limit { get; set; }

        [StringLength(30)]
        public string USERNAME { get; set; }
    }
}
