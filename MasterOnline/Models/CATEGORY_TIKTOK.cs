using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("CATEGORY_TIKTOK")]
    public class CATEGORY_TIKTOK
    {
        [Key]
        [StringLength(50)]
        public string CATEGORY_CODE { get; set; }

        [StringLength(250)]
        public string CATEGORY_NAME { get; set; }

        [StringLength(50)]
        public string PARENT_CODE { get; set; }

        [StringLength(1)]
        public string IS_LAST_NODE { get; set; }

        [StringLength(50)]
        public string MASTER_CATEGORY_CODE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }
    [Table("TABEL_MAPPING_TIKTOK")]
    public class TABEL_MAPPING_TIKTOK
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }
        public string DBPATHERA { get; set; }
        public string SHOPID { get; set; }
        public string CUST { get; set; }

    }
    [Table("TABEL_MAPPING_LAZADA")]
    public class TABEL_MAPPING_LAZADA
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }
        public string DBPATHERA { get; set; }
        public string SHOPID { get; set; }
        public string CUST { get; set; }

    }

    [Table("TABEL_CHECK_HANGFIRE")]
    public class TABEL_CHECK_HANGFIRE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }
        public string DBPATHERA { get; set; }
        public string ARG { get; set; }
        public DateTime TGL { get; set; }

    }
}