using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("Provinsi")]
    public class Provinsi
    {
        [Key]
        [Required]
        [MaxLength(2)]
        public String KodeProv { get; set; }

        [MaxLength(50)]
        [Required]
        public String NamaProv { get; set; }

        [NotMapped]
        public virtual IList<KabupatenKota> KabupatenKota { get; set; }
    }
}