using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace MasterOnline.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class PickingListDBClass
    {

    }
    public partial class SOT03A
    {
        [Key]
        [StringLength(15)]
        public string NO_BUKTI { get; set; }

        public DateTime? TGL { get; set; }

        [StringLength(20)]
        public string USER_NAME { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }
    }

    public partial class SOT03B
    {
        [StringLength(15)]
        public string NO_BUKTI { get; set; }
        
        [StringLength(15)]
        public string NO_PESANAN { get; set; }

        public DateTime? TGL_PESANAN { get; set; }

        public string PEMBELI { get; set; }

        public string MARKETPLACE { get; set; }

        [StringLength(20)]
        public string USER_NAME { get; set; }

        public DateTime? TGL_INPUT { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

    }

    public partial class SOT03C
    {
        [StringLength(15)]
        public string NO_BUKTI { get; set; }
        
        [StringLength(15)]
        public string NO_PESANAN { get; set; }
        
        [StringLength(20)]
        public string BRG { get; set; }

        public string NAMA_BARANG { get; set; }

        public int? QTY { get; set; }

        [StringLength(20)]
        public string USER_NAME { get; set; }

        public DateTime? TGL_INPUT { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RecNum { get; set; }

    }
}