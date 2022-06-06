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
        public string CATEGORY_CODE { get; set; }

        public string CATEGORY_NAME { get; set; }

        public string PARENT_CODE { get; set; }

        public string IS_LAST_NODE { get; set; }

        public string SIZE_CHART { get; set; }
        public string COD { get; set; }
        public string CERTIFICATION { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
        public string CUST { get; set; }
    }
    [Table("TABEL_MAPPING_TOKPED")]
    public class TABEL_MAPPING_TOKPED
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }
        public string DBPATHERA { get; set; }
        public string SHOPID { get; set; }
        public string CUST { get; set; }

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