using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("ATTRIBUTE_OPT_SHOPEE")]
    public class ATTRIBUTE_OPT_SHOPEE
    {
        public ATTRIBUTE_OPT_SHOPEE()
        {
        }
        public ATTRIBUTE_OPT_SHOPEE(string asd,string dsa)
        {
            ACODE = asd;
            OPTION_VALUE = dsa;
        }

        [StringLength(50)]
        public string ACODE { get; set; }

        [StringLength(250)]
        public string OPTION_VALUE { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }
    public class ATTRIBUTE_OPT_SHOPEE_V2
    {
        public ATTRIBUTE_OPT_SHOPEE_V2()
        {
        }
        public ATTRIBUTE_OPT_SHOPEE_V2(string attr_code, string option_name, string option_code)
        {
            ACODE = attr_code;
            OPTION_VALUE = option_code;
            OPTION_NAME = option_name;
        }

        [StringLength(50)]
        public string ACODE { get; set; }

        [StringLength(250)]
        public string OPTION_VALUE { get; set; }
        [StringLength(250)]
        public string OPTION_NAME { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }
}