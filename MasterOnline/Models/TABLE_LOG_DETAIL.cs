namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TABLE_LOG_DETAIL
    {
        public string LOG_FILE { get; set; }

        [StringLength(100)]
        public string VARIABLE_1 { get; set; }

        public string VARIABLE_2 { get; set; }

        public string TEXT_1 { get; set; }

        [StringLength(200)]
        public string TEXT_2 { get; set; }

        [StringLength(200)]
        public string TEXT_3 { get; set; }

        public DateTime TGL_INPUT { get; set; }

        [StringLength(20)]
        public string USERNAME { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }
    }
}
