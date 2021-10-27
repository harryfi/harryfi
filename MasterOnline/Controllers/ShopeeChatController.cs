﻿using System;
using System.CodeDom;
using System.CodeDom.Compiler;
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

namespace MasterOnline.Controllers
{
    public class ShopeeChatController : Controller
    {
        //AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);//string auth = Base64Encode();
#if AWS
        string shpCallbackUrl = "https://masteronline.co.id/shp/code?user=";
        string shpCallbackUrlV2 = "https://masteronline.co.id/shp/v2/code?user=";
#else
        //string shpCallbackUrl = "https://dev.masteronline.co.id/shp/code?user=";
        //string shpCallbackUrlV2 = "http://dev.masteronline.co.id/shp/v2/code?user=";
        string shpCallbackUrlV2 = "http://localhost:50109/shp/v2/codechat?user=";
        string shpCallbackUrl = "https://masteronline.my.id/shp/code?user=";
#endif
        string shopeeV2Url = "https://partner.shopeemobile.com";
        int MOPartnerIDV2 = 2000905;
        string MOPartnerKeyV2 = "b0fc80f8e4c8391fe5a0dc7bf8ff3aa170ac2be00dc9ce71d66317aebc2ba7e9";
        //string shopeeV2Url = "https://partner.test-stable.shopeemobile.com";
        //int MOPartnerIDV2 = 1000723;
        //string MOPartnerKeyV2 = "d59a300f63f9d36b92f71b0ccb5b37e4e2b43e9c567df3f2e2808136dd4893dd";

        int loop_page = 0;

        protected int MOPartnerID = 841371;
        protected string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        DatabaseSQL EDB;
        string username;
        string DatabasePathErasoft;
        string dbSourceEra = "";
        private static string ToLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }

