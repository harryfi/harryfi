using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("CATEGORY_SHOPIFY")]
    public class CATEGORY_SHOPIFY
    {
        [Key]
        [StringLength(50)]
        public string ID_CATEGORY { get; set; }

        [StringLength(50)]
        public string NAME { get; set; }

        [StringLength(50)]
        public string ID_PARENT { get; set; }

        [StringLength(10)]
        public string LEVEL_DEPTH { get; set; }

        [StringLength(10)]
        public string POSITION { get; set; }

        [StringLength(1)]
        public string ACTIVE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }
}