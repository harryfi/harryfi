using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("BRAND_JDID")]
    public class BRAND_JDID
    {
        [Key]
        public string brandId { get; set; }
        public string brandName { get; set; }
    }
}