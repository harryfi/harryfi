namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class API_LOG_MARKETPLACE_PER_ITEM
    {
        public int IDMARKET { get; set; }

        [StringLength(10)]
        public string CUST { get; set; }

        public string REQUEST_ACTION { get; set; }

        public DateTime REQUEST_DATETIME{ get; set; }

        [StringLength(20)]
        public string REQUEST_STATUS { get; set; }

        public string REQUEST_ATTRIBUTE_1 { get; set; }
        public string REQUEST_RESULT { get; set; }
        public string REQUEST_EXCEPTION { get; set; }
    }
}
