namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SOFLOCK")]
    public partial class SOFLOCK
    {
        [Key]
        [StringLength(10)]
        public string NO_BUKTI { get; set; }

        [StringLength(50)]
        public string USER_NAME { get; set; }

        [StringLength(50)]
        public string MACHINE_NAME { get; set; }
    }
}
