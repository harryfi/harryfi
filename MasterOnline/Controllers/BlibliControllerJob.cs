﻿using System;
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Server;

namespace MasterOnline.Controllers
{
    public class BlibliControllerJob : Controller
    {
        //AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);//string auth = Base64Encode();
        private MoDbContext MoDbContext { get; set; }
        private ErasoftContext ErasoftDbContext { get; set; }
        private DatabaseSQL EDB;
        private string username;
        private string DatabasePathErasoft;
        public BlibliControllerJob()
        {
            //Catatan by calvin :
            //beda antara BlibliController dengan BlibliControllerjob :
            //- controllerjob digunakan oleh hangfire
            //- semua variable global dideclare private
            //- context akan dideclare dengan function SetupContext
            //- semua method yang menggunakan hangfire, akan memiliki jobOptions, contoh [AutomaticRetry(Attempts = 3)],[Queue("get_token")]  di atas nama method
            //- sessionData tidak dipakai, karena hangfire tidak bisa mendapatkan nilai session
            //- manageAPI_LOG_MARKETPLACE tidak digunakan mulai dari tanggal 1 april 2019, hingga waktu yang belum ditentukan. 
            //- manageAPI_LOG_MARKETPLACE tetap digunakan di method GetProdukInReviewList karena createProductV2
            //- Method UploadImage dan UploadProduk sudah obsolete, dan diremark

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
        protected string SetupContext(BlibliAPIData data)
        {
            string ret = "";
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(data.DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, data.DatabasePathErasoft);
            username = data.username;
            DatabasePathErasoft = data.DatabasePathErasoft;

            var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.RecNum == data.idmarket).SingleOrDefault();
            if (arf01inDB != null)
            {
                ret = arf01inDB.TOKEN;

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
                if (TokenExpired && data.versiToken != "2")
                {
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
                        var retAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(BliBliToken)) as BliBliToken;
                        if (retAPI.error == null)
                        {
                            //var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.API_CLIENT_P.Equals(data.API_client_password) && p.API_CLIENT_U.Equals(data.API_client_username)).SingleOrDefault();
                            //if (arf01inDB != null)
                            //{
                            arf01inDB.TOKEN = retAPI.access_token;
                            arf01inDB.REFRESH_TOKEN = retAPI.refresh_token + ";" + Convert.ToString(retAPI.expires_in) + ";" + Convert.ToString(currentTimeRequest);

                            //ADD BY TRI, SET STATUS_API
                            arf01inDB.STATUS_API = "1";
                            //END ADD BY TRI, SET STATUS_API

                            ErasoftDbContext.SaveChanges();

                            ret = retAPI.access_token;
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

            }
            return ret;
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
        [AutomaticRetry(Attempts = 3)]
        [Queue("2_get_token")]
        public async Task<BliBliToken> GetToken(BlibliAPIData data, bool syncData, bool resetToken)//string API_client_username, string API_client_password, string API_secret_key, string email_merchant, string password_merchant)
        {
            var ret = new BliBliToken();
            var token = SetupContext(data);
            data.token = token;
            //change 29 mei 2020, api client username dan password akan memiliki value yg sama di semua akun blibli. diisi data MO
            //var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.API_CLIENT_P.Equals(data.API_client_password) && p.API_CLIENT_U.Equals(data.API_client_username) && !string.IsNullOrEmpty(p.Sort1_Cust)).SingleOrDefault();
            var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust.Equals(data.merchant_code) && !string.IsNullOrEmpty(p.Sort1_Cust) && p.NAMA == "16").SingleOrDefault();
            //end change 29 mei 2020, api client username dan password akan memiliki value yg sama di semua akun blibli. diisi data MO
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
                if (TokenExpired || resetToken)
                {
                    if (data.versiToken != "2")
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
                                    await GetPickupPoint(data); // untuk prompt pickup point saat insert barang
                                    await GetCategoryPerUser(data); // untuk category code yg muncul saat insert barang
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
                    else
                    {
                        await GetCategoryPerUser(data);
                    }
                }
                //await GetQueueFeedDetail(data, null);
            }
            return ret;
        }
        public async Task<string> GetPickupPoint(BlibliAPIData data)
        {
            string ret = "";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);// Jan1st1970.AddMilliseconds(Convert.ToDouble(milis)).AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getPickupPoint";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (data.versiToken != "2")
            {
                //string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/getPickupPoint", data.API_secret_key);
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getPickupPoint", data.API_secret_key);

                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getPickupPoint?businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline";

                //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                //{
                //    REQUEST_ID = milis.ToString(),
                //    REQUEST_ACTION = "Get List Pickup Point",
                //    REQUEST_DATETIME = milisBack,
                //    REQUEST_STATUS = "Pending",
                //};
                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + data.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = data.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = data.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getPickupPoint?businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", data.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020

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
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
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
                                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);
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
                    //currentLog.REQUEST_RESULT = result.errorCode.Value;
                    //currentLog.REQUEST_EXCEPTION = result.errorMessage.Value;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
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

