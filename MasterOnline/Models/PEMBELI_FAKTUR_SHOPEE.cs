namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PEMBELI_FAKTUR_SHOPEE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }

        [StringLength(10)]
        public string PEMBELI { get; set; }

        [StringLength(100)]
        public string NAMA { get; set; }
        
        public string ALAMAT { get; set; }

        [StringLength(100)]
        public string TLP { get; set; }
        
        [StringLength(15)]
        public string FAKTUR { get; set; }

        [StringLength(15)]
        public string PESANAN { get; set; }
        
        [StringLength(100)]
        public string RESI { get; set; }

        [StringLength(100)]
        public string KURIR { get; set; }
    }
}
