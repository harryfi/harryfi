using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Xml;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Net.Http;
using System.Globalization;
using Erasoft.Function;
using MasterOnline.Models;
using MasterOnline.ViewModels;

namespace MasterOnline.Controllers
{
    public class EightTwoCartController : Controller
    {
        // GET: EightTwoCart
        private string url = "dev.api.82cart.com";
        private string apiKey = "35e8ac7e833721d0bd7d5bc66cb75";
        private string apiCred = "dev82cart";

        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        DatabaseSQL EDB;
        string dbPathEra = "";
        string dbSourceEra = "";
        string usernameLogin;
        string EDBConnID = "";
        private string username;

        public ActionResult Index()
        {
            return View();
        }

        public EightTwoCartController()
        {
            MoDbContext = new MoDbContext("");
            usernameLogin = "";
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
            {
                dbPathEra = sessionData.Account.DatabasePathErasoft;
                dbSourceEra = sessionData.Account.DataSourcePath;

                if (sessionData.Account.UserId == "admin_manage")
                    ErasoftDbContext = new ErasoftContext();
                else
                    ErasoftDbContext = new ErasoftContext(dbSourceEra, dbPathEra);

                EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);
                EDBConnID = EDB.GetConnectionString("ConnID");
                usernameLogin = sessionData.Account.Username;

            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    dbPathEra = accFromUser.DatabasePathErasoft;
                    dbSourceEra = accFromUser.DataSourcePath;
                    ErasoftDbContext = new ErasoftContext(dbSourceEra, dbPathEra);

                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                    EDBConnID = EDB.GetConnectionString("ConnID");
                    usernameLogin = sessionData.User.Username;
                }
            }
            if (usernameLogin.Length > 20)
                usernameLogin = usernameLogin.Substring(0, 17) + "...";

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

