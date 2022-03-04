﻿using Erasoft.Function;
using Hangfire;
using Hangfire.SqlServer;
using Lazop.Api;
using Lazop.Api.Util;
using MasterOnline.Models;
using MasterOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using System.Data.Entity.Validation;

namespace MasterOnline.Controllers
{
    public class TiktokControllerJob : Controller
    {
#if AWS
                        
        string eraAppKey = "3cqbhg";
        string eraAppSecret = "57fb173019d59898be333ac5af995585437ed8bf";
        string eraCallbackUrl = "https://masteronline.co.id/tiktok/auth";
#elif Debug_AWS

        string eraAppKey = "3cqbhg";
        string eraAppSecret = "57fb173019d59898be333ac5af995585437ed8bf";
        string eraCallbackUrl = "https://masteronline.co.id/tiktok/auth";
#else

        string eraAppKey = "3cqbhg";
        string eraAppSecret = "57fb173019d59898be333ac5af995585437ed8bf";
        string eraCallbackUrl = "https://dev.masteronline.co.id/tiktok/auth";

        //string eraAppKey = "101775";
        //string eraAppSecret = "QwUJjjtZ3eCy2qaz6Rv1PEXPyPaPkDSu";
        //string eraCallbackUrl = "https://masteronline.my.id/lzd/code?user=";
#endif
        DatabaseSQL EDB;
        MoDbContext MoDbContext;
        ErasoftContext ErasoftDbContext;
        string DatabasePathErasoft;
        string dbSourceEra = "";
        string username;
        // GET: TiktokController
        public ActionResult Index()
        {
            return View();
        }
        protected void SetupContext(string DatabasePathErasoft, string uname)
        {
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, DatabasePathErasoft);
            username = uname;
        }
        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrder_Insert_Tiktok(TTApiData iden, string CUST, string NAMA_CUST)
        {
            SetupContext(apidata.access_token, apidata.username);

            if (!string.IsNullOrEmpty(iden.token))
            {
                iden = await RefreshTokenShopee_V2(iden, false);
            }
            var delQry = "delete a from sot01a a left join sot01b b on a.no_bukti = b.no_bukti where isnull(b.no_bukti, '') = '' and tgl >= '";
            delQry += DateTime.UtcNow.AddHours(7).AddHours(-12).ToString("yyyy-MM-dd HH:mm:ss") + "' and cust = '" + CUST + "'";

            var resultDel = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, delQry);

