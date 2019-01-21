using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using MasterOnline.Models;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using MasterOnline.ViewModels;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Erasoft.Function;
using System.Xml;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Net.Http;
namespace MasterOnline.Controllers
{
    public class TokopediaController : Controller
    {
        AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);//string auth = Base64Encode();

        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        DatabaseSQL EDB;
        string username;
        public TokopediaController()
        {
            MoDbContext = new MoDbContext();
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
            {
                if (sessionData.Account.UserId == "admin_manage")
                    ErasoftDbContext = new ErasoftContext();
                else
                    ErasoftDbContext = new ErasoftContext(sessionData.Account.DatabasePathErasoft);

                EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);
                username = sessionData.Account.Username;
            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    ErasoftDbContext = new ErasoftContext(accFromUser.DatabasePathErasoft);
                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                    username = accFromUser.Username;
                }
            }
        }
        public async Task<string> PostAckOrder(TokopediaAPIData iden, string ordNo)
        {
            string ret = "";
            string urll = "https://fs.tokopedia.net//v1/order/" + Uri.EscapeDataString(ordNo) + "/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/ack";
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Accept Order",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = "fs : " + iden.merchant_code,
                REQUEST_ATTRIBUTE_2 = "orderNo : " + ordNo,
                REQUEST_STATUS = "Pending",
            };
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            AckOrder newData = new AckOrder();
            var detailSO = ErasoftDbContext.SOT01B.Where(p => p.NO_BUKTI == ordNo).ToList();
            foreach (var item in detailSO)
            {
                AckOrder_Product product = new AckOrder_Product
                {
                    product_id = item.BRG,
                    quantity_deliver = item.QTY,
                    quantity_reject = 0
                };
                newData.products.Add(product);
            }
            string myData = JsonConvert.SerializeObject(newData);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            try
            {
                myReq.ContentLength = myData.Length;
                using (var dataStream = myReq.GetRequestStream())
                {
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
                }
                using (WebResponse response = await myReq.GetResponseAsync())
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
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }
            if (responseFromServer != null)
            {
                TokopediaOrders result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaOrders)) as TokopediaOrders;
                //if (string.IsNullOrEmpty(result.errorCode.Value))
                //{
                //    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                //    if (result.content.Count > 0)
                //    {
                //        foreach (var item in result.content)
                //        {
                //            await GetOrderDetail(iden, item.orderNo.Value, item.orderItemNo.Value, connId, CUST, NAMA_CUST);
                //        }
                //    }
                //}
                //else
                //{
                //    currentLog.REQUEST_RESULT = result.errorCode.Value;
                //    currentLog.REQUEST_EXCEPTION = result.errorMessage.Value;
                //    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                //}
            }
            return ret;
        }
        public async Task<string> GetOrderList(TokopediaAPIData iden, StatusOrder stat, string connId, string CUST, string NAMA_CUST)
        {
            //if merchant code diisi. barulah GetOrderList
            string ret = "";
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";

            //complete list of order status at https://fs.tokopedia.net/docs#order-status-codes
            //400 seller accepted the order
            //401 seller accepted the order, partially
            //10  seller rejected the order
            //500 seller confirm for shipment

            switch (stat)
            {
                case StatusOrder.Cancel:
                    //Cancel Order
                    status = "0";
                    break;
                case StatusOrder.Paid:
                    //paid
                    status = "200";
                    break;
                //case StatusOrder.PackagingINP:
                //    status = "500";
                //    break;
                case StatusOrder.ShippingINP:
                    //Shipping in Progress
                    status = "500";
                    break;
                case StatusOrder.Completed:
                    //Completed (Shipping)
                    status = "610";
                    break;
                default:
                    break;
            }
            long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

            string urll = "https://fs.tokopedia.net/v1/order/list?fs_id=" + Uri.EscapeDataString(iden.merchant_code) + "&from_date=" + Convert.ToString(unixTimestampFrom) + "&to_date=" + Convert.ToString(unixTimestampTo) + "&page=1&per_page=100&shop_id=" + Uri.EscapeDataString(iden.API_secret_key);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Get Order List",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = stat.ToString(),
                REQUEST_STATUS = "Pending",
            };
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            //myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            //myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            //myReq.Headers.Add("requestId", milis.ToString());
            //myReq.Headers.Add("sessionId", milis.ToString());
            //myReq.Headers.Add("username", userMTA);
            string responseFromServer = "";
            try
            {
                using (WebResponse response = await myReq.GetResponseAsync())
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
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }
            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                TokopediaOrders result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaOrders)) as TokopediaOrders;
                var orderPaid = result.data.Where(p => p.order_status == 220).ToList();
                var orderTokpedInDb = ErasoftDbContext.TEMP_TOKPED_ORDERS.Where(p => p.fs_id == iden.merchant_code);
                List<TEMP_TOKPED_ORDERS> ListNewOrders = new List<TEMP_TOKPED_ORDERS>();
                var connIdARF01C = Guid.NewGuid().ToString();

                foreach (var order in orderPaid)
                {
                    ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_TOKPED_ORDERS");

                    string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                    insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                    insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                    var kabKot = "3174";
                    var prov = "31";
                    insertPembeli += "('" + order.recipient.name + "','" + order.recipient.address.address_full + "','" + order.recipient.phone + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
                    insertPembeli += "1, 'IDR', '01', '" + order.recipient.address.address_full + "', 0, 0, 0, 0, '1', 0, 0, ";
                    insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.recipient.address.postal_code + "', '', '" + kabKot + "', '" + prov + "', '', '','" + connIdARF01C + "'),";

                    var order_order_id = Convert.ToString(order.order_id);
                    if (orderTokpedInDb.Where(p => p.order_id == order_order_id).Count() == 0)
                    {
                        //belum ada di temp
                        foreach (var product in order.products)
                        {
                            TEMP_TOKPED_ORDERS newOrder = new TEMP_TOKPED_ORDERS()
                            {
                                fs_id = order.fs_id,
                                order_id = Convert.ToString(order.order_id),
                                accept_partial = order.accept_partial,
                                invoice_ref_num = order.invoice_ref_num,
                                product_id = product.id,
                                product_name = product.name,
                                product_quantity = product.quantity,
                                product_notes = product.notes,
                                product_weight = product.weight,
                                product_total_weight = product.total_weight,
                                product_price = product.price,
                                product_total_price = product.total_price,
                                product_currency = product.currency,
                                product_sku = product.sku,
                                products_fulfilled_product_id = 0,
                                products_fulfilled_quantity_deliver = 0,
                                products_fulfilled_quantity_reject = 0,
                                device_type = order.device_type,
                                buyer_id = order.buyer.id,
                                buyer_name = order.buyer.name,
                                buyer_email = order.buyer.email,
                                buyer_phone = order.buyer.phone,
                                shop_id = order.shop_id,
                                payment_id = order.payment_id,
                                recipient_name = order.recipient.name,
                                recipient_address_address_full = order.recipient.address.address_full,
                                recipient_address_district = order.recipient.address.district,
                                recipient_address_district_id = order.recipient.address.district_id,
                                recipient_address_city = order.recipient.address.city,
                                recipient_address_city_id = order.recipient.address.city_id,
                                recipient_address_province = order.recipient.address.province,
                                recipient_address_province_id = order.recipient.address.province_id,
                                recipient_address_country = order.recipient.address.country,
                                recipient_address_geo = order.recipient.address.geo,
                                recipient_address_postal_code = order.recipient.address.postal_code,
                                recipient_phone = order.recipient.phone,
                                logistics_shipping_id = order.logistics.shipping_id,
                                logistics_shipping_agency = order.logistics.shipping_agency,
                                logistics_service_type = order.logistics.service_type,
                                amt_ttl_product_price = order.amt.ttl_product_price,
                                amt_shipping_cost = order.amt.shipping_cost,
                                amt_insurance_cost = order.amt.insurance_cost,
                                amt_ttl_amount = order.amt.ttl_amount,
                                amt_voucher_amount = order.amt.voucher_amount,
                                amt_toppoints_amount = order.amt.toppoints_amount,
                                dropshipper_info_name = order.dropshipper_info.name,
                                dropshipper_info_phone = order.dropshipper_info.phone,
                                voucher_info_voucher_code = order.voucher_info.voucher_code,
                                voucher_info_voucher_type = order.voucher_info.voucher_type,
                                order_status = order.order_status,
                                create_time = DateTimeOffset.FromUnixTimeSeconds(order.create_time).UtcDateTime,
                                custom_fields_awb = order.custom_fields.awb,
                                conn_id = connId,
                                CUST = CUST,
                                NAMA_CUST = NAMA_CUST
                            };
                            var product_fulfilled = order.products_fulfilled.SingleOrDefault(p => p.product_id == product.id);
                            if (product_fulfilled != null)
                            {
                                newOrder.products_fulfilled_product_id = product_fulfilled.product_id;
                                newOrder.products_fulfilled_quantity_deliver = product_fulfilled.quantity_deliver;
                                newOrder.products_fulfilled_quantity_reject = product_fulfilled.quantity_reject;
                            }
                            ListNewOrders.Add(newOrder);
                        }
                    }

                    insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
                    EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);

                    ErasoftDbContext.TEMP_TOKPED_ORDERS.AddRange(ListNewOrders);
                    ErasoftDbContext.SaveChanges();

                    using (SqlCommand CommandSQL = new SqlCommand())
                    {
                        //call sp to insert buyer data
                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIdARF01C;

                        EDB.ExecuteSQL("Con", "MoveARF01CFromTempTable", CommandSQL);
                    };
                    using (SqlCommand CommandSQL = new SqlCommand())
                    {
                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connId;
                        CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                        CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 1;
                        CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                        EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
                    }
                }
            }
            return ret;
        }

        public async Task<ItemListResult> GetItemList(TokopediaAPIData iden, string connId, string CUST, string NAMA_CUST, string product_id)
        {
            //if merchant code diisi. barulah GetOrderList
            var ret = new ItemListResult();
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";

            long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

            string urll = "https://fs.tokopedia.net/v1/products/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/1/100?product_id=" + Uri.EscapeDataString(product_id);

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = milis.ToString(),
            //    REQUEST_ACTION = "Get Item List",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = stat.ToString(),
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            try
            {
                using (WebResponse response = await myReq.GetResponseAsync())
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
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }
            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ItemListResult)) as ItemListResult;
                bool adaError = false;
                if (result.error_message != null)
                {
                    if (result.error_message.Count() > 0)
                    {
                        adaError = true;
                    }
                }
                if (!adaError)
                {
                    ret = result;
                }
            }

            return ret;
        }

        public async Task<BindingBase> GetActiveItemList(TokopediaAPIData iden,int page,int recordCount, string CUST, string NAMA_CUST, int recnumArf01)
        {
            var connId = Guid.NewGuid().ToString();

            BindingBase ret = new BindingBase();
            ret.message = "";// jika perlu lanjut recursive, isi
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";

            long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
            int rows = 10;
            int Rowsstart = page * rows;
            //order by name
            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/list?";
            string queryParam = "shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&rows=" + Uri.EscapeDataString(Convert.ToString(rows)) + "&start=" + Uri.EscapeDataString(Convert.ToString(Rowsstart)) + "&product_id=&order_by=12&keyword=&exclude_keyword=&sku=&price_min=1&price_max=500000000&preorder=false&free_return=false&wholesale=false";
            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = milis.ToString(),
            //    REQUEST_ACTION = "Get Item List",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = stat.ToString(),
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);


            string responseFromServer = "";
            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + iden.token));
            //HttpResponseMessage clientResponse = await client.GetAsync(
            //    urll + queryParam);

            //using (HttpContent responseContent = clientResponse.Content)
            //{
            //    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
            //    {
            //        responseFromServer = await reader.ReadToEndAsync();
            //    }
            //};

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + queryParam);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            try
            {
                using (WebResponse response = await myReq.GetResponseAsync())
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
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }
            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ActiveProductListResult)) as ActiveProductListResult;
                if (result.header.error_code == 0)
                {
                    if (Rowsstart + rows >= result.data.total_data)
                    {

                    }
                    ret.status = 1;
                    ret.recordCount = recordCount;
                    foreach (var item in result.data.products)
                    {
                        bool adaChild = false;
                        if (item.childs != null)
                        {
                            if (item.childs.Count() > 0)
                            {
                                adaChild = true;
                                await GetActiveItemVariant(iden, connId, CUST, NAMA_CUST, recnumArf01, item,ret);
                            }
                        }
                        //if (!adaChild)
                        //{
                        //    string namaBrg = item.name;
                        //    string nama, nama2, nama3, urlImage, urlImage2, urlImage3;
                        //    urlImage = "";
                        //    urlImage2 = "";
                        //    urlImage3 = "";
                        //    if (namaBrg.Length > 30)
                        //    {
                        //        nama = namaBrg.Substring(0, 30);
                        //        if (namaBrg.Length > 60)
                        //        {
                        //            nama2 = namaBrg.Substring(30, 30);
                        //            nama3 = (namaBrg.Length > 90) ? namaBrg.Substring(60, 30) : namaBrg.Substring(60);
                        //        }
                        //        else
                        //        {
                        //            nama2 = namaBrg.Substring(30);
                        //            nama3 = "";
                        //        }
                        //    }
                        //    else
                        //    {
                        //        nama = namaBrg;
                        //        nama2 = "";
                        //        nama3 = "";
                        //    }
                        //    var dsa = await GetItemList(iden, connId, CUST, NAMA_CUST, Convert.ToString(item.id));
                        //    Models.TEMP_BRG_MP newrecord = new TEMP_BRG_MP()
                        //    {
                        //        SELLER_SKU = item.sku,
                        //        BRG_MP = Convert.ToString(item.id),
                        //        NAMA = nama,
                        //        NAMA2 = nama2,
                        //        NAMA3 = nama3,
                        //        CATEGORY_CODE = Convert.ToString(item.category_id),
                        //        CATEGORY_NAME = item.category_name,
                        //        IDMARKET = recnumArf01,
                        //        IMAGE = item.image_url_700,
                        //        CUST = CUST
                        //    };
                        //}
                    }
                }
            }

            return ret;
        }

        public async Task<string> GetActiveItemVariant(TokopediaAPIData iden, string connId, string CUST, string NAMA_CUST, int recnumArf01, ActiveProductListResultProduct parent,BindingBase retParent)
        {
            string ret = "";
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";

            long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

            //order by name
            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/variant/" + Uri.EscapeDataString(Convert.ToString(parent.id));

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = milis.ToString(),
            //    REQUEST_ACTION = "Get Item List",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = stat.ToString(),
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            string responseFromServer = "";
            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + iden.token));
            //HttpResponseMessage clientResponse = await client.GetAsync(
            //    urll + queryParam);

            //using (HttpContent responseContent = clientResponse.Content)
            //{
            //    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
            //    {
            //        responseFromServer = await reader.ReadToEndAsync();
            //    }
            //};

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            try
            {
                using (WebResponse response = await myReq.GetResponseAsync())
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
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ActiveProductVariantResult)) as ActiveProductVariantResult;
                if (result.header.error_code == 200)
                {
                    List<TEMP_BRG_MP> listNewData = new List<TEMP_BRG_MP>();

                    foreach (var item in result.data.children)
                    {
                        string brgMp = Convert.ToString(item.product_id);
                        var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.ToUpper().Equals(brgMp.ToUpper()) && t.IDMARKET == recnumArf01).FirstOrDefault();
                        var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(brgMp) && t.IDMARKET == recnumArf01).FirstOrDefault();
                        if (tempbrginDB == null && brgInDB == null)
                        {
                            string namaBrg = item.name;
                            string nama, nama2, nama3, urlImage, urlImage2, urlImage3;
                            urlImage = "";
                            urlImage2 = "";
                            urlImage3 = "";
                            if (namaBrg.Length > 30)
                            {
                                nama = namaBrg.Substring(0, 30);
                                if (namaBrg.Length > 60)
                                {
                                    nama2 = namaBrg.Substring(30, 30);
                                    nama3 = (namaBrg.Length > 90) ? namaBrg.Substring(60, 30) : namaBrg.Substring(60);
                                }
                                else
                                {
                                    nama2 = namaBrg.Substring(30);
                                    nama3 = "";
                                }
                            }
                            else
                            {
                                nama = namaBrg;
                                nama2 = "";
                                nama3 = "";
                            }

                            var desc = await GetItemList(iden, connId, CUST, NAMA_CUST, Convert.ToString(parent.id));

                            string description = "";
                            foreach (var dtail in desc.data)
                            {
                                description = dtail.desc;
                            }
                            Models.TEMP_BRG_MP newrecord = new TEMP_BRG_MP()
                            {
                                SELLER_SKU = item.sku,
                                BRG_MP = Convert.ToString(item.product_id),
                                NAMA = nama,
                                NAMA2 = nama2,
                                NAMA3 = nama3,
                                CATEGORY_CODE = Convert.ToString(parent.category_id),
                                CATEGORY_NAME = parent.category_name,
                                IDMARKET = recnumArf01,
                                IMAGE = parent.image_url_700,
                                DISPLAY = item.is_buyable,
                                HJUAL = item.price,
                                HJUAL_MP = item.price,
                                Deskripsi = description,
                                MEREK = "OEM",
                                CUST = CUST
                            };
                            listNewData.Add(newrecord);
                            retParent.recordCount = retParent.recordCount + 1;
                        }
                    }
                    if (listNewData.Count() > 0)
                    {
                        ErasoftDbContext.TEMP_BRG_MP.AddRange(listNewData);
                        ErasoftDbContext.SaveChanges();
                    }
                }
            }
            return ret;
        }

        //public TokopediaToken GetToken(TokopediaAPIData data, bool syncData)
        public TokopediaToken GetToken(TokopediaAPIData data)
        {
            var ret = new TokopediaToken();
            var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.API_CLIENT_P.Equals(data.API_client_password) && p.API_CLIENT_U.Equals(data.API_client_username)).SingleOrDefault();
            if (arf01inDB != null)
            {
                string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
                //string apiId = "36bc3d7bcc13404c9e670a84f0c61676:8a76adc52d144a9fa1ef4f96b59b7419";
                //apiId = "mta-api-sandbox:sandbox-secret-key";
                //string urll = "https://apisandbox.blibli.com/v2/oauth/token?grant_type=client_credentials";

                string urll = "https://accounts.tokopedia.com/token";
                //string urll = "https://accounts-staging.tokopedia.com";

                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "POST";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(apiId))));
                myReq.ContentType = "application/x-www-form-urlencoded";
                myReq.Accept = "application/json";
                string myData = "grant_type=client_credentials";
                //Stream dataStream = myReq.GetRequestStream();
                //WebResponse response = myReq.GetResponse();
                //dataStream = response.GetResponseStream();
                //StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = "";
                try
                {
                    myReq.ContentLength = myData.Length;
                    using (var dataStream = myReq.GetRequestStream())
                    {
                        dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
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
                // nilai token yg diambil adalah access-token. setelah 24jam biasanya harus masuk ke refresh token. dan harus diambil lagi acces token yg baru
                if (responseFromServer != "")
                {
                    ret = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaToken)) as TokopediaToken;
                    if (ret.error == null)
                    {
                        arf01inDB.TOKEN = ret.access_token;
                        arf01inDB.STATUS_API = "1";
                        ErasoftDbContext.SaveChanges();
                    }
                    else
                    {

                    }
                }
            }
            //}
            return ret;
        }
        //categoryAPIResult

        public async Task<string> GetCategoryTree(TokopediaAPIData data)
        {
            string ret = "";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(data.merchant_code) + "/product/category";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + data.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            try
            {
                using (WebResponse response = await myReq.GetResponseAsync())
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
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(categoryAPIResult)) as categoryAPIResult;
                if (string.IsNullOrEmpty(result.header.reason))
                {
                    if (result.data.categories.Count() > 0)
                    {
#if AWS
                        string con = "Data Source=localhost;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#elif Debug_AWS
                        string con = "Data Source=13.250.232.74;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#else
                        string con = "Data Source=13.251.222.53;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#endif

                        using (SqlConnection oConnection = new SqlConnection(con))
                        {
                            oConnection.Open();
                            //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                            //{
                            using (SqlCommand oCommand = oConnection.CreateCommand())
                            {
                                //oCommand.CommandText = "DELETE FROM [CATEGORY_BLIBLI] WHERE ARF01_SORT1_CUST='" + data.merchant_code + "'";
                                //oCommand.ExecuteNonQuery();
                                //oCommand.Transaction = oTransaction;
                                oCommand.CommandType = CommandType.Text;
                                oCommand.CommandText = "INSERT INTO [CATEGORY_TOKPED] ([CATEGORY_CODE], [CATEGORY_NAME], [PARENT_CODE], [IS_LAST_NODE], [MASTER_CATEGORY_CODE]) VALUES (@CATEGORY_CODE, @CATEGORY_NAME, @PARENT_CODE, @IS_LAST_NODE, @MASTER_CATEGORY_CODE)";
                                //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                                oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
                                oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));
                                oCommand.Parameters.Add(new SqlParameter("@PARENT_CODE", SqlDbType.NVarChar, 50));
                                oCommand.Parameters.Add(new SqlParameter("@IS_LAST_NODE", SqlDbType.NVarChar, 1));
                                oCommand.Parameters.Add(new SqlParameter("@MASTER_CATEGORY_CODE", SqlDbType.NVarChar, 50));

                                try
                                {
                                    //oCommand.Parameters[0].Value = data.merchant_code;
                                    foreach (var item in result.data.categories) //foreach parent level top
                                    {
                                        oCommand.Parameters[0].Value = item.id;
                                        oCommand.Parameters[1].Value = item.name;
                                        oCommand.Parameters[2].Value = "";
                                        oCommand.Parameters[3].Value = item.child == null ? "1" : item.child.Count() == 0 ? "1" : "0";
                                        oCommand.Parameters[4].Value = "";
                                        if (oCommand.ExecuteNonQuery() == 1)
                                        {
                                            if ((item.child == null ? 0 : item.child.Count()) > 0)
                                            {
                                                RecursiveInsertCategory(oCommand, item.child, item.id, item.id, data);
                                            }
                                            //throw new InvalidProgramException();
                                        }
                                    }
                                    //oTransaction.Commit();
                                }
                                catch (Exception ex)
                                {
                                    //oTransaction.Rollback();
                                }
                            }
                            //}
                        }
                        //await GetAttributeList(data);
                    }
                }
            }

            return ret;
        }


        //        public async Task<string> GetAttribute(TokopediaAPIData data, string catid)
        //        {
        //            string ret = "";

        //            long milis = CurrentTimeMillis();
        //            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

        //            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(data.merchant_code) + "/category/get_variant?cat_id=" + Uri.EscapeDataString(catid);

        //            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
        //            myReq.Method = "GET";
        //            myReq.Headers.Add("Authorization", ("Bearer " + data.token));
        //            myReq.Accept = "application/x-www-form-urlencoded";
        //            myReq.ContentType = "application/json";
        //            string responseFromServer = "";
        //            try
        //            {
        //                using (WebResponse response = await myReq.GetResponseAsync())
        //                {
        //                    using (Stream stream = response.GetResponseStream())
        //                    {
        //                        StreamReader reader = new StreamReader(stream);
        //                        responseFromServer = reader.ReadToEnd();
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {

        //            }

        //            if (responseFromServer != null)
        //            {
        //                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(categoryAPIResult)) as categoryAPIResult;
        //                if (string.IsNullOrEmpty(result.header.reason))
        //                {
        //                    if (result.data.categories.Count() > 0)
        //                    {178
        //#if AWS
        //                        string con = "Data Source=localhost;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
        //#elif Debug_AWS
        //                        string con = "Data Source=13.250.232.74;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
        //#else
        //                        string con = "Data Source=13.251.222.53;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
        //#endif

        //                        using (SqlConnection oConnection = new SqlConnection(con))
        //                        {
        //                            oConnection.Open();
        //                            //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
        //                            //{
        //                            using (SqlCommand oCommand = oConnection.CreateCommand())
        //                            {
        //                                //oCommand.CommandText = "DELETE FROM [CATEGORY_BLIBLI] WHERE ARF01_SORT1_CUST='" + data.merchant_code + "'";
        //                                //oCommand.ExecuteNonQuery();
        //                                //oCommand.Transaction = oTransaction;
        //                                oCommand.CommandType = CommandType.Text;
        //                                oCommand.CommandText = "INSERT INTO [CATEGORY_TOKPED] ([CATEGORY_CODE], [CATEGORY_NAME], [PARENT_CODE], [IS_LAST_NODE], [MASTER_CATEGORY_CODE]) VALUES (@CATEGORY_CODE, @CATEGORY_NAME, @PARENT_CODE, @IS_LAST_NODE, @MASTER_CATEGORY_CODE)";
        //                                //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
        //                                oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
        //                                oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));
        //                                oCommand.Parameters.Add(new SqlParameter("@PARENT_CODE", SqlDbType.NVarChar, 50));
        //                                oCommand.Parameters.Add(new SqlParameter("@IS_LAST_NODE", SqlDbType.NVarChar, 1));
        //                                oCommand.Parameters.Add(new SqlParameter("@MASTER_CATEGORY_CODE", SqlDbType.NVarChar, 50));

        //                                try
        //                                {
        //                                    //oCommand.Parameters[0].Value = data.merchant_code;
        //                                    foreach (var item in result.data.categories) //foreach parent level top
        //                                    {
        //                                        oCommand.Parameters[0].Value = item.id;
        //                                        oCommand.Parameters[1].Value = item.name;
        //                                        oCommand.Parameters[2].Value = "";
        //                                        oCommand.Parameters[3].Value = item.child == null ? "1" : item.child.Count() == 0 ? "1" : "0";
        //                                        oCommand.Parameters[4].Value = "";
        //                                        if (oCommand.ExecuteNonQuery() == 1)
        //                                        {
        //                                            if ((item.child == null ? 0 : item.child.Count()) > 0)
        //                                            {
        //                                                RecursiveInsertCategory(oCommand, item.child, item.id, item.id, data);
        //                                            }
        //                                            //throw new InvalidProgramException();
        //                                        }
        //                                    }
        //                                    //oTransaction.Commit();
        //                                }
        //                                catch (Exception ex)
        //                                {
        //                                    //oTransaction.Rollback();
        //                                }
        //                            }
        //                            //}
        //                        }
        //                        //await GetAttributeList(data);
        //                    }
        //                }
        //            }

        //            return ret;
        //            var categories = MoDbContext.CategoryShopee.Where(p => p.IS_LAST_NODE.Equals("1")).ToList();
        //            foreach (var category in categories)
        //            {
        //                long seconds = CurrentTimeSecond();
        //                DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

        //                //ganti
        //                string urll = "https://partner.shopeemobile.com/api/v1/item/attributes/get";

        //                //ganti
        //                ShopeeGetAttributeData HttpBody = new ShopeeGetAttributeData
        //                {
        //                    partner_id = MOPartnerID,
        //                    shopid = Convert.ToInt32(iden.merchant_code),
        //                    timestamp = seconds,
        //                    language = "id",
        //                    category_id = Convert.ToInt32(category.CATEGORY_CODE)
        //                };

        //                string myData = JsonConvert.SerializeObject(HttpBody);

        //                string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

        //                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
        //                myReq.Method = "POST";
        //                myReq.Headers.Add("Authorization", signature);
        //                myReq.Accept = "application/json";
        //                myReq.ContentType = "application/json";
        //                string responseFromServer = "";
        //                try
        //                {
        //                    myReq.ContentLength = myData.Length;
        //                    using (var dataStream = myReq.GetRequestStream())
        //                    {
        //                        dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
        //                    }
        //                    using (WebResponse response = await myReq.GetResponseAsync())
        //                    {
        //                        using (Stream stream = response.GetResponseStream())
        //                        {
        //                            StreamReader reader = new StreamReader(stream);
        //                            responseFromServer = reader.ReadToEnd();
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                }

        //                if (responseFromServer != null)
        //                {
        //                    try
        //                    {
        //                        var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetAttributeResult)) as ShopeeGetAttributeResult;
        //#if AWS
        //                        string con = "Data Source=localhost;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
        //#elif Debug_AWS
        //                        string con = "Data Source=13.250.232.74;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
        //#else
        //                        string con = "Data Source=13.251.222.53;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
        //#endif
        //                        using (SqlConnection oConnection = new SqlConnection(con))
        //                        {
        //                            oConnection.Open();
        //                            //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
        //                            //{
        //                            using (SqlCommand oCommand = oConnection.CreateCommand())
        //                            {
        //                                var AttributeInDb = MoDbContext.AttributeShopee.ToList();
        //                                //cek jika belum ada di database, insert
        //                                var cari = AttributeInDb.Where(p => p.CATEGORY_CODE.ToUpper().Equals(category.CATEGORY_CODE));
        //                                if (cari.Count() == 0)
        //                                {
        //                                    oCommand.CommandType = CommandType.Text;
        //                                    oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
        //                                    oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));

        //                                    string sSQL = "INSERT INTO [ATTRIBUTE_SHOPEE] ([CATEGORY_CODE], [CATEGORY_NAME],";
        //                                    string sSQLValue = ") VALUES (@CATEGORY_CODE, @CATEGORY_NAME,";
        //                                    string a = "";
        //                                    int i = 0;
        //                                    foreach (var attribs in result.attributes)
        //                                    {
        //                                        a = Convert.ToString(i + 1);
        //                                        sSQL += "[ACODE_" + a + "],[ATYPE_" + a + "],[ANAME_" + a + "],[AOPTIONS_" + a + "],[AMANDATORY_" + a + "],";
        //                                        sSQLValue += "@ACODE_" + a + ",@ATYPE_" + a + ",@ANAME_" + a + ",@AOPTIONS_" + a + ",@AMANDATORY_" + a + ",";
        //                                        oCommand.Parameters.Add(new SqlParameter("@ACODE_" + a, SqlDbType.NVarChar, 50));
        //                                        oCommand.Parameters.Add(new SqlParameter("@ATYPE_" + a, SqlDbType.NVarChar, 50));
        //                                        oCommand.Parameters.Add(new SqlParameter("@ANAME_" + a, SqlDbType.NVarChar, 250));
        //                                        oCommand.Parameters.Add(new SqlParameter("@AOPTIONS_" + a, SqlDbType.NVarChar, 1));
        //                                        oCommand.Parameters.Add(new SqlParameter("@AMANDATORY_" + a, SqlDbType.NVarChar, 1));

        //                                        a = Convert.ToString(i * 5 + 2);
        //                                        oCommand.Parameters[(i * 5) + 2].Value = "";
        //                                        oCommand.Parameters[(i * 5) + 3].Value = "";
        //                                        oCommand.Parameters[(i * 5) + 4].Value = "";
        //                                        oCommand.Parameters[(i * 5) + 5].Value = "";
        //                                        oCommand.Parameters[(i * 5) + 6].Value = "";

        //                                        oCommand.Parameters[(i * 5) + 2].Value = attribs.attribute_id;
        //                                        oCommand.Parameters[(i * 5) + 3].Value = attribs.options.Count() > 0 ? "PREDEFINED_ATTRIBUTE" : "DESCRIPTIVE_ATTRIBUTE";
        //                                        oCommand.Parameters[(i * 5) + 4].Value = attribs.attribute_name;
        //                                        oCommand.Parameters[(i * 5) + 5].Value = attribs.options.Count() > 0 ? "1" : "0";
        //                                        oCommand.Parameters[(i * 5) + 6].Value = attribs.is_mandatory ? "1" : "0";

        //                                        if (attribs.options.Count() > 0)
        //                                        {
        //                                            var AttributeOptInDb = MoDbContext.AttributeOptShopee.AsNoTracking().ToList();
        //                                            foreach (var option in attribs.options)
        //                                            {
        //                                                var cariOpt = AttributeOptInDb.Where(p => p.ACODE == Convert.ToString(attribs.attribute_id) && p.OPTION_VALUE == option);
        //                                                if (cariOpt.Count() == 0)
        //                                                {
        //                                                    using (SqlCommand oCommand2 = oConnection.CreateCommand())
        //                                                    {
        //                                                        oCommand2.CommandType = CommandType.Text;
        //                                                        oCommand2.CommandText = "INSERT INTO ATTRIBUTE_OPT_SHOPEE ([ACODE], [OPTION_VALUE]) VALUES (@ACODE, @OPTION_VALUE)";
        //                                                        oCommand2.Parameters.Add(new SqlParameter("@ACODE", SqlDbType.NVarChar, 50));
        //                                                        oCommand2.Parameters.Add(new SqlParameter("@OPTION_VALUE", SqlDbType.NVarChar, 250));
        //                                                        oCommand2.Parameters[0].Value = attribs.attribute_id;
        //                                                        oCommand2.Parameters[1].Value = option;
        //                                                        oCommand2.ExecuteNonQuery();
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                        i = i + 1;
        //                                    }
        //                                    sSQL = sSQL.Substring(0, sSQL.Length - 1) + sSQLValue.Substring(0, sSQLValue.Length - 1) + ")";
        //                                    oCommand.CommandText = sSQL;
        //                                    oCommand.Parameters[0].Value = category.CATEGORY_CODE;
        //                                    oCommand.Parameters[1].Value = "";
        //                                    oCommand.ExecuteNonQuery();
        //                                }
        //                            }
        //                        }
        //                    }
        //                    catch (Exception ex2)
        //                    {

        //                    }
        //                }
        //            }
        //            return ret;
        //        }

        protected void RecursiveInsertCategory(SqlCommand oCommand, CategoryChild[] item_children, string parent, string master_category_code, TokopediaAPIData data)
        {
            foreach (var child in item_children)
            {
                oCommand.Parameters[0].Value = child.id;
                oCommand.Parameters[1].Value = child.name;
                oCommand.Parameters[2].Value = parent;
                oCommand.Parameters[3].Value = child.child == null ? "1" : child.child.Count() == 0 ? "1" : "0";
                oCommand.Parameters[4].Value = master_category_code;

                if (oCommand.ExecuteNonQuery() == 1)
                {
                    if ((child.child == null ? 0 : child.child.Count()) > 0)
                    {
                        RecursiveInsertCategory(oCommand, child.child, child.id, master_category_code, data);
                    }
                }
            }
        }
        public enum StatusOrder
        {
            Cancel = 1,
            Paid = 2,
            PackagingINP = 3,
            ShippingINP = 4,
            Completed = 5
        }
        protected enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4
        }

        private string CreateToken(string urlBlili, string secretMTA)
        {
            secretMTA = secretMTA ?? "";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secretMTA);
            byte[] messageBytes = encoding.GetBytes(urlBlili);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
                //return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();

            }
        }
        protected void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, TokopediaAPIData iden, API_LOG_MARKETPLACE data)
        {
            switch (action)
            {
                case api_status.Pending:
                    {
                        var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.merchant_code).FirstOrDefault();
                        var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                        {
                            CUST = arf01 != null ? arf01.CUST : iden.merchant_code,
                            CUST_ATTRIBUTE_1 = iden.merchant_code,
                            CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                            CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                            CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                            CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                            MARKETPLACE = "Tokopedia",
                            REQUEST_ACTION = data.REQUEST_ACTION,
                            REQUEST_ATTRIBUTE_1 = data.REQUEST_ATTRIBUTE_1 != null ? data.REQUEST_ATTRIBUTE_1 : "",
                            REQUEST_ATTRIBUTE_2 = data.REQUEST_ATTRIBUTE_2 != null ? data.REQUEST_ATTRIBUTE_2 : "",
                            REQUEST_ATTRIBUTE_3 = data.REQUEST_ATTRIBUTE_3 != null ? data.REQUEST_ATTRIBUTE_3 : "",
                            REQUEST_ATTRIBUTE_4 = data.REQUEST_ATTRIBUTE_4 != null ? data.REQUEST_ATTRIBUTE_4 : "",
                            REQUEST_ATTRIBUTE_5 = data.REQUEST_ATTRIBUTE_5 != null ? data.REQUEST_ATTRIBUTE_5 : "",
                            REQUEST_DATETIME = data.REQUEST_DATETIME,
                            REQUEST_ID = data.REQUEST_ID,
                            REQUEST_STATUS = data.REQUEST_STATUS,
                            REQUEST_EXCEPTION = data.REQUEST_EXCEPTION != null ? data.REQUEST_EXCEPTION : "",
                            REQUEST_RESULT = data.REQUEST_RESULT != null ? data.REQUEST_RESULT : "",
                        };
                        ErasoftDbContext.API_LOG_MARKETPLACE.Add(apiLog);
                        ErasoftDbContext.SaveChanges();
                    }
                    break;
                case api_status.Success:
                    {
                        var apiLogInDb = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == data.REQUEST_ID).SingleOrDefault();
                        if (apiLogInDb != null)
                        {
                            apiLogInDb.REQUEST_STATUS = "Success";
                            apiLogInDb.REQUEST_RESULT = data.REQUEST_RESULT;
                            apiLogInDb.REQUEST_EXCEPTION = data.REQUEST_EXCEPTION;
                            ErasoftDbContext.SaveChanges();
                        }
                    }
                    break;
                case api_status.Failed:
                    {
                        var apiLogInDb = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == data.REQUEST_ID).SingleOrDefault();
                        if (apiLogInDb != null)
                        {
                            apiLogInDb.REQUEST_STATUS = "Failed";
                            apiLogInDb.REQUEST_RESULT = data.REQUEST_RESULT;
                            apiLogInDb.REQUEST_EXCEPTION = data.REQUEST_EXCEPTION;
                            ErasoftDbContext.SaveChanges();
                        }
                    }
                    break;
                case api_status.Exception:
                    {
                        var apiLogInDb = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == data.REQUEST_ID).SingleOrDefault();
                        if (apiLogInDb != null)
                        {
                            apiLogInDb.REQUEST_STATUS = "Failed";
                            apiLogInDb.REQUEST_RESULT = "Exception";
                            apiLogInDb.REQUEST_EXCEPTION = data.REQUEST_EXCEPTION;
                            ErasoftDbContext.SaveChanges();
                        }
                    }
                    break;
            }
        }
        public static long CurrentTimeMillis()
        {
            //        return (long)DateTime.Now.ToUniversalTime().Subtract(
            //new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            //).TotalMilliseconds;
            return (long)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        public class TokopediaAPIData
        {
            public string merchant_code { get; set; }
            public string API_client_username { get; set; }
            public string API_client_password { get; set; }
            public string API_secret_key { get; set; }
            public string mta_username_email_merchant { get; set; }
            public string mta_password_password_merchant { get; set; }
            public string token { get; set; }
        }
        public class TokopediaToken
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
            public string error { get; set; }
            public string error_description { get; set; }
        }
        protected string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            //byte[] encodedBytes = Encoding.UTF8.GetBytes(input);
            //Encoding.Convert(Encoding.UTF8, Encoding.Unicode, encodedBytes);
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            //byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }



        public class TokopediaOrders
        {
            public Jsonapi jsonapi { get; set; }
            public object meta { get; set; }
            public TokopediaOrder[] data { get; set; }
            public Links links { get; set; }
        }

        public class Jsonapi
        {
            public string version { get; set; }
        }

        public class Links
        {
            public string self { get; set; }
            public string related { get; set; }
            public string first { get; set; }
            public string last { get; set; }
            public string prev { get; set; }
            public string next { get; set; }
        }

        public class TokopediaOrder
        {
            public string fs_id { get; set; }
            public int order_id { get; set; }
            public bool accept_partial { get; set; }
            public string invoice_ref_num { get; set; }
            public Product[] products { get; set; }
            public Products_Fulfilled[] products_fulfilled { get; set; }
            public string device_type { get; set; }
            public Buyer buyer { get; set; }
            public int shop_id { get; set; }
            public int payment_id { get; set; }
            public Recipient recipient { get; set; }
            public Logistics logistics { get; set; }
            public Amt amt { get; set; }
            public Dropshipper_Info dropshipper_info { get; set; }
            public Voucher_Info voucher_info { get; set; }
            public int order_status { get; set; }
            public int create_time { get; set; }
            public Custom_Fields custom_fields { get; set; }
        }

        public class Buyer
        {
            public int id { get; set; }
            public string name { get; set; }
            public string phone { get; set; }
            public string email { get; set; }
        }

        public class Recipient
        {
            public string name { get; set; }
            public Address address { get; set; }
            public string phone { get; set; }
        }

        public class Address
        {
            public string address_full { get; set; }
            public string district { get; set; }
            public string city { get; set; }
            public string province { get; set; }
            public string country { get; set; }
            public string postal_code { get; set; }
            public int district_id { get; set; }
            public int city_id { get; set; }
            public int province_id { get; set; }
            public string geo { get; set; }
        }

        public class Logistics
        {
            public int shipping_id { get; set; }
            public string shipping_agency { get; set; }
            public string service_type { get; set; }
        }

        public class Amt
        {
            public int ttl_product_price { get; set; }
            public int shipping_cost { get; set; }
            public int insurance_cost { get; set; }
            public int ttl_amount { get; set; }
            public int voucher_amount { get; set; }
            public int toppoints_amount { get; set; }
        }

        public class Dropshipper_Info
        {
            public string name { get; set; }
            public string phone { get; set; }
        }

        public class Voucher_Info
        {
            public long voucher_type { get; set; }
            public string voucher_code { get; set; }
        }

        public class Custom_Fields
        {
            public string awb { get; set; }
        }

        public class Product
        {
            public int id { get; set; }
            public string name { get; set; }
            public int quantity { get; set; }
            public string notes { get; set; }
            public float weight { get; set; }
            public float total_weight { get; set; }
            public int price { get; set; }
            public int total_price { get; set; }
            public string currency { get; set; }
            public string sku { get; set; }
        }

        public class Products_Fulfilled
        {
            public int product_id { get; set; }
            public int quantity_deliver { get; set; }
            public int quantity_reject { get; set; }
        }


        public class AckOrder
        {
            public List<AckOrder_Product> products { get; set; }
        }

        public class AckOrder_Product
        {
            public string product_id { get; set; }
            public double quantity_deliver { get; set; }
            public double quantity_reject { get; set; }
        }


        public class categoryAPIResult
        {
            public categoryAPIResultHeader header { get; set; }
            public categoryAPIResultData data { get; set; }
        }

        public class categoryAPIResultHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            public int error_code { get; set; }
        }

        public class categoryAPIResultData
        {
            public Category[] categories { get; set; }
        }

        public class Category
        {
            public string name { get; set; }
            public string id { get; set; }
            public CategoryChild[] child { get; set; }
        }

        public class CategoryChild
        {
            public string name { get; set; }
            public string id { get; set; }
            public CategoryChild[] child { get; set; }
        }

        public class ActiveProductListResult
        {
            public ActiveProductListResultHeader header { get; set; }
            public ActiveProductListResultData data { get; set; }
        }

        public class ActiveProductListResultHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            public int error_code { get; set; }
        }

        public class ActiveProductListResultData
        {
            public int total_data { get; set; }
            public ActiveProductListResultShop shop { get; set; }
            public ActiveProductListResultProduct[] products { get; set; }
        }

        public class ActiveProductListResultShop
        {
            public int id { get; set; }
            public string name { get; set; }
            public string uri { get; set; }
            public string location { get; set; }
        }

        public class ActiveProductListResultProduct
        {
            public int id { get; set; }
            public string name { get; set; }
            public int[] childs { get; set; }
            public string url { get; set; }
            public string image_url { get; set; }
            public string image_url_700 { get; set; }
            public string price { get; set; }
            public ActiveProductListResultShop1 shop { get; set; }
            public object[] wholesale_price { get; set; }
            public int courier_count { get; set; }
            public int condition { get; set; }
            public int category_id { get; set; }
            public string category_name { get; set; }
            public string category_breadcrumb { get; set; }
            public int department_id { get; set; }
            public object[] labels { get; set; }
            public ActiveProductListResultBadge[] badges { get; set; }
            public int is_featured { get; set; }
            public int rating { get; set; }
            public int count_review { get; set; }
            public string original_price { get; set; }
            public string discount_expired { get; set; }
            public int discount_percentage { get; set; }
            public string sku { get; set; }
            public int stock { get; set; }
        }

        public class ActiveProductListResultShop1
        {
            public int id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public bool is_gold { get; set; }
            public string location { get; set; }
            public string city { get; set; }
            public string reputation { get; set; }
            public string clover { get; set; }
        }

        public class ActiveProductListResultBadge
        {
            public string title { get; set; }
            public string image_url { get; set; }
        }

        public class ActiveProductVariantResult
        {
            public ActiveProductVariantResultHeader header { get; set; }
            public ActiveProductVariantResultData data { get; set; }
        }

        public class ActiveProductVariantResultHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            public int error_code { get; set; }
        }

        public class ActiveProductVariantResultData
        {
            public int parent_id { get; set; }
            public int default_child { get; set; }
            public string sizechart { get; set; }
            public ActiveProductVariantResultDataVariant[] variant { get; set; }
            public ActiveProductVariantResultDataChild[] children { get; set; }
        }

        public class ActiveProductVariantResultDataVariant
        {
            public string name { get; set; }
            public string identifier { get; set; }
            public string unit_name { get; set; }
            public int position { get; set; }
            public ActiveProductVariantResultDataVariantOption[] option { get; set; }
        }

        public class ActiveProductVariantResultDataVariantOption
        {
            public int id { get; set; }
            public string value { get; set; }
            public string hex { get; set; }
        }

        public class ActiveProductVariantResultDataChild
        {
            public string name { get; set; }
            public string url { get; set; }
            public int product_id { get; set; }
            public int price { get; set; }
            public string price_fmt { get; set; }
            public int stock { get; set; }
            public string sku { get; set; }
            public int[] option_ids { get; set; }
            public bool enabled { get; set; }
            public bool is_buyable { get; set; }
            public bool is_wishlist { get; set; }
            public ActiveProductVariantResultDataChildPicture picture { get; set; }
            public ActiveProductVariantResultDataChildCampaign campaign { get; set; }
            public bool always_available { get; set; }
            public string stock_wording { get; set; }
            public string other_variant_stock { get; set; }
            public bool is_limited_stock { get; set; }
        }

        public class ActiveProductVariantResultDataChildPicture
        {
            public string original { get; set; }
            public string thumbnail { get; set; }
        }

        public class ActiveProductVariantResultDataChildCampaign
        {
            public bool is_active { get; set; }
            public int discounted_percentage { get; set; }
            public int discounted_price { get; set; }
            public string discounted_price_fmt { get; set; }
            public int campaign_type { get; set; }
            public string campaign_type_name { get; set; }
            public string start_date { get; set; }
            public string end_date { get; set; }
        }


        public class ItemListResult
        {
            public ItemListResultData[] data { get; set; }
            public string status { get; set; }
            public object[] error_message { get; set; }
        }

        public class ItemListResultData
        {
            public int product_id { get; set; }
            public string name { get; set; }
            public int shop_id { get; set; }
            public string shop_name { get; set; }
            public int category_id { get; set; }
            public string desc { get; set; }
            public int stock { get; set; }
            public int price { get; set; }
            public string status { get; set; }
        }

    }
}