using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIT04A
    {
        [Key]
        [StringLength(10)]
        public string NO_BUKTI { get; set; }

        [Required]
        public DateTime? TGL_KIRIM { get; set; }

        [StringLength(50)]
        public string KURIR { get; set; }

        [StringLength(50)]
        public string NAMA_EKSPEDISI { get; set; }

        [StringLength(50)]
        public string NAMA_KURIR { get; set; }

        public DateTime? JAM_KIRIM { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RECNUM { get; set; }
    }
}