        public ShopeeChatController()
        {
            MoDbContext = new MoDbContext("");
            username = "";
            //            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
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
            //                username = sessionData.Account.Username;
            //            }
            //            else
            //            {
            //                if (sessionData?.User != null)
            //                {
            //                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
            //#if (Debug_AWS)
            //                    dbSourceEra = accFromUser.DataSourcePathDebug;
            //#else
            //                    dbSourceEra = accFromUser.DataSourcePath;
            //#endif
            //                    //ErasoftDbContext = new ErasoftContext(accFromUser.DataSourcePath, accFromUser.DatabasePathErasoft);
            //                    ErasoftDbContext = new ErasoftContext(dbSourceEra, accFromUser.DatabasePathErasoft);
            //                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
            //                    DatabasePathErasoft = accFromUser.DatabasePathErasoft;
            //                    username = accFromUser.Username;
            //                }
            //            }

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
                if (sessionAccountUserID.ToString() == "admin_manage")
                {
                    ErasoftDbContext = new ErasoftContext();
                }
                else
                {
#if (Debug_AWS || DEBUG)
                    dbSourceEra = sessionAccountDataSourcePathDebug.ToString();
#else
                    dbSourceEra = sessionAccountDataSourcePath.ToString();
#endif
                    ErasoftDbContext = new ErasoftContext(dbSourceEra, sessionAccountDatabasePathErasoft.ToString());
                }
                EDB = new DatabaseSQL(sessionAccountDatabasePathErasoft.ToString());
                DatabasePathErasoft = sessionAccountDatabasePathErasoft.ToString();
                username = sessionAccountUserName.ToString();
            }
            else
            {
                if (sessionUser != null)
                {
                    var userAccID = Convert.ToInt64(sessionUserAccountID);
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == userAccID);
#if (Debug_AWS || DEBUG)
                    dbSourceEra = accFromUser.DataSourcePathDebug;
#else
                    dbSourceEra = accFromUser.DataSourcePath;
#endif
                    ErasoftDbContext = new ErasoftContext(dbSourceEra, accFromUser.DatabasePathErasoft);
                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                    DatabasePathErasoft = accFromUser.DatabasePathErasoft;
                    username = accFromUser.Username;
                }
            }

            if (username.Length > 20)
                username = username.Substring(0, 17) + "...";
        }
        //[Route("shp/code")]
        //[HttpGet]
        //public ActionResult ShopeeCode(string user, string shop_id)
        //{
        //    var param = user.Split(new string[] { "_param_" }, StringSplitOptions.None);
        //    if (param.Count() == 2)
        //    {
        //        ShopeeChatController.ShopeeAPIData dataSp = new ShopeeChatController.ShopeeAPIData()
        //        {
        //            merchant_code = shop_id,
        //            DatabasePathErasoft = param[0],
        //            no_cust = param[1],
        //        };
        //        Task.Run(() => GetTokenShopee(dataSp, true)).Wait();
        //    }
        //    return View("ShopeeAuth");
        //}
        [Route("shp/v2/codechat")]
        [HttpGet]
        public ActionResult ShopeeCode_V2(string user, string shop_id, string code)
        {
            var param = user.Split(new string[] { "_param_" }, StringSplitOptions.None);
            if (param.Count() == 2)
            {
                ShopeeChatController.ShopeeAPIDataChat dataSp = new ShopeeChatController.ShopeeAPIDataChat()
                {
                    merchant_code = shop_id,
                    DatabasePathErasoft = param[0],
                    no_cust = param[1],
                    API_secret_key = code
                };
                Task.Run(() => GetTokenShopee_V2(dataSp, true)).Wait();
            }
            return View("ShopeeAuthChat");
        }

        //public async Task<BindingBase> GetItemsList(ShopeeAPIData iden, int IdMarket, int page, int recordCount, int totalData)
        //{
        //    int MOPartnerID = 841371;
        //    string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
        //    string urll = "https://partner.shopeemobile.com/api/v1/items/get";
        //    if (!string.IsNullOrEmpty(iden.token))
        //    {
        //        MOPartnerID = MOPartnerIDV2;
        //        MOPartnerKey = MOPartnerKeyV2;
        //        urll = shopeeV2Url + "/api/v1/items/get";
        //    }
        //    //string ret = "";
        //    var ret = new BindingBase
        //    {
        //        status = 0,
        //        recordCount = recordCount,
        //        exception = 0,
        //        totalData = totalData//add 18 Juli 2019, show total record
        //    };

        //    long seconds = CurrentTimeSecond();
        //    DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

        //    MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
        //    {
        //        REQUEST_ID = seconds.ToString() + "_" + page,
        //        REQUEST_ACTION = "Get Item List",
        //        REQUEST_DATETIME = milisBack,
        //        REQUEST_ATTRIBUTE_1 = iden.merchant_code,
        //        REQUEST_ATTRIBUTE_3 = page.ToString(),
        //        REQUEST_STATUS = "Pending",
        //    };
        //    manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

        //    //string urll = "https://partner.shopeemobile.com/api/v1/items/get";

        //    ShopeeGetItemListData HttpBody = new ShopeeGetItemListData
        //    {
        //        partner_id = MOPartnerID, //MasterOnline Partner ID
        //        shopid = Convert.ToInt32(iden.merchant_code),
        //        timestamp = seconds,
        //        pagination_offset = page * 5,
        //        pagination_entries_per_page = 5,

        //    };

        //    string myData = JsonConvert.SerializeObject(HttpBody);

        //    string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

        //    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
        //    myReq.Method = "POST";
        //    myReq.Headers.Add("Authorization", signature);
        //    myReq.Accept = "application/json";
        //    myReq.ContentType = "application/json";
        //    string responseFromServer = "";
        //    try
        //    {
        //        myReq.ContentLength = myData.Length;
        //        using (var dataStream = myReq.GetRequestStream())
        //        {
        //            dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
        //        }
        //        using (WebResponse response = await myReq.GetResponseAsync())
        //        {
        //            using (Stream stream = response.GetResponseStream())
        //            {
        //                StreamReader reader = new StreamReader(stream);
        //                responseFromServer = reader.ReadToEnd();
        //            }
        //        }
        //        //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
        //    }
        //    catch (Exception ex)
        //    {
        //        ret.nextPage = 1;
        //        ret.exception = 1;
        //        currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
        //        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
        //    }

        //    if (!string.IsNullOrEmpty(responseFromServer))
        //    {
        //        try
        //        {
        //            var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetItemListResult)) as ShopeeGetItemListResult;
        //            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
        //            //add 13 Feb 2019, tuning


        //            //var stf02h_local = ErasoftDbContext.STF02H.Select(p => new stf02h_local { BRG = p.BRG, BRG_MP = p.BRG_MP, IDMARKET = p.IDMARKET }).Where(m => m.IDMARKET == IdMarket).ToList();
        //            //var tempBrg_local = ErasoftDbContext.TEMP_BRG_MP.Select(p=> new tempBrg_local { BRG_MP = p.BRG_MP, IDMARKET = p.IDMARKET }).Where(m => m.IDMARKET == IdMarket).ToList();

        //            //var stf02h_local = (from a in ErasoftDbContext.STF02H where a.IDMARKET == IdMarket select new stf02h_local { BRG = a.BRG, BRG_MP = a.BRG_MP, IDMARKET = a.IDMARKET }).ToList();
        //            //var tempBrg_local = (from a in ErasoftDbContext.TEMP_BRG_MP where a.IDMARKET == IdMarket select new tempBrg_local { BRG_MP = a.BRG_MP, IDMARKET = a.IDMARKET }).ToList();

        //            //end add 13 Feb 2019, tuning
        //            ret.status = 1;
        //            if (listBrg.items != null)
        //            {
        //                var listBrgMP = new List<string>();
        //                foreach (var lis in listBrg.items)
        //                {
        //                    listBrgMP.Add(lis.item_id.ToString() + ";0");
        //                    if (lis.variations.Length > 0)
        //                    {
        //                        foreach (var listVar in lis.variations)
        //                        {
        //                            listBrgMP.Add(lis.item_id.ToString() + ";" + listVar.variation_id);
        //                        }
        //                    }
        //                }
        //                var stf02h_local = (from a in ErasoftDbContext.STF02H where a.IDMARKET == IdMarket && listBrgMP.Contains(a.BRG_MP) select new stf02h_local { BRG = a.BRG, BRG_MP = a.BRG_MP, IDMARKET = a.IDMARKET }).ToList();
        //                var tempBrg_local = (from a in ErasoftDbContext.TEMP_BRG_MP where a.IDMARKET == IdMarket && listBrgMP.Contains(a.BRG_MP) select new tempBrg_local { BRG_MP = a.BRG_MP, IDMARKET = a.IDMARKET }).ToList();
        //                //if (listBrg.items.Length == 10)
        //                if (listBrg.more)
        //                    //ret.message = (page + 1).ToString();
        //                    ret.nextPage = 1;
        //                if (listBrgMP.Count == (stf02h_local.Count + tempBrg_local.Count))
        //                {
        //                    ret.totalData += listBrgMP.Count;
        //                    return ret;
        //                }
        //                ret.totalData += listBrg.items.Count();//add 18 Juli 2019, show total record
        //                foreach (var item in listBrg.items)
        //                {
        //                    if (item.status.ToUpper() != "BANNED" && item.status.ToUpper() != "DELETED")
        //                    {
        //                        //if (item.item_id == 1512392638 || item.item_id == 1790099887 || item.item_sku == "1660" || item.item_sku == "51")
        //                        //{

        //                        //}
        //                        string kdBrg = string.IsNullOrEmpty(item.item_sku) ? item.item_id.ToString() : item.item_sku;
        //                        string brgMp = item.item_id.ToString() + ";0";
        //                        //change 13 Feb 2019, tuning
        //                        //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.ToUpper().Equals(brgMp.ToUpper()) && t.IDMARKET == IdMarket).FirstOrDefault();
        //                        //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(brgMp) && t.IDMARKET == IdMarket).FirstOrDefault();
        //                        //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.IDMARKET == IdMarket && (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
        //                        //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.IDMARKET == IdMarket && (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
        //                        var tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
        //                        var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
        //                        //end change 13 Feb 2019, tuning

        //                        if ((tempbrginDB == null && brgInDB == null) || item.variations.Length > 0)
        //                        {
        //                            //var getDetailResult = await GetItemDetail(iden, item.item_id);
        //                            //var getDetailResult = await GetItemDetail(iden, item.item_id, new List<tempBrg_local>(), new List<stf02h_local>(), IdMarket);
        //                            var getDetailResult = await GetItemDetail(iden, item.item_id, tempBrg_local, stf02h_local, IdMarket);
        //                            ret.totalData += getDetailResult.totalData;//add 18 Juli 2019, show total record
        //                            if (getDetailResult.exception == 1)
        //                                ret.exception = 1;
        //                            if (getDetailResult.status == 1)
        //                            {
        //                                ret.recordCount += getDetailResult.recordCount;
        //                            }
        //                            else
        //                            {
        //                                currentLog.REQUEST_EXCEPTION = getDetailResult.message;
        //                                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                ret.nextPage = 0;
        //            }


        //        }
        //        catch (Exception ex2)
        //        {
        //            ret.nextPage = 1;
        //            ret.exception = 1;
        //            currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
        //            manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
        //        }
        //    }
        //    return ret;
        //}
        //public async Task<BindingBase> GetItemsList_V2(ShopeeAPIData iden, int IdMarket, int page, int recordCount, int totalData, bool display)
        //{
        //    iden = await RefreshTokenShopee_V2(iden, false);

        //    var ret = new BindingBase
        //    {
        //        status = 0,
        //        recordCount = recordCount,
        //        exception = 0,
        //        totalData = totalData//add 18 Juli 2019, show total record
        //    };

        //    long seconds = CurrentTimeSecond();
        //    DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

        //    MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
        //    {
        //        REQUEST_ID = seconds.ToString() + "_" + page,
        //        REQUEST_ACTION = "Get Item List V2 " + (display ? "Active" : "Not Active"),
        //        REQUEST_DATETIME = milisBack,
        //        REQUEST_ATTRIBUTE_1 = iden.merchant_code,
        //        REQUEST_ATTRIBUTE_3 = page.ToString(),
        //        REQUEST_STATUS = "Pending",
        //    };
        //    manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

        //    int MOPartnerID = MOPartnerIDV2;
        //    string MOPartnerKey = MOPartnerKeyV2;
        //    string urll = shopeeV2Url;
        //    string path = "/api/v2/product/get_item_list";

        //    var baseString = MOPartnerID + path + seconds + iden.token + iden.merchant_code;
        //    var sign = CreateSignAuthenShop_V2(baseString, MOPartnerKey);

        //    string param = "?partner_id=" + MOPartnerID + "&timestamp=" + seconds + "&sign=" + sign
        //        + "&access_token=" + iden.token + "&shop_id=" + iden.merchant_code
        //        + "&offset=" + (page * 5) + "&page_size=" + 5 + "&item_status=" + (display ? "NORMAL" : "UNLIST");

        //    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + path + param);
        //    myReq.Method = "GET";
        //    //myReq.Headers.Add("Authorization", signature);
        //    myReq.Accept = "application/json";
        //    myReq.ContentType = "application/json";
        //    string responseFromServer = "";
        //    try
        //    {
        //        using (WebResponse response = await myReq.GetResponseAsync())
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
        //        ret.nextPage = 0;
        //        ret.exception = 1;
        //        currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
        //        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
        //    }

        //    if (!string.IsNullOrEmpty(responseFromServer))
        //    {
        //        try
        //        {
        //            var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetItemListResult_V2)) as ShopeeGetItemListResult_V2;
        //            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
        //            ret.status = 1;
        //            if (string.IsNullOrEmpty(listBrg.error))
        //            {
        //                if (listBrg.response != null)
        //                {
        //                    var listBrgMP = new List<string>();
        //                    var qryKdBrg = "";
        //                    if(listBrg.response.item == null)
        //                    {
        //                        ret.nextPage = 0;
        //                        if (display)
        //                        {
        //                            ret.nextPage = 1;
        //                            ret.message = "MOVE_TO_INACTIVE_PRODUCTS";
        //                        }
        //                        return ret;
        //                    }
        //                    foreach (var lis in listBrg.response.item)
        //                    {
        //                        listBrgMP.Add(lis.item_id.ToString() + ";0");
        //                        if(!string.IsNullOrEmpty(qryKdBrg))
        //                        {
        //                            qryKdBrg += " or ";
        //                        }
        //                        qryKdBrg += " BRG_MP LIKE '"+ lis.item_id.ToString() + ";%' ";
        //                    }
        //                    //var stf02h_local = (from a in ErasoftDbContext.STF02H where a.IDMARKET == IdMarket && listBrgMP.Contains(a.BRG_MP) select new stf02h_local { BRG = a.BRG, BRG_MP = a.BRG_MP, IDMARKET = a.IDMARKET }).ToList();
        //                    //var tempBrg_local = (from a in ErasoftDbContext.TEMP_BRG_MP where a.IDMARKET == IdMarket && listBrgMP.Contains(a.BRG_MP) select new tempBrg_local { BRG_MP = a.BRG_MP, IDMARKET = a.IDMARKET }).ToList();
        //                    var stf02h_local = ErasoftDbContext.Database.SqlQuery<stf02h_local>("SELECT BRG, BRG_MP, IDMARKET FROM STF02H WHERE IDMARKET = "+ IdMarket + " AND ("+ qryKdBrg + ")").ToList();
        //                    var tempBrg_local = ErasoftDbContext.Database.SqlQuery<tempBrg_local>("SELECT BRG_MP, IDMARKET FROM TEMP_BRG_MP WHERE IDMARKET = " + IdMarket + " AND ("+ qryKdBrg + ")").ToList();
        //                    //if (listBrg.items.Length == 10)
        //                    if (listBrg.response.has_next_page)
        //                    {
        //                        //ret.message = (page + 1).ToString();
        //                        ret.nextPage = 1;
        //                        if (!display)
        //                            ret.message = "MOVE_TO_INACTIVE_PRODUCTS";
        //                    }
        //                    else
        //                    {
        //                        if (display)
        //                        {
        //                            ret.nextPage = 1;
        //                            ret.message = "MOVE_TO_INACTIVE_PRODUCTS";
        //                        }
        //                    }
        //                    //if (listBrgMP.Count == (stf02h_local.Count + tempBrg_local.Count))
        //                    //{
        //                    //    ret.totalData += listBrgMP.Count;
        //                    //    return ret;
        //                    //}
        //                    ret.totalData += listBrg.response.item.Count();//add 18 Juli 2019, show total record
        //                    foreach (var item in listBrg.response.item)
        //                    {
        //                        if (item.item_status.ToUpper() != "BANNED" && item.item_status.ToUpper() != "DELETED")
        //                        {
        //                            //if ((tempbrginDB == null && brgInDB == null) || item.variations.Length > 0)
        //                            {
        //                                var getDetailResult = await GetItemDetail_V2(iden, item.item_id, tempBrg_local, stf02h_local, IdMarket);
        //                                ret.totalData += getDetailResult.totalData;//add 18 Juli 2019, show total record
        //                                if (getDetailResult.exception == 1)
        //                                    ret.exception = 1;
        //                                if (getDetailResult.status == 1)
        //                                {
        //                                    ret.recordCount += getDetailResult.recordCount;
        //                                }
        //                                else
        //                                {
        //                                    currentLog.REQUEST_EXCEPTION = getDetailResult.message;
        //                                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    ret.nextPage = 0;
        //                    if (display)
        //                    {
        //                        ret.nextPage = 1;
        //                        ret.message = "MOVE_TO_INACTIVE_PRODUCTS";
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                ret.nextPage = 0;
        //                ret.exception = 1;
        //                currentLog.REQUEST_EXCEPTION = listBrg.error + listBrg.message;
        //                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
        //            }

        //        }
        //        catch (Exception ex2)
        //        {
        //            ret.nextPage = 1;
        //            ret.exception = 1;
        //            currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
        //            manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
        //        }
        //    }
        //    return ret;
        //}

        public class stf02h_local
        {
            public string BRG { get; set; }
            public string BRG_MP { get; set; }
            public int IDMARKET { get; set; }
        }
        public class tempBrg_local
        {
            //public string BRG { get; set; }
            public string BRG_MP { get; set; }
            public int IDMARKET { get; set; }

        }

        //        public async Task<BindingBase> GetItemDetail(ShopeeAPIData iden, long item_id, List<tempBrg_local> tempBrg_local, List<stf02h_local> stf02h_local, int IdMarket)
        //        {
        //            int MOPartnerID = 841371;
        //            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
        //            string urll = "https://partner.shopeemobile.com/api/v1/item/get";
        //            if (!string.IsNullOrEmpty(iden.token))
        //            {
        //                MOPartnerID = MOPartnerIDV2;
        //                MOPartnerKey = MOPartnerKeyV2;
        //                urll = shopeeV2Url + "/api/v1/item/get";
        //            }
        //            //string ret = "";
        //            var ret = new BindingBase
        //            {
        //                status = 0,
        //                recordCount = 0,
        //                exception = 0,
        //                totalData = 0//add 18 Juli 2019, show total record
        //            };

        //            long seconds = CurrentTimeSecond();
        //            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

        //            //string urll = "https://partner.shopeemobile.com/api/v1/item/get";

        //            ShopeeGetItemDetailData HttpBody = new ShopeeGetItemDetailData
        //            {
        //                partner_id = MOPartnerID,
        //                shopid = Convert.ToInt32(iden.merchant_code),
        //                timestamp = seconds,
        //                item_id = item_id
        //            };

        //            string myData = JsonConvert.SerializeObject(HttpBody);

        //            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

        //            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
        //            myReq.Method = "POST";
        //            myReq.Headers.Add("Authorization", signature);
        //            myReq.Accept = "application/json";
        //            myReq.ContentType = "application/json";
        //            string responseFromServer = "";
        //            try
        //            {
        //                myReq.ContentLength = myData.Length;
        //                using (var dataStream = myReq.GetRequestStream())
        //                {
        //                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
        //                }
        //                using (WebResponse response = await myReq.GetResponseAsync())
        //                {
        //                    using (Stream stream = response.GetResponseStream())
        //                    {
        //                        StreamReader reader = new StreamReader(stream);
        //                        responseFromServer = reader.ReadToEnd();
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                ret.exception = 1;
        //                ret.message = ex.Message;
        //            }

        //            if (responseFromServer != null)
        //            {
        //                try
        //                {
        //                    var detailBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetItemDetailResult)) as ShopeeGetItemDetailResult;
        //                    if (detailBrg != null)
        //                    {
        //                        if (detailBrg.item != null)
        //                        {
        //                            //string IdMarket = ErasoftDbContext.ARF01.Where(c => c.Sort1_Cust.Equals(iden.merchant_code)).FirstOrDefault().RecNum.ToString();
        //                            string cust = ErasoftDbContext.ARF01.Where(c => c.Sort1_Cust == iden.merchant_code).FirstOrDefault().CUST.ToString();
        //                            string categoryCode = detailBrg.item.category_id.ToString();
        //                            var categoryInDB = MoDbContext.CategoryShopee.Where(p => p.CATEGORY_CODE == categoryCode).FirstOrDefault();
        //                            string categoryName = "";
        //                            if (categoryInDB != null)
        //                            {
        //                                categoryName = categoryInDB.CATEGORY_NAME;
        //                            }
        //                            ret.status = 1;

        //                            var sellerSku = "";
        //                            //if (string.IsNullOrEmpty(sellerSku))
        //                            //{
        //                            //    var nm = barang_id.Split(';');
        //                            //    if (nm.Length > 1)
        //                            //    {
        //                            //        sellerSku = nm[1];
        //                            //    }
        //                            //    else
        //                            //    {
        //                            //        sellerSku = barang_id;
        //                            //    }
        //                            //}
        //                            if (detailBrg.item.has_variation)
        //                            {
        //                                ret.totalData += detailBrg.item.variations.Count();//add 18 Juli 2019, show total record
        //                                //insert brg induk
        //                                //string brgMpInduk = Convert.ToString(detailBrg.item.item_id) + ";";
        //                                string brgMpInduk = Convert.ToString(detailBrg.item.item_id) + ";0";
        //                                //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.ToUpper().Equals(brgMpInduk.ToUpper()) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
        //                                //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(brgMpInduk) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
        //                                var tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMpInduk.ToUpper()).FirstOrDefault();
        //                                var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMpInduk.ToUpper()).FirstOrDefault();
        //                                //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.IDMARKET == IdMarket && (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMpInduk.ToUpper()).FirstOrDefault();
        //                                //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.IDMARKET == IdMarket && (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMpInduk.ToUpper()).FirstOrDefault();
        //                                if (tempbrginDB == null && brgInDB == null)
        //                                {
        //                                    //ret.recordCount++;
        //                                    var ret1 = await proses_Item_detail(detailBrg, categoryCode, categoryName, cust, IdMarket, brgMpInduk, detailBrg.item.name, detailBrg.item.variations[0].status, detailBrg.item.original_price, string.IsNullOrEmpty(detailBrg.item.item_sku) ? brgMpInduk : detailBrg.item.item_sku, 1, "", iden, true);
        //                                    ret.recordCount += ret1.status;
        //                                }
        //                                else if (brgInDB != null)
        //                                {
        //                                    brgMpInduk = brgInDB.BRG;
        //                                }
        //                                //string skuInduk = string.IsNullOrEmpty(detailBrg.item.item_sku) ? brgMpInduk : detailBrg.item.item_sku;
        //                                //end insert brg induk
        //                                var insert_1st_img = true;

        //                                foreach (var item in detailBrg.item.variations)
        //                                {
        //                                    sellerSku = item.variation_sku;
        //                                    //remark 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
        //                                    //if (string.IsNullOrEmpty(sellerSku))
        //                                    //{
        //                                    //    sellerSku = item.variation_id.ToString();
        //                                    //}
        //                                    //end remark 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
        //                                    string brgMp = Convert.ToString(detailBrg.item.item_id) + ";" + Convert.ToString(item.variation_id);
        //                                    //tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.ToUpper().Equals(brgMp.ToUpper()) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
        //                                    //brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(brgMp) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
        //                                    tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
        //                                    brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
        //                                    //tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.IDMARKET == IdMarket && (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
        //                                    //brgInDB = ErasoftDbContext.STF02H.Where(t => t.IDMARKET == IdMarket && (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
        //                                    if (tempbrginDB == null && brgInDB == null)
        //                                    {
        //                                        //ret.recordCount++;
        //                                        var ret2 = await proses_Item_detail(detailBrg, categoryCode, categoryName, cust, IdMarket, brgMp, detailBrg.item.name + " " + item.name, item.status, item.original_price, sellerSku, 2, brgMpInduk, iden, insert_1st_img);
        //                                        //var ret2 = await proses_Item_detail(detailBrg, categoryCode, categoryName, cust, IdMarket, brgMp, detailBrg.item.name + " " + item.name, item.status, item.original_price, sellerSku, 2, skuInduk, iden, insert_1st_img);
        //                                        ret.recordCount += ret2.status;
        //                                        insert_1st_img = false;//varian ke-2 tidak perlu ambil gambar
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                //change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
        //                                //sellerSku = string.IsNullOrEmpty(detailBrg.item.item_sku) ? detailBrg.item.item_id.ToString() : detailBrg.item.item_sku;
        //                                sellerSku = detailBrg.item.item_sku;
        //                                //end change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel

        //                                //ret.recordCount++;
        //                                var ret0 = await proses_Item_detail(detailBrg, categoryCode, categoryName, cust, IdMarket, Convert.ToString(detailBrg.item.item_id) + ";0", detailBrg.item.name, detailBrg.item.status, detailBrg.item.original_price, sellerSku, 0, "", iden, true);
        //                                ret.recordCount += ret0.status;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            ret.message = responseFromServer;
        //                        }

        //                    }
        //                }
        //                catch (Exception ex2)
        //                {
        //                    ret.exception = 1;
        //                    ret.message = ex2.Message;
        //                }
        //            }
        //            return ret;
        //        }
        //        protected async Task<BindingBase> proses_Item_detail(ShopeeGetItemDetailResult detailBrg, string categoryCode, string categoryName, string cust, int IdMarket, string barang_id, string barang_name, string barang_status, float barang_price, string sellerSku, int typeBrg, string kdBrgInduk, ShopeeAPIData iden, bool insert_1st_img)
        //        {
        //            // typeBrg : 0 = barang tanpa varian; 1 = barang induk; 2 = barang varian
        //            var ret = new BindingBase();
        //            string brand = "OEM";
        //            string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, NAMA3, BERAT, PANJANG, LEBAR, TINGGI, CUST, AVALUE_34, AVALUE_39, ";
        //            sSQL += "Deskripsi, IDMARKET, HJUAL, HJUAL_MP, DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, IMAGE4, IMAGE5, KODE_BRG_INDUK, TYPE, AVALUE_45,";
        //            sSQL += "ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
        //            sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20, ";
        //            sSQL += "ACODE_21, ANAME_21, AVALUE_21, ACODE_22, ANAME_22, AVALUE_22, ACODE_23, ANAME_23, AVALUE_23, ACODE_24, ANAME_24, AVALUE_24, ACODE_25, ANAME_25, AVALUE_25, ACODE_26, ANAME_26, AVALUE_26, ACODE_27, ANAME_27, AVALUE_27, ACODE_28, ANAME_28, AVALUE_28, ACODE_29, ANAME_29, AVALUE_29, ACODE_30, ANAME_30, AVALUE_30) VALUES ";

        //            string namaBrg = barang_name.Replace('\'', '`');
        //            string nama, nama2, nama3, urlImage, urlImage2, urlImage3, urlImage4, urlImage5;
        //            urlImage = "";
        //            urlImage2 = "";
        //            urlImage3 = "";
        //            urlImage4 = "";
        //            urlImage5 = "";
        //            sellerSku = sellerSku.ToString().Replace("\'", "\'\'");

        //            //change by calvin 16 september 2019
        //            //if (namaBrg.Length > 30)
        //            //{
        //            //    nama = namaBrg.Substring(0, 30);
        //            //    //change by calvin 15 januari 2019
        //            //    //if (namaBrg.Length > 60)
        //            //    //{
        //            //    //    nama2 = namaBrg.Substring(30, 30);
        //            //    //    nama3 = (namaBrg.Length > 90) ? namaBrg.Substring(60, 30) : namaBrg.Substring(60);
        //            //    //}
        //            //    if (namaBrg.Length > 285)
        //            //    {
        //            //        nama2 = namaBrg.Substring(30, 255);
        //            //        nama3 = "";
        //            //    }
        //            //    //end change by calvin 15 januari 2019
        //            //    else
        //            //    {
        //            //        nama2 = namaBrg.Substring(30);
        //            //        nama3 = "";
        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    nama = namaBrg;
        //            //    nama2 = "";
        //            //    nama3 = "";
        //            //}
        //            var splitItemName = new StokControllerJob().SplitItemName(namaBrg);
        //            nama = splitItemName[0];
        //            nama2 = splitItemName[1];
        //            nama3 = "";
        //            //end change by calvin 16 september 2019
        //            string urlBrg = "https://shopee.co.id/product/" + detailBrg.item.shopid + "/" + detailBrg.item.item_id;
        //            if (detailBrg.item.images.Count() > 0)
        //            {
        //                if (insert_1st_img)
        //                {
        //                    urlImage = detailBrg.item.images[0];
        //                    //change 21/8/2019, barang varian ambil 1 gambar saja
        //                    if (typeBrg != 2)
        //                    //if (typeBrg == 0)
        //                    {
        //                        if (detailBrg.item.images.Count() >= 2)
        //                        {
        //                            urlImage2 = detailBrg.item.images[1];
        //                            if (detailBrg.item.images.Count() >= 3)
        //                            {
        //                                urlImage3 = detailBrg.item.images[2];
        //                                //add 16/9/19, 5 gambar
        //                                if (detailBrg.item.images.Count() >= 4)
        //                                {
        //                                    urlImage4 = detailBrg.item.images[3];
        //                                    if (detailBrg.item.images.Count() >= 5)
        //                                    {
        //                                        urlImage5 = detailBrg.item.images[4];
        //                                    }
        //                                }
        //                                //end add 16/9/19, 5 gambar
        //                            }
        //                        }
        //                    }
        //                    //end change 21/8/2019, barang varian ambil 1 gambar saja
        //                }
        //            }

        //            //add 21 mei 2021, logistic shopee
        //            var listLogistic = "";
        //            if (detailBrg.item.logistics != null)
        //            {
        //                if (detailBrg.item.logistics.Length > 0)
        //                {
        //                    foreach (var log in detailBrg.item.logistics)
        //                    {
        //                        if (log.enabled)
        //                        {
        //                            if (!string.IsNullOrEmpty(listLogistic))
        //                            {
        //                                listLogistic += ",";
        //                            }
        //                            listLogistic += log.logistic_id.ToString();
        //                        }
        //                    }
        //                }
        //            }
        //            //end add 21 mei 2021, logistic shopee

        //            sSQL += "('" + barang_id + "' , '" + sellerSku + "' , '" + nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' , '" + nama3.Replace('\'', '`') + "' ,";
        //            sSQL += detailBrg.item.weight * 1000 + "," + detailBrg.item.package_length + "," + detailBrg.item.package_width + "," + detailBrg.item.package_height + ", '";
        //            //change 21 okt 2020 tambah url shopee
        //            //change by nurul 22/1/2020, tambah paragraf
        //            //sSQL += cust + "' , '" + detailBrg.item.description.Replace('\'', '`') + "' , " + IdMarket + " , " + barang_price + " , " + barang_price;
        //            //sSQL += cust + "' , '" + "<p>" + detailBrg.item.description.Replace('\'', '`').Replace("\n", "</p><p>") + "</p>" + "' , " + IdMarket + " , " + barang_price + " , " + barang_price;
        //            sSQL += cust + "' , '" + urlBrg + "' , '" + listLogistic + "' , '" + "<p>" + detailBrg.item.description.Replace('\'', '`').Replace("\n", "</p><p>") + "</p>" + "' , " + IdMarket + " , " + barang_price + " , " + barang_price;
        //            //end change by nurul 22/1/2020, tambah paragraf
        //            //end change 21 okt 2020 tambah url shopee
        //            sSQL += " , " + (barang_status.Contains("NORMAL") ? "1" : "0") + " , '" + categoryCode + "' , '" + categoryName + "' , '" + "REPLACE_MEREK" + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "' , '" + urlImage4 + "' , '" + urlImage5 + "'";
        //            //add kode brg induk dan type brg
        //            sSQL += ", '" + (typeBrg == 2 ? kdBrgInduk : "") + "' , '" + (typeBrg == 1 ? "4" : "3") + "'";
        //            //end add kode brg induk dan type brg
        //            sSQL += ",'" + (namaBrg.Length > 250 ? namaBrg.Substring(0, 250) : namaBrg) + "'"; //request by Calvin, 19 maret 2019

        //            //var attributeShopee = MoDbContext.AttributeShopee.Where(a => a.CATEGORY_CODE == categoryCode).FirstOrDefault();
        //            var GetAttributeShopee = await GetAttributeToList(iden, categoryCode, categoryName);
        //            var attributeShopee = GetAttributeShopee.attributes.FirstOrDefault();

        //            #region set attribute
        //            if (attributeShopee != null)
        //            {
        //                string attrVal = "";
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_1))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_1.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`').Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_1 + "' , '" + attributeShopee.ANAME_1.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_2))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_2.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_2 + "' , '" + attributeShopee.ANAME_2.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }

        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_3))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_3.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_3 + "' , '" + attributeShopee.ANAME_3.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_4))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_4.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_4 + "' , '" + attributeShopee.ANAME_4.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }

        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_5))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_5.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_5 + "' , '" + attributeShopee.ANAME_5.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_6))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_6.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_6 + "' , '" + attributeShopee.ANAME_6.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_7))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_7.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_7 + "' , '" + attributeShopee.ANAME_7.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_8))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_8.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_8 + "' , '" + attributeShopee.ANAME_8.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_9))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_9.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_9 + "' , '" + attributeShopee.ANAME_9.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_10))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_10.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_10 + "' , '" + attributeShopee.ANAME_10.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_11))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_11.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_11 + "' , '" + attributeShopee.ANAME_11.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_12))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_12.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_12 + "' , '" + attributeShopee.ANAME_12.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_13))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_13.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_13 + "' , '" + attributeShopee.ANAME_13.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_14))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_14.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_14 + "' , '" + attributeShopee.ANAME_14.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_15))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_15.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_15 + "' , '" + attributeShopee.ANAME_15.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_16))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_16.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_16 + "' , '" + attributeShopee.ANAME_16.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_17))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_17.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_17 + "' , '" + attributeShopee.ANAME_17.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_18))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_18.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_18 + "' , '" + attributeShopee.ANAME_18.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_19))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_19.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_19 + "' , '" + attributeShopee.ANAME_19.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_20))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_20.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_20 + "' , '" + attributeShopee.ANAME_20.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_21))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_21.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_21 + "' , '" + attributeShopee.ANAME_21.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_22))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_22.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_22 + "' , '" + attributeShopee.ANAME_22.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_23))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_23.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_23 + "' , '" + attributeShopee.ANAME_23.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_24))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_24.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_24 + "' , '" + attributeShopee.ANAME_24.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_25))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_25.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_25 + "' , '" + attributeShopee.ANAME_25.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_26))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_26.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_26 + "' , '" + attributeShopee.ANAME_26.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_27))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_27.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_27 + "' , '" + attributeShopee.ANAME_27.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_28))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_28.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_28 + "' , '" + attributeShopee.ANAME_28.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_29))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_29.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_29 + "' , '" + attributeShopee.ANAME_29.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', ''";
        //                }
        //                if (!string.IsNullOrEmpty(attributeShopee.ACODE_30))
        //                {
        //                    foreach (var property in detailBrg.item.attributes)
        //                    {
        //                        string tempCode = property.attribute_id.ToString();
        //                        if (attributeShopee.ACODE_30.ToUpper().Equals(tempCode.ToString()))
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                                attrVal += ";";
        //                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
        //                            if (property.attribute_name.ToUpper() == "MEREK")
        //                            {
        //                                brand = property.attribute_value;
        //                            }
        //                        }
        //                    }
        //                    sSQL += ", '" + attributeShopee.ACODE_30 + "' , '" + attributeShopee.ANAME_30.Replace("\'", "\'\'") + "' , '" + attrVal + "')";
        //                    attrVal = "";
        //                }
        //                else
        //                {
        //                    sSQL += ", '', '', '')";
        //                }
        //            }
        //            else
        //            {
        //                //attribute not found
        //                sSQL += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
        //                sSQL += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
        //                sSQL += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '')";
        //            }

        //            #endregion
        //            sSQL = sSQL.Replace("REPLACE_MEREK", brand.Replace('\'', '`'));
        //            var retRec = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
        //            ret.status = retRec;
        //            return ret;
        //        }

        //        public async Task<BindingBase> GetItemDetail_V2(ShopeeAPIData iden, long item_id, List<tempBrg_local> tempBrg_local, List<stf02h_local> stf02h_local, int IdMarket)
        //        {
        //            var ret = new BindingBase
        //            {
        //                status = 0,
        //                recordCount = 0,
        //                exception = 0,
        //                totalData = 0//add 18 Juli 2019, show total record
        //            };

        //            iden = await RefreshTokenShopee_V2(iden, false);
        //            long seconds = CurrentTimeSecond();
        //            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

        //            int MOPartnerID = MOPartnerIDV2;
        //            string MOPartnerKey = MOPartnerKeyV2;
        //            string urll = shopeeV2Url;
        //            string path = "/api/v2/product/get_item_base_info";

        //            var baseString = MOPartnerID + path + seconds + iden.token + iden.merchant_code;
        //            var sign = CreateSignAuthenShop_V2(baseString, MOPartnerKey);


        //            string param = "?partner_id=" + MOPartnerID + "&timestamp=" + seconds + "&sign=" + sign
        //                + "&access_token=" + iden.token + "&shop_id=" + iden.merchant_code + "&item_id_list=" + item_id;

        //            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + path + param);
        //            myReq.Method = "GET";
        //            myReq.Accept = "application/json";
        //            myReq.ContentType = "application/json";
        //            string responseFromServer = "";
        //            try
        //            {
        //                using (WebResponse response = await myReq.GetResponseAsync())
        //                {
        //                    using (Stream stream = response.GetResponseStream())
        //                    {
        //                        StreamReader reader = new StreamReader(stream);
        //                        responseFromServer = reader.ReadToEnd();
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                ret.exception = 1;
        //                ret.message = ex.Message;
        //            }

        //            if (responseFromServer != null)
        //            {
        //                try
        //                {
        //                    var detailBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetItemDetailResult_V2)) as ShopeeGetItemDetailResult_V2;
        //                    if (detailBrg != null)
        //                    {
        //                        if(detailBrg.response != null)
        //                        {
        //                            foreach(var itemRes in detailBrg.response.item_list)
        //                            {
        //                                string cust = ErasoftDbContext.ARF01.Where(c => c.Sort1_Cust == iden.merchant_code).FirstOrDefault().CUST.ToString();
        //                                string categoryCode = itemRes.category_id.ToString();
        //                                var categoryInDB = MoDbContext.CategoryShopeeV2.Where(p => p.CATEGORY_CODE == categoryCode).FirstOrDefault();
        //                                string categoryName = "";
        //                                if (categoryInDB != null)
        //                                {
        //                                    categoryName = categoryInDB.CATEGORY_NAME;
        //                                }
        //                                ret.status = 1;

        //                                var sellerSku = "";
        //                                if (itemRes.has_model)
        //                                {
        //                                    var itemVariation = await CekVariationShopee_V2(iden, item_id);
        //                                    float hargaBrg = 0;
        //                                    if(itemVariation.response != null)
        //                                    {
        //                                        ret.totalData += itemVariation.response.model.Length;
        //                                        hargaBrg = itemVariation.response.model[0].price_info[0].original_price;
        //                                    }
        //                                    //ret.totalData += detailBrg.item.variations.Count();//add 18 Juli 2019, show total record
        //                                    //insert brg induk
        //                                    string brgMpInduk = Convert.ToString(itemRes.item_id) + ";0";
        //                                    //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.ToUpper().Equals(brgMpInduk.ToUpper()) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
        //                                    //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(brgMpInduk) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
        //                                    var tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMpInduk.ToUpper()).FirstOrDefault();
        //                                    var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMpInduk.ToUpper()).FirstOrDefault();
        //                                    //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.IDMARKET == IdMarket && (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMpInduk.ToUpper()).FirstOrDefault();
        //                                    //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.IDMARKET == IdMarket && (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMpInduk.ToUpper()).FirstOrDefault();
        //                                    if (tempbrginDB == null && brgInDB == null)
        //                                    {
        //                                        //ret.recordCount++;
        //                                        var ret1 = await proses_Item_detail_V2(itemRes, categoryCode, categoryName, cust, IdMarket, brgMpInduk, itemRes.item_name, itemRes.item_status, hargaBrg, string.IsNullOrEmpty(itemRes.item_sku) ? brgMpInduk : itemRes.item_sku, 1, "", iden, true);
        //                                        ret.recordCount += ret1.status;
        //                                    }
        //                                    else if (brgInDB != null)
        //                                    {
        //                                        brgMpInduk = brgInDB.BRG;
        //                                    }
        //                                    //string skuInduk = string.IsNullOrEmpty(detailBrg.item.item_sku) ? brgMpInduk : detailBrg.item.item_sku;
        //                                    //end insert brg induk
        //                                    var insert_1st_img = true;

        //                                    foreach (var item in itemVariation.response.model)
        //                                    {
        //                                        sellerSku = item.model_sku ?? "";
        //                                        string brgMp = Convert.ToString(item_id) + ";" + Convert.ToString(item.model_id);

        //                                        tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
        //                                        brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();

        //                                        if (tempbrginDB == null && brgInDB == null)
        //                                        {
        //                                            var variationName = "";
        //                                            for(int i =0; i < item.tier_index.Length; i++)
        //                                            {
        //                                                var tier_var = itemVariation.response.tier_variation[i];
        //                                                variationName += tier_var.option_list[item.tier_index[i]].option + " ";
        //                                            }
        //                                            //ret.recordCount++;
        //                                            var ret2 = await proses_Item_detail_V2(itemRes, categoryCode, categoryName, cust, IdMarket, brgMp, itemRes.item_name + " " + variationName, itemRes.item_status, item.price_info[0].original_price, sellerSku, 2, brgMpInduk, iden, insert_1st_img);
        //                                            ret.recordCount += ret2.status;
        //                                            insert_1st_img = false;//varian ke-2 tidak perlu ambil gambar
        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    sellerSku = itemRes.item_sku ?? "";
        //                                    string brgMp = Convert.ToString(itemRes.item_id) + ";0";
        //                                    float hargaBrg = 0;
        //                                    var tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
        //                                    var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
        //                                    if (tempbrginDB == null && brgInDB == null)
        //                                    {
        //                                        if (itemRes.price_info != null)
        //                                        {
        //                                            hargaBrg = itemRes.price_info[0].original_price;
        //                                        }
        //                                        var ret0 = await proses_Item_detail_V2(itemRes, categoryCode, categoryName, cust, IdMarket, Convert.ToString(itemRes.item_id) + ";0", itemRes.item_name, itemRes.item_status, hargaBrg, sellerSku, 0, "", iden, true);
        //                                        ret.recordCount += ret0.status;
        //                                    }
        //                                }

        //                            }
        //                        }

        //                    }
        //                }
        //                catch (Exception ex2)
        //                {
        //                    ret.exception = 1;
        //                    ret.message = ex2.Message;
        //                }
        //            }
        //            return ret;
        //        }


        //        public async Task<ShopeeControllerJob.GetVariationResult_V2> CekVariationShopee_V2(ShopeeAPIData iden, long item_id)
        //        {
        //            var ret = new ShopeeControllerJob.GetVariationResult_V2();

        //            long seconds = CurrentTimeSecond();
        //            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

        //            int MOPartnerID = MOPartnerIDV2;
        //            string MOPartnerKey = MOPartnerKeyV2;
        //            string urll = shopeeV2Url;
        //            string path = "/api/v2/product/get_model_list";

        //            var baseString = MOPartnerID + path + seconds + iden.token + iden.merchant_code;
        //            var sign = CreateSignAuthenShop_V2(baseString, MOPartnerKey);

        //            string param = "?partner_id=" + MOPartnerID + "&timestamp=" + seconds + "&sign=" + sign
        //                + "&access_token=" + iden.token + "&shop_id=" + iden.merchant_code + "&item_id=" + item_id;

        //            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + path + param);
        //            myReq.Method = "GET";
        //            myReq.Accept = "application/json";
        //            myReq.ContentType = "application/json";
        //            string responseFromServer = "";
        //            try
        //            {
        //                using (WebResponse response = await myReq.GetResponseAsync())
        //                {
        //                    using (Stream stream = response.GetResponseStream())
        //                    {
        //                        StreamReader reader = new StreamReader(stream);
        //                        responseFromServer = reader.ReadToEnd();
        //                    }
        //                }
        //            }
        //            catch (WebException e)
        //            {
        //                string err = "";
        //                if (e.Status == WebExceptionStatus.ProtocolError)
        //                {
        //                    WebResponse resp = e.Response;
        //                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
        //                    {
        //                        err = sr.ReadToEnd();
        //                    }
        //                }
        //            }


        //            if (responseFromServer != "")
        //            {
        //                ret = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeControllerJob.GetVariationResult_V2)) as ShopeeControllerJob.GetVariationResult_V2;

        //            }

        //            return ret;
        //        }

        //        protected async Task<BindingBase> proses_Item_detail_V2(Item_List detailBrg, string categoryCode, string categoryName, string cust, int IdMarket, string barang_id, string barang_name, string barang_status, float barang_price, string sellerSku, int typeBrg, string kdBrgInduk, ShopeeAPIData iden, bool insert_1st_img)
        //        {
        //            // typeBrg : 0 = barang tanpa varian; 1 = barang induk; 2 = barang varian
        //            var ret = new BindingBase();
        //            string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, NAMA3, BERAT, PANJANG, LEBAR, TINGGI, CUST, AVALUE_34, AVALUE_39, AVALUE_38, ANAME_38, ";
        //            sSQL += "Deskripsi, IDMARKET, HJUAL, HJUAL_MP, DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, IMAGE4, IMAGE5, KODE_BRG_INDUK, TYPE, AVALUE_45,";
        //            sSQL += "ACODE_1, ANAME_1, AVALUE_1, AUNIT_1, ACODE_2, ANAME_2, AVALUE_2, AUNIT_2, ACODE_3, ANAME_3, AVALUE_3, AUNIT_3, ACODE_4, ANAME_4, AVALUE_4, AUNIT_4, ACODE_5, ANAME_5, AVALUE_5, AUNIT_5, ACODE_6, ANAME_6, AVALUE_6, AUNIT_6, ACODE_7, ANAME_7, AVALUE_7, AUNIT_7, ACODE_8, ANAME_8, AVALUE_8, AUNIT_8, ACODE_9, ANAME_9, AVALUE_9, AUNIT_9, ACODE_10, ANAME_10, AVALUE_10, AUNIT_10, ";
        //            sSQL += "ACODE_11, ANAME_11, AVALUE_11, AUNIT_11, ACODE_12, ANAME_12, AVALUE_12, AUNIT_12, ACODE_13, ANAME_13, AVALUE_13, AUNIT_13, ACODE_14, ANAME_14, AVALUE_14, AUNIT_14, ACODE_15, ANAME_15, AVALUE_15, AUNIT_15, ACODE_16, ANAME_16, AVALUE_16, AUNIT_16, ACODE_17, ANAME_17, AVALUE_17, AUNIT_17, ACODE_18, ANAME_18, AVALUE_18, AUNIT_18, ACODE_19, ANAME_19, AVALUE_19, AUNIT_19, ACODE_20, ANAME_20, AVALUE_20, AUNIT_20, ";
        //            sSQL += "ACODE_21, ANAME_21, AVALUE_21, AUNIT_21, ACODE_22, ANAME_22, AVALUE_22, AUNIT_22, ACODE_23, ANAME_23, AVALUE_23, AUNIT_23, ACODE_24, ANAME_24, AVALUE_24, AUNIT_24, ACODE_25, ANAME_25, AVALUE_25, AUNIT_25, ACODE_26, ANAME_26, AVALUE_26, AUNIT_26, ACODE_27, ANAME_27, AVALUE_27, AUNIT_27, ACODE_28, ANAME_28, AVALUE_28, AUNIT_28, ACODE_29, ANAME_29, AVALUE_29, AUNIT_29, ACODE_30, ANAME_30, AVALUE_30, AUNIT_30) VALUES ";

        //            string namaBrg = barang_name.Replace('\'', '`');
        //            string nama, nama2, nama3, urlImage, urlImage2, urlImage3, urlImage4, urlImage5;
        //            urlImage = "";
        //            urlImage2 = "";
        //            urlImage3 = "";
        //            urlImage4 = "";
        //            urlImage5 = "";
        //            sellerSku = sellerSku.ToString().Replace("\'", "\'\'");

        //            var splitItemName = new StokControllerJob().SplitItemName(namaBrg);
        //            nama = splitItemName[0];
        //            nama2 = splitItemName[1];
        //            nama3 = "";

        //            string urlBrg = "https://shopee.co.id/product/" + iden.merchant_code + "/" + detailBrg.item_id;
        //            if (detailBrg.image.image_url_list.Length > 0)
        //            {
        //                if (insert_1st_img)
        //                {
        //                    urlImage = detailBrg.image.image_url_list[0];
        //                    //change 21/8/2019, barang varian ambil 1 gambar saja
        //                    if (typeBrg != 2)
        //                    //if (typeBrg == 0)
        //                    {
        //                        if (detailBrg.image.image_url_list.Length >= 2)
        //                        {
        //                            urlImage2 = detailBrg.image.image_url_list[1];
        //                            if (detailBrg.image.image_url_list.Length >= 3)
        //                            {
        //                                urlImage3 = detailBrg.image.image_url_list[2];
        //                                //add 16/9/19, 5 gambar
        //                                if (detailBrg.image.image_url_list.Length >= 4)
        //                                {
        //                                    urlImage4 = detailBrg.image.image_url_list[3];
        //                                    if (detailBrg.image.image_url_list.Length >= 5)
        //                                    {
        //                                        urlImage5 = detailBrg.image.image_url_list[4];
        //                                    }
        //                                }
        //                                //end add 16/9/19, 5 gambar
        //                            }
        //                        }
        //                    }
        //                    //end change 21/8/2019, barang varian ambil 1 gambar saja
        //                }
        //            }

        //            //add 21 mei 2021, logistic shopee
        //            var listLogistic = "";
        //            if (detailBrg.logistic_info != null)
        //            {
        //                if (detailBrg.logistic_info.Length > 0)
        //                {
        //                    foreach (var log in detailBrg.logistic_info)
        //                    {
        //                        if (log.enabled)
        //                        {
        //                            if (!string.IsNullOrEmpty(listLogistic))
        //                            {
        //                                listLogistic += ",";
        //                            }
        //                            listLogistic += log.logistic_id.ToString();
        //                        }
        //                    }
        //                }
        //            }
        //            //end add 21 mei 2021, logistic shopee

        //            var brandId = "";
        //            var brandName = "NO BRAND";
        //            if(detailBrg.brand != null)
        //            {
        //                brandId = detailBrg.brand.brand_id.ToString();
        //                brandName = detailBrg.brand.original_brand_name;
        //            }
        //            sSQL += "('" + barang_id + "' , '" + sellerSku + "' , '" + nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' , '" + nama3.Replace('\'', '`') + "' ,";
        //            sSQL += Convert.ToDouble(detailBrg.weight) * 1000 + "," + detailBrg.dimension.package_length + "," + detailBrg.dimension.package_width + "," + detailBrg.dimension.package_height + ", '";
        //            sSQL += cust + "' , '" + urlBrg + "' , '" + listLogistic + "' , '" + brandId + "' , '" + brandName + "' , '" + "<p>" + detailBrg.description.Replace('\'', '`').Replace("\n", "</p><p>") + "</p>" + "' , " + IdMarket + " , " + barang_price + " , " + barang_price;
        //            sSQL += " , " + (barang_status.Contains("NORMAL") ? "1" : "0") + " , '" + categoryCode + "' , '" + categoryName + "' , '" + "REPLACE_MEREK" + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "' , '" + urlImage4 + "' , '" + urlImage5 + "'";
        //            sSQL += ", '" + (typeBrg == 2 ? kdBrgInduk : "") + "' , '" + (typeBrg == 1 ? "4" : "3") + "'";
        //            sSQL += ",'" + (namaBrg.Length > 250 ? namaBrg.Substring(0, 250) : namaBrg) + "'"; 

        //            #region set attribute
        //            int indexAttr = 1;
        //            if(detailBrg.attribute_list != null)
        //            {

        //                var listAttrShopee = await GetAttribute_V2(iden, categoryCode);

        //                foreach (var attr in detailBrg.attribute_list)
        //                {
        //                    var attrVal = "";
        //                    foreach (var attrOpt in attr.attribute_value_list)
        //                    {
        //                        if (attrOpt.value_id == 0)
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                            {
        //                                attrVal += ",";
        //                            }
        //                            if (listAttrShopee.response != null)
        //                            {
        //                                if (listAttrShopee.response.attribute_list != null)
        //                                {
        //                                    var currAttr = listAttrShopee.response.attribute_list.Where(m => m.attribute_id == attr.attribute_id).FirstOrDefault();
        //                                    if (currAttr != null)
        //                                    {
        //                                        if (currAttr.input_validation_type.ToUpper().Contains("DATE_TYPE"))
        //                                        {
        //                                            long n;
        //                                            bool isNumeric = long.TryParse(attrOpt.original_value_name.Trim(), out n);
        //                                            if (isNumeric)
        //                                            {
        //                                                attrOpt.original_value_name = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(n)).UtcDateTime.AddHours(7).ToString("dd/MM/yyyy");
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                            attrVal += attrOpt.original_value_name.Replace("\'", "`");
        //                        }
        //                        else
        //                        {
        //                            if (!string.IsNullOrEmpty(attrVal))
        //                            {
        //                                attrVal += ",";
        //                            }
        //                            attrVal += attrOpt.value_id;
        //                        }
        //                    }
        //                    sSQL += ", '" + attr.attribute_id + "' , '" + attr.original_attribute_name.Replace("\'", "\'\'") + "' , '" + attrVal + "' , '" + attr.attribute_value_list[0].value_unit + "'";
        //                    if(indexAttr == 30)
        //                    {
        //                        sSQL += ")";
        //                        break;
        //                    }
        //                    indexAttr++;
        //                }
        //            }
        //            while(indexAttr <= 30)
        //            {
        //                sSQL += ", '' , '' , '' , ''";
        //                if (indexAttr == 30)
        //                {
        //                    sSQL += ")";
        //                    break;
        //                }
        //                indexAttr++;
        //            }
        //            #endregion
        //            sSQL = sSQL.Replace("REPLACE_MEREK", brandName.Replace('\'', '`'));
        //            var retRec = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
        //            ret.status = retRec;
        //            return ret;
        //        }
        //        public async Task<ShopeeChatController.ShopeeGetAttributeResult_V2> GetAttribute_V2(ShopeeAPIData dataAPI, string category)
        //        {
        //            dataAPI = await RefreshTokenShopee_V2(dataAPI, false);
        //            int MOPartnerID = MOPartnerIDV2;
        //            string MOPartnerKey = MOPartnerKeyV2;
        //            var ret = new ShopeeChatController.ShopeeGetAttributeResult_V2();

        //            long seconds = CurrentTimeSecond();
        //            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

        //            string urll = shopeeV2Url;
        //            string path = "/api/v2/product/get_attributes";

        //            var baseString = MOPartnerID + path + seconds + dataAPI.token + dataAPI.merchant_code;
        //            var sign = CreateSignAuthenShop_V2(baseString, MOPartnerKey);

        //            string param = "?partner_id=" + MOPartnerID + "&timestamp=" + seconds + "&access_token=" + dataAPI.token
        //                + "&shop_id=" + dataAPI.merchant_code + "&sign=" + sign + "&language=id&category_id=" + category;


        //            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + path + param);
        //            myReq.Method = "GET";
        //            myReq.Accept = "application/json";
        //            myReq.ContentType = "application/json";
        //            string responseFromServer = "";
        //            try
        //            {
        //                using (WebResponse response = await myReq.GetResponseAsync())
        //                {
        //                    using (Stream stream = response.GetResponseStream())
        //                    {
        //                        StreamReader reader = new StreamReader(stream);
        //                        responseFromServer = reader.ReadToEnd();
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //            }

        //            if (!string.IsNullOrEmpty(responseFromServer))
        //            {
        //                try
        //                {
        //                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeChatController.ShopeeGetAttributeResult_V2)) as ShopeeChatController.ShopeeGetAttributeResult_V2;

        //                    if (result.response.attribute_list != null)
        //                    {
        //                        ret = result;
        //                    }

        //                }
        //                catch (Exception ex2)
        //                {

        //                }
        //            }

        //            return ret;
        //        }

        //        public async Task<string> GetCategory(ShopeeAPIData iden)
        //        {
        //            int MOPartnerID = 841371;
        //            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
        //            string ret = "";

        //            long seconds = CurrentTimeSecond();
        //            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

        //            //ganti
        //            string urll = "https://partner.shopeemobile.com/api/v1/item/categories/get";

        //            //ganti
        //            ShopeeGetCategoryData HttpBody = new ShopeeGetCategoryData
        //            {
        //                partner_id = MOPartnerID,
        //                shopid = Convert.ToInt32(iden.merchant_code),
        //                timestamp = seconds,
        //                language = "id"
        //            };

        //            string myData = JsonConvert.SerializeObject(HttpBody);

        //            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

        //            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
        //            myReq.Method = "POST";
        //            myReq.Headers.Add("Authorization", signature);
        //            myReq.Accept = "application/json";
        //            myReq.ContentType = "application/json";
        //            string responseFromServer = "";
        //            try
        //            {
        //                myReq.ContentLength = myData.Length;
        //                using (var dataStream = myReq.GetRequestStream())
        //                {
        //                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
        //                }
        //                using (WebResponse response = await myReq.GetResponseAsync())
        //                {
        //                    using (Stream stream = response.GetResponseStream())
        //                    {
        //                        StreamReader reader = new StreamReader(stream);
        //                        responseFromServer = reader.ReadToEnd();
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //            }

        //            if (responseFromServer != null)
        //            {
        //                try
        //                {
        //                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetCategoryResult)) as ShopeeGetCategoryResult;
        //#if AWS
        //                    string con = "Data Source=localhost;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
        //#elif Debug_AWS
        //                    string con = "Data Source=13.250.232.74;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
        //#else
        //                    string con = "Data Source=13.251.222.53;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
        //#endif

        //                    using (SqlConnection oConnection = new SqlConnection(con))
        //                    {
        //                        oConnection.Open();
        //                        //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
        //                        //{
        //                        using (SqlCommand oCommand = oConnection.CreateCommand())
        //                        {
        //                            //oCommand.CommandText = "DELETE FROM [CATEGORY_BLIBLI] WHERE ARF01_SORT1_CUST='" + data.merchant_code + "'";
        //                            //oCommand.ExecuteNonQuery();
        //                            //oCommand.Transaction = oTransaction;
        //                            oCommand.CommandType = CommandType.Text;
        //                            oCommand.CommandText = "INSERT INTO [CATEGORY_SHOPEE] ([CATEGORY_CODE], [CATEGORY_NAME], [PARENT_CODE], [IS_LAST_NODE], [MASTER_CATEGORY_CODE]) VALUES (@CATEGORY_CODE, @CATEGORY_NAME, @PARENT_CODE, @IS_LAST_NODE, @MASTER_CATEGORY_CODE)";
        //                            //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
        //                            oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
        //                            oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));
        //                            oCommand.Parameters.Add(new SqlParameter("@PARENT_CODE", SqlDbType.NVarChar, 50));
        //                            oCommand.Parameters.Add(new SqlParameter("@IS_LAST_NODE", SqlDbType.NVarChar, 1));
        //                            oCommand.Parameters.Add(new SqlParameter("@MASTER_CATEGORY_CODE", SqlDbType.NVarChar, 50));

        //                            try
        //                            {
        //                                foreach (var item in result.categories.Where(P => P.parent_id == 0)) //foreach parent level top
        //                                {
        //                                    oCommand.Parameters[0].Value = item.category_id;
        //                                    oCommand.Parameters[1].Value = item.category_name;
        //                                    oCommand.Parameters[2].Value = "";
        //                                    oCommand.Parameters[3].Value = item.has_children ? "0" : "1";
        //                                    oCommand.Parameters[4].Value = "";
        //                                    if (oCommand.ExecuteNonQuery() > 0)
        //                                    {
        //                                        if (item.has_children)
        //                                        {
        //                                            RecursiveInsertCategory(oCommand, result.categories, item.category_id, item.category_id);
        //                                        }
        //                                    }
        //                                    else
        //                                    {

        //                                    }
        //                                }
        //                                //oTransaction.Commit();
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                //oTransaction.Rollback();
        //                            }
        //                        }
        //                    }
        //                }
        //                catch (Exception ex2)
        //                {

        //                }
        //            }
        //            return ret;
        //        }
        //        protected void RecursiveInsertCategory(SqlCommand oCommand, ShopeeGetCategoryCategory[] categories, long parent, long master_category_code)
        //        {
        //            foreach (var child in categories.Where(p => p.parent_id == parent))
        //            {

        //                var cekChildren = categories.Where(p => p.parent_id == child.category_id).FirstOrDefault();
        //                oCommand.Parameters[0].Value = child.category_id;
        //                oCommand.Parameters[1].Value = child.category_name;
        //                oCommand.Parameters[2].Value = parent;
        //                //change by calvin 11 maret 2019, API returnnya item.has children false, tetapi nyatanya ada childrennya, kasus 15949
        //                //oCommand.Parameters[3].Value = child.has_children ? "0" : "1";
        //                oCommand.Parameters[3].Value = cekChildren == null ? "1" : "0";
        //                //end change by calvin 11 maret 2019
        //                oCommand.Parameters[4].Value = master_category_code;

        //                if (oCommand.ExecuteNonQuery() > 0)
        //                {
        //                    if (cekChildren != null)
        //                    {
        //                        RecursiveInsertCategory(oCommand, categories, child.category_id, master_category_code);
        //                    }
        //                }
        //                else
        //                {

        //                }
        //            }
        //        }

        //        public async Task<ATTRIBUTE_SHOPEE_AND_OPT> GetAttributeToList(ShopeeAPIData iden, string category_code, string category_name)
        //        {
        //            int MOPartnerID = 841371;
        //            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
        //            string urll = "https://partner.shopeemobile.com/api/v1/item/attributes/get";
        //            if (!string.IsNullOrEmpty(iden.token))
        //            {
        //                MOPartnerID = MOPartnerIDV2;
        //                MOPartnerKey = MOPartnerKeyV2;
        //                urll = shopeeV2Url + "/api/v1/item/attributes/get";
        //            }
        //            ATTRIBUTE_SHOPEE_AND_OPT ret = new ATTRIBUTE_SHOPEE_AND_OPT();

        //            long seconds = CurrentTimeSecond();
        //            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

        //            //ganti
        //            //string urll = "https://partner.shopeemobile.com/api/v1/item/attributes/get";

        //            //ganti
        //            ShopeeGetAttributeData HttpBody = new ShopeeGetAttributeData
        //            {
        //                partner_id = MOPartnerID,
        //                shopid = Convert.ToInt32(iden.merchant_code),
        //                timestamp = seconds,
        //                language = "id",
        //                category_id = Convert.ToInt32(category_code)
        //            };

        //            string myData = JsonConvert.SerializeObject(HttpBody);

        //            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

        //            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
        //            myReq.Method = "POST";
        //            myReq.Headers.Add("Authorization", signature);
        //            myReq.Accept = "application/json";
        //            myReq.ContentType = "application/json";
        //            string responseFromServer = "";
        //            try
        //            {
        //                myReq.ContentLength = myData.Length;
        //                using (var dataStream = myReq.GetRequestStream())
        //                {
        //                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
        //                }
        //                using (WebResponse response = await myReq.GetResponseAsync())
        //                {
        //                    using (Stream stream = response.GetResponseStream())
        //                    {
        //                        StreamReader reader = new StreamReader(stream);
        //                        responseFromServer = reader.ReadToEnd();
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //            }

        //            if (responseFromServer != null)
        //            {
        //                try
        //                {
        //                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetAttributeResult)) as ShopeeGetAttributeResult;
        //#if AWS
        //                    string con = "Data Source=localhost;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
        //#elif Debug_AWS
        //                    string con = "Data Source=13.250.232.74;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
        //#else
        //                    string con = "Data Source=13.251.222.53;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
        //#endif
        //                    string a = "";
        //                    int i = 0;
        //                    ATTRIBUTE_SHOPEE returnData = new ATTRIBUTE_SHOPEE();
        //                    foreach (var attribs in result.attributes)
        //                    {
        //                        a = Convert.ToString(i + 1);
        //                        returnData.CATEGORY_CODE = category_code;
        //                        returnData.CATEGORY_NAME = category_name;

        //                        returnData["ACODE_" + a] = Convert.ToString(attribs.attribute_id);
        //                        returnData["ATYPE_" + a] = attribs.options.Count() > 0 ? "PREDEFINED_ATTRIBUTE" : "DESCRIPTIVE_ATTRIBUTE";
        //                        returnData["ANAME_" + a] = attribs.attribute_name;
        //                        returnData["AOPTIONS_" + a] = attribs.options.Count() > 0 ? "1" : "0";
        //                        returnData["AMANDATORY_" + a] = attribs.is_mandatory ? "1" : "0";

        //                        if (attribs.options.Count() > 0)
        //                        {
        //                            var optList = attribs.options.ToList();
        //                            var listOpt = optList.Select(x => new ATTRIBUTE_OPT_SHOPEE(attribs.attribute_id.ToString(), x)).ToList();
        //                            ret.attribute_opts.AddRange(listOpt);
        //                        }
        //                        i = i + 1;
        //                    }
        //                    ret.attributes.Add(returnData);

        //                }
        //                catch (Exception ex2)
        //                {

        //                }
        //            }

        //            return ret;
        //        }

        //        public async Task<ATTRIBUTE_SHOPEE_AND_OPT> GetAttributeToList(ShopeeAPIData iden, CATEGORY_SHOPEE category)
        //        {
        //            int MOPartnerID = 841371;
        //            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
        //            ATTRIBUTE_SHOPEE_AND_OPT ret = new ATTRIBUTE_SHOPEE_AND_OPT();

        //            long seconds = CurrentTimeSecond();
        //            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

        //            //ganti
        //            string urll = "https://partner.shopeemobile.com/api/v1/item/attributes/get";

        //            //ganti
        //            ShopeeGetAttributeData HttpBody = new ShopeeGetAttributeData
        //            {
        //                partner_id = MOPartnerID,
        //                shopid = Convert.ToInt32(iden.merchant_code),
        //                timestamp = seconds,
        //                language = "id",
        //                category_id = Convert.ToInt32(category.CATEGORY_CODE)
        //            };

        //            string myData = JsonConvert.SerializeObject(HttpBody);

        //            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

        //            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
        //            myReq.Method = "POST";
        //            myReq.Headers.Add("Authorization", signature);
        //            myReq.Accept = "application/json";
        //            myReq.ContentType = "application/json";
        //            string responseFromServer = "";
        //            try
        //            {
        //                myReq.ContentLength = myData.Length;
        //                using (var dataStream = myReq.GetRequestStream())
        //                {
        //                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
        //                }
        //                using (WebResponse response = await myReq.GetResponseAsync())
        //                {
        //                    using (Stream stream = response.GetResponseStream())
        //                    {
        //                        StreamReader reader = new StreamReader(stream);
        //                        responseFromServer = reader.ReadToEnd();
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //            }

        //            if (responseFromServer != null)
        //            {
        //                try
        //                {
        //                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetAttributeResult)) as ShopeeGetAttributeResult;
        //#if AWS
        //                        string con = "Data Source=localhost;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
        //#elif Debug_AWS
        //                    string con = "Data Source=13.250.232.74;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
        //#else
        //                    string con = "Data Source=13.251.222.53;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
        //#endif
        //                    string a = "";
        //                    int i = 0;
        //                    ATTRIBUTE_SHOPEE returnData = new ATTRIBUTE_SHOPEE();
        //                    foreach (var attribs in result.attributes)
        //                    {
        //                        a = Convert.ToString(i + 1);
        //                        returnData.CATEGORY_CODE = category.CATEGORY_CODE;
        //                        returnData.CATEGORY_NAME = category.CATEGORY_NAME;

        //                        returnData["ACODE_" + a] = Convert.ToString(attribs.attribute_id);
        //                        returnData["ATYPE_" + a] = attribs.options.Count() > 0 ? "PREDEFINED_ATTRIBUTE" : "DESCRIPTIVE_ATTRIBUTE";
        //                        returnData["ANAME_" + a] = attribs.attribute_name;
        //                        returnData["AOPTIONS_" + a] = attribs.options.Count() > 0 ? "1" : "0";
        //                        returnData["AMANDATORY_" + a] = attribs.is_mandatory ? "1" : "0";

        //                        if (attribs.options.Count() > 0)
        //                        {
        //                            var optList = attribs.options.ToList();
        //                            var listOpt = optList.Select(x => new ATTRIBUTE_OPT_SHOPEE(attribs.attribute_id.ToString(), x)).ToList();
        //                            ret.attribute_opts.AddRange(listOpt);
        //                        }
        //                        i = i + 1;
        //                    }
        //                    ret.attributes.Add(returnData);

        //                }
        //                catch (Exception ex2)
        //                {

        //                }
        //            }

        //            return ret;
        //        }

        //        public async Task<ATTRIBUTE_SHOPEE_AND_OPT_v2> GetAttributeToList_V2(ShopeeAPIData dataAPI, CATEGORY_SHOPEE_V2 category)
        //        {
        //            dataAPI = await RefreshTokenShopee_V2(dataAPI, false);
        //            int MOPartnerID = MOPartnerIDV2;
        //            string MOPartnerKey = MOPartnerKeyV2;
        //            var ret = new ATTRIBUTE_SHOPEE_AND_OPT_v2();

        //            long seconds = CurrentTimeSecond();
        //            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

        //            //ganti
        //            //string urll = "https://partner.shopeemobile.com/api/v1/item/attributes/get";

        //            ////ganti
        //            //ShopeeGetAttributeData HttpBody = new ShopeeGetAttributeData
        //            //{
        //            //    partner_id = MOPartnerID,
        //            //    shopid = Convert.ToInt32(iden.merchant_code),
        //            //    timestamp = seconds,
        //            //    language = "id",
        //            //    category_id = Convert.ToInt32(category.CATEGORY_CODE)
        //            //};

        //            string urll = shopeeV2Url;
        //            string path = "/api/v2/product/get_attributes";

        //            var baseString = MOPartnerID + path + seconds + dataAPI.token + dataAPI.merchant_code;
        //            var sign = CreateSignAuthenShop_V2(baseString, MOPartnerKey);

        //            string param = "?partner_id=" + MOPartnerID + "&timestamp=" + seconds + "&access_token=" + dataAPI.token
        //                + "&shop_id=" + dataAPI.merchant_code + "&sign=" + sign + "&language=id&category_id=" + category.CATEGORY_CODE;

        //            //string myData = JsonConvert.SerializeObject(HttpBody);

        //            //string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

        //            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + path + param);
        //            myReq.Method = "GET";
        //            //myReq.Headers.Add("Authorization", signature);
        //            myReq.Accept = "application/json";
        //            myReq.ContentType = "application/json";
        //            string responseFromServer = "";
        //            try
        //            {
        //                //myReq.ContentLength = myData.Length;
        //                //using (var dataStream = myReq.GetRequestStream())
        //                //{
        //                //    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
        //                //}
        //                using (WebResponse response = await myReq.GetResponseAsync())
        //                {
        //                    using (Stream stream = response.GetResponseStream())
        //                    {
        //                        StreamReader reader = new StreamReader(stream);
        //                        responseFromServer = reader.ReadToEnd();
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //            }

        //            if (!string.IsNullOrEmpty(responseFromServer))
        //            {
        //                try
        //                {
        //                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetAttributeResult_V2)) as ShopeeGetAttributeResult_V2;

        //                    string a = "";
        //                    int i = 0;
        //                    ATTRIBUTE_SHOPEE_V2 returnData = new ATTRIBUTE_SHOPEE_V2();
        //                    if (result.response.attribute_list != null)
        //                        foreach (var attribs in result.response.attribute_list)
        //                        {
        //                            a = Convert.ToString(i + 1);
        //                            returnData.CATEGORY_CODE = category.CATEGORY_CODE;
        //                            returnData.CATEGORY_NAME = category.CATEGORY_NAME;

        //                            returnData["ACODE_" + a] = Convert.ToString(attribs.attribute_id);
        //                            returnData["ATYPE_" + a] = attribs.input_type + "|" + attribs.input_validation_type;
        //                            returnData["ANAME_" + a] = attribs.original_attribute_name;
        //                            returnData["AOPTIONS_" + a] = attribs.attribute_value_list.Count() > 0 ? "1" : "0";
        //                            returnData["AMANDATORY_" + a] = attribs.is_mandatory ? "1" : "0";
        //                            returnData["AUNIT_" + a] = "0";

        //                            if (attribs.attribute_value_list.Count() > 0)
        //                            {
        //                                var optList = attribs.attribute_value_list.ToList();
        //                                var listOpt = optList.Select(x => new ATTRIBUTE_OPT_SHOPEE_V2(attribs.attribute_id.ToString(), x.original_value_name, x.value_id.ToString())).ToList();
        //                                ret.attribute_opts_v2.AddRange(listOpt);
        //                            }
        //                            if (attribs.attribute_unit != null)
        //                            {
        //                                if (attribs.attribute_unit.Length > 0)
        //                                {
        //                                    returnData["AUNIT_" + a] = "1";
        //                                    var unitList = attribs.attribute_unit.ToList();
        //                                    var listOpt = unitList.Select(x => new ATTRIBUTE_UNIT_SHOPEE_V2(attribs.attribute_id.ToString(), x, x)).ToList();
        //                                    ret.attribute_unit.AddRange(listOpt);
        //                                }

        //                            }
        //                            i = i + 1;
        //                        }
        //                    ret.attributes.Add(returnData);

        //                }
        //                catch (Exception ex2)
        //                {

        //                }
        //            }

        //            return ret;
        //        }

        //        public async Task<ShopeeGetBrandResult_V2> GetBrand_V2(ShopeeAPIData dataAPI, string category, int page)
        //        {
        //            dataAPI = await RefreshTokenShopee_V2(dataAPI, false);
        //            int MOPartnerID = MOPartnerIDV2;
        //            string MOPartnerKey = MOPartnerKeyV2;
        //            ShopeeGetBrandResult_V2 ret = new ShopeeGetBrandResult_V2();

        //            long seconds = CurrentTimeSecond();
        //            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

        //            string urll = shopeeV2Url;
        //            string path = "/api/v2/product/get_brand_list";

        //            var baseString = MOPartnerID + path + seconds + dataAPI.token + dataAPI.merchant_code;
        //            var sign = CreateSignAuthenShop_V2(baseString, MOPartnerKey);

        //            string param = "?partner_id=" + MOPartnerID + "&timestamp=" + seconds + "&access_token=" + dataAPI.token
        //                + "&shop_id=" + dataAPI.merchant_code + "&sign=" + sign + "&status=1&page_size=100&category_id=" + category + "&offset=" + (page);


        //            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + path + param);
        //            myReq.Method = "GET";
        //            myReq.Accept = "application/json";
        //            myReq.ContentType = "application/json";
        //            string responseFromServer = "";
        //            try
        //            {
        //                using (WebResponse response = await myReq.GetResponseAsync())
        //                {
        //                    using (Stream stream = response.GetResponseStream())
        //                    {
        //                        StreamReader reader = new StreamReader(stream);
        //                        responseFromServer = reader.ReadToEnd();
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //            }

        //            if (!string.IsNullOrEmpty(responseFromServer))
        //            {
        //                try
        //                {
        //                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetBrandResult_V2)) as ShopeeGetBrandResult_V2;
        //                    ret = result;
        //                    if (string.IsNullOrEmpty(ret.error))
        //                    {
        //                        foreach (var brand in result.response.brand_list)
        //                        {
        //                            var newData = new TEMP_SHOPEE_BRAND()
        //                            {
        //                                CATEGORY_CODE = category,
        //                                BRAND_ID = brand.brand_id.ToString(),
        //                                //NAME = brand.display_brand_name.Replace('\'', '`')
        //                                NAME = brand.original_brand_name.Replace('\'', '`')
        //                            };
        //                            if (ErasoftDbContext.TEMP_SHOPEE_BRAND.Where(m => m.CATEGORY_CODE == category && m.BRAND_ID == newData.BRAND_ID).ToList().Count == 0)
        //                            {
        //                                ErasoftDbContext.TEMP_SHOPEE_BRAND.Add(newData);
        //                                ErasoftDbContext.SaveChanges();
        //                            }

        //                        }
        //                        if (result.response.has_next_page)
        //                        {
        //                            await GetBrand_V2(dataAPI, category, result.response.next_offset);
        //                        }
        //                        else
        //                        {

        //                        }
        //                    }

        //                }
        //                catch (Exception ex2)
        //                {

        //                }
        //            }

        //            return ret;
        //        }
        //        //add by fauzi 21 Februari 2020
        //        public async Task<string> GetTokenShopee(ShopeeAPIData dataAPI, bool bForceRefresh)
        //        {
        //            string ret = "";
        //            DateTime dateNow = DateTime.UtcNow.AddHours(7);
        //            bool TokenExpired = false;
        //            if (!string.IsNullOrWhiteSpace(dataAPI.tgl_expired.ToString()))
        //            {
        //                if (dateNow >= dataAPI.tgl_expired)
        //                {
        //                    TokenExpired = true;
        //                }
        //            }
        //            else
        //            {
        //                TokenExpired = true;
        //            }

        //            if (TokenExpired || bForceRefresh)
        //            {
        //                int MOPartnerID = 841371;
        //                string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";

        //                long seconds = CurrentTimeSecond();
        //                DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

        //                MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
        //                {
        //                    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
        //                    REQUEST_ACTION = "Refresh Token Shopee", //ganti
        //                    REQUEST_DATETIME = milisBack,
        //                    REQUEST_ATTRIBUTE_1 = dataAPI.merchant_code,
        //                    REQUEST_STATUS = "Pending",
        //                };

        //                //ganti
        //                string urll = "https://partner.shopeemobile.com/api/v1/shop/get_partner_shop";

        //                //ganti
        //                ShopeeGetTokenShop HttpBody = new ShopeeGetTokenShop
        //                {
        //                    partner_id = MOPartnerID,
        //                    shopid = Convert.ToInt32(dataAPI.merchant_code),
        //                    timestamp = seconds
        //                };

        //                string myData = JsonConvert.SerializeObject(HttpBody);

        //                string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

        //                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
        //                myReq.Method = "POST";
        //                myReq.Headers.Add("Authorization", signature);
        //                myReq.Accept = "application/json";
        //                myReq.ContentType = "application/json";
        //                string responseFromServer = "";
        //                try
        //                {
        //                    myReq.ContentLength = myData.Length;
        //                    using (var dataStream = myReq.GetRequestStream())
        //                    {
        //                        dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
        //                    }
        //                    using (WebResponse response = await myReq.GetResponseAsync())
        //                    {
        //                        using (Stream stream = response.GetResponseStream())
        //                        {
        //                            StreamReader reader = new StreamReader(stream);
        //                            responseFromServer = reader.ReadToEnd();
        //                        }
        //                    }
        //                    currentLog.REQUEST_RESULT = "Process Get API Token Shopee";
        //                    manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, dataAPI, currentLog);
        //                }
        //                catch (Exception ex)
        //                {
        //                    currentLog.REQUEST_EXCEPTION = ex.Message.ToString();
        //                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
        //                }

        //                if (responseFromServer != null)
        //                {
        //                    try
        //                    {
        //                        var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetTokenShopResult)) as ShopeeGetTokenShopResult;
        //                        if (result.error == null && !string.IsNullOrWhiteSpace(result.ToString()))
        //                        {
        //                            if (result.authed_shops.Length > 0)
        //                            {
        //                                foreach (var item in result.authed_shops)
        //                                {
        //                                    if (item.shopid.ToString() == dataAPI.merchant_code.ToString())
        //                                    {
        //                                        var dateExpired = DateTimeOffset.FromUnixTimeSeconds(item.expire_time).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
        //                                        DatabaseSQL EDB = new DatabaseSQL(dataAPI.DatabasePathErasoft);
        //                                        var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '1', Sort1_Cust = '" + dataAPI.merchant_code + "', TGL_EXPIRED = '" + dateExpired + "' WHERE CUST = '" + dataAPI.no_cust + "'");
        //                                        if (resultquery != 0)
        //                                        {
        //                                            currentLog.REQUEST_RESULT = "Update Status API Complete";
        //                                            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, dataAPI, currentLog);
        //                                        }
        //                                        else
        //                                        {
        //                                            currentLog.REQUEST_RESULT = "Update Status API Failed";
        //                                            currentLog.REQUEST_EXCEPTION = "Failed Update Table";
        //                                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, dataAPI, currentLog);
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (!string.IsNullOrWhiteSpace(result.msg.ToString()))
        //                            {
        //                                currentLog.REQUEST_EXCEPTION = result.msg.ToString();
        //                                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
        //                            }
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        currentLog.REQUEST_EXCEPTION = ex.Message.ToString();
        //                        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
        //                    }
        //                }
        //            }
        //            return ret;
        //        }
        public async Task<ShopeeGetShopResult_V2> GetShopDetail_V2(ShopeeAPIDataChat dataAPI)
        {
            //dataAPI = await RefreshTokenShopee_V2(dataAPI, false);
            var ret = new ShopeeGetShopResult_V2();
            DateTime dateNow = DateTime.UtcNow.AddHours(7);

            int MOPartnerID = MOPartnerIDV2;
            string MOPartnerKey = MOPartnerKeyV2;

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //ganti
            string urll = shopeeV2Url;
            string path = "/api/v2/shop/get_shop_info";

            var baseString = MOPartnerID + path + seconds + dataAPI.token + dataAPI.merchant_code;
            var sign = CreateSignAuthenShop_V2(baseString, MOPartnerKey);

            string param = "?partner_id=" + MOPartnerID + "&timestamp=" + seconds + "&sign=" + sign
                + "&access_token=" + dataAPI.token + "&shop_id=" + dataAPI.merchant_code;
            //ganti
            //ShopeeGetTokenShop_V2 HttpBody = new ShopeeGetTokenShop_V2
            //{
            //    partner_id = MOPartnerID,
            //    shop_id = Convert.ToInt32(dataAPI.merchant_code),
            //    code = dataAPI.API_secret_key
            //};

            //string myData = JsonConvert.SerializeObject(HttpBody);

            //string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + path + param);
            myReq.Method = "GET";
            //myReq.Headers.Add("Authorization", signature);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            string responseFromServer = "";
            try
            {
                //myReq.ContentLength = myData.Length;
                //using (var dataStream = myReq.GetRequestStream())
                //{
                //    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
                //}
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
                }
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetShopResult_V2)) as ShopeeGetShopResult_V2;
                    if (string.IsNullOrWhiteSpace(result.error))
                    {
                        ret = result;
                    }

                }
                catch (Exception ex)
                {
                }
            }

            return ret;
        }

        public async Task<string> GetTokenShopee_V2(ShopeeAPIDataChat dataAPI, bool bForceRefresh)
        {
            string ret = "";
            DateTime dateNow = DateTime.UtcNow.AddHours(7);
            bool TokenExpired = false;
            //if (!string.IsNullOrWhiteSpace(dataAPI.tgl_expired.ToString()))
            //{
            //    if (dateNow >= dataAPI.tgl_expired)
            //    {
            //        TokenExpired = true;
            //    }
            //}
            //else
            //{
            //    TokenExpired = true;
            //}

            //if (TokenExpired || bForceRefresh)
            {
                int MOPartnerID = MOPartnerIDV2;
                string MOPartnerKey = MOPartnerKeyV2;

                long seconds = CurrentTimeSecond();
                DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

                MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                {
                    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                    REQUEST_ACTION = "Refresh Token Shopee V2", //ganti
                    REQUEST_DATETIME = milisBack,
                    REQUEST_ATTRIBUTE_1 = dataAPI.merchant_code,
                    REQUEST_STATUS = "Pending",
                };

                //ganti
                string urll = shopeeV2Url;
                string path = "/api/v2/auth/token/get";

                var baseString = MOPartnerID + path + seconds;
                var sign = CreateSignAuthenShop_V2(baseString, MOPartnerKey);

                string param = "?partner_id=" + MOPartnerID + "&timestamp=" + seconds + "&sign=" + sign;
                //ganti
                ShopeeGetTokenShop_V2 HttpBody = new ShopeeGetTokenShop_V2
                {
                    partner_id = MOPartnerID,
                    shop_id = Convert.ToInt32(dataAPI.merchant_code),
                    code = dataAPI.API_secret_key
                };

                string myData = JsonConvert.SerializeObject(HttpBody);

                //string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + path + param);
                myReq.Method = "POST";
                //myReq.Headers.Add("Authorization", signature);
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
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
                    currentLog.REQUEST_RESULT = "Process Get API Token Shopee";
                    manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, dataAPI, currentLog);
                }
                catch (Exception ex)
                {
                    currentLog.REQUEST_EXCEPTION = ex.Message.ToString();
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    //temp
                    //var simpanResponse = new BUKALAPAK_TOKEN()
                    //{
                    //    ACCOUNT = "test_auth_shopee",
                    //    CUST = DateTime.UtcNow.AddHours(7).ToString("HHmmss"),
                    //    VAR_1 = responseFromServer,
                    //    CREATED_AT = DateTime.UtcNow.AddHours(7)
                    //};
                    //MoDbContext.BUKALAPAK_TOKEN.Add(simpanResponse);
                    //MoDbContext.SaveChanges();
                    try
                    {
                        var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetTokenShopResult_V2)) as ShopeeGetTokenShopResult_V2;
                        if (string.IsNullOrWhiteSpace(result.error))
                        {
                            var tglExpired = "";
                            dataAPI.token = result.access_token;
                            var cekShopDetail = await GetShopDetail_V2(dataAPI);
                            if (cekShopDetail.expire_time > 0)
                            {
                                tglExpired = DateTimeOffset.FromUnixTimeSeconds(cekShopDetail.expire_time).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

                            }
                            if (tglExpired == "")
                            {
                                tglExpired = DateTime.UtcNow.AddHours(7).AddMonths(1).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            var dateExpired = DateTime.UtcNow.AddHours(7).AddSeconds(result.expire_in).ToString("yyyy-MM-dd HH:mm:ss");

                            //DatabaseSQL EDB = new DatabaseSQL(dataAPI.DatabasePathErasoft);
                            //var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '1', KD_ANALISA = '2', Sort1_Cust = '"
                            //    + dataAPI.merchant_code + "', TOKEN_EXPIRED = '" + dateExpired + "', TGL_EXPIRED = '" + tglExpired + "', API_KEY = '" + dataAPI.API_secret_key
                            //     + "', TOKEN = '" + result.access_token + "', REFRESH_TOKEN = '" + result.refresh_token
                            //    + "' WHERE CUST = '" + dataAPI.no_cust + "'");
                            DatabaseSQL EDB = new DatabaseSQL(dataAPI.DatabasePathErasoft);
                            var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API_CHAT  = '1', Sort1_Cust = '"
                                + dataAPI.merchant_code + "', TOKEN_EXPIRED_CHAT  = '" + dateExpired
                                 + "', TOKEN_CHAT = '" + result.access_token + "', REFRESH_TOKEN_CHAT = '" + result.refresh_token
                                 + "', TGL_EXPIRED_CHAT='" + tglExpired 
                                + "' WHERE CUST = '" + dataAPI.no_cust + "'");
                            if (resultquery != 0)
                            {
                                currentLog.REQUEST_RESULT = "Update Status API Complete";
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, dataAPI, currentLog);
                            }
                            else
                            {
                                currentLog.REQUEST_RESULT = "Update Status API Failed";
                                currentLog.REQUEST_EXCEPTION = "Failed Update Table";
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, dataAPI, currentLog);
                            }
                            //        }
                            //    }
                            //}
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(result.message.ToString()))
                            {
                                currentLog.REQUEST_EXCEPTION = result.message.ToString();
                                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        currentLog.REQUEST_EXCEPTION = ex.Message.ToString();
                        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
                    }
                }
            }
            return ret;
        }

        //protected void SetupContext(ShopeeAPIDataChat data)
        //{
        //    //string ret = "";
        //    MoDbContext = new MoDbContext("");
        //    //EDB = new DatabaseSQL(data.DatabasePathErasoft);
        //    //string EraServerName = EDB.GetServerName("sConn");
        //    ErasoftDbContext = new ErasoftContext(EraServerName, data.DatabasePathErasoft);
        //    username = data.username;
        //    //var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.RecNum == data.idmarket).SingleOrDefault();
        //    //if (arf01inDB != null)
        //    //{
        //    //    ret = arf01inDB.TOKEN;
        //    //}
        //    //return ret;
        //}
        protected void SetupContext(ShopeeAPIDataChat data)
        {
            //string ret = "";
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(data.DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, data.DatabasePathErasoft);
            username = data.username;
            //var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.RecNum == data.idmarket).SingleOrDefault();
            //if (arf01inDB != null)
            //{
            //    ret = arf01inDB.TOKEN;
            //}
            //return ret;
        }
        public async Task<ShopeeAPIDataChat> RefreshTokenShopee_V2(ShopeeAPIDataChat dataAPI, bool bForceRefresh)
        {
            ShopeeAPIDataChat ret = dataAPI;
            DateTime dateNow = DateTime.UtcNow.AddHours(7);
            bool TokenExpired = false;

            SetupContext(dataAPI);

            if (!string.IsNullOrWhiteSpace(dataAPI.TOKEN_EXPIRED_CHAT.ToString()))
            {
                if (dataAPI.TOKEN_EXPIRED_CHAT < DateTime.UtcNow.AddHours(7).AddMinutes(30))
                {
                    var cekInDB = ErasoftDbContext.ARF01.Where(m => m.CUST == dataAPI.no_cust).FirstOrDefault();
                    if (cekInDB != null)
                    {
                        //if (dataAPI.token != cekInDB.TOKEN)
                        if (dataAPI.TOKEN_EXPIRED_CHAT != cekInDB.TOKEN_EXPIRED_CHAT)
                        {
                            dataAPI.refresh_token = cekInDB.REFRESH_TOKEN;
                            dataAPI.tgl_expired = cekInDB.TGL_EXPIRED.Value;
                            dataAPI.token_expired = cekInDB.TOKEN_EXPIRED.Value;
                            dataAPI.token = cekInDB.TOKEN;
                            dataAPI.TOKEN_CHAT = cekInDB.TOKEN_CHAT;
                            dataAPI.TOKEN_EXPIRED_CHAT = cekInDB.TOKEN_EXPIRED_CHAT;
                            dataAPI.STATUS_API_CHAT = cekInDB.STATUS_API_CHAT;
                            dataAPI.STATUS_SYNC_CHAT = cekInDB.STATUS_SYNC_CHAT;
                            dataAPI.REFRESH_TOKEN_CHAT = cekInDB.REFRESH_TOKEN_CHAT;

                            if (cekInDB.TOKEN_EXPIRED_CHAT.Value.AddMinutes(-30) > DateTime.UtcNow.AddHours(7))
                            {
                                return dataAPI;
                            }
                        }
                    }
                    TokenExpired = true;
                }
            }
            else
            {
                TokenExpired = true;
            }

            if (TokenExpired || bForceRefresh)
            {
                int MOPartnerID = MOPartnerIDV2;
                string MOPartnerKey = MOPartnerKeyV2;

                long seconds = CurrentTimeSecond();
                DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

                //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                //{
                //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                //    REQUEST_ACTION = "Refresh Token Shopee V2", //ganti
                //    REQUEST_DATETIME = milisBack,
                //    REQUEST_ATTRIBUTE_1 = dataAPI.merchant_code,
                //    REQUEST_STATUS = "Pending",
                //};

                //ganti
                string urll = shopeeV2Url;
                string path = "/api/v2/auth/access_token/get";

                var baseString = MOPartnerID + path + seconds;
                var sign = CreateSignAuthenShop_V2(baseString, MOPartnerKey);

                string param = "?partner_id=" + MOPartnerID + "&timestamp=" + seconds + "&sign=" + sign;

                //string myData = JsonConvert.SerializeObject(HttpBody);
                string myData = "{\"refresh_token\":\"" + dataAPI.REFRESH_TOKEN_CHAT + "\",\"partner_id\":" + MOPartnerID + ",\"shop_id\":" + dataAPI.merchant_code + "}";
                //string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + path + param);
                myReq.Method = "POST";
                //myReq.Headers.Add("Authorization", signature);
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";
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
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        WebResponse resp = e.Response;
                        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                        {
                            err = sr.ReadToEnd();
                        }
                    }
                    MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                    {
                        REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                        REQUEST_ACTION = "Refresh Token Shopee V2 Chat", //ganti
                        REQUEST_DATETIME = milisBack,
                        REQUEST_ATTRIBUTE_1 = dataAPI.merchant_code
                    };
                    currentLog.REQUEST_EXCEPTION = err;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
                }

                if (responseFromServer != null)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetTokenShopResult_V2)) as ShopeeGetTokenShopResult_V2;
                        if (string.IsNullOrWhiteSpace(result.error))
                        {
                            if (!string.IsNullOrEmpty(result.access_token))
                            {
                                dataAPI.TOKEN_CHAT = result.access_token;
                                dataAPI.REFRESH_TOKEN_CHAT = result.refresh_token;
                                dataAPI.TOKEN_EXPIRED_CHAT = DateTime.UtcNow.AddHours(7).AddSeconds(result.expire_in);
                                var dateExpired = dataAPI.TOKEN_EXPIRED_CHAT.Value.ToString("yyyy-MM-dd HH:mm:ss");

                                //DatabaseSQL EDB = new DatabaseSQL(dataAPI.DatabasePathErasoft);
                                //var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API_CHAT = '1', Sort1_Cust = '"
                                //    + dataAPI.merchant_code + "', TOKEN_EXPIRED_CHAT = '" + dateExpired + "', API_KEY = '" + dataAPI.API_secret_key
                                //     + "', TOKEN_CHAT = '" + result.access_token + "', REFRESH_TOKEN_CHAT = '" + result.refresh_token
                                //    + "' WHERE CUST = '" + dataAPI.no_cust + "'");
                                var sSQL = "UPDATE ARF01 SET STATUS_API_CHAT = '1', Sort1_Cust = '"
                                    + dataAPI.merchant_code + "', TOKEN_EXPIRED_CHAT = '" + dateExpired + "', API_KEY = '" + dataAPI.API_secret_key
                                     + "', TOKEN_CHAT = '" + result.access_token + "', REFRESH_TOKEN_CHAT = '" + result.refresh_token
                                    + "' WHERE CUST = '" + dataAPI.no_cust + "'";
                                var resultquery = ErasoftDbContext.Database.ExecuteSqlCommand(sSQL);
                            }
                            else
                            {
                                MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                                {
                                    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                                    REQUEST_ACTION = "Refresh Token Shopee V2 Chat", //ganti
                                    REQUEST_DATETIME = milisBack,
                                    REQUEST_ATTRIBUTE_1 = dataAPI.merchant_code
                                };
                                currentLog.REQUEST_EXCEPTION = responseFromServer;
                                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
                            }
                        }
                        else
                        {
                            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                            {
                                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                                REQUEST_ACTION = "Refresh Token Shopee V2 Chat", //ganti
                                REQUEST_DATETIME = milisBack,
                                REQUEST_ATTRIBUTE_1 = dataAPI.merchant_code
                            };
                            currentLog.REQUEST_EXCEPTION = responseFromServer;
                            manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
                        }

                    }
                    catch (Exception ex)
                    {
                        //currentLog.REQUEST_EXCEPTION = ex.Message.ToString();
                        //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
                    }
                }
            }
            return ret;
        }

        //public async Task<ShopeeAPIData> RefreshTokenShopee_V2(ShopeeAPIData dataAPI, bool bForceRefresh)
        //{
        //    ShopeeAPIData ret = dataAPI;
        //    DateTime dateNow = DateTime.UtcNow.AddHours(7);
        //    bool TokenExpired = false;
        //    if (!string.IsNullOrWhiteSpace(dataAPI.token_expired.ToString()))
        //    {
        //        if (dataAPI.token_expired < DateTime.UtcNow.AddHours(7).AddMinutes(30))
        //        {
        //            var cekInDB = ErasoftDbContext.ARF01.Where(m => m.CUST == dataAPI.no_cust).FirstOrDefault();
        //            if (cekInDB != null)
        //            {
        //                //if (dataAPI.token != cekInDB.TOKEN)
        //                if (dataAPI.token_expired != cekInDB.TOKEN_EXPIRED)
        //                {
        //                    dataAPI.refresh_token = cekInDB.REFRESH_TOKEN;
        //                    dataAPI.tgl_expired = cekInDB.TGL_EXPIRED.Value;
        //                    dataAPI.token_expired = cekInDB.TOKEN_EXPIRED.Value;
        //                    dataAPI.token = cekInDB.TOKEN;

        //                    if (cekInDB.TOKEN_EXPIRED.Value.AddMinutes(-30) > DateTime.UtcNow.AddHours(7))
        //                    {
        //                        return dataAPI;
        //                    }
        //                }
        //            }
        //            TokenExpired = true;
        //        }
        //    }
        //    else
        //    {
        //        TokenExpired = true;
        //    }

        //    if (TokenExpired || bForceRefresh)
        //    {
        //        int MOPartnerID = MOPartnerIDV2;
        //        string MOPartnerKey = MOPartnerKeyV2;

        //        long seconds = CurrentTimeSecond();
        //        DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

        //        //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
        //        //{
        //        //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
        //        //    REQUEST_ACTION = "Refresh Token Shopee V2", //ganti
        //        //    REQUEST_DATETIME = milisBack,
        //        //    REQUEST_ATTRIBUTE_1 = dataAPI.merchant_code,
        //        //    REQUEST_STATUS = "Pending",
        //        //};

        //        //ganti
        //        string urll = shopeeV2Url;
        //        string path = "/api/v2/auth/access_token/get";

        //        var baseString = MOPartnerID + path + seconds;
        //        var sign = CreateSignAuthenShop_V2(baseString, MOPartnerKey);

        //        string param = "?partner_id=" + MOPartnerID + "&timestamp=" + seconds + "&sign=" + sign;

        //        //string myData = JsonConvert.SerializeObject(HttpBody);
        //        string myData = "{\"refresh_token\":\"" + dataAPI.refresh_token + "\",\"partner_id\":" + MOPartnerID + ",\"shop_id\":" + dataAPI.merchant_code + "}";
        //        //string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

        //        HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + path + param);
        //        myReq.Method = "POST";
        //        //myReq.Headers.Add("Authorization", signature);
        //        myReq.Accept = "application/json";
        //        myReq.ContentType = "application/json";
        //        string responseFromServer = "";
        //        try
        //        {
        //            myReq.ContentLength = myData.Length;
        //            using (var dataStream = myReq.GetRequestStream())
        //            {
        //                dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
        //            }
        //            using (WebResponse response = await myReq.GetResponseAsync())
        //            {
        //                using (Stream stream = response.GetResponseStream())
        //                {
        //                    StreamReader reader = new StreamReader(stream);
        //                    responseFromServer = reader.ReadToEnd();
        //                }
        //            }
        //            //currentLog.REQUEST_RESULT = "Process Get API Token Shopee";
        //            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, dataAPI, currentLog);
        //        }
        //        catch (WebException e)
        //        {
        //            string err = "";
        //            //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
        //            //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
        //            if (e.Status == WebExceptionStatus.ProtocolError)
        //            {
        //                WebResponse resp = e.Response;
        //                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
        //                {
        //                    err = sr.ReadToEnd();
        //                }
        //            }
        //            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
        //            {
        //                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
        //                REQUEST_ACTION = "Refresh Token Shopee V2", //ganti
        //                REQUEST_DATETIME = milisBack,
        //                REQUEST_ATTRIBUTE_1 = dataAPI.merchant_code
        //            };
        //            currentLog.REQUEST_EXCEPTION = err;
        //            manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
        //        }

        //        if (responseFromServer != "")
        //        {
        //            //temp
        //            //var simpanResponse = new BUKALAPAK_TOKEN()
        //            //{
        //            //    ACCOUNT = "test_auth_shopee",
        //            //    CUST = DateTime.UtcNow.AddHours(7).ToString("HHmmss"),
        //            //    VAR_1 = responseFromServer,
        //            //    CREATED_AT = DateTime.UtcNow.AddHours(7)
        //            //};
        //            //MoDbContext.BUKALAPAK_TOKEN.Add(simpanResponse);
        //            //MoDbContext.SaveChanges();
        //            try
        //            {
        //                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetTokenShopResult_V2)) as ShopeeGetTokenShopResult_V2;
        //                if (string.IsNullOrWhiteSpace(result.error))
        //                {
        //                    if (!string.IsNullOrEmpty(result.access_token))
        //                    {
        //                        dataAPI.token = result.access_token;
        //                        dataAPI.refresh_token = result.refresh_token;
        //                        dataAPI.token_expired = DateTime.UtcNow.AddHours(7).AddSeconds(result.expire_in);
        //                        //if (result.authed_shops.Length > 0)
        //                        //{
        //                        //    foreach (var item in result.authed_shops)
        //                        //    {
        //                        //        if (item.shopid.ToString() == dataAPI.merchant_code.ToString())
        //                        //        {
        //                        //var dateExpired = DateTimeOffset.FromUnixTimeSeconds(result.expire_in).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
        //                        var dateExpired = dataAPI.token_expired.Value.ToString("yyyy-MM-dd HH:mm:ss");

        //                        DatabaseSQL EDB = new DatabaseSQL(dataAPI.DatabasePathErasoft);
        //                        var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API_CHAT  = '1', Sort1_Cust = '"
        //                            + dataAPI.merchant_code + "', TOKEN_EXPIRED_CHAT  = '" + dateExpired 
        //                             + "', TOKEN_CHAT = '" + result.access_token + "', REFRESH_TOKEN_CHAT = '" + result.refresh_token
        //                            + "' WHERE CUST = '" + dataAPI.no_cust + "'");
        //                        if (resultquery != 0)
        //                        {
        //                            //currentLog.REQUEST_RESULT = "Update Status API Complete";
        //                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, dataAPI, currentLog);
        //                        }
        //                        else
        //                        {
        //                            //currentLog.REQUEST_RESULT = "Update Status API Failed";
        //                            //currentLog.REQUEST_EXCEPTION = "Failed Update Table";
        //                            //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, dataAPI, currentLog);
        //                        }
        //                        //        }
        //                        //    }
        //                        //}
        //                    }
        //                    else
        //                    {
        //                        MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
        //                        {
        //                            REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
        //                            REQUEST_ACTION = "Refresh Token Shopee V2", //ganti
        //                            REQUEST_DATETIME = milisBack,
        //                            REQUEST_ATTRIBUTE_1 = dataAPI.merchant_code
        //                        };
        //                        currentLog.REQUEST_EXCEPTION = responseFromServer;
        //                        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
        //                    }
        //                }
        //                else
        //                {
        //                    if (!string.IsNullOrWhiteSpace(result.message.ToString()))
        //                    {
        //                        //currentLog.REQUEST_EXCEPTION = result.message.ToString();
        //                        //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
        //                    }
        //                    MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
        //                    {
        //                        REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
        //                        REQUEST_ACTION = "Refresh Token Shopee V2", //ganti
        //                        REQUEST_DATETIME = milisBack,
        //                        REQUEST_ATTRIBUTE_1 = dataAPI.merchant_code
        //                    };
        //                    currentLog.REQUEST_EXCEPTION = responseFromServer;
        //                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                //currentLog.REQUEST_EXCEPTION = ex.Message.ToString();
        //                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
        //            }
        //        }
        //    }
        //    return ret;
        //}
