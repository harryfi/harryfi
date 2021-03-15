using System;
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
using Hangfire;
using Hangfire.SqlServer;

namespace MasterOnline.Controllers
{
    public class EightTwoCartControllerJob : Controller
    {
        //AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);//string auth = Base64Encode();

        // GET: EightTwoCart
        private string url = "dev.api.82cart.com";
        private string apiKey = "35e8ac7e833721d0bd7d5bc66cb75";
        private string apiCred = "dev82cart";
        private MoDbContext MoDbContext { get; set; }
        private ErasoftContext ErasoftDbContext { get; set; }
        private DatabaseSQL EDB;
        private string username;

        public EightTwoCartControllerJob()
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

        protected void SetupContext(E2CartAPIData data)
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

        public static long CurrentTimeMillis()
        {
            //        return (long)DateTime.Now.ToUniversalTime().Subtract(
            //new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            //).TotalMilliseconds;
            return (long)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        protected enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4
        }

        protected void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, E2CartAPIData iden, API_LOG_MARKETPLACE data)
        {
            try
            {
                switch (action)
                {
                    case api_status.Pending:
                        {
                            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.API_credential).FirstOrDefault();
                            var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                            {
                                CUST = arf01 != null ? arf01.CUST : iden.API_credential,
                                CUST_ATTRIBUTE_1 = iden.API_credential,
                                CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                                CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                                CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                                CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                                MARKETPLACE = "82Cart",
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

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_update_stok")]
        [NotifyOnFailed("Update Stok {obj} ke 82Cart gagal.")]
        public async Task<string> E2Cart_UpdateStock_82Cart(string DatabasePathErasoft, string brg, string no_cust, string log_ActionCategory, string log_ActionName, E2CartAPIData iden, string brg_mp, int qty, string uname)
        {
            string ret = "";
            SetupContext(iden);
            //var EDB = new DatabaseSQL(DatabasePathErasoft);
            //string EraServerName = EDB.GetServerName("sConn");

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            //handle log activity
            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = milis.ToString(),
            //    REQUEST_ACTION = "Update Stock",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = "Kode Barang : " + brg,
            //    REQUEST_ATTRIBUTE_2 = "MO Stock : " + Convert.ToString(qty), //updating to stock
            //    REQUEST_STATUS = "Pending",
            //};
            //var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, no_cust, currentLog, "82Cart");
            //handle log activity

            var qtyOnHand = new StokControllerJob(DatabasePathErasoft, username).GetQOHSTF08A(brg, "ALL");
            if (qtyOnHand < 0)
            {
                qtyOnHand = qty;
            }
            //else if(qtyOnHand == 0)
            //{
            //    qtyOnHand = qty;
            //}

            qty = Convert.ToInt32(qtyOnHand);

            string urll = string.Format("{0}/api/v1/editProductdetail", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            string[] brg_mp_split = brg_mp.Split(';');

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            if (brg_mp_split[1] == "0")
            {
                postData += "&id_product=" + Uri.EscapeDataString(brg_mp_split[0]);
                postData += "&quantity=" + Uri.EscapeDataString(qty.ToString());
            }
            else
            {
                postData += "&id_product=" + Uri.EscapeDataString(brg_mp_split[0]);
                postData += "&id_product_attribute=" + Uri.EscapeDataString(brg_mp_split[1]);
                postData += "&quantity_attribute=" + Uri.EscapeDataString(qty.ToString());
            }
            postData += "&available_for_order=" + Uri.EscapeDataString("1");
            //postData += "&id_product=" + Uri.EscapeDataString(brg_mp_split[0]);
            ////postData += "&id_product_attribute=" + Uri.EscapeDataString(brg_mp_split[1]);
            //postData += "&stock=" + Uri.EscapeDataString(qty.ToString());

            var data = Encoding.ASCII.GetBytes(postData);

            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = data.Length;

            try
            {
                string responseFromServer = "";

                using (var stream = myReq.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream2 = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream2);
                        responseFromServer = reader.ReadToEnd();
                    }
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    var resultAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ResultUpdateStock82Cart)) as ResultUpdateStock82Cart;
                    if (resultAPI.error != "none" && resultAPI.error != null)
                    {
                        //currentLog.REQUEST_ATTRIBUTE_3 = "Exception"; //marketplace stock
                        //currentLog.REQUEST_EXCEPTION = resultAPI.error.ToString();
                        //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, no_cust, currentLog, "82Cart");
                        throw new Exception(resultAPI.error.ToString());
                    }
                    //else
                    //{
                    //    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, no_cust, currentLog, "82Cart");
                    //}
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                //currentLog.REQUEST_ATTRIBUTE_3 = "Exception"; //marketplace stock
                //currentLog.REQUEST_EXCEPTION = msg;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, no_cust, currentLog, "82Cart");
                throw new Exception(msg);
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke 82Cart Gagal.")]
        public async Task<string> E2Cart_CreateProduct(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, E2CartAPIData iden)
        {
            string ret = "";
            SetupContext(iden);

            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper()).FirstOrDefault();
            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == log_CUST.ToUpper()).FirstOrDefault();
            if (brgInDb == null || marketplace == null)
                return "invalid passing data";
            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum && b.DISPLAY == true).FirstOrDefault();
            if (detailBrg == null)
                return "invalid passing data";

            var categoryID = "";
            var categoryIDDefault = "";
            var attributeIDGroup = "";
            var attributeIDItems = "";

            var listattributeIDGroup = "";
            var listattributeIDItems = "";


            if (detailBrg != null)
            {
                if (detailBrg.ACODE_1 != null)
                {
                    categoryID = categoryID + detailBrg.ACODE_1 + ",";
                }
                if (detailBrg.ACODE_2 != null)
                {
                    categoryID = categoryID + detailBrg.ACODE_2 + ",";
                }
                if (detailBrg.ACODE_3 != null)
                {
                    categoryID = categoryID + detailBrg.ACODE_3 + ",";
                }
                if (detailBrg.ACODE_4 != null)
                {
                    categoryID = categoryID + detailBrg.ACODE_4 + ",";
                }

                if (detailBrg.ACODE_10 != null)
                {
                    attributeIDGroup = detailBrg.ACODE_10;
                }
                if (detailBrg.ACODE_11 != null)
                {
                    attributeIDItems = detailBrg.ACODE_11;
                }

                if (detailBrg.CATEGORY_CODE != null)
                {
                    categoryIDDefault = detailBrg.CATEGORY_CODE;
                }
            }

            if (!string.IsNullOrEmpty(categoryID))
            {
                categoryID = categoryID.Length > 0 ? categoryID.Substring(0, categoryID.Length - 1) : "2,3";
            }

            var weight = Convert.ToDouble(brgInDb.BERAT / 1000);

            string urll = string.Format("{0}/api/v1/addProduct", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            if (!string.IsNullOrEmpty(brgInDb.NAMA2))
            {
                postData += "&name=" + Uri.EscapeDataString(brgInDb.NAMA + " " + brgInDb.NAMA2);
            }
            else
            {
                postData += "&name=" + Uri.EscapeDataString(brgInDb.NAMA);
            }
            postData += "&reference=" + Uri.EscapeDataString(brgInDb.BRG);
            postData += "&active=" + Uri.EscapeDataString("1");
            postData += "&visibility=" + Uri.EscapeDataString("both");
            postData += "&available_for_order=" + Uri.EscapeDataString("1");
            postData += "&show_price=" + Uri.EscapeDataString("1");
            postData += "&online_only=" + Uri.EscapeDataString("0");
            postData += "&condition=" + Uri.EscapeDataString("new");
            postData += "&wholesale_price=" + Uri.EscapeDataString("0");
            postData += "&price=" + Uri.EscapeDataString(detailBrg.HJUAL.ToString());
            postData += "&on_sale=" + Uri.EscapeDataString("1");
            postData += "&link_rewrite=" + Uri.EscapeDataString(brgInDb.NAMA.Replace(" ", "-").Replace("+", "plus").ToLower());
            postData += "&width=" + Uri.EscapeDataString(brgInDb.LEBAR.ToString());
            postData += "&height=" + Uri.EscapeDataString(brgInDb.TINGGI.ToString());
            postData += "&depth=" + Uri.EscapeDataString("0");
            postData += "&weight=" + Uri.EscapeDataString(weight.ToString());
            postData += "&additional_shipping_cost=" + Uri.EscapeDataString("0");
            postData += "&minimal_quantity=" + Uri.EscapeDataString(brgInDb.MINI.ToString());
            postData += "&out_of_stock=" + Uri.EscapeDataString("0");
            postData += "&id_category_default=" + Uri.EscapeDataString(categoryIDDefault.ToString());
            postData += "&category=" + Uri.EscapeDataString("[" + categoryID.ToString() + "]");
            postData += "&id_manufacturer=" + Uri.EscapeDataString(detailBrg.AVALUE_38.ToString());

            //Start handle Check Category
            EightTwoCartControllerJob.E2CartAPIData dataLocal = new EightTwoCartControllerJob.E2CartAPIData
            {
                username = iden.username,
                no_cust = iden.no_cust,
                API_key = iden.API_key,
                API_credential = iden.API_credential,
                API_url = iden.API_url,
                ID_MARKET = iden.ID_MARKET,
                DatabasePathErasoft = iden.DatabasePathErasoft
            };
            EightTwoCartControllerJob c82CartController = new EightTwoCartControllerJob();

            //Start handle description
            var vDescription = brgInDb.Deskripsi;
            vDescription = new StokControllerJob().RemoveSpecialCharacters(vDescription);

            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
            //vDescription = vDescription.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
            vDescription = vDescription.Replace("<li>", "- ").Replace("</li>", "\r\n");
            vDescription = vDescription.Replace("<ul>", "").Replace("</ul>", "\r\n");
            vDescription = vDescription.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
            vDescription = vDescription.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
            //vDescription = vDescription.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");
            //end add by nurul 20/1/2020, handle <p> dan enter double di shopee

            //add by calvin 10 september 2019
            vDescription = vDescription.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
            vDescription = vDescription.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
            vDescription = vDescription.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
            //vDescription = vDescription.Replace("<p>", "\r\n").Replace("</p>", "\r\n");
            //HttpBody.description = HttpBody.description.Replace("<li>", "- ").Replace("</li>", "\r\n");
            //HttpBody.description = HttpBody.description.Replace("&nbsp;", "");

            //vDescription = System.Text.RegularExpressions.Regex.Replace(vDescription, "<.*?>", String.Empty);
            //end add by calvin 10 september 2019

            postData += "&short_description=" + Uri.EscapeDataString(vDescription);
            //postData += "&description=" + Uri.EscapeDataString(vDescription);
            //end handle description

            //handle image
            List<string> lGambarUploaded = new List<string>();

            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
            {
                lGambarUploaded.Add(brgInDb.LINK_GAMBAR_1);
            }
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
            {
                lGambarUploaded.Add(brgInDb.LINK_GAMBAR_2);
            }
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
            {
                lGambarUploaded.Add(brgInDb.LINK_GAMBAR_3);
            }
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_4))
            {
                lGambarUploaded.Add(brgInDb.LINK_GAMBAR_4);
            }
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_5))
            {
                lGambarUploaded.Add(brgInDb.LINK_GAMBAR_5);
            }
            //end handle image

            //start handle stock
            double qty_stock = 1;
            //qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(kodeProduk, "ALL");
            //if (qty_stock > 0)
            //{
            //    //postData += "&quantity=" + Uri.EscapeDataString(qty_stock.ToString());
            //}
            //else
            //{
                qty_stock = brgInDb.ISI;
            //}
            //end handle stock

            var data = Encoding.ASCII.GetBytes(postData);

            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = data.Length;

            try
            {
                string responseFromServer = "";

                using (var stream = myReq.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream2 = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream2);
                        responseFromServer = reader.ReadToEnd();
                    }
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    var resultAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ResultCreateProduct82Cart)) as ResultCreateProduct82Cart;

                    if (resultAPI != null)
                    {
                        if (resultAPI.error == "none" && resultAPI.results == "success")
                        {
                            if (resultAPI.data.data[0].id_product != null)
                            {
                                var item = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum).SingleOrDefault();
                                if (item != null)
                                {
                                    item.BRG_MP = Convert.ToString(resultAPI.data.data[0].id_product) + ";0";
                                    item.LINK_STATUS = "Buat Produk Berhasil";
                                    item.LINK_DATETIME = DateTime.UtcNow.AddHours(7);
                                    item.LINK_ERROR = "0;Buat Produk;;";
                                    ErasoftDbContext.SaveChanges();
                                }

                                if (brgInDb.TYPE == "4") // punya variasi
                                {
                                    //handle variasi product
                                    #region variasi product
                                    var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == kodeProduk).ToList();
                                    var var_strukturVar = ErasoftDbContext.STF02I.Where(p => p.BRG == kodeProduk && p.MARKET == "82CART").ToList().OrderBy(p => p.RECNUM);

                                    foreach (var itemData in var_stf02)
                                    {
                                        #region varian LV1
                                        if (!string.IsNullOrEmpty(itemData.Sort8))
                                        {
                                            var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == itemData.Sort8).FirstOrDefault();
                                            listattributeIDGroup = variant_id_group.MP_JUDUL_VAR + ",";
                                            listattributeIDItems = variant_id_group.MP_VALUE_VAR + ",";
                                            //foreach (var itemVar in variant_id_items)
                                            //{
                                                //var dataBRGItem = var_stf02.Where(p => p.Sort8 == itemVar.KODE_VAR).FirstOrDefault();
                                                //c82CartController.E2Cart_AddAttributeProduct(dataLocal, itemData.BRG, item.BRG_MP, attributeIDGroup, attributeIDItems, weight.ToString(), detailBrg.HJUAL.ToString(), Convert.ToInt32(qty_stock), dataBRGItem.LINK_GAMBAR_1.ToString());

                                            //}
                                        }
                                        #endregion

                                        #region varian LV2
                                        if (!string.IsNullOrEmpty(itemData.Sort9))
                                        {
                                            var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == itemData.Sort9).FirstOrDefault();
                                            listattributeIDGroup = listattributeIDGroup + variant_id_group.MP_JUDUL_VAR + ",";
                                            listattributeIDItems = listattributeIDItems + variant_id_group.MP_VALUE_VAR + ",";
                                            //foreach (var itemVar in variant_id_items)
                                            //{
                                            //var dataBRGItem = var_stf02.Where(p => p.Sort8 == itemVar.KODE_VAR).FirstOrDefault();
                                            //c82CartController.E2Cart_AddAttributeProduct(dataLocal, itemData.BRG, item.BRG_MP, attributeIDGroup, attributeIDItems, weight.ToString(), detailBrg.HJUAL.ToString(), Convert.ToInt32(qty_stock), dataBRGItem.LINK_GAMBAR_1.ToString());

                                            //}
                                        }
                                        #endregion

                                        #region varian LV3
                                        if (!string.IsNullOrEmpty(itemData.Sort10))
                                        {
                                            var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 3 && p.KODE_VAR == itemData.Sort10).FirstOrDefault();
                                            listattributeIDGroup = listattributeIDGroup + variant_id_group.MP_JUDUL_VAR + ",";
                                            listattributeIDItems = listattributeIDItems + variant_id_group.MP_VALUE_VAR + ",";
                                            //foreach (var itemVar in variant_id_items)
                                            //{
                                            //var dataBRGItem = var_stf02.Where(p => p.Sort8 == itemVar.KODE_VAR).FirstOrDefault();
                                            //c82CartController.E2Cart_AddAttributeProduct(dataLocal, itemData.BRG, item.BRG_MP, attributeIDGroup, attributeIDItems, weight.ToString(), detailBrg.HJUAL.ToString(), Convert.ToInt32(qty_stock), dataBRGItem.LINK_GAMBAR_1.ToString());
                                            //}
                                        }
                                        #endregion

                                        listattributeIDGroup = listattributeIDGroup.Substring(0, listattributeIDGroup.Length - 1);
                                        listattributeIDItems = listattributeIDItems.Substring(0, listattributeIDItems.Length - 1);

                                        await c82CartController.E2Cart_AddAttributeProduct(dataLocal, itemData.BRG, item.BRG_MP, listattributeIDGroup, listattributeIDItems, weight.ToString(), detailBrg.HJUAL.ToString(), Convert.ToInt32(qty_stock), itemData.LINK_GAMBAR_1.ToString());

                                        listattributeIDGroup = "";
                                        listattributeIDItems = "";
                                    }



                                    #endregion
                                    //end handle variasi product
                                }

                                //handle attribute product
                                //Task.Run(() => c82CartController.E2Cart_AddAttributeProduct(dataLocal, item.BRG_MP, attributeIDGroup, attributeIDItems, brgInDb.BERAT.ToString())).Wait();
                                //end handle attribute product

                                //handle all image was uploaded
                                foreach (var images in lGambarUploaded)
                                {
                                    Task.Run(() => c82CartController.E2Cart_AddImageProduct(dataLocal, item.BRG_MP, images)).Wait(); 
                                }
                                //end handle all image was uploaded

                                //start handle update stock for default product

