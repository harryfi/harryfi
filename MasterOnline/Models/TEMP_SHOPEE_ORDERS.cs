using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public class TEMP_SHOPEE_ORDERS
    {
        public string note { get; set; }
        public string estimated_shipping_fee { get; set; }
        public string payment_method { get; set; }
        public string escrow_amount { get; set; }
        public string message_to_seller { get; set; }
        public string shipping_carrier { get; set; }
        public string currency { get; set; }
        public int create_time { get; set; }
        public int pay_time { get; set; }

        public string Recipient_Address_town { get; set; }
        public string Recipient_Address_city { get; set; }
        public string Recipient_Address_name { get; set; }
        public string Recipient_Address_district { get; set; }
        public string Recipient_Address_country { get; set; }
        public string Recipient_Address_zipcode { get; set; }
        public string Recipient_Address_full_address { get; set; }
        public string Recipient_Address_phone { get; set; }
        public string Recipient_Address_state { get; set; }

        public int days_to_ship { get; set; }
        public string tracking_no { get; set; }
        public string order_status { get; set; }
        public int note_update_time { get; set; }
        public int update_time { get; set; }
        public bool goods_to_declare { get; set; }
        public string total_amount { get; set; }
        public string service_code { get; set; }
        public string country { get; set; }
        public string actual_shipping_cost { get; set; }
        public bool cod { get; set; }

        [Key]
        public string ordersn { get; set; }
        public string dropshipper { get; set; }
        public string buyer_username { get; set; }

    }

}
