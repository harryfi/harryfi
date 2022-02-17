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
using System.Net.Http;
using Hangfire;
using Hangfire.SqlServer;
using MasterOnline.Utils;
using Microsoft.Ajax.Utilities;

namespace MasterOnline.Controllers
{
    public class TokopediaControllerJob : Controller
    {
        //AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);//string auth = Base64Encode();

        private MoDbContext MoDbContext { get; set; }
        private ErasoftContext ErasoftDbContext { get; set; }
        private DatabaseSQL EDB;
        private string username;
        public TokopediaControllerJob()
        {
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
        protected string SetupContextForGetToken(TokopediaAPIData data)
        {
            string ret = "";
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(data.DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, data.DatabasePathErasoft);
            username = data.username;
            return ret;
        }
        protected string SetupContext(TokopediaAPIData data)
        {
            string ret = "";
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(data.DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, data.DatabasePathErasoft);
            username = data.username;
            var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.RecNum == data.idmarket).SingleOrDefault();
            if (arf01inDB != null)
            {
                var tokenRet = GetToken(data);

                ret = tokenRet.access_token;
            }
            else
            {
                try
                {
                    //stop recurring job karena arf01 not found
                    string EDBConnID = EDB.GetConnectionString("ConnID");
                    var sqlStorage = new SqlServerStorage(EDBConnID);
                    var client = new BackgroundJobClient(sqlStorage);
                    RecurringJobManager recurJobM = new RecurringJobManager(sqlStorage);
                    string connId_JobId = "";
                    string dbPathEra = data.DatabasePathErasoft;

                    connId_JobId = dbPathEra + "_tokopedia_check_pending_" + Convert.ToString(data.idmarket);
                    recurJobM.RemoveIfExists(connId_JobId);

                    connId_JobId = dbPathEra + "_tokopedia_pesanan_paid_" + Convert.ToString(data.idmarket);
                    recurJobM.RemoveIfExists(connId_JobId);

                    connId_JobId = dbPathEra + "_tokopedia_pesanan_completed_" + Convert.ToString(data.idmarket);
                    recurJobM.RemoveIfExists(connId_JobId);
                }
                catch (Exception ex)
                {

                }
            }
            return ret;
        }

        //[AutomaticRetry(Attempts = 0)]
        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new int[] { 300 })]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Tokopedia Gagal.")]
        public async Task<string> CreateProductGetStatus(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, string brg, int upload_id, string log_request_id)
        {
            //if merchant code diisi. barulah GetOrderList
            string ret = "";
            var token = SetupContext(iden);
            iden.token = token;
            long milis = CurrentTimeMillis();

            var cekInDB = ErasoftDbContext.STF02H.Where(m => m.BRG == kodeProduk && m.IDMARKET == iden.idmarket).FirstOrDefault();
            if(cekInDB != null)
            {
                if (!string.IsNullOrEmpty(cekInDB.BRG_MP))
                {
                    if (!cekInDB.BRG_MP.Contains("PENDING;"))//sudah berhasil link
                    {
                        return "";
                    }
                }
            }

            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            //string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/create/status?shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&upload_id=" + Uri.EscapeDataString(Convert.ToString(upload_id));
            string urll = "https://fs.tokopedia.net/v2/products/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/status/" + Uri.EscapeDataString(Convert.ToString(upload_id)) + "?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = log_request_id
            };

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            //try
            //{
            string err = "";
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
                //remark 8 des 2020, kalau exception perlu di retry
                //var failed = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '' WHERE BRG = '" + Convert.ToString(brg) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                //end remark 8 des 2020, kalau exception perlu di retry

                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                    currentLog.REQUEST_EXCEPTION = err;
                    //throw new Exception(err);
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = e.InnerException == null ? e.Message : e.InnerException.Message;
                    //throw e;
                }
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                //add 4 des 2020, set status pending
                string Link_Error = "0;Buat Produk;;";//jobid;request_action;request_result;request_exception
                var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = 'PENDING;" + Convert.ToString(upload_id) + ";" + Convert.ToString(log_request_id) + "',LINK_STATUS='Buat Produk Pending', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '" + Link_Error + "' WHERE BRG = '" + Convert.ToString(brg) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                //end add 4 des 2020, set status pending
                throw new Exception(currentLog.REQUEST_EXCEPTION);
            }
            //if (err != "")
            //{
            //    var resultErr = Newtonsoft.Json.JsonConvert.DeserializeObject(err, typeof(CreateProductGetStatusResult)) as CreateProductGetStatusResult;
            //    if (resultErr.header.messages != "" && resultErr.header.reason == "error get upload id")
            //    {
            //        var token1 = SetupContext(iden);
            //        iden.token = token1;
            //        long milis1 = CurrentTimeMillis();
            //        DateTime milisBack1 = DateTimeOffset.FromUnixTimeMilliseconds(milis1).UtcDateTime.AddHours(7);
            //        string urll1 = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/create/status?shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&upload_id=" + Uri.EscapeDataString(Convert.ToString(upload_id));

            //        HttpWebRequest myReq1 = (HttpWebRequest)WebRequest.Create(urll1);
            //        myReq.Method = "GET";
            //        myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            //        myReq.Accept = "application/x-www-form-urlencoded";
            //        myReq.ContentType = "application/json";
            //        try
            //        {
            //            using (WebResponse response = await myReq1.GetResponseAsync())
            //            {
            //                using (Stream stream = response.GetResponseStream())
            //                {
            //                    StreamReader reader = new StreamReader(stream);
            //                    responseFromServer = reader.ReadToEnd();
            //                }
            //            }
            //        }
            //        catch (WebException ex)
            //        {
            //            string err1 = "";
            //            if (ex.Status == WebExceptionStatus.ProtocolError)
            //            {
            //                WebResponse resp1 = ex.Response;
            //                using (StreamReader sr1 = new StreamReader(resp1.GetResponseStream()))
            //                {
            //                    err1 = sr1.ReadToEnd();
            //                }
            //            }
            //            throw new Exception(err1);
            //        }
            //    }
            //}
            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}

            if (responseFromServer != "")
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(CreateProductGetStatusResult)) as CreateProductGetStatusResult;
                //if (result.header.error_code == "")
                {
                    //if (result.data.upload_data.Count() > 0)
                    if (result.data != null)
                    {
                        //foreach (var item in result.data.upload_data)
                        {
                            if (result.data.unprocessed_rows > 0)
                            {
                                string Link_Error = "0;Buat Produk;;";//jobid;request_action;request_result;request_exception
                                var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = 'PENDING;" + Convert.ToString(upload_id) + ";" + Convert.ToString(log_request_id) + "',LINK_STATUS='Buat Produk Pending', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '" + Link_Error + "' WHERE BRG = '" + Convert.ToString(brg) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                                if (success == 1)
                                {
                                    manageAPI_LOG_MARKETPLACE(api_status.RePending, ErasoftDbContext, iden, currentLog);
                                }
                                throw new Exception("Create/Edit Product still unprocessed.");
                            }
                            else if (result.data.success_rows > 0)
                            {
//                                if(result.data.success_rows_data != null)
//                                {
//                                    if (result.data.success_rows_data.Length > 0)
//                                    {
//#if (DEBUG || Debug_AWS)
//                                        await new TokopediaControllerJob().getItemDetail(iden.DatabasePathErasoft, brg, log_CUST, "Barang", "Link Produk (Tahap 2 / 2) V2", iden, iden.idmarket, 1, result.data.success_rows_data[0].product_id);
//#else
//                                string EDBConnID = EDB.GetConnectionString("ConnId");
//                                var sqlStorage = new SqlServerStorage(EDBConnID);

//                                var Jobclient = new BackgroundJobClient(sqlStorage);
//                                Jobclient.Enqueue<TokopediaControllerJob>(x => x.getItemDetail(iden.DatabasePathErasoft, brg, log_CUST, "Barang", "Link Produk (Tahap 2 / 2) V2", iden, iden.idmarket, 1, result.data.success_rows_data[0].product_id));                                
//#endif
//                                        return ret;
//                                    }
//                                }
#if (DEBUG || Debug_AWS)
                                await new TokopediaControllerJob().GetActiveItemListBySKU(iden.DatabasePathErasoft, brg, log_CUST, "Barang", "Link Produk (Tahap 2 / 2)", iden, 0, 50, iden.idmarket, brg, currentLog.REQUEST_ID);
#else
                                //change by calvin 9 juni 2019
                                //await GetActiveItemListBySKU(iden, 0, 50, iden.idmarket, brg);
                                string EDBConnID = EDB.GetConnectionString("ConnId");
                                var sqlStorage = new SqlServerStorage(EDBConnID);

                                var Jobclient = new BackgroundJobClient(sqlStorage);
                                Jobclient.Enqueue<TokopediaControllerJob>(x => x.GetActiveItemListBySKU(iden.DatabasePathErasoft, brg, log_CUST, "Barang", "Link Produk (Tahap 2 / 2)", iden, 0, 50, iden.idmarket, brg, currentLog.REQUEST_ID));
                                //end change by calvin 9 juni 2019
                                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
#endif
                            }
                            else if (result.data.failed_rows > 0)
                            {
                                if (result.data.failed_rows_data != null)
                                {
                                    foreach (var item_failed in result.data.failed_rows_data)
                                    {
                                        currentLog.REQUEST_RESULT = item_failed.error[0];
                                        currentLog.REQUEST_EXCEPTION = item_failed.error[0];
                                        var failed = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '' WHERE BRG = '" + Convert.ToString(brg) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                                    }
                                }

                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                                throw new Exception(currentLog.REQUEST_RESULT + ";" + currentLog.REQUEST_EXCEPTION);
                            }
                        }
                    }
                }
                //else
                //{
                //    currentLog.REQUEST_RESULT = result.header.reason;
                //    currentLog.REQUEST_EXCEPTION = result.header.messages;
                //    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                //    throw new Exception(result.header.messages + ";" + result.header.reason);
                //}
            }

            return ret;
        }

        public async Task<string> getItemDetail(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, int recnumArf01, int retry, long product_id)
        {
            var ret = "";

            var token = SetupContext(iden);
            iden.token = token;

            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/info?product_id=" + Uri.EscapeDataString(product_id.ToString());
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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
            catch (WebException e)
            {
                string err = e.Message;
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                    if (err.Contains("Too Many Request") && retry <= 3)
                    {
                        await Task.Delay(retry * 1000);
                        await getItemDetail(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, recnumArf01, retry + 1, product_id);
                        return "";
                    }
                    else
                    {
                        throw new Exception(err);
                    }
                }
                else
                {
                    throw e;
                }
            }
            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaController.TokpedGetItemDetail)) as TokopediaController.TokpedGetItemDetail;
                bool adaError = false;
                if (result.data != null)
                {
                    if (result.data.Count() == 0)
                    {
                        adaError = true;
                    }

                }
                if (!adaError)
                {
                    string urlBrg = "";
                    //if(result.data[0].other != null)
                    //{
                    //    urlBrg = result.data[0].other.url;
                    //}
                    string sSQL = "UPDATE STF02H SET BRG_MP = '"+ product_id + "', AVALUE_34 = '"+ result.data[0].other.url 
                        + "', LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss")
                        + "',LINK_ERROR = '0;;;' WHERE BRG = '" + result.data[0].other.sku + "' AND IDMARKET = '" + recnumArf01 + "'";
                    EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                    if(result.data[0].variant != null)
                    {
                        if (result.data[0].variant.isParent)
                        {
                            if (result.data[0].variant.childrenID != null)
                            {
                                if (result.data[0].variant.childrenID.Length > 0)
                                {
                                    foreach (var varid in result.data[0].variant.childrenID)
                                    {
                                        await getItemDetail(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, recnumArf01, retry, varid);
                                    }
                                }
                            }
                        }
                        else
                        {

                        }
                    }
                    else
                    {

                    }
                }
            }
            return ret;
        }
        public async Task<string> getItemDetailToLink(string dbPathEra, string kodeProduk, string log_CUST, TokopediaAPIData iden, int recnumArf01, int retry, long product_id)
        {
            var ret = "";

            var token = SetupContext(iden);
            iden.token = token;

            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/info?product_id=" + Uri.EscapeDataString(product_id.ToString());
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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
            catch (WebException e)
            {
                string err = e.Message;
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                    if (err.Contains("Too Many Request") && retry <= 3)
                    {
                        await Task.Delay(retry * 1000);
                        await getItemDetailToLink(dbPathEra, kodeProduk, log_CUST, iden, recnumArf01, retry + 1, product_id);
                        return "";
                    }
                    else
                    {
                        throw new Exception(err);
                    }
                }
                else
                {
                    throw e;
                }
            }
            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaController.TokpedGetItemDetail)) as TokopediaController.TokpedGetItemDetail;
                bool adaError = false;
                if (result.data != null)
                {
                    if (result.data.Count() == 0)
                    {
                        adaError = true;
                    }

                }
                if (!adaError)
                {
                    string sSQL = "UPDATE STF02H SET BRG_MP = '" + product_id + "', AVALUE_34 = '" + result.data[0].other.url
                        + "', LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss")
                        + "',LINK_ERROR = '0;;;' WHERE BRG = '" + result.data[0].other.sku + "' AND IDMARKET = '" + recnumArf01 + "'";
                    EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                    
                }
            }
            return ret;
        }
        public async Task<List<STF02E>> getLogistic(TokopediaAPIData iden)
        {
            var ret = new List<STF02E>();

            var token = SetupContext(iden);
            iden.token = token;

            string urll = "https://fs.tokopedia.net/v1/logistic/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/active-info?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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
            catch (WebException e)
            {
                string err = e.Message;
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                   
                }
                
            }
            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ResultGetActiveCourier)) as ResultGetActiveCourier;
                if (string.IsNullOrWhiteSpace(result.header.reason))
                {
                    foreach(var a1 in result.data.Shops[0].ShipmentInfos)
                    {
                        foreach(var a2 in a1.ShipmentPackages)
                        {
                            if(a2.IsAvailable == 1)
                            {
                                var courier = new STF02E
                                {
                                    KET = a1.ShipmentName + " " + a2.ProductName,
                                    KODE = a2.ShippingProductID.ToString()
                                };
                                ret.Add(courier);
                            }
                        }
                    }
                }
            }
            return ret;
        }
        //[AutomaticRetry(Attempts = 0)]
        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new int[] { 300 })]
        [Queue("1_create_product")]
        [NotifyOnFailed("Edit Product {obj} ke Tokopedia Gagal.")]
        public async Task<string> EditProductGetStatus(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, string brg, int upload_id, string log_request_id, string product_id)
        {
            //if merchant code diisi. barulah GetOrderList
            string ret = "";
            long milis = CurrentTimeMillis();

            var token = SetupContext(iden);
            iden.token = token;

            //var cekInDB = ErasoftDbContext.STF02H.Where(m => m.BRG == kodeProduk && m.IDMARKET == iden.idmarket).FirstOrDefault();
            //if (cekInDB != null)
            //{
            //    if (!string.IsNullOrEmpty(cekInDB.BRG_MP))
            //    {
            //        if (!cekInDB.BRG_MP.Contains("PEDITENDING;"))//sudah berhasil link
            //        {
            //            return "";
            //        }
            //    }
            //}
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            //string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/edit/status?shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&upload_id=" + Uri.EscapeDataString(Convert.ToString(upload_id));
            string urll = "https://fs.tokopedia.net/v2/products/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/status/" + Uri.EscapeDataString(Convert.ToString(upload_id)) + "?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = log_request_id
            };

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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
            catch (WebException e)
            {
                string err = "";
                //remark 8 des 2020, kalau exception perlu di retry
                //var failed = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(product_id) + "' WHERE BRG = '" + Convert.ToString(brg) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                //end remark 8 des 2020, kalau exception perlu di retry
                
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                        //throw new Exception(err);//add by Tri 23 apr 2020
                        currentLog.REQUEST_EXCEPTION = err;
                    }
                }
                else
                {
                    //throw e;
                    currentLog.REQUEST_EXCEPTION = e.InnerException == null ? e.Message : e.InnerException.Message;
                }
                //throw new Exception(e.Message);//add by Tri 23 apr 2020
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                //add 4 des 2020, set status pending
                EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = 'PEDITENDING;" + Convert.ToString(upload_id) + ";" + Convert.ToString(log_request_id) + ";" + Convert.ToString(product_id) + "' WHERE BRG = '" + Convert.ToString(brg) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                //end add 4 des 2020, set status pending

                throw new Exception(currentLog.REQUEST_EXCEPTION);
            }

            if (responseFromServer != "")
            {

                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(CreateProductGetStatusResult)) as CreateProductGetStatusResult;
                //if (result.header.error_code == "")
                {
                    //if (result.data.upload_data.Count() > 0)
                    if (result.data != null)
                    {
                        //foreach (var item in result.data.upload_data)
                        {
                            if (result.data.unprocessed_rows > 0)
                            {
                                var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = 'PEDITENDING;" + Convert.ToString(upload_id) + ";" + Convert.ToString(log_request_id) + ";" + Convert.ToString(product_id) + "' WHERE BRG = '" + Convert.ToString(brg) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                                if (success == 1)
                                {
                                    manageAPI_LOG_MARKETPLACE(api_status.RePending, ErasoftDbContext, iden, currentLog);
                                }
                                throw new Exception("Create/Edit Product still unprocessed.");
                            }
                            else if (result.data.success_rows > 0)
                            {
#if (DEBUG || Debug_AWS)
                                await GetActiveItemListBySKU(iden.DatabasePathErasoft, brg, log_CUST, "Barang", "Link Produk (Tahap 2 / 2)", iden, 0, 50, iden.idmarket, brg, currentLog.REQUEST_ID);
#else
                                //change by calvin 9 juni 2019
                                //await GetActiveItemVariantByProductID(iden, brg, iden.idmarket, Convert.ToString(product_id));
                                string EDBConnID = EDB.GetConnectionString("ConnId");
                                var sqlStorage = new SqlServerStorage(EDBConnID);

                                var Jobclient = new BackgroundJobClient(sqlStorage);
                                Jobclient.Enqueue<TokopediaControllerJob>(x => x.GetActiveItemListBySKU(iden.DatabasePathErasoft, brg, log_CUST, "Barang", "Link Produk (Tahap 2 / 2)", iden, 0, 50, iden.idmarket, brg, currentLog.REQUEST_ID));
                                //end change by calvin 9 juni 2019

                                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
#endif
                            }
                            else if (result.data.failed_rows > 0)
                            {
                                if (result.data.failed_rows_data != null)
                                {
                                    foreach (var item_failed in result.data.failed_rows_data)
                                    {
                                        currentLog.REQUEST_RESULT = item_failed.error[0];
                                        currentLog.REQUEST_EXCEPTION = item_failed.error[0];
                                        var failed = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(product_id) + "' WHERE BRG = '" + Convert.ToString(brg) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                                    }
                                }

                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                                throw new Exception(currentLog.REQUEST_RESULT + ";" + currentLog.REQUEST_EXCEPTION + ";PEDITFAILED;" + Convert.ToString(upload_id) + ";" + Convert.ToString(log_request_id) + ";");
                            }
                        }
                    }
                }
                //else
                //{
                //    currentLog.REQUEST_RESULT = result.header.reason;
                //    currentLog.REQUEST_EXCEPTION = result.header.messages;
                //    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                //    throw new Exception(result.header.messages + ";" + result.header.reason);
                //}
            }

            return ret;
        }

        public class CreateProductGetStatusResult
        {
            public CreateProductGetStatusResultHeader header { get; set; }
            public CreateProductGetStatusResultUpload_Data data { get; set; }
        }

        public class CreateProductGetStatusResultHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            //public int error_code { get; set; }
        }

        public class CreateProductGetStatusResultData
        {
            public CreateProductGetStatusResultUpload_Data[] upload_data { get; set; }
        }


        public class CreateProductGetStatusResultUpload_Data
        {
            public int upload_id { get; set; }
            public string status { get; set; }
            public int total_data { get; set; }
            public int unprocessed_rows { get; set; }
            public int success_rows { get; set; }
            public int failed_rows { get; set; }
            public CreateProductGetStatusResultFailed_Rows_Data[] failed_rows_data { get; set; }
            public CreateProductGetStatusResultSuccess_Rows_Data[] success_rows_data { get; set; }
            public int processed { get; set; }
        }
        public class CreateProductGetStatusResultSuccess_Rows_Data
        {
            public long product_id { get; set; }
        }

        public class CreateProductGetStatusResultFailed_Rows_Data
        {
            public string product_name { get; set; }
            public int product_price { get; set; }
            public string sku { get; set; }
            public string[] error { get; set; }
        }


        //public class EditProductGetStatusResult
        //{
        //    public EditProductGetStatusResultHeader header { get; set; }
        //    public EditProductGetStatusResultData[] data { get; set; }
        //}

        //public class EditProductGetStatusResultHeader
        //{
        //    public float process_time { get; set; }
        //    public string messages { get; set; }
        //    public string reason { get; set; }
        //    public int error_code { get; set; }
        //}

        //public class EditProductGetStatusResultData
        //{
        //    public int upload_id { get; set; }
        //    public string status { get; set; }
        //    public int total_data { get; set; }
        //    public int unprocessed_rows { get; set; }
        //    public int success_rows { get; set; }
        //    public EditProductGetStatusResultSuccess_Rows_Data[] success_rows_data { get; set; }
        //    public EditProductGetStatusResultFailed_Rows_Data[] failed_rows_data { get; set; }
        //    public int failed_rows { get; set; }
        //    public int processed { get; set; }
        //}

        //public class EditProductGetStatusResultSuccess_Rows_Data
        //{
        //    public int product_id { get; set; }
        //}

        //public class EditProductGetStatusResultFailed_Rows_Data
        //{
        //    public string product_name { get; set; }
        //    public int product_price { get; set; }
        //    public string sku { get; set; }
        //    public string error { get; set; }
        //}

        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Edit Product {obj} ke Tokopedia Gagal.")]
        public async Task<string> EditProduct(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, string brg, string product_id)
        {
            string ret = "";

            var token = SetupContext(iden);
            iden.token = token;
            var brg_stf02 = ErasoftDbContext.STF02.Where(p => p.BRG == brg).SingleOrDefault();
            if (brg_stf02 != null)
            {
                var brg_stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == brg && p.IDMARKET == iden.idmarket).SingleOrDefault();
                //string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/edit?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);
                string urll = "https://fs.tokopedia.net/v2/products/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/edit?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);
                if((brg_stf02h.BRG_MP ?? "").Contains("PENDING") || (brg_stf02h.BRG_MP ?? "").Contains("WAITING_FOR_HANGFIRE"))
                {
                    throw new Exception("Produk masih dalam proses create/upload ke Tokopedia, belum bisa di edit.");
                }
                long milis = CurrentTimeMillis();
                DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

                //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                //{
                //    REQUEST_ID = milis.ToString(),
                //    REQUEST_ACTION = "Edit Product",
                //    REQUEST_DATETIME = milisBack,
                //    REQUEST_ATTRIBUTE_1 = "fs : " + iden.merchant_code,
                //    REQUEST_ATTRIBUTE_2 = "brg : " + brg,
                //    REQUEST_STATUS = "Pending",
                //};
                EditProductTokpedData newData = new EditProductTokpedData()
                {
                    products = new List<EditProduct_Product>()
                };
                EditProduct_Product newDataProduct = new EditProduct_Product()
                {
                    id = Convert.ToInt64(product_id),
                    name = Convert.ToString(brg_stf02.NAMA + " " + brg_stf02.NAMA2).Trim(),
                    category_id = Convert.ToInt32(brg_stf02h.CATEGORY_CODE),
                    //category_id = null,
                    price = Convert.ToInt32(brg_stf02h.HJUAL),
                    status = "LIMITED",
                    min_order = 1,
                    weight = Convert.ToInt32(brg_stf02.BERAT),
                    weight_unit = "GR",
                    condition = "NEW",
                    description = brg_stf02.Deskripsi,
                    is_must_insurance = false,
                    is_free_return = false,
                    sku = brg_stf02.BRG,
                    stock = 1, //1 - 10000.Stock should be 1 if want to add variant product. 0 indicates always availabl
                    wholesale = null,
                    preorder = null,
                    videos = null,
                    pictures = new List<CreateProduct_Images>(),
                    price_currency = "IDR"
                };
                newDataProduct.dimension = new CreateProduct_Dimension
                {
                    height = brg_stf02.TINGGI,
                    width = brg_stf02.LEBAR,
                    length = brg_stf02.PANJANG
                };
                if (!string.IsNullOrEmpty(brg_stf02h.NAMA_BARANG_MP))
                {
                    newDataProduct.name = brg_stf02h.NAMA_BARANG_MP.Trim().Replace("�", "`");
                }
                if (!string.IsNullOrEmpty(brg_stf02h.DESKRIPSI_MP))
                {
                    if (brg_stf02h.DESKRIPSI_MP != "null")
                        newDataProduct.description = brg_stf02h.DESKRIPSI_MP.Replace("�", "`");
                }
                if (!brg_stf02h.DISPLAY)
                {
                    newDataProduct.status = "EMPTY";
                }
                if (!string.IsNullOrEmpty(brg_stf02h.AVALUE_35))
                {
                    if (brg_stf02h.AVALUE_35 == "1")
                    {
                        newDataProduct.is_must_insurance = true;
                    }
                }
                if(brg_stf02h.MIN_ORDER != null)
                {
                    if(brg_stf02h.MIN_ORDER.Value > 1)
                    {
                        newDataProduct.min_order = brg_stf02h.MIN_ORDER.Value;
                    }
                }
                //add by nurul 6/2/2020
                //newDataProduct.description = newDataProduct.description.Replace("<p>", "").Replace("</p>", "").
                //        Replace("</ul>\r\n\r\n", "").Replace("<ul>", "").Replace("&nbsp;\r\n\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("&nbsp;", " ").
                //        Replace("\r\n", "").Replace("<br />", "\n");
                //newDataProduct.description = System.Net.WebUtility.HtmlDecode(newDataProduct.description);
                newDataProduct.description = WebUtility.HtmlDecode(System.Net.WebUtility.HtmlDecode(newDataProduct.description.Replace("&nbsp;", " "))).Replace("<p>", "").
                    Replace("</p>", "").Replace("</ul>", "").Replace("<ul>", "").Replace("\r\n\r\n", "\n").Replace("\r\n", "").Replace("<br />", "\n");
                //end add by nurul 6/2/2020
                newDataProduct.description = newDataProduct.description.Replace("&nbsp;", " ");
                var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == log_CUST).FirstOrDefault();

                var dataTokped = await getItemDetailVarian(iden, Convert.ToInt64(product_id), 1);
                if (dataTokped != null)
                {
                    if (dataTokped.data != null)
                    {
                        if (dataTokped.data[0].preorder != null)
                        {
                            if (dataTokped.data[0].preorder.duration > 0)
                            {
                                newDataProduct.preorder = new CreateProduct_Product_Preorder
                                {
                                    duration = Convert.ToInt32(dataTokped.data[0].preorder.duration),
                                    is_active = true,
                                    //time_unit = dataTokped.data[0].preorder.time_unit
                                };
                                string timeUnit = "DAY";
                                if (dataTokped.data[0].preorder.time_unit > 0)
                                {
                                    switch (dataTokped.data[0].preorder.time_unit)
                                    {
                                        case 1:
                                            timeUnit = "DAY";
                                            break;
                                        case 2:
                                            timeUnit = "WEEK";
                                            break;
                                        case 3:
                                            timeUnit = "MONTH";
                                            break;
                                    }
                                }
                                newDataProduct.preorder.time_unit = timeUnit;
                            }

                        }
                        if (dataTokped.data[0].wholesale != null)
                        {
                            newDataProduct.wholesale = new CreateProduct_Product_Wholesale_Price[dataTokped.data[0].wholesale.Length];
                            int i = 0;
                            foreach (var ws in dataTokped.data[0].wholesale)
                            {
                                var newWS = new CreateProduct_Product_Wholesale_Price
                                {
                                    min_qty = ws.minQuantity,
                                    price = Convert.ToInt32(ws.price.idr)
                                };
                                newDataProduct.wholesale[i] = newWS;
                                i++;
                            }
                        }
                        if (dataTokped.data[0].basic.condition == 2)
                        {
                            newDataProduct.condition = "USED";
                        }
                        if (!customer.TIDAK_HIT_UANG_R)
                        {
                            newDataProduct.stock = Convert.ToInt32(dataTokped.data[0].stock.value);
                        }
                        if (dataTokped.data[0].GMStats != null)
                        {
                            if (dataTokped.data[0].GMStats.countSold > 0)
                            {
                                newDataProduct.name = dataTokped.data[0].basic.name;
                            }
                        }
                    }
                }
                if (customer.TIDAK_HIT_UANG_R)
                {
                    var qty_stock = new StokControllerJob(iden.DatabasePathErasoft, username).GetQOHSTF08A(brg, "ALL");
                    if (qty_stock >= 0)
                    {
                        newDataProduct.stock = Convert.ToInt32(qty_stock);
                    }
                }
                if (newDataProduct.stock == 0)
                {
                    newDataProduct.stock = 1;
                    newDataProduct.status = "EMPTY";
                }
                //else
                //{
                //    var dataTokped = await getItemDetailVarian(iden, Convert.ToInt32(product_id));
                //    if (dataTokped != null)
                //    {
                //        if (dataTokped.data != null)
                //        {
                //            newDataProduct.stock = Convert.ToInt32(dataTokped.data[0].stock.value);
                //        }
                //    }
                //}
                //add by calvin 1 mei 2019
                //var qty_stock = new StokControllerJob(iden.DatabasePathErasoft, username).GetQOHSTF08A(brg, "ALL");
                //if (qty_stock > 0)
                //{
                //    newDataProduct.stock = Convert.ToInt32(qty_stock);
                //}
                ////end add by calvin 1 mei 2019

                int etalase_id = Convert.ToInt32(brg_stf02h.PICKUP_POINT);

                newDataProduct.etalase = new CreateProduct_Etalase()
                {
                    id = etalase_id,
                    name = ""
                };

                //add 15/10/2019, selalu set gambar induk
                if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_1))
                {
                    newDataProduct.pictures.Add(new CreateProduct_Images()
                    {
                        //image_description = "",
                        //image_file_name = "",
                        //image_file_path = brg_stf02.LINK_GAMBAR_1
                        file_path = brg_stf02.LINK_GAMBAR_1
                    });
                }

                if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_2))
                {
                    newDataProduct.pictures.Add(new CreateProduct_Images()
                    {
                        //image_description = "",
                        //image_file_name = "",
                        //image_file_path = brg_stf02.LINK_GAMBAR_2
                        file_path = brg_stf02.LINK_GAMBAR_2
                    });
                }

                if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_3))
                {
                    newDataProduct.pictures.Add(new CreateProduct_Images()
                    {
                        //image_description = "",
                        //image_file_name = "",
                        //image_file_path = brg_stf02.LINK_GAMBAR_3
                        file_path = brg_stf02.LINK_GAMBAR_3
                    });
                }
                //add 15/10/2019, 5 gambar
                if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_4))
                {
                    newDataProduct.pictures.Add(new CreateProduct_Images()
                    {
                        //image_description = "",
                        //image_file_name = "",
                        //image_file_path = brg_stf02.LINK_GAMBAR_4
                        file_path = brg_stf02.LINK_GAMBAR_4
                    });
                }

                if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_5))
                {
                    newDataProduct.pictures.Add(new CreateProduct_Images()
                    {
                        //image_description = "",
                        //image_file_name = "",
                        //image_file_path = brg_stf02.LINK_GAMBAR_5
                        file_path = brg_stf02.LINK_GAMBAR_5
                    });
                }
                //end add 15/10/2019, 5 gambar
                //end add 15/10/2019, selalu set gambar induk

                if (brg_stf02.TYPE == "4") // punya variasi
                {
                    CreateProduct_Product_Variant product_variant = new CreateProduct_Product_Variant()
                    {
                        products = new List<CreateProduct_Product_Variant1>(),
                        selection = new List<CreateProduct_Variant>(),
                        sizecharts = new List<CreateProduct_Images>()
                    };
                    //var AttributeOptTokped = MoDbContext.AttributeOptTokped.ToList();
                    var AttributeOptTokped = (await GetAttributeToList(iden, brg_stf02h.CATEGORY_CODE)).attribute_opt;
                    var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == brg).ToList();
                    var var_strukturVar = ErasoftDbContext.STF02I.Where(p => p.BRG == brg && p.MARKET == "TOKPED").ToList().OrderBy(p => p.RECNUM);
                    var var_stf02_brg_list = var_stf02.Select(p => p.BRG).ToList();
                    var var_stf02h = ErasoftDbContext.STF02H.Where(p => var_stf02_brg_list.Contains(p.BRG) && p.IDMARKET == iden.idmarket).ToList();

                    string category_mo = var_strukturVar.Select(p => p.CATEGORY_MO).FirstOrDefault();
                    var var_stf20 = ErasoftDbContext.STF20B.Where(p => p.CATEGORY_MO == category_mo).ToList();

                    #region variant lv 1
                    if (var_strukturVar.Where(p => p.LEVEL_VAR == 1).Count() > 0)
                    {
                        //int variant_id = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_JUDUL_VAR);
                        //int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                        //int unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                        var variant_idText = var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_JUDUL_VAR;
                        int variant_id = 0;
                        int unit_id = 0;
                        var sVariant_idText = variant_idText.Split(',');
                        variant_id = Convert.ToInt32(sVariant_idText[0]);
                        if (sVariant_idText.Length == 2)
                        {
                            unit_id = Convert.ToInt32(sVariant_idText[1]);
                        }
                        else
                        {
                            int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                            unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                        }

                        CreateProduct_Variant newVariasi = new CreateProduct_Variant()
                        {
                            id = variant_id,
                            unit_id = unit_id,
                            //pos = 1,
                            options = new List<CreateProduct_Opt>()
                        };

                        foreach (var fe_record in var_strukturVar.Where(p => p.LEVEL_VAR == 1))
                        {
                            #region cek duplikat variant_id, unit_id, value_id
                            bool add = true;
                            if (product_variant.selection.Count > 0)
                            {
                                foreach (var variant in product_variant.selection.Where(p => p.id == variant_id && p.unit_id == unit_id))
                                {
                                    var added_value_id = variant.options.Select(p => p.unit_value_id).ToList();
                                    if (add)
                                    {
                                        if (added_value_id.Contains(Convert.ToInt32(fe_record.MP_VALUE_VAR))) //value_id sudah ada 
                                        {
                                            add = false;
                                        }
                                    }
                                }
                            }
                            #endregion
                            if (add)
                            {
                                //CreateProduct_Image gambarVariant = new CreateProduct_Image()
                                //{
                                //    file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                //    file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                //    x = 128,
                                //    y = 128
                                //};
                                CreateProduct_Opt newOpt = new CreateProduct_Opt()
                                {
                                    //change 30 nov 2020, set 0 untuk custom text
                                    //unit_value_id = Convert.ToInt32(fe_record.MP_VALUE_VAR),
                                    unit_value_id = 0,
                                    //end change 30 nov 2020, set 0 untuk custom text
                                    //t_id = fe_record.RECNUM,
                                    value = var_stf20.Where(p => p.LEVEL_VAR == fe_record.LEVEL_VAR && p.KODE_VAR == fe_record.KODE_VAR).FirstOrDefault()?.KET_VAR,
                                    //image = new List<CreateProduct_Image>()
                                };
                                //newOpt.image.Add(gambarVariant);

                                newVariasi.options.Add(newOpt);

                                if (newDataProduct.pictures.Count() == 0)
                                {
                                    newDataProduct.pictures.Add(new CreateProduct_Images()
                                    {
                                        //image_file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                        //image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                        //image_description = ""
                                        file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1
                                    });
                                }
                            }
                        }
                        product_variant.selection.Add(newVariasi);
                    }
                    #endregion

                    #region variant lv 2
                    if (var_strukturVar.Where(p => p.LEVEL_VAR == 2).Count() > 0)
                    {
                        //int variant_id = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_JUDUL_VAR);
                        //int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                        //int unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                        var variant_idText = var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_JUDUL_VAR;
                        int variant_id = 0;
                        int unit_id = 0;
                        var sVariant_idText = variant_idText.Split(',');
                        variant_id = Convert.ToInt32(sVariant_idText[0]);
                        if (sVariant_idText.Length == 2)
                        {
                            unit_id = Convert.ToInt32(sVariant_idText[1]);
                        }
                        else
                        {
                            int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                            unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                        }

                        CreateProduct_Variant newVariasi = new CreateProduct_Variant()
                        {
                            id = variant_id,
                            unit_id = unit_id,
                            //pos = 2,
                            options = new List<CreateProduct_Opt>()
                        };

                        foreach (var fe_record in var_strukturVar.Where(p => p.LEVEL_VAR == 2))
                        {
                            #region cek duplikat variant_id, unit_id, value_id
                            bool add = true;
                            if (product_variant.selection.Count > 0)
                            {
                                foreach (var variant in product_variant.selection.Where(p => p.id == variant_id && p.unit_id == unit_id))
                                {
                                    var added_value_id = variant.options.Select(p => p.unit_value_id).ToList();
                                    if (add)
                                    {
                                        if (added_value_id.Contains(Convert.ToInt32(fe_record.MP_VALUE_VAR))) //value_id sudah ada 
                                        {
                                            add = false;
                                        }
                                    }
                                }
                            }
                            #endregion
                            if (add)
                            {
                                //CreateProduct_Image gambarVariant = new CreateProduct_Image()
                                //{
                                //    file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                //    file_path = var_stf02.Where(p => p.Sort9 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                //    x = 128,
                                //    y = 128
                                //};
                                CreateProduct_Opt newOpt = new CreateProduct_Opt()
                                {
                                    //change 30 nov 2020, set 0 untuk custom text
                                    //unit_value_id = Convert.ToInt32(fe_record.MP_VALUE_VAR),
                                    unit_value_id = 0,
                                    //end change 30 nov 2020, set 0 untuk custom text
                                    //t_id = fe_record.RECNUM,
                                    value = var_stf20.Where(p => p.LEVEL_VAR == fe_record.LEVEL_VAR && p.KODE_VAR == fe_record.KODE_VAR).FirstOrDefault()?.KET_VAR,
                                    //image = new List<CreateProduct_Image>()
                                };
                                //newOpt.image.Add(gambarVariant);

                                newVariasi.options.Add(newOpt);

                                if (newDataProduct.pictures.Count() == 0)
                                {
                                    newDataProduct.pictures.Add(new CreateProduct_Images()
                                    {
                                        //image_file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                        //image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                        //image_description = ""
                                        file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1
                                    });
                                }
                            }
                        }
                        product_variant.selection.Add(newVariasi);
                    }
                    #endregion

                    //product variasi
                    foreach (var item_var in var_stf02)
                    {
                        var price_var = var_stf02h.Where(p => p.BRG == item_var.BRG).FirstOrDefault();
                        CreateProduct_Product_Variant1 newProductVariasi = new CreateProduct_Product_Variant1()
                        {
                            //st = 1,
                            stock = 1,
                            //change by nurul 13/2/2020
                            //price_var = (float)item_var.HJUAL,
                            price = Convert.ToInt32(price_var.HJUAL),
                            //end change by nurul 13/2/2020
                            sku = item_var.BRG,
                            combination = new List<int>(),
                            pictures = new List<CreateProduct_Images>(),
                            status = "LIMITED"
                        };
                        if (!brg_stf02h.DISPLAY)
                        {
                            newProductVariasi.status = "EMPTY";
                        }
                        if (!string.IsNullOrEmpty(price_var.BRG_MP))
                        {
                            var dataTokpedVarian = await getItemDetailVarian(iden, Convert.ToInt64(price_var.BRG_MP), 1);
                            if (customer.TIDAK_HIT_UANG_R)
                            {
                                var qty_stock_var = new StokControllerJob(iden.DatabasePathErasoft, username).GetQOHSTF08A(item_var.BRG, "ALL");
                                if (qty_stock_var > 0)
                                {
                                    newProductVariasi.stock = Convert.ToInt32(qty_stock_var);
                                }
                            }
                            else
                            {
                                if (dataTokpedVarian != null)
                                {
                                    //newProductVariasi.stock = Convert.ToInt32(dataTokped.data[0].stock.value);
                                    if (dataTokpedVarian.data[0].stock.value > 0)
                                    {
                                        newProductVariasi.stock = Convert.ToInt32(dataTokpedVarian.data[0].stock.value);
                                    }
                                }
                            }
                            if (dataTokpedVarian != null)
                            {
                                if (dataTokpedVarian.data[0].other != null)
                                {
                                    if (!string.IsNullOrEmpty(dataTokpedVarian.data[0].other.sku))
                                    {
                                        newProductVariasi.sku = dataTokpedVarian.data[0].other.sku;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (customer.TIDAK_HIT_UANG_R)
                            {
                                var qty_stock_var = new StokControllerJob(iden.DatabasePathErasoft, username).GetQOHSTF08A(item_var.BRG, "ALL");
                                if (qty_stock_var > 0)
                                {
                                    newProductVariasi.stock = Convert.ToInt32(qty_stock_var);
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(item_var.Sort8))
                        {
                            var recnumVariasi = var_strukturVar.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item_var.Sort8).FirstOrDefault();
                            if (recnumVariasi != null)
                            {
                                //newProductVariasi.combination.Add(Convert.ToInt32(recnumVariasi.RECNUM));
                                foreach (var item in product_variant.selection)
                                {
                                    int combi = 0;
                                    foreach (var opts in item.options)
                                    {
                                        //if (opts.t_id == Convert.ToInt32(recnumVariasi.RECNUM))
                                        //if (listRecnumLv1.Contains(recnumVariasi.RECNUM))
                                        if (opts.value == item_var.Ket_Sort8)
                                        {
                                            //doAddOpt = true;
                                            newProductVariasi.combination.Add(combi);
                                        }
                                        combi++;
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(item_var.Sort9))
                        {
                            var recnumVariasi = var_strukturVar.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == item_var.Sort9).FirstOrDefault();
                            if (recnumVariasi != null)
                            {
                                //newProductVariasi.combination.Add(Convert.ToInt32(recnumVariasi.RECNUM));
                                foreach (var item in product_variant.selection)
                                {
                                    int combi2 = 0;
                                    foreach (var opts in item.options)
                                    {
                                        //if (opts.t_id == Convert.ToInt32(recnumVariasi.RECNUM))
                                        //if (listRecnumLv1.Contains(recnumVariasi.RECNUM))
                                        //{
                                        //    doAddOpt = true;
                                        //}
                                        //else if (listRecnumLv2.Contains(recnumVariasi.RECNUM))
                                        //{
                                        //    doAddOpt = true;
                                        //}
                                        if (opts.value == item_var.Ket_Sort9)
                                        {
                                            //doAddOpt = true;
                                            newProductVariasi.combination.Add(combi2);
                                        }
                                        combi2++;
                                    }
                                }
                            }
                        }
                        var imageVar = new CreateProduct_Images();
                        imageVar.file_path = item_var.LINK_GAMBAR_1;
                        newProductVariasi.pictures.Add(imageVar);
                        product_variant.products.Add(newProductVariasi);
                    }
                    //if (newDataProduct.pictures.Count > 0)
                    //    product_variant.sizecharts.Add(newDataProduct.pictures[0]);
                    newDataProduct.variant = product_variant;
                }
                //else if (brg_stf02.TYPE == "3")
                //{
                //    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_1))
                //    {
                //        newDataProduct.images.Add(new CreateProduct_Images()
                //        {
                //            image_description = "",
                //            image_file_name = "",
                //            image_file_path = brg_stf02.LINK_GAMBAR_1
                //        });
                //    }

                //    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_2))
                //    {
                //        newDataProduct.images.Add(new CreateProduct_Images()
                //        {
                //            image_description = "",
                //            image_file_name = "",
                //            image_file_path = brg_stf02.LINK_GAMBAR_2
                //        });
                //    }

                //    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_3))
                //    {
                //        newDataProduct.images.Add(new CreateProduct_Images()
                //        {
                //            image_description = "",
                //            image_file_name = "",
                //            image_file_path = brg_stf02.LINK_GAMBAR_3
                //        });
                //    }
                //    //add 15/10/2019, 5 gambar
                //    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_4))
                //    {
                //        newDataProduct.images.Add(new CreateProduct_Images()
                //        {
                //            image_description = "",
                //            image_file_name = "",
                //            image_file_path = brg_stf02.LINK_GAMBAR_4
                //        });
                //    }

                //    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_5))
                //    {
                //        newDataProduct.images.Add(new CreateProduct_Images()
                //        {
                //            image_description = "",
                //            image_file_name = "",
                //            image_file_path = brg_stf02.LINK_GAMBAR_5
                //        });
                //    }
                //    //end add 15/10/2019, 5 gambar
                //}
                newData.products.Add(newDataProduct);

                string myData = JsonConvert.SerializeObject(newData);
                string responseFromServer = "";

                //try
                //{

                MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                {
                    REQUEST_ID = milis.ToString(),
                    REQUEST_ACTION = "Create Product",
                    REQUEST_DATETIME = milisBack,
                    REQUEST_ATTRIBUTE_1 = brg,
                    REQUEST_ATTRIBUTE_2 = "fs : " + iden.merchant_code,
                    REQUEST_STATUS = "Pending",
                };
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

                var client = new HttpClient();

                //var request = new HttpRequestMessage(new HttpMethod("PATCH"), urll);
                //request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", iden.token);
                //request.Content = new StringContent(myData, System.Text.Encoding.UTF8, "application/json");
                //HttpResponseMessage response;
                //response = await client.SendAsync(request);
                //responseFromServer = await response.Content.ReadAsStringAsync();
                try
                {

                    //    client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + iden.token));
                    //var content = new StringContent(myData, Encoding.UTF8, "application/json");
                    //content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
                    //HttpResponseMessage clientResponse = await client.PostAsync(
                    //    urll, content);

                    //using (HttpContent responseContent = clientResponse.Content)
                    //{
                    //    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                    //    {
                    //        responseFromServer = await reader.ReadToEndAsync();
                    //    }
                    //};
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), urll);
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", iden.token);
                    request.Content = new StringContent(myData, System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response;
                    response = await client.SendAsync(request);
                    responseFromServer = await response.Content.ReadAsStringAsync();
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
                        throw new Exception(err);
                    }
                    else
                    {
                        throw e;
                    }
                }

                if (responseFromServer != "")
                {
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokpedCreateProductResult)) as TokpedCreateProductResult;
                    //if (result.header.error_code == "")
                    if (string.IsNullOrEmpty(result.header.reason))
                    {
                        //change by calvin 9 juni 2019
                        //await EditProductGetStatus(iden, brg, result.data.upload_id, currentLog.REQUEST_ID, product_id);
                        EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = 'PEDITENDING;" + Convert.ToString(result.data.upload_id) 
                            + ";" + Convert.ToString(currentLog.REQUEST_ID) + ";" + Convert.ToString(product_id) + "' WHERE BRG = '" + Convert.ToString(brg) 
                            + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
#if (DEBUG || Debug_AWS)
                        await EditProductGetStatus(iden.DatabasePathErasoft, brg, log_CUST, "Barang", "Edit Produk Get Status", iden, brg, result.data.upload_id, currentLog.REQUEST_ID, product_id);
#else
                        string EDBConnID = EDB.GetConnectionString("ConnId");
                        var sqlStorage = new SqlServerStorage(EDBConnID);

                        var Jobclient = new BackgroundJobClient(sqlStorage);
                        //Jobclient.Enqueue<TokopediaControllerJob>(x => x.EditProductGetStatus(iden.DatabasePathErasoft, brg, log_CUST, "Barang", "Edit Produk Get Status", iden, brg, result.data.upload_id, currentLog.REQUEST_ID, product_id));
                        Jobclient.Schedule<TokopediaControllerJob>(x => x.EditProductGetStatus(iden.DatabasePathErasoft, brg, log_CUST, "Barang", "Edit Produk Get Status", iden, brg, result.data.upload_id, currentLog.REQUEST_ID, product_id), TimeSpan.FromMinutes(1));

#endif
                        //end change by calvin 9 juni 2019
                    }
                    else
                    {
                        currentLog.REQUEST_RESULT = result.header.reason;
                        currentLog.REQUEST_EXCEPTION = result.header.messages;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        throw new Exception(result.header.messages + ";" + result.header.reason);
                    }
                }
                //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                //myReq.Method = "POST";
                //myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
                //myReq.Accept = "application/x-www-form-urlencoded";
                //myReq.ContentType = "application/json";
                //try
                //{
                //    myReq.ContentLength = myData.Length;
                //    using (var dataStream = myReq.GetRequestStream())
                //    {
                //        dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
                //    }
                //    using (WebResponse response = await myReq.GetResponseAsync())
                //    {
                //        using (Stream stream = response.GetResponseStream())
                //        {
                //            StreamReader reader = new StreamReader(stream);
                //            responseFromServer = reader.ReadToEnd();
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}

            }

            return ret;
        }


        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Edit Product {obj} ke Tokopedia Gagal.")]
        public async Task<string> EditProduct_V3(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, string brg, string product_id)
        {
            string ret = "";

            var token = SetupContext(iden);
            iden.token = token;
            var brg_stf02 = ErasoftDbContext.STF02.Where(p => p.BRG == brg).SingleOrDefault();
            if (brg_stf02 != null)
            {
                var brg_stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == brg && p.IDMARKET == iden.idmarket).SingleOrDefault();
                //string urll = "https://fs.tokopedia.net/v2/products/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/edit?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);
                string urll = "https://fs.tokopedia.net/v2/products/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/edit?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);
                if ((brg_stf02h.BRG_MP ?? "").Contains("PENDING") || (brg_stf02h.BRG_MP ?? "").Contains("WAITING_FOR_HANGFIRE"))
                {
                    throw new Exception("Produk masih dalam proses create/upload ke Tokopedia, belum bisa di edit.");
                }
                long milis = CurrentTimeMillis();
                DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

                EditProductTokpedDataV3 newData = new EditProductTokpedDataV3()
                {
                    products = new List<EditProduct_ProductV3>()
                };
                EditProduct_ProductV3 newDataProduct = new EditProduct_ProductV3()
                {
                    id = Convert.ToInt64(product_id),
                    name = Convert.ToString(brg_stf02.NAMA + " " + brg_stf02.NAMA2).Trim(),
                    category_id = Convert.ToInt32(brg_stf02h.CATEGORY_CODE),
                    //category_id = null,
                    price = Convert.ToInt32(brg_stf02h.HJUAL),
                    status = "LIMITED",
                    min_order = 1,
                    weight = Convert.ToInt32(brg_stf02.BERAT),
                    weight_unit = "GR",
                    condition = "NEW",
                    description = brg_stf02.Deskripsi,
                    is_must_insurance = false,
                    is_free_return = false,
                    sku = brg_stf02.BRG,
                    stock = 1, //1 - 10000.Stock should be 1 if want to add variant product. 0 indicates always availabl
                    wholesale = null,
                    preorder = null,
                    videos = null,
                    pictures = new List<CreateProduct_Images>(),
                    price_currency = "IDR"
                };
                newDataProduct.dimension = new CreateProduct_Dimension
                {
                    height = brg_stf02.TINGGI,
                    width = brg_stf02.LEBAR,
                    length = brg_stf02.PANJANG
                };
                if (!string.IsNullOrEmpty(brg_stf02h.NAMA_BARANG_MP))
                {
                    newDataProduct.name = brg_stf02h.NAMA_BARANG_MP.Trim().Replace("�", "`");
                }
                if (!string.IsNullOrEmpty(brg_stf02h.DESKRIPSI_MP))
                {
                    if (brg_stf02h.DESKRIPSI_MP != "null")
                        newDataProduct.description = brg_stf02h.DESKRIPSI_MP.Replace("�", "`");
                }
                if (!brg_stf02h.DISPLAY)
                {
                    newDataProduct.status = "EMPTY";
                }
                if (!string.IsNullOrEmpty(brg_stf02h.AVALUE_35))
                {
                    if (brg_stf02h.AVALUE_35 == "1")
                    {
                        newDataProduct.is_must_insurance = true;
                    }
                }
                if (brg_stf02h.MIN_ORDER != null)
                {
                    if (brg_stf02h.MIN_ORDER.Value > 1)
                    {
                        newDataProduct.min_order = brg_stf02h.MIN_ORDER.Value;
                    }
                }
                if (!string.IsNullOrEmpty(brg_stf02h.AVALUE_39))
                {
                    var listCourir = brg_stf02h.AVALUE_39.Split(',');
                    newDataProduct.custom_product_logistics = new List<int>();
                    foreach (var courier in listCourir)
                    {
                        newDataProduct.custom_product_logistics.Add(Convert.ToInt32(courier));
                    }
                }
                //add by nurul 6/2/2020
                //newDataProduct.description = newDataProduct.description.Replace("<p>", "").Replace("</p>", "").
                //        Replace("</ul>\r\n\r\n", "").Replace("<ul>", "").Replace("&nbsp;\r\n\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("&nbsp;", " ").
                //        Replace("\r\n", "").Replace("<br />", "\n");
                //newDataProduct.description = System.Net.WebUtility.HtmlDecode(newDataProduct.description);
                newDataProduct.description = WebUtility.HtmlDecode(System.Net.WebUtility.HtmlDecode(newDataProduct.description.Replace("&nbsp;", " "))).Replace("<p>", "").
                    Replace("</p>", "").Replace("</ul>", "").Replace("<ul>", "").Replace("\r\n\r\n", "\n").Replace("\r\n", "").Replace("<br />", "\n");
                //end add by nurul 6/2/2020
                newDataProduct.description = newDataProduct.description.Replace("&nbsp;", " ");
                var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == log_CUST).FirstOrDefault();

                var dataTokped = await getItemDetailVarian(iden, Convert.ToInt64(product_id), 1);
                if (dataTokped != null)
                {
                    if (dataTokped.data != null)
                    {
                        if (dataTokped.data[0].preorder != null)
                        {
                            if (dataTokped.data[0].preorder.duration > 0)
                            {
                                newDataProduct.preorder = new CreateProduct_Product_Preorder
                                {
                                    duration = Convert.ToInt32(dataTokped.data[0].preorder.duration),
                                    is_active = true,
                                    //time_unit = dataTokped.data[0].preorder.time_unit
                                };
                                string timeUnit = "DAY";
                                if (dataTokped.data[0].preorder.time_unit > 0)
                                {
                                    switch (dataTokped.data[0].preorder.time_unit)
                                    {
                                        case 1:
                                            timeUnit = "DAY";
                                            break;
                                        case 2:
                                            timeUnit = "WEEK";
                                            break;
                                        case 3:
                                            timeUnit = "MONTH";
                                            break;
                                    }
                                }
                                newDataProduct.preorder.time_unit = timeUnit;
                            }

                        }
                        if (dataTokped.data[0].wholesale != null)
                        {
                            newDataProduct.wholesale = new CreateProduct_Product_Wholesale_Price[dataTokped.data[0].wholesale.Length];
                            int i = 0;
                            foreach (var ws in dataTokped.data[0].wholesale)
                            {
                                var newWS = new CreateProduct_Product_Wholesale_Price
                                {
                                    min_qty = ws.minQuantity,
                                    price = Convert.ToInt32(ws.price.idr)
                                };
                                newDataProduct.wholesale[i] = newWS;
                                i++;
                            }
                        }
                        if (dataTokped.data[0].basic.condition == 2)
                        {
                            newDataProduct.condition = "USED";
                        }
                        if (!customer.TIDAK_HIT_UANG_R)
                        {
                            newDataProduct.stock = Convert.ToInt32(dataTokped.data[0].stock.value);
                        }
                        if (dataTokped.data[0].GMStats != null)
                        {
                            if (dataTokped.data[0].GMStats.countSold > 0)
                            {
                                newDataProduct.name = dataTokped.data[0].basic.name;
                            }
                        }
                    }
                }
                if (customer.TIDAK_HIT_UANG_R)
                {
                    var qty_stock = new StokControllerJob(iden.DatabasePathErasoft, username).GetQOHSTF08A(brg, "ALL");
                    if (qty_stock >= 0)
                    {
                        newDataProduct.stock = Convert.ToInt32(qty_stock);
                    }
                }
                if (newDataProduct.stock == 0)
                {
                    newDataProduct.stock = 1;
                    newDataProduct.status = "EMPTY";
                }
                int etalase_id = Convert.ToInt32(brg_stf02h.PICKUP_POINT);

                newDataProduct.etalase = new CreateProduct_Etalase()
                {
                    id = etalase_id,
                    name = ""
                };

                //add 15/10/2019, selalu set gambar induk
                if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_1))
                {
                    newDataProduct.pictures.Add(new CreateProduct_Images()
                    {
                        //image_description = "",
                        //image_file_name = "",
                        //image_file_path = brg_stf02.LINK_GAMBAR_1
                        file_path = brg_stf02.LINK_GAMBAR_1
                    });
                }

                if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_2))
                {
                    newDataProduct.pictures.Add(new CreateProduct_Images()
                    {
                        //image_description = "",
                        //image_file_name = "",
                        //image_file_path = brg_stf02.LINK_GAMBAR_2
                        file_path = brg_stf02.LINK_GAMBAR_2
                    });
                }

                if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_3))
                {
                    newDataProduct.pictures.Add(new CreateProduct_Images()
                    {
                        //image_description = "",
                        //image_file_name = "",
                        //image_file_path = brg_stf02.LINK_GAMBAR_3
                        file_path = brg_stf02.LINK_GAMBAR_3
                    });
                }
                //add 15/10/2019, 5 gambar
                if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_4))
                {
                    newDataProduct.pictures.Add(new CreateProduct_Images()
                    {
                        //image_description = "",
                        //image_file_name = "",
                        //image_file_path = brg_stf02.LINK_GAMBAR_4
                        file_path = brg_stf02.LINK_GAMBAR_4
                    });
                }

                if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_5))
                {
                    newDataProduct.pictures.Add(new CreateProduct_Images()
                    {
                        //image_description = "",
                        //image_file_name = "",
                        //image_file_path = brg_stf02.LINK_GAMBAR_5
                        file_path = brg_stf02.LINK_GAMBAR_5
                    });
                }
                //end add 15/10/2019, 5 gambar
                //end add 15/10/2019, selalu set gambar induk

                if (brg_stf02.TYPE == "4") // punya variasi
                {
                    CreateProduct_Product_Variant product_variant = new CreateProduct_Product_Variant()
                    {
                        products = new List<CreateProduct_Product_Variant1>(),
                        selection = new List<CreateProduct_Variant>(),
                        sizecharts = new List<CreateProduct_Images>()
                    };
                    //var AttributeOptTokped = MoDbContext.AttributeOptTokped.ToList();
                    var AttributeOptTokped = (await GetAttributeToList(iden, brg_stf02h.CATEGORY_CODE)).attribute_opt;
                    var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == brg).ToList();
                    var var_strukturVar = ErasoftDbContext.STF02I.Where(p => p.BRG == brg && p.MARKET == "TOKPED").ToList().OrderBy(p => p.RECNUM);
                    var var_stf02_brg_list = var_stf02.Select(p => p.BRG).ToList();
                    var var_stf02h = ErasoftDbContext.STF02H.Where(p => var_stf02_brg_list.Contains(p.BRG) && p.IDMARKET == iden.idmarket).ToList();

                    string category_mo = var_strukturVar.Select(p => p.CATEGORY_MO).FirstOrDefault();
                    var var_stf20 = ErasoftDbContext.STF20B.Where(p => p.CATEGORY_MO == category_mo).ToList();

                    #region variant lv 1
                    if (var_strukturVar.Where(p => p.LEVEL_VAR == 1).Count() > 0)
                    {
                        //int variant_id = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_JUDUL_VAR);
                        //int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                        //int unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                        var variant_idText = var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_JUDUL_VAR;
                        int variant_id = 0;
                        int unit_id = 0;
                        var sVariant_idText = variant_idText.Split(',');
                        variant_id = Convert.ToInt32(sVariant_idText[0]);
                        if (sVariant_idText.Length == 2)
                        {
                            unit_id = Convert.ToInt32(sVariant_idText[1]);
                        }
                        else
                        {
                            int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                            unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                        }

                        CreateProduct_Variant newVariasi = new CreateProduct_Variant()
                        {
                            id = variant_id,
                            unit_id = unit_id,
                            //pos = 1,
                            options = new List<CreateProduct_Opt>()
                        };

                        foreach (var fe_record in var_strukturVar.Where(p => p.LEVEL_VAR == 1))
                        {
                            #region cek duplikat variant_id, unit_id, value_id
                            bool add = true;
                            if (product_variant.selection.Count > 0)
                            {
                                foreach (var variant in product_variant.selection.Where(p => p.id == variant_id && p.unit_id == unit_id))
                                {
                                    var added_value_id = variant.options.Select(p => p.unit_value_id).ToList();
                                    if (add)
                                    {
                                        if (added_value_id.Contains(Convert.ToInt32(fe_record.MP_VALUE_VAR))) //value_id sudah ada 
                                        {
                                            add = false;
                                        }
                                    }
                                }
                            }
                            #endregion
                            if (add)
                            {
                                //CreateProduct_Image gambarVariant = new CreateProduct_Image()
                                //{
                                //    file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                //    file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                //    x = 128,
                                //    y = 128
                                //};
                                CreateProduct_Opt newOpt = new CreateProduct_Opt()
                                {
                                    //change 30 nov 2020, set 0 untuk custom text
                                    //unit_value_id = Convert.ToInt32(fe_record.MP_VALUE_VAR),
                                    unit_value_id = 0,
                                    //end change 30 nov 2020, set 0 untuk custom text
                                    //t_id = fe_record.RECNUM,
                                    value = var_stf20.Where(p => p.LEVEL_VAR == fe_record.LEVEL_VAR && p.KODE_VAR == fe_record.KODE_VAR).FirstOrDefault()?.KET_VAR,
                                    //image = new List<CreateProduct_Image>()
                                };
                                //newOpt.image.Add(gambarVariant);

                                newVariasi.options.Add(newOpt);

                                if (newDataProduct.pictures.Count() == 0)
                                {
                                    newDataProduct.pictures.Add(new CreateProduct_Images()
                                    {
                                        //image_file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                        //image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                        //image_description = ""
                                        file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1
                                    });
                                }
                            }
                        }
                        product_variant.selection.Add(newVariasi);
                    }
                    #endregion

                    #region variant lv 2
                    if (var_strukturVar.Where(p => p.LEVEL_VAR == 2).Count() > 0)
                    {
                        //int variant_id = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_JUDUL_VAR);
                        //int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                        //int unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                        var variant_idText = var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_JUDUL_VAR;
                        int variant_id = 0;
                        int unit_id = 0;
                        var sVariant_idText = variant_idText.Split(',');
                        variant_id = Convert.ToInt32(sVariant_idText[0]);
                        if (sVariant_idText.Length == 2)
                        {
                            unit_id = Convert.ToInt32(sVariant_idText[1]);
                        }
                        else
                        {
                            int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                            unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                        }

                        CreateProduct_Variant newVariasi = new CreateProduct_Variant()
                        {
                            id = variant_id,
                            unit_id = unit_id,
                            //pos = 2,
                            options = new List<CreateProduct_Opt>()
                        };

                        foreach (var fe_record in var_strukturVar.Where(p => p.LEVEL_VAR == 2))
                        {
                            #region cek duplikat variant_id, unit_id, value_id
                            bool add = true;
                            if (product_variant.selection.Count > 0)
                            {
                                foreach (var variant in product_variant.selection.Where(p => p.id == variant_id && p.unit_id == unit_id))
                                {
                                    var added_value_id = variant.options.Select(p => p.unit_value_id).ToList();
                                    if (add)
                                    {
                                        if (added_value_id.Contains(Convert.ToInt32(fe_record.MP_VALUE_VAR))) //value_id sudah ada 
                                        {
                                            add = false;
                                        }
                                    }
                                }
                            }
                            #endregion
                            if (add)
                            {
                                //CreateProduct_Image gambarVariant = new CreateProduct_Image()
                                //{
                                //    file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                //    file_path = var_stf02.Where(p => p.Sort9 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                //    x = 128,
                                //    y = 128
                                //};
                                CreateProduct_Opt newOpt = new CreateProduct_Opt()
                                {
                                    //change 30 nov 2020, set 0 untuk custom text
                                    //unit_value_id = Convert.ToInt32(fe_record.MP_VALUE_VAR),
                                    unit_value_id = 0,
                                    //end change 30 nov 2020, set 0 untuk custom text
                                    //t_id = fe_record.RECNUM,
                                    value = var_stf20.Where(p => p.LEVEL_VAR == fe_record.LEVEL_VAR && p.KODE_VAR == fe_record.KODE_VAR).FirstOrDefault()?.KET_VAR,
                                    //image = new List<CreateProduct_Image>()
                                };
                                //newOpt.image.Add(gambarVariant);

                                newVariasi.options.Add(newOpt);

                                if (newDataProduct.pictures.Count() == 0)
                                {
                                    newDataProduct.pictures.Add(new CreateProduct_Images()
                                    {
                                        //image_file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                        //image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                        //image_description = ""
                                        file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1
                                    });
                                }
                            }
                        }
                        product_variant.selection.Add(newVariasi);
                    }
                    #endregion

                    //product variasi
                    foreach (var item_var in var_stf02)
                    {
                        var price_var = var_stf02h.Where(p => p.BRG == item_var.BRG).FirstOrDefault();
                        CreateProduct_Product_Variant1 newProductVariasi = new CreateProduct_Product_Variant1()
                        {
                            //st = 1,
                            stock = 1,
                            //change by nurul 13/2/2020
                            //price_var = (float)item_var.HJUAL,
                            price = Convert.ToInt32(price_var.HJUAL),
                            //end change by nurul 13/2/2020
                            sku = item_var.BRG,
                            combination = new List<int>(),
                            pictures = new List<CreateProduct_Images>(),
                            status = "LIMITED"
                        };
                        if (!brg_stf02h.DISPLAY)
                        {
                            newProductVariasi.status = "EMPTY";
                        }
                        if (!string.IsNullOrEmpty(price_var.BRG_MP))
                        {
                            var dataTokpedVarian = await getItemDetailVarian(iden, Convert.ToInt64(price_var.BRG_MP), 1);
                            if (customer.TIDAK_HIT_UANG_R)
                            {
                                var qty_stock_var = new StokControllerJob(iden.DatabasePathErasoft, username).GetQOHSTF08A(item_var.BRG, "ALL");
                                if (qty_stock_var > 0)
                                {
                                    newProductVariasi.stock = Convert.ToInt32(qty_stock_var);
                                }
                            }
                            else
                            {
                                if (dataTokpedVarian != null)
                                {
                                    //newProductVariasi.stock = Convert.ToInt32(dataTokped.data[0].stock.value);
                                    if (dataTokpedVarian.data[0].stock.value > 0)
                                    {
                                        newProductVariasi.stock = Convert.ToInt32(dataTokpedVarian.data[0].stock.value);
                                    }
                                }
                            }
                            if (dataTokpedVarian != null)
                            {
                                if (dataTokpedVarian.data[0].other != null)
                                {
                                    if (!string.IsNullOrEmpty(dataTokpedVarian.data[0].other.sku))
                                    {
                                        newProductVariasi.sku = dataTokpedVarian.data[0].other.sku;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (customer.TIDAK_HIT_UANG_R)
                            {
                                var qty_stock_var = new StokControllerJob(iden.DatabasePathErasoft, username).GetQOHSTF08A(item_var.BRG, "ALL");
                                if (qty_stock_var > 0)
                                {
                                    newProductVariasi.stock = Convert.ToInt32(qty_stock_var);
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(item_var.Sort8))
                        {
                            var recnumVariasi = var_strukturVar.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item_var.Sort8).FirstOrDefault();
                            if (recnumVariasi != null)
                            {
                                //newProductVariasi.combination.Add(Convert.ToInt32(recnumVariasi.RECNUM));
                                foreach (var item in product_variant.selection)
                                {
                                    int combi = 0;
                                    foreach (var opts in item.options)
                                    {
                                        //if (opts.t_id == Convert.ToInt32(recnumVariasi.RECNUM))
                                        //if (listRecnumLv1.Contains(recnumVariasi.RECNUM))
                                        if (opts.value == item_var.Ket_Sort8)
                                        {
                                            //doAddOpt = true;
                                            newProductVariasi.combination.Add(combi);
                                        }
                                        combi++;
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(item_var.Sort9))
                        {
                            var recnumVariasi = var_strukturVar.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == item_var.Sort9).FirstOrDefault();
                            if (recnumVariasi != null)
                            {
                                //newProductVariasi.combination.Add(Convert.ToInt32(recnumVariasi.RECNUM));
                                foreach (var item in product_variant.selection)
                                {
                                    int combi2 = 0;
                                    foreach (var opts in item.options)
                                    {
                                        //if (opts.t_id == Convert.ToInt32(recnumVariasi.RECNUM))
                                        //if (listRecnumLv1.Contains(recnumVariasi.RECNUM))
                                        //{
                                        //    doAddOpt = true;
                                        //}
                                        //else if (listRecnumLv2.Contains(recnumVariasi.RECNUM))
                                        //{
                                        //    doAddOpt = true;
                                        //}
                                        if (opts.value == item_var.Ket_Sort9)
                                        {
                                            //doAddOpt = true;
                                            newProductVariasi.combination.Add(combi2);
                                        }
                                        combi2++;
                                    }
                                }
                            }
                        }
                        var imageVar = new CreateProduct_Images();
                        imageVar.file_path = item_var.LINK_GAMBAR_1;
                        newProductVariasi.pictures.Add(imageVar);
                        product_variant.products.Add(newProductVariasi);
                    }
                    //if (newDataProduct.pictures.Count > 0)
                    //    product_variant.sizecharts.Add(newDataProduct.pictures[0]);
                    newDataProduct.variant = product_variant;
                }
                newData.products.Add(newDataProduct);

                string myData = JsonConvert.SerializeObject(newData);
                string responseFromServer = "";

                //try
                //{

                MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                {
                    REQUEST_ID = milis.ToString(),
                    REQUEST_ACTION = "Create Product",
                    REQUEST_DATETIME = milisBack,
                    REQUEST_ATTRIBUTE_1 = brg,
                    REQUEST_ATTRIBUTE_2 = "fs : " + iden.merchant_code,
                    REQUEST_STATUS = "Pending",
                };
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

                var client = new HttpClient();

                try
                {
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), urll);
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", iden.token);
                    request.Content = new StringContent(myData, System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response;
                    response = await client.SendAsync(request);
                    responseFromServer = await response.Content.ReadAsStringAsync();
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
                        throw new Exception(err);
                    }
                    else
                    {
                        throw e;
                    }
                }

                if (responseFromServer != "")
                {
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokpedCreateProductResult)) as TokpedCreateProductResult;
                    //if (result.header.error_code == "")
                    if (string.IsNullOrEmpty(result.header.reason))
                    {
                        await getItemDetailToLink(dbPathEra, kodeProduk, log_CUST, iden, iden.idmarket, 1, result.data.success_rows_data[0].product_id);
                        if (brg_stf02.TYPE == "4")
                        {
                            await GetActiveItemVariantByProductID(dbPathEra, kodeProduk, log_CUST, "", "", iden, brg, iden.idmarket, result.data.success_rows_data[0].product_id.ToString(), "");
                        }
                    }
                    else
                    {
                        currentLog.REQUEST_RESULT = result.header.reason;
                        currentLog.REQUEST_EXCEPTION = result.header.messages;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        throw new Exception(result.header.messages + ";" + result.header.reason);
                    }
                }

            }

            return ret;
        }

        public async Task<TokopediaController.TokpedGetItemDetail> getItemDetailVarian(TokopediaAPIData iden, long product_id, int retry)
        {
            var ret = new TokopediaController.TokpedGetItemDetail();
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";

            //long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            //long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/info?product_id=" + Uri.EscapeDataString(product_id.ToString());
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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
            catch (WebException e)
            {
                string err = e.Message;
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                    if (err.Contains("Too Many Request"))
                    {
                        await Task.Delay(retry * 1000);
                        var retLoop = await getItemDetailVarian(iden, product_id, retry + 1);
                        return retLoop;
                    }
                    else
                    {
                        throw new Exception(err);
                    }
                }
                else
                {
                    throw e;
                }
            }
            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaController.TokpedGetItemDetail)) as TokopediaController.TokpedGetItemDetail;
                bool adaError = false;
                //if (result.header != null)
                //{
                //    //if (result.data.Count() == 0)
                //    //{
                //    //    adaError = true;
                //    //}
                //    if (!string.IsNullOrEmpty(result.header.error_code))
                //    {
                //        adaError = true;
                //    }

                //}
                if (result.data != null)
                    if (!adaError)
                    {
                        ret = result;
                    }
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Tokopedia Gagal.")]
        public async Task<string> CreateProduct(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, string brg)
        {
            string ret = "";
            var token = SetupContext(iden);
            iden.token = token;
            var brg_stf02 = ErasoftDbContext.STF02.Where(p => p.BRG == brg).SingleOrDefault();
            try
            {
                if (brg_stf02 != null)
                {
                    var brg_stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == brg && p.IDMARKET == iden.idmarket).SingleOrDefault();
                    //string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/create?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);
                    string urll = "https://fs.tokopedia.net/v2/products/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/create?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);

                    long milis = CurrentTimeMillis();
                    DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

                    CreateProductTokpedData newData = new CreateProductTokpedData()
                    {
                        products = new List<CreateProduct_Product>()
                    };
                    CreateProduct_Product newDataProduct = new CreateProduct_Product()
                    {
                        name = Convert.ToString(brg_stf02.NAMA + " " + brg_stf02.NAMA2).Trim(),
                        category_id = Convert.ToInt32(brg_stf02h.CATEGORY_CODE),
                        price = Convert.ToInt32(brg_stf02h.HJUAL),
                        status = "LIMITED",//UNLIMITED, LIMITED, and EMPTY
                        min_order = 1,
                        weight = Convert.ToInt32(brg_stf02.BERAT),
                        weight_unit = "GR",//GR (gram) and KG (kilogram)
                        condition = "NEW",//NEW and USED
                        description = brg_stf02.Deskripsi,
                        is_must_insurance = false,
                        is_free_return = false,
                        sku = brg_stf02.BRG,
                        stock = 1, //1 - 10000.Stock should be 1 if want to add variant product. 0 indicates always availabl
                        wholesale = null,
                        preorder = null,
                        videos = null,
                        pictures = new List<CreateProduct_Images>(),
                        price_currency = "IDR"
                    };
                    newDataProduct.dimension = new CreateProduct_Dimension
                    {
                        height = brg_stf02.TINGGI,
                        width = brg_stf02.LEBAR,
                        length = brg_stf02.PANJANG
                    };
                    if (!string.IsNullOrEmpty(brg_stf02h.NAMA_BARANG_MP))
                    {
                        newDataProduct.name = brg_stf02h.NAMA_BARANG_MP.Trim().Replace("�", "`");
                    }
                    if (!string.IsNullOrEmpty(brg_stf02h.DESKRIPSI_MP))
                    {
                        if (brg_stf02h.DESKRIPSI_MP != "null")
                            newDataProduct.description = brg_stf02h.DESKRIPSI_MP.Replace("�", "`");
                    }

                    if (!string.IsNullOrEmpty(brg_stf02h.AVALUE_35))
                    {
                        if (brg_stf02h.AVALUE_35 == "1")
                        {
                            newDataProduct.is_must_insurance = true;
                        }
                    }
                    if (brg_stf02h.MIN_ORDER != null)
                    {
                        if (brg_stf02h.MIN_ORDER.Value > 1)
                        {
                            newDataProduct.min_order = brg_stf02h.MIN_ORDER.Value;
                        }
                    }
                    //add by nurul 6/2/2020
                    //newDataProduct.description = newDataProduct.description.Replace("<p>", "").Replace("</p>", "").
                    //    Replace("</ul>\r\n\r\n", "").Replace("<ul>", "").Replace("&nbsp;\r\n\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("&nbsp;", " ").
                    //    Replace("\r\n", "").Replace("<br />", "\n");
                    //newDataProduct.description = System.Net.WebUtility.HtmlDecode(newDataProduct.description);
                    newDataProduct.description = WebUtility.HtmlDecode(System.Net.WebUtility.HtmlDecode(newDataProduct.description.Replace("&nbsp;", " "))).Replace("<p>", "").
                        Replace("</p>", "").Replace("</ul>", "").Replace("<ul>", "").Replace("\r\n\r\n", "\n").Replace("\r\n", "").Replace("<br />", "\n");
                    //end add by nurul 6/2/2020

                    newDataProduct.description = newDataProduct.description.Replace("&nbsp;", " ");
                    //add by calvin 1 mei 2019
                    var qty_stock = new StokControllerJob(iden.DatabasePathErasoft, username).GetQOHSTF08A(brg, "ALL");
                    if (qty_stock > 0)
                    {
                        newDataProduct.stock = Convert.ToInt32(qty_stock);
                    }
                    //end add by calvin 1 mei 2019

                    int etalase_id = Convert.ToInt32(brg_stf02h.PICKUP_POINT);

                    newDataProduct.etalase = new CreateProduct_Etalase()
                    {
                        id = etalase_id,
                        name = ""
                    };

                    //add 15/10/2019, selalu isi gambar induk
                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_1))
                    {
                        newDataProduct.pictures.Add(new CreateProduct_Images()
                        {
                            //image_description = "",
                            //image_file_name = "",
                            file_path = brg_stf02.LINK_GAMBAR_1
                        });
                    }

                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_2))
                    {
                        newDataProduct.pictures.Add(new CreateProduct_Images()
                        {
                            //image_description = "",
                            //image_file_name = "",
                            file_path = brg_stf02.LINK_GAMBAR_2
                        });
                    }

                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_3))
                    {
                        newDataProduct.pictures.Add(new CreateProduct_Images()
                        {
                            //image_description = "",
                            //image_file_name = "",
                            file_path = brg_stf02.LINK_GAMBAR_3
                        });
                    }
                    //add 6/9/2019, 5 gambar
                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_4))
                    {
                        newDataProduct.pictures.Add(new CreateProduct_Images()
                        {
                            //image_description = "",
                            //image_file_name = "",
                            file_path = brg_stf02.LINK_GAMBAR_4
                        });
                    }

                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_5))
                    {
                        newDataProduct.pictures.Add(new CreateProduct_Images()
                        {
                            //image_description = "",
                            //image_file_name = "",
                            file_path = brg_stf02.LINK_GAMBAR_5
                        });
                    }
                    //end add 6/9/2019, 5 gambar
                    //end add 15/20/2019, selalu isi gambar induk

                    if (brg_stf02.TYPE == "4") // punya variasi
                    {
                        CreateProduct_Product_Variant product_variant = new CreateProduct_Product_Variant()
                        {
                            products = new List<CreateProduct_Product_Variant1>(),
                            selection = new List<CreateProduct_Variant>(),
                            sizecharts = new List<CreateProduct_Images>()
                        };
                        //var AttributeOptTokped = MoDbContext.AttributeOptTokped.ToList();
                        var AttributeOptTokped = (await GetAttributeToList(iden, brg_stf02h.CATEGORY_CODE)).attribute_opt;
                        var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == brg).ToList();
                        var var_strukturVar = ErasoftDbContext.STF02I.Where(p => p.BRG == brg && p.MARKET == "TOKPED").ToList().OrderBy(p => p.RECNUM);
                        var var_stf02_brg_list = var_stf02.Select(p => p.BRG).ToList();
                        var var_stf02h = ErasoftDbContext.STF02H.Where(p => var_stf02_brg_list.Contains(p.BRG) && p.IDMARKET == iden.idmarket).ToList();

                        string category_mo = var_strukturVar.Select(p => p.CATEGORY_MO).FirstOrDefault();
                        var var_stf20 = ErasoftDbContext.STF20B.Where(p => p.CATEGORY_MO == category_mo).ToList();

                        var listRecnumLv1 = new List<int>();
                        var listRecnumLv2 = new List<int>();
                        #region variant lv 1
                        if (var_strukturVar.Where(p => p.LEVEL_VAR == 1).Count() > 0)
                        {
                            //int variant_id = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_JUDUL_VAR);
                            //int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                            //int unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                            var variant_idText = var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_JUDUL_VAR;
                            int variant_id = 0;
                            int unit_id = 0;
                            var sVariant_idText = variant_idText.Split(',');
                            variant_id = Convert.ToInt32(sVariant_idText[0]);
                            if (sVariant_idText.Length == 2)
                            {
                                unit_id = Convert.ToInt32(sVariant_idText[1]);
                            }
                            else
                            {
                                int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                                unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                            }

                            CreateProduct_Variant newVariasi = new CreateProduct_Variant()
                            {
                                id = variant_id,
                                unit_id = unit_id,
                                //pos = 1,
                                options = new List<CreateProduct_Opt>()
                            };

                            foreach (var fe_record in var_strukturVar.Where(p => p.LEVEL_VAR == 1))
                            {
                                #region cek duplikat variant_id, unit_id, value_id
                                bool add = true;
                                if (product_variant.selection.Count > 0)
                                {
                                    foreach (var variant in product_variant.selection.Where(p => p.id == variant_id && p.unit_id == unit_id))
                                    {
                                        var added_value_id = variant.options.Select(p => p.unit_value_id).ToList();
                                        if (add)
                                        {
                                            if (added_value_id.Contains(Convert.ToInt32(fe_record.MP_VALUE_VAR))) //value_id sudah ada 
                                            {
                                                add = false;
                                            }
                                        }
                                    }
                                }
                                #endregion
                                if (add)
                                {
                                    //CreateProduct_Image gambarVariant = new CreateProduct_Image()
                                    //{
                                    //    file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                    //    file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                    //    x = 128,
                                    //    y = 128
                                    //};
                                    CreateProduct_Opt newOpt = new CreateProduct_Opt()
                                    {
                                        //change 30 nov 2020, set 0 untuk custom text
                                        //unit_value_id = Convert.ToInt32(fe_record.MP_VALUE_VAR),
                                        unit_value_id = 0,
                                        //end change 30 nov 2020, set 0 untuk custom text
                                        //t_id = fe_record.RECNUM,
                                        value = var_stf20.Where(p => p.LEVEL_VAR == fe_record.LEVEL_VAR && p.KODE_VAR == fe_record.KODE_VAR).FirstOrDefault()?.KET_VAR,
                                        //image = new List<CreateProduct_Image>()
                                    };
                                    listRecnumLv1.Add(fe_record.RECNUM);
                                    //newOpt.image.Add(gambarVariant);

                                    #region 6/9/2019, barang varian 2 gambar
                                    //if (!string.IsNullOrEmpty(var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_2))
                                    //{
                                    //    CreateProduct_Image gambarVariant2 = new CreateProduct_Image()
                                    //    {
                                    //        file_name = "Image2 " + Convert.ToString(fe_record.RECNUM),
                                    //        file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_2,
                                    //        x = 128,
                                    //        y = 128
                                    //    };
                                    //    newOpt.image.Add(gambarVariant);
                                    //}
                                    #endregion

                                    newVariasi.options.Add(newOpt);

                                    if (newDataProduct.pictures.Count() == 0)
                                    {
                                        newDataProduct.pictures.Add(new CreateProduct_Images()
                                        {
                                            //image_file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                            //image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                            //image_description = ""
                                            file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1
                                        });

                                        #region 6/9/2019, barang varian 2 gambar
                                        ////if (!string.IsNullOrEmpty(var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1))
                                        ////{
                                        ////    newDataProduct.images.Add(new CreateProduct_Images()
                                        ////    {
                                        ////        image_file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                        ////        image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                        ////        image_description = ""
                                        ////    });
                                        ////}

                                        //if (!string.IsNullOrEmpty(var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_2))
                                        //{
                                        //    newDataProduct.images.Add(new CreateProduct_Images()
                                        //    {
                                        //        image_file_name = "Image2 " + Convert.ToString(fe_record.RECNUM),
                                        //        image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_2,
                                        //        image_description = ""
                                        //    });
                                        //}
                                        #endregion
                                    }
                                }
                            }
                            product_variant.selection.Add(newVariasi);
                        }
                        #endregion

                        #region variant lv 2
                        if (var_strukturVar.Where(p => p.LEVEL_VAR == 2).Count() > 0)
                        {
                            //int variant_id = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_JUDUL_VAR);
                            //int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                            //int unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                            var variant_idText = var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_JUDUL_VAR;
                            int variant_id = 0;
                            int unit_id = 0;
                            var sVariant_idText = variant_idText.Split(',');
                            variant_id = Convert.ToInt32(sVariant_idText[0]);
                            if (sVariant_idText.Length == 2)
                            {
                                unit_id = Convert.ToInt32(sVariant_idText[1]);
                            }
                            else
                            {
                                int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                                unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                            }

                            CreateProduct_Variant newVariasi = new CreateProduct_Variant()
                            {
                                id = variant_id,
                                unit_id = unit_id,
                                //pos = 2,
                                options = new List<CreateProduct_Opt>()
                            };

                            foreach (var fe_record in var_strukturVar.Where(p => p.LEVEL_VAR == 2))
                            {
                                #region cek duplikat variant_id, unit_id, value_id
                                bool add = true;
                                if (product_variant.selection.Count > 0)
                                {
                                    foreach (var variant in product_variant.selection.Where(p => p.id == variant_id && p.unit_id == unit_id))
                                    {
                                        var added_value_id = variant.options.Select(p => p.unit_value_id).ToList();
                                        if (add)
                                        {
                                            if (added_value_id.Contains(Convert.ToInt32(fe_record.MP_VALUE_VAR))) //value_id sudah ada 
                                            {
                                                add = false;
                                            }
                                        }
                                    }
                                }
                                #endregion
                                if (add)
                                {
                                    //CreateProduct_Image gambarVariant = new CreateProduct_Image()
                                    //{
                                    //    file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                    //    file_path = var_stf02.Where(p => p.Sort9 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                    //    x = 128,
                                    //    y = 128
                                    //};
                                    CreateProduct_Opt newOpt = new CreateProduct_Opt()
                                    {
                                        //change 30 nov 2020, set 0 untuk custom text
                                        //unit_value_id = Convert.ToInt32(fe_record.MP_VALUE_VAR),
                                        unit_value_id = 0,
                                        //end change 30 nov 2020, set 0 untuk custom text
                                        //t_id = fe_record.RECNUM,
                                        value = var_stf20.Where(p => p.LEVEL_VAR == fe_record.LEVEL_VAR && p.KODE_VAR == fe_record.KODE_VAR).FirstOrDefault()?.KET_VAR,
                                        //image = new List<CreateProduct_Image>()
                                    };
                                    //newOpt.image.Add(gambarVariant);
                                    listRecnumLv2.Add(fe_record.RECNUM);

                                    #region 6/9/2019, barang varian 2 gambar
                                    //if (!string.IsNullOrEmpty(var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_2))
                                    //{
                                    //    CreateProduct_Image gambarVariant2 = new CreateProduct_Image()
                                    //    {
                                    //        file_name = "Image2 " + Convert.ToString(fe_record.RECNUM),
                                    //        file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_2,
                                    //        x = 128,
                                    //        y = 128
                                    //    };
                                    //    newOpt.image.Add(gambarVariant);
                                    //}
                                    #endregion

                                    newVariasi.options.Add(newOpt);

                                    if (newDataProduct.pictures.Count() == 0)
                                    {
                                        newDataProduct.pictures.Add(new CreateProduct_Images()
                                        {
                                            //image_file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                            //image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                            //image_description = ""
                                            file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1
                                        });
                                        #region 6/9/2019, barang varian 2 gambar
                                        ////if (!string.IsNullOrEmpty(var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1))
                                        ////{
                                        ////    newDataProduct.images.Add(new CreateProduct_Images()
                                        ////    {
                                        ////        image_file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                        ////        image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                        ////        image_description = ""
                                        ////    });
                                        ////}

                                        //if (!string.IsNullOrEmpty(var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_2))
                                        //{
                                        //    newDataProduct.images.Add(new CreateProduct_Images()
                                        //    {
                                        //        image_file_name = "Image2 " + Convert.ToString(fe_record.RECNUM),
                                        //        image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_2,
                                        //        image_description = ""
                                        //    });
                                        //}
                                        #endregion
                                    }
                                }
                            }
                            //cek duplikat map variasi
                            var duplicateVdanVU = false;
                            foreach (var item in product_variant.selection)
                            {
                                if (item.id == newVariasi.id && item.unit_id == newVariasi.unit_id)
                                {
                                    duplicateVdanVU = true;
                                }
                            }
                            if (!duplicateVdanVU)
                            {
                                product_variant.selection.Add(newVariasi);
                            }
                        }
                        #endregion

                        //product variasi
                        foreach (var item_var in var_stf02)
                        {
                            var price_var = var_stf02h.Where(p => p.BRG == item_var.BRG).FirstOrDefault();
                            CreateProduct_Product_Variant1 newProductVariasi = new CreateProduct_Product_Variant1()
                            {
                                //st = 1,
                                status = "LIMITED",
                                stock = 1,
                                //change by nurul 11/2/2020, ambil harga jual barang per variasi
                                //price_var = (float)item_var.HJUAL,
                                price = Convert.ToInt32(price_var.HJUAL),
                                //end change by nurul 11/2/2020, ambil harga jual barang per variasi
                                sku = item_var.BRG,
                                combination = new List<int>(),
                                pictures = new List<CreateProduct_Images>()
                            };
                            //newProductVariasi.combination.Add(Convert.ToInt32(price_var.RecNum));
                            if (!string.IsNullOrWhiteSpace(item_var.Sort8))
                            {
                                var recnumVariasi = var_strukturVar.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item_var.Sort8).FirstOrDefault();
                                if (recnumVariasi != null)
                                {
                                    //cek apakah recnumVariasi.RECNUM ada di opt variant
                                    //var doAddOpt = false;
                                    foreach (var item in product_variant.selection)
                                    {
                                        int combi = 0;
                                        foreach (var opts in item.options)
                                        {
                                            //if (opts.t_id == Convert.ToInt32(recnumVariasi.RECNUM))
                                            //if (listRecnumLv1.Contains(recnumVariasi.RECNUM))
                                            if (opts.value == item_var.Ket_Sort8)
                                            {
                                                //doAddOpt = true;
                                                newProductVariasi.combination.Add(combi);
                                            }
                                            combi++;
                                        }
                                    }
                                    //if (doAddOpt)
                                    //{
                                    //    newProductVariasi.combination.Add(Convert.ToInt32(recnumVariasi.RECNUM));
                                    //}
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(item_var.Sort9))
                            {
                                var recnumVariasi = var_strukturVar.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == item_var.Sort9).FirstOrDefault();
                                if (recnumVariasi != null)
                                {
                                    //cek apakah recnumVariasi.RECNUM ada di opt variant
                                    //var doAddOpt = false;
                                    foreach (var item in product_variant.selection)
                                    {
                                        int combi2 = 0;
                                        foreach (var opts in item.options)
                                        {
                                            //if (opts.t_id == Convert.ToInt32(recnumVariasi.RECNUM))
                                            //if (listRecnumLv1.Contains(recnumVariasi.RECNUM))
                                            //{
                                            //    doAddOpt = true;
                                            //}
                                            //else if (listRecnumLv2.Contains(recnumVariasi.RECNUM))
                                            //{
                                            //    doAddOpt = true;
                                            //}
                                            if (opts.value == item_var.Ket_Sort9)
                                            {
                                                //doAddOpt = true;
                                                newProductVariasi.combination.Add(combi2);
                                            }
                                            combi2++;
                                        }
                                    }
                                    //if (doAddOpt)
                                    //{
                                    //    newProductVariasi.combination.Add(Convert.ToInt32(recnumVariasi.RECNUM));
                                    //}
                                }
                            }
                            var imageVar = new CreateProduct_Images();
                            imageVar.file_path = item_var.LINK_GAMBAR_1;
                            newProductVariasi.pictures.Add(imageVar);
                            product_variant.products.Add(newProductVariasi);
                        }

                        //if (newDataProduct.pictures.Count > 0)
                        //    product_variant.sizecharts.Add(newDataProduct.pictures[0]);
                        newDataProduct.variant = product_variant;
                    }
                    //else if (brg_stf02.TYPE == "3")
                    //{
                    //    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_1))
                    //    {
                    //        newDataProduct.images.Add(new CreateProduct_Images()
                    //        {
                    //            image_description = "",
                    //            image_file_name = "",
                    //            image_file_path = brg_stf02.LINK_GAMBAR_1
                    //        });
                    //    }

                    //    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_2))
                    //    {
                    //        newDataProduct.images.Add(new CreateProduct_Images()
                    //        {
                    //            image_description = "",
                    //            image_file_name = "",
                    //            image_file_path = brg_stf02.LINK_GAMBAR_2
                    //        });
                    //    }

                    //    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_3))
                    //    {
                    //        newDataProduct.images.Add(new CreateProduct_Images()
                    //        {
                    //            image_description = "",
                    //            image_file_name = "",
                    //            image_file_path = brg_stf02.LINK_GAMBAR_3
                    //        });
                    //    }
                    //    //add 6/9/2019, 5 gambar
                    //    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_4))
                    //    {
                    //        newDataProduct.images.Add(new CreateProduct_Images()
                    //        {
                    //            image_description = "",
                    //            image_file_name = "",
                    //            image_file_path = brg_stf02.LINK_GAMBAR_4
                    //        });
                    //    }

                    //    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_5))
                    //    {
                    //        newDataProduct.images.Add(new CreateProduct_Images()
                    //        {
                    //            image_description = "",
                    //            image_file_name = "",
                    //            image_file_path = brg_stf02.LINK_GAMBAR_5
                    //        });
                    //    }
                    //    //end add 6/9/2019, 5 gambar
                    //}
                    newData.products.Add(newDataProduct);

                    string myData = JsonConvert.SerializeObject(newData);
                    string responseFromServer = "";

                    //try
                    //{
                    MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                    {
                        REQUEST_ID = milis.ToString(),
                        REQUEST_ACTION = "Create Product",
                        REQUEST_DATETIME = milisBack,
                        REQUEST_ATTRIBUTE_1 = brg,
                        REQUEST_ATTRIBUTE_2 = "fs : " + iden.merchant_code,
                        REQUEST_STATUS = "Pending",
                    };
                    manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
                    try
                    {
                        var client = new HttpClient();
                        client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + iden.token));
                        var content = new StringContent(myData, Encoding.UTF8, "application/json");
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
                        HttpResponseMessage clientResponse = await client.PostAsync(
                            urll, content);

                        using (HttpContent responseContent = clientResponse.Content)
                        {
                            using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                            {
                                responseFromServer = await reader.ReadToEndAsync();
                            }
                        };
                    }
                    catch (WebException e)
                    {
                        EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE STF02H SET BRG_MP = '' WHERE BRG = '" + Convert.ToString(brg)
                                + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
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
                            throw new Exception(err);
                        }
                        throw e;
                    }

                    if (responseFromServer != "")
                    {
                        var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokpedCreateProductResult)) as TokpedCreateProductResult;
                        //if (result.header.error_code == 0)
                        if (string.IsNullOrEmpty(result.header.reason))
                        {
                            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
                            //change by calvin 9 juni 2019
                            //await CreateProductGetStatus(iden, brg, result.data.upload_id, currentLog.REQUEST_ID);
                            EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = 'PENDING;" + Convert.ToString(result.data.upload_id) + ";"
                                + Convert.ToString(currentLog.REQUEST_ID) + "',LINK_STATUS='Buat Produk Pending', LINK_DATETIME = '"
                                + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '0;Buat Produk;;' WHERE BRG = '" + Convert.ToString(brg)
                                + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
#if (DEBUG || Debug_AWS)
                        await CreateProductGetStatus(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, "Link Produk (Tahap 1 / 2 )", iden, brg, result.data.upload_id, currentLog.REQUEST_ID);
#else
                            string EDBConnID = EDB.GetConnectionString("ConnId");
                            var sqlStorage = new SqlServerStorage(EDBConnID);

                            var Jobclient = new BackgroundJobClient(sqlStorage);
                            //Jobclient.Enqueue<TokopediaControllerJob>(x => x.CreateProductGetStatus(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, "Link Produk (Tahap 1 / 2 )", iden, brg, result.data.upload_id, currentLog.REQUEST_ID));
                            Jobclient.Schedule<TokopediaControllerJob>(x => x.CreateProductGetStatus(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, "Link Produk (Tahap 1 / 2 )", iden, brg, result.data.upload_id, currentLog.REQUEST_ID), TimeSpan.FromMinutes(1));
                            //end change by calvin 9 juni 2019
#endif
                        }
                        else
                        {
                            EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE STF02H SET BRG_MP = '' WHERE BRG = '" + Convert.ToString(brg)
                                + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                            currentLog.REQUEST_RESULT = result.header.reason;
                            currentLog.REQUEST_EXCEPTION = result.header.messages;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                            throw new Exception(result.header.messages + ";" + result.header.reason);
                        }
                    }
                    //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                    //myReq.Method = "POST";
                    //myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
                    //myReq.Accept = "application/x-www-form-urlencoded";
                    //myReq.ContentType = "application/json";
                    //try
                    //{
                    //    myReq.ContentLength = myData.Length;
                    //    using (var dataStream = myReq.GetRequestStream())
                    //    {
                    //        dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
                    //    }
                    //    using (WebResponse response = await myReq.GetResponseAsync())
                    //    {
                    //        using (Stream stream = response.GetResponseStream())
                    //        {
                    //            StreamReader reader = new StreamReader(stream);
                    //            responseFromServer = reader.ReadToEnd();
                    //        }
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                    //    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                    //}

                }
            }
            catch(Exception ex)
            {
                string sSQL = "UPDATE STF02H SET BRG_MP = '' WHERE BRG_MP = 'WAITING_FOR_HANGFIRE' and BRG = '" + kodeProduk + "' AND IDMARKET = " + iden.idmarket;
                EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);
                throw new Exception(ex.Message);
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Tokopedia Gagal.")]
        public async Task<string> CreateProduct_V3(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, string brg)
        {
            string ret = "";
            var token = SetupContext(iden);
            iden.token = token;
            var brg_stf02 = ErasoftDbContext.STF02.Where(p => p.BRG == brg).SingleOrDefault();
            try
            {
                if (brg_stf02 != null)
                {
                    var brg_stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == brg && p.IDMARKET == iden.idmarket).SingleOrDefault();
                    //string urll = "https://fs.tokopedia.net/v2/products/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/create?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);
                    string urll = "https://fs.tokopedia.net/v3/products/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/create?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);

                    long milis = CurrentTimeMillis();
                    DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

                    CreateProductTokpedData newData = new CreateProductTokpedData()
                    {
                        products = new List<CreateProduct_Product>()
                    };
                    CreateProduct_Productv3 newDataProduct = new CreateProduct_Productv3()
                    {
                        name = Convert.ToString(brg_stf02.NAMA + " " + brg_stf02.NAMA2).Trim(),
                        category_id = Convert.ToInt32(brg_stf02h.CATEGORY_CODE),
                        price = Convert.ToInt32(brg_stf02h.HJUAL),
                        status = "LIMITED",//UNLIMITED, LIMITED, and EMPTY
                        min_order = 1,
                        weight = Convert.ToInt32(brg_stf02.BERAT),
                        weight_unit = "GR",//GR (gram) and KG (kilogram)
                        condition = "NEW",//NEW and USED
                        description = brg_stf02.Deskripsi,
                        is_must_insurance = false,
                        is_free_return = false,
                        sku = brg_stf02.BRG,
                        stock = 1, //1 - 10000.Stock should be 1 if want to add variant product. 0 indicates always availabl
                        wholesale = null,
                        preorder = null,
                        videos = null,
                        pictures = new List<CreateProduct_Images>(),
                        price_currency = "IDR"
                    };
                    newDataProduct.dimension = new CreateProduct_Dimension
                    {
                        height = brg_stf02.TINGGI,
                        width = brg_stf02.LEBAR,
                        length = brg_stf02.PANJANG
                    };
                    if (!string.IsNullOrEmpty(brg_stf02h.NAMA_BARANG_MP))
                    {
                        newDataProduct.name = brg_stf02h.NAMA_BARANG_MP.Trim().Replace("�", "`");
                    }
                    if (!string.IsNullOrEmpty(brg_stf02h.DESKRIPSI_MP))
                    {
                        if (brg_stf02h.DESKRIPSI_MP != "null")
                            newDataProduct.description = brg_stf02h.DESKRIPSI_MP.Replace("�", "`");
                    }

                    if (!string.IsNullOrEmpty(brg_stf02h.AVALUE_35))
                    {
                        if (brg_stf02h.AVALUE_35 == "1")
                        {
                            newDataProduct.is_must_insurance = true;
                        }
                    }
                    if (brg_stf02h.MIN_ORDER != null)
                    {
                        if (brg_stf02h.MIN_ORDER.Value > 1)
                        {
                            newDataProduct.min_order = brg_stf02h.MIN_ORDER.Value;
                        }
                    }
                    if (!string.IsNullOrEmpty(brg_stf02h.AVALUE_39))
                    {
                        var listCourir = brg_stf02h.AVALUE_39.Split(',');
                        newDataProduct.custom_product_logistics = new List<int>();
                        foreach (var courier in listCourir)
                        {
                            newDataProduct.custom_product_logistics.Add(Convert.ToInt32(courier));
                        }
                    }
                    //add by nurul 6/2/2020
                    //newDataProduct.description = newDataProduct.description.Replace("<p>", "").Replace("</p>", "").
                    //    Replace("</ul>\r\n\r\n", "").Replace("<ul>", "").Replace("&nbsp;\r\n\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("&nbsp;", " ").
                    //    Replace("\r\n", "").Replace("<br />", "\n");
                    //newDataProduct.description = System.Net.WebUtility.HtmlDecode(newDataProduct.description);
                    newDataProduct.description = WebUtility.HtmlDecode(System.Net.WebUtility.HtmlDecode(newDataProduct.description.Replace("&nbsp;", " "))).Replace("<p>", "").
                        Replace("</p>", "").Replace("</ul>", "").Replace("<ul>", "").Replace("\r\n\r\n", "\n").Replace("\r\n", "").Replace("<br />", "\n");
                    //end add by nurul 6/2/2020

                    newDataProduct.description = newDataProduct.description.Replace("&nbsp;", " ");
                    //add by calvin 1 mei 2019
                    var qty_stock = new StokControllerJob(iden.DatabasePathErasoft, username).GetQOHSTF08A(brg, "ALL");
                    if (qty_stock > 0)
                    {
                        newDataProduct.stock = Convert.ToInt32(qty_stock);
                    }
                    //end add by calvin 1 mei 2019

                    int etalase_id = Convert.ToInt32(brg_stf02h.PICKUP_POINT);

                    newDataProduct.etalase = new CreateProduct_Etalase()
                    {
                        id = etalase_id,
                        name = ""
                    };

                    //add 15/10/2019, selalu isi gambar induk
                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_1))
                    {
                        newDataProduct.pictures.Add(new CreateProduct_Images()
                        {
                            //image_description = "",
                            //image_file_name = "",
                            file_path = brg_stf02.LINK_GAMBAR_1
                        });
                    }

                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_2))
                    {
                        newDataProduct.pictures.Add(new CreateProduct_Images()
                        {
                            //image_description = "",
                            //image_file_name = "",
                            file_path = brg_stf02.LINK_GAMBAR_2
                        });
                    }

                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_3))
                    {
                        newDataProduct.pictures.Add(new CreateProduct_Images()
                        {
                            //image_description = "",
                            //image_file_name = "",
                            file_path = brg_stf02.LINK_GAMBAR_3
                        });
                    }
                    //add 6/9/2019, 5 gambar
                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_4))
                    {
                        newDataProduct.pictures.Add(new CreateProduct_Images()
                        {
                            //image_description = "",
                            //image_file_name = "",
                            file_path = brg_stf02.LINK_GAMBAR_4
                        });
                    }

                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_5))
                    {
                        newDataProduct.pictures.Add(new CreateProduct_Images()
                        {
                            //image_description = "",
                            //image_file_name = "",
                            file_path = brg_stf02.LINK_GAMBAR_5
                        });
                    }
                    //end add 6/9/2019, 5 gambar
                    //end add 15/20/2019, selalu isi gambar induk

                    if (brg_stf02.TYPE == "4") // punya variasi
                    {
                        CreateProduct_Product_Variant product_variant = new CreateProduct_Product_Variant()
                        {
                            products = new List<CreateProduct_Product_Variant1>(),
                            selection = new List<CreateProduct_Variant>(),
                            sizecharts = new List<CreateProduct_Images>()
                        };
                        //var AttributeOptTokped = MoDbContext.AttributeOptTokped.ToList();
                        var AttributeOptTokped = (await GetAttributeToList(iden, brg_stf02h.CATEGORY_CODE)).attribute_opt;
                        var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == brg).ToList();
                        var var_strukturVar = ErasoftDbContext.STF02I.Where(p => p.BRG == brg && p.MARKET == "TOKPED").ToList().OrderBy(p => p.RECNUM);
                        var var_stf02_brg_list = var_stf02.Select(p => p.BRG).ToList();
                        var var_stf02h = ErasoftDbContext.STF02H.Where(p => var_stf02_brg_list.Contains(p.BRG) && p.IDMARKET == iden.idmarket).ToList();

                        string category_mo = var_strukturVar.Select(p => p.CATEGORY_MO).FirstOrDefault();
                        var var_stf20 = ErasoftDbContext.STF20B.Where(p => p.CATEGORY_MO == category_mo).ToList();

                        var listRecnumLv1 = new List<int>();
                        var listRecnumLv2 = new List<int>();
                        #region variant lv 1
                        if (var_strukturVar.Where(p => p.LEVEL_VAR == 1).Count() > 0)
                        {
                            var variant_idText = var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_JUDUL_VAR;
                            int variant_id = 0;
                            int unit_id = 0;
                            var sVariant_idText = variant_idText.Split(',');
                            variant_id = Convert.ToInt32(sVariant_idText[0]);
                            if (sVariant_idText.Length == 2)
                            {
                                unit_id = Convert.ToInt32(sVariant_idText[1]);
                            }
                            else
                            {
                                int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                                unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                            }

                            CreateProduct_Variant newVariasi = new CreateProduct_Variant()
                            {
                                id = variant_id,
                                unit_id = unit_id,
                                //pos = 1,
                                options = new List<CreateProduct_Opt>()
                            };

                            foreach (var fe_record in var_strukturVar.Where(p => p.LEVEL_VAR == 1))
                            {
                                #region cek duplikat variant_id, unit_id, value_id
                                bool add = true;
                                if (product_variant.selection.Count > 0)
                                {
                                    foreach (var variant in product_variant.selection.Where(p => p.id == variant_id && p.unit_id == unit_id))
                                    {
                                        var added_value_id = variant.options.Select(p => p.unit_value_id).ToList();
                                        if (add)
                                        {
                                            if (added_value_id.Contains(Convert.ToInt32(fe_record.MP_VALUE_VAR))) //value_id sudah ada 
                                            {
                                                add = false;
                                            }
                                        }
                                    }
                                }
                                #endregion
                                if (add)
                                {
                                    //CreateProduct_Image gambarVariant = new CreateProduct_Image()
                                    //{
                                    //    file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                    //    file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                    //    x = 128,
                                    //    y = 128
                                    //};
                                    CreateProduct_Opt newOpt = new CreateProduct_Opt()
                                    {
                                        //change 30 nov 2020, set 0 untuk custom text
                                        //unit_value_id = Convert.ToInt32(fe_record.MP_VALUE_VAR),
                                        unit_value_id = 0,
                                        //end change 30 nov 2020, set 0 untuk custom text
                                        //t_id = fe_record.RECNUM,
                                        value = var_stf20.Where(p => p.LEVEL_VAR == fe_record.LEVEL_VAR && p.KODE_VAR == fe_record.KODE_VAR).FirstOrDefault()?.KET_VAR,
                                        //image = new List<CreateProduct_Image>()
                                    };
                                    listRecnumLv1.Add(fe_record.RECNUM);
                                    //newOpt.image.Add(gambarVariant);


                                    newVariasi.options.Add(newOpt);

                                    if (newDataProduct.pictures.Count() == 0)
                                    {
                                        newDataProduct.pictures.Add(new CreateProduct_Images()
                                        {
                                            //image_file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                            //image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                            //image_description = ""
                                            file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1
                                        });

                                    }
                                }
                            }
                            product_variant.selection.Add(newVariasi);
                        }
                        #endregion

                        #region variant lv 2
                        if (var_strukturVar.Where(p => p.LEVEL_VAR == 2).Count() > 0)
                        {
                            var variant_idText = var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_JUDUL_VAR;
                            int variant_id = 0;
                            int unit_id = 0;
                            var sVariant_idText = variant_idText.Split(',');
                            variant_id = Convert.ToInt32(sVariant_idText[0]);
                            if (sVariant_idText.Length == 2)
                            {
                                unit_id = Convert.ToInt32(sVariant_idText[1]);
                            }
                            else
                            {
                                int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                                unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                            }

                            CreateProduct_Variant newVariasi = new CreateProduct_Variant()
                            {
                                id = variant_id,
                                unit_id = unit_id,
                                //pos = 2,
                                options = new List<CreateProduct_Opt>()
                            };

                            foreach (var fe_record in var_strukturVar.Where(p => p.LEVEL_VAR == 2))
                            {
                                #region cek duplikat variant_id, unit_id, value_id
                                bool add = true;
                                if (product_variant.selection.Count > 0)
                                {
                                    foreach (var variant in product_variant.selection.Where(p => p.id == variant_id && p.unit_id == unit_id))
                                    {
                                        var added_value_id = variant.options.Select(p => p.unit_value_id).ToList();
                                        if (add)
                                        {
                                            if (added_value_id.Contains(Convert.ToInt32(fe_record.MP_VALUE_VAR))) //value_id sudah ada 
                                            {
                                                add = false;
                                            }
                                        }
                                    }
                                }
                                #endregion
                                if (add)
                                {
                                    //CreateProduct_Image gambarVariant = new CreateProduct_Image()
                                    //{
                                    //    file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                    //    file_path = var_stf02.Where(p => p.Sort9 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                    //    x = 128,
                                    //    y = 128
                                    //};
                                    CreateProduct_Opt newOpt = new CreateProduct_Opt()
                                    {
                                        //change 30 nov 2020, set 0 untuk custom text
                                        //unit_value_id = Convert.ToInt32(fe_record.MP_VALUE_VAR),
                                        unit_value_id = 0,
                                        //end change 30 nov 2020, set 0 untuk custom text
                                        //t_id = fe_record.RECNUM,
                                        value = var_stf20.Where(p => p.LEVEL_VAR == fe_record.LEVEL_VAR && p.KODE_VAR == fe_record.KODE_VAR).FirstOrDefault()?.KET_VAR,
                                        //image = new List<CreateProduct_Image>()
                                    };
                                    //newOpt.image.Add(gambarVariant);
                                    listRecnumLv2.Add(fe_record.RECNUM);


                                    newVariasi.options.Add(newOpt);

                                    if (newDataProduct.pictures.Count() == 0)
                                    {
                                        newDataProduct.pictures.Add(new CreateProduct_Images()
                                        {
                                            //image_file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                            //image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                            //image_description = ""
                                            file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1
                                        });
                                    }
                                }
                            }
                            //cek duplikat map variasi
                            var duplicateVdanVU = false;
                            foreach (var item in product_variant.selection)
                            {
                                if (item.id == newVariasi.id && item.unit_id == newVariasi.unit_id)
                                {
                                    duplicateVdanVU = true;
                                }
                            }
                            if (!duplicateVdanVU)
                            {
                                product_variant.selection.Add(newVariasi);
                            }
                        }
                        #endregion

                        //product variasi
                        foreach (var item_var in var_stf02)
                        {
                            var price_var = var_stf02h.Where(p => p.BRG == item_var.BRG).FirstOrDefault();
                            CreateProduct_Product_Variant1 newProductVariasi = new CreateProduct_Product_Variant1()
                            {
                                //st = 1,
                                status = "LIMITED",
                                stock = 1,
                                //change by nurul 11/2/2020, ambil harga jual barang per variasi
                                //price_var = (float)item_var.HJUAL,
                                price = Convert.ToInt32(price_var.HJUAL),
                                //end change by nurul 11/2/2020, ambil harga jual barang per variasi
                                sku = item_var.BRG,
                                combination = new List<int>(),
                                pictures = new List<CreateProduct_Images>()
                            };
                            //newProductVariasi.combination.Add(Convert.ToInt32(price_var.RecNum));
                            if (!string.IsNullOrWhiteSpace(item_var.Sort8))
                            {
                                var recnumVariasi = var_strukturVar.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item_var.Sort8).FirstOrDefault();
                                if (recnumVariasi != null)
                                {
                                    //cek apakah recnumVariasi.RECNUM ada di opt variant
                                    //var doAddOpt = false;
                                    foreach (var item in product_variant.selection)
                                    {
                                        int combi = 0;
                                        foreach (var opts in item.options)
                                        {
                                            //if (opts.t_id == Convert.ToInt32(recnumVariasi.RECNUM))
                                            //if (listRecnumLv1.Contains(recnumVariasi.RECNUM))
                                            if (opts.value == item_var.Ket_Sort8)
                                            {
                                                //doAddOpt = true;
                                                newProductVariasi.combination.Add(combi);
                                            }
                                            combi++;
                                        }
                                    }
                                    //if (doAddOpt)
                                    //{
                                    //    newProductVariasi.combination.Add(Convert.ToInt32(recnumVariasi.RECNUM));
                                    //}
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(item_var.Sort9))
                            {
                                var recnumVariasi = var_strukturVar.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == item_var.Sort9).FirstOrDefault();
                                if (recnumVariasi != null)
                                {
                                    //cek apakah recnumVariasi.RECNUM ada di opt variant
                                    //var doAddOpt = false;
                                    foreach (var item in product_variant.selection)
                                    {
                                        int combi2 = 0;
                                        foreach (var opts in item.options)
                                        {
                                            //if (opts.t_id == Convert.ToInt32(recnumVariasi.RECNUM))
                                            //if (listRecnumLv1.Contains(recnumVariasi.RECNUM))
                                            //{
                                            //    doAddOpt = true;
                                            //}
                                            //else if (listRecnumLv2.Contains(recnumVariasi.RECNUM))
                                            //{
                                            //    doAddOpt = true;
                                            //}
                                            if (opts.value == item_var.Ket_Sort9)
                                            {
                                                //doAddOpt = true;
                                                newProductVariasi.combination.Add(combi2);
                                            }
                                            combi2++;
                                        }
                                    }
                                    //if (doAddOpt)
                                    //{
                                    //    newProductVariasi.combination.Add(Convert.ToInt32(recnumVariasi.RECNUM));
                                    //}
                                }
                            }
                            var imageVar = new CreateProduct_Images();
                            imageVar.file_path = item_var.LINK_GAMBAR_1;
                            newProductVariasi.pictures.Add(imageVar);
                            product_variant.products.Add(newProductVariasi);
                        }

                        //if (newDataProduct.pictures.Count > 0)
                        //    product_variant.sizecharts.Add(newDataProduct.pictures[0]);
                        newDataProduct.variant = product_variant;
                    }
                    newData.products.Add(newDataProduct);

                    string myData = JsonConvert.SerializeObject(newData);
                    string responseFromServer = "";

                    //try
                    //{
                    MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                    {
                        REQUEST_ID = milis.ToString(),
                        REQUEST_ACTION = "Create Product",
                        REQUEST_DATETIME = milisBack,
                        REQUEST_ATTRIBUTE_1 = brg,
                        REQUEST_ATTRIBUTE_2 = "fs : " + iden.merchant_code,
                        REQUEST_STATUS = "Pending",
                    };
                    manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
                    try
                    {
                        var client = new HttpClient();
                        client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + iden.token));
                        var content = new StringContent(myData, Encoding.UTF8, "application/json");
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
                        HttpResponseMessage clientResponse = await client.PostAsync(
                            urll, content);

                        using (HttpContent responseContent = clientResponse.Content)
                        {
                            using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                            {
                                responseFromServer = await reader.ReadToEndAsync();
                            }
                        };
                    }
                    catch (WebException e)
                    {
                        EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE STF02H SET BRG_MP = '' WHERE BRG = '" + Convert.ToString(brg)
                                + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
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
                            throw new Exception(err);
                        }
                        throw e;
                    }

                    if (responseFromServer != "")
                    {
                        var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokpedCreateProductResultV3)) as TokpedCreateProductResultV3;
                        //if (result.header.error_code == 0)
                        if (result.data.fail_data == 0)
                        {
                            await getItemDetailToLink(dbPathEra, kodeProduk, log_CUST, iden, iden.idmarket, 1, result.data.success_rows_data[0].product_id);
                            if (brg_stf02.TYPE == "4")
                            {
                                await GetActiveItemVariantByProductID(dbPathEra, kodeProduk, log_CUST, "", "", iden, brg, iden.idmarket, result.data.success_rows_data[0].product_id.ToString(), "");
                            }
                        }
                        else
                        {
                            EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE STF02H SET BRG_MP = '' WHERE BRG = '" + Convert.ToString(brg)
                                + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                            string errMsg = "";
                            foreach(var err in result.data.failed_rows_data[0].error)
                            {
                                if (!string.IsNullOrEmpty(errMsg))
                                    errMsg += " ; ";
                                errMsg += err;
                            }
                            currentLog.REQUEST_RESULT = errMsg;
                            currentLog.REQUEST_EXCEPTION = errMsg;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                            throw new Exception(errMsg);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                string sSQL = "UPDATE STF02H SET BRG_MP = '' WHERE BRG_MP = 'WAITING_FOR_HANGFIRE' and BRG = '" + kodeProduk + "' AND IDMARKET = " + iden.idmarket;
                EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);
                throw new Exception(ex.Message);
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 1)]
        [Queue("3_general")]
        public async Task<string> CheckPendings(TokopediaAPIData data)
        {
            SetupContext(data);
            var arf01inDB = ErasoftDbContext.ARF01.Where(p => (p.RecNum ?? 0) == data.idmarket).SingleOrDefault();

            string ret = "";
            if (arf01inDB.STATUS_API == "1")
            {
                var cekPendingCreate = ErasoftDbContext.STF02H.Where(p => p.IDMARKET == data.idmarket && p.BRG_MP.Contains("PENDING;")).ToList();
                if (cekPendingCreate.Count > 0)
                {
                    foreach (var item in cekPendingCreate)
                    {
#if (DEBUG || Debug_AWS)
                        await new TokopediaControllerJob().CreateProductGetStatus(data.DatabasePathErasoft, item.BRG, arf01inDB.CUST, "Barang", "Link Produk (Tahap 1 / 2 )", data, item.BRG, Convert.ToInt32(item.BRG_MP.Split(';')[1]), Convert.ToString(item.BRG_MP.Split(';')[2]));
                        //end change by calvin 9 juni 2019
#else

                        //change by calvin 9 juni 2019
                        //Task.Run(() => CreateProductGetStatus(data, item.BRG, Convert.ToInt32(item.BRG_MP.Split(';')[1]), item.BRG_MP.Split(';')[2]).Wait());
                        string EDBConnID = EDB.GetConnectionString("ConnId");
                        var sqlStorage = new SqlServerStorage(EDBConnID);

                        var Jobclient = new BackgroundJobClient(sqlStorage);
                        Jobclient.Enqueue<TokopediaControllerJob>(x => x.CreateProductGetStatus(data.DatabasePathErasoft, item.BRG, arf01inDB.CUST, "Barang", "Link Produk (Tahap 1 / 2 )", data, item.BRG, Convert.ToInt32(item.BRG_MP.Split(';')[1]), Convert.ToString(item.BRG_MP.Split(';')[2])));
                        //end change by calvin 9 juni 2019
#endif
                    }
                }
                var cekPendingEdit = ErasoftDbContext.STF02H.Where(p => p.IDMARKET == data.idmarket && p.BRG_MP.Contains("PEDITENDING;")).ToList();
                if (cekPendingEdit.Count > 0)
                {
                    foreach (var item in cekPendingEdit)
                    {
                        //change by calvin 9 juni 2019
                        //Task.Run(() => EditProductGetStatus(data, item.BRG, Convert.ToInt32(item.BRG_MP.Split(';')[1]), item.BRG_MP.Split(';')[2], item.BRG_MP.Split(';')[3]).Wait());
                        string EDBConnID = EDB.GetConnectionString("ConnId");
                        var sqlStorage = new SqlServerStorage(EDBConnID);

                        var Jobclient = new BackgroundJobClient(sqlStorage);
                        Jobclient.Enqueue<TokopediaControllerJob>(x => x.EditProductGetStatus(data.DatabasePathErasoft, item.BRG, arf01inDB.CUST, "Barang", "Edit Produk Get Status", data, item.BRG, Convert.ToInt32(item.BRG_MP.Split(';')[1]), item.BRG_MP.Split(';')[2], item.BRG_MP.Split(';')[3]));
                        //end change by calvin 9 juni 2019
                    }
                }
            }
            return ret;
        }

        //change by nurul 23/3/2020, request pickup tokopedia 
        //[AutomaticRetry(Attempts = 3)]
        //[Queue("1_manage_pesanan")]
        //[NotifyOnFailed("Request Pickup Pesanan {obj} ke Tokopedia Gagal.")]
        //public async Task<string> PostRequestPickup(string dbPathEra, string namaPemesan, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, string NO_BUKTI_SOT01A, string NO_REFERENSI_SOT01A)
        //{
        //    var token = SetupContext(iden);
        //    iden.token = token;
        //    string ret = "";
        //    string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/pick-up";
        //    long milis = CurrentTimeMillis();
        //    DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

        //    //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
        //    //{
        //    //    REQUEST_ID = milis.ToString(),
        //    //    REQUEST_ACTION = "Request PIckup",
        //    //    REQUEST_DATETIME = milisBack,
        //    //    REQUEST_ATTRIBUTE_1 = "fs : " + iden.merchant_code,
        //    //    REQUEST_ATTRIBUTE_2 = "orderNo : " + NO_BUKTI_SOT01A,
        //    //    REQUEST_ATTRIBUTE_3 = "NoRef : " + NO_REFERENSI_SOT01A,
        //    //    REQUEST_STATUS = "Pending",
        //    //};

        //    //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
        //    RequestPickup newData = new RequestPickup()
        //    {
        //        order_id = Convert.ToInt32(NO_REFERENSI_SOT01A),
        //        shop_id = Convert.ToInt32(iden.API_secret_key),
        //        request_time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss +0000 UTC")
        //    };
        //    List<RequestPickup> newDataList = new List<RequestPickup>();
        //    newDataList.Add(newData);
        //    string myData = JsonConvert.SerializeObject(newDataList.ToArray());

        //    //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
        //    //myReq.Method = "POST";
        //    //myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
        //    //myReq.Accept = "application/json";
        //    //myReq.ContentType = "application/json";
        //    //string responseFromServer = "";
        //    //try
        //    //{
        //    //    myReq.ContentLength = myData.Length;
        //    //    using (var dataStream = myReq.GetRequestStream())
        //    //    {
        //    //        dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
        //    //    }
        //    //    using (WebResponse response = await myReq.GetResponseAsync())
        //    //    {
        //    //        using (Stream stream = response.GetResponseStream())
        //    //        {
        //    //            StreamReader reader = new StreamReader(stream);
        //    //            responseFromServer = reader.ReadToEnd();
        //    //        }
        //    //    }
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
        //    //    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
        //    //}


        //    string responseFromServer = "";
        //    //try
        //    //{
        //    var client = new HttpClient();
        //    client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + iden.token));
        //    var content = new StringContent(myData, Encoding.UTF8, "application/json");
        //    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
        //    HttpResponseMessage clientResponse = await client.PostAsync(
        //        urll, content);

        //    using (HttpContent responseContent = clientResponse.Content)
        //    {
        //        using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
        //        {
        //            responseFromServer = await reader.ReadToEndAsync();
        //        }
        //    };
        //    //}
        //    //catch (Exception ex)
        //    //{

        //    //}

        //    if (responseFromServer != "")
        //    {
        //        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
        //        contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Berhasil Request Pickup Pesanan " + Convert.ToString(namaPemesan) + " ke Tokopedia.");
        //        EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM='2' WHERE NO_BUKTI = '" + NO_BUKTI_SOT01A + "'");
        //        //TokopediaOrders result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaOrders)) as TokopediaOrders;
        //        //if (string.IsNullOrEmpty(result.errorCode.Value))
        //        //{
        //        //    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
        //        //    if (result.content.Count > 0)
        //        //    {
        //        //        foreach (var item in result.content)
        //        //        {
        //        //            await GetOrderDetail(iden, item.orderNo.Value, item.orderItemNo.Value, connId, CUST, NAMA_CUST);
        //        //        }
        //        //    }
        //        //}
        //        //else
        //        //{
        //        //    currentLog.REQUEST_RESULT = result.errorCode.Value;
        //        //    currentLog.REQUEST_EXCEPTION = result.errorMessage.Value;
        //        //    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
        //        //}
        //    }
        //    return ret;
        //}

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Request Pickup Pesanan {obj} ke Tokopedia Gagal.")]
        public async Task<string> PostRequestPickup(string dbPathEra, string namaPemesan, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, string NO_BUKTI_SOT01A, string NO_REFERENSI_SOT01A)
        {
            var token = SetupContext(iden);
            iden.token = token;
            string ret = "";
            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/pick-up";
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Request Pickup",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = "fs : " + iden.merchant_code,
                REQUEST_ATTRIBUTE_2 = "orderNo : " + NO_BUKTI_SOT01A,
                REQUEST_ATTRIBUTE_3 = "NoRef : " + NO_REFERENSI_SOT01A,
                REQUEST_STATUS = "Pending",
            };

            RequestPickup newData = new RequestPickup()
            {
                order_id = Convert.ToInt32(NO_REFERENSI_SOT01A),
                shop_id = Convert.ToInt32(iden.API_secret_key),
                //change by nurul 17/2/2020
                //request_time = DateTime.UtcNow.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss")
                //change by nurul 17/12/2021
                //request_time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                request_time = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss")
                //end change by nurul 17/12/2021
                //end change by nurul 17/2/2020
            };
            List<RequestPickup> newDataList = new List<RequestPickup>();
            newDataList.Add(newData);
            //change by nurul 17/2/2020
            //string myData = JsonConvert.SerializeObject(newDataList.ToArray());
            string myData = JsonConvert.SerializeObject(newData);
            //end change by nurul 17/2/2020

            string responseFromServer = "";
            var isSuccess = false;
            //try
            //{
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + iden.token));
            var content = new StringContent(myData, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
            HttpResponseMessage clientResponse = await client.PostAsync(
                urll, content);

            if (clientResponse != null)
            {
                if (clientResponse.IsSuccessStatusCode)
                {
                    isSuccess = true;
                }
                //responseFromServer = await clientResponse.Content.ReadAsStringAsync();
                using (HttpContent responseContent = clientResponse.Content)
                {
                    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                    {
                        responseFromServer = await reader.ReadToEndAsync();
                    }
                };
            }
            var httpReason = clientResponse.ReasonPhrase;



            //}
            //catch (Exception ex)
            //{

            //}

            if (responseFromServer != "")
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ReqPickupResult)) as ReqPickupResult;
                //if (result.header.error_code == 200)
                //{
                //    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                //    contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Berhasil Request Pickup Pesanan " + Convert.ToString(NO_BUKTI_SOT01A) + " ke Tokopedia.");
                //    EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM='2' WHERE NO_BUKTI = '" + NO_BUKTI_SOT01A + "'");

                //    //ret = NO_BUKTI_SOT01A;

                //    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                //    await GetNoAWB(iden, NO_BUKTI_SOT01A, NO_REFERENSI_SOT01A);
                //}
                //else
                //{
                //    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                //    contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Gagal Request Pickup Pesanan " + Convert.ToString(NO_BUKTI_SOT01A) + " ke Tokopedia.");
                //    //EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM='2' WHERE NO_BUKTI = '" + NO_BUKTI_SOT01A + "'");

                //    //currentLog.REQUEST_RESULT = result.header.reason;
                //    //currentLog.REQUEST_EXCEPTION = result.header.messages;
                //    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                //    throw new Exception(result.header.messages + ";" + result.header.reason);
                //}

                if (isSuccess)
                {
                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                    contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Berhasil Request Pickup Pesanan " + Convert.ToString(NO_BUKTI_SOT01A) + " ke Tokopedia.");
                    EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM='2' WHERE NO_BUKTI = '" + NO_BUKTI_SOT01A + "'");

                    //ret = NO_BUKTI_SOT01A;

                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    await GetNoAWB(iden, NO_BUKTI_SOT01A, NO_REFERENSI_SOT01A);
                }
                else
                {
                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                    contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Gagal Request Pickup Pesanan " + Convert.ToString(NO_BUKTI_SOT01A) + " ke Tokopedia.");
                    //EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM='2' WHERE NO_BUKTI = '" + NO_BUKTI_SOT01A + "'");

                    //currentLog.REQUEST_RESULT = result.header.reason;
                    //currentLog.REQUEST_EXCEPTION = result.header.messages;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                    throw new Exception(result.header.messages + ";" + result.header.reason);
                }
            }
            return ret;
        }
        //end change by nurul 23/3/2020

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Status Accept Pesanan {obj} ke Tokopedia Gagal.")]
        public async Task<string> PostAckOrder(string dbPathEra, string namaPemesan, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, string ordNo, string noref)
        {
            string ret = "";
            string[] splitNoRef = noref.Split(';');
            string urll = "https://fs.tokopedia.net/v1/order/" + Uri.EscapeDataString(splitNoRef[0]) + "/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/ack";
            long milis = CurrentTimeMillis();
            var token = SetupContext(iden);
            iden.token = token;
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Accept Order",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = "fs : " + iden.merchant_code,
                REQUEST_ATTRIBUTE_2 = "orderNo : " + ordNo,
                REQUEST_ATTRIBUTE_3 = "NoRef : " + splitNoRef[0],
                REQUEST_STATUS = "Pending",
            };

            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            AckOrder newData = new AckOrder();

            //remark by calvin 11 maret 2019
            //dari tokopedia : jika full accept, set saja products : null
            //MO belum ada accept partial
            //newData.products = new List<AckOrder_Product>();
            //var detailSO = ErasoftDbContext.SOT01B.Where(p => p.NO_BUKTI == ordNo).ToList();
            //foreach (var item in detailSO)
            //{
            //    AckOrder_Product product = new AckOrder_Product
            //    {
            //        product_id = item.BRG,
            //        quantity_deliver = item.QTY,
            //        quantity_reject = 0
            //    };
            //    newData.products.Add(product);
            //}
            //end remark by calvin 11 maret 2019

            string myData = JsonConvert.SerializeObject(newData);

            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            //myReq.Method = "POST";
            //myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            //myReq.Accept = "application/json";
            //myReq.ContentType = "application/json";
            //string responseFromServer = "";
            //try
            //{
            //    myReq.ContentLength = myData.Length;
            //    using (var dataStream = myReq.GetRequestStream())
            //    {
            //        dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
            //    }
            //    using (WebResponse response = await myReq.GetResponseAsync())
            //    {
            //        using (Stream stream = response.GetResponseStream())
            //        {
            //            StreamReader reader = new StreamReader(stream);
            //            responseFromServer = reader.ReadToEnd();
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}


            string responseFromServer = "";
            var isSuccess = false;
            //try
            //{
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + iden.token));
            var content = new StringContent(myData, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
            HttpResponseMessage clientResponse = await client.PostAsync(
                urll, content);

            if (clientResponse != null)
            {
                if (clientResponse.IsSuccessStatusCode)
                {
                    isSuccess = true;
                }
                //responseFromServer = await clientResponse.Content.ReadAsStringAsync();
                using (HttpContent responseContent = clientResponse.Content)
                {
                    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                    {
                        responseFromServer = await reader.ReadToEndAsync();
                    }
                };
            }
            //using (HttpContent responseContent = clientResponse.Content)
            //{
            //    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
            //    {
            //        responseFromServer = await reader.ReadToEndAsync();
            //    }
            //};

            var httpReason = clientResponse.ReasonPhrase;
            //}
            //catch (Exception ex)
            //{

            //}

            //if (responseFromServer != null)
            if (!string.IsNullOrEmpty(responseFromServer))
            {
                try
                {
                    //var sukses = false;
                    ActOrderResult result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ActOrderResult)) as ActOrderResult;
                    //if (result.status == "200 Ok")
                    //{
                    //    var pesananInDb = ErasoftDbContext.SOT01A.Where(a => a.NO_BUKTI == ordNo && a.NO_REFERENSI == noref).FirstOrDefault();
                    //    if (pesananInDb != null)
                    //    {
                    //        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    //        //#if (DEBUG || Debug_AWS)
                    //        //                        await JOBCOD(iden, pesananInDb.NO_BUKTI, pesananInDb.NO_REFERENSI);
                    //        //#else
                    //        //                        string EDBConnID = EDB.GetConnectionString("ConnId");
                    //        //                        var sqlStorage = new SqlServerStorage(EDBConnID);

                    //        //                        var Jobclient = new BackgroundJobClient(sqlStorage);
                    //        //                        Jobclient.Enqueue<TokopediaControllerJob>(x => x.JOBCOD(iden, pesananInDb.NO_BUKTI, pesananInDb.NO_REFERENSI));
                    //        //#endif
                    //    }
                    //}
                    //else if (result.error_message[0].Contains("order already ack-ed") || result.error_message[0].Contains("400") || result.error_message[0].Contains("450") || result.error_message[0].Contains("500") || result.error_message[0].Contains("600"))
                    //{
                    //    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    //}
                    //else
                    //{
                    //    var err_msg = "";
                    //    if (result.error_message.Count() > 0)
                    //    {
                    //        foreach (var err in result.error_message)
                    //        {
                    //            err_msg += " " + err + ".";
                    //        }
                    //    }
                    //    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                    //    //throw new Exception("Update Status Accept Pesanan " + splitNoRef[1] + " ke Tokopedia Gagal. " + result.error_message[0] + ".");
                    //    throw new Exception("Update Status Accept Pesanan " + splitNoRef[1] + " ke Tokopedia Gagal. " + err_msg );
                    //}

                    if (isSuccess)
                    {
                        //var pesananInDb = ErasoftDbContext.SOT01A.Where(a => a.NO_BUKTI == ordNo && a.NO_REFERENSI == noref).FirstOrDefault();
                        //if (pesananInDb != null)
                        //{
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                        //sukses = true;
                        //}
                    }
                    else if (httpReason == "Bad Request")
                    {
                        //if (responseFromServer.Contains("Order Status not eligible to accept") || result.header.reason == "Order Status not eligible to accept")

                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                        //sukses = true;
                    }
                    else
                    {
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        //sukses = false;
                        throw new Exception("Update Status Accept Pesanan " + splitNoRef[1] + " ke Tokopedia Gagal. " + result.header.messages + ". " + result.header.reason);
                    }

//                    if (sukses && type == 2)
//                    {
//#if (DEBUG || Debug_AWS)
//                        JOBCOD1(iden, ordNo, noref);
//#else
//                        string EDBConnID = EDB.GetConnectionString("ConnId");
//                        var sqlStorage = new SqlServerStorage(EDBConnID);

//                        var Jobclient = new BackgroundJobClient(sqlStorage);
//                        Jobclient.Enqueue<TokopediaControllerJob>(x => x.JOBCOD1(iden, ordNo, noref));
//#endif
//                    }
                }
                catch(Exception ex)
                {

                }
                //TokopediaOrders result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaOrders)) as TokopediaOrders;
                //if (string.IsNullOrEmpty(result.errorCode.Value))
                //{
                //    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                //    if (result.content.Count > 0)
                //    {
                //        foreach (var item in result.content)
                //        {
                //            await GetOrderDetail(iden, item.orderNo.Value, item.orderItemNo.Value, connId, CUST, NAMA_CUST);
                //        }
                //    }
                //}
                //else
                //{
                //    currentLog.REQUEST_RESULT = result.errorCode.Value;
                //    currentLog.REQUEST_EXCEPTION = result.errorMessage.Value;
                //    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                //}
            }
            return ret;
        }

        //add by nurul 19/3/2020, untuk get kode booking 
        [AutomaticRetry(Attempts = 3)]
        [Queue("3_general")]
        public async Task<string> GetSingleOrder(TokopediaAPIData iden, string cust, string nama_cust)
        {
            string ret = "";
            string connId = Guid.NewGuid().ToString();
            var token = SetupContext(iden);
            iden.token = token;
            //change by nurul 6/12/2021
            //var list_ordersn = ErasoftDbContext.SOT01A.AsNoTracking().Where(a => (a.TRACKING_SHIPMENT == null || a.TRACKING_SHIPMENT == "-" || a.TRACKING_SHIPMENT == "") && a.CUST == cust && (a.NO_REFERENSI.Contains("INV")) && (a.STATUS_TRANSAKSI.Contains("02") || a.STATUS_TRANSAKSI.Contains("03") || a.STATUS_TRANSAKSI.Contains("04"))).ToList();
            var getHMin2 = DateTime.UtcNow.AddHours(7).AddDays(-2);
            var list_ordersn = ErasoftDbContext.SOT01A.AsNoTracking().Where(a => (a.TRACKING_SHIPMENT == null || a.TRACKING_SHIPMENT == "-" || a.TRACKING_SHIPMENT == "") && a.CUST == cust && (a.NO_REFERENSI.Contains("INV")) && a.STATUS_TRANSAKSI.Contains("03") && a.TGL >= getHMin2).ToList();
            //end change by nurul 6/12/2021
            //try
            //{
            if (list_ordersn.Count() > 0)
            {
                foreach (var pesanan in list_ordersn)
                {
                    string[] splitNoRef = pesanan.NO_REFERENSI.Split(';');
                    string urll = "https://fs.tokopedia.net/v2/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/order?invoice_num=" + Uri.EscapeDataString(splitNoRef.Last());
                    long milis = CurrentTimeMillis();


                    DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "GET";
                    myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
                    myReq.Accept = "application/x-www-form-urlencoded";
                    myReq.ContentType = "application/json";
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
                    //using (WebResponse response = await myReq.GetResponse())
                    //    {
                    //        using (Stream stream = response.GetResponseStream())
                    //        {
                    //            StreamReader reader = new StreamReader(stream);
                    //            responseFromServer = reader.ReadToEnd();
                    //        }
                    //    }
                    //}
                    //catch (WebException e)
                    //{
                    //    string err = "";
                    //    if (e.Status == WebExceptionStatus.ProtocolError)
                    //    {
                    //        WebResponse resp = e.Response;
                    //        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    //        {
                    //            err = sr.ReadToEnd();
                    //        }
                    //    }
                    //    throw new Exception(err);
                    //}

                    //if (responseFromServer != null)
                    if (!string.IsNullOrEmpty(responseFromServer))
                    {
                        TokpedSingleOrderResult result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokpedSingleOrderResult)) as TokpedSingleOrderResult;
                        //if (result.header.error_code == "")
                        {
                            var tempAWB = result.data.order_info.shipping_info.awb;
                            if (tempAWB != null && tempAWB != "")
                            {
                                var pesananIndb = ErasoftDbContext.SOT01A.AsNoTracking().Where(a => a.NO_BUKTI == pesanan.NO_BUKTI).SingleOrDefault();
                                if (pesananIndb != null)
                                {
                                    ret = ret + tempAWB;
                                    //pesananIndb.TRACKING_SHIPMENT = tempAWB;
                                    ErasoftDbContext.Database.ExecuteSqlCommand("UPDATE SOT01A SET TRACKING_SHIPMENT = '" + tempAWB + "' where NO_BUKTI='" + pesanan.NO_BUKTI + "'");
                                    ErasoftDbContext.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
            //}catch(Exception ex)
            //{
            //    if(!ex.Message.ToLower().Contains("unauthorized"))
            //        if (!ex.Message.ToLower().Contains("many") && !ex.Message.ToLower().Contains("request"))
            //    {
            //        var log = new TABEL_LOG_GETORDERS()
            //        {
            //            DBPATHERA = iden.DatabasePathErasoft,
            //            MARKETPLACE = "TOKPED",
            //            TGL = DateTime.UtcNow.AddHours(7),
            //            FUNCTION = "GetSingleOrder(get kode booking) : " + cust,
            //            ERRORMSG = ex.Message
            //        };
            //        MoDbContext.TABEL_LOG_GETORDERS.Add(log);
            //        MoDbContext.SaveChanges();
            //    }
            //    throw ex;
            //}
            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        public async Task<string> GetNoAWB(TokopediaAPIData iden, string ordNo, string noref)
        {
            string ret = "";
            var token = SetupContext(iden);
            iden.token = token;
            var getPesanan = ErasoftDbContext.SOT01A.Where(a => a.NO_BUKTI == ordNo && a.NO_REFERENSI.Contains(noref)).FirstOrDefault();
            if (getPesanan != null)
            {
                string[] splitNoRef = getPesanan.NO_REFERENSI.Split(';');
                string urll = "https://fs.tokopedia.net/v2/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/order?invoice_num=" + Uri.EscapeDataString(splitNoRef.Last());
                long milis = CurrentTimeMillis();


                DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
                myReq.Accept = "application/x-www-form-urlencoded";
                myReq.ContentType = "application/json";
                string responseFromServer = "";
                try
                {
                    using (WebResponse response = await myReq.GetResponseAsync())
                    //using (WebResponse response = myReq.GetResponse())
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(stream);
                            responseFromServer = reader.ReadToEnd();
                        }
                    }
                    //using (WebResponse response = myReq.GetResponse())
                    //{
                    //    using (Stream stream = response.GetResponseStream())
                    //    {
                    //        StreamReader reader = new StreamReader(stream);
                    //        responseFromServer = reader.ReadToEnd();
                    //    }
                    //}
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

                //if (responseFromServer != null)
                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    TokpedSingleOrderResult result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokpedSingleOrderResult)) as TokpedSingleOrderResult;
                    //if (result.header.error_code == "")
                    {
                        var tempAWB = result.data.order_info.shipping_info.awb;
                        if (tempAWB != null && tempAWB != "")
                        {
                            //var pesananIndb = ErasoftDbContext.SOT01A.Where(a => a.NO_BUKTI == ordNo).SingleOrDefault();
                            if (getPesanan != null)
                            {
                                ret = ret + tempAWB;
                                getPesanan.TRACKING_SHIPMENT = tempAWB;
                                ErasoftDbContext.SaveChanges();
                            }
                        }
                    }
                }
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        public async Task<string> JOBCOD(TokopediaAPIData iden, string ordNo, string noref)
        {
            string ret = "";
            //string connId = Guid.NewGuid().ToString();
            var token = SetupContext(iden);
            iden.token = token;
            string[] splitNoRef = noref.Split(';');
            string urll = "https://fs.tokopedia.net/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/fulfillment_order?shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&order_id=" + Uri.EscapeDataString(splitNoRef[0]);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";


            string responseFromServer = "";
            //try
            //{
            using (WebResponse response = myReq.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseFromServer = reader.ReadToEnd();
                }
            }
            //}
            //catch (WebException e)
            //{
            //    string err = "";
            //    if (e.Status == WebExceptionStatus.ProtocolError)
            //    {
            //        WebResponse resp = e.Response;
            //        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
            //        {
            //            err = sr.ReadToEnd();
            //        }
            //    }
            //}
            
            //if (responseFromServer != null)
            if (!string.IsNullOrEmpty(responseFromServer))
            {
                JOBCODResult result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(JOBCODResult)) as JOBCODResult;
                var pesananIndb = ErasoftDbContext.SOT01A.Where(a => a.NO_BUKTI == ordNo).SingleOrDefault();
                if (pesananIndb != null)
                {
                    ret = result.data.order_data.Where(a => a.order.invoice_number == splitNoRef.Last()).Select(a => a.booking_data.booking_code).FirstOrDefault();
                    //if (result.status == "200")
                    //{
                    //    //var pesananIndb = ErasoftDbContext.SOT01A.Where(a => a.NO_BUKTI == ordNo).SingleOrDefault();
                    //    //if (pesananIndb != null)
                    //    //{
                    //      ret = result.data.order_data.Where(a => a.order.invoice_number == splitNoRef.Last()).Select(a => a.booking_data.booking_code).FirstOrDefault();
                    //    if (ret != "" && ret != null)
                    //    {
                    //        //EDB.ExecuteSQL("sConn", CommandType.Text, "UPDATE SOT01A SET STATUS_KIRIM='2' WHERE NO_BUKTI = '" + NO_BUKTI_SOT01A + "'");
                    //        pesananIndb.status_kirim = "2";
                    //        pesananIndb.NO_PO_CUST = ret;
                    //        ErasoftDbContext.SaveChanges();
                    //        //} else if (pesananIndb.STATUS_TRANSAKSI == "02" && (pesananIndb.SHIPMENT.Contains("SiCepat") || pesananIndb.SHIPMENT.Contains("AnterAja") || pesananIndb.SHIPMENT.Contains("J&T") || pesananIndb.SHIPMENT.Contains("JNE") || pesananIndb.SHIPMENT.Contains("Lion")))
                    //    }
                    //    //}
                    //}
                    if (ret != "" && ret != null)
                    {
                        pesananIndb.status_kirim = "2";
                        pesananIndb.NO_PO_CUST = ret;
                        ErasoftDbContext.SaveChanges();
                    }
                }
            }
            return ret;
        }
        
        public async Task<string> JOBCOD1(TokopediaAPIData iden, string ordNo, string noref)
        {
            string ret = "";
            //string connId = Guid.NewGuid().ToString();
            var token = SetupContext(iden);
            iden.token = token;
            string[] splitNoRef = noref.Split(';');
            string urll = "https://fs.tokopedia.net/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/fulfillment_order?shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&order_id=" + Uri.EscapeDataString(splitNoRef[0]);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";


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
            
            //if (responseFromServer != null)
            if (!string.IsNullOrEmpty(responseFromServer))
            {
                JOBCODResult result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(JOBCODResult)) as JOBCODResult;
                var pesananIndb = ErasoftDbContext.SOT01A.Where(a => a.NO_BUKTI == ordNo).SingleOrDefault();
                if (pesananIndb != null)
                {
                    ret = result.data.order_data.Where(a => a.order.invoice_number == splitNoRef.Last()).Select(a => a.booking_data.booking_code).FirstOrDefault();
                    if (ret != "" && ret != null)
                    {
                        pesananIndb.status_kirim = "2";
                        pesananIndb.NO_PO_CUST = ret;
                        ErasoftDbContext.SaveChanges();
                    }
                }
            }
            return ret;
        }
        //end add by nurul 19/3/2020, untuk get kode booking 

        //add by nurul 23/7/2021
        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        public async Task<string> JOBCOD_2(string dbPathEra, string nobuk, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, string ordNo, string noref)
        {
            string ret = "";
            //string connId = Guid.NewGuid().ToString();
            var token = SetupContext(iden);
            iden.token = token;
            string[] splitNoRef = noref.Split(';');
            string urll = "https://fs.tokopedia.net/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/fulfillment_order?shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&order_id=" + Uri.EscapeDataString(splitNoRef[0]);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";


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
            
            //if (responseFromServer != null)
            if (!string.IsNullOrEmpty(responseFromServer))
            {
                JOBCODResult result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(JOBCODResult)) as JOBCODResult;
                var pesananIndb = ErasoftDbContext.SOT01A.Where(a => a.NO_BUKTI == ordNo).SingleOrDefault();
                if (pesananIndb != null)
                {
                    ret = result.data.order_data.Where(a => a.order.invoice_number == splitNoRef.Last()).Select(a => a.booking_data.booking_code).FirstOrDefault();
                    if (ret != "" && ret != null)
                    {
                        pesananIndb.status_kirim = "2";
                        pesananIndb.NO_PO_CUST = ret;
                        ErasoftDbContext.SaveChanges();
                    }
                }
            }
            return ret;
        }
        //end add by nurul 23/7/2021

        //add by nurul 1/4/2020
        public async Task<string> PrintLabel(TokopediaAPIData iden, string ordNo, string noref)
        {
            string ret = "";
            var token = SetupContext(iden);
            iden.token = token;

            string[] splitNoRef = noref.Split(';');
            //change by nurul 25/5/2021
            //string urll = "https://fs.tokopedia.net/v1/order/" + Uri.EscapeDataString(splitNoRef[0]) + "/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/shipping-label?printed=0";
            string urll = "https://fs.tokopedia.net/v1/order/" + Uri.EscapeDataString(splitNoRef[0]) + "/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/shipping-label?printed=1";
            //end change by nurul 25/5/2021
            long milis = CurrentTimeMillis();


            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            //try
            //{
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
            catch (WebException e)
            {
                string err = "";
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        //err = sr.ReadToEnd();
                        responseFromServer = sr.ReadToEnd();
                    }
                }
                //throw new Exception(err);
            }
            //using (WebResponse response = myReq.GetResponse())
            //{
            //    using (Stream stream = response.GetResponseStream())
            //    {
            //        StreamReader reader = new StreamReader(stream);
            //        responseFromServer = reader.ReadToEnd();
            //    }
            //}
            //}
            //catch (WebException e)
            //{
            //    string err = "";
            //    if (e.Status == WebExceptionStatus.ProtocolError)
            //    {
            //        WebResponse resp = e.Response;
            //        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
            //        {
            //            err = sr.ReadToEnd();
            //        }
            //    }
            //    //throw new Exception(err);
            //}

            //if (responseFromServer != null)
            if (!string.IsNullOrEmpty(responseFromServer))
            {
                try
                {
                    TokpedPrintLabel result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokpedPrintLabel)) as TokpedPrintLabel;
                    if (result.header.messages != "" || result.header.reason != "")
                    {
                        ret = "Error: " + result.header.messages + " " + result.header.reason;
                    }
                }
                catch
                {
                    ret = responseFromServer;
                }
            }

            return ret;
        }
        //end add by nurul 1/4/2020

        public async Task<string> GetOrderList3days(TokopediaAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, long daysFrom, long daysTo, string ord_stat)
        {
            //if merchant code diisi. barulah GetOrderList
            string ret = "";
            string connId = Guid.NewGuid().ToString();
            var token = SetupContext(iden);
            iden.token = token;
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";

            //add by nurul 20/1/2021, bundling
            //ret = connId;
            ret += "'" + connId + "' , ";
            //end add by nurul 20/1/2021, bundling

            //complete list of order status at https://fs.tokopedia.net/docs#order-status-codes
            //400 seller accepted the order
            //401 seller accepted the order, partially
            //10  seller rejected the order
            //500 seller confirm for shipment

            switch (stat)
            {
                case StatusOrder.Cancel:
                    //Cancel Order
                    status = "0";
                    break;
                case StatusOrder.Paid:
                    //paid
                    status = "220";
                    break;
                //case StatusOrder.PackagingINP:
                //    status = "500";
                //    break;
                case StatusOrder.ShippingINP:
                    //Shipping in Progress
                    status = "500";
                    break;
                case StatusOrder.Completed:
                    //Completed (Shipping)
                    status = "610";
                    break;
                default:
                    break;
            }
            //change 16 des 2020, fixed date
            //long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(daysFrom).ToUnixTimeSeconds();
            //long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(daysTo).ToUnixTimeSeconds();
            long unixTimestampFrom = daysFrom;
            long unixTimestampTo = daysTo;
            //end change 16 des 2020, fixed date
            string urll = "https://fs.tokopedia.net/v2/order/list?fs_id=" + Uri.EscapeDataString(iden.merchant_code) + "&from_date=" 
                + Convert.ToString(unixTimestampFrom) + "&to_date=" + Convert.ToString(unixTimestampTo) + "&page=" + Convert.ToString(page) 
                + "&per_page=100&shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&encrypt=1";
            if (!string.IsNullOrEmpty(ord_stat))
            {
                urll += "&status=" + ord_stat;
            }
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
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            //myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            //myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            //myReq.Headers.Add("requestId", milis.ToString());
            //myReq.Headers.Add("sessionId", milis.ToString());
            //myReq.Headers.Add("username", userMTA);
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
            //}
            //catch (Exception ex)
            //{
            //    //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}
            int rowCount = 0;
            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                TokopediaOrders result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaOrders)) as TokopediaOrders;
                if (result.data != null)
                {
                    if (!string.IsNullOrEmpty(ord_stat))
                    {
                        result.data = result.data.Where(p => p.fulfill_by == 0).ToArray();
                        if(result.data.Length == 0)
                        {
                            return ret;
                        }
                    }
                    var orderPaid = result.data.Where(p => p.order_status == 220).ToList();
                    var orderAccepted = result.data.Where(p => p.order_status == 400).ToList();
                    //add by Tri 17 mar 2020, insert pesanan dengan status 450
                    var orderWaitingPickUp = result.data.Where(p => p.order_status == 450).ToList();
                    if (orderWaitingPickUp != null)
                    {
                        if (orderWaitingPickUp.Count > 0)
                            orderAccepted.AddRange(orderWaitingPickUp);

                    }
                    //end add by Tri 17 mar 2020, insert pesanan dengan status 450
                    var orderTokpedInDb = ErasoftDbContext.TEMP_TOKPED_ORDERS.Where(p => p.fs_id == iden.merchant_code);

                    //var last21days = DateTimeOffset.UtcNow.AddHours(7).AddDays(-21).DateTime;
                    //System.DateTime datetimeisnull = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    //var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST && (p.TGL ?? datetimeisnull) > last21days).Select(p => p.NO_REFERENSI).ToList();

                    //var last21days = DateTimeOffset.UtcNow.AddHours(7).AddDays(daysFrom - 1).DateTime;
                    //var last21days2 = DateTimeOffset.UtcNow.AddHours(7).AddDays(daysTo + 1).DateTime;
                    var last21days = DateTimeOffset.FromUnixTimeMilliseconds(daysFrom * 1000).UtcDateTime.AddHours(7).AddDays(-1);
                    var last21days2 = DateTimeOffset.FromUnixTimeMilliseconds(daysTo * 1000).UtcDateTime.AddHours(7).AddDays(1);
                    System.DateTime datetimeisnull = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST && (p.TGL ?? datetimeisnull) >= last21days && (p.TGL ?? datetimeisnull) <= last21days2).Select(p => p.NO_REFERENSI).ToList();

                    var connIdARF01C = Guid.NewGuid().ToString();
                    rowCount = result.data.Count();

                    var decryptF = new EncryptTokped();//add 28 NOv 2020, decrypt tokped
                    if (orderPaid != null)
                    {
                        foreach (var order in orderPaid)
                        {
                            if (!OrderNoInDb.Contains(order.order_id + ";" + order.invoice_ref_num))
                            {
                                //add 28 NOv 2020, decrypt tokped
                                if (order.encryption != null)
                                {
                                    if (!string.IsNullOrEmpty(order.encryption.secret) && !string.IsNullOrEmpty(order.encryption.content))
                                    {
                                        var decryptedOrder = decryptF.DecryptOrderTokped(order.encryption.secret, order.encryption.content);
                                        if (!string.IsNullOrEmpty(decryptedOrder))
                                        {
                                            DecryptedOrderList decryptedOrderData = Newtonsoft.Json.JsonConvert.DeserializeObject(decryptedOrder, typeof(DecryptedOrderList)) as DecryptedOrderList;
                                            if (!string.IsNullOrEmpty(decryptedOrderData.buyer.name))
                                                order.buyer.name = decryptedOrderData.buyer.name;
                                            if (!string.IsNullOrEmpty(decryptedOrderData.buyer.phone))
                                                order.buyer.phone = decryptedOrderData.buyer.phone;
                                            if (!string.IsNullOrEmpty(decryptedOrderData.recipient.name))
                                                order.recipient.name = decryptedOrderData.recipient.name;
                                            if (!string.IsNullOrEmpty(decryptedOrderData.recipient.phone))
                                                order.recipient.phone = decryptedOrderData.recipient.phone;
                                            if (!string.IsNullOrEmpty(decryptedOrderData.recipient.address.address_full))
                                                order.recipient.address.address_full = decryptedOrderData.recipient.address.address_full;
                                        }
                                    }
                                }
                                //end add 28 NOv 2020, decrypt tokped

                                List<TEMP_TOKPED_ORDERS> ListNewOrders = new List<TEMP_TOKPED_ORDERS>();
                                ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_TOKPED_ORDERS");

                                string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                                var kabKot = "3174";
                                var prov = "31";
                                var nama = order.recipient.name.Replace("'", "`");
                                if (nama.Length > 30)
                                    nama = nama.Substring(0, 30);
                                string TLP = !string.IsNullOrEmpty(order.recipient.phone) ? order.recipient.phone : "";
                                if (TLP.Length > 30)
                                    TLP = TLP.Substring(0, 30);
                                if (NAMA_CUST.Length > 30)
                                    NAMA_CUST = NAMA_CUST.Substring(0, 30);
                                string AL_KIRIM1 = !string.IsNullOrEmpty(order.recipient.address.address_full) ? order.recipient.address.address_full.Replace('\'', '`') : "";
                                if (AL_KIRIM1.Length > 30)
                                {
                                    AL_KIRIM1 = AL_KIRIM1.Substring(0, 30);
                                }
                                string KODEPOS = !string.IsNullOrEmpty(order.recipient.address.postal_code) ? order.recipient.address.postal_code.Replace('\'', '`') : "";
                                if (KODEPOS.Length > 7)
                                {
                                    KODEPOS = KODEPOS.Substring(0, 7);
                                }
                                //insertPembeli += "('" + order.recipient.name.Replace("'", "`") + "','" + order.recipient.address.address_full.Replace("'", "`") + "','" + order.recipient.phone + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
                                insertPembeli += "('" + nama + "','" + order.recipient.address.address_full.Replace("'", "`") + "','" + TLP + "','" + NAMA_CUST + "',0,0,'0','01',";
                                insertPembeli += "1, 'IDR', '01', '" + AL_KIRIM1 + "', 0, 0, 0, 0, '1', 0, 0, ";
                                insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + KODEPOS + "', '', '" + kabKot + "', '" + prov + "', '', '','" + connIdARF01C + "'),";

                                var order_order_id = Convert.ToString(order.order_id);
                                #region cut max length dan ubah '
                                string fs_id = !string.IsNullOrEmpty(order.fs_id) ? order.fs_id.Replace('\'', '`') : "";
                                if (fs_id.Length > 50)
                                {
                                    fs_id = fs_id.Substring(0, 50);
                                }
                                string order_id = !string.IsNullOrEmpty(Convert.ToString(order.order_id)) ? Convert.ToString(order.order_id).Replace('\'', '`') : "";
                                if (order_id.Length > 50)
                                {
                                    order_id = order_id.Substring(0, 50);
                                }
                                string invoice_ref_num = !string.IsNullOrEmpty(order.invoice_ref_num) ? order.invoice_ref_num.Replace('\'', '`') : "";
                                if (invoice_ref_num.Length > 50)
                                {
                                    invoice_ref_num = invoice_ref_num.Substring(0, 50);
                                }
                                string device_type = !string.IsNullOrEmpty(order.device_type) ? order.device_type.Replace('\'', '`') : "";
                                if (device_type.Length > 50)
                                {
                                    device_type = device_type.Substring(0, 50);
                                }
                                string buyer_name = !string.IsNullOrEmpty(order.buyer.name) ? order.buyer.name.Replace('\'', '`') : "";
                                if (buyer_name.Length > 250)
                                {
                                    buyer_name = buyer_name.Substring(0, 250);
                                }
                                string buyer_phone = !string.IsNullOrEmpty(order.buyer.phone) ? order.buyer.phone.Replace('\'', '`') : "";
                                if (buyer_phone.Length > 50)
                                {
                                    buyer_phone = buyer_phone.Substring(0, 50);
                                }
                                string buyer_email = !string.IsNullOrEmpty(order.buyer.email) ? order.buyer.email.Replace('\'', '`') : "";
                                if (buyer_email.Length > 250)
                                {
                                    buyer_email = buyer_email.Substring(0, 250);
                                }
                                string recipient_name = !string.IsNullOrEmpty(order.recipient.name) ? order.recipient.name.Replace('\'', '`') : "";
                                if (recipient_name.Length > 250)
                                {
                                    recipient_name = recipient_name.Substring(0, 250);
                                }
                                string recipient_address_district = !string.IsNullOrEmpty(order.recipient.address.district) ? order.recipient.address.district.Replace('\'', '`') : "";
                                if (recipient_address_district.Length > 150)
                                {
                                    recipient_address_district = recipient_address_district.Substring(0, 150);
                                }
                                string recipient_address_city = !string.IsNullOrEmpty(order.recipient.address.city) ? order.recipient.address.city.Replace('\'', '`') : "";
                                if (recipient_address_city.Length > 150)
                                {
                                    recipient_address_city = recipient_address_city.Substring(0, 150);
                                }
                                string recipient_address_province = !string.IsNullOrEmpty(order.recipient.address.province) ? order.recipient.address.province.Replace('\'', '`') : "";
                                if (recipient_address_province.Length > 150)
                                {
                                    recipient_address_province = recipient_address_province.Substring(0, 150);
                                }
                                string recipient_address_country = !string.IsNullOrEmpty(order.recipient.address.country) ? order.recipient.address.country.Replace('\'', '`') : "";
                                if (recipient_address_country.Length > 150)
                                {
                                    recipient_address_country = recipient_address_country.Substring(0, 150);
                                }
                                string recipient_address_postal_code = !string.IsNullOrEmpty(order.recipient.address.postal_code) ? order.recipient.address.postal_code.Replace('\'', '`') : "";
                                if (recipient_address_postal_code.Length > 10)
                                {
                                    recipient_address_postal_code = recipient_address_postal_code.Substring(0, 10);
                                }
                                //string recipient_address_geo = !string.IsNullOrEmpty(order.recipient.address.geo) ? order.recipient.address.geo.Replace('\'', '`') : "";
                                //if (recipient_address_geo.Length > 150)
                                //{
                                //    recipient_address_geo = recipient_address_geo.Substring(0, 150);
                                //}
                                string recipient_address_geo = "";
                                string recipient_phone = !string.IsNullOrEmpty(order.recipient.phone) ? order.recipient.phone.Replace('\'', '`') : "";
                                if (recipient_phone.Length > 50)
                                {
                                    recipient_phone = recipient_phone.Substring(0, 50);
                                }
                                string logistics_shipping_agency = !string.IsNullOrEmpty(order.logistics.shipping_agency) ? order.logistics.shipping_agency.Replace('\'', '`') : "";
                                if (logistics_shipping_agency.Length > 150)
                                {
                                    logistics_shipping_agency = logistics_shipping_agency.Substring(0, 150);
                                }
                                string logistics_service_type = !string.IsNullOrEmpty(order.logistics.service_type) ? order.logistics.service_type.Replace('\'', '`') : "";
                                if (logistics_service_type.Length > 150)
                                {
                                    logistics_service_type = logistics_service_type.Substring(0, 150);
                                }
                                string dropshipper_info_name = !string.IsNullOrEmpty(order.dropshipper_info.name) ? order.dropshipper_info.name.Replace('\'', '`') : "";
                                if (dropshipper_info_name.Length > 250)
                                {
                                    dropshipper_info_name = dropshipper_info_name.Substring(0, 250);
                                }
                                string dropshipper_info_phone = !string.IsNullOrEmpty(order.dropshipper_info.phone) ? order.dropshipper_info.phone.Replace('\'', '`') : "";
                                if (dropshipper_info_phone.Length > 50)
                                {
                                    dropshipper_info_phone = dropshipper_info_phone.Substring(0, 50);
                                }
                                string voucher_info_voucher_code = !string.IsNullOrEmpty(order.voucher_info.voucher_code) ? order.voucher_info.voucher_code.Replace('\'', '`') : "";
                                if (voucher_info_voucher_code.Length > 150)
                                {
                                    voucher_info_voucher_code = voucher_info_voucher_code.Substring(0, 150);
                                }
                                string custom_fields_awb = !string.IsNullOrEmpty(order.custom_fields.awb) ? order.custom_fields.awb.Replace('\'', '`') : "";
                                if (custom_fields_awb.Length > 150)
                                {
                                    custom_fields_awb = custom_fields_awb.Substring(0, 150);
                                }
                                #endregion
                                var skipInsert = false;
                                if (orderTokpedInDb.Where(p => p.order_id == order_order_id).Count() == 0)
                                {
                                    DateTime? expiredDate = null;
                                    DateTime? paymentDate = null;
                                    long? warehouse_id = 0;
                                    //remark 13 nov 2020 tutup sementara
                                    var orderDetail = await GetOrderDetail(iden, order.invoice_ref_num);
                                    if (orderDetail != null)
                                    {
                                        if (orderDetail.data != null)
                                        {
                                            if (orderDetail.data.shipment_fulfillment != null)
                                            {
                                                expiredDate = orderDetail.data.shipment_fulfillment.confirm_shipping_deadline;
                                            }
                                            paymentDate = orderDetail.data.payment_date;
                                            if(orderDetail.data.payment_info != null)
                                            {
                                                var payment_info = !string.IsNullOrEmpty(orderDetail.data.payment_info.gateway_name) ? orderDetail.data.payment_info.gateway_name.Replace('\'', '`') : "";
                                                if (payment_info.Length > 50)
                                                {
                                                    payment_info = payment_info.Substring(0, 50);
                                                }
                                                device_type = payment_info;
                                            }
                                            if(orderDetail.data.preorder != null)
                                            {
                                                if (orderDetail.data.preorder.order_id.HasValue)
                                                {
                                                    if (orderDetail.data.preorder.order_id.Value > 0)
                                                    {
                                                        recipient_address_geo = "Preorder";
                                                    }
                                                    else
                                                    {
                                                        recipient_address_geo = "";
                                                    }
                                                }
                                            }
                                            //add by nurul 23/11/2021
                                            if (orderDetail.data.order_warehouse != null)
                                            {
                                                warehouse_id = orderDetail.data.order_warehouse.warehouse_id;
                                            }
                                            //end add by nurul 23/11/2021
                                        }
                                    }
                                    //belum ada di temp
                                    //end remark 13 nov 2020 tutup sementara
                                    var listBRG = new List<string>();
                                    int indexProduct = 0;
                                    foreach (var product in order.products)
                                    {
                                        indexProduct++;
                                        if (!listBRG.Contains(product.id.ToString()))
                                        {
                                            listBRG.Add(product.id.ToString());
                                        }
                                        else
                                        {
                                            //ada barang yg duplikat
                                            skipInsert = true;
                                            //break; //remark 15 des 2021, handle brg double dengan sp baru
                                        }
                                        #region cut max length dan ubah '
                                        string currency = !string.IsNullOrEmpty(product.currency) ? product.currency.Replace('\'', '`') : "";
                                        if (currency.Length > 50)
                                        {
                                            currency = currency.Substring(0, 50);
                                        }
                                        string product_sku = !string.IsNullOrEmpty(product.sku) ? product.sku.Replace('\'', '`') : "";
                                        if (product_sku.Length > 50)
                                        {
                                            product_sku = product_sku.Substring(0, 50);
                                        }
                                        #endregion
                                        TEMP_TOKPED_ORDERS newOrder = new TEMP_TOKPED_ORDERS()
                                        {
                                            fs_id = fs_id,
                                            order_id = order_id,
                                            accept_partial = order.accept_partial,
                                            invoice_ref_num = invoice_ref_num,
                                            product_id = product.id,
                                            product_name = string.IsNullOrEmpty(product.name) ? "" : product.name.Replace("'", "`"),
                                            product_quantity = product.quantity,
                                            product_notes = string.IsNullOrEmpty(product.notes) ? "" : product.notes.Replace("'", "`"),
                                            product_weight = product.weight,
                                            product_total_weight = product.total_weight,
                                            product_price = product.price,
                                            product_total_price = product.total_price,
                                            product_currency = currency,
                                            product_sku = product_sku,
                                            products_fulfilled_product_id = 0,
                                            products_fulfilled_quantity_deliver = 0,
                                            products_fulfilled_quantity_reject = 0,
                                            device_type = device_type,
                                            buyer_id = order.buyer.id,
                                            buyer_name = buyer_name,
                                            buyer_email = buyer_email,
                                            buyer_phone = buyer_phone,
                                            shop_id = order.shop_id,
                                            payment_id = order.payment_id,
                                            //recipient_name = order.recipient.name,
                                            recipient_name = nama,
                                            recipient_address_address_full = string.IsNullOrEmpty(order.recipient.address.address_full) ? "" : order.recipient.address.address_full.Replace("'", "`"),
                                            recipient_address_district = recipient_address_district,
                                            recipient_address_district_id = order.recipient.address.district_id,
                                            recipient_address_city = recipient_address_city,
                                            recipient_address_city_id = order.recipient.address.city_id,
                                            recipient_address_province = recipient_address_province,
                                            recipient_address_province_id = order.recipient.address.province_id,
                                            recipient_address_country = recipient_address_country,
                                            recipient_address_geo = recipient_address_geo,
                                            recipient_address_postal_code = recipient_address_postal_code,
                                            recipient_phone = recipient_phone,
                                            logistics_shipping_id = order.logistics.shipping_id,
                                            logistics_shipping_agency = logistics_shipping_agency,
                                            logistics_service_type = logistics_service_type,
                                            amt_ttl_product_price = order.amt.ttl_product_price,
                                            amt_shipping_cost = order.amt.shipping_cost,
                                            amt_insurance_cost = order.amt.insurance_cost,
                                            amt_ttl_amount = order.amt.ttl_amount,
                                            amt_voucher_amount = order.amt.voucher_amount,
                                            amt_toppoints_amount = order.amt.toppoints_amount,
                                            dropshipper_info_name = dropshipper_info_name,
                                            dropshipper_info_phone = dropshipper_info_phone,
                                            voucher_info_voucher_code = voucher_info_voucher_code,
                                            voucher_info_voucher_type = order.voucher_info.voucher_type,
                                            order_status = order.order_status,
                                            create_time = DateTimeOffset.FromUnixTimeSeconds(order.create_time).UtcDateTime,
                                            custom_fields_awb = custom_fields_awb,
                                            conn_id = connId,
                                            CUST = CUST,
                                            NAMA_CUST = NAMA_CUST
                                        };
                                        var product_fulfilled = order.products_fulfilled.Where(p => p.product_id == product.id).FirstOrDefault();
                                        if (product_fulfilled != null)
                                        {
                                            newOrder.products_fulfilled_product_id = product_fulfilled.product_id;
                                            newOrder.products_fulfilled_quantity_deliver = product_fulfilled.quantity_deliver;
                                            newOrder.products_fulfilled_quantity_reject = product_fulfilled.quantity_reject;
                                        }
                                        newOrder.product_currency = product.id + "_" + indexProduct;
                                        newOrder.confirm_shipping_deadline = expiredDate;
                                        if (paymentDate != null)
                                        {
                                            newOrder.create_time = paymentDate.Value;
                                        }
                                        //add by nurul 23/11/2021
                                        if (warehouse_id != 0 && warehouse_id != null)
                                        {
                                            newOrder.warehouse_id = warehouse_id;
                                        }
                                        //end add by nurul 23/11/2021
                                        ListNewOrders.Add(newOrder);
                                    }
                                    //if (skipInsert)
                                    //{
                                    //    continue;
                                    //}
                                }

                                insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
                                EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);

                                ErasoftDbContext.TEMP_TOKPED_ORDERS.AddRange(ListNewOrders);
                                ErasoftDbContext.SaveChanges();

                                using (SqlCommand CommandSQL = new SqlCommand())
                                {
                                    //call sp to insert buyer data
                                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIdARF01C;

                                    EDB.ExecuteSQL("Con", "MoveARF01CFromTempTable", CommandSQL);
                                };
                                if (skipInsert)
                                {
                                    using (SqlCommand CommandSQL = new SqlCommand())
                                    {
                                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connId;
                                        CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                                        CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        CommandSQL.Parameters.Add("@MARKET", SqlDbType.VarChar).Value = "TOKPED";
                                        CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                                        EDB.ExecuteSQL("Con", "MoveOrderFromTempTableNew", CommandSQL);
                                        jmlhNewOrder++;
                                    }
                                }
                                else
                                {
                                    using (SqlCommand CommandSQL = new SqlCommand())
                                    {
                                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connId;
                                        CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                                        CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 1;
                                        CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                                        EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
                                        jmlhNewOrder++;
                                    }
                                }
                            }
                        }
                    }

                    if (orderAccepted != null)
                    {
                        foreach (var order in orderAccepted)
                        {
                            if (!OrderNoInDb.Contains(order.order_id + ";" + order.invoice_ref_num))
                            {
                                //add 28 NOv 2020, decrypt tokped
                                if (order.encryption != null)
                                {
                                    if (!string.IsNullOrEmpty(order.encryption.secret) && !string.IsNullOrEmpty(order.encryption.content))
                                    {
                                        var decryptedOrder = decryptF.DecryptOrderTokped(order.encryption.secret, order.encryption.content);
                                        if (!string.IsNullOrEmpty(decryptedOrder))
                                        {
                                            DecryptedOrderList decryptedOrderData = Newtonsoft.Json.JsonConvert.DeserializeObject(decryptedOrder, typeof(DecryptedOrderList)) as DecryptedOrderList;
                                            if (!string.IsNullOrEmpty(decryptedOrderData.buyer.name))
                                                order.buyer.name = decryptedOrderData.buyer.name;
                                            if (!string.IsNullOrEmpty(decryptedOrderData.buyer.phone))
                                                order.buyer.phone = decryptedOrderData.buyer.phone;
                                            if (!string.IsNullOrEmpty(decryptedOrderData.recipient.name))
                                                order.recipient.name = decryptedOrderData.recipient.name;
                                            if (!string.IsNullOrEmpty(decryptedOrderData.recipient.phone))
                                                order.recipient.phone = decryptedOrderData.recipient.phone;
                                            if (!string.IsNullOrEmpty(decryptedOrderData.recipient.address.address_full))
                                                order.recipient.address.address_full = decryptedOrderData.recipient.address.address_full;
                                        }
                                    }
                                }
                                //end add 28 NOv 2020, decrypt tokped

                                DateTime? expiredDate = null;
                                DateTime? paymentDate = null;
                                long? warehouse_id = 0;
                                //remark 13 nov 2020 tutup sementara
                                //var orderDetail = await GetOrderDetail(iden, order.invoice_ref_num);
                                //if (orderDetail != null)
                                //{
                                //    if (orderDetail.data != null)
                                //    {
                                //        if (orderDetail.data.shipment_fulfillment != null)
                                //        {
                                //            expiredDate = orderDetail.data.shipment_fulfillment.confirm_shipping_deadline;
                                //        }
                                //        paymentDate = orderDetail.data.payment_date;
                                //    }
                                //}
                                //end remark 13 nov 2020 tutup sementara
                                List<TEMP_TOKPED_ORDERS> ListNewOrders = new List<TEMP_TOKPED_ORDERS>();

                                ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_TOKPED_ORDERS");

                                var nama2 = order.recipient.name.Replace("'", "`");
                                if (nama2.Length > 30)
                                    nama2 = nama2.Substring(0, 30);
                                string aTLP = !string.IsNullOrEmpty(order.recipient.phone) ? order.recipient.phone : "";
                                if (aTLP.Length > 30)
                                    aTLP = aTLP.Substring(0, 30);
                                if (NAMA_CUST.Length > 30)
                                    NAMA_CUST = NAMA_CUST.Substring(0, 30);
                                string aAL_KIRIM1 = !string.IsNullOrEmpty(order.recipient.address.address_full) ? order.recipient.address.address_full.Replace('\'', '`') : "";
                                if (aAL_KIRIM1.Length > 30)
                                {
                                    aAL_KIRIM1 = aAL_KIRIM1.Substring(0, 30);
                                }
                                string aKODEPOS = !string.IsNullOrEmpty(order.recipient.address.postal_code) ? order.recipient.address.postal_code.Replace('\'', '`') : "";
                                if (aKODEPOS.Length > 7)
                                {
                                    aKODEPOS = aKODEPOS.Substring(0, 7);
                                }

                                string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                                var kabKot = "3174";
                                var prov = "31";
                                //insertPembeli += "('" + order.recipient.name + "','" + order.recipient.address.address_full + "','" + order.recipient.phone + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
                                insertPembeli += "('" + nama2 + "','" + order.recipient.address.address_full.Replace("'", "`") + "','" + aTLP + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
                                insertPembeli += "1, 'IDR', '01', '" + aAL_KIRIM1 + "', 0, 0, 0, 0, '1', 0, 0, ";
                                insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + aKODEPOS + "', '', '" + kabKot + "', '" + prov + "', '', '','" + connIdARF01C + "'),";

                                var order_order_id = Convert.ToString(order.order_id);
                                var skipInsert2 = false;
                                if (orderTokpedInDb.Where(p => p.order_id == order_order_id).Count() == 0)
                                {
                                    #region cut max length dan ubah '
                                    string a_fs_id = !string.IsNullOrEmpty(order.fs_id) ? order.fs_id.Replace('\'', '`') : "";
                                    if (a_fs_id.Length > 50)
                                    {
                                        a_fs_id = a_fs_id.Substring(0, 50);
                                    }
                                    string a_order_id = !string.IsNullOrEmpty(Convert.ToString(order.order_id)) ? Convert.ToString(order.order_id).Replace('\'', '`') : "";
                                    if (a_order_id.Length > 50)
                                    {
                                        a_order_id = a_order_id.Substring(0, 50);
                                    }
                                    string a_invoice_ref_num = !string.IsNullOrEmpty(order.invoice_ref_num) ? order.invoice_ref_num.Replace('\'', '`') : "";
                                    if (a_invoice_ref_num.Length > 50)
                                    {
                                        a_invoice_ref_num = a_invoice_ref_num.Substring(0, 50);
                                    }
                                    string a_device_type = !string.IsNullOrEmpty(order.device_type) ? order.device_type.Replace('\'', '`') : "";
                                    if (a_device_type.Length > 50)
                                    {
                                        a_device_type = a_device_type.Substring(0, 50);
                                    }
                                    string a_buyer_name = !string.IsNullOrEmpty(order.buyer.name) ? order.buyer.name.Replace('\'', '`') : "";
                                    if (a_buyer_name.Length > 250)
                                    {
                                        a_buyer_name = a_buyer_name.Substring(0, 250);
                                    }
                                    string a_buyer_phone = !string.IsNullOrEmpty(order.buyer.phone) ? order.buyer.phone.Replace('\'', '`') : "";
                                    if (a_buyer_phone.Length > 50)
                                    {
                                        a_buyer_phone = a_buyer_phone.Substring(0, 50);
                                    }
                                    string a_buyer_email = !string.IsNullOrEmpty(order.buyer.email) ? order.buyer.email.Replace('\'', '`') : "";
                                    if (a_buyer_email.Length > 250)
                                    {
                                        a_buyer_email = a_buyer_email.Substring(0, 250);
                                    }
                                    string a_recipient_name = !string.IsNullOrEmpty(order.recipient.name) ? order.recipient.name.Replace('\'', '`') : "";
                                    if (a_recipient_name.Length > 250)
                                    {
                                        a_recipient_name = a_recipient_name.Substring(0, 250);
                                    }
                                    string a_recipient_address_district = !string.IsNullOrEmpty(order.recipient.address.district) ? order.recipient.address.district.Replace('\'', '`') : "";
                                    if (a_recipient_address_district.Length > 150)
                                    {
                                        a_recipient_address_district = a_recipient_address_district.Substring(0, 150);
                                    }
                                    string a_recipient_address_city = !string.IsNullOrEmpty(order.recipient.address.city) ? order.recipient.address.city.Replace('\'', '`') : "";
                                    if (a_recipient_address_city.Length > 150)
                                    {
                                        a_recipient_address_city = a_recipient_address_city.Substring(0, 150);
                                    }
                                    string a_recipient_address_province = !string.IsNullOrEmpty(order.recipient.address.province) ? order.recipient.address.province.Replace('\'', '`') : "";
                                    if (a_recipient_address_province.Length > 150)
                                    {
                                        a_recipient_address_province = a_recipient_address_province.Substring(0, 150);
                                    }
                                    string a_recipient_address_country = !string.IsNullOrEmpty(order.recipient.address.country) ? order.recipient.address.country.Replace('\'', '`') : "";
                                    if (a_recipient_address_country.Length > 150)
                                    {
                                        a_recipient_address_country = a_recipient_address_country.Substring(0, 150);
                                    }
                                    string a_recipient_address_postal_code = !string.IsNullOrEmpty(order.recipient.address.postal_code) ? order.recipient.address.postal_code.Replace('\'', '`') : "";
                                    if (a_recipient_address_postal_code.Length > 10)
                                    {
                                        a_recipient_address_postal_code = a_recipient_address_postal_code.Substring(0, 10);
                                    }
                                    //string a_recipient_address_geo = !string.IsNullOrEmpty(order.recipient.address.geo) ? order.recipient.address.geo.Replace('\'', '`') : "";
                                    //if (a_recipient_address_geo.Length > 150)
                                    //{
                                    //    a_recipient_address_geo = a_recipient_address_geo.Substring(0, 150);
                                    //}
                                    string a_recipient_address_geo = "";
                                    string a_recipient_phone = !string.IsNullOrEmpty(order.recipient.phone) ? order.recipient.phone.Replace('\'', '`') : "";
                                    if (a_recipient_phone.Length > 50)
                                    {
                                        a_recipient_phone = a_recipient_phone.Substring(0, 50);
                                    }
                                    string a_logistics_shipping_agency = !string.IsNullOrEmpty(order.logistics.shipping_agency) ? order.logistics.shipping_agency.Replace('\'', '`') : "";
                                    if (a_logistics_shipping_agency.Length > 150)
                                    {
                                        a_logistics_shipping_agency = a_logistics_shipping_agency.Substring(0, 150);
                                    }
                                    string a_logistics_service_type = !string.IsNullOrEmpty(order.logistics.service_type) ? order.logistics.service_type.Replace('\'', '`') : "";
                                    if (a_logistics_service_type.Length > 150)
                                    {
                                        a_logistics_service_type = a_logistics_service_type.Substring(0, 150);
                                    }
                                    string a_dropshipper_info_name = !string.IsNullOrEmpty(order.dropshipper_info.name) ? order.dropshipper_info.name.Replace('\'', '`') : "";
                                    if (a_dropshipper_info_name.Length > 250)
                                    {
                                        a_dropshipper_info_name = a_dropshipper_info_name.Substring(0, 250);
                                    }
                                    string a_dropshipper_info_phone = !string.IsNullOrEmpty(order.dropshipper_info.phone) ? order.dropshipper_info.phone.Replace('\'', '`') : "";
                                    if (a_dropshipper_info_phone.Length > 50)
                                    {
                                        a_dropshipper_info_phone = a_dropshipper_info_phone.Substring(0, 50);
                                    }
                                    string a_voucher_info_voucher_code = !string.IsNullOrEmpty(order.voucher_info.voucher_code) ? order.voucher_info.voucher_code.Replace('\'', '`') : "";
                                    if (a_voucher_info_voucher_code.Length > 150)
                                    {
                                        a_voucher_info_voucher_code = a_voucher_info_voucher_code.Substring(0, 150);
                                    }
                                    string a_custom_fields_awb = !string.IsNullOrEmpty(order.custom_fields.awb) ? order.custom_fields.awb.Replace('\'', '`') : "";
                                    if (a_custom_fields_awb.Length > 150)
                                    {
                                        a_custom_fields_awb = a_custom_fields_awb.Substring(0, 150);
                                    }
                                    #endregion
                                    var orderDetail = await GetOrderDetail(iden, order.invoice_ref_num);
                                    if (orderDetail != null)
                                    {
                                        if (orderDetail.data != null)
                                        {
                                            if (orderDetail.data.shipment_fulfillment != null)
                                            {
                                                expiredDate = orderDetail.data.shipment_fulfillment.confirm_shipping_deadline;
                                            }
                                            paymentDate = orderDetail.data.payment_date;
                                            if (orderDetail.data.payment_info != null)
                                            {
                                                var payment_info = !string.IsNullOrEmpty(orderDetail.data.payment_info.gateway_name) ? orderDetail.data.payment_info.gateway_name.Replace('\'', '`') : "";
                                                if (payment_info.Length > 50)
                                                {
                                                    payment_info = payment_info.Substring(0, 50);
                                                }
                                                a_device_type = payment_info;
                                            }
                                            if (orderDetail.data.preorder != null)
                                            {
                                                if (orderDetail.data.preorder.order_id.HasValue)
                                                {
                                                    if (orderDetail.data.preorder.order_id.Value > 0)
                                                    {
                                                        a_recipient_address_geo = "Preorder";
                                                    }
                                                    else
                                                    {
                                                        a_recipient_address_geo = "";
                                                    }
                                                }
                                            }
                                            //add by nurul 23/11/2021
                                            if (orderDetail.data.order_warehouse != null)
                                            {
                                                warehouse_id = orderDetail.data.order_warehouse.warehouse_id;
                                            }
                                            //end add by nurul 23/11/2021
                                        }
                                    }
                                    //belum ada di temp
                                    var listBRG2 = new List<string>();
                                    int indexProduct2 = 0;
                                    foreach (var product in order.products)
                                    {
                                        indexProduct2++;
                                        if (!listBRG2.Contains(product.id.ToString()))
                                        {
                                            listBRG2.Add(product.id.ToString());
                                        }
                                        else
                                        {
                                            //ada barang yg duplikat
                                            skipInsert2 = true;
                                            //break; //remark 15 des 2021, handle brg double dengan sp baru
                                        }
                                        #region cut max length dan ubah '
                                        string a_currency = !string.IsNullOrEmpty(product.currency) ? product.currency.Replace('\'', '`') : "";
                                        if (a_currency.Length > 50)
                                        {
                                            a_currency = a_currency.Substring(0, 50);
                                        }
                                        string a_product_sku = !string.IsNullOrEmpty(product.sku) ? product.sku.Replace('\'', '`') : "";
                                        if (a_product_sku.Length > 50)
                                        {
                                            a_product_sku = a_product_sku.Substring(0, 50);
                                        }
                                        #endregion
                                        TEMP_TOKPED_ORDERS newOrder = new TEMP_TOKPED_ORDERS()
                                        {
                                            fs_id = a_fs_id,
                                            order_id = a_order_id,
                                            accept_partial = order.accept_partial,
                                            invoice_ref_num = a_invoice_ref_num,
                                            product_id = product.id,
                                            product_name = string.IsNullOrEmpty(product.name) ? "" : product.name.Replace("'", "`"),
                                            product_quantity = product.quantity,
                                            product_notes = string.IsNullOrEmpty(product.notes) ? "" : product.notes.Replace("'", "`"),
                                            product_weight = product.weight,
                                            product_total_weight = product.total_weight,
                                            product_price = product.price,
                                            product_total_price = product.total_price,
                                            product_currency = a_currency,
                                            product_sku = a_product_sku,
                                            products_fulfilled_product_id = 0,
                                            products_fulfilled_quantity_deliver = 0,
                                            products_fulfilled_quantity_reject = 0,
                                            device_type = a_device_type,
                                            buyer_id = order.buyer.id,
                                            buyer_name = a_buyer_name,
                                            buyer_email = a_buyer_email,
                                            buyer_phone = a_buyer_phone,
                                            shop_id = order.shop_id,
                                            payment_id = order.payment_id,
                                            //recipient_name = order.recipient.name,
                                            recipient_name = nama2,
                                            recipient_address_address_full = string.IsNullOrEmpty(order.recipient.address.address_full) ? "" : order.recipient.address.address_full.Replace("'", "`"),
                                            recipient_address_district = a_recipient_address_district,
                                            recipient_address_district_id = order.recipient.address.district_id,
                                            recipient_address_city = a_recipient_address_city,
                                            recipient_address_city_id = order.recipient.address.city_id,
                                            recipient_address_province = a_recipient_address_province,
                                            recipient_address_province_id = order.recipient.address.province_id,
                                            recipient_address_country = a_recipient_address_country,
                                            recipient_address_geo = a_recipient_address_geo,
                                            recipient_address_postal_code = a_recipient_address_postal_code,
                                            //recipient_phone = a_buyer_phone,
                                            recipient_phone = a_recipient_phone,
                                            logistics_shipping_id = order.logistics.shipping_id,
                                            logistics_shipping_agency = a_logistics_shipping_agency,
                                            logistics_service_type = a_logistics_service_type,
                                            amt_ttl_product_price = order.amt.ttl_product_price,
                                            amt_shipping_cost = order.amt.shipping_cost,
                                            amt_insurance_cost = order.amt.insurance_cost,
                                            amt_ttl_amount = order.amt.ttl_amount,
                                            amt_voucher_amount = order.amt.voucher_amount,
                                            amt_toppoints_amount = order.amt.toppoints_amount,
                                            dropshipper_info_name = a_dropshipper_info_name,
                                            dropshipper_info_phone = a_dropshipper_info_phone,
                                            voucher_info_voucher_code = a_voucher_info_voucher_code,
                                            voucher_info_voucher_type = order.voucher_info.voucher_type,
                                            order_status = order.order_status,
                                            create_time = DateTimeOffset.FromUnixTimeSeconds(order.create_time).UtcDateTime,
                                            custom_fields_awb = a_custom_fields_awb,
                                            conn_id = connId,
                                            CUST = CUST,
                                            NAMA_CUST = NAMA_CUST
                                        };
                                        var product_fulfilled = order.products_fulfilled.Where(p => p.product_id == product.id).FirstOrDefault();
                                        if (product_fulfilled != null)
                                        {
                                            newOrder.products_fulfilled_product_id = product_fulfilled.product_id;
                                            newOrder.products_fulfilled_quantity_deliver = product_fulfilled.quantity_deliver;
                                            newOrder.products_fulfilled_quantity_reject = product_fulfilled.quantity_reject;
                                        }
                                        newOrder.product_currency = product.id + "_" + indexProduct2;
                                        newOrder.confirm_shipping_deadline = expiredDate;
                                        if (paymentDate != null)
                                        {
                                            newOrder.create_time = paymentDate.Value;
                                        }
                                        //add by nurul 23/11/2021
                                        if(warehouse_id != 0 && warehouse_id != null)
                                        {
                                            newOrder.warehouse_id = warehouse_id;
                                        }
                                        //end add by nurul 23/11/2021
                                        ListNewOrders.Add(newOrder);
                                    }
                                    //if (skipInsert2)
                                    //{
                                    //    continue;
                                    //}
                                }

                                insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
                                EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);

                                ErasoftDbContext.TEMP_TOKPED_ORDERS.AddRange(ListNewOrders);
                                ErasoftDbContext.SaveChanges();

                                using (SqlCommand CommandSQL = new SqlCommand())
                                {
                                    //call sp to insert buyer data
                                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIdARF01C;

                                    EDB.ExecuteSQL("Con", "MoveARF01CFromTempTable", CommandSQL);
                                };
                                if (skipInsert2)
                                {
                                    using (SqlCommand CommandSQL = new SqlCommand())
                                    {
                                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connId;
                                        CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                                        CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        CommandSQL.Parameters.Add("@MARKET", SqlDbType.VarChar).Value = "TOKPED";
                                        CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                                        EDB.ExecuteSQL("Con", "MoveOrderFromTempTableNew", CommandSQL);
                                        jmlhNewOrder++;
                                    }
                                }
                                else
                                {
                                    using (SqlCommand CommandSQL = new SqlCommand())
                                    {
                                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connId;
                                        CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                                        CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 1;
                                        CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                                        EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
                                        jmlhNewOrder++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (rowCount > 99)
            {
                //add by nurul 25/1/2021, bundling
                //var listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in ('" + connId + "')").ToList();
                //if (listBrgKomponen.Count() > 0)
                //{
                //    ret = "1";
                //}
                //end add by nurul 25/1/2021, bundling

                //add by Tri 4 Mei 2020, update stok di jalankan per batch karena batch berikutnya akan memiliki connID yg berbeda
                new StokControllerJob().updateStockMarketPlace(connId, iden.DatabasePathErasoft, iden.username);
                //end add by Tri 4 Mei 2020, update stok di jalankan per batch karena batch berikutnya akan memiliki connID yg berbeda

                //change by nurul 25/1/2021, bundling
                //await GetOrderList3days(iden, stat, CUST, NAMA_CUST, (page + 1), jmlhNewOrder, daysFrom, daysTo);
                var returnGetOrder = await GetOrderList3days(iden, stat, CUST, NAMA_CUST, (page + 1), jmlhNewOrder, daysFrom, daysTo, ord_stat);
                //if(returnGetOrder == "1")
                //{
                //    ret = "1";
                //}
                if (!string.IsNullOrEmpty(returnGetOrder))
                {
                    ret += returnGetOrder;
                }
                //end change by nurul 25/1/2021, bundling
            }
            else
            {
                //add by calvin 1 april 2019
                //notify user
                if (jmlhNewOrder > 0)
                {
                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari Tokopedia.");

                    //add by nurul 25/1/2021, bundling
                    //var listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in ('" + connId + "')").ToList();
                    //if (listBrgKomponen.Count() > 0)
                    //{
                    //    ret = "1";
                    //}
                    //ret += "'" + ret + "' , ";
                    //end add by nurul 25/1/2021, bundling

                    new StokControllerJob().updateStockMarketPlace(connId, iden.DatabasePathErasoft, iden.username);
                }
                //end add by calvin 1 april 2019
            }
            //}
            //catch (Exception ex)
            //{
            //    if(!ex.Message.ToLower().Contains("unauthorized"))
            //        if (!ex.Message.ToLower().Contains("many") && !ex.Message.ToLower().Contains("request"))
            //    {
            //        var log = new TABEL_LOG_GETORDERS()
            //        {
            //            DBPATHERA = iden.DatabasePathErasoft,
            //            MARKETPLACE = "TOKPED",
            //            TGL = DateTime.UtcNow.AddHours(7),
            //            FUNCTION = "GetOrderList3days " + stat.ToString() + " : " + CUST,
            //            ERRORMSG = ex.Message
            //        };
            //        MoDbContext.TABEL_LOG_GETORDERS.Add(log);
            //        MoDbContext.SaveChanges();
            //    }
            //    throw ex;
            //}
            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrderList(TokopediaAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            var token = SetupContext(iden);

            var daysFrom = -1;
            var daysTo = 1;
            var daysNow = DateTime.UtcNow.AddHours(7);

            //add by nurul 20/1/2021, bundling 
            var AdaKomponen = false;
            var connIdProses = "";
            List<string> tempConnId = new List<string>() { };
            //end add by nurul 20/1/2021, bundling 

            //while (daysFrom > -13)
            if (iden.webhook != "1")
            {
                while (daysFrom >= -3)//pesanan sudah dibayar ambil -3 hari saja
                {
                    //add 16 des 2020, fixed date
                    //var fromDt = (long)DateTimeOffset.UtcNow.AddDays(daysFrom).ToUnixTimeSeconds();
                    //var toDt = (long)DateTimeOffset.UtcNow.AddDays(daysTo).ToUnixTimeSeconds();
                    var fromDt = (long)daysNow.AddDays(daysFrom).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                    var toDt = (long)daysNow.AddDays(daysTo > 0 ? 0 : daysTo).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

                    //end add 16 des 2020, fixed date

                    //change by nurul 20/1/2021, bundling 
                    //await GetOrderList3days(iden, stat, CUST, NAMA_CUST, 1, 0, fromDt, toDt);
                    var returnGetOrder = await GetOrderList3days(iden, stat, CUST, NAMA_CUST, 1, 0, fromDt, toDt, "");
                    //change by nurul 20/1/2021, bundling 
                    //daysFrom -= 3;
                    //daysTo -= 3;
                    daysFrom -= 2;
                    daysTo -= 2;

                    //add by nurul 20/1/2021, bundling 
                    //if (returnGetOrder == "1")
                    //{
                    //    AdaKomponen = true;
                    //    //tempConnId.Add(returnGetOrder);
                    //    //connIdProses += "'" + returnGetOrder + "' , ";
                    //}
                    if (!string.IsNullOrEmpty(returnGetOrder))
                    {
                        connIdProses += returnGetOrder;
                    }
                    //end add by nurul 20/1/2021, bundling 
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
            }
            else
            {
                await GetOrderList_webhookON( iden,  stat,  CUST,  NAMA_CUST,  page,  jmlhNewOrder);
            }
            // tunning untuk tidak duplicate
            var queryStatus = "\\\"}\"" + "," + "\"2\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","2","\"000003\""
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + queryStatus + "%' and arguments like '%" + iden.API_secret_key + "%' and invocationdata like '%tokopedia%' and invocationdata like '%GetOrderList%' and statename like '%Enque%' and invocationdata not like '%resi%' and invocationdata not like '%GetOrderListCompleted%' and invocationdata not like '%GetOrderListCancel%' and invocationdata not like '%GetSingleOrder%' and invocationdata not like '%CheckPendings%' and invocationdata not like '%GetOrderList_webhook%'");
            // end tunning untuk tidak duplicate
            

            return ret;
        }
        public async Task<string> GetOrderList_webhookON(TokopediaAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            var daysFrom = -1;
            var daysTo = 1;
            var daysNow = DateTime.UtcNow.AddHours(7);
            var connIdProses = "";
            List<string> tempConnId = new List<string>() { };

            while (daysFrom >= -3)//pesanan sudah dibayar ambil -3 hari saja
            {
                var fromDt = (long)daysNow.AddDays(daysFrom).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                var toDt = (long)daysNow.AddDays(daysTo > 0 ? 0 : daysTo).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

                var returnGetOrder = await GetOrderList3days(iden, stat, CUST, NAMA_CUST, 1, 0, fromDt, toDt, "220");

                if (!string.IsNullOrEmpty(returnGetOrder))
                {
                    connIdProses += returnGetOrder;
                }

                returnGetOrder = await GetOrderList3days(iden, stat, CUST, NAMA_CUST, 1, 0, fromDt, toDt, "400");

                if (!string.IsNullOrEmpty(returnGetOrder))
                {
                    connIdProses += returnGetOrder;
                }

                returnGetOrder = await GetOrderList3days(iden, stat, CUST, NAMA_CUST, 1, 0, fromDt, toDt, "450");

                if (!string.IsNullOrEmpty(returnGetOrder))
                {
                    connIdProses += returnGetOrder;
                }

                daysFrom -= 2;
                daysTo -= 2;

            }

            if (!string.IsNullOrEmpty(connIdProses))
            {
                new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username, connIdProses.Substring(0, connIdProses.Length - 3));
            }

            return "";
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrderList_webhook(TokopediaAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            var token = SetupContext(iden);
            var daysNow = DateTime.UtcNow.AddHours(7).AddDays(-1);
            EDB.ExecuteSQL("CString", CommandType.Text, "DELETE FROM TABEL_WEBHOOK_TOKPED WHERE CUST = '" + CUST 
                + "' AND TGL <  '" + daysNow.AddDays(-2).ToString("yyyy-MM-dd HH:mm:ss") + "'");
            var dsNewOrder = EDB.GetDataSet("CString", "SO", "SELECT T.* FROM TABEL_WEBHOOK_TOKPED (NOLOCK) T LEFT JOIN SOT01A (NOLOCK) S ON S.NO_REFERENSI = (CONVERT(NVARCHAR(50),T.ORDERID) + ';' + T.NO_INV) AND T.CUST = S.CUST WHERE T.TGL >= '"+ daysNow.ToString("yyyy-MM-dd HH:mm:ss") + "' AND ORDER_STATUS = '220' AND T.CUST = '" + CUST + "' AND ISNULL(S.NO_BUKTI, '') = ''");
            
            var connIdProses = "";
            var dataJsonOrder = new TokopediaOrders();
            var listData = new List<TokopediaOrder>();
            if (dsNewOrder.Tables[0].Rows.Count > 0)
            {
                for(int i=0;i < dsNewOrder.Tables[0].Rows.Count; i++)
                {
                    var insertData = dsNewOrder.Tables[0].Rows[i]["JSON"].ToString();
                    var cJson = JsonConvert.DeserializeObject(insertData, typeof(TokopediaOrder)) as TokopediaOrder;
                    listData.Add(cJson);
                    if (listData.Count >= 1 || i == dsNewOrder.Tables[0].Rows.Count - 1)
                    {
                        dataJsonOrder.data = listData.ToArray();
                        var returnGetOrder = await InsertOrderFromWebhook(iden, stat, CUST, NAMA_CUST, 1, 0, daysNow.AddDays(-1), dataJsonOrder);
                        if (!string.IsNullOrEmpty(returnGetOrder))
                        {
                            connIdProses += returnGetOrder;
                        }
                        listData = new List<TokopediaOrder>();
                    }
                }
            }

            if (!string.IsNullOrEmpty(connIdProses))
            {
                new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username, connIdProses.Substring(0, connIdProses.Length - 3));
            }
            

            var queryStatus = "\\\"}\"" + "," + "\"2\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","2","\"000003\""
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + queryStatus + "%' and arguments like '%" + iden.API_secret_key + "%' and invocationdata like '%tokopedia%' and invocationdata like '%GetOrderList_webhook%' and statename like '%Enque%' ");
            
            return ret;
        }
        public async Task<string> InsertOrderFromWebhook(TokopediaAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, DateTime daysNow, TokopediaOrders result)
        {
            string ret = "";
            string connId = Guid.NewGuid().ToString();
            var token = SetupContext(iden);
            iden.token = token;
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";

            //add by nurul 20/1/2021, bundling
            //ret = connId;
            ret += "'" + connId + "' , ";
            //end add by nurul 20/1/2021, bundling

            int rowCount = 0;
            //if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                //TokopediaOrders result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaOrders)) as TokopediaOrders;
                if (result.data != null)
                {
                    var orderPaid = result.data.Where(p => p.order_status == 220).ToList();
                    

                    var orderTokpedInDb = ErasoftDbContext.TEMP_TOKPED_ORDERS.Where(p => p.fs_id == iden.merchant_code);

                    //var last21days = DateTimeOffset.UtcNow.AddHours(7).AddDays(-21).DateTime;
                    //System.DateTime datetimeisnull = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    //var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST && (p.TGL ?? datetimeisnull) > last21days).Select(p => p.NO_REFERENSI).ToList();

                    //var last21days = DateTimeOffset.UtcNow.AddHours(7).AddDays(daysFrom - 1).DateTime;
                    //var last21days2 = DateTimeOffset.UtcNow.AddHours(7).AddDays(daysTo + 1).DateTime;
                    //var last21days = DateTimeOffset.FromUnixTimeMilliseconds(daysFrom * 1000).UtcDateTime.AddHours(7).AddDays(-1);
                    //var last21days2 = DateTimeOffset.FromUnixTimeMilliseconds(daysTo * 1000).UtcDateTime.AddHours(7).AddDays(1);
                    System.DateTime datetimeisnull = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    //var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST && (p.TGL ?? datetimeisnull) >= last21days && (p.TGL ?? datetimeisnull) <= last21days2).Select(p => p.NO_REFERENSI).ToList();
                    var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST && (p.TGL ?? datetimeisnull) >= daysNow).Select(p => p.NO_REFERENSI).ToList();

                    var connIdARF01C = Guid.NewGuid().ToString();
                    rowCount = result.data.Count();

                    var decryptF = new EncryptTokped();//add 28 NOv 2020, decrypt tokped
                    if (orderPaid != null)
                    {
                        foreach (var order in orderPaid)
                        {
                            if (!OrderNoInDb.Contains(order.order_id + ";" + order.invoice_ref_num))
                            {
                                //add 28 NOv 2020, decrypt tokped
                                if (order.encryption != null)
                                {
                                    if (!string.IsNullOrEmpty(order.encryption.secret) && !string.IsNullOrEmpty(order.encryption.content))
                                    {
                                        if(order.buyer == null)
                                        {
                                            order.buyer = new Buyer();
                                        }
                                        var decryptedOrder = decryptF.DecryptOrderTokped(order.encryption.secret, order.encryption.content);
                                        if (!string.IsNullOrEmpty(decryptedOrder))
                                        {
                                            DecryptedOrderList decryptedOrderData = Newtonsoft.Json.JsonConvert.DeserializeObject(decryptedOrder, typeof(DecryptedOrderList)) as DecryptedOrderList;
                                            //if (!string.IsNullOrEmpty(decryptedOrderData.buyer.name))
                                            //    order.buyer.name = decryptedOrderData.buyer.name;
                                            //if (!string.IsNullOrEmpty(decryptedOrderData.buyer.phone))
                                            //    order.buyer.phone = decryptedOrderData.buyer.phone;
                                            if(decryptedOrderData.customer != null)
                                            {
                                                if (!string.IsNullOrEmpty(decryptedOrderData.customer.name))
                                                    order.buyer.name = decryptedOrderData.customer.name;
                                                if (!string.IsNullOrEmpty(decryptedOrderData.customer.phone))
                                                    order.buyer.phone = decryptedOrderData.customer.phone;
                                            }
                                            if (!string.IsNullOrEmpty(decryptedOrderData.recipient.name))
                                                order.recipient.name = decryptedOrderData.recipient.name;
                                            if (!string.IsNullOrEmpty(decryptedOrderData.recipient.phone))
                                                order.recipient.phone = decryptedOrderData.recipient.phone;
                                            if (!string.IsNullOrEmpty(decryptedOrderData.recipient.address.address_full))
                                                order.recipient.address.address_full = decryptedOrderData.recipient.address.address_full;
                                        }
                                    }
                                }
                                //end add 28 NOv 2020, decrypt tokped

                                List<TEMP_TOKPED_ORDERS> ListNewOrders = new List<TEMP_TOKPED_ORDERS>();
                                ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_TOKPED_ORDERS");

                                string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                                var kabKot = "3174";
                                var prov = "31";
                                var nama = order.recipient.name.Replace("'", "`");
                                if (nama.Length > 30)
                                    nama = nama.Substring(0, 30);
                                string TLP = !string.IsNullOrEmpty(order.recipient.phone) ? order.recipient.phone : "";
                                if (TLP.Length > 30)
                                    TLP = TLP.Substring(0, 30);
                                if (NAMA_CUST.Length > 30)
                                    NAMA_CUST = NAMA_CUST.Substring(0, 30);
                                string AL_KIRIM1 = !string.IsNullOrEmpty(order.recipient.address.address_full) ? order.recipient.address.address_full.Replace('\'', '`') : "";
                                if (AL_KIRIM1.Length > 30)
                                {
                                    AL_KIRIM1 = AL_KIRIM1.Substring(0, 30);
                                }
                                string KODEPOS = !string.IsNullOrEmpty(order.recipient.address.postal_code) ? order.recipient.address.postal_code.Replace('\'', '`') : "";
                                if (KODEPOS.Length > 7)
                                {
                                    KODEPOS = KODEPOS.Substring(0, 7);
                                }
                                //insertPembeli += "('" + order.recipient.name.Replace("'", "`") + "','" + order.recipient.address.address_full.Replace("'", "`") + "','" + order.recipient.phone + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
                                insertPembeli += "('" + nama + "','" + order.recipient.address.address_full.Replace("'", "`") + "','" + TLP + "','" + NAMA_CUST + "',0,0,'0','01',";
                                insertPembeli += "1, 'IDR', '01', '" + AL_KIRIM1 + "', 0, 0, 0, 0, '1', 0, 0, ";
                                insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + KODEPOS + "', '', '" + kabKot + "', '" + prov + "', '', '','" + connIdARF01C + "'),";

                                var order_order_id = Convert.ToString(order.order_id);
                                #region cut max length dan ubah '
                                string fs_id = !string.IsNullOrEmpty(order.fs_id) ? order.fs_id.Replace('\'', '`') : "";
                                if (fs_id.Length > 50)
                                {
                                    fs_id = fs_id.Substring(0, 50);
                                }
                                string order_id = !string.IsNullOrEmpty(Convert.ToString(order.order_id)) ? Convert.ToString(order.order_id).Replace('\'', '`') : "";
                                if (order_id.Length > 50)
                                {
                                    order_id = order_id.Substring(0, 50);
                                }
                                string invoice_ref_num = !string.IsNullOrEmpty(order.invoice_ref_num) ? order.invoice_ref_num.Replace('\'', '`') : "";
                                if (invoice_ref_num.Length > 50)
                                {
                                    invoice_ref_num = invoice_ref_num.Substring(0, 50);
                                }
                                string device_type = !string.IsNullOrEmpty(order.device_type) ? order.device_type.Replace('\'', '`') : "";
                                if (device_type.Length > 50)
                                {
                                    device_type = device_type.Substring(0, 50);
                                }
                                string buyer_name = !string.IsNullOrEmpty(order.buyer.name) ? order.buyer.name.Replace('\'', '`') : "";
                                if (buyer_name.Length > 250)
                                {
                                    buyer_name = buyer_name.Substring(0, 250);
                                }
                                string buyer_phone = !string.IsNullOrEmpty(order.buyer.phone) ? order.buyer.phone.Replace('\'', '`') : "";
                                if (buyer_phone.Length > 50)
                                {
                                    buyer_phone = buyer_phone.Substring(0, 50);
                                }
                                string buyer_email = !string.IsNullOrEmpty(order.buyer.email) ? order.buyer.email.Replace('\'', '`') : "";
                                if (buyer_email.Length > 250)
                                {
                                    buyer_email = buyer_email.Substring(0, 250);
                                }
                                string recipient_name = !string.IsNullOrEmpty(order.recipient.name) ? order.recipient.name.Replace('\'', '`') : "";
                                if (recipient_name.Length > 250)
                                {
                                    recipient_name = recipient_name.Substring(0, 250);
                                }
                                string recipient_address_district = !string.IsNullOrEmpty(order.recipient.address.district) ? order.recipient.address.district.Replace('\'', '`') : "";
                                if (recipient_address_district.Length > 150)
                                {
                                    recipient_address_district = recipient_address_district.Substring(0, 150);
                                }
                                string recipient_address_city = !string.IsNullOrEmpty(order.recipient.address.city) ? order.recipient.address.city.Replace('\'', '`') : "";
                                if (recipient_address_city.Length > 150)
                                {
                                    recipient_address_city = recipient_address_city.Substring(0, 150);
                                }
                                string recipient_address_province = !string.IsNullOrEmpty(order.recipient.address.province) ? order.recipient.address.province.Replace('\'', '`') : "";
                                if (recipient_address_province.Length > 150)
                                {
                                    recipient_address_province = recipient_address_province.Substring(0, 150);
                                }
                                string recipient_address_country = !string.IsNullOrEmpty(order.recipient.address.country) ? order.recipient.address.country.Replace('\'', '`') : "";
                                if (recipient_address_country.Length > 150)
                                {
                                    recipient_address_country = recipient_address_country.Substring(0, 150);
                                }
                                string recipient_address_postal_code = !string.IsNullOrEmpty(order.recipient.address.postal_code) ? order.recipient.address.postal_code.Replace('\'', '`') : "";
                                if (recipient_address_postal_code.Length > 10)
                                {
                                    recipient_address_postal_code = recipient_address_postal_code.Substring(0, 10);
                                }
                                //string recipient_address_geo = !string.IsNullOrEmpty(order.recipient.address.geo) ? order.recipient.address.geo.Replace('\'', '`') : "";
                                //if (recipient_address_geo.Length > 150)
                                //{
                                //    recipient_address_geo = recipient_address_geo.Substring(0, 150);
                                //}
                                string recipient_address_geo = "";
                                string recipient_phone = !string.IsNullOrEmpty(order.recipient.phone) ? order.recipient.phone.Replace('\'', '`') : "";
                                if (recipient_phone.Length > 50)
                                {
                                    recipient_phone = recipient_phone.Substring(0, 50);
                                }
                                string logistics_shipping_agency = !string.IsNullOrEmpty(order.logistics.shipping_agency) ? order.logistics.shipping_agency.Replace('\'', '`') : "";
                                if (logistics_shipping_agency.Length > 150)
                                {
                                    logistics_shipping_agency = logistics_shipping_agency.Substring(0, 150);
                                }
                                string logistics_service_type = !string.IsNullOrEmpty(order.logistics.service_type) ? order.logistics.service_type.Replace('\'', '`') : "";
                                if (logistics_service_type.Length > 150)
                                {
                                    logistics_service_type = logistics_service_type.Substring(0, 150);
                                }
                                string dropshipper_info_name = !string.IsNullOrEmpty(order.dropshipper_info.name) ? order.dropshipper_info.name.Replace('\'', '`') : "";
                                if (dropshipper_info_name.Length > 250)
                                {
                                    dropshipper_info_name = dropshipper_info_name.Substring(0, 250);
                                }
                                string dropshipper_info_phone = !string.IsNullOrEmpty(order.dropshipper_info.phone) ? order.dropshipper_info.phone.Replace('\'', '`') : "";
                                if (dropshipper_info_phone.Length > 50)
                                {
                                    dropshipper_info_phone = dropshipper_info_phone.Substring(0, 50);
                                }
                                string voucher_info_voucher_code = !string.IsNullOrEmpty(order.voucher_info.voucher_code) ? order.voucher_info.voucher_code.Replace('\'', '`') : "";
                                if (voucher_info_voucher_code.Length > 150)
                                {
                                    voucher_info_voucher_code = voucher_info_voucher_code.Substring(0, 150);
                                }
                                string custom_fields_awb = !string.IsNullOrEmpty(order.custom_fields.awb) ? order.custom_fields.awb.Replace('\'', '`') : "";
                                if (custom_fields_awb.Length > 150)
                                {
                                    custom_fields_awb = custom_fields_awb.Substring(0, 150);
                                }
                                #endregion
                                var skipInsert = false;
                                if (orderTokpedInDb.Where(p => p.order_id == order_order_id).Count() == 0)
                                {
                                    DateTime? expiredDate = null;
                                    DateTime? paymentDate = null;
                                    long? warehouse_id = 0;
                                    //remark 13 nov 2020 tutup sementara
                                    var orderDetail = await GetOrderDetail(iden, order.invoice_ref_num);
                                    if (orderDetail != null)
                                    {
                                        if (orderDetail.data != null)
                                        {
                                            if (orderDetail.data.shipment_fulfillment != null)
                                            {
                                                expiredDate = orderDetail.data.shipment_fulfillment.confirm_shipping_deadline;
                                            }
                                            paymentDate = orderDetail.data.payment_date;
                                            if (orderDetail.data.payment_info != null)
                                            {
                                                var payment_info = !string.IsNullOrEmpty(orderDetail.data.payment_info.gateway_name) ? orderDetail.data.payment_info.gateway_name.Replace('\'', '`') : "";
                                                if (payment_info.Length > 50)
                                                {
                                                    payment_info = payment_info.Substring(0, 50);
                                                }
                                                device_type = payment_info;
                                            }
                                            if (orderDetail.data.preorder != null)
                                            {
                                                if (orderDetail.data.preorder.order_id.HasValue)
                                                {
                                                    if (orderDetail.data.preorder.order_id.Value > 0)
                                                    {
                                                        recipient_address_geo = "Preorder";
                                                    }
                                                    else
                                                    {
                                                        recipient_address_geo = "";
                                                    }
                                                }
                                            }
                                            //add by nurul 23/11/2021
                                            if (orderDetail.data.order_warehouse != null)
                                            {
                                                warehouse_id = orderDetail.data.order_warehouse.warehouse_id;
                                            }
                                            //end add by nurul 23/11/2021
                                        }
                                    }
                                    //belum ada di temp
                                    //end remark 13 nov 2020 tutup sementara
                                    var listBRG = new List<string>();
                                    int indexProduct = 0;
                                    foreach (var product in order.products)
                                    {
                                        indexProduct++;
                                        if (!listBRG.Contains(product.id.ToString()))
                                        {
                                            listBRG.Add(product.id.ToString());
                                        }
                                        else
                                        {
                                            //ada barang yg duplikat
                                            skipInsert = true;
                                            //break; //remark 15 des 2021, handle brg double dengan sp baru
                                        }
                                        #region cut max length dan ubah '
                                        string currency = !string.IsNullOrEmpty(product.currency) ? product.currency.Replace('\'', '`') : "";
                                        if (currency.Length > 50)
                                        {
                                            currency = currency.Substring(0, 50);
                                        }
                                        string product_sku = !string.IsNullOrEmpty(product.sku) ? product.sku.Replace('\'', '`') : "";
                                        if (product_sku.Length > 50)
                                        {
                                            product_sku = product_sku.Substring(0, 50);
                                        }
                                        #endregion
                                        TEMP_TOKPED_ORDERS newOrder = new TEMP_TOKPED_ORDERS()
                                        {
                                            fs_id = fs_id,
                                            order_id = order_id,
                                            accept_partial = order.accept_partial,
                                            invoice_ref_num = invoice_ref_num,
                                            product_id = product.id,
                                            product_name = string.IsNullOrEmpty(product.name) ? "" : product.name.Replace("'", "`"),
                                            product_quantity = product.quantity,
                                            product_notes = string.IsNullOrEmpty(product.notes) ? "" : product.notes.Replace("'", "`"),
                                            product_weight = product.weight,
                                            product_total_weight = product.total_weight,
                                            product_price = product.price,
                                            product_total_price = product.total_price,
                                            product_currency = currency,
                                            product_sku = product_sku,
                                            products_fulfilled_product_id = 0,
                                            products_fulfilled_quantity_deliver = 0,
                                            products_fulfilled_quantity_reject = 0,
                                            device_type = device_type,
                                            buyer_id = order.buyer.id,
                                            buyer_name = buyer_name,
                                            buyer_email = buyer_email,
                                            buyer_phone = buyer_phone,
                                            shop_id = order.shop_id,
                                            payment_id = order.payment_id,
                                            //recipient_name = order.recipient.name,
                                            recipient_name = nama,
                                            recipient_address_address_full = string.IsNullOrEmpty(order.recipient.address.address_full) ? "" : order.recipient.address.address_full.Replace("'", "`"),
                                            recipient_address_district = recipient_address_district,
                                            recipient_address_district_id = order.recipient.address.district_id,
                                            recipient_address_city = recipient_address_city,
                                            recipient_address_city_id = order.recipient.address.city_id,
                                            recipient_address_province = recipient_address_province,
                                            recipient_address_province_id = order.recipient.address.province_id,
                                            recipient_address_country = recipient_address_country,
                                            recipient_address_geo = recipient_address_geo,
                                            recipient_address_postal_code = recipient_address_postal_code,
                                            recipient_phone = recipient_phone,
                                            logistics_shipping_id = order.logistics.shipping_id,
                                            logistics_shipping_agency = logistics_shipping_agency,
                                            logistics_service_type = logistics_service_type,
                                            amt_ttl_product_price = order.amt.ttl_product_price,
                                            amt_shipping_cost = order.amt.shipping_cost,
                                            amt_insurance_cost = order.amt.insurance_cost,
                                            amt_ttl_amount = order.amt.ttl_amount,
                                            amt_voucher_amount = order.amt.voucher_amount,
                                            amt_toppoints_amount = order.amt.toppoints_amount,
                                            dropshipper_info_name = dropshipper_info_name,
                                            dropshipper_info_phone = dropshipper_info_phone,
                                            voucher_info_voucher_code = voucher_info_voucher_code,
                                            voucher_info_voucher_type = order.voucher_info.voucher_type,
                                            order_status = order.order_status,
                                            create_time = DateTimeOffset.FromUnixTimeSeconds(order.create_time).UtcDateTime,
                                            custom_fields_awb = custom_fields_awb,
                                            conn_id = connId,
                                            CUST = CUST,
                                            NAMA_CUST = NAMA_CUST
                                        };
                                        //var product_fulfilled = order.products_fulfilled.Where(p => p.product_id == product.id).FirstOrDefault();
                                        //if (product_fulfilled != null)
                                        //{
                                        //    newOrder.products_fulfilled_product_id = product_fulfilled.product_id;
                                        //    newOrder.products_fulfilled_quantity_deliver = product_fulfilled.quantity_deliver;
                                        //    newOrder.products_fulfilled_quantity_reject = product_fulfilled.quantity_reject;
                                        //}
                                        newOrder.product_currency = product.id + "_" + indexProduct;
                                        newOrder.confirm_shipping_deadline = expiredDate;
                                        if (paymentDate != null)
                                        {
                                            newOrder.create_time = paymentDate.Value;
                                        }
                                        //add by nurul 23/11/2021
                                        if (warehouse_id != 0 && warehouse_id != null)
                                        {
                                            newOrder.warehouse_id = warehouse_id;
                                        }
                                        //end add by nurul 23/11/2021
                                        ListNewOrders.Add(newOrder);
                                    }
                                    //if (skipInsert)
                                    //{
                                    //    continue;
                                    //}
                                }

                                insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
                                EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);

                                ErasoftDbContext.TEMP_TOKPED_ORDERS.AddRange(ListNewOrders);
                                ErasoftDbContext.SaveChanges();

                                using (SqlCommand CommandSQL = new SqlCommand())
                                {
                                    //call sp to insert buyer data
                                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIdARF01C;

                                    EDB.ExecuteSQL("Con", "MoveARF01CFromTempTable", CommandSQL);
                                };
                                if (skipInsert)
                                {
                                    using (SqlCommand CommandSQL = new SqlCommand())
                                    {
                                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connId;
                                        CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                                        CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        CommandSQL.Parameters.Add("@MARKET", SqlDbType.VarChar).Value = "TOKPED";
                                        CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                                        EDB.ExecuteSQL("Con", "MoveOrderFromTempTableNew", CommandSQL);
                                        jmlhNewOrder++;
                                    }
                                }
                                else
                                {
                                    using (SqlCommand CommandSQL = new SqlCommand())
                                    {
                                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connId;
                                        CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                                        CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 1;
                                        CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                                        EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
                                        jmlhNewOrder++;
                                    }
                                }
                            }
                        }
                    }

                }
            }
            if (rowCount > 99)
            {
                new StokControllerJob().updateStockMarketPlace(connId, iden.DatabasePathErasoft, iden.username);
                
            }
            else
            {
                //add by calvin 1 april 2019
                //notify user
                if (jmlhNewOrder > 0)
                {
                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari Tokopedia.");


                    new StokControllerJob().updateStockMarketPlace(connId, iden.DatabasePathErasoft, iden.username);
                }
                //end add by calvin 1 april 2019
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrderList_update_webhook(TokopediaAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            var token = SetupContext(iden);
            var daysNow = DateTime.UtcNow.AddHours(7).AddDays(-1);
            EDB.ExecuteSQL("CString", CommandType.Text, "DELETE FROM TABEL_WEBHOOK_TOKPED WHERE CUST = '" + CUST
                + "' AND TGL <  '" + daysNow.AddDays(-2).ToString("yyyy-MM-dd HH:mm:ss") + "'");

            //string sSQL = "SELECT NO_REFERENSI INTO #TEMP_ORDER_UPDATE FROM SOT01A (NOLOCK) WHERE STATUS_TRANSAKSI = '03' AND TGL >= '"
            //    + daysNow.AddDays(-13).ToString("yyyy-MM-dd HH:mm:ss") + "' AND CUST = '" + CUST + "';";
            //sSQL += "SELECT T.* FROM TABEL_WEBHOOK_TOKPED (NOLOCK) T INNER JOIN #TEMP_ORDER_UPDATE S ON S.NO_REFERENSI LIKE (CONVERT(NVARCHAR(50),T.ORDERID) + ';%') ";
            //sSQL += "WHERE T.TGL >= '" + daysNow.ToString("yyyy-MM-dd HH:mm:ss") + "' AND ORDER_STATUS IN ('700', '701') AND T.CUST = '" + CUST + "'; DROP TABLE #TEMP_ORDER_UPDATE;";

            var dsNewOrder = EDB.ExecuteSQL("CString", CommandType.Text, "UPDATE S SET STATUS_TRANSAKSI = '04' FROM TABEL_WEBHOOK_TOKPED (NOLOCK) T INNER JOIN SOT01A (NOLOCK) S ON S.NO_REFERENSI LIKE (CONVERT(NVARCHAR(50),T.ORDERID) + ';%') AND T.CUST = S.CUST WHERE T.TGL >= '"
                + daysNow.ToString("yyyy-MM-dd HH:mm:ss") + "' AND ORDER_STATUS IN ('700', '701') AND T.CUST = '" + CUST + "' AND STATUS_TRANSAKSI = '03'");
            if(dsNewOrder > 0)
            {
                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(dsNewOrder) + " Pesanan dari Tokopedia sudah selesai.");

                var dateTimeNow = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd");
                string sSQLUpdateDatePesananSelesai = "UPDATE S SET TGL_KIRIM = '" + dateTimeNow 
                    + "' FROM TABEL_WEBHOOK_TOKPED (NOLOCK) T INNER JOIN SIT01A (NOLOCK) S ON S.NO_REF LIKE (CONVERT(NVARCHAR(50),T.ORDERID) + ';%')  AND T.CUST = S.CUST WHERE T.TGL >= '"
                    + daysNow.ToString("yyyy-MM-dd HH:mm:ss") + "' AND T.CUST = '" + CUST + "' AND ORDER_STATUS IN ('700', '701') ";
                var resultUpdateDatePesanan = EDB.ExecuteSQL("CString", CommandType.Text, sSQLUpdateDatePesananSelesai);
            }

            var queryStatus = "\\\"}\"" + "," + "\"5\"" + "," + "\"\\\"" + CUST + "\\\"\""; //     \"}","2","\"000003\""
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + queryStatus + "%' and arguments like '%" + iden.API_secret_key + "%' and invocationdata like '%tokopedia%' and invocationdata like '%GetOrderList_update_webhook%' and statename like '%Enque%' ");

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrderList_cancel_webhook(TokopediaAPIData iden, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            var token = SetupContext(iden);
            var daysNow = DateTime.UtcNow.AddHours(7).AddDays(-1);
            EDB.ExecuteSQL("CString", CommandType.Text, "DELETE FROM TABEL_WEBHOOK_TOKPED WHERE CUST = '" + CUST
                + "' AND TGL <  '" + daysNow.AddDays(-2).ToString("yyyy-MM-dd HH:mm:ss") + "'");
            string sSQL = "SELECT NO_REFERENSI INTO #TEMP_ORDER_CANCEL FROM SOT01A (NOLOCK) WHERE STATUS_TRANSAKSI not in ('11', '12') AND TGL >= '"
                + daysNow.AddDays(-13).ToString("yyyy-MM-dd HH:mm:ss") + "' AND CUST = '" + CUST + "';";
            sSQL += "SELECT T.*,S.NO_REFERENSI FROM TABEL_WEBHOOK_TOKPED (NOLOCK) T INNER JOIN #TEMP_ORDER_CANCEL S ON S.NO_REFERENSI LIKE (CONVERT(NVARCHAR(50),T.ORDERID) + ';%') ";
            sSQL += "WHERE T.TGL >= '" + daysNow.ToString("yyyy-MM-dd HH:mm:ss") + "' AND ORDER_STATUS IN ('0', '800', '801', '10', '15') AND T.CUST = '" + CUST + "'; DROP TABLE #TEMP_ORDER_CANCEL;";
            var dsNewOrder = EDB.GetDataSet("CString", "SO", sSQL);

            //var dsNewOrder = EDB.GetDataSet("CString", "SO", "SELECT T.* FROM TABEL_WEBHOOK_TOKPED (NOLOCK) T INNER JOIN SOT01A (NOLOCK) S ON (CONVERT(NVARCHAR(50),T.ORDERID) + ';%') LIKE S.NO_REFERENSI AND T.CUST = S.CUST WHERE T.TGL >= '"
            //    + daysNow.ToString("yyyy-MM-dd HH:mm:ss") + "' AND ORDER_STATUS IN ('0', '800', '801', '10', '15') AND T.CUST = '" + CUST + "' AND STATUS_TRANSAKSI not in ('11', '12')");
            //var orderCancel = result.data.Where(p => p.order_status == 0).ToList();
            //var orderRefund = result.data.Where(p => p.order_status == 800).ToList();
            //var orderRollback = result.data.Where(p => p.order_status == 801).ToList();
            //var orderCancelBySeller = result.data.Where(p => p.order_status == 10).ToList();
            //var orderCancelByBuyer = result.data.Where(p => p.order_status == 15).ToList();//add 22 sept 2020

            var connIdProses = "";
            var dataJsonOrder = new TokopediaOrders();
            var listData = new List<TokopediaOrder>();
            if (dsNewOrder.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsNewOrder.Tables[0].Rows.Count; i++)
                {
                    var insertData = dsNewOrder.Tables[0].Rows[i]["JSON"].ToString();
                    var cJson = JsonConvert.DeserializeObject(insertData, typeof(TokopediaOrder)) as TokopediaOrder;
                    var noref = dsNewOrder.Tables[0].Rows[i]["NO_REFERENSI"].ToString();
                    cJson.invoice_ref_num = noref.Split(';')[1];
                    listData.Add(cJson);
                    if (listData.Count >= 10 || i == dsNewOrder.Tables[0].Rows.Count - 1)
                    {
                        dataJsonOrder.data = listData.ToArray();
                        var returnGetOrder = await CancelOrderFromWebhook(iden, CUST, NAMA_CUST, 1, 0, daysNow.AddDays(-1), dataJsonOrder);
                        if (!string.IsNullOrEmpty(returnGetOrder))
                        {
                            connIdProses += returnGetOrder;
                        }
                        listData = new List<TokopediaOrder>();
                    }
                }
            }

            if (!string.IsNullOrEmpty(connIdProses))
            {
                new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username, connIdProses.Substring(0, connIdProses.Length - 3));
            }


            var queryStatus = "\\\"}\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","2","\"000003\""
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + queryStatus + "%' and arguments like '%" + iden.API_secret_key + "%' and invocationdata like '%tokopedia%' and invocationdata like '%GetOrderList_cancel_webhook%' and statename like '%Enque%' ");

            return ret;
        }
        public async Task<string> CancelOrderFromWebhook(TokopediaAPIData iden, string CUST, string NAMA_CUST, int page, int jmlhOrder, DateTime daysFrom, TokopediaOrders result)
        {
            string connId = Guid.NewGuid().ToString();
            //add by nurul 20/1/2021, bundling 
            //var ret = connId;
            var ret = "";
            ret += "'" + connId + "' , ";
            //end add by nurul 20/1/2021, bundling 

            var token = SetupContext(iden);
            iden.token = token;
            long milis = CurrentTimeMillis();

            int rowCount = 0;

            //if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                //TokopediaOrders result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaOrders)) as TokopediaOrders;
                if (result.data != null)
                {
                    var orderCancel = result.data.Where(p => p.order_status == 0).ToList();
                    var orderRefund = result.data.Where(p => p.order_status == 800).ToList();
                    var orderRollback = result.data.Where(p => p.order_status == 801).ToList();
                    var orderCancelBySeller = result.data.Where(p => p.order_status == 10).ToList();
                    var orderCancelByBuyer = result.data.Where(p => p.order_status == 15).ToList();//add 22 sept 2020
                    var connIdARF01C = Guid.NewGuid().ToString();
                    rowCount = result.data.Count();

                    string ordersn = "";
                    foreach (var item in orderCancel)
                    {
                        ordersn = ordersn + "'" + item.order_id + ";" + item.invoice_ref_num + "',";
                    }
                    foreach (var item in orderRefund)
                    {
                        ordersn = ordersn + "'" + item.order_id + ";" + item.invoice_ref_num + "',";
                    }
                    foreach (var item in orderRollback)
                    {
                        ordersn = ordersn + "'" + item.order_id + ";" + item.invoice_ref_num + "',";
                    }
                    foreach (var item in orderCancelBySeller)
                    {
                        ordersn = ordersn + "'" + item.order_id + ";" + item.invoice_ref_num + "',";
                    }
                    //add 22 sept 2020
                    foreach (var item in orderCancelByBuyer)
                    {
                        ordersn = ordersn + "'" + item.order_id + ";" + item.invoice_ref_num + "',";
                    }
                    //end add 22 sept 2020

                    if (ordersn != "")
                    {
                        ordersn = ordersn.Substring(0, ordersn.Length - 1);
                        var brgAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connId
                            + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + ordersn
                            + ") AND STATUS_TRANSAKSI NOT IN ('11', '12') AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) <> 1");
                        //change by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                        //var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2', STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI <> '11' AND CUST = '" + CUST + "'");
                        ////var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN (" + ordersn + ") AND STATUS <> '2' AND ST_POSTING = 'T'");
                        var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2',ORDER_CANCEL_DATE = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', STATUS_TRANSAKSI = '11', STATUS_KIRIM='5' WHERE NO_REFERENSI IN ("
                            + ordersn + ") AND STATUS_TRANSAKSI NOT IN ('11', '12') AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) <> 1");
                        //END change by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                        jmlhOrder = jmlhOrder + rowAffected;
                        if (rowAffected > 0)
                        {
                            var dsOrders = EDB.GetDataSet("MOConnectionString", "SOT01", "SELECT A.NO_BUKTI, A.NO_REFERENSI FROM SOT01A A LEFT JOIN SOT01D D ON A.NO_BUKTI = D.NO_BUKTI WHERE ISNULL(D.NO_BUKTI, '') = '' AND NO_REFERENSI IN ("
                                + ordersn + ") AND STATUS_TRANSAKSI = '11' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) <> 1");
                            if (dsOrders.Tables[0].Rows.Count > 0)
                            {
                                string sSQL = "INSERT INTO SOT01D (NO_BUKTI, CATATAN_1, USERNAME) VALUES ";
                                string sSQL2 = "";
                                for (int i = 0; i < dsOrders.Tables[0].Rows.Count; i++)
                                {
                                    var nobuk = dsOrders.Tables[0].Rows[i]["NO_REFERENSI"].ToString().Split(';');
                                    var cancelReason = "";
                                    if (nobuk.Length > 1)
                                        cancelReason = await GetCancelReason(iden, nobuk[1]);
                                    if (!string.IsNullOrEmpty(cancelReason))
                                    {
                                        sSQL2 += "('" + dsOrders.Tables[0].Rows[i]["NO_BUKTI"].ToString() + "','" + cancelReason + "','AUTO_TOKPED'),";
                                    }
                                }
                                if (!string.IsNullOrEmpty(sSQL2))
                                {
                                    sSQL += sSQL2.Substring(0, sSQL2.Length - 1);
                                    EDB.ExecuteSQL("MOConnectionString", CommandType.Text, sSQL);
                                }
                            }
                            //cek faktur
                            string qry_Retur = "SELECT F.NO_REF FROM SIT01A (NOLOCK) F INNER JOIN SOT01A (NOLOCK) P ON P.NO_BUKTI = F.NO_SO AND F.JENIS_FORM = '2' ";
                            qry_Retur += "WHERE P.NO_REFERENSI IN (" + ordersn + ") AND ISNULL(F.NO_FA_OUTLET, '-') LIKE '%-%' AND P.CUST = '" + CUST + "' AND ISNULL(P.TIPE_KIRIM,0) <> 1";
                            var dsFaktur = EDB.GetDataSet("MOConnectionString", "RETUR", qry_Retur);
                            if (dsFaktur.Tables[0].Rows.Count > 0)
                            {
                                var listFaktur = "";
                                for (int j = 0; j < dsFaktur.Tables[0].Rows.Count; j++)
                                {
                                    listFaktur += "'" + dsFaktur.Tables[0].Rows[j]["NO_REF"].ToString() + "',";
                                }
                                listFaktur = listFaktur.Substring(0, listFaktur.Length - 1);
                                var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN (" + listFaktur + ") AND STATUS <> '2' AND ST_POSTING = 'T' AND CUST = '" + CUST + "'");
                            }
                        }
                        #region handle cancel COD
                        string qrycod = "SELECT P.NO_REFERENSI, ISNULL(F.NO_REF, '') NO_REF, P.NO_BUKTI FROM SIT01A (NOLOCK) F RIGHT JOIN SOT01A (NOLOCK) P ON P.NO_BUKTI = F.NO_SO AND F.JENIS_FORM = '2' ";
                        qrycod += "WHERE P.NO_REFERENSI IN (" + ordersn + ") AND ISNULL(F.NO_FA_OUTLET, '-') LIKE '%-%' AND P.CUST = '"
                            + CUST + "' AND ISNULL(P.TIPE_KIRIM,0) = 1 AND P.STATUS_TRANSAKSI NOT IN ('11', '12')";
                        var dsOrderCOD = EDB.GetDataSet("MOConnectionString", "COD", qrycod);
                        if (dsOrderCOD.Tables[0].Rows.Count > 0)
                        {
                            var listNoRefCOD = new List<string>();
                            string sSQL_COD = "INSERT INTO SOT01D (NO_BUKTI, CATATAN_1, USERNAME) VALUES ";
                            string sSQL_COD2 = "";
                            var listPesananCOD_11 = "";
                            var listPesananCOD_12 = "";
                            for (int k = 0; k < dsOrderCOD.Tables[0].Rows.Count; k++)
                            {
                                //listNoRefCOD.Add(dsOrderCOD.Tables[0].Rows[k]["NO_REFERENSI"].ToString());
                                var nobuk = dsOrderCOD.Tables[0].Rows[k]["NO_REFERENSI"].ToString().Split(';');
                                var cancelReason = "";
                                if (nobuk.Length > 1)
                                {
                                    var orderDetail = await GetCancelReasonV2(iden, nobuk[1]);
                                    if (orderDetail.data != null)
                                    {
                                        if (orderDetail.data.cancel_request_info != null)
                                        {
                                            cancelReason = orderDetail.data.cancel_request_info.reason.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            if (orderDetail.data.order_info.order_history != null)
                                            {
                                                cancelReason = orderDetail.data.order_info.order_history[0].comment.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                cancelReason = orderDetail.data.comment.Replace('\'', '`');
                                            }
                                        }
                                        if (orderDetail.data.shipping_date.HasValue)//sudah dikirim
                                        {
                                            listPesananCOD_12 += "'" + dsOrderCOD.Tables[0].Rows[k]["NO_REFERENSI"].ToString() + "',";
                                        }
                                        else
                                        {
                                            listPesananCOD_11 += "'" + dsOrderCOD.Tables[0].Rows[k]["NO_REFERENSI"].ToString() + "',";
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(cancelReason))
                                {
                                    sSQL_COD2 += "('" + dsOrderCOD.Tables[0].Rows[k]["NO_BUKTI"].ToString() + "','" + cancelReason + "','AUTO_TOKPED'),";
                                }
                            }
                            if (!string.IsNullOrEmpty(sSQL_COD2))
                            {
                                sSQL_COD += sSQL_COD2.Substring(0, sSQL_COD2.Length - 1);
                                EDB.ExecuteSQL("MOConnectionString", CommandType.Text, sSQL_COD);
                            }

                            if (listPesananCOD_11 != "")//pesanan cod batal tapi belum di kirim
                            {
                                listPesananCOD_11 = listPesananCOD_11.Substring(0, listPesananCOD_11.Length - 1);
                                EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connId
                                        + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + listPesananCOD_11
                                        + ") AND STATUS_TRANSAKSI <> '11' AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1 "
                                        + "AND BRG NOT IN ( SELECT BRG FROM TEMP_ALL_MP_ORDER_ITEM (NOLOCK) WHERE CONN_ID = '" + connId + "')");

                                var rowAffected_2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2',ORDER_CANCEL_DATE = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', STATUS_TRANSAKSI = '11', STATUS_KIRIM='5' WHERE NO_REFERENSI IN ("
                                                 + listPesananCOD_11 + ") AND STATUS_TRANSAKSI <> '11' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1");
                                if (rowAffected_2 > 0)
                                {
                                    var rowAffectedSI_2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN ("
                                        + listPesananCOD_11 + ") AND STATUS <> '2' AND ST_POSTING = 'T' AND CUST = '" + CUST
                                        + "' AND ISNULL(NO_FA_OUTLET, '-') LIKE '%-%' ");
                                    rowAffected += rowAffected_2;
                                }

                            }
                            if (listPesananCOD_12 != "")//pesanan cod batal sudah di kirim
                            {
                                listPesananCOD_12 = listPesananCOD_12.Substring(0, listPesananCOD_12.Length - 1);
                                EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connId
                                        + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + listPesananCOD_12
                                        + ") AND STATUS_TRANSAKSI <> '12' AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1 "
                                        + "AND BRG NOT IN ( SELECT BRG FROM TEMP_ALL_MP_ORDER_ITEM (NOLOCK) WHERE CONN_ID = '" + connId + "')");

                                var rowAffected_3 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '12',ORDER_CANCEL_DATE = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE NO_REFERENSI IN ("
                                                 + listPesananCOD_12 + ") AND STATUS_TRANSAKSI <> '12' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1");
                                rowAffected += rowAffected_3;
                            }

                        }
                        #endregion

                        //add by nurul 14/4/2021, stok bundling
                        var sSQLInsertTempBundling = "INSERT INTO TEMP_ALL_MP_ORDER_ITEM_BUNDLING ([BRG],[CONN_ID],[TGL]) " +
                                                     "SELECT DISTINCT C.UNIT AS BRG, '" + connId + "' AS CONN_ID, DATEADD(HOUR, +7, GETUTCDATE()) AS TGL " +
                                                     "FROM TEMP_ALL_MP_ORDER_ITEM A(NOLOCK) " +
                                                     "LEFT JOIN TEMP_ALL_MP_ORDER_ITEM_BUNDLING B(NOLOCK) ON B.CONN_ID = '" + connId + "' AND A.BRG = B.BRG " +
                                                     "INNER JOIN STF03 C(NOLOCK) ON A.BRG = C.BRG " +
                                                     "WHERE ISNULL(A.CONN_ID,'') = '" + connId + "' " +
                                                     "AND ISNULL(B.BRG,'') = '' AND A.BRG <> 'NOT_FOUND'";
                        var execInsertTempBundling = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, sSQLInsertTempBundling);
                        //end add by nurul 14/4/2021, stok bundling

                        new StokControllerJob().updateStockMarketPlace(connId, iden.DatabasePathErasoft, iden.username);
                    }
                }
            }
            //if (rowCount > 99)
            //{
            //    if (!string.IsNullOrEmpty(returnGetOrder))
            //    {
            //        ret += returnGetOrder;
            //    }
            //}
            //else
            {
                if (jmlhOrder > 0)
                {
                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrder) + " Pesanan dari Tokopedia dibatalkan.");
                }
            }
            return ret;
        }


        public async Task<string> GetOrderListCompleted3Days(TokopediaAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhOrderComplete, long daysFrom, long daysTo, string ord_stat)
        {
            string ret = "";
            string connId = Guid.NewGuid().ToString();
            var token = SetupContext(iden);
            iden.token = token;
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";

            //complete list of order status at https://fs.tokopedia.net/docs#order-status-codes
            //400 seller accepted the order
            //401 seller accepted the order, partially
            //10  seller rejected the order
            //500 seller confirm for shipment

            switch (stat)
            {
                case StatusOrder.Cancel:
                    //Cancel Order
                    status = "0";
                    break;
                case StatusOrder.Paid:
                    //paid
                    status = "220";
                    break;
                //case StatusOrder.PackagingINP:
                //    status = "500";
                //    break;
                case StatusOrder.ShippingINP:
                    //Shipping in Progress
                    status = "500";
                    break;
                case StatusOrder.Completed:
                    //Completed (Shipping)
                    status = "700";
                    break;
                default:
                    break;
            }
            //change 16 des 2020, fixed date
            //long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(daysFrom).ToUnixTimeSeconds();
            //long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(daysTo).ToUnixTimeSeconds();
            long unixTimestampFrom = daysFrom;
            long unixTimestampTo = daysTo;
            //end change 16 des 2020, fixed date

            ////untuk perbaiki data
            //unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-106).ToUnixTimeSeconds();

            string urll = "https://fs.tokopedia.net/v1/order/list?fs_id=" + Uri.EscapeDataString(iden.merchant_code) + "&from_date=" + Convert.ToString(unixTimestampFrom) + "&to_date=" + Convert.ToString(unixTimestampTo) + "&page=" + Convert.ToString(page) + "&per_page=100&shop_id=" + Uri.EscapeDataString(iden.API_secret_key);
            if (!string.IsNullOrEmpty(ord_stat))
            {
                urll += "&status=" + ord_stat;
            }
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
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            //myReq.Headers.Add("x-blibli-mta-authorization", ("BMA " + userMTA + ":" + signature));
            //myReq.Headers.Add("x-blibli-mta-date-milis", (milis.ToString()));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            //myReq.Headers.Add("requestId", milis.ToString());
            //myReq.Headers.Add("sessionId", milis.ToString());
            //myReq.Headers.Add("username", userMTA);
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
            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}

            string ordersn = "";
            int rowCount = 0;
            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                TokopediaOrders result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaOrders)) as TokopediaOrders;
                if (result.data != null)
                {
                    var orderCompleted = result.data.Where(p => p.order_status == 700).ToList();
                    var order701 = result.data.Where(p => p.order_status == 701).ToList(); // order yang dianggap selesai tetapi barang tidak sampai ke buyer

                    var connIdARF01C = Guid.NewGuid().ToString();
                    rowCount = result.data.Count();

                    //string ordersn = "";
                    foreach (var item in orderCompleted)
                    {
                        ordersn = ordersn + "'" + item.order_id + ";" + item.invoice_ref_num + "',";
                    }
                    foreach (var item in order701)
                    {
                        ordersn = ordersn + "'" + item.order_id + ";" + item.invoice_ref_num + "',";
                    }

                    if (ordersn != "")
                    {
                        ordersn = ordersn.Substring(0, ordersn.Length - 1);
                        var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '03'");
                        jmlhOrderComplete = jmlhOrderComplete + rowAffected;
                    }
                }

            }
            if (rowCount > 99)
            {
                await GetOrderListCompleted3Days(iden, stat, CUST, NAMA_CUST, (page + 1), jmlhOrderComplete, daysFrom, daysTo, ord_stat);
            }
            else
            {
                //add by calvin 1 april 2019
                //notify user
                if (jmlhOrderComplete > 0)
                {
                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderComplete) + " Pesanan dari Tokopedia sudah selesai.");

                    //add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                    if (!string.IsNullOrEmpty(ordersn))
                    {
                        var dateTimeNow = Convert.ToDateTime(DateTime.Now.AddHours(7).ToString("yyyy-MM-dd"));
                        string sSQLUpdateDatePesananSelesai = "UPDATE SIT01A SET TGL_KIRIM = '" + dateTimeNow + "' WHERE NO_REF IN (" + ordersn + ")";
                        var resultUpdateDatePesanan = EDB.ExecuteSQL("CString", CommandType.Text, sSQLUpdateDatePesananSelesai);
                    }
                    //end add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                }
                    //end add by calvin 1 april 2019
                }
            //}
            //catch (Exception ex)
            //{
            //    if(!ex.Message.ToLower().Contains("unauthorized"))
            //        if (!ex.Message.ToLower().Contains("many") && !ex.Message.ToLower().Contains("request"))
            //    {
            //        var log = new TABEL_LOG_GETORDERS()
            //        {
            //            DBPATHERA = iden.DatabasePathErasoft,
            //            MARKETPLACE = "TOKPED",
            //            TGL = DateTime.UtcNow.AddHours(7),
            //            FUNCTION = "GetOrderListCompleted3Days : " + CUST,
            //            ERRORMSG = ex.Message
            //        };
            //        MoDbContext.TABEL_LOG_GETORDERS.Add(log);
            //        MoDbContext.SaveChanges();
            //    }
            //    throw ex;
            //}
            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrderListCompleted(TokopediaAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhOrderComplete)
        {
            //if merchant code diisi. barulah GetOrderList
            string ret = "";
            var token = SetupContext(iden);

            var daysFrom = -1;
            var daysTo = 1;
            var daysNow = DateTime.UtcNow.AddHours(7);
            if (iden.webhook != "1")
            {
                while (daysFrom >= -13)
                {
                    //add 16 des 2020, fixed date
                    //var fromDt = (long)DateTimeOffset.UtcNow.AddDays(daysFrom).ToUnixTimeSeconds();
                    //var toDt = (long)DateTimeOffset.UtcNow.AddDays(daysTo).ToUnixTimeSeconds();
                    var fromDt = (long)daysNow.AddDays(daysFrom).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                    var toDt = (long)daysNow.AddDays(daysTo > 0 ? 0 : daysTo).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                    //end add 16 des 2020, fixed date

                    await GetOrderListCompleted3Days(iden, stat, CUST, NAMA_CUST, 1, 0, fromDt, toDt, "");

                    //daysFrom -= 3;
                    //daysTo -= 3;
                    daysFrom -= 2;
                    daysTo -= 2;
                }
            }
            else
            {
                await GetOrderListCompleted_webhookON(iden, stat, CUST, NAMA_CUST, page, jmlhOrderComplete);
            }
            // tunning untuk tidak duplicate
            var queryStatus = "\\\"}\"" + "," + "\"5\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","5","\"000003\""
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + queryStatus + "%' and arguments like '%" + iden.API_secret_key + "%' and invocationdata like '%tokopedia%' and invocationdata like '%GetOrderListCompleted%' and statename like '%Enque%' and invocationdata not like '%resi%'");
            // end tunning untuk tidak duplicate

            return ret;

        }
        public async Task<string> GetOrderListCompleted_webhookON(TokopediaAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhOrderComplete)
        {
            var daysFrom = -1;
            var daysTo = 1;
            var daysNow = DateTime.UtcNow.AddHours(7);

            while (daysFrom >= -13)
            {
                var fromDt = (long)daysNow.AddDays(daysFrom).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                var toDt = (long)daysNow.AddDays(daysTo > 0 ? 0 : daysTo).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                
                await GetOrderListCompleted3Days(iden, stat, CUST, NAMA_CUST, 1, 0, fromDt, toDt, "700");

                await GetOrderListCompleted3Days(iden, stat, CUST, NAMA_CUST, 1, 0, fromDt, toDt, "701");

                daysFrom -= 2;
                daysTo -= 2;
            }

            return "";
        }
        public async Task<string> GetOrderListCancel3days(TokopediaAPIData iden, string CUST, string NAMA_CUST, int page, int jmlhOrder, long daysFrom, long daysTo, string ord_stat)
        {
            ////request by Pak Richard, cek pesanan cancel tokped mulai dari tgl publish agar tidak menumpuk antrian hangfire
            //var fixedDate = new DateTime(2020, 3, 20);
            //if (DateTimeOffset.UtcNow.AddDays(daysFrom) < fixedDate)
            //    return "";
            ////end request by Pak Richard, cek pesanan cancel tokped mulai dari tgl publish agar tidak menumpuk antrian hangfire
            string connId = Guid.NewGuid().ToString();
            //add by nurul 20/1/2021, bundling 
            //var ret = connId;
            var ret = "";
            ret += "'" + connId + "' , ";
            //end add by nurul 20/1/2021, bundling 

            var token = SetupContext(iden);
            iden.token = token;
            long milis = CurrentTimeMillis();
            //change 16 des 2020, fixed date
            //long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(daysFrom).ToUnixTimeSeconds();
            //long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(daysTo).ToUnixTimeSeconds();
            long unixTimestampFrom = daysFrom;
            long unixTimestampTo = daysTo;
            //end change 16 des 2020, fixed date

            

            string urll = "https://fs.tokopedia.net/v1/order/list?fs_id=" + Uri.EscapeDataString(iden.merchant_code) + "&from_date=" + Convert.ToString(unixTimestampFrom) + "&to_date=" + Convert.ToString(unixTimestampTo) + "&page=" + Convert.ToString(page) + "&per_page=100&shop_id=" + Uri.EscapeDataString(iden.API_secret_key);
            if (!string.IsNullOrEmpty(ord_stat))
            {
                urll += "&status=" + ord_stat;
            }
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            //try { 
            using (WebResponse response = await myReq.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseFromServer = reader.ReadToEnd();
                }
            }

            int rowCount = 0;

            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                TokopediaOrders result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaOrders)) as TokopediaOrders;
                if (result.data != null)
                {
                    var orderCancel = result.data.Where(p => p.order_status == 0).ToList();
                    var orderRefund = result.data.Where(p => p.order_status == 800).ToList();
                    var orderRollback = result.data.Where(p => p.order_status == 801).ToList();
                    var orderCancelBySeller = result.data.Where(p => p.order_status == 10).ToList();
                    var orderCancelByBuyer = result.data.Where(p => p.order_status == 15).ToList();//add 22 sept 2020
                    var connIdARF01C = Guid.NewGuid().ToString();
                    rowCount = result.data.Count();

                    string ordersn = "";
                    foreach (var item in orderCancel)
                    {
                        ordersn = ordersn + "'" + item.order_id + ";" + item.invoice_ref_num + "',";
                    }
                    foreach (var item in orderRefund)
                    {
                        ordersn = ordersn + "'" + item.order_id + ";" + item.invoice_ref_num + "',";
                    }
                    foreach (var item in orderRollback)
                    {
                        ordersn = ordersn + "'" + item.order_id + ";" + item.invoice_ref_num + "',";
                    }
                    foreach (var item in orderCancelBySeller)
                    {
                        ordersn = ordersn + "'" + item.order_id + ";" + item.invoice_ref_num + "',";
                    }
                    //add 22 sept 2020
                    foreach (var item in orderCancelByBuyer)
                    {
                        ordersn = ordersn + "'" + item.order_id + ";" + item.invoice_ref_num + "',";
                    }
                    //end add 22 sept 2020

                    if (ordersn != "")
                    {
                        ordersn = ordersn.Substring(0, ordersn.Length - 1);
                        var brgAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connId 
                            + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + ordersn 
                            + ") AND STATUS_TRANSAKSI NOT IN ('11', '12') AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) <> 1");
                        //change by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                        //var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2', STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI <> '11' AND CUST = '" + CUST + "'");
                        ////var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN (" + ordersn + ") AND STATUS <> '2' AND ST_POSTING = 'T'");
                        var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2',ORDER_CANCEL_DATE = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', STATUS_TRANSAKSI = '11', STATUS_KIRIM='5' WHERE NO_REFERENSI IN ("
                            + ordersn + ") AND STATUS_TRANSAKSI NOT IN ('11', '12') AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) <> 1");
                        //END change by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                        jmlhOrder = jmlhOrder + rowAffected;
                        if (rowAffected > 0)
                        {
                            //add by Tri 1 sep 2020, hapus packing list
                            //remark by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                            //var delPL = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03B WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN (" + ordersn + ")  AND STATUS_TRANSAKSI = '11' AND CUST = '" + CUST + "')");
                            //var delPLDetail = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03C WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN (" + ordersn + ")  AND STATUS_TRANSAKSI = '11' AND CUST = '" + CUST + "')");
                            //END remark by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                            //end add by Tri 1 sep 2020, hapus packing list
                            var dsOrders = EDB.GetDataSet("MOConnectionString", "SOT01", "SELECT A.NO_BUKTI, A.NO_REFERENSI FROM SOT01A A LEFT JOIN SOT01D D ON A.NO_BUKTI = D.NO_BUKTI WHERE ISNULL(D.NO_BUKTI, '') = '' AND NO_REFERENSI IN (" 
                                + ordersn + ") AND STATUS_TRANSAKSI = '11' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) <> 1");
                            if (dsOrders.Tables[0].Rows.Count > 0)
                            {
                                string sSQL = "INSERT INTO SOT01D (NO_BUKTI, CATATAN_1, USERNAME) VALUES ";
                                string sSQL2 = "";
                                for (int i = 0; i < dsOrders.Tables[0].Rows.Count; i++)
                                {
                                    var nobuk = dsOrders.Tables[0].Rows[i]["NO_REFERENSI"].ToString().Split(';');
                                    var cancelReason = "";
                                    if (nobuk.Length > 1)
                                        cancelReason = await GetCancelReason(iden, nobuk[1]);
                                    if (!string.IsNullOrEmpty(cancelReason))
                                    {
                                        sSQL2 += "('" + dsOrders.Tables[0].Rows[i]["NO_BUKTI"].ToString() + "','" + cancelReason + "','AUTO_TOKPED'),";
                                    }
                                }
                                if (!string.IsNullOrEmpty(sSQL2))
                                {
                                    sSQL += sSQL2.Substring(0, sSQL2.Length - 1);
                                    EDB.ExecuteSQL("MOConnectionString", CommandType.Text, sSQL);
                                }
                            }
                            //cek faktur
                            string qry_Retur = "SELECT F.NO_REF FROM SIT01A (NOLOCK) F INNER JOIN SOT01A (NOLOCK) P ON P.NO_BUKTI = F.NO_SO AND F.JENIS_FORM = '2' ";
                            qry_Retur += "WHERE P.NO_REFERENSI IN (" + ordersn + ") AND ISNULL(F.NO_FA_OUTLET, '-') LIKE '%-%' AND P.CUST = '" + CUST + "' AND ISNULL(P.TIPE_KIRIM,0) <> 1";
                            var dsFaktur = EDB.GetDataSet("MOConnectionString", "RETUR", qry_Retur);
                            if (dsFaktur.Tables[0].Rows.Count > 0)
                            {
                                var listFaktur = "";
                                for (int j = 0; j < dsFaktur.Tables[0].Rows.Count; j++)
                                {
                                    listFaktur += "'" + dsFaktur.Tables[0].Rows[j]["NO_REF"].ToString() + "',";
                                }
                                listFaktur = listFaktur.Substring(0, listFaktur.Length - 1);
                                var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN (" + listFaktur + ") AND STATUS <> '2' AND ST_POSTING = 'T' AND CUST = '" + CUST + "'");
                            }
                        }
                        //string qry_Retur = "SELECT F.NO_REF FROM SIT01A F LEFT JOIN SIT01A R ON R.NO_REF = F.NO_BUKTI AND R.JENIS_FORM = '3' AND F.JENIS_FORM = '2' ";
                        //qry_Retur += "WHERE F.NO_REF IN (" + ordersn + ") AND ISNULL(R.NO_BUKTI, '') = '' AND F.CUST = '" + CUST + "'";
                        //var dsFaktur = EDB.GetDataSet("MOConnectionString", "RETUR", qry_Retur);
                        //if (dsFaktur.Tables[0].Rows.Count > 0)
                        //{
                        //    var listFaktur = "";
                        //    for (int j = 0; j < dsFaktur.Tables[0].Rows.Count; j++)
                        //    {
                        //        listFaktur += "'" + dsFaktur.Tables[0].Rows[j]["NO_REF"].ToString() + "',";
                        //    }
                        //    listFaktur = listFaktur.Substring(0, listFaktur.Length - 1);
                        //    var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN (" + listFaktur + ") AND STATUS <> '2' AND ST_POSTING = 'T' AND CUST = '" + CUST + "'");
                        //}
                        #region handle cancel COD
                        string qrycod = "SELECT P.NO_REFERENSI, ISNULL(F.NO_REF, '') NO_REF, P.NO_BUKTI FROM SIT01A (NOLOCK) F RIGHT JOIN SOT01A (NOLOCK) P ON P.NO_BUKTI = F.NO_SO AND F.JENIS_FORM = '2' ";
                        qrycod += "WHERE P.NO_REFERENSI IN (" + ordersn + ") AND ISNULL(F.NO_FA_OUTLET, '-') LIKE '%-%' AND P.CUST = '"
                            + CUST + "' AND ISNULL(P.TIPE_KIRIM,0) = 1 AND P.STATUS_TRANSAKSI NOT IN ('11', '12')";
                        var dsOrderCOD = EDB.GetDataSet("MOConnectionString", "COD", qrycod);
                        if (dsOrderCOD.Tables[0].Rows.Count > 0)
                        {
                            var listNoRefCOD = new List<string>();
                            string sSQL_COD = "INSERT INTO SOT01D (NO_BUKTI, CATATAN_1, USERNAME) VALUES ";
                            string sSQL_COD2 = "";
                            var listPesananCOD_11 = "";
                            var listPesananCOD_12 = "";
                            for (int k = 0; k < dsOrderCOD.Tables[0].Rows.Count; k++)
                            {
                                //listNoRefCOD.Add(dsOrderCOD.Tables[0].Rows[k]["NO_REFERENSI"].ToString());
                                var nobuk = dsOrderCOD.Tables[0].Rows[k]["NO_REFERENSI"].ToString().Split(';');
                                var cancelReason = "";
                                if (nobuk.Length > 1)
                                {
                                    var orderDetail = await GetCancelReasonV2(iden, nobuk[1]);
                                    if(orderDetail.data != null)
                                    {
                                        if (orderDetail.data.cancel_request_info != null)
                                        {
                                            cancelReason = orderDetail.data.cancel_request_info.reason.Replace('\'', '`');
                                        }
                                        else
                                        {
                                            if (orderDetail.data.order_info.order_history != null)
                                            {
                                                cancelReason = orderDetail.data.order_info.order_history[0].comment.Replace('\'', '`');
                                            }
                                            else
                                            {
                                                cancelReason = orderDetail.data.comment.Replace('\'', '`');
                                            }
                                        }
                                        if (orderDetail.data.shipping_date.HasValue)//sudah dikirim
                                        {
                                            listPesananCOD_12 += "'" + dsOrderCOD.Tables[0].Rows[k]["NO_REFERENSI"].ToString() + "',";
                                        }
                                        else
                                        {
                                            listPesananCOD_11 += "'" + dsOrderCOD.Tables[0].Rows[k]["NO_REFERENSI"].ToString() + "',";
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(cancelReason))
                                {
                                    sSQL_COD2 += "('" + dsOrderCOD.Tables[0].Rows[k]["NO_BUKTI"].ToString() + "','" + cancelReason + "','AUTO_TOKPED'),";
                                }
                            }
                            if (!string.IsNullOrEmpty(sSQL_COD2))
                            {
                                sSQL_COD += sSQL_COD2.Substring(0, sSQL_COD2.Length - 1);
                                EDB.ExecuteSQL("MOConnectionString", CommandType.Text, sSQL_COD);
                            }

                            if (listPesananCOD_11 != "")//pesanan cod batal tapi belum di kirim
                            {
                                listPesananCOD_11 = listPesananCOD_11.Substring(0, listPesananCOD_11.Length - 1);
                                EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connId
                                        + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + listPesananCOD_11
                                        + ") AND STATUS_TRANSAKSI <> '11' AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1 "
                                        + "AND BRG NOT IN ( SELECT BRG FROM TEMP_ALL_MP_ORDER_ITEM (NOLOCK) WHERE CONN_ID = '" + connId + "')");

                                var rowAffected_2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2',ORDER_CANCEL_DATE = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', STATUS_TRANSAKSI = '11', STATUS_KIRIM='5' WHERE NO_REFERENSI IN ("
                                                 + listPesananCOD_11 + ") AND STATUS_TRANSAKSI <> '11' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1");
                                if (rowAffected_2 > 0)
                                {
                                    var rowAffectedSI_2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN ("
                                        + listPesananCOD_11 + ") AND STATUS <> '2' AND ST_POSTING = 'T' AND CUST = '" + CUST
                                        + "' AND ISNULL(NO_FA_OUTLET, '-') LIKE '%-%' ");
                                    rowAffected += rowAffected_2;
                                }

                            }
                            if (listPesananCOD_12 != "")//pesanan cod batal sudah di kirim
                            {
                                listPesananCOD_12 = listPesananCOD_12.Substring(0, listPesananCOD_12.Length - 1);
                                EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connId
                                        + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + listPesananCOD_12
                                        + ") AND STATUS_TRANSAKSI <> '12' AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1 "
                                        + "AND BRG NOT IN ( SELECT BRG FROM TEMP_ALL_MP_ORDER_ITEM (NOLOCK) WHERE CONN_ID = '" + connId + "')");

                                var rowAffected_3 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '12',ORDER_CANCEL_DATE = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE NO_REFERENSI IN ("
                                                 + listPesananCOD_12 + ") AND STATUS_TRANSAKSI <> '12' AND CUST = '" + CUST + "' AND ISNULL(TIPE_KIRIM,0) = 1");
                                rowAffected += rowAffected_3;
                            }

                        }
                        #endregion
                        //add by nurul 25/1/2021, bundling
                        //var listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in ('" + connId + "')").ToList();
                        //if (listBrgKomponen.Count() > 0)
                        //{
                        //    ret = "1";
                        //}
                        //end add by nurul 25/1/2021, bundling

                        //add by nurul 14/4/2021, stok bundling
                        var sSQLInsertTempBundling = "INSERT INTO TEMP_ALL_MP_ORDER_ITEM_BUNDLING ([BRG],[CONN_ID],[TGL]) " +
                                                     "SELECT DISTINCT C.UNIT AS BRG, '" + connId + "' AS CONN_ID, DATEADD(HOUR, +7, GETUTCDATE()) AS TGL " +
                                                     "FROM TEMP_ALL_MP_ORDER_ITEM A(NOLOCK) " +
                                                     "LEFT JOIN TEMP_ALL_MP_ORDER_ITEM_BUNDLING B(NOLOCK) ON B.CONN_ID = '" + connId + "' AND A.BRG = B.BRG " +
                                                     "INNER JOIN STF03 C(NOLOCK) ON A.BRG = C.BRG " +
                                                     "WHERE ISNULL(A.CONN_ID,'') = '" + connId + "' " +
                                                     "AND ISNULL(B.BRG,'') = '' AND A.BRG <> 'NOT_FOUND'";
                        var execInsertTempBundling = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, sSQLInsertTempBundling);
                        //end add by nurul 14/4/2021, stok bundling

                        new StokControllerJob().updateStockMarketPlace(connId, iden.DatabasePathErasoft, iden.username);
                    }
                }
            }
            if (rowCount > 99)
            {
                //change by nurul 25/1/2021, bundling
                //await GetOrderListCancel3days(iden, CUST, NAMA_CUST, (page + 1), jmlhOrder, daysFrom, daysTo);
                var returnGetOrder = await GetOrderListCancel3days(iden, CUST, NAMA_CUST, (page + 1), jmlhOrder, daysFrom, daysTo, ord_stat);
                //if(returnGetOrder == "1")
                //{
                //    ret = "1";
                //}
                if (!string.IsNullOrEmpty(returnGetOrder))
                {
                    ret += returnGetOrder;
                }
                //end change by nurul 25/1/2021, bundling
            }
            else
            {
                if (jmlhOrder > 0)
                {
                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrder) + " Pesanan dari Tokopedia dibatalkan.");
                }
                }
            //}
            //catch (Exception ex)
            //{
            //    if(!ex.Message.ToLower().Contains("unauthorized"))
            //        if (!ex.Message.ToLower().Contains("many") && !ex.Message.ToLower().Contains("request"))
            //    {
            //        var log = new TABEL_LOG_GETORDERS()
            //        {
            //            DBPATHERA = iden.DatabasePathErasoft,
            //            MARKETPLACE = "TOKPED",
            //            TGL = DateTime.UtcNow.AddHours(7),
            //            FUNCTION = "GetOrderListCancel3days : " + CUST,
            //            ERRORMSG = ex.Message
            //        };
            //        MoDbContext.TABEL_LOG_GETORDERS.Add(log);
            //        MoDbContext.SaveChanges();
            //    }
            //    throw ex;
            //}
            //return "";
            return ret; 
        }
        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrderListCancel(TokopediaAPIData iden, string CUST, string NAMA_CUST, int page, int jmlhOrder)
        {
            string ret = "";
            var token = SetupContext(iden);

            var daysFrom = -1;
            var daysTo = 1;
            var daysNow = DateTime.UtcNow.AddHours(7);
            //add by nurul 20/1/2021, bundling 
            var AdaKomponen = false;
            var connIdProses = "";
            List<string> tempConnId = new List<string>() { };
            //end add by nurul 20/1/2021, bundling 

            //while (daysFrom > -13)
            if (iden.webhook != "1")
            {
                while (daysFrom >= -7)
                {
                    //add 16 des 2020, fixed date
                    //var fromDt = (long)DateTimeOffset.UtcNow.AddDays(daysFrom).ToUnixTimeSeconds();
                    //var toDt = (long)DateTimeOffset.UtcNow.AddDays(daysTo).ToUnixTimeSeconds();
                    var fromDt = (long)daysNow.AddDays(daysFrom).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                    var toDt = (long)daysNow.AddDays(daysTo > 0 ? 0 : daysTo).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                    //end add 16 des 2020, fixed date

                    //change by nurul 20/1/2021, bundling 
                    //await GetOrderListCancel3days(iden, CUST, NAMA_CUST, 1, 0, fromDt, toDt);
                    var returnGetOrder = await GetOrderListCancel3days(iden, CUST, NAMA_CUST, 1, 0, fromDt, toDt, "");
                    //end change by nurul 20/1/2021, bundling
                    //daysFrom -= 3;
                    //daysTo -= 3;
                    daysFrom -= 2;
                    daysTo -= 2;

                    //add by nurul 20/1/2021, bundling 
                    //if (returnGetOrder == "1")
                    //{
                    //    AdaKomponen = true;
                    //    //tempConnId.Add(returnGetOrder);
                    //    //connIdProses += "'" + returnGetOrder + "' , ";
                    //}
                    if (!string.IsNullOrEmpty(returnGetOrder))
                    {
                        connIdProses += returnGetOrder;
                    }
                    //end add by nurul 20/1/2021, bundling 
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
            }
            else
            {
                await GetOrderListCancel_webhookON(iden, CUST, NAMA_CUST, page, jmlhOrder);
            }
            // add tuning no duplicate hangfire job get order
            var queryStatus = "\\\"}\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","\"000003\""
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + queryStatus + "%' and arguments like '%" + iden.API_secret_key + "%' and invocationdata like '%tokopedia%' and invocationdata like '%GetOrderListCancel%' and statename like '%Enque%' and invocationdata not like '%resi%'");
            // end add tuning no duplicate hangfire job get order
            
            return ret;
        }
        public async Task<string> GetOrderListCancel_webhookON(TokopediaAPIData iden, string CUST, string NAMA_CUST, int page, int jmlhOrder)
        {
            var daysFrom = -1;
            var daysTo = 1;
            var daysNow = DateTime.UtcNow.AddHours(7);
            var connIdProses = "";

            while (daysFrom >= -7)
            {
                var fromDt = (long)daysNow.AddDays(daysFrom).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                var toDt = (long)daysNow.AddDays(daysTo > 0 ? 0 : daysTo).ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                
                var returnGetOrder = await GetOrderListCancel3days(iden, CUST, NAMA_CUST, 1, 0, fromDt, toDt, "0");
                if (!string.IsNullOrEmpty(returnGetOrder))
                {
                    connIdProses += returnGetOrder;
                }

                returnGetOrder = await GetOrderListCancel3days(iden, CUST, NAMA_CUST, 1, 0, fromDt, toDt, "800");
                if (!string.IsNullOrEmpty(returnGetOrder))
                {
                    connIdProses += returnGetOrder;
                }

                returnGetOrder = await GetOrderListCancel3days(iden, CUST, NAMA_CUST, 1, 0, fromDt, toDt, "801");
                if (!string.IsNullOrEmpty(returnGetOrder))
                {
                    connIdProses += returnGetOrder;
                }

                returnGetOrder = await GetOrderListCancel3days(iden, CUST, NAMA_CUST, 1, 0, fromDt, toDt, "10");
                if (!string.IsNullOrEmpty(returnGetOrder))
                {
                    connIdProses += returnGetOrder;
                }

                returnGetOrder = await GetOrderListCancel3days(iden, CUST, NAMA_CUST, 1, 0, fromDt, toDt, "15");
                if (!string.IsNullOrEmpty(returnGetOrder))
                {
                    connIdProses += returnGetOrder;
                }

                daysFrom -= 2;
                daysTo -= 2;

            }
            if (!string.IsNullOrEmpty(connIdProses))
            {
                new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username, connIdProses.Substring(0, connIdProses.Length - 3));
            }

            return "";
        }

        //add by Tri 22 Jan 2020, cancel reason
        public async Task<string> GetCancelReason(TokopediaAPIData iden, string nobuk)
        {
            var ret = "";
            string connId = Guid.NewGuid().ToString();
            var token = SetupContext(iden);
            iden.token = token;
            long milis = CurrentTimeMillis();
            string urll = "https://fs.tokopedia.net/v2/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/order?invoice_num=" + nobuk;

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaSingleOrder)) as TokopediaSingleOrder;
                if (result != null)
                {
                    //if (result.header.error_code == 0)
                    {
                        if (result.data.cancel_request_info != null)
                        {
                            ret = result.data.cancel_request_info.reason.Replace('\'', '`');
                        }
                        else
                        {
                            if (result.data.order_info.order_history != null)
                            {
                                ret = result.data.order_info.order_history[0].comment.Replace('\'', '`');
                            }
                            else
                            {
                                ret = result.data.comment.Replace('\'', '`');
                            }
                        }
                    }
                }
            }
            return ret;
        }

        public async Task<TokopediaSingleOrder> GetCancelReasonV2(TokopediaAPIData iden, string nobuk)
        {
            var ret = new TokopediaSingleOrder();
            string connId = Guid.NewGuid().ToString();
            var token = SetupContext(iden);
            iden.token = token;
            long milis = CurrentTimeMillis();
            string urll = "https://fs.tokopedia.net/v2/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/order?invoice_num=" + nobuk;

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaSingleOrder)) as TokopediaSingleOrder;
                if (result != null)
                {
                    //if (result.header.error_code == 0)
                    //{
                    //    if (result.data.cancel_request_info != null)
                    //    {
                    //        ret = result.data.cancel_request_info.reason.Replace('\'', '`');
                    //    }
                    //    else
                    //    {
                    //        if (result.data.order_info.order_history != null)
                    //        {
                    //            ret = result.data.order_info.order_history[0].comment.Replace('\'', '`');
                    //        }
                    //        else
                    //        {
                    //            ret = result.data.comment.Replace('\'', '`');
                    //        }
                    //    }
                    //}
                    if(result.data != null)
                    {
                        ret = result;
                    }
                }
            }
            return ret;
        }
        //end add by Tri 22 Jan 2020, cancel reason

        //add by Tri 27 okt 2020, order detail
        public async Task<TokopediaSingleOrder> GetOrderDetail(TokopediaAPIData iden, string nobuk)
        {
            var ret = new TokopediaSingleOrder();
            string connId = Guid.NewGuid().ToString();
            var token = SetupContext(iden);
            iden.token = token;
            long milis = CurrentTimeMillis();
            string urll = "https://fs.tokopedia.net/v2/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/order?invoice_num=" + nobuk;

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaSingleOrder)) as TokopediaSingleOrder;
                if (result != null)
                {
                    return result;
                    //if (result.header.error_code == 0)
                    //{
                    //    if (result.data.cancel_request_info != null)
                    //    {
                    //        ret = result.data.cancel_request_info.reason.Replace('\'', '`');
                    //    }
                    //    else
                    //    {
                    //        if (result.data.order_info.order_history != null)
                    //        {
                    //            ret = result.data.order_info.order_history[0].comment.Replace('\'', '`');
                    //        }
                    //        else
                    //        {
                    //            ret = result.data.comment.Replace('\'', '`');
                    //        }
                    //    }
                    //}
                }
            }
            return ret;
        }
        //end add by Tri 27 okt 2020, order detail

        public async Task<BindingBase> GetItemListSemua(TokopediaAPIData iden, int page, int recordCount, string CUST, string NAMA_CUST, int recnumArf01)
        {
            var connId = Guid.NewGuid().ToString();
            BindingBase ret = new BindingBase();
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";

            long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
            string urll = "https://fs.tokopedia.net/v1/products/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/" + Convert.ToString(page + 1) + "/100";

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = milis.ToString(),
            //    REQUEST_ACTION = "Get Item List",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = stat.ToString(),
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ItemListResult)) as ItemListResult;
                bool adaError = false;
                //foreach (var item in result.data)
                //{
                //    if (item.stock > 0)
                //    {
                //        adaError = adaError;
                //    }
                //    else
                //    {
                //        adaError = adaError;

                //    }
                //}
                if (result.error_message != null)
                {
                    if (result.error_message.Count() > 0)
                    {
                        adaError = true;
                    }
                }
                if (!adaError)
                {
                    ret.message = (page + 1).ToString();
                    if (result.data.Count() < 100)
                    {
                        ret.message = "";
                    }
                    ret.status = 1;
                    ret.recordCount = recordCount;
                    List<TEMP_BRG_MP> listNewRecord = new List<TEMP_BRG_MP>();
                    var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.IDMARKET == recnumArf01).Select(t => new { t.CUST, t.BRG_MP }).ToList();
                    var brgInDB = ErasoftDbContext.STF02H.Where(t => t.IDMARKET == recnumArf01).Select(t => new { t.RecNum, t.BRG_MP }).ToList();
                    string brgMp = "";
                    foreach (var item in result.data)
                    {
                        brgMp = Convert.ToString(item.product_id);
                        if (item.status.ToUpper() != "DELETE")
                        {
                            var CektempbrginDB = tempbrginDB.Where(t => (t.BRG_MP ?? "").ToUpper().Equals(brgMp.ToUpper())).FirstOrDefault();
                            //var CekbrgInDB = brgInDB.Where(t => t.BRG_MP.Equals(brgMp)).FirstOrDefault();
                            var CekbrgInDB = brgInDB.Where(t => (t.BRG_MP ?? "").Equals(brgMp)).FirstOrDefault();
                            if (CektempbrginDB == null && CekbrgInDB == null)
                            {
                                string namaBrg = item.name;
                                string nama, nama2, nama3, urlImage, urlImage2, urlImage3;
                                urlImage = "";
                                urlImage2 = "";
                                urlImage3 = "";
                                if (namaBrg.Length > 30)
                                {
                                    nama = namaBrg.Substring(0, 30);
                                    //change by calvin 15 januari 2019
                                    //if (namaBrg.Length > 60)
                                    //{
                                    //    nama2 = namaBrg.Substring(30, 30);
                                    //    nama3 = (namaBrg.Length > 90) ? namaBrg.Substring(60, 30) : namaBrg.Substring(60);
                                    //}
                                    if (namaBrg.Length > 285)
                                    {
                                        nama2 = namaBrg.Substring(30, 255);
                                        nama3 = "";
                                    }
                                    //end change by calvin 15 januari 2019
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

                                Models.TEMP_BRG_MP newrecord = new TEMP_BRG_MP()
                                {
                                    SELLER_SKU = Convert.ToString(item.product_id),
                                    BRG_MP = Convert.ToString(item.product_id),
                                    NAMA = nama,
                                    NAMA2 = nama2,
                                    NAMA3 = nama3,
                                    CATEGORY_CODE = Convert.ToString(item.category_id),
                                    CATEGORY_NAME = "",
                                    IDMARKET = recnumArf01,
                                    IMAGE = "",
                                    DISPLAY = true,
                                    HJUAL = item.price,
                                    HJUAL_MP = item.price,
                                    Deskripsi = item.desc,
                                    MEREK = "OEM",
                                    CUST = CUST,
                                };
                                newrecord.AVALUE_45 = namaBrg.Length > 250 ? namaBrg.Substring(0, 250) : namaBrg; //request by Calvin 19 maret 2019, isi nama barang ke avalue 45
                                //add by Tri, 26 Feb 2019
                                var kategory = MoDbContext.CategoryTokped.Where(m => m.CATEGORY_CODE == newrecord.CATEGORY_CODE).FirstOrDefault();
                                if (kategory != null)
                                {
                                    newrecord.CATEGORY_NAME = kategory.CATEGORY_NAME;
                                }
                                //end add by Tri, 26 Feb 2019
                                listNewRecord.Add(newrecord);
                                ret.recordCount = ret.recordCount + 1;
                            }
                        }

                    }
                    if (listNewRecord.Count() > 0)
                    {
                        ErasoftDbContext.TEMP_BRG_MP.AddRange(listNewRecord);
                        ErasoftDbContext.SaveChanges();
                    }
                }
            }

            return ret;
        }


        public async Task<ItemListResult> GetItemList(TokopediaAPIData iden, string connId, string CUST, string NAMA_CUST, string product_id)
        {
            //if merchant code diisi. barulah GetOrderList
            var ret = new ItemListResult();
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";

            long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

            string urll = "https://fs.tokopedia.net/v1/products/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/1/100?product_id=" + Uri.EscapeDataString(product_id);

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = milis.ToString(),
            //    REQUEST_ACTION = "Get Item List",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = stat.ToString(),
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ItemListResult)) as ItemListResult;
                bool adaError = false;
                if (result.error_message != null)
                {
                    if (result.error_message.Count() > 0)
                    {
                        adaError = true;
                    }
                }
                if (!adaError)
                {
                    ret = result;
                }
            }

            return ret;
        }

        public async Task<string> UpdateStock(TokopediaAPIData iden, long product_id, int stok)
        {
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/stock/update?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);

            string responseFromServer = "";
            List<UpdateStockData> HttpBodies = new List<UpdateStockData>();
            UpdateStockData HttpBody = new UpdateStockData()
            {
                sku = "",
                product_id = product_id,
                new_stock = stok
            };
            HttpBodies.Add(HttpBody);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            string myData = JsonConvert.SerializeObject(HttpBodies);
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
            return "";
        }

        //change by nurul 12/2/2020, price (int)
        //public async Task<string> UpdatePrice(TokopediaAPIData iden, int product_id, float price)
        //end change by nurul 12/2/2020
        public async Task<string> UpdatePrice(TokopediaAPIData iden, long product_id, int price)
        {
            var token = SetupContext(iden);
            iden.token = token;

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/price/update?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);

            string responseFromServer = "";
            List<UpdatePriceData> HttpBodies = new List<UpdatePriceData>();
            UpdatePriceData HttpBody = new UpdatePriceData()
            {
                sku = "",
                product_id = product_id,
                new_price = price
            };
            HttpBodies.Add(HttpBody);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            string myData = JsonConvert.SerializeObject(HttpBodies);
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
            return "";
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Harga Jual Produk {obj} ke Tokopedia gagal.")]
        public async Task<string> UpdatePrice_Job(string dbPathEra, string kdbrgMO, string log_CUST, string log_ActionCategory, string log_ActionName, long product_id, TokopediaAPIData iden, int price)
        {
            //add 19 sept 2020, update harga massal
            if (log_ActionName.Contains("UPDATE_MASSAL"))
            {
                await Task.Delay(1000);//delay agar tidak terkena limit
            }
            //end add 19 sept 2020, update harga massal
            var token = SetupContext(iden);
            iden.token = token;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Price",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = Convert.ToString(product_id),
                REQUEST_ATTRIBUTE_2 = Convert.ToString(price),
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/price/update?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);

            string responseFromServer = "";
            List<UpdatePriceData> HttpBodies = new List<UpdatePriceData>();
            UpdatePriceData HttpBody = new UpdatePriceData()
            {
                sku = "",
                product_id = product_id,
                new_price = price
            };
            HttpBodies.Add(HttpBody);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            string myData = JsonConvert.SerializeObject(HttpBodies);
            try
            {
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
                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
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
                        throw new Exception(err);
                    }
                    throw e;
                }
                if (responseFromServer != "")
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(UpdatePriceResponse)) as UpdatePriceResponse;
                    if (result != null)
                    {
                        //if (result.header.error_code != 0)
                        //{
                        //    currentLog.REQUEST_EXCEPTION = (result.header.reason ?? result.header.messages);
                        //    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        //    throw new Exception(currentLog.REQUEST_EXCEPTION);
                        //}
                        //else
                        {
                            if (result.data != null)
                            {
                                if (result.data.failed_rows > 0)
                                {
                                    if (result.data.failed_rows_data.Length > 0)
                                    {
                                        var rowFailedMessage = "";
                                        foreach (var itemRow in result.data.failed_rows_data)
                                        {
                                            if (!string.IsNullOrEmpty(itemRow.message) && itemRow.product_id != 0)
                                            {
                                                rowFailedMessage = rowFailedMessage + Convert.ToString(itemRow.message) + " product id:" + Convert.ToString(itemRow.product_id) + ";";
                                            }
                                        }
                                        currentLog.REQUEST_EXCEPTION = "failed_rows_data:" + rowFailedMessage;
                                    }
                                    else
                                    {
                                        currentLog.REQUEST_EXCEPTION = responseFromServer;
                                    }
                                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                                    throw new Exception(currentLog.REQUEST_EXCEPTION);
                                }
                                else
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
                                            }
                                        }
                                    }
                                    //end add 19 sept 2020, update harga massal
                                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.Message.ToString();
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                throw new Exception(currentLog.REQUEST_EXCEPTION);
            }


            return "";
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Tokopedia Berhasil. Link Produk Gagal.")]
        public async Task<BindingBase> GetActiveItemListBySKU(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, int page, int recordCount, int recnumArf01, string SKU, string log_request_id)
        {
            var connId = Guid.NewGuid().ToString();

            var token = SetupContext(iden);
            iden.token = token;

            var found = false;
            var stop = false;
            page = 0;
            //change by Tri 23 apr 2020, get 50 data per page 
            //int rows = 100;
            int rows = 50;
            //end change by Tri 23 apr 2020, get 50 data per page 

            int Rowsstart = page * rows;
            BindingBase ret = new BindingBase();
            var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == log_CUST).FirstOrDefault();
            var brgInDB = ErasoftDbContext.STF02H.Where(m => m.BRG == SKU && m.IDMARKET == customer.RecNum).FirstOrDefault();
            while (!found && !stop)
            {
                Rowsstart = page * rows;

                ret.message = "";// jika perlu lanjut recursive, isi
                long milis = CurrentTimeMillis();
                DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
                string status = "";

                long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeSeconds();
                long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

                //string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/list?";
                string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/info?";

                string queryParam = "";
                //queryParam = "shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&rows=" + Uri.EscapeDataString(Convert.ToString(rows)) + "&start=" + Uri.EscapeDataString(Convert.ToString(Rowsstart)) + "&product_id=&order_by=9&keyword=&exclude_keyword=&sku=&price_min=1&price_max=500000000&preorder=false&free_return=false&wholesale=false";
                //queryParam = "shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&rows=" + Uri.EscapeDataString(Convert.ToString(rows)) + "&start=" + Uri.EscapeDataString(Convert.ToString(Rowsstart)) + "&order_by=9";
                queryParam = "shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&page=" + Uri.EscapeDataString(Convert.ToString(page)) + "&per_page=50&sort=2";
                string responseFromServer = "";
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + iden.token));
                HttpResponseMessage clientResponse = await client.GetAsync(
                    urll + queryParam);
                try
                {
                    using (HttpContent responseContent = clientResponse.Content)
                    {
                        using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                        {
                            responseFromServer = await reader.ReadToEndAsync();
                        }
                    };
                }
                catch (WebException e)
                {
                    string err = e.Message;
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
                    else
                    {
                        throw e;
                    }
                    throw new Exception(err);
                }

                if (!string.IsNullOrWhiteSpace(responseFromServer))
                {
                    //var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ActiveProductListResult)) as ActiveProductListResult;
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaController.TokpedGetListItemV2)) as TokopediaController.TokpedGetListItemV2;
                    //if (result.header.error_code == 0)
                    {
                        foreach (var item in result.data)
                        {
                            if (!found)
                            {
                                var createProduct = true;
                                var itemSKU = "";
                                if (item.other != null)
                                    itemSKU = item.other.sku;
                                var isValid = (itemSKU == SKU);
                                if (brgInDB != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(brgInDB.BRG_MP))
                                    {
                                        if (!brgInDB.BRG_MP.Contains("PENDING"))
                                        {
                                            createProduct = false;
                                            var brgmp = brgInDB.BRG_MP.Split(';');
                                            var kdBrg = brgmp[0];
                                            if (brgmp.Length > 1)
                                            {
                                                if (brgInDB.BRG_MP.Contains("EDIT"))
                                                {
                                                    kdBrg = brgmp[3];
                                                }
                                            }
                                            isValid = (item.basic.productID == Convert.ToInt64(kdBrg));
                                        }
                                    }
                                }
                                //if (item.sku == SKU)
                                if (isValid)
                                {
                                    found = true;
                                    if (item.variant != null)
                                    {
                                        var urlBrg = "";
                                        if (item.other != null)
                                        {
                                            urlBrg = item.other.url;
                                        }
                                        string Link_Error = "0;Buat Produk;;";//jobid;request_action;request_result;request_exception
                                        var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '"
                                            + Convert.ToString(item.basic.productID) + "',LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '"
                                            + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '" + Link_Error
                                            + "', AVALUE_34 = '" + urlBrg
                                            + "' WHERE BRG = '" + Convert.ToString(SKU) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");

                                        if (item.variant.isVariant)
                                        {
#if (DEBUG || Debug_AWS)
                                            await new TokopediaControllerJob().GetActiveItemVariantByProductID(iden.DatabasePathErasoft, SKU, log_CUST, "Barang", "Link Variasi Produk", iden, SKU, recnumArf01, Convert.ToString(item.basic.productID), log_request_id);
#else
                                            //change by calvin 9 juni 2019
                                            //await GetActiveItemVariantByProductID(iden, SKU, recnumArf01, Convert.ToString(item.id));
                                            string EDBConnID = EDB.GetConnectionString("ConnId");
                                            var sqlStorage = new SqlServerStorage(EDBConnID);

                                            var Jobclient = new BackgroundJobClient(sqlStorage);
                                            Jobclient.Enqueue<TokopediaControllerJob>(x => x.GetActiveItemVariantByProductID(iden.DatabasePathErasoft, SKU, log_CUST, "Barang", "Link Variasi Produk", iden, SKU, recnumArf01, Convert.ToString(item.basic.productID), log_request_id));
                                            //end change by calvin 9 juni 2019
#endif
                                        }
                                        else
                                        {
                                            //var urlBrg = "";
                                            //if(item.other != null)
                                            //{
                                            //    urlBrg = item.other.url;
                                            //}
                                            //string Link_Error = "0;Buat Produk;;";//jobid;request_action;request_result;request_exception
                                            //var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" 
                                            //    + Convert.ToString(item.basic.productID) + "',LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" 
                                            //    + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '" + Link_Error 
                                            //    + "', AVALUE_34 = '" + urlBrg
                                            //    + "' WHERE BRG = '" + Convert.ToString(SKU) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                                            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                                            {
                                                REQUEST_ID = log_request_id
                                            };
                                            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);

                                            //add by Tri 10 Jan 2019, update stok setelah create product sukses 
                                            //var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == log_CUST).FirstOrDefault();
                                            //if (createProduct)//remark 29 des 2020, update stok setelah update juga
                                            if (customer != null)
                                                {
                                                    if (customer.TIDAK_HIT_UANG_R)
                                                    {
                                                        //StokControllerJob.TokopediaAPIData data = new StokControllerJob.TokopediaAPIData()
                                                        //{
                                                        //    merchant_code = iden.merchant_code, //FSID
                                                        //    API_client_password = iden.API_client_password, //Client ID
                                                        //    API_client_username = iden.API_client_username, //Client Secret
                                                        //    API_secret_key = iden.API_secret_key, //Shop ID 
                                                        //    token = iden.token,
                                                        //    idmarket = iden.idmarket
                                                        //};
                                                        StokControllerJob.TokopediaAPIData data = new StokControllerJob.TokopediaAPIData()
                                                        {
                                                            //merchant_code = iden.merchant_code, //FSID
                                                            //API_client_password = iden.API_client_password, //Client ID
                                                            //API_client_username = iden.API_client_username, //Client Secret
                                                            //API_secret_key = iden.API_secret_key, //Shop ID 
                                                            //token = iden.token,
                                                            //idmarket = iden.idmarket
                                                        };
                                                        data.merchant_code = iden.merchant_code; //FSID
                                                        data.API_client_password = iden.API_client_password; //Client ID
                                                        data.API_client_username = iden.API_client_username; //Client Secret
                                                        data.API_secret_key = iden.API_secret_key; //Shop ID 
                                                        data.token = iden.token;
                                                        data.idmarket = iden.idmarket;

                                                        StokControllerJob stokAPI = new StokControllerJob(dbPathEra, username);
#if (DEBUG || Debug_AWS)
                                                        Task.Run(() => stokAPI.Tokped_updateStock(dbPathEra, kodeProduk, log_CUST, "Stock", "Update Stok", data, Convert.ToInt64(item.basic.productID), 0, username, null)).Wait();
#else
                                            string EDBConnID = EDB.GetConnectionString("ConnId");
                                            var sqlStorage = new SqlServerStorage(EDBConnID);

                                            var Jobclient = new BackgroundJobClient(sqlStorage);
                                            Jobclient.Enqueue<StokControllerJob>(x => x.Tokped_updateStock(dbPathEra, kodeProduk, log_CUST, "Stock", "Update Stok", data, Convert.ToInt64(item.basic.productID), 0, username, null));
#endif
                                                }
                                            }
                                            //end add by Tri 10 Jan 2019, update stok setelah create product sukses
                                        }
                                    }
                                    else
                                    {
                                        var urlBrg = "";
                                        if (item.other != null)
                                        {
                                            urlBrg = item.other.url;
                                        }
                                        string Link_Error = "0;Buat Produk;;";//jobid;request_action;request_result;request_exception
                                        var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" 
                                            + Convert.ToString(item.basic.productID) + "',LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" 
                                            + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '" + Link_Error
                                            + "', AVALUE_34 = '" + urlBrg
                                            + "' WHERE BRG = '" + Convert.ToString(SKU) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                                        MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                                        {
                                            REQUEST_ID = log_request_id
                                        };
                                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);

                                        //add by Tri 21 Jan 2019, update stok setelah create product sukses  
                                        //var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == log_CUST).FirstOrDefault();
                                        //if (createProduct)//remark 29 des 2020, update stok setelah update juga
                                            if (customer != null)
                                            {
                                                if (customer.TIDAK_HIT_UANG_R)
                                                {
                                                    //try
                                                    //{
                                                    StokControllerJob.TokopediaAPIData data = new StokControllerJob.TokopediaAPIData()
                                                    {
                                                        //merchant_code = iden.merchant_code, //FSID
                                                        //API_client_password = iden.API_client_password, //Client ID
                                                        //API_client_username = iden.API_client_username, //Client Secret
                                                        //API_secret_key = iden.API_secret_key, //Shop ID 
                                                        //token = iden.token,
                                                        //idmarket = iden.idmarket
                                                    };
                                                    data.merchant_code = iden.merchant_code; //FSID
                                                    data.API_client_password = iden.API_client_password; //Client ID
                                                    data.API_client_username = iden.API_client_username; //Client Secret
                                                    data.API_secret_key = iden.API_secret_key; //Shop ID 
                                                    data.token = iden.token;
                                                    data.idmarket = iden.idmarket;
                                                    StokControllerJob stokAPI = new StokControllerJob(dbPathEra, username);
#if (DEBUG || Debug_AWS)
                                                    Task.Run(() => stokAPI.Tokped_updateStock(dbPathEra, kodeProduk, log_CUST, "Stock", "Update Stok", data, Convert.ToInt64(item.basic.productID), 0, username, null)).Wait();
#else
                                            string EDBConnID = EDB.GetConnectionString("ConnId");
                                            var sqlStorage = new SqlServerStorage(EDBConnID);

                                            var Jobclient = new BackgroundJobClient(sqlStorage);
                                            Jobclient.Enqueue<StokControllerJob>(x => x.Tokped_updateStock(dbPathEra, kodeProduk, log_CUST, "Stock", "Update Stok", data, Convert.ToInt64(item.basic.productID), 0, username, null));
#endif
                                                //}
                                                //catch (Exception ex)
                                                //{

                                                //}


                                            }
                                        }
                                        //end add by Tri 21 Jan 2019, update stok setelah create product sukses
                                    }
                                }
                            }
                        }

                        //if (rows > result.data.Count())
                        if (result.data.Count() <= 0)//change 16 sept 2020, ambil data next page sampai return 0 karena ada bugs di tokped
                        {
                            stop = true;
                        }
                    }
                }

                page++;
            }


            return ret;
        }

        public async Task<BindingBase> GetActiveItemList(TokopediaAPIData iden, int page, int recordCount, string CUST, string NAMA_CUST, int recnumArf01)
        {
            var connId = Guid.NewGuid().ToString();

            BindingBase ret = new BindingBase();
            ret.message = "";// jika perlu lanjut recursive, isi
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";

            long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
            int rows = 10;
            int Rowsstart = page * rows;
            //order by name
            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/list?";
            string queryParam = "shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&rows=" + Uri.EscapeDataString(Convert.ToString(rows)) + "&start=" + Uri.EscapeDataString(Convert.ToString(Rowsstart)) + "&product_id=&order_by=12&keyword=&exclude_keyword=&sku=&price_min=1&price_max=500000000&preorder=false&free_return=false&wholesale=false";
            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = milis.ToString(),
            //    REQUEST_ACTION = "Get Item List",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = stat.ToString(),
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);


            string responseFromServer = "";
            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + iden.token));
            //HttpResponseMessage clientResponse = await client.GetAsync(
            //    urll + queryParam);

            //using (HttpContent responseContent = clientResponse.Content)
            //{
            //    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
            //    {
            //        responseFromServer = await reader.ReadToEndAsync();
            //    }
            //};

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + queryParam);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ActiveProductListResult)) as ActiveProductListResult;
                if (result.header.error_code == 0)
                {
                    ret.message = (page + 1).ToString();
                    if (Rowsstart + rows >= result.data.total_data)
                    {
                        ret.message = "";
                    }
                    ret.status = 1;
                    ret.recordCount = recordCount;
                    foreach (var item in result.data.products)
                    {
                        bool adaChild = false;
                        if (item.childs != null)
                        {
                            if (item.childs.Count() > 0)
                            {
                                adaChild = true;
                                await GetActiveItemVariant(iden, connId, CUST, NAMA_CUST, recnumArf01, item, ret);
                            }
                        }
                        if (!adaChild)
                        {
                            string namaBrg = item.name;
                            string nama, nama2, nama3, urlImage, urlImage2, urlImage3;
                            urlImage = "";
                            urlImage2 = "";
                            urlImage3 = "";
                            if (namaBrg.Length > 30)
                            {
                                nama = namaBrg.Substring(0, 30);
                                //change by calvin 15 januari 2019
                                //if (namaBrg.Length > 60)
                                //{
                                //    nama2 = namaBrg.Substring(30, 30);
                                //    nama3 = (namaBrg.Length > 90) ? namaBrg.Substring(60, 30) : namaBrg.Substring(60);
                                //}
                                if (namaBrg.Length > 285)
                                {
                                    nama2 = namaBrg.Substring(30, 255);
                                    nama3 = "";
                                }
                                //end change by calvin 15 januari 2019
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

                            var desc = await GetItemList(iden, connId, CUST, NAMA_CUST, Convert.ToString(item.id));

                            string description = "";
                            double price = 0;
                            foreach (var dtail in desc.data)
                            {
                                description = dtail.desc;
                                price = dtail.price;
                            }
                            Models.TEMP_BRG_MP newrecord = new TEMP_BRG_MP()
                            {
                                SELLER_SKU = item.sku,
                                BRG_MP = Convert.ToString(item.id),
                                NAMA = nama,
                                NAMA2 = nama2,
                                NAMA3 = nama3,
                                CATEGORY_CODE = Convert.ToString(item.category_id),
                                CATEGORY_NAME = item.category_name,
                                IDMARKET = recnumArf01,
                                IMAGE = item.image_url_700,
                                DISPLAY = true,
                                HJUAL = price,
                                HJUAL_MP = price,
                                Deskripsi = description,
                                MEREK = "OEM",
                                CUST = CUST
                            };
                            ErasoftDbContext.TEMP_BRG_MP.Add(newrecord);
                            ErasoftDbContext.SaveChanges();
                            ret.recordCount = ret.recordCount + 1;
                        }
                    }
                }
            }

            return ret;
        }
        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Tokopedia Berhasil. Link Produk Gagal.")]
        public async Task<string> GetActiveItemVariantByProductID(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, string brg, int recnumArf01, string product_id, string log_request_id)
        {
            string ret = "";
            var token = SetupContext(iden);
            iden.token = token;
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";

            long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

            //order by name
            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/variant/" + Uri.EscapeDataString(product_id);

            string responseFromServer = "";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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
                string err = e.Message;
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
                else
                {
                    throw e;
                }
                throw new Exception(err);
            }

            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ActiveProductVariantResult)) as ActiveProductVariantResult;
                //if (result.header.error_code == 0)
                {
                    var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == log_CUST).FirstOrDefault();
                    var createBrg = true;
                    var brgInDB = new STF02H();
                    if (customer != null)
                    {
                        brgInDB = ErasoftDbContext.STF02H.Where(m => m.BRG == brg && m.IDMARKET == customer.RecNum).FirstOrDefault();
                        if (brgInDB != null)
                        {
                            if (!string.IsNullOrWhiteSpace(brgInDB.BRG_MP))
                            {
                                if (!brgInDB.BRG_MP.Contains("PENDING"))
                                {
                                    createBrg = false;
                                }
                            }
                        }

                    }
                    List<TEMP_BRG_MP> listNewData = new List<TEMP_BRG_MP>();
                    string Link_Error = "0;Buat Produk;;";//jobid;request_action;request_result;request_exception
                    var success_induk = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(product_id) + "',LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '" + Link_Error + "' WHERE BRG = '" + Convert.ToString(brg) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                    //var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == log_CUST).FirstOrDefault();
                    foreach (var item in result.data.children)
                    {
                        //change by Tri 3 nov 2020, selalu update brg mp tokped karena bisa berubah saat edit nama
                        //if (createBrg)
                        //{
                        //    var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(item.product_id) + "',LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '" + Link_Error + "' WHERE BRG = '" + Convert.ToString(item.sku) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");

                        //}
                        //else//saat update, isi kode brg mp jika sebelumnya kosong(tambah varian baru)
                        //{
                        //    var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(item.product_id) + "',LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '" + Link_Error + "' WHERE BRG = '" + Convert.ToString(item.sku) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "' AND ISNULL(BRG_MP, '') = ''");

                        //}
                        if (!string.IsNullOrWhiteSpace(item.sku))
                        {
                            var urlBrg = item.url;

                            var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" 
                                + Convert.ToString(item.product_id) + "',LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" 
                                + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '" + Link_Error 
                                + "' , AVALUE_34 = '" + urlBrg
                                + "' WHERE BRG = '" + Convert.ToString(item.sku) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                        }
                        //end change by Tri 3 nov 2020, selalu update brg mp tokped karena bisa berubah saat edit nama
                        //add by Tri 21 Jan 2019, update stok setelah create product sukses 
                        //if (createBrg || brgInDB.DISPLAY)//remark 29 des 2020, update stok setelah update juga
                        if (customer != null)
                            {
                                if (customer.TIDAK_HIT_UANG_R)
                                {
                                    StokControllerJob.TokopediaAPIData data = new StokControllerJob.TokopediaAPIData()
                                    {
                                        //merchant_code = iden.merchant_code, //FSID
                                        //API_client_password = iden.API_client_password, //Client ID
                                        //API_client_username = iden.API_client_username, //Client Secret
                                        //API_secret_key = iden.API_secret_key, //Shop ID 
                                        //token = iden.token,
                                        //idmarket = iden.idmarket
                                    };
                                    data.merchant_code = iden.merchant_code; //FSID
                                    data.API_client_password = iden.API_client_password; //Client ID
                                    data.API_client_username = iden.API_client_username; //Client Secret
                                    data.API_secret_key = iden.API_secret_key; //Shop ID 
                                    data.token = iden.token;
                                    data.idmarket = iden.idmarket;

                                    StokControllerJob stokAPI = new StokControllerJob(dbPathEra, username);
                                    var kdBrg = Convert.ToString(item.sku);
                                    var varianInDB = ErasoftDbContext.STF02H.Where(m => m.IDMARKET == customer.RecNum && m.BRG_MP == kdBrg).FirstOrDefault();
                                    if (varianInDB != null)
                                    {
                                        kdBrg = varianInDB.BRG;
                                    }
#if (DEBUG || Debug_AWS)
                                    Task.Run(() => stokAPI.Tokped_updateStock(dbPathEra, kdBrg, log_CUST, "Stock", "Update Stok", data, item.product_id, 0, username, null)).Wait();
#else
                                            string EDBConnID = EDB.GetConnectionString("ConnId");
                                            var sqlStorage = new SqlServerStorage(EDBConnID);

                                            var Jobclient = new BackgroundJobClient(sqlStorage);
                                            Jobclient.Enqueue<StokControllerJob>(x => x.Tokped_updateStock(dbPathEra, kdBrg, log_CUST, "Stock", "Update Stok", data, item.product_id, 0, username, null));
#endif
                                }
                            }
                        //end add by Tri 21 Jan 2019, update stok setelah create product sukses
                    }

                    MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                    {
                        REQUEST_ID = log_request_id
                    };
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }
        public async Task<string> GetActiveItemVariant(TokopediaAPIData iden, string connId, string CUST, string NAMA_CUST, int recnumArf01, ActiveProductListResultProduct parent, BindingBase retParent)
        {
            string ret = "";
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";

            long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

            //order by name
            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/variant/" + Uri.EscapeDataString(Convert.ToString(parent.id));

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = milis.ToString(),
            //    REQUEST_ACTION = "Get Item List",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = stat.ToString(),
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            string responseFromServer = "";
            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + iden.token));
            //HttpResponseMessage clientResponse = await client.GetAsync(
            //    urll + queryParam);

            //using (HttpContent responseContent = clientResponse.Content)
            //{
            //    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
            //    {
            //        responseFromServer = await reader.ReadToEndAsync();
            //    }
            //};

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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
                string err = e.Message;
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
                else
                {
                    throw e;
                }
                throw new Exception(err);
            }

            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ActiveProductVariantResult)) as ActiveProductVariantResult;
                //if (result.header.error_code == 0)
                {
                    List<TEMP_BRG_MP> listNewData = new List<TEMP_BRG_MP>();

                    foreach (var item in result.data.children)
                    {
                        string brgMp = Convert.ToString(item.product_id);
                        var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.ToUpper().Equals(brgMp.ToUpper()) && t.IDMARKET == recnumArf01).FirstOrDefault();
                        var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(brgMp) && t.IDMARKET == recnumArf01).FirstOrDefault();
                        if (tempbrginDB == null && brgInDB == null)
                        {
                            string namaBrg = item.name;
                            string nama, nama2, nama3, urlImage, urlImage2, urlImage3;
                            urlImage = "";
                            urlImage2 = "";
                            urlImage3 = "";
                            if (namaBrg.Length > 30)
                            {
                                nama = namaBrg.Substring(0, 30);
                                if (namaBrg.Length > 60)
                                {
                                    nama2 = namaBrg.Substring(30, 30);
                                    nama3 = (namaBrg.Length > 90) ? namaBrg.Substring(60, 30) : namaBrg.Substring(60);
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

                            var desc = await GetItemList(iden, connId, CUST, NAMA_CUST, Convert.ToString(parent.id));

                            string description = "";
                            foreach (var dtail in desc.data)
                            {
                                description = dtail.desc;
                            }
                            Models.TEMP_BRG_MP newrecord = new TEMP_BRG_MP()
                            {
                                SELLER_SKU = item.sku,
                                BRG_MP = Convert.ToString(item.product_id),
                                NAMA = nama,
                                NAMA2 = nama2,
                                NAMA3 = nama3,
                                CATEGORY_CODE = Convert.ToString(parent.category_id),
                                CATEGORY_NAME = parent.category_name,
                                IDMARKET = recnumArf01,
                                IMAGE = parent.image_url_700,
                                DISPLAY = item.is_buyable,
                                HJUAL = item.price,
                                HJUAL_MP = item.price,
                                Deskripsi = description,
                                MEREK = "OEM",
                                CUST = CUST
                            };
                            listNewData.Add(newrecord);
                            retParent.recordCount = retParent.recordCount + 1;
                        }
                    }
                    if (listNewData.Count() > 0)
                    {
                        ErasoftDbContext.TEMP_BRG_MP.AddRange(listNewData);
                        ErasoftDbContext.SaveChanges();
                    }
                }
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("2_get_token")]
        //public TokopediaToken GetToken(TokopediaAPIData data, bool syncData)
        public TokopediaToken GetToken(TokopediaAPIData data)
        {
            var ret = new TokopediaToken();

            SetupContextForGetToken(data);

            var arf01inDB = ErasoftDbContext.ARF01.Where(p => (p.RecNum ?? 0) == data.idmarket).SingleOrDefault();
            if (arf01inDB != null)
            {
                ret.access_token = arf01inDB.TOKEN;

                if (!string.IsNullOrWhiteSpace(arf01inDB.API_KEY))
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
                        string apiId = data.API_client_username + ":" + data.API_client_password;//<-- diambil dari profil API
                                                                                                 //string apiId = "36bc3d7bcc13404c9e670a84f0c61676:8a76adc52d144a9fa1ef4f96b59b7419";
                                                                                                 //apiId = "mta-api-sandbox:sandbox-secret-key";
                                                                                                 //string urll = "https://apisandbox.blibli.com/v2/oauth/token?grant_type=client_credentials";

                        string urll = "https://accounts.tokopedia.com/token";
                        //string urll = "https://accounts-staging.tokopedia.com";

                        HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                        myReq.Method = "POST";
                        myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(apiId))));
                        myReq.ContentType = "application/x-www-form-urlencoded";
                        myReq.Accept = "application/json";
                        string myData = "grant_type=client_credentials";
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
                        // nilai token yg diambil adalah access-token. setelah 24jam biasanya harus masuk ke refresh token. dan harus diambil lagi acces token yg baru
                        if (responseFromServer != "")
                        {
                            ret = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaToken)) as TokopediaToken;
                            if (ret.error == null)
                            {
                                arf01inDB.TOKEN = ret.access_token;
                                arf01inDB.REFRESH_TOKEN = ret.refresh_token + ";" + Convert.ToString(ret.expires_in) + ";" + Convert.ToString(currentTimeRequest);
                                arf01inDB.STATUS_API = "1";
                                data.token = ret.access_token;

                                ErasoftDbContext.SaveChanges();
                            }
                            else
                            {
                                arf01inDB.TOKEN = "";
                                arf01inDB.STATUS_API = "0";
                                ErasoftDbContext.SaveChanges();
                                data.token = "";
                            }
                        }
                        else
                        {
                            arf01inDB.TOKEN = "";
                            arf01inDB.STATUS_API = "0";
                            ErasoftDbContext.SaveChanges();
                            data.token = "";
                        }
                    }
                }
                else
                {
                    arf01inDB.TOKEN = "";
                    arf01inDB.STATUS_API = "0";
                    ErasoftDbContext.SaveChanges();
                    data.token = "";
                }
            }
            //}
            return ret;
        }
        //categoryAPIResult

        public async Task<string> GetCategoryTree(TokopediaAPIData data)
        {
            string ret = "";

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(data.merchant_code) + "/product/category";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + data.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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

            //if (responseFromServer != null)
            if (!string.IsNullOrEmpty(responseFromServer))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(categoryAPIResult)) as categoryAPIResult;
                if (string.IsNullOrEmpty(result.header.reason))
                {
                    if (result.data.categories.Count() > 0)
                    {
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
                                oCommand.CommandText = "INSERT INTO [CATEGORY_TOKPED] ([CATEGORY_CODE], [CATEGORY_NAME], [PARENT_CODE], [IS_LAST_NODE], [MASTER_CATEGORY_CODE]) VALUES (@CATEGORY_CODE, @CATEGORY_NAME, @PARENT_CODE, @IS_LAST_NODE, @MASTER_CATEGORY_CODE)";
                                //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                                oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
                                oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));
                                oCommand.Parameters.Add(new SqlParameter("@PARENT_CODE", SqlDbType.NVarChar, 50));
                                oCommand.Parameters.Add(new SqlParameter("@IS_LAST_NODE", SqlDbType.NVarChar, 1));
                                oCommand.Parameters.Add(new SqlParameter("@MASTER_CATEGORY_CODE", SqlDbType.NVarChar, 50));

                                try
                                {
                                    //oCommand.Parameters[0].Value = data.merchant_code;
                                    foreach (var item in result.data.categories) //foreach parent level top
                                    {
                                        oCommand.Parameters[0].Value = item.id;
                                        oCommand.Parameters[1].Value = item.name;
                                        oCommand.Parameters[2].Value = "";
                                        oCommand.Parameters[3].Value = item.child == null ? "1" : item.child.Count() == 0 ? "1" : "0";
                                        oCommand.Parameters[4].Value = "";
                                        if (oCommand.ExecuteNonQuery() == 1)
                                        {
                                            if ((item.child == null ? 0 : item.child.Count()) > 0)
                                            {
                                                RecursiveInsertCategory(oCommand, item.child, item.id, item.id, data);
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
                        //await GetAttributeList(data);
                    }
                }
            }

            return ret;
        }

        public List<GetEtalaseReturnEtalase> GetEtalase(TokopediaAPIData data)
        {
            List<GetEtalaseReturnEtalase> res = new List<GetEtalaseReturnEtalase>();
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(data.merchant_code) + "/product/etalase?shop_id=" + Uri.EscapeDataString(data.API_secret_key);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + data.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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
            if (responseFromServer != "")
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(GetEtalaseReturn)) as GetEtalaseReturn;
                res = result.data.etalase;
            }
            return res;
        }
        public async Task<string> GetAttribute(TokopediaAPIData data)
        {
            string ret = "";
            List<CATEGORY_TOKPED> categories = MoDbContext.Database.SqlQuery<CATEGORY_TOKPED>("SELECT * FROM CATEGORY_TOKPED WHERE IS_LAST_NODE = '1'").ToList();
            foreach (var category in categories)
            {
                long milis = CurrentTimeMillis();
                DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

                string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(data.merchant_code) + "/category/get_variant?cat_id=" + Uri.EscapeDataString(category.CATEGORY_CODE);
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "GET";
                myReq.Headers.Add("Authorization", ("Bearer " + data.token));
                myReq.Accept = "application/x-www-form-urlencoded";
                myReq.ContentType = "application/json";
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

                //if (responseFromServer != null)
                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(GetVariantResult)) as GetVariantResult;
                    if (string.IsNullOrEmpty(result.header.reason))
                    {
                        try
                        {
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
                                    var AttributeInDb = MoDbContext.AttributeTokped.ToList();
                                    //cek jika belum ada di database, insert
                                    var cari = AttributeInDb.Where(p => p.CATEGORY_CODE.ToUpper().Equals(category.CATEGORY_CODE));
                                    if (cari.Count() == 0)
                                    {
                                        oCommand.CommandType = CommandType.Text;
                                        oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));

                                        string sSQL = "INSERT INTO [ATTRIBUTE_TOKPED] ([CATEGORY_CODE], [CATEGORY_NAME],";
                                        string sSQLValue = ") VALUES (@CATEGORY_CODE, @CATEGORY_NAME,";
                                        string a = "";
                                        int i = 0;
                                        foreach (var attribs in result.data)
                                        {
                                            a = Convert.ToString(i + 1);
                                            sSQL += "[VARIANT_ID_" + a + "],[HAS_UNIT_" + a + "],[ANAME_" + a + "],[STATUS_" + a + "],";
                                            sSQLValue += "@VARIANT_ID_" + a + ",@HAS_UNIT_" + a + ",@ANAME_" + a + ",@STATUS_" + a + ",";
                                            oCommand.Parameters.Add(new SqlParameter("@VARIANT_ID_" + a, SqlDbType.Int));
                                            oCommand.Parameters.Add(new SqlParameter("@HAS_UNIT_" + a, SqlDbType.Int));
                                            oCommand.Parameters.Add(new SqlParameter("@ANAME_" + a, SqlDbType.NVarChar, 250));
                                            oCommand.Parameters.Add(new SqlParameter("@STATUS_" + a, SqlDbType.NVarChar, 1));

                                            a = Convert.ToString(i * 4 + 2);
                                            oCommand.Parameters[(i * 4) + 2].Value = "";
                                            oCommand.Parameters[(i * 4) + 3].Value = "";
                                            oCommand.Parameters[(i * 4) + 4].Value = "";
                                            oCommand.Parameters[(i * 4) + 5].Value = "";

                                            oCommand.Parameters[(i * 4) + 2].Value = attribs.variant_id;
                                            oCommand.Parameters[(i * 4) + 3].Value = attribs.has_unit;
                                            oCommand.Parameters[(i * 4) + 4].Value = attribs.name;
                                            oCommand.Parameters[(i * 4) + 5].Value = attribs.status;

                                            if (attribs.units.Count() > 0)
                                            {
                                                var AttributeUnitInDb = MoDbContext.AttributeUnitTokped.AsNoTracking().ToList();
                                                foreach (var unit in attribs.units)
                                                {
                                                    var cariUnit = AttributeUnitInDb.Where(p => p.UNIT_ID == unit.unit_id);
                                                    if (cariUnit.Count() == 0)
                                                    {
                                                        using (SqlCommand oCommand2 = oConnection.CreateCommand())
                                                        {
                                                            oCommand2.CommandType = CommandType.Text;
                                                            oCommand2.CommandText = "INSERT INTO ATTRIBUTE_UNIT_TOKPED ([VARIANT_ID], [UNIT_ID], [UNIT_NAME], [UNIT_SHORT_NAME]) VALUES (@VARIANT_ID, @UNIT_ID, @UNIT_NAME, @UNIT_SHORT_NAME)";
                                                            oCommand2.Parameters.Add(new SqlParameter("@VARIANT_ID", SqlDbType.Int));
                                                            oCommand2.Parameters.Add(new SqlParameter("@UNIT_ID", SqlDbType.Int));
                                                            oCommand2.Parameters.Add(new SqlParameter("@UNIT_NAME", SqlDbType.NVarChar, 100));
                                                            oCommand2.Parameters.Add(new SqlParameter("@UNIT_SHORT_NAME", SqlDbType.NVarChar, 75));
                                                            oCommand2.Parameters[0].Value = attribs.variant_id;
                                                            oCommand2.Parameters[1].Value = unit.unit_id;
                                                            oCommand2.Parameters[2].Value = unit.name;
                                                            oCommand2.Parameters[3].Value = unit.short_name;
                                                            oCommand2.ExecuteNonQuery();
                                                        }
                                                    }
                                                }

                                                var AttributeOptInDb = MoDbContext.AttributeOptTokped.AsNoTracking().ToList();
                                                foreach (var unit in attribs.units)
                                                {
                                                    foreach (var opt in unit.values)
                                                    {
                                                        var cariOpt = AttributeOptInDb.Where(p => p.VARIANT_ID == attribs.variant_id && p.UNIT_ID == unit.unit_id && p.VALUE_ID == opt.value_id);
                                                        if (cariOpt.Count() == 0)
                                                        {
                                                            using (SqlCommand oCommand2 = oConnection.CreateCommand())
                                                            {
                                                                oCommand2.CommandType = CommandType.Text;
                                                                oCommand2.CommandText = "INSERT INTO ATTRIBUTE_OPT_TOKPED ([VALUE_ID], [UNIT_ID], [VALUE], [HEX_CODE], [ICON], [VARIANT_ID]) VALUES (@VALUE_ID, @UNIT_ID, @VALUE, @HEX_CODE, @ICON, @VARIANT_ID)";
                                                                oCommand2.Parameters.Add(new SqlParameter("@VALUE_ID", SqlDbType.Int));
                                                                oCommand2.Parameters.Add(new SqlParameter("@UNIT_ID", SqlDbType.Int));
                                                                oCommand2.Parameters.Add(new SqlParameter("@VALUE", SqlDbType.NVarChar, 50));
                                                                oCommand2.Parameters.Add(new SqlParameter("@HEX_CODE", SqlDbType.NVarChar, 50));
                                                                oCommand2.Parameters.Add(new SqlParameter("@ICON", SqlDbType.NVarChar, 200));
                                                                oCommand2.Parameters.Add(new SqlParameter("@VARIANT_ID", SqlDbType.Int));
                                                                oCommand2.Parameters[0].Value = opt.value_id;
                                                                oCommand2.Parameters[1].Value = unit.unit_id;
                                                                oCommand2.Parameters[2].Value = opt.value;
                                                                oCommand2.Parameters[3].Value = opt.hex_code;
                                                                oCommand2.Parameters[4].Value = opt.icon;
                                                                oCommand2.Parameters[5].Value = attribs.variant_id;
                                                                oCommand2.ExecuteNonQuery();
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            i = i + 1;
                                        }
                                        sSQL = sSQL.Substring(0, sSQL.Length - 1) + sSQLValue.Substring(0, sSQLValue.Length - 1) + ")";
                                        oCommand.CommandText = sSQL;
                                        oCommand.Parameters[0].Value = category.CATEGORY_CODE;
                                        oCommand.Parameters[1].Value = "";
                                        oCommand.ExecuteNonQuery();
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

            return ret;
        }

        public async Task<ManageController.GetAttributeTokpedReturn> GetAttributeToList(TokopediaAPIData iden, string category_CATEGORY_CODE)
        {
            var token = SetupContext(iden);
            iden.token = token;

            ManageController.GetAttributeTokpedReturn ret = new ManageController.GetAttributeTokpedReturn()
            {
                attribute = new List<ATTRIBUTE_TOKPED>(),
                attribute_opt = new List<ATTRIBUTE_OPT_TOKPED>(),
                attribute_unit = new List<ATTRIBUTE_UNIT_TOKPED>()
            };

            //List<CATEGORY_TOKPED> categories = MoDbContext.Database.SqlQuery<CATEGORY_TOKPED>("SELECT * FROM CATEGORY_TOKPED WHERE IS_LAST_NODE = '1'").ToList();
            //foreach (var category in categories)
            //{
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/category/get_variant?cat_id=" + Uri.EscapeDataString(category_CATEGORY_CODE);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
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

            //if (responseFromServer != null)
            if (!string.IsNullOrEmpty(responseFromServer))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(GetVariantResult)) as GetVariantResult;
                //if (string.IsNullOrEmpty(result.header.reason))
                //if (result.header.error_code == 0)
                {
                    try
                    {
                        string a = "";
                        int i = 0;
                        if (result.data != null)
                            foreach (var attribs in result.data)
                            {
                                a = Convert.ToString(i + 1);

                                ATTRIBUTE_TOKPED newRecord = new ATTRIBUTE_TOKPED();

                                newRecord["VARIANT_ID_" + a] = attribs.variant_id;
                                newRecord["HAS_UNIT_" + a] = attribs.has_unit;
                                newRecord["ANAME_" + a] = attribs.name;
                                newRecord["STATUS_" + a] = Convert.ToString(attribs.status);
                                ret.attribute.Add(newRecord);

                                if (attribs.units.Count() > 0)
                                {
                                    foreach (var unit in attribs.units)
                                    {
                                        ATTRIBUTE_UNIT_TOKPED newRecordUnit = new ATTRIBUTE_UNIT_TOKPED();
                                        newRecordUnit["VARIANT_ID"] = attribs.variant_id;
                                        newRecordUnit["UNIT_ID"] = unit.unit_id;
                                        newRecordUnit["UNIT_NAME"] = unit.name;
                                        newRecordUnit["UNIT_SHORT_NAME"] = unit.short_name;
                                        ret.attribute_unit.Add(newRecordUnit);
                                    }

                                    foreach (var unit in attribs.units)
                                    {
                                        if (unit.values != null)
                                        {
                                            foreach (var opt in unit.values)
                                            {
                                                ATTRIBUTE_OPT_TOKPED newRecordOpt = new ATTRIBUTE_OPT_TOKPED();
                                                newRecordOpt["VALUE_ID"] = opt.value_id;
                                                newRecordOpt["UNIT_ID"] = unit.unit_id;
                                                newRecordOpt["VALUE"] = opt.value;
                                                newRecordOpt["HEX_CODE"] = opt.hex_code;
                                                newRecordOpt["ICON"] = opt.icon;
                                                newRecordOpt["VARIANT_ID"] = attribs.variant_id;
                                                ret.attribute_opt.Add(newRecordOpt);
                                            }
                                        }
                                    }
                                }
                                i = i + 1;
                            }
                    }
                    catch (Exception ex2)
                    {

                    }
                }
            }
            //}

            return ret;
        }

        protected void RecursiveInsertCategory(SqlCommand oCommand, CategoryChild[] item_children, string parent, string master_category_code, TokopediaAPIData data)
        {
            foreach (var child in item_children)
            {
                oCommand.Parameters[0].Value = child.id;
                oCommand.Parameters[1].Value = child.name;
                oCommand.Parameters[2].Value = parent;
                oCommand.Parameters[3].Value = child.child == null ? "1" : child.child.Count() == 0 ? "1" : "0";
                oCommand.Parameters[4].Value = master_category_code;

                if (oCommand.ExecuteNonQuery() == 1)
                {
                    if ((child.child == null ? 0 : child.child.Count()) > 0)
                    {
                        RecursiveInsertCategory(oCommand, child.child, child.id, master_category_code, data);
                    }
                }
            }
        }

        //add by Tri 11 Nov 2019, cancel order
        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Status Cancel Pesanan {obj} ke Tokopedia Gagal.")]
        public async Task<BindingBase> SetStatusToCanceled(TokopediaAPIData data, string dbPathEra, string namaPemesan, string log_CUST, string log_ActionCategory, string log_ActionName, string orderId, string uname, string cancelReason)
        {
            var ret = new BindingBase();
            ret.status = 0;
            var MoDbContext = new MoDbContext();
            //var ErasoftDbContext = new ErasoftContext(dbPathEra);
            //var EDB = new DatabaseSQL(dbPathEra);
            var token = SetupContext(data);
            data.token = token;

            var username = uname;
            string myData = "";
            var spltReason = cancelReason.Split(';');
            if (spltReason.Count() == 2)
            {
                myData = "{ \"reason_code\":" + spltReason[0] + ", \"reason\": \"" + spltReason[1] + "\" }";
            }
            else if (spltReason.Count() == 3)
            {
                myData = "{ \"reason_code\":" + spltReason[0] + ", \"reason\": \"" + spltReason[2] + "\" }";
            }
            else if (spltReason.Count() == 4)
            {
                myData = "{ \"reason_code\":" + spltReason[0] + ", \"reason\": \"" + spltReason[1] + "\",";
                myData += "\"shop_close_end_date\": \"" + (Convert.ToDateTime(spltReason[3])).ToString("dd/MM/yyyy") + "\", \"shop_close_note\": \"" + spltReason[2] + "\" }";
            }
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            List<UpdatePriceData> HttpBodies = new List<UpdatePriceData>();

            //UpdatePriceData HttpBody = new UpdatePriceData()
            //{
            //    sku = "",
            //    product_id = product_id,
            //    new_price = price
            //};
            //HttpBodies.Add(HttpBody);

            //string myData = JsonConvert.SerializeObject(HttpBodies);
            var ordID = orderId.Split(';');
            string urll = "https://fs.tokopedia.net/v1/order/" + Uri.EscapeDataString(ordID[0]) + "/fs/" + Uri.EscapeDataString(data.merchant_code) + "/nack";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", ("Bearer " + data.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            //try
            //{
            //using (WebResponse response = myReq.GetResponse())
            //{
            //    using (Stream stream = response.GetResponseStream())
            //    {
            //        StreamReader reader = new StreamReader(stream);
            //        responseFromServer = reader.ReadToEnd();
            //    }
            //}
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
            //catch (WebException e)
            //{
            //    string err = "";
            //    //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //    if (e.Status == WebExceptionStatus.ProtocolError)
            //    {
            //        WebResponse resp = e.Response;
            //        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
            //        {
            //            err = sr.ReadToEnd();
            //        }
            //    }
            //}
            if (responseFromServer != "")
            {
                //var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(GetEtalaseReturn)) as GetEtalaseReturn;
                //res = result.data.etalase;
            }

            return ret;

        }
        //end add by Tri 11 Nov 2019, cancel order

        //add by nurul 20/9/2021
        public async Task<string> ListMessage(TokopediaAPIData iden, string filter, int page)
        {
            string ret = "";
            string connId = Guid.NewGuid().ToString();
            var token = SetupContext(iden);
            iden.token = token;
            //filter: �all�, �read�, or �unread�.
            string urll = "https://fs.tokopedia.net/v1/chat/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/messages?shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&page=" + page + "&per_page=15&order=desc&filter=" + Uri.EscapeDataString(filter);


            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";


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

            //if (responseFromServer != null)
            if (!string.IsNullOrEmpty(responseFromServer))
            {
                try
                {
                    resultListMessage result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(resultListMessage)) as resultListMessage;
                    if (result.header.error_code == 0)
                    {
                        var listMessage = new List<TOKPED_LISTMESSAGE>() { };
                        if (result.data.Count() > 0)
                        {
                            var dateNow = DateTime.UtcNow.AddHours(7);
                            //var dateLast1Month = dateNow.AddMonths(-1);
                            var dateLast1Month = dateNow.AddDays(-14);
                            var cust = ErasoftDbContext.ARF01.Where(a => a.API_KEY == iden.API_secret_key && a.Sort1_Cust == iden.merchant_code).FirstOrDefault();
                            if (cust != null)
                            {
                                var cekListMessage = ErasoftDbContext.TOKPED_LISTMESSAGE.Select(a => a.msg_id).ToList();
                                var lastGetMessage = false;
                                while (!lastGetMessage)
                                {
                                    foreach (var msg in result.data)
                                    {
                                        var ax = DateTimeOffset.FromUnixTimeMilliseconds(msg.attributes.last_reply_time).UtcDateTime.AddHours(7);
                                        var message = new TOKPED_LISTMESSAGE
                                        {
                                            CUST = cust.CUST,
                                            msg_id = msg.msg_id.ToString(),
                                            contact_id = msg.attributes.contact.id.ToString(),
                                            contact_role = msg.attributes.contact.role,
                                            attributes_name = msg.attributes.contact.attributes.name,
                                            attributes_tag = msg.attributes.contact.attributes.tag,
                                            attributes_thumbnail = msg.attributes.contact.attributes.thumbnail + "desktop",
                                            last_reply_msg = msg.attributes.last_reply_msg,
                                            //last_reply_time = Convert.ToDateTime(msg.attributes.last_reply_time),
                                            last_reply_time = ax,
                                            read_status = msg.attributes.read_status,
                                            unreads = msg.attributes.unreads,
                                            pin_status = msg.attributes.pin_status,
                                            tglinput = DateTime.UtcNow.AddHours(7),
                                            shop_id = iden.API_secret_key
                                        };
                                        //masukin sampe -1 bulan 
                                        if (message.last_reply_time < dateLast1Month && msg.attributes.pin_status == 0)
                                        {
                                            lastGetMessage = true; break;
                                        }
                                        //hanya masukin yg blm ada di list message 
                                        //var cekExistingHeader = ErasoftDbContext.TOKPED_LISTMESSAGE.Where(a => a.shop_id == cust.API_KEY && a.CUST == cust.CUST && a.last_reply_time >= dateLast1Month && a.msg_id == message.msg_id).ToList();
                                        //if (cekExistingHeader.Count() > 0)
                                        //{
                                        //    ErasoftDbContext.TOKPED_LISTMESSAGE.RemoveRange(cekExistingHeader);
                                        //}
                                        var cekExistingDetail = ErasoftDbContext.TOKPED_LISTCHAT.Where(a => a.shop_id == cust.API_KEY && a.CUST == cust.CUST && a.reply_time >= dateLast1Month && a.msg_id == message.msg_id).ToList();
                                        if (cekExistingDetail.Count() > 0)
                                        {
                                            ErasoftDbContext.TOKPED_LISTCHAT.RemoveRange(cekExistingDetail);
                                        }
                                        if (!cekListMessage.Contains(message.msg_id))
                                        {
                                            listMessage.Add(message);
                                            ErasoftDbContext.SaveChanges();
                                            //var cekListChat = ErasoftDbContext.TOKPED_LISTCHAT.AsNoTracking().Where(b => b.msg_id == message.msg_id && b.CUST == cust.CUST).Count();
                                            //if (cekListChat == 0)
                                            //{
                                            //await ListReply(iden, message.msg_id, 1);
                                            Task.Run(() => ListReply(iden, message.msg_id, 1)).Wait();
                                            //}
                                        }
                                        else
                                        {
                                            var getConversation = ErasoftDbContext.TOKPED_LISTMESSAGE.Where(a => a.msg_id == message.msg_id).FirstOrDefault();
                                            if(getConversation != null)
                                            {
                                                getConversation.last_reply_time = message.last_reply_time;
                                                getConversation.unreads = message.unreads;
                                                getConversation.last_reply_msg = message.last_reply_msg;
                                                ErasoftDbContext.SaveChanges();
                                                //await ListReply(iden, message.msg_id, 1);
                                                Task.Run(() => ListReply(iden, message.msg_id, 1)).Wait();
                                            }
                                        }
                                    }
                                    lastGetMessage = true; break;
                                }
                                if (listMessage.Count() > 0)
                                {
                                    ErasoftDbContext.TOKPED_LISTMESSAGE.AddRange(listMessage);
                                    ErasoftDbContext.SaveChanges();
                                }
                                if (!lastGetMessage)
                                {
                                    var nextMessage = await ListMessage(iden, filter, page + 1);
                                    //ret.AddRange(nextOrders);
                                }
                                
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return ret;
        }

        public async Task<string> ListReply(TokopediaAPIData iden, string msgId, int page)
        {
            string ret = "";
            string connId = Guid.NewGuid().ToString();
            var token = SetupContext(iden);
            iden.token = token;
            string urll = "https://fs.tokopedia.net/v1/chat/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/messages/" + Uri.EscapeDataString(msgId) + "/replies?shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&page=" + page + "&per_page=15&order=desc";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            myReq.Accept = "application/x-www-form-urlencoded";
            myReq.ContentType = "application/json";

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

            //if (responseFromServer != null)
            if (!string.IsNullOrEmpty(responseFromServer))
            {
                ResultListReply result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ResultListReply)) as ResultListReply;
                if (result.header.error_code == 0)
                {
                    var listChat = new List<TOKPED_LISTCHAT>() { };
                    if (result.data.Count() > 0)
                    {
                        var cust = ErasoftDbContext.ARF01.Where(a => a.API_KEY == iden.API_secret_key && a.Sort1_Cust == iden.merchant_code).FirstOrDefault();
                        if (cust != null)
                        {
                            var cekFirstReply = ErasoftDbContext.TOKPED_LISTCHAT.Where(a => a.CUST == cust.CUST && a.msg_id == msgId && a.is_first_reply == 1).FirstOrDefault();
                            var dateNow = DateTime.UtcNow.AddHours(7);
                            var dateLast1Month = dateNow.AddDays(-14);
                            var firstReply = false;
                            var cekListReply = ErasoftDbContext.TOKPED_LISTCHAT.ToList();
                            var replyid = cekListReply.Select(a => a.reply_id).ToList();
                            var lastGetMessage = false;
                            var temp_listChat = new List<TOKPED_LISTCHAT>() { };
                            var orderDesc = result.data.OrderByDescending(a => a.reply_time).ToList();
                            while (!lastGetMessage)
                            {
                                //foreach (var msg in result.data)
                                foreach (var msg in orderDesc)
                                {
                                    try
                                    {
                                        var ax = DateTimeOffset.FromUnixTimeMilliseconds(msg.reply_time).UtcDateTime.AddHours(7);
                                        var bx = DateTimeOffset.FromUnixTimeMilliseconds(msg.read_time).UtcDateTime.AddHours(7);
                                        //var bx = DateTime.UtcNow.AddHours(7);
                                        //if(msg.read_time != 0)
                                        //{
                                        //    bx = DateTimeOffset.FromUnixTimeMilliseconds(msg.read_time).UtcDateTime.AddHours(7);
                                        //}
                                        //if (cekListReply.Count() == 0 || !replyid.Contains(msg.reply_id.ToString()))
                                        //{
                                        var message = new TOKPED_LISTCHAT
                                        {
                                            CUST = cust.CUST,
                                            msg_id = msg.msg_id.ToString(),
                                            reply_id = msg.reply_id.ToString(),
                                            sender_id = msg.sender_id.ToString(),
                                            sender_name = msg.sender_name,
                                            msg = msg.msg,
                                            reply_time = ax,
                                            read_status = msg.read_status,
                                            read_time = bx,
                                            status_ = msg.status,
                                            //message_is_read = msg.message_is_read.ToString(),
                                            //is_opposite = msg.is_opposite.ToString(),
                                            //is_first_reply = msg.is_first_reply.ToString(),
                                            //is_reported = msg.is_reported.ToString(),

                                            tglinput = DateTime.UtcNow.AddHours(7),
                                            shop_id = iden.API_secret_key
                                        };
                                        if (msg.message_is_read)
                                        {
                                            message.message_is_read = 1;
                                        }
                                        else
                                        {
                                            message.message_is_read = 0;
                                        }
                                        if (msg.is_opposite)
                                        {
                                            message.is_opposite = 1;
                                        }
                                        else
                                        {
                                            message.is_opposite = 0;
                                        }
                                        if (msg.is_first_reply)
                                        {
                                            message.is_first_reply = 1;
                                            firstReply = true;
                                        }
                                        else
                                        {
                                            message.is_first_reply = 0;
                                        }
                                        if (msg.is_reported)
                                        {
                                            message.is_reported = 1;
                                        }
                                        else
                                        {
                                            message.is_reported = 0;
                                        }
                                        if (msg.attachment != null)
                                        {
                                            message.attachment_id = string.IsNullOrEmpty(Convert.ToString(msg.attachment_id)) ? "" : Convert.ToString(msg.attachment_id);
                                            message.attachment_type = msg.attachment.type;
                                            if (msg.attachment.attributes != null)
                                            {
                                                message.image_url = string.IsNullOrEmpty(msg.attachment.attributes.image_url) ? "" : msg.attachment.attributes.image_url;
                                                message.product_id = string.IsNullOrEmpty(msg.attachment.attributes.product_id.ToString()) ? "" : msg.attachment.attributes.product_id.ToString();
                                            }
                                            if (msg.attachment.fallback_attachment != null)
                                            {
                                                message.fallback_attachment_html = string.IsNullOrEmpty(msg.attachment.fallback_attachment.html) ? "" : msg.attachment.fallback_attachment.html;
                                                message.fallback_attachment_message = string.IsNullOrEmpty(msg.attachment.fallback_attachment.message) ? "" : msg.attachment.fallback_attachment.message;
                                            }
                                        }

                                        //masukin sampe -1 bulan 
                                        //if (message.reply_time < dateLast1Month)
                                        //{
                                        //    firstReply = true;
                                        //    message.is_first_reply = 1;
                                        //    listChat.Add(message);
                                        //    lastGetMessage = true; break;
                                        //}
                                        //else
                                        //{
                                        //    listChat.Add(message);
                                        //}

                                        if (message.reply_time < dateLast1Month)
                                        {
                                            if(cekFirstReply != null)
                                            {
                                                message.is_first_reply = 1;
                                            }
                                            firstReply = true;
                                            var cekExist = ErasoftDbContext.TOKPED_LISTCHAT.Where(a => a.CUST == message.CUST && a.msg_id == message.msg_id && a.msg == message.msg && a.attachment_id == message.attachment_id && a.attachment_type == a.attachment_type && a.product_id == message.product_id).Count();
                                            if(cekExist == 0)
                                            {
                                                listChat.Add(message);
                                            }
                                            lastGetMessage = true; break;
                                        }
                                        else
                                        {
                                            listChat.Add(message);
                                        }
                                        //listChat.Add(message);

                                        if (firstReply)
                                        {
                                            lastGetMessage = true; break;
                                        }
                                        //}
                                        //else
                                        //{
                                        //    var cekreply = cekListReply.Where(a => a.reply_id == msg.reply_id.ToString()).FirstOrDefault();
                                        //    if (cekreply != null)
                                        //    {
                                        //        cekreply.CUST = cust.CUST;
                                        //        cekreply.msg_id = msg.msg_id.ToString();
                                        //        cekreply.reply_id = msg.reply_id.ToString();
                                        //        cekreply.sender_id = msg.sender_id.ToString();
                                        //        cekreply.sender_name = msg.sender_name;
                                        //        cekreply.msg = msg.msg.ToString();
                                        //        cekreply.reply_time = ax;
                                        //        cekreply.read_status = msg.read_status;
                                        //        cekreply.read_time = bx;
                                        //        cekreply.status_ = msg.status;
                                        //        if (msg.attachment != null)
                                        //        {
                                        //            cekreply.attachment_id = string.IsNullOrEmpty(Convert.ToString(msg.attachment_id)) ? "" : Convert.ToString(msg.attachment_id);
                                        //            cekreply.attachment_type = msg.attachment.type;
                                        //            if (msg.attachment.attributes != null)
                                        //            {
                                        //                cekreply.image_url = string.IsNullOrEmpty(msg.attachment.attributes.image_url) ? "" : msg.attachment.attributes.image_url;
                                        //                cekreply.product_id = string.IsNullOrEmpty(msg.attachment.attributes.product_id.ToString()) ? "" : msg.attachment.attributes.product_id.ToString();
                                        //            }
                                        //            if (msg.attachment.fallback_attachment != null)
                                        //            {
                                        //                cekreply.fallback_attachment_html = string.IsNullOrEmpty(msg.attachment.fallback_attachment.html) ? "" : msg.attachment.fallback_attachment.html;
                                        //                cekreply.fallback_attachment_message = string.IsNullOrEmpty(msg.attachment.fallback_attachment.message) ? "" : msg.attachment.fallback_attachment.message;
                                        //            }
                                        //        }
                                        //        cekreply.tglinput = DateTime.UtcNow.AddHours(7);
                                        //        cekreply.shop_id = iden.API_secret_key;
                                        //        if (msg.message_is_read)
                                        //        {
                                        //            cekreply.message_is_read = 1;
                                        //        }
                                        //        if (msg.is_opposite)
                                        //        {
                                        //            cekreply.is_opposite = 1;
                                        //        }
                                        //        if (msg.is_first_reply)
                                        //        {
                                        //            cekreply.is_first_reply = 1;
                                        //            firstReply = true;
                                        //        }
                                        //        if (msg.is_reported)
                                        //        {
                                        //            cekreply.is_reported = 1;
                                        //        }

                                        //    }

                                        //    //masukin sampe -1 bulan 
                                        //    if (ax < dateLast1Month)
                                        //    {
                                        //        cekreply.is_first_reply = 1;
                                        //        firstReply = true;
                                        //        lastGetMessage = true; break;
                                        //    }

                                        //    if (firstReply) break;
                                        //}
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }

                                lastGetMessage = true;break;
                            }
                            if (listChat.Count() > 0)
                            {
                                ErasoftDbContext.TOKPED_LISTCHAT.AddRange(listChat);
                                ErasoftDbContext.SaveChanges();
                            }
                            if (!firstReply && result.data.Count() == 15)
                            {
                                var nextReply = await ListReply(iden, msgId, page + 1);
                            }
                        }
                    }
                }
            }
            return ret;
        }
        //end add by nurul 20/9/2021

        public enum StatusOrder
        {
            Cancel = 1,
            Paid = 2,
            PackagingINP = 3,
            ShippingINP = 4,
            Completed = 5
        }
        protected enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4,
            RePending = 5,
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
        protected void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, TokopediaAPIData iden, API_LOG_MARKETPLACE data)
        {
            try
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
                                MARKETPLACE = "Tokopedia",
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
                    case api_status.RePending:
                        {
                            var apiLogInDb = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ID == data.REQUEST_ID).SingleOrDefault();
                            if (apiLogInDb != null)
                            {
                                apiLogInDb.REQUEST_STATUS = "Pending";
                                ErasoftDbContext.SaveChanges();
                            }
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
            catch (Exception ex)
            {

            }
        }
        public static long CurrentTimeMillis()
        {
            //        return (long)DateTime.Now.ToUniversalTime().Subtract(
            //new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            //).TotalMilliseconds;
            return (long)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        public class TokopediaAPIData
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
            public string webhook { get; set; }
        }
        public class TokopediaToken
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


        public class DecryptedOrderList
        {
            public BuyerDecryptedOrderList buyer { get; set; }
            public RecipientDecryptedOrderList recipient { get; set; }
            public BuyerDecryptedOrderList customer { get; set; }//webhook
        }

        public class BuyerDecryptedOrderList
        {
            public long id { get; set; }
            public string name { get; set; }
            public string phone { get; set; }
            public string email { get; set; }
        }

        public class RecipientDecryptedOrderList
        {
            public string name { get; set; }
            public string phone { get; set; }
            public AddressDecryptedOrderList address { get; set; }
        }

        public class AddressDecryptedOrderList
        {
            public string address_full { get; set; }
        }


        public class TokopediaOrders
        {
            public Jsonapi jsonapi { get; set; }
            public object meta { get; set; }
            public TokopediaOrder[] data { get; set; }
            public Links links { get; set; }
        }

        public class Jsonapi
        {
            public string version { get; set; }
        }

        public class Links
        {
            public string self { get; set; }
            public string related { get; set; }
            public string first { get; set; }
            public string last { get; set; }
            public string prev { get; set; }
            public string next { get; set; }
        }

        public class TokopediaOrder
        {
            public string fs_id { get; set; }
            public int order_id { get; set; }
            public bool accept_partial { get; set; }
            public string invoice_ref_num { get; set; }
            public Products[] products { get; set; }
            public Products_Fulfilled[] products_fulfilled { get; set; }
            public string device_type { get; set; }
            public Buyer buyer { get; set; }
            public int shop_id { get; set; }
            public int payment_id { get; set; }
            public Recipient recipient { get; set; }
            public Logistics logistics { get; set; }
            public Amt amt { get; set; }
            public Dropshipper_Info dropshipper_info { get; set; }
            public Voucher_Info voucher_info { get; set; }
            public int order_status { get; set; }
            public int create_time { get; set; }
            public Custom_Fields custom_fields { get; set; }
            public EncryptionTokped encryption { get; set; }
            public int fulfill_by { get; set; }
        }
        public class EncryptionTokped
        {
            public string secret { get; set; }
            public string content { get; set; }
        }
        public class Buyer
        {
            public int id { get; set; }
            public string name { get; set; }
            public string phone { get; set; }
            public string email { get; set; }
        }

        public class Recipient
        {
            public string name { get; set; }
            public Address address { get; set; }
            public string phone { get; set; }
        }

        public class Address
        {
            public string address_full { get; set; }
            public string district { get; set; }
            public string city { get; set; }
            public string province { get; set; }
            public string country { get; set; }
            public string postal_code { get; set; }
            public int district_id { get; set; }
            public int city_id { get; set; }
            public int province_id { get; set; }
            public string geo { get; set; }
        }

        public class Logistics
        {
            public int shipping_id { get; set; }
            public string shipping_agency { get; set; }
            public string service_type { get; set; }
        }

        public class Amt
        {
            public int ttl_product_price { get; set; }
            public int shipping_cost { get; set; }
            public int insurance_cost { get; set; }
            public int ttl_amount { get; set; }
            public int voucher_amount { get; set; }
            public int toppoints_amount { get; set; }
        }

        public class Dropshipper_Info
        {
            public string name { get; set; }
            public string phone { get; set; }
        }

        public class Voucher_Info
        {
            public long voucher_type { get; set; }
            public string voucher_code { get; set; }
        }

        public class Custom_Fields
        {
            public string awb { get; set; }
        }

        public class Products
        {
            public long id { get; set; }
            public string name { get; set; }
            public int quantity { get; set; }
            public string notes { get; set; }
            public float weight { get; set; }
            public float total_weight { get; set; }
            public int price { get; set; }
            public int total_price { get; set; }
            public string currency { get; set; }
            public string sku { get; set; }
        }

        public class Products_Fulfilled
        {
            public long product_id { get; set; }
            public int quantity_deliver { get; set; }
            public int quantity_reject { get; set; }
        }

        public class RequestPickup
        {
            public int order_id { get; set; }
            public int shop_id { get; set; }
            public string request_time { get; set; }
        }

        public class AckOrder
        {
            public List<AckOrder_Product> products { get; set; }
        }

        public class AckOrder_Product
        {
            public string product_id { get; set; }
            public double quantity_deliver { get; set; }
            public double quantity_reject { get; set; }
        }


        public class categoryAPIResult
        {
            public categoryAPIResultHeader header { get; set; }
            public categoryAPIResultData data { get; set; }
        }

        public class categoryAPIResultHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            //public int error_code { get; set; }
        }

        public class categoryAPIResultData
        {
            public Category[] categories { get; set; }
        }

        public class Category
        {
            public string name { get; set; }
            public string id { get; set; }
            public CategoryChild[] child { get; set; }
        }

        public class CategoryChild
        {
            public string name { get; set; }
            public string id { get; set; }
            public CategoryChild[] child { get; set; }
        }

        public class ActiveProductListResult
        {
            public ActiveProductListResultHeader header { get; set; }
            public ActiveProductListResultData data { get; set; }
        }

        public class ActiveProductListResultHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            public int error_code { get; set; }
        }

        public class ActiveProductListResultData
        {
            public int total_data { get; set; }
            public ActiveProductListResultShop shop { get; set; }
            public ActiveProductListResultProduct[] products { get; set; }
        }

        public class ActiveProductListResultShop
        {
            //public int id { get; set; }
            public string name { get; set; }
            public string uri { get; set; }
            public string location { get; set; }
        }

        public class ActiveProductListResultProduct
        {
            public long id { get; set; }
            public string name { get; set; }
            public int[] childs { get; set; }
            public string url { get; set; }
            public string image_url { get; set; }
            public string image_url_700 { get; set; }
            public string price { get; set; }
            public ActiveProductListResultShop1 shop { get; set; }
            public object[] wholesale_price { get; set; }
            public int courier_count { get; set; }
            public int condition { get; set; }
            public int category_id { get; set; }
            public string category_name { get; set; }
            public string category_breadcrumb { get; set; }
            //public int department_id { get; set; }
            //public object[] labels { get; set; }
            public ActiveProductListResultBadge[] badges { get; set; }
            public int is_featured { get; set; }
            public int rating { get; set; }
            public int count_review { get; set; }
            public string original_price { get; set; }
            public string discount_expired { get; set; }
            public int discount_percentage { get; set; }
            public string sku { get; set; }
            public int stock { get; set; }
        }

        public class ActiveProductListResultShop1
        {
            //public int id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public bool is_gold { get; set; }
            public string location { get; set; }
            public string city { get; set; }
            public string reputation { get; set; }
            public string clover { get; set; }
        }

        public class ActiveProductListResultBadge
        {
            public string title { get; set; }
            public string image_url { get; set; }
        }

        public class ActiveProductVariantResult
        {
            public ActiveProductVariantResultHeader header { get; set; }
            public ActiveProductVariantResultData data { get; set; }
        }

        public class ActiveProductVariantResultHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            //public int error_code { get; set; }
        }

        public class ActiveProductVariantResultData
        {
            public long parent_id { get; set; }
            public long default_child { get; set; }
            public string sizechart { get; set; }
            public ActiveProductVariantResultDataVariant[] variant { get; set; }
            public ActiveProductVariantResultDataChild[] children { get; set; }
        }

        public class ActiveProductVariantResultDataVariant
        {
            public string name { get; set; }
            public string identifier { get; set; }
            public string unit_name { get; set; }
            public int position { get; set; }
            public ActiveProductVariantResultDataVariantOption[] option { get; set; }
        }

        public class ActiveProductVariantResultDataVariantOption
        {
            public int id { get; set; }
            public string value { get; set; }
            public string hex { get; set; }
        }

        public class ActiveProductVariantResultDataChild
        {
            public string name { get; set; }
            public string url { get; set; }
            public long product_id { get; set; }
            public int price { get; set; }
            public string price_fmt { get; set; }
            public int stock { get; set; }
            public string sku { get; set; }
            public int[] option_ids { get; set; }
            public bool enabled { get; set; }
            public bool is_buyable { get; set; }
            public bool is_wishlist { get; set; }
            public ActiveProductVariantResultDataChildPicture picture { get; set; }
            public ActiveProductVariantResultDataChildCampaign campaign { get; set; }
            public bool always_available { get; set; }
            public string stock_wording { get; set; }
            public string other_variant_stock { get; set; }
            public bool is_limited_stock { get; set; }
        }

        public class ActiveProductVariantResultDataChildPicture
        {
            public string original { get; set; }
            public string thumbnail { get; set; }
        }

        public class ActiveProductVariantResultDataChildCampaign
        {
            public bool is_active { get; set; }
            public int discounted_percentage { get; set; }
            public int discounted_price { get; set; }
            public string discounted_price_fmt { get; set; }
            public int campaign_type { get; set; }
            public string campaign_type_name { get; set; }
            public string start_date { get; set; }
            public string end_date { get; set; }
        }


        public class ItemListResult
        {
            public ItemListResultData[] data { get; set; }
            public string status { get; set; }
            public object[] error_message { get; set; }
        }

        public class ItemListResultData
        {
            public long product_id { get; set; }
            public string name { get; set; }
            //public int shop_id { get; set; }
            public string shop_name { get; set; }
            public int category_id { get; set; }
            public string desc { get; set; }
            public int stock { get; set; }
            public int price { get; set; }
            public string status { get; set; }
        }
        public class UpdateStockData
        {
            public string sku { get; set; }
            public long product_id { get; set; }
            public int new_stock { get; set; }

        }
        public class UpdatePriceData
        {
            public string sku { get; set; }
            public long product_id { get; set; }
            //change by nurul 12/2/2020
            //public float new_price { get; set; }
            public int new_price { get; set; }
            //end change by nurul 12/2/2020
        }


        public class Product
        {
            public ProductList[] products { get; set; }
        }

        public class ProductList
        {
            public string name { get; set; }
            public int category_id { get; set; }
            public int price { get; set; }
            public int status { get; set; }
            public int minimum_order { get; set; }
            public int weight { get; set; }
            public int weight_unit { get; set; }
            public int condition { get; set; }
            public string description { get; set; }
            public bool must_insurance { get; set; }
            public bool returnable { get; set; }
            public string sku { get; set; }
            public int stock { get; set; }
            public Product_Etalase etalase { get; set; }
            public Product_Wholesale_Price[] product_wholesale_price { get; set; }
            public Product_Preorder product_preorder { get; set; }
            public Product_Image[] images { get; set; }
            public Product_Video[] product_video { get; set; }
            public Product_Variant product_variant { get; set; }
        }

        public class Product_Etalase
        {
            public int etalase_id { get; set; }
            public string etalase_name { get; set; }
        }

        public class Product_Preorder
        {
            public int preorder_process_time { get; set; }
            public int preorder_time_unit { get; set; }
            public int preorder_status { get; set; }
        }

        public class Product_Variant
        {
            public Variant[] variant { get; set; }
            public Variant_Product[] product_variant { get; set; }
        }

        public class Variant
        {
            public int v { get; set; }
            public int vu { get; set; }
            public int pos { get; set; }
            public Variant_Opt[] opt { get; set; }
        }

        public class Variant_Opt
        {
            public int vuv { get; set; }
            public int t_id { get; set; }
            public string cstm { get; set; }
            public Variant_Opt_Image[] image { get; set; }
        }

        public class Variant_Opt_Image
        {
            public string file_path { get; set; }
            public string file_name { get; set; }
            public int x { get; set; }
            public int y { get; set; }
        }

        public class Variant_Product
        {
            public int st { get; set; }
            public int stock { get; set; }
            public float price_var { get; set; }
            public string sku { get; set; }
            public int[] opt { get; set; }
        }

        public class Product_Wholesale_Price
        {
            public int qty_min { get; set; }
            public int qty_max { get; set; }
            public int prd_prc { get; set; }
        }

        public class Product_Image
        {
            public string image_description { get; set; }
            public string image_file_path { get; set; }
            public string image_file_name { get; set; }
        }

        public class Product_Video
        {
            public string url { get; set; }
            public string type { get; set; }
        }

        public class GetVariantResult
        {
            public GetVariantResultHeader header { get; set; }
            public GetVariantResultData[] data { get; set; }
        }

        public class GetVariantResultHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            //public int error_code { get; set; }
        }

        public class GetVariantResultData
        {
            //public int variant_id { get; set; }
            public int variant_id { get; set; }
            public string name { get; set; }
            public string identifier { get; set; }
            public int status { get; set; }
            public int has_unit { get; set; }
            public GetVariantResultHeaderDataUnit[] units { get; set; }
        }

        public class GetVariantResultHeaderDataUnit
        {
            public int unit_id { get; set; }
            public string name { get; set; }
            public string short_name { get; set; }
            public GetVariantResultHeaderDataUnitValue[] values { get; set; }
        }

        public class GetVariantResultHeaderDataUnitValue
        {
            public int value_id { get; set; }
            public string value { get; set; }
            public string hex_code { get; set; }
            public string icon { get; set; }
        }

        public class EditProductTokpedData
        {
            public List<EditProduct_Product> products { get; set; }
        }
        public class EditProductTokpedDataV3
        {
            public List<EditProduct_ProductV3> products { get; set; }
        }
        public class EditProduct_ProductV3 : EditProduct_Product
        {
        
            public List<int> custom_product_logistics { get; set; }
        }
        public class EditProduct_Product
        {
            public long id { get; set; }
            public string name { get; set; }
            public int? category_id { get; set; }
            public int price { get; set; }
            public string price_currency { get; set; }
            public string status { get; set; }
            public int min_order { get; set; }
            public int weight { get; set; }
            public string weight_unit { get; set; }
            public string condition { get; set; }
            public string description { get; set; }
            public bool is_must_insurance { get; set; }
            public bool is_free_return { get; set; }
            public string sku { get; set; }
            public int stock { get; set; }
            //tes
            public CreateProduct_Etalase etalase { get; set; }
            public CreateProduct_Product_Wholesale_Price[] wholesale { get; set; }
            public CreateProduct_Product_Preorder preorder { get; set; }
            public List<CreateProduct_Images> pictures { get; set; }
            public CreateProduct_Product_Video[] videos { get; set; }
            public CreateProduct_Product_Variant variant { get; set; }
            public CreateProduct_Dimension dimension { get; set; }
        }

        public class CreateProductTokpedData
        {
            public List<CreateProduct_Product> products { get; set; }
        }

        public class CreateProduct_Productv3 : CreateProduct_Product
        {
            public List<int> custom_product_logistics { get; set; }

        }
        public class CreateProduct_Product
        {
            public string name { get; set; }
            public int category_id { get; set; }
            public int price { get; set; }
            public string price_currency { get; set; }
            public string status { get; set; }
            //public int minimum_order { get; set; }
            public int min_order { get; set; }//api v2
            public int weight { get; set; }
            public string weight_unit { get; set; }
            public string condition { get; set; }
            public string description { get; set; }
            //public bool must_insurance { get; set; }
            //public bool returnable { get; set; }
            public bool is_must_insurance { get; set; }//api v2
            public bool is_free_return { get; set; }//api v2
            public string sku { get; set; }
            public int stock { get; set; }
            public CreateProduct_Etalase etalase { get; set; }
            public CreateProduct_Dimension dimension { get; set; }
            //public CreateProduct_Product_Wholesale_Price[] product_wholesale_price { get; set; }
            //public CreateProduct_Product_Preorder product_preorder { get; set; }
            //public List<CreateProduct_Images> images { get; set; }
            //public CreateProduct_Product_Video[] product_video { get; set; }
            //public CreateProduct_Product_Variant product_variant { get; set; }
            public CreateProduct_Product_Wholesale_Price[] wholesale { get; set; }//api v2
            public CreateProduct_Product_Preorder preorder { get; set; }//api v2
            public List<CreateProduct_Images> pictures { get; set; }//api v2
            public CreateProduct_Product_Video[] videos { get; set; }//api v2
            public CreateProduct_Product_Variant variant { get; set; }//api v2
        }

        public class CreateProduct_Dimension
        {
            public double height { get; set; }
            public double width { get; set; }
            public double length { get; set; }
        }

        public class CreateProduct_Etalase
        {
            //public int etalase_id { get; set; }
            //public string etalase_name { get; set; }
            public int id { get; set; }
            public string name { get; set; }
        }

        public class CreateProduct_Product_Preorder
        {
            //public int preorder_process_time { get; set; }
            //public int preorder_time_unit { get; set; }
            //public int preorder_status { get; set; }
            public bool is_active { get; set; }
            public int duration { get; set; }
            public string time_unit { get; set; }//"time_unit":"DAY"
        }

        public class CreateProduct_Product_Variant
        {
            //public List<CreateProduct_Variant> variant { get; set; }
            //public List<CreateProduct_Product_Variant1> product_variant { get; set; }
            public List<CreateProduct_Variant> selection { get; set; }
            public List<CreateProduct_Product_Variant1> products { get; set; }
            public List<CreateProduct_Images> sizecharts { get; set; }
        }

        public class CreateProduct_Variant
        {
            //public int v { get; set; }
            //public int vu { get; set; }
            //public int pos { get; set; }
            //public List<CreateProduct_Opt> opt { get; set; }
            public int id { get; set; }
            public int unit_id { get; set; }
            //public int pos { get; set; }
            public List<CreateProduct_Opt> options { get; set; }
        }

        public class CreateProduct_Opt
        {
            //public int vuv { get; set; }
            //public int t_id { get; set; }
            //public string cstm { get; set; }
            //public List<CreateProduct_Image> image { get; set; }
            public int unit_value_id { get; set; }
            //public int t_id { get; set; }
            public string hex_code { get; set; }
            public string value { get; set; }
            //public List<CreateProduct_Image> image { get; set; }
        }

        public class CreateProduct_Image
        {
            public string file_path { get; set; }
            public string file_name { get; set; }
            public int x { get; set; }
            public int y { get; set; }
        }

        public class CreateProduct_Product_Variant1
        {
            //public int st { get; set; }
            //public int stock { get; set; }
            //public float price_var { get; set; }
            //public string sku { get; set; }
            //public List<int> opt { get; set; }
            public string status { get; set; }
            public int stock { get; set; }
            public int price { get; set; }
            public string sku { get; set; }
            public List<int> combination { get; set; }
            public List<CreateProduct_Images> pictures { get; set; }

        }

        public class CreateProduct_Product_Wholesale_Price
        {
            //public int qty_min { get; set; }
            //public int qty_max { get; set; }
            //public int prd_prc { get; set; }
            public int min_qty { get; set; }
            public int price { get; set; }
        }

        public class CreateProduct_Images
        {
            //public string image_description { get; set; }
            //public string image_file_path { get; set; }
            //public string image_file_name { get; set; }
            public string file_path { get; set; }
        }

        public class CreateProduct_Product_Video
        {
            public string url { get; set; }//url should only contain the YouTube video id
            //public string type { get; set; }
            public string source { get; set; }//"source": "youtube"

        }

        public class GetEtalaseReturn
        {
            public GetEtalaseReturnHeader header { get; set; }
            public GetEtalaseReturnData data { get; set; }
        }

        public class GetEtalaseReturnHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            public int error_code { get; set; }
        }

        public class GetEtalaseReturnData
        {
            public GetEtalaseReturnShop shop { get; set; }
            public List<GetEtalaseReturnEtalase> etalase { get; set; }
        }

        public class GetEtalaseReturnShop
        {
            public int id { get; set; }
            public string name { get; set; }
            public string uri { get; set; }
            public string location { get; set; }
        }

        public class GetEtalaseReturnEtalase
        {
            public int etalase_id { get; set; }
            public string etalase_name { get; set; }
            public string url { get; set; }
        }

        public class TokpedCreateProductResult
        {
            public TokpedCreateProductResultHeader header { get; set; }
            public TokpedCreateProductResultData data { get; set; }
        }

        public class TokpedCreateProductResultV3
        {
            public TokpedCreateProductResultHeader header { get; set; }
            public TokpedCreateProductResultDataV3 data { get; set; }
        }
        public class TokpedCreateProductResultDataV3
        {
            public int success_data { get; set; }
            public int fail_data { get; set; }
            public List<SuccessRowDataTokpedV3> success_rows_data { get; set; }
            public List<FailedRowDataTokpedV3> failed_rows_data { get; set; }
        }
        public class SuccessRowDataTokpedV3 {
            public long product_id { get; set; }
        }
        public class FailedRowDataTokpedV3
        {
            public string product_name { get; set; }
            public int product_price { get; set; }
            public string sku { get; set; }
            public List<string> error { get; set; }

        }
        public class TokpedCreateProductResultHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            //public int error_code { get; set; }
        }

        public class TokpedCreateProductResultData
        {
            public int upload_id { get; set; }
        }
        //add 22 jan 2020, get cancel reason

        public class TokopediaSingleOrder
        {
            public TokopediaSingleOrderHeader header { get; set; }
            public TokopediaSingleOrderData data { get; set; }
        }

        public class TokopediaSingleOrderHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            //public int error_code { get; set; }
        }

        public class TokopediaSingleOrderData
        {
            public long order_id { get; set; }
            public long buyer_id { get; set; }
            public long seller_id { get; set; }
            public long payment_id { get; set; }
            public bool is_affiliate { get; set; }
            public bool is_fulfillment { get; set; }
            //change by nurul 23/11/2021
            //public Order_Warehouse order_warehouse { get; set; }
            public OrderWarehouse order_warehouse { get; set; }
            //end change by nurul 23/11/2021
            public int order_status { get; set; }
            public string invoice_number { get; set; }
            public string invoice_pdf { get; set; }
            public string invoice_url { get; set; }
            public int open_amt { get; set; }
            public int lp_amt { get; set; }
            public int cashback_amt { get; set; }
            public string info { get; set; }
            public string comment { get; set; }
            public int item_price { get; set; }
            public Buyer_Info buyer_info { get; set; }
            //public Shop_Info shop_info { get; set; }
            public Shipment_Fulfillment shipment_fulfillment { get; set; }
            public Preorder preorder { get; set; }
            public Order_Info order_info { get; set; }
            //public Origin_Info origin_info { get; set; }
            public Payment_Info payment_info { get; set; }
            //public Insurance_Info insurance_info { get; set; }
            //public object hold_info { get; set; }
            public Cancel_Request_Info cancel_request_info { get; set; }
            public DateTime create_time { get; set; }
            //public object shipping_date { get; set; }
            public DateTime update_time { get; set; }
            public DateTime payment_date { get; set; }
            public DateTime? shipping_date { get; set; }
            //public object delivered_date { get; set; }
            //public object est_shipping_date { get; set; }
            //public object est_delivery_date { get; set; }
            //public object related_invoices { get; set; }
            //public object custom_fields { get; set; }
        }

        public class Order_Warehouse
        {
            public int warehouse_id { get; set; }
            public int fulfill_by { get; set; }
            public Meta_Data meta_data { get; set; }
        }

        public class Meta_Data
        {
            public int warehouse_id { get; set; }
            public int partner_id { get; set; }
            public int shop_id { get; set; }
            public string warehouse_name { get; set; }
            public int district_id { get; set; }
            public string district_name { get; set; }
            public int city_id { get; set; }
            public string city_name { get; set; }
            public int province_id { get; set; }
            public string province_name { get; set; }
            public int status { get; set; }
            public string postal_code { get; set; }
            public int is_default { get; set; }
            public string latlon { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
            public string email { get; set; }
            public string address_detail { get; set; }
            public string country_name { get; set; }
            public bool is_fulfillment { get; set; }
        }

        public class Buyer_Info
        {
            //public int buyer_id { get; set; }
            public string buyer_fullname { get; set; }
            public string buyer_email { get; set; }
            public string buyer_phone { get; set; }
        }

        public class Shop_Info
        {
            public int shop_owner_id { get; set; }
            public string shop_owner_email { get; set; }
            public string shop_owner_phone { get; set; }
            public string shop_name { get; set; }
            public string shop_domain { get; set; }
            public int shop_id { get; set; }
        }

        public class Shipment_Fulfillment
        {
            public long id { get; set; }
            public long order_id { get; set; }
            public DateTime? payment_date_time { get; set; }
            public bool is_same_day { get; set; }
            public DateTime? accept_deadline { get; set; }
            public DateTime? confirm_shipping_deadline { get; set; }
            public Item_Delivered_Deadline item_delivered_deadline { get; set; }
            public bool is_accepted { get; set; }
            public bool is_confirm_shipping { get; set; }
            public bool is_item_delivered { get; set; }
            public int fulfillment_status { get; set; }
        }

        public class Item_Delivered_Deadline
        {
            public DateTime? Time { get; set; }
            public bool Valid { get; set; }
        }

        public class Preorder
        {
            public long? order_id { get; set; }
            public int? preorder_type { get; set; }
            public int? preorder_process_time { get; set; }
            public DateTime? preorder_process_start { get; set; }
            public DateTime? preorder_deadline { get; set; }
            //public int shop_id { get; set; }
            //public int customer_id { get; set; }
        }

        public class Order_Info
        {
            //public Order_Detail[] order_detail { get; set; }
            public Order_History[] order_history { get; set; }
            public int order_age_day { get; set; }
            public int shipping_age_day { get; set; }
            public int delivered_age_day { get; set; }
            public bool partial_process { get; set; }
            public Shipping_Info shipping_info { get; set; }
            public Destination destination { get; set; }
            public bool is_replacement { get; set; }
            public int replacement_multiplier { get; set; }
        }

        public class Shipping_Info
        {
            public int sp_id { get; set; }
            public int shipping_id { get; set; }
            public string logistic_name { get; set; }
            public string logistic_service { get; set; }
            public int shipping_price { get; set; }
            public int shipping_price_rate { get; set; }
            public int shipping_fee { get; set; }
            public int insurance_price { get; set; }
            public int fee { get; set; }
            public bool is_change_courier { get; set; }
            public int second_sp_id { get; set; }
            public int second_shipping_id { get; set; }
            public string second_logistic_name { get; set; }
            public string second_logistic_service { get; set; }
            public int second_agency_fee { get; set; }
            public int second_insurance { get; set; }
            public int second_rate { get; set; }
            public string awb { get; set; }
            public int autoresi_cashless_status { get; set; }
            public string autoresi_awb { get; set; }
            public int autoresi_shipping_price { get; set; }
            public int count_awb { get; set; }
            public bool isCashless { get; set; }
            public bool is_fake_delivery { get; set; }
        }

        public class Destination
        {
            public string receiver_name { get; set; }
            public string receiver_phone { get; set; }
            public string address_street { get; set; }
            public string address_district { get; set; }
            public string address_city { get; set; }
            public string address_province { get; set; }
            public string address_postal { get; set; }
            public int customer_address_id { get; set; }
            public int district_id { get; set; }
            public int city_id { get; set; }
            public int province_id { get; set; }
        }

        public class Order_Detail
        {
            public int order_detail_id { get; set; }
            public long product_id { get; set; }
            public string product_name { get; set; }
            public string product_desc_pdp { get; set; }
            public string product_desc_atc { get; set; }
            public int product_price { get; set; }
            public int subtotal_price { get; set; }
            public double weight { get; set; }
            public double total_weight { get; set; }
            public int quantity { get; set; }
            public int quantity_deliver { get; set; }
            public int quantity_reject { get; set; }
            public bool is_free_returns { get; set; }
            public int insurance_price { get; set; }
            public int normal_price { get; set; }
            public int currency_id { get; set; }
            public int currency_rate { get; set; }
            public int min_order { get; set; }
            public int child_cat_id { get; set; }
            public string campaign_id { get; set; }
            public string product_picture { get; set; }
            public string snapshot_url { get; set; }
        }

        public class Order_History
        {
            public string action_by { get; set; }
            public int hist_status_code { get; set; }
            public string message { get; set; }
            public DateTime timestamp { get; set; }
            public string comment { get; set; }
            public int create_by { get; set; }
            public string update_by { get; set; }
            public string ip_address { get; set; }
        }

        public class Origin_Info
        {
            public string sender_name { get; set; }
            public int origin_province { get; set; }
            public string origin_province_name { get; set; }
            public int origin_city { get; set; }
            public string origin_city_name { get; set; }
            public string origin_address { get; set; }
            public int origin_district { get; set; }
            public string origin_district_name { get; set; }
            public string origin_postal_code { get; set; }
            public string origin_geo { get; set; }
            public string receiver_name { get; set; }
            public string destination_address { get; set; }
            public int destination_province { get; set; }
            public int destination_city { get; set; }
            public int destination_district { get; set; }
            public string destination_postal_code { get; set; }
            public string destination_geo { get; set; }
            public Destination_Loc destination_loc { get; set; }
        }

        public class Destination_Loc
        {
            public int lat { get; set; }
            public int lon { get; set; }
        }

        public class Payment_Info
        {
            //public int payment_id { get; set; }
            //public string payment_ref_num { get; set; }
            //public DateTime payment_date { get; set; }
            //public int payment_method { get; set; }
            //public string payment_status { get; set; }
            //public int payment_status_id { get; set; }
            //public DateTime create_time { get; set; }
            //public int pg_id { get; set; }
            public string gateway_name { get; set; }
            //public int discount_amount { get; set; }
            //public string voucher_code { get; set; }
            //public int voucher_id { get; set; }
        }

        public class Insurance_Info
        {
            public int insurance_type { get; set; }
        }

        public class Cancel_Request_Info
        {
            public DateTime create_time { get; set; }
            public string reason { get; set; }
            public int status { get; set; }
        }

        //end 22 jan 2020, get cancel reason

        //add by nurul 23/3/2020
        public class JOBCODResult
        {
            public Header header { get; set; }
            public TokpedJOBCOD data { get; set; }
            public string status { get; set; }
            //public string[] error_message { get; set; }
        }
        public class Header
        {
            public string process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            public string error_code { get; set; }
        }
        public class TokpedJOBCOD
        {
            public OrderData[] order_data { get; set; }
            public long next_order_id { get; set; }
            public long first_order_id { get; set; }
        }
        public class OrderData
        {
            public OrderJOB order { get; set; }
            public OrderHistory[] order_history { get; set; }
            public OrderDetail[] order_detail { get; set; }
            public DropShipper drop_shipper { get; set; }
            public TypeMeta type_meta { get; set; }
            public OrderShipmentFulfillment order_shipment_fulfillment { get; set; }
            public BookingData booking_data { get; set; }
        }
        public class OrderJOB
        {
            public long order_id { get; set; }
            public long buyer_id { get; set; }
            public long seller_id { get; set; }
            public long payment_id { get; set; }
            public long order_status { get; set; }
            public string invoice_number { get; set; }
            public string invoice_pdf_link { get; set; }
            public long open_amt { get; set; }
            public float payment_amt_cod { get; set; }

        }
        public class OrderHistory
        {
            public long order_hist_id { get; set; }
            public long status { get; set; }
            public DateTime? shipping_date { get; set; }
            public long create_by { get; set; }
        }
        public class OrderDetail
        {
            public long order_detail_id { get; set; }
            public long product_id { get; set; }
            public string product_name { get; set; }
            public long quantity { get; set; }
            public float product_price { get; set; }
            public float insurance_price { get; set; }
        }
        public class DropShipper
        {
            public long order_id { get; set; }
            public string dropship_name { get; set; }
            public string dropship_telp { get; set; }
        }
        public class TypeMeta
        {
            public kelontong kelontong { get; set; }
            public cod cod { get; set; }
            public sampai sampai { get; set; }
            public now now { get; set; }
            public ppp ppp { get; set; }
            public trade_in trade_in { get; set; }
            public vehicle_leasing vehicle_leasing { get; set; }
        }
        public class kelontong { }
        public class cod { }
        public class sampai { }
        public class now { }
        public class ppp { }
        public class trade_in { }
        public class vehicle_leasing { }
        public class OrderShipmentFulfillment
        {
            public long id { get; set; }
            public long order_id { get; set; }
            public DateTime payment_date_time { get; set; }
            public bool is_same_day { get; set; }
            public DateTime accept_deadline { get; set; }
            public DateTime confirm_shipping_deadline { get; set; }
            public ItemDeliveredDeadline item_delivered_deadline { get; set; }
            public bool is_accepted { get; set; }
            public bool is_confirm_shipping { get; set; }
            public bool is_item_delivered { get; set; }
            public int fulfillment_status { get; set; }
        }
        public class BookingData
        {
            public long order_id { get; set; }
            public string booking_code { get; set; }
            public int booking_status { get; set; }
        }
        public class ItemDeliveredDeadline
        {
            public DateTime Time { get; set; }
            public bool Valid { get; set; }
        }
        //end class job
        //start class print label
        public class TokpedPrintLabel
        {
            public categoryAPIResultHeader header { get; set; }
            public string data { get; set; }
        }
        //end class print label 
        ////start class single order 
        public class TokpedSingleOrderResult
        {
            public categoryAPIResultHeader header { get; set; }
            public TokpedSingleOrder data { get; set; }
        }
        public class TokpedSingleOrder
        {
            public long order_id { get; set; }
            public long buyer_id { get; set; }
            public long seller_id { get; set; }
            public long payment_id { get; set; }
            public bool is_affiliate { get; set; }
            public bool is_fulfillment { get; set; }
            public OrderWarehouse order_warehouse { get; set; }
            public int order_status { get; set; }
            public string invoice_number { get; set; }
            public string invoice_pdf { get; set; }
            public string invoice_url { get; set; }
            public long open_amt { get; set; }
            public long lp_amt { get; set; }
            public long cashback_amt { get; set; }
            public string info { get; set; }
            public string comment { get; set; }
            public long item_price { get; set; }
            public BuyerInfo buyer_info { get; set; }
            public ShopInfo shop_info { get; set; }
            public ShipmentFulfillment shipment_fulfillment { get; set; }
            public PreorderAWB preorder { get; set; }
            public OrderInfo order_info { get; set; }
            public OriginInfo origin_info { get; set; }
            public PaymentInfo payment_info { get; set; }
            public InsuranceInfo insurance_info { get; set; }
            //public string hold_info { get; set; }
            public Cancel_Request_Info cancel_request_info { get; set; }
            public DateTime create_time { get; set; }
            //public object shipping_date { get; set; }
            public DateTime update_time { get; set; }
            public DateTime payment_date { get; set; }
            //public object delivered_date { get; set; }
            //public object est_shipping_date { get; set; }
            //public object est_delivery_date { get; set; }
            //public object related_invoices { get; set; }
            //public object custom_fields { get; set; }
        }
        public class OrderWarehouse
        {
            public long warehouse_id { get; set; }
            public int fulfill_by { get; set; }
            public MetaData meta_data { get; set; }
        }
        public class MetaData
        {
            public long warehouse_id { get; set; }
            public long partner_id { get; set; }
            public long shop_id { get; set; }
            public string warehouse_name { get; set; }
            public long district_id { get; set; }
            public string district_name { get; set; }
            public long city_id { get; set; }
            public string city_name { get; set; }
            public long province_id { get; set; }
            public string province_name { get; set; }
            public int status { get; set; }
            public string postal_code { get; set; }
            public int is_default { get; set; }
            public string latlon { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
            public string email { get; set; }
            public string address_detail { get; set; }
            public string country_name { get; set; }
            public bool is_fulfillment { get; set; }
        }
        public class BuyerInfo
        {
            public long buyer_id { get; set; }
            public string buyer_fullname { get; set; }
            public string buyer_email { get; set; }
            public string buyer_phone { get; set; }
        }
        public class ShopInfo
        {
            public long shop_owner_id { get; set; }
            public string shop_owner_email { get; set; }
            public string shop_owner_phone { get; set; }
            public string shop_name { get; set; }
            public string shop_domain { get; set; }
            public long shop_id { get; set; }
        }
        public class ShipmentFulfillment
        {
            public long id { get; set; }
            public long order_id { get; set; }
            public DateTime? payment_date_time { get; set; }
            public bool is_same_day { get; set; }
            public DateTime? accept_deadline { get; set; }
            public DateTime? confirm_shipping_deadline { get; set; }
            public ItemDeliveredDeadline item_delivered_deadline { get; set; }
            public bool is_accepted { get; set; }
            public bool is_confirm_shipping { get; set; }
            public bool is_item_delivered { get; set; }
            public int fulfillment_status { get; set; }
        }
        public class PreorderAWB
        {
            public long order_id { get; set; }
            public long preorder_type { get; set; }
            public long preorder_process_time { get; set; }
            public DateTime? preorder_process_start { get; set; }
            public DateTime? preorder_deadline { get; set; }
            public long shop_id { get; set; }
            public long customer_id { get; set; }
        }
        public class OrderInfo
        {
            public OrderDetailSingleOrder[] order_detail { get; set; }
            public OrderHistorySingleOrder[] order_history { get; set; }
            public long order_age_day { get; set; }
            public long shipping_age_day { get; set; }
            public long delivered_age_day { get; set; }
            public bool partial_process { get; set; }
            public ShippingInfo shipping_info { get; set; }
            public DestinationAWB destination { get; set; }
            public bool is_replacement { get; set; }
            public int replacement_multiplier { get; set; }
        }
        public class OrderDetailSingleOrder
        {
            public long order_detail_id { get; set; }
            public long product_id { get; set; }
            public string product_name { get; set; }
            public string product_desc_pdp { get; set; }
            public string product_desc_atc { get; set; }
            public float product_price { get; set; }
            public string subtotal_price { get; set; }
            public float weight { get; set; }
            public float total_weight { get; set; }
            public int quantity { get; set; }
            public int quantity_deliver { get; set; }
            public int quantity_reject { get; set; }
            public bool is_free_returns { get; set; }
            public float insurance_price { get; set; }
            public float normal_price { get; set; }
            public long currency_id { get; set; }
            public float currency_rate { get; set; }
            public int min_order { get; set; }
            public long child_cat_id { get; set; }
            public string campaign_id { get; set; }
            public string product_picture { get; set; }
            public string snapshot_url { get; set; }
        }
        public class OrderHistorySingleOrder
        {
            public string action_by { get; set; }
            public long hist_status_code { get; set; }
            public string message { get; set; }
            public DateTime? timestamp { get; set; }
            public string comment { get; set; }
            public long create_by { get; set; }
            public string update_by { get; set; }
            public string ip_address { get; set; }
        }
        public class ShippingInfo
        {
            public long sp_id { get; set; }
            public long shipping_id { get; set; }
            public string logistic_name { get; set; }
            public string logistic_service { get; set; }
            public float shipping_price { get; set; }
            public float shipping_price_rate { get; set; }
            public float shipping_fee { get; set; }
            public float insurance_price { get; set; }
            public float fee { get; set; }
            public bool is_change_courier { get; set; }
            public long second_sp_id { get; set; }
            public long second_shipping_id { get; set; }
            public string second_logistic_name { get; set; }
            public string second_logistic_service { get; set; }
            public float second_agency_fee { get; set; }
            public float second_insurance { get; set; }
            public float second_rate { get; set; }
            public string awb { get; set; }
            public int autoresi_cashless_status { get; set; }
            public string autoresi_awb { get; set; }
            public float autoresi_shipping_price { get; set; }
            public int count_awb { get; set; }
            public bool isCashless { get; set; }
            public bool is_fake_delivery { get; set; }
        }
        public class DestinationAWB
        {
            public string receiver_name { get; set; }
            public string receiver_phone { get; set; }
            public string address_street { get; set; }
            public string address_district { get; set; }
            public string address_city { get; set; }
            public string address_province { get; set; }
            public string address_postal { get; set; }
            public long customer_address_id { get; set; }
            public long district_id { get; set; }
            public long city_id { get; set; }
            public long province_id { get; set; }
        }
        public class OriginInfo
        {
            public string sender_name { get; set; }
            public long origin_province { get; set; }
            public string origin_province_name { get; set; }
            public long origin_city { get; set; }
            public string origin_city_name { get; set; }
            public string origin_address { get; set; }
            public long origin_district { get; set; }
            public string origin_district_name { get; set; }
            public string origin_postal_code { get; set; }
            public string origin_geo { get; set; }
            public string receiver_name { get; set; }
            public string destination_address { get; set; }
            public long destination_province { get; set; }
            public long destination_city { get; set; }
            public long destination_district { get; set; }
            public string destination_postal_code { get; set; }
            public string destination_geo { get; set; }
            public DestinationLoc destination_loc { get; set; }
        }
        public class DestinationLoc
        {
            public long lat { get; set; }
            public long lon { get; set; }
        }
        public class PaymentInfo
        {
            public long payment_id { get; set; }
            public string payment_ref_num { get; set; }
            public DateTime? payment_date { get; set; }
            public long payment_method { get; set; }
            public string payment_status { get; set; }
            public int payment_status_id { get; set; }
            public DateTime? create_time { get; set; }
            public long pg_id { get; set; }
            public string gateway_name { get; set; }
            public float discount_amount { get; set; }
            public string voucher_code { get; set; }
            public long voucher_id { get; set; }
        }
        public class InsuranceInfo
        {
            public long insurance_type { get; set; }
        }
        //add class postActOrder
        public class ActOrderResult
        {
            //change by nurul 4/6/2020
            //public string data { get; set; }
            //public string status { get; set; }
            //public string[] error_message { get; set; }
            public TokopediaAckOrderHeader header { get; set; }
            public string data { get; set; }
            //end change by nurul 4/6/2020
        }
        //end add by nurul 23/3/2020

        //add 4 jun 2020

        public class UpdatePriceResponse
        {
            public UpdatePriceResponseHeader header { get; set; }
            public UpdatePriceResponseData data { get; set; }
        }

        public class UpdatePriceResponseHeader
        {
            //public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            //public int error_code { get; set; }
        }

        public class UpdatePriceResponseData
        {
            public int succeed_rows { get; set; }
            public int failed_rows { get; set; }
            public UpdatePriceResponseFailed_Rows_Data[] failed_rows_data { get; set; }
        }

        public class UpdatePriceResponseFailed_Rows_Data
        {
            public long product_id { get; set; }
            //public long new_price { get; set; }
            //public long new_stock { get; set; }
            public string message { get; set; }
        }
        //end add 4 jun 2020

        //add by nurul 4/6/2020
        public class TokopediaAckOrderHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            public string error_code { get; set; }
        }

        public class ReqPickupResult
        {
            public TokopediaAckOrderHeader header { get; set; }
            public TokpedReqPickupData data { get; set; }
        }
        public class TokpedReqPickupData
        {
            public long order_id { get; set; }
            public long shop_id { get; set; }
            public DateTime request_time { get; set; }
            public string result { get; set; }
            public string shipping_ref_num { get; set; }
        }
        //end add by nurul 4/6/2020

        //add by nurul 20/9/2021
        //List Message 
        public class resultListMessage
        {
            public HeaderListMessage header { get; set; }
            public DatumListMessage[] data { get; set; }
        }

        public class HeaderListMessage
        {
            public int process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            public int error_code { get; set; }
        }

        public class DatumListMessage
        {
            public string message_key { get; set; }
            public long msg_id { get; set; }
            public AttributesListMessage attributes { get; set; }
        }

        public class AttributesListMessage
        {
            public Contact contact { get; set; }
            public string last_reply_msg { get; set; }
            public long last_reply_time { get; set; }
            public int read_status { get; set; }
            public int unreads { get; set; }
            public int pin_status { get; set; }
        }

        public class Contact
        {
            public long id { get; set; }
            public string role { get; set; }
            public Attributes1 attributes { get; set; }
        }

        public class Attributes1
        {
            public string name { get; set; }
            public string tag { get; set; }
            public string thumbnail { get; set; }
        }

        //list reply
        public class ResultListReply
        {
            public HeaderListReply header { get; set; }
            public DatumListReply[] data { get; set; }
        }

        public class HeaderListReply
        {
            public int process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            public int error_code { get; set; }
        }

        public class DatumListReply
        {
            public long msg_id { get; set; }
            public long reply_id { get; set; }
            public int sender_id { get; set; }
            public string sender_name { get; set; }
            public string role { get; set; }
            public string msg { get; set; }
            public long reply_time { get; set; }
            public int read_status { get; set; }
            public long read_time { get; set; }
            public int status { get; set; }
            public long attachment_id { get; set; }
            public bool message_is_read { get; set; }
            public bool is_opposite { get; set; }
            public bool is_first_reply { get; set; }
            public bool is_reported { get; set; }
            public Attachment attachment { get; set; }
        }

        public class Attachment
        {
            public long id { get; set; }
            public int type { get; set; }
            public AttributesListReply attributes { get; set; }
            public Fallback_Attachment fallback_attachment { get; set; }
        }

        public class AttributesListReply
        {
            public long product_id { get; set; }
            public string image_url { get; set; }
        }

        public class Fallback_Attachment
        {
            public string html { get; set; }
            public string message { get; set; }
        }
        //end add by nurul 20/9/2021

        public class ResultGetActiveCourier
        {
            public HeaderListReply header { get; set; }
            public GetActiveCourierData data { get; set; }
        }
        public class GetActiveCourierData
        {
            public List<ListShopData> Shops { get; set; }
        }

        public class ListShopData
        {
            public long ShopID { get; set; }
            public List<ListActiveCourierData> ShipmentInfos { get; set; }
        }
        public class ListActiveCourierData
        {
            public int ShipmentID { get; set; }
            public string ShipmentName { get; set; }
            public int ShipmentAvailable { get; set; }
            public List<ShipmentPackagesData> ShipmentPackages { get; set; }
        }
        public class ShipmentPackagesData
        {
            public int IsAvailable { get; set; }
            public string ProductName { get; set; }
            public int ShippingProductID { get; set; }
        }

    }
}