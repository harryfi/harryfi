namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFBATCH")]
    public partial class GLFBATCH
    {
        [Key]
        [Column(Order = 0)]
        public DateTime TGL { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LAST_BATCH_NO { get; set; }
    }
}
