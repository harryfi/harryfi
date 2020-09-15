using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Web.Mvc;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STF03C
    {
        [StringLength(20)]
        public string BRG_ACUAN { get; set; }

        [StringLength(20)]
        public string BRG { get; set; }

        [StringLength(50)]
        public string USERNAME { get; set; }

        public DateTime? TGL_EDIT { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? RECNUM { get; set; }
    }
}
