namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STT07A
    {
        [Key]
        [StringLength(10)]
        public string NO_BUKTI { get; set; }

        public DateTime TGL { get; set; }
    }
}
