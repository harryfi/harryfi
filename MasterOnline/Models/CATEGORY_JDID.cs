using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("CATEGORY_JDID")]
    public class CATEGORY_JDID
    {
        [StringLength(50)]
        public string CATEGORY_CODE { get; set; }

        [StringLength(250)]
        public string CATEGORY_NAME { get; set; }

        [StringLength(3)]
        public string CATE_STATE { get; set; }

        [StringLength(3)]
        public string TYPE { get; set; }

        [StringLength(1)]
        public string LEAF { get; set; }

        [StringLength(50)]
        public string PARENT_CODE { get; set; }

        //ADD BY NURUL 27/5/2021
        [StringLength(1)]
        public string IS_LAST_NODE { get; set; }

        [StringLength(50)]
        public string MASTER_CATEGORY_CODE { get; set; }
        //END ADD BY NURUL 27/5/2021

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }
}