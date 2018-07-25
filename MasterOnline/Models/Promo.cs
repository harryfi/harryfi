using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("Promo")]
    public class Promo
    {
        [StringLength(50)]
        public string EMAIL_REF { get; set; }

        [StringLength(20)]
        public string HP_REF { get; set; }

        [StringLength(50)]
        public string EMAIL { get; set; }

        [StringLength(20)]
        public string HP { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TGL { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TGL_REPLY { get; set; }

        public bool PAID { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }
}