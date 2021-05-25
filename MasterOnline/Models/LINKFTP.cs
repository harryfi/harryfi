namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LINKFTP")]
    public partial class LINKFTP
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        //[Key]
        //[RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$")]
        [StringLength(15)]
        public string IP { get; set; }

        [StringLength(50)]
        public string LOGIN { get; set; }

        [StringLength(20)]
        public string PASSWORD { get; set; }

        public string STATUS_FTP { get; set; }
        public string PPN { get; set; }

        [StringLength(5)]
        public string KODE_TRANSAKSI { get; set; }

        public TimeSpan? JAM1 { get; set; }
        public TimeSpan? JAM2 { get; set; }
        public TimeSpan? JAM3 { get; set; }
        public TimeSpan? JAM4 { get; set; }
        public TimeSpan? JAM5 { get; set; }
    }

    [Table("TEMP_SHOPEE_BRAND")]
    public partial class TEMP_SHOPEE_BRAND
    {
        [Key]
        [Column(Order = 0)]
        public string CATEGORY_CODE { get; set; }

        [Key]
        [Column(Order = 1)]
        public string BRAND_ID { get; set; }

        public string NAME { get; set; }

    }
}