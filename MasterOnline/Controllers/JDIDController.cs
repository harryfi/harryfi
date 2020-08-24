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
using System.Web.Http;
using System.Threading.Tasks;

namespace MasterOnline.Controllers
{
    public class JDIDController : ApiController
    {

        AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
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

        public ErasoftContext ErasoftDbContext { get; set; }
        DatabaseSQL EDB;
        string username;

        public JDIDController()
        {
            MoDbContext = new MoDbContext("");
            username = "";
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
            {
                if (sessionData.Account.UserId == "admin_manage")
                    ErasoftDbContext = new ErasoftContext();
                else
                    ErasoftDbContext = new ErasoftContext(sessionData.Account.DataSourcePath, sessionData.Account.DatabasePathErasoft);

                EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);
                username = sessionData.Account.Username;
            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    ErasoftDbContext = new ErasoftContext(accFromUser.DataSourcePath, accFromUser.DatabasePathErasoft);
                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                    username = accFromUser.Username;
                }
            }
            if (username.Length > 20)
                username = username.Substring(0, 17) + "...";
        }

        #region jdid tools
        private string getCurrentTimeFormatted()
        {
            var dt = System.DateTime.Now.ToLocalTime();
            return dt.ToString("yyyy-MM-dd HH:mm:ss.fff") + dt.ToString("zzzz").Replace(":", "");
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
                        var listKategori = JsonConvert.DeserializeObject(ret.openapi_data, typeof(DATA_CAT)) as DATA_CAT;
                        if (listKategori != null)
                        {
                            if (listKategori.success)
                            {
                                contextNotif.Clients.Group(data.DatabasePathErasoft).notifTransaction("Akun marketplace " + data.email.ToString() + " (JD.ID) berhasil aktif", true);
                                EDB.ExecuteSQL("CString", CommandType.Text, "Update ARF01 SET STATUS_API = '1' WHERE TOKEN = '" + data.accessToken + "' AND API_KEY = '" + data.appKey + "'");
                                string dbPath = "";
                                var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
                                if (sessionData?.Account != null)
                                {
                                    dbPath = sessionData.Account.DatabasePathErasoft;
                                }
                                else
                                {
                                    if (sessionData?.User != null)
                                    {
                                        var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
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
                                var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
                                if (sessionData?.Account != null)
                                {
                                    dbPath = sessionData.Account.DatabasePathErasoft;
                                }
                                else
                                {
                                    if (sessionData?.User != null)
                                    {
                                        var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
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

        public ATTRIBUTE_JDID getAttribute(JDIDAPIData data, string catId)
        {
            var retAttr = new ATTRIBUTE_JDID();
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

        public List<ATTRIBUTE_OPT_JDID> getAttributeOpt(JDIDAPIData data, string catId, string attrId, int page)
        {
            var mgrApiManager = new JDIDController();
            var listOpt = new List<ATTRIBUTE_OPT_JDID>();
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

        public BindingBase getListProduct(JDIDAPIData data, int page, string cust, int recordCount, int totalData)
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
                                if (listProd.model.spuInfoVoList.Count > 0)
                                {
                                    ret.status = 1;
                                    int IdMarket = ErasoftDbContext.ARF01.Where(c => c.CUST == cust).FirstOrDefault().RecNum.Value;
                                    if (listProd.model.spuInfoVoList.Count == 10)
                                        //ret.message = (page + 1).ToString();
                                        ret.nextPage = 1;

                                    foreach (var item in listProd.model.spuInfoVoList)
                                    {
                                        //product status: 1.online,2.offline,3.punish,4.deleted
                                        if (item.wareStatus == 1 || item.wareStatus == 2)
                                        {
                                            var retProd = GetProduct(data, item, IdMarket, cust);
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

        public BindingBase GetProduct(JDIDAPIData data, Spuinfovolist itemFromList, int IdMarket, string cust)
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
                                            if (retData.exception == 1)
                                                ret.exception = 1;
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
                                            if (retData.exception == 1)
                                                ret.exception = 1;
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

        public BindingBase getProductDetail(JDIDAPIData data, Model_Product item, string kdBrgInduk, bool createParent, string skuId, string cust, int IdMarket, Spuinfovolist itemFromList)
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
                                        var retSQL = CreateSQLValue(item, detailData.model[0], kdBrgInduk, "", cust, IdMarket, 1, itemFromList);
                                        if (retSQL.exception == 1)
                                            ret.exception = 1;
                                        if (retSQL.status == 1)
                                            sSQLVal += retSQL.message;
                                    }

                                    var retSQL2 = CreateSQLValue(item, detailData.model[0], kdBrgInduk, skuId, cust, IdMarket, 2, itemFromList);
                                    if (retSQL2.exception == 1)
                                        ret.exception = 1;
                                    if (retSQL2.status == 1)
                                        sSQLVal += retSQL2.message;
                                }
                                else
                                {
                                    var retSQL = CreateSQLValue(item, detailData.model[0], "", skuId, cust, IdMarket, 0, itemFromList);
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

        protected BindingBase CreateSQLValue(Model_Product item, Model_Detail_Product detItem, string kdBrgInduk, string skuId, string cust, int IdMarket, int typeBrg, Spuinfovolist itemFromList)
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
                string brand = itemFromList.brandId.ToString();
                //var display = statusBrg.Equals("active") ? 1 : 0;
                string deskripsi = itemFromList.description;

                if (typeBrg != 1)
                {
                    //change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                    if (!string.IsNullOrEmpty(detItem.sellerSkuId.ToString()))
                    {
                        sSQL_Value += " ( '" + skuId + "' , '" + detItem.sellerSkuId.ToString() + "' , '";
                    }
                    else
                    {
                        sSQL_Value += " ( '" + skuId + "' , '" + skuId + "' , '";
                    }
                    //sSQL_Value += " ( '" + skuId + "' , '' , '";
                    //end change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                }
                else
                {
                    namaBrg = itemFromList.spuName;
                    //change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                    //sSQL_Value += " ( '" + kdBrgInduk + "' , '" + kdBrgInduk + "' , '";
                    sSQL_Value += " ( '" + kdBrgInduk + "' , '' , '";
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
                    //string[] ssplitNama = namaBrg.Substring(0, 30).Split(' ');
                    //var jumlahLength = ssplitNama.Length - 2;
                    //if(ssplitNama.Length >= 2)
                    //{
                    //    nama = ssplitNama[0] + " " + ssplitNama[1] + " " + ssplitNama[2];
                    //    nama2 = namaBrg.in(ssplitNama[3], );
                    //}
                    //else
                    //{
                    //    nama = ssplitNama[0] + " " + ssplitNama[1];
                    //}
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
                sSQL_Value += brand + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "' , '" + urlImage4 + "' , '" + urlImage5 + "' , '" + (typeBrg == 2 ? kdBrgInduk : "") + "' , '" + (typeBrg == 1 ? "4" : "3") + "'";
                int i;
                if (!string.IsNullOrEmpty(detItem.saleAttributeIds) && detItem.saleAttributeIds != "null")
                {
                    attrVal = detItem.saleAttributeIds.Split(';');
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
                }
                else
                {
                    sSQL_Value += ",'','',''";
                    for (int j = 2; j < 20; j++)
                    {
                        sSQL_Value += ",'','',''";
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

                    EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);
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
    }

    #region jdid data class

    public class Model_Promo
    {
        public bool success { get; set; }
        public int code { get; set; }
        public string description { get; set; }
        public string order_id { get; set; }
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

    public class JDIDAPIData
    {
        public string appKey { get; set; }
        public string appSecret { get; set; }
        public string accessToken { get; set; }
        public string no_cust { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string DatabasePathErasoft { get; set; }
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
        //public int afterSale { get; set; }
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
    #endregion
}
