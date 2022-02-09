using Erasoft.Function;
using Hangfire;
using Hangfire.SqlServer;
using Lazop.Api;
using Lazop.Api.Util;
using MasterOnline.Models;
using MasterOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace MasterOnline.Controllers
{
    public class LazadaChatController : Controller
    {
        //AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        string urlLazada = "https://api.lazada.co.id/rest";
        List<string> listSku = new List<string>();
        //string eraCallbackUrl = "https://dev.masteronline.co.id/lzdchat/code?user=";
        //string eraAppKey = "";101775;106147
#if AWS
                        
        string eraAppKey = "106112";
        string eraAppSecret = "7rR0SgbthC50HsDQPIa1lcwGvwZMOCJD";
        string eraCallbackUrl = "https://masteronline.co.id/lzdchat/code?user=";
#elif Debug_AWS

        string eraAppKey = "106112";
        string eraAppSecret = "7rR0SgbthC50HsDQPIa1lcwGvwZMOCJD";
        string eraCallbackUrl = "https://masteronline.co.id/lzdchat/code?user=";
#else

        string eraAppKey = "106112";
        string eraAppSecret = "7rR0SgbthC50HsDQPIa1lcwGvwZMOCJD";
        string eraCallbackUrl = "https://dev.masteronline.co.id/lzdchat/code?user=";

        //string eraAppKey = "106112";
        //string eraAppSecret = "7rR0SgbthC50HsDQPIa1lcwGvwZMOCJD";
        //string eraCallbackUrl = "https://masteronline.my.id/lzdchat/code?user=";
#endif
        // GET: Lazada; QwUJjjtZ3eCy2qaz6Rv1PEXPyPaPkDSu;So2KEplWTt4XFO9OGmXjuFFVIT1Wc6FU
        DatabaseSQL EDB;
        MoDbContext MoDbContext;
        ErasoftContext ErasoftDbContext;
        string DatabasePathErasoft;
        string dbSourceEra = "";
        string username;

        public LazadaChatController()
        {
            MoDbContext = new MoDbContext("");
            //            if (sessionData?.Account != null)
            //            {
            //                if (sessionData.Account.UserId == "admin_manage")
            //                {
            //                    ErasoftDbContext = new ErasoftContext();
            //                }
            //                else
            //                {
            //#if (Debug_AWS)
            //                    dbSourceEra = sessionData.Account.DataSourcePathDebug;
            //#else
            //                    dbSourceEra = sessionData.Account.DataSourcePath;
            //#endif
            //                    ErasoftDbContext = new ErasoftContext(dbSourceEra, sessionData.Account.DatabasePathErasoft);
            //                }

            //                EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);
            //                DatabasePathErasoft = sessionData.Account.DatabasePathErasoft;

            //            }
            //            else
            //            {
            //                if (sessionData?.User != null)
            //                {
            //                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
            //                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
            //#if (Debug_AWS)
            //                    dbSourceEra = accFromUser.DataSourcePathDebug;
            //#else
            //                    dbSourceEra = accFromUser.DataSourcePath;
            //#endif
            //                    //ErasoftDbContext = new ErasoftContext(accFromUser.DataSourcePath, accFromUser.DatabasePathErasoft);
            //                    ErasoftDbContext = new ErasoftContext(dbSourceEra, accFromUser.DatabasePathErasoft);
            //                    DatabasePathErasoft = accFromUser.DatabasePathErasoft;
            //                }
            //            }

//            var sessionAccount = System.Web.HttpContext.Current.Session["SessionAccount"];
//            var sessionAccountUserID = System.Web.HttpContext.Current.Session["SessionAccountUserID"];
//            var sessionAccountUserName = System.Web.HttpContext.Current.Session["SessionAccountUserName"];
//            var sessionAccountDataSourcePathDebug = System.Web.HttpContext.Current.Session["SessionAccountDataSourcePathDebug"];
//            var sessionAccountDataSourcePath = System.Web.HttpContext.Current.Session["SessionAccountDataSourcePath"];
//            var sessionAccountDatabasePathErasoft = System.Web.HttpContext.Current.Session["SessionAccountDatabasePathErasoft"];

//            var sessionUser = System.Web.HttpContext.Current.Session["SessionUser"];
//            var sessionUserAccountID = System.Web.HttpContext.Current.Session["SessionUserAccountID"];
//            var sessionUserUsername = System.Web.HttpContext.Current.Session["SessionUserUsername"];

//            if (sessionAccount != null)
//            {
//                if (sessionAccountUserID.ToString() == "admin_manage")
//                {
//                    ErasoftDbContext = new ErasoftContext();
//                }
//                else
//                {
//#if (Debug_AWS || DEBUG)
//                    dbSourceEra = sessionAccountDataSourcePathDebug.ToString();
//#else
//                    dbSourceEra = sessionAccountDataSourcePath.ToString();
//#endif
//                    ErasoftDbContext = new ErasoftContext(dbSourceEra, sessionAccountDatabasePathErasoft.ToString());
//                }

//                EDB = new DatabaseSQL(sessionAccountDatabasePathErasoft.ToString());
//                DatabasePathErasoft = sessionAccountDatabasePathErasoft.ToString();
//            }
//            else
//            {
//                if (sessionUser != null)
//                {
//                    var userAccID = Convert.ToInt64(sessionUserAccountID);
//                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == userAccID);
//                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
//#if (Debug_AWS || DEBUG)
//                    dbSourceEra = accFromUser.DataSourcePathDebug;
//#else
//                    dbSourceEra = accFromUser.DataSourcePath;
//#endif
//                    //ErasoftDbContext = new ErasoftContext(accFromUser.DataSourcePath, accFromUser.DatabasePathErasoft);
//                    ErasoftDbContext = new ErasoftContext(dbSourceEra, accFromUser.DatabasePathErasoft);
//                    DatabasePathErasoft = accFromUser.DatabasePathErasoft;
//                }
//            }
        }

        protected void SetupContext(string DatabasePathErasoft, string Username)
        {
            //string ret = "";
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, DatabasePathErasoft);
            username = Username;
            //var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.RecNum == data.idmarket).SingleOrDefault();
            //if (arf01inDB != null)
            //{
            //    ret = arf01inDB.TOKEN;
            //}
            //return ret;
        }

        [Route("lzdchat/code")]
        [HttpGet]
        public ActionResult LazadaCode(string user, string code)
        {
            var param = user.Split(new string[] { "_param_" }, StringSplitOptions.None);
            if (param.Count() == 2)
            {
                DatabaseSQL EDB = new DatabaseSQL(param[0]);
                var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET API_CLIENT_P = '" + code + "' WHERE CUST = '" + param[1] + "'");

                GetToken(param[0], param[1], code);
            }
            return View("LazadaAuthChat");
        }

        [HttpGet]
        public string LazadaUrlChat(string cust)
        {
            string userId = "";
            var sessionAccount = System.Web.HttpContext.Current.Session["SessionAccount"];
            var sessionAccountUserID = System.Web.HttpContext.Current.Session["SessionAccountUserID"];
            var sessionAccountUserName = System.Web.HttpContext.Current.Session["SessionAccountUserName"];
            var sessionAccountDataSourcePathDebug = System.Web.HttpContext.Current.Session["SessionAccountDataSourcePathDebug"];
            var sessionAccountDataSourcePath = System.Web.HttpContext.Current.Session["SessionAccountDataSourcePath"];
            var sessionAccountDatabasePathErasoft = System.Web.HttpContext.Current.Session["SessionAccountDatabasePathErasoft"];

            var sessionUser = System.Web.HttpContext.Current.Session["SessionUser"];
            var sessionUserAccountID = System.Web.HttpContext.Current.Session["SessionUserAccountID"];
            var sessionUserUsername = System.Web.HttpContext.Current.Session["SessionUserUsername"];

            if (sessionAccount != null)
            {
                userId = sessionAccountDatabasePathErasoft.ToString();

            }
            else
            {
                if (sessionUser != null)
                {
                    var userAccID = Convert.ToInt64(sessionUserAccountID);
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == userAccID);
                    userId = accFromUser.DatabasePathErasoft;
                }
            }

            string lzdId = cust;
            string compUrl = eraCallbackUrl + userId + "_param_" + lzdId;
            string uri = "https://auth.lazada.com/oauth/authorize?response_type=code&force_auth=true&redirect_uri=" + compUrl + "&client_id=" + eraAppKey + "&country=id";
            return uri;
        }

        public string GetToken(string user, string cust, string accessToken)
        {
            string ret;
            string url;
            url = "https://auth.lazada.com/rest";
            DatabaseSQL EDB = new DatabaseSQL(user);
            string EraServerName = EDB.GetServerName("sConn");
            ILazopClient client = new LazopClient(url, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest("/auth/token/create");
            request.SetHttpMethod("GET");
            request.AddApiParameter("code", accessToken);

            ErasoftDbContext = new ErasoftContext(EraServerName, user);
            //add 22 april 2021, handle spamming
            var cekLog = ErasoftDbContext.API_LOG_MARKETPLACE.Where(p => p.REQUEST_ACTION == "Get Token" && p.REQUEST_ATTRIBUTE_1 == cust
                && p.REQUEST_ATTRIBUTE_2 == accessToken && p.REQUEST_STATUS == "Success").FirstOrDefault();
            if (cekLog != null)
            {
                ret = "data sudah ada";
                return ret;
            }
            //end add 22 april 2021, handle spamming
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Token Chat",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = cust,
                REQUEST_ATTRIBUTE_2 = accessToken,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, "", currentLog);

            try
            {
                LazopResponse response = client.Execute(request);
                ret = "error:" + response.IsError() + "\nbody:" + response.Body;

                if (!response.IsError())
                {
                    var bindAuth = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaAuth)) as LazadaAuth;
                    // add by fauzi 20 februari 2020
                    var dateExpired = DateTime.UtcNow.AddSeconds(bindAuth.expires_in).ToString("yyyy-MM-dd HH:mm:ss");
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET TOKEN_CHAT = '" + bindAuth.access_token + "', REFRESH_TOKEN_CHAT = '" + bindAuth.refresh_token + "', STATUS_API_CHAT = '1', TGL_EXPIRED_CHAT = '" + dateExpired + "'  WHERE CUST = '" + cust + "'");
                    if (result == 1)
                    {
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, "", currentLog);
                        //GetShipment(cust, bindAuth.access_token);
                        GetSellerId(cust);
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = "failed to update token chat;execute result=" + result;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, "", currentLog);
                    }
                }
                else
                {
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API_CHAT = '0' WHERE CUST = '" + cust + "'");
                    currentLog.REQUEST_EXCEPTION = response.Body;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, "", currentLog);
                }
                return ret;
            }
            catch (Exception ex)
            {
                var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API_CHAT = '0' WHERE CUST = '" + cust + "'");
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, "", currentLog);
                return ex.ToString();
            }

        }

        public BindingBase GetSellerId(string cust)
        {
            var ret = new BindingBase();
            ret.status = 0;
            var accessToken = ErasoftDbContext.Database.SqlQuery<string>("SELECT TOP 1 ISNULL(TOKEN,'') FROM ARF01 (NOLOCK) WHERE CUST='" + cust + "'").FirstOrDefault();
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Seller Id",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = cust,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, accessToken, currentLog);

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/seller/get");
            request.SetHttpMethod("GET");
            //LazopResponse response = client.Execute(request, accessToken);
            try
            {
                LazopResponse response = client.Execute(request, accessToken);
                var bindDelivery = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(responseGetSellerInfo)) as responseGetSellerInfo;
                if (bindDelivery != null)
                {
                    if (bindDelivery.code.Equals("0"))
                    {
                        if (bindDelivery.data != null)
                        {
                            if (!string.IsNullOrEmpty(bindDelivery.data.seller_id.ToString()))
                            {
                                var tempCust = ErasoftDbContext.ARF01.Where(m => m.CUST == cust).FirstOrDefault();
                                if (tempCust != null)
                                {
                                    tempCust.Sort1_Cust = bindDelivery.data.seller_id.ToString();
                                    ErasoftDbContext.SaveChanges();
                                }
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, accessToken, currentLog);
                            }
                        }
                    }
                    else
                    {
                        ret.message = bindDelivery.message;
                        currentLog.REQUEST_EXCEPTION = bindDelivery.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, accessToken, currentLog);
                    }
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.ToString();
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, accessToken, currentLog);
            }
            return ret;
        }

        public LazadaAuth GetRefToken(string cust, string refreshToken)
        {
            LazadaAuth ret = new LazadaAuth();
            string url;
            url = "https://auth.lazada.com/rest";
            ILazopClient client = new LazopClient(url, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest("/auth/token/refresh");
            request.SetHttpMethod("GET");
            request.AddApiParameter("refresh_token", refreshToken);
            //moved to inside try catch
            //LazopResponse response = client.Execute(request);
            //end moved to inside try catch

            ////Console.WriteLine(response.IsError());
            ////Console.WriteLine(response.Body);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Refresh Token Chat",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = cust,
                REQUEST_ATTRIBUTE_2 = refreshToken,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, "", currentLog);
            try
            {
                LazopResponse response = client.Execute(request);
                //Console.WriteLine(response.IsError());
                //Console.WriteLine(response.Body);


                ret = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Body, typeof(LazadaAuth)) as LazadaAuth;
                if (!response.IsError())
                {
                    //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET TOKEN_CHAT = '" + ret.access_token + "', REFRESH_TOKEN_CHAT = '" + ret.refresh_token + "', STATUS_API_CHAT = '1'  WHERE CUST = '" + cust + "'");
                    if (result == 1)
                    {
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, "", currentLog);
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = "failed to update token;execute result=" + result;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, "", currentLog);
                    }
                }
                else
                {
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API_CHAT = '2' WHERE CUST = '" + cust + "'");
                    currentLog.REQUEST_EXCEPTION = response.Body;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, "", currentLog);
                }
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, "", currentLog);
                return null;
            }
            return ret;
        }

        [Route("SessionList")]
        public async Task<RootobjectSessionList> GetConversationList(string dbpath, string uname, string CUST, string accessToken, long milis, string last_sessionid)
        {
            SetupContext(dbpath, uname);
            DateTime date0 = DateTime.UtcNow;
            DateTime datenow = date0.AddHours(7);
            string datetimeDelete = datenow.AddDays(-3).ToString("yyyy-MM-dd HH:mm:ss");
            var ret = new RootobjectSessionList();
            string responses = "";
            //long ml = (long)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/im/session/list");
            request.SetHttpMethod("GET");

            request.AddApiParameter("start_time", milis.ToString());
            if (!string.IsNullOrEmpty(last_sessionid))
            {
                request.AddApiParameter("last_session_id", last_sessionid.ToString());
            }
            request.AddApiParameter("page_size", "20");

            //var cust_getmsg = "";
            LazopResponse response = client.Execute(request, accessToken);
            responses = response.Body;

            if (!string.IsNullOrEmpty(responses))
            {
                try
                {
                    //string res = responses.Replace("\\", "").Replace("\"{", "{").Replace("}\"", "}");
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responses, typeof(RootobjectSessionList)) as RootobjectSessionList;
                    var session_list = new List<string>();

                    if (result.data.session_list != null)
                    {
                        ret = result;

                        var listConversation = new List<LAZADA_SESSIONLIST>() { };
                        if (result.data.session_list.Count() > 0)
                        {
                            var sudah14hari = false;
                            var dateNow = DateTime.UtcNow.AddHours(7);
                            var dateLast1Month = dateNow.AddMonths(-1);
                            var dateLast14Days = dateNow.AddDays(-14);

                            //var cekAkunLazada = MoDbContext.Database.SqlQuery<TOKPED_SHOPID>("SELECT TOP 1 * FROM TOKPED_SHOPID WHERE cust='" + CUST + "' AND IDMARKET='7'").FirstOrDefault();

                            //var cekAccount = MoDbContext.Account.AsNoTracking().Where(a => a.AccountId == cekAkunLazada.ACCOUNTID).FirstOrDefault();

                            //ErasoftDbContext = new ErasoftContext(cekAccount.DataSourcePath, cekAccount.DatabasePathErasoft);

                            //var cust = ErasoftDbContext.ARF01.Where(a => a.Sort1_Cust == CUST).FirstOrDefault();
                            //if (cust != null)
                            //{
                            var cekListMessage = ErasoftDbContext.LAZADA_SESSIONLIST.Select(a => a.session_id).ToList();
                            var lastGetMessage = false;

                            while (!lastGetMessage)
                            {
                                foreach (var msg in result.data.session_list)
                                {
                                    var txtmsg = "";
                                    if (msg.summary == "text")
                                    {
                                        txtmsg = msg.last_message_id;
                                    }

                                    var date_last_message_time = DateTimeOffset.FromUnixTimeMilliseconds((msg.last_message_time)).UtcDateTime.AddHours(7);
                                    var last_message_time = date_last_message_time.ToString("yyyy-MM-dd HH:mm:ss");

                                    var message = new LAZADA_SESSIONLIST
                                    {
                                        session_id = msg.session_id,
                                        last_message_id = msg.last_message_id,
                                        last_message_time = Convert.ToDateTime(last_message_time),
                                        summary = msg.summary,
                                        head_url = msg.head_url,
                                        buyer_id = msg.buyer_id,
                                        self_position = msg.self_position,
                                        to_position = msg.to_position,
                                        site_id = msg.site_id,
                                        title = msg.title,
                                        //tags = msg.tags,
                                        //max_general_option_hide_time = msg.max_general_option_hide_time,
                                        //pinned = Convert.ToString(msg.pinned),
                                        //shop_id = msg.shop_id,
                                        //to_avatar = msg.to_avatar,
                                        //to_id = msg.to_id,
                                        //to_name = msg.to_name,
                                        unread_count = msg.unread_count,
                                        //17/09/2021
                                        cust = CUST,
                                        //

                                    };

                                    //masukin sampe -1 bulan 
                                    if (Convert.ToDateTime(last_message_time) < dateLast14Days)
                                    {
                                        sudah14hari = true;
                                        lastGetMessage = true; break;
                                    }
                                    //hanya masukin yg blm ada di list Conversation
                                    if (!cekListMessage.Contains(message.session_id))
                                    {
                                        listConversation.Add(message);
                                        session_list.Add(message.session_id);
                                        //cust_getmsg = CUST;
                                    }
                                    else
                                    {
                                        var getConversation = ErasoftDbContext.LAZADA_SESSIONLIST.Where(a => a.session_id == message.session_id).FirstOrDefault();
                                        if (getConversation != null)
                                        {
                                            getConversation.last_message_time = message.last_message_time;
                                            getConversation.unread_count = message.unread_count;
                                            getConversation.summary = message.summary;
                                            getConversation.last_message_id = message.last_message_id;
                                            ErasoftDbContext.SaveChanges();
                                            session_list.Add(message.session_id);
                                        }
                                    }

                                }
                                lastGetMessage = true; break;

                            }

                            if (listConversation.Count() > 0)
                            {
                                ErasoftDbContext.LAZADA_SESSIONLIST.AddRange(listConversation);
                                ErasoftDbContext.SaveChanges();
                            }
                            if (session_list.Count() > 0)
                            {
                                foreach (var session in session_list)
                                {
                                    GetGetMessage(dbpath, uname, session, CUST, accessToken, milis, "").Start();
                                }
                            }
                            if (!sudah14hari)
                            {
                                if (!string.IsNullOrEmpty(result.data.last_session_id))
                                {
                                    if (!string.IsNullOrEmpty(result.data.next_start_time.ToString()))
                                    {
                                        await GetConversationList(dbpath, uname, CUST, accessToken, Convert.ToInt64(result.data.next_start_time), result.data.last_session_id);
                                    }
                                }
                            }

                            //}
                        }

                    }
                }
                catch (Exception ex2)
                {
                    var i = ex2.ToString();
                }

            }
            return ret;
        }

        [Route("GetMessage")]
        public async Task<RootobjectGetMsg> GetGetMessage(string dbpath, string uname, string session, string cust_getmsg, string accessToken, long milis, string last_msg_id)
        {
            SetupContext(dbpath, uname);
            var ret = new RootobjectGetMsg();
            //long ml = (long)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            //var session_id = session_list;
            String responses;

            //var cekListSession = ErasoftDbContext.LAZADA_SESSIONLIST.Select(a => a.session_id).ToList();
            //var lastGetSession = false;
            //while (!lastGetSession)
            //{
            //    foreach (var session in session_id)
            //    {
            ILazopClient client = new LazopClient(urlLazada, eraAppKey, eraAppSecret);
            LazopRequest request = new LazopRequest();
            request.SetApiName("/im/message/list");
            request.SetHttpMethod("GET");

            request.AddApiParameter("session_id", session);
            request.AddApiParameter("start_time", milis.ToString());
            if (!string.IsNullOrEmpty(last_msg_id))
            {
                request.AddApiParameter("last_message_id", last_msg_id);
            }
            request.AddApiParameter("page_size", "20");


            LazopResponse response = client.Execute(request, accessToken);
            responses = response.Body;



            if (!string.IsNullOrEmpty(responses))
            {
                try
                {
                    string res = responses.Replace("\\", "").Replace("\"{", "{").Replace("}\"", "}");
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject(res, typeof(RootobjectGetMsg)) as RootobjectGetMsg;

                    if (!string.IsNullOrEmpty(result.data.last_message_id))
                    {
                        ret = result;

                        var listMsg = new List<LAZADA_MESSAGES>() { };
                        if (result.data.message_list.Count() > 0)
                        {
                            var sudah14hari = false;
                            var dateNow = DateTime.UtcNow.AddHours(7);
                            var dateLast1Month = dateNow.AddMonths(-1);
                            var dateLast14Days = dateNow.AddDays(-14);
                            string cust = cust_getmsg.ToString();
                            //var cust //lempar sellerid dari convlist
                            if (!string.IsNullOrEmpty(cust))
                            {
                                var cekListMessage = ErasoftDbContext.LAZADA_MESSAGES.AsNoTracking().Select(a => a.message_id).ToList();
                                var lastGetMessage = false;


                                while (!lastGetMessage)
                                {

                                    foreach (var msg in result.data.message_list)
                                    {
                                        var created_sendtime = DateTimeOffset.FromUnixTimeMilliseconds(msg.send_time).UtcDateTime.AddHours(7);
                                        var last_created_sendtime = created_sendtime.ToString("yyyy-MM-dd HH:mm:ss");

                                        var orderId = "";
                                        if (string.IsNullOrEmpty(msg.content.orderId))
                                        {
                                            orderId = "";
                                        }
                                        else
                                        {
                                            orderId = msg.content.orderId;
                                        }

                                        //var txt = "";
                                        //if (string.IsNullOrEmpty(msg.content.txt))
                                        //{
                                        //    txt = "";
                                        //}
                                        //else
                                        //{
                                        //    txt = msg.content.txt;
                                        //}

                                        var actionUrl = "";
                                        if (string.IsNullOrEmpty(msg.content.actionUrl))
                                        {
                                            actionUrl = "";
                                        }
                                        else
                                        {
                                            actionUrl = msg.content.actionUrl;
                                        }

                                        var iconUrl = "";
                                        if (string.IsNullOrEmpty(msg.content.iconUrl))
                                        {
                                            iconUrl = "";
                                        }
                                        else
                                        {
                                            iconUrl = msg.content.iconUrl;
                                        }

                                        var title = "";
                                        if (string.IsNullOrEmpty(msg.content.title))
                                        {
                                            title = "";
                                        }
                                        else
                                        {
                                            title = msg.content.title;
                                        }

                                        var content = "";
                                        if (string.IsNullOrEmpty(msg.content.content))
                                        {
                                            content = "";
                                        }
                                        else
                                        {
                                            content = msg.content.content;
                                        }

                                        var status = "";
                                        if (string.IsNullOrEmpty(msg.content.status))
                                        {
                                            status = "";
                                        }
                                        else
                                        {
                                            status = msg.content.status;
                                        }

                                        var itemId = "";
                                        if (string.IsNullOrEmpty(msg.content.itemId))
                                        {
                                            itemId = "";
                                        }
                                        else
                                        {
                                            itemId = msg.content.itemId;
                                        }

                                        var price = "";
                                        if (string.IsNullOrEmpty(msg.content.price))
                                        {
                                            price = "";
                                        }
                                        else
                                        {
                                            price = msg.content.price;
                                        }

                                        var skuId = "";
                                        if (string.IsNullOrEmpty(msg.content.skuId))
                                        {
                                            skuId = "";
                                        }
                                        else
                                        {
                                            skuId = msg.content.skuId;
                                        }

                                        var imgUrl = "";
                                        if (string.IsNullOrEmpty(msg.content.imgUrl))
                                        {
                                            imgUrl = "";
                                        }
                                        else
                                        {
                                            imgUrl = msg.content.imgUrl;
                                        }

                                        var smallImgUrl = "";
                                        if (string.IsNullOrEmpty(msg.content.smallImgUrl))
                                        {
                                            smallImgUrl = "";
                                        }
                                        else
                                        {
                                            smallImgUrl = msg.content.smallImgUrl;
                                        }

                                        var osskey = "";
                                        if (string.IsNullOrEmpty(msg.content.osskey))
                                        {
                                            osskey = "";
                                        }
                                        else
                                        {
                                            osskey = msg.content.osskey;
                                        }

                                        var width = "";
                                        if (string.IsNullOrEmpty(msg.content.width))
                                        {
                                            width = "";
                                        }
                                        else
                                        {
                                            width = msg.content.width;
                                        }

                                        var height = "";
                                        if (string.IsNullOrEmpty(msg.content.height))
                                        {
                                            height = "";
                                        }
                                        else
                                        {
                                            height = msg.content.height;
                                        }

                                        var text = "";
                                        if (msg.template_id == "1") //txt
                                        {
                                            if (!string.IsNullOrEmpty(msg.content.txt))
                                            {
                                                text = msg.content.txt;
                                            }
                                            else
                                            {
                                                text = "*Ada pesan baru masuk dengan tipe text.";
                                            }
                                        }
                                        else if (msg.template_id == "3") //image
                                        {
                                            if (!string.IsNullOrEmpty(msg.content.imgUrl))
                                            {
                                                //text = result.data.content.content.url;
                                                var url_ = msg.content.imgUrl;
                                                text = "<u><a rel=\"nofollow\" target=\"blank\" href=\"" + url_ + "\">Link Gambar</a></u>";
                                                //height = msg.content.height;
                                                //width = msg.content.width;
                                                //osskey = msg.content.osskey;
                                            }
                                            else
                                            {
                                                text = "*Ada pesan baru masuk dengan tipe image.";
                                                //height = msg.content.height;
                                                //width = msg.content.width;
                                                //osskey = msg.content.osskey;
                                            }
                                        }
                                        else if (msg.template_id == "10007") //order
                                        {
                                            if (!string.IsNullOrEmpty(msg.content.orderId))
                                            {
                                                //text = "https://seller.shopee.co.id/portal/sale/order/?search=" + result.data.content.content.order_sn;
                                                var url_ = "https://sellercenter.lazada.co.id/order/detail/" + msg.content.orderId + "/" + msg.to_account_id;
                                                text = "<u><a rel=\"nofollow\" target=\"blank\" href=\"" + url_ + "\">Link Produk " + msg.content.title + "</a></u>";
                                            }
                                            else
                                            {
                                                text = "*Ada pesan baru masuk dengan tipe order";
                                            }
                                        }
                                        else if (msg.template_id == "10006")
                                        {
                                            if (!string.IsNullOrEmpty(msg.content.itemId))
                                            {
                                                //text = "https://shopee.co.id/product/" + listShopeeShop.Sort1_Cust + "/" + result.data.content.content.item_id.ToString();
                                                var url_ = msg.content.actionUrl;
                                                text = "<u><a rel=\"nofollow\" target=\"blank\" href=\"" + url_ + "\">Link Produk " + msg.content.title + "</a></u>";
                                            }
                                            else
                                            {
                                                text = "*Ada pesan baru masuk dengan tipe item.";
                                            }
                                        }
                                        else if (msg.template_id == "4")
                                        {
                                            if (!string.IsNullOrEmpty(msg.content.smallImgUrl))
                                            {
                                                var url_ = msg.content.imgUrl;
                                                text = "<u><a rel=\"nofollow\" target=\"blank\" href=\"" + url_ + "\"><img src=\"" + msg.content.smallImgUrl + "\"></a></u>";
                                            }
                                            else
                                            {
                                                text = "*Ada pesan baru masuk dengan tipe stiker.";
                                            }
                                        }
                                        else
                                        {
                                            text = "*Ada pesan baru masuk dengan template id = " + msg.template_id;
                                        }

                                        var message = new LAZADA_MESSAGES
                                        {
                                            session_id = msg.session_id,
                                            message_id = msg.message_id,
                                            last_message_id = result.data.last_message_id,
                                            type = msg.type,
                                            from_account_type = msg.from_account_type,
                                            from_account_id = msg.from_account_id,
                                            to_account_type = msg.to_account_type,
                                            to_account_id = msg.to_account_id,
                                            send_time = created_sendtime,
                                            template_id = msg.template_id,
                                            site_id = msg.site_id,
                                            auto_reply = msg.auto_reply,
                                            status = msg.status,
                                            txt = text,
                                            orderId = orderId,
                                            actionUrl = actionUrl,
                                            iconUrl = iconUrl,
                                            title = title,
                                            content_order = content,
                                            status_order = status,
                                            itemId = itemId,
                                            price = price,
                                            skuId = skuId,
                                            smallImgUrl = smallImgUrl,
                                            imgUrl = imgUrl,
                                            osskey = osskey,
                                            width = width,
                                            height = height,
                                            //17/09/2021
                                            cust = cust,
                                            //
                                        };
                                        //masukin sampe -1 bulan 
                                        if (created_sendtime < dateLast14Days)
                                        {
                                            sudah14hari = true;
                                            lastGetMessage = true; break;
                                        }
                                        //hanya masukin yg blm ada di list Conversation
                                        if (!cekListMessage.Contains(message.message_id))
                                        {
                                            listMsg.Add(message);
                                        }
                                    }




                                    lastGetMessage = true; break;
                                }
                                if (listMsg.Count() > 0)
                                {
                                    try
                                    {
                                        ErasoftDbContext.LAZADA_MESSAGES.AddRange(listMsg).OrderByDescending(a => a.send_time);
                                        ErasoftDbContext.SaveChanges();
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                if (!sudah14hari)
                                {
                                    if (!string.IsNullOrEmpty(result.data.last_message_id))
                                    {
                                        if (!string.IsNullOrEmpty(result.data.next_start_time.ToString()))
                                        {
                                            await GetGetMessage(dbpath, uname, session, cust_getmsg, accessToken, result.data.next_start_time, result.data.last_message_id);
                                        }
                                    }
                                }
                            }

                            //if (string.IsNullOrEmpty(offset)) // page 1
                            //{
                            //    if (result.response.page_result.page_size == 25) // max show per page 25, must check with offset for another page
                            //    {
                            //        await GetGetMessage(dataAPI, EraServerName, conversation_id, result.response.page_result.next_offset);
                            //    }
                            //}
                            //else
                            //{
                            //    await GetGetMessage(dataAPI, EraServerName, conversation_id, result.response.page_result.next_offset);
                            //}
                        }
                    }

                }
                catch (Exception ex2)
                {
                    var i = ex2.ToString();
                }
                //        }

                //    }
                //    lastGetSession = true; break;
            }
            return ret;
        }

        public enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4
        }
        public void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, string iden, API_LOG_MARKETPLACE data)
        {
            switch (action)
            {
                case api_status.Pending:
                    {
                        var arf01 = ErasoftDbContext.ARF01.Where(p => p.TOKEN == iden).FirstOrDefault();
                        var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                        {
                            CUST = arf01 != null ? arf01.CUST : "",
                            CUST_ATTRIBUTE_1 = arf01 != null ? arf01.PERSO : "",
                            CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                            CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                            CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                            CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                            MARKETPLACE = "Lazada",
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
                        var apiLogInDb = ErasoftDbContext.API_LOG_MARKETPLACE.FirstOrDefault(p => p.REQUEST_ID == data.REQUEST_ID);
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


    public class responseGetSellerInfo
    {
        public string type { get; set; }
        public string message { get; set; }
        public Data data { get; set; }
        public string code { get; set; }
        public string request_id { get; set; }
    }

    public class Data
    {
        public string name_company { get; set; }
        public string name { get; set; }
        public bool verified { get; set; }
        public string location { get; set; }
        public long seller_id { get; set; }
        public string email { get; set; }
        public string short_code { get; set; }
        public bool cb { get; set; }
        public string status { get; set; }
    }


    public class RootobjectSessionList
    {
        public DataSessionList data { get; set; }
        public string success { get; set; }
        public string err_code { get; set; }
        public string err_message { get; set; }
        public string code { get; set; }
        public string request_id { get; set; }
    }

    public class DataSessionList
    {
        public Session_List[] session_list { get; set; }
        public string next_start_time { get; set; }
        public string has_more { get; set; }
        public string last_session_id { get; set; }
    }

    public class Session_List
    {
        public string summary { get; set; }
        public long unread_count { get; set; }
        public string last_message_id { get; set; }
        public string head_url { get; set; }
        public string self_position { get; set; }
        public long last_message_time { get; set; }
        public string site_id { get; set; }
        public string session_id { get; set; }
        public string title { get; set; }
        public string buyer_id { get; set; }
        public string to_position { get; set; }
        //public string tags { get; set; }
    }


    public class RootobjectGetMsg
    {
        public DataGetMsg data { get; set; }
        public bool success { get; set; }
        public string err_code { get; set; }
        public string code { get; set; }
        public string request_id { get; set; }
    }

    public class DataGetMsg
    {
        public string last_message_id { get; set; }
        public Message_List[] message_list { get; set; }
        public long next_start_time { get; set; }
        public bool has_more { get; set; }
    }

    public class Message_List
    {
        public string from_account_type { get; set; }
        public string session_id { get; set; }
        public string message_id { get; set; }
        public string type { get; set; }
        public ContentGetMsg content { get; set; }
        public string to_account_id { get; set; }
        public long send_time { get; set; }
        public string auto_reply { get; set; }
        public string to_account_type { get; set; }
        public string site_id { get; set; }
        public string template_id { get; set; }
        public string from_account_id { get; set; }
        public string status { get; set; }
    }

    public class ContentGetMsg
    {
        public string txt { get; set; }
        public string orderId { get; set; }
        public string actionUrl { get; set; }
        public string iconUrl { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string status { get; set; }
        public string itemId { get; set; }
        public string price { get; set; }
        public string skuId { get; set; }
        public string imgUrl { get; set; }
        public string osskey { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string smallImgUrl { get; set; }
    }

}