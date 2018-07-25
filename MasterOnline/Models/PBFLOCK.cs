namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PBFLOCK")]
    public partial class PBFLOCK
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(1)]
        public string JENISFORM { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string INV { get; set; }

        [Required]
        [StringLength(50)]
        public string USER_NAME { get; set; }

        [Required]
        [StringLength(50)]
        public string MACHINE_NAME { get; set; }
    }
}
