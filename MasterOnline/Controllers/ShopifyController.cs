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

namespace MasterOnline.Controllers
{
    public class ShopifyController : Controller
    {
        AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);//string auth = Base64Encode();
#if AWS
        string shpCallbackUrl = "https://masteronline.co.id/shp/code?user=";
#else
        string shpCallbackUrl = "https://dev.masteronline.co.id/shp/code?user=";
#endif

        protected int MOPartnerID = 841371;
        protected string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        DatabaseSQL EDB;
        string username;
        string DatabasePathErasoft;
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

        public ShopifyController()
        {
            MoDbContext = new MoDbContext("");
            username = "";
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
            {
                if (sessionData.Account.UserId == "admin_manage")
                    ErasoftDbContext = new ErasoftContext();
                else
                    ErasoftDbContext = new ErasoftContext(sessionData.Account.DataSourcePath, sessionData.Account.DatabasePathErasoft);

                EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);
                DatabasePathErasoft = sessionData.Account.DatabasePathErasoft;
                username = sessionData.Account.Username;
            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    ErasoftDbContext = new ErasoftContext(accFromUser.DataSourcePath, accFromUser.DatabasePathErasoft);
                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                    DatabasePathErasoft = accFromUser.DatabasePathErasoft;
                    username = accFromUser.Username;
                }
            }
            if (username.Length > 20)
                username = username.Substring(0, 17) + "...";
        }

        public async Task<string> Shopify_GetAccount(ShopifyAPIData dataAPI)
        {
            string ret = "";
            var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    //REQUEST_ID = seconds.ToString(),
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //    REQUEST_ACTION = "Update Price", //ganti
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = iden.no_cust,
            //    REQUEST_STATUS = "Pending",
            //};
            var vurl = "https://{0}:{1}@{2}.myshopify.com/admin/shop.json";
            var vformatUrl = String.Format(vurl, dataAPI.API_key, dataAPI.API_password, dataAPI.account_store);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.Headers.Add("X-Shopify-Access-Token", dataAPI.API_password);
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
                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToString().Contains("404"))
                {
                    contextNotif.Clients.Group(dataAPI.DatabasePathErasoft).notifTransaction("Data akun marketplace (Shopify) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                }
                else if (ex.Message.ToString().Contains("402") && ex.Message.ToString().ToLower().Contains("payment"))
                {
                    contextNotif.Clients.Group(dataAPI.DatabasePathErasoft).notifTransaction("Masa trial akun marketplace (Shopify) sudah habis.", false);
                }
                else
                {
                    contextNotif.Clients.Group(dataAPI.DatabasePathErasoft).notifTransaction("Info: " + ex.Message.ToString(), false);
                }
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (!String.IsNullOrWhiteSpace(DatabasePathErasoft.ToString()))
            {
                DatabaseSQL EDB = new DatabaseSQL(DatabasePathErasoft);
                var resultExecDefault = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST = '" + dataAPI.no_cust + "'");
                if (responseFromServer != "")
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetShopAccount)) as ShopifyGetShopAccount;

                        if (!String.IsNullOrWhiteSpace(result.ToString()))
                        {
                            if (result.shop != null && result.errors == null)
                            {
                                if (result.shop.email == dataAPI.email || result.shop.customer_email == dataAPI.email)
                                {
                                    if (!String.IsNullOrWhiteSpace(DatabasePathErasoft.ToString()))
                                    {
                                        var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '1', AL = '" + result.shop.address1 + "', Sort1_Cust = '" + result.shop.id + "', TLP = '" + result.shop.phone + "' WHERE CUST = '" + dataAPI.no_cust + "'");
                                        if (resultquery != 0)
                                        {
                                            contextNotif.Clients.Group(dataAPI.DatabasePathErasoft).notifTransaction("Akun marketplace " + result.shop.name.ToString() + " (Shopify) berhasil aktif", true);
                                            //currentLog.REQUEST_RESULT = "Update Status API Complete";
                                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, dataAPI, currentLog);
                                        }
                                        else
                                        {
                                            contextNotif.Clients.Group(dataAPI.DatabasePathErasoft).notifTransaction("Akun marketplace (Shopify) gagal diaktifkan", true);
                                            //currentLog.REQUEST_RESULT = "Update Status API Failed";
                                            //currentLog.REQUEST_EXCEPTION = "Failed Update Table";
                                            //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, dataAPI, currentLog);
                                        }
                                    }
                                    else
                                    {
                                        //currentLog.REQUEST_RESULT = "Tidak dapat variable DatabasePathErasoft";
                                        //currentLog.REQUEST_EXCEPTION = "Failed Get Variable DatabasePathErasoft";
                                        //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, dataAPI, currentLog);

                                    }
                                }
                            }
                        }
                        //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    }
                    catch (Exception ex2)
                    {
                        contextNotif.Clients.Group(dataAPI.DatabasePathErasoft).notifTransaction("Marketplace Shopify gagal diaktifkan. Mohon hubungi support Master Online.", false);
                        //currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                        //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                    }
                }
            }
            return ret;
        }

        public async Task<BindingBase> Shopify_GetProductList_Sync(ShopifyAPIData iden, int IdMarket, int page, int recordCount, int totalData)
        {
            var ret = new BindingBase
            {
                status = 0,
                recordCount = recordCount,
                exception = 0,
                totalData = totalData//add 18 Juli 2019, show total record
            };

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = seconds.ToString() + "_" + page,
                REQUEST_ACTION = "Get Item List",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.no_cust,
                //REQUEST_ATTRIBUTE_3 = page.ToString(),
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/product_listings.json?limit={3}&page={4}";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store, 10, page);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.Headers.Add("X-Shopify-Access-Token", iden.API_password);
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
                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                ret.exception = 1;
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (!string.IsNullOrEmpty(responseFromServer))
            {
                try
                {
                    var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetItemListResult)) as ShopifyGetItemListResult;

                    ret.status = 1;
                    if (listBrg != null)
                    {
                        if (listBrg.product_listings != null)
                        {
                            if (listBrg.product_listings.Count() > 0)
                            {
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);

                                if (listBrg.product_listings.Count() == 10)
                                    ret.nextPage = 1;

                                List<TEMP_BRG_MP> listNewRecord = new List<TEMP_BRG_MP>();
                                var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.IDMARKET == IdMarket && t.BRG_MP != null).Select(t => new { t.CUST, t.BRG_MP }).ToList();
                                //var tempBrg_local = (from a in ErasoftDbContext.TEMP_BRG_MP where a.IDMARKET == IdMarket select new tempBrg_local { BRG_MP = a.BRG_MP, IDMARKET = a.IDMARKET }).ToList();
                                var brgInDB = ErasoftDbContext.STF02H.Where(t => t.IDMARKET == IdMarket && t.BRG_MP != null).Select(t => new { t.RecNum, t.BRG_MP }).ToList();
                                string brgMp = "";

                                foreach (var item in listBrg.product_listings)
                                {
                                    if (item.variants.Count() == 1)
                                    {
                                        foreach (var cekbrgMP in item.variants)
                                        {
                                            brgMp = Convert.ToString(item.product_id) + ";" + cekbrgMP.id;
                                        }
                                    }
                                    else
                                    {
                                        brgMp = Convert.ToString(item.product_id) + ";0";
                                    }
                                    //if (item.status.ToUpper() != "DELETE" && item.status.ToUpper() != "BANNED")
                                    if (item.available == true)
                                    {
                                        ret.totalData++;//add 18 Juli 2019, show total record
                                        var CektempbrginDB = tempbrginDB.Where(t => t.BRG_MP.ToUpper() == brgMp.ToUpper()).SingleOrDefault();
                                        //var CekbrgInDB = brgInDB.Where(t => t.BRG_MP.Equals(brgMp)).FirstOrDefault();
                                        var CekbrgInDB = brgInDB.Where(t => t.BRG_MP.ToUpper() == brgMp.ToUpper()).SingleOrDefault();
                                        if (CektempbrginDB == null && CekbrgInDB == null)
                                        {
                                            string namaBrg = item.title;
                                            string nama, nama2, nama3, urlImage, urlImage2, urlImage3;
                                            nama = "";
                                            nama2 = "";
                                            nama3 = "";
                                            urlImage = "";
                                            urlImage2 = "";
                                            urlImage3 = "";
                                            if (namaBrg != null)
                                            {
                                                namaBrg = namaBrg.Replace('\'', '`');

                                                var splitItemName = new StokControllerJob().SplitItemName(namaBrg);
                                                nama = splitItemName[0];
                                                nama2 = splitItemName[1];
                                                nama3 = "";
                                            }

                                            Models.TEMP_BRG_MP newrecord = new TEMP_BRG_MP()
                                            {
                                                //SELLER_SKU = brgMp,
                                                //BRG_MP = brgMp,
                                                //KODE_BRG_INDUK = Convert.ToString(item.id_product),
                                                NAMA = nama,
                                                NAMA2 = nama2,
                                                NAMA3 = nama3,
                                                //CATEGORY_CODE = Convert.ToString(product_induk.id_category_default),
                                                CATEGORY_CODE = "1",
                                                CATEGORY_NAME = item.product_type,
                                                IDMARKET = IdMarket,
                                                //IMAGE = item.cover_image_url ?? "",
                                                DISPLAY = true,
                                                //HJUAL = Convert.ToDouble(item.price),
                                                //HJUAL_MP = Convert.ToDouble(item.price),
                                                Deskripsi = item.body_html.Replace("\r\n", "<br />").Replace("\n", "<br />"),
                                                MEREK = item.vendor,
                                                CUST = iden.no_cust,
                                            };
                                            newrecord.AVALUE_45 = namaBrg.Length > 250 ? namaBrg.Substring(0, 250) : namaBrg; //request by Calvin 19 maret 2019, isi nama barang ke avalue 45
                                                                                                                              //add by Tri, 26 Feb 2019
                                            var kategory = MoDbContext.CategoryShopify.Where(m => m.NAME == newrecord.CATEGORY_NAME).FirstOrDefault();
                                            if (kategory != null)
                                            {
                                                newrecord.CATEGORY_CODE = Convert.ToString(kategory.RecNum);
                                            }
                                            else
                                            {
                                                List<CATEGORY_SHOPIFY> listNewRecordCategory = new List<CATEGORY_SHOPIFY>();
                                                Models.CATEGORY_SHOPIFY newrecordCategory = new CATEGORY_SHOPIFY()
                                                {
                                                    ACTIVE = "1",
                                                    ID_CATEGORY = "",
                                                    ID_PARENT = "",
                                                    LEVEL_DEPTH = "",
                                                    POSITION = "",
                                                    NAME = newrecord.CATEGORY_NAME
                                                };
                                                listNewRecordCategory.Add(newrecordCategory);
                                                MoDbContext.CategoryShopify.AddRange(listNewRecordCategory);
                                                MoDbContext.SaveChanges();
                                                var kategoryCheck = MoDbContext.CategoryShopify.Where(m => m.NAME == newrecord.CATEGORY_NAME).FirstOrDefault();
                                                newrecord.CATEGORY_CODE = Convert.ToString(kategoryCheck.RecNum);
                                            }

                                            int typeBrg = 0;
                                            //if (!string.IsNullOrEmpty(Convert.ToString(item.other.sku)))
                                            //    newrecord.SELLER_SKU = item.other.sku;
                                            if (item.variants.Count() == 1)//barang non varian
                                            {
                                                newrecord.TYPE = "3";
                                                foreach (var varID in item.variants)
                                                {
                                                    if (varID.sku != null)
                                                    {
                                                        newrecord.SELLER_SKU = varID.sku;
                                                    }
                                                    else
                                                    {
                                                        newrecord.SELLER_SKU = brgMp;
                                                    }
                                                    newrecord.BRG_MP = brgMp;
                                                    newrecord.HJUAL = Convert.ToDouble(varID.price);
                                                    newrecord.HJUAL_MP = Convert.ToDouble(varID.price);
                                                    if (Convert.ToDouble(varID.weight) >= 0)
                                                    {
                                                        if (varID.weight_unit.Contains("kg"))
                                                        {
                                                            newrecord.BERAT = Convert.ToDouble(varID.weight) * 1000;
                                                        }
                                                        else
                                                        {
                                                            newrecord.BERAT = Convert.ToDouble(varID.weight);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (item.variants.Count() > 1)
                                                {
                                                    typeBrg = 1;
                                                    newrecord.TYPE = "4";
                                                    foreach (var varID in item.variants)
                                                    {
                                                        var brg_mp_variant = Convert.ToString(item.product_id) + ";" + varID.id.ToString();
                                                        var CektempbrginDB2 = tempbrginDB.Where(t => t.BRG_MP.ToUpper() == brg_mp_variant.ToUpper()).SingleOrDefault();
                                                        var CekbrgInDB2 = brgInDB.Where(t => t.BRG_MP.ToUpper() == brg_mp_variant.ToUpper()).SingleOrDefault();
                                                        if (CektempbrginDB2 == null && CekbrgInDB2 == null)
                                                        {
                                                            var retVar = await Shopify_GetProductVariant_Sync(iden, item, varID, brg_mp_variant, brgMp, iden.no_cust, IdMarket);
                                                            ret.recordCount += retVar.recordCount;
                                                        }
                                                    }
                                                    ret.totalData += item.variants.Count();
                                                }
                                                else
                                                {
                                                    //newrecord.TYPE = "3";
                                                    //newrecord.KODE_BRG_INDUK = Convert.ToString(item.id_product);
                                                    typeBrg = 2;
                                                }
                                            }
                                            //if (Convert.ToDouble(item.weight) >= 0)
                                            //{
                                            //    newrecord.BERAT = Convert.ToDouble(item.weight);
                                            //}
                                            //if (item.height != null)
                                            //{
                                            //    if (Convert.ToDouble(item.height) > 0)
                                            //    {
                                            //        newrecord.TINGGI = Convert.ToDouble(item.height);
                                            //    }
                                            //}
                                            //if (item.width != null)
                                            //{
                                            //    if (Convert.ToDouble(item.width) > 0)
                                            //    {
                                            //        newrecord.LEBAR = Convert.ToDouble(item.width);
                                            //    }
                                            //}
                                            //if (item.menu != null)
                                            //{
                                            //    if (!string.IsNullOrEmpty(Convert.ToString(item.menu.id)))
                                            //    {
                                            //        newrecord.PICKUP_POINT = Convert.ToString(item.menu.id);
                                            //    }
                                            //}
                                            if (item.images != null)
                                                if (item.images.Count() > 0)
                                                {
                                                    foreach (var itemImages in item.images)
                                                    {
                                                        if (itemImages.variant_ids.Count() == 0)
                                                        {
                                                            if (newrecord.IMAGE == null)
                                                            {
                                                                newrecord.IMAGE = itemImages.src;
                                                            }

                                                            if (newrecord.IMAGE2 == null && newrecord.IMAGE != itemImages.src)
                                                            {
                                                                newrecord.IMAGE2 = itemImages.src;
                                                            }

                                                            if (newrecord.IMAGE3 == null && newrecord.IMAGE != itemImages.src && newrecord.IMAGE2 != itemImages.src)
                                                            {
                                                                newrecord.IMAGE3 = itemImages.src;
                                                            }

                                                            if (newrecord.IMAGE4 == null && newrecord.IMAGE != itemImages.src && newrecord.IMAGE2 != itemImages.src && newrecord.IMAGE3 != itemImages.src)
                                                            {
                                                                newrecord.IMAGE4 = itemImages.src;
                                                            }

                                                            if (newrecord.IMAGE5 == null && newrecord.IMAGE != itemImages.src && newrecord.IMAGE2 != itemImages.src && newrecord.IMAGE3 != itemImages.src && newrecord.IMAGE4 != itemImages.src)
                                                            {
                                                                newrecord.IMAGE5 = itemImages.src;
                                                            }

                                                        }
                                                    }

                                                    //newrecord.IMAGE = product_varian.attribute_image[0].image_url;
                                                    //if (product_varian.attribute_image.Length > 1 && typeBrg != 2)
                                                    //{
                                                    //    newrecord.IMAGE2 = product_varian.attribute_image[1].image_url;
                                                    //    if (product_varian.attribute_image.Length > 2)
                                                    //    {
                                                    //        newrecord.IMAGE3 = product_varian.attribute_image[2].image_url;
                                                    //        if (product_varian.attribute_image.Length > 3)
                                                    //        {
                                                    //            newrecord.IMAGE4 = product_varian.attribute_image[3].image_url;
                                                    //            if (product_varian.attribute_image.Length > 4)
                                                    //            {
                                                    //                newrecord.IMAGE5 = product_varian.attribute_image[4].image_url;

                                                    //            }
                                                    //        }
                                                    //    }
                                                    //}
                                                }

                                            listNewRecord.Add(newrecord);
                                            ret.recordCount = ret.recordCount + 1;
                                        }
                                        else if (item.variants.Length > 1)
                                        {
                                            foreach (var varID in item.variants)
                                            {
                                                var brg_mp_variant = Convert.ToString(item.product_id) + ";" + varID.id.ToString();
                                                var CektempbrginDB2 = tempbrginDB.Where(t => (t.BRG_MP ?? "").Equals(brg_mp_variant)).FirstOrDefault();
                                                var CekbrgInDB2 = brgInDB.Where(t => (t.BRG_MP ?? "").Equals(brg_mp_variant)).FirstOrDefault();
                                                if (CektempbrginDB2 == null && CekbrgInDB2 == null)
                                                {
                                                    var retVar = await Shopify_GetProductVariant_Sync(iden, item, varID, brg_mp_variant, brgMp, iden.no_cust, IdMarket);
                                                    ret.recordCount += retVar.recordCount;
                                                }
                                            }
                                            ret.totalData += item.variants.Count();
                                        }
                                    }

                                }
                                if (listNewRecord.Count() > 0)
                                {
                                    ErasoftDbContext.TEMP_BRG_MP.AddRange(listNewRecord);
                                    ErasoftDbContext.SaveChanges();
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
                        ret.message = "Gagal mendapatkan produk";
                        currentLog.REQUEST_EXCEPTION = ret.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                        ret.nextPage = 0;
                    }


                }
                catch (Exception ex2)
                {
                    ret.exception = 1;
                    ret.message = ex2.Message.ToString();
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }

        public async Task<BindingBase> Shopify_GetProductVariant_Sync(ShopifyAPIData iden, ShopifyGetItemListResultProduct product_induk, ShopifyGetItemListResultProductVariant product_varian, string brgmp_varian, string brg_mp_induk, string CUST, int idmarket)
        {
            var ret = new BindingBase();

            string status = "";
            List<TEMP_BRG_MP> listNewRecord = new List<TEMP_BRG_MP>();
            ret.totalData++;

            string namaBrg = product_induk.title;
            string nama, nama2, nama3, urlImage, urlImage2, urlImage3;
            urlImage = "";
            urlImage2 = "";
            urlImage3 = "";
            if (namaBrg != null)
                namaBrg = namaBrg.Replace('\'', '`');

            var splitItemName = new StokControllerJob().SplitItemName(namaBrg);
            nama = splitItemName[0];
            nama2 = splitItemName[1];
            if (!string.IsNullOrEmpty(product_varian.title))
            {
                nama2 = product_varian.title.ToString();
            }
            nama3 = "";

            double hargaJual = 0;
            if (Convert.ToDouble(product_varian.price) > 0)
            {
                hargaJual = Convert.ToDouble(product_varian.price);
            }
            else
            {
                hargaJual = 0;
            }

            var skuBRG = "";
            if (!string.IsNullOrEmpty(product_varian.sku))
            {
                skuBRG = product_varian.sku;
            }
            else
            {
                skuBRG = brgmp_varian;
            }

            Models.TEMP_BRG_MP newrecord = new TEMP_BRG_MP()
            {
                SELLER_SKU = skuBRG,
                BRG_MP = brgmp_varian,
                KODE_BRG_INDUK = brg_mp_induk,
                TYPE = "3",
                NAMA = nama,
                NAMA2 = nama2,
                NAMA3 = nama3,
                //CATEGORY_CODE = Convert.ToString(product_induk.id_category_default),
                CATEGORY_CODE = "1",
                CATEGORY_NAME = product_induk.product_type,
                IDMARKET = idmarket,
                IMAGE = "",
                DISPLAY = true,
                HJUAL = hargaJual,
                HJUAL_MP = hargaJual,
                Deskripsi = product_induk.body_html.Replace("\r\n", "<br />").Replace("\n", "<br />"),
                MEREK = product_induk.vendor,
                CUST = CUST,
            };
            newrecord.AVALUE_45 = namaBrg.Length > 250 ? namaBrg.Substring(0, 250) : namaBrg; //request by Calvin 19 maret 2019, isi nama barang ke avalue 45
                                                                                              //add by Tri, 26 Feb 2019
            var kategory = MoDbContext.CategoryShopify.Where(m => m.NAME == newrecord.CATEGORY_NAME).FirstOrDefault();
            if (kategory != null)
            {
                newrecord.CATEGORY_CODE = Convert.ToString(kategory.RecNum);
            }
            else
            {
                List<CATEGORY_SHOPIFY> listNewRecordCategory = new List<CATEGORY_SHOPIFY>();
                Models.CATEGORY_SHOPIFY newrecordCategory = new CATEGORY_SHOPIFY()
                {
                    NAME = newrecord.CATEGORY_NAME
                };
                listNewRecordCategory.Add(newrecordCategory);
                MoDbContext.CategoryShopify.AddRange(listNewRecordCategory);
                MoDbContext.SaveChanges();
                var kategoryCheck = MoDbContext.CategoryShopify.Where(m => m.NAME == newrecord.CATEGORY_NAME).FirstOrDefault();
                newrecord.CATEGORY_CODE = Convert.ToString(kategoryCheck.RecNum);
            }

            // typeBrg : 0 = barang tanpa varian; 1 = barang induk; 2 = barang varian
            int typeBrg = 0;
            //if (!string.IsNullOrEmpty(Convert.ToString(item.other.sku)))
            //    newrecord.SELLER_SKU = item.other.sku;
            //if (!item.variant.isVariant)//barang non varian
            //{
            //    newrecord.TYPE = "3";
            //}
            //else
            //{
            //if (item.variant.isParent)
            //{
            typeBrg = 1;
            //}
            //else
            //{
            //    newrecord.TYPE = "3";
            //    newrecord.KODE_BRG_INDUK = Convert.ToString(item.variant.parentID);
            //    typeBrg = 2;
            //}
            //}
            if (Convert.ToDouble(product_varian.weight) >= 0)
            {
                if (product_varian.weight_unit.Contains("kg"))
                {
                    newrecord.BERAT = Convert.ToDouble(product_varian.weight) * 1000;
                }
                else
                {
                    newrecord.BERAT = Convert.ToDouble(product_varian.weight);
                }
            }
            newrecord.TINGGI = 0;
            newrecord.LEBAR = 0;
            newrecord.PANJANG = 0;
            //if (product_induk.height != null)
            //{
            //    if (Convert.ToDouble(product_induk.height) > 0)
            //    {
            //        newrecord.TINGGI = Convert.ToDouble(product_induk.height);
            //    }
            //}
            //if (product_induk.width != null)
            //{
            //    if (Convert.ToDouble(product_induk.width) > 0)
            //    {
            //        newrecord.LEBAR = Convert.ToDouble(product_induk.width);
            //    }
            //}
            //if (item.menu != null)
            //{
            //    if (!string.IsNullOrEmpty(Convert.ToString(item.menu.id)))
            //    {
            //        newrecord.PICKUP_POINT = Convert.ToString(item.menu.id);
            //    }
            //}
            if (product_varian.image_id != null)
                if (product_induk.images.Count() > 0)
                {
                    foreach (var itemImages in product_induk.images)
                    {
                        if (Convert.ToString(itemImages.id) == Convert.ToString(product_varian.image_id))
                        {
                            newrecord.IMAGE = itemImages.src;
                        }
                    }

                    //newrecord.IMAGE = product_varian.attribute_image[0].image_url;
                    //if (product_varian.attribute_image.Length > 1 && typeBrg != 2)
                    //{
                    //    newrecord.IMAGE2 = product_varian.attribute_image[1].image_url;
                    //    if (product_varian.attribute_image.Length > 2)
                    //    {
                    //        newrecord.IMAGE3 = product_varian.attribute_image[2].image_url;
                    //        if (product_varian.attribute_image.Length > 3)
                    //        {
                    //            newrecord.IMAGE4 = product_varian.attribute_image[3].image_url;
                    //            if (product_varian.attribute_image.Length > 4)
                    //            {
                    //                newrecord.IMAGE5 = product_varian.attribute_image[4].image_url;

                    //            }
                    //        }
                    //    }
                    //}
                }

            listNewRecord.Add(newrecord);
            ret.recordCount = ret.recordCount + 1;

            if (listNewRecord.Count > 0)
            {
                ErasoftDbContext.TEMP_BRG_MP.AddRange(listNewRecord);
                try
                {
                    ErasoftDbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    ret.message = ex.Message.ToString();
                }
            }
            return ret;
        }

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

        public async Task<BindingBase> CheckProduct(ShopifyAPIData iden, string item_id, string brg)
        {
            var ret = new BindingBase
            {
                status = 0,
                recordCount = 0,
                exception = 0,
                totalData = 0//add 18 Juli 2019, show total record
            };

            string[] brg_mp = item_id.Split(';');

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/products/{3}.json";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store, brg_mp[0].ToString());

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.Headers.Add("X-Shopify-Access-Token", iden.API_password);
            myReq.Accept = "application/json";
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
                //if (ex.Message.Contains("404")) {
                ret.status = 0;
                ret.recordCount = 0;
                ret.totalData = 0;
                //}
            }

            if (responseFromServer != null)
            {
                try
                {
                    var detailBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetItemDetailResult)) as ShopifyGetItemDetailResult;
                    if (detailBrg != null)
                    {
                        if (detailBrg.product != null)
                        {
                            if (Convert.ToString(detailBrg.product.id) != null)
                            {
                                ret.status = 1;
                                ret.recordCount = 1;
                                ret.totalData = 1;
                            }
                        }

                    }
                }
                catch (Exception ex2)
                {
                    ret.status = 0;
                    ret.recordCount = 0;
                    ret.totalData = 0;
                }
            }

            if (ret.status == 1)
            {
                //Task.Run(() => UpdateProduct(iden, brg_mp[0], iden.no_cust).Wait());
                UpdateProduct(iden, brg, iden.no_cust);
            }
            else
            {
                CreateProduct(iden, brg, iden.no_cust);
            }

            return ret;
        }

        public async Task<BindingBase> GetItemDetail(ShopifyAPIData iden, long item_id, List<tempBrg_local> tempBrg_local, List<stf02h_local> stf02h_local, int IdMarket)
        {
            var ret = new BindingBase
            {
                status = 0,
                recordCount = 0,
                exception = 0,
                totalData = 0//add 18 Juli 2019, show total record
            };

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //string urll = "https://partner.shopeemobile.com/api/v1/item/get";
            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/products/{3}.json";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store, item_id.ToString());

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.Headers.Add("X-Shopify-Access-Token", iden.API_password);
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
                ret.exception = 1;
                ret.message = ex.Message;
            }

            if (responseFromServer != null)
            {
                try
                {
                    var detailBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetItemDetailResult)) as ShopifyGetItemDetailResult;
                    if (detailBrg != null)
                    {
                        if (detailBrg.product != null)
                        {
                            //string IdMarket = ErasoftDbContext.ARF01.Where(c => c.Sort1_Cust.Equals(iden.no_cust)).FirstOrDefault().RecNum.ToString();
                            string cust = ErasoftDbContext.ARF01.Where(c => c.Sort1_Cust == iden.no_cust).FirstOrDefault().CUST.ToString();
                            string categoryCode = detailBrg.product.vendor.ToString();
                            string price = detailBrg.product.variants[0].price;
                            var categoryInDB = MoDbContext.CategoryShopify.Where(p => p.ID_CATEGORY == categoryCode).FirstOrDefault();
                            //var categoryInDB = "";
                            string categoryName = detailBrg.product.vendor.ToString();
                            if (categoryInDB != null)
                            {
                                categoryName = categoryInDB.NAME;
                                //categoryName = "";
                            }
                            ret.status = 1;

                            var sellerSku = "";
                            //if (string.IsNullOrEmpty(sellerSku))
                            //{
                            //    var nm = barang_id.Split(';');
                            //    if (nm.Length > 1)
                            //    {
                            //        sellerSku = nm[1];
                            //    }
                            //    else
                            //    {
                            //        sellerSku = barang_id;
                            //    }
                            //}
                            if (detailBrg.product.variants.Length > 0)
                            {
                                ret.totalData += detailBrg.product.variants.Count();//add 18 Juli 2019, show total record
                                //insert brg induk
                                //string brgMpInduk = Convert.ToString(detailBrg.item.item_id) + ";";
                                string brgMpInduk = Convert.ToString(detailBrg.product.id) + ";0";
                                //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.ToUpper().Equals(brgMpInduk.ToUpper()) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
                                //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(brgMpInduk) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
                                var tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMpInduk.ToUpper()).FirstOrDefault();
                                var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMpInduk.ToUpper()).FirstOrDefault();
                                //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.IDMARKET == IdMarket && (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMpInduk.ToUpper()).FirstOrDefault();
                                //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.IDMARKET == IdMarket && (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMpInduk.ToUpper()).FirstOrDefault();
                                if (tempbrginDB == null && brgInDB == null)
                                {
                                    //ret.recordCount++;
                                    var ret1 = await proses_Item_detail(detailBrg, categoryCode, categoryName, cust, IdMarket, brgMpInduk, detailBrg.product.title, "NORMAL", Convert.ToDouble(detailBrg.product.variants[0].price), brgMpInduk, 1, "", iden, true);
                                    ret.recordCount += ret1.status;
                                }
                                else if (brgInDB != null)
                                {
                                    brgMpInduk = brgInDB.BRG;
                                }
                                //string skuInduk = string.IsNullOrEmpty(detailBrg.item.item_sku) ? brgMpInduk : detailBrg.item.item_sku;
                                //end insert brg induk
                                var insert_1st_img = true;

                                foreach (var item in detailBrg.product.variants)
                                {
                                    sellerSku = item.sku;
                                    //remark 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                                    //if (string.IsNullOrEmpty(sellerSku))
                                    //{
                                    //    sellerSku = item.variation_id.ToString();
                                    //}
                                    //end remark 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                                    string brgMp = Convert.ToString(detailBrg.product.id) + ";" + Convert.ToString(item.id);
                                    //tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.ToUpper().Equals(brgMp.ToUpper()) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
                                    //brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(brgMp) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
                                    tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
                                    brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
                                    //tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.IDMARKET == IdMarket && (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
                                    //brgInDB = ErasoftDbContext.STF02H.Where(t => t.IDMARKET == IdMarket && (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
                                    if (tempbrginDB == null && brgInDB == null)
                                    {
                                        //ret.recordCount++;
                                        var ret2 = await proses_Item_detail(detailBrg, categoryCode, categoryName, cust, IdMarket, brgMp, detailBrg.product.title + " " + item.title, "NORMAL", Convert.ToDouble(item.price), sellerSku, 2, brgMpInduk, iden, insert_1st_img);
                                        //var ret2 = await proses_Item_detail(detailBrg, categoryCode, categoryName, cust, IdMarket, brgMp, detailBrg.item.name + " " + item.name, item.status, item.original_price, sellerSku, 2, skuInduk, iden, insert_1st_img);
                                        ret.recordCount += ret2.status;
                                        insert_1st_img = false;//varian ke-2 tidak perlu ambil gambar
                                    }
                                }
                            }
                            else
                            {
                                //change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel
                                //sellerSku = string.IsNullOrEmpty(detailBrg.item.item_sku) ? detailBrg.item.item_id.ToString() : detailBrg.item.item_sku;
                                sellerSku = detailBrg.product.variants[0].sku;
                                //end change 17 juli 2019, jika seller sku kosong biarkan kosong di tabel

                                //ret.recordCount++;
                                var ret0 = await proses_Item_detail(detailBrg, categoryCode, categoryName, cust, IdMarket, Convert.ToString(detailBrg.product.id) + ";0", detailBrg.product.title, "NORMAL", Convert.ToDouble(detailBrg.product.variants[0].price), sellerSku, 0, "", iden, true);
                                ret.recordCount += ret0.status;
                            }
                        }
                        else
                        {
                            ret.message = responseFromServer;
                        }

                    }
                }
                catch (Exception ex2)
                {
                    ret.exception = 1;
                    ret.message = ex2.Message;
                }
            }
            return ret;
        }


        public class ShopifyUpdateHarga
        {
            public ShopifyUpdateHargaProduct product { get; set; }
        }

        public class ShopifyUpdateHargaProduct
        {
            public long id { get; set; }
            public List<ShopifyUpdateHargaProductVariant> variants { get; set; }
        }

        public class ShopifyUpdateHargaProductVariant
        {
            //public long id { get; set; }
            public long product_id { get; set; }
            public string price { get; set; }
        }

        public class ShopifyUpdateStockResult
        {
            public ShopifyUpdateStockResultProduct product { get; set; }
            public ShopifyUpdateStockResultError errors { get; set; }
        }

        public class ShopifyUpdateStockResultError
        {
            public string[] inventory_quantity { get; set; }
        }

        public class ShopifyUpdateStockResultProduct
        {
            public long id { get; set; }
            public string title { get; set; }
            public string body_html { get; set; }
            public string vendor { get; set; }
            public string product_type { get; set; }
            public DateTime created_at { get; set; }
            public string handle { get; set; }
            public DateTime updated_at { get; set; }
            public DateTime published_at { get; set; }
            public string template_suffix { get; set; }
            public string published_scope { get; set; }
            public string tags { get; set; }
            public string admin_graphql_api_id { get; set; }
            public ShopifyUpdateStockResultProductVariant[] variants { get; set; }
            public ShopifyUpdateStockResultProductOption[] options { get; set; }
            public ShopifyUpdateStockResultProductImage1[] images { get; set; }
            public ShopifyUpdateStockResultProductImage image { get; set; }
        }

        public class ShopifyUpdateStockResultProductImage
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyUpdateStockResultProductVariant
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string sku { get; set; }
            public int position { get; set; }
            public string inventory_policy { get; set; }
            public object compare_at_price { get; set; }
            public string fulfillment_service { get; set; }
            public string inventory_management { get; set; }
            public string option1 { get; set; }
            public object option2 { get; set; }
            public object option3 { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public bool taxable { get; set; }
            public string barcode { get; set; }
            public int grams { get; set; }
            public object image_id { get; set; }
            public float weight { get; set; }
            public string weight_unit { get; set; }
            public long inventory_item_id { get; set; }
            public int inventory_quantity { get; set; }
            public int old_inventory_quantity { get; set; }
            public bool requires_shipping { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyUpdateStockResultProductOption
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string name { get; set; }
            public int position { get; set; }
            public string[] values { get; set; }
        }

        public class ShopifyUpdateStockResultProductImage1
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }


        public class ShopifyResultOrders
        {
            public ShopifyResultListOrders[] orders { get; set; }
        }

        public class ShopifyResultListOrders
        {
            public long id { get; set; }
            public string email { get; set; }
            public object closed_at { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public int number { get; set; }
            public string note { get; set; }
            public string token { get; set; }
            public string gateway { get; set; }
            public bool test { get; set; }
            public string total_price { get; set; }
            public string subtotal_price { get; set; }
            public int total_weight { get; set; }
            public string total_tax { get; set; }
            public bool taxes_included { get; set; }
            public string currency { get; set; }
            public string financial_status { get; set; }
            public bool confirmed { get; set; }
            public string total_discounts { get; set; }
            public string total_line_items_price { get; set; }
            public object cart_token { get; set; }
            public bool buyer_accepts_marketing { get; set; }
            public string name { get; set; }
            public object referring_site { get; set; }
            public object landing_site { get; set; }
            public object cancelled_at { get; set; }
            public object cancel_reason { get; set; }
            public string total_price_usd { get; set; }
            public object checkout_token { get; set; }
            public object reference { get; set; }
            public long user_id { get; set; }
            public long location_id { get; set; }
            public object source_identifier { get; set; }
            public object source_url { get; set; }
            public DateTime processed_at { get; set; }
            public object device_id { get; set; }
            public object phone { get; set; }
            public object customer_locale { get; set; }
            public int app_id { get; set; }
            public object browser_ip { get; set; }
            public object landing_site_ref { get; set; }
            public int order_number { get; set; }
            public object[] discount_applications { get; set; }
            public object[] discount_codes { get; set; }
            public object[] note_attributes { get; set; }
            public string[] payment_gateway_names { get; set; }
            public string processing_method { get; set; }
            public object checkout_id { get; set; }
            public string source_name { get; set; }
            public object fulfillment_status { get; set; }
            public Tax_Lines[] tax_lines { get; set; }
            public string tags { get; set; }
            public object contact_email { get; set; }
            public string order_status_url { get; set; }
            public string presentment_currency { get; set; }
            public Total_Line_Items_Price_Set total_line_items_price_set { get; set; }
            public Total_Discounts_Set total_discounts_set { get; set; }
            public Total_Shipping_Price_Set total_shipping_price_set { get; set; }
            public Subtotal_Price_Set subtotal_price_set { get; set; }
            public Total_Price_Set total_price_set { get; set; }
            public Total_Tax_Set total_tax_set { get; set; }
            public Line_Items[] line_items { get; set; }
            public object[] fulfillments { get; set; }
            public object[] refunds { get; set; }
            public string total_tip_received { get; set; }
            public string admin_graphql_api_id { get; set; }
            public object[] shipping_lines { get; set; }
        }

        public class Total_Line_Items_Price_Set
        {
            public Shop_Money shop_money { get; set; }
            public Presentment_Money presentment_money { get; set; }
        }

        public class Shop_Money
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Total_Discounts_Set
        {
            public Shop_Money1 shop_money { get; set; }
            public Presentment_Money1 presentment_money { get; set; }
        }

        public class Shop_Money1
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money1
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Total_Shipping_Price_Set
        {
            public Shop_Money2 shop_money { get; set; }
            public Presentment_Money2 presentment_money { get; set; }
        }

        public class Shop_Money2
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money2
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Subtotal_Price_Set
        {
            public Shop_Money3 shop_money { get; set; }
            public Presentment_Money3 presentment_money { get; set; }
        }

        public class Shop_Money3
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money3
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Total_Price_Set
        {
            public Shop_Money4 shop_money { get; set; }
            public Presentment_Money4 presentment_money { get; set; }
        }

        public class Shop_Money4
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money4
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Total_Tax_Set
        {
            public Shop_Money5 shop_money { get; set; }
            public Presentment_Money5 presentment_money { get; set; }
        }

        public class Shop_Money5
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money5
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Tax_Lines
        {
            public string price { get; set; }
            public float rate { get; set; }
            public string title { get; set; }
            public Price_Set price_set { get; set; }
        }

        public class Price_Set
        {
            public Shop_Money6 shop_money { get; set; }
            public Presentment_Money6 presentment_money { get; set; }
        }

        public class Shop_Money6
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money6
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Line_Items
        {
            public long id { get; set; }
            public long variant_id { get; set; }
            public string title { get; set; }
            public int quantity { get; set; }
            public string sku { get; set; }
            public object variant_title { get; set; }
            public string vendor { get; set; }
            public string fulfillment_service { get; set; }
            public long product_id { get; set; }
            public bool requires_shipping { get; set; }
            public bool taxable { get; set; }
            public bool gift_card { get; set; }
            public string name { get; set; }
            public string variant_inventory_management { get; set; }
            public object[] properties { get; set; }
            public bool product_exists { get; set; }
            public int fulfillable_quantity { get; set; }
            public int grams { get; set; }
            public string price { get; set; }
            public string total_discount { get; set; }
            public object fulfillment_status { get; set; }
            public Price_Set1 price_set { get; set; }
            public Total_Discount_Set total_discount_set { get; set; }
            public object[] discount_allocations { get; set; }
            public string admin_graphql_api_id { get; set; }
            public Tax_Lines1[] tax_lines { get; set; }
        }

        public class Price_Set1
        {
            public Shop_Money7 shop_money { get; set; }
            public Presentment_Money7 presentment_money { get; set; }
        }

        public class Shop_Money7
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money7
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Total_Discount_Set
        {
            public Shop_Money8 shop_money { get; set; }
            public Presentment_Money8 presentment_money { get; set; }
        }

        public class Shop_Money8
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money8
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Tax_Lines1
        {
            public string title { get; set; }
            public string price { get; set; }
            public float rate { get; set; }
            public Price_Set2 price_set { get; set; }
        }

        public class Price_Set2
        {
            public Shop_Money9 shop_money { get; set; }
            public Presentment_Money9 presentment_money { get; set; }
        }

        public class Shop_Money9
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }

        public class Presentment_Money9
        {
            public string amount { get; set; }
            public string currency_code { get; set; }
        }


        public async Task<string> UpdatePrice(string dbPathEra, string kdbrgMO, string log_CUST, string log_ActionCategory, string log_ActionName, ShopifyAPIData iden, string brg_mp, double price)
        {
            string ret = "";
            string[] brg_mp_split = brg_mp.Split(';');

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/products/{3}.json";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store, Convert.ToInt64(brg_mp_split[0]));

            ShopifyUpdateHargaProduct putProdData = new ShopifyUpdateHargaProduct
            {
                id = Convert.ToInt64(brg_mp_split[0]),
                variants = new List<ShopifyUpdateHargaProductVariant>()
            };
            ShopifyUpdateHargaProductVariant variants = new ShopifyUpdateHargaProductVariant
            {
                //id = Convert.ToInt64(brg_mp_split[1]),
                product_id = Convert.ToInt64(brg_mp_split[0]),
                price = Convert.ToString(price)
            };

            putProdData.variants.Add(variants);

            ShopifyUpdateHarga putData = new ShopifyUpdateHarga
            {
                product = putProdData
            };

            string myData = JsonConvert.SerializeObject(putData);

            string responseFromServer = "";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", (iden.API_password));
            var content = new StringContent(myData, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
            HttpResponseMessage clientResponse = await client.PutAsync(vformatUrl, content);

            using (HttpContent responseContent = clientResponse.Content)
            {
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    responseFromServer = await reader.ReadToEndAsync();
                }
            };


            if (responseFromServer != "")
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyUpdateStockResult)) as ShopifyUpdateStockResult;
                    if (!string.IsNullOrWhiteSpace(result.ToString()))
                    {
                        if (result.errors == null)
                        {
                            if (result.product.variants.Length >= 1)
                            {
                                //foreach (var item in result.product.variants)
                                //{
                                //    //if (item.price == Convert.ToString(price))
                                //    //{
                                //        //throw new Exception("Success update stock " + stf02_brg + ": " + Convert.ToString(qty) + " stock");
                                //    }
                                //}
                            }
                            else
                            {
                                throw new Exception("Failed update harga " + Convert.ToString(brg_mp_split[0]) + ": " + Convert.ToString(price));
                            }
                        }
                        else
                        {
                            //if (result.errors.Length > 0)
                            //{
                            throw new Exception("Failed update harga " + Convert.ToString(brg_mp_split[0]) + ":" + Convert.ToString(price));
                            //}
                        }
                    }
                    else
                    {
                        throw new Exception("Failed update harga " + Convert.ToString(brg_mp_split[0]) + ":" + Convert.ToString(price));
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    throw new Exception(msg);
                }
            }
            return ret;
        }

        public async Task<string> GetOrderByStatus(ShopifyAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page)
        {
            //int MOPartnerID = 841371;
            //string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";
            string connID = Guid.NewGuid().ToString();

            long seconds = CurrentTimeSecond();
            long timestamp7Days = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();

            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //string urll = "https://partner.shopeemobile.com/api/v1/orders/get";
            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/orders.json";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(vformatUrl);
            myReq.Method = "GET";
            myReq.Headers.Add("X-Shopify-Access-Token", iden.API_password);
            //myReq.Accept = "application/x-www-form-urlencoded";
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

            if (responseFromServer != null)
            {
                try
                {

                    var listOrder = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyResultOrders)) as ShopifyResultOrders;
                    if (stat == StatusOrder.UNPAID)
                    {
                        string[] ordersn_list = listOrder.orders.Select(p => p.id.ToString()).ToArray();
                        //add by calvin 4 maret 2019, filter
                        var dariTgl = DateTimeOffset.UtcNow.AddDays(-30).DateTime;
                        var SudahAdaDiMO = ErasoftDbContext.SOT01A.Where(p => p.USER_NAME == "Auto Shopify" && p.TGL >= dariTgl).Select(p => p.NO_REFERENSI).ToList();
                        //end add by calvin
                        var filtered = ordersn_list.Where(p => !SudahAdaDiMO.Contains(p));
                        if (filtered.Count() > 0)
                        {
                            await GetOrderDetails(iden, filtered.ToArray(), connID, CUST, NAMA_CUST);
                        }
                        //if (listOrder.)
                        //{
                        //    await GetOrderByStatus(iden, stat, connID, CUST, NAMA_CUST, page + 50);
                        //}
                    }

                }
                catch (Exception ex2)
                {
                }
            }
            return ret;
        }

        public async Task<string> CreateProduct(ShopifyAPIData iden, string brg, string cust)
        {
            string ret = "";
            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == brg.ToUpper()).FirstOrDefault();
            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == cust.ToUpper()).FirstOrDefault();
            if (brgInDb == null || marketplace == null)
                return "invalid passing data";
            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == brg.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
            if (detailBrg == null)
                return "invalid passing data";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                REQUEST_ACTION = "Create Product",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = brg,
                REQUEST_ATTRIBUTE_2 = brgInDb.NAMA,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/products.json";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store);

            ShopifyCreateProductData body = new ShopifyCreateProductData
            {
                title = (brgInDb.NAMA + " " + brgInDb.NAMA2).Trim().Replace("’", "`"),
                body_html = brgInDb.Deskripsi.Replace("’", "`"),
                vendor = brgInDb.KET_SORT2,
                //product_type = detailBrg.CATEGORY_NAME,
                product_type = "Battery",
                tags = "",
                template_suffix = "",
                variants = new List<ShopifyCreateProductDataVariant>(),
                images = new List<ShopifyCreateProductImages>()
            };

            ShopifyCreateProductDataVariant variants = new ShopifyCreateProductDataVariant
            {
                option1 = "First",
                price = detailBrg.HJUAL.ToString(),
                inventory_quantity = Convert.ToInt32(1),
                grams = Convert.ToInt32(brgInDb.BERAT / 1000),
                weight = Convert.ToInt64(brgInDb.BERAT),
                sku = brg
            };

            body.variants.Add(variants);

            ShopifyCreateProduct HttpBody = new ShopifyCreateProduct
            {
                product = body
            };

            HttpBody.product.body_html = new StokControllerJob().RemoveSpecialCharacters(HttpBody.product.body_html);

            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<li>", "- ").Replace("</li>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<ul>", "").Replace("</ul>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");

            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<p>", "\r\n").Replace("</p>", "\r\n");

            HttpBody.product.body_html = System.Text.RegularExpressions.Regex.Replace(HttpBody.product.body_html, "<.*?>", String.Empty);

            var qty_stock = new StokControllerJob(DatabasePathErasoft, username).GetQOHSTF08A(brg, "ALL");
            if (qty_stock > 0)
            {
                HttpBody.product.variants[0].inventory_quantity = Convert.ToInt32(20);
            }

            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
                HttpBody.product.images.Add(new ShopifyCreateProductImages { position = "1", src = brgInDb.LINK_GAMBAR_1, alt = brgInDb.NAMA.ToString() });
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
                HttpBody.product.images.Add(new ShopifyCreateProductImages { position = "2", src = brgInDb.LINK_GAMBAR_2, alt = brgInDb.NAMA.ToString() });
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
                HttpBody.product.images.Add(new ShopifyCreateProductImages { position = "3", src = brgInDb.LINK_GAMBAR_3, alt = brgInDb.NAMA.ToString() });
            if (brgInDb.TYPE == "4")
            {
                var ListVariant = ErasoftDbContext.STF02.Where(p => p.PART == brg).ToList();
                var ListSettingVariasi = ErasoftDbContext.STF02I.Where(p => p.BRG == brg).ToList();
                //add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
                List<string> byteGambarUploaded = new List<string>();
                //end add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
                foreach (var item in ListVariant)
                {
                    List<string> Duplikat = HttpBody.product.variants.Select(p => p.sku.ToString()).ToList();
                    //add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
                    if (!byteGambarUploaded.Contains(item.Sort5))
                    {
                        byteGambarUploaded.Add(item.Sort5);
                        if (!string.IsNullOrEmpty(item.LINK_GAMBAR_1))
                            HttpBody.product.images.Add(new ShopifyCreateProductImages { src = item.LINK_GAMBAR_1 });
                    }
                }
            }

            string myData = JsonConvert.SerializeObject(HttpBody);

            string responseFromServer = "";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", (iden.API_password));
            var content = new StringContent(myData, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");

            try
            {
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
                using (HttpResponseMessage clientResponse = await client.PostAsync(vformatUrl, content))
                {
                    using (HttpContent responseContent = clientResponse.Content)
                    {
                        using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                        {
                            responseFromServer = await reader.ReadToEndAsync();
                            Console.WriteLine(responseFromServer);
                        };
                    };
                };

            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                try
                {
                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyCreateResult)) as ShopifyCreateResult;
                    if (resServer != null)
                    {
                        if (resServer.errors == null)
                        {
                            var item = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == brg.ToUpper() && b.IDMARKET == marketplace.RecNum).SingleOrDefault();
                            if (item != null)
                            {
                                item.BRG_MP = Convert.ToString(resServer.product.id) + ";0";
                                ErasoftDbContext.SaveChanges();

                                if (brgInDb.TYPE == "4")
                                {
                                    //await InitTierVariation(iden, brgInDb, resServer.product.id, marketplace);
                                }

                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            }
                            else
                            {
                                currentLog.REQUEST_EXCEPTION = "item not found";
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                            }
                        }
                        else
                        {
                            //currentLog.REQUEST_RESULT = resServer.msg;
                            //currentLog.REQUEST_EXCEPTION = resServer.error + ";" + resServer.msg;
                            //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        }
                    }

                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }



        [Route("shwwwwp/code")]
        [HttpGet]
        public ActionResult ShopifyCode(string user, string shop_id)
        {
            var param = user.Split(new string[] { "_param_" }, StringSplitOptions.None);
            if (param.Count() == 2)
            {
                ShopifyController.ShopifyAPIData dataSp = new ShopifyController.ShopifyAPIData()
                {
                    no_cust = shop_id,
                    DatabasePathErasoft = param[0],
                    email = param[1],
                };
                Task.Run(() => GetTokenShopify(dataSp, true)).Wait();
            }
            return View("ShopifyAuth");
        }




        protected async Task<BindingBase> proses_Item_detail(ShopifyGetItemDetailResult detailBrg, string categoryCode, string categoryName, string cust, int IdMarket, string barang_id, string barang_name, string barang_status, double barang_price, string sellerSku, int typeBrg, string kdBrgInduk, ShopifyAPIData iden, bool insert_1st_img)
        {
            // typeBrg : 0 = barang tanpa varian; 1 = barang induk; 2 = barang varian
            var ret = new BindingBase();
            string brand = "OEM";
            string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, NAMA3, BERAT, PANJANG, LEBAR, TINGGI, CUST, ";
            sSQL += "Deskripsi, IDMARKET, HJUAL, HJUAL_MP, DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, IMAGE4, IMAGE5, KODE_BRG_INDUK, TYPE, AVALUE_45,";
            sSQL += "ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
            sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20, ";
            sSQL += "ACODE_21, ANAME_21, AVALUE_21, ACODE_22, ANAME_22, AVALUE_22, ACODE_23, ANAME_23, AVALUE_23, ACODE_24, ANAME_24, AVALUE_24, ACODE_25, ANAME_25, AVALUE_25, ACODE_26, ANAME_26, AVALUE_26, ACODE_27, ANAME_27, AVALUE_27, ACODE_28, ANAME_28, AVALUE_28, ACODE_29, ANAME_29, AVALUE_29, ACODE_30, ANAME_30, AVALUE_30) VALUES ";

            string namaBrg = barang_name.Replace('\'', '`');
            string nama, nama2, nama3, urlImage, urlImage2, urlImage3, urlImage4, urlImage5;
            urlImage = "";
            urlImage2 = "";
            urlImage3 = "";
            urlImage4 = "";
            urlImage5 = "";
            sellerSku = sellerSku.ToString().Replace("\'", "\'\'");

            //change by calvin 16 september 2019
            //if (namaBrg.Length > 30)
            //{
            //    nama = namaBrg.Substring(0, 30);
            //    //change by calvin 15 januari 2019
            //    //if (namaBrg.Length > 60)
            //    //{
            //    //    nama2 = namaBrg.Substring(30, 30);
            //    //    nama3 = (namaBrg.Length > 90) ? namaBrg.Substring(60, 30) : namaBrg.Substring(60);
            //    //}
            //    if (namaBrg.Length > 285)
            //    {
            //        nama2 = namaBrg.Substring(30, 255);
            //        nama3 = "";
            //    }
            //    //end change by calvin 15 januari 2019
            //    else
            //    {
            //        nama2 = namaBrg.Substring(30);
            //        nama3 = "";
            //    }
            //}
            //else
            //{
            //    nama = namaBrg;
            //    nama2 = "";
            //    nama3 = "";
            //}
            var splitItemName = new StokControllerJob().SplitItemName(namaBrg);
            nama = splitItemName[0];
            nama2 = splitItemName[1];
            nama3 = "";
            //end change by calvin 16 september 2019

            if (detailBrg.product.images.Count() > 0)
            {
                if (insert_1st_img)
                {
                    urlImage = detailBrg.product.images[0].src;
                    //change 21/8/2019, barang varian ambil 1 gambar saja
                    if (typeBrg != 2)
                    //if (typeBrg == 0)
                    {
                        if (detailBrg.product.images.Count() >= 2)
                        {
                            urlImage2 = detailBrg.product.images[1].src;
                            if (detailBrg.product.images.Count() >= 3)
                            {
                                urlImage3 = detailBrg.product.images[2].src;
                                //add 16/9/19, 5 gambar
                                if (detailBrg.product.images.Count() >= 4)
                                {
                                    urlImage4 = detailBrg.product.images[3].src;
                                    if (detailBrg.product.images.Count() >= 5)
                                    {
                                        urlImage5 = detailBrg.product.images[4].src;
                                    }
                                }
                                //end add 16/9/19, 5 gambar
                            }
                        }
                    }
                    //end change 21/8/2019, barang varian ambil 1 gambar saja
                }
            }
            sSQL += "('" + barang_id + "' , '" + sellerSku + "' , '" + nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' , '" + nama3.Replace('\'', '`') + "' ,";
            sSQL += detailBrg.product.variants[0].weight * 1000 + ", '', '','', '";
            //change by nurul 22/1/2020, tambah paragraf
            //sSQL += cust + "' , '" + detailBrg.item.description.Replace('\'', '`') + "' , " + IdMarket + " , " + barang_price + " , " + barang_price;
            sSQL += cust + "' , '" + "<p>" + detailBrg.product.body_html.Replace('\'', '`').Replace("\n", "</p><p>") + "</p>" + "' , " + IdMarket + " , " + barang_price + " , " + barang_price;
            //end change by nurul 22/1/2020, tambah paragraf
            sSQL += " , " + (barang_status.Contains("NORMAL") ? "1" : "0") + " , '" + categoryCode + "' , '" + categoryName + "' , '" + "REPLACE_MEREK" + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "' , '" + urlImage4 + "' , '" + urlImage5 + "'";
            //add kode brg induk dan type brg
            sSQL += ", '" + (typeBrg == 2 ? kdBrgInduk : "") + "' , '" + (typeBrg == 1 ? "4" : "3") + "'";
            //end add kode brg induk dan type brg
            sSQL += ",'" + (namaBrg.Length > 250 ? namaBrg.Substring(0, 250) : namaBrg) + "'"; //request by Calvin, 19 maret 2019

            //var attributeShopify = MoDbContext.AttributeShopify.Where(a => a.CATEGORY_CODE == categoryCode).FirstOrDefault();
            //var GetAttributeShopify = await GetAttributeToList(iden, categoryCode, categoryName);
            //var attributeShopify = GetAttributeShopify.attributes.FirstOrDefault();

            //#region set attribute
            //if (attributeShopify != null)
            //{
            //    string attrVal = "";
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_1))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_1.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`').Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_1 + "' , '" + attributeShopify.ANAME_1.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_2))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_2.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_2 + "' , '" + attributeShopify.ANAME_2.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }

            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_3))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_3.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_3 + "' , '" + attributeShopify.ANAME_3.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_4))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_4.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_4 + "' , '" + attributeShopify.ANAME_4.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }

            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_5))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_5.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_5 + "' , '" + attributeShopify.ANAME_5.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_6))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_6.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_6 + "' , '" + attributeShopify.ANAME_6.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_7))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_7.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_7 + "' , '" + attributeShopify.ANAME_7.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_8))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_8.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_8 + "' , '" + attributeShopify.ANAME_8.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_9))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_9.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_9 + "' , '" + attributeShopify.ANAME_9.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_10))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_10.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_10 + "' , '" + attributeShopify.ANAME_10.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_11))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_11.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_11 + "' , '" + attributeShopify.ANAME_11.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_12))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_12.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_12 + "' , '" + attributeShopify.ANAME_12.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_13))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_13.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_13 + "' , '" + attributeShopify.ANAME_13.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_14))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_14.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_14 + "' , '" + attributeShopify.ANAME_14.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_15))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_15.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_15 + "' , '" + attributeShopify.ANAME_15.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_16))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_16.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_16 + "' , '" + attributeShopify.ANAME_16.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_17))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_17.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_17 + "' , '" + attributeShopify.ANAME_17.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_18))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_18.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_18 + "' , '" + attributeShopify.ANAME_18.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_19))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_19.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_19 + "' , '" + attributeShopify.ANAME_19.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_20))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_20.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_20 + "' , '" + attributeShopify.ANAME_20.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_21))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_21.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_21 + "' , '" + attributeShopify.ANAME_21.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_22))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_22.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_22 + "' , '" + attributeShopify.ANAME_22.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_23))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_23.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_23 + "' , '" + attributeShopify.ANAME_23.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_24))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_24.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_24 + "' , '" + attributeShopify.ANAME_24.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_25))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_25.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_25 + "' , '" + attributeShopify.ANAME_25.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_26))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_26.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_26 + "' , '" + attributeShopify.ANAME_26.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_27))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_27.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_27 + "' , '" + attributeShopify.ANAME_27.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_28))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_28.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_28 + "' , '" + attributeShopify.ANAME_28.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_29))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_29.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_29 + "' , '" + attributeShopify.ANAME_29.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', ''";
            //    }
            //    if (!string.IsNullOrEmpty(attributeShopify.ACODE_30))
            //    {
            //        foreach (var property in detailBrg.item.attributes)
            //        {
            //            string tempCode = property.attribute_id.ToString();
            //            if (attributeShopify.ACODE_30.ToUpper().Equals(tempCode.ToString()))
            //            {
            //                if (!string.IsNullOrEmpty(attrVal))
            //                    attrVal += ";";
            //                attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
            //                if (property.attribute_name.ToUpper() == "MEREK")
            //                {
            //                    brand = property.attribute_value;
            //                }
            //            }
            //        }
            //        sSQL += ", '" + attributeShopify.ACODE_30 + "' , '" + attributeShopify.ANAME_30.Replace("\'", "\'\'") + "' , '" + attrVal + "')";
            //        attrVal = "";
            //    }
            //    else
            //    {
            //        sSQL += ", '', '', '')";
            //    }
            //}
            //else
            //{
            //    //attribute not found
            //    sSQL += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
            //    sSQL += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
            //    sSQL += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '')";
            //}

            //attribute not found
            sSQL += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
            sSQL += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', ''";
            sSQL += " , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '' , '', '', '')";

            //#endregion
            sSQL = sSQL.Replace("REPLACE_MEREK", brand.Replace('\'', '`'));
            var retRec = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
            ret.status = retRec;
            return ret;
        }

        public async Task<string> GetCategory(ShopifyAPIData iden)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //ganti
            string urll = "https://partner.shopeemobile.com/api/v1/item/categories/get";

            //ganti
            ShopifyGetCategoryData HttpBody = new ShopifyGetCategoryData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                language = "id"
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
            catch (Exception ex)
            {
            }

            if (responseFromServer != null)
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetCategoryResult)) as ShopifyGetCategoryResult;
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
                            //oCommand.CommandText = "DELETE FROM [CATEGORY_BLIBLI] WHERE ARF01_SORT1_CUST='" + data.no_cust + "'";
                            //oCommand.ExecuteNonQuery();
                            //oCommand.Transaction = oTransaction;
                            oCommand.CommandType = CommandType.Text;
                            oCommand.CommandText = "INSERT INTO [CATEGORY_SHOPEE] ([CATEGORY_CODE], [CATEGORY_NAME], [PARENT_CODE], [IS_LAST_NODE], [MASTER_CATEGORY_CODE]) VALUES (@CATEGORY_CODE, @CATEGORY_NAME, @PARENT_CODE, @IS_LAST_NODE, @MASTER_CATEGORY_CODE)";
                            //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));
                            oCommand.Parameters.Add(new SqlParameter("@PARENT_CODE", SqlDbType.NVarChar, 50));
                            oCommand.Parameters.Add(new SqlParameter("@IS_LAST_NODE", SqlDbType.NVarChar, 1));
                            oCommand.Parameters.Add(new SqlParameter("@MASTER_CATEGORY_CODE", SqlDbType.NVarChar, 50));

                            try
                            {
                                foreach (var item in result.categories.Where(P => P.parent_id == 0)) //foreach parent level top
                                {
                                    oCommand.Parameters[0].Value = item.category_id;
                                    oCommand.Parameters[1].Value = item.category_name;
                                    oCommand.Parameters[2].Value = "";
                                    oCommand.Parameters[3].Value = item.has_children ? "0" : "1";
                                    oCommand.Parameters[4].Value = "";
                                    if (oCommand.ExecuteNonQuery() > 0)
                                    {
                                        if (item.has_children)
                                        {
                                            RecursiveInsertCategory(oCommand, result.categories, item.category_id, item.category_id);
                                        }
                                    }
                                    else
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
                    }
                }
                catch (Exception ex2)
                {

                }
            }
            return ret;
        }
        protected void RecursiveInsertCategory(SqlCommand oCommand, ShopifyGetCategoryCategory[] categories, long parent, long master_category_code)
        {
            foreach (var child in categories.Where(p => p.parent_id == parent))
            {

                var cekChildren = categories.Where(p => p.parent_id == child.category_id).FirstOrDefault();
                oCommand.Parameters[0].Value = child.category_id;
                oCommand.Parameters[1].Value = child.category_name;
                oCommand.Parameters[2].Value = parent;
                //change by calvin 11 maret 2019, API returnnya item.has children false, tetapi nyatanya ada childrennya, kasus 15949
                //oCommand.Parameters[3].Value = child.has_children ? "0" : "1";
                oCommand.Parameters[3].Value = cekChildren == null ? "1" : "0";
                //end change by calvin 11 maret 2019
                oCommand.Parameters[4].Value = master_category_code;

                if (oCommand.ExecuteNonQuery() > 0)
                {
                    if (cekChildren != null)
                    {
                        RecursiveInsertCategory(oCommand, categories, child.category_id, master_category_code);
                    }
                }
                else
                {

                }
            }
        }

        public async Task<ATTRIBUTE_SHOPEE_AND_OPT> GetAttributeToList(ShopifyAPIData iden, string category_code, string category_name)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            ATTRIBUTE_SHOPEE_AND_OPT ret = new ATTRIBUTE_SHOPEE_AND_OPT();

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //ganti
            string urll = "https://partner.shopeemobile.com/api/v1/item/attributes/get";

            //ganti
            ShopifyGetAttributeData HttpBody = new ShopifyGetAttributeData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                language = "id",
                category_id = Convert.ToInt32(category_code)
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
            catch (Exception ex)
            {
            }

            if (responseFromServer != null)
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetAttributeResult)) as ShopifyGetAttributeResult;
#if AWS
                    string con = "Data Source=localhost;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#elif Debug_AWS
                    string con = "Data Source=13.250.232.74;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#else
                    string con = "Data Source=13.251.222.53;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#endif
                    string a = "";
                    int i = 0;
                    ATTRIBUTE_SHOPEE returnData = new ATTRIBUTE_SHOPEE();
                    foreach (var attribs in result.attributes)
                    {
                        a = Convert.ToString(i + 1);
                        returnData.CATEGORY_CODE = category_code;
                        returnData.CATEGORY_NAME = category_name;

                        returnData["ACODE_" + a] = Convert.ToString(attribs.attribute_id);
                        returnData["ATYPE_" + a] = attribs.options.Count() > 0 ? "PREDEFINED_ATTRIBUTE" : "DESCRIPTIVE_ATTRIBUTE";
                        returnData["ANAME_" + a] = attribs.attribute_name;
                        returnData["AOPTIONS_" + a] = attribs.options.Count() > 0 ? "1" : "0";
                        returnData["AMANDATORY_" + a] = attribs.is_mandatory ? "1" : "0";

                        if (attribs.options.Count() > 0)
                        {
                            var optList = attribs.options.ToList();
                            var listOpt = optList.Select(x => new ATTRIBUTE_OPT_SHOPEE(attribs.attribute_id.ToString(), x)).ToList();
                            ret.attribute_opts.AddRange(listOpt);
                        }
                        i = i + 1;
                    }
                    ret.attributes.Add(returnData);

                }
                catch (Exception ex2)
                {

                }
            }

            return ret;
        }

        public async Task<ATTRIBUTE_SHOPEE_AND_OPT> GetAttributeToList(ShopifyAPIData iden, CATEGORY_SHOPEE category)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            ATTRIBUTE_SHOPEE_AND_OPT ret = new ATTRIBUTE_SHOPEE_AND_OPT();

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //ganti
            string urll = "https://partner.shopeemobile.com/api/v1/item/attributes/get";

            //ganti
            ShopifyGetAttributeData HttpBody = new ShopifyGetAttributeData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                language = "id",
                category_id = Convert.ToInt32(category.CATEGORY_CODE)
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
            catch (Exception ex)
            {
            }

            if (responseFromServer != null)
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetAttributeResult)) as ShopifyGetAttributeResult;
#if AWS
                        string con = "Data Source=localhost;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#elif Debug_AWS
                    string con = "Data Source=13.250.232.74;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#else
                    string con = "Data Source=13.251.222.53;Initial Catalog=MO;Persist Security Info=True;User ID=sa;Password=admin123^";
