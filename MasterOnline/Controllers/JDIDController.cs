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

namespace MasterOnline.Controllers
{
    public class JDIDController : ApiController
    {

        AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        public MoDbContext MoDbContext { get; set; }
        public string ServerUrl = "https://open.jd.id/api";
        public string AccessToken = "4304bd28315728067f7db7e6ff8cc015";
        public string AppKey = "86b082cb8d3436bb340739a90d953ec7";
        public string AppSecret = "1bcda1dca02339e049cb26c5b4c7da12";
        public string Version = "1.0";
        public string Format = "json";
        public string SignMethod = "md5";
        private string Charset_utf8 = "UTF-8";
        public string Method;
        public string ParamJson;
        public string ParamFile;
        protected List<long> listCategory = new List<long>();

        #region jdid tools
        private string getCurrentTimeFormatted()
        {
            var dt = System.DateTime.Now.ToLocalTime();
            return dt.ToString("yyyy-MM-dd HH:mm:ss.fff") + dt.ToString("zzzz").Replace(":", "");
        }

        public string Call()
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
            //sign
            sysParams.Add("sign", this.generateSign(sysParams));

            //send http post request
            var content = this.curl(this.ServerUrl, null, sysParams);
            return content;
        }

        public string Call4BigData()
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
            sysParams.Add("sign", this.generateSign(sysParams));

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

        private string generateSign(Dictionary<string, string> sysDataDictionary)
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
            sb.Insert(0, this.AppSecret);
            sb.Append(this.AppSecret);
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

        [System.Web.Mvc.HttpGet]
        public void getCategory()
        {
            var mgrApiManager = new JDIDController();
            mgrApiManager.Method = "epi.ware.openapi.CategoryApi.getAllCategoryTree";
            mgrApiManager.ParamJson = "";

            var response = mgrApiManager.Call();
            var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
            if (ret.openapi_code == 0)
            {
                var listKategori = JsonConvert.DeserializeObject(ret.openapi_data, typeof(DATA_CAT)) as DATA_CAT;
                if (listKategori != null)
                {
                    try
                    {
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

                                try
                                {
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
                                }
                                catch (Exception ex)
                                {
                                    //oTransaction.Rollback();
                                }
                            }
                        }
                    }
                    catch (Exception ex2)
                    {

                    }
                }
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

            var response = mgrApiManager.Call();
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
                            for(int j = i; j < 20; j++)
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

            var response = mgrApiManager.Call();
            var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RES)) as JDID_RES;
            if (ret != null)
            {
                if (ret.openapi_code == 0)
                {
                    var retOpt = JsonConvert.DeserializeObject(ret.openapi_data, typeof(JDID_ATTRIBUTE_OPT)) as JDID_ATTRIBUTE_OPT;
                    if (retOpt != null)
                    {
                        if(retOpt.model != null)
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
    }
    #region jdid data class
    public class JDIDAPIData
    {
        public string appKey { get; set; }
        public string appSecret { get; set; }
        public string accessToken { get; set; }
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
        public bool sucess { get; set; }
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
        public int propertyId { get; set; }
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
        public int attributeValueId { get; set; }
        public string name { get; set; }
        public string nameEn { get; set; }
        public int attributeId { get; set; }
    }
    #endregion
}
