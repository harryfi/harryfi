namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class POF01
    {
        [Key]
        [StringLength(10)]
        public string DIVISION_NO { get; set; }

        [StringLength(40)]
        public string DESCRIPTION { get; set; }

        public DateTime? TGLINPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        [Required]
        [StringLength(4)]
        public string GD { get; set; }
    }
}