#endif
                    string a = "";
                    int i = 0;
                    ATTRIBUTE_SHOPEE returnData = new ATTRIBUTE_SHOPEE();
                    foreach (var attribs in result.attributes)
                    {
                        a = Convert.ToString(i + 1);
                        returnData.CATEGORY_CODE = category.CATEGORY_CODE;
                        returnData.CATEGORY_NAME = category.CATEGORY_NAME;

                        returnData["ACODE_" + a] = Convert.ToString(attribs.attribute_id);
                        returnData["ATYPE_" + a] = attribs.options.Count() > 0 ? "PREDEFINED_ATTRIBUTE" : "DESCRIPTIVE_ATTRIBUTE";
                        returnData["ANAME_" + a] = attribs.attribute_name;
                        returnData["AOPTIONS_" + a] = attribs.options.Count() > 0 ? "1" : "0";
                        returnData["AMANDATORY_" + a] = attribs.is_mandatory ? "1" : "0";

                        if (attribs.options.Count() > 0)
                        {
                            var optList = attribs.options.ToList();
                            var listOpt = optList.Select(x => new ATTRIBUTE_OPT_SHOPEE(attribs.attribute_id.ToString(), x)).ToList();
                            ret.attribute_opts.AddRange(listOpt);
                        }
                        i = i + 1;
                    }
                    ret.attributes.Add(returnData);

                }
                catch (Exception ex2)
                {

                }
            }

            return ret;
        }

        //add by fauzi 21 Februari 2020
        public async Task<string> GetTokenShopify(ShopifyAPIData dataAPI, bool bForceRefresh)
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

            if (TokenExpired || bForceRefresh)
            {
                int MOPartnerID = 841371;
                string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";

                long seconds = CurrentTimeSecond();
                DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

                MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                {
                    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                    REQUEST_ACTION = "Refresh Token Shopify", //ganti
                    REQUEST_DATETIME = milisBack,
                    REQUEST_ATTRIBUTE_1 = dataAPI.no_cust,
                    REQUEST_STATUS = "Pending",
                };

                //ganti
                string urll = "https://partner.shopeemobile.com/api/v1/shop/get_partner_shop";

                //ganti
                //ShopifyGetTokenShop HttpBody = new ShopifyGetTokenShop
                //{
                //    partner_id = MOPartnerID,
                //    shopid = Convert.ToInt32(dataAPI.no_cust),
                //    timestamp = seconds
                //};

                //string myData = JsonConvert.SerializeObject(HttpBody);

                //string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "POST";
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
                    currentLog.REQUEST_RESULT = "Process Get API Token Shopify";
                    manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, dataAPI, currentLog);
                }
                catch (Exception ex)
                {
                    currentLog.REQUEST_EXCEPTION = ex.Message.ToString();
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, dataAPI, currentLog);
                }

                if (responseFromServer != null)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetTokenShopResult)) as ShopifyGetTokenShopResult;
                        if (result.error == null && !string.IsNullOrWhiteSpace(result.ToString()))
                        {
                            if (result.authed_shops.Length > 0)
                            {
                                foreach (var item in result.authed_shops)
                                {
                                    if (item.shopid.ToString() == dataAPI.no_cust.ToString())
                                    {
                                        var dateExpired = DateTimeOffset.FromUnixTimeSeconds(item.expire_time).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                                        DatabaseSQL EDB = new DatabaseSQL(dataAPI.DatabasePathErasoft);
                                        var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '1', Sort1_Cust = '" + dataAPI.no_cust + "', TGL_EXPIRED = '" + dateExpired + "' WHERE CUST = '" + dataAPI.no_cust + "'");
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
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(result.msg.ToString()))
                            {
                                currentLog.REQUEST_EXCEPTION = result.msg.ToString();
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

        public async Task<string> GetAttribute(ShopifyAPIData iden)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";
            //var categories = MoDbContext.CategoryShopify.Where(p => p.IS_LAST_NODE.Equals("1")).ToList();
            var categories = "a:a:c";
            foreach (var category in categories)
            {
                long seconds = CurrentTimeSecond();
                DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

                //ganti
                string urll = "https://partner.shopeemobile.com/api/v1/item/attributes/get";

                //ganti
                ShopifyGetAttributeData HttpBody = new ShopifyGetAttributeData
                {
                    partner_id = MOPartnerID,
                    shopid = Convert.ToInt32(iden.no_cust),
                    timestamp = seconds,
                    language = "id",
                    //category_id = Convert.ToInt32(category.CATEGORY_CODE)
                    category_id = Convert.ToInt32(category)
                };

                string myData = JsonConvert.SerializeObject(HttpBody);

                string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "POST";
                myReq.Headers.Add("Authorization", signature);
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
                catch (Exception ex)
                {
                }

                if (responseFromServer != null)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetAttributeResult)) as ShopifyGetAttributeResult;
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
                                //var AttributeInDb = MoDbContext.AttributeShopify.Where(p => p.CATEGORY_CODE.ToUpper().Equals(category.CATEGORY_CODE)).ToList();
                                var AttributeInDb = 0;
                                //cek jika belum ada di database, insert
                                var cari = AttributeInDb;
                                //if (cari.Count() == 0)
                                if (cari == 0)
                                {
                                    oCommand.CommandType = CommandType.Text;
                                    oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
                                    oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));

                                    string sSQL = "INSERT INTO [ATTRIBUTE_SHOPEE] ([CATEGORY_CODE], [CATEGORY_NAME],";
                                    string sSQLValue = ") VALUES (@CATEGORY_CODE, @CATEGORY_NAME,";
                                    string a = "";
                                    int i = 0;
                                    foreach (var attribs in result.attributes)
                                    {
                                        a = Convert.ToString(i + 1);
                                        sSQL += "[ACODE_" + a + "],[ATYPE_" + a + "],[ANAME_" + a + "],[AOPTIONS_" + a + "],[AMANDATORY_" + a + "],";
                                        sSQLValue += "@ACODE_" + a + ",@ATYPE_" + a + ",@ANAME_" + a + ",@AOPTIONS_" + a + ",@AMANDATORY_" + a + ",";
                                        oCommand.Parameters.Add(new SqlParameter("@ACODE_" + a, SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@ATYPE_" + a, SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@ANAME_" + a, SqlDbType.NVarChar, 250));
                                        oCommand.Parameters.Add(new SqlParameter("@AOPTIONS_" + a, SqlDbType.NVarChar, 1));
                                        oCommand.Parameters.Add(new SqlParameter("@AMANDATORY_" + a, SqlDbType.NVarChar, 1));

                                        a = Convert.ToString(i * 5 + 2);
                                        oCommand.Parameters[(i * 5) + 2].Value = "";
                                        oCommand.Parameters[(i * 5) + 3].Value = "";
                                        oCommand.Parameters[(i * 5) + 4].Value = "";
                                        oCommand.Parameters[(i * 5) + 5].Value = "";
                                        oCommand.Parameters[(i * 5) + 6].Value = "";

                                        oCommand.Parameters[(i * 5) + 2].Value = attribs.attribute_id;
                                        oCommand.Parameters[(i * 5) + 3].Value = attribs.options.Count() > 0 ? "PREDEFINED_ATTRIBUTE" : "DESCRIPTIVE_ATTRIBUTE";
                                        oCommand.Parameters[(i * 5) + 4].Value = attribs.attribute_name;
                                        oCommand.Parameters[(i * 5) + 5].Value = attribs.options.Count() > 0 ? "1" : "0";
                                        oCommand.Parameters[(i * 5) + 6].Value = attribs.is_mandatory ? "1" : "0";

                                        if (attribs.options.Count() > 0)
                                        {
                                            //var AttributeOptInDb = MoDbContext.AttributeOptShopify.AsNoTracking().ToList();
                                            var AttributeOptInDb = "";
                                            foreach (var option in attribs.options)
                                            {
                                                //var cariOpt = AttributeOptInDb.Where(p => p.ACODE == Convert.ToString(attribs.attribute_id) && p.OPTION_VALUE == option);
                                                var cariOpt = 0;
                                                //if (cariOpt.Count() == 0)
                                                if (cariOpt == 0)
                                                {
                                                    using (SqlCommand oCommand2 = oConnection.CreateCommand())
                                                    {
                                                        oCommand2.CommandType = CommandType.Text;
                                                        oCommand2.CommandText = "INSERT INTO ATTRIBUTE_OPT_SHOPEE ([ACODE], [OPTION_VALUE]) VALUES (@ACODE, @OPTION_VALUE)";
                                                        oCommand2.Parameters.Add(new SqlParameter("@ACODE", SqlDbType.NVarChar, 50));
                                                        oCommand2.Parameters.Add(new SqlParameter("@OPTION_VALUE", SqlDbType.NVarChar, 250));
                                                        oCommand2.Parameters[0].Value = attribs.attribute_id;
                                                        oCommand2.Parameters[1].Value = option;
                                                        oCommand2.ExecuteNonQuery();
                                                    }
                                                }
                                            }
                                        }
                                        i = i + 1;
                                    }
                                    sSQL = sSQL.Substring(0, sSQL.Length - 1) + sSQLValue.Substring(0, sSQLValue.Length - 1) + ")";
                                    oCommand.CommandText = sSQL;
                                    //oCommand.Parameters[0].Value = category.CATEGORY_CODE;
                                    oCommand.Parameters[0].Value = "";
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
            return ret;
        }

        public async Task<string> GetOrderDetails(ShopifyAPIData iden, string[] ordersn_list, string connID, string CUST, string NAMA_CUST)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                REQUEST_ACTION = "Get Order List", //ganti
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.no_cust,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/orders/detail";

            //GetOrderDetailsData HttpBody = new GetOrderDetailsData
            //{
            //    partner_id = MOPartnerID,
            //    shopid = Convert.ToInt32(iden.no_cust),
            //    timestamp = seconds,
            //    ordersn_list = ordersn_list
            //    //ordersn_list = ordersn_list_test.ToArray()
            //};

            //string myData = JsonConvert.SerializeObject(HttpBody);

            //string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
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
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetOrderDetailsResult)) as ShopifyGetOrderDetailsResult;
                    var connIdARF01C = Guid.NewGuid().ToString();
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    TEMP_SHOPEE_ORDERS batchinsert = new TEMP_SHOPEE_ORDERS();
                    List<TEMP_SHOPEE_ORDERS_ITEM> batchinsertItem = new List<TEMP_SHOPEE_ORDERS_ITEM>();
                    string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                    insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                    insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                    var kabKot = "3174";
                    var prov = "31";

                    foreach (var order in result.orders)
                    {
                        //insertPembeli += "('" + order.recipient_address.name + "','" + order.recipient_address.full_address + "','" + order.recipient_address.phone + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
                        //insertPembeli += "1, 'IDR', '01', '" + order.recipient_address.full_address + "', 0, 0, 0, 0, '1', 0, 0, ";
                        //insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.recipient_address.zipcode + "', '', '" + kabKot + "', '" + prov + "', '', '','" + connIdARF01C + "'),";

                        string nama = order.recipient_address.name.Length > 30 ? order.recipient_address.name.Substring(0, 30) : order.recipient_address.name;

                        insertPembeli += string.Format("('{0}','{1}','{2}','{3}',0,0,'0','01',1, 'IDR', '01', '{4}', 0, 0, 0, 0, '1', 0, 0,'FP', '{5}', '{6}', '{7}', '', '{8}', '{9}', '', '','{10}'),",
                            ((nama ?? "").Replace("'", "`")),
                            ((order.recipient_address.full_address ?? "").Replace("'", "`")),
                            ((order.recipient_address.phone ?? "").Replace("'", "`")),
                            (NAMA_CUST.Replace(',', '.')),
                            ((order.recipient_address.full_address ?? "").Replace("'", "`")),
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            (username),
                            ((order.recipient_address.zipcode ?? "").Replace("'", "`")),
                            kabKot,
                            prov,
                            connIdARF01C
                            );
                    }
                    insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
                    EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);

                    foreach (var order in result.orders)
                    {
                        try
                        {
                            ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_SHOPEE_ORDERS");
                            ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_SHOPEE_ORDERS_ITEM");
                            batchinsertItem = new List<TEMP_SHOPEE_ORDERS_ITEM>();
                            TEMP_SHOPEE_ORDERS newOrder = new TEMP_SHOPEE_ORDERS()
                            {
                                actual_shipping_cost = order.actual_shipping_cost,
                                buyer_username = order.buyer_username,
                                cod = order.cod,
                                country = order.country,
                                create_time = DateTimeOffset.FromUnixTimeSeconds(order.create_time).UtcDateTime,
                                currency = order.currency,
                                days_to_ship = order.days_to_ship,
                                dropshipper = order.dropshipper,
                                escrow_amount = order.escrow_amount,
                                estimated_shipping_fee = order.estimated_shipping_fee,
                                goods_to_declare = order.goods_to_declare,
                                message_to_seller = order.message_to_seller,
                                note = order.note,
                                note_update_time = DateTimeOffset.FromUnixTimeSeconds(order.note_update_time).UtcDateTime,
                                ordersn = order.ordersn,
                                order_status = order.order_status,
                                payment_method = order.payment_method,
                                pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime,
                                Recipient_Address_country = order.recipient_address.country,
                                Recipient_Address_state = order.recipient_address.state,
                                Recipient_Address_city = order.recipient_address.city,
                                Recipient_Address_town = order.recipient_address.town,
                                Recipient_Address_district = order.recipient_address.district,
                                Recipient_Address_full_address = order.recipient_address.full_address,
                                Recipient_Address_name = order.recipient_address.name,
                                Recipient_Address_phone = order.recipient_address.phone,
                                Recipient_Address_zipcode = order.recipient_address.zipcode,
                                service_code = order.service_code,
                                shipping_carrier = order.shipping_carrier,
                                total_amount = order.total_amount,
                                tracking_no = order.tracking_no,
                                update_time = DateTimeOffset.FromUnixTimeSeconds(order.update_time).UtcDateTime,
                                CONN_ID = connID,
                                CUST = CUST,
                                NAMA_CUST = NAMA_CUST
                            };
                            foreach (var item in order.items)
                            {
                                TEMP_SHOPEE_ORDERS_ITEM newOrderItem = new TEMP_SHOPEE_ORDERS_ITEM()
                                {
                                    ordersn = order.ordersn,
                                    is_wholesale = item.is_wholesale,
                                    item_id = item.item_id,
                                    item_name = item.item_name,
                                    item_sku = item.item_sku,
                                    variation_discounted_price = item.variation_discounted_price,
                                    variation_id = item.variation_id,
                                    variation_name = item.variation_name,
                                    variation_original_price = item.variation_original_price,
                                    variation_quantity_purchased = item.variation_quantity_purchased,
                                    variation_sku = item.variation_sku,
                                    weight = item.weight,
                                    pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime,
                                    CONN_ID = connID,
                                    CUST = CUST,
                                    NAMA_CUST = NAMA_CUST
                                };
                                batchinsertItem.Add(newOrderItem);
                            }
                            batchinsert = (newOrder);

                            ErasoftDbContext.TEMP_SHOPEE_ORDERS.Add(batchinsert);
                            ErasoftDbContext.TEMP_SHOPEE_ORDERS_ITEM.AddRange(batchinsertItem);
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
                                CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 1;
                                CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                                EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
                            }
                        }
                        catch (Exception ex3)
                        {

                        }
                    }
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }
        public async Task<ShopifyGetParameterForInitLogisticResult> GetParameterForInitLogistic(ShopifyAPIData iden, string ordersn)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            ShopifyGetParameterForInitLogisticResult ret = null;
            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/init_parameter/get";

            ShopifyGetParameterForInitLogisticData HttpBody = new ShopifyGetParameterForInitLogisticData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                ordersn = ordersn
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
            catch (Exception ex)
            {

            }

            if (responseFromServer != "")
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetParameterForInitLogisticResult)) as ShopifyGetParameterForInitLogisticResult;
                    ret = result;
                }
                catch (Exception ex2)
                {

                }
            }
            return ret;
        }
        public async Task<ShopifyGetParameterForInitLogisticResult> GetLogisticInfo(ShopifyAPIData iden, string ordersn)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            ShopifyGetParameterForInitLogisticResult ret = null;
            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/init_info/get";

            ShopifyGetParameterForInitLogisticData HttpBody = new ShopifyGetParameterForInitLogisticData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                ordersn = ordersn
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
            catch (Exception ex)
            {

            }

            if (responseFromServer != null)
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetParameterForInitLogisticResult)) as ShopifyGetParameterForInitLogisticResult;
                    ret = result;
                }
                catch (Exception ex2)
                {

                }
            }
            return ret;
        }
        public async Task<string> InitLogisticDropOff(ShopifyAPIData iden, string ordersn, ShopifyInitLogisticDropOffDetailData data, int recnum, string dBranch, string dSender, string dTrackNo)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                REQUEST_ACTION = "Update No Resi",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = ordersn,
                REQUEST_ATTRIBUTE_2 = dTrackNo,
                REQUEST_ATTRIBUTE_3 = "dropoff",
                REQUEST_ATTRIBUTE_4 = dSender + "[;]" + dBranch,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/init";
            ShopifyInitLogisticDropOffData HttpBody = new ShopifyInitLogisticDropOffData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                ordersn = ordersn,
                dropoff = data
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);
            string responseFromServer = "";

            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Add("Authorization", signature);
            //var content = new FormUrlEncodedContent(ToKeyValue(HttpBody));

            //HttpResponseMessage clientResponse = await client.PostAsync(
            //    urll, content);

            //using (HttpContent responseContent = clientResponse.Content)
            //{
            //    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
            //    {
            //        responseFromServer = await reader.ReadToEndAsync();
            //    }
            //};

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
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
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyInitLogisticResult)) as ShopifyInitLogisticResult;
                    if ((result.error == null ? "" : result.error) == "")
                    {
                        if (!string.IsNullOrWhiteSpace(result.tracking_no) || !string.IsNullOrWhiteSpace(result.tracking_number))
                        {
                            var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.RecNum == recnum);
                            if (pesananInDb != null)
                            {
                                if (dTrackNo == "")
                                {
                                    dTrackNo = ((result.tracking_no == null ? "" : result.tracking_no) == "") ? (result.tracking_number) : result.tracking_no;
                                }
                                string nilaiTRACKING_SHIPMENT = "D[;]" + dBranch + "[;]" + dSender + "[;]" + dTrackNo;
                                pesananInDb.TRACKING_SHIPMENT = nilaiTRACKING_SHIPMENT;
                                ErasoftDbContext.SaveChanges();
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            }
                        }
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = result.msg;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                    }
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }
        public async Task<string> InitLogisticNonIntegrated(ShopifyAPIData iden, string ordersn, ShopifyInitLogisticNotIntegratedDetailData data, int recnum, string savedParam)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                REQUEST_ACTION = "Update No Resi",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = ordersn,
                REQUEST_ATTRIBUTE_2 = "",
                REQUEST_ATTRIBUTE_3 = "NonIntegrated",
                REQUEST_ATTRIBUTE_4 = savedParam,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/init";
            ShopifyInitLogisticNonIntegratedData HttpBody = new ShopifyInitLogisticNonIntegratedData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                ordersn = ordersn,
                non_integrated = data
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyInitLogisticResult)) as ShopifyInitLogisticResult;
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }
        public async Task<string> InitLogisticPickup(ShopifyAPIData iden, string ordersn, ShopifyInitLogisticPickupDetailData data, int recnum, string savedParam)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                REQUEST_ACTION = "Update No Resi",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = ordersn,
                REQUEST_ATTRIBUTE_2 = "",
                REQUEST_ATTRIBUTE_3 = "Pickup",
                REQUEST_ATTRIBUTE_4 = savedParam,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/init";
            ShopifyInitLogisticPickupData HttpBody = new ShopifyInitLogisticPickupData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                ordersn = ordersn,
                pickup = data
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyInitLogisticResult)) as ShopifyInitLogisticResult;
                    if (result.error == null)
                    {
                        if (!string.IsNullOrWhiteSpace(result.tracking_no) || !string.IsNullOrWhiteSpace(result.tracking_number))
                        {
                            var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.RecNum == recnum);
                            if (pesananInDb != null)
                            {
                                pesananInDb.TRACKING_SHIPMENT = savedParam;
                                ErasoftDbContext.SaveChanges();
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            }
                        }
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = result.msg;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                    }
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }

        public async Task<string> UpdateStock(ShopifyAPIData iden, string brg_mp, int qty)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                REQUEST_ACTION = "Update QOH",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.no_cust,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/items/update_stock";
            string[] brg_mp_split = brg_mp.Split(';');
            ShopifyUpdateStockData HttpBody = new ShopifyUpdateStockData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                item_id = Convert.ToInt64(brg_mp_split[0]),
                stock = qty
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                try
                {
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }
        public async Task<string> UpdateVariationStock(ShopifyAPIData iden, string brg_mp, int qty)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                REQUEST_ACTION = "Update QOH",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.no_cust,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/items/update_variation_stock";
            string[] brg_mp_split = brg_mp.Split(';');
            ShopifyUpdateVariationStockData HttpBody = new ShopifyUpdateVariationStockData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                item_id = Convert.ToInt64(brg_mp_split[0]),
                variation_id = Convert.ToInt64(brg_mp_split[1]),
                stock = qty
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                try
                {
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }

        public async Task<ShopifyGetLogisticsResult> GetLogistics(ShopifyAPIData iden)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            ShopifyGetLogisticsResult ret = null;

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/channel/get";

            ShopifyGetLogisticsData HttpBody = new ShopifyGetLogisticsData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
            catch (Exception ex)
            {
            }

            if (responseFromServer != null)
            {
                try
                {
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetLogisticsResult)) as ShopifyGetLogisticsResult;
                    ret = result;
                }
                catch (Exception ex2)
                {
                }
            }
            return ret;
        }
        public async Task<string> AcceptBuyerCancellation(ShopifyAPIData iden, string ordersn)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                REQUEST_ACTION = "Accept Buyer Cancel", //ganti
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.no_cust,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/orders/buyer_cancellation/accept";

            ShopifyAcceptBuyerCancelOrderData HttpBody = new ShopifyAcceptBuyerCancelOrderData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                ordersn = ordersn
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                try
                {
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);



                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }
        public async Task<string> CancelOrder(ShopifyAPIData iden, string ordersn)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                REQUEST_ACTION = "Cancel Order",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.no_cust,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/orders/cancel";

            ShopifyCancelOrderData HttpBody = new ShopifyCancelOrderData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                ordersn = ordersn,
                cancel_reason = "CUSTOMER_REQUEST"
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                try
                {
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyCancelOrderResult)) as ShopifyCancelOrderResult;
                    if (result.error != null)
                    {
                        if (result.error != "")
                        {
                            await AcceptBuyerCancellation(iden, ordersn);
                        }
                    }
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }

        public async Task<string> GetVariation(ShopifyAPIData iden, STF02 brgInDb, long item_id, ARF01 marketplace, Dictionary<string, int> mapSTF02HRecnum_IndexVariasi, List<ShopifyVariation> MOVariation, List<ShopifyTierVariation> tier_variation)
        {
            var MOVariationNew = MOVariation.ToList();

            string ret = "";
            string brg = brgInDb.BRG;

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/item/tier_var/get";

            ShopifyGetVariation HttpBody = new ShopifyGetVariation
            {
                partner_id = MOPartnerID,
                item_id = item_id,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }


            if (responseFromServer != null)
            {
                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(GetVariationResult)) as GetVariationResult;
                var new_tier_variation = new List<ShopifyUpdateVariation>();

                foreach (var variasi in resServer.variations)
                {
                    string key_map_tier_index_recnum = "";
                    foreach (var indexes in variasi.tier_index)
                    {
                        key_map_tier_index_recnum = key_map_tier_index_recnum + Convert.ToString(indexes) + ";";
                    }
                    int recnum_stf02h_var = mapSTF02HRecnum_IndexVariasi.Where(p => p.Key == key_map_tier_index_recnum).Select(p => p.Value).SingleOrDefault();

                    //var var_item = ErasoftDbContext.STF02H.Where(b => b.RecNum == recnum_stf02h_var).SingleOrDefault();
                    //var_item.BRG_MP = Convert.ToString(resServer.item_id) + ";" + Convert.ToString(variasi.variation_id);
                    //ErasoftDbContext.SaveChanges();
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(resServer.item_id) + ";" + Convert.ToString(variasi.variation_id) + "' WHERE RECNUM = '" + Convert.ToString(recnum_stf02h_var) + "' AND ISNULL(BRG_MP,'') = '' ");
                    //tes remark by calvin 16 mei
                    mapSTF02HRecnum_IndexVariasi.Remove(key_map_tier_index_recnum);
                    foreach (var item in MOVariation)
                    {
                        var isiTier_Index = "";
                        foreach (var indexes in item.tier_index)
                        {
                            isiTier_Index = isiTier_Index + Convert.ToString(indexes) + ";";
                        }
                        if (isiTier_Index == key_map_tier_index_recnum)
                        {
                            MOVariationNew.Remove(item);
                            new_tier_variation.Add(new ShopifyUpdateVariation()
                            {
                                price = item.price,
                                stock = item.stock,
                                tier_index = item.tier_index,
                                variation_sku = item.variation_sku,
                                variation_id = variasi.variation_id
                            });
                        }
                    }
                    //end tes remark by calvin 16 mei
                }
                if (MOVariationNew.Count() > 0)
                {
                    //foreach (var variasi in mapSTF02HRecnum_IndexVariasi)
                    //{
                    //    await AddVariation(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariation, MOVariationNew);
                    //}
                    //await UpdateTierVariationIndex(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariationNew, tier_variation, new_tier_variation);
                    await UpdateTierVariationList(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariationNew, tier_variation, new_tier_variation, MOVariation);
                }
            }

            return ret;
        }
        public async Task<string> UpdateTierVariationList(ShopifyAPIData iden, STF02 brgInDb, long item_id, ARF01 marketplace, Dictionary<string, int> mapSTF02HRecnum_IndexVariasi, List<ShopifyVariation> MOVariationNew, List<ShopifyTierVariation> tier_variation, List<ShopifyUpdateVariation> new_tier_variation, List<ShopifyVariation> MOVariation)
        {
            //Use this api to update tier-variation list or upload variation image of a tier-variation item
            string ret = "";
            string brg = brgInDb.BRG;

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/item/tier_var/update_list";

            ShopifyUpdateTierVariationList HttpBody = new ShopifyUpdateTierVariationList
            {
                partner_id = MOPartnerID,
                item_id = item_id,
                shopid = Convert.ToInt32(iden.no_cust),
                tier_variation = tier_variation.ToArray(),
                timestamp = seconds,
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }


            if (responseFromServer != null)
            {
                //AddTierVariation
                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyUpdateTierVariationResult)) as ShopifyUpdateTierVariationResult;
                if (resServer.item_id == item_id)
                {
                    //foreach (var variasi in mapSTF02HRecnum_IndexVariasi)
                    //{
                    //    await AddVariation(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariation, MOVariationNew);
                    //}
                    await AddTierVariation(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariation, MOVariationNew);
                }
            }

            return ret;
        }
        public async Task<string> AddTierVariation(ShopifyAPIData iden, STF02 brgInDb, long item_id, ARF01 marketplace, Dictionary<string, int> mapSTF02HRecnum_IndexVariasi, List<ShopifyVariation> MOVariation, List<ShopifyVariation> MOVariationNew)
        {
            string ret = "";
            string brg = brgInDb.BRG;

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/item/tier_var/add";
            ShopifyAddTierVariation HttpBody = new ShopifyAddTierVariation
            {
                partner_id = MOPartnerID,
                item_id = item_id,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                variation = MOVariationNew.ToArray()
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }


            if (responseFromServer != null)
            {
                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(InitTierVariationResult)) as InitTierVariationResult;
                if (resServer.variation_id_list != null)
                {
                    await GetVariation(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariation, null);

                }
            }

            return ret;
        }
        public async Task<string> UpdateTierVariationIndex(ShopifyAPIData iden, STF02 brgInDb, long item_id, ARF01 marketplace, Dictionary<string, int> mapSTF02HRecnum_IndexVariasi, List<ShopifyVariation> MOVariationNew, List<ShopifyTierVariation> tier_variation, List<ShopifyUpdateVariation> new_tier_variation)
        {
            List<object> variation = new List<object>();
            string ret = "";
            string brg = brgInDb.BRG;

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/item/tier_var/update";
            foreach (var item in new_tier_variation)
            {
                variation.Add(new ShopifyUpdateVariation()
                {
                    price = item.price,
                    stock = item.stock,
                    tier_index = item.tier_index,
                    variation_sku = item.variation_sku,
                    variation_id = item.variation_id
                });
            }
            //foreach (var item in MOVariationNew)
            //{
            //    variation.Add(new ShopifyVariation()
            //    {
            //        price = item.price,
            //        stock = item.stock,
            //        tier_index = item.tier_index,
            //        variation_sku = item.variation_sku,

            //    });
            //}

            ShopifyUpdateTierVariationIndex HttpBody = new ShopifyUpdateTierVariationIndex
            {
                partner_id = MOPartnerID,
                item_id = item_id,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                tier_variation = tier_variation.ToArray(),
                variation = variation.ToArray()
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }
            return ret;
        }
        public async Task<string> InitTierVariation(ShopifyAPIData iden, STF02 brgInDb, long item_id, ARF01 marketplace)
        {
            string ret = "";
            string brg = brgInDb.BRG;

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = seconds.ToString(),
            //    REQUEST_ACTION = "Create Product",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = brg,
            //    REQUEST_ATTRIBUTE_2 = brgInDb.NAMA,
            //    REQUEST_STATUS = "Pending",
            //};

            string urll = "https://partner.shopeemobile.com/api/v1/item/tier_var/init";

            ShopifyInitTierVariation HttpBody = new ShopifyInitTierVariation
            {
                partner_id = MOPartnerID,
                item_id = item_id,
                shopid = Convert.ToInt32(iden.no_cust),
                tier_variation = new List<ShopifyTierVariation>().ToArray(),
                variation = new List<ShopifyVariation>().ToArray(),
                timestamp = seconds
            };
            List<ShopifyTierVariation> tier_variation = new List<ShopifyTierVariation>();
            List<ShopifyVariation> variation = new List<ShopifyVariation>();
            Dictionary<string, int> mapIndexVariasi1 = new Dictionary<string, int>();
            Dictionary<string, int> mapIndexVariasi2 = new Dictionary<string, int>();
            Dictionary<string, int> mapSTF02HRecnum_IndexVariasi = new Dictionary<string, int>();

            if (brgInDb.TYPE == "4")//Barang Induk ( memiliki Variant )
            {
                var ListVariant = ErasoftDbContext.STF02.Where(p => p.PART == brg).ToList();
                var ListSettingVariasi = ErasoftDbContext.STF02I.Where(p => p.BRG == brg).ToList();
                var ListKodeBrgVariant = ListVariant.Select(p => p.BRG).ToList();
                var ListStf02hVariasi = ErasoftDbContext.STF02H.Where(p => ListKodeBrgVariant.Contains(p.BRG) && p.IDMARKET == marketplace.RecNum).ToList();

                int index_var_1 = 0;
                int index_var_2 = 0;
                ShopifyTierVariation tier1 = new ShopifyTierVariation();
                ShopifyTierVariation tier2 = new ShopifyTierVariation();
                List<string> tier1_options = new List<string>();
                List<string> tier2_options = new List<string>();
                foreach (var item in ListVariant.OrderBy(p => p.ID))
                {
                    var stf02h = ListStf02hVariasi.Where(p => p.BRG.ToUpper() == item.BRG.ToUpper() && p.IDMARKET == marketplace.RecNum).FirstOrDefault();
                    if (stf02h != null)
                    {
                        string namaVariasiLV1 = "";
                        if ((item.Sort8 == null ? "" : item.Sort8) != "")
                        {
                            var getNamaVar = ListSettingVariasi.Where(p => p.LEVEL_VAR == 1 && p.MARKET == "SHOPEE" && p.KODE_VAR == item.Sort8).FirstOrDefault();
                            if (getNamaVar != null)
                            {
                                namaVariasiLV1 = getNamaVar.MP_JUDUL_VAR;
                            }
                        }

                        string namaVariasiLV2 = "";
                        if ((item.Sort9 == null ? "" : item.Sort9) != "")
                        {
                            var getNamaVar = ListSettingVariasi.Where(p => p.LEVEL_VAR == 2 && p.MARKET == "SHOPEE" && p.KODE_VAR == item.Sort9).FirstOrDefault();
                            if (getNamaVar != null)
                            {
                                namaVariasiLV2 = getNamaVar.MP_JUDUL_VAR;
                            }
                        }

                        List<string> DuplikatNamaVariasi = tier_variation.Select(p => p.name).ToList();
                        if (!DuplikatNamaVariasi.Contains(namaVariasiLV1))
                        {
                            if (namaVariasiLV1 != "")
                            {
                                tier1.name = namaVariasiLV1;
                            }
                        }
                        if (!DuplikatNamaVariasi.Contains(namaVariasiLV2))
                        {
                            if (namaVariasiLV2 != "")
                            {
                                tier2.name = namaVariasiLV2;
                            }
                        }

                        string nama_var1 = "";
                        if ((item.Sort8 == null ? "" : item.Sort8) != "")
                        {
                            var getNamaVar = ListSettingVariasi.Where(p => p.LEVEL_VAR == 1 && p.MARKET == "SHOPEE" && p.KODE_VAR == item.Sort8).FirstOrDefault();
                            if (getNamaVar != null)
                            {
                                nama_var1 = getNamaVar.MP_VALUE_VAR;
                                if (!mapIndexVariasi1.ContainsKey(nama_var1))
                                {
                                    mapIndexVariasi1.Add(nama_var1, index_var_1);
                                    tier1_options.Add(nama_var1);
                                    index_var_1++;
                                }
                            }
                        }
                        string nama_var2 = "";
                        if ((item.Sort9 == null ? "" : item.Sort9) != "")
                        {
                            var getNamaVar = ListSettingVariasi.Where(p => p.LEVEL_VAR == 2 && p.MARKET == "SHOPEE" && p.KODE_VAR == item.Sort9).FirstOrDefault();
                            if (getNamaVar != null)
                            {
                                nama_var2 = getNamaVar.MP_VALUE_VAR;
                                if (!mapIndexVariasi2.ContainsKey(nama_var2))
                                {
                                    mapIndexVariasi2.Add(nama_var2, index_var_2);
                                    tier2_options.Add(nama_var2);
                                    index_var_2++;
                                }
                            }
                        }
                        int getIndexVar1 = 0;
                        int getIndexVar2 = 0;
                        List<int> ListTierIndex = new List<int>();
                        if (mapIndexVariasi1.ContainsKey(nama_var1))
                        {
                            getIndexVar1 = mapIndexVariasi1[nama_var1];
                            ListTierIndex.Add(getIndexVar1);
                        }
                        if (mapIndexVariasi2.ContainsKey(nama_var2))
                        {
                            getIndexVar2 = mapIndexVariasi2[nama_var2];
                            ListTierIndex.Add(getIndexVar2);
                        }

                        ShopifyVariation adaVariant = new ShopifyVariation()
                        {
                            tier_index = ListTierIndex.ToArray(),
                            price = (float)stf02h.HJUAL,
                            stock = 1,//create product min stock = 1
                            variation_sku = item.BRG
                        };

                        //add by calvin 1 mei 2019
                        var qty_stock = new StokControllerJob(DatabasePathErasoft, username).GetQOHSTF08A(item.BRG, "ALL");
                        if (qty_stock > 0)
                        {
                            adaVariant.stock = Convert.ToInt32(qty_stock);
                        }
                        //end add by calvin 1 mei 2019

                        string key_map_tier_index_recnum = "";
                        foreach (var indexes in ListTierIndex)
                        {
                            key_map_tier_index_recnum = key_map_tier_index_recnum + Convert.ToString(indexes) + ";";
                        }
                        mapSTF02HRecnum_IndexVariasi.Add(key_map_tier_index_recnum, stf02h.RecNum.Value);
                        variation.Add(adaVariant);
                    }
                }
                if (!string.IsNullOrWhiteSpace(tier1.name))
                {
                    tier1.options = tier1_options.ToArray();
                    tier_variation.Add(tier1);
                }
                if (!string.IsNullOrWhiteSpace(tier2.name))
                {
                    tier2.options = tier2_options.ToArray();
                    tier_variation.Add(tier2);
                }
            }
            HttpBody.variation = variation.ToArray();
            HttpBody.tier_variation = tier_variation.ToArray();

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }


            if (responseFromServer != "")
            {
                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(InitTierVariationResult)) as InitTierVariationResult;
                if (resServer.variation_id_list != null)
                {
                    if (resServer.variation_id_list.Count() > 0)
                    {
                        foreach (var variasi in resServer.variation_id_list)
                        {
                            string key_map_tier_index_recnum = "";
                            foreach (var indexes in variasi.tier_index)
                            {
                                key_map_tier_index_recnum = key_map_tier_index_recnum + Convert.ToString(indexes) + ";";
                            }
                            int recnum_stf02h_var = mapSTF02HRecnum_IndexVariasi.Where(p => p.Key == key_map_tier_index_recnum).Select(p => p.Value).SingleOrDefault();
                            //var var_item = ErasoftDbContext.STF02H.Where(b => b.RecNum == recnum_stf02h_var).SingleOrDefault();
                            //var_item.BRG_MP = Convert.ToString(resServer.item_id) + ";" + Convert.ToString(variasi.variation_id);
                            //ErasoftDbContext.SaveChanges();
                            var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(resServer.item_id) + ";" + Convert.ToString(variasi.variation_id) + "' WHERE RECNUM = '" + Convert.ToString(recnum_stf02h_var) + "'");
                        }
                    }
                }
                else
                {
                    await GetVariation(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, variation, tier_variation);
                }
            }

            return ret;
        }
        public async Task<string> UpdateProduct(ShopifyAPIData iden, string brg, string cust)
        {
            string ret = "";

            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToString() == brg.ToString()).FirstOrDefault();
            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == cust.ToUpper()).FirstOrDefault();
            if (brgInDb == null || marketplace == null)
                return "invalid passing data";
            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == brg.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
            if (detailBrg == null)
                return "invalid passing data";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);
            string[] brg_mp = detailBrg.BRG_MP.Split(';');

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                REQUEST_ACTION = "Update Product",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = brg,
                REQUEST_ATTRIBUTE_2 = brgInDb.NAMA,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://{0}:{1}@{2}.myshopify.com/admin/products/{3}.json";
            var vformatUrl = String.Format(urll, iden.API_key, iden.API_password, iden.account_store, Convert.ToInt64(brg_mp[0]));

            ShopifyUpdateProductData body = new ShopifyUpdateProductData
            {
                id = Convert.ToInt64(brg_mp[0]),
                title = (brgInDb.NAMA + " " + brgInDb.NAMA2).Trim().Replace("’", "`"),
                body_html = brgInDb.Deskripsi.Replace("’", "`"),
                vendor = brgInDb.KET_SORT2,
                product_type = detailBrg.CATEGORY_NAME,
                tags = "",
                template_suffix = "",
                variants = new List<ShopifyUpdateProductDataVariant>(),
                images = new List<ShopifyUpdateProductImages>()
            };

            ShopifyUpdateProductDataVariant variants = new ShopifyUpdateProductDataVariant
            {
                option1 = "NEW",
                price = detailBrg.HJUAL.ToString(),
                inventory_quantity = Convert.ToInt32(1),
                grams = Convert.ToInt32(brgInDb.BERAT / 1000),
                weight = Convert.ToInt64(brgInDb.BERAT),
                sku = brg
            };

            body.variants.Add(variants);

            ShopifyUpdateProduct HttpBody = new ShopifyUpdateProduct
            {
                product = body
            };

            HttpBody.product.body_html = new StokControllerJob().RemoveSpecialCharacters(HttpBody.product.body_html);

            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<li>", "- ").Replace("</li>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<ul>", "").Replace("</ul>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");

            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
            HttpBody.product.body_html = HttpBody.product.body_html.Replace("<p>", "\r\n").Replace("</p>", "\r\n");

            HttpBody.product.body_html = System.Text.RegularExpressions.Regex.Replace(HttpBody.product.body_html, "<.*?>", String.Empty);

            var qty_stock = new StokControllerJob(DatabasePathErasoft, username).GetQOHSTF08A(brg, "ALL");
            if (qty_stock > 0)
            {
                HttpBody.product.variants[0].inventory_quantity = Convert.ToInt32(qty_stock);
            }

            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
                HttpBody.product.images.Add(new ShopifyUpdateProductImages { position = "1", src = brgInDb.LINK_GAMBAR_1, alt = brgInDb.NAMA.ToString() });
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
                HttpBody.product.images.Add(new ShopifyUpdateProductImages { position = "2", src = brgInDb.LINK_GAMBAR_2, alt = brgInDb.NAMA.ToString() });
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
                HttpBody.product.images.Add(new ShopifyUpdateProductImages { position = "3", src = brgInDb.LINK_GAMBAR_3, alt = brgInDb.NAMA.ToString() });
            if (brgInDb.TYPE == "4")
            {
                var ListVariant = ErasoftDbContext.STF02.Where(p => p.PART == brg).ToList();
                var ListSettingVariasi = ErasoftDbContext.STF02I.Where(p => p.BRG == brg).ToList();
                //add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
                List<string> byteGambarUploaded = new List<string>();
                //end add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
                foreach (var item in ListVariant)
                {
                    List<string> Duplikat = HttpBody.product.variants.Select(p => p.sku.ToString()).ToList();
                    //add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
                    if (!byteGambarUploaded.Contains(item.Sort5))
                    {
                        byteGambarUploaded.Add(item.Sort5);
                        if (!string.IsNullOrEmpty(item.LINK_GAMBAR_1))
                            HttpBody.product.images.Add(new ShopifyUpdateProductImages { src = item.LINK_GAMBAR_1 });
                    }
                }
            }

            string myData = JsonConvert.SerializeObject(HttpBody);

            string responseFromServer = "";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", (iden.API_password));
            var content = new StringContent(myData, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
            HttpResponseMessage clientResponse = await client.PutAsync(vformatUrl, content);

            try
            {
                using (HttpContent responseContent = clientResponse.Content)
                {
                    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                    {
                        responseFromServer = await reader.ReadToEndAsync();
                        manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
                    }
                };
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }


            //try
            //{
            //    string sSQL = "SELECT * FROM (";
            //    for (int i = 1; i <= 30; i++)
            //    {
            //        sSQL += "SELECT A.ACODE_" + i.ToString() + " AS CATEGORY_CODE,A.ANAME_" + i.ToString() + " AS CATEGORY_NAME,B.ATYPE_" + i.ToString() + " AS CATEGORY_TYPE,A.AVALUE_" + i.ToString() + " AS VALUE FROM STF02H A INNER JOIN MO.DBO.ATTRIBUTE_SHOPEE B ON A.CATEGORY_CODE = B.CATEGORY_CODE WHERE A.BRG='" + brg + "' AND A.IDMARKET = '" + marketplace.RecNum + "' " + System.Environment.NewLine;
            //        if (i < 30)
            //        {
            //            sSQL += "UNION ALL " + System.Environment.NewLine;
            //        }
            //    }

            //    DataSet dsFeature = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' ");

            //    for (int i = 0; i < dsFeature.Tables[0].Rows.Count; i++)
            //    {
            //        HttpBody.attributes.Add(new ShopifyAttributeClass
            //        {
            //            attributes_id = Convert.ToInt64(dsFeature.Tables[0].Rows[i]["CATEGORY_CODE"]),
            //            value = Convert.ToString(dsFeature.Tables[0].Rows[i]["VALUE"]).Trim()
            //        });
            //    }

            //}
            //catch (Exception ex)
            //{

            //}


            if (responseFromServer != "")
            {
                try
                {
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    //var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyCreateProdResponse)) as ShopifyCreateProdResponse;
                    //if (resServer != null)
                    //{
                    //    if (string.IsNullOrEmpty(resServer.error))
                    //    {
                    //        var item = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == brg.ToUpper() && b.IDMARKET == marketplace.RecNum).SingleOrDefault();
                    //        if (item != null)
                    //        {
                    //            item.BRG_MP = resServer.item_id.ToString() + ";0";
                    //            ErasoftDbContext.SaveChanges();
                    //            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    //        }
                    //        else
                    //        {
                    //            currentLog.REQUEST_EXCEPTION = "item not found";
                    //            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        currentLog.REQUEST_EXCEPTION = resServer.error + ";" + resServer.msg;
                    //        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                    //    }
                    //}

                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }



        public async Task<string> UpdateVariationPrice(ShopifyAPIData iden, string brg_mp, float price)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                REQUEST_ACTION = "Update Price",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.no_cust,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/items/update_variation_price";

            string[] brg_mp_split = brg_mp.Split(';');
            ShopifyUpdateVariantionPriceData HttpBody = new ShopifyUpdateVariantionPriceData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                item_id = Convert.ToInt64(brg_mp_split[0]),
                variation_id = Convert.ToInt64(brg_mp_split[1]),
                price = price
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != "")
            {
                try
                {
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }




        public async Task<string> UpdateImage(ShopifyAPIData iden, string brg, string brg_mp)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == brg.ToUpper()).FirstOrDefault();
            if (brgInDb == null)
                return "invalid passing data";
            //var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == brg.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
            //if (detailBrg == null)
            //    return "invalid passing data";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                REQUEST_ACTION = "Update Product Image",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.no_cust,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/item/img/update";

            List<string> imagess = new List<string>();

            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
                imagess.Add(brgInDb.LINK_GAMBAR_1);
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
                imagess.Add(brgInDb.LINK_GAMBAR_2);
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
                imagess.Add(brgInDb.LINK_GAMBAR_3);

            string[] brg_mp_split = brg_mp.Split(';');
            ShopifyUpdateImageData HttpBody = new ShopifyUpdateImageData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                item_id = Convert.ToInt64(brg_mp_split[0]),
                images = imagess.ToArray()
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                try
                {
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }

        public async Task<string> AddDiscount(ShopifyAPIData iden, int recNumPromosi)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";
            double promoPrice;
            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.no_cust).FirstOrDefault();
            var varPromo = ErasoftDbContext.PROMOSI.Where(p => p.RecNum == recNumPromosi).FirstOrDefault();
            var varPromoDetail = ErasoftDbContext.DETAILPROMOSI.Where(p => p.RecNumPromosi == recNumPromosi).ToList();
            long starttime = ((DateTimeOffset)varPromo.TGL_MULAI).ToUnixTimeSeconds();
            long endtime = ((DateTimeOffset)varPromo.TGL_AKHIR).ToUnixTimeSeconds();

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                REQUEST_ACTION = "Add Discount",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.no_cust,
                REQUEST_ATTRIBUTE_2 = recNumPromosi.ToString(),
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/discount/add";

            ShopifyAddDiscountData HttpBody = new ShopifyAddDiscountData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                discount_name = varPromo.NAMA_PROMOSI,
                start_time = starttime,
                end_time = endtime,
                items = new List<ShopifyAddDiscountDataItems>()
            };
            try
            {
                foreach (var promoDetail in varPromoDetail)
                {
                    var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG.Equals(promoDetail.KODE_BRG) && t.IDMARKET == arf01.RecNum).FirstOrDefault();
                    if (brgInDB != null)
                    {
                        promoPrice = promoDetail.HARGA_PROMOSI;
                        if (promoPrice == 0)
                        {
                            promoPrice = brgInDB.HJUAL - (brgInDB.HJUAL * promoDetail.PERSEN_PROMOSI / 100);
                        }
                        string[] brg_mp = new string[1];
                        if (!string.IsNullOrEmpty(brgInDB.BRG_MP))
                            brg_mp = brgInDB.BRG_MP.Split(';');
                        if (brg_mp.Count() == 2)
                        {
                            if (brg_mp[1] == "0")
                            {
                                ShopifyAddDiscountDataItems item = new ShopifyAddDiscountDataItems()
                                {
                                    item_id = Convert.ToInt64(brg_mp[0]),
                                    item_promotion_price = (float)promoPrice,
                                    purchase_limit = (UInt32)(promoDetail.MAX_QTY == 0 ? 2 : promoDetail.MAX_QTY),
                                    variations = new List<ShopifyAddDiscountDataItemsVariation>()
                                };
                                //item.variations.Add(new ShopifyAddDiscountDataItemsVariation()
                                //{
                                //    variation_id = 0,
                                //    variation_promotion_price = (float)0.1
                                //});
                                HttpBody.items.Add(item);
                            }
                            else /*if (brg_mp[1] == "0")*/
                            {
                                ShopifyAddDiscountDataItems item = new ShopifyAddDiscountDataItems()
                                {
                                    item_id = Convert.ToInt64(brg_mp[0]),
                                    item_promotion_price = (float)promoPrice,
                                    //purchase_limit = 10,
                                    purchase_limit = (UInt32)(promoDetail.MAX_QTY == 0 ? 2 : promoDetail.MAX_QTY),
                                    variations = new List<ShopifyAddDiscountDataItemsVariation>()
                                };
                                item.variations.Add(new ShopifyAddDiscountDataItemsVariation()
                                {
                                    variation_id = Convert.ToInt64(brg_mp[1]),
                                    //variation_promotion_price = (float)promoDetail.HARGA_PROMOSI
                                    variation_promotion_price = (float)promoPrice,
                                });
                                HttpBody.items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                return "";
            }


            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                return "";
            }

            if (responseFromServer != null)
            {
                try
                {
                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyCreatePromoRes)) as ShopifyCreatePromoRes;
                    if (resServer != null)
                    {
                        if (string.IsNullOrEmpty(resServer.error))
                        {
                            EDB.ExecuteSQL("CString", CommandType.Text, "UPDATE PROMOSIS SET MP_PROMO_ID = '" + resServer.discount_id + "' WHERE RECNUM = " + recNumPromosi);
                            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                        }
                        else
                        {
                            currentLog.REQUEST_RESULT = resServer.error + " " + resServer.msg;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        }
                    }
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }

        public async Task<string> AddDiscountItem(ShopifyAPIData iden, long discount_id, DetailPromosi detilPromosi)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";
            double promoPrice;
            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.no_cust).FirstOrDefault();

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Add Discount Item",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.no_cust,
                REQUEST_ATTRIBUTE_2 = detilPromosi.KODE_BRG,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/discount/items/add";

            ShopifyAddDiscountItemData HttpBody = new ShopifyAddDiscountItemData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                discount_id = discount_id,
                items = new List<ShopifyAddDiscountDataItems>()
            };

            var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG.Equals(detilPromosi.KODE_BRG) && t.IDMARKET == arf01.RecNum).FirstOrDefault();
            if (brgInDB != null)
            {
                promoPrice = detilPromosi.HARGA_PROMOSI;
                if (promoPrice == 0)
                {
                    promoPrice = brgInDB.HJUAL - (brgInDB.HJUAL * detilPromosi.PERSEN_PROMOSI / 100);
                }
                //string[] brg_mp = brgInDB.BRG_MP.Split(';');
                string[] brg_mp = new string[1];
                if (!string.IsNullOrEmpty(brgInDB.BRG_MP))
                    brg_mp = brgInDB.BRG_MP.Split(';');
                if (brg_mp.Count() == 2)
                {
                    if (brg_mp[1] == "0")
                    {
                        ShopifyAddDiscountDataItems item = new ShopifyAddDiscountDataItems()
                        {
                            item_id = Convert.ToInt64(brg_mp[0]),
                            //item_promotion_price = (float)detilPromosi.HARGA_PROMOSI,
                            item_promotion_price = (float)promoPrice,
                            //purchase_limit = 10,
                            purchase_limit = (UInt32)(detilPromosi.MAX_QTY == 0 ? 2 : detilPromosi.MAX_QTY),
                            variations = new List<ShopifyAddDiscountDataItemsVariation>()
                        };
                        //item.variations.Add(new ShopifyAddDiscountDataItemsVariation()
                        //{
                        //    variation_id = 0,
                        //    variation_promotion_price = 0
                        //});
                        HttpBody.items.Add(item);
                    }
                    else/* if (brg_mp[1] == "0")*/
                    {
                        ShopifyAddDiscountDataItems item = new ShopifyAddDiscountDataItems()
                        {
                            item_id = Convert.ToInt64(brg_mp[0]),
                            //item_promotion_price = (float)detilPromosi.HARGA_PROMOSI,
                            item_promotion_price = (float)promoPrice,
                            //purchase_limit = 10,
                            purchase_limit = (UInt32)(detilPromosi.MAX_QTY == 0 ? 2 : detilPromosi.MAX_QTY),
                            variations = new List<ShopifyAddDiscountDataItemsVariation>()
                        };
                        item.variations.Add(new ShopifyAddDiscountDataItemsVariation()
                        {
                            variation_id = Convert.ToInt64(brg_mp[1]),
                            //variation_promotion_price = (float)detilPromosi.HARGA_PROMOSI
                            variation_promotion_price = (float)promoPrice,
                        });
                        HttpBody.items.Add(item);
                    }
                }
            }


            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                try
                {
                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyCreatePromoRes)) as ShopifyCreatePromoRes;
                    if (resServer != null)
                    {
                        if (string.IsNullOrEmpty(resServer.error))
                        {
                            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                        }
                        else
                        {
                            currentLog.REQUEST_RESULT = resServer.error + " " + resServer.msg;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        }
                    }
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }
        public async Task<string> DeleteDiscount(ShopifyAPIData iden, long discount_id)
        {
            //Use this api to delete one discount activity BEFORE it starts.
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.no_cust).FirstOrDefault();

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Delete Discount",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.no_cust,
                REQUEST_ATTRIBUTE_2 = discount_id.ToString(),
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/discount/delete";

            ShopifyDeleteDiscountData HttpBody = new ShopifyDeleteDiscountData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                discount_id = discount_id,
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                try
                {
                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyDeleteDiscountData)) as ShopifyDeleteDiscountData;
                    if (resServer != null)
                    {
                        if (string.IsNullOrEmpty(resServer.error))
                        {
                            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                        }
                        else
                        {
                            currentLog.REQUEST_RESULT = resServer.error + " " + resServer.msg;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        }
                    }
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }
        public async Task<string> DeleteDiscountItem(ShopifyAPIData iden, long discount_id, DetailPromosi detilPromosi)
        {
            //Use this api to delete items of the discount activity
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.no_cust).FirstOrDefault();

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Delete Discount Item",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.no_cust,
                REQUEST_ATTRIBUTE_2 = detilPromosi.KODE_BRG,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/discount/item/delete";

            ShopifyDeleteDiscountItemData HttpBody = new ShopifyDeleteDiscountItemData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                discount_id = discount_id,
            };

            var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG.Equals(detilPromosi.KODE_BRG) && t.IDMARKET == arf01.RecNum).FirstOrDefault();
            string[] brg_mp = new string[1];
            if (!string.IsNullOrEmpty(brgInDB.BRG_MP))
                brg_mp = brgInDB.BRG_MP.Split(';');
            //string[] brg_mp = brgInDB.BRG_MP.Split(';');
            if (brg_mp.Count() == 2)
            {
                //if(brg_mp[1] == "0")
                //{
                HttpBody.item_id = Convert.ToInt64(brg_mp[0]);
                //}
                //else
                //{
                HttpBody.variation_id = Convert.ToInt64(brg_mp[1]);
                //}
            }

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                try
                {
                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyDeleteDiscountItemData)) as ShopifyDeleteDiscountItemData;
                    if (resServer != null)
                    {
                        if (string.IsNullOrEmpty(resServer.error))
                        {
                            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                        }
                        else
                        {
                            currentLog.REQUEST_RESULT = resServer.error + " " + resServer.msg;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        }
                    }
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }

        public async Task<ShopifyGetAddressResult> GetAddress(ShopifyAPIData iden)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            var ret = new ShopifyGetAddressResult();

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/address/get";

            ShopifyGetAddressData HttpBody = new ShopifyGetAddressData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
            catch (Exception ex)
            {
            }

            if (responseFromServer != null)
            {
                try
                {
                    ret = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetAddressResult)) as ShopifyGetAddressResult;

                }
                catch (Exception ex2)
                {
                }
            }
            return ret;
        }
        public async Task<ShopifyGetTimeSlotResult> GetLabel(ShopifyAPIData iden, bool is_batch, List<string> ordersn_list)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            var ret = new ShopifyGetTimeSlotResult();

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/airway_bill/get_mass";

            ShopifyGetAirwayBill HttpBody = new ShopifyGetAirwayBill
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                is_batch = is_batch,
                ordersn_list = ordersn_list
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
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
            //}

            if (responseFromServer != "")
            {

            }
            return ret;
        }
        public async Task<ShopifyGetTimeSlotResult> GetTimeSlot(ShopifyAPIData iden, long address_id, string ordersn)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            var ret = new ShopifyGetTimeSlotResult();

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/timeslot/get";

            ShopifyGetTimeSlotData HttpBody = new ShopifyGetTimeSlotData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                address_id = address_id,
                ordersn = ordersn
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
            catch (Exception ex)
            {
            }

            if (responseFromServer != null)
            {
                try
                {
                    ret = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetTimeSlotResult)) as ShopifyGetTimeSlotResult;
                    if (ret.pickup_time != null)
                    {
                        foreach (var item in ret.pickup_time)
                        {
                            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                            dtDateTime = dtDateTime.AddSeconds(item.date).ToLocalTime();
                            if (!string.IsNullOrWhiteSpace(item.time_text))
                            {
                                item.date_string = item.time_text;
                            }
                            else
                            {
                                item.date_string = dtDateTime.ToString("dd MMMM yyyy HH:mm:ss");
                            }
                        }
                    }
                    else
                    {
                        var err = JsonConvert.DeserializeObject(responseFromServer, typeof(GetPickupTimeSlotError)) as GetPickupTimeSlotError;
                        ShopifyGetTimeSlotResultPickup_Time errItem = new ShopifyGetTimeSlotResultPickup_Time()
                        {
                            pickup_time_id = "-1",
                            date_string = "Order sudah Expired."
                        };
                        ret.pickup_time = new ShopifyGetTimeSlotResultPickup_Time[1];
                        ret.pickup_time[0] = (errItem);
                    }
                }
                catch (Exception ex2)
                {
                }
            }
            return ret;
        }
        public async Task<string> GetBranch(ShopifyAPIData iden, string ordersn)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/branch/get";

            ShopifyGetBranchData HttpBody = new ShopifyGetBranchData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.no_cust),
                timestamp = seconds,
                ordersn = ordersn
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
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
            catch (Exception ex)
            {
            }

            if (responseFromServer != null)
            {
                try
                {
                    //var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetItemListResult)) as ShopifyGetItemListResult;

                }
                catch (Exception ex2)
                {
                }
            }
            return ret;
        }

        public async Task<string> Template(ShopifyAPIData iden)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = seconds.ToString(),
                REQUEST_ACTION = "Get Item List", //ganti
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.no_cust,
                REQUEST_STATUS = "Pending",
            };

            //ganti
            string urll = "https://partner.shopeemobile.com/api/v1/items/get";

            //ganti
            //ShopifyGetItemListData HttpBody = new ShopifyGetItemListData
            //{
            //    partner_id = MOPartnerID,
            //    shopid = Convert.ToInt32(iden.no_cust),
            //    timestamp = seconds,
            //    pagination_offset = 0,
            //    pagination_entries_per_page = 100
            //};

            //string myData = JsonConvert.SerializeObject(HttpBody);

            //string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
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
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (responseFromServer != null)
            {
                try
                {
                    //ganti
                    var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopifyGetItemListResult)) as ShopifyGetItemListResult;
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);

                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }

        [HttpGet]
        public string ShopifyUrl(string cust)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string userId = "";
            if (sessionData?.Account != null)
            {
                userId = sessionData.Account.DatabasePathErasoft;
            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    userId = accFromUser.DatabasePathErasoft;
                }
            }

            string compUrl = shpCallbackUrl + userId + "_param_" + cust;
            string token = CreateTokenAuthenShop(Convert.ToString(MOPartnerID), MOPartnerKey, compUrl);
            string uri = "https://partner.shopeemobile.com/api/v1/shop/auth_partner?id=" + Convert.ToString(MOPartnerID) + "&token=" + token + "&redirect=" + compUrl;
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
        protected void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, ShopifyAPIData iden, API_LOG_MARKETPLACE data)
        {
            try
            {
                switch (action)
                {
                    case api_status.Pending:
                        {
                            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.no_cust).FirstOrDefault();
                            var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                            {
                                CUST = arf01 != null ? arf01.CUST : iden.no_cust,
                                CUST_ATTRIBUTE_1 = iden.no_cust,
                                CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                                CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                                CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                                CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                                MARKETPLACE = "Shopify",
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
        public class ShopifyAPIData
        {
            public string no_cust { get; set; }
            public string account_store { get; set; }
            public string API_key { get; set; }
            public string API_password { get; set; }
            public string DatabasePathErasoft { get; set; }
            public string email { get; set; }
            public int rec_num { get; set; }
        }

        public class ShopifyAPIData_Temp
        {
            public string no_cust { get; set; }
            public string API_client_username { get; set; }
            public string API_client_password { get; set; }
            public string API_secret_key { get; set; }
            public string mta_username_email_merchant { get; set; }
            public string mta_password_password_merchant { get; set; }
            public string token { get; set; }
            public string DatabasePathErasoft { get; set; }
            public string username { get; set; }
            public string merchant_code { get; set; }
            public DateTime? tgl_expired { get; set; }
        }


        public class ShopifyGetShopAccount
        {
            public ShopifyGetShopAccountResultAttribute shop { get; set; }
            public string errors { get; set; }
        }

        public class ShopifyGetShopAccountResultAttribute
        {
            public long id { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            public string domain { get; set; }
            public string province { get; set; }
            public string country { get; set; }
            public string address1 { get; set; }
            public string zip { get; set; }
            public string city { get; set; }
            public object source { get; set; }
            public string phone { get; set; }
            public float latitude { get; set; }
            public float longitude { get; set; }
            public string primary_locale { get; set; }
            public string address2 { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public string country_code { get; set; }
            public string country_name { get; set; }
            public string currency { get; set; }
            public string customer_email { get; set; }
            public string timezone { get; set; }
            public string iana_timezone { get; set; }
            public string shop_owner { get; set; }
            public string money_format { get; set; }
            public string money_with_currency_format { get; set; }
            public string weight_unit { get; set; }
            public string province_code { get; set; }
            public bool taxes_included { get; set; }
            public object tax_shipping { get; set; }
            public bool county_taxes { get; set; }
            public string plan_display_name { get; set; }
            public string plan_name { get; set; }
            public bool has_discounts { get; set; }
            public bool has_gift_cards { get; set; }
            public string myshopify_domain { get; set; }
            public object google_apps_domain { get; set; }
            public object google_apps_login_enabled { get; set; }
            public string money_in_emails_format { get; set; }
            public string money_with_currency_in_emails_format { get; set; }
            public bool eligible_for_payments { get; set; }
            public bool requires_extra_payments_agreement { get; set; }
            public bool password_enabled { get; set; }
            public bool has_storefront { get; set; }
            public bool eligible_for_card_reader_giveaway { get; set; }
            public bool finances { get; set; }
            public long primary_location_id { get; set; }
            public string cookie_consent_level { get; set; }
            public bool force_ssl { get; set; }
            public bool checkout_api_supported { get; set; }
            public bool multi_location_enabled { get; set; }
            public bool setup_required { get; set; }
            public bool pre_launch_enabled { get; set; }
            public string[] enabled_presentment_currencies { get; set; }
        }

        public class ShopifyGetAttributeData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public string language { get; set; }
            public int category_id { get; set; }
        }
        public class ShopifyGetTokenShop
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
        }

        public class ShopifyGetTokenShopResult
        {
            public string error { get; set; }
            public string msg { get; set; }
            public ShopifyGetTokenShopResultAtribute[] authed_shops { get; set; }
            public string request_id { get; set; }
        }

        public class ShopifyGetTokenShopResultAtribute
        {
            public long expire_time { get; set; }
            public string country { get; set; }
            public string[] sip_a_shops { get; set; }
            public int shopid { get; set; }
            public long auth_time { get; set; }
        }


        public class ShopifyGetCategoryData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public string language { get; set; }
        }

        public class ShopifyGetOrderByStatusData
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

        public class ShopifyGetParameterForInitLogisticData
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
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public int pagination_offset { get; set; }
            public int pagination_entries_per_page { get; set; }
        }

        public class ShopifyGetItemListResult
        {
            public ShopifyGetItemListResultProduct[] product_listings { get; set; }
        }

        public class ShopifyGetItemListResultProduct
        {
            public long product_id { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public string body_html { get; set; }
            public string handle { get; set; }
            public string product_type { get; set; }
            public string title { get; set; }
            public string vendor { get; set; }
            public bool available { get; set; }
            public string tags { get; set; }
            public DateTime published_at { get; set; }
            public ShopifyGetItemListResultProductVariant[] variants { get; set; }
            public ShopifyGetItemListResultProductImage[] images { get; set; }
            public ShopifyGetItemListResultProductOption[] options { get; set; }
        }

        public class ShopifyGetItemListResultProductVariant
        {
            public long id { get; set; }
            public string title { get; set; }
            public ShopifyGetItemListResultProductOption_Values[] option_values { get; set; }
            public string price { get; set; }
            public string formatted_price { get; set; }
            public object compare_at_price { get; set; }
            public int grams { get; set; }
            public bool requires_shipping { get; set; }
            public string sku { get; set; }
            public string barcode { get; set; }
            public bool taxable { get; set; }
            public int position { get; set; }
            public bool available { get; set; }
            public string inventory_policy { get; set; }
            public int inventory_quantity { get; set; }
            public string inventory_management { get; set; }
            public string fulfillment_service { get; set; }
            public float weight { get; set; }
            public string weight_unit { get; set; }
            public object image_id { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
        }

        public class ShopifyGetItemListResultProductOption_Values
        {
            public long option_id { get; set; }
            public string name { get; set; }
            public string value { get; set; }
        }

        public class ShopifyGetItemListResultProductImage
        {
            public long id { get; set; }
            public DateTime created_at { get; set; }
            public int position { get; set; }
            public DateTime updated_at { get; set; }
            public long product_id { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class ShopifyGetItemListResultProductOption
        {
            public long id { get; set; }
            public string name { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public string[] values { get; set; }
        }



        public class ShopifyGetItemDetailData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
        }


        public class ShopifyGetItemDetailResult
        {
            public ShopifyGetItemDetailResultProduct product { get; set; }
        }

        public class ShopifyGetItemDetailResultProduct
        {
            public long id { get; set; }
            public string title { get; set; }
            public string body_html { get; set; }
            public string vendor { get; set; }
            public string product_type { get; set; }
            public DateTime created_at { get; set; }
            public string handle { get; set; }
            public DateTime updated_at { get; set; }
            public DateTime published_at { get; set; }
            public string template_suffix { get; set; }
            public string published_scope { get; set; }
            public string tags { get; set; }
            public string admin_graphql_api_id { get; set; }
            public ShopifyGetItemDetailResultProductVariant[] variants { get; set; }
            public ShopifyGetItemDetailResultProductOption[] options { get; set; }
            public ShopifyGetItemDetailResultProductImageMore[] images { get; set; }
            public ShopifyGetItemDetailResultProductImage image { get; set; }
        }

        public class ShopifyGetItemDetailResultProductImage
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyGetItemDetailResultProductVariant
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string sku { get; set; }
            public int position { get; set; }
            public string inventory_policy { get; set; }
            public object compare_at_price { get; set; }
            public string fulfillment_service { get; set; }
            public string inventory_management { get; set; }
            public string option1 { get; set; }
            public object option2 { get; set; }
            public object option3 { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public bool taxable { get; set; }
            public string barcode { get; set; }
            public int grams { get; set; }
            public object image_id { get; set; }
            public float weight { get; set; }
            public string weight_unit { get; set; }
            public long inventory_item_id { get; set; }
            public int inventory_quantity { get; set; }
            public int old_inventory_quantity { get; set; }
            public bool requires_shipping { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyGetItemDetailResultProductOption
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string name { get; set; }
            public int position { get; set; }
            public string[] values { get; set; }
        }

        public class ShopifyGetItemDetailResultProductImageMore
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }


        public class ShopifyGetCategoryResult
        {
            public ShopifyGetCategoryCategory[] categories { get; set; }
            public string request_id { get; set; }
        }

        public class ShopifyGetCategoryCategory
        {
            public long parent_id { get; set; }
            public bool has_children { get; set; }
            public long category_id { get; set; }
            public string category_name { get; set; }
        }

        public class ShopifyGetAttributeResult
        {
            public ShopifyGetAttributeAttribute[] attributes { get; set; }
            public string request_id { get; set; }
        }

        public class ShopifyGetAttributeAttribute
        {
            public string attribute_name { get; set; }
            public string input_type { get; set; }
            public ShopifyGetAttributeValue[] values { get; set; }
            public long attribute_id { get; set; }
            public string attribute_type { get; set; }
            public bool is_mandatory { get; set; }
            public string[] options { get; set; }
        }

        public class ShopifyGetAttributeValue
        {
            public string original_value { get; set; }
            public string translate_value { get; set; }
        }
        //public class ShopifyGetOrderByStatusResult

        public class ShopifyGetOrderByStatusResult
        {
            public string request_id { get; set; }
            public ShopifyGetOrderByStatusResultOrder[] orders { get; set; }
            public bool more { get; set; }
        }

        public class ShopifyGetOrderByStatusResultOrder
        {
            public string ordersn { get; set; }
            public string order_status { get; set; }
            public long update_time { get; set; }
        }

        public class ShopifyGetOrderDetailsResult
        {
            public object[] errors { get; set; }
            public ShopifyGetOrderDetailsResultOrder[] orders { get; set; }
            public string request_id { get; set; }
        }

        public class ShopifyGetOrderDetailsResultOrder
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
            public ShopifyGetOrderDetailsResultRecipient_Address recipient_address { get; set; }
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
            public ShopifyGetOrderDetailsResultItem[] items { get; set; }
            public string ordersn { get; set; }
            public string dropshipper { get; set; }
            public string buyer_username { get; set; }
        }

        public class ShopifyGetOrderDetailsResultRecipient_Address
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

        public class ShopifyGetOrderDetailsResultItem
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
        public class ShopifyGetParameterForInitLogisticResult
        {
            public string[] pickup { get; set; }
            public string[] dropoff { get; set; }
            public string[] non_integrated { get; set; }
            public string request_id { get; set; }
        }

        public class ShopifyInitLogisticDropOffData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
            public object dropoff { get; set; }
        }
        public class ShopifyInitLogisticPickupData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
            public object pickup { get; set; }
        }
        public class ShopifyInitLogisticNonIntegratedData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
            public object non_integrated { get; set; }
        }
        public class ShopifyInitLogisticPickupDetailData
        {
            public long address_id { get; set; }
            public string pickup_time_id { get; set; }
        }
        public class ShopifyInitLogisticDropOffDetailData
        {
            public long branch_id { get; set; }
            public string sender_real_name { get; set; }
            public string tracking_no { get; set; }
        }
        public class ShopifyInitLogisticNotIntegratedDetailData
        {
            public string tracking_no { get; set; }
        }
        public class ShopifyInitLogisticResult
        {
            public string tracking_no { get; set; }
            public string tracking_number { get; set; }
            public string request_id { get; set; }
            public string msg { get; set; }
            public string error { get; set; }
        }

        public class ShopifyUpdateStockData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public long stock { get; set; }
        }

        public class ShopifyUpdateVariationStockData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public long variation_id { get; set; }
            public int stock { get; set; }
        }
        public class ShopifyError
        {
            public string msg { get; set; }
            public string request_id { get; set; }
            public string error { get; set; }
        }
        public class ShopifyGetLogisticsData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
        }

        public class ShopifyGetLogisticsResult
        {
            public ShopifyGetLogisticsLogistic[] logistics { get; set; }
            public string request_id { get; set; }
        }

        public class ShopifyGetLogisticsLogistic
        {
            public ShopifyGetLogisticsResultWeight_Limits weight_limits { get; set; }
            public bool has_cod { get; set; }
            public ShopifyGetLogisticsResultItem_Max_Dimension item_max_dimension { get; set; }
            public object[] sizes { get; set; }
            public string logistic_name { get; set; }
            public bool enabled { get; set; }
            public long logistic_id { get; set; }
            public string fee_type { get; set; }
        }

        public class ShopifyGetLogisticsResultWeight_Limits
        {
            public float item_min_weight { get; set; }
            public float item_max_weight { get; set; }
        }

        public class ShopifyGetLogisticsResultItem_Max_Dimension
        {
            public int width { get; set; }
            public int length { get; set; }
            public string unit { get; set; }
            public int height { get; set; }
        }
        public class ShopifyCancelOrderData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
            public string cancel_reason { get; set; }
        }
        public class ShopifyCancelOrderResult
        {
            public long modified_time { get; set; }
            public string request_id { get; set; }
            public string msg { get; set; }
            public string error { get; set; }
        }
        public class ShopifyGetVariation
        {
            public long item_id { get; set; }
            public long shopid { get; set; }
            public long partner_id { get; set; }
            public long timestamp { get; set; }
        }
        public class ShopifyInitTierVariation
        {
            public long item_id { get; set; }
            public ShopifyTierVariation[] tier_variation { get; set; }
            public ShopifyVariation[] variation { get; set; }
            public long shopid { get; set; }
            public long partner_id { get; set; }
            public long timestamp { get; set; }

        }

        public class ShopifyUpdateTierVariationIndex
        {
            public long item_id { get; set; }
            public ShopifyTierVariation[] tier_variation { get; set; }
            //public ShopifyUpdateVariation[] variation { get; set; }
            //public ShopifyVariation[] variation { get; set; }
            public object[] variation { get; set; }
            public long shopid { get; set; }
            public long partner_id { get; set; }
            public long timestamp { get; set; }
        }
        public class ShopifyUpdateTierVariationList
        {
            public long item_id { get; set; }
            public ShopifyTierVariation[] tier_variation { get; set; }
            public long shopid { get; set; }
            public long partner_id { get; set; }
            public long timestamp { get; set; }
        }

        public class ShopifyUpdateTierVariationResult
        {
            public long item_id { get; set; }
            public string request_id { get; set; }
        }

        public class ShopifyTierVariation
        {
            public string name { get; set; }
            public string[] options { get; set; }
        }
        public class ShopifyVariation
        {
            public int[] tier_index { get; set; }
            public int stock { get; set; }
            public float price { get; set; }
            public string variation_sku { get; set; }
        }
        public class ShopifyUpdateVariation
        {
            public int[] tier_index { get; set; }
            public int stock { get; set; }
            public float price { get; set; }
            public string variation_sku { get; set; }
            public long? variation_id { get; set; }
        }

        public class ShopifyProductData
        {
            public long category_id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public double price { get; set; }
            public int stock { get; set; }
            public string item_sku { get; set; }
            public List<ShopifyVariationClass> variations { get; set; }
            public List<ShopifyImageClass> images { get; set; }
            public List<ShopifyAttributeClass> attributes { get; set; }
            public List<ShopifyLogisticsClass> logistics { get; set; }
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

        public class ShopifyVariationClass
        {
            public string name { get; set; }
            public int stock { get; set; }
            public double price { get; set; }
            public string variation_sku { get; set; }
        }
        public class ShopifyImageClass
        {
            public string url { get; set; }
        }
        public class ShopifyAttributeClass
        {
            public long attributes_id { get; set; }
            public string value { get; set; }

        }
        public class ShopifyLogisticsClass
        {
            public long logistic_id { get; set; }
            public bool enabled { get; set; }
            //public double shipping_fee { get; set; }//Only needed when logistics fee_type = CUSTOM_PRICE.
            //public long size_id { get; set; }//If specify logistic fee_type is SIZE_SELECTION size_id is required
            public bool is_free { get; set; }

        }

        public class ShopifyCreateProdResponse : ShopifyError
        {
            public long item_id { get; set; }
            public Item item { get; set; }
        }

        public class ShopifyCreatePromoRes : ShopifyError
        {
            public long discount_id { get; set; }
            public int count { get; set; }
        }
        public class ShopifyDeletePromo : ShopifyError
        {
            public UInt64 discount_id { get; set; }
            public DateTime modify_time { get; set; }
        }
        public class Item
        {
            public List<ShopifyLogisticsClass> logistics { get; set; }
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

        public class ShopifyAcceptBuyerCancelOrderData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
        }
        public class ShopifyUpdatePriceData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public float price { get; set; }
        }
        public class ShopifyUpdateImageData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public string[] images { get; set; }
        }
        public class ShopifyUpdateVariantionPriceData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public long variation_id { get; set; }
            public float price { get; set; }
        }
        public class ShopifyAddDiscountData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public string discount_name { get; set; }
            public long start_time { get; set; }
            public long end_time { get; set; }
            public List<ShopifyAddDiscountDataItems> items { get; set; }
        }
        public class ShopifyAddDiscountDataItems
        {
            public long item_id { get; set; }
            public float item_promotion_price { get; set; }
            public UInt32 purchase_limit { get; set; }
            public List<ShopifyAddDiscountDataItemsVariation> variations { get; set; }
        }
        public class ShopifyAddDiscountDataItemsVariation
        {
            public long variation_id { get; set; }
            public float variation_promotion_price { get; set; }
        }

        public class ShopifyAddDiscountItemData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long discount_id { get; set; }
            public List<ShopifyAddDiscountDataItems> items { get; set; }
        }
        public class ShopifyDeleteDiscountItemData : ShopifyError
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long discount_id { get; set; }
            public long item_id { get; set; }
            public long variation_id { get; set; }
        }
        public class ShopifyDeleteDiscountData : ShopifyError
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long discount_id { get; set; }
        }

        public class ShopifyGetAddressData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
        }
        public class ShopifyGetAirwayBill
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public bool is_batch { get; set; }
            public List<string> ordersn_list { get; set; } = new List<string>();
        }
        public class ShopifyGetTimeSlotData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public long address_id { get; set; }
            public string ordersn { get; set; }
        }
        public class ShopifyGetBranchData
        {
            public long partner_id { get; set; }
            public long shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
        }

        public class ShopifyGetAddressResult
        {
            public string request_id { get; set; }
            public ShopifyGetAddressResultAddress_List[] address_list { get; set; }
        }

        public class ShopifyGetAddressResultAddress_List
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

        public class ShopifyGetTimeSlotResult
        {
            public ShopifyGetTimeSlotResultPickup_Time[] pickup_time { get; set; }
            public string request_id { get; set; }
        }

        public class ShopifyGetTimeSlotResultPickup_Time
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

        public class ShopifyAddTierVariation
        {
            public long item_id { get; set; }
            public ShopifyVariation[] variation { get; set; }
            public long shopid { get; set; }
            public long partner_id { get; set; }
            public long timestamp { get; set; }

        }
        public class ShopifyAddVariation
        {
            public long item_id { get; set; }
            public ShopifyNewVariation[] variations { get; set; }
            public long shopid { get; set; }
            public long partner_id { get; set; }
            public long timestamp { get; set; }

        }
        public class ShopifyNewVariation
        {

            public string name { get; set; }
            public int stock { get; set; }
            public float price { get; set; }
            public string variation_sku { get; set; }
        }

        /////////////////////// SHOPIFY VARIABLE OBJECT JSON ADD BY FAUZI

        // CREATE PRODUCT
        public class ShopifyCreateProduct
        {
            public ShopifyCreateProductData product { get; set; }
        }

        public class ShopifyCreateProductData
        {
            public string title { get; set; }
            public string body_html { get; set; }
            public string vendor { get; set; }
            public string product_type { get; set; }
            public string template_suffix { get; set; }
            public string tags { get; set; }
            public List<ShopifyCreateProductDataVariant> variants { get; set; }
            public List<ShopifyCreateProductImages> images { get; set; }
        }

        public class ShopifyCreateProductDataVariant
        {
            public string option1 { get; set; }
            public string price { get; set; }
            public object sku { get; set; }
            public int inventory_quantity { get; set; }
            public int grams { get; set; }
            public float weight { get; set; }
            public string weight_unit { get; set; }
        }

        public class ShopifyCreateProductImages
        {
            public object alt { get; set; }
            public string position { get; set; }
            public string src { get; set; }
        }
        //// END CREATE PRODUCT 

        /// RESULT CREATE PRODUCT SHOPIFY
        public class ShopifyCreateResult
        {
            public ShopifyCreateProductResult product { get; set; }
            public ShopifyCreateProductResultErrors errors { get; set; }
        }

        public class ShopifyCreateProductResultErrors
        {
            public string[] inventory_quantity { get; set; }
        }

        public class ShopifyCreateProductResult
        {
            public long id { get; set; }
            public string title { get; set; }
            public string body_html { get; set; }
            public string vendor { get; set; }
            public string product_type { get; set; }
            public DateTime created_at { get; set; }
            public string handle { get; set; }
            public DateTime updated_at { get; set; }
            public DateTime published_at { get; set; }
            public object template_suffix { get; set; }
            public string published_scope { get; set; }
            public string tags { get; set; }
            public string admin_graphql_api_id { get; set; }
            public ShopifyCreateProductVariantsResult[] variants { get; set; }
            public ShopifyCreateProductOptionResult[] options { get; set; }
            public ShopifyCreateProductImagesResult[] images { get; set; }
            public ShopifyCreateProductImageResult image { get; set; }
        }

        public class ShopifyCreateProductImageResult
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyCreateProductVariantsResult
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string title { get; set; }
            public string price { get; set; }
            public string sku { get; set; }
            public int position { get; set; }
            public string inventory_policy { get; set; }
            public object compare_at_price { get; set; }
            public string fulfillment_service { get; set; }
            public object inventory_management { get; set; }
            public string option1 { get; set; }
            public object option2 { get; set; }
            public object option3 { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public bool taxable { get; set; }
            public object barcode { get; set; }
            public int grams { get; set; }
            public object image_id { get; set; }
            public float weight { get; set; }
            public string weight_unit { get; set; }
            public long inventory_item_id { get; set; }
            public int inventory_quantity { get; set; }
            public int old_inventory_quantity { get; set; }
            public bool requires_shipping { get; set; }
            public string admin_graphql_api_id { get; set; }
        }

        public class ShopifyCreateProductOptionResult
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public string name { get; set; }
            public int position { get; set; }
            public string[] values { get; set; }
        }

        public class ShopifyCreateProductImagesResult
        {
            public long id { get; set; }
            public long product_id { get; set; }
            public int position { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public object alt { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string src { get; set; }
            public object[] variant_ids { get; set; }
            public string admin_graphql_api_id { get; set; }
        }
        // END RESULT CREATE PRODUCT

        ///// UPDATE PRODUCT
        public class ShopifyUpdateProduct
        {
            public ShopifyUpdateProductData product { get; set; }
        }

        public class ShopifyUpdateProductData
        {
            public long id { get; set; }
            public string title { get; set; }
            public string body_html { get; set; }
            public string vendor { get; set; }
            public string product_type { get; set; }
            public string template_suffix { get; set; }
            public string tags { get; set; }
            public List<ShopifyUpdateProductDataVariant> variants { get; set; }
            public List<ShopifyUpdateProductImages> images { get; set; }
        }

        public class ShopifyUpdateProductDataVariant
        {
            public string option1 { get; set; }
            public string price { get; set; }
            public object sku { get; set; }
            public int inventory_quantity { get; set; }
            public int grams { get; set; }
            public float weight { get; set; }
            public string weight_unit { get; set; }
        }

        public class ShopifyUpdateProductImages
        {
            public object alt { get; set; }
            public string position { get; set; }
            public string src { get; set; }
        }
        //// END CREATE PRODUCT 
    }
}