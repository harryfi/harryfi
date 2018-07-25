using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterOnline.Models
{
    public class Lazada
    {
    }
    public class PromoLazadaObj {
        public string kdBrg { get; set; }
        public string fromDt { get; set; }
        public string toDt { get; set; }
        public string promoPrice { get; set; }
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
    public class detailUpdateBrg
    {
        public string seller_sku { get; set; }
        public string message { get; set; }
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
        public int cod { get; set; }
        public int is_default { get; set; }
        public int api_integration { get; set; }
    }

    public class LazadaToDeliver : LazadaCommonRes
    {
        public OrdersPacked data { get; set; }
    }
    public class OrdersPacked
    {
        public List<OrderPacked> OrderItems { get; set; }
    }
    public class OrderPacked
    {
        public long OrderItemId { get; set; }
        public long PurchaseOrderId { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string PackageId { get; set; }
        public string ShipmentProvider { get; set; }
        public string TrackingNumber { get; set; }
    }

}