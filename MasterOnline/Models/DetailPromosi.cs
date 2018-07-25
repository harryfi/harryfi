using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class DetailPromosi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        [StringLength(20)]
        public string KODE_BRG { get; set; }

        public int? RecNumPromosi { get; set; }

        public double HARGA_NORMAL { get; set; }

        public double HARGA_PROMOSI { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TGL_INPUT { get; set; }
    }
}