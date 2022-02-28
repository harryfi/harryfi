using Erasoft.Function;
using MasterOnline.Models;
using MasterOnline.ViewModels;
using Newtonsoft.Json;
using System;
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
//using System.Web.Http;
using System.Threading.Tasks;

using System.Web.Mvc;

namespace MasterOnline.Controllers
{
    public class JDIDController : System.Web.Http.ApiController
    {

        //AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        //#if AWS
        //        string jdidCallbackUrl = "https://masteronline.co.id/jdid/auth?user=";
        //#else
        //        string jdidCallbackUrl = "https://dev.masteronline.co.id/jdid/auth?user=";
        //        //string jdidCallbackUrl = "https://masteronline.my.id/jdid/auth?user=";
        //#endif
#if AWS
        string jdidCallbackUrl = "https://masteronline.co.id/jdid/auth?shop_id=";
#else
        string jdidCallbackUrl = "https://dev.masteronline.co.id/jdid/auth?shop_id=";
        //string jdidCallbackUrl = "https://masteronline.my.id/jdid/auth?shop_id=";
#endif

        public MoDbContext MoDbContext { get; set; }
        public string imageUrl = "https://img20.jd.id/Indonesia/s300x300_/";
        public string ServerUrl = "https://open.jd.id/api";
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
        public List<Model_Brand> listBrand = new List<Model_Brand>();
        // add by fauzi 8 Desember 2020
        public string shopID_JDID;
        protected List<long> listCategory_lv1 = new List<long>();
        protected List<long> listCategory_lv2 = new List<long>();
        protected List<long> listCategory_lv3 = new List<long>();
        // end by fauzi 8 Desember 2020

        //add by nurul 4/5/2021, JDID versi 2
        public string ServerUrlV2 = "https://open-api.jd.id/routerjson";
        public string Version2 = "2.0";
        //end add by nurul 4/5/2021, JDID versi 2

        public ErasoftContext ErasoftDbContext { get; set; }
        DatabaseSQL EDB;
        string username;
        string dbSourceEra = "";

        public JDIDController()
        {
            MoDbContext = new MoDbContext("");
            username = "";
            //var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            //            if (sessionData?.Account != null)
            //            {
            //                if (sessionData.Account.UserId == "admin_manage")
            //                {
            //                    ErasoftDbContext = new ErasoftContext();
            //                }
            //                else
            //                {
            //#if (Debug_AWS)
            //                    dbSourceEra = sessionData.Account.DataSourcePathDebug;
            //#else
            //                    dbSourceEra = sessionData.Account.DataSourcePath;
            //#endif
            //                    ErasoftDbContext = new ErasoftContext(dbSourceEra, sessionData.Account.DatabasePathErasoft);
            //                }

            //                EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);
            //                username = sessionData.Account.Username;
            //            }
            //            else
            //            {
            //                if (sessionData?.User != null)
            //                {
            //                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
            //#if (Debug_AWS)
            //                    dbSourceEra = accFromUser.DataSourcePathDebug;
            //#else
            //                    dbSourceEra = accFromUser.DataSourcePath;
            //#endif
            //                    ErasoftDbContext = new ErasoftContext(dbSourceEra, accFromUser.DatabasePathErasoft);
            //                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
            //                    username = accFromUser.Username;
            //                }
            //            }

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
                    username = accFromUser.Username;
                }
            }

