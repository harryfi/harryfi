using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("Admin")]
    public class Admin
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 AdminId { get; set; }

        [Required]
        [MaxLength(50)]
        public String Username { get; set; }

        [Required]
        [MaxLength(50)]
        [EmailAddress]
        public String Email { get; set; }

        [Required]
        [MaxLength(50)]
        public String Password { get; set; }
    }
    public class SupportLogin
    {
        public Int32 AdminId { get; set; }
        public String Username { get; set; }
        public String Email { get; set; }
        public String Password { get; set; }
        public List<string> AccountList { get; set; }
        public string SelectedAccount { get; set; }
    }
}