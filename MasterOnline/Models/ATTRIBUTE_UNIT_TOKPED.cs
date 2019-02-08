using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("ATTRIBUTE_UNIT_TOKPED")]
    public class ATTRIBUTE_UNIT_TOKPED
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
        public int VARIANT_ID { get; set; }
        public int UNIT_ID { get; set; }
        [StringLength(100)]
        public string UNIT_NAME { get; set; }
        [StringLength(75)]
        public string UNIT_SHORT_NAME { get; set; }
    }
}