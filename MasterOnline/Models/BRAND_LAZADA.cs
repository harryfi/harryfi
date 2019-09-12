using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("BRAND_LAZADA")]
    public class BRAND_LAZADA
    {
        [Key]
        public string brand_id { get; set; }
        public string name { get; set; }
        public string global_identifier { get; set; }
        public string name_en { get; set; }
    }
}