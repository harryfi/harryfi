using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterOnline.Models
{
    [Table("TABEL_LOG_GETORDERS")]
    public class TABEL_LOG_GETORDERS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }

        public DateTime TGL { get; set; }

        public string FUNCTION { get; set; }
        public string DBPATHERA { get; set; }
        public string MARKETPLACE { get; set; }
        public string ERRORMSG { get; set; }
    }
}