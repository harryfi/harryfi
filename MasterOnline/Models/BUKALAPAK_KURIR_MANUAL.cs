using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasterOnline.Models
{
    [Table("BUKALAPAK_KURIR_MANUAL")]
    public class BUKALAPAK_KURIR_MANUAL
    {
        [Key]
        public String KURIR { get; set; }
        public String NAMA { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }
    }
}