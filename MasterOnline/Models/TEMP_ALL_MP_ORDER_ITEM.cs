using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TEMP_ALL_MP_ORDER_ITEM
    {
        [StringLength(20)]
        public string BRG { get; set; }

        [StringLength(50)]
        public string CONN_ID { get; set; }
    }
}
