namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class API_LOG_MARKETPLACE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RECNUM { get; set; }

        [StringLength(10)]
        public string CUST { get; set; }

        [StringLength(100)]
        public string CUST_ATTRIBUTE_1 { get; set; }

        [StringLength(100)]
        public string CUST_ATTRIBUTE_2 { get; set; }

        [StringLength(100)]
        public string CUST_ATTRIBUTE_3 { get; set; }

        [StringLength(100)]
        public string CUST_ATTRIBUTE_4 { get; set; }

        [StringLength(100)]
        public string CUST_ATTRIBUTE_5 { get; set; }

        [StringLength(100)]
        public string MARKETPLACE { get; set; }

        public string REQUEST_ID { get; set; }

        public string REQUEST_ACTION { get; set; }

        public DateTime REQUEST_DATETIME { get; set; }

        [StringLength(20)]
        public string REQUEST_STATUS { get; set; }

        public string REQUEST_ATTRIBUTE_1 { get; set; }

        public string REQUEST_ATTRIBUTE_2 { get; set; }

        public string REQUEST_ATTRIBUTE_3 { get; set; }

        public string REQUEST_ATTRIBUTE_4 { get; set; }

        public string REQUEST_ATTRIBUTE_5 { get; set; }

        public string REQUEST_RESULT { get; set; }

        public string REQUEST_EXCEPTION { get; set; }

    }
}
