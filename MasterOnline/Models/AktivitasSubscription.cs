using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("AktivitasSubscription")]
    public class AktivitasSubscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        [MaxLength(30)]
        public string Account { get; set; }

        [MaxLength(50)]
        public string Email { get; set; }

        [MaxLength(2)]
        public string TipeSubs { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TanggalBayar { get; set; }

        public double Nilai { get; set; }

        [MaxLength(50)]
        public string TipePembayaran { get; set; }
        
        public DateTime? DrTGL { get; set; }

        public DateTime? SdTGL { get; set; }

    }
}