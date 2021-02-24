using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MasterOnline.Models;
using System.Text;
using System.Data;
using MasterOnline.ViewModels;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Erasoft.Function;
using System.Data.SqlClient;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Web.Util;
using RestSharp;

namespace MasterOnline.Controllers
{
    public class BukaLapakController : Controller
    {
        // GET: BukaLapak
        AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        DatabaseSQL EDB;
        MoDbContext MoDbContext;
        public ErasoftContext ErasoftDbContext { get; set; }

#if AWS
        private static string callBackUrl = "https://masteronline.co.id/bukalapak/auth";
        private static string client_id = "GovVusRdl0QwJCXu1F0th5lezoFYvVIW4XHv4U1M05U";
        private static string client_secret = "osqzx8n3y3YRJ0vydm_8qOZ9N9f95EvrZSvTFtKQCzM";
#else
        private static string callBackUrl = "https://dev.masteronline.co.id/bukalapak/auth";
        private static string client_id = "laJXb5jh91BelPQg2VmE2ooa58UVJmlJkNq98EPJc6s";
        private static string client_secret = "AXe5u7JcYiSNLvOsGW92Dzc4li6mbrWpN9qjlLD4OxI";
#endif
        string dbSourceEra = "";

        public BukaLapakController()
        {
            MoDbContext = new MoDbContext("");
            if (sessionData?.Account != null)
            {
                if (sessionData.Account.UserId == "admin_manage")
                {
                    ErasoftDbContext = new ErasoftContext();
                }
                else
                {
#if (Debug_AWS)
                    dbSourceEra = sessionData.Account.DataSourcePathDebug;
#else
                    dbSourceEra = sessionData.Account.DataSourcePath;
#endif
                    ErasoftDbContext = new ErasoftContext(dbSourceEra, sessionData.Account.DatabasePathErasoft);
                }
                
                EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);

            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
#if (Debug_AWS)
                    dbSourceEra = accFromUser.DataSourcePathDebug;
#else
                    dbSourceEra = accFromUser.DataSourcePath;
#endif
                    ErasoftDbContext = new ErasoftContext(dbSourceEra, accFromUser.DatabasePathErasoft);
                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                }
            }
        }
        [HttpGet]
        public string BukalapakAuth(string cust)
        {
            string userId = "";
            if (sessionData?.Account != null)
            {
                userId = sessionData.Account.DatabasePathErasoft;

            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    userId = accFromUser.DatabasePathErasoft;
                }
            }
            var dataToken = MoDbContext.BUKALAPAK_TOKEN.Where(m => m.ACCOUNT == userId && m.CUST == cust).FirstOrDefault();
            var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == cust).FirstOrDefault();
            if (dataToken == null)
            {
                //var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == cust).FirstOrDefault();
                dataToken = new BUKALAPAK_TOKEN();
                dataToken.ACCOUNT = userId;
                dataToken.CUST = cust;
                dataToken.CODE = "";
                dataToken.EMAIL = customer.EMAIL;
                dataToken.CREATED_AT = DateTime.UtcNow.AddHours(7);
                MoDbContext.BUKALAPAK_TOKEN.Add(dataToken);
                MoDbContext.SaveChanges();
            }
            else
            {
                dataToken.EMAIL = customer.EMAIL;
                dataToken.CREATED_AT = DateTime.UtcNow.AddHours(7);
                MoDbContext.SaveChanges();

            }
            string lzdId = cust;
            //string compUrl = callBackUrl + userId + "_param_" + cust;
            string scope = "public user store";
            string uri = "https://accounts.bukalapak.com/oauth/authorize?client_id=" + client_id + "&scope=" + Uri.EscapeDataString(scope)
                + "&response_type=code" + "&redirect_uri=" + Uri.EscapeDataString(callBackUrl);
            return uri;
        }

        [Route("bukalapak/auth")]
        [HttpGet]
        public async Task<ActionResult> BukalapakCode(string code)
        {
            AccessKeyBL retGetToken = new AccessKeyBL();
            #region get token
            var urll = ("https://accounts.bukalapak.com/oauth/token");

            var client = new RestClient(urll);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("client_id", client_id);
            request.AddParameter("client_secret", client_secret);
            request.AddParameter("code", code);
            request.AddParameter("redirect_uri", callBackUrl);
            string stringRet = "";
            try
            {
                IRestResponse response = client.Execute(request);
                stringRet = response.Content;
            }
            catch (WebException e)
            {
                string err = "";
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                }

                //var ex3 = new BUKALAPAK_TOKEN();
                //ex3.ACCOUNT = DateTime.UtcNow.AddHours(7).ToString("yyyyMMdd") + "3";
                //ex3.CUST = DateTime.UtcNow.AddHours(7).ToString("HHmmss");
                //ex3.CODE = err;
                //ex3.EMAIL = "qc-failtoken";
                //ex3.CREATED_AT = DateTime.UtcNow.AddHours(7);
                //MoDbContext.BUKALAPAK_TOKEN.Add(ex3);
                //MoDbContext.SaveChanges();
            }

            if (!string.IsNullOrEmpty(stringRet))
            {
                try
                {
                    //var ex22 = new BUKALAPAK_TOKEN();
                    //ex22.ACCOUNT = DateTime.UtcNow.AddHours(7).ToString("yyyyMMdd") + "1";
                    //ex22.CUST = DateTime.UtcNow.AddHours(7).ToString("HHmmss");
                    //ex22.CODE = stringRet;
                    //ex22.EMAIL = "qc-successtoken";
                    //ex22.CREATED_AT = DateTime.UtcNow.AddHours(7);
                    //MoDbContext.BUKALAPAK_TOKEN.Add(ex22);
                    //MoDbContext.SaveChanges();

                    AccessKeyBL retObj = JsonConvert.DeserializeObject(stringRet, typeof(AccessKeyBL)) as AccessKeyBL;
                    if (retObj != null)
                    {
                        //DateTime tglExpired = DateTimeOffset.FromUnixTimeSeconds(retObj.created_at).UtcDateTime.AddHours(7).AddSeconds(retObj.expires_in);
                        //var a = EDB.ExecuteSQL("ARConnectionString", CommandType.Text, "UPDATE ARF01 SET REFRESH_TOKEN='" + retObj.refresh_token + "', TGL_EXPIRED='" + tglExpired.ToString("yyyy-MM-dd HH:mm:ss") + "', TOKEN='" + retObj.access_token + "', STATUS_API = '1' WHERE CUST ='" + cust + "'");
                        retGetToken = retObj;
                        //if (a == 1)
                        //{
                        //}
                        //else
                        //{
                        //}
                    }
                    else
                    {
                    }
                }catch(Exception ex)
                {

                    //var ex11 = new BUKALAPAK_TOKEN();
                    //ex11.ACCOUNT = DateTime.UtcNow.AddHours(7).ToString("yyyyMMdd");
                    //ex11.CUST = DateTime.UtcNow.AddHours(7).ToString("HHmmss");
                    //ex11.CODE = ex.Message;
                    //ex11.EMAIL = "qc-exctoken";
                    //ex11.CREATED_AT = DateTime.UtcNow.AddHours(7);
                    //MoDbContext.BUKALAPAK_TOKEN.Add(ex11);
                    //MoDbContext.SaveChanges();
                    throw new Exception("data : " + stringRet);
                }
            }
            #endregion
            urll = "https://api.bukalapak.com/me";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + retGetToken.access_token));
            //myReq.Accept = "application/x-www-form-urlencoded";
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
                //var ex1 = new BUKALAPAK_TOKEN();
                //ex1.ACCOUNT = DateTime.UtcNow.AddHours(7).ToString("yyyyMMdd") + "4";
                //ex1.CUST = DateTime.UtcNow.AddHours(7).ToString("HHmmss");
                //ex1.CODE = ex.Message;
                //ex1.EMAIL = "qc-failshop";
                //ex1.CREATED_AT = DateTime.UtcNow.AddHours(7);
                //MoDbContext.BUKALAPAK_TOKEN.Add(ex1);
                //MoDbContext.SaveChanges();
            }

            if (responseFromServer != "")
            {
                try
                {
                    //var ex2 = new BUKALAPAK_TOKEN();
                    //ex2.ACCOUNT = DateTime.UtcNow.AddHours(7).ToString("yyyyMMdd") + "2";
                    //ex2.CUST = DateTime.UtcNow.AddHours(7).ToString("HHmmss");
                    //ex2.CODE = responseFromServer;
                    //ex2.EMAIL = "qc-successshop";
                    //ex2.CREATED_AT = DateTime.UtcNow.AddHours(7);
                    //MoDbContext.BUKALAPAK_TOKEN.Add(ex2);
                    //MoDbContext.SaveChanges();

                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ShopDetailResponse)) as ShopDetailResponse;
                    if (result != null)
                    {
                        if (result.data != null)
                        {


                            var datacc = MoDbContext.BUKALAPAK_TOKEN.Where(m => m.EMAIL == result.data.email).FirstOrDefault();
                            if (datacc != null)
                            {
                                datacc.CODE = code;
                                MoDbContext.SaveChanges();
                                //ErasoftDbContext.SaveChanges();
                                DateTime tglExpired = DateTimeOffset.FromUnixTimeSeconds(retGetToken.created_at).UtcDateTime.AddHours(7).AddSeconds(retGetToken.expires_in);

                                DatabaseSQL EDB = new DatabaseSQL(datacc.ACCOUNT);
                                var res = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET API_KEY = '" + code 
                                    + "', REFRESH_TOKEN='" + retGetToken.refresh_token + "', TGL_EXPIRED='" + tglExpired.ToString("yyyy-MM-dd HH:mm:ss") + "', TOKEN='" 
                                    + retGetToken.access_token + "', STATUS_API = '1' WHERE CUST = '" + datacc.CUST + "'");
                                //GetAccessKey(datacc.ACCOUNT, datacc.CUST, code);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //var ex1 = new BUKALAPAK_TOKEN();
                    //ex1.ACCOUNT = DateTime.UtcNow.AddHours(7).ToString("yyyyMMdd") + "5";
                    //ex1.CUST = DateTime.UtcNow.AddHours(7).ToString("HHmmss");
                    //ex1.CODE = ex.Message;
                    //ex1.EMAIL = "qc-excshop";
                    //ex1.CREATED_AT = DateTime.UtcNow.AddHours(7);
                    //MoDbContext.BUKALAPAK_TOKEN.Add(ex1);
                    //MoDbContext.SaveChanges();
                }
            }
            return View("BukalapakAuth");
        }
        public BukaLapakKey RefreshToken(BukaLapakKey data)
        {
            var ret = data;
            if (data.tgl_expired < DateTime.UtcNow.AddHours(7).AddMinutes(30))
            {
                var urll = ("https://accounts.bukalapak.com/oauth/token");
                var client = new RestClient(urll);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AlwaysMultipartFormData = true;
                request.AddParameter("grant_type", "refresh_token");
                request.AddParameter("client_id", client_id);
                request.AddParameter("client_secret", client_secret);
                request.AddParameter("refresh_token", data.refresh_token);
                string stringRet = "";
                try
                {
                    IRestResponse response = client.Execute(request);
                    stringRet = response.Content;
                }
                catch (WebException e)
                {
                    string err = "";
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        WebResponse resp = e.Response;
                        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                        {
                            err = sr.ReadToEnd();
                        }
                    }
                    //ret = "error : " + err;
                }

                if (!string.IsNullOrEmpty(stringRet))
                {
                    AccessKeyBL retObj = JsonConvert.DeserializeObject(stringRet, typeof(AccessKeyBL)) as AccessKeyBL;
                    if (retObj != null)
                    {
                        DateTime tglExpired = DateTimeOffset.FromUnixTimeSeconds(retObj.created_at).UtcDateTime.AddHours(7).AddSeconds(retObj.expires_in);
                        var a = EDB.ExecuteSQL("ARConnectionString", CommandType.Text, "UPDATE ARF01 SET REFRESH_TOKEN='" + retObj.refresh_token + "', TGL_EXPIRED='" + tglExpired.ToString("yyyy-MM-dd HH:mm:ss") + "', TOKEN='" + retObj.access_token + "', STATUS_API = '1' WHERE CUST ='" + data.cust + "'");
                        ret.token = retObj.access_token;
                        ret.tgl_expired = tglExpired;
                        ret.refresh_token = retObj.refresh_token;
                    }
                    else
                    {
                    }
                }

            }
            return ret;
        }
        [HttpPost]
        //public BindingBase GetAccessKey(string cust, string email, string password)
        public BindingBase GetAccessKey(string user, string cust, string code)
        {
            var ret = new BindingBase();
            ret.status = 0;
            //var accountUser = MoDbContext.Account.Where(m => m.DatabasePathErasoft == user).FirstOrDefault();

            EDB = new DatabaseSQL(user);
            string EraServerName = EDB.GetServerName("sConn");

            ErasoftDbContext = new ErasoftContext(EraServerName, user);

            //var urll = ("https://api.bukalapak.com/v2/authenticate.json");
            var urll = ("https://accounts.bukalapak.com/oauth/token");

            //var myReq = HttpWebRequest.Create(urll);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get API Key",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = cust,
                REQUEST_ATTRIBUTE_2 = code,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, "", currentLog);

            //myReq.Method = "POST";
            //myReq.ContentType = "application/json";
            //myReq.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(email + ":" + password)));
            //var myData = "{\"version\":\"1.1\",\"method\":\"\",\"params\":{\"account\":{\"type\":\"\",\"value\":\"0\"}}}";
            //myReq.GetRequestStream().Write(Encoding.UTF8.GetBytes(myData), 0, Encoding.UTF8.GetBytes(myData).Count());
            //var myResp = myReq.GetResponse();
            //var myreader = new System.IO.StreamReader(myResp.GetResponseStream());
            ////Dim myText As String;
            //var stringRet = myreader.ReadToEnd();
            //string myData = "?grant_type=client_credentials&code=" + code;
            //myData += "&redirect_uri=https://dev.masteronline.co.id/manage/master/marketplace";
            //myData += "&client_id=" + client_id;
            //myData += "&client_secret=" + client_secret;

            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //myReq.Method = "POST";
            ////myReq.Headers.Add("Authorization", ("Bearer " + code));
            ////myReq.Credentials = new NetworkCredential(email, password);
            //myReq.ContentType = "application/json";
            ////myReq.ContentType = "application/x-www-form-urlencoded";
            //myReq.Accept = "application/json";
            ////myReq.UserAgent = "curl/7.37.0";
            //string myData = "{\"grant_type\":\"client_credentials\",\"code\":\"" + code + "\",";
            //myData += "\"redirect_uri\":\"" + HttpUtility.UrlEncode("https://dev.masteronline.co.id/manage/master/marketplace") + "\",";
            //myData += "\"client_id\":\"" + client_id + "\",\"client_secret\":\"" + client_secret + "\"}";

            //var data = Encoding.ASCII.GetBytes(myData);
            var client = new RestClient(urll);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("client_id", client_id);
            request.AddParameter("client_secret", client_secret);
            request.AddParameter("code", code);
            request.AddParameter("redirect_uri", callBackUrl);
            //IRestResponse response = client.Execute(request);
            //Console.WriteLine(response.Content);
            string stringRet = "";
            try
            {
                IRestResponse response = client.Execute(request);
                stringRet = response.Content;
                //myReq.ContentLength = myData.Length;
                //using (var dataStream = myReq.GetRequestStream())
                //{
                //    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
                //}
                ////using (var dataStream = myReq.GetRequestStream())
                ////{
                ////    dataStream.Write(data, 0, data.Length);
                ////}
                //using (WebResponse response = myReq.GetResponse())
                //{
                //    using (Stream stream = response.GetResponseStream())
                //    {
                //        StreamReader reader = new StreamReader(stream);
                //        stringRet = reader.ReadToEnd();
                //    }
                //}
            }
            catch (WebException e)
            {
                string err = "";
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                }
            }

            if (!string.IsNullOrEmpty(stringRet))
            {
                AccessKeyBL retObj = JsonConvert.DeserializeObject(stringRet, typeof(AccessKeyBL)) as AccessKeyBL;
                if (retObj != null)
                {
                    //if (retObj.status.Equals("OK"))
                    //{
                    //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                    //string username = sessionData?.Account != null ? sessionData.Account.Username : sessionData.User.Username;

                    DateTime tglExpired = DateTimeOffset.FromUnixTimeSeconds(retObj.created_at).UtcDateTime.AddHours(7).AddSeconds(retObj.expires_in);
                    var a = EDB.ExecuteSQL("ARConnectionString", CommandType.Text, "UPDATE ARF01 SET REFRESH_TOKEN='" + retObj.refresh_token + "', TGL_EXPIRED='" + tglExpired.ToString("yyyy-MM-dd HH:mm:ss") + "', TOKEN='" + retObj.access_token + "', STATUS_API = '1' WHERE CUST ='" + cust + "'");
                    //var a = EDB.GetDataSet("ARConnectionString", "ARF01", "SELECT * FROM ARF01");
                    if (a == 1)
                    {
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, "", currentLog);
                        ret.status = 1;
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = "failed to update api_key;execute result=" + a;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, "", currentLog);
                    }
                    //}
                    //else
                    //{
                    //    var a = EDB.ExecuteSQL("ARConnectionString", CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST ='" + cust + "'");

                    //    ret.message = retObj.message;
                    //    currentLog.REQUEST_EXCEPTION = ret.message;
                    //    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, "", currentLog);
                    //}
                }
                else
                {
                    ret.message = "Failed to call Buka Lapak API";
                    currentLog.REQUEST_EXCEPTION = ret.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, "", currentLog);
                }
            }

            return ret;
        }

        public async Task<ActionResult> GetKurir( string cust)
        {
            var marketPlace = ErasoftDbContext.ARF01.Where(m => m.CUST == cust).FirstOrDefault();
            var data = new BukaLapakKey
            {
                code = marketPlace.API_KEY,
                cust = marketPlace.CUST,
                dbPathEra = "",
                refresh_token = marketPlace.REFRESH_TOKEN,
                tgl_expired = marketPlace.TGL_EXPIRED.Value,
                token = marketPlace.TOKEN
            };
            data = RefreshToken(data);
            var ret = new BindingBase();

            string urll = "https://api.bukalapak.com/info/carriers";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", "Bearer " + data.token);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            //try
            //{
           
            using (WebResponse response = await myReq.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseFromServer = reader.ReadToEnd();
                }
            }
            if (responseFromServer != "")
            {
                var resp = JsonConvert.DeserializeObject(responseFromServer, typeof(GetCourierResponse)) as GetCourierResponse;
                if (resp != null)
                {
                    if (resp.meta != null)
                    {
                        return Json(resp, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(ret, JsonRequestBehavior.AllowGet);
        }
        public byte[] GetLabel(BukaLapakKey data, string orderID)
        {
            data = RefreshToken(data);
            var ret = new BindingBase();

            string urll = "https://api.bukalapak.com/transactions/download?file_type=pdf&detail_product=true";
            //foreach(var transID in orderID)
            //{
                string transid = orderID.Substring(2, orderID.Length - 2);
                urll += "&transaction_ids[]=" + transid;
            //}
            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //myReq.Method = "GET";
            //myReq.Headers.Add("Authorization", "Bearer " + data.token);
            //myReq.Accept = "application/json";
            //myReq.ContentType = "application/json";
            string responseFromServer = "";
            ////try
            ////{

            //using (WebResponse response = await myReq.GetResponseAsync())
            //{
            //    using (Stream stream = response.GetResponseStream())
            //    {
            //        StreamReader reader = new StreamReader(stream);
            //        responseFromServer = reader.ReadToEnd();
            //    }
            //}
            var client = new RestClient(urll);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + data.token);
            //IRestResponse response2 = client.Execute(request);
            var response = client.DownloadData(request);

            var returnPdf = Convert.ToBase64String(response);
            return response;
            //if (responseFromServer != "")
            //{
            //var resp = JsonConvert.DeserializeObject(responseFromServer, typeof(GetCourierResponse)) as GetCourierResponse;
            //if (resp != null)
            //{
            //    if (resp.meta != null)
            //    {
            //        return Json(resp, JsonRequestBehavior.AllowGet);
            //    }
            //}
            //}
            //return Json(ret, JsonRequestBehavior.AllowGet);
        }
        public async Task<BindingBase> ChangeOrderStatus_delivered( BukaLapakKey data, string noref, string DeliveryProvider, string noResi)
        {
            //SetupContext(DatabasePathErasoft, username);
            //data = new BukaLapakControllerJob().RefreshToken(data);
            var ret = new BindingBase();
            ret.status = 0;
            string transid = noref.Substring(2, noref.Length-2);
            
            string urll = "https://api.bukalapak.com/transactions/" + transid + "/status";

            string myData = "{\"state\":\"delivered\", \"state_options\": {\"carrier\":  \""+ DeliveryProvider + "\", \"tracking_number\":  \"" + noResi + "\" } }";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "PUT";
            myReq.Headers.Add("Authorization", "Bearer " + data.token);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            //try
            //{
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
            if (responseFromServer != "")
            {
                var resp = JsonConvert.DeserializeObject(responseFromServer, typeof(ChangeOrderStatusResponse)) as ChangeOrderStatusResponse;
                if (resp != null)
                {
                    if (resp.meta != null)
                    {
                        if (resp.meta.http_status != 200)
                        {
                            var orderDetail = ErasoftDbContext.SOT01A.Where(p => p.NO_REFERENSI == noref && p.CUST == data.cust).FirstOrDefault();
                            EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM='1', SHIPMENT = '"+ DeliveryProvider + "' WHERE NO_BUKTI = '" + orderDetail.NO_BUKTI + "'");

                            if (resp.errors != null)
                            {
                                if (resp.errors.Length > 0)
                                {
                                    string errMsg = "";
                                    foreach (var error in resp.errors)
                                    {
                                        errMsg += error.code + ":" + error.message + "\n";
                                    }
                                    //throw new Exception(errMsg);
                                    ret.message = errMsg;
                                }
                            }
                            //throw new Exception(responseFromServer);
                            if(string.IsNullOrEmpty(ret.message))
                            ret.message = responseFromServer;
                        }
                        else
                        {
                            ret.status = 1;
                            var orderDetail = ErasoftDbContext.SOT01A.Where(p => p.NO_REFERENSI == noref && p.CUST == data.cust).FirstOrDefault();

                            EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM='2', SHIPMENT = '" + DeliveryProvider + "' WHERE NO_BUKTI = '" + orderDetail.NO_BUKTI + "'");
                        }

                    }
                }
            }
            return ret;
        }
        public async Task<BindingBase> ChangeOrderStatus_setCourrier(BukaLapakKey data, string noref, string courrier, string serviceType)
        {
            //SetupContext(DatabasePathErasoft, username);
            //data = new BukaLapakControllerJob().RefreshToken(data);
            var ret = new BindingBase();
            ret.status = 0;
            string transid = noref.Substring(2, noref.Length - 2);
            //if (string.IsNullOrEmpty(courrier))
            {
                var orderinDB = ErasoftDbContext.SOT01A.Where(p => p.NO_REFERENSI == noref && p.CUST == data.cust).FirstOrDefault();
                courrier = orderinDB.SHIPMENT;
            }
            string urll = "https://api.bukalapak.com/_partners/logistic-bookings";

            string myData = "{\"transaction_id\":\""+transid+ "\",\"courier_selection\":\"" + courrier + "\",\"service_type\":\"" + serviceType + "\"}";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", "Bearer " + data.token);
            myReq.Accept = "application/json";
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
                if (responseFromServer != "")
                {
                    var resp = JsonConvert.DeserializeObject(responseFromServer, typeof(SetOrderCourrierResponse)) as SetOrderCourrierResponse;
                    if (resp != null)
                    {
                        if (resp.meta != null)
                        {
                            if (resp.meta.http_status != 200)
                            {
                                var orderDetail = ErasoftDbContext.SOT01A.Where(p => p.NO_REFERENSI == noref && p.CUST == data.cust).FirstOrDefault();
                                EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM='1' WHERE NO_BUKTI = '" + orderDetail.NO_BUKTI + "'");

                                if (resp.errors != null)
                                {
                                    if (resp.errors.Length > 0)
                                    {
                                        string errMsg = "";
                                        foreach (var error in resp.errors)
                                        {
                                            errMsg += error.code + ":" + error.message + "\n";
                                        }
                                        //throw new Exception(errMsg);
                                        ret.message = errMsg;
                                    }
                                }
                                //throw new Exception(responseFromServer);
                                if (string.IsNullOrEmpty(ret.message))
                                    ret.message = responseFromServer;
                            }
                            else
                            {
                                ret.status = 1;
                                var orderDetail = ErasoftDbContext.SOT01A.Where(p => p.NO_REFERENSI == noref && p.CUST == data.cust).FirstOrDefault();

                                EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM = '2', SHIPMENT='" + resp.data.courier_name + "', NO_PO_CUST = '" + resp.data.booking_code + "' WHERE NO_BUKTI = '" + orderDetail.NO_BUKTI + "'");
                            }

                        }
                    }
                }
            }
            catch (WebException e)
            {
                string err = "";
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                }
                ret.message = err;
                var orderDetail = ErasoftDbContext.SOT01A.Where(p => p.NO_REFERENSI == noref && p.CUST == data.cust).FirstOrDefault();
                EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM='1' WHERE NO_BUKTI = '" + orderDetail.NO_BUKTI + "'");
            }
            return ret;
        }
        [HttpPost]
        public BindingBase CreateProduct(BrgViewModel data)
        {
            var ret = new BindingBase();
            ret.status = 0;
            //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
            //change by Tri 7 Mei 2019
            //var dataProduct = new BindingBukaLapakProduct
            //{
            //    images = data.imageId,
            //    product = new ProductBukaLpk
            //    {
            //        category_id = "564",
            //        name = data.nama,
            //        @new = "true",
            //        price = data.harga,
            //        negotiable = "false",
            //        weight = data.weight,//weight in gram
            //        stock = data.qty,
            //        description_bb = data.deskripsi,
            //        product_detail_attributes = new Product_Detail_Attributes
            //        {
            //            bahan = "baku",
            //            merek = data.merk,
            //            tipe = "formal",
            //            //type = "",
            //            ukuran = "S",
            //        }
            //    }
            //};
            //if (!string.IsNullOrEmpty(data.nama2))
            //{
            //    dataProduct.product.name += " " + data.nama2;
            //}
            //if (!string.IsNullOrEmpty(data.imageId2))
            //{
            //    if (!string.IsNullOrEmpty(dataProduct.images))
            //        dataProduct.images += ",";
            //    dataProduct.images += data.imageId2;
            //}
            //if (!string.IsNullOrEmpty(data.imageId3))
            //{
            //    if (!string.IsNullOrEmpty(dataProduct.images))
            //        dataProduct.images += ",";
            //    dataProduct.images += "," + data.imageId3;
            //}
            var brgInDB = ErasoftDbContext.STF02H.Where(m => m.BRG == data.kdBrg && m.IDMARKET.ToString() == data.idMarket).FirstOrDefault();
            var customer = ErasoftDbContext.ARF01.Where(m => m.RecNum.ToString() == data.idMarket).FirstOrDefault();
            string dataBrg = "{ \"product\": { \"category_id\":\"" + brgInDB.CATEGORY_CODE + "\", ";
            dataBrg += "\"name\":\"" + data.nama + (string.IsNullOrEmpty(data.nama2) ? "" : " " + data.nama2) + "\",";
            dataBrg += "\"new\":\"true\", \"price\":" + data.harga + ", \"negotiable\":\"false\", \"weight\":" + data.weight + ", ";
            dataBrg += "\"stock\":" + data.qty + ", \"description_bb\":\"" + data.deskripsi + "\", \"product_detail_attributes\":{";

            var attributeBL = GetAttr(customer.API_KEY, customer.TOKEN, brgInDB.CATEGORY_CODE);
            List<string> dsAttr = new List<string>();
            for (int i = 1; i <= 30; i++)
            {
                string attribute_id = Convert.ToString(attributeBL["FIELDNAME_" + i.ToString()]);
                if (!string.IsNullOrWhiteSpace(attribute_id))
                {
                    dsAttr.Add(attribute_id);
                }
            }

            Dictionary<string, string> BLAttrWithVal = new Dictionary<string, string>();
            for (int i = 1; i <= 30; i++)
            {
                string attribute_id = Convert.ToString(brgInDB["ACODE_" + i.ToString()]);
                string value = Convert.ToString(brgInDB["AVALUE_" + i.ToString()]);
                if (!string.IsNullOrWhiteSpace(value) && value != "null")
                {
                    if (dsAttr.Contains(attribute_id))
                    {
                        //if (!lzdAttrWithVal.ContainsKey(attribute_id))
                        //{
                        BLAttrWithVal.Add(attribute_id, value.Trim());
                        //}
                    }
                }
            }

            foreach (var BLAttr in BLAttrWithVal)
            {
                dataBrg += "\"" + BLAttr.Key + "\":";
                dataBrg += "\"" + BLAttr.Value.ToString() + "\",";
            }
            dataBrg = dataBrg.Substring(0, dataBrg.Length - 1);

            string listImage = data.imageId;
            if (!string.IsNullOrEmpty(data.imageId2))
            {
                if (!string.IsNullOrEmpty(listImage))
                    listImage += ",";
                listImage += data.imageId2;
            }
            if (!string.IsNullOrEmpty(data.imageId3))
            {
                if (!string.IsNullOrEmpty(listImage))
                    listImage += ",";
                listImage += "," + data.imageId3;
            }
            //add 6/9/2019, 5 gambar
            if (!string.IsNullOrEmpty(data.imageId4))
            {
                if (!string.IsNullOrEmpty(listImage))
                    listImage += ",";
                listImage += data.imageId4;
            }
            if (!string.IsNullOrEmpty(data.imageId5))
            {
                if (!string.IsNullOrEmpty(listImage))
                    listImage += ",";
                listImage += "," + data.imageId5;
            }
            //end add 6/9/2019, 5 gambar
            dataBrg += "} }, \"images\":\"" + listImage + "\"}";

            //end change by Tri 7 Mei 2019

            //string hargaMarket = EDB.GetFieldValue("", "STF02H", "BRG = '" + data.kdBrg + "' AND AKUNMARKET = '" + data.akunMarket + "'", "HJUAL").ToString();
            //if (!string.IsNullOrEmpty(hargaMarket))
            //    dataProduct.product.price = hargaMarket;
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Create Product",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = data.kdBrg,
                REQUEST_ATTRIBUTE_2 = data.token,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data.key, currentLog);

            //string dataPost = JsonConvert.SerializeObject(dataProduct);
            Utils.HttpRequest req = new Utils.HttpRequest();
            var bindResponse = req.CallBukaLapakAPI("POST", "products.json", dataBrg, data.key, data.token, typeof(CreateProductBukaLapak)) as CreateProductBukaLapak;
            if (bindResponse != null)
            {
                if (bindResponse.status.Equals("OK"))
                {
                    ret.status = 1;
                    ret.message = bindResponse.product_detail.id;
                    var a = EDB.ExecuteSQL("", CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + bindResponse.product_detail.id + "' WHERE BRG = '" + data.kdBrg + "' AND IDMARKET = '" + data.idMarket + "'");
                    if (a == 1)
                    {
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data.key, currentLog);
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = "failed to update brg_mp;execute result=" + a;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data.key, currentLog);
                    }
                }
                else
                {
                    ret.message = bindResponse.message;
                    currentLog.REQUEST_EXCEPTION = ret.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data.key, currentLog);
                }
            }
            else
            {
                ret.message = "Failed to call Buka Lapak API";
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data.key, currentLog);
            }
            return ret;
        }

        [HttpGet]
        public BindingBase uploadGambar(string imagePath, string userId, string token)
        {
            //ukuran minimum gambar adalah 300x300
            BindingBase ret = new BindingBase();
            ret.status = 0;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Upload Image Product",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = token,
                REQUEST_ATTRIBUTE_2 = imagePath,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, userId, currentLog);

            string URL = "https://api.bukalapak.com/v2/images.json";
            string post_data = imagePath;//"D:\\kaos.jpg"; //alamat file

            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

            WebRequest myReq = WebRequest.Create(URL);
            myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userId + ":" + token))));

            myReq.Method = "POST";
            myReq.ContentType = "multipart/form-data; boundary=" + boundary;

            Stream postDataStream = GetPostStream(post_data, boundary);

            myReq.ContentLength = postDataStream.Length;
            Stream reqStream = myReq.GetRequestStream();

            postDataStream.Position = 0;

            byte[] buffer = new byte[1024];
            int bytesRead = 0;

            while ((bytesRead = postDataStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                reqStream.Write(buffer, 0, bytesRead);
            }

            postDataStream.Close();
            reqStream.Close();
            try
            {
                StreamReader sr = new StreamReader(myReq.GetResponse().GetResponseStream());
                var stringRes = JsonConvert.DeserializeObject(sr.ReadToEnd(), typeof(BukaLapakRes)) as BukaLapakRes;
                if (stringRes != null)
                {
                    if (stringRes.status.Equals("OK"))
                    {
                        ret.status = 1;
                        ret.message = stringRes.id;
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);
                    }
                    else
                    {
                        ret.message = stringRes.message;
                        currentLog.REQUEST_EXCEPTION = ret.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                    }
                }
                else
                {
                    ret.message = "Failed to call Buka Lapak API";
                    currentLog.REQUEST_EXCEPTION = ret.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.ToString();
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                return ret;
            }

            return ret;
        }

        private static Stream GetPostStream(string filePath, string boundary)
        {
            Stream postDataStream = new System.IO.MemoryStream();
            System.Uri fileUrl = new Uri(filePath);

            //change by calvin 16 nov 2018
            //FileInfo fileInfo = new FileInfo(filePath);
            String filename = fileUrl.PathAndQuery.Replace('/', Path.DirectorySeparatorChar);
            FileInfo fileInfo = new FileInfo(filename);
            //end change by calvin 16 nov 2018

            string fileHeaderTemplate = Environment.NewLine + "--" + boundary + Environment.NewLine +
            "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" +
            Environment.NewLine + "Content-Type: multipart/form-data" + Environment.NewLine + Environment.NewLine;

            byte[] fileHeaderBytes = System.Text.Encoding.UTF8.GetBytes(string.Format(fileHeaderTemplate,
            "file", fileInfo.FullName));

            postDataStream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);

            //change by calvin 16 nov 2018
            //FileStream fileStream = fileInfo.OpenRead();
            var req = System.Net.WebRequest.Create(filePath);
            Stream stream = req.GetResponse().GetResponseStream();
            //end change by calvin 16 nov 2018

            byte[] buffer = new byte[1024];

            int bytesRead = 0;

            //while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                postDataStream.Write(buffer, 0, bytesRead);
            }

            //fileStream.Close();
            stream.Close();

            byte[] endBoundaryBytes = System.Text.Encoding.UTF8.GetBytes("--" + boundary + "--");
            postDataStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);

            return postDataStream;
        }

        [HttpGet]
        public CreateProductBukaLapak updateProduk(string brg, string brgMp, string price, string stock, string userId, string token)
        {

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Price/Stock Product",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = brg,
                REQUEST_ATTRIBUTE_2 = price,
                REQUEST_ATTRIBUTE_3 = stock,
                REQUEST_ATTRIBUTE_4 = token,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, userId, currentLog);

            var ret = new CreateProductBukaLapak();
            string Myprod = "{\"product\": {";
            if (!string.IsNullOrEmpty(price))
            {
                Myprod += "\"price\":\"" + price + "\"";
            }
            if (!string.IsNullOrEmpty(price) && !string.IsNullOrEmpty(stock))
                Myprod += ",";
            if (!string.IsNullOrEmpty(stock))
            {
                Myprod += "\"stock\":\"" + stock + "\"";
            }
            Myprod += "}}";
            Utils.HttpRequest req = new Utils.HttpRequest();
            ret = req.CallBukaLapakAPI("PUT", "products/" + brgMp + ".json", Myprod, userId, token, typeof(CreateProductBukaLapak)) as CreateProductBukaLapak;
            if (ret != null)
            {
                if (ret.status.ToString().Equals("OK"))
                {
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);

                    //add by calvin 8 nov 2018
                    if (!string.IsNullOrEmpty(stock))
                    {
                        //jika stok di bukalapak 0, di bukalapak akan menjadi non display, MO disamakan
                        if (Convert.ToDouble(stock) == 0)
                        {
                            var arf01Bukalapak = ErasoftDbContext.ARF01.Where(p => p.NAMA == "8").ToList();
                            foreach (var akun in arf01Bukalapak)
                            {
                                string sSQL = "UPDATE STF02H SET DISPLAY = '0' WHERE IDMARKET = '" + Convert.ToString(akun.RecNum) + "' AND BRG = '" + brg + "'";
                                var a = EDB.ExecuteSQL(sSQL, CommandType.Text, sSQL);
                                if (a <= 0)
                                {

                                }
                            }
                        }
                    }
                    //end add by calvin 8 nov 2018
                }
                else
                {
                    ret.message = ret.message;
                    currentLog.REQUEST_EXCEPTION = ret.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                }
            }
            else
            {
                ret = new CreateProductBukaLapak();
                ret.message = "Failed to call Buka Lapak API";
                currentLog.REQUEST_EXCEPTION = "Failed to call Buka Lapak API";
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
            }
            return ret;
        }

        [HttpGet]
        public BindingBase updateProdukStat(string kdBrg, string id, string akunMarket, bool stat, string userId, string token)
        {
            var ret = new BindingBase { status = 0 };
            Utils.HttpRequest req = new Utils.HttpRequest();
            BindingProductBL response = req.CallBukaLapakAPI("", "products/" + id + ".json", "", userId, token, typeof(BindingProductBL)) as BindingProductBL;
            if (response != null)
            {
                if (response.status.ToString().Equals("OK"))
                {
                    if (stat != Convert.ToBoolean(response.product.active))
                    {
                        if (stat)
                        {
                            var res = prodAktif(kdBrg, id, userId, token);
                            ret.status = res.status;
                            ret.message = res.message;

                        }
                        else
                        {
                            var res = prodNonAktif(kdBrg, id, userId, token);
                            ret.status = res.status;
                            ret.message = res.message;
                        }
                    }
                }
                else//no product on bukalapak > create new
                {
                    //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                    var dsSTF02 = EDB.GetDataSet("MOConnectionString", "STF02", "SELECT * FROM STF02 WHERE BRG = '" + kdBrg + "'");
                    if (dsSTF02.Tables[0].Rows.Count > 0)
                    {
                        dsSTF02 = EDB.GetDataSet("MOConnectionString", "STF02H", "SELECT * FROM STF02H WHERE BRG = '" + kdBrg + "' AND AKUN_MARKET = '" + akunMarket + "'");

                        BrgViewModel dataBrg = new BrgViewModel
                        {
                            nama = dsSTF02.Tables["STF02"].Rows[0]["NAMA"].ToString(),
                            weight = dsSTF02.Tables["STF02"].Rows[0]["NAMA"].ToString(),
                            qty = "1",
                            deskripsi = dsSTF02.Tables["STF02"].Rows[0]["DESKRIPSI"].ToString(),
                            merk = dsSTF02.Tables["STF02"].Rows[0]["KET_SORT2"].ToString(),

                        };
                        if (dsSTF02.Tables["STF02H"].Rows.Count > 0)
                        {
                            dataBrg.harga = dsSTF02.Tables["STF02H"].Rows[0]["HJUAL"].ToString();
                        }
                        else
                        {
                            //tidak ada data barang dengan brg dan marketplace tsb, gunakan harga brg dr marketplace lain
                            dsSTF02 = EDB.GetDataSet("", "STF02H", "SELECT * FROM STF02H WHERE BRG = '" + kdBrg + "'");
                            if (dsSTF02.Tables["STF02H"].Rows.Count > 0)
                            {
                                dataBrg.harga = dsSTF02.Tables["STF02H"].Rows[0]["HJUAL"].ToString();
                            }
                        }
                        string path = "C:\\MasterOnline\\Content\\Uploaded\\";
                        string fileName = "FotoProduk-" + dsSTF02.Tables["STF02"].Rows[0]["USERNAME"].ToString() + "-" + kdBrg + "-foto-";
                        string[] files = System.IO.Directory.GetFiles(path, "*-" + kdBrg + "-foto-1.*", System.IO.SearchOption.TopDirectoryOnly);
                        if (files.Length > 0)
                        {
                            var a = uploadGambar(path + fileName + "1", userId, token);
                            dataBrg.imageId = a.message;
                        }
                        files = System.IO.Directory.GetFiles(path, "*-" + kdBrg + "-foto-2.*", System.IO.SearchOption.TopDirectoryOnly);
                        if (files.Length > 0)
                        {
                            var a = uploadGambar(path + fileName + "2", userId, token);
                            dataBrg.imageId2 = a.message;
                        }
                        files = System.IO.Directory.GetFiles(path, "*-" + kdBrg + "-foto-3.*", System.IO.SearchOption.TopDirectoryOnly);
                        if (files.Length > 0)
                        {
                            var a = uploadGambar(path + fileName + "3", userId, token);
                            dataBrg.imageId3 = a.message;
                        }
                        var res = CreateProduct(dataBrg);
                    }

                }
            }
            else
            {
                ret.message = "Failed to call Buka Lapak API";
            }

            return ret;
        }

        public BindingBase prodNonAktif(string kdBrg, string id, string userId, string token)
        {
            var ret = new BindingBase { status = 0 };
            Utils.HttpRequest req = new Utils.HttpRequest();

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Hide Product",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = kdBrg,
                REQUEST_ATTRIBUTE_2 = id,
                REQUEST_ATTRIBUTE_3 = token,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, userId, currentLog);

            ResProduct response = req.CallBukaLapakAPI("PUT", "products/" + id + "/sold.json", "", userId, token, typeof(ResProduct)) as ResProduct;
            if (response != null)
            {
                if (response.status.ToString().Equals("OK"))
                {
                    ret.status = 1;
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);
                }
                else
                {
                    ret.message = response.message;
                    currentLog.REQUEST_EXCEPTION = response.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                }
            }
            else
            {
                ret.message = "Failed to call Buka Lapak API";
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
            }
            return ret;
        }


        public BindingBase prodAktif(string brg, string id, string userId, string token)
        {
            var ret = new BindingBase { status = 0 };
            Utils.HttpRequest req = new Utils.HttpRequest();

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Show Product",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = brg,
                REQUEST_ATTRIBUTE_2 = id,
                REQUEST_ATTRIBUTE_3 = token,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, userId, currentLog);

            ResProduct response = req.CallBukaLapakAPI("PUT", "products/" + id + "/relist.json", "", userId, token, typeof(ResProduct)) as ResProduct;
            if (response != null)
            {
                if (response.status.ToString().Equals("OK"))
                {
                    ret.status = 1;
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);
                    //call api to set product stock, buka lapak non active product = 0 stock

                    var qtyOnHand = 0d;
                    {
                        object[] spParams = {
                                                    new SqlParameter("@BRG", brg),
                                                    new SqlParameter("@GD","ALL"),
                                                    new SqlParameter("@Satuan", "2"),
                                                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                                                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}
                                                };

                        ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                        qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                    }
                    updateProduk(brg, id, "", qtyOnHand > 0 ? qtyOnHand.ToString() : "1", userId, token);
                }
                else
                {
                    ret.message = response.message;
                    currentLog.REQUEST_EXCEPTION = response.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                }
            }
            else
            {
                ret.message = "Failed to call Buka Lapak API";
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
            }
            return ret;
        }

        [HttpGet]
        public BindingBase cekTransaksi(/*string transId,*/ string Cust, string email, string userId, string token, string connectionID)
        {
            //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
            var ret = new BindingBase();
            ret.status = 0;

            Utils.HttpRequest req = new Utils.HttpRequest();
            string url = "transactions.json";
            //if (!string.IsNullOrEmpty(transId))
            //    url = "transactions/" + transId + ".json";
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Order",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = token,
                REQUEST_ATTRIBUTE_2 = email,
                REQUEST_ATTRIBUTE_3 = connectionID,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, userId, currentLog);

            var bindOrder = req.CallBukaLapakAPI("", url, "", userId, token, typeof(BukaLapakOrder)) as BukaLapakOrder;
            if (bindOrder != null)
            {
                //ret = bindOrder;
                if (bindOrder.status.Equals("OK"))
                {
                    var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == Cust).Select(p => p.NO_REFERENSI).ToList();
                    bool adaInsert = false;

                    string insertQ = "INSERT INTO TEMP_BL_ORDER ([ID],[INVOICE_ID],[STATE],[TRANSACTION_ID],[AMOUNT],[QUANTITY],[COURIER],[BUYERS_NOTE],[SHIPPING_FEE],";
                    insertQ += "[SHIPPING_ID],[SHIPPING_CODE],[SHIPPING_SERVICE],[SUBTOTAL_AMOUNT],[TOTAL_AMOUNT],[PAYMENT_AMOUNT],[CREATED_AT],[UPDATED_AT],";
                    insertQ += "[BUYER_EMAIL],[BUYER_ID],[BUYER_NAME],[BUYER_USERNAME],[BUYER_LOGISTIC_CHOICE],[CONSIGNEE_ADDRESS],[CONSIGNEE_AREA],[CONSIGNEE_CITY],";
                    insertQ += "[CONSIGNEE_NAME],[CONSIGNEE_PHONE],[CONSIGNEE_POSTCODE],[CONSIGNEE_PROVICE],[CUST],[USERNAME],[CONNECTION_ID]) VALUES ";

                    string insertOrderItems = "INSERT INTO TEMP_BL_ORDERITEMS ([ID],[TRANSACTION_ID],[PRODUCT_ID],[CATEGORY],[CATEGORY_ID],[NAME]";
                    insertOrderItems += ",[PRICE],[WEIGHT],[DESC],[CONDITON],[STOCK],[QTY], [CREATED_AT], [UPDATED_AT], [USERNAME], [CONNECTION_ID]) VALUES ";

                    string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                    insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                    insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";
                    //int i = 1;
                    var connIDARF01C = Guid.NewGuid().ToString();
                    string username = sessionData?.Account != null ? sessionData.Account.Username : sessionData.User.Username;
                    if (username.Length > 20)
                        username = username.Substring(0, 17) + "...";
                    var dtNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    foreach (Transaction order in bindOrder.transactions)
                    {
                        if (!order.buyer.email.Equals(email))//cek email pembeli != email user untuk mendapatkan order penjualan
                        {
                            bool doInsert = true;
                            if (OrderNoInDb.Contains(Convert.ToString(order.id)) && order.state.ToString().ToLower() == "paid")
                            {
                                doInsert = false;
                            }
                            //add 19 Feb 2019
                            else if (order.state.ToString().ToLower() == "received" || order.state.ToString().ToLower() == "remitted")
                            {
                                if (OrderNoInDb.Contains(Convert.ToString(order.id)))
                                {
                                    //tidak ubah status menjadi selesai jika belum diisi faktur
                                    var dsSIT01A = EDB.GetDataSet("CString", "SIT01A", "SELECT NO_REFERENSI, O.NO_BUKTI, O.STATUS_TRANSAKSI FROM SIT01A I INNER JOIN SOT01A O ON I.NO_SO = O.NO_BUKTI WHERE NO_REFERENSI = '" + order.id + "'");
                                    if (dsSIT01A.Tables[0].Rows.Count == 0)
                                    {
                                        doInsert = false;
                                    }
                                }
                                else
                                {
                                    //tidak diinput jika order sudah selesai sebelum masuk MO
                                    doInsert = false;
                                }
                            }
                            //end add 19 Feb 2019

                            if (doInsert)
                            {
                                adaInsert = true;
                                var statusEra = "";
                                switch (order.state.ToString().ToLower())
                                {
                                    case "pending":
                                    case "addressed":
                                    case "payment_chosen":
                                    case "confirm_payment":
                                    case "paid":
                                        statusEra = "01";
                                        break;
                                    case "accepted":
                                        statusEra = "02";
                                        break;
                                    case "delivered":
                                        statusEra = "03";
                                        break;
                                    case "received":
                                    //statusEra = "03";
                                    //break;
                                    case "remitted":
                                        statusEra = "04";
                                        break;
                                    case "rejected":
                                    case "expired":
                                    case "cancelled":
                                    //statusEra = "11";
                                    //break;
                                    case "refunded":
                                        statusEra = "11";
                                        break;
                                    default:
                                        statusEra = "99";
                                        break;
                                }
                                //jika status pesanan sudah diubah di mo, dari 01 -> 02/03, status tidak dikembalikan ke 01
                                if (statusEra == "01")
                                {
                                    var currentStatus = EDB.GetFieldValue("", "SOT01A", "NO_REFERENSI = '" + order.id + "'", "STATUS_TRANSAKSI").ToString();
                                    if (!string.IsNullOrEmpty(currentStatus))
                                        if (currentStatus == "02" || currentStatus == "03")
                                            statusEra = currentStatus;
                                }
                                //end jika status pesanan sudah diubah di mo, dari 01 -> 02/03, status tidak dikembalikan ke 01

                                insertQ += "(" + order.id + "," + order.invoice_id + ",'" + statusEra + "','" + order.transaction_id + "'," + order.amount + "," + order.quantity + ",'" + order.courier.Replace('\'', '`') + "','" + order.buyer_notes.Replace('\'', '`') + "'," + order.shipping_fee + ",";
                                insertQ += order.shipping_id + ",'" + order.shipping_code + "','" + order.shipping_service.Replace('\'', '`') + "'," + order.subtotal_amount + "," + order.total_amount + "," + order.payment_amount + ",'" + /*Convert.ToDateTime(order.created_at).ToString("yyyy-MM-dd HH:mm:ss")*/ order.created_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + /*Convert.ToDateTime(order.updated_at).ToString("yyyy-MM-dd HH:mm:ss")*/ order.updated_at.ToString("yyyy-MM-dd HH:mm:ss") + "','";
                                insertQ += order.buyer.email.Replace('\'', '`') + "','" + order.buyer.id + "','" + order.buyer.name.Replace('\'', '`') + "','" + order.buyer.username.Replace('\'', '`') + "','" + order.buyer_logistic_choice.Replace('\'', '`') + "','" + order.consignee.address.Replace('\'', '`') + "','" + order.consignee.area.Replace('\'', '`') + "','" + order.consignee.city.Replace('\'', '`') + "','";
                                insertQ += order.consignee.name.Replace('\'', '`') + "','" + order.consignee.phone + "','" + order.consignee.post_code.Replace('\'', '`') + "','" + order.consignee.province.Replace('\'', '`') + "','" + Cust + "','" + username + "','" + connectionID + "')";

                                if (order.products != null)
                                {
                                    foreach (ProductBukaLapak items in order.products)
                                    {
                                        string namaBrg = "";
                                        //CHANGE BY CALVIN 19 DESEMBER 2018, NAMA BARANG DIISI DARI MARKETPLACE, UNTUK DISIMPAN DI CATATAN
                                        //var ds = EDB.GetDataSet("MOConnectionString", "0", "SELECT STF02.NAMA AS NAMA_BRG FROM STF02H S INNER JOIN ARF01 A ON S.AKUNMARKET = A.PERSO INNER JOIN STF02 ON S.BRG = STF02.BRG WHERE BRG_MP = '" + items.id + "' AND CUST = '" + Cust + "'");
                                        //if (ds.Tables[0].Rows.Count > 0)
                                        //{
                                        //    namaBrg = ds.Tables[0].Rows[0]["NAMA_BRG"].ToString();
                                        //}
                                        //END CHANGE BY CALVIN 19 DESEMBER 2018, NAMA BARANG DIISI DARI MARKETPLACE, UNTUK DISIMPAN DI CATATAN
                                        namaBrg = items.name;
                                        insertOrderItems += "(" + order.id + ", '" + order.transaction_id + "','" + items.id + "','" + items.category + "'," + items.category_id + ",'" + namaBrg.Replace('\'', '`') + "',";
                                        insertOrderItems += items.accepted_price + "," + items.weight + ",'" + items.desc + "','" + items.condition.Replace('\'', '`') + "'," + items.stock + "," + items.order_quantity + ",'" + order.created_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.updated_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + username + "','" + connectionID + "')";
                                        insertOrderItems += " ,";
                                    }
                                }

                                var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + order.consignee.city + "%'");
                                var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + order.consignee.province + "%'");

                                var kabKot = "3174";//set default value jika tidak ada di db
                                var prov = "31";//set default value jika tidak ada di db

                                if (tblProv.Tables[0].Rows.Count > 0)
                                    prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
                                if (tblKabKot.Tables[0].Rows.Count > 0)
                                    kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();

                                insertPembeli += "('" + order.buyer.name.Replace('\'', '`') + "','" + order.consignee.address.Replace('\'', '`') + "','" + order.consignee.phone + "','" + order.buyer.email.Replace('\'', '`') + "',0,0,'0','01',";
                                insertPembeli += "1, 'IDR', '01', '" + order.consignee.address.Replace('\'', '`') + "', 0, 0, 0, 0, '1', 0, 0, ";
                                insertPembeli += "'FP', '" + dtNow + "', '" + username + "', '" + order.consignee.post_code.Replace('\'', '`') + "', '" + order.buyer.email.Replace('\'', '`') + "', '" + kabKot + "', '" + prov + "', '" + order.consignee.city.Replace('\'', '`') + "', '" + order.consignee.province.Replace('\'', '`') + "', '" + connIDARF01C + "')";

                                //if (i < bindOrder.transactions.Length)
                                insertQ += " ,";
                                insertPembeli += " ,";
                            }
                        }

                        //i = i + 1;
                    }
                    string errorMsg = "";
                    if (adaInsert)
                    {
                        insertQ = insertQ.Substring(0, insertQ.Length - 2);
                        var a = EDB.ExecuteSQL(username, CommandType.Text, insertQ);
                        if (a <= 0)
                        {
                            errorMsg = "failed to insert order to temp table;";
                        }

                        insertOrderItems = insertOrderItems.Substring(0, insertOrderItems.Length - 2);
                        a = EDB.ExecuteSQL(username, CommandType.Text, insertOrderItems);
                        if (a <= 0)
                        {
                            errorMsg += "failed to insert order item to temp table;";
                        }

                        insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 2);
                        a = EDB.ExecuteSQL(username, CommandType.Text, insertPembeli);
                        if (a <= 0)
                        {
                            errorMsg += "failed to insert pembeli to temp table;";
                        }
                        if (!string.IsNullOrEmpty(errorMsg))
                        {
                            currentLog.REQUEST_EXCEPTION = errorMsg;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                        }
                        else
                        {
                            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);
                        }

                        ret.status = 1;
                        ret.message = a.ToString();

                        #region call sp
                        SqlCommand CommandSQL = new SqlCommand();

                        //add by Tri call sp to insert buyer data
                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIDARF01C;

                        EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                        //end add by Tri call sp to insert buyer data

                        CommandSQL = new SqlCommand();
                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                        CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                        CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 1;
                        CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = Cust;

                        EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);
                        #endregion
                    }
                    else
                    {
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);
                    }
                }
                else
                {
                    ret.message = bindOrder.message;
                    currentLog.REQUEST_EXCEPTION = bindOrder.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                }
            }
            else
            {
                ret.message = "failed to call buka lapak api";
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
            }

            return ret;
        }

        [HttpGet]
        public BindingBase KonfirmasiPengiriman(/*string noBukti,*/ string shipCode, string transId, string courier, string userId, string token)
        {
            var ret = new BindingBase();
            ret.status = 0;

            var data = new BindingShipBL
            {
                payment_shipping = new ShippingBukaLapak
                {
                    shipping_code = shipCode,
                    transaction_id = transId,
                }
            };
            if (courier.ToUpper().Contains("POS") || courier.ToUpper().Contains("TIKI") || courier.ToUpper().Contains("JNE"))
            {
                //tidak perlu ditambahkan nama courier
            }
            else
            {
                data.payment_shipping.new_courier = courier;
            }
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Confirm Shipment",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = token,
                REQUEST_ATTRIBUTE_2 = shipCode,
                REQUEST_ATTRIBUTE_3 = transId,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, userId, currentLog);

            string dataPost = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            Utils.HttpRequest req = new Utils.HttpRequest();
            var bindStatus = req.CallBukaLapakAPI("POST", "transactions/confirm_shipping.json", dataPost, userId, token, typeof(BukaLapakRes)) as BukaLapakRes;
            if (bindStatus != null)
            {
                if (bindStatus.status.Equals("OK"))
                {
                    //string username = sessionData.Account.Username;
                    //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                    ret.status = 1;
                    //change status menjadi  04 => shipped
                    //EDB.ExecuteSQL("", CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI = '" + noBukti + "'");
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);

                }
                else
                {
                    ret.message = bindStatus.message;
                    currentLog.REQUEST_EXCEPTION = bindStatus.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                }
            }
            else
            {
                ret.message = "failed to call Buka Lapak api";
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
            }

            return ret;
        }

        public async Task<BindingBase> getListProduct(string cust, string userId, string token, int page, bool display, int recordCount, int totaldata, string storeid)
        {
            var ret = new BindingBase();
            ret.status = 0;
            ret.recordCount = recordCount;
            ret.totalData = totaldata;//add 18 Juli 2019, show total record
            ret.exception = 0;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Item List " + (display ? "Active" : "Not Active"),
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = token,
                REQUEST_ATTRIBUTE_2 = cust,
                REQUEST_ATTRIBUTE_3 = page.ToString(),
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, userId, currentLog);
            try
            {
                string urll = "https://api.preproduction.bukalapak.com/products?offset=" + (page * 10) + "&limit=10&store_id=" + storeid;
                //Utils.HttpRequest req = new Utils.HttpRequest();
                //string nonaktifUrl = "&not_for_sale_only=1";
                //ProdBL resListProd = req.CallBukaLapakAPI("", "products/mylapak.json?page=" + page + "&per_page=10" + (display ? "" : nonaktifUrl), "", userId, token, typeof(ProdBL)) as ProdBL;
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", "Bearer " + token);
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                string responseFromServer = "";
                //try
                //{
                //myReq.ContentLength = myData.Length;
                //using (var dataStream = myReq.GetRequestStream())
                //{
                //    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
                //}
                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
                if (responseFromServer != null)
                {
                    var resListProd = JsonConvert.DeserializeObject(responseFromServer, typeof(ProdBL)) as ProdBL;
                    if (resListProd != null)
                    {
                        if (resListProd.status.Equals("OK") && resListProd.products != null)
                        {
                            if (resListProd.products.Count == 0)
                            {
                                if (display)
                                {
                                    ret.status = 1;
                                    ret.message = "MOVE_TO_INACTIVE_PRODUCTS";
                                }
                                else
                                {
                                    return ret;
                                }

                            }
                            ret.status = 1;
                            if (resListProd.products.Count == 10)
                            {
                                //ret.message = (page + 1).ToString();
                                ret.nextPage = 1;
                                if (!display)
                                    ret.message = "MOVE_TO_INACTIVE_PRODUCTS";
                            }
                            else
                            {
                                if (display)
                                {
                                    ret.message = "MOVE_TO_INACTIVE_PRODUCTS";
                                    ret.nextPage = 1;
                                }
                            }
                            int IdMarket = ErasoftDbContext.ARF01.Where(c => c.CUST.Equals(cust)).FirstOrDefault().RecNum.Value;
                            var stf02h_local = ErasoftDbContext.STF02H.Where(m => m.IDMARKET == IdMarket).ToList();
                            var tempBrg_local = ErasoftDbContext.TEMP_BRG_MP.Where(m => m.IDMARKET == IdMarket).ToList();

                            string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, NAMA3, BERAT, PANJANG, LEBAR, TINGGI, CUST, ";
                            sSQL += "Deskripsi, IDMARKET, HJUAL, HJUAL_MP, DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, IMAGE4, IMAGE5, KODE_BRG_INDUK, TYPE";
                            sSQL += ", ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
                            sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20, ";
                            sSQL += "ACODE_21, ANAME_21, AVALUE_21, ACODE_22, ANAME_22, AVALUE_22, ACODE_23, ANAME_23, AVALUE_23, ACODE_24, ANAME_24, AVALUE_24, ACODE_25, ANAME_25, AVALUE_25, ACODE_26, ANAME_26, AVALUE_26, ACODE_27, ANAME_27, AVALUE_27, ACODE_28, ANAME_28, AVALUE_28, ACODE_29, ANAME_29, AVALUE_29, ACODE_30, ANAME_30, AVALUE_30 ";
                            //sSQL += "ACODE_31, ANAME_31, AVALUE_31, ACODE_32, ANAME_32, AVALUE_32, ACODE_33, ANAME_33, AVALUE_33, ACODE_34, ANAME_34, AVALUE_34, ACODE_35, ANAME_35, AVALUE_35, ACODE_36, ANAME_36, AVALUE_36, ACODE_37, ANAME_37, AVALUE_37, ACODE_38, ANAME_38, AVALUE_38, ACODE_39, ANAME_39, AVALUE_39, ACODE_40, ANAME_40, AVALUE_40, ";
                            //sSQL += "ACODE_41, ANAME_41, AVALUE_41, ACODE_42, ANAME_42, AVALUE_42, ACODE_43, ANAME_43, AVALUE_43, ACODE_44, ANAME_44, AVALUE_44, ACODE_45, ANAME_45, AVALUE_45, ACODE_46, ANAME_46, AVALUE_46, ACODE_47, ANAME_47, AVALUE_47, ACODE_48, ANAME_48, AVALUE_48, ACODE_49, ANAME_49, AVALUE_49, ACODE_50, ANAME_50, AVALUE_50) VALUES ";
                            sSQL += ") VALUES ";
                            string sSQL_Value = "";
                            foreach (var brg in resListProd.products)
                            {
                                ret.recordCount += 1;//add 18 Juli 2019, show total record
                                bool haveVarian = false;
                                string kdBrgInduk = "";
                                if (brg.product_sku.Count > 0)
                                {
                                    haveVarian = true;
                                    kdBrgInduk = brg.id;
                                    var tempbrginDBInduk = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == kdBrgInduk.ToUpper()).FirstOrDefault();
                                    var brgInDBInduk = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == kdBrgInduk.ToUpper()).FirstOrDefault();
                                    if (tempbrginDBInduk == null && brgInDBInduk == null)
                                    {
                                        var insert1 = CreateTempQry(brg, cust, IdMarket, display, 1, "", 0);
                                        if (insert1.exception == 1)
                                            ret.exception = 1;
                                        if (insert1.status == 1)
                                            sSQL_Value += insert1.message;
                                    }
                                    else if (brgInDBInduk != null)
                                    {
                                        kdBrgInduk = brgInDBInduk.BRG;
                                    }
                                }
                                //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.ToUpper().Equals(brg.id.ToUpper()) && t.IDMARKET == IdMarket).FirstOrDefault();
                                //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.ToUpper().Equals(brg.id.ToUpper()) && t.IDMARKET == IdMarket).FirstOrDefault();
                                var tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brg.id.ToUpper()).FirstOrDefault();
                                var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brg.id.ToUpper()).FirstOrDefault();
                                if (tempbrginDB == null && brgInDB == null)
                                {
                                    #region remark
                                    //ret.recordCount++;
                                    //string nama, nama2, nama3, urlImage, urlImage2, urlImage3;
                                    //urlImage = "";
                                    //urlImage2 = "";
                                    //urlImage3 = "";
                                    //if (brg.name.Length > 30)
                                    //{
                                    //    nama = brg.name.Substring(0, 30);
                                    //    //change by calvin 15 januari 2019
                                    //    //if (brg.name.Length > 60)
                                    //    //{
                                    //    //    nama2 = brg.name.Substring(30, 30);
                                    //    //    nama3 = (brg.name.Length > 90) ? brg.name.Substring(60, 30) : brg.name.Substring(60);
                                    //    //}
                                    //    if (brg.name.Length > 285)
                                    //    {
                                    //        nama2 = brg.name.Substring(30, 255);
                                    //        nama3 = "";
                                    //    }
                                    //    //end change by calvin 15 januari 2019
                                    //    else
                                    //    {
                                    //        nama2 = brg.name.Substring(30);
                                    //        nama3 = "";
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    nama = brg.name;
                                    //    nama2 = "";
                                    //    nama3 = "";
                                    //}

                                    //if (brg.images != null)
                                    //{
                                    //    urlImage = brg.images[0];
                                    //    if (brg.images.Length >= 2)
                                    //    {
                                    //        urlImage2 = brg.images[1];
                                    //        if (brg.images.Length >= 3)
                                    //        {
                                    //            urlImage3 = brg.images[2];
                                    //        }
                                    //    }
                                    //}

                                    //sSQL_Value += "('" + brg.id + "' , '" + brg.id + "' , '";
                                    ////if (brg.name.Length > 30)
                                    ////{
                                    ////    sSQL += brg.name.Substring(0, 30) + "' , '" + brg.name.Substring(30) + "' , ";
                                    ////}
                                    ////else
                                    ////{
                                    ////    sSQL += brg.name + "' , '' , ";
                                    ////}
                                    //sSQL_Value += nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' , '" + nama3.Replace('\'', '`') + "' ,";
                                    //sSQL_Value += brg.weight + " , 1, 1, 1, '" + cust + "' , '" + brg.desc.Replace("<br/>", "\r\n").Replace("<br />", "\r\n").Replace('\'', '`') + "' , " + ErasoftDbContext.ARF01.Where(c => c.CUST.Equals(cust)).FirstOrDefault().RecNum;
                                    //sSQL_Value += " , " + brg.price + " , " + brg.price + " , " + (display ? "1" : "0") + ", '";
                                    //sSQL_Value += brg.category_id + "' , '" + brg.category + "' , '" + (string.IsNullOrEmpty(brg.specs.merek) ? brg.specs.brand : brg.specs.merek);
                                    //sSQL_Value += "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "') ,";
                                    #endregion
                                    if (haveVarian)
                                    {
                                        ret.totalData += brg.product_sku.Count;//add 18 Juli 2019, show total record
                                        for (int i = 0; i < brg.product_sku.Count; i++)
                                        {
                                            var insert2 = CreateTempQry(brg, cust, IdMarket, display, 2, kdBrgInduk, i);
                                            if (insert2.exception == 1)
                                                ret.exception = 1;
                                            if (insert2.status == 1)
                                                sSQL_Value += insert2.message;
                                        }
                                    }
                                    else
                                    {
                                        var insert2 = CreateTempQry(brg, cust, IdMarket, display, 0, "", 0);
                                        if (insert2.exception == 1)
                                            ret.exception = 1;
                                        if (insert2.status == 1)
                                            sSQL_Value += insert2.message;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(sSQL_Value))
                            {
                                sSQL = sSQL + sSQL_Value;
                                sSQL = sSQL.Substring(0, sSQL.Length - 1);
                                var a = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                                ret.recordCount += a;
                            }
                            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);
                        }
                        else
                        {
                            ret.message = resListProd.message;
                            currentLog.REQUEST_EXCEPTION = resListProd.message;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                        }
                    }
                    else
                    {
                        ret.exception = 1;
                        ret.message = "failed to call Buka Lapak api";
                        currentLog.REQUEST_EXCEPTION = ret.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                    }
                }

            }
            catch (Exception ex)
            {
                ret.exception = 1;
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, userId, currentLog);
            }


            return ret;
        }

        public BindingBase CreateTempQry(ListProduct brg, string cust, int idMarket, bool display, int type, string kdBrgInduk, int i)
        {
            // typeBrg : 0 = barang tanpa varian; 1 = barang induk; 2 = barang varian
            var ret = new BindingBase();
            ret.status = 0;
            string sSQL_Value = "";
            try
            {
                string nama, nama2, nama3, urlImage, urlImage2, urlImage3, urlImage4, urlImage5;
                urlImage = "";
                urlImage2 = "";
                urlImage3 = "";
                urlImage4 = "";
                urlImage5 = "";
                string namaBrg = brg.name;
                long itemPrice = brg.price;
                if (type == 2)
                {
                    namaBrg += " " + brg.product_sku[i].variant_name;
                    itemPrice = brg.product_sku[i].price;
                }
                namaBrg = namaBrg.Replace('\'', '`');//add by Tri 8 Juli 2019, replace petik pada nama barang

                //change by calvin 16 september 2019
                //if (namaBrg.Length > 30)
                //{
                //    nama = namaBrg.Substring(0, 30);
                //    //change by calvin 15 januari 2019
                //    //if (brg.name.Length > 60)
                //    //{
                //    //    nama2 = brg.name.Substring(30, 30);
                //    //    nama3 = (brg.name.Length > 90) ? brg.name.Substring(60, 30) : brg.name.Substring(60);
                //    //}
                //    if (namaBrg.Length > 285)
                //    {
                //        nama2 = namaBrg.Substring(30, 255);
                //        nama3 = "";
                //    }
                //    //end change by calvin 15 januari 2019
                //    else
                //    {
                //        nama2 = namaBrg.Substring(30);
                //        nama3 = "";
                //    }
                //}
                //else
                //{
                //    nama = namaBrg;
                //    nama2 = "";
                //    nama3 = "";
                //}
                var splitItemName = new StokControllerJob().SplitItemName(namaBrg);
                nama = splitItemName[0];
                nama2 = splitItemName[1];
                nama3 = "";
                //end change by calvin 16 september 2019

                //add 10 Mei 2019, handle harga promo
                if (brg.deal_info != null)
                {
                    if (brg.deal_info.original_price > 0)
                    {
                        itemPrice = brg.deal_info.original_price;
                    }
                }
                //end add 10 Mei 2019, handle harga promo

                if (brg.images != null)
                {
                    if (type != 2)
                    {
                        urlImage = brg.images[0];
                        if (type == 0)
                        {
                            if (brg.images.Length >= 2)
                            {
                                urlImage2 = brg.images[1];
                                if (brg.images.Length >= 3)
                                {
                                    urlImage3 = brg.images[2];
                                }
                            }
                        }
                    }
                    else
                    {
                        if (brg.product_sku[i].images != null)
                        {
                            urlImage = brg.product_sku[i].images[0];
                            //remark 21/8/2019, barang varian ambil 1 gambar saja
                            //if (brg.product_sku[i].images.Length >= 2)
                            //{
                            //    urlImage2 = brg.product_sku[i].images[1];
                            //    if (brg.product_sku[i].images.Length >= 3)
                            //    {
                            //        urlImage3 = brg.product_sku[i].images[2];
                            //    }
                            //}
                            //end remark 21/8/2019, barang varian ambil 1 gambar saja
                        }
                    }

                }
                if (type != 2)
                {
                    //change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                    //sSQL_Value += "('" + brg.id + "' , '" + brg.id + "' , '";
                    sSQL_Value += "('" + brg.id + "' , '' , '";
                    //end change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                }
                else
                {
                    sSQL_Value += "('" + brg.product_sku[i].id + "' , '" + brg.product_sku[i].sku_name + "' , '";
                }
                string brand = "";
                if (brg.specs != null)
                {
                    brand = brg.specs.merek;
                    if (string.IsNullOrEmpty(brand))
                    {
                        brand = brg.specs.brand;
                    }
                }

                sSQL_Value += nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' , '" + nama3.Replace('\'', '`') + "' ,";
                sSQL_Value += brg.weight + " , 1, 1, 1, '" + cust + "' , '" + brg.desc.Replace("<br/>", "\r\n").Replace("<br />", "\r\n").Replace('\'', '`') + "' , " + idMarket;
                sSQL_Value += " , " + itemPrice + " , " + itemPrice + " , " + (display ? "1" : "0") + ", '";
                sSQL_Value += brg.category_id + "' , '" + brg.category + "' , '" + brand;
                sSQL_Value += "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "','" + urlImage4 + "' , '" + urlImage5 + "','";
                sSQL_Value += (type == 2 ? kdBrgInduk : "") + "','" + (type == 1 ? "4" : "3") + /*"') ,"*/ "'";
                #region attribute
                var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == cust).FirstOrDefault();
                var listAttr = GetAttr(customer.API_KEY, customer.TOKEN, brg.category_id.ToString());
                if (listAttr != null)
                {
                    var attrBL = new Dictionary<string, string>();
                    string value = "";
                    foreach (Newtonsoft.Json.Linq.JProperty property in brg.specs)
                    {
                        if (attrBL.ContainsKey(property.Name))
                        {
                            attrBL.Add(property.Name, property.Value.ToString());
                        }
                        else
                        {
                            attrBL.Add(property.Name + "2", property.Value.ToString());
                        }
                    }

                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_1))
                    {
                        //sSQL_Value += ", '" + listAttr.FIELDNAME_1 + "','" + listAttr.DISPLAYNAME_1.Replace("\'", "\'\'") + "','" + (attrBL.TryGetValue(listAttr.FIELDNAME_1, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_1, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_1))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_1);
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_1 + "','" + listAttr.DISPLAYNAME_1.Replace("\'", "\'\'") + "','" + val + "'";

                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_2))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_2, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_2))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_2);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_2 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_2 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_2 + "','" + listAttr.DISPLAYNAME_2.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_3))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_3, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_3))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_3);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_3 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_3 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_3 + "','" + listAttr.DISPLAYNAME_3.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_4))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_4, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_4))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_4);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_4 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_4 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_4 + "','" + listAttr.DISPLAYNAME_4.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_5))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_5, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_5))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_5);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_5 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_5 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_5 + "','" + listAttr.DISPLAYNAME_5.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_6))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_6, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_6))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_6);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_6 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_6 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_6 + "','" + listAttr.DISPLAYNAME_6.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_7))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_7, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_7))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_7);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_7 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_7 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_7 + "','" + listAttr.DISPLAYNAME_7.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_8))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_8, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_8))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_8);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_8 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_8 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_8 + "','" + listAttr.DISPLAYNAME_8.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_9))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_9, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_9))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_9);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_9 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_9 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_9 + "','" + listAttr.DISPLAYNAME_9.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_10))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_10, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_10))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_10);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_10 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_10 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_10 + "','" + listAttr.DISPLAYNAME_10.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_11))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_11, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_11))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_11);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_11 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_11 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_11 + "','" + listAttr.DISPLAYNAME_11.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_12))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_12, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_12))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_12);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_12 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_12 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_12 + "','" + listAttr.DISPLAYNAME_12.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_13))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_13, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_13))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_13);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_13 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_13 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_13 + "','" + listAttr.DISPLAYNAME_13.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_14))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_14, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_14))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_14);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_14 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_14 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_14 + "','" + listAttr.DISPLAYNAME_14.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_15))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_15, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_15))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_15);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_15 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_15 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_15 + "','" + listAttr.DISPLAYNAME_15.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_16))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_16, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_16))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_16);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_16 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_16 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_16 + "','" + listAttr.DISPLAYNAME_16.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_17))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_17, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_17))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_17);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_17 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_17 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_17 + "','" + listAttr.DISPLAYNAME_17.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_18))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_18, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_18))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_18);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_18 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_18 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_18 + "','" + listAttr.DISPLAYNAME_18.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_19))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_19, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_19))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_19);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_19 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_19 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_19 + "','" + listAttr.DISPLAYNAME_19.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_20))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_20, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_20))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_20);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_20 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_20 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_20 + "','" + listAttr.DISPLAYNAME_20.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_21))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_21, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_21))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_21);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_21 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_21 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_21 + "','" + listAttr.DISPLAYNAME_21.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_22))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_22, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_22))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_22);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_22 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_22 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_22 + "','" + listAttr.DISPLAYNAME_22.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_23))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_23, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_23))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_23);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_23 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_23 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_23 + "','" + listAttr.DISPLAYNAME_23.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_24))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_24, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_24))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_24);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_24 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_24 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_24 + "','" + listAttr.DISPLAYNAME_24.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_25))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_25, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_25))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_25);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_25 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_25 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_25 + "','" + listAttr.DISPLAYNAME_25.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_26))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_26, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_26))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_26);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_26 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_26 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_26 + "','" + listAttr.DISPLAYNAME_26.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_27))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_27, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_27))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_27);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_27 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_27 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_27 + "','" + listAttr.DISPLAYNAME_27.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_28))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_28, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_28))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_28);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_28 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_28 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_28 + "','" + listAttr.DISPLAYNAME_28.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_29))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_29, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_29))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_29);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_29 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_29 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_29 + "','" + listAttr.DISPLAYNAME_29.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_30))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_30, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_30))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_30);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_30 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_30 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_30 + "','" + listAttr.DISPLAYNAME_30.Replace("\'", "\'\'") + "','" + val + "') ,";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', '') ,";
                    }
                }
                else
                {
                    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
                    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
                    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '') ,";
                }
                #endregion
                ret.status = 1;
                ret.message = sSQL_Value;
            }
            catch (Exception ex)
            {
                ret.exception = 1;
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }

        public async Task<BindingBase> getListProductV2(BukaLapakKey data, int page, bool display, int recordCount, int totaldata)
        {
            string test = "";
            data = RefreshToken(data);
           
            var ret = new BindingBase();
            ret.status = 0;
            ret.recordCount = recordCount;
            ret.totalData = totaldata;//add 18 Juli 2019, show total record
            ret.exception = 0;
           
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Item List " + (display ? "Active" : "Not Active"),
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = data.token,
                REQUEST_ATTRIBUTE_2 = data.cust,
                REQUEST_ATTRIBUTE_3 = page.ToString(),
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data.code, currentLog);
            try
            {
                string urll = "https://api.bukalapak.com/stores/me/products?limit=10&offset=" + (page * 10) + "&product_type=";
                //Utils.HttpRequest req = new Utils.HttpRequest();
                //string nonaktifUrl = "&not_for_sale_only=1";
                if (display)
                {
                    urll += "available";
                }
                else
                {
                    urll += "sold";
                }
                //ProdBL resListProd = req.CallBukaLapakAPI("", "products/mylapak.json?page=" + page + "&per_page=10" + (display ? "" : nonaktifUrl), "", userId, token, typeof(ProdBL)) as ProdBL;
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", "Bearer " + data.token);
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                string responseFromServer = "";
                //try
                //{
                //myReq.ContentLength = myData.Length;
                //using (var dataStream = myReq.GetRequestStream())
                //{
                //    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
                //}
                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
                if (responseFromServer != null)
                {
                    test = responseFromServer;
                    var resListProd = JsonConvert.DeserializeObject(responseFromServer, typeof(GetItemListResponse)) as GetItemListResponse;
                    if (resListProd != null)
                    {
                        if (resListProd.meta.http_status == 200 && resListProd.data != null)
                        {
                            if (resListProd.data.Length == 0)
                            {
                                if (display)
                                {
                                    ret.status = 1;
                                    ret.nextPage = 1;
                                    ret.message = "MOVE_TO_INACTIVE_PRODUCTS";
                                }
                                //else
                                //{
                                    return ret;
                                //}

                            }
                            ret.status = 1;
                            if (resListProd.data.Length == 10)
                            {
                                //ret.message = (page + 1).ToString();
                                ret.nextPage = 1;
                                if (!display)
                                    ret.message = "MOVE_TO_INACTIVE_PRODUCTS";
                            }
                            else
                            {
                                if (display)
                                {
                                    ret.message = "MOVE_TO_INACTIVE_PRODUCTS";
                                    ret.nextPage = 1;
                                }
                            }
                            if(resListProd.meta.total < (page * 10))
                            {
                                if (display)
                                {
                                    ret.status = 1;
                                    ret.nextPage = 1;
                                    ret.message = "MOVE_TO_INACTIVE_PRODUCTS";
                                }
                                else
                                {
                                    ret.status = 0;
                                    ret.nextPage = 0;
                                    ret.message = "";
                                    return ret;
                                }
                            }
                            int IdMarket = ErasoftDbContext.ARF01.Where(c => c.CUST.Equals(data.cust)).FirstOrDefault().RecNum.Value;
                            var stf02h_local = ErasoftDbContext.STF02H.Where(m => m.IDMARKET == IdMarket).ToList();
                            var tempBrg_local = ErasoftDbContext.TEMP_BRG_MP.Where(m => m.IDMARKET == IdMarket).ToList();

                            string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, NAMA3, BERAT, PANJANG, LEBAR, TINGGI, CUST, AVALUE_34, AVALUE_45,";
                            sSQL += "Deskripsi, IDMARKET, HJUAL, HJUAL_MP, DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, IMAGE4, IMAGE5, KODE_BRG_INDUK, TYPE";
                            sSQL += ", ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
                            sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20, ";
                            sSQL += "ACODE_21, ANAME_21, AVALUE_21, ACODE_22, ANAME_22, AVALUE_22, ACODE_23, ANAME_23, AVALUE_23, ACODE_24, ANAME_24, AVALUE_24, ACODE_25, ANAME_25, AVALUE_25, ACODE_26, ANAME_26, AVALUE_26, ACODE_27, ANAME_27, AVALUE_27, ACODE_28, ANAME_28, AVALUE_28, ACODE_29, ANAME_29, AVALUE_29, ACODE_30, ANAME_30, AVALUE_30 ";
                            //sSQL += "ACODE_31, ANAME_31, AVALUE_31, ACODE_32, ANAME_32, AVALUE_32, ACODE_33, ANAME_33, AVALUE_33, ACODE_34, ANAME_34, AVALUE_34, ACODE_35, ANAME_35, AVALUE_35, ACODE_36, ANAME_36, AVALUE_36, ACODE_37, ANAME_37, AVALUE_37, ACODE_38, ANAME_38, AVALUE_38, ACODE_39, ANAME_39, AVALUE_39, ACODE_40, ANAME_40, AVALUE_40, ";
                            //sSQL += "ACODE_41, ANAME_41, AVALUE_41, ACODE_42, ANAME_42, AVALUE_42, ACODE_43, ANAME_43, AVALUE_43, ACODE_44, ANAME_44, AVALUE_44, ACODE_45, ANAME_45, AVALUE_45, ACODE_46, ANAME_46, AVALUE_46, ACODE_47, ANAME_47, AVALUE_47, ACODE_48, ANAME_48, AVALUE_48, ACODE_49, ANAME_49, AVALUE_49, ACODE_50, ANAME_50, AVALUE_50) VALUES ";
                            sSQL += ") VALUES ";
                            string sSQL_Value = "";
                            foreach (var brg in resListProd.data)
                            {
                                ret.totalData += 1;//add 18 Juli 2019, show total record
                                bool haveVarian = false;
                                string kdBrgInduk = "";
                                if (brg.variants != null)
                                    if (brg.variants.Length > 0)
                                    {
                                        haveVarian = true;
                                        kdBrgInduk = brg.id + ";0" ;
                                        var tempbrginDBInduk = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == kdBrgInduk.ToUpper()).FirstOrDefault();
                                        var brgInDBInduk = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == kdBrgInduk.ToUpper()).FirstOrDefault();
                                        if (tempbrginDBInduk == null && brgInDBInduk == null)
                                        {
                                            var insert1 = CreateTempQryV2(brg, data.cust, IdMarket, display, 1, "", 0);
                                            if (insert1.exception == 1)
                                                ret.exception = 1;
                                            if (insert1.status == 1)
                                                sSQL_Value += insert1.message;
                                        }
                                        else if (brgInDBInduk != null)
                                        {
                                            kdBrgInduk = brgInDBInduk.BRG;
                                        }
                                    }
                                var brgmp = brg.sku_id.ToString();
                                //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.ToUpper().Equals(brg.id.ToUpper()) && t.IDMARKET == IdMarket).FirstOrDefault();
                                //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.ToUpper().Equals(brg.id.ToUpper()) && t.IDMARKET == IdMarket).FirstOrDefault();
                                //var tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == brgmp).FirstOrDefault();
                                //var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == brgmp).FirstOrDefault();
                                //if (tempbrginDB == null && brgInDB == null)
                                {
                                    if (haveVarian)
                                    {
                                        ret.totalData += brg.variants.Length;//add 18 Juli 2019, show total record
                                        for (int i = 0; i < brg.variants.Length; i++)
                                        {
                                            brgmp = brg.variants[i].product_id + ";" + brg.variants[i].id;
                                            var tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == brgmp).FirstOrDefault();
                                            var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == brgmp).FirstOrDefault();
                                            if (tempbrginDB == null && brgInDB == null)
                                            {
                                                var insert2 = CreateTempQryV2(brg, data.cust, IdMarket, display, 2, kdBrgInduk, i);
                                                if (insert2.exception == 1)
                                                    ret.exception = 1;
                                                if (insert2.status == 1)
                                                    sSQL_Value += insert2.message;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        brgmp = brg.id + ";" + brg.sku_id;
                                        var tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == brgmp).FirstOrDefault();
                                        var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == brgmp).FirstOrDefault();
                                        if (tempbrginDB == null && brgInDB == null)
                                        {
                                            var insert2 = CreateTempQryV2(brg, data.cust, IdMarket, display, 0, "", 0);
                                            if (insert2.exception == 1)
                                                ret.exception = 1;
                                            if (insert2.status == 1)
                                                sSQL_Value += insert2.message;
                                        }
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(sSQL_Value))
                            {
                                sSQL = sSQL + sSQL_Value;
                                sSQL = sSQL.Substring(0, sSQL.Length - 1);
                                var a = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                                if(a < 1)
                                {
                                    currentLog.REQUEST_EXCEPTION = sSQL.Replace("'", "\'\'");
                                }
                                ret.recordCount += a;
                            }
                            currentLog.REQUEST_RESULT = sSQL.Replace("'", "\'\'");//add 23 feb 2021, cek failed to move to inactive product
                            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data.code, currentLog);
                        }
                        else
                        {
                            //ret.message = resListProd.message;
                            //currentLog.REQUEST_EXCEPTION = resListProd.message;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data.code, currentLog);
                        }
                    }
                    else
                    {
                        ret.exception = 1;
                        ret.message = "failed to call Buka Lapak api";
                        currentLog.REQUEST_EXCEPTION = ret.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data.code, currentLog);
                    }
                }

            }
            catch (Exception ex)
            {
                ret.exception = 1;
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //ret.message = test;
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data.code, currentLog);
            }


            return ret;
        }
        public BindingBase CreateTempQryV2(GetItemListDatum brg, string cust, int idMarket, bool display, int type, string kdBrgInduk, int i)
        {
            // typeBrg : 0 = barang tanpa varian; 1 = barang induk; 2 = barang varian
            var ret = new BindingBase();
            ret.status = 0;
            string sSQL_Value = "";
            try
            {
                string nama, nama2, nama3, urlImage, urlImage2, urlImage3, urlImage4, urlImage5;
                urlImage = "";
                urlImage2 = "";
                urlImage3 = "";
                urlImage4 = "";
                urlImage5 = "";
                string namaBrg = brg.name;
                long itemPrice = brg.original_price;
                if (type == 2)
                {
                    namaBrg += " " + brg.variants[i].variant_name;
                    itemPrice = brg.variants[i].price;
                }
                namaBrg = namaBrg.Replace('\'', '`');//add by Tri 8 Juli 2019, replace petik pada nama barang

                //change by calvin 16 september 2019
                //if (namaBrg.Length > 30)
                //{
                //    nama = namaBrg.Substring(0, 30);
                //    //change by calvin 15 januari 2019
                //    //if (brg.name.Length > 60)
                //    //{
                //    //    nama2 = brg.name.Substring(30, 30);
                //    //    nama3 = (brg.name.Length > 90) ? brg.name.Substring(60, 30) : brg.name.Substring(60);
                //    //}
                //    if (namaBrg.Length > 285)
                //    {
                //        nama2 = namaBrg.Substring(30, 255);
                //        nama3 = "";
                //    }
                //    //end change by calvin 15 januari 2019
                //    else
                //    {
                //        nama2 = namaBrg.Substring(30);
                //        nama3 = "";
                //    }
                //}
                //else
                //{
                //    nama = namaBrg;
                //    nama2 = "";
                //    nama3 = "";
                //}
                var splitItemName = new StokControllerJob().SplitItemName(namaBrg);
                nama = splitItemName[0];
                nama2 = splitItemName[1];
                nama3 = "";
                //end change by calvin 16 september 2019

                //add 10 Mei 2019, handle harga promo
                //if (brg.deal_info != null)
                //{
                //    if (brg.deal_info.original_price > 0)
                //    {
                //        itemPrice = brg.deal_info.original_price;
                //    }
                //}
                //end add 10 Mei 2019, handle harga promo

                if (brg.images != null)
                {
                    if (type != 2)
                    {
                        if (brg.images.large_urls != null)
                        {
                            urlImage = brg.images.large_urls[0];
                            if (type == 0)
                            {
                                if (brg.images.large_urls.Length >= 2)
                                {
                                    urlImage2 = brg.images.large_urls[1];
                                    if (brg.images.large_urls.Length >= 3)
                                    {
                                        urlImage3 = brg.images.large_urls[2];
                                        if (brg.images.large_urls.Length >= 4)
                                        {
                                            urlImage4 = brg.images.large_urls[3];
                                            if (brg.images.large_urls.Length >= 5)
                                            {
                                                urlImage5 = brg.images.large_urls[4];
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (brg.images.small_urls != null)
                        {
                            urlImage = brg.images.small_urls[0];
                            if (type == 0)
                            {
                                if (brg.images.small_urls.Length >= 2)
                                {
                                    urlImage2 = brg.images.small_urls[1];
                                    if (brg.images.small_urls.Length >= 3)
                                    {
                                        urlImage3 = brg.images.small_urls[2];
                                        if (brg.images.small_urls.Length >= 4)
                                        {
                                            urlImage4 = brg.images.small_urls[3];
                                            if (brg.images.small_urls.Length >= 5)
                                            {
                                                urlImage5 = brg.images.small_urls[4];
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (brg.variants[i].images != null)
                        {
                            if (brg.variants[i].images.large_urls != null)
                            {
                                if(brg.variants[i].images.large_urls.Length > 1)
                                {
                                    urlImage = brg.variants[i].images.large_urls[1];
                                }
                                else
                                {
                                    urlImage = brg.variants[i].images.large_urls[0];
                                }
                            }
                            else if (brg.variants[i].images.small_urls != null)
                            {
                                if (brg.variants[i].images.small_urls.Length > 1)
                                {
                                    urlImage = brg.variants[i].images.small_urls[1];
                                }
                                else
                                {
                                    urlImage = brg.variants[i].images.small_urls[0];
                                }
                            }
                            //remark 21/8/2019, barang varian ambil 1 gambar saja
                            //if (brg.product_sku[i].images.Length >= 2)
                            //{
                            //    urlImage2 = brg.product_sku[i].images[1];
                            //    if (brg.product_sku[i].images.Length >= 3)
                            //    {
                            //        urlImage3 = brg.product_sku[i].images[2];
                            //    }
                            //}
                            //end remark 21/8/2019, barang varian ambil 1 gambar saja
                        }
                    }

                }
                if (type != 2)
                {
                    //change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                    //sSQL_Value += "('" + brg.id + "' , '" + brg.id + "' , '";
                    if (type == 1)
                    {
                        sSQL_Value += "('" + brg.id + ";0" + "' , '" + (brg.sku_name ?? "") + "' , '";
                    }
                    else
                    {
                        sSQL_Value += "('" + brg.id + ";" + brg.sku_id + "' , '" + (brg.sku_name ?? "") + "' , '";
                    }
                    //end change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                }
                else
                {
                    sSQL_Value += "('" + brg.variants[i].product_id + ";" + brg.variants[i].id + "' , '" + (brg.variants[i].sku_name ?? "") + "' , '";
                }
                string brand = "";
                if (brg.specs != null)
                {
                    //brand = brg.specs.merek;
                    //if (string.IsNullOrEmpty(brand))
                    if (brg.specs.Brand != null)
                    {
                        brand = brg.specs.Brand;
                    }
                    if (brg.specs.brand != null)
                    {
                        brand = brg.specs.brand;
                    }
                }
                int p = 0;
                int l = 0;
                int t = 0;
                if (brg.dimensions != null)
                {
                    p = brg.dimensions.length;
                    l = brg.dimensions.width;
                    t = brg.dimensions.height;
                }
                //string desc = brg.description.Replace("<br/>", "\r\n").Replace("<br />", "\r\n").Replace('\'', '`');
                string desc = brg.description.Replace("\r\n", "<br />").Replace("\n", "<br />").Replace('\'', '`');
                if (!string.IsNullOrEmpty(brg.description_bb))
                {
                    //desc = brg.description_bb.Replace("<br/>", "\r\n").Replace("<br />", "\r\n").Replace('\'', '`');
                    desc = brg.description_bb.Replace("\r\n", "<br />").Replace("\n", "<br />").Replace('\'', '`');
                }
                sSQL_Value += nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' , '" + nama3.Replace('\'', '`') + "' ,";
                sSQL_Value += brg.weight + " , " + p + ", " + l + ", " + t + ", '" + cust + "' , '" + brg.url + "' , '" + (namaBrg.Length > 250 ? namaBrg.Substring(0, 250) : namaBrg) + "' , '";
                sSQL_Value += desc + "' , " + idMarket;
                sSQL_Value += " , " + itemPrice + " , " + itemPrice + " , " + (display ? "1" : "0") + ", '";
                sSQL_Value += brg.category.id + "' , '" + brg.category.name.Replace('\'', '`') + "' , '" + brand.Replace('\'', '`');
                sSQL_Value += "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "','" + urlImage4 + "' , '" + urlImage5 + "','";
                sSQL_Value += (type == 2 ? kdBrgInduk : "") + "','" + (type == 1 ? "4" : "3") + /*"') ,"*/ "'";
                #region attribute
                var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == cust).FirstOrDefault();
                var listAttr = GetAttr(customer.API_KEY, customer.TOKEN, brg.category.id.ToString());
                if (listAttr != null)
                {
                    var attrBL = new Dictionary<string, string>();
                    string value = "";
                    foreach (Newtonsoft.Json.Linq.JProperty property in brg.specs)
                    {
                        if (attrBL.ContainsKey(property.Name))
                        {
                            attrBL.Add(property.Name, property.Value.ToString());
                        }
                        else
                        {
                            attrBL.Add(property.Name + "2", property.Value.ToString());
                        }
                    }

                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_1))
                    {
                        //sSQL_Value += ", '" + listAttr.FIELDNAME_1 + "','" + listAttr.DISPLAYNAME_1.Replace("\'", "\'\'") + "','" + (attrBL.TryGetValue(listAttr.FIELDNAME_1, out value) ? value.Replace("\'", "\'\'") : "") + "'";
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_1, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_1))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_1);
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_1.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_1.Replace("\'", "\'\'") + "','" + val + "'";

                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_2))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_2, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_2))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_2);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_2 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_2 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_2.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_2.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_3))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_3, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_3))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_3);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_3 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_3 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_3.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_3.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_4))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_4, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_4))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_4);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_4 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_4 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_4.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_4.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_5))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_5, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_5))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_5);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_5 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_5 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_5.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_5.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_6))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_6, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_6))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_6);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_6 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_6 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_6.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_6.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_7))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_7, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_7))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_7);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_7 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_7 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_7.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_7.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_8))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_8, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_8))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_8);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_8 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_8 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_8.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_8.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_9))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_9, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_9))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_9);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_9 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_9 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_9.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_9.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_10))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_10, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_10))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_10);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_10 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_10 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_10.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_10.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_11))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_11, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_11))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_11);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_11 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_11 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_11.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_11.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_12))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_12, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_12))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_12);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_12 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_12 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_12.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_12.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_13))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_13, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_13))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_13);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_13 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_13 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_13.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_13.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_14))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_14, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_14))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_14);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_14 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_14 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_14.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_14.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_15))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_15, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_15))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_15);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_15 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_15 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_15.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_15.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_16))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_16, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_16))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_16);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_16 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_16 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_16.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_16.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_17))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_17, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_17))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_17);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_17 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_17 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_17.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_17.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_18))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_18, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_18))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_18);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_18 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_18 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_18.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_18.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_19))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_19, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_19))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_19);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_19 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_19 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_19.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_19.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_20))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_20, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_20))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_20);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_20 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_20 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_20.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_20.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_21))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_21, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_21))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_21);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_21 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_21 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_21.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_21.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_22))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_22, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_22))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_22);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_22 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_22 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_22.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_22.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_23))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_23, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_23))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_23);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_23 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_23 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_23.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_23.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_24))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_24, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_24))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_24);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_24 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_24 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_24.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_24.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_25))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_25, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_25))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_25);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_25 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_25 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_25.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_25.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_26))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_26, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_26))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_26);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_26 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_26 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_26.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_26.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_27))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_27, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_27))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_27);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_27 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_27 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_27.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_27.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_28))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_28, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_28))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_28);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_28 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_28 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_28.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_28.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_29))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_29, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_29))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_29);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_29 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_29 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_29.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_29.Replace("\'", "\'\'") + "','" + val + "'";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', ''";
                    }
                    if (!string.IsNullOrEmpty(listAttr.FIELDNAME_30))
                    {
                        string val = (attrBL.TryGetValue(listAttr.FIELDNAME_30, out value) ? value.Replace("\'", "\'\'") : "");
                        if (attrBL.ContainsKey(listAttr.FIELDNAME_30))
                        {
                            attrBL.Remove(listAttr.FIELDNAME_30);
                        }
                        else if (attrBL.ContainsKey(listAttr.FIELDNAME_30 + "2"))
                        {
                            val = (attrBL.TryGetValue(listAttr.FIELDNAME_30 + "2", out value) ? value.Replace("\'", "\'\'") : "");
                        }
                        sSQL_Value += ", '" + listAttr.FIELDNAME_30.Replace("\'", "\'\'") + "','" + listAttr.DISPLAYNAME_30.Replace("\'", "\'\'") + "','" + val + "') ,";
                    }
                    else
                    {
                        sSQL_Value += ", '', '', '') ,";
                    }
                }
                else
                {
                    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
                    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
                    sSQL_Value += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '') ,";
                }
                #endregion
                ret.status = 1;
                ret.message = sSQL_Value;
            }
            catch (Exception ex)
            {
                ret.exception = 1;
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }

        [HttpGet]
        public BindingBase CancelOrder(string noBukti, string transId, string userId, string token)
        {
            var ret = new BindingBase();
            string Myprod = "{ \"data\": { \"id\":\"" + transId + "\", \"payment_rejection[reason]\":\"Stok Habis\" } }";
            /* REASON (case sensitive) :
             * Stok Habis
             * Harga barang/biaya kirim tidak sesuai
             * Ada kesibukan lain yang sifatnya mendadak
             * Permintaan pembeli tidak dapat dilayani
            */
            Utils.HttpRequest req = new Utils.HttpRequest();
            var bindCancel = req.CallBukaLapakAPI("PUT", "transactions/reject.json", Myprod, userId, token, typeof(BukaLapakRes)) as BukaLapakRes;
            if (bindCancel != null)
            {
                if (bindCancel.status.Equals("OK"))
                {
                    //string username = sessionData.Account.Username;
                    //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                    ret.status = 1;
                    //change status menjadi  11 => cancelled
                    //EDB.ExecuteSQL("", CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI = '" + noBukti + "'");
                }
                else
                {
                    ret.message = bindCancel.message;
                }
            }
            else
            {
                ret.message = "failed to call Buka Lapak api";
            }

            return ret;
        }

        public async Task<string> CekKategory(string token)
        {
            //Utils.HttpRequest req = new Utils.HttpRequest();
            try
            {
                //BLKCategory ret = req.CallBukaLapakAPI("", "categories.json", "", userId, token, typeof(BLKCategory)) as BLKCategory;
                string urll = "https://api.bukalapak.com/categories";

                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", "Bearer " + token);
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                string responseFromServer = "";

                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
                if (responseFromServer != "")
                {
                    var resListCategory = JsonConvert.DeserializeObject(responseFromServer, typeof(CategoryBL)) as CategoryBL;
                    if (resListCategory.data != null)
                        foreach (var data in resListCategory.data)
                        {
                            RecursiveCategory(data, "");
                            //var cat = new CATEGORY_BUKALAPAK();
                            //cat.CATEGORY_ID = data.id.ToString();
                            //cat.NAME = data.name;
                            //cat.PARENT_ID = "";

                            //if (data.children != null)
                            //{
                            //    cat.LEAF = false;
                            //    MoDbContext.CATEGORY_BUKALAPAKs.Add(cat);
                            //    MoDbContext.SaveChanges();
                            //    foreach (var child in data.children)
                            //    {
                            //        var cat2 = new CATEGORY_BUKALAPAK();
                            //        cat2.CATEGORY_ID = child.id.ToString();
                            //        cat2.NAME = child.name;
                            //        cat2.PARENT_ID = data.id.ToString();
                            //        if (child.children != null)
                            //        {
                            //            cat2.LEAF = false;
                            //            MoDbContext.CATEGORY_BUKALAPAKs.Add(cat2);
                            //            MoDbContext.SaveChanges();
                            //            foreach (var leafChild in child.children)
                            //            {
                            //                var cat3 = new CATEGORY_BUKALAPAK();
                            //                cat3.CATEGORY_ID = leafChild.id.ToString();
                            //                cat3.NAME = leafChild.name;
                            //                cat3.PARENT_ID = child.id.ToString();
                            //                cat3.LEAF = true;
                            //                MoDbContext.CATEGORY_BUKALAPAKs.Add(cat3);
                            //                MoDbContext.SaveChanges();
                            //            }
                            //        }
                            //        else
                            //        {
                            //            cat2.LEAF = true;
                            //            MoDbContext.CATEGORY_BUKALAPAKs.Add(cat2);
                            //            MoDbContext.SaveChanges();
                            //        }
                            //    }
                            //}
                            //else
                            //{
                            //    cat.LEAF = true;
                            //    MoDbContext.CATEGORY_BUKALAPAKs.Add(cat);
                            //    MoDbContext.SaveChanges();
                            //}
                        }
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return "finish";
        }
        public void RecursiveCategory(CategoryBukaLapakV2 data, string parent_id)
        {
            try
            {
                var cat2 = new CATEGORY_BUKALAPAK();
                cat2.CATEGORY_ID = data.id.ToString();
                cat2.NAME = data.name;
                cat2.PARENT_ID = parent_id;
                if (data.children != null)
                {
                    if (data.children.Count > 0)
                    {
                        cat2.LEAF = false;
                        MoDbContext.CATEGORY_BUKALAPAKs.Add(cat2);
                        MoDbContext.SaveChanges();
                        foreach (var kategori in data.children)
                        {
                            RecursiveCategory(kategori, cat2.CATEGORY_ID);
                        }
                    }
                    else
                    {
                        cat2.LEAF = true;
                        MoDbContext.CATEGORY_BUKALAPAKs.Add(cat2);
                        MoDbContext.SaveChanges();
                    }
                }
                else
                {
                    cat2.LEAF = true;
                    MoDbContext.CATEGORY_BUKALAPAKs.Add(cat2);
                    MoDbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }

        }
        public ATTRIBUTE_BL GetAttr(string userId, string token, string id)
        {
            Utils.HttpRequest req = new Utils.HttpRequest();
            var retAttr = new ATTRIBUTE_BL();

            //var ret = req.CallBukaLapakAPI("", "categories/" + id + "/attributes.json", "", userId, token, typeof(BLAttribute)) as BLAttribute;
            string urll = "https://api.bukalapak.com/_partners/categories/" + id + "/attributes";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", "Bearer " + token);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            string responseFromServer = "";

            using (WebResponse response =  myReq.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseFromServer = reader.ReadToEnd();
                }
            }
            //if (ret.status.ToUpper() == "OK")
            if (responseFromServer != "")
            {
                var resListAttr = JsonConvert.DeserializeObject(responseFromServer, typeof(ResponseGetAttr)) as ResponseGetAttr;
                if(resListAttr != null)
                {
                    retAttr.CATEGORY_CODE = id;
                    try
                    {
                        int i = 1;
                        foreach (var attr in resListAttr.data)
                        {
                            retAttr["FIELDNAME_" + i] = attr.name;
                            retAttr["DISPLAYNAME_" + i] = attr.display_name;
                            retAttr["INPUTTYPE_" + i] = "";
                            retAttr["REQUIRED_" + i] = attr.required;
                            i++;
                        }

                        for (int j = i; j <= 30; j++)
                        {
                            retAttr["FIELDNAME_" + j] = "";
                            retAttr["DISPLAYNAME_" + j] = "";
                            retAttr["INPUTTYPE_" + j] = "";
                            retAttr["REQUIRED_" + j] = false;
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            return retAttr;
        }

        public List<ATTRIBUTE_OPT_BL> GetAttrOpt(string userId, string token, string id, string fieldName, bool variant)
        {
            Utils.HttpRequest req = new Utils.HttpRequest();
            var retAttrOpt = new List<ATTRIBUTE_OPT_BL>();

            var ret = req.CallBukaLapakAPI("", "categories/" + id + "/attributes.json", "", userId, token, typeof(BLAttribute)) as BLAttribute;
            if (ret.status.ToUpper() == "OK")
            {
                try
                {
                    if (!variant)
                    {
                        var attrBrg = ret.attributes.Where(m => m.fieldName.ToUpper() == fieldName.ToUpper()).SingleOrDefault();
                        if (attrBrg != null)
                        {
                            foreach (var opt in attrBrg.options)
                            {
                                var newOpt = new ATTRIBUTE_OPT_BL();
                                newOpt.CATEGORY_CODE = id;
                                newOpt.ID = opt;
                                newOpt.VALUE = opt;
                                retAttrOpt.Add(newOpt);
                            }
                        }
                    }
                    else
                    {
                        if (ret.variants.Length > 0)
                        {
                            var attrBrg = ret.variants.Where(m => m.name.ToUpper() == fieldName.ToUpper()).SingleOrDefault();
                            if (attrBrg != null)
                            {
                                foreach (var opt in attrBrg.value)
                                {
                                    var newOpt = new ATTRIBUTE_OPT_BL();
                                    newOpt.CATEGORY_CODE = id;
                                    newOpt.ID = opt.id.ToString();
                                    newOpt.VALUE = opt.value;
                                    retAttrOpt.Add(newOpt);
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {

                }
            }
            return retAttrOpt;
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
                        var arf01 = ErasoftDbContext.ARF01.Where(p => p.API_KEY == iden).FirstOrDefault();
                        var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                        {
                            CUST = arf01 != null ? arf01.CUST : "",
                            CUST_ATTRIBUTE_1 = arf01 != null ? arf01.PERSO : "",
                            CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                            CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                            CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                            CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                            MARKETPLACE = "Bukalapak",
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
    }

    public class BLKCategory
    {
        public string status { get; set; }
        public CategoryClass[] categories { get; set; }
        public object message { get; set; }
    }

    public class CategoryClass
    {
        public long id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public bool revamped { get; set; }
        public SubCategory[] children { get; set; }
    }

    public class SubCategory
    {
        public long id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public bool revamped { get; set; }
        public EndCategory[] children { get; set; }
    }

    public class EndCategory
    {
        public long id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public bool revamped { get; set; }
    }

    public class ResponseGetAttr : BLErrorResponse
    {
        public DataAttributes[] data { get; set; }
        public ResponseGetAttrMeta meta { get; set; }
    }

    public class ResponseGetAttrMeta
    {
        public int http_status { get; set; }
    }

    public class DataAttributes
    {
        public string name { get; set; }
        public string display_name { get; set; }
        public bool multiple { get; set; }
        public bool required { get; set; }
        public string[] options { get; set; }
    }


    public class BLAttribute
    {
        public string status { get; set; }
        public Attribute[] attributes { get; set; }
        public Variant[] variants { get; set; }
        public string message { get; set; }
    }

    public class Attribute
    {
        public string fieldName { get; set; }
        public string displayName { get; set; }
        public string inputType { get; set; }
        public string[] options { get; set; }
        public bool required { get; set; }
    }

    public class Variant
    {
        public long id { get; set; }
        public string name { get; set; }
        public Value[] value { get; set; }
    }

    public class Value
    {
        public long id { get; set; }
        public string value { get; set; }
    }

    public class ShopDetailResponse
    {
        public DataShopDetail data { get; set; }
        public MetaShopDetail meta { get; set; }
    }

    public class DataShopDetail
    {
        //public AddressShopDetail address { get; set; }
        //public long agent_id { get; set; }
        //public Avatar avatar { get; set; }
        //public Bank[] banks { get; set; }
        public string birth_date { get; set; }
        //public bool blacklisted_promo { get; set; }
        //public string bullion_auto_investment_status { get; set; }
        //public bool confirmed { get; set; }
        public string email { get; set; }
        //public string[] favorite_payment_types { get; set; }
        //public string gender { get; set; }
        //public long id { get; set; }
        //public DateTime joined_at { get; set; }
        //public DateTime last_login_at { get; set; }
        //public DateTime last_otp { get; set; }
        public string name { get; set; }
        //public object o2o_agent { get; set; }
        //public bool official { get; set; }
        public string phone { get; set; }
        //public bool phone_confirmed { get; set; }
        //public object priority_buyer_package_type { get; set; }
        //public bool registered { get; set; }
        //public string role { get; set; }
        //public string tfa_status { get; set; }
        //public Unfreezing unfreezing { get; set; }
        public string username { get; set; }
        //public bool verified { get; set; }
        //public string wallet_state { get; set; }
    }

    public class AddressShopDetail
    {
        public string address { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public string postal_code { get; set; }
        public string province { get; set; }
    }

    public class Avatar
    {
        public long id { get; set; }
        public string url { get; set; }
    }

    public class Unfreezing
    {
        public int counter { get; set; }
        public bool eligible { get; set; }
        public string freeze_category { get; set; }
        public object freezed_until { get; set; }
        public bool frozen { get; set; }
        public bool permanent_frozen { get; set; }
    }

    public class Bank
    {
        public _Cache_Keys _cache_keys { get; set; }
        public string bank { get; set; }
        public long id { get; set; }
        public string name { get; set; }
        public string number { get; set; }
        public bool primary { get; set; }
    }

    public class _Cache_Keys
    {
        public string _self { get; set; }
    }

    public class MetaShopDetail
    {
        public int http_status { get; set; }
    }
    public class GetCourierResponse : BLErrorResponse
    {
        public ResponseGetAttrMeta meta { get; set; }
        public CourierData[] data { get; set; }
    }
    public class CourierData
    {
        public string carrier { get; set; }
        public string courier_group { get; set; }
        public bool express { get; set; }
    }
}