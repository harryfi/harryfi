using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace MasterOnline.Models
{
    [Table("LAZADA_MESSAGES")]
    public class LAZADA_MESSAGES
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long recnum { get; set; }
        public string last_message_id { get; set; }
        
        public string session_id { get; set; }
        public string from_account_type { get; set; }

        public string message_id { get; set; }
        
        public string type { get; set; }

        public string txt { get; set; }

        public string imgUrl { get; set; }

        
        public string osskey { get; set; }


        public string smallImgUrl { get; set; }

        public string itemId { get; set; }

        [StringLength(50)]
        public string price { get; set; }

        [StringLength(50)]
        public string oldPrice { get; set; }

        public string discount { get; set; }

        public string iconUrl { get; set; }

        public string title { get; set; }
        public string skuId { get; set; }

        public string content_order { get; set; }
        public string status_order { get; set; }

        public string orderId { get; set; }

        public string width { get; set; }

        public string height { get; set; }

        public string to_account_id { get; set; }
        public string to_account_type { get; set; }

        public DateTime send_time { get; set; }
        public string auto_reply { get; set; }
        public string cust { get; set; }
        public string actionUrl { get; set; }
        public string site_id { get; set; }
        public string template_id { get; set; }
        public string status { get; set; }
        public string from_account_id { get; set; }
    }
}