        public async Task<string> CekOrder(BlibliAPIData iden, StatusOrder stat, string connId, string CUST, string NAMA_CUST)
        {
            //if merchant code diisi. barulah GetOrderList
            var token = SetupContext(iden);
            iden.token = token;

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
            //string filterstartdate = DateTime.UtcNow.AddDays(-28).ToString("yyyy-MM-dd HH:mm:ss");
            //string filterenddate = DateTime.UtcNow.AddDays(-25).ToString("yyyy-MM-dd HH:mm:ss");
            string filterstartdate = "2019-08-15 00:00:00";
            string filterenddate = "2019-08-15 23:59:59";
            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderList";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (iden.versiToken != "2")
            {
                //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/order/orderList", iden.API_secret_key);
                //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/createProduct";
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderList?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&status=" + status + "&channelId=MasterOnline&filterStartDate=" + Uri.EscapeDataString(filterstartdate) + "&filterEndDate=" + Uri.EscapeDataString(filterenddate);

                //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                //{
                //    REQUEST_ID = milis.ToString(),
                //    REQUEST_ACTION = "Get Order List",
                //    REQUEST_DATETIME = milisBack,
                //    REQUEST_ATTRIBUTE_1 = stat.ToString(),
                //    REQUEST_STATUS = "Pending",
                //};
                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderList?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&status=" + status + "&channelId=MasterOnline&filterStartDate=" + Uri.EscapeDataString(filterstartdate) + "&filterEndDate=" + Uri.EscapeDataString(filterenddate);

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020

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
            if (!string.IsNullOrWhiteSpace(responseFromServer))
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
                            var jmlhNewOrder = 0;//add by calvin 1 april 2019
                            foreach (var item in result.content)
                            {
                                if (!OrderNoInDb.Contains(item.orderNo))
                                {
                                    await GetOrderDetail(iden, item.orderNo, item.orderItemNo, connId, CUST, NAMA_CUST);
                                    jmlhNewOrder++;
                                }
                            }
                            ////add by calvin 1 april 2019
                            ////notify user
                            //if (jmlhNewOrder > 0)
                            //{
                            //    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                            //    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari Blibli.");

                            //    new StokControllerJob().updateStockMarketPlace(connId, iden.DatabasePathErasoft, iden.username);
                            //}
                            ////end add by calvin 1 april 2019
                        }
                        else
                        {
                            if (stat == StatusOrder.Completed)
                            {
                                //var jmlhSuccessOrder = 0;
                                foreach (var item in result.content)
                                {
                                    if (item.orderNo == "12034924873")
                                    {
                                        await GetOrderDetail(iden, item.orderNo, item.orderItemNo, connId, CUST, NAMA_CUST);
                                    }
                                    //    //remark by calvin 10 januari 2019, update saja, langsung ke sot01a, tidak usah getorderdetail lagi
                                    //    //await GetOrderDetail(iden, item.orderNo.Value, item.orderItemNo.Value, connId, CUST, NAMA_CUST);
                                    //    using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                                    //    {
                                    //        oConnection.Open();
                                    //        //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                                    //        //{
                                    //        using (SqlCommand oCommand = oConnection.CreateCommand())
                                    //        {
                                    //            oCommand.CommandType = CommandType.Text;
                                    //            oCommand.CommandText = "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI = '" + item.orderNo + "' AND STATUS_TRANSAKSI='03'";
                                    //            int affected = oCommand.ExecuteNonQuery();
                                    //            if (affected == 1)
                                    //            {
                                    //                jmlhSuccessOrder++;
                                    //            }
                                    //        }
                                    //    }
                                }
                                //if (jmlhSuccessOrder > 0)
                                //{
                                //    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                //    contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification(Convert.ToString(jmlhSuccessOrder) + " Pesanan dari Blibli sudah selesai.");
                                //}
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


        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrderList(BlibliAPIData iden, StatusOrder stat, string connId, string CUST, string NAMA_CUST)
        {
            string ret = "";
            if (!string.IsNullOrEmpty(iden.merchant_code))
            {
                var token = SetupContext(iden);
                iden.token = token;
                int page = 0;
                var more = true;

                //add 16 des 2020, fixed date
                var fromDt = DateTime.UtcNow.AddHours(7).AddDays(-5);
                if (stat == StatusOrder.Completed)
                {
                    fromDt = DateTime.UtcNow.AddHours(7).AddDays(-7);
                }
                //var toDt = DateTime.UtcNow.AddHours(14);
                var toDt = DateTime.UtcNow.AddHours(7);
                //end add 16 des 2020, fixed date

                //add by nurul 20/1/2021, bundling 
                var AdaKomponen = false;
                var connIdProses = "";
                List<string> tempConnId = new List<string>() { };
                //end add by nurul 20/1/2021, bundling 
                
                while (more)
                {
                    //int count = await GetOrderListWithPage(iden, stat, connId, CUST, NAMA_CUST, page, fromDt, toDt);
                    var count = await GetOrderListWithPage(iden, stat, connId, CUST, NAMA_CUST, page, fromDt, toDt);
                    page++;
                    //add by nurul 20/1/2021, bundling
                    if (connId != "")
                    {
                        tempConnId.Add(connId);
                        connIdProses += "'" + connId + "' , ";
                    }
                    if (count.AdaKomponen)
                    {
                        AdaKomponen = count.AdaKomponen;
                    }
                    //end add by nurul 20/1/2021, bundling
                    if (count.Count < 10)
                    {
                        more = false;
                    }
                }
                //add by nurul 20/1/2021, bundling 
                //List<string> listBrgKomponen = new List<string>();
                //if (tempConnId.Count() > 0)
                //{
                //    listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in (" + connIdProses.Substring(0, connIdProses.Length - 3) + ")").ToList();
                //}
                //if (listBrgKomponen.Count() > 0)
                if (!string.IsNullOrEmpty(connIdProses))
                {
                    new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username, connIdProses.Substring(0, connIdProses.Length - 3));
                }
                //end add by nurul 20/1/2021, bundling 


                // tunning untuk tidak duplicate
                var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + connId + "%' and invocationdata like '%blibli%' and invocationdata like '%GetOrderList%' and statename like '%Enque%' and invocationdata not like '%resi%'");
                // end tunning untuk tidak duplicate
            }


            return ret;
        }

        //public async Task<int> GetOrderListWithPage(BlibliAPIData iden, StatusOrder stat, string connId, string CUST, string NAMA_CUST, int page, DateTime fromDt, DateTime toDt)
        public async Task<tempOrderCount> GetOrderListWithPage(BlibliAPIData iden, StatusOrder stat, string connId, string CUST, string NAMA_CUST, int page, DateTime fromDt, DateTime toDt)
        {
            var ret = new tempOrderCount();
            int count = 0;
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";
            //add by nurul 10/12/2019, ubah startdate
            string startDate = "";
            //end change by nurul 10/12/2019, ubah startdate
            switch (stat)
            {
                case StatusOrder.Cancel:
                    //Cancel Order
                    status = "X";
                    break;
                case StatusOrder.Paid:
                    //paid
                    status = "FP";
                    //add by nurul 10/12/2019, ubah startdate
                    //startDate = Uri.EscapeDataString(DateTime.UtcNow.AddDays(-5).ToString("yyyy-MM-dd HH:mm:ss"));
                    startDate = Uri.EscapeDataString(fromDt.ToString("yyyy-MM-dd HH:mm:ss"));
                    //end add by nurul 10/12/2019, ubah startdate
                    //add by Tri 17 mar 2020, insert pesanan dengan status PF dan PU
                    status = "FP,PF,PU";
                    //end add by Tri 17 mar 2020, insert pesanan dengan status PF dan PU
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
                    //add by nurul 10/12/2019, ubah startdate
                    //startDate = Uri.EscapeDataString(DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss"));
                    startDate = Uri.EscapeDataString(fromDt.ToString("yyyy-MM-dd HH:mm:ss"));
                    //end add by nurul 10/12/2019, ubah startdate
                    break;
                default:
                    break;
            }
            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderList";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (iden.versiToken != "2")
            {
                //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/order/orderList", iden.API_secret_key);
                //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/createProduct";
                //change by nurul 10/12/2019, ubah startdate & enddate
                //string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderList?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&page=" + page.ToString() + "&size=10&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&status=" + status + "&channelId=MasterOnline&filterStartDate=" + Uri.EscapeDataString(DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss")) + "&filterEndDate=" + Uri.EscapeDataString(DateTime.UtcNow.AddHours(14).ToString("yyyy-MM-dd HH:mm:ss"));
                //urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderList?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&page=" + page.ToString() + "&size=10&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&status=" + status + "&channelId=MasterOnline&filterStartDate=" + startDate + "&filterEndDate=" + Uri.EscapeDataString(DateTime.UtcNow.AddHours(14).ToString("yyyy-MM-dd HH:mm:ss"));
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderList?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&page=" + page.ToString() + "&size=10&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&status=" + status + "&channelId=MasterOnline&filterStartDate=" + startDate + "&filterEndDate=" + Uri.EscapeDataString(toDt.ToString("yyyy-MM-dd HH:mm:ss"));
                //end change by nurul 10/12/2019, ubah startdate & enddate

                //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                //{
                //    REQUEST_ID = milis.ToString(),
                //    REQUEST_ACTION = "Get Order List",
                //    REQUEST_DATETIME = milisBack,
                //    REQUEST_ATTRIBUTE_1 = stat.ToString(),
                //    REQUEST_STATUS = "Pending",
                //};
                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                //urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderList?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&page=" + page.ToString() + "&size=10&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&status=" + status + "&channelId=MasterOnline&filterStartDate=" + startDate + "&filterEndDate=" + Uri.EscapeDataString(DateTime.UtcNow.AddHours(14).ToString("yyyy-MM-dd HH:mm:ss"));
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderList?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&page=" + page.ToString() + "&size=10&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&status=" + status + "&channelId=MasterOnline&filterStartDate=" + startDate + "&filterEndDate=" + Uri.EscapeDataString(toDt.ToString("yyyy-MM-dd HH:mm:ss"));

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020

            string responseFromServer = "";
            //try
            //{
                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
            //catch (Exception ex)
            //{
            //    //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}
            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(BlibliGetOrder)) as BlibliGetOrder;
                if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                {
                    count = result.content.Count();
                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    if (result.content.Count() > 0)
                    {
                        if (stat == StatusOrder.Paid)
                        {
                            var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();
                            var jmlhNewOrder = 0;//add by calvin 1 april 2019
                            foreach (var item in result.content)
                            {
                                if (!OrderNoInDb.Contains(item.orderNo))
                                {
                                    await GetOrderDetail(iden, item.orderNo, item.orderItemNo, connId, CUST, NAMA_CUST);
                                    jmlhNewOrder++;
                                }
                                else
                                {
                                    var GetNoBukti = ErasoftDbContext.SOT01A.Where(p => p.NO_REFERENSI == item.orderNo).Select(p => p.NO_BUKTI).First();
                                    var CekItemOrderNo = ErasoftDbContext.SOT01B.Where(p => p.NO_BUKTI == GetNoBukti && p.ORDER_ITEM_ID == item.orderItemNo).FirstOrDefault();

                                    if (CekItemOrderNo == null)
                                    {
                                        await GetOrderDetail(iden, item.orderNo, item.orderItemNo, connId, CUST, NAMA_CUST);
                                    }
                                }
                            }
                            //add by calvin 1 april 2019
                            //notify user
                            if (jmlhNewOrder > 0)
                            {
                                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari Blibli.");

                                //add by nurul 25/1/2021, bundling
                                var listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in ('" + connId + "')").ToList();
                                if (listBrgKomponen.Count() > 0)
                                {
                                    ret.AdaKomponen = true;
                                }
                                //end add by nurul 25/1/2021, bundling

                                new StokControllerJob().updateStockMarketPlace(connId, iden.DatabasePathErasoft, iden.username);
                            }
                            //end add by calvin 1 april 2019
                        }
                        else
                        {
                            if (stat == StatusOrder.Completed)
                            {
                                //add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                string noBuktiRef = "";
                                //end add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                var jmlhSuccessOrder = 0;
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
                                            int affected = oCommand.ExecuteNonQuery();
                                            if (affected == 1)
                                            {
                                                jmlhSuccessOrder++;
                                                noBuktiRef += "'" + item.orderNo + "' ,";
                                            }
                                        }
                                    }
                                }
                                if (jmlhSuccessOrder > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification(Convert.ToString(jmlhSuccessOrder) + " Pesanan dari Blibli sudah selesai.");

                                    //add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                    if (!string.IsNullOrEmpty(noBuktiRef))
                                    {
                                        var dateTimeNow = Convert.ToDateTime(DateTime.Now.AddHours(7).ToString("yyyy-MM-dd"));
                                        noBuktiRef = noBuktiRef.Substring(0, noBuktiRef.Length - 2) + ")";
                                        string sSQLUpdateDatePesananSelesai = "UPDATE SIT01A SET TGL_KIRIM = '" + dateTimeNow + "' WHERE NO_REF IN (" + noBuktiRef;
                                        var resultUpdateDatePesanan = EDB.ExecuteSQL("CString", CommandType.Text, sSQLUpdateDatePesananSelesai);
                                    }
                                    //end add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
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
                //return count;
            //}
            //catch (Exception ex)
            //{
            //    if(!ex.Message.ToLower().Contains("unauthorized"))
            //    if (!ex.Message.ToLower().Contains("many") && !ex.Message.ToLower().Contains("request"))
            //    {
            //        var log = new TABEL_LOG_GETORDERS()
            //        {
            //            DBPATHERA = iden.DatabasePathErasoft,
            //            MARKETPLACE = "BLIBLI",
            //            TGL = DateTime.UtcNow.AddHours(7),
            //            FUNCTION = "GetOrderListWithPage "+stat.ToString()+" : " + CUST,
            //            ERRORMSG = ex.Message
            //        };
            //        MoDbContext.TABEL_LOG_GETORDERS.Add(log);
            //        MoDbContext.SaveChanges();
            //        }
            //    throw ex;
            //}
            ret.Count = count;
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

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderDetail";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (iden.versiToken != "2")
            {
                //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/order/orderDetail", iden.API_secret_key);
                //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/order/orderDetail";
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderDetail?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&orderNo=" + orderNo + "&orderItemNo=" + orderItemNo + "&channelId=MasterOnline";

                //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                //{
                //    REQUEST_ID = milis.ToString(),
                //    REQUEST_ACTION = "Get Order Detail",
                //    REQUEST_DATETIME = milisBack,
                //    REQUEST_ATTRIBUTE_1 = orderNo,
                //    REQUEST_ATTRIBUTE_2 = orderItemNo,
                //    REQUEST_STATUS = "Pending",
                //};

                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderDetail?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&orderNo=" + orderNo + "&orderItemNo=" + orderItemNo + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020

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
                            //change 23 jun 2020
                            //oCommand.CommandText = "DELETE FROM [TEMP_BLI_ORDERDETAIL] WHERE CUST = '" + CUST + "'";
                            oCommand.CommandText = "DELETE FROM [TEMP_BLI_ORDERDETAIL]";
                            //end change 23 jun 2020
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
                            #region set param
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
                            #endregion
                            var nama = result.value.custName.Replace("'", "`");
                            if (nama.Length > 30)
                                nama = nama.Substring(0, 30);
                            #region cut max length dan ubah '
                            var order_no = !string.IsNullOrEmpty(result.value.orderNo) ? result.value.orderNo.Replace("'", "`") : "";
                            if (order_no.Length > 50)
                                order_no = order_no.Substring(0, 50);
                            var order_ItemNo = !string.IsNullOrEmpty(result.value.orderItemNo) ? result.value.orderItemNo.Replace("'", "`") : "";
                            if (order_ItemNo.Length > 50)
                                order_ItemNo = order_ItemNo.Substring(0, 50);
                            var orderStatus = !string.IsNullOrEmpty(result.value.orderStatus) ? result.value.orderStatus.Replace("'", "`") : "";
                            if (orderStatus.Length > 10)
                                orderStatus = orderStatus.Substring(0, 10);
                            var orderStatusString = !string.IsNullOrEmpty(result.value.orderStatusString) ? result.value.orderStatusString.Replace("'", "`") : "";
                            if (orderStatusString.Length > 250)
                                orderStatusString = orderStatusString.Substring(0, 250);
                            var customerEmail = !string.IsNullOrEmpty(result.value.customerEmail) ? result.value.customerEmail.Replace("'", "`") : "";
                            if (customerEmail.Length > 250)
                                customerEmail = customerEmail.Substring(0, 250);
                            var logisticsService = !string.IsNullOrEmpty(result.value.logisticsService) ? result.value.logisticsService.Replace("'", "`") : "";
                            if (logisticsService.Length > 250)
                                logisticsService = logisticsService.Substring(0, 250);
                            var currentLogisticService = !string.IsNullOrEmpty(result.value.currentLogisticService) ? result.value.currentLogisticService.Replace("'", "`") : "";
                            if (currentLogisticService.Length > 250)
                                currentLogisticService = currentLogisticService.Substring(0, 250);
                            var pickupPoint = !string.IsNullOrEmpty(result.value.pickupPoint) ? result.value.pickupPoint.Replace("'", "`") : "";
                            if (pickupPoint.Length > 50)
                                pickupPoint = pickupPoint.Substring(0, 50);
                            var gdnSku = !string.IsNullOrEmpty(result.value.gdnSku) ? result.value.gdnSku.Replace("'", "`") : "";
                            if (gdnSku.Length > 100)
                                gdnSku = gdnSku.Substring(0, 100);
                            var gdnItemSku = !string.IsNullOrEmpty(result.value.gdnItemSku) ? result.value.gdnItemSku.Replace("'", "`") : "";
                            if (gdnItemSku.Length > 100)
                                gdnItemSku = gdnItemSku.Substring(0, 100);
                            var merchantSku = !string.IsNullOrEmpty(result.value.merchantSku) ? result.value.merchantSku.Replace("'", "`") : "";
                            if (merchantSku.Length > 100)
                                merchantSku = merchantSku.Substring(0, 100);
                            var merchantDeliveryType = !string.IsNullOrEmpty(result.value.merchantDeliveryType) ? result.value.merchantDeliveryType.Replace("'", "`") : "";
                            if (merchantDeliveryType.Length > 50)
                                merchantDeliveryType = merchantDeliveryType.Substring(0, 50);
                            var awbNumber = !string.IsNullOrEmpty(result.value.awbNumber) ? result.value.awbNumber.Replace("'", "`") : "";
                            if (awbNumber.Length > 50)
                                awbNumber = awbNumber.Substring(0, 50);
                            var awbStatus = !string.IsNullOrEmpty(result.value.awbStatus) ? result.value.awbStatus.Replace("'", "`") : "";
                            if (awbStatus.Length > 50)
                                awbStatus = awbStatus.Substring(0, 50);
                            var shippingCity = !string.IsNullOrEmpty(result.value.shippingCity) ? result.value.shippingCity.Replace("'", "`") : "";
                            if (shippingCity.Length > 200)
                                shippingCity = shippingCity.Substring(0, 200);
                            var shippingSubDistrict = !string.IsNullOrEmpty(result.value.shippingSubDistrict) ? result.value.shippingSubDistrict.Replace("'", "`") : "";
                            if (shippingSubDistrict.Length > 200)
                                shippingSubDistrict = shippingSubDistrict.Substring(0, 200);
                            var shippingDistrict = !string.IsNullOrEmpty(result.value.shippingDistrict) ? result.value.shippingDistrict.Replace("'", "`") : "";
                            if (shippingDistrict.Length > 200)
                                shippingDistrict = shippingDistrict.Substring(0, 200);
                            var shippingProvince = !string.IsNullOrEmpty(result.value.shippingProvince) ? result.value.shippingProvince.Replace("'", "`") : "";
                            if (shippingProvince.Length > 200)
                                shippingProvince = shippingProvince.Substring(0, 200);
                            var shippingMobile = !string.IsNullOrEmpty(result.value.shippingMobile) ? result.value.shippingMobile.Replace("'", "`") : "";
                            if (shippingMobile.Length > 100)
                                shippingMobile = shippingMobile.Substring(0, 100);
                            //var startOperationalTime = !string.IsNullOrEmpty(result.value.startOperationalTime) ? result.value.startOperationalTime.Replace("'", "`") : "";
                            //if (startOperationalTime.Length > 200)
                            //    startOperationalTime = startOperationalTime.Substring(0, 200);
                            //var endOperationalTime = !string.IsNullOrEmpty(result.value.endOperationalTime) ? result.value.endOperationalTime.Replace("'", "`") : "";
                            //if (endOperationalTime.Length > 200)
                            //    endOperationalTime = endOperationalTime.Substring(0, 200);
                            //var issuer = !string.IsNullOrEmpty(result.value.issuer) ? result.value.issuer.Replace("'", "`") : "";
                            var issuer = !string.IsNullOrEmpty(result.value.orderType) ? result.value.orderType.Replace("'", "`") : "";
                            if (issuer.Length > 200)
                                issuer = issuer.Substring(0, 200);
                            var refundResolution = !string.IsNullOrEmpty(result.value.refundResolution) ? result.value.refundResolution.Replace("'", "`") : "";
                            if (refundResolution.Length > 200)
                                refundResolution = refundResolution.Substring(0, 200);
                            var productTypeCode = !string.IsNullOrEmpty(result.value.productTypeCode) ? result.value.productTypeCode.Replace("'", "`") : "";
                            if (productTypeCode.Length > 10)
                                productTypeCode = productTypeCode.Substring(0, 10);
                            var productTypeName = !string.IsNullOrEmpty(result.value.productTypeName) ? result.value.productTypeName.Replace("'", "`") : "";
                            if (productTypeName.Length > 200)
                                productTypeName = productTypeName.Substring(0, 200);
                            var shippingRecipientName = !string.IsNullOrEmpty(result.value.shippingRecipientName) ? result.value.shippingRecipientName.Replace("'", "`") : "";
                            if (shippingRecipientName.Length > 200)
                                shippingRecipientName = shippingRecipientName.Substring(0, 200);
                            var logisticsProductCode = !string.IsNullOrEmpty(result.value.logisticsProductCode) ? result.value.logisticsProductCode.Replace("'", "`") : "";
                            if (logisticsProductCode.Length > 50)
                                logisticsProductCode = logisticsProductCode.Substring(0, 50);
                            var logisticsProductName = !string.IsNullOrEmpty(result.value.logisticsProductName) ? result.value.logisticsProductName.Replace("'", "`") : "";
                            if (logisticsProductName.Length > 200)
                                logisticsProductName = logisticsProductName.Substring(0, 200);
                            var logisticsOptionCode = !string.IsNullOrEmpty(result.value.logisticsOptionCode) ? result.value.logisticsOptionCode.Replace("'", "`") : "";
                            if (logisticsOptionCode.Length > 50)
                                logisticsOptionCode = logisticsOptionCode.Substring(0, 50);
                            var logisticsOptionName = !string.IsNullOrEmpty(result.value.logisticsOptionName) ? result.value.logisticsOptionName.Replace("'", "`") : "";
                            if (logisticsOptionName.Length > 200)
                                logisticsOptionName = logisticsOptionName.Substring(0, 200);
                            if (NAMA_CUST.Length > 50)
                                NAMA_CUST = NAMA_CUST.Substring(0, 50);
                            var shippingZipCode = !string.IsNullOrEmpty(result.value.shippingZipCode) ? result.value.shippingZipCode.Replace("'", "`") : "";
                            if (shippingZipCode.Length > 7)// karena di arf01 max length = 7
                                shippingZipCode = shippingZipCode.Substring(0, 7);
                            #endregion
                            try
                            {
                                oCommand.Parameters[0].Value = CUST;
                                oCommand.Parameters[1].Value = NAMA_CUST;
                                oCommand.Parameters[2].Value = connId;

                                oCommand.Parameters["@orderNo"].Value = order_no;
                                oCommand.Parameters["@orderItemNo"].Value = order_ItemNo;
                                oCommand.Parameters["@qty"].Value = result.value.qty;
                                oCommand.Parameters["@orderDate"].Value = DateTimeOffset.FromUnixTimeMilliseconds(result.value.orderDate).UtcDateTime.AddHours(7);
                                //change by Tri 12 Mei 2020, autocanceldate bisa null
                                //oCommand.Parameters["@autoCancelDate"].Value = DateTimeOffset.FromUnixTimeMilliseconds(result.value.autoCancelDate).UtcDateTime.AddHours(7);
                                oCommand.Parameters["@autoCancelDate"].Value = DateTimeOffset.FromUnixTimeMilliseconds(result.value.autoCancelDate.HasValue ? result.value.autoCancelDate.Value : result.value.orderDate).UtcDateTime.AddHours(7);
                                //end change by Tri 12 Mei 2020, autocanceldate bisa null

                                oCommand.Parameters["@productName"].Value = !string.IsNullOrEmpty(result.value.productName) ? result.value.productName.Replace("'", "`") : "";
                                oCommand.Parameters["@productItemName"].Value = !string.IsNullOrEmpty(result.value.productItemName) ? result.value.productItemName.Replace("'", "`") : "";
                                //change by Tri 27 Mar 2020, gunakan final price
                                //oCommand.Parameters["@productPrice"].Value = result.value.productPrice;
                                //oCommand.Parameters["@total"].Value = result.value.total;
                                oCommand.Parameters["@productPrice"].Value = result.value.finalPrice;
                                oCommand.Parameters["@total"].Value = result.value.finalPriceTotal;
                                //end change by Tri 27 Mar 2020, gunakan final price
                                oCommand.Parameters["@itemWeightInKg"].Value = result.value.itemWeightInKg;

                                //oCommand.Parameters["@custName"].Value = result.value.custName;
                                oCommand.Parameters["@custName"].Value = nama;
                                //oCommand.Parameters["@orderStatus"].Value = result.value.orderStatus != null ? result.value.orderStatus : "";
                                var ordStatus = orderStatus;
                                if (ordStatus == "PF" || ordStatus == "PU")
                                {
                                    ordStatus = "FP";
                                }
                                oCommand.Parameters["@orderStatus"].Value = ordStatus;
                                oCommand.Parameters["@orderStatusString"].Value = orderStatusString;
                                oCommand.Parameters["@customerAddress"].Value = result.value.customerAddress != null ? result.value.customerAddress.Replace("'", "`") : "";
                                oCommand.Parameters["@customerEmail"].Value = customerEmail;

                                oCommand.Parameters["@logisticsService"].Value = logisticsService;
                                oCommand.Parameters["@currentLogisticService"].Value = currentLogisticService;
                                oCommand.Parameters["@pickupPoint"].Value = pickupPoint;
                                oCommand.Parameters["@gdnSku"].Value = gdnSku;
                                oCommand.Parameters["@gdnItemSku"].Value = gdnItemSku;

                                oCommand.Parameters["@merchantSku"].Value = merchantSku;
                                oCommand.Parameters["@totalWeight"].Value = result.value.totalWeight;
                                oCommand.Parameters["@merchantDeliveryType"].Value = merchantDeliveryType;
                                oCommand.Parameters["@awbNumber"].Value = awbNumber;
                                oCommand.Parameters["@awbStatus"].Value = awbStatus;

                                oCommand.Parameters["@shippingStreetAddress"].Value = !string.IsNullOrEmpty(result.value.shippingStreetAddress) ? result.value.shippingStreetAddress.Replace("'", "`") : "";
                                oCommand.Parameters["@shippingCity"].Value = shippingCity;
                                oCommand.Parameters["@shippingSubDistrict"].Value = shippingSubDistrict;
                                oCommand.Parameters["@shippingDistrict"].Value = shippingSubDistrict;
                                oCommand.Parameters["@shippingProvince"].Value = shippingProvince;

                                oCommand.Parameters["@shippingZipCode"].Value = shippingZipCode;
                                oCommand.Parameters["@shippingCost"].Value = result.value.shippingCost;
                                oCommand.Parameters["@shippingMobile"].Value = shippingMobile;
                                oCommand.Parameters["@shippingInsuredAmount"].Value = result.value.shippingInsuredAmount;
                                oCommand.Parameters["@startOperationalTime"].Value = result.value.startOperationalTime ?? 0;

                                oCommand.Parameters["@endOperationalTime"].Value = result.value.endOperationalTime ?? 0;
                                oCommand.Parameters["@issuer"].Value = issuer;
                                oCommand.Parameters["@refundResolution"].Value = refundResolution;
                                oCommand.Parameters["@unFullFillReason"].Value = !string.IsNullOrEmpty(result.value.unFullFillReason) ? result.value.unFullFillReason.Replace("'", "`") : "";
                                oCommand.Parameters["@unFullFillQuantity"].Value = result.value.unFullFillQuantity != null ? result.value.unFullFillQuantity : 0;

                                oCommand.Parameters["@productTypeCode"].Value = productTypeCode;
                                oCommand.Parameters["@productTypeName"].Value = productTypeName;
                                oCommand.Parameters["@custNote"].Value = result.value.custNote != null ? result.value.custNote.Replace("'", "`") : "";
                                oCommand.Parameters["@shippingRecipientName"].Value = shippingRecipientName;
                                oCommand.Parameters["@logisticsProductCode"].Value = logisticsProductCode;

                                oCommand.Parameters["@logisticsProductName"].Value = logisticsProductName;
                                oCommand.Parameters["@logisticsOptionCode"].Value = logisticsOptionCode;
                                oCommand.Parameters["@logisticsOptionName"].Value = logisticsOptionName;
                                oCommand.Parameters["@destinationLongitude"].Value = result.value.destinationLongitude ?? 0;
                                oCommand.Parameters["@destinationLatitude"].Value = result.value.destinationLatitude ?? 0;

                                if (oCommand.ExecuteNonQuery() == 1)
                                {
                                    var connIdARF01C = Guid.NewGuid().ToString();

                                    var kabKot = "3174";
                                    var prov = "31";
                                    //var nama = result.value.custName.Replace("'", "`");
                                    //if (nama.Length > 30)
                                    //    nama = nama.Substring(0, 30);


                                    string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                    insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                    insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                                    //insertPembeli += "('" + result.value.custName + "','" + result.value.shippingStreetAddress + "','" + result.value.shippingMobile + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
                                    insertPembeli += "('" + nama + "','" + result.value.shippingStreetAddress.Replace("'", "`") + "','" + shippingMobile + "','" + NAMA_CUST + "',0,0,'0','01',";
                                    insertPembeli += "1, 'IDR', '01', '" + result.value.shippingStreetAddress.Replace("'", "`") + "', 0, 0, 0, 0, '1', 0, 0, ";
                                    insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + shippingZipCode + "', '" + customerEmail + "', '" + kabKot + "', '" + prov + "', '', '','" + connIdARF01C + "')";
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
                                    CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                                    CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                                    CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                                    CommandSQL.Parameters.Add("@MARKET", SqlDbType.VarChar).Value = "";
                                    CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;
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
                                    //EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);

                                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                                }
                            }
                            catch (Exception ex)
                            {
                                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                            }
                        }
                    }
                }
                else
                {
                    //currentLog.REQUEST_RESULT = Convert.ToString(result.errorCode);
                    //currentLog.REQUEST_EXCEPTION = Convert.ToString(result.errorMessage);
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }
        //public async Task<string> UploadImage(BlibliAPIData iden, string[] imgPaths, string ProductCode, string merchantSku)
        //{
        //    string ret = "";
        //    long milis = CurrentTimeMillis();
        //    DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

        //    string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
        //    //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
        //    string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
        //    string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

        //    string signature = CreateToken("POST\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/postImage", iden.API_secret_key);
        //    string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/postImage";

        //    //string boundary = "WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
        //    string boundary = "WebKitFormBoundary7MA4YWxkTrZu0gW";
        //    string delimiter = "-------------" + boundary;

        //    //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
        //    //{
        //    //    REQUEST_ID = milis.ToString(),
        //    //    REQUEST_ACTION = "Upload Image",
        //    //    REQUEST_DATETIME = milisBack,
        //    //    REQUEST_ATTRIBUTE_1 = merchantSku,
        //    //    REQUEST_ATTRIBUTE_2 = ProductCode,
        //    //    REQUEST_STATUS = "Pending",
        //    //};

        //    //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

        //    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
        //    myReq.Method = "POST";
        //    myReq.Headers.Add("Authorization", ("bearer " + iden.token));
        //    myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
        //    myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
        //    myReq.Accept = "application/json";
        //    myReq.ContentType = "multipart/form-data; boundary=" + delimiter;
        //    myReq.Headers.Add("requestId", milis.ToString());
        //    myReq.Headers.Add("sessionId", milis.ToString());
        //    myReq.Headers.Add("username", userMTA);
        //    string[,] postFields = new string[,]
        //    {
        //        {"username",userMTA },
        //        {"merchantCode",iden.merchant_code },
        //        {"productCode",ProductCode }
        //    };

        //    Stream postDataStream = GetPostStream(postFields, imgPaths, boundary);

        //    string responseFromServer = "";
        //    try
        //    {
        //        myReq.ContentLength = postDataStream.Length;
        //        postDataStream.Position = 0;
        //        using (var dataStream = myReq.GetRequestStream())
        //        {
        //            byte[] buffer = new byte[1024];
        //            int bytesRead = 0;

        //            while ((bytesRead = postDataStream.Read(buffer, 0, buffer.Length)) != 0)
        //            {
        //                dataStream.Write(buffer, 0, bytesRead);
        //            }
        //        }
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
        //        //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
        //        //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
        //    }

        //    if (responseFromServer != null)
        //    {
        //        dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
        //        if (string.IsNullOrEmpty(result.errorCode.Value))
        //        {
        //            //INSERT QUEUE FEED
        //            using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
        //            {
        //                oConnection.Open();
        //                //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
        //                //{
        //                using (SqlCommand oCommand = oConnection.CreateCommand())
        //                {
        //                    //oCommand.CommandText = "DELETE FROM [CATEGORY_BLIBLI] WHERE ARF01_SORT1_CUST='" + data.merchant_code + "'";
        //                    //oCommand.ExecuteNonQuery();
        //                    //oCommand.Transaction = oTransaction;
        //                    oCommand.CommandType = CommandType.Text;
        //                    oCommand.CommandText = "INSERT INTO [QUEUE_FEED_BLIBLI] ([REQUESTID],[REQUEST_ACTION],[MERCHANT_CODE],[STATUS],[LOG_REQUEST_ID]) VALUES (@REQUESTID,'postImage',@MERCHANTCODE,'1',@LOG_REQUEST_ID)";
        //                    //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
        //                    oCommand.Parameters.Add(new SqlParameter("@REQUESTID", SqlDbType.NVarChar, 50));
        //                    oCommand.Parameters.Add(new SqlParameter("@MERCHANTCODE", SqlDbType.NVarChar, 50));
        //                    oCommand.Parameters.Add(new SqlParameter("@LOG_REQUEST_ID", SqlDbType.NVarChar, 50));

        //                    try
        //                    {
        //                        oCommand.Parameters[0].Value = result.requestId.Value;
        //                        oCommand.Parameters[1].Value = iden.merchant_code;
        //                        oCommand.Parameters[2].Value = currentLog.REQUEST_ID;

        //                        if (oCommand.ExecuteNonQuery() == 1)
        //                        {
        //                            BlibliQueueFeedData queueData = new BlibliQueueFeedData
        //                            {
        //                                request_id = result.requestId.Value,
        //                                log_request_id = currentLog.REQUEST_ID
        //                            };
        //                            await GetQueueFeedDetail(iden, queueData);
        //                        }
        //                        //oTransaction.Commit();
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        //oTransaction.Rollback();
        //                    }
        //                }
        //                //}
        //            }
        //        }
        //        else
        //        {
        //            //currentLog.REQUEST_RESULT = result.errorCode.Value;
        //            //currentLog.REQUEST_EXCEPTION = result.errorMessage.Value;
        //            //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
        //        }
        //    }
        //    return ret;
        //}
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
        public async Task<string> GetProdukInReviewList(BlibliAPIData iden, string requestID, string ProductCode, string gdnSku, string api_log_requestId)
        {
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/inProcessProduct";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (iden.versiToken != "2")
            {
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/inProcessProduct", iden.API_secret_key);
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/inProcessProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/inProcessProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
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
            if (responseFromServer != null)
            {
                //perlu tes item tanpa varian
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ProductInReviewListResult)) as ProductInReviewListResult;
                if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                {
                    foreach (var item in result.content)
                    {
                        if (item.productCode == ProductCode)
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
                                                oCommand.CommandText = "UPDATE [QUEUE_FEED_BLIBLI] SET [STATUS] = 0 WHERE REQUESTID = @REQUESTID ";
                                                oCommand.Parameters.Add(new SqlParameter("@REQUESTID", SqlDbType.NVarChar, 50));
                                                oCommand.Parameters[0].Value = requestID; // BRG MO
                                                oCommand.ExecuteNonQuery();
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
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
            return "";
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
        //public async Task<string> UploadProduk(BlibliAPIData iden, BlibliProductData data)
        //{
        //    //if merchant code diisi. barulah upload produk
        //    string ret = "";

        //    long milis = CurrentTimeMillis();
        //    DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

        //    string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
        //    //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
        //    string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
        //    string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

        //    string features = "";
        //    string variasi = "";
        //    string gambar = "";

        //    string sSQL = "SELECT * FROM (";
        //    for (int i = 1; i <= 30; i++)
        //    {
        //        sSQL += "SELECT A.ACODE_" + i.ToString() + " AS CATEGORY_CODE,A.ANAME_" + i.ToString() + " AS CATEGORY_NAME,B.ATYPE_" + i.ToString() + " AS CATEGORY_TYPE,A.AVALUE_" + i.ToString() + " AS VALUE FROM STF02H A INNER JOIN MO.DBO.ATTRIBUTE_BLIBLI B ON A.CATEGORY_CODE = B.CATEGORY_CODE WHERE A.BRG='" + data.kode + "' AND A.IDMARKET = '" + data.IDMarket + "' " + System.Environment.NewLine;
        //        if (i < 30)
        //        {
        //            sSQL += "UNION ALL " + System.Environment.NewLine;
        //        }
        //    }

        //    DataSet dsFeature = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE <> 'DEFINING_ATTRIBUTE' ");
        //    DataSet dsVariasi = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE = 'DEFINING_ATTRIBUTE' ");

        //    features += "{ \"name\": \"Brand\", \"value\": \"" + data.Brand + "\"}, ";
        //    List<Features> featuresList = new List<Features>();
        //    featuresList.Add(new Features
        //    {
        //        name = "Brand",
        //        value = data.Brand
        //    });
        //    for (int i = 0; i < dsFeature.Tables[0].Rows.Count; i++)
        //    {
        //        features += "{ \"name\": \"" + Convert.ToString(dsFeature.Tables[0].Rows[i]["CATEGORY_NAME"]) + "\", \"value\": \"" + Convert.ToString(dsFeature.Tables[0].Rows[i]["VALUE"]).Trim() + "\"},";
        //        featuresList.Add(new Features
        //        {
        //            name = Convert.ToString(dsFeature.Tables[0].Rows[i]["CATEGORY_NAME"]),
        //            value = Convert.ToString(dsFeature.Tables[0].Rows[i]["VALUE"]).Trim()
        //        });
        //    }
        //    List<Variasi> VariasiList = new List<Variasi>();
        //    for (int i = 0; i < dsVariasi.Tables[0].Rows.Count; i++)
        //    {
        //        string[] values = Convert.ToString(dsVariasi.Tables[0].Rows[i]["VALUE"]).Split(',');
        //        for (int a = 0; a < values.Length; a++)
        //        {
        //            VariasiList.Add(new Variasi
        //            {
        //                name = Convert.ToString(dsVariasi.Tables[0].Rows[i]["CATEGORY_NAME"]),
        //                value = Convert.ToString(values[a]).Trim()
        //            });
        //            variasi += "{\"name\": \"" + Convert.ToString(dsVariasi.Tables[0].Rows[i]["CATEGORY_NAME"]) + "\",\"value\": \"" + Convert.ToString(values[a]).Trim() + "\"},";
        //        }
        //    }
        //    List<imagess> ImagesList = new List<imagess>();
        //    for (int i = 0; i < 3; i++)
        //    {
        //        ImagesList.Add(new imagess
        //        {
        //            locationPath = data.Brand + "_" + data.nama + "_full0" + Convert.ToString(i + 1) + ".jpg",
        //            sequence = i
        //        });
        //    }
        //    List<UploadProdukNewDataProduct> products = new List<UploadProdukNewDataProduct>();
        //    products.Add(new UploadProdukNewDataProduct
        //    {
        //        merchantCode = iden.merchant_code,
        //        categoryCode = data.CategoryCode,
        //        productName = data.nama,
        //        url = "-",
        //        merchantSku = data.kode,
        //        tipePenanganan = 1,
        //        price = Convert.ToInt32(data.Price),
        //        salePrice = Convert.ToInt32(data.MarketPrice),
        //        stock = Convert.ToInt32(data.Qty),
        //        minimumStock = Convert.ToInt32(data.MinQty),
        //        pickupPointCode = data.PickupPoint,
        //        length = Convert.ToDouble(data.Length),
        //        width = Convert.ToDouble(data.Width),
        //        height = Convert.ToDouble(data.Height),
        //        weight = Convert.ToDouble(data.berat),
        //        desc = data.Keterangan,
        //        uniqueSellingPoint = data.Keterangan,
        //        productStory = data.Keterangan,
        //        upcCode = "-",
        //        display = data.display == "true" ? true : false,
        //        buyable = true,
        //        features = featuresList,
        //        variasi = VariasiList,
        //        images = ImagesList
        //    });
        //    UploadProdukNewData newData = new UploadProdukNewData
        //    {
        //        merchantCode = iden.merchant_code,
        //        products = products
        //    };
        //    string myData = "{";
        //    myData += "\"merchantCode\": \"" + iden.merchant_code + "\", ";
        //    myData += "\"products\": ";
        //    myData += "[{ ";  //MERCHANT ID ADA DI https://merchant.blibli.com/MTA/store-info/store-info
        //    {
        //        myData += "\"merchantCode\": \"" + iden.merchant_code + "\",  ";
        //        myData += "\"categoryCode\": \"" + data.CategoryCode + " \", ";                                       //LIHAT BAGIAN GETKATEGORI
        //        myData += "\"productName\": \"" + EscapeForJson(data.nama) + "\", ";                                 // NAMA PRODUK
        //        myData += "\"url\": \"\", ";                   // LINK URL IKLAN KALO ADA
        //        myData += "\"merchantSku\": \"" + EscapeForJson(data.kode) + "\", ";                                // SKU
        //        myData += "\"tipePenanganan\": 1, ";                                            // 1= reguler produk (dikirim oleh blili)| 2= dikirim oleh kurir | 3 =ambil sendiri di toko
        //        myData += "\"price\": " + data.MarketPrice + ", ";                                                      //harga reguler (no diskon)
        //        myData += "\"salePrice\": " + data.MarketPrice + ", ";                                                // harga yg tercantum di display blibli
        //        myData += "\"stock\": " + data.Qty + ", ";
        //        myData += "\"minimumStock\": " + data.MinQty + ", ";
        //        myData += "\"pickupPointCode\": \"" + data.PickupPoint + "\", ";                                   //pick up poin code, baca GetPickUp
        //        myData += "\"length\": " + data.Length + ", ";
        //        myData += "\"width\": " + data.Width + ", ";
        //        myData += "\"height\": " + data.Height + ", ";
        //        myData += "\"weight\": " + data.berat + ", "; // dalam gram, sama seperti MO
        //        myData += "\"desc\": \"" + EscapeForJson(data.Keterangan) + "\", ";
        //        myData += "\"uniqueSellingPoint\": \"" + EscapeForJson(data.Keterangan) + "\", "; //ex : Unique selling point of current product
        //        myData += "\"productStory\": \"" + EscapeForJson(data.Keterangan) + "\", "; //ex : This product is launched at 25 Des 2016, made in Indonesia
        //        myData += "\"upcCode\": \"\", "; //barcode, ex :1231230010
        //        myData += "\"display\": " + data.display + ", "; // true=tampil                
        //        myData += "\"buyable\": true, ";
        //        myData += "\"features\": [";
        //        //for (int i = 0; i < length; i++)
        //        //{
        //        //    features += "{ \"name\": \"Brand\", \"value\": \"" + data.Brand + "\"}, ";
        //        //}
        //        features = features.Substring(0, features.Length - 1);
        //        myData += features + "], ";
        //        myData += "\"variasi\": [";
        //        //for (int i = 0; i < length; i++)
        //        //{
        //        //    variasi += "{\"name\": \"Warna\",\"value\": \"Black\"},";
        //        //}
        //        variasi = variasi.Substring(0, variasi.Length - 1);
        //        myData += variasi + "], ";
        //        myData += "\"images\": [";
        //        for (int i = 0; i < 3; i++)
        //        {
        //            gambar += "{\"locationPath\": \"" + data.Brand + "_" + data.nama + "_full0" + Convert.ToString(i + 1) + ".jpg\",\"sequence\": " + Convert.ToString(i) + "},";
        //        }
        //        gambar = gambar.Substring(0, gambar.Length - 1);
        //        myData += gambar + "]";
        //    }
        //    myData += "}]";
        //    myData += "}";

        //    myData = JsonConvert.SerializeObject(newData);

        //    //myData = myData.Replace(System.Environment.NewLine, "\\r\\n");
        //    //myData = System.Text.RegularExpressions.Regex.Replace(myData, @"\r\n?|\n", "\\n");
        //    //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
        //    myData = myData.Replace("\\r\\n", "\\n").Replace("–", "-").Replace("\\\"\\\"", "").Replace("×", "x");
        //    string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
        //    //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/createProduct";
        //    string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/createProduct";

        //    //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
        //    //{
        //    //    REQUEST_ID = milis.ToString(),
        //    //    REQUEST_ACTION = "Create Product",
        //    //    REQUEST_DATETIME = milisBack,
        //    //    REQUEST_ATTRIBUTE_1 = data.kode,
        //    //    REQUEST_ATTRIBUTE_2 = data.nama,
        //    //    REQUEST_STATUS = "Pending",
        //    //};
        //    //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

        //    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
        //    myReq.Method = "POST";
        //    myReq.Headers.Add("Authorization", ("bearer " + iden.token));
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
        //        myReq.ContentLength = myData.Length;
        //        using (var dataStream = myReq.GetRequestStream())
        //        {
        //            dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
        //        }
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
        //        //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
        //        //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
        //    }
        //    if (responseFromServer != null)
        //    {
        //        dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
        //        if (string.IsNullOrEmpty(result.errorCode.Value))
        //        {
        //            //INSERT QUEUE FEED
        //            using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
        //            {
        //                oConnection.Open();
        //                //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
        //                //{
        //                using (SqlCommand oCommand = oConnection.CreateCommand())
        //                {
        //                    //oCommand.CommandText = "DELETE FROM [CATEGORY_BLIBLI] WHERE ARF01_SORT1_CUST='" + data.merchant_code + "'";
        //                    //oCommand.ExecuteNonQuery();
        //                    //oCommand.Transaction = oTransaction;
        //                    oCommand.CommandType = CommandType.Text;
        //                    oCommand.CommandText = "INSERT INTO [QUEUE_FEED_BLIBLI] ([REQUESTID],[REQUEST_ACTION],[MERCHANT_CODE],[STATUS],[LOG_REQUEST_ID]) VALUES (@REQUESTID,'createProductV2',@MERCHANTCODE,'1',@LOG_REQUEST_ID)";
        //                    //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
        //                    oCommand.Parameters.Add(new SqlParameter("@REQUESTID", SqlDbType.NVarChar, 50));
        //                    oCommand.Parameters.Add(new SqlParameter("@MERCHANTCODE", SqlDbType.NVarChar, 50));
        //                    oCommand.Parameters.Add(new SqlParameter("@LOG_REQUEST_ID", SqlDbType.NVarChar, 50));

        //                    try
        //                    {
        //                        oCommand.Parameters[0].Value = result.requestId.Value;
        //                        oCommand.Parameters[1].Value = iden.merchant_code;
        //                        oCommand.Parameters[2].Value = currentLog.REQUEST_ID;

        //                        if (oCommand.ExecuteNonQuery() == 1)
        //                        {
        //                            //ADD BY CALVIN 9 NOV 2018
        //                            try
        //                            {
        //                                //SET BRG_MP JADI PENDING, AGAR TIDAK DOUBLE UPLOAD
        //                                oCommand.CommandType = CommandType.Text;
        //                                oCommand.CommandText = "UPDATE H SET BRG_MP='PENDING' FROM STF02H H INNER JOIN ARF01 A ON H.IDMARKET = A.RECNUM WHERE H.BRG=@MERCHANTSKU AND A.SORT1_CUST=@MERCHANTCODE AND ISNULL(H.BRG_MP,'') = ''";
        //                                oCommand.Parameters.Add(new SqlParameter("@MERCHANTSKU", SqlDbType.NVarChar, 20));
        //                                oCommand.Parameters[3].Value = Convert.ToString(data.kode);
        //                                oCommand.ExecuteNonQuery();
        //                            }
        //                            catch (Exception ex)
        //                            {

        //                            }
        //                            //END ADD BY CALVIN 9 NOV 2018

        //                            BlibliQueueFeedData queueData = new BlibliQueueFeedData
        //                            {
        //                                request_id = result.requestId.Value,
        //                                log_request_id = currentLog.REQUEST_ID
        //                            };
        //                            await GetQueueFeedDetail(iden, queueData);
        //                        }
        //                        //oTransaction.Commit();
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        //oTransaction.Rollback();
        //                    }
        //                }
        //                //}
        //            }
        //        }
        //        else
        //        {
        //            //currentLog.REQUEST_EXCEPTION = result.errorCode.Value;
        //            //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
        //        }
        //    }

        //    return ret;
        //}

        protected string EscapeForJson(string s)
        {
            //string quoted = System.Web.Helpers.Json.Encode(s.Replace("–", "-").Replace("\"\"", "''"));
            //return quoted.Substring(1, quoted.Length - 2);
            string quoted = Newtonsoft.Json.JsonConvert.ToString(s);
            return quoted.Substring(1, quoted.Length - 2);
        }

        public class createPackageData
        {
            public List<string> orderItemIds { get; set; }
        }
        public class fillOrderAWBData
        {
            public int type { get; set; }
            public string awbNo { get; set; }
            public string orderNo { get; set; }
            public string orderItemNo { get; set; }
            public List<fillOrderAWBCombineShipping> combineShipping { get; set; }
        }
        public class fillOrderAWBDataV2
        {
            public string awbNo { get; set; }
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
        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Konfirmasi Pengiriman Pesanan {obj} ke Blibli Gagal.")]
        public void fillOrderAWB(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, BlibliAPIData iden, string awbNo, string orderNo, string orderItemNo)
        {
            long milis = CurrentTimeMillis();
            var token = SetupContext(iden);
            iden.token = token;
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

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

            string myData = JsonConvert.SerializeObject(thisData);

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = milis.ToString(),
            //    REQUEST_ACTION = "Update No Resi",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = orderNo,
            //    REQUEST_ATTRIBUTE_2 = orderItemNo,
            //    REQUEST_ATTRIBUTE_3 = awbNo,
            //    REQUEST_STATUS = "Pending",
            //};

            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/fulfillRegular";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (iden.versiToken != "2")
            {
                //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
                string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/order/fulfillRegular", iden.API_secret_key);
                //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/order/fulfillRegular";
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/fulfillRegular?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&storeId=10001" + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "POST";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/fulfillRegular?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&storeId=10001" + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "POST";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            string responseFromServer = "";
            //try
            //{
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
            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}
            if (responseFromServer != "")
            {
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {
                    EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM = '2' WHERE CUST = '" + log_CUST + "' AND NO_REFERENSI = '" + orderNo + "'");
                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                }
                else
                {
                    EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM = '1' WHERE CUST = '" + log_CUST + "' AND NO_REFERENSI = '" + orderNo + "'");
                    throw new Exception(result.errorMessage.Value);
                    //currentLog.REQUEST_RESULT = result.errorCode.Value;
                    //currentLog.REQUEST_EXCEPTION = result.errorMessage.Value;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                }
            }
        }

        //add by nurul 18/12/2020
        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Konfirmasi Pengiriman Pesanan {obj} ke Blibli Gagal.")]
        public void fillOrderAWBV2(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, BlibliAPIData iden, string awbNo, string orderNo, string orderItemNo)
        {
            long milis = CurrentTimeMillis();
            var token = SetupContext(iden);
            iden.token = token;
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            List<fillOrderAWBCombineShipping> combineShipping = new List<fillOrderAWBCombineShipping>();
            combineShipping.Add(new fillOrderAWBCombineShipping
            {
                orderNo = orderNo,
                orderItemNo = orderItemNo
            });
            
            fillOrderAWBDataV2 thisData = new fillOrderAWBDataV2();
            thisData.awbNo = awbNo;

            string myData = JsonConvert.SerializeObject(thisData);

            string urll = "https://api.blibli.com/v2/proxy/seller/v1/orders/regular/" + awbNo + "/fulfill?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            
            if (iden.versiToken != "2")
            {
                string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/order/fulfillRegular", iden.API_secret_key);
                urll = "https://api.blibli.com/v2/proxy/seller/v1/orders/regular/" + awbNo + "/fulfill?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&storeId=10001" + "&channelId=MasterOnline&username=" + Uri.EscapeDataString(iden.mta_username_email_merchant) + "&storeCode=" + Uri.EscapeDataString(iden.merchant_code);

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "POST";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/seller/v1/orders/regular/" + awbNo + "/fulfill?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&storeId=10001" + "&channelId=MasterOnline&username=" + Uri.EscapeDataString(iden.mta_username_email_merchant) + "&storeCode=" + Uri.EscapeDataString(iden.merchant_code);

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "POST";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
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
            catch (WebException ex)
            {
                string err1 = "";
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp1 = ex.Response;
                    using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                    {
                        err1 = sr1.ReadToEnd();
                        responseFromServer = err1;
                    }
                }
                //throw new Exception(err1);
            }
            if (responseFromServer != "")
            {
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (responseFromServer == "{}")
                {
                    EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM = '2' WHERE CUST = '" + log_CUST + "' AND NO_REFERENSI = '" + orderNo + "'");
                }
                else
                {
                    if (string.IsNullOrEmpty(result.errorCode.Value))
                    {
                        EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM = '2' WHERE CUST = '" + log_CUST + "' AND NO_REFERENSI = '" + orderNo + "'");
                    }
                    else
                    {
                        EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM = '1' WHERE CUST = '" + log_CUST + "' AND NO_REFERENSI = '" + orderNo + "'");
                        throw new Exception(result.errorMessage.Value);
                    }
                }
            }
            else
            {
                EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM = '2' WHERE CUST = '" + log_CUST + "' AND NO_REFERENSI = '" + orderNo + "'");
            }
        }
        //end add by nurul 18/12/2020

        public async Task<string> UpdateProdukQOH_Display(BlibliAPIData iden, BlibliProductData data)
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
            string signature_1 = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/detailProduct", iden.API_secret_key);
            string[] brg_mp = data.kode_mp.Split(';');
            //change by Tri 21 Feb 2020, brg mp bisa tidak ada product code ny
            //if (brg_mp.Length == 2)
            if (brg_mp.Length >= 1)
            //end change by Tri 21 Feb 2020, brg mp bisa tidak ada product code ny
            {
                //add by nurul 13/7/2020
                string urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct";
                HttpWebRequest myReq_1 = (HttpWebRequest)WebRequest.Create(urll_1);
                //end add by nurul 13/7/2020

                //change by nurul 13/7/2020
                if (iden.versiToken != "2")
                {
                    urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&gdnSku=" + Uri.EscapeDataString(brg_mp[0]) + "&channelId=MasterOnline";

                    myReq_1 = (HttpWebRequest)WebRequest.Create(urll_1);
                    myReq_1.Method = "GET";
                    myReq_1.Headers.Add("Authorization", ("bearer " + iden.token));
                    myReq_1.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature_1));
                    myReq_1.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                    myReq_1.Accept = "application/json";
                    myReq_1.ContentType = "application/json";
                    myReq_1.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
                    myReq_1.Headers.Add("sessionId", milis.ToString());
                    myReq_1.Headers.Add("username", userMTA);
                }
                else
                {
                    string usernameMO = iden.API_client_username;
                    //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                    string passMO = iden.API_client_password;
                    urll_1 = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&gdnSku=" + Uri.EscapeDataString(brg_mp[0]) + "&channelId=MasterOnline";

                    myReq_1 = (HttpWebRequest)WebRequest.Create(urll_1);
                    myReq_1.Method = "GET";
                    myReq_1.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                    myReq_1.Accept = "application/json";
                    myReq_1.ContentType = "application/json";
                    myReq_1.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                    myReq_1.Headers.Add("Signature-Time", milis.ToString());
                }
                string responseFromServer_1 = "";
                try
                {
                    using (WebResponse response = await myReq_1.GetResponseAsync())
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
                    BlibliDetailProductResult result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer_1, typeof(BlibliDetailProductResult)) as BlibliDetailProductResult;
                    if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                    {
                        if (result.value.items.Count() > 0)
                        {
                            string myData = "{";
                            myData += "\"merchantCode\": \"" + iden.merchant_code + "\", ";
                            myData += "\"productRequests\": ";
                            myData += "[ ";  //MERCHANT ID ADA DI https://merchant.blibli.com/MTA/store-info/store-info
                            {
                                if (result.value.items.Count() > 0)
                                {
                                    QOHBlibli = result.value.items[0].availableStockLevel2;
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
                                        myData += "\"gdnSku\": \"" + brg_mp[0] + "\",  ";
                                        myData += "\"stock\": " + Convert.ToString(QOHBlibli) + ", ";
                                        myData += "\"minimumStock\": " + data.MinQty + ", ";
                                        myData += "\"price\": " + data.Price + ", ";
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

                            //add by nurul 13/7/2020
                            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct";
                            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                            //end add by nurul 13/7/2020

                            //change by nurul 13/7/2020
                            if (iden.versiToken != "2")
                            {
                                string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/updateProduct", iden.API_secret_key);
                                //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/createProduct";
                                //string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/updateProduct";

                                //add by calvin 15 nov 2018
                                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/updateProduct?channelId=MasterOnline";
                                //end add by calvin 15 nov 2018

                                //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                                //{
                                //    REQUEST_ID = milis.ToString(),
                                //    REQUEST_ACTION = "Update QOH dan Display",
                                //    REQUEST_DATETIME = milisBack,
                                //    REQUEST_ATTRIBUTE_1 = data.kode,
                                //    REQUEST_ATTRIBUTE_2 = brg_mp[1], //product_code
                                //    REQUEST_ATTRIBUTE_3 = brg_mp[0], //gdnsku
                                //    REQUEST_STATUS = "Pending",
                                //};
                                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

                                myReq = (HttpWebRequest)WebRequest.Create(urll);
                                myReq.Method = "POST";
                                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                                myReq.Accept = "application/json";
                                myReq.ContentType = "application/json";
                                myReq.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
                                myReq.Headers.Add("sessionId", milis.ToString());
                                myReq.Headers.Add("username", userMTA);
                            }
                            else
                            {
                                string usernameMO = iden.API_client_username;
                                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                                string passMO = iden.API_client_password;
                                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/updateProduct?channelId=MasterOnline";

                                myReq = (HttpWebRequest)WebRequest.Create(urll);
                                myReq.Method = "POST";
                                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                                myReq.Accept = "application/json";
                                myReq.ContentType = "application/json";
                                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                                myReq.Headers.Add("Signature-Time", milis.ToString());
                            }

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
                            }
                            catch (Exception ex)
                            {
                                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                            }
                            if (responseFromServer != null)
                            {
                                dynamic result2 = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                                if (string.IsNullOrEmpty(result2.errorCode.Value))
                                {
                                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);

                                    //remark by calvin 2 april 2019
                                    //BlibliQueueFeedData queueData = new BlibliQueueFeedData
                                    //{
                                    //    request_id = result2.requestId.Value,
                                    //    log_request_id = currentLog.REQUEST_ID
                                    //};
                                    //await GetQueueFeedDetail(iden, queueData);
                                    //end remark by calvin 2 april 2019
                                }
                                else
                                {
                                    //currentLog.REQUEST_RESULT = Convert.ToString(result.errorCode);
                                    //currentLog.REQUEST_EXCEPTION = Convert.ToString(result.errorMessage);
                                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Harga Jual Produk {obj} ke Blibli gagal.")]
        public async Task<string> UpdateProdukQOH_Display_Job(string dbPathEra, string kdbrgMO, string log_CUST, string log_ActionCategory, string log_ActionName, string product_id, BlibliAPIData iden, BlibliProductData data)
        {
            SetupContext(iden);
            //if merchant code diisi. barulah upload produk
            string ret = "";
            StokControllerJob stokAPI = new StokControllerJob(dbPathEra, iden.username);
            
            //change by nurul 19/1/2022
            //var qtyOnHand = stokAPI.GetQOHSTF08A(data.kode, "ALL");
            var multilokasi = ErasoftDbContext.SIFSYS_TAMBAHAN.FirstOrDefault().MULTILOKASI;
            double qtyOnHand = 0;
            if (multilokasi == "1")
            {
                qtyOnHand = stokAPI.GetQOHSTF08A_MultiLokasi(data.kode, "ALL", log_CUST);
            }
            else
            {
                qtyOnHand = stokAPI.GetQOHSTF08A(data.kode, "ALL");
            }
            //end change by nurul 19/1/2022

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant


            #region Get Product List ( untuk dapatkan QOH di Blibi )
            double QOHBlibli = 0;
            //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
            string signature_1 = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/detailProduct", iden.API_secret_key);
            string[] brg_mp = data.kode_mp.Split(';');
            //change by Tri 21 Feb 2020, brg mp bisa tidak ada product code ny
            //if (brg_mp.Length == 2)
            if (brg_mp.Length >= 1)
            //end change by Tri 21 Feb 2020, brg mp bisa tidak ada product code ny
            {
                string myData = "{";
                myData += "\"merchantCode\": \"" + iden.merchant_code + "\", ";
                myData += "\"productRequests\": ";
                myData += "[ ";  //MERCHANT ID ADA DI https://merchant.blibli.com/MTA/store-info/store-info
                {
                    {
                        myData += "{";
                        myData += "\"gdnSku\": \"" + brg_mp[0] + "\",  ";
                        myData += "\"stock\": null, ";
                        myData += "\"minimumStock\": null, ";
                        myData += "\"price\": " + data.Price + ", ";
                        myData += "\"salePrice\": " + data.MarketPrice + " ";// harga yg tercantum di display blibli
                                                                              //myData += "\"salePrice\": " + item.sellingPrice + ", ";// harga yg promo di blibli
                        //myData += "\"buyable\": " + data.display + ", ";
                        //myData += "\"buyable\": " + (qtyOnHand > 0 ? data.display : "false") + ", ";
                        //myData += "\"displayable\": " + data.display + " "; // true=tampil    
                        myData += "},";
                    }
                }
                myData = myData.Remove(myData.Length - 1);
                myData += "]";
                myData += "}";

                //add by nurul 13/7/2020
                string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/updateProduct?channelId=MasterOnline";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                //end add by nurul 13/7/2020

                //change by nurul 13/7/2020
                if (iden.versiToken != "2")
                {
                    string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/updateProduct", iden.API_secret_key);
                    //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/createProduct";
                    //string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/updateProduct";

                    //add by calvin 15 nov 2018
                    urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/updateProduct?channelId=MasterOnline";
                    //end add by calvin 15 nov 2018

                    myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "POST";
                    myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                    myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                    myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                    myReq.Accept = "application/json";
                    myReq.ContentType = "application/json";
                    myReq.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
                    myReq.Headers.Add("sessionId", milis.ToString());
                    myReq.Headers.Add("username", userMTA);
                }
                else
                {
                    string usernameMO = iden.API_client_username;
                    //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                    string passMO = iden.API_client_password;
                    urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/updateProduct?channelId=MasterOnline";

                    myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "POST";
                    myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                    myReq.Accept = "application/json";
                    myReq.ContentType = "application/json";
                    myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                    myReq.Headers.Add("Signature-Time", milis.ToString());
                }
                //end change by nurul 13/7/2020

                MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                {
                    REQUEST_ID = milis.ToString(),
                    REQUEST_ACTION = "Update QOH dan Display",
                    REQUEST_DATETIME = milisBack,
                    REQUEST_ATTRIBUTE_1 = data.kode,
                    //REQUEST_ATTRIBUTE_2 = brg_mp[1], //product_code
                    REQUEST_ATTRIBUTE_2 = data.kode_mp, //link barang dari fix barang notfound tidak mendapat product code
                    REQUEST_ATTRIBUTE_3 = brg_mp[0], //gdnsku
                    REQUEST_STATUS = "Pending",
                };
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

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
                }
                catch (WebException e)
                {
                    string err = "";
                    //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        WebResponse resp = e.Response;
                        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                        {
                            err = sr.ReadToEnd();
                        }
                        var response = e.Response as HttpWebResponse;
                        var status = (int)response.StatusCode;
                        if (status == 429)
                        {
                            if (string.IsNullOrEmpty(data.berat))
                            {
                                data.berat = "0";
                            }
                            var loop = Convert.ToInt32(data.berat);
                            if (loop < 2)
                            {
                                await Task.Delay(60000);
                                data.berat = (loop + 1).ToString();
                                await UpdateProdukQOH_Display_Job(dbPathEra, kdbrgMO, log_CUST, log_ActionCategory, log_ActionName, product_id, iden, data);
                            }
                            else
                            {
                                currentLog.REQUEST_EXCEPTION = err;
                                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                                throw new Exception(err);
                            }
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = err;
                            manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                            throw new Exception(err);
                        }
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = e.Message;
                        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                        throw new Exception(e.Message);
                    }
                }
                //catch (Exception ex)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //    throw new Exception(currentLog.REQUEST_EXCEPTION);
                //}
                if (responseFromServer != "")
                {
                    dynamic result2 = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                    if (string.IsNullOrEmpty(result2.errorCode.Value))
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
                                    currentLog.CUST_ATTRIBUTE_2 = (Convert.ToInt32(currentProgress[0]) + 1).ToString();
                                    currentLog.CUST_ATTRIBUTE_3 = currentProgress[1];
                                }
                            }
                        }
                        //end add 19 sept 2020, update harga massal
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);

                        BlibliQueueFeedData queueData = new BlibliQueueFeedData
                        {
                            request_id = result2.requestId.Value,
                            log_request_id = currentLog.REQUEST_ID
                        };
                        await GetQueueFeedDetail(iden, queueData);
                    }
                    else
                    {
                        currentLog.REQUEST_RESULT = Convert.ToString(result2.errorCode);
                        currentLog.REQUEST_EXCEPTION = Convert.ToString(result2.errorMessage);
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        throw new Exception(currentLog.REQUEST_EXCEPTION);
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

        [AutomaticRetry(Attempts = 1)]
        [Queue("3_general")]
        public async Task<string> GetQueueFeedDetail(BlibliAPIData data, BlibliQueueFeedData feed)
        {
            string ret = "";

            var token = SetupContext(data);
            data.token = token;

            if (feed != null)//satu requestId
            {
                await prosesQueueFeedDetail(data, feed.request_id, feed.log_request_id);
            }
            else
            {
                DataSet dsRequestIdList = new DataSet();
                dsRequestIdList = EDB.GetDataSet("sCon", "QUEUE_FEED_BLIBLI", "SELECT * FROM [QUEUE_FEED_BLIBLI] WHERE MERCHANT_CODE='" + data.merchant_code + "' AND [STATUS] = '1'");
                if (dsRequestIdList.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsRequestIdList.Tables[0].Rows.Count; i++)
                    {
                        await prosesQueueFeedDetail(data, Convert.ToString(dsRequestIdList.Tables[0].Rows[i]["REQUESTID"]), Convert.ToString(dsRequestIdList.Tables[0].Rows[i]["LOG_REQUEST_ID"]));
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

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = milis.ToString(),
            //    REQUEST_ACTION = "Get Item List",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = iden.merchant_code,
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/feed/detail?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (iden.versiToken != "2")
            {
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getProductSummary", iden.API_secret_key);
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getProductSummary?requestId=" + Uri.EscapeDataString(Convert.ToString(milis)) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + (string.IsNullOrEmpty(productCode) ? "" : "&gdnSku=" + Uri.EscapeDataString(productCode));
                urll += "&page=" + page + "&size=10";
                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getProductSummary?requestId=" + Uri.EscapeDataString(Convert.ToString(milis)) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + (string.IsNullOrEmpty(productCode) ? "" : "&gdnSku=" + Uri.EscapeDataString(productCode));
                urll += "&page=" + page + "&size=10";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020

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
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ListProductBlibli)) as ListProductBlibli;
                if (listBrg != null)
                {
                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
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
                                    //var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == (item.gdnSku + ";" + item.productItemCode).ToUpper()).FirstOrDefault();
                                    var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper().Contains(item.productItemCode.ToUpper())).FirstOrDefault();
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
                                //currentLog.REQUEST_EXCEPTION = ret.message;
                                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                            }
                        }
                        else
                        {
                            ret.message = "Gagal mendapatkan produk";
                            //currentLog.REQUEST_EXCEPTION = ret.message;
                            //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                        }
                    }
                    else
                    {
                        ret.message = listBrg.errorMessage;
                        //currentLog.REQUEST_EXCEPTION = ret.message;
                        //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                    }
                }
                else
                {
                    ret.message = "Gagal mendapatkan produk";
                    //currentLog.REQUEST_EXCEPTION = ret.message;
                    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            else
            {
                ret.message = "Failed to get response from Blibli API";
                //currentLog.REQUEST_EXCEPTION = ret.message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
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

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/feed/detail?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (iden.versiToken != "2")
            {
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/detailProduct", iden.API_secret_key);
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?requestId=" + Uri.EscapeDataString(Convert.ToString(milis)) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&gdnSku=" + Uri.EscapeDataString(productCode) + "&username=" + iden.API_client_username + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/detailProduct?requestId=" + Uri.EscapeDataString(Convert.ToString(milis)) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&gdnSku=" + Uri.EscapeDataString(productCode) + "&username=" + iden.API_client_username + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020

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
                            string prdCd = result.value.productCode;
                            kdBrgInduk = splitSku[splitSku.Length - 1] + ";" + result.value.productCode;
                            //cek brg induk di db
                            var brgIndukinDB = ErasoftDbContext.STF02H.Where(p => p.BRG_MP.Contains(prdCd) && p.IDMARKET.ToString() == IdMarket).FirstOrDefault();
                            //var brgIndukinDB = ErasoftDbContext.STF02H.Where(p => p.BRG_MP == kdBrgInduk && p.IDMARKET.ToString() == IdMarket).FirstOrDefault();
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
                        sSQL += cust + "' , '" + desc.Replace('\'', '`') + "' , " + IdMarket + " , " + result.value.items[0].prices[0].price + " , " + result.value.items[0].prices[0].salePrice;
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
            sSQL += cust + "' , '" + desc.Replace('\'', '`') + "' , " + IdMarket + " , " + result.value.items[0].prices[0].price + " , " + result.value.items[0].prices[0].salePrice;
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

        protected async Task<string> prosesQueueFeedDetail(BlibliAPIData data, string requestId, string log_request_id)
        {
            string ret = "";
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/feed/detail?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (data.versiToken != "2")
            {
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/feed/detail", data.API_secret_key);
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/feed/detail?requestId=" + Uri.EscapeDataString(requestId) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + data.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = data.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = data.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/feed/detail?requestId=" + Uri.EscapeDataString(requestId) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", data.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020

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

            if (responseFromServer != "")
            {
                MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                {
                    REQUEST_ID = log_request_id
                };

                //dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                GetQueueFeedDetailResult result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(GetQueueFeedDetailResult)) as GetQueueFeedDetailResult;
                if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                {
                    var queueHistoryKosong = true;
                    if (result.value.queueHistory != null)
                    {
                        if (result.value.queueHistory.Count() > 0)
                        {
                            queueHistoryKosong = false;
                            foreach (var item in result.value.queueHistory)
                            {
                                if (Convert.ToBoolean(item.isSuccess))
                                {
                                    //if (Convert.ToString(result.value.queueFeed.requestAction) == "createProductV2")
                                    //{
                                    //    string ProductCode = "";
                                    //    string gdnSku = "";
                                    //    if (result.value.queueHistory.Count() > 0)
                                    //    {
                                    //        gdnSku = result.value.queueHistory[0].gdnSku;
                                    //        ProductCode = result.value.queueHistory[0].value;
                                    //    }

                                    //    await GetProdukInReviewList(data, requestId, ProductCode, gdnSku, log_request_id);
                                    //}

                                    if (Convert.ToString(result.value.queueFeed.requestAction) == "createProductV2" || Convert.ToString(result.value.queueFeed.requestAction) == "productRevision")
                                    {
                                        string ProductCode = "";
                                        string gdnSku = "";
                                        if (result.value.queueHistory.Count() > 0)
                                        {
                                            gdnSku = result.value.queueHistory[0].gdnSku;
                                            ProductCode = result.value.queueHistory[0].value;
                                        }

                                        //await GetProdukInReviewList(data, feed.request_id, ProductCode, gdnSku, feed.log_request_id);
                                        var apiLogInDb = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == currentLog.REQUEST_ID).SingleOrDefault();
                                        if (apiLogInDb != null)
                                        {
#if (DEBUG || Debug_AWS)
                                            await CreateProductGetProdukInReviewList(DatabasePathErasoft, apiLogInDb.REQUEST_ATTRIBUTE_1, apiLogInDb.CUST, "Barang", "Cek Status Review", data, requestId, ProductCode, gdnSku, currentLog.REQUEST_ID);

#else
                                            string EDBConnID = EDB.GetConnectionString("ConnId");
                                            var sqlStorage = new SqlServerStorage(EDBConnID);
                                            var client = new BackgroundJobClient(sqlStorage);
                                            client.Enqueue<BlibliControllerJob>(x => x.CreateProductGetProdukInReviewList(DatabasePathErasoft, apiLogInDb.REQUEST_ATTRIBUTE_1, apiLogInDb.CUST, "Barang", "Cek Status Review", data, requestId, ProductCode, gdnSku, currentLog.REQUEST_ID));
#endif
                                        }
                                    }
                                    else
                                    {
                                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);
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
                                            oCommand.Parameters[0].Value = Convert.ToString(data.merchant_code);
                                            oCommand.ExecuteNonQuery();

                                            currentLog.REQUEST_RESULT = Convert.ToString(item.errorMessage);
                                            currentLog.REQUEST_EXCEPTION = "";
                                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);

                                            if (Convert.ToString(result.value.queueFeed.requestAction) == "createProductV2")
                                            {
                                                var getLogMarketplace = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == log_request_id).FirstOrDefault();
                                                if (getLogMarketplace != null)
                                                {
                                                    oCommand.CommandType = CommandType.Text;
                                                    oCommand.CommandText = "UPDATE H SET BRG_MP='' FROM STF02H H INNER JOIN ARF01 A ON H.IDMARKET = A.RECNUM WHERE H.BRG=@MERCHANTSKU AND A.SORT1_CUST=@MERCHANTCODE AND ISNULL(H.BRG_MP,'') = 'PENDING'";
                                                    oCommand.Parameters.Add(new SqlParameter("@MERCHANTSKU", SqlDbType.NVarChar, 20));
                                                    oCommand.Parameters[1].Value = Convert.ToString(getLogMarketplace.REQUEST_ATTRIBUTE_1);
                                                    oCommand.ExecuteNonQuery();

                                                    #region Create Log Error khusus create barang
                                                    string subjectDescription = Convert.ToString(getLogMarketplace.REQUEST_ATTRIBUTE_1).Replace("'", "`");
                                                    string CUST = Convert.ToString(getLogMarketplace.CUST); //mengambil Cust
                                                    string ActionCategory = Convert.ToString("Barang"); //mengambil Kategori
                                                    string ActionName = Convert.ToString("Buat Produk"); //mengambil Action
                                                    string exceptionMessage = "From Blibli API : " + Convert.ToString(item.errorMessage);
                                                    string jobId = Convert.ToString(getLogMarketplace.REQUEST_ATTRIBUTE_3); // Hangfire Job Id saat Create Produk

                                                    string sSQL = "INSERT INTO API_LOG_MARKETPLACE (REQUEST_STATUS,CUST_ATTRIBUTE_1,CUST_ATTRIBUTE_2,CUST,MARKETPLACE,REQUEST_ID,";
                                                    sSQL += "REQUEST_ACTION,REQUEST_DATETIME,";
                                                    sSQL += "REQUEST_ATTRIBUTE_3, REQUEST_ATTRIBUTE_4,REQUEST_ATTRIBUTE_5,";
                                                    sSQL += "REQUEST_RESULT,REQUEST_EXCEPTION) ";
                                                    sSQL += "SELECT 'FAILED',A.CUST_ATTRIBUTE_1, '1', A.CUST,A.MARKETPLACE,A.REQUEST_ID,A.REQUEST_ACTION,A.REQUEST_DATETIME,A.REQUEST_ATTRIBUTE_3,A.REQUEST_ATTRIBUTE_4,A.REQUEST_ATTRIBUTE_5,A.REQUEST_RESULT,A.REQUEST_EXCEPTION ";
                                                    sSQL += "FROM ( SELECT '" + subjectDescription + "' CUST_ATTRIBUTE_1,'" + CUST + "' CUST,(SELECT TOP 1 B.NAMAMARKET FROM ARF01 A INNER JOIN MO.DBO.MARKETPLACE B ON A.NAMA = B.IDMARKET AND A.CUST='" + CUST + "') MARKETPLACE, '" + jobId + "' REQUEST_ID, ";
                                                    sSQL += "'" + ActionName + "' REQUEST_ACTION, '" + getLogMarketplace.REQUEST_DATETIME.ToString("yyyy-MM-dd HH:mm:ss") + "' REQUEST_DATETIME, ";
                                                    sSQL += "'" + ActionCategory + "' REQUEST_ATTRIBUTE_3,'" + subjectDescription + "' REQUEST_ATTRIBUTE_4, 'HANGFIRE' REQUEST_ATTRIBUTE_5, ";
                                                    sSQL += "'Create Product " + subjectDescription + " ke Blibli Gagal.' REQUEST_RESULT, '" + exceptionMessage.Replace("'", "`") + "' REQUEST_EXCEPTION ) A ";
                                                    sSQL += "LEFT JOIN API_LOG_MARKETPLACE B ON B.REQUEST_ATTRIBUTE_5 = 'HANGFIRE' AND A.REQUEST_ACTION = B.REQUEST_ACTION AND A.CUST = B.CUST AND A.CUST_ATTRIBUTE_1 = B.CUST_ATTRIBUTE_1 WHERE ISNULL(B.RECNUM,0) = 0 ";
                                                    int adaInsert = EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);
                                                    if (adaInsert == 0) //JIKA 
                                                    {
                                                        //update REQUEST_STATUS = 'FAILED', DATE, FAIL COUNT
                                                        sSQL = "UPDATE B SET REQUEST_STATUS = 'FAILED', REQUEST_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', CUST_ATTRIBUTE_2 = CONVERT(INT,CUST_ATTRIBUTE_2) + 1 ";
                                                        sSQL += "FROM API_LOG_MARKETPLACE B WHERE B.REQUEST_ATTRIBUTE_5 = 'HANGFIRE' AND B.REQUEST_STATUS = 'RETRYING' AND B.REQUEST_ID = '" + jobId + "'";
                                                        EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);

                                                        //update JOBID MENJADI JOBID BARU JIKA TIDAK SEDANG RETRY,STATUS,DATE,FAIL COUNT
                                                        sSQL = "UPDATE B SET REQUEST_STATUS = 'FAILED', REQUEST_ID = '" + jobId + "', REQUEST_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', CUST_ATTRIBUTE_2 = CONVERT(INT,CUST_ATTRIBUTE_2) + 1 ";
                                                        sSQL += "FROM API_LOG_MARKETPLACE B INNER JOIN ";
                                                        sSQL += "( SELECT '" + subjectDescription + "' CUST_ATTRIBUTE_1,'" + CUST + "' CUST,(SELECT TOP 1 B.NAMAMARKET FROM ARF01 A INNER JOIN MO.DBO.MARKETPLACE B ON A.NAMA = B.IDMARKET AND A.CUST='" + CUST + "') MARKETPLACE, '" + jobId + "' REQUEST_ID, ";
                                                        sSQL += "'" + ActionName + "' REQUEST_ACTION, '" + getLogMarketplace.REQUEST_DATETIME.ToString("yyyy-MM-dd HH:mm:ss") + "' REQUEST_DATETIME, ";
                                                        sSQL += "'" + ActionCategory + "' REQUEST_ATTRIBUTE_3,'" + subjectDescription + "' REQUEST_ATTRIBUTE_4, 'HANGFIRE' REQUEST_ATTRIBUTE_5, ";
                                                        sSQL += "'Create Product " + subjectDescription + " ke Blibli Gagal.' REQUEST_RESULT, '" + exceptionMessage.Replace("'", "`") + "' REQUEST_EXCEPTION ) A ";
                                                        sSQL += "ON B.REQUEST_ATTRIBUTE_5 = 'HANGFIRE' AND A.REQUEST_ACTION = B.REQUEST_ACTION AND A.CUST = B.CUST AND A.CUST_ATTRIBUTE_1 = B.CUST_ATTRIBUTE_1 AND B.REQUEST_STATUS IN ('FAILED','RETRYING')";
                                                        EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);
                                                    }
                                                    sSQL = "UPDATE S SET LINK_STATUS='Buat Produk Gagal', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', ";
                                                    //jobid;request_action;request_result;request_exception
                                                    string Link_Error = jobId + ";" + ActionName + ";Create Product " + subjectDescription + " ke Blibli Gagal.;" + exceptionMessage.Replace("'", "`");
                                                    sSQL += "LINK_ERROR = '" + Link_Error + "' FROM STF02H S INNER JOIN ARF01 A ON S.IDMARKET = A.RECNUM AND A.CUST = '" + CUST + "' WHERE S.BRG = '" + subjectDescription + "' ";
                                                    EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);
                                                    #endregion
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            var cekDateRequest = (new DateTime(1970, 1, 1)).AddMilliseconds(result.value.queueFeed.timeStamp);
                            if (Convert.ToString(result.value.queueFeed.requestAction) == "IN_PROGRESS" && cekDateRequest < DateTime.UtcNow.AddHours(7).AddDays(-1))
                            {
                                queueHistoryKosong = true;
                            }
                        }
                    }
                    if (queueHistoryKosong)
                    {
                        //contoh kasus : create product berhasil tanpa error dan product dalam status in QC di blibli mta
                        //tetapi saat queue feed detail, tidak ada queue history

                        //info dari Support Blibli API 
                        //6 september 2019 by +62 857-7329-2608 Christian Antonio
                        //ada beberapa kemungkinan pak, beberapa di antaranya:
                        //-Queuenya belum dijankan
                        //-Queuenya sudah dijalankan tetapi statusnya belum terupdate
                        //-Terjadi masalah saat mengirim queue / mengupdate status queue
                        //Kalau productnya sudah dalam status QC, kemungkinan itu karena statusnya belum terupdate di queuenya pak
                        //Terkadang bisa terjadi masalah pada sistem kami dan queuenya nyangkut sehingga statusnya tidak terupdate
                        //bisa ditambah pada flownya jika sudah melewati waktu tertentu dan statusnya masih in progress, 
                        //maka bisa langsung hit api get product in review, bila produknya belum ada di situ, 
                        //maka bisa hit api create product v2nya lagi, karena kemungkinan besar queuenya nyangkut dan belum diexcecute
                        var dateRequest = (new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(log_request_id));
                        if (dateRequest <= DateTime.UtcNow.AddHours(7).AddDays(-1))
                        {
                            //cek ke product in review
                            if (Convert.ToString(result.value.queueFeed.requestAction) == "createProductV2")
                            {
                                string ProductCode = "";
                                string gdnSku = "";

                                //await GetProdukInReviewList(data, feed.request_id, ProductCode, gdnSku, feed.log_request_id);
                                var apiLogInDb = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == currentLog.REQUEST_ID).SingleOrDefault();
                                if (apiLogInDb != null)
                                {
#if (DEBUG || Debug_AWS)
                                    //await CreateProductGetProdukInReviewList(DatabasePathErasoft, apiLogInDb.REQUEST_ATTRIBUTE_1, apiLogInDb.CUST, "Barang", "Link Produk (Tahap 1 / 2)", data, requestId, ProductCode, gdnSku, currentLog.REQUEST_ID);
                                    await GetProdukInReviewList(DatabasePathErasoft, apiLogInDb.REQUEST_ATTRIBUTE_1, apiLogInDb.CUST, "Barang", "Link Produk (Tahap 1 / 2)", data, requestId, ProductCode, gdnSku, currentLog.REQUEST_ID);

#else
                                    string EDBConnID = EDB.GetConnectionString("ConnId");
                                    var sqlStorage = new SqlServerStorage(EDBConnID);
                                    var client = new BackgroundJobClient(sqlStorage);
                                    //client.Enqueue<BlibliControllerJob>(x => x.CreateProductGetProdukInReviewList(DatabasePathErasoft, apiLogInDb.REQUEST_ATTRIBUTE_1, apiLogInDb.CUST, "Barang", "Link Produk (Tahap 1 / 2)", data, requestId, ProductCode, gdnSku, currentLog.REQUEST_ID));
                                    client.Enqueue<BlibliControllerJob>(x => x.GetProdukInReviewList(DatabasePathErasoft, apiLogInDb.REQUEST_ATTRIBUTE_1, apiLogInDb.CUST, "Barang", "Link Produk (Tahap 1 / 2)", data, requestId, ProductCode, gdnSku, currentLog.REQUEST_ID));
#endif
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        public async Task<string> GetProdukInReviewList(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, BlibliAPIData iden, string requestID, string ProductCode, string gdnSku, string api_log_requestId)
        {
            long milis = CurrentTimeMillis();

            var token = SetupContext(iden);
            iden.token = token;

            string productCodeBlibli = "";
            try
            {
                var aProductCode = JsonConvert.DeserializeObject(ProductCode, typeof(BlibliGetQueueProductCode)) as BlibliGetQueueProductCode;
                if (aProductCode != null)
                {
                    if (!string.IsNullOrEmpty(aProductCode.productCode))
                    {
                        productCodeBlibli = aProductCode.productCode;
                    }
                    else if (!string.IsNullOrEmpty(aProductCode.newProductCode))//need correction
                    {
                        productCodeBlibli = aProductCode.newProductCode;
                    }
                }

            }
            catch (Exception ex)
            {

            }

            if (iden.versiToken == "2")
            {
                milis = CurrentTimeMillis();
                DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
                iden.token = token;
                string userMTA = iden.mta_username_email_merchant;//<-- email user merchant

                var urll = "https://api.blibli.com/v2/proxy/seller/v1/product-submissions/filter?requestId=MasterOnline-" + Uri.EscapeDataString(milis.ToString()) + "&username=" + Uri.EscapeDataString(userMTA) + "&storeCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&channelId=MasterOnline";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

                string usernameMO = iden.API_client_username;
                string passMO = iden.API_client_password;

                myReq.Method = "POST";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());

                string myData = "{ \"filter\" : { \"sellerSku\": \"" + kodeProduk + "\",";
                //string myData = "{ \"filter\" : { ";
                myData += "\"state\": \"ALL\"},";
                myData += "\"paging\": {";
                myData += "\"page\": 0, ";
                myData += "\"size\": 100 } ";
                myData += "}";
                string responseFromServer = "";
                //try
                //{
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
                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    var tblCustomer = ErasoftDbContext.ARF01.Where(m => m.CUST == log_CUST).FirstOrDefault();
                    var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(Blibli_ProductSubmissionList_Response)) as Blibli_ProductSubmissionList_Response;
                    if (listBrg != null)
                    {
                        if (!string.IsNullOrEmpty(listBrg.errorCode))
                        {
                            throw new Exception(listBrg.errorCode + " : " + listBrg.errorMessage);
                        }
                        else
                        {
                            if (listBrg.content.Length > 0)//ada di need correction
                            {
                                foreach (var data in listBrg.content)
                                {
                                    if (data.product.code == kodeProduk)
                                    {
                                        if (data.product.state == "NEED_CORRECTION")
                                        {
                                            EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE [QUEUE_FEED_BLIBLI] SET [STATUS] = '2' WHERE [REQUESTID] = '" + requestID + "' AND [MERCHANT_CODE]='" + iden.merchant_code + "' AND [STATUS] = '1'");
                                            EDB.ExecuteSQL("CString", CommandType.Text, "UPDATE STF02H SET BRG_MP = 'NEED_CORRECTION;" + listBrg.content[0].product.code + "' WHERE BRG = '" + kodeProduk + "' AND IDMARKET = " + tblCustomer.RecNum);
                                            //ret.status = 1;
                                            //ret.message = data.product.revisionNotes;
                                            //return ret;
                                        }
                                        return "";
                                    }
                                }

                                //if (listBrg.content.Length == 100)
                                //{
                                //    cekProductQC(kodeProduk, iden, idMarket, brg, page + 1);
                                //}
                            }
                        }

                    }
                    EDB.ExecuteSQL("CString", CommandType.Text, "UPDATE STF02H SET BRG_MP = '' WHERE BRG = '" + kodeProduk + "' AND IDMARKET = " + tblCustomer.RecNum);
                    string sSQL = "DELETE FROM API_LOG_MARKETPLACE WHERE REQUEST_ATTRIBUTE_5 = 'HANGFIRE' AND REQUEST_ACTION = 'Buat Produk' AND CUST = '" + tblCustomer.CUST + "' AND CUST_ATTRIBUTE_1 = '" + kodeProduk + "'";
                    EDB.ExecuteSQL("sConn", CommandType.Text, sSQL); EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE [QUEUE_FEED_BLIBLI] SET [STATUS] = '2' WHERE [REQUESTID] = '" + requestID + "' AND [MERCHANT_CODE]='" + iden.merchant_code + "' AND [STATUS] = '1'");
#if (DEBUG || Debug_AWS)
                    await CreateProduct(dbPathEra, kodeProduk, tblCustomer.CUST, "Barang", "Buat Produk", iden, null, null);
#else
                    var sqlStorage = new SqlServerStorage(iden.DatabasePathErasoft);
                    var clientJobServer = new BackgroundJobClient(sqlStorage);
                    clientJobServer.Enqueue<BlibliControllerJob>(x => x.CreateProduct(dbPathEra, kodeProduk, tblCustomer.CUST, "Barang", "Buat Produk", iden, null, null));
#endif
                }
            }

            return "";
        }

        public async Task<string> GetCategoryPerUser(BlibliAPIData data)
        {

            string ret = "";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategory?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (data.versiToken != "2")
            {
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getCategory", data.API_secret_key);
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategory?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline";


                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + data.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = data.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = data.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategory?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", data.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020

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
            if (responseFromServer != null)
            {
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                var cekCust = ErasoftDbContext.ARF01.Where(a => a.RecNum == data.idmarket).FirstOrDefault();
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
                                if (cekCust != null)
                                {
                                    if (cekCust.KD_ANALISA == "2")
                                    {
                                        cekCust.STATUS_API = "1";
                                        ErasoftDbContext.SaveChanges();
                                    }
                                }
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
                else
                {
                    if (cekCust != null)
                    {
                        if (cekCust.KD_ANALISA == "2")
                        {
                            cekCust.STATUS_API = "0";
                            ErasoftDbContext.SaveChanges();
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

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategory?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (data.versiToken != "2")
            {
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getCategory", data.API_secret_key);
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategory?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline";


                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + data.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = data.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = data.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategory?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", data.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020

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

        public async Task<string> UpdateAttributeList(BlibliAPIData data, List<CATEGORY_BLIBLI> category)
        {
            string ret = "";
            var listCategoryCode = category.Select(p => p.CATEGORY_CODE).ToList();
            var AttributeInDb = MoDbContext.AttributeBlibli.Where(p => listCategoryCode.Contains(p.CATEGORY_CODE)).ToList();
            MoDbContext.AttributeBlibli.RemoveRange(AttributeInDb);
            MoDbContext.SaveChanges();

            foreach (var item in category)
            {
                string categoryCode = item.CATEGORY_CODE;
                string categoryName = item.CATEGORY_NAME;

                long milis = CurrentTimeMillis();
                DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

                string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
                string userMTA = data.mta_username_email_merchant;//<-- email user merchant
                string passMTA = data.mta_password_password_merchant;//<-- pass merchant

                //add by nurul 13/7/2020
                string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategoryAttributes?";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                //end add by nurul 13/7/2020

                //change by nurul 13/7/2020
                if (data.versiToken != "2")
                {
                    string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getCategoryAttributes", data.API_secret_key);
                    urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&categoryCode=" + Uri.EscapeDataString(categoryCode) + "&channelId=MasterOnline";
                    //string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/getCategoryAttributes", data.API_secret_key);
                    //    string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + milis + "&businessPartnerCode=" + data.merchant_code + "&categoryCode=" + categoryCode;

                    myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    myReq.Headers.Add("Authorization", ("bearer " + data.token));
                    myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                    myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                    myReq.Accept = "application/json";
                    myReq.ContentType = "application/json";
                    myReq.Headers.Add("requestId", milis.ToString());
                    myReq.Headers.Add("sessionId", milis.ToString());
                    myReq.Headers.Add("username", userMTA);
                }
                else
                {
                    string usernameMO = data.API_client_username;
                    //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                    string passMO = data.API_client_password;
                    urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&categoryCode=" + Uri.EscapeDataString(categoryCode) + "&channelId=MasterOnline";

                    myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                    myReq.Accept = "application/json";
                    myReq.ContentType = "application/json";
                    myReq.Headers.Add("Api-Seller-Key", data.API_secret_key.ToString());
                    myReq.Headers.Add("Signature-Time", milis.ToString());
                }
                //end change by nurul 13/7/2020

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

                                    //cek jika sudah ada di database
                                    var cari = AttributeInDb.Where(p => p.CATEGORY_CODE.ToUpper().Equals(categoryCode.ToUpper())
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

        public class GetAttributeBlibliResult
        {
            public string requestId { get; set; }
            public object headers { get; set; }
            public object errorMessage { get; set; }
            public object errorCode { get; set; }
            public bool success { get; set; }
            public GetAttributeBlibliResultValue value { get; set; }
        }

        public class GetAttributeBlibliResultValue
        {
            public string categoryCode { get; set; }
            public string name { get; set; }
            public List<GetAttributeBlibliResultAttribute> attributes { get; set; }
        }

        public class GetAttributeBlibliResultAttribute
        {
            public string attributeCode { get; set; }
            public string attributeType { get; set; }
            public string name { get; set; }
            public List<string> options { get; set; }
            public bool variantCreation { get; set; }
            public bool mandatory { get; set; }
            public bool skuValue { get; set; }
        }


        public async Task<ATTRIBUTE_BLIBLI_AND_OPT> GetAttributeToList(BlibliAPIData data, CATEGORY_BLIBLI category)
        {
            //var category = MoDbContext.CategoryBlibli.Where(p => p.IS_LAST_NODE.Equals("1")).ToList();
            //string ret = "";
            ATTRIBUTE_BLIBLI_AND_OPT ret = new ATTRIBUTE_BLIBLI_AND_OPT();
            //foreach (var item in category)
            //{
            string categoryCode = category.CATEGORY_CODE;
            string categoryName = category.CATEGORY_NAME;
            //    string categoryCode = "3 -1000001";
            //string categoryName = "3 Kamar +";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategoryAttributes?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (data.versiToken != "2")
            {
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getCategoryAttributes", data.API_secret_key);
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&categoryCode=" + Uri.EscapeDataString(categoryCode) + "&channelId=MasterOnline";
                //string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/getCategoryAttributes", data.API_secret_key);
                //    string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + milis + "&businessPartnerCode=" + data.merchant_code + "&categoryCode=" + categoryCode;

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + data.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = data.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = data.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&categoryCode=" + Uri.EscapeDataString(categoryCode) + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", data.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020

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
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(GetAttributeBlibliResult)) as GetAttributeBlibliResult;
                if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                {
                    if (result.value.attributes.Count() > 0)
                    {
                        ATTRIBUTE_BLIBLI_NEW returnData = new ATTRIBUTE_BLIBLI_NEW();
                        int i = 0;
                        string a = "";
                        foreach (var attribs in result.value.attributes)
                        {
                            a = Convert.ToString(i + 1);
                            returnData.CATEGORY_CODE = category.CATEGORY_CODE;
                            returnData.CATEGORY_NAME = category.CATEGORY_NAME;

                            //sSQL += "[ACODE_" + a + "],[ATYPE_" + a + "],[ANAME_" + a + "],[AOPTIONS_" + a + "],";
                            //oCommand.Parameters[(i * 4) + 2].Value = result.value.attributes[i].attributeCode.Value;
                            //oCommand.Parameters[(i * 4) + 3].Value = result.value.attributes[i].attributeType.Value;
                            //oCommand.Parameters[(i * 4) + 4].Value = result.value.attributes[i].name.Value;
                            //oCommand.Parameters[(i * 4) + 5].Value = result.value.attributes[i].options.Count > 0 ? "1" : "0";
                            returnData["ACODE_" + a] = Convert.ToString(attribs.attributeCode);
                            returnData["ATYPE_" + a] = Convert.ToString(attribs.attributeType);
                            returnData["ANAME_" + a] = Convert.ToString(attribs.name);
                            returnData["AOPTIONS_" + a] = attribs.options.Count > 0 ? "1" : "0";
                            returnData["AVARCREATE_" + a] = attribs.variantCreation ? "1" : "0";

                            if (attribs.options.Count() > 0)
                            {
                                var optList = attribs.options.ToList();
                                var listOpt = optList.Select(x => new ATTRIBUTE_OPT_BLIBLI(attribs.attributeCode.ToString(), attribs.attributeType.ToString(), attribs.name.ToString(), x)).ToList();
                                ret.attribute_opt.AddRange(listOpt);
                            }
                            i = i + 1;
                        }
                        ret.attributes.Add(returnData);
                    }
                }
            }
            //}

            return ret;
        }

        //add by nurul 10/5/2021, limit blibli
        public async Task<string> GetCategoryTreeV2(BlibliAPIData data)
        {
            //HASIL MEETING : SIMPAN CATEGORY DAN ATTRIBUTE NYA KE DATABASE MO
            //INSERT JIKA CATEGORY_CODE UTAMA BELUM ADA DI MO
            EDB = new DatabaseSQL(data.DatabasePathErasoft);
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

            string ret = "1";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategory?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (data.versiToken != "2")
            {
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getCategory", data.API_secret_key);
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategory?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline";


                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + data.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = data.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = data.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategory?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", data.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
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
                ret = "0";
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
            if (responseFromServer != "")
            {
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {
                    if (result.content.Count > 0)
                    {
                        var successInsert = false;
                        using (SqlConnection oConnection = new SqlConnection(con))
                        //using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                        {
                            oConnection.Open();
                            //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
                            //{
                            using (SqlCommand oCommand = oConnection.CreateCommand())
                            {
                                //oCommand.CommandText = "DELETE FROM [CATEGORY_BLIBLI] WHERE ARF01_SORT1_CUST='" + data.merchant_code + "'";
                                //oCommand.ExecuteNonQuery();
                                //oCommand.Transaction = oTransaction;
                                oCommand.CommandText = "DELETE FROM [CATEGORY_BLIBLI]";
                                oCommand.ExecuteNonQuery();

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
                                            //string getCatCode = Convert.ToString(item.categoryCode.Value);
                                            //var CategoryBlibli = MoDbContext.CategoryBlibli.Where(k => k.CATEGORY_CODE == getCatCode).FirstOrDefault();
                                            //var listAttributeBlibli = await GetAttributeToListV2(data, CategoryBlibli);
                                            successInsert = true;
                                        }
                                    }
                                    //oTransaction.Commit();
                                }
                                catch (Exception ex)
                                {
                                    //oTransaction.Rollback();
                                    successInsert = false;
                                }
                            }
                            //}
                        }
                        //await GetAttributeToListV2(data);
                        if (successInsert)
                        {
                            MoDbContext = new MoDbContext("");
                            var ListCategory = MoDbContext.CategoryBlibli.ToList();
                            foreach (var cat in ListCategory)
                            {
                                var listAttributeBlibli = await GetAttributeToListV2(data, cat);
                            }
                        }
                        else
                        {
                            ret = "0";
                        }
                    }
                    else
                    {
                        ret = "0";
                    }
                }
                else
                {
                    ret = "0";
                }
            }
            else
            {
                ret = "0";
            }

            return ret;
        }

        public async Task<ATTRIBUTE_BLIBLI_AND_OPT_New> GetAttributeToListV2(BlibliAPIData data, CATEGORY_BLIBLI category)
        {
            MoDbContext = new MoDbContext("");
            //var category = MoDbContext.CategoryBlibli.Where(p => p.IS_LAST_NODE.Equals("1")).ToList();
            //string ret = "";
            ATTRIBUTE_BLIBLI_AND_OPT_New ret = new ATTRIBUTE_BLIBLI_AND_OPT_New();
            //foreach (var item in category)
            //{
            string categoryCode = category.CATEGORY_CODE;
            string categoryName = category.CATEGORY_NAME;
            //    string categoryCode = "3 -1000001";
            //string categoryName = "3 Kamar +";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategoryAttributes?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (data.versiToken != "2")
            {
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getCategoryAttributes", data.API_secret_key);
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&categoryCode=" + Uri.EscapeDataString(categoryCode) + "&channelId=MasterOnline";
                //string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/getCategoryAttributes", data.API_secret_key);
                //    string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + milis + "&businessPartnerCode=" + data.merchant_code + "&categoryCode=" + categoryCode;

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + data.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = data.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = data.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&categoryCode=" + Uri.EscapeDataString(categoryCode) + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", data.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020

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
            if (responseFromServer != "")
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(GetAttributeBlibliResult)) as GetAttributeBlibliResult;
                if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                {
                    if (result.value.attributes.Count() > 0)
                    {
                        ATTRIBUTE_BLIBLI returnData = new ATTRIBUTE_BLIBLI();
                        int i = 0;
                        string a = "";
                        foreach (var attribs in result.value.attributes)
                        {
                            a = Convert.ToString(i + 1);
                            returnData.CATEGORY_CODE = category.CATEGORY_CODE;
                            returnData.CATEGORY_NAME = category.CATEGORY_NAME;

                            //sSQL += "[ACODE_" + a + "],[ATYPE_" + a + "],[ANAME_" + a + "],[AOPTIONS_" + a + "],";
                            //oCommand.Parameters[(i * 4) + 2].Value = result.value.attributes[i].attributeCode.Value;
                            //oCommand.Parameters[(i * 4) + 3].Value = result.value.attributes[i].attributeType.Value;
                            //oCommand.Parameters[(i * 4) + 4].Value = result.value.attributes[i].name.Value;
                            //oCommand.Parameters[(i * 4) + 5].Value = result.value.attributes[i].options.Count > 0 ? "1" : "0";
                            returnData["ACODE_" + a] = Convert.ToString(attribs.attributeCode);
                            returnData["ATYPE_" + a] = Convert.ToString(attribs.attributeType);
                            returnData["ANAME_" + a] = Convert.ToString(attribs.name);
                            returnData["AOPTIONS_" + a] = attribs.options.Count > 0 ? "1" : "0";
                            //returnData["AOPTIONS_" + a] = attribs.attributeType != "DESCRIPTIVE_ATTRIBUTE" ? "1" : "0";
                            returnData["AMANDATORY_" + a] = attribs.mandatory ? "1" : "0";
                            returnData["AVARCREATE_" + a] = attribs.variantCreation ? "1" : "0";
                            returnData["ASKUVALUE_" + a] = attribs.skuValue ? "1" : "0";

                            if (attribs.options.Count() > 0)
                            {
                                var optList = attribs.options.ToList();
                                var listOpt = optList.Select(x => new ATTRIBUTE_OPT_BLIBLI(attribs.attributeCode.ToString(), attribs.attributeType.ToString(), attribs.name.ToString(), x)).ToList();
                                ret.attribute_opt.AddRange(listOpt);
                            }
                            i = i + 1;
                        }
                        ret.attributes.Add(returnData);
                        try
                        {
                            var getAttributeOld = MoDbContext.AttributeBlibli.Where(b => b.CATEGORY_CODE == categoryCode).ToList();
                            if (getAttributeOld.Count() > 0)
                            {
                                MoDbContext.AttributeBlibli.RemoveRange(getAttributeOld);
                                MoDbContext.SaveChanges();
                            }
                            MoDbContext.AttributeBlibli.AddRange(ret.attributes);
                            MoDbContext.SaveChanges();
                            var getAttribute = MoDbContext.AttributeBlibli.Where(b => b.CATEGORY_CODE == categoryCode).ToList();
                            var listAttributeOpt = new List<ATTRIBUTE_OPT_BLIBLI>();
                            foreach (var attr in getAttribute)
                            {
                                var getAttributeOpt = MoDbContext.AttributeOptBlibli.Where(b => b.ACODE == attr.ACODE_1 || b.ACODE == attr.ACODE_2 || b.ACODE == attr.ACODE_3 || b.ACODE == attr.ACODE_4 || b.ACODE == attr.ACODE_5 || b.ACODE == attr.ACODE_6 || b.ACODE == attr.ACODE_7 || b.ACODE == attr.ACODE_8 || b.ACODE == attr.ACODE_9 || b.ACODE == attr.ACODE_10 ||
                                                                                                b.ACODE == attr.ACODE_11 || b.ACODE == attr.ACODE_12 || b.ACODE == attr.ACODE_13 || b.ACODE == attr.ACODE_14 || b.ACODE == attr.ACODE_15 || b.ACODE == attr.ACODE_16 || b.ACODE == attr.ACODE_17 || b.ACODE == attr.ACODE_18 || b.ACODE == attr.ACODE_19 || b.ACODE == attr.ACODE_20 ||
                                                                                                b.ACODE == attr.ACODE_21 || b.ACODE == attr.ACODE_22 || b.ACODE == attr.ACODE_23 || b.ACODE == attr.ACODE_24 || b.ACODE == attr.ACODE_25 || b.ACODE == attr.ACODE_26 || b.ACODE == attr.ACODE_27 || b.ACODE == attr.ACODE_28 || b.ACODE == attr.ACODE_29 || b.ACODE == attr.ACODE_30 ||
                                                                                                b.ACODE == attr.ACODE_31 || b.ACODE == attr.ACODE_32 || b.ACODE == attr.ACODE_33 || b.ACODE == attr.ACODE_34 || b.ACODE == attr.ACODE_35).ToList();
                                listAttributeOpt.AddRange(getAttributeOpt);
                            };
                            //if (getAttribute.Count() > 0)
                            //{
                            //    MoDbContext.AttributeBlibli.RemoveRange(getAttribute);
                            if (listAttributeOpt.Count() > 0)
                            {
                                MoDbContext.AttributeOptBlibli.RemoveRange(listAttributeOpt);
                                MoDbContext.SaveChanges();
                            }
                            //}
                            MoDbContext.AttributeOptBlibli.AddRange(ret.attribute_opt);
                            MoDbContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            //}

            return ret;
        }
        //end add by nurul 10/5/2021, limit blibli

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

                //add by nurul 13/7/2020
                string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategoryAttributes?";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                //end add by nurul 13/7/2020

                //change by nurul 13/7/2020
                if (data.versiToken != "2")
                {
                    string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getCategoryAttributes", data.API_secret_key);
                    urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&categoryCode=" + Uri.EscapeDataString(categoryCode) + "&channelId=MasterOnline";
                    //string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/getCategoryAttributes", data.API_secret_key);
                    //    string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + milis + "&businessPartnerCode=" + data.merchant_code + "&categoryCode=" + categoryCode;

                    myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    myReq.Headers.Add("Authorization", ("bearer " + data.token));
                    myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                    myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                    myReq.Accept = "application/json";
                    myReq.ContentType = "application/json";
                    myReq.Headers.Add("requestId", milis.ToString());
                    myReq.Headers.Add("sessionId", milis.ToString());
                    myReq.Headers.Add("username", userMTA);
                }
                else
                {
                    string usernameMO = data.API_client_username;
                    //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                    string passMO = data.API_client_password;
                    urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getCategoryAttributes?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&categoryCode=" + Uri.EscapeDataString(categoryCode) + "&channelId=MasterOnline";

                    myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                    myReq.Accept = "application/json";
                    myReq.ContentType = "application/json";
                    myReq.Headers.Add("Api-Seller-Key", data.API_secret_key.ToString());
                    myReq.Headers.Add("Signature-Time", milis.ToString());
                }
                //end change by nurul 13/7/2020

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

        public async Task<int> FixOrderBlibliWithPage(BlibliAPIData iden, string CUST, string NAMA_CUST, string datefrom, string dateto, int page)
        {
            int count = 0;
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderList?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (iden.versiToken != "2")
            {
                //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/order/orderList", iden.API_secret_key);
                //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/product/createProduct";
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderList?orderBy=statusFPUpdatedTimestamp&sortBy=asc&page=" + page + "&size=100&requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&status=D&channelId=MasterOnline&filterStartDate=" + Uri.EscapeDataString(Convert.ToDateTime(datefrom).ToString("yyyy-MM-dd HH:mm:ss")) + "&filterEndDate=" + Uri.EscapeDataString(Convert.ToDateTime(dateto).ToString("yyyy-MM-dd HH:mm:ss"));

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderList?orderBy=statusFPUpdatedTimestamp&sortBy=asc&page=" + page + "&size=100&requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&status=D&channelId=MasterOnline&filterStartDate=" + Uri.EscapeDataString(Convert.ToDateTime(datefrom).ToString("yyyy-MM-dd HH:mm:ss")) + "&filterEndDate=" + Uri.EscapeDataString(Convert.ToDateTime(dateto).ToString("yyyy-MM-dd HH:mm:ss"));

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020

            string responseFromServer = "";

            using (WebResponse response = await myReq.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseFromServer = reader.ReadToEnd();
                }
            }

            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(BlibliGetOrder)) as BlibliGetOrder;
                if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                {
                    count = result.content.Count();
                    if (result.content.Count() > 0)
                    {
                        foreach (var item in result.content)
                        {
                            await FixOrderGetOrderDetail(iden, item.orderNo, item.orderItemNo, CUST, NAMA_CUST);
                        }
                    }
                }
            }
            return count;
        }

        public async Task<string> FixOrderBlibli(BlibliAPIData iden, string CUST, string NAMA_CUST, string datefrom, string dateto)
        {
            if (!string.IsNullOrEmpty(iden.merchant_code))
            {
                var token = SetupContext(iden);
                iden.token = token;
                int page = 0;
                var more = true;

                while (more)
                {
                    int count = await FixOrderBlibliWithPage(iden, CUST, NAMA_CUST, datefrom, dateto, page);
                    page++;
                    if (count < 100)
                    {
                        more = false;
                    }
                }
            }

            string ret = "";
            return ret;
        }

        public async Task<string> FixOrderGetOrderDetail(BlibliAPIData iden, string orderNo, string orderItemNo, string CUST, string NAMA_CUST)
        {
            string connId = "fix_blibli";
            //if merchant code diisi. barulah GetOrderList
            string ret = "";
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
            //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi-sandbox/api/businesspartner/v1/product/createProduct", iden.API_secret_key);

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderDetail?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (iden.versiToken != "2")
            {
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/order/orderDetail", iden.API_secret_key);
                //string urll = "https://apisandbox.blibli.com/v2/proxy/mtaapi-sandbox/api/businesspartner/v1/order/orderDetail";
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderDetail?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&orderNo=" + orderNo + "&orderItemNo=" + orderItemNo + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderDetail?requestId=" + Uri.EscapeDataString(milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&orderNo=" + orderNo + "&orderItemNo=" + orderItemNo + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020

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
                            oCommand.CommandText = "DELETE FROM [TEMP_BLI_ORDERDETAIL_FIX] WHERE CUST = '" + CUST + "'";
                            oCommand.ExecuteNonQuery();
                            //oCommand.Transaction = oTransaction;
                            oCommand.CommandType = CommandType.Text;
                            string sSQL = "INSERT INTO [TEMP_BLI_ORDERDETAIL_FIX] (";
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
                                //change by Tri 12 Mei 2020, autocanceldate bisa null
                                //oCommand.Parameters["@autoCancelDate"].Value = DateTimeOffset.FromUnixTimeMilliseconds(result.value.autoCancelDate).UtcDateTime.AddHours(7);
                                oCommand.Parameters["@autoCancelDate"].Value = DateTimeOffset.FromUnixTimeMilliseconds(result.value.autoCancelDate.HasValue ? result.value.autoCancelDate.Value : result.value.orderDate).UtcDateTime.AddHours(7);
                                //end change by Tri 12 Mei 2020, autocanceldate bisa null

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

                                oCommand.Parameters["@merchantSku"].Value = result.value.merchantSku != null ? result.value.merchantSku : "";
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
                                oCommand.Parameters["@startOperationalTime"].Value = result.value.startOperationalTime ?? 0;

                                oCommand.Parameters["@endOperationalTime"].Value = result.value.endOperationalTime ?? 0;
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
                                    //var connIdARF01C = Guid.NewGuid().ToString();

                                    //var kabKot = "3174";
                                    //var prov = "31";

                                    //string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                    //insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                    //insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                                    //insertPembeli += "('" + result.value.custName + "','" + result.value.shippingStreetAddress + "','" + result.value.shippingMobile + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
                                    //insertPembeli += "1, 'IDR', '01', '" + result.value.shippingStreetAddress + "', 0, 0, 0, 0, '1', 0, 0, ";
                                    //insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + result.value.shippingZipCode + "', '" + result.value.customerEmail + "', '" + kabKot + "', '" + prov + "', '', '','" + connIdARF01C + "')";
                                    //EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);

                                    SqlCommand CommandSQL = new SqlCommand();
                                    ////call sp to insert buyer data
                                    //CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                    //CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIdARF01C;

                                    //EDB.ExecuteSQL("Con", "MoveARF01CFromTempTable", CommandSQL);

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
                                    CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                                    CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                                    EDB.ExecuteSQL("Con", "FixOrderBlibli", CommandSQL);

                                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                                }
                            }
                            catch (Exception ex)
                            {
                                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                            }
                        }
                    }
                }
                else
                {
                    //currentLog.REQUEST_RESULT = Convert.ToString(result.errorCode);
                    //currentLog.REQUEST_EXCEPTION = Convert.ToString(result.errorMessage);
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
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
            public string DatabasePathErasoft { get; set; }
            public string username { get; set; }
            //add by nurul 16/7/2020
            public string versiToken { get; set; }
            //end add by nurul 16/7/2020
        }
        public class BlibliQueueFeedData
        {
            public string request_id { get; set; }
            public string log_request_id { get; set; }

        }
        public class BlibliProductData
        {
            public string type { get; set; }
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
            public STF02 dataBarangInDb { get; set; }


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
            public ItemImage[] images { get; set; }
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

        public class ItemImage
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

        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Blibli Gagal.")]
        public async Task<string> CreateProduct(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, BlibliAPIData iden, BlibliProductData data, PerformContext context)
        {
            var token = SetupContext(iden);
            iden.token = token;

            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.merchant_code).FirstOrDefault();
            var stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == kodeProduk && p.IDMARKET == arf01.RecNum).FirstOrDefault();
            var barangInDb = ErasoftDbContext.STF02.AsNoTracking().SingleOrDefault(b => b.BRG == kodeProduk);
            data = new BlibliControllerJob.BlibliProductData
            {
                kode = barangInDb.BRG,
                nama = barangInDb.NAMA + ' ' + barangInDb.NAMA2 + ' ' + barangInDb.NAMA3,
                berat = (barangInDb.BERAT).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                Keterangan = barangInDb.Deskripsi,
                Qty = "0",
                MinQty = "0",
                //PickupPoint = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == barangInDb.BRG && m.IDMARKET == arf01.RecNum).PICKUP_POINT.ToString(),
                PickupPoint = stf02h.PICKUP_POINT,
                IDMarket = arf01.RecNum.ToString(),
                Length = Convert.ToString(barangInDb.PANJANG),
                Width = Convert.ToString(barangInDb.LEBAR),
                Height = Convert.ToString(barangInDb.TINGGI),
                dataBarangInDb = barangInDb
            };
            if (!string.IsNullOrEmpty(stf02h.NAMA_BARANG_MP))
            {
                data.nama = stf02h.NAMA_BARANG_MP;
            }
            if (!string.IsNullOrEmpty(stf02h.DESKRIPSI_MP))
            {
                if(stf02h.DESKRIPSI_MP != "null")
                data.Keterangan = stf02h.DESKRIPSI_MP;
            }
            data.type = barangInDb.TYPE;//add by Tri 27/9/2019
            data.Brand = stf02h.AVALUE_38;
            data.Price = barangInDb.HJUAL.ToString();
            data.MarketPrice = stf02h.HJUAL.ToString();
            data.CategoryCode = stf02h.CATEGORY_CODE.ToString();

            data.display = stf02h.DISPLAY ? "true" : "false";

            //if merchant code diisi. barulah upload produk
            string ret = "";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/getProductList?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

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
                length = Convert.ToInt32(Convert.ToDouble(data.Length)),
                width = Convert.ToInt32(Convert.ToDouble(data.Width)),
                height = Convert.ToInt32(Convert.ToDouble(data.Height)),
                weight = Convert.ToInt32(Convert.ToDouble(data.berat)),
                description = Convert.ToBase64String(Encoding.ASCII.GetBytes(WebUtility.HtmlDecode(data.Keterangan))),
                //uniqueSellingPoint = Convert.ToBase64String(Encoding.ASCII.GetBytes(data.Keterangan)),
                //diisi dengan AVALUE_39
                productStory = Convert.ToBase64String(Encoding.ASCII.GetBytes(WebUtility.HtmlDecode(data.Keterangan))),
            };
            //add 6 april 2021, validasi big product
            if (newData.weight > 50000)// berat lebih dari 50kg -> big product
            {
                newData.productType = 2;
            }
            //end add 6 april 2021, validasi big product

            //string sSQL = "SELECT * FROM (";
            //for (int i = 1; i <= 30; i++)
            //{
            //    sSQL += "SELECT B.ACODE_" + i.ToString() + " AS CATEGORY_CODE,B.ANAME_" + i.ToString() + " AS CATEGORY_NAME,B.ATYPE_" + i.ToString() + " AS CATEGORY_TYPE,A.AVALUE_" + i.ToString() + " AS VALUE FROM STF02H (NOLOCK) A INNER JOIN MO.DBO.ATTRIBUTE_BLIBLI (NOLOCK) B ON A.CATEGORY_CODE = B.CATEGORY_CODE WHERE A.BRG='" + data.kode + "' AND A.IDMARKET = '" + data.IDMarket + "' " + System.Environment.NewLine;
            //    if (i < 30)
            //    {
            //        sSQL += "UNION ALL " + System.Environment.NewLine;
            //    }
            //}

            //DataSet dsFeature = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE <> 'DEFINING_ATTRIBUTE' ");
            //DataSet dsVariasi = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE = 'DEFINING_ATTRIBUTE' ");

            var CategoryBlibli = MoDbContext.CategoryBlibli.Where(k => k.CATEGORY_CODE == data.CategoryCode).FirstOrDefault();
            //change by nurul 11/5/2021, limit blibli
            //var listAttributeBlibli = await GetAttributeToList(iden, CategoryBlibli);
            var getAttribute = MoDbContext.AttributeBlibli.Where(a => a.CATEGORY_CODE == data.CategoryCode).ToList();
            var listAttributeOpt = new List<ATTRIBUTE_OPT_BLIBLI>();
            foreach (var attr in getAttribute)
            {
                var getAttributeOpt = MoDbContext.AttributeOptBlibli.Where(a => a.ACODE == attr.ACODE_1 || a.ACODE == attr.ACODE_2 || a.ACODE == attr.ACODE_3 || a.ACODE == attr.ACODE_4 || a.ACODE == attr.ACODE_5 || a.ACODE == attr.ACODE_6 || a.ACODE == attr.ACODE_7 || a.ACODE == attr.ACODE_8 || a.ACODE == attr.ACODE_9 || a.ACODE == attr.ACODE_10 ||
                                                                                a.ACODE == attr.ACODE_11 || a.ACODE == attr.ACODE_12 || a.ACODE == attr.ACODE_13 || a.ACODE == attr.ACODE_14 || a.ACODE == attr.ACODE_15 || a.ACODE == attr.ACODE_16 || a.ACODE == attr.ACODE_17 || a.ACODE == attr.ACODE_18 || a.ACODE == attr.ACODE_19 || a.ACODE == attr.ACODE_20 ||
                                                                                a.ACODE == attr.ACODE_21 || a.ACODE == attr.ACODE_22 || a.ACODE == attr.ACODE_23 || a.ACODE == attr.ACODE_24 || a.ACODE == attr.ACODE_25 || a.ACODE == attr.ACODE_26 || a.ACODE == attr.ACODE_27 || a.ACODE == attr.ACODE_28 || a.ACODE == attr.ACODE_29 || a.ACODE == attr.ACODE_30 ||
                                                                                a.ACODE == attr.ACODE_31 || a.ACODE == attr.ACODE_32 || a.ACODE == attr.ACODE_33 || a.ACODE == attr.ACODE_34 || a.ACODE == attr.ACODE_35).ToList();
                listAttributeOpt.AddRange(getAttributeOpt);
            };
            var listAttributeBlibli = new ATTRIBUTE_BLIBLI_AND_OPT_New()
            {
                attributes = getAttribute,
                attribute_opt = listAttributeOpt,
            };
            //end change by nurul 11/5/2021, limit blibli

            List<string> dsFeature = new List<string>();
            List<string> dsVariasi = new List<string>();
            var attribute = listAttributeBlibli.attributes.FirstOrDefault();
            //for (int i = 1; i <= 30; i++)
            for (int i = 1; i <= 35; i++)
            {
                string attribute_id = Convert.ToString(attribute["ACODE_" + i.ToString()]);
                string attribute_name = Convert.ToString(attribute["ANAME_" + i.ToString()]);
                string attribute_type = Convert.ToString(attribute["ATYPE_" + i.ToString()]);
                string attribute_vc = Convert.ToString(attribute["AVARCREATE_" + i.ToString()]);
                if (!string.IsNullOrWhiteSpace(attribute_id))
                {
                    if (attribute_type == "DEFINING_ATTRIBUTE" || attribute_vc == "1")
                    {
                        dsVariasi.Add(attribute_id);
                    }
                    else
                    {
                        dsFeature.Add(attribute_id);
                    }
                }
            }

            Dictionary<string, string> nonDefiningAttributes = new Dictionary<string, string>();

            //for (int i = 0; i < dsFeature.Tables[0].Rows.Count; i++)
            //{
            //    if (!nonDefiningAttributes.ContainsKey(Convert.ToString(dsFeature.Tables[0].Rows[i]["CATEGORY_CODE"])))
            //    {
            //        nonDefiningAttributes.Add(Convert.ToString(dsFeature.Tables[0].Rows[i]["CATEGORY_CODE"]), Convert.ToString(dsFeature.Tables[0].Rows[i]["VALUE"]).Trim());
            //    }
            //}

            //change 9 juli 2020, ambil 35 attribute
            //for (int i = 1; i <= 30; i++)
            for (int i = 1; i <= 35; i++)
            //end change 9 juli 2020, ambil 35 attribute
            {
                string attribute_id = Convert.ToString(stf02h["ACODE_" + i.ToString()]);
                string value = Convert.ToString(stf02h["AVALUE_" + i.ToString()]);
                //if (!string.IsNullOrWhiteSpace(attribute_id))
                if (!string.IsNullOrWhiteSpace(attribute_id) && (value ?? "null") != "null" && !string.IsNullOrEmpty(value))
                {
                    if (dsFeature.Contains(attribute_id))
                    {
                        if (!nonDefiningAttributes.ContainsKey(attribute_id))
                        {
                            nonDefiningAttributes.Add(attribute_id, value.Trim());
                        }
                    }
                }
            }

            newData.productNonDefiningAttributes = nonDefiningAttributes;

            Dictionary<string, string> images = new Dictionary<string, string>();
            //List<string> uploadedImageID = new List<string>();
            List<Productitem> productItems = new List<Productitem>();
#region bukan barang variasi
            //change by nurul 14/9/2020, handle barang multi sku juga 
            //if (data.type == "3")
            if (data.type == "3" || data.type == "6")
            //change by nurul 14/9/2020, handle barang multi sku juga 
            {
                List<string> images_pervar = new List<string>();
                string idGambar = "";
                string urlGambar = "";

                idGambar = stf02h.ACODE_50;
                urlGambar = stf02h.AVALUE_50;
                //if (string.IsNullOrWhiteSpace(idGambar))
                if (string.IsNullOrWhiteSpace(urlGambar))
                {
                    idGambar = data.dataBarangInDb.Sort5;
                    urlGambar = data.dataBarangInDb.LINK_GAMBAR_1;
                }
                idGambar = stf02h.BRG + "_1_" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
                //if (!string.IsNullOrWhiteSpace(idGambar))
                if (!string.IsNullOrWhiteSpace(urlGambar))
                {
                    //if (!uploadedImageID.Contains(idGambar))
                    //{
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(urlGambar);

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
                            //System.Drawing.Imaging.ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);
                            ImageCodecInfo jpgEncoder = GetEncoder(urlGambar.Split('.').Last() != "png" ? ImageFormat.Jpeg : ImageFormat.Png);

                            System.Drawing.Imaging.Encoder myEncoder =
                                System.Drawing.Imaging.Encoder.Quality;
                            System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);

                            System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, 90L);
                            myEncoderParameters.Param[0] = myEncoderParameter;

                            var resizedStream = new System.IO.MemoryStream();
                            resizedImage.Save(resizedStream, jpgEncoder, myEncoderParameters);
                            resizedStream.Position = 0;
                            byte[] resizedByteArr = resizedStream.ToArray();
                            //end change by calvin 1 maret 2019
                            resizedStream.Dispose();

                            if (string.IsNullOrWhiteSpace(idGambar))
                            {
                                idGambar = Convert.ToString(bytes.Length);
                            }
                            //if (!uploadedImageID.Contains(idGambar))
                            //{
                            //    uploadedImageID.Add(idGambar);
                            images.Add(idGambar, Convert.ToBase64String(resizedByteArr)); // size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                            images_pervar.Add(idGambar);
                            //}
                        }
                    }
                    //}
                }
                idGambar = stf02h.ACODE_49;
                urlGambar = stf02h.AVALUE_49;
                //if (string.IsNullOrWhiteSpace(idGambar))
                if (string.IsNullOrWhiteSpace(urlGambar))
                {
                    idGambar = data.dataBarangInDb.Sort6;
                    urlGambar = data.dataBarangInDb.LINK_GAMBAR_2;
                }
                idGambar = stf02h.BRG + "_2_" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
                //if (!string.IsNullOrWhiteSpace(idGambar))
                if (!string.IsNullOrWhiteSpace(urlGambar))
                {
                    //if (!uploadedImageID.Contains(idGambar))
                    //{
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(urlGambar);

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
                            //System.Drawing.Imaging.ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);
                            ImageCodecInfo jpgEncoder = GetEncoder(urlGambar.Split('.').Last() != "png" ? ImageFormat.Jpeg : ImageFormat.Png);

                            System.Drawing.Imaging.Encoder myEncoder =
                                System.Drawing.Imaging.Encoder.Quality;
                            System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);

                            System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, 90L);
                            myEncoderParameters.Param[0] = myEncoderParameter;

                            var resizedStream = new System.IO.MemoryStream();
                            resizedImage.Save(resizedStream, jpgEncoder, myEncoderParameters);
                            resizedStream.Position = 0;
                            byte[] resizedByteArr = resizedStream.ToArray();
                            //end change by calvin 1 maret 2019
                            resizedStream.Dispose();

                            if (string.IsNullOrWhiteSpace(idGambar))
                            {
                                idGambar = Convert.ToString(bytes.Length);
                            }

                            //if (!uploadedImageID.Contains(idGambar))
                            //{
                            //    uploadedImageID.Add(idGambar);
                            images.Add(idGambar, Convert.ToBase64String(resizedByteArr)); // size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                            images_pervar.Add(idGambar);
                            //}
                        }
                    }
                    //}
                }

                idGambar = stf02h.ACODE_48;
                urlGambar = stf02h.AVALUE_48;
                //if (string.IsNullOrWhiteSpace(idGambar))
                if (string.IsNullOrWhiteSpace(urlGambar))
                {
                    idGambar = data.dataBarangInDb.Sort7;
                    urlGambar = data.dataBarangInDb.LINK_GAMBAR_3;
                }
                idGambar = stf02h.BRG + "_3_" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
                if (!string.IsNullOrWhiteSpace(urlGambar))
                //if (!string.IsNullOrWhiteSpace(idGambar))
                {
                    //if (!uploadedImageID.Contains(idGambar))
                    //{
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(urlGambar);

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
                            //System.Drawing.Imaging.ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);
                            ImageCodecInfo jpgEncoder = GetEncoder(urlGambar.Split('.').Last() != "png" ? ImageFormat.Jpeg : ImageFormat.Png);

                            System.Drawing.Imaging.Encoder myEncoder =
                                System.Drawing.Imaging.Encoder.Quality;
                            System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);

                            System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, 90L);
                            myEncoderParameters.Param[0] = myEncoderParameter;

                            var resizedStream = new System.IO.MemoryStream();
                            resizedImage.Save(resizedStream, jpgEncoder, myEncoderParameters);
                            resizedStream.Position = 0;
                            byte[] resizedByteArr = resizedStream.ToArray();
                            //end change by calvin 1 maret 2019
                            resizedStream.Dispose();

                            if (string.IsNullOrWhiteSpace(idGambar))
                            {
                                idGambar = Convert.ToString(bytes.Length);
                            }

                            //if (!uploadedImageID.Contains(idGambar))
                            //{
                            //    uploadedImageID.Add(idGambar);
                            images.Add(idGambar, Convert.ToBase64String(resizedByteArr)); // size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                            images_pervar.Add(idGambar);
                            //}
                        }
                    }
                    //}
                }
#region 6/9/2019, 5 gambar

                idGambar = stf02h.SIZE_GAMBAR_4;
                urlGambar = stf02h.LINK_GAMBAR_4;
                //if (string.IsNullOrWhiteSpace(idGambar))
                if (string.IsNullOrWhiteSpace(urlGambar))
                {
                    idGambar = data.dataBarangInDb.SIZE_GAMBAR_4;
                    urlGambar = data.dataBarangInDb.LINK_GAMBAR_4;
                }
                idGambar = stf02h.BRG + "_4_" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
                //if (!string.IsNullOrWhiteSpace(idGambar))
                if (!string.IsNullOrWhiteSpace(urlGambar))
                {
                    //if (!uploadedImageID.Contains(idGambar))
                    //{
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(urlGambar);

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
                            //System.Drawing.Imaging.ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);
                            ImageCodecInfo jpgEncoder = GetEncoder(urlGambar.Split('.').Last() != "png" ? ImageFormat.Jpeg : ImageFormat.Png);

                            System.Drawing.Imaging.Encoder myEncoder =
                                System.Drawing.Imaging.Encoder.Quality;
                            System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);

                            System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, 90L);
                            myEncoderParameters.Param[0] = myEncoderParameter;

                            var resizedStream = new System.IO.MemoryStream();
                            resizedImage.Save(resizedStream, jpgEncoder, myEncoderParameters);
                            resizedStream.Position = 0;
                            byte[] resizedByteArr = resizedStream.ToArray();
                            //end change by calvin 1 maret 2019
                            resizedStream.Dispose();

                            if (string.IsNullOrWhiteSpace(idGambar))
                            {
                                idGambar = Convert.ToString(bytes.Length);
                            }

                            //if (!uploadedImageID.Contains(idGambar))
                            //{
                            //    uploadedImageID.Add(idGambar);
                            images.Add(idGambar, Convert.ToBase64String(resizedByteArr)); // size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                            images_pervar.Add(idGambar);
                            //}
                        }
                    }
                    //}
                }

