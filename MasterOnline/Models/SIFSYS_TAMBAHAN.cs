using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("SIFSYS_TAMBAHAN")]
    public class SIFSYS_TAMBAHAN
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        [StringLength(30)]
        public string PERSON { get; set; }

        [Required]
        [StringLength(50)]
        public string EMAIL { get; set; }

        [Required]
        [StringLength(30)]
        public string TELEPON { get; set; }

        [Required]
        [StringLength(7)]
        public string KODEPOS { get; set; }

        [Required]
        [StringLength(4)]
        public string KODEKABKOT { get; set; }

        [Required]
        [StringLength(2)]
        public string KODEPROV { get; set; }

        [StringLength(50)]
        public string NAMA_KABKOT { get; set; }

        [StringLength(50)]
        public string NAMA_PROV { get; set; }
    }
}