//        public async Task<string> GetCategoryShopee_V2(ShopeeAPIData dataAPI)
//        {
//            dataAPI = await RefreshTokenShopee_V2(dataAPI, false);
//            string ret = "";
//            DateTime dateNow = DateTime.UtcNow.AddHours(7);

//            {
//                int MOPartnerID = MOPartnerIDV2;
//                string MOPartnerKey = MOPartnerKeyV2;

//                long seconds = CurrentTimeSecond();
//                DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//                //ganti
//                string urll = shopeeV2Url;
//                string path = "/api/v2/product/get_category";

//                var baseString = MOPartnerID + path + seconds + dataAPI.token + dataAPI.merchant_code;
//                var sign = CreateSignAuthenShop_V2(baseString, MOPartnerKey);

//                string param = "?partner_id=" + MOPartnerID + "&timestamp=" + seconds + "&access_token=" + dataAPI.token
//                    + "&shop_id=" + dataAPI.merchant_code + "&sign=" + sign + "&language=id";

//                //string myData = "{\"refresh_token\":\"" + dataAPI.refresh_token + "\",\"partner_id\":" + MOPartnerID + ",\"shop_id\":" + dataAPI.merchant_code + "}";

//                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + path + param);
//                myReq.Method = "GET";
//                myReq.Accept = "application/json";
//                myReq.ContentType = "application/json";
//                string responseFromServer = "";
//                try
//                {
//                    //myReq.ContentLength = myData.Length;
//                    //using (var dataStream = myReq.GetRequestStream())
//                    //{
//                    //    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                    //}
//                    using (WebResponse response = await myReq.GetResponseAsync())
//                    {
//                        using (Stream stream = response.GetResponseStream())
//                        {
//                            StreamReader reader = new StreamReader(stream);
//                            responseFromServer = reader.ReadToEnd();
//                        }
//                    }
//                }
//                catch (WebException e)
//                {
//                    string err = "";
//                    //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                    if (e.Status == WebExceptionStatus.ProtocolError)
//                    {
//                        WebResponse resp = e.Response;
//                        using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
//                        {
//                            err = sr.ReadToEnd();
//                        }
//                    }
//                }