            var fromDt = (long)DateTimeOffset.UtcNow.AddHours(-12).ToUnixTimeSeconds();
            var toDt = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var lanjut = true;
            var connIdProses = "";
            var nextPage = "";
            while (lanjut)
            {
                var returnGetOrder = await GetOrderList_Insert(iden, 100, CUST, NAMA_CUST, nextPage, fromDt, toDt);
                if (returnGetOrder.ConnId != "")
                {
                    connIdProses += "'" + returnGetOrder.ConnId + "' , ";
                }
                if (returnGetOrder.AdaKomponen)
                {
                    AdaKomponen = returnGetOrder.AdaKomponen;
                }
                if (!returnGetOrder.more) { lanjut = false; break; }
                nextPage = returnGetOrder.nextPage;
            }
            if (!string.IsNullOrEmpty(connIdProses))
            {
                new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username, connIdProses.Substring(0, connIdProses.Length - 3));
            }


            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust
                + "%' and arguments like '%GetOrder_Insert_Tiktok%' and statename like '%Enque%'");

            return "";
        }
        public async Task<returnsGetOrder> GetOrderList_Insert(TTApiData apidata, int order_status, string CUST, string NAMA_CUST, string page, long fromDt, long toDt)
        {
            SetupContext(apidata.access_token, apidata.username);
            string ret = new returnsGetOrder();
            string connId = Guid.NewGuid().ToString();
            string status = "";
            ret.ConnId = connId;
            var jmlhNewOrder = 0;
            string urll = "https://open-api.tiktokglobalshop.com/api/orders/search?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/orders/searchapp_key" + eraAppKey + "shop_id" + apidata.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, apidata.access_token, timestamp, signencry, eraAppKey, iden.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "POST";
            myReq.ContentType = "application/json";

            string myData = "{\"create_time_from\": " + fromDt + ",\"create_time_to\": " + toDt + ",\"order_status\": " + order_status
                + ",\"sort_type\": 1,\"sort_by\": \"CREATE_TIME\",\"page_size\":10";
            if (!string.IsNullOrEmpty(page))
            {
                myData += ",\"cursor\": \"" + page + "\"";
            }
            myData += "}";
            string responseFromServer = "";
            try
            {
                myReq.ContentLength = System.Text.Encoding.UTF8.GetBytes(myData).Length;
                using (var dataStream = myReq.GetRequestStream())
                {
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myReq.ContentLength);
                }
                using (WebResponse response = myReq.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {

            }

            if (responseFromServer != "")
            {
                var listOrder = JsonConvert.DeserializeObject(responseFromServer, typeof(GetOrderListResponse)) as GetOrderListResponse;
                if (listOrder.order_list != null)
                {
                    if (listOrder.orders.Length > 0)
                    {
                        string[] ordersn_list = listOrder.data.order_list.Select(p => p.order_id).ToArray();
                        var dariTgl = DateTimeOffset.FromUnixTimeSeconds(daysFrom).UtcDateTime.AddHours(7).AddDays(-1);

                        var SudahAdaDiMO = ErasoftDbContext.SOT01A.Where(p => p.USER_NAME == "Auto TikTok" && p.CUST == CUST && p.TGL >= dariTgl).Select(p => p.NO_REFERENSI).ToList();

                        var filtered = ordersn_list.Where(p => !SudahAdaDiMO.Contains(p));
                        if (filtered.Count() > 0)
                        {
                            await GetOrderDetails(apidata, filtered.ToArray(), connId, CUST, NAMA_CUST, order_status);
                            jmlhNewOrder = filtered.Count();
                        }

                        if (order_status == 100)//unpaid
                        {

                        }
                    }
                }
            }

            return ret;
        }

        public async Task<string> GetOrderDetails(ShopeeAPIData iden, string[] ordersn_list, string connID, string CUST, string NAMA_CUST, StatusOrder stat)
        {
            string urll = "https://open-api.tiktokglobalshop.com/api/orders/detail/query?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/orders/detail/queryapp_key" + eraAppKey + "shop_id" + apidata.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, apidata.access_token, timestamp, signencry, eraAppKey, iden.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "POST";
            myReq.ContentType = "application/json";

            GetOrderDetailsData HttpBody = new GetOrderDetailsData
            {
                ordersn_list = ordersn_list
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string responseFromServer = "";
            try
            {
                myReq.ContentLength = System.Text.Encoding.UTF8.GetBytes(myData).Length;
                using (var dataStream = myReq.GetRequestStream())
                {
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myReq.ContentLength);
                }
                using (WebResponse response = myReq.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {

            }

            if (responseFromServer != null)
            {
                //try
                //{
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(GetOrderDetailResponse)) as GetOrderDetailResponse;
                var connIdARF01C = Guid.NewGuid().ToString();
                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                TEMP_SHOPEE_ORDERS batchinsert = new TEMP_SHOPEE_ORDERS();
                List<TEMP_SHOPEE_ORDERS_ITEM> batchinsertItem = new List<TEMP_SHOPEE_ORDERS_ITEM>();
                string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                var kabKot = "3174";
                var prov = "31";
                string sqlVal = "";
                foreach (var order in result.orders)
                {
                    //add by nurul 25/8/2021, handle pembeli d samarkan ***
                    if (!order.recipient_address.name.Contains('*'))
                    //end add by nurul 25/8/2021, handle pembeli d samarkan ***
                    {
                        string nama = order.recipient_address.name.Trim().Length > 30 ? order.recipient_address.name.Trim().Substring(0, 30) : order.recipient_address.name.Trim();
                        string tlp = !string.IsNullOrEmpty(order.recipient_address.phone) ? order.recipient_address.phone.Trim().Replace('\'', '`') : "";
                        if (tlp.Length > 30)
                        {
                            tlp = tlp.Substring(0, 30);
                        }
                        //change by nurul 23/8/2021
                        //string AL_KIRIM1 = !string.IsNullOrEmpty(order.recipient_address.full_address) ? order.recipient_address.full_address.Replace('\'', '`') : "";
                        string AL_KIRIM1 = !string.IsNullOrEmpty(order.recipient_address.full_address.Trim()) ? order.recipient_address.full_address.Trim().Replace('\'', '`') : "";
                        //end change by nurul 23/8/2021
                        if (AL_KIRIM1.Length > 30)
                        {
                            AL_KIRIM1 = AL_KIRIM1.Substring(0, 30);
                        }
                        string KODEPOS = !string.IsNullOrEmpty(order.recipient_address.zipcode) ? order.recipient_address.zipcode.Trim().Replace('\'', '`') : "";
                        if (KODEPOS.Length > 7)
                        {
                            KODEPOS = KODEPOS.Substring(0, 7);
                        }

                        sqlVal += string.Format("('{0}','{1}','{2}','{3}',0,0,'0','01',1, 'IDR', '01', '{4}', 0, 0, 0, 0, '1', 0, 0,'FP', '{5}', '{6}', '{7}', '', '{8}', '{9}', '', '','{10}'),",
                            ((nama ?? "").Replace("'", "`")),
                            //change by nurul 23/8/2021
                            //((order.recipient_address.full_address ?? "").Replace("'", "`")),
                            ((order.recipient_address.full_address.Trim() ?? "").Replace("'", "`")),
                            //end change by nurul 23/8/2021
                            (tlp),
                            //(NAMA_CUST.Replace(',', '.')),
                            (NAMA_CUST.Length > 30 ? NAMA_CUST.Substring(0, 30) : NAMA_CUST),
                            (AL_KIRIM1),
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            (username),
                            (KODEPOS),
                            kabKot,
                            prov,
                            connIdARF01C
                            );
                    }
                }
                if (!string.IsNullOrEmpty(sqlVal))
                {
                    insertPembeli += sqlVal.Substring(0, sqlVal.Length - 1);
                    EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);

                    using (SqlCommand CommandSQL = new SqlCommand())
                    {
                        //call sp to insert buyer data
                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIdARF01C;

                        EDB.ExecuteSQL("Con", "MoveARF01CFromTempTable", CommandSQL);
                    };
                }
                foreach (var order in result.orders)
                {
                    try
                    {
                        //connID = Guid.NewGuid().ToString();//remark 4 jan 2020, connid sama per batch untuk update stok
                        ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_SHOPEE_ORDERS");
                        ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_SHOPEE_ORDERS_ITEM");
                        batchinsertItem = new List<TEMP_SHOPEE_ORDERS_ITEM>();
                        #region cut max length dan ubah '
                        string payment_method = !string.IsNullOrEmpty(order.payment_method) ? order.payment_method.Trim().Replace('\'', '`') : "";
                        if (payment_method.Length > 100)
                        {
                            payment_method = payment_method.Substring(0, 100);
                        }
                        string shipping_carrier = !string.IsNullOrEmpty(order.shipping_carrier) ? order.shipping_carrier.Trim().Replace('\'', '`') : "";
                        if (shipping_carrier.Length > 300)
                        {
                            shipping_carrier = shipping_carrier.Substring(0, 300);
                        }
                        string currency = !string.IsNullOrEmpty(order.currency) ? order.currency.Trim().Replace('\'', '`') : "";
                        if (currency.Length > 50)
                        {
                            currency = currency.Substring(0, 50);
                        }
                        string Recipient_Address_town = !string.IsNullOrEmpty(order.recipient_address.town) ? order.recipient_address.town.Trim().Replace('\'', '`') : "";
                        if (Recipient_Address_town.Length > 300)
                        {
                            Recipient_Address_town = Recipient_Address_town.Substring(0, 300);
                        }
                        string Recipient_Address_city = !string.IsNullOrEmpty(order.recipient_address.city) ? order.recipient_address.city.Trim().Replace('\'', '`') : "";
                        if (Recipient_Address_city.Length > 300)
                        {
                            Recipient_Address_city = Recipient_Address_city.Substring(0, 300);
                        }
                        string Recipient_Address_name = !string.IsNullOrEmpty(order.recipient_address.name) ? order.recipient_address.name.Trim().Replace('\'', '`') : "";
                        if (Recipient_Address_name.Length > 300)
                        {
                            Recipient_Address_name = Recipient_Address_name.Substring(0, 300);
                        }
                        string Recipient_Address_district = !string.IsNullOrEmpty(order.recipient_address.district) ? order.recipient_address.district.Trim().Replace('\'', '`') : "";
                        if (Recipient_Address_district.Length > 300)
                        {
                            Recipient_Address_district = Recipient_Address_district.Substring(0, 300);
                        }
                        string Recipient_Address_country = !string.IsNullOrEmpty(order.recipient_address.country) ? order.recipient_address.country.Trim().Replace('\'', '`') : "";
                        if (Recipient_Address_country.Length > 300)
                        {
                            Recipient_Address_country = Recipient_Address_country.Substring(0, 300);
                        }
                        string Recipient_Address_zipcode = !string.IsNullOrEmpty(order.recipient_address.zipcode) ? order.recipient_address.zipcode.Trim().Replace('\'', '`') : "";
                        if (Recipient_Address_zipcode.Length > 300)
                        {
                            Recipient_Address_zipcode = Recipient_Address_zipcode.Substring(0, 300);
                        }
                        string Recipient_Address_phone = !string.IsNullOrEmpty(order.recipient_address.phone) ? order.recipient_address.phone.Trim().Replace('\'', '`') : "";
                        if (Recipient_Address_phone.Length > 50)
                        {
                            Recipient_Address_phone = Recipient_Address_phone.Substring(0, 50);
                        }
                        string Recipient_Address_state = !string.IsNullOrEmpty(order.recipient_address.state) ? order.recipient_address.state.Trim().Replace('\'', '`') : "";
                        if (Recipient_Address_state.Length > 300)
                        {
                            Recipient_Address_state = Recipient_Address_state.Substring(0, 300);
                        }
                        string tracking_no = !string.IsNullOrEmpty(order.tracking_no) ? order.tracking_no.Trim().Replace('\'', '`') : "";
                        if (tracking_no.Length > 100)
                        {
                            tracking_no = tracking_no.Substring(0, 100);
                        }
                        string order_status = !string.IsNullOrEmpty(order.order_status) ? order.order_status.Trim().Replace('\'', '`') : "";
                        if (order_status.Length > 100)
                        {
                            order_status = order_status.Substring(0, 100);
                        }
                        string service_code = !string.IsNullOrEmpty(order.service_code) ? order.service_code.Trim().Replace('\'', '`') : "";
                        if (service_code.Length > 100)
                        {
                            service_code = service_code.Substring(0, 100);
                        }
                        string ordersn = !string.IsNullOrEmpty(order.ordersn) ? order.ordersn.Trim().Replace('\'', '`') : "";
                        if (ordersn.Length > 100)
                        {
                            ordersn = ordersn.Substring(0, 100);
                        }
                        string country = !string.IsNullOrEmpty(order.country) ? order.country.Trim().Replace('\'', '`') : "";
                        if (country.Length > 100)
                        {
                            country = country.Substring(0, 100);
                        }
                        string dropshipper = !string.IsNullOrEmpty(order.dropshipper) ? order.dropshipper.Trim().Replace('\'', '`') : "";
                        if (dropshipper.Length > 300)
                        {
                            dropshipper = dropshipper.Substring(0, 300);
                        }
                        string buyer_username = !string.IsNullOrEmpty(order.buyer_username) ? order.buyer_username.Trim().Replace('\'', '`') : "";
                        if (buyer_username.Length > 300)
                        {
                            buyer_username = buyer_username.Substring(0, 300);
                        }
                        if (NAMA_CUST.Length > 50)
                        {
                            NAMA_CUST = NAMA_CUST.Substring(0, 50);
                        }
                        //add by nurul 22/3/2021
                        string checkout_shipping_carrier = !string.IsNullOrEmpty(order.checkout_shipping_carrier) ? order.checkout_shipping_carrier.Trim().Replace('\'', '`') : "";
                        if (checkout_shipping_carrier.Length > 300)
                        {
                            checkout_shipping_carrier = checkout_shipping_carrier.Substring(0, 300);
                        }
                        //end add by nurul 22/3/2021
                        #endregion
                        TEMP_SHOPEE_ORDERS newOrder = new TEMP_SHOPEE_ORDERS()
                        {
                            actual_shipping_cost = order.actual_shipping_cost,
                            buyer_username = buyer_username,
                            cod = order.cod,
                            country = country,
                            create_time = DateTimeOffset.FromUnixTimeSeconds(order.create_time).UtcDateTime,
                            currency = currency,
                            days_to_ship = order.days_to_ship,
                            dropshipper = dropshipper,
                            escrow_amount = order.escrow_amount,
                            estimated_shipping_fee = order.estimated_shipping_fee,
                            goods_to_declare = order.goods_to_declare,
                            message_to_seller = (order.message_to_seller ?? "").Replace('\'', '`'),
                            note = (order.note ?? "").Replace('\'', '`'),
                            note_update_time = DateTimeOffset.FromUnixTimeSeconds(order.note_update_time).UtcDateTime,
                            ordersn = ordersn,
                            order_status = order_status,
                            payment_method = payment_method,
                            //change by nurul 5/12/2019, local time 
                            //pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime,
                            pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime.AddHours(7),
                            //end change by nurul 5/12/2019, local time 
                            Recipient_Address_country = Recipient_Address_country,
                            Recipient_Address_state = Recipient_Address_state,
                            Recipient_Address_city = Recipient_Address_city,
                            Recipient_Address_town = Recipient_Address_town,
                            Recipient_Address_district = Recipient_Address_district,
                            Recipient_Address_full_address = (order.recipient_address.full_address.Trim() ?? "").Replace('\'', '`'),
                            Recipient_Address_name = Recipient_Address_name,
                            Recipient_Address_phone = Recipient_Address_phone,
                            Recipient_Address_zipcode = Recipient_Address_zipcode,
                            service_code = service_code,
                            shipping_carrier = shipping_carrier,
                            total_amount = order.total_amount,
                            tracking_no = tracking_no,
                            update_time = DateTimeOffset.FromUnixTimeSeconds(order.update_time).UtcDateTime,
                            CONN_ID = connID,
                            CUST = CUST,
                            NAMA_CUST = NAMA_CUST,
                            //add by nurul 22/3/2021
                            checkout_shipping_carrier = checkout_shipping_carrier
                            //end add by nurul 22/3/2021
                        };
                        //add 27 okt 2020, expired shipping date
                        newOrder.ship_by_date = null;
                        if (order.ship_by_date > 0)
                        {
                            newOrder.ship_by_date = DateTimeOffset.FromUnixTimeSeconds(order.ship_by_date).UtcDateTime.AddHours(7);
                        }
                        //end add 27 okt 2020, expired shipping date
                        var ShippingFeeData = await GetShippingFee(iden, order.ordersn);
                        if (ShippingFeeData != null)
                        {
                            newOrder.estimated_shipping_fee = (ShippingFeeData.order_income.buyer_paid_shipping_fee + ShippingFeeData.order_income.shopee_shipping_rebate - ShippingFeeData.order_income.actual_shipping_fee).ToString();
                            if (ShippingFeeData.order_income.buyer_paid_shipping_fee + ShippingFeeData.order_income.shopee_shipping_rebate - ShippingFeeData.order_income.actual_shipping_fee < 0)
                            {
                                newOrder.estimated_shipping_fee = "0";
                            }
                        }
                        //var listPromo = new Dictionary<long, double>();//add 6 juli 2020
                        var listPromo = new Dictionary<long, List<Activity>>();//add 6 juli 2020
                        foreach (var item in order.items)
                        {
                            string item_name = !string.IsNullOrEmpty(item.item_name) ? item.item_name.Replace('\'', '`') : "";
                            if (item_name.Length > 400)
                            {
                                item_name = item_name.Substring(0, 400);
                            }
                            string item_sku = !string.IsNullOrEmpty(item.item_sku) ? item.item_sku.Replace('\'', '`') : "";
                            if (item_sku.Length > 400)
                            {
                                item_sku = item_sku.Substring(0, 400);
                            }
                            string variation_name = !string.IsNullOrEmpty(item.variation_name) ? item.variation_name.Replace('\'', '`') : "";
                            if (variation_name.Length > 400)
                            {
                                variation_name = variation_name.Substring(0, 400);
                            }
                            string variation_sku = !string.IsNullOrEmpty(item.variation_sku) ? item.variation_sku.Replace('\'', '`') : "";
                            if (variation_sku.Length > 400)
                            {
                                variation_sku = variation_sku.Substring(0, 400);
                            }

                            TEMP_SHOPEE_ORDERS_ITEM newOrderItem = new TEMP_SHOPEE_ORDERS_ITEM()
                            {
                                ordersn = ordersn,
                                is_wholesale = item.is_wholesale,
                                item_id = item.item_id,
                                item_name = item_name,
                                item_sku = item_sku,
                                variation_discounted_price = item.variation_discounted_price,
                                variation_id = item.variation_id,
                                variation_name = variation_name,
                                variation_original_price = item.variation_original_price,
                                variation_quantity_purchased = item.variation_quantity_purchased,
                                variation_sku = variation_sku,
                                weight = item.weight,
                                pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime,
                                CONN_ID = connID,
                                CUST = CUST,
                                NAMA_CUST = NAMA_CUST
                            };
                            if (!string.IsNullOrEmpty(item.promotion_type))
                            {
                                if (item.promotion_type == "bundle_deal")
                                {
                                    var discount = 0d;
                                    if (!listPromo.ContainsKey(item.promotion_id))
                                    {
                                        var dataDisc = await GetEscrowDetail(iden, order.ordersn, item.item_id, item.variation_id, item.promotion_id);
                                        //if (dataDisc.activity_id > 0)
                                        //{
                                        //    listPromo.Add(item.promotion_id, dataDisc);
                                        //}
                                        if (dataDisc.Count > 0)
                                        {
                                            listPromo.Add(item.promotion_id, dataDisc);
                                        }
                                    }
                                    //else
                                    //{
                                    //    discount = listPromo[item.promotion_id];
                                    //}
                                    newOrderItem.variation_discounted_price = item.variation_original_price;
                                    var dDisc = listPromo[item.promotion_id];
                                    foreach (var listAct in dDisc)
                                    {
                                        if (listAct.activity_id == item.promotion_id)
                                        {
                                            foreach (var disc in listAct.items)
                                            {
                                                if (item.item_id == disc.item_id && item.variation_id == disc.variation_id)
                                                {
                                                    discount = (Convert.ToInt64(listAct.original_price) - Convert.ToInt64(listAct.discounted_price))
                                                        * 100 / Convert.ToInt64(listAct.original_price);
                                                    newOrderItem.variation_discounted_price = disc.original_price;
                                                }
                                            }
                                        }

                                    }
                                    newOrderItem.DISC = discount;
                                    newOrderItem.N_DISC = Convert.ToInt64(newOrderItem.variation_discounted_price) * newOrderItem.variation_quantity_purchased * discount / 100;
                                }
                            }
                            batchinsertItem.Add(newOrderItem);
                        }
                        batchinsert = (newOrder);

                        ErasoftDbContext.TEMP_SHOPEE_ORDERS.Add(batchinsert);
                        ErasoftDbContext.TEMP_SHOPEE_ORDERS_ITEM.AddRange(batchinsertItem);
                        ErasoftDbContext.SaveChanges();

                        //add 3 Des 2020
                        EDB.ExecuteSQL("Con", CommandType.Text, "DELETE FROM TEMP_SHOPEE_ORDERS_ITEM WHERE ordersn <> '" + ordersn + "'");
                        //end add 3 Des 2020
                        using (SqlCommand CommandSQL = new SqlCommand())
                        {
                            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connID;
                            CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                            CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 1;
                            CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                            EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
                        }
                    }
                    catch (Exception ex3)
                    {

                    }
                }
            }
            return ret;
        }

    }
}