                idGambar = stf02h.SIZE_GAMBAR_5;
                urlGambar = stf02h.LINK_GAMBAR_5;
                //if (string.IsNullOrWhiteSpace(idGambar))
                if (string.IsNullOrWhiteSpace(urlGambar))
                {
                    idGambar = data.dataBarangInDb.SIZE_GAMBAR_5;
                    urlGambar = data.dataBarangInDb.LINK_GAMBAR_5;
                }
                idGambar = stf02h.BRG + "_5_" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
                //if (!string.IsNullOrWhiteSpace(idGambar))
                if (!string.IsNullOrWhiteSpace(urlGambar))
                {
                    //if (!uploadedImageID.Contains(idGambar))
                    //{
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(urlGambar);

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
                            //System.Drawing.Imaging.ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);
                            ImageCodecInfo jpgEncoder = GetEncoder(urlGambar.Split('.').Last() != "png" ? ImageFormat.Jpeg : ImageFormat.Png);

                            System.Drawing.Imaging.Encoder myEncoder =
                                System.Drawing.Imaging.Encoder.Quality;
                            System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);

                            System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, 90L);
                            myEncoderParameters.Param[0] = myEncoderParameter;

                            var resizedStream = new System.IO.MemoryStream();
                            resizedImage.Save(resizedStream, jpgEncoder, myEncoderParameters);
                            resizedStream.Position = 0;
                            byte[] resizedByteArr = resizedStream.ToArray();
                            //end change by calvin 1 maret 2019
                            resizedStream.Dispose();

