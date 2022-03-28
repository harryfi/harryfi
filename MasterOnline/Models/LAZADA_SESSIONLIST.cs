using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace MasterOnline.Models
{
    [Table("LAZADA_SESSIONLIST")]
    public class LAZADA_SESSIONLIST
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long recnum { get; set; }

        public string summary { get; set; }
        public long unread_count { get; set; }
        public string last_message_id { get; set; }

        public string head_url { get; set; }

        public string self_position { get; set; }

        public DateTime last_message_time { get; set; }

        public string site_id { get; set; }

        [StringLength(50)]
        public string session_id { get; set; }

        public string title { get; set; }
        public string buyer_id { get; set; }
        public string to_position { get; set; }
        public string cust { get; set; }
        public string tags { get; set; }
    }
}