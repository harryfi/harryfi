namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PARTNER_API
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int fs_id { get; set; }

        [StringLength(25)]
        public string Nama_Partner { get; set; }

        [StringLength(100)]
        public string Access_Token { get; set; }

        [StringLength(100)]
        public string Refresh_Token { get; set; }

        [StringLength(100)]
        public string ClientId { get; set; }

        [StringLength(100)]
        public string ClientSecret { get; set; }

        [StringLength(100)]
        public string Session { get; set; }

        [StringLength(100)]
        public string Host { get; set; }

        [StringLength(50)]
        public string Token { get; set; }

        public DateTime? Token_ExpiredDate { get; set; }

        [StringLength(50)]
        public string IP_Address { get; set; }

        [StringLength(10)]
        public string DatabaseId { get; set; }

        public bool? Status { get; set; }
        public bool? isPaid { get; set; }

        public DateTime? Session_ExpiredDate { get; set; }

        [StringLength(100)]
        public string OAuthCallbackCode { get; set; }
        public int? PartnerId { get; set; }
    }
}