//                if (!string.IsNullOrEmpty(responseFromServer))
//                {
//                    try
//                    {
//                        var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetCategoryResult_V2)) as ShopeeGetCategoryResult_V2;
//#if AWS
//                string con = "Data Source=172.31.20.192;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
//#elif Debug_AWS
//                        string con = "Data Source=54.151.175.62, 12354;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
//#elif DEV
//                        string con = "Data Source=172.31.20.73;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
//#else
//                        string con = "Data Source=54.151.175.62, 45650;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
//#endif

//                        using (SqlConnection oConnection = new SqlConnection(con))
//                        {
//                            oConnection.Open();
//                            //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
//                            //{
//                            using (SqlCommand oCommand = oConnection.CreateCommand())
//                            {
//                                oCommand.CommandText = "DELETE FROM [CATEGORY_SHOPEE_V2]";
//                                oCommand.ExecuteNonQuery();
//                                //oCommand.Transaction = oTransaction;
//                                oCommand.CommandType = CommandType.Text;
//                                oCommand.CommandText = "INSERT INTO [CATEGORY_SHOPEE_V2] ([CATEGORY_CODE], [CATEGORY_NAME], [PARENT_CODE], [IS_LAST_NODE], [MASTER_CATEGORY_CODE]) VALUES (@CATEGORY_CODE, @CATEGORY_NAME, @PARENT_CODE, @IS_LAST_NODE, @MASTER_CATEGORY_CODE)";
//                                //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
//                                oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
//                                oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));
//                                oCommand.Parameters.Add(new SqlParameter("@PARENT_CODE", SqlDbType.NVarChar, 50));
//                                oCommand.Parameters.Add(new SqlParameter("@IS_LAST_NODE", SqlDbType.NVarChar, 1));
//                                oCommand.Parameters.Add(new SqlParameter("@MASTER_CATEGORY_CODE", SqlDbType.NVarChar, 50));

//                                try
//                                {
//                                    //foreach (var item in result.categories.Where(P => P.parent_id == 0)) //foreach parent level top
//                                    foreach (var item in result.response.category_list) //foreach parent level top
//                                    {
//                                        oCommand.Parameters[0].Value = item.category_id;
//                                        oCommand.Parameters[1].Value = item.display_category_name;
//                                        oCommand.Parameters[2].Value = item.parent_category_id == 0 ? "" : item.parent_category_id.ToString();
//                                        oCommand.Parameters[3].Value = item.has_children ? "0" : "1";
//                                        oCommand.Parameters[4].Value = "";
//                                        if (oCommand.ExecuteNonQuery() > 0)
//                                        {
//                                            //if (item.has_children)
//                                            //{
//                                            //    RecursiveInsertCategory(oCommand, result.categories, item.category_id, item.category_id);
//                                            //}
//                                        }
//                                        else
//                                        {

//                                        }
//                                    }
//                                    //oTransaction.Commit();
//                                }
//                                catch (Exception ex)
//                                {
//                                    //oTransaction.Rollback();
//                                }
//                            }
//                        }
//                    }
//                    catch (Exception ex2)
//                    {

//                    }
//                }
//            }
//            return ret;
//        }
//        public async Task<string> GetAttribute(ShopeeAPIData iden)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";
//            var categories = MoDbContext.CategoryShopee.Where(p => p.IS_LAST_NODE.Equals("1")).ToList();
//            foreach (var category in categories)
//            {
//                long seconds = CurrentTimeSecond();
//                DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//                //ganti
//                string urll = "https://partner.shopeemobile.com/api/v1/item/attributes/get";

//                //ganti
//                ShopeeGetAttributeData HttpBody = new ShopeeGetAttributeData
//                {
//                    partner_id = MOPartnerID,
//                    shopid = Convert.ToInt32(iden.merchant_code),
//                    timestamp = seconds,
//                    language = "id",
//                    category_id = Convert.ToInt32(category.CATEGORY_CODE)
//                };

//                string myData = JsonConvert.SerializeObject(HttpBody);

//                string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//                myReq.Method = "POST";
//                myReq.Headers.Add("Authorization", signature);
//                myReq.Accept = "application/json";
//                myReq.ContentType = "application/json";
//                string responseFromServer = "";
//                try
//                {
//                    myReq.ContentLength = myData.Length;
//                    using (var dataStream = myReq.GetRequestStream())
//                    {
//                        dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                    }
//                    using (WebResponse response = await myReq.GetResponseAsync())
//                    {
//                        using (Stream stream = response.GetResponseStream())
//                        {
//                            StreamReader reader = new StreamReader(stream);
//                            responseFromServer = reader.ReadToEnd();
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                }

//                if (responseFromServer != null)
//                {
//                    try
//                    {
//                        var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetAttributeResult)) as ShopeeGetAttributeResult;
//#if AWS
//                        string con = "Data Source=localhost;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
//#elif Debug_AWS
//                        string con = "Data Source=13.250.232.74;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
//#else
//                        string con = "Data Source=13.251.222.53;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
//#endif
//                        using (SqlConnection oConnection = new SqlConnection(con))
//                        {
//                            oConnection.Open();
//                            //using (SqlTransaction oTransaction = oConnection.BeginTransaction())
//                            //{
//                            using (SqlCommand oCommand = oConnection.CreateCommand())
//                            {
//                                var AttributeInDb = MoDbContext.AttributeShopee.Where(p => p.CATEGORY_CODE.ToUpper().Equals(category.CATEGORY_CODE)).ToList();
//                                //cek jika belum ada di database, insert
//                                var cari = AttributeInDb;
//                                if (cari.Count() == 0)
//                                {
//                                    oCommand.CommandType = CommandType.Text;
//                                    oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
//                                    oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));

//                                    string sSQL = "INSERT INTO [ATTRIBUTE_SHOPEE] ([CATEGORY_CODE], [CATEGORY_NAME],";
//                                    string sSQLValue = ") VALUES (@CATEGORY_CODE, @CATEGORY_NAME,";
//                                    string a = "";
//                                    int i = 0;
//                                    foreach (var attribs in result.attributes)
//                                    {
//                                        a = Convert.ToString(i + 1);
//                                        sSQL += "[ACODE_" + a + "],[ATYPE_" + a + "],[ANAME_" + a + "],[AOPTIONS_" + a + "],[AMANDATORY_" + a + "],";
//                                        sSQLValue += "@ACODE_" + a + ",@ATYPE_" + a + ",@ANAME_" + a + ",@AOPTIONS_" + a + ",@AMANDATORY_" + a + ",";
//                                        oCommand.Parameters.Add(new SqlParameter("@ACODE_" + a, SqlDbType.NVarChar, 50));
//                                        oCommand.Parameters.Add(new SqlParameter("@ATYPE_" + a, SqlDbType.NVarChar, 50));
//                                        oCommand.Parameters.Add(new SqlParameter("@ANAME_" + a, SqlDbType.NVarChar, 250));
//                                        oCommand.Parameters.Add(new SqlParameter("@AOPTIONS_" + a, SqlDbType.NVarChar, 1));
//                                        oCommand.Parameters.Add(new SqlParameter("@AMANDATORY_" + a, SqlDbType.NVarChar, 1));

//                                        a = Convert.ToString(i * 5 + 2);
//                                        oCommand.Parameters[(i * 5) + 2].Value = "";
//                                        oCommand.Parameters[(i * 5) + 3].Value = "";
//                                        oCommand.Parameters[(i * 5) + 4].Value = "";
//                                        oCommand.Parameters[(i * 5) + 5].Value = "";
//                                        oCommand.Parameters[(i * 5) + 6].Value = "";

//                                        oCommand.Parameters[(i * 5) + 2].Value = attribs.attribute_id;
//                                        oCommand.Parameters[(i * 5) + 3].Value = attribs.options.Count() > 0 ? "PREDEFINED_ATTRIBUTE" : "DESCRIPTIVE_ATTRIBUTE";
//                                        oCommand.Parameters[(i * 5) + 4].Value = attribs.attribute_name;
//                                        oCommand.Parameters[(i * 5) + 5].Value = attribs.options.Count() > 0 ? "1" : "0";
//                                        oCommand.Parameters[(i * 5) + 6].Value = attribs.is_mandatory ? "1" : "0";

//                                        if (attribs.options.Count() > 0)
//                                        {
//                                            var AttributeOptInDb = MoDbContext.AttributeOptShopee.AsNoTracking().ToList();
//                                            foreach (var option in attribs.options)
//                                            {
//                                                var cariOpt = AttributeOptInDb.Where(p => p.ACODE == Convert.ToString(attribs.attribute_id) && p.OPTION_VALUE == option);
//                                                if (cariOpt.Count() == 0)
//                                                {
//                                                    using (SqlCommand oCommand2 = oConnection.CreateCommand())
//                                                    {
//                                                        oCommand2.CommandType = CommandType.Text;
//                                                        oCommand2.CommandText = "INSERT INTO ATTRIBUTE_OPT_SHOPEE ([ACODE], [OPTION_VALUE]) VALUES (@ACODE, @OPTION_VALUE)";
//                                                        oCommand2.Parameters.Add(new SqlParameter("@ACODE", SqlDbType.NVarChar, 50));
//                                                        oCommand2.Parameters.Add(new SqlParameter("@OPTION_VALUE", SqlDbType.NVarChar, 250));
//                                                        oCommand2.Parameters[0].Value = attribs.attribute_id;
//                                                        oCommand2.Parameters[1].Value = option;
//                                                        oCommand2.ExecuteNonQuery();
//                                                    }
//                                                }
//                                            }
//                                        }
//                                        i = i + 1;
//                                    }
//                                    sSQL = sSQL.Substring(0, sSQL.Length - 1) + sSQLValue.Substring(0, sSQLValue.Length - 1) + ")";
//                                    oCommand.CommandText = sSQL;
//                                    oCommand.Parameters[0].Value = category.CATEGORY_CODE;
//                                    oCommand.Parameters[1].Value = "";
//                                    oCommand.ExecuteNonQuery();
//                                }
//                            }
//                        }
//                    }
//                    catch (Exception ex2)
//                    {

//                    }
//                }
//            }
//            return ret;
//        }
//        public async Task<string> GetOrderByStatus(ShopeeAPIData iden, StatusOrder stat, string connID, string CUST, string NAMA_CUST, int page)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";

//            long seconds = CurrentTimeSecond();
//            long timestamp7Days = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();

//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            string urll = "https://partner.shopeemobile.com/api/v1/orders/get";

//            ShopeeGetOrderByStatusData HttpBody = new ShopeeGetOrderByStatusData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                pagination_offset = page,
//                pagination_entries_per_page = 50,
//                create_time_from = timestamp7Days,
//                create_time_to = seconds,
//                order_status = stat.ToString()
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {

//                    var listOrder = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetOrderByStatusResult)) as ShopeeGetOrderByStatusResult;
//                    if (stat == StatusOrder.READY_TO_SHIP)
//                    {
//                        string[] ordersn_list = listOrder.orders.Select(p => p.ordersn).ToArray();
//                        //add by calvin 4 maret 2019, filter
//                        var dariTgl = DateTimeOffset.UtcNow.AddDays(-30).DateTime;
//                        var SudahAdaDiMO = ErasoftDbContext.SOT01A.Where(p => p.USER_NAME == "Auto Shopee" && p.TGL >= dariTgl).Select(p => p.NO_REFERENSI).ToList();
//                        //end add by calvin
//                        var filtered = ordersn_list.Where(p => !SudahAdaDiMO.Contains(p));
//                        if (filtered.Count() > 0)
//                        {
//                            await GetOrderDetails(iden, filtered.ToArray(), connID, CUST, NAMA_CUST);
//                        }
//                        if (listOrder.more)
//                        {
//                            await GetOrderByStatus(iden, stat, connID, CUST, NAMA_CUST, page + 50);
//                        }
//                    }

//                }
//                catch (Exception ex2)
//                {
//                }
//            }
//            return ret;
//        }
//        public async Task<string> GetOrderDetails(ShopeeAPIData iden, string[] ordersn_list, string connID, string CUST, string NAMA_CUST)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
//                REQUEST_ACTION = "Get Order List", //ganti
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
//                REQUEST_STATUS = "Pending",
//            };

//            string urll = "https://partner.shopeemobile.com/api/v1/orders/detail";

//            GetOrderDetailsData HttpBody = new GetOrderDetailsData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                ordersn_list = ordersn_list
//                //ordersn_list = ordersn_list_test.ToArray()
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetOrderDetailsResult)) as ShopeeGetOrderDetailsResult;
//                    var connIdARF01C = Guid.NewGuid().ToString();
//                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                    TEMP_SHOPEE_ORDERS batchinsert = new TEMP_SHOPEE_ORDERS();
//                    List<TEMP_SHOPEE_ORDERS_ITEM> batchinsertItem = new List<TEMP_SHOPEE_ORDERS_ITEM>();
//                    string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
//                    insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
//                    insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
//                    var kabKot = "3174";
//                    var prov = "31";

