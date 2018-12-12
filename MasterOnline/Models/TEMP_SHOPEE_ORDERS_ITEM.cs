using Newtonsoft.Json;

namespace MasterOnline
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public class TEMP_SHOPEE_ORDERS_ITEM
    {
        [Key, Column(Order = 0)]
        public string ordersn { get; set; }
        public float weight { get; set; }
        public string item_name { get; set; }
        public bool is_wholesale { get; set; }
        public string item_sku { get; set; }
        public string variation_discounted_price { get; set; }
        [Key, Column(Order = 2)]
        public long variation_id { get; set; }
        public string variation_name { get; set; }
        [Key, Column(Order = 1)]
        public int item_id { get; set; }
        public int variation_quantity_purchased { get; set; }
        public string variation_sku { get; set; }
        public string variation_original_price { get; set; }
    }

}
