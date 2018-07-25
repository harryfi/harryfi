namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLRF02
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short URUT { get; set; }

        public double HASIL { get; set; }
    }
}
