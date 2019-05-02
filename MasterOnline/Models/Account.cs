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
    [Table("Account")]
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64 AccountId { get; set; }

        [MaxLength(50)]
        [Required]
        public String UserId { get; set; }

        public string VCode { get; set; }

        [MaxLength(50)]
        [Required]
        [EmailAddress(ErrorMessage = "Masukkan email yang valid!")]
        public String Email { get; set; }

        [MaxLength(30)]
        [Required]
        [DisplayName("Nama Lengkap")]
        public String Username { get; set; }

        [Required]
        public String Password { get; set; }

        [NotMapped]
        public String ConfirmPassword { get; set; }

        [MaxLength(30)]
        [Required]
        [DisplayName("No. HP")]
        public String NoHp { get; set; }

        [MaxLength(255)]
        public String DatabasePathMo { get; set; }

        [MaxLength(255)]
        public String PhotoKtpUrl { get; set; }

        [NotMapped]
        public HttpPostedFileBase ImageFile { get; set; }

        [MaxLength(100)]
        [Required]
        [DisplayName("Nama Toko Online")]
        public String NamaTokoOnline { get; set; }

        [MaxLength(255)]
        public String DatabasePathErasoft { get; set; }

        public Boolean Status { get; set; }

        [StringLength(2)]
        public string KODE_SUBSCRIPTION { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TGL_SUBSCRIPTION { get; set; }

        public string PhotoKtpBase64 { get; set; }

        [StringLength(100)]
        public string TOKEN_CC { get; set; }

        [StringLength(100)]
        public string KODE_REFERRAL { get; set; }
        
        public DateTime? TGL_DAFTAR { get; set; }

        public int? jumlahUser { get; set; }

        //add by nurul 12/3/2019
        public DateTime? tgl_approve { get; set; }
        //end add by nurul 12/3/2019

        //add by Iman 15/04/2019
        [Column(TypeName = "date")]
        public DateTime? tgl_email1 { get; set; }       

        //add by Iman 15/04/2019
        [Column(TypeName = "date")]
        public DateTime? tgl_email2 { get; set; }
        //end add by Iman 15/04/2019

        public DateTime? LAST_LOGIN_DATE { get; set; }
    }
}