                            if (string.IsNullOrWhiteSpace(idGambar))
                            {
                                idGambar = Convert.ToString(bytes.Length);
                            }

                            //if (!uploadedImageID.Contains(idGambar))
                            //{
                            //    uploadedImageID.Add(idGambar);
                            images.Add(idGambar, Convert.ToBase64String(resizedByteArr)); // size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                            images_pervar.Add(idGambar);
                            //}
                        }
                    }
                    //}
                }
#endregion
                Dictionary<string, string[]> DefiningAttributes = new Dictionary<string, string[]>();
                Dictionary<string, string> attributeMap = new Dictionary<string, string>();
                //for (int a = 0; a < dsVariasi.Tables[0].Rows.Count; a++)
                //{
                //    List<string> dsVariasiValues = new List<string>();
                //    string A_CODE = Convert.ToString(dsVariasi.Tables[0].Rows[a]["CATEGORY_CODE"]);
                //    string A_VALUE = Convert.ToString(dsVariasi.Tables[0].Rows[a]["VALUE"]);
                //    if (!dsVariasiValues.Contains(A_VALUE))
                //    {
                //        dsVariasiValues.Add(A_VALUE);
                //    }

                //    if (!DefiningAttributes.ContainsKey(A_CODE))
                //    {
                //        DefiningAttributes.Add(A_CODE, dsVariasiValues.ToArray());
                //    }

                //    attributeMap.Add(A_CODE, A_VALUE);
                //}
                //change 9 juli 2020, ambil 35 attribute
                //for (int i = 1; i <= 30; i++)
                for (int i = 1; i <= 35; i++)
                //end change 9 juli 2020, ambil 35 attribute
                {
                    string attribute_id = Convert.ToString(stf02h["ACODE_" + i.ToString()]);
                    string value = Convert.ToString(stf02h["AVALUE_" + i.ToString()]);
                    string aname = Convert.ToString(stf02h["ANAME_" + i.ToString()]);//add 30 april 2020, get attr name
                    if (!string.IsNullOrWhiteSpace(attribute_id))
                    {
                        if (dsVariasi.Contains(attribute_id))
                        {
                            List<string> dsVariasiValues = new List<string>();
                            if (!dsVariasiValues.Contains(value))
                            {
                                dsVariasiValues.Add(value);
                            }

                            if (!DefiningAttributes.ContainsKey(attribute_id))
                            {
                                //if(aname != "Family Colour")//filter family color sementara karena validasi baru di blibli
                                DefiningAttributes.Add(attribute_id, dsVariasiValues.ToArray());
                            }
                            if (aname != "Family Colour")//filter family color sementara karena validasi baru di blibli
                                attributeMap.Add(attribute_id, value);
                        }
                    }
                }

                Productitem newVarItem = new Productitem()
                {
                    //change sementara, perubahan ketentuan UPC di blibli
                    //upcCode = data.dataBarangInDb.BRG,
                    upcCode = "",
                    //end change sementara, perubahan ketentuan UPC di blibli
                    merchantSku = data.dataBarangInDb.BRG,
                    price = Convert.ToInt32(Convert.ToDouble(data.Price)),
                    salePrice = Convert.ToInt32(stf02h.HJUAL),
                    minimumStock = Convert.ToInt32(data.dataBarangInDb.MINI),
                    //stock = Convert.ToInt32(data.dataBarangInDb.MINI),
                    stock = 0,
                    buyable = true,
                    displayable = true,
                    dangerousGoodsLevel = 0,
                    images = images_pervar.ToArray(),
                    attributesMap = attributeMap
                };

                //add by Tri, 1 july 2021
                if ((stf02h.HARGA_NORMAL ?? 0) > 0)
                {
                    newVarItem.price = Convert.ToInt32(stf02h.HARGA_NORMAL);
                }
                //end add by Tri, 1 july 2021

                //add by calvin 15 agustus 2019

                //change by nurul 19/1/2022
                //var qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(data.dataBarangInDb.BRG, "ALL");
                var multilokasi = ErasoftDbContext.SIFSYS_TAMBAHAN.FirstOrDefault().MULTILOKASI;
                double qty_stock = 1;
                if (multilokasi == "1")
                {
                    qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A_MultiLokasi(data.dataBarangInDb.BRG, "ALL", log_CUST);
                }
                else
                {
                    qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(data.dataBarangInDb.BRG, "ALL");
                }
                //end change by nurul 19/1/2022

                if (qty_stock > 0)
                {
                    newVarItem.stock = Convert.ToInt32(qty_stock);
                }
                //end add by calvin 15 agustus 2019

                productItems.Add(newVarItem);
                newData.productDefiningAttributes = DefiningAttributes;
            }