#if (DEBUG || Debug_AWS)
                                Task.Run(() => c82CartController.E2Cart_UpdateStock_82Cart(dbPathEra, item.BRG, iden.no_cust, "Stock", "Update Stok", dataLocal, item.BRG_MP, Convert.ToInt32(qty_stock), iden.username)).Wait();
#else
                                var EDB = new DatabaseSQL(dbPathEra);
                                string EDBConnID = EDB.GetConnectionString("ConnId");
                                var sqlStorage = new SqlServerStorage(EDBConnID);
                                var client = new BackgroundJobClient(sqlStorage);
                                client.Enqueue<EightTwoCartControllerJob>(x => x.E2Cart_UpdateStock_82Cart(dbPathEra, item.BRG, iden.no_cust, "Stock", "Update Stok", dataLocal, item.BRG_MP, Convert.ToInt32(qty_stock), iden.username));
                                //end handle update stock for default product
#endif
                                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            }
                        }
                        else
                        {
                            if (resultAPI.error != null && resultAPI.error != "none")
                            {
                                throw new Exception(responseFromServer);
                                //currentLog.REQUEST_EXCEPTION = resultAPI.error.ToString();
                                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Product {obj} ke 82Cart Gagal.")]
        public async Task<string> E2Cart_UpdateProduct(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, E2CartAPIData iden)
        {
            string ret = "";
            SetupContext(iden);

            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper()).FirstOrDefault();
            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == log_CUST.ToUpper()).FirstOrDefault();
            if (brgInDb == null || marketplace == null)
                return "invalid passing data";
            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum && b.DISPLAY == true).FirstOrDefault();
            if (detailBrg == null)
                return "invalid passing data";

            var categoryID = "";
            var categoryIDDefault = "";
            var attributeIDGroup = "";
            var attributeIDItems = "";

            var listattributeIDGroup = "";
            var listattributeIDItems = "";

            if (detailBrg != null)
            {
                if (detailBrg.ACODE_1 != null)
                {
                    categoryID = categoryID + detailBrg.ACODE_1 + ",";
                }
                if (detailBrg.ACODE_2 != null)
                {
                    categoryID = categoryID + detailBrg.ACODE_2 + ",";
                }
                if (detailBrg.ACODE_3 != null)
                {
                    categoryID = categoryID + detailBrg.ACODE_3 + ",";
                }
                if (detailBrg.ACODE_4 != null)
                {
                    categoryID = categoryID + detailBrg.ACODE_4 + ",";
                }

                if (detailBrg.ACODE_10 != null)
                {
                    attributeIDGroup = detailBrg.ACODE_10;
                }
                if (detailBrg.ACODE_11 != null)
                {
                    attributeIDItems = detailBrg.ACODE_11;
                }

                if (detailBrg.CATEGORY_CODE != null)
                {
                    categoryIDDefault = detailBrg.CATEGORY_CODE;
                }
            }

            if (!string.IsNullOrEmpty(categoryID))
            {
                categoryID = categoryID.Substring(0, categoryID.Length - 1);
            }

            var weight = Convert.ToDouble(brgInDb.BERAT / 1000);

            string urll = string.Format("{0}/api/v1/editProduct", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            string[] splitBrg = detailBrg.BRG_MP.Split(';');

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            postData += "&id_product=" + Uri.EscapeDataString(splitBrg[0]);
            if (!string.IsNullOrEmpty(brgInDb.NAMA2))
            {
                postData += "&name=" + Uri.EscapeDataString(brgInDb.NAMA + " " + brgInDb.NAMA2);
            }
            else
            {
                postData += "&name=" + Uri.EscapeDataString(brgInDb.NAMA);
            }
            postData += "&reference=" + Uri.EscapeDataString(brgInDb.BRG.Replace(";", ""));
            postData += "&active=" + Uri.EscapeDataString("1");
            postData += "&visibility=" + Uri.EscapeDataString("both");
            postData += "&available_for_order=" + Uri.EscapeDataString("1");
            postData += "&show_price=" + Uri.EscapeDataString("1");
            postData += "&online_only=" + Uri.EscapeDataString("0");
            postData += "&condition=" + Uri.EscapeDataString("new");
            postData += "&wholesale_price=" + Uri.EscapeDataString("0");
            postData += "&price=" + Uri.EscapeDataString(detailBrg.HJUAL.ToString());
            postData += "&on_sale=" + Uri.EscapeDataString("1");
            postData += "&link_rewrite=" + Uri.EscapeDataString(brgInDb.NAMA.Replace(" ", "-").Replace("+", "plus").ToLower());
            postData += "&width=" + Uri.EscapeDataString(brgInDb.LEBAR.ToString());
            postData += "&height=" + Uri.EscapeDataString(brgInDb.TINGGI.ToString());
            postData += "&depth=" + Uri.EscapeDataString("0");
            postData += "&weight=" + Uri.EscapeDataString(weight.ToString());
            postData += "&additional_shipping_cost=" + Uri.EscapeDataString("0");
            postData += "&minimal_quantity=" + Uri.EscapeDataString(brgInDb.MINI.ToString());
            postData += "&out_of_stock=" + Uri.EscapeDataString("0");
            postData += "&id_category_default=" + Uri.EscapeDataString(categoryIDDefault.ToString());
            postData += "&category=" + Uri.EscapeDataString("[" + categoryID.ToString() + "]");
            postData += "&id_manufacturer=" + Uri.EscapeDataString(detailBrg.AVALUE_38.ToString());

            //Start handle Check Category
            EightTwoCartControllerJob.E2CartAPIData dataLocal = new EightTwoCartControllerJob.E2CartAPIData
            {
                username = iden.username,
                no_cust = iden.no_cust,
                API_key = iden.API_key,
                API_credential = iden.API_credential,
                API_url = iden.API_url,
                ID_MARKET = iden.ID_MARKET,
                DatabasePathErasoft = iden.DatabasePathErasoft
            };
            EightTwoCartControllerJob c82CartController = new EightTwoCartControllerJob();

            //Start handle description
            var vDescription = brgInDb.Deskripsi;
            vDescription = new StokControllerJob().RemoveSpecialCharacters(vDescription);

            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
            //vDescription = vDescription.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
            vDescription = vDescription.Replace("<li>", "- ").Replace("</li>", "\r\n");
            vDescription = vDescription.Replace("<ul>", "").Replace("</ul>", "\r\n");
            vDescription = vDescription.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
            vDescription = vDescription.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
            //vDescription = vDescription.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");
            //end add by nurul 20/1/2020, handle <p> dan enter double di shopee

            //add by calvin 10 september 2019
            vDescription = vDescription.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
            vDescription = vDescription.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
            vDescription = vDescription.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
            //vDescription = vDescription.Replace("<p>", "\r\n").Replace("</p>", "\r\n");
            //HttpBody.description = HttpBody.description.Replace("<li>", "- ").Replace("</li>", "\r\n");
            //HttpBody.description = HttpBody.description.Replace("&nbsp;", "");

            //vDescription = System.Text.RegularExpressions.Regex.Replace(vDescription, "<.*?>", String.Empty);
            //end add by calvin 10 september 2019

            postData += "&short_description=" + Uri.EscapeDataString(vDescription);
            //postData += "&description=" + Uri.EscapeDataString(vDescription);
            //end handle description

            //handle image
            List<string> lGambarUploaded = new List<string>();

            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
            {
                lGambarUploaded.Add(brgInDb.LINK_GAMBAR_1);
            }
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
            {
                lGambarUploaded.Add(brgInDb.LINK_GAMBAR_2);
            }
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
            {
                lGambarUploaded.Add(brgInDb.LINK_GAMBAR_3);
            }
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_4))
            {
                lGambarUploaded.Add(brgInDb.LINK_GAMBAR_4);
            }
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_5))
            {
                lGambarUploaded.Add(brgInDb.LINK_GAMBAR_5);
            }
            
            if(lGambarUploaded.Count() > 0)
            {
                var tempListBarang = c82CartController.E2Cart_GetProduct_ForListImage(dataLocal, detailBrg.BRG_MP);
            }

            //end handle image

            //start handle stock
            double qty_stock = 1;
            //qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(kodeProduk, "ALL");
            //if (qty_stock > 0)
            //{
            //    //postData += "&quantity=" + Uri.EscapeDataString(qty_stock.ToString());
            //}
            //else
            //{
            qty_stock = brgInDb.ISI;
            //}
            //end handle stock

            var data = Encoding.ASCII.GetBytes(postData);

            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = data.Length;

            try
            {
                string responseFromServer = "";

                using (var stream = myReq.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (WebResponse response = myReq.GetResponse())
                {
                    using (Stream stream2 = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream2);
                        responseFromServer = reader.ReadToEnd();
                    }
                }

                if (!string.IsNullOrEmpty(responseFromServer))
                {
                    var resultAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ResultCreateProduct82Cart)) as ResultCreateProduct82Cart;

                    if (resultAPI != null)
                    {
                        if (resultAPI.error == "none" && resultAPI.results == "success")
                        {
                            if (resultAPI.data != null)
                            {
                                if (brgInDb.TYPE == "4") // punya variasi
                                {
                                    //handle variasi product
                                    #region variasi product
                                    var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == kodeProduk).ToList();
                                    var var_strukturVar = ErasoftDbContext.STF02I.Where(p => p.BRG == kodeProduk && p.MARKET == "82CART").ToList().OrderBy(p => p.RECNUM);
                                    //var brgMP = "";

                                    //if(splitBrg[1] == "0")
                                    //{
                                    //    brgMP = splitBrg[1];
                                    //}
                                    //else
                                    //{
                                    //    brgMP = splitBrg[1];
                                    //}

                                    foreach (var itemData in var_stf02)
                                    {
                                        #region varian LV1
                                        if (!string.IsNullOrEmpty(itemData.Sort8))
                                        {
                                            var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == itemData.Sort8).FirstOrDefault();
                                            listattributeIDGroup = variant_id_group.MP_JUDUL_VAR + ",";
                                            listattributeIDItems = variant_id_group.MP_VALUE_VAR + ",";
                                            //foreach (var itemVar in variant_id_items)
                                            //{
                                            //var dataBRGItem = var_stf02.Where(p => p.Sort8 == itemVar.KODE_VAR).FirstOrDefault();
                                            //c82CartController.E2Cart_AddAttributeProduct(dataLocal, itemData.BRG, item.BRG_MP, attributeIDGroup, attributeIDItems, weight.ToString(), detailBrg.HJUAL.ToString(), Convert.ToInt32(qty_stock), dataBRGItem.LINK_GAMBAR_1.ToString());

                                            //}
                                        }
                                        #endregion

                                        #region varian LV2
                                        if (!string.IsNullOrEmpty(itemData.Sort9))
                                        {
                                            var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == itemData.Sort9).FirstOrDefault();
                                            listattributeIDGroup = listattributeIDGroup + variant_id_group.MP_JUDUL_VAR + ",";
                                            listattributeIDItems = listattributeIDItems + variant_id_group.MP_VALUE_VAR + ",";
                                            //foreach (var itemVar in variant_id_items)
                                            //{
                                            //var dataBRGItem = var_stf02.Where(p => p.Sort8 == itemVar.KODE_VAR).FirstOrDefault();
                                            //c82CartController.E2Cart_AddAttributeProduct(dataLocal, itemData.BRG, item.BRG_MP, attributeIDGroup, attributeIDItems, weight.ToString(), detailBrg.HJUAL.ToString(), Convert.ToInt32(qty_stock), dataBRGItem.LINK_GAMBAR_1.ToString());

                                            //}
                                        }
                                        #endregion

                                        #region varian LV3
                                        if (!string.IsNullOrEmpty(itemData.Sort10))
                                        {
                                            var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 3 && p.KODE_VAR == itemData.Sort10).FirstOrDefault();
                                            listattributeIDGroup = listattributeIDGroup + variant_id_group.MP_JUDUL_VAR + ",";
                                            listattributeIDItems = listattributeIDItems + variant_id_group.MP_VALUE_VAR + ",";
                                            //foreach (var itemVar in variant_id_items)
                                            //{
                                            //var dataBRGItem = var_stf02.Where(p => p.Sort8 == itemVar.KODE_VAR).FirstOrDefault();
                                            //c82CartController.E2Cart_AddAttributeProduct(dataLocal, itemData.BRG, item.BRG_MP, attributeIDGroup, attributeIDItems, weight.ToString(), detailBrg.HJUAL.ToString(), Convert.ToInt32(qty_stock), dataBRGItem.LINK_GAMBAR_1.ToString());
                                            //}
                                        }
                                        #endregion

                                        listattributeIDGroup = listattributeIDGroup.Substring(0, listattributeIDGroup.Length - 1);
                                        listattributeIDItems = listattributeIDItems.Substring(0, listattributeIDItems.Length - 1);

                                        int idMarket = Convert.ToInt32(iden.ID_MARKET);

                                        var var_stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == itemData.BRG && p.IDMARKET == idMarket).SingleOrDefault().BRG_MP;
                                        if (!string.IsNullOrEmpty(var_stf02h))
                                        {
                                            string[] splitBrgMP = var_stf02h.Split(';');

                                            if (splitBrgMP[1] != "0")
                                            {
                                                c82CartController.E2Cart_UpdateAttributeProduct(dataLocal, itemData.BRG, var_stf02h, listattributeIDGroup, listattributeIDItems, weight.ToString(), "0", Convert.ToInt32(qty_stock), itemData.LINK_GAMBAR_1.ToString());
                                            }
                                            else
                                            {
                                                c82CartController.E2Cart_AddAttributeProduct(dataLocal, itemData.BRG, var_stf02h, listattributeIDGroup, listattributeIDItems, weight.ToString(), "0", Convert.ToInt32(qty_stock), itemData.LINK_GAMBAR_1.ToString());
                                            }
                                        }
                                        else
                                        {
                                            c82CartController.E2Cart_AddAttributeProduct(dataLocal, itemData.BRG, detailBrg.BRG_MP, listattributeIDGroup, listattributeIDItems, weight.ToString(), "0", Convert.ToInt32(qty_stock), itemData.LINK_GAMBAR_1.ToString());
                                        }

                                        listattributeIDGroup = "";
                                        listattributeIDItems = "";

                                    }

                                    #endregion
                                    //end handle variasi product
                                }
                                //handle attribute product
                                //c82CartController.E2Cart_AddAttributeProduct(dataLocal, detailBrg.BRG_MP, attributeIDGroup, attributeIDItems, brgInDb.BERAT.ToString());
                                //Task.Run(() => 
                                //end handle attribute product

                                //handle all image was uploaded
                                foreach (var images in lGambarUploaded)
                                {
                                    c82CartController.E2Cart_AddImageProduct(dataLocal, detailBrg.BRG_MP, images);
                                    //Task.Run(() => 
                                }
                                //end handle all image was uploaded

                                //start handle update stock for default product