//                    foreach (var order in result.orders)
//                    {
//                        //insertPembeli += "('" + order.recipient_address.name + "','" + order.recipient_address.full_address + "','" + order.recipient_address.phone + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
//                        //insertPembeli += "1, 'IDR', '01', '" + order.recipient_address.full_address + "', 0, 0, 0, 0, '1', 0, 0, ";
//                        //insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.recipient_address.zipcode + "', '', '" + kabKot + "', '" + prov + "', '', '','" + connIdARF01C + "'),";

//                        string nama = order.recipient_address.name.Length > 30 ? order.recipient_address.name.Substring(0, 30) : order.recipient_address.name;

//                        insertPembeli += string.Format("('{0}','{1}','{2}','{3}',0,0,'0','01',1, 'IDR', '01', '{4}', 0, 0, 0, 0, '1', 0, 0,'FP', '{5}', '{6}', '{7}', '', '{8}', '{9}', '', '','{10}'),",
//                            ((nama ?? "").Replace("'", "`")),
//                            ((order.recipient_address.full_address ?? "").Replace("'", "`")),
//                            ((order.recipient_address.phone ?? "").Replace("'", "`")),
//                            (NAMA_CUST.Replace(',', '.')),
//                            ((order.recipient_address.full_address ?? "").Replace("'", "`")),
//                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
//                            (username),
//                            ((order.recipient_address.zipcode ?? "").Replace("'", "`")),
//                            kabKot,
//                            prov,
//                            connIdARF01C
//                            );
//                    }
//                    insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
//                    EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);

//                    foreach (var order in result.orders)
//                    {
//                        try
//                        {
//                            ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_SHOPEE_ORDERS");
//                            ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_SHOPEE_ORDERS_ITEM");
//                            batchinsertItem = new List<TEMP_SHOPEE_ORDERS_ITEM>();
//                            TEMP_SHOPEE_ORDERS newOrder = new TEMP_SHOPEE_ORDERS()
//                            {
//                                actual_shipping_cost = order.actual_shipping_cost,
//                                buyer_username = order.buyer_username,
//                                cod = order.cod,
//                                country = order.country,
//                                create_time = DateTimeOffset.FromUnixTimeSeconds(order.create_time).UtcDateTime,
//                                currency = order.currency,
//                                days_to_ship = order.days_to_ship,
//                                dropshipper = order.dropshipper,
//                                escrow_amount = order.escrow_amount,
//                                estimated_shipping_fee = order.estimated_shipping_fee,
//                                goods_to_declare = order.goods_to_declare,
//                                message_to_seller = order.message_to_seller,
//                                note = order.note,
//                                note_update_time = DateTimeOffset.FromUnixTimeSeconds(order.note_update_time).UtcDateTime,
//                                ordersn = order.ordersn,
//                                order_status = order.order_status,
//                                payment_method = order.payment_method,
//                                pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime,
//                                Recipient_Address_country = order.recipient_address.country,
//                                Recipient_Address_state = order.recipient_address.state,
//                                Recipient_Address_city = order.recipient_address.city,
//                                Recipient_Address_town = order.recipient_address.town,
//                                Recipient_Address_district = order.recipient_address.district,
//                                Recipient_Address_full_address = order.recipient_address.full_address,
//                                Recipient_Address_name = order.recipient_address.name,
//                                Recipient_Address_phone = order.recipient_address.phone,
//                                Recipient_Address_zipcode = order.recipient_address.zipcode,
//                                service_code = order.service_code,
//                                shipping_carrier = order.shipping_carrier,
//                                total_amount = order.total_amount,
//                                tracking_no = order.tracking_no,
//                                update_time = DateTimeOffset.FromUnixTimeSeconds(order.update_time).UtcDateTime,
//                                CONN_ID = connID,
//                                CUST = CUST,
//                                NAMA_CUST = NAMA_CUST
//                            };
//                            foreach (var item in order.items)
//                            {
//                                TEMP_SHOPEE_ORDERS_ITEM newOrderItem = new TEMP_SHOPEE_ORDERS_ITEM()
//                                {
//                                    ordersn = order.ordersn,
//                                    is_wholesale = item.is_wholesale,
//                                    item_id = item.item_id,
//                                    item_name = item.item_name,
//                                    item_sku = item.item_sku,
//                                    variation_discounted_price = item.variation_discounted_price,
//                                    variation_id = item.variation_id,
//                                    variation_name = item.variation_name,
//                                    variation_original_price = item.variation_original_price,
//                                    variation_quantity_purchased = item.variation_quantity_purchased,
//                                    variation_sku = item.variation_sku,
//                                    weight = item.weight,
//                                    pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime,
//                                    CONN_ID = connID,
//                                    CUST = CUST,
//                                    NAMA_CUST = NAMA_CUST
//                                };
//                                batchinsertItem.Add(newOrderItem);
//                            }
//                            batchinsert = (newOrder);

//                            ErasoftDbContext.TEMP_SHOPEE_ORDERS.Add(batchinsert);
//                            ErasoftDbContext.TEMP_SHOPEE_ORDERS_ITEM.AddRange(batchinsertItem);
//                            ErasoftDbContext.SaveChanges();
//                            using (SqlCommand CommandSQL = new SqlCommand())
//                            {
//                                //call sp to insert buyer data
//                                CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
//                                CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIdARF01C;

//                                EDB.ExecuteSQL("Con", "MoveARF01CFromTempTable", CommandSQL);
//                            };
//                            using (SqlCommand CommandSQL = new SqlCommand())
//                            {
//                                CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
//                                CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connID;
//                                CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
//                                CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//                                CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
//                                CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
//                                CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
//                                CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
//                                CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
//                                CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 1;
//                                CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
//                                CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
//                                CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

//                                EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
//                            }
//                        }
//                        catch (Exception ex3)
//                        {

//                        }
//                    }
//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                }
//            }
//            return ret;
//        }
//        public async Task<ShopeeGetParameterForInitLogisticResult> GetParameterForInitLogistic(ShopeeAPIData iden, string ordersn)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string urll = "https://partner.shopeemobile.com/api/v1/logistics/init_parameter/get";
//            if (!string.IsNullOrEmpty(iden.token))
//            {
//                MOPartnerID = MOPartnerIDV2;
//                MOPartnerKey = MOPartnerKeyV2;
//                urll = shopeeV2Url + "/api/v1/logistics/init_parameter/get";
//            }
//            ShopeeGetParameterForInitLogisticResult ret = null;
//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            //string urll = "https://partner.shopeemobile.com/api/v1/logistics/init_parameter/get";

//            ShopeeGetParameterForInitLogisticData HttpBody = new ShopeeGetParameterForInitLogisticData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                ordersn = ordersn
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }

//            }
//            catch (Exception ex)
//            {

//            }

//            if (responseFromServer != "")
//            {
//                try
//                {
//                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetParameterForInitLogisticResult)) as ShopeeGetParameterForInitLogisticResult;
//                    ret = result;
//                }
//                catch (Exception ex2)
//                {

//                }
//            }
//            return ret;
//        }
//        public async Task<ShopeeGetParameterForInitLogisticResult> GetLogisticInfo(ShopeeAPIData iden, string ordersn)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            ShopeeGetParameterForInitLogisticResult ret = null;
//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            string urll = "https://partner.shopeemobile.com/api/v1/logistics/init_info/get";

//            ShopeeGetParameterForInitLogisticData HttpBody = new ShopeeGetParameterForInitLogisticData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                ordersn = ordersn
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }

//            }
//            catch (Exception ex)
//            {

//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetParameterForInitLogisticResult)) as ShopeeGetParameterForInitLogisticResult;
//                    ret = result;
//                }
//                catch (Exception ex2)
//                {

//                }
//            }
//            return ret;
//        }
//        public async Task<string> InitLogisticDropOff(ShopeeAPIData iden, string ordersn, ShopeeInitLogisticDropOffDetailData data, int recnum, string dBranch, string dSender, string dTrackNo)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
//                REQUEST_ACTION = "Update No Resi",
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = ordersn,
//                REQUEST_ATTRIBUTE_2 = dTrackNo,
//                REQUEST_ATTRIBUTE_3 = "dropoff",
//                REQUEST_ATTRIBUTE_4 = dSender + "[;]" + dBranch,
//                REQUEST_STATUS = "Pending",
//            };

//            string urll = "https://partner.shopeemobile.com/api/v1/logistics/init";
//            ShopeeInitLogisticDropOffData HttpBody = new ShopeeInitLogisticDropOffData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                ordersn = ordersn,
//                dropoff = data
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);
//            string responseFromServer = "";

//            //var client = new HttpClient();
//            //client.DefaultRequestHeaders.Add("Authorization", signature);
//            //var content = new FormUrlEncodedContent(ToKeyValue(HttpBody));

//            //HttpResponseMessage clientResponse = await client.PostAsync(
//            //    urll, content);

//            //using (HttpContent responseContent = clientResponse.Content)
//            //{
//            //    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
//            //    {
//            //        responseFromServer = await reader.ReadToEndAsync();
//            //    }
//            //};

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeInitLogisticResult)) as ShopeeInitLogisticResult;
//                    if ((result.error == null ? "" : result.error) == "")
//                    {
//                        if (!string.IsNullOrWhiteSpace(result.tracking_no) || !string.IsNullOrWhiteSpace(result.tracking_number))
//                        {
//                            var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.RecNum == recnum);
//                            if (pesananInDb != null)
//                            {
//                                if (dTrackNo == "")
//                                {
//                                    dTrackNo = ((result.tracking_no == null ? "" : result.tracking_no) == "") ? (result.tracking_number) : result.tracking_no;
//                                }
//                                string nilaiTRACKING_SHIPMENT = "D[;]" + dBranch + "[;]" + dSender + "[;]" + dTrackNo;
//                                pesananInDb.TRACKING_SHIPMENT = nilaiTRACKING_SHIPMENT;
//                                ErasoftDbContext.SaveChanges();
//                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                            }
//                        }
//                    }
//                    else
//                    {
//                        currentLog.REQUEST_EXCEPTION = result.msg;
//                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
//                    }
//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                }
//            }
//            return ret;
//        }
//        public async Task<string> InitLogisticNonIntegrated(ShopeeAPIData iden, string ordersn, ShopeeInitLogisticNotIntegratedDetailData data, int recnum, string savedParam)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
//                REQUEST_ACTION = "Update No Resi",
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = ordersn,
//                REQUEST_ATTRIBUTE_2 = "",
//                REQUEST_ATTRIBUTE_3 = "NonIntegrated",
//                REQUEST_ATTRIBUTE_4 = savedParam,
//                REQUEST_STATUS = "Pending",
//            };

//            string urll = "https://partner.shopeemobile.com/api/v1/logistics/init";
//            ShopeeInitLogisticNonIntegratedData HttpBody = new ShopeeInitLogisticNonIntegratedData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                ordersn = ordersn,
//                non_integrated = data
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeInitLogisticResult)) as ShopeeInitLogisticResult;
//                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                }
//            }
//            return ret;
//        }
//        public async Task<string> InitLogisticPickup(ShopeeAPIData iden, string ordersn, ShopeeInitLogisticPickupDetailData data, int recnum, string savedParam)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
//                REQUEST_ACTION = "Update No Resi",
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = ordersn,
//                REQUEST_ATTRIBUTE_2 = "",
//                REQUEST_ATTRIBUTE_3 = "Pickup",
//                REQUEST_ATTRIBUTE_4 = savedParam,
//                REQUEST_STATUS = "Pending",
//            };

//            string urll = "https://partner.shopeemobile.com/api/v1/logistics/init";
//            ShopeeInitLogisticPickupData HttpBody = new ShopeeInitLogisticPickupData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                ordersn = ordersn,
//                pickup = data
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeInitLogisticResult)) as ShopeeInitLogisticResult;
//                    if (result.error == null)
//                    {
//                        if (!string.IsNullOrWhiteSpace(result.tracking_no) || !string.IsNullOrWhiteSpace(result.tracking_number))
//                        {
//                            var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.RecNum == recnum);
//                            if (pesananInDb != null)
//                            {
//                                pesananInDb.TRACKING_SHIPMENT = savedParam;
//                                ErasoftDbContext.SaveChanges();
//                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                            }
//                        }
//                    }
//                    else
//                    {
//                        currentLog.REQUEST_EXCEPTION = result.msg;
//                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
//                    }
//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                }
//            }
//            return ret;
//        }

//        public async Task<string> UpdateStock(ShopeeAPIData iden, string brg_mp, int qty)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
//                REQUEST_ACTION = "Update QOH",
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
//                REQUEST_STATUS = "Pending",
//            };

//            string urll = "https://partner.shopeemobile.com/api/v1/items/update_stock";
//            string[] brg_mp_split = brg_mp.Split(';');
//            ShopeeUpdateStockData HttpBody = new ShopeeUpdateStockData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                item_id = Convert.ToInt64(brg_mp_split[0]),
//                stock = qty
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                }
//            }
//            return ret;
//        }
//        public async Task<string> UpdateVariationStock(ShopeeAPIData iden, string brg_mp, int qty)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
//                REQUEST_ACTION = "Update QOH",
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
//                REQUEST_STATUS = "Pending",
//            };

//            string urll = "https://partner.shopeemobile.com/api/v1/items/update_variation_stock";
//            string[] brg_mp_split = brg_mp.Split(';');
//            ShopeeUpdateVariationStockData HttpBody = new ShopeeUpdateVariationStockData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                item_id = Convert.ToInt64(brg_mp_split[0]),
//                variation_id = Convert.ToInt64(brg_mp_split[1]),
//                stock = qty
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                }
//            }
//            return ret;
//        }

//        public async Task<ShopeeGetLogisticsResult> GetLogistics(ShopeeAPIData iden)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            ShopeeGetLogisticsResult ret = null;

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            string urll = "https://partner.shopeemobile.com/api/v1/logistics/channel/get";

//            ShopeeGetLogisticsData HttpBody = new ShopeeGetLogisticsData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetLogisticsResult)) as ShopeeGetLogisticsResult;
//                    ret = result;
//                }
//                catch (Exception ex2)
//                {
//                }
//            }
//            return ret;
//        }
//        public async Task<string> AcceptBuyerCancellation(ShopeeAPIData iden, string ordersn)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
//                REQUEST_ACTION = "Accept Buyer Cancel", //ganti
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
//                REQUEST_STATUS = "Pending",
//            };

//            string urll = "https://partner.shopeemobile.com/api/v1/orders/buyer_cancellation/accept";

//            ShopeeAcceptBuyerCancelOrderData HttpBody = new ShopeeAcceptBuyerCancelOrderData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                ordersn = ordersn
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);



//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                }
//            }
//            return ret;
//        }
//        public async Task<string> CancelOrder(ShopeeAPIData iden, string ordersn)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
//                REQUEST_ACTION = "Cancel Order",
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
//                REQUEST_STATUS = "Pending",
//            };

//            string urll = "https://partner.shopeemobile.com/api/v1/orders/cancel";

//            ShopeeCancelOrderData HttpBody = new ShopeeCancelOrderData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                ordersn = ordersn,
//                cancel_reason = "CUSTOMER_REQUEST"
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeCancelOrderResult)) as ShopeeCancelOrderResult;
//                    if (result.error != null)
//                    {
//                        if (result.error != "")
//                        {
//                            await AcceptBuyerCancellation(iden, ordersn);
//                        }
//                    }
//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                }
//            }
//            return ret;
//        }
//        public async Task<string> CreateProduct(ShopeeAPIData iden, string brg, string cust, List<ShopeeLogisticsClass> logistics)
//        {
//            string ret = "";
//            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == brg.ToUpper()).FirstOrDefault();
//            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == cust.ToUpper()).FirstOrDefault();
//            if (brgInDb == null || marketplace == null)
//                return "invalid passing data";
//            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == brg.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
//            if (detailBrg == null)
//                return "invalid passing data";

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
//                REQUEST_ACTION = "Create Product",
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = brg,
//                REQUEST_ATTRIBUTE_2 = brgInDb.NAMA,
//                REQUEST_STATUS = "Pending",
//            };

//            string urll = "https://partner.shopeemobile.com/api/v1/item/add";

//            //add by calvin 21 desember 2018, default nya semua logistic enabled
//            var ShopeeGetLogisticsResult = await GetLogistics(iden);

//            foreach (var log in ShopeeGetLogisticsResult.logistics.Where(p => p.enabled == true && p.fee_type.ToUpper() != "CUSTOM_PRICE" && p.fee_type.ToUpper() != "SIZE_SELECTION"))
//            {
//                bool lolosValidLogistic = true;
//                if (log.weight_limits != null)
//                {
//                    if (log.weight_limits.item_max_weight < (brgInDb.BERAT / 1000))
//                    {
//                        lolosValidLogistic = false;
//                    }
//                    if (log.weight_limits.item_min_weight > (brgInDb.BERAT / 1000))
//                    {
//                        lolosValidLogistic = false;
//                    }
//                }

//                if (log.item_max_dimension != null)
//                {
//                    if (log.item_max_dimension.length > 0)
//                    {
//                        if (log.item_max_dimension.length < (Convert.ToInt32(brgInDb.PANJANG) == 0 ? 1 : Convert.ToInt32(brgInDb.PANJANG)))
//                        {
//                            lolosValidLogistic = false;
//                        }
//                    }
//                    if (log.item_max_dimension.height > 0)
//                    {
//                        if (log.item_max_dimension.height < (Convert.ToInt32(brgInDb.TINGGI) == 0 ? 1 : Convert.ToInt32(brgInDb.TINGGI)))
//                        {
//                            lolosValidLogistic = false;
//                        }
//                    }
//                    if (log.item_max_dimension.width > 0)
//                    {
//                        if (log.item_max_dimension.width < (Convert.ToInt32(brgInDb.LEBAR) == 0 ? 1 : Convert.ToInt32(brgInDb.LEBAR)))
//                        {
//                            lolosValidLogistic = false;
//                        }
//                    }
//                }

//                if (lolosValidLogistic)
//                {
//                    logistics.Add(new ShopeeLogisticsClass()
//                    {
//                        enabled = log.enabled,
//                        is_free = false,
//                        logistic_id = log.logistic_id,
//                    });
//                }
//            }
//            //end add by calvin 21 desember 2018, default nya semua logistic enabled

//            ShopeeProductData HttpBody = new ShopeeProductData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                item_sku = brg,
//                category_id = Convert.ToInt64(detailBrg.CATEGORY_CODE),
//                condition = "NEW",
//                name = (brgInDb.NAMA + " " + brgInDb.NAMA2).Trim().Replace("’", "`"),
//                description = brgInDb.Deskripsi.Replace("’", "`"),
//                package_height = Convert.ToInt32(brgInDb.TINGGI) == 0 ? 1 : Convert.ToInt32(brgInDb.TINGGI),
//                package_length = Convert.ToInt32(brgInDb.PANJANG) == 0 ? 1 : Convert.ToInt32(brgInDb.PANJANG),
//                package_width = Convert.ToInt32(brgInDb.LEBAR) == 0 ? 1 : Convert.ToInt32(brgInDb.LEBAR),
//                weight = brgInDb.BERAT / 1000,
//                price = detailBrg.HJUAL,
//                stock = 1,//create product min stock = 1
//                images = new List<ShopeeImageClass>(),
//                attributes = new List<ShopeeAttributeClass>(),
//                variations = new List<ShopeeVariationClass>(),
//                logistics = logistics
//            };

//            //add by calvin 10 mei 2019
//            HttpBody.description = new StokControllerJob().RemoveSpecialCharacters(HttpBody.description);
//            //end add by calvin 10 mei 2019

//            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
//            HttpBody.description = HttpBody.description.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
//            HttpBody.description = HttpBody.description.Replace("<li>", "- ").Replace("</li>", "\r\n");
//            HttpBody.description = HttpBody.description.Replace("<ul>", "").Replace("</ul>", "\r\n");
//            HttpBody.description = HttpBody.description.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
//            HttpBody.description = HttpBody.description.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
//            HttpBody.description = HttpBody.description.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");

//            HttpBody.description = HttpBody.description.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
//            HttpBody.description = HttpBody.description.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
//            HttpBody.description = HttpBody.description.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
//            HttpBody.description = HttpBody.description.Replace("<p>", "\r\n").Replace("</p>", "\r\n");

//            HttpBody.description = System.Text.RegularExpressions.Regex.Replace(HttpBody.description, "<.*?>", String.Empty);
//            //end add by nurul 20/1/2020, handle <p> dan enter double di shopee

//            //add by calvin 1 mei 2019
//            var qty_stock = new StokControllerJob(DatabasePathErasoft, username).GetQOHSTF08A(brg, "ALL");
//            if (qty_stock > 0)
//            {
//                HttpBody.stock = Convert.ToInt32(qty_stock);
//            }
//            //end add by calvin 1 mei 2019

//            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
//                HttpBody.images.Add(new ShopeeImageClass { url = brgInDb.LINK_GAMBAR_1 });
//            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
//                HttpBody.images.Add(new ShopeeImageClass { url = brgInDb.LINK_GAMBAR_2 });
//            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
//                HttpBody.images.Add(new ShopeeImageClass { url = brgInDb.LINK_GAMBAR_3 });
//            if (brgInDb.TYPE == "4")
//            {
//                var ListVariant = ErasoftDbContext.STF02.Where(p => p.PART == brg).ToList();
//                var ListSettingVariasi = ErasoftDbContext.STF02I.Where(p => p.BRG == brg).ToList();
//                //add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
//                List<string> byteGambarUploaded = new List<string>();
//                //end add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
//                foreach (var item in ListVariant)
//                {
//                    List<string> Duplikat = HttpBody.variations.Select(p => p.name).ToList();
//                    //add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
//                    if (!byteGambarUploaded.Contains(item.Sort5))
//                    {
//                        byteGambarUploaded.Add(item.Sort5);
//                        if (!string.IsNullOrEmpty(item.LINK_GAMBAR_1))
//                            HttpBody.images.Add(new ShopeeImageClass { url = item.LINK_GAMBAR_1 });
//                    }
//                    //end add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
//                }
//            }
//            try
//            {
//                for (int i = 1; i <= 30; i++)
//                {
//                    string attribute_id = Convert.ToString(detailBrg["ACODE_" + i.ToString()]);
//                    string value = Convert.ToString(detailBrg["AVALUE_" + i.ToString()]);
//                    if (!string.IsNullOrWhiteSpace(attribute_id))
//                    {

//                        HttpBody.attributes.Add(new ShopeeAttributeClass
//                        {
//                            attributes_id = Convert.ToInt64(attribute_id),
//                            value = value.Trim()
//                        });
//                    }
//                }

//            }
//            catch (Exception ex)
//            {

//            }

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            string responseFromServer = "";
//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";

//            try
//            {
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }

//            //try
//            //{
//            //    var client = new HttpClient();

//            //    client.DefaultRequestHeaders.Add("Authorization", (signature));
//            //    var content = new StringContent(myData, Encoding.UTF8, "application/json");
//            //    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
//            //    HttpResponseMessage clientResponse = await client.PostAsync(
//            //        urll, content);

//            //    using (HttpContent responseContent = clientResponse.Content)
//            //    {
//            //        using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
//            //        {
//            //            responseFromServer = await reader.ReadToEndAsync();
//            //        }
//            //    };
//            //}
//            //catch (Exception ex)
//            //{
//            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//            //}


//            if (responseFromServer != null)
//            {
//                try
//                {
//                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeCreateProdResponse)) as ShopeeCreateProdResponse;
//                    if (resServer != null)
//                    {
//                        if (string.IsNullOrEmpty(resServer.error))
//                        {
//                            var item = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == brg.ToUpper() && b.IDMARKET == marketplace.RecNum).SingleOrDefault();
//                            if (item != null)
//                            {
//                                item.BRG_MP = Convert.ToString(resServer.item_id) + ";0";
//                                ErasoftDbContext.SaveChanges();

//                                if (brgInDb.TYPE == "4")
//                                {
//                                    await InitTierVariation(iden, brgInDb, resServer.item_id, marketplace);
//                                }

//                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                            }
//                            else
//                            {
//                                currentLog.REQUEST_EXCEPTION = "item not found";
//                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
//                            }
//                        }
//                        else
//                        {
//                            currentLog.REQUEST_RESULT = resServer.msg;
//                            currentLog.REQUEST_EXCEPTION = resServer.error + ";" + resServer.msg;
//                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
//                        }
//                    }

//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                }
//            }
//            return ret;
//        }
//        public async Task<string> GetVariation(ShopeeAPIData iden, STF02 brgInDb, long item_id, ARF01 marketplace, Dictionary<string, int> mapSTF02HRecnum_IndexVariasi, List<ShopeeVariation> MOVariation, List<ShopeeTierVariation> tier_variation)
//        {
//            var MOVariationNew = MOVariation.ToList();

//            string ret = "";
//            string brg = brgInDb.BRG;

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            string urll = "https://partner.shopeemobile.com/api/v1/item/tier_var/get";

//            ShopeeGetVariation HttpBody = new ShopeeGetVariation
//            {
//                partner_id = MOPartnerID,
//                item_id = item_id,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }


//            if (responseFromServer != null)
//            {
//                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(GetVariationResult)) as GetVariationResult;
//                var new_tier_variation = new List<ShopeeUpdateVariation>();

//                foreach (var variasi in resServer.variations)
//                {
//                    string key_map_tier_index_recnum = "";
//                    foreach (var indexes in variasi.tier_index)
//                    {
//                        key_map_tier_index_recnum = key_map_tier_index_recnum + Convert.ToString(indexes) + ";";
//                    }
//                    int recnum_stf02h_var = mapSTF02HRecnum_IndexVariasi.Where(p => p.Key == key_map_tier_index_recnum).Select(p => p.Value).SingleOrDefault();

//                    //var var_item = ErasoftDbContext.STF02H.Where(b => b.RecNum == recnum_stf02h_var).SingleOrDefault();
//                    //var_item.BRG_MP = Convert.ToString(resServer.item_id) + ";" + Convert.ToString(variasi.variation_id);
//                    //ErasoftDbContext.SaveChanges();
//                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(resServer.item_id) + ";" + Convert.ToString(variasi.variation_id) + "' WHERE RECNUM = '" + Convert.ToString(recnum_stf02h_var) + "' AND ISNULL(BRG_MP,'') = '' ");
//                    //tes remark by calvin 16 mei
//                    mapSTF02HRecnum_IndexVariasi.Remove(key_map_tier_index_recnum);
//                    foreach (var item in MOVariation)
//                    {
//                        var isiTier_Index = "";
//                        foreach (var indexes in item.tier_index)
//                        {
//                            isiTier_Index = isiTier_Index + Convert.ToString(indexes) + ";";
//                        }
//                        if (isiTier_Index == key_map_tier_index_recnum)
//                        {
//                            MOVariationNew.Remove(item);
//                            new_tier_variation.Add(new ShopeeUpdateVariation()
//                            {
//                                price = item.price,
//                                stock = item.stock,
//                                tier_index = item.tier_index,
//                                variation_sku = item.variation_sku,
//                                variation_id = variasi.variation_id
//                            });
//                        }
//                    }
//                    //end tes remark by calvin 16 mei
//                }
//                if (MOVariationNew.Count() > 0)
//                {
//                    //foreach (var variasi in mapSTF02HRecnum_IndexVariasi)
//                    //{
//                    //    await AddVariation(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariation, MOVariationNew);
//                    //}
//                    //await UpdateTierVariationIndex(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariationNew, tier_variation, new_tier_variation);
//                    await UpdateTierVariationList(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariationNew, tier_variation, new_tier_variation, MOVariation);
//                }
//            }

//            return ret;
//        }
//        public async Task<string> UpdateTierVariationList(ShopeeAPIData iden, STF02 brgInDb, long item_id, ARF01 marketplace, Dictionary<string, int> mapSTF02HRecnum_IndexVariasi, List<ShopeeVariation> MOVariationNew, List<ShopeeTierVariation> tier_variation, List<ShopeeUpdateVariation> new_tier_variation, List<ShopeeVariation> MOVariation)
//        {
//            //Use this api to update tier-variation list or upload variation image of a tier-variation item
//            string ret = "";
//            string brg = brgInDb.BRG;

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            string urll = "https://partner.shopeemobile.com/api/v1/item/tier_var/update_list";

//            ShopeeUpdateTierVariationList HttpBody = new ShopeeUpdateTierVariationList
//            {
//                partner_id = MOPartnerID,
//                item_id = item_id,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                tier_variation = tier_variation.ToArray(),
//                timestamp = seconds,
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }


//            if (responseFromServer != null)
//            {
//                //AddTierVariation
//                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeUpdateTierVariationResult)) as ShopeeUpdateTierVariationResult;
//                if (resServer.item_id == item_id)
//                {
//                    //foreach (var variasi in mapSTF02HRecnum_IndexVariasi)
//                    //{
//                    //    await AddVariation(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariation, MOVariationNew);
//                    //}
//                    await AddTierVariation(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariation, MOVariationNew);
//                }
//            }

//            return ret;
//        }
//        public async Task<string> AddTierVariation(ShopeeAPIData iden, STF02 brgInDb, long item_id, ARF01 marketplace, Dictionary<string, int> mapSTF02HRecnum_IndexVariasi, List<ShopeeVariation> MOVariation, List<ShopeeVariation> MOVariationNew)
//        {
//            string ret = "";
//            string brg = brgInDb.BRG;

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            string urll = "https://partner.shopeemobile.com/api/v1/item/tier_var/add";
//            ShopeeAddTierVariation HttpBody = new ShopeeAddTierVariation
//            {
//                partner_id = MOPartnerID,
//                item_id = item_id,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                variation = MOVariationNew.ToArray()
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }


//            if (responseFromServer != null)
//            {
//                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(InitTierVariationResult)) as InitTierVariationResult;
//                if (resServer.variation_id_list != null)
//                {
//                    await GetVariation(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariation, null);

//                }
//            }

