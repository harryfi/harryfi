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
            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    ErasoftDbContext = new ErasoftContext(accFromUser.UserId);
                }
            }
        }
        public BliBliToken GetToken(string API_client_username, string API_client_password, string API_secret_key, string email_merchant, string password_merchant)
        {
            var ret = new BliBliToken();
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string apiId = API_client_username + ":" + API_client_password;//<-- diambil dari profil API
            string userMTA = email_merchant;//<-- email user merchant
            string passMTA = password_merchant;//<-- pass merchant
            //apiId = "mta-api-sandbox:sandbox-secret-key";
            //string urll = "https://apisandbox.blibli.com/v2/oauth/token?grant_type=client_credentials";
            string urll = "https://api.blibli.com/v2/oauth/token?grant_type=password&password=" + passMTA + "&username=" + userMTA + "";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(apiId))));
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.Accept = "application/json";
            Stream dataStream = myReq.GetRequestStream();
            WebResponse response = myReq.GetResponse();
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            dataStream.Close();
            response.Close();
            // nilai token yg diambil adalah access-token. setelah 24jam biasanya harus masuk ke refresh token. dan harus diambil lagi acces token yg baru
            //cek refreshToken
            ret = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(BliBliToken)) as BliBliToken;
            if (ret.error == null)
            {
                var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.API_CLIENT_P.Equals(API_client_password) && p.API_CLIENT_U.Equals(API_client_username)).SingleOrDefault();
                if (arf01inDB != null)
                {
                    arf01inDB.TOKEN = ret.access_token;
                    arf01inDB.REFRESH_TOKEN = ret.refresh_token;
                    ErasoftDbContext.SaveChanges();

                    BlibliAPIData apidata = new BlibliAPIData {
                        API_client_username = API_client_username,
                        API_client_password = API_client_password,
                        API_secret_key = API_secret_key,
                        merchant_code = arf01inDB.Sort1_Cust,
                        mta_username_email_merchant = email_merchant,
                        mta_password_password_merchant = password_merchant,
                        token = ret.access_token
                    };

                    GetCategoryTree(apidata);
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
        public string GetCategoryTree(BlibliAPIData data)
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
            myReq.Headers.Add("Authorization", ("Bearer " + Convert.ToBase64String(Encoding.UTF8.GetBytes(data.token))));
            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            myReq.Headers.Add("x-blibli-mta-date-milis", (CurrentTimeMillis().ToString()));
            myReq.Headers.Add("username", userMTA);
            myReq.Headers.Add("requestId", milis);
            myReq.Headers.Add("sessionId", milis);
            myReq.ContentType = "application/json";
            myReq.Accept = "application/json";
            Stream dataStream = myReq.GetRequestStream();
            WebResponse response = myReq.GetResponse();
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            dataStream.Close();
            response.Close();
            // nilai token yg diambil adalah access-token. setelah 24jam biasanya harus masuk ke refresh token. dan harus diambil lagi acces token yg baru
            //cek refreshToken
            BlibliGetCategoryReturn result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(BlibliGetCategoryReturn)) as BlibliGetCategoryReturn;
            if (result.errorCode == null)
            {
                //var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.API_CLIENT_P.Equals(API_client_password) && p.API_CLIENT_U.Equals(API_client_username)).SingleOrDefault();
                //if (arf01inDB != null)
                //{
                //    arf01inDB.TOKEN = ret.access_token;
                //    arf01inDB.REFRESH_TOKEN = ret.refresh_token;
                //    ErasoftDbContext.SaveChanges();
                //}
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
        public class BlibliAPIData {
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