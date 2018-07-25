using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("Subscription")]
    public class Subscription
    {
        [Key]
        [StringLength(2)]
        public string KODE { get; set; }

        [StringLength(30)]
        public string KETERANGAN { get; set; }

        public short JUMLAH_MP { get; set; }

        public int JUMLAH_PESANAN { get; set; }

        public double HARGA { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }
}