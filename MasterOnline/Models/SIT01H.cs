using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SIT01H
    {
        [StringLength(15)]
        public string NO_BUKTI { get; set; }

        [StringLength(20)]
        public string BRG { get; set; }

        public double? QTY { get; set; }

        public double? HARGA { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        public DateTime? TGL_EDIT { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RECNUM { get; set; }

        [StringLength(4)]
        public string GD { get; set; }
    }
}
