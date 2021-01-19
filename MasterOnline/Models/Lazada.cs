using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class Lazada
    {
    }
    public class LazadaGetResiObj
    {
        public string Errors { get; set; }
        public string NoResi { get; set; }
    }
    public class PromoLazadaObj
    {
        public string kdBrg { get; set; }
        public string fromDt { get; set; }
        public string toDt { get; set; }
        public double promoPrice { get; set; }
        public string token { get; set; }
    }
    public class LazadaCommonRes
    {
        public string code { get; set; }
        public string request_id { get; set; }
        public string type { get; set; }
        public string message { get; set; }
    }
    public class LazadaResponseObj : LazadaCommonRes
    {
        public detailUpdateBrg[] detail { get; set; }
        public dataUploadBrg data { get; set; }
    }

    public class LazadaItemDetailResponse
    {
        public string code { get; set; }
        public DataItemDetail data { get; set; }
        public string request_id { get; set; }
    }

    public class DataItemDetail
    {
        public SkuItemDetail[] skus { get; set; }
        public string item_id { get; set; }
        public string primary_category { get; set; }
        public AttributesItemDetail attributes { get; set; }
    }

    public class AttributesItemDetail
    {
        public string short_description { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string warranty_type { get; set; }
        public string brand { get; set; }
    }

    public class SkuItemDetail
    {
        public string Status { get; set; }
        public int quantity { get; set; }
        public string product_weight { get; set; }
        public string[] Images { get; set; }
        public string SellerSku { get; set; }
        public string ShopSku { get; set; }
        public string Url { get; set; }
        public MultiwarehouseinventoryItemDetail[] multiWarehouseInventories { get; set; }
        public string package_width { get; set; }
        public string special_to_time { get; set; }
        public string special_from_time { get; set; }
        public string package_height { get; set; }
        public double special_price { get; set; }
        public double price { get; set; }
        public string package_length { get; set; }
        public string package_weight { get; set; }
        public int Available { get; set; }
        public long SkuId { get; set; }
        public string special_to_date { get; set; }
    }

    public class MultiwarehouseinventoryItemDetail
    {
        public int quantity { get; set; }
        public string warehouseCode { get; set; }
    }

    public class LazadaCreateBarangResponse : LazadaCommonRes
    {
        public detailUpdateBrg[] detail { get; set; }
        public LazadaCreateBarangResponseData data { get; set; }
    }

    public class LazadaCreateBarangResponseData
    {
        public long item_id { get; set; }
        public LazadaCreateBarangResponseDataSku_List[] sku_list { get; set; }
    }

    public class LazadaCreateBarangResponseDataSku_List
    {
        public string shop_sku { get; set; }
        public string seller_sku { get; set; }
        public long sku_id { get; set; }
    }

    public class detailUpdateBrg
    {
        public string seller_sku { get; set; }
        public string message { get; set; }
        public string field { get; set; }
    }
    public class dataUploadBrg
    {
        public imgLazada image { get; set; }
    }
    public class imgLazada
    {
        public string hash_code { get; set; }
        public string url { get; set; }
    }

    public class LazadaAuth
    {
        public string access_token { get; set; }
        public string country { get; set; }
        public string refresh_token { get; set; }
        public string account_platform { get; set; }
        public int refresh_expires_in { get; set; }
        public Country_User_Info[] country_user_info { get; set; }
        public int expires_in { get; set; }
        public string account { get; set; }
        public string code { get; set; }
        public string request_id { get; set; }
    }

    public class Country_User_Info
    {
        public string country { get; set; }
        public string user_id { get; set; }
        public string seller_id { get; set; }
        public string short_code { get; set; }
    }

    public class LazadaCancelOrder : LazadaCommonRes
    {
        public string success { get; set; }
    }

    public class LazadaGetLabel : LazadaCommonRes
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public Document document { get; set; }
    }

    public class Document
    {
        public string document_type { get; set; }
        public string mime_type { get; set; }
        public string file { get; set; }
    }

    public class ImageLzd : LazadaCommonRes
    {
        public DataImage data { get; set; }
    }

    public class DataImage
    {
        public ImageObj image { get; set; }
    }

    public class ImageObj
    {
        public string hash_code { get; set; }
        public string url { get; set; }
    }

    public class ShipmentLazada : LazadaCommonRes
    {
        public ShipmentData data { get; set; }
    }

    public class ShipmentData
    {
        public Shipment_Providers[] shipment_providers { get; set; }
    }

    public class Shipment_Providers
    {
        public string name { get; set; }
        public string cod { get; set; }
        public string is_default { get; set; }
        public string api_integration { get; set; }
    }

    public class LazadaToDeliver : LazadaCommonRes
    {
        public OrdersPacked data { get; set; }
    }
    //public class OrdersPacked
    //{
    //    public List<OrderPacked> OrderItems { get; set; }
    //}
    //public class OrderPacked
    //{
    //    public long OrderItemId { get; set; }
    //    public long PurchaseOrderId { get; set; }
    //    public string PurchaseOrderNumber { get; set; }
    //    public string PackageId { get; set; }
    //    public string ShipmentProvider { get; set; }
    //    public string TrackingNumber { get; set; }
    //}    

    public class OrdersPacked
    {
        public List<OrderPacked> order_items { get; set; }
    }

    public class OrderPacked
    {
        public long order_item_id { get; set; }
        public string tracking_number { get; set; }
        public string shipment_provider { get; set; }
        public string package_id { get; set; }
    }

    public class NewLzdOrders : LazadaCommonRes
    {
        public DataOrders data { get; set; }
    }
    public class DataOrders
    {
        public List<Order> orders { get; set; }
        public string count { get; set; }
    }

    public class Order
    {
        public string voucher { get; set; }
        public string order_number { get; set; }
        public DateTime created_at { get; set; }
        public string voucher_code { get; set; }
        public string gift_option { get; set; }
        public string customer_last_name { get; set; }
        public DateTime updated_at { get; set; }
        public string promised_shipping_times { get; set; }
        public string price { get; set; }
        public string national_registration_number { get; set; }
        public string payment_method { get; set; }
        public string address_updated_at { get; set; }
        public string customer_first_name { get; set; }
        public string shipping_fee { get; set; }
        public string branch_number { get; set; }
        public string tax_code { get; set; }
        public string items_count { get; set; }
        public string delivery_info { get; set; }
        public string[] statuses { get; set; }
        public Address_Billing address_billing { get; set; }
        public string extra_attributes { get; set; }
        public string order_id { get; set; }
        public string gift_message { get; set; }
        public string remarks { get; set; }
        public Address_Shipping address_shipping { get; set; }
        public string reason { get; set; }//add 4 Des 2019, cancel reason
    }
    public class Address_Billing
    {
        public string country { get; set; }
        public string address3 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string address1 { get; set; }
        public string phone2 { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string customer_email { get; set; }
        public string post_code { get; set; }
        public string address5 { get; set; }
        public string address4 { get; set; }
        public string first_name { get; set; }
    }

    public class Address_Shipping
    {
        public string country { get; set; }
        public string address3 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string address1 { get; set; }
        public string phone2 { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string customer_email { get; set; }
        public string post_code { get; set; }
        public string address5 { get; set; }
        public string address4 { get; set; }
        public string first_name { get; set; }
    }

    public class LzdNewOrderItems : LazadaCommonRes
    {
        public List<Orderitem> data { get; set; }
    }
    public class Orderitem
    {
        public long OrderItemId { get; set; }
        public string ShopId { get; set; }
        public long OrderId { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public string ShopSku { get; set; }
        public string ShippingType { get; set; }
        public float ItemPrice { get; set; }
        public float PaidPrice { get; set; }
        public string Currency { get; set; }
        public float TaxAmount { get; set; }
        public float ShippingAmount { get; set; }
        public int ShippingServiceCost { get; set; }
        public int VoucherAmount { get; set; }
        public string VoucherCode { get; set; }
        public string Status { get; set; }
        public string ShipmentProvider { get; set; }
        public int IsDigital { get; set; }
        public string DigitalDeliveryInfo { get; set; }
        public string TrackingCode { get; set; }
        public string TrackingCodePre { get; set; }
        public string Reason { get; set; }
        public string ReasonDetail { get; set; }
        public string PurchaseOrderId { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string PackageId { get; set; }
        public string PromisedShippingTime { get; set; }
        public string ExtraAttributes { get; set; }
        public string ShippingProviderType { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string ReturnStatus { get; set; }
        public string productMainImage { get; set; }
        public string Variation { get; set; }
        public string ProductDetailUrl { get; set; }
        public string invoiceNumber { get; set; }
    }
    #region multi oder item

    public class LazadaOrderItems : LazadaCommonRes
    {
        //public string code { get; set; }
        public List<Datum> data { get; set; }
        //public string request_id { get; set; }
    }

    public class Datum
    {
        public string order_number { get; set; }
        public string order_id { get; set; }
        public Order_Items[] order_items { get; set; }
    }

    public class Order_Items
    {
        public string paid_price { get; set; }
        public string product_main_image { get; set; }
        public string tax_amount { get; set; }
        public string reason { get; set; }
        public string product_detail_url { get; set; }
        public string promised_shipping_time { get; set; }
        public string purchase_order_id { get; set; }
        public string shipping_type { get; set; }
        public DateTime created_at { get; set; }
        public string voucher_code { get; set; }
        public string package_id { get; set; }
        public string variation { get; set; }
        public string wallet_credits { get; set; }
        public DateTime updated_at { get; set; }
        public string purchase_order_number { get; set; }
        public string currency { get; set; }
        public string shipping_provider_type { get; set; }
        public string sku { get; set; }
        public string invoice_number { get; set; }
        public string shop_sku { get; set; }
        public string is_digital { get; set; }
        public string item_price { get; set; }
        public string shipping_service_cost { get; set; }
        public string tracking_code_pre { get; set; }
        public string tracking_code { get; set; }
        public string shipping_amount { get; set; }
        public string order_item_id { get; set; }
        public string reason_detail { get; set; }
        public string shop_id { get; set; }
        public string return_status { get; set; }
        public string name { get; set; }
        public string shipment_provider { get; set; }
        public string voucher_amount { get; set; }
        public string digital_delivery_info { get; set; }
        public string extra_attributes { get; set; }
        public string order_id { get; set; }
        public string status { get; set; }
        public string sla_time_stamp { get; set; }
    }


    public class LazadaGetOrderItem
    {
        public LazadaGetOrderItemData[] data { get; set; }
        public string code { get; set; }
        public string request_id { get; set; }
    }

    public class LazadaGetOrderItemData
    {
        public float paid_price { get; set; }
        public string product_main_image { get; set; }
        public float tax_amount { get; set; }
        public double voucher_platform { get; set; }
        public string reason { get; set; }
        public DateTime? sla_time_stamp { get; set; }
        public string product_detail_url { get; set; }
        public string promised_shipping_time { get; set; }
        public string warehouse_code { get; set; }
        public string purchase_order_id { get; set; }
        public double voucher_seller { get; set; }
        public string shipping_type { get; set; }
        public string created_at { get; set; }
        public string voucher_code { get; set; }
        public string package_id { get; set; }
        public string variation { get; set; }
        public string updated_at { get; set; }
        public string purchase_order_number { get; set; }
        public string currency { get; set; }
        public string shipping_provider_type { get; set; }
        public string sku { get; set; }
        public string invoice_number { get; set; }
        public string order_type { get; set; }
        public string cancel_return_initiator { get; set; }
        public string shop_sku { get; set; }
        public int is_digital { get; set; }
        public float item_price { get; set; }
        public double shipping_service_cost { get; set; }
        public string stage_pay_status { get; set; }
        public string tracking_code_pre { get; set; }
        public string tracking_code { get; set; }
        public float shipping_amount { get; set; }
        public long order_item_id { get; set; }
        public string reason_detail { get; set; }
        public string shop_id { get; set; }
        public string order_flag { get; set; }
        public string return_status { get; set; }
        public string name { get; set; }
        public string shipment_provider { get; set; }
        public float voucher_amount { get; set; }
        public string digital_delivery_info { get; set; }
        public string extra_attributes { get; set; }
        public long order_id { get; set; }
        public string status { get; set; }
    }

    #endregion

    public class AttributeBody : LazadaCommonRes
    {
        public List<DataAttr> data { get; set; }
        //public string code { get; set; }
        //public string request_id { get; set; }
    }

    public class DataAttr
    {
        public int is_sale_prop { get; set; }
        public string name { get; set; }
        public string input_type { get; set; }
        public List<Option> options { get; set; }
        public int is_mandatory { get; set; }
        public string attribute_type { get; set; }
        public string label { get; set; }
    }

    public class Option
    {
        public string name { get; set; }
    }


    public class SingleOrderReturn
    {
        public Order data { get; set; }
        public string code { get; set; }
        public string request_id { get; set; }
    }


    public class CancelReason : LazadaCommonRes
    {
        public List<CancelReasonDetail> data { get; set; }
    }

    public class CancelReasonDetail
    {
        public string name { get; set; }
        public string type { get; set; }
        public int reason_id { get; set; }
    }

}