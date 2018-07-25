namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GLFLEVEL")]
    public partial class GLFLEVEL
    {
        public int ID { get; set; }

        public double P1 { get; set; }

        public double P2 { get; set; }

        public double P3 { get; set; }

        public double P4 { get; set; }

        public double P5 { get; set; }

        public double J1 { get; set; }

        public double J2 { get; set; }

        public double J3 { get; set; }

        public double J4 { get; set; }

        public double J5 { get; set; }
    }
}
