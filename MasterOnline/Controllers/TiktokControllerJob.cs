using Erasoft.Function;
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
using System.Net.Http;

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
        public async Task<TiktokAuth> GetRefToken(string cust, string refreshToken, string dbpath, string username, DateTime? tanggal_exptoken, DateTime? tanggal_exprtok)
        {
            SetupContext(dbpath, username);
            DateTime dateNow = DateTime.UtcNow.AddHours(7).AddMinutes(-30);
            DateTime parse = DateTime.Parse(tanggal_exprtok.ToString());
            TimeSpan ts = parse.Subtract(dateNow);
            bool ATExp = false;

            //if (ts.Days < 1 && ts.Hours < 24 && dateNow < tanggal_exptoken)
            if (dateNow > tanggal_exprtok)
            {
                ATExp = true;
            }

            if (ATExp)
            {
                string ret;
                string url;
                url = "https://auth.tiktok-shops.com/api/token/refreshToken";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
                myReq.Method = "POST";
                myReq.ContentType = "application/json";
                string responseFromServer = "";
                PostTiktokApiRef postdata = new PostTiktokApiRef()
                {
                    app_key = eraAppKey,
                    app_secret = eraAppSecret,
                    refresh_token = refreshToken,
                    grant_type = "refresh_token"
                };
                var data = JsonConvert.SerializeObject(postdata);
                ////add 22 april 2021, handle spamming
                //var cekLog = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ACTION == "Refresh Token" && p.REQUEST_ATTRIBUTE_1 == cust
                //    && p.REQUEST_ATTRIBUTE_2 == refreshToken && p.REQUEST_STATUS == "Success").FirstOrDefault();
                //if (cekLog != null)
                //{
                //    ret = "data sudah ada";
                //}
                ////end add 22 april 2021, handle spamming
                //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                //{
                //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                //    REQUEST_ACTION = "Refresh Token",
                //    REQUEST_DATETIME = DateTime.Now,
                //    REQUEST_ATTRIBUTE_1 = cust,
                //    REQUEST_ATTRIBUTE_2 = refreshToken,
                //    REQUEST_STATUS = "Pending",
                //};
                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, cust, currentLog);
                myReq.ContentLength = data.Length;
                try
                {
                    using (var dataStream = myReq.GetRequestStream())
                    {
                        dataStream.Write(System.Text.Encoding.UTF8.GetBytes(data), 0, data.Length);
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
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST = '" + cust + "'");
                    //currentLog.REQUEST_EXCEPTION = ex.Message;
                    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, cust, currentLog);
                    return null;
                }
                try
                {
                    if (responseFromServer != null)
                    {
                        ret = "";
                        TiktokAuth tauth = JsonConvert.DeserializeObject<TiktokAuth>(responseFromServer);
                        var dateExpired = DateTimeOffset.FromUnixTimeSeconds(tauth.Data.AccessTokenExpireIn).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                        var tokendateExpired = DateTimeOffset.FromUnixTimeSeconds(tauth.Data.RefreshTokenExpireIn).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                        var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET TOKEN = '" + tauth.Data.AccessToken
                            + "', REFRESH_TOKEN = '" + tauth.Data.RefreshToken + "', STATUS_API = '1', TGL_EXPIRED = '" + tokendateExpired + "',TOKEN_EXPIRED = '"
                            + dateExpired + "' WHERE CUST = '" + cust + "'");
                        if (result == 1)
                        {
                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, cust, currentLog);
                            return null;
                        }
                        else
                        {
                            //currentLog.REQUEST_EXCEPTION = "failed to update token;execute result=" + result;
                            //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, cust, currentLog);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST = '" + cust + "'");
                    //currentLog.REQUEST_EXCEPTION = ex.Message;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, cust, currentLog);
                }
            }

            return null;
        }

        public TTApiData RefreshTokenTikTok(TTApiData iden)
        {
            SetupContext(iden.DatabasePathErasoft, iden.username);

            DateTime dateNow = DateTime.UtcNow.AddHours(7).AddMinutes(-30);
            bool ATExp = false;

            //if (ts.Days < 1 && ts.Hours < 24 && dateNow < tanggal_exptoken)
            if (dateNow > iden.expired_date)
            {
                var cekInDB = ErasoftDbContext.ARF01.Where(m => m.CUST == iden.no_cust).FirstOrDefault();
                if (cekInDB != null)
                {
                    //if (dataAPI.token != cekInDB.TOKEN)
                    //if (dataAPI.token_expired != cekInDB.TOKEN_EXPIRED)
                    if (iden.expired_date < cekInDB.TOKEN_EXPIRED)
                    {
                        iden.access_token = cekInDB.TOKEN;
                        iden.expired_date = cekInDB.TOKEN_EXPIRED.Value;
                        iden.refresh_token = cekInDB.REFRESH_TOKEN;

                        if (cekInDB.TOKEN_EXPIRED.Value.AddMinutes(-30) > DateTime.UtcNow.AddHours(7))
                        {
                            return iden;
                        }
                    }
                }
                ATExp = true;
            }

            if (ATExp)
            {
                string url;
                url = "https://auth.tiktok-shops.com/api/token/refreshToken";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
                myReq.Method = "POST";
                myReq.ContentType = "application/json";
                string responseFromServer = "";
                PostTiktokApiRef postdata = new PostTiktokApiRef()
                {
                    app_key = eraAppKey,
                    app_secret = eraAppSecret,
                    refresh_token = iden.refresh_token,
                    grant_type = "refresh_token"
                };
                var data = JsonConvert.SerializeObject(postdata);
                myReq.ContentLength = data.Length;
                try
                {
                    using (var dataStream = myReq.GetRequestStream())
                    {
                        dataStream.Write(System.Text.Encoding.UTF8.GetBytes(data), 0, data.Length);
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
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST = '" + iden.no_cust + "'");

                    return null;
                }
                try
                {
                    if (responseFromServer != "")
                    {
                        TiktokAuth tauth = JsonConvert.DeserializeObject<TiktokAuth>(responseFromServer);
                        var dateExpired = DateTimeOffset.FromUnixTimeSeconds(tauth.Data.AccessTokenExpireIn).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                        var tokendateExpired = DateTimeOffset.FromUnixTimeSeconds(tauth.Data.RefreshTokenExpireIn).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                        var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET TOKEN = '" + tauth.Data.AccessToken
                            + "', REFRESH_TOKEN = '" + tauth.Data.RefreshToken + "', STATUS_API = '1', TGL_EXPIRED = '" + tokendateExpired
                            + "',TOKEN_EXPIRED = '" + dateExpired + "' WHERE CUST = '" + iden.no_cust + "'");
                        if (result == 1)
                        {
                            iden.access_token = tauth.Data.AccessToken;
                            iden.refresh_token = tauth.Data.RefreshToken;
                            iden.expired_date = DateTimeOffset.FromUnixTimeSeconds(tauth.Data.AccessTokenExpireIn).UtcDateTime.AddHours(7);
                            //return null;
                        }
                        else
                        {
                        }
                    }
                }
                catch (Exception ex)
                {
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST = '" + iden.no_cust + "'");
                }
            }

            return iden;
        }
        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrder_GoLive_Insert_Tiktok(TTApiData iden, string CUST, string NAMA_CUST)
        {
            SetupContext(iden.DatabasePathErasoft, iden.username);
            iden = RefreshTokenTikTok(iden);
            var delQry = "delete a from sot01a a left join sot01b b on a.no_bukti = b.no_bukti where isnull(b.no_bukti, '') = '' and tgl >= '";
            delQry += DateTime.UtcNow.AddHours(7).AddHours(-12).ToString("yyyy-MM-dd HH:mm:ss") + "' and cust = '" + CUST + "'";

            //var resultDel = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, delQry);

            var fromDt = (long)DateTimeOffset.UtcNow.AddDays(-3).ToUnixTimeSeconds();
            var toDt = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            //var fromDt = (long)DateTimeOffset.UtcNow.AddDays(-5).ToUnixTimeSeconds();
            //var toDt = (long)DateTimeOffset.UtcNow.AddDays(-3).ToUnixTimeSeconds();

            var lanjut = true;
            var connIdProses = "";
            var nextPage = "";
            int ord_status = 100;
            while (lanjut)
            {
                var returnGetOrder = await GetOrderList_Insert(iden, ord_status, CUST, NAMA_CUST, nextPage, fromDt, toDt);
                if (returnGetOrder.ConnId != "")
                {
                    connIdProses += "'" + returnGetOrder.ConnId + "' , ";
                }
                //if (returnGetOrder.AdaKomponen)
                //{
                //    AdaKomponen = returnGetOrder.AdaKomponen;
                //}
                nextPage = returnGetOrder.nextPage;
                if (!returnGetOrder.more)
                {
                    if (ord_status == 100)
                    {
                        ord_status = 111;
                        nextPage = "";
                    }
                    else if (ord_status == 111)
                    {
                        ord_status = 112;
                        nextPage = "";
                    }
                    else
                    {
                        lanjut = false; break;
                    }
                }
            }
            if (!string.IsNullOrEmpty(connIdProses))
            {
                new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username, connIdProses.Substring(0, connIdProses.Length - 3));
            }


            //var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust
            //    + "%' and arguments like '%GetOrder_Insert_Tiktok%' and statename like '%Enque%'");

            return "";
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrder_Insert_Tiktok(TTApiData iden, string CUST, string NAMA_CUST)
        {
            SetupContext(iden.DatabasePathErasoft, iden.username);
            iden = RefreshTokenTikTok(iden);
            var delQry = "delete a from sot01a a left join sot01b b on a.no_bukti = b.no_bukti where isnull(b.no_bukti, '') = '' and tgl >= '";
            delQry += DateTime.UtcNow.AddHours(7).AddHours(-12).ToString("yyyy-MM-dd HH:mm:ss") + "' and cust = '" + CUST + "'";

            //var resultDel = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, delQry);

            var fromDt = (long)DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();
            var toDt = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            //var fromDt = (long)DateTimeOffset.UtcNow.AddDays(-5).ToUnixTimeSeconds();
            //var toDt = (long)DateTimeOffset.UtcNow.AddDays(-3).ToUnixTimeSeconds();

            var lanjut = true;
            var connIdProses = "";
            var nextPage = "";
            int ord_status = 100;
            while (lanjut)
            {
                var returnGetOrder = await GetOrderList_Insert(iden, ord_status, CUST, NAMA_CUST, nextPage, fromDt, toDt);
                if (returnGetOrder.ConnId != "")
                {
                    connIdProses += "'" + returnGetOrder.ConnId + "' , ";
                }
                //if (returnGetOrder.AdaKomponen)
                //{
                //    AdaKomponen = returnGetOrder.AdaKomponen;
                //}
                nextPage = returnGetOrder.nextPage;
                if (!returnGetOrder.more)
                {
                    if (ord_status == 100)
                    {
                        ord_status = 111;
                        nextPage = "";
                    }
                    else if (ord_status == 111)
                    {
                        ord_status = 112;
                        nextPage = "";
                    }
                    else
                    {
                        lanjut = false; break;
                    }
                }
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
            SetupContext(apidata.DatabasePathErasoft, apidata.username);
            var ret = new returnsGetOrder();
            string connId = Guid.NewGuid().ToString();
            string status = "";
            ret.ConnId = connId;
            var jmlhNewOrder = 0;
            string urll = "https://open-api.tiktokglobalshop.com/api/orders/search?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/orders/searchapp_key" + eraAppKey + "shop_id" + apidata.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, apidata.access_token, timestamp, signencry, eraAppKey, apidata.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "POST";
            myReq.ContentType = "application/json";

            string myData = "{\"create_time_from\": " + fromDt + ",\"create_time_to\": " + toDt + ",\"order_status\": " + order_status
                + ",\"sort_type\": 1,\"sort_by\": \"CREATE_TIME\",\"page_size\":10";
            //string myData = "{\"create_time_from\": " + fromDt + ",\"create_time_to\": " + toDt 
            //    + ",\"sort_type\": 1,\"sort_by\": \"CREATE_TIME\",\"page_size\":10";
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
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Length);
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

            }

            if (responseFromServer != "")
            {
                var listOrder = JsonConvert.DeserializeObject(responseFromServer, typeof(GetOrderListResponse)) as GetOrderListResponse;
                if (listOrder.data != null)
                    if (listOrder.data.order_list != null)
                    {
                        ret.more = listOrder.data.more;
                        ret.nextPage = listOrder.data.next_cursor;
                        if (listOrder.data.order_list.Length > 0)
                        {
                            string[] ordersn_list = listOrder.data.order_list.Select(p => p.order_id).ToArray();
                            var dariTgl = DateTimeOffset.FromUnixTimeSeconds(fromDt).UtcDateTime.AddHours(7).AddDays(-1);

                            var SudahAdaDiMO = ErasoftDbContext.SOT01A.Where(p => p.USER_NAME == "Auto TikTok" && p.CUST == CUST && p.TGL >= dariTgl).Select(p => p.NO_REFERENSI).ToList();

                            var filtered = ordersn_list.Where(p => !SudahAdaDiMO.Contains(p));
                            if (filtered.Count() > 0)
                            {
                                await GetOrderDetails(apidata, filtered.ToArray(), connId, CUST, NAMA_CUST, order_status);
                                jmlhNewOrder = filtered.Count();


                                new StokControllerJob().updateStockMarketPlace(connId, apidata.DatabasePathErasoft, apidata.username);
                            }

                            if (order_status != 100)//update paid
                            {
                                string ordersn = "";
                                var filteredSudahAda = ordersn_list.Where(p => SudahAdaDiMO.Contains(p));
                                foreach (var item in filteredSudahAda)
                                {
                                    ordersn = ordersn + "'" + item + "',";
                                }
                                if (!string.IsNullOrEmpty(ordersn))
                                {
                                    ordersn = ordersn.Substring(0, ordersn.Length - 1);
                                    var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '0' AND CUST = '" + CUST + "'");
                                    if (rowAffected > 0)
                                    {
                                        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                        contextNotif.Clients.Group(apidata.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(rowAffected) + " Pesanan terbayar dari TikTok.");

                                    }
                                }
                            }
                        }
                    }
            }

            return ret;
        }

        public async Task<string> GetOrderDetails(TTApiData iden, string[] ordersn_list, string connID, string CUST, string NAMA_CUST, int stat)
        {
            var ret = "";
            string urll = "https://open-api.tiktokglobalshop.com/api/orders/detail/query?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/orders/detail/queryapp_key" + eraAppKey + "shop_id" + iden.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, iden.access_token, timestamp, signencry, eraAppKey, iden.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "POST";
            myReq.ContentType = "application/json";

            GetOrderDetailsData HttpBody = new GetOrderDetailsData
            {
                order_id_list = ordersn_list
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string responseFromServer = "";
            try
            {
                myReq.ContentLength = System.Text.Encoding.UTF8.GetBytes(myData).Length;
                using (var dataStream = myReq.GetRequestStream())
                {
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Length);
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

            }

            if (responseFromServer != "")
            {
                //try
                //{
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(GetOrderDetailResponse)) as GetOrderDetailResponse;
                var connIdARF01C = Guid.NewGuid().ToString();
                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                TEMP_TIKTOK_ORDERS batchinsert = new TEMP_TIKTOK_ORDERS();
                List<TEMP_TIKTOK_ORDERS_ITEM> batchinsertItem = new List<TEMP_TIKTOK_ORDERS_ITEM>();
                string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                var kabKot = "3174";
                var prov = "31";
                string sqlVal = "";
                foreach (var order in result.data.order_list)
                {
                    //add by nurul 25/8/2021, handle pembeli d samarkan ***
                    //if (!order.recipient_address.name.Contains('*')) // tetap insert karena tidak bisa dapat unmask data kecuali di shipping label
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
                            DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss"),
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
                foreach (var order in result.data.order_list)
                {
                    try
                    {
                        //connID = Guid.NewGuid().ToString();//remark 4 jan 2020, connid sama per batch untuk update stok
                        ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_TIKTOK_ORDERS");
                        ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_TIKTOK_ORDERS_ITEM");
                        batchinsertItem = new List<TEMP_TIKTOK_ORDERS_ITEM>();
                        #region cut max length dan ubah '
                        string payment_method = !string.IsNullOrEmpty(order.payment_method) ? order.payment_method.Trim().Replace('\'', '`') : "";
                        if (payment_method.Length > 100)
                        {
                            payment_method = payment_method.Substring(0, 100);
                        }
                        string shipping_carrier = !string.IsNullOrEmpty(order.shipping_provider) ? order.shipping_provider.Trim().Replace('\'', '`') : "";
                        if (shipping_carrier.Length > 300)
                        {
                            shipping_carrier = shipping_carrier.Substring(0, 300);
                        }
                        //string currency = !string.IsNullOrEmpty(order.currency) ? order.currency.Trim().Replace('\'', '`') : "";
                        //if (currency.Length > 50)
                        //{
                        //    currency = currency.Substring(0, 50);
                        //}
                        string currency = "";
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
                        string Recipient_Address_country = !string.IsNullOrEmpty(order.recipient_address.region) ? order.recipient_address.region.Trim().Replace('\'', '`') : "";
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
                        string tracking_no = !string.IsNullOrEmpty(order.tracking_number) ? order.tracking_number.Trim().Replace('\'', '`') : "";
                        if (tracking_no.Length > 100)
                        {
                            tracking_no = tracking_no.Substring(0, 100);
                        }
                        string order_status = !string.IsNullOrEmpty(order.order_status.ToString()) ? order.order_status.ToString().Trim().Replace('\'', '`') : "";
                        if (order_status.Length > 100)
                        {
                            order_status = order_status.Substring(0, 100);
                        }
                        string service_code = !string.IsNullOrEmpty(order.shipping_provider_id) ? order.shipping_provider_id.Trim().Replace('\'', '`') : "";
                        if (service_code.Length > 100)
                        {
                            service_code = service_code.Substring(0, 100);
                        }
                        string ordersn = !string.IsNullOrEmpty(order.order_id) ? order.order_id.Trim().Replace('\'', '`') : "";
                        if (ordersn.Length > 100)
                        {
                            ordersn = ordersn.Substring(0, 100);
                        }
                        //string country = !string.IsNullOrEmpty(order.country) ? order.country.Trim().Replace('\'', '`') : "";
                        //if (country.Length > 100)
                        //{
                        //    country = country.Substring(0, 100);
                        //}
                        string country = "";
                        //string dropshipper = !string.IsNullOrEmpty(order.dropshipper) ? order.dropshipper.Trim().Replace('\'', '`') : "";
                        //if (dropshipper.Length > 300)
                        //{
                        //    dropshipper = dropshipper.Substring(0, 300);
                        //}
                        string dropshipper = "";
                        //string buyer_username = !string.IsNullOrEmpty(order.buyer_username) ? order.buyer_username.Trim().Replace('\'', '`') : "";
                        //if (buyer_username.Length > 300)
                        //{
                        //    buyer_username = buyer_username.Substring(0, 300);
                        //}
                        string buyer_username = "";
                        if (NAMA_CUST.Length > 50)
                        {
                            NAMA_CUST = NAMA_CUST.Substring(0, 50);
                        }
                        ////add by nurul 22/3/2021
                        //string checkout_shipping_carrier = !string.IsNullOrEmpty(order.checkout_shipping_carrier) ? order.checkout_shipping_carrier.Trim().Replace('\'', '`') : "";
                        //if (checkout_shipping_carrier.Length > 300)
                        //{
                        //    checkout_shipping_carrier = checkout_shipping_carrier.Substring(0, 300);
                        //}
                        ////end add by nurul 22/3/2021
                        string checkout_shipping_carrier = "";
                        #endregion
                        long paidTime = 0;
                        //if (order.paid_time == null)
                        {
                            paidTime = Convert.ToInt64(order.create_time);
                        }
                        //else
                        //{
                        //    paidTime = order.paid_time.Value;
                        //}
                        var newOrder = new TEMP_TIKTOK_ORDERS()
                        {
                            actual_shipping_cost = order.payment_info.original_shipping_fee.ToString(),
                            buyer_username = buyer_username,
                            cod = false,
                            country = country,
                            create_time = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(order.create_time)).UtcDateTime,
                            currency = currency,
                            days_to_ship = 0,
                            dropshipper = dropshipper,
                            escrow_amount = order.payment_info.total_amount.ToString(),
                            estimated_shipping_fee = order.payment_info.shipping_fee.ToString(),
                            goods_to_declare = false,
                            message_to_seller = (order.buyer_message ?? "").Replace('\'', '`'),
                            note = "",
                            note_update_time = DateTime.UtcNow.AddHours(7),
                            ordersn = ordersn,
                            order_status = order_status,
                            payment_method = payment_method,
                            //change by nurul 5/12/2019, local time 
                            //pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime,
                            pay_time = DateTimeOffset.FromUnixTimeMilliseconds(paidTime).UtcDateTime.AddHours(7),
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
                            total_amount = order.payment_info.total_amount.ToString(),
                            tracking_no = tracking_no,
                            update_time = DateTimeOffset.FromUnixTimeSeconds(order.update_time).UtcDateTime,
                            CONN_ID = connID,
                            CUST = CUST,
                            NAMA_CUST = NAMA_CUST,
                            //add by nurul 22/3/2021
                            checkout_shipping_carrier = checkout_shipping_carrier
                            //end add by nurul 22/3/2021
                        };
                        if (order.payment_method == "CASH_ON_DELIVERY")
                        {
                            newOrder.cod = true;
                        }
                        //add 27 okt 2020, expired shipping date
                        newOrder.ship_by_date = null;
                        if (order.cancel_order_sla > 0)
                        {
                            newOrder.ship_by_date = DateTimeOffset.FromUnixTimeSeconds(order.cancel_order_sla).UtcDateTime.AddHours(7);
                        }
                        //end add 27 okt 2020, expired shipping date
                        //var ShippingFeeData = await GetShippingFee(iden, order.ordersn);
                        //if (ShippingFeeData != null)
                        //{
                        //    newOrder.estimated_shipping_fee = (ShippingFeeData.order_income.buyer_paid_shipping_fee + ShippingFeeData.order_income.shopee_shipping_rebate - ShippingFeeData.order_income.actual_shipping_fee).ToString();
                        //    if (ShippingFeeData.order_income.buyer_paid_shipping_fee + ShippingFeeData.order_income.shopee_shipping_rebate - ShippingFeeData.order_income.actual_shipping_fee < 0)
                        //    {
                        //        newOrder.estimated_shipping_fee = "0";
                        //    }
                        //}
                        //newOrder.estimated_shipping_fee = (order.payment_info.original_shipping_fee - order.payment_info.shipping_fee_seller_discount).ToString();
                        //var listPromo = new Dictionary<long, double>();//add 6 juli 2020
                        var listPromo = new Dictionary<long, List<Activity>>();//add 6 juli 2020
                        foreach (var item in order.item_list)
                        {
                            string item_name = !string.IsNullOrEmpty(item.product_name) ? item.product_name.Replace('\'', '`') : "";
                            if (item_name.Length > 400)
                            {
                                item_name = item_name.Substring(0, 400);
                            }
                            string item_sku = !string.IsNullOrEmpty(item.seller_sku) ? item.seller_sku.Replace('\'', '`') : "";
                            if (item_sku.Length > 400)
                            {
                                item_sku = item_sku.Substring(0, 400);
                            }
                            string variation_name = !string.IsNullOrEmpty(item.sku_name) ? item.sku_name.Replace('\'', '`') : "";
                            if (variation_name.Length > 400)
                            {
                                variation_name = variation_name.Substring(0, 400);
                            }
                            //string variation_sku = !string.IsNullOrEmpty(item.variation_sku) ? item.variation_sku.Replace('\'', '`') : "";
                            //if (variation_sku.Length > 400)
                            //{
                            //    variation_sku = variation_sku.Substring(0, 400);
                            //}
                            string variation_sku = "";

                            var newOrderItem = new TEMP_TIKTOK_ORDERS_ITEM()
                            {
                                ordersn = ordersn,
                                is_wholesale = false,
                                item_id = Convert.ToInt64(item.product_id),
                                item_name = item_name,
                                item_sku = item_sku,
                                variation_discounted_price = item.sku_sale_price.ToString(),
                                variation_id = Convert.ToInt64(item.sku_id),
                                variation_name = variation_name,
                                variation_original_price = item.sku_original_price.ToString(),
                                variation_quantity_purchased = item.quantity,
                                variation_sku = variation_sku,
                                weight = 0,
                                pay_time = DateTimeOffset.FromUnixTimeMilliseconds(paidTime).UtcDateTime,
                                CONN_ID = connID,
                                CUST = CUST,
                                NAMA_CUST = NAMA_CUST
                            };

                            batchinsertItem.Add(newOrderItem);
                        }
                        batchinsert = (newOrder);

                        ErasoftDbContext.TEMP_TIKTOK_ORDERS.Add(batchinsert);
                        ErasoftDbContext.TEMP_TIKTOK_ORDERS_ITEM.AddRange(batchinsertItem);
                        ErasoftDbContext.SaveChanges();

                        //add 3 Des 2020
                        EDB.ExecuteSQL("Con", CommandType.Text, "DELETE FROM TEMP_TIKTOK_ORDERS_ITEM WHERE ordersn <> '" + ordersn + "'");
                        //end add 3 Des 2020
                        using (SqlCommand CommandSQL = new SqlCommand())
                        {
                            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connID;
                            CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.UtcNow.AddHours(7).AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                            CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                            CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@MARKET", SqlDbType.VarChar).Value = "TIKTOK";
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

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrderTiktok_webhook_Insert(TTApiData iden, string CUST, string NAMA_CUST)
        {
            string ret = "";
            SetupContext(iden.DatabasePathErasoft, iden.username);
            var daysNow = DateTime.UtcNow.AddHours(7).AddDays(-1);
            EDB.ExecuteSQL("CString", CommandType.Text, "DELETE FROM TABEL_WEBHOOK_TIKTOK WHERE CUST = '" + CUST
                + "' AND TGL <  '" + daysNow.AddDays(-2).ToString("yyyy-MM-dd HH:mm:ss") + "'");
            var dsNewOrder = EDB.GetDataSet("CString", "SO", "SELECT T.ORDERID FROM TABEL_WEBHOOK_TIKTOK (NOLOCK) T LEFT JOIN SOT01A (NOLOCK) S ON S.NO_REFERENSI = T.ORDERID AND T.CUST = S.CUST WHERE T.TGL >= '"
                + daysNow.ToString("yyyy-MM-dd HH:mm:ss") + "' AND ORDER_STATUS = 'UNPAID' AND T.CUST = '" + CUST + "' AND ISNULL(S.NO_BUKTI, '') = ''");

            var connIdProses = "";
            var ordersn_list = new List<string>();
            if (dsNewOrder.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsNewOrder.Tables[0].Rows.Count; i++)
                {
                    var insertData = dsNewOrder.Tables[0].Rows[i]["ORDERID"].ToString();
                    ordersn_list.Add(insertData);
                    if (ordersn_list.Count >= 10 || i == dsNewOrder.Tables[0].Rows.Count - 1)
                    {
                        string connId = Guid.NewGuid().ToString();

                        var returnGetOrder = await GetOrderDetails(iden, ordersn_list.ToArray(), connId, CUST, NAMA_CUST, 100);
                        new StokControllerJob().updateStockMarketPlace(connId, iden.DatabasePathErasoft, iden.username);
                        //if (!string.IsNullOrEmpty(returnGetOrder))
                        {
                            connIdProses += "'" + connId + "' , ";
                        }
                        ordersn_list = new List<string>();
                    }
                }
            }
            var dsNewOrderPaid = EDB.GetDataSet("CString", "SO", "SELECT T.ORDERID FROM TABEL_WEBHOOK_TOKPED (NOLOCK) T LEFT JOIN SOT01A (NOLOCK) S ON S.NO_REFERENSI = T.ORDERID AND T.CUST = S.CUST WHERE T.TGL >= '"
                + daysNow.ToString("yyyy-MM-dd HH:mm:ss") + "' AND ORDER_STATUS = 'AWAITING_SHIPMENT' AND T.CUST = '" + CUST + "' AND ISNULL(S.NO_BUKTI, '') = ''");

            if (dsNewOrderPaid.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsNewOrderPaid.Tables[0].Rows.Count; i++)
                {
                    var insertData = dsNewOrderPaid.Tables[0].Rows[i]["ORDERID"].ToString();
                    ordersn_list.Add(insertData);
                    if (ordersn_list.Count >= 10 || i == dsNewOrderPaid.Tables[0].Rows.Count - 1)
                    {
                        string connId = Guid.NewGuid().ToString();

                        var returnGetOrder = await GetOrderDetails(iden, ordersn_list.ToArray(), connId, CUST, NAMA_CUST, 111);
                        new StokControllerJob().updateStockMarketPlace(connId, iden.DatabasePathErasoft, iden.username);
                        //if (!string.IsNullOrEmpty(returnGetOrder))
                        {
                            connIdProses += "'" + connId + "' , ";
                        }
                        ordersn_list = new List<string>();
                    }
                }
            }

            if (!string.IsNullOrEmpty(connIdProses))
            {
                new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username, connIdProses.Substring(0, connIdProses.Length - 3));
            }


            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust
                 + "%' and arguments like '%GetOrderTiktok_webhook_Insert%' and statename like '%Enque%'");

            return ret;
        }
        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrderTiktok_webhook_Cancel(TTApiData iden, string CUST, string NAMA_CUST)
        {
            string ret = "";
            SetupContext(iden.DatabasePathErasoft, iden.username);
            var daysNow = DateTime.UtcNow.AddHours(7).AddDays(-1);
            EDB.ExecuteSQL("CString", CommandType.Text, "DELETE FROM TABEL_WEBHOOK_TIKTOK WHERE CUST = '" + CUST
                + "' AND TGL <  '" + daysNow.AddDays(-2).ToString("yyyy-MM-dd HH:mm:ss") + "'");
            var dsNewOrder = EDB.GetDataSet("CString", "SO", "SELECT T.ORDERID FROM TABEL_WEBHOOK_TIKTOK (NOLOCK) T LEFT JOIN SOT01A (NOLOCK) S ON S.NO_REFERENSI = T.ORDERID AND T.CUST = S.CUST WHERE T.TGL >= '"
                + daysNow.ToString("yyyy-MM-dd HH:mm:ss") + "' AND ORDER_STATUS = 'CANCEL' AND T.CUST = '" + CUST
                + "' AND ISNULL(S.NO_BUKTI, '') <> '' AND STATUS_TRANSAKSI not in ('11', '12')");

            var ordersn_list = new List<string>();
            if (dsNewOrder.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsNewOrder.Tables[0].Rows.Count; i++)
                {
                    var insertData = dsNewOrder.Tables[0].Rows[i]["ORDERID"].ToString();
                    ordersn_list.Add(insertData);
                    if (ordersn_list.Count >= 10 || i == dsNewOrder.Tables[0].Rows.Count - 1)
                    {
                        string connId = Guid.NewGuid().ToString();

                        var returnGetOrder = await GetOrderWebhook_Cancel(iden, CUST, NAMA_CUST, ordersn_list);

                        ordersn_list = new List<string>();
                    }
                }
            }



            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust
                 + "%' and arguments like '%GetOrderTiktok_webhook_Cancel%' and statename like '%Enque%'");

            return ret;
        }
        public async Task<returnsGetOrder> GetOrderWebhook_Cancel(TTApiData apidata, string CUST, string NAMA_CUST, List<string> ordersn_list)
        {
            SetupContext(apidata.DatabasePathErasoft, apidata.username);
            var ret = new returnsGetOrder();
            string connId = Guid.NewGuid().ToString();
            string status = "";
            ret.ConnId = connId;

            //string[] ordersn_list = listOrder.data.order_list.Select(p => p.order_id).ToArray();
            var dariTgl = DateTime.UtcNow.AddHours(7).AddDays(-14);

            var SudahAdaDiMO = ErasoftDbContext.SOT01A.Where(p => p.USER_NAME == "Auto TikTok" && p.CUST == CUST &&
            (p.STATUS_TRANSAKSI != "11" || p.STATUS_TRANSAKSI != "12") && p.TGL >= dariTgl).Select(p => p.NO_REFERENSI).ToList();

            var filtered = ordersn_list.Where(p => SudahAdaDiMO.Contains(p));
            if (filtered.Count() > 0)
            {
                string ordersn = "";
                foreach (var item in filtered)
                {
                    ordersn = ordersn + "'" + item + "',";
                }
                ordersn = ordersn.Substring(0, ordersn.Length - 1);
                var brgAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connId
                    + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + ordersn
                    //+ ") AND STATUS_TRANSAKSI  NOT IN ('11', '12') AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) <> 1");
                    + ") AND STATUS_TRANSAKSI  NOT IN ('11', '12') AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "'");

                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2',ORDER_CANCEL_DATE = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', STATUS_TRANSAKSI = '11', STATUS_KIRIM='5' WHERE NO_REFERENSI IN ("
                    //+ ordersn + ") AND STATUS_TRANSAKSI  NOT IN ('11', '12') AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) <> 1");
                    + ordersn + ") AND STATUS_TRANSAKSI  NOT IN ('11', '12') AND CUST = '" + CUST + "'");

                if (rowAffected > 0)
                {
                    await GetOrderDetailsForCancelReason(apidata, filtered.ToArray(), connId, CUST, NAMA_CUST);

                    string qry_Retur = "SELECT F.NO_REF FROM SIT01A (NOLOCK) F INNER JOIN SOT01A (NOLOCK) P ON P.NO_BUKTI = F.NO_SO AND F.JENIS_FORM = '2' ";
                    //qry_Retur += "WHERE P.NO_REFERENSI IN (" + ordersn + ") AND ISNULL(F.NO_FA_OUTLET, '-') LIKE '%-%' AND P.CUST = '" + CUST + "' AND ISNULL(P.TIPE_KIRIM,0) <> 1";
                    qry_Retur += "WHERE P.NO_REFERENSI IN (" + ordersn + ") AND ISNULL(F.NO_FA_OUTLET, '-') LIKE '%-%' AND P.CUST = '" + CUST + "'";
                    var dsFaktur = EDB.GetDataSet("MOConnectionString", "RETUR", qry_Retur);
                    if (dsFaktur.Tables[0].Rows.Count > 0)
                    {
                        var listFaktur = "";
                        for (int j = 0; j < dsFaktur.Tables[0].Rows.Count; j++)
                        {
                            listFaktur += "'" + dsFaktur.Tables[0].Rows[j]["NO_REF"].ToString() + "',";
                        }
                        listFaktur = listFaktur.Substring(0, listFaktur.Length - 1);
                        var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN (" + listFaktur + ") AND STATUS <> '2' AND ST_POSTING = 'T' AND CUST = '" + CUST + "'");
                    }

                }

                #region handle cancel COD
                //string qrycod = "SELECT P.NO_REFERENSI, ISNULL(F.NO_REF, '') NO_REF FROM SIT01A (NOLOCK) F RIGHT JOIN SOT01A (NOLOCK) P ON P.NO_BUKTI = F.NO_SO AND F.JENIS_FORM = '2' ";
                //qrycod += "WHERE P.NO_REFERENSI IN (" + ordersn + ") AND ISNULL(F.NO_FA_OUTLET, '-') LIKE '%-%' AND P.CUST = '"
                //    + CUST + "' AND ISNULL(P.TIPE_KIRIM,0) = 1 AND P.STATUS_TRANSAKSI NOT IN ('11', '12')";
                //var dsOrderCOD = EDB.GetDataSet("MOConnectionString", "COD", qrycod);
                //if (dsOrderCOD.Tables[0].Rows.Count > 0)
                //{
                //    var listNoRefCOD = new List<string>();
                //    for (int k = 0; k < dsOrderCOD.Tables[0].Rows.Count; k++)
                //    {
                //        listNoRefCOD.Add(dsOrderCOD.Tables[0].Rows[k]["NO_REFERENSI"].ToString());

                //    }
                //    if (listNoRefCOD.Count > 0)
                //    {
                //        var ordersDetail = await GetOrderDetailsForCancelReason(apidata, filtered.ToArray(), connId, CUST, NAMA_CUST);

                //        var fData = ordersDetail.Where(m => listNoRefCOD.Contains(m.order_id)).ToList();
                //        var listPesananCOD_11 = "";
                //        var listPesananCOD_12 = "";

                //        foreach (var ord in fData)
                //        {
                //            if (ord.rts_sla.Value > 0)//sudah kirim
                //            {
                //                listPesananCOD_12 += "'" + ord.order_id + "',";
                //            }
                //            else
                //            {
                //                listPesananCOD_11 += "'" + ord.order_id + "',";
                //            }
                //        }
                //        if (listPesananCOD_11 != "")//pesanan cod batal tapi belum di kirim
                //        {
                //            listPesananCOD_11 = listPesananCOD_11.Substring(0, listPesananCOD_11.Length - 1);
                //            EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connId
                //                    + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + listPesananCOD_11
                //                    + ") AND STATUS_TRANSAKSI <> '11' AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1 "
                //                    + "AND BRG NOT IN ( SELECT BRG FROM TEMP_ALL_MP_ORDER_ITEM (NOLOCK) WHERE CONN_ID = '" + connId + "')");

                //            var rowAffected_2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2',ORDER_CANCEL_DATE = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', STATUS_TRANSAKSI = '11', STATUS_KIRIM='5' WHERE NO_REFERENSI IN ("
                //                             + listPesananCOD_11 + ") AND STATUS_TRANSAKSI <> '11' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1");
                //            if (rowAffected_2 > 0)
                //            {
                //                var rowAffectedSI_2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN ("
                //                    + listPesananCOD_11 + ") AND STATUS <> '2' AND ST_POSTING = 'T' AND CUST = '" + CUST
                //                    + "' AND ISNULL(NO_FA_OUTLET, '-') LIKE '%-%' ");
                //                rowAffected += rowAffected_2;
                //            }

                //        }
                //        if (listPesananCOD_12 != "")//pesanan cod batal sudah di kirim
                //        {
                //            listPesananCOD_12 = listPesananCOD_12.Substring(0, listPesananCOD_12.Length - 1);
                //            EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connId
                //                    + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + listPesananCOD_12
                //                    + ") AND STATUS_TRANSAKSI <> '12' AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1 "
                //                    + "AND BRG NOT IN ( SELECT BRG FROM TEMP_ALL_MP_ORDER_ITEM (NOLOCK) WHERE CONN_ID = '" + connId + "')");

                //            var rowAffected_3 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '12',ORDER_CANCEL_DATE = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE NO_REFERENSI IN ("
                //                             + listPesananCOD_12 + ") AND STATUS_TRANSAKSI <> '12' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1");
                //            rowAffected += rowAffected_3;
                //        }
                //    }

                //}

                #endregion

                var sSQLInsertTempBundling = "INSERT INTO TEMP_ALL_MP_ORDER_ITEM_BUNDLING ([BRG],[CONN_ID],[TGL]) " +
                                             "SELECT DISTINCT C.UNIT AS BRG, '" + connId + "' AS CONN_ID, DATEADD(HOUR, +7, GETUTCDATE()) AS TGL " +
                                             "FROM TEMP_ALL_MP_ORDER_ITEM A(NOLOCK) " +
                                             "LEFT JOIN TEMP_ALL_MP_ORDER_ITEM_BUNDLING B(NOLOCK) ON B.CONN_ID = '" + connId + "' AND A.BRG = B.BRG " +
                                             "INNER JOIN STF03 C(NOLOCK) ON A.BRG = C.BRG " +
                                             "WHERE ISNULL(A.CONN_ID,'') = '" + connId + "' " +
                                             "AND ISNULL(B.BRG,'') = '' AND A.BRG <> 'NOT_FOUND'";
                var execInsertTempBundling = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, sSQLInsertTempBundling);
                new StokControllerJob().updateStockMarketPlace(connId, apidata.DatabasePathErasoft, apidata.username);

                if (rowAffected > 0)
                {
                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                    contextNotif.Clients.Group(apidata.DatabasePathErasoft).moNewOrder("" + Convert.ToString(rowAffected)
                        + " Pesanan dari TikTok dibatalkan.");
                }
            }



            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrder_Complete_Tiktok(TTApiData iden, string CUST, string NAMA_CUST)
        {
            SetupContext(iden.DatabasePathErasoft, iden.username);
            iden = RefreshTokenTikTok(iden);

            var fromDt = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            var toDt = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var lanjut = true;
            var connIdProses = "";
            var nextPage = "";
            int ord_status = 130;
            while (lanjut)
            {
                var returnGetOrder = await GetOrderList_Insert(iden, ord_status, CUST, NAMA_CUST, nextPage, fromDt, toDt);

                nextPage = returnGetOrder.nextPage;
                if (!returnGetOrder.more)
                {
                    {
                        lanjut = false; break;
                    }
                }
            }


            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust
                + "%' and arguments like '%GetOrder_Complete_Tiktok%' and statename like '%Enque%'");

            return "";
        }
        public async Task<returnsGetOrder> GetOrderList_Complete(TTApiData apidata, int order_status, string CUST, string NAMA_CUST, string page, long fromDt, long toDt)
        {
            SetupContext(apidata.DatabasePathErasoft, apidata.username);
            var ret = new returnsGetOrder();
            string connId = Guid.NewGuid().ToString();
            ret.ConnId = connId;
            string urll = "https://open-api.tiktokglobalshop.com/api/orders/search?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/orders/searchapp_key" + eraAppKey + "shop_id" + apidata.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, apidata.access_token, timestamp, signencry, eraAppKey, apidata.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "POST";
            myReq.ContentType = "application/json";

            string myData = "{\"create_time_from\": " + fromDt + ",\"create_time_to\": " + toDt + ",\"order_status\": " + order_status
                + ",\"sort_type\": 1,\"sort_by\": \"CREATE_TIME\",\"page_size\":10";
            //string myData = "{\"create_time_from\": " + fromDt + ",\"create_time_to\": " + toDt 
            //    + ",\"sort_type\": 1,\"sort_by\": \"CREATE_TIME\",\"page_size\":10";
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
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Length);
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

            }

            if (responseFromServer != "")
            {
                var listOrder = JsonConvert.DeserializeObject(responseFromServer, typeof(GetOrderListResponse)) as GetOrderListResponse;
                if (listOrder.data != null)
                    if (listOrder.data.order_list != null)
                    {
                        ret.more = listOrder.data.more;
                        ret.nextPage = listOrder.data.next_cursor;
                        if (listOrder.data.order_list.Length > 0)
                        {
                            string[] ordersn_list = listOrder.data.order_list.Select(p => p.order_id).ToArray();
                            string ordersn = "";
                            foreach (var item in ordersn_list)
                            {
                                ordersn = ordersn + "'" + item + "',";
                                ordersn = ordersn.Substring(0, ordersn.Length - 1);
                                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE CUST = '"
                                    + apidata.no_cust + "' AND NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '03'");
                                if (rowAffected > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(apidata.DatabasePathErasoft).moNewOrder("" + Convert.ToString(rowAffected)
                                        + " Pesanan dari TikTok sudah selesai.");

                                    var dateTimeNow = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd");
                                    string sSQLUpdateDatePesananSelesai = "UPDATE SIT01A SET TGL_KIRIM = '" + dateTimeNow + "' WHERE CUST = '" + apidata.no_cust
                                        + "' AND NO_REF IN (" + ordersn + ")";
                                    var resultUpdateDatePesanan = EDB.ExecuteSQL("CString", CommandType.Text, sSQLUpdateDatePesananSelesai);
                                }
                            }
                        }
                    }
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrder_Cancel_Tiktok(TTApiData iden, string CUST, string NAMA_CUST)
        {
            SetupContext(iden.DatabasePathErasoft, iden.username);
            iden = RefreshTokenTikTok(iden);

            var fromDt = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            var toDt = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            //var fromDt = (long)DateTimeOffset.UtcNow.AddDays(-5).ToUnixTimeSeconds();
            //var toDt = (long)DateTimeOffset.UtcNow.AddDays(-3).ToUnixTimeSeconds();

            var lanjut = true;
            var connIdProses = "";
            var nextPage = "";
            int ord_status = 140;
            while (lanjut)
            {
                var returnGetOrder = await GetOrderList_Cancel(iden, ord_status, CUST, NAMA_CUST, nextPage, fromDt, toDt);
                if (returnGetOrder.ConnId != "")
                {
                    connIdProses += "'" + returnGetOrder.ConnId + "' , ";
                }
                nextPage = returnGetOrder.nextPage;
                if (!returnGetOrder.more)
                {
                    {
                        lanjut = false; break;
                    }
                }
            }
            if (!string.IsNullOrEmpty(connIdProses))
            {
                new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username, connIdProses.Substring(0, connIdProses.Length - 3));
            }


            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust
                + "%' and arguments like '%GetOrder_Cancel_Tiktok%' and statename like '%Enque%'");

            return "";
        }
        public async Task<returnsGetOrder> GetOrderList_Cancel(TTApiData apidata, int order_status, string CUST, string NAMA_CUST, string page, long fromDt, long toDt)
        {
            SetupContext(apidata.DatabasePathErasoft, apidata.username);
            var ret = new returnsGetOrder();
            string connId = Guid.NewGuid().ToString();
            string status = "";
            ret.ConnId = connId;
            var jmlhNewOrder = 0;
            string urll = "https://open-api.tiktokglobalshop.com/api/orders/search?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/orders/searchapp_key" + eraAppKey + "shop_id" + apidata.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, apidata.access_token, timestamp, signencry, eraAppKey, apidata.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "POST";
            myReq.ContentType = "application/json";

            string myData = "{\"create_time_from\": " + fromDt + ",\"create_time_to\": " + toDt + ",\"order_status\": " + order_status
                + ",\"sort_type\": 1,\"sort_by\": \"CREATE_TIME\",\"page_size\":10";
            //string myData = "{\"create_time_from\": " + fromDt + ",\"create_time_to\": " + toDt 
            //    + ",\"sort_type\": 1,\"sort_by\": \"CREATE_TIME\",\"page_size\":10";
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
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Length);
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

            }

            if (responseFromServer != "")
            {
                var listOrder = JsonConvert.DeserializeObject(responseFromServer, typeof(GetOrderListResponse)) as GetOrderListResponse;
                if (listOrder.data != null)
                    if (listOrder.data.order_list != null)
                    {
                        ret.more = listOrder.data.more;
                        ret.nextPage = listOrder.data.next_cursor;
                        if (listOrder.data.order_list.Length > 0)
                        {
                            string[] ordersn_list = listOrder.data.order_list.Select(p => p.order_id).ToArray();
                            var dariTgl = DateTimeOffset.FromUnixTimeSeconds(fromDt).UtcDateTime.AddHours(7).AddDays(-1);

                            var SudahAdaDiMO = ErasoftDbContext.SOT01A.Where(p => p.USER_NAME == "Auto TikTok" && p.CUST == CUST &&
                            (p.STATUS_TRANSAKSI != "11" || p.STATUS_TRANSAKSI != "12") && p.TGL >= dariTgl).Select(p => p.NO_REFERENSI).ToList();

                            var filtered = ordersn_list.Where(p => SudahAdaDiMO.Contains(p));
                            if (filtered.Count() > 0)
                            {
                                string ordersn = "";
                                foreach (var item in filtered)
                                {
                                    ordersn = ordersn + "'" + item + "',";
                                }
                                ordersn = ordersn.Substring(0, ordersn.Length - 1);
                                var brgAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connId
                                    + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + ordersn
                                    //+ ") AND STATUS_TRANSAKSI  NOT IN ('11', '12') AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) <> 1");
                                    + ") AND STATUS_TRANSAKSI  NOT IN ('11', '12') AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "'");

                                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2',ORDER_CANCEL_DATE = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', STATUS_TRANSAKSI = '11', STATUS_KIRIM='5' WHERE NO_REFERENSI IN ("
                                    //+ ordersn + ") AND STATUS_TRANSAKSI  NOT IN ('11', '12') AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) <> 1");
                                    + ordersn + ") AND STATUS_TRANSAKSI  NOT IN ('11', '12') AND CUST = '" + CUST + "'");

                                if (rowAffected > 0)
                                {
                                    await GetOrderDetailsForCancelReason(apidata, filtered.ToArray(), connId, CUST, NAMA_CUST);

                                    string qry_Retur = "SELECT F.NO_REF FROM SIT01A (NOLOCK) F INNER JOIN SOT01A (NOLOCK) P ON P.NO_BUKTI = F.NO_SO AND F.JENIS_FORM = '2' ";
                                    //qry_Retur += "WHERE P.NO_REFERENSI IN (" + ordersn + ") AND ISNULL(F.NO_FA_OUTLET, '-') LIKE '%-%' AND P.CUST = '" + CUST + "' AND ISNULL(P.TIPE_KIRIM,0) <> 1";
                                    qry_Retur += "WHERE P.NO_REFERENSI IN (" + ordersn + ") AND ISNULL(F.NO_FA_OUTLET, '-') LIKE '%-%' AND P.CUST = '" + CUST + "'";
                                    var dsFaktur = EDB.GetDataSet("MOConnectionString", "RETUR", qry_Retur);
                                    if (dsFaktur.Tables[0].Rows.Count > 0)
                                    {
                                        var listFaktur = "";
                                        for (int j = 0; j < dsFaktur.Tables[0].Rows.Count; j++)
                                        {
                                            listFaktur += "'" + dsFaktur.Tables[0].Rows[j]["NO_REF"].ToString() + "',";
                                        }
                                        listFaktur = listFaktur.Substring(0, listFaktur.Length - 1);
                                        var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN (" + listFaktur + ") AND STATUS <> '2' AND ST_POSTING = 'T' AND CUST = '" + CUST + "'");
                                    }

                                }

                                #region handle cancel COD
                                //string qrycod = "SELECT P.NO_REFERENSI, ISNULL(F.NO_REF, '') NO_REF FROM SIT01A (NOLOCK) F RIGHT JOIN SOT01A (NOLOCK) P ON P.NO_BUKTI = F.NO_SO AND F.JENIS_FORM = '2' ";
                                //qrycod += "WHERE P.NO_REFERENSI IN (" + ordersn + ") AND ISNULL(F.NO_FA_OUTLET, '-') LIKE '%-%' AND P.CUST = '"
                                //    + CUST + "' AND ISNULL(P.TIPE_KIRIM,0) = 1 AND P.STATUS_TRANSAKSI NOT IN ('11', '12')";
                                //var dsOrderCOD = EDB.GetDataSet("MOConnectionString", "COD", qrycod);
                                //if (dsOrderCOD.Tables[0].Rows.Count > 0)
                                //{
                                //    var listNoRefCOD = new List<string>();
                                //    for (int k = 0; k < dsOrderCOD.Tables[0].Rows.Count; k++)
                                //    {
                                //        listNoRefCOD.Add(dsOrderCOD.Tables[0].Rows[k]["NO_REFERENSI"].ToString());

                                //    }
                                //    if (listNoRefCOD.Count > 0)
                                //    {
                                //        var ordersDetail = await GetOrderDetailsForCancelReason(apidata, filtered.ToArray(), connId, CUST, NAMA_CUST);

                                //        var fData = ordersDetail.Where(m => listNoRefCOD.Contains(m.order_id)).ToList();
                                //        var listPesananCOD_11 = "";
                                //        var listPesananCOD_12 = "";

                                //        foreach (var ord in fData)
                                //        {
                                //            if (ord.rts_sla.Value > 0)//sudah kirim
                                //            {
                                //                listPesananCOD_12 += "'" + ord.order_id + "',";
                                //            }
                                //            else
                                //            {
                                //                listPesananCOD_11 += "'" + ord.order_id + "',";
                                //            }
                                //        }
                                //        if (listPesananCOD_11 != "")//pesanan cod batal tapi belum di kirim
                                //        {
                                //            listPesananCOD_11 = listPesananCOD_11.Substring(0, listPesananCOD_11.Length - 1);
                                //            EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connId
                                //                    + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + listPesananCOD_11
                                //                    + ") AND STATUS_TRANSAKSI <> '11' AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1 "
                                //                    + "AND BRG NOT IN ( SELECT BRG FROM TEMP_ALL_MP_ORDER_ITEM (NOLOCK) WHERE CONN_ID = '" + connId + "')");

                                //            var rowAffected_2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2',ORDER_CANCEL_DATE = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', STATUS_TRANSAKSI = '11', STATUS_KIRIM='5' WHERE NO_REFERENSI IN ("
                                //                             + listPesananCOD_11 + ") AND STATUS_TRANSAKSI <> '11' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1");
                                //            if (rowAffected_2 > 0)
                                //            {
                                //                var rowAffectedSI_2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN ("
                                //                    + listPesananCOD_11 + ") AND STATUS <> '2' AND ST_POSTING = 'T' AND CUST = '" + CUST
                                //                    + "' AND ISNULL(NO_FA_OUTLET, '-') LIKE '%-%' ");
                                //                rowAffected += rowAffected_2;
                                //            }

                                //        }
                                //        if (listPesananCOD_12 != "")//pesanan cod batal sudah di kirim
                                //        {
                                //            listPesananCOD_12 = listPesananCOD_12.Substring(0, listPesananCOD_12.Length - 1);
                                //            EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connId
                                //                    + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + listPesananCOD_12
                                //                    + ") AND STATUS_TRANSAKSI <> '12' AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1 "
                                //                    + "AND BRG NOT IN ( SELECT BRG FROM TEMP_ALL_MP_ORDER_ITEM (NOLOCK) WHERE CONN_ID = '" + connId + "')");

                                //            var rowAffected_3 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '12',ORDER_CANCEL_DATE = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE NO_REFERENSI IN ("
                                //                             + listPesananCOD_12 + ") AND STATUS_TRANSAKSI <> '12' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1");
                                //            rowAffected += rowAffected_3;
                                //        }
                                //    }

                                //}

                                #endregion

                                var sSQLInsertTempBundling = "INSERT INTO TEMP_ALL_MP_ORDER_ITEM_BUNDLING ([BRG],[CONN_ID],[TGL]) " +
                                                             "SELECT DISTINCT C.UNIT AS BRG, '" + connId + "' AS CONN_ID, DATEADD(HOUR, +7, GETUTCDATE()) AS TGL " +
                                                             "FROM TEMP_ALL_MP_ORDER_ITEM A(NOLOCK) " +
                                                             "LEFT JOIN TEMP_ALL_MP_ORDER_ITEM_BUNDLING B(NOLOCK) ON B.CONN_ID = '" + connId + "' AND A.BRG = B.BRG " +
                                                             "INNER JOIN STF03 C(NOLOCK) ON A.BRG = C.BRG " +
                                                             "WHERE ISNULL(A.CONN_ID,'') = '" + connId + "' " +
                                                             "AND ISNULL(B.BRG,'') = '' AND A.BRG <> 'NOT_FOUND'";
                                var execInsertTempBundling = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, sSQLInsertTempBundling);
                                new StokControllerJob().updateStockMarketPlace(connId, apidata.DatabasePathErasoft, apidata.username);

                                if (rowAffected > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(apidata.DatabasePathErasoft).moNewOrder("" + Convert.ToString(rowAffected)
                                        + " Pesanan dari TikTok dibatalkan.");
                                }
                            }

                        }
                    }
            }

            return ret;
        }
        public async Task<List<OrderDetail_List>> GetOrderDetailsForCancelReason(TTApiData iden, string[] ordersn_list, string connID, string CUST, string NAMA_CUST)
        {
            var ret = new List<OrderDetail_List>();
            string urll = "https://open-api.tiktokglobalshop.com/api/orders/detail/query?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/orders/detail/queryapp_key" + eraAppKey + "shop_id" + iden.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, iden.access_token, timestamp, signencry, eraAppKey, iden.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "POST";
            myReq.ContentType = "application/json";

            GetOrderDetailsData HttpBody = new GetOrderDetailsData
            {
                order_id_list = ordersn_list
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string responseFromServer = "";
            try
            {
                myReq.ContentLength = System.Text.Encoding.UTF8.GetBytes(myData).Length;
                using (var dataStream = myReq.GetRequestStream())
                {
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Length);
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

            }

            if (responseFromServer != "")
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(GetOrderDetailResponse)) as GetOrderDetailResponse;
                if (result.data != null)
                {
                    var sSQL1 = "";
                    var sSQL2 = "SELECT * INTO #TEMP FROM (";
                    foreach (var order in result.data.order_list)
                    {
                        if (!string.IsNullOrEmpty(sSQL1))
                        {
                            sSQL1 += " UNION ALL ";
                        }
                        sSQL1 += " SELECT '" + order.order_id + "' NO_REFERENSI, '" + (order.cancel_reason ?? "") + "' ALASAN ";
                    }
                    sSQL2 += sSQL1 + ") as qry; INSERT INTO SOT01D (NO_BUKTI, CATATAN_1, USERNAME) ";
                    sSQL2 += " SELECT A.NO_BUKTI, ALASAN, 'AUTO_SHOPEE' FROM SOT01A A INNER JOIN #TEMP T ON A.NO_REFERENSI = T.NO_REFERENSI ";
                    sSQL2 += " LEFT JOIN SOT01D D ON A.NO_BUKTI = D.NO_BUKTI WHERE ISNULL(D.NO_BUKTI, '') = ''; DROP TABLE #TEMP";
                    EDB.ExecuteSQL("MOConnectionString", CommandType.Text, sSQL2);
                    ret = result.data.order_list.ToList();
                }

            }
            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Status Ready To Ship Pesanan {obj} ke TikTok Gagal.")]
        public string UpdateStatus_RTS(TTApiData iden, string ordersn, string no_bukti, string DeliveryProvider, string tracking_no)
        {
            SetupContext(iden.DatabasePathErasoft, iden.username);
            var ret = "";
            string urll = "https://open-api.tiktokglobalshop.com/api/order/rts?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/order/rtsapp_key" + eraAppKey + "shop_id" + iden.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, iden.access_token, timestamp, signencry, eraAppKey, iden.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "POST";
            myReq.ContentType = "application/json";


            string myData = "{\"order_id\":\"" + ordersn + "\"";
            if (!string.IsNullOrEmpty(DeliveryProvider))
            {
                myData += ", \"self_shipment\" : { \"tracking_number\" : " + tracking_no + "\"" + "\"shipping_provider_id\" : " + DeliveryProvider + "\"}";
            }
            myData += "}";
            string responseFromServer = "";
            try
            {
                myReq.ContentLength = System.Text.Encoding.UTF8.GetBytes(myData).Length;
                using (var dataStream = myReq.GetRequestStream())
                {
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Length);
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
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(TiktokCommonResponse)) as TiktokCommonResponse;
                if (result.code != 0)
                {
                    EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM='1' WHERE NO_BUKTI = '" + no_bukti + "'");
                    throw new Exception(responseFromServer);
                }
                else
                {

                    EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM='2' WHERE NO_BUKTI = '" + no_bukti + "'");
                }

            }
            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Status Ready To Ship Pesanan {obj} ke TikTok Gagal.")]
        public string GetShippingDoc(TTApiData iden, string ordersn)
        {
            SetupContext(iden.DatabasePathErasoft, iden.username);
            var ret = "";
            string urll = "https://open-api.tiktokglobalshop.com/api/logistics/shipping_document?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}&order_id={5}&document_type={6}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/logistics/shipping_documentapp_key" + eraAppKey + "document_typeSL_PLorder_id" + ordersn
                + "shop_id" + iden.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, iden.access_token, timestamp, signencry, eraAppKey, iden.shop_id, ordersn, "SL_PL");
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";


            //string myData = "{\"order_id\":\"" + ordersn + "\", \"document_type\" : \"SHIPPING_LABEL\", \"document_size\" : \"A6\"}";

            string responseFromServer = "";
            try
            {
                //myReq.ContentLength = System.Text.Encoding.UTF8.GetBytes(myData).Length;
                //using (var dataStream = myReq.GetRequestStream())
                //{
                //    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Length);
                //}
                using (WebResponse response = myReq.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException e)
            {
                string err = e.Message;
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                }
                return "error : " + err;
            }

            if (responseFromServer != "")
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(TiktokPrintLabelResponse)) as TiktokPrintLabelResponse;
                if (result.code != 0)
                {
                    //throw new Exception(responseFromServer);
                    return "error : " + responseFromServer;
                }
                else
                {
                    return result.data.doc_url;
                }

            }
            return ret;
        }

        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Tiktok gagal.")]
        public async Task<string> CreateProduct_tiktok(string dbPathEra, string kdbrgMO, string log_CUST, string log_ActionCategory, string log_ActionName, TTApiData iden)
        {
            SetupContext(iden.DatabasePathErasoft, iden.username);
            iden = RefreshTokenTikTok(iden);

            var ret = "";
            var brginDb = ErasoftDbContext.STF02.Where(m => m.BRG == kdbrgMO).FirstOrDefault();
            var tblCustomer = ErasoftDbContext.ARF01.Where(m => m.CUST == log_CUST).FirstOrDefault();
            var brg_stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == kdbrgMO && p.IDMARKET == tblCustomer.RecNum).SingleOrDefault();
            var postData = new CreateProductTiktok
            {
                images = new List<CreateImage>(),
                category_id = brg_stf02h.CATEGORY_CODE,
                package_height = Convert.ToInt32( brginDb.TINGGI),
                package_length = Convert.ToInt32(brginDb.PANJANG),
                package_width = Convert.ToInt32(brginDb.LEBAR),
                package_weight = (brginDb.BERAT/1000).ToString(),
                skus = new List<CreateSku>(),
                product_name = brginDb.NAMA,

            };
            string descBrg = brginDb.Deskripsi;
            if (!string.IsNullOrEmpty(brginDb.NAMA2))
            {
                postData.product_name += " " + brginDb.NAMA2;
            }
            if (!string.IsNullOrEmpty(brg_stf02h.ANAME_38))
            {
                postData.brand_id = brg_stf02h.ANAME_38;
            }
            if (!string.IsNullOrEmpty(brg_stf02h.AVALUE_31))
            {
                postData.warranty_period = Convert.ToInt32(brg_stf02h.AVALUE_31);
            }
            if (!string.IsNullOrEmpty(brg_stf02h.AVALUE_32))
            {
                postData.warranty_policy = brg_stf02h.AVALUE_32;
            }
            if (!string.IsNullOrEmpty(brg_stf02h.AVALUE_39))
            {
                postData.is_cod_open = (brg_stf02h.AVALUE_39 == "1" ? true : false);
            }
            if (!string.IsNullOrEmpty(brg_stf02h.NAMA_BARANG_MP))
            {
                postData.product_name = brg_stf02h.NAMA_BARANG_MP;
            }
            if (!string.IsNullOrEmpty(brg_stf02h.DESKRIPSI_MP))
            {
                descBrg = brg_stf02h.DESKRIPSI_MP;
            }
            descBrg = System.Net.WebUtility.HtmlDecode(descBrg).Replace("&nbsp;", " ");
            postData.description = descBrg;

            #region gambar induk
            var img_induk = new CreateImage();
            if (!string.IsNullOrEmpty(brginDb.LINK_GAMBAR_1))
            {
                img_induk.id = await UpladImage(iden, brginDb.LINK_GAMBAR_1, "1");
                postData.images.Add(img_induk);
            }
            if (!string.IsNullOrEmpty(brginDb.LINK_GAMBAR_2))
            {
                img_induk.id = await UpladImage(iden, brginDb.LINK_GAMBAR_2, "1");
                postData.images.Add(img_induk);
            }
            if (!string.IsNullOrEmpty(brginDb.LINK_GAMBAR_3))
            {
                img_induk.id = await UpladImage(iden, brginDb.LINK_GAMBAR_3, "1");
                postData.images.Add(img_induk);
            }
            if (!string.IsNullOrEmpty(brginDb.LINK_GAMBAR_4))
            {
                img_induk.id = await UpladImage(iden, brginDb.LINK_GAMBAR_4, "1");
                postData.images.Add(img_induk);
            }
            if (!string.IsNullOrEmpty(brginDb.LINK_GAMBAR_5))
            {
                img_induk.id = await UpladImage(iden, brginDb.LINK_GAMBAR_5, "1");
                postData.images.Add(img_induk);
            }
            #endregion
            if (brginDb.TYPE == "4")
            {

            }
            else
            {
                var itemskus = new CreateSku()
                {
                    original_price = brg_stf02h.HJUAL.ToString(),
                    seller_sku = kdbrgMO,
                    sales_attributes = new List<CreateSales_Attributes>(),
                    stock_infos = new List<CreateStock_Infos>()
                };
                var stockInfo = new CreateStock_Infos();
                stockInfo.warehouse_id = brg_stf02h.PICKUP_POINT;
                stockInfo.available_stock = 0;
                var qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(kdbrgMO, "ALL");
                if (qty_stock > 0)
                {
                    stockInfo.available_stock = Convert.ToInt32(qty_stock);
                }
                itemskus.stock_infos.Add(stockInfo);
                var sales_attr = new CreateSales_Attributes();
                if (!string.IsNullOrEmpty(brg_stf02h.AVALUE_1))
                {
                    sales_attr.custom_value = brg_stf02h.AVALUE_1;
                    sales_attr.attribute_id = brg_stf02h.ACODE_1;
                    if (!string.IsNullOrEmpty(brg_stf02h.AVALUE_50))
                    {
                        sales_attr.sku_img = new Sku_Img()
                        {
                            id = await UpladImage(iden, brg_stf02h.AVALUE_50, "3"),
                        };
                    }
                    itemskus.sales_attributes.Add(sales_attr);
                }
                if (!string.IsNullOrEmpty(brg_stf02h.AVALUE_2))
                {
                    sales_attr = new CreateSales_Attributes();
                    sales_attr.custom_value = brg_stf02h.AVALUE_2;
                    sales_attr.attribute_id = brg_stf02h.ACODE_2;
                    if (string.IsNullOrEmpty(brg_stf02h.AVALUE_1))
                    {
                        if (!string.IsNullOrEmpty(brg_stf02h.AVALUE_50))
                        {
                            sales_attr.sku_img = new Sku_Img()
                            {
                                id = await UpladImage(iden, brg_stf02h.AVALUE_50, "3"),
                            };
                        }
                    }
                    itemskus.sales_attributes.Add(sales_attr);
                };
                postData.skus.Add(itemskus);
            };

            string urll = "https://open-api.tiktokglobalshop.com/api/products?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/productsapp_key" + eraAppKey + "shop_id" + iden.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, iden.access_token, timestamp, signencry, eraAppKey, iden.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "POST";
            myReq.ContentType = "application/json";

            string myData = JsonConvert.SerializeObject(postData);

            //string myData = "{\"product_id\":\"" + brgmp[0] + "\", \"skus\" : [{ \"original_price\":\"" + price + "\",\"id\" : \"" + brgmp[1] + "\" }] }";

            string responseFromServer = "";
            try
            {
                myReq.ContentLength = System.Text.Encoding.UTF8.GetBytes(myData).Length;
                using (var dataStream = myReq.GetRequestStream())
                {
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Length);
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
            catch (WebException e)
            {
                string err = e.Message;
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                }
                return "error : " + err;
            }

            if (responseFromServer != "")
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(TiktokCommonResponse)) as TiktokCommonResponse;
                if (result.code != 0)
                {
                    throw new Exception(responseFromServer);
                    //return "error : " + responseFromServer;
                }


            }
            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Harga Jual Produk {obj} ke Tiktok gagal.")]
        public string UpdatePrice_job(string dbPathEra, string kdbrgMO, string log_CUST, string log_ActionCategory, string log_ActionName, string product_id, TTApiData iden, string price)
        {
            SetupContext(iden.DatabasePathErasoft, iden.username);
            var brgmp = product_id.Split(';');
            if (brgmp.Length != 2)
            {
                throw new Exception("Update harga gagal, Link barang salah.");
            }
            var ret = "";
            string urll = "https://open-api.tiktokglobalshop.com/api/products/prices?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/products/pricesapp_key" + eraAppKey + "shop_id" + iden.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, iden.access_token, timestamp, signencry, eraAppKey, iden.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "PUT";
            myReq.ContentType = "application/json";


            string myData = "{\"product_id\":\"" + brgmp[0] + "\", \"skus\" : [{ \"original_price\":\"" + price + "\",\"id\" : \"" + brgmp[1] + "\" }] }";

            string responseFromServer = "";
            try
            {
                myReq.ContentLength = System.Text.Encoding.UTF8.GetBytes(myData).Length;
                using (var dataStream = myReq.GetRequestStream())
                {
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Length);
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
            catch (WebException e)
            {
                string err = e.Message;
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                }
                return "error : " + err;
            }

            if (responseFromServer != "")
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(TiktokCommonResponse)) as TiktokCommonResponse;
                if (result.code != 0)
                {
                    throw new Exception(responseFromServer);
                    //return "error : " + responseFromServer;
                }


            }
            return ret;
        }

        public ATTRIBUTE_SHOPEE_AND_OPT_v2 GetAttributeList(TTApiData iden, string categoryCode)
        {
            SetupContext(iden.DatabasePathErasoft, iden.username);
            var ret = new ATTRIBUTE_TIKTOK_AND_OPT_v2();
            var katInDB = ErasoftDbContext.CATEGORY_TIKTOK.Where(k => k.CATEGORY_CODE == categoryCode && k.CUST == iden.no_cust).FirstOrDefault();
            if(katInDB != null)
            {
                ret.cod = katInDB.COD ?? "";
                ret.size_chart = katInDB.SIZE_CHART ?? "";
            }
            string urll = "https://open-api.tiktokglobalshop.com/api/products/attributes?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}&category_id={5}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/products/attributesapp_key" + eraAppKey + "category_id" + categoryCode
                + "shop_id" + iden.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, iden.access_token, timestamp, signencry, eraAppKey, iden.shop_id, categoryCode);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
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
            catch (WebException e)
            {
                string err = e.Message;
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                }
            }

            if (responseFromServer != "")
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(TiktokAttributeResponse)) as TiktokAttributeResponse;
                if (result.code == 0)
                {
                    ATTRIBUTE_SHOPEE_V2 returnData = new ATTRIBUTE_SHOPEE_V2();
                    string a = "";
                    int i = 0;
                    if (result.data != null)
                        foreach (var attribs in result.data.attributes)
                        {
                            if (attribs.attribute_type.ToString() == "2")
                            {
                                a = Convert.ToString(i + 1);

                                returnData["ACODE_" + a] = attribs.id;
                                returnData["ATYPE_" + a] = attribs.attribute_type.ToString();
                                returnData["ANAME_" + a] = attribs.name;
                                returnData["AOPTIONS_" + a] = "0";
                                returnData["AMANDATORY_" + a] = attribs.input_type.is_mandatory ? "1" : "0";
                                returnData["AUNIT_" + a] = attribs.input_type.is_multiple_selected ? "MULTI" : "";
                                if (attribs.input_type.is_customized)
                                {
                                    returnData["AUNIT_" + a] += "CUSTOM";
                                }
                                if (attribs.values != null)
                                {
                                    if (attribs.values.Count() > 0)
                                    {
                                        returnData["AOPTIONS_" + a] = "1";
                                        var optList = attribs.values.ToList();
                                        var listOpt = optList.Select(x => new ATTRIBUTE_OPT_SHOPEE_V2(attribs.id, x.id, x.name)).ToList();
                                        ret.attribute_opts_v2.AddRange(listOpt);
                                    }
                                }
                                i = i + 1;
                            }
                        }
                    ret.attributes.Add(returnData);
                }

            }
            return ret;
        }

        public async Task<string> UpladImage(TTApiData iden, string url, string type)
        {
            SetupContext(iden.DatabasePathErasoft, iden.username);
            var ret = "";
            string urll = "https://open-api.tiktokglobalshop.com/api/products/upload_imgs?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/products/upload_imgsapp_key" + eraAppKey + "shop_id" + iden.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, iden.access_token, timestamp, signencry, eraAppKey, iden.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "POST";
            myReq.ContentType = "application/json";

            #region url image into base 64
            var base64String = "";
            using (var client = new HttpClient())
            {
                var bytes = await client.GetByteArrayAsync(url);
                base64String = Convert.ToBase64String(bytes);
            }
            #endregion
            string myData = "{\"img_data\":\"" + base64String + "\", \"img_scene\" : " + type;
            //1:"PRODUCT_IMAGE" , 2:"DESCRIPTION_IMAGE" , 3:"ATTRIBUTE_IMAGE " , 4:"CERTIFICATION_IMAGE" , 5:"SIZE_CHART_IMAGE"
            myData += "}";
            string responseFromServer = "";
            try
            {
                myReq.ContentLength = System.Text.Encoding.UTF8.GetBytes(myData).Length;
                using (var dataStream = myReq.GetRequestStream())
                {
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Length);
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
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(TiktokUploadImageResponse)) as TiktokUploadImageResponse;
                if (result.code == 0)
                {
                    ret = result.data.img_id;
                }

            }
            return ret;
        }
        #region Encyrptor
        public static String GetHash(String text, String key)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();

            Byte[] textBytes = encoding.GetBytes(text);
            Byte[] keyBytes = encoding.GetBytes(key);

            Byte[] hashBytes;

            using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        public static string EncryptString(string input, Encoding encoding)
        {
            Byte[] stringBytes = encoding.GetBytes(input);
            StringBuilder sbBytes = new StringBuilder(stringBytes.Length * 2);
            foreach (byte b in stringBytes)
            {
                sbBytes.AppendFormat("{0:X2}", b);
            }
            return sbBytes.ToString();
        }

        public static string DecryptString(string hexInput, Encoding encoding)
        {
            int numberChars = hexInput.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexInput.Substring(i, 2), 16);
            }
            return encoding.GetString(bytes);
        }
        #endregion

    }
}