#endregion
#region Barang Variasi
            else
            {
                Dictionary<string, string> DefiningDariStf02H = new Dictionary<string, string>();

                var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == data.kode).ToList();
                var var_stf02_listbrg = var_stf02.Select(p => p.BRG).ToList();
                var var_stf02h = ErasoftDbContext.STF02H.Where(p => var_stf02_listbrg.Contains(p.BRG) && p.IDMARKET == arf01.RecNum).ToList();
                var var_stf02i = ErasoftDbContext.STF02I.Where(p => p.BRG == data.kode && p.MARKET == "BLIBLI").ToList().OrderBy(p => p.RECNUM);

                Dictionary<string, string[]> DefiningAttributes = new Dictionary<string, string[]>();
                //for (int a = 0; a < dsVariasi.Tables[0].Rows.Count; a++)
                //{
                //    string A_CODE = Convert.ToString(dsVariasi.Tables[0].Rows[a]["CATEGORY_CODE"]);
                //    string A_VALUE = Convert.ToString(dsVariasi.Tables[0].Rows[a]["VALUE"]);

                //    List<string> dsVariasiValues = new List<string>();
                //    var var_stf02i_distinct = var_stf02i.Where(p => p.MP_JUDUL_VAR == A_CODE).ToList().OrderBy(p => p.RECNUM);
                //    foreach (var v in var_stf02i_distinct)
                //    {
                //        if (!dsVariasiValues.Contains(v.MP_VALUE_VAR))
                //        {
                //            dsVariasiValues.Add(v.MP_VALUE_VAR);
                //        }
                //    }

                //    //add by calvin 26 februari, kasus pak rocky, masing" warna 1 sku induk
                //    //maka ambil value untuk attribute tersebut dari stf02h, ( diisi pada bagian detail per marketplace ).
                //    if (var_stf02i_distinct.Count() == 0)
                //    {
                //        if (A_CODE == "WA-0000002") // Warna
                //        {
                //            ValueVariasiWarna = A_VALUE;
                //            dsVariasiValues.Add(A_VALUE);
                //        }
                //    }
                //    //end add by calvin 26 februari, kasus pak rocky, masing" warna 1 sku induk

                //    if (!DefiningAttributes.ContainsKey(A_CODE))
                //    {
                //        DefiningAttributes.Add(A_CODE, dsVariasiValues.ToArray());
                //    }
                //}
                //change 9 juli 2020, ambil 35 attribute
                //for (int i = 1; i <= 30; i++)
                List<string> dsVariasiFCValues = new List<string>();//untuk menampung family color
                List<string> dsVariasiFCValuesInduk = new List<string>();//untuk menampung family color induk
                for (int i = 1; i <= 35; i++)
                //end change 9 juli 2020, ambil 35 attribute
                {
                    string attribute_id = Convert.ToString(stf02h["ACODE_" + i.ToString()]);
                    string value = Convert.ToString(stf02h["AVALUE_" + i.ToString()]);
                    string aname = Convert.ToString(stf02h["ANAME_" + i.ToString()]);//add 30 april 2020, get attr name
                    if (!string.IsNullOrWhiteSpace(attribute_id))
                    {
                        if (dsVariasi.Contains(attribute_id))
                        {
                            List<string> dsVariasiValues = new List<string>();
                            var var_stf02i_distinct = var_stf02i.Where(p => p.MP_JUDUL_VAR == attribute_id).ToList().OrderBy(p => p.RECNUM);
                            foreach (var v in var_stf02i_distinct)
                            {
                                if (!dsVariasiValues.Contains(v.MP_VALUE_VAR))
                                {
                                    dsVariasiValues.Add(v.MP_VALUE_VAR);
                                    //if (v.MP_JUDUL_VAR == "WA-0000002")//add family color jika ada attribute warna
                                    if (aname == "Warna")//add family color jika ada attribute warna
                                    {
                                        dsVariasiFCValues.Add(v.MP_VALUE_FC_VAR);
                                    }
                                }
                            }

                            //add by calvin 26 februari, kasus pak rocky, masing" warna 1 sku induk
                            //maka ambil value untuk attribute tersebut dari stf02h, ( diisi pada bagian detail per marketplace ).
                            if (var_stf02i_distinct.Count() == 0)
                            {
                                if (aname != "Family Colour")//filter family color sementara karena validasi baru di blibli
                                {
                                    DefiningDariStf02H.Add(attribute_id, value);
                                    //if (attribute_id == "WA-0000002") // Warna
                                    //{
                                    //    ValueVariasiWarna = value;
                                    dsVariasiValues.Add(value);
                                    //}
                                }
                            }
                            //end add by calvin 26 februari, kasus pak rocky, masing" warna 1 sku induk

                            if (!DefiningAttributes.ContainsKey(attribute_id))
                            {
                                if (aname != "Family Colour")//filter family color sementara karena validasi baru di blibli
                                    DefiningAttributes.Add(attribute_id, dsVariasiValues.ToArray());
                            }

                            if(attribute_id == "FA-2000060")
                            {
                                dsVariasiFCValuesInduk.Add(value);
                            }
                        }
                    }
                }

                if (dsVariasiFCValues.Count > 0)//masukan attribute family color kalau ada isinya
                {
                    DefiningAttributes.Add("FA-2000060", dsVariasiFCValues.ToArray());
                }

                if (!DefiningAttributes.ContainsKey("FA-2000060") && dsVariasiFCValuesInduk.Count > 0)//belum ada familu color dan attr ada di brg induk
                {
                    DefiningAttributes.Add("FA-2000060", dsVariasiFCValuesInduk.ToArray());

                }
                newData.productDefiningAttributes = DefiningAttributes;

                foreach (var var_item in var_stf02)
                {
                    var var_stf02h_item = var_stf02h.Where(p => p.BRG == var_item.BRG).FirstOrDefault();

                    List<string> images_pervar = new List<string>();
                    string image_id = var_stf02h_item.ACODE_50;
                    if (string.IsNullOrWhiteSpace(image_id))
                    {
                        image_id = var_item.Sort5;
                    }
                    string url = var_stf02h_item.AVALUE_50;
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        url = var_item.LINK_GAMBAR_1;
                    }
                    image_id = "_1_" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
                    //if (!string.IsNullOrWhiteSpace(image_id))
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        //if (!uploadedImageID.Contains(image_id))
                        //{
                        using (var client = new HttpClient())
                        {
                            //string url = var_stf02h_item.AVALUE_50;
                            //if (string.IsNullOrWhiteSpace(url))
                            //{
                            //    url = var_item.LINK_GAMBAR_1;
                            //}
                            //var bytes = await client.GetByteArrayAsync(var_item.LINK_GAMBAR_1);
                            var bytes = await client.GetByteArrayAsync(url);

                            //images.Add(var_item.Sort5, Convert.ToBase64String(bytes));// size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
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
                                //System.Drawing.Imaging.ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);
                                ImageCodecInfo jpgEncoder = GetEncoder(url.Split('.').Last() != "png" ? ImageFormat.Jpeg : ImageFormat.Png);

                                System.Drawing.Imaging.Encoder myEncoder =
                                    System.Drawing.Imaging.Encoder.Quality;
                                System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);

                                System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, 90L);
                                myEncoderParameters.Param[0] = myEncoderParameter;

                                var resizedStream = new System.IO.MemoryStream();
                                resizedImage.Save(resizedStream, jpgEncoder, myEncoderParameters);
                                resizedStream.Position = 0;
                                byte[] resizedByteArr = resizedStream.ToArray();
                                //end change by calvin 1 maret 2019
                                resizedStream.Dispose();

                                //images.Add(var_item.Sort5, Convert.ToBase64String(resizedByteArr));// size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                                if (string.IsNullOrWhiteSpace(image_id))
                                {
                                    image_id = Convert.ToString(bytes.Length);
                                }
                                //if (!uploadedImageID.Contains(image_id))
                                //{
                                //    uploadedImageID.Add(image_id);
                                images.Add(var_item.BRG + image_id, Convert.ToBase64String(resizedByteArr));// size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                                images_pervar.Add(var_item.BRG + image_id); // size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                                                                            //}
                            }
                        }
                        //}
                    }
#region 6/9/2019, barang varian 2 gambar
                    //image_id = var_stf02h_item.ACODE_49;
                    //if (string.IsNullOrWhiteSpace(image_id))
                    //{
                    //    image_id = var_item.Sort6;
                    //}
                    //if (!string.IsNullOrWhiteSpace(image_id))
                    //{
                    //    if (!uploadedImageID.Contains(image_id))
                    //    {
                    //        using (var client = new HttpClient())
                    //        {
                    //            string url = var_stf02h_item.AVALUE_49;
                    //            if (string.IsNullOrWhiteSpace(url))
                    //            {
                    //                url = var_item.LINK_GAMBAR_2;
                    //            }
                    //            //var bytes = await client.GetByteArrayAsync(var_item.LINK_GAMBAR_1);
                    //            var bytes = await client.GetByteArrayAsync(url);

                    //            //images.Add(var_item.Sort5, Convert.ToBase64String(bytes));// size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                    //            using (var stream = new MemoryStream(bytes, true))
                    //            {
                    //                var img = Image.FromStream(stream);
                    //                float newResolution = img.Height;
                    //                if (img.Width < newResolution)
                    //                {
                    //                    newResolution = img.Width;
                    //                }
                    //                var resizedImage = (Image)BlibliResizeImage(img, Convert.ToInt32(newResolution), Convert.ToInt32(newResolution));
                    //                //var resizedImage = (Image)BlibliResizeImageFromStream(stream);

                    //                //change by calvin 1 maret 2019
                    //                //ImageConverter _imageConverter = new ImageConverter();
                    //                //byte[] resizedByteArr = (byte[])_imageConverter.ConvertTo(resizedImage, typeof(byte[]));
                    //                System.Drawing.Imaging.ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);

                    //                System.Drawing.Imaging.Encoder myEncoder =
                    //                    System.Drawing.Imaging.Encoder.Quality;
                    //                System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);

                    //                System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, 90L);
                    //                myEncoderParameters.Param[0] = myEncoderParameter;

                    //                var resizedStream = new System.IO.MemoryStream();
                    //                resizedImage.Save(resizedStream, jpgEncoder, myEncoderParameters);
                    //                resizedStream.Position = 0;
                    //                byte[] resizedByteArr = resizedStream.ToArray();
                    //                //end change by calvin 1 maret 2019
                    //                resizedStream.Dispose();

                    //                //images.Add(var_item.Sort5, Convert.ToBase64String(resizedByteArr));// size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                    //                if (string.IsNullOrWhiteSpace(image_id))
                    //                {
                    //                    image_id = Convert.ToString(bytes.Length);
                    //                }
                    //                if (!uploadedImageID.Contains(image_id))
                    //                {
                    //                    uploadedImageID.Add(image_id);
                    //                    images.Add(image_id, Convert.ToBase64String(resizedByteArr));// size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                    //                    images_pervar.Add(image_id); // size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
#endregion
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
                        //add by calvin 26 februari, kasus pak rocky, masing" warna 1 sku induk
                        //if (!attributeMap.ContainsKey("WA-0000002"))
                        //{
                        //    attributeMap.Add("WA-0000002", ValueVariasiWarna);
                        //}
                        if (DefiningDariStf02H.Count() > 0)
                        {
                            foreach (var item in DefiningDariStf02H)
                            {
                                if (!attributeMap.ContainsKey(item.Key))
                                {
                                    attributeMap.Add(item.Key, item.Value);
                                }
                            }
                        }
                        //end add by calvin 26 februari, kasus pak rocky, masing" warna 1 sku induk
                    }
                    else if (!string.IsNullOrWhiteSpace(var_item.Sort9))
                    {
                        var var_stf02i_judul_mp_var_2 = var_stf02i.Where(p => p.KODE_VAR == var_item.Sort9 && p.LEVEL_VAR == 2).FirstOrDefault();
                        if (var_stf02i_judul_mp_var_2 != null)
                        {
                            attributeMap.Add(var_stf02i_judul_mp_var_2.MP_JUDUL_VAR, var_stf02i_judul_mp_var_2.MP_VALUE_VAR);
                        }
                        if (DefiningDariStf02H.Count() > 0)
                        {
                            foreach (var item in DefiningDariStf02H)
                            {
                                if (!attributeMap.ContainsKey(item.Key))
                                {
                                    attributeMap.Add(item.Key, item.Value);
                                }
                            }
                        }
                    }

                    Productitem newVarItem = new Productitem()
                    {
                        //change sementara, perubahan ketentuan UPC di blibli
                        //upcCode = var_item.BRG,
                        upcCode = "",
                        //change sementara, perubahan ketentuan UPC di blibli
                        merchantSku = var_item.BRG,
                        price = Convert.ToInt32(var_item.HJUAL),
                        salePrice = Convert.ToInt32(var_stf02h_item.HJUAL),
                        minimumStock = Convert.ToInt32(var_item.MINI),
                        //stock = Convert.ToInt32(var_item.MINI),
                        stock = 0,
                        buyable = true,
                        displayable = true,
                        dangerousGoodsLevel = 0,
                        images = images_pervar.ToArray(),
                        attributesMap = attributeMap
                    };
                    //add by Tri, 1 july 2021
                    if((var_stf02h_item.HARGA_NORMAL ?? 0) > 0)
                    {
                        newVarItem.price = Convert.ToInt32(var_stf02h_item.HARGA_NORMAL);
                    }
                    //end add by Tri, 1 july 2021

                    //add by calvin 15 agustus 2019

                    //change by nurul 19/1/2022
                    //var qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(var_item.BRG, "ALL");
                    var multilokasi = ErasoftDbContext.SIFSYS_TAMBAHAN.FirstOrDefault().MULTILOKASI;
                    double qty_stock = 1;
                    if (multilokasi == "1")
                    {
                        qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A_MultiLokasi(var_item.BRG, "ALL", log_CUST);
                    }
                    else
                    {
                        qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(var_item.BRG, "ALL");
                    }
                    //end change by nurul 19/1/2022

                    if (qty_stock > 0)
                    {
                        newVarItem.stock = Convert.ToInt32(qty_stock);
                    }
                    //end add by calvin 15 agustus 2019
                    productItems.Add(newVarItem);
                }
            }
#endregion

            newData.productItems = (productItems);
            newData.imageMap = images;
            newData.uniqueSellingPoint = Convert.ToBase64String(Encoding.ASCII.GetBytes(System.Net.WebUtility.HtmlDecode(Convert.ToString(stf02h["AVALUE_39"]))));

            string myData = JsonConvert.SerializeObject(newData);

            //myData = myData.Replace("\\r\\n", "\\n").Replace("–", "-").Replace("\\\"\\\"", "").Replace("×", "x");

            //change by nurul 13/7/2020
            //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/createProduct", iden.API_secret_key);
            //urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/createProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&username=" + Uri.EscapeDataString(userMTA) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&channelId=MasterOnline";
            string signature = "";
            if (iden.versiToken != "2")
            {
                signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/createProduct", iden.API_secret_key);
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/createProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&username=" + Uri.EscapeDataString(userMTA) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&channelId=MasterOnline";
            }
            else
            {
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/createProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&username=" + Uri.EscapeDataString(userMTA) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&channelId=MasterOnline";
            }
            //end change by nurul 13/7/2020
#if (DEBUG || Debug_AWS)
            string jobId = "";
#else
            string jobId = context.BackgroundJob.Id;
