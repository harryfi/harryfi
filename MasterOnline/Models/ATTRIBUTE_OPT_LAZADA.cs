using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("ATTRIBUTE_OPT_LAZADA")]
    public class ATTRIBUTE_OPT_LAZADA
    {
        [StringLength(50)]
        public string A_NAME { get; set; }

        [StringLength(100)]
        public string O_NAME { get; set; }        

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }
}