public class CreateProductTiktok
{
    public string product_name { get; set; }
    public string description { get; set; }
    public string category_id { get; set; }
    public string brand_id { get; set; }
    public List<CreateImage> images { get; set; }
    public int warranty_period { get; set; }
    public string warranty_policy { get; set; }
    public int package_length { get; set; }
    public int package_width { get; set; }
    public int package_height { get; set; }
    public string package_weight { get; set; }
    //public CreateProduct_Certifications[] product_certifications { get; set; }
    public bool is_cod_open { get; set; }
    public List<CreateSku> skus { get; set; }
}

public class CreateImage
{
    public string id { get; set; }
}

//public class CreateProduct_Certifications
//{
//    public string id { get; set; }
//    public File[] files { get; set; }
//    public Image1[] images { get; set; }
//}

//public class File
//{
//    public string id { get; set; }
//    public string name { get; set; }
//    public string type { get; set; }
//}

//public class Image1
//{
//    public string id { get; set; }
//}

public class CreateSku
{
    public List<CreateSales_Attributes> sales_attributes { get; set; }
    public List<CreateStock_Infos> stock_infos { get; set; }
    public string seller_sku { get; set; }
    public string original_price { get; set; }
}

public class CreateSales_Attributes
{
    public string attribute_id { get; set; }
    public string custom_value { get; set; }
    public Sku_Img sku_img { get; set; }
}

