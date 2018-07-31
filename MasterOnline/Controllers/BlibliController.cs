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

namespace MasterOnline.Controllers
{
    public class BlibliController : Controller
    {
        AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);//string auth = Base64Encode();

        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        DatabaseSQL EDB;

        public BlibliController()
        {
            MoDbContext = new MoDbContext();
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
            {
                if (sessionData.Account.UserId == "admin_manage")
                    ErasoftDbContext = new ErasoftContext();
                else
                    ErasoftDbContext = new ErasoftContext(sessionData.Account.UserId);

                EDB = new DatabaseSQL(sessionData.Account.UserId);
            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    ErasoftDbContext = new ErasoftContext(accFromUser.UserId);
                    EDB = new DatabaseSQL(accFromUser.UserId);
                }
            }
        }
        public async Task<BliBliToken> GetToken(BlibliAPIData data, bool getCategory)//string API_client_username, string API_client_password, string API_secret_key, string email_merchant, string password_merchant)
        {
            var ret = new BliBliToken();
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant
            //apiId = "mta-api-sandbox:sandbox-secret-key";
            //string urll = "https://apisandbox.blibli.com/v2/oauth/token?grant_type=client_credentials";
            string urll = "https://api.blibli.com/v2/oauth/token?grant_type=password&password=" + passMTA + "&username=" + userMTA + "";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(apiId))));
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.Accept = "application/json";

            //Stream dataStream = myReq.GetRequestStream();
            //WebResponse response = myReq.GetResponse();
            //dataStream = response.GetResponseStream();
            //StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = "";

            using (WebResponse response = await myReq.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseFromServer = reader.ReadToEnd();
                }
            }
            //dataStream.Close();
            //response.Close();
            // nilai token yg diambil adalah access-token. setelah 24jam biasanya harus masuk ke refresh token. dan harus diambil lagi acces token yg baru
            //cek refreshToken
            if (responseFromServer != "")
            {
                ret = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(BliBliToken)) as BliBliToken;
                if (ret.error == null)
                {
                    var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.API_CLIENT_P.Equals(data.API_client_password) && p.API_CLIENT_U.Equals(data.API_client_username)).SingleOrDefault();
                    if (arf01inDB != null)
                    {
                        arf01inDB.TOKEN = ret.access_token;
                        arf01inDB.REFRESH_TOKEN = ret.refresh_token;
                        ErasoftDbContext.SaveChanges();
                        if (getCategory)
                        {
                            data.merchant_code = arf01inDB.Sort1_Cust;
                            data.token = ret.access_token;
                            await GetCategoryTree(data);
                        }
                    }
                }
            }
            return ret;
        }
        public string UploadProduk(BlibliProductData data)
        {
            string ret = "";
            string aksesToken = data.ID_Merchant;// "58b755b8-2acc-45f6-bd46-78021c218645";
            string emailuser = "fierywings5@gmail.com";
            string clientScreet = "123era";
            string myData = "{\"merchantCode\": \"ID MERCHANT\",  \"products\": [   { " +  //MERCHANT ID ADA DI https://merchant.blibli.com/MTA/store-info/store-info
            "\"merchantCode\": \"" + data.ID_Merchant + "\",  " +
            "\"categoryCode\": \"KATEGORI \", " +                                       //LIHAT BAGIAN GETKATEGORI
            "\"productName\": \"" + data.nama + "\", " +                                 // NAMA PRODUK
            "\"url\": \"\", " +                   // LINK URL IKLAN KALO ADA
            "\"merchantSku\": \"" + data.kode + "\",    " +                                   // SKU
            "\"tipePenanganan\": 1,     " +                                             // 1= reguler produk (dikirim oleh blili)| 2= dikirim oleh kurir | 3 =ambil sendiri di toko
            "\"price\": " + data.Price + ", " +                                                      //harga reguler (no diskon)
            "\"salePrice\": " + data.MarketPrice + ",  " +                                                  // harga yg tercantum di display blibli
            "\"stock\": " + data.Qty + ",  " +
            "\"minimumStock\": " + data.MinQty + ", " +
            "\"pickupPointCode\": \"PP-3000179\", " +                                   //pick up poin code, baca GetPickUp
            "\"length\": " + data.Length + ",  " +
            "\"width\": " + data.Width + ", " +
            "\"height\": " + data.Height + ", " +
            "\"weight\": " + data.berat + ", " + // dalam gram, sama seperti MO
            "\"desc\": \"" + data.Keterangan + "\", " +
            "\"uniqueSellingPoint\": \"\", " + //ex : Unique selling point of current product
            "\"productStory\": \"\", " + //ex : This product is launched at 25 Des 2016, made in Indonesia
            "\"upcCode\": \"\",  " + //barcode, ex :1231230010
            "\"display\": " + data.display + ",   " + // true=tampil                
            "\"buyable\": true,  " +
            "\"installation\": false, " +
            "\"features\": [{ \"name\": \"Brand\", \"value\": \"" + data.Brand + "\"}, " +
            "               {\"name\": \"Berat\",\"value\": \"" + Convert.ToString(Convert.ToInt32(data.berat) / 1000).Replace(",", ".") + " Kg\"}, " +
            "               {\"name\" : \"Dimensi Produk\",\"value\" : \"" + data.Length + "cm x " + data.Width + "cm x " + data.Height + "cm\"}], " +
            "\"variasi\": [{\"name\": \"Warna\",\"value\": \"Black\"},{\"name\": \"Warna\",\"value\": \"Red\"},{\"name\": \"Ukuran\",\"value\": \"35\"},{\"name\": \"Ukuran\",\"value\": \"36\"}], " +
            "\"images\": [{\"locationPath\": \"samsung_product-merchant_full01.jpg\",\"sequence\": 0},{\"locationPath\": \"samsung_product-merchant_full02.jpg\",\"sequence\": 1},]}}";

            //cara penulisan nama file untuk gambar produk lihat uploadGambar

            string signature = CreateToken("POST\n" + aksesToken + "\napplication/json \n" + String.Format("{0:ddd MMM d HH:mm:ss WIB yyyy}", DateTime.Now) + "\n//mtaapi/api/businesspartner/v1/product/createProduct ", clientScreet);

            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/createProduct");
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/createProduct");

            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("Bearer " + aksesToken));
            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + emailuser + ":" + signature));
            myReq.Headers.Add("x-blibli-mta-date-milis", (CurrentTimeMillis().ToString()));
            myReq.Headers.Add("username", emailuser);
            myReq.Headers.Add("requestId", aksesToken);
            myReq.Headers.Add("sessionId", aksesToken);
            myReq.ContentType = "application/json";
            myReq.Accept = "application/json";
            Stream dataStream = myReq.GetRequestStream();
            dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Length);
            dataStream.Close();
            WebResponse response = myReq.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }
        public async Task<string> GetCategoryTree(BlibliAPIData data)
        {
            string ret = "";

            string milis = CurrentTimeMillis().ToString();
            DateTime milisBack = Jan1st1970.AddMilliseconds(Convert.ToDouble(milis)).AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant

            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM d HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getCategory", data.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategory?requestId=" + milis + "&businessPartnerCode=" + data.merchant_code;

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("bearer " + data.token));
            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            myReq.Headers.Add("x-blibli-mta-date-milis", (milis));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            myReq.Headers.Add("requestId", milis);
            myReq.Headers.Add("sessionId", milis);
            myReq.Headers.Add("username", userMTA);
            string responseFromServer = "";
            using (WebResponse response = await myReq.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseFromServer = reader.ReadToEnd();
                }
            }

            //Stream dataStream = myReq.GetRequestStream();
            //WebResponse response = myReq.GetResponse();
            //dataStream = response.GetResponseStream();
            //StreamReader reader = new StreamReader(dataStream);
            //string responseFromServer = reader.ReadToEnd();
            //dataStream.Close();
            //response.Close();

            // nilai token yg diambil adalah access-token. setelah 24jam biasanya harus masuk ke refresh token. dan harus diambil lagi acces token yg baru
            //cek refreshToken
            if (responseFromServer != null)
            {
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {
                    if (result.content.Count > 0)
                    {
                        //Data Source = 202.67.14.92; Initial Catalog = ERASOFT_rahmamk; Persist Security Info = True; User ID = sa; Password = admin123 ^
                        //using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                        using (SqlConnection oConnection = new SqlConnection("Data Source=202.67.14.92;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^"))
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
                                oCommand.CommandText = "INSERT INTO [CATEGORY_BLIBLI] ([CATEGORY_CODE], [CATEGORY_NAME], [PARENT_CODE], [IS_LAST_NODE], [MASTER_CATEGORY_CODE]) VALUES (@CATEGORY_CODE, @CATEGORY_NAME, @PARENT_CODE, @IS_LAST_NODE, @MASTER_CATEGORY_CODE)";
                                //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                                oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
                                oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));
                                oCommand.Parameters.Add(new SqlParameter("@PARENT_CODE", SqlDbType.NVarChar, 50));
                                oCommand.Parameters.Add(new SqlParameter("@IS_LAST_NODE", SqlDbType.NVarChar, 1));
                                oCommand.Parameters.Add(new SqlParameter("@MASTER_CATEGORY_CODE", SqlDbType.NVarChar, 50));

                                try
                                {
                                    //oCommand.Parameters[0].Value = data.merchant_code;
                                    foreach (var item in result.content) //foreach parent level top
                                    {
                                        oCommand.Parameters[0].Value = item.categoryCode.Value;
                                        oCommand.Parameters[1].Value = item.categoryName.Value;
                                        oCommand.Parameters[2].Value = "";
                                        oCommand.Parameters[3].Value = item.children == null ? "1" : "0";
                                        oCommand.Parameters[4].Value = "";
                                        if (oCommand.ExecuteNonQuery() == 1)
                                        {
                                            if (item.children != null)
                                            {
                                                RecursiveInsertCategory(oCommand, item.children, item.categoryCode.Value, item.categoryCode.Value, data);
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
                        //await GetAttributeList(data, "AK-1000205", "Aksesoris Audio Lainnya");
                    }
                }
            }

            return ret;
        }
        protected void RecursiveInsertCategory(SqlCommand oCommand, dynamic item_children, string parent, string master_category_code, BlibliAPIData data)
        {
            foreach (var child in item_children)
            {
                oCommand.Parameters[0].Value = child.categoryCode.Value;
                oCommand.Parameters[1].Value = child.categoryName.Value;
                oCommand.Parameters[2].Value = parent;
                oCommand.Parameters[3].Value = child.children == null ? "1" : "0";
                oCommand.Parameters[4].Value = master_category_code;

                if (oCommand.ExecuteNonQuery() == 1)
                {
                    if (child.children != null)
                    {
                        RecursiveInsertCategory(oCommand, child.children, child.categoryCode.Value, master_category_code, data);
                    }
                    else
                    {
                        GetAttributeList(data, child.categoryCode.Value, child.categoryName.Value);
                    }
                }
            }
        }
        public async Task<string> GetAttributeList(BlibliAPIData data, string categoryCode, string categoryName)
        {
            string ret = "";

            string milis = CurrentTimeMillis().ToString();
            DateTime milisBack = Jan1st1970.AddMilliseconds(Convert.ToDouble(milis)).AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant

            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM d HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getCategoryAttributes", data.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + milis + "&businessPartnerCode=" + data.merchant_code + "&categoryCode=" + categoryCode;

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("bearer " + data.token));
            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            myReq.Headers.Add("x-blibli-mta-date-milis", (milis));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            myReq.Headers.Add("requestId", milis);
            myReq.Headers.Add("sessionId", milis);
            myReq.Headers.Add("username", userMTA);
            string responseFromServer = "";
            using (WebResponse response = await myReq.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseFromServer = reader.ReadToEnd();
                }
            }

            //Stream dataStream = myReq.GetRequestStream();
            //WebResponse response = myReq.GetResponse();
            //dataStream = response.GetResponseStream();
            //StreamReader reader = new StreamReader(dataStream);
            //string responseFromServer = reader.ReadToEnd();
            //dataStream.Close();
            //response.Close();

            // nilai token yg diambil adalah access-token. setelah 24jam biasanya harus masuk ke refresh token. dan harus diambil lagi acces token yg baru
            //cek refreshToken
            if (responseFromServer != null)
            {
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {
                    if (result.value.attributes.Count > 0)
                    {
                        using (SqlConnection oConnection = new SqlConnection("Data Source=202.67.14.92;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^"))
                        {
                            oConnection.Open();
                            using (SqlCommand oCommand = oConnection.CreateCommand())
                            {
                                oCommand.CommandType = CommandType.Text;
                                oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
                                oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));
                                string sSQL = "INSERT INTO [ATTRIBUTE_BLIBLI] ([CATEGORY_CODE], [CATEGORY_NAME],";
                                string sSQLValue = ") VALUES (@CATEGORY_CODE, @CATEGORY_NAME,";
                                string a = "";
                                #region Generate Parameters dan CommandText
                                for (int i = 1; i <= 20; i++)
                                {
                                    a = Convert.ToString(i);
                                    sSQL += "[ACODE_" + a + "],[ATYPE_" + a + "],[ANAME_" + a + "],[AOPTIONS_" + a + "],";
                                    sSQLValue += "@ACODE_" + a + ",@ATYPE_" + a + ",@ANAME_" + a + ",@AOPTIONS_" + a + ",";
                                    oCommand.Parameters.Add(new SqlParameter("@ACODE_" + a, SqlDbType.NVarChar, 50));
                                    oCommand.Parameters.Add(new SqlParameter("@ATYPE_" + a, SqlDbType.NVarChar, 50));
                                    oCommand.Parameters.Add(new SqlParameter("@ANAME_" + a, SqlDbType.NVarChar, 250));
                                    oCommand.Parameters.Add(new SqlParameter("@AOPTIONS_" + a, SqlDbType.NVarChar, 1));
                                }
                                sSQL = sSQL.Substring(0, sSQL.Length - 1) + sSQLValue.Substring(0, sSQLValue.Length - 1) + ")";
                                #endregion
                                oCommand.CommandText = sSQL;
                                oCommand.Parameters[0].Value = categoryCode;
                                oCommand.Parameters[1].Value = categoryName;
                                for (int i = 0; i < 20; i++)
                                {
                                    a = Convert.ToString(i * 4 + 2);
                                    oCommand.Parameters[(i * 4) + 2].Value = "";
                                    oCommand.Parameters[(i * 4) + 3].Value = "";
                                    oCommand.Parameters[(i * 4) + 4].Value = "";
                                    oCommand.Parameters[(i * 4) + 5].Value = "";
                                    try
                                    {
                                        oCommand.Parameters[(i * 4) + 2].Value = result.value.attributes[i].attributeCode.Value;
                                        oCommand.Parameters[(i * 4) + 3].Value = result.value.attributes[i].attributeType.Value;
                                        oCommand.Parameters[(i * 4) + 4].Value = result.value.attributes[i].name.Value;
                                        oCommand.Parameters[(i * 4) + 5].Value = result.value.attributes[i].options.Count > 0 ? "1" : "0";
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                oCommand.ExecuteNonQuery();
                            }
                            using (SqlCommand oCommand2 = oConnection.CreateCommand())
                            {
                                oCommand2.CommandType = CommandType.Text;
                                oCommand2.Parameters.Add(new SqlParameter("@ACODE", SqlDbType.NVarChar, 50));
                                oCommand2.Parameters.Add(new SqlParameter("@ATYPE", SqlDbType.NVarChar, 50));
                                oCommand2.Parameters.Add(new SqlParameter("@ANAME", SqlDbType.NVarChar, 250));
                                oCommand2.Parameters.Add(new SqlParameter("@OPTION_VALUE", SqlDbType.NVarChar, 250));
                                oCommand2.CommandText = "INSERT INTO ATTRIBUTE_OPT_BLIBLI (ACODE,ATYPE,ANAME,OPTION_VALUE) VALUES (@ACODE,@ATYPE,@ANAME,@OPTION_VALUE)";
                                string a = "";
                                for (int i = 0; i < 20; i++)
                                {
                                    a = Convert.ToString(i + 1);
                                    try
                                    {
                                        if (result.value.attributes[i].options.Count > 0)
                                        {
                                            for (int j = 0; j < result.value.attributes[i].options.Count; j++)
                                            {
                                                oCommand2.Parameters[0].Value = result.value.attributes[i].attributeCode.Value;
                                                oCommand2.Parameters[1].Value = result.value.attributes[i].attributeType.Value;
                                                oCommand2.Parameters[2].Value = result.value.attributes[i].name.Value;
                                                oCommand2.Parameters[3].Value = result.value.attributes[i].options[j].Value;
                                                oCommand2.ExecuteNonQuery();
                                            }
                                        }

                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }
                            //}
                        }
                    }
                }
            }
            return ret;
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
        public static long CurrentTimeMillis()
        {
            //TimeSpan span = DateTime.Now - Jan1st1970;
            //return (long)span.TotalMilliseconds;
            return (long)DateTime.Now.ToUniversalTime().Subtract(
    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    ).TotalMilliseconds;
        }
        public class BlibliAPIData
        {
            public string merchant_code { get; set; }
            public string API_client_username { get; set; }
            public string API_client_password { get; set; }
            public string API_secret_key { get; set; }
            public string mta_username_email_merchant { get; set; }
            public string mta_password_password_merchant { get; set; }
            public string token { get; set; }
        }
        public class BlibliProductData
        {
            public string ID_Merchant { get; set; }
            public string api_key { get; set; }
            public string kode { get; set; }
            public string nama { get; set; }
            public string display { get; set; }
            public string Length { get; set; }
            public string Width { get; set; }
            public string Height { get; set; }
            public string berat { get; set; }
            public string[] imgUrl { get; set; }
            public string Keterangan { get; set; }
            public string Price { get; set; }
            public string MarketPrice { get; set; }
            public string Qty { get; set; }
            public string MinQty { get; set; }
            public string DeliveryTempNo { get; set; }
            public string Brand { get; set; }
            public string IDMarket { get; set; }
            public string kode_mp { get; set; }

        }
        public class BlibliGetCategoryReturn
        {
            public string requestId { get; set; }
            public string errorMessage { get; set; }
            public string errorCode { get; set; }
            public string success { get; set; }
            public string content { get; set; }
        }
        public class BliBliToken
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
            public string error { get; set; }
            public string error_description { get; set; }
        }
    }
}