
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("TABEL_TRANSAKSI_MIDTRANS")]
    public class TransaksiMidtrans
    {
        [Key]
        [StringLength(50)]
        public string NO_TRANSAKSI { get; set; }

        [Required]
        public long ACCOUNT_ID { get; set; }

        [Required]
        public double VALUE { get; set; }

        [Required]
        [StringLength(2)]
        public string TYPE { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime? TGL_INPUT { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RECNUM { get; set; }

        [Required]
        public int BULAN { get; set; }

        public int? jumlahUser { get; set; }
}
}