public class Sku_Img
{
    public string id { get; set; }
}

public class CreateStock_Infos
{
    public string warehouse_id { get; set; }
    public int available_stock { get; set; }
}

public class TiktokUploadImageResponse : TiktokCommonResponse
{
    public TiktokUploadImageData data { get; set; }
}
public class TiktokUploadImageData
{
    public string img_id { get; set; }

}

public class TiktokPrintLabelResponse : TiktokCommonResponse
{
    public TiktokPrintLabelData data { get; set; }
}
public class TiktokPrintLabelData
{
    public string doc_url { get; set; }

}
public class TiktokCommonResponse
{
    public int code { get; set; }
    public string message { get; set; }
    public string request_id { get; set; }
}

public class TiktokAttributeResponse : TiktokCommonResponse
{
    public TiktokAttributeData data { get; set; }
}

public class TiktokAttributeData
{
    public TiktokAttribute[] attributes { get; set; }
}

public class TiktokAttribute
{
    public string id { get; set; }
    public string name { get; set; }
    public int attribute_type { get; set; }
    public TiktokInput_Type input_type { get; set; }
    public List<TiktokValues> values { get; set; }
}

public class TiktokInput_Type
{
    public bool is_mandatory { get; set; }
    public bool is_multiple_selected { get; set; }
    public bool is_customized { get; set; }
}
public class TiktokValues
{
    public string id { get; set; }
    public string name { get; set; }
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
    public long? paid_time { get; set; }
    public string buyer_message { get; set; }
    public Payment_Info payment_info { get; set; }
    public Recipient_Address recipient_address { get; set; }
    public string tracking_number { get; set; }
    public Item_List[] item_list { get; set; }
    public long? rts_time { get; set; }
    public long? rts_sla { get; set; }
    public long? tts_sla { get; set; }
    public long cancel_order_sla { get; set; }
    public long receiver_address_updated { get; set; }
    public long update_time { get; set; }
    public string cancel_reason { get; set; }
    public string cancel_user { get; set; }
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
