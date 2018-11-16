namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class QOH_QOO_ALL_ITEM
    {
        public string JENIS { get; set; }
        public string BRG { get; set; }
        public int JUMLAH { get; set; }
    }
}
