using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("CATEGORY_SHOPEE")]
    public class CATEGORY_SHOPEE
    {
        [Key]
        [StringLength(50)]
        public string CATEGORY_CODE { get; set; }

        [StringLength(250)]
        public string CATEGORY_NAME { get; set; }

        [StringLength(50)]
        public string PARENT_CODE { get; set; }

        [StringLength(1)]
        public string IS_LAST_NODE { get; set; }

        [StringLength(50)]
        public string MASTER_CATEGORY_CODE { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }
}