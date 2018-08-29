using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("CATEGORY_LAZADA")]
    public class CATEGORY_LAZADA
    {
        [Key]
        [StringLength(50)]
        public string CATEGORY_ID { get; set; }

        [StringLength(150)]
        public string NAME { get; set; }

        //[StringLength(1)]
        public bool LEAF { get; set; }

        [StringLength(50)]
        public string PARENT_ID { get; set; }        

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }
}