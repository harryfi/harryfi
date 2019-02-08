using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("ATTRIBUTE_OPT_TOKPED")]
    public class ATTRIBUTE_OPT_TOKPED
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
        public int VARIANT_ID { get; set; }
        public int UNIT_ID { get; set; }
        public int VALUE_ID { get; set; }

        [StringLength(50)]
        public string VALUE { get; set; }
        [StringLength(50)]
        public string HEX_CODE { get; set; }
        [StringLength(200)]
        public string ICON { get; set; }

    }
}