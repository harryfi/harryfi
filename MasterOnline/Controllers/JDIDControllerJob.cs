using Erasoft.Function;
using Hangfire;
using MasterOnline.Models;
using MasterOnline.ViewModels;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using System.Web.Mvc;
using Hangfire.SqlServer;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using System.Web.Util;
using RestSharp;

namespace MasterOnline.Controllers
{
    public class JDIDControllerJob : Controller
    {
        //AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        public string imageUrl = "https://img20.jd.id/Indonesia/s300x300_/";
        public string ServerUrl = "https://open.jd.id/api";
        public string ServerUrlBigData = "https://open.jd.id/api_bigdata";
        public string AccessToken = "";
        public string AppKey = "";
        public string AppSecret = "";
        public string Version = "1.0";
        public string Format = "json";
        public string SignMethod = "md5";
        private string Charset_utf8 = "UTF-8";
        public string Method;
        public string ParamJson;
        public string ParamFile;
        protected List<long> listCategory = new List<long>();
        public List<Model_BrandJob> listBrand = new List<Model_BrandJob>();

        private MoDbContext MoDbContext { get; set; }
        private ErasoftContext ErasoftDbContext { get; set; }
        private DatabaseSQL EDB;
        private string username;

        //add by nurul 4/5/2021, JDID versi 2
        public string ServerUrlV2 = "https://open-api.jd.id/routerjson";
        public string Version2 = "2.0";
        //end add by nurul 4/5/2021, JDID versi 2

        public JDIDControllerJob()
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

        #region jdid tools
        private string getCurrentTimeFormatted()
        {
            var dt = System.DateTime.Now.ToLocalTime();
            return dt.ToString("yyyy-MM-dd HH:mm:ss.fff") + dt.ToString("zzzz").Replace(":", "");
        }

        public string Call(string sappKey, string saccessToken, string sappSecret, string sMethod, string sParamJson)
        {
            //construct system parameters
            var sysParams = new Dictionary<string, string>();
            //sysParams.Add("app_key", this.AppKey);
            sysParams.Add("app_key", sappKey);
            sysParams.Add("v", this.Version);
            sysParams.Add("format", this.Format);
            sysParams.Add("sign_method", this.SignMethod);
            //sysParams.Add("method", this.Method);
            sysParams.Add("method", sMethod);
            sysParams.Add("timestamp", this.getCurrentTimeFormatted());
            //sysParams.Add("access_token", this.AccessToken);
            sysParams.Add("access_token", saccessToken);

            //get business parameters
            if (sParamJson != null && sParamJson.Length > 0)
            {
                sysParams.Add("param_json", sParamJson);
            }
            else
            {
                sysParams.Add("param_json", "{}");
            }
            //sign
            sysParams.Add("sign", this.generateSign(sysParams, sappSecret));

            //send http post request
            var content = this.curl(this.ServerUrl, null, sysParams);
            return content;
        }

        public string Call4BigData(string sappKey, string saccessToken, string sappSecret, string sMethod, string sParamJson, string sParamFile)
        {
            //construct system parameters
            var sysParams = new Dictionary<string, string>();
            sysParams.Add("app_key", sappKey);
            sysParams.Add("v", this.Version);
            sysParams.Add("format", this.Format);
            sysParams.Add("sign_method", this.SignMethod);
            sysParams.Add("method", sMethod);
            sysParams.Add("timestamp", this.getCurrentTimeFormatted());
            sysParams.Add("access_token", saccessToken);

            //get business parameters
            if (null != sParamJson && sParamJson.Length > 0)
            {
                sysParams.Add("param_json", sParamJson);
            }
            else
            {
                sysParams.Add("param_json", "{}");
            }

            //get business file which would upload
            if (null != sParamFile && sParamFile.Length > 0)
            {
                sysParams.Add("param_file_md5", this.GetMD5HashFromFile(sParamFile));
            }
            else
            {
                throw new ArgumentException("paramter ParamFile is required");
            }

            //sign
            sysParams.Add("sign", this.generateSign(sysParams, sappSecret));

            //send http post request
            var postDatas = new NameValueCollection();
            foreach (var item in sysParams)
            {
                postDatas.Add(item.Key, item.Value);
            }
            var content = this.curl(this.ServerUrlBigData, new string[] { sParamFile }, sysParams);
            return content;
        }

        private string GetMD5HashFromFile(string fileName)
        {
            using (var md5 = MD5.Create())
            {
                var req = System.Net.WebRequest.Create(fileName);
                using (Stream stream = req.GetResponse().GetResponseStream())
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
                //using (var stream = System.IO.File.OpenRead(fileName))
                //{
                //    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                //}
            }
        }

        public string curl(string url, string[] files, Dictionary<string, string> formFields = null)
        {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "multipart/form-data; boundary=" +
                                    boundary;
            request.Method = "POST";
            request.KeepAlive = true;

            Stream memStream = new System.IO.MemoryStream();

            var boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" +
                                                                    boundary + "\r\n");
            var endBoundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" +
                                                                        boundary + "--");


            string formdataTemplate = "\r\n--" + boundary +
                                        "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";
            try
            {
                if (formFields != null)
                {
                    foreach (string key in formFields.Keys)
                    {
                        string formitem = string.Format(formdataTemplate, key, formFields[key]);
                        byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                        memStream.Write(formitembytes, 0, formitembytes.Length);
                    }
                }


                //file
                if (files != null)
                {
                    string headerTemplate =
                        "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                        "Content-Type: application/octet-stream\r\n\r\n";
                    for (int i = 0; i < files.Length; i++)
                    {
                        memStream.Write(boundarybytes, 0, boundarybytes.Length);
                        var header = string.Format(headerTemplate, "param_file", files[i]);
                        var headerbytes = System.Text.Encoding.UTF8.GetBytes(header);

                        memStream.Write(headerbytes, 0, headerbytes.Length);

                        var req = System.Net.WebRequest.Create(files[i]);
                        using (Stream stream = req.GetResponse().GetResponseStream())
                        {
                            var buffer = new byte[1024];
                            var bytesRead = 0;
                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                memStream.Write(buffer, 0, bytesRead);
                            }
                        }

                        //using (var fileStream = new FileStream(files[i], FileMode.Open, FileAccess.Read))
                        //{
                        //    var buffer = new byte[1024];
                        //    var bytesRead = 0;
                        //    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                        //    {
                        //        memStream.Write(buffer, 0, bytesRead);
                        //    }
                        //}
                    }
                }
                //~:end file


                memStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                request.ContentLength = memStream.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    memStream.Position = 0;
                    byte[] tempBuffer = new byte[memStream.Length];
                    memStream.Read(tempBuffer, 0, tempBuffer.Length);
                    memStream.Close();
                    requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                }

                using (var response = request.GetResponse())
                {
                    Stream stream2 = response.GetResponseStream();
                    StreamReader reader2 = new StreamReader(stream2);
                    return reader2.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }

        }

        private string generateSign(Dictionary<string, string> sysDataDictionary, string sappSecret)
        {
            var dic = sysDataDictionary.OrderBy(key => key.Key).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
            var sb = new System.Text.StringBuilder();
            foreach (var item in dic)
            {
                if (!"".Equals(item.Key) && !"".Equals(item.Value))
                {
                    sb.Append(item.Key).Append(item.Value);
                }

            }
            //prepend and append appsecret   
            //sb.Insert(0, this.AppSecret);
            //sb.Append(this.AppSecret);
            sb.Insert(0, sappSecret);
            sb.Append(sappSecret);
            var signValue = this.calculateMD5Hash(sb.ToString());
            //Console.WriteLine("raw string=" + sb.ToString());
            //Console.WriteLine("signValue=" + signValue);
            return signValue;
        }


        private string calculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();

        }
        #endregion

        protected void SetupContext(JDIDAPIData data)
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

        protected void SetupContext(JDIDAPIDataJob dataJob)
        {
            //string ret = "";
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(dataJob.DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, dataJob.DatabasePathErasoft);
            username = dataJob.username;
            //var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.RecNum == data.idmarket).SingleOrDefault();
            //if (arf01inDB != null)
            //{
            //    ret = arf01inDB.TOKEN;
            //}
            //return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("2_get_token")]
        public async Task<string> GetTokenJDID(JDIDAPIDataJob dataAPI, bool bForceRefresh, bool getAccessToken)
        {
            string ret = "";
            DateTime dateNow = DateTime.UtcNow.AddHours(7);
            bool TokenExpired = false;
            if (!string.IsNullOrWhiteSpace(dataAPI.tgl_expired.ToString()))
            {
                if (dateNow >= dataAPI.tgl_expired)
                {
                    TokenExpired = true;
                }
            }
            else
            {
                TokenExpired = true;
            }

            string urll = "";
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                REQUEST_ACTION = "Get Token JDID", //ganti
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = dataAPI.no_cust,
                REQUEST_ATTRIBUTE_2 = dataAPI.merchant_code,
                REQUEST_STATUS = "Pending",
            };


            if (getAccessToken)
            {
                currentLog.REQUEST_RESULT = "Process Get API Token JDID";
                urll = "https://oauth.jd.id/oauth2/access_token?app_key=" + dataAPI.appKey + "&app_secret=" + dataAPI.appSecret + "&grant_type=authorization_code&code=" + dataAPI.account_store;
            }
            else
            {
                if (TokenExpired || bForceRefresh)
                {
                    currentLog.REQUEST_RESULT = "Process Refresh API Token JDID";
                    urll = "https://oauth.jd.id/oauth2/refresh_token?app_key=" + dataAPI.appKey + "&app_secret=" + dataAPI.appSecret + "&grant_type=refresh_token&refresh_token=" + dataAPI.refreshToken;
                }
            }

            if (urll != "")
            {
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 3)
                {
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";

                    try
                    {
                        using (WebResponse response = await myReq.GetResponseAsync())
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream);
                                responseFromServer = reader.ReadToEnd();
                                responseApi = true; break;
                            }
                        }
                    }
                    catch (WebException e)
                    {
                        retry = retry + 1;
                        string err = "";
                        if (e.Status == WebExceptionStatus.ProtocolError)
                        {
                            WebResponse resp = e.Response;
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                err = sr.ReadToEnd();
                                responseFromServer = err;
                            }
                        }

                    }
                }

                if (responseFromServer != "")
                {
                    SetupContext(dataAPI);
                    manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, dataAPI, currentLog);
                    try
                    {
                        var result = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDGetTokenResult)) as JDIDGetTokenResult;
                        if (!string.IsNullOrEmpty(result.access_token) && !string.IsNullOrEmpty(result.refresh_token))
                        {
                            var getTimeExec = DateTimeOffset.FromUnixTimeSeconds(result.time / 1000).UtcDateTime.AddHours(7);
                            var timeExpired = getTimeExec.AddSeconds(result.expires_in).ToString("yyyy-MM-dd HH:mm:ss");
                            DatabaseSQL EDB = new DatabaseSQL(dataAPI.DatabasePathErasoft);
                            var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '1', TOKEN = '" + result.access_token + "', REFRESH_TOKEN = '" + result.refresh_token + "', tgl_expired ='" + timeExpired + "'  WHERE CUST = '" + dataAPI.no_cust + "'");
                            if (resultquery != 0)
                            {
                                currentLog.REQUEST_RESULT = "Update Status API Complete";
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, dataAPI, currentLog);
                            }
                            else
                            {
                                var resultqueryFailed = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0'  WHERE CUST = '" + dataAPI.no_cust + "'");
                                currentLog.REQUEST_RESULT = "Update Status API Failed";
                                currentLog.REQUEST_EXCEPTION = "Failed Update Table";
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, dataAPI, currentLog);
                            }
                        }
                        else
                        {
                            var resultqueryFailed = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0'  WHERE CUST = '" + dataAPI.no_cust + "'");
                            currentLog.REQUEST_RESULT = "Token tidak ditemukan.";
                            currentLog.REQUEST_EXCEPTION = "Failed Get Token";
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, dataAPI, currentLog);
                        }
                    }
                    catch (Exception ex)
                    {
                        var resultqueryFailed = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0'  WHERE CUST = '" + dataAPI.no_cust + "'");
                        currentLog.REQUEST_EXCEPTION = ex.Message.ToString();
                        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
                    }
                }
            }
            return ret;
        }

        public JDIDAPIDataJob RefreshToken(JDIDAPIDataJob data)
        {
            SetupContext(data);
            var ret = data;
            DateTime dateNow = DateTime.UtcNow.AddHours(7);
            bool TokenExpired = false;
            if (!string.IsNullOrWhiteSpace(data.tgl_expired.ToString()))
            {
                if (dateNow >= data.tgl_expired)
                {
                    TokenExpired = true;
                }
            }
            else
            {
                TokenExpired = true;
            }

            string urll = "";
            if (TokenExpired)
            {
                var cekInDB = ErasoftDbContext.ARF01.Where(m => m.CUST == data.no_cust).FirstOrDefault();
                if (cekInDB != null)
                {
                    if (data.tgl_expired != cekInDB.TGL_EXPIRED)
                    //if (data.accessToken != cekInDB.TOKEN && data.refreshToken != cekInDB.REFRESH_TOKEN)
                    {
                        data.appKey = cekInDB.API_KEY;
                        data.refreshToken = cekInDB.REFRESH_TOKEN;
                        data.tgl_expired = cekInDB.TGL_EXPIRED.Value;
                        data.accessToken = cekInDB.TOKEN;

                        if (cekInDB.TGL_EXPIRED > DateTime.UtcNow.AddHours(7))
                        {
                            return data;
                        }
                    }
                }

                urll = "https://oauth.jd.id/oauth2/refresh_token?app_key=" + data.appKey + "&app_secret=" + data.appSecret + "&grant_type=refresh_token&refresh_token=" + data.refreshToken;
            }
            if (urll != "")
            {
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 3)
                {
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";

                    try
                    {
                        using (WebResponse response = myReq.GetResponse())
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream);
                                responseFromServer = reader.ReadToEnd();
                                responseApi = true; break;
                            }
                        }
                    }
                    catch (WebException e)
                    {
                        if (e.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                        {
                            retry = retry + 1;
                            string err = "";
                            if (e.Status == WebExceptionStatus.ProtocolError)
                            {
                                WebResponse resp = e.Response;
                                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                                {
                                    err = sr.ReadToEnd();
                                    responseFromServer = err;
                                }
                            }
                        }
                        else
                        {
                            retry = 4;
                            string err = "";
                            if (e.Status == WebExceptionStatus.ProtocolError)
                            {
                                WebResponse resp = e.Response;
                                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                                {
                                    err = sr.ReadToEnd();
                                    responseFromServer = err;
                                }
                            }
                            responseApi = true; break;
                        }
                    }
                }

                if (responseFromServer != "")
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDGetTokenResult)) as JDIDGetTokenResult;
                        if (!string.IsNullOrEmpty(result.access_token) && !string.IsNullOrEmpty(result.refresh_token))
                        {
                            var getTimeExec = DateTimeOffset.FromUnixTimeSeconds(result.time / 1000).UtcDateTime.AddHours(7);
                            var timeExpired = getTimeExec.AddSeconds(result.expires_in).ToString("yyyy-MM-dd HH:mm:ss");
                            DatabaseSQL EDB = new DatabaseSQL(data.DatabasePathErasoft);
                            var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '1', TOKEN = '" + result.access_token + "', REFRESH_TOKEN = '" + result.refresh_token + "', tgl_expired ='" + timeExpired + "'  WHERE CUST = '" + data.no_cust + "'");
                            if (resultquery != 0)
                            {
                                ret.accessToken = result.access_token;
                                ret.tgl_expired = Convert.ToDateTime(timeExpired);
                                ret.refreshToken = result.refresh_token;

                                string sSQLInsert = "INSERT INTO API_LOG_MARKETPLACE(REQUEST_ID,REQUEST_ACTION,REQUEST_DATETIME,REQUEST_ATTRIBUTE_1,REQUEST_ATTRIBUTE_2,REQUEST_STATUS,REQUEST_EXCEPTION,CUST) ";
                                sSQLInsert += "SELECT '" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss") + "' AS REQUEST_ID,'REFRESH_TOKEN_JDID' AS REQUEST_ACTION,DATEADD(HOUR, +7, GETUTCDATE()) AS REQUEST_DATETIME,'" + data.accessToken + "' AS REQUEST_ATTRIBUTE_1,'" + data.refreshToken + "' AS REQUEST_ATTRIBUTE_2,'REFRESH_JDID SUCCESS' AS REQUEST_STATUS, 'SUCCESS' AS REQUEST_EXCEPTION, '" + data.no_cust + "' AS CUST";
                                var resultInsert = EDB.ExecuteSQL("CString", CommandType.Text, sSQLInsert);
                            }
                            else
                            {
                                string sSQLInsert = "INSERT INTO API_LOG_MARKETPLACE(REQUEST_ID,REQUEST_ACTION,REQUEST_DATETIME,REQUEST_ATTRIBUTE_1,REQUEST_ATTRIBUTE_2,REQUEST_STATUS,REQUEST_EXCEPTION,CUST) ";
                                sSQLInsert += "SELECT '" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss") + "' AS REQUEST_ID,'REFRESH_TOKEN_JDID' AS REQUEST_ACTION,DATEADD(HOUR, +7, GETUTCDATE()) AS REQUEST_DATETIME,'" + data.accessToken + "' AS REQUEST_ATTRIBUTE_1,'" + data.refreshToken + "' AS REQUEST_ATTRIBUTE_2,'REFRESH_JDID FAILED' AS REQUEST_STATUS, 'UPDATE TOKEN FAILED' AS REQUEST_EXCEPTION, '" + data.no_cust + "' AS CUST";
                                var resultInsert = EDB.ExecuteSQL("CString", CommandType.Text, sSQLInsert);
                            }
                        }
                        else
                        {
                            var responseToExp = "";
                            if (responseFromServer.Length > 255)
                            {
                                responseToExp = responseFromServer.Substring(0, 255);
                            }
                            else
                            {
                                responseToExp = responseFromServer;
                            }
                            string sSQLInsert = "INSERT INTO API_LOG_MARKETPLACE(REQUEST_ID,REQUEST_ACTION,REQUEST_DATETIME,REQUEST_ATTRIBUTE_1,REQUEST_ATTRIBUTE_2,REQUEST_STATUS,REQUEST_RESULT,REQUEST_EXCEPTION,CUST) ";
                            sSQLInsert += "SELECT '" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss") + "' AS REQUEST_ID,'REFRESH_TOKEN_JDID' AS REQUEST_ACTION,DATEADD(HOUR, +7, GETUTCDATE()) AS REQUEST_DATETIME,'" + data.accessToken + "' AS REQUEST_ATTRIBUTE_1,'" + data.refreshToken + "' AS REQUEST_ATTRIBUTE_2,'REFRESH_JDID FAILED' AS REQUEST_STATUS, 'ACCESS / REFRESH TOKEN NULL' AS REQUEST_RESULT, '" + responseToExp + "' AS REQUEST_EXCEPTION, '" + data.no_cust + "' AS CUST";
                            var resultInsert = EDB.ExecuteSQL("CString", CommandType.Text, sSQLInsert);
                        }
                    }
                    catch (Exception ex)
                    {
                        var responseToExp = "";
                        if (responseFromServer.Length > 255)
                        {
                            responseToExp = responseFromServer.Substring(0, 255);
                        }
                        else
                        {
                            responseToExp = responseFromServer;
                        }
                        string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        string sSQLInsert = "INSERT INTO API_LOG_MARKETPLACE(REQUEST_ID,REQUEST_ACTION,REQUEST_DATETIME,REQUEST_ATTRIBUTE_1,REQUEST_ATTRIBUTE_2,REQUEST_STATUS,REQUEST_RESULT,REQUEST_EXCEPTION,CUST) ";
                        sSQLInsert += "SELECT '" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss") + "' AS REQUEST_ID,'REFRESH_TOKEN_JDID' AS REQUEST_ACTION,DATEADD(HOUR, +7, GETUTCDATE()) AS REQUEST_DATETIME,'" + data.accessToken + "' AS REQUEST_ATTRIBUTE_1,'" + data.refreshToken + "' AS REQUEST_ATTRIBUTE_2,'REFRESH_JDID FAILED' AS REQUEST_STATUS, '" + msg + "' AS REQUEST_RESULT, '" + responseToExp + "' AS REQUEST_EXCEPTION, '" + data.no_cust + "' AS CUST";
                        var resultInsert = EDB.ExecuteSQL("CString", CommandType.Text, sSQLInsert);
                    }
                }
            }
            return ret;
        }

        protected void SetupContext(string DatabasePathErasoft, string uname)
        {
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, DatabasePathErasoft);
            username = uname;
        }

        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke JDID Gagal.")]
        public async Task<string> JD_CreateProduct(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, JDIDAPIDataJob data)
        {
            SetupContext(data.DatabasePathErasoft, data.username);

            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper()).FirstOrDefault();
            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == log_CUST.ToUpper()).FirstOrDefault();
            if (brgInDb == null || marketplace == null)
                return "invalid passing data";
            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum && b.DISPLAY == true).FirstOrDefault();
            if (detailBrg == null)
                return "invalid passing data";

            var listattributeIDGroup = "";
            var listattributeIDAllVariantGroup = "";

            var listattributeIDAllVariantGroup1 = "";
            var listattributeIDAllVariantGroup2 = "";
            var listattributeIDAllVariantGroup3 = "";


            //Start handle description
            var vDescription = brgInDb.Deskripsi;
            if (!string.IsNullOrEmpty(detailBrg.DESKRIPSI_MP))
            {
                if (detailBrg.DESKRIPSI_MP != "null")
                    vDescription = detailBrg.DESKRIPSI_MP;
            }
            vDescription = new StokControllerJob().RemoveSpecialCharacters(WebUtility.HtmlDecode(WebUtility.HtmlDecode(vDescription)));

            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
            //vDescription = vDescription.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
            vDescription = vDescription.Replace("<li>", "- ").Replace("</li>", "\r\n");
            vDescription = vDescription.Replace("<ul>", "").Replace("</ul>", "\r\n");
            vDescription = vDescription.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
            vDescription = vDescription.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
            vDescription = vDescription.Replace("\r\r", "<br />");
            //vDescription = vDescription.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");
            //end add by nurul 20/1/2020, handle <p> dan enter double di shopee

            //add by calvin 10 september 2019
            vDescription = vDescription.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
            vDescription = vDescription.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
            vDescription = vDescription.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
            vDescription = vDescription.Replace("<p>", "").Replace("</p>", "");
            vDescription = vDescription.Replace("\r", "");
            //HttpBody.description = HttpBody.description.Replace("<li>", "- ").Replace("</li>", "\r\n");
            //HttpBody.description = HttpBody.description.Replace("&nbsp;", "");

            //vDescription = System.Text.RegularExpressions.Regex.Replace(vDescription, "<.*?>", String.Empty);
            //end add by calvin 10 september 2019

            //postData += "&short_description=" + Uri.EscapeDataString(vDescription);
            //postData += "&description=" + Uri.EscapeDataString(vDescription);
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
            double qty_stock = 0;
            //qty_stock = brgInDb.ISI;
            //end handle stock

            var weight = Convert.ToDouble(brgInDb.BERAT / 1000);

            var namafull = "";
            namafull = brgInDb.NAMA;
            if (!string.IsNullOrEmpty(brgInDb.NAMA2))
            {
                namafull += " " + brgInDb.NAMA2;
            }
            if (!string.IsNullOrEmpty(brgInDb.NAMA3))
            {
                namafull += " " + brgInDb.NAMA3;
            }
            if (!string.IsNullOrEmpty(detailBrg.NAMA_BARANG_MP))
            {
                namafull = detailBrg.NAMA_BARANG_MP;
            }

            var commonAttribute = "";

            string sMethod = "epi.ware.openapi.SpuApi.publishWare";

            var urlHref = detailBrg.AVALUE_44;
            var paramHref = "";
            if (!string.IsNullOrEmpty(urlHref))
            {
                if (!urlHref.Contains("http://"))
                {
                    urlHref = "http://" + urlHref;
                    paramHref = "\"subtitleHref\":\"" + urlHref + "\", \"subtitleHrefM\":\"" + urlHref + "\",";
                }
            }

            var paramSKUVariant = "";

            if (brgInDb.TYPE == "4") // punya variasi
            {
                //handle variasi product
                #region variasi product
                var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == kodeProduk).ToList();
                var var_strukturVar = ErasoftDbContext.STF02I.Where(p => p.BRG == kodeProduk && p.MARKET == "JDID").ToList().OrderBy(p => p.RECNUM);

                foreach (var itemData in var_stf02)
                {
                    #region varian LV1
                    if (!string.IsNullOrEmpty(itemData.Sort8))
                    {
                        var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == itemData.Sort8).FirstOrDefault();
                        listattributeIDAllVariantGroup1 = variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR;
                        if (!listattributeIDAllVariantGroup.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                            listattributeIDAllVariantGroup += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                    }
                    #endregion

                    #region varian LV2
                    if (!string.IsNullOrEmpty(itemData.Sort9))
                    {
                        var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == itemData.Sort9).FirstOrDefault();
                        listattributeIDAllVariantGroup2 = variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR;
                        if (!listattributeIDAllVariantGroup.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                            listattributeIDAllVariantGroup += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                    }
                    #endregion

                    #region varian LV3
                    if (!string.IsNullOrEmpty(itemData.Sort10))
                    {
                        var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 3 && p.KODE_VAR == itemData.Sort10).FirstOrDefault();
                        listattributeIDAllVariantGroup3 = variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR;
                        if (!listattributeIDAllVariantGroup.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                            listattributeIDAllVariantGroup += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                    }
                    #endregion

                    if (listattributeIDAllVariantGroup1.Length > 0)
                        listattributeIDGroup = listattributeIDAllVariantGroup1;
                    if (listattributeIDAllVariantGroup2.Length > 0)
                        listattributeIDGroup += ";" + listattributeIDAllVariantGroup2;
                    if (listattributeIDAllVariantGroup3.Length > 0)
                        listattributeIDGroup += ";" + listattributeIDAllVariantGroup3;

                    var namafullVariant = "";
                    namafullVariant = itemData.NAMA;
                    if (!string.IsNullOrEmpty(itemData.NAMA2))
                    {
                        namafullVariant += itemData.NAMA2;
                    }
                    if (!string.IsNullOrEmpty(itemData.NAMA3))
                    {
                        namafullVariant += itemData.NAMA3;
                    }

                    var detailBrgMP = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == itemData.BRG.ToUpper() && b.IDMARKET == marketplace.RecNum && b.DISPLAY == true).FirstOrDefault();

                    paramSKUVariant += "{\"costPrice\":" + detailBrgMP.HJUAL + ",\"jdPrice\":" + detailBrgMP.HJUAL + ", \"saleAttributeIds\":\"" + listattributeIDGroup + "\", \"sellerSkuId\":\"" + detailBrgMP.BRG + "\", \"skuName\":\"" + namafullVariant + "\", \"stock\":" + qty_stock + ", \"upc\":\"upc\" } ,";
                }

                if (paramSKUVariant.Length > 0 && listattributeIDAllVariantGroup.Length > 0)
                {
                    paramSKUVariant = paramSKUVariant.Substring(0, paramSKUVariant.Length - 1);
                    listattributeIDAllVariantGroup = listattributeIDAllVariantGroup.Substring(0, listattributeIDAllVariantGroup.Length - 1);
                    commonAttribute = "\"commonAttributeIds\":\"" + listattributeIDAllVariantGroup + "\", ";

                }

                #endregion
                //end handle variasi product
            }
            else
            {

                //commonAttribute = "\"commonAttributeIds\":\"" + commonAttribute + "\", ";

                paramSKUVariant = "{\"costPrice\":" + detailBrg.HJUAL + ",\"jdPrice\":" + detailBrg.HJUAL + ", \"sellerSkuId\":\"" + detailBrg.BRG + "\", \"skuName\":\"" + namafull + "\", \"stock\":" + qty_stock + ", \"upc\":\"upc\" }";
            }


            var paramQualityAsurance = "";
            if (!string.IsNullOrEmpty(detailBrg.ANAME_47))
            {
                paramQualityAsurance = " \"qualityDays\":" + detailBrg.ANAME_47 + ", ";
            }

            var skeyword = "";

            if (!string.IsNullOrEmpty(detailBrg.AVALUE_46))
            {
                skeyword = detailBrg.AVALUE_46 + ",";
            }
            if (!string.IsNullOrEmpty(detailBrg.AVALUE_48))
            {
                skeyword = skeyword + detailBrg.AVALUE_48 + ",";
            }
            if (!string.IsNullOrEmpty(detailBrg.AVALUE_49))
            {
                skeyword = skeyword + detailBrg.AVALUE_49 + ",";
            }

            if (skeyword != null)
                skeyword = skeyword.Substring(0, skeyword.Length - 1);


            string sParamJson = "{\"spuInfo\":{\"spuName\":\"" + namafull + "\", " +
                "\"appDescription\":\"" + vDescription + "\", " +
                "\"description\":\"" + vDescription + "\", \"packageInfo\":\"PAKET INFO\", " +
                "\"brandId\":" + detailBrg.AVALUE_38 + ", \"catId\":" + detailBrg.CATEGORY_CODE + ", " + commonAttribute + " \"isSequenceNumber\":1, \"keywords\":\"" + skeyword + "\", \"productArea\":\"" + detailBrg.ACODE_47 + "\", " +
                "\"crossProductType\":\"1\", \"clearanceType\":\"2\" , \"taxesType\":\"2\", \"countryId\":\"10000000\", " +
                paramHref +
                "\"subtitle\":\"" + detailBrg.AVALUE_43 + "\", \"transportId\":42, \"isQuality\":" + detailBrg.AVALUE_47 + ", " +
                paramQualityAsurance +
                "\"warrantyPeriod\":" + detailBrg.ACODE_41 + ", \"afterSale\":" + detailBrg.ACODE_40 + ", \"whetherCod\":" + detailBrg.AVALUE_45 + ", " +
                "\"weight\":\"" + weight + "\", \"netWeight\":\"" + weight + "\", \"packHeight\":\"" + brgInDb.TINGGI + "\", \"packLong\":\"" + brgInDb.PANJANG + "\", \"packWide\":\"" + brgInDb.LEBAR + "\", \"piece\":" + detailBrg.ACODE_39 + "}, " +
                "\"skuList\":[ " +
                paramSKUVariant +
                //"{\"costPrice\":" + detailBrg.HJUAL + ",\"jdPrice\":" + detailBrg.HJUAL + ", \"sellerSkuId\":\"" + detailBrg.BRG + "\", \"skuName\":\"" + namafull + "\", \"stock\":" + qty_stock + ", \"upc\":\"upc\" }" +
                "" +
                "]}";


            var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
            var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
            if (ret != null)
            {
                if (ret.openapi_msg.ToLower() == "success")
                {
                    var retData = JsonConvert.DeserializeObject(ret.openapi_data, typeof(JDID_DetailResultCreateProduct)) as JDID_DetailResultCreateProduct;
                    if (retData != null)
                    {
                        if (retData.success)
                        {
                            try
                            {
                                if (retData.model != null)
                                {
                                    if (retData.model.skuIdList != null)
                                    {
                                        if (retData.model.skuIdList.Count() > 0)
                                        {
                                            var dataSkuResult = JD_getSKUVariantbySPU(data, Convert.ToString(retData.model.spuId));
                                            if (dataSkuResult != null)
                                            {
                                                var brgMPInduk = "";
                                                foreach (var dataSKU in dataSkuResult.model)
                                                {
                                                    if (brgInDb.TYPE == "4") // punya variasi
                                                    {
                                                        brgMPInduk = Convert.ToString(retData.model.spuId) + ";0";
                                                        //handle variasi product
                                                        #region variasi product
                                                        var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == kodeProduk).ToList();
                                                        foreach (var itemDatas in var_stf02)
                                                        {
                                                            if (dataSKU.sellerSkuId == itemDatas.BRG)
                                                            {
                                                                var item = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == itemDatas.BRG && b.IDMARKET == marketplace.RecNum).SingleOrDefault();
                                                                if (item != null)
                                                                {
                                                                    item.BRG_MP = Convert.ToString(dataSKU.spuId) + ";" + dataSKU.skuId;
                                                                    item.LINK_STATUS = "Buat Produk Berhasil";
                                                                    item.LINK_DATETIME = DateTime.UtcNow.AddHours(7);
                                                                    item.LINK_ERROR = "0;Buat Produk;;";
                                                                    ErasoftDbContext.SaveChanges();
                                                                }
                                                                if (lGambarUploaded.Count() > 0)
                                                                {
                                                                    JD_addSKUMainPicture(data, Convert.ToString(dataSKU.skuId), brgInDb.LINK_GAMBAR_1);
                                                                    JD_addSKUDetailPicture(data, Convert.ToString(dataSKU.skuId), itemDatas.LINK_GAMBAR_1, 1);
                                                                    if (lGambarUploaded.Count() > 1)
                                                                    {
                                                                        for (int i = 1; i < lGambarUploaded.Count(); i++)
                                                                        {
                                                                            var urlImageJDID = "";
                                                                            switch (i)
                                                                            {
                                                                                case 1:
                                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_2;
                                                                                    break;
                                                                                case 2:
                                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_3;
                                                                                    break;
                                                                                case 3:
                                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_4;
                                                                                    break;
                                                                                case 4:
                                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_5;
                                                                                    break;
                                                                            }
                                                                            JD_addSKUDetailPicture(data, Convert.ToString(dataSKU.skuId), urlImageJDID, i + 1);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        #endregion
                                                        //end handle variasi product
                                                    }
                                                    else
                                                    {
                                                        brgMPInduk = Convert.ToString(retData.model.spuId) + ";" + retData.model.skuIdList[0].skuId.ToString();
                                                        if (lGambarUploaded.Count() > 0)
                                                        {
                                                            JD_addSKUMainPicture(data, retData.model.skuIdList[0].skuId.ToString(), brgInDb.LINK_GAMBAR_1);
                                                            if (lGambarUploaded.Count() > 1)
                                                            {
                                                                for (int i = 1; i < lGambarUploaded.Count(); i++)
                                                                {
                                                                    var urlImageJDID = "";
                                                                    switch (i)
                                                                    {
                                                                        case 1:
                                                                            urlImageJDID = brgInDb.LINK_GAMBAR_2;
                                                                            break;
                                                                        case 2:
                                                                            urlImageJDID = brgInDb.LINK_GAMBAR_3;
                                                                            break;
                                                                        case 3:
                                                                            urlImageJDID = brgInDb.LINK_GAMBAR_4;
                                                                            break;
                                                                        case 4:
                                                                            urlImageJDID = brgInDb.LINK_GAMBAR_5;
                                                                            break;
                                                                    }
                                                                    JD_addSKUDetailPicture(data, retData.model.skuIdList[0].skuId.ToString(), urlImageJDID, i + 1);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                var itemDataInduk = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum).SingleOrDefault();
                                                if (itemDataInduk != null)
                                                {
                                                    itemDataInduk.BRG_MP = brgMPInduk;
                                                    itemDataInduk.LINK_STATUS = "Buat Produk Berhasil";
                                                    itemDataInduk.LINK_DATETIME = DateTime.UtcNow.AddHours(7);
                                                    itemDataInduk.LINK_ERROR = "0;Buat Produk;;";
                                                    ErasoftDbContext.SaveChanges();
                                                }

                                                JD_doAuditProduct(data, Convert.ToString(retData.model.spuId), kodeProduk);
                                            }
                                        }

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(Convert.ToString(ex.Message));
                                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                            }
                        }
                        else
                        {
                            throw new Exception(Convert.ToString(retData.message));
                            //currentLog.REQUEST_EXCEPTION = retStok.message;
                            //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        }
                    }
                    else
                    {
                        throw new Exception("No response API. Please contact support.");
                        //currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                        //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    }
                }
                else
                {
                    throw new Exception("API error. Please contact support.");
                    //currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                }
            }
            else
            {
                throw new Exception("API error.Please contact support.");
                //currentLog.REQUEST_EXCEPTION = response;
                //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
            }



            return "";
        }

        public async Task<string> JD_addSKUVariant(JDIDAPIDataJob data, DataAddSKUVariant dataSKU, string sSPUID, string kodeProduk, string urlImage, int? recnum)
        {
            var resultSKUID = "";
            try
            {
                //string[] spuID = sSPUID.Split(';');

                string sMethod = "epi.ware.openapi.SkuApi.addSkuInfo";
                string sParamJson = "{\"spuId\":\"" + sSPUID + "\", \"skuList\": " +
                "[{\"skuName\":\"" + dataSKU.skuName + "\", \"sellerSkuId\":\"" + dataSKU.sellerSkuId + "\", \"saleAttributeIds\":\"" + dataSKU.saleAttributeIds + "\", \"jdPrice\":" + dataSKU.jdPrice + ", " +
                "\"costPrice\":" + dataSKU.costPrice + ", \"stock\":" + dataSKU.stock + ", \"weight\":\"" + dataSKU.weight + "\", \"netWeight\":\"" + dataSKU.netWeight + "\", " +
                "\"packHeight\":\"" + dataSKU.packHeight + "\", \"packLong\":\"" + dataSKU.packLong + "\", \"packWide\":\"" + dataSKU.packWide + "\", \"piece\":" + dataSKU.piece + "}]}";

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_ResultAddSKUMainPicture)) as JDID_ResultAddSKUMainPicture;
                if (ret != null)
                {
                    if (ret.openapi_msg.ToLower() == "success")
                    {
                        if (ret.openapi_data != null)
                        {
                            var res = JsonConvert.DeserializeObject(ret.openapi_data, typeof(JDID_ResultAddSKUVariant)) as JDID_ResultAddSKUVariant;
                            if (res.model.Count() > 0)
                            {
                                resultSKUID = res.model[0].skuId.ToString();

                                var item = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk && b.IDMARKET == recnum).SingleOrDefault();
                                if (item != null)
                                {
                                    item.BRG_MP = sSPUID + ";" + resultSKUID;
                                    item.LINK_STATUS = "Buat Produk Berhasil";
                                    item.LINK_DATETIME = DateTime.UtcNow.AddHours(7);
                                    item.LINK_ERROR = "0;Buat Produk;;";
                                    ErasoftDbContext.SaveChanges();
                                }
                                if (!string.IsNullOrEmpty(urlImage))
                                {
                                    //await JD_addSKUMainPicture(data, Convert.ToString(skuidVar), dataVar.LINK_GAMBAR_1);
                                    await JD_addSKUDetailPicture(data, resultSKUID, urlImage, 1);
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception(ret.openapi_msg.ToString());
                    }
                }
                else
                {
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception(msg);
            }

            return resultSKUID;
        }

        public async Task<string> JD_updateSKU(JDIDAPIDataJob data, string sSKUName, string sSellerSKUID, string sJDPrice, string sCostPrice, string sSKUID)
        {
            try
            {
                //string result = "";
                string sMethod = "epi.ware.openapi.SkuApi.updateSkuInfo";
                string sParamJson = "{ \"skuInfo\": " +
                "{\"skuId\":\"" + sSKUID + "\",  \"skuName\":\"" + sSKUName + "\", \"sellerSkuId\":\"" + sSellerSKUID + "\", \"jdPrice\":" + sJDPrice + ", " +
                "\"costPrice\":" + sCostPrice + "}}";

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_ResultAddSKUMainPicture)) as JDID_ResultAddSKUMainPicture;
                if (ret != null)
                {
                    if (ret.openapi_msg.ToLower() == "success")
                    {
                        if (ret.openapi_data != null)
                        {

                        }
                    }
                    else
                    {
                        throw new Exception(ret.openapi_msg.ToString());
                    }
                }
                else
                {
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception(msg);
            }

            return "";
        }

        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Product {obj} ke JDID Gagal.")]
        public async Task<string> JD_UpdateProduct(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, JDIDAPIDataJob data)
        {
            SetupContext(data.DatabasePathErasoft, data.username);

            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper()).FirstOrDefault();
            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == log_CUST.ToUpper()).FirstOrDefault();
            if (brgInDb == null || marketplace == null)
                return "invalid passing data";
            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum && b.DISPLAY == true).FirstOrDefault();
            if (detailBrg == null)
                return "invalid passing data";

            var listattributeIDGroup = "";
            var listattributeIDAllVariantGroup = "";

            //Start handle description
            var vDescription = brgInDb.Deskripsi;
            if (!string.IsNullOrEmpty(detailBrg.DESKRIPSI_MP))
            {
                if (detailBrg.DESKRIPSI_MP != "null")
                    vDescription = detailBrg.DESKRIPSI_MP;
            }
            vDescription = new StokControllerJob().RemoveSpecialCharacters(WebUtility.HtmlDecode(WebUtility.HtmlDecode(vDescription)));
            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
            //vDescription = vDescription.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
            vDescription = vDescription.Replace("<li>", "- ").Replace("</li>", "\r\n");
            vDescription = vDescription.Replace("<ul>", "").Replace("</ul>", "\r\n");
            vDescription = vDescription.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
            vDescription = vDescription.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
            vDescription = vDescription.Replace("\r\r", "<br />");
            //vDescription = vDescription.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");
            //end add by nurul 20/1/2020, handle <p> dan enter double di shopee

            //add by calvin 10 september 2019
            vDescription = vDescription.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
            vDescription = vDescription.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
            vDescription = vDescription.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
            vDescription = vDescription.Replace("<p>", "").Replace("</p>", "");
            vDescription = vDescription.Replace("\r", "");
            //HttpBody.description = HttpBody.description.Replace("<li>", "- ").Replace("</li>", "\r\n");
            //HttpBody.description = HttpBody.description.Replace("&nbsp;", "");


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
            double qty_stock = 0;
            //qty_stock = brgInDb.ISI;
            //end handle stock

            var weight = Convert.ToDouble(brgInDb.BERAT / 1000);

            var namafull = "";
            namafull = brgInDb.NAMA;
            if (!string.IsNullOrEmpty(brgInDb.NAMA2))
            {
                namafull += " " + brgInDb.NAMA2;
            }
            if (!string.IsNullOrEmpty(brgInDb.NAMA3))
            {
                namafull += " " + brgInDb.NAMA3;
            }
            if (!string.IsNullOrEmpty(detailBrg.NAMA_BARANG_MP))
            {
                namafull = detailBrg.NAMA_BARANG_MP;
            }

            var commonAttribute = "";

            string sMethod = "epi.ware.openapi.SpuApi.updateSpuInfo";

            var urlHref = detailBrg.AVALUE_44;
            if (!string.IsNullOrEmpty(urlHref))
            {
                if (!urlHref.Contains("http://"))
                {
                    urlHref = "http://" + urlHref;
                }
            }
            string[] spuID = detailBrg.BRG_MP.Split(';');

            var paramQualityAsurance = "";
            if (!string.IsNullOrEmpty(detailBrg.ANAME_47))
            {
                paramQualityAsurance = " \"qualityDays\":" + detailBrg.ANAME_47 + ", ";
            }

            //var paramSKUVariant = "";

            if (brgInDb.TYPE == "4") // punya variasi
            {
                //handle variasi product
                #region variasi product
                var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == kodeProduk).ToList();
                var var_strukturVar = ErasoftDbContext.STF02I.Where(p => p.BRG == kodeProduk && p.MARKET == "JDID").ToList().OrderBy(p => p.RECNUM);

                foreach (var itemData in var_stf02)
                {
                    var brgSTF02hCek = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == itemData.BRG.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
                    if (!string.IsNullOrEmpty(brgSTF02hCek.BRG_MP))
                    {
                        #region varian LV1
                        if (!string.IsNullOrEmpty(itemData.Sort8))
                        {
                            var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == itemData.Sort8).FirstOrDefault();
                            if (!listattributeIDAllVariantGroup.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                                listattributeIDAllVariantGroup += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                        }
                        #endregion

                        #region varian LV2
                        if (!string.IsNullOrEmpty(itemData.Sort9))
                        {
                            var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == itemData.Sort9).FirstOrDefault();
                            if (!listattributeIDAllVariantGroup.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                                listattributeIDAllVariantGroup += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                        }
                        #endregion

                        #region varian LV3
                        if (!string.IsNullOrEmpty(itemData.Sort10))
                        {
                            var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 3 && p.KODE_VAR == itemData.Sort10).FirstOrDefault();
                            if (!listattributeIDAllVariantGroup.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                                listattributeIDAllVariantGroup += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                        }
                        #endregion
                    }

                    var namafullVariant = "";
                    namafullVariant = itemData.NAMA;
                    if (!string.IsNullOrEmpty(itemData.NAMA2))
                    {
                        namafullVariant += itemData.NAMA2;
                    }
                    if (!string.IsNullOrEmpty(itemData.NAMA3))
                    {
                        namafullVariant += itemData.NAMA3;
                    }
                }

                if (listattributeIDAllVariantGroup.Length > 0)
                {
                    listattributeIDAllVariantGroup = listattributeIDAllVariantGroup.Substring(0, listattributeIDAllVariantGroup.Length - 1);
                    commonAttribute = "\"commonAttributeIds\":\"" + listattributeIDAllVariantGroup + "\", ";

                }

                #endregion
                //end handle variasi product
            }
            else
            {
                //paramSKUVariant = "{\"costPrice\":" + detailBrg.HJUAL + ",\"jdPrice\":" + detailBrg.HJUAL + ", \"sellerSkuId\":\"" + detailBrg.BRG + "\", \"skuName\":\"" + namafull + "\", \"stock\":" + qty_stock + ", \"upc\":\"upc\" }";
            }

            var skeyword = "";

            if (!string.IsNullOrEmpty(detailBrg.AVALUE_46))
            {
                skeyword = detailBrg.AVALUE_46 + ",";
            }
            if (!string.IsNullOrEmpty(detailBrg.AVALUE_48))
            {
                skeyword = skeyword + detailBrg.AVALUE_48 + ",";
            }
            if (!string.IsNullOrEmpty(detailBrg.AVALUE_49))
            {
                skeyword = skeyword + detailBrg.AVALUE_49 + ",";
            }

            if (skeyword != null)
                skeyword = skeyword.Substring(0, skeyword.Length - 1);

            string sParamJson = "{\"spuInfo\":{\"spuName\":\"" + namafull + "\", \"spuId\":" + spuID[0] + ", " +
                //"\"packageInfo\":\"PAKET INFO\", " +
                "\"brandId\":" + detailBrg.AVALUE_38 + ", \"catId\":" + detailBrg.CATEGORY_CODE + ", " + commonAttribute + " \"isSequenceNumber\":1, \"keywords\":\"" + skeyword + "\", \"productArea\":\"" + detailBrg.ACODE_47 + "\", " +
                "\"crossProductType\":\"1\", \"clearanceType\":\"2\" , \"taxesType\":\"2\", \"countryId\":\"10000000\", " +
                "\"subtitle\":\"" + detailBrg.AVALUE_43 + "\", \"subtitleHref\":\"" + urlHref + "\", \"subtitleHrefM\":\"" + urlHref + "\", \"transportId\":42, \"isQuality\":" + detailBrg.AVALUE_47 + ", " +
                paramQualityAsurance +
                "\"warrantyPeriod\":" + detailBrg.ACODE_41 +
                //", \"afterSale\":" + Convert.ToInt32(detailBrg.ACODE_40) + 
                ", \"whetherCod\":" + detailBrg.AVALUE_45 + ", " +
                "\"weight\":\"" + weight + "\",  \"Piece\": " + detailBrg.ACODE_39 + ", \"netWeight\":\"" + weight + "\", \"packHeight\":\"" + brgInDb.TINGGI + "\", \"packLong\":\"" + brgInDb.PANJANG + "\", \"packWide\":\"" + brgInDb.LEBAR + "\"," +
                "\"appDescription\":\"" + vDescription + "\"" +
                //", \"description\":\"" + vDescription + "\"" + 
                "}}";
            //"\"skuList\":[ " +
            //paramSKUVariant +
            ////"{\"costPrice\":" + detailBrg.HJUAL + ", \"jdPrice\":" + detailBrg.HJUAL + ", \"sellerSkuId\":\"" + detailBrg.BRG + "\", \"skuName\":\"" + namafull + "\", \"stock\":" + qty_stock + ", \"upc\":\"upc\" }" +
            //"]" +


            var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
            var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
            if (ret != null)
            {
                if (ret.openapi_msg.ToLower() == "success")
                {
                    var retData = JsonConvert.DeserializeObject(ret.openapi_data, typeof(JDID_DetailResultUpdateProduct)) as JDID_DetailResultUpdateProduct;
                    if (retData != null)
                    {
                        if (retData.success)
                        {
                            try
                            {
                                if (retData.model)
                                {
                                    var dataSkuResult = JD_getSKUVariantbySPU(data, spuID[0]);

                                    if (dataSkuResult != null)
                                    {
                                        var urutanGambar = 0;
                                        var listattributeIDAllVariantGroupCreate = "";
                                        var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == kodeProduk).ToList();
                                        var var_strukturVar = ErasoftDbContext.STF02I.Where(p => p.BRG == kodeProduk && p.MARKET == "JDID").ToList().OrderBy(p => p.RECNUM);

                                        foreach (var dataVar in var_stf02)
                                        {
                                            if (brgInDb.TYPE == "4") // punya variasi
                                            {
                                                var brgSTF02h = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == dataVar.BRG.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
                                                if (!string.IsNullOrEmpty(brgSTF02h.BRG_MP))
                                                {
                                                    if (dataSkuResult.model.Count() > 0)
                                                    {
                                                        foreach (var dataSKU in dataSkuResult.model)
                                                        {
                                                            if (dataSKU.sellerSkuId == dataVar.BRG)
                                                            {
                                                                urutanGambar = urutanGambar + 1;
                                                                var namafullVariant = "";
                                                                namafullVariant = dataVar.NAMA;
                                                                if (!string.IsNullOrEmpty(dataVar.NAMA2))
                                                                {
                                                                    namafullVariant += dataVar.NAMA2;
                                                                }
                                                                if (!string.IsNullOrEmpty(dataVar.NAMA3))
                                                                {
                                                                    namafullVariant += dataVar.NAMA3;
                                                                }

                                                                await JD_updateSKU(data, namafullVariant, dataVar.BRG, brgSTF02h.HJUAL.ToString(), brgSTF02h.HJUAL.ToString(), dataSKU.skuId.ToString());


                                                                if (lGambarUploaded.Count() > 0)
                                                                {
                                                                    if (!string.IsNullOrEmpty(dataVar.LINK_GAMBAR_1))
                                                                    {
                                                                        await JD_addSKUMainPicture(data, Convert.ToString(dataSKU.skuId), dataVar.LINK_GAMBAR_1);
                                                                        await JD_addSKUDetailPicture(data, Convert.ToString(dataSKU.skuId), dataVar.LINK_GAMBAR_1, urutanGambar);
                                                                    }

                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {

                                                    #region varian LV1
                                                    if (!string.IsNullOrEmpty(dataVar.Sort8))
                                                    {
                                                        var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == dataVar.Sort8).FirstOrDefault();
                                                        if (!listattributeIDAllVariantGroupCreate.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                                                            listattributeIDAllVariantGroupCreate += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                                                    }
                                                    #endregion

                                                    #region varian LV2
                                                    if (!string.IsNullOrEmpty(dataVar.Sort9))
                                                    {
                                                        var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == dataVar.Sort9).FirstOrDefault();
                                                        if (!listattributeIDAllVariantGroupCreate.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                                                            listattributeIDAllVariantGroupCreate += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                                                    }
                                                    #endregion

                                                    if (listattributeIDAllVariantGroupCreate.Length > 0)
                                                    {
                                                        listattributeIDAllVariantGroupCreate = listattributeIDAllVariantGroupCreate.Substring(0, listattributeIDAllVariantGroupCreate.Length - 1);
                                                    }

                                                    var namafullVariant = "";
                                                    namafullVariant = dataVar.NAMA;
                                                    if (!string.IsNullOrEmpty(dataVar.NAMA2))
                                                    {
                                                        namafullVariant += dataVar.NAMA2;
                                                    }
                                                    if (!string.IsNullOrEmpty(dataVar.NAMA3))
                                                    {
                                                        namafullVariant += dataVar.NAMA3;
                                                    }

                                                    DataAddSKUVariant dataSKUVar = new DataAddSKUVariant();
                                                    dataSKUVar.sellerSkuId = dataVar.BRG;
                                                    dataSKUVar.skuName = namafullVariant;
                                                    dataSKUVar.saleAttributeIds = listattributeIDAllVariantGroupCreate;
                                                    dataSKUVar.stock = 0;
                                                    dataSKUVar.weight = Convert.ToString(weight);
                                                    dataSKUVar.piece = Convert.ToInt32(detailBrg.ACODE_41);
                                                    dataSKUVar.packWide = Convert.ToString(brgInDb.LEBAR);
                                                    dataSKUVar.packLong = Convert.ToString(brgInDb.PANJANG);
                                                    dataSKUVar.packHeight = Convert.ToString(brgInDb.TINGGI);
                                                    dataSKUVar.netWeight = Convert.ToString(weight);
                                                    dataSKUVar.jdPrice = Convert.ToInt64(brgSTF02h.HJUAL);
                                                    dataSKUVar.costPrice = Convert.ToInt64(brgSTF02h.HJUAL);

                                                    await JD_addSKUVariant(data, dataSKUVar, dataSkuResult.model[0].spuId.ToString(), dataVar.BRG, dataVar.LINK_GAMBAR_1, marketplace.RecNum);

                                                    listattributeIDAllVariantGroupCreate = "";
                                                }
                                            }
                                            else
                                            {
                                                if (dataSkuResult.model.Count() > 0)
                                                {
                                                    foreach (var dataSKU in dataSkuResult.model)
                                                    {
                                                        var namafullVariant = "";
                                                        namafullVariant = brgInDb.NAMA;
                                                        if (!string.IsNullOrEmpty(brgInDb.NAMA2))
                                                        {
                                                            namafullVariant += brgInDb.NAMA2;
                                                        }
                                                        if (!string.IsNullOrEmpty(brgInDb.NAMA3))
                                                        {
                                                            namafullVariant += brgInDb.NAMA3;
                                                        }

                                                        await JD_updateSKU(data, namafullVariant, detailBrg.BRG, detailBrg.HJUAL.ToString(), detailBrg.HJUAL.ToString(), dataSKU.skuId.ToString());


                                                        if (lGambarUploaded.Count() > 0)
                                                        {
                                                            await JD_addSKUMainPicture(data, dataSKU.skuId.ToString(), brgInDb.LINK_GAMBAR_1);
                                                            if (lGambarUploaded.Count() > 1)
                                                            {
                                                                for (int i = 1; i < lGambarUploaded.Count(); i++)
                                                                {
                                                                    var urlImageJDID = "";
                                                                    switch (i)
                                                                    {
                                                                        case 1:
                                                                            urlImageJDID = brgInDb.LINK_GAMBAR_2;
                                                                            break;
                                                                        case 2:
                                                                            urlImageJDID = brgInDb.LINK_GAMBAR_3;
                                                                            break;
                                                                        case 3:
                                                                            urlImageJDID = brgInDb.LINK_GAMBAR_4;
                                                                            break;
                                                                        case 4:
                                                                            urlImageJDID = brgInDb.LINK_GAMBAR_5;
                                                                            break;
                                                                    }
                                                                    await JD_addSKUDetailPicture(data, dataSKU.skuId.ToString(), urlImageJDID, i + 1);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }


                                        //foreach (var dataSKU in dataSkuResult.model)
                                        //{
                                        //    if (brgInDb.TYPE == "4") // punya variasi
                                        //    {
                                        //        //handle variasi product
                                        //        #region variasi product
                                        //        var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == kodeProduk).ToList();
                                        //        //var var_strukturVar = ErasoftDbContext.STF02I.Where(p => p.BRG == kodeProduk && p.MARKET == "JDID").ToList().OrderBy(p => p.RECNUM);

                                        //        foreach (var itemDatas in var_stf02)
                                        //        {
                                        //            if (dataSKU.sellerSkuId == itemDatas.BRG)
                                        //            {
                                        //                urutanGambar = urutanGambar + 1;
                                        //                var namafullVariant = "";
                                        //                namafullVariant = itemDatas.NAMA;
                                        //                if (!string.IsNullOrEmpty(itemDatas.NAMA2))
                                        //                {
                                        //                    namafullVariant += itemDatas.NAMA2;
                                        //                }
                                        //                if (!string.IsNullOrEmpty(itemDatas.NAMA3))
                                        //                {
                                        //                    namafullVariant += itemDatas.NAMA3;
                                        //                }

                                        //                var detailBrgMP = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == itemDatas.BRG.ToUpper() && b.IDMARKET == marketplace.RecNum && b.DISPLAY == true).FirstOrDefault();

                                        //                JD_updateSKU(data, namafullVariant, itemDatas.BRG, detailBrgMP.HJUAL.ToString(), detailBrgMP.HJUAL.ToString(), dataSKU.skuId.ToString());

                                        //                if (lGambarUploaded.Count() > 0)
                                        //                {
                                        //                    if (!string.IsNullOrEmpty(itemDatas.LINK_GAMBAR_1))
                                        //                    {
                                        //                        JD_addSKUMainPicture(data, Convert.ToString(dataSKU.skuId), itemDatas.LINK_GAMBAR_1);
                                        //                        JD_addSKUDetailPicture(data, Convert.ToString(dataSKU.skuId), itemDatas.LINK_GAMBAR_1, urutanGambar);
                                        //                    }

                                        //                }
                                        //            }
                                        //        }

                                        //        //if (lGambarUploaded.Count() > 0)
                                        //        //{
                                        //        //    for (int i = 1; i < lGambarUploaded.Count() + 1; i++)
                                        //        //    {
                                        //        //        var urlImageJDID = "";
                                        //        //        switch (i)
                                        //        //        {
                                        //        //            case 1:
                                        //        //                urlImageJDID = brgInDb.LINK_GAMBAR_1;
                                        //        //                break;
                                        //        //            case 2:
                                        //        //                urlImageJDID = brgInDb.LINK_GAMBAR_2;
                                        //        //                break;
                                        //        //            case 3:
                                        //        //                urlImageJDID = brgInDb.LINK_GAMBAR_3;
                                        //        //                break;
                                        //        //            case 4:
                                        //        //                urlImageJDID = brgInDb.LINK_GAMBAR_4;
                                        //        //                break;
                                        //        //            case 5:
                                        //        //                urlImageJDID = brgInDb.LINK_GAMBAR_5;
                                        //        //                break;
                                        //        //        }
                                        //        //        JD_addSKUDetailPicture(data, Convert.ToString(dataSKU.skuId), urlImageJDID, urutanGambar + i);
                                        //        //    }
                                        //        //}

                                        //        #endregion
                                        //        //end handle variasi product
                                        //    }
                                        //    else
                                        //    {
                                        //        var namafullVariant = "";
                                        //        namafullVariant = brgInDb.NAMA;
                                        //        if (!string.IsNullOrEmpty(brgInDb.NAMA2))
                                        //        {
                                        //            namafullVariant += brgInDb.NAMA2;
                                        //        }
                                        //        if (!string.IsNullOrEmpty(brgInDb.NAMA3))
                                        //        {
                                        //            namafullVariant += brgInDb.NAMA3;
                                        //        }

                                        //        JD_updateSKU(data, namafullVariant, detailBrg.BRG, detailBrg.HJUAL.ToString(), detailBrg.HJUAL.ToString(), dataSKU.skuId.ToString());

                                        //        if (lGambarUploaded.Count() > 0)
                                        //        {
                                        //            JD_addSKUMainPicture(data, dataSKU.skuId.ToString(), brgInDb.LINK_GAMBAR_1);
                                        //            if (lGambarUploaded.Count() > 1)
                                        //            {
                                        //                for (int i = 1; i < lGambarUploaded.Count(); i++)
                                        //                {
                                        //                    var urlImageJDID = "";
                                        //                    switch (i)
                                        //                    {
                                        //                        case 1:
                                        //                            urlImageJDID = brgInDb.LINK_GAMBAR_2;
                                        //                            break;
                                        //                        case 2:
                                        //                            urlImageJDID = brgInDb.LINK_GAMBAR_3;
                                        //                            break;
                                        //                        case 3:
                                        //                            urlImageJDID = brgInDb.LINK_GAMBAR_4;
                                        //                            break;
                                        //                        case 4:
                                        //                            urlImageJDID = brgInDb.LINK_GAMBAR_5;
                                        //                            break;
                                        //                    }
                                        //                    JD_addSKUDetailPicture(data, dataSKU.skuId.ToString(), urlImageJDID, i + 1);
                                        //                }
                                        //            }
                                        //        }
                                        //    }
                                        //}
                                        JD_doAuditProduct(data, Convert.ToString(dataSkuResult.model[0].spuId), kodeProduk);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(Convert.ToString(ex.Message));
                                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                            }
                        }
                        else
                        {
                            throw new Exception(Convert.ToString(retData.message));
                            //currentLog.REQUEST_EXCEPTION = retStok.message;
                            //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        }
                    }
                    else
                    {
                        throw new Exception("No response API. Please contact support.");
                        //currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                        //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    }
                }
                else
                {
                    throw new Exception("API error. Please contact support.");
                    //currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                }
            }
            else
            {
                throw new Exception("API error. Please contact support.");
                //currentLog.REQUEST_EXCEPTION = response;
                //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
            }



            return "";
        }

        public JDID_GetSKUVariantbySPU JD_getSKUVariantbySPU(JDIDAPIDataJob data, string sSPUID)
        {
            JDID_GetSKUVariantbySPU datasku = new JDID_GetSKUVariantbySPU();
            try
            {
                string[] spuID = sSPUID.Split(';');

                string sMethod = "com.jd.eptid.warecenter.api.ware.WarePlusClient.getSkuInfoBySpuId";
                string sParamJson = "[" + sSPUID + "]";

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_ResultAddSKUMainPicture)) as JDID_ResultAddSKUMainPicture;
                if (ret != null)
                {
                    if (ret.openapi_msg.ToLower() == "success")
                    {
                        if (ret.openapi_data != null)
                        {
                            var res = JsonConvert.DeserializeObject(ret.openapi_data, typeof(JDID_GetSKUVariantbySPU)) as JDID_GetSKUVariantbySPU;
                            datasku = res;
                        }
                    }
                    else
                    {
                        throw new Exception(ret.openapi_msg.ToString());
                    }
                }
                else
                {
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception(msg);
            }
            return datasku;
        }

        public async Task<string> JD_addSKUMainPicture(JDIDAPIDataJob data, string skuID, string urlPicture)
        {
            try
            {
                string sMethod = "epi.ware.openapi.SkuApi.saveSkuMainPic";
                string sParamJson = "{\"skuId\":\"" + skuID + "\",\"fileName\":\"" + urlPicture + "\"}";
                string sParamFile = urlPicture;

                var response = Call4BigData(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson, sParamFile);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_ResultAddSKUMainPicture)) as JDID_ResultAddSKUMainPicture;
                if (ret != null)
                {
                    if (ret.openapi_msg.ToLower() == "success")
                    {
                        if (ret.openapi_data != null)
                        {
                            //var result = JsonConvert.DeserializeObject(response, typeof(JDID_DetailResultAddSKUMainPicture)) as JDID_DetailResultAddSKUMainPicture;
                            //if (result.success == true)
                            //{

                            //}
                            //else
                            //{
                            //    throw new Exception(result.message.ToString());
                            //}
                        }
                    }
                    else
                    {
                        throw new Exception(ret.openapi_msg.ToString());
                    }
                }
                else
                {
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception(msg);
            }

            return "";
        }

        public async Task<string> JD_addSKUDetailPicture(JDIDAPIDataJob data, string skuID, string urlPicture, int urutan)
        {
            try
            {
                string sMethod = "epi.ware.openapi.SkuApi.saveSkuDetailPic";
                string sParamJson = "{\"skuId\":\"" + skuID + "\",\"fileName\":\"" + urlPicture + "\", \"order\":\"" + urutan + "\"}";
                string sParamFile = urlPicture;

                var response = Call4BigData(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson, sParamFile);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_ResultAddSKUMainPicture)) as JDID_ResultAddSKUMainPicture;
                if (ret != null)
                {
                    if (ret.openapi_msg.ToLower() == "success")
                    {
                        if (ret.openapi_data != null)
                        {
                            //var result = JsonConvert.DeserializeObject(response, typeof(JDID_DetailResultAddSKUMainPicture)) as JDID_DetailResultAddSKUMainPicture;
                            //if (result.success == true)
                            //{

                            //}
                            //else
                            //{
                            //    throw new Exception(result.message.ToString());
                            //}
                        }
                    }
                    else
                    {
                        throw new Exception(ret.openapi_msg.ToString());
                    }
                }
                else
                {
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception(msg);
            }

            return "";
        }

        public async Task<string> JD_doAuditProduct(JDIDAPIDataJob data, string spuid, string brg)
        {
            try
            {
                string sMethod = "epi.ware.openapi.WareOperation.doAudit";
                string sParamJson = "{\"spuIds\":\"" + spuid + "\"}";

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_ResultAddSKUMainPicture)) as JDID_ResultAddSKUMainPicture;
                if (ret != null)
                {
                    if (ret.openapi_msg.ToLower() == "success")
                    {
                        if (ret.openapi_data != null)
                        {
                            //var result = JsonConvert.DeserializeObject(response, typeof(JDID_DetailResultAddSKUMainPicture)) as JDID_DetailResultAddSKUMainPicture;
                            //if (result.success == true)
                            //{

                            //}
                            //else
                            //{
                            //    throw new Exception(result.message.ToString());
                            //}
                        }
                        var tblCustomer = ErasoftDbContext.ARF01.Where(m => m.CUST == data.no_cust).FirstOrDefault();
                        if (tblCustomer.TIDAK_HIT_UANG_R)
                        {
                            var stf02 = ErasoftDbContext.STF02.Where(m => m.BRG == brg).FirstOrDefault();
                            if (stf02 != null)
                            {
                                MasterOnline.Controllers.JDIDAPIData dataStok = new MasterOnline.Controllers.JDIDAPIData()
                                {
                                    accessToken = data.accessToken,
                                    appKey = data.appKey,
                                    appSecret = data.appSecret,
                                };
                                StokControllerJob stokAPI = new StokControllerJob(data.DatabasePathErasoft, username);
                                if (stf02.TYPE == "4")
                                {
                                    var listStf02 = ErasoftDbContext.STF02.Where(m => m.PART == brg).ToList();
                                    foreach (var barang in listStf02)
                                    {
                                        var stf02h = ErasoftDbContext.STF02H.Where(m => m.BRG == barang.BRG && m.IDMARKET == tblCustomer.RecNum).FirstOrDefault();
                                        if (stf02h != null)
                                        {
                                            if (!string.IsNullOrEmpty(stf02h.BRG_MP))
                                            {
#if (DEBUG || Debug_AWS)
                                                Task.Run(() => stokAPI.JD_updateStock(data.DatabasePathErasoft, stf02h.BRG, tblCustomer.CUST, "Stock", "Update Stok", dataStok, stf02h.BRG_MP, 0, username, null)).Wait();
#else
                                                string EDBConnID = EDB.GetConnectionString("ConnId");
                                                var sqlStorage = new SqlServerStorage(EDBConnID);

                                                var Jobclient = new BackgroundJobClient(sqlStorage);
                                                Jobclient.Enqueue<StokControllerJob>(x => x.JD_updateStock(data.DatabasePathErasoft, stf02h.BRG, tblCustomer.CUST, "Stock", "Update Stok", dataStok, stf02h.BRG_MP, 0, username, null));
#endif
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var stf02h = ErasoftDbContext.STF02H.Where(m => m.BRG == stf02.BRG && m.IDMARKET == tblCustomer.RecNum).FirstOrDefault();
                                    if (stf02h != null)
                                    {
                                        if (!string.IsNullOrEmpty(stf02h.BRG_MP))
                                        {
#if (DEBUG || Debug_AWS)
                                            Task.Run(() => stokAPI.JD_updateStock(data.DatabasePathErasoft, stf02h.BRG, tblCustomer.CUST, "Stock", "Update Stok", dataStok, stf02h.BRG_MP, 0, username, null)).Wait();
#else
                                            string EDBConnID = EDB.GetConnectionString("ConnId");
                                            var sqlStorage = new SqlServerStorage(EDBConnID);

                                            var Jobclient = new BackgroundJobClient(sqlStorage);
                                            Jobclient.Enqueue<StokControllerJob>(x => x.JD_updateStock(data.DatabasePathErasoft, stf02h.BRG, tblCustomer.CUST, "Stock", "Update Stok", dataStok, stf02h.BRG_MP, 0, username, null));
#endif
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception(ret.openapi_msg.ToString());
                    }
                }
                else
                {
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception(msg);
            }

            return "";
        }

        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Harga Jual Produk {obj} ke JD.ID gagal.")]
        public async Task<string> JD_updatePrice(string DatabasePathErasoft, string stf02_brg, string log_CUST, string log_ActionCategory, string log_ActionName, JDIDAPIDataJob data, string id, int price, string uname)
        {
            SetupContext(DatabasePathErasoft, uname);

            try
            {
                var brgMp = "";
                if (id.Contains(";"))
                {
                    string[] brgSplit = id.Split(';');
                    if (brgSplit[1] != "0")
                    {
                        brgMp = brgSplit[1].ToString();
                    }
                }
                else
                {
                    brgMp = id;
                }
                string sMethod = "epi.ware.openapi.SkuApi.updateSkuInfo";
                string sParamJson = "{\"skuInfo\":{\"skuId\":" + brgMp + ", \"jdPrice\":" + price + "}}";

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (ret != null)
                {
                    if (ret.openapi_msg.ToLower() == "success")
                    {
                        var retPrice = JsonConvert.DeserializeObject(ret.openapi_data, typeof(Data_UpPriceJob)) as Data_UpPriceJob;
                        if (retPrice != null)
                        {
                            if (retPrice.success)
                            {
                                //add 19 sept 2020, update harga massal
                                if (log_ActionName.Contains("UPDATE_MASSAL"))
                                {
                                    var dataLog = log_ActionName.Split('_');
                                    if (dataLog.Length >= 4)
                                    {
                                        var nobuk = dataLog[2];
                                        var indexData = Convert.ToInt32(dataLog[3]);
                                        var log_b = ErasoftDbContext.LOG_HARGAJUAL_B.Where(m => m.NO_BUKTI == nobuk && m.NO_FILE == indexData).FirstOrDefault();
                                        if (log_b != null)
                                        {
                                            var currentProgress = log_b.KET.Split('/');
                                            if (currentProgress.Length == 2)
                                            {
                                                log_b.KET = (Convert.ToInt32(currentProgress[0]) + 1) + "/" + currentProgress[1];
                                                ErasoftDbContext.SaveChanges();
                                            }
                                        }
                                    }
                                }
                                //end add 19 sept 2020, update harga massal
                            }
                            else
                            {
                                throw new Exception(retPrice.message.ToString());
                            }
                        }
                        else
                        {
                            throw new Exception(ret.openapi_msg.ToString());
                        }
                    }
                    else
                    {
                        throw new Exception(ret.openapi_msg.ToString());
                    }
                }
                else
                {
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception(msg);
            }

            return "";
        }

        [System.Web.Mvc.HttpGet]
        public void getCategory(JDIDAPIDataJob data)
        {
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Category",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = data.accessToken,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            //var mgrApiManager = new JDIDControllerJob();
            //mgrApiManager.AppKey = data.appKey;
            //mgrApiManager.AppSecret = data.appSecret;
            //mgrApiManager.AccessToken = data.accessToken;
            //mgrApiManager.Method = "epi.ware.openapi.CategoryApi.getAllCategoryTree";
            //mgrApiManager.ParamJson = "";

            string sMethod = "epi.ware.openapi.CategoryApi.getAllCategoryTree";
            string sParamJson = "";

            try
            {
                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (ret != null)
                {
                    if (ret.openapi_code == 0)
                    {
                        var listKategori = JsonConvert.DeserializeObject(ret.openapi_data, typeof(DATA_CATJob)) as DATA_CATJob;
                        if (listKategori != null)
                        {
                            if (listKategori.success)
                            {
                                EDB.ExecuteSQL("CString", CommandType.Text, "Update ARF01 SET STATUS_API = '1' WHERE TOKEN = '" + data.accessToken + "' AND API_KEY = '" + data.appKey + "'");
                                string dbPath = "";
                                //var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
                                //if (sessionData?.Account != null)
                                //{
                                //    dbPath = sessionData.Account.DatabasePathErasoft;
                                //}
                                //else
                                //{
                                //    if (sessionData?.User != null)
                                //    {
                                //        var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                                //        dbPath = accFromUser.DatabasePathErasoft;
                                //    }
                                //}

                                var sessionAccount = System.Web.HttpContext.Current.Session["SessionAccount"];
                                var sessionAccountDatabasePathErasoft = System.Web.HttpContext.Current.Session["SessionAccountDatabasePathErasoft"];

                                var sessionUser = System.Web.HttpContext.Current.Session["SessionUser"];
                                var sessionUserAccountID = System.Web.HttpContext.Current.Session["SessionUserAccountID"];

                                if (sessionAccount != null)
                                {
                                    dbPath = sessionAccountDatabasePathErasoft.ToString();
                                }
                                else
                                {
                                    if (sessionUser != null)
                                    {
                                        var userAccID = Convert.ToInt64(sessionUserAccountID);
                                        var accFromUser = MoDbContext.Account.Single(a => a.AccountId == userAccID);
                                        dbPath = accFromUser.DatabasePathErasoft;
                                    }
                                }

                                #region connstring
                                //#if AWS
                                //                    string con = "Data Source=localhost;Initial Catalog=" + dbPath + ";Persist Security Info=True;User ID=sa;Password=admin123^";
                                //#elif Debug_AWS
                                //                                string con = "Data Source=13.250.232.74;Initial Catalog=" + dbPath + ";Persist Security Info=True;User ID=sa;Password=admin123^";
                                //#else
                                //                                string con = "Data Source=13.251.222.53;Initial Catalog=" + dbPath + ";Persist Security Info=True;User ID=sa;Password=admin123^";
                                //#endif
                                #endregion
                                using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
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
                                        oCommand.CommandText = "INSERT INTO [CATEGORY_JDID] ([CATEGORY_CODE], [CATEGORY_NAME], [CATE_STATE], [TYPE], [LEAF], [PARENT_CODE]) VALUES (@CATEGORY_CODE, @CATEGORY_NAME, @CATE_STATE, @TYPE, @LEAF, @PARENT_CODE)";
                                        //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));
                                        oCommand.Parameters.Add(new SqlParameter("@CATE_STATE", SqlDbType.NVarChar, 3));
                                        oCommand.Parameters.Add(new SqlParameter("@TYPE", SqlDbType.NVarChar, 3));
                                        oCommand.Parameters.Add(new SqlParameter("@LEAF", SqlDbType.NVarChar, 1));
                                        oCommand.Parameters.Add(new SqlParameter("@PARENT_CODE", SqlDbType.NVarChar, 50));

                                        //try
                                        //{
                                        foreach (var item in listKategori.model) //foreach parent level top
                                        {
                                            oCommand.Parameters[0].Value = item.cateId;
                                            oCommand.Parameters[1].Value = item.cateName;
                                            oCommand.Parameters[2].Value = item.cateState;
                                            oCommand.Parameters[3].Value = item.type;
                                            oCommand.Parameters[4].Value = "1";
                                            if (item.parentCateVo != null)
                                            {
                                                oCommand.Parameters[5].Value = item.parentCateVo.cateId;
                                            }
                                            else
                                            {
                                                oCommand.Parameters[5].Value = "";
                                            }
                                            if (oCommand.ExecuteNonQuery() > 0)
                                            {
                                                listCategory.Add(item.cateId);
                                                if (item.parentCateVo != null)
                                                {
                                                    if (!listCategory.Contains(item.parentCateVo.cateId))
                                                    {
                                                        RecursiveInsertCategory(oCommand, item.parentCateVo);
                                                    }
                                                }
                                            }
                                            else
                                            {

                                            }
                                        }
                                        //oTransaction.Commit();
                                        //}
                                        //catch (Exception ex)
                                        //{
                                        //    //oTransaction.Rollback();
                                        //}
                                    }
                                }
                            }
                            else
                            {
                                currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            }
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        }
                    }
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = response;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                }
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
            }


        }
        protected void RecursiveInsertCategory(SqlCommand oCommand, Model_CatJob item)
        {
            //foreach (var child in categories.Where(p => p.parent_id == parent))
            //{

            oCommand.Parameters[0].Value = item.cateId;
            oCommand.Parameters[1].Value = item.cateName;
            oCommand.Parameters[2].Value = item.cateState;
            oCommand.Parameters[3].Value = item.type;
            oCommand.Parameters[4].Value = "0";
            if (item.parentCateVo != null)
            {
                oCommand.Parameters[5].Value = item.parentCateVo.cateId;

            }
            else
            {
                oCommand.Parameters[5].Value = "";
            }

            if (oCommand.ExecuteNonQuery() > 0)
            {
                listCategory.Add(item.cateId);
                if (item.parentCateVo != null)
                {
                    if (!listCategory.Contains(item.parentCateVo.cateId))
                    {
                        RecursiveInsertCategory(oCommand, item.parentCateVo);
                    }
                }
            }
            else
            {

            }
            //}
        }

        public ATTRIBUTE_JDID getAttribute(JDIDAPIDataJob data, string catId)
        {
            var retAttr = new ATTRIBUTE_JDID();
            //var mgrApiManager = new JDIDControllerJob();

            //mgrApiManager.AppKey = data.appKey;
            //mgrApiManager.AppSecret = data.appSecret;
            //mgrApiManager.AccessToken = data.accessToken;

            //mgrApiManager.Method = "epi.ware.openapi.WareAttributeApi.getAttributesByCatId";
            //mgrApiManager.ParamJson = "{ \"catId\":\"" + catId + "\"}";

            string sMethod = "epi.ware.openapi.WareAttributeApi.getAttributesByCatId";
            string sParamJson = "{ \"catId\":\"" + catId + "\"}";

            var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
            var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
            if (ret != null)
            {
                if (ret.openapi_code == 0)
                {
                    var listAttr = JsonConvert.DeserializeObject(ret.openapi_data, typeof(AttrDataJob)) as AttrDataJob;
                    if (listAttr != null)
                    {
                        if (listAttr.model.Count > 0)
                        {
                            string a = "";
                            int i = 0;
                            retAttr.CATEGORY_CODE = catId;
                            foreach (var attr in listAttr.model)
                            {

                                a = Convert.ToString(i + 1);
                                retAttr["ACODE_" + a] = Convert.ToString(attr.propertyId);
                                retAttr["ATYPE_" + a] = attr.type.ToString();
                                retAttr["ANAME_" + a] = attr.nameEn;
                                i = i + 1;
                            }
                            for (int j = i; j < 20; j++)
                            {
                                a = Convert.ToString(j + 1);
                                retAttr["ACODE_" + a] = "";
                                retAttr["ATYPE_" + a] = "";
                                retAttr["ANAME_" + a] = "";
                            }
                        }

                    }
                }
            }
            //return response;
            //return ret.openapi_data;
            return retAttr;
        }

        public List<ATTRIBUTE_OPT_JDID> getAttributeOpt(JDIDAPIDataJob data, string catId, string attrId, int page)
        {
            //var mgrApiManager = new JDIDControllerJob();
            var listOpt = new List<ATTRIBUTE_OPT_JDID>();
            //mgrApiManager.AppKey = data.appKey;
            //mgrApiManager.AppSecret = data.appSecret;
            //mgrApiManager.AccessToken = data.accessToken;

            //mgrApiManager.Method = "epi.ware.openapi.WareAttributeApi.getAttrValuesByCatIdAndAttrId";
            //mgrApiManager.ParamJson = "{ \"catId\":\"" + catId + "\", \"attrId\":\"" + attrId + "\", \"currentPage\":" + page + ", \"pageSize\":20}";

            string sMethod = "epi.ware.openapi.WareAttributeApi.getAttrValuesByCatIdAndAttrId";
            string sParamJson = "{ \"catId\":\"" + catId + "\", \"attrId\":\"" + attrId + "\", \"currentPage\":" + page + ", \"pageSize\":20}";

            var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
            var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
            if (ret != null)
            {
                if (ret.openapi_code == 0)
                {
                    var retOpt = JsonConvert.DeserializeObject(ret.openapi_data, typeof(JDID_ATTRIBUTE_OPTJob)) as JDID_ATTRIBUTE_OPTJob;
                    if (retOpt != null)
                    {
                        if (retOpt.model != null)
                        {
                            if (retOpt.model.data.Count > 0)
                            {
                                foreach (var opt in retOpt.model.data)
                                {
                                    var newOpt = new ATTRIBUTE_OPT_JDID()
                                    {
                                        ACODE = opt.attributeValueId.ToString(),
                                        OPTION_VALUE = opt.nameEn
                                    };
                                    listOpt.Add(newOpt);
                                }
                                //if(retOpt.model.data.Count == 20)
                                //{
                                var cursiveOpt = getAttributeOpt(data, catId, attrId, page + 1);
                                if (cursiveOpt.Count > 0)
                                {
                                    foreach (var opt2 in cursiveOpt)
                                    {
                                        listOpt.Add(opt2);
                                    }
                                }
                                //}
                            }
                        }

                    }
                }
            }


            return listOpt;
        }

        public BindingBase getListProduct(JDIDAPIDataJob data, int page, string cust, int recordCount)
        {
            var ret = new BindingBase
            {
                status = 0,
                recordCount = recordCount,
            };

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Item List",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = (page + 1).ToString(),
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            try
            {
                if (listBrand.Count == 0)
                {
                    getShopBrand(data);
                }
                //var mgrApiManager = new JDIDControllerJob();
                //mgrApiManager.AppKey = data.appKey;
                //mgrApiManager.AppSecret = data.appSecret;
                //mgrApiManager.AccessToken = data.accessToken;
                //mgrApiManager.Method = "com.jd.eptid.warecenter.api.ware.WarePlusClient.getWareTinyInfoListByVenderId";
                //mgrApiManager.ParamJson = (page + 1) + ",10";

                string sMethod = "com.jd.eptid.warecenter.api.ware.WarePlusClient.getWareTinyInfoListByVenderId";
                string sParamJson = (page + 1) + ",10";

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var retData = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (retData != null)
                {
                    if (retData.openapi_msg.ToLower() == "success")
                    {
                        var listProd = JsonConvert.DeserializeObject(retData.openapi_data, typeof(Data_ListProdJob)) as Data_ListProdJob;
                        if (listProd != null)
                        {
                            if (listProd.success)
                            {
                                string msg = "";
                                bool adaError = false;
                                if (listProd.model.spuInfoVoList.Count > 0)
                                {
                                    ret.status = 1;
                                    int IdMarket = ErasoftDbContext.ARF01.Where(c => c.CUST == cust).FirstOrDefault().RecNum.Value;
                                    if (listProd.model.spuInfoVoList.Count == 10)
                                        ret.message = (page + 1).ToString();

                                    foreach (var item in listProd.model.spuInfoVoList)
                                    {
                                        //product status: 1.online,2.offline,3.punish,4.deleted
                                        if (item.wareStatus == 1 || item.wareStatus == 2)
                                        {
                                            var retProd = GetProduct(data, item, IdMarket, cust);
                                            if (retProd.status == 1)
                                            {
                                                ret.recordCount += retProd.recordCount;
                                            }
                                            else
                                            {
                                                adaError = true;
                                                msg += item.spuId + ":" + retProd.message + "___||___";
                                            }
                                        }

                                    }
                                }
                                if (adaError)
                                {
                                    currentLog.REQUEST_EXCEPTION = msg;
                                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                }
                                else
                                {
                                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);
                                }
                            }
                            else
                            {
                                ret.message = retData.openapi_data;
                                currentLog.REQUEST_EXCEPTION = ret.message;
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            }
                        }
                        else
                        {
                            ret.message = retData.openapi_data;
                            currentLog.REQUEST_EXCEPTION = ret.message;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        }
                    }
                    else
                    {
                        ret.message = retData.openapi_msg;
                        currentLog.REQUEST_EXCEPTION = ret.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    }
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
            }
            return ret;
        }

        public BindingBase GetProduct(JDIDAPIDataJob data, SpuinfovolistJob itemFromList, int IdMarket, string cust)
        {
            var ret = new BindingBase
            {
                status = 0,
            };

            try
            {
                //var mgrApiManager = new JDIDControllerJob();
                //mgrApiManager.AppKey = data.appKey;
                //mgrApiManager.AppSecret = data.appSecret;
                //mgrApiManager.AccessToken = data.accessToken;
                //mgrApiManager.Method = "com.jd.eptid.warecenter.api.ware.WarePlusClient.getSkuInfoBySpuId";
                //mgrApiManager.ParamJson = "[" + itemFromList.spuId + "]";

                string sMethod = "com.jd.eptid.warecenter.api.ware.WarePlusClient.getSkuInfoBySpuId";
                string sParamJson = "[" + itemFromList.spuId + "]";

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var retProd = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (retProd != null)
                {
                    if (retProd.openapi_msg.ToLower() == "success")
                    {
                        var dataProduct = JsonConvert.DeserializeObject(retProd.openapi_data, typeof(ProductDataJob)) as ProductDataJob;
                        if (dataProduct != null)
                        {
                            if (dataProduct.success)
                            {
                                var stf02h_local = ErasoftDbContext.STF02H.Where(m => m.IDMARKET == IdMarket).ToList();
                                var tempBrg_local = ErasoftDbContext.TEMP_BRG_MP.Where(m => m.IDMARKET == IdMarket).ToList();

                                var haveVarian = false;
                                if (dataProduct.model.Count > 1)
                                {
                                    haveVarian = true;
                                }

                                foreach (var item in dataProduct.model)
                                {
                                    var tempbrginDB = new TEMP_BRG_MP();
                                    var brgInDB = new STF02H();

                                    if (haveVarian)
                                    {
                                        //handle parent
                                        string kdBrgInduk = item.spuId.ToString();
                                        bool createParent = false;
                                        tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == kdBrgInduk).FirstOrDefault();
                                        brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == kdBrgInduk).FirstOrDefault();
                                        if (tempbrginDB == null && brgInDB == null)
                                        {
                                            if (item.skuId == dataProduct.model[0].skuId)
                                                createParent = true;
                                        }
                                        else if (brgInDB != null)
                                        {
                                            kdBrgInduk = brgInDB.BRG;
                                        }
                                        //end handle parent

                                        //foreach (var varian in item.skuIds)
                                        //{
                                        //    tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == varian.ToString().ToUpper()).FirstOrDefault();
                                        //    brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == varian.ToString().ToUpper()).FirstOrDefault();
                                        //    if (tempbrginDB == null && brgInDB == null)
                                        //    {

                                        //    }
                                        //}
                                        //for (int i = 0; i < item.skuIds.Count; i++)
                                        //{
                                        tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        if (tempbrginDB == null && brgInDB == null)
                                        {
                                            var retData = getProductDetail(data, item, kdBrgInduk, createParent, item.skuId.ToString(), cust, IdMarket, itemFromList);
                                            if (retData.status == 1)
                                            {
                                                ret.recordCount += retData.recordCount;
                                                //createParent = false;
                                            }
                                        }
                                        //}

                                    }
                                    else
                                    {
                                        tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        if (tempbrginDB == null && brgInDB == null)
                                        {
                                            var retData = getProductDetail(data, item, "", false, item.skuId.ToString(), cust, IdMarket, itemFromList);
                                            if (retData.status == 1)
                                            {
                                                ret.recordCount += retData.recordCount;
                                            }
                                        }
                                    }

                                }

                                ret.status = 1;
                            }
                            else
                            {
                                ret.message = retProd.openapi_data;
                            }
                        }
                        else
                        {
                            ret.message = retProd.openapi_data;
                        }
                    }
                    else
                    {
                        ret.message = retProd.openapi_msg;
                    }
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }

            return ret;
        }

        public BindingBase getProductDetail(JDIDAPIDataJob data, Model_ProductJob item, string kdBrgInduk, bool createParent, string skuId, string cust, int IdMarket, SpuinfovolistJob itemFromList)
        {
            var ret = new BindingBase
            {
                status = 0,
            };

            try
            {
                //var mgrApiManager = new JDIDControllerJob();
                //mgrApiManager.AppKey = data.appKey;
                //mgrApiManager.AppSecret = data.appSecret;
                //mgrApiManager.AccessToken = data.accessToken;
                //mgrApiManager.Method = "epi.ware.openapi.SkuApi.getSkuBySkuIds";
                ////mgrApiManager.ParamJson = "{ \"page\":" + page + ", \"pageSize\":10}";
                //mgrApiManager.ParamJson = "{\"skuIds\" : \"" + skuId + "\"}";

                string sMethod = "epi.ware.openapi.SkuApi.getSkuBySkuIds";
                string sParamJson = "{\"skuIds\" : \"" + skuId + "\"}";

                string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, BERAT, PANJANG, LEBAR, TINGGI, CUST, Deskripsi, IDMARKET, HJUAL, HJUAL_MP, ";
                sSQL += "DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, KODE_BRG_INDUK, TYPE, ";
                sSQL += "ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
                sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20) VALUES ";

                string sSQLVal = "";

                //if (!string.IsNullOrEmpty(kdBrgInduk))
                //{
                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var retProd = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (retProd != null)
                {
                    if (retProd.openapi_msg.ToLower() == "success")
                    {
                        var detailData = JsonConvert.DeserializeObject(retProd.openapi_data, typeof(Data_Detail_ProductJob)) as Data_Detail_ProductJob;
                        if (detailData != null)
                        {
                            if (detailData.success)
                            {
                                if (!string.IsNullOrEmpty(kdBrgInduk))
                                {
                                    if (createParent)
                                    {
                                        var retSQL = CreateSQLValue(item, detailData.model[0], kdBrgInduk, "", cust, IdMarket, 1, itemFromList);
                                        if (retSQL.status == 1)
                                            sSQLVal += retSQL.message;
                                    }

                                    var retSQL2 = CreateSQLValue(item, detailData.model[0], kdBrgInduk, skuId, cust, IdMarket, 2, itemFromList);
                                    if (retSQL2.status == 1)
                                        sSQLVal += retSQL2.message;
                                }
                                else
                                {
                                    var retSQL = CreateSQLValue(item, detailData.model[0], "", skuId, cust, IdMarket, 0, itemFromList);
                                    if (retSQL.status == 1)
                                        sSQLVal += retSQL.message;
                                }
                            }
                            else
                            {
                                ret.message = string.IsNullOrEmpty(detailData.message) ? retProd.openapi_data : detailData.message;
                            }
                        }
                        else
                        {
                            ret.message = retProd.openapi_data;
                        }
                    }
                    else
                    {
                        ret.message = retProd.openapi_msg;
                    }
                }
                else
                {
                    ret.message = response;
                }

                if (!string.IsNullOrEmpty(sSQLVal))
                {
                    sSQL = sSQL + sSQLVal;
                    sSQL = sSQL.Substring(0, sSQL.Length - 1);
                    var a = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                    ret.recordCount += a;
                    ret.status = 1;
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }

        protected BindingBase CreateSQLValue(Model_ProductJob item, Model_Detail_ProductJob detItem, string kdBrgInduk, string skuId, string cust, int IdMarket, int typeBrg, SpuinfovolistJob itemFromList)
        {
            // typeBrg : 0 = barang tanpa varian; 1 = barang induk; 2 = barang varian
            var ret = new BindingBase
            {
                status = 0,
            };

            string sSQL_Value = "";
            try
            {

                string[] attrVal;
                string value = "";
                var brgAttribute = new Dictionary<string, string>();
                string namaBrg = item.skuName;
                string nama, nama2, urlImage, urlImage2, urlImage3;
                urlImage = "";
                urlImage2 = "";
                urlImage3 = "";
                var categoryCode = itemFromList.fullCategoryId.Split('/');
                var categoryName = itemFromList.fullCategoryName.Split('/');
                double price = Convert.ToDouble(item.jdPrice);
                //var statusBrg = detItem != null ? detItem.status : 1;
                var statusBrg = detItem.status;
                string brand = itemFromList.brandId.ToString();
                //var display = statusBrg.Equals("active") ? 1 : 0;
                string deskripsi = itemFromList.description;

                if (typeBrg != 1)
                {
                    sSQL_Value += " ( '" + skuId + "' , '" + skuId + "' , '";
                }
                else
                {
                    namaBrg = itemFromList.spuName;
                    sSQL_Value += " ( '" + kdBrgInduk + "' , '" + kdBrgInduk + "' , '";
                }

                if (typeBrg == 2)
                {
                    //namaBrg += " " + detItem.skuName;
                    urlImage = imageUrl + detItem.mainImgUri;
                }
                else
                {
                    urlImage = imageUrl + item.mainImgUri;
                }

                if (namaBrg.Length > 30)
                {
                    nama = namaBrg.Substring(0, 30);
                    if (namaBrg.Length > 285)
                    {
                        nama2 = namaBrg.Substring(30, 255);
                    }
                    else
                    {
                        nama2 = namaBrg.Substring(30);
                    }
                }
                else
                {
                    nama = namaBrg;
                    nama2 = "";
                }


                sSQL_Value += nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' ,";

                //var attrVal = detItem.saleAttributeIds.Split(';');
                //if (detItem == null)
                //{
                //    attrVal = item.saleAttributeIds.Split(';');
                //    foreach (Newtonsoft.Json.Linq.JProperty property in item.saleAttributeNameMap)
                //    {
                //        brgAttribute.Add(property.Name, property.Value.ToString());
                //    }
                //}
                //else
                //{
                attrVal = detItem.saleAttributeIds.Split(';');
                foreach (Newtonsoft.Json.Linq.JProperty property in detItem.saleAttributeNameMap)
                {
                    brgAttribute.Add(property.Name, property.Value.ToString());
                }
                //price = brgAttribute.TryGetValue("jdPrice", out value) ? Convert.ToDouble(value) : Convert.ToDouble(item.jdPrice);
                if (Convert.ToDouble(detItem.jdPrice) > 0)
                    price = Convert.ToDouble(detItem.jdPrice);

                //}
                if (listBrand.Count > 0)
                {
                    var a = listBrand.Where(m => m.brandId == itemFromList.brandId).FirstOrDefault();
                    if (a != null)
                        brand = a.brandName;
                }
                //sSQL_Value += Convert.ToDouble(detItem != null ? detItem.netWeight : item.netWeight) * 1000 + " , " + Convert.ToDouble(detItem != null ? detItem.packLong : item.packLong) + " , ";
                //sSQL_Value += Convert.ToDouble(detItem != null ? detItem.packWide : item.packWide) + " , " + Convert.ToDouble(detItem != null ? detItem.packHeight : item.packHeight);
                sSQL_Value += Convert.ToDouble(detItem.netWeight) * 1000 + " , " + Convert.ToDouble(detItem.packLong) + " , ";
                sSQL_Value += Convert.ToDouble(detItem.packWide) + " , " + Convert.ToDouble(detItem.packHeight);
                sSQL_Value += " , '" + cust + "' , '" + deskripsi.Replace('\'', '`') + "' , " + IdMarket + " , " + price + " , " + price + " , ";
                sSQL_Value += statusBrg + " , '" + categoryCode[categoryCode.Length - 1] + "' , '" + categoryName[categoryName.Length - 1] + "' , '";
                sSQL_Value += brand + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "' , '" + (typeBrg == 2 ? kdBrgInduk : "") + "' , '" + (typeBrg == 1 ? "4" : "3") + "'";
                int i;
                for (i = 0; i < attrVal.Length; i++)
                {
                    var attr = attrVal[i].Split(':');
                    if (attr.Length == 2)
                    {
                        var attrName = (brgAttribute.TryGetValue(attrVal[i], out value) ? value : "").Split(':');

                        sSQL_Value += ",'" + attr[0] + "','" + attrName[0] + "','" + attr[1] + "'";
                    }
                    else
                    {
                        sSQL_Value += ",'','',''";
                    }
                }

                for (int j = i; j < 20; j++)
                {
                    sSQL_Value += ",'','',''";
                }

                sSQL_Value += "),";
                //if (typeBrg == 1)
                //    sSQL_Value += ",";
                ret.status = 1;
                ret.message = sSQL_Value;
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }

        protected void getShopBrand(JDIDAPIDataJob data)
        {
            try
            {

                //var mgrApiManager = new JDIDControllerJob();
                //mgrApiManager.AppKey = data.appKey;
                //mgrApiManager.AppSecret = data.appSecret;
                //mgrApiManager.AccessToken = data.accessToken;
                //mgrApiManager.Method = "epi.popShop.getShopBrandList";

                string sMethod = "epi.popShop.getShopBrandList";
                string sParamJson = "";

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var retBrand = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (retBrand != null)
                {
                    if (retBrand.openapi_msg.ToLower() == "success")
                    {
                        var dataBrand = JsonConvert.DeserializeObject(retBrand.openapi_data, typeof(Data_BrandJob)) as Data_BrandJob;
                        if (dataBrand.success)
                        {
                            listBrand = dataBrand.model;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public string JD_printLabelJDID(JDIDAPIDataJob data, string noref)
        {
            SetupContext(data.DatabasePathErasoft, data.username);
            string ret = "";

            try
            {
                string sMethod = "epi.pop.order.print.uat1";
                string sParamJson = noref + ",1,1,\"PDF\""; //orderId, printType, packageNum, imageType //example: "1027577635,1,1,\"PDF\""

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var result = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (result != null)
                {
                    if (result.openapi_msg.ToLower() == "success")
                    {
                        var listPrintLabel = JsonConvert.DeserializeObject(result.openapi_data, typeof(Data_PrintLabel)) as Data_PrintLabel;
                        if (listPrintLabel.success)
                        {
                            var str = "{\"data\":" + listPrintLabel.model + "}";
                            foreach (var dataDetail in listPrintLabel.model.data)
                            {
                                try
                                {
                                    if (!string.IsNullOrEmpty(dataDetail.deliveryId))
                                    {
                                        var cekPesanan = ErasoftDbContext.SOT01A.Where(a => a.CUST == data.no_cust && a.NO_REFERENSI == noref).FirstOrDefault();
                                        if (cekPesanan != null)
                                        {
                                            cekPesanan.TRACKING_SHIPMENT = dataDetail.deliveryId;
                                            ErasoftDbContext.SaveChanges();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                                ret = dataDetail.PDF.ToString();
                            }
                            //var listDetails = JsonConvert.DeserializeObject(str, typeof(ModelOrderJob)) as ModelOrderJob;

                            //var test = result;
                        }
                        else
                        {
                            ret = "error. " + listPrintLabel.message;
                        }
                    }
                    else
                    {
                        ret = "error";
                    }
                }
            }
            catch (Exception ex)
            {
                ret = "error";
            }

            return ret;
        }

        public string JD_printLabelJDIDV2(JDIDAPIDataJob data, string noref)
        {
            SetupContext(data.DatabasePathErasoft, data.username);
            string ret = "";
            try
            {
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 3)
                {
                    data = RefreshToken(data);
                    var sysParams = new Dictionary<string, string>();
                    this.ParamJson = "{\"printType\":\"1\",\"printNum\":\"1\",\"orderId\":\"" + noref + "\"}";
                    sysParams.Add("360buy_param_json", this.ParamJson);

                    sysParams.Add("access_token", data.accessToken);
                    sysParams.Add("app_key", data.appKey);
                    this.Method = "jingdong.seller.order.printOrder";
                    sysParams.Add("method", this.Method);
                    var gettimestamp = getCurrentTimeFormatted();
                    sysParams.Add("timestamp", gettimestamp);
                    sysParams.Add("v", this.Version2);
                    sysParams.Add("format", this.Format);
                    sysParams.Add("sign_method", this.SignMethod);

                    var signature = this.generateSign(sysParams, data.appSecret);

                    string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + Uri.EscapeDataString(this.ParamJson) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                    urll += "&format=json&sign_method=md5";
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    responseFromServer = "";
                    try
                    {
                        using (WebResponse response = myReq.GetResponse())
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream);
                                responseFromServer = reader.ReadToEnd();
                                responseApi = true; break;
                            }
                        }
                    }
                    //catch (WebException ex)
                    //{
                    //    string err1 = "";
                    //    if (ex.Status == WebExceptionStatus.ProtocolError)
                    //    {
                    //        WebResponse resp1 = ex.Response;
                    //        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                    //        {
                    //            err1 = sr1.ReadToEnd();
                    //        }
                    //    }
                    //    //throw new Exception(err1);
                    //}
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                        {
                            retry = retry + 1;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            if (retry == 3)
                            {
                                ret = "error";
                            }
                        }
                        else
                        {
                            retry = 3;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            ret = "error";
                            responseApi = true; break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    var respons = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDLabelV2)) as JDIDLabelV2;
                    if (respons.jingdong_seller_order_printorder_response.result != null)
                    {
                        if (respons.jingdong_seller_order_printorder_response.result.success)
                        {
                            if (respons.jingdong_seller_order_printorder_response.result.model != null)
                            {
                                if (!string.IsNullOrEmpty(respons.jingdong_seller_order_printorder_response.result.model.content))
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(respons.jingdong_seller_order_printorder_response.result.model.expressNo))
                                        {
                                            var cekPesanan = ErasoftDbContext.SOT01A.Where(a => a.CUST == data.no_cust && a.NO_REFERENSI == noref).FirstOrDefault();
                                            if (cekPesanan != null)
                                            {
                                                cekPesanan.TRACKING_SHIPMENT = respons.jingdong_seller_order_printorder_response.result.model.expressNo;
                                                ErasoftDbContext.SaveChanges();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    ret = respons.jingdong_seller_order_printorder_response.result.model.content.ToString();
                                }
                            }
                            else
                            {
                                ret = "error. " + respons.jingdong_seller_order_printorder_response.result.message;
                            }
                        }
                        else
                        {
                            ret = "error. " + respons.jingdong_seller_order_printorder_response.result.message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ret = "error";
            }

            return ret;
        }

        public string JD_sendGoodJDID(JDIDAPIDataJob data, string noref, string noresi)
        {

            string ret = "";

            try
            {
                string sMethod = "epi.popOrder.sendGoods.uat";
                string sParamJson = "{\"orderId\":" + noref + ", \"expressNo\":\"" + noresi + "\"}";

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var result = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (result != null)
                {
                    if (result.openapi_msg.ToLower() == "success")
                    {
                        var listRTS = JsonConvert.DeserializeObject(result.openapi_data, typeof(Data_ReadyToShip)) as Data_ReadyToShip;
                        if (listRTS.success == true)
                        {
                            ret = listRTS.message.ToString();
                        }
                        else
                        {
                            ret = listRTS.message.ToString();
                        }
                    }
                    else
                    {
                        ret = "error";
                    }
                }
            }
            catch (Exception ex)
            {
                ret = "error";
            }

            return ret;
        }

        //add by nurul 4/5/2021, JDID versi 2
        public string JD_sendGoodJDIDV2(JDIDAPIDataJob data, string noref, string noresi)
        {

            string ret = "";
            try
            {
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 3)
                {
                    data = RefreshToken(data);
                    var sysParams = new Dictionary<string, string>();
                    this.ParamJson = "{\"orderId\":\"" + noref + "\",\"expressNo\":\"" + noresi + "\"}";
                    sysParams.Add("360buy_param_json", this.ParamJson);

                    sysParams.Add("access_token", data.accessToken);
                    sysParams.Add("app_key", data.appKey);
                    this.Method = "jingdong.seller.order.sendGoodsOpenApi";
                    sysParams.Add("method", this.Method);
                    var gettimestamp = getCurrentTimeFormatted();
                    sysParams.Add("timestamp", gettimestamp);
                    sysParams.Add("v", this.Version2);
                    sysParams.Add("format", this.Format);
                    sysParams.Add("sign_method", this.SignMethod);

                    var signature = this.generateSign(sysParams, data.appSecret);

                    string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + Uri.EscapeDataString(this.ParamJson) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                    urll += "&format=json&sign_method=md5";
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    responseFromServer = "";
                    try
                    {
                        using (WebResponse response = myReq.GetResponse())
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream);
                                responseFromServer = reader.ReadToEnd();
                                responseApi = true; break;
                            }
                        }
                    }
                    //catch (WebException ex)
                    //{
                    //    string err1 = "";
                    //    if (ex.Status == WebExceptionStatus.ProtocolError)
                    //    {
                    //        WebResponse resp1 = ex.Response;
                    //        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                    //        {
                    //            err1 = sr1.ReadToEnd();
                    //        }
                    //    }
                    //    //throw new Exception(err1);
                    //}
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                        {
                            retry = retry + 1;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            if (retry == 3)
                            {
                                ret = "error";
                            }
                        }
                        else
                        {
                            retry = 3;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            ret = "error";
                            responseApi = true; break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    var respons = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDRTSV2)) as JDIDRTSV2;
                    if (respons.jingdong_seller_order_sendgoodsopenapi_response.result != null)
                    {
                        if (respons.jingdong_seller_order_sendgoodsopenapi_response.result.success)
                        {
                            if (respons.jingdong_seller_order_sendgoodsopenapi_response.result.model != null)
                            {
                                //if (!string.IsNullOrEmpty(respons.data.result.model.expressCompany))
                                //{
                                //    ret = respons.data.result.model.expressCompany.ToString();
                                //}
                                ret = respons.jingdong_seller_order_sendgoodsopenapi_response.result.message.ToString();
                            }
                        }
                        else
                        {
                            ret = "error. " + respons.jingdong_seller_order_sendgoodsopenapi_response.result.message.ToString();
                        }
                    }
                    else
                    {
                        ret = "error.";
                    }
                }
            }
            catch (Exception ex)
            {
                ret = "error.";
            }

            return ret;
        }
        //end add by nurul 4/5/2021, JDID versi 2 

        public void UpdateStock(JDIDAPIDataJob data, string id, int stok)
        {
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Stock",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = id,
                REQUEST_ATTRIBUTE_2 = stok.ToString(),
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);
            //var mgrApiManager = new JDIDControllerJob();

            //mgrApiManager.AppKey = data.appKey;
            //mgrApiManager.AppSecret = data.appSecret;
            //mgrApiManager.AccessToken = data.accessToken;
            //mgrApiManager.Method = "epi.ware.openapi.warestock.updateWareStock";
            //mgrApiManager.ParamJson = "{\"jsonStr\":[{\"skuId\":" + id + ", \"realNum\": " + stok + "}]}";

            string sMethod = "epi.ware.openapi.warestock.updateWareStock";
            string sParamJson = "{\"jsonStr\":[{\"skuId\":" + id + ", \"realNum\": " + stok + "}]}";

            try
            {
                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (ret != null)
                {
                    if (ret.openapi_msg.ToLower() == "success")
                    {
                        var retStok = JsonConvert.DeserializeObject(ret.openapi_data, typeof(Data_UpStokJob)) as Data_UpStokJob;
                        if (retStok != null)
                        {
                            if (retStok.success)
                            {

                            }
                            else
                            {
                                currentLog.REQUEST_EXCEPTION = retStok.message;
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            }
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        }
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    }
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = response;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                }
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
            }

        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> JD_GetOrderByStatusPaid(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            SetupContext(iden.DatabasePathErasoft, iden.username);

            //var daysFrom = -1;
            //var daysTo = 0;
            //var daysNow = DateTime.UtcNow.AddHours(7);
            var daysNow = DateTime.UtcNow;
            //add by nurul 20/1/2021, bundling 
            var AdaKomponen = false;
            var connIdProses = "";
            List<string> tempConnId = new List<string>() { };
            //end add by nurul 20/1/2021, bundling 

            //while (daysFrom > -13)
            //while (daysFrom >= -3)
            {
                //var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).ToUnixTimeSeconds() * 1000;
                //var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).ToUnixTimeSeconds() * 1000;
                var dateFrom = (long)daysNow.AddDays(-1).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                var dateTo = (long)daysNow.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                //change by nurul 20/1/2021, bundling 
                //await JD_GetOrderByStatusPaidList3Days(iden, stat, CUST, NAMA_CUST, 1, 0, 0, dateFrom, dateTo);
                var returnGetOrder = await JD_GetOrderByStatusPaidList3Days(iden, stat, CUST, NAMA_CUST, 0, 0, 0, dateFrom, dateTo);
                //change by nurul 20/1/2021, bundling 

                //daysFrom -= 3;
                //daysTo -= 3;
                //daysFrom -= 1;
                //daysTo -= 1;

                //add by nurul 20/1/2021, bundling 
                //if (returnGetOrder != "")
                //{
                //    tempConnId.Add(returnGetOrder);
                //    connIdProses += "'" + returnGetOrder + "' , ";
                //}
                //if(returnGetOrder == "1")
                //{
                //    AdaKomponen = true;
                //}
                if (!string.IsNullOrEmpty(returnGetOrder))
                {
                    connIdProses += "'" + returnGetOrder + "' , ";
                }
                //end add by nurul 20/1/2021, bundling 
            }
            //add by nurul 20/1/2021, bundling 
            //List<string> listBrgKomponen = new List<string>();
            //if (tempConnId.Count() > 0)
            //{
            //    listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in (" + connIdProses.Substring(0, connIdProses.Length - 3) + ")").ToList();
            //}
            //if (listBrgKomponen.Count() > 0)
            //if(AdaKomponen)
            if (!string.IsNullOrEmpty(connIdProses))
            {
                new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username, connIdProses.Substring(0, connIdProses.Length - 3));
            }
            //end add by nurul 20/1/2021, bundling 

            // tunning untuk tidak duplicate
            var queryStatus = "";
            if (stat == StatusOrder.PAID)
            {
                queryStatus = "\"1\"" + "," + "\"\\\"" + CUST + "\\\"\"" + "," + "\"\\\"" + NAMA_CUST + "\\\"\"";  // "1","\"000011\"","\"Echoboomers\""
            }
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust + "%' and arguments like '%" + queryStatus + "%' and invocationdata like '%JD_GetOrderByStatusPaid%' and statename like '%Enque%' and invocationdata not like '%resi%' and invocationdata not like '%JD_GetOrderByStatusComplete%' and invocationdata not like '%JD_GetOrderByStatusCancel%' ");
            // end tunning untuk tidak duplicate

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> JD_GOLIVE_GetOrderByStatusPaid(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            SetupContext(iden.DatabasePathErasoft, iden.username);

            var daysFrom = -1;
            var daysTo = 0;
            //var daysNow = DateTime.UtcNow.AddHours(7);
            var daysNow = DateTime.UtcNow;
            //add by nurul 20/1/2021, bundling 
            var AdaKomponen = false;
            var connIdProses = "";
            List<string> tempConnId = new List<string>() { };
            //end add by nurul 20/1/2021, bundling 

            //while (daysFrom > -13)
            while (daysFrom >= -3)
            {
                var dateFrom = (long)daysNow.AddDays(daysFrom).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                var dateTo = (long)daysNow.AddDays(daysTo > 0 ? 0 : daysTo).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                var returnGetOrder = await JD_GetOrderByStatusPaidList3Days(iden, stat, CUST, NAMA_CUST, 0, 0, 0, dateFrom, dateTo);

                daysFrom -= 1;
                daysTo -= 1;

                if (!string.IsNullOrEmpty(returnGetOrder))
                {
                    connIdProses += "'" + returnGetOrder + "' , ";
                }
                //end add by nurul 20/1/2021, bundling 
            }

            if (!string.IsNullOrEmpty(connIdProses))
            {
                new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username, connIdProses.Substring(0, connIdProses.Length - 3));
            }

            return ret;
        }

        public async Task<string> JD_GetOrderByStatusPaidList3Days(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar, long daysFrom, long daysTo)
        {
            //1:Waiting for delivery, 2:Shipped, 3:Waiting_Cancel, 4:Waiting_Refuse, 5:Canceled, 6:Completed, 7:Ready to Ship

            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden.DatabasePathErasoft, iden.username);

            var listOrderId = new List<long>();
            var loop = true;

            //add by nurul 4/5/2021, JDID versi 2
            if (iden.versi == "2")
            {
                //listOrderId.AddRange(GetOrderListV2(iden, "1", 0, daysFrom, daysTo));
                while (loop)
                {
                    var retData = GetOrderListV2(iden, "1", page, daysFrom, daysTo);
                    if (retData.Count > 0)
                    {
                        if (retData.Count < 20)
                        {
                            loop = false;
                        }
                        listOrderId.AddRange(retData);
                    }
                    else
                    {
                        loop = false;
                    }
                    page++;
                    if (page > 100)
                    {
                        loop = false;
                    }
                }
            }
            else
            //end add by nurul 4/5/2021, JDID versi 2
            {
                //listOrderId.AddRange(GetOrderList(iden, "1", 0, daysFrom, daysTo));
                while (loop)
                {
                    var retData = GetOrderList(iden, "1", page, daysFrom, daysTo);
                    if (retData.Count > 0)
                    {
                        if (retData.Count < 20)
                        {
                            loop = false;
                        }
                        listOrderId.AddRange(retData);
                    }
                    else
                    {
                        loop = false;
                    }
                    page++;
                    if (page > 100)
                    {
                        loop = false;
                    }
                }
            }

            string connectionID = Guid.NewGuid().ToString();
            var connIdARF01C = Guid.NewGuid().ToString();

            //add by nurul 19/1/2021, bundling 
            ret = connectionID;
            //end add by nurul 19/1/2021, bundling

            if (listOrderId.Count > 0)
            {
                var ord = new List_Order_JD { orderIds = new List<string>() };
                string order_10 = "";
                int i = 1;
                foreach (var id in listOrderId)
                {
                    order_10 += id.ToString() + ",";
                    i++;
                    if (i >= 10)
                    {
                        i = 0;
                        order_10 = order_10.Substring(0, order_10.Length - 1);
                        ord.orderIds.Add(order_10);
                        order_10 = "";
                    }
                }
                if (!string.IsNullOrEmpty(order_10))
                {
                    order_10 = order_10.Substring(0, order_10.Length - 1);
                    ord.orderIds.Add(order_10);
                }

                //bool callSP = false;
                int newRecord = 0;
                foreach (var listOrder in ord.orderIds)
                {
                    //add by nurul 4/5/2021, JDID versi 2
                    var insertTemp = new BindingBase();
                    if (iden.versi == "2")
                    {
                        insertTemp = GetOrderDetailV2(iden, listOrder, iden.no_cust, connIdARF01C, connectionID);
                    }
                    else
                    //end add by nurul 4/5/2021, JDID versi 2
                    {
                        insertTemp = GetOrderDetail(iden, listOrder, iden.no_cust, connIdARF01C, connectionID);
                    }
                    if (insertTemp.status == 1)
                    {
                        //callSP = true;
                        if (insertTemp.recordCount > 0)
                            newRecord += insertTemp.recordCount;
                    }
                    //add by nurul 25/1/2021, bundling
                    //if (insertTemp.AdaKomponen)
                    //{
                    //    ret = "1";
                    //}
                    //end add by nurul 25/1/2021, bundling
                }

                //if (callSP)
                //{
                //    SqlCommand CommandSQL = new SqlCommand();

                //    //add by Tri call sp to insert buyer data
                //    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                //    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;

                //    EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                //    //end add by Tri call sp to insert buyer data

                //    CommandSQL = new SqlCommand();
                //    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                //    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                //    CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                //    CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //    CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 1;
                //    CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = iden.no_cust;

                //    EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);

                //    if (newRecord > 0)
                //    {
                //        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                //        contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(newRecord) + " Pesanan baru dari JD.ID.");

                //        new StokControllerJob().updateStockMarketPlace(connectionID, iden.DatabasePathErasoft, iden.username);
                //    }
                //}
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> JD_GetOrderByStatusRTS(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            SetupContext(iden.DatabasePathErasoft, iden.username);

            //var daysFrom = -1;
            //var daysTo = 0;
            //var daysNow = DateTime.UtcNow.AddHours(7);
            var daysNow = DateTime.UtcNow;
            //add by nurul 20/1/2021, bundling 
            var AdaKomponen = false;
            var connIdProses = "";
            List<string> tempConnId = new List<string>() { };
            //end add by nurul 20/1/2021, bundling 


            //while (daysFrom >= -3)
            {
                //var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).ToUnixTimeSeconds() * 1000;
                //var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).ToUnixTimeSeconds() * 1000;
                var dateFrom = (long)daysNow.AddDays(-1).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                var dateTo = (long)daysNow.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                //change by nurul 20/1/2021, bundling 
                //await JD_GetOrderByStatusRTSList3Days(iden, stat, CUST, NAMA_CUST, 1, 0, 0, dateFrom, dateTo);
                var returnGetOrder = await JD_GetOrderByStatusRTSList3Days(iden, stat, CUST, NAMA_CUST, 0, 0, 0, dateFrom, dateTo);
                //change by nurul 20/1/2021, bundling
                //daysFrom -= 3;
                //daysTo -= 3;
                //daysFrom -= 1;
                //daysTo -= 1;

                //add by nurul 20/1/2021, bundling 
                //if (returnGetOrder != "")
                //{
                //    tempConnId.Add(returnGetOrder);
                //    connIdProses += "'" + returnGetOrder + "' , ";
                //}
                //if (returnGetOrder == "1")
                //{
                //    AdaKomponen = true;
                //}
                if (!string.IsNullOrEmpty(returnGetOrder))
                {
                    connIdProses += "'" + returnGetOrder + "' , ";
                }
                //end add by nurul 20/1/2021, bundling 
            }
            //add by nurul 20/1/2021, bundling 
            //List<string> listBrgKomponen = new List<string>();
            //if (tempConnId.Count() > 0)
            //{
            //    listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in (" + connIdProses.Substring(0, connIdProses.Length - 3) + ")").ToList();
            //}
            //if (listBrgKomponen.Count() > 0)
            //if (AdaKomponen)
            if (!string.IsNullOrEmpty(connIdProses))
            {
                new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username, connIdProses.Substring(0, connIdProses.Length - 3));
            }
            //end add by nurul 20/1/2021, bundling 

            // tunning untuk tidak duplicate
            var queryStatus = "";
            if (stat == StatusOrder.READY_TO_SHIP)
            {
                queryStatus = "\"7\"" + "," + "\"\\\"" + CUST + "\\\"\"" + "," + "\"\\\"" + NAMA_CUST + "\\\"\"";  // "7","\"000011\"","\"Echoboomers\""
            }
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust + "%' and arguments like '%" + queryStatus + "%' and invocationdata like '%JD_GetOrderByStatusRTS%' and statename like '%Enque%' and invocationdata not like '%resi%' and invocationdata not like '%JD_GetOrderByStatusComplete%' and invocationdata not like '%JD_GetOrderByStatusCancel%' ");
            // end tunning untuk tidak duplicate

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> JD_GOLIVE_GetOrderByStatusRTS(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            SetupContext(iden.DatabasePathErasoft, iden.username);

            var daysFrom = -1;
            var daysTo = 0;
            //var daysNow = DateTime.UtcNow.AddHours(7);
            var daysNow = DateTime.UtcNow;
            //add by nurul 20/1/2021, bundling 
            var AdaKomponen = false;
            var connIdProses = "";
            List<string> tempConnId = new List<string>() { };
            //end add by nurul 20/1/2021, bundling 


            while (daysFrom >= -3)
            {
                var dateFrom = (long)daysNow.AddDays(daysFrom).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                var dateTo = (long)daysNow.AddDays(daysTo > 0 ? 0 : daysTo).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                var returnGetOrder = await JD_GetOrderByStatusRTSList3Days(iden, stat, CUST, NAMA_CUST, 0, 0, 0, dateFrom, dateTo);
                daysFrom -= 1;
                daysTo -= 1;

                if (!string.IsNullOrEmpty(returnGetOrder))
                {
                    connIdProses += "'" + returnGetOrder + "' , ";
                }
            }
            if (!string.IsNullOrEmpty(connIdProses))
            {
                new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username, connIdProses.Substring(0, connIdProses.Length - 3));
            }

            return ret;
        }
        public async Task<string> JD_GetOrderByStatusRTSList3Days(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar, long daysFrom, long daysTo)
        {
            //1:Waiting for delivery, 2:Shipped, 3:Waiting_Cancel, 4:Waiting_Refuse, 5:Canceled, 6:Completed, 7:Ready to Ship

            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden.DatabasePathErasoft, iden.username);

            var listOrderId = new List<long>();
            var loop = true;
            //add by nurul 4/5/2021, JDID versi 2
            if (iden.versi == "2")
            {
                //listOrderId.AddRange(GetOrderListV2(iden, "7", 0, daysFrom, daysTo));
                while (loop)
                {
                    var retData = GetOrderListV2(iden, "7", page, daysFrom, daysTo);
                    if (retData.Count > 0)
                    {
                        if (retData.Count < 20)
                        {
                            loop = false;
                        }
                        listOrderId.AddRange(retData);
                    }
                    else
                    {
                        loop = false;
                    }
                    page++;
                    if (page > 100)
                    {
                        loop = false;
                    }
                }
            }
            else
            //end add by nurul 4/5/2021, JDID versi 2
            {
                //listOrderId.AddRange(GetOrderList(iden, "7", 0, daysFrom, daysTo));
                while (loop)
                {
                    var retData = GetOrderList(iden, "7", page, daysFrom, daysTo);
                    if (retData.Count > 0)
                    {
                        if (retData.Count < 20)
                        {
                            loop = false;
                        }
                        listOrderId.AddRange(retData);
                    }
                    else
                    {
                        loop = false;
                    }
                    page++;
                    if (page > 100)
                    {
                        loop = false;
                    }
                }
            }

            string connectionID = Guid.NewGuid().ToString();
            var connIdARF01C = Guid.NewGuid().ToString();
            //add by nurul 19/1/2021, bundling 
            ret = connectionID;
            //end add by nurul 19/1/2021, bundling

            if (listOrderId.Count > 0)
            {
                var ord = new List_Order_JD { orderIds = new List<string>() };
                string order_10 = "";
                int i = 1;
                foreach (var id in listOrderId)
                {
                    order_10 += id.ToString() + ",";
                    i++;
                    if (i >= 10)
                    {
                        i = 0;
                        order_10 = order_10.Substring(0, order_10.Length - 1);
                        ord.orderIds.Add(order_10);
                        order_10 = "";
                    }
                }
                if (!string.IsNullOrEmpty(order_10))
                {
                    order_10 = order_10.Substring(0, order_10.Length - 1);
                    ord.orderIds.Add(order_10);
                }

                bool callSP = false;
                int newRecord = 0;
                foreach (var listOrder in ord.orderIds)
                {
                    //add by nurul 4/5/2021, JDID versi 2
                    var insertTemp = new BindingBase();
                    if (iden.versi == "2")
                    {
                        insertTemp = GetOrderDetailV2(iden, listOrder, iden.no_cust, connIdARF01C, connectionID);
                    }
                    else
                    //end add by nurul 4/5/2021, JDID versi 2
                    {
                        insertTemp = GetOrderDetail(iden, listOrder, iden.no_cust, connIdARF01C, connectionID);
                    }
                    if (insertTemp.status == 1)
                    {
                        //callSP = true;
                        if (insertTemp.recordCount > 0)
                            newRecord += insertTemp.recordCount;
                    }
                    //add by nurul 25/1/2021, bundling
                    //if (insertTemp.AdaKomponen)
                    //{
                    //    ret = "1";
                    //}
                    //end add by nurul 25/1/2021, bundling
                }

                //if (callSP)
                //{
                //    SqlCommand CommandSQL = new SqlCommand();

                //    //add by Tri call sp to insert buyer data
                //    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                //    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;

                //    EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                //    //end add by Tri call sp to insert buyer data

                //    CommandSQL = new SqlCommand();
                //    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                //    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                //    CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                //    CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //    CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 1;
                //    CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = iden.no_cust;

                //    EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);

                //    if (newRecord > 0)
                //    {
                //        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                //        contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(newRecord) + " Pesanan baru dari JD.ID.");

                //        //new StokControllerJob().updateStockMarketPlace(connectionID, iden.DatabasePathErasoft, iden.username);
                //    }
                //}
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> JD_GetOrderByStatusCancel(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            SetupContext(iden.DatabasePathErasoft, iden.username);

            var daysFrom = -1;
            var daysTo = 0;
            //var daysNow = DateTime.UtcNow.AddHours(7);
            var daysNow = DateTime.UtcNow;
            //add by nurul 20/1/2021, bundling 
            var AdaKomponen = false;
            var connIdProses = "";
            List<string> tempConnId = new List<string>() { };
            //end add by nurul 20/1/2021, bundling 

            while (daysFrom >= -10)
            {
                //var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).ToUnixTimeSeconds() * 1000;
                //var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).ToUnixTimeSeconds() * 1000;
                var dateFrom = (long)daysNow.AddDays(daysFrom).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                var dateTo = (long)daysNow.AddDays(daysTo > 0 ? 0 : daysTo).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                //change by nurul 20/1/2021, bundling 
                //await JD_GetOrderByStatusCancelList3Days(iden, stat, CUST, NAMA_CUST, 1, 0, 0, dateFrom, dateTo);
                var returnGetOrder = await JD_GetOrderByStatusCancelList3Days(iden, stat, CUST, NAMA_CUST, 0, 0, 0, dateFrom, dateTo);
                //change by nurul 20/1/2021, bundling 

                //daysFrom -= 3;
                //daysTo -= 3;
                daysFrom -= 1;
                daysTo -= 1;

                //add by nurul 20/1/2021, bundling 
                //if (returnGetOrder != "")
                //{
                //    tempConnId.Add(returnGetOrder);
                //    connIdProses += "'" + returnGetOrder + "' , ";
                //}
                //if (returnGetOrder == "1")
                //{
                //    AdaKomponen = true;
                //}
                if (!string.IsNullOrEmpty(returnGetOrder))
                {
                    connIdProses += "'" + returnGetOrder + "' , ";
                }
                //end add by nurul 20/1/2021, bundling 
            }
            //add by nurul 20/1/2021, bundling 
            //List<string> listBrgKomponen = new List<string>();
            //if (tempConnId.Count() > 0)
            //{
            //    listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in (" + connIdProses.Substring(0, connIdProses.Length - 3) + ")").ToList();
            //}
            //if (listBrgKomponen.Count() > 0)
            //if (AdaKomponen)
            if (!string.IsNullOrEmpty(connIdProses))
            {
                new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username, connIdProses.Substring(0, connIdProses.Length - 3));
            }
            //end add by nurul 20/1/2021, bundling 

            // tunning untuk tidak duplicate
            var queryStatus = "";
            if (stat == StatusOrder.CANCELLED)
            {
                queryStatus = "\"5\"" + "," + "\"\\\"" + CUST + "\\\"\"" + "," + "\"\\\"" + NAMA_CUST + "\\\"\"";  // "5","\"000011\"","\"Echoboomers\""
            }
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust + "%' and arguments like '%" + queryStatus + "%' and invocationdata like '%JD_GetOrderByStatusCancel%' and statename like '%Enque%' and invocationdata not like '%resi%' and invocationdata not like '%JD_GetOrderByStatusComplete%' ");
            // end tunning untuk tidak duplicate

            return ret;
        }

        public async Task<string> JD_GetOrderByStatusCancelList3Days(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar, long daysFrom, long daysTo)
        {
            //1:Waiting for delivery, 2:Shipped, 3:Waiting_Cancel, 4:Waiting_Refuse, 5:Canceled, 6:Completed, 7:Ready to Ship

            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden.DatabasePathErasoft, iden.username);

            var listOrderId = new List<long>();
            var loop = true;
            //add by nurul 4/5/2021, JDID versi 2
            if (iden.versi == "2")
            {
                //listOrderId.AddRange(GetOrderListV2(iden, "5", 0, daysFrom, daysTo));
                while (loop)
                {
                    var retData = GetOrderListV2(iden, "5", page, daysFrom, daysTo);
                    if (retData.Count > 0)
                    {
                        if (retData.Count < 20)
                        {
                            loop = false;
                        }
                        listOrderId.AddRange(retData);
                    }
                    else
                    {
                        loop = false;
                    }
                    page++;
                    if (page > 100)
                    {
                        loop = false;
                    }
                }
            }
            else
            //end add by nurul 4/5/2021, JDID versi 2
            {
                //listOrderId.AddRange(GetOrderList(iden, "5", 0, daysFrom, daysTo));
                while (loop)
                {
                    var retData = GetOrderList(iden, "5", page, daysFrom, daysTo);
                    if (retData.Count > 0)
                    {
                        if (retData.Count < 20)
                        {
                            loop = false;
                        }
                        listOrderId.AddRange(retData);
                    }
                    else
                    {
                        loop = false;
                    }
                    page++;
                    if (page > 100)
                    {
                        loop = false;
                    }
                }
            }

            string connectionID = Guid.NewGuid().ToString();
            var connIdARF01C = Guid.NewGuid().ToString();
            //add by nurul 19/1/2021, bundling 
            ret = connectionID;
            //end add by nurul 19/1/2021, bundling

            if (listOrderId.Count > 0)
            {
                var ord = new List_Order_JD { orderIds = new List<string>() };
                string order_10 = "";
                int i = 1;
                foreach (var id in listOrderId)
                {
                    order_10 += id.ToString() + ",";
                    i++;
                    if (i >= 10)
                    {
                        i = 0;
                        order_10 = order_10.Substring(0, order_10.Length - 1);
                        ord.orderIds.Add(order_10);
                        order_10 = "";
                    }
                }
                if (!string.IsNullOrEmpty(order_10))
                {
                    order_10 = order_10.Substring(0, order_10.Length - 1);
                    ord.orderIds.Add(order_10);
                }

                bool callSP = false;
                int cancelRecord = 0;
                foreach (var listOrder in ord.orderIds)
                {
                    //add by nurul 4/5/2021, JDID versi 2
                    var insertTemp = new BindingBase();
                    if (iden.versi == "2")
                    {
                        insertTemp = GetOrderDetailV2(iden, listOrder, iden.no_cust, connIdARF01C, connectionID);
                    }
                    else
                    //end add by nurul 4/5/2021, JDID versi 2
                    {
                        insertTemp = GetOrderDetail(iden, listOrder, iden.no_cust, connIdARF01C, connectionID);
                    }
                    if (insertTemp.status == 1)
                    {
                        callSP = true;
                        if (insertTemp.recordCount > 0)
                            cancelRecord += insertTemp.recordCount;
                    }
                    //add by nurul 25/1/2021, bundling
                    //if (insertTemp.AdaKomponen)
                    //{
                    //    ret = "1";
                    //}
                    //end add by nurul 25/1/2021, bundling
                }

                //if (cancelRecord > 0)
                //{
                //    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                //    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(cancelRecord) + " Pesanan dari JD.ID dibatalkan.");

                //    new StokControllerJob().updateStockMarketPlace(connectionID, iden.DatabasePathErasoft, iden.username);
                //}

                //if (callSP)
                //{
                //    SqlCommand CommandSQL = new SqlCommand();

                //    //add by Tri call sp to insert buyer data
                //    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                //    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;

                //    EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                //    //end add by Tri call sp to insert buyer data

                //    CommandSQL = new SqlCommand();
                //    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                //    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                //    CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                //    CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //    CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 1;
                //    CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = iden.no_cust;

                //    EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);

                //    if (newRecord > 0)
                //    {
                //        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                //        contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(newRecord) + " Pesanan baru dari JD.ID.");

                //        new StokControllerJob().updateStockMarketPlace(connectionID, iden.DatabasePathErasoft, iden.username);
                //    }
                //}
            }

            return ret;
        }

        //[AutomaticRetry(Attempts = 2)]
        //[Queue("3_general")]
        //public async Task<string> JD_GetOrderByStatusComplete(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        //{
        //    string ret = "";
        //    SetupContext(iden.DatabasePathErasoft, iden.username);

        //    var daysFrom = -2;
        //    var daysTo = 0;
        //    var daysNow = DateTime.UtcNow.AddHours(7);
        //    while (daysFrom >= -10)
        //    {
        //        //var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).ToUnixTimeSeconds() * 1000;
        //        //var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).ToUnixTimeSeconds() * 1000;
        //        var dateFrom = (long)daysNow.AddDays(daysFrom).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        //        var dateTo = (long)daysNow.AddDays(daysTo > 0 ? 0 : daysTo).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        //        await JD_GetOrderByStatusCompleteList3Days(iden, stat, CUST, NAMA_CUST, 1, 0, 0, dateFrom, dateTo);
        //        //daysFrom -= 3;
        //        //daysTo -= 3;
        //        daysFrom -= 2;
        //        daysTo -= 2;
        //    }

        //    // tunning untuk tidak duplicate
        //    var queryStatus = "";
        //    if (stat == StatusOrder.COMPLETED)
        //    {
        //        queryStatus = "\"6\"" + "," + "\"\\\"" + CUST + "\\\"\"" + "," + "\"\\\"" + NAMA_CUST + "\\\"\"";  // "6","\"000011\"","\"Echoboomers\""
        //    }
        //    var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust + "%' and arguments like '%" + queryStatus + "%' and invocationdata like '%JD_GetOrderByStatusComplete%' and statename like '%Enque%' and invocationdata not like '%resi%' and invocationdata not like '%JD_GetOrderByStatusCancel%' ");
        //    // end tunning untuk tidak duplicate

        //    return ret;
        //}

        //add by nurul 9/8/2021
        [AutomaticRetry(Attempts = 2)]
        [Queue("1_manage_pesanan")]
        public async Task<string> JD_GetOrderByStatusComplete(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            SetupContext(iden.DatabasePathErasoft, iden.username);

            string sSQL = "SELECT DISTINCT NO_REFERENSI FROM SOT01A (NOLOCK) A INNER JOIN SIT01A (NOLOCK) B ON  A.NO_BUKTI = B.NO_SO ";
            sSQL += "WHERE STATUS_TRANSAKSI = '03' AND A.TGL >= '" + DateTime.UtcNow.AddHours(7).AddDays(-10).ToString("yyyy-MM-dd HH:mm:ss") + "' AND A.CUST = '" + CUST + "' AND ISNULL(A.NO_REFERENSI,'')<>''";

            var dsPesanan = EDB.GetDataSet("CString", "SOT01", sSQL);

            if (dsPesanan.Tables[0].Rows.Count > 0)
            {
                var string_listNoRef = "";
                var listNoRef = new List<string>();
                for (int i = 0; i < dsPesanan.Tables[0].Rows.Count; i++)
                {
                    string_listNoRef += dsPesanan.Tables[0].Rows[i]["NO_REFERENSI"].ToString() + ",";
                    listNoRef.Add(dsPesanan.Tables[0].Rows[i]["NO_REFERENSI"].ToString());
                    if (listNoRef.Count == 100 || i == dsPesanan.Tables[0].Rows.Count - 1)
                    {
                        string_listNoRef = string_listNoRef.Substring(0, string_listNoRef.Length - 1);
                        if (iden.versi == "2")
                        {
                            await GetOrderByStatusCompletedAPIV2(iden, listNoRef.ToArray(), string_listNoRef, CUST);
                        }
                        else
                        {
                            GetOrderByStatusCompletedAPI(iden, listNoRef.ToArray(), string_listNoRef, CUST);
                        }
                        listNoRef = new List<string>();
                        string_listNoRef = "";
                    }
                }
            }

            // tunning untuk tidak duplicate
            var queryStatus = "";
            if (stat == StatusOrder.COMPLETED)
            {
                queryStatus = "\"6\"" + "," + "\"\\\"" + CUST + "\\\"\"" + "," + "\"\\\"" + NAMA_CUST + "\\\"\"";  // "6","\"000011\"","\"Echoboomers\""
            }
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust + "%' and arguments like '%" + queryStatus + "%' and invocationdata like '%JD_GetOrderByStatusComplete%' and statename like '%Enque%' and invocationdata not like '%resi%' and invocationdata not like '%JD_GetOrderByStatusCancel%' ");
            // end tunning untuk tidak duplicate

            return "";
        }

        public string GetOrderByStatusCompletedAPI(JDIDAPIDataJob data, string[] ordersn_list, string ordersn_string, string cust)
        {
            string sMethod = "epi.popOrder.getOrderInfoListForBatch";
            string sParamJson = "[" + ordersn_string + "]";

            try
            {
                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var retData = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (retData.openapi_code == 0)
                {
                    var listOrderId = JsonConvert.DeserializeObject(retData.openapi_data, typeof(Data_OrderDetailJob)) as Data_OrderDetailJob;
                    if (listOrderId.success)
                    {
                        var str = "{\"data\":" + listOrderId.model + "}";
                        var listDetails = JsonConvert.DeserializeObject(str, typeof(ModelOrderJob)) as ModelOrderJob;
                        if (listDetails != null)
                        {
                            string ordList = "";
                            foreach (var order in listDetails.data)
                            {
                                if (order.orderState.ToString() == "6")
                                {
                                    if (!string.IsNullOrEmpty(order.orderId.ToString()))
                                    {
                                        ordList += "'" + order.orderId + "',";
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(ordList))
                            {
                                ordList = ordList.Substring(0, ordList.Length - 1);
                                try
                                {
                                    var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI IN (" + ordList + ") AND CUST = '" + cust + "' AND STATUS_TRANSAKSI = '03'");

                                    if (rowAffected > 0)
                                    {
                                        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                        contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("" + Convert.ToString(rowAffected) + " Pesanan dari JD.ID sudah selesai.");

                                        //add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                        if (!string.IsNullOrEmpty(ordList))
                                        {
                                            var dateTimeNow = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                                            string sSQLUpdateDatePesananSelesai = "UPDATE SIT01A SET TGL_KIRIM = '" + dateTimeNow + "' WHERE NO_REF IN (" + ordList + ") AND CUST = '" + cust + "'";
                                            try
                                            {
                                                var resultUpdateDatePesanan = EDB.ExecuteSQL("CString", CommandType.Text, sSQLUpdateDatePesananSelesai);
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
                                        //end add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return "";
        }

        public async Task<string> GetOrderByStatusCompletedAPIV2(JDIDAPIDataJob data, string[] ordersn_list, string ordersn_string, string cust)
        {
            string responseFromServer = "";
            bool responseApi = false;
            int retry = 0;
            while (!responseApi && retry <= 3)
            {
                data = RefreshToken(data);
                var sysParams = new Dictionary<string, string>();
                this.ParamJson = "{\"orderId\":\"" + ordersn_string + "\" }";
                sysParams.Add("360buy_param_json", this.ParamJson);

                sysParams.Add("access_token", data.accessToken);
                sysParams.Add("app_key", data.appKey);
                this.Method = "jingdong.seller.order.batchGetOrderInfoList";
                sysParams.Add("method", this.Method);
                var gettimestamp = getCurrentTimeFormatted();
                sysParams.Add("timestamp", gettimestamp);
                sysParams.Add("v", this.Version2);
                sysParams.Add("format", this.Format);
                sysParams.Add("sign_method", this.SignMethod);

                var signature = this.generateSign(sysParams, data.appSecret);

                string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + Uri.EscapeDataString(this.ParamJson) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                urll += "&format=json&sign_method=md5";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                responseFromServer = "";
                try
                {
                    using (WebResponse response = await myReq.GetResponseAsync())
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(stream);
                            responseFromServer = reader.ReadToEnd();
                            responseApi = true; break;
                        }
                    }
                }
                //catch (WebException ex)
                //{
                //    string err1 = "";
                //    if (ex.Status == WebExceptionStatus.ProtocolError)
                //    {
                //        WebResponse resp1 = ex.Response;
                //        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                //        {
                //            err1 = sr1.ReadToEnd();
                //        }
                //    }
                //    //throw new Exception(err1);
                //}
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                    {
                        retry = retry + 1;
                        string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        if (retry == 3)
                        {

                        }
                    }
                    else
                    {
                        retry = 3;
                        string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        responseApi = true; break;
                    }
                }
            }


            if (responseFromServer != "")
            {
                try
                {
                    var listOrderId = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDGetOrderDetailV2Result)) as JDIDGetOrderDetailV2Result;
                    if (listOrderId.jingdong_seller_order_batchgetorderinfolist_response.result.success)
                    {
                        var listDetails = listOrderId.jingdong_seller_order_batchgetorderinfolist_response.result.model;
                        if (listDetails != null)
                        {
                            string ordList = "";
                            foreach (var order in listDetails)
                            {
                                if (order.orderState == 6)
                                {
                                    if (!string.IsNullOrEmpty(order.orderId.ToString()))
                                    {
                                        ordList += "'" + order.orderId + "',";
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(ordList))
                            {
                                ordList = ordList.Substring(0, ordList.Length - 1);
                                try
                                {
                                    var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI IN (" + ordList + ") AND CUST = '" + cust + "' AND STATUS_TRANSAKSI = '03'");

                                    if (rowAffected > 0)
                                    {
                                        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                        contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("" + Convert.ToString(rowAffected) + " Pesanan dari JD.ID sudah selesai.");

                                        //add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                        if (!string.IsNullOrEmpty(ordList))
                                        {
                                            var dateTimeNow = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                                            string sSQLUpdateDatePesananSelesai = "UPDATE SIT01A SET TGL_KIRIM = '" + dateTimeNow + "' WHERE NO_REF IN (" + ordList + ") AND CUST = '" + cust + "'";
                                            try
                                            {
                                                var resultUpdateDatePesanan = EDB.ExecuteSQL("CString", CommandType.Text, sSQLUpdateDatePesananSelesai);
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
                                        //end add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return "";
        }
        //end add by nurul 9/8/2021

        public async Task<string> JD_GetOrderByStatusCompleteList3Days(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar, long daysFrom, long daysTo)
        {
            //1:Waiting for delivery, 2:Shipped, 3:Waiting_Cancel, 4:Waiting_Refuse, 5:Canceled, 6:Completed, 7:Ready to Ship

            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden.DatabasePathErasoft, iden.username);

            var listOrderId = new List<long>();
            //add by nurul 4/5/2021, JDID versi 2
            if (iden.versi == "2")
            {
                listOrderId.AddRange(GetOrderListV2(iden, "6", 0, daysFrom, daysTo));
            }
            else
            //end add by nurul 4/5/2021, JDID versi 2
            {
                listOrderId.AddRange(GetOrderList(iden, "6", 0, daysFrom, daysTo));
            }

            string connectionID = Guid.NewGuid().ToString();
            var connIdARF01C = Guid.NewGuid().ToString();

            if (listOrderId.Count > 0)
            {
                var ord = new List_Order_JD { orderIds = new List<string>() };
                string order_10 = "";
                int i = 1;
                foreach (var id in listOrderId)
                {
                    order_10 += id.ToString() + ",";
                    i++;
                    if (i >= 10)
                    {
                        i = 0;
                        order_10 = order_10.Substring(0, order_10.Length - 1);
                        ord.orderIds.Add(order_10);
                        order_10 = "";
                    }
                }
                if (!string.IsNullOrEmpty(order_10))
                {
                    order_10 = order_10.Substring(0, order_10.Length - 1);
                    ord.orderIds.Add(order_10);
                }

                bool callSP = false;
                int newRecord = 0;
                foreach (var listOrder in ord.orderIds)
                {
                    //add by nurul 4/5/2021, JDID versi 2
                    var insertTemp = new BindingBase();
                    if (iden.versi == "2")
                    {
                        insertTemp = GetOrderDetailV2(iden, listOrder, iden.no_cust, connIdARF01C, connectionID);
                    }
                    else
                    //end add by nurul 4/5/2021, JDID versi 2
                    {
                        insertTemp = GetOrderDetail(iden, listOrder, iden.no_cust, connIdARF01C, connectionID);
                    }
                    if (insertTemp.status == 1)
                    {
                        callSP = true;
                        if (insertTemp.recordCount > 0)
                            newRecord += insertTemp.recordCount;
                    }
                }

            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public void Order_JD(JDIDAPIDataJob data, string uname)
        {
            //1:Waiting for delivery, 2:Shipped, 3:Waiting_Cancel, 4:Waiting_Refuse, 5:Canceled, 6:Completed, 7:Ready to Ship
            var listOrderId = new List<long>();
            SetupContext(data.DatabasePathErasoft, uname);
            //listOrderId.AddRange(GetOrderList(data, "1", 1, 1));
            //listOrderId.AddRange(GetOrderList(data, "2", 1, 1));
            //listOrderId.AddRange(GetOrderList(data, "3", 1, 1));
            //listOrderId.AddRange(GetOrderList(data, "4", 1, 1));
            //listOrderId.AddRange(GetOrderList(data, "5", 0, 1));
            //listOrderId.AddRange(GetOrderList(data, "6", 1, 1));
            string connectionID = Guid.NewGuid().ToString();
            var connIdARF01C = Guid.NewGuid().ToString();

            if (listOrderId.Count > 0)
            {
                var ord = new List_Order_JD { orderIds = new List<string>() };
                string order_10 = "";
                int i = 1;
                foreach (var id in listOrderId)
                {
                    order_10 += id.ToString() + ",";
                    i++;
                    if (i >= 10)
                    {
                        i = 0;
                        order_10 = order_10.Substring(0, order_10.Length - 1);
                        ord.orderIds.Add(order_10);
                        order_10 = "";
                    }
                }
                if (!string.IsNullOrEmpty(order_10))
                {
                    order_10 = order_10.Substring(0, order_10.Length - 1);
                    ord.orderIds.Add(order_10);
                }

                bool callSP = false;
                int newRecord = 0;
                foreach (var listOrder in ord.orderIds)
                {
                    //add by nurul 4/5/2021, JDID versi 2
                    var insertTemp = new BindingBase();
                    if (data.versi == "2")
                    {
                        insertTemp = GetOrderDetailV2(data, listOrder, data.no_cust, connIdARF01C, connectionID);
                    }
                    else
                    //end add by nurul 4/5/2021, JDID versi 2
                    {
                        insertTemp = GetOrderDetail(data, listOrder, data.no_cust, connIdARF01C, connectionID);
                    }
                    if (insertTemp.status == 1)
                    {
                        callSP = true;
                        if (insertTemp.recordCount > 0)
                            newRecord += insertTemp.recordCount;
                    }
                }

                if (callSP)
                {
                    SqlCommand CommandSQL = new SqlCommand();

                    //add by Tri call sp to insert buyer data
                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;

                    EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                    //end add by Tri call sp to insert buyer data

                    CommandSQL = new SqlCommand();
                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                    CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                    CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 1;
                    CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = data.no_cust;

                    EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);

                    if (newRecord > 0)
                    {
                        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(newRecord) + " Pesanan baru dari Lazada.");

                        //new StokControllerJob().updateStockMarketPlace(connectionID, data.DatabasePathErasoft, uname);
                    }
                }
            }
        }

        public List<long> GetOrderList(JDIDAPIDataJob data, string status, int page, long addDays, long addDays2)
        {
            var ret = new List<long>();
            //var mgrApiManager = new JDIDControllerJob();
            //mgrApiManager.AppKey = data.appKey;
            //mgrApiManager.AppSecret = data.appSecret;
            //mgrApiManager.AccessToken = data.accessToken;

            //mgrApiManager.Method = "epi.popOrder.getOrderIdListByCondition";
            //mgrApiManager.ParamJson = "{\"orderStatus\":" + status + ", \"startRow\": " + page * 20 + ", \"bookTimeBegin\": "
            //    + DateTimeOffset.Now.AddDays(-14).ToUnixTimeSeconds() + "}";

            string sMethod = "epi.popOrder.getOrderIdListByCondition";
            //string sParamJson = "{\"orderStatus\":" + status + ", \"startRow\": " + page * 20 + ", \"bookTimeBegin\": "
            //    + DateTimeOffset.Now.AddDays(addDays).AddHours(7).ToUnixTimeSeconds() + "000 }";
            string sParamJson = "{\"orderStatus\":" + status + ", \"startRow\": " + page * 20 + ", \"bookTimeBegin\": "
                + addDays + ", \"bookTimeEnd\": " + addDays2 + " }";
            //string sParamJson = "{\"orderStatus\":" + status + ", \"startRow\": " + page * 20 + "}";

            try
            {
                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var retData = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (retData.openapi_code == 0)
                {
                    var listOrderId = JsonConvert.DeserializeObject(retData.openapi_data, typeof(Data_OrderIdsJob)) as Data_OrderIdsJob;
                    if (listOrderId.success)
                    {
                        ret = listOrderId.model;
                        //if (listOrderId.model.Count == 20)
                        //{
                        //    var nextOrders = GetOrderList(data, status, page + 1, addDays, addDays2);
                        //    ret.AddRange(nextOrders);
                        //}
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return ret;
        }

        //add by nurul 4/3/2021
        public class listOrderNobuk
        {
            public string noref { get; set; }
            public string nobuk { get; set; }
        }
        //[AutomaticRetry(Attempts = 2)]
        //[Queue("1_manage_pesanan")]
        //public async Task<string> getKurirJDID(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, JDIDAPIDataJob data, string listOrderIds, List<listOrderNobuk> ListOrderNobuk)
        public string getKurirJDID(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, JDIDAPIDataJob data, string listOrderIds, List<listOrderNobuk> ListOrderNobuk)
        {
            string ret = "";
            SetupContext(data.DatabasePathErasoft, data.username);
            try
            {
                if (!string.IsNullOrEmpty(listOrderIds) && ListOrderNobuk.Count() > 0)
                {
                    string sMethod = "epi.popOrder.getOrderInfoListForBatch";
                    string sParamJson = "[" + listOrderIds + "]";
                    var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                    var retData = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                    if (retData.openapi_code == 0)
                    {
                        var listOrderId = JsonConvert.DeserializeObject(retData.openapi_data, typeof(Data_OrderDetailJob)) as Data_OrderDetailJob;
                        if (listOrderId.success)
                        {
                            var str = "{\"data\":" + listOrderId.model + "}";
                            var listDetails = JsonConvert.DeserializeObject(str, typeof(ModelOrderJob)) as ModelOrderJob;
                            if (listDetails != null)
                            {
                                if (listDetails.data.Count() > 0)
                                {
                                    foreach (var order in listDetails.data)
                                    {
                                        //long orderId = Convert.ToInt64(listOrderIds);
                                        //var cekDetailOrder = listDetails.data.Where(a => a.orderId == orderId).FirstOrDefault();
                                        //if (!string.IsNullOrEmpty(cekDetailOrder.carrierCompany))
                                        //{
                                        //    string sSQL = "UPDATE SOT01A SET SHIPMENT = '" + cekDetailOrder.carrierCompany + "' WHERE NO_BUKTI = '" + nobuk + "'";
                                        //    var resultUpdateKurirPesanan = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                                        //}
                                        if (!string.IsNullOrEmpty(order.carrierCompany))
                                        {
                                            var getNobuk = ListOrderNobuk.Where(a => a.noref == order.orderId.ToString()).FirstOrDefault();
                                            if (getNobuk != null)
                                            {
                                                if (!string.IsNullOrEmpty(getNobuk.nobuk))
                                                {
                                                    string sSQL = "UPDATE SOT01A SET SHIPMENT = '" + order.carrierCompany + "' WHERE NO_BUKTI = '" + getNobuk.nobuk + "'";
                                                    var resultUpdateKurirPesanan = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return ret;
        }
        //end add by nurul 4/3/2021
        //add by nurul 4/5/2021, JDID versi 2
        //[AutomaticRetry(Attempts = 2)]
        //[Queue("1_manage_pesanan")]
        //public async Task<string> getKurirJDIDV2(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, JDIDAPIDataJob data, string listOrderIds, List<listOrderNobuk> ListOrderNobuk)
        public string getKurirJDIDV2(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, JDIDAPIDataJob data, string listOrderIds, List<listOrderNobuk> ListOrderNobuk)
        {
            string ret = "";
            SetupContext(data.DatabasePathErasoft, data.username);
            try
            {
                if (!string.IsNullOrEmpty(listOrderIds) && ListOrderNobuk.Count() > 0)
                {
                    string responseFromServer = "";
                    bool responseApi = false;
                    int retry = 0;
                    while (!responseApi && retry <= 3)
                    {
                        data = RefreshToken(data);
                        var sysParams = new Dictionary<string, string>();
                        this.ParamJson = "{\"orderId\":\"" + listOrderIds + "\" }";
                        sysParams.Add("360buy_param_json", this.ParamJson);

                        sysParams.Add("access_token", data.accessToken);
                        sysParams.Add("app_key", data.appKey);
                        this.Method = "jingdong.seller.order.batchGetOrderInfoList";
                        sysParams.Add("method", this.Method);
                        var gettimestamp = getCurrentTimeFormatted();
                        sysParams.Add("timestamp", gettimestamp);
                        sysParams.Add("v", this.Version2);
                        sysParams.Add("format", this.Format);
                        sysParams.Add("sign_method", this.SignMethod);

                        var signature = this.generateSign(sysParams, data.appSecret);

                        string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + Uri.EscapeDataString(this.ParamJson) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                        urll += "&format=json&sign_method=md5";
                        HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                        myReq.Method = "GET";
                        responseFromServer = "";
                        try
                        {
                            using (WebResponse response = myReq.GetResponse())
                            {
                                using (Stream stream = response.GetResponseStream())
                                {
                                    StreamReader reader = new StreamReader(stream);
                                    responseFromServer = reader.ReadToEnd();
                                    responseApi = true; break;
                                }
                            }
                        }
                        //catch (WebException ex)
                        //{
                        //    string err1 = "";
                        //    if (ex.Status == WebExceptionStatus.ProtocolError)
                        //    {
                        //        WebResponse resp1 = ex.Response;
                        //        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                        //        {
                        //            err1 = sr1.ReadToEnd();
                        //        }
                        //    }
                        //    //throw new Exception(err1);
                        //}
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                            {
                                retry = retry + 1;
                                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                                if (retry == 3)
                                {

                                }
                            }
                            else
                            {
                                retry = 3;
                                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                                responseApi = true; break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(responseFromServer))
                    {
                        var respons = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDGetOrderDetailV2Result)) as JDIDGetOrderDetailV2Result;
                        if (respons.jingdong_seller_order_batchgetorderinfolist_response.result != null)
                        {
                            if (respons.jingdong_seller_order_batchgetorderinfolist_response.result.success)
                            {
                                if (respons.jingdong_seller_order_batchgetorderinfolist_response.result.model != null)
                                {
                                    var listDetails = respons.jingdong_seller_order_batchgetorderinfolist_response.result.model;
                                    if (listDetails != null)
                                    {
                                        if (listDetails.Count() > 0)
                                        {
                                            foreach (var order in listDetails)
                                            {
                                                //long orderId = Convert.ToInt64(listOrderIds);
                                                //var cekDetailOrder = listDetails.data.Where(a => a.orderId == orderId).FirstOrDefault();
                                                //if (!string.IsNullOrEmpty(cekDetailOrder.carrierCompany))
                                                //{
                                                //    string sSQL = "UPDATE SOT01A SET SHIPMENT = '" + cekDetailOrder.carrierCompany + "' WHERE NO_BUKTI = '" + nobuk + "'";
                                                //    var resultUpdateKurirPesanan = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                                                //}
                                                if (!string.IsNullOrEmpty(order.carrierCompany))
                                                {
                                                    var getNobuk = ListOrderNobuk.Where(a => a.noref == order.orderId.ToString()).FirstOrDefault();
                                                    if (getNobuk != null)
                                                    {
                                                        if (!string.IsNullOrEmpty(getNobuk.nobuk))
                                                        {
                                                            string sSQL = "UPDATE SOT01A SET SHIPMENT = '" + order.carrierCompany + "' WHERE NO_BUKTI = '" + getNobuk.nobuk + "'";
                                                            var resultUpdateKurirPesanan = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return ret;
        }
        //end add by nurul 4/5/2021, JDID versi 2

        public BindingBase GetOrderDetail(JDIDAPIDataJob data, string listOrderIds, string cust, string conn_id_arf01c, string conn_id_order)
        {
            //var ret = new List<long>();
            var ret = new BindingBase();
            ret.status = 0;
            bool adaInsert = false;
            //var mgrApiManager = new JDIDControllerJob();
            //mgrApiManager.AppKey = data.appKey;
            //mgrApiManager.AppSecret = data.appSecret;
            //mgrApiManager.AccessToken = data.accessToken;

            //mgrApiManager.Method = "epi.popOrder.getOrderInfoListForBatch";
            //mgrApiManager.ParamJson = "[" + listOrderIds + "]";

            string sMethod = "epi.popOrder.getOrderInfoListForBatch";
            string sParamJson = "[" + listOrderIds + "]";

            var jmlhNewOrder = 0;
            try
            {
                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var retData = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (retData.openapi_code == 0)
                {
                    var listOrderId = JsonConvert.DeserializeObject(retData.openapi_data, typeof(Data_OrderDetailJob)) as Data_OrderDetailJob;
                    if (listOrderId.success)
                    {
                        var str = "{\"data\":" + listOrderId.model + "}";
                        var listDetails = JsonConvert.DeserializeObject(str, typeof(ModelOrderJob)) as ModelOrderJob;
                        if (listDetails != null)
                        {

                            string insertQ = "INSERT INTO TEMP_ORDER_JD ([ADDRESS_CUSTOMER],[AREA],[BOOKTIME],[CITY],[COUPON_AMOUNT],[CUSTOMER_NAME],";
                            insertQ += "[DELIVERY_ADDR],[DELIVERY_TYPE],[EMAIL],[FREIGHT_AMOUNT],[FULL_CUT_AMMOUNT],[INSTALLMENT_FEE],[ORDER_COMPLETE_TIME],";
                            insertQ += "[ORDER_ID],[ORDER_SKU_NUM],[ORDER_STATE],[ORDER_TYPE],[PAY_SUBTOTAL],[PAYMENT_TYPE],[PHONE],[POSTCODE],[PROMOTION_AMOUNT],";
                            insertQ += "[SENDPAY],[STATE_CUSTOMER],[TOTAL_PRICE],[USER_PIN],[CUST],[USERNAME],[CONN_ID],[KET_CUSTOMER],[KODE_KURIR],[NAMA_KURIR],[NO_RESI],[NAMA_CUST]) VALUES ";

                            string insertOrderItems = "INSERT INTO TEMP_ORDERITEMS_JD ([ORDER_ID],[COMMISSION],[COST_PRICE],[COUPON_AMOUNT],[FULL_CUT_AMMOUNT]";
                            insertOrderItems += ",[HAS_PROMO],[JDPRICE],[PROMOTION_AMOUNT],[SKUID],[SKU_NAME],[SKU_NUMBER],[SPUID],[WEIGHT],[USERNAME],[CONN_ID],[BOOKTIME],[CUST],[NAMA_CUST]) VALUES ";

                            string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                            insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                            insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";

                            var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust).Select(p => p.NO_REFERENSI).ToList();
                            string idOrderCancel = ""; //untuk cancel
                            string idOrderComplete = ""; //untuk completed
                            string idOrderRTS = ""; //untuk readytoship
                            int jmlhOrderCancel = 0;
                            int jmlhOrderCompleted = 0;
                            int jmlhOrderReadytoShip = 0;
                            int jmlhOrderNew = 0;

                            foreach (var order in listDetails.data)
                            {
                                //1: waiting for delivery, 2: shipped, 3: Waiting_Cancel, 4: Waiting_Refuse, 5: canceled, 6: Completed, 7: Ready to ship
                                bool doInsert = true;
                                if (OrderNoInDb.Contains(Convert.ToString(order.orderId)) && order.orderState.ToString() == "1")
                                {
                                    doInsert = false;
                                }
                                else if (order.orderState.ToString() == "5") //CANCELED
                                {
                                    if (OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        doInsert = false;
                                        idOrderCancel = idOrderCancel + "'" + order.orderId + "',";
                                        jmlhOrderCancel++;
                                        //tidak ubah status menjadi selesai jika belum diisi faktur
                                        //remark by nurul 12/8/2021
                                        //var dsSIT01A = EDB.GetDataSet("CString", "SIT01A", "SELECT NO_REFERENSI, O.NO_BUKTI, O.STATUS_TRANSAKSI FROM SIT01A I INNER JOIN SOT01A O ON I.NO_SO = O.NO_BUKTI WHERE NO_REFERENSI = '" + order.orderId + "'");
                                        //if (dsSIT01A.Tables[0].Rows.Count == 0)
                                        //{
                                        //    doInsert = false;
                                        //}
                                        //end remark by nurul 12/8/2021
                                    }
                                    else
                                    {
                                        //tidak diinput jika order sudah selesai sebelum masuk MO
                                        doInsert = false;
                                    }
                                }
                                else if (order.orderState.ToString() == "6") // COMPLETED
                                {
                                    if (OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        idOrderComplete = idOrderComplete + "'" + order.orderId + "',";
                                    }
                                    doInsert = false;
                                }
                                else if (order.orderState.ToString() == "7") // READY TO SHIP
                                {
                                    if (OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        //jmlhOrderReadytoShip++;
                                        doInsert = false;
                                    }
                                    else
                                    {
                                        idOrderRTS = idOrderRTS + "'" + order.orderId + "',";
                                        doInsert = true;
                                    }
                                }
                                else if (order.orderState.ToString() == "3" || order.orderState.ToString() == "4")
                                {
                                    if (!OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        doInsert = false;
                                    }
                                    else
                                    {
                                        doInsert = false;
                                    }
                                }

                                if (doInsert)
                                {
                                    ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_ORDER_JD");
                                    ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_ORDERITEMS_JD");

                                    var dtNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    adaInsert = true;
                                    var statusEra = "";
                                    switch (order.orderState.ToString())
                                    {
                                        //1:Waiting for delivery, 2:Shipped, 3:Waiting_Cancel, 4:Waiting_Refuse, 5:Canceled, 6:Completed, 7:Ready to Ship
                                        case "1":
                                            statusEra = "01";
                                            break;
                                        case "2":
                                            statusEra = "03";
                                            break;
                                        case "3":
                                        case "4":
                                        case "5":
                                            statusEra = "11";
                                            break;
                                        case "6":
                                            statusEra = "04";
                                            break;
                                        case "7":
                                            statusEra = "01";
                                            break;
                                        default:
                                            statusEra = "99";
                                            break;
                                    }

                                    var nama = order.customerName != null ? order.customerName.Replace('\'', '`').Replace("'", "") : "";
                                    if (nama.Length > 30)
                                        nama = nama.Substring(0, 30);

                                    var vOrderAddress = order.address != null ? order.address.Replace('\'', '`').Replace("'", "") : "";
                                    var vDeliveryAddress = order.deliveryAddr != null ? order.deliveryAddr.Replace('\'', '`').Replace("'", "") : "";
                                    var vArea = order.area != null ? order.area.Replace('\'', '`').Replace("'", "") : "";
                                    var vCity = order.city != null ? order.city.Replace('\'', '`').Replace("'", "") : "";
                                    var vState = order.state != null ? order.state.Replace('\'', '`').Replace("'", "") : "";
                                    var messageCustomer = order.buyerMessage != null ? order.buyerMessage.Replace("'", "") : "";
                                    var vEmail = order.email != null ? order.email.Replace('\'', '`').Replace("'", "") : "";
                                    var vCodeKurir = order.carrierCode != null ? order.carrierCode.Replace('\'', '`').Replace("'", "") : "";
                                    var vNamaKurir = order.carrierCompany != null ? order.carrierCompany.Replace('\'', '`').Replace("'", "") : "";
                                    var vNoResi = order.expressNo != null ? order.expressNo.Replace('\'', '`').Replace("'", "") : "";

                                    #region cut char
                                    if (data.nama_cust.Length > 30)
                                        data.nama_cust = data.nama_cust.Substring(0, 30);
                                    if (vCodeKurir.Length > 20)
                                    {
                                        vCodeKurir = vCodeKurir.Substring(0, 20);
                                    }
                                    if (messageCustomer.Length > 250)
                                    {
                                        messageCustomer = messageCustomer.Substring(0, 250);
                                    }
                                    if (vArea.Length > 50)
                                    {
                                        vArea = vArea.Substring(0, 50);
                                    }
                                    if (vCity.Length > 50)
                                    {
                                        vCity = vCity.Substring(0, 50);
                                    }
                                    if (vState.Length > 50)
                                    {
                                        vState = vState.Substring(0, 50);
                                    }
                                    if (vEmail.Length > 50)
                                    {
                                        vEmail = vEmail.Substring(0, 50);
                                    }
                                    if (vNamaKurir.Length > 50)
                                    {
                                        vNamaKurir = vNamaKurir.Substring(0, 50);
                                    }
                                    if (vNoResi.Length > 50)
                                    {
                                        vNoResi = vNoResi.Substring(0, 50);
                                    }
                                    string orderId = !string.IsNullOrEmpty(order.orderId.ToString()) ? order.orderId.ToString().Replace("'", "`") : "";
                                    if (orderId.Length > 50)
                                    {
                                        orderId = orderId.Substring(0, 50);
                                    }
                                    string TLP = !string.IsNullOrEmpty(order.phone) ? order.phone.Replace('\'', '`') : "";
                                    if (TLP.Length > 30)
                                        TLP = TLP.Substring(0, 30);
                                    string KODEPOS = !string.IsNullOrEmpty(order.postCode) ? order.postCode.Replace('\'', '`') : "";
                                    if (KODEPOS.Length > 7)
                                    {
                                        KODEPOS = KODEPOS.Substring(0, 7);
                                    }
                                    string userPin = !string.IsNullOrEmpty(order.userPin) ? order.userPin.Replace("'", "`") : "";
                                    if (userPin.Length > 50)
                                    {
                                        userPin = userPin.Substring(0, 50);
                                    }
                                    #endregion

                                    if (!string.IsNullOrEmpty(vNamaKurir))
                                    {
                                        if (vNamaKurir.Contains("Gosend"))
                                        {
                                            vNamaKurir = "Go-Send";
                                        }
                                    }

                                    //insertQ += "('" + order.address.Replace('\'', '`') + "','" + order.area.Replace('\'', '`') + "','" + DateTimeOffset.FromUnixTimeSeconds(order.bookTime / 1000).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss") + "','" + order.city.Replace('\'', '`') + "'," + order.couponAmount + ",'" + order.customerName + "','";
                                    //var insertQValue = "('" + vOrderAddress + "','" + vArea + "','" + DateTimeOffset.FromUnixTimeSeconds(order.bookTime / 1000).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss") + "','" + vCity + "'," + order.couponAmount + ",'" + nama + "','";
                                    var insertQValue = "('" + vOrderAddress + "','" + vArea + "','" + DateTimeOffset.FromUnixTimeSeconds(order.bookTime / 1000).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "','" + vCity + "'," + order.couponAmount + ",'" + nama + "','";
                                    insertQValue += vDeliveryAddress + "'," + order.deliveryType + ",'" + vEmail + "'," + order.freightAmount + "," + order.fullCutAmount + "," + order.installmentFee + ",'" + DateTimeOffset.FromUnixTimeSeconds(order.orderCompleteTime / 1000).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "','";
                                    insertQValue += orderId + "'," + order.orderSkuNum + "," + statusEra + "," + order.orderType + "," + order.paySubtotal + "," + order.paymentType + ",'" + TLP + "','" + KODEPOS + "'," + order.promotionAmount + ",'";
                                    //insertQValue += order.sendPay + "','" + vState + "'," + order.totalPrice + ",'" + order.userPin + "','" + data.no_cust + "','" + username + "','" + conn_id_order + "', '" + messageCustomer + "', '" + order.carrierCode + "', '" + order.carrierCompany + "', '" + order.expressNo + "', '" + data.nama_cust + "') ,";
                                    insertQValue += order.sendPay + "','" + vState + "'," + order.totalPrice + ",'" + userPin + "','" + data.no_cust + "','" + username + "','" + conn_id_order + "', '" + messageCustomer + "', '" + vCodeKurir + "', '" + vNamaKurir + "', '" + vNoResi + "', '" + data.nama_cust + "') ,";

                                    var insertOrderItemsValue = "";

                                    if (order.orderSkuinfos != null)
                                    {
                                        foreach (var ordItem in order.orderSkuinfos)
                                        {
                                            #region cut char
                                            string skuId = !string.IsNullOrEmpty(ordItem.skuId.ToString()) ? ordItem.skuId.ToString().Replace("'", "`") : "";
                                            if (skuId.Length > 50)
                                            {
                                                skuId = skuId.Substring(0, 50);
                                            }
                                            string spuId = !string.IsNullOrEmpty(ordItem.spuId.ToString()) ? ordItem.spuId.ToString().Replace("'", "`") : "";
                                            if (spuId.Length > 50)
                                            {
                                                spuId = spuId.Substring(0, 50);
                                            }
                                            string skuName = !string.IsNullOrEmpty(ordItem.skuName) ? ordItem.skuName.Replace("'", "`") : "";
                                            if (skuName.Length > 150)
                                            {
                                                skuName = skuName.Substring(0, 150);
                                            }

                                            #endregion
                                            insertOrderItemsValue += "('" + orderId + "'," + ordItem.commission + "," + ordItem.costPrice + "," + ordItem.couponAmount + "," + ordItem.fullCutAmount + ",";
                                            insertOrderItemsValue += ordItem.hasPromo + "," + ordItem.jdPrice + "," + ordItem.promotionAmount + ",'" + spuId + ";" + skuId + "','" + skuName + "',";
                                            insertOrderItemsValue += ordItem.skuNumber + ",'" + ordItem.spuId + "'," + ordItem.weight + ",'" + username + "','" + conn_id_order + "','" + DateTimeOffset.FromUnixTimeSeconds(order.bookTime / 1000).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "','" + data.no_cust + "','" + data.nama_cust + "') ,";
                                        }
                                    }

                                    var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + vCity + "%'");
                                    var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + vState + "%'");

                                    var kabKot = "3174";//set default value jika tidak ada di db
                                    var prov = "31";//set default value jika tidak ada di db

                                    if (tblProv.Tables[0].Rows.Count > 0)
                                        prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
                                    if (tblKabKot.Tables[0].Rows.Count > 0)
                                        kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();


                                    var vAddress = order.address != null ? order.address.Replace('\'', '`').Replace("'", "") : "";

                                    //var vPostCode = order.postCode != null ? order.postCode.Replace('\'', '`') : "";

                                    //insertPembeli += "('" + order.customerName.Replace('\'', '`') + "','" + order.address.Replace('\'', '`') + "','" + order.phone + "','" + order.email.Replace('\'', '`') + "',0,0,'0','01',";
                                    var insertPembeliValue = "('" + nama + "','" + vAddress + "','" + TLP + "','" + nama + "',0,0,'0','01',";
                                    insertPembeliValue += "1, 'IDR', '01', '" + vAddress + "', 0, 0, 0, 0, '1', 0, 0, ";
                                    insertPembeliValue += "'FP', '" + dtNow + "', '" + username + "', '" + KODEPOS + "', '" + vEmail + "', '" + kabKot + "', '" + prov + "', '" + vCity + "', '" + vState + "', '" + conn_id_arf01c + "') ,";

                                    if (!OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        if (string.IsNullOrEmpty(idOrderRTS))
                                        {
                                            jmlhNewOrder++;
                                        }
                                        insertQValue = insertQValue.Substring(0, insertQValue.Length - 2);
                                        EDB.ExecuteSQL(username, CommandType.Text, insertQ + insertQValue);


                                        insertOrderItemsValue = insertOrderItemsValue.Substring(0, insertOrderItemsValue.Length - 2);
                                        EDB.ExecuteSQL(username, CommandType.Text, insertOrderItems + insertOrderItemsValue);


                                        insertPembeliValue = insertPembeliValue.Substring(0, insertPembeliValue.Length - 2);
                                        EDB.ExecuteSQL(username, CommandType.Text, insertPembeli + insertPembeliValue);

                                        using (SqlCommand CommandSQL = new SqlCommand())
                                        {
                                            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = conn_id_arf01c;

                                            EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                                        }

                                        using (SqlCommand CommandSQL = new SqlCommand())
                                        {
                                            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = conn_id_order;
                                            CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                                            CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                            CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 1;
                                            CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = data.no_cust;

                                            EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);
                                        }
                                    }
                                }
                            }

                            if (adaInsert)
                            {
                                ret.status = 1;

                                if (jmlhNewOrder > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari JD.ID.");

                                    //add by nurul 25/1/2021, bundling
                                    //var listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in ('" + conn_id_order + "')").ToList();
                                    //if (listBrgKomponen.Count() > 0)
                                    //{
                                    //    ret.AdaKomponen = true;
                                    //}
                                    //end add by nurul 25/1/2021, bundling
                                    new StokControllerJob().updateStockMarketPlace(conn_id_order, data.DatabasePathErasoft, data.username);
                                }

                            }

                            if (!string.IsNullOrEmpty(idOrderCancel))
                            {
                                idOrderCancel = idOrderCancel.Substring(0, idOrderCancel.Length - 1);
                                var brgAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + conn_id_order + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + idOrderCancel + ") AND STATUS_TRANSAKSI <> '11' AND BRG <> 'NOT_FOUND' AND CUST = '" + cust + "'");
                                //change by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                                //var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2', STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN (" + idOrderCancel + ") AND STATUS_TRANSAKSI <> '11' AND CUST = '" + cust + "'");
                                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2',ORDER_CANCEL_DATE = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', STATUS_TRANSAKSI = '11', STATUS_KIRIM='5' WHERE NO_REFERENSI IN (" + idOrderCancel + ") AND STATUS_TRANSAKSI <> '11' AND CUST = '" + cust + "'");
                                //END change by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                                if (rowAffected > 0)
                                {
                                    //add by Tri 1 sep 2020, hapus packing list
                                    //remark by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                                    //var delPL = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03B WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN (" + idOrderCancel + ")  AND STATUS_TRANSAKSI = '11' AND CUST = '" + cust + "')");
                                    //var delPLDetail = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03C WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN (" + idOrderCancel + ")  AND STATUS_TRANSAKSI = '11' AND CUST = '" + cust + "')");
                                    //END remark by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                                    //end add by Tri 1 sep 2020, hapus packing list
                                    var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN (" + idOrderCancel + ") AND STATUS <> '2' AND ST_POSTING = 'T' AND CUST = '" + cust + "'");
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderCancel) + " Pesanan dari JD.ID dibatalkan.");

                                    //add by nurul 25/1/2021, bundling
                                    //var listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in ('" + conn_id_order + "')").ToList();
                                    //if (listBrgKomponen.Count() > 0)
                                    //{
                                    //    ret.AdaKomponen = true;
                                    //}
                                    //end add by nurul 25/1/2021, bundling

                                    //add by nurul 14/4/2021, stok bundling
                                    var sSQLInsertTempBundling = "INSERT INTO TEMP_ALL_MP_ORDER_ITEM_BUNDLING ([BRG],[CONN_ID],[TGL]) " +
                                                                 "SELECT DISTINCT C.UNIT AS BRG, '" + conn_id_order + "' AS CONN_ID, DATEADD(HOUR, +7, GETUTCDATE()) AS TGL " +
                                                                 "FROM TEMP_ALL_MP_ORDER_ITEM A(NOLOCK) " +
                                                                 "LEFT JOIN TEMP_ALL_MP_ORDER_ITEM_BUNDLING B(NOLOCK) ON B.CONN_ID = '" + conn_id_order + "' AND A.BRG = B.BRG " +
                                                                 "INNER JOIN STF03 C(NOLOCK) ON A.BRG = C.BRG " +
                                                                 "WHERE ISNULL(A.CONN_ID,'') = '" + conn_id_order + "' " +
                                                                 "AND ISNULL(B.BRG,'') = '' AND A.BRG <> 'NOT_FOUND'";
                                    var execInsertTempBundling = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, sSQLInsertTempBundling);
                                    //end add by nurul 14/4/2021, stok bundling

                                    new StokControllerJob().updateStockMarketPlace(conn_id_order, data.DatabasePathErasoft, data.username);
                                }
                            }

                            if (!string.IsNullOrEmpty(idOrderComplete))
                            {
                                idOrderComplete = idOrderComplete.Substring(0, idOrderComplete.Length - 1);
                                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI IN (" + idOrderComplete + ") AND STATUS_TRANSAKSI = '03'");
                                jmlhOrderCompleted = jmlhOrderCompleted + rowAffected;
                                if (jmlhOrderCompleted > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderCompleted) + " Pesanan dari JD.ID sudah selesai.");

                                    //add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                    if (!string.IsNullOrEmpty(idOrderComplete))
                                    {
                                        var dateTimeNow = Convert.ToDateTime(DateTime.Now.AddHours(7).ToString("yyyy-MM-dd"));
                                        string sSQLUpdateDatePesananSelesai = "UPDATE SIT01A SET TGL_KIRIM = '" + dateTimeNow + "' WHERE NO_REF IN (" + idOrderComplete + ")";
                                        var resultUpdateDatePesanan = EDB.ExecuteSQL("CString", CommandType.Text, sSQLUpdateDatePesananSelesai);
                                    }
                                    //end add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                }
                            }

                            if (!string.IsNullOrEmpty(idOrderRTS))
                            {
                                idOrderRTS = idOrderRTS.Substring(0, idOrderRTS.Length - 1);
                                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI IN (" + idOrderRTS + ") AND STATUS_TRANSAKSI = '0'");
                                jmlhOrderReadytoShip = jmlhOrderReadytoShip + rowAffected;
                                if (jmlhOrderReadytoShip > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderReadytoShip) + " Pesanan dari JD.ID Ready To Ship.");
                                }

                                //add by nurul 25/1/2021, bundling
                                //var listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in ('" + conn_id_order + "')").ToList();
                                //if (listBrgKomponen.Count() > 0)
                                //{
                                //    ret.AdaKomponen = true;
                                //}
                                //end add by nurul 25/1/2021, bundling
                                new StokControllerJob().updateStockMarketPlace(conn_id_order, data.DatabasePathErasoft, data.username);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            //return adaInsert;
            ret.recordCount = jmlhNewOrder;
            return ret;
        }

        //add by nurul 4/5/2021, JDID versi 2
        public List<long> GetOrderListV2(JDIDAPIDataJob data, string status, int page, long addDays, long addDays2)
        {
            var ret = new List<long>();

            string responseFromServer = "";
            bool responseApi = false;
            int retry = 0;
            while (!responseApi && retry <= 3)
            {
                data = RefreshToken(data);
                //string sMethod = "epi.popOrder.getOrderIdListByCondition";
                //string sParamJson = "{\"orderStatus\":" + status + ", \"startRow\": " + page * 20 + ", \"createdTimeBegin\": "
                //    + addDays + ", \"createdTimeEnd\": " + addDays2 + " }";


                var sysParams = new Dictionary<string, string>();
                var dayFrom = DateTimeOffset.FromUnixTimeMilliseconds(addDays).UtcDateTime;
                var dayTo = DateTimeOffset.FromUnixTimeMilliseconds(addDays2).UtcDateTime;
                var dayFrom1 = dayFrom.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                var dayTo1 = dayTo.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

                this.ParamJson = "{\"orderStatus\":\"" + status + "\", \"startRow\": \"" + page * 20 + "\", \"bookTimeBegin\": \""
                    + dayFrom1 + "\", \"bookTimeEnd\": \"" + dayTo1 + "\" }";
                sysParams.Add("360buy_param_json", this.ParamJson);

                sysParams.Add("access_token", data.accessToken);
                sysParams.Add("app_key", data.appKey);
                this.Method = "jingdong.seller.order.getOrderIdListByCondition";
                sysParams.Add("method", this.Method);
                var gettimestamp = getCurrentTimeFormatted();
                sysParams.Add("timestamp", gettimestamp);
                sysParams.Add("v", this.Version2);
                sysParams.Add("format", this.Format);
                sysParams.Add("sign_method", this.SignMethod);

                var signature = this.generateSign(sysParams, data.appSecret);

                string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + Uri.EscapeDataString(this.ParamJson) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                urll += "&format=json&sign_method=md5";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                responseFromServer = "";
                try
                {
                    using (WebResponse response = myReq.GetResponse())
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(stream);
                            responseFromServer = reader.ReadToEnd();
                            responseApi = true; break;
                        }
                    }
                }
                //catch (WebException ex)
                //{
                //    string err1 = "";
                //    if (ex.Status == WebExceptionStatus.ProtocolError)
                //    {
                //        WebResponse resp1 = ex.Response;
                //        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                //        {
                //            err1 = sr1.ReadToEnd();
                //        }
                //    }
                //    //throw new Exception(err1);
                //}
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                    {
                        retry = retry + 1;
                        string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        if (retry == 3)
                        {
                        }
                    }
                    else
                    {
                        retry = 3;
                        string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        responseApi = true; break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                try
                {
                    var listOrderId = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDGetOrderListV2Result)) as JDIDGetOrderListV2Result;
                    if (listOrderId.jingdong_seller_order_getOrderIdListByCondition_response.result != null)
                    {
                        if (listOrderId.jingdong_seller_order_getOrderIdListByCondition_response.result.success)
                        {
                            if (listOrderId.jingdong_seller_order_getOrderIdListByCondition_response.result.model != null)
                            {
                                ret = listOrderId.jingdong_seller_order_getOrderIdListByCondition_response.result.model;
                                //if (listOrderId.jingdong_seller_order_getOrderIdListByCondition_response.result.model.Count() == 20)
                                //{
                                //var nextOrders = GetOrderListV2(data, status, page + 1, addDays, addDays2);
                                //ret.AddRange(nextOrders);
                                //}
                            }
                        }
                    }
                }
                catch (Exception ex2)
                {
                    string msg = ex2.InnerException != null ? ex2.InnerException.Message : ex2.Message;
                }
            }

            return ret;
        }

        public BindingBase GetOrderDetailV2(JDIDAPIDataJob data, string listOrderIds, string cust, string conn_id_arf01c, string conn_id_order)
        {
            //var ret = new List<long>();
            var ret = new BindingBase();
            ret.status = 0;
            bool adaInsert = false;

            //string sMethod = "epi.popOrder.getOrderInfoListForBatch";
            //string sParamJson = "[" + listOrderIds + "]";

            string responseFromServer = "";
            bool responseApi = false;
            int retry = 0;
            while (!responseApi && retry <= 3)
            {
                data = RefreshToken(data);
                var sysParams = new Dictionary<string, string>();
                this.ParamJson = "{\"orderId\":\"" + listOrderIds + "\" }";
                sysParams.Add("360buy_param_json", this.ParamJson);

                sysParams.Add("access_token", data.accessToken);
                sysParams.Add("app_key", data.appKey);
                this.Method = "jingdong.seller.order.batchGetOrderInfoList";
                sysParams.Add("method", this.Method);
                var gettimestamp = getCurrentTimeFormatted();
                sysParams.Add("timestamp", gettimestamp);
                sysParams.Add("v", this.Version2);
                sysParams.Add("format", this.Format);
                sysParams.Add("sign_method", this.SignMethod);

                var signature = this.generateSign(sysParams, data.appSecret);

                string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + Uri.EscapeDataString(this.ParamJson) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                urll += "&format=json&sign_method=md5";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                responseFromServer = "";
                try
                {
                    using (WebResponse response = myReq.GetResponse())
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(stream);
                            responseFromServer = reader.ReadToEnd();
                            responseApi = true; break;
                        }
                    }
                }
                //catch (WebException ex)
                //{
                //    string err1 = "";
                //    if (ex.Status == WebExceptionStatus.ProtocolError)
                //    {
                //        WebResponse resp1 = ex.Response;
                //        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                //        {
                //            err1 = sr1.ReadToEnd();
                //        }
                //    }
                //    //throw new Exception(err1);
                //}
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                    {
                        retry = retry + 1;
                        string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        if (retry == 3)
                        {

                        }
                    }
                    else
                    {
                        retry = 3;
                        string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        responseApi = true; break;
                    }
                }
            }


            var jmlhNewOrder = 0;
            if (!string.IsNullOrEmpty(responseFromServer))
            {
                try
                {
                    var listOrderId = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDGetOrderDetailV2Result)) as JDIDGetOrderDetailV2Result;
                    if (listOrderId.jingdong_seller_order_batchgetorderinfolist_response.result.success)
                    {
                        var listDetails = listOrderId.jingdong_seller_order_batchgetorderinfolist_response.result.model;
                        if (listDetails != null)
                        {
                            string insertQ = "INSERT INTO TEMP_ORDER_JD ([ADDRESS_CUSTOMER],[AREA],[BOOKTIME],[CITY],[COUPON_AMOUNT],[CUSTOMER_NAME],";
                            insertQ += "[DELIVERY_ADDR],[DELIVERY_TYPE],[EMAIL],[FREIGHT_AMOUNT],[FULL_CUT_AMMOUNT],[INSTALLMENT_FEE],[ORDER_COMPLETE_TIME],";
                            insertQ += "[ORDER_ID],[ORDER_SKU_NUM],[ORDER_STATE],[ORDER_TYPE],[PAY_SUBTOTAL],[PAYMENT_TYPE],[PHONE],[POSTCODE],[PROMOTION_AMOUNT],";
                            insertQ += "[SENDPAY],[STATE_CUSTOMER],[TOTAL_PRICE],[USER_PIN],[CUST],[USERNAME],[CONN_ID],[KET_CUSTOMER],[KODE_KURIR],[NAMA_KURIR],[NO_RESI],[NAMA_CUST]) VALUES ";

                            string insertOrderItems = "INSERT INTO TEMP_ORDERITEMS_JD ([ORDER_ID],[COMMISSION],[COST_PRICE],[COUPON_AMOUNT],[FULL_CUT_AMMOUNT]";
                            insertOrderItems += ",[HAS_PROMO],[JDPRICE],[PROMOTION_AMOUNT],[SKUID],[SKU_NAME],[SKU_NUMBER],[SPUID],[WEIGHT],[USERNAME],[CONN_ID],[BOOKTIME],[CUST],[NAMA_CUST]) VALUES ";

                            string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                            insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                            insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";

                            var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust).Select(p => p.NO_REFERENSI).ToList();
                            string idOrderCancel = ""; //untuk cancel
                            string idOrderComplete = ""; //untuk completed
                            string idOrderRTS = ""; //untuk readytoship
                            int jmlhOrderCancel = 0;
                            int jmlhOrderCompleted = 0;
                            int jmlhOrderReadytoShip = 0;
                            int jmlhOrderNew = 0;

                            foreach (var order in listDetails)
                            {
                                //1: waiting for delivery, 2: shipped, 3: Waiting_Cancel, 4: Waiting_Refuse, 5: canceled, 6: Completed, 7: Ready to ship
                                bool doInsert = true;
                                if (OrderNoInDb.Contains(Convert.ToString(order.orderId)) && order.orderState.ToString() == "1")
                                {
                                    doInsert = false;
                                }
                                else if (order.orderState.ToString() == "5") //CANCELED
                                {
                                    if (OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        doInsert = false;
                                        idOrderCancel = idOrderCancel + "'" + order.orderId + "',";
                                        jmlhOrderCancel++;
                                        //tidak ubah status menjadi selesai jika belum diisi faktur
                                        //remark by nurul 12/8/2021
                                        //var dsSIT01A = EDB.GetDataSet("CString", "SIT01A", "SELECT NO_REFERENSI, O.NO_BUKTI, O.STATUS_TRANSAKSI FROM SIT01A I INNER JOIN SOT01A O ON I.NO_SO = O.NO_BUKTI WHERE NO_REFERENSI = '" + order.orderId + "'");
                                        //if (dsSIT01A.Tables[0].Rows.Count == 0)
                                        //{
                                        //    doInsert = false;
                                        //}
                                        //end remark by nurul 12/8/2021
                                    }
                                    else
                                    {
                                        //tidak diinput jika order sudah selesai sebelum masuk MO
                                        doInsert = false;
                                    }
                                }
                                else if (order.orderState.ToString() == "6") // COMPLETED
                                {
                                    if (OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        idOrderComplete = idOrderComplete + "'" + order.orderId + "',";
                                    }
                                    doInsert = false;
                                }
                                else if (order.orderState.ToString() == "7") // READY TO SHIP
                                {
                                    if (OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        //jmlhOrderReadytoShip++;
                                        doInsert = false;
                                    }
                                    else
                                    {
                                        idOrderRTS = idOrderRTS + "'" + order.orderId + "',";
                                        doInsert = true;
                                    }
                                }
                                else if (order.orderState.ToString() == "3" || order.orderState.ToString() == "4")
                                {
                                    if (!OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        doInsert = false;
                                    }
                                    else
                                    {
                                        doInsert = false;
                                    }
                                }

                                if (doInsert)
                                {
                                    ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_ORDER_JD");
                                    ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_ORDERITEMS_JD");

                                    var dtNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    adaInsert = true;
                                    var statusEra = "";
                                    switch (order.orderState.ToString())
                                    {
                                        //1:Waiting for delivery, 2:Shipped, 3:Waiting_Cancel, 4:Waiting_Refuse, 5:Canceled, 6:Completed, 7:Ready to Ship
                                        case "1":
                                            statusEra = "01";
                                            break;
                                        case "2":
                                            statusEra = "03";
                                            break;
                                        case "3":
                                        case "4":
                                        case "5":
                                            statusEra = "11";
                                            break;
                                        case "6":
                                            statusEra = "04";
                                            break;
                                        case "7":
                                            statusEra = "01";
                                            break;
                                        default:
                                            statusEra = "99";
                                            break;
                                    }

                                    var nama = order.customerName != null ? order.customerName.Replace('\'', '`').Replace("'", "") : "";
                                    if (nama.Length > 30)
                                        nama = nama.Substring(0, 30);

                                    var vOrderAddress = order.address != null ? order.address.Replace('\'', '`').Replace("'", "") : "";
                                    var vDeliveryAddress = order.deliveryAddr != null ? order.deliveryAddr.Replace('\'', '`').Replace("'", "") : "";
                                    var vArea = order.area != null ? order.area.Replace('\'', '`').Replace("'", "") : "";
                                    var vCity = order.city != null ? order.city.Replace('\'', '`').Replace("'", "") : "";
                                    var vState = order.state != null ? order.state.Replace('\'', '`').Replace("'", "") : "";
                                    var messageCustomer = order.buyerMessage != null ? order.buyerMessage.Replace("'", "") : "";
                                    var vEmail = order.email != null ? order.email.Replace('\'', '`').Replace("'", "") : "";
                                    var vCodeKurir = order.carrierCode.ToString() != null ? order.carrierCode.ToString().Replace('\'', '`').Replace("'", "") : "";
                                    var vNamaKurir = order.carrierCompany != null ? order.carrierCompany.Replace('\'', '`').Replace("'", "") : "";
                                    var vNoResi = order.expressNo != null ? order.expressNo.Replace('\'', '`').Replace("'", "") : "";

                                    #region cut char
                                    if (data.nama_cust.Length > 30)
                                        data.nama_cust = data.nama_cust.Substring(0, 30);
                                    if (vCodeKurir.Length > 20)
                                    {
                                        vCodeKurir = vCodeKurir.Substring(0, 20);
                                    }
                                    if (messageCustomer.Length > 250)
                                    {
                                        messageCustomer = messageCustomer.Substring(0, 250);
                                    }
                                    if (vArea.Length > 50)
                                    {
                                        vArea = vArea.Substring(0, 50);
                                    }
                                    if (vCity.Length > 50)
                                    {
                                        vCity = vCity.Substring(0, 50);
                                    }
                                    if (vState.Length > 50)
                                    {
                                        vState = vState.Substring(0, 50);
                                    }
                                    if (vEmail.Length > 50)
                                    {
                                        vEmail = vEmail.Substring(0, 50);
                                    }
                                    if (vNamaKurir.Length > 50)
                                    {
                                        vNamaKurir = vNamaKurir.Substring(0, 50);
                                    }
                                    if (vNoResi.Length > 50)
                                    {
                                        vNoResi = vNoResi.Substring(0, 50);
                                    }
                                    string orderId = !string.IsNullOrEmpty(order.orderId.ToString()) ? order.orderId.ToString().Replace("'", "`") : "";
                                    if (orderId.Length > 50)
                                    {
                                        orderId = orderId.Substring(0, 50);
                                    }
                                    string TLP = !string.IsNullOrEmpty(order.phone) ? order.phone.Replace('\'', '`') : "";
                                    if (TLP.Length > 30)
                                        TLP = TLP.Substring(0, 30);
                                    string KODEPOS = !string.IsNullOrEmpty(order.postCode) ? order.postCode.Replace('\'', '`') : "";
                                    if (KODEPOS.Length > 7)
                                    {
                                        KODEPOS = KODEPOS.Substring(0, 7);
                                    }
                                    string userPin = !string.IsNullOrEmpty(order.userPin) ? order.userPin.Replace("'", "`") : "";
                                    if (userPin.Length > 50)
                                    {
                                        userPin = userPin.Substring(0, 50);
                                    }
                                    #endregion

                                    if (!string.IsNullOrEmpty(vNamaKurir))
                                    {
                                        if (vNamaKurir.Contains("Gosend"))
                                        {
                                            vNamaKurir = "Go-Send";
                                        }
                                    }

                                    //insertQ += "('" + order.address.Replace('\'', '`') + "','" + order.area.Replace('\'', '`') + "','" + DateTimeOffset.FromUnixTimeSeconds(order.bookTime / 1000).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss") + "','" + order.city.Replace('\'', '`') + "'," + order.couponAmount + ",'" + order.customerName + "','";
                                    //var insertQValue = "('" + vOrderAddress + "','" + vArea + "','" + DateTimeOffset.FromUnixTimeSeconds(order.bookTime / 1000).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss") + "','" + vCity + "'," + order.couponAmount + ",'" + nama + "','";
                                    var insertQValue = "('" + vOrderAddress + "','" + vArea + "','" + DateTimeOffset.FromUnixTimeSeconds(order.bookTime / 1000).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "','" + vCity + "'," + order.couponAmount + ",'" + nama + "','";
                                    insertQValue += vDeliveryAddress + "'," + order.deliveryType + ",'" + vEmail + "'," + order.freightAmount + "," + order.fullCutAmount + "," + order.installmentFee + ",'" + DateTimeOffset.FromUnixTimeSeconds(order.orderCompleteTime / 1000).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "','";
                                    insertQValue += orderId + "'," + order.orderSkuNum + "," + statusEra + "," + order.orderType + "," + order.paySubtotal + "," + order.paymentType + ",'" + TLP + "','" + KODEPOS + "'," + order.promotionAmount + ",'";
                                    //insertQValue += order.sendPay + "','" + vState + "'," + order.totalPrice + ",'" + order.userPin + "','" + data.no_cust + "','" + username + "','" + conn_id_order + "', '" + messageCustomer + "', '" + order.carrierCode + "', '" + order.carrierCompany + "', '" + order.expressNo + "', '" + data.nama_cust + "') ,";
                                    insertQValue += order.sendPay + "','" + vState + "'," + order.totalPrice + ",'" + userPin + "','" + data.no_cust + "','" + username + "','" + conn_id_order + "', '" + messageCustomer + "', '" + vCodeKurir + "', '" + vNamaKurir + "', '" + vNoResi + "', '" + data.nama_cust + "') ,";

                                    var insertOrderItemsValue = "";

                                    if (order.orderSkuinfos != null)
                                    {
                                        foreach (var ordItem in order.orderSkuinfos)
                                        {
                                            #region cut char
                                            string skuId = !string.IsNullOrEmpty(ordItem.skuId.ToString()) ? ordItem.skuId.ToString().Replace("'", "`") : "";
                                            if (skuId.Length > 50)
                                            {
                                                skuId = skuId.Substring(0, 50);
                                            }
                                            string spuId = !string.IsNullOrEmpty(ordItem.spuId.ToString()) ? ordItem.spuId.ToString().Replace("'", "`") : "";
                                            if (spuId.Length > 50)
                                            {
                                                spuId = spuId.Substring(0, 50);
                                            }
                                            string skuName = !string.IsNullOrEmpty(ordItem.skuName) ? ordItem.skuName.Replace("'", "`") : "";
                                            if (skuName.Length > 150)
                                            {
                                                skuName = skuName.Substring(0, 150);
                                            }

                                            #endregion
                                            insertOrderItemsValue += "('" + orderId + "'," + ordItem.commission + "," + ordItem.costPrice + "," + ordItem.couponAmount + "," + ordItem.fullCutAmount + ",";
                                            insertOrderItemsValue += ordItem.hasPromo + "," + ordItem.jdPrice + "," + ordItem.promotionAmount + ",'" + spuId + ";" + skuId + "','" + skuName + "',";
                                            insertOrderItemsValue += ordItem.skuNumber + ",'" + ordItem.spuId + "'," + ordItem.weight + ",'" + username + "','" + conn_id_order + "','" + DateTimeOffset.FromUnixTimeSeconds(order.bookTime / 1000).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "','" + data.no_cust + "','" + data.nama_cust + "') ,";
                                        }
                                    }

                                    var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + vCity + "%'");
                                    var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + vState + "%'");

                                    var kabKot = "3174";//set default value jika tidak ada di db
                                    var prov = "31";//set default value jika tidak ada di db

                                    if (tblProv.Tables[0].Rows.Count > 0)
                                        prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
                                    if (tblKabKot.Tables[0].Rows.Count > 0)
                                        kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();


                                    var vAddress = order.address != null ? order.address.Replace('\'', '`').Replace("'", "") : "";

                                    //var vPostCode = order.postCode != null ? order.postCode.Replace('\'', '`') : "";

                                    //insertPembeli += "('" + order.customerName.Replace('\'', '`') + "','" + order.address.Replace('\'', '`') + "','" + order.phone + "','" + order.email.Replace('\'', '`') + "',0,0,'0','01',";
                                    var insertPembeliValue = "('" + nama + "','" + vAddress + "','" + TLP + "','" + nama + "',0,0,'0','01',";
                                    insertPembeliValue += "1, 'IDR', '01', '" + vAddress + "', 0, 0, 0, 0, '1', 0, 0, ";
                                    insertPembeliValue += "'FP', '" + dtNow + "', '" + username + "', '" + KODEPOS + "', '" + vEmail + "', '" + kabKot + "', '" + prov + "', '" + vCity + "', '" + vState + "', '" + conn_id_arf01c + "') ,";

                                    if (!OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        if (string.IsNullOrEmpty(idOrderRTS))
                                        {
                                            jmlhNewOrder++;
                                        }
                                        insertQValue = insertQValue.Substring(0, insertQValue.Length - 2);
                                        EDB.ExecuteSQL(username, CommandType.Text, insertQ + insertQValue);


                                        insertOrderItemsValue = insertOrderItemsValue.Substring(0, insertOrderItemsValue.Length - 2);
                                        EDB.ExecuteSQL(username, CommandType.Text, insertOrderItems + insertOrderItemsValue);


                                        insertPembeliValue = insertPembeliValue.Substring(0, insertPembeliValue.Length - 2);
                                        EDB.ExecuteSQL(username, CommandType.Text, insertPembeli + insertPembeliValue);

                                        using (SqlCommand CommandSQL = new SqlCommand())
                                        {
                                            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = conn_id_arf01c;

                                            EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                                        }

                                        using (SqlCommand CommandSQL = new SqlCommand())
                                        {
                                            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = conn_id_order;
                                            CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                                            CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                            CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 1;
                                            CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = data.no_cust;

                                            EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);
                                        }
                                    }
                                }
                            }

                            if (adaInsert)
                            {
                                ret.status = 1;

                                if (jmlhNewOrder > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari JD.ID.");

                                    //add by nurul 25/1/2021, bundling
                                    //var listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in ('" + conn_id_order + "')").ToList();
                                    //if (listBrgKomponen.Count() > 0)
                                    //{
                                    //    ret.AdaKomponen = true;
                                    //}
                                    //end add by nurul 25/1/2021, bundling
                                    new StokControllerJob().updateStockMarketPlace(conn_id_order, data.DatabasePathErasoft, data.username);
                                }

                            }

                            if (!string.IsNullOrEmpty(idOrderCancel))
                            {
                                idOrderCancel = idOrderCancel.Substring(0, idOrderCancel.Length - 1);
                                var brgAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + conn_id_order + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + idOrderCancel + ") AND STATUS_TRANSAKSI <> '11' AND BRG <> 'NOT_FOUND' AND CUST = '" + cust + "'");
                                //change by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                                //var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2', STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN (" + idOrderCancel + ") AND STATUS_TRANSAKSI <> '11' AND CUST = '" + cust + "'");
                                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2',ORDER_CANCEL_DATE = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', STATUS_TRANSAKSI = '11', STATUS_KIRIM='5' WHERE NO_REFERENSI IN (" + idOrderCancel + ") AND STATUS_TRANSAKSI <> '11' AND CUST = '" + cust + "'");
                                //END change by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                                if (rowAffected > 0)
                                {
                                    //add by Tri 1 sep 2020, hapus packing list
                                    //remark by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                                    //var delPL = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03B WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN (" + idOrderCancel + ")  AND STATUS_TRANSAKSI = '11' AND CUST = '" + cust + "')");
                                    //var delPLDetail = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03C WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN (" + idOrderCancel + ")  AND STATUS_TRANSAKSI = '11' AND CUST = '" + cust + "')");
                                    //END remark by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                                    //end add by Tri 1 sep 2020, hapus packing list
                                    var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN (" + idOrderCancel + ") AND STATUS <> '2' AND ST_POSTING = 'T' AND CUST = '" + cust + "'");
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderCancel) + " Pesanan dari JD.ID dibatalkan.");

                                    //add by nurul 25/1/2021, bundling
                                    //var listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in ('" + conn_id_order + "')").ToList();
                                    //if (listBrgKomponen.Count() > 0)
                                    //{
                                    //    ret.AdaKomponen = true;
                                    //}
                                    //end add by nurul 25/1/2021, bundling

                                    //add by nurul 14/4/2021, stok bundling
                                    var sSQLInsertTempBundling = "INSERT INTO TEMP_ALL_MP_ORDER_ITEM_BUNDLING ([BRG],[CONN_ID],[TGL]) " +
                                                                 "SELECT DISTINCT C.UNIT AS BRG, '" + conn_id_order + "' AS CONN_ID, DATEADD(HOUR, +7, GETUTCDATE()) AS TGL " +
                                                                 "FROM TEMP_ALL_MP_ORDER_ITEM A(NOLOCK) " +
                                                                 "LEFT JOIN TEMP_ALL_MP_ORDER_ITEM_BUNDLING B(NOLOCK) ON B.CONN_ID = '" + conn_id_order + "' AND A.BRG = B.BRG " +
                                                                 "INNER JOIN STF03 C(NOLOCK) ON A.BRG = C.BRG " +
                                                                 "WHERE ISNULL(A.CONN_ID,'') = '" + conn_id_order + "' " +
                                                                 "AND ISNULL(B.BRG,'') = '' AND A.BRG <> 'NOT_FOUND'";
                                    var execInsertTempBundling = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, sSQLInsertTempBundling);
                                    //end add by nurul 14/4/2021, stok bundling

                                    new StokControllerJob().updateStockMarketPlace(conn_id_order, data.DatabasePathErasoft, data.username);
                                }
                            }

                            if (!string.IsNullOrEmpty(idOrderComplete))
                            {
                                idOrderComplete = idOrderComplete.Substring(0, idOrderComplete.Length - 1);
                                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI IN (" + idOrderComplete + ") AND STATUS_TRANSAKSI = '03'");
                                jmlhOrderCompleted = jmlhOrderCompleted + rowAffected;
                                if (jmlhOrderCompleted > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderCompleted) + " Pesanan dari JD.ID sudah selesai.");

                                    //add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                    if (!string.IsNullOrEmpty(idOrderComplete))
                                    {
                                        var dateTimeNow = Convert.ToDateTime(DateTime.Now.AddHours(7).ToString("yyyy-MM-dd"));
                                        string sSQLUpdateDatePesananSelesai = "UPDATE SIT01A SET TGL_KIRIM = '" + dateTimeNow + "' WHERE NO_REF IN (" + idOrderComplete + ")";
                                        var resultUpdateDatePesanan = EDB.ExecuteSQL("CString", CommandType.Text, sSQLUpdateDatePesananSelesai);
                                    }
                                    //end add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                }
                            }

                            if (!string.IsNullOrEmpty(idOrderRTS))
                            {
                                idOrderRTS = idOrderRTS.Substring(0, idOrderRTS.Length - 1);
                                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI IN (" + idOrderRTS + ") AND STATUS_TRANSAKSI = '0'");
                                jmlhOrderReadytoShip = jmlhOrderReadytoShip + rowAffected;
                                if (jmlhOrderReadytoShip > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderReadytoShip) + " Pesanan dari JD.ID Ready To Ship.");
                                }

                                //add by nurul 25/1/2021, bundling
                                //var listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in ('" + conn_id_order + "')").ToList();
                                //if (listBrgKomponen.Count() > 0)
                                //{
                                //    ret.AdaKomponen = true;
                                //}
                                //end add by nurul 25/1/2021, bundling
                                new StokControllerJob().updateStockMarketPlace(conn_id_order, data.DatabasePathErasoft, data.username);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            //return adaInsert;
            ret.recordCount = jmlhNewOrder;
            return ret;
        }
        //end add by nurul 4/5/2021, JDID versi 2

        //add by nurul 21/5/2021, JDID versi 2 tahap 2 
        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Harga Jual Produk {obj} ke JD.ID gagal.")]
        public async Task<string> JD_updatePriceV2(string DatabasePathErasoft, string stf02_brg, string log_CUST, string log_ActionCategory, string log_ActionName, JDIDAPIDataJob data, string id, int price, string uname)
        {
            SetupContext(DatabasePathErasoft, uname);
            try
            {
                var brgMp = "";
                if (id.Contains(";"))
                {
                    string[] brgSplit = id.Split(';');
                    if (brgSplit[1] != "0")
                    {
                        brgMp = brgSplit[1].ToString();
                    }
                }
                else
                {
                    brgMp = id;
                }

                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 2)
                {
                    data = RefreshToken(data);
                    var sysParams = new Dictionary<string, string>();
                    this.ParamJson = "{\"salePrice\":\"" + price + "\",\"skuId\":\"" + brgMp + "\"}";
                    sysParams.Add("360buy_param_json", this.ParamJson);

                    sysParams.Add("access_token", data.accessToken);
                    sysParams.Add("app_key", data.appKey);
                    this.Method = "jingdong.seller.price.updatePriceBySkuIds"; //update skus prices
                    sysParams.Add("method", this.Method);
                    var gettimestamp = getCurrentTimeFormatted();
                    sysParams.Add("timestamp", gettimestamp);
                    sysParams.Add("v", this.Version2);
                    sysParams.Add("format", this.Format);
                    sysParams.Add("sign_method", this.SignMethod);

                    var signature = this.generateSign(sysParams, data.appSecret);

                    string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + Uri.EscapeDataString(this.ParamJson) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                    urll += "&format=json&sign_method=md5";
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    try
                    {
                        using (WebResponse response = await myReq.GetResponseAsync())
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream);
                                responseFromServer = reader.ReadToEnd();
                                responseApi = true; break;
                            }
                        }
                    }
                    //catch (WebException ex)
                    //{
                    //    string err1 = "";
                    //    if (ex.Status == WebExceptionStatus.ProtocolError)
                    //    {
                    //        WebResponse resp1 = ex.Response;
                    //        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                    //        {
                    //            err1 = sr1.ReadToEnd();
                    //        }
                    //    }
                    //    //throw new Exception(err1);
                    //}
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                        {
                            retry = retry + 1;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            if (retry == 3)
                            {
                                throw new Exception(msg);
                            }
                        }
                        else
                        {
                            retry = 3;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            throw new Exception(msg);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    try
                    {
                        var retPrice = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDUpdatePriceV2)) as JDIDUpdatePriceV2;
                        if (retPrice.jingdong_seller_price_updatePriceBySkuIds_response.returnType != null)
                        {
                            if (retPrice.jingdong_seller_price_updatePriceBySkuIds_response.returnType.success)
                            {
                                if (log_ActionName.Contains("UPDATE_MASSAL"))
                                {
                                    var dataLog = log_ActionName.Split('_');
                                    if (dataLog.Length >= 4)
                                    {
                                        var nobuk = dataLog[2];
                                        var indexData = Convert.ToInt32(dataLog[3]);
                                        var log_b = ErasoftDbContext.LOG_HARGAJUAL_B.Where(m => m.NO_BUKTI == nobuk && m.NO_FILE == indexData).FirstOrDefault();
                                        if (log_b != null)
                                        {
                                            var currentProgress = log_b.KET.Split('/');
                                            if (currentProgress.Length == 2)
                                            {
                                                log_b.KET = (Convert.ToInt32(currentProgress[0]) + 1) + "/" + currentProgress[1];
                                                ErasoftDbContext.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception(retPrice.jingdong_seller_price_updatePriceBySkuIds_response.returnType.message.ToString());
                            }
                        }
                        else
                        {
                            throw new Exception("Update harga gagal.");
                        }

                    }
                    catch (Exception ex2)
                    {
                        throw new Exception(ex2.InnerException == null ? ex2.Message.ToString() : ex2.InnerException.Message.ToString());
                    }
                }
                else
                {
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception(msg);
            }

            return "";
        }

        public async Task<string> JD_updateSKUV2(JDIDAPIDataJob data, string sSKUName, string sSellerSKUID, string sJDPrice, string sCostPrice, string sSKUID, string spuId)
        {
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update SKU V2",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = sSellerSKUID,
                REQUEST_ATTRIBUTE_2 = spuId,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            try
            {
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 2)
                {
                    data = RefreshToken(data);
                    var sysParams = new Dictionary<string, string>();
                    this.ParamJson = "{\"spuId\":\"" + spuId + "\",\"costPrice\":\"" + sCostPrice + "\",\"sellerSkuId\":\"" + sSellerSKUID + "\",\"skuName\":\"" + sSKUName + "\",\"jdPrice\":\"" + sJDPrice + "\",\"skuId\":\"" + sSKUID + "\"}";
                    sysParams.Add("360buy_param_json", this.ParamJson);

                    sysParams.Add("access_token", data.accessToken);
                    sysParams.Add("app_key", data.appKey);
                    this.Method = "jingdong.seller.product.sku.write.updateSkuList"; //update skus prices
                    sysParams.Add("method", this.Method);
                    var gettimestamp = getCurrentTimeFormatted();
                    sysParams.Add("timestamp", gettimestamp);
                    sysParams.Add("v", this.Version2);
                    sysParams.Add("format", this.Format);
                    sysParams.Add("sign_method", this.SignMethod);

                    var signature = this.generateSign(sysParams, data.appSecret);

                    string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + Uri.EscapeDataString(this.ParamJson) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                    urll += "&format=json&sign_method=md5";
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    try
                    {
                        using (WebResponse response = await myReq.GetResponseAsync())
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream);
                                responseFromServer = reader.ReadToEnd();
                                responseApi = true; break;
                            }
                        }
                    }
                    //catch (WebException ex)
                    //{
                    //    string err1 = "";
                    //    if (ex.Status == WebExceptionStatus.ProtocolError)
                    //    {
                    //        WebResponse resp1 = ex.Response;
                    //        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                    //        {
                    //            err1 = sr1.ReadToEnd();
                    //        }
                    //    }
                    //    //throw new Exception(err1);
                    //}
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                        {
                            retry = retry + 1;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            if (retry == 3)
                            {
                                currentLog.REQUEST_EXCEPTION = msg;
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                throw new Exception(msg);
                            }
                        }
                        else
                        {
                            retry = 3;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            currentLog.REQUEST_EXCEPTION = msg;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            throw new Exception(msg);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    try
                    {
                        var retPrice = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDUpdateSKUV2)) as JDIDUpdateSKUV2;
                        if (retPrice.jingdong_seller_product_sku_write_updateSkuList_response.returnType != null)
                        {
                            if (retPrice.jingdong_seller_product_sku_write_updateSkuList_response.returnType.success)
                            {
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);
                            }
                            else
                            {
                                currentLog.REQUEST_EXCEPTION = retPrice.jingdong_seller_product_sku_write_updateSkuList_response.returnType.message.ToString();
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                throw new Exception(retPrice.jingdong_seller_product_sku_write_updateSkuList_response.returnType.message.ToString());
                            }
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = "JD_updateSKUV2 gagal.";
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            throw new Exception("JD_updateSKUV2 gagal.");
                        }

                    }
                    catch (Exception ex2)
                    {
                        string msg = ex2.InnerException != null ? ex2.InnerException.Message : ex2.Message;
                        currentLog.REQUEST_EXCEPTION = msg;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        throw new Exception(ex2.InnerException == null ? ex2.Message.ToString() : ex2.InnerException.Message.ToString());
                    }
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = "Tidak ada respon dari API.";
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                currentLog.REQUEST_EXCEPTION = msg;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                throw new Exception(msg);
            }

            return "";
        }

        public static long CurrentTimeMillis()
        {
            //        return (long)DateTime.Now.ToUniversalTime().Subtract(
            //new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            //).TotalMilliseconds;
            return (long)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        public class updateImage
        {
            public updateImageJDID _360buy_param_json { get; set; }
        }
        public class updateImageJDID
        {
            public updateImageDetailJDID imageApiVo { get; set; }
        }
        public class updateImageDetailJDID
        {
            public string colorId { get; set; }
            public int order { get; set; }
            public long productId { get; set; }
            //public Dictionary<string, string> imageByteBase64 { get; set; }
            public string imageByteBase64 { get; set; }
        }
        public async Task<string> JD_addSKUDetailPictureV2(JDIDAPIDataJob data, string kdbrg, string urlPicture, int urutan, bool mainPic, string spuID)
        {
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Picture V2",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = kdbrg,
                REQUEST_ATTRIBUTE_2 = spuID,
                //CUST_ATTRIBUTE_3 = urutan + ";" + urlPicture,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            try
            {
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 2)
                {
                    data = RefreshToken(data);
                    long milis = CurrentTimeMillis();

                    var myData = new updateImageJDID() { };
                    var detailMyData = new updateImageDetailJDID()
                    {
                        order = urutan,
                        productId = Convert.ToInt64(spuID),
                    };
                    if (!string.IsNullOrWhiteSpace(urlPicture))
                    {
                        using (var client = new HttpClient())
                        {
                            var bytes = await client.GetByteArrayAsync(urlPicture);
                            using (var stream = new MemoryStream(bytes, true))
                            {
                                var img = Image.FromStream(stream);
                                float newResolution = img.Height;
                                if (img.Width < newResolution)
                                {
                                    newResolution = img.Width;
                                }
                                var resizedImage = (Image)BlibliResizeImage(img, Convert.ToInt32(newResolution), Convert.ToInt32(newResolution));
                                //var resizedImage = (Image)BlibliResizeImageFromStream(stream);

                                //change by calvin 1 maret 2019
                                //ImageConverter _imageConverter = new ImageConverter();
                                //byte[] resizedByteArr = (byte[])_imageConverter.ConvertTo(resizedImage, typeof(byte[]));
                                System.Drawing.Imaging.ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);

                                System.Drawing.Imaging.Encoder myEncoder =
                                    System.Drawing.Imaging.Encoder.Quality;
                                System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);

                                System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, 90L);
                                myEncoderParameters.Param[0] = myEncoderParameter;


                                var resizedStream = new System.IO.MemoryStream();
                                resizedImage.Save(resizedStream, jpgEncoder, myEncoderParameters);
                                //img.Save(resizedStream, jpgEncoder, myEncoderParameters);
                                resizedStream.Position = 0;
                                byte[] resizedByteArr = resizedStream.ToArray();
                                //end change by calvin 1 maret 2019
                                resizedStream.Dispose();

                                var image64 = Convert.ToBase64String(resizedByteArr);
                                detailMyData.imageByteBase64 = image64;

                                var sysParams = new Dictionary<string, string>();
                                detailMyData.colorId = "0000000000";

                                myData.imageApiVo = detailMyData;
                                var newData = JsonConvert.SerializeObject(myData);
                                this.ParamJson = newData;
                                sysParams.Add("360buy_param_json", this.ParamJson);

                                var sysParamsBody = new Dictionary<string, string>();
                                sysParamsBody.Add("360buy_param_json", this.ParamJson);
                                var newDataSysParamsBody = JsonConvert.SerializeObject(sysParamsBody);

                                sysParams.Add("access_token", data.accessToken);
                                sysParams.Add("app_key", data.appKey);
                                this.Method = "jingdong.seller.product.sku.write.updateProductImages"; //update skus prices
                                sysParams.Add("method", this.Method);
                                var gettimestamp = getCurrentTimeFormatted();
                                sysParams.Add("timestamp", gettimestamp);
                                sysParams.Add("v", this.Version2);
                                sysParams.Add("format", this.Format);
                                sysParams.Add("sign_method", this.SignMethod);



                                var signature = this.generateSign(sysParams, data.appSecret);

                                //string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + this.ParamJson + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                                string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                                urll += "&format=json&sign_method=md5";

                                var client_jd = new RestClient(urll);
                                client_jd.Timeout = -1;
                                var request = new RestRequest(RestSharp.Method.POST);
                                request.AlwaysMultipartFormData = true;
                                request.AddParameter("360buy_param_json", newData);
                                try
                                {
                                    IRestResponse response = client_jd.Execute(request);
                                    responseFromServer = response.Content;
                                    responseApi = true; break;
                                }
                                ////catch (WebException ex)
                                ////{
                                ////    string err1 = "";
                                ////    if (ex.Status == WebExceptionStatus.ProtocolError)
                                ////    {
                                ////        WebResponse resp1 = ex.Response;
                                ////        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                                ////        {
                                ////            err1 = sr1.ReadToEnd();
                                ////        }
                                ////    }
                                ////    //throw new Exception(err1);
                                ////}
                                catch (Exception ex)
                                {
                                    if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                                    {
                                        retry = retry + 1;
                                        string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                                        if (retry == 3)
                                        {
                                            currentLog.REQUEST_EXCEPTION = msg;
                                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                            throw new Exception(msg);
                                        }
                                    }
                                    else
                                    {
                                        retry = 3;
                                        string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                                        currentLog.REQUEST_EXCEPTION = msg;
                                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                        throw new Exception(msg);
                                    }
                                }
                            }
                        }
                    }

                }
                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    try
                    {
                        var retPrice = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDaddSKUDetailPictureV2)) as JDIDaddSKUDetailPictureV2;
                        if (retPrice.jingdong_seller_product_sku_write_updateProductImages_response.returnType != null)
                        {
                            if (retPrice.jingdong_seller_product_sku_write_updateProductImages_response.returnType.success)
                            {
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);
                            }
                            else
                            {
                                currentLog.REQUEST_EXCEPTION = retPrice.jingdong_seller_product_sku_write_updateProductImages_response.returnType.message.ToString();
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                throw new Exception(retPrice.jingdong_seller_product_sku_write_updateProductImages_response.returnType.message.ToString());
                            }
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = "JD_addSKUDetailPictureV2 gagal.";
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            throw new Exception("JD_addSKUDetailPictureV2 gagal.");
                        }

                    }
                    catch (Exception ex2)
                    {
                        string msg = ex2.InnerException != null ? ex2.InnerException.Message : ex2.Message;
                        currentLog.REQUEST_EXCEPTION = msg;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        throw new Exception(msg);
                    }
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = "Tidak ada respon dari API.";
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                currentLog.REQUEST_EXCEPTION = msg;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                throw new Exception(msg);
            }

            return "";
        }

        public static Bitmap BlibliResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private System.Drawing.Imaging.ImageCodecInfo GetEncoder(System.Drawing.Imaging.ImageFormat format)
        {
            System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders();
            foreach (System.Drawing.Imaging.ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public async Task<string> JD_doAuditProductV2(JDIDAPIDataJob data, string spuid, string brg)
        {
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Audit Product V2",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = brg,
                REQUEST_ATTRIBUTE_2 = spuid,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            try
            {
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 2)
                {
                    data = RefreshToken(data);
                    var sysParams = new Dictionary<string, string>();
                    this.ParamJson = "{\"spuId\":\"" + spuid + "\"}";
                    sysParams.Add("360buy_param_json", this.ParamJson);

                    sysParams.Add("access_token", data.accessToken);
                    sysParams.Add("app_key", data.appKey);
                    this.Method = "jingdong.seller.product.api.write.submitAudit"; //update skus prices
                    sysParams.Add("method", this.Method);
                    var gettimestamp = getCurrentTimeFormatted();
                    sysParams.Add("timestamp", gettimestamp);
                    sysParams.Add("v", this.Version2);
                    sysParams.Add("format", this.Format);
                    sysParams.Add("sign_method", this.SignMethod);

                    var signature = this.generateSign(sysParams, data.appSecret);

                    string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + Uri.EscapeDataString(this.ParamJson) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                    urll += "&format=json&sign_method=md5";
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    try
                    {
                        using (WebResponse response = await myReq.GetResponseAsync())
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream);
                                responseFromServer = reader.ReadToEnd();
                                responseApi = true; break;
                            }
                        }
                    }
                    //catch (WebException ex)
                    //{
                    //    string err1 = "";
                    //    if (ex.Status == WebExceptionStatus.ProtocolError)
                    //    {
                    //        WebResponse resp1 = ex.Response;
                    //        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                    //        {
                    //            err1 = sr1.ReadToEnd();
                    //        }
                    //    }
                    //    //throw new Exception(err1);
                    //}
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                        {
                            retry = retry + 1;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            if (retry == 3)
                            {
                                currentLog.REQUEST_EXCEPTION = msg;
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                throw new Exception(msg);
                            }
                        }
                        else
                        {
                            retry = 3;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            currentLog.REQUEST_EXCEPTION = msg;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            throw new Exception(msg);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    try
                    {
                        var retPrice = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDdoAuditProductV2)) as JDIDdoAuditProductV2;
                        if (retPrice.jingdong_seller_product_api_write_submitAudit_response.returnType != null)
                        {
                            if (retPrice.jingdong_seller_product_api_write_submitAudit_response.returnType.success)
                            {
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);

                                var tblCustomer = ErasoftDbContext.ARF01.Where(m => m.CUST == data.no_cust).FirstOrDefault();
                                if (tblCustomer.TIDAK_HIT_UANG_R)
                                {
                                    var stf02 = ErasoftDbContext.STF02.Where(m => m.BRG == brg).FirstOrDefault();
                                    if (stf02 != null)
                                    {
                                        MasterOnline.Controllers.JDIDAPIData dataStok = new MasterOnline.Controllers.JDIDAPIData()
                                        {
                                            accessToken = data.accessToken,
                                            appKey = data.appKey,
                                            appSecret = data.appSecret,
                                            //add by nurul 6/6/2021
                                            no_cust = data.no_cust,
                                            username = data.username,
                                            email = data.email,
                                            DatabasePathErasoft = data.DatabasePathErasoft,
                                            versi = data.versi,
                                            tgl_expired = data.tgl_expired,
                                            merchant_code = data.merchant_code,
                                            refreshToken = data.refreshToken
                                            //add by nurul 6/6/2021
                                        };
                                        StokControllerJob stokAPI = new StokControllerJob(data.DatabasePathErasoft, username);
                                        if (stf02.TYPE == "4")
                                        {
                                            var listStf02 = ErasoftDbContext.STF02.Where(m => m.PART == brg).ToList();
                                            foreach (var barang in listStf02)
                                            {
                                                var stf02h = ErasoftDbContext.STF02H.Where(m => m.BRG == barang.BRG && m.IDMARKET == tblCustomer.RecNum).FirstOrDefault();
                                                if (stf02h != null)
                                                {
                                                    if (!string.IsNullOrEmpty(stf02h.BRG_MP))
                                                    {
                                                        //add by nurul 4/5/2021, JDID versi 2
                                                        if (tblCustomer.KD_ANALISA == "2")
                                                        {
#if (DEBUG || Debug_AWS)
                                                            Task.Run(() => stokAPI.JD_updateStockV2(data.DatabasePathErasoft, stf02h.BRG, tblCustomer.CUST, "Stock", "Update Stok", dataStok, stf02h.BRG_MP, 0, username, null)).Wait();
#else
                                                    string EDBConnID = EDB.GetConnectionString("ConnId");
                                                var sqlStorage = new SqlServerStorage(EDBConnID);

                                                var Jobclient = new BackgroundJobClient(sqlStorage);
                                                Jobclient.Enqueue<StokControllerJob>(x => x.JD_updateStockV2(data.DatabasePathErasoft, stf02h.BRG, tblCustomer.CUST, "Stock", "Update Stok", dataStok, stf02h.BRG_MP, 0, username, null));
#endif
                                                        }
                                                        else
                                                        //end add by nurul 4/5/2021, JDID versi 2
                                                        {
#if (DEBUG || Debug_AWS)
                                                            Task.Run(() => stokAPI.JD_updateStock(data.DatabasePathErasoft, stf02h.BRG, tblCustomer.CUST, "Stock", "Update Stok", dataStok, stf02h.BRG_MP, 0, username, null)).Wait();
#else
                                                string EDBConnID = EDB.GetConnectionString("ConnId");
                                                var sqlStorage = new SqlServerStorage(EDBConnID);

                                                var Jobclient = new BackgroundJobClient(sqlStorage);
                                                Jobclient.Enqueue<StokControllerJob>(x => x.JD_updateStock(data.DatabasePathErasoft, stf02h.BRG, tblCustomer.CUST, "Stock", "Update Stok", dataStok, stf02h.BRG_MP, 0, username, null));
#endif
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var stf02h = ErasoftDbContext.STF02H.Where(m => m.BRG == stf02.BRG && m.IDMARKET == tblCustomer.RecNum).FirstOrDefault();
                                            if (stf02h != null)
                                            {
                                                if (!string.IsNullOrEmpty(stf02h.BRG_MP))
                                                {
                                                    //add by nurul 4/5/2021, JDID versi 2
                                                    if (tblCustomer.KD_ANALISA == "2")
                                                    {
#if (DEBUG || Debug_AWS)
                                                        Task.Run(() => stokAPI.JD_updateStockV2(data.DatabasePathErasoft, stf02h.BRG, tblCustomer.CUST, "Stock", "Update Stok", dataStok, stf02h.BRG_MP, 0, username, null)).Wait();
#else
                                            string EDBConnID = EDB.GetConnectionString("ConnId");
                                            var sqlStorage = new SqlServerStorage(EDBConnID);

                                            var Jobclient = new BackgroundJobClient(sqlStorage);
                                            Jobclient.Enqueue<StokControllerJob>(x => x.JD_updateStockV2(data.DatabasePathErasoft, stf02h.BRG, tblCustomer.CUST, "Stock", "Update Stok", dataStok, stf02h.BRG_MP, 0, username, null));
#endif
                                                    }
                                                    else
                                                    //end add by nurul 4/5/2021, JDID versi 2
                                                    {
#if (DEBUG || Debug_AWS)
                                                        Task.Run(() => stokAPI.JD_updateStock(data.DatabasePathErasoft, stf02h.BRG, tblCustomer.CUST, "Stock", "Update Stok", dataStok, stf02h.BRG_MP, 0, username, null)).Wait();
#else
                                            string EDBConnID = EDB.GetConnectionString("ConnId");
                                            var sqlStorage = new SqlServerStorage(EDBConnID);

                                            var Jobclient = new BackgroundJobClient(sqlStorage);
                                            Jobclient.Enqueue<StokControllerJob>(x => x.JD_updateStock(data.DatabasePathErasoft, stf02h.BRG, tblCustomer.CUST, "Stock", "Update Stok", dataStok, stf02h.BRG_MP, 0, username, null));
#endif
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                currentLog.REQUEST_EXCEPTION = retPrice.jingdong_seller_product_api_write_submitAudit_response.returnType.message.ToString();
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                throw new Exception(retPrice.jingdong_seller_product_api_write_submitAudit_response.returnType.message.ToString());
                            }
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = "JD_doAuditProductV2 gagal.";
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            throw new Exception("JD_doAuditProductV2 gagal.");
                        }

                    }
                    catch (Exception ex2)
                    {
                        string msg = ex2.InnerException != null ? ex2.InnerException.Message : ex2.Message;
                        currentLog.REQUEST_EXCEPTION = msg;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        throw new Exception(msg);
                    }
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = "Tidak ada respon dari API.";
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                currentLog.REQUEST_EXCEPTION = msg;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                throw new Exception(msg);
            }

            return "";
        }

        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke JDID Gagal.")]
        public async Task<string> JD_CreateProductV2(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, JDIDAPIDataJob data)
        {
            SetupContext(data.DatabasePathErasoft, data.username);

            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper()).FirstOrDefault();
            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == log_CUST.ToUpper()).FirstOrDefault();
            if (brgInDb == null || marketplace == null)
                return "invalid passing data";
            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum && b.DISPLAY == true).FirstOrDefault();
            if (detailBrg == null)
                return "invalid passing data";

            var listattributeIDGroup = "";
            var listattributeIDAllVariantGroup = "";

            var listattributeIDAllVariantGroup1 = "";
            var listattributeIDAllVariantGroup2 = "";
            var listattributeIDAllVariantGroup3 = "";

            //add by nurul 28/5/2021
            spuInfo_JDIDCREATE spuInfoJD = new spuInfo_JDIDCREATE() { };
            List<skuList_JDIDCREATE> skuListJD = new List<skuList_JDIDCREATE>() { };
            var tempcommonAttributeIds = "";
            //end add by nurul 28/5/2021

            //Start handle description
            var vDescription = brgInDb.Deskripsi;
            if (!string.IsNullOrEmpty(detailBrg.DESKRIPSI_MP))
            {
                if (detailBrg.DESKRIPSI_MP != "null")
                    vDescription = detailBrg.DESKRIPSI_MP;
            }
            vDescription = new StokControllerJob().RemoveSpecialCharacters(WebUtility.HtmlDecode(WebUtility.HtmlDecode(vDescription)));

            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
            //vDescription = vDescription.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
            vDescription = vDescription.Replace("<li>", "- ").Replace("</li>", "\r\n");
            vDescription = vDescription.Replace("<ul>", "").Replace("</ul>", "\r\n");
            vDescription = vDescription.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
            vDescription = vDescription.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
            vDescription = vDescription.Replace("\r\r", "<br />");
            //vDescription = vDescription.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");
            //end add by nurul 20/1/2020, handle <p> dan enter double di shopee

            //add by calvin 10 september 2019
            vDescription = vDescription.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
            vDescription = vDescription.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
            vDescription = vDescription.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
            vDescription = vDescription.Replace("<p>", "").Replace("</p>", "");
            vDescription = vDescription.Replace("\r", "");
            //HttpBody.description = HttpBody.description.Replace("<li>", "- ").Replace("</li>", "\r\n");
            //HttpBody.description = HttpBody.description.Replace("&nbsp;", "");

            //vDescription = System.Text.RegularExpressions.Regex.Replace(vDescription, "<.*?>", String.Empty);
            //end add by calvin 10 september 2019

            //postData += "&short_description=" + Uri.EscapeDataString(vDescription);
            //postData += "&description=" + Uri.EscapeDataString(vDescription);
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
            double qty_stock = 0;
            //qty_stock = brgInDb.ISI;
            //end handle stock

            var weight = Convert.ToDouble(brgInDb.BERAT / 1000);

            var namafull = "";
            namafull = brgInDb.NAMA;
            if (!string.IsNullOrEmpty(brgInDb.NAMA2))
            {
                namafull += " " + brgInDb.NAMA2;
            }
            if (!string.IsNullOrEmpty(brgInDb.NAMA3))
            {
                namafull += " " + brgInDb.NAMA3;
            }
            if (!string.IsNullOrEmpty(detailBrg.NAMA_BARANG_MP))
            {
                namafull = detailBrg.NAMA_BARANG_MP;
            }

            var commonAttribute = "";

            string sMethod = "epi.ware.openapi.SpuApi.publishWare";

            var urlHref = detailBrg.AVALUE_44;
            var paramHref = "";
            var tempUrlHref = "";
            if (!string.IsNullOrEmpty(urlHref))
            {
                if (!urlHref.Contains("http://"))
                {
                    urlHref = "http://" + urlHref;
                    paramHref = "\"subtitleHref\":\"" + urlHref + "\", \"subtitleHrefM\":\"" + urlHref + "\",";
                    tempUrlHref = urlHref;
                }
            }

            var paramSKUVariant = "";

            try
            {
                if (brgInDb.TYPE != "4")
                {
                    var attributeList = "";
                    for (int i = 1; i <= 30; i++)
                    {
                        string attribute_id = Convert.ToString(detailBrg["ACODE_" + i.ToString()]);
                        string value = Convert.ToString(detailBrg["AVALUE_" + i.ToString()]);
                        if (!string.IsNullOrWhiteSpace(attribute_id) && !string.IsNullOrWhiteSpace(value))
                        {
                            if (value != "null")
                            {
                                //HttpBody.attributes.Add(new ShopeeAttributeClass
                                //{
                                //    attributes_id = Convert.ToInt64(attribute_id),
                                //    value = value.Trim()
                                //});
                                attributeList += attribute_id + ":" + value + ";";
                            }

                        }
                    }
                    tempcommonAttributeIds = attributeList;
                }
            }
            catch (Exception ex)
            {

            }

            if (brgInDb.TYPE == "4") // punya variasi
            {
                //handle variasi product
                #region variasi product
                var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == kodeProduk).ToList();
                var var_strukturVar = ErasoftDbContext.STF02I.Where(p => p.BRG == kodeProduk && p.MARKET == "JDID").ToList().OrderBy(p => p.RECNUM);

                var listMP_VALUE_VAR = var_strukturVar.Select(a => a.MP_VALUE_VAR).ToList();
                var valueName = MoDbContext.AttributeOptJDID.Where(a => listMP_VALUE_VAR.Contains(a.ACODE)).ToList();

                foreach (var itemData in var_stf02)
                {
                    #region varian LV1
                    if (!string.IsNullOrEmpty(itemData.Sort8))
                    {
                        var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == itemData.Sort8).FirstOrDefault();
                        listattributeIDAllVariantGroup1 = variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR;
                        //listattributeIDAllVariantGroup1 = valueName.Where(a => a.ACODE == variant_id_group.MP_VALUE_VAR).Select(a => a.OPTION_VALUE).FirstOrDefault() + ":" + variant_id_group.MP_JUDUL_VAR;
                        if (!listattributeIDAllVariantGroup.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                            listattributeIDAllVariantGroup += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                        //listattributeIDAllVariantGroup += valueName.Where(a => a.ACODE == variant_id_group.MP_VALUE_VAR).Select(a => a.OPTION_VALUE).FirstOrDefault() + ":" + variant_id_group.MP_JUDUL_VAR + ";";
                    }
                    #endregion

                    #region varian LV2
                    if (!string.IsNullOrEmpty(itemData.Sort9))
                    {
                        var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == itemData.Sort9).FirstOrDefault();
                        listattributeIDAllVariantGroup2 = variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR;
                        //listattributeIDAllVariantGroup2 = valueName.Where(a => a.ACODE == variant_id_group.MP_VALUE_VAR).Select(a => a.OPTION_VALUE).FirstOrDefault() + ":" + variant_id_group.MP_JUDUL_VAR;
                        if (!listattributeIDAllVariantGroup.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                            listattributeIDAllVariantGroup += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                        //listattributeIDAllVariantGroup += valueName.Where(a => a.ACODE == variant_id_group.MP_VALUE_VAR).Select(a => a.OPTION_VALUE).FirstOrDefault() + ":" + variant_id_group.MP_JUDUL_VAR + ";";
                    }
                    #endregion

                    #region varian LV3
                    if (!string.IsNullOrEmpty(itemData.Sort10))
                    {
                        var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 3 && p.KODE_VAR == itemData.Sort10).FirstOrDefault();
                        listattributeIDAllVariantGroup3 = variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR;
                        //listattributeIDAllVariantGroup3 = valueName.Where(a => a.ACODE == variant_id_group.MP_VALUE_VAR).Select(a => a.OPTION_VALUE).FirstOrDefault() + ":" + variant_id_group.MP_JUDUL_VAR;
                        if (!listattributeIDAllVariantGroup.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                            listattributeIDAllVariantGroup += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                        //listattributeIDAllVariantGroup += valueName.Where(a => a.ACODE == variant_id_group.MP_VALUE_VAR).Select(a => a.OPTION_VALUE).FirstOrDefault() + ":" + variant_id_group.MP_JUDUL_VAR + ";";
                    }
                    #endregion

                    if (listattributeIDAllVariantGroup1.Length > 0)
                        listattributeIDGroup = listattributeIDAllVariantGroup1;
                    if (listattributeIDAllVariantGroup2.Length > 0)
                        listattributeIDGroup += ";" + listattributeIDAllVariantGroup2;
                    if (listattributeIDAllVariantGroup3.Length > 0)
                        listattributeIDGroup += ";" + listattributeIDAllVariantGroup3;

                    var namafullVariant = "";
                    namafullVariant = itemData.NAMA;
                    if (!string.IsNullOrEmpty(itemData.NAMA2))
                    {
                        namafullVariant += itemData.NAMA2;
                    }
                    if (!string.IsNullOrEmpty(itemData.NAMA3))
                    {
                        namafullVariant += itemData.NAMA3;
                    }

                    var detailBrgMP = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == itemData.BRG.ToUpper() && b.IDMARKET == marketplace.RecNum && b.DISPLAY == true).FirstOrDefault();

                    paramSKUVariant += "{\"costPrice\":" + detailBrgMP.HJUAL + ",\"jdPrice\":" + detailBrgMP.HJUAL + ", \"saleAttributeIds\":\"" + listattributeIDGroup + "\", \"sellerSkuId\":\"" + detailBrgMP.BRG + "\", \"skuName\":\"" + namafullVariant + "\", \"stock\":" + qty_stock + ", \"upc\":\"upc\" } ,";

                    skuList_JDIDCREATE skuInfo = new skuList_JDIDCREATE()
                    {
                        saleAttributeIds = listattributeIDGroup,
                        costPrice = Convert.ToInt64(detailBrgMP.HJUAL),
                        //upc = "upc",
                        sellerSkuId = detailBrgMP.BRG,
                        //saleAttrValueAlias = listattributeIDGroup,
                        skuName = namafullVariant,
                        jdPrice = Convert.ToInt64(detailBrgMP.HJUAL),
                        stock = Convert.ToInt64(qty_stock)
                    };
                    skuListJD.Add(skuInfo);
                }

                if (paramSKUVariant.Length > 0 && listattributeIDAllVariantGroup.Length > 0)
                {
                    paramSKUVariant = paramSKUVariant.Substring(0, paramSKUVariant.Length - 1);
                    listattributeIDAllVariantGroup = listattributeIDAllVariantGroup.Substring(0, listattributeIDAllVariantGroup.Length - 1);
                    commonAttribute = "\"commonAttributeIds\":\"" + listattributeIDAllVariantGroup + "\", ";
                    tempcommonAttributeIds = listattributeIDAllVariantGroup;
                }

                #endregion
                //end handle variasi product
            }
            else
            {

                //commonAttribute = "\"commonAttributeIds\":\"" + commonAttribute + "\", ";

                paramSKUVariant = "{\"costPrice\":" + detailBrg.HJUAL + ",\"jdPrice\":" + detailBrg.HJUAL + ", \"sellerSkuId\":\"" + detailBrg.BRG + "\", \"skuName\":\"" + namafull + "\", \"stock\":" + qty_stock + ", \"upc\":\"upc\" }";

                skuList_JDIDCREATE skuInfo = new skuList_JDIDCREATE()
                {
                    //saleAttributeIds = 
                    costPrice = Convert.ToInt64(detailBrg.HJUAL),
                    //upc = "upc",
                    sellerSkuId = detailBrg.BRG,
                    //saleAttrValueAlias 
                    skuName = namafull,
                    jdPrice = Convert.ToInt64(detailBrg.HJUAL),
                    stock = Convert.ToInt64(qty_stock)
                };
                skuListJD.Add(skuInfo);
            }


            var paramQualityAsurance = "";
            if (!string.IsNullOrEmpty(detailBrg.ANAME_47))
            {
                paramQualityAsurance = " \"qualityDays\":" + detailBrg.ANAME_47 + ", ";
            }

            var skeyword = "";

            if (!string.IsNullOrEmpty(detailBrg.AVALUE_46))
            {
                skeyword = detailBrg.AVALUE_46 + ",";
            }
            if (!string.IsNullOrEmpty(detailBrg.AVALUE_48))
            {
                skeyword = skeyword + detailBrg.AVALUE_48 + ",";
            }
            if (!string.IsNullOrEmpty(detailBrg.AVALUE_49))
            {
                skeyword = skeyword + detailBrg.AVALUE_49 + ",";
            }

            if (skeyword != null)
                skeyword = skeyword.Substring(0, skeyword.Length - 1);


            string sParamJson = "{\"spuInfo\":{\"spuName\":\"" + namafull + "\", " +
                "\"appDescription\":\"" + vDescription + "\", " +
                "\"description\":\"" + vDescription + "\", \"packageInfo\":\"PAKET INFO\", " +
                "\"brandId\":" + detailBrg.AVALUE_38 + ", \"catId\":" + detailBrg.CATEGORY_CODE + ", " + commonAttribute + " \"isSequenceNumber\":1, \"keywords\":\"" + skeyword + "\", \"productArea\":\"" + detailBrg.ACODE_47 + "\", " +
                "\"crossProductType\":\"1\", \"clearanceType\":\"2\" , \"taxesType\":\"2\", \"countryId\":\"10000000\", " +
                paramHref +
                "\"subtitle\":\"" + detailBrg.AVALUE_43 + "\", \"transportId\":42, \"isQuality\":" + detailBrg.AVALUE_47 + ", " +
                paramQualityAsurance +
                "\"warrantyPeriod\":" + detailBrg.ACODE_41 + ", \"afterSale\":" + detailBrg.ACODE_40 + ", \"whetherCod\":" + detailBrg.AVALUE_45 + ", " +
                "\"weight\":\"" + weight + "\", \"netWeight\":\"" + weight + "\", \"packHeight\":\"" + brgInDb.TINGGI + "\", \"packLong\":\"" + brgInDb.PANJANG + "\", \"packWide\":\"" + brgInDb.LEBAR + "\", \"piece\":" + detailBrg.ACODE_39 + "}, " +
                "\"skuList\":[ " +
                paramSKUVariant +
                //"{\"costPrice\":" + detailBrg.HJUAL + ",\"jdPrice\":" + detailBrg.HJUAL + ", \"sellerSkuId\":\"" + detailBrg.BRG + "\", \"skuName\":\"" + namafull + "\", \"stock\":" + qty_stock + ", \"upc\":\"upc\" }" +
                "" +
                "]}";

            //minQuantity,maxQuantity
            //    saleAttrValueAlias

            //add by nurul 28/5/2021
            spuInfoJD.packLong = Convert.ToString(brgInDb.PANJANG);
            spuInfoJD.spuName = namafull;
            spuInfoJD.commonAttributeIds = tempcommonAttributeIds;
            spuInfoJD.keywords = skeyword;
            spuInfoJD.description = vDescription;
            spuInfoJD.countryId = 10000000;
            spuInfoJD.warrantyPeriod = Convert.ToInt32(detailBrg.ACODE_41);
            spuInfoJD.productArea = detailBrg.ACODE_47;
            spuInfoJD.minQuantity = 1;
            spuInfoJD.crossProductType = 1;
            spuInfoJD.packHeight = Convert.ToString(brgInDb.TINGGI);
            spuInfoJD.taxesType = 2;
            spuInfoJD.appDescription = vDescription;
            spuInfoJD.weight = Convert.ToString(weight);
            if (!string.IsNullOrEmpty(tempUrlHref))
            {
                spuInfoJD.subtitleHrefM = tempUrlHref;
            }
            if (!string.IsNullOrEmpty(detailBrg.ANAME_47))
            {
                spuInfoJD.qualityDays = Convert.ToInt32(detailBrg.ANAME_47);
            }
            spuInfoJD.packWide = Convert.ToString(brgInDb.LEBAR);
            spuInfoJD.catId = Convert.ToInt64(detailBrg.CATEGORY_CODE);
            spuInfoJD.whetherCod = Convert.ToInt32(detailBrg.AVALUE_45);
            spuInfoJD.piece = Convert.ToInt32(detailBrg.ACODE_39);
            spuInfoJD.brandId = Convert.ToInt64(detailBrg.AVALUE_38);
            spuInfoJD.subtitle = detailBrg.AVALUE_43;
            spuInfoJD.isQuality = Convert.ToInt32(detailBrg.AVALUE_47);
            spuInfoJD.packageInfo = "PAKET INFO";
            spuInfoJD.afterSale = Convert.ToInt32(detailBrg.ACODE_40);
            spuInfoJD.clearanceType = 2;
            if (!string.IsNullOrEmpty(tempUrlHref))
            {
                spuInfoJD.subtitleHref = tempUrlHref;
            }
            spuInfoJD.maxQuantity = 1000000;


            //end add by nurul 28/5/2021
            CreateProductJDID newData = new CreateProductJDID()
            {
                spuInfo = spuInfoJD,
                skuList = skuListJD
            };
            string myData = JsonConvert.SerializeObject(newData);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Create Product",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = kodeProduk,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            string responseFromServer = "";
            bool responseApi = false;
            int retry = 0;
            while (!responseApi && retry <= 2)
            {
                data = RefreshToken(data);
                var sysParams = new Dictionary<string, string>();
                this.ParamJson = myData;
                sysParams.Add("360buy_param_json", this.ParamJson);

                sysParams.Add("access_token", data.accessToken);
                sysParams.Add("app_key", data.appKey);
                this.Method = "jingdong.seller.product.api.write.addProduct"; //seller create product
                sysParams.Add("method", this.Method);
                var gettimestamp = getCurrentTimeFormatted();
                sysParams.Add("timestamp", gettimestamp);
                sysParams.Add("v", this.Version2);
                sysParams.Add("format", this.Format);
                sysParams.Add("sign_method", this.SignMethod);

                var signature = this.generateSign(sysParams, data.appSecret);

                string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + Uri.EscapeDataString(this.ParamJson) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                urll += "&format=json&sign_method=md5";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                try
                {
                    using (WebResponse responseTest = await myReq.GetResponseAsync())
                    {
                        using (Stream stream = responseTest.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(stream);
                            responseFromServer = reader.ReadToEnd();
                            responseApi = true; break;
                        }
                    }
                }
                //catch (WebException ex)
                //{
                //    string err1 = "";
                //    if (ex.Status == WebExceptionStatus.ProtocolError)
                //    {
                //        WebResponse resp1 = ex.Response;
                //        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                //        {
                //            err1 = sr1.ReadToEnd();
                //        }
                //    }
                //    //throw new Exception(err1);
                //}
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                    {
                        retry = retry + 1;
                        string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        if (retry == 3)
                        {
                            currentLog.REQUEST_EXCEPTION = msg;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            throw new Exception(msg);
                        }
                    }
                    else
                    {
                        retry = 3;
                        string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        currentLog.REQUEST_EXCEPTION = msg;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        throw new Exception(msg);
                    }
                }
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                try
                {
                    var retData = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDCreateProductV2)) as JDIDCreateProductV2;
                    if (retData.jingdong_seller_product_api_write_addProduct_response.returnType != null)
                    {
                        if (retData.jingdong_seller_product_api_write_addProduct_response.returnType.success)
                        {
                            if (retData.jingdong_seller_product_api_write_addProduct_response.returnType.model != null)
                            {
                                if (retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.skuIdList != null)
                                {
                                    if (retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.skuIdList.Count() > 0)
                                    {
                                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);

                                        var dataSkuResult = JD_getSKUVariantbySPUV2(data, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId), kodeProduk);
                                        if (dataSkuResult != null)
                                        {
                                            var brgMPInduk = "";
                                            //var dataSKUOnShelf = JD_setSPUOnShelfV2(data, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId));
                                            //if (dataSKUOnShelf)
                                            //{
                                            string EDBConnID = EDB.GetConnectionString("ConnId");
                                            var sqlStorage = new SqlServerStorage(EDBConnID);

                                            var client = new BackgroundJobClient(sqlStorage);
                                            foreach (var dataSKU in dataSkuResult.model)
                                            {

                                                if (brgInDb.TYPE == "4") // punya variasi
                                                {
                                                    brgMPInduk = Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId) + ";0";

                                                    if (lGambarUploaded.Count() > 0)
                                                    {
                                                        //change by nurul 12/10/2021
                                                        var hitung = 0;
                                                        for (int i = 1; i < 6; i++)
                                                        {
                                                            var urlImageJDID = "";
                                                            switch (i)
                                                            {
                                                                case 1:
                                                                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
                                                                    {
                                                                        hitung++;
                                                                        urlImageJDID = brgInDb.LINK_GAMBAR_1;
                                                                    }
                                                                    break;
                                                                case 2:
                                                                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
                                                                    {
                                                                        hitung++;
                                                                        urlImageJDID = brgInDb.LINK_GAMBAR_2;
                                                                    }
                                                                    break;
                                                                case 3:
                                                                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
                                                                    {
                                                                        hitung++;
                                                                        urlImageJDID = brgInDb.LINK_GAMBAR_3;
                                                                    }
                                                                    break;
                                                                case 4:
                                                                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_4))
                                                                    {
                                                                        hitung++;
                                                                        urlImageJDID = brgInDb.LINK_GAMBAR_4;
                                                                    }
                                                                    break;
                                                                case 5:
                                                                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_5))
                                                                    {
                                                                        hitung++;
                                                                        urlImageJDID = brgInDb.LINK_GAMBAR_5;
                                                                    }
                                                                    break;
                                                            }
                                                            if (!string.IsNullOrEmpty(urlImageJDID))
                                                            {
                                                                if (hitung == 1)
                                                                {
                                                                    await JD_addSKUDetailPictureV2(data, kodeProduk, urlImageJDID, hitung, true, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId));
                                                                }
                                                                else
                                                                {
                                                                    await JD_addSKUDetailPictureV2(data, kodeProduk, urlImageJDID, hitung, false, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId));
                                                                }
                                                            }
                                                        }

                                                        //////JD_addSKUMainPicture(data, Convert.ToString(dataSKU.skuId), brgInDb.LINK_GAMBAR_1);
                                                        ////JD_addSKUDetailPictureV2(data, Convert.ToString(dataSKU.skuId), itemDatas.LINK_GAMBAR_1, 1, true, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId));
                                                        ////JD_addSKUDetailPictureV2(data, Convert.ToString(dataSKU.skuId), itemDatas.LINK_GAMBAR_1, 1, false, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId));
                                                        ////#if (DEBUG || Debug_AWS)
                                                        //await JD_addSKUDetailPictureV2(data, kodeProduk, brgInDb.LINK_GAMBAR_1, 1, true, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId));
                                                        ////#else
                                                        ////                                                                client.Enqueue<JDIDControllerJob>(x => x.JD_addSKUDetailPictureV2(data, kodeProduk, itemDatas.LINK_GAMBAR_1, 1, true, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId)));
                                                        ////#endif


                                                        //if (lGambarUploaded.Count() > 1)
                                                        //{
                                                        //    //for (int i = 1; i < lGambarUploaded.Count(); i++)
                                                        //    for (int i = 1; i < 6; i++)
                                                        //    {
                                                        //        var urlImageJDID = "";
                                                        //        switch (i)
                                                        //        {
                                                        //            case 1:
                                                        //                urlImageJDID = brgInDb.LINK_GAMBAR_2;
                                                        //                break;
                                                        //            case 2:
                                                        //                urlImageJDID = brgInDb.LINK_GAMBAR_3;
                                                        //                break;
                                                        //            case 3:
                                                        //                urlImageJDID = brgInDb.LINK_GAMBAR_4;
                                                        //                break;
                                                        //            case 4:
                                                        //                urlImageJDID = brgInDb.LINK_GAMBAR_5;
                                                        //                break;
                                                        //        }
                                                        //        if (!string.IsNullOrEmpty(urlImageJDID))
                                                        //        {
                                                        //            await JD_addSKUDetailPictureV2(data, kodeProduk, urlImageJDID, i + 1, false, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId));
                                                        //        }
                                                        //    }
                                                        //}
                                                        //change by nurul 12/10/2021
                                                    }

                                                    //handle variasi product
                                                    #region variasi product
                                                    var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == kodeProduk).ToList();
                                                    foreach (var itemDatas in var_stf02)
                                                    {
                                                        if (dataSKU.sellerSkuId == itemDatas.BRG)
                                                        {
                                                            var item = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == itemDatas.BRG && b.IDMARKET == marketplace.RecNum).SingleOrDefault();
                                                            if (item != null)
                                                            {
                                                                item.BRG_MP = Convert.ToString(dataSKU.spuId) + ";" + dataSKU.skuId;
                                                                item.LINK_STATUS = "Buat Produk Berhasil";
                                                                item.LINK_DATETIME = DateTime.UtcNow.AddHours(7);
                                                                item.LINK_ERROR = "0;Buat Produk;;";
                                                                ErasoftDbContext.SaveChanges();
                                                            }

                                                            //List<string> lGambarUploadedVar = new List<string>();

                                                            //if (!string.IsNullOrEmpty(itemDatas.LINK_GAMBAR_1))
                                                            //{
                                                            //    lGambarUploadedVar.Add(itemDatas.LINK_GAMBAR_1);
                                                            //}
                                                            //if (!string.IsNullOrEmpty(itemDatas.LINK_GAMBAR_2))
                                                            //{
                                                            //    lGambarUploadedVar.Add(itemDatas.LINK_GAMBAR_2);
                                                            //}
                                                            //if (!string.IsNullOrEmpty(itemDatas.LINK_GAMBAR_3))
                                                            //{
                                                            //    lGambarUploadedVar.Add(itemDatas.LINK_GAMBAR_3);
                                                            //}
                                                            //if (!string.IsNullOrEmpty(itemDatas.LINK_GAMBAR_4))
                                                            //{
                                                            //    lGambarUploadedVar.Add(itemDatas.LINK_GAMBAR_4);
                                                            //}
                                                            //if (!string.IsNullOrEmpty(itemDatas.LINK_GAMBAR_5))
                                                            //{
                                                            //    lGambarUploadedVar.Add(itemDatas.LINK_GAMBAR_5);
                                                            //}

                                                            //if(lGambarUploadedVar.Count() > 0)
                                                            //{
                                                            //    await JD_addSKUDetailPictureV2(data, itemDatas.BRG, itemDatas.LINK_GAMBAR_1, 1, true, Convert.ToString(dataSKU.spuId));

                                                            //    if (lGambarUploaded.Count() > 1)
                                                            //    {
                                                            //        for (int i = 1; i < lGambarUploaded.Count(); i++)
                                                            //        {
                                                            //            var urlImageJDID = "";
                                                            //            switch (i)
                                                            //            {
                                                            //                case 1:
                                                            //                    urlImageJDID = itemDatas.LINK_GAMBAR_2;
                                                            //                    break;
                                                            //                case 2:
                                                            //                    urlImageJDID = itemDatas.LINK_GAMBAR_3;
                                                            //                    break;
                                                            //                case 3:
                                                            //                    urlImageJDID = itemDatas.LINK_GAMBAR_4;
                                                            //                    break;
                                                            //                case 4:
                                                            //                    urlImageJDID = itemDatas.LINK_GAMBAR_5;
                                                            //                    break;
                                                            //            }
                                                            //            await JD_addSKUDetailPictureV2(data, itemDatas.BRG, urlImageJDID, i + 1, false, Convert.ToString(dataSKU.spuId));

                                                            //        }
                                                            //    }
                                                            //}

                                                        }
                                                    }

                                                    #endregion
                                                    //end handle variasi product
                                                }
                                                else
                                                {
                                                    brgMPInduk = Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId) + ";" + retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.skuIdList[0].skuId.ToString();
                                                    if (lGambarUploaded.Count() > 0)
                                                    {
                                                        //change by nurul 12/10/2021
                                                        var hitung = 0;
                                                        for (int i = 1; i < 6; i++)
                                                        {
                                                            var urlImageJDID = "";
                                                            switch (i)
                                                            {
                                                                case 1:
                                                                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
                                                                    {
                                                                        hitung++;
                                                                        urlImageJDID = brgInDb.LINK_GAMBAR_1;
                                                                    }
                                                                    break;
                                                                case 2:
                                                                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
                                                                    {
                                                                        hitung++;
                                                                        urlImageJDID = brgInDb.LINK_GAMBAR_2;
                                                                    }
                                                                    break;
                                                                case 3:
                                                                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
                                                                    {
                                                                        hitung++;
                                                                        urlImageJDID = brgInDb.LINK_GAMBAR_3;
                                                                    }
                                                                    break;
                                                                case 4:
                                                                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_4))
                                                                    {
                                                                        hitung++;
                                                                        urlImageJDID = brgInDb.LINK_GAMBAR_4;
                                                                    }
                                                                    break;
                                                                case 5:
                                                                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_5))
                                                                    {
                                                                        hitung++;
                                                                        urlImageJDID = brgInDb.LINK_GAMBAR_5;
                                                                    }
                                                                    break;
                                                            }
                                                            if (!string.IsNullOrEmpty(urlImageJDID))
                                                            {
                                                                if (hitung == 1)
                                                                {
                                                                    await JD_addSKUDetailPictureV2(data, kodeProduk, urlImageJDID, hitung, true, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId));
                                                                }
                                                                else
                                                                {
                                                                    await JD_addSKUDetailPictureV2(data, kodeProduk, urlImageJDID, hitung, false, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId));
                                                                }
                                                            }
                                                        }

                                                        //                                                        ////JD_addSKUMainPicture(data, retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.skuIdList[0].skuId.ToString(), brgInDb.LINK_GAMBAR_1);
                                                        //                                                        //JD_addSKUDetailPictureV2(data, retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.skuIdList[0].skuId.ToString(), brgInDb.LINK_GAMBAR_1, 1, true, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId));
                                                        //                                                        //#if (DEBUG || Debug_AWS)
                                                        //                                                        await JD_addSKUDetailPictureV2(data, kodeProduk, brgInDb.LINK_GAMBAR_1, 1, true, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId));
                                                        ////#else
                                                        ////                                                        client.Enqueue<JDIDControllerJob>(x => x.JD_addSKUDetailPictureV2(data, kodeProduk, brgInDb.LINK_GAMBAR_1, 1, true, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId)));
                                                        ////#endif

                                                        //                                                        if (lGambarUploaded.Count() > 1)
                                                        //                                                        {
                                                        //                                                            //for (int i = 1; i < lGambarUploaded.Count(); i++)
                                                        //                                                            for (int i = 1; i < 6; i++)
                                                        //                                                            {
                                                        //                                                                var urlImageJDID = "";
                                                        //                                                                switch (i)
                                                        //                                                                {
                                                        //                                                                    case 1:
                                                        //                                                                        urlImageJDID = brgInDb.LINK_GAMBAR_2;
                                                        //                                                                        break;
                                                        //                                                                    case 2:
                                                        //                                                                        urlImageJDID = brgInDb.LINK_GAMBAR_3;
                                                        //                                                                        break;
                                                        //                                                                    case 3:
                                                        //                                                                        urlImageJDID = brgInDb.LINK_GAMBAR_4;
                                                        //                                                                        break;
                                                        //                                                                    case 4:
                                                        //                                                                        urlImageJDID = brgInDb.LINK_GAMBAR_5;
                                                        //                                                                        break;
                                                        //                                                                }
                                                        //                                                                if (!string.IsNullOrEmpty(urlImageJDID))
                                                        //                                                                {
                                                        //                                                                    await JD_addSKUDetailPictureV2(data, kodeProduk, urlImageJDID, i + 1, false, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId));
                                                        //                                                                }
                                                        //                                                            }
                                                        //                                                        }
                                                        //change by nurul 12/10/2021
                                                    }
                                                }
                                            }
                                            //}

                                            var itemDataInduk = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum).SingleOrDefault();
                                            if (itemDataInduk != null)
                                            {
                                                itemDataInduk.BRG_MP = brgMPInduk;
                                                itemDataInduk.LINK_STATUS = "Buat Produk Berhasil";
                                                itemDataInduk.LINK_DATETIME = DateTime.UtcNow.AddHours(7);
                                                itemDataInduk.LINK_ERROR = "0;Buat Produk;;";
                                                ErasoftDbContext.SaveChanges();
                                            }

                                            //JD_doAuditProductV2(data, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId), kodeProduk);
                                            //#if (DEBUG || Debug_AWS)
                                            await JD_doAuditProductV2(data, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId), kodeProduk);
                                            //#else
                                            //                                            client.Enqueue<JDIDControllerJob>(x => x.JD_doAuditProductV2(data, Convert.ToString(retData.jingdong_seller_product_api_write_addProduct_response.returnType.model.spuId), kodeProduk));
                                            //#endif
                                        }
                                    }

                                }
                            }
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = retData.jingdong_seller_product_api_write_addProduct_response.returnType.message.ToString();
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            throw new Exception(retData.jingdong_seller_product_api_write_addProduct_response.returnType.message.ToString());
                        }
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = "API error. Please contact support.";
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        throw new Exception("API error. Please contact support.");
                    }

                }
                catch (Exception ex2)
                {
                    var msgex2 = ex2.InnerException == null ? ex2.Message.ToString() : ex2.InnerException.Message.ToString();
                    currentLog.REQUEST_EXCEPTION = msgex2;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    throw new Exception(msgex2);
                }
            }
            else
            {
                currentLog.REQUEST_EXCEPTION = "No response API. Please contact support.";
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                throw new Exception("No response API. Please contact support.");
            }

            return "";
        }
        
        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Product {obj} ke JDID Gagal.")]
        public async Task<string> JD_UpdateProductV2(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, JDIDAPIDataJob data)
        {
            SetupContext(data.DatabasePathErasoft, data.username);

            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper()).FirstOrDefault();
            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == log_CUST.ToUpper()).FirstOrDefault();
            if (brgInDb == null || marketplace == null)
                return "invalid passing data";
            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum && b.DISPLAY == true).FirstOrDefault();
            if (detailBrg == null)
                return "invalid passing data";

            var listattributeIDGroup = "";
            var listattributeIDAllVariantGroup = "";

            //add by nurul 28/5/2021
            spuInfo_JDIDCREATE spuInfoJD = new spuInfo_JDIDCREATE() { };
            List<skuList_JDIDCREATE> skuListJD = new List<skuList_JDIDCREATE>() { };
            var tempcommonAttributeIds = "";
            //end add by nurul 28/5/2021

            //Start handle description
            var vDescription = brgInDb.Deskripsi;
            if (!string.IsNullOrEmpty(detailBrg.DESKRIPSI_MP))
            {
                if (detailBrg.DESKRIPSI_MP != "null")
                    vDescription = detailBrg.DESKRIPSI_MP;
            }
            vDescription = new StokControllerJob().RemoveSpecialCharacters(WebUtility.HtmlDecode(WebUtility.HtmlDecode(vDescription)));
            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
            //vDescription = vDescription.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
            vDescription = vDescription.Replace("<li>", "- ").Replace("</li>", "\r\n");
            vDescription = vDescription.Replace("<ul>", "").Replace("</ul>", "\r\n");
            vDescription = vDescription.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
            vDescription = vDescription.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
            vDescription = vDescription.Replace("\r\r", "<br />");
            //vDescription = vDescription.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");
            //end add by nurul 20/1/2020, handle <p> dan enter double di shopee

            //add by calvin 10 september 2019
            vDescription = vDescription.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
            vDescription = vDescription.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
            vDescription = vDescription.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
            vDescription = vDescription.Replace("<p>", "").Replace("</p>", "");
            vDescription = vDescription.Replace("\r", "");
            //HttpBody.description = HttpBody.description.Replace("<li>", "- ").Replace("</li>", "\r\n");
            //HttpBody.description = HttpBody.description.Replace("&nbsp;", "");


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
            double qty_stock = 0;
            //qty_stock = brgInDb.ISI;
            //end handle stock

            var weight = Convert.ToDouble(brgInDb.BERAT / 1000);

            var namafull = "";
            namafull = brgInDb.NAMA;
            if (!string.IsNullOrEmpty(brgInDb.NAMA2))
            {
                namafull += " " + brgInDb.NAMA2;
            }
            if (!string.IsNullOrEmpty(brgInDb.NAMA3))
            {
                namafull += " " + brgInDb.NAMA3;
            }
            if (!string.IsNullOrEmpty(detailBrg.NAMA_BARANG_MP))
            {
                namafull = detailBrg.NAMA_BARANG_MP;
            }

            var commonAttribute = "";

            string sMethod = "epi.ware.openapi.SpuApi.updateSpuInfo";

            var urlHref = detailBrg.AVALUE_44;
            var tempUrlHref = "";
            if (!string.IsNullOrEmpty(urlHref))
            {
                if (!urlHref.Contains("http://"))
                {
                    urlHref = "http://" + urlHref;
                    tempUrlHref = urlHref;
                }
            }
            string[] spuID = detailBrg.BRG_MP.Split(';');

            var paramQualityAsurance = "";
            if (!string.IsNullOrEmpty(detailBrg.ANAME_47))
            {
                paramQualityAsurance = " \"qualityDays\":" + detailBrg.ANAME_47 + ", ";
            }

            //var paramSKUVariant = "";

            try
            {
                if (brgInDb.TYPE != "4")
                {
                    var attributeList = "";
                    for (int i = 1; i <= 30; i++)
                    {
                        string attribute_id = Convert.ToString(detailBrg["ACODE_" + i.ToString()]);
                        string value = Convert.ToString(detailBrg["AVALUE_" + i.ToString()]);
                        if (!string.IsNullOrWhiteSpace(attribute_id) && !string.IsNullOrWhiteSpace(value))
                        {
                            if (value != "null")
                            {
                                //HttpBody.attributes.Add(new ShopeeAttributeClass
                                //{
                                //    attributes_id = Convert.ToInt64(attribute_id),
                                //    value = value.Trim()
                                //});
                                attributeList += attribute_id + ":" + value + ";";
                            }

                        }
                    }
                    tempcommonAttributeIds = attributeList;
                }
            }
            catch (Exception ex)
            {

            }

            if (brgInDb.TYPE == "4") // punya variasi
            {
                //handle variasi product
                #region variasi product
                var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == kodeProduk).ToList();
                var var_strukturVar = ErasoftDbContext.STF02I.Where(p => p.BRG == kodeProduk && p.MARKET == "JDID").ToList().OrderBy(p => p.RECNUM);

                foreach (var itemData in var_stf02)
                {
                    var brgSTF02hCek = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == itemData.BRG.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
                    if (!string.IsNullOrEmpty(brgSTF02hCek.BRG_MP))
                    {
                        #region varian LV1
                        if (!string.IsNullOrEmpty(itemData.Sort8))
                        {
                            var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == itemData.Sort8).FirstOrDefault();
                            if (!listattributeIDAllVariantGroup.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                                listattributeIDAllVariantGroup += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                        }
                        #endregion

                        #region varian LV2
                        if (!string.IsNullOrEmpty(itemData.Sort9))
                        {
                            var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == itemData.Sort9).FirstOrDefault();
                            if (!listattributeIDAllVariantGroup.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                                listattributeIDAllVariantGroup += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                        }
                        #endregion

                        #region varian LV3
                        if (!string.IsNullOrEmpty(itemData.Sort10))
                        {
                            var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 3 && p.KODE_VAR == itemData.Sort10).FirstOrDefault();
                            if (!listattributeIDAllVariantGroup.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                                listattributeIDAllVariantGroup += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                        }
                        #endregion
                    }

                    var namafullVariant = "";
                    namafullVariant = itemData.NAMA;
                    if (!string.IsNullOrEmpty(itemData.NAMA2))
                    {
                        namafullVariant += itemData.NAMA2;
                    }
                    if (!string.IsNullOrEmpty(itemData.NAMA3))
                    {
                        namafullVariant += itemData.NAMA3;
                    }
                }

                if (listattributeIDAllVariantGroup.Length > 0)
                {
                    listattributeIDAllVariantGroup = listattributeIDAllVariantGroup.Substring(0, listattributeIDAllVariantGroup.Length - 1);
                    commonAttribute = "\"commonAttributeIds\":\"" + listattributeIDAllVariantGroup + "\", ";
                    tempcommonAttributeIds = listattributeIDAllVariantGroup;

                }

                #endregion
                //end handle variasi product
            }
            else
            {
                //paramSKUVariant = "{\"costPrice\":" + detailBrg.HJUAL + ",\"jdPrice\":" + detailBrg.HJUAL + ", \"sellerSkuId\":\"" + detailBrg.BRG + "\", \"skuName\":\"" + namafull + "\", \"stock\":" + qty_stock + ", \"upc\":\"upc\" }";
            }

            var skeyword = "";

            if (!string.IsNullOrEmpty(detailBrg.AVALUE_46))
            {
                skeyword = detailBrg.AVALUE_46 + ",";
            }
            if (!string.IsNullOrEmpty(detailBrg.AVALUE_48))
            {
                skeyword = skeyword + detailBrg.AVALUE_48 + ",";
            }
            if (!string.IsNullOrEmpty(detailBrg.AVALUE_49))
            {
                skeyword = skeyword + detailBrg.AVALUE_49 + ",";
            }

            if (skeyword != null)
                skeyword = skeyword.Substring(0, skeyword.Length - 1);

            string sParamJson = "{\"spuInfo\":{\"spuName\":\"" + namafull + "\", \"spuId\":" + spuID[0] + ", " +
                "\"packageInfo\":\"PAKET INFO\", " +
                "\"brandId\":" + detailBrg.AVALUE_38 + ", \"catId\":" + detailBrg.CATEGORY_CODE + ", " + commonAttribute + " \"isSequenceNumber\":1, \"keywords\":\"" + skeyword + "\", \"productArea\":\"" + detailBrg.ACODE_47 + "\", " +
                "\"crossProductType\":\"1\", \"clearanceType\":\"2\" , \"taxesType\":\"2\", \"countryId\":\"10000000\", " +
                "\"subtitle\":\"" + detailBrg.AVALUE_43 + "\", \"subtitleHref\":\"" + urlHref + "\", \"subtitleHrefM\":\"" + urlHref + "\", \"transportId\":42, \"isQuality\":" + detailBrg.AVALUE_47 + ", " +
                paramQualityAsurance +
                "\"warrantyPeriod\":" + detailBrg.ACODE_41 +
                ", \"afterSale\":" + Convert.ToInt32(detailBrg.ACODE_40) +
                ", \"whetherCod\":" + detailBrg.AVALUE_45 + ", " +
                "\"weight\":\"" + weight + "\",  \"Piece\": " + detailBrg.ACODE_39 + ", \"netWeight\":\"" + weight + "\", \"packHeight\":\"" + brgInDb.TINGGI + "\", \"packLong\":\"" + brgInDb.PANJANG + "\", \"packWide\":\"" + brgInDb.LEBAR + "\"," +
                "\"appDescription\":\"" + vDescription + "\"" +
                ", \"description\":\"" + vDescription + "\"" +
                "}}";
            //"\"skuList\":[ " +
            //paramSKUVariant +
            ////"{\"costPrice\":" + detailBrg.HJUAL + ", \"jdPrice\":" + detailBrg.HJUAL + ", \"sellerSkuId\":\"" + detailBrg.BRG + "\", \"skuName\":\"" + namafull + "\", \"stock\":" + qty_stock + ", \"upc\":\"upc\" }" +
            //"]" +

            //description,minQuantity,packageInfo,afterSale,shopCategoryIds,maxQuantity

            //add by nurul 28/5/2021
            UpdateProductJDID newData = new UpdateProductJDID()
            {
                packLong = Convert.ToString(brgInDb.PANJANG),
                spuName = namafull,
                commonAttributeIds = tempcommonAttributeIds,
                keywords = skeyword,
                description = vDescription,
                countryId = 10000000,
                warrantyPeriod = Convert.ToInt32(detailBrg.ACODE_41),
                productArea = detailBrg.ACODE_47,
                minQuantity = 1,
                crossProductType = 1,
                packHeight = Convert.ToString(brgInDb.TINGGI),
                taxesType = 2,
                appDescription = vDescription,
                weight = Convert.ToString(weight),
                //subtitleHrefM
                //qualityDays
                packWide = Convert.ToString(brgInDb.LEBAR),
                catId = Convert.ToInt64(detailBrg.CATEGORY_CODE),
                whetherCod = Convert.ToInt32(detailBrg.AVALUE_45),
                piece = Convert.ToInt32(detailBrg.ACODE_39),
                brandId = Convert.ToInt64(detailBrg.AVALUE_38),
                subtitle = detailBrg.AVALUE_43,
                isQuality = Convert.ToInt32(detailBrg.AVALUE_47),
                packageInfo = "PAKET INFO",
                afterSale = Convert.ToInt32(detailBrg.ACODE_40),
                clearanceType = 2,
                //subtitleHref
                maxQuantity = 1000000,
                spuId = Convert.ToInt64(spuID[0]),

            };
            if (!string.IsNullOrEmpty(tempUrlHref))
            {
                newData.subtitleHrefM = tempUrlHref;
            }
            if (!string.IsNullOrEmpty(detailBrg.ANAME_47))
            {
                newData.qualityDays = Convert.ToInt32(detailBrg.ANAME_47);
            }
            if (!string.IsNullOrEmpty(tempUrlHref))
            {
                newData.subtitleHref = tempUrlHref;
            }

            string myData = JsonConvert.SerializeObject(newData);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Product",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = kodeProduk,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            string responseFromServer = "";
            bool responseApi = false;
            int retry = 0;
            while (!responseApi && retry <= 2)
            {
                data = RefreshToken(data);
                var sysParams = new Dictionary<string, string>();
                this.ParamJson = myData;
                sysParams.Add("360buy_param_json", this.ParamJson);

                sysParams.Add("access_token", data.accessToken);
                sysParams.Add("app_key", data.appKey);
                this.Method = "jingdong.seller.product.api.write.updateProduct"; //update product informtaion ,only include SPU level information, this API only for POP sellers
                sysParams.Add("method", this.Method);
                var gettimestamp = getCurrentTimeFormatted();
                sysParams.Add("timestamp", gettimestamp);
                sysParams.Add("v", this.Version2);
                sysParams.Add("format", this.Format);
                sysParams.Add("sign_method", this.SignMethod);

                var signature = this.generateSign(sysParams, data.appSecret);

                string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + Uri.EscapeDataString(this.ParamJson) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                urll += "&format=json&sign_method=md5";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                try
                {
                    using (WebResponse responseTest = await myReq.GetResponseAsync())
                    {
                        using (Stream stream = responseTest.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(stream);
                            responseFromServer = reader.ReadToEnd();
                            responseApi = true; break;
                        }
                    }
                }
                //catch (WebException ex)
                //{
                //    string err1 = "";
                //    if (ex.Status == WebExceptionStatus.ProtocolError)
                //    {
                //        WebResponse resp1 = ex.Response;
                //        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                //        {
                //            err1 = sr1.ReadToEnd();
                //        }
                //    }
                //    //throw new Exception(err1);
                //}
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                    {
                        retry = retry + 1;
                        string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        if (retry == 3)
                        {
                            currentLog.REQUEST_EXCEPTION = msg;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            throw new Exception(msg);
                        }
                    }
                    else
                    {
                        retry = 3;
                        string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        currentLog.REQUEST_EXCEPTION = msg;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        throw new Exception(msg);
                    }
                }
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                try
                {
                    var retData = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDUpdateProductV2)) as JDIDUpdateProductV2;
                    if (retData.jingdong_seller_product_api_write_updateProduct_response.returnType != null)
                    {
                        if (retData.jingdong_seller_product_api_write_updateProduct_response.returnType.success)
                        {
                            if (retData.jingdong_seller_product_api_write_updateProduct_response.returnType.model)
                            {
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);

                                var dataSkuResult = JD_getSKUVariantbySPUV2(data, spuID[0], kodeProduk);

                                if (dataSkuResult != null)
                                {
                                    var urutanGambar = 0;
                                    var listattributeIDAllVariantGroupCreate = "";
                                    var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == kodeProduk).ToList();
                                    var var_strukturVar = ErasoftDbContext.STF02I.Where(p => p.BRG == kodeProduk && p.MARKET == "JDID").ToList().OrderBy(p => p.RECNUM);

                                    //var dataSKUOnShelf = JD_setSPUOnShelfV2(data, spuID[0]);
                                    //if (dataSKUOnShelf)
                                    //{
                                    string EDBConnID = EDB.GetConnectionString("ConnId");
                                    var sqlStorage = new SqlServerStorage(EDBConnID);

                                    var client = new BackgroundJobClient(sqlStorage);

                                    if (brgInDb.TYPE == "4") // punya variasi
                                    {
                                        foreach (var dataVar in var_stf02)
                                        {
                                            if (brgInDb.TYPE == "4") // punya variasi
                                            {
                                                var brgSTF02h = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == dataVar.BRG.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
                                                if (!string.IsNullOrEmpty(brgSTF02h.BRG_MP))
                                                {
                                                    if (dataSkuResult.model.Count() > 0)
                                                    {
                                                        foreach (var dataSKU in dataSkuResult.model)
                                                        {
                                                            if (dataSKU.sellerSkuId == dataVar.BRG)
                                                            {
                                                                urutanGambar = urutanGambar + 1;
                                                                var namafullVariant = "";
                                                                namafullVariant = dataVar.NAMA;
                                                                if (!string.IsNullOrEmpty(dataVar.NAMA2))
                                                                {
                                                                    namafullVariant += dataVar.NAMA2;
                                                                }
                                                                if (!string.IsNullOrEmpty(dataVar.NAMA3))
                                                                {
                                                                    namafullVariant += dataVar.NAMA3;
                                                                }

                                                                //await JD_updateSKUV2(data, namafullVariant, dataVar.BRG, brgSTF02h.HJUAL.ToString(), brgSTF02h.HJUAL.ToString(), dataSKU.skuId.ToString(), spuID[0].ToString());
                                                                //#if (DEBUG || Debug_AWS)
                                                                await JD_updateSKUV2(data, namafullVariant, dataVar.BRG, brgSTF02h.HJUAL.ToString(), brgSTF02h.HJUAL.ToString(), dataSKU.skuId.ToString(), spuID[0].ToString());
                                                                //#else
                                                                //                                                                client.Enqueue<JDIDControllerJob>(x => x.JD_updateSKUV2(data, namafullVariant, dataVar.BRG, brgSTF02h.HJUAL.ToString(), brgSTF02h.HJUAL.ToString(), dataSKU.skuId.ToString(), spuID[0].ToString()));
                                                                //#endif


                                                                if (lGambarUploaded.Count() > 0)
                                                                {
                                                                    //change by nurul 12/10/2021
                                                                    var hitung = 0;
                                                                    for (int i = 1; i < 6; i++)
                                                                    {
                                                                        var urlImageJDID = "";
                                                                        switch (i)
                                                                        {
                                                                            case 1:
                                                                                if (!string.IsNullOrEmpty(dataVar.LINK_GAMBAR_1))
                                                                                {
                                                                                    hitung++;
                                                                                    urlImageJDID = dataVar.LINK_GAMBAR_1;
                                                                                }
                                                                                break;
                                                                            case 2:
                                                                                if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
                                                                                {
                                                                                    hitung++;
                                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_2;
                                                                                }
                                                                                break;
                                                                            case 3:
                                                                                if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
                                                                                {
                                                                                    hitung++;
                                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_3;
                                                                                }
                                                                                break;
                                                                            case 4:
                                                                                if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_4))
                                                                                {
                                                                                    hitung++;
                                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_4;
                                                                                }
                                                                                break;
                                                                            case 5:
                                                                                if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_5))
                                                                                {
                                                                                    hitung++;
                                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_5;
                                                                                }
                                                                                break;
                                                                        }
                                                                        if (!string.IsNullOrEmpty(urlImageJDID))
                                                                        {
                                                                            if (hitung == 1)
                                                                            {
                                                                                await JD_addSKUDetailPictureV2(data, kodeProduk, urlImageJDID, hitung, true, spuID[0].ToString());
                                                                            }
                                                                            else
                                                                            {
                                                                                await JD_addSKUDetailPictureV2(data, kodeProduk, urlImageJDID, hitung, false, spuID[0].ToString());
                                                                            }
                                                                        }
                                                                    }

                                                                    //                                                                    if (!string.IsNullOrEmpty(dataVar.LINK_GAMBAR_1))
                                                                    //                                                                    {
                                                                    //                                                                        ////await JD_addSKUMainPicture(data, Convert.ToString(dataSKU.skuId), dataVar.LINK_GAMBAR_1);
                                                                    //                                                                        //await JD_addSKUDetailPictureV2(data, kodeProduk, dataVar.LINK_GAMBAR_1, urutanGambar, true, spuID[0].ToString());
                                                                    //                                                                        //await JD_addSKUDetailPictureV2(data, kodeProduk, dataVar.LINK_GAMBAR_1, urutanGambar, false, spuID[0].ToString());
                                                                    ////#if (DEBUG || Debug_AWS)
                                                                    //                                                                        await JD_addSKUDetailPictureV2(data, kodeProduk, dataVar.LINK_GAMBAR_1, urutanGambar, true, spuID[0].ToString());
                                                                    ////#else
                                                                    ////                                                                client.Enqueue<JDIDControllerJob>(x => x.JD_addSKUDetailPictureV2(data, kodeProduk, dataVar.LINK_GAMBAR_1, urutanGambar, true, spuID[0].ToString()));
                                                                    ////#endif
                                                                    //                                                                    }


                                                                    //                                                                    if (lGambarUploaded.Count() > 1)
                                                                    //                                                                    {
                                                                    //                                                                        //for (int i = 1; i < lGambarUploaded.Count(); i++)
                                                                    //                                                                        for (int i = 1; i < 6; i++)
                                                                    //                                                                        {
                                                                    //                                                                            var urlImageJDID = "";
                                                                    //                                                                            switch (i)
                                                                    //                                                                            {
                                                                    //                                                                                case 1:
                                                                    //                                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_2;
                                                                    //                                                                                    break;
                                                                    //                                                                                case 2:
                                                                    //                                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_3;
                                                                    //                                                                                    break;
                                                                    //                                                                                case 3:
                                                                    //                                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_4;
                                                                    //                                                                                    break;
                                                                    //                                                                                case 4:
                                                                    //                                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_5;
                                                                    //                                                                                    break;
                                                                    //                                                                            }
                                                                    //                                                                            if (!string.IsNullOrEmpty(urlImageJDID))
                                                                    //                                                                            {
                                                                    //                                                                                await JD_addSKUDetailPictureV2(data, kodeProduk, urlImageJDID, i + 1, false, spuID[0].ToString());
                                                                    //                                                                            }
                                                                    //                                                                        }
                                                                    //                                                                    }
                                                                    //change by nurul 12/10/2021
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {

                                                    #region varian LV1
                                                    if (!string.IsNullOrEmpty(dataVar.Sort8))
                                                    {
                                                        var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == dataVar.Sort8).FirstOrDefault();
                                                        if (!listattributeIDAllVariantGroupCreate.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                                                            listattributeIDAllVariantGroupCreate += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                                                    }
                                                    #endregion

                                                    #region varian LV2
                                                    if (!string.IsNullOrEmpty(dataVar.Sort9))
                                                    {
                                                        var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == dataVar.Sort9).FirstOrDefault();
                                                        if (!listattributeIDAllVariantGroupCreate.Contains(variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR))
                                                            listattributeIDAllVariantGroupCreate += variant_id_group.MP_JUDUL_VAR + ":" + variant_id_group.MP_VALUE_VAR + ";";
                                                    }
                                                    #endregion

                                                    if (listattributeIDAllVariantGroupCreate.Length > 0)
                                                    {
                                                        listattributeIDAllVariantGroupCreate = listattributeIDAllVariantGroupCreate.Substring(0, listattributeIDAllVariantGroupCreate.Length - 1);
                                                    }

                                                    var namafullVariant = "";
                                                    namafullVariant = dataVar.NAMA;
                                                    if (!string.IsNullOrEmpty(dataVar.NAMA2))
                                                    {
                                                        namafullVariant += dataVar.NAMA2;
                                                    }
                                                    if (!string.IsNullOrEmpty(dataVar.NAMA3))
                                                    {
                                                        namafullVariant += dataVar.NAMA3;
                                                    }

                                                    //DataAddSKUVariant dataSKUVar = new DataAddSKUVariant();
                                                    //dataSKUVar.sellerSkuId = dataVar.BRG;
                                                    //dataSKUVar.skuName = namafullVariant;
                                                    //dataSKUVar.saleAttributeIds = listattributeIDAllVariantGroupCreate;
                                                    //dataSKUVar.stock = 0;
                                                    //dataSKUVar.weight = Convert.ToString(weight);
                                                    //dataSKUVar.piece = Convert.ToInt32(detailBrg.ACODE_41);
                                                    //dataSKUVar.packWide = Convert.ToString(brgInDb.LEBAR);
                                                    //dataSKUVar.packLong = Convert.ToString(brgInDb.PANJANG);
                                                    //dataSKUVar.packHeight = Convert.ToString(brgInDb.TINGGI);
                                                    //dataSKUVar.netWeight = Convert.ToString(weight);
                                                    //dataSKUVar.jdPrice = Convert.ToInt64(brgSTF02h.HJUAL);
                                                    //dataSKUVar.costPrice = Convert.ToInt64(brgSTF02h.HJUAL);

                                                    addSKUVariantJDID dataSKUVar = new addSKUVariantJDID();
                                                    dataSKUVar.spuId = Convert.ToInt32(spuID[0]);
                                                    List<string> _packlong = new List<string>();
                                                    _packlong.Add(Convert.ToString(brgInDb.PANJANG));
                                                    dataSKUVar.packLong = _packlong;
                                                    List<string> _saleAttributeIds = new List<string>();
                                                    _saleAttributeIds.Add(listattributeIDAllVariantGroupCreate);
                                                    dataSKUVar.saleAttributeIds = _saleAttributeIds;
                                                    List<Int32> _costPrice = new List<Int32>();
                                                    _costPrice.Add(Convert.ToInt32(brgSTF02h.HJUAL));
                                                    dataSKUVar.costPrice = _costPrice.ToArray();
                                                    //dataSKUVar.upc.Add("upc");
                                                    List<string> _upc = new List<string>();
                                                    _upc.Add("upc");
                                                    dataSKUVar.upc = _upc;
                                                    List<string> _weight = new List<string>();
                                                    _weight.Add(Convert.ToString(weight));
                                                    dataSKUVar.weight = _weight;
                                                    List<string> _sellerSkuId = new List<string>();
                                                    var ax = brgSTF02h.BRG.ToString();
                                                    _sellerSkuId.Add(ax);
                                                    dataSKUVar.sellerSkuId = _sellerSkuId;
                                                    List<string> _saleAttrValueAlias = new List<string>();
                                                    _saleAttrValueAlias.Add(listattributeIDAllVariantGroupCreate);
                                                    dataSKUVar.saleAttrValueAlias = _saleAttrValueAlias;
                                                    List<string> _skuName = new List<string>();
                                                    _skuName.Add(Convert.ToString(namafullVariant));
                                                    dataSKUVar.skuName = _skuName;
                                                    List<string> _packWide = new List<string>();
                                                    _packWide.Add(Convert.ToString(brgInDb.LEBAR));
                                                    dataSKUVar.packWide = _packWide;
                                                    List<int> _piece = new List<int>();
                                                    _piece.Add(Convert.ToInt32(detailBrg.ACODE_41));
                                                    dataSKUVar.piece = _piece.ToArray();
                                                    List<Int32> _jdPrice = new List<Int32>();
                                                    _jdPrice.Add(Convert.ToInt32(brgSTF02h.HJUAL));
                                                    dataSKUVar.jdPrice = _jdPrice.ToArray();
                                                    List<string> _packHeight = new List<string>();
                                                    _packHeight.Add(Convert.ToString(brgInDb.TINGGI));
                                                    dataSKUVar.packHeight = _packHeight;

                                                    List<Int32> _stock = new List<Int32>();
                                                    _stock.Add(0);
                                                    dataSKUVar.stock = _stock.ToArray();
                                                    //dataSKUVar.stock.Add(0);

                                                    //await JD_addSKUVariantV2(data, dataSKUVar, dataSkuResult.model[0].spuId.ToString(), dataVar.BRG, dataVar.LINK_GAMBAR_1, marketplace.RecNum);
                                                    //#if (DEBUG || Debug_AWS)
                                                    await JD_addSKUVariantV2(data, dataSKUVar, dataSkuResult.model[0].spuId.ToString(), dataVar.BRG, dataVar.LINK_GAMBAR_1, marketplace.RecNum);
                                                    //#else
                                                    //                                                client.Enqueue<JDIDControllerJob>(x => x.JD_addSKUVariantV2(data, dataSKUVar, dataSkuResult.model[0].spuId.ToString(), dataVar.BRG, dataVar.LINK_GAMBAR_1, marketplace.RecNum));
                                                    //#endif

                                                    listattributeIDAllVariantGroupCreate = "";
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (dataSkuResult.model.Count() > 0)
                                        {
                                            foreach (var dataSKU in dataSkuResult.model)
                                            {
                                                var namafullVariant = "";
                                                namafullVariant = brgInDb.NAMA;
                                                if (!string.IsNullOrEmpty(brgInDb.NAMA2))
                                                {
                                                    namafullVariant += brgInDb.NAMA2;
                                                }
                                                if (!string.IsNullOrEmpty(brgInDb.NAMA3))
                                                {
                                                    namafullVariant += brgInDb.NAMA3;
                                                }

                                                await JD_updateSKUV2(data, namafullVariant, detailBrg.BRG, detailBrg.HJUAL.ToString(), detailBrg.HJUAL.ToString(), dataSKU.skuId.ToString(), spuID[0].ToString());


                                                if (lGambarUploaded.Count() > 0)
                                                {
                                                    //change by nurul 12/10/2021
                                                    var hitung = 0;
                                                    for (int i = 1; i < 6; i++)
                                                    {
                                                        var urlImageJDID = "";
                                                        switch (i)
                                                        {
                                                            case 1:
                                                                if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
                                                                {
                                                                    hitung++;
                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_1;
                                                                }
                                                                break;
                                                            case 2:
                                                                if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
                                                                {
                                                                    hitung++;
                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_2;
                                                                }
                                                                break;
                                                            case 3:
                                                                if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
                                                                {
                                                                    hitung++;
                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_3;
                                                                }
                                                                break;
                                                            case 4:
                                                                if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_4))
                                                                {
                                                                    hitung++;
                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_4;
                                                                }
                                                                break;
                                                            case 5:
                                                                if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_5))
                                                                {
                                                                    hitung++;
                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_5;
                                                                }
                                                                break;
                                                        }
                                                        if (!string.IsNullOrEmpty(urlImageJDID))
                                                        {
                                                            if (hitung == 1)
                                                            {
                                                                await JD_addSKUDetailPictureV2(data, kodeProduk, urlImageJDID, hitung, true, spuID[0].ToString());
                                                            }
                                                            else
                                                            {
                                                                await JD_addSKUDetailPictureV2(data, kodeProduk, urlImageJDID, hitung, false, spuID[0].ToString());
                                                            }
                                                        }
                                                    }
                                                    //                                                    ////await JD_addSKUMainPicture(data, dataSKU.skuId.ToString(), brgInDb.LINK_GAMBAR_1);
                                                    //                                                    //await JD_addSKUDetailPictureV2(data, kodeProduk, brgInDb.LINK_GAMBAR_1, urutanGambar, true, spuID[0].ToString());
                                                    //                                                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
                                                    //                                                    {
                                                    ////#if (DEBUG || Debug_AWS)
                                                    //                                                        await JD_addSKUDetailPictureV2(data, kodeProduk, brgInDb.LINK_GAMBAR_1, 1, true, spuID[0].ToString());
                                                    ////#else
                                                    ////                                                                client.Enqueue<JDIDControllerJob>(x => x.JD_addSKUDetailPictureV2(data, kodeProduk, brgInDb.LINK_GAMBAR_1, 1, true, spuID[0].ToString()));
                                                    ////#endif
                                                    //                                                    }


                                                    //                                                    if (lGambarUploaded.Count() > 1)
                                                    //                                                    {
                                                    //                                                        //for (int i = 1; i < lGambarUploaded.Count(); i++)
                                                    //                                                        for (int i = 1; i < 6; i++)
                                                    //                                                        {
                                                    //                                                            var urlImageJDID = "";
                                                    //                                                            switch (i)
                                                    //                                                            {
                                                    //                                                                case 1:
                                                    //                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_2;
                                                    //                                                                    break;
                                                    //                                                                case 2:
                                                    //                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_3;
                                                    //                                                                    break;
                                                    //                                                                case 3:
                                                    //                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_4;
                                                    //                                                                    break;
                                                    //                                                                case 4:
                                                    //                                                                    urlImageJDID = brgInDb.LINK_GAMBAR_5;
                                                    //                                                                    break;
                                                    //                                                            }
                                                    //                                                            if (!string.IsNullOrEmpty(urlImageJDID))
                                                    //                                                            {
                                                    //                                                                await JD_addSKUDetailPictureV2(data, kodeProduk, urlImageJDID, i + 1, false, spuID[0].ToString());
                                                    //                                                            }
                                                    //                                                        }
                                                    //                                                    }
                                                    //change by nurul 12/10/2021                                       
                                                }
                                            }
                                        }
                                    }
                                    //}

                                    //change by nurul 12/10/2021
                                    ////JD_doAuditProductV2(data, Convert.ToString(dataSkuResult.model[0].spuId), kodeProduk);
                                    ////#if (DEBUG || Debug_AWS)

                                    //await JD_doAuditProductV2(data, Convert.ToString(dataSkuResult.model[0].spuId), kodeProduk);
                                    ////#else
                                    ////                                    client.Enqueue<JDIDControllerJob>(x => x.JD_doAuditProductV2(data, Convert.ToString(dataSkuResult.model[0].spuId), kodeProduk));
                                    ////#endif

                                    var dataSPUInfoResult = JD_getSPUInfoV2(data, Convert.ToString(dataSkuResult.model[0].spuId), kodeProduk);
                                    if (dataSPUInfoResult.model.Count() > 0)
                                    {
                                        if (dataSPUInfoResult.model.FirstOrDefault().auditStatus != 4) //bukan Modification Under Audit
                                        {
                                            await JD_doAuditProductV2(data, Convert.ToString(dataSkuResult.model[0].spuId), kodeProduk);
                                        }
                                    }
                                    else
                                    {
                                        await JD_doAuditProductV2(data, Convert.ToString(dataSkuResult.model[0].spuId), kodeProduk);
                                    }
                                    //end change by nurul 12/10/2021
                                }
                            }
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = retData.jingdong_seller_product_api_write_updateProduct_response.returnType.message.ToString();
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            throw new Exception(retData.jingdong_seller_product_api_write_updateProduct_response.returnType.message.ToString());
                        }
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = "API error. Please contact support.";
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        throw new Exception("API error. Please contact support.");
                    }

                }
                catch (Exception ex2)
                {
                    var msg = ex2.InnerException == null ? ex2.Message.ToString() : ex2.InnerException.Message.ToString();
                    currentLog.REQUEST_EXCEPTION = msg;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    throw new Exception(ex2.InnerException == null ? ex2.Message.ToString() : ex2.InnerException.Message.ToString());
                }
            }
            else
            {
                currentLog.REQUEST_EXCEPTION = "No response API. Please contact support.";
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                throw new Exception("No response API. Please contact support.");
            }

            return "";
        }

        public ReturntypeGetSPUInfoV2 JD_getSPUInfoV2(JDIDAPIDataJob data, string sSPUID, string kdbrg)
        {
            ReturntypeGetSPUInfoV2 datasku = new ReturntypeGetSPUInfoV2();
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Create Product",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = kdbrg,
                REQUEST_ATTRIBUTE_2 = sSPUID,
                REQUEST_ATTRIBUTE_3 = "JD_getSKUVariantbySPUV2",
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            try
            {
                //string[] spuID = sSPUID.Split(';');
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 2)
                {
                    data = RefreshToken(data);
                    var sysParams = new Dictionary<string, string>();
                    this.ParamJson = "{\"spuId\":\"" + sSPUID + "\",\"spuDescription\":\"1\",\"spuImgs\":\"1\",\"brandInfo\":\"1\",\"skuIds\":\"1\"}";
                    sysParams.Add("360buy_param_json", this.ParamJson);

                    sysParams.Add("access_token", data.accessToken);
                    sysParams.Add("app_key", data.appKey);
                    this.Method = "jingdong.seller.product.getWareBySpuIds"; //query product information via productId list
                    sysParams.Add("method", this.Method);
                    var gettimestamp = getCurrentTimeFormatted();
                    sysParams.Add("timestamp", gettimestamp);
                    sysParams.Add("v", this.Version2);
                    sysParams.Add("format", this.Format);
                    sysParams.Add("sign_method", this.SignMethod);

                    var signature = this.generateSign(sysParams, data.appSecret);

                    string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + Uri.EscapeDataString(this.ParamJson) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                    urll += "&format=json&sign_method=md5";
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    try
                    {
                        using (WebResponse response = myReq.GetResponse())
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream);
                                responseFromServer = reader.ReadToEnd();
                                responseApi = true; break;
                            }
                        }
                    }
                    //catch (WebException ex)
                    //{
                    //    string err1 = "";
                    //    if (ex.Status == WebExceptionStatus.ProtocolError)
                    //    {
                    //        WebResponse resp1 = ex.Response;
                    //        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                    //        {
                    //            err1 = sr1.ReadToEnd();
                    //        }
                    //    }
                    //    //throw new Exception(err1);
                    //}
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                        {
                            retry = retry + 1;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            if (retry == 3)
                            {
                                currentLog.REQUEST_EXCEPTION = msg;
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                throw new Exception(msg);
                            }
                        }
                        else
                        {
                            retry = 3;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            currentLog.REQUEST_EXCEPTION = msg;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            throw new Exception(msg);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    try
                    {
                        var ret = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDGetSPUInfoV2)) as JDIDGetSPUInfoV2;
                        if (ret.jingdong_seller_product_getWareBySpuIds_response.returnType != null)
                        {
                            if (ret.jingdong_seller_product_getWareBySpuIds_response.returnType.success)
                            {
                                if (ret.jingdong_seller_product_getWareBySpuIds_response.returnType.model != null)
                                {
                                    datasku = ret.jingdong_seller_product_getWareBySpuIds_response.returnType;
                                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);
                                }
                            }
                            else
                            {
                                currentLog.REQUEST_EXCEPTION = ret.jingdong_seller_product_getWareBySpuIds_response.returnType.message.ToString();
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                throw new Exception(ret.jingdong_seller_product_getWareBySpuIds_response.returnType.message.ToString());
                            }
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = "API error. Please contact support.";
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            throw new Exception("API error. Please contact support.");
                        }

                    }
                    catch (Exception ex2)
                    {
                        string msg = ex2.InnerException != null ? ex2.InnerException.Message : ex2.Message;
                        currentLog.REQUEST_EXCEPTION = msg;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        throw new Exception(msg);
                    }
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = "Tidak ada respon dari API.";
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                currentLog.REQUEST_EXCEPTION = msg;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                throw new Exception(msg);
            }

            return datasku;
        }

        public ReturntypeGetSKUVariantBySPUV2 JD_getSKUVariantbySPUV2(JDIDAPIDataJob data, string sSPUID, string kdbrg)
        {
            ReturntypeGetSKUVariantBySPUV2 datasku = new ReturntypeGetSKUVariantBySPUV2();
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Create Product",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = kdbrg,
                REQUEST_ATTRIBUTE_2 = sSPUID,
                REQUEST_ATTRIBUTE_3 = "JD_getSKUVariantbySPUV2",
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            try
            {
                //string[] spuID = sSPUID.Split(';');
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 2)
                {
                    data = RefreshToken(data);
                    var sysParams = new Dictionary<string, string>();
                    this.ParamJson = "{\"spuId\":\"" + sSPUID + "\"}";
                    sysParams.Add("360buy_param_json", this.ParamJson);

                    sysParams.Add("access_token", data.accessToken);
                    sysParams.Add("app_key", data.appKey);
                    this.Method = "jingdong.seller.product.getSkuInfoBySpuIdAndVenderId"; //this API is for query sku information via spuId
                    sysParams.Add("method", this.Method);
                    var gettimestamp = getCurrentTimeFormatted();
                    sysParams.Add("timestamp", gettimestamp);
                    sysParams.Add("v", this.Version2);
                    sysParams.Add("format", this.Format);
                    sysParams.Add("sign_method", this.SignMethod);

                    var signature = this.generateSign(sysParams, data.appSecret);

                    string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + Uri.EscapeDataString(this.ParamJson) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                    urll += "&format=json&sign_method=md5";
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    try
                    {
                        using (WebResponse response = myReq.GetResponse())
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream);
                                responseFromServer = reader.ReadToEnd();
                                responseApi = true; break;
                            }
                        }
                    }
                    //catch (WebException ex)
                    //{
                    //    string err1 = "";
                    //    if (ex.Status == WebExceptionStatus.ProtocolError)
                    //    {
                    //        WebResponse resp1 = ex.Response;
                    //        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                    //        {
                    //            err1 = sr1.ReadToEnd();
                    //        }
                    //    }
                    //    //throw new Exception(err1);
                    //}
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                        {
                            retry = retry + 1;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            if (retry == 3)
                            {
                                currentLog.REQUEST_EXCEPTION = msg;
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                throw new Exception(msg);
                            }
                        }
                        else
                        {
                            retry = 3;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            currentLog.REQUEST_EXCEPTION = msg;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            throw new Exception(msg);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    try
                    {
                        var ret = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDGetSKUVariantBySPUV2)) as JDIDGetSKUVariantBySPUV2;
                        if (ret.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType != null)
                        {
                            if (ret.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType.success)
                            {
                                if (ret.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType.model != null)
                                {
                                    datasku = ret.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType;
                                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);
                                }
                            }
                            else
                            {
                                currentLog.REQUEST_EXCEPTION = ret.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType.message.ToString();
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                throw new Exception(ret.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType.message.ToString());
                            }
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = "API error. Please contact support.";
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            throw new Exception("API error. Please contact support.");
                        }

                    }
                    catch (Exception ex2)
                    {
                        string msg = ex2.InnerException != null ? ex2.InnerException.Message : ex2.Message;
                        currentLog.REQUEST_EXCEPTION = msg;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        throw new Exception(msg);
                    }
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = "Tidak ada respon dari API.";
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                currentLog.REQUEST_EXCEPTION = msg;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                throw new Exception(msg);
            }

            return datasku;
        }

        public bool JD_setSPUOnShelfV2(JDIDAPIDataJob data, string sSPUID)
        {
            //ReturntypeGetSKUVariantBySPUV2 datasku = new ReturntypeGetSKUVariantBySPUV2();
            var rett = false;
            try
            {
                //string[] spuID = sSPUID.Split(';');
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 2)
                {
                    data = RefreshToken(data);
                    var sysParams = new Dictionary<string, string>();
                    this.ParamJson = "{\"spuId\":\"" + sSPUID + "\"}";
                    sysParams.Add("360buy_param_json", this.ParamJson);

                    sysParams.Add("access_token", data.accessToken);
                    sysParams.Add("app_key", data.appKey);
                    this.Method = "jingdong.seller.product.api.write.onShelf"; //POP product on Shelf
                    sysParams.Add("method", this.Method);
                    var gettimestamp = getCurrentTimeFormatted();
                    sysParams.Add("timestamp", gettimestamp);
                    sysParams.Add("v", this.Version2);
                    sysParams.Add("format", this.Format);
                    sysParams.Add("sign_method", this.SignMethod);

                    var signature = this.generateSign(sysParams, data.appSecret);

                    string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + Uri.EscapeDataString(this.ParamJson) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                    urll += "&format=json&sign_method=md5";
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    try
                    {
                        using (WebResponse response = myReq.GetResponse())
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream);
                                responseFromServer = reader.ReadToEnd();
                                responseApi = true; break;
                            }
                        }
                    }
                    //catch (WebException ex)
                    //{
                    //    string err1 = "";
                    //    if (ex.Status == WebExceptionStatus.ProtocolError)
                    //    {
                    //        WebResponse resp1 = ex.Response;
                    //        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                    //        {
                    //            err1 = sr1.ReadToEnd();
                    //        }
                    //    }
                    //    //throw new Exception(err1);
                    //}
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                        {
                            retry = retry + 1;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            if (retry == 3)
                            {
                                throw new Exception(msg);
                            }
                        }
                        else
                        {
                            retry = 3;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            throw new Exception(msg);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    try
                    {
                        var ret = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDSetOnShelfV2)) as JDIDSetOnShelfV2;
                        if (ret.jingdong_seller_product_api_write_onShelf_response.returnType != null)
                        {
                            if (ret.jingdong_seller_product_api_write_onShelf_response.returnType.success)
                            {
                                rett = true;
                            }
                            else
                            {
                                throw new Exception(ret.jingdong_seller_product_api_write_onShelf_response.returnType.message.ToString());
                            }
                        }
                        else
                        {
                            throw new Exception("API error. Please contact support.");
                        }

                    }
                    catch (Exception ex2)
                    {
                        throw new Exception(ex2.InnerException == null ? ex2.Message.ToString() : ex2.InnerException.Message.ToString());
                    }
                }
                else
                {
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception(msg);
            }

            return rett;
        }

        public async Task<string> JD_addSKUVariantV2(JDIDAPIDataJob data, addSKUVariantJDID dataSKU, string sSPUID, string kodeProduk, string urlImage, int? recnum)
        {
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Add SKUVar V2",
                REQUEST_DATETIME = DateTime.UtcNow.AddHours(7),
                REQUEST_ATTRIBUTE_1 = kodeProduk,
                REQUEST_ATTRIBUTE_2 = sSPUID,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            var resultSKUID = "";
            try
            {
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;

                string myData = JsonConvert.SerializeObject(dataSKU);
                while (!responseApi && retry <= 2)
                {
                    data = RefreshToken(data);
                    var sysParams = new Dictionary<string, string>();
                    //    string sParamJson = "{\"spuId\":\"" + sSPUID + "\", \"skuList\": " +
                    //"[{\"skuName\":\"" + dataSKU.skuName + "\", \"sellerSkuId\":\"" + dataSKU.sellerSkuId + "\", \"saleAttributeIds\":\"" + dataSKU.saleAttributeIds + "\", \"jdPrice\":" + dataSKU.jdPrice + ", " +
                    //"\"costPrice\":" + dataSKU.costPrice + ", \"stock\":" + dataSKU.stock + ", \"weight\":\"" + dataSKU.weight + "\", \"netWeight\":\"" + dataSKU.netWeight + "\", " +
                    //"\"packHeight\":\"" + dataSKU.packHeight + "\", \"packLong\":\"" + dataSKU.packLong + "\", \"packWide\":\"" + dataSKU.packWide + "\", \"piece\":" + dataSKU.piece + "}]}";

                    this.ParamJson = myData;
                    sysParams.Add("360buy_param_json", this.ParamJson);

                    sysParams.Add("access_token", data.accessToken);
                    sysParams.Add("app_key", data.appKey);
                    this.Method = "jingdong.seller.product.sku.write.addSkuInfo"; //this API is for query sku information via spuId
                    sysParams.Add("method", this.Method);
                    var gettimestamp = getCurrentTimeFormatted();
                    sysParams.Add("timestamp", gettimestamp);
                    sysParams.Add("v", this.Version2);
                    sysParams.Add("format", this.Format);
                    sysParams.Add("sign_method", this.SignMethod);

                    var signature = this.generateSign(sysParams, data.appSecret);

                    string urll = ServerUrlV2 + "?v=" + Uri.EscapeDataString(Version2) + "&method=" + this.Method + "&app_key=" + Uri.EscapeDataString(data.appKey) + "&access_token=" + Uri.EscapeDataString(data.accessToken) + "&360buy_param_json=" + Uri.EscapeDataString(this.ParamJson) + "&timestamp=" + Uri.EscapeDataString(gettimestamp) + "&sign=" + Uri.EscapeDataString(signature);
                    urll += "&format=json&sign_method=md5";
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    try
                    {
                        using (WebResponse response = myReq.GetResponse())
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader reader = new StreamReader(stream);
                                responseFromServer = reader.ReadToEnd();
                                responseApi = true; break;
                            }
                        }
                    }
                    //catch (WebException ex)
                    //{
                    //    string err1 = "";
                    //    if (ex.Status == WebExceptionStatus.ProtocolError)
                    //    {
                    //        WebResponse resp1 = ex.Response;
                    //        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                    //        {
                    //            err1 = sr1.ReadToEnd();
                    //        }
                    //    }
                    //    //throw new Exception(err1);
                    //}
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                        {
                            retry = retry + 1;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            if (retry == 3)
                            {
                                currentLog.REQUEST_EXCEPTION = msg;
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                throw new Exception(msg);
                            }
                        }
                        else
                        {
                            retry = 3;
                            string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            currentLog.REQUEST_EXCEPTION = msg;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            throw new Exception(msg);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    try
                    {
                        var ret = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDAddSKUVariantV2)) as JDIDAddSKUVariantV2;
                        if (ret.jingdong_seller_product_sku_write_addSkuInfo_response.returnType != null)
                        {
                            if (ret.jingdong_seller_product_sku_write_addSkuInfo_response.returnType.success)
                            {
                                if (ret.jingdong_seller_product_sku_write_addSkuInfo_response.returnType.model != null)
                                {
                                    if (ret.jingdong_seller_product_sku_write_addSkuInfo_response.returnType.model.Count() > 0)
                                    {
                                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);

                                        resultSKUID = ret.jingdong_seller_product_sku_write_addSkuInfo_response.returnType.model[0].skuId.ToString();

                                        var item = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk && b.IDMARKET == recnum).SingleOrDefault();
                                        if (item != null)
                                        {
                                            item.BRG_MP = sSPUID + ";" + resultSKUID;
                                            item.LINK_STATUS = "Buat Produk Berhasil";
                                            item.LINK_DATETIME = DateTime.UtcNow.AddHours(7);
                                            item.LINK_ERROR = "0;Buat Produk;;";
                                            ErasoftDbContext.SaveChanges();
                                        }
                                        if (!string.IsNullOrEmpty(urlImage))
                                        {
                                            ////await JD_addSKUMainPicture(data, Convert.ToString(skuidVar), dataVar.LINK_GAMBAR_1);
                                            //await JD_addSKUDetailPictureV2(data, resultSKUID, urlImage, 1, false, sSPUID);
                                            string EDBConnID = EDB.GetConnectionString("ConnId");
                                            var sqlStorage = new SqlServerStorage(EDBConnID);

                                            var client = new BackgroundJobClient(sqlStorage);
                                            //#if (DEBUG || Debug_AWS)
                                            await JD_addSKUDetailPictureV2(data, kodeProduk, urlImage, 1, false, sSPUID);
                                            //#else
                                            //                                            client.Enqueue<JDIDControllerJob>(x => x.JD_addSKUDetailPictureV2(data, kodeProduk, urlImage, 1, false, sSPUID));
                                            //#endif
                                        }
                                    }
                                }
                            }
                            else
                            {
                                currentLog.REQUEST_EXCEPTION = ret.jingdong_seller_product_sku_write_addSkuInfo_response.returnType.message.ToString();
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                throw new Exception(ret.jingdong_seller_product_sku_write_addSkuInfo_response.returnType.message.ToString());
                            }
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = "API error. Please contact support.";
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            throw new Exception("API error. Please contact support.");
                        }

                    }
                    catch (Exception ex2)
                    {
                        string msg = ex2.InnerException != null ? ex2.InnerException.Message : ex2.Message;
                        currentLog.REQUEST_EXCEPTION = msg;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        throw new Exception(msg);
                    }
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = "Tidak ada respon dari API.";
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    throw new Exception("Tidak ada respon dari API.");
                }

            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                currentLog.REQUEST_EXCEPTION = msg;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                throw new Exception(msg);
            }

            return resultSKUID;
        }

        //end add by nurul 21/5/2021, JDID versi 2 tahap 2 

        protected enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4
        }
        protected void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, JDIDAPIDataJob iden, API_LOG_MARKETPLACE data)
        {
            switch (action)
            {
                case api_status.Pending:
                    {
                        var arf01 = ErasoftDbContext.ARF01.Where(p => p.TOKEN == iden.accessToken).FirstOrDefault();
                        var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                        {
                            //CUST = arf01 != null ? arf01.CUST : iden.accessToken,
                            CUST = arf01 != null ? arf01.CUST : iden.no_cust != null ? iden.no_cust : iden.merchant_code != null ? iden.merchant_code : "",
                            CUST_ATTRIBUTE_1 = iden.accessToken,
                            CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                            CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                            CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                            CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                            MARKETPLACE = "JD",
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

        public enum StatusOrder
        {
            PAID = 1,
            SHIPPED = 2,
            IN_CANCEL = 3,
            WAITING_REFUSE = 4,
            CANCELLED = 5,
            COMPLETED = 6,
            READY_TO_SHIP = 7
        }

        #region jdid data class

        public class JDIDAPIData
        {
            public string appKey { get; set; }
            public string appSecret { get; set; }
            public string accessToken { get; set; }
            public string no_cust { get; set; }
            public string account_store { get; set; }
            public string ID_MARKET { get; set; }
            public string username { get; set; }
            public string email { get; set; }
            public string DatabasePathErasoft { get; set; }
            public DateTime? tgl_expired { get; set; }
            public string merchant_code { get; set; }
            public string versi { get; set; }
            public string refreshToken { get; set; }
        }

        public class Model_PromoJob
        {
            public bool success { get; set; }
            public int code { get; set; }
            public string description { get; set; }
            public string order_id { get; set; }
        }

        public class Data_OrderDetailJob
        {
            public string message { get; set; }
            public string model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
        }

        public class Data_ReadyToShip
        {
            public int code { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
        }

        public class Data_PrintLabel
        {
            public int code { get; set; }
            public bool success { get; set; }
            public Model_PrintLabel model { get; set; }
            public string message { get; set; }
        }

        public class Model_PrintLabel
        {
            public Data_DetailPrintLabel[] data { get; set; }
            public int templateId { get; set; }
        }

        public class Data_DetailPrintLabel
        {
            public string PDF { get; set; }
            public int orderId { get; set; }
            public string preDeliveryId { get; set; }
            public string deliveryId { get; set; }
        }


        public class ModelOrderJob
        {
            public List<DetailOrder_JDJob> data { get; set; }
        }

        public class DetailOrder_JDJob
        {
            public string address { get; set; }
            public string area { get; set; }
            public long bookTime { get; set; }
            public string city { get; set; }
            public float couponAmount { get; set; }
            public string customerName { get; set; }
            public string deliveryAddr { get; set; }
            public int deliveryType { get; set; }
            public string email { get; set; }
            public float freightAmount { get; set; }
            public float fullCutAmount { get; set; }
            public float installmentFee { get; set; }
            public long orderCompleteTime { get; set; }
            public long orderId { get; set; }
            public long orderSkuNum { get; set; }
            public Orderskuinfo[] orderSkuinfos { get; set; }
            public int orderState { get; set; }
            public int orderType { get; set; }
            public float paySubtotal { get; set; }
            public int paymentType { get; set; }
            public string phone { get; set; }
            public string postCode { get; set; }
            public float promotionAmount { get; set; }
            public string sendPay { get; set; }
            public string state { get; set; }
            public float totalPrice { get; set; }
            public string userPin { get; set; }
            public string buyerMessage { get; set; }
            public string carrierCode { get; set; }
            public string carrierCompany { get; set; }
            public string expressNo { get; set; }
        }

        public class ModelPrintLabel
        {
            //public List<DetailPrintLabel_JD> data { get; set; }
        }



        public class OrderskuinfoJob
        {
            public float commission { get; set; }
            public float costPrice { get; set; }
            public float couponAmount { get; set; }
            public float fullCutAmount { get; set; }
            public int hasPromo { get; set; }
            public float jdPrice { get; set; }
            public float promotionAmount { get; set; }
            public long skuId { get; set; }
            public string skuName { get; set; }
            public int skuNumber { get; set; }
            public long spuId { get; set; }
            public int weight { get; set; }
            public string popSkuId { get; set; }
        }


        public class List_Order_JDJob
        {
            public List<string> orderIds { get; set; }
        }

        public class Data_OrderIdsJob
        {
            public string message { get; set; }
            public List<long> model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
        }

        public class Data_Detail_ProductJob
        {
            public int code { get; set; }
            public string message { get; set; }
            public List<Model_Detail_ProductJob> model { get; set; }
            public bool success { get; set; }
        }

        public class Model_Detail_ProductJob
        {
            public string skuName { get; set; }
            public long skuId { get; set; }
            public object upc { get; set; }
            public object sellerSkuId { get; set; }
            public string weight { get; set; }
            public string netWeight { get; set; }
            public string packLong { get; set; }
            public string packWide { get; set; }
            public string packHeight { get; set; }
            public int piece { get; set; }
            public string mainImgUri { get; set; }
            public int status { get; set; }
            public long spuId { get; set; }
            public string saleAttributeIds { get; set; }
            public dynamic saleAttributeNameMap { get; set; }
            public float jdPrice { get; set; }
            public object maxQuantity { get; set; }
        }

        public class JDIDAPIDataJob
        {
            public string appKey { get; set; }
            public string appSecret { get; set; }
            public string accessToken { get; set; }
            public string no_cust { get; set; }
            public string nama_cust { get; set; }
            public string username { get; set; }
            public string account_store { get; set; }
            public string ID_MARKET { get; set; }
            public string email { get; set; }
            public string DatabasePathErasoft { get; set; }
            public DateTime? tgl_expired { get; set; }
            public string merchant_code { get; set; }
            public string versi { get; set; }
            public string refreshToken { get; set; }
        }

        public class DataAddSKUVariant
        {
            public string skuName { get; set; }
            public string sellerSkuId { get; set; }
            public string saleAttributeIds { get; set; }
            public long jdPrice { get; set; }
            public long costPrice { get; set; }
            public int stock { get; set; }
            public string weight { get; set; }
            public string netWeight { get; set; }
            public string packHeight { get; set; }
            public string packLong { get; set; }
            public string packWide { get; set; }
            public int piece { get; set; }
        }

        public class Data_UpStokJob
        {
            public int code { get; set; }
            public string message { get; set; }
            public dynamic model { get; set; }
            public bool success { get; set; }
        }

        public class Data_UpPriceJob
        {
            public int code { get; set; }
            public string message { get; set; }
            public dynamic model { get; set; }
            public bool success { get; set; }
        }


        public class Data_BrandJob
        {
            public List<Model_BrandJob> model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
        }

        public class Model_BrandJob
        {
            public long id { get; set; }
            public int isForever { get; set; }
            public string logo { get; set; }
            public long shopId { get; set; }
            public int state { get; set; }
            public int brandState { get; set; }
            public long brandId { get; set; }
            public string brandName { get; set; }
        }


        public class JDID_ResultAddSKUMainPicture
        {
            public int openapi_code { get; set; }
            public string openapi_data { get; set; }
            public string openapi_msg { get; set; }
        }


        public class JDID_DetailResultAddSKUMainPicture
        {
            public int code { get; set; }
            public string model { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
        }

        public class JDID_ResultAddSKUVariant
        {
            public int code { get; set; }
            public JDID_ResultAddSKUVariantListModel[] model { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
        }

        public class JDID_GetSKUVariantbySPU
        {
            public int code { get; set; }
            public JDID_ResultGetSKUVariantModel[] model { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
        }

        public class JDID_ResultGetSKUVariantModel
        {
            public string sellerSkuId { get; set; }
            public string skuId { get; set; }
            public string skuName { get; set; }
            public string spuId { get; set; }
        }



        public class JDID_ResultAddSKUVariantListModel
        {
            public int skuId { get; set; }
        }

        public class JDID_DetailResultCreateProduct
        {
            public int code { get; set; }
            public DetailResponseCreateProduct model { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
        }

        public class JDID_DetailResultUpdateProduct
        {
            public int code { get; set; }
            public bool model { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
        }

        public class DetailResponseCreateProduct
        {
            public Skuidlist[] skuIdList { get; set; }
            public int spuId { get; set; }
            public int venderId { get; set; }
        }

        public class Skuidlist
        {
            public int skuId { get; set; }
            public string skuName { get; set; }
            public int status { get; set; }
            public int stock { get; set; }
        }


        public class JDID_ResultCreateProduct
        {
            public int openapi_code { get; set; }
            public string openapi_data { get; set; }
            public string openapi_msg { get; set; }
        }


        public class JDID_SubDetailResultCreateProduct
        {
            public string skuIdList { get; set; }
            public string spuId { get; set; }
            public string venderId { get; set; }
        }

        public class JDID_SubSKUListDetailResultCreateProduct
        {
            public int skuId { get; set; }
            public string skuName { get; set; }
            public int status { get; set; }
            public int stock { get; set; }
        }

        public class JDID_RESJob
        {
            public string openapi_data { get; set; }
            public string error { get; set; }
            public int openapi_code { get; set; }
            public string openapi_msg { get; set; }
        }


        public class DATA_CATJob
        {
            public List<Model_CatJob> model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
        }
        public class Model_CatJob
        {
            public long id { get; set; }
            public int cateState { get; set; }
            public Model_CatJob parentCateVo { get; set; }
            public long cate1Id { get; set; }
            public string cateNameEn { get; set; }
            public long shopId { get; set; }
            public int state { get; set; }
            public long cate3Id { get; set; }
            public int type { get; set; }
            public long cate2Id { get; set; }
            public long cateId { get; set; }
            public string cateName { get; set; }
        }


        public class AttrDataJob
        {
            public int code { get; set; }
            public string message { get; set; }
            public List<Model_AttrJob> model { get; set; }
            public bool success { get; set; }
        }

        public class Model_AttrJob
        {
            public long propertyId { get; set; }
            public int type { get; set; }
            public string name { get; set; }
            public string nameEn { get; set; }
        }

        public class JDID_ATTRIBUTE_OPTJob
        {
            public int code { get; set; }
            public string message { get; set; }
            public Model_OptJob model { get; set; }
            public bool success { get; set; }
        }

        public class Model_OptJob
        {
            public int pageSize { get; set; }
            public int totalCount { get; set; }
            public List<JDOptJob> data { get; set; }
        }

        public class JDOptJob
        {
            public long attributeValueId { get; set; }
            public string name { get; set; }
            public string nameEn { get; set; }
            public long attributeId { get; set; }
        }

        public class Data_ListProdJob
        {
            public Model_ListProdJob model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
        }

        public class Model_ListProdJob
        {
            public int totalNum { get; set; }
            public string _class { get; set; }
            public List<SpuinfovolistJob> spuInfoVoList { get; set; }
            public int pageNum { get; set; }
        }

        public class SpuinfovolistJob
        {
            public long transportId { get; set; }
            public string mainImgUri { get; set; }
            public long spuId { get; set; }
            public string fullCategoryId { get; set; }
            public string _class { get; set; }
            public string fullCategoryName { get; set; }
            public long brandId { get; set; }
            public long warrantyPeriod { get; set; }
            public string description { get; set; }
            public long shopId { get; set; }
            //public int afterSale { get; set; }
            public string spuName { get; set; }
            //public string appDescription { get; set; }
            public int wareStatus { get; set; }
        }
        public class ProductDataJob
        {
            public List<Model_ProductJob> model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
        }

        public class Model_ProductJob
        {
            public string packWide { get; set; }
            public string weight { get; set; }
            public string mainImgUri { get; set; }
            public long spuId { get; set; }
            public float jdPrice { get; set; }
            public string skuName { get; set; }
            public dynamic saleAttributeNameMap { get; set; }
            public string saleAttributeIds { get; set; }
            public string netWeight { get; set; }
            public long skuId { get; set; }
            public string packHeight { get; set; }
            public string packLong { get; set; }
            public int piece { get; set; }
        }
        public class DataProdJob
        {
            public int code { get; set; }
            public object message { get; set; }
            public List<Model_Product_2Job> model { get; set; }
            public bool success { get; set; }
        }

        public class Model_Product_2Job
        {
            public object wareTypeId { get; set; }
            //public object sellerType { get; set; }
            public object catId { get; set; }
            public object wareStatus { get; set; }
            public string mainImgUri { get; set; }
            //public int shopId { get; set; }
            public long spuId { get; set; }
            public string spuName { get; set; }
            public string fullCategoryId { get; set; }
            public string fullCategoryName { get; set; }
            public string shopName { get; set; }
            public long transportId { get; set; }
            public object propertyRight { get; set; }
            public string brandName { get; set; }
            public string brandLogo { get; set; }
            public List<long> skuIds { get; set; }
            public string appDescription { get; set; }
            public string commonAttributeIds { get; set; }
            public dynamic commonAttributeNameMap { get; set; }
            public long modified { get; set; }
            public object imgUris { get; set; }
            public long brandId { get; set; }
            public object productArea { get; set; }
            public string description { get; set; }
            public int auditStatus { get; set; }
            public int minQuantity { get; set; }
            public int afterSale { get; set; }
            public object crossProductType { get; set; }
            public object clearanceType { get; set; }
            public object taxesType { get; set; }
            public object countryId { get; set; }
        }

        //add by nurul 28/4/2021
        public class JDIDGetTokenResult
        {
            public string access_token { get; set; }
            public long expires_in { get; set; }
            public string refresh_token { get; set; }
            public string scope { get; set; }
            public string open_id { get; set; }
            public string uid { get; set; }
            public long time { get; set; }
            public string token_type { get; set; }
            public string code { get; set; }
        }

        //end add by nurul 28/4/2021

        //add by nurul 4/5/2021, JDID versi 2

        public class JDIDGetOrderListV2Result
        {
            public Jingdong_Seller_Order_Getorderidlistbycondition_Response jingdong_seller_order_getOrderIdListByCondition_response { get; set; }
        }

        public class Jingdong_Seller_Order_Getorderidlistbycondition_Response
        {
            public string code { get; set; }
            public data_GetOrderListV2 result { get; set; }
        }

        public class data_GetOrderListV2
        {
            public string message { get; set; }
            public List<long> model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
        }

        //---------------------------------------------

        public class JDIDGetOrderDetailV2Result
        {
            public Jingdong_Seller_Order_Batchgetorderinfolist_Response jingdong_seller_order_batchgetorderinfolist_response { get; set; }
        }

        public class Jingdong_Seller_Order_Batchgetorderinfolist_Response
        {
            public GetOrderDetailV2Result result { get; set; }
            public string code { get; set; }
        }

        public class GetOrderDetailV2Result
        {
            public int code { get; set; }
            public bool success { get; set; }
            public GetOrderDetailV2Model[] model { get; set; }
            public string message { get; set; }
        }

        public class GetOrderDetailV2Model
        {
            public int serviceType { get; set; }
            public int orderType { get; set; }
            public string sendPay { get; set; }
            public float installmentFee { get; set; }
            public long payTime { get; set; }
            public string city { get; set; }
            public string expressNo { get; set; }
            public long orderId { get; set; }
            public float totalPrice { get; set; }
            public string carrierCompany { get; set; }
            public long venderId { get; set; }
            public float paySubtotal { get; set; }
            public float fullCutAmount { get; set; }
            public string addLat { get; set; }
            public int orderState { get; set; }
            public int paymentType { get; set; }
            public bool o2oOrder { get; set; }
            public float couponAmount { get; set; }
            public long orderCompleteTime { get; set; }
            public long modifyTime { get; set; }
            public string state { get; set; }
            public long expressAttribute { get; set; }
            public string email { get; set; }
            public string area { get; set; }
            public float jdSubsidy { get; set; }
            public string address { get; set; }
            public GetOrderDetailV2Orderskuinfo[] orderSkuinfos { get; set; }
            public float freightAmount { get; set; }
            public long orderSkuNum { get; set; }
            public string mobile { get; set; }
            public int deliveryType { get; set; }
            public long bookTime { get; set; }
            public string customerName { get; set; }
            public string userPin { get; set; }
            public string addLng { get; set; }
            public string phone { get; set; }
            public long createTime { get; set; }
            public long carrierCode { get; set; }
            public float taxationAmount { get; set; }
            public float sellerSubsidy { get; set; }
            public string postCode { get; set; }
            public float promotionAmount { get; set; }
            public string deliveryAddr { get; set; }
            public string buyerMessage { get; set; }
        }

        public class GetOrderDetailV2Orderskuinfo
        {
            public string salesAttr { get; set; }
            public int hasPromo { get; set; }
            public float costPrice { get; set; }
            public int weight { get; set; }
            public float fullCutAmount { get; set; }
            public string skuName { get; set; }
            public int crossType { get; set; }
            public float couponAmount { get; set; }
            public long skuNumber { get; set; }
            public float taxationAmount { get; set; }
            public long spuId { get; set; }
            public float jdPrice { get; set; }
            public float commission { get; set; }
            public string popSkuId { get; set; }
            public float promotionAmount { get; set; }
            public string skuImage { get; set; }
            public long skuId { get; set; }
        }

        //------------------------------------------


        public class JDIDLabelV2
        {
            public Jingdong_Seller_Order_Printorder_Response jingdong_seller_order_printorder_response { get; set; }
        }

        public class Jingdong_Seller_Order_Printorder_Response
        {
            public LabelV2Result result { get; set; }
            public string code { get; set; }
        }

        public class LabelV2Result
        {
            public int code { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
            public LabelV2Model model { get; set; }
        }

        public class LabelV2Model
        {
            public long orderId { get; set; }
            public string expressNo { get; set; }
            public string preExpressNo { get; set; }
            public string content { get; set; }
        }

        //---------------------------------------

        public class JDIDRTSV2
        {
            public Jingdong_Seller_Order_Sendgoodsopenapi_Response jingdong_seller_order_sendgoodsopenapi_response { get; set; }
        }

        public class Jingdong_Seller_Order_Sendgoodsopenapi_Response
        {
            public RTSV2Result result { get; set; }
            public string code { get; set; }
        }

        public class RTSV2Result
        {
            public int code { get; set; }
            public bool success { get; set; }
            public RTSV2Model model { get; set; }
            public string message { get; set; }
        }

        public class RTSV2Model
        {
            public long orderId { get; set; }
            public string expressCompany { get; set; }
            public string expressId { get; set; }
            public string expressNo { get; set; }
        }

        //end add by nurul 4/5/2021, JDID versi 2

        //add by nurul 21/5/2021, JDID versi 2 tahap 2 

        public class JDIDUpdatePriceV2
        {
            public Jingdong_Seller_Price_Updatepricebyskuids_Response jingdong_seller_price_updatePriceBySkuIds_response { get; set; }
        }

        public class Jingdong_Seller_Price_Updatepricebyskuids_Response
        {
            public string code { get; set; }
            public ReturntypeUpdatePriceV2 returnType { get; set; }
        }

        public class ReturntypeUpdatePriceV2
        {
            public string message { get; set; }
            public int[] model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
        }

        //-------------------------------------

        public class JDIDUpdateSKUV2
        {
            public Jingdong_Seller_Product_Sku_Write_Updateskulist_Response jingdong_seller_product_sku_write_updateSkuList_response { get; set; }
        }

        public class Jingdong_Seller_Product_Sku_Write_Updateskulist_Response
        {
            public string code { get; set; }
            public ReturntypeUpdateSKUV2 returnType { get; set; }
        }

        public class ReturntypeUpdateSKUV2
        {
            public int code { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
        }

        //------------------------------------
        public class JDIDdoAuditProductV2
        {
            public Jingdong_Seller_Product_Api_Write_SubmitAudit_Response jingdong_seller_product_api_write_submitAudit_response { get; set; }
        }

        public class Jingdong_Seller_Product_Api_Write_SubmitAudit_Response
        {
            public string code { get; set; }
            public ReturntypedoAuditProductV2 returnType { get; set; }
        }

        public class ReturntypedoAuditProductV2
        {
            public int code { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
            public string model { get; set; }
        }

        //-----------------------------------------
        public class JDIDaddSKUDetailPictureV2
        {
            public Jingdong_Seller_Product_Sku_Write_UpdateProductImages_Response jingdong_seller_product_sku_write_updateProductImages_response { get; set; }
        }

        public class Jingdong_Seller_Product_Sku_Write_UpdateProductImages_Response
        {
            public string code { get; set; }
            public ReturntypeaddSKUDetailPictureV2 returnType { get; set; }
        }

        public class ReturntypeaddSKUDetailPictureV2
        {
            public int code { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
            public string model { get; set; }
        }

        //-------------------------------------------
        public class CreateProductJDID
        {
            public spuInfo_JDIDCREATE spuInfo { get; set; }
            public List<skuList_JDIDCREATE> skuList { get; set; }
        }

        public class spuInfo_JDIDCREATE
        {
            public string packLong { get; set; }
            public string spuName { get; set; }
            public string commonAttributeIds { get; set; }
            public string keywords { get; set; }
            public string description { get; set; }
            public long countryId { get; set; }
            public int warrantyPeriod { get; set; }
            public string productArea { get; set; }
            public long minQuantity { get; set; }
            public long crossProductType { get; set; }
            public string packHeight { get; set; }
            public int taxesType { get; set; }
            public string appDescription { get; set; }
            public string weight { get; set; }
            public string subtitleHrefM { get; set; }
            public int qualityDays { get; set; }
            public string packWide { get; set; }
            public long catId { get; set; }
            public int whetherCod { get; set; }
            public int piece { get; set; }
            public long brandId { get; set; }
            public string subtitle { get; set; }
            public int isQuality { get; set; }
            public string packageInfo { get; set; }
            public int afterSale { get; set; }
            public int clearanceType { get; set; }
            public string subtitleHref { get; set; }
            public long maxQuantity { get; set; }
        }

        public class skuList_JDIDCREATE
        {
            public string saleAttributeIds { get; set; }
            public long costPrice { get; set; }
            public string upc { get; set; }
            public string sellerSkuId { get; set; }
            public string saleAttrValueAlias { get; set; }
            public string skuName { get; set; }
            public long jdPrice { get; set; }
            public long stock { get; set; }
        }

        public class UpdateProductJDID
        {
            public string packLong { get; set; }
            public string spuName { get; set; }
            public string commonAttributeIds { get; set; }
            public string keywords { get; set; }
            public string description { get; set; }
            public long countryId { get; set; }
            public int warrantyPeriod { get; set; }
            public string productArea { get; set; }
            public long minQuantity { get; set; }
            public long crossProductType { get; set; }
            public string packHeight { get; set; }
            public int taxesType { get; set; }
            public string appDescription { get; set; }
            public string weight { get; set; }
            public string subtitleHrefM { get; set; }
            public int qualityDays { get; set; }
            public string packWide { get; set; }
            public long catId { get; set; }
            public int whetherCod { get; set; }
            public int piece { get; set; }
            public long brandId { get; set; }
            public string subtitle { get; set; }
            public int isQuality { get; set; }
            public long spuId { get; set; }
            public string packageInfo { get; set; }
            public int afterSale { get; set; }
            public int clearanceType { get; set; }
            public string subtitleHref { get; set; }
            public long maxQuantity { get; set; }
            public List<long> shopCategoryIds { get; set; }
        }

        //------------------------------------------------

        public class JDIDCreateProductV2
        {
            public Jingdong_Seller_Product_Api_Write_Addproduct_Response jingdong_seller_product_api_write_addProduct_response { get; set; }
        }

        public class Jingdong_Seller_Product_Api_Write_Addproduct_Response
        {
            public string code { get; set; }
            public ReturntypeCreateProductV2 returnType { get; set; }
        }

        public class ReturntypeCreateProductV2
        {
            public ModelCreateProductV2 model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
        }

        public class ModelCreateProductV2
        {
            public long spuId { get; set; }
            public SkuidlistCreateProductV2[] skuIdList { get; set; }
        }

        public class SkuidlistCreateProductV2
        {
            public long skuId { get; set; }
        }

        //------------------------------

        public class JDIDUpdateProductV2
        {
            public Jingdong_Seller_Product_Api_Write_Updateproduct_Response jingdong_seller_product_api_write_updateProduct_response { get; set; }
        }

        public class Jingdong_Seller_Product_Api_Write_Updateproduct_Response
        {
            public string code { get; set; }
            public ReturntypeUpdateProductV2 returnType { get; set; }
        }

        public class ReturntypeUpdateProductV2
        {
            public string message { get; set; }
            public bool model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
        }

        //-----------------------------------------

        public class JDIDGetSKUVariantBySPUV2
        {
            public Jingdong_Seller_Product_Getskuinfobyspuidandvenderid_Response jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response { get; set; }
        }

        public class Jingdong_Seller_Product_Getskuinfobyspuidandvenderid_Response
        {
            public string code { get; set; }
            public ReturntypeGetSKUVariantBySPUV2 returnType { get; set; }
        }

        public class ReturntypeGetSKUVariantBySPUV2
        {
            public int code { get; set; }
            public bool success { get; set; }
            public ModelGetSKUVariantBySPUV2[] model { get; set; }
            public string message { get; set; }
        }

        public class ModelGetSKUVariantBySPUV2
        {
            public string packLong { get; set; }
            public string weight { get; set; }
            public string upc { get; set; }
            public string sellerSkuId { get; set; }
            public string packWide { get; set; }
            public string skuName { get; set; }
            public string netWeight { get; set; }
            public int piece { get; set; }
            public int spuId { get; set; }
            public float jdPrice { get; set; }
            public string packHeight { get; set; }
            public int skuId { get; set; }
            public int status { get; set; }
            public string mainImgUri { get; set; }
            public string saleAttributeIds { get; set; }
            public dynamic saleAttributeNameMap { get; set; }
        }

        //---------------------------------------
        public class addSKUVariantJDID
        {
            public long spuId { get; set; }
            public List<string> packLong { get; set; }
            public List<string> saleAttributeIds { get; set; }
            public Int32[] costPrice { get; set; }
            public List<string> upc { get; set; }
            public List<string> weight { get; set; }
            public List<string> sellerSkuId { get; set; }
            public List<string> saleAttrValueAlias { get; set; }
            public List<string> skuName { get; set; }
            public List<string> packWide { get; set; }
            public int[] piece { get; set; }
            public Int32[] jdPrice { get; set; }
            public List<string> packHeight { get; set; }
            public Int32[] stock { get; set; }

            //public string[] packLong { get; set; }
            //public string[] saleAttributeIds { get; set; }
            //public long[] costPrice { get; set; }
            //public string[] upc { get; set; }
            //public string[] weight { get; set; }
            //public string[] sellerSkuId { get; set; }
            //public string[] saleAttrValueAlias { get; set; }
            //public string[] skuName { get; set; }
            //public string[] packWide { get; set; }
            //public int[] piece { get; set; }
            //public long[] jdPrice { get; set; }
            //public string[] packHeight { get; set; }
            //public long[] stock { get; set; }
        }

        //-----------------------------------------

        public class JDIDAddSKUVariantV2
        {
            public Jingdong_Seller_Product_Sku_Write_AddSkuInfo_Response jingdong_seller_product_sku_write_addSkuInfo_response { get; set; }
        }

        public class Jingdong_Seller_Product_Sku_Write_AddSkuInfo_Response
        {
            public string code { get; set; }
            public ReturntypeAddSKUVariantV2 returnType { get; set; }
        }

        public class ReturntypeAddSKUVariantV2
        {
            public int code { get; set; }
            public bool success { get; set; }
            public ModelAddSKUVariantV2[] model { get; set; }
            public string message { get; set; }
        }

        public class ModelAddSKUVariantV2
        {
            public string packLong { get; set; }
            public string weight { get; set; }
            public string upc { get; set; }
            public string sellerSkuId { get; set; }
            public string packWide { get; set; }
            public string skuName { get; set; }
            public string netWeight { get; set; }
            public int piece { get; set; }
            public int spuId { get; set; }
            public int jdPrice { get; set; }
            public string packHeight { get; set; }
            public int skuId { get; set; }
            public int status { get; set; }
            public string mainImgUri { get; set; }
            public string saleAttributeIds { get; set; }
            public dynamic saleAttributeNameMap { get; set; }
        }

        //------------------------

        public class JDIDSetOnShelfV2
        {
            public Jingdong_Seller_Product_Api_Write_Onshelf_Response jingdong_seller_product_api_write_onShelf_response { get; set; }
        }

        public class Jingdong_Seller_Product_Api_Write_Onshelf_Response
        {
            public string code { get; set; }
            public ReturntypeSetOnShelfV2 returnType { get; set; }
        }

        public class ReturntypeSetOnShelfV2
        {
            public int code { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
        }

        //end add by nurul 21/5/2021, JDID versi 2 tahap 2 

        //add by nurul 12/10/2021

        public class JDIDGetSPUInfoV2
        {
            public Jingdong_Seller_Product_Getwarebyspuids_Response jingdong_seller_product_getWareBySpuIds_response { get; set; }
        }

        public class Jingdong_Seller_Product_Getwarebyspuids_Response
        {
            public string code { get; set; }
            public ReturntypeGetSPUInfoV2 returnType { get; set; }
        }

        public class ReturntypeGetSPUInfoV2
        {
            public int code { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
            public ModelGetSPUInfoV2[] model { get; set; }
        }

        public class ModelGetSPUInfoV2
        {
            public string brandName { get; set; }
            public string spuName { get; set; }
            public string[] keywords { get; set; }
            public long maxQuantity { get; set; }
            public string[] imgUris { get; set; }
            public string appDescription { get; set; }
            public string description { get; set; }
            public long[] skuIds { get; set; }
            public string fullCategoryId { get; set; }
            public int wareStatus { get; set; }
            public int warrantyPeriod { get; set; }
            public int minQuantity { get; set; }
            public int whetherCod { get; set; }
            public long brandId { get; set; }
            public int auditStatus { get; set; }
            public long modified { get; set; }
            public long crossProductType { get; set; }
            public long spuId { get; set; }
            public long shopId { get; set; }
            public int afterSale { get; set; }
            public string brandLogo { get; set; }
            public string mainImgUri { get; set; }
        }

        //end add by nurul 12/10/2021
        #endregion
    }
}
