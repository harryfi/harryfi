using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public class TEMP_TOKPED_ORDERS
    {
        public string fs_id { get; set; }

        [Key]
        [Column(Order = 0)]
        public string order_id { get; set; }

        public bool? accept_partial { get; set; }

        public string invoice_ref_num { get; set; }

        [Key]
        [Column(Order = 1)]
        public int product_id { get; set; }

        public string product_name { get; set; }

        public int? product_quantity { get; set; }

        public string product_notes { get; set; }

        public double? product_weight { get; set; }

        public double? product_total_weight { get; set; }

        public int? product_price { get; set; }

        public int? product_total_price { get; set; }

        public string product_currency { get; set; }

        public string product_sku { get; set; }

        public int? products_fulfilled_product_id { get; set; }

        public int? products_fulfilled_quantity_deliver { get; set; }

        public int? products_fulfilled_quantity_reject { get; set; }

        public string device_type { get; set; }

        public int? buyer_id { get; set; }

        public string buyer_name { get; set; }

        public string buyer_phone { get; set; }

        public string buyer_email { get; set; }

        public int? shop_id { get; set; }

        public int? payment_id { get; set; }

        public string recipient_name { get; set; }

        public string recipient_address_address_full { get; set; }

        public string recipient_address_district { get; set; }

        public string recipient_address_city { get; set; }

        public string recipient_address_province { get; set; }

        public string recipient_address_country { get; set; }

        public string recipient_address_postal_code { get; set; }

        public int? recipient_address_district_id { get; set; }

        public int? recipient_address_city_id { get; set; }

        public int? recipient_address_province_id { get; set; }

        public string recipient_address_geo { get; set; }

        public string recipient_phone { get; set; }

        public int? logistics_shipping_id { get; set; }

        public string logistics_shipping_agency { get; set; }

        public string logistics_service_type { get; set; }

        public int? amt_ttl_product_price { get; set; }

        public int? amt_shipping_cost { get; set; }

        public int? amt_insurance_cost { get; set; }

        public int? amt_ttl_amount { get; set; }

        public int? amt_voucher_amount { get; set; }

        public int? amt_toppoints_amount { get; set; }

        public string dropshipper_info_name { get; set; }

        public string dropshipper_info_phone { get; set; }

        public decimal? voucher_info_voucher_type { get; set; }

        public string voucher_info_voucher_code { get; set; }

        public int? order_status { get; set; }

        public DateTime create_time { get; set; }

        public string custom_fields_awb { get; set; }

        [Key]
        [Column(Order = 2)]
        public string conn_id { get; set; }
        public string CUST { get; set; }
        public string NAMA_CUST { get; set; }

    }

}
