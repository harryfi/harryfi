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
                                        var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '1', AL = '" + result.shop.address1 + "', Sort1_Cust = '" + result.shop.id + "' WHERE CUST = '" + dataAPI.no_cust + "'");
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
                                                        if (itemImages.variant_ids.Count() <= 1)
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