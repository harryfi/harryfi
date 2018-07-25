using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.DynamicData;

namespace MasterOnline.Models
{
    [Table("User")]
    public class User
    {
        [Required]
        public Int64 AccountId { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64? UserId { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(50)]
        [DisplayName("Email *")]
        public String Email { get; set; }

        [Required]
        [MaxLength(50)]
        [DisplayName("Username *")]
        public String Username { get; set; }

        [Required]
        [MaxLength(50)]
        [DisplayName("Password *")]
        public String Password { get; set; }

        [NotMapped]
        [DisplayName("Konfirmasi Password *")]
        public String KonfirmasiPassword { get; set; }

        [Required]
        [MaxLength(20)]
        [DisplayName("No. HP *")]
        public String NoHp { get; set; }

        public Boolean Status { get; set; }
    }
}