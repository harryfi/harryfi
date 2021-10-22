
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

namespace MasterOnline.Models
{
    [Table("TOKPED_LISTCHAT")]
    public partial class TOKPED_LISTCHAT
    {
        [StringLength(10)]
        public string CUST { get; set; }

        [StringLength(50)]
        public string msg_id { get; set; }

        [StringLength(50)]
        public string reply_id { get; set; }

        [StringLength(50)]
        public string sender_id { get; set; }

        [StringLength(100)]
        public string sender_name { get; set; }
        
        public string msg { get; set; }

        //public DateTime? reply_time { get; set; }
        public DateTime reply_time { get; set; }

        public int read_status { get; set; }

        public DateTime? read_time { get; set; }

        public int status_ { get; set; }

        public int message_is_read { get; set; }

        public int is_opposite { get; set; }

        public int is_first_reply { get; set; }

        public int is_reported { get; set; }

        [StringLength(50)]
        public string attachment_id { get; set; }

        public int attachment_type { get; set; }

        public string image_url { get; set; }

        [StringLength(200)]
        public string product_id { get; set; }
        
        public string fallback_attachment_html { get; set; }
        
        public string fallback_attachment_message { get; set; }

        public DateTime tglinput { get; set; }

        //public string shop_id { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }

        public string shop_id { get; set; }

    }

}
