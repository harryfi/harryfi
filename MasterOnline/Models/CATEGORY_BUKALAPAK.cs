using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("CATEGORY_BUKALAPAK")]
    public class CATEGORY_BUKALAPAK
    {
        [Key]
        public string CATEGORY_ID { get; set; }

        public string NAME { get; set; }
        public bool LEAF { get; set; }
        public string PARENT_ID { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }

    }
}