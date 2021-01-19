using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("Tutorial_Header")]
    public class Tutorial_Header
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? IdKategori { get; set; }

        [StringLength(50)]
        public string Nama_Kategori { get; set; }
    }
}