#endif
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Create Product",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = data.kode,
                REQUEST_ATTRIBUTE_2 = data.nama,
                REQUEST_ATTRIBUTE_3 = jobId, //hangfire job id ( create product )
                //REQUEST_ATTRIBUTE_5 = "BLIBLI_CPRODUCT",//add by Tri 19 Des 2019, agar log create brg blibli tidak terhapus
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            //change by nurul 13/7/2020
            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //myReq.Method = "POST";
            //myReq.Headers.Add("Authorization", ("bearer " + iden.token));
            //myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            //myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            //myReq.Accept = "application/json";
            //myReq.ContentType = "application/json";
            //myReq.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
            //myReq.Headers.Add("sessionId", milis.ToString());
            //myReq.Headers.Add("username", userMTA);
            myReq = (HttpWebRequest)WebRequest.Create(urll);
            if (iden.versiToken != "2")
            {
                myReq.Method = "POST";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                string passMO = iden.API_client_password;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                myReq.Method = "POST";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020

            string responseFromServer = "";
            //try
            //{
            var byteData = Encoding.UTF8.GetBytes(myData);
            myReq.ContentLength = byteData.Length;
            using (var dataStream = myReq.GetRequestStream())
            {
                //dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
                dataStream.Write(byteData, 0, byteData.Length);
            }
            using (WebResponse response = myReq.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseFromServer = reader.ReadToEnd();
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}

            if (responseFromServer != "")
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(CreateProductResult)) as CreateProductResult;
                if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                {
                    string EDBConnID = EDB.GetConnectionString("ConnId");
                    var sqlStorage = new SqlServerStorage(EDBConnID);

                    var client = new BackgroundJobClient(sqlStorage);
                    //INSERT QUEUE FEED
#if (DEBUG || Debug_AWS)
                    await CreateProductSuccess_1(dbPathEra, kodeProduk, log_CUST, "Barang", "Buat Produk (Tahap 2 / 3)", iden, Convert.ToString(data.kode), Convert.ToString(result.value.queueFeedId), Convert.ToString(milis));
#else
                    client.Enqueue<BlibliControllerJob>(x => x.CreateProductSuccess_1(dbPathEra, kodeProduk, log_CUST, "Barang", "Buat Produk (Tahap 2 / 3)", iden, Convert.ToString(data.kode), Convert.ToString(result.value.queueFeedId), Convert.ToString(milis)));
#endif
                    //client.Enqueue<BlibliControllerJob>(x => x.CreateProductSuccess_2(dbPathEra, kodeProduk, log_CUST, "Barang", "Buat Produk (Tahap 2 / 3)", iden, Convert.ToString(data.kode), Convert.ToString(result.value.queueFeedId), Convert.ToString(milis)));
                }
                else
                {
                    throw new Exception(Convert.ToString(result.errorCode));

                    //currentLog.REQUEST_EXCEPTION = Convert.ToString(result.errorCode);
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                }
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Blibli Gagal.")]
        public async Task<string> CreateProductSuccess_1(string dbPathEra, string namaPemesan, string log_CUST, string log_ActionCategory, string log_ActionName, BlibliAPIData iden, string data_kode, string result_value_queueFeedId, string milis)
        {
            var token = SetupContext(iden);
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

                    //try
                    //{
                    oCommand.Parameters[0].Value = result_value_queueFeedId;
                    oCommand.Parameters[1].Value = iden.merchant_code;
                    oCommand.Parameters[2].Value = milis;

                    if (oCommand.ExecuteNonQuery() == 1)
                    {
                        string EDBConnID = EDB.GetConnectionString("ConnId");
                        var sqlStorage = new SqlServerStorage(EDBConnID);

                        var client = new BackgroundJobClient(sqlStorage);

                        //INSERT QUEUE FEED

#if (DEBUG || Debug_AWS)
                        await CreateProductSuccess_2(dbPathEra, namaPemesan, log_CUST, "Barang", "Buat Produk (Tahap 3 / 3)", iden, (data_kode), (result_value_queueFeedId), milis);
#else
                    client.Enqueue<BlibliControllerJob>(x => x.CreateProductSuccess_2(dbPathEra, namaPemesan, log_CUST, "Barang", "Buat Produk (Tahap 3 / 3)", iden, (data_kode), (result_value_queueFeedId), milis));
#endif
                    }
                    else
                    {
                        throw new Exception("Create Product store queue feed.");
                    }
                    //oTransaction.Commit();
                    //}
                    //catch (Exception ex)
                    //{
                    //    //oTransaction.Rollback();
                    //}
                }
                //}
            }
            return "";
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Blibli Gagal.")]
        public async Task<string> CreateProductSuccess_2(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, BlibliAPIData iden, string data_kode, string result_value_queueFeedId, string milis)
        {
            var token = SetupContext(iden);
            using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
            {
                oConnection.Open();

                using (SqlCommand oCommand = oConnection.CreateCommand())
                {
                    //oCommand.CommandText = "DELETE FROM [CATEGORY_BLIBLI] WHERE ARF01_SORT1_CUST='" + data.merchant_code + "'";
                    //oCommand.ExecuteNonQuery();
                    //oCommand.Transaction = oTransaction;
                    oCommand.CommandType = CommandType.Text;
                    string Link_Error = "0;Buat Produk;;";//jobid;request_action;request_result;request_exception
                    oCommand.CommandText = "UPDATE H SET BRG_MP='PENDING',LINK_STATUS='Buat Produk Pending', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '" + Link_Error + "' FROM STF02H H INNER JOIN ARF01 A ON H.IDMARKET = A.RECNUM WHERE H.BRG=@MERCHANTSKU AND A.SORT1_CUST=@MERCHANTCODE";
                    //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                    oCommand.Parameters.Add(new SqlParameter("@REQUESTID", SqlDbType.NVarChar, 50));
                    oCommand.Parameters.Add(new SqlParameter("@MERCHANTCODE", SqlDbType.NVarChar, 50));
                    oCommand.Parameters.Add(new SqlParameter("@LOG_REQUEST_ID", SqlDbType.NVarChar, 50));
                    oCommand.Parameters.Add(new SqlParameter("@MERCHANTSKU", SqlDbType.NVarChar, 20));
                    oCommand.Parameters[0].Value = result_value_queueFeedId;
                    oCommand.Parameters[1].Value = iden.merchant_code;
                    oCommand.Parameters[2].Value = milis;
                    oCommand.Parameters[3].Value = Convert.ToString(data_kode);

                    if (oCommand.ExecuteNonQuery() == 1)
                    {
                        //BlibliQueueFeedData queueData = new BlibliQueueFeedData
                        //{
                        //    request_id = result_value_queueFeedId,
                        //    log_request_id = milis
                        //};

                        //string EDBConnID = EDB.GetConnectionString("ConnId");
                        //var sqlStorage = new SqlServerStorage(EDBConnID);

                        //var client = new BackgroundJobClient(sqlStorage);

                        //client.Enqueue<BlibliControllerJob>(x => x.CreateProductGetQueueFeedDetail(dbPathEra, kodeProduk, log_CUST, "Barang", "Link Produk (Tahap 1 / 2)", iden, queueData));
                    }
                    else
                    {
                        throw new Exception("Create Product set status pending failure.");
                    }
                }
            }
            return "";
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Blibli Berhasil. Cek review gagal.")]
        public async Task<string> CreateProductGetProdukInReviewList(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, BlibliAPIData iden, string requestID, string ProductCode, string gdnSku, string api_log_requestId)
        {
            long milis = CurrentTimeMillis();

            var token = SetupContext(iden);
            iden.token = token;

            string productCodeBlibli = "";
            try
            {
                var aProductCode = JsonConvert.DeserializeObject(ProductCode, typeof(BlibliGetQueueProductCode)) as BlibliGetQueueProductCode;
                if (aProductCode != null)
                {
                    if (!string.IsNullOrEmpty(aProductCode.productCode))
                    {
                        productCodeBlibli = aProductCode.productCode;
                    }
                    else if (!string.IsNullOrEmpty(aProductCode.newProductCode))//need correction
                    {
                        productCodeBlibli = aProductCode.newProductCode;
                    }
                }

            }
            catch (Exception ex)
            {

            }

            if (iden.versiToken == "2")
            {
                var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == log_CUST).FirstOrDefault();
                var resCek = cekProductQC(productCodeBlibli, iden, customer.RecNum ?? 0, kodeProduk, 0);
                if (resCek.status == 1)//need correction
                {
                    using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                    {
                        oConnection.Open();
                        using (SqlCommand oCommand = oConnection.CreateCommand())
                        {
                            oCommand.CommandType = CommandType.Text;
                            oCommand.CommandText = "UPDATE [QUEUE_FEED_BLIBLI] SET [STATUS] = 0 WHERE REQUESTID = @REQUESTID ";
                            oCommand.Parameters.Add(new SqlParameter("@REQUESTID", SqlDbType.NVarChar, 50));
                            oCommand.Parameters[0].Value = requestID;
                            oCommand.ExecuteNonQuery();
                        }
                    }
                    string errorLog = "Data Produk ada yang perlu diperbaiki. Silahkan edit di Master Barang.";
                    if (!string.IsNullOrEmpty(resCek.message))
                    {
                        errorLog += " Revision Notes : " + resCek.message;
                    }
                    throw new Exception(errorLog);
                }
            }


            //string productCodeBlibli = "";
            //try
            //{
            //    var aProductCode = JsonConvert.DeserializeObject(ProductCode, typeof(BlibliGetQueueProductCode)) as BlibliGetQueueProductCode;
            //    if(aProductCode != null)
            //    {
            //        if (!string.IsNullOrEmpty(aProductCode.productCode))
            //        {
            //            productCodeBlibli = aProductCode.productCode;
            //        }
            //        else if (!string.IsNullOrEmpty(aProductCode.newProductCode))//need correction
            //        {
            //            productCodeBlibli = aProductCode.newProductCode;
            //        }
            //    }

            //}
            //catch (Exception ex)
            //{

            //}

            if (!string.IsNullOrEmpty(productCodeBlibli))
            {

                var productInProcess = await cekProdukInProcess(iden, productCodeBlibli, 0);
                if (productInProcess.status == 1)//product masih di qc
                {
                    return "";
                }
            }
            //remark 19 Des 2019
            //DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            //string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            //string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            //string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/inProcessProduct", iden.API_secret_key);
            //string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/inProcessProduct?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&size=100&channelId=MasterOnline";

            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //myReq.Method = "GET";
            //myReq.Headers.Add("Authorization", ("bearer " + iden.token));
            //myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            //myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            //myReq.Accept = "application/json";
            //myReq.ContentType = "application/json";
            //myReq.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
            //myReq.Headers.Add("sessionId", milis.ToString());
            //myReq.Headers.Add("username", userMTA);
            //string responseFromServer = "";
            //using (WebResponse response = await myReq.GetResponseAsync())
            //{
            //    using (Stream stream = response.GetResponseStream())
            //    {
            //        StreamReader reader = new StreamReader(stream);
            //        responseFromServer = reader.ReadToEnd();
            //    }
            //}

            //if (responseFromServer != "")
            //{
            //    //perlu tes item tanpa varian
            //    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ProductInReviewListResult)) as ProductInReviewListResult;
            //    if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
            //    {
            //        var foundInReview = false;
            //        foreach (var item in result.content) //cek semua item in review
            //        {
            //            if (item.productItems.Count() > 0)
            //            {
            //                var item_var = item.productItems[0];
            //                if (item_var.upcCode != "-" && !string.IsNullOrWhiteSpace(item_var.upcCode))
            //                {
            //                    if (item_var.upcCode.Contains(kodeProduk))// cari kode product
            //                    {
            //                        foundInReview = true;
            //                        var rowUpdated = EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE STF02H SET LINK_STATUS='Buat Produk dalam proses', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '0;Buat Produk;;' WHERE BRG = '" + kodeProduk + "' AND IDMARKET = '" + iden.idmarket + "' AND LINK_STATUS='Buat Produk Pending'");
            //                    }
            //                }
            //            }
            //        }

            //        if (!foundInReview)
            //        {
            //            //jika tidak ketemu, bisa jadi queue belum di proses
            //            //tetapi, cek link_status, jika in 'Buat Produk dalam proses', berarti barang sudah melewati proses in review, bisa jadi reject / active
            //            var link_status = Convert.ToString(EDB.GetFieldValue("sConn", "STF02H", "BRG = '" + kodeProduk + "' AND IDMARKET = '" + iden.idmarket + "'", "LINK_STATUS"));
            //            if (link_status == "Buat Produk dalam proses")
            //            {
            //end remark 19 Des 2019
            DataSet dsStf02 = EDB.GetDataSet("sCon", "STF02", "SELECT * FROM STF02 WHERE BRG = '" + kodeProduk + "'");
            if (dsStf02.Tables[0].Rows.Count > 0)
            {
                var tipe = Convert.ToString(dsStf02.Tables[0].Rows[0]["TYPE"]);
                if (tipe == "4") //barang induk
                {
                    DataSet dsStf02Variant = EDB.GetDataSet("sCon", "STF02", "SELECT * FROM STF02 WHERE PART = '" + kodeProduk + "'");
                    if (dsStf02Variant.Tables[0].Rows.Count > 0)
                    {
                        List<string> merchantskus = new List<string>();
                        for (int i = 0; i < dsStf02Variant.Tables[0].Rows.Count; i++)
                        {
                            string BRG = Convert.ToString(dsStf02Variant.Tables[0].Rows[i]["BRG"]);
                            merchantskus.Add(BRG);
                            if(merchantskus.Count == 50 || i == dsStf02Variant.Tables[0].Rows.Count - 1)
                            {
#if (DEBUG || Debug_AWS)
                                await CekProductActive(DatabasePathErasoft, kodeProduk, log_CUST, "Barang", "Cek Active/Reject", iden, kodeProduk, merchantskus, log_CUST, requestID, api_log_requestId);

#else
                                        string EDBConnID = EDB.GetConnectionString("ConnId");
                                        var sqlStorage = new SqlServerStorage(EDBConnID);
                                        var client = new BackgroundJobClient(sqlStorage);
                                        client.Enqueue<BlibliControllerJob>(x => x.CekProductActive(DatabasePathErasoft, kodeProduk, log_CUST, "Barang", "Cek Active/Reject", iden, kodeProduk, merchantskus, log_CUST, requestID, api_log_requestId));
#endif
                                merchantskus = new List<string>();
                            }
                        }
//#if (DEBUG || Debug_AWS)
//                        await CekProductActive(DatabasePathErasoft, kodeProduk, log_CUST, "Barang", "Cek Active/Reject", iden, kodeProduk, merchantskus, log_CUST, requestID, api_log_requestId);

//#else
//                                        string EDBConnID = EDB.GetConnectionString("ConnId");
//                                        var sqlStorage = new SqlServerStorage(EDBConnID);
//                                        var client = new BackgroundJobClient(sqlStorage);
//                                        client.Enqueue<BlibliControllerJob>(x => x.CekProductActive(DatabasePathErasoft, kodeProduk, log_CUST, "Barang", "Cek Active/Reject", iden, kodeProduk, merchantskus, log_CUST, requestID, api_log_requestId));
//#endif
                    }
                }
                //change by nurul 14/9/2020, handle barang multi sku juga 
                //else if (tipe == "3")
                else if (tipe == "3" || tipe == "6")
                //change by nurul 14/9/2020, handle barang multi sku juga 
                {
                    List<string> merchantskus = new List<string>();
                    merchantskus.Add(kodeProduk);

#if (DEBUG || Debug_AWS)
                    await CekProductActive(DatabasePathErasoft, kodeProduk, log_CUST, "Barang", "Cek Active/Reject", iden, kodeProduk, merchantskus, log_CUST, requestID, api_log_requestId);
#else
                                    string EDBConnID = EDB.GetConnectionString("ConnId");
                                    var sqlStorage = new SqlServerStorage(EDBConnID);
                                    var client = new BackgroundJobClient(sqlStorage);
                                    client.Enqueue<BlibliControllerJob>(x => x.CekProductActive(DatabasePathErasoft, kodeProduk, log_CUST, "Barang", "Cek Active/Reject", iden, kodeProduk, merchantskus, log_CUST, requestID, api_log_requestId));
#endif


                }
            }
            //remark 19 Des 2019
            //            }
            //        }
            //    }
            //}
            //end remark 19 Des 2019
            return "";
        }
        public async Task<BindingBase> cekProdukInProcess(BlibliAPIData iden, string kodeProduk, int page)
        {
            var ret = new BindingBase
            {
                status = 0,
            };
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/inProcessProduct", iden.API_secret_key);
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/inProcessProduct?requestId=" 
                + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) 
                + "&size=50&channelId=MasterOnline&page=" + page;

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            if (iden.versiToken != "2")
            {
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", "MasterOnline-" + milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(iden.API_client_username + ":" + iden.API_client_password))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            string responseFromServer = "";
            using (WebResponse response = await myReq.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseFromServer = reader.ReadToEnd();
                }
            }
            if (responseFromServer != "")
            {
                //perlu tes item tanpa varian
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ProductInReviewListResult)) as ProductInReviewListResult;
                if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                {
                    foreach (var item in result.content) //cek semua item in review
                    {
                        if(item.productCode == kodeProduk)
                        {
                            ret.status = 1;
                            return ret;
                        }
                    }
                    if(result.content.Length >= 50)
                    {
                        var recur = await cekProdukInProcess(iden, kodeProduk, page +1);
                        if(recur.status == 1)
                        {
                            ret.status = 1;
                            return ret;
                        }
                    }
                }
            }

            return ret;
        }
        public BindingBase cekProductQC(string kodeProduk, BlibliAPIData iden, int idMarket, string brg, int page)
        {

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            var ret = new BindingBase();
            var token = SetupContext(iden);
            iden.token = token;
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant

            var urll = "https://api.blibli.com/v2/proxy/seller/v1/product-submissions/filter?requestId=MasterOnline-" + Uri.EscapeDataString(milis.ToString()) + "&username=" + Uri.EscapeDataString(userMTA) + "&storeCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&channelId=MasterOnline";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            string usernameMO = iden.API_client_username;
            string passMO = iden.API_client_password;

            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
            myReq.Headers.Add("Signature-Time", milis.ToString());

            //string myData = "{ \"filter\" : { \"sellerSku\": \"" + kodeProduk + "\",";
            string myData = "{ \"filter\" : { ";
            myData += "\"state\": \"NEED_CORRECTION\"},";
            myData += "\"paging\": {";
            myData += "\"page\": "+page+", ";
            myData += "\"size\": 100 } ";
            myData += "}";
            string responseFromServer = "";
            //try
            //{
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
            //}
            //catch (Exception ex)
            //{
            //    //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}
            if (!string.IsNullOrEmpty(responseFromServer))
            {

                var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(Blibli_ProductSubmissionList_Response)) as Blibli_ProductSubmissionList_Response;
                if (listBrg != null)
                {
                    if (!string.IsNullOrEmpty(listBrg.errorCode))
                    {
                        throw new Exception(listBrg.errorCode + " : " + listBrg.errorMessage);
                    }
                    else
                    {
                        if (listBrg.content.Length > 0)//ada di need correction
                        {
                            foreach(var data in listBrg.content)
                            {
                                if(data.product.code == kodeProduk)
                                {
                                    EDB.ExecuteSQL("CString", CommandType.Text, "UPDATE STF02H SET BRG_MP = 'NEED_CORRECTION;" + listBrg.content[0].product.code + "' WHERE BRG = '" + brg + "' AND IDMARKET = " + idMarket);
                                    ret.status = 1;
                                    ret.message = data.product.revisionNotes;
                                    return ret;
                                }
                            }

                            if(listBrg.content.Length == 100)
                            {
                                cekProductQC( kodeProduk,  iden,  idMarket,  brg,  page + 1);
                            }
                        }
                    }
                }

            }
            return ret;
        }

        public async Task<BindingBase> getBrandCode(BlibliAPIData data, string brandName)
        {
            var ret = new BindingBase();
            ret.status = 0;
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);// Jan1st1970.AddMilliseconds(Convert.ToDouble(milis)).AddHours(7);

            string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
            string userMTA = data.mta_username_email_merchant;//<-- email user merchant
            string passMTA = data.mta_password_password_merchant;//<-- pass merchant

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/getBrands";
            System.Net.HttpWebRequest myReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //change by nurul 13/7/2020
            if (data.versiToken != "2")
            {
                //string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/product/getBrands", data.API_secret_key);

                //string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/product/getBrands?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline&brands=" + search + "&masterCategoryCode=" + category + "&page=" + pagenumber + "&size=10";
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/getBrands", data.API_secret_key);

                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/getBrands?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline&brandName=" + brandName + "&username=" + Uri.EscapeDataString(data.mta_username_email_merchant) + "&page=" + 0 + "&size=50&storeId=10001";

                myReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + data.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = data.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = data.API_client_password;
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/getBrands?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(data.merchant_code) + "&channelId=MasterOnline&brandName=" + brandName + "&username=" + Uri.EscapeDataString(data.mta_username_email_merchant) + "&page=" + 0 + "&size=50&storeId=10001";

                myReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", data.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            string responseFromServer = "";
            try
            {
                using (System.Net.WebResponse response = myReq.GetResponse())
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

            if (responseFromServer != "")
            {
                //var ret = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(BlibliBrand)) as BlibliBrand;BlibliBrandV2
                try
                {
                    var retData = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(BlibliBrandV2)) as BlibliBrandV2;
                    var list_value = new List<BRAND_BLIBLI>();
                    foreach (var item in retData.content)
                    {
                        if (item.brandApprovalStatus == "APPROVED" && item.brandName == brandName)
                        {
                            ret.status = 1;
                            ret.message = item.code;
                            return ret;
                        }

                    }
                }
                catch (Exception ex)
                {

                }

            }
            return ret;
        }
        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Blibli Gagal.")]
        public async Task<string> ReviseProduct(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, BlibliAPIData iden, BlibliProductData data, PerformContext context)
        {
            var token = SetupContext(iden);
            iden.token = token;

            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.merchant_code).FirstOrDefault();
            var stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == kodeProduk && p.IDMARKET == arf01.RecNum).FirstOrDefault();
            var barangInDb = ErasoftDbContext.STF02.AsNoTracking().SingleOrDefault(b => b.BRG == kodeProduk);
            data = new BlibliControllerJob.BlibliProductData
            {
                kode = barangInDb.BRG,
                nama = barangInDb.NAMA + ' ' + barangInDb.NAMA2 + ' ' + barangInDb.NAMA3,
                berat = (barangInDb.BERAT).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                Keterangan = barangInDb.Deskripsi,
                Qty = "0",
                MinQty = "0",
                PickupPoint = ErasoftDbContext.STF02H.SingleOrDefault(m => m.BRG == barangInDb.BRG && m.IDMARKET == arf01.RecNum).PICKUP_POINT.ToString(),
                IDMarket = arf01.RecNum.ToString(),
                Length = Convert.ToString(barangInDb.PANJANG),
                Width = Convert.ToString(barangInDb.LEBAR),
                Height = Convert.ToString(barangInDb.TINGGI),
                dataBarangInDb = barangInDb
            };
            data.type = barangInDb.TYPE;//add by Tri 27/9/2019
            data.Brand = stf02h.AVALUE_38;
            var dataBrand = await getBrandCode(iden, stf02h.AVALUE_38);
            if(dataBrand.status == 1)
            {
                data.Brand = dataBrand.message;
            }

            data.Price = barangInDb.HJUAL.ToString();
            data.MarketPrice = stf02h.HJUAL.ToString();
            data.CategoryCode = stf02h.CATEGORY_CODE.ToString();

            data.display = stf02h.DISPLAY ? "true" : "false";

            //if merchant code diisi. barulah upload produk
            string ret = "";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            //add by nurul 13/7/2020
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/getProductList?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //end add by nurul 13/7/2020

            //-productType / productHandlingType / productTypeCode
            //It's a product handling type, it's determine on how it will be shipped:

            //REGULAR: regular product, this type is handled by Blibli for specified merchantType. The shipping cost of Regular Product is covered by Blibli.
            //The ID number for REGULAR type = 1.
            //BIG_PRODUCT / HANDLING_BY_MERCHANT : shipped or handling by merchant, Blibli not covered the shipping cost for this product type.Ideally the big product category is like AC, refrigerator and other product that need instalment.Or maybe the electric voucher that sold by merchant by sending email or sms to customer can be included as this type.
            //The ID number for BIG_PRODUCT type = 2.
            //BOPIS : is (Buy Online Pickup In merchant Store).Customer that bought must came to merchant store to pick their product.
            //The ID number for BOPIS type = 3.

            ReviseProductBlibliData newData = new ReviseProductBlibliData()
            {
                //name = data.nama,
                //brand = data.Brand,
                //url = "",
                //categoryCode = data.CategoryCode,
                //productType = 1,
                //pickupPointCode = data.PickupPoint,
                //length = Convert.ToInt32(Convert.ToDouble(data.Length)),
                //width = Convert.ToInt32(Convert.ToDouble(data.Width)),
                //height = Convert.ToInt32(Convert.ToDouble(data.Height)),
                //weight = Convert.ToInt32(Convert.ToDouble(data.berat)),
                //description = Convert.ToBase64String(Encoding.ASCII.GetBytes(data.Keterangan)),
                ////uniqueSellingPoint = Convert.ToBase64String(Encoding.ASCII.GetBytes(data.Keterangan)),
                ////diisi dengan AVALUE_39
                //productStory = Convert.ToBase64String(Encoding.ASCII.GetBytes(data.Keterangan)),
            };
            newData.product = new ReviseProductData
            {
                name = data.nama,
                description = Convert.ToBase64String(Encoding.ASCII.GetBytes(data.Keterangan)),
                productType = 1,                
            };
            newData.dimension = new ReviseDimension
            {
                length = Convert.ToInt32(Convert.ToDouble(data.Length)),
                width = Convert.ToInt32(Convert.ToDouble(data.Width)),
                height = Convert.ToInt32(Convert.ToDouble(data.Height)),
                weight = Convert.ToInt32(Convert.ToDouble(data.berat)),
            };

            //add 6 april 2021, validasi big product
            if (newData.dimension.weight > 50000)// berat lebih dari 50kg -> big product
            {
                newData.product.productType = 2;
            }
            //end add 6 april 2021, validasi big product

            newData.pickupPoint = new RevisepickupPoint
            {
                code = data.PickupPoint
            };
            newData.category = new ReviseCategoryCode
            {
                code = data.CategoryCode
            };
            newData.brand = new ReviseBrand
            {
                code = data.Brand
            };
            //string sSQL = "SELECT * FROM (";
            //for (int i = 1; i <= 30; i++)
            //{
            //    sSQL += "SELECT B.ACODE_" + i.ToString() + " AS CATEGORY_CODE,B.ANAME_" + i.ToString() + " AS CATEGORY_NAME,B.ATYPE_" + i.ToString() + " AS CATEGORY_TYPE,A.AVALUE_" + i.ToString() + " AS VALUE FROM STF02H (NOLOCK) A INNER JOIN MO.DBO.ATTRIBUTE_BLIBLI (NOLOCK) B ON A.CATEGORY_CODE = B.CATEGORY_CODE WHERE A.BRG='" + data.kode + "' AND A.IDMARKET = '" + data.IDMarket + "' " + System.Environment.NewLine;
            //    if (i < 30)
            //    {
            //        sSQL += "UNION ALL " + System.Environment.NewLine;
            //    }
            //}

            //DataSet dsFeature = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE <> 'DEFINING_ATTRIBUTE' ");
            //DataSet dsVariasi = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' AND CATEGORY_TYPE = 'DEFINING_ATTRIBUTE' ");

            var CategoryBlibli = MoDbContext.CategoryBlibli.Where(k => k.CATEGORY_CODE == data.CategoryCode).FirstOrDefault();
            //change by nurul 11/5/2021, limit blibli
            //var listAttributeBlibli = await GetAttributeToList(iden, CategoryBlibli);
            var getAttribute = MoDbContext.AttributeBlibli.Where(a => a.CATEGORY_CODE == data.CategoryCode).ToList();
            var listAttributeOpt = new List<ATTRIBUTE_OPT_BLIBLI>();
            foreach (var attr in getAttribute)
            {
                var getAttributeOpt = MoDbContext.AttributeOptBlibli.Where(a => a.ACODE == attr.ACODE_1 || a.ACODE == attr.ACODE_2 || a.ACODE == attr.ACODE_3 || a.ACODE == attr.ACODE_4 || a.ACODE == attr.ACODE_5 || a.ACODE == attr.ACODE_6 || a.ACODE == attr.ACODE_7 || a.ACODE == attr.ACODE_8 || a.ACODE == attr.ACODE_9 || a.ACODE == attr.ACODE_10 ||
                                                                                a.ACODE == attr.ACODE_11 || a.ACODE == attr.ACODE_12 || a.ACODE == attr.ACODE_13 || a.ACODE == attr.ACODE_14 || a.ACODE == attr.ACODE_15 || a.ACODE == attr.ACODE_16 || a.ACODE == attr.ACODE_17 || a.ACODE == attr.ACODE_18 || a.ACODE == attr.ACODE_19 || a.ACODE == attr.ACODE_20 ||
                                                                                a.ACODE == attr.ACODE_21 || a.ACODE == attr.ACODE_22 || a.ACODE == attr.ACODE_23 || a.ACODE == attr.ACODE_24 || a.ACODE == attr.ACODE_25 || a.ACODE == attr.ACODE_26 || a.ACODE == attr.ACODE_27 || a.ACODE == attr.ACODE_28 || a.ACODE == attr.ACODE_29 || a.ACODE == attr.ACODE_30 ||
                                                                                a.ACODE == attr.ACODE_31 || a.ACODE == attr.ACODE_32 || a.ACODE == attr.ACODE_33 || a.ACODE == attr.ACODE_34 || a.ACODE == attr.ACODE_35).ToList();
                listAttributeOpt.AddRange(getAttributeOpt);
            };
            var listAttributeBlibli = new ATTRIBUTE_BLIBLI_AND_OPT_New()
            {
                attributes = getAttribute,
                attribute_opt = listAttributeOpt,
            };
            //end change by nurul 11/5/2021, limit blibli

            List<string> dsFeature = new List<string>();
            List<string> dsVariasi = new List<string>();
            var attribute = listAttributeBlibli.attributes.FirstOrDefault();
            //for (int i = 1; i <= 30; i++)
            for (int i = 1; i <= 35; i++)
            {
                string attribute_id = Convert.ToString(attribute["ACODE_" + i.ToString()]);
                string attribute_name = Convert.ToString(attribute["ANAME_" + i.ToString()]);
                string attribute_type = Convert.ToString(attribute["ATYPE_" + i.ToString()]);
                string attribute_vc = Convert.ToString(attribute["AVARCREATE_" + i.ToString()]);
                if (!string.IsNullOrWhiteSpace(attribute_id))
                {
                    if (attribute_type == "DEFINING_ATTRIBUTE" || attribute_vc == "1")
                    {
                        dsVariasi.Add(attribute_id);
                    }
                    else
                    {
                        dsFeature.Add(attribute_id);
                    }
                }
            }

            Dictionary<string, string> nonDefiningAttributes = new Dictionary<string, string>();

            //for (int i = 0; i < dsFeature.Tables[0].Rows.Count; i++)
            //{
            //    if (!nonDefiningAttributes.ContainsKey(Convert.ToString(dsFeature.Tables[0].Rows[i]["CATEGORY_CODE"])))
            //    {
            //        nonDefiningAttributes.Add(Convert.ToString(dsFeature.Tables[0].Rows[i]["CATEGORY_CODE"]), Convert.ToString(dsFeature.Tables[0].Rows[i]["VALUE"]).Trim());
            //    }
            //}

            //change 9 juli 2020, ambil 35 attribute
            //for (int i = 1; i <= 30; i++)
            for (int i = 1; i <= 35; i++)
            //end change 9 juli 2020, ambil 35 attribute
            {
                string attribute_id = Convert.ToString(stf02h["ACODE_" + i.ToString()]);
                string value = Convert.ToString(stf02h["AVALUE_" + i.ToString()]);
                if (!string.IsNullOrWhiteSpace(attribute_id) && (value ?? "null") != "null" && !string.IsNullOrEmpty(value))
                {
                    if (dsFeature.Contains(attribute_id))
                    {
                        if (!nonDefiningAttributes.ContainsKey(attribute_id))
                        {
                            nonDefiningAttributes.Add(attribute_id, value.Trim());
                        }
                    }
                }
            }

            newData.nonDefiningAttributes = nonDefiningAttributes;

            //Dictionary<string, string> images = new Dictionary<string, string>();
            var images = new List<ReviseImageMap>();
            var sImages = new ReviseImageMap();
            //List<string> uploadedImageID = new List<string>();
            List<ReviseProductitem> productItems = new List<ReviseProductitem>();
#region bukan barang variasi
            if (data.type == "3")
            {
                List<string> images_pervar = new List<string>();
                string idGambar = "";
                string urlGambar = "";

                idGambar = stf02h.ACODE_50;
                urlGambar = stf02h.AVALUE_50;
                if (string.IsNullOrWhiteSpace(idGambar))
                {
                    idGambar = data.dataBarangInDb.Sort5;
                    urlGambar = data.dataBarangInDb.LINK_GAMBAR_1;
                }
                idGambar = stf02h.BRG + "_1_" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
                //if (!string.IsNullOrWhiteSpace(idGambar))
                if (!string.IsNullOrWhiteSpace(urlGambar))
                {
                    //if (!uploadedImageID.Contains(idGambar))
                    //{
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(urlGambar);

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
                            resizedStream.Position = 0;
                            byte[] resizedByteArr = resizedStream.ToArray();
                            //end change by calvin 1 maret 2019
                            resizedStream.Dispose();

                            if (string.IsNullOrWhiteSpace(idGambar))
                            {
                                idGambar = Convert.ToString(bytes.Length);
                            }
                            //if (!uploadedImageID.Contains(idGambar))
                            //{
                            //    uploadedImageID.Add(idGambar);
                            //images.Add(idGambar, Convert.ToBase64String(resizedByteArr)); // size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                            sImages.data = Convert.ToBase64String(resizedByteArr);
                            sImages.name = idGambar;
                            images.Add(sImages);
                            images_pervar.Add(idGambar);
                            //}
                        }
                    }
                    //}
                }
                idGambar = stf02h.ACODE_49;
                urlGambar = stf02h.AVALUE_49;
                if (string.IsNullOrWhiteSpace(idGambar))
                {
                    idGambar = data.dataBarangInDb.Sort6;
                    urlGambar = data.dataBarangInDb.LINK_GAMBAR_2;
                }
                idGambar = stf02h.BRG + "_2_" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
                //if (!string.IsNullOrWhiteSpace(idGambar))
                if (!string.IsNullOrWhiteSpace(urlGambar))
                {
                    //if (!uploadedImageID.Contains(idGambar))
                    //{
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(urlGambar);

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
                            resizedStream.Position = 0;
                            byte[] resizedByteArr = resizedStream.ToArray();
                            //end change by calvin 1 maret 2019
                            resizedStream.Dispose();

                            if (string.IsNullOrWhiteSpace(idGambar))
                            {
                                idGambar = Convert.ToString(bytes.Length);
                            }

                            //if (!uploadedImageID.Contains(idGambar))
                            //{
                            //    uploadedImageID.Add(idGambar);
                            //images.Add(idGambar, Convert.ToBase64String(resizedByteArr)); // size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                            sImages.data = Convert.ToBase64String(resizedByteArr);
                            sImages.name = idGambar;
                            images.Add(sImages);
                            images_pervar.Add(idGambar);
                            //}
                        }
                    }
                    //}
                }

                idGambar = stf02h.ACODE_48;
                urlGambar = stf02h.AVALUE_48;
                if (string.IsNullOrWhiteSpace(idGambar))
                {
                    idGambar = data.dataBarangInDb.Sort7;
                    urlGambar = data.dataBarangInDb.LINK_GAMBAR_3;
                }
                idGambar = stf02h.BRG + "_3_" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
                if (!string.IsNullOrWhiteSpace(urlGambar))
                //if (!string.IsNullOrWhiteSpace(idGambar))
                {
                    //if (!uploadedImageID.Contains(idGambar))
                    //{
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(urlGambar);

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
                            resizedStream.Position = 0;
                            byte[] resizedByteArr = resizedStream.ToArray();
                            //end change by calvin 1 maret 2019
                            resizedStream.Dispose();

                            if (string.IsNullOrWhiteSpace(idGambar))
                            {
                                idGambar = Convert.ToString(bytes.Length);
                            }

                            //if (!uploadedImageID.Contains(idGambar))
                            //{
                            //    uploadedImageID.Add(idGambar);
                            //images.Add(idGambar, Convert.ToBase64String(resizedByteArr)); // size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                            sImages.data = Convert.ToBase64String(resizedByteArr);
                            sImages.name = idGambar;
                            images.Add(sImages);
                            images_pervar.Add(idGambar);
                            //}
                        }
                    }
                    //}
                }
#region 6/9/2019, 5 gambar

                idGambar = stf02h.SIZE_GAMBAR_4;
                urlGambar = stf02h.LINK_GAMBAR_4;
                if (string.IsNullOrWhiteSpace(idGambar))
                {
                    idGambar = data.dataBarangInDb.SIZE_GAMBAR_4;
                    urlGambar = data.dataBarangInDb.LINK_GAMBAR_4;
                }
                //if (!string.IsNullOrWhiteSpace(idGambar))
                idGambar = stf02h.BRG + "_4_" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
                if (!string.IsNullOrWhiteSpace(urlGambar))
                {
                    //if (!uploadedImageID.Contains(idGambar))
                    //{
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(urlGambar);

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
                            resizedStream.Position = 0;
                            byte[] resizedByteArr = resizedStream.ToArray();
                            //end change by calvin 1 maret 2019
                            resizedStream.Dispose();

                            if (string.IsNullOrWhiteSpace(idGambar))
                            {
                                idGambar = Convert.ToString(bytes.Length);
                            }

                            //if (!uploadedImageID.Contains(idGambar))
                            //{
                            //    uploadedImageID.Add(idGambar);
                            //images.Add(idGambar, Convert.ToBase64String(resizedByteArr)); // size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                            sImages.data = Convert.ToBase64String(resizedByteArr);
                            sImages.name = idGambar;
                            images.Add(sImages);
                            images_pervar.Add(idGambar);
                            //}
                        }
                    }
                    //}
                }

                idGambar = stf02h.SIZE_GAMBAR_5;
                urlGambar = stf02h.LINK_GAMBAR_5;
                if (string.IsNullOrWhiteSpace(idGambar))
                {
                    idGambar = data.dataBarangInDb.SIZE_GAMBAR_5;
                    urlGambar = data.dataBarangInDb.LINK_GAMBAR_5;
                }
                idGambar = stf02h.BRG + "_5_" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
                //if (!string.IsNullOrWhiteSpace(idGambar))
                if (!string.IsNullOrWhiteSpace(urlGambar))
                {
                    //if (!uploadedImageID.Contains(idGambar))
                    //{
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(urlGambar);

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
                            resizedStream.Position = 0;
                            byte[] resizedByteArr = resizedStream.ToArray();
                            //end change by calvin 1 maret 2019
                            resizedStream.Dispose();

                            if (string.IsNullOrWhiteSpace(idGambar))
                            {
                                idGambar = Convert.ToString(bytes.Length);
                            }

                            //if (!uploadedImageID.Contains(idGambar))
                            //{
                            //    uploadedImageID.Add(idGambar);
                            //images.Add(idGambar, Convert.ToBase64String(resizedByteArr)); // size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                            sImages.data = Convert.ToBase64String(resizedByteArr);
                            sImages.name = idGambar;
                            images.Add(sImages);
                            images_pervar.Add(idGambar);
                            //}
                        }
                    }
                    //}
                }
