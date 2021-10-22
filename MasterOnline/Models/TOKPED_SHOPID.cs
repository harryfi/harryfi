
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

namespace MasterOnline.Models
{
    [Table("TOKPED_SHOPID")]
    public partial class TOKPED_SHOPID
    {
        [StringLength(100)]
        public string SHOPID { get; set; }

        [StringLength(100)]
        public string CUST { get; set; }

        public Int64 ACCOUNTID { get; set; }

        [StringLength(50)]
        public String EMAIL_ACCOUNT { get; set; }

        [StringLength(1)]
        public String STATUS_CHAT { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }
        
        public int IDMARKET { get; set; }
    }

}
