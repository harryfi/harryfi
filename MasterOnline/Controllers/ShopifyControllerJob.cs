using System;
using System.CodeDom;
using System.CodeDom.Compiler;
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
using Hangfire;
using Hangfire.SqlServer;

namespace MasterOnline.Controllers
{
    public class ShopifyControllerJob : Controller
    {
        //AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);//string auth = Base64Encode();

        //protected int MOPartnerID = 841371;
        //protected string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
        private MoDbContext MoDbContext { get; set; }
        private ErasoftContext ErasoftDbContext { get; set; }
        private DatabaseSQL EDB;
        private string username;
        private static string ToLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }

        public ShopifyControllerJob()
        {
            //MoDbContext = new MoDbContext();
            //var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            //if (sessionData?.Account != null)
            //{
            //    if (sessionData.Account.UserId == "admin_manage")
            //        ErasoftDbContext = new ErasoftContext();
            //    else
            //        ErasoftDbContext = new ErasoftContext(sessionData.Account.DatabasePathErasoft);

            //    EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);
            //    username = sessionData.Account.Username;
            //}
            //else
            //{
            //    if (sessionData?.User != null)
            //    {
            //        var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
            //        ErasoftDbContext = new ErasoftContext(accFromUser.DatabasePathErasoft);
            //        EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
            //        username = accFromUser.Username;
            //    }
            //}
        }

        protected void SetupContext(ShopifyAPIData data)
        {
            //string ret = "";
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(data.DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, data.DatabasePathErasoft);
            username = data.account_store;
            //var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.RecNum == data.idmarket).SingleOrDefault();
            //if (arf01inDB != null)
            //{
            //    ret = arf01inDB.TOKEN;
            //}
            //return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> Shopify_GetOrderByStatusUnpaid(ShopifyAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar)
        {
            string ret = "";
            SetupContext(iden);

            var daysFrom = -1;
            var daysTo = 1;

            while (daysFrom > -13)
            {
                await Shopify_GetOrderByStatusUnpaid_List3Days(iden, stat, CUST, NAMA_CUST, 1, 0, 0, daysFrom, daysTo);
                daysFrom -= 3;
                daysTo -= 3;
            }


            // tunning untuk tidak duplicate
            //var queryStatus = "";
            //if (stat == StatusOrder.UNPAID)
            //{
            //    //queryStatus = "\"}\"" + "," + "\"23\"" + "," + "\"";
            //    queryStatus = "\\\"}\"" + "," + "\"23\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","23","\"000003\""
            //}
            //else if (stat == StatusOrder.PAID)
            //{
            //    //queryStatus = "\"}\"" + "," + "\"2\"" + "," + "\"";
            //    queryStatus = "\\\"}\"" + "," + "\"2\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","2","\"000003\""
            //}
            //var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust + "%' and arguments like '%" + queryStatus + "%' and invocationdata like '%Shopify_GetOrderByStatusUnpaid%' and statename like '%Enque%' and invocationdata not like '%resi%' and invocationdata not like '%Shopify_GetOrderByStatusPaid%' and invocationdata not like '%Shopify_GetOrderByStatusCompleted%' and invocationdata not like '%Shopify_GetOrderByStatusCancel%' ");
            // end tunning untuk tidak duplicate

            return ret;
        }

        public async Task<string> Shopify_GetOrderByStatusUnpaid_List3Days(ShopifyAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar, int daysFrom, int daysTo)
        {
            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).AddHours(7).ToString("yyyy-MM-dd") + " 00:00:00";
            var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).AddHours(7).ToString("yyyy-MM-dd") + " 23:59:59";

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/orders.json?status=any&created_at_min=" + dateFrom + "&created_at_max=" + dateTo;
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.Headers.Add("X-Shopify-Access-Token", iden.API_password);
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseServer = "";