#endregion
                Dictionary<string, string[]> DefiningAttributes = new Dictionary<string, string[]>();
                Dictionary<string, string> attributeMap = new Dictionary<string, string>();
                //for (int a = 0; a < dsVariasi.Tables[0].Rows.Count; a++)
                //{
                //    List<string> dsVariasiValues = new List<string>();
                //    string A_CODE = Convert.ToString(dsVariasi.Tables[0].Rows[a]["CATEGORY_CODE"]);
                //    string A_VALUE = Convert.ToString(dsVariasi.Tables[0].Rows[a]["VALUE"]);
                //    if (!dsVariasiValues.Contains(A_VALUE))
                //    {
                //        dsVariasiValues.Add(A_VALUE);
                //    }

                //    if (!DefiningAttributes.ContainsKey(A_CODE))
                //    {
                //        DefiningAttributes.Add(A_CODE, dsVariasiValues.ToArray());
                //    }

                //    attributeMap.Add(A_CODE, A_VALUE);
                //}
                //change 9 juli 2020, ambil 35 attribute
                //for (int i = 1; i <= 30; i++)
                for (int i = 1; i <= 35; i++)
                //end change 9 juli 2020, ambil 35 attribute
                {
                    string attribute_id = Convert.ToString(stf02h["ACODE_" + i.ToString()]);
                    string value = Convert.ToString(stf02h["AVALUE_" + i.ToString()]);
                    string aname = Convert.ToString(stf02h["ANAME_" + i.ToString()]);//add 30 april 2020, get attr name
                    if (!string.IsNullOrWhiteSpace(attribute_id))
                    {
                        if (dsVariasi.Contains(attribute_id))
                        {
                            List<string> dsVariasiValues = new List<string>();
                            if (!dsVariasiValues.Contains(value))
                            {
                                dsVariasiValues.Add(value);
                            }

                            if (!DefiningAttributes.ContainsKey(attribute_id))
                            {
                                //if(aname != "Family Colour")//filter family color sementara karena validasi baru di blibli
                                DefiningAttributes.Add(attribute_id, dsVariasiValues.ToArray());
                            }
                            if (aname != "Family Colour")//filter family color sementara karena validasi baru di blibli
                                attributeMap.Add(attribute_id, value);
                        }
                    }
                }

                ReviseProductitem newVarItem = new ReviseProductitem()
                {
                    //change sementara, perubahan ketentuan UPC di blibli
                    //upcCode = data.dataBarangInDb.BRG,
                    upcCode = "",
                    //end change sementara, perubahan ketentuan UPC di blibli
                    sellerSku = data.dataBarangInDb.BRG,
                    //price = Convert.ToInt32(Convert.ToDouble(data.Price)),
                    //salePrice = Convert.ToInt32(stf02h.HJUAL),
                    minimumStock = Convert.ToInt32(data.dataBarangInDb.MINI),
                    //stock = Convert.ToInt32(data.dataBarangInDb.MINI),
                    stock = 0,
                    buyable = true,
                    displayable = true,
                    //dangerousGoodsLevel = 0,
                    images = images_pervar.ToArray(),
                    attributesMap = attributeMap
                };
                newVarItem.price = new ReviseProductPrice
                {
                    regular = Convert.ToInt32(Convert.ToDouble(data.Price)),
                    sale = Convert.ToInt32(stf02h.HJUAL),
                };
                //add by calvin 15 agustus 2019

                //change by nurul 19/1/2022
                //var qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(data.dataBarangInDb.BRG, "ALL");
                var multilokasi = ErasoftDbContext.SIFSYS_TAMBAHAN.FirstOrDefault().MULTILOKASI;
                double qty_stock = 1;
                if (multilokasi == "1")
                {
                    qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A_MultiLokasi(data.dataBarangInDb.BRG, "ALL", log_CUST);
                }
                else
                {
                    qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(data.dataBarangInDb.BRG, "ALL");
                }
                //end change by nurul 19/1/2022

                if (qty_stock > 0)
                {
                    newVarItem.stock = Convert.ToInt32(qty_stock);
                }
                //end add by calvin 15 agustus 2019

                productItems.Add(newVarItem);
                newData.definingAttributes = DefiningAttributes;
            }
#endregion
#region Barang Variasi
            else
            {
                Dictionary<string, string> DefiningDariStf02H = new Dictionary<string, string>();

                var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == data.kode).ToList();
                var var_stf02_listbrg = var_stf02.Select(p => p.BRG).ToList();
                var var_stf02h = ErasoftDbContext.STF02H.Where(p => var_stf02_listbrg.Contains(p.BRG) && p.IDMARKET == arf01.RecNum).ToList();
                var var_stf02i = ErasoftDbContext.STF02I.Where(p => p.BRG == data.kode && p.MARKET == "BLIBLI").ToList().OrderBy(p => p.RECNUM);

                Dictionary<string, string[]> DefiningAttributes = new Dictionary<string, string[]>();
                //for (int a = 0; a < dsVariasi.Tables[0].Rows.Count; a++)
                //{
                //    string A_CODE = Convert.ToString(dsVariasi.Tables[0].Rows[a]["CATEGORY_CODE"]);
                //    string A_VALUE = Convert.ToString(dsVariasi.Tables[0].Rows[a]["VALUE"]);

                //    List<string> dsVariasiValues = new List<string>();
                //    var var_stf02i_distinct = var_stf02i.Where(p => p.MP_JUDUL_VAR == A_CODE).ToList().OrderBy(p => p.RECNUM);
                //    foreach (var v in var_stf02i_distinct)
                //    {
                //        if (!dsVariasiValues.Contains(v.MP_VALUE_VAR))
                //        {
                //            dsVariasiValues.Add(v.MP_VALUE_VAR);
                //        }
                //    }

                //    //add by calvin 26 februari, kasus pak rocky, masing" warna 1 sku induk
                //    //maka ambil value untuk attribute tersebut dari stf02h, ( diisi pada bagian detail per marketplace ).
                //    if (var_stf02i_distinct.Count() == 0)
                //    {
                //        if (A_CODE == "WA-0000002") // Warna
                //        {
                //            ValueVariasiWarna = A_VALUE;
                //            dsVariasiValues.Add(A_VALUE);
                //        }
                //    }
                //    //end add by calvin 26 februari, kasus pak rocky, masing" warna 1 sku induk

                //    if (!DefiningAttributes.ContainsKey(A_CODE))
                //    {
                //        DefiningAttributes.Add(A_CODE, dsVariasiValues.ToArray());
                //    }
                //}
                //change 9 juli 2020, ambil 35 attribute
                //for (int i = 1; i <= 30; i++)
                List<string> dsVariasiFCValues = new List<string>();//untuk menampung family color
                List<string> dsVariasiFCValuesInduk = new List<string>();//untuk menampung family color induk
                for (int i = 1; i <= 35; i++)
                //end change 9 juli 2020, ambil 35 attribute
                {
                    string attribute_id = Convert.ToString(stf02h["ACODE_" + i.ToString()]);
                    string value = Convert.ToString(stf02h["AVALUE_" + i.ToString()]);
                    string aname = Convert.ToString(stf02h["ANAME_" + i.ToString()]);//add 30 april 2020, get attr name
                    if (!string.IsNullOrWhiteSpace(attribute_id))
                    {
                        if (dsVariasi.Contains(attribute_id))
                        {
                            List<string> dsVariasiValues = new List<string>();
                            var var_stf02i_distinct = var_stf02i.Where(p => p.MP_JUDUL_VAR == attribute_id).ToList().OrderBy(p => p.RECNUM);
                            foreach (var v in var_stf02i_distinct)
                            {
                                if (!dsVariasiValues.Contains(v.MP_VALUE_VAR))
                                {
                                    dsVariasiValues.Add(v.MP_VALUE_VAR);
                                    //if (v.MP_JUDUL_VAR == "WA-0000002")//add family color jika ada attribute warna
                                    if (aname == "Warna")//add family color jika ada attribute warna
                                    {
                                        dsVariasiFCValues.Add(v.MP_VALUE_FC_VAR);
                                    }
                                }
                            }

                            //add by calvin 26 februari, kasus pak rocky, masing" warna 1 sku induk
                            //maka ambil value untuk attribute tersebut dari stf02h, ( diisi pada bagian detail per marketplace ).
                            if (var_stf02i_distinct.Count() == 0)
                            {
                                if (aname != "Family Colour")//filter family color sementara karena validasi baru di blibli
                                {
                                    DefiningDariStf02H.Add(attribute_id, value);
                                    //if (attribute_id == "WA-0000002") // Warna
                                    //{
                                    //    ValueVariasiWarna = value;
                                    dsVariasiValues.Add(value);
                                    //}
                                }
                            }
                            //end add by calvin 26 februari, kasus pak rocky, masing" warna 1 sku induk

                            if (!DefiningAttributes.ContainsKey(attribute_id))
                            {
                                if (aname != "Family Colour")//filter family color sementara karena validasi baru di blibli
                                    DefiningAttributes.Add(attribute_id, dsVariasiValues.ToArray());
                            }

                            if (attribute_id == "FA-2000060")
                            {
                                dsVariasiFCValuesInduk.Add(value);
                            }
                        }
                    }
                }

                if (dsVariasiFCValues.Count > 0)//masukan attribute family color kalau ada isinya
                {
                    DefiningAttributes.Add("FA-2000060", dsVariasiFCValues.ToArray());
                }

                if (!DefiningAttributes.ContainsKey("FA-2000060") && dsVariasiFCValuesInduk.Count > 0)//belum ada familu color dan attr ada di brg induk
                {
                    DefiningAttributes.Add("FA-2000060", dsVariasiFCValuesInduk.ToArray());

                }

                newData.definingAttributes = DefiningAttributes;

                foreach (var var_item in var_stf02)
                {
                    var var_stf02h_item = var_stf02h.Where(p => p.BRG == var_item.BRG).FirstOrDefault();

                    List<string> images_pervar = new List<string>();
                    string image_id = var_stf02h_item.ACODE_50;
                    if (string.IsNullOrWhiteSpace(image_id))
                    {
                        image_id = var_item.Sort5;
                    }
                    string url = var_stf02h_item.AVALUE_50;
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        url = var_item.LINK_GAMBAR_1;
                    }
                    image_id = "_1_" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
                    //if (!string.IsNullOrWhiteSpace(image_id))
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        //if (!uploadedImageID.Contains(image_id))
                        //{
                        using (var client = new HttpClient())
                        {
                            //string url = var_stf02h_item.AVALUE_50;
                            //if (string.IsNullOrWhiteSpace(url))
                            //{
                            //    url = var_item.LINK_GAMBAR_1;
                            //}
                            //var bytes = await client.GetByteArrayAsync(var_item.LINK_GAMBAR_1);
                            var bytes = await client.GetByteArrayAsync(url);

                            //images.Add(var_item.Sort5, Convert.ToBase64String(bytes));// size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
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
                                resizedStream.Position = 0;
                                byte[] resizedByteArr = resizedStream.ToArray();
                                //end change by calvin 1 maret 2019
                                resizedStream.Dispose();

                                //images.Add(var_item.Sort5, Convert.ToBase64String(resizedByteArr));// size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                                if (string.IsNullOrWhiteSpace(image_id))
                                {
                                    image_id = Convert.ToString(bytes.Length);
                                }
                                //if (!uploadedImageID.Contains(image_id))
                                //{
                                //    uploadedImageID.Add(image_id);
                                //images.Add(var_item.BRG + image_id, Convert.ToBase64String(resizedByteArr));// size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                                sImages.data = Convert.ToBase64String(resizedByteArr);
                                sImages.name = var_item.BRG + image_id;
                                images.Add(sImages);
                                images_pervar.Add(var_item.BRG + image_id); // size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                                                                            //}
                            }
                        }
                        //}
                    }
#region 6/9/2019, barang varian 2 gambar
                    //image_id = var_stf02h_item.ACODE_49;
                    //if (string.IsNullOrWhiteSpace(image_id))
                    //{
                    //    image_id = var_item.Sort6;
                    //}
                    //if (!string.IsNullOrWhiteSpace(image_id))
                    //{
                    //    if (!uploadedImageID.Contains(image_id))
                    //    {
                    //        using (var client = new HttpClient())
                    //        {
                    //            string url = var_stf02h_item.AVALUE_49;
                    //            if (string.IsNullOrWhiteSpace(url))
                    //            {
                    //                url = var_item.LINK_GAMBAR_2;
                    //            }
                    //            //var bytes = await client.GetByteArrayAsync(var_item.LINK_GAMBAR_1);
                    //            var bytes = await client.GetByteArrayAsync(url);

                    //            //images.Add(var_item.Sort5, Convert.ToBase64String(bytes));// size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                    //            using (var stream = new MemoryStream(bytes, true))
                    //            {
                    //                var img = Image.FromStream(stream);
                    //                float newResolution = img.Height;
                    //                if (img.Width < newResolution)
                    //                {
                    //                    newResolution = img.Width;
                    //                }
                    //                var resizedImage = (Image)BlibliResizeImage(img, Convert.ToInt32(newResolution), Convert.ToInt32(newResolution));
                    //                //var resizedImage = (Image)BlibliResizeImageFromStream(stream);

                    //                //change by calvin 1 maret 2019
                    //                //ImageConverter _imageConverter = new ImageConverter();
                    //                //byte[] resizedByteArr = (byte[])_imageConverter.ConvertTo(resizedImage, typeof(byte[]));
                    //                System.Drawing.Imaging.ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);

                    //                System.Drawing.Imaging.Encoder myEncoder =
                    //                    System.Drawing.Imaging.Encoder.Quality;
                    //                System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);

                    //                System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, 90L);
                    //                myEncoderParameters.Param[0] = myEncoderParameter;

                    //                var resizedStream = new System.IO.MemoryStream();
                    //                resizedImage.Save(resizedStream, jpgEncoder, myEncoderParameters);
                    //                resizedStream.Position = 0;
                    //                byte[] resizedByteArr = resizedStream.ToArray();
                    //                //end change by calvin 1 maret 2019
                    //                resizedStream.Dispose();

                    //                //images.Add(var_item.Sort5, Convert.ToBase64String(resizedByteArr));// size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                    //                if (string.IsNullOrWhiteSpace(image_id))
                    //                {
                    //                    image_id = Convert.ToString(bytes.Length);
                    //                }
                    //                if (!uploadedImageID.Contains(image_id))
                    //                {
                    //                    uploadedImageID.Add(image_id);
                    //                    images.Add(image_id, Convert.ToBase64String(resizedByteArr));// size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                    //                    images_pervar.Add(image_id); // size kb nya, sebagai id, agar tidak ada gambar duplikat terupload
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
#endregion
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
                        //add by calvin 26 februari, kasus pak rocky, masing" warna 1 sku induk
                        //if (!attributeMap.ContainsKey("WA-0000002"))
                        //{
                        //    attributeMap.Add("WA-0000002", ValueVariasiWarna);
                        //}
                        if (DefiningDariStf02H.Count() > 0)
                        {
                            foreach (var item in DefiningDariStf02H)
                            {
                                if (!attributeMap.ContainsKey(item.Key))
                                {
                                    attributeMap.Add(item.Key, item.Value);
                                }
                            }
                        }
                        //end add by calvin 26 februari, kasus pak rocky, masing" warna 1 sku induk
                    }
                    else if (!string.IsNullOrWhiteSpace(var_item.Sort9))
                    {
                        var var_stf02i_judul_mp_var_2 = var_stf02i.Where(p => p.KODE_VAR == var_item.Sort9 && p.LEVEL_VAR == 2).FirstOrDefault();
                        if (var_stf02i_judul_mp_var_2 != null)
                        {
                            attributeMap.Add(var_stf02i_judul_mp_var_2.MP_JUDUL_VAR, var_stf02i_judul_mp_var_2.MP_VALUE_VAR);
                        }
                        if (DefiningDariStf02H.Count() > 0)
                        {
                            foreach (var item in DefiningDariStf02H)
                            {
                                if (!attributeMap.ContainsKey(item.Key))
                                {
                                    attributeMap.Add(item.Key, item.Value);
                                }
                            }
                        }
                    }

                    ReviseProductitem newVarItem = new ReviseProductitem()
                    {
                        //change sementara, perubahan ketentuan UPC di blibli
                        //upcCode = var_item.BRG,
                        upcCode = "",
                        //change sementara, perubahan ketentuan UPC di blibli
                        sellerSku = var_item.BRG,
                        //price = Convert.ToInt32(var_item.HJUAL),
                        //salePrice = Convert.ToInt32(var_stf02h_item.HJUAL),
                        minimumStock = Convert.ToInt32(var_item.MINI),
                        //stock = Convert.ToInt32(var_item.MINI),
                        stock = 0,
                        buyable = true,
                        displayable = true,
                        //dangerousGoodsLevel = 0,
                        images = images_pervar.ToArray(),
                        attributesMap = attributeMap
                    };

                    newVarItem.price = new ReviseProductPrice
                    {
                        regular = Convert.ToInt32(Convert.ToDouble(var_item.HJUAL)),
                        sale = Convert.ToInt32(var_stf02h_item.HJUAL),
                    };
                    //add by calvin 15 agustus 2019

                    //change by nurul 19/1/2022
                    //var qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(var_item.BRG, "ALL");
                    var multilokasi = ErasoftDbContext.SIFSYS_TAMBAHAN.FirstOrDefault().MULTILOKASI;
                    double qty_stock = 1;
                    if (multilokasi == "1")
                    {
                        qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A_MultiLokasi(var_item.BRG, "ALL", log_CUST);
                    }
                    else
                    {
                        qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(var_item.BRG, "ALL");
                    }
                    //end change by nurul 19/1/2022

                    if (qty_stock > 0)
                    {
                        newVarItem.stock = Convert.ToInt32(qty_stock);
                    }
                    //end add by calvin 15 agustus 2019
                    productItems.Add(newVarItem);
                }
            }
#endregion

            newData.productItems = (productItems);
            newData.imageMap = images.ToArray();
            newData.product.uniqueSellingPoint = Convert.ToBase64String(Encoding.ASCII.GetBytes(Convert.ToString(stf02h["AVALUE_39"])));

            string myData = JsonConvert.SerializeObject(newData);

            //myData = myData.Replace("\\r\\n", "\\n").Replace("–", "-").Replace("\\\"\\\"", "").Replace("×", "x");
            var prdCode = stf02h.BRG_MP.Split(';');
            string product_code = "";
            if (prdCode.Length == 2)
            {
                product_code = prdCode[1];
            }
            urll = "https://api.blibli.com/v2/proxy/seller/v1/product-submissions/" + product_code + "?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&username=" + Uri.EscapeDataString(userMTA) + "&storeCode=" + Uri.EscapeDataString(iden.merchant_code) + "&storeId=10001&channelId=MasterOnline";

#if (DEBUG || Debug_AWS)
            string jobId = "";
#else
            string jobId = context.BackgroundJob.Id;
