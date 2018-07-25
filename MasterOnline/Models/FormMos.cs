using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("FormMos")]
    public class FormMos
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 ScrId { get; set; }

        [Required]
        public Int32 ParentId { get; set; }

        [Required]
        [MaxLength(30)]
        public String NamaForm { get; set; }

        [MaxLength(150)]
        public String Url { get; set; }

        [MaxLength(20)]
        public String Icon { get; set; }

        public Boolean HaveChild { get; set; }

        public Boolean HaveChildFromChild { get; set; }

        public Boolean Show { get; set; }
    }
}