//            return ret;
//        }
//        public async Task<string> UpdateTierVariationIndex(ShopeeAPIData iden, STF02 brgInDb, long item_id, ARF01 marketplace, Dictionary<string, int> mapSTF02HRecnum_IndexVariasi, List<ShopeeVariation> MOVariationNew, List<ShopeeTierVariation> tier_variation, List<ShopeeUpdateVariation> new_tier_variation)
//        {
//            List<object> variation = new List<object>();
//            string ret = "";
//            string brg = brgInDb.BRG;

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            string urll = "https://partner.shopeemobile.com/api/v1/item/tier_var/update";
//            foreach (var item in new_tier_variation)
//            {
//                variation.Add(new ShopeeUpdateVariation()
//                {
//                    price = item.price,
//                    stock = item.stock,
//                    tier_index = item.tier_index,
//                    variation_sku = item.variation_sku,
//                    variation_id = item.variation_id
//                });
//            }
//            //foreach (var item in MOVariationNew)
//            //{
//            //    variation.Add(new ShopeeVariation()
//            //    {
//            //        price = item.price,
//            //        stock = item.stock,
//            //        tier_index = item.tier_index,
//            //        variation_sku = item.variation_sku,

//            //    });
//            //}

//            ShopeeUpdateTierVariationIndex HttpBody = new ShopeeUpdateTierVariationIndex
//            {
//                partner_id = MOPartnerID,
//                item_id = item_id,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                tier_variation = tier_variation.ToArray(),
//                variation = variation.ToArray()
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }
//            return ret;
//        }
//        public async Task<string> InitTierVariation(ShopeeAPIData iden, STF02 brgInDb, long item_id, ARF01 marketplace)
//        {
//            string ret = "";
//            string brg = brgInDb.BRG;

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            //{
//            //    REQUEST_ID = seconds.ToString(),
//            //    REQUEST_ACTION = "Create Product",
//            //    REQUEST_DATETIME = milisBack,
//            //    REQUEST_ATTRIBUTE_1 = brg,
//            //    REQUEST_ATTRIBUTE_2 = brgInDb.NAMA,
//            //    REQUEST_STATUS = "Pending",
//            //};

//            string urll = "https://partner.shopeemobile.com/api/v1/item/tier_var/init";

//            ShopeeInitTierVariation HttpBody = new ShopeeInitTierVariation
//            {
//                partner_id = MOPartnerID,
//                item_id = item_id,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                tier_variation = new List<ShopeeTierVariation>().ToArray(),
//                variation = new List<ShopeeVariation>().ToArray(),
//                timestamp = seconds
//            };
//            List<ShopeeTierVariation> tier_variation = new List<ShopeeTierVariation>();
//            List<ShopeeVariation> variation = new List<ShopeeVariation>();
//            Dictionary<string, int> mapIndexVariasi1 = new Dictionary<string, int>();
//            Dictionary<string, int> mapIndexVariasi2 = new Dictionary<string, int>();
//            Dictionary<string, int> mapSTF02HRecnum_IndexVariasi = new Dictionary<string, int>();

//            if (brgInDb.TYPE == "4")//Barang Induk ( memiliki Variant )
//            {
//                var ListVariant = ErasoftDbContext.STF02.Where(p => p.PART == brg).ToList();
//                var ListSettingVariasi = ErasoftDbContext.STF02I.Where(p => p.BRG == brg).ToList();
//                var ListKodeBrgVariant = ListVariant.Select(p => p.BRG).ToList();
//                var ListStf02hVariasi = ErasoftDbContext.STF02H.Where(p => ListKodeBrgVariant.Contains(p.BRG) && p.IDMARKET == marketplace.RecNum).ToList();

//                int index_var_1 = 0;
//                int index_var_2 = 0;
//                ShopeeTierVariation tier1 = new ShopeeTierVariation();
//                ShopeeTierVariation tier2 = new ShopeeTierVariation();
//                List<string> tier1_options = new List<string>();
//                List<string> tier2_options = new List<string>();
//                foreach (var item in ListVariant.OrderBy(p => p.ID))
//                {
//                    var stf02h = ListStf02hVariasi.Where(p => p.BRG.ToUpper() == item.BRG.ToUpper() && p.IDMARKET == marketplace.RecNum).FirstOrDefault();
//                    if (stf02h != null)
//                    {
//                        string namaVariasiLV1 = "";
//                        if ((item.Sort8 == null ? "" : item.Sort8) != "")
//                        {
//                            var getNamaVar = ListSettingVariasi.Where(p => p.LEVEL_VAR == 1 && p.MARKET == "SHOPEE" && p.KODE_VAR == item.Sort8).FirstOrDefault();
//                            if (getNamaVar != null)
//                            {
//                                namaVariasiLV1 = getNamaVar.MP_JUDUL_VAR;
//                            }
//                        }

//                        string namaVariasiLV2 = "";
//                        if ((item.Sort9 == null ? "" : item.Sort9) != "")
//                        {
//                            var getNamaVar = ListSettingVariasi.Where(p => p.LEVEL_VAR == 2 && p.MARKET == "SHOPEE" && p.KODE_VAR == item.Sort9).FirstOrDefault();
//                            if (getNamaVar != null)
//                            {
//                                namaVariasiLV2 = getNamaVar.MP_JUDUL_VAR;
//                            }
//                        }

//                        List<string> DuplikatNamaVariasi = tier_variation.Select(p => p.name).ToList();
//                        if (!DuplikatNamaVariasi.Contains(namaVariasiLV1))
//                        {
//                            if (namaVariasiLV1 != "")
//                            {
//                                tier1.name = namaVariasiLV1;
//                            }
//                        }
//                        if (!DuplikatNamaVariasi.Contains(namaVariasiLV2))
//                        {
//                            if (namaVariasiLV2 != "")
//                            {
//                                tier2.name = namaVariasiLV2;
//                            }
//                        }

//                        string nama_var1 = "";
//                        if ((item.Sort8 == null ? "" : item.Sort8) != "")
//                        {
//                            var getNamaVar = ListSettingVariasi.Where(p => p.LEVEL_VAR == 1 && p.MARKET == "SHOPEE" && p.KODE_VAR == item.Sort8).FirstOrDefault();
//                            if (getNamaVar != null)
//                            {
//                                nama_var1 = getNamaVar.MP_VALUE_VAR;
//                                if (!mapIndexVariasi1.ContainsKey(nama_var1))
//                                {
//                                    mapIndexVariasi1.Add(nama_var1, index_var_1);
//                                    tier1_options.Add(nama_var1);
//                                    index_var_1++;
//                                }
//                            }
//                        }
//                        string nama_var2 = "";
//                        if ((item.Sort9 == null ? "" : item.Sort9) != "")
//                        {
//                            var getNamaVar = ListSettingVariasi.Where(p => p.LEVEL_VAR == 2 && p.MARKET == "SHOPEE" && p.KODE_VAR == item.Sort9).FirstOrDefault();
//                            if (getNamaVar != null)
//                            {
//                                nama_var2 = getNamaVar.MP_VALUE_VAR;
//                                if (!mapIndexVariasi2.ContainsKey(nama_var2))
//                                {
//                                    mapIndexVariasi2.Add(nama_var2, index_var_2);
//                                    tier2_options.Add(nama_var2);
//                                    index_var_2++;
//                                }
//                            }
//                        }
//                        int getIndexVar1 = 0;
//                        int getIndexVar2 = 0;
//                        List<int> ListTierIndex = new List<int>();
//                        if (mapIndexVariasi1.ContainsKey(nama_var1))
//                        {
//                            getIndexVar1 = mapIndexVariasi1[nama_var1];
//                            ListTierIndex.Add(getIndexVar1);
//                        }
//                        if (mapIndexVariasi2.ContainsKey(nama_var2))
//                        {
//                            getIndexVar2 = mapIndexVariasi2[nama_var2];
//                            ListTierIndex.Add(getIndexVar2);
//                        }

//                        ShopeeVariation adaVariant = new ShopeeVariation()
//                        {
//                            tier_index = ListTierIndex.ToArray(),
//                            price = (float)stf02h.HJUAL,
//                            stock = 1,//create product min stock = 1
//                            variation_sku = item.BRG
//                        };

//                        //add by calvin 1 mei 2019
//                        var qty_stock = new StokControllerJob(DatabasePathErasoft, username).GetQOHSTF08A(item.BRG, "ALL");
//                        if (qty_stock > 0)
//                        {
//                            adaVariant.stock = Convert.ToInt32(qty_stock);
//                        }
//                        //end add by calvin 1 mei 2019

//                        string key_map_tier_index_recnum = "";
//                        foreach (var indexes in ListTierIndex)
//                        {
//                            key_map_tier_index_recnum = key_map_tier_index_recnum + Convert.ToString(indexes) + ";";
//                        }
//                        mapSTF02HRecnum_IndexVariasi.Add(key_map_tier_index_recnum, stf02h.RecNum.Value);
//                        variation.Add(adaVariant);
//                    }
//                }
//                if (!string.IsNullOrWhiteSpace(tier1.name))
//                {
//                    tier1.options = tier1_options.ToArray();
//                    tier_variation.Add(tier1);
//                }
//                if (!string.IsNullOrWhiteSpace(tier2.name))
//                {
//                    tier2.options = tier2_options.ToArray();
//                    tier_variation.Add(tier2);
//                }
//            }
//            HttpBody.variation = variation.ToArray();
//            HttpBody.tier_variation = tier_variation.ToArray();

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }


//            if (responseFromServer != "")
//            {
//                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(InitTierVariationResult)) as InitTierVariationResult;
//                if (resServer.variation_id_list != null)
//                {
//                    if (resServer.variation_id_list.Count() > 0)
//                    {
//                        foreach (var variasi in resServer.variation_id_list)
//                        {
//                            string key_map_tier_index_recnum = "";
//                            foreach (var indexes in variasi.tier_index)
//                            {
//                                key_map_tier_index_recnum = key_map_tier_index_recnum + Convert.ToString(indexes) + ";";
//                            }
//                            int recnum_stf02h_var = mapSTF02HRecnum_IndexVariasi.Where(p => p.Key == key_map_tier_index_recnum).Select(p => p.Value).SingleOrDefault();
//                            //var var_item = ErasoftDbContext.STF02H.Where(b => b.RecNum == recnum_stf02h_var).SingleOrDefault();
//                            //var_item.BRG_MP = Convert.ToString(resServer.item_id) + ";" + Convert.ToString(variasi.variation_id);
//                            //ErasoftDbContext.SaveChanges();
//                            var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(resServer.item_id) + ";" + Convert.ToString(variasi.variation_id) + "' WHERE RECNUM = '" + Convert.ToString(recnum_stf02h_var) + "'");
//                        }
//                    }
//                }
//                else
//                {
//                    await GetVariation(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, variation, tier_variation);
//                }
//            }

//            return ret;
//        }
//        public async Task<string> UpdateProduct(ShopeeAPIData iden, string brg, string cust, List<ShopeeLogisticsClass> logistics)
//        {
//            string ret = "";
//            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == brg.ToUpper()).FirstOrDefault();
//            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == cust.ToUpper()).FirstOrDefault();
//            if (brgInDb == null || marketplace == null)
//                return "invalid passing data";
//            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == brg.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
//            if (detailBrg == null)
//                return "invalid passing data";

//            long item_id = 0;
//            string[] brg_mp = detailBrg.BRG_MP.Split(';');
//            if (brg_mp.Count() == 2)
//            {
//                item_id = Convert.ToInt64(brg_mp[0]);
//            }

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
//                REQUEST_ACTION = "Update Product",
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
//                REQUEST_STATUS = "Pending",
//            };

//            string urll = "https://partner.shopeemobile.com/api/v1/item/update";

//            //add by calvin 21 desember 2018, default nya semua logistic enabled
//            var ShopeeGetLogisticsResult = await GetLogistics(iden);

//            foreach (var log in ShopeeGetLogisticsResult.logistics.Where(p => p.enabled == true && p.fee_type.ToUpper() != "CUSTOM_PRICE" && p.fee_type.ToUpper() != "SIZE_SELECTION"))
//            {
//                logistics.Add(new ShopeeLogisticsClass()
//                {
//                    enabled = log.enabled,
//                    is_free = false,
//                    logistic_id = log.logistic_id,
//                });
//            }
//            //end add by calvin 21 desember 2018, default nya semua logistic enabled

//            ShopeeUpdateProductData HttpBody = new ShopeeUpdateProductData
//            {
//                item_id = item_id,
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                item_sku = brg,
//                category_id = Convert.ToInt64(detailBrg.CATEGORY_CODE),
//                condition = "NEW",
//                name = (brgInDb.NAMA + " " + brgInDb.NAMA2).Trim().Replace("’", "`"),
//                description = brgInDb.Deskripsi.Replace("’", "`"),
//                package_height = Convert.ToInt32(brgInDb.TINGGI) == 0 ? 1 : Convert.ToInt32(brgInDb.TINGGI),
//                package_length = Convert.ToInt32(brgInDb.PANJANG) == 0 ? 1 : Convert.ToInt32(brgInDb.PANJANG),
//                package_width = Convert.ToInt32(brgInDb.LEBAR) == 0 ? 1 : Convert.ToInt32(brgInDb.LEBAR),
//                weight = brgInDb.BERAT / 1000,
//                attributes = new List<ShopeeAttributeClass>(),
//                logistics = logistics
//            };

//            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
//            HttpBody.description = new StokControllerJob().RemoveSpecialCharacters(HttpBody.description);

//            HttpBody.description = HttpBody.description.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
//            HttpBody.description = HttpBody.description.Replace("<li>", "- ").Replace("</li>", "\r\n");
//            HttpBody.description = HttpBody.description.Replace("<ul>", "").Replace("</ul>", "\r\n");
//            HttpBody.description = HttpBody.description.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
//            HttpBody.description = HttpBody.description.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
//            HttpBody.description = HttpBody.description.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");

//            HttpBody.description = HttpBody.description.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
//            HttpBody.description = HttpBody.description.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
//            HttpBody.description = HttpBody.description.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
//            HttpBody.description = HttpBody.description.Replace("<p>", "\r\n").Replace("</p>", "\r\n");


//            HttpBody.description = System.Text.RegularExpressions.Regex.Replace(HttpBody.description, "<.*?>", String.Empty);
//            //end add by nurul 20/1/2020, handle <p> dan enter double di shopee

//            try
//            {
//                string sSQL = "SELECT * FROM (";
//                for (int i = 1; i <= 30; i++)
//                {
//                    sSQL += "SELECT A.ACODE_" + i.ToString() + " AS CATEGORY_CODE,A.ANAME_" + i.ToString() + " AS CATEGORY_NAME,B.ATYPE_" + i.ToString() + " AS CATEGORY_TYPE,A.AVALUE_" + i.ToString() + " AS VALUE FROM STF02H A INNER JOIN MO.DBO.ATTRIBUTE_SHOPEE B ON A.CATEGORY_CODE = B.CATEGORY_CODE WHERE A.BRG='" + brg + "' AND A.IDMARKET = '" + marketplace.RecNum + "' " + System.Environment.NewLine;
//                    if (i < 30)
//                    {
//                        sSQL += "UNION ALL " + System.Environment.NewLine;
//                    }
//                }

//                DataSet dsFeature = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' ");

//                for (int i = 0; i < dsFeature.Tables[0].Rows.Count; i++)
//                {
//                    HttpBody.attributes.Add(new ShopeeAttributeClass
//                    {
//                        attributes_id = Convert.ToInt64(dsFeature.Tables[0].Rows[i]["CATEGORY_CODE"]),
//                        value = Convert.ToString(dsFeature.Tables[0].Rows[i]["VALUE"]).Trim()
//                    });
//                }

//            }
//            catch (Exception ex)
//            {

//            }


//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }

//            if (responseFromServer != "")
//            {
//                try
//                {
//                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                    //var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeCreateProdResponse)) as ShopeeCreateProdResponse;
//                    //if (resServer != null)
//                    //{
//                    //    if (string.IsNullOrEmpty(resServer.error))
//                    //    {
//                    //        var item = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == brg.ToUpper() && b.IDMARKET == marketplace.RecNum).SingleOrDefault();
//                    //        if (item != null)
//                    //        {
//                    //            item.BRG_MP = resServer.item_id.ToString() + ";0";
//                    //            ErasoftDbContext.SaveChanges();
//                    //            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                    //        }
//                    //        else
//                    //        {
//                    //            currentLog.REQUEST_EXCEPTION = "item not found";
//                    //            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
//                    //        }
//                    //    }
//                    //    else
//                    //    {
//                    //        currentLog.REQUEST_EXCEPTION = resServer.error + ";" + resServer.msg;
//                    //        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
//                    //    }
//                    //}

//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                }
//            }
//            return ret;
//        }
//        public async Task<string> UpdatePrice(ShopeeAPIData iden, string brg_mp, float price)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";
//            string urll = "https://partner.shopeemobile.com/api/v1/items/update_price";
//            if (!string.IsNullOrEmpty(iden.token))
//            {
//                MOPartnerID = MOPartnerIDV2;
//                MOPartnerKey = MOPartnerKeyV2;
//                urll = shopeeV2Url + "/api/v1/items/update_price";
//            }

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
//                REQUEST_ACTION = "Update Price", //ganti
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
//                REQUEST_STATUS = "Pending",
//            };

//            //string urll = "https://partner.shopeemobile.com/api/v1/items/update_price";

//            string[] brg_mp_split = brg_mp.Split(';');
//            ShopeeUpdatePriceData HttpBody = new ShopeeUpdatePriceData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                item_id = Convert.ToInt64(brg_mp_split[0]),
//                price = price
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }

//            if (responseFromServer != "")
//            {
//                try
//                {
//                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                }
//            }
//            return ret;
//        }
//        public async Task<string> UpdateVariationPrice(ShopeeAPIData iden, string brg_mp, float price)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";
//            string urll = "https://partner.shopeemobile.com/api/v1/items/update_variation_price";
//            if (!string.IsNullOrEmpty(iden.token))
//            {
//                MOPartnerID = MOPartnerIDV2;
//                MOPartnerKey = MOPartnerKeyV2;
//                urll = shopeeV2Url + "/api/v1/items/update_variation_price";
//            }

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
//                REQUEST_ACTION = "Update Price",
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
//                REQUEST_STATUS = "Pending",
//            };

//            //string urll = "https://partner.shopeemobile.com/api/v1/items/update_variation_price";

//            string[] brg_mp_split = brg_mp.Split(';');
//            ShopeeUpdateVariantionPriceData HttpBody = new ShopeeUpdateVariantionPriceData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                item_id = Convert.ToInt64(brg_mp_split[0]),
//                variation_id = Convert.ToInt64(brg_mp_split[1]),
//                price = price
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }

//            if (responseFromServer != "")
//            {
//                try
//                {
//                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                }
//            }
//            return ret;
//        }


//        public async Task<string> UpdateImage(ShopeeAPIData iden, string brg, string brg_mp)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";
//            string urll = "https://partner.shopeemobile.com/api/v1/item/img/update";
//            if (!string.IsNullOrEmpty(iden.token))
//            {
//                MOPartnerID = MOPartnerIDV2;
//                MOPartnerKey = MOPartnerKeyV2;
//                urll = shopeeV2Url + "/api/v1/item/img/update";
//            }

//            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == brg.ToUpper()).FirstOrDefault();
//            if (brgInDb == null)
//                return "invalid passing data";
//            //var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == brg.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
//            //if (detailBrg == null)
//            //    return "invalid passing data";

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
//                REQUEST_ACTION = "Update Product Image",
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
//                REQUEST_STATUS = "Pending",
//            };

//            //string urll = "https://partner.shopeemobile.com/api/v1/item/img/update";

//            List<string> imagess = new List<string>();

//            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
//                imagess.Add(brgInDb.LINK_GAMBAR_1);
//            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
//                imagess.Add(brgInDb.LINK_GAMBAR_2);
//            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
//                imagess.Add(brgInDb.LINK_GAMBAR_3);
//            //add 25 jun 2020, ada 5 gambar
//            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_4))
//                imagess.Add(brgInDb.LINK_GAMBAR_4);
//            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_5))
//                imagess.Add(brgInDb.LINK_GAMBAR_5);
//            //end add 25 jun 2020, ada 5 gambar

//            string[] brg_mp_split = brg_mp.Split(';');
//            ShopeeUpdateImageData HttpBody = new ShopeeUpdateImageData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                item_id = Convert.ToInt64(brg_mp_split[0]),
//                images = imagess.ToArray()
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                }
//            }
//            return ret;
//        }

//        public async Task<string> AddDiscount(ShopeeAPIData iden, int recNumPromosi)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";
//            string urll = "https://partner.shopeemobile.com/api/v1/discount/add";
//            if (!string.IsNullOrEmpty(iden.token))
//            {
//                MOPartnerID = MOPartnerIDV2;
//                MOPartnerKey = MOPartnerKeyV2;
//                urll = shopeeV2Url + "/api/v1/discount/add";
//            }
//            double promoPrice;
//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.merchant_code).FirstOrDefault();
//            var varPromo = ErasoftDbContext.PROMOSI.Where(p => p.RecNum == recNumPromosi).FirstOrDefault();
//            var varPromoDetail = ErasoftDbContext.DETAILPROMOSI.Where(p => p.RecNumPromosi == recNumPromosi).ToList();
//            //long starttime = ((DateTimeOffset)varPromo.TGL_MULAI).ToUnixTimeSeconds();
//            //long endtime = ((DateTimeOffset)varPromo.TGL_AKHIR).ToUnixTimeSeconds();
//            long starttime = ((DateTimeOffset)varPromo.TGL_MULAI.Value.AddHours(-7)).ToUnixTimeSeconds();
//            long endtime = ((DateTimeOffset)varPromo.TGL_AKHIR.Value.AddHours(-7)).ToUnixTimeSeconds();

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
//                REQUEST_ACTION = "Add Discount",
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
//                REQUEST_ATTRIBUTE_2 = recNumPromosi.ToString(),
//                REQUEST_STATUS = "Pending",
//            };

//            //string urll = "https://partner.shopeemobile.com/api/v1/discount/add";

//            ShopeeAddDiscountData HttpBody = new ShopeeAddDiscountData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                discount_name = varPromo.NAMA_PROMOSI,
//                start_time = starttime,
//                end_time = endtime,
//                items = new List<ShopeeAddDiscountDataItems>()
//            };
//            try
//            {
//                foreach (var promoDetail in varPromoDetail)
//                {
//                    var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG.Equals(promoDetail.KODE_BRG) && t.IDMARKET == arf01.RecNum).FirstOrDefault();
//                    if (brgInDB != null)
//                    {
//                        promoPrice = promoDetail.HARGA_PROMOSI;
//                        if (promoPrice == 0)
//                        {
//                            promoPrice = brgInDB.HJUAL - (brgInDB.HJUAL * promoDetail.PERSEN_PROMOSI / 100);
//                        }
//                        string[] brg_mp = new string[1];
//                        if (!string.IsNullOrEmpty(brgInDB.BRG_MP))
//                            brg_mp = brgInDB.BRG_MP.Split(';');
//                        if (brg_mp.Count() == 2)
//                        {
//                            if (brg_mp[1] == "0")
//                            {
//                                ShopeeAddDiscountDataItems item = new ShopeeAddDiscountDataItems()
//                                {
//                                    item_id = Convert.ToInt64(brg_mp[0]),
//                                    item_promotion_price = (float)promoPrice,
//                                    purchase_limit = (UInt32)(promoDetail.MAX_QTY == 0 ? 2 : promoDetail.MAX_QTY),
//                                    variations = new List<ShopeeAddDiscountDataItemsVariation>()
//                                };
//                                //item.variations.Add(new ShopeeAddDiscountDataItemsVariation()
//                                //{
//                                //    variation_id = 0,
//                                //    variation_promotion_price = (float)0.1
//                                //});
//                                HttpBody.items.Add(item);
//                            }
//                            else /*if (brg_mp[1] == "0")*/
//                            {
//                                ShopeeAddDiscountDataItems item = new ShopeeAddDiscountDataItems()
//                                {
//                                    item_id = Convert.ToInt64(brg_mp[0]),
//                                    item_promotion_price = (float)promoPrice,
//                                    //purchase_limit = 10,
//                                    purchase_limit = (UInt32)(promoDetail.MAX_QTY == 0 ? 2 : promoDetail.MAX_QTY),
//                                    variations = new List<ShopeeAddDiscountDataItemsVariation>()
//                                };
//                                item.variations.Add(new ShopeeAddDiscountDataItemsVariation()
//                                {
//                                    variation_id = Convert.ToInt64(brg_mp[1]),
//                                    //variation_promotion_price = (float)promoDetail.HARGA_PROMOSI
//                                    variation_promotion_price = (float)promoPrice,
//                                });
//                                HttpBody.items.Add(item);
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                ret = currentLog.REQUEST_EXCEPTION;
//                return ret;
//            }


//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                ret = currentLog.REQUEST_EXCEPTION;
//                return ret;
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeCreatePromoRes)) as ShopeeCreatePromoRes;
//                    if (resServer != null)
//                    {
//                        if (string.IsNullOrEmpty(resServer.error))
//                        {
//                            EDB.ExecuteSQL("CString", CommandType.Text, "UPDATE PROMOSIS SET MP_PROMO_ID = '" + resServer.discount_id + "' WHERE RECNUM = " + recNumPromosi);
//                            //if(resServer.errors.Count == 0)
//                            if (resServer.errors == null)
//                            {
//                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                            }
//                            else
//                            {
//                                foreach (var err in resServer.errors)
//                                {
//                                    currentLog.REQUEST_RESULT += "brg_mp:" + err.item_id + ";" + err.variation_id + ",error:" + err.error_msg + "\n";
//                                }
//                                ret = currentLog.REQUEST_RESULT;
//                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
//                            }
//                        }
//                        else
//                        {
//                            currentLog.REQUEST_RESULT = resServer.error + " " + resServer.msg;
//                            ret = currentLog.REQUEST_RESULT;
//                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
//                        }
//                    }
//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    ret = currentLog.REQUEST_EXCEPTION;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                }
//            }
//            return ret;
//        }

//        public async Task<string> AddDiscountItem(ShopeeAPIData iden, long discount_id, DetailPromosi detilPromosi)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";
//            string urll = "https://partner.shopeemobile.com/api/v1/discount/items/add";
//            if (!string.IsNullOrEmpty(iden.token))
//            {
//                MOPartnerID = MOPartnerIDV2;
//                MOPartnerKey = MOPartnerKeyV2;
//                urll = shopeeV2Url + "/api/v1/discount/items/add";
//            }
//            double promoPrice;
//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.merchant_code).FirstOrDefault();

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
//                REQUEST_ACTION = "Add Discount Item",
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
//                REQUEST_ATTRIBUTE_2 = detilPromosi.KODE_BRG,
//                REQUEST_STATUS = "Pending",
//            };

//            //string urll = "https://partner.shopeemobile.com/api/v1/discount/items/add";

//            ShopeeAddDiscountItemData HttpBody = new ShopeeAddDiscountItemData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                discount_id = discount_id,
//                items = new List<ShopeeAddDiscountDataItems>()
//            };

//            var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG.Equals(detilPromosi.KODE_BRG) && t.IDMARKET == arf01.RecNum).FirstOrDefault();
//            if (brgInDB != null)
//            {
//                promoPrice = detilPromosi.HARGA_PROMOSI;
//                if (promoPrice == 0)
//                {
//                    promoPrice = brgInDB.HJUAL - (brgInDB.HJUAL * detilPromosi.PERSEN_PROMOSI / 100);
//                }
//                //string[] brg_mp = brgInDB.BRG_MP.Split(';');
//                string[] brg_mp = new string[1];
//                if (!string.IsNullOrEmpty(brgInDB.BRG_MP))
//                    brg_mp = brgInDB.BRG_MP.Split(';');
//                if (brg_mp.Count() == 2)
//                {
//                    if (brg_mp[1] == "0")
//                    {
//                        ShopeeAddDiscountDataItems item = new ShopeeAddDiscountDataItems()
//                        {
//                            item_id = Convert.ToInt64(brg_mp[0]),
//                            //item_promotion_price = (float)detilPromosi.HARGA_PROMOSI,
//                            item_promotion_price = (float)promoPrice,
//                            //purchase_limit = 10,
//                            purchase_limit = (UInt32)(detilPromosi.MAX_QTY == 0 ? 2 : detilPromosi.MAX_QTY),
//                            variations = new List<ShopeeAddDiscountDataItemsVariation>()
//                        };
//                        //item.variations.Add(new ShopeeAddDiscountDataItemsVariation()
//                        //{
//                        //    variation_id = 0,
//                        //    variation_promotion_price = 0
//                        //});
//                        HttpBody.items.Add(item);
//                    }
//                    else/* if (brg_mp[1] == "0")*/
//                    {
//                        ShopeeAddDiscountDataItems item = new ShopeeAddDiscountDataItems()
//                        {
//                            item_id = Convert.ToInt64(brg_mp[0]),
//                            //item_promotion_price = (float)detilPromosi.HARGA_PROMOSI,
//                            item_promotion_price = (float)promoPrice,
//                            //purchase_limit = 10,
//                            purchase_limit = (UInt32)(detilPromosi.MAX_QTY == 0 ? 2 : detilPromosi.MAX_QTY),
//                            variations = new List<ShopeeAddDiscountDataItemsVariation>()
//                        };
//                        item.variations.Add(new ShopeeAddDiscountDataItemsVariation()
//                        {
//                            variation_id = Convert.ToInt64(brg_mp[1]),
//                            //variation_promotion_price = (float)detilPromosi.HARGA_PROMOSI
//                            variation_promotion_price = (float)promoPrice,
//                        });
//                        HttpBody.items.Add(item);
//                    }
//                }
//            }


