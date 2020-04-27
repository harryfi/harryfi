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
    public class EightTwoCartControllerJob : Controller
    {
        //AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);//string auth = Base64Encode();

        // GET: EightTwoCart
        private string url = "dev.api.82cart.com";
        private string apiKey = "35e8ac7e833721d0bd7d5bc66cb75";
        private string apiCred = "dev82cart";
        private MoDbContext MoDbContext { get; set; }
        private ErasoftContext ErasoftDbContext { get; set; }
        private DatabaseSQL EDB;
        private string username;

        public EightTwoCartControllerJob()
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

        protected void SetupContext(E2CartAPIData data)
        {
            //string ret = "";
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(data.DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, data.DatabasePathErasoft);
            username = data.username;
            //var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.RecNum == data.idmarket).SingleOrDefault();
            //if (arf01inDB != null)
            //{
            //    ret = arf01inDB.TOKEN;
            //}
            //return ret;
        }

        public static long CurrentTimeMillis()
        {
            //        return (long)DateTime.Now.ToUniversalTime().Subtract(
            //new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            //).TotalMilliseconds;
            return (long)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        protected enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4
        }

        protected void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, E2CartAPIData iden, API_LOG_MARKETPLACE data)
        {
            try
            {
                switch (action)
                {
                    case api_status.Pending:
                        {
                            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.API_credential).FirstOrDefault();
                            var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                            {
                                CUST = arf01 != null ? arf01.CUST : iden.API_credential,
                                CUST_ATTRIBUTE_1 = iden.API_credential,
                                CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                                CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                                CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                                CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                                MARKETPLACE = "82Cart",
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
            catch (Exception ex)
            {

            }
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke 82Cart Gagal.")]
        public async Task<string> E2Cart_CreateProduct(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, E2CartAPIData iden)
        {
            string ret = "";
            SetupContext(iden);

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            //handle log activity
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Create Product",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = kodeProduk,
                REQUEST_ATTRIBUTE_2 = "fs : " + iden.API_credential,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            //handle log activity

            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper()).FirstOrDefault();
            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == log_CUST.ToUpper()).FirstOrDefault();
            if (brgInDb == null || marketplace == null)
                return "invalid passing data";
            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
            if (detailBrg == null)
                return "invalid passing data";

            string urll = string.Format("{0}/api/v1/addProduct", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            postData += "&name=" + Uri.EscapeDataString(brgInDb.NAMA);
            postData += "&active=" + Uri.EscapeDataString("1");
            postData += "&visibility=" + Uri.EscapeDataString("both");
            postData += "&available_for_order=" + Uri.EscapeDataString("1");
            postData += "&show_price=" + Uri.EscapeDataString("1");
            postData += "&online_only=" + Uri.EscapeDataString("0");
            postData += "&condition=" + Uri.EscapeDataString("new");
            postData += "&wholesale_price=" + Uri.EscapeDataString(detailBrg.HJUAL.ToString());
            postData += "&price=" + Uri.EscapeDataString(detailBrg.HJUAL.ToString());
            postData += "&on_sale=" + Uri.EscapeDataString("1");
            postData += "&link_rewrite=" + Uri.EscapeDataString("asus-vivobook-2020");
            postData += "&id_category_default=" + Uri.EscapeDataString("2");
            postData += "&width=" + Uri.EscapeDataString(brgInDb.LEBAR.ToString());
            postData += "&height=" + Uri.EscapeDataString(brgInDb.TINGGI.ToString());
            postData += "&depth=" + Uri.EscapeDataString("0");
            postData += "&weight=" + Uri.EscapeDataString(brgInDb.BERAT.ToString());
            postData += "&additional_shipping_cost=" + Uri.EscapeDataString("200000");
            postData += "&minimal_quantity=" + Uri.EscapeDataString("1");
            postData += "&out_of_stock=" + Uri.EscapeDataString("0");

            //Start handle Check Category
            EightTwoCartController.E2CartAPIData dataLocal = new EightTwoCartController.E2CartAPIData
            {
                username = iden.username,
                no_cust = iden.no_cust,
                API_key = iden.API_key,
                API_credential = iden.API_credential,
                DatabasePathErasoft = iden.DatabasePathErasoft
            };
            EightTwoCartController c82CartController = new EightTwoCartController();

            var resultCategory = await c82CartController.E2Cart_CheckAlreadyCategory(dataLocal, brgInDb.KET_SORT1);
            if (!string.IsNullOrEmpty(resultCategory.name_category) && !string.IsNullOrEmpty(resultCategory.id_category))
            {
                postData += "&category=" + Uri.EscapeDataString("[2,3," + resultCategory.id_category + "]");
            }
            else
            {
                resultCategory = await c82CartController.E2Cart_AddCategoryProduct(dataLocal, "3", brgInDb.KET_SORT1);
                postData += "&category=" + Uri.EscapeDataString("[2,3," + resultCategory.id_category + "]");
                //resultCategory = await E2Cart_CheckCategoryAlready(iden, brgInDb.Sort2);
            }
            //End handle Check Category

            //Start handle Check Manufacture/Brand
            var resultManufacture = await c82CartController.E2Cart_CheckAlreadyManufacture(dataLocal, brgInDb.KET_SORT2);
            if (!string.IsNullOrEmpty(resultManufacture.name_manufacture) && !string.IsNullOrEmpty(resultManufacture.id_manufacture))
            {
                postData += "&id_manufacturer=" + Uri.EscapeDataString(resultManufacture.id_manufacture);
            }
            else
            {
                resultManufacture = await c82CartController.E2Cart_AddManufactureProduct(dataLocal, brgInDb.KET_SORT2);
                postData += "&id_manufacturer=" + Uri.EscapeDataString(resultManufacture.id_manufacture);
            }
            //End handle Check Manufacture/Brand

            //Start handle description
            var vDescription = brgInDb.Deskripsi;
            vDescription = new StokControllerJob().RemoveSpecialCharacters(vDescription);

            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
            vDescription = vDescription.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
            vDescription = vDescription.Replace("<li>", "- ").Replace("</li>", "\r\n");
            vDescription = vDescription.Replace("<ul>", "").Replace("</ul>", "\r\n");
            vDescription = vDescription.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
            vDescription = vDescription.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
            vDescription = vDescription.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");
            //end add by nurul 20/1/2020, handle <p> dan enter double di shopee

            //add by calvin 10 september 2019
            vDescription = vDescription.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
            vDescription = vDescription.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
            vDescription = vDescription.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
            vDescription = vDescription.Replace("<p>", "\r\n").Replace("</p>", "\r\n");
            //HttpBody.description = HttpBody.description.Replace("<li>", "- ").Replace("</li>", "\r\n");
            //HttpBody.description = HttpBody.description.Replace("&nbsp;", "");

            vDescription = System.Text.RegularExpressions.Regex.Replace(vDescription, "<.*?>", String.Empty);
            //end add by calvin 10 september 2019

            postData += "&description_short=" + Uri.EscapeDataString(vDescription);
            postData += "&description=" + Uri.EscapeDataString(vDescription);
            //end handle description

            //handle image
            List<string> lGambarUploaded = new List<string>();

            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
            {
                lGambarUploaded.Add(brgInDb.LINK_GAMBAR_1);
            }
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
            {
                lGambarUploaded.Add(brgInDb.LINK_GAMBAR_2);
            }
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
            {
                lGambarUploaded.Add(brgInDb.LINK_GAMBAR_3);
            }
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_4))
            {
                lGambarUploaded.Add(brgInDb.LINK_GAMBAR_4);
            }
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_5))
            {
                lGambarUploaded.Add(brgInDb.LINK_GAMBAR_5);
            }
            //end handle image

            //start handle stock
            double qty_stock = 1;
            qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(kodeProduk, "ALL");
            if (qty_stock > 0)
            {
                postData += "&quantity=" + Uri.EscapeDataString(qty_stock.ToString());
            }
            //end handle stock

            var data = Encoding.ASCII.GetBytes(postData);

            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = data.Length;

            try
            {
                string responseFromServer = "";

                using (var stream = myReq.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream2 = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream2);
                        responseFromServer = reader.ReadToEnd();
                    }
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    var resultAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ResultCreateProduct82Cart)) as ResultCreateProduct82Cart;

                    if (resultAPI != null)
                    {
                        if (resultAPI.error == "none" && resultAPI.results == "success")
                        {
                            if (resultAPI.data.data[0].id_product != null)
                            {
                                var item = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum).SingleOrDefault();
                                if (item != null)
                                {
                                    item.BRG_MP = Convert.ToString(resultAPI.data.data[0].id_product) + ";0";
                                    item.LINK_STATUS = "Buat Produk Berhasil";
                                    item.LINK_DATETIME = DateTime.UtcNow.AddHours(7);
                                    item.LINK_ERROR = "0;Buat Produk;;";
                                    ErasoftDbContext.SaveChanges();
                                }
                                //handle all image was uploaded
                                foreach (var images in lGambarUploaded)
                                {
                                    Task.Run(() => c82CartController.E2Cart_AddImageProduct(dataLocal, item.BRG_MP, images)).Wait();
                                }
                                //end handle all image was uploaded

                                //start handle update stock for default product
                                Task.Run(() => c82CartController.E2Cart_UpdateStock_82Cart(dataLocal, item.BRG, item.BRG_MP, Convert.ToInt32(qty_stock))).Wait();
                                //end handle update stock for default product

                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            }
                        }
                        else
                        {
                            if (resultAPI.error != null && resultAPI.error != "none")
                            {
                                currentLog.REQUEST_EXCEPTION = resultAPI.error.ToString();
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> E2Cart_GetOrderByStatus(E2CartAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar)
        {
            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            var dateFrom = DateTimeOffset.UtcNow.AddDays(-10).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            var dateTo = DateTimeOffset.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

            string urll = string.Format("{0}/api/v1/getOrder?apiKey={1}&apiCredential={2}&date_add_from={3}&date_add_to={4}", iden.API_url, iden.API_key, iden.API_credential, dateFrom, dateTo);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
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
                var listOrder = JsonConvert.DeserializeObject(responseServer, typeof(E2CartOrderResult)) as E2CartOrderResult;

                string[] statusAwaiting = { "1", "3", "10", "11", "13", "14", "16", "17", "18", "19", "20", "21", "23", "25" };
                string[] ordersn_list = listOrder.data.Select(p => p.id_order).ToArray();
                var dariTgl = DateTimeOffset.UtcNow.AddDays(-10).DateTime;
                jmlhNewOrder = 0;
                var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();

                if (stat == StatusOrder.UNPAID)
                {
                    for (int itemOrder = 0; itemOrder < statusAwaiting.Length; itemOrder++)
                    {
                        var orderFilter = listOrder.data.Where(p => p.current_state == statusAwaiting[itemOrder]).ToList();
                        if (orderFilter.Count() > 0)
                        {
                            foreach (var order in orderFilter)
                            {
                                try
                                {
                                    if (!OrderNoInDb.Contains(order.id_order))
                                    {
                                        jmlhNewOrder++;
                                        //await GetOrderDetails(iden, filtered.ToArray(), connID, CUST, NAMA_CUST, stat);
                                        var connIdARF01C = Guid.NewGuid().ToString();
                                        TEMP_82CART_ORDERS batchinsert = new TEMP_82CART_ORDERS();
                                        List<TEMP_82CART_ORDERS_ITEM> batchinsertItem = new List<TEMP_82CART_ORDERS_ITEM>();
                                        string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                        insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                        insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                                        var kabKot = "3174";
                                        var prov = "31";


                                        string fullname = order.firstname.ToString() + " " + order.lastname.ToString();
                                        string nama = fullname.Length > 30 ? order.firstname.Substring(0, 30) : order.lastname.ToString();

                                        insertPembeli += string.Format("('{0}','{1}','{2}','{3}',0,0,'0','01',1, 'IDR', '01', '{4}', 0, 0, 0, 0, '1', 0, 0,'FP', '{5}', '{6}', '{7}', '', '{8}', '{9}', '', '','{10}'),",
                                            ((nama ?? "").Replace("'", "`")),
                                            ((order.delivery_address[0].address1 ?? "").Replace("'", "`")),
                                            ((order.delivery_address[0].phone_mobile ?? "").Replace("'", "`")),
                                            (NAMA_CUST.Replace(',', '.')),
                                            ((order.delivery_address[0].address1 + " " + order.delivery_address[0].address2 ?? "").Replace("'", "`")),
                                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                            (username),
                                            ((order.delivery_address[0].postcode ?? "").Replace("'", "`")),
                                            kabKot,
                                            prov,
                                            connIdARF01C
                                            );
                                        insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
                                        EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);


                                        ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_82CART_ORDERS");
                                        ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_82CART_ORDERS_ITEM");
                                        batchinsertItem = new List<TEMP_82CART_ORDERS_ITEM>();

                                        //2020-04-08T05:12:41
                                        var dateOrder = Convert.ToDateTime(order.date_add).ToString("yyyy-MM-dd HH:mm:ss");
                                        var datePay = dateOrder;
                                        if (order.invoice_date == "0000-00-00 00:00:00")
                                        {
                                            datePay = dateOrder;
                                        }
                                        else
                                        {
                                            datePay = Convert.ToDateTime(order.invoice_date).ToString("yyyy-MM-dd HH:mm:ss");
                                        }


                                        TEMP_82CART_ORDERS newOrder = new TEMP_82CART_ORDERS()
                                        {
                                            actual_shipping_cost = order.total_shipping,
                                            buyer_username = order.firstname + " " + order.lastname,
                                            cod = false,
                                            country = "",
                                            create_time = Convert.ToDateTime(dateOrder),
                                            currency = order.currency,
                                            days_to_ship = 0,
                                            dropshipper = "",
                                            escrow_amount = "",
                                            estimated_shipping_fee = order.total_shipping,
                                            goods_to_declare = false,
                                            message_to_seller = "",
                                            note = "",
                                            note_update_time = Convert.ToDateTime(dateOrder),
                                            ordersn = order.id_order,
                                            //order_status = order.current_state_name,
                                            order_status = "UNPAID",
                                            payment_method = order.payment,
                                            //change by nurul 5/12/2019, local time 
                                            //pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime,
                                            pay_time = Convert.ToDateTime(datePay),
                                            //end change by nurul 5/12/2019, local time 
                                            Recipient_Address_country = order.delivery_address[0].id_country ?? "ID",
                                            Recipient_Address_state = order.delivery_address[0].id_state ?? "",
                                            Recipient_Address_city = order.delivery_address[0].city ?? "",
                                            Recipient_Address_town = "",
                                            Recipient_Address_district = "",
                                            Recipient_Address_full_address = order.delivery_address[0].address1 ?? "",
                                            Recipient_Address_name = order.firstname + " " + order.lastname ?? "",
                                            Recipient_Address_phone = order.delivery_address[0].phone_mobile ?? order.delivery_address[0].phone ?? "",
                                            Recipient_Address_zipcode = order.delivery_address[0].postcode,
                                            service_code = order.id_carrier,
                                            shipping_carrier = order.name_carrier,
                                            total_amount = order.total_paid,
                                            tracking_no = order.shipping_number ?? "",
                                            update_time = Convert.ToDateTime(dateOrder),
                                            CONN_ID = connID,
                                            CUST = CUST,
                                            NAMA_CUST = NAMA_CUST
                                        };
                                        foreach (var item in order.order_detail)
                                        {
                                            //var product_id = "";
                                            //var name_brg = "";
                                            var name_brg_variasi = "";
                                            if (item.product_attribute_id == "0")
                                            {
                                                //var kodeBrg = ErasoftDbContext.STF02.SingleOrDefault(p => p.NAMA.Contains(item.product_name) && p.PART == "");
                                                //product_id = kodeBrg.BRG;
                                                //product_id = item.product_id;
                                                //name_brg = item.product_name;
                                            }
                                            else
                                            {
                                                //product_id = item.product_attribute_id;
                                                name_brg_variasi = item.product_name;
                                            }

                                            TEMP_82CART_ORDERS_ITEM newOrderItem = new TEMP_82CART_ORDERS_ITEM()
                                            {
                                                ordersn = order.id_order,
                                                is_wholesale = false,
                                                item_id = item.product_id,
                                                item_name = item.product_name,
                                                item_sku = item.reference,
                                                variation_discounted_price = item.original_product_price,
                                                variation_id = item.product_attribute_id,
                                                variation_name = name_brg_variasi,
                                                variation_original_price = item.original_product_price,
                                                variation_quantity_purchased = Convert.ToInt32(item.product_quantity),
                                                variation_sku = item.reference,
                                                weight = 0,
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

                                        ErasoftDbContext.TEMP_82CART_ORDERS.Add(batchinsert);
                                        ErasoftDbContext.TEMP_82CART_ORDERS_ITEM.AddRange(batchinsertItem);
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
                                            CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 1;
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
                    }

                    if (jmlhNewOrder > 0)
                    {
                        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari 82Cart.");
                        new StokControllerJob().updateStockMarketPlace(connID, iden.DatabasePathErasoft, iden.username);
                    }
                }

                if (stat == StatusOrder.PAID)
                {
                    string[] statusCAP = { "2", "15" };
                    string ordersn = "";
                    jmlhPesananDibayar = 0;

                    for (int itemOrderExisting = 0; itemOrderExisting < statusCAP.Length; itemOrderExisting++)
                    {
                        var orderFilterExisting = listOrder.data.Where(p => p.current_state == statusCAP[itemOrderExisting]).ToList();
                        foreach (var item in orderFilterExisting)
                        {
                            if (OrderNoInDb.Contains(item.id_order))
                            {
                                ordersn = ordersn + "'" + item.id_order + "',";
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(ordersn))
                    {
                        ordersn = ordersn.Substring(0, ordersn.Length - 1);
                        var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '0'");
                        if (rowAffected > 0)
                        {
                            jmlhPesananDibayar++;
                        }
                    }

                    if (jmlhPesananDibayar > 0)
                    {
                        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhPesananDibayar) + " Pesanan terbayar dari 82Cart.");
                    }
                }
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> E2Cart_GetOrderByStatusCompleted(E2CartAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            var dateFrom = DateTimeOffset.UtcNow.AddDays(-10).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            var dateTo = DateTimeOffset.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

            string urll = string.Format("{0}/api/v1/getOrder?apiKey={1}&apiCredential={2}&date_add_from={3}&date_add_to={4}", iden.API_url, iden.API_key, iden.API_credential, dateFrom, dateTo);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            try
            {
                using (WebResponse response = myReq.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseServer = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {

            }


            if (responseServer != null)
            {
                //try
                //{
                var listOrder = JsonConvert.DeserializeObject(responseServer, typeof(E2CartOrderResult)) as E2CartOrderResult;

                var statusCompleted = "5";
                var orderFilterCompleted = listOrder.data.Where(p => p.current_state == statusCompleted).ToList();
                var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();
                string ordersn = "";
                jmlhNewOrder = 0;
                foreach (var item in orderFilterCompleted)
                {
                    if (OrderNoInDb.Contains(item.id_order))
                    {
                        ordersn = ordersn + "'" + item.id_order + "',";
                    }
                }
                if (orderFilterCompleted.Count() > 0 && !string.IsNullOrEmpty(ordersn))
                {
                    ordersn = ordersn.Substring(0, ordersn.Length - 1);
                    var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '03'");
                    jmlhNewOrder = jmlhNewOrder + rowAffected;
                    if (jmlhNewOrder > 0)
                    {
                        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhNewOrder) + " Pesanan dari 82Cart sudah selesai.");
                    }
                }
            }
            return ret;
        }


        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> E2Cart_GetOrderByStatusCancelled(E2CartAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            var dateFrom = DateTimeOffset.UtcNow.AddDays(-10).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            var dateTo = DateTimeOffset.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

            //DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = string.Format("{0}/api/v1/getOrder?apiKey={1}&apiCredential={2}&date_add_from={3}&date_add_to={4}", iden.API_url, iden.API_key, iden.API_credential, dateFrom, dateTo);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
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
                var listOrder = JsonConvert.DeserializeObject(responseServer, typeof(E2CartOrderResult)) as E2CartOrderResult;

                var statusCancel = "6";
                var orderFilterCancel = listOrder.data.Where(p => p.current_state == statusCancel).ToList();
                var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();
                string ordersn = "";
                jmlhNewOrder = 0;
                foreach (var item in orderFilterCancel)
                {
                    if (OrderNoInDb.Contains(item.id_order))
                    {
                        ordersn = ordersn + "'" + item.id_order + "',";
                    }
                }
                if (orderFilterCancel.Count() > 0 && !string.IsNullOrEmpty(ordersn))
                {
                    ordersn = ordersn.Substring(0, ordersn.Length - 1);
                    var brgAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connID + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI <> '11' AND BRG <> 'NOT_FOUND'");
                    var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2', STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI <> '11'");
                    if (rowAffected > 0)
                    {
                        //add by Tri 4 Des 2019, isi cancel reason
                        var sSQL = "";
                        var sSQL2 = "SELECT * INTO #TEMP FROM (";
                        var listReason = new Dictionary<string, string>();

                        foreach (var order in listOrder.data)
                        {
                            string reasonValue;
                            if (listReason.TryGetValue(order.id_order, out reasonValue))
                            {
                                if (!string.IsNullOrEmpty(sSQL))
                                {
                                    sSQL += " UNION ALL ";
                                }
                                sSQL += " SELECT '" + order.id_order + "' NO_REFERENSI, '" + listReason[order.id_order] + "' ALASAN ";
                            }
                        }
                        sSQL2 += sSQL + ") as qry; INSERT INTO SOT01D (NO_BUKTI, CATATAN_1, USERNAME) ";
                        sSQL2 += " SELECT A.NO_BUKTI, ALASAN, 'AUTO_82CART' FROM SOT01A A INNER JOIN #TEMP T ON A.NO_REFERENSI = T.NO_REFERENSI ";
                        sSQL2 += " LEFT JOIN SOT01D D ON A.NO_BUKTI = D.NO_BUKTI WHERE ISNULL(D.NO_BUKTI, '') = ''";
                        EDB.ExecuteSQL("MOConnectionString", CommandType.Text, sSQL2);
                        //var nobuk = ErasoftDbContext.SOT01A.Where(m => m.NO_REFERENSI == ordersn && m.CUST == CUST).Select(m => m.NO_BUKTI).FirstOrDefault();
                        //if (!string.IsNullOrEmpty(nobuk))
                        //{
                        //    var sot01d = ErasoftDbContext.SOT01D.Where(m => m.NO_BUKTI == nobuk).FirstOrDefault();
                        //    if (sot01d == null)
                        //    {
                        //        EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "INSERT INTO SOT01D(NO_BUKTI, CATATAN_1, USERNAME) VALUES ('" + nobuk + "','" + order.reason + "','AUTO LAZADA')");
                        //    }
                        //}
                        //end add by Tri 4 Des 2019, isi cancel reason

                        var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN (" + ordersn + ") AND STATUS <> '2' AND ST_POSTING = 'T'");

                        new StokControllerJob().updateStockMarketPlace(connID, iden.DatabasePathErasoft, iden.username);
                    }
                    jmlhNewOrder = jmlhNewOrder + rowAffected;
                    //if (listOrder.more)
                    //{
                    //    await GetOrderByStatusCancelled(iden, stat, CUST, NAMA_CUST, page + 50, jmlhNewOrder);
                    //}
                    //else
                    //{
                    if (jmlhNewOrder > 0)
                    {
                        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhNewOrder) + " Pesanan dari 82Cart dibatalkan.");
                    }
                    //}
                }

                //}
                //catch (Exception ex2)
                //{
                //}
            }
            return ret;
        }


        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Status Cancel Pesanan {obj} ke 82Cart Gagal.")]
        public async Task<string> E2Cart_SetOrderStatus(E2CartAPIData iden, string dbPathEra, string log_CUST, string log_ActionCategory, string log_ActionName, string orderId, string codeStatus)
        {
            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            var dateFrom = DateTimeOffset.UtcNow.AddDays(-10).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            var dateTo = DateTimeOffset.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

            string urll = string.Format("{0}/api/v1/editOrder", iden.API_url);

            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            postData += "&id_order=" + Uri.EscapeDataString(orderId);
            postData += "&current_state=" + Uri.EscapeDataString(codeStatus);

            var data = Encoding.ASCII.GetBytes(postData);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = data.Length;

            string responseServer = "";

            try
            {
                using (var stream = myReq.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream2 = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream2);
                        responseServer = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {

            }


            if (responseServer != null)
            {
                //var listOrder = JsonConvert.DeserializeObject(responseServer, typeof(E2CartOrderResult)) as E2CartOrderResult;

                //var statusCancel = "6";
                //var orderFilterCanceled = listOrder.data.Where(p => p.current_state == statusCancel).ToList();
                //string ordersn = "";
                //foreach (var item in orderFilterCanceled)
                //{
                //    ordersn = ordersn + "'" + item + "',";
                //}
                //if (orderFilterCanceled.Count() > 0)
                //{
                //ordersn = ordersn.Substring(0, ordersn.Length - 1);
                //var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '03'");
                //jmlhNewOrder = jmlhNewOrder + rowAffected;
                ////if (listOrder.more)
                ////{
                ////    await GetOrderByStatusCompleted(iden, stat, CUST, NAMA_CUST, page + 50, jmlhNewOrder);
                ////}
                ////else
                ////{
                //if (jmlhNewOrder > 0)
                //{
                //    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                //    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhNewOrder) + " Pesanan dari 82Cart sudah selesai.");
                //}
                //}
                //}
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Harga Jual Produk {obj} ke 82Cart gagal.")]
        public async Task<string> E2Cart_UpdatePrice_82Cart(E2CartAPIData iden, string brg_mo, string brg_mp, int priceInduk, int priceGrosir)
        {
            SetupContext(iden);
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Price", //ganti
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.API_credential,
                REQUEST_STATUS = "Pending",
            };

            string[] brg_mp_split = brg_mp.Split(';');

            string urll = string.Format("{0}/api/v1/editProductdetail", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            if(brg_mp_split[1] == "0")
            {
                postData += "&id_product=" + Uri.EscapeDataString(brg_mp_split[0]);
                postData += "&price=" + Uri.EscapeDataString(priceInduk.ToString());
                postData += "&wholesale_price=" + Uri.EscapeDataString(priceGrosir.ToString());
            }
            else
            {
                postData += "&id_product=" + Uri.EscapeDataString(brg_mp_split[0]);
                postData += "&id_product_attribute=" + Uri.EscapeDataString(brg_mp_split[1]);
                postData += "&price_attribute=" + Uri.EscapeDataString(priceInduk.ToString());
                postData += "&wholesale_price=" + Uri.EscapeDataString(priceGrosir.ToString());

            }
            
            var data = Encoding.ASCII.GetBytes(postData);

            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = data.Length;

            string responseFromServer = "";
            try
            {
                using (var stream = myReq.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream2 = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream2);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
                throw new Exception(currentLog.REQUEST_EXCEPTION);
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                try
                {
                    var resultAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ResultUpdatePrice)) as ResultUpdatePrice;
                    if (resultAPI.error == "none" && resultAPI.data.Length > 0)
                    {
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = resultAPI.error.ToString();
                        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                        throw new Exception(currentLog.REQUEST_EXCEPTION);
                    }
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                    throw new Exception(currentLog.REQUEST_EXCEPTION);
                }

            }

            return "";
        }

        public enum StatusOrder
        {
            //IN_CANCEL = 1,
            CANCELLED = 6,
            READY_TO_SHIP = 4,
            COMPLETED = 5,
            //TO_RETURN = 5,
            PAID = 2,
            UNPAID = 23
        }


        public class E2CartAPIData
        {
            public string no_cust { get; set; }
            public string username { get; set; }
            public string account_store { get; set; }
            public string API_key { get; set; }
            public string API_password { get; set; }
            public string DatabasePathErasoft { get; set; }
            public string email { get; set; }
            public int rec_num { get; set; }
            public string API_url { get; set; }
            public string API_credential { get; set; }
        }

        public class ResultCreateProduct82Cart
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public string results { get; set; }
            public ResultCreateProduct82CartData data { get; set; }
        }

        public class ResultCreateProduct82CartData
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public ResultCreateProductDetail[] data { get; set; }
        }

        public class ResultCreateProductDetail
        {
            public string id_product { get; set; }
            public string name { get; set; }
            public string ean13 { get; set; }
            public string upc { get; set; }
            public string active { get; set; }
            public string visibility { get; set; }
            public string available_for_order { get; set; }
            public string show_price { get; set; }
            public string online_only { get; set; }
            public string condition { get; set; }
            public string description_short { get; set; }
            public string description { get; set; }
            public string meta_keywords { get; set; }
            public string wholesale_price { get; set; }
            public string price { get; set; }
            public string on_sale { get; set; }
            public string meta_title { get; set; }
            public string meta_description { get; set; }
            public string link_rewrite { get; set; }
            public string id_category_default { get; set; }
            public string category_default { get; set; }
            public ResultCreateProductDetailCategory[] category { get; set; }
            public string id_manufacturer { get; set; }
            public string manufacturer { get; set; }
            public string width { get; set; }
            public string height { get; set; }
            public string depth { get; set; }
            public string weight { get; set; }
            public string additional_shipping_cost { get; set; }
            public string quantity { get; set; }
            public string minimal_quantity { get; set; }
            public string out_of_stock { get; set; }
            public string indexed { get; set; }
            public string date_add { get; set; }
            public string date_upd { get; set; }
            public object[] combinations { get; set; }
        }

        public class ResultCreateProductDetailCategory
        {
            public string id_category { get; set; }
            public string category_name { get; set; }
        }

        public class E2CartOrderResult
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public E2CartOrder[] data { get; set; }
        }

        public class E2CartOrder
        {
            public string id_order { get; set; }
            public string reference { get; set; }
            public string current_state { get; set; }
            public string current_state_name { get; set; }
            public string id_carrier { get; set; }
            public string name_carrier { get; set; }
            public string shipping_number { get; set; }
            public string id_customer { get; set; }
            public string firstname { get; set; }
            public string lastname { get; set; }
            public string id_currency { get; set; }
            public string currency { get; set; }
            public string payment { get; set; }
            public string conversion_rate { get; set; }
            public string total_paid { get; set; }
            public string total_paid_tax_incl { get; set; }
            public string total_paid_tax_excl { get; set; }
            public string total_paid_real { get; set; }
            public string total_products { get; set; }
            public string total_discounts { get; set; }
            public string total_shipping { get; set; }
            public string total_wrapping { get; set; }
            public string invoice_date { get; set; }
            public string delivery_date { get; set; }
            public string date_add { get; set; }
            public address[] delivery_address { get; set; }
            public address[] invoice_address { get; set; }
            public order_detail[] order_detail { get; set; }
        }

        public class order_detail
        {
            public string product_id { get; set; }
            public string product_attribute_id { get; set; }
            public string product_name { get; set; }
            public string reference { get; set; }
            public string product_quantity { get; set; }
            public string original_product_price { get; set; }
            public string reduction_percent { get; set; }
            public string reduction_amount { get; set; }
            public string unit_price { get; set; }
            public string total_price { get; set; }
        }

        public class address
        {
            public string id_address { get; set; }
            public string id_country { get; set; }
            public string id_state { get; set; }
            public string id_customer { get; set; }
            public string id_manufacturer { get; set; }
            public string id_supplier { get; set; }
            public string id_warehouse { get; set; }
            public string alias { get; set; }
            public string company { get; set; }
            public string lastname { get; set; }
            public string firstname { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string postcode { get; set; }
            public string city { get; set; }
            public string other { get; set; }
            public string phone { get; set; }
            public string phone_mobile { get; set; }
            public string vat_number { get; set; }
            public string dni { get; set; }
            public string date_add { get; set; }
            public string date_upd { get; set; }
            public string active { get; set; }
            public string deleted { get; set; }
        }

        public class ResultUpdatePrice
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public DetailUpdatePrice[] data { get; set; }
        }

        public class DetailUpdatePrice
        {
            public string id_product { get; set; }
            public string active { get; set; }
            public string name { get; set; }
            public string visibility { get; set; }
            public string available_for_order { get; set; }
            public string show_price { get; set; }
            public string online_only { get; set; }
            public string wholesale_price { get; set; }
            public string price { get; set; }
            public string on_sale { get; set; }
            public string id_category_default { get; set; }
            public string id_manufacturer { get; set; }
            public string width { get; set; }
            public string height { get; set; }
            public string depth { get; set; }
            public string weight { get; set; }
            public string additional_shipping_cost { get; set; }
            public string minimal_quantity { get; set; }
            public string out_of_stock { get; set; }
            public string indexed { get; set; }
            public string quantity { get; set; }
            public string date_add { get; set; }
            public string date_upd { get; set; }
        }
    }
}