        public static long CurrentTimeSecond()
        {
            //        return (long)DateTime.Now.ToUniversalTime().Subtract(
            //new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            //).TotalMilliseconds;
            return (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

        //Get All Products.
        public async Task<BindingBase82Cart> E2Cart_GetProductsList(E2CartAPIData iden, int IdMarket, int page, int recordCount, int totalData)
        {
            var ret = new BindingBase82Cart
            {
                status = 0,
                recordCount = recordCount,
                exception = 0,
                totalData = totalData//add 18 Juli 2019, show total record
            };

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Get Item List",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.API_credential,
                REQUEST_ATTRIBUTE_3 = page.ToString(),
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            var limit = "10";
            var offset = 10 * page;

            string urll = string.Format("{0}/api/v1/getProduct?apiKey={1}&apiCredential={2}&limit={3}&offset={4}", iden.API_url, iden.API_key, iden.API_credential, limit, offset);


            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            try
            {
                using (WebResponse response = myReq.GetResponse())
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
                ret.message = ex.Message.ToString();
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (!string.IsNullOrEmpty(responseServer))
            {
                try
                {
                    var resultAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(E2CartProductListResult)) as E2CartProductListResult;

                    ret.status = 1;
                    if (resultAPI != null)
                    {
                        if (resultAPI.data != null && resultAPI.error == "none")
                        {
                            if (resultAPI.data.Count() > 0)
                            {
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);

                                if (resultAPI.data.Count() == 10)
                                    ret.nextPage = 1;

                                List<TEMP_BRG_MP> listNewRecord = new List<TEMP_BRG_MP>();
                                var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.IDMARKET.Equals(IdMarket)).Select(t => new { t.CUST, t.BRG_MP }).ToList();
                                //var tempBrg_local = (from a in ErasoftDbContext.TEMP_BRG_MP where a.IDMARKET == IdMarket select new tempBrg_local { BRG_MP = a.BRG_MP, IDMARKET = a.IDMARKET }).ToList();
                                var brgInDB = ErasoftDbContext.STF02H.Where(t => t.IDMARKET == IdMarket).Select(t => new { t.RecNum, t.BRG_MP }).ToList();
                                string brgMp = "";

                                foreach (var item in resultAPI.data)
                                {
                                    brgMp = Convert.ToString(item.id_product) + ";0";
                                    //if (item.status.ToUpper() != "DELETE" && item.status.ToUpper() != "BANNED")
                                    if (item.active == "1")
                                    {
                                        ret.totalData++;//add 18 Juli 2019, show total record
                                        var CektempbrginDB = tempbrginDB.Where(t => (t.BRG_MP ?? "").ToUpper().Equals(brgMp.ToUpper())).FirstOrDefault();
                                        //var CekbrgInDB = brgInDB.Where(t => t.BRG_MP.Equals(brgMp)).FirstOrDefault();
                                        var CekbrgInDB = brgInDB.Where(t => (t.BRG_MP ?? "").Equals(brgMp)).FirstOrDefault();
                                        if (CektempbrginDB == null && CekbrgInDB == null)
                                        {
                                            string namaBrg = item.name;
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
                                                SELLER_SKU = brgMp,
                                                BRG_MP = brgMp,
                                                //KODE_BRG_INDUK = Convert.ToString(item.id_product),
                                                NAMA = nama,
                                                NAMA2 = nama2,
                                                NAMA3 = nama3,
                                                CATEGORY_CODE = Convert.ToString(item.id_category_default),
                                                CATEGORY_NAME = item.category_default,
                                                IDMARKET = IdMarket,
                                                IMAGE = item.cover_image_url ?? "",
                                                DISPLAY = true,
                                                HJUAL = Convert.ToDouble(item.price),
                                                HJUAL_MP = Convert.ToDouble(item.price),
                                                Deskripsi = item.description.Replace("\r\n", "<br />").Replace("\n", "<br />"),
                                                MEREK = item.id_manufacturer,
                                                CUST = iden.no_cust,
                                            };
                                            newrecord.AVALUE_45 = namaBrg.Length > 250 ? namaBrg.Substring(0, 250) : namaBrg; //request by Calvin 19 maret 2019, isi nama barang ke avalue 45
                                                                                                                              //add by Tri, 26 Feb 2019
                                            var kategory = MoDbContext.Category82Cart.Where(m => m.ID_CATEGORY == newrecord.CATEGORY_CODE).FirstOrDefault();
                                            if (kategory != null)
                                            {
                                                newrecord.CATEGORY_NAME = kategory.NAME;
                                            }

                                            int typeBrg = 0;
                                            //if (!string.IsNullOrEmpty(Convert.ToString(item.other.sku)))
                                            //    newrecord.SELLER_SKU = item.other.sku;
                                            if (item.combinations.Length == 0)//barang non varian
                                            {
                                                newrecord.TYPE = "3";
                                            }
                                            else
                                            {
                                                if (item.combinations.Length > 1)
                                                {
                                                    typeBrg = 1;
                                                    newrecord.TYPE = "4";
                                                    foreach (var varID in item.combinations)
                                                    {
                                                        var brg_mp_variant = Convert.ToString(item.id_product) + ";" + varID.id_product_attribute.ToString();
                                                        var CektempbrginDB2 = tempbrginDB.Where(t => (t.BRG_MP ?? "").Equals(brg_mp_variant)).FirstOrDefault();
                                                        var CekbrgInDB2 = brgInDB.Where(t => (t.BRG_MP ?? "").Equals(brg_mp_variant)).FirstOrDefault();
                                                        if (CektempbrginDB2 == null && CekbrgInDB2 == null)
                                                        {
                                                            var retVar = await E2Cart_GetProductVariant(iden, item, varID, brg_mp_variant, brgMp, iden.no_cust, IdMarket);
                                                            ret.recordCount += retVar.recordCount;
                                                        }
                                                    }
                                                    ret.totalData += item.combinations.Count();
                                                }
                                                else
                                                {
                                                    //newrecord.TYPE = "3";
                                                    //newrecord.KODE_BRG_INDUK = Convert.ToString(item.id_product);
                                                    typeBrg = 2;
                                                }
                                            }
                                            if (item.weight != null)
                                            {
                                                if (Convert.ToDouble(item.weight) > 0)
                                                {
                                                    newrecord.BERAT = Convert.ToDouble(item.weight);
                                                }
                                            }
                                            if (item.height != null)
                                            {
                                                if (Convert.ToDouble(item.height) > 0)
                                                {
                                                    newrecord.TINGGI = Convert.ToDouble(item.height);
                                                }
                                            }
                                            if (item.width != null)
                                            {
                                                if (Convert.ToDouble(item.width) > 0)
                                                {
                                                    newrecord.LEBAR = Convert.ToDouble(item.width);
                                                }
                                            }
                                            //if (item.menu != null)
                                            //{
                                            //    if (!string.IsNullOrEmpty(Convert.ToString(item.menu.id)))
                                            //    {
                                            //        newrecord.PICKUP_POINT = Convert.ToString(item.menu.id);
                                            //    }
                                            //}
                                            if (item.combinations != null)
                                                if (item.combinations.Length > 1)
                                                {
                                                    if (item.combinations[0].attribute_image.Length > 0)
                                                    {
                                                        newrecord.IMAGE = item.combinations[0].attribute_image[0].image_url;
                                                    }
                                                    foreach (var detail in item.combinations)
                                                    {
                                                        if (detail.attribute_image.Length > 0)
                                                        {
                                                            newrecord.IMAGE = detail.attribute_image[0].image_url;

                                                            if (detail.attribute_image.Length > 2 && typeBrg != 2)
                                                            {
                                                                newrecord.IMAGE2 = detail.attribute_image[1].image_url;
                                                                if (detail.attribute_image.Length > 3)
                                                                {
                                                                    newrecord.IMAGE3 = detail.attribute_image[2].image_url;
                                                                    if (detail.attribute_image.Length > 4)
                                                                    {
                                                                        newrecord.IMAGE4 = detail.attribute_image[3].image_url;
                                                                        if (detail.attribute_image.Length > 5)
                                                                        {
                                                                            newrecord.IMAGE5 = detail.attribute_image[4].image_url;

                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                            listNewRecord.Add(newrecord);
                                            ret.recordCount = ret.recordCount + 1;
                                        }
                                        //else if (item.combinations.Length > 1)
                                        //{
                                        //    foreach (var varID in item.combinations)
                                        //    {
                                        //        var brg_mp_variant = Convert.ToString(item.id_product) + ";" + varID.id_product_attribute.ToString();
                                        //        var CektempbrginDB2 = tempbrginDB.Where(t => (t.BRG_MP ?? "").Equals(varID.id_product_attribute.ToString())).FirstOrDefault();
                                        //        var CekbrgInDB2 = brgInDB.Where(t => (t.BRG_MP ?? "").Equals(varID.id_product_attribute.ToString())).FirstOrDefault();
                                        //        if (CektempbrginDB2 == null && CekbrgInDB2 == null)
                                        //        {
                                        //            var retVar = await E2Cart_GetProductVariant(iden, item, varID, brg_mp_variant, brgMp, iden.no_cust, IdMarket);
                                        //            ret.recordCount += retVar.recordCount;
                                        //        }
                                        //    }
                                        //    ret.totalData += item.combinations.Count();
                                        //}
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
                catch (Exception ex)
                {
                    ret.exception = 1;
                    ret.message = ex.Message.ToString();
                    currentLog.REQUEST_EXCEPTION = ret.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
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

        public async Task<BindingBase82Cart> E2Cart_GetProductVariant(E2CartAPIData iden, E2CartProduct product_induk, ProductCombinations product_varian, string brgmp_varian, string brg_mp_induk, string CUST, int idmarket)
        {
            var ret = new BindingBase82Cart();

            string status = "";
            List<TEMP_BRG_MP> listNewRecord = new List<TEMP_BRG_MP>();
            ret.totalData++;

            string namaBrg = product_induk.name;
            string nama, nama2, nama3, urlImage, urlImage2, urlImage3;
            urlImage = "";
            urlImage2 = "";
            urlImage3 = "";
            namaBrg = namaBrg.Replace('\'', '`');

            var splitItemName = new StokControllerJob().SplitItemName(namaBrg);
            nama = splitItemName[0];
            nama2 = splitItemName[1];
            if (!string.IsNullOrEmpty(product_varian.attribute_list[0].attribute_group) && !string.IsNullOrEmpty(product_varian.attribute_list[0].attribute))
            {
                nama2 = product_varian.attribute_list[0].attribute_group.ToString() + " " + product_varian.attribute_list[0].attribute.ToString();
            }
            nama3 = "";

            double hargaJual = 0;
            if(Convert.ToDouble(product_varian.price) > 0)
            {
                hargaJual = Convert.ToDouble(product_varian.price);
            }
            else
            {
                hargaJual = Convert.ToDouble(product_induk.price);
            }

            var skuBRG = "";
            if (!string.IsNullOrEmpty(product_varian.attribute_reference))
            {
                skuBRG = product_varian.attribute_reference;
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
                CATEGORY_CODE = Convert.ToString(product_induk.id_category_default),
                CATEGORY_NAME = "",
                IDMARKET = idmarket,
                IMAGE = product_induk.cover_image_url ?? "",
                DISPLAY = true,
                HJUAL = hargaJual,
                HJUAL_MP = hargaJual,
                Deskripsi = product_induk.description.Replace("\r\n", "<br />").Replace("\n", "<br />"),
                MEREK = product_induk.id_manufacturer,
                CUST = CUST,
            };
            newrecord.AVALUE_45 = namaBrg.Length > 250 ? namaBrg.Substring(0, 250) : namaBrg; //request by Calvin 19 maret 2019, isi nama barang ke avalue 45
                                                                                              //add by Tri, 26 Feb 2019
            var kategory = MoDbContext.Category82Cart.Where(m => m.ID_CATEGORY == newrecord.CATEGORY_CODE).FirstOrDefault();
            if (kategory != null)
            {
                newrecord.CATEGORY_NAME = kategory.NAME;
            }
            //end add by Tri, 26 Feb 2019

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
            if (product_induk.weight != null)
            {
                if (Convert.ToDouble(product_induk.weight) > 0)
                {
                    newrecord.BERAT = Convert.ToDouble(product_induk.weight);
                }
            }
            if (product_induk.height != null)
            {
                if (Convert.ToDouble(product_induk.height) > 0)
                {
                    newrecord.TINGGI = Convert.ToDouble(product_induk.height);
                }
            }
            if (product_induk.width != null)
            {
                if (Convert.ToDouble(product_induk.width) > 0)
                {
                    newrecord.LEBAR = Convert.ToDouble(product_induk.width);
                }
            }
            //if (item.menu != null)
            //{
            //    if (!string.IsNullOrEmpty(Convert.ToString(item.menu.id)))
            //    {
            //        newrecord.PICKUP_POINT = Convert.ToString(item.menu.id);
            //    }
            //}
            if (product_varian.attribute_image != null)
                if (product_varian.attribute_image.Length > 0)
                {
                    newrecord.IMAGE = product_varian.attribute_image[0].image_url;
                    if (product_varian.attribute_image.Length > 1 && typeBrg != 2)
                    {
                        newrecord.IMAGE2 = product_varian.attribute_image[1].image_url;
                        if (product_varian.attribute_image.Length > 2)
                        {
                            newrecord.IMAGE3 = product_varian.attribute_image[2].image_url;
                            if (product_varian.attribute_image.Length > 3)
                            {
                                newrecord.IMAGE4 = product_varian.attribute_image[3].image_url;
                                if (product_varian.attribute_image.Length > 4)
                                {
                                    newrecord.IMAGE5 = product_varian.attribute_image[4].image_url;

                                }
                            }
                        }
                    }
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

                }
            }
            return ret;
        }

        //Add Product.
        public async Task<string> E2Cart_CreateProduct(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, E2CartAPIData iden)
        {
            //string ret = "";
            //SetupContext(iden);

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            //handle log activity
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Create Product",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = kodeProduk,
                REQUEST_ATTRIBUTE_2 = "fs : " + iden.API_credential,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            //handle log activity

            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper()).FirstOrDefault();
            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == log_CUST.ToUpper()).FirstOrDefault();
            if (brgInDb == null || marketplace == null)
                return "invalid passing data";
            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
            if (detailBrg == null)
                return "invalid passing data";

            string urll = string.Format("{0}/api/v1/addProduct", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            postData += "&name=" + Uri.EscapeDataString(brgInDb.NAMA);
            postData += "&active=" + Uri.EscapeDataString("1");
            postData += "&visibility=" + Uri.EscapeDataString("both");
            postData += "&available_for_order=" + Uri.EscapeDataString("1");
            postData += "&show_price=" + Uri.EscapeDataString("1");
            postData += "&online_only=" + Uri.EscapeDataString("0");
            postData += "&condition=" + Uri.EscapeDataString("new");
            postData += "&wholesale_price=" + Uri.EscapeDataString(detailBrg.HJUAL.ToString());
            postData += "&price=" + Uri.EscapeDataString(detailBrg.HJUAL.ToString());
            postData += "&on_sale=" + Uri.EscapeDataString("1");
            postData += "&link_rewrite=" + Uri.EscapeDataString("asus-vivobook-2020");
            postData += "&id_category_default=" + Uri.EscapeDataString("2");
            postData += "&width=" + Uri.EscapeDataString(brgInDb.LEBAR.ToString());
            postData += "&height=" + Uri.EscapeDataString(brgInDb.TINGGI.ToString());
            postData += "&depth=" + Uri.EscapeDataString("0");
            postData += "&weight=" + Uri.EscapeDataString(brgInDb.BERAT.ToString());
            postData += "&additional_shipping_cost=" + Uri.EscapeDataString("200000");
            postData += "&minimal_quantity=" + Uri.EscapeDataString("1");
            postData += "&out_of_stock=" + Uri.EscapeDataString("0");

            //Start handle Check Category
            var resultCategory = await E2Cart_CheckAlreadyCategory(iden, brgInDb.KET_SORT1);
            if (!string.IsNullOrEmpty(resultCategory.name_category) && !string.IsNullOrEmpty(resultCategory.id_category))
            {
                postData += "&category=" + Uri.EscapeDataString("[2,3," + resultCategory.id_category + "]");
            }
            else
            {
                resultCategory = await E2Cart_AddCategoryProduct(iden, "3", brgInDb.KET_SORT1);
                postData += "&category=" + Uri.EscapeDataString("[2,3," + resultCategory.id_category + "]");
                //resultCategory = await E2Cart_CheckCategoryAlready(iden, brgInDb.Sort2);
            }
            //End handle Check Category

            //Start handle Check Manufacture/Brand
            var resultManufacture = await E2Cart_CheckAlreadyManufacture(iden, brgInDb.KET_SORT2);
            if (!string.IsNullOrEmpty(resultManufacture.name_manufacture) && !string.IsNullOrEmpty(resultManufacture.id_manufacture))
            {
                postData += "&id_manufacturer=" + Uri.EscapeDataString(resultManufacture.id_manufacture);
            }
            else
            {
                resultManufacture = await E2Cart_AddManufactureProduct(iden, brgInDb.KET_SORT2);
                postData += "&id_manufacturer=" + Uri.EscapeDataString(resultManufacture.id_manufacture);
            }
            //End handle Check Manufacture/Brand

            //Start handle description
            var vDescription = brgInDb.Deskripsi;
            vDescription = new StokControllerJob().RemoveSpecialCharacters(vDescription);

            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
            vDescription = vDescription.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
            vDescription = vDescription.Replace("<li>", "- ").Replace("</li>", "\r\n");
            vDescription = vDescription.Replace("<ul>", "").Replace("</ul>", "\r\n");
            vDescription = vDescription.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
            vDescription = vDescription.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
            vDescription = vDescription.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");
            //end add by nurul 20/1/2020, handle <p> dan enter double di shopee

            //add by calvin 10 september 2019
            vDescription = vDescription.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
            vDescription = vDescription.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
            vDescription = vDescription.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
            vDescription = vDescription.Replace("<p>", "\r\n").Replace("</p>", "\r\n");
            //HttpBody.description = HttpBody.description.Replace("<li>", "- ").Replace("</li>", "\r\n");
            //HttpBody.description = HttpBody.description.Replace("&nbsp;", "");

            vDescription = System.Text.RegularExpressions.Regex.Replace(vDescription, "<.*?>", String.Empty);
            //end add by calvin 10 september 2019

            postData += "&description_short=" + Uri.EscapeDataString(vDescription);
            postData += "&description=" + Uri.EscapeDataString(vDescription);
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
            qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(kodeProduk, "ALL");
            if (qty_stock > 0)
            {
                postData += "&quantity=" + Uri.EscapeDataString(qty_stock.ToString());
            }
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
                                //handle all image was uploaded
                                foreach (var images in lGambarUploaded)
                                {
                                    Task.Run(() => E2Cart_AddImageProduct(iden, resultAPI.data.data[0].id_product, images)).Wait();
                                }
                                //end handle all image was uploaded

                                //start handle update stock for default product
                                Task.Run(() => E2Cart_UpdateStock_82Cart(iden, resultAPI.data.data[0].id_product, item.BRG_MP, Convert.ToInt32(qty_stock))).Wait();
                                //end handle update stock for default product

                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            }
                        }
                        else
                        {
                            if (resultAPI.error != null && resultAPI.error != "none")
                            {
                                currentLog.REQUEST_EXCEPTION = resultAPI.error.ToString();
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            return "";
        }

        //Add or Edit Product.
        public ActionResult E2Cart_UpdateProduct(E2CartAPIData iden, string prodId)
        {
            string urll = string.Format("{0}/api/v1/addProduct", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(apiKey);
            postData += "&apiCredential=" + Uri.EscapeDataString(apiCred);
            if (!string.IsNullOrEmpty(prodId))
            {
                postData += "&id_product=" + Uri.EscapeDataString(prodId);
            }
            postData += "&name=" + Uri.EscapeDataString("Yamaha Pacifica 112J");
            postData += "&active=" + Uri.EscapeDataString("1");
            postData += "&visibility=" + Uri.EscapeDataString("both");
            postData += "&available_for_order=" + Uri.EscapeDataString("1");
            postData += "&show_price=" + Uri.EscapeDataString("1");
            postData += "&online_only=" + Uri.EscapeDataString("0");
            postData += "&condition=" + Uri.EscapeDataString("new");
            postData += "&wholesale_price=" + Uri.EscapeDataString("1650000");
            postData += "&price=" + Uri.EscapeDataString("2000000");
            postData += "&on_sale=" + Uri.EscapeDataString("1");
            postData += "&link_rewrite=" + Uri.EscapeDataString("yamaha-pacifica-112j");
            postData += "&id_category_default=" + Uri.EscapeDataString("9");
            postData += "&id_manufacturer=" + Uri.EscapeDataString("2");
            postData += "&width=" + Uri.EscapeDataString("5");
            postData += "&height=" + Uri.EscapeDataString("100");
            postData += "&depth=" + Uri.EscapeDataString("5");
            postData += "&weight=" + Uri.EscapeDataString("20");
            postData += "&additional_shipping_cost=" + Uri.EscapeDataString("200000");
            postData += "&minimal_quantity=" + Uri.EscapeDataString("1");
            postData += "&out_of_stock=" + Uri.EscapeDataString("2");
            postData += "&category=" + Uri.EscapeDataString("[2,3,9]");

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

                ViewBag.response = resServer;
            }


            return View();
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


        //[AutomaticRetry(Attempts = 3)]
        //[Queue("1_create_product")]
        //[NotifyOnFailed("Update Harga Jual Produk {obj} ke 82Cart gagal.")]
        public async Task<string> E2Cart_UpdatePrice_82Cart(E2CartAPIData iden, string brg_mo, string brg_mp, float price)
        {
            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

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
            postData += "&id_product=" + Uri.EscapeDataString(brg_mp_split[0]);
            //postData += "&id_product_attribute=" + Uri.EscapeDataString("73");
            postData += "&price=" + Uri.EscapeDataString(price.ToString());

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

        //[AutomaticRetry(Attempts = 3)]
        //[Queue("1_update_stok")]
        //[NotifyOnFailed("Update Stok {obj} ke 82Cart gagal.")]
        public async Task<string> E2Cart_UpdateStock_82Cart(E2CartAPIData iden, string brg, string brg_mp, int qty)
        {
            string ret = "";
            //SetupContext(iden.DatabasePathErasoft, uname);
            var EDB = new DatabaseSQL(iden.DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");

            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);

            //handle log activity
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = milis.ToString(),
                REQUEST_ACTION = "Update Stock",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = "Kode Barang : " + brg,
                REQUEST_ATTRIBUTE_2 = "Barang MP : " + brg_mp,
                REQUEST_STATUS = "Pending",
            };
            var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, no_cust, currentLog, "82Cart");
            //handle log activity

            //var qtyOnHand = GetQOHSTF08A(brg, "ALL");
            //if (qtyOnHand < 0)
            //{
            //    qtyOnHand = 0;
            //}

            //qty = Convert.ToInt32(qtyOnHand);

            string urll = string.Format("{0}/api/v1/editInventory", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            string[] brg_mp_split = brg_mp.Split(';');

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            postData += "&id_product=" + Uri.EscapeDataString(brg_mp_split[0]);
            postData += "&id_product_attribute=" + Uri.EscapeDataString(brg_mp_split[1]);
            postData += "&stock=" + Uri.EscapeDataString(qty.ToString());

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
                        currentLog.REQUEST_EXCEPTION = resultAPI.error.ToString();
                        manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                        throw new Exception(resultAPI.error.ToString());
                    }
                    else
                    {
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                currentLog.REQUEST_EXCEPTION = msg;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                throw new Exception(msg);
            }

            return ret;
        }

        //public async Task<string> E2Cart_UpdateStock_82CartVariant(string DatabasePathErasoft, E2CartAPIData iden, string id_product, string no_cust, int qty, string uname)
        //{

        //    string url = "dev.api.82cart.com";
        //    string urll = string.Format("{0}/api/v1/editInventory", iden.API_url);

        //    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

        //    //Required parameters, other parameters can be add
        //    var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
        //    postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
        //    postData += "&id_product=" + Uri.EscapeDataString(id_product);
        //    //postData += "&id_product_attribute=" + Uri.EscapeDataString("73");
        //    postData += "&stock=" + Uri.EscapeDataString(qty.ToString());

        //    var data = Encoding.ASCII.GetBytes(postData);

        //    myReq.Method = "POST";
        //    myReq.ContentType = "application/x-www-form-urlencoded";
        //    myReq.ContentLength = data.Length;

        //    using (var stream = myReq.GetRequestStream())
        //    {
        //        stream.Write(data, 0, data.Length);
        //    }

        //    var response = (HttpWebResponse)myReq.GetResponse();

        //    var responseServer = new StreamReader(response.GetResponseStream()).ReadToEnd();

        //    if (!string.IsNullOrEmpty(responseServer))
        //    {
        //        var resultAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(ResultUpdateStock82Cart)) as ResultUpdateStock82Cart;

        //    }


        //    return "";
        //}

        //Add or Edit Product Attribute.
        public ActionResult E2Cart_PostProductAtt(E2CartAPIData iden)
        {

            string urll = string.Format("{0}/api/v1/addProductAttribute", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(apiKey);
            postData += "&apiCredential=" + Uri.EscapeDataString(apiCred);
            postData += "&id_product=" + Uri.EscapeDataString("134");
            //postData += "&id_product_attribute=" + Uri.EscapeDataString("134");
            postData += "&id_attribute=" + Uri.EscapeDataString("[48]");
            postData += "&wholesale_price=" + Uri.EscapeDataString("0");
            postData += "&price=" + Uri.EscapeDataString("0");
            postData += "&weight=" + Uri.EscapeDataString("25");
            postData += "&minimal_quantity=" + Uri.EscapeDataString("1");

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
                //ResultUpdateStock

                //ViewBag.response = resServer;
            }


            return View();
        }

        //Delete Product Image.
        public ActionResult E2Cart_DelProductImage(E2CartAPIData iden)
        {

            string urll = string.Format("{0}/api/v1/deleteProductImage", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(apiKey);
            postData += "&apiCredential=" + Uri.EscapeDataString(apiCred);
            postData += "&id_image=" + Uri.EscapeDataString("408");

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

                ViewBag.response = resServer;
            }


            return View();
        }

        //Get All Product Category.
        public async Task<BindingBase82Cart> E2Cart_CheckAlreadyCategory(E2CartAPIData iden, string nama_category)
        {
            BindingBase82Cart ret = new BindingBase82Cart();
            ret.status = 0;
            ret.exception = 0;
            ret.message = "";
            ret.recordCount = 0;
            ret.name_category = "";
            ret.id_category = "";

            string urll = string.Format("{0}/api/v1/getCategory?apiKey={1}&apiCredential={2}", iden.API_url, iden.API_key, iden.API_credential);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            try
            {
                using (WebResponse response = myReq.GetResponse())
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
                var vresultCategoryAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(E2CartProductCategoryResult)) as E2CartProductCategoryResult;
                if (vresultCategoryAPI.error == "none" && vresultCategoryAPI.data != null)
                {
                    if (vresultCategoryAPI.data[0].child[0].name == "Categories")
                    {
                        foreach (var child in vresultCategoryAPI.data[0].child[0].child)
                        {
                            if (child.name.ToUpper() == nama_category.ToUpper())
                            {
                                ret.name_category = child.name;
                                ret.id_category = child.id_category;
                                return ret;
                            }
                        }
                    }
                }
            }

            return ret;

        }

        //Get All Product Category.
        public async Task<BindingBase82Cart> E2Cart_GetCategoryProduct_2(E2CartAPIData iden, string id_category, string nama_category)
        {
            BindingBase82Cart ret = new BindingBase82Cart();
            ret.status = 0;
            ret.exception = 0;
            ret.message = "";
            ret.recordCount = 0;

            string urll = string.Format("{0}/api/v1/getCategory?apiKey={1}&apiCredential={2}", iden.API_url, iden.API_key, iden.API_credential);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            try
            {
                using (WebResponse response = myReq.GetResponse())
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
                var vresultCategoryAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(E2CartProductCategoryResult)) as E2CartProductCategoryResult;
                if (vresultCategoryAPI.error == "none" && vresultCategoryAPI.data != null)
                {
                    if (vresultCategoryAPI.data[0].child[0].child[0].name == "Categories")
                    {
                        foreach (var child in vresultCategoryAPI.data[0].child[0].child)
                        {
                            if (child.name == nama_category)
                            {
                                ret.message = child.name;
                            }
                        }
                    }
                }
            }

            return ret;

        }

        //Add Product Category.
        public async Task<BindingBase82Cart> E2Cart_AddCategoryProduct(E2CartAPIData iden, string idparentCategory, string nama_category)
        {
            BindingBase82Cart ret = new BindingBase82Cart();
            ret.status = 0;
            ret.exception = 0;
            ret.message = "";
            ret.recordCount = 0;
            ret.name_category = "";
            ret.id_category = "3";

            string urll = string.Format("{0}/api/v1/addCategory", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            //if (!string.IsNullOrEmpty(idCat))
            //{
            //    postData += "&id_category=" + Uri.EscapeDataString(idCat);
            //}
            postData += "&name=" + Uri.EscapeDataString(nama_category);
            postData += "&active=" + Uri.EscapeDataString("1");
            postData += "&id_parent=" + Uri.EscapeDataString(idparentCategory);
            postData += "&link_rewrite=" + Uri.EscapeDataString(nama_category.ToLower().Replace(" ", "-").Replace("&", "").Replace("--", "-"));

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
                var vresultCategoryAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(ResultAddCategory82Cart)) as ResultAddCategory82Cart;
                if (vresultCategoryAPI != null)
                {
                    if (vresultCategoryAPI.error == "none" && vresultCategoryAPI.results == "success")
                    {
                        if (vresultCategoryAPI.data != null)
                        {
                            if (vresultCategoryAPI.data.id_category != null)
                            {
                                ret.id_category = vresultCategoryAPI.data.id_category;
                            }
                        }
                    }
                }
            }


            return ret;
        }


        //Edit Product Category.
        public ActionResult E2Cart_EditCategoryProduct(E2CartAPIData iden, string idCat)
        {

            string urll = string.Format("{0}/api/v1/editCategory", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(apiKey);
            postData += "&apiCredential=" + Uri.EscapeDataString(apiCred);
            if (!string.IsNullOrEmpty(idCat))
            {
                postData += "&id_category=" + Uri.EscapeDataString(idCat);
            }
            postData += "&name=" + Uri.EscapeDataString("Pianos");
            postData += "&active=" + Uri.EscapeDataString("1");
            postData += "&id_parent=" + Uri.EscapeDataString("3");
            postData += "&link_rewrite=" + Uri.EscapeDataString("pianos");

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

                ViewBag.response = resServer;
            }


            return View();
        }


        public async Task<BindingBase82Cart> E2Cart_CheckAlreadyManufacture(E2CartAPIData iden, string name_manufacture)
        {
            BindingBase82Cart ret = new BindingBase82Cart();
            ret.status = 0;
            ret.exception = 0;
            ret.message = "";
            ret.recordCount = 0;
            ret.name_manufacture = "";
            ret.id_manufacture = "";

            string urll = string.Format("{0}/api/v1/getManufacturer?apiKey={1}&apiCredential={2}", iden.API_url, iden.API_key, iden.API_credential);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            try
            {
                using (WebResponse response = myReq.GetResponse())
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
                var vResultManifactureAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(E2CartManufactureResult)) as E2CartManufactureResult;
                if (vResultManifactureAPI != null)
                {
                    if (vResultManifactureAPI.error.ToString() == "none" && vResultManifactureAPI.data.Length > 0)
                    {
                        foreach (var item in vResultManifactureAPI.data)
                        {
                            if (item.name.ToUpper() == name_manufacture.ToUpper())
                            {
                                ret.id_manufacture = item.id_manufacturer;
                                ret.name_manufacture = item.name;
                                return ret;
                            }
                        }

                    }
                }

            }

            return ret;

        }

        //Get All Manufactures.
        public ActionResult E2Cart_GetManufacture(E2CartAPIData iden)
        {

            string urll = string.Format("{0}/api/v1/getManufacturer?apiKey={1}&apiCredential={2}", iden.API_url, iden.API_key, iden.API_credential);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            try
            {
                using (WebResponse response = myReq.GetResponse())
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
                var lProdMan = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(E2CartManufactureResult)) as E2CartManufactureResult;

                ViewBag.ProdMan = lProdMan.data;
            }

            return View();

        }

        //Add or Edit Product Manufacture.
        public async Task<BindingBase82Cart> E2Cart_AddManufactureProduct(E2CartAPIData iden, string name_manufature)
        {
            BindingBase82Cart ret = new BindingBase82Cart();
            ret.status = 0;
            ret.exception = 0;
            ret.message = "";
            ret.recordCount = 0;
            ret.name_manufacture = "";
            ret.id_manufacture = "";

            string urll = string.Format("{0}/api/v1/addManufacturer", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            //if (!string.IsNullOrEmpty(idMan))
            //{
            //    postData += "&id_manufacturer=" + Uri.EscapeDataString("");
            //}
            postData += "&name=" + Uri.EscapeDataString(name_manufature);
            postData += "&active=" + Uri.EscapeDataString("1");

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
                var vResult = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(ResultAddManufacturer)) as ResultAddManufacturer;
                if (vResult != null)
                {
                    if (vResult.error == "none" && vResult.data != null)
                    {
                        if (vResult.data.id_manufacturer != null)
                        {
                            ret.id_manufacture = vResult.data.id_manufacturer;
                            ret.name_manufacture = vResult.data.name;
                        }
                    }
                }
            }


            return ret;
        }

        //Add or Edit Product Manufacture.
        public ActionResult E2Cart_UpdateManufactureProduct(E2CartAPIData iden, string idMan)
        {

            string urll = string.Format("{0}/api/v1/addManufacturer", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            if (!string.IsNullOrEmpty(idMan))
            {
                postData += "&id_manufacturer=" + Uri.EscapeDataString("");
            }
            postData += "&name=" + Uri.EscapeDataString("Yamaha");
            postData += "&active=" + Uri.EscapeDataString("1");

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

                ViewBag.response = resServer;
            }


            return View();
        }

        //Get All Inventory.
        public ActionResult E2Cart_GetInventory(E2CartAPIData iden)
        {

            string urll = string.Format("{0}/api/v1/getInventory?apiKey={1}&apiCredential={2}", iden.API_url, iden.API_key, iden.API_credential);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            try
            {
                using (WebResponse response = myReq.GetResponse())
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
                var lProdCat = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(E2CartInventoryResult)) as E2CartInventoryResult;
                ViewBag.Inventory = lProdCat.data;
            }

            return View();

        }



        //Get All Attributes.
        public ActionResult E2Cart_GetAttribute(E2CartAPIData iden)
        {

            string urll = string.Format("{0}/api/v1/getAttribute?apiKey={1}&apiCredential={2}", iden.API_url, iden.API_key, iden.API_credential);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            try
            {
                using (WebResponse response = myReq.GetResponse())
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
                var lProdAtt = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(E2CartAttributeResult)) as E2CartAttributeResult;

                ViewBag.ProdAtt = lProdAtt.data;
            }

            return View();
        }

        //Add or Edit Attribute Group.
        public ActionResult E2Cart_PostProdAttGroup(E2CartAPIData iden, string idAttGrp)
        {

            string urll = string.Format("{0}/api/v1/addAttributeGroup", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(apiKey);
            postData += "&apiCredential=" + Uri.EscapeDataString(apiCred);
            if (!string.IsNullOrEmpty(idAttGrp))
            {
                postData += "&id_attribute_group=" + Uri.EscapeDataString("");
            }
            postData += "&name=" + Uri.EscapeDataString("Piano Color");
            postData += "&attribute_type=" + Uri.EscapeDataString("color");

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

                ViewBag.response = resServer;
            }


            return View();
        }

        //Add or Edit Attribute.
        public ActionResult E2Cart_PostProdAttribute(E2CartAPIData iden, string idAtt)
        {

            string urll = string.Format("{0}/api/v1/addAttribute", iden.API_url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(apiKey);
            postData += "&apiCredential=" + Uri.EscapeDataString(apiCred);
            if (string.IsNullOrEmpty(idAtt))
            {
                postData += "&id_attribute_group=" + Uri.EscapeDataString("6");
            }
            else
            {
                postData += "&id_attribute=" + Uri.EscapeDataString("");
            }
            postData += "&name=" + Uri.EscapeDataString("Black");
            postData += "&color=" + Uri.EscapeDataString("#000000");

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

                ViewBag.response = resServer;
            }


            return View();
        }

        //Get All Customers.
        public async Task<string> E2Cart_GetCustomer(E2CartAPIData iden)
        {
            string ret = "";
            var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();

            string urll = string.Format("{0}/api/v1/getManufacturer?apiKey={1}&apiCredential={2}", iden.API_url, iden.API_key, iden.API_credential);

            DatabaseSQL EDB = new DatabaseSQL(iden.DatabasePathErasoft);
            var resultExecDefault = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST = '" + iden.no_cust + "'");


            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            try
            {
                using (WebResponse response = myReq.GetResponse())
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
                if (ex.Message.ToString().Contains("404"))
                {
                    contextNotif.Clients.Group(iden.DatabasePathErasoft).notifTransaction("Data akun marketplace (82Cart) tidak valid. Mohon periksa kembali data Anda dengan benar.", false);
                }
                else if (ex.Message.ToString().Contains("402") && ex.Message.ToString().ToLower().Contains("payment"))
                {
                    contextNotif.Clients.Group(iden.DatabasePathErasoft).notifTransaction("Masa trial akun marketplace (82Cart) sudah habis.", false);
                }
                else
                {
                    contextNotif.Clients.Group(iden.DatabasePathErasoft).notifTransaction("Info: " + ex.Message.ToString(), false);
                }
            }

            if (!string.IsNullOrEmpty(responseServer))
            {
                var resultApi = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(E2CartCustomerResult)) as E2CartCustomerResult;
                if (resultApi != null)
                {
                    //if (resultApi.error == "none" && resultApi.data.Length > 0)
                    if (resultApi.error == "none")
                    {

                        var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '1' WHERE CUST = '" + iden.no_cust + "'");
                        if (resultquery == 1)
                        {
                            contextNotif.Clients.Group(iden.DatabasePathErasoft).notifTransaction("Akun marketplace " + iden.email.ToString() + " (82Cart) berhasil aktif", true);
                            ////currentLog.REQUEST_RESULT = "Update Status API Complete";
                            ////manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, dataAPI, currentLog);
                        }
                        else
                        {
                            contextNotif.Clients.Group(iden.DatabasePathErasoft).notifTransaction("Akun marketplace (82Cart) gagal diaktifkan", true);
                            ////currentLog.REQUEST_RESULT = "Update Status API Failed";
                            ////currentLog.REQUEST_EXCEPTION = "Failed Update Table";
                            ////manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, dataAPI, currentLog);
                        }
                        ////foreach (var data in resultApi.data)
                        ////{
                        ////    if (data.email == iden.email)
                        ////    {
                        ////        //var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '1', AL = '" + result.shop.address1 + "', Sort1_Cust = '" + result.shop.id + "', TLP = '" + result.shop.phone + "' WHERE CUST = '" + dataAPI.no_cust + "'");
                        ////        var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '1' WHERE CUST = '" + iden.no_cust + "'");
                        ////        if (resultquery == 1)
                        ////        {
                        ////            contextNotif.Clients.Group(iden.DatabasePathErasoft).notifTransaction("Akun marketplace " + iden.email.ToString() + " (82Cart) berhasil aktif", true);
                        ////            //currentLog.REQUEST_RESULT = "Update Status API Complete";
                        ////            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, dataAPI, currentLog);
                        ////        }
                        ////        else
                        ////        {
                        ////            contextNotif.Clients.Group(iden.DatabasePathErasoft).notifTransaction("Akun marketplace (82Cart) gagal diaktifkan", true);
                        ////            //currentLog.REQUEST_RESULT = "Update Status API Failed";
                        ////            //currentLog.REQUEST_EXCEPTION = "Failed Update Table";
                        ////            //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, dataAPI, currentLog);
                        ////        }
                        ////    }
                        ////}
                        //
                    }
                    else
                    {
                        contextNotif.Clients.Group(iden.DatabasePathErasoft).notifTransaction("Akun marketplace (82Cart) gagal diaktifkan", true);
                        //error api response
                    }
                }
                else
                {
                    //error api response
                }


                //ViewBag.Customer = lCust.data;
            }

            return ret;

        }

        //Get All Orders.
        public ActionResult E2Cart_GetOrders(E2CartAPIData iden)
        {

            string urll = string.Format("{0}/api/v1/getOrder?apiKey={1}&apiCredential={2}", iden.API_url, iden.API_key, iden.API_credential);


            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            try
            {
                using (WebResponse response = myReq.GetResponse())
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
                var lOrder = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(E2CartOrderResult)) as E2CartOrderResult;

                ViewBag.Order = lOrder.data;
            }

            return View();
        }

        //Get All Orders State.
        public ActionResult E2Cart_GetOrdersState(E2CartAPIData iden)
        {

            string urll = string.Format("{0}/api/v1/getOrderStates?apiKey={1}&apiCredential={2}", iden.API_url, iden.API_key, iden.API_credential);


            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            try
            {
                using (WebResponse response = myReq.GetResponse())
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
                var lOrderState = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(E2CartOrderStateResult)) as E2CartOrderStateResult;

                ViewBag.OrderState = lOrderState.data;
            }

            return View();
        }

        //[AutomaticRetry(Attempts = 2)]
        //[Queue("3_general")]
        public async Task<string> E2Cart_GetOrderByStatusCompleted(E2CartAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            var dateFrom = DateTimeOffset.UtcNow.AddDays(-10).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            var dateTo = DateTimeOffset.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

            string urll = string.Format("{0}/api/v1/getOrder?apiKey={1}&apiCredential={2}&date_add_from={3}&date_add_to={4}", iden.API_url, iden.API_key, iden.API_credential, dateFrom, dateTo);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            try
            {
                using (WebResponse response = myReq.GetResponse())
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


            if (responseServer != null)
            {
                //try
                //{
                var listOrder = JsonConvert.DeserializeObject(responseServer, typeof(E2CartOrderResult)) as E2CartOrderResult;

                var statusCompleted = "5";
                var orderFilterCompleted = listOrder.data.Where(p => p.current_state == statusCompleted).ToList();
                var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();
                string ordersn = "";
                jmlhNewOrder = 0;
                foreach (var item in orderFilterCompleted)
                {
                    if (OrderNoInDb.Contains(item.id_order))
                    {
                        ordersn = ordersn + "'" + item.id_order + "',";
                    }
                }
                if (orderFilterCompleted.Count() > 0 && !string.IsNullOrEmpty(ordersn))
                {
                    ordersn = ordersn.Substring(0, ordersn.Length - 1);
                    var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '03'");
                    jmlhNewOrder = jmlhNewOrder + rowAffected;
                    if (jmlhNewOrder > 0)
                    {
                        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhNewOrder) + " Pesanan dari 82Cart sudah selesai.");
                    }
                }
            }
            return ret;
        }


        //[AutomaticRetry(Attempts = 2)]
        //[Queue("3_general")]
        public async Task<string> E2Cart_GetOrderByStatus(E2CartAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar)
        {
            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            var dateFrom = DateTimeOffset.UtcNow.AddDays(-10).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            var dateTo = DateTimeOffset.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

            string urll = string.Format("{0}/api/v1/getOrder?apiKey={1}&apiCredential={2}&date_add_from={3}&date_add_to={4}", iden.API_url, iden.API_key, iden.API_credential, dateFrom, dateTo);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            //try
            //{
            using (WebResponse response = myReq.GetResponse())
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

                string[] statusAwaiting = { "1", "3", "10", "11", "13", "14", "16", "17", "18", "19", "20", "21", "23", "25" };
                string[] ordersn_list = listOrder.data.Select(p => p.id_order).ToArray();
                var dariTgl = DateTimeOffset.UtcNow.AddDays(-10).DateTime;
                jmlhNewOrder = 0;
                var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();

                if (stat == StatusOrder.UNPAID)
                {
                    for (int itemOrder = 0; itemOrder < statusAwaiting.Length; itemOrder++)
                    {
                        var orderFilter = listOrder.data.Where(p => p.current_state == statusAwaiting[itemOrder]).ToList();
                        if (orderFilter.Count() > 0)
                        {
                            foreach (var order in orderFilter)
                            {
                                try
                                {
                                    if (!OrderNoInDb.Contains(order.id_order))
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
                                        string nama = fullname.Length > 30 ? order.firstname.Substring(0, 30) : order.lastname.ToString();

                                        insertPembeli += string.Format("('{0}','{1}','{2}','{3}',0,0,'0','01',1, 'IDR', '01', '{4}', 0, 0, 0, 0, '1', 0, 0,'FP', '{5}', '{6}', '{7}', '', '{8}', '{9}', '', '','{10}'),",
                                            ((nama ?? "").Replace("'", "`")),
                                            ((order.delivery_address[0].address1 ?? "").Replace("'", "`")),
                                            ((order.delivery_address[0].phone_mobile ?? "").Replace("'", "`")),
                                            (NAMA_CUST.Replace(',', '.')),
                                            ((order.delivery_address[0].address1 + " " + order.delivery_address[0].address2 ?? "").Replace("'", "`")),
                                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                            (username),
                                            ((order.delivery_address[0].postcode ?? "").Replace("'", "`")),
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


                                        TEMP_82CART_ORDERS newOrder = new TEMP_82CART_ORDERS()
                                        {
                                            actual_shipping_cost = order.total_shipping,
                                            buyer_username = order.firstname + " " + order.lastname,
                                            cod = false,
                                            country = "",
                                            create_time = Convert.ToDateTime(dateOrder),
                                            currency = order.currency,
                                            days_to_ship = 0,
                                            dropshipper = "",
                                            escrow_amount = "",
                                            estimated_shipping_fee = order.total_shipping,
                                            goods_to_declare = false,
                                            message_to_seller = "",
                                            note = "",
                                            note_update_time = Convert.ToDateTime(dateOrder),
                                            ordersn = order.id_order,
                                            //order_status = order.current_state_name,
                                            order_status = "UNPAID",
                                            payment_method = order.payment,
                                            //change by nurul 5/12/2019, local time 
                                            //pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime,
                                            pay_time = Convert.ToDateTime(datePay),
                                            //end change by nurul 5/12/2019, local time 
                                            Recipient_Address_country = order.delivery_address[0].id_country ?? "ID",
                                            Recipient_Address_state = order.delivery_address[0].id_state ?? "",
                                            Recipient_Address_city = order.delivery_address[0].city ?? "",
                                            Recipient_Address_town = "",
                                            Recipient_Address_district = "",
                                            Recipient_Address_full_address = order.delivery_address[0].address1 ?? "",
                                            Recipient_Address_name = order.firstname + " " + order.lastname ?? "",
                                            Recipient_Address_phone = order.delivery_address[0].phone_mobile ?? order.delivery_address[0].phone ?? "",
                                            Recipient_Address_zipcode = order.delivery_address[0].postcode,
                                            service_code = order.id_carrier,
                                            shipping_carrier = order.name_carrier,
                                            total_amount = order.total_paid,
                                            tracking_no = order.shipping_number ?? "",
                                            update_time = Convert.ToDateTime(dateOrder),
                                            CONN_ID = connID,
                                            CUST = CUST,
                                            NAMA_CUST = NAMA_CUST
                                        };

                                        foreach (var item in order.order_detail)
                                        {
                                            //var product_id = "";
                                            //var name_brg = "";
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
                                                name_brg_variasi = item.product_name;
                                            }

                                            TEMP_82CART_ORDERS_ITEM newOrderItem = new TEMP_82CART_ORDERS_ITEM()
                                            {
                                                ordersn = order.id_order,
                                                is_wholesale = false,
                                                //item_id = Convert.ToInt32(product_id),
                                                item_id = item.product_id,
                                                item_name = item.product_name,
                                                item_sku = item.reference,
                                                variation_discounted_price = item.original_product_price,
                                                //variation_id = Convert.ToInt32(item.product_attribute_id),
                                                variation_id = item.product_attribute_id,
                                                variation_name = name_brg_variasi,
                                                variation_original_price = item.original_product_price,
                                                variation_quantity_purchased = Convert.ToInt32(item.product_quantity),
                                                variation_sku = item.reference,
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

                    if (jmlhNewOrder > 0)
                    {
                        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari 82Cart.");
                        new StokControllerJob().updateStockMarketPlace(connID, iden.DatabasePathErasoft, iden.username);
                    }
                }

                if (stat == StatusOrder.PAID)
                {
                    string[] statusCAP = { "2", "15" };
                    string ordersn = "";
                    jmlhPesananDibayar = 0;

                    for (int itemOrderExisting = 0; itemOrderExisting < statusCAP.Length; itemOrderExisting++)
                    {
                        var orderFilterExisting = listOrder.data.Where(p => p.current_state == statusCAP[itemOrderExisting]).ToList();
                        foreach (var item in orderFilterExisting)
                        {
                            if (OrderNoInDb.Contains(item.id_order))
                            {
                                ordersn = ordersn + "'" + item.id_order + "',";
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(ordersn))
                    {
                        ordersn = ordersn.Substring(0, ordersn.Length - 1);
                        var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '0'");
                        if (rowAffected > 0)
                        {
                            jmlhPesananDibayar++;
                        }
                    }

                    if (jmlhPesananDibayar > 0)
                    {
                        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhPesananDibayar) + " Pesanan terbayar dari 82Cart.");
                    }
                }
            }
            return ret;
        }

        //[AutomaticRetry(Attempts = 2)]
        //[Queue("3_general")]
        public async Task<string> E2Cart_GetOrderByStatusCancelled(E2CartAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            var dateFrom = DateTimeOffset.UtcNow.AddDays(-10).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
            var dateTo = DateTimeOffset.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");

            //DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = string.Format("{0}/api/v1/getOrder?apiKey={1}&apiCredential={2}&date_add_from={3}&date_add_to={4}", iden.API_url, iden.API_key, iden.API_credential, dateFrom, dateTo);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.ContentType = "application/json";
            string responseServer = "";

            using (WebResponse response = myReq.GetResponse())
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

                var statusCancel = "6";
                var orderFilterCancel = listOrder.data.Where(p => p.current_state == statusCancel).ToList();
                var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST).Select(p => p.NO_REFERENSI).ToList();
                string ordersn = "";
                jmlhNewOrder = 0;
                foreach (var item in orderFilterCancel)
                {
                    if (OrderNoInDb.Contains(item.id_order))
                    {
                        ordersn = ordersn + "'" + item.id_order + "',";
                    }
                }
                if (orderFilterCancel.Count() > 0 && !string.IsNullOrEmpty(ordersn))
                {
                    ordersn = ordersn.Substring(0, ordersn.Length - 1);
                    var brgAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connID + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI <> '11' AND BRG <> 'NOT_FOUND'");
                    var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2', STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI <> '11'");
                    if (rowAffected > 0)
                    {
                        //add by Tri 4 Des 2019, isi cancel reason
                        var sSQL = "";
                        var sSQL2 = "SELECT * INTO #TEMP FROM (";
                        var listReason = new Dictionary<string, string>();

                        foreach (var order in listOrder.data)
                        {
                            string reasonValue;
                            if (listReason.TryGetValue(order.id_order, out reasonValue))
                            {
                                if (!string.IsNullOrEmpty(sSQL))
                                {
                                    sSQL += " UNION ALL ";
                                }
                                sSQL += " SELECT '" + order.id_order + "' NO_REFERENSI, '" + listReason[order.id_order] + "' ALASAN ";
                            }
                        }
                        sSQL2 += sSQL + ") as qry; INSERT INTO SOT01D (NO_BUKTI, CATATAN_1, USERNAME) ";
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

                        var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN (" + ordersn + ") AND STATUS <> '2' AND ST_POSTING = 'T'");

                        new StokControllerJob().updateStockMarketPlace(connID, iden.DatabasePathErasoft, iden.username);
                    }
                    jmlhNewOrder = jmlhNewOrder + rowAffected;
                    //if (listOrder.more)
                    //{
                    //    await GetOrderByStatusCancelled(iden, stat, CUST, NAMA_CUST, page + 50, jmlhNewOrder);
                    //}
                    //else
                    //{
                    if (jmlhNewOrder > 0)
                    {
                        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhNewOrder) + " Pesanan dari 82Cart dibatalkan.");
                    }
                    //}
                }

                //}
                //catch (Exception ex2)
                //{
                //}
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
            public string API_credential { get; set; }
        }

        public class E2CartProductListResult
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public E2CartProduct[] data { get; set; }
        }

        public class E2CartProductCategoryResult
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public E2CartProductCategory[] data { get; set; }
        }

        public class E2CartManufactureResult
        {
            public string requestid { get; set; }
            public object error { get; set; }
            public E2CartProductManufacture[] data { get; set; }
        }

        public class E2CartInventoryResult
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public E2CartInventory[] data { get; set; }
        }

