using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace MasterOnline.Models
{
    [Table("SHOPEE_MESSAGE")]
    public class SHOPEE_MESSAGE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long recnum { get; set; }

        public string message { get; set; }

        [StringLength(50)]
        public string request_id { get; set; }

        [StringLength(50)]
        public string conversation_id { get; set; }

        public DateTime created_timestamp { get; set; }

        public long from_id { get; set; }

        public long from_shop_id { get; set; }

        [StringLength(50)]
        public string message_id { get; set; }

        public long message_option { get; set; }

        [StringLength(50)]
        public string message_type { get; set; }

        [StringLength(50)]
        public string region { get; set; }

        [StringLength(50)]
        public string source { get; set; }

        [StringLength(50)]
        public string status { get; set; }

        public long to_id { get; set; }

        public long to_shop_id { get; set; }

        public long content_shop_id { get; set; }

        public long content_chat_product_infos_item_id { get; set; }

        public long content_chat_product_infos_shop_id { get; set; }

        [StringLength(50)]
        public string content_chat_product_infos_name { get; set; }

        public string content_chat_product_infos_thumb_url { get; set; }

        [StringLength(50)]
        public string content_chat_product_infos_price { get; set; }

        public long content_chat_product_infos_quantity { get; set; }

        public long content_chat_product_infos_snap_shop_id { get; set; }

        [StringLength(50)]
        public string content_chat_product_infos_model_name { get; set; }

        [StringLength(50)]
        public string content_chat_product_infos_price_before_discount { get; set; }

        public string content_voucher_id { get; set; }

        public string content_voucher_code { get; set; }

        public string content_source { get; set; }

        public string content_url { get; set; }

        public long content_file_server_id { get; set; }

        public string content_text { get; set; }

        [StringLength(50)]
        public string cust { get; set; }


        public string thumb_url { get; set; }
        public long thumb_height { get; set; }
        public long thumb_width { get; set; }
        public long duration_seconds { get; set; }
    }
}