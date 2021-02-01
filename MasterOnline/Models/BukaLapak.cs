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

        //add by nurul 20/1/2021, bundling 
        public bool AdaPesanan { get; set; }
        public string ConnId { get; set; }
        public bool AdaKomponen { get; set; }
        //end add by nurul 20/1/2021, bundling
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
        //public string user_id { get; set; }
        //public string user_name { get; set; }
        //public bool confirmed { get; set; }
        //public string token { get; set; }
        //public string email { get; set; }
        //public string omnikey { get; set; }
        //public string message { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
        public long expires_in { get; set; }
        public string refresh_token { get; set; }
        public long created_at { get; set; }

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
        public string merek { get; set; }
        
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
    #region get item list v2

    public class GetItemListResponse
    {
        public GetItemListDatum[] data { get; set; }
        public GetItemListMeta meta { get; set; }
    }

    public class GetItemListMeta
    {
        public int http_status { get; set; }
        public int offset { get; set; }
        public int total { get; set; }
        public int limit { get; set; }
    }

    public class GetItemListDatum
    {
        public bool active { get; set; }
        public bool assurance { get; set; }
        //public object[] available_countries { get; set; }
        public GetItemListCategory category { get; set; }
        public string condition { get; set; }
        public string[] couriers { get; set; }
        public DateTime created_at { get; set; }
        //public Deal deal { get; set; }
        //public object default_catalog { get; set; }
        public long default_sku_id { get; set; }
        public string description { get; set; }
        public string description_bb { get; set; }
        public bool digital_product { get; set; }
        public DimensionProduct dimensions { get; set; }
        //public int discount_percentage { get; set; }
        //public object discount_subsidy { get; set; }
        public bool for_sale { get; set; }
        //public object[] free_shipping_coverage { get; set; }
        public string id { get; set; }
        public GetItemListImages images { get; set; }
        public bool imported { get; set; }
        //public Installment[] installments { get; set; }
        //public object[] international_couriers { get; set; }
        //public string international_shipping_status { get; set; }
        //public Label[] labels { get; set; }
        public int max_quantity { get; set; }
        public bool merchant_return_insurance { get; set; }
        public int min_quantity { get; set; }
        public string name { get; set; }
        public long original_price { get; set; }
        public long price { get; set; }
        //public object[] product_sin { get; set; }
        //public Promoted_Detail promoted_detail { get; set; }
        //public Rating rating { get; set; }
        public DateTime relisted_at { get; set; }
        //public bool rush_delivery { get; set; }
        //public Shipping shipping { get; set; }
        public long sku_id { get; set; }
        public string sku_name { get; set; }
        //public Sla sla { get; set; }
        //public int special_campaign_id { get; set; }
        public dynamic specs { get; set; }
        public string state { get; set; }
        //public object[] state_description { get; set; }
        public GetItemListStats stats { get; set; }
        public int stock { get; set; }
        //public Store store { get; set; }
        //public Tag_Pages[] tag_pages { get; set; }
        public DateTime updated_at { get; set; }
        public string url { get; set; }
        public GetItemListVariant[] variants { get; set; }
        //public object video_url { get; set; }
        //public Warranty warranty { get; set; }
        public int weight { get; set; }
        //public object[] wholesales { get; set; }
        //public bool without_shipping { get; set; }
    }
    public class DimensionProduct
    {
        public int width { get; set; }
        public int height { get; set; }
        public int length { get; set; }

    }
    public class GetItemListCategory
    {
        public long id { get; set; }
        public string name { get; set; }
        public string[] structure { get; set; }
        public string url { get; set; }
    }

    //public class Deal
    //{
    //}

    public class GetItemListImages
    {
        public long[] ids { get; set; }
        public string[] large_urls { get; set; }
        public string[] original_urls { get; set; }
        public string[] small_urls { get; set; }
    }

    //public class Promoted_Detail
    //{
    //    public bool active { get; set; }
    //    public int bid_value { get; set; }
    //    public string end_date { get; set; }
    //    public string start_date { get; set; }
    //}

    //public class Rating
    //{
    //}

    //public class Shipping
    //{
    //    public bool force_insurance { get; set; }
    //    public object[] free_shipping_coverage { get; set; }
    //}

    //public class Sla
    //{
    //    public string type { get; set; }
    //    public int? value { get; set; }
    //}

    public class GetItemListSpecs
    {
        public string brand { get; set; }
        public string kapasitas_hardisk { get; set; }
        public string kapasitas_memory { get; set; }
        public string screen_size { get; set; }
    }

    public class GetItemListStats
    {
        public int interest_count { get; set; }
        public int sold_count { get; set; }
        public int view_count { get; set; }
        public int waiting_payment_count { get; set; }
    }

    //public class Store
    //{
    //    public Acceptance acceptance { get; set; }
    //    public Address address { get; set; }
    //    public string alert { get; set; }
    //    public string avatar_url { get; set; }
    //    public bool brand_seller { get; set; }
    //    public string[] carriers { get; set; }
    //    public Closing closing { get; set; }
    //    public bool connected_facebook { get; set; }
    //    public bool connected_twitter { get; set; }
    //    public string delivery_time { get; set; }
    //    public string description { get; set; }
    //    public DateTime first_upload_product_at { get; set; }
    //    public bool flagship { get; set; }
    //    public object[] groups { get; set; }
    //    public Header_Image header_image { get; set; }
    //    public int id { get; set; }
    //    public Inactivity inactivity { get; set; }
    //    public International_Shipping international_shipping { get; set; }
    //    public string lapak_phone { get; set; }
    //    public Last_Order_Schedule last_order_schedule { get; set; }
    //    public Level level { get; set; }
    //    public int max_deal_duration { get; set; }
    //    public string name { get; set; }
    //    public string premium_level { get; set; }
    //    public bool premium_top_seller { get; set; }
    //    public Product_Upload_Remaining product_upload_remaining { get; set; }
    //    public Rejection rejection { get; set; }
    //    public Reputation reputation { get; set; }
    //    public Reviews reviews { get; set; }
    //    public Sla1 sla { get; set; }
    //    public int subscriber_amount { get; set; }
    //    public string term_and_condition { get; set; }
    //    public string url { get; set; }
    //    public User_Term_And_Condition user_term_and_condition { get; set; }
    //}

    //public class Acceptance
    //{
    //    public int acceptance_rate { get; set; }
    //    public int accepted_transaction { get; set; }
    //}

    //public class Address
    //{
    //    public string city { get; set; }
    //    public string province { get; set; }
    //}

    //public class Closing
    //{
    //    public bool closed { get; set; }
    //    public string closed_reason { get; set; }
    //    public DateTime reopen_date { get; set; }
    //    public DateTime start_date { get; set; }
    //}

    //public class Header_Image
    //{
    //    public int id { get; set; }
    //    public string url { get; set; }
    //}

    //public class Inactivity
    //{
    //    public bool inactive { get; set; }
    //    public DateTime last_appear_at { get; set; }
    //}

    //public class International_Shipping
    //{
    //    public string status { get; set; }
    //    public string tnc_status { get; set; }
    //}

    //public class Last_Order_Schedule
    //{
    //    public string friday { get; set; }
    //    public string monday { get; set; }
    //    public string saturday { get; set; }
    //    public string sunday { get; set; }
    //    public string thursday { get; set; }
    //    public string tuesday { get; set; }
    //    public string wednesday { get; set; }
    //}

    public class Level
    {
        public string image_url { get; set; }
        public string name { get; set; }
    }

    //public class Product_Upload_Remaining
    //{
    //    public int daily { get; set; }
    //    public int monthly { get; set; }
    //    public int weekly { get; set; }
    //}

    //public class Rejection
    //{
    //    public int recent_transactions { get; set; }
    //    public int rejected { get; set; }
    //}

    //public class Reputation
    //{
    //    public float level_progress { get; set; }
    //    public string next_level { get; set; }
    //    public int point_needed_next_level { get; set; }
    //}

    //public class Reviews
    //{
    //    public int negative { get; set; }
    //    public int positive { get; set; }
    //}

    //public class Sla1
    //{
    //    public string type { get; set; }
    //    public int value { get; set; }
    //}

    //public class User_Term_And_Condition
    //{
    //    public string description { get; set; }
    //    public DateTime updated_at { get; set; }
    //}

    //public class Warranty
    //{
    //    public bool cheapest { get; set; }
    //}

    //public class Installment
    //{
    //    public string bank_issuer { get; set; }
    //    public string bank_issuer_name { get; set; }
    //    public string logo_url { get; set; }
    //    public int[] terms { get; set; }
    //}

    public class GetItemListLabel
    {
        public string description { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public class Tag_Pages
    {
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class GetItemListVariant
    {
        //public Address1 address { get; set; }
        //public Deal1 deal { get; set; }
        public bool default_sku { get; set; }
        public VariantDetail[] details { get; set; }
        //public int discount { get; set; }
        //public int discount_percentage { get; set; }
        //public object discount_subsidy { get; set; }
        public long id { get; set; }
        public Images1 images { get; set; }
        public int price { get; set; }
        public string product_id { get; set; }
        public string sku_name { get; set; }
        public string state { get; set; }
        public int stock { get; set; }
        public string variant_name { get; set; }
        //public object[] wholesales { get; set; }
    }

    public class Address1
    {
        public int id { get; set; }
        public string title { get; set; }
    }

    public class Deal1
    {
    }

    public class Images1
    {
        public long[] ids { get; set; }
        public string[] large_urls { get; set; }
        public string[] small_urls { get; set; }
    }

    public class VariantDetail
    {
        public VariantLabel label { get; set; }
        public VariantValue value { get; set; }
        public long variant_id { get; set; }
    }

    public class VariantLabel
    {
        public long id { get; set; }
        public string name { get; set; }
        public string state { get; set; }
    }

    public class VariantValue
    {
        public long id { get; set; }
        public string name { get; set; }
        public string state { get; set; }
    }

    #endregion
    public class BukaLapakKey
    {
        public string token { get; set; }
        public string code { get; set; }
        public string refresh_token { get; set; }
        public DateTime tgl_expired { get; set; }
        public string dbPathEra { get; set; }
        public string cust { get; set; }

    }
    public class BLErrorResponse
    {
        public ErrorDetail[] errors { get; set; }
    }
    public class ErrorDetail
    {
        public string message { get; set; }
        public long code { get; set; }

    }
    public class CategoryBL
    {
        public string status { get; set; }
        public List<CategoryBukaLapakV2> data { get; set; }
    }

    public class CategoryBukaLapakV2
    {
        public long id { get; set; }
        public string name { get; set; }
        public List<CategoryBukaLapakV2> children { get; set; }
    }


}