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

namespace MasterOnline.Controllers
{
    public class TiktokController : Controller
    {
        //AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        List<string> listSku = new List<string>();
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
        TTApiData apidata;

        public TiktokController()
        {
            MoDbContext = new MoDbContext("");
            username = "";

            var sessionAccount = System.Web.HttpContext.Current.Session["SessionAccount"];
            var sessionAccountUserID = System.Web.HttpContext.Current.Session["SessionAccountUserID"];
            var sessionAccountUserName = System.Web.HttpContext.Current.Session["SessionAccountUserName"];
            var sessionAccountDataSourcePathDebug = System.Web.HttpContext.Current.Session["SessionAccountDataSourcePathDebug"];
            var sessionAccountDataSourcePath = System.Web.HttpContext.Current.Session["SessionAccountDataSourcePath"];
            var sessionAccountDatabasePathErasoft = System.Web.HttpContext.Current.Session["SessionAccountDatabasePathErasoft"];

            var sessionUser = System.Web.HttpContext.Current.Session["SessionUser"];
            var sessionUserAccountID = System.Web.HttpContext.Current.Session["SessionUserAccountID"];
            var sessionUserUsername = System.Web.HttpContext.Current.Session["SessionUserUsername"];

            if (sessionAccount != null)
            {
                if (sessionAccountUserID.ToString() == "admin_manage")
                {
                    ErasoftDbContext = new ErasoftContext();
                }
                else
                {
#if (Debug_AWS || DEBUG)
                    dbSourceEra = sessionAccountDataSourcePathDebug.ToString();
#else
                    dbSourceEra = sessionAccountDataSourcePath.ToString();
#endif
                    ErasoftDbContext = new ErasoftContext(dbSourceEra, sessionAccountDatabasePathErasoft.ToString());
                }
                EDB = new DatabaseSQL(sessionAccountDatabasePathErasoft.ToString());
                DatabasePathErasoft = sessionAccountDatabasePathErasoft.ToString();
                username = sessionAccountUserName.ToString();
            }
            else
            {
                if (sessionUser != null)
                {
                    var userAccID = Convert.ToInt64(sessionUserAccountID);
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == userAccID);
#if (Debug_AWS || DEBUG)
                    dbSourceEra = accFromUser.DataSourcePathDebug;
#else
                    dbSourceEra = accFromUser.DataSourcePath;
#endif
                    ErasoftDbContext = new ErasoftContext(dbSourceEra, accFromUser.DatabasePathErasoft);
                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                    DatabasePathErasoft = accFromUser.DatabasePathErasoft;
                    username = accFromUser.Username;
                }
            }

            if (username.Length > 20)
                username = username.Substring(0, 17) + "...";
        }