//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                ret = currentLog.REQUEST_EXCEPTION;
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeCreatePromoRes)) as ShopeeCreatePromoRes;
//                    if (resServer != null)
//                    {
//                        if (string.IsNullOrEmpty(resServer.error))
//                        {
//                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                            //if (resServer.errors.Count == 0)
//                            if (resServer.errors == null)
//                            {
//                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                            }
//                            else
//                            {
//                                foreach (var err in resServer.errors)
//                                {
//                                    currentLog.REQUEST_RESULT += "brg_mp:" + err.item_id + ";" + err.variation_id + ",error:" + err.error_msg + "\n";
//                                }
//                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
//                                ret = currentLog.REQUEST_RESULT;
//                            }
//                        }
//                        else
//                        {
//                            currentLog.REQUEST_RESULT = resServer.error + " " + resServer.msg;
//                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
//                            ret = currentLog.REQUEST_RESULT;
//                        }
//                    }
//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                    ret = currentLog.REQUEST_EXCEPTION;
//                }
//            }
//            return ret;
//        }
//        public async Task<string> DeleteDiscount(ShopeeAPIData iden, long discount_id)
//        {
//            //Use this api to delete one discount activity BEFORE it starts.
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";
//            string urll = "https://partner.shopeemobile.com/api/v1/discount/delete";
//            if (!string.IsNullOrEmpty(iden.token))
//            {
//                MOPartnerID = MOPartnerIDV2;
//                MOPartnerKey = MOPartnerKeyV2;
//                urll = shopeeV2Url + "/api/v1/discount/delete";
//            }

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.merchant_code).FirstOrDefault();

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
//                REQUEST_ACTION = "Delete Discount",
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
//                REQUEST_ATTRIBUTE_2 = discount_id.ToString(),
//                REQUEST_STATUS = "Pending",
//            };

//            //string urll = "https://partner.shopeemobile.com/api/v1/discount/delete";

//            ShopeeDeleteDiscountData HttpBody = new ShopeeDeleteDiscountData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                discount_id = discount_id,
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                ret = currentLog.REQUEST_EXCEPTION;
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeDeleteDiscountData)) as ShopeeDeleteDiscountData;
//                    if (resServer != null)
//                    {
//                        if (string.IsNullOrEmpty(resServer.error))
//                        {
//                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                            //if (resServer.errors.Count == 0)
//                            if (resServer.errors == null)
//                            {
//                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                            }
//                            else
//                            {
//                                foreach (var err in resServer.errors)
//                                {
//                                    currentLog.REQUEST_RESULT += "brg_mp:" + err.item_id + ";" + err.variation_id + ",error:" + err.error_msg + "\n";
//                                }
//                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
//                                ret = currentLog.REQUEST_RESULT;
//                            }
//                        }
//                        else
//                        {
//                            currentLog.REQUEST_RESULT = resServer.error + " " + resServer.msg;
//                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
//                            ret = currentLog.REQUEST_RESULT;
//                        }
//                    }
//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                    ret = currentLog.REQUEST_EXCEPTION;
//                }
//            }
//            return ret;
//        }
//        public async Task<string> DeleteDiscountItem(ShopeeAPIData iden, long discount_id, DetailPromosi detilPromosi)
//        {
//            //Use this api to delete items of the discount activity
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";
//            string urll = "https://partner.shopeemobile.com/api/v1/discount/item/delete";
//            if (!string.IsNullOrEmpty(iden.token))
//            {
//                MOPartnerID = MOPartnerIDV2;
//                MOPartnerKey = MOPartnerKeyV2;
//                urll = shopeeV2Url + "/api/v1/discount/item/delete";
//            }

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.merchant_code).FirstOrDefault();

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                //REQUEST_ID = seconds.ToString(),
//                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
//                REQUEST_ACTION = "Delete Discount Item",
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
//                REQUEST_ATTRIBUTE_2 = detilPromosi.KODE_BRG,
//                REQUEST_STATUS = "Pending",
//            };

//            //string urll = "https://partner.shopeemobile.com/api/v1/discount/item/delete";

//            ShopeeDeleteDiscountItemData HttpBody = new ShopeeDeleteDiscountItemData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                discount_id = discount_id,
//            };

//            var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG.Equals(detilPromosi.KODE_BRG) && t.IDMARKET == arf01.RecNum).FirstOrDefault();
//            string[] brg_mp = new string[1];
//            if (!string.IsNullOrEmpty(brgInDB.BRG_MP))
//                brg_mp = brgInDB.BRG_MP.Split(';');
//            //string[] brg_mp = brgInDB.BRG_MP.Split(';');
//            if (brg_mp.Count() == 2)
//            {
//                //if(brg_mp[1] == "0")
//                //{
//                HttpBody.item_id = Convert.ToInt64(brg_mp[0]);
//                //}
//                //else
//                //{
//                HttpBody.variation_id = Convert.ToInt64(brg_mp[1]);
//                //}
//            }

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                ret = currentLog.REQUEST_EXCEPTION;
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeDeleteDiscountItemData)) as ShopeeDeleteDiscountItemData;
//                    if (resServer != null)
//                    {
//                        if (string.IsNullOrEmpty(resServer.error))
//                        {
//                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                            //if (resServer.errors.Count == 0)
//                            if (resServer.errors == null)
//                            {
//                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
//                            }
//                            else
//                            {
//                                foreach (var err in resServer.errors)
//                                {
//                                    currentLog.REQUEST_RESULT += "brg_mp:" + err.item_id + ";" + err.variation_id + ",error:" + err.error_msg + "\n";
//                                }
//                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
//                                ret = currentLog.REQUEST_RESULT;
//                            }
//                        }
//                        else
//                        {
//                            currentLog.REQUEST_RESULT = resServer.error + " " + resServer.msg;
//                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
//                            ret = currentLog.REQUEST_RESULT;
//                        }
//                    }
//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                    ret = currentLog.REQUEST_EXCEPTION;
//                }
//            }
//            return ret;
//        }

//        public async Task<ShopeeGetAddressResult> GetAddress(ShopeeAPIData iden)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            var ret = new ShopeeGetAddressResult();
//            string urll = "https://partner.shopeemobile.com/api/v1/logistics/address/get";
//            if (!string.IsNullOrEmpty(iden.token))
//            {
//                MOPartnerID = MOPartnerIDV2;
//                MOPartnerKey = MOPartnerKeyV2;
//                urll = shopeeV2Url + "/api/v1/logistics/address/get";
//            }

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            //string urll = "https://partner.shopeemobile.com/api/v1/logistics/address/get";

//            ShopeeGetAddressData HttpBody = new ShopeeGetAddressData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    ret = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetAddressResult)) as ShopeeGetAddressResult;

//                }
//                catch (Exception ex2)
//                {
//                }
//            }
//            return ret;
//        }
//        public async Task<ShopeeGetTimeSlotResult> GetLabel(ShopeeAPIData iden, bool is_batch, List<string> ordersn_list)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            var ret = new ShopeeGetTimeSlotResult();

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            string urll = "https://partner.shopeemobile.com/api/v1/logistics/airway_bill/get_mass";

//            ShopeeGetAirwayBill HttpBody = new ShopeeGetAirwayBill
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                is_batch = is_batch,
//                ordersn_list = ordersn_list
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            //try
//            //{
//            myReq.ContentLength = myData.Length;
//            using (var dataStream = myReq.GetRequestStream())
//            {
//                dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//            }
//            using (WebResponse response = await myReq.GetResponseAsync())
//            {
//                using (Stream stream = response.GetResponseStream())
//                {
//                    StreamReader reader = new StreamReader(stream);
//                    responseFromServer = reader.ReadToEnd();
//                }
//            }
//            //}
//            //catch (Exception ex)
//            //{
//            //}

//            if (responseFromServer != "")
//            {

//            }
//            return ret;
//        }
//        public async Task<ShopeeGetTimeSlotResult> GetTimeSlot(ShopeeAPIData iden, long address_id, string ordersn)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            var ret = new ShopeeGetTimeSlotResult();
//            string urll = "https://partner.shopeemobile.com/api/v1/logistics/timeslot/get";
//            if (!string.IsNullOrEmpty(iden.token))
//            {
//                MOPartnerID = MOPartnerIDV2;
//                MOPartnerKey = MOPartnerKeyV2;
//                urll = shopeeV2Url + "/api/v1/logistics/timeslot/get";
//            }

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            //string urll = "https://partner.shopeemobile.com/api/v1/logistics/timeslot/get";

//            ShopeeGetTimeSlotData HttpBody = new ShopeeGetTimeSlotData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                address_id = address_id,
//                ordersn = ordersn
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    ret = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetTimeSlotResult)) as ShopeeGetTimeSlotResult;
//                    if (ret.pickup_time != null)
//                    {
//                        foreach (var item in ret.pickup_time)
//                        {
//                            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
//                            dtDateTime = dtDateTime.AddSeconds(item.date).ToLocalTime();
//                            if (!string.IsNullOrWhiteSpace(item.time_text))
//                            {
//                                item.date_string = item.time_text;
//                            }
//                            else
//                            {
//                                item.date_string = dtDateTime.ToString("dd MMMM yyyy HH:mm:ss");
//                            }
//                        }
//                    }
//                    else
//                    {
//                        var err = JsonConvert.DeserializeObject(responseFromServer, typeof(GetPickupTimeSlotError)) as GetPickupTimeSlotError;
//                        ShopeeGetTimeSlotResultPickup_Time errItem = new ShopeeGetTimeSlotResultPickup_Time()
//                        {
//                            pickup_time_id = "-1",
//                            date_string = "Order sudah Expired."
//                        };
//                        ret.pickup_time = new ShopeeGetTimeSlotResultPickup_Time[1];
//                        ret.pickup_time[0] = (errItem);
//                    }
//                }
//                catch (Exception ex2)
//                {
//                }
//            }
//            return ret;
//        }
//        public async Task<string> GetBranch(ShopeeAPIData iden, string ordersn)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";
//            string urll = "https://partner.shopeemobile.com/api/v1/logistics/branch/get";
//            if (!string.IsNullOrEmpty(iden.token))
//            {
//                MOPartnerID = MOPartnerIDV2;
//                MOPartnerKey = MOPartnerKeyV2;
//                urll = shopeeV2Url + "/api/v1/logistics/branch/get";
//            }

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            //string urll = "https://partner.shopeemobile.com/api/v1/logistics/branch/get";

//            ShopeeGetBranchData HttpBody = new ShopeeGetBranchData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                ordersn = ordersn
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    //var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetItemListResult)) as ShopeeGetItemListResult;

//                }
//                catch (Exception ex2)
//                {
//                }
//            }
//            return ret;
//        }

//        public async Task<string> Template(ShopeeAPIData iden)
//        {
//            int MOPartnerID = 841371;
//            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
//            string ret = "";

//            long seconds = CurrentTimeSecond();
//            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

//            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
//            {
//                REQUEST_ID = seconds.ToString(),
//                REQUEST_ACTION = "Get Item List", //ganti
//                REQUEST_DATETIME = milisBack,
//                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
//                REQUEST_STATUS = "Pending",
//            };

//            //ganti
//            string urll = "https://partner.shopeemobile.com/api/v1/items/get";

//            //ganti
//            ShopeeGetItemListData HttpBody = new ShopeeGetItemListData
//            {
//                partner_id = MOPartnerID,
//                shopid = Convert.ToInt32(iden.merchant_code),
//                timestamp = seconds,
//                pagination_offset = 0,
//                pagination_entries_per_page = 100
//            };

//            string myData = JsonConvert.SerializeObject(HttpBody);

//            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

//            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
//            myReq.Method = "POST";
//            myReq.Headers.Add("Authorization", signature);
//            myReq.Accept = "application/json";
//            myReq.ContentType = "application/json";
//            string responseFromServer = "";
//            try
//            {
//                myReq.ContentLength = myData.Length;
//                using (var dataStream = myReq.GetRequestStream())
//                {
//                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
//                }
//                using (WebResponse response = await myReq.GetResponseAsync())
//                {
//                    using (Stream stream = response.GetResponseStream())
//                    {
//                        StreamReader reader = new StreamReader(stream);
//                        responseFromServer = reader.ReadToEnd();
//                    }
//                }
//                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
//            }
//            catch (Exception ex)
//            {
//                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
//                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//            }

//            if (responseFromServer != null)
//            {
//                try
//                {
//                    //ganti
//                    var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetItemListResult)) as ShopeeGetItemListResult;
//                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);