            //try
            //{
            using (WebResponse response = myReq.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseServer = reader.ReadToEnd();
                }
            }
            //}
            //catch (Exception ex)
            //{

            //}

            if (responseServer != null)
            {
                //try
                //{
                var listOrder = JsonConvert.DeserializeObject(responseServer, typeof(ResultOrderShopify)) as ResultOrderShopify;
                if (listOrder != null)
                    if (listOrder.orders != null)
                    {
                        //string[] statusAwaiting = { "1", "3", "10", "11", "13", "14", "16", "17", "18", "19", "20", "21", "23", "25" };

                        //string[] ordersn_list = listOrder.data.Select(p => p.id_order).ToArray();
                        //var dariTgl = DateTimeOffset.UtcNow.AddDays(-10).DateTime;
                        if (listOrder.orders.Count() > 0)
                        {
                            //jmlhNewOrder = 0;

                            var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();

                            #region UNPAID
                            if (stat == StatusOrder.UNPAID)
                            {
                                var orderFilter = listOrder.orders.Where(p => p.financial_status == "pending" && p.cancel_reason == null && p.cancelled_at == null || p.financial_status == "unpaid" && p.cancel_reason == null && p.cancelled_at == null).ToList();
                                if (orderFilter.Count() > 0)
                                {
                                    foreach (var order in orderFilter)
                                    {
                                        try
                                        {
                                            if (!OrderNoInDb.Contains(order.id.ToString() + ";" + Convert.ToString(order.order_number)))
                                            {
                                                jmlhNewOrder++;
                                                var connIdARF01C = Guid.NewGuid().ToString();
                                                TEMP_SHOPIFY_ORDERS batchinsert = new TEMP_SHOPIFY_ORDERS();
                                                List<TEMP_SHOPIFY_ORDERS_ITEM> batchinsertItem = new List<TEMP_SHOPIFY_ORDERS_ITEM>();
                                                string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                                insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                                insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                                                var kabKot = "3174";
                                                var prov = "31";


                                                string fullname = order.billing_address.first_name.ToString() + " " + order.billing_address.last_name.ToString();
                                                string nama = fullname.Length > 30 ? fullname.Substring(0, 30) : order.billing_address.last_name.ToString();

                                                insertPembeli += string.Format("('{0}','{1}','{2}','{3}',0,0,'0','01',1, 'IDR', '01', '{4}', 0, 0, 0, 0, '1', 0, 0,'FP', '{5}', '{6}', '{7}', '', '{8}', '{9}', '', '','{10}'),",
                                                    ((nama ?? "").Replace("'", "`")),
                                                    ((order.billing_address.address1 ?? "").Replace("'", "`") + " " + (order.billing_address.address2 ?? "").Replace("'", "`")),
                                                    ((order.billing_address.phone ?? "").Replace("'", "`")),
                                                    (NAMA_CUST.Replace(',', '.')),
                                                    ((order.billing_address.address1 ?? "").Replace("'", "`") + " " + (order.billing_address.address2 ?? "").Replace("'", "`")),
                                                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                                    (username),
                                                    ((order.billing_address.zip ?? "").Replace("'", "`")),
                                                    kabKot,
                                                    prov,
                                                    connIdARF01C
                                                    );
                                                insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
                                                EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);


                                                ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_SHOPIFY_ORDERS");
                                                ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_SHOPIFY_ORDERS_ITEM");
                                                batchinsertItem = new List<TEMP_SHOPIFY_ORDERS_ITEM>();

                                                //2020-04-08T05:12:41
                                                var dateOrder = Convert.ToDateTime(order.created_at).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                                                var datePay = Convert.ToDateTime(order.processed_at).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                                                //if (order.processed_at == "0000-00-00 00:00:00")
                                                //{
                                                //    datePay = dateOrder;
                                                //}
                                                //else
                                                //{
                                                //    datePay = Convert.ToDateTime(order.invoice_date).ToString("yyyy-MM-dd HH:mm:ss");
                                                //}

                                                var shippingLine = "";
                                                var trackingCompany = "";
                                                var trackingNumber = "";

                                                if (order.shipping_lines.Count() > 0)
                                                {
                                                    shippingLine = Convert.ToString(order.shipping_lines[0].code);
                                                }

                                                if (order.fulfillments.Count() > 0)
                                                {
                                                    trackingCompany = Convert.ToString(order.fulfillments[0].tracking_company);
                                                    trackingNumber = Convert.ToString(order.fulfillments[0].tracking_number);
                                                }


                                                TEMP_SHOPIFY_ORDERS newOrder = new TEMP_SHOPIFY_ORDERS()
                                                {
                                                    actual_shipping_cost = Convert.ToString(double.Parse(order.total_shipping_price_set.shop_money.amount)),
                                                    buyer_username = nama,
                                                    cod = false,
                                                    country = order.billing_address.country,
                                                    create_time = Convert.ToDateTime(dateOrder),
                                                    currency = order.currency,
                                                    days_to_ship = 0,
                                                    dropshipper = "",
                                                    escrow_amount = "",
                                                    estimated_shipping_fee = Convert.ToString(double.Parse(order.total_shipping_price_set.shop_money.amount)),
                                                    goods_to_declare = false,
                                                    message_to_seller = order.note ?? "",
                                                    note = order.note ?? "",
                                                    note_update_time = Convert.ToDateTime(dateOrder),
                                                    ordersn = Convert.ToString(order.id + ";" + order.order_number),
                                                    //ordersn = Convert.ToString(order.order_number),
                                                    //order_status = order.current_state_name,
                                                    order_status = "UNPAID",
                                                    payment_method = order.gateway,
                                                    //change by nurul 5/12/2019, local time 
                                                    //pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime,
                                                    pay_time = Convert.ToDateTime(datePay),
                                                    //end change by nurul 5/12/2019, local time 
                                                    Recipient_Address_country = order.billing_address.country_code ?? "ID",
                                                    Recipient_Address_state = order.billing_address.province_code ?? "",
                                                    Recipient_Address_city = order.billing_address.city ?? "",
                                                    Recipient_Address_town = "",
                                                    Recipient_Address_district = "",
                                                    Recipient_Address_full_address = (order.billing_address.address1 ?? "").Replace("'", "`") + " " + (order.billing_address.address2 ?? "").Replace("'", "`"),
                                                    Recipient_Address_name = nama,
                                                    Recipient_Address_phone = order.billing_address.phone ?? "",
                                                    Recipient_Address_zipcode = order.billing_address.zip ?? "",
                                                    service_code = shippingLine,
                                                    shipping_carrier = trackingCompany,
                                                    total_amount = Convert.ToString(double.Parse(order.total_price)),
                                                    tracking_no = trackingNumber,
                                                    update_time = Convert.ToDateTime(dateOrder),
                                                    CONN_ID = connID,
                                                    CUST = CUST,
                                                    NAMA_CUST = NAMA_CUST
                                                };
                                                foreach (var item in order.line_items)
                                                {
                                                    //var product_id = "";
                                                    //var name_brg = "";
                                                    //var name_brg_variasi = "";
                                                    //if (item.variant_id == "0")
                                                    //{
                                                    //    //var kodeBrg = ErasoftDbContext.STF02.SingleOrDefault(p => p.NAMA.Contains(item.product_name) && p.PART == "");
                                                    //    //product_id = kodeBrg.BRG;
                                                    //    //product_id = item.product_id;
                                                    //    //name_brg = item.product_name;
                                                    //}
                                                    //else
                                                    //{
                                                    //    //product_id = item.product_attribute_id;
                                                    //    name_brg_variasi = item.product_name;
                                                    //}

                                                    TEMP_SHOPIFY_ORDERS_ITEM newOrderItem = new TEMP_SHOPIFY_ORDERS_ITEM()
                                                    {
                                                        ordersn = Convert.ToString(order.id + ";" + order.order_number),
                                                        is_wholesale = false,
                                                        item_id = Convert.ToString(item.product_id),
                                                        item_name = item.title,
                                                        item_sku = item.sku,
                                                        variation_discounted_price = Convert.ToString(double.Parse(item.price)),
                                                        variation_id = Convert.ToString(item.variant_id),
                                                        variation_name = item.variant_title,
                                                        variation_original_price = Convert.ToString(double.Parse(item.price)),
                                                        variation_quantity_purchased = Convert.ToInt32(item.quantity),
                                                        variation_sku = item.sku,
                                                        weight = item.grams,
                                                        pay_time = Convert.ToDateTime(datePay),
                                                        CONN_ID = connID,
                                                        CUST = CUST,
                                                        NAMA_CUST = NAMA_CUST
                                                    };
                                                    //if (!string.IsNullOrEmpty(item.promotion_type))
                                                    //{
                                                    //    if (item.promotion_type == "bundle_deal")
                                                    //    {
                                                    //        var promoPrice = await GetEscrowDetail(iden, order.ordersn, item.item_id, item.variation_id);
                                                    //        newOrderItem.variation_discounted_price = promoPrice;
                                                    //    }
                                                    //}
                                                    batchinsertItem.Add(newOrderItem);
                                                }
                                                batchinsert = (newOrder);

                                                ErasoftDbContext.TEMP_SHOPIFY_ORDERS.Add(batchinsert);
                                                ErasoftDbContext.TEMP_SHOPIFY_ORDERS_ITEM.AddRange(batchinsertItem);
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
                                                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connID;
                                                    CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                                                    CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                                                    CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                                                    CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                                                    CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                                                    CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                                                    CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                                                    CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                                                    CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                                                    CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 1;
                                                    CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                                                    EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
                                                }
                                            }
                                        }
                                        catch (Exception ex3)
                                        {

                                        }
                                    }
                                }

                                if (jmlhNewOrder > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari Shopify.");
                                    new StokControllerJob().updateStockMarketPlace(connID, iden.DatabasePathErasoft, iden.username);
                                }
                            }
                            #endregion
                        }
                    }
            }

            return ret;
        }


        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> Shopify_GetOrderByStatusPaid(ShopifyAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar)
        {
            string ret = "";
            SetupContext(iden);

            var daysFrom = -1;
            var daysTo = 1;

            while (daysFrom > -13)
            {
                await Shopify_GetOrderByStatusPaid_List3Days(iden, stat, CUST, NAMA_CUST, 1, 0, daysFrom, daysTo);
                daysFrom -= 3;
                daysTo -= 3;
            }


            // tunning untuk tidak duplicate
            //var queryStatus = "";
            //if (stat == StatusOrder.UNPAID)
            //{
            //    //queryStatus = "\"}\"" + "," + "\"23\"" + "," + "\"";
            //    queryStatus = "\\\"}\"" + "," + "\"23\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","23","\"000003\""
            //}
            //else if (stat == StatusOrder.PAID)
            //{
            //    //queryStatus = "\"}\"" + "," + "\"2\"" + "," + "\"";
            //    queryStatus = "\\\"}\"" + "," + "\"2\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","2","\"000003\""
            //}
            //var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust + "%' and arguments like '%" + queryStatus + "%' and invocationdata like '%Shopify_GetOrderByStatusUnpaid%' and statename like '%Enque%' and invocationdata not like '%resi%' and invocationdata not like '%Shopify_GetOrderByStatusPaid%' and invocationdata not like '%Shopify_GetOrderByStatusCompleted%' and invocationdata not like '%Shopify_GetOrderByStatusCancel%' ");
            // end tunning untuk tidak duplicate

            return ret;
        }

        public async Task<string> Shopify_GetOrderByStatusPaid_List3Days(ShopifyAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhOrderPaid, int daysFrom, int daysTo)
        {
            string ret = "";

            SetupContext(iden);

            var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).AddHours(7).ToString("yyyy-MM-dd") + " 00:00:00";
            var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).AddHours(7).ToString("yyyy-MM-dd") + " 23:59:59";

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/orders.json?status=any&created_at_min=" + dateFrom + "&created_at_max=" + dateTo;
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.Headers.Add("X-Shopify-Access-Token", iden.API_password);
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseServer = "";

            //try
            //{
            using (WebResponse response = myReq.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseServer = reader.ReadToEnd();
                }
            }
            //}
            //catch (Exception ex)
            //{

            //}


            if (responseServer != null)
            {
                //try
                //{
                var listOrder = JsonConvert.DeserializeObject(responseServer, typeof(ResultOrderShopify)) as ResultOrderShopify;
                if (listOrder != null)
                    if (listOrder.orders != null)
                    {
                        if (listOrder.orders.Count() > 0)
                        {
                            var orderFilterPaid = listOrder.orders.Where(p => p.financial_status == "paid" && p.cancel_reason == null && p.cancelled_at == null).ToList();
                            var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();
                            string ordersn = "";
                            jmlhOrderPaid = 0;
                            if (orderFilterPaid != null && orderFilterPaid.Count() > 0)
                            {
                                foreach (var item in orderFilterPaid)
                                {
                                    if (OrderNoInDb.Contains(item.id.ToString() + ";" + Convert.ToString(item.order_number)))
                                    {
                                        ordersn = ordersn + "'" + item.id.ToString() + ";" + Convert.ToString(item.order_number) + "',";
                                    }
                                }
                            }
                            if (orderFilterPaid.Count() > 0 && !string.IsNullOrEmpty(ordersn))
                            {
                                ordersn = ordersn.Substring(0, ordersn.Length - 1);
                                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '0'");
                                jmlhOrderPaid = jmlhOrderPaid + rowAffected;
                                if (jmlhOrderPaid > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderPaid) + " Pesanan terbayar dari Shopify.");
                                }
                            }
                        }
                    }
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> Shopify_GetOrderByStatusCompleted(ShopifyAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            SetupContext(iden);

            var daysFrom = -1;
            var daysTo = 1;

            while (daysFrom > -13)
            {
                await Shopify_GetOrderByStatusCompleted_List3Days(iden, stat, CUST, NAMA_CUST, 1, 0, daysFrom, daysTo);
                daysFrom -= 3;
                daysTo -= 3;
            }


            // tunning untuk tidak duplicate
            //var queryStatus = "";
            //if (stat == StatusOrder.UNPAID)
            //{
            //    //queryStatus = "\"}\"" + "," + "\"23\"" + "," + "\"";
            //    queryStatus = "\\\"}\"" + "," + "\"23\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","23","\"000003\""
            //}
            //else if (stat == StatusOrder.PAID)
            //{
            //    //queryStatus = "\"}\"" + "," + "\"2\"" + "," + "\"";
            //    queryStatus = "\\\"}\"" + "," + "\"2\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","2","\"000003\""
            //}
            //var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust + "%' and arguments like '%" + queryStatus + "%' and invocationdata like '%Shopify_GetOrderByStatusUnpaid%' and statename like '%Enque%' and invocationdata not like '%resi%' and invocationdata not like '%Shopify_GetOrderByStatusPaid%' and invocationdata not like '%Shopify_GetOrderByStatusCompleted%' and invocationdata not like '%Shopify_GetOrderByStatusCancel%' ");
            // end tunning untuk tidak duplicate

            return ret;
        }

        public async Task<string> Shopify_GetOrderByStatusCompleted_List3Days(ShopifyAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhOrderCompleted, int daysFrom, int daysTo)
        {
            string ret = "";

            SetupContext(iden);

            var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).AddHours(7).ToString("yyyy-MM-dd") + " 00:00:00";
            var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).AddHours(7).ToString("yyyy-MM-dd") + " 23:59:59";

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/orders.json?status=any&created_at_min=" + dateFrom + "&created_at_max=" + dateTo;
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.Headers.Add("X-Shopify-Access-Token", iden.API_password);
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseServer = "";

            //try
            //{
            using (WebResponse response = myReq.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseServer = reader.ReadToEnd();
                }
            }
            //}
            //catch (Exception ex)
            //{

            //}


            if (responseServer != null)
            {
                //try
                //{
                var listOrder = JsonConvert.DeserializeObject(responseServer, typeof(ResultOrderShopify)) as ResultOrderShopify;
                if (listOrder.orders != null)
                {
                    if (listOrder.orders.Count() > 0)
                    {
                        var orderFilterCompleted = listOrder.orders.Where(p => p.financial_status == "paid" && p.fulfillment_status == "fulfilled" && p.cancel_reason == null && p.cancelled_at == null).ToList();
                        var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();
                        string ordersn = "";
                        jmlhOrderCompleted = 0;
                        if (orderFilterCompleted != null && orderFilterCompleted.Count() > 0)
                        {
                            foreach (var item in orderFilterCompleted)
                            {
                                if (OrderNoInDb.Contains(item.id.ToString() + ";" + Convert.ToString(item.order_number)))
                                {
                                    ordersn = ordersn + "'" + item.id.ToString() + ";" + Convert.ToString(item.order_number) + "',";
                                }
                            }
                        }
                        if (orderFilterCompleted.Count() > 0 && !string.IsNullOrEmpty(ordersn))
                        {
                            ordersn = ordersn.Substring(0, ordersn.Length - 1);
                            var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '03'");
                            jmlhOrderCompleted = jmlhOrderCompleted + rowAffected;
                            if (jmlhOrderCompleted > 0)
                            {
                                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderCompleted) + " Pesanan dari Shopify sudah selesai.");
                            }
                        }
                    }
                }
            }

            return ret;
        }


        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> Shopify_GetOrderByStatusCancelled(ShopifyAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhOrderCancel)
        {
            string ret = "";
            SetupContext(iden);

            var daysFrom = -1;
            var daysTo = 1;

            while (daysFrom > -13)
            {
                await Shopify_GetOrderByStatusCancelledList3Days(iden, stat, CUST, NAMA_CUST, 1, 0, daysFrom, daysTo);
                daysFrom -= 3;
                daysTo -= 3;
            }

            //// tunning untuk tidak duplicate
            //var queryStatus = "\\\"}\"" + "," + "\"6\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","6","\"000003\""
            //var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + queryStatus + "%' and arguments like '%" + iden.no_cust + "%' and invocationdata like '%E2Cart_GetOrderByStatusCancelled%' and statename like '%Enque%' and invocationdata not like '%resi%'");
            //// end tunning untuk tidak duplicate

            return ret;
        }

        public async Task<string> Shopify_GetOrderByStatusCancelledList3Days(ShopifyAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhOrderCancel, int daysFrom, int daysTo)
        {
            string ret = "";

            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).AddHours(7).ToString("yyyy-MM-dd") + " 00:00:00";
            var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).AddHours(7).ToString("yyyy-MM-dd") + " 23:59:59";

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/orders.json?status=cancelled&created_at_min=" + dateFrom + "&created_at_max=" + dateTo;
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.Headers.Add("X-Shopify-Access-Token", iden.API_password);
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseServer = "";

            using (WebResponse response = myReq.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseServer = reader.ReadToEnd();
                }
            }

            if (responseServer != null)
            {
                //try
                //{
                var listOrder = JsonConvert.DeserializeObject(responseServer, typeof(ResultOrderShopify)) as ResultOrderShopify;
                if (listOrder != null)
                    if (listOrder.orders != null)
                    {
                        if (listOrder.orders.Count() > 0)
                        {
                            var orderFilterCancelled = listOrder.orders.Where(p => p.cancel_reason != null && p.cancelled_at != null).ToList();
                            var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();
                            string ordersn = "";
                            jmlhOrderCancel = 0;
                            if (orderFilterCancelled.Count() > 0)
                            {
                                foreach (var item in listOrder.orders)
                                {
                                    if (OrderNoInDb.Contains(item.id.ToString() + ";" + Convert.ToString(item.order_number)))
                                    {
                                        ordersn = ordersn + "'" + item.id.ToString() + ";" + Convert.ToString(item.order_number) + "',";
                                    }
                                }
                            }

                            if (orderFilterCancelled.Count() > 0 && !string.IsNullOrEmpty(ordersn))
                            {
                                ordersn = ordersn.Substring(0, ordersn.Length - 1);
                                var brgAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connID + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI <> '11' AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "'");
                                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2', STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI <> '11' AND CUST = '" + CUST + "'");
                                if (rowAffected > 0)
                                {
                                    //add by Tri 1 sep 2020, hapus packing list
                                    var delPL = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03B WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN (" + ordersn + ")  AND STATUS_TRANSAKSI = '11' AND CUST = '" + CUST + "')");
                                    var delPLDetail = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03C WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN (" + ordersn + ")  AND STATUS_TRANSAKSI = '11' AND CUST = '" + CUST + "')");
                                    //end add by Tri 1 sep 2020, hapus packing list
                                    //add by Tri 4 Des 2019, isi cancel reason
                                    var sSQL1 = "";
                                    var sSQL2 = "SELECT * INTO #TEMP FROM (";
                                    var listReason = new Dictionary<string, string>();

                                    foreach (var order in listOrder.orders)
                                    {
                                        string reasonValue;
                                        if (listReason.TryGetValue(order.id.ToString() + ";" + Convert.ToString(order.order_number), out reasonValue))
                                        {
                                            if (!string.IsNullOrEmpty(sSQL1))
                                            {
                                                sSQL1 += " UNION ALL ";
                                            }
                                            sSQL1 += " SELECT '" + order.id.ToString() + ";" + Convert.ToString(order.order_number) + "' NO_REFERENSI, '" + listReason[order.id.ToString() + ";" + Convert.ToString(order.order_number)] + "' ALASAN ";
                                        }
                                    }
                                    sSQL2 += sSQL1 + ") as qry; INSERT INTO SOT01D (NO_BUKTI, CATATAN_1, USERNAME) ";
                                    sSQL2 += " SELECT A.NO_BUKTI, ALASAN, 'AUTO_SHOPIFY' FROM SOT01A A INNER JOIN #TEMP T ON A.NO_REFERENSI = T.NO_REFERENSI ";
                                    sSQL2 += " LEFT JOIN SOT01D D ON A.NO_BUKTI = D.NO_BUKTI WHERE ISNULL(D.NO_BUKTI, '') = ''";
                                    EDB.ExecuteSQL("MOConnectionString", CommandType.Text, sSQL2);
                                    var nobuk = ErasoftDbContext.SOT01A.Where(m => m.NO_REFERENSI == ordersn && m.CUST == CUST).Select(m => m.NO_BUKTI).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(nobuk))
                                    {
                                        var sot01d = ErasoftDbContext.SOT01D.Where(m => m.NO_BUKTI == nobuk).FirstOrDefault();
                                        if (sot01d == null)
                                        {
                                            EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "INSERT INTO SOT01D(NO_BUKTI, CATATAN_1, USERNAME) VALUES ('" + nobuk + "','cancel','AUTO SHOPIFY')");
                                        }
                                    }
                                    //end add by Tri 4 Des 2019, isi cancel reason

                                    var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN (" + ordersn + ") AND STATUS <> '2' AND ST_POSTING = 'T' AND CUST = '" + CUST + "'");

                                    new StokControllerJob().updateStockMarketPlace(connID, iden.DatabasePathErasoft, iden.username);
                                }
                                jmlhOrderCancel = jmlhOrderCancel + rowAffected;
                                if (jmlhOrderCancel > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderCancel) + " Pesanan dari Shopify dibatalkan.");
                                }
                                //}
                            }
                        }
                    }
                //}
                //catch (Exception ex2)
                //{
                //}
            }

            return ret;
        }


        [AutomaticRetry(Attempts = 3)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Harga Jual Produk {obj} ke Shopify gagal.")]
        public async Task<string> Shopify_UpdatePrice_Job(string dbPathEra, string kdbrgMO, string log_CUST, string log_ActionCategory, string log_ActionName, ShopifyAPIData iden, string brg_mp, double price)
        {
            string ret = "";
            SetupContext(iden);

            string[] brg_mp_split = brg_mp.Split(';');

            //string urll = "https://{0}:{1}@{2}.myshopify.com/admin/products/{3}.json";
            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/variants/{3}.json";
            var kodeBarang = "";
            if (brg_mp_split[1] != "0")
            {
                kodeBarang = brg_mp_split[1];
            }
            else
            {
                kodeBarang = brg_mp_split[0];
            }

            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store, Convert.ToInt64(kodeBarang));

            ShopifyUpdateHargaProduct putProdData = new ShopifyUpdateHargaProduct
            {
                //id = Convert.ToInt64(brg_mp_split[0]),
                variant = new ShopifyUpdateHargaProductVariant()
            };
            //ShopifyUpdateHargaProductVariant variants = new ShopifyUpdateHargaProductVariant
            //{
            //    id = Convert.ToInt64(kodeBarang),
            //    //product_id = Convert.ToInt64(brg_mp_split[0]),
            //    price = Convert.ToString(price)
            //};

            putProdData.variant.id = Convert.ToInt64(kodeBarang);
            putProdData.variant.price = Convert.ToString(price);

            //ShopifyUpdateHarga putData = new ShopifyUpdateHarga
            //{
            //    product = putProdData
            //};

            string myData = JsonConvert.SerializeObject(putProdData);

            string responseFromServer = "";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", (iden.API_password));
            var content = new StringContent(myData, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
            HttpResponseMessage clientResponse = await client.PutAsync(vformatUrl, content);

            using (HttpContent responseContent = clientResponse.Content)
            {
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    responseFromServer = await reader.ReadToEndAsync();
                }
            };

            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            //myReq.Method = "PUT";
            //myReq.Headers.Add("X-Shopify-Access-Token", (iden.API_password));
            //myReq.Accept = "application/json";
            //myReq.ContentType = "application/json";
            //myReq.ContentLength = myData.Length;
            //using (var dataStream = myReq.GetRequestStream())
            //{
            //    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
            //}
            //using (WebResponse response = myReq.GetResponse())
            //{
            //    using (Stream stream = response.GetResponseStream())
            //    {
            //        StreamReader reader = new StreamReader(stream);
            //        responseFromServer = reader.ReadToEnd();
            //    }
            //}


            if (responseFromServer != "")
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ResultUpdatePriceVariant)) as ResultUpdatePriceVariant;
                    if (!string.IsNullOrWhiteSpace(result.ToString()))
                    {
                        if (result != null)
                        {
                            if (result.variant != null)
                            {
                                //foreach (var item in result.product.variants)
                                //{
                                //    //if (item.price == Convert.ToString(price))
                                //    //{
                                //        //throw new Exception("Success update stock " + stf02_brg + ": " + Convert.ToString(qty) + " stock");
                                //    }
                                //}
                            }
                            else
                            {
                                var msgError = "";
                                if (result.errors != null)
                                {
                                    msgError = result.errors;
                                }
                                throw new Exception("Failed update harga " + Convert.ToString(brg_mp_split[0]) + ": price " + Convert.ToString(price) + ". " + msgError);
                            }
                        }
                        else
                        {
                            //if (result.errors.Length > 0)
                            //{
                            throw new Exception("Failed update harga " + Convert.ToString(brg_mp_split[0]) + ": price " + Convert.ToString(price) + ". API no response");
                            //}
                        }
                    }
                    else
                    {
                        throw new Exception("Failed update harga " + Convert.ToString(brg_mp_split[0]) + ": price " + Convert.ToString(price) + ". API no response");
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    throw new Exception(msg);
                }
            }
            return ret;
        }


        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Status Cancel Pesanan {obj} ke Shopify Gagal.")]
        public async Task<string> Shopify_SetOrderStatusCancelled(string dbPathEra, string orderId, string log_CUST, string log_ActionCategory, string log_ActionName, ShopifyAPIData iden)
        {
            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            string[] splitOrderID = orderId.Split(';');

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/orders/" + splitOrderID[0] + "/cancel.json";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "POST";
            myReq.Headers.Add("X-Shopify-Access-Token", iden.API_password);
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseServer = "";

            //try
            //{
            using (WebResponse response = myReq.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseServer = reader.ReadToEnd();
                }
            }

            if (responseServer != null)
            {

            }
            return ret;
        }


        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Status Paid Pesanan {obj} ke Shopify Gagal.")]
        public async Task<string> Shopify_SetOrderStatusPaid(string dbPathEra, string orderId, string log_CUST, string log_ActionCategory, string log_ActionName, ShopifyAPIData iden, double totalSemua)
        {
            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            var resultParentID = await Shopify_GetTransactionParentID(orderId, iden);

            string[] splitOrderID = orderId.Split(';');

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/orders/" + splitOrderID[0] + "/transactions.json";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store);

            CreateTransactionPaid body = new CreateTransactionPaid
            {
                transaction = new Transaction()
            };

            Transaction transaksi = new Transaction
            {
                kind = "capture",
                gateway = "manual",
                //amount = "14300.00",
                amount = Convert.ToString(totalSemua),
                //parent_id = "2881947566139",
                parent_id = resultParentID.id_parent_transaction.ToString(),
                status = "success",
                currency = "IDR"
            };

            body.transaction = transaksi;

            string myData = JsonConvert.SerializeObject(body);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", (iden.API_password));
            var content = new StringContent(myData, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
            string responseFromServer = "";

            try
            {
                HttpResponseMessage clientResponse = await client.PostAsync(vformatUrl, content);

                using (HttpContent responseContent = clientResponse.Content)
                {
                    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                    {
                        responseFromServer = await reader.ReadToEndAsync();
                    }
                };
            }
            catch (Exception ex)
            {
            }

            if (responseFromServer != null)
            {

            }
            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Status Faktur Pesanan {obj} ke Shopify Gagal.")]
        public async Task<string> Shopify_SetOrderStatusFulfillment(string dbPathEra, string orderId, string log_CUST, string log_ActionCategory, string log_ActionName, ShopifyAPIData iden)
        {
            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            string[] splitOrderID = orderId.Split(';');

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/orders/" + splitOrderID[0] + "/fulfillments.json";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store);

            RequestFulfillment body = new RequestFulfillment
            {
                fulfillment = new FulfillmentDataRequest()
            };

            FulfillmentDataRequest dataFulfillment = new FulfillmentDataRequest
            {
                location_id = null,
                tracking_number = null,
                tracking_urls = null,
                notify_customer = true
            };

            body.fulfillment = dataFulfillment;

            string myData = JsonConvert.SerializeObject(body);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", (iden.API_password));
            var content = new StringContent(myData, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
            string responseFromServer = "";

            try
            {
                HttpResponseMessage clientResponse = await client.PostAsync(vformatUrl, content);

                using (HttpContent responseContent = clientResponse.Content)
                {
                    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                    {
                        responseFromServer = await reader.ReadToEndAsync();
                    }
                };
            }
            catch (Exception ex)
            {
            }

            if (responseFromServer != null)
            {

            }
            return ret;
        }

        public async Task<BindingBase> Shopify_GetTransactionParentID(string orderId, ShopifyAPIData iden)
        {
            BindingBase ret = new BindingBase();
            ret.id_parent_transaction = "";

            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            string[] splitOrderID = orderId.Split(';');

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/orders/" + splitOrderID[0] + "/transactions.json";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.Headers.Add("X-Shopify-Access-Token", iden.API_password);
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseServer = "";

            //try
            //{
            using (WebResponse response = myReq.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseServer = reader.ReadToEnd();
                }
            }

            if (responseServer != null)
            {
                var result = JsonConvert.DeserializeObject(responseServer, typeof(ResultGetTransaction)) as ResultGetTransaction;
                if (!string.IsNullOrWhiteSpace(result.ToString()))
                {
                    if (result.transactions != null && result.transactions.Count() == 1)
                    {
                        foreach (var item in result.transactions)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(item.id)) && string.IsNullOrEmpty(Convert.ToString(item.parent_id)))
                            {
                                ret.id_parent_transaction = Convert.ToString(item.id);
                            }
                        }
                    }
                }

            }
            return ret;
        }

        public async Task<string> Shopify_UpdateInventoryItemSKU(ShopifyAPIData iden, string sku, long inventory_item_id)
        {
            string ret = "";

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/api/2020-07/inventory_items/{3}.json";

            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store, inventory_item_id);

            ShopifyUpdateInventoryItemSKU putProdData = new ShopifyUpdateInventoryItemSKU
            {
                inventory_item = new ShopifyUpdateInventoryItemSKU_Inventory_Item()
            };

            putProdData.inventory_item.id = inventory_item_id;
            putProdData.inventory_item.sku = sku;
            putProdData.inventory_item.cost = null;
            putProdData.inventory_item.tracked = true;
            putProdData.inventory_item.requires_shipping = true;


            string myData = JsonConvert.SerializeObject(putProdData);

            string responseFromServer = "";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", (iden.API_password));
            var content = new StringContent(myData, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
            HttpResponseMessage clientResponse = await client.PutAsync(vformatUrl, content);

            using (HttpContent responseContent = clientResponse.Content)
            {
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    responseFromServer = await reader.ReadToEndAsync();
                }
            };

            if (responseFromServer != "")
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyUpdateInventoryItemSKUResult)) as ShopifyUpdateInventoryItemSKUResult;
                    if (!string.IsNullOrWhiteSpace(result.ToString()))
                    {
                        if (result != null)
                        {
                            if (result.inventory_item != null)
                            {
                                ret = "success";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    throw new Exception(msg);
                }
            }
            return ret;
        }

        public string Shopify_GetLocationID(ShopifyAPIData dataAPI)
        {
            var result = "";
            var vurl = "https://{0}:{1}@{2}.myshopify.com/admin/shop.json";
            var vformatUrl = String.Format(vurl, dataAPI.API_key, dataAPI.API_password, dataAPI.account_store);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.Headers.Add("X-Shopify-Access-Token", dataAPI.API_password);
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            try
            {
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
                try
                {
                    var resultAPI = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetLocationID)) as ShopifyGetLocationID;

                    if (!String.IsNullOrWhiteSpace(resultAPI.ToString()))
                    {
                        //if (result.shop != null && result.errors == null)
                        if (resultAPI.shop != null)
                        {
                            //if (resultAPI.shop.email == dataAPI.email || resultAPI.shop.customer_email == dataAPI.email)
                            //{
                            if (resultAPI.shop.primary_location_id != 0)
                            {
                                result = Convert.ToString(resultAPI.shop.primary_location_id);
                            }
                            //}
                        }
                    }
                }
                catch (Exception ex2)
                {

                }
            }

            return result;
        }

        public string Shopify_getSingleProductforUpdateStock(ShopifyAPIData iden, string kode_barang)
        {
            string result = "";
            var kodeBrg = "";
            string[] brg_mp = kode_barang.Split(';');

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/products/{3}.json";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store, brg_mp[0].ToString());

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.Headers.Add("X-Shopify-Access-Token", iden.API_password);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";

            string responseFromServer = "";
            try
            {
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
                try
                {
                    var detailBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetItemDetailResult)) as ShopifyGetItemDetailResult;
                    if (detailBrg != null)
                    {
                        if (detailBrg.product != null)
                        {
                            if (Convert.ToString(detailBrg.product.id) != null)
                            {
                                if (detailBrg.product.variants.Count() > 0)
                                {
                                    foreach (var itemVar in detailBrg.product.variants)
                                    {
                                        if (itemVar.product_id.ToString() == brg_mp[0] && itemVar.id.ToString() == brg_mp[1])
                                        {
                                            result = itemVar.inventory_item_id.ToString();
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
                catch (Exception ex2)
                {

                }
            }

            return result;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Shopify Gagal.")]
        public async Task<string> Shopify_CreateProduct(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, ShopifyAPIData iden)
        {
            string ret = "";
            SetupContext(iden);

            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper()).FirstOrDefault();
            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == log_CUST.ToUpper()).FirstOrDefault();
            if (brgInDb == null || marketplace == null)
                return "invalid passing data";
            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
            if (detailBrg == null)
                return "invalid passing data";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    //REQUEST_ID = seconds.ToString(),
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
            //    REQUEST_ACTION = "Create Product",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = kodeProduk,
            //    REQUEST_ATTRIBUTE_2 = brgInDb.NAMA,
            //    REQUEST_STATUS = "Pending",
            //};

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/products.json";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store);

            ShopifyCreateProductData body = new ShopifyCreateProductData
            {
                title = (brgInDb.NAMA + " " + brgInDb.NAMA2).Trim().Replace("’", "`"),
                body_html = brgInDb.Deskripsi.Replace("’", "`"),
                vendor = brgInDb.KET_SORT2,
                product_type = brgInDb.KET_SORT1,
                tags = "",
                template_suffix = "",
                available = true,
                published = true,
                variants = new List<ShopifyCreateProductDataVariant>(),
                images = new List<ShopifyCreateProductImages>()
            };

            ShopifyCreateProductDataVariant variants = new ShopifyCreateProductDataVariant
            {
                title = brgInDb.NAMA,
                option1 = brgInDb.NAMA2,
                price = detailBrg.HJUAL.ToString(),
                inventory_quantity = 1,
                grams = Convert.ToInt32(brgInDb.BERAT * 1000),
                weight = Convert.ToInt64(brgInDb.BERAT),
                weight_unit = "kg",
                sku = detailBrg.BRG
            };

            if (brgInDb.TYPE == "3")
            {
                variants.option1 = null;
                variants.title = null;
            }
            else
            {
                variants.option1 = brgInDb.NAMA;
                variants.title = brgInDb.NAMA;
            }

            body.variants.Add(variants);

            ShopifyCreateProduct HttpBody = new ShopifyCreateProduct
            {
                product = body
            };

            HttpBody.product.body_html = new StokControllerJob().RemoveSpecialCharacters(HttpBody.product.body_html);

            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<li>", "- ").Replace("</li>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<ul>", "").Replace("</ul>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");

            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<p>", "\r\n").Replace("</p>", "\r\n");

            HttpBody.product.body_html = System.Text.RegularExpressions.Regex.Replace(HttpBody.product.body_html, "<.*?>", String.Empty);

            var qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(kodeProduk, "ALL");
            if (qty_stock > 0)
            {
                HttpBody.product.variants[0].inventory_quantity = Convert.ToInt32(qty_stock);
            }
            //end add by calvin 1 mei 2019

            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
                HttpBody.product.images.Add(new ShopifyCreateProductImages { position = "1", src = brgInDb.LINK_GAMBAR_1, alt = brgInDb.NAMA.ToString() });
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
                HttpBody.product.images.Add(new ShopifyCreateProductImages { position = "2", src = brgInDb.LINK_GAMBAR_2, alt = brgInDb.NAMA.ToString() });
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
                HttpBody.product.images.Add(new ShopifyCreateProductImages { position = "3", src = brgInDb.LINK_GAMBAR_3, alt = brgInDb.NAMA.ToString() });


            string myData = JsonConvert.SerializeObject(HttpBody);

            string sSQL = "UPDATE S SET LINK_STATUS='Buat Produk Pending', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', ";
            //jobid;request_action;request_result;request_exception
            string Link_Error = "0;Buat Produk;;";
            sSQL += "LINK_ERROR = '" + Link_Error + "' FROM STF02H S INNER JOIN ARF01 A ON S.IDMARKET = A.RECNUM AND A.CUST = '" + log_CUST + "' WHERE S.BRG = '" + kodeProduk + "' ";
            EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);

            string responseFromServer = "";
            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", (iden.API_password));
            //var content = new StringContent(myData, Encoding.UTF8, "application/json");
            //content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");

            //try
            //{
            //    ////manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            //    HttpResponseMessage clientResponse = await client.PostAsync(vformatUrl, content);

            //    using (HttpContent responseContent = clientResponse.Content)
            //    {
            //        using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
            //        {
            //            responseFromServer = await reader.ReadToEndAsync();
            //        }
            //    };
            //}
            //catch (Exception ex)
            //{
            //    //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    ////manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "POST";
            myReq.Headers.Add("X-Shopify-Access-Token", (iden.API_password));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
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

            if (responseFromServer != null)
            {
                //try
                //{
                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyCreateResult)) as ShopifyCreateResult;
                if (resServer != null)
                {
                    if (resServer != null)
                    {
                        var item = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum).SingleOrDefault();
                        if (item != null)
                        {
                            var brg_mp = "";
                            if (resServer.product.variants.Count() > 0)
                            {
                                brg_mp = Convert.ToString(resServer.product.id) + ";" + Convert.ToString(resServer.product.variants[0].id);
                                item.BRG_MP = brg_mp;
                            }
                            else
                            {
                                brg_mp = Convert.ToString(resServer.product.id) + ";0";
                                item.BRG_MP = brg_mp;
                            }

                            item.LINK_STATUS = "Buat Produk Berhasil";
                            item.LINK_DATETIME = DateTime.UtcNow.AddHours(7);
                            item.LINK_ERROR = "0;Buat Produk;;";
                            ErasoftDbContext.SaveChanges();
                                                        
                            if (brgInDb.TYPE == "4")
                            {
                                //await InitTierVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, resServer.item_id, marketplace);
                                string EDBConnID = EDB.GetConnectionString("ConnId");
                                var sqlStorage = new SqlServerStorage(EDBConnID);
                                var clients = new BackgroundJobClient(sqlStorage);

                                //delay 1 menit, karena API shopee ada delay saat create barang.
                                //clients.Enqueue<ShopifyControllerJob>(x => x.InitTierVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, "Buat Variasi Produk", iden, brgInDb, resServer.item_id, marketplace, currentLog));
#if (DEBUG || Debug_AWS)
                                //await InitTierVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, "Buat Variasi Produk", iden, brgInDb, resServer.item_id, marketplace, currentLog);
#else
                                //clients.Schedule<ShopifyControllerJob>(x => x.InitTierVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, "Buat Variasi Produk", iden, brgInDb, resServer.item_id, marketplace, currentLog), TimeSpan.FromSeconds(30));
#endif

                                //HANDLE VARIANT SHOPIFY
                                var attributeIDGroup = "";
                                var attributeIDItems = "";

                                var listattributeIDGroup = "";
                                var listattributeIDItems = "";

                                var ListVariantSTF02 = ErasoftDbContext.STF02.Where(p => p.PART == kodeProduk).ToList();
                                var ListSettingVariasi = ErasoftDbContext.STF02I.Where(p => p.BRG == kodeProduk && p.MARKET == "JDID").ToList();
                                List<string> byteGambarUploaded = new List<string>();
                                
                                foreach (var itemData in ListVariantSTF02)
                                {
                                    #region varian LV1
                                    if (!string.IsNullOrEmpty(itemData.Sort8))
                                    {
                                        var variant_id_group = ListSettingVariasi.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == itemData.Sort8).FirstOrDefault();
                                        listattributeIDGroup = variant_id_group.MP_JUDUL_VAR + ",";
                                        listattributeIDItems = variant_id_group.MP_VALUE_VAR + ",";
                                    }
                                    #endregion

                                    #region varian LV2
                                    if (!string.IsNullOrEmpty(itemData.Sort9))
                                    {
                                        var variant_id_group = ListSettingVariasi.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == itemData.Sort9).FirstOrDefault();
                                        listattributeIDGroup = listattributeIDGroup + variant_id_group.MP_JUDUL_VAR + ",";
                                        listattributeIDItems = listattributeIDItems + variant_id_group.MP_VALUE_VAR + ",";
                                    }
                                    #endregion

                                    #region varian LV3
                                    if (!string.IsNullOrEmpty(itemData.Sort10))
                                    {
                                        var variant_id_group = ListSettingVariasi.Where(p => p.LEVEL_VAR == 3 && p.KODE_VAR == itemData.Sort10).FirstOrDefault();
                                        listattributeIDGroup = listattributeIDGroup + variant_id_group.MP_JUDUL_VAR + ",";
                                        listattributeIDItems = listattributeIDItems + variant_id_group.MP_VALUE_VAR + ",";
                                    }
                                    #endregion

                                    listattributeIDGroup = listattributeIDGroup.Substring(0, listattributeIDGroup.Length - 1);
                                    listattributeIDItems = listattributeIDItems.Substring(0, listattributeIDItems.Length - 1);
                                                                        
                                    new ShopifyControllerJob().Shopify_CreateProductVariant(iden, resServer.product.id, itemData.Ket_Sort8, itemData.HJUAL.ToString(), itemData.LINK_GAMBAR_1.ToString());

                                }
                                //END HANDLE VARIANT SHOPIFY
                            }


                            if (resServer.product.variants.Count() > 0)
                            {
                                foreach (var variant in resServer.product.variants)
                                {
                                    //Task.Run(() => shopify.Shopify_UpdateInventoryItemSKU(dataAPI, variant.sku ,variant.inventory_item_id).Wait());
                                    new ShopifyControllerJob().Shopify_UpdateInventoryItemSKU(iden, variant.sku, variant.inventory_item_id);
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("item not found");
                        }
                    }
                    else
                    {
                        throw new Exception("error");
                    }
                }
                //}
                //catch (Exception ex2)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                //    ////manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}
            }
            return ret;
        }

        public async Task<string> Shopify_CreateProductImageVariant(ShopifyAPIData iden, long product_id, long variant_id, string url_image)
        {
            string ret = "";
            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/api/2020-07/products/{3}/images.json";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store, product_id);

            ShopifyCreateImageProductVariant body = new ShopifyCreateImageProductVariant
            {
                image = new ShopifyCreateImageProductVariant_Image()
            };

            body.image.variant_ids = new long[] { variant_id };
            body.image.src = url_image;

            string myData = JsonConvert.SerializeObject(body);

            string responseFromServer = "";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "POST";
            myReq.Headers.Add("X-Shopify-Access-Token", (iden.API_password));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
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

            if (responseFromServer != null)
            {
                //try
                //{
                //var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyCreateResult)) as ShopifyCreateResult;
                //if (resServer != null)
                //{
                //    if (resServer != null)
                //    {

                //    }
                //    else
                //    {
                //        throw new Exception("error");
                //    }
                //}

            }
            return ret;
        }

        public async Task<string> Shopify_CreateProductVariant(ShopifyAPIData iden, long product_id, string option, string price, string urlImage)
        {
            string ret = "";
            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/api/2020-07/products/{3}/variants.json";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store, product_id);

            ShopifyCreateProductVariant body = new ShopifyCreateProductVariant
            {
                variant = new ShopifyCreateProductVariant_Detail()
            };

            body.variant.option1 = option;
            body.variant.price = price;

            string myData = JsonConvert.SerializeObject(body);

            string responseFromServer = "";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "POST";
            myReq.Headers.Add("X-Shopify-Access-Token", (iden.API_password));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
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

            if (responseFromServer != null)
            {
                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyResultCreateProductVariant)) as ShopifyResultCreateProductVariant;
                if (resServer != null)
                {
                    if (resServer.variant != null)
                    {
                        new ShopifyControllerJob().Shopify_CreateProductImageVariant(iden, product_id, resServer.variant.id, urlImage);
                    }
                    else
                    {
                        throw new Exception("Gagal Buat Product Variant.");
                    }
                }
                else {
                    throw new Exception("error");
                }
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Produk {obj} ke Shopify gagal.")]
        public async Task<string> Shopify_UpdateProduct(string dbPathEra, string kdbrgMO, string log_CUST, string log_ActionCategory, string log_ActionName, ShopifyAPIData iden, string brg_mp)
        {
            string ret = "";
            SetupContext(iden);

            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == kdbrgMO.ToUpper()).FirstOrDefault();
            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == log_CUST.ToUpper()).FirstOrDefault();
            if (brgInDb == null || marketplace == null)
                return "invalid passing data";
            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kdbrgMO.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
            if (detailBrg == null)
                return "invalid passing data";

            string[] brg_mp_split = brg_mp.Split(';');

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/products/{3}.json";
            //var kodeBarang = "";
            //if (brg_mp_split[1] != "0")
            //{
            //    kodeBarang = brg_mp_split[1];
            //}
            //else
            //{
            //    kodeBarang = brg_mp_split[0];
            //}

            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store, Convert.ToInt64(brg_mp_split[0]));

            ProductFieldUpdate body = new ProductFieldUpdate
            {
                id = Convert.ToInt64(brg_mp_split[0]),
                title = (brgInDb.NAMA + " " + brgInDb.NAMA2).Trim().Replace("’", "`"),
                body_html = brgInDb.Deskripsi.Replace("’", "`"),
                vendor = brgInDb.KET_SORT2,
                product_type = brgInDb.KET_SORT1,
                //tags = "",
                //template_suffix = "",
                available = true,
                published = true,
                variants = new List<VariantProductUpdate>(),
                images = new List<ImagesUpdateProduct>()
            };

            VariantProductUpdate variantsData = new VariantProductUpdate
            {
                id = Convert.ToInt64(brg_mp_split[1]),
                //title = brgInDb.NAMA,
                //option1 = brgInDb.NAMA,
                price = Convert.ToInt32(detailBrg.HJUAL),
                inventory_quantity = 1,
                grams = Convert.ToInt32(brgInDb.BERAT * 1000),
                weight = Convert.ToInt32(brgInDb.BERAT),
                unit_weight = "kg",
                sku = detailBrg.BRG
            };

            if (brgInDb.TYPE == "3")
            {
                variantsData.option1 = "Default Title";
                variantsData.title = "Default Title";
            }
            else
            {
                variantsData.option1 = brgInDb.NAMA;
                variantsData.title = brgInDb.NAMA;
            }

            body.variants.Add(variantsData);

            ShopifyAPIUpdateProduct HttpBody = new ShopifyAPIUpdateProduct
            {
                product = body
            };

            HttpBody.product.body_html = new StokControllerJob().RemoveSpecialCharacters(HttpBody.product.body_html);

            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<li>", "- ").Replace("</li>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<ul>", "").Replace("</ul>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");

            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<p>", "\r\n").Replace("</p>", "\r\n");

            HttpBody.product.body_html = System.Text.RegularExpressions.Regex.Replace(HttpBody.product.body_html, "<.*?>", String.Empty);

            var qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(kdbrgMO, "ALL");
            if (qty_stock > 0)
            {
                HttpBody.product.variants[0].inventory_quantity = Convert.ToInt32(qty_stock);
            }
            //end add by calvin 1 mei 2019

            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
                HttpBody.product.images.Add(new ImagesUpdateProduct { position = 1, src = brgInDb.LINK_GAMBAR_1, alt = brgInDb.NAMA.ToString() });
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
                HttpBody.product.images.Add(new ImagesUpdateProduct { position = 2, src = brgInDb.LINK_GAMBAR_2, alt = brgInDb.NAMA.ToString() });
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
                HttpBody.product.images.Add(new ImagesUpdateProduct { position = 3, src = brgInDb.LINK_GAMBAR_3, alt = brgInDb.NAMA.ToString() });
            if (brgInDb.TYPE == "4")
            {
                //var ListVariant = ErasoftDbContext.STF02.Where(p => p.PART == kdbrgMO).ToList();
                //var ListSettingVariasi = ErasoftDbContext.STF02I.Where(p => p.BRG == kdbrgMO).ToList();
                //add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
                //List<string> byteGambarUploaded = new List<string>();
                ////end add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
                //foreach (var item in ListVariant)
                //{
                //    List<string> Duplikat = HttpBody.product.variants.Select(p => p.sku.ToString()).ToList();
                //    //add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
                //    if (!byteGambarUploaded.Contains(item.Sort5))
                //    {
                //        byteGambarUploaded.Add(item.Sort5);
                //        if (!string.IsNullOrEmpty(item.LINK_GAMBAR_1))
                //            HttpBody.product.images.Add(new ShopifyCreateProductImages { src = item.LINK_GAMBAR_1 });
                //    }
                //}
            }

            string myData = JsonConvert.SerializeObject(HttpBody);

            string responseFromServer = "";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", (iden.API_password));
            var content = new StringContent(myData, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
            HttpResponseMessage clientResponse = await client.PutAsync(vformatUrl, content);

            using (HttpContent responseContent = clientResponse.Content)
            {
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    responseFromServer = await reader.ReadToEndAsync();
                }
            };
            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            //myReq.Method = "PUT";
            //myReq.Headers.Add("X-Shopify-Access-Token", (iden.API_password));
            //myReq.Accept = "application/json";
            //myReq.ContentType = "application/json";
            //myReq.ContentLength = myData.Length;
            //using (var dataStream = myReq.GetRequestStream())
            //{
            //    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
            //}
            //using (WebResponse response = myReq.GetResponse())
            //{
            //    using (Stream stream = response.GetResponseStream())
            //    {
            //        StreamReader reader = new StreamReader(stream);
            //        responseFromServer = reader.ReadToEnd();
            //    }
            //}

            if (responseFromServer != "")
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ResultShopifyAPIUpdateProduct)) as ResultShopifyAPIUpdateProduct;
                    if (!string.IsNullOrWhiteSpace(result.ToString()))
                    {
                        if (result != null)
                        {
                            if (result.product != null)
                            {
                                //foreach (var item in result.product.variants)
                                //{
                                //    //if (item.price == Convert.ToString(price))
                                //    //{
                                //        //throw new Exception("Success update stock " + stf02_brg + ": " + Convert.ToString(qty) + " stock");
                                //    }
                                //}
                            }
                            else
                            {
                                var msgError = "";
                                //if (result != null)
                                //{
                                //    msgError = result;
                                //}
                                throw new Exception("Failed update product " + Convert.ToString(kdbrgMO) + msgError);
                            }
                        }
                        else
                        {
                            //if (result.errors.Length > 0)
                            //{
                            throw new Exception("Failed update product " + Convert.ToString(kdbrgMO) + ". API no response");
                            //}
                        }
                    }
                    else
                    {
                        throw new Exception("Failed update product " + Convert.ToString(kdbrgMO) + ". API no response");
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    throw new Exception(msg);
                }
            }
            return ret;
        }


        public enum StatusOrder
        {
            IN_CANCEL = 1,
            CANCELLED = 2,
            PAID = 3,
            COMPLETED = 4,
            TO_RETURN = 5,
            UNPAID = 6
        }
        protected enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4,
            RePending = 5,
        }

        protected void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, ShopifyAPIData iden, API_LOG_MARKETPLACE data)
        {
            try
            {
                switch (action)
                {
                    case api_status.Pending:
                        {
                            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.no_cust).FirstOrDefault();
                            var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                            {
                                CUST = arf01 != null ? arf01.CUST : iden.no_cust,
                                CUST_ATTRIBUTE_1 = iden.no_cust,
                                CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                                CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                                CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                                CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                                MARKETPLACE = "Shopify",
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
                    case api_status.RePending:
                        {
                            var apiLogInDb = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == data.REQUEST_ID).SingleOrDefault();
                            if (apiLogInDb != null)
                            {
                                apiLogInDb.REQUEST_STATUS = "Pending";
                                ErasoftDbContext.SaveChanges();
                            }
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
            catch (Exception ex)
            {

            }
        }
        public static long CurrentTimeSecond()
        {
            //        return (long)DateTime.Now.ToUniversalTime().Subtract(
            //new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            //).TotalMilliseconds;
            return (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        public IDictionary<string, string> ToKeyValue(object metaToken)
        {
            if (metaToken == null)
            {
                return null;
            }

            Newtonsoft.Json.Linq.JToken token = metaToken as Newtonsoft.Json.Linq.JToken;
            if (token == null)
            {
                return ToKeyValue(Newtonsoft.Json.Linq.JObject.FromObject(metaToken));
            }

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, string>();
                foreach (var child in token.Children().ToList())
                {
                    var childContent = ToKeyValue(child);
                    if (childContent != null)
                    {
                        contentData = contentData.Concat(childContent)
                                                 .ToDictionary(k => k.Key, v => v.Value);
                    }
                }

                return contentData;
            }

            var jValue = token as Newtonsoft.Json.Linq.JValue;
            if (jValue?.Value == null)
            {
                return null;
            }

            var value = jValue?.Type == Newtonsoft.Json.Linq.JTokenType.Date ?
                            jValue?.ToString("o", System.Globalization.CultureInfo.InvariantCulture) :
                            jValue?.ToString(System.Globalization.CultureInfo.InvariantCulture);

            return new Dictionary<string, string> { { token.Path, value } };
        }

        public class ShopifyAPIData
        {
            public string no_cust { get; set; }
            public string username { get; set; }
            public string account_store { get; set; }
            public string API_key { get; set; }
            public string API_password { get; set; }
            public string DatabasePathErasoft { get; set; }
            public string email { get; set; }
            public int rec_num { get; set; }

        }


        public class CreateTransactionPaid
        {
            public Transaction transaction { get; set; }
        }

        public class Transaction
        {
            public string kind { get; set; }
            public string gateway { get; set; }
            public string amount { get; set; }
            public string parent_id { get; set; }
            public string status { get; set; }
            public string currency { get; set; }
        }

        public class RequestFulfillment
        {
            public FulfillmentDataRequest fulfillment { get; set; }
        }

        public class FulfillmentDataRequest
        {
            public string location_id { get; set; }
            public string tracking_number { get; set; }
            public string tracking_urls { get; set; }
            public bool notify_customer { get; set; }
        }


        /////////////////////// SHOPIFY VARIABLE OBJECT JSON ADD BY FAUZI

        // CREATE PRODUCT
        public class ShopifyCreateProduct
        {
            public ShopifyCreateProductData product { get; set; }
        }

        public class ShopifyCreateProductData
        {
            public string title { get; set; }
            public string body_html { get; set; }
            public string vendor { get; set; }
            public string product_type { get; set; }
            public string template_suffix { get; set; }
            public string tags { get; set; }
            public bool published { get; set; }
            public bool available { get; set; }
            public List<ShopifyCreateProductDataVariant> variants { get; set; }
            public List<ShopifyCreateProductImages> images { get; set; }
        }

        public class ShopifyCreateProductDataVariant
        {
            public string title { get; set; }
            public string option1 { get; set; }
            public string price { get; set; }
            public object sku { get; set; }
            public int inventory_quantity { get; set; }
            public int grams { get; set; }
            public float weight { get; set; }
            public string weight_unit { get; set; }
        }

        public class ShopifyCreateProductImages
        {
            public object alt { get; set; }
            public string position { get; set; }
            public string src { get; set; }
        }
        //// END CREATE PRODUCT 

        /// RESULT CREATE PRODUCT SHOPIFY
        public class ShopifyCreateResult
        {
            public ShopifyCreateProductResult product { get; set; }
            public ShopifyCreateProductResultErrors errors { get; set; }
        }

        public class ShopifyCreateProductResultErrors
        {
            public string[] inventory_quantity { get; set; }
        }

        public class ShopifyCreateProductResult
        {
            public long id { get; set; }
            public string title { get; set; }
            public string body_html { get; set; }
            public string vendor { get; set; }
            public string product_type { get; set; }
            public DateTime created_at { get; set; }
            public string handle { get; set; }
            public DateTime updated_at { get; set; }
            public DateTime published_at { get; set; }
            public object template_suffix { get; set; }
            public string published_scope { get; set; }
            public string tags { get; set; }
            public string admin_graphql_api_id { get; set; }
            public ShopifyCreateProductVariantsResult[] variants { get; set; }
            public ShopifyCreateProductOptionResult[] options { get; set; }
            public ShopifyCreateProductImagesResult[] images { get; set; }
            public ShopifyCreateProductImageResult image { get; set; }
        }

        public class ShopifyCreateProductImageResult
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyCreateProductVariantsResult
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string sku { get; set; }
            public int position { get; set; }
            public string inventory_policy { get; set; }
            public object compare_at_price { get; set; }
            public string fulfillment_service { get; set; }
            public object inventory_management { get; set; }
            public string option1 { get; set; }
            public object option2 { get; set; }
            public object option3 { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public bool taxable { get; set; }
            public object barcode { get; set; }
            public int grams { get; set; }
            public object image_id { get; set; }
            public float weight { get; set; }
            public string weight_unit { get; set; }
            public long inventory_item_id { get; set; }
            public int inventory_quantity { get; set; }
            public int old_inventory_quantity { get; set; }
            public bool requires_shipping { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        // RESULT CREATE VARIANT ITEM

        public class ShopifyResultCreateProductVariant
        {
            public ShopifyResultCreateProductVariant_Variant variant { get; set; }
        }

        public class ShopifyResultCreateProductVariant_Variant
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string sku { get; set; }
            public int position { get; set; }
            public string inventory_policy { get; set; }
            public object compare_at_price { get; set; }
            public string fulfillment_service { get; set; }
            public string inventory_management { get; set; }
            public string option1 { get; set; }
            public object option2 { get; set; }
            public object option3 { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public bool taxable { get; set; }
            public object barcode { get; set; }
            public int grams { get; set; }
            public object image_id { get; set; }
            public float weight { get; set; }
            public string weight_unit { get; set; }
            public long inventory_item_id { get; set; }
            public int inventory_quantity { get; set; }
            public int old_inventory_quantity { get; set; }
            public bool requires_shipping { get; set; }
            public string admin_graphql_api_id { get; set; }
        }
        // END RESULT CREATE VARIANT ITEM

        public class ShopifyCreateProductOptionResult
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string name { get; set; }
            public int position { get; set; }
            public string[] values { get; set; }
        }

        public class ShopifyCreateProductImagesResult
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }
        // END RESULT CREATE PRODUCT

        //CREATE IMAGE FOR PRODUCT VARIANT
        public class ShopifyCreateImageProductVariant
        {
            public ShopifyCreateImageProductVariant_Image image { get; set; }
        }

        public class ShopifyCreateImageProductVariant_Image
        {
            public long[] variant_ids { get; set; }
            public string src { get; set; }
        }
        //END CREATE IMAGE FOR PRODUCT VARIANT

        //CREATE ITEM FOR PRODUCT VARIANT
        public class ShopifyCreateProductVariant
        {
            public ShopifyCreateProductVariant_Detail variant { get; set; }
        }

        public class ShopifyCreateProductVariant_Detail
        {
            public string option1 { get; set; }
            public string price { get; set; }
        }
        //END CREATE ITEM FOR PRODUCT VARIANT

        //UPDATE STOCK
        public class ShopifyUpdateStockResult
        {
            public ShopifyUpdateStockResultProduct product { get; set; }
            public ShopifyUpdateStockResultError errors { get; set; }
        }

        public class ShopifyUpdateStockResultError
        {
            public string[] inventory_quantity { get; set; }
        }

        public class ShopifyUpdateStockResultProduct
        {
            public long id { get; set; }
            public string title { get; set; }
            public string body_html { get; set; }
            public string vendor { get; set; }
            public string product_type { get; set; }
            public DateTime created_at { get; set; }
            public string handle { get; set; }
            public DateTime updated_at { get; set; }
            public DateTime published_at { get; set; }
            public string template_suffix { get; set; }
            public string published_scope { get; set; }
            public string tags { get; set; }
            public string admin_graphql_api_id { get; set; }
            public ShopifyUpdateStockResultProductVariant[] variants { get; set; }
            public ShopifyUpdateStockResultProductOption[] options { get; set; }
            public ShopifyUpdateStockResultProductImage1[] images { get; set; }
            public ShopifyUpdateStockResultProductImage image { get; set; }
        }

        public class ShopifyUpdateStockResultProductImage
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyUpdateStockResultProductVariant
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string sku { get; set; }
            public int position { get; set; }
            public string inventory_policy { get; set; }
            public object compare_at_price { get; set; }
            public string fulfillment_service { get; set; }
            public string inventory_management { get; set; }
            public string option1 { get; set; }
            public object option2 { get; set; }
            public object option3 { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public bool taxable { get; set; }
            public string barcode { get; set; }
            public int grams { get; set; }
            public object image_id { get; set; }
            public float weight { get; set; }
            public string weight_unit { get; set; }
            public long inventory_item_id { get; set; }
            public int inventory_quantity { get; set; }
            public int old_inventory_quantity { get; set; }
            public bool requires_shipping { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyUpdateStockResultProductOption
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string name { get; set; }
            public int position { get; set; }
            public string[] values { get; set; }
        }

        public class ShopifyUpdateStockResultProductImage1
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyUpdateHarga
        {
            public ShopifyUpdateHargaProduct product { get; set; }
        }

        public class ShopifyUpdateHargaProduct
        {
            //public long id { get; set; }
            public ShopifyUpdateHargaProductVariant variant { get; set; }
        }

        public class ShopifyUpdateHargaProductVariant
        {
            public long id { get; set; }
            //public long product_id { get; set; }
            public string price { get; set; }
        }


        public class ResultUpdatePriceVariant
        {
            public object variant { get; set; }
            public string errors { get; set; }
        }

        public class Variant
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string sku { get; set; }
            public int position { get; set; }
            public string inventory_policy { get; set; }
            public object compare_at_price { get; set; }
            public string fulfillment_service { get; set; }
            public string inventory_management { get; set; }
            public string option1 { get; set; }
            public object option2 { get; set; }
            public object option3 { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public bool taxable { get; set; }
            public string barcode { get; set; }
            public int grams { get; set; }
            public object image_id { get; set; }
            public float weight { get; set; }
            public string weight_unit { get; set; }
            public long inventory_item_id { get; set; }
            public int inventory_quantity { get; set; }
            public int old_inventory_quantity { get; set; }
            public bool requires_shipping { get; set; }
            public string admin_graphql_api_id { get; set; }
        }


        // result order shopify

        public class ResultOrderShopify
        {
            public ResultOrderShopifyData[] orders { get; set; }
        }

        public class ResultOrderShopifyData
        {
            public long id { get; set; }
            public string email { get; set; }
            public object closed_at { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public int number { get; set; }
            public string note { get; set; }
            public string token { get; set; }
            public string gateway { get; set; }
            public bool test { get; set; }
            public string total_price { get; set; }
            public string subtotal_price { get; set; }
            public int total_weight { get; set; }
            public string total_tax { get; set; }
            public bool taxes_included { get; set; }
            public string currency { get; set; }
            public string financial_status { get; set; }
            public bool confirmed { get; set; }
            public string total_discounts { get; set; }
            public string total_line_items_price { get; set; }
            public string cart_token { get; set; }
            public bool buyer_accepts_marketing { get; set; }
            public string name { get; set; }
            public string referring_site { get; set; }
            public string landing_site { get; set; }
            public object cancelled_at { get; set; }
            public object cancel_reason { get; set; }
            public string total_price_usd { get; set; }
            public string checkout_token { get; set; }
            public object reference { get; set; }
            public long? user_id { get; set; }
            public long? location_id { get; set; }
            public object source_identifier { get; set; }
            public object source_url { get; set; }
            public DateTime processed_at { get; set; }
            public object device_id { get; set; }
            public object phone { get; set; }
            public string customer_locale { get; set; }
            public int app_id { get; set; }
            public string browser_ip { get; set; }
            public object landing_site_ref { get; set; }
            public int order_number { get; set; }
            public object[] discount_applications { get; set; }
            public object[] discount_codes { get; set; }
            public object[] note_attributes { get; set; }
            public string[] payment_gateway_names { get; set; }
            public string processing_method { get; set; }
            public long? checkout_id { get; set; }
            public string source_name { get; set; }
            public string fulfillment_status { get; set; }
            public Tax_Lines[] tax_lines { get; set; }
            public string tags { get; set; }
            public string contact_email { get; set; }
            public string order_status_url { get; set; }
            public string presentment_currency { get; set; }
            public Total_Line_Items_Price_Set total_line_items_price_set { get; set; }
            public Total_Discounts_Set total_discounts_set { get; set; }
            public Total_Shipping_Price_Set total_shipping_price_set { get; set; }
            public Subtotal_Price_Set subtotal_price_set { get; set; }
            public Total_Price_Set total_price_set { get; set; }
            public Total_Tax_Set total_tax_set { get; set; }
            public Line_Items[] line_items { get; set; }
            public Fulfillment[] fulfillments { get; set; }
            public object[] refunds { get; set; }
            public string total_tip_received { get; set; }
            public string admin_graphql_api_id { get; set; }
            public Shipping_Lines[] shipping_lines { get; set; }
            public Billing_Address billing_address { get; set; }
            public Shipping_Address shipping_address { get; set; }
            public Client_Details client_details { get; set; }
            public Customer customer { get; set; }
        }



        public class Total_Line_Items_Price_Set
        {
            public Shop_Money shop_money { get; set; }
            public Presentment_Money presentment_money { get; set; }
        }

        public class Shop_Money
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Total_Discounts_Set
        {
            public Shop_Money1 shop_money { get; set; }
            public Presentment_Money1 presentment_money { get; set; }
        }

        public class Shop_Money1
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money1
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Total_Shipping_Price_Set
        {
            public Shop_Money2 shop_money { get; set; }
            public Presentment_Money2 presentment_money { get; set; }
        }

        public class Shop_Money2
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money2
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Subtotal_Price_Set
        {
            public Shop_Money3 shop_money { get; set; }
            public Presentment_Money3 presentment_money { get; set; }
        }

        public class Shop_Money3
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money3
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Total_Price_Set
        {
            public Shop_Money4 shop_money { get; set; }
            public Presentment_Money4 presentment_money { get; set; }
        }

        public class Shop_Money4
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money4
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Total_Tax_Set
        {
            public Shop_Money5 shop_money { get; set; }
            public Presentment_Money5 presentment_money { get; set; }
        }

        public class Shop_Money5
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money5
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Billing_Address
        {
            public string first_name { get; set; }
            public string address1 { get; set; }
            public string phone { get; set; }
            public string city { get; set; }
            public string zip { get; set; }
            public string province { get; set; }
            public string country { get; set; }
            public string last_name { get; set; }
            public string address2 { get; set; }
            public object company { get; set; }
            public float latitude { get; set; }
            public float longitude { get; set; }
            public string name { get; set; }
            public string country_code { get; set; }
            public string province_code { get; set; }
        }

        public class Shipping_Address
        {
            public string first_name { get; set; }
            public string address1 { get; set; }
            public object phone { get; set; }
            public string city { get; set; }
            public string zip { get; set; }
            public string province { get; set; }
            public string country { get; set; }
            public string last_name { get; set; }
            public string address2 { get; set; }
            public object company { get; set; }
            public float latitude { get; set; }
            public float longitude { get; set; }
            public string name { get; set; }
            public string country_code { get; set; }
            public string province_code { get; set; }
        }

        public class Client_Details
        {
            public string browser_ip { get; set; }
            public string accept_language { get; set; }
            public string user_agent { get; set; }
            public object session_hash { get; set; }
            public int browser_width { get; set; }
            public int browser_height { get; set; }
        }

        public class Customer
        {
            public long id { get; set; }
            public string email { get; set; }
            public bool accepts_marketing { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public int orders_count { get; set; }
            public string state { get; set; }
            public string total_spent { get; set; }
            public long last_order_id { get; set; }
            public object note { get; set; }
            public bool verified_email { get; set; }
            public object multipass_identifier { get; set; }
            public bool tax_exempt { get; set; }
            public object phone { get; set; }
            public string tags { get; set; }
            public string last_order_name { get; set; }
            public string currency { get; set; }
            public DateTime accepts_marketing_updated_at { get; set; }
            public object marketing_opt_in_level { get; set; }
            public string admin_graphql_api_id { get; set; }
            public Default_Address default_address { get; set; }
        }

        public class Default_Address
        {
            public long id { get; set; }
            public long customer_id { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public object company { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string city { get; set; }
            public string province { get; set; }
            public string country { get; set; }
            public string zip { get; set; }
            public object phone { get; set; }
            public string name { get; set; }
            public string province_code { get; set; }
            public string country_code { get; set; }
            public string country_name { get; set; }
            public bool _default { get; set; }
        }

        public class Tax_Lines
        {
            public string price { get; set; }
            public float rate { get; set; }
            public string title { get; set; }
            public Price_Set price_set { get; set; }
        }

        public class Price_Set
        {
            public Shop_Money6 shop_money { get; set; }
            public Presentment_Money6 presentment_money { get; set; }
        }

        public class Shop_Money6
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money6
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Line_Items
        {
            public long id { get; set; }
            public long variant_id { get; set; }
            public string title { get; set; }
            public int quantity { get; set; }
            public string sku { get; set; }
            public string variant_title { get; set; }
            public string vendor { get; set; }
            public string fulfillment_service { get; set; }
            public long product_id { get; set; }
            public bool requires_shipping { get; set; }
            public bool taxable { get; set; }
            public bool gift_card { get; set; }
            public string name { get; set; }
            public string variant_inventory_management { get; set; }
            public object[] properties { get; set; }
            public bool product_exists { get; set; }
            public int fulfillable_quantity { get; set; }
            public int grams { get; set; }
            public string price { get; set; }
            public string total_discount { get; set; }
            public object fulfillment_status { get; set; }
            public Price_Set1 price_set { get; set; }
            public Total_Discount_Set total_discount_set { get; set; }
            public object[] discount_allocations { get; set; }
            public string admin_graphql_api_id { get; set; }
            public Tax_Lines1[] tax_lines { get; set; }
            public Origin_Location origin_location { get; set; }
        }

        public class Price_Set1
        {
            public Shop_Money7 shop_money { get; set; }
            public Presentment_Money7 presentment_money { get; set; }
        }

        public class Shop_Money7
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money7
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Total_Discount_Set
        {
            public Shop_Money8 shop_money { get; set; }
            public Presentment_Money8 presentment_money { get; set; }
        }

        public class Shop_Money8
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money8
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Origin_Location
        {
            public long id { get; set; }
            public string country_code { get; set; }
            public string province_code { get; set; }
            public string name { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string city { get; set; }
            public string zip { get; set; }
        }

        public class Tax_Lines1
        {
            public string title { get; set; }
            public string price { get; set; }
            public float rate { get; set; }
            public Price_Set2 price_set { get; set; }
        }

        public class Price_Set2
        {
            public Shop_Money9 shop_money { get; set; }
            public Presentment_Money9 presentment_money { get; set; }
        }

        public class Shop_Money9
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money9
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Shipping_Lines
        {
            public long id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string code { get; set; }
            public string source { get; set; }
            public object phone { get; set; }
            public object requested_fulfillment_service_id { get; set; }
            public object delivery_category { get; set; }
            public object carrier_identifier { get; set; }
            public string discounted_price { get; set; }
            public Price_Set3 price_set { get; set; }
            public Discounted_Price_Set discounted_price_set { get; set; }
            public object[] discount_allocations { get; set; }
            public object[] tax_lines { get; set; }
        }

        public class Price_Set3
        {
            public Shop_Money10 shop_money { get; set; }
            public Presentment_Money10 presentment_money { get; set; }
        }

        public class Shop_Money10
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money10
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Discounted_Price_Set
        {
            public Shop_Money11 shop_money { get; set; }
            public Presentment_Money11 presentment_money { get; set; }
        }

        public class Shop_Money11
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money11
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }


        public class Fulfillment
        {
            public long id { get; set; }
            public long order_id { get; set; }
            public string status { get; set; }
            public DateTime created_at { get; set; }
            public string service { get; set; }
            public DateTime updated_at { get; set; }
            public string tracking_company { get; set; }
            public object shipment_status { get; set; }
            public long location_id { get; set; }
            public Line_Items1[] line_items { get; set; }
            public string tracking_number { get; set; }
            public string[] tracking_numbers { get; set; }
            public string tracking_url { get; set; }
            public string[] tracking_urls { get; set; }
            public Receipt receipt { get; set; }
            public string name { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class Receipt
        {
        }

        public class Line_Items1
        {
            public long id { get; set; }
            public long variant_id { get; set; }
            public string title { get; set; }
            public int quantity { get; set; }
            public string sku { get; set; }
            public string variant_title { get; set; }
            public string vendor { get; set; }
            public string fulfillment_service { get; set; }
            public long product_id { get; set; }
            public bool requires_shipping { get; set; }
            public bool taxable { get; set; }
            public bool gift_card { get; set; }
            public string name { get; set; }
            public string variant_inventory_management { get; set; }
            public object[] properties { get; set; }
            public bool product_exists { get; set; }
            public int fulfillable_quantity { get; set; }
            public int grams { get; set; }
            public string price { get; set; }
            public string total_discount { get; set; }
            public string fulfillment_status { get; set; }
            public Price_Set3 price_set { get; set; }
            public Total_Discount_Set1 total_discount_set { get; set; }
            public object[] discount_allocations { get; set; }
            public string admin_graphql_api_id { get; set; }
            public Tax_Lines2[] tax_lines { get; set; }
            public Origin_Location1 origin_location { get; set; }
        }

        public class Total_Discount_Set1
        {
            public Shop_Money11 shop_money { get; set; }
            public Presentment_Money11 presentment_money { get; set; }
        }

        public class Origin_Location1
        {
            public long id { get; set; }
            public string country_code { get; set; }
            public string province_code { get; set; }
            public string name { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string city { get; set; }
            public string zip { get; set; }
        }

        public class Tax_Lines2
        {
            public string title { get; set; }
            public string price { get; set; }
            public float rate { get; set; }
            public Price_Set4 price_set { get; set; }
        }

        public class Price_Set4
        {
            public Shop_Money12 shop_money { get; set; }
            public Presentment_Money12 presentment_money { get; set; }
        }

        public class Shop_Money12
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money12
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }


        public class Price_Set5
        {
            public Shop_Money13 shop_money { get; set; }
            public Presentment_Money13 presentment_money { get; set; }
        }

        public class Shop_Money13
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money13
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Shop_Money14
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money14
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        // end result order shopify

        public class BindingBase
        {
            public int status { get; set; }
            public string message { get; set; }
            public int recordCount { get; set; }
            public int exception { get; set; }
            public int totalData { get; set; }
            public string id_parent_transaction { get; set; }
        }

        public class ShopifyGetItemDetailResult
        {
            public ShopifyGetItemDetailResultProduct product { get; set; }
        }

        public class ShopifyGetItemDetailResultProduct
        {
            public long id { get; set; }
            public string title { get; set; }
            public string body_html { get; set; }
            public string vendor { get; set; }
            public string product_type { get; set; }
            public DateTime created_at { get; set; }
            public string handle { get; set; }
            public DateTime updated_at { get; set; }
            public DateTime published_at { get; set; }
            public string template_suffix { get; set; }
            public string published_scope { get; set; }
            public string tags { get; set; }
            public string admin_graphql_api_id { get; set; }
            public ShopifyGetItemDetailResultProductVariant[] variants { get; set; }
            public ShopifyGetItemDetailResultProductOption[] options { get; set; }
            public ShopifyGetItemDetailResultProductImageMore[] images { get; set; }
            public ShopifyGetItemDetailResultProductImage image { get; set; }
        }

        public class ShopifyGetItemDetailResultProductImage
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyGetItemDetailResultProductVariant
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string sku { get; set; }
            public int position { get; set; }
            public string inventory_policy { get; set; }
            public object compare_at_price { get; set; }
            public string fulfillment_service { get; set; }
            public string inventory_management { get; set; }
            public string option1 { get; set; }
            public object option2 { get; set; }
            public object option3 { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public bool taxable { get; set; }
            public string barcode { get; set; }
            public int grams { get; set; }
            public object image_id { get; set; }
            public float weight { get; set; }
            public string weight_unit { get; set; }
            public long inventory_item_id { get; set; }
            public int inventory_quantity { get; set; }
            public int old_inventory_quantity { get; set; }
            public bool requires_shipping { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyGetItemDetailResultProductOption
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string name { get; set; }
            public int position { get; set; }
            public string[] values { get; set; }
        }

        public class ShopifyGetItemDetailResultProductImageMore
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }


        public class ShopifyUpdateInventoryItemSKU
        {
            public ShopifyUpdateInventoryItemSKU_Inventory_Item inventory_item { get; set; }
        }

        public class ShopifyUpdateInventoryItemSKU_Inventory_Item
        {
            public long id { get; set; }
            public string sku { get; set; }
            public string cost { get; set; }
            public bool tracked { get; set; }
            public bool requires_shipping { get; set; }
        }


        public class ShopifyUpdateInventoryItemSKUResult
        {
            public ShopifyUpdateInventoryItemSKUResult_Inventory_Item inventory_item { get; set; }
        }

        public class ShopifyUpdateInventoryItemSKUResult_Inventory_Item
        {
            public long id { get; set; }
            public string sku { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public bool requires_shipping { get; set; }
            public object cost { get; set; }
            public string country_code_of_origin { get; set; }
            //public object province_code_of_origin { get; set; }
            //public object harmonized_system_code { get; set; }
            public bool tracked { get; set; }
            //public object[] country_harmonized_system_codes { get; set; }
            public string admin_graphql_api_id { get; set; }
        }


        public class ShopifyGetLocationID
        {
            public ShopifyGetShopAccountResultLocationID shop { get; set; }
        }

        public class ShopifyGetShopAccountResultLocationID
        {
            public long id { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            //public string domain { get; set; }
            //public string province { get; set; }
            //public string country { get; set; }
            //public string address1 { get; set; }
            //public string zip { get; set; }
            //public string city { get; set; }
            //public object source { get; set; }
            //public string phone { get; set; }
            //public float latitude { get; set; }
            //public float longitude { get; set; }
            public string primary_locale { get; set; }
            //public string address2 { get; set; }
            //public DateTime created_at { get; set; }
            //public DateTime updated_at { get; set; }
            //public string country_code { get; set; }
            //public string country_name { get; set; }
            //public string currency { get; set; }
            public string customer_email { get; set; }
            //public string timezone { get; set; }
            //public string iana_timezone { get; set; }
            //public string shop_owner { get; set; }
            //public string money_format { get; set; }
            //public string money_with_currency_format { get; set; }
            //public string weight_unit { get; set; }
            //public string province_code { get; set; }
            //public bool taxes_included { get; set; }
            //public object tax_shipping { get; set; }
            //public bool county_taxes { get; set; }
            //public string plan_display_name { get; set; }
            //public string plan_name { get; set; }
            //public bool has_discounts { get; set; }
            //public bool has_gift_cards { get; set; }
            //public string myshopify_domain { get; set; }
            //public object google_apps_domain { get; set; }
            //public object google_apps_login_enabled { get; set; }
            //public string money_in_emails_format { get; set; }
            //public string money_with_currency_in_emails_format { get; set; }
            //public bool eligible_for_payments { get; set; }
            //public bool requires_extra_payments_agreement { get; set; }
            //public bool password_enabled { get; set; }
            //public bool has_storefront { get; set; }
            //public bool eligible_for_card_reader_giveaway { get; set; }
            //public bool finances { get; set; }
            public long primary_location_id { get; set; }
            //public string cookie_consent_level { get; set; }
            //public string visitor_tracking_consent_preference { get; set; }
            //public bool force_ssl { get; set; }
            //public bool checkout_api_supported { get; set; }
            //public bool multi_location_enabled { get; set; }
            //public bool setup_required { get; set; }
            //public bool pre_launch_enabled { get; set; }
            //public string[] enabled_presentment_currencies { get; set; }
        }


        public class ResultGetTransaction
        {
            public ResultTransaction[] transactions { get; set; }
        }

        public class ResultTransaction
        {
            public long? id { get; set; }
            public long order_id { get; set; }
            public string kind { get; set; }
            public string gateway { get; set; }
            public string status { get; set; }
            public string message { get; set; }
            public DateTime created_at { get; set; }
            public bool test { get; set; }
            public object authorization { get; set; }
            public object location_id { get; set; }
            public object user_id { get; set; }
            public long? parent_id { get; set; }
            public DateTime processed_at { get; set; }
            public object device_id { get; set; }
            public object receipt { get; set; }
            public object error_code { get; set; }
            public string source_name { get; set; }
            public string amount { get; set; }
            public string currency { get; set; }
            public string admin_graphql_api_id { get; set; }
        }


        public class ShopifyAPIUpdateProduct
        {
            public ProductFieldUpdate product { get; set; }
        }

        public class ProductFieldUpdate
        {
            public long id { get; set; }
            public string title { get; set; }
            public string body_html { get; set; }
            public string vendor { get; set; }
            public string product_type { get; set; }
            public bool published { get; set; }
            public bool available { get; set; }
            public List<VariantProductUpdate> variants { get; set; }
            public List<ImagesUpdateProduct> images { get; set; }
        }

        public class VariantProductUpdate
        {
            public long id { get; set; }
            public string option1 { get; set; }
            public string option2 { get; set; }
            public string option3 { get; set; }
            public string title { get; set; }
            public int price { get; set; }
            public int inventory_quantity { get; set; }
            public int weight { get; set; }
            public string unit_weight { get; set; }
            public int grams { get; set; }
            public string sku { get; set; }
        }

        public class ImagesUpdateProduct
        {
            public int position { get; set; }
            public string src { get; set; }
            public string alt { get; set; }
        }


        public class ResultShopifyAPIUpdateProduct
        {
            public ResultUpdateProduct product { get; set; }
        }

        public class ResultUpdateProduct
        {
            public long id { get; set; }
            public string title { get; set; }
            public string body_html { get; set; }
            public string vendor { get; set; }
            public string product_type { get; set; }
            public DateTime created_at { get; set; }
            public string handle { get; set; }
            public DateTime updated_at { get; set; }
            public DateTime published_at { get; set; }
            public string template_suffix { get; set; }
            public string published_scope { get; set; }
            public string tags { get; set; }
            public string admin_graphql_api_id { get; set; }
            public ResultUpdateProductVariant[] variants { get; set; }
            public Option[] options { get; set; }
            public Image1[] images { get; set; }
            public Image image { get; set; }
        }

        public class Image
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ResultUpdateProductVariant
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string sku { get; set; }
            public int position { get; set; }
            public string inventory_policy { get; set; }
            public object compare_at_price { get; set; }
            public string fulfillment_service { get; set; }
            public string inventory_management { get; set; }
            public string option1 { get; set; }
            public object option2 { get; set; }
            public object option3 { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public bool taxable { get; set; }
            public string barcode { get; set; }
            public int grams { get; set; }
            public long image_id { get; set; }
            public float weight { get; set; }
            public string weight_unit { get; set; }
            public long inventory_item_id { get; set; }
            public int inventory_quantity { get; set; }
            public int old_inventory_quantity { get; set; }
            public bool requires_shipping { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class Option
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string name { get; set; }
            public int position { get; set; }
            public string[] values { get; set; }
        }

        public class Image1
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public long?[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

    }
}