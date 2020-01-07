using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class QOH_PER_GD
    {
        public string BRG { get; set; }
        public string GD { get; set; }
        public string Nama_Gudang { get; set; }
        public double QOH { get; set; }
        //add by nurul 7/1/2020
        public double QSO { get; set; }
        public double SISA { get; set; }
        //end add by nurul 7/1/2020
    }
    public partial class QOO_PER_BRG
    {
        public string BRG { get; set; }
        public string GD { get; set; }
        public double QSO { get; set; }
    }
}
