namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class LOG_IMPORT_FAKTUR
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }

        [StringLength(100)]
        public string UPLOADER { get; set; }

        [StringLength(100)]
        public string LAST_FAKTUR_UPLOADED { get; set; }
        
        public DateTime UPLOAD_DATETIME{ get; set; }

        public DateTime LAST_FAKTUR_UPLOADED_DATETIME{ get; set; }

        public string CUST { get; set; }
        
        public string LOG_FILE { get; set; }
    }
}
