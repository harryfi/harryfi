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
        public BliBliToken GetTokenSandbox(BlibliAPIData data)
        {
            var ret = new BliBliToken();
            string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string urll = "https://apisandbox.blibli.com/v2/oauth/token";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(apiId))));
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.Accept = "application/json";
            string myData = "grant_type=client_credentials";// "&password=" + passMTA + "&username=" + userMTA + "";
            //Stream dataStream = myReq.GetRequestStream();
            //WebResponse response = myReq.GetResponse();
            //dataStream = response.GetResponseStream();
            //StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = "";
            try
            {
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
            }
            catch (Exception ex)
            {

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

                    }
                }
            }
            return ret;
        }
        public BliBliToken GetToken(BlibliAPIData data, bool syncData)//string API_client_username, string API_client_password, string API_secret_key, string email_merchant, string password_merchant)
        {
            var ret = new BliBliToken();
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant
            //apiId = "mta-api-sandbox:sandbox-secret-key";
            //string urll = "https://apisandbox.blibli.com/v2/oauth/token?grant_type=client_credentials";
            string urll = "https://api.blibli.com/v2/oauth/token";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(apiId))));
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.Accept = "application/json";
            string myData = "grant_type=password&password=" + passMTA + "&username=" + userMTA + "";
            //Stream dataStream = myReq.GetRequestStream();
            //WebResponse response = myReq.GetResponse();
            //dataStream = response.GetResponseStream();
            //StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = "";
            try
            {
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
            }
            catch (Exception)
            {

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
                        if (syncData)
                        {
                            data.merchant_code = arf01inDB.Sort1_Cust;
                            data.token = ret.access_token;
                            GetPickupPoint(data); // untuk prompt pickup point saat insert barang
                            SetCategoryCode(data); // untuk category code yg muncul saat insert barang
                        }
                    }
                }
            }
            return ret;
        }
        public string GetPickupPoint(BlibliAPIData data)
        {
            string ret = "";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);// Jan1st1970.AddMilliseconds(Convert.ToDouble(milis)).AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant

            //string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/getPickupPoint", data.API_secret_key);
            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getPickupPoint", data.API_secret_key);

            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getPickupPoint?businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code);


            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("bearer " + data.token));
            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            myReq.Headers.Add("requestId", milis.ToString());
            myReq.Headers.Add("sessionId", milis.ToString());
            myReq.Headers.Add("username", userMTA);
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
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {
                    if (result.content.Count > 0)
                    {
                        using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                        {
                            oConnection.Open();
                            //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                            //{
                            using (SqlCommand oCommand = oConnection.CreateCommand())
                            {
                                oCommand.CommandType = CommandType.Text;
                                oCommand.CommandText = "DELETE FROM [PICKUP_POINT_BLIBLI] WHERE [MERCHANT_CODE]='" + data.merchant_code + "'";
                                oCommand.ExecuteNonQuery();

                                //oCommand.Transaction = oTransaction;
                                oCommand.CommandText = "INSERT INTO [PICKUP_POINT_BLIBLI] ([KODE], [KETERANGAN], [MERCHANT_CODE]) VALUES (@KODE, @KETERANGAN, @MERCHANT_CODE)";
                                oCommand.Parameters.Add(new SqlParameter("@KODE", SqlDbType.NVarChar, 30));
                                oCommand.Parameters.Add(new SqlParameter("@KETERANGAN", SqlDbType.NVarChar, 250));
                                oCommand.Parameters.Add(new SqlParameter("@MERCHANT_CODE", SqlDbType.NVarChar, 30));

                                try
                                {
                                    oCommand.Parameters[2].Value = data.merchant_code;
                                    foreach (var item in result.content)
                                    {
                                        oCommand.Parameters[0].Value = item.code.Value;
                                        oCommand.Parameters[1].Value = item.name.Value;
                                        if (oCommand.ExecuteNonQuery() == 1)
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
                            //}
                        }
                    }
                }
            }

            return ret;
        }
        public string GetOrderList(BlibliAPIData iden)
        {
            //if merchant code diisi. barulah GetOrderList
            string ret = "";
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
                                                                 //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/order/orderList", iden.API_secret_key);
            //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/createProduct";
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderList?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("bearer " + iden.token));
            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            myReq.Headers.Add("requestId", milis.ToString());
            myReq.Headers.Add("sessionId", milis.ToString());
            myReq.Headers.Add("username", userMTA);
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

            }
                return ret;
        }
        public string UploadProduk(BlibliAPIData iden, BlibliProductData data)
        {
            //if merchant code diisi. barulah upload produk
            string ret = "";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            string features = "";
            string variasi = "";
            string gambar = "";

            string sSQL = "SELECT * FROM (";
            for (int i = 1; i <= 30; i++)
            {
                sSQL += "SELECT A.ACODE_" + i.ToString() + " AS CATEGORY_CODE,A.ANAME_" + i.ToString() + " AS CATEGORY_NAME,B.ATYPE_" + i.ToString() + " AS CATEGORY_TYPE,A.AVALUE_" + i.ToString() + " AS VALUE FROM STF02H A INNER JOIN MO.DBO.ATTRIBUTE_BLIBLI B ON A.CATEGORY_CODE = B.CATEGORY_CODE WHERE A.BRG='" + data.kode + "' " + System.Environment.NewLine;
                if (i < 30)
                {
                    sSQL += "UNION ALL " + System.Environment.NewLine;
                }
            }

            DataSet dsFeature = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE <> 'DEFINING_ATTRIBUTE' ");
            DataSet dsVariasi = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE = 'DEFINING_ATTRIBUTE' ");
            features += "{ \"name\": \"Brand\", \"value\": \"" + data.Brand + "\"}, ";
            for (int i = 0; i < dsFeature.Tables[0].Rows.Count; i++)
            {
                features += "{ \"name\": \"" + Convert.ToString(dsFeature.Tables[0].Rows[i]["CATEGORY_NAME"]) + "\", \"value\": \"" + Convert.ToString(dsFeature.Tables[0].Rows[i]["VALUE"]) + "\"},";
            }
            for (int i = 0; i < dsVariasi.Tables[0].Rows.Count; i++)
            {
                string[] values = Convert.ToString(dsVariasi.Tables[0].Rows[i]["VALUE"]).Split(';');
                for (int a = 0; a < values.Length; a++)
                {
                    variasi += "{\"name\": \"" + Convert.ToString(dsVariasi.Tables[0].Rows[i]["CATEGORY_NAME"]) + "\",\"value\": \"" + values[a] + "\"},";
                }
            }

            string myData = "{";
            myData += "\"merchantCode\": \"" + iden.merchant_code + "\", ";
            myData += "\"products\": ";
            myData += "[{ ";  //MERCHANT ID ADA DI https://merchant.blibli.com/MTA/store-info/store-info
            {
                myData += "\"merchantCode\": \"" + iden.merchant_code + "\",  ";
                myData += "\"categoryCode\": \"" + data.CategoryCode + " \", ";                                       //LIHAT BAGIAN GETKATEGORI
                myData += "\"productName\": \"" + data.nama + "\", ";                                 // NAMA PRODUK
                myData += "\"url\": \"\", ";                   // LINK URL IKLAN KALO ADA
                myData += "\"merchantSku\": \"" + data.kode + "\", ";                                // SKU
                myData += "\"tipePenanganan\": 1, ";                                            // 1= reguler produk (dikirim oleh blili)| 2= dikirim oleh kurir | 3 =ambil sendiri di toko
                myData += "\"price\": " + data.Price + ", ";                                                      //harga reguler (no diskon)
                myData += "\"salePrice\": " + data.MarketPrice + ", ";                                                // harga yg tercantum di display blibli
                myData += "\"stock\": " + data.Qty + ", ";
                myData += "\"minimumStock\": " + data.MinQty + ", ";
                myData += "\"pickupPointCode\": \"" + data.PickupPoint + "\", ";                                   //pick up poin code, baca GetPickUp
                myData += "\"length\": " + data.Length + ", ";
                myData += "\"width\": " + data.Width + ", ";
                myData += "\"height\": " + data.Height + ", ";
                myData += "\"weight\": " + data.berat + ", "; // dalam gram, sama seperti MO
                myData += "\"desc\": \"" + data.Keterangan + "\", ";
                myData += "\"uniqueSellingPoint\": \"" + data.Keterangan + "\", "; //ex : Unique selling point of current product
                myData += "\"productStory\": \"" + data.Keterangan + "\", "; //ex : This product is launched at 25 Des 2016, made in Indonesia
                myData += "\"upcCode\": \"\", "; //barcode, ex :1231230010
                myData += "\"display\": " + data.display + ", "; // true=tampil                
                myData += "\"buyable\": true, ";
                myData += "\"features\": [";
                //for (int i = 0; i < length; i++)
                //{
                //    features += "{ \"name\": \"Brand\", \"value\": \"" + data.Brand + "\"}, ";
                //}
                features = features.Substring(0, features.Length - 1);
                myData += features + "], ";
                myData += "\"variasi\": [";
                //for (int i = 0; i < length; i++)
                //{
                //    variasi += "{\"name\": \"Warna\",\"value\": \"Black\"},";
                //}
                variasi = variasi.Substring(0, variasi.Length - 1);
                myData += variasi + "], ";
                myData += "\"images\": [";
                for (int i = 0; i < 3; i++)
                {
                    gambar += "{\"locationPath\": \"" + data.Brand + "_" + data.nama + "_full0" + Convert.ToString(i + 1) + ".jpg\",\"sequence\": " + Convert.ToString(i) + "},";
                }
                gambar = gambar.Substring(0, gambar.Length - 1);
                myData += gambar + "]";
            }
            myData += "}]";
            myData += "}";

            //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
            string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
            //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/createProduct";
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/createProduct";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("bearer " + iden.token));
            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            myReq.Headers.Add("requestId", milis.ToString());
            myReq.Headers.Add("sessionId", milis.ToString());
            myReq.Headers.Add("username", userMTA);
            string responseFromServer = "";
            try
            {
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
            }
            catch (Exception ex)
            {

            }
            if (responseFromServer != null)
            {
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {
                    //INSERT QUEUE FEED
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
                            oCommand.CommandText = "INSERT INTO [QUEUE_FEED_BLIBLI] ([REQUESTID],[REQUEST_ACTION],[MERCHANT_CODE]) VALUES (@REQUESTID,'createProduct',@MERCHANTCODE";
                            //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@REQUESTID", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@MERCHANTCODE", SqlDbType.NVarChar, 50));

                            try
                            {
                                oCommand.Parameters[0].Value = result.requestId;
                                oCommand.Parameters[1].Value = iden.merchant_code;

                                if (oCommand.ExecuteNonQuery() == 1)
                                {
                                    BlibliQueueFeedData queueData = new BlibliQueueFeedData
                                    {
                                        request_id = result.requestId,
                                        update_BRG_MP = true
                                    };
                                    GetQueueFeedDetail(iden, queueData);
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
                }
            }

            return ret;
        }
        public string UpdateProdukQOH_Display(BlibliAPIData iden, BlibliProductData data)
        {
            //if merchant code diisi. barulah upload produk
            string ret = "";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            #region Get Product List ( untuk dapatkan QOH di Blibi )
            int QOHBlibli = 0;
            //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
            string signature_1 = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getProductSummary", iden.API_secret_key);
            string urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getProductSummary?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&gdnSku=" + Uri.EscapeDataString(data.kode_mp);
            
            HttpWebRequest myReq_1 = (HttpWebRequest)WebRequest.Create(urll_1);
            myReq_1.Method = "POST";
            myReq_1.Headers.Add("Authorization", ("bearer " + iden.token));
            myReq_1.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature_1));
            myReq_1.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            myReq_1.Accept = "application/json";
            myReq_1.ContentType = "application/json";
            myReq_1.Headers.Add("requestId", milis.ToString());
            myReq_1.Headers.Add("sessionId", milis.ToString());
            myReq_1.Headers.Add("username", userMTA);
            string responseFromServer_1 = "";
            try
            {
                using (WebResponse response = myReq_1.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer_1 = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            if (responseFromServer_1 != null)
            {
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer_1);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {
                    if (result.content.Count > 0)
                    {
                        foreach (var item in result.content)
                        {
                            QOHBlibli = item.stockAvailableLv2.Value;
                        }
                    }
                }
            }
            #endregion

            if (Convert.ToInt32(data.Qty) - QOHBlibli != 0) // tidak beda
            {
                QOHBlibli = Convert.ToInt32(data.Qty) - QOHBlibli;
            }

            string myData = "{";
            myData += "\"merchantCode\": \"" + iden.merchant_code + "\", ";
            myData += "\"productRequests\": ";
            myData += "[{ ";  //MERCHANT ID ADA DI https://merchant.blibli.com/MTA/store-info/store-info
            {
                myData += "\"gdnSku\": \"" + data.kode_mp + "\",  ";
                myData += "\"stock\": " + Convert.ToString(QOHBlibli) + ", ";
                myData += "\"minimumStock\": " + data.MinQty + ", ";
                myData += "\"price\": " + data.Price + ", ";
                myData += "\"salePrice\": " + data.MarketPrice + ", ";// harga yg tercantum di display blibli
                myData += "\"buyable\": " + data.display + ", ";
                myData += "\"display\": " + data.display + " "; // true=tampil                
            }
            myData += "}]";
            myData += "}";

            //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
            string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/updateProduct", iden.API_secret_key);
            //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/createProduct";
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/updateProduct";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("bearer " + iden.token));
            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            myReq.Headers.Add("requestId", milis.ToString());
            myReq.Headers.Add("sessionId", milis.ToString());
            myReq.Headers.Add("username", userMTA);
            string responseFromServer = "";
            try
            {
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
            }
            catch (Exception ex)
            {

            }
            if (responseFromServer != null)
            {
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {

                }
            }

            return ret;
        }
        public string SetCategoryCode(BlibliAPIData data)
        {
            //HASIL MEETING : SIMPAN CATEGORY DAN ATTRIBUTE NYA KE DATABASE MO
            //INSERT JIKA CATEGORY_CODE UTAMA BELUM ADA DI MO

            string ret = "";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant

            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getCategory", data.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategory?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code);


            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("bearer " + data.token));
            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            myReq.Headers.Add("requestId", milis.ToString());
            myReq.Headers.Add("sessionId", milis.ToString());
            myReq.Headers.Add("username", userMTA);
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
                                oCommand.CommandText = "UPDATE [ARF01] SET KODE=@KODE WHERE SORT1_CUST='" + data.merchant_code + "' ";
                                //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                                oCommand.Parameters.Add(new SqlParameter("@KODE", SqlDbType.NVarChar, 250));

                                try
                                {
                                    string kode = "";
                                    //oCommand.Parameters[0].Value = data.merchant_code;
                                    foreach (var item in result.content) //foreach parent level top
                                    {
                                        kode = kode + item.categoryCode.Value + ";";
                                    }
                                    kode = kode.Substring(0, kode.Length - 1);
                                    oCommand.Parameters[0].Value = kode;
                                    if (oCommand.ExecuteNonQuery() == 1)
                                    {
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
                    }
                }
            }

            return ret;
        }

        public string GetQueueFeedDetail(BlibliAPIData data, BlibliQueueFeedData feed)
        {
            string ret = "";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant

            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/feed/detail", data.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/feed/detail?requestId=" + Uri.EscapeDataString(feed.request_id) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("bearer " + data.token));
            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            myReq.Headers.Add("requestId", milis.ToString());
            myReq.Headers.Add("sessionId", milis.ToString());
            myReq.Headers.Add("username", userMTA);
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
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {
                    if (result.value.queueHistory.Count > 0)
                    {
                        using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                        {
                            oConnection.Open();
                            //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                            //{
                            using (SqlCommand oCommand = oConnection.CreateCommand())
                            {

                                foreach (var item in result.value.queueHistory)
                                {
                                    if (Convert.ToBoolean(item.isSuccess))
                                    {
                                        dynamic values = Newtonsoft.Json.JsonConvert.DeserializeObject(item.value.Value);

                                        if (Convert.ToString(values.type) == "createProduct" && feed.update_BRG_MP)
                                        {
                                            oCommand.CommandType = CommandType.Text;
                                            oCommand.CommandText = "UPDATE H SET BRG_MP=@BRG_MP FROM STF02H H INNER JOIN ARF01 A ON H.IDMARKET = A.RECNUM WHERE H.BRG=@MERCHANTSKU AND A.SORT1_CUST=@MERCHANTCODE";
                                            oCommand.Parameters.Add(new SqlParameter("@BRG_MP", SqlDbType.NVarChar, 50));
                                            oCommand.Parameters.Add(new SqlParameter("@MERCHANTSKU", SqlDbType.NVarChar, 20));
                                            oCommand.Parameters.Add(new SqlParameter("@MERCHANTCODE", SqlDbType.NVarChar, 10));

                                            try
                                            {
                                                oCommand.Parameters[0].Value = Convert.ToString(values.gdnSku);
                                                oCommand.Parameters[1].Value = Convert.ToString(values.merchantSku);
                                                oCommand.Parameters[2].Value = Convert.ToString(data.merchant_code);
                                                if (oCommand.ExecuteNonQuery() == 1)
                                                {
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
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
        public async Task<string> GetCategoryTree(BlibliAPIData data)
        {
            //HASIL MEETING : SIMPAN CATEGORY DAN ATTRIBUTE NYA KE DATABASE MO
            //INSERT JIKA CATEGORY_CODE UTAMA BELUM ADA DI MO

            string ret = "";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant

            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getCategory", data.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategory?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code);


            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("bearer " + data.token));
            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            myReq.Headers.Add("requestId", milis.ToString());
            myReq.Headers.Add("sessionId", milis.ToString());
            myReq.Headers.Add("username", userMTA);
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
                        await GetAttributeList(data);
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
                    //else
                    //{
                    //    GetAttributeList(data, child.categoryCode.Value, child.categoryName.Value);
                    //}
                }
            }
        }
        public async Task<string> GetAttributeList(BlibliAPIData data)
        {
            var category = MoDbContext.CategoryBlibli.Where(p => p.IS_LAST_NODE.Equals("1")).ToList();
            string ret = "";
            foreach (var item in category)
            {
                string categoryCode = item.CATEGORY_CODE;
                string categoryName = item.CATEGORY_NAME;
                //    string categoryCode = "3 -1000001";
                //string categoryName = "3 Kamar +";

                long milis = CurrentTimeMillis();
                DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

                string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
                string userMTA = data.mta_username_email_merchant;//<-- email user merchant
                string passMTA = data.mta_password_password_merchant;//<-- pass merchant

                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getCategoryAttributes", data.API_secret_key);
                string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&categoryCode=" + Uri.EscapeDataString(categoryCode);
                //string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/getCategoryAttributes", data.API_secret_key);
                //    string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + milis + "&businessPartnerCode=" + data.merchant_code + "&categoryCode=" + categoryCode;

                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + data.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
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
                                    for (int i = 1; i <= 30; i++)
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
                                    for (int i = 0; i < 30; i++)
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
                                    var AttributeOptInDb = MoDbContext.AttributeOptBlibli.ToList();
                                    for (int i = 0; i < 30; i++)
                                    {
                                        a = Convert.ToString(i + 1);
                                        try
                                        {
                                            if (result.value.attributes[i].options.Count > 0)
                                            {
                                                string ACODE = "";
                                                string ATYPE = "";
                                                string ANAME = "";
                                                string OPTION_VALUE = "";
                                                for (int j = 0; j < result.value.attributes[i].options.Count; j++)
                                                {
                                                    ACODE = result.value.attributes[i].attributeCode.Value;
                                                    ATYPE = result.value.attributes[i].attributeType.Value;
                                                    ANAME = result.value.attributes[i].name.Value;
                                                    OPTION_VALUE = result.value.attributes[i].options[j].Value;

                                                    //cek jika sudah ada di database
                                                    var cari = AttributeOptInDb.Where(p => p.ACODE.ToUpper().Equals(ACODE.ToUpper())
                                                    && p.ATYPE.ToUpper().Equals(ATYPE.ToUpper())
                                                    && p.ANAME.ToUpper().Equals(ANAME.ToUpper())
                                                    && p.OPTION_VALUE.ToUpper().Equals(OPTION_VALUE.ToUpper())
                                                    ).ToList();
                                                    //cek jika sudah ada di database

                                                    if (cari.Count == 0)
                                                    {
                                                        oCommand2.Parameters[0].Value = ACODE;
                                                        oCommand2.Parameters[1].Value = ATYPE;
                                                        oCommand2.Parameters[2].Value = ANAME;
                                                        oCommand2.Parameters[3].Value = OPTION_VALUE;
                                                        oCommand2.ExecuteNonQuery();

                                                        AttributeOptInDb = MoDbContext.AttributeOptBlibli.ToList();
                                                    }

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
            //        return (long)DateTime.Now.ToUniversalTime().Subtract(
            //new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            //).TotalMilliseconds;
            return (long)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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
        public class BlibliQueueFeedData
        {
            public string request_id { get; set; }
            public bool update_BRG_MP { get; set; }

        }
        public class BlibliProductData
        {
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
            public string CategoryCode { get; set; }
            public string[] attribute { get; set; }
            public string feature { get; set; }
            public string PickupPoint { get; set; }


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
        protected string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

    }
}