public class returnsGetOrder
{
    public string ConnId { get; set; }
    public int page { get; set; }
    public int jmlhNewOrder { get; set; }
    public int jmlhPesananDibayar { get; set; }
    public bool more { get; set; }
    public bool AdaKomponen { get; set; }
    public string nextPage { get; set; }
}

public class GetOrderListResponse
{
    public int code { get; set; }
    public string message { get; set; }
    public string request_id { get; set; }
    public GetOrderListData data { get; set; }
}

public class GetOrderListData
{
    public Order_List[] order_list { get; set; }
    public bool more { get; set; }
    public string next_cursor { get; set; }
}

public class Order_List
{
    public string order_id { get; set; }
    public int order_status { get; set; }
    public long update_time { get; set; }
}
public class GetOrderDetailsData
{
    public string[] order_id_list { get; set; }
}

public class GetOrderDetailResponse
{
    public int code { get; set; }
    public string message { get; set; }
    public string request_id { get; set; }
    public GetOrderDetailData data { get; set; }
}

public class GetOrderDetailData
{
    public OrderDetail_List[] order_list { get; set; }
}

public class OrderDetail_List
{
    public string order_id { get; set; }
    public int order_status { get; set; }
    public string payment_method { get; set; }
    public string delivery_option { get; set; }
    public string shipping_provider { get; set; }
    public string shipping_provider_id { get; set; }
    public string create_time { get; set; }
    public long paid_time { get; set; }
    public string buyer_message { get; set; }
    public Payment_Info payment_info { get; set; }
    public Recipient_Address recipient_address { get; set; }
    public string tracking_number { get; set; }
    public Item_List[] item_list { get; set; }
    public long rts_time { get; set; }
    public long rts_sla { get; set; }
    public long tts_sla { get; set; }
    public long cancel_order_sla { get; set; }
    public long receiver_address_updated { get; set; }
    public long update_time { get; set; }
}

public class Payment_Info
{
    public string currency { get; set; }
    public int sub_total { get; set; }
    public int shipping_fee { get; set; }
    public int seller_discount { get; set; }
    public int total_amount { get; set; }
    public int original_total_product_price { get; set; }
    public int original_shipping_fee { get; set; }
    public int shipping_fee_seller_discount { get; set; }
    public int shipping_fee_platform_discount { get; set; }
}

public class Recipient_Address
{
    public string full_address { get; set; }
    public string region { get; set; }
    public string state { get; set; }
    public string city { get; set; }
    public string district { get; set; }
    public string town { get; set; }
    public string phone { get; set; }
    public string name { get; set; }
    public string zipcode { get; set; }
    public string address_detail { get; set; }
    public string[] address_line_list { get; set; }
}

public class Item_List
{
    public string sku_id { get; set; }
    public string product_id { get; set; }
    public string product_name { get; set; }
    public string sku_name { get; set; }
    public string sku_image { get; set; }
    public int quantity { get; set; }
    public string seller_sku { get; set; }
    public int sku_original_price { get; set; }
    public int sku_sale_price { get; set; }
}