#if (DEBUG || Debug_AWS)
                                Task.Run(() => c82CartController.E2Cart_UpdateStock_82Cart(dbPathEra, detailBrg.BRG, iden.no_cust, "Stock", "Update Stok", dataLocal, detailBrg.BRG_MP, Convert.ToInt32(qty_stock), iden.username)).Wait();
#else
                                var EDB = new DatabaseSQL(dbPathEra);
                                string EDBConnID = EDB.GetConnectionString("ConnId");
                                var sqlStorage = new SqlServerStorage(EDBConnID);
                                var client = new BackgroundJobClient(sqlStorage);
                                client.Enqueue<EightTwoCartControllerJob>(x => x.E2Cart_UpdateStock_82Cart(dbPathEra, detailBrg.BRG, iden.no_cust, "Stock", "Update Stok", dataLocal, detailBrg.BRG_MP, Convert.ToInt32(qty_stock), iden.username));
                                //end handle update stock for default product
#endif
                                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            }
                        }
                        else
                        {
                            if (resultAPI.error != null && resultAPI.error != "none")
                            {
                                throw new Exception(responseFromServer);
                                //currentLog.REQUEST_EXCEPTION = resultAPI.error.ToString();
                                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            return ret;
        }

        //Update Product Attribute & Image.
        public async Task<string> E2Cart_UpdateAttributeProduct(E2CartAPIData iden, string kodeBarang, string brg_mp, string attributeIDProduct, string attributeItems, string weight, string price, int qty, string urlImage)
        {
            SetupContext(iden);
            string[] brg_mp_split = brg_mp.Split(';');
            string urll = string.Format("{0}/api/v1/editProductAttribute", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            postData += "&id_product_attribute=" + Uri.EscapeDataString(brg_mp_split[1]);
            postData += "&id_attribute=" + Uri.EscapeDataString("[" + attributeItems + "]");
            //postData += "&weight=" + Uri.EscapeDataString(weight); remark for default 0 karena ada impact berat ke induk dari berat varian.
            //postData += "&price=" + Uri.EscapeDataString(price); remark for default 0 karena ada impact harga ke induk dari harga varian.
            postData += "&weight=" + Uri.EscapeDataString("0");
            postData += "&price=" + Uri.EscapeDataString("0");
            postData += "&reference=" + Uri.EscapeDataString(kodeBarang);
            postData += "&minimal_quantity=" + Uri.EscapeDataString("1");
            postData += "&wholesale_price=" + Uri.EscapeDataString("0");
            if (!string.IsNullOrEmpty(urlImage))
            {
                postData += "&image_url=" + Uri.EscapeDataString(urlImage);
            }


            var data = Encoding.ASCII.GetBytes(postData);

            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = data.Length;

            using (var stream = myReq.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)myReq.GetResponse();

            var responseServer = new StreamReader(response.GetResponseStream()).ReadToEnd();

            if (!string.IsNullOrEmpty(responseServer))
            {
                //var resServer = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer);
                var resultAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(ResultAddAttribute)) as ResultAddAttribute;

                if (resultAPI != null)
                {
                    if (resultAPI.error == "none" && resultAPI.results == "success")
                    {
                        if (resultAPI.data != null)
                        {
                            var idAttribute = resultAPI.data.id;
                            string Link_Error = "0;Update Produk;;";
                            var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(brg_mp_split[0] + ";" + idAttribute) + "',LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '" + Link_Error + "' WHERE BRG = '" + Convert.ToString(kodeBarang) + "' AND IDMARKET = '" + Convert.ToString(iden.ID_MARKET) + "'");
                            //var e2CartController = new EightTwoCartControllerJob();
                            //E2Cart_AddImageProduct_varian(iden, brg_mp_split[0], Convert.ToString(idAttribute), urlImage);

#if (DEBUG || Debug_AWS)
                            Task.Run(() => E2Cart_UpdateStock_82Cart(iden.DatabasePathErasoft, kodeBarang, iden.no_cust, "Stock", "Update Stok", iden, Convert.ToString(brg_mp_split[0] + ";" + idAttribute), Convert.ToInt32(qty), iden.username)).Wait();
#else
                            var EDB2 = new DatabaseSQL(iden.DatabasePathErasoft);
                            string EDBConnID = EDB2.GetConnectionString("ConnId");
                            var sqlStorage = new SqlServerStorage(EDBConnID);
                            var client = new BackgroundJobClient(sqlStorage);
                            client.Enqueue<EightTwoCartControllerJob>(x => x.E2Cart_UpdateStock_82Cart(iden.DatabasePathErasoft, kodeBarang, iden.no_cust, "Stock", "Update Stok", iden, Convert.ToString(brg_mp_split[0] + ";" + idAttribute), Convert.ToInt32(qty), iden.username));
                            //end handle update stock for default product
#endif
                        }
                    }
                }
                //ViewBag.response = resServer;
            }

            return "";
        }

        //Add Product Attribute & Image.
        public async Task<string> E2Cart_AddAttributeProduct(E2CartAPIData iden, string kodeBarang, string brg_mp, string attributeGroup, string attributeItems, string weight, string price, int qty, string urlImage)
        {
            SetupContext(iden);
            string[] brg_mp_split = brg_mp.Split(';');
            string urll = string.Format("{0}/api/v1/addProductAttribute", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            postData += "&id_product=" + Uri.EscapeDataString(brg_mp_split[0]);
            postData += "&id_attribute_group=" + Uri.EscapeDataString(attributeGroup);
            postData += "&id_attribute=" + Uri.EscapeDataString("[" + attributeItems + "]");
            //postData += "&weight=" + Uri.EscapeDataString(weight); remark for default 0 karena ada impact berat ke induk dari berat varian.
            //postData += "&price=" + Uri.EscapeDataString(price); remark for default 0 karena ada impact harga ke induk dari harga varian.
            postData += "&weight=" + Uri.EscapeDataString("0");
            postData += "&price=" + Uri.EscapeDataString("0");
            postData += "&reference=" + Uri.EscapeDataString(kodeBarang);
            if (!string.IsNullOrEmpty(urlImage))
            {
                postData += "&image_url=" + Uri.EscapeDataString(urlImage);
            }

            var data = Encoding.ASCII.GetBytes(postData);

            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = data.Length;

            using (var stream = myReq.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)myReq.GetResponse();

            var responseServer = new StreamReader(response.GetResponseStream()).ReadToEnd();

            if (!string.IsNullOrEmpty(responseServer))
            {
                //var resServer = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer);
                var resultAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(ResultAddAttribute)) as ResultAddAttribute;

                if (resultAPI != null)
                {
                    if (resultAPI.error == "none" && resultAPI.results == "success")
                    {
                        if (resultAPI.data != null)
                        {
                            var idAttribute = resultAPI.data.id;
                            string Link_Error = "0;Buat Produk;;";
                            var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(brg_mp_split[0] + ";" + idAttribute) + "',LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '" + Link_Error + "' WHERE BRG = '" + Convert.ToString(kodeBarang) + "' AND IDMARKET = '" + Convert.ToString(iden.ID_MARKET) + "'");
                            //var e2CartController = new EightTwoCartControllerJob();
                            //E2Cart_AddImageProduct_varian(iden, brg_mp_split[0], Convert.ToString(idAttribute), urlImage);

#if (DEBUG || Debug_AWS)
                            Task.Run(() => E2Cart_UpdateStock_82Cart(iden.DatabasePathErasoft, kodeBarang, iden.no_cust, "Stock", "Update Stok", iden, Convert.ToString(brg_mp_split[0] + ";" + idAttribute), Convert.ToInt32(qty), iden.username)).Wait();
#else
                                var EDB2 = new DatabaseSQL(iden.DatabasePathErasoft);
                                string EDBConnID = EDB2.GetConnectionString("ConnId");
                                var sqlStorage = new SqlServerStorage(EDBConnID);
                                var client = new BackgroundJobClient(sqlStorage);
                                client.Enqueue<EightTwoCartControllerJob>(x => x.E2Cart_UpdateStock_82Cart(iden.DatabasePathErasoft, kodeBarang, iden.no_cust, "Stock", "Update Stok", iden, Convert.ToString(brg_mp_split[0] + ";" + idAttribute), Convert.ToInt32(qty), iden.username));
                                //end handle update stock for default product
#endif
                        }
                    }
                }
                //ViewBag.response = resServer;
            }

            return "";
        }

        //Add Product Image.
        public async Task<string> E2Cart_AddImageProduct(E2CartAPIData iden, string brg_mp, string image_url)
        {
            string[] brg_mp_split = brg_mp.Split(';');
            string urll = string.Format("{0}/api/v1/addProductImage", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            postData += "&id_product=" + Uri.EscapeDataString(brg_mp_split[0]);
            postData += "&image_url=" + Uri.EscapeDataString(image_url);

            var data = Encoding.ASCII.GetBytes(postData);

            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = data.Length;

            using (var stream = myReq.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)myReq.GetResponse();

            var responseServer = new StreamReader(response.GetResponseStream()).ReadToEnd();

            if (!string.IsNullOrEmpty(responseServer))
            {
                var resServer = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer);

                //ViewBag.response = resServer;
            }

            return "";
        }

        //Add Product Image.
        public async Task<string> E2Cart_AddImageProduct_varian(E2CartAPIData iden, string product_id, string brg_varian, string image_url)
        {
            string urll = string.Format("{0}/api/v1/addProductImage", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            postData += "&id_product=" + Uri.EscapeDataString(product_id);
            postData += "&id_product_attribute=" + Uri.EscapeDataString("[" + brg_varian + "]");
            postData += "&image_url=" + Uri.EscapeDataString(image_url);

            var data = Encoding.ASCII.GetBytes(postData);

            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = data.Length;

            using (var stream = myReq.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)myReq.GetResponse();

            var responseServer = new StreamReader(response.GetResponseStream()).ReadToEnd();

            if (!string.IsNullOrEmpty(responseServer))
            {
                var resServer = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer);

                //ViewBag.response = resServer;
            }

            return "";
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> E2Cart_GetOrderByStatus(E2CartAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar)
        {
            string ret = "";
            SetupContext(iden);

            var daysFrom = -1;
            var daysTo = 1;
            var daysNow = DateTime.UtcNow.AddHours(7);

            //add by nurul 20/1/2021, bundling 
            var AdaKomponen = false;
            var connIdProses = "";
            List<string> tempConnId = new List<string>() { };
            //end add by nurul 20/1/2021, bundling 

            while (daysFrom > -13)
            {
                //var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).AddHours(7).ToString("yyyy-MM-dd") + " 00:00:00";
                //var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).AddHours(7).ToString("yyyy-MM-dd") + " 23:59:59";
                var dateFrom = daysNow.AddDays(daysFrom).ToString("yyyy-MM-dd HH:mm:ss");
                var dateTo = daysNow.AddDays(daysTo > 0 ? 0 : daysTo).ToString("yyyy-MM-dd HH:mm:ss");


                //change by nurul 20/1/2021, bundling 
                //await E2Cart_GetOrderByStatusList3Days(iden, stat, CUST, NAMA_CUST, 1, 0, 0, dateFrom, dateTo);
                var returnGetOrder = await E2Cart_GetOrderByStatusList3Days(iden, stat, CUST, NAMA_CUST, 1, 0, 0, dateFrom, dateTo);
                //change by nurul 20/1/2021, bundling 
                //daysFrom -= 3;
                //daysTo -= 3;
                daysFrom -= 2;
                daysTo -= 2;

                //add by nurul 20/1/2021, bundling 
                //if (returnGetOrder != "")
                //{
                //    tempConnId.Add(returnGetOrder);
                //    connIdProses += "'" + returnGetOrder + "' , ";
                //}
                if(returnGetOrder == "1")
                {
                    AdaKomponen = true;
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
            if(AdaKomponen)
            {
                new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username);
            }
            //end add by nurul 20/1/2021, bundling 

            // tunning untuk tidak duplicate
            var queryStatus = "";
            if (stat == StatusOrder.UNPAID)
            {
                //queryStatus = "\"}\"" + "," + "\"23\"" + "," + "\"";
                queryStatus = "\\\"}\"" + "," + "\"23\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","23","\"000003\""
            }
            else if (stat == StatusOrder.PAID)
            {
                //queryStatus = "\"}\"" + "," + "\"2\"" + "," + "\"";
                queryStatus = "\\\"}\"" + "," + "\"2\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","2","\"000003\""
            }
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust + "%' and arguments like '%" + queryStatus + "%' and invocationdata like '%E2Cart_GetOrderByStatus%' and statename like '%Enque%' and invocationdata not like '%resi%' and invocationdata not like '%E2Cart_GetOrderByStatusCompleted%' and invocationdata not like '%E2Cart_GetOrderByStatusCancelled%' ");
            // end tunning untuk tidak duplicate

            return ret;
        }

        public async Task<string> E2Cart_GetOrderByStatusList3Days(E2CartAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar, string daysFrom, string daysTo)
        {
            string ret = "";
            string connID = Guid.NewGuid().ToString();

            //add by nurul 19/1/2021, bundling 
            //ret = connID;
            //end add by nurul 19/1/2021, bundling

            int jmlhPesananUbahDibayar = 0;
            SetupContext(iden);

            //var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            //var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            //var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).AddHours(7).ToString("yyyy-MM-dd") + " 00:00:00";
            //var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).AddHours(7).ToString("yyyy-MM-dd") + " 23:59:59";
            var dateFrom = daysFrom;
            var dateTo = daysTo;

            string urll = string.Format("{0}/api/v1/getOrder?apiKey={1}&apiCredential={2}&date_add_from={3}&date_add_to={4}", iden.API_url, iden.API_key, iden.API_credential, dateFrom, dateTo);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            //try
            //{
            using (WebResponse response = await myReq.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseServer = reader.ReadToEnd();
                }
            }
            //}
            //catch (Exception ex)
            //{

            //}

            if (responseServer != null)
            {
                //try
                //{
                var listOrder = JsonConvert.DeserializeObject(responseServer, typeof(E2CartOrderResult)) as E2CartOrderResult;
                if (listOrder != null)
                {
                    if (listOrder.data != null)
                    {
                        //string[] statusAwaiting = { "1", "3", "10", "11", "13", "14", "16", "17", "18", "19", "20", "21", "23", "25" };

                        //string[] ordersn_list = listOrder.data.Select(p => p.id_order).ToArray();
                        //var dariTgl = DateTimeOffset.UtcNow.AddDays(-10).DateTime;
                        //jmlhNewOrder = 0;
                        var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();

                        #region UNPAID
                        if (stat == StatusOrder.UNPAID)
                        {
                            //check state order
                            var resultStatusAwaiting = await E2Cart_GetOrdersState(iden);
                            //end check state order

                            if (resultStatusAwaiting.dataObject != null)
                                if (resultStatusAwaiting.dataObject.Count() > 0)
                                    foreach (var itemOrder in resultStatusAwaiting.dataObject)
                                    {
                                        if (itemOrder.name.ToString().ToLower().Contains("awaiting") && itemOrder.name.ToString().ToLower().Contains("payment") || itemOrder.name.ToString().ToLower().Contains("payment confirm"))
                                        {
                                            var orderFilter = listOrder.data.Where(p => p.current_state == itemOrder.id_order_state).ToList();
                                            if (orderFilter.Count() > 0)
                                            {
                                                foreach (var order in orderFilter)
                                                {
                                                    try
                                                    {
                                                        if (!OrderNoInDb.Contains(order.id_order + ";" + order.reference))
                                                        {
                                                            jmlhNewOrder++;
                                                            var connIdARF01C = Guid.NewGuid().ToString();
                                                            TEMP_82CART_ORDERS batchinsert = new TEMP_82CART_ORDERS();
                                                            List<TEMP_82CART_ORDERS_ITEM> batchinsertItem = new List<TEMP_82CART_ORDERS_ITEM>();
                                                            string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                                            insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                                            insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                                                            var kabKot = "3174";
                                                            var prov = "31";


                                                            string fullname = order.firstname.ToString() + " " + order.lastname.ToString();
                                                            string nama = fullname.Length > 30 ? fullname.Substring(0, 30) : order.firstname.ToString() + " " + order.lastname.ToString();
                                                            
                                                            string TLP = !string.IsNullOrEmpty(order.delivery_address[0].phone_mobile) ? order.delivery_address[0].phone_mobile.Replace('\'', '`') : "";
                                                            if (TLP.Length > 30)
                                                                TLP = TLP.Substring(0, 30);
                                                            if (NAMA_CUST.Length > 30)
                                                                NAMA_CUST = NAMA_CUST.Substring(0, 30);
                                                            string AL_KIRIM1 = !string.IsNullOrEmpty(order.delivery_address[0].address1) ? order.delivery_address[0].address1.Replace('\'', '`') : "";
                                                            if (AL_KIRIM1.Length > 30)
                                                            {
                                                                AL_KIRIM1 = AL_KIRIM1.Substring(0, 30);
                                                            }
                                                            string KODEPOS = !string.IsNullOrEmpty(order.delivery_address[0].postcode) ? order.delivery_address[0].postcode.Replace('\'', '`') : "";
                                                            if (KODEPOS.Length > 7)
                                                            {
                                                                KODEPOS = KODEPOS.Substring(0, 7);
                                                            }

                                                            insertPembeli += string.Format("('{0}','{1}','{2}','{3}',0,0,'0','01',1, 'IDR', '01', '{4}', 0, 0, 0, 0, '1', 0, 0,'FP', '{5}', '{6}', '{7}', '', '{8}', '{9}', '', '','{10}'),",
                                                                ((nama ?? "").Replace("'", "`")),
                                                                ((order.delivery_address[0].address1 ?? "" + " " + order.delivery_address[0].address2 ?? "").Replace("'", "`") + " " + order.delivery_address[0].state),
                                                                //((order.delivery_address[0].phone_mobile ?? "").Replace("'", "`")),
                                                                TLP,
                                                                (NAMA_CUST.Replace(',', '.')),
                                                                //((order.delivery_address[0].address1 ?? "" + " " + order.delivery_address[0].address2 ?? "").Replace("'", "`") + " " + order.delivery_address[0].state),
                                                                AL_KIRIM1,
                                                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                                                (username),
                                                                //((order.delivery_address[0].postcode ?? "").Replace("'", "`")),
                                                                KODEPOS,
                                                                kabKot,
                                                                prov,
                                                                connIdARF01C
                                                                );
                                                            insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
                                                            EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);


                                                            ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_82CART_ORDERS");
                                                            ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_82CART_ORDERS_ITEM");
                                                            batchinsertItem = new List<TEMP_82CART_ORDERS_ITEM>();

                                                            //2020-04-08T05:12:41
                                                            var dateOrder = Convert.ToDateTime(order.date_add).ToString("yyyy-MM-dd HH:mm:ss");
                                                            var datePay = dateOrder;
                                                            if (order.invoice_date == "0000-00-00 00:00:00")
                                                            {
                                                                datePay = dateOrder;
                                                            }
                                                            else
                                                            {
                                                                datePay = Convert.ToDateTime(order.invoice_date).ToString("yyyy-MM-dd HH:mm:ss");
                                                            }
                                                            #region cut char
                                                            string estimated_shipping_fee = !string.IsNullOrEmpty(order.total_shipping) ? order.total_shipping.Replace('\'', '`') : "";
                                                            if (estimated_shipping_fee.Length > 100)
                                                            {
                                                                estimated_shipping_fee = estimated_shipping_fee.Substring(0, 100);
                                                            }
                                                            string payment_method = !string.IsNullOrEmpty(order.payment) ? order.payment.Replace('\'', '`') : "";
                                                            if (payment_method.Length > 100)
                                                            {
                                                                payment_method = payment_method.Substring(0, 100);
                                                            }
                                                            string shipping_carrier = !string.IsNullOrEmpty(order.name_carrier) ? order.name_carrier.Replace('\'', '`') : "";
                                                            if (shipping_carrier.Length > 300)
                                                            {
                                                                shipping_carrier = shipping_carrier.Substring(0, 300);
                                                            }
                                                            string currency = !string.IsNullOrEmpty(order.currency) ? order.currency.Replace('\'', '`') : "";
                                                            if (currency.Length > 50)
                                                            {
                                                                currency = currency.Substring(0, 50);
                                                            }
                                                            string Recipient_Address_city = !string.IsNullOrEmpty(order.delivery_address[0].city) ? order.delivery_address[0].city.Replace('\'', '`') : "";
                                                            if (Recipient_Address_city.Length > 50)
                                                            {
                                                                Recipient_Address_city = Recipient_Address_city.Substring(0, 50);
                                                            }
                                                            string Recipient_Address_country = !string.IsNullOrEmpty(order.delivery_address[0].id_country) ? order.delivery_address[0].id_country.Replace('\'', '`') : "ID";
                                                            if (Recipient_Address_country.Length > 50)
                                                            {
                                                                Recipient_Address_country = Recipient_Address_country.Substring(0, 50);
                                                            }
                                                            string Recipient_Address_state = !string.IsNullOrEmpty(order.delivery_address[0].state) ? order.delivery_address[0].state.Replace('\'', '`') : "";
                                                            if (Recipient_Address_state.Length > 50)
                                                            {
                                                                Recipient_Address_state = Recipient_Address_state.Substring(0, 50);
                                                            }
                                                            string tracking_no = !string.IsNullOrEmpty(order.shipping_number) ? order.shipping_number.Replace('\'', '`') : "";
                                                            if (tracking_no.Length > 50)
                                                            {
                                                                tracking_no = tracking_no.Substring(0, 50);
                                                            }
                                                            string total_amount = !string.IsNullOrEmpty(order.total_paid) ? order.total_paid.Replace('\'', '`') : "";
                                                            if (total_amount.Length > 100)
                                                            {
                                                                total_amount = total_amount.Substring(0, 100);
                                                            }
                                                            string service_code = !string.IsNullOrEmpty(order.id_carrier) ? order.id_carrier.Replace('\'', '`') : "";
                                                            if (service_code.Length > 100)
                                                            {
                                                                service_code = service_code.Substring(0, 100);
                                                            }
                                                            string actual_shipping_cost = !string.IsNullOrEmpty(order.total_shipping) ? order.total_shipping.Replace('\'', '`') : "";
                                                            if (actual_shipping_cost.Length > 100)
                                                            {
                                                                actual_shipping_cost = actual_shipping_cost.Substring(0, 100);
                                                            }
                                                            string u_ordersn = !string.IsNullOrEmpty(order.id_order + ";" + order.reference) ? (order.id_order + ";" + (order.reference ?? "")).Replace('\'', '`') : "";
                                                            if (u_ordersn.Length > 70)
                                                            {
                                                                u_ordersn = u_ordersn.Substring(0, 70);
                                                            }

                                                            #endregion

                                                            TEMP_82CART_ORDERS newOrder = new TEMP_82CART_ORDERS()
                                                            {
                                                                actual_shipping_cost = actual_shipping_cost,
                                                                buyer_username = nama,
                                                                cod = false,
                                                                country = "",
                                                                create_time = Convert.ToDateTime(dateOrder),
                                                                currency = currency,
                                                                days_to_ship = 0,
                                                                dropshipper = "",
                                                                escrow_amount = "",
                                                                estimated_shipping_fee = estimated_shipping_fee,
                                                                goods_to_declare = false,
                                                                message_to_seller = "",
                                                                note = "",
                                                                note_update_time = Convert.ToDateTime(dateOrder),
                                                                ordersn = u_ordersn,
                                                                //order_status = order.current_state_name,
                                                                order_status = "UNPAID",
                                                                payment_method = order.payment,
                                                                //change by nurul 5/12/2019, local time 
                                                                //pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime,
                                                                pay_time = Convert.ToDateTime(datePay),
                                                                //end change by nurul 5/12/2019, local time 
                                                                Recipient_Address_country = Recipient_Address_country,
                                                                Recipient_Address_state = Recipient_Address_state,
                                                                Recipient_Address_city = Recipient_Address_city,
                                                                Recipient_Address_town = "",
                                                                Recipient_Address_district = "",
                                                                Recipient_Address_full_address = (order.delivery_address[0].address1 ?? "" + " " + order.delivery_address[0].address2 ?? "").Replace("'", "`") + " " + Recipient_Address_state,
                                                                Recipient_Address_name = nama,
                                                                Recipient_Address_phone = TLP,
                                                                Recipient_Address_zipcode = KODEPOS,
                                                                service_code = service_code,
                                                                shipping_carrier = shipping_carrier,
                                                                total_amount = total_amount,
                                                                tracking_no = tracking_no,
                                                                update_time = Convert.ToDateTime(dateOrder),
                                                                CONN_ID = connID,
                                                                CUST = CUST,
                                                                NAMA_CUST = NAMA_CUST
                                                            };
                                                            foreach (var item in order.order_detail)
                                                            {
                                                                //var product_id = "";
                                                                //var name_brg = "";

                                                                #region cut char
                                                                string item_name = !string.IsNullOrEmpty(item.product_name) ? item.product_name.Replace('\'', '`') : "";
                                                                if (item_name.Length > 150)
                                                                {
                                                                    item_name = item_name.Substring(0, 150);
                                                                }
                                                                string item_sku = !string.IsNullOrEmpty(item.reference) ? item.reference.Replace('\'', '`') : "";
                                                                if (item_sku.Length > 400)
                                                                {
                                                                    item_sku = item_sku.Substring(0, 400);
                                                                }
                                                                string item_id = !string.IsNullOrEmpty(item.product_id) ? item.product_id.Replace('\'', '`') : "";
                                                                if (item_id.Length > 20)
                                                                {
                                                                    item_id = item_id.Substring(0, 20);
                                                                }
                                                                string variation_id = !string.IsNullOrEmpty(item.product_attribute_id) ? item.product_attribute_id.Replace('\'', '`') : "";
                                                                if (variation_id.Length > 20)
                                                                {
                                                                    variation_id = variation_id.Substring(0, 20);
                                                                }
                                                                string variation_discounted_price = !string.IsNullOrEmpty(item.original_product_price) ? item.original_product_price.Replace('\'', '`') : "";
                                                                if (variation_discounted_price.Length > 50)
                                                                {
                                                                    variation_discounted_price = variation_discounted_price.Substring(0, 50);
                                                                }
                                                                #endregion

                                                                var name_brg_variasi = "";
                                                                if (item.product_attribute_id == "0")
                                                                {
                                                                    //var kodeBrg = ErasoftDbContext.STF02.SingleOrDefault(p => p.NAMA.Contains(item.product_name) && p.PART == "");
                                                                    //product_id = kodeBrg.BRG;
                                                                    //product_id = item.product_id;
                                                                    //name_brg = item.product_name;
                                                                }
                                                                else
                                                                {
                                                                    //product_id = item.product_attribute_id;
                                                                    //name_brg_variasi = item.product_name;
                                                                    name_brg_variasi = item_name;
                                                                }
                                                                TEMP_82CART_ORDERS_ITEM newOrderItem = new TEMP_82CART_ORDERS_ITEM()
                                                                {
                                                                    ordersn = u_ordersn,
                                                                    is_wholesale = false,
                                                                    item_id = item_id,
                                                                    item_name = item_name,
                                                                    item_sku = item_sku,
                                                                    variation_discounted_price = variation_discounted_price,
                                                                    variation_id = variation_id,
                                                                    variation_name = name_brg_variasi,
                                                                    variation_original_price = variation_discounted_price,
                                                                    variation_quantity_purchased = Convert.ToInt32(item.product_quantity),
                                                                    variation_sku = item_sku,
                                                                    weight = 0,
                                                                    pay_time = Convert.ToDateTime(datePay),
                                                                    CONN_ID = connID,
                                                                    CUST = CUST,
                                                                    NAMA_CUST = NAMA_CUST
                                                                };
                                                                //if (!string.IsNullOrEmpty(item.promotion_type))
                                                                //{
                                                                //    if (item.promotion_type == "bundle_deal")
                                                                //    {
                                                                //        var promoPrice = await GetEscrowDetail(iden, order.ordersn, item.item_id, item.variation_id);
                                                                //        newOrderItem.variation_discounted_price = promoPrice;
                                                                //    }
                                                                //}
                                                                batchinsertItem.Add(newOrderItem);
                                                            }
                                                            batchinsert = (newOrder);

                                                            ErasoftDbContext.TEMP_82CART_ORDERS.Add(batchinsert);
                                                            ErasoftDbContext.TEMP_82CART_ORDERS_ITEM.AddRange(batchinsertItem);
                                                            ErasoftDbContext.SaveChanges();
                                                            using (SqlCommand CommandSQL = new SqlCommand())
                                                            {
                                                                //call sp to insert buyer data
                                                                CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                                                CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIdARF01C;

                                                                EDB.ExecuteSQL("Con", "MoveARF01CFromTempTable", CommandSQL);
                                                            };
                                                            using (SqlCommand CommandSQL = new SqlCommand())
                                                            {
                                                                CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                                                CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connID;
                                                                CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                                                                CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                                CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                                                                CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                                                                CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                                                                CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                                                                CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                                                                CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                                                                CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                                                                CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 1;
                                                                CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                                                                CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                                                                EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
                                                            }
                                                        }
                                                    }
                                                    catch (Exception ex3)
                                                    {

                                                    }
                                                }
                                            }
                                        }

                                    }

                            if (jmlhNewOrder > 0)
                            {
                                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari 82Cart.");

                                //add by nurul 25/1/2021, bundling
                                var listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in ('" + connID + "')").ToList();
                                if (listBrgKomponen.Count() > 0)
                                {
                                    ret = "1";
                                }
                                //end add by nurul 25/1/2021, bundling
                                new StokControllerJob().updateStockMarketPlace(connID, iden.DatabasePathErasoft, iden.username);
                            }
                        }
                        #endregion

                        #region PAID
                        if (stat == StatusOrder.PAID)
                        {
                            string[] statusCAP = { "2", "3" }; // CODE STATUS PESANAN PAYMENT ACCEPTED, DAN PREPARATION IN PROGRESS INSERT KE DB
                            string ordersn = "";
                            jmlhPesananDibayar = 0;

                            string ordersn2 = "";

                            for (int itemOrderExisting = 0; itemOrderExisting < statusCAP.Length; itemOrderExisting++)
                            {
                                var orderFilterExisting = listOrder.data.Where(p => p.current_state == statusCAP[itemOrderExisting]).ToList();
                                if (orderFilterExisting != null)
                                {
                                    foreach (var order in orderFilterExisting)
                                    {
                                        if (!OrderNoInDb.Contains(order.id_order + ";" + order.reference))
                                        {
                                            jmlhPesananDibayar++;
                                            ordersn = ordersn + "'" + order.id_order + ";" + order.reference + "',";
                                            var statusOrderSP = "";

                                            if (statusCAP[itemOrderExisting].ToString() == "2")
                                            {
                                                statusOrderSP = "PAID";
                                            }
                                            else if (statusCAP[itemOrderExisting].ToString() == "3")
                                            {
                                                //statusOrderSP = "PREPARATION IN PROGRESS";
                                                statusOrderSP = "PAID";
                                            }

                                            var connIdARF01C = Guid.NewGuid().ToString();
                                            TEMP_82CART_ORDERS batchinsert = new TEMP_82CART_ORDERS();
                                            List<TEMP_82CART_ORDERS_ITEM> batchinsertItem = new List<TEMP_82CART_ORDERS_ITEM>();
                                            string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                            insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                            insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                                            var kabKot = "3174";
                                            var prov = "31";


                                            string fullname = order.firstname.ToString() + " " + order.lastname.ToString();
                                            string nama = fullname.Length > 30 ? fullname.Substring(0, 30) : order.firstname.ToString() + " " + order.lastname.ToString();

                                            string TLP = !string.IsNullOrEmpty(order.delivery_address[0].phone_mobile) ? order.delivery_address[0].phone_mobile.Replace('\'', '`') : "";
                                            if (TLP.Length > 30)
                                                TLP = TLP.Substring(0, 30);
                                            if (NAMA_CUST.Length > 30)
                                                NAMA_CUST = NAMA_CUST.Substring(0, 30);
                                            string AL_KIRIM1 = !string.IsNullOrEmpty(order.delivery_address[0].address1) ? order.delivery_address[0].address1.Replace('\'', '`') : "";
                                            if (AL_KIRIM1.Length > 30)
                                            {
                                                AL_KIRIM1 = AL_KIRIM1.Substring(0, 30);
                                            }
                                            string KODEPOS = !string.IsNullOrEmpty(order.delivery_address[0].postcode) ? order.delivery_address[0].postcode.Replace('\'', '`') : "";
                                            if (KODEPOS.Length > 7)
                                            {
                                                KODEPOS = KODEPOS.Substring(0, 7);
                                            }

                                            insertPembeli += string.Format("('{0}','{1}','{2}','{3}',0,0,'0','01',1, 'IDR', '01', '{4}', 0, 0, 0, 0, '1', 0, 0,'FP', '{5}', '{6}', '{7}', '', '{8}', '{9}', '', '','{10}'),",
                                                ((nama ?? "").Replace("'", "`")),
                                                ((order.delivery_address[0].address1 ?? "" + " " + order.delivery_address[0].address2 ?? "").Replace("'", "`") + " " + order.delivery_address[0].state),
                                                TLP,
                                                (NAMA_CUST.Replace(',', '.')),
                                                AL_KIRIM1,
                                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                                (username),
                                                KODEPOS,
                                                kabKot,
                                                prov,
                                                connIdARF01C
                                                );
                                            insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
                                            EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);


                                            ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_82CART_ORDERS");
                                            ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_82CART_ORDERS_ITEM");
                                            batchinsertItem = new List<TEMP_82CART_ORDERS_ITEM>();

                                            //2020-04-08T05:12:41
                                            var dateOrder = Convert.ToDateTime(order.date_add).ToString("yyyy-MM-dd HH:mm:ss");
                                            var datePay = dateOrder;
                                            if (order.invoice_date == "0000-00-00 00:00:00")
                                            {
                                                datePay = dateOrder;
                                            }
                                            else
                                            {
                                                datePay = Convert.ToDateTime(order.invoice_date).ToString("yyyy-MM-dd HH:mm:ss");
                                            }

                                            #region cut char
                                            string p_estimated_shipping_fee = !string.IsNullOrEmpty(order.total_shipping) ? order.total_shipping.Replace('\'', '`') : "";
                                            if (p_estimated_shipping_fee.Length > 100)
                                            {
                                                p_estimated_shipping_fee = p_estimated_shipping_fee.Substring(0, 100);
                                            }
                                            string p_payment_method = !string.IsNullOrEmpty(order.payment) ? order.payment.Replace('\'', '`') : "";
                                            if (p_payment_method.Length > 100)
                                            {
                                                p_payment_method = p_payment_method.Substring(0, 100);
                                            }
                                            string p_shipping_carrier = !string.IsNullOrEmpty(order.name_carrier) ? order.name_carrier.Replace('\'', '`') : "";
                                            if (p_shipping_carrier.Length > 300)
                                            {
                                                p_shipping_carrier = p_shipping_carrier.Substring(0, 300);
                                            }
                                            string p_currency = !string.IsNullOrEmpty(order.currency) ? order.currency.Replace('\'', '`') : "";
                                            if (p_currency.Length > 50)
                                            {
                                                p_currency = p_currency.Substring(0, 50);
                                            }
                                            string p_Recipient_Address_city = !string.IsNullOrEmpty(order.delivery_address[0].city) ? order.delivery_address[0].city.Replace('\'', '`') : "";
                                            if (p_Recipient_Address_city.Length > 50)
                                            {
                                                p_Recipient_Address_city = p_Recipient_Address_city.Substring(0, 50);
                                            }
                                            string p_Recipient_Address_country = !string.IsNullOrEmpty(order.delivery_address[0].id_country) ? order.delivery_address[0].id_country.Replace('\'', '`') : "ID";
                                            if (p_Recipient_Address_country.Length > 50)
                                            {
                                                p_Recipient_Address_country = p_Recipient_Address_country.Substring(0, 50);
                                            }
                                            string p_Recipient_Address_state = !string.IsNullOrEmpty(order.delivery_address[0].state) ? order.delivery_address[0].state.Replace('\'', '`') : "";
                                            if (p_Recipient_Address_state.Length > 50)
                                            {
                                                p_Recipient_Address_state = p_Recipient_Address_state.Substring(0, 50);
                                            }
                                            string p_tracking_no = !string.IsNullOrEmpty(order.shipping_number) ? order.shipping_number.Replace('\'', '`') : "";
                                            if (p_tracking_no.Length > 50)
                                            {
                                                p_tracking_no = p_tracking_no.Substring(0, 50);
                                            }
                                            string p_total_amount = !string.IsNullOrEmpty(order.total_paid) ? order.total_paid.Replace('\'', '`') : "";
                                            if (p_total_amount.Length > 100)
                                            {
                                                p_total_amount = p_total_amount.Substring(0, 100);
                                            }
                                            string p_service_code = !string.IsNullOrEmpty(order.id_carrier) ? order.id_carrier.Replace('\'', '`') : "";
                                            if (p_service_code.Length > 100)
                                            {
                                                p_service_code = p_service_code.Substring(0, 100);
                                            }
                                            string p_actual_shipping_cost = !string.IsNullOrEmpty(order.total_shipping) ? order.total_shipping.Replace('\'', '`') : "";
                                            if (p_actual_shipping_cost.Length > 100)
                                            {
                                                p_actual_shipping_cost = p_actual_shipping_cost.Substring(0, 100);
                                            }
                                            string p_ordersn = !string.IsNullOrEmpty(order.id_order + ";" + order.reference) ? (order.id_order + ";" + (order.reference ?? "")).Replace('\'', '`') : "";
                                            if (p_ordersn.Length > 70)
                                            {
                                                p_ordersn = p_ordersn.Substring(0, 70);
                                            }

                                            #endregion

                                            TEMP_82CART_ORDERS newOrder = new TEMP_82CART_ORDERS()
                                            {
                                                actual_shipping_cost = p_actual_shipping_cost,
                                                buyer_username = nama,
                                                cod = false,
                                                country = "",
                                                create_time = Convert.ToDateTime(dateOrder),
                                                currency = p_currency,
                                                days_to_ship = 0,
                                                dropshipper = "",
                                                escrow_amount = "",
                                                estimated_shipping_fee = p_actual_shipping_cost,
                                                goods_to_declare = false,
                                                message_to_seller = "",
                                                note = "",
                                                note_update_time = Convert.ToDateTime(dateOrder),
                                                ordersn = order.id_order + ";" + order.reference ?? "",
                                                //order_status = order.current_state_name,
                                                order_status = statusOrderSP,
                                                payment_method = p_payment_method,
                                                //change by nurul 5/12/2019, local time 
                                                //pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime,
                                                pay_time = Convert.ToDateTime(datePay),
                                                //end change by nurul 5/12/2019, local time 
                                                Recipient_Address_country = p_Recipient_Address_country,
                                                Recipient_Address_state = p_Recipient_Address_state,
                                                Recipient_Address_city = p_Recipient_Address_city,
                                                Recipient_Address_town = "",
                                                Recipient_Address_district = "",
                                                Recipient_Address_full_address = (order.delivery_address[0].address1 ?? "" + " " + order.delivery_address[0].address2 ?? "").Replace("'", "`") + " " + p_Recipient_Address_state,
                                                Recipient_Address_name = nama,
                                                Recipient_Address_phone = TLP,
                                                Recipient_Address_zipcode = KODEPOS,
                                                service_code = p_service_code,
                                                shipping_carrier = p_shipping_carrier,
                                                total_amount = p_total_amount,
                                                tracking_no = p_tracking_no,
                                                update_time = Convert.ToDateTime(dateOrder),
                                                CONN_ID = connID,
                                                CUST = CUST,
                                                NAMA_CUST = NAMA_CUST
                                            };
                                            foreach (var item in order.order_detail)
                                            {
                                                //var product_id = "";
                                                //var name_brg = "";

                                                #region cut char
                                                string p_item_name = !string.IsNullOrEmpty(item.product_name) ? item.product_name.Replace('\'', '`') : "";
                                                if (p_item_name.Length > 150)
                                                {
                                                    p_item_name = p_item_name.Substring(0, 150);
                                                }
                                                string p_item_sku = !string.IsNullOrEmpty(item.reference) ? item.reference.Replace('\'', '`') : "";
                                                if (p_item_sku.Length > 400)
                                                {
                                                    p_item_sku = p_item_sku.Substring(0, 400);
                                                }
                                                string p_item_id = !string.IsNullOrEmpty(item.product_id) ? item.product_id.Replace('\'', '`') : "";
                                                if (p_item_id.Length > 20)
                                                {
                                                    p_item_id = p_item_id.Substring(0, 20);
                                                }
                                                string p_variation_id = !string.IsNullOrEmpty(item.product_attribute_id) ? item.product_attribute_id.Replace('\'', '`') : "";
                                                if (p_variation_id.Length > 20)
                                                {
                                                    p_variation_id = p_variation_id.Substring(0, 20);
                                                }
                                                string p_variation_discounted_price = !string.IsNullOrEmpty(item.original_product_price) ? item.original_product_price.Replace('\'', '`') : "";
                                                if (p_variation_discounted_price.Length > 50)
                                                {
                                                    p_variation_discounted_price = p_variation_discounted_price.Substring(0, 50);
                                                }
                                                #endregion

                                                var name_brg_variasi = "";
                                                if (item.product_attribute_id == "0")
                                                {
                                                    //var kodeBrg = ErasoftDbContext.STF02.SingleOrDefault(p => p.NAMA.Contains(item.product_name) && p.PART == "");
                                                    //product_id = kodeBrg.BRG;
                                                    //product_id = item.product_id;
                                                    //name_brg = item.product_name;
                                                }
                                                else
                                                {
                                                    //product_id = item.product_attribute_id;
                                                    //name_brg_variasi = item.product_name;
                                                    name_brg_variasi = p_item_name;
                                                }

                                                TEMP_82CART_ORDERS_ITEM newOrderItem = new TEMP_82CART_ORDERS_ITEM()
                                                {
                                                    ordersn = p_ordersn,
                                                    is_wholesale = false,
                                                    item_id = p_item_id,
                                                    item_name = p_item_name,
                                                    item_sku = p_item_sku,
                                                    variation_discounted_price = p_variation_discounted_price,
                                                    variation_id = p_variation_id,
                                                    variation_name = name_brg_variasi,
                                                    variation_original_price = p_variation_discounted_price,
                                                    variation_quantity_purchased = Convert.ToInt32(item.product_quantity),
                                                    variation_sku = p_item_sku,
                                                    weight = 0,
                                                    pay_time = Convert.ToDateTime(datePay),
                                                    CONN_ID = connID,
                                                    CUST = CUST,
                                                    NAMA_CUST = NAMA_CUST
                                                };

                                                batchinsertItem.Add(newOrderItem);
                                            }
                                            batchinsert = (newOrder);

                                            ErasoftDbContext.TEMP_82CART_ORDERS.Add(batchinsert);
                                            ErasoftDbContext.TEMP_82CART_ORDERS_ITEM.AddRange(batchinsertItem);
                                            ErasoftDbContext.SaveChanges();
                                            using (SqlCommand CommandSQL = new SqlCommand())
                                            {
                                                //call sp to insert buyer data
                                                CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                                CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIdARF01C;

                                                EDB.ExecuteSQL("Con", "MoveARF01CFromTempTable", CommandSQL);
                                            };
                                            using (SqlCommand CommandSQL = new SqlCommand())
                                            {
                                                CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                                CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connID;
                                                CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                                                CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                                                CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                                                CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                                                CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                                                CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                                                CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                                                CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                                                CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 1;
                                                CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                                                CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                                                EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
                                            }
                                        }
                                        else
                                        {
                                            ordersn2 = ordersn2 + "'" + order.id_order + ";" + order.reference + "',";
                                        }

                                    }
                                    if (!string.IsNullOrEmpty(ordersn2))
                                    {
                                        ordersn2 = ordersn2.Substring(0, ordersn2.Length - 1);
                                        var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI IN (" + ordersn2 + ") AND STATUS_TRANSAKSI = '0'");
                                        if (rowAffected > 0)
                                        {
                                            jmlhPesananUbahDibayar += rowAffected;
                                        }
                                    }
                                }
                            }

                            if (jmlhPesananUbahDibayar > 0)
                            {
                                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhPesananUbahDibayar) + " Pesanan terbayar dari 82Cart.");
                            }

                            if (jmlhPesananDibayar > 0)
                            {
                                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhPesananDibayar) + " Pesanan terbayar dari 82Cart.");

                                //add by nurul 25/1/2021, bundling
                                var listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in ('" + connID + "')").ToList();
                                if (listBrgKomponen.Count() > 0)
                                {
                                    ret = "1";
                                }
                                //end add by nurul 25/1/2021, bundling
                                new StokControllerJob().updateStockMarketPlace(connID, iden.DatabasePathErasoft, iden.username);
                            }
                        }
                        #endregion

                    }
                }
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> E2Cart_GetOrderByStatusCompleted(E2CartAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhOrderCompeleted)
        {
            string ret = "";
            SetupContext(iden);

            var daysFrom = -1;
            var daysTo = 1;
            var daysNow = DateTime.UtcNow.AddHours(7);
            while (daysFrom > -13)
            {
                //var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).AddHours(7).ToString("yyyy-MM-dd") + " 00:00:00";
                //var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).AddHours(7).ToString("yyyy-MM-dd") + " 23:59:59";
                var dateFrom = daysNow.AddDays(daysFrom).ToString("yyyy-MM-dd HH:mm:ss");
                var dateTo = daysNow.AddDays(daysTo > 0 ? 0 : daysTo).ToString("yyyy-MM-dd HH:mm:ss");

                await E2Cart_GetOrderByStatusCompletedList3Days(iden, stat, CUST, NAMA_CUST, 1, 0, dateFrom, dateTo);
                //daysFrom -= 3;
                //daysTo -= 3;
                daysFrom -= 2;
                daysTo -= 2;
            }


            // tunning untuk tidak duplicate
            var queryStatus = "\\\"}\"" + "," + "\"5\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","5","\"000003\""
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + queryStatus + "%' and arguments like '%" + iden.no_cust + "%' and invocationdata like '%E2Cart_GetOrderByStatusCompleted%' and statename like '%Enque%' and invocationdata not like '%resi%'");
            // end tunning untuk tidak duplicate

            return ret;
        }

        public async Task<string> E2Cart_GetOrderByStatusCompletedList3Days(E2CartAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhOrderCompeleted, string daysFrom, string daysTo)
        {
            string ret = "";

            SetupContext(iden);

            //var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            //var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            //var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).AddHours(7).ToString("yyyy-MM-dd") + " 00:00:00";
            //var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).AddHours(7).ToString("yyyy-MM-dd") + " 23:59:59";
            var dateFrom = daysFrom;
            var dateTo = daysTo;

            string urll = string.Format("{0}/api/v1/getOrder?apiKey={1}&apiCredential={2}&date_add_from={3}&date_add_to={4}", iden.API_url, iden.API_key, iden.API_credential, dateFrom, dateTo);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            //try
            //{
            using (WebResponse response = await myReq.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseServer = reader.ReadToEnd();
                }
            }
            //}
            //catch (Exception ex)
            //{

            //}


            if (responseServer != null)
            {
                //try
                //{
                var listOrder = JsonConvert.DeserializeObject(responseServer, typeof(E2CartOrderResult)) as E2CartOrderResult;
                if (listOrder != null)
                {
                    if (listOrder.data != null)
                    {
                        var statusCompleted = "5";
                        var orderFilterCompleted = listOrder.data.Where(p => p.current_state == statusCompleted).ToList();
                        var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();
                        string ordersn = "";
                        jmlhOrderCompeleted = 0;
                        if (orderFilterCompleted != null)
                        {
                            foreach (var item in orderFilterCompleted)
                            {
                                if (OrderNoInDb.Contains(item.id_order + ";" + item.reference))
                                {
                                    ordersn = ordersn + "'" + item.id_order + ";" + item.reference + "',";
                                }
                            }
                        }
                        if (orderFilterCompleted.Count() > 0 && !string.IsNullOrEmpty(ordersn))
                        {
                            ordersn = ordersn.Substring(0, ordersn.Length - 1);
                            var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '03'");
                            jmlhOrderCompeleted = jmlhOrderCompeleted + rowAffected;
                            if (jmlhOrderCompeleted > 0)
                            {
                                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderCompeleted) + " Pesanan dari 82Cart sudah selesai.");

                                //add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                if (!string.IsNullOrEmpty(ordersn))
                                {
                                    var dateTimeNow = Convert.ToDateTime(DateTime.Now.AddHours(7).ToString("yyyy-MM-dd"));
                                    string sSQLUpdateDatePesananSelesai = "UPDATE SIT01A SET TGL_KIRIM = '" + dateTimeNow + "' WHERE NO_REF IN (" + ordersn + ")";
                                    var resultUpdateDatePesanan = EDB.ExecuteSQL("CString", CommandType.Text, sSQLUpdateDatePesananSelesai);
                                }
                                //end add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                            }
                        }
                    }
                }
            }

            return ret;
        }


        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> E2Cart_GetOrderByStatusCancelled(E2CartAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhOrderCancel)
        {
            string ret = "";
            SetupContext(iden);

            var daysFrom = -1;
            var daysTo = 1;
            var daysNow = DateTime.UtcNow.AddHours(7);
            //add by nurul 20/1/2021, bundling 
            var AdaKomponen = false;
            var connIdProses = "";
            List<string> tempConnId = new List<string>() { };
            //end add by nurul 20/1/2021, bundling 

            while (daysFrom > -13)
            {
                //var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).AddHours(7).ToString("yyyy-MM-dd") + " 00:00:00";
                //var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).AddHours(7).ToString("yyyy-MM-dd") + " 23:59:59";
                var dateFrom = daysNow.AddDays(daysFrom).ToString("yyyy-MM-dd HH:mm:ss");
                var dateTo = daysNow.AddDays(daysTo > 0 ? 0 : daysTo).ToString("yyyy-MM-dd HH:mm:ss");
                
                //change by nurul 20/1/2021, bundling 
                //await E2Cart_GetOrderByStatusCancelledList3Days(iden, stat, CUST, NAMA_CUST, 1, 0, dateFrom, dateTo);
                var returnGetOrder = await E2Cart_GetOrderByStatusCancelledList3Days(iden, stat, CUST, NAMA_CUST, 1, 0, dateFrom, dateTo);
                //change by nurul 20/1/2021, bundling 
                //daysFrom -= 3;
                //daysTo -= 3;
                daysFrom -= 2;
                daysTo -= 2;

                //add by nurul 20/1/2021, bundling 
                //if (returnGetOrder != "")
                //{
                //    tempConnId.Add(returnGetOrder);
                //    connIdProses += "'" + returnGetOrder + "' , ";
                //}
                if(returnGetOrder == "1")
                {
                    AdaKomponen = true;
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
            if(AdaKomponen)
            {
                new StokControllerJob().getQtyBundling(iden.DatabasePathErasoft, iden.username);
            }
            //end add by nurul 20/1/2021, bundling 

            // tunning untuk tidak duplicate
            var queryStatus = "\\\"}\"" + "," + "\"6\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","6","\"000003\""
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + queryStatus + "%' and arguments like '%" + iden.no_cust + "%' and invocationdata like '%E2Cart_GetOrderByStatusCancelled%' and statename like '%Enque%' and invocationdata not like '%resi%'");
            // end tunning untuk tidak duplicate
            
            return ret;
        }

        public async Task<string> E2Cart_GetOrderByStatusCancelledList3Days(E2CartAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhOrderCancel, string daysFrom, string daysTo)
        {
            string ret = "";

            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);
            //add by nurul 19/1/2021, bundling 
            //ret = connID;
            //end add by nurul 19/1/2021, bundling

            //var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            //var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            //var dateFrom = DateTimeOffset.UtcNow.AddDays(daysFrom).AddHours(7).ToString("yyyy-MM-dd") + " 00:00:00";
            //var dateTo = DateTimeOffset.UtcNow.AddDays(daysTo).AddHours(7).ToString("yyyy-MM-dd") + " 23:59:59";
            var dateFrom = daysFrom;
            var dateTo = daysTo;

            string urll = string.Format("{0}/api/v1/getOrder?apiKey={1}&apiCredential={2}&date_add_from={3}&date_add_to={4}", iden.API_url, iden.API_key, iden.API_credential, dateFrom, dateTo);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            using (WebResponse response = await myReq.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseServer = reader.ReadToEnd();
                }
            }

            if (responseServer != null)
            {
                //try
                //{
                var listOrder = JsonConvert.DeserializeObject(responseServer, typeof(E2CartOrderResult)) as E2CartOrderResult;
                if (listOrder != null)
                {
                    if (listOrder.data != null)
                    {
                        var statusCancel = "6";
                        var orderFilterCancel = listOrder.data.Where(p => p.current_state == statusCancel).ToList();
                        var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();
                        string ordersn = "";
                        jmlhOrderCancel = 0;
                        if (orderFilterCancel != null)
                        {
                            foreach (var item in orderFilterCancel)
                            {
                                if (OrderNoInDb.Contains(item.id_order + ";" + item.reference))
                                {
                                    ordersn = ordersn + "'" + item.id_order + ";" + item.reference + "',";
                                }
                            }
                        }
                        if (orderFilterCancel.Count() > 0 && !string.IsNullOrEmpty(ordersn))
                        {
                            ordersn = ordersn.Substring(0, ordersn.Length - 1);
                            var brgAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connID + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI <> '11' AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "'");
                            //change by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                            //var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2', STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI <> '11' AND CUST = '" + CUST + "'");
                            var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2', STATUS_TRANSAKSI = '11', STATUS_KIRIM='5' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI <> '11' AND CUST = '" + CUST + "'");
                            //change by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                            if (rowAffected > 0)
                            {
                                //add by Tri 1 sep 2020, hapus packing list
                                //remark by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                                //var delPL = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03B WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN (" + ordersn + ")  AND STATUS_TRANSAKSI = '11' AND CUST = '" + CUST + "')");
                                //var delPLDetail = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03C WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN (" + ordersn + ")  AND STATUS_TRANSAKSI = '11' AND CUST = '" + CUST + "')");
                                //END remark by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                                //end add by Tri 1 sep 2020, hapus packing list
                                //add by Tri 4 Des 2019, isi cancel reason
                                var sSQL1 = "";
                                var sSQL2 = "SELECT * INTO #TEMP FROM (";
                                var listReason = new Dictionary<string, string>();

                                foreach (var order in listOrder.data)
                                {
                                    string reasonValue;
                                    if (listReason.TryGetValue(order.id_order + ";" + order.reference, out reasonValue))
                                    {
                                        if (!string.IsNullOrEmpty(sSQL1))
                                        {
                                            sSQL1 += " UNION ALL ";
                                        }
                                        sSQL1 += " SELECT '" + order.id_order + ";" + order.reference + "' NO_REFERENSI, '" + listReason[order.id_order + ";" + order.reference] + "' ALASAN ";
                                    }
                                }
                                sSQL2 += sSQL1 + ") as qry; INSERT INTO SOT01D (NO_BUKTI, CATATAN_1, USERNAME) ";
                                sSQL2 += " SELECT A.NO_BUKTI, ALASAN, 'AUTO_82CART' FROM SOT01A A INNER JOIN #TEMP T ON A.NO_REFERENSI = T.NO_REFERENSI ";
                                sSQL2 += " LEFT JOIN SOT01D D ON A.NO_BUKTI = D.NO_BUKTI WHERE ISNULL(D.NO_BUKTI, '') = ''";
                                EDB.ExecuteSQL("MOConnectionString", CommandType.Text, sSQL2);
                                //var nobuk = ErasoftDbContext.SOT01A.Where(m => m.NO_REFERENSI == ordersn && m.CUST == CUST).Select(m => m.NO_BUKTI).FirstOrDefault();
                                //if (!string.IsNullOrEmpty(nobuk))
                                //{
                                //    var sot01d = ErasoftDbContext.SOT01D.Where(m => m.NO_BUKTI == nobuk).FirstOrDefault();
                                //    if (sot01d == null)
                                //    {
                                //        EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "INSERT INTO SOT01D(NO_BUKTI, CATATAN_1, USERNAME) VALUES ('" + nobuk + "','" + order.reason + "','AUTO LAZADA')");
                                //    }
                                //}
                                //end add by Tri 4 Des 2019, isi cancel reason

                                var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN (" + ordersn + ") AND STATUS <> '2' AND ST_POSTING = 'T' AND CUST = '" + CUST + "'");

                                //add by nurul 25/1/2021, bundling
                                var listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in ('" + connID + "')").ToList();
                                if (listBrgKomponen.Count() > 0)
                                {
                                    ret = "1";
                                }
                                //end add by nurul 25/1/2021, bundling
                                new StokControllerJob().updateStockMarketPlace(connID, iden.DatabasePathErasoft, iden.username);
                            }
                            jmlhOrderCancel = jmlhOrderCancel + rowAffected;
                            if (jmlhOrderCancel > 0)
                            {
                                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderCancel) + " Pesanan dari 82Cart dibatalkan.");
                            }
                            //}
                        }
                    }
                }
                //}
                //catch (Exception ex2)
                //{
                //}
            }

            return ret;
        }


        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Status Pesanan {obj} ke 82Cart Gagal.")]
        public async Task<string> E2Cart_SetOrderStatus(E2CartAPIData iden, string dbPathEra, string log_CUST, string log_ActionCategory, string log_ActionName, string orderId, string codeStatus)
        {
            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            var dateFrom = DateTimeOffset.UtcNow.AddDays(-10).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            var dateTo = DateTimeOffset.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

            string urll = string.Format("{0}/api/v1/editOrder", iden.API_url);

            var orderIdFix = "";
            //handle orderid reference
            if (orderId.Contains(";"))
            {
                string[] splitOrderID = orderId.Split(';');
                orderIdFix = splitOrderID[0];
            }
            else
            {
                orderIdFix = orderId;
            }
            //end handle orderid reference

            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            postData += "&id_order=" + Uri.EscapeDataString(orderIdFix);
            postData += "&current_state=" + Uri.EscapeDataString(codeStatus);

            var data = Encoding.ASCII.GetBytes(postData);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = data.Length;

            string responseServer = "";

            try
            {
                using (var stream = myReq.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream2 = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream2);
                        responseServer = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {

            }


            if (responseServer != null)
            {
                //var listOrder = JsonConvert.DeserializeObject(responseServer, typeof(E2CartOrderResult)) as E2CartOrderResult;

                //var statusCancel = "6";
                //var orderFilterCanceled = listOrder.data.Where(p => p.current_state == statusCancel).ToList();
                //string ordersn = "";
                //foreach (var item in orderFilterCanceled)
                //{
                //    ordersn = ordersn + "'" + item + "',";
                //}
                //if (orderFilterCanceled.Count() > 0)
                //{
                //ordersn = ordersn.Substring(0, ordersn.Length - 1);
                //var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '03'");
                //jmlhNewOrder = jmlhNewOrder + rowAffected;
                ////if (listOrder.more)
                ////{
                ////    await GetOrderByStatusCompleted(iden, stat, CUST, NAMA_CUST, page + 50, jmlhNewOrder);
                ////}
                ////else
                ////{
                //if (jmlhNewOrder > 0)
                //{
                //    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                //    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhNewOrder) + " Pesanan dari 82Cart sudah selesai.");
                //}
                //}
                //}
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Harga Jual Produk {obj} ke 82Cart gagal.")]
        public async Task<string> E2Cart_UpdatePrice_82Cart(string dbPathEra, string kdbrg, string log_CUST, string log_ActionCategory, string log_ActionName, E2CartAPIData iden, string brg_mp, int priceInduk, string priceImpact)
        {
            SetupContext(iden);
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Price", //ganti
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.API_credential,
                REQUEST_STATUS = "Pending",
            };

            string[] brg_mp_split = brg_mp.Split(';');

            string urll = string.Format("{0}/api/v1/editProductdetail", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            if (brg_mp_split[1] == "0")
            {
                postData += "&id_product=" + Uri.EscapeDataString(brg_mp_split[0]);
                postData += "&price=" + Uri.EscapeDataString(priceInduk.ToString());
                postData += "&wholesale_price=" + Uri.EscapeDataString("0");
            }
            else
            {
                postData += "&id_product=" + Uri.EscapeDataString(brg_mp_split[0]);
                postData += "&id_product_attribute=" + Uri.EscapeDataString(brg_mp_split[1]);
                postData += "&price_attribute=" + Uri.EscapeDataString(priceImpact.ToString());
                postData += "&wholesale_price=" + Uri.EscapeDataString("0");

            }

            var data = Encoding.ASCII.GetBytes(postData);

            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = data.Length;

            string responseFromServer = "";
            try
            {
                using (var stream = myReq.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream2 = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream2);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
                throw new Exception(currentLog.REQUEST_EXCEPTION);
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                try
                {
                    var resultAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ResultUpdatePrice)) as ResultUpdatePrice;
                    if (resultAPI.error == "none" && resultAPI.data.Length > 0)
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
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = resultAPI.error.ToString();
                        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                        throw new Exception(currentLog.REQUEST_EXCEPTION);
                    }
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                    throw new Exception(currentLog.REQUEST_EXCEPTION);
                }

            }

            return "";
        }

        //Get All Attributes.
        public async Task<String> E2Cart_GetProduct_ForListImage(E2CartAPIData iden, string brg_mp)
        {
            //var ret = new BindingBase82Cart
            //{
            //    status = 0,
            //    recordCount = 0,
            //    exception = 0,
            //    totalData = 0
            //};
            string ret = "";
            string[] splitBrg = brg_mp.Split(';');
            string urll = string.Format("{0}/api/v1/getProduct?apiKey={1}&apiCredential={2}&id_product={3}", iden.API_url, iden.API_key, iden.API_credential, splitBrg[0]);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            try
            {
                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseServer = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            if (!string.IsNullOrEmpty(responseServer))
            {
                var vresultAttributeAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(E2CartProductListResult)) as E2CartProductListResult;
                if (vresultAttributeAPI.error == "none" && vresultAttributeAPI.data != null)
                {
                    if (vresultAttributeAPI.data.Count() > 0)
                    {
                        try
                        {
                            //ret.dataProductList = vresultAttributeAPI.data;
                            if (vresultAttributeAPI.data != null)
                            {
                                foreach(var dataListImages in vresultAttributeAPI.data.Select(p => p.image_product).ToList())
                                {
                                    foreach (var item in dataListImages) {
                                        var deleteImage = E2Cart_Delete_ImageList(iden, item.id_image);
                                    }
                                }
                                //foreach (var item in dataListImages) {
                                //    item.
                                //}
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }

            return ret;
        }

        public async Task<String> E2Cart_Delete_ImageList(E2CartAPIData iden, string id_image)
        {
            string urll = string.Format("{0}/api/v1/deleteProductImage", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            postData += "&id_image=" + Uri.EscapeDataString(id_image);

            var data = Encoding.ASCII.GetBytes(postData);

            myReq.Method = "POST";
            myReq.ContentType = "application/x-www-form-urlencoded";
            myReq.ContentLength = data.Length;

            using (var stream = myReq.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse) await myReq.GetResponseAsync();

            var responseServer = new StreamReader(response.GetResponseStream()).ReadToEnd();

            if (!string.IsNullOrEmpty(responseServer))
            {
                var resServer = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer);

                //ViewBag.response = resServer;
            }

            return "";
        }

        public async Task<BindingBase82Cart> E2Cart_GetOrdersState(E2CartAPIData iden)
        {
            var ret = new BindingBase82Cart
            {
                status = 0,
                recordCount = 0,
                exception = 0,
                totalData = 0
            };

            SetupContext(iden);

            string urll = string.Format("{0}/api/v1/getOrderStates?apiKey={1}&apiCredential={2}", iden.API_url, iden.API_key, iden.API_credential);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            try
            {
                using (WebResponse response = await myReq.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseServer = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            if (!string.IsNullOrEmpty(responseServer))
            {
                var resultOrderState = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(E2CartOrderStateResult)) as E2CartOrderStateResult;

                if (resultOrderState != null)
                {
                    if (resultOrderState.error == "none" && resultOrderState.data.Length > 0)
                    {
                        ret.dataObject = resultOrderState.data;
                    }
                }

            }
            return ret;
        }

        public enum StatusOrder
        {
            //IN_CANCEL = 1,
            CANCELLED = 6,
            READY_TO_SHIP = 4,
            COMPLETED = 5,
            //TO_RETURN = 5,
            PAID = 2,
            UNPAID = 23
        }


        public class resultDeleteImage
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public string results { get; set; }
        }


        public class E2CartProductListResult
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public E2CartProduct[] data { get; set; }
        }

        public class E2CartProduct
        {
            public string apiKey { get; set; }
            public string apiCredential { get; set; }
            public string id_product { get; set; }
            public string name { get; set; }
            public string ean13 { get; set; }
            public string upc { get; set; }
            public string active { get; set; }
            public string visibility { get; set; }
            public string available_for_order { get; set; }
            public string show_price { get; set; }
            public string online_only { get; set; }
            public string condition { get; set; }
            public string description_short { get; set; }
            public string description { get; set; }
            public string meta_keywords { get; set; }
            public string wholesale_price { get; set; }
            public string price { get; set; }
            public string on_sale { get; set; }
            public string meta_title { get; set; }
            public string meta_description { get; set; }
            public string link_rewrite { get; set; }
            public string id_category_default { get; set; }
            public string category_default { get; set; }
            public E2CartProductCategory[] category { get; set; }
            public string cover_image_url { get; set; }
            public Image_Product[] image_product { get; set; }
            public string id_manufacturer { get; set; }
            public object manufacturer { get; set; }
            public string width { get; set; }
            public string height { get; set; }
            public string depth { get; set; }
            public string weight { get; set; }
            public string additional_shipping_cost { get; set; }
            public int quantity { get; set; }
            public int minimal_quantity { get; set; }
            public int out_of_stock { get; set; }
            public string indexed { get; set; }
            public string date_add { get; set; }
            public string date_upd { get; set; }
            public ProductCombinations[] combinations { get; set; }
        }

        public class ProductCombinations
        {
            public string id_product_attribute { get; set; }
            public string attribute_reference { get; set; }
            public string attribute_ean13 { get; set; }
            public string upc { get; set; }
            public string wholesale_price { get; set; }
            public string price { get; set; }
            public string weight { get; set; }
            public string quantity { get; set; }
            public string default_on { get; set; }
            public attribute_image[] attribute_image { get; set; }
            public attribute_list[] attribute_list { get; set; }
        }

        public class attribute_list
        {
            public string id_attribute_group { get; set; }
            public string attribute_group { get; set; }
            public string id_attribute { get; set; }
            public string attribute { get; set; }
        }

        public class attribute_image
        {
            public long id_image { get; set; }
            public string image_url { get; set; }
        }

        public class Image_Product
        {
            public string id_image { get; set; }
            public string link_image { get; set; }
        }

        public class E2CartProductCategory
        {
            public string id_category { get; set; }
            public string category_name { get; set; }
            public string name { get; set; }
            public string id_parent { get; set; }
            public string level_depth { get; set; }
            public string active { get; set; }
            public string date_add { get; set; }
            public string date_upd { get; set; }
            public string position { get; set; }
            public E2CartProductCategory[] child { get; set; }

        }

        public class BindingBase82Cart
        {
            public int status { get; set; }
            public string message { get; set; }
            public int recordCount { get; set; }
            public int exception { get; set; }
            public int totalData { get; set; }
            public int nextPage { get; set; }
            public string id_category { get; set; }
            public string name_category { get; set; }
            public string id_manufacture { get; set; }
            public string name_manufacture { get; set; }
            public E2CartOrderState[] dataObject { get; set; }
            public E2CartProduct[] dataProductList { get; set; }
        }

        public class E2CartOrderStateResult
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public E2CartOrderState[] data { get; set; }
        }

        public class E2CartOrderState
        {
            public string id_order_state { get; set; }
            public string name { get; set; }
        }


        public class E2CartAPIData
        {
            public string no_cust { get; set; }
            public string username { get; set; }
            public string account_store { get; set; }
            public string API_key { get; set; }
            public string API_password { get; set; }
            public string DatabasePathErasoft { get; set; }
            public string email { get; set; }
            public int rec_num { get; set; }
            public string API_url { get; set; }
            public string ID_MARKET { get; set; }
            public string API_credential { get; set; }
        }

        public class ResultAddAttribute
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public string results { get; set; }
            public AttributeData data { get; set; }
        }

        public class AttributeData
        {
            public string id_product { get; set; }
            public string reference { get; set; }
            public string supplier_reference { get; set; }
            public string location { get; set; }
            public string ean13 { get; set; }
            public string upc { get; set; }
            public string wholesale_price { get; set; }
            public string price { get; set; }
            public string unit_price_impact { get; set; }
            public string ecotax { get; set; }
            public string minimal_quantity { get; set; }
            public string quantity { get; set; }
            public string weight { get; set; }
            public string default_on { get; set; }
            public string available_date { get; set; }
            public int id { get; set; }
            public object id_shop_list { get; set; }
            public bool force_id { get; set; }
        }

        public class ResultCreateProduct82Cart
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public string results { get; set; }
            public ResultCreateProduct82CartData data { get; set; }
        }

        public class ResultCreateProduct82CartData
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public ResultCreateProductDetail[] data { get; set; }
        }

        public class ResultCreateProductDetail
        {
            public string id_product { get; set; }
            public string name { get; set; }
            public string ean13 { get; set; }
            public string upc { get; set; }
            public string active { get; set; }
            public string visibility { get; set; }
            public string available_for_order { get; set; }
            public string show_price { get; set; }
            public string online_only { get; set; }
            public string condition { get; set; }
            public string description_short { get; set; }
            public string description { get; set; }
            public string meta_keywords { get; set; }
            public string wholesale_price { get; set; }
            public string price { get; set; }
            public string on_sale { get; set; }
            public string meta_title { get; set; }
            public string meta_description { get; set; }
            public string link_rewrite { get; set; }
            public string id_category_default { get; set; }
            public string category_default { get; set; }
            public ResultCreateProductDetailCategory[] category { get; set; }
            public string id_manufacturer { get; set; }
            public string manufacturer { get; set; }
            public string width { get; set; }
            public string height { get; set; }
            public string depth { get; set; }
            public string weight { get; set; }
            public string additional_shipping_cost { get; set; }
            public string quantity { get; set; }
            public string minimal_quantity { get; set; }
            public string out_of_stock { get; set; }
            public string indexed { get; set; }
            public string date_add { get; set; }
            public string date_upd { get; set; }
            public object[] combinations { get; set; }
        }

        public class ResultCreateProductDetailCategory
        {
            public string id_category { get; set; }
            public string category_name { get; set; }
        }

        public class E2CartOrderResult
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public E2CartOrder[] data { get; set; }
        }

        public class E2CartOrder
        {
            public string id_order { get; set; }
            public string reference { get; set; }
            public string current_state { get; set; }
            public string current_state_name { get; set; }
            public string id_carrier { get; set; }
            public string name_carrier { get; set; }
            public string shipping_number { get; set; }
            public string id_customer { get; set; }
            public string firstname { get; set; }
            public string lastname { get; set; }
            public string id_currency { get; set; }
            public string currency { get; set; }
            public string payment { get; set; }
            public string conversion_rate { get; set; }
            public string total_paid { get; set; }
            public string total_paid_tax_incl { get; set; }
            public string total_paid_tax_excl { get; set; }
            public string total_paid_real { get; set; }
            public string total_products { get; set; }
            public string total_discounts { get; set; }
            public string total_shipping { get; set; }
            public string total_wrapping { get; set; }
            public string invoice_date { get; set; }
            public string delivery_date { get; set; }
            public string date_add { get; set; }
            public address[] delivery_address { get; set; }
            public address[] invoice_address { get; set; }
            public order_detail[] order_detail { get; set; }
        }

        public class order_detail
        {
            public string product_id { get; set; }
            public string product_attribute_id { get; set; }
            public string product_name { get; set; }
            public string reference { get; set; }
            public string product_quantity { get; set; }
            public string original_product_price { get; set; }
            public string reduction_percent { get; set; }
            public string reduction_amount { get; set; }
            public string unit_price { get; set; }
            public string total_price { get; set; }
        }

        public class address
        {
            public string id_address { get; set; }
            public string id_country { get; set; }
            public string id_state { get; set; }
            public string id_customer { get; set; }
            public string id_manufacturer { get; set; }
            public string id_supplier { get; set; }
            public string id_warehouse { get; set; }
            public string alias { get; set; }
            public string company { get; set; }
            public string lastname { get; set; }
            public string firstname { get; set; }
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string postcode { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string other { get; set; }
            public string phone { get; set; }
            public string phone_mobile { get; set; }
            public string vat_number { get; set; }
            public string dni { get; set; }
            public string date_add { get; set; }
            public string date_upd { get; set; }
            public string active { get; set; }
            public string deleted { get; set; }
        }

        public class ResultUpdatePrice
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public DetailUpdatePrice[] data { get; set; }
        }

        public class DetailUpdatePrice
        {
            public string id_product { get; set; }
            public string active { get; set; }
            public string name { get; set; }
            public string visibility { get; set; }
            public string available_for_order { get; set; }
            public string show_price { get; set; }
            public string online_only { get; set; }
            public string wholesale_price { get; set; }
            public string price { get; set; }
            public string on_sale { get; set; }
            public string id_category_default { get; set; }
            public string id_manufacturer { get; set; }
            public string width { get; set; }
            public string height { get; set; }
            public string depth { get; set; }
            public string weight { get; set; }
            public string additional_shipping_cost { get; set; }
            public string minimal_quantity { get; set; }
            public string out_of_stock { get; set; }
            public string indexed { get; set; }
            public string quantity { get; set; }
            public string date_add { get; set; }
            public string date_upd { get; set; }
        }

        public class ResultUpdateStock82Cart
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public string results { get; set; }
            public object data { get; set; }
        }
    }
}