#endif
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Create Product",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = data.kode,
                REQUEST_ATTRIBUTE_2 = data.nama,
                REQUEST_ATTRIBUTE_3 = jobId, //hangfire job id ( create product )
                //REQUEST_ATTRIBUTE_5 = "BLIBLI_CPRODUCT",//add by Tri 19 Des 2019, agar log create brg blibli tidak terhapus
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            myReq = (HttpWebRequest)WebRequest.Create(urll);
            string usernameMO = iden.API_client_username;
            string passMO = iden.API_client_password;
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
            myReq.Headers.Add("Signature-Time", milis.ToString());

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
            catch (WebException e)
            {
                string err = "";
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                }
            }

            if (responseFromServer != "")
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ReviseProductResult)) as ReviseProductResult;
                if (string.IsNullOrEmpty(Convert.ToString(result.errorCode)))
                {
                    string EDBConnID = EDB.GetConnectionString("ConnId");
                    var sqlStorage = new SqlServerStorage(EDBConnID);

                    var client = new BackgroundJobClient(sqlStorage);
                    //INSERT QUEUE FEED
#if (DEBUG || Debug_AWS)
                    await CreateProductSuccess_1(dbPathEra, kodeProduk, log_CUST, "Barang", "Buat Produk (Tahap 2 / 3)", iden, Convert.ToString(data.kode), Convert.ToString(result.content.queueId), Convert.ToString(milis));
#else
                    client.Enqueue<BlibliControllerJob>(x => x.CreateProductSuccess_1(dbPathEra, kodeProduk, log_CUST, "Barang", "Buat Produk (Tahap 2 / 3)", iden, Convert.ToString(data.kode), Convert.ToString(result.content.queueId), Convert.ToString(milis)));
#endif
                    //client.Enqueue<BlibliControllerJob>(x => x.CreateProductSuccess_2(dbPathEra, kodeProduk, log_CUST, "Barang", "Buat Produk (Tahap 2 / 3)", iden, Convert.ToString(data.kode), Convert.ToString(result.value.queueFeedId), Convert.ToString(milis)));
                }
                else
                {
                    throw new Exception(Convert.ToString(result.errorCode));

                    //currentLog.REQUEST_EXCEPTION = Convert.ToString(result.errorCode);
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                }
            }

            return ret;
        }


        public class BlibliCekProductActive
        {
            public string[] merchantSkus { get; set; }
            public bool isArchive { get; set; }
            public int size { get; set; }
            public int page { get; set; }
        }

        public class BlibliGetQueueProductCode
        {
            public string productCode { get; set; }
            public string productName { get; set; }
            public string[] merchantSkuList { get; set; }
            //need_correction
            public string oldProductCode { get; set; }
            public string newProductCode { get; set; }

        }

        public class Blibli_ProductSubmissionList_Response
        {
            public string requestId { get; set; }
            public ProductSubmissionList_Content[] content { get; set; }
            public Paging paging { get; set; }
            public string errorMessage { get; set; }
            public string errorCode { get; set; }
        }

        public class Paging
        {
            public int pageSize { get; set; }
            public int pageNumber { get; set; }
            public int totalPage { get; set; }
            public int totalRecord { get; set; }
        }

        public class ProductSubmissionList_Content
        {
            public Product product { get; set; }
            //public Brand brand { get; set; }
            //public Dimension dimension { get; set; }
            //public Category category { get; set; }
            //public ProductSubmissionList_Image[] images { get; set; }
            public ProductSubmissionList_Productitem[] productItems { get; set; }
        }

        public class Product
        {
            public string code { get; set; }
            public string sku { get; set; }
            public string name { get; set; }
            public string videoUrl { get; set; }
            public string uniqueSellingPoint { get; set; }
            public string description { get; set; }
            public string revisionNotes { get; set; }
            public string state { get; set; }
        }

        public class Brand
        {
            public string name { get; set; }
            public string state { get; set; }
        }

        public class Dimension
        {
            public int length { get; set; }
            public int width { get; set; }
            public float weight { get; set; }
            public int height { get; set; }
            public int shippingWeight { get; set; }
        }

        public class Category
        {
            public string code { get; set; }
            public string name { get; set; }
        }

        public class ProductSubmissionList_Image
        {
            public string path { get; set; }
            public int sequence { get; set; }
            public bool main { get; set; }
        }

        public class ProductSubmissionList_Productitem
        {
            public string name { get; set; }
            public string code { get; set; }
            public string upcCode { get; set; }
        }

        public class BlibliCekProductActiveResult
        {
            public string requestId { get; set; }
            public object headers { get; set; }
            public object errorMessage { get; set; }
            public object errorCode { get; set; }
            public bool success { get; set; }
            public BlibliCekProductActiveResultContent[] content { get; set; }
            public BlibliCekProductActiveResultPagemetadata pageMetaData { get; set; }
        }

        public class BlibliCekProductActiveResultPagemetadata
        {
            public int pageSize { get; set; }
            public int pageNumber { get; set; }
            public int totalRecords { get; set; }
        }

        public class BlibliCekProductActiveResultContent
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
            public bool isArchived { get; set; }
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Blibli Berhasil. Cek Active Gagal.")]
        public async Task<string> CekProductActive(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, BlibliAPIData iden, string kodeInduk, List<string> merchantskus, string cust, string queue_feed_requestid, string api_log_requestId)
        {
            await CekProductActiveWithPage(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, kodeInduk, merchantskus, cust, queue_feed_requestid, api_log_requestId, 0);
            return "";
        }
        public async Task<string> CekProductActiveWithPage(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, BlibliAPIData iden, string kodeInduk, List<string> merchantskus, string cust, string queue_feed_requestid, string api_log_requestId, int page)
        {

            var token = SetupContext(iden);
            iden.token = token;

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string myData = JsonConvert.SerializeObject(new BlibliCekProductActive() { merchantSkus = merchantskus.ToArray(), isArchive = false, size = 100, page = page });

            //change by nurul 13/7/2020
            //    string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //    string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            //    string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
            //    string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/getProductList", iden.API_secret_key);
            //    string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/getProductList?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&username=" + Uri.EscapeDataString(userMTA) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code);
            //    urll += "&channelId=MasterOnline";
            //    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //    myReq.Method = "POST";
            //    myReq.Headers.Add("Authorization", ("bearer " + iden.token));
            //    myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            //    myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            //    myReq.Accept = "application/json";
            //    myReq.ContentType = "application/json";
            //    myReq.Headers.Add("requestId", milis.ToString());
            //    myReq.Headers.Add("sessionId", milis.ToString());
            //    myReq.Headers.Add("username", userMTA);

            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/getProductList?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            if (iden.versiToken != "2")
            {
                string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
                string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/getProductList", iden.API_secret_key);
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/getProductList?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&username=" + Uri.EscapeDataString(userMTA) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code);
                urll += "&channelId=MasterOnline";
                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "POST";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                //add by nurul 10/7/2020
                string usernameMO = iden.API_client_username;
                string passMO = iden.API_client_password;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                //end add by nurul 10/7/2020
                string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
                //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/getProductList", iden.API_secret_key);
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/getProductList?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&username=" + Uri.EscapeDataString(userMTA);
                urll += "&channelId=MasterOnline";
                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "POST";
                //myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                //myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                //myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":"));
                //myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                //myReq.Headers.Add("Accept", "application/json");
                //myReq.Headers.Add("Content-Type", "application/json");
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());

                //myReq.Headers.Add("requestId", milis.ToString());
                //myReq.Headers.Add("sessionId", milis.ToString());
                //myReq.Headers.Add("username", userMTA);
            }
            //end change by nurul 13/7/2020

            string responseFromServer = "";
            myReq.ContentLength = myData.Length;
            using (var dataStream = myReq.GetRequestStream())
            {
                dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
            }
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
                    throw new Exception(err);
                }
                else
                {
                    throw e;
                }
            }

            if (responseFromServer != null)
            {
                var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(BlibliCekProductActiveResult)) as BlibliCekProductActiveResult;
                if (listBrg != null)
                {
                    if (listBrg.success)
                    {
                        if (listBrg.content.Count() > 0)
                        {
                            List<string> itemUpdateStok = new List<string>();

                            string urlbrg = "";
                            //product lolos qc dengan status active
                            foreach (var item in listBrg.content)
                            {
                                if (listBrg.content.Count() > 1)
                                {
                                    urlbrg = "is--" + item.gdnSku;
                                }
                                else
                                {
                                    var splitCode = item.gdnSku.Split('-');
                                    urlbrg = "ps--" + splitCode[0] + "-" + splitCode[1] + "-" + splitCode[2];
                                }
                                using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                                {
                                    oConnection.Open();
                                    using (SqlCommand oCommand = oConnection.CreateCommand())
                                    {
                                        oCommand.CommandType = CommandType.Text;
                                        oCommand.CommandText = "UPDATE STF02H SET BRG_MP = @BRG_MP,LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" 
                                            + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") 
                                            + "',AVALUE_34 = 'https://www.blibli.com/p/-/" + urlbrg
                                            + "',LINK_ERROR = '0;Buat Produk;;' WHERE BRG = @BRG AND IDMARKET = @IDMARKET ";
                                        //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@BRG", SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@IDMARKET", SqlDbType.Int));
                                        oCommand.Parameters.Add(new SqlParameter("@BRG_MP", SqlDbType.NVarChar, 50));

                                        oCommand.Parameters[0].Value = item.merchantSku;
                                        oCommand.Parameters[1].Value = iden.idmarket;
                                        //oCommand.Parameters[2].Value = item.gdnSku + ";" + item.productItemCode; // seharusnya gdnSku + item_var.productItemCode, tidak ketemu darimana gdnSku nya
                                        oCommand.Parameters[2].Value = item.gdnSku; 

                                        oCommand.ExecuteNonQuery();
                                    }
                                }
                                itemUpdateStok.Add(item.merchantSku);
                            }

                            string STF02_BRG = kodeInduk;

                            var apiLogInDb = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == api_log_requestId).SingleOrDefault();
                            if (apiLogInDb != null)
                            {
                                apiLogInDb.REQUEST_STATUS = "Success";
                                apiLogInDb.REQUEST_RESULT = "";
                                apiLogInDb.REQUEST_EXCEPTION = "";
                                ErasoftDbContext.SaveChanges();
                            }

                            using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                            {
                                oConnection.Open();
                                using (SqlCommand oCommand = oConnection.CreateCommand())
                                {
                                    oCommand.CommandType = CommandType.Text;
                                    oCommand.CommandText = "UPDATE [QUEUE_FEED_BLIBLI] SET [STATUS] = 0 WHERE REQUESTID = @REQUESTID ";
                                    oCommand.Parameters.Add(new SqlParameter("@REQUESTID", SqlDbType.NVarChar, 50));
                                    oCommand.Parameters[0].Value = queue_feed_requestid;
                                    oCommand.ExecuteNonQuery();
                                }
                            }
                            var splitCodeInduk = listBrg.content[0].gdnSku.Split('-');
                            urlbrg = "ps--" + splitCodeInduk[0] + "-" + splitCodeInduk[1] + "-" + splitCodeInduk[2];

                            using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                            {
                                oConnection.Open();
                                using (SqlCommand oCommand = oConnection.CreateCommand())
                                {
                                    oCommand.CommandType = CommandType.Text;
                                    oCommand.CommandText = "UPDATE STF02H SET BRG_MP = @BRG_MP,LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" 
                                        + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") 
                                        + "',AVALUE_34 = 'https://www.blibli.com/p/-/" + urlbrg
                                        + "',LINK_ERROR = '0;Buat Produk;;' WHERE BRG = @BRG AND IDMARKET = @IDMARKET AND BRG_MP = 'PENDING' ";
                                    oCommand.Parameters.Add(new SqlParameter("@BRG", SqlDbType.NVarChar, 50));
                                    oCommand.Parameters.Add(new SqlParameter("@IDMARKET", SqlDbType.Int));
                                    oCommand.Parameters.Add(new SqlParameter("@BRG_MP", SqlDbType.NVarChar, 50));

                                    oCommand.Parameters[0].Value = STF02_BRG;
                                    oCommand.Parameters[1].Value = iden.idmarket;
                                    oCommand.Parameters[2].Value = kodeInduk;
                                    oCommand.ExecuteNonQuery();
                                }
                            }

#region update stok
                            string ConnId = "[BLI_QC][" + DateTime.Now.ToString("yyyyMMddhhmmss") + "]";
                            string sSQLValues = "";

                            foreach (var item in itemUpdateStok)
                            {
                                sSQLValues = sSQLValues + "('" + item + "', '" + ConnId + "'),";
                            }
                            if (sSQLValues != "")
                            {
                                sSQLValues = sSQLValues.Substring(0, sSQLValues.Length - 1);
                                using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                                {
                                    oConnection.Open();
                                    using (SqlCommand oCommand = oConnection.CreateCommand())
                                    {
                                        oCommand.CommandType = CommandType.Text;
                                        oCommand.CommandText = "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES " + sSQLValues;
                                        oCommand.ExecuteNonQuery();
                                    }
                                }

                                new StokControllerJob().updateStockMarketPlace(ConnId, dbPathEra, "BlibliActive");
                            }
#endregion
                            if (listBrg.content.Count() == 100)
                            {
                                await CekProductActiveWithPage(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, kodeInduk, merchantskus, cust, queue_feed_requestid, api_log_requestId, page + 1);
                            }
                        }
                        else
                        {
                            //cek ke api product reject
                            await CekProductReject(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, kodeInduk, merchantskus, cust, queue_feed_requestid, api_log_requestId);
                        }
                    }
                    else
                    {
                        throw new Exception(Convert.ToString(listBrg.errorMessage));
                    }
                }
            }
            return "";
        }
        //end change by nurul 13/7/2020

        public async Task<string> createPackage(string dbPathEra, BlibliAPIData iden, List<string> orderItemIDs)
        {
            string ret = "";
            long milis = CurrentTimeMillis();
            var token = SetupContext(iden);
            iden.token = token;
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            //change by nurul 13/7/2020
            //string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            ////string apiId = "mta-api-sandbox:sandbox-secret-key";//<-- diambil dari profil API
            //string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            //string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

            //createPackageData thisData = new createPackageData();

            //thisData.orderItemIds = orderItemIDs;

            //string myData = JsonConvert.SerializeObject(thisData);

            //string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/order/createPackage", iden.API_secret_key);
            //string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/createPackage?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&storeId=10001" + "&channelId=MasterOnline&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code);

            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //myReq.Method = "POST";
            //myReq.Headers.Add("Authorization", ("bearer " + iden.token));
            //myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            //myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            //myReq.Accept = "application/json";
            //myReq.ContentType = "application/json";
            //myReq.Headers.Add("requestId", milis.ToString());
            //myReq.Headers.Add("sessionId", milis.ToString());
            //myReq.Headers.Add("username", userMTA);

            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/createPackage?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            createPackageData thisData = new createPackageData();

            thisData.orderItemIds = orderItemIDs;

            string myData = JsonConvert.SerializeObject(thisData);

            if (iden.versiToken != "2")
            {
                string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

                string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/order/createPackage", iden.API_secret_key);
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/createPackage?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&storeId=10001" + "&channelId=MasterOnline&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code);

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "POST";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/createPackage?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&storeId=10001" + "&channelId=MasterOnline&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code);

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "POST";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }

            string responseFromServer = "";

            //try
            //{
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
            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}
            if (responseFromServer != null)
            {
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {
                    ret = result.value.packageId;
                }
            }
            return ret;
        }

        public class BlibliShippingLabelRet
        {
            public bool success { get; set; }
            public string errorMessage { get; set; }
            public BlibliShippingLabelRetValue value { get; set; }
        }
        public class BlibliShippingLabelRetValue
        {
            public string document { get; set; }
        }
        public async Task<BlibliShippingLabelRet> GetShippingLabel(string dbPathEra, BlibliAPIData iden, string orderItemId)
        {
            var result = new BlibliShippingLabelRet();
            var token = SetupContext(iden);
            iden.token = token;

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            //change by nurul 13/7/2020
            //string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
            //string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
            //string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
            //string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/order/downloadShippingLabel", iden.API_secret_key);

            //string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/downloadShippingLabel?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString());
            //urll += "&channelId=MasterOnline";
            //urll += "&storeId=10001";
            //urll += "&orderItemId=" + Uri.EscapeDataString(orderItemId);
            //urll += "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code);

            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //myReq.Method = "GET";
            //myReq.Headers.Add("Authorization", ("bearer " + iden.token));
            //myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            //myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            //myReq.Accept = "application/json";
            //myReq.ContentType = "application/json";
            //myReq.Headers.Add("requestId", milis.ToString());
            //myReq.Headers.Add("sessionId", milis.ToString());
            //myReq.Headers.Add("username", userMTA);

            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/downloadShippingLabel?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            if (iden.versiToken != "2")
            {
                string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v1/order/downloadShippingLabel", iden.API_secret_key);

                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/downloadShippingLabel?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString());
                urll += "&channelId=MasterOnline";
                urll += "&storeId=10001";
                urll += "&orderItemId=" + Uri.EscapeDataString(orderItemId);
                urll += "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code);

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/downloadShippingLabel?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString());
                urll += "&channelId=MasterOnline";
                urll += "&storeId=10001";
                urll += "&orderItemId=" + Uri.EscapeDataString(orderItemId);
                urll += "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code);

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }
            //end change by nurul 13/7/2020

            string responseFromServer = "";
            using (WebResponse response = await myReq.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseFromServer = reader.ReadToEnd();
                }
            }
            if (responseFromServer != "")
            {
                result = JsonConvert.DeserializeObject(responseFromServer, typeof(BlibliShippingLabelRet)) as BlibliShippingLabelRet;
            }
            return result;
        }

        //add by nurul 3/2/2021, new shipping label blibli
        public async Task<bool> CombineShippingList(string dbPathEra, BlibliAPIData iden, List<string> orderItemIDs)
        {
            //string ret = "";
            var ret = false;
            long milis = CurrentTimeMillis();
            var token = SetupContext(iden);
            iden.token = token;
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            
            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/getCombineShipping?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            createPackageData thisData = new createPackageData();

            thisData.orderItemIds = orderItemIDs;

            string myData = JsonConvert.SerializeObject(thisData);

            if (iden.versiToken != "2")
            {
                string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

                string signature = CreateToken("POST\n" + CalculateMD5Hash(myData) + "\napplication/json\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mta/api/businesspartner/v1/order/getCombineShipping", iden.API_secret_key);
                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/getCombineShipping?businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&requestId =" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&storeId=10001" + "&orderItemNo=" + Uri.EscapeDataString(orderItemIDs.FirstOrDefault()) + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "POST";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/getCombineShipping?businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code) + "&requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&storeId=10001" + "&orderItemNo=" + Uri.EscapeDataString(orderItemIDs.FirstOrDefault()) + "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }

            string responseFromServer = "";

            //try
            //{
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
            catch (WebException ex)
            {
                string err1 = "";
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp1 = ex.Response;
                    using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                    {
                        err1 = sr1.ReadToEnd();
                        responseFromServer = err1;
                    }
                }
            }
            if (responseFromServer != null)
            {
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                if (string.IsNullOrEmpty(result.errorCode.Value))
                {
                    //ret = result.success;
                    ret = true;
                }
            }
            return ret;
        }
        public async Task<BlibliShippingLabelRet> GetShippingLabelV2(string dbPathEra, BlibliAPIData iden, string packageId, string nobuk)
        {
            var result = new BlibliShippingLabelRet();
            var token = SetupContext(iden);
            iden.token = token;

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = Uri.EscapeDataString("MasterOnline-" + milis.ToString()),
                REQUEST_ACTION = "Print Label BLibli",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
                REQUEST_ATTRIBUTE_2 = nobuk,
                REQUEST_ATTRIBUTE_3 = packageId,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://api.blibli.com/v2/proxy/seller/v1/orders/{package-id}/shippingLabel?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            if (iden.versiToken != "2")
            {
                string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/seller/v1/orders/" + Uri.EscapeDataString(packageId) + "/shippingLabel", iden.API_secret_key);

                urll = "https://api.blibli.com/v2/proxy/seller/v1/orders/" + Uri.EscapeDataString(packageId) + "/shippingLabel?package-id=" + Uri.EscapeDataString(packageId) + "&requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString());
                urll += "&username=" + Uri.EscapeDataString(iden.mta_username_email_merchant);
                urll += "&storeId=10001";
                urll += "&storeCode=" + Uri.EscapeDataString(iden.merchant_code);
                urll += "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

                urll = "https://api.blibli.com/v2/proxy/seller/v1/orders/" + Uri.EscapeDataString(packageId) + "/shippingLabel?package-id=" + Uri.EscapeDataString(packageId) + "&requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString());
                urll += "&username=" + Uri.EscapeDataString(iden.mta_username_email_merchant);
                urll += "&storeId=10001";
                urll += "&storeCode=" + Uri.EscapeDataString(iden.merchant_code);
                urll += "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }

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
            }catch(WebException ex)
            {
                string err1 = "";
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp1 = ex.Response;
                    using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
                    {
                        err1 = sr1.ReadToEnd();
                        responseFromServer = err1;
                    }
                }
            }
            if (responseFromServer != "")
            {
                dynamic resultRespons = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                try
                {
                    var cekDoc = resultRespons.content.shippingLabel.Value;
                    if (!string.IsNullOrEmpty(cekDoc))
                    {
                        result.success = true;
                        var pdf = new BlibliShippingLabelRetValue();
                        pdf.document = cekDoc;
                        result.value = pdf;
                        try
                        {
                            var dsSOT01B = EDB.GetDataSet("SConn", "SO", "SELECT TOP 1 ISNULL(TRACKING_SHIPMENT,'') AS TRACKING_SHIPMENT, NO_REFERENSI, ORDER_ITEM_ID FROM SOT01A A (NOLOCK) INNER JOIN SOT01B B (NOLOCK) ON A.NO_BUKTI=B.NO_BUKTI WHERE NO_PO_CUST = '" + packageId + "' AND A.NO_BUKTI='" + nobuk + "'");
                            if (dsSOT01B.Tables[0].Rows.Count > 0)
                            {
                                if (string.IsNullOrEmpty(Convert.ToString(dsSOT01B.Tables[0].Rows[0]["TRACKING_SHIPMENT"])))
                                {
                                    await UpdateNoResi(dbPathEra, iden, Convert.ToString(dsSOT01B.Tables[0].Rows[0]["NO_REFERENSI"]), Convert.ToString(dsSOT01B.Tables[0].Rows[0]["ORDER_ITEM_ID"]), 1);
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        result.success = false;
                        result.errorMessage = resultRespons.errorMessage.Value;

                        //add by nurul 11/2/2021, save error print label 
                        manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
                        currentLog.REQUEST_EXCEPTION = resultRespons.errorMessage.Value;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        //end add by nurul 11/2/2021, save error print label 
                    }
                }
                catch (Exception ex)
                {
                    result.success = false;
                    result.errorMessage = "";
                    if (!string.IsNullOrEmpty(resultRespons.errorMessage.Value))
                    {
                        result.errorMessage = resultRespons.errorMessage.Value;

                        //add by nurul 11/2/2021, save error print label 
                        manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
                        currentLog.REQUEST_EXCEPTION = resultRespons.errorMessage.Value;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        //end add by nurul 11/2/2021, save error print label 
                    }
                }
            }
            return result;
        }
        public async Task<string> UpdateNoResi(string dbPathEra, BlibliAPIData iden, string orderNo, string orderItemNo, int resi)
        {
            var result = "";
            var token = SetupContext(iden);
            iden.token = token;

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderDetail?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            if (iden.versiToken != "2")
            {
                string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mta/api/businesspartner/v1/order/orderDetail", iden.API_secret_key);

                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderDetail?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString());
                urll += "&storeId=10001";
                urll += "&orderNo=" + Uri.EscapeDataString(orderNo);
                urll += "&orderItemNo=" + Uri.EscapeDataString(orderItemNo);
                urll += "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderDetail?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString());
                urll += "&storeId=10001";
                urll += "&orderNo=" + Uri.EscapeDataString(orderNo);
                urll += "&orderItemNo=" + Uri.EscapeDataString(orderItemNo);
                urll += "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }

            string responseFromServer = "";
            using (WebResponse response = await myReq.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseFromServer = reader.ReadToEnd();
                }
            }
            if (responseFromServer != "")
            {
                dynamic resultRespons = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                try
                {
                    if (resi == 0)
                    {
                        var PackageId = resultRespons.value.packageId.Value;
                        if (!string.IsNullOrEmpty(PackageId))
                        {
                            result = PackageId;
                            try
                            {
                                var updated = EDB.ExecuteSQL("SConn", CommandType.Text, "UPDATE SOT01A SET NO_PO_CUST = '" + PackageId + "' WHERE NO_REFERENSI='" + orderNo + "'");
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                    else 
                    {
                        var noResi = resultRespons.value.awbNumber.Value;
                        if (!string.IsNullOrEmpty(noResi))
                        {
                            result = noResi;
                            try
                            {
                                var updated = EDB.ExecuteSQL("SConn", CommandType.Text, "UPDATE SOT01A SET TRACKING_SHIPMENT = '" + noResi + "' WHERE NO_REFERENSI='" + orderNo + "'");
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }catch(Exception ex)
                {

                }
            }
            return result;
        }
        //end add by nurul 3/2/2021, new shipping label blibli

        //add by nurul 5/4/2022
        public class tempPackageId_Status
        {
            public string packageid { get; set; }
            public string status { get; set; }
        }
        public async Task<tempPackageId_Status> GetPackageId(string dbPathEra, BlibliAPIData iden, string orderNo, string orderItemNo)
        {
            var result = new tempPackageId_Status();
            var token = SetupContext(iden);
            iden.token = token;

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderDetail?";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            if (iden.versiToken != "2")
            {
                string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
                string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mta/api/businesspartner/v1/order/orderDetail", iden.API_secret_key);

                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderDetail?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString());
                urll += "&storeId=10001";
                urll += "&orderNo=" + Uri.EscapeDataString(orderNo);
                urll += "&orderItemNo=" + Uri.EscapeDataString(orderItemNo);
                urll += "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("requestId", milis.ToString());
                myReq.Headers.Add("sessionId", milis.ToString());
                myReq.Headers.Add("username", userMTA);
            }
            else
            {
                string usernameMO = iden.API_client_username;
                //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                string passMO = iden.API_client_password;
                string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                string passMTA = iden.mta_password_password_merchant;//<-- pass merchant

                urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v1/order/orderDetail?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString());
                urll += "&storeId=10001";
                urll += "&orderNo=" + Uri.EscapeDataString(orderNo);
                urll += "&orderItemNo=" + Uri.EscapeDataString(orderItemNo);
                urll += "&channelId=MasterOnline";

                myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
                myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                myReq.Headers.Add("Signature-Time", milis.ToString());
            }

            string responseFromServer = "";
            using (WebResponse response = await myReq.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseFromServer = reader.ReadToEnd();
                }
            }
            if (responseFromServer != "")
            {
                //dynamic resultRespons = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer);
                var resultRespons = JsonConvert.DeserializeObject(responseFromServer, typeof(BlibliGetOrderDetail)) as BlibliGetOrderDetail;
                if (string.IsNullOrEmpty(Convert.ToString(resultRespons.errorCode)))
                {
                    try
                    {
                        var PackageId = resultRespons.value.packageId;
                        var status = resultRespons.value.orderStatus;
                        if (resultRespons.value.orderHistory.Count() > 0)
                        {
                            status = resultRespons.value.orderHistory.FirstOrDefault().orderStatus;
                        }
                        if (!string.IsNullOrEmpty(PackageId))
                        {
                            result.packageid = PackageId;
                            result.status = status;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }
            return result;
        }
        //end add by nurul 5/4/2022

        public class CekProductRejectResult
        {
            public string requestId { get; set; }
            public object headers { get; set; }
            public object errorMessage { get; set; }
            public object errorCode { get; set; }
            public bool success { get; set; }
            public CekProductRejectResultContent[] content { get; set; }
            public CekProductRejectResultPagemetadata pageMetaData { get; set; }
        }

        public class CekProductRejectResultPagemetadata
        {
            public int pageSize { get; set; }
            public int pageNumber { get; set; }
            public int totalRecords { get; set; }
        }

        public class CekProductRejectResultContent
        {
            public object id { get; set; }
            public object storeId { get; set; }
            public object createdDate { get; set; }
            public object createdBy { get; set; }
            public object updatedDate { get; set; }
            public object updatedBy { get; set; }
            public object version { get; set; }
            public string productName { get; set; }
            public string categoryName { get; set; }
            public string brand { get; set; }
            public long submitDate { get; set; }
            public string initiator { get; set; }
            public string rejectedReason { get; set; }
            public long rejectedDate { get; set; }
            public string productCode { get; set; }
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Blibli Berhasil. Cek Reject Gagal.")]
        public async Task<string> CekProductReject(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, BlibliAPIData iden, string kodeInduk, List<string> merchantskus, string cust, string queue_feed_requestid, string api_log_requestId)
        {
            var token = SetupContext(iden);
            iden.token = token;

            var adaDiReject = false;
            string reasonReject = "";
            foreach (var item in merchantskus)
            {
                long milis = CurrentTimeMillis();
                DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

                //string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                //string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                //string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
                //string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/rejectedProductByMerchantSku", iden.API_secret_key);

                //string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/rejectedProductByMerchantSku?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&username=" + Uri.EscapeDataString(userMTA) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code);
                //urll += "&channelId=MasterOnline&merchantSku=" + Uri.EscapeDataString(item) + "&storeId=10001";
                //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                //myReq.Method = "GET";
                //myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                //myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                //myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                //myReq.Accept = "application/json";
                //myReq.ContentType = "application/json";
                //myReq.Headers.Add("requestId", milis.ToString());
                //myReq.Headers.Add("sessionId", milis.ToString());
                //myReq.Headers.Add("username", userMTA);

                string urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/rejectedProductByMerchantSku?";
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                if (iden.versiToken != "2")
                {
                    string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                    string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                    string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
                    string signature = CreateToken("GET\n\n\n" + milisBack.ToString("ddd MMM dd HH:mm:ss WIB yyyy") + "\n/mtaapi/api/businesspartner/v2/product/rejectedProductByMerchantSku", iden.API_secret_key);

                    urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/rejectedProductByMerchantSku?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&username=" + Uri.EscapeDataString(userMTA) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code);
                    urll += "&channelId=MasterOnline&merchantSku=" + Uri.EscapeDataString(item) + "&storeId=10001";
                    myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    myReq.Headers.Add("Authorization", ("bearer " + iden.token));
                    myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
                    myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
                    myReq.Accept = "application/json";
                    myReq.ContentType = "application/json";
                    myReq.Headers.Add("requestId", milis.ToString());
                    myReq.Headers.Add("sessionId", milis.ToString());
                    myReq.Headers.Add("username", userMTA);
                }
                else
                {
                    string usernameMO = iden.API_client_username;
                    //string passMO = "mta-api-r1O1hntBZOQsQuNpCN5lfTKPIOJbHJk9NWRfvOEEUc3H2yVCKk";
                    string passMO = iden.API_client_password;
                    string apiId = iden.API_client_username + ":" + iden.API_client_password;//<-- diambil dari profil API
                    string userMTA = iden.mta_username_email_merchant;//<-- email user merchant
                    string passMTA = iden.mta_password_password_merchant;//<-- pass merchant
                    urll = "https://api.blibli.com/v2/proxy/mta/api/businesspartner/v2/product/rejectedProductByMerchantSku?requestId=" + Uri.EscapeDataString("MasterOnline-" + milis.ToString()) + "&username=" + Uri.EscapeDataString(userMTA) + "&businessPartnerCode=" + Uri.EscapeDataString(iden.merchant_code);
                    urll += "&channelId=MasterOnline&merchantSku=" + Uri.EscapeDataString(item) + "&storeId=10001";
                    myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameMO + ":" + passMO))));
                    myReq.Accept = "application/json";
                    myReq.ContentType = "application/json";
                    myReq.Headers.Add("Api-Seller-Key", iden.API_secret_key.ToString());
                    myReq.Headers.Add("Signature-Time", milis.ToString());
                }

                string responseFromServer = "";
                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
                if (responseFromServer != "")
                {

                    var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(CekProductRejectResult)) as CekProductRejectResult;
                    if (listBrg != null)
                    {
                        if (listBrg.success)
                        {
                            if (listBrg.content.Count() > 0)
                            {
                                foreach (var itemReject in listBrg.content)
                                {
                                    adaDiReject = true;
                                    reasonReject = itemReject.rejectedReason;
                                }
                            }
                        }
                        else
                        {
                            throw new Exception(Convert.ToString(listBrg.errorMessage));
                        }
                    }
                }
            }
            if (adaDiReject)
            {
                using (SqlConnection oConnection = new SqlConnection(EDB.GetConnectionString("sConn")))
                {
                    oConnection.Open();
                    using (SqlCommand oCommand = oConnection.CreateCommand())
                    {
                        oCommand.CommandType = CommandType.Text;
                        oCommand.CommandText = "UPDATE [QUEUE_FEED_BLIBLI] SET [STATUS] = '2' WHERE [REQUESTID] = '" + queue_feed_requestid + "' AND [MERCHANT_CODE]=@MERCHANTCODE AND [STATUS] = '1'";
                        oCommand.Parameters.Add(new SqlParameter("@MERCHANTCODE", SqlDbType.NVarChar, 10));
                        oCommand.Parameters[0].Value = Convert.ToString(iden.merchant_code);
                        oCommand.ExecuteNonQuery();

                        MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                        {
                            REQUEST_ID = api_log_requestId
                        };
                        currentLog.REQUEST_RESULT = Convert.ToString(reasonReject);
                        currentLog.REQUEST_EXCEPTION = "";
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);

                        var getLogMarketplace = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == api_log_requestId).FirstOrDefault();
                        if (getLogMarketplace != null)
                        {
                            oCommand.CommandType = CommandType.Text;
                            //changed 18 aug 2021, jika sudah reject hilangkan saja status pending nya
                            //oCommand.CommandText = "UPDATE H SET BRG_MP='PENDING' FROM STF02H H INNER JOIN ARF01 A ON H.IDMARKET = A.RECNUM WHERE H.BRG=@MERCHANTSKU AND A.SORT1_CUST=@MERCHANTCODE AND ISNULL(H.BRG_MP,'') = 'PENDING'";
                            oCommand.CommandText = "UPDATE H SET BRG_MP='' FROM STF02H H INNER JOIN ARF01 A ON H.IDMARKET = A.RECNUM WHERE H.BRG=@MERCHANTSKU AND A.SORT1_CUST=@MERCHANTCODE AND ISNULL(H.BRG_MP,'') = 'PENDING'";
                            //end changed 18 aug 2021, jika sudah reject hilangkan saja status pending nya
                            //oCommand.Parameters.Add(new SqlParameter("@MERCHANTSKU", SqlDbType.NVarChar, 20));
                            oCommand.Parameters[1].Value = Convert.ToString(getLogMarketplace.REQUEST_ATTRIBUTE_1);
                            oCommand.ExecuteNonQuery();

#region Create Log Error khusus create barang
                            string subjectDescription = Convert.ToString(getLogMarketplace.REQUEST_ATTRIBUTE_1).Replace("'", "`");
                            string CUST = Convert.ToString(getLogMarketplace.CUST); //mengambil Cust
                            string ActionCategory = Convert.ToString("Barang"); //mengambil Kategori
                            string ActionName = Convert.ToString("Buat Produk"); //mengambil Action
                            string exceptionMessage = "From Blibli API : " + Convert.ToString(reasonReject);
                            string jobId = Convert.ToString(getLogMarketplace.REQUEST_ATTRIBUTE_3); // Hangfire Job Id saat Create Produk

                            string sSQL = "INSERT INTO API_LOG_MARKETPLACE (REQUEST_STATUS,CUST_ATTRIBUTE_1,CUST_ATTRIBUTE_2,CUST,MARKETPLACE,REQUEST_ID,";
                            sSQL += "REQUEST_ACTION,REQUEST_DATETIME,";
                            sSQL += "REQUEST_ATTRIBUTE_3, REQUEST_ATTRIBUTE_4,REQUEST_ATTRIBUTE_5,";
                            sSQL += "REQUEST_RESULT,REQUEST_EXCEPTION) ";
                            sSQL += "SELECT 'FAILED',A.CUST_ATTRIBUTE_1, '1', A.CUST,A.MARKETPLACE,A.REQUEST_ID,A.REQUEST_ACTION,A.REQUEST_DATETIME,A.REQUEST_ATTRIBUTE_3,A.REQUEST_ATTRIBUTE_4,A.REQUEST_ATTRIBUTE_5,A.REQUEST_RESULT,A.REQUEST_EXCEPTION ";
                            sSQL += "FROM ( SELECT '" + subjectDescription + "' CUST_ATTRIBUTE_1,'" + CUST + "' CUST,(SELECT TOP 1 B.NAMAMARKET FROM ARF01 A INNER JOIN MO.DBO.MARKETPLACE B ON A.NAMA = B.IDMARKET AND A.CUST='" + CUST + "') MARKETPLACE, '" + jobId + "' REQUEST_ID, ";
                            sSQL += "'" + ActionName + "' REQUEST_ACTION, '" + getLogMarketplace.REQUEST_DATETIME.ToString("yyyy-MM-dd HH:mm:ss") + "' REQUEST_DATETIME, ";
                            sSQL += "'" + ActionCategory + "' REQUEST_ATTRIBUTE_3,'" + subjectDescription + "' REQUEST_ATTRIBUTE_4, 'HANGFIRE' REQUEST_ATTRIBUTE_5, ";
                            sSQL += "'Create Product " + subjectDescription + " ke Blibli Gagal.' REQUEST_RESULT, '" + exceptionMessage.Replace("'", "`") + "' REQUEST_EXCEPTION ) A ";
                            sSQL += "LEFT JOIN API_LOG_MARKETPLACE B ON B.REQUEST_ATTRIBUTE_5 = 'HANGFIRE' AND A.REQUEST_ACTION = B.REQUEST_ACTION AND A.CUST = B.CUST AND A.CUST_ATTRIBUTE_1 = B.CUST_ATTRIBUTE_1 WHERE ISNULL(B.RECNUM,0) = 0 ";
                            int adaInsert = EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);
                            if (adaInsert == 0) //JIKA 
                            {
                                //update REQUEST_STATUS = 'FAILED', DATE, FAIL COUNT
                                sSQL = "UPDATE B SET REQUEST_STATUS = 'FAILED', REQUEST_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', CUST_ATTRIBUTE_2 = CONVERT(INT,CUST_ATTRIBUTE_2) + 1 ";
                                sSQL += "FROM API_LOG_MARKETPLACE B WHERE B.REQUEST_ATTRIBUTE_5 = 'HANGFIRE' AND B.REQUEST_STATUS = 'RETRYING' AND B.REQUEST_ID = '" + jobId + "'";
                                EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);

                                //update JOBID MENJADI JOBID BARU JIKA TIDAK SEDANG RETRY,STATUS,DATE,FAIL COUNT
                                sSQL = "UPDATE B SET REQUEST_STATUS = 'FAILED', REQUEST_ID = '" + jobId + "', REQUEST_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', CUST_ATTRIBUTE_2 = CONVERT(INT,CUST_ATTRIBUTE_2) + 1 ";
                                sSQL += "FROM API_LOG_MARKETPLACE B INNER JOIN ";
                                sSQL += "( SELECT '" + subjectDescription + "' CUST_ATTRIBUTE_1,'" + CUST + "' CUST,(SELECT TOP 1 B.NAMAMARKET FROM ARF01 A INNER JOIN MO.DBO.MARKETPLACE B ON A.NAMA = B.IDMARKET AND A.CUST='" + CUST + "') MARKETPLACE, '" + jobId + "' REQUEST_ID, ";
                                sSQL += "'" + ActionName + "' REQUEST_ACTION, '" + getLogMarketplace.REQUEST_DATETIME.ToString("yyyy-MM-dd HH:mm:ss") + "' REQUEST_DATETIME, ";
                                sSQL += "'" + ActionCategory + "' REQUEST_ATTRIBUTE_3,'" + subjectDescription + "' REQUEST_ATTRIBUTE_4, 'HANGFIRE' REQUEST_ATTRIBUTE_5, ";
                                sSQL += "'Create Product " + subjectDescription + " ke Blibli Gagal.' REQUEST_RESULT, '" + exceptionMessage.Replace("'", "`") + "' REQUEST_EXCEPTION ) A ";
                                sSQL += "ON B.REQUEST_ATTRIBUTE_5 = 'HANGFIRE' AND A.REQUEST_ACTION = B.REQUEST_ACTION AND A.CUST = B.CUST AND A.CUST_ATTRIBUTE_1 = B.CUST_ATTRIBUTE_1 AND B.REQUEST_STATUS IN ('FAILED','RETRYING')";
                                EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);
                            }
                            sSQL = "UPDATE S SET LINK_STATUS='Buat Produk Berhasil, Rejected by Blibli', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', ";
                            //jobid;request_action;request_result;request_exception
                            string Link_Error = jobId + ";" + ActionName + ";Create Product " + subjectDescription + " ke Blibli Berhasil, tetapi Rejected by Blibli;" + exceptionMessage.Replace("'", "`");
                            sSQL += "LINK_ERROR = '" + Link_Error + "' FROM STF02H S INNER JOIN ARF01 A ON S.IDMARKET = A.RECNUM AND A.CUST = '" + CUST + "' WHERE S.BRG = '" + subjectDescription + "' ";
                            EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);
#endregion
                        }
                    }
                }

            }
            else
            {
                var dateRequest = (new DateTime(1970, 1, 1)).AddMilliseconds(double.Parse(api_log_requestId));
                if (dateRequest <= DateTime.UtcNow.AddHours(7).AddDays(-1))
                {

                    var tblCustomer = ErasoftDbContext.ARF01.Where(m => m.CUST == log_CUST).FirstOrDefault();
                    EDB.ExecuteSQL("CString", CommandType.Text, "UPDATE STF02H SET BRG_MP = '' WHERE BRG = '" + kodeProduk + "' AND IDMARKET = " + tblCustomer.RecNum);
                    string sSQL = "DELETE FROM API_LOG_MARKETPLACE WHERE REQUEST_ATTRIBUTE_5 = 'HANGFIRE' AND REQUEST_ACTION = 'Buat Produk' AND CUST = '" + tblCustomer.CUST + "' AND CUST_ATTRIBUTE_1 = '" + kodeProduk + "'";
                    EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);

                    EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE [QUEUE_FEED_BLIBLI] SET [STATUS] = '2' WHERE [REQUESTID] = '" + queue_feed_requestid + "' AND [MERCHANT_CODE]='" + iden.merchant_code + "' AND [STATUS] = '1'");
#if (DEBUG || Debug_AWS)
                    await CreateProduct(dbPathEra, kodeProduk, tblCustomer.CUST, "Barang", "Buat Produk", iden, null, null);
#else
                    var EDBConnID = EDB.GetConnectionString("ConnID");
                    var sqlStorage = new SqlServerStorage(EDBConnID);
                    var clientJobServer = new BackgroundJobClient(sqlStorage);
                    clientJobServer.Enqueue<BlibliControllerJob>(x => x.CreateProduct(dbPathEra, kodeProduk, tblCustomer.CUST, "Barang", "Buat Produk", iden, null, null));
#endif

                }
            }
            return "";
        }

        public static Bitmap BlibliResizeImageFromStream(MemoryStream stream)
        {
            using (var img = Image.FromStream(stream))
            {
                float newResolution = img.Height;
                if (img.Width < newResolution)
                {
                    newResolution = img.Width;
                }
                var destRect = new Rectangle(0, 0, Convert.ToInt32(newResolution), Convert.ToInt32(newResolution));
                var destImage = new Bitmap(Convert.ToInt32(newResolution), Convert.ToInt32(newResolution));

                //var newWidth = (int)(srcImage.Width * scaleFactor);
                //var newHeight = (int)(srcImage.Height * scaleFactor);
                var newWidth = (int)(newResolution);
                var newHeight = (int)(newResolution);
                using (var newImage = new Bitmap(newWidth, newHeight))
                using (var graphics = Graphics.FromImage(newImage))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(img, destRect);
                    //newImage.Save(outputFile);
                }
                return destImage;
            }
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
        public class ReviseProductBlibliData
        {
            public ReviseProductData product { get; set; }
            public RevisepickupPoint pickupPoint { get; set; }
            public ReviseCategoryCode category { get; set; }
            public ReviseBrand brand { get; set; }
            //public string url { get; set; }
            public ReviseDimension dimension { get; set; }
            //public string productStory { get; set; }
            public Dictionary<string, string> nonDefiningAttributes { get; set; }
            public Dictionary<string, string[]> definingAttributes { get; set; }
            public List<ReviseProductitem> productItems { get; set; }
            public ReviseImageMap[] imageMap { get; set; }
        }
        public class ReviseProductitem
        {
            public string upcCode { get; set; }
            public string sellerSku { get; set; }
            public ReviseProductPrice price { get; set; }
            //public int salePrice { get; set; }
            public int stock { get; set; }
            public int minimumStock { get; set; }
            public bool displayable { get; set; }
            public bool buyable { get; set; }
            public string[] images { get; set; }
            //public int dangerousGoodsLevel { get; set; }
            public Dictionary<string, string> attributesMap { get; set; }
        }
        public class ReviseProductPrice
        {
            public int regular { get; set; }
            public int sale { get; set; }

        }
        public class ReviseDimension
        {
            public int length { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public int weight { get; set; }

        }
        public class ReviseBrand
        {
            public string code { get; set; }

        }
        public class ReviseCategoryCode
        {
            public string code { get; set; }

        }
        public class RevisepickupPoint
        {
            public string code { get; set; }

        }
        public class ReviseImageMap
        {
            public string data { get; set; }
            public string name { get; set; }
            public string path { get; set; }
        }
        public class ReviseProductData
        {
            public string name { get; set; }
            public string description { get; set; }
            public int productType { get; set; }
            public string uniqueSellingPoint { get; set; }
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
            public object errorMessage { get; set; }
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
            public string orderType { get; set; }
            public int qty { get; set; }
            public long orderDate { get; set; }
            public long? autoCancelDate { get; set; }
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
            public bool? installationRequired { get; set; }
            //public object awbNumber { get; set; }
            public string awbNumber { get; set; }
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
            //public object startOperationalTime { get; set; }
            public long? startOperationalTime { get; set; }

            //public object endOperationalTime { get; set; }
            public long? endOperationalTime { get; set; }

            //public object issuer { get; set; }
            public string issuer { get; set; }

            //public object refundResolution { get; set; }
            public string refundResolution { get; set; }

            //public object unFullFillReason { get; set; }
            public string unFullFillReason { get; set; }
            public object unFullFillQuantity { get; set; }
            public string productTypeCode { get; set; }
            public string productTypeName { get; set; }
            public string custNote { get; set; }
            public string shippingRecipientName { get; set; }
            public string logisticsProductCode { get; set; }
            public string logisticsProductName { get; set; }
            public string logisticsOptionCode { get; set; }
            public object originLongitude { get; set; }
            public object originLatitude { get; set; }
            public float? destinationLongitude { get; set; }
            public float? destinationLatitude { get; set; }
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
            public float finalPrice { get; set; }
            public float finalPriceTotal { get; set; }
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
        public class ReviseProductResult
        {
            public string requestId { get; set; }
            public object headers { get; set; }
            public object errorMessage { get; set; }
            public object errorCode { get; set; }
            public bool success { get; set; }
            public ReviseProductResultValue content { get; set; }
        }
        public class CreateProductResultValue
        {
            public string queueFeedId { get; set; }
            public string requestAction { get; set; }
            public int total { get; set; }
            public long timeStamp { get; set; }
        }
        public class ReviseProductResultValue
        {
            public string queueId { get; set; }
            public string action { get; set; }
            public long timeStamp { get; set; }
        }

        public class BlibliDetailProductResult
        {
            public string requestId { get; set; }
            public object headers { get; set; }
            public object errorMessage { get; set; }
            public object errorCode { get; set; }
            public bool success { get; set; }
            public BlibliDetailProductResultValue value { get; set; }
        }

        public class BlibliDetailProductResultValue
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
            public BlibliDetailProductResultItem[] items { get; set; }
            public BlibliDetailProductResultAttribute[] attributes { get; set; }
            public BlibliDetailProductResultImage1[] images { get; set; }
            public string url { get; set; }
            public bool installationRequired { get; set; }
            public string categoryId { get; set; }
        }

        public class BlibliDetailProductResultItem
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
            public BlibliDetailProductResultPrice[] prices { get; set; }
            public BlibliDetailProductResultViewconfig[] viewConfigs { get; set; }
            public BlibliDetailProductResultImage[] images { get; set; }
            public object cogs { get; set; }
            public string cogsErrorCode { get; set; }
            public bool promoBundling { get; set; }
        }

        public class BlibliDetailProductResultPrice
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

        public class BlibliDetailProductResultViewconfig
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

        public class BlibliDetailProductResultImage
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

        public class BlibliDetailProductResultAttribute
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

        public class BlibliDetailProductResultImage1
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

        //add by nurul 25/1/2021, bundling
        public class tempOrderCount
        {
            public int Count { get; set; }
            public bool AdaKomponen { get; set; }
        }
        //end add by nurul 25/1/2021, bundling 
    }
}