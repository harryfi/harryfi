namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PARTNER_API_LOG_ERROR
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int log_id { get; set; }

        public int fs_id { get; set; }

        [StringLength(50)]
        public string Modul { get; set; }

        [StringLength(50)]
        public string No_Bukti { get; set; }

        [StringLength(200)]
        public string Keterangan { get; set; }

        public DateTime? Created_Date { get; set; }

        public string JSON_String { get; set; }
        public bool Status { get; set; }
    }
    public partial class PARTNER_API_LOG_ERROR_VIEW
    {
        public int log_id { get; set; }
        public string Nama_Partner { get; set; }
        public string Modul { get; set; }
        public string No_Bukti { get; set; }
        public string Keterangan { get; set; }
        public DateTime? Created_Date { get; set; }
        public string JSON_String { get; set; }
        public bool Status { get; set; }
    }
}