        public class E2CartAttributeResult
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public E2CartAttribute[] data { get; set; }
        }

        public class E2CartAttribute
        {
            public string id_attribute_group { get; set; }
            public string is_color_group { get; set; }
            public string group_type { get; set; }
            public string group_position { get; set; }
            public string group_name { get; set; }
            public attribute[] attribute { get; set; }
        }

        public class E2CartCustomerResult
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public E2CartCustomer[] data { get; set; }
        }

        public class E2CartCustomer
        {
            public string id_customer { get; set; }
            public string id_shop_group { get; set; }
            public string id_shop { get; set; }
            public string id_gender { get; set; }
            public string id_default_group { get; set; }
            public string id_lang { get; set; }
            public string id_risk { get; set; }
            public string company { get; set; }
            public string siret { get; set; }
            public string ape { get; set; }
            public string firstname { get; set; }
            public string lastname { get; set; }
            public string email { get; set; }
            public string birthday { get; set; }
            public string active { get; set; }
            public string is_guest { get; set; }
            public string date_add { get; set; }
            public string date_upd { get; set; }
            public address[] address { get; set; }
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

        public class attribute
        {
            public string id_attribute { get; set; }
            public string color { get; set; }
            public string attribute_position { get; set; }
            public string attribute_name { get; set; }
        }

        public class E2CartInventory
        {
            public string id_product { get; set; }
            public string id_product_attribute { get; set; }
            public string referrence { get; set; }
            public string name { get; set; }
            public string quantity { get; set; }
            public E2CartInventoryAtt[] dataArray { get; set; }
        }

        public class E2CartInventoryAtt
        {
            public string id_product_attribute { get; set; }
            public string referrence { get; set; }
            public string name { get; set; }
            public string quantity { get; set; }
        }

        public class E2CartProductManufacture
        {
            public string id_manufacturer { get; set; }
            public string name { get; set; }
            public string date_add { get; set; }
            public string date_upd { get; set; }
            public string active { get; set; }
            public string description { get; set; }
            public string short_description { get; set; }
            public string meta_title { get; set; }
            public string meta_description { get; set; }
            public string meta_keywords { get; set; }
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
        
        public class Image_Product
        {
            public string id_image { get; set; }
            public string link_image { get; set; }
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

        public class Attribute_Image
        {
            public int id_image { get; set; }
            public string image_url { get; set; }
        }


        public class E2CartProductPost
        {
            public string apiKey { get; set; }
            public string apiCredential { get; set; }
            public string id_product { get; set; }
            public string name { get; set; }
            public string reference { get; set; }
            public string ean13 { get; set; }
            public string upc { get; set; }
            public string active { get; set; }
            public string visibility { get; set; }
            public string available_for_order { get; set; }
            public string show_price { get; set; }
            public string online_only { get; set; }
            public string condition { get; set; }
            public string description_short { get; set; }
            public string short_description { get; set; }
            public string description { get; set; }
            public string meta_keyword { get; set; }
            public string meta_keywords { get; set; }
            public float wholesale_price { get; set; }
            public float price { get; set; }
            public string on_sale { get; set; }
            public string meta_title { get; set; }
            public string meta_description { get; set; }
            public string link_rewrite { get; set; }
            public string id_category_default { get; set; }
            public string category_default { get; set; }
            public string id_manufacturer { get; set; }
            public string manufacturer { get; set; }
            public float width { get; set; }
            public float height { get; set; }
            public float depth { get; set; }
            public float weight { get; set; }
            public float additional_shipping_cost { get; set; }
            public int quantity { get; set; }
            public int minimal_quantity { get; set; }
            public int out_of_stock { get; set; }
            public string indexed { get; set; }
            public string date_add { get; set; }
            public string date_upd { get; set; }
            public string category { get; set; }
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


        public class ResultUpdateStock82Cart
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public string results { get; set; }
            public ResultUpdateStockData[] data { get; set; }
        }

        public class ResultUpdateStockData
        {
            public string id_stock_available { get; set; }
            public string id_product { get; set; }
            public string id_product_attribute { get; set; }
            public string id_shop { get; set; }
            public string id_shop_group { get; set; }
            public string quantity { get; set; }
            public string depends_on_stock { get; set; }
            public string out_of_stock { get; set; }
        }


        public class ResultAddCategory82Cart
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public string results { get; set; }
            public ResultAddCategory82CartData data { get; set; }
        }

        public class ResultAddCategory82CartData
        {
            public int id { get; set; }
            public string id_category { get; set; }
            public object[] name { get; set; }
            public string active { get; set; }
            public string position { get; set; }
            public object[] description { get; set; }
            public string id_parent { get; set; }
            public object id_category_default { get; set; }
            public int level_depth { get; set; }
            public string nleft { get; set; }
            public string nright { get; set; }
            public object[] link_rewrite { get; set; }
            public object[] meta_title { get; set; }
            public object[] meta_keywords { get; set; }
            public object[] meta_description { get; set; }
            public string date_add { get; set; }
            public string date_upd { get; set; }
            public string is_root_category { get; set; }
            public string id_shop_default { get; set; }
            public object groupBox { get; set; }
            public bool id_image { get; set; }
            public object id_shop_list { get; set; }
            public bool force_id { get; set; }
        }


        public class ResultAddManufacturer
        {
            public string requestid { get; set; }
            public string error { get; set; }
            public string results { get; set; }
            public ResultAddManufacturerData data { get; set; }
        }

        public class ResultAddManufacturerData
        {
            public int id { get; set; }
            public string id_manufacturer { get; set; }
            public string name { get; set; }
            public object[] description { get; set; }
            public object[] short_description { get; set; }
            public object id_address { get; set; }
            public string date_add { get; set; }
            public string date_upd { get; set; }
            public string link_rewrite { get; set; }
            public object[] meta_title { get; set; }
            public object[] meta_keywords { get; set; }
            public object[] meta_description { get; set; }
            public string active { get; set; }
            public object id_shop_list { get; set; }
            public bool force_id { get; set; }
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


    }
}