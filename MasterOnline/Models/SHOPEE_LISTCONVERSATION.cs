using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;


namespace MasterOnline.Models
{
    [Table("SHOPEE_LISTCONVERSATION")]
    public class SHOPEE_LISTCONVERSATION
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int recnum { get; set; }
        public long recnum { get; set; }

        [StringLength(50)]
        public string conversation_id { get; set; }

        public long last_message_option { get; set; }

        public DateTime last_message_timestamp { get; set; }

        [StringLength(50)]
        public string last_read_message_id { get; set; }

        public string latest_message_content_text { get; set; }

        public string latest_message_content_url { get; set; }

        public long latest_message_from_id { get; set; }

        [StringLength(50)]
        public string latest_message_id { get; set; }

        [StringLength(50)]
        public string latest_message_type { get; set; }

        public string max_general_option_hide_time { get; set; }

        [StringLength(50)]
        public string pinned { get; set; }

        public long shop_id { get; set; }

        public string to_avatar { get; set; }

        public long to_id { get; set; }

        [StringLength(50)]
        public string to_name { get; set; }

        public long unread_count { get; set; }

        [StringLength(50)]
        public string cust { get; set; }
    }
}