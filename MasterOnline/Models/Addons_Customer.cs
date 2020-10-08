using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    [Table("Addons_Customer")]
    public class Addons_Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

        [StringLength(50)]
        public string NamaAddons { get; set; }

        [StringLength(100)]
        public string Account { get; set; }

        [StringLength(100)]
        public string NamaTokoOnline { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? TglSubscription { get; set; }

        public int Harga { get; set; }

        public DateTime? TGL_DAFTAR { get; set; }

        [StringLength(50)]
        public string ID_ADDON { get; set; }

        [StringLength(50)]
        public string ID_TRANS_MIDTRANS { get; set; }

        [StringLength(1)]
        public string STATUS { get; set; }
    }
}