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
        public DateTime create_time { get; set; }
        public DateTime pay_time { get; set; }

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
        public DateTime note_update_time { get; set; }
        public DateTime update_time { get; set; }
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

        public string CUST { get; set; }
        public string NAMA_CUST { get; set; }
        public string CONN_ID { get; set; }

        public DateTime? ship_by_date { get; set; }

        //add by nurul 22/3/2021
        public string checkout_shipping_carrier { get; set; }
        //end add by nurul 22/3/2021
    }

}
