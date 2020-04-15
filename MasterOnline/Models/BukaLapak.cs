using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class BukaLapak
    {
    }
    public class BrgViewModel
    {
        public string key { get; set; }
        public string user { get; set; }
        public string kdBrg { get; set; }
        public string harga { get; set; }
        public string qty { get; set; }
        public string deskripsi { get; set; }
        public string nama { get; set; }
        public string nama2 { get; set; }
        public string weight { get; set; }
        public string height { get; set; }
        public string length { get; set; }
        public string width { get; set; }
        public string imageUrl { get; set; }
        public string imageUrl2 { get; set; }
        public string imageUrl3 { get; set; }
        public string imageId { get; set; }
        public string imageId2 { get; set; }
        public string imageId3 { get; set; }
        public string merk { get; set; }
        public string token { get; set; }
        //public string userBL { get; set; }
        public string appkey { get; set; }
        public string appsecret { get; set; }
        public string idMarket { get; set; }
        public bool activeProd { get; set; }
        //add 6/9/2019, 5 gambar
        public string imageUrl4 { get; set; }
        public string imageUrl5 { get; set; }
        public string imageId4 { get; set; }
        public string imageId5 { get; set; }
        //end add 6/9/2019, 5 gambar
    }
    public class BindingBase
    {
        public int status { get; set; }
        public string message { get; set; }
        public int recordCount { get; set; }
        public int exception { get; set; }
        public int totalData { get; set; }
        public int nextPage { get; set; }
    }

    public class BindingBase82Cart
    {
        public int status { get; set; }
        public string message { get; set; }
        public int recordCount { get; set; }
        public int exception { get; set; }
        public int totalData { get; set; }
        public int nextPage { get; set; }
        public string id_category { get; set; }
        public string name_category { get; set; }
        public string id_manufacture { get; set; }
        public string name_manufacture { get; set; }
    }

    public class BukaLapakResponse
    {
        public string status { get; set; }
        public string message { get; set; }
    }
    public class BukaLapakRes : BukaLapakResponse
    {
        public string id { get; set; }
    }
    public class CreateProductBukaLapak : BukaLapakResponse
    {
        public Product_Detail product_detail { get; set; }
    }
    #region access key
    public class AccessKeyBL : BukaLapakResponse
    {
        //public string status { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public bool confirmed { get; set; }
        public string token { get; set; }
        public string email { get; set; }
        public string omnikey { get; set; }
        //public string message { get; set; }

    }
    #endregion
    #region create product
    public class Product_Detail
    {
        //public Deal_Info deal_info { get; set; }
        public string deal_request_state { get; set; }
        public long price { get; set; }
        public long category_id { get; set; }
        public string category { get; set; }
        public string[] category_structure { get; set; }
        public string seller_username { get; set; }
        public string seller_name { get; set; }
        public long seller_id { get; set; }
        public string seller_avatar { get; set; }
        public string seller_level { get; set; }
        public string seller_level_badge_url { get; set; }
        public string seller_delivery_time { get; set; }
        //public int seller_positive_feedback { get; set; }
        //public int seller_negative_feedback { get; set; }
        public string seller_term_condition { get; set; }
        public object seller_alert { get; set; }
        public bool for_sale { get; set; }
        public object[] state_description { get; set; }
        public bool premium_account { get; set; }
        public bool brand { get; set; }
        public bool top_merchant { get; set; }
        //public Last_Order_Schedule last_order_schedule { get; set; }
        //public Seller_Voucher seller_voucher { get; set; }
        //public int waiting_payment { get; set; }
        //public int sold_count { get; set; }
        public Specs specs { get; set; }
        public bool force_insurance { get; set; }
        public object[] free_shipping_coverage { get; set; }
        public object video_url { get; set; }
        public int sla_display { get; set; }
        public string sla_type { get; set; }
        public bool assurance { get; set; }
        public string id { get; set; }//seller sku
        public string url { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public int weight { get; set; }
        public long[] image_ids { get; set; }
        public long[] new_image_ids { get; set; }
        public string[] images { get; set; }
        public string[] small_images { get; set; }
        public string desc { get; set; }
        public string condition { get; set; }
        public int stock { get; set; }
        public bool favorited { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object[] product_sin { get; set; }
        //public Rating rating { get; set; }
        public string current_variant_name { get; set; }
        public long current_product_sku_id { get; set; }
        public object[] product_sku { get; set; }
        public object[] options { get; set; }
        public object alternative_image { get; set; }
        public int min_quantity { get; set; }
        public int max_quantity { get; set; }
        public bool has_bundling { get; set; }
        public bool on_bundling { get; set; }
        public string[] courier { get; set; }
        //public Negotiation negotiation { get; set; }
        //public int interest_count { get; set; }
        public DateTime last_relist_at { get; set; }
        //public int view_count { get; set; }
    }
    public class Specs
    {
        public string brand { get; set; }
        public string tipe { get; set; }
    }

    public class BindingBukaLapakProduct
    {
        public ProductBukaLpk product { get; set; }
        public string images { get; set; }
    }
    public class ProductBukaLpk
    {
        public string category_id { get; set; }
        public string name { get; set; }
        public string @new { get; set; }
        public string price { get; set; }
        public string negotiable { get; set; }
        public string weight { get; set; }
        public string stock { get; set; }
        public string description_bb { get; set; }
        public Product_Detail_Attributes product_detail_attributes { get; set; }
    }
    public class Product_Detail_Attributes
    {
        //public string type { get; set; }
        public string merek { get; set; }
        public string bahan { get; set; }
        public string ukuran { get; set; }
        public string tipe { get; set; }
    }
    #endregion
    #region get order
    public class BukaLapakOrder : BukaLapakResponse
    {
        public Transaction[] transactions { get; set; }
    }
    public class Transaction
    {
        public long id { get; set; }
        public long invoice_id { get; set; }
        public string state { get; set; }
        public DateTime updated_at { get; set; }
        public bool unread { get; set; }
        public bool quick_trans { get; set; }
        public string transaction_id { get; set; }
        public long amount { get; set; }
        public int quantity { get; set; }
        public string courier { get; set; }
        public object same_day_service_info { get; set; }
        public object ojek_service_info { get; set; }
        public Pickup_Service_Info pickup_service_info { get; set; }
        public string buyer_notes { get; set; }
        public string dropshipper_name { get; set; }
        public string dropshipper_notes { get; set; }
        public long shipping_fee { get; set; }
        public long shipping_id { get; set; }
        public string shipping_code { get; set; }
        public Shipping_History[] shipping_history { get; set; }
        public string shipping_service { get; set; }
        public long insurance_cost { get; set; }
        public long subtotal_amount { get; set; }
        public long total_amount { get; set; }
        public long coded_amount { get; set; }
        public long? uniq_code { get; set; }
        public long refund_amount { get; set; }
        public long reduction_amount { get; set; }
        public bool? use_seller_voucher { get; set; }
        public bool use_voucher { get; set; }
        public long voucher_amount { get; set; }
        public long reward_amount { get; set; }
        public long promo_payment_amount { get; set; }
        public long priority_buyer_reduction_amount { get; set; }
        public long priority_buyer_package_price { get; set; }
        public long agent_commission_amount { get; set; }
        public string payment_method { get; set; }
        public string payment_method_name { get; set; }
        public long payment_amount { get; set; }
        public long remit_amount { get; set; }
        public long service_fee { get; set; }
        //public Feedback feedback { get; set; }
        public ProductBukaLapak[] products { get; set; }
        public object[] bundles_items { get; set; }
        public object pickup_time { get; set; }
        //public Amount_Details[] amount_details { get; set; }
        //public Installment installment { get; set; }
        public Consignee consignee { get; set; }
        public Buyer buyer { get; set; }
        public Seller seller { get; set; }
        public Invoice invoice { get; set; }
        //public Voucher voucher { get; set; }
        public object reward { get; set; }
        public string[] actions { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? deliver_before { get; set; }
        public DateTime? pay_before { get; set; }
        public string reject_reason { get; set; }
        public object[] return_reason { get; set; }
        public State_Changes state_changes { get; set; }
        public bool has_deal_product { get; set; }
        public object return_info { get; set; }
        public int total_weight { get; set; }
        public bool need_action { get; set; }
        public bool _virtual { get; set; }
        public bool remote { get; set; }
        //public Phone_Credit phone_credit { get; set; }
        public string buyer_logistic_choice { get; set; }
        //public Logistic_Booking logistic_booking { get; set; }
        public string type { get; set; }
        public object replacement { get; set; }
        public bool on_hold { get; set; }
        public string created_on { get; set; }
        public bool assurance_available { get; set; }
        public object claim { get; set; }
        public object retur { get; set; }
    }
    public class Shipping_History
    {
        public DateTime date { get; set; }
        public string status { get; set; }
    }
    public class Pickup_Service_Info
    {
        public object shipping_status { get; set; }
        public bool show_order_pickup { get; set; }
        public string policy { get; set; }
        public Shipping_Info shipping_info { get; set; }
    }

    public class Shipping_Info
    {
        public string title { get; set; }
        public string last_order_schedule { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public object latitude { get; set; }
        public object longitude { get; set; }
    }
    public class Consignee
    {
        public string name { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string area { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string post_code { get; set; }
    }
    public class Buyer
    {
        public long id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public string email { get; set; }
    }

    public class Seller
    {
        public long id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
    }

    public class Invoice
    {
        public long id { get; set; }
        public string invoice_id { get; set; }
        public string state { get; set; }
    }
    public class State_Changes
    {
        public DateTime addressed_at { get; set; }
        public DateTime payment_chosen_at { get; set; }
        public DateTime paid_at { get; set; }
        public DateTime accepted_at { get; set; }
        public DateTime delivered_at { get; set; }
        public DateTime received_at { get; set; }
        public DateTime remitted_at { get; set; }
        public DateTime expired_at { get; set; }
        public DateTime rejected_at { get; set; }
        public DateTime refunded_at { get; set; }
    }
    public class ProductBukaLapak
    {
        //public Deal_Info deal_info { get; set; }
        public string deal_request_state { get; set; }
        public double price { get; set; }
        public long category_id { get; set; }
        public string category { get; set; }
        public string[] category_structure { get; set; }
        public string seller_username { get; set; }
        public string seller_name { get; set; }
        public long seller_id { get; set; }
        public string seller_avatar { get; set; }
        public string seller_level { get; set; }
        public string seller_level_badge_url { get; set; }
        public string seller_delivery_time { get; set; }
        //public int seller_positive_feedback { get; set; }
        //public int seller_negative_feedback { get; set; }
        public string seller_term_condition { get; set; }
        public string seller_alert { get; set; }
        public bool for_sale { get; set; }
        public string[] state_description { get; set; }
        public bool premium_account { get; set; }
        public bool brand { get; set; }
        public bool top_merchant { get; set; }
        //public Last_Order_Schedule last_order_schedule { get; set; }
        //public Seller_Voucher seller_voucher { get; set; }
        public int waiting_payment { get; set; }
        public int sold_count { get; set; }
        public Specs specs { get; set; }
        public bool force_insurance { get; set; }
        public object[] free_shipping_coverage { get; set; }
        public string video_url { get; set; }
        public int sla_display { get; set; }
        public string sla_type { get; set; }
        public bool assurance { get; set; }
        //public Label[] labels { get; set; }
        //public Tag_Pages[] tag_pages { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public int weight { get; set; }
        public long[] image_ids { get; set; }
        public long[] new_image_ids { get; set; }
        public string[] images { get; set; }
        public string[] small_images { get; set; }
        public string desc { get; set; }
        public string condition { get; set; }
        public int stock { get; set; }
        public bool favorited { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object[] product_sin { get; set; }
        //public Rating rating { get; set; }
        public string current_variant_name { get; set; }
        public long current_product_sku_id { get; set; }
        public object[] product_sku { get; set; }
        public object[] options { get; set; }
        public object alternative_image { get; set; }
        public int min_quantity { get; set; }
        public int max_quantity { get; set; }
        public bool has_bundling { get; set; }
        public bool on_bundling { get; set; }
        //public Wholesale[] wholesale { get; set; }
        public string[] courier { get; set; }
        //public Negotiation negotiation { get; set; }
        public int interest_count { get; set; }
        public DateTime last_relist_at { get; set; }
        //public int view_count { get; set; }
        public int order_quantity { get; set; }
        public long accepted_price { get; set; }
        public long cart_item_id { get; set; }//changed from int to long
        public string status { get; set; }
        public long nominal { get; set; }
        public long active_mapping_id { get; set; }
        public string _operator { get; set; }
    }
    public class Amount_Details
    {
        public string name { get; set; }
        public long? amount { get; set; }
    }
    #endregion
    #region change status
    public class BindingShipBL
    {
        public ShippingBukaLapak payment_shipping { get; set; }
    }
    public class ShippingBukaLapak
    {
        public string transaction_id { get; set; }
        public string shipping_code { get; set; }
        public string new_courier { get; set; }
    }
    #endregion
    #region change product stat
    public class ResProduct : BukaLapakResponse
    {
        public string product_id { get; set; }
    }
    public class BindingProductBL
    {
        public string status { get; set; }
        public ProductSingle product { get; set; }
        public object message { get; set; }
    }
    public class ProductSingle
    {
        //public Deal_Info deal_info { get; set; }
        public string deal_request_state { get; set; }
        public long price { get; set; }
        public long category_id { get; set; }
        public string category { get; set; }
        public string[] category_structure { get; set; }
        public string seller_username { get; set; }
        public string seller_name { get; set; }
        public long seller_id { get; set; }
        public string seller_avatar { get; set; }
        public string seller_level { get; set; }
        public string seller_level_badge_url { get; set; }
        public string seller_delivery_time { get; set; }
        //public int seller_positive_feedback { get; set; }
        //public int seller_negative_feedback { get; set; }
        public string seller_term_condition { get; set; }
        public object seller_alert { get; set; }
        public bool for_sale { get; set; }
        public object[] state_description { get; set; }
        public bool premium_account { get; set; }
        public bool brand { get; set; }
        public bool top_merchant { get; set; }
        //public Last_Order_Schedule last_order_schedule { get; set; }
        //public Seller_Voucher seller_voucher { get; set; }
        public int waiting_payment { get; set; }
        //public int sold_count { get; set; }
        public Specs specs { get; set; }
        public bool force_insurance { get; set; }
        public object[] free_shipping_coverage { get; set; }
        public string video_url { get; set; }
        public bool assurance { get; set; }
        //public Label[] labels { get; set; }
        //public Tag_Pages[] tag_pages { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public int weight { get; set; }
        public long[] image_ids { get; set; }
        public long[] new_image_ids { get; set; }
        public string[] images { get; set; }
        public string[] small_images { get; set; }
        public string desc { get; set; }
        public string condition { get; set; }
        public int stock { get; set; }
        public bool favorited { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object[] product_sin { get; set; }
        //public Rating rating { get; set; }
        public string current_variant_name { get; set; }
        public long current_product_sku_id { get; set; }
        public object[] product_sku { get; set; }
        public object[] options { get; set; }
        public object alternative_image { get; set; }
        public int min_quantity { get; set; }
        public int max_quantity { get; set; }
        public bool has_bundling { get; set; }
        public bool on_bundling { get; set; }
        //public Wholesale[] wholesale { get; set; }
        public string[] courier { get; set; }
        public bool on_daily_deal { get; set; }
        //public Daily_Deal daily_deal { get; set; }
        //public Negotiation negotiation { get; set; }
        public int sla_display { get; set; }
        public string sla_type { get; set; }
        public object sla_display_raw { get; set; }
        public object sla_type_raw { get; set; }
        public long interest_count { get; set; }
        public DateTime last_relist_at { get; set; }
        public int view_count { get; set; }
    }
    #endregion
    #region list prod
    public class ProdBL : BukaLapakResponse
    {
        //public string status { get; set; }
        public List<ListProduct> products { get; set; }
        //public bool can_push { get; set; }
        //public int remaining_push { get; set; }
        //public object message { get; set; }
        //public Label1[] labels { get; set; }
        //public int push_price { get; set; }
        //public int deposit { get; set; }
        //public string push_status { get; set; }
        //public DateTime active_until { get; set; }
        //public DateTime grace_period_until { get; set; }
        //public Loan_Info loan_info { get; set; }
    }
    public class ListProduct
    {
        public Deal_Info deal_info { get; set; }
        //public string deal_request_state { get; set; }
        public long price { get; set; }
        public long category_id { get; set; }
        public string category { get; set; }
        public string[] category_structure { get; set; }
        //public string seller_username { get; set; }
        //public string seller_name { get; set; }
        //public int seller_id { get; set; }
        //public string seller_avatar { get; set; }
        //public string seller_level { get; set; }
        //public string seller_level_badge_url { get; set; }
        //public string seller_delivery_time { get; set; }
        //public int seller_positive_feedback { get; set; }
        //public int seller_negative_feedback { get; set; }
        //public string seller_term_condition { get; set; }
        //public object seller_alert { get; set; }
        public bool for_sale { get; set; }
        public object[] state_description { get; set; }
        //public bool premium_account { get; set; }
        public bool brand { get; set; }
        //public bool top_merchant { get; set; }
        //public Last_Order_Schedule last_order_schedule { get; set; }
        //public Seller_Voucher seller_voucher { get; set; }
        //public int waiting_payment { get; set; }
        //public long sold_count { get; set; }

        //public Specs_prod specs { get; set; }
        public dynamic specs { get; set; }

        //public bool force_insurance { get; set; }
        //public object[] free_shipping_coverage { get; set; }
        //public string video_url { get; set; }
        //public bool assurance { get; set; }
        //public Label[] labels { get; set; }
        //public Tag_Pages[] tag_pages { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
        //public string city { get; set; }
        //public string province { get; set; }
        public int weight { get; set; }
        public long[] image_ids { get; set; }
        public long[] new_image_ids { get; set; }
        public string[] images { get; set; }
        public string[] small_images { get; set; }
        public string desc { get; set; }
        public string condition { get; set; }
        public long stock { get; set; }
        //public bool favorited { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object[] product_sin { get; set; }
        //public Rating rating { get; set; }
        public string current_variant_name { get; set; }
        public long current_product_sku_id { get; set; }
        public List<ProductSku> product_sku { get; set; }
        public object[] options { get; set; }
        public object alternative_image { get; set; }
        //public int min_quantity { get; set; }
        //public int max_quantity { get; set; }
        //public bool has_bundling { get; set; }
        //public bool on_bundling { get; set; }
        //public Wholesale[] wholesale { get; set; }
        //public string[] courier { get; set; }
        //public bool on_daily_deal { get; set; }
        //public Daily_Deal daily_deal { get; set; }
        //public Negotiation negotiation { get; set; }
        //public int sla_display { get; set; }
        //public string sla_type { get; set; }
        //public object sla_display_raw { get; set; }
        //public object sla_type_raw { get; set; }
        //public int interest_count { get; set; }
        //public DateTime last_relist_at { get; set; }
        //public int view_count { get; set; }
    }

    public class ProductSku
    {
        //public Deal_Info deal_info { get; set; }
        //public string deal_request_state { get; set; }
        public long price { get; set; }
        public long id { get; set; }
        public string sku_name { get; set; }
        public int stock { get; set; }
        public string variant_name { get; set; }
        public Variant[] variant { get; set; }
        public int is_default { get; set; }
        public string state { get; set; }
        public long[] image_ids { get; set; }
        public long[] new_image_ids { get; set; }
        public string[] images { get; set; }
        public string[] small_images { get; set; }
        public Installment[] installment { get; set; }
        public string min_installment_price { get; set; }
    }

    public class Deal_Info
    {
        public long original_price { get; set; }
        public long discount_price { get; set; }
        public double discount_percentage { get; set; }
        public DateTime discount_date { get; set; }
        public DateTime discount_expired_date { get; set; }
        public string state { get; set; }
    }

    public class Variant
    {
        public long label_id { get; set; }
        public string label { get; set; }
        public string value { get; set; }
        public long value_id { get; set; }
    }

    public class Installment
    {
        public string bank_issuer { get; set; }
        public long[] terms { get; set; }
        public string bank_name { get; set; }
        public string bank_acquirer { get; set; }
        public string url_logo { get; set; }
    }

    public class Specs_prod
    {
        public string merek { get; set; }
        public string brand { get; set; }
        public string tipe { get; set; }
    }
    #endregion


    public class CategoryBL
    {
        public string status { get; set; }
        public List<Category> categories { get; set; }
    }

    public class Category
    {
        public long id { get; set; }
        public string name { get; set; }
        public List<Category> children { get; set; }
    }


}