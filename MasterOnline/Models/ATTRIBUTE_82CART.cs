using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("ATTRIBUTE_82CART")]
    public class ATTRIBUTE_82CART
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
        
        [StringLength(20)]
        public string id_attribute_group { get; set; }

        [StringLength(50)]
        public string group_name { get; set; }

        [StringLength(50)]
        public string id_attribute { get; set; }

        [StringLength(20)]
        public string color_attribute { get; set; }

        [StringLength(50)]
        public string attribute_name { get; set; }

        [StringLength(100)]
        public string account_name { get; set; }

        
    }
}