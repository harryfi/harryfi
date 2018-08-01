using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("ATTRIBUTE_OPT_BLIBLI")]
    public class ATTRIBUTE_OPT_BLIBLI
    {
        [StringLength(50)]
        public string ACODE { get; set; }

        [StringLength(50)]
        public string ATYPE { get; set; }

        [StringLength(250)]
        public string ANAME { get; set; }

        [StringLength(250)]
        public string OPTION_VALUE { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }
}