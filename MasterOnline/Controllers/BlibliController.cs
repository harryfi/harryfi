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
                        arf01inDB.REFRESH_TOKEN = ret.refresh_token;

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
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderList?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&status=" + status;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Get Order List",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = stat.ToString(),
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
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    if (result.content.Count > 0)
                    {
                        foreach (var item in result.content)
                        {
                            await GetOrderDetail(iden, item.orderNo.Value, item.orderItemNo.Value, connId, CUST, NAMA_CUST);
                        }
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
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderDetail?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&orderNo=" + orderNo + "&orderItemNo=" + orderItemNo;

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
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
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

                                oCommand.Parameters["@orderNo"].Value = result.value.orderNo.Value;
                                oCommand.Parameters["@orderItemNo"].Value = result.value.orderItemNo.Value;
                                oCommand.Parameters["@qty"].Value = result.value.qty.Value;
                                oCommand.Parameters["@orderDate"].Value = result.value.orderDate.Value != null ? DateTimeOffset.FromUnixTimeMilliseconds(result.value.orderDate.Value).UtcDateTime.AddHours(7) : null;
                                oCommand.Parameters["@autoCancelDate"].Value = result.value.autoCancelDate.Value != null ? DateTimeOffset.FromUnixTimeMilliseconds(result.value.autoCancelDate.Value).UtcDateTime.AddHours(7) : null; ;

                                oCommand.Parameters["@productName"].Value = result.value.productName.Value;
                                oCommand.Parameters["@productItemName"].Value = result.value.productItemName.Value;
                                oCommand.Parameters["@productPrice"].Value = result.value.productPrice.Value;
                                oCommand.Parameters["@total"].Value = result.value.total.Value;
                                oCommand.Parameters["@itemWeightInKg"].Value = result.value.itemWeightInKg.Value;

                                oCommand.Parameters["@custName"].Value = result.value.custName.Value;
                                oCommand.Parameters["@orderStatus"].Value = result.value.orderStatus.Value != null ? result.value.orderStatus.Value : "";
                                oCommand.Parameters["@orderStatusString"].Value = result.value.orderStatusString.Value != null ? result.value.orderStatusString.Value : "";
                                oCommand.Parameters["@customerAddress"].Value = result.value.customerAddress.Value != null ? result.value.customerAddress.Value : "";
                                oCommand.Parameters["@customerEmail"].Value = result.value.customerEmail.Value != null ? result.value.customerEmail.Value : "";

                                oCommand.Parameters["@logisticsService"].Value = result.value.logisticsService.Value != null ? result.value.logisticsService.Value : "";
                                oCommand.Parameters["@currentLogisticService"].Value = result.value.currentLogisticService.Value != null ? result.value.currentLogisticService.Value : "";
                                oCommand.Parameters["@pickupPoint"].Value = result.value.pickupPoint.Value != null ? result.value.pickupPoint.Value : "";
                                oCommand.Parameters["@gdnSku"].Value = result.value.gdnSku.Value;
                                oCommand.Parameters["@gdnItemSku"].Value = result.value.gdnItemSku.Value;

                                oCommand.Parameters["@merchantSku"].Value = result.value.merchantSku.Value;
                                oCommand.Parameters["@totalWeight"].Value = result.value.totalWeight.Value;
                                oCommand.Parameters["@merchantDeliveryType"].Value = result.value.merchantDeliveryType.Value;
                                oCommand.Parameters["@awbNumber"].Value = result.value.awbNumber.Value != null ? result.value.awbNumber.Value : "";
                                oCommand.Parameters["@awbStatus"].Value = result.value.awbStatus.Value != null ? result.value.awbStatus.Value : "";

                                oCommand.Parameters["@shippingStreetAddress"].Value = result.value.shippingStreetAddress.Value;
                                oCommand.Parameters["@shippingCity"].Value = result.value.shippingCity.Value;
                                oCommand.Parameters["@shippingSubDistrict"].Value = result.value.shippingSubDistrict.Value;
                                oCommand.Parameters["@shippingDistrict"].Value = result.value.shippingSubDistrict.Value;
                                oCommand.Parameters["@shippingProvince"].Value = result.value.shippingProvince.Value;

                                oCommand.Parameters["@shippingZipCode"].Value = result.value.shippingZipCode.Value;
                                oCommand.Parameters["@shippingCost"].Value = result.value.shippingCost.Value;
                                oCommand.Parameters["@shippingMobile"].Value = result.value.shippingMobile.Value;
                                oCommand.Parameters["@shippingInsuredAmount"].Value = result.value.shippingInsuredAmount.Value;
                                oCommand.Parameters["@startOperationalTime"].Value = result.value.startOperationalTime.Value != null ? result.value.startOperationalTime.Value : "";

                                oCommand.Parameters["@endOperationalTime"].Value = result.value.endOperationalTime.Value != null ? result.value.endOperationalTime.Value : "";
                                oCommand.Parameters["@issuer"].Value = result.value.issuer.Value != null ? result.value.issuer.Value : "";
                                oCommand.Parameters["@refundResolution"].Value = result.value.refundResolution.Value != null ? result.value.refundResolution.Value : "";
                                oCommand.Parameters["@unFullFillReason"].Value = result.value.unFullFillReason.Value != null ? result.value.unFullFillReason.Value : "";
                                oCommand.Parameters["@unFullFillQuantity"].Value = result.value.unFullFillQuantity.Value != null ? result.value.unFullFillQuantity.Value : 0;

                                oCommand.Parameters["@productTypeCode"].Value = result.value.productTypeCode.Value != null ? result.value.productTypeCode.Value : "";
                                oCommand.Parameters["@productTypeName"].Value = result.value.productTypeName.Value != null ? result.value.productTypeName.Value : "";
                                oCommand.Parameters["@custNote"].Value = result.value.custNote.Value != null ? result.value.custNote.Value : "";
                                oCommand.Parameters["@shippingRecipientName"].Value = result.value.shippingRecipientName.Value != null ? result.value.shippingRecipientName.Value : "";
                                oCommand.Parameters["@logisticsProductCode"].Value = result.value.logisticsProductCode.Value != null ? result.value.logisticsProductCode.Value : "";

                                oCommand.Parameters["@logisticsProductName"].Value = result.value.logisticsProductName.Value != null ? result.value.logisticsProductName.Value : "";
                                oCommand.Parameters["@logisticsOptionCode"].Value = result.value.logisticsOptionCode.Value != null ? result.value.logisticsOptionCode.Value : "";
                                oCommand.Parameters["@logisticsOptionName"].Value = result.value.logisticsOptionName.Value != null ? result.value.logisticsOptionName.Value : "";
                                oCommand.Parameters["@destinationLongitude"].Value = result.value.destinationLongitude.Value != null ? result.value.destinationLongitude.Value : "";
                                oCommand.Parameters["@destinationLatitude"].Value = result.value.destinationLatitude.Value != null ? result.value.destinationLatitude.Value : "";

                                if (oCommand.ExecuteNonQuery() == 1)
                                {
                                    var connIdARF01C = Guid.NewGuid().ToString();

                                    var kabKot = "3174";
                                    var prov = "31";

                                    string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                    insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                    insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                                    insertPembeli += "('" + result.value.custName.Value + "','" + result.value.shippingStreetAddress.Value + "','" + result.value.shippingMobile.Value + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
                                    insertPembeli += "1, 'IDR', '01', '" + result.value.shippingStreetAddress.Value + "', 0, 0, 0, 0, '1', 0, 0, ";
                                    insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + result.value.shippingZipCode.Value + "', '" + result.value.customerEmail.Value + "', '" + kabKot + "', '" + prov + "', '', '','" + connIdARF01C + "')";
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
                    currentLog.REQUEST_RESULT = result.errorCode.Value;
                    currentLog.REQUEST_EXCEPTION = result.errorMessage.Value;
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
                    FileInfo fileInfo = new FileInfo(filePath);

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
                    FileStream fileStream = fileInfo.OpenRead();
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
        public void GetProdukInReviewList(BlibliAPIData iden)
        {
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/inProcessProduct", iden.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/inProcessProduct?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code);

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
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {

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
                myData += "\"price\": " + data.Price + ", ";                                                      //harga reguler (no diskon)
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
                            oCommand.CommandText = "INSERT INTO [QUEUE_FEED_BLIBLI] ([REQUESTID],[REQUEST_ACTION],[MERCHANT_CODE],[STATUS],[LOG_REQUEST_ID]) VALUES (@REQUESTID,'createProduct',@MERCHANTCODE,'1',@LOG_REQUEST_ID)";
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
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/fulfillRegular?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&storeId=10001";

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
                string urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getProductSummary?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code);
                urll_1 += "&size=100";
                if(!string.IsNullOrEmpty(data.nama))
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
                                        myData += "\"price\": " + data.Price + ", ";
                                        //myData += "\"salePrice\": " + data.MarketPrice + ", ";// harga yg tercantum di display blibli
                                        myData += "\"salePrice\": " + item.sellingPrice + ", ";// harga yg promo di blibli
                                        myData += "\"buyable\": " + data.display + ", ";
                                        myData += "\"display\": " + data.display + " "; // true=tampil    
                                        myData += "},";
                                    }
                                }
                            }
                            myData = myData.Remove(myData.Length - 1);
                            myData += "]";
                            myData += "}";

                            //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
                            string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/updateProduct", iden.API_secret_key);
                            //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/createProduct";
                            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/updateProduct";

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
                                dynamic result2 = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                                if (string.IsNullOrEmpty(result2.errorCode.Value))
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
        protected void getProduct(BlibliAPIData iden, string productCode)
        {
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getProductSummary", iden.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getProductSummary?requestId=" + Uri.EscapeDataString(Convert.ToString(milis)) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&gdnSku=" + Uri.EscapeDataString(productCode);

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
        }
        protected void getProductDetail(BlibliAPIData iden, string productCode)
        {
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/detailProduct", iden.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?requestId=" + Uri.EscapeDataString(Convert.ToString(milis)) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&gdnSku=" + Uri.EscapeDataString(productCode) + "&username=" + iden.API_client_username;

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
        }


        protected void prosesQueueFeedDetail(BlibliAPIData data, string requestId, string log_request_id)
        {
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant
            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/feed/detail", data.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/feed/detail?requestId=" + Uri.EscapeDataString(requestId) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code);

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
                MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                {
                    REQUEST_ID = log_request_id
                };

                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {
                    if (result.value.queueHistory != null)
                    {
                        if (result.value.queueHistory.Count > 0)
                        {
                            foreach (var item in result.value.queueHistory)
                            {
                                if (Convert.ToBoolean(item.isSuccess))
                                {
                                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);
                                    dynamic values = null;
                                    try
                                    {
                                        values = Newtonsoft.Json.JsonConvert.DeserializeObject(item.value.Value);
                                    }
                                    catch (Exception ex)
                                    {
                                        if (Convert.ToString(item.value.Value).Contains("postImage"))
                                        {
                                            using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                                            {
                                                oConnection.Open();
                                                using (SqlCommand oCommand = oConnection.CreateCommand())
                                                {
                                                    oCommand.CommandType = CommandType.Text;
                                                    oCommand.CommandText = "UPDATE [QUEUE_FEED_BLIBLI] SET [STATUS] = '0' WHERE [REQUESTID] = '" + requestId + "' AND [MERCHANT_CODE]=@MERCHANTCODE AND [STATUS] = '1'";
                                                    oCommand.Parameters.Add(new SqlParameter("@MERCHANTCODE", SqlDbType.NVarChar, 10));
                                                    oCommand.Parameters[0].Value = Convert.ToString(data.merchant_code);
                                                    oCommand.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                    if (values != null)
                                    {
                                        if (Convert.ToString(values.type) == "createProduct")
                                        {
                                            //SET BRG_MP
                                            using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                                            {
                                                oConnection.Open();
                                                using (SqlCommand oCommand = oConnection.CreateCommand())
                                                {
                                                    oCommand.CommandType = CommandType.Text;
                                                    oCommand.CommandText = "UPDATE H SET BRG_MP=@BRG_MP FROM STF02H H INNER JOIN ARF01 A ON H.IDMARKET = A.RECNUM WHERE H.BRG=@MERCHANTSKU AND A.SORT1_CUST=@MERCHANTCODE AND ISNULL(H.BRG_MP,'') = ''";
                                                    oCommand.Parameters.Add(new SqlParameter("@BRG_MP", SqlDbType.NVarChar, 50));
                                                    oCommand.Parameters.Add(new SqlParameter("@MERCHANTSKU", SqlDbType.NVarChar, 20));
                                                    oCommand.Parameters.Add(new SqlParameter("@MERCHANTCODE", SqlDbType.NVarChar, 10));

                                                    try
                                                    {
                                                        oCommand.Parameters[0].Value = Convert.ToString(values.gdnSku.Value) + ';' + Convert.ToString(values.productCode.Value);
                                                        oCommand.Parameters[1].Value = Convert.ToString(values.merchantSku.Value);
                                                        oCommand.Parameters[2].Value = Convert.ToString(data.merchant_code);
                                                        if (oCommand.ExecuteNonQuery() == 1)
                                                        {
                                                            oCommand.CommandType = CommandType.Text;
                                                            oCommand.CommandText = "UPDATE [QUEUE_FEED_BLIBLI] SET [STATUS] = '0' WHERE [REQUESTID] = '" + requestId + "' AND [MERCHANT_CODE]=@MERCHANTCODE AND [STATUS] = '1'";
                                                            if (oCommand.ExecuteNonQuery() == 1)
                                                            {
                                                                string[] imgPath = new string[3];
                                                                for (int i = 0; i < 3; i++)
                                                                {
                                                                    var namaFile = "FotoProduk-" + username + "-" + Convert.ToString(values.merchantSku.Value) + "-foto-" + Convert.ToString(i + 1) + ".jpg";
                                                                    //var path = Path.Combine(Server.MapPath("~/Content/Uploaded/"), namaFile);
                                                                    var path = Path.Combine(HttpRuntime.AppDomainAppPath, "Content\\Uploaded\\" + namaFile);
                                                                    if (System.IO.File.Exists(path))
                                                                    {
                                                                        imgPath[i] = path;
                                                                    }
                                                                }

                                                                UploadImage(data, imgPath, Convert.ToString(values.productCode.Value), Convert.ToString(values.merchantSku.Value));
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
                                else
                                {
                                    using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                                    {
                                        oConnection.Open();
                                        using (SqlCommand oCommand = oConnection.CreateCommand())
                                        {
                                            oCommand.CommandType = CommandType.Text;
                                            oCommand.CommandText = "UPDATE [QUEUE_FEED_BLIBLI] SET [STATUS] = '2' WHERE [REQUESTID] = '" + requestId + "' AND [MERCHANT_CODE]=@MERCHANTCODE AND [STATUS] = '1'";
                                            oCommand.Parameters.Add(new SqlParameter("@MERCHANTCODE", SqlDbType.NVarChar, 10));

                                            currentLog.REQUEST_RESULT = item.errorMessage.Value;
                                            currentLog.REQUEST_EXCEPTION = "";
                                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);

                                            try
                                            {
                                                oCommand.Parameters[0].Value = Convert.ToString(data.merchant_code);
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

    }
}