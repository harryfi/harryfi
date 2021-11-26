using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Threading.Tasks;
using MasterOnline.Models;
using System.Data;
using System.Data.SqlClient;
using MasterOnline.ViewModels;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Erasoft.Function;
using Hangfire;
using Hangfire.SqlServer;

namespace MasterOnline.Controllers
{
    public class PartnerApiControllerJob : Controller
    {
        private MoDbContext MoDbContext { get; set; }
        private ErasoftContext ErasoftDbContext { get; set; }
        private DatabaseSQL EDB;
        private string username;
        string dbPathEra = "";
        string DataSourcePath = "";
        string dbSourceEra = "";

        // GET: PartnerApiControllerJob
        public ActionResult Index()
        {
            return View();
        }

        protected void SetupContext(string dbPathEra, string dbSourceEra)
        {
            //string ret = "";
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(dbPathEra);
            //string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(dbSourceEra, dbPathEra);
            //username = user_name;
            //return ret;
        }

        public async Task<string> TadaAuthorization(PartnerApiData data)
        {
            string token = "";
            string url = "https://api.gift.id/v1/pos/token";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);

            var encodedData = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data.ClientId + ":" + data.ClientSecret));

            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", "Basic " + encodedData);
            myReq.Accept = "*/*";
            myReq.ContentType = "application/json";
            myReq.ContentLength = 0;
            string myData = "{\"username\":\"" + data.Username + "\",\"password\":\"" + data.Password + "\",\"grant_type\":\"password\",\"scope\":\"offline_access\"}";

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

                var response_token = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokenClass)) as TokenClass;
                //var date_expires = DateTime.UtcNow.AddHours(7).AddDays((response_token.expires_in) / 86400).ToString("yyyy-MM-dd HH:mm:ss");// +1
                var date_expires = response_token.expiredAt.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                ErasoftDbContext.Database.ExecuteSqlCommand("UPDATE PARTNER_API SET Access_Token = '" + response_token.access_token + "', Session_ExpiredDate = '" + date_expires + "' WHERE PartnerId = 30007 ");
                token = response_token.access_token;
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
            }
            return token;
        }

        [AutomaticRetry(Attempts = 0)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("{obj}")]
        public async Task<string> TADATopupByPhoneJob(string DatabasePathErasoft, string msgError, string cust, string logActionCategory, string logActionName, string dbSourceEra)
        {
            string ret = "";
            SetupContext(DatabasePathErasoft, dbSourceEra);

            var partnerDb = ErasoftDbContext.PARTNER_API.SingleOrDefault(p => p.PartnerId == 30007);

            string idProgram = partnerDb.ProgramId;
            string idWallet = partnerDb.WalletId;
            string token = partnerDb.Access_Token;
            string fs_id = partnerDb.fs_id.ToString();

            PartnerApiData data = new PartnerApiData()
            {
                ClientId = partnerDb.ClientId,
                ClientSecret = partnerDb.ClientSecret,
                Username = partnerDb.Username,
                Password = partnerDb.Password
            };

            if (DateTime.UtcNow.AddHours(7) >= partnerDb.Session_ExpiredDate || partnerDb.Session_ExpiredDate == null)
            {
                token = await TadaAuthorization(data);
            }

            try
            {
                string dateFrom = DateTime.UtcNow.AddDays(-1).AddHours(7).ToString("yyyy-MM-dd");
                string dateTo = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

                string sSQLHeader = "SELECT ISNULL(SOA.NO_BUKTI,'') [NOBUK], ISNULL(ARC.TLP,'') [PHONE], " +
                        "ISNULL(SOA.NO_REFERENSI,'') [BILLNUMBER], ISNULL(SOA.BRUTO,'') [AMOUNT] " +
                        "FROM SOT01A SOA " +
                        "JOIN SIT01A SIA ON SOA.NO_BUKTI = SIA.NO_SO " +
                        "JOIN ARF01C ARC ON ARC.BUYER_CODE = SOA.PEMESAN " +
                        "WHERE SOA.STATUS_TRANSAKSI = '04'" +
                        //"AND SIA.TGL_KIRIM BETWEEN '" + dateFrom + "' and '" + dateTo + "'"; //04 //01 //TGL //TGL_KIRIM
                        "AND CONVERT(VARCHAR(25), SIA.TGL_KIRIM, 126) LIKE '" + dateFrom + "%'"; // SIA.TGL_KIRIM = '" + dateFrom + "'

                var sot01a = ErasoftDbContext.Database.SqlQuery<Order>(sSQLHeader).ToList();
                if (sot01a.Count == 0)
                {
                    ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', 'SOT01A_NULL', 'Data Pesanan dari tanggal " + dateFrom + " sampai tanggal " + dateTo + ", kosong.', dateadd(hour, 7, getdate()), null, 1) ");
                    //throw new Exception("Data Pesanan dari tanggal " + dateFrom + " sampai tanggal " + dateTo + ", kosong.");
                    return ret;
                }
                string listNobuk = "";
                foreach (var pesanan in sot01a)
                {
                    listNobuk += "'" + pesanan.NOBUK + "' , ";
                }
                listNobuk = listNobuk.Substring(0, listNobuk.Length - 2);

                string sSQLDetail = "SELECT ISNULL(SOB.NO_BUKTI,'') [NOBUK], ISNULL(SOB.BRG,'') [SKU], " +
                        "REPLACE(REPLACE(REPLACE(CONVERT(NVARCHAR(MAX),STF.NAMA + ' ' + ISNULL(STF.NAMA2, '')), CHAR(9), ' '), CHAR(10), ' ') , CHAR(13), ' ') [ITEMNAME], " +
                        "SOB.QTY [QUANTITY], SOB.H_SATUAN [PRICE] " +
                        "FROM SOT01B SOB JOIN STF02 STF ON STF.BRG = SOB.BRG " +
                        "WHERE SOB.NO_BUKTI IN (" + listNobuk + ")";

                var sot01b = ErasoftDbContext.Database.SqlQuery<OrderItem>(sSQLDetail).ToList();
                if (sot01b.Count == 0)
                {
                    ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', 'SOT01B_NULL', 'Data Barang kosong.', dateadd(hour, 7, getdate()), null, 1) ");
                    //throw new Exception("Data Barang kosong.");
                    return ret;
                }

                foreach (var a in sot01a)
                {
                    var dataheader = new TopupByPhone();

                    string phonenumber = "";
                    string initialnumber = a.PHONE.Substring(0, 2);
                    if (a.PHONE.Substring(0, 4) == "6208")
                    {
                        phonenumber = "+" + a.PHONE.Remove(2, 1);
                    }
                    else if (initialnumber == "628")
                    {
                        phonenumber = "+" + a.PHONE;
                    }
                    else if (initialnumber == "08")
                    {
                        phonenumber = initialnumber.Replace("08", "+628") + a.PHONE.Remove(0, 2);
                    }
                    dataheader.phone = phonenumber;

                    dataheader.amount = a.AMOUNT;
                    //if (!string.IsNullOrEmpty(a.BILLNUMBER))
                    //    dataheader.billNumber = a.BILLNUMBER;
                    //else
                    dataheader.billNumber = a.NOBUK;
                    dataheader.programId = idProgram;
                    dataheader.walletId = idWallet;
                    dataheader.paymentMethod = "cash";
                    dataheader.items = new List<Item>();

                    var newsot01b = sot01b.Where(x => x.NOBUK == a.NOBUK).ToList();
                    foreach (var b in newsot01b)
                    {
                        var datadetail = new Item();
                        datadetail.sku = b.SKU;
                        datadetail.itemName = b.ITEMNAME;
                        datadetail.quantity = b.QUANTITY;
                        datadetail.price = b.PRICE;
                        dataheader.items.Add(datadetail);
                    }

                    string url = "https://api.gift.id/v1/pos/phone/topup";

                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);

                    myReq.Method = "POST";
                    myReq.Headers.Add("Authorization", "Bearer " + token);
                    myReq.Accept = "*/*";
                    myReq.ContentType = "application/json";
                    myReq.ContentLength = 0;
                    string myData = Newtonsoft.Json.JsonConvert.SerializeObject(dataheader);

                    string responseFromServer = "";
                    string idTrx = "";

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

                        if (responseFromServer.Contains("error"))
                        {
                            var response_error = JsonConvert.DeserializeObject(responseFromServer, typeof(ErrorTada)) as ErrorTada;
                            string errorMessage = response_error.error[0].ToString();
                            string messageError = response_error.message[0].ToString();
                            string msg_response = errorMessage.Replace("\n", "").Replace("\r", "").Replace("\"", "").Replace("[", "").Replace("]", "").Replace("\\", "").TrimStart();

                            ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', '" + a.NOBUK + "', '" + errorMessage + ".', dateadd(hour, 7, getdate()), null, 0) ");
                            continue;
                        }

                        var response_tada = JsonConvert.DeserializeObject(responseFromServer, typeof(Response_TADA)) as Response_TADA;
                        foreach (var trx in response_tada.transactions)
                        {
                            if (trx.trxType == "walletTopup")
                            {
                                idTrx = trx.id.ToString();
                            }
                        }

                        ErasoftDbContext.Database.ExecuteSqlCommand("INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', '" + a.NOBUK + "', '', dateadd(hour, 7, getdate()), '" + responseFromServer + "', 1) ");
                        ErasoftDbContext.Database.ExecuteSqlCommand("INSERT INTO FAKTUR_API (No_Faktur, No_FakturRef, Tgl, LUNAS, PARTNER) VALUES ('" + a.NOBUK + "', '" + idTrx + "', dateadd(hour, 7, getdate()), '1', '2')"); //'" + id.ToString() + "'

                    }
                    catch (WebException e)
                    {
                        string err = "";
                        string errorMessage = "";
                        string messageError = "";
                        string messageResponse = "";

                        if (e.Status == WebExceptionStatus.ProtocolError)
                        {
                            WebResponse resp = e.Response;
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                            {
                                err = sr.ReadToEnd();
                            }

                            var response_error = JsonConvert.DeserializeObject(err, typeof(ErrorTada)) as ErrorTada;
                            if (response_error.message != null)
                            {
                                errorMessage = response_error.message;
                                errorMessage.Replace("\n", "").Replace("\r", "").Replace("\"", "").Replace("[", "").Replace("]", "").Replace("\\", "").Replace("'", "").TrimStart();
                                ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', '" + a.NOBUK + "', '" + errorMessage + ".', dateadd(hour, 7, getdate()), '" + myData + "', 0) ");
                                if (errorMessage.Contains("Access token expired"))
                                {
                                    token = await TadaAuthorization(data);
                                }
                                continue;
                            }
                            if (response_error.error != null)
                            {
                                messageError = response_error.error.system.message;
                                messageError.Replace("\n", "").Replace("\r", "").Replace("\"", "").Replace("[", "").Replace("]", "").Replace("\\", "").Replace("'", "").TrimStart();
                                ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', '" + a.NOBUK + "', '" + messageError + ".', dateadd(hour, 7, getdate()), '" + myData + "', 0) ");
                                if (messageError.Contains("Access token expired"))
                                {
                                    token = await TadaAuthorization(data);
                                }
                                continue;
                            }

                            //string errorMessage = response_error.error.ToString();
                            //string messageError = response_error.message[0].ToString();
                            //string msg_response = errorMessage.Replace("\n", "").Replace("\r", "").Replace("\"", "").Replace("[", "").Replace("]", "").Replace("\\", "").Replace("'", "").TrimStart();
                            //ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR VALUES (2, 'Sync TADA', '" + a.NOBUK + "', '" + msg_response + ".', dateadd(hour, 7, getdate()), '" + myData + "', 0) ");
                            continue;
                        }
                        else
                        {
                            err = "Call API " + e.Message;
                        }

                        ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', '" + a.NOBUK + "', '" + err + ".', dateadd(hour, 7, getdate()), '" + myData + "', 0) ");
                        //ErasoftDbContext.Database.ExecuteSqlCommand(@"DELETE FROM PARTNER_API_LOG_ERROR WHERE Created_Date < DATEADD(DAY, -14, GETDATE()) ");
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ErasoftDbContext.Database.ExecuteSqlCommand(@"INSERT INTO PARTNER_API_LOG_ERROR (fs_id, Modul, No_Bukti, Keterangan, Created_Date, JSON_String, Status) VALUES (2, 'Sync TADA', 'ERR_EXC', '" + msg + ".', dateadd(hour, 7, getdate()), null, 1) ");
                //ErasoftDbContext.Database.ExecuteSqlCommand(@"DELETE FROM PARTNER_API_LOG_ERROR WHERE Created_Date < DATEADD(DAY, -14, GETDATE()) ");
                //throw new Exception(msg);
            }

            ErasoftDbContext.Database.ExecuteSqlCommand(@"DELETE FROM PARTNER_API_LOG_ERROR WHERE Created_Date < DATEADD(DAY, -14, GETDATE()) ");

            return ret;
        }

        public class Order
        {
            public string NOBUK { get; set; }
            public string PHONE { get; set; }
            public string CUST_NAME { get; set; }
            public string BILLNUMBER { get; set; }
            public double AMOUNT { get; set; }
            public List<OrderItem> detailItem { get; set; }
        }

        public class OrderItem
        {
            public string NOBUK { get; set; }
            public string SKU { get; set; }
            public string ITEMNAME { get; set; }
            public double QUANTITY { get; set; }
            public double PRICE { get; set; }
        }

        public class TopupByPhone
        {
            public string phone { get; set; }
            public double amount { get; set; }
            public string programId { get; set; }
            public string billNumber { get; set; }
            public string walletId { get; set; }
            public string paymentMethod { get; set; }
            public List<Item> items { get; set; }
        }

        public class Item
        {
            public string sku { get; set; }
            public string itemName { get; set; }
            public double quantity { get; set; }
            public double price { get; set; }
        }

        public class TokenClass
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }
            public DateTime expiredAt { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
        }

        public class PartnerApiData
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public string Token { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }


        public class Response_TADA
        {
            public string phone { get; set; }
            public int amount { get; set; }
            public string billNumber { get; set; }
            public string programId { get; set; }
            public string paymentMethod { get; set; }
            public List<Item> items { get; set; }
            public string walletId { get; set; }
            public string walletName { get; set; }
            public int walletBalance { get; set; }
            public object balance { get; set; }
            public int reward { get; set; }
            public Card card { get; set; }
            public User user { get; set; }
            public List<Transaction> transactions { get; set; } //Transaction[]
        }

        public class Card
        {
            public int id { get; set; }
            public string no { get; set; }
            public int batchNum { get; set; }
            public string distributionId { get; set; }
            public string status { get; set; }
            public DateTime createdAt { get; set; }
        }

        public class User
        {
            public int id { get; set; }
            public string phone { get; set; }
            public object name { get; set; }
            public object email { get; set; }
        }

        //public class Item
        //{
        //    public string sku { get; set; }
        //    public string itemName { get; set; }
        //    public int quantity { get; set; }
        //    public int price { get; set; }
        //}

        public class Transaction
        {
            public int id { get; set; }
            public string trxNo { get; set; }
            public string trxType { get; set; }
            public string trxStatus { get; set; }
            public int trxAmount { get; set; }
            public string trxAmountType { get; set; }
            public string trxTime { get; set; }
            public int approvalCode { get; set; }
            public string cardNo { get; set; }
            public int balance { get; set; }
            public string balanceType { get; set; }
            public List<Item1> items { get; set; }
            public object amount { get; set; }
            public int point { get; set; }
            public string name { get; set; }
        }

        public class Item1
        {
            public string sku { get; set; }
            public string itemName { get; set; }
            public int quantity { get; set; }
            public int price { get; set; }
        }

        public class ErrorTada
        {
            public dynamic message { get; set; }
            public DataBN data { get; set; }
            public dynamic error { get; set; }
            public string title { get; set; }
        }


        public class ErrorCredential
        {
            public dynamic error { get; set; }
        }

        //public class Error
        //{
        //    public System system { get; set; }
        //    public User user { get; set; }
        //}

        //public class System
        //{
        //    public string message { get; set; }
        //}

        //public class User
        //{
        //    public string message { get; set; }
        //}s



        public class ErrorBillNumber
        {
            public DataBN data { get; set; }
            public string message { get; set; }
            public dynamic error { get; set; }
            public string title { get; set; }
        }

        public class DataBN
        {
            public string trxNo { get; set; }
            public string trxTime { get; set; }
            public int amount { get; set; }
            public int reward { get; set; }
        }


        public class ErrorToken
        {
            public string message { get; set; }
            public dynamic error { get; set; }
            public string title { get; set; }
        }
    }
}