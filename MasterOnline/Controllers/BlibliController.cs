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
using System.Net.Http;
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
        string username;
        public BlibliController()
        {
            MoDbContext = new MoDbContext();
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
            {
                if (sessionData.Account.UserId == "admin_manage")
                    ErasoftDbContext = new ErasoftContext();
                else
                    ErasoftDbContext = new ErasoftContext(sessionData.Account.DatabasePathErasoft);

                EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);
                username = sessionData.Account.Username;
            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    ErasoftDbContext = new ErasoftContext(accFromUser.DatabasePathErasoft);
                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                    username = accFromUser.Username;
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
            var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.API_CLIENT_P.Equals(data.API_client_password) && p.API_CLIENT_U.Equals(data.API_client_username) && !string.IsNullOrEmpty(p.Sort1_Cust)).SingleOrDefault();
            if (arf01inDB != null)
            {
                bool TokenExpired = true;
                var currentTimeRequest = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (!string.IsNullOrWhiteSpace(arf01inDB.REFRESH_TOKEN))
                {
                    var splitRefreshToken = arf01inDB.REFRESH_TOKEN.Split(';');
                    if (splitRefreshToken.Count() == 3)
                    {
                        if ((Convert.ToInt64(splitRefreshToken[2]) + Convert.ToInt64(splitRefreshToken[1]) - 10000) >= currentTimeRequest)
                        {
                            TokenExpired = false;
                        }
                    }
                }
                if (TokenExpired)
                {
                    //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
                    string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
                    string userMTA = data.mta_username_email_merchant;//<-- email user merchant
                    string passMTA = data.mta_password_password_merchant;//<-- pass merchant
                                                                         //apiId = "mta-api-sandbox:sandbox-secret-key";
                                                                         //string urll = "https://apisandbox.blibli.com/v2/oauth/token?grant_type=client_credentials";
                    string urll = "https://api.blibli.com/v2/oauth/token?channelId=MasterOnline";
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
                            //var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.API_CLIENT_P.Equals(data.API_client_password) && p.API_CLIENT_U.Equals(data.API_client_username)).SingleOrDefault();
                            //if (arf01inDB != null)
                            //{
                            arf01inDB.TOKEN = ret.access_token;
                            arf01inDB.REFRESH_TOKEN = ret.refresh_token + ";" + Convert.ToString(ret.expires_in) + ";" + Convert.ToString(currentTimeRequest);

                            //ADD BY TRI, SET STATUS_API
                            arf01inDB.STATUS_API = "1";
                            //END ADD BY TRI, SET STATUS_API

                            ErasoftDbContext.SaveChanges();
                            if (syncData)
                            {
                                data.merchant_code = arf01inDB.Sort1_Cust;
                                data.token = ret.access_token;
                                //GetProdukInReviewList(data);
                                GetPickupPoint(data); // untuk prompt pickup point saat insert barang
                                GetCategoryPerUser(data); // untuk category code yg muncul saat insert barang
                            }
                            //}
                        }
                        else
                        {
                            //ADD BY TRI, SET STATUS_API
                            arf01inDB.STATUS_API = "0";
                            //END ADD BY TRI, SET STATUS_API

                            ErasoftDbContext.SaveChanges();
                        }
                    }
                }
                GetQueueFeedDetail(data, null);
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

            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getPickupPoint?businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline";

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Get List Pickup Point",
                REQUEST_DATETIME = milisBack,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

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
                currentLog.REQUEST_EXCEPTION = ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
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
                                            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);
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
                else
                {
                    currentLog.REQUEST_RESULT = result.errorCode.Value;
                    currentLog.REQUEST_EXCEPTION = result.errorMessage.Value;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                }
            }

            return ret;
        }
        public enum StatusOrder
        {
            Cancel = 1,
            Paid = 2,
            PackagingINP = 3,
            ShippingINP = 4,
            Completed = 5
        }
        public async Task<string> GetOrderList(BlibliAPIData iden, StatusOrder stat, string connId, string CUST, string NAMA_CUST)
        {
            //if merchant code diisi. barulah GetOrderList
            string ret = "";
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";
            switch (stat)
            {
                case StatusOrder.Cancel:
                    //Cancel Order
                    status = "X";
                    break;
                case StatusOrder.Paid:
                    //paid
                    status = "FP";
                    break;
                //case StatusOrder.PackagingINP:
                //    //Packaging in Progress
                //    status = "301";
                //    break;
                case StatusOrder.ShippingINP:
                    //Shipping in Progress
                    status = "CX";
                    break;
                case StatusOrder.Completed:
                    //Completed (Shipping)
                    status = "D";
                    break;
                default:
                    break;
            }
            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
                                                                 //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/order/orderList", iden.API_secret_key);
            //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/createProduct";
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderList?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&status=" + status + "&channelId=MasterOnline&filterStartDate=" + Uri.EscapeDataString(DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss")) + "&filterEndDate=" + Uri.EscapeDataString(DateTime.UtcNow.AddHours(14).ToString("yyyy-MM-dd HH:mm:ss"));

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = milis.ToString(),
            //    REQUEST_ACTION = "Get Order List",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = stat.ToString(),
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

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
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }
            if (responseFromServer != null)
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(BlibliGetOrder)) as BlibliGetOrder;
                if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                {
                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    if (result.content.Count() > 0)
                    {
                        if (stat == StatusOrder.Paid)
                        {
                            var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();

                            foreach (var item in result.content)
                            {
                                if (!OrderNoInDb.Contains(item.orderNo))
                                {
                                    await GetOrderDetail(iden, item.orderNo, item.orderItemNo, connId, CUST, NAMA_CUST);
                                }
                            }
                        }
                        else
                        {
                            if (stat == StatusOrder.Completed)
                            {
                                foreach (var item in result.content)
                                {
                                    //remark by calvin 10 januari 2019, update saja, langsung ke sot01a, tidak usah getorderdetail lagi
                                    //await GetOrderDetail(iden, item.orderNo.Value, item.orderItemNo.Value, connId, CUST, NAMA_CUST);
                                    using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                                    {
                                        oConnection.Open();
                                        //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                                        //{
                                        using (SqlCommand oCommand = oConnection.CreateCommand())
                                        {
                                            oCommand.CommandType = CommandType.Text;
                                            oCommand.CommandText = "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI = '" + item.orderNo + "' AND STATUS_TRANSAKSI='03'";
                                            oCommand.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    //currentLog.REQUEST_RESULT = result.errorCode.Value;
                    //currentLog.REQUEST_EXCEPTION = result.errorMessage.Value;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }

        public async Task<string> GetOrderDetail(BlibliAPIData iden, string orderNo, string orderItemNo, string connId, string CUST, string NAMA_CUST)
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
            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/order/orderDetail", iden.API_secret_key);
            //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/order/orderDetail";
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderDetail?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&orderNo=" + orderNo + "&orderItemNo=" + orderItemNo + "&channelId=MasterOnline";

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Get Order Detail",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = orderNo,
                REQUEST_ATTRIBUTE_2 = orderItemNo,
                REQUEST_STATUS = "Pending",
            };

            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

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
                currentLog.REQUEST_EXCEPTION = ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }
            if (responseFromServer != null)
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(BlibliGetOrderDetail)) as BlibliGetOrderDetail;
                if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                {
                    //INSERT TEMP ORDER
                    using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                    {
                        oConnection.Open();
                        //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                        //{
                        using (SqlCommand oCommand = oConnection.CreateCommand())
                        {
                            oCommand.CommandText = "DELETE FROM [TEMP_BLI_ORDERDETAIL] WHERE CUST = '" + CUST + "'";
                            oCommand.ExecuteNonQuery();
                            //oCommand.Transaction = oTransaction;
                            oCommand.CommandType = CommandType.Text;
                            string sSQL = "INSERT INTO [TEMP_BLI_ORDERDETAIL] (";
                            sSQL += "[CUST],[NAMA_CUST],[CONN_ID],";
                            sSQL += "[orderNo],[orderItemNo],[qty],[orderDate],[autoCancelDate],";
                            sSQL += "[productName],[productItemName],[productPrice],[total],[itemWeightInKg],";
                            sSQL += "[custName],[orderStatus],[orderStatusString],[customerAddress],[customerEmail],";
                            sSQL += "[logisticsService],[currentLogisticService],[pickupPoint],[gdnSku],[gdnItemSku],";
                            sSQL += "[merchantSku],[totalWeight],[merchantDeliveryType],[awbNumber],[awbStatus],";
                            sSQL += "[shippingAddress],[shippingCity],[shippingSubDistrict],[shippingDistrict],[shippingProvince],";
                            sSQL += "[shippingZipCode],[shippingCost],[shippingMobile],[shippingInsuredAmount],[startOperationalTime],";
                            sSQL += "[endOperationalTime],[issuer],[refundResolution],[unFullFillReason],[unFullFillQty],";
                            sSQL += "[productTypeCode],[productTypeName],[custNote],[shippingRecipientName],[logisticsProductCode],";
                            sSQL += "[logisticsProductName],[logisticsOptionCode],[logisticsOptionName],";
                            sSQL += "[destinationLongitude],[destinationLatitude]";
                            sSQL += ") VALUES (";
                            sSQL += "@CUST,@NAMA_CUST,@CONN_ID,";
                            sSQL += "@orderNo,@orderItemNo,@qty,@orderDate,@autoCancelDate,";
                            sSQL += "@productName,@productItemName,@productPrice,@total,@itemWeightInKg,";
                            sSQL += "@custName,@orderStatus,@orderStatusString,@customerAddress,@customerEmail,";
                            sSQL += "@logisticsService,@currentLogisticService,@pickupPoint,@gdnSku,@gdnItemSku,";
                            sSQL += "@merchantSku,@totalWeight,@merchantDeliveryType,@awbNumber,@awbStatus,";
                            sSQL += "@shippingStreetAddress,@shippingCity,@shippingSubDistrict,@shippingDistrict,@shippingProvince,";
                            sSQL += "@shippingZipCode,@shippingCost,@shippingMobile,@shippingInsuredAmount,@startOperationalTime,";
                            sSQL += "@endOperationalTime,@issuer,@refundResolution,@unFullFillReason,@unFullFillQuantity,";
                            sSQL += "@productTypeCode,@productTypeName,@custNote,@shippingRecipientName,@logisticsProductCode,";
                            sSQL += "@logisticsProductName,@logisticsOptionCode,@logisticsOptionName,";
                            sSQL += "@destinationLongitude,@destinationLatitude";
                            sSQL += ")";
                            oCommand.CommandText = sSQL;

                            oCommand.Parameters.Add(new SqlParameter("@CUST", SqlDbType.NVarChar, 10));
                            oCommand.Parameters.Add(new SqlParameter("@NAMA_CUST", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@CONN_ID", SqlDbType.NVarChar, 100));

                            oCommand.Parameters.Add(new SqlParameter("@orderNo", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@orderItemNo", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@qty", SqlDbType.Float));
                            oCommand.Parameters.Add(new SqlParameter("@orderDate", SqlDbType.DateTime));
                            oCommand.Parameters.Add(new SqlParameter("@autoCancelDate", SqlDbType.DateTime));

                            oCommand.Parameters.Add(new SqlParameter("@productName", SqlDbType.NVarChar));
                            oCommand.Parameters.Add(new SqlParameter("@productItemName", SqlDbType.NVarChar));
                            oCommand.Parameters.Add(new SqlParameter("@productPrice", SqlDbType.Float));
                            oCommand.Parameters.Add(new SqlParameter("@total", SqlDbType.Float));
                            oCommand.Parameters.Add(new SqlParameter("@itemWeightInKg", SqlDbType.Float));

                            oCommand.Parameters.Add(new SqlParameter("@custName", SqlDbType.NVarChar, 250));
                            oCommand.Parameters.Add(new SqlParameter("@orderStatus", SqlDbType.NVarChar, 10));
                            oCommand.Parameters.Add(new SqlParameter("@orderStatusString", SqlDbType.NVarChar, 250));
                            oCommand.Parameters.Add(new SqlParameter("@customerAddress", SqlDbType.NVarChar));
                            oCommand.Parameters.Add(new SqlParameter("@customerEmail", SqlDbType.NVarChar, 250));

                            oCommand.Parameters.Add(new SqlParameter("@logisticsService", SqlDbType.NVarChar, 250));
                            oCommand.Parameters.Add(new SqlParameter("@currentLogisticService", SqlDbType.NVarChar, 250));
                            oCommand.Parameters.Add(new SqlParameter("@pickupPoint", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@gdnSku", SqlDbType.NVarChar, 100));
                            oCommand.Parameters.Add(new SqlParameter("@gdnItemSku", SqlDbType.NVarChar, 100));

                            oCommand.Parameters.Add(new SqlParameter("@merchantSku", SqlDbType.NVarChar, 100));
                            oCommand.Parameters.Add(new SqlParameter("@totalWeight", SqlDbType.Float));
                            oCommand.Parameters.Add(new SqlParameter("@merchantDeliveryType", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@awbNumber", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@awbStatus", SqlDbType.NVarChar, 50));

                            oCommand.Parameters.Add(new SqlParameter("@shippingStreetAddress", SqlDbType.NVarChar));
                            oCommand.Parameters.Add(new SqlParameter("@shippingCity", SqlDbType.NVarChar, 200));
                            oCommand.Parameters.Add(new SqlParameter("@shippingSubDistrict", SqlDbType.NVarChar, 200));
                            oCommand.Parameters.Add(new SqlParameter("@shippingDistrict", SqlDbType.NVarChar, 200));
                            oCommand.Parameters.Add(new SqlParameter("@shippingProvince", SqlDbType.NVarChar, 200));

                            oCommand.Parameters.Add(new SqlParameter("@shippingZipCode", SqlDbType.NVarChar, 10));
                            oCommand.Parameters.Add(new SqlParameter("@shippingCost", SqlDbType.Float));
                            oCommand.Parameters.Add(new SqlParameter("@shippingMobile", SqlDbType.NVarChar, 100));
                            oCommand.Parameters.Add(new SqlParameter("@shippingInsuredAmount", SqlDbType.Float));
                            oCommand.Parameters.Add(new SqlParameter("@startOperationalTime", SqlDbType.NVarChar, 200));

                            oCommand.Parameters.Add(new SqlParameter("@endOperationalTime", SqlDbType.NVarChar, 200));
                            oCommand.Parameters.Add(new SqlParameter("@issuer", SqlDbType.NVarChar, 200));
                            oCommand.Parameters.Add(new SqlParameter("@refundResolution", SqlDbType.NVarChar, 200));
                            oCommand.Parameters.Add(new SqlParameter("@unFullFillReason", SqlDbType.NVarChar));
                            oCommand.Parameters.Add(new SqlParameter("@unFullFillQuantity", SqlDbType.Float));

                            oCommand.Parameters.Add(new SqlParameter("@productTypeCode", SqlDbType.NVarChar, 10));
                            oCommand.Parameters.Add(new SqlParameter("@productTypeName", SqlDbType.NVarChar, 200));
                            oCommand.Parameters.Add(new SqlParameter("@custNote", SqlDbType.NVarChar));
                            oCommand.Parameters.Add(new SqlParameter("@shippingRecipientName", SqlDbType.NVarChar, 200));
                            oCommand.Parameters.Add(new SqlParameter("@logisticsProductCode", SqlDbType.NVarChar, 50));

                            oCommand.Parameters.Add(new SqlParameter("@logisticsProductName", SqlDbType.NVarChar, 200));
                            oCommand.Parameters.Add(new SqlParameter("@logisticsOptionCode", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@logisticsOptionName", SqlDbType.NVarChar, 200));
                            oCommand.Parameters.Add(new SqlParameter("@destinationLongitude", SqlDbType.NVarChar, 200));
                            oCommand.Parameters.Add(new SqlParameter("@destinationLatitude", SqlDbType.NVarChar, 200));

                            try
                            {
                                oCommand.Parameters[0].Value = CUST;
                                oCommand.Parameters[1].Value = NAMA_CUST;
                                oCommand.Parameters[2].Value = connId;

                                oCommand.Parameters["@orderNo"].Value = result.value.orderNo;
                                oCommand.Parameters["@orderItemNo"].Value = result.value.orderItemNo;
                                oCommand.Parameters["@qty"].Value = result.value.qty;
                                oCommand.Parameters["@orderDate"].Value = DateTimeOffset.FromUnixTimeMilliseconds(result.value.orderDate).UtcDateTime.AddHours(7);
                                oCommand.Parameters["@autoCancelDate"].Value = DateTimeOffset.FromUnixTimeMilliseconds(result.value.autoCancelDate).UtcDateTime.AddHours(7);

                                oCommand.Parameters["@productName"].Value = result.value.productName;
                                oCommand.Parameters["@productItemName"].Value = result.value.productItemName;
                                oCommand.Parameters["@productPrice"].Value = result.value.productPrice;
                                oCommand.Parameters["@total"].Value = result.value.total;
                                oCommand.Parameters["@itemWeightInKg"].Value = result.value.itemWeightInKg;

                                oCommand.Parameters["@custName"].Value = result.value.custName;
                                oCommand.Parameters["@orderStatus"].Value = result.value.orderStatus != null ? result.value.orderStatus : "";
                                oCommand.Parameters["@orderStatusString"].Value = result.value.orderStatusString != null ? result.value.orderStatusString : "";
                                oCommand.Parameters["@customerAddress"].Value = result.value.customerAddress != null ? result.value.customerAddress : "";
                                oCommand.Parameters["@customerEmail"].Value = result.value.customerEmail != null ? result.value.customerEmail : "";

                                oCommand.Parameters["@logisticsService"].Value = result.value.logisticsService != null ? result.value.logisticsService : "";
                                oCommand.Parameters["@currentLogisticService"].Value = result.value.currentLogisticService != null ? result.value.currentLogisticService : "";
                                oCommand.Parameters["@pickupPoint"].Value = result.value.pickupPoint != null ? result.value.pickupPoint : "";
                                oCommand.Parameters["@gdnSku"].Value = result.value.gdnSku;
                                oCommand.Parameters["@gdnItemSku"].Value = result.value.gdnItemSku;

                                oCommand.Parameters["@merchantSku"].Value = result.value.merchantSku;
                                oCommand.Parameters["@totalWeight"].Value = result.value.totalWeight;
                                oCommand.Parameters["@merchantDeliveryType"].Value = result.value.merchantDeliveryType;
                                oCommand.Parameters["@awbNumber"].Value = result.value.awbNumber != null ? result.value.awbNumber : "";
                                oCommand.Parameters["@awbStatus"].Value = result.value.awbStatus != null ? result.value.awbStatus : "";

                                oCommand.Parameters["@shippingStreetAddress"].Value = result.value.shippingStreetAddress;
                                oCommand.Parameters["@shippingCity"].Value = result.value.shippingCity;
                                oCommand.Parameters["@shippingSubDistrict"].Value = result.value.shippingSubDistrict;
                                oCommand.Parameters["@shippingDistrict"].Value = result.value.shippingSubDistrict;
                                oCommand.Parameters["@shippingProvince"].Value = result.value.shippingProvince;

                                oCommand.Parameters["@shippingZipCode"].Value = result.value.shippingZipCode;
                                oCommand.Parameters["@shippingCost"].Value = result.value.shippingCost;
                                oCommand.Parameters["@shippingMobile"].Value = result.value.shippingMobile;
                                oCommand.Parameters["@shippingInsuredAmount"].Value = result.value.shippingInsuredAmount;
                                oCommand.Parameters["@startOperationalTime"].Value = result.value.startOperationalTime != null ? result.value.startOperationalTime : "";

                                oCommand.Parameters["@endOperationalTime"].Value = result.value.endOperationalTime != null ? result.value.endOperationalTime : "";
                                oCommand.Parameters["@issuer"].Value = result.value.issuer != null ? result.value.issuer : "";
                                oCommand.Parameters["@refundResolution"].Value = result.value.refundResolution != null ? result.value.refundResolution : "";
                                oCommand.Parameters["@unFullFillReason"].Value = result.value.unFullFillReason != null ? result.value.unFullFillReason : "";
                                oCommand.Parameters["@unFullFillQuantity"].Value = result.value.unFullFillQuantity != null ? result.value.unFullFillQuantity : 0;

                                oCommand.Parameters["@productTypeCode"].Value = result.value.productTypeCode != null ? result.value.productTypeCode : "";
                                oCommand.Parameters["@productTypeName"].Value = result.value.productTypeName != null ? result.value.productTypeName : "";
                                oCommand.Parameters["@custNote"].Value = result.value.custNote != null ? result.value.custNote : "";
                                oCommand.Parameters["@shippingRecipientName"].Value = result.value.shippingRecipientName != null ? result.value.shippingRecipientName : "";
                                oCommand.Parameters["@logisticsProductCode"].Value = result.value.logisticsProductCode != null ? result.value.logisticsProductCode : "";

                                oCommand.Parameters["@logisticsProductName"].Value = result.value.logisticsProductName != null ? result.value.logisticsProductName : "";
                                oCommand.Parameters["@logisticsOptionCode"].Value = result.value.logisticsOptionCode != null ? result.value.logisticsOptionCode : "";
                                oCommand.Parameters["@logisticsOptionName"].Value = result.value.logisticsOptionName != null ? result.value.logisticsOptionName : "";
                                oCommand.Parameters["@destinationLongitude"].Value = result.value.destinationLongitude;
                                oCommand.Parameters["@destinationLatitude"].Value = result.value.destinationLatitude;

                                if (oCommand.ExecuteNonQuery() == 1)
                                {
                                    var connIdARF01C = Guid.NewGuid().ToString();

                                    var kabKot = "3174";
                                    var prov = "31";

                                    string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                    insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                    insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                                    insertPembeli += "('" + result.value.custName + "','" + result.value.shippingStreetAddress + "','" + result.value.shippingMobile + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
                                    insertPembeli += "1, 'IDR', '01', '" + result.value.shippingStreetAddress + "', 0, 0, 0, 0, '1', 0, 0, ";
                                    insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + result.value.shippingZipCode + "', '" + result.value.customerEmail + "', '" + kabKot + "', '" + prov + "', '', '','" + connIdARF01C + "')";
                                    EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);

                                    SqlCommand CommandSQL = new SqlCommand();
                                    //call sp to insert buyer data
                                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIdARF01C;

                                    EDB.ExecuteSQL("Con", "MoveARF01CFromTempTable", CommandSQL);

                                    CommandSQL = new SqlCommand();
                                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connId;
                                    CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                                    CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                                    CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                                    CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                                    CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 1;
                                    CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                                    CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                                    CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                                    EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);

                                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                                }
                            }
                            catch (Exception ex)
                            {
                                currentLog.REQUEST_EXCEPTION = ex.InnerException.Message;
                                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                            }
                        }
                    }
                }
                else
                {
                    currentLog.REQUEST_RESULT = Convert.ToString(result.errorCode);
                    currentLog.REQUEST_EXCEPTION = Convert.ToString(result.errorMessage);
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }
        public string UploadImage(BlibliAPIData iden, string[] imgPaths, string ProductCode, string merchantSku)
        {
            string ret = "";
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            string signature = CreateToken("POST\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/postImage", iden.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/postImage";

            //string boundary = "WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            string boundary = "WebKitFormBoundary7MA4YWxkTrZu0gW";
            string delimiter = "-------------" + boundary;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Upload Image",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = merchantSku,
                REQUEST_ATTRIBUTE_2 = ProductCode,
                REQUEST_STATUS = "Pending",
            };

            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("bearer " + iden.token));
            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            myReq.Accept = "application/json";
            myReq.ContentType = "multipart/form-data; boundary=" + delimiter;
            myReq.Headers.Add("requestId", milis.ToString());
            myReq.Headers.Add("sessionId", milis.ToString());
            myReq.Headers.Add("username", userMTA);
            string[,] postFields = new string[,]
            {
                {"username",userMTA },
                {"merchantCode",iden.merchant_code },
                {"productCode",ProductCode }
            };

            Stream postDataStream = GetPostStream(postFields, imgPaths, boundary);

            string responseFromServer = "";
            try
            {
                myReq.ContentLength = postDataStream.Length;
                postDataStream.Position = 0;
                using (var dataStream = myReq.GetRequestStream())
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = 0;

                    while ((bytesRead = postDataStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        dataStream.Write(buffer, 0, bytesRead);
                    }
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
                currentLog.REQUEST_EXCEPTION = ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
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
                            oCommand.CommandText = "INSERT INTO [QUEUE_FEED_BLIBLI] ([REQUESTID],[REQUEST_ACTION],[MERCHANT_CODE],[STATUS],[LOG_REQUEST_ID]) VALUES (@REQUESTID,'postImage',@MERCHANTCODE,'1',@LOG_REQUEST_ID)";
                            //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@REQUESTID", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@MERCHANTCODE", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@LOG_REQUEST_ID", SqlDbType.NVarChar, 50));

                            try
                            {
                                oCommand.Parameters[0].Value = result.requestId.Value;
                                oCommand.Parameters[1].Value = iden.merchant_code;
                                oCommand.Parameters[2].Value = currentLog.REQUEST_ID;

                                if (oCommand.ExecuteNonQuery() == 1)
                                {
                                    BlibliQueueFeedData queueData = new BlibliQueueFeedData
                                    {
                                        request_id = result.requestId.Value,
                                        log_request_id = currentLog.REQUEST_ID
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
                else
                {
                    currentLog.REQUEST_RESULT = result.errorCode.Value;
                    currentLog.REQUEST_EXCEPTION = result.errorMessage.Value;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }
        private static Stream GetPostStream(string[,] fields, string[] files, string boundary)
        {
            Stream postDataStream = new System.IO.MemoryStream();
            string postData = "";
            string eol = "\r\n";
            string delimiter = "-------------" + boundary;

            for (int i = 0; i < fields.GetLength(0); i++)
            {
                postData += "--" + delimiter + eol
                    + "Content-Disposition: form-data; name=\"" + fields[i, 0] + "\"" + eol + eol
                    + fields[i, 1] + eol;

            }
            postDataStream.Write(System.Text.Encoding.UTF8.GetBytes(postData), 0, postData.Length);
            int imageKe = 0;
            for (int i = 0; i < files.Length; i++)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(files[i])))
                {
                    string filePath = files[i];

                    //change by calvin 21 nov 2018
                    //FileInfo fileInfo = new FileInfo(filePath);
                    System.Uri fileUrl = new Uri(filePath);
                    String filename = fileUrl.PathAndQuery.Replace('/', Path.DirectorySeparatorChar);
                    FileInfo fileInfo = new FileInfo(filename);
                    //end change by calvin 21 nov 2018

                    string filepostData = "";

                    //start
                    filepostData = "--" + delimiter + eol
                    + "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" + eol
                    + "Content-Transfer-Encoding: binary" + eol + eol;

                    byte[] filepostDataBytes = System.Text.Encoding.UTF8.GetBytes(string.Format(filepostData,
                    "productImages[" + Convert.ToString(imageKe) + "]", "productImages[" + Convert.ToString(imageKe) + "]"));

                    postDataStream.Write(filepostDataBytes, 0, filepostDataBytes.Length);
                    //end start

                    //content
                    //change by calvin 19 nov 2018
                    //FileStream fileStream = fileInfo.OpenRead();
                    var req = System.Net.WebRequest.Create(filePath);
                    Stream fileStream = req.GetResponse().GetResponseStream();
                    //end change by calvin 19 nov 2018
                    byte[] buffer = new byte[1024];
                    int bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        postDataStream.Write(buffer, 0, bytesRead);
                    }
                    fileStream.Close();
                    //end content

                    //finish
                    filepostData = eol;
                    byte[] endfilepostDataBytes = System.Text.Encoding.UTF8.GetBytes(filepostData);
                    postDataStream.Write(endfilepostDataBytes, 0, endfilepostDataBytes.Length);
                    //end finish

                    imageKe++;
                }
            }

            string endpostDataStream = "--" + delimiter + "--" + eol;
            byte[] endpostDataBytes = System.Text.Encoding.UTF8.GetBytes(endpostDataStream);
            postDataStream.Write(endpostDataBytes, 0, endpostDataBytes.Length);

            return postDataStream;
        }
        public void GetProdukInReviewList(BlibliAPIData iden, string requestID, string ProductCode, string gdnSku, string api_log_requestId)
        {
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/inProcessProduct", iden.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/inProcessProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&channelId=MasterOnline";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("bearer " + iden.token));
            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            myReq.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
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
                //perlu tes item tanpa varian
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ProductInReviewListResult)) as ProductInReviewListResult;
                if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                {
                    foreach (var item in result.content)
                    {
                        if (item.productItems.Count() > 0)
                        {
                            bool successPerItem = false;
                            foreach (var item_var in item.productItems)
                            {
                                if (item_var.upcCode != "-" && !string.IsNullOrWhiteSpace(item_var.upcCode))
                                {
                                    using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                                    {
                                        oConnection.Open();
                                        using (SqlCommand oCommand = oConnection.CreateCommand())
                                        {
                                            try
                                            {
                                                oCommand.CommandType = CommandType.Text;
                                                oCommand.CommandText = "UPDATE STF02H SET BRG_MP = @BRG_MP WHERE BRG = @BRG AND IDMARKET = @IDMARKET ";
                                                //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                                                oCommand.Parameters.Add(new SqlParameter("@BRG", SqlDbType.NVarChar, 50));
                                                oCommand.Parameters.Add(new SqlParameter("@IDMARKET", SqlDbType.Int));
                                                oCommand.Parameters.Add(new SqlParameter("@BRG_MP", SqlDbType.NVarChar, 50));
                                            
                                                oCommand.Parameters[0].Value = item_var.upcCode; 
                                                oCommand.Parameters[1].Value = iden.idmarket;
                                                oCommand.Parameters[2].Value = item_var.productItemCode; // seharusnya gdnSku + item_var.productItemCode, tidak ketemu darimana gdnSku nya

                                                if (oCommand.ExecuteNonQuery() == 1)
                                                {
                                                    successPerItem = true;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                successPerItem = false;
                                            }
                                        }
                                    }
                                }
                            }
                            if (successPerItem)
                            {
                                string STF02_BRG = "";
                                var apiLogInDb = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == api_log_requestId).SingleOrDefault();
                                if (apiLogInDb != null)
                                {
                                    apiLogInDb.REQUEST_STATUS = "Success";
                                    apiLogInDb.REQUEST_RESULT = "";
                                    apiLogInDb.REQUEST_EXCEPTION = "";
                                    STF02_BRG = apiLogInDb.REQUEST_ATTRIBUTE_1;
                                    ErasoftDbContext.SaveChanges();
                                }

                                using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                                {
                                    oConnection.Open();
                                    using (SqlCommand oCommand = oConnection.CreateCommand())
                                    {
                                        try
                                        {
                                            oCommand.CommandType = CommandType.Text;
                                            oCommand.CommandText = "UPDATE STF02H SET BRG_MP = @BRG_MP WHERE BRG = @BRG AND IDMARKET = @IDMARKET ";
                                            oCommand.Parameters.Add(new SqlParameter("@BRG", SqlDbType.NVarChar, 50));
                                            oCommand.Parameters.Add(new SqlParameter("@IDMARKET", SqlDbType.Int));
                                            oCommand.Parameters.Add(new SqlParameter("@BRG_MP", SqlDbType.NVarChar, 50));
                                            
                                            oCommand.Parameters[0].Value = STF02_BRG; // BRG MO
                                            oCommand.Parameters[1].Value = iden.idmarket;
                                            oCommand.Parameters[2].Value = ProductCode; // STF02H.BRG_MP, seharusnya gdnSku + ProductCode, tidak ketemu darimana gdnSku nya
                                            oCommand.ExecuteNonQuery();
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
            }
        }
        public class Features
        {
            public string name { get; set; }
            public string value { get; set; }
        }
        public class Variasi
        {
            public string name { get; set; }
            public string value { get; set; }
        }
        public class imagess
        {
            public string locationPath { get; set; }
            public int sequence { get; set; }
        }
        public class UploadProdukNewData
        {
            public string merchantCode { get; set; }
            public List<UploadProdukNewDataProduct> products { get; set; }

        }
        public class UploadProdukNewDataProduct
        {
            public string merchantCode { get; set; }
            public string categoryCode { get; set; }
            public string productName { get; set; }
            public string url { get; set; }
            public string merchantSku { get; set; }
            public int tipePenanganan { get; set; }
            public int price { get; set; }
            public int salePrice { get; set; }
            public int stock { get; set; }
            public int minimumStock { get; set; }
            public string pickupPointCode { get; set; }
            public double length { get; set; }
            public double width { get; set; }
            public double height { get; set; }
            public double weight { get; set; }
            public string desc { get; set; }
            public string uniqueSellingPoint { get; set; }
            public string productStory { get; set; }
            public string upcCode { get; set; }
            public bool display { get; set; }
            public bool buyable { get; set; }
            public List<Features> features { get; set; }
            public List<Variasi> variasi { get; set; }
            public List<imagess> images { get; set; }

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
                sSQL += "SELECT A.ACODE_" + i.ToString() + " AS CATEGORY_CODE,A.ANAME_" + i.ToString() + " AS CATEGORY_NAME,B.ATYPE_" + i.ToString() + " AS CATEGORY_TYPE,A.AVALUE_" + i.ToString() + " AS VALUE FROM STF02H A INNER JOIN MO.DBO.ATTRIBUTE_BLIBLI B ON A.CATEGORY_CODE = B.CATEGORY_CODE WHERE A.BRG='" + data.kode + "' AND A.IDMARKET = '" + data.IDMarket + "' " + System.Environment.NewLine;
                if (i < 30)
                {
                    sSQL += "UNION ALL " + System.Environment.NewLine;
                }
            }

            DataSet dsFeature = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE <> 'DEFINING_ATTRIBUTE' ");
            DataSet dsVariasi = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE = 'DEFINING_ATTRIBUTE' ");

            features += "{ \"name\": \"Brand\", \"value\": \"" + data.Brand + "\"}, ";
            List<Features> featuresList = new List<Features>();
            featuresList.Add(new Features
            {
                name = "Brand",
                value = data.Brand
            });
            for (int i = 0; i < dsFeature.Tables[0].Rows.Count; i++)
            {
                features += "{ \"name\": \"" + Convert.ToString(dsFeature.Tables[0].Rows[i]["CATEGORY_NAME"]) + "\", \"value\": \"" + Convert.ToString(dsFeature.Tables[0].Rows[i]["VALUE"]).Trim() + "\"},";
                featuresList.Add(new Features
                {
                    name = Convert.ToString(dsFeature.Tables[0].Rows[i]["CATEGORY_NAME"]),
                    value = Convert.ToString(dsFeature.Tables[0].Rows[i]["VALUE"]).Trim()
                });
            }
            List<Variasi> VariasiList = new List<Variasi>();
            for (int i = 0; i < dsVariasi.Tables[0].Rows.Count; i++)
            {
                string[] values = Convert.ToString(dsVariasi.Tables[0].Rows[i]["VALUE"]).Split(',');
                for (int a = 0; a < values.Length; a++)
                {
                    VariasiList.Add(new Variasi
                    {
                        name = Convert.ToString(dsVariasi.Tables[0].Rows[i]["CATEGORY_NAME"]),
                        value = Convert.ToString(values[a]).Trim()
                    });
                    variasi += "{\"name\": \"" + Convert.ToString(dsVariasi.Tables[0].Rows[i]["CATEGORY_NAME"]) + "\",\"value\": \"" + Convert.ToString(values[a]).Trim() + "\"},";
                }
            }
            List<imagess> ImagesList = new List<imagess>();
            for (int i = 0; i < 3; i++)
            {
                ImagesList.Add(new imagess
                {
                    locationPath = data.Brand + "_" + data.nama + "_full0" + Convert.ToString(i + 1) + ".jpg",
                    sequence = i
                });
            }
            List<UploadProdukNewDataProduct> products = new List<UploadProdukNewDataProduct>();
            products.Add(new UploadProdukNewDataProduct
            {
                merchantCode = iden.merchant_code,
                categoryCode = data.CategoryCode,
                productName = data.nama,
                url = "-",
                merchantSku = data.kode,
                tipePenanganan = 1,
                price = Convert.ToInt32(data.Price),
                salePrice = Convert.ToInt32(data.MarketPrice),
                stock = Convert.ToInt32(data.Qty),
                minimumStock = Convert.ToInt32(data.MinQty),
                pickupPointCode = data.PickupPoint,
                length = Convert.ToDouble(data.Length),
                width = Convert.ToDouble(data.Width),
                height = Convert.ToDouble(data.Height),
                weight = Convert.ToDouble(data.berat),
                desc = data.Keterangan,
                uniqueSellingPoint = data.Keterangan,
                productStory = data.Keterangan,
                upcCode = "-",
                display = data.display == "true" ? true : false,
                buyable = true,
                features = featuresList,
                variasi = VariasiList,
                images = ImagesList
            });
            UploadProdukNewData newData = new UploadProdukNewData
            {
                merchantCode = iden.merchant_code,
                products = products
            };
            string myData = "{";
            myData += "\"merchantCode\": \"" + iden.merchant_code + "\", ";
            myData += "\"products\": ";
            myData += "[{ ";  //MERCHANT ID ADA DI https://merchant.blibli.com/MTA/store-info/store-info
            {
                myData += "\"merchantCode\": \"" + iden.merchant_code + "\",  ";
                myData += "\"categoryCode\": \"" + data.CategoryCode + " \", ";                                       //LIHAT BAGIAN GETKATEGORI
                myData += "\"productName\": \"" + EscapeForJson(data.nama) + "\", ";                                 // NAMA PRODUK
                myData += "\"url\": \"\", ";                   // LINK URL IKLAN KALO ADA
                myData += "\"merchantSku\": \"" + EscapeForJson(data.kode) + "\", ";                                // SKU
                myData += "\"tipePenanganan\": 1, ";                                            // 1= reguler produk (dikirim oleh blili)| 2= dikirim oleh kurir | 3 =ambil sendiri di toko
                myData += "\"price\": " + data.MarketPrice + ", ";                                                      //harga reguler (no diskon)
                myData += "\"salePrice\": " + data.MarketPrice + ", ";                                                // harga yg tercantum di display blibli
                myData += "\"stock\": " + data.Qty + ", ";
                myData += "\"minimumStock\": " + data.MinQty + ", ";
                myData += "\"pickupPointCode\": \"" + data.PickupPoint + "\", ";                                   //pick up poin code, baca GetPickUp
                myData += "\"length\": " + data.Length + ", ";
                myData += "\"width\": " + data.Width + ", ";
                myData += "\"height\": " + data.Height + ", ";
                myData += "\"weight\": " + data.berat + ", "; // dalam gram, sama seperti MO
                myData += "\"desc\": \"" + EscapeForJson(data.Keterangan) + "\", ";
                myData += "\"uniqueSellingPoint\": \"" + EscapeForJson(data.Keterangan) + "\", "; //ex : Unique selling point of current product
                myData += "\"productStory\": \"" + EscapeForJson(data.Keterangan) + "\", "; //ex : This product is launched at 25 Des 2016, made in Indonesia
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

            myData = JsonConvert.SerializeObject(newData);

            //myData = myData.Replace(System.Environment.NewLine, "\\r\\n");
            //myData = System.Text.RegularExpressions.Regex.Replace(myData, @"\r\n?|\n", "\\n");
            //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
            myData = myData.Replace("\\r\\n", "\\n").Replace("–", "-").Replace("\\\"\\\"", "").Replace("×", "x");
            string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
            //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/createProduct";
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/createProduct";

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Create Product",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = data.kode,
                REQUEST_ATTRIBUTE_2 = data.nama,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

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
                currentLog.REQUEST_EXCEPTION = ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
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
                            oCommand.CommandText = "INSERT INTO [QUEUE_FEED_BLIBLI] ([REQUESTID],[REQUEST_ACTION],[MERCHANT_CODE],[STATUS],[LOG_REQUEST_ID]) VALUES (@REQUESTID,'createProductV2',@MERCHANTCODE,'1',@LOG_REQUEST_ID)";
                            //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@REQUESTID", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@MERCHANTCODE", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@LOG_REQUEST_ID", SqlDbType.NVarChar, 50));

                            try
                            {
                                oCommand.Parameters[0].Value = result.requestId.Value;
                                oCommand.Parameters[1].Value = iden.merchant_code;
                                oCommand.Parameters[2].Value = currentLog.REQUEST_ID;

                                if (oCommand.ExecuteNonQuery() == 1)
                                {
                                    //ADD BY CALVIN 9 NOV 2018
                                    try
                                    {
                                        //SET BRG_MP JADI PENDING, AGAR TIDAK DOUBLE UPLOAD
                                        oCommand.CommandType = CommandType.Text;
                                        oCommand.CommandText = "UPDATE H SET BRG_MP='PENDING' FROM STF02H H INNER JOIN ARF01 A ON H.IDMARKET = A.RECNUM WHERE H.BRG=@MERCHANTSKU AND A.SORT1_CUST=@MERCHANTCODE AND ISNULL(H.BRG_MP,'') = ''";
                                        oCommand.Parameters.Add(new SqlParameter("@MERCHANTSKU", SqlDbType.NVarChar, 20));
                                        oCommand.Parameters[3].Value = Convert.ToString(data.kode);
                                        oCommand.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    //END ADD BY CALVIN 9 NOV 2018

                                    BlibliQueueFeedData queueData = new BlibliQueueFeedData
                                    {
                                        request_id = result.requestId.Value,
                                        log_request_id = currentLog.REQUEST_ID
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
                else
                {
                    currentLog.REQUEST_EXCEPTION = result.errorCode.Value;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                }
            }

            return ret;
        }

        protected string EscapeForJson(string s)
        {
            //string quoted = System.Web.Helpers.Json.Encode(s.Replace("–", "-").Replace("\"\"", "''"));
            //return quoted.Substring(1, quoted.Length - 2);
            string quoted = Newtonsoft.Json.JsonConvert.ToString(s);
            return quoted.Substring(1, quoted.Length - 2);
        }
        public class fillOrderAWBData
        {
            public int type { get; set; }
            public string awbNo { get; set; }
            public string orderNo { get; set; }
            public string orderItemNo { get; set; }
            public List<fillOrderAWBCombineShipping> combineShipping { get; set; }
        }
        public class fillOrderAWBCombineShipping
        {
            public string orderNo { get; set; }
            public string orderItemNo { get; set; }
        }
        protected enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4
        }
        protected void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, BlibliAPIData iden, API_LOG_MARKETPLACE data)
        {
            switch (action)
            {
                case api_status.Pending:
                    {
                        var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.merchant_code).FirstOrDefault();
                        var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                        {
                            CUST = arf01 != null ? arf01.CUST : iden.merchant_code,
                            CUST_ATTRIBUTE_1 = iden.merchant_code,
                            CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                            CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                            CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                            CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                            MARKETPLACE = "Blibli",
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
        public void fillOrderAWB(BlibliAPIData iden, string awbNo, string orderNo, string orderItemNo)
        {
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            string myData = "{";
            myData += "\"type\": 1, ";
            myData += "\"awbNo\": \"" + awbNo + "\", ";
            myData += "\"orderNo\": \"" + orderNo + "\", ";
            myData += "\"orderItemNo\": \"" + orderItemNo + "\" ";
            myData += "\"combineShipping\":[{";
            myData += "\"orderNo\": \"" + orderNo + "\", ";
            myData += "\"orderItemNo\": \"" + orderItemNo + "\" ";
            myData += "}] ";
            myData += "}";

            List<fillOrderAWBCombineShipping> combineShipping = new List<fillOrderAWBCombineShipping>();
            combineShipping.Add(new fillOrderAWBCombineShipping
            {
                orderNo = orderNo,
                orderItemNo = orderItemNo
            });

            fillOrderAWBData thisData = new fillOrderAWBData();

            thisData.type = 1;
            thisData.awbNo = awbNo;
            thisData.orderNo = orderNo;
            thisData.orderItemNo = orderItemNo;
            thisData.combineShipping = combineShipping;

            myData = JsonConvert.SerializeObject(thisData);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Update No Resi",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = orderNo,
                REQUEST_ATTRIBUTE_2 = orderItemNo,
                REQUEST_ATTRIBUTE_3 = awbNo,
                REQUEST_STATUS = "Pending",
            };

            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
            string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/order/fulfillRegular", iden.API_secret_key);
            //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/order/fulfillRegular";
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/fulfillRegular?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&storeId=10001" + "&channelId=MasterOnline";

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
                currentLog.REQUEST_EXCEPTION = ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }
            if (responseFromServer != null)
            {
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                }
                else
                {
                    currentLog.REQUEST_RESULT = result.errorCode.Value;
                    currentLog.REQUEST_EXCEPTION = result.errorMessage.Value;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                }
            }
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
            double QOHBlibli = 0;
            //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
            string signature_1 = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getProductSummary", iden.API_secret_key);
            string[] brg_mp = data.kode_mp.Split(';');
            if (brg_mp.Length == 2)
            {
                string urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getProductSummary?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&channelId=MasterOnline";
                urll_1 += "&size=100";
                if (!string.IsNullOrEmpty(data.nama))
                {
                    var search = data.nama.Split(' ');
                    urll_1 += "&productName=" + search[1];
                }

                HttpWebRequest myReq_1 = (HttpWebRequest)WebRequest.Create(urll_1);
                myReq_1.Method = "GET";
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
                            List<ProductSummaryResult> availableGdnSkus = new List<ProductSummaryResult>();

                            foreach (var item in result.content)
                            {
                                if (item.gdnSku.Value.Contains(brg_mp[0]))
                                {
                                    availableGdnSkus.Add(new ProductSummaryResult
                                    {
                                        gdnSku = (item.gdnSku.Value),
                                        stockAvailableLv2 = item.stockAvailableLv2.Value,
                                        sellingPrice = item.sellingPrice.Value,
                                    });
                                }
                            }

                            //foreach (var item in result.content)
                            //{
                            //    QOHBlibli = item.stockAvailableLv2.Value;
                            //}

                            string myData = "{";
                            myData += "\"merchantCode\": \"" + iden.merchant_code + "\", ";
                            myData += "\"productRequests\": ";
                            myData += "[ ";  //MERCHANT ID ADA DI https://merchant.blibli.com/MTA/store-info/store-info
                            {
                                foreach (var item in availableGdnSkus)
                                {
                                    QOHBlibli = item.stockAvailableLv2;
                                    if (Convert.ToInt32(data.Qty) - QOHBlibli != 0) // tidak sama
                                    {
                                        QOHBlibli = Convert.ToInt32(data.Qty) - QOHBlibli;
                                    }
                                    else
                                    {
                                        QOHBlibli = 0;
                                    }
                                    //if (QOHBlibli != 0)
                                    {
                                        myData += "{";
                                        myData += "\"gdnSku\": \"" + item.gdnSku + "\",  ";
                                        myData += "\"stock\": " + Convert.ToString(QOHBlibli) + ", ";
                                        myData += "\"minimumStock\": " + data.MinQty + ", ";
                                        myData += "\"price\": " + data.MarketPrice + ", ";
                                        myData += "\"salePrice\": " + data.MarketPrice + ", ";// harga yg tercantum di display blibli
                                        //myData += "\"salePrice\": " + item.sellingPrice + ", ";// harga yg promo di blibli
                                        myData += "\"buyable\": " + data.display + ", ";
                                        myData += "\"displayable\": " + data.display + " "; // true=tampil    
                                        myData += "},";
                                    }
                                }
                            }
                            myData = myData.Remove(myData.Length - 1);
                            myData += "]";
                            myData += "}";

                            string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/updateProduct", iden.API_secret_key);
                            //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/createProduct";
                            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/updateProduct";

                            //add by calvin 15 nov 2018
                            urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/updateProduct?channelId=MasterOnline";
                            //end add by calvin 15 nov 2018

                            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                            {
                                REQUEST_ID = milis.ToString(),
                                REQUEST_ACTION = "Update QOH dan Display",
                                REQUEST_DATETIME = milisBack,
                                REQUEST_ATTRIBUTE_1 = data.kode,
                                REQUEST_ATTRIBUTE_2 = brg_mp[1], //product_code
                                REQUEST_ATTRIBUTE_3 = brg_mp[0], //gdnsku
                                REQUEST_STATUS = "Pending",
                            };
                            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

                            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                            myReq.Method = "POST";
                            myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                            myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                            myReq.Accept = "application/json";
                            myReq.ContentType = "application/json";
                            myReq.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
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
                                currentLog.REQUEST_EXCEPTION = ex.InnerException.Message;
                                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                            }
                            if (responseFromServer != null)
                            {
                                dynamic result2 = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                                if (string.IsNullOrEmpty(result2.errorCode.Value))
                                {
                                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);

                                    BlibliQueueFeedData queueData = new BlibliQueueFeedData
                                    {
                                        request_id = result2.requestId.Value,
                                        log_request_id = currentLog.REQUEST_ID
                                    };
                                    GetQueueFeedDetail(iden, queueData);
                                }
                                else
                                {
                                    currentLog.REQUEST_RESULT = result.errorCode.Value;
                                    currentLog.REQUEST_EXCEPTION = result.errorMessage.Value;
                                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            return ret;
        }
        public class ProductSummaryResult
        {
            public string gdnSku { get; set; }
            public double stockAvailableLv2 { get; set; }
            public double sellingPrice { get; set; }
        }
        //public string SetCategoryCode(BlibliAPIData data)
        //{
        //    //HASIL MEETING : SIMPAN CATEGORY DAN ATTRIBUTE NYA KE DATABASE MO
        //    //INSERT JIKA CATEGORY_CODE UTAMA BELUM ADA DI MO

        //    string ret = "";

        //    long milis = CurrentTimeMillis();
        //    DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

        //    string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
        //    string userMTA = data.mta_username_email_merchant;//<-- email user merchant
        //    string passMTA = data.mta_password_password_merchant;//<-- pass merchant

        //    string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getCategory", data.API_secret_key);
        //    string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategory?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code);


        //    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
        //    myReq.Method = "GET";
        //    myReq.Headers.Add("Authorization", ("bearer " + data.token));
        //    myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
        //    myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
        //    myReq.Accept = "application/json";
        //    myReq.ContentType = "application/json";
        //    myReq.Headers.Add("requestId", milis.ToString());
        //    myReq.Headers.Add("sessionId", milis.ToString());
        //    myReq.Headers.Add("username", userMTA);
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

        //    }

        //    //Stream dataStream = myReq.GetRequestStream();
        //    //WebResponse response = myReq.GetResponse();
        //    //dataStream = response.GetResponseStream();
        //    //StreamReader reader = new StreamReader(dataStream);
        //    //string responseFromServer = reader.ReadToEnd();
        //    //dataStream.Close();
        //    //response.Close();

        //    // nilai token yg diambil adalah access-token. setelah 24jam biasanya harus masuk ke refresh token. dan harus diambil lagi acces token yg baru
        //    //cek refreshToken
        //    if (responseFromServer != null)
        //    {
        //        dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
        //        if (string.IsNullOrEmpty(result.errorCode.Value))
        //        {
        //            if (result.content.Count > 0)
        //            {
        //                //Data Source = 13.251.222.53; Initial Catalog = ERASOFT_rahmamk; Persist Security Info = True; User ID = sa; Password = admin123 ^
        //                //using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))


        //                using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
        //                {
        //                    oConnection.Open();
        //                    //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
        //                    //{
        //                    using (SqlCommand oCommand = oConnection.CreateCommand())
        //                    {
        //                        //oCommand.CommandText = "DELETE FROM [CATEGORY_BLIBLI] WHERE ARF01_SORT1_CUST='" + data.merchant_code + "'";
        //                        //oCommand.ExecuteNonQuery();
        //                        //oCommand.Transaction = oTransaction;
        //                        oCommand.CommandType = CommandType.Text;
        //                        oCommand.CommandText = "UPDATE [ARF01] SET KODE=@KODE WHERE SORT1_CUST='" + data.merchant_code + "' ";
        //                        //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
        //                        oCommand.Parameters.Add(new SqlParameter("@KODE", SqlDbType.NVarChar, 250));

        //                        try
        //                        {
        //                            string kode = "";
        //                            //oCommand.Parameters[0].Value = data.merchant_code;
        //                            foreach (var item in result.content) //foreach parent level top
        //                            {
        //                                kode = kode + item.categoryCode.Value + ";";
        //                            }
        //                            kode = kode.Substring(0, kode.Length - 1);
        //                            oCommand.Parameters[0].Value = kode;
        //                            if (oCommand.ExecuteNonQuery() == 1)
        //                            {
        //                            }
        //                            //oTransaction.Commit();
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            //oTransaction.Rollback();
        //                        }
        //                    }
        //                    //}
        //                }
        //            }
        //        }
        //    }

        //    return ret;
        //}

        public string GetQueueFeedDetail(BlibliAPIData data, BlibliQueueFeedData feed)
        {
            string ret = "";

            if (feed != null)//satu requestId
            {
                prosesQueueFeedDetail(data, feed.request_id, feed.log_request_id);
            }
            else
            {
                DataSet dsRequestIdList = new DataSet();
                dsRequestIdList = EDB.GetDataSet("sCon", "QUEUE_FEED_BLIBLI", "SELECT * FROM [QUEUE_FEED_BLIBLI] WHERE MERCHANT_CODE='" + data.merchant_code + "' AND [STATUS] = '1'");
                if (dsRequestIdList.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsRequestIdList.Tables[0].Rows.Count; i++)
                    {
                        prosesQueueFeedDetail(data, Convert.ToString(dsRequestIdList.Tables[0].Rows[i]["REQUESTID"]), Convert.ToString(dsRequestIdList.Tables[0].Rows[i]["LOG_REQUEST_ID"]));
                    }
                }
            }


            return ret;
        }
        public BindingBase getProduct(BlibliAPIData iden, string productCode, int page, string cust, int recordCount)
        {
            var ret = new BindingBase
            {
                status = 0,
                recordCount = recordCount,
            };
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Get Item List",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getProductSummary", iden.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getProductSummary?requestId=" + Uri.EscapeDataString(Convert.ToString(milis)) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + (string.IsNullOrEmpty(productCode) ? "" : "&gdnSku=" + Uri.EscapeDataString(productCode));
            urll += "&page=" + page + "&size=10";
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
                //ret.message = ex.Message;
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ListProductBlibli)) as ListProductBlibli;
                if (listBrg != null)
                {
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    if (string.IsNullOrEmpty(listBrg.errorCode))
                    {
                        if (listBrg.content != null)
                        {
                            if (listBrg.content.Count > 0)
                            {
                                ret.status = 1;
                                int IdMarket = ErasoftDbContext.ARF01.Where(c => c.CUST.Equals(cust)).FirstOrDefault().RecNum.Value;
                                if (listBrg.content.Count == 10)
                                    ret.message = (page + 1).ToString();

                                //add 13 Feb 2019, tuning
                                var stf02h_local = ErasoftDbContext.STF02H.Where(m => m.IDMARKET == IdMarket).ToList();
                                var tempBrg_local = ErasoftDbContext.TEMP_BRG_MP.Where(m => m.IDMARKET == IdMarket).ToList();
                                //end add 13 Feb 2019, tuning

                                foreach (var item in listBrg.content)
                                {
                                    //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.Equals(item.gdnSku + ";" + item.productItemCode) && t.IDMARKET == IdMarket).FirstOrDefault();
                                    //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(item.gdnSku + ";" + item.productItemCode) && t.IDMARKET == IdMarket).FirstOrDefault();
                                    var tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == (item.gdnSku + ";" + item.productItemCode).ToUpper()).FirstOrDefault();
                                    var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == (item.gdnSku + ";" + item.productItemCode).ToUpper()).FirstOrDefault();
                                    if (tempbrginDB == null && brgInDB == null)
                                    {
                                        var retDet = getProductDetail(iden, item.gdnSku, cust, (item.displayable ? 1 : 0)/*, tempBrg_local, stf02h_local*/);
                                        if (retDet.status >= 1)
                                        {
                                            ret.recordCount += retDet.status;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ret.message = "Gagal mendapatkan produk";
                                currentLog.REQUEST_EXCEPTION = ret.message;
                                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                            }
                        }
                        else
                        {
                            ret.message = "Gagal mendapatkan produk";
                            currentLog.REQUEST_EXCEPTION = ret.message;
                            manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                        }
                    }
                    else
                    {
                        ret.message = listBrg.errorMessage;
                        currentLog.REQUEST_EXCEPTION = ret.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                    }
                }
                else
                {
                    ret.message = "Gagal mendapatkan produk";
                    currentLog.REQUEST_EXCEPTION = ret.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            else
            {
                ret.message = "Failed to get response from Blibli API";
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }
            return ret;
        }
        protected BindingBase getProductDetail(BlibliAPIData iden, string productCode, string cust, int display/*, List<TEMP_BRG_MP> tempBrg_local, List<STF02H> stf02h_local*/)
        {
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            var ret = new BindingBase();
            ret.status = 0;
            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/detailProduct", iden.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?requestId=" + Uri.EscapeDataString(Convert.ToString(milis)) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&gdnSku=" + Uri.EscapeDataString(productCode) + "&username=" + iden.API_client_username + "&channelId=MasterOnline";

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
                ret.message = ex.Message;
            }

            if (responseFromServer != null)
            {
                //var brg = JsonConvert.DeserializeObject(responseFromServer, typeof(DetailBrgBlibli)) as DetailBrgBlibli;

                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {
                    if (result.value != null)
                    {
                        //var b = result.value.description.ToString();
                        //var a = HttpUtility.HtmlEncode(b);
                        //var c = HttpUtility.HtmlDecode(a);
                        string IdMarket = ErasoftDbContext.ARF01.Where(c => c.CUST.Equals(cust)).FirstOrDefault().RecNum.ToString();
                        string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, NAMA3, BERAT, PANJANG, LEBAR, TINGGI, CUST, ";
                        sSQL += "Deskripsi, IDMARKET, HJUAL, HJUAL_MP, DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, KODE_BRG_INDUK, TYPE,";
                        sSQL += "ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
                        sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20, ";
                        sSQL += "ACODE_21, ANAME_21, AVALUE_21, ACODE_22, ANAME_22, AVALUE_22, ACODE_23, ANAME_23, AVALUE_23, ACODE_24, ANAME_24, AVALUE_24, ACODE_25, ANAME_25, AVALUE_25, ACODE_26, ANAME_26, AVALUE_26, ACODE_27, ANAME_27, AVALUE_27, ACODE_28, ANAME_28, AVALUE_28, ACODE_29, ANAME_29, AVALUE_29, ACODE_30, ANAME_30, AVALUE_30) VALUES ";

                        string namaBrg = result.value.items[0].itemName;
                        string nama, nama2, nama3, urlImage, urlImage2, urlImage3;
                        urlImage = "";
                        urlImage2 = "";
                        urlImage3 = "";
                        if (namaBrg.Length > 30)
                        {
                            nama = namaBrg.Substring(0, 30);
                            if (namaBrg.Length > 285)
                            {
                                //change by calvin 15 januari 2019
                                //nama2 = namaBrg.Substring(30, 30);
                                //nama3 = (namaBrg.Length > 90) ? namaBrg.Substring(60, 30) : namaBrg.Substring(60);
                                nama2 = namaBrg.Substring(30, 255);
                                nama3 = "";
                                //end change by calvin 15 januari 2019
                            }
                            else
                            {
                                nama2 = namaBrg.Substring(30);
                                nama3 = "";
                            }
                        }
                        else
                        {
                            nama = namaBrg;
                            nama2 = "";
                            nama3 = "";
                        }
                        if (result.value.items[0].images != null)
                        {
                            urlImage = result.value.items[0].images[0].locationPath;
                            if (result.value.items[0].images.Count >= 2)
                            {
                                urlImage2 = result.value.items[0].images[1].locationPath;
                                if (result.value.items[0].images.Count >= 3)
                                {
                                    urlImage3 = result.value.items[0].images[2].locationPath;
                                }
                            }
                        }
                        //var attr = new Dictionary<string, string>();
                        //foreach (var property in result.value.attributes)
                        //{
                        //    attr.Add(property.attributeCode.ToString(), property.values[0].ToString());
                        //}

                        //add, check ada varian
                        int numVarian = 0;
                        bool insertParent = false;
                        string kdBrgInduk = "";
                        string sSQLInduk = ", ";
                        foreach (var property in result.value.attributes)
                        {
                            if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            {
                                numVarian++;
                            }
                        }
                        if (numVarian > 1)
                        {
                            //remove bussiness partner code from productsku -> max length < 20
                            string productSku = result.value.productSku;
                            var splitSku = productSku.Split('-');

                            kdBrgInduk = splitSku[splitSku.Length - 1] + ";" + result.value.productCode;
                            //cek brg induk di db
                            var brgIndukinDB = ErasoftDbContext.STF02H.Where(p => p.BRG_MP == kdBrgInduk && p.IDMARKET.ToString() == IdMarket).FirstOrDefault();
                            var tempBrgIndukinDB = ErasoftDbContext.TEMP_BRG_MP.Where(p => p.BRG_MP == kdBrgInduk && p.IDMARKET.ToString() == IdMarket).FirstOrDefault();
                            //var tempBrgIndukinDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == kdBrgInduk.ToUpper()).FirstOrDefault();
                            //var brgIndukinDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == kdBrgInduk.ToUpper()).FirstOrDefault();
                            if (brgIndukinDB == null && tempBrgIndukinDB == null)
                            {
                                insertParent = true;
                                sSQLInduk += sqlValueBrgInduk(result, kdBrgInduk, cust, IdMarket, display, urlImage, urlImage2, urlImage3);
                            }
                            else if (brgIndukinDB != null)
                            {
                                kdBrgInduk = brgIndukinDB.BRG;
                            }
                        }
                        //end add, check ada varian

                        string desc = result.value.description;
                        string categoryCode = result.value.categoryCode.ToString();
                        string merchantSku = result.value.items[0].merchantSku.ToString();
                        if (string.IsNullOrEmpty(merchantSku))
                            merchantSku = result.value.items[0].skuCode;
                        sSQL += "('" + productCode + ";" + result.value.items[0].skuCode + "' , '" + merchantSku.Replace('\'', '`') + "' , '" + nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' , '" + nama3.Replace('\'', '`') + "' ,";
                        sSQL += Convert.ToDouble(result.value.items[0].weight) * 1000 + "," + result.value.items[0].length + "," + result.value.items[0].width + "," + result.value.items[0].height + ", '";
                        sSQL += cust + "' , '" + desc.Replace('\'', '`') + "' , " + IdMarket + " , " + result.value.items[0].prices[0].price + " , " + result.value.items[0].prices[0].price;
                        sSQL += " , " + display + " , '" + categoryCode + "' , '" + result.value.categoryName + "' , '" + result.value.brand + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "'";
                        //add kode brg induk dan type brg
                        sSQL += ", '" + kdBrgInduk + "' , '3'";
                        //end add kode brg induk dan type brg

                        var attributeBlibli = MoDbContext.AttributeBlibli.Where(a => a.CATEGORY_CODE.Equals(categoryCode)).FirstOrDefault();
                        #region set attribute
                        if (attributeBlibli != null)
                        {
                            string attrVal = "";
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_1))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_1.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }

                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_1 + "' , '" + attributeBlibli.ANAME_1.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_2))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_2.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_2 + "' , '" + attributeBlibli.ANAME_2.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_3))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_3.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_3 + "' , '" + attributeBlibli.ANAME_3.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_4))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_4.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_4 + "' , '" + attributeBlibli.ANAME_4.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_5))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_5.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_5 + "' , '" + attributeBlibli.ANAME_5.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_6))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_6.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_6 + "' , '" + attributeBlibli.ANAME_6.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_7))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_7.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_7 + "' , '" + attributeBlibli.ANAME_7.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_8))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_8.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_8 + "' , '" + attributeBlibli.ANAME_8.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_9))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_9.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_9 + "' , '" + attributeBlibli.ANAME_9.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_10))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_10.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_10 + "' , '" + attributeBlibli.ANAME_10.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_11))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_11.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_11 + "' , '" + attributeBlibli.ANAME_11.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_12))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_12.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_12 + "' , '" + attributeBlibli.ANAME_12.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_13))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_13.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_13 + "' , '" + attributeBlibli.ANAME_13.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_14))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_14.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_14 + "' , '" + attributeBlibli.ANAME_14.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_15))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_15.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_15 + "' , '" + attributeBlibli.ANAME_15.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_16))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_16.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_16 + "' , '" + attributeBlibli.ANAME_16.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_17))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_17.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_17 + "' , '" + attributeBlibli.ANAME_17.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_18))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_18.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_18 + "' , '" + attributeBlibli.ANAME_18.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_19))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_19.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_19 + "' , '" + attributeBlibli.ANAME_19.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_20))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_20.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_20 + "' , '" + attributeBlibli.ANAME_20.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_21))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_21.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_21 + "' , '" + attributeBlibli.ANAME_21.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_22))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_22.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_22 + "' , '" + attributeBlibli.ANAME_22.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_23))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_23.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_23 + "' , '" + attributeBlibli.ANAME_23.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_24))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_24.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_24 + "' , '" + attributeBlibli.ANAME_24.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_25))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_25.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_25 + "' , '" + attributeBlibli.ANAME_25.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_26))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_26.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_26 + "' , '" + attributeBlibli.ANAME_26.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_27))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_27.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_27 + "' , '" + attributeBlibli.ANAME_27.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_28))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_28.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_28 + "' , '" + attributeBlibli.ANAME_28.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_29))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_29.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_29 + "' , '" + attributeBlibli.ANAME_29.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', ''";
                            }
                            if (!string.IsNullOrEmpty(attributeBlibli.ACODE_30))
                            {
                                foreach (var property in result.value.attributes)
                                {
                                    string tempCode = property.attributeCode.ToString();
                                    if (attributeBlibli.ACODE_30.ToUpper().Equals(tempCode.ToString()))
                                    {
                                        //if (!string.IsNullOrEmpty(attrVal))
                                        //    attrVal += ";";
                                        //attrVal += property.values[0].ToString();
                                        if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                                        {
                                            if (property.itemSku.ToString() == productCode)
                                            {
                                                attrVal = property.values[0].ToString();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(attrVal))
                                                attrVal += ";";
                                            attrVal += property.values[0].ToString();
                                        }
                                    }
                                }
                                sSQL += ", '" + attributeBlibli.ACODE_30 + "' , '" + attributeBlibli.ANAME_30.Replace("\'", "\'\'") + "' , '" + attrVal + "')";
                                attrVal = "";
                            }
                            else
                            {
                                sSQL += ", '', '', '')";
                            }
                        }
                        #endregion

                        if (insertParent)
                            sSQL += sSQLInduk;

                        var retExec = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                        ret.status = retExec;
                        //return ret;
                    }
                }

            }
            return ret;
        }
        public string sqlValueBrgInduk(dynamic result, string kdBrg, string cust, string IdMarket, int display, string urlImage, string urlImage2, string urlImage3)
        {
            string sSQL = "";
            string namaBrg = result.value.productName;
            string nama, nama2, nama3;

            if (namaBrg.Length > 30)
            {
                nama = namaBrg.Substring(0, 30);
                if (namaBrg.Length > 285)
                {
                    //change by calvin 15 januari 2019
                    //nama2 = namaBrg.Substring(30, 30);
                    //nama3 = (namaBrg.Length > 90) ? namaBrg.Substring(60, 30) : namaBrg.Substring(60);
                    nama2 = namaBrg.Substring(30, 255);
                    nama3 = "";
                    //end change by calvin 15 januari 2019
                }
                else
                {
                    nama2 = namaBrg.Substring(30);
                    nama3 = "";
                }
            }
            else
            {
                nama = namaBrg;
                nama2 = "";
                nama3 = "";
            }

            string desc = result.value.description;
            string categoryCode = result.value.categoryCode.ToString();
            //string merchantSku = result.value.items[0].merchantSku.ToString();
            sSQL += "('" + kdBrg + "' , '" + kdBrg + "' , '" + nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' , '" + nama3.Replace('\'', '`') + "' ,";
            sSQL += Convert.ToDouble(result.value.items[0].weight) * 1000 + "," + result.value.items[0].length + "," + result.value.items[0].width + "," + result.value.items[0].height + ", '";
            sSQL += cust + "' , '" + desc.Replace('\'', '`') + "' , " + IdMarket + " , " + result.value.items[0].prices[0].price + " , " + result.value.items[0].prices[0].price;
            sSQL += " , " + display + " , '" + categoryCode + "' , '" + result.value.categoryName + "' , '" + result.value.brand + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "'";
            //add kode brg induk dan type brg
            sSQL += ", '' , '4'";
            //end add kode brg induk dan type brg

            var attributeBlibli = MoDbContext.AttributeBlibli.Where(a => a.CATEGORY_CODE.Equals(categoryCode)).FirstOrDefault();
            #region set attribute
            if (attributeBlibli != null)
            {
                string attrVal = "";
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_1))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_1.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}

                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_1 + "' , '" + attributeBlibli.ANAME_1.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_2))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_2.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_2 + "' , '" + attributeBlibli.ANAME_2.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_3))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_3.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_3 + "' , '" + attributeBlibli.ANAME_3.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_4))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_4.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_4 + "' , '" + attributeBlibli.ANAME_4.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_5))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_5.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_5 + "' , '" + attributeBlibli.ANAME_5.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_6))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_6.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_6 + "' , '" + attributeBlibli.ANAME_6.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_7))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_7.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_7 + "' , '" + attributeBlibli.ANAME_7.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_8))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_8.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_8 + "' , '" + attributeBlibli.ANAME_8.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_9))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_9.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_9 + "' , '" + attributeBlibli.ANAME_9.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_10))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_10.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_10 + "' , '" + attributeBlibli.ANAME_10.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_11))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_11.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_11 + "' , '" + attributeBlibli.ANAME_11.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_12))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_12.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_12 + "' , '" + attributeBlibli.ANAME_12.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_13))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_13.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_13 + "' , '" + attributeBlibli.ANAME_13.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_14))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_14.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_14 + "' , '" + attributeBlibli.ANAME_14.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_15))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_15.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_15 + "' , '" + attributeBlibli.ANAME_15.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_16))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_16.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_16 + "' , '" + attributeBlibli.ANAME_16.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_17))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_17.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_17 + "' , '" + attributeBlibli.ANAME_17.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_18))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_18.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_18 + "' , '" + attributeBlibli.ANAME_18.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_19))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_19.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_19 + "' , '" + attributeBlibli.ANAME_19.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_20))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_20.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_20 + "' , '" + attributeBlibli.ANAME_20.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_21))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_21.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_21 + "' , '" + attributeBlibli.ANAME_21.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_22))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_22.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_22 + "' , '" + attributeBlibli.ANAME_22.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_23))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_23.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_23 + "' , '" + attributeBlibli.ANAME_23.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_24))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_24.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_24 + "' , '" + attributeBlibli.ANAME_24.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_25))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_25.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_25 + "' , '" + attributeBlibli.ANAME_25.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_26))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_26.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_26 + "' , '" + attributeBlibli.ANAME_26.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_27))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_27.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_27 + "' , '" + attributeBlibli.ANAME_27.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_28))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_28.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_28 + "' , '" + attributeBlibli.ANAME_28.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_29))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_29.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_29 + "' , '" + attributeBlibli.ANAME_29.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeBlibli.ACODE_30))
                {
                    foreach (var property in result.value.attributes)
                    {
                        string tempCode = property.attributeCode.ToString();
                        if (attributeBlibli.ACODE_30.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += property.values[0].ToString();
                            //if (!string.IsNullOrEmpty(property.itemSku.ToString()))
                            //{
                            //    if (property.itemSku.ToString() == productCode)
                            //    {
                            //        attrVal = property.values[0].ToString();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    if (!string.IsNullOrEmpty(attrVal))
                            //        attrVal += ";";
                            //    attrVal += property.values[0].ToString();
                            //}
                        }
                    }
                    sSQL += ", '" + attributeBlibli.ACODE_30 + "' , '" + attributeBlibli.ANAME_30.Replace("\'", "\'\'") + "' , '" + attrVal + "')";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', '')";
                }
            }
            #endregion


            return sSQL;
        }

        protected void prosesQueueFeedDetail(BlibliAPIData data, string requestId, string log_request_id)
        {
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant
            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/feed/detail", data.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/feed/detail?requestId=" + Uri.EscapeDataString(requestId) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline";

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

            if (responseFromServer != null && responseFromServer != "")
            {
                MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                {
                    REQUEST_ID = requestId
                };

                //dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                GetQueueFeedDetailResult result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(GetQueueFeedDetailResult)) as GetQueueFeedDetailResult;
                if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                {
                    if (result.value.queueHistory != null)
                    {
                        if (result.value.queueHistory.Count() > 0)
                        {
                            foreach (var item in result.value.queueHistory)
                            {
                                if (Convert.ToBoolean(item.isSuccess))
                                {
                                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);
                                    {
                                        if (Convert.ToString(result.value.queueFeed.requestAction) == "createProductV2")
                                        {
                                            string ProductCode = "";
                                            string gdnSku = "";
                                            if (result.value.queueHistory.Count() > 0)
                                            {
                                                gdnSku = result.value.queueHistory[0].gdnSku;
                                                ProductCode = result.value.queueHistory[0].value;
                                            }
                                                
                                            GetProdukInReviewList(data, requestId, ProductCode, gdnSku, log_request_id);
                                        }
                                    }
                                }
                                else
                                {
                                    using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                                    {
                                        oConnection.Open();
                                        using (SqlCommand oCommand = oConnection.CreateCommand())
                                        {
                                            try
                                            {
                                                oCommand.CommandType = CommandType.Text;
                                                oCommand.CommandText = "UPDATE [QUEUE_FEED_BLIBLI] SET [STATUS] = '2' WHERE [REQUESTID] = '" + requestId + "' AND [MERCHANT_CODE]=@MERCHANTCODE AND [STATUS] = '1'";
                                                oCommand.Parameters.Add(new SqlParameter("@MERCHANTCODE", SqlDbType.NVarChar, 10));
                                                oCommand.Parameters[0].Value = Convert.ToString(data.merchant_code);
                                                oCommand.ExecuteNonQuery();

                                                currentLog.REQUEST_RESULT = Convert.ToString(result.errorMessage);
                                                currentLog.REQUEST_EXCEPTION = "";
                                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                            }
                                            catch (Exception ex)
                                            {

                                            }

                                            try
                                            {
                                                if (Convert.ToString(result.value.queueFeed.requestAction) == "createProductV2")
                                                {
                                                    var getKodeItem = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == requestId).FirstOrDefault();
                                                    if (getKodeItem != null)
                                                    {
                                                        oCommand.CommandType = CommandType.Text;
                                                        oCommand.CommandText = "UPDATE H SET BRG_MP='' FROM STF02H H INNER JOIN ARF01 A ON H.IDMARKET = A.RECNUM WHERE H.BRG=@MERCHANTSKU AND A.SORT1_CUST=@MERCHANTCODE AND ISNULL(H.BRG_MP,'') = 'PENDING'";
                                                        oCommand.Parameters.Add(new SqlParameter("@MERCHANTSKU", SqlDbType.NVarChar, 20));
                                                        oCommand.Parameters[1].Value = Convert.ToString(getKodeItem.REQUEST_ATTRIBUTE_1);
                                                        oCommand.ExecuteNonQuery();
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            { }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public string GetCategoryPerUser(BlibliAPIData data)
        {

            string ret = "";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant

            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getCategory", data.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategory?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline";


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
                        //Data Source = 13.251.222.53; Initial Catalog = ERASOFT_rahmamk; Persist Security Info = True; User ID = sa; Password = admin123 ^
                        using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                        //using (SqlConnection oConnection = new SqlConnection("Data Source=13.251.222.53;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^"))
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

                                oCommand.CommandText = "UPDATE [ARF01] SET KODE=@CATEGORY_CODE WHERE Sort1_Cust = @MERCHANT_CODE";
                                oCommand.Parameters.Add(new SqlParameter("@MERCHANT_CODE", SqlDbType.NVarChar, 30));
                                oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar));
                                oCommand.Parameters[0].Value = data.merchant_code;
                                try
                                {
                                    string category_codes = "";
                                    //oCommand.Parameters[0].Value = data.merchant_code;
                                    foreach (var item in result.content) //foreach parent level top
                                    {
                                        category_codes += item.categoryCode.Value + ";";
                                    }
                                    category_codes = category_codes.Substring(0, category_codes.Length - 1);
                                    oCommand.Parameters[1].Value = category_codes;
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
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategory?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline";


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
                        //Data Source = 13.251.222.53; Initial Catalog = ERASOFT_rahmamk; Persist Security Info = True; User ID = sa; Password = admin123 ^
                        //using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
#if AWS
                        string con = "Data Source=localhost;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#elif Debug_AWS
                        string con = "Data Source=13.250.232.74;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#else
                        string con = "Data Source=13.251.222.53;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#endif

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
                string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&categoryCode=" + Uri.EscapeDataString(categoryCode) + "&channelId=MasterOnline";
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
                            bool insertAttribute = false;
#if AWS
                            string con = "Data Source=localhost;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#elif Debug_AWS
                            string con = "Data Source=13.250.232.74;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#else
                            string con = "Data Source=13.251.222.53;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#endif
                            using (SqlConnection oConnection = new SqlConnection(con))
                            {
                                oConnection.Open();
                                using (SqlCommand oCommand = oConnection.CreateCommand())
                                {
                                    var AttributeInDb = MoDbContext.AttributeBlibli.ToList();

                                    //cek jika sudah ada di database
                                    var cari = AttributeInDb.Where(p => p.CATEGORY_CODE.ToUpper().Equals(categoryCode.ToUpper())
                                    && p.CATEGORY_NAME.ToUpper().Equals(categoryName.ToUpper())
                                    ).ToList();
                                    //cek jika sudah ada di database

                                    if (cari.Count == 0)
                                    {
                                        insertAttribute = true;
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
                                }
                                if (insertAttribute)
                                {
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
                                                    if (Convert.ToString(result.value.attributes[i].attributeCode.Value) != "WA-0000002") // warna
                                                    {
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

                                            }
                                            catch (Exception ex)
                                            {

                                            }
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
            public int idmarket { get; set; }
        }
        public class BlibliQueueFeedData
        {
            public string request_id { get; set; }
            public string log_request_id { get; set; }

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
            //byte[] encodedBytes = Encoding.UTF8.GetBytes(input);
            //Encoding.Convert(Encoding.UTF8, Encoding.Unicode, encodedBytes);
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            //byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public class ListProductBlibli
        {
            public string requestId { get; set; }
            public object headers { get; set; }
            public string errorMessage { get; set; }
            public string errorCode { get; set; }
            public bool success { get; set; }
            public List<ContentBlibli> content { get; set; }
            public Pagemetadata pageMetaData { get; set; }
        }

        public class Pagemetadata
        {
            public int pageSize { get; set; }
            public int pageNumber { get; set; }
            public int totalRecords { get; set; }
        }

        public class ContentBlibli
        {
            public object id { get; set; }
            public object storeId { get; set; }
            public object createdDate { get; set; }
            public object createdBy { get; set; }
            public object updatedDate { get; set; }
            public object updatedBy { get; set; }
            public object version { get; set; }
            public string gdnSku { get; set; }
            public string productName { get; set; }
            public string productItemCode { get; set; }
            public string merchantSku { get; set; }
            public float regularPrice { get; set; }
            public float sellingPrice { get; set; }
            public int stockAvailableLv2 { get; set; }
            public int stockReservedLv2 { get; set; }
            public string productType { get; set; }
            public string pickupPointCode { get; set; }
            public string pickupPointName { get; set; }
            public bool displayable { get; set; }
            public bool buyable { get; set; }
            public string image { get; set; }
            public bool synchronizeStock { get; set; }
            public bool promoBundling { get; set; }
        }

        public class DetailBrgBlibli
        {
            public string requestId { get; set; }
            public object headers { get; set; }
            public object errorMessage { get; set; }
            public object errorCode { get; set; }
            public bool success { get; set; }
            public Value value { get; set; }
        }

        public class Value
        {
            public string id { get; set; }
            public string storeId { get; set; }
            public long createdDate { get; set; }
            public string createdBy { get; set; }
            public long updatedDate { get; set; }
            public string updatedBy { get; set; }
            public object version { get; set; }
            public string productSku { get; set; }
            public string productCode { get; set; }
            public string businessPartnerCode { get; set; }
            public bool synchronize { get; set; }
            public string productName { get; set; }
            public int productType { get; set; }
            public string categoryCode { get; set; }
            public string categoryName { get; set; }
            public string categoryHierarchy { get; set; }
            public string brand { get; set; }
            public string description { get; set; }
            public string specificationDetail { get; set; }
            public string uniqueSellingPoint { get; set; }
            public string productStory { get; set; }
            public Item[] items { get; set; }
            public Attribute[] attributes { get; set; }
            public Image1[] images { get; set; }
            public object url { get; set; }
            public bool installationRequired { get; set; }
            public string categoryId { get; set; }
        }

        public class Item
        {
            public string id { get; set; }
            public string storeId { get; set; }
            public long createdDate { get; set; }
            public string createdBy { get; set; }
            public long updatedDate { get; set; }
            public string updatedBy { get; set; }
            public object version { get; set; }
            public string itemSku { get; set; }
            public string skuCode { get; set; }
            public string merchantSku { get; set; }
            public string upcCode { get; set; }
            public string itemName { get; set; }
            public float length { get; set; }
            public float width { get; set; }
            public float height { get; set; }
            public float weight { get; set; }
            public float shippingWeight { get; set; }
            public int dangerousGoodsLevel { get; set; }
            public bool lateFulfillment { get; set; }
            public string pickupPointCode { get; set; }
            public string pickupPointName { get; set; }
            public int availableStockLevel1 { get; set; }
            public int reservedStockLevel1 { get; set; }
            public int availableStockLevel2 { get; set; }
            public int reservedStockLevel2 { get; set; }
            public int minimumStock { get; set; }
            public bool synchronizeStock { get; set; }
            public bool off2OnActiveFlag { get; set; }
            public object pristineId { get; set; }
            public Price[] prices { get; set; }
            public Viewconfig[] viewConfigs { get; set; }
            public Image[] images { get; set; }
            public object cogs { get; set; }
            public string cogsErrorCode { get; set; }
            public bool promoBundling { get; set; }
        }

        public class Price
        {
            public object id { get; set; }
            public object storeId { get; set; }
            public object createdDate { get; set; }
            public object createdBy { get; set; }
            public object updatedDate { get; set; }
            public object updatedBy { get; set; }
            public object version { get; set; }
            public string channelId { get; set; }
            public float price { get; set; }
            public float salePrice { get; set; }
            public object discountAmount { get; set; }
            public object discountStartDate { get; set; }
            public object discountEndDate { get; set; }
            public object promotionName { get; set; }
        }

        public class Viewconfig
        {
            public object id { get; set; }
            public object storeId { get; set; }
            public object createdDate { get; set; }
            public object createdBy { get; set; }
            public object updatedDate { get; set; }
            public object updatedBy { get; set; }
            public object version { get; set; }
            public string channelId { get; set; }
            public bool display { get; set; }
            public bool buyable { get; set; }
        }

        public class Image
        {
            public object id { get; set; }
            public object storeId { get; set; }
            public object createdDate { get; set; }
            public object createdBy { get; set; }
            public object updatedDate { get; set; }
            public object updatedBy { get; set; }
            public object version { get; set; }
            public bool mainImage { get; set; }
            public int sequence { get; set; }
            public string locationPath { get; set; }
        }

        public class Attribute
        {
            public object id { get; set; }
            public object storeId { get; set; }
            public object createdDate { get; set; }
            public object createdBy { get; set; }
            public object updatedDate { get; set; }
            public object updatedBy { get; set; }
            public object version { get; set; }
            public string attributeCode { get; set; }
            public string attributeType { get; set; }
            public string[] values { get; set; }
            public bool skuValue { get; set; }
            public string attributeName { get; set; }
            public string itemSku { get; set; }
        }

        public class Image1
        {
            public object id { get; set; }
            public object storeId { get; set; }
            public object createdDate { get; set; }
            public object createdBy { get; set; }
            public object updatedDate { get; set; }
            public object updatedBy { get; set; }
            public object version { get; set; }
            public bool mainImage { get; set; }
            public int sequence { get; set; }
            public string locationPath { get; set; }
        }

        public async Task<string> CreateProduct(BlibliAPIData iden, BlibliProductData data)
        {
            //if merchant code diisi. barulah upload produk
            string ret = "";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            //-productType / productHandlingType / productTypeCode
            //It's a product handling type, it's determine on how it will be shipped:

            //REGULAR: regular product, this type is handled by Blibli for specified merchantType. The shipping cost of Regular Product is covered by Blibli.
            //The ID number for REGULAR type = 1.
            //BIG_PRODUCT / HANDLING_BY_MERCHANT : shipped or handling by merchant, Blibli not covered the shipping cost for this product type.Ideally the big product category is like AC, refrigerator and other product that need instalment.Or maybe the electric voucher that sold by merchant by sending email or sms to customer can be included as this type.
            //The ID number for BIG_PRODUCT type = 2.
            //BOPIS : is (Buy Online Pickup In merchant Store).Customer that bought must came to merchant store to pick their product.
            //The ID number for BOPIS type = 3.

            CreateProductBlibliData newData = new CreateProductBlibliData()
            {
                name = data.nama,
                brand = data.Brand,
                url = "",
                categoryCode = data.CategoryCode,
                productType = 1,
                pickupPointCode = data.PickupPoint,
                length = Convert.ToInt32(data.Length),
                width = Convert.ToInt32(data.Width),
                height = Convert.ToInt32(data.Height),
                weight = Convert.ToInt32(data.berat),
                description = Convert.ToBase64String(Encoding.ASCII.GetBytes(data.Keterangan)),
                uniqueSellingPoint = Convert.ToBase64String(Encoding.ASCII.GetBytes(data.Keterangan)),
                productStory = Convert.ToBase64String(Encoding.ASCII.GetBytes(data.Keterangan)),
            };

            string sSQL = "SELECT * FROM (";
            for (int i = 1; i <= 30; i++)
            {
                sSQL += "SELECT B.ACODE_" + i.ToString() + " AS CATEGORY_CODE,B.ANAME_" + i.ToString() + " AS CATEGORY_NAME,B.ATYPE_" + i.ToString() + " AS CATEGORY_TYPE,A.AVALUE_" + i.ToString() + " AS VALUE FROM STF02H (NOLOCK) A INNER JOIN MO.DBO.ATTRIBUTE_BLIBLI (NOLOCK) B ON A.CATEGORY_CODE = B.CATEGORY_CODE WHERE A.BRG='" + data.kode + "' AND A.IDMARKET = '" + data.IDMarket + "' " + System.Environment.NewLine;
                if (i < 30)
                {
                    sSQL += "UNION ALL " + System.Environment.NewLine;
                }
            }

            DataSet dsFeature = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE <> 'DEFINING_ATTRIBUTE' ");
            DataSet dsVariasi = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE = 'DEFINING_ATTRIBUTE' ");

            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.merchant_code).FirstOrDefault();
            var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == data.kode).ToList();
            var var_stf02_listbrg = var_stf02.Select(p => p.BRG).ToList();
            var var_stf02h = ErasoftDbContext.STF02H.Where(p => var_stf02_listbrg.Contains(p.BRG) && p.IDMARKET == arf01.RecNum).ToList();
            var var_stf02i = ErasoftDbContext.STF02I.Where(p => p.BRG == data.kode && p.MARKET == "BLIBLI").ToList().OrderBy(p => p.RECNUM);

            Dictionary<string, string> nonDefiningAttributes = new Dictionary<string, string>();
            for (int i = 0; i < dsFeature.Tables[0].Rows.Count; i++)
            {
                if (!nonDefiningAttributes.ContainsKey(Convert.ToString(dsFeature.Tables[0].Rows[i]["CATEGORY_CODE"])))
                {
                    nonDefiningAttributes.Add(Convert.ToString(dsFeature.Tables[0].Rows[i]["CATEGORY_CODE"]), Convert.ToString(dsFeature.Tables[0].Rows[i]["VALUE"]).Trim());
                }
            }
            newData.productNonDefiningAttributes = nonDefiningAttributes;

            Dictionary<string, string[]> DefiningAttributes = new Dictionary<string, string[]>();
            for (int a = 0; a < dsVariasi.Tables[0].Rows.Count; a++)
            {
                List<string> dsVariasiValues = new List<string>();
                var var_stf02i_distinct = var_stf02i.Where(p => p.MP_JUDUL_VAR == Convert.ToString(dsVariasi.Tables[0].Rows[a]["CATEGORY_CODE"])).ToList().OrderBy(p => p.RECNUM);
                foreach (var v in var_stf02i_distinct)
                {
                    if (!dsVariasiValues.Contains(v.MP_VALUE_VAR))
                    {
                        dsVariasiValues.Add(v.MP_VALUE_VAR);
                    }
                }
                if (!DefiningAttributes.ContainsKey(Convert.ToString(dsVariasi.Tables[0].Rows[a]["CATEGORY_CODE"])))
                {
                    DefiningAttributes.Add(Convert.ToString(dsVariasi.Tables[0].Rows[a]["CATEGORY_CODE"]), dsVariasiValues.ToArray());
                }
            }
            newData.productDefiningAttributes = DefiningAttributes;

            Dictionary<string, string> images = new Dictionary<string, string>();
            List<string> uploadedImageID = new List<string>();
            List<Productitem> productItems = new List<Productitem>();
            foreach (var var_item in var_stf02)
            {
                var var_stf02h_item = var_stf02h.Where(p => p.BRG == var_item.BRG).FirstOrDefault();

                List<string> images_pervar = new List<string>();
                images_pervar.Add(var_item.Sort5); // size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                if (!uploadedImageID.Contains(var_item.Sort5))
                {
                    uploadedImageID.Add(var_item.Sort5);
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(var_item.LINK_GAMBAR_1);
                        images.Add(var_item.Sort5, Convert.ToBase64String(bytes));
                    }
                }

                Dictionary<string, string> attributeMap = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(var_item.Sort8))
                {
                    var var_stf02i_judul_mp_var_1 = var_stf02i.Where(p => p.KODE_VAR == var_item.Sort8 && p.LEVEL_VAR == 1).FirstOrDefault();
                    if (var_stf02i_judul_mp_var_1 != null)
                    {
                        attributeMap.Add(var_stf02i_judul_mp_var_1.MP_JUDUL_VAR, var_stf02i_judul_mp_var_1.MP_VALUE_VAR);
                    }
                    if (!string.IsNullOrWhiteSpace(var_item.Sort9))
                    {
                        var var_stf02i_judul_mp_var_2 = var_stf02i.Where(p => p.KODE_VAR == var_item.Sort9 && p.LEVEL_VAR == 2).FirstOrDefault();
                        if (var_stf02i_judul_mp_var_2 != null)
                        {
                            attributeMap.Add(var_stf02i_judul_mp_var_2.MP_JUDUL_VAR, var_stf02i_judul_mp_var_2.MP_VALUE_VAR);
                        }
                    }
                }

                Productitem newVarItem = new Productitem()
                {
                    upcCode = var_item.BRG,
                    merchantSku = var_item.BRG,
                    price = Convert.ToInt32(var_stf02h_item.HJUAL),
                    salePrice = Convert.ToInt32(var_stf02h_item.HJUAL),
                    minimumStock = Convert.ToInt32(var_item.MINI),
                    stock = Convert.ToInt32(var_item.MINI),
                    buyable = true,
                    displayable = true,
                    dangerousGoodsLevel = 0,
                    images = images_pervar.ToArray(),
                    attributesMap = attributeMap
                };
                productItems.Add(newVarItem);
            }
            newData.productItems = (productItems);
            newData.imageMap = images;

            string myData = JsonConvert.SerializeObject(newData);

            //myData = myData.Replace("\\r\\n", "\\n").Replace("–", "-").Replace("\\\"\\\"", "").Replace("×", "x");
            string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/createProduct", iden.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/createProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&username=" + Uri.EscapeDataString(userMTA) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&channelId=MasterOnline";

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Create Product",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = data.kode,
                REQUEST_ATTRIBUTE_2 = data.nama,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("bearer " + iden.token));
            myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            myReq.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
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
                currentLog.REQUEST_EXCEPTION = ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }
            if (responseFromServer != null)
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(CreateProductResult)) as CreateProductResult;
                if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
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
                            oCommand.CommandText = "INSERT INTO [QUEUE_FEED_BLIBLI] ([REQUESTID],[REQUEST_ACTION],[MERCHANT_CODE],[STATUS],[LOG_REQUEST_ID]) VALUES (@REQUESTID,'createProductV2',@MERCHANTCODE,'1',@LOG_REQUEST_ID)";
                            //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@REQUESTID", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@MERCHANTCODE", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@LOG_REQUEST_ID", SqlDbType.NVarChar, 50));

                            try
                            {
                                oCommand.Parameters[0].Value = result.value.queueFeedId;
                                oCommand.Parameters[1].Value = iden.merchant_code;
                                oCommand.Parameters[2].Value = currentLog.REQUEST_ID;

                                if (oCommand.ExecuteNonQuery() == 1)
                                {
                                    //ADD BY CALVIN 9 NOV 2018
                                    try
                                    {
                                        //SET BRG_MP JADI PENDING, AGAR TIDAK DOUBLE UPLOAD
                                        oCommand.CommandType = CommandType.Text;
                                        oCommand.CommandText = "UPDATE H SET BRG_MP='PENDING' FROM STF02H H INNER JOIN ARF01 A ON H.IDMARKET = A.RECNUM WHERE H.BRG=@MERCHANTSKU AND A.SORT1_CUST=@MERCHANTCODE AND ISNULL(H.BRG_MP,'') = ''";
                                        oCommand.Parameters.Add(new SqlParameter("@MERCHANTSKU", SqlDbType.NVarChar, 20));
                                        oCommand.Parameters[3].Value = Convert.ToString(data.kode);
                                        oCommand.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    //END ADD BY CALVIN 9 NOV 2018

                                    BlibliQueueFeedData queueData = new BlibliQueueFeedData
                                    {
                                        request_id = result.value.queueFeedId,
                                        log_request_id = currentLog.REQUEST_ID
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
                else
                {
                    currentLog.REQUEST_EXCEPTION = Convert.ToString(result.errorCode);
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                }
            }

            return ret;
        }

        public class CreateProductBlibliData
        {
            public string name { get; set; }
            public string brand { get; set; }
            public string url { get; set; }
            public string categoryCode { get; set; }
            public int productType { get; set; }
            public string pickupPointCode { get; set; }
            public int length { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public int weight { get; set; }
            public string description { get; set; }
            public string uniqueSellingPoint { get; set; }
            public string productStory { get; set; }
            public Dictionary<string, string> productNonDefiningAttributes { get; set; }
            public Dictionary<string, string[]> productDefiningAttributes { get; set; }
            public List<Productitem> productItems { get; set; }
            public Dictionary<string, string> imageMap { get; set; }
        }

        public class Productitem
        {
            public string upcCode { get; set; }
            public string merchantSku { get; set; }
            public int price { get; set; }
            public int salePrice { get; set; }
            public int stock { get; set; }
            public int minimumStock { get; set; }
            public bool displayable { get; set; }
            public bool buyable { get; set; }
            public string[] images { get; set; }
            public int dangerousGoodsLevel { get; set; }
            public Dictionary<string, string> attributesMap { get; set; }
        }


        public class GetQueueFeedDetailResult
        {
            public string requestId { get; set; }
            public object headers { get; set; }
            public object errorMessage { get; set; }
            public object errorCode { get; set; }
            public bool success { get; set; }
            public GetQueueFeedDetailResultValue value { get; set; }
        }

        public class GetQueueFeedDetailResultValue
        {
            public Queuefeed queueFeed { get; set; }
            public Queuehistory[] queueHistory { get; set; }
        }

        public class Queuefeed
        {
            public string requestId { get; set; }
            public string requestAction { get; set; }
            public int total { get; set; }
            public long timeStamp { get; set; }
        }

        public class Queuehistory
        {
            public string gdnSku { get; set; }
            public string value { get; set; }
            public long timestamp { get; set; }
            public bool isSuccess { get; set; }
        }


        public class ProductInReviewListResult
        {
            public string requestId { get; set; }
            public object headers { get; set; }
            public object errorMessage { get; set; }
            public object errorCode { get; set; }
            public bool success { get; set; }
            public ProductInReviewListResult_Content[] content { get; set; }
            public ProductInReviewListResult_Pagemetadata pageMetaData { get; set; }
        }

        public class ProductInReviewListResult_Pagemetadata
        {
            public int pageSize { get; set; }
            public int pageNumber { get; set; }
            public int totalRecords { get; set; }
        }

        public class ProductInReviewListResult_Content
        {
            public string productCode { get; set; }
            public string productName { get; set; }
            public string brand { get; set; }
            public float length { get; set; }
            public float width { get; set; }
            public float weight { get; set; }
            public float height { get; set; }
            public float shippingWeight { get; set; }
            public string url { get; set; }
            public string description { get; set; }
            public string uniqueSellingPoint { get; set; }
            public string productStory { get; set; }
            public string specificationDetail { get; set; }
            public ProductInReviewListResult_Productimage[] productImages { get; set; }
            public string categoryCode { get; set; }
            public string categoryName { get; set; }
            public bool activated { get; set; }
            public bool viewable { get; set; }
            public ProductInReviewListResult_Productitem[] productItems { get; set; }
        }

        public class ProductInReviewListResult_Productimage
        {
            public string imagePath { get; set; }
            public int sequence { get; set; }
            public bool active { get; set; }
            public bool mainImage { get; set; }
            public bool uploaded { get; set; }
        }

        public class ProductInReviewListResult_Productitem
        {
            public string generatedItemName { get; set; }
            public string upcCode { get; set; }
            public string productItemCode { get; set; }
        }


        public class BlibliGetOrder
        {
            public string requestId { get; set; }
            public object headers { get; set; }
            public object errorMessage { get; set; }
            public object errorCode { get; set; }
            public bool success { get; set; }
            public BlibliGetOrderContent[] content { get; set; }
            public BlibliGetOrderPagemetadata pageMetaData { get; set; }
        }

        public class BlibliGetOrderPagemetadata
        {
            public int pageSize { get; set; }
            public int pageNumber { get; set; }
            public int totalRecords { get; set; }
        }

        public class BlibliGetOrderContent
        {
            public string orderNo { get; set; }
            public string orderItemNo { get; set; }
            public int qty { get; set; }
            public long orderDate { get; set; }
            public string orderStatus { get; set; }
            public string orderStatusString { get; set; }
            public string customerFullName { get; set; }
            public string productName { get; set; }
            public float productPrice { get; set; }
            public string logisticService { get; set; }
            public string logisticProviderCode { get; set; }
            public long dueDate { get; set; }
            public string merchantDeliveryType { get; set; }
            public string logisticsOptionName { get; set; }
            public string logisticsOptionCode { get; set; }
            public string merchantSku { get; set; }
            public string productTypeCode { get; set; }
            public string productTypeName { get; set; }
            public string logisticsProductName { get; set; }
            public string pickupPointCode { get; set; }
            public string pickupPointName { get; set; }
            public string itemSku { get; set; }
            public string awbNumber { get; set; }
            public string awbStatus { get; set; }
            public bool paid { get; set; }
            public bool instantPickup { get; set; }
            public bool settlementCodeExpired { get; set; }
            public bool autoCancelWarning { get; set; }
            public long readyToProcessDate { get; set; }
            public string packageId { get; set; }
            public bool packageCreated { get; set; }
        }

        public class BlibliGetOrderDetail
        {
            public string requestId { get; set; }
            public object headers { get; set; }
            public object errorMessage { get; set; }
            public object errorCode { get; set; }
            public bool success { get; set; }
            public BlibliGetOrderDetailValue value { get; set; }
        }

        public class BlibliGetOrderDetailValue
        {
            public object id { get; set; }
            public string storeId { get; set; }
            public object createdDate { get; set; }
            public object createdBy { get; set; }
            public object updatedDate { get; set; }
            public object updatedBy { get; set; }
            public object version { get; set; }
            public string orderNo { get; set; }
            public string orderItemNo { get; set; }
            public int qty { get; set; }
            public long orderDate { get; set; }
            public long autoCancelDate { get; set; }
            public string productName { get; set; }
            public string productItemName { get; set; }
            public float productPrice { get; set; }
            public string gdnSku { get; set; }
            public string gdnItemSku { get; set; }
            public float totalWeight { get; set; }
            public string merchantSku { get; set; }
            public float total { get; set; }
            public string custName { get; set; }
            public string orderStatus { get; set; }
            public string orderStatusString { get; set; }
            public string customerAddress { get; set; }
            public string customerEmail { get; set; }
            public string logisticsService { get; set; }
            public string currentLogisticService { get; set; }
            public string pickupPoint { get; set; }
            public string pickupPointName { get; set; }
            public string pickupPointAddress { get; set; }
            public string pickupPointCity { get; set; }
            public string pickupPointProvince { get; set; }
            public string pickupPointCountry { get; set; }
            public string pickupPointZipcode { get; set; }
            public string merchantDeliveryType { get; set; }
            public bool installationRequired { get; set; }
            public object awbNumber { get; set; }
            public string awbStatus { get; set; }
            public string shippingStreetAddress { get; set; }
            public string shippingCity { get; set; }
            public string shippingSubDistrict { get; set; }
            public string shippingDistrict { get; set; }
            public string shippingProvince { get; set; }
            public string shippingZipCode { get; set; }
            public string shippingMobile { get; set; }
            public float shippingCost { get; set; }
            public float shippingInsuredAmount { get; set; }
            public object startOperationalTime { get; set; }
            public object endOperationalTime { get; set; }
            public object issuer { get; set; }
            public object refundResolution { get; set; }
            public object unFullFillReason { get; set; }
            public object unFullFillQuantity { get; set; }
            public string productTypeCode { get; set; }
            public string productTypeName { get; set; }
            public string custNote { get; set; }
            public string shippingRecipientName { get; set; }
            public string logisticsProductCode { get; set; }
            public string logisticsProductName { get; set; }
            public string logisticsOptionCode { get; set; }
            public float originLongitude { get; set; }
            public float originLatitude { get; set; }
            public float destinationLongitude { get; set; }
            public float destinationLatitude { get; set; }
            public float itemWeightInKg { get; set; }
            public object fulfillmentInfo { get; set; }
            public object settlementInfo { get; set; }
            public object financeSettlementInfo { get; set; }
            public bool instantPickup { get; set; }
            public object instantPickupDeadline { get; set; }
            public bool settlementCodeExpired { get; set; }
            public string onlineBookingId { get; set; }
            public string packageId { get; set; }
            public bool packageCreated { get; set; }
            public string logisticsOptionName { get; set; }
            public BlibliGetOrderDetailOrderhistory[] orderHistory { get; set; }
            public object manifestInfo { get; set; }
            public object manifest { get; set; }
        }

        public class BlibliGetOrderDetailOrderhistory
        {
            public string id { get; set; }
            public string storeId { get; set; }
            public long createdDate { get; set; }
            public string createdBy { get; set; }
            public long updatedDate { get; set; }
            public string updatedBy { get; set; }
            public object version { get; set; }
            public string orderStatus { get; set; }
            public string orderStatusDesc { get; set; }
            public long createdTimestamp { get; set; }
        }

        public class CreateProductResult
        {
            public string requestId { get; set; }
            public object headers { get; set; }
            public object errorMessage { get; set; }
            public object errorCode { get; set; }
            public bool success { get; set; }
            public CreateProductResultValue value { get; set; }
        }

        public class CreateProductResultValue
        {
            public string queueFeedId { get; set; }
            public string requestAction { get; set; }
            public int total { get; set; }
            public long timeStamp { get; set; }
        }

    }
}