//                }
//                catch (Exception ex2)
//                {
//                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
//                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
//                }
//            }
//            return ret;
//        }

        [HttpGet]
        public string ShopeeUrl(string cust)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
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

            string compUrl = shpCallbackUrl + userId + "_param_" + cust;
            string token = CreateTokenAuthenShop(Convert.ToString(MOPartnerID), MOPartnerKey, compUrl);
            string uri = "https://partner.shopeemobile.com/api/v1/shop/auth_partner?id=" + Convert.ToString(MOPartnerID) + "&token=" + token + "&redirect=" + compUrl;
            return uri;
        }

        [HttpGet]
        public string ShopeeUrl_V2(string cust)
        {
            int MOPartnerID = MOPartnerIDV2;
            string MOPartnerKey = MOPartnerKeyV2;
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

            string compUrl = shpCallbackUrlV2 + userId + "_param_" + cust;
            var milis = CurrentTimeSecond();
            string baseString = Convert.ToString(MOPartnerID) + "/api/v2/shop/auth_partner" + milis;
            var sign = CreateSignAuthenShop_V2(baseString, MOPartnerKey);
            string uri = shopeeV2Url + "/api/v2/shop/auth_partner?partner_id=" + Convert.ToString(MOPartnerID) + "&sign=" + sign
                + "&redirect=" + compUrl + "&timestamp=" + milis;
            return uri;
        }
        public enum StatusOrder
        {
            IN_CANCEL = 1,
            CANCELLED = 2,
            READY_TO_SHIP = 3,
            COMPLETED = 4,
            TO_RETURN = 5,
            UNPAID = 6
        }
        protected enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4
        }

        public string CreateTokenAuthenShop(string partnerID, string partnerKey, string redirectUrl)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(partnerKey + redirectUrl));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public string CreateSignAuthenShop_V2(string data, string secretKey)
        {
            //using (SHA256 sha256Hash = SHA256.Create())
            //{
            //    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data));

            //    StringBuilder builder = new StringBuilder();
            //    for (int i = 0; i < bytes.Length; i++)
            //    {
            //        builder.Append(bytes[i].ToString("x2"));
            //    }
            //    return builder.ToString();
            //}
            secretKey = secretKey ?? "";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secretKey);
            byte[] messageBytes = encoding.GetBytes(data);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
                //return BitConverter.ToString(hashmessage).ToLower();
            }
        }
        private string CreateSign(string signBase, string secretKey)
        {
            secretKey = secretKey ?? "";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secretKey);
            byte[] messageBytes = encoding.GetBytes(signBase);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
                //return BitConverter.ToString(hashmessage).ToLower();
            }
        }
        protected void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, ShopeeAPIDataChat iden, API_LOG_MARKETPLACE data)
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
                                MARKETPLACE = "Shopee",
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
            catch (Exception ex)
            {

            }
        }
        public static long CurrentTimeSecond()
        {
            //        return (long)DateTime.Now.ToUniversalTime().Subtract(
            //new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            //).TotalMilliseconds;
            return (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        public IDictionary<string, string> ToKeyValue(object metaToken)
        {
            if (metaToken == null)
            {
                return null;
            }

            Newtonsoft.Json.Linq.JToken token = metaToken as Newtonsoft.Json.Linq.JToken;
            if (token == null)
            {
                return ToKeyValue(Newtonsoft.Json.Linq.JObject.FromObject(metaToken));
            }

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, string>();
                foreach (var child in token.Children().ToList())
                {
                    var childContent = ToKeyValue(child);
                    if (childContent != null)
                    {
                        contentData = contentData.Concat(childContent)
                                                 .ToDictionary(k => k.Key, v => v.Value);
                    }
                }

                return contentData;
            }

            var jValue = token as Newtonsoft.Json.Linq.JValue;
            if (jValue?.Value == null)
            {
                return null;
            }

            var value = jValue?.Type == Newtonsoft.Json.Linq.JTokenType.Date ?
                            jValue?.ToString("o", System.Globalization.CultureInfo.InvariantCulture) :
                            jValue?.ToString(System.Globalization.CultureInfo.InvariantCulture);

            return new Dictionary<string, string> { { token.Path, value } };
        }

        //add by nurul 21/9/2021
        public async Task<Rootobject> GetConversationList(ShopeeAPIDataChat dataAPI, string direction, string type, string next_timestamp_nano)
        {
            dataAPI = await RefreshTokenShopee_V2(dataAPI, false);
            int MOPartnerID = MOPartnerIDV2;
            string MOPartnerKey = MOPartnerKeyV2;
            var ret = new Rootobject();

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = shopeeV2Url;
            string path = "/api/v2/sellerchat/get_conversation_list";

            var baseString = MOPartnerID + path + seconds + dataAPI.TOKEN_CHAT + dataAPI.merchant_code;
            var sign = CreateSignAuthenShop_V2(baseString, MOPartnerKey);

            //direction = "latest";
            //type = "all";

            string param = "?partner_id=" + MOPartnerID + "&timestamp=" + seconds + "&access_token=" + dataAPI.TOKEN_CHAT
                + "&shop_id=" + dataAPI.merchant_code + "&sign=" + sign + "&direction=" + direction + "&type=" + type + "&next_timestamp_nano=" + next_timestamp_nano;

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + path + param);
            myReq.Method = "GET";
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            //myReq.UseDefaultCredentials = true;
            //myReq.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
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
            //catch (Exception ex)
            //{
            //}
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
                //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                //{
                //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                //    REQUEST_ACTION = "Refresh Token Shopee V2", //ganti
                //    REQUEST_DATETIME = milisBack,
                //    REQUEST_ATTRIBUTE_1 = dataAPI.merchant_code
                //};
                //currentLog.REQUEST_EXCEPTION = err;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(Rootobject)) as Rootobject;

                    if (result.message != null)
                    {
                        ret = result;

                        if (loop_page == 0)
                        {
                            var sSQL = "DELETE FROM SHOPEE_LISTCONVERSATION WHERE SHOP_ID ='" + dataAPI.merchant_code + "'";
                            var resultquery = ErasoftDbContext.Database.ExecuteSqlCommand(sSQL);

                            loop_page = 1;
                        }

                        var listConversation = new List<SHOPEE_LISTCONVERSATION>() { };
                        if (result.response.conversations.Count() > 0)
                        {
                            var dateNow = DateTime.UtcNow.AddHours(7);
                            //var dateLast1Month = dateNow.AddMonths(-1);
                            var dateLast1Month = dateNow.AddDays(-14);

                            var cust = ErasoftDbContext.ARF01.Where(a => a.API_KEY == dataAPI.API_secret_key && a.Sort1_Cust == dataAPI.merchant_code).FirstOrDefault();
                            if (cust != null)
                            {
                                var cekListMessage = ErasoftDbContext.SHOPEE_LISTCONVERSATION.Select(a => a.conversation_id).ToList();
                                var lastGetMessage = false;

                                while (!lastGetMessage)
                                {
                                    foreach (var msg in result.response.conversations)
                                    {
                                        var txtmsg = "";
                                        if (msg.latest_message_type == "text")
                                        {
                                            txtmsg = msg.latest_message_type;
                                        }

                                        //var date_last_message_timestamp = DateTime.UtcNow.AddHours(7).AddSeconds(msg.last_message_timestamp);
                                        //var date_last_message_timestamp = DateTimeOffset.FromUnixTimeSeconds(msg.last_message_timestamp).UtcDateTime.AddHours(7);
                                        //var date_last_message_timestamp = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(msg.last_message_timestamp.ToString().Substring(0,10))).UtcDateTime.AddHours(7);
                                        //nanoseconds timestamp
                                        var date_last_message_timestamp = DateTimeOffset.FromUnixTimeSeconds((msg.last_message_timestamp) / 1000000000).UtcDateTime.AddHours(7);
                                        var last_message_timestamp = date_last_message_timestamp.ToString("yyyy-MM-dd HH:mm:ss");

                                        var message = new SHOPEE_LISTCONVERSATION
                                        {
                                            conversation_id = msg.conversation_id,
                                            last_message_option = msg.last_message_option,
                                            last_message_timestamp = Convert.ToDateTime(last_message_timestamp),
                                            last_read_message_id = msg.last_read_message_id,
                                            latest_message_content_text = msg.latest_message_content.text,
                                            latest_message_content_url = "",
                                            latest_message_from_id = msg.latest_message_from_id,
                                            latest_message_id = msg.latest_message_id,
                                            latest_message_type = msg.latest_message_type,
                                            max_general_option_hide_time = msg.max_general_option_hide_time,
                                            pinned = Convert.ToString(msg.pinned),
                                            shop_id = msg.shop_id,
                                            to_avatar = msg.to_avatar,
                                            to_id = msg.to_id,
                                            to_name = msg.to_name,
                                            unread_count = msg.unread_count,
                                        };
                                        //masukin sampe -1 bulan 
                                        if (Convert.ToDateTime(last_message_timestamp) < dateLast1Month)
                                        {
                                            lastGetMessage = true; break;
                                        }

                                        //hanya masukin yg blm ada di list Conversation
                                        if (!cekListMessage.Contains(message.conversation_id))
                                        {
                                            listConversation.Add(message);
                                            var cekListChat = ErasoftDbContext.SHOPEE_MESSAGE.AsNoTracking().Where(a => a.conversation_id == message.conversation_id).Count();
                                            if (cekListChat == 0)
                                            {
                                                await GetGetMessage(dataAPI, message.conversation_id, "");
                                            }
                                        }
                                    }
                                }

                                if (listConversation.Count() > 0)
                                {
                                    ErasoftDbContext.SHOPEE_LISTCONVERSATION.AddRange(listConversation);
                                    ErasoftDbContext.SaveChanges();
                                }
                            }

                            if (string.IsNullOrEmpty(next_timestamp_nano)) //page 1
                            {
                                if (result.response.page_result.next_cursor.next_message_time_nano != "0") // has possibility the next page exist
                                {
                                    await GetConversationList(dataAPI, direction, type, result.response.page_result.next_cursor.next_message_time_nano);
                                }
                            }
                            else
                            {
                                if (result.response.page_result.next_cursor.next_message_time_nano != "0")
                                {
                                    await GetConversationList(dataAPI, direction, type, result.response.page_result.next_cursor.next_message_time_nano);
                                }
                            }
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

        public async Task<RootobjectGetMsg> GetGetMessage(ShopeeAPIDataChat dataAPI, string conversation_id, string offset)
        {
            dataAPI = await RefreshTokenShopee_V2(dataAPI, false);
            int MOPartnerID = MOPartnerIDV2;
            string MOPartnerKey = MOPartnerKeyV2;
            var ret = new RootobjectGetMsg();

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = shopeeV2Url;
            string path = "/api/v2/sellerchat/get_message";

            var baseString = MOPartnerID + path + seconds + dataAPI.TOKEN_CHAT + dataAPI.merchant_code;
            var sign = CreateSignAuthenShop_V2(baseString, MOPartnerKey);

            //direction = "latest";
            //type = "all";

            string param = "?partner_id=" + MOPartnerID + "&timestamp=" + seconds + "&access_token=" + dataAPI.TOKEN_CHAT
                + "&shop_id=" + dataAPI.merchant_code + "&sign=" + sign + "&conversation_id=" + conversation_id + "&offset=" + offset;

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + path + param);
            myReq.Method = "GET";
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            //myReq.UseDefaultCredentials = true;
            //myReq.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
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
            //catch (Exception ex)
            //{
            //}
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
                //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                //{
                //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                //    REQUEST_ACTION = "Refresh Token Shopee V2", //ganti
                //    REQUEST_DATETIME = milisBack,
                //    REQUEST_ATTRIBUTE_1 = dataAPI.merchant_code
                //};
                //currentLog.REQUEST_EXCEPTION = err;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(RootobjectGetMsg)) as RootobjectGetMsg;

                    if (result.response.messages != null)
                    {
                        ret = result;

                        var listMsg = new List<SHOPEE_MESSAGE>() { };
                        if (result.response.page_result.page_size > 0)
                        {
                            var firstReply = false;
                            var dateNow = DateTime.UtcNow.AddHours(7);
                            var dateLast1Month = dateNow.AddMonths(-1);
                            var cust = ErasoftDbContext.ARF01.Where(a => a.API_KEY == dataAPI.API_secret_key && a.Sort1_Cust == dataAPI.merchant_code).FirstOrDefault();
                            if (cust != null)
                            {
                                var cekListMessage = ErasoftDbContext.SHOPEE_MESSAGE.Select(a => a.message_id).ToList();
                                var lastGetMessage = false;
                                while (!lastGetMessage)
                                {
                                    foreach (var msg in result.response.messages)
                                    {
                                        var created_timestamp = DateTimeOffset.FromUnixTimeSeconds(msg.created_timestamp).UtcDateTime.AddHours(7);
                                        var last_created_timestamp = created_timestamp.ToString("yyyy-MM-dd HH:mm:ss");

                                        var file_server_id = 0;
                                        if (msg.content.file_server_id == 0)
                                        {
                                            file_server_id = 0;
                                        }
                                        else
                                        {
                                            file_server_id = msg.content.file_server_id;
                                        }

                                        var shop_id = 0;
                                        if (msg.content.shop_id == 0)
                                        {
                                            shop_id = 0;
                                        }
                                        else
                                        {
                                            shop_id = msg.content.shop_id;
                                        }

                                        var voucher_id = "";
                                        if (string.IsNullOrEmpty(msg.content.voucher_id))
                                        {
                                            voucher_id = "";
                                        }
                                        else
                                        {
                                            voucher_id = msg.content.voucher_id;
                                        }

                                        var voucher_code = "";
                                        if (string.IsNullOrEmpty(msg.content.voucher_code))
                                        {
                                            voucher_code = "";
                                        }
                                        else
                                        {
                                            voucher_code = msg.content.voucher_code;
                                        }

                                        var url = "";
                                        if (string.IsNullOrEmpty(msg.content.url))
                                        {
                                            url = "";
                                        }
                                        else
                                        {
                                            url = msg.content.url;
                                        }

                                        var text = "";
                                        if (string.IsNullOrEmpty(msg.content.text))
                                        {
                                            text = "";
                                        }
                                        else
                                        {
                                            text = msg.content.text;
                                        }

                                        var source = "";
                                        if (string.IsNullOrEmpty(msg.content.source))
                                        {
                                            source = "";
                                        }
                                        else
                                        {
                                            source = msg.content.source;
                                        }

                                        var message = new SHOPEE_MESSAGE
                                        {
                                            to_shop_id = msg.to_shop_id,
                                            to_id = msg.to_id,
                                            from_shop_id = msg.from_shop_id,
                                            from_id = msg.from_id,
                                            request_id = result.request_id,
                                            created_timestamp = Convert.ToDateTime(last_created_timestamp),
                                            conversation_id = msg.conversation_id,
                                            status = msg.status,
                                            source = msg.source,
                                            region = msg.region,
                                            message_type = msg.message_type,
                                            message_option = msg.message_option,
                                            message_id = msg.message_id,
                                            message = result.message,
                                            content_voucher_id = voucher_id,
                                            content_voucher_code = voucher_code,
                                            content_url = url,
                                            content_text = text,
                                            content_source = source,
                                            content_shop_id = shop_id,
                                            content_file_server_id = file_server_id,
                                            content_chat_product_infos_item_id = 0,
                                            content_chat_product_infos_model_name = "",
                                            content_chat_product_infos_name = "",
                                            content_chat_product_infos_price = "",
                                            content_chat_product_infos_price_before_discount = "",
                                            content_chat_product_infos_quantity = 0,
                                            content_chat_product_infos_shop_id = 0,
                                            content_chat_product_infos_snap_shop_id = 0,
                                            content_chat_product_infos_thumb_url = "",
                                        };
                                        //masukin sampe -1 bulan 
                                        //if (message.last_reply_time < dateLast1Month)
                                        //{
                                        //    lastGetMessage = true; break;
                                        //}


                                        ////hanya masukin yg blm ada di list Conversation
                                        //if (!cekListMessage.Contains(message.message_id))
                                        //{
                                        //    listMsg.Add(message);
                                        //}

                                        //masukin sampe -1 bulan 
                                        if (message.created_timestamp < dateLast1Month)
                                        {
                                            if (!cekListMessage.Contains(message.message_id))
                                            {
                                                listMsg.Add(message);
                                            }

                                            firstReply = true;
                                            lastGetMessage = true; break;
                                        }
                                        else
                                        {
                                            if (!cekListMessage.Contains(message.message_id))
                                            {
                                                listMsg.Add(message);
                                            }
                                        }

                                        if (firstReply) break;
                                    }
                                }
                                if (listMsg.Count() > 0)
                                {
                                    ErasoftDbContext.SHOPEE_MESSAGE.AddRange(listMsg);
                                    ErasoftDbContext.SaveChanges();
                                }
                            }

                            if (!firstReply)
                            {
                                if (string.IsNullOrEmpty(offset)) // page 1
                                {
                                    if (result.response.page_result.page_size == 25) // max show per page 25, must check with offset for another page
                                    {
                                        await GetGetMessage(dataAPI, conversation_id, result.response.page_result.next_offset);
                                    }
                                }
                                else
                                {
                                    await GetGetMessage(dataAPI, conversation_id, result.response.page_result.next_offset);
                                }
                            }
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
        //end add by nurul 21/9/2021

        public class ShopeeAPIData
        {
            public string merchant_code { get; set; }
            public string API_client_username { get; set; }
            public string API_client_password { get; set; }
            public string API_secret_key { get; set; }
            public string mta_username_email_merchant { get; set; }
            public string mta_password_password_merchant { get; set; }
            public string token { get; set; }
            public string DatabasePathErasoft { get; set; }
            public string username { get; set; }
            public string no_cust { get; set; }
            public DateTime? tgl_expired { get; set; }
            public string refresh_token { get; set; }//add for api v2
            public DateTime? token_expired { get; set; }
        }
        public class ShopeeGetAttributeData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public string language { get; set; }
            public int category_id { get; set; }
        }
        public class ShopeeGetTokenShop
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
        }
        public class ShopeeGetTokenShop_V2
        {
            public string code { get; set; }
            public int partner_id { get; set; }
            public int shop_id { get; set; }
            //public long timestamp { get; set; }
        }

        public class ShopeeGetTokenShopResult
        {
            public string error { get; set; }
            public string msg { get; set; }
            public ShopeeGetTokenShopResultAtribute[] authed_shops { get; set; }
            public string request_id { get; set; }
        }
        public class ShopeeGetShopResult_V2
        {
            public string error { get; set; }
            public string msg { get; set; }
            public string shop_name { get; set; }
            public string region { get; set; }
            public string status { get; set; }
            public string request_id { get; set; }
            public long expire_time { get; set; }
        }
        public class ShopeeGetTokenShopResult_V2
        {
            public string error { get; set; }
            public string message { get; set; }
            public string request_id { get; set; }
            public string refresh_token { get; set; }
            public string access_token { get; set; }
            public int expire_in { get; set; }
        }

        public class ShopeeGetTokenShopResultAtribute
        {
            public long expire_time { get; set; }
            public string country { get; set; }
            public object[] sip_a_shops { get; set; }
            public int shopid { get; set; }
            public long auth_time { get; set; }
        }

        //public class Sip_A_Shops
        //{
        //    public int a_shop_id { get; set; }
        //    public string country { get; set; }
        //}


        public class ShopeeGetCategoryData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public string language { get; set; }
        }

        public class ShopeeGetOrderByStatusData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public int pagination_offset { get; set; }
            public int pagination_entries_per_page { get; set; }
            public long create_time_from { get; set; }
            public long create_time_to { get; set; }
            public string order_status { get; set; }
        }

        public class ShopeeGetParameterForInitLogisticData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
        }
        public class GetOrderDetailsData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public string[] ordersn_list { get; set; }
        }

        public class ShopeeGetItemListData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public int pagination_offset { get; set; }
            public int pagination_entries_per_page { get; set; }
        }
        public class ShopeeGetItemDetailData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
        }
        #region get item list v2
        public class ShopeeGetItemListResult_V2
        {
            public ResponseShopeeGetItemListResult_V2 response { get; set; }
            public string request_id { get; set; }
            public string error { get; set; }
            public string message { get; set; }
            public string warning { get; set; }
        }
        public class ResponseShopeeGetItemListResult_V2
        {
            public ShopeeGetItemListItem_V2[] item { get; set; }
            public int total_count { get; set; }
            public int next_offset { get; set; }
            public bool has_next_page { get; set; }
        }
        public class ShopeeGetItemListItem_V2
        {
            public string item_status { get; set; }
            public long update_time { get; set; }
            public long item_id { get; set; }
        }
        #endregion
        public class ShopeeGetItemListResult
        {
            public ShopeeGetItemListItem[] items { get; set; }
            public string request_id { get; set; }
            public bool more { get; set; }
        }

        public class ShopeeGetItemListItem
        {
            public string status { get; set; }
            public long update_time { get; set; }
            public string item_sku { get; set; }
            public ShopeeGetItemListVariation[] variations { get; set; }
            public long shopid { get; set; }
            public long item_id { get; set; }
        }

        public class ShopeeGetItemListVariation
        {
            public string variation_sku { get; set; }
            public long variation_id { get; set; }
        }
        #region get item detail v2

        public class ShopeeGetItemDetailResult_V2
        {
            public string error { get; set; }
            public string message { get; set; }
            public string warning { get; set; }
            public string request_id { get; set; }
            public ResponseShopeeGetItemDetailResult response { get; set; }
        }

        public class ResponseShopeeGetItemDetailResult
        {
            public Item_List[] item_list { get; set; }
        }

        public class Item_List
        {
            public long item_id { get; set; }
            public long category_id { get; set; }
            public string item_name { get; set; }
            public string description { get; set; }
            public string item_sku { get; set; }
            //public int create_time { get; set; }
            //public int update_time { get; set; }
            public Attribute_List[] attribute_list { get; set; }
            public Price_Info[] price_info { get; set; }
            public Stock_Info[] stock_info { get; set; }
            public Item_List_Image image { get; set; }
            public string weight { get; set; }
            public Dimension dimension { get; set; }
            public Logistic_Info[] logistic_info { get; set; }
            public Pre_Order pre_order { get; set; }
            public string condition { get; set; }
            public string size_chart { get; set; }
            public string item_status { get; set; }
            public bool has_model { get; set; }
            public long promotion_id { get; set; }
            public Item_List_Brand brand { get; set; }
        }

        public class Item_List_Image
        {
            public string[] image_url_list { get; set; }
            public string[] image_id_list { get; set; }
        }

        public class Dimension
        {
            public int package_length { get; set; }
            public int package_width { get; set; }
            public int package_height { get; set; }
        }

        public class Pre_Order
        {
            public bool is_pre_order { get; set; }
            public int days_to_ship { get; set; }
        }

        public class Item_List_Brand
        {
            public long brand_id { get; set; }
            public string original_brand_name { get; set; }
        }

        public class Attribute_List
        {
            public long attribute_id { get; set; }
            public string original_attribute_name { get; set; }
            public bool is_mandatory { get; set; }
            public Attribute_Value_List[] attribute_value_list { get; set; }
        }

        public class Attribute_Value_List
        {
            public long value_id { get; set; }
            public string original_value_name { get; set; }
            public string value_unit { get; set; }
        }

        public class Price_Info
        {
            public string currency { get; set; }
            public int original_price { get; set; }
            public int current_price { get; set; }
        }

        public class Stock_Info
        {
            public int stock_type { get; set; }
            public int current_stock { get; set; }
            public int normal_stock { get; set; }
            public int reserved_stock { get; set; }
        }

        public class Logistic_Info
        {
            public int logistic_id { get; set; }
            public string logistic_name { get; set; }
            public bool enabled { get; set; }
            public int shipping_fee { get; set; }
            public bool is_free { get; set; }
            public float estimated_shipping_fee { get; set; }
        }

        #endregion
        public class ShopeeGetItemDetailResult
        {
            public ShopeeGetItemDetailItem item { get; set; }
            public string warning { get; set; }
            public string request_id { get; set; }
        }

        public class ShopeeGetItemDetailItem
        {
            public ShopeeGetItemDetailLogistic[] logistics { get; set; }
            public float original_price { get; set; }
            public float package_width { get; set; }
            public int cmt_count { get; set; }
            public float weight { get; set; }
            public long shopid { get; set; }
            public string currency { get; set; }
            public long create_time { get; set; }
            public int likes { get; set; }
            public string[] images { get; set; }
            public int days_to_ship { get; set; }
            public float package_length { get; set; }
            public int stock { get; set; }
            public string status { get; set; }
            public long update_time { get; set; }
            public string description { get; set; }
            public long views { get; set; }
            public float price { get; set; }
            public int sales { get; set; }
            public long discount_id { get; set; }
            public long item_id { get; set; }
            public object[] wholesales { get; set; }
            public string condition { get; set; }
            public float package_height { get; set; }
            public string name { get; set; }
            public float rating_star { get; set; }
            public string item_sku { get; set; }
            public ShopeeGetItemDetailVariation[] variations { get; set; }
            public string size_chart { get; set; }
            public bool has_variation { get; set; }
            public ShopeeGetItemDetailAttribute[] attributes { get; set; }
            public long category_id { get; set; }
        }

        public class ShopeeGetItemDetailLogistic
        {
            public string logistic_name { get; set; }
            public bool is_free { get; set; }
            public float estimated_shipping_fee { get; set; }
            public long logistic_id { get; set; }
            public bool enabled { get; set; }
        }

        public class ShopeeGetItemDetailVariation
        {
            public string status { get; set; }
            public float original_price { get; set; }
            public long update_time { get; set; }
            public long create_time { get; set; }
            public string name { get; set; }
            public float price { get; set; }
            public string variation_sku { get; set; }
            public long variation_id { get; set; }
            public int stock { get; set; }
        }

        public class ShopeeGetItemDetailAttribute
        {
            public string attribute_name { get; set; }
            public bool is_mandatory { get; set; }
            public long attribute_id { get; set; }
            public string attribute_value { get; set; }
            public string attribute_type { get; set; }
        }


        public class ShopeeGetCategoryResult
        {
            public ShopeeGetCategoryCategory[] categories { get; set; }
            public string request_id { get; set; }
        }
        public class ShopeeGetCategoryResult_V2 : ShopeeDefaultResponse_V2
        {
            public ListCategoryShopee response { get; set; }
        }
        public class ListCategoryShopee
        {
            public ShopeeGetCategoryCategory_V2[] category_list { get; set; }

        }
        public class ShopeeDefaultResponse_V2
        {
            public string error { get; set; }
            public string message { get; set; }
            public string warning { get; set; }
            public string request_id { get; set; }

        }
        public class ShopeeGetCategoryCategory
        {
            public long parent_id { get; set; }
            public bool has_children { get; set; }
            public long category_id { get; set; }
            public string category_name { get; set; }
        }
        public class ShopeeGetCategoryCategory_V2
        {
            public long parent_category_id { get; set; }
            public bool has_children { get; set; }
            public long category_id { get; set; }
            public string original_category_name { get; set; }
            public string display_category_name { get; set; }
        }

        public class ShopeeGetAttributeResult
        {
            public ShopeeGetAttributeAttribute[] attributes { get; set; }
            public string request_id { get; set; }
        }

        public class ShopeeGetAttributeAttribute
        {
            public string attribute_name { get; set; }
            public string input_type { get; set; }
            public ShopeeGetAttributeValue[] values { get; set; }
            public long attribute_id { get; set; }
            public string attribute_type { get; set; }
            public bool is_mandatory { get; set; }
            public string[] options { get; set; }
        }

        public class ShopeeGetAttributeValue
        {
            public string original_value { get; set; }
            public string translate_value { get; set; }
        }
        //public class ShopeeGetOrderByStatusResult

        public class ShopeeGetAttributeResult_V2
        {
            public string error { get; set; }
            public string message { get; set; }
            public string warning { get; set; }
            public string request_id { get; set; }
            public ShopeeGetAttribute_V2 response { get; set; }
        }

        public class ShopeeGetAttribute_V2
        {
            public Attribute_List_V2[] attribute_list { get; set; }
        }

        public class Attribute_List_V2
        {
            public long attribute_id { get; set; }
            public string original_attribute_name { get; set; }
            public string display_attribute_name { get; set; }
            public bool is_mandatory { get; set; }
            public string input_validation_type { get; set; }
            public string input_type { get; set; }
            public string[] attribute_unit { get; set; }
            public Attribute_Value_List_V2[] attribute_value_list { get; set; }
        }

        public class Attribute_Value_List_V2
        {
            public int value_id { get; set; }
            public string original_value_name { get; set; }
            public string display_value_name { get; set; }
        }

        public class ShopeeGetBrandResult_V2
        {
            public string error { get; set; }
            public string message { get; set; }
            public string warning { get; set; }
            public string request_id { get; set; }
            public ShopeeGetBrand_V2 response { get; set; }
        }

        public class ShopeeGetBrand_V2
        {
            public Brand_List_V2[] brand_list { get; set; }
            public bool has_next_page { get; set; }
            public int next_offset { get; set; }
            public bool is_mandatory { get; set; }
            public string input_type { get; set; }
        }

        public class Brand_List_V2
        {
            public long brand_id { get; set; }
            public string original_brand_name { get; set; }
            public string display_brand_name { get; set; }
        }

        //
        public class ShopeeGetOrderByStatusResult
        {
            public string request_id { get; set; }
            public ShopeeGetOrderByStatusResultOrder[] orders { get; set; }
            public bool more { get; set; }
        }

        public class ShopeeGetOrderByStatusResultOrder
        {
            public string ordersn { get; set; }
            public string order_status { get; set; }
            public long update_time { get; set; }
        }

        public class ShopeeGetOrderDetailsResult
        {
            public object[] errors { get; set; }
            public ShopeeGetOrderDetailsResultOrder[] orders { get; set; }
            public string request_id { get; set; }
        }

        public class ShopeeGetOrderDetailsResultOrder
        {
            public string note { get; set; }
            public string estimated_shipping_fee { get; set; }
            public string payment_method { get; set; }
            public string escrow_amount { get; set; }
            public string message_to_seller { get; set; }
            public string shipping_carrier { get; set; }
            public string currency { get; set; }
            public long create_time { get; set; }
            public int? pay_time { get; set; }
            public ShopeeGetOrderDetailsResultRecipient_Address recipient_address { get; set; }
            public int days_to_ship { get; set; }
            public string tracking_no { get; set; }
            public string order_status { get; set; }
            public long note_update_time { get; set; }
            public long update_time { get; set; }
            public bool goods_to_declare { get; set; }
            public string total_amount { get; set; }
            public string service_code { get; set; }
            public string country { get; set; }
            public string actual_shipping_cost { get; set; }
            public bool cod { get; set; }
            public ShopeeGetOrderDetailsResultItem[] items { get; set; }
            public string ordersn { get; set; }
            public string dropshipper { get; set; }
            public string buyer_username { get; set; }
        }

        public class ShopeeGetOrderDetailsResultRecipient_Address
        {
            public string town { get; set; }
            public string city { get; set; }
            public string name { get; set; }
            public string district { get; set; }
            public string country { get; set; }
            public string zipcode { get; set; }
            public string full_address { get; set; }
            public string phone { get; set; }
            public string state { get; set; }
        }

        public class ShopeeGetOrderDetailsResultItem
        {
            public float weight { get; set; }
            public string item_name { get; set; }
            public bool is_wholesale { get; set; }
            public string item_sku { get; set; }
            public string variation_discounted_price { get; set; }
            public long variation_id { get; set; }
            public string variation_name { get; set; }
            public long item_id { get; set; }
            public int variation_quantity_purchased { get; set; }
            public string variation_sku { get; set; }
            public string variation_original_price { get; set; }
        }
        public class ShopeeGetParameterForInitLogisticResult
        {
            public string[] pickup { get; set; }
            public string[] dropoff { get; set; }
            public string[] non_integrated { get; set; }
            public string request_id { get; set; }
        }

        public class ShopeeInitLogisticDropOffData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
            public object dropoff { get; set; }
        }
        public class ShopeeInitLogisticPickupData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
            public object pickup { get; set; }
        }
        public class ShopeeInitLogisticNonIntegratedData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
            public object non_integrated { get; set; }
        }
        public class ShopeeInitLogisticPickupDetailData
        {
            public long address_id { get; set; }
            public string pickup_time_id { get; set; }
        }
        public class ShopeeInitLogisticDropOffDetailData
        {
            public long branch_id { get; set; }
            public string sender_real_name { get; set; }
            public string tracking_no { get; set; }
        }
        public class ShopeeInitLogisticNotIntegratedDetailData
        {
            public string tracking_no { get; set; }
        }
        public class ShopeeInitLogisticResult
        {
            public string tracking_no { get; set; }
            public string tracking_number { get; set; }
            public string request_id { get; set; }
            public string msg { get; set; }
            public string error { get; set; }
        }

        public class ShopeeUpdateStockData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public long stock { get; set; }
        }

        public class ShopeeUpdateVariationStockData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public long variation_id { get; set; }
            public int stock { get; set; }
        }
        public class ShopeeError
        {
            public string msg { get; set; }
            public string request_id { get; set; }
            public string error { get; set; }
        }
        public class ShopeeGetLogisticsData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
        }

        public class ShopeeGetLogisticsResult
        {
            public ShopeeGetLogisticsLogistic[] logistics { get; set; }
            public string request_id { get; set; }
        }

        public class ShopeeGetLogisticsLogistic
        {
            public ShopeeGetLogisticsResultWeight_Limits weight_limits { get; set; }
            public bool has_cod { get; set; }
            public ShopeeGetLogisticsResultItem_Max_Dimension item_max_dimension { get; set; }
            public object[] sizes { get; set; }
            public string logistic_name { get; set; }
            public bool enabled { get; set; }
            public long logistic_id { get; set; }
            public string fee_type { get; set; }
        }

        public class ShopeeGetLogisticsResultWeight_Limits
        {
            public float item_min_weight { get; set; }
            public float item_max_weight { get; set; }
        }

        public class ShopeeGetLogisticsResultItem_Max_Dimension
        {
            public int width { get; set; }
            public int length { get; set; }
            public string unit { get; set; }
            public int height { get; set; }
        }
        public class ShopeeCancelOrderData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
            public string cancel_reason { get; set; }
        }
        public class ShopeeCancelOrderResult
        {
            public long modified_time { get; set; }
            public string request_id { get; set; }
            public string msg { get; set; }
            public string error { get; set; }
        }
        public class ShopeeGetVariation
        {
            public long item_id { get; set; }
            public long shopid { get; set; }
            public long partner_id { get; set; }
            public long timestamp { get; set; }
        }
        public class ShopeeInitTierVariation
        {
            public long item_id { get; set; }
            public ShopeeTierVariation[] tier_variation { get; set; }
            public ShopeeVariation[] variation { get; set; }
            public long shopid { get; set; }
            public long partner_id { get; set; }
            public long timestamp { get; set; }

        }

        public class ShopeeUpdateTierVariationIndex
        {
            public long item_id { get; set; }
            public ShopeeTierVariation[] tier_variation { get; set; }
            //public ShopeeUpdateVariation[] variation { get; set; }
            //public ShopeeVariation[] variation { get; set; }
            public object[] variation { get; set; }
            public long shopid { get; set; }
            public long partner_id { get; set; }
            public long timestamp { get; set; }
        }
        public class ShopeeUpdateTierVariationList
        {
            public long item_id { get; set; }
            public ShopeeTierVariation[] tier_variation { get; set; }
            public long shopid { get; set; }
            public long partner_id { get; set; }
            public long timestamp { get; set; }
        }

        public class ShopeeUpdateTierVariationResult
        {
            public long item_id { get; set; }
            public string request_id { get; set; }
        }

        public class ShopeeTierVariation
        {
            public string name { get; set; }
            public string[] options { get; set; }
        }
        public class ShopeeVariation
        {
            public int[] tier_index { get; set; }
            public int stock { get; set; }
            public float price { get; set; }
            public string variation_sku { get; set; }
        }
        public class ShopeeUpdateVariation
        {
            public int[] tier_index { get; set; }
            public int stock { get; set; }
            public float price { get; set; }
            public string variation_sku { get; set; }
            public long? variation_id { get; set; }
        }

        public class ShopeeProductData
        {
            public long category_id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public double price { get; set; }
            public int stock { get; set; }
            public string item_sku { get; set; }
            public List<ShopeeVariationClass> variations { get; set; }
            public List<ShopeeImageClass> images { get; set; }
            public List<ShopeeAttributeClass> attributes { get; set; }
            public List<ShopeeLogisticsClass> logistics { get; set; }
            public double weight { get; set; } // in kg
            public int package_length { get; set; }
            public int package_width { get; set; }
            public int package_height { get; set; }
            //public int days_to_ship { get; set; } // jika di remark, jadi 2 hari, jika diisi, minimal 7-15 hari
            //public object wholesales { get; set; }
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            //public string size_chart { get; set; }
            public string condition { get; set; }//NEW or USED

        }

        public class ShopeeUpdateProductData
        {
            public long item_id { get; set; }
            public long category_id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string item_sku { get; set; }
            //public object variations { get; set; }
            public List<ShopeeAttributeClass> attributes { get; set; }
            public List<ShopeeLogisticsClass> logistics { get; set; }
            public double weight { get; set; } // in kg
            public int package_length { get; set; }
            public int package_width { get; set; }
            public int package_height { get; set; }
            //public int days_to_ship { get; set; } // jika di remark, jadi 2 hari, jika diisi, minimal 7-15 hari
            //public object wholesales { get; set; }
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            //public string size_chart { get; set; }
            public string condition { get; set; }//NEW or USED

        }
        public class ShopeeVariationClass
        {
            public string name { get; set; }
            public int stock { get; set; }
            public double price { get; set; }
            public string variation_sku { get; set; }
        }
        public class ShopeeImageClass
        {
            public string url { get; set; }
        }
        public class ShopeeAttributeClass
        {
            public long attributes_id { get; set; }
            public string value { get; set; }

        }
        public class ShopeeLogisticsClass
        {
            public long logistic_id { get; set; }
            public bool enabled { get; set; }
            //public double shipping_fee { get; set; }//Only needed when logistics fee_type = CUSTOM_PRICE.
            //public long size_id { get; set; }//If specify logistic fee_type is SIZE_SELECTION size_id is required
            public bool is_free { get; set; }

        }

        public class ShopeeCreateProdResponse : ShopeeError
        {
            public long item_id { get; set; }
            public Item item { get; set; }
        }

        public class ShopeeCreatePromoRes : ShopeeError
        {
            public long discount_id { get; set; }
            public int count { get; set; }
            public List<ShopeePromoErrors> errors { get; set; }
        }
        public class ShopeePromoErrors
        {
            public long item_id { get; set; }
            public long variation_id { get; set; }
            public string error_msg { get; set; }

        }
        public class ShopeeDeletePromo : ShopeeError
        {
            public UInt64 discount_id { get; set; }
            public DateTime modify_time { get; set; }
            public List<ShopeePromoErrors> errors { get; set; }
        }
        public class Item
        {
            public List<ShopeeLogisticsClass> logistics { get; set; }
            public double original_price { get; set; }
            public double package_width { get; set; }
            public int cmt_count { get; set; }
            public double weight { get; set; }
            public long shopid { get; set; }
            public string currency { get; set; }
            public long create_time { get; set; }
            public int likes { get; set; }
            public List<string> images { get; set; }
            public int days_to_ship { get; set; }
            public double package_length { get; set; }
            public int stock { get; set; }
            public string status { get; set; }
            public long update_time { get; set; }
            public string description { get; set; }
            public int views { get; set; }
            public double price { get; set; }
            public int sales { get; set; }
            public long discount_id { get; set; }
            public object[] wholesales { get; set; }
            public string condition { get; set; }
            public double package_height { get; set; }
            public string name { get; set; }
            public double rating_star { get; set; }
            public string item_sku { get; set; }
            public ItemVariation[] variations { get; set; }
            public string size_chart { get; set; }
            public bool has_variation { get; set; }
            public List<Attribute> attributes { get; set; }
            public long category_id { get; set; }
        }

        public class Attribute
        {
            public string attribute_name { get; set; }
            public bool is_mandatory { get; set; }
            public long attribute_id { get; set; }
            public string attribute_value { get; set; }
            public string attribute_type { get; set; }
        }

        public class ShopeeAcceptBuyerCancelOrderData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
        }
        public class ShopeeUpdatePriceData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public float price { get; set; }
        }
        public class ShopeeUpdateImageData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public string[] images { get; set; }
        }
        public class ShopeeUpdateVariantionPriceData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public long variation_id { get; set; }
            public float price { get; set; }
        }
        public class ShopeeAddDiscountData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public string discount_name { get; set; }
            public long start_time { get; set; }
            public long end_time { get; set; }
            public List<ShopeeAddDiscountDataItems> items { get; set; }
        }
        public class ShopeeAddDiscountDataItems
        {
            public long item_id { get; set; }
            public float item_promotion_price { get; set; }
            public UInt32 purchase_limit { get; set; }
            public List<ShopeeAddDiscountDataItemsVariation> variations { get; set; }
        }
        public class ShopeeAddDiscountDataItemsVariation
        {
            public long variation_id { get; set; }
            public float variation_promotion_price { get; set; }
        }

        public class ShopeeAddDiscountItemData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long discount_id { get; set; }
            public List<ShopeeAddDiscountDataItems> items { get; set; }
        }
        public class ShopeeDeleteDiscountItemData : ShopeeError
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long discount_id { get; set; }
            public long item_id { get; set; }
            public long variation_id { get; set; }
            public List<ShopeePromoErrors> errors { get; set; }
        }
        public class ShopeeDeleteDiscountData : ShopeeError
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long discount_id { get; set; }
            public List<ShopeePromoErrors> errors { get; set; }
        }

        public class ShopeeGetAddressData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
        }
        public class ShopeeGetAirwayBill
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public bool is_batch { get; set; }
            public List<string> ordersn_list { get; set; } = new List<string>();
        }
        public class ShopeeGetTimeSlotData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long address_id { get; set; }
            public string ordersn { get; set; }
        }
        public class ShopeeGetBranchData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
        }

        public class ShopeeGetAddressResult
        {
            public string request_id { get; set; }
            public ShopeeGetAddressResultAddress_List[] address_list { get; set; }
        }

        public class ShopeeGetAddressResultAddress_List
        {
            public string town { get; set; }
            public string city { get; set; }
            public long address_id { get; set; }
            public string district { get; set; }
            public string country { get; set; }
            public string zipcode { get; set; }
            public string state { get; set; }
            public string address { get; set; }
        }

        public class ShopeeGetTimeSlotResult
        {
            public ShopeeGetTimeSlotResultPickup_Time[] pickup_time { get; set; }
            public string request_id { get; set; }
        }

        public class ShopeeGetTimeSlotResultPickup_Time
        {
            public long date { get; set; }
            public string date_string { get; set; }
            public string pickup_time_id { get; set; }
            public string time_text { get; set; }
        }

        public class GetPickupTimeSlotError
        {
            public string msg { get; set; }
            public string request_id { get; set; }
            public string error { get; set; }
        }

        public class ItemVariation
        {
            public string status { get; set; }
            public float original_price { get; set; }
            public long update_time { get; set; }
            public long create_time { get; set; }
            public long discount_id { get; set; }
            public string name { get; set; }
            public float price { get; set; }
            public string variation_sku { get; set; }
            public long variation_id { get; set; }
            public int stock { get; set; }
        }

        public class InitTierVariationResult
        {
            public long item_id { get; set; }
            public Variation_Id_List[] variation_id_list { get; set; }
            public string request_id { get; set; }
        }

        public class Variation_Id_List
        {
            public long variation_id { get; set; }
            public int[] tier_index { get; set; }
        }

        public class GetVariationResult
        {
            public long item_id { get; set; }
            public GetVariationResultTier_Variation[] tier_variation { get; set; }
            public GetVariationResultVariation[] variations { get; set; }
            public string request_id { get; set; }
        }

        public class GetVariationResultTier_Variation
        {
            public string name { get; set; }
            public string[] options { get; set; }
        }

        public class GetVariationResultVariation
        {
            public long variation_id { get; set; }
            public int[] tier_index { get; set; }
        }

        public class ShopeeAddTierVariation
        {
            public long item_id { get; set; }
            public ShopeeVariation[] variation { get; set; }
            public long shopid { get; set; }
            public long partner_id { get; set; }
            public long timestamp { get; set; }

        }
        public class ShopeeAddVariation
        {
            public long item_id { get; set; }
            public ShopeeNewVariation[] variations { get; set; }
            public long shopid { get; set; }
            public long partner_id { get; set; }
            public long timestamp { get; set; }

        }
        public class ShopeeNewVariation
        {

            public string name { get; set; }
            public int stock { get; set; }
            public float price { get; set; }
            public string variation_sku { get; set; }
        }

        //get msg

        public class RootobjectGetMsg
        {
            public string error { get; set; }
            public string message { get; set; }
            public string request_id { get; set; }
            public ResponseGetMsg response { get; set; }
        }

        public class ResponseGetMsg
        {
            public MessageGetMsg[] messages { get; set; }
            public Page_ResultGetMsg page_result { get; set; }
        }

        public class Page_ResultGetMsg
        {
            public string next_offset { get; set; }
            public int page_size { get; set; }
        }

        public class MessageGetMsg
        {
            public string message_id { get; set; }
            public long from_id { get; set; }
            public long to_id { get; set; }
            public long from_shop_id { get; set; }
            public long to_shop_id { get; set; }
            public string message_type { get; set; }
            public ContentGetMsg content { get; set; }
            public string conversation_id { get; set; }
            public long created_timestamp { get; set; }
            public string region { get; set; }
            public string status { get; set; }
            public int message_option { get; set; }
            public string source { get; set; }
        }

        public class ContentGetMsg
        {
            public int shop_id { get; set; }
            public Chat_Product_InfosGetMsg[] chat_product_infos { get; set; }
            public string voucher_id { get; set; }
            public string voucher_code { get; set; }
            public string source { get; set; }
            public string url { get; set; }
            public int file_server_id { get; set; }
            public string text { get; set; }
        }

        public class Chat_Product_InfosGetMsg
        {
            public long item_id { get; set; }
            public long shop_id { get; set; }
            public string name { get; set; }
            public string thumb_url { get; set; }
            public string price { get; set; }
            public int quantity { get; set; }
            public int snap_shop_id { get; set; }
            public string model_name { get; set; }
            public string price_before_discount { get; set; }
        }

        //end get msg

        //Conversation list

        public class Rootobject
        {
            public string error { get; set; }
            public string message { get; set; }
            public string request_id { get; set; }
            public Response response { get; set; }
        }

        public class Response
        {
            public Page_Result page_result { get; set; }
            public Conversation[] conversations { get; set; }
        }

        public class Page_Result
        {
            public long page_size { get; set; }
            public Next_Cursor next_cursor { get; set; }
            public bool more { get; set; }
        }

        public class Next_Cursor
        {
            public string next_message_time_nano { get; set; }
            public string conversation_id { get; set; }
        }

        public class Conversation
        {
            public string conversation_id { get; set; }
            public long to_id { get; set; }
            public string to_name { get; set; }
            public string to_avatar { get; set; }
            public long shop_id { get; set; }
            public long unread_count { get; set; }
            public bool pinned { get; set; }
            public string last_read_message_id { get; set; }
            public string latest_message_id { get; set; }
            public string latest_message_type { get; set; }
            public Latest_Message_Content latest_message_content { get; set; }
            public long latest_message_from_id { get; set; }
            public long last_message_timestamp { get; set; }
            public long last_message_option { get; set; }
            public string max_general_option_hide_time { get; set; }
        }

        public class Latest_Message_Content
        {
            public string text { get; set; }
        }

        //

        //add by nurul 21/9/2021
        public class ShopeeAPIDataChat
        {
            public string merchant_code { get; set; }
            public string API_client_username { get; set; }
            public string API_client_password { get; set; }
            public string API_secret_key { get; set; }
            public string mta_username_email_merchant { get; set; }
            public string mta_password_password_merchant { get; set; }
            public string token { get; set; }
            public string DatabasePathErasoft { get; set; }
            public string username { get; set; }
            public string no_cust { get; set; }
            public DateTime? tgl_expired { get; set; }
            public string refresh_token { get; set; }//add for api v2
            public DateTime? token_expired { get; set; }
            public string TOKEN_CHAT { get; set; }
            public string REFRESH_TOKEN_CHAT { get; set; }
            public DateTime? TOKEN_EXPIRED_CHAT { get; set; }//add 28 mei 2021, for shopee
            public string STATUS_API_CHAT { get; set; }
            public string STATUS_SYNC_CHAT { get; set; }
        }
        //end add by nurul 21/9/2021
    }
}