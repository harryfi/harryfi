using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("Ekspedisi")]
    public class Ekspedisi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32? RecNum { get; set; }

        [Required]
        [MaxLength(25)]
        public String NamaEkspedisi { get; set; }

        [Required]
        public Boolean Status { get; set; }
    }
}