namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TEMP_UPLOAD_EXCEL_BAYAR
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }
        
        public string NAMA_FILE { get; set; }

        [StringLength(100)]
        public string CUST { get; set; }

        [StringLength(100)]
        public string MARKETPLACE { get; set; }

        [StringLength(200)]
        public string NOREF { get; set; }

        public DateTime TGL { get; set; }

        public double BAYAR { get; set; }

        public double POTONGAN { get; set; }

        public double NILAI_REF { get; set; }

        public double NILAI_LAIN { get; set; }
        
        public string KET { get; set; }

        public bool SUDAH_INPUT { get; set; }

        public DateTime TGL_INPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

    }
}
