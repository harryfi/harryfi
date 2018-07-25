namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GLFTEMP2
    {
        public int ID { get; set; }

        public short LKS { get; set; }

        [StringLength(15)]
        public string NO_REK { get; set; }

        [StringLength(1)]
        public string DK { get; set; }

        public double? NILAI { get; set; }
    }
}