        protected void SetupContext(string DatabasePathErasoft, string uname, TTApiData apidata)
        {
            //string ret = "";
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, DatabasePathErasoft);
            username = uname;
            this.apidata = apidata;
        }

        #region Authentication
        [Route("tiktok/auth")]
        [HttpGet]
        public ActionResult TiktokCode(string code, string state)
        {
            var decrypt = DecryptString(state, System.Text.Encoding.Unicode);
            var param = decrypt.Split(new string[] { "_param_" }, StringSplitOptions.None);
            if (param.Count() == 2)
            {
                DatabaseSQL EDB = new DatabaseSQL(param[0]);
                var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET API_KEY = '" + code + "' WHERE CUST = '" + param[1] + "'");
                GetToken(param[0], param[1], code);
            }
            return View("Tiktokauth");
        }

        [HttpGet]
        public string TiktokUrl(string cust)
        {
            string userId = "";
            var sessionAccount = System.Web.HttpContext.Current.Session["SessionAccount"];
            var sessionAccountUserID = System.Web.HttpContext.Current.Session["SessionAccountUserID"];
            var sessionAccountUserName = System.Web.HttpContext.Current.Session["SessionAccountUserName"];
            var sessionAccountDataSourcePathDebug = System.Web.HttpContext.Current.Session["SessionAccountDataSourcePathDebug"];
            var sessionAccountDataSourcePath = System.Web.HttpContext.Current.Session["SessionAccountDataSourcePath"];
            var sessionAccountDatabasePathErasoft = System.Web.HttpContext.Current.Session["SessionAccountDatabasePathErasoft"];

            var sessionUser = System.Web.HttpContext.Current.Session["SessionUser"];
            var sessionUserAccountID = System.Web.HttpContext.Current.Session["SessionUserAccountID"];
            var sessionUserUsername = System.Web.HttpContext.Current.Session["SessionUserUsername"];

            if (sessionAccount != null)
            {
                userId = sessionAccountDatabasePathErasoft.ToString();

            }
            else
            {
                if (sessionUser != null)
                {
                    var userAccID = Convert.ToInt64(sessionUserAccountID);
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == userAccID);
                    userId = accFromUser.DatabasePathErasoft;
                }
            }

            string tikId = cust;
            string compUrl = userId + "_param_" + tikId;
            string sha256encryp = EncryptString(compUrl, System.Text.Encoding.Unicode);
            string uri = "https://auth.tiktok-shops.com/oauth/authorize?app_key=" + eraAppKey + "&state=" + sha256encryp;
            return uri;
        }



        public string GetToken(string user, string cust, string authcode)
        {
            string ret;
            string url;
            url = "https://auth.tiktok-shops.com/api/token/getAccessToken";
            MoDbContext = new MoDbContext("");

            DatabaseSQL EDB = new DatabaseSQL(user);
            string EraServerName = EDB.GetServerName("sConn");
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
            myReq.Method = "POST";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            PostTiktokApi postdata = new PostTiktokApi()
            {
                app_key = eraAppKey,
                app_secret = eraAppSecret,
                auth_code = authcode,
                grant_type = "authorized_code"
            };
            var data = JsonConvert.SerializeObject(postdata);
            ErasoftDbContext = new ErasoftContext(EraServerName, user);
            //add 22 april 2021, handle spamming
            var cekLog = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ACTION == "Get Token" && p.REQUEST_ATTRIBUTE_1 == cust
                && p.REQUEST_ATTRIBUTE_2 == authcode && p.REQUEST_STATUS == "Success").FirstOrDefault();
            if (cekLog != null)
            {
                ret = "data sudah ada";
                return ret;
            }
            //end add 22 april 2021, handle spamming
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Token",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = cust,
                REQUEST_ATTRIBUTE_2 = authcode,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, cust, currentLog);
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
                var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST = '" + cust + "'");
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, cust, currentLog);
                return ex.ToString();
            }
            try
            {
                if (responseFromServer != null)
                {
                    ret = "";
                    TiktokAuth tauth = JsonConvert.DeserializeObject<TiktokAuth>(responseFromServer);
                    string shopid = getShopId(tauth.Data.AccessToken);
                    var dateExpired = DateTimeOffset.FromUnixTimeSeconds(tauth.Data.AccessTokenExpireIn).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                    var tokendateExpired = DateTimeOffset.FromUnixTimeSeconds(tauth.Data.RefreshTokenExpireIn).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET TOKEN = '" + tauth.Data.AccessToken
                        + "', REFRESH_TOKEN = '" + tauth.Data.RefreshToken + "', STATUS_API = '1', TGL_EXPIRED = '" + tokendateExpired
                        + "',TOKEN_EXPIRED = '" + dateExpired + "' , SORT1_CUST = '" + shopid + "' WHERE CUST = '" + cust + "'");
                    //MoDbContext.Database.ExecuteSqlCommand("INSERT INTO [TABEL_MAPPING_TIKTOK] (dbpathera, shopid,cust) values ('"+ user + "', '"+ shopid + "', '" + cust + "') ");
                    var tblMapping = MoDbContext.TABEL_MAPPING_TIKTOK.Where(m => m.DBPATHERA == user && m.CUST == cust).FirstOrDefault();
                    if (tblMapping != null)
                    {
                        tblMapping.SHOPID = shopid;
                    }
                    else
                    {
                        tblMapping = new TABEL_MAPPING_TIKTOK
                        {
                            CUST = cust,
                            SHOPID = shopid,
                            DBPATHERA = user
                        };
                        MoDbContext.TABEL_MAPPING_TIKTOK.Add(tblMapping);
                    }
                    MoDbContext.SaveChanges();
                    if (result == 1)
                    {
                        //GetShippingProvider(cust);
                        var tblCustomer = ErasoftDbContext.ARF01.Where(p => p.CUST == cust).FirstOrDefault();
                        var idenTikTok = new TTApiData
                        {
                            access_token = tblCustomer.TOKEN,
                            no_cust = tblCustomer.CUST,
                            DatabasePathErasoft = user,
                            shop_id = tblCustomer.Sort1_Cust,
                            username = "",
                            expired_date = tblCustomer.TOKEN_EXPIRED.Value,
                            refresh_token = tblCustomer.REFRESH_TOKEN
                        };
                        getCategory(idenTikTok);
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, cust, currentLog);
                        return ret;
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = "failed to update token;execute result=" + responseFromServer;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, cust, currentLog);
                    }
                }
            }
            catch (Exception ex)
            {
                var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST = '" + cust + "'");
                currentLog.REQUEST_EXCEPTION = responseFromServer + ";" + ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, cust, currentLog);
            }
            return null;

        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("2_get_token")]
        public async Task<TiktokAuth> GetRefToken(string cust, string refreshToken, string dbpath, string username, DateTime? tanggal_exptoken, DateTime? tanggal_exprtok)
        {
            SetupContext(dbpath, username, null);
            DateTime dateNow = DateTime.UtcNow.AddHours(7);
            DateTime parse = DateTime.Parse(tanggal_exprtok.ToString());
            TimeSpan ts = parse.Subtract(dateNow);
            bool ATExp = false;

            //if (ts.Days < 1 && ts.Hours < 24 && dateNow < tanggal_exptoken)
            if (dateNow >= tanggal_exptoken)
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
                        string shopid = getShopId(tauth.Data.AccessToken);
                        var dateExpired = DateTimeOffset.FromUnixTimeSeconds(tauth.Data.AccessTokenExpireIn).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                        var tokendateExpired = DateTimeOffset.FromUnixTimeSeconds(tauth.Data.RefreshTokenExpireIn).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                        var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET TOKEN = '" + tauth.Data.AccessToken
                            + "', REFRESH_TOKEN = '" + tauth.Data.RefreshToken + "', STATUS_API = '1', TGL_EXPIRED = '" + tokendateExpired + "',TOKEN_EXPIRED = '"
                            + dateExpired + "' , SORT1_CUST = '" + shopid + "' WHERE CUST = '" + cust + "'");
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

        public string getShopId(string acctoken)
        {
            try
            {
                string urll = "https://open-api.tiktokglobalshop.com/api/shop/get_authorized_shop?access_token={0}&timestamp={1}&sign={2}&app_key={3}";
                int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                string sign = eraAppSecret + "/api/shop/get_authorized_shopapp_key" + eraAppKey + "timestamp" + timestamp + eraAppSecret;
                string signencry = GetHash(sign, eraAppSecret);
                var vformatUrl = String.Format(urll, acctoken, timestamp, signencry, eraAppKey);
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
                catch (Exception ex)
                {
                }

                try
                {
                    if (responseFromServer != null)
                    {
                        ShopListRes sres = JsonConvert.DeserializeObject<ShopListRes>(responseFromServer);
                        return sres.Data.ShopList[0].ShopId;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }


        #endregion

        #region Main Function
        #region HangFire Trigger
        //[AutomaticRetry(Attempts = 2)]
        //[Queue("3_general")]
        //public async Task<string> GetOrderListPaid(string CUST, string NAMA_CUST, int page, TTApiData apidata)
        //{
        //    SetupContext(apidata.access_token, apidata.username, apidata);
        //    string ret = "";
        //    string connId = Guid.NewGuid().ToString();
        //    var dateFrom = DateTimeOffset.UtcNow.AddDays(-3).AddHours(7).ToString("yyyy-MM-ddTHH:mm:ssZ");
        //    var dateTo = DateTimeOffset.UtcNow.AddHours(7).ToString("yyyy-MM-ddTHH:mm:ssZ");
        //    string status = "";
        //    ret += "'" + connId + "' , ";
        //    string urll = "https://open-api.tiktokglobalshop.com/api/shop/get_authorized_shop?access_token={0}&timestamp={1}&sign={2}&app_key={3}";
        //    int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        //    string sign = eraAppSecret + "/api/shop/get_authorized_shopapp_key" + eraAppKey + "timestamp" + timestamp + eraAppSecret;
        //    string signencry = GetHash(sign, eraAppSecret);
        //    var vformatUrl = String.Format(urll, apidata.access_token, timestamp, signencry, eraAppKey);
        //    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
        //    myReq.Method = "GET";
        //    myReq.ContentType = "application/json";

        //    string responseFromServer = "";
        //    try
        //    {
        //        using (WebResponse response = myReq.GetResponse())
        //        {
        //            using (Stream stream = response.GetResponseStream())
        //            {
        //                StreamReader reader = new StreamReader(stream);
        //                responseFromServer = reader.ReadToEnd();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.Print(ex.Message);
        //    }

        //    if (responseFromServer != null)
        //    {
        //        List<WooController.WooOrder> orderData = JsonConvert.DeserializeObject<List<WooController.WooOrder>>(responseFromServer, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        //        if (orderData != null)
        //        {
        //            string connID = Guid.NewGuid().ToString();
        //            var getdata = await GetPaidOrder(orderData, CUST, NAMA_CUST, connID);
        //            if (getdata)
        //            {
        //                await GetOrderListPaid(CUST, NAMA_CUST, page + 1, apidata);
        //            }
        //        }
        //    }
        //    // add tuning no duplicate hangfire job get order
        //    var queryStatus = "\\\"}\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","\"000003\""
        //    var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + queryStatus + "%' and arguments like '%" + apidata.access_token + "%' and invocationdata like '%woocomerce%' and invocationdata like '%GetOrderListPaid%' and statename like '%Enque%' and invocationdata not like '%resi%'");
        //    // end add tuning no duplicate hangfire job get order
        //    return null;
        //}

        #endregion

        #region Order Function
        //public async Task<bool> GetPaidOrder(List<WooController.WooOrder> orderData, string CUST, string NAMA_CUST, string connID)
        //{
        //    ErasoftDbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE TEMP_WOOCOMERCE_ORDERS");
        //    ErasoftDbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE TEMP_WOOCOMERCE_ORDERS_ITEM");
        //    var orderPaid = orderData.Where(p => p.Status == "processing").ToList();
        //    var jmlhOrderNew = 0;
        //    string ordersn = "";
        //    if (orderPaid != null)
        //    {
        //        var ordernoindb = ErasoftDbContext.SOT01A.Where(x => x.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();
        //        var arf01 = ErasoftDbContext.ARF01.FirstOrDefault(x => x.CUST == CUST);
        //        var connIdARF01C = Guid.NewGuid().ToString();
        //        foreach (var order in orderPaid)
        //        {
        //            if (ordernoindb.Contains(order.Id.ToString()))
        //            {
        //                ordersn = ordersn + "'" + order.Id.ToString() + "',";
        //            }
        //            else
        //            {
        //                jmlhOrderNew++;
        //                #region Insert Pembeli
        //                string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
        //                insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
        //                insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
        //                var kabKot = "3174";
        //                var prov = "31";


        //                string fullname = order.Billing.FirstName.ToString() + " " + order.Billing.LastName.ToString();
        //                string nama = fullname.Length > 30 ? fullname.Substring(0, 30) : fullname;

        //                string TLP = !string.IsNullOrEmpty(order.Billing.Phone) ? order.Billing.Phone.Replace('\'', '`') : "";
        //                if (TLP.Length > 30)
        //                    TLP = TLP.Substring(0, 30);
        //                if (NAMA_CUST.Length > 30)
        //                    NAMA_CUST = NAMA_CUST.Substring(0, 30);
        //                string AL_KIRIM1 = !string.IsNullOrEmpty((order.Billing.Address1 ?? "").Replace("'", "`") + " " + (order.Billing.Address2 ?? "").Replace("'", "`")) ? ((order.Billing.Address1 ?? "").Replace("'", "`") + " " + (order.Billing.Address2 ?? "").Replace("'", "`")) : "";
        //                if (AL_KIRIM1.Length > 30)
        //                {
        //                    AL_KIRIM1 = AL_KIRIM1.Substring(0, 30);
        //                }
        //                string KODEPOS = !string.IsNullOrEmpty(order.Billing.Postcode) ? order.Billing.Postcode.Replace('\'', '`') : "";
        //                if (KODEPOS.Length > 7)
        //                {
        //                    KODEPOS = KODEPOS.Substring(0, 7);
        //                }
        //                string province = !string.IsNullOrEmpty(order.Billing.State) ? order.Billing.State.Replace("'", "`") : "";
        //                if (province.Length > 50)
        //                {
        //                    province = province.Substring(0, 50);
        //                }
        //                string city = !string.IsNullOrEmpty(order.Billing.City) ? order.Billing.City.Replace("'", "`") : "";
        //                if (city.Length > 50)
        //                {
        //                    city = city.Substring(0, 50);
        //                }
        //                string contact_email = !string.IsNullOrEmpty(order.Billing.Email) ? order.Billing.Email.Replace("'", "`") : "";
        //                if (contact_email.Length > 50)
        //                {
        //                    contact_email = city.Substring(0, 50);
        //                }

        //                insertPembeli += string.Format("('{0}','{1}','{2}','{3}',0,0,'0','01',1, 'IDR', '01', '{4}', 0, 0, 0, 0, '1', 0, 0,'FP', '{5}', '{6}', '{7}', '{13}', '{8}', '{9}', '{12}', '{11}','{10}'),",
        //                    ((nama ?? "").Replace("'", "`")),
        //                    ((order.Billing.Address1 ?? "").Replace("'", "`") + " " + (order.Billing.Address2 ?? "").Replace("'", "`")),
        //                    (TLP),
        //                    (NAMA_CUST.Replace(',', '.')),
        //                    AL_KIRIM1,
        //                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //                    (username),
        //                    KODEPOS,
        //                    kabKot,
        //                    prov,
        //                    connIdARF01C,
        //                    province,
        //                    city,
        //                    contact_email
        //                    );
        //                insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
        //                EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);

        //                #endregion
        //                if (!ordernoindb.Contains(order.Id.ToString()))
        //                {
        //                    try
        //                    {
        //                        var conidARF = Guid.NewGuid().ToString();
        //                        var dateOrder = Convert.ToDateTime(order.DateCreated).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
        //                        var datePay = Convert.ToDateTime(order.DatePaid).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
        //                        #region cut char
        //                        string estimated_shipping_fee = !string.IsNullOrEmpty(Convert.ToString(double.Parse(order.ShippingTax))) ? Convert.ToString(double.Parse(order.ShippingTax)).Replace("'", "`") : "";
        //                        if (estimated_shipping_fee.Length > 100)
        //                        {
        //                            estimated_shipping_fee = estimated_shipping_fee.Substring(0, 100);
        //                        }
        //                        string payment_method = !string.IsNullOrEmpty(order.PaymentMethod) ? order.PaymentMethod.Replace("'", "`") : "";
        //                        if (payment_method.Length > 100)
        //                        {
        //                            payment_method = payment_method.Substring(0, 100);
        //                        }
        //                        string escrow_amount = !string.IsNullOrEmpty(order.DiscountTotal) ? order.DiscountTotal.Replace("'", "`") : "";
        //                        if (escrow_amount.Length > 100)
        //                        {
        //                            escrow_amount = escrow_amount.Substring(0, 100);
        //                        }
        //                        string currency = !string.IsNullOrEmpty(order.Currency) ? order.Currency.Replace("'", "`") : "";
        //                        if (currency.Length > 50)
        //                        {
        //                            currency = currency.Substring(0, 50);
        //                        }
        //                        string Recipient_Address_country = !string.IsNullOrEmpty(order.Shipping.Country) ? order.Shipping.Country.Replace("'", "`") : "ID";
        //                        if (Recipient_Address_country.Length > 50)
        //                        {
        //                            Recipient_Address_country = Recipient_Address_country.Substring(0, 50);
        //                        }
        //                        string Recipient_Address_city = !string.IsNullOrEmpty(order.Shipping.City) ? order.Shipping.City.Replace("'", "`") : "";
        //                        if (Recipient_Address_city.Length > 50)
        //                        {
        //                            Recipient_Address_city = Recipient_Address_city.Substring(0, 50);
        //                        }
        //                        string Recipient_Address_state = !string.IsNullOrEmpty(order.Shipping.State) ? order.Shipping.State.Replace("'", "`") : "";
        //                        if (Recipient_Address_state.Length > 50)
        //                        {
        //                            Recipient_Address_state = Recipient_Address_state.Substring(0, 50);
        //                        }
        //                        string total_amount = !string.IsNullOrEmpty(Convert.ToString(double.Parse(order.Total))) ? Convert.ToString(double.Parse(order.Total)).Replace("'", "`") : "";
        //                        if (total_amount.Length > 100)
        //                        {
        //                            total_amount = total_amount.Substring(0, 100);
        //                        }
        //                        string country = !string.IsNullOrEmpty(order.Shipping.Country) ? order.Shipping.Country.Replace("'", "`") : "";
        //                        if (country.Length > 100)
        //                        {
        //                            country = country.Substring(0, 100);
        //                        }
        //                        string ordersn1 = !string.IsNullOrEmpty(Convert.ToString(order.Id)) ? Convert.ToString(order.Id).Replace("'", "`") : "";
        //                        if (ordersn1.Length > 100)
        //                        {
        //                            ordersn1 = ordersn1.Substring(0, 100);
        //                        }

        //                        #endregion
        //                        var shippingLine = "";
        //                        var ongkir = "";
        //                        var trackingCompany = "";
        //                        var trackingNumber = "";

        //                        if (order.ShippingLines.Count() > 0)
        //                        {
        //                            shippingLine = order.ShippingLines[0].MethodTitle;
        //                            ongkir = order.ShippingLines[0].Total;
        //                        }
        //                        TEMP_WOOCOMERCE_ORDERS newOrder = new TEMP_WOOCOMERCE_ORDERS()
        //                        {
        //                            company = nama,
        //                            country = country,
        //                            date_created = Convert.ToDateTime(dateOrder),
        //                            currency = currency,
        //                            order_key = order.OrderKey,
        //                            customer_note = order.CustomerNote ?? "",
        //                            date_completed = DateTime.Now,
        //                            id = ordersn1,
        //                            //ordersn = Convert.ToString(order.order_number),
        //                            //order_status = order.current_state_name,
        //                            status = "PAID",
        //                            //change by nurul 5/12/2019, local time 
        //                            //pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime,
        //                            date_paid = Convert.ToDateTime(datePay),
        //                            //end change by nurul 5/12/2019, local time 
        //                            state = Recipient_Address_state,
        //                            city = Recipient_Address_city,
        //                            address_1 = (order.Shipping.Address1 ?? "").Replace("'", "`") + " " + (order.Shipping.Address2 ?? "").Replace("'", "`"),
        //                            phone = TLP,
        //                            postalcode = KODEPOS,
        //                            email = contact_email,
        //                            number = order.Number,
        //                            total = total_amount,
        //                            date_modified = Convert.ToDateTime(dateOrder),
        //                            CONN_ID = connID,
        //                            CUST = CUST,
        //                            NAMA_CUST = NAMA_CUST,
        //                            ship_title = shippingLine,
        //                            ongkos_kirim = ongkir
        //                        };
        //                        List<TEMP_WOOCOMERCE_ORDERS_ITEM> addedsot1b = new List<TEMP_WOOCOMERCE_ORDERS_ITEM>();
        //                        var getlastinsert = ErasoftDbContext.SOT01A.OrderBy(x => x.RecNum).ToList().LastOrDefault();
        //                        foreach (var item in order.LineItems)
        //                        {
        //                            #region cut char
        //                            string item_name = !string.IsNullOrEmpty(item.Name) ? (item.Name).Replace("'", "`") : "";
        //                            if (item_name.Length > 100)
        //                            {
        //                                item_name = item_name.Substring(0, 100);
        //                            }
        //                            string item_sku = !string.IsNullOrEmpty(item.Sku) ? item.Sku.Replace("'", "`") : "";
        //                            if (item_sku.Length > 150)
        //                            {
        //                                item_sku = item_sku.Substring(0, 150);
        //                            }
        //                            string variation_discounted_price = !string.IsNullOrEmpty(Convert.ToString(double.Parse(item.Total))) ? Convert.ToString(double.Parse(item.Total)).Replace("'", "`") : "";
        //                            if (variation_discounted_price.Length > 100)
        //                            {
        //                                variation_discounted_price = variation_discounted_price.Substring(0, 100);
        //                            }
        //                            string variation_name = !string.IsNullOrEmpty(item.Name) ? item.Name.Replace("'", "`") : "";
        //                            if (variation_name.Length > 50)
        //                            {
        //                                variation_name = variation_name.Substring(0, 50);
        //                            }
        //                            string item_id = !string.IsNullOrEmpty(Convert.ToString(item.ProductId)) ? Convert.ToString(item.ProductId).Replace("'", "`") : "";
        //                            if (item_id.Length > 20)
        //                            {
        //                                item_id = item_id.Substring(0, 20);
        //                            }
        //                            string variation_id = !string.IsNullOrEmpty(Convert.ToString(item.VariationId)) ? Convert.ToString(item.VariationId).Replace("'", "`") : "";
        //                            if (variation_id.Length > 20)
        //                            {
        //                                variation_id = variation_id.Substring(0, 20);
        //                            }
        //                            var getbrgindb = ErasoftDbContext.STF02.FirstOrDefault(x => x.BRG == item_id + ";" + variation_id);
        //                            #endregion

        //                            TEMP_WOOCOMERCE_ORDERS_ITEM newOrderItem = new TEMP_WOOCOMERCE_ORDERS_ITEM()
        //                            {
        //                                id = ordersn1,
        //                                product_id = item.ProductId.ToString(),
        //                                name = item_name,
        //                                sku = item_sku,
        //                                variation_id = variation_id,
        //                                weight = 1,
        //                                CONN_ID = connID,
        //                                date_created = Convert.ToDateTime(dateOrder),
        //                                quantity = item.Quantity.ToString(),
        //                                total = item.Total.ToString(),
        //                                price = item.Price.ToString(),
        //                                CUST = CUST,
        //                                NAMA_CUST = NAMA_CUST
        //                            };

        //                            addedsot1b.Add(newOrderItem);
        //                        }
        //                        try
        //                        {
        //                            ErasoftDbContext.TEMP_WOOCOMERCE_ORDERS.Add(newOrder);
        //                            ErasoftDbContext.TEMP_WOOCOMERCE_ORDERS_ITEM.AddRange(addedsot1b);
        //                            ErasoftDbContext.SaveChanges();
        //                        }
        //                        catch (Exception ex)
        //                        {

        //                        }
        //                        using (SqlCommand CommandSQL = new SqlCommand())
        //                        {
        //                            //call sp to insert buyer data
        //                            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
        //                            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIdARF01C;

        //                            EDB.ExecuteSQL("Con", "MoveARF01CFromTempTable", CommandSQL);
        //                        };
        //                        using (SqlCommand CommandSQL = new SqlCommand())
        //                        {
        //                            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
        //                            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connID;
        //                            CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
        //                            CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //                            CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
        //                            CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
        //                            CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
        //                            CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
        //                            CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
        //                            CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
        //                            CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
        //                            CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
        //                            CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
        //                            CommandSQL.Parameters.Add("@Woocom", SqlDbType.Int).Value = 1;
        //                            CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

        //                            EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
        //                        }
        //                    }
        //                    catch (DbEntityValidationException ex)
        //                    {
        //                        foreach (var eve in ex.EntityValidationErrors)
        //                        {
        //                            Debug.Print("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
        //                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
        //                            foreach (var ve in eve.ValidationErrors)
        //                            {
        //                                Debug.Print("- Property: \"{0}\", Error: \"{1}\"",
        //                                    ve.PropertyName, ve.ErrorMessage);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(ordersn))
        //        {
        //            ordersn = ordersn.Substring(0, ordersn.Length - 1);
        //            var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '0'");
        //            jmlhOrderNew = jmlhOrderNew + rowAffected;
        //            if (jmlhOrderNew > 0)
        //            {
        //                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
        //                contextNotif.Clients.Group(apidata.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderNew) + " Pesanan terbayar dari Woocomerce.");
        //            }
        //        }
        //    }
        //    if (orderPaid.Count() == 10)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
        #endregion

        #region Update Stock
        public async Task<string> UpdateProductTiktok(TTApiData iden, string idbarang = null, int stok = 0)
        {
            SetupContext(iden.DatabasePathErasoft, iden.username, iden);
            string[] split = idbarang.Split(';');
            StockUpdateTik sut = new StockUpdateTik()
            {
                ProductId = split[0],
                Skus = new List<SkuTik>()
            };
            SkuTik sku = new SkuTik()
            {
                Id = split[1],
                StockInfos = new List<StockInfoTik>()
            };
            StockInfoTik soi = new StockInfoTik()
            {
                AvailableStock = stok
            };
            sku.StockInfos.Add(soi);
            sut.Skus.Add(sku);
            string data = JsonConvert.SerializeObject(sut);
            string urll = "https://open-api.tiktokglobalshop.com/api/products/stocks?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/products/stocksapp_key" + eraAppKey + "shop_id" + apidata.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, apidata.access_token, timestamp, signencry, eraAppKey, iden.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "PUT";
            myReq.ContentType = "application/json";
            myReq.Accept = "application/json";
            string responseFromServer = "";
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
            catch (Exception e)
            {

            }

            if (responseFromServer != null)
            {
                return null;
            }
            return null;
        }
        #endregion


        public async Task<string> GetShippingProvider(string cust)
        {
            var ret = "";
            var tblCustomer = ErasoftDbContext.ARF01.Where(m => m.CUST == cust).FirstOrDefault();
            if (tblCustomer.STATUS_API == "1")
            {
                if (!string.IsNullOrEmpty(tblCustomer.TOKEN))
                {
                    string urll = "https://open-api.tiktokglobalshop.com/api/logistics/shipping_providers?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
                    int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    string sign = eraAppSecret + "/api/logistics/shipping_providersapp_key" + eraAppKey + "shop_id" + tblCustomer.Sort1_Cust + "timestamp" + timestamp + eraAppSecret;
                    string signencry = GetHash(sign, eraAppSecret);
                    var vformatUrl = String.Format(urll, tblCustomer.TOKEN, timestamp, signencry, eraAppKey, tblCustomer.Sort1_Cust);
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
                    myReq.Method = "GET";
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

                    if (responseFromServer != "")
                    {
                        var result = JsonConvert.DeserializeObject(responseFromServer, typeof(TiktokGetShipmentResponse)) as TiktokGetShipmentResponse;
                        if (result.code == 0)
                        {
                            if (result.data != null)
                            {
                                var tempShipment = ErasoftDbContext.DELIVERY_PROVIDER_TIKTOK.Where(m => m.CUST == cust).ToList();
                                foreach (var delivery in result.data.delivery_option_list)
                                {
                                    foreach (var shipment in delivery.shipping_provider_list)
                                    {
                                        if (tempShipment.Where(m => m.SHIPPING_ID == shipment.shipping_provider_id).ToList().Count == 0)
                                        {
                                            var newProvider = new DELIVERY_PROVIDER_TIKTOK();
                                            newProvider.CUST = cust;
                                            newProvider.NAME = delivery.delivery_option_name + " " + shipment.shipping_provider_name;
                                            newProvider.SHIPPING_ID = shipment.shipping_provider_id;

                                            ErasoftDbContext.DELIVERY_PROVIDER_TIKTOK.Add(newProvider);
                                            ErasoftDbContext.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ret;
        }
        #region Fetch Function
        #region Category
        public string getCategory(TTApiData apidata)
        {
            string urll = "https://open-api.tiktokglobalshop.com/api/products/categories?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/products/categoriesapp_key" + eraAppKey + "shop_id" + apidata.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, apidata.access_token, timestamp, signencry, eraAppKey, apidata.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string ret = "";
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
            try
            {
                if (responseFromServer != null)
                {
                    ResProd respon = JsonConvert.DeserializeObject<ResProd>(responseFromServer);

                    try
                    {
                        EDB.ExecuteSQL("CString", CommandType.Text, "DELETE FROM CATEGORY_TIKTOK WHERE CUST = '" + apidata.no_cust + "'");
                        foreach (var item in respon.Data.CategoryList)
                        {
                            var newCategory = new CATEGORY_TIKTOK
                            {
                                CATEGORY_CODE = item.Id,
                                CATEGORY_NAME = item.LocalDisplayName.Replace("'", "`"),
                                IS_LAST_NODE = (item.IsLeaf) ? "1" : "0",
                                PARENT_CODE = item.ParentId ?? "0",
                                CUST = apidata.no_cust
                            };
                            if (item.IsLeaf)//cek category rule //remark, pindah cek category rule saat ambil attr agar tidak lama saat insert category
                            {
                                //var rule = getCategoryRule(apidata, item.Id);
                                //if (rule   != null)
                                //{
                                //    if (rule.category_rules.Count > 0)
                                //    {
                                //        if (rule.category_rules[0].support_cod)
                                //        {
                                //            newCategory.COD = "1";
                                //        }
                                //        if (rule.category_rules[0].support_size_chart)
                                //        {
                                //            newCategory.SIZE_CHART = "1";
                                //        }
                                //        if (rule.category_rules[0].product_certifications != null)
                                //        {
                                //            if (rule.category_rules[0].product_certifications.Count > 0)
                                //            {
                                //                foreach (var pCert in rule.category_rules[0].product_certifications)
                                //                {
                                //                    if (pCert.is_mandatory)
                                //                    {
                                //                        newCategory.CERTIFICATION = pCert.id + ":" + pCert.name.Replace("'", "`") + ",";
                                //                    }
                                //                }
                                //                if (!string.IsNullOrEmpty(newCategory.CERTIFICATION))
                                //                    newCategory.CERTIFICATION = newCategory.CERTIFICATION.Substring(0, newCategory.CERTIFICATION.Length - 1);
                                //            }
                                //        }
                                //    }
                                //}
                            }
                            ErasoftDbContext.CATEGORY_TIKTOK.Add(newCategory);
                            ErasoftDbContext.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            catch (Exception ex)
            {

            }
            return ret;

        }

        public TiktokCategoryRuleData getCategoryRule(TTApiData apidata, string code)
        {
            string urll = "https://open-api.tiktokglobalshop.com/api/products/categories/rules?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}&category_id={5}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/products/categories/rulesapp_key" + eraAppKey + "category_id" + code
                + "shop_id" + apidata.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, apidata.access_token, timestamp, signencry, eraAppKey, apidata.shop_id, code);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            var ret = new TiktokCategoryRuleData();
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
            try
            {
                if (responseFromServer != null)
                {
                    TiktokCategoryRuleRespose respon = JsonConvert.DeserializeObject<TiktokCategoryRuleRespose>(responseFromServer);
                    if (respon.code == 0)
                    {
                        ret = respon.data;
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return ret;

        }
        #endregion
        public List<TiktokBrand> getBrand(TTApiData apidata)
        {
            string urll = "https://open-api.tiktokglobalshop.com/api/products/brands?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/products/brandsapp_key" + eraAppKey + "shop_id" + apidata.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, apidata.access_token, timestamp, signencry, eraAppKey, apidata.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            var ret = new List<TiktokBrand>();
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
            try
            {
                if (responseFromServer != "")
                {
                    TiktokGetBrandResponse respon = JsonConvert.DeserializeObject<TiktokGetBrandResponse>(responseFromServer);
                    if (respon.code == 0)
                    {
                        if (respon.data != null)
                            ret = respon.data.brand_list;
                        //foreach (var brand in respon.data.brand_list)
                        //{
                        //var newData = new TABEL_TIKTOK_BRAND()
                        //{
                        //    BRAND_ID = brand.id,
                        //    NAME = brand.name.Replace('\'', '`')
                        //};
                        //if (ErasoftDbContext.TABEL_TIKTOK_BRAND.Where(m => m.BRAND_ID == newData.BRAND_ID).ToList().Count == 0)
                        //{
                        //    ErasoftDbContext.TABEL_TIKTOK_BRAND.Add(newData);
                        //    ErasoftDbContext.SaveChanges();
                        //}

                        //}
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return ret;

        }
        public List<TiktokWarehouse> getWarehouse(TTApiData apidata)
        {
            string urll = "https://open-api.tiktokglobalshop.com/api/logistics/get_warehouse_list?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/logistics/get_warehouse_listapp_key" + eraAppKey + "shop_id" + apidata.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, apidata.access_token, timestamp, signencry, eraAppKey, apidata.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            var ret = new List<TiktokWarehouse>();
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
            try
            {
                if (responseFromServer != "")
                {
                    TiktokGetWarehouseResponse respon = JsonConvert.DeserializeObject<TiktokGetWarehouseResponse>(responseFromServer);
                    if (respon.code == 0)
                    {
                        if (respon.data != null)
                            ret = respon.data.warehouse_list.Where(m => m.warehouse_type == 1).ToList();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return ret;

        }
        #region FetchBarang
        public async Task<BindingBase> getproduct(TTApiData iden, int IdMarket, int page, int recordCount, int totalData)
        {
            SetupContext(iden.DatabasePathErasoft, username, iden);
            var ret = new BindingBase
            {
                status = 0,
                recordCount = recordCount,
                exception = 0,
                totalData = totalData,//add 18 Juli 2019, show total record
                pageinfo = ""
            };

            long seconds = CurrentTimeSecond();
            List<TEMP_BRG_MP> listnewrec = new List<TEMP_BRG_MP>();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = seconds.ToString() + "_" + page,
                REQUEST_ACTION = "Get Item List",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.no_cust,
                //REQUEST_ATTRIBUTE_3 = page.ToString(),
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden.no_cust, currentLog);
            string urll = "https://open-api.tiktokglobalshop.com/api/products/search?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/products/searchapp_key" + eraAppKey + "shop_id" + apidata.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, apidata.access_token, timestamp, signencry, eraAppKey, iden.shop_id);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "POST";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            ProductParameter paramter = new ProductParameter()
            {
                PageSize = 10,
                PageNumber = page,
                SearchStatus = 0,
                SellerSkuList = null
            };
            string data = JsonConvert.SerializeObject(paramter);
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
                //var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST = '" + iden.no_cust + "'");
                ret.exception = 1;
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden.no_cust, currentLog);
            }
            if (responseFromServer != null)
            {
                ResProd res = JsonConvert.DeserializeObject<ResProd>(responseFromServer);
                if (res.Data.Products.Count == 10)
                {
                    ret.nextPage = 1;
                }
                foreach (ProductTick ptick in res.Data.Products)
                {
                    //1 - draft、2 - pending、3 - failed、4 - live、5 - seller_deactivated、6 - platform_deactivated、7 - freeze 、8 - deleted
                    if (ptick.Status == 4 || ptick.Status == 5)
                    {
                        var bindGetProd = await getdetailproduct(ptick.Id, apidata, listnewrec, IdMarket);
                        if (bindGetProd.exception == 1)
                        {
                            ret.exception = 1;
                        }
                        ret.recordCount += bindGetProd.recordCount;
                        ret.totalData += bindGetProd.totalData;
                    }
                }
                //if (listnewrec.Count() > 0)
                //{
                //    ErasoftDbContext.TEMP_BRG_MP.AddRange(listnewrec);
                //    ErasoftDbContext.SaveChanges();
                //}
            }
            return ret;
        }

        public async Task<BindingBase> getdetailproduct(string productid, TTApiData iden, List<TEMP_BRG_MP> newrec, int idmarket)
        {
            var ret = new BindingBase();
            string urll = "https://open-api.tiktokglobalshop.com/api/products/details?access_token={0}&timestamp={1}&sign={2}&app_key={3}&shop_id={4}&product_id={5}";
            int timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string sign = eraAppSecret + "/api/products/detailsapp_key" + eraAppKey + "product_id" + productid + "shop_id" + apidata.shop_id + "timestamp" + timestamp + eraAppSecret;
            string signencry = GetHash(sign, eraAppSecret);
            var vformatUrl = String.Format(urll, apidata.access_token, timestamp, signencry, eraAppKey, iden.shop_id, productid);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
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
                ret.exception = 1;
            }
            try
            {
                if (responseFromServer != null)
                {
                    ResProductDet res = JsonConvert.DeserializeObject<ResProductDet>(responseFromServer);
                    DetailProductTik detail = res.productTik;
                    ret.totalData = detail.Skus.Count;
                    if (detail.Skus.Count > 1)
                    {
                        ret.totalData++;
                    }
                    string kdmp = detail.ProductId.ToString() + ";" + "0";
                    var checkstf02h = ErasoftDbContext.STF02H.FirstOrDefault(x => x.BRG_MP == kdmp && x.IDMARKET == idmarket);
                    var checkTemp = ErasoftDbContext.TEMP_BRG_MP.FirstOrDefault(x => x.BRG_MP == kdmp && x.IDMARKET == idmarket);
                    if (checkstf02h == null && checkTemp == null && detail.Skus.Count > 1)
                    {
                        var splitItemName = new StokControllerJob().SplitItemName(detail.ProductName.Replace('\'', '`'));
                        var nama = splitItemName[0];
                        var nama2 = splitItemName[1];
                        TEMP_BRG_MP tempbarang = new TEMP_BRG_MP();
                        tempbarang.BRG_MP = detail.ProductId.ToString() + ";" + "0";
                        tempbarang.NAMA = nama;
                        tempbarang.NAMA2 = nama2;
                        tempbarang.BERAT = detail.PackageWeight == "" ? double.Parse(0.ToString()) : (double.Parse(detail.PackageWeight) * 1000);//berat dalam kg
                        tempbarang.PANJANG = detail.PackageLength == 0 ? double.Parse(0.ToString()) : double.Parse(detail.PackageLength.ToString());
                        tempbarang.LEBAR = detail.PackageWidth == 0 ? double.Parse(0.ToString()) : double.Parse(detail.PackageWidth.ToString());
                        tempbarang.TINGGI = detail.PackageHeight == 0 ? double.Parse(0.ToString()) : double.Parse(detail.PackageHeight.ToString());
                        //tempbarang.CATEGORY_CODE = detail.CategoryList[2].Id;
                        //tempbarang.CATEGORY_NAME = detail.CategoryList[2].LocalDisplayName;
                        tempbarang.CATEGORY_CODE = detail.CategoryList[0].Id;
                        tempbarang.CATEGORY_NAME = detail.CategoryList[0].LocalDisplayName;
                        foreach (var kat in detail.CategoryList)
                        {
                            if (kat.IsLeaf)
                            {
                                tempbarang.CATEGORY_CODE = kat.Id;
                                tempbarang.CATEGORY_NAME = kat.LocalDisplayName;
                            }
                        }
                        foreach (ImageTik img in detail.Images)
                        {
                            if (tempbarang.IMAGE == null)
                            {
                                tempbarang.IMAGE = img.UrlList[0];
                            }
                            else if (tempbarang.IMAGE2 == null)
                            {
                                tempbarang.IMAGE2 = img.UrlList[0];
                            }
                            else if (tempbarang.IMAGE3 == null)
                            {
                                tempbarang.IMAGE3 = img.UrlList[0];
                            }
                            else if (tempbarang.IMAGE4 == null)
                            {
                                tempbarang.IMAGE4 = img.UrlList[0];
                            }
                            else if (tempbarang.IMAGE5 == null)
                            {
                                tempbarang.IMAGE5 = img.UrlList[0];
                            }

                        }
                        tempbarang.Deskripsi = detail.Description;
                        tempbarang.IDMARKET = int.Parse(idmarket.ToString());
                        tempbarang.CUST = iden.no_cust;
                        tempbarang.AVALUE_45 = detail.ProductName.Replace('\'', '`');
                        tempbarang.ACODE_31 = "warranty";
                        tempbarang.ANAME_31 = "Warranty Period";
                        tempbarang.AVALUE_31 = detail.WarrantyPeriod.WarrantyId.ToString();
                        tempbarang.ACODE_32 = "product_warranty";
                        tempbarang.ANAME_32 = "Warranty Policy";
                        tempbarang.AVALUE_32 = detail.WarrantyPolicy;
                        tempbarang.MEREK = detail.Brand == null ? "No Brand" : detail.Brand.Name;
                        tempbarang.ANAME_38 = detail.Brand == null ? "" : detail.Brand.Id;
                        tempbarang.AVALUE_38 = tempbarang.MEREK;
                        tempbarang.DISPLAY = (detail.ProductStatus == 4 ? true : false);
                        tempbarang.SELLER_SKU = "";
                        tempbarang.TYPE = "4";
                        tempbarang.HJUAL = Convert.ToDouble( detail.Skus[0].Price.OriginalPrice);
                        tempbarang.HJUAL_MP = Convert.ToDouble(detail.Skus[0].Price.OriginalPrice);
                        tempbarang.AVALUE_34 = "https://shop.tiktok.com/view/product/" + detail.ProductId;
                        tempbarang.AVALUE_39 = (detail.IsCodOpen ? "1" : "0");
                        tempbarang.PICKUP_POINT = detail.Skus[0].StockInfos[0].WarehouseId;
                        //foreach (SkuTik satikd in detail.Skus)
                        {
                            foreach (SalesAttributeTik satik in detail.Skus[0].SalesAttributes)
                            {
                                if (tempbarang.ACODE_1 == null)
                                {
                                    tempbarang.ACODE_1 = satik.Id;
                                    tempbarang.ANAME_1 = satik.Name;
                                    tempbarang.AVALUE_1 = satik.ValueName;
                                }
                                else if (tempbarang.ACODE_2 == null)
                                {
                                    tempbarang.ACODE_2 = satik.Id;
                                    tempbarang.ANAME_2 = satik.Name;
                                    tempbarang.AVALUE_2 = satik.ValueName;
                                }

                            }
                            if (detail.product_attributes != null)
                            {
                                foreach (var prodAttr in detail.product_attributes)
                                {
                                    if (tempbarang.ACODE_3 == null)
                                    {
                                        tempbarang.ACODE_3 = prodAttr.Id;
                                        tempbarang.ANAME_3 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_3))
                                            {
                                                tempbarang.AVALUE_3 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_3 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_3 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_4 == null)
                                    {
                                        tempbarang.ACODE_4 = prodAttr.Id;
                                        tempbarang.ANAME_4 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_4))
                                            {
                                                tempbarang.AVALUE_4 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_4 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_4 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_5 == null)
                                    {
                                        tempbarang.ACODE_5 = prodAttr.Id;
                                        tempbarang.ANAME_5 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_5))
                                            {
                                                tempbarang.AVALUE_5 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_5 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_5 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_6 == null)
                                    {
                                        tempbarang.ACODE_6 = prodAttr.Id;
                                        tempbarang.ANAME_6 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_6))
                                            {
                                                tempbarang.AVALUE_6 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_6 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_6 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_7 == null)
                                    {
                                        tempbarang.ACODE_7 = prodAttr.Id;
                                        tempbarang.ANAME_7 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_7))
                                            {
                                                tempbarang.AVALUE_7 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_7 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_7 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_8 == null)
                                    {
                                        tempbarang.ACODE_8 = prodAttr.Id;
                                        tempbarang.ANAME_8 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_8))
                                            {
                                                tempbarang.AVALUE_8 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_8 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_8 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_9 == null)
                                    {
                                        tempbarang.ACODE_9 = prodAttr.Id;
                                        tempbarang.ANAME_9 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_9))
                                            {
                                                tempbarang.AVALUE_9 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_9 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_9 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_10 == null)
                                    {
                                        tempbarang.ACODE_10 = prodAttr.Id;
                                        tempbarang.ANAME_10 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_10))
                                            {
                                                tempbarang.AVALUE_10 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_10 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_10 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_11 == null)
                                    {
                                        tempbarang.ACODE_11 = prodAttr.Id;
                                        tempbarang.ANAME_11 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_11))
                                            {
                                                tempbarang.AVALUE_11 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_11 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_11 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_12 == null)
                                    {
                                        tempbarang.ACODE_12 = prodAttr.Id;
                                        tempbarang.ANAME_12 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_12))
                                            {
                                                tempbarang.AVALUE_12 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_12 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_12 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_13 == null)
                                    {
                                        tempbarang.ACODE_13 = prodAttr.Id;
                                        tempbarang.ANAME_13 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_13))
                                            {
                                                tempbarang.AVALUE_13 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_13 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_13 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_14 == null)
                                    {
                                        tempbarang.ACODE_14 = prodAttr.Id;
                                        tempbarang.ANAME_14 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_14))
                                            {
                                                tempbarang.AVALUE_14 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_14 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_14 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_15 == null)
                                    {
                                        tempbarang.ACODE_15 = prodAttr.Id;
                                        tempbarang.ANAME_15 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_15))
                                            {
                                                tempbarang.AVALUE_15 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_15 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_15 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_16 == null)
                                    {
                                        tempbarang.ACODE_16 = prodAttr.Id;
                                        tempbarang.ANAME_16 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_16))
                                            {
                                                tempbarang.AVALUE_16 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_16 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_16 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_17 == null)
                                    {
                                        tempbarang.ACODE_17 = prodAttr.Id;
                                        tempbarang.ANAME_17 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_17))
                                            {
                                                tempbarang.AVALUE_17 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_17 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_17 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_18 == null)
                                    {
                                        tempbarang.ACODE_18 = prodAttr.Id;
                                        tempbarang.ANAME_18 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_18))
                                            {
                                                tempbarang.AVALUE_18 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_18 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_18 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_19 == null)
                                    {
                                        tempbarang.ACODE_19 = prodAttr.Id;
                                        tempbarang.ANAME_19 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_19))
                                            {
                                                tempbarang.AVALUE_19 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_19 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_19 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_20 == null)
                                    {
                                        tempbarang.ACODE_20 = prodAttr.Id;
                                        tempbarang.ANAME_20 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_20))
                                            {
                                                tempbarang.AVALUE_20 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_20 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_20 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_21 == null)
                                    {
                                        tempbarang.ACODE_21 = prodAttr.Id;
                                        tempbarang.ANAME_21 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_21))
                                            {
                                                tempbarang.AVALUE_21 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_21 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_21 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_22 == null)
                                    {
                                        tempbarang.ACODE_22 = prodAttr.Id;
                                        tempbarang.ANAME_22 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_22))
                                            {
                                                tempbarang.AVALUE_22 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_22 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_22 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_23 == null)
                                    {
                                        tempbarang.ACODE_23 = prodAttr.Id;
                                        tempbarang.ANAME_23 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_23))
                                            {
                                                tempbarang.AVALUE_23 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_23 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_23 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_24 == null)
                                    {
                                        tempbarang.ACODE_24 = prodAttr.Id;
                                        tempbarang.ANAME_24 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_24))
                                            {
                                                tempbarang.AVALUE_24 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_24 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_24 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_25 == null)
                                    {
                                        tempbarang.ACODE_25 = prodAttr.Id;
                                        tempbarang.ANAME_25 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_25))
                                            {
                                                tempbarang.AVALUE_25 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_25 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_25 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_26 == null)
                                    {
                                        tempbarang.ACODE_26 = prodAttr.Id;
                                        tempbarang.ANAME_26 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_26))
                                            {
                                                tempbarang.AVALUE_26 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_26 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_26 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_27 == null)
                                    {
                                        tempbarang.ACODE_27 = prodAttr.Id;
                                        tempbarang.ANAME_27 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_27))
                                            {
                                                tempbarang.AVALUE_27 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_27 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_27 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_28 == null)
                                    {
                                        tempbarang.ACODE_28 = prodAttr.Id;
                                        tempbarang.ANAME_28 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_28))
                                            {
                                                tempbarang.AVALUE_28 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_28 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_28 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_29 == null)
                                    {
                                        tempbarang.ACODE_29 = prodAttr.Id;
                                        tempbarang.ANAME_29 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_29))
                                            {
                                                tempbarang.AVALUE_29 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_29 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_29 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                    else if (tempbarang.ACODE_30 == null)
                                    {
                                        tempbarang.ACODE_30 = prodAttr.Id;
                                        tempbarang.ANAME_30 = prodAttr.name.Replace('\'', '`');
                                        foreach (var attrVal in prodAttr.values)
                                        {
                                            if (!string.IsNullOrEmpty(tempbarang.AVALUE_30))
                                            {
                                                tempbarang.AVALUE_30 += ",";
                                            }
                                            if ((attrVal.Id ?? "").Length > 10)
                                            {
                                                tempbarang.AVALUE_30 += attrVal.name.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                tempbarang.AVALUE_30 += attrVal.Id.Replace('\'', '`');
                                            }
                                        }
                                    }
                                }
                            }
                            #region old attr
                            //foreach (SalesAttributeTik satik in detail.Skus[0].SalesAttributes)
                            //    {
                            //    if (tempbarang.ACODE_1 == null)
                            //    {
                            //        tempbarang.ACODE_1 = satik.Id;
                            //        tempbarang.ANAME_1 = satik.Name;
                            //        tempbarang.AVALUE_1 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_2 == null)
                            //    {
                            //        tempbarang.ACODE_2 = satik.Id;
                            //        tempbarang.ANAME_2 = satik.Name;
                            //        tempbarang.AVALUE_2 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_3 == null)
                            //    {
                            //        tempbarang.ACODE_3 = satik.Id;
                            //        tempbarang.ANAME_3 = satik.Name;
                            //        tempbarang.AVALUE_3 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_4 == null)
                            //    {
                            //        tempbarang.ACODE_4 = satik.Id;
                            //        tempbarang.ANAME_4 = satik.Name;
                            //        tempbarang.AVALUE_4 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_5 == null)
                            //    {
                            //        tempbarang.ACODE_5 = satik.Id;
                            //        tempbarang.ANAME_5 = satik.Name;
                            //        tempbarang.AVALUE_5 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_6 == null)
                            //    {
                            //        tempbarang.ACODE_6 = satik.Id;
                            //        tempbarang.ANAME_6 = satik.Name;
                            //        tempbarang.AVALUE_6 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_7 == null)
                            //    {
                            //        tempbarang.ACODE_7 = satik.Id;
                            //        tempbarang.ANAME_7 = satik.Name;
                            //        tempbarang.AVALUE_7 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_8 == null)
                            //    {
                            //        tempbarang.ACODE_8 = satik.Id;
                            //        tempbarang.ANAME_8 = satik.Name;
                            //        tempbarang.AVALUE_8 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_9 == null)
                            //    {
                            //        tempbarang.ACODE_9 = satik.Id;
                            //        tempbarang.ANAME_9 = satik.Name;
                            //        tempbarang.AVALUE_9 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_10 == null)
                            //    {
                            //        tempbarang.ACODE_10 = satik.Id;
                            //        tempbarang.ANAME_10 = satik.Name;
                            //        tempbarang.AVALUE_10 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_11 == null)
                            //    {
                            //        tempbarang.ACODE_11 = satik.Id;
                            //        tempbarang.ANAME_11 = satik.Name;
                            //        tempbarang.AVALUE_11 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_12 == null)
                            //    {
                            //        tempbarang.ACODE_12 = satik.Id;
                            //        tempbarang.ANAME_12 = satik.Name;
                            //        tempbarang.AVALUE_12 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_13 == null)
                            //    {
                            //        tempbarang.ACODE_13 = satik.Id;
                            //        tempbarang.ANAME_13 = satik.Name;
                            //        tempbarang.AVALUE_13 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_14 == null)
                            //    {
                            //        tempbarang.ACODE_14 = satik.Id;
                            //        tempbarang.ANAME_14 = satik.Name;
                            //        tempbarang.AVALUE_14 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_15 == null)
                            //    {
                            //        tempbarang.ACODE_15 = satik.Id;
                            //        tempbarang.ANAME_15 = satik.Name;
                            //        tempbarang.AVALUE_15 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_16 == null)
                            //    {
                            //        tempbarang.ACODE_16 = satik.Id;
                            //        tempbarang.ANAME_16 = satik.Name;
                            //        tempbarang.AVALUE_16 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_17 == null)
                            //    {
                            //        tempbarang.ACODE_17 = satik.Id;
                            //        tempbarang.ANAME_17 = satik.Name;
                            //        tempbarang.AVALUE_17 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_18 == null)
                            //    {
                            //        tempbarang.ACODE_18 = satik.Id;
                            //        tempbarang.ANAME_18 = satik.Name;
                            //        tempbarang.AVALUE_18 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_19 == null)
                            //    {
                            //        tempbarang.ACODE_19 = satik.Id;
                            //        tempbarang.ANAME_19 = satik.Name;
                            //        tempbarang.AVALUE_19 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_20 == null)
                            //    {
                            //        tempbarang.ACODE_20 = satik.Id;
                            //        tempbarang.ANAME_20 = satik.Name;
                            //        tempbarang.AVALUE_20 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_21 == null)
                            //    {
                            //        tempbarang.ACODE_21 = satik.Id;
                            //        tempbarang.ANAME_21 = satik.Name;
                            //        tempbarang.AVALUE_21 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_22 == null)
                            //    {
                            //        tempbarang.ACODE_22 = satik.Id;
                            //        tempbarang.ANAME_22 = satik.Name;
                            //        tempbarang.AVALUE_22 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_23 == null)
                            //    {
                            //        tempbarang.ACODE_23 = satik.Id;
                            //        tempbarang.ANAME_23 = satik.Name;
                            //        tempbarang.AVALUE_23 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_24 == null)
                            //    {
                            //        tempbarang.ACODE_24 = satik.Id;
                            //        tempbarang.ANAME_24 = satik.Name;
                            //        tempbarang.AVALUE_24 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_25 == null)
                            //    {
                            //        tempbarang.ACODE_25 = satik.Id;
                            //        tempbarang.ANAME_25 = satik.Name;
                            //        tempbarang.AVALUE_25 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_26 == null)
                            //    {
                            //        tempbarang.ACODE_26 = satik.Id;
                            //        tempbarang.ANAME_26 = satik.Name;
                            //        tempbarang.AVALUE_26 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_27 == null)
                            //    {
                            //        tempbarang.ACODE_27 = satik.Id;
                            //        tempbarang.ANAME_27 = satik.Name;
                            //        tempbarang.AVALUE_27 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_28 == null)
                            //    {
                            //        tempbarang.ACODE_28 = satik.Id;
                            //        tempbarang.ANAME_28 = satik.Name;
                            //        tempbarang.AVALUE_28 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_29 == null)
                            //    {
                            //        tempbarang.ACODE_29 = satik.Id;
                            //        tempbarang.ANAME_29 = satik.Name;
                            //        tempbarang.AVALUE_29 = satik.ValueName;
                            //    }
                            //    else if (tempbarang.ACODE_30 == null)
                            //    {
                            //        tempbarang.ACODE_30 = satik.Id;
                            //        tempbarang.ANAME_30 = satik.Name;
                            //        tempbarang.AVALUE_30 = satik.ValueName;
                            //    }

                            //}
                            #endregion
                        }
                        //await getvariationproduct(productid, detail.Skus, newrec, idmarket, detail, apidata, ret);
                        //ret.recordCount++;
                        //newrec.Add(tempbarang);
                        try
                        {
                            ErasoftDbContext.TEMP_BRG_MP.Add(tempbarang);
                            ErasoftDbContext.SaveChanges();
                            ret.recordCount++;
                        }
                        catch (Exception ex)
                        {
                            ret.exception = 1;
                        }
                    }
                    else if (checkstf02h != null && detail.Skus.Count > 1)
                    {
                        kdmp = checkstf02h.BRG;
                    }
                    #region remark
                    //else
                    //{
                    //    TEMP_BRG_MP tempbarang = new TEMP_BRG_MP();
                    //    tempbarang.BRG_MP = detail.ProductId.ToString() + ";" + "0";
                    //    tempbarang.NAMA = detail.ProductName;
                    //    tempbarang.BERAT = detail.PackageWeight == "" ? double.Parse(0.ToString()) : double.Parse(detail.PackageWeight);
                    //    tempbarang.PANJANG = detail.PackageLength == 0 ? double.Parse(0.ToString()) : double.Parse(detail.PackageLength.ToString());
                    //    tempbarang.LEBAR = detail.PackageWidth == 0 ? double.Parse(0.ToString()) : double.Parse(detail.PackageWidth.ToString());
                    //    tempbarang.TINGGI = detail.PackageHeight == 0 ? double.Parse(0.ToString()) : double.Parse(detail.PackageHeight.ToString());
                    //    tempbarang.CATEGORY_CODE = detail.CategoryList[2].Id;
                    //    tempbarang.CATEGORY_NAME = detail.CategoryList[2].LocalDisplayName;
                    //    foreach (ImageTik img in detail.Images)
                    //    {
                    //        if (tempbarang.IMAGE == null)
                    //        {
                    //            tempbarang.IMAGE = img.UrlList[0];
                    //        }
                    //        else if (tempbarang.IMAGE2 == null)
                    //        {
                    //            tempbarang.IMAGE2 = img.UrlList[0];
                    //        }
                    //        else if (tempbarang.IMAGE3 == null)
                    //        {
                    //            tempbarang.IMAGE3 = img.UrlList[0];
                    //        }
                    //        else if (tempbarang.IMAGE4 == null)
                    //        {
                    //            tempbarang.IMAGE4 = img.UrlList[0];
                    //        }
                    //        else if (tempbarang.IMAGE5 == null)
                    //        {
                    //            tempbarang.IMAGE5 = img.UrlList[0];
                    //        }

                    //    }
                    //    tempbarang.Deskripsi = detail.Description;
                    //    tempbarang.IDMARKET = int.Parse(idmarket.ToString());
                    //    tempbarang.CUST = iden.no_cust;
                    //    tempbarang.AVALUE_45 = detail.ProductName;
                    //    tempbarang.ACODE_9 = "warranty";
                    //    tempbarang.ANAME_9 = "Warranty Period";
                    //    tempbarang.AVALUE_9 = detail.WarrantyPeriod.WarrantyDescription;
                    //    tempbarang.ACODE_11 = "product_warranty";
                    //    tempbarang.ANAME_11 = "Warranty Policy";
                    //    tempbarang.AVALUE_11 = detail.WarrantyPolicy;
                    //    tempbarang.MEREK = detail.Brand == null ? "No Brand" : detail.Brand.Name;
                    //    tempbarang.DISPLAY = true;
                    //    tempbarang.SELLER_SKU = checkstf02h.BRG;
                    //    tempbarang.TYPE = "4";
                    //    foreach (SkuTik satikd in detail.Skus)
                    //    {
                    //        foreach (SalesAttributeTik satik in satikd.SalesAttributes)
                    //        {
                    //            if (tempbarang.ACODE_1 == null)
                    //            {
                    //                tempbarang.ACODE_1 = satik.Id;
                    //                tempbarang.ANAME_1 = satik.Name;
                    //                tempbarang.AVALUE_1 = satik.ValueName;
                    //            }
                    //            else if (tempbarang.ACODE_2 == null)
                    //            {
                    //                tempbarang.ACODE_2 = satik.Id;
                    //                tempbarang.ANAME_2 = satik.Name;
                    //                tempbarang.AVALUE_2 = satik.ValueName;
                    //            }
                    //            else if (tempbarang.ACODE_3 == null)
                    //            {
                    //                tempbarang.ACODE_3 = satik.Id;
                    //                tempbarang.ANAME_3 = satik.Name;
                    //                tempbarang.AVALUE_3 = satik.ValueName;
                    //            }
                    //            else if (tempbarang.ACODE_4 == null)
                    //            {
                    //                tempbarang.ACODE_4 = satik.Id;
                    //                tempbarang.ANAME_4 = satik.Name;
                    //                tempbarang.AVALUE_4 = satik.ValueName;
                    //            }
                    //            else if (tempbarang.ACODE_5 == null)
                    //            {
                    //                tempbarang.ACODE_5 = satik.Id;
                    //                tempbarang.ANAME_5 = satik.Name;
                    //                tempbarang.AVALUE_5 = satik.ValueName;
                    //            }

                    //        }

                    //    }
                    //    await getvariationproduct(productid, detail.Skus, newrec, idmarket, detail, apidata, ret, tempbarang.SELLER_SKU);
                    //    ret.recordCount++;
                    //    newrec.Add(tempbarang);
                    //}
                    #endregion
                    var bindGetVar = getvariationproduct(productid, detail.Skus, newrec, idmarket, detail, apidata, kdmp);
                    if (bindGetVar.exception == 1)
                    {
                        ret.exception = 1;
                    }
                    ret.recordCount += bindGetVar.recordCount;

                }
            }
            catch (Exception ex)
            {
                ret.exception = 1;
            }
            return ret;
        }

        public BindingBase getvariationproduct(string productid, List<SkuTik> skudata, List<TEMP_BRG_MP> newrec, int idmarket, DetailProductTik detail, TTApiData iden, string sellersku = null)
        {
            var ret = new BindingBase();
            int insertJml = 0;
            newrec = new List<TEMP_BRG_MP>();
            try
            {
                foreach (SkuTik sku in skudata)
                {
                    string kdbrgvar = productid + ";" + sku.Id;
                    var checkstf02h = ErasoftDbContext.STF02H.SingleOrDefault(x => x.BRG_MP == kdbrgvar && x.IDMARKET == idmarket);
                    var checkTemp = ErasoftDbContext.TEMP_BRG_MP.SingleOrDefault(x => x.BRG_MP == kdbrgvar && x.IDMARKET == idmarket);
                    if (checkstf02h == null && checkTemp == null)
                    {
                        string namabarang = detail.ProductName;

                        TEMP_BRG_MP tempbarang = new TEMP_BRG_MP();
                        tempbarang.BRG_MP = kdbrgvar;

                        foreach (SalesAttributeTik satik in sku.SalesAttributes)
                        {
                            if (tempbarang.ACODE_1 == null)
                            {
                                tempbarang.ACODE_1 = satik.Id;
                                tempbarang.ANAME_1 = satik.Name;
                                tempbarang.AVALUE_1 = satik.ValueName;
                            }
                            else if (tempbarang.ACODE_2 == null)
                            {
                                tempbarang.ACODE_2 = satik.Id;
                                tempbarang.ANAME_2 = satik.Name;
                                tempbarang.AVALUE_2 = satik.ValueName;
                            }
                            namabarang += " " + satik.ValueName;
                            if (string.IsNullOrEmpty(tempbarang.IMAGE))
                            {
                                if (satik.SkuImg != null)
                                {
                                    if (satik.SkuImg.UrlList != null)
                                    {
                                        tempbarang.IMAGE = satik.SkuImg.UrlList[0];
                                    }
                                }
                            }
                            if(skudata.Count == 1)
                            {
                                if (string.IsNullOrEmpty(tempbarang.AVALUE_50))
                                {
                                    if (satik.SkuImg != null)
                                    {
                                        if (satik.SkuImg.UrlList != null)
                                        {
                                            tempbarang.AVALUE_50 = satik.SkuImg.UrlList[0];
                                        }
                                    }
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(tempbarang.IMAGE) && skudata.Count == 1)// non varian tidak set gambar
                        {
                            if (detail.Images != null)
                            {
                                if (tempbarang.IMAGE == null)
                                {
                                    tempbarang.IMAGE = detail.Images[0].UrlList[0];
                                }
                            }

                        }
                        var splitItemName = new StokControllerJob().SplitItemName(namabarang.Replace('\'', '`'));
                        var nama = splitItemName[0];
                        var nama2 = splitItemName[1];

                        if (detail.product_attributes != null)
                        {
                            foreach (var prodAttr in detail.product_attributes)
                            {
                                if (tempbarang.ACODE_3 == null)
                                {
                                    tempbarang.ACODE_3 = prodAttr.Id;
                                    tempbarang.ANAME_3 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_3))
                                        {
                                            tempbarang.AVALUE_3 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_3 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_3 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_4 == null)
                                {
                                    tempbarang.ACODE_4 = prodAttr.Id;
                                    tempbarang.ANAME_4 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_4))
                                        {
                                            tempbarang.AVALUE_4 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_4 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_4 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_5 == null)
                                {
                                    tempbarang.ACODE_5 = prodAttr.Id;
                                    tempbarang.ANAME_5 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_5))
                                        {
                                            tempbarang.AVALUE_5 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_5 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_5 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_6 == null)
                                {
                                    tempbarang.ACODE_6 = prodAttr.Id;
                                    tempbarang.ANAME_6 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_6))
                                        {
                                            tempbarang.AVALUE_6 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_6 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_6 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_7 == null)
                                {
                                    tempbarang.ACODE_7 = prodAttr.Id;
                                    tempbarang.ANAME_7 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_7))
                                        {
                                            tempbarang.AVALUE_7 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_7 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_7 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_8 == null)
                                {
                                    tempbarang.ACODE_8 = prodAttr.Id;
                                    tempbarang.ANAME_8 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_8))
                                        {
                                            tempbarang.AVALUE_8 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_8 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_8 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_9 == null)
                                {
                                    tempbarang.ACODE_9 = prodAttr.Id;
                                    tempbarang.ANAME_9 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_9))
                                        {
                                            tempbarang.AVALUE_9 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_9 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_9 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_10 == null)
                                {
                                    tempbarang.ACODE_10 = prodAttr.Id;
                                    tempbarang.ANAME_10 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_10))
                                        {
                                            tempbarang.AVALUE_10 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_10 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_10 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_11 == null)
                                {
                                    tempbarang.ACODE_11 = prodAttr.Id;
                                    tempbarang.ANAME_11 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_11))
                                        {
                                            tempbarang.AVALUE_11 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_11 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_11 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_12 == null)
                                {
                                    tempbarang.ACODE_12 = prodAttr.Id;
                                    tempbarang.ANAME_12 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_12))
                                        {
                                            tempbarang.AVALUE_12 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_12 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_12 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_13 == null)
                                {
                                    tempbarang.ACODE_13 = prodAttr.Id;
                                    tempbarang.ANAME_13 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_13))
                                        {
                                            tempbarang.AVALUE_13 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_13 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_13 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_14 == null)
                                {
                                    tempbarang.ACODE_14 = prodAttr.Id;
                                    tempbarang.ANAME_14 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_14))
                                        {
                                            tempbarang.AVALUE_14 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_14 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_14 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_15 == null)
                                {
                                    tempbarang.ACODE_15 = prodAttr.Id;
                                    tempbarang.ANAME_15 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_15))
                                        {
                                            tempbarang.AVALUE_15 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_15 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_15 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_16 == null)
                                {
                                    tempbarang.ACODE_16 = prodAttr.Id;
                                    tempbarang.ANAME_16 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_16))
                                        {
                                            tempbarang.AVALUE_16 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_16 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_16 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_17 == null)
                                {
                                    tempbarang.ACODE_17 = prodAttr.Id;
                                    tempbarang.ANAME_17 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_17))
                                        {
                                            tempbarang.AVALUE_17 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_17 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_17 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_18 == null)
                                {
                                    tempbarang.ACODE_18 = prodAttr.Id;
                                    tempbarang.ANAME_18 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_18))
                                        {
                                            tempbarang.AVALUE_18 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_18 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_18 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_19 == null)
                                {
                                    tempbarang.ACODE_19 = prodAttr.Id;
                                    tempbarang.ANAME_19 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_19))
                                        {
                                            tempbarang.AVALUE_19 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_19 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_19 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_20 == null)
                                {
                                    tempbarang.ACODE_20 = prodAttr.Id;
                                    tempbarang.ANAME_20 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_20))
                                        {
                                            tempbarang.AVALUE_20 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_20 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_20 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_21 == null)
                                {
                                    tempbarang.ACODE_21 = prodAttr.Id;
                                    tempbarang.ANAME_21 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_21))
                                        {
                                            tempbarang.AVALUE_21 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_21 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_21 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_22 == null)
                                {
                                    tempbarang.ACODE_22 = prodAttr.Id;
                                    tempbarang.ANAME_22 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_22))
                                        {
                                            tempbarang.AVALUE_22 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_22 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_22 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_23 == null)
                                {
                                    tempbarang.ACODE_23 = prodAttr.Id;
                                    tempbarang.ANAME_23 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_23))
                                        {
                                            tempbarang.AVALUE_23 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_23 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_23 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_24 == null)
                                {
                                    tempbarang.ACODE_24 = prodAttr.Id;
                                    tempbarang.ANAME_24 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_24))
                                        {
                                            tempbarang.AVALUE_24 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_24 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_24 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_25 == null)
                                {
                                    tempbarang.ACODE_25 = prodAttr.Id;
                                    tempbarang.ANAME_25 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_25))
                                        {
                                            tempbarang.AVALUE_25 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_25 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_25 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_26 == null)
                                {
                                    tempbarang.ACODE_26 = prodAttr.Id;
                                    tempbarang.ANAME_26 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_26))
                                        {
                                            tempbarang.AVALUE_26 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_26 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_26 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_27 == null)
                                {
                                    tempbarang.ACODE_27 = prodAttr.Id;
                                    tempbarang.ANAME_27 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_27))
                                        {
                                            tempbarang.AVALUE_27 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_27 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_27 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_28 == null)
                                {
                                    tempbarang.ACODE_28 = prodAttr.Id;
                                    tempbarang.ANAME_28 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_28))
                                        {
                                            tempbarang.AVALUE_28 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_28 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_28 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_29 == null)
                                {
                                    tempbarang.ACODE_29 = prodAttr.Id;
                                    tempbarang.ANAME_29 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_29))
                                        {
                                            tempbarang.AVALUE_29 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_29 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_29 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                                else if (tempbarang.ACODE_30 == null)
                                {
                                    tempbarang.ACODE_30 = prodAttr.Id;
                                    tempbarang.ANAME_30 = prodAttr.name.Replace('\'', '`');
                                    foreach (var attrVal in prodAttr.values)
                                    {
                                        if (!string.IsNullOrEmpty(tempbarang.AVALUE_30))
                                        {
                                            tempbarang.AVALUE_30 += ",";
                                        }
                                        if ((attrVal.Id ?? "").Length > 10)
                                        {
                                            tempbarang.AVALUE_30 += attrVal.name.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            tempbarang.AVALUE_30 += attrVal.Id.Replace('\'', '`');
                                        }
                                    }
                                }
                            }

                        }
                        #region old attr
                        //foreach (SalesAttributeTik satik in sku.SalesAttributes)
                        //{
                        //    if (tempbarang.ACODE_1 == null)
                        //    {
                        //        tempbarang.ACODE_1 = satik.Id;
                        //        tempbarang.ANAME_1 = satik.Name;
                        //        tempbarang.AVALUE_1 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_2 == null)
                        //    {
                        //        tempbarang.ACODE_2 = satik.Id;
                        //        tempbarang.ANAME_2 = satik.Name;
                        //        tempbarang.AVALUE_2 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_3 == null)
                        //    {
                        //        tempbarang.ACODE_3 = satik.Id;
                        //        tempbarang.ANAME_3 = satik.Name;
                        //        tempbarang.AVALUE_3 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_4 == null)
                        //    {
                        //        tempbarang.ACODE_4 = satik.Id;
                        //        tempbarang.ANAME_4 = satik.Name;
                        //        tempbarang.AVALUE_4 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_5 == null)
                        //    {
                        //        tempbarang.ACODE_5 = satik.Id;
                        //        tempbarang.ANAME_5 = satik.Name;
                        //        tempbarang.AVALUE_5 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_6 == null)
                        //    {
                        //        tempbarang.ACODE_6 = satik.Id;
                        //        tempbarang.ANAME_6 = satik.Name;
                        //        tempbarang.AVALUE_6 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_7 == null)
                        //    {
                        //        tempbarang.ACODE_7 = satik.Id;
                        //        tempbarang.ANAME_7 = satik.Name;
                        //        tempbarang.AVALUE_7 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_8 == null)
                        //    {
                        //        tempbarang.ACODE_8 = satik.Id;
                        //        tempbarang.ANAME_8 = satik.Name;
                        //        tempbarang.AVALUE_8 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_9 == null)
                        //    {
                        //        tempbarang.ACODE_9 = satik.Id;
                        //        tempbarang.ANAME_9 = satik.Name;
                        //        tempbarang.AVALUE_9 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_10 == null)
                        //    {
                        //        tempbarang.ACODE_10 = satik.Id;
                        //        tempbarang.ANAME_10 = satik.Name;
                        //        tempbarang.AVALUE_10 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_11 == null)
                        //    {
                        //        tempbarang.ACODE_11 = satik.Id;
                        //        tempbarang.ANAME_11 = satik.Name;
                        //        tempbarang.AVALUE_11 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_12 == null)
                        //    {
                        //        tempbarang.ACODE_12 = satik.Id;
                        //        tempbarang.ANAME_12 = satik.Name;
                        //        tempbarang.AVALUE_12 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_13 == null)
                        //    {
                        //        tempbarang.ACODE_13 = satik.Id;
                        //        tempbarang.ANAME_13 = satik.Name;
                        //        tempbarang.AVALUE_13 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_14 == null)
                        //    {
                        //        tempbarang.ACODE_14 = satik.Id;
                        //        tempbarang.ANAME_14 = satik.Name;
                        //        tempbarang.AVALUE_14 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_15 == null)
                        //    {
                        //        tempbarang.ACODE_15 = satik.Id;
                        //        tempbarang.ANAME_15 = satik.Name;
                        //        tempbarang.AVALUE_15 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_16 == null)
                        //    {
                        //        tempbarang.ACODE_16 = satik.Id;
                        //        tempbarang.ANAME_16 = satik.Name;
                        //        tempbarang.AVALUE_16 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_17 == null)
                        //    {
                        //        tempbarang.ACODE_17 = satik.Id;
                        //        tempbarang.ANAME_17 = satik.Name;
                        //        tempbarang.AVALUE_17 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_18 == null)
                        //    {
                        //        tempbarang.ACODE_18 = satik.Id;
                        //        tempbarang.ANAME_18 = satik.Name;
                        //        tempbarang.AVALUE_18 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_19 == null)
                        //    {
                        //        tempbarang.ACODE_19 = satik.Id;
                        //        tempbarang.ANAME_19 = satik.Name;
                        //        tempbarang.AVALUE_19 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_20 == null)
                        //    {
                        //        tempbarang.ACODE_20 = satik.Id;
                        //        tempbarang.ANAME_20 = satik.Name;
                        //        tempbarang.AVALUE_20 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_21 == null)
                        //    {
                        //        tempbarang.ACODE_21 = satik.Id;
                        //        tempbarang.ANAME_21 = satik.Name;
                        //        tempbarang.AVALUE_21 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_22 == null)
                        //    {
                        //        tempbarang.ACODE_22 = satik.Id;
                        //        tempbarang.ANAME_22 = satik.Name;
                        //        tempbarang.AVALUE_22 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_23 == null)
                        //    {
                        //        tempbarang.ACODE_23 = satik.Id;
                        //        tempbarang.ANAME_23 = satik.Name;
                        //        tempbarang.AVALUE_23 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_24 == null)
                        //    {
                        //        tempbarang.ACODE_24 = satik.Id;
                        //        tempbarang.ANAME_24 = satik.Name;
                        //        tempbarang.AVALUE_24 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_25 == null)
                        //    {
                        //        tempbarang.ACODE_25 = satik.Id;
                        //        tempbarang.ANAME_25 = satik.Name;
                        //        tempbarang.AVALUE_25 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_26 == null)
                        //    {
                        //        tempbarang.ACODE_26 = satik.Id;
                        //        tempbarang.ANAME_26 = satik.Name;
                        //        tempbarang.AVALUE_26 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_27 == null)
                        //    {
                        //        tempbarang.ACODE_27 = satik.Id;
                        //        tempbarang.ANAME_27 = satik.Name;
                        //        tempbarang.AVALUE_27 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_28 == null)
                        //    {
                        //        tempbarang.ACODE_28 = satik.Id;
                        //        tempbarang.ANAME_28 = satik.Name;
                        //        tempbarang.AVALUE_28 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_29 == null)
                        //    {
                        //        tempbarang.ACODE_29 = satik.Id;
                        //        tempbarang.ANAME_29 = satik.Name;
                        //        tempbarang.AVALUE_29 = satik.ValueName;
                        //    }
                        //    else if (tempbarang.ACODE_30 == null)
                        //    {
                        //        tempbarang.ACODE_30 = satik.Id;
                        //        tempbarang.ANAME_30 = satik.Name;
                        //        tempbarang.AVALUE_30 = satik.ValueName;
                        //    }

                        //}
                        #endregion
                        tempbarang.NAMA = nama;
                        tempbarang.NAMA2 = nama2;
                        tempbarang.BERAT = detail.PackageWeight == "" ? double.Parse(0.ToString()) : (double.Parse(detail.PackageWeight) * 1000);//berat dalam kg
                        tempbarang.PANJANG = detail.PackageLength == 0 ? double.Parse(0.ToString()) : double.Parse(detail.PackageLength.ToString());
                        tempbarang.LEBAR = detail.PackageWidth == 0 ? double.Parse(0.ToString()) : double.Parse(detail.PackageWidth.ToString());
                        tempbarang.TINGGI = detail.PackageHeight == 0 ? double.Parse(0.ToString()) : double.Parse(detail.PackageHeight.ToString());
                        //tempbarang.CATEGORY_CODE = detail.CategoryList[2].Id;
                        tempbarang.CATEGORY_CODE = detail.CategoryList[0].Id;
                        tempbarang.CATEGORY_NAME = detail.CategoryList[0].LocalDisplayName;
                        foreach (var kat in detail.CategoryList)
                        {
                            if (kat.IsLeaf)
                            {
                                tempbarang.CATEGORY_CODE = kat.Id;
                                tempbarang.CATEGORY_NAME = kat.LocalDisplayName;
                            }
                        }
                        tempbarang.HJUAL = double.Parse(sku.Price.OriginalPrice.ToString());
                        tempbarang.HJUAL_MP = double.Parse(sku.Price.OriginalPrice.ToString());
                        //tempbarang.CATEGORY_NAME = detail.CategoryList[2].LocalDisplayName;
                        tempbarang.Deskripsi = detail.Description;
                        tempbarang.IDMARKET = int.Parse(idmarket.ToString());
                        tempbarang.CUST = iden.no_cust;
                        tempbarang.SELLER_SKU = sku.SellerSku;
                        tempbarang.AVALUE_45 = namabarang;
                        tempbarang.ACODE_31 = "warranty";
                        tempbarang.ANAME_31 = "Warranty Period";
                        //tempbarang.AVALUE_31 = detail.WarrantyPeriod.WarrantyDescription;
                        tempbarang.AVALUE_31 = detail.WarrantyPeriod.WarrantyId.ToString();
                        tempbarang.ACODE_32 = "product_warranty";
                        tempbarang.ANAME_32 = "Warranty Policy";
                        tempbarang.AVALUE_32 = detail.WarrantyPolicy;
                        tempbarang.MEREK = detail.Brand == null ? "No Brand" : detail.Brand.Name;
                        tempbarang.ANAME_38 = detail.Brand == null ? "" : detail.Brand.Id;
                        tempbarang.AVALUE_38 = tempbarang.MEREK;
                        tempbarang.AVALUE_34 = "https://shop.tiktok.com/view/product/" + productid;
                        tempbarang.DISPLAY = (detail.ProductStatus == 4 ? true : false);
                        tempbarang.KODE_BRG_INDUK = sellersku == null ? productid + ";0" : sellersku;
                        tempbarang.AVALUE_39 = (detail.IsCodOpen ? "1" : "0");
                        if (skudata.Count == 1)
                        {
                            tempbarang.KODE_BRG_INDUK = "";
                        };
                        tempbarang.TYPE = "3";
                        tempbarang.PICKUP_POINT = sku.StockInfos[0].WarehouseId;
                        insertJml++;
                        newrec.Add(tempbarang);
                    }
                    #region remark
                    //else
                    //{

                    //    TEMP_BRG_MP tempbarang = new TEMP_BRG_MP();
                    //    tempbarang.BRG_MP = kdbrgvar;
                    //    string namabarang = detail.ProductName;
                    //    foreach (SalesAttributeTik sat in sku.SalesAttributes)
                    //    {
                    //        namabarang += " " + sat.ValueName;
                    //        tempbarang.IMAGE = sat.SkuImg == null ? null : sat.SkuImg.UrlList[0];
                    //        if (tempbarang.ACODE_1 == null)
                    //        {
                    //            tempbarang.ACODE_1 = sat.Id;
                    //            tempbarang.ANAME_1 = sat.Name;
                    //            tempbarang.AVALUE_1 = sat.ValueName;
                    //        }
                    //        else if (tempbarang.ACODE_2 == null)
                    //        {
                    //            tempbarang.ACODE_2 = sat.Id;
                    //            tempbarang.ANAME_2 = sat.Name;
                    //            tempbarang.AVALUE_2 = sat.ValueName;
                    //        }
                    //        else if (tempbarang.ACODE_3 == null)
                    //        {
                    //            tempbarang.ACODE_3 = sat.Id;
                    //            tempbarang.ANAME_3 = sat.Name;
                    //            tempbarang.AVALUE_3 = sat.ValueName;
                    //        }
                    //        else if (tempbarang.ACODE_4 == null)
                    //        {
                    //            tempbarang.ACODE_4 = sat.Id;
                    //            tempbarang.ANAME_4 = sat.Name;
                    //            tempbarang.AVALUE_4 = sat.ValueName;
                    //        }
                    //        else if (tempbarang.ACODE_5 == null)
                    //        {
                    //            tempbarang.ACODE_5 = sat.Id;
                    //            tempbarang.ANAME_5 = sat.Name;
                    //            tempbarang.AVALUE_5 = sat.ValueName;
                    //        }
                    //    }
                    //    tempbarang.NAMA = namabarang;
                    //    tempbarang.BERAT = detail.PackageWeight == "" ? double.Parse(0.ToString()) : double.Parse(detail.PackageWeight);
                    //    tempbarang.PANJANG = detail.PackageLength == 0 ? double.Parse(0.ToString()) : double.Parse(detail.PackageLength.ToString());
                    //    tempbarang.LEBAR = detail.PackageWidth == 0 ? double.Parse(0.ToString()) : double.Parse(detail.PackageWidth.ToString());
                    //    tempbarang.TINGGI = detail.PackageHeight == 0 ? double.Parse(0.ToString()) : double.Parse(detail.PackageHeight.ToString());
                    //    tempbarang.CATEGORY_CODE = detail.CategoryList[2].Id;
                    //    tempbarang.HJUAL = double.Parse(sku.Price.OriginalPrice.ToString());
                    //    tempbarang.HJUAL_MP = double.Parse(sku.Price.OriginalPrice.ToString());
                    //    tempbarang.CATEGORY_NAME = detail.CategoryList[2].LocalDisplayName;
                    //    tempbarang.Deskripsi = detail.Description;
                    //    tempbarang.IDMARKET = int.Parse(idmarket.ToString());
                    //    tempbarang.CUST = iden.no_cust;
                    //    tempbarang.SELLER_SKU = checkstf02h.BRG;
                    //    tempbarang.AVALUE_45 = namabarang;
                    //    tempbarang.ACODE_9 = "warranty";
                    //    tempbarang.ANAME_9 = "Warranty Period";
                    //    tempbarang.AVALUE_9 = detail.WarrantyPeriod.WarrantyDescription;
                    //    tempbarang.ACODE_11 = "product_warranty";
                    //    tempbarang.ANAME_11 = "Warranty Policy";
                    //    tempbarang.AVALUE_11 = detail.WarrantyPolicy;
                    //    tempbarang.MEREK = detail.Brand == null ? "No Brand" : detail.Brand.Name;
                    //    tempbarang.DISPLAY = true;
                    //    tempbarang.KODE_BRG_INDUK = sellersku;
                    //    tempbarang.TYPE = "3";
                    //    ret.recordCount++;
                    //    newrec.Add(tempbarang);
                    //}
                    #endregion
                }
            }
            catch (Exception ex)
            {
                ret.exception = 1;
            }
            if (insertJml > 0)
            {
                try
                {
                    ErasoftDbContext.TEMP_BRG_MP.AddRange(newrec);
                    ErasoftDbContext.SaveChanges();
                    ret.recordCount += insertJml;
                }
                catch (Exception ex)
                {
                    ret.exception = 1;
                }
            }
            return ret;
        }
        #endregion
        #endregion
        #endregion


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

        #region helper

        public static long CurrentTimeSecond()
        {
            //        return (long)DateTime.Now.ToUniversalTime().Subtract(
            //new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            //).TotalMilliseconds;
            return (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        public static string XmlEscape(string unescaped)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerXml;
        }

        public static string XmlUnescape(string escaped)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerXml = escaped;
            return node.InnerText;
        }

        public enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4
        }
        public void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, string iden, API_LOG_MARKETPLACE data)
        {
            switch (action)
            {
                case api_status.Pending:
                    {
                        var arf01 = ErasoftDbContext.ARF01.Where(p => p.CUST == iden).FirstOrDefault();
                        var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                        {
                            CUST = arf01 != null ? arf01.CUST : "",
                            CUST_ATTRIBUTE_1 = arf01 != null ? arf01.PERSO : "",
                            CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                            CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                            CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                            CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                            MARKETPLACE = "TikTok",
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
                        var apiLogInDb = ErasoftDbContext.API_LOG_MARKETPLACE.FirstOrDefault(p => p.REQUEST_ID == data.REQUEST_ID);
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
        #endregion
    }

    #region Model

    public class TiktokCategoryRuleRespose : TiktokCommonResponse
    {
        public TiktokCategoryRuleData data { get; set; }
    }

    public class TiktokCategoryRuleData
    {
        public List<Category_Rules> category_rules { get; set; }
    }

    public class Category_Rules
    {
        public List<TiktokCategoryRuleProduct_Certifications> product_certifications { get; set; }
        public bool support_size_chart { get; set; }
        public bool support_cod { get; set; }
    }

    public class TiktokCategoryRuleProduct_Certifications
    {
        public string name { get; set; }
        public string id { get; set; }
        public string sample { get; set; }
        public bool is_mandatory { get; set; }
    }
    public class TiktokGetWarehouseResponse : TiktokCommonResponse
    {
        public TiktokGetWarehouseResponseData data { get; set; }

    }
    public class TiktokGetWarehouseResponseData
    {
        public List<TiktokWarehouse> warehouse_list { get; set; }
    }

    public class TiktokWarehouse
    {
        //public Warehouse_Address warehouse_address { get; set; }
        public int warehouse_effect_status { get; set; }
        public string warehouse_id { get; set; }
        public string warehouse_name { get; set; }
        public int warehouse_sub_type { get; set; }
        public int warehouse_type { get; set; }
    }

    public class Warehouse_Address
    {
        public string city { get; set; }
        public string contact_person { get; set; }
        public string district { get; set; }
        public string full_address { get; set; }
        public string phone { get; set; }
        public string region { get; set; }
        public string region_code { get; set; }
        public string state { get; set; }
        public string town { get; set; }
        public string zipcode { get; set; }
    }

    public class TiktokGetBrandResponse : TiktokCommonResponse
    {
        public TiktokGetBrandResponseData data { get; set; }

    }
    public class TiktokGetBrandResponseData
    {
        public List<TiktokBrand> brand_list { get; set; }
    }
    public class TiktokBrand
    {
        public string id { get; set; }
        public string name { get; set; }
    }
    public class TiktokGetShipmentResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public string request_id { get; set; }
        public TiktokGetShipmentData data { get; set; }
    }

    public class TiktokGetShipmentData
    {
        public Delivery_Option_List[] delivery_option_list { get; set; }
    }

    public class Delivery_Option_List
    {
        public string delivery_option_id { get; set; }
        public string delivery_option_name { get; set; }
        public Item_Weight_Limit item_weight_limit { get; set; }
        public Item_Dimension_Limit item_dimension_limit { get; set; }
        public Shipping_Provider_List[] shipping_provider_list { get; set; }
    }

    public class Item_Weight_Limit
    {
        public int max_weight { get; set; }
        public int min_weight { get; set; }
    }

    public class Item_Dimension_Limit
    {
        public int length { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Shipping_Provider_List
    {
        public string shipping_provider_id { get; set; }
        public string shipping_provider_name { get; set; }
    }

    public class DataTiktok
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("access_token_expire_in")]
        public int AccessTokenExpireIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("refresh_token_expire_in")]
        public int RefreshTokenExpireIn { get; set; }

        [JsonProperty("open_id")]
        public string OpenId { get; set; }

        [JsonProperty("seller_name")]
        public string SellerName { get; set; }
    }

    public class TiktokAuth
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public DataTiktok Data { get; set; }
    }

    public class PostTiktokApi
    {
        [JsonProperty("app_key")]
        public string app_key { get; set; }
        [JsonProperty("app_secret")]
        public string app_secret { get; set; }
        [JsonProperty("auth_code")]
        public string auth_code { get; set; }
        [JsonProperty("grant_type")]
        public string grant_type { get; set; }
    }

    public class PostTiktokApiRef
    {
        [JsonProperty("app_key")]
        public string app_key { get; set; }
        [JsonProperty("app_secret")]
        public string app_secret { get; set; }
        [JsonProperty("refresh_token")]
        public string refresh_token { get; set; }
        [JsonProperty("grant_type")]
        public string grant_type { get; set; }
    }

    public class ShopListTiktok
    {
        [JsonProperty("shop_id")]
        public string ShopId { get; set; }

        [JsonProperty("shop_name")]
        public string ShopName { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class DataTiktokShopList
    {
        [JsonProperty("shop_list")]
        public List<ShopListTiktok> ShopList { get; set; }
    }

    public class ShopListRes
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("request_id")]
        public string RequestId { get; set; }

        [JsonProperty("data")]
        public DataTiktokShopList Data { get; set; }
    }

    public class TTApiData
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string shop_id { get; set; }
        public string DatabasePathErasoft { get; set; }
        public string no_cust { get; set; }
        public string username { get; set; }
        public DateTime expired_date { get; set; }
    }

    public class PriceTik
    {
        [JsonProperty("original_price")]
        public string OriginalPrice { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }

    public class StockInfoTik
    {
        [JsonProperty("warehouse_id")]
        public string WarehouseId { get; set; }

        [JsonProperty("available_stock")]
        public int AvailableStock { get; set; }
    }

    public class SalesAttributeTik
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value_id")]
        public string ValueId { get; set; }

        [JsonProperty("value_name")]
        public string ValueName { get; set; }

        [JsonProperty("sku_img")]
        public SkuImgTik SkuImg { get; set; }
    }

    public class SkuImgTik
    {
        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("thumb_url_list")]
        public List<string> ThumbUrlList { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url_list")]
        public List<string> UrlList { get; set; }
    }


    public class SkuTik
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("seller_sku")]
        public string SellerSku { get; set; }

        [JsonProperty("price")]
        public PriceTik Price { get; set; }

        [JsonProperty("stock_infos")]
        public List<StockInfoTik> StockInfos { get; set; }
        [JsonProperty("sales_attributes")]
        public List<SalesAttributeTik> SalesAttributes { get; set; }
        [JsonProperty("sku_image")]
        public SkuImgTik skuimage { get; set; }
    }

    public class BrandTik
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }
    }
    public class ImageTik
    {
        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("thumb_url_list")]
        public List<string> ThumbUrlList { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url_list")]
        public List<string> UrlList { get; set; }
    }

    public class ProductTick
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("sale_regions")]
        public List<string> SaleRegions { get; set; }

        [JsonProperty("skus")]
        public List<SkuTik> Skus { get; set; }
    }

    public class WarrantyPeriodTick
    {
        [JsonProperty("warranty_id")]
        public int WarrantyId { get; set; }

        [JsonProperty("warranty_description")]
        public string WarrantyDescription { get; set; }
    }
    public class DetailProductTik
    {
        [JsonProperty("product_id")]
        public string ProductId { get; set; }

        [JsonProperty("product_status")]
        public int ProductStatus { get; set; }

        [JsonProperty("product_name")]
        public string ProductName { get; set; }

        [JsonProperty("category_list")]
        public List<CategoryListTik> CategoryList { get; set; }

        [JsonProperty("brand")]
        public BrandTik Brand { get; set; }

        [JsonProperty("images")]
        public List<ImageTik> Images { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("warranty_period")]
        public WarrantyPeriodTick WarrantyPeriod { get; set; }

        [JsonProperty("warranty_policy")]
        public string WarrantyPolicy { get; set; }

        [JsonProperty("package_length")]
        public int PackageLength { get; set; }

        [JsonProperty("package_width")]
        public int PackageWidth { get; set; }

        [JsonProperty("package_height")]
        public int PackageHeight { get; set; }

        [JsonProperty("package_weight")]
        public string PackageWeight { get; set; }

        [JsonProperty("skus")]
        public List<SkuTik> Skus { get; set; }

        [JsonProperty("product_certifications")]
        public List<ProductCertificationTik> ProductCertifications { get; set; }

        [JsonProperty("is_cod_open")]
        public bool IsCodOpen { get; set; }

        [JsonProperty("update_time")]
        public long UpdateTime { get; set; }
        public List<Attribute_Tik> product_attributes { get; set; }
    }

    public class Attribute_Tik
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("values")]
        public List<Attribute_Value_Tik> values { get; set; }

    }
    public class Attribute_Value_Tik
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

    }
    public class ProductCertificationTik
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("images")]
        public List<ImageTik> Images { get; set; }

        [JsonProperty("files")]
        public List<FileTik> Files { get; set; }
    }

    public class FileTik
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("list")]
        public List<string> List { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }


    public class TiktokProd
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("products")]
        public List<ProductTick> Products { get; set; }

        [JsonProperty("category_list")]
        public List<CategoryListTik> CategoryList { get; set; }

    }

    public class ResProd
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("request_id")]
        public string RequestId { get; set; }

        [JsonProperty("data")]
        public TiktokProd Data { get; set; }
    }

    public class ResProductDet
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("request_id")]
        public string RequestId { get; set; }
        [JsonProperty("data")]
        public DetailProductTik productTik { get; set; }
    }
    public class ProductParameter
    {
        [JsonProperty("page_size")]
        public int PageSize { get; set; }

        [JsonProperty("page_number")]
        public int PageNumber { get; set; }

        [JsonProperty("search_status")]
        public int SearchStatus { get; set; }

        [JsonProperty("seller_sku_list")]
        public List<string> SellerSkuList { get; set; }
    }

    public class CategoryListTik
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("parent_id")]
        public string ParentId { get; set; }

        [JsonProperty("local_display_name")]
        public string LocalDisplayName { get; set; }

        [JsonProperty("is_leaf")]
        public bool IsLeaf { get; set; }
    }

    public class StockUpdateTik
    {
        [JsonProperty("product_id")]
        public string ProductId { get; set; }

        [JsonProperty("skus")]
        public List<SkuTik> Skus { get; set; }
    }

    #endregion
}