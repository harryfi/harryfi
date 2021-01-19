using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class Promosi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        [StringLength(50)]
        public string NAMA_PROMOSI { get; set; }

        [StringLength(30)]
        public string NAMA_MARKET { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? TGL_MULAI { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? TGL_AKHIR { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? TGL_INPUT { get; set; }

        [StringLength(100)]
        public string MP_PROMO_ID { get; set; }
    }
}