namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLBatchTable")]
    public partial class GLBatchTable
    {
        public DateTime BatchDate { get; set; }

        public int BatchNo { get; set; }

        [Key]
        public int RECNUM { get; set; }
    }
}