            if (username.Length > 20)
                username = username.Substring(0, 17) + "...";
        }

        [HttpGet]
        public string JDIDUrl(string cust, string appkey, string shopid)
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
            string compUrl = jdidCallbackUrl + shopid;
            //string uri = "https://oauth.jd.id/oauth2/to_login?app_key=" + appkey + "&response_type=code&redirect_uri=https://masteronline.co.id/&state=20200428&scope=snsapi_base";
            string uri = "https://oauth.jd.id/oauth2/to_login?app_key=" + appkey + "&response_type=code&redirect_uri=" + compUrl + "&state=20200428&scope=snsapi_base";
            return uri;
        }

        #region jdid tools
        private string getCurrentTimeFormatted()
        {
            var dt = System.DateTime.Now.ToLocalTime();
            return dt.ToString("yyyy-MM-dd HH:mm:ss.fff") + dt.ToString("zzzz").Replace(":", "");
        }

        private string getCurrentTimeFormattedV2()
        {
            var dt = System.DateTime.Now.ToLocalTime();
            return dt.ToString("yyyy-MM-dd HH:mm:ss.fff") + dt.ToString("zzzz").Replace(":", "");
            //return dt.ToString("yyyy-MM-dd HH:mm:ss.SSSZ");
            //return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public string Call(string sappKey, string saccessToken, string sappSecret)
        {
            //construct system parameters
            var sysParams = new Dictionary<string, string>();
            //sysParams.Add("app_key", this.AppKey);
            sysParams.Add("app_key", sappKey);
            sysParams.Add("v", this.Version);
            sysParams.Add("format", this.Format);
            sysParams.Add("sign_method", this.SignMethod);
            sysParams.Add("method", this.Method);
            sysParams.Add("timestamp", this.getCurrentTimeFormatted());
            //sysParams.Add("access_token", this.AccessToken);
            sysParams.Add("access_token", saccessToken);

            //get business parameters
            if (null != this.ParamJson && this.ParamJson.Length > 0)
            {
                sysParams.Add("param_json", this.ParamJson);
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
        //public string CallV2(string sappKey, string saccessToken, string sappSecret)
        //{
        //    //construct system parameters
        //    var sysParams = new Dictionary<string, string>();
        //    if (null != this.ParamJson && this.ParamJson.Length > 0)
        //    {
        //        sysParams.Add("360buy_param_json", this.ParamJson);
        //    }
        //    else
        //    {
        //        sysParams.Add("360buy_param_json", "{}");
        //    }
        //    sysParams.Add("access_token", saccessToken);
        //    sysParams.Add("app_key", sappKey);
        //    sysParams.Add("method", this.Method);
        //    var gettimestamp = this.getCurrentTimeFormatted();
        //    sysParams.Add("timestamp", gettimestamp);
        //    //sysParams.Add("timestamp", gettimestamp);
        //    sysParams.Add("v", this.Version);

        //    sysParams.Add("format", this.Format);
        //    sysParams.Add("sign_method", this.SignMethod);

        //    //sysParams.Add("sign", this.generateSignV2(sysParams, sappSecret));
        //    sysParams.Add("sign", this.generateSign(sysParams, sappSecret));

        //    //send http post request
        //    sysParams.Remove("timestamp");
        //    sysParams.Remove("360buy_param_json");
        //    sysParams.Add("timestamp", Uri.EscapeDataString(gettimestamp));
        //    if (null != this.ParamJson && this.ParamJson.Length > 0)
        //    {
        //        sysParams.Add("360buy_param_json", Uri.EscapeDataString(this.ParamJson));
        //    }
        //    else
        //    {
        //        sysParams.Add("360buy_param_json", Uri.EscapeDataString("{}"));
        //    }
        //    var content = this.curl(this.ServerUrl, null, sysParams);
        //    return content;
        //}

        public string Call4BigData(string sappSecret)
        {
            //construct system parameters
            var sysParams = new Dictionary<string, string>();
            sysParams.Add("app_key", this.AppKey);
            sysParams.Add("v", this.Version);
            sysParams.Add("format", this.Format);
            sysParams.Add("sign_method", this.SignMethod);
            sysParams.Add("method", this.Method);
            sysParams.Add("timestamp", this.getCurrentTimeFormatted());
            sysParams.Add("access_token", this.AccessToken);

            //get business parameters
            if (null != this.ParamJson && this.ParamJson.Length > 0)
            {
                sysParams.Add("param_json", this.ParamJson);
            }
            else
            {
                sysParams.Add("param_json", "{}");
            }

            //get business file which would upload
            if (null != this.ParamFile && this.ParamFile.Length > 0)
            {
                sysParams.Add("param_file_md5", this.GetMD5HashFromFile(this.ParamFile));
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
            var content = this.curl(this.ServerUrl, new string[] { this.ParamFile }, sysParams);
            return content;
        }

        private string GetMD5HashFromFile(string fileName)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
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

                        using (var fileStream = new FileStream(files[i], FileMode.Open, FileAccess.Read))
                        {
                            var buffer = new byte[1024];
                            var bytesRead = 0;
                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                memStream.Write(buffer, 0, bytesRead);
                            }
                        }
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
        private string generateSignV2(Dictionary<string, string> sysDataDictionary, string sappSecret)
        {
            var dic = sysDataDictionary.OrderBy(key => key.Key).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
            var sb = new System.Text.StringBuilder();
            var buy_param_json = new System.Text.StringBuilder();
            var access_token = new System.Text.StringBuilder();
            var app_key = new System.Text.StringBuilder();
            var method = new System.Text.StringBuilder();
            var timestamp = new System.Text.StringBuilder();
            var v = new System.Text.StringBuilder();
            foreach (var item in dic)
            {
                if (!"".Equals(item.Key) && !"".Equals(item.Value))
                {
                    if (item.Key == "360buy_param_json")
                    {
                        buy_param_json.Append(item.Key).Append(item.Value);
                    }
                    if (item.Key == "access_token")
                    {
                        access_token.Append(item.Key).Append(item.Value);
                    }
                    if (item.Key == "app_key")
                    {
                        app_key.Append(item.Key).Append(item.Value);
                    }
                    if (item.Key == "method")
                    {
                        method.Append(item.Key).Append(item.Value);
                    }
                    if (item.Key == "timestamp")
                    {
                        timestamp.Append(item.Key).Append(item.Value);
                    }
                    if (item.Key == "v")
                    {
                        v.Append(item.Key).Append(item.Value);
                    }
                    //sb.Append(item.Key).Append(item.Value);
                }

            }
            sb.Append(sappSecret).Append(buy_param_json).Append(access_token).Append(app_key).Append(method).Append(timestamp).Append(v).Append(sappSecret);
            //prepend and append appsecret   
            //sb.Insert(0, this.AppSecret);
            //sb.Append(this.AppSecret);
            //sb.Insert(0, sappSecret);
            //sb.Append(sappSecret);
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

        //Check Data Shop
        public async Task<string> JDID_checkAPICustomerShop(JDIDAPIData data)
        {
            getShopBrand(data);
            string retr = "";
            var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();

            DatabaseSQL EDB = new DatabaseSQL(data.DatabasePathErasoft);
            var resultExecDefault = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST = '" + data.no_cust + "'");

            var mgrApiManager = new JDIDController();
            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;
            mgrApiManager.Method = "epi.ware.openapi.CategoryApi.getAllCategoryTree";
            mgrApiManager.ParamJson = "";

            try
            {
                var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (ret != null)
                {
                    if (ret.openapi_code == 0)
                    {
                        //var listKategori = JsonConvert.DeserializeObject(ret.openapi_data, typeof(DATA_CAT)) as DATA_CAT;
                        var listKategori = JsonConvert.DeserializeObject(ret.openapi_data, typeof(DATA_CAT)) as DATA_CAT;
                        if (listKategori != null)
                        {
                            if (listKategori.success)
                            {
                                contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Akun marketplace " + data.email.ToString() + " (JD.ID) berhasil aktif", true);
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
                                var listKtg = ErasoftDbContext.CATEGORY_JDID.ToList();
                                if (listKtg.Count > 0)
                                {
                                    EDB.ExecuteSQL("CString", CommandType.Text, "DELETE FROM CATEGORY_JDID");
                                }
                                //                                #region connstring
                                //#if AWS
                                //                    string con = "Data Source=" + dbPath + ";Initial Catalog=" + dbPath + ";Persist Security Info=True;User ID=sa;Password=admin123^";
                                //#elif Debug_AWS
                                //                                string con = "Data Source=13.250.232.74;Initial Catalog=" + dbPath + ";Persist Security Info=True;User ID=sa;Password=admin123^";
                                //#else
                                //                                string con = "Data Source=13.251.222.53;Initial Catalog=" + dbPath + ";Persist Security Info=True;User ID=sa;Password=admin123^";
                                //#endif
                                //                                #endregion

                                string con = EDB.ConnectionStrings.FirstOrDefault().Value.ToString();

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
                                contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Data akun marketplace (JD.ID) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                            }
                        }
                        else
                        {
                            contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Data akun marketplace (JD.ID) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                        }
                    }
                    else
                    {
                        contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Data akun marketplace (JD.ID) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                    }
                }
                else
                {
                    contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Data akun marketplace (JD.ID) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                }
            }
            catch (Exception ex)
            {
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
            }

            return retr;
        }

        //Get Category New
        public async Task<string> JDID_checkAPICustomerShopNew3(JDIDAPIData data)
        {
            string retr = "";

            //var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();

            //DatabaseSQL EDB = new DatabaseSQL(data.DatabasePathErasoft);
            //var resultExecDefault = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST = '" + data.no_cust + "'");

            var sysParams = new Dictionary<string, string>();
            this.ParamJson = "{\"venderId\":\"" + data.merchant_code + "\"}";
            sysParams.Add("360buy_param_json", this.ParamJson);

            sysParams.Add("access_token", data.accessToken);
            sysParams.Add("app_key", data.appKey);
            this.Method = "jingdong.seller.category.api.read.getAllCategory";
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
                //currentLog.REQUEST_RESULT = "Process Get API Token JD.ID";
                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, dataAPI, currentLog);
            }
            catch (WebException ex)
            {
                string err1 = "";
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp1 = ex.Response;
                    using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                    {
                        err1 = sr1.ReadToEnd();
                    }
                }
                //throw new Exception(err1);
            }
            return retr;
        }
        public async Task<string> JDID_checkAPICustomerShopNew(JDIDAPIData data)
        {
            getShopID(data);
            string retr = "";
            string sArrayListLevel = "";
            string sArrayListLevelTempNoDuplicate = "";
            int iLimitCategory = 0;

            var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();

            DatabaseSQL EDB = new DatabaseSQL(data.DatabasePathErasoft);
            var resultExecDefault = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST = '" + data.no_cust + "'");

            var mgrApiManager = new JDIDController();
            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;
            mgrApiManager.Method = "epi.ware.openapi.CategoryApi.getAuthenCategory";
            mgrApiManager.ParamJson = "{\"shopId\":" + shopID_JDID + "}";

            try
            {
                var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (ret != null)
                {
                    if (ret.openapi_code == 0)
                    {
                        var listKategori = JsonConvert.DeserializeObject(ret.openapi_data, typeof(DATA_CAT_NEW)) as DATA_CAT_NEW;
                        if (listKategori != null)
                        {
                            if (listKategori.success)
                            {
                                contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Akun marketplace " + data.email.ToString() + " (JD.ID) berhasil aktif", true);
                                EDB.ExecuteSQL("CString", CommandType.Text, "Update ARF01 SET STATUS_API = '1' WHERE TOKEN = '" + data.accessToken + "' AND API_KEY = '" + data.appKey + "'");
                                //string dbPath = "";
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
                                //var listKtg = ErasoftDbContext.CATEGORY_JDID.ToList();
                                //if (listKtg.Count > 0)
                                //{
                                //    EDB.ExecuteSQL("CString", CommandType.Text, "DELETE FROM CATEGORY_JDID");
                                //}

                                //string con = EDB.ConnectionStrings.FirstOrDefault().Value.ToString();

                                //using (SqlConnection oConnection = new SqlConnection(con))
                                //{
                                //    oConnection.Open();
                                //    //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                                //    //{
                                //    using (SqlCommand oCommand = oConnection.CreateCommand())
                                //    {
                                //        //oCommand.CommandText = "DELETE FROM [CATEGORY_BLIBLI] WHERE ARF01_SORT1_CUST='" + data.merchant_code + "'";
                                //        //oCommand.ExecuteNonQuery();
                                //        //oCommand.Transaction = oTransaction;
                                //        oCommand.CommandType = CommandType.Text;
                                //        oCommand.CommandText = "INSERT INTO [CATEGORY_JDID] ([CATEGORY_CODE], [CATEGORY_NAME], [CATE_STATE], [TYPE], [LEAF], [PARENT_CODE]) VALUES (@CATEGORY_CODE, @CATEGORY_NAME, @CATE_STATE, @TYPE, @LEAF, @PARENT_CODE)";
                                //        //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                                //        oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
                                //        oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));
                                //        oCommand.Parameters.Add(new SqlParameter("@CATE_STATE", SqlDbType.NVarChar, 3));
                                //        oCommand.Parameters.Add(new SqlParameter("@TYPE", SqlDbType.NVarChar, 3));
                                //        oCommand.Parameters.Add(new SqlParameter("@LEAF", SqlDbType.NVarChar, 1));
                                //        oCommand.Parameters.Add(new SqlParameter("@PARENT_CODE", SqlDbType.NVarChar, 50));

                                //        //try
                                //        //{
                                //        foreach (var item in listKategori.model) //foreach parent level 3
                                //        {
                                //            oCommand.Parameters[0].Value = item.id;
                                //            oCommand.Parameters[1].Value = item.name;
                                //            oCommand.Parameters[2].Value = 1;
                                //            oCommand.Parameters[3].Value = item.level;
                                //            if (Convert.ToString(item.parentId) != null)
                                //            {
                                //                oCommand.Parameters[4].Value = "1";
                                //                oCommand.Parameters[5].Value = item.parentId;
                                //            }
                                //            else
                                //            {
                                //                oCommand.Parameters[4].Value = "1";
                                //                oCommand.Parameters[5].Value = "";
                                //            }
                                //            if (oCommand.ExecuteNonQuery() > 0)
                                //            {
                                //                if (Convert.ToString(item.parentId) != null)
                                //                {
                                //                    sArrayListLevel = sArrayListLevel + item.parentId + ",";
                                //                    if (!sArrayListLevelTempNoDuplicate.Contains(Convert.ToString(item.parentId)))
                                //                    {
                                //                        iLimitCategory += 1;
                                //                        sArrayListLevelTempNoDuplicate = sArrayListLevelTempNoDuplicate + item.parentId + ",";
                                //                        if (iLimitCategory == 50)
                                //                        {
                                //                            sArrayListLevelTempNoDuplicate = sArrayListLevelTempNoDuplicate.Substring(0, sArrayListLevelTempNoDuplicate.Length - 1);
                                //                            RecursiveInsertCategoryNewLevel2(oCommand, sArrayListLevelTempNoDuplicate, data);
                                //                            iLimitCategory = 0;
                                //                            sArrayListLevelTempNoDuplicate = "";
                                //                        }
                                //                    }
                                //                }
                                //            }
                                //            else
                                //            {

                                //            }
                                //        }

                                //        if(!string.IsNullOrEmpty(sArrayListLevelTempNoDuplicate))
                                //        {
                                //            sArrayListLevelTempNoDuplicate = sArrayListLevelTempNoDuplicate.Substring(0, sArrayListLevelTempNoDuplicate.Length - 1);
                                //            RecursiveInsertCategoryNewLevel2(oCommand, sArrayListLevelTempNoDuplicate, data);
                                //            iLimitCategory = 0;
                                //            sArrayListLevelTempNoDuplicate = "";
                                //        }

                                //        //oTransaction.Commit();
                                //        //}
                                //        //catch (Exception ex)
                                //        //{
                                //        //    //oTransaction.Rollback();
                                //        //}
                                //    }
                                //}
                            }
                            else
                            {
                                contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Data akun marketplace (JD.ID) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                            }
                        }
                        else
                        {
                            contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Data akun marketplace (JD.ID) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                        }
                    }
                    else
                    {
                        contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Data akun marketplace (JD.ID) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                    }
                }
                else
                {
                    contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Data akun marketplace (JD.ID) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                }
            }
            catch (Exception ex)
            {
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
            }

            return retr;
        }

        [System.Web.Mvc.HttpGet]
        public void getCategory(JDIDAPIData data)
        {

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //    REQUEST_ACTION = "Get Category",
            //    REQUEST_DATETIME = DateTime.Now,
            //    REQUEST_ATTRIBUTE_1 = data.accessToken,
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            var mgrApiManager = new JDIDController();
            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;
            mgrApiManager.Method = "epi.ware.openapi.CategoryApi.getAllCategoryTree";
            mgrApiManager.ParamJson = "";

            try
            {
                var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (ret != null)
                {
                    if (ret.openapi_code == 0)
                    {
                        var listKategori = JsonConvert.DeserializeObject(ret.openapi_data, typeof(DATA_CAT)) as DATA_CAT;
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

                                var listKtg = ErasoftDbContext.CATEGORY_JDID.ToList();
                                if (listKtg.Count > 0)
                                {
                                    EDB.ExecuteSQL("CString", CommandType.Text, "DELETE FROM CATEGORY_JDID");
                                }
                                #region connstring
#if AWS
                    string con = "Data Source=localhost;Initial Catalog=" + dbPath + ";Persist Security Info=True;User ID=sa;Password=admin123^";
#elif Debug_AWS
                                string con = "Data Source=13.250.232.74;Initial Catalog=" + dbPath + ";Persist Security Info=True;User ID=sa;Password=admin123^";
#else
                                string con = "Data Source=13.251.222.53;Initial Catalog=" + dbPath + ";Persist Security Info=True;User ID=sa;Password=admin123^";
#endif
                                #endregion
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
                                //currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                                //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            }
                        }
                        else
                        {
                            //currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                            //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        }
                    }
                }
                else
                {
                    //currentLog.REQUEST_EXCEPTION = response;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                }
            }
            catch (Exception ex)
            {
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
            }


        }
        protected void RecursiveInsertCategory(SqlCommand oCommand, Model_Cat item)
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

        protected void RecursiveInsertCategoryNewLevel2(SqlCommand oCommand, string listCategoryID, JDIDAPIData data)
        {
            string retr = "";
            string sArrayListLevel = "";
            string sArrayListLevelTempNoDuplicate = "";
            int iLimitCategory = 0;

            var mgrApiManager = new JDIDController();
            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;
            mgrApiManager.Method = "epi.ware.openapi.CategoryApi.getCategoryByCatIds";
            mgrApiManager.ParamJson = "{\"catIds\":\"" + listCategoryID + "\"}";


            try
            {
                var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (ret != null)
                {
                    if (ret.openapi_code == 0)
                    {
                        var listKategori = JsonConvert.DeserializeObject(ret.openapi_data, typeof(DATA_CAT_NEW)) as DATA_CAT_NEW;
                        if (listKategori != null)
                        {
                            if (listKategori.success)
                            {
                                foreach (var item in listKategori.model) //foreach parent level next
                                {
                                    if (item.level == 2)
                                    {
                                        oCommand.Parameters[0].Value = item.id;
                                        oCommand.Parameters[1].Value = item.name;
                                        oCommand.Parameters[2].Value = 1;
                                        oCommand.Parameters[3].Value = item.level;
                                        if (Convert.ToString(item.parentId) != null)
                                        {
                                            oCommand.Parameters[4].Value = "0";
                                            oCommand.Parameters[5].Value = item.parentId;
                                        }
                                        else
                                        {
                                            oCommand.Parameters[4].Value = "1";
                                            oCommand.Parameters[5].Value = "";
                                        }

                                        if (oCommand.ExecuteNonQuery() > 0)
                                        {
                                            if (Convert.ToString(item.parentId) != null)
                                            {
                                                sArrayListLevel = sArrayListLevel + item.parentId + ",";
                                                if (!sArrayListLevelTempNoDuplicate.Contains(Convert.ToString(item.parentId)))
                                                {
                                                    iLimitCategory += 1;
                                                    sArrayListLevelTempNoDuplicate = sArrayListLevelTempNoDuplicate + item.parentId + ",";
                                                    if (iLimitCategory == 10)
                                                    {
                                                        sArrayListLevelTempNoDuplicate = sArrayListLevelTempNoDuplicate.Substring(0, sArrayListLevelTempNoDuplicate.Length - 1);
                                                        RecursiveInsertCategoryNewLevel1(oCommand, sArrayListLevelTempNoDuplicate, data);
                                                        iLimitCategory = 0;
                                                        sArrayListLevelTempNoDuplicate = "";
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {

                                        }

                                    }
                                }

                                if (!string.IsNullOrEmpty(sArrayListLevelTempNoDuplicate))
                                {
                                    sArrayListLevelTempNoDuplicate = sArrayListLevelTempNoDuplicate.Substring(0, sArrayListLevelTempNoDuplicate.Length - 1);
                                    RecursiveInsertCategoryNewLevel1(oCommand, sArrayListLevelTempNoDuplicate, data);
                                    iLimitCategory = 0;
                                    sArrayListLevelTempNoDuplicate = "";
                                }
                            }
                            else
                            {
                                //contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Data akun marketplace (JD.ID) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                            }
                        }
                        else
                        {
                            //contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Data akun marketplace (JD.ID) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                        }
                    }
                    else
                    {
                        //contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Data akun marketplace (JD.ID) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
            }
        }

        protected void RecursiveInsertCategoryNewLevel1(SqlCommand oCommand, string listCategoryID, JDIDAPIData data)
        {
            var mgrApiManager = new JDIDController();
            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;
            mgrApiManager.Method = "epi.ware.openapi.CategoryApi.getCategoryByCatIds";
            mgrApiManager.ParamJson = "{\"catIds\":\"" + listCategoryID + "\"}";


            try
            {
                var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (ret != null)
                {
                    if (ret.openapi_code == 0)
                    {
                        var listKategori = JsonConvert.DeserializeObject(ret.openapi_data, typeof(DATA_CAT_NEW)) as DATA_CAT_NEW;
                        if (listKategori != null)
                        {
                            if (listKategori.success)
                            {
                                foreach (var item in listKategori.model) //foreach parent level next
                                {
                                    if (item.level == 1)
                                    {
                                        oCommand.Parameters[0].Value = item.id;
                                        oCommand.Parameters[1].Value = item.name;
                                        oCommand.Parameters[2].Value = 1;
                                        oCommand.Parameters[3].Value = item.level;
                                        oCommand.Parameters[4].Value = "0";
                                        oCommand.Parameters[5].Value = "";
                                        //if (Convert.ToString(item.parentId) != null)
                                        //{
                                        //    //oCommand.Parameters[5].Value = item.parentId;
                                        //}
                                        //else
                                        //{
                                        //    oCommand.Parameters[5].Value = "";
                                        //}

                                        if (oCommand.ExecuteNonQuery() > 0)
                                        {
                                            //if (Convert.ToString(item.parentId) != null)
                                            //{
                                            //    sArrayListLevel = sArrayListLevel + item.parentId + ",";
                                            //    if (!sArrayListLevelTempNoDuplicate.Contains(Convert.ToString(item.parentId)))
                                            //    {
                                            //        iLimitCategory += 1;
                                            //        sArrayListLevelTempNoDuplicate = sArrayListLevelTempNoDuplicate + item.parentId + ",";
                                            //        sArrayListLevelTempNoDuplicate = sArrayListLevelTempNoDuplicate.Substring(0, sArrayListLevelTempNoDuplicate.Length - 1);
                                            //        RecursiveInsertCategoryNewLevel1(oCommand, sArrayListLevelTempNoDuplicate, data);
                                            //        iLimitCategory = 0;
                                            //        sArrayListLevelTempNoDuplicate = "";
                                            //    }
                                            //    else
                                            //    {
                                            //        statusLoop = false;
                                            //    }
                                            //}
                                        }
                                        else
                                        {

                                        }
                                    }
                                }

                                //if (statusLoop == false && !string.IsNullOrEmpty(sArrayListLevelTempNoDuplicate))
                                //{
                                //    sArrayListLevelTempNoDuplicate = sArrayListLevelTempNoDuplicate.Substring(0, sArrayListLevelTempNoDuplicate.Length - 1);
                                //    RecursiveInsertCategoryNewLevel1(oCommand, sArrayListLevelTempNoDuplicate, data);
                                //    iLimitCategory = 0;
                                //    sArrayListLevelTempNoDuplicate = "";
                                //}
                            }
                            else
                            {
                                //contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Data akun marketplace (JD.ID) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                            }
                        }
                        else
                        {
                            //contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Data akun marketplace (JD.ID) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                        }
                    }
                    else
                    {
                        //contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Data akun marketplace (JD.ID) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
            }
        }

        public ATTRIBUTE_JDID_LAMA getAttribute(JDIDAPIData data, string catId)
        {
            var retAttr = new ATTRIBUTE_JDID_LAMA();
            var mgrApiManager = new JDIDController();

            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;

            mgrApiManager.Method = "epi.ware.openapi.WareAttributeApi.getAttributesByCatId";
            mgrApiManager.ParamJson = "{ \"catId\":\"" + catId + "\"}";

            var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
            var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
            if (ret != null)
            {
                if (ret.openapi_code == 0)
                {
                    var listAttr = JsonConvert.DeserializeObject(ret.openapi_data, typeof(AttrData)) as AttrData;
                    if (listAttr != null)
                    {
                        if (listAttr.model.Count > 0)
                        {
                            string a = "";
                            int i = 2;
                            retAttr.CATEGORY_CODE = catId;

                            retAttr["ACODE_1"] = "9170";
                            retAttr["AVALUE_1"] = "";
                            retAttr["ANAME_1"] = "Warna";

                            retAttr["ACODE_2"] = "9248";
                            retAttr["AVALUE_2"] = "";
                            retAttr["ANAME_2"] = "Ukuran";

                            foreach (var attr in listAttr.model)
                            {

                                //a = Convert.ToString(i + 1);
                                //retAttr["ACODE_" + a] = Convert.ToString(attr.propertyId);
                                //retAttr["AVALUE_" + a] = attr.type.ToString();
                                //retAttr["ANAME_" + a] = attr.nameEn;
                                //i = i + 1;
                                if (!string.IsNullOrEmpty(attr.name) && !string.IsNullOrEmpty(Convert.ToString(attr.propertyId)))
                                {

                                    if (!attr.name.Contains("Coming Soon")
                                        //&& !attr.name.Contains("Warna") 
                                        //&& !attr.name.Contains("Ukuran") 
                                        && !attr.name.Contains("Test11")
                                        //&& !attr.name.Contains("Network") 
                                        && !attr.name.Contains("Operating system")
                                        && !attr.name.Contains("Upgradable")
                                        //&& !attr.name.Contains("OS Upgrade to") 
                                        //&& !attr.name.Contains("Chipset") 
                                        //&& !attr.name.Contains("CPU")
                                        && !attr.name.Contains("GPU")
                                        //&& !attr.name.Contains("RAM") 
                                        //&& !attr.name.Contains("Memory Internal")
                                        //&& !attr.name.Contains("Memory External") 
                                        //&& !attr.name.Contains("Rear Camera 1") 
                                        //&& !attr.name.Contains("Rear Camera 2")
                                        //&& !attr.name.Contains("Rear Camera 3") 
                                        //&& !attr.name.Contains("Rear Camera 4") 
                                        //&& !attr.name.Contains("Front Camera 1")
                                        && !attr.name.Contains("Front Camera 2")
                                        && !attr.name.Contains("Video")
                                        //&& !attr.name.Contains("Battery Type")
                                        //&& !attr.name.Contains("Removable Battery") 
                                        //&& !attr.name.Contains("Battery Capacity") 
                                        && !attr.name.Contains("LCD Size")
                                        //&& !attr.name.Contains("LCD Type") 
                                        //&& !attr.name.Contains("Screen Resolution") && !attr.name.Contains("Dimensions")
                                        //&& !attr.name.Contains("Sensor") 
                                        //&& !attr.name.Contains("SIM Card") 
                                        //&& !attr.name.Contains("WLAN")
                                        //&& !attr.name.Contains("NFC") 
                                        && !attr.name.Contains("ROM")
                                        && !attr.name.Contains("Megapixel(MP)")
                                        && !attr.name.Contains("MicroSD")
                                        //&& !attr.name.Contains("OS") 
                                        && !attr.name.Contains("Secondary")
                                        && !attr.name.Contains("Primary")
                                        && !attr.name.Contains("GPRS")
                                        && !attr.name.Contains("Multitouch")
                                        && !attr.name.Contains("EDGE")
                                        //&& !attr.name.Contains("Dual SIM") 
                                        && !attr.name.Contains("Screen size (inch)")
                                        //&& !attr.name.Contains("Price") 
                                        //&& !attr.name.Contains("Kapasitas")
                                        )
                                    {
                                        if (i < 20)
                                        {
                                            a = Convert.ToString(i + 1);
                                            retAttr["ACODE_" + a] = Convert.ToString(attr.propertyId) ?? "";
                                            retAttr["AVALUE_" + a] = attr.type.ToString() ?? "";
                                            retAttr["ANAME_" + a] = attr.nameEn ?? "";
                                            i = i + 1;
                                        }
                                    }
                                }
                            }
                            for (int j = i; j < 20; j++)
                            {
                                a = Convert.ToString(j + 1);
                                retAttr["ACODE_" + a] = "";
                                retAttr["AVALUE_" + a] = "";
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

        public List<ATTRIBUTE_OPT_JDID_LAMA> getAttributeOpt(JDIDAPIData data, string catId, string attrId, int page)
        {
            var mgrApiManager = new JDIDController();
            var listOpt = new List<ATTRIBUTE_OPT_JDID_LAMA>();
            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;

            mgrApiManager.Method = "epi.ware.openapi.WareAttributeApi.getAttrValuesByCatIdAndAttrId";
            mgrApiManager.ParamJson = "{ \"catId\":\"" + catId + "\", \"attrId\":\"" + attrId + "\", \"currentPage\":" + page + ", \"pageSize\":20}";

            var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
            var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
            if (ret != null)
            {
                if (ret.openapi_code == 0)
                {
                    var retOpt = JsonConvert.DeserializeObject(ret.openapi_data, typeof(JDID_ATTRIBUTE_OPT)) as JDID_ATTRIBUTE_OPT;
                    if (retOpt != null)
                    {
                        if (retOpt.model != null)
                        {
                            if (retOpt.model.data.Count > 0)
                            {
                                foreach (var opt in retOpt.model.data)
                                {
                                    var newOpt = new ATTRIBUTE_OPT_JDID_LAMA()
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

        // add by fauzi for get name brand in sinkro and master barang
        public string getBrandByShopID(JDIDAPIData data, string shopID, int brandID)
        {
            var retResult = "";
            var mgrApiManager = new JDIDController();

            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;

            mgrApiManager.Method = "epi.shop.center.sdk.service.outer.ShopSdk.getBrandByShopId";
            mgrApiManager.ParamJson = "{ \"shopId\":" + shopID + "}";

            var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
            var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
            if (ret != null)
            {
                if (ret.openapi_code == 0 && ret.openapi_msg == "success" && ret.openapi_data != null)
                {
                    var listAttr = JsonConvert.DeserializeObject(ret.openapi_data, typeof(Data_Detail_Brand)) as Data_Detail_Brand;
                    if (listAttr != null)
                    {
                        if (listAttr.model.Count > 0)
                        {
                            foreach (var itemBrand in listAttr.model)
                            {
                                if (itemBrand.brandId == brandID)
                                {
                                    retResult = itemBrand.brandDto.nameInd;
                                }
                            }
                        }

                    }
                }
            }
            return retResult;
        }

        public async Task<BindingBase> getListProduct(JDIDAPIData data, int page, string cust, int recordCount, int totalData)
        {
            var ret = new BindingBase
            {
                status = 0,
                recordCount = recordCount,
                exception = 0,
                totalData = totalData//add 18 Juli 2019, show total record
            };

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Item List",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = cust,
                REQUEST_ATTRIBUTE_3 = page.ToString(),
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            try
            {
                if (listBrand.Count == 0)
                {
                    getShopBrand(data);
                }
                var mgrApiManager = new JDIDController();
                mgrApiManager.AppKey = data.appKey;
                mgrApiManager.AppSecret = data.appSecret;
                mgrApiManager.AccessToken = data.accessToken;
                mgrApiManager.Method = "com.jd.eptid.warecenter.api.ware.WarePlusClient.getWareTinyInfoListByVenderId";
                mgrApiManager.ParamJson = (page + 1) + ",10";

                var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
                var retData = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (retData != null)
                {
                    if (retData.openapi_msg.ToLower() == "success")
                    {
                        var listProd = JsonConvert.DeserializeObject(retData.openapi_data, typeof(Data_ListProd)) as Data_ListProd;
                        if (listProd != null)
                        {
                            if (listProd.success)
                            {
                                string msg = "";
                                bool adaError = false;
                                if (listProd.model.spuInfoVoList == null)
                                {
                                    ret.status = 1;
                                    ret.nextPage = 0;
                                    return ret;
                                }
                                if (listProd.model.spuInfoVoList.Count > 0)
                                {
                                    ret.status = 1;
                                    int IdMarket = ErasoftDbContext.ARF01.Where(c => c.CUST == cust).FirstOrDefault().RecNum.Value;
                                    if (listProd.model.spuInfoVoList.Count == 10)
                                        //ret.message = (page + 1).ToString();
                                        ret.nextPage = 1;
                                    var spu = "";
                                    foreach (var item in listProd.model.spuInfoVoList)
                                    {
                                        //product status: 1.online,2.offline,3.punish,4.deleted
                                        if (item.wareStatus == 1 || item.wareStatus == 2)
                                        {
                                            var retProd = await GetProduct(data, item, IdMarket, cust);
                                            ret.totalData += retProd.totalData;//add 18 Juli 2019, show total record
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
                                        else
                                        {
                                            spu = spu + item.spuId + ",";
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
                else
                {
                    ret.exception = 1;
                    currentLog.REQUEST_EXCEPTION = "failed to call api";
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);

                }
            }
            catch (Exception ex)
            {
                ret.nextPage = 1;
                ret.exception = 1;
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
            }
            return ret;
        }

        public async Task<BindingBase> GetProduct(JDIDAPIData data, Spuinfovolist itemFromList, int IdMarket, string cust)
        {
            var ret = new BindingBase
            {
                status = 0,
                exception = 0,
                totalData = 0,//add 18 Juli 2019, show total record
            };

            try
            {
                var mgrApiManager = new JDIDController();
                mgrApiManager.AppKey = data.appKey;
                mgrApiManager.AppSecret = data.appSecret;
                mgrApiManager.AccessToken = data.accessToken;
                mgrApiManager.Method = "com.jd.eptid.warecenter.api.ware.WarePlusClient.getSkuInfoBySpuId";
                mgrApiManager.ParamJson = "[" + itemFromList.spuId + "]";

                var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
                var retProd = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (retProd != null)
                {
                    if (retProd.openapi_msg.ToLower() == "success")
                    {
                        var dataProduct = JsonConvert.DeserializeObject(retProd.openapi_data, typeof(ProductData)) as ProductData;
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
                                    ret.totalData += 1;//add 18 Juli 2019, show total record
                                }
                                ret.totalData += dataProduct.model.Count();//add 18 Juli 2019, show total record
                                foreach (var item in dataProduct.model)
                                {
                                    var tempbrginDB = new TEMP_BRG_MP();
                                    var brgInDB = new STF02H();

                                    if (haveVarian)
                                    {
                                        //handle parent
                                        string kdBrgInduk = item.spuId.ToString();
                                        bool createParent = false;
                                        tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == kdBrgInduk + ";0").FirstOrDefault();
                                        brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == kdBrgInduk + ";0").FirstOrDefault();
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
                                        tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.spuId.ToString() + ";" + item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.spuId.ToString() + ";" + item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        if (tempbrginDB == null && brgInDB == null)
                                        {
                                            var retData = await getProductDetail(data, item, kdBrgInduk, createParent, item.skuId.ToString(), cust, IdMarket, itemFromList);
                                            if (retData.exception == 1)
                                                ret.exception = 1;
                                            if (retData.status == 1)
                                            {
                                                ret.recordCount += retData.recordCount;
                                                //createParent = false;
                                            }
                                        }
                                        else
                                        {
                                            if (createParent)
                                            {
                                                var retDataParent = getProductDetailParentOnly(data, item, kdBrgInduk, createParent, item.skuId.ToString(), cust, IdMarket, itemFromList);
                                                if (retDataParent.exception == 1)
                                                    ret.exception = 1;
                                                if (retDataParent.status == 1)
                                                {
                                                    ret.recordCount += retDataParent.recordCount;
                                                    //createParent = false;
                                                }
                                            }
                                            var datasudahada = item.skuId.ToString(); // breakpoint
                                        }
                                        //}

                                    }
                                    else
                                    {
                                        tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.spuId.ToString() + ";" + item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.spuId.ToString() + ";" + item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        if (tempbrginDB == null && brgInDB == null)
                                        {
                                            var retData = await getProductDetail(data, item, "", false, item.skuId.ToString(), cust, IdMarket, itemFromList);
                                            if (retData.exception == 1)
                                                ret.exception = 1;
                                            if (retData.status == 1)
                                            {
                                                ret.recordCount += retData.recordCount;
                                            }
                                        }
                                        else
                                        {
                                            var datasudahada = item.skuId.ToString(); // breakpoint
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
                else
                {
                    ret.exception = 1;
                }
            }
            catch (Exception ex)
            {
                ret.exception = 1;
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }

            return ret;
        }

        public async Task<BindingBase> getProductDetail(JDIDAPIData data, Model_Product item, string kdBrgInduk, bool createParent, string skuId, string cust, int IdMarket, Spuinfovolist itemFromList)
        {
            var ret = new BindingBase
            {
                status = 0,
                exception = 0
            };

            try
            {
                var mgrApiManager = new JDIDController();
                mgrApiManager.AppKey = data.appKey;
                mgrApiManager.AppSecret = data.appSecret;
                mgrApiManager.AccessToken = data.accessToken;
                mgrApiManager.Method = "epi.ware.openapi.SkuApi.getSkuBySkuIds";
                //mgrApiManager.ParamJson = "{ \"page\":" + page + ", \"pageSize\":10}";
                mgrApiManager.ParamJson = "{\"skuIds\" : \"" + skuId + "\"}";

                string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, BERAT, PANJANG, LEBAR, TINGGI, CUST, Deskripsi, IDMARKET, HJUAL, HJUAL_MP, ";
                sSQL += "DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, IMAGE4, IMAGE5, KODE_BRG_INDUK, TYPE, ";
                sSQL += "ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
                sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20) VALUES ";

                string sSQLVal = "";

                //if (!string.IsNullOrEmpty(kdBrgInduk))
                //{
                var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
                var retProd = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (retProd != null)
                {
                    if (retProd.openapi_msg.ToLower() == "success")
                    {
                        var detailData = JsonConvert.DeserializeObject(retProd.openapi_data, typeof(Data_Detail_Product)) as Data_Detail_Product;
                        if (detailData != null)
                        {
                            if (detailData.success)
                            {
                                if (!string.IsNullOrEmpty(kdBrgInduk))
                                {
                                    if (createParent)
                                    {
                                        var retSQL = CreateSQLValue(data, item, detailData.model[0], kdBrgInduk, "", cust, IdMarket, 1, itemFromList);
                                        if (retSQL.exception == 1)
                                            ret.exception = 1;
                                        if (retSQL.status == 1)
                                            sSQLVal += retSQL.message;
                                    }

                                    var retSQL2 = CreateSQLValue(data, item, detailData.model[0], kdBrgInduk, skuId, cust, IdMarket, 2, itemFromList);
                                    if (retSQL2.exception == 1)
                                        ret.exception = 1;
                                    if (retSQL2.status == 1)
                                        sSQLVal += retSQL2.message;
                                }
                                else
                                {
                                    var retSQL = CreateSQLValue(data, item, detailData.model[0], "", skuId, cust, IdMarket, 0, itemFromList);
                                    if (retSQL.exception == 1)
                                        ret.exception = 1;
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
                    ret.exception = 1;
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
                ret.exception = 1;
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }

        public BindingBase getProductDetailParentOnly(JDIDAPIData data, Model_Product item, string kdBrgInduk, bool createParent, string skuId, string cust, int IdMarket, Spuinfovolist itemFromList)
        {
            var ret = new BindingBase
            {
                status = 0,
                exception = 0
            };

            try
            {
                var mgrApiManager = new JDIDController();
                mgrApiManager.AppKey = data.appKey;
                mgrApiManager.AppSecret = data.appSecret;
                mgrApiManager.AccessToken = data.accessToken;
                mgrApiManager.Method = "epi.ware.openapi.SkuApi.getSkuBySkuIds";
                //mgrApiManager.ParamJson = "{ \"page\":" + page + ", \"pageSize\":10}";
                mgrApiManager.ParamJson = "{\"skuIds\" : \"" + skuId + "\"}";

                string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, BERAT, PANJANG, LEBAR, TINGGI, CUST, Deskripsi, IDMARKET, HJUAL, HJUAL_MP, ";
                sSQL += "DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, IMAGE4, IMAGE5, KODE_BRG_INDUK, TYPE, ";
                sSQL += "ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
                sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20) VALUES ";

                string sSQLVal = "";

                //if (!string.IsNullOrEmpty(kdBrgInduk))
                //{
                var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
                var retProd = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (retProd != null)
                {
                    if (retProd.openapi_msg.ToLower() == "success")
                    {
                        var detailData = JsonConvert.DeserializeObject(retProd.openapi_data, typeof(Data_Detail_Product)) as Data_Detail_Product;
                        if (detailData != null)
                        {
                            if (detailData.success)
                            {
                                if (!string.IsNullOrEmpty(kdBrgInduk))
                                {
                                    if (createParent)
                                    {
                                        var retSQL = CreateSQLValue(data, item, detailData.model[0], kdBrgInduk, "", cust, IdMarket, 1, itemFromList);
                                        if (retSQL.exception == 1)
                                            ret.exception = 1;
                                        if (retSQL.status == 1)
                                            sSQLVal += retSQL.message;
                                    }
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
                    ret.exception = 1;
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
                ret.exception = 1;
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }

        protected BindingBase CreateSQLValue(JDIDAPIData data, Model_Product item, Model_Detail_Product detItem, string kdBrgInduk, string skuId, string cust, int IdMarket, int typeBrg, Spuinfovolist itemFromList)
        {
            // typeBrg : 0 = barang tanpa varian; 1 = barang induk; 2 = barang varian
            var ret = new BindingBase
            {
                status = 0,
                exception = 0
            };

            string sSQL_Value = "";
            try
            {

                string[] attrVal;
                string value = "";
                var brgAttribute = new Dictionary<string, string>();
                string namaBrg = item.skuName;
                string nama, nama2, urlImage, urlImage2, urlImage3, urlImage4, urlImage5;
                var namaTemp = "";
                urlImage = "";
                urlImage2 = "";
                urlImage3 = "";
                urlImage4 = "";
                urlImage5 = "";
                var categoryCode = itemFromList.fullCategoryId.Split('/');
                var categoryName = itemFromList.fullCategoryName.Split('/');
                double price = Convert.ToDouble(item.jdPrice);
                //var statusBrg = detItem != null ? detItem.status : 1;
                var statusBrg = detItem.status;
                string brand = "";
                if (itemFromList.brandId.ToString() != null)
                {
                    brand = getBrandByShopID(data, itemFromList.shopId.ToString(), Convert.ToInt32(itemFromList.brandId));
                }

                //var display = statusBrg.Equals("active") ? 1 : 0;
                string deskripsi = itemFromList.description;

                var typeItemCode = "";
                var typeItemDesc = "";
                if (!string.IsNullOrEmpty(Convert.ToString(item.piece)))
                {
                    typeItemCode = Convert.ToString(item.piece);
                    switch (typeItemCode)
                    {
                        case "0":
                            typeItemDesc = "Small item <= 20KG";
                            break;
                        case "1":
                            typeItemDesc = "Big item > 20KG";
                            break;
                        default:
                            typeItemDesc = "";
                            break;
                    }
                }

                var afterSaleCode = "";
                var afterSaleDesc = "";
                if (!string.IsNullOrEmpty(Convert.ToString(itemFromList.afterSale)))
                {
                    afterSaleCode = Convert.ToString(itemFromList.afterSale);
                    switch (afterSaleCode)
                    {
                        case "1":
                            afterSaleDesc = "Only Support 7 Days Refund";
                            break;
                        case "2":
                            afterSaleDesc = "Not Support 7 Days Refund And 15 Days Exchange";
                            break;
                        case "3":
                            afterSaleDesc = "Support 7 Days Refund And 15 Days Exchange";
                            break;
                        case "4":
                            afterSaleDesc = "Only Support 15 Days Exchange";
                            break;
                        default:
                            afterSaleDesc = "";
                            break;
                    }
                }

                var warrantyCode = "";
                var warrantyDesc = "";
                if (!string.IsNullOrEmpty(Convert.ToString(itemFromList.warrantyPeriod)))
                {
                    warrantyCode = Convert.ToString(itemFromList.warrantyPeriod);
                    switch (warrantyCode)
                    {
                        case "1":
                            warrantyDesc = "No warranty";
                            break;
                        case "50":
                            warrantyDesc = "6 months official warranty";
                            break;
                        case "51":
                            warrantyDesc = "8 months official warranty";
                            break;
                        case "52":
                            warrantyDesc = "18 months official warranty";
                            break;
                        case "2":
                            warrantyDesc = "1 year official warranty";
                            break;
                        case "3":
                            warrantyDesc = "2 year official warranty";
                            break;
                        case "4":
                            warrantyDesc = "3 year official warranty";
                            break;
                        case "11":
                            warrantyDesc = "4 year official warranty";
                            break;
                        case "12":
                            warrantyDesc = "5 year official warranty";
                            break;
                        case "5":
                            warrantyDesc = "1 year shop warranty";
                            break;
                        case "6":
                            warrantyDesc = "2 year shop warranty";
                            break;
                        case "7":
                            warrantyDesc = "3 year shop warranty";
                            break;
                        case "21":
                            warrantyDesc = "4 year shop warranty";
                            break;
                        case "22":
                            warrantyDesc = "5 year shop warranty";
                            break;
                        case "31":
                            warrantyDesc = "1 year compressor warranty";
                            break;
                        case "32":
                            warrantyDesc = "2 year compressor warranty";
                            break;
                        case "33":
                            warrantyDesc = "3 year compressor warranty";
                            break;
                        case "35":
                            warrantyDesc = "5 year compressor warranty";
                            break;
                        case "30":
                            warrantyDesc = "10 year compressor warranty";
                            break;
                        case "41":
                            warrantyDesc = "1 year motor warranty";
                            break;
                        case "42":
                            warrantyDesc = "2 year motor warranty";
                            break;
                        case "43":
                            warrantyDesc = "3 year motor warranty";
                            break;
                        case "45":
                            warrantyDesc = "5 year motor warranty";
                            break;
                        case "40":
                            warrantyDesc = "10 year motor warranty";
                            break;
                        case "8":
                            warrantyDesc = "Lifetime warranty";
                            break;
                        default:
                            warrantyDesc = "";
                            break;
                    }
                }



                if (typeBrg != 1)
                {
                    //change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                    //if (!string.IsNullOrEmpty(detItem.sellerSkuId.ToString()) && detItem.sellerSkuId.ToString() != "null")
                    // change by fauzi 03/09/2020 tambah validasi untuk seller sku dengan isi null
                    var sellerSKUAPI = Convert.ToString(detItem.sellerSkuId);
                    if (!string.IsNullOrEmpty(sellerSKUAPI) && sellerSKUAPI != "null")
                    {
                        sSQL_Value += " ( '" + item.spuId + ";" + skuId + "' , '" + sellerSKUAPI + "' , '";
                    }
                    else
                    {
                        sSQL_Value += " ( '" + item.spuId + ";" + skuId + "' , '" + skuId + "' , '";
                    }
                    //end changed by fauzi 03/09/2020
                    //sSQL_Value += " ( '" + skuId + "' , '' , '";
                    //end change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                }
                else
                {
                    namaBrg = itemFromList.spuName;
                    //change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                    //sSQL_Value += " ( '" + kdBrgInduk + "' , '" + kdBrgInduk + "' , '";
                    sSQL_Value += " ( '" + kdBrgInduk + ";0" + "' , '' , '";
                    //end change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
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
                    string[] ssplitNama = namaBrg.Split(' ');
                    nama = ssplitNama[0];

                    int c;
                    for (c = 1; c < ssplitNama.Length; c++)
                    {
                        namaTemp = namaTemp + ssplitNama[c] + " ";
                    }

                    //if (ssplitNama.Length >= 2)
                    //{
                    //    nama = ssplitNama[0] + " " + ssplitNama[1] + " " + ssplitNama[2];
                    //    nama2 = namaBrg.in(ssplitNama[3], );
                    //}
                    //else
                    //{
                    //    nama = ssplitNama[0] + " " + ssplitNama[1];
                    //}

                    //nama = namaBrg.Substring(0, 30);
                    //if (namaBrg.Length > 285)
                    //{
                    //    nama2 = namaBrg.Substring(30, 255);
                    //}
                    //else
                    //{
                    //    nama2 = namaBrg.Substring(30);
                    //}
                    nama2 = namaTemp;
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
                //if (detItem.saleAttributeNameMap != null){
                //    if(detItem.saleAttributeNameMap.Count > 0)
                //    {

                //    }
                //}

                //price = brgAttribute.TryGetValue("jdPrice", out value) ? Convert.ToDouble(value) : Convert.ToDouble(item.jdPrice);
                if (Convert.ToDouble(detItem.jdPrice) > 0)
                    price = Convert.ToDouble(detItem.jdPrice);

                //}
                //if (listBrand.Count > 0)
                //{
                //    var a = listBrand.Where(m => m.brandId == itemFromList.brandId).FirstOrDefault();
                //    if (a != null)
                //        brand = a.brandName;
                //}
                //sSQL_Value += Convert.ToDouble(detItem != null ? detItem.netWeight : item.netWeight) * 1000 + " , " + Convert.ToDouble(detItem != null ? detItem.packLong : item.packLong) + " , ";
                //sSQL_Value += Convert.ToDouble(detItem != null ? detItem.packWide : item.packWide) + " , " + Convert.ToDouble(detItem != null ? detItem.packHeight : item.packHeight);
                sSQL_Value += Convert.ToDouble(detItem.netWeight) * 1000 + " , " + Convert.ToDouble(detItem.packLong) + " , ";
                sSQL_Value += Convert.ToDouble(detItem.packWide) + " , " + Convert.ToDouble(detItem.packHeight);
                sSQL_Value += " , '" + cust + "' , '" + deskripsi.Replace('\'', '`') + "' , " + IdMarket + " , " + price + " , " + price + " , ";
                sSQL_Value += statusBrg + " , '" + categoryCode[categoryCode.Length - 1] + "' , '" + categoryName[categoryName.Length - 1] + "' , '";
                //change by nurul 9/6/2021
                //sSQL_Value += brand + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "' , '" + urlImage4 + "' , '" + urlImage5 + "' , '" + (typeBrg == 2 ? kdBrgInduk : "") + "' , '" + (typeBrg == 1 ? "4" : "3") + "'";
                var kd_brg_induk = "";
                if (kdBrgInduk == Convert.ToString(item.spuId))
                {
                    kd_brg_induk = kdBrgInduk + ";0";
                }
                else
                {
                    kd_brg_induk = kdBrgInduk;
                }
                sSQL_Value += brand + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "' , '" + urlImage4 + "' , '" + urlImage5 + "' , '" + (typeBrg == 2 ? kd_brg_induk : "") + "' , '" + (typeBrg == 1 ? "4" : "3") + "'";
                //end change by nurul 9/6/2021
                int i;
                if (!string.IsNullOrEmpty(detItem.saleAttributeIds) && detItem.saleAttributeIds != "null")
                {
                    attrVal = detItem.saleAttributeIds.Split(';');

                    foreach (Newtonsoft.Json.Linq.JProperty property in detItem.saleAttributeNameMap)
                    {
                        brgAttribute.Add(property.Name, property.Value.ToString());
                    }

                    for (i = 0; i < attrVal.Length; i++)
                    {
                        var attr = attrVal[i].Split(':');
                        if (attr.Length == 2)
                        {
                            if (brgAttribute.Count > 0)
                            {
                                var attrName = (brgAttribute.TryGetValue(attrVal[i], out value) ? value : "").Split(':');

                                sSQL_Value += ",'" + attr[0] + "','" + attrName[0] + "','" + attr[1] + "'";
                            }
                            else
                            {
                                sSQL_Value += ",'','',''";
                            }
                        }
                        else
                        {
                            sSQL_Value += ",'','',''";
                        }
                    }

                    for (int j = i; j < 20; j++)
                    {
                        if (j == 17)
                        {
                            //ACODE_18, ANAME_18, AVALUE_18 for piece / tipe barang  Small item ≤20KG / Big item > 20KG
                            //if (typeBrg != 2)
                            //{
                            sSQL_Value += ",'" + typeItemCode + "','typeitem','" + typeItemDesc + "'";
                            //}
                            //else
                            //{
                            //    sSQL_Value += ",'','',''";
                            //}
                        }
                        else if (j == 18)
                        {
                            //ACODE_19, ANAME_19, AVALUE_19 for aftersale
                            //if (typeBrg != 2)
                            //{
                            sSQL_Value += ",'" + afterSaleCode + "','aftersale','" + afterSaleDesc + "'";
                            //}
                            //else
                            //{
                            //    sSQL_Value += ",'','',''";
                            //}
                        }
                        else if (j == 19)
                        {
                            //ACODE_20, ANAME_20, AVALUE_20 for warranty
                            //if (typeBrg != 2)
                            //{
                            sSQL_Value += ",'" + warrantyCode + "','warranty','" + warrantyDesc + "'";
                            //}
                            //else
                            //{
                            //    sSQL_Value += ",'','',''";
                            //}                                
                        }
                        else
                        {
                            sSQL_Value += ",'','',''";
                        }
                    }
                }
                else
                {
                    sSQL_Value += ",'','',''";
                    for (int j = 1; j < 20; j++)
                    {
                        if (j == 17)
                        {
                            //ACODE_18, ANAME_18, AVALUE_18 for piece / tipe barang  Small item ≤20KG / Big item > 20KG
                            //if (typeBrg != 2)
                            //{
                            sSQL_Value += ",'" + typeItemCode + "','typeitem','" + typeItemDesc + "'";
                            //}
                            //else
                            //{
                            //    sSQL_Value += ",'','',''";
                            //}
                        }
                        else if (j == 18)
                        {
                            //ACODE_20, ANAME_20, AVALUE_20 for aftersale
                            //if (typeBrg != 2)
                            //{
                            sSQL_Value += ",'" + afterSaleCode + "','aftersale','" + afterSaleDesc + "'";
                            //}
                            //else
                            //{
                            //    sSQL_Value += ",'','',''";
                            //}
                        }
                        else if (j == 19)
                        {
                            //ACODE_20, ANAME_20, AVALUE_20 for warranty
                            //if (typeBrg != 2)
                            //{
                            sSQL_Value += ",'" + warrantyCode + "','warranty','" + warrantyDesc + "'";
                            //}
                            //else
                            //{
                            //    sSQL_Value += ",'','',''";
                            //}
                        }
                        else
                        {
                            sSQL_Value += ",'','',''";
                        }
                    }
                }


                sSQL_Value += "),";
                //if (typeBrg == 1)
                //    sSQL_Value += ",";
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

        protected void getShopBrand(JDIDAPIData data)
        {
            try
            {

                var mgrApiManager = new JDIDController();
                mgrApiManager.AppKey = data.appKey;
                mgrApiManager.AppSecret = data.appSecret;
                mgrApiManager.AccessToken = data.accessToken;
                mgrApiManager.Method = "epi.popShop.getShopBrandList";

                var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
                var retBrand = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (retBrand != null)
                {
                    if (retBrand.openapi_msg.ToLower() == "success")
                    {
                        var dataBrand = JsonConvert.DeserializeObject(retBrand.openapi_data, typeof(Data_Brand)) as Data_Brand;
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

        protected void getShopID(JDIDAPIData data)
        {
            try
            {

                var mgrApiManager = new JDIDController();
                mgrApiManager.AppKey = data.appKey;
                mgrApiManager.AppSecret = data.appSecret;
                mgrApiManager.AccessToken = data.accessToken;
                mgrApiManager.Method = "epi.ware.openapi.CategoryApi.getAllCategoryTree";
                mgrApiManager.ParamJson = "";

                var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
                var retBrand = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (retBrand != null)
                {
                    if (retBrand.openapi_msg.ToLower() == "success")
                    {
                        var dataCAT = JsonConvert.DeserializeObject(retBrand.openapi_data, typeof(DATA_CAT)) as DATA_CAT;
                        if (dataCAT.success)
                        {
                            if (dataCAT.model.Count() > 0)
                                shopID_JDID = Convert.ToString(dataCAT.model[0].shopId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void UpdateStock(JDIDAPIData data, string id, int stok)
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
            var mgrApiManager = new JDIDController();

            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;
            mgrApiManager.Method = "epi.ware.openapi.warestock.updateWareStock";
            mgrApiManager.ParamJson = "{\"jsonStr\":[{\"skuId\":" + id + ", \"realNum\": " + stok + "}]}";
            try
            {
                var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (ret != null)
                {
                    if (ret.openapi_msg.ToLower() == "success")
                    {
                        var retStok = JsonConvert.DeserializeObject(ret.openapi_data, typeof(Data_UpStok)) as Data_UpStok;
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

        public void Order_JD(JDIDAPIData data, string cust)
        {
            //1: waiting for delivery, 2: shipped, 3: Waiting_Cancel, 4: Waiting_Refuse, 5: canceled, 6: Completed, 7: Ready to ship
            var listOrderId = new List<long>();

            listOrderId.AddRange(GetOrderList(data, "1", 1));
            listOrderId.AddRange(GetOrderList(data, "2", 1));
            listOrderId.AddRange(GetOrderList(data, "6", 1));
            listOrderId.AddRange(GetOrderList(data, "7", 1));
            string connectionID = Guid.NewGuid().ToString();

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
                bool pesananBaru = false;
                foreach (var listOrder in ord.orderIds)
                {
                    var insertTemp = GetOrderDetail(data, listOrder, cust, connectionID);
                    if (insertTemp.status == 1)
                    {
                        callSP = true;
                        if (insertTemp.recordCount > 0)
                            pesananBaru = true;
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
                    CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = cust;
                    //add by nurul 3/2/2022
                    var multilokasi = ErasoftDbContext.Database.SqlQuery<string>("select top 1 case when isnull(multilokasi,'')='' then '0' else multilokasi end as multilokasi from sifsys_tambahan (nolock)").FirstOrDefault();
                    if (multilokasi == "1")
                    {
                        EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable_MultiLokasi", CommandSQL);
                    }
                    else
                    {
                        EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);
                    }
                    //add by nurul 3/2/2022
                    //EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);
                }
            }
        }

        public List<long> GetOrderList(JDIDAPIData data, string status, int page)
        {
            var ret = new List<long>();
            var mgrApiManager = new JDIDController();
            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;

            mgrApiManager.Method = "epi.popOrder.getOrderIdListByCondition";
            mgrApiManager.ParamJson = "{\"orderStatus\":" + status + ", \"startRow\": " + page * 20 + ", \"bookTimeBegin\": "
                + DateTimeOffset.Now.AddDays(-14).ToUnixTimeSeconds() + "}";

            try
            {
                var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
                var retData = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (retData.openapi_code == 0)
                {
                    var listOrderId = JsonConvert.DeserializeObject(retData.openapi_data, typeof(Data_OrderIds)) as Data_OrderIds;
                    if (listOrderId.success)
                    {
                        ret = listOrderId.model;
                        if (listOrderId.model.Count == 20)
                        {
                            var nextOrders = GetOrderList(data, status, page + 1);
                            ret.AddRange(nextOrders);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return ret;
        }

        public BindingBase GetOrderDetail(JDIDAPIData data, string listOrderIds, string cust, string conn_id)
        {
            //var ret = new List<long>();
            var ret = new BindingBase();
            ret.status = 0;
            bool adaInsert = false;
            var mgrApiManager = new JDIDController();
            mgrApiManager.AppKey = data.appKey;
            mgrApiManager.AppSecret = data.appSecret;
            mgrApiManager.AccessToken = data.accessToken;

            mgrApiManager.Method = "epi.popOrder.getOrderInfoListForBatch";
            mgrApiManager.ParamJson = "[" + listOrderIds + "]";
            var jmlhNewOrder = 0;
            try
            {
                var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
                var retData = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (retData.openapi_code == 0)
                {
                    var listOrderId = JsonConvert.DeserializeObject(retData.openapi_data, typeof(Data_OrderDetail)) as Data_OrderDetail;
                    if (listOrderId.success)
                    {
                        var str = "{\"data\":" + listOrderId.model + "}";
                        var listDetails = JsonConvert.DeserializeObject(str, typeof(ModelOrder)) as ModelOrder;
                        if (listDetails != null)
                        {

                            string insertQ = "INSERT INTO TEMP_ORDER_JD ([ADDRESS],[AREA],[BOOKTIME],[CITY],[COUPON_AMOUNT],[CUSTOMER_NAME],";
                            insertQ += "[DELIVERY_ADDR],[DELIVERY_TYPE],[EMAIL],[FREIGHT_AMOUNT],[FULL_CUT_AMMOUNT],[INSTALLMENT_FEE],[ORDER_COMPLETE_TIME],";
                            insertQ += "[ORDER_ID],[ORDER_SKU_NUM],[ORDER_STATE],[ORDER_TYPE],[PAY_SUBTOTAL],[PAYMENT_TYPE],[PHONE],[POSTCODE],[PROMOTION_AMOUNT],";
                            insertQ += "[SENDPAY],[STATE],[TOTAL_PRICE],[USER_PIN],[CUST],[USERNAME],[CONN_ID]) VALUES ";

                            string insertOrderItems = "INSERT INTO TEMP_ORDERITEMS_JD ([ORDER_ID],[COMMISSION],[COST_PRICE],[COUPON_AMOUNT],[FULL_CUT_AMMOUNT]";
                            insertOrderItems += ",[HAS_PROMO],[JDPRICE],[PROMOTION_AMOUNT],[SKUID],[SKU_NAME],[SKU_NUMBER],[SPUID],[WEIGHT],[USERNAME],[CONN_ID]) VALUES ";

                            string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                            insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                            insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";

                            var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust).Select(p => p.NO_REFERENSI).ToList();
                            foreach (var order in listDetails.data)
                            {
                                bool doInsert = true;
                                if (OrderNoInDb.Contains(Convert.ToString(order.orderId)) && order.orderState.ToString() == "1")
                                {
                                    doInsert = false;
                                }
                                else if (order.orderState.ToString() == "5" || order.orderState.ToString() == "6")
                                {
                                    if (OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        //tidak ubah status menjadi selesai jika belum diisi faktur
                                        var dsSIT01A = EDB.GetDataSet("CString", "SIT01A", "SELECT NO_REFERENSI, O.NO_BUKTI, O.STATUS_TRANSAKSI FROM SIT01A I INNER JOIN SOT01A O ON I.NO_SO = O.NO_BUKTI WHERE NO_REFERENSI = '" + order.orderId + "'");
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

                                if (doInsert)
                                {
                                    var dtNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    adaInsert = true;
                                    var statusEra = "";
                                    switch (order.orderState.ToString())
                                    {
                                        //1: waiting for delivery, 2: shipped, 3: Waiting_Cancel, 4: Waiting_Refuse, 5: canceled, 6: Completed, 7: ready to ship
                                        case "1":
                                            statusEra = "01";
                                            break;
                                        case "7":
                                            statusEra = "02";
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
                                        default:
                                            statusEra = "99";
                                            break;
                                    }

                                    insertQ += "('" + order.address.Replace('\'', '`') + "','" + order.area.Replace('\'', '`') + "','" + DateTimeOffset.FromUnixTimeSeconds(order.bookTime / 1000).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss") + "','" + order.city.Replace('\'', '`') + "'," + order.couponAmount + ",'" + order.customerName + "','";
                                    insertQ += order.deliveryAddr.Replace('\'', '`') + "'," + order.deliveryType + ",'" + order.email + "'," + order.freightAmount + "," + order.fullCutAmount + "," + order.installmentFee + ",'" + DateTimeOffset.FromUnixTimeSeconds(order.orderCompleteTime / 1000).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss") + "','";
                                    insertQ += order.orderId + "'," + order.orderSkuNum + "," + statusEra + "," + order.orderType + "," + order.paySubtotal + "," + order.paymentType + ",'" + order.phone + "','" + order.postCode + "'," + order.promotionAmount + ",'";
                                    insertQ += order.sendPay + "','" + order.state.Replace('\'', '`') + "'," + order.totalPrice + ",'" + order.userPin + "','" + cust + "','" + username + "','" + conn_id + "') ,";

                                    if (order.orderSkuinfos != null)
                                    {
                                        foreach (var ordItem in order.orderSkuinfos)
                                        {
                                            insertOrderItems += "('" + order.orderId + "'," + ordItem.commission + "," + ordItem.costPrice + "," + ordItem.couponAmount + "," + ordItem.fullCutAmount + ",";
                                            insertOrderItems += ordItem.hasPromo + "," + ordItem.jdPrice + "," + ordItem.promotionAmount + ",'" + ordItem.skuId + "','" + ordItem.skuName + "',";
                                            insertOrderItems += ordItem.skuNumber + ",'" + ordItem.spuId + "'," + ordItem.weight + ",'" + username + "','" + conn_id + "') ,";
                                        }
                                    }

                                    var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + order.city + "%'");
                                    var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + order.state + "%'");

                                    var kabKot = "3174";//set default value jika tidak ada di db
                                    var prov = "31";//set default value jika tidak ada di db

                                    if (tblProv.Tables[0].Rows.Count > 0)
                                        prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
                                    if (tblKabKot.Tables[0].Rows.Count > 0)
                                        kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();

                                    insertPembeli += "('" + order.customerName.Replace('\'', '`') + "','" + order.address.Replace('\'', '`') + "','" + order.phone + "','" + order.email.Replace('\'', '`') + "',0,0,'0','01',";
                                    insertPembeli += "1, 'IDR', '01', '" + order.address.Replace('\'', '`') + "', 0, 0, 0, 0, '1', 0, 0, ";
                                    insertPembeli += "'FP', '" + dtNow + "', '" + username + "', '" + order.postCode.Replace('\'', '`') + "', '" + order.email.Replace('\'', '`') + "', '" + kabKot + "', '" + prov + "', '" + order.city.Replace('\'', '`') + "', '" + order.state.Replace('\'', '`') + "', '" + conn_id + "') ,";

                                    if (!OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                        jmlhNewOrder++;
                                }
                            }

                            if (adaInsert)
                            {
                                ret.status = 1;
                                insertQ = insertQ.Substring(0, insertQ.Length - 2);
                                EDB.ExecuteSQL(username, CommandType.Text, insertQ);


                                insertOrderItems = insertOrderItems.Substring(0, insertOrderItems.Length - 2);
                                EDB.ExecuteSQL(username, CommandType.Text, insertOrderItems);


                                insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 2);
                                EDB.ExecuteSQL(username, CommandType.Text, insertPembeli);

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

        public void CreatePromo(JDIDAPIData data, int recnumPromo, string kdBrg, double promoPrice)
        {
            try
            {

                var mgrApiManager = new JDIDController();
                mgrApiManager.AppKey = data.appKey;
                mgrApiManager.AppSecret = data.appSecret;
                mgrApiManager.AccessToken = data.accessToken;
                mgrApiManager.Method = "com.jd.eptid.promo.manager.sdk.service.CreatePromoService.singleCreatePlummetedPromo";


                var promoInDB = ErasoftDbContext.PROMOSI.Where(m => m.RecNum == recnumPromo).FirstOrDefault();

                mgrApiManager.ParamJson = "{\"plummetedInfoFormDTO\": {\"shopId\":";
                mgrApiManager.ParamJson += "\"promoName\":\"" + promoInDB.NAMA_PROMOSI + "\", \"promoType\":1,";
                mgrApiManager.ParamJson += "\"quota\":" + promoPrice + ",\"beginTime\":\"" + promoInDB.TGL_MULAI.Value.ToString("yyyy-MM-dd HH:mm:ss") + "\",";
                mgrApiManager.ParamJson += "\"endTime\":\"" + promoInDB.TGL_AKHIR.Value.ToString("yyyy-MM-dd HH:mm:ss") + "\",";
                mgrApiManager.ParamJson += "\"promoState\":1, \"createPin\":\"" + username + "\",";
                mgrApiManager.ParamJson += "\"skuId\":" + kdBrg + ", \"limitBuyType\":0,";
                mgrApiManager.ParamJson += "\"operator\":\"" + username + "\", \"childType\":1,";
                mgrApiManager.ParamJson += "\"promoChannel\":\"1,4,5\", \"sourceFrom\":0} }";

                var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
                var retPromo = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
                if (retPromo != null)
                {
                    if (retPromo.openapi_msg.ToLower() == "success")
                    {
                        var dataPromo = JsonConvert.DeserializeObject(retPromo.openapi_data, typeof(Model_Promo)) as Model_Promo;
                        if (dataPromo.success)
                        {
                            EDB.ExecuteSQL("CString", CommandType.Text, "UPDATE PROMOSIS SET MP_PROMO_ID = '" + dataPromo.order_id + "' WHERE RECNUM = " + recnumPromo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        protected enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4
        }
        protected void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, JDIDAPIData iden, API_LOG_MARKETPLACE data)
        {
            switch (action)
            {
                case api_status.Pending:
                    {
                        var arf01 = ErasoftDbContext.ARF01.Where(p => p.TOKEN == iden.accessToken).FirstOrDefault();
                        var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                        {
                            CUST = arf01 != null ? arf01.CUST : iden.accessToken,
                            CUST_ATTRIBUTE_1 = iden.accessToken,
                            CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                            CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                            CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                            CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                            MARKETPLACE = "JD.ID",
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

        //add by nurul 29/4/2021, JDID versi 2
        public async Task<BindingBase> getListProductV2(JDIDAPIData data, int page, string cust, int recordCount, int totalData)
        {
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(data.DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, data.DatabasePathErasoft);
            //int MOPartnerID = 841371;
            //string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            //string ret = "";
            var ret = new BindingBase
            {
                status = 0,
                recordCount = recordCount,
                exception = 0,
                totalData = totalData//add 18 Juli 2019, show total record
            };

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Item List",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = cust,
                REQUEST_ATTRIBUTE_3 = page.ToString(),
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            string responseFromServer = "";
            bool responseApi = false;
            int retry = 0;
            while (!responseApi && retry <= 3)
            {
                data = RefreshToken(data);
                var sysParams = new Dictionary<string, string>();
                this.ParamJson = "{\"page\":\"" + (page + 1) + "\",\"size\":\"10\"}";
                sysParams.Add("360buy_param_json", this.ParamJson);

                sysParams.Add("access_token", data.accessToken);
                sysParams.Add("app_key", data.appKey);
                this.Method = "jingdong.seller.product.getWareInfoListByVendorId"; //this API is for query product information list via venderId( this API is only for POP sellers)
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
                        ret.nextPage = 1;
                        ret.exception = 1;
                        ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                        currentLog.REQUEST_EXCEPTION = ret.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
                    }
                    else
                    {
                        retry = 4;
                        ret.nextPage = 1;
                        ret.exception = 1;
                        ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                        currentLog.REQUEST_EXCEPTION = ret.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
                    }
                }
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                try
                {
                    var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDGetListProductResult)) as JDIDGetListProductResult;
                    if (listBrg.jingdong_seller_product_getWareInfoListByVendorId_response.returnType != null)
                    {
                        //change by nurul 19/5/2021
                        //if (listBrg.jingdong_seller_product_getWareInfoListByVendorId_response.returnType.isSuccess)
                        if (listBrg.jingdong_seller_product_getWareInfoListByVendorId_response.returnType.isSuccess || listBrg.jingdong_seller_product_getWareInfoListByVendorId_response.returnType.success)
                        //end change by nurul 19/5/2021
                        {
                            string msg = "";
                            bool adaError = false;
                            if (listBrg.jingdong_seller_product_getWareInfoListByVendorId_response.returnType.model.spuInfoVoList == null)
                            {
                                ret.status = 1;
                                ret.nextPage = 0;
                                return ret;
                            }
                            if (listBrg.jingdong_seller_product_getWareInfoListByVendorId_response.returnType.model.spuInfoVoList.Count() > 0)
                            {
                                ret.status = 1;
                                int IdMarket = ErasoftDbContext.ARF01.Where(c => c.CUST == cust).FirstOrDefault().RecNum.Value;
                                if (listBrg.jingdong_seller_product_getWareInfoListByVendorId_response.returnType.model.spuInfoVoList.Count() == 10)
                                    //ret.message = (page + 1).ToString();
                                    ret.nextPage = 1;
                                var spu = "";
                                foreach (var item in listBrg.jingdong_seller_product_getWareInfoListByVendorId_response.returnType.model.spuInfoVoList)
                                {
                                    //product status: 1.online,2.offline,3.punish,4.deleted
                                    if (item.wareStatus == 1 || item.wareStatus == 2)
                                    {
                                        var retProd = await GetProductV2(data, item, IdMarket, cust);
                                        ret.totalData += retProd.totalData;//add 18 Juli 2019, show total record
                                        if (retProd.status == 1)
                                        {
                                            //add by nurul 12/10/2021
                                            ret.status = retProd.status;
                                            //end add by nurul 12/10/2021
                                            ret.recordCount += retProd.recordCount;
                                        }
                                        else
                                        {
                                            adaError = true;
                                            msg += item.spuId + ":" + retProd.message + "___||___";
                                        }
                                    }
                                    else
                                    {
                                        spu = spu + item.spuId + ",";
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
                            ret.message = listBrg.jingdong_seller_product_getWareInfoListByVendorId_response.returnType.message;
                            currentLog.REQUEST_EXCEPTION = ret.message;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        }
                    }

                }
                catch (Exception ex2)
                {
                    ret.nextPage = 1;
                    ret.exception = 1;
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
                }
            }
            return ret;
        }

        public async Task<BindingBase> GetProductV2(JDIDAPIData data, SpuinfovolistGetListProductV2 itemFromList, int IdMarket, string cust)
        {
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(data.DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, data.DatabasePathErasoft);
            var ret = new BindingBase
            {
                status = 0,
                exception = 0,
                totalData = 0,//add 18 Juli 2019, show total record
            };

            try
            {
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 3)
                {
                    data = RefreshToken(data);
                    var sysParams = new Dictionary<string, string>();
                    this.ParamJson = "{\"spuId\":\"" + itemFromList.spuId + "\"}";
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
                        retry = retry + 1;
                        ret.exception = 1;
                        ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                    }
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    try
                    {
                        var dataProduct = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDGetProductV2Result)) as JDIDGetProductV2Result;
                        if (dataProduct.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType != null)
                        {
                            //change by nurul 19/5/2021
                            //if (dataProduct.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType.isSuccess)
                            if (dataProduct.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType.isSuccess || dataProduct.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType.success)
                            //end change by nurul 19/5/2021
                            {
                                var stf02h_local = ErasoftDbContext.STF02H.Where(m => m.IDMARKET == IdMarket).ToList();
                                var tempBrg_local = ErasoftDbContext.TEMP_BRG_MP.Where(m => m.IDMARKET == IdMarket).ToList();

                                var haveVarian = false;
                                if (dataProduct.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType.model != null)
                                {
                                    if (dataProduct.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType.model.Count() > 1)
                                    {
                                        haveVarian = true;
                                        ret.totalData += 1;//add 18 Juli 2019, show total record
                                    }
                                    ret.totalData += dataProduct.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType.model.Count();//add 18 Juli 2019, show total record
                                    foreach (var item in dataProduct.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType.model)
                                    {
                                        var tempbrginDB = new TEMP_BRG_MP();
                                        var brgInDB = new STF02H();

                                        if (haveVarian)
                                        {
                                            //handle parent
                                            string kdBrgInduk = item.spuId.ToString();
                                            bool createParent = false;
                                            tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == kdBrgInduk + ";0").FirstOrDefault();
                                            brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == kdBrgInduk + ";0").FirstOrDefault();
                                            if (tempbrginDB == null && brgInDB == null)
                                            {
                                                if (item.skuId == dataProduct.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType.model[0].skuId)
                                                    createParent = true;
                                            }
                                            else if (brgInDB != null)
                                            {
                                                kdBrgInduk = brgInDB.BRG;
                                            }
                                            //end handle parent

                                            tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.spuId.ToString() + ";" + item.skuId.ToString().ToUpper()).FirstOrDefault();
                                            brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.spuId.ToString() + ";" + item.skuId.ToString().ToUpper()).FirstOrDefault();
                                            if (tempbrginDB == null && brgInDB == null)
                                            {
                                                var retData = await getProductDetailV2(data, item, kdBrgInduk, createParent, item.skuId.ToString(), cust, IdMarket, itemFromList);
                                                if (retData.exception == 1)
                                                    ret.exception = 1;
                                                if (retData.status == 1)
                                                {
                                                    //add by nurul 12/10/2021
                                                    ret.status = retData.status;
                                                    //end add by nurul 12/10/2021
                                                    ret.recordCount += retData.recordCount;
                                                }
                                            }
                                            else
                                            {
                                                if (createParent)
                                                {
                                                    var retDataParent = await getProductDetailParentOnlyV2(data, item, kdBrgInduk, createParent, item.skuId.ToString(), cust, IdMarket, itemFromList);
                                                    if (retDataParent.exception == 1)
                                                        ret.exception = 1;
                                                    if (retDataParent.status == 1)
                                                    {
                                                        //add by nurul 12/10/2021
                                                        ret.status = retDataParent.status;
                                                        //end add by nurul 12/10/2021
                                                        ret.recordCount += retDataParent.recordCount;
                                                        //createParent = false;
                                                    }
                                                }
                                                var datasudahada = item.skuId.ToString(); // breakpoint
                                            }
                                            //}

                                        }
                                        else
                                        {
                                            tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.spuId.ToString() + ";" + item.skuId.ToString().ToUpper()).FirstOrDefault();
                                            brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.spuId.ToString() + ";" + item.skuId.ToString().ToUpper()).FirstOrDefault();
                                            if (tempbrginDB == null && brgInDB == null)
                                            {
                                                var retData = await getProductDetailV2(data, item, "", false, item.skuId.ToString(), cust, IdMarket, itemFromList);
                                                if (retData.exception == 1)
                                                    ret.exception = 1;
                                                if (retData.status == 1)
                                                {
                                                    //add by nurul 12/10/2021
                                                    ret.status = retData.status;
                                                    //end add by nurul 12/10/2021
                                                    ret.recordCount += retData.recordCount;
                                                }
                                            }
                                            else
                                            {
                                                var datasudahada = item.skuId.ToString(); // breakpoint
                                            }
                                        }

                                    }
                                }

                                ret.status = 1;

                            }
                            else
                            {
                                ret.message = dataProduct.jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response.returnType.message;
                                //currentLog.REQUEST_EXCEPTION = ret.message;
                                //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            }
                        }
                    }
                    catch (Exception ex2)
                    {
                        ret.nextPage = 1;
                        ret.exception = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                ret.exception = 1;
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }

            return ret;
        }

        public async Task<BindingBase> getProductDetailV2(JDIDAPIData data, ModelGetProductV2 item, string kdBrgInduk, bool createParent, string skuId, string cust, int IdMarket, SpuinfovolistGetListProductV2 itemFromList)
        {
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(data.DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, data.DatabasePathErasoft);
            var ret = new BindingBase
            {
                status = 0,
                exception = 0
            };

            try
            {
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 3)
                {
                    data = RefreshToken(data);
                    var sysParams = new Dictionary<string, string>();
                    this.ParamJson = "{\"skuId\":\"" + skuId + "\"}";
                    sysParams.Add("360buy_param_json", this.ParamJson);

                    sysParams.Add("access_token", data.accessToken);
                    sysParams.Add("app_key", data.appKey);
                    this.Method = "jingdong.seller.product.sku.read.getSkuBySkuIds"; //this API is for query sku information via spuId
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
                        retry = retry + 1;
                        ret.exception = 1;
                        ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                    }
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    try
                    {
                        var detailData = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDGetProductDetailV2Result)) as JDIDGetProductDetailV2Result;
                        if (detailData.jingdong_seller_product_sku_read_getSkuBySkuIds_response.returnType != null)
                        {
                            if (detailData.jingdong_seller_product_sku_read_getSkuBySkuIds_response.returnType.success)
                            {
                                var retData = await getProductDetailLanjutanV2(data, item, detailData.jingdong_seller_product_sku_read_getSkuBySkuIds_response.returnType.model[0], kdBrgInduk, createParent, item.skuId.ToString(), cust, IdMarket, itemFromList);
                                if (retData.exception == 1)
                                    ret.exception = 1;
                                if (retData.status == 1)
                                {
                                    //add by nurul 12/10/2021
                                    ret.status = retData.status;
                                    //end add by nurul 12/10/2021
                                    ret.recordCount += retData.recordCount;
                                }

                                //----------------------------------------------------------

                                //if (!string.IsNullOrEmpty(kdBrgInduk))
                                //{
                                //    if (createParent)
                                //    {
                                //        var a = detailData.jingdong_seller_product_sku_read_getSkuBySkuIds_response.returnType.model[0].
                                //        var retSQL = CreateSQLValueV2(data, item, detailData.jingdong_seller_product_sku_read_getSkuBySkuIds_response.returnType.model[0], kdBrgInduk, "", cust, IdMarket, 1, itemFromList);
                                //        if (retSQL.exception == 1)
                                //            ret.exception = 1;
                                //        if (retSQL.status == 1)
                                //            sSQLVal += retSQL.message;
                                //    }

                                //    var retSQL2 = CreateSQLValueV2(data, item, detailData.jingdong_seller_product_sku_read_getSkuBySkuIds_response.returnType.model[0], kdBrgInduk, skuId, cust, IdMarket, 2, itemFromList);
                                //    if (retSQL2.exception == 1)
                                //        ret.exception = 1;
                                //    if (retSQL2.status == 1)
                                //        sSQLVal += retSQL2.message;
                                //}
                                //else
                                //{
                                //    var retSQL = CreateSQLValueV2(data, item, detailData.jingdong_seller_product_sku_read_getSkuBySkuIds_response.returnType.model[0], "", skuId, cust, IdMarket, 0, itemFromList);
                                //    if (retSQL.exception == 1)
                                //        ret.exception = 1;
                                //    if (retSQL.status == 1)
                                //        sSQLVal += retSQL.message;
                                //}
                            }
                            else
                            {
                                ret.message = string.IsNullOrEmpty(detailData.jingdong_seller_product_sku_read_getSkuBySkuIds_response.returnType.message) ? "" : detailData.jingdong_seller_product_sku_read_getSkuBySkuIds_response.returnType.message;
                            }
                        }
                        else
                        {
                            ret.message = "getProductDetailV2 error.";
                            ret.exception = 1;
                        }

                    }
                    catch (Exception ex)
                    {
                        ret.exception = 1;
                        ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                    }
                }
                else
                {
                    ret.message = "respons getProductDetailV2 kosong.";
                    ret.exception = 1;
                }
            }
            catch (Exception ex)
            {
                ret.exception = 1;
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }

        public async Task<BindingBase> getProductDetailLanjutanV2(JDIDAPIData data, ModelGetProductV2 item, ModelGetProductDetailV2 detItem, string kdBrgInduk, bool createParent, string skuId, string cust, int IdMarket, SpuinfovolistGetListProductV2 itemFromList)
        {
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(data.DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, data.DatabasePathErasoft);
            var ret = new BindingBase
            {
                status = 0,
                exception = 0
            };

            try
            {
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 3)
                {
                    data = RefreshToken(data);
                    var sysParams = new Dictionary<string, string>();
                    this.ParamJson = "{\"spuId\":\"" + item.spuId + "\",\"spuDescription\":\"1\",\"spuImgs\":\"1\",\"brandInfo\":\"1\",\"skuIds\":\"1\"}";
                    sysParams.Add("360buy_param_json", this.ParamJson);

                    sysParams.Add("access_token", data.accessToken);
                    sysParams.Add("app_key", data.appKey);
                    this.Method = "jingdong.seller.product.getWareBySpuIds"; //query product information via productId list, tambahan untuk dapetin warrantyPeriod
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
                        retry = retry + 1;
                        ret.exception = 1;
                        ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                    }
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, BERAT, PANJANG, LEBAR, TINGGI, CUST, Deskripsi, IDMARKET, HJUAL, HJUAL_MP, ";
                    sSQL += "DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, IMAGE4, IMAGE5, KODE_BRG_INDUK, TYPE, ";
                    sSQL += "ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
                    sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20) VALUES ";

                    string sSQLVal = "";
                    try
                    {
                        var detailData = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDgetProductDetailLanjutanV2Result)) as JDIDgetProductDetailLanjutanV2Result;
                        if (detailData.jingdong_seller_product_getWareBySpuIds_response.returnType != null)
                        {
                            if (detailData.jingdong_seller_product_getWareBySpuIds_response.returnType.success)
                            {
                                if (!string.IsNullOrEmpty(kdBrgInduk))
                                {
                                    if (createParent)
                                    {
                                        var retSQL = CreateSQLValueV2(data, item, detItem, detailData.jingdong_seller_product_getWareBySpuIds_response.returnType.model[0], kdBrgInduk, "", cust, IdMarket, 1, itemFromList);
                                        if (retSQL.exception == 1)
                                            ret.exception = 1;
                                        if (retSQL.status == 1)
                                            sSQLVal += retSQL.message;
                                    }

                                    var retSQL2 = CreateSQLValueV2(data, item, detItem, detailData.jingdong_seller_product_getWareBySpuIds_response.returnType.model[0], kdBrgInduk, skuId, cust, IdMarket, 2, itemFromList);
                                    if (retSQL2.exception == 1)
                                        ret.exception = 1;
                                    if (retSQL2.status == 1)
                                        sSQLVal += retSQL2.message;
                                }
                                else
                                {
                                    var retSQL = CreateSQLValueV2(data, item, detItem, detailData.jingdong_seller_product_getWareBySpuIds_response.returnType.model[0], "", skuId, cust, IdMarket, 0, itemFromList);
                                    if (retSQL.exception == 1)
                                        ret.exception = 1;
                                    if (retSQL.status == 1)
                                        sSQLVal += retSQL.message;
                                }
                            }
                            else
                            {
                                ret.message = string.IsNullOrEmpty(detailData.jingdong_seller_product_getWareBySpuIds_response.returnType.message) ? "" : detailData.jingdong_seller_product_getWareBySpuIds_response.returnType.message;
                            }
                        }
                        else
                        {
                            ret.message = "getProductDetailLanjutanV2 error.";
                            ret.exception = 1;
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
                        ret.exception = 1;
                        ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                    }
                }
                else
                {
                    ret.message = "respons getProductDetailLanjutanV2 kosong.";
                    ret.exception = 1;
                }
            }
            catch (Exception ex)
            {
                ret.exception = 1;
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }

        public async Task<BindingBase> getProductDetailParentOnlyV2(JDIDAPIData data, ModelGetProductV2 item, string kdBrgInduk, bool createParent, string skuId, string cust, int IdMarket, SpuinfovolistGetListProductV2 itemFromList)
        {
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(data.DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, data.DatabasePathErasoft);
            var ret = new BindingBase
            {
                status = 0,
                exception = 0
            };

            try
            {
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 3)
                {
                    data = RefreshToken(data);
                    var sysParams = new Dictionary<string, string>();
                    this.ParamJson = "{\"skuId\":\"" + skuId + "\"}";
                    sysParams.Add("360buy_param_json", this.ParamJson);

                    sysParams.Add("access_token", data.accessToken);
                    sysParams.Add("app_key", data.appKey);
                    this.Method = "jingdong.seller.product.sku.read.getSkuBySkuIds"; //this API is for query sku information via spuId
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
                        retry = retry + 1;
                        ret.exception = 1;
                        ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                    }
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    try
                    {
                        var detailData = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDGetProductDetailV2Result)) as JDIDGetProductDetailV2Result;
                        if (detailData.jingdong_seller_product_sku_read_getSkuBySkuIds_response.returnType != null)
                        {
                            if (detailData.jingdong_seller_product_sku_read_getSkuBySkuIds_response.returnType.success)
                            {
                                var retData = await getProductDetailParentOnlyLanjutanV2(data, item, detailData.jingdong_seller_product_sku_read_getSkuBySkuIds_response.returnType.model[0], kdBrgInduk, createParent, item.skuId.ToString(), cust, IdMarket, itemFromList);
                                if (retData.exception == 1)
                                    ret.exception = 1;
                                if (retData.status == 1)
                                {
                                    ret.recordCount += retData.recordCount;
                                }
                            }
                            else
                            {
                                ret.message = string.IsNullOrEmpty(detailData.jingdong_seller_product_sku_read_getSkuBySkuIds_response.returnType.message) ? "" : detailData.jingdong_seller_product_sku_read_getSkuBySkuIds_response.returnType.message;
                            }
                        }
                        else
                        {
                            ret.message = "getProductDetailParentOnlyV2 error.";
                            ret.exception = 1;
                        }

                    }
                    catch (Exception ex)
                    {
                        ret.exception = 1;
                        ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                    }
                }
                else
                {
                    ret.message = "respons getProductDetailParentOnlyV2 kosong.";
                    ret.exception = 1;
                }
            }
            catch (Exception ex)
            {
                ret.exception = 1;
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }

        public async Task<BindingBase> getProductDetailParentOnlyLanjutanV2(JDIDAPIData data, ModelGetProductV2 item, ModelGetProductDetailV2 detItem, string kdBrgInduk, bool createParent, string skuId, string cust, int IdMarket, SpuinfovolistGetListProductV2 itemFromList)
        {
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(data.DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, data.DatabasePathErasoft);
            var ret = new BindingBase
            {
                status = 0,
                exception = 0
            };

            try
            {
                string responseFromServer = "";
                bool responseApi = false;
                int retry = 0;
                while (!responseApi && retry <= 3)
                {
                    data = RefreshToken(data);
                    var sysParams = new Dictionary<string, string>();
                    this.ParamJson = "{\"spuId\":\"" + item.spuId + "\",\"spuDescription\":\"1\",\"spuImgs\":\"1\",\"brandInfo\":\"1\",\"skuIds\":\"1\"}";
                    sysParams.Add("360buy_param_json", this.ParamJson);

                    sysParams.Add("access_token", data.accessToken);
                    sysParams.Add("app_key", data.appKey);
                    this.Method = "jingdong.seller.product.getWareBySpuIds"; //query product information via productId list, tambahan untuk dapetin warrantyPeriod
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
                        retry = retry + 1;
                        ret.exception = 1;
                        ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                    }
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, BERAT, PANJANG, LEBAR, TINGGI, CUST, Deskripsi, IDMARKET, HJUAL, HJUAL_MP, ";
                    sSQL += "DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, IMAGE4, IMAGE5, KODE_BRG_INDUK, TYPE, ";
                    sSQL += "ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
                    sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20) VALUES ";

                    string sSQLVal = "";
                    try
                    {
                        var detailData = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDgetProductDetailLanjutanV2Result)) as JDIDgetProductDetailLanjutanV2Result;
                        if (detailData.jingdong_seller_product_getWareBySpuIds_response.returnType != null)
                        {
                            if (detailData.jingdong_seller_product_getWareBySpuIds_response.returnType.success)
                            {
                                if (!string.IsNullOrEmpty(kdBrgInduk))
                                {
                                    if (createParent)
                                    {
                                        var retSQL = CreateSQLValueV2(data, item, detItem, detailData.jingdong_seller_product_getWareBySpuIds_response.returnType.model[0], kdBrgInduk, "", cust, IdMarket, 1, itemFromList);
                                        if (retSQL.exception == 1)
                                            ret.exception = 1;
                                        if (retSQL.status == 1)
                                            sSQLVal += retSQL.message;
                                    }
                                }
                            }
                            else
                            {
                                ret.message = string.IsNullOrEmpty(detailData.jingdong_seller_product_getWareBySpuIds_response.returnType.message) ? "" : detailData.jingdong_seller_product_getWareBySpuIds_response.returnType.message;
                            }
                        }
                        else
                        {
                            ret.message = "getProductDetailParentOnlyLanjutanV2 error.";
                            ret.exception = 1;
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
                        ret.exception = 1;
                        ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                    }
                }
                else
                {
                    ret.message = "respons getProductDetailParentOnlyLanjutanV2 kosong.";
                    ret.exception = 1;
                }
            }
            catch (Exception ex)
            {
                ret.exception = 1;
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }

        protected BindingBase CreateSQLValueV2(JDIDAPIData data, ModelGetProductV2 item, ModelGetProductDetailV2 detItem, ModelgetProductDetailLanjutanV2 detItemLanjutan, string kdBrgInduk, string skuId, string cust, int IdMarket, int typeBrg, SpuinfovolistGetListProductV2 itemFromList)
        {
            // typeBrg : 0 = barang tanpa varian; 1 = barang induk; 2 = barang varian
            var ret = new BindingBase
            {
                status = 0,
                exception = 0
            };

            string sSQL_Value = "";
            try
            {

                string[] attrVal;
                string value = "";
                var brgAttribute = new Dictionary<string, string>();
                string namaBrg = item.skuName;
                string nama, nama2, urlImage, urlImage2, urlImage3, urlImage4, urlImage5;
                var namaTemp = "";
                urlImage = "";
                urlImage2 = "";
                urlImage3 = "";
                urlImage4 = "";
                urlImage5 = "";
                var categoryCode = itemFromList.fullCategoryId.Split('/');
                var categoryName = itemFromList.fullCategoryName.Replace('\'', '`').Split('/');
                double price = Convert.ToDouble(item.jdPrice);
                //var statusBrg = detItem != null ? detItem.status : 1;
                //var statusBrg = detItem.status; //display 
                var statusBrg = 0;
                if (detItemLanjutan.wareStatus == 1)
                {
                    statusBrg = detItemLanjutan.wareStatus;
                }

                string brand = "";
                if (itemFromList.brandId.ToString() != null)
                {
                    //brand = getBrandByShopID(data, detItemLanjutan.shopId.ToString(), Convert.ToInt32(itemFromList.brandId));
                    brand = detItemLanjutan.brandName;
                }

                //var display = statusBrg.Equals("active") ? 1 : 0;
                string deskripsi = itemFromList.description;

                var typeItemCode = "";
                var typeItemDesc = "";
                if (!string.IsNullOrEmpty(Convert.ToString(item.piece)))
                {
                    typeItemCode = Convert.ToString(item.piece);
                    switch (typeItemCode)
                    {
                        case "0":
                            typeItemDesc = "Small item <= 20KG";
                            break;
                        case "1":
                            typeItemDesc = "Big item > 20KG";
                            break;
                        default:
                            typeItemDesc = "";
                            break;
                    }
                }

                var afterSaleCode = "";
                var afterSaleDesc = "";
                if (!string.IsNullOrEmpty(Convert.ToString(itemFromList.afterSale)))
                {
                    afterSaleCode = Convert.ToString(itemFromList.afterSale);
                    switch (afterSaleCode)
                    {
                        case "1":
                            afterSaleDesc = "Only Support 7 Days Refund";
                            break;
                        case "2":
                            afterSaleDesc = "Not Support 7 Days Refund And 15 Days Exchange";
                            break;
                        case "3":
                            afterSaleDesc = "Support 7 Days Refund And 15 Days Exchange";
                            break;
                        case "4":
                            afterSaleDesc = "Only Support 15 Days Exchange";
                            break;
                        default:
                            afterSaleDesc = "";
                            break;
                    }
                }

                var warrantyCode = "";
                var warrantyDesc = "";
                if (!string.IsNullOrEmpty(Convert.ToString(detItemLanjutan.warrantyPeriod)))
                {
                    warrantyCode = Convert.ToString(detItemLanjutan.warrantyPeriod);
                    switch (warrantyCode)
                    {
                        case "1":
                            warrantyDesc = "No warranty";
                            break;
                        case "50":
                            warrantyDesc = "6 months official warranty";
                            break;
                        case "51":
                            warrantyDesc = "8 months official warranty";
                            break;
                        case "52":
                            warrantyDesc = "18 months official warranty";
                            break;
                        case "2":
                            warrantyDesc = "1 year official warranty";
                            break;
                        case "3":
                            warrantyDesc = "2 year official warranty";
                            break;
                        case "4":
                            warrantyDesc = "3 year official warranty";
                            break;
                        case "11":
                            warrantyDesc = "4 year official warranty";
                            break;
                        case "12":
                            warrantyDesc = "5 year official warranty";
                            break;
                        case "5":
                            warrantyDesc = "1 year shop warranty";
                            break;
                        case "6":
                            warrantyDesc = "2 year shop warranty";
                            break;
                        case "7":
                            warrantyDesc = "3 year shop warranty";
                            break;
                        case "21":
                            warrantyDesc = "4 year shop warranty";
                            break;
                        case "22":
                            warrantyDesc = "5 year shop warranty";
                            break;
                        case "31":
                            warrantyDesc = "1 year compressor warranty";
                            break;
                        case "32":
                            warrantyDesc = "2 year compressor warranty";
                            break;
                        case "33":
                            warrantyDesc = "3 year compressor warranty";
                            break;
                        case "35":
                            warrantyDesc = "5 year compressor warranty";
                            break;
                        case "30":
                            warrantyDesc = "10 year compressor warranty";
                            break;
                        case "41":
                            warrantyDesc = "1 year motor warranty";
                            break;
                        case "42":
                            warrantyDesc = "2 year motor warranty";
                            break;
                        case "43":
                            warrantyDesc = "3 year motor warranty";
                            break;
                        case "45":
                            warrantyDesc = "5 year motor warranty";
                            break;
                        case "40":
                            warrantyDesc = "10 year motor warranty";
                            break;
                        case "8":
                            warrantyDesc = "Lifetime warranty";
                            break;
                        default:
                            warrantyDesc = "";
                            break;
                    }
                }



                if (typeBrg != 1)
                {
                    //change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                    //if (!string.IsNullOrEmpty(detItem.sellerSkuId.ToString()) && detItem.sellerSkuId.ToString() != "null")
                    // change by fauzi 03/09/2020 tambah validasi untuk seller sku dengan isi null
                    var sellerSKUAPI = Convert.ToString(detItem.sellerSkuId);
                    if (!string.IsNullOrEmpty(sellerSKUAPI) && sellerSKUAPI != "null")
                    {
                        sSQL_Value += " ( '" + item.spuId + ";" + skuId + "' , '" + sellerSKUAPI + "' , '";
                    }
                    else
                    {
                        sSQL_Value += " ( '" + item.spuId + ";" + skuId + "' , '" + skuId + "' , '";
                    }
                    //end changed by fauzi 03/09/2020
                    //sSQL_Value += " ( '" + skuId + "' , '' , '";
                    //end change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                }
                else
                {
                    namaBrg = itemFromList.spuName;
                    //change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                    //sSQL_Value += " ( '" + kdBrgInduk + "' , '" + kdBrgInduk + "' , '";
                    sSQL_Value += " ( '" + kdBrgInduk + ";0" + "' , '' , '";
                    //end change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
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
                    string[] ssplitNama = namaBrg.Split(' ');
                    nama = ssplitNama[0];

                    int c;
                    for (c = 1; c < ssplitNama.Length; c++)
                    {
                        namaTemp = namaTemp + ssplitNama[c] + " ";
                    }

                    //if (ssplitNama.Length >= 2)
                    //{
                    //    nama = ssplitNama[0] + " " + ssplitNama[1] + " " + ssplitNama[2];
                    //    nama2 = namaBrg.in(ssplitNama[3], );
                    //}
                    //else
                    //{
                    //    nama = ssplitNama[0] + " " + ssplitNama[1];
                    //}

                    //nama = namaBrg.Substring(0, 30);
                    //if (namaBrg.Length > 285)
                    //{
                    //    nama2 = namaBrg.Substring(30, 255);
                    //}
                    //else
                    //{
                    //    nama2 = namaBrg.Substring(30);
                    //}
                    nama2 = namaTemp;
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
                //if (detItem.saleAttributeNameMap != null){
                //    if(detItem.saleAttributeNameMap.Count > 0)
                //    {

                //    }
                //}

                //price = brgAttribute.TryGetValue("jdPrice", out value) ? Convert.ToDouble(value) : Convert.ToDouble(item.jdPrice);
                if (Convert.ToDouble(detItem.jdPrice) > 0)
                    price = Convert.ToDouble(detItem.jdPrice);

                //}
                //if (listBrand.Count > 0)
                //{
                //    var a = listBrand.Where(m => m.brandId == itemFromList.brandId).FirstOrDefault();
                //    if (a != null)
                //        brand = a.brandName;
                //}
                //sSQL_Value += Convert.ToDouble(detItem != null ? detItem.netWeight : item.netWeight) * 1000 + " , " + Convert.ToDouble(detItem != null ? detItem.packLong : item.packLong) + " , ";
                //sSQL_Value += Convert.ToDouble(detItem != null ? detItem.packWide : item.packWide) + " , " + Convert.ToDouble(detItem != null ? detItem.packHeight : item.packHeight);
                sSQL_Value += Convert.ToDouble(detItem.netWeight) * 1000 + " , " + Convert.ToDouble(detItem.packLong) + " , ";
                sSQL_Value += Convert.ToDouble(detItem.packWide) + " , " + Convert.ToDouble(detItem.packHeight);
                sSQL_Value += " , '" + cust + "' , '" + deskripsi.Replace('\'', '`') + "' , " + IdMarket + " , " + price + " , " + price + " , ";
                sSQL_Value += statusBrg + " , '" + categoryCode[categoryCode.Length - 1] + "' , '" + categoryName[categoryName.Length - 1] + "' , '";
                //change by nurul 9/6/2021
                //sSQL_Value += brand + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "' , '" + urlImage4 + "' , '" + urlImage5 + "' , '" + (typeBrg == 2 ? kdBrgInduk : "") + "' , '" + (typeBrg == 1 ? "4" : "3") + "'";
                var kd_brg_induk = "";
                if (kdBrgInduk == Convert.ToString(item.spuId))
                {
                    kd_brg_induk = kdBrgInduk + ";0";
                }
                else
                {
                    kd_brg_induk = kdBrgInduk;
                }
                sSQL_Value += brand.Replace('\'', '`') + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "' , '" + urlImage4 + "' , '" + urlImage5 + "' , '" + (typeBrg == 2 ? kd_brg_induk : "") + "' , '" + (typeBrg == 1 ? "4" : "3") + "'";
                //end change by nurul 9/6/2021
                int i;
                if (!string.IsNullOrEmpty(detItem.saleAttributeIds) && detItem.saleAttributeIds != "null")
                {
                    attrVal = detItem.saleAttributeIds.Split(';');

                    foreach (Newtonsoft.Json.Linq.JProperty property in detItem.saleAttributeNameMap)
                    {
                        brgAttribute.Add(property.Name, property.Value.ToString());
                    }

                    for (i = 0; i < attrVal.Length; i++)
                    {
                        var attr = attrVal[i].Split(':');
                        if (attr.Length == 2)
                        {
                            if (brgAttribute.Count > 0)
                            {
                                var attrName = (brgAttribute.TryGetValue(attrVal[i], out value) ? value : "").Split(':');

                                sSQL_Value += ",'" + attr[0] + "','" + attrName[0] + "','" + attr[1] + "'";
                            }
                            else
                            {
                                sSQL_Value += ",'','',''";
                            }
                        }
                        else
                        {
                            sSQL_Value += ",'','',''";
                        }
                    }

                    for (int j = i; j < 20; j++)
                    {
                        if (j == 17)
                        {
                            //ACODE_18, ANAME_18, AVALUE_18 for piece / tipe barang  Small item ≤20KG / Big item > 20KG
                            //if (typeBrg != 2)
                            //{
                            sSQL_Value += ",'" + typeItemCode + "','typeitem','" + typeItemDesc + "'";
                            //}
                            //else
                            //{
                            //    sSQL_Value += ",'','',''";
                            //}
                        }
                        else if (j == 18)
                        {
                            //ACODE_19, ANAME_19, AVALUE_19 for aftersale
                            //if (typeBrg != 2)
                            //{
                            sSQL_Value += ",'" + afterSaleCode + "','aftersale','" + afterSaleDesc + "'";
                            //}
                            //else
                            //{
                            //    sSQL_Value += ",'','',''";
                            //}
                        }
                        else if (j == 19)
                        {
                            //ACODE_20, ANAME_20, AVALUE_20 for warranty
                            //if (typeBrg != 2)
                            //{
                            sSQL_Value += ",'" + warrantyCode + "','warranty','" + warrantyDesc + "'";
                            //}
                            //else
                            //{
                            //    sSQL_Value += ",'','',''";
                            //}                                
                        }
                        else
                        {
                            sSQL_Value += ",'','',''";
                        }
                    }
                }
                else
                {
                    sSQL_Value += ",'','',''";
                    for (int j = 1; j < 20; j++)
                    {
                        if (j == 17)
                        {
                            //ACODE_18, ANAME_18, AVALUE_18 for piece / tipe barang  Small item ≤20KG / Big item > 20KG
                            //if (typeBrg != 2)
                            //{
                            sSQL_Value += ",'" + typeItemCode + "','typeitem','" + typeItemDesc + "'";
                            //}
                            //else
                            //{
                            //    sSQL_Value += ",'','',''";
                            //}
                        }
                        else if (j == 18)
                        {
                            //ACODE_20, ANAME_20, AVALUE_20 for aftersale
                            //if (typeBrg != 2)
                            //{
                            sSQL_Value += ",'" + afterSaleCode + "','aftersale','" + afterSaleDesc + "'";
                            //}
                            //else
                            //{
                            //    sSQL_Value += ",'','',''";
                            //}
                        }
                        else if (j == 19)
                        {
                            //ACODE_20, ANAME_20, AVALUE_20 for warranty
                            //if (typeBrg != 2)
                            //{
                            sSQL_Value += ",'" + warrantyCode + "','warranty','" + warrantyDesc + "'";
                            //}
                            //else
                            //{
                            //    sSQL_Value += ",'','',''";
                            //}
                        }
                        else
                        {
                            sSQL_Value += ",'','',''";
                        }
                    }
                }


                sSQL_Value += "),";
                //if (typeBrg == 1)
                //    sSQL_Value += ",";
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
        //end add by nurul 29/4/2021, JDID versi 2 

        //add by nurul 27/5/2021, JDID versi 2 tahap 2 
        public async Task<string> JDID_getCategoryV2(JDIDAPIData data)
        {
            DatabaseSQL EDB = new DatabaseSQL(data.DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
#if AWS
                        string con = "Data Source=172.31.20.192;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#elif Debug_AWS
                        string con = "Data Source=54.151.175.62\\SQLEXPRESS,12354;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#elif DEV
                        string con = "Data Source=172.31.20.73;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#elif DEBUG
            string con = "Data Source=54.151.175.62\\SQLEXPRESS,45650;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#endif

            using (SqlConnection oConnection = new SqlConnection(con))
            {
                oConnection.Open();
            }

            //getShopBrand(data);
            string retr = "1";
            var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();

            //DatabaseSQL EDB = new DatabaseSQL(data.DatabasePathErasoft);
            //var resultExecDefault = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST = '" + data.no_cust + "'");

            string responseFromServer = "";
            bool responseApi = false;
            int retry = 0;
            while (!responseApi && retry <= 3)
            {
                data = RefreshToken(data);
                var sysParams = new Dictionary<string, string>();
                this.ParamJson = "{\"venderId\":\"" + data.merchant_code + "\"}";
                sysParams.Add("360buy_param_json", this.ParamJson);

                sysParams.Add("access_token", data.accessToken);
                sysParams.Add("app_key", data.appKey);
                this.Method = "jingdong.seller.category.api.read.getAllCategory";
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
                catch (WebException ex)
                {
                    retry = retry + 1;
                    string err1 = "";
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        WebResponse resp1 = ex.Response;
                        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                        {
                            err1 = sr1.ReadToEnd();
                            retr = "1";
                        }
                    }
                    retr = "1";
                }
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDGetCategoryV2)) as JDIDGetCategoryV2;
                if (result.jingdong_seller_category_api_read_getAllCategory_response.returnType != null)
                {
                    if (result.jingdong_seller_category_api_read_getAllCategory_response.returnType.success)
                    {
                        if (result.jingdong_seller_category_api_read_getAllCategory_response.returnType.model != null)
                        {
                            if (result.jingdong_seller_category_api_read_getAllCategory_response.returnType.model.Count() > 0)
                            {
                                var successInsert = false;
                                //contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Akun marketplace " + data.email.ToString() + " (JD.ID) berhasil aktif", true);
                                //EDB.ExecuteSQL("CString", CommandType.Text, "Update ARF01 SET STATUS_API = '1' WHERE TOKEN = '" + data.accessToken + "' AND API_KEY = '" + data.appKey + "'");
                                //string dbPath = "";

                                //var sessionAccount = System.Web.HttpContext.Current.Session["SessionAccount"];
                                //var sessionAccountDatabasePathErasoft = System.Web.HttpContext.Current.Session["SessionAccountDatabasePathErasoft"];

                                //var sessionUser = System.Web.HttpContext.Current.Session["SessionUser"];
                                //var sessionUserAccountID = System.Web.HttpContext.Current.Session["SessionUserAccountID"];

                                //if (sessionAccount != null)
                                //{
                                //    dbPath = sessionAccountDatabasePathErasoft.ToString();
                                //}
                                //else
                                //{
                                //    if (sessionUser != null)
                                //    {
                                //        var userAccID = Convert.ToInt64(sessionUserAccountID);
                                //        var accFromUser = MoDbContext.Account.Single(a => a.AccountId == userAccID);
                                //        dbPath = accFromUser.DatabasePathErasoft;
                                //    }
                                //}
                                //var listKtg = ErasoftDbContext.CATEGORY_JDID.ToList();
                                //if (listKtg.Count > 0)
                                //{
                                //    EDB.ExecuteSQL("CString", CommandType.Text, "DELETE FROM CATEGORY_JDID");
                                //}

                                using (SqlConnection oConnection = new SqlConnection(con))
                                {

                                    oConnection.Open();
                                    //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                                    //{
                                    using (SqlCommand oCommand = oConnection.CreateCommand())
                                    {
                                        //oCommand.CommandText = "DELETE FROM [CATEGORY_BLIBLI] WHERE ARF01_SORT1_CUST='" + data.merchant_code + "'";
                                        //oCommand.ExecuteNonQuery();
                                        oCommand.CommandText = "DELETE FROM [CATEGORY_JDID]";
                                        oCommand.ExecuteNonQuery();
                                        //oCommand.Transaction = oTransaction;
                                        oCommand.CommandType = CommandType.Text;
                                        //oCommand.CommandText = "INSERT INTO [CATEGORY_JDID] ([CATEGORY_CODE], [CATEGORY_NAME], [CATE_STATE], [TYPE], [LEAF], [PARENT_CODE]) VALUES (@CATEGORY_CODE, @CATEGORY_NAME, @CATE_STATE, @TYPE, @LEAF, @PARENT_CODE)";
                                        oCommand.CommandText = "INSERT INTO [CATEGORY_JDID] ([CATEGORY_CODE], [CATEGORY_NAME], [CATE_STATE], [TYPE], [LEAF], [PARENT_CODE], [IS_LAST_NODE], [MASTER_CATEGORY_CODE]) VALUES (@CATEGORY_CODE, @CATEGORY_NAME, @CATE_STATE, @TYPE, @LEAF, @PARENT_CODE, @IS_LAST_NODE, @MASTER_CATEGORY_CODE)";
                                        //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));
                                        oCommand.Parameters.Add(new SqlParameter("@CATE_STATE", SqlDbType.NVarChar, 3));
                                        oCommand.Parameters.Add(new SqlParameter("@TYPE", SqlDbType.NVarChar, 3));
                                        oCommand.Parameters.Add(new SqlParameter("@LEAF", SqlDbType.NVarChar, 1));
                                        oCommand.Parameters.Add(new SqlParameter("@PARENT_CODE", SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@IS_LAST_NODE", SqlDbType.NVarChar, 3));
                                        oCommand.Parameters.Add(new SqlParameter("@MASTER_CATEGORY_CODE", SqlDbType.NVarChar, 1));

                                        try
                                        {
                                            MoDbContext = new MoDbContext("");
                                            foreach (var item in result.jingdong_seller_category_api_read_getAllCategory_response.returnType.model) //foreach parent level top
                                            {
                                                oCommand.Parameters[0].Value = item.id;
                                                oCommand.Parameters[1].Value = item.name;
                                                oCommand.Parameters[2].Value = "1";
                                                oCommand.Parameters[3].Value = item.level;
                                                oCommand.Parameters[4].Value = "1";
                                                var parentID = "";
                                                if (!string.IsNullOrEmpty(item.parentId.ToString()))
                                                {
                                                    parentID = item.parentId.ToString();
                                                }
                                                oCommand.Parameters[5].Value = parentID;
                                                oCommand.Parameters[6].Value = (item.parentId != null ? "1" : "0");
                                                oCommand.Parameters[7].Value = "";
                                                try
                                                {
                                                    if (oCommand.ExecuteNonQuery() > 0)
                                                    {
                                                        successInsert = true;
                                                        if (item.parentId != null)
                                                        {
                                                            var cekCatParent = MoDbContext.CATEGORY_JDID.Where(a => a.CATEGORY_CODE == parentID).FirstOrDefault();
                                                            if (cekCatParent != null)
                                                            {
                                                                if (!string.IsNullOrEmpty(cekCatParent.MASTER_CATEGORY_CODE))
                                                                {
                                                                    var execQuery = MoDbContext.Database.ExecuteSqlCommand("UPDATE MO..CATEGORY_JDID SET MASTER_CATEGORY_CODE = '" + cekCatParent.MASTER_CATEGORY_CODE + "' WHERE CATEGORY_CODE = '" + item.id + "' AND CATEGORY_NAME = '" + item.name + "' AND TYPE = '" + item.level + "' AND PARENT_CODE = '" + parentID + "'");
                                                                }
                                                                else
                                                                {
                                                                    var execQuery = MoDbContext.Database.ExecuteSqlCommand("UPDATE MO..CATEGORY_JDID SET MASTER_CATEGORY_CODE = '" + cekCatParent.CATEGORY_CODE + "' WHERE CATEGORY_CODE = '" + item.id + "' AND CATEGORY_NAME = '" + item.name + "' AND TYPE = '" + item.level + "' AND PARENT_CODE = '" + parentID + "'");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                //var MASTER_CATEGORY = JDID_getCategoryParentV2(data, oCommand, item);
                                                                var MASTER_CATEGORY = await JDID_getCategoryParentV2(data, oCommand, item);
                                                                //oCommand.Parameters[5].Value = MASTER_CATEGORY;
                                                                if (!string.IsNullOrEmpty(MASTER_CATEGORY))
                                                                {

                                                                    var execQuery = MoDbContext.Database.ExecuteSqlCommand("UPDATE MO..CATEGORY_JDID SET MASTER_CATEGORY_CODE = '" + MASTER_CATEGORY + "' WHERE CATEGORY_CODE = '" + item.id + "' AND CATEGORY_NAME = '" + item.name + "' AND TYPE = '" + item.level + "' AND PARENT_CODE = '" + parentID + "'");
                                                                    //var execQuery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE MO..CATEGORY_JDID SET MASTER_CATEGORY_CODE = '" + MASTER_CATEGORY + "' WHERE CATEGORY_CODE = '" + item.id + "' AND CATEGORY_NAME = '" + item.name + "' AND TYPE = '" + item.level + "' AND PARENT_CODE = '" + parentID + "'");
                                                                    //oCommand.CommandText = "UPDATE MO..CATEGORY_JDID SET MASTER_CATEGORY_CODE = '" + MASTER_CATEGORY + "' WHERE CATEGORY_CODE = '" + item.id + "' AND CATEGORY_NAME = '" + item.name + "' AND TYPE = '" + item.level + "' AND PARENT_CODE = '" + parentID + "'";
                                                                    //if (oCommand.ExecuteNonQuery() > 0)
                                                                    //{

                                                                    //}
                                                                }
                                                                else
                                                                {
                                                                    var cekCatParentLagi = MoDbContext.CATEGORY_JDID.Where(a => a.CATEGORY_CODE == parentID).FirstOrDefault();
                                                                    if (cekCatParent != null)
                                                                    {
                                                                        if (!string.IsNullOrEmpty(cekCatParent.MASTER_CATEGORY_CODE))
                                                                        {
                                                                            var execQuery = MoDbContext.Database.ExecuteSqlCommand("UPDATE MO..CATEGORY_JDID SET MASTER_CATEGORY_CODE = '" + cekCatParent.MASTER_CATEGORY_CODE + "' WHERE CATEGORY_CODE = '" + item.id + "' AND CATEGORY_NAME = '" + item.name + "' AND TYPE = '" + item.level + "' AND PARENT_CODE = '" + parentID + "'");
                                                                        }
                                                                        else
                                                                        {
                                                                            var execQuery = MoDbContext.Database.ExecuteSqlCommand("UPDATE MO..CATEGORY_JDID SET MASTER_CATEGORY_CODE = '" + cekCatParent.CATEGORY_CODE + "' WHERE CATEGORY_CODE = '" + item.id + "' AND CATEGORY_NAME = '" + item.name + "' AND TYPE = '" + item.level + "' AND PARENT_CODE = '" + parentID + "'");
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {

                                                    }
                                                }
                                                catch (Exception ex)
                                                {

                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            //oTransaction.Rollback();
                                            successInsert = false;
                                        }
                                    }
                                }
                                if (successInsert)
                                {
                                    //MoDbContext = new MoDbContext("");
                                    //var ListCategory = MoDbContext.CATEGORY_JDID.Where(a => a.TYPE == "3").ToList();
                                    //foreach (var cat in ListCategory)
                                    //{
                                    //    var listAttributeJDID = await getAttributeV2(data, cat.CATEGORY_CODE, cat.CATEGORY_NAME);
                                    //}
                                }
                                else
                                {
                                    retr = "0";
                                }
                            }
                            else
                            {
                                retr = "0";
                            }
                        }
                        else
                        {
                            retr = "0";
                        }
                    }
                    else
                    {
                        retr = "0";
                    }
                }
                else
                {
                    retr = "0";
                }
            }
            else
            {
                retr = "0";
            }

            return retr;
        }

        public async Task<string> JDID_getCategoryParentV2(JDIDAPIData data, SqlCommand oCommand, ModelGetCategoryV2 item)
        {
            DatabaseSQL EDB = new DatabaseSQL(data.DatabasePathErasoft);
            var ret = "";

            string responseFromServer = "";
            bool responseApi = false;
            int retry = 0;
            while (!responseApi && retry <= 3)
            {
                data = RefreshToken(data);
                var sysParams = new Dictionary<string, string>();
                this.ParamJson = "{\"catId\":\"" + item.parentId + "\"}";
                sysParams.Add("360buy_param_json", this.ParamJson);

                sysParams.Add("access_token", data.accessToken);
                sysParams.Add("app_key", data.appKey);
                this.Method = "jingdong.seller.category.api.read.getCategoryByCatIds"; //query category information by category id， this API only for POP sellers
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
                catch (WebException ex)
                {
                    retry = retry + 1;
                    string err1 = "";
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        WebResponse resp1 = ex.Response;
                        using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                        {
                            err1 = sr1.ReadToEnd();
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDGetCategoryParentV2)) as JDIDGetCategoryParentV2;
                if (result.jingdong_seller_category_api_read_getCategoryByCatIds_response.returnType != null)
                {
                    if (result.jingdong_seller_category_api_read_getCategoryByCatIds_response.returnType.success)
                    {
                        if (result.jingdong_seller_category_api_read_getCategoryByCatIds_response.returnType.model != null)
                        {
                            if (result.jingdong_seller_category_api_read_getCategoryByCatIds_response.returnType.model.Count() > 0)
                            {
                                var detail = result.jingdong_seller_category_api_read_getCategoryByCatIds_response.returnType.model.FirstOrDefault();
                                if (detail != null)
                                {
                                    oCommand.Parameters[0].Value = detail.id;
                                    oCommand.Parameters[1].Value = detail.name;

                                    oCommand.Parameters[2].Value = "1";
                                    oCommand.Parameters[3].Value = detail.level;
                                    oCommand.Parameters[4].Value = "0";
                                    var parentIDParent = "";
                                    if (!string.IsNullOrEmpty(detail.parentId.ToString()))
                                    {
                                        parentIDParent = detail.parentId.ToString();
                                    }
                                    oCommand.Parameters[5].Value = parentIDParent;
                                    oCommand.Parameters[6].Value = (detail.parentId != null ? "1" : "0");
                                    oCommand.Parameters[7].Value = "";
                                    try
                                    {
                                        if (oCommand.ExecuteNonQuery() > 0)
                                        {
                                            if (detail.parentId != null)
                                            {
                                                var cekCatParent = MoDbContext.CATEGORY_JDID.Where(a => a.CATEGORY_CODE == parentIDParent).FirstOrDefault();
                                                if (cekCatParent != null)
                                                {
                                                    if (!string.IsNullOrEmpty(cekCatParent.MASTER_CATEGORY_CODE))
                                                    {
                                                        var execQuery = MoDbContext.Database.ExecuteSqlCommand("UPDATE MO..CATEGORY_JDID SET MASTER_CATEGORY_CODE = '" + cekCatParent.MASTER_CATEGORY_CODE + "' WHERE CATEGORY_CODE = '" + detail.id + "' AND CATEGORY_NAME = '" + detail.name + "' AND TYPE = '" + detail.level + "' AND PARENT_CODE = '" + parentIDParent + "'");
                                                        ret = cekCatParent.MASTER_CATEGORY_CODE;
                                                    }
                                                    else
                                                    {
                                                        var execQuery = MoDbContext.Database.ExecuteSqlCommand("UPDATE MO..CATEGORY_JDID SET MASTER_CATEGORY_CODE = '" + cekCatParent.CATEGORY_CODE + "' WHERE CATEGORY_CODE = '" + detail.id + "' AND CATEGORY_NAME = '" + detail.name + "' AND TYPE = '" + detail.level + "' AND PARENT_CODE = '" + parentIDParent + "'");
                                                        ret = cekCatParent.CATEGORY_CODE;
                                                    }
                                                }
                                                else
                                                {
                                                    //var MASTER_CATEGORY = JDID_getCategoryParentV2(data, oCommand, detail);
                                                    var MASTER_CATEGORY = await JDID_getCategoryParentV2(data, oCommand, detail);
                                                    //oCommand.Parameters[5].Value = MASTER_CATEGORY;
                                                    if (!string.IsNullOrEmpty(MASTER_CATEGORY))
                                                    {
                                                        //var execQuery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE MO..CATEGORY_JDID SET MASTER_CATEGORY_CODE = '" + MASTER_CATEGORY + "' WHERE CATEGORY_CODE = '" + detail.id + "' AND CATEGORY_NAME = '" + detail.name + "' AND TYPE = '" + detail.level + "' AND PARENT_CODE = '" + parentID + "'");
                                                        MoDbContext = new MoDbContext("");
                                                        var execQuery = MoDbContext.Database.ExecuteSqlCommand("UPDATE MO..CATEGORY_JDID SET MASTER_CATEGORY_CODE = '" + MASTER_CATEGORY + "' WHERE CATEGORY_CODE = '" + detail.id + "' AND CATEGORY_NAME = '" + detail.name + "' AND TYPE = '" + detail.level + "' AND PARENT_CODE = '" + parentIDParent + "'");
                                                        ret = MASTER_CATEGORY;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ret = detail.id.ToString();
                                                //oCommand.Parameters[5].Value = "";
                                            }
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
            }

            return ret;
        }

        public async Task<ATTRIBUTE_JDID> getAttributeV2(JDIDAPIData data, string catId, string catName)
        {
            MoDbContext = new MoDbContext("");
            var retAttr = new ATTRIBUTE_JDID();
            var retAttrOPt = new List<ATTRIBUTE_OPT_JDID>();

            string responseFromServer = "";
            bool responseApi = false;
            int retry = 0;
            while (!responseApi && retry <= 3)
            {
                data = RefreshToken(data);
                var sysParams = new Dictionary<string, string>();
                this.ParamJson = "{\"catId\":\"" + catId + "\"}";
                sysParams.Add("360buy_param_json", this.ParamJson);

                sysParams.Add("access_token", data.accessToken);
                sysParams.Add("app_key", data.appKey);
                this.Method = "jingdong.category.api.read.getAttributesByCatId"; //query category information by category id， this API only for POP sellers
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
                catch (WebException ex)
                {
                    if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                    {
                        retry = retry + 1;
                        string err = "";
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            WebResponse resp = ex.Response;
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                err = sr.ReadToEnd();
                                //responseFromServer = err;
                            }
                        }
                    }
                    else
                    {
                        retry = 4;
                        string err = "";
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            WebResponse resp = ex.Response;
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                err = sr.ReadToEnd();
                                //responseFromServer = err;
                            }
                        }
                        responseApi = true; break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDgetAttributeV2)) as JDIDgetAttributeV2;
                if (result.jingdong_category_api_read_getAttributesByCatId_response.returnType != null)
                {
                    if (result.jingdong_category_api_read_getAttributesByCatId_response.returnType.success)
                    {
                        if (result.jingdong_category_api_read_getAttributesByCatId_response.returnType.model != null)
                        {
                            if (result.jingdong_category_api_read_getAttributesByCatId_response.returnType.model.Count() > 0)
                            {
                                ATTRIBUTE_JDID returnData = new ATTRIBUTE_JDID();
                                int i = 0;
                                string a = "";
                                foreach (var attribs in result.jingdong_category_api_read_getAttributesByCatId_response.returnType.model)
                                {
                                    if (i < 35)
                                    {
                                        var tempRetAttrOPt = new List<ATTRIBUTE_OPT_JDID>();
                                        a = Convert.ToString(i + 1);
                                        returnData.CATEGORY_CODE = catId;
                                        returnData.CATEGORY_NAME = catName;

                                        //sSQL += "[ACODE_" + a + "],[ATYPE_" + a + "],[ANAME_" + a + "],[AOPTIONS_" + a + "],";
                                        //oCommand.Parameters[(i * 4) + 2].Value = result.value.attributes[i].attributeCode.Value;
                                        //oCommand.Parameters[(i * 4) + 3].Value = result.value.attributes[i].attributeType.Value;
                                        //oCommand.Parameters[(i * 4) + 4].Value = result.value.attributes[i].name.Value;
                                        //oCommand.Parameters[(i * 4) + 5].Value = result.value.attributes[i].options.Count > 0 ? "1" : "0";
                                        returnData["ACODE_" + a] = Convert.ToString(attribs.propertyId);
                                        returnData["ATYPE_" + a] = Convert.ToString(attribs.type);
                                        returnData["ANAME_" + a] = Convert.ToString(attribs.name);
                                        //returnData["AOPTIONS_" + a] = attribs.options.Count > 0 ? "1" : "0";

                                        if (!string.IsNullOrEmpty(Convert.ToString(attribs.propertyId)))
                                        {
                                            //var optList = attribs.options.ToList();
                                            //var listOpt = optList.Select(x => new ATTRIBUTE_OPT_BLIBLI(attribs.attributeCode.ToString(), attribs.attributeType.ToString(), attribs.name.ToString(), x)).ToList();
                                            //ret.attribute_opt.AddRange(listOpt);
                                            var attrId = Convert.ToString(attribs.propertyId);
                                            var attrName = Convert.ToString(attribs.name);
                                            var listOpt = await getAttributeOptV2(data, catId, catName, attrId, attrName, 1);
                                            retAttrOPt.AddRange(listOpt);
                                            tempRetAttrOPt.AddRange(listOpt);
                                        }
                                        if (tempRetAttrOPt.Count() > 0)
                                        {
                                            returnData["AOPTIONS_" + a] = "1";
                                        }
                                        else
                                        {
                                            returnData["AOPTIONS_" + a] = "0";
                                        }
                                    }
                                    i = i + 1;

                                }
                                retAttr = returnData;
                                try
                                {
                                    var getAttributeOld = MoDbContext.AttributeJDID.Where(b => b.CATEGORY_CODE == catId).ToList();
                                    if (getAttributeOld.Count() > 0)
                                    {
                                        MoDbContext.AttributeJDID.RemoveRange(getAttributeOld);
                                        MoDbContext.SaveChanges();
                                    }
                                    MoDbContext.AttributeJDID.Add(retAttr);
                                    MoDbContext.SaveChanges();
                                    var getAttribute = MoDbContext.AttributeJDID.Where(b => b.CATEGORY_CODE == catId).ToList();
                                    var listAttributeOpt = new List<ATTRIBUTE_OPT_JDID>();
                                    foreach (var attr in getAttribute)
                                    {
                                        var getAttributeOpt = MoDbContext.AttributeOptJDID.Where(b => b.ATTRIBUTEVALUEID == attr.ACODE_1 || b.ATTRIBUTEVALUEID == attr.ACODE_2 || b.ATTRIBUTEVALUEID == attr.ACODE_3 || b.ATTRIBUTEVALUEID == attr.ACODE_4 || b.ATTRIBUTEVALUEID == attr.ACODE_5 || b.ATTRIBUTEVALUEID == attr.ACODE_6 || b.ATTRIBUTEVALUEID == attr.ACODE_7 || b.ATTRIBUTEVALUEID == attr.ACODE_8 || b.ATTRIBUTEVALUEID == attr.ACODE_9 || b.ATTRIBUTEVALUEID == attr.ACODE_10 ||
                                                                                                        b.ATTRIBUTEVALUEID == attr.ACODE_11 || b.ATTRIBUTEVALUEID == attr.ACODE_12 || b.ATTRIBUTEVALUEID == attr.ACODE_13 || b.ATTRIBUTEVALUEID == attr.ACODE_14 || b.ATTRIBUTEVALUEID == attr.ACODE_15 || b.ATTRIBUTEVALUEID == attr.ACODE_16 || b.ATTRIBUTEVALUEID == attr.ACODE_17 || b.ATTRIBUTEVALUEID == attr.ACODE_18 || b.ATTRIBUTEVALUEID == attr.ACODE_19 || b.ATTRIBUTEVALUEID == attr.ACODE_20 ||
                                                                                                        b.ATTRIBUTEVALUEID == attr.ACODE_21 || b.ATTRIBUTEVALUEID == attr.ACODE_22 || b.ATTRIBUTEVALUEID == attr.ACODE_23 || b.ATTRIBUTEVALUEID == attr.ACODE_24 || b.ATTRIBUTEVALUEID == attr.ACODE_25 || b.ATTRIBUTEVALUEID == attr.ACODE_26 || b.ATTRIBUTEVALUEID == attr.ACODE_27 || b.ATTRIBUTEVALUEID == attr.ACODE_28 || b.ATTRIBUTEVALUEID == attr.ACODE_29 || b.ATTRIBUTEVALUEID == attr.ACODE_30 ||
                                                                                                        b.ATTRIBUTEVALUEID == attr.ACODE_31 || b.ATTRIBUTEVALUEID == attr.ACODE_32 || b.ATTRIBUTEVALUEID == attr.ACODE_33 || b.ATTRIBUTEVALUEID == attr.ACODE_34 || b.ATTRIBUTEVALUEID == attr.ACODE_35).ToList();
                                        listAttributeOpt.AddRange(getAttributeOpt);
                                    };
                                    //if (getAttribute.Count() > 0)
                                    //{
                                    //    MoDbContext.AttributeBlibli.RemoveRange(getAttribute);
                                    if (listAttributeOpt.Count() > 0)
                                    {
                                        MoDbContext.AttributeOptJDID.RemoveRange(listAttributeOpt);
                                        MoDbContext.SaveChanges();
                                    }
                                    //}
                                    MoDbContext.AttributeOptJDID.AddRange(retAttrOPt);
                                    MoDbContext.SaveChanges();
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                }
            }

            //var mgrApiManager = new JDIDController();

            //mgrApiManager.AppKey = data.appKey;
            //mgrApiManager.AppSecret = data.appSecret;
            //mgrApiManager.AccessToken = data.accessToken;

            //mgrApiManager.Method = "epi.ware.openapi.WareAttributeApi.getAttributesByCatId";
            //mgrApiManager.ParamJson = "{ \"catId\":\"" + catId + "\"}";

            ////var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
            //var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
            //if (ret != null)
            //{
            //    if (ret.openapi_code == 0)
            //    {
            //        var listAttr = JsonConvert.DeserializeObject(ret.openapi_data, typeof(AttrData)) as AttrData;
            //        if (listAttr != null)
            //        {
            //            if (listAttr.model.Count > 0)
            //            {
            //                string a = "";
            //                int i = 2;
            //                retAttr.CATEGORY_CODE = catId;

            //                retAttr["ACODE_1"] = "9170";
            //                retAttr["AVALUE_1"] = "";
            //                retAttr["ANAME_1"] = "Warna";

            //                retAttr["ACODE_2"] = "9248";
            //                retAttr["AVALUE_2"] = "";
            //                retAttr["ANAME_2"] = "Ukuran";

            //                foreach (var attr in listAttr.model)
            //                {

            //                    //a = Convert.ToString(i + 1);
            //                    //retAttr["ACODE_" + a] = Convert.ToString(attr.propertyId);
            //                    //retAttr["AVALUE_" + a] = attr.type.ToString();
            //                    //retAttr["ANAME_" + a] = attr.nameEn;
            //                    //i = i + 1;
            //                    if (!string.IsNullOrEmpty(attr.name) && !string.IsNullOrEmpty(Convert.ToString(attr.propertyId)))
            //                    {

            //                        if (!attr.name.Contains("Coming Soon")
            //                            //&& !attr.name.Contains("Warna") 
            //                            //&& !attr.name.Contains("Ukuran") 
            //                            && !attr.name.Contains("Test11")
            //                            //&& !attr.name.Contains("Network") 
            //                            && !attr.name.Contains("Operating system")
            //                            && !attr.name.Contains("Upgradable")
            //                            //&& !attr.name.Contains("OS Upgrade to") 
            //                            //&& !attr.name.Contains("Chipset") 
            //                            //&& !attr.name.Contains("CPU")
            //                            && !attr.name.Contains("GPU")
            //                            //&& !attr.name.Contains("RAM") 
            //                            //&& !attr.name.Contains("Memory Internal")
            //                            //&& !attr.name.Contains("Memory External") 
            //                            //&& !attr.name.Contains("Rear Camera 1") 
            //                            //&& !attr.name.Contains("Rear Camera 2")
            //                            //&& !attr.name.Contains("Rear Camera 3") 
            //                            //&& !attr.name.Contains("Rear Camera 4") 
            //                            //&& !attr.name.Contains("Front Camera 1")
            //                            && !attr.name.Contains("Front Camera 2")
            //                            && !attr.name.Contains("Video")
            //                            //&& !attr.name.Contains("Battery Type")
            //                            //&& !attr.name.Contains("Removable Battery") 
            //                            //&& !attr.name.Contains("Battery Capacity") 
            //                            && !attr.name.Contains("LCD Size")
            //                            //&& !attr.name.Contains("LCD Type") 
            //                            //&& !attr.name.Contains("Screen Resolution") && !attr.name.Contains("Dimensions")
            //                            //&& !attr.name.Contains("Sensor") 
            //                            //&& !attr.name.Contains("SIM Card") 
            //                            //&& !attr.name.Contains("WLAN")
            //                            //&& !attr.name.Contains("NFC") 
            //                            && !attr.name.Contains("ROM")
            //                            && !attr.name.Contains("Megapixel(MP)")
            //                            && !attr.name.Contains("MicroSD")
            //                            //&& !attr.name.Contains("OS") 
            //                            && !attr.name.Contains("Secondary")
            //                            && !attr.name.Contains("Primary")
            //                            && !attr.name.Contains("GPRS")
            //                            && !attr.name.Contains("Multitouch")
            //                            && !attr.name.Contains("EDGE")
            //                            //&& !attr.name.Contains("Dual SIM") 
            //                            && !attr.name.Contains("Screen size (inch)")
            //                            //&& !attr.name.Contains("Price") 
            //                            //&& !attr.name.Contains("Kapasitas")
            //                            )
            //                        {
            //                            if (i < 20)
            //                            {
            //                                a = Convert.ToString(i + 1);
            //                                retAttr["ACODE_" + a] = Convert.ToString(attr.propertyId) ?? "";
            //                                retAttr["AVALUE_" + a] = attr.type.ToString() ?? "";
            //                                retAttr["ANAME_" + a] = attr.nameEn ?? "";
            //                                i = i + 1;
            //                            }
            //                        }
            //                    }
            //                }
            //                for (int j = i; j < 20; j++)
            //                {
            //                    a = Convert.ToString(j + 1);
            //                    retAttr["ACODE_" + a] = "";
            //                    retAttr["AVALUE_" + a] = "";
            //                    retAttr["ANAME_" + a] = "";
            //                }
            //            }

            //        }
            //    }
            //}
            //return response;
            //return ret.openapi_data;
            return retAttr;
        }

        public async Task<List<ATTRIBUTE_OPT_JDID>> getAttributeOptV2(JDIDAPIData data, string catId, string catName, string attrId, string attrName, int page)
        {
            var mgrApiManager = new JDIDController();
            var listOpt = new List<ATTRIBUTE_OPT_JDID>();

            string responseFromServer = "";
            bool responseApi = false;
            int retry = 0;
            while (!responseApi && retry <= 3)
            {
                data = RefreshToken(data);
                var sysParams = new Dictionary<string, string>();
                this.ParamJson = "{\"catId\":\"" + catId + "\", \"attrId\":\"" + attrId + "\", \"currentPage\":\"" + page + "\", \"pageSize\":\"20\",}";
                sysParams.Add("360buy_param_json", this.ParamJson);

                sysParams.Add("access_token", data.accessToken);
                sysParams.Add("app_key", data.appKey);
                this.Method = "jingdong.category.api.read.getAttrValuesByCatIdAndAttrId"; //query the attribute value information by attribute id and category id
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
                catch (WebException ex)
                {
                    if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                    {
                        retry = retry + 1;
                        string err = "";
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            WebResponse resp = ex.Response;
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                err = sr.ReadToEnd();
                                //responseFromServer = err;
                            }
                        }
                    }
                    else
                    {
                        retry = 4;
                        string err = "";
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            WebResponse resp = ex.Response;
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                err = sr.ReadToEnd();
                                //responseFromServer = err;
                            }
                        }
                        responseApi = true; break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDgetAttributeOptV2)) as JDIDgetAttributeOptV2;
                if (result.jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response.returnType != null)
                {
                    if (result.jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response.returnType.success)
                    {
                        if (result.jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response.returnType.model != null)
                        {
                            if(result.jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response.returnType.model.result.Count() > 0)
                            {
                                foreach (var opt in result.jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response.returnType.model.result)
                                {
                                    var newOpt = new ATTRIBUTE_OPT_JDID()
                                    {
                                        //ACODE = opt.attributeValueId.ToString(),
                                        //OPTION_VALUE = opt.nameEn
                                        ACODE = opt.attributeValueId.ToString(),
                                        ANAME = attrName,
                                        SORT = opt.sort.ToString(),
                                        ATTRIBUTEVALUEID = opt.attributeId.ToString(),
                                        OPTION_VALUE = opt.nameEn,
                                        OPTION_NAMEEN = opt.name
                                    };
                                    listOpt.Add(newOpt);
                                }
                                //if(retOpt.model.data.Count == 20)
                                //{
                                var cursiveOpt = await getAttributeOptV2(data, catId, catName, attrId, attrName, page + 1);
                                if (cursiveOpt.Count > 0)
                                {
                                    foreach (var opt2 in cursiveOpt)
                                    {
                                        listOpt.Add(opt2);
                                    }
                                }
                            }
                        }
                    }
                }
            }


            //                mgrApiManager.AppKey = data.appKey;
            //mgrApiManager.AppSecret = data.appSecret;
            //mgrApiManager.AccessToken = data.accessToken;

            //mgrApiManager.Method = "epi.ware.openapi.WareAttributeApi.getAttrValuesByCatIdAndAttrId";
            //mgrApiManager.ParamJson = "{ \"catId\":\"" + catId + "\", \"attrId\":\"" + attrId + "\", \"currentPage\":" + page + ", \"pageSize\":20}";

            ////var response = mgrApiManager.Call(data.appKey, data.accessToken, data.appSecret);
            ////var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
            //if (ret != null)
            //{
            //    if (ret.openapi_code == 0)
            //    {
            //        var retOpt = JsonConvert.DeserializeObject(ret.openapi_data, typeof(JDID_ATTRIBUTE_OPT)) as JDID_ATTRIBUTE_OPT;
            //        if (retOpt != null)
            //        {
            //            if (retOpt.model != null)
            //            {
            //                if (retOpt.model.data.Count > 0)
            //                {
            //                    foreach (var opt in retOpt.model.data)
            //                    {
            //                        var newOpt = new ATTRIBUTE_OPT_JDID()
            //                        {
            //                            ACODE = opt.attributeValueId.ToString(),
            //                            OPTION_VALUE = opt.nameEn
            //                        };
            //                        listOpt.Add(newOpt);
            //                    }
            //                    //if(retOpt.model.data.Count == 20)
            //                    //{
            //                    var cursiveOpt = getAttributeOpt(data, catId, attrId, page + 1);
            //                    if (cursiveOpt.Count > 0)
            //                    {
            //                        foreach (var opt2 in cursiveOpt)
            //                        {
            //                            listOpt.Add(opt2);
            //                        }
            //                    }
            //                    //}
            //                }
            //            }

            //        }
            //    }
            //}


            return listOpt;
        }

        //add by nurul 3/8/2021
        public ATTRIBUTE_JDID getAttributeV2_Baru(JDIDAPIData data, string catId, string catName)
        {
            MoDbContext = new MoDbContext("");
            var retAttr = new ATTRIBUTE_JDID();
            var retAttrOPt = new List<ATTRIBUTE_OPT_JDID>();

            string responseFromServer = "";
            bool responseApi = false;
            int retry = 0;
            while (!responseApi && retry <= 3)
            {
                data = RefreshToken(data);
                var sysParams = new Dictionary<string, string>();
                this.ParamJson = "{\"catId\":\"" + catId + "\"}";
                sysParams.Add("360buy_param_json", this.ParamJson);

                sysParams.Add("access_token", data.accessToken);
                sysParams.Add("app_key", data.appKey);
                this.Method = "jingdong.category.api.read.getAttributesByCatId"; //query category information by category id， this API only for POP sellers
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
                catch (WebException ex)
                {
                    if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                    {
                        retry = retry + 1;
                        string err = "";
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            WebResponse resp = ex.Response;
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                err = sr.ReadToEnd();
                            }
                        }
                    }
                    else
                    {
                        retry = 4;
                        string err = "";
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            WebResponse resp = ex.Response;
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                err = sr.ReadToEnd();
                            }
                        }
                        responseApi = true; break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDgetAttributeV2)) as JDIDgetAttributeV2;
                if (result.jingdong_category_api_read_getAttributesByCatId_response.returnType != null)
                {
                    if (result.jingdong_category_api_read_getAttributesByCatId_response.returnType.success)
                    {
                        if (result.jingdong_category_api_read_getAttributesByCatId_response.returnType.model != null)
                        {
                            if (result.jingdong_category_api_read_getAttributesByCatId_response.returnType.model.Count() > 0)
                            {
                                ATTRIBUTE_JDID returnData = new ATTRIBUTE_JDID();
                                int i = 0;
                                string a = "";
                                foreach (var attribs in result.jingdong_category_api_read_getAttributesByCatId_response.returnType.model)
                                {
                                    if (i < 35)
                                    {
                                        var tempRetAttrOPt = new List<ATTRIBUTE_OPT_JDID>();
                                        a = Convert.ToString(i + 1);
                                        returnData.CATEGORY_CODE = catId;
                                        returnData.CATEGORY_NAME = catName;
                                        
                                        returnData["ACODE_" + a] = Convert.ToString(attribs.propertyId);
                                        returnData["ATYPE_" + a] = Convert.ToString(attribs.type);
                                        returnData["ANAME_" + a] = Convert.ToString(attribs.name);

                                        if (!string.IsNullOrEmpty(Convert.ToString(attribs.propertyId)))
                                        {
                                            var attrId = Convert.ToString(attribs.propertyId);
                                            var attrName = Convert.ToString(attribs.name);
                                            var listOpt = getAttributeOptV2_Baru(data, catId, catName, attrId, attrName, 1, 2);
                                            retAttrOPt.AddRange(listOpt);
                                            tempRetAttrOPt.AddRange(listOpt);
                                        }
                                        if (tempRetAttrOPt.Count() > 0)
                                        {
                                            returnData["AOPTIONS_" + a] = "1";
                                        }
                                        else
                                        {
                                            returnData["AOPTIONS_" + a] = "0";
                                        }
                                    }
                                    i = i + 1;

                                }
                                retAttr = returnData;
                                //try
                                //{
                                //    var getAttributeOld = MoDbContext.AttributeJDID.Where(b => b.CATEGORY_CODE == catId).ToList();
                                //    if (getAttributeOld.Count() > 0)
                                //    {
                                //        MoDbContext.AttributeJDID.RemoveRange(getAttributeOld);
                                //        MoDbContext.SaveChanges();
                                //    }
                                //    MoDbContext.AttributeJDID.Add(retAttr);
                                //    MoDbContext.SaveChanges();
                                //    var getAttribute = MoDbContext.AttributeJDID.Where(b => b.CATEGORY_CODE == catId).ToList();
                                //    var listAttributeOpt = new List<ATTRIBUTE_OPT_JDID>();
                                //    foreach (var attr in getAttribute)
                                //    {
                                //        var getAttributeOpt = MoDbContext.AttributeOptJDID.Where(b => b.ATTRIBUTEVALUEID == attr.ACODE_1 || b.ATTRIBUTEVALUEID == attr.ACODE_2 || b.ATTRIBUTEVALUEID == attr.ACODE_3 || b.ATTRIBUTEVALUEID == attr.ACODE_4 || b.ATTRIBUTEVALUEID == attr.ACODE_5 || b.ATTRIBUTEVALUEID == attr.ACODE_6 || b.ATTRIBUTEVALUEID == attr.ACODE_7 || b.ATTRIBUTEVALUEID == attr.ACODE_8 || b.ATTRIBUTEVALUEID == attr.ACODE_9 || b.ATTRIBUTEVALUEID == attr.ACODE_10 ||
                                //                                                                        b.ATTRIBUTEVALUEID == attr.ACODE_11 || b.ATTRIBUTEVALUEID == attr.ACODE_12 || b.ATTRIBUTEVALUEID == attr.ACODE_13 || b.ATTRIBUTEVALUEID == attr.ACODE_14 || b.ATTRIBUTEVALUEID == attr.ACODE_15 || b.ATTRIBUTEVALUEID == attr.ACODE_16 || b.ATTRIBUTEVALUEID == attr.ACODE_17 || b.ATTRIBUTEVALUEID == attr.ACODE_18 || b.ATTRIBUTEVALUEID == attr.ACODE_19 || b.ATTRIBUTEVALUEID == attr.ACODE_20 ||
                                //                                                                        b.ATTRIBUTEVALUEID == attr.ACODE_21 || b.ATTRIBUTEVALUEID == attr.ACODE_22 || b.ATTRIBUTEVALUEID == attr.ACODE_23 || b.ATTRIBUTEVALUEID == attr.ACODE_24 || b.ATTRIBUTEVALUEID == attr.ACODE_25 || b.ATTRIBUTEVALUEID == attr.ACODE_26 || b.ATTRIBUTEVALUEID == attr.ACODE_27 || b.ATTRIBUTEVALUEID == attr.ACODE_28 || b.ATTRIBUTEVALUEID == attr.ACODE_29 || b.ATTRIBUTEVALUEID == attr.ACODE_30 ||
                                //                                                                        b.ATTRIBUTEVALUEID == attr.ACODE_31 || b.ATTRIBUTEVALUEID == attr.ACODE_32 || b.ATTRIBUTEVALUEID == attr.ACODE_33 || b.ATTRIBUTEVALUEID == attr.ACODE_34 || b.ATTRIBUTEVALUEID == attr.ACODE_35).ToList();
                                //        listAttributeOpt.AddRange(getAttributeOpt);
                                //    };
                                //    if (listAttributeOpt.Count() > 0)
                                //    {
                                //        MoDbContext.AttributeOptJDID.RemoveRange(listAttributeOpt);
                                //        MoDbContext.SaveChanges();
                                //    }
                                //    MoDbContext.AttributeOptJDID.AddRange(retAttrOPt);
                                //    MoDbContext.SaveChanges();
                                //}
                                //catch (Exception ex)
                                //{

                                //}
                            }
                        }
                    }
                }
            }
            return retAttr;
        }

        public List<ATTRIBUTE_OPT_JDID> getAttributeOptV2_Baru(JDIDAPIData data, string catId, string catName, string attrId, string attrName, int page, int type )
        {
            var mgrApiManager = new JDIDController();
            var listOpt = new List<ATTRIBUTE_OPT_JDID>();

            string responseFromServer = "";
            bool responseApi = false;
            int retry = 0;
            while (!responseApi && retry <= 3)
            {
                data = RefreshToken(data);
                var sysParams = new Dictionary<string, string>();
                this.ParamJson = "{\"catId\":\"" + catId + "\", \"attrId\":\"" + attrId + "\", \"currentPage\":\"" + page + "\", \"pageSize\":\"20\",}";
                sysParams.Add("360buy_param_json", this.ParamJson);

                sysParams.Add("access_token", data.accessToken);
                sysParams.Add("app_key", data.appKey);
                this.Method = "jingdong.category.api.read.getAttrValuesByCatIdAndAttrId"; //query the attribute value information by attribute id and category id
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
                catch (WebException ex)
                {
                    if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                    {
                        retry = retry + 1;
                        string err = "";
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            WebResponse resp = ex.Response;
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                err = sr.ReadToEnd();
                                //responseFromServer = err;
                            }
                        }
                    }
                    else
                    {
                        retry = 4;
                        string err = "";
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            WebResponse resp = ex.Response;
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                err = sr.ReadToEnd();
                            }
                        }
                        responseApi = true; break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDgetAttributeOptV2)) as JDIDgetAttributeOptV2;
                if (result.jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response.returnType != null)
                {
                    if (result.jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response.returnType.success)
                    {
                        if (result.jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response.returnType.model != null)
                        {
                            if (result.jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response.returnType.model.result.Count() > 0)
                            {
                                foreach (var opt in result.jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response.returnType.model.result)
                                {
                                    var newOpt = new ATTRIBUTE_OPT_JDID()
                                    {
                                        //ACODE = opt.attributeValueId.ToString(),
                                        //OPTION_VALUE = opt.nameEn
                                        ACODE = opt.attributeValueId.ToString(),
                                        ANAME = attrName,
                                        SORT = opt.sort.ToString(),
                                        ATTRIBUTEVALUEID = opt.attributeId.ToString(),
                                        OPTION_VALUE = opt.nameEn,
                                        OPTION_NAMEEN = opt.name
                                    };
                                    listOpt.Add(newOpt);
                                }
                                //if(retOpt.model.data.Count == 20)
                                //{
                                if (type == 1)
                                {
                                    var cursiveOpt = getAttributeOptV2_Baru(data, catId, catName, attrId, attrName, page + 1, type);
                                    if (cursiveOpt.Count > 0)
                                    {
                                        foreach (var opt2 in cursiveOpt)
                                        {
                                            listOpt.Add(opt2);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return listOpt;
        }
        //end add by nurul 3/8/2021

        public ATTRIBUTE_JDID getAttributeV2Lama(JDIDAPIData data, string catId)
        {
            var retAttr = new ATTRIBUTE_JDID();
            var retAttrOPt = new List<ATTRIBUTE_OPT_JDID>();

            string responseFromServer = "";
            bool responseApi = false;
            int retry = 0;
            while (!responseApi && retry <= 3)
            {
                var sysParams = new Dictionary<string, string>();
                this.ParamJson = "{\"catId\":\"" + catId + "\"}";
                sysParams.Add("360buy_param_json", this.ParamJson);

                sysParams.Add("access_token", data.accessToken);
                sysParams.Add("app_key", data.appKey);
                this.Method = "jingdong.category.api.read.getAttributesByCatId"; //query category information by category id， this API only for POP sellers
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
                catch (WebException ex)
                {
                    if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                    {
                        retry = retry + 1;
                        string err = "";
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            WebResponse resp = ex.Response;
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                err = sr.ReadToEnd();
                                //responseFromServer = err;
                            }
                        }
                    }
                    else
                    {
                        retry = 4;
                        string err = "";
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            WebResponse resp = ex.Response;
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                err = sr.ReadToEnd();
                                //responseFromServer = err;
                            }
                        }
                        responseApi = true; break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDgetAttributeV2)) as JDIDgetAttributeV2;
                if (result.jingdong_category_api_read_getAttributesByCatId_response.returnType != null)
                {
                    if (result.jingdong_category_api_read_getAttributesByCatId_response.returnType.success)
                    {
                        if (result.jingdong_category_api_read_getAttributesByCatId_response.returnType.model != null)
                        {
                            if (result.jingdong_category_api_read_getAttributesByCatId_response.returnType.model.Count() > 0)
                            {
                                string a = "";
                                int i = 2;
                                retAttr.CATEGORY_CODE = catId;

                                retAttr["ACODE_1"] = "9170";
                                retAttr["AVALUE_1"] = "";
                                retAttr["ANAME_1"] = "Warna";

                                retAttr["ACODE_2"] = "9248";
                                retAttr["AVALUE_2"] = "";
                                retAttr["ANAME_2"] = "Ukuran";
                                foreach (var attr in result.jingdong_category_api_read_getAttributesByCatId_response.returnType.model)
                                {
                                    if (!string.IsNullOrEmpty(attr.name) && !string.IsNullOrEmpty(Convert.ToString(attr.propertyId)))
                                    {
                                        if (!attr.name.Contains("Coming Soon")
                                            //&& !attr.name.Contains("Warna") 
                                            //&& !attr.name.Contains("Ukuran") 
                                            && !attr.name.Contains("Test11")
                                            //&& !attr.name.Contains("Network") 
                                            && !attr.name.Contains("Operating system")
                                            && !attr.name.Contains("Upgradable")
                                            //&& !attr.name.Contains("OS Upgrade to") 
                                            //&& !attr.name.Contains("Chipset") 
                                            //&& !attr.name.Contains("CPU")
                                            && !attr.name.Contains("GPU")
                                            //&& !attr.name.Contains("RAM") 
                                            //&& !attr.name.Contains("Memory Internal")
                                            //&& !attr.name.Contains("Memory External") 
                                            //&& !attr.name.Contains("Rear Camera 1") 
                                            //&& !attr.name.Contains("Rear Camera 2")
                                            //&& !attr.name.Contains("Rear Camera 3") 
                                            //&& !attr.name.Contains("Rear Camera 4") 
                                            //&& !attr.name.Contains("Front Camera 1")
                                            && !attr.name.Contains("Front Camera 2")
                                            && !attr.name.Contains("Video")
                                            //&& !attr.name.Contains("Battery Type")
                                            //&& !attr.name.Contains("Removable Battery") 
                                            //&& !attr.name.Contains("Battery Capacity") 
                                            && !attr.name.Contains("LCD Size")
                                            //&& !attr.name.Contains("LCD Type") 
                                            //&& !attr.name.Contains("Screen Resolution") && !attr.name.Contains("Dimensions")
                                            //&& !attr.name.Contains("Sensor") 
                                            //&& !attr.name.Contains("SIM Card") 
                                            //&& !attr.name.Contains("WLAN")
                                            //&& !attr.name.Contains("NFC") 
                                            && !attr.name.Contains("ROM")
                                            && !attr.name.Contains("Megapixel(MP)")
                                            && !attr.name.Contains("MicroSD")
                                            //&& !attr.name.Contains("OS") 
                                            && !attr.name.Contains("Secondary")
                                            && !attr.name.Contains("Primary")
                                            && !attr.name.Contains("GPRS")
                                            && !attr.name.Contains("Multitouch")
                                            && !attr.name.Contains("EDGE")
                                            //&& !attr.name.Contains("Dual SIM") 
                                            && !attr.name.Contains("Screen size (inch)")
                                            //&& !attr.name.Contains("Price") 
                                            //&& !attr.name.Contains("Kapasitas")
                                            )
                                        {
                                            if (i < 20)
                                            {
                                                a = Convert.ToString(i + 1);
                                                retAttr["ACODE_" + a] = Convert.ToString(attr.propertyId) ?? "";
                                                retAttr["AVALUE_" + a] = attr.type.ToString() ?? "";
                                                retAttr["ANAME_" + a] = attr.nameEn ?? "";
                                                i = i + 1;
                                            }
                                        }
                                    }
                                }
                                for (int j = i; j < 20; j++)
                                {
                                    a = Convert.ToString(j + 1);
                                    retAttr["ACODE_" + a] = "";
                                    retAttr["AVALUE_" + a] = "";
                                    retAttr["ANAME_" + a] = "";
                                }
                            }
                        }
                    }
                }
            }

            
            return retAttr;
        }

        public List<ATTRIBUTE_OPT_JDID> getAttributeOptV2Lama(JDIDAPIData data, string catId, string attrId, int page)
        {
            var listOpt = new List<ATTRIBUTE_OPT_JDID>();
            string responseFromServer = "";
            bool responseApi = false;
            int retry = 0;
            while (!responseApi && retry <= 3)
            {
                var sysParams = new Dictionary<string, string>();
                this.ParamJson = "{\"catId\":\"" + catId + "\", \"attrId\":\"" + attrId + "\", \"currentPage\":\"" + page + "\", \"pageSize\":\"20\",}";
                sysParams.Add("360buy_param_json", this.ParamJson);

                sysParams.Add("access_token", data.accessToken);
                sysParams.Add("app_key", data.appKey);
                this.Method = "jingdong.category.api.read.getAttrValuesByCatIdAndAttrId"; //query the attribute value information by attribute id and category id
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
                catch (WebException ex)
                {
                    if (ex.Message.Contains("The remote name could not be resolved: 'open-api.jd.id'"))
                    {
                        retry = retry + 1;
                        string err = "";
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            WebResponse resp = ex.Response;
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                err = sr.ReadToEnd();
                                //responseFromServer = err;
                            }
                        }
                    }
                    else
                    {
                        retry = 4;
                        string err = "";
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            WebResponse resp = ex.Response;
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                err = sr.ReadToEnd();
                                //responseFromServer = err;
                            }
                        }
                        responseApi = true; break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(JDIDgetAttributeOptV2)) as JDIDgetAttributeOptV2;
                if (result.jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response.returnType != null)
                {
                    if (result.jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response.returnType.success)
                    {
                        if (result.jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response.returnType.model != null)
                        {
                            if (result.jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response.returnType.model.result.Count() > 0)
                            {
                                foreach (var opt in result.jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response.returnType.model.result)
                                {
                                    var newOpt = new ATTRIBUTE_OPT_JDID()
                                    {
                                        //ACODE = opt.attributeValueId.ToString(),
                                        //OPTION_VALUE = opt.nameEn
                                        ACODE = opt.attributeValueId.ToString(),
                                        //ANAME = attrName,
                                        SORT = opt.sort.ToString(),
                                        ATTRIBUTEVALUEID = opt.attributeId.ToString(),
                                        OPTION_VALUE = opt.nameEn,
                                        OPTION_NAMEEN = opt.name
                                    };
                                    listOpt.Add(newOpt);
                                }
                                //if(retOpt.model.data.Count == 20)
                                //{
                                var cursiveOpt = getAttributeOptV2Lama(data, catId, attrId, page + 1);
                                if (cursiveOpt.Count > 0)
                                {
                                    foreach (var opt2 in cursiveOpt)
                                    {
                                        listOpt.Add(opt2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return listOpt;
        }

        public JDIDAPIData RefreshToken(JDIDAPIData data)
        {
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
        //end add by nurul 27/5/2021, JDID versi 2 tahap 2 
    }

    #region jdid data class

    public class Model_Promo
    {
        public bool success { get; set; }
        public int code { get; set; }
        public string description { get; set; }
        public string order_id { get; set; }
    }

    public class JD_HeaderTipe
    {
        public List<JD_TipeItem> data { get; set; }
    }

    public class JD_TipeItem
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class JD_HeaderAfterSale
    {
        public List<JD_AfterSale> data { get; set; }
    }

    public class JD_AfterSale
    {
        public string code { get; set; }
        public string desc { get; set; }
    }

    public class JD_HeaderJD_Warranty
    {
        public List<JD_Warranty> data { get; set; }
    }

    public class JD_Warranty
    {
        public string code { get; set; }
        public string desc { get; set; }
    }

    public class Data_OrderDetail
    {
        public string message { get; set; }
        public string model { get; set; }
        public int code { get; set; }
        public bool success { get; set; }
    }

    public class ModelOrder
    {
        public List<DetailOrder_JD> data { get; set; }
    }

    public class DetailOrder_JD
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
    }

    public class Orderskuinfo
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


    public class List_Order_JD
    {
        public List<string> orderIds { get; set; }
    }

    public class Data_OrderIds
    {
        public string message { get; set; }
        public List<long> model { get; set; }
        public int code { get; set; }
        public bool success { get; set; }
    }

    public class Data_Detail_Product
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<Model_Detail_Product> model { get; set; }
        public bool success { get; set; }
    }

    public class Model_Detail_Product
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

    public class Data_Detail_Brand
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<Model_Detail_Brand> model { get; set; }
        public bool success { get; set; }
    }

    public class Model_Detail_Brand
    {
        public int isForever { get; set; }
        public Branddto brandDto { get; set; }
        public long endDate { get; set; }
        public int brandId { get; set; }
        public int shopId { get; set; }
        public int id { get; set; }
        public long startDate { get; set; }
        public int status { get; set; }
    }

    public class Branddto
    {
        public string firstChar { get; set; }
        public string logoImg { get; set; }
        public long modifyDate { get; set; }
        public string nameInd { get; set; }
        public int origin { get; set; }
        public string nameCn { get; set; }
        public int id { get; set; }
        public string nameEn { get; set; }
        public string _class { get; set; }
        public long createDate { get; set; }
        public int status { get; set; }
    }

    public class JDIDAPIData
    {
        public string appKey { get; set; }
        public string appSecret { get; set; }
        public string accessToken { get; set; }
        public string no_cust { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string DatabasePathErasoft { get; set; }
        public DateTime? tgl_expired { get; set; }
        public string merchant_code { get; set; }
        public string versi { get; set; }
        public string refreshToken { get; set; }
    }


    public class Data_UpStok
    {
        public int code { get; set; }
        public string message { get; set; }
        public dynamic model { get; set; }
        public bool success { get; set; }
    }

    public class Data_UpPrice
    {
        public int code { get; set; }
        public string message { get; set; }
        public dynamic model { get; set; }
        public bool success { get; set; }
    }


    public class Data_Brand
    {
        public List<Model_Brand> model { get; set; }
        public int code { get; set; }
        public bool success { get; set; }
    }

    public class Model_Brand
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


    public class JDID_RES
    {
        public string openapi_data { get; set; }
        public string error { get; set; }
        public int openapi_code { get; set; }
        public string openapi_msg { get; set; }
    }


    public class DATA_CAT
    {
        public List<Model_Cat> model { get; set; }
        public int code { get; set; }
        public bool success { get; set; }
    }
    public class Model_Cat
    {
        public long id { get; set; }
        public int cateState { get; set; }
        public Model_Cat parentCateVo { get; set; }
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

    public class DATA_CAT_NEW
    {
        public List<Model_Cat_new> model { get; set; }
        public int code { get; set; }
        public bool success { get; set; }
    }
    public class Model_Cat_new
    {
        public long id { get; set; }
        public int level { get; set; }
        public string name { get; set; }
        public string nameEn { get; set; }
        public long parentId { get; set; }
    }


    public class AttrData
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<Model_Attr> model { get; set; }
        public bool success { get; set; }
    }

    public class Model_Attr
    {
        public long propertyId { get; set; }
        public int type { get; set; }
        public string name { get; set; }
        public string nameEn { get; set; }
    }

    public class JDID_ATTRIBUTE_OPT
    {
        public int code { get; set; }
        public string message { get; set; }
        public Model_Opt model { get; set; }
        public bool success { get; set; }
    }

    public class Model_Opt
    {
        public int pageSize { get; set; }
        public int totalCount { get; set; }
        public List<JDOpt> data { get; set; }
    }

    public class JDOpt
    {
        public long attributeValueId { get; set; }
        public string name { get; set; }
        public string nameEn { get; set; }
        public long attributeId { get; set; }
    }

    public class Data_ListProd
    {
        public Model_ListProd model { get; set; }
        public int code { get; set; }
        public bool success { get; set; }
    }

    public class Model_ListProd
    {
        public int totalNum { get; set; }
        public string _class { get; set; }
        public List<Spuinfovolist> spuInfoVoList { get; set; }
        public int pageNum { get; set; }
    }

    public class Spuinfovolist
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
        public int afterSale { get; set; }
        public string spuName { get; set; }
        //public string appDescription { get; set; }
        public int wareStatus { get; set; }
    }
    public class ProductData
    {
        public List<Model_Product> model { get; set; }
        public int code { get; set; }
        public bool success { get; set; }
    }

    public class Model_Product
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
    public class DataProd
    {
        public int code { get; set; }
        public object message { get; set; }
        public List<Model_Product_2> model { get; set; }
        public bool success { get; set; }
    }

    public class Model_Product_2
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

    //add by nurul 29/4/2021, JDID versi 2
    public class JDIDGetListProductResult
    {
        public Jingdong_Seller_Product_Getwareinfolistbyvendorid_Response jingdong_seller_product_getWareInfoListByVendorId_response { get; set; }
    }

    public class Jingdong_Seller_Product_Getwareinfolistbyvendorid_Response
    {
        public string code { get; set; }
        public ReturntypeGetListProduct returnType { get; set; }
    }

    public class ReturntypeGetListProduct
    {
        public int code { get; set; }
        public ModelGetListProduct model { get; set; }
        public bool isSuccess { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
    }

    public class ModelGetListProduct
    {
        public SpuinfovolistGetListProductV2[] spuInfoVoList { get; set; }
        public long totalNum { get; set; }
        public long pageNum { get; set; }
    }

    public class SpuinfovolistGetListProductV2
    {
        public int wareStatus { get; set; }
        public string spuName { get; set; }
        public string productArea { get; set; }
        public string fullCategoryName { get; set; }
        public long brandId { get; set; }
        public string[] imgUris { get; set; }
        public string appDescription { get; set; }
        public string description { get; set; }
        public long spuId { get; set; }
        public long afterSale { get; set; }
        public string fullCategoryId { get; set; }
        public string mainImgUri { get; set; }
    }



    //-----------------------------------------------

    public class JDIDGetProductV2Result
    {
        public Jingdong_Seller_Product_Getskuinfobyspuidandvenderid_Response jingdong_seller_product_getSkuInfoBySpuIdAndVenderId_response { get; set; }
    }

    public class Jingdong_Seller_Product_Getskuinfobyspuidandvenderid_Response
    {
        public string code { get; set; }
        public ReturntypeGetProductV2 returnType { get; set; }
    }

    public class ReturntypeGetProductV2
    {
        public int code { get; set; }
        public ModelGetProductV2[] model { get; set; }
        public bool isSuccess { get; set; }
        public string message { get; set; }
        public bool success { get; set; }
    }

    public class ModelGetProductV2
    {
        public string packLong { get; set; }
        public string saleAttributeIds { get; set; }
        public string weight { get; set; }
        public string upc { get; set; }
        public string sellerSkuId { get; set; }
        public string packWide { get; set; }
        public string skuName { get; set; }
        public dynamic saleAttributeNameMap { get; set; }
        public int piece { get; set; }
        public long spuId { get; set; }
        public float jdPrice { get; set; }
        public string packHeight { get; set; }
        public long skuId { get; set; }
        public string mainImgUri { get; set; }

    }

    public class Saleattributenamemap
    {
        public string key1 { get; set; }
    }

    //-----------------------------------------------
    public class JDIDGetProductDetailV2Result
    {
        public Jingdong_Seller_Product_Sku_Read_Getskubyskuids_Response jingdong_seller_product_sku_read_getSkuBySkuIds_response { get; set; }
    }

    public class Jingdong_Seller_Product_Sku_Read_Getskubyskuids_Response
    {
        public string code { get; set; }
        public ReturntypeGetProductDetail returnType { get; set; }
    }

    public class ReturntypeGetProductDetail
    {
        public int code { get; set; }
        public bool success { get; set; }
        public ModelGetProductDetailV2[] model { get; set; }
        public string message { get; set; }
        public bool isSuccess { get; set; }
    }

    public class ModelGetProductDetailV2
    {
        public string packLong { get; set; }
        public string saleAttributeIds { get; set; }
        public string weight { get; set; }
        public string sellerSkuId { get; set; }
        public string packWide { get; set; }
        public string skuName { get; set; }
        public dynamic saleAttributeNameMap { get; set; }
        public string netWeight { get; set; }
        public int piece { get; set; }
        public long spuId { get; set; }
        public float jdPrice { get; set; }
        public string packHeight { get; set; }
        public long skuId { get; set; }
        public string mainImgUri { get; set; }
    }

    //-------------------------------------------

    public class JDIDgetProductDetailLanjutanV2Result
    {
        public Jingdong_Seller_Product_Getwarebyspuids_Response jingdong_seller_product_getWareBySpuIds_response { get; set; }
    }

    public class Jingdong_Seller_Product_Getwarebyspuids_Response
    {
        public string code { get; set; }
        public Returntype returnType { get; set; }
    }

    public class Returntype
    {
        public int code { get; set; }
        public bool success { get; set; }
        public ModelgetProductDetailLanjutanV2[] model { get; set; }
        public string message { get; set; }
        public bool isSuccess { get; set; }
    }

    public class ModelgetProductDetailLanjutanV2
    {
        public string brandName { get; set; }
        public string spuName { get; set; }
        public string[] keywords { get; set; }
        public long transportId { get; set; }
        public string[] imgUris { get; set; }
        public string appDescription { get; set; }
        public string description { get; set; }
        public long[] skuIds { get; set; }
        public string fullCategoryId { get; set; }
        public int wareStatus { get; set; }
        public long warrantyPeriod { get; set; }
        public string productArea { get; set; }
        public int minQuantity { get; set; }
        public int whetherCod { get; set; }
        public long brandId { get; set; }
        public int auditStatus { get; set; }
        public long modified { get; set; }
        public object crossProductType { get; set; }
        public long spuId { get; set; }
        public long shopId { get; set; }
        public long afterSale { get; set; }
        public string brandLogo { get; set; }
        public string mainImgUri { get; set; }
        public string subtitle { get; set; }
        public string subtitleHref { get; set; }
    }

    //end add by nurul 29/4/2021, JDID versi 2

    //add by nurul 27/5/2021, JDID versi 2 tahap 2
    public class JDIDGetCategoryV2
    {
        public Jingdong_Seller_Category_Api_Read_Getallcategory_Response jingdong_seller_category_api_read_getAllCategory_response { get; set; }
    }

    public class Jingdong_Seller_Category_Api_Read_Getallcategory_Response
    {
        public string code { get; set; }
        public ReturntypeGetCategoryV2 returnType { get; set; }
    }

    public class ReturntypeGetCategoryV2
    {
        public int code { get; set; }
        public bool success { get; set; }
        public ModelGetCategoryV2[] model { get; set; }
        public string message { get; set; }
        public bool isSuccess { get; set; }
    }

    public class ModelGetCategoryV2
    {
        public int level { get; set; }
        public string name { get; set; }
        public long id { get; set; }
        public string nameEn { get; set; }
        public long? parentId { get; set; }
    }

    //---------------------------------------------

    public class JDIDGetCategoryParentV2
    {
        public Jingdong_Seller_Category_Api_Read_Getcategorybycatids_Response jingdong_seller_category_api_read_getCategoryByCatIds_response { get; set; }
    }

    public class Jingdong_Seller_Category_Api_Read_Getcategorybycatids_Response
    {
        public string code { get; set; }
        public ReturntypeGetCategoryParentV2 returnType { get; set; }
    }

    public class ReturntypeGetCategoryParentV2
    {
        public int code { get; set; }
        public bool success { get; set; }
        public ModelGetCategoryV2[] model { get; set; }
        public string message { get; set; }
    }

    public class ModelGetCategoryParentV2
    {
        public int level { get; set; }
        public string name { get; set; }
        public long id { get; set; }
        public string nameEn { get; set; }
        public long? parentId { get; set; }
    }

    //---------------------------------------

    public class JDIDgetAttributeV2
    {
        public Jingdong_Category_Api_Read_Getattributesbycatid_Response jingdong_category_api_read_getAttributesByCatId_response { get; set; }
    }

    public class Jingdong_Category_Api_Read_Getattributesbycatid_Response
    {
        public string code { get; set; }
        public ReturntypegetAttributeV2 returnType { get; set; }
    }

    public class ReturntypegetAttributeV2
    {
        public int code { get; set; }
        public bool success { get; set; }
        public ModelgetAttributeV2[] model { get; set; }
    }

    public class ModelgetAttributeV2
    {
        public string name { get; set; }
        public string nameEn { get; set; }
        public int type { get; set; }
        public long propertyId { get; set; }
    }

    //-----------------------------------------

    public class JDIDgetAttributeOptV2
    {
        public Jingdong_Category_Api_Read_Getattrvaluesbycatidandattrid_Response jingdong_category_api_read_getAttrValuesByCatIdAndAttrId_response { get; set; }
    }

    public class Jingdong_Category_Api_Read_Getattrvaluesbycatidandattrid_Response
    {
        public string code { get; set; }
        public ReturntypegetAttributeOptV2 returnType { get; set; }
    }

    public class ReturntypegetAttributeOptV2
    {
        public int code { get; set; }
        public bool success { get; set; }
        public ModelgetAttributeOptV2 model { get; set; }
    }

    public class ModelgetAttributeOptV2
    {
        public ResultgetAttributeOptV2[] result { get; set; }
        public long pageSize { get; set; }
        public long totalCount { get; set; }
        public long currentPage { get; set; }
    }

    public class ResultgetAttributeOptV2
    {
        public long attributeId { get; set; }
        public long attributeValueId { get; set; }
        public string name { get; set; }
        public long sort { get; set; }
        public string nameEn { get; set; }
    }

    //---------------------------------
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
    //end add by nurul, JDID versi 2 tahap 2 

    #endregion
}
