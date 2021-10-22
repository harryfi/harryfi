
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

namespace MasterOnline.Models
{
    [Table("TOKPED_LISTMESSAGE")]
    public partial class TOKPED_LISTMESSAGE
    {
        [StringLength(10)]
        public string CUST { get; set; }

        [StringLength(100)]
        public string message_key { get; set; }

        [StringLength(50)]
        public string msg_id { get; set; }

        [StringLength(50)]
        public string contact_id { get; set; }

        [StringLength(50)]
        public string contact_role { get; set; }

        [StringLength(100)]
        public string attributes_name { get; set; }

        [StringLength(100)]
        public string attributes_tag { get; set; }

        public string attributes_thumbnail { get; set; }

        public string last_reply_msg { get; set; }

        public DateTime last_reply_time { get; set; }

        public int read_status { get; set; }

        public int unreads { get; set; }

        public int pin_status { get; set; }

        public DateTime tglinput { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecNum { get; set; }

        public string shop_id { get; set; }

    }

}
