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
        protected string SetupContext(TokopediaAPIData data)
        {
            string ret = "";
            MoDbContext = new MoDbContext();
            ErasoftDbContext = new ErasoftContext(data.DatabasePathErasoft);
            EDB = new DatabaseSQL(data.DatabasePathErasoft);
            username = data.username;
            var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.RecNum == data.idmarket).SingleOrDefault();
            if (arf01inDB != null)
            {
                ret = arf01inDB.TOKEN;
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Tokopedia Berhasil. Link Produk Gagal.")]
        public async Task<string> CreateProductGetStatus(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, string brg, int upload_id, string log_request_id)
        {
            //if merchant code diisi. barulah GetOrderList
            string ret = "";
            var token = SetupContext(iden);
            iden.token = token;
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/create/status?shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&upload_id=" + Uri.EscapeDataString(Convert.ToString(upload_id));

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

            if (responseFromServer != "")
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(CreateProductGetStatusResult)) as CreateProductGetStatusResult;
                if (result.header.error_code == 0)
                {
                    if (result.data.upload_data.Count() > 0)
                    {
                        foreach (var item in result.data.upload_data)
                        {
                            if (item.unprocessed_rows > 0)
                            {
                                var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = 'PENDING;" + Convert.ToString(upload_id) + ";" + Convert.ToString(log_request_id) + "' WHERE BRG = '" + Convert.ToString(brg) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                                if (success == 1)
                                {
                                    manageAPI_LOG_MARKETPLACE(api_status.RePending, ErasoftDbContext, iden, currentLog);
                                }
                            }
                            else if (item.success_rows > 0)
                            {
                                //change by calvin 9 juni 2019
                                //await GetActiveItemListBySKU(iden, 0, 50, iden.idmarket, brg);
                                string EDBConnID = EDB.GetConnectionString("ConnId");
                                var sqlStorage = new SqlServerStorage(EDBConnID);

                                var Jobclient = new BackgroundJobClient(sqlStorage);
                                Jobclient.Enqueue<TokopediaControllerJob>(x => x.GetActiveItemListBySKU(iden.DatabasePathErasoft, brg, log_CUST, "Barang", "Link Produk (Tahap 2 / 2)", iden, 0, 50, iden.idmarket, brg, currentLog.REQUEST_ID));
                                //end change by calvin 9 juni 2019
                                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            }
                            else if (item.failed_rows > 0)
                            {
                                foreach (var item_failed in item.failed_rows_data)
                                {
                                    currentLog.REQUEST_RESULT = item_failed.error;
                                    currentLog.REQUEST_EXCEPTION = item_failed.error;
                                    var failed = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '' WHERE BRG = '" + Convert.ToString(brg) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                                }
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                                throw new Exception(currentLog.REQUEST_RESULT + ";" + currentLog.REQUEST_EXCEPTION);
                            }
                        }
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

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Edit Product {obj} ke Tokopedia Gagal.")]
        public async Task<string> EditProductGetStatus(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, string brg, int upload_id, string log_request_id, string product_id)
        {
            //if merchant code diisi. barulah GetOrderList
            string ret = "";
            long milis = CurrentTimeMillis();

            var token = SetupContext(iden);
            iden.token = token;

            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/edit/status?shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&upload_id=" + Uri.EscapeDataString(Convert.ToString(upload_id));

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

            if (responseFromServer != "")
            {

                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(CreateProductGetStatusResult)) as CreateProductGetStatusResult;
                if (result.header.error_code == 0)
                {
                    if (result.data.upload_data.Count() > 0)
                    {
                        foreach (var item in result.data.upload_data)
                        {
                            if (item.unprocessed_rows > 0)
                            {
                                var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = 'PEDITENDING;" + Convert.ToString(upload_id) + ";" + Convert.ToString(log_request_id) + ";" + Convert.ToString(product_id) + "' WHERE BRG = '" + Convert.ToString(brg) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                                if (success == 1)
                                {
                                    manageAPI_LOG_MARKETPLACE(api_status.RePending, ErasoftDbContext, iden, currentLog);
                                }
                            }
                            else if (item.success_rows > 0)
                            {
                                //change by calvin 9 juni 2019
                                //await GetActiveItemVariantByProductID(iden, brg, iden.idmarket, Convert.ToString(product_id));
                                string EDBConnID = EDB.GetConnectionString("ConnId");
                                var sqlStorage = new SqlServerStorage(EDBConnID);

                                var Jobclient = new BackgroundJobClient(sqlStorage);
                                Jobclient.Enqueue<TokopediaControllerJob>(x => x.GetActiveItemListBySKU(iden.DatabasePathErasoft, brg, log_CUST, "Barang", "Link Produk (Tahap 2 / 2)", iden, 0, 50, iden.idmarket, brg, currentLog.REQUEST_ID));
                                //end change by calvin 9 juni 2019

                                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            }
                            else if (item.failed_rows > 0)
                            {
                                foreach (var item_failed in item.failed_rows_data)
                                {
                                    currentLog.REQUEST_RESULT = item_failed.error;
                                    currentLog.REQUEST_EXCEPTION = item_failed.error;
                                    var failed = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = 'PEDITFAILED;" + Convert.ToString(upload_id) + ";" + Convert.ToString(log_request_id) + ";" + Convert.ToString(product_id) + "' WHERE BRG = '" + Convert.ToString(brg) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                                }
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                                throw new Exception(currentLog.REQUEST_RESULT + ";" + currentLog.REQUEST_EXCEPTION);
                            }
                        }
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

            return ret;
        }

        public class CreateProductGetStatusResult
        {
            public CreateProductGetStatusResultHeader header { get; set; }
            public CreateProductGetStatusResultData data { get; set; }
        }

        public class CreateProductGetStatusResultHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            public int error_code { get; set; }
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
            public int processed { get; set; }
        }

        public class CreateProductGetStatusResultFailed_Rows_Data
        {
            public string product_name { get; set; }
            public int product_price { get; set; }
            public string sku { get; set; }
            public string error { get; set; }
        }

        [AutomaticRetry(Attempts = 2)]
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
                string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/edit?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);


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
                    product_id = Convert.ToInt32(product_id),
                    name = Convert.ToString(brg_stf02.NAMA + " " + brg_stf02.NAMA2).Trim(),
                    //category_id = Convert.ToInt32(brg_stf02h.CATEGORY_CODE),
                    category_id = null,
                    price = Convert.ToInt32(brg_stf02h.HJUAL),
                    status = 1,
                    minimum_order = 1,
                    weight = Convert.ToInt32(brg_stf02.BERAT),
                    weight_unit = 1,
                    condition = 1,
                    description = brg_stf02.Deskripsi,
                    must_insurance = false,
                    returnable = false,
                    //sku = brg_stf02.BRG,
                    stock = 1, //1 - 10000.Stock should be 1 if want to add variant product. 0 indicates always availabl
                    product_wholesale_price = null,
                    product_preorder = null,
                    product_video = null,
                    images = new List<CreateProduct_Images>()
                };

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
                    etalase_id = etalase_id,
                    etalase_name = ""
                };

                if (brg_stf02.TYPE == "4") // punya variasi
                {
                    CreateProduct_Product_Variant product_variant = new CreateProduct_Product_Variant()
                    {
                        product_variant = new List<CreateProduct_Product_Variant1>(),
                        variant = new List<CreateProduct_Variant>()
                    };
                    var AttributeOptTokped = MoDbContext.AttributeOptTokped.ToList();
                    var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == brg).ToList();
                    var var_strukturVar = ErasoftDbContext.STF02I.Where(p => p.BRG == brg && p.MARKET == "TOKPED").ToList().OrderBy(p => p.RECNUM);
                    var var_stf02_brg_list = var_stf02.Select(p => p.BRG).ToList();
                    var var_stf02h = ErasoftDbContext.STF02H.Where(p => var_stf02_brg_list.Contains(p.BRG) && p.IDMARKET == iden.idmarket).ToList();

                    string category_mo = var_strukturVar.Select(p => p.CATEGORY_MO).FirstOrDefault();
                    var var_stf20 = ErasoftDbContext.STF20B.Where(p => p.CATEGORY_MO == category_mo).ToList();

                    #region variant lv 1
                    if (var_strukturVar.Where(p => p.LEVEL_VAR == 1).Count() > 0)
                    {
                        int variant_id = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_JUDUL_VAR);
                        int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                        int unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                        CreateProduct_Variant newVariasi = new CreateProduct_Variant()
                        {
                            v = variant_id,
                            vu = unit_id,
                            pos = 1,
                            opt = new List<CreateProduct_Opt>()
                        };

                        foreach (var fe_record in var_strukturVar.Where(p => p.LEVEL_VAR == 1))
                        {
                            #region cek duplikat variant_id, unit_id, value_id
                            bool add = true;
                            if (product_variant.variant.Count > 0)
                            {
                                foreach (var variant in product_variant.variant.Where(p => p.v == variant_id && p.vu == unit_id))
                                {
                                    var added_value_id = variant.opt.Select(p => p.vuv).ToList();
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
                                CreateProduct_Image gambarVariant = new CreateProduct_Image()
                                {
                                    file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                    file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                    x = 128,
                                    y = 128
                                };
                                CreateProduct_Opt newOpt = new CreateProduct_Opt()
                                {
                                    vuv = Convert.ToInt32(fe_record.MP_VALUE_VAR),
                                    t_id = fe_record.RECNUM,
                                    cstm = var_stf20.Where(p => p.LEVEL_VAR == fe_record.LEVEL_VAR && p.KODE_VAR == fe_record.KODE_VAR).FirstOrDefault()?.KET_VAR,
                                    image = new List<CreateProduct_Image>()
                                };
                                newOpt.image.Add(gambarVariant);

                                newVariasi.opt.Add(newOpt);

                                if (newDataProduct.images.Count() == 0)
                                {
                                    newDataProduct.images.Add(new CreateProduct_Images()
                                    {
                                        image_file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                        image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                        image_description = ""
                                    });
                                }
                            }
                        }
                        product_variant.variant.Add(newVariasi);
                    }
                    #endregion

                    #region variant lv 2
                    if (var_strukturVar.Where(p => p.LEVEL_VAR == 2).Count() > 0)
                    {
                        int variant_id = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_JUDUL_VAR);
                        int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                        int unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                        CreateProduct_Variant newVariasi = new CreateProduct_Variant()
                        {
                            v = variant_id,
                            vu = unit_id,
                            pos = 2,
                            opt = new List<CreateProduct_Opt>()
                        };

                        foreach (var fe_record in var_strukturVar.Where(p => p.LEVEL_VAR == 2))
                        {
                            #region cek duplikat variant_id, unit_id, value_id
                            bool add = true;
                            if (product_variant.variant.Count > 0)
                            {
                                foreach (var variant in product_variant.variant.Where(p => p.v == variant_id && p.vu == unit_id))
                                {
                                    var added_value_id = variant.opt.Select(p => p.vuv).ToList();
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
                                CreateProduct_Image gambarVariant = new CreateProduct_Image()
                                {
                                    file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                    file_path = var_stf02.Where(p => p.Sort9 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                    x = 128,
                                    y = 128
                                };
                                CreateProduct_Opt newOpt = new CreateProduct_Opt()
                                {
                                    vuv = Convert.ToInt32(fe_record.MP_VALUE_VAR),
                                    t_id = fe_record.RECNUM,
                                    cstm = var_stf20.Where(p => p.LEVEL_VAR == fe_record.LEVEL_VAR && p.KODE_VAR == fe_record.KODE_VAR).FirstOrDefault()?.KET_VAR,
                                    image = new List<CreateProduct_Image>()
                                };
                                newOpt.image.Add(gambarVariant);

                                newVariasi.opt.Add(newOpt);

                                if (newDataProduct.images.Count() == 0)
                                {
                                    newDataProduct.images.Add(new CreateProduct_Images()
                                    {
                                        image_file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                        image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                        image_description = ""
                                    });
                                }
                            }
                        }
                        product_variant.variant.Add(newVariasi);
                    }
                    #endregion

                    //product variasi
                    foreach (var item_var in var_stf02)
                    {
                        var price_var = var_stf02h.Where(p => p.BRG == item_var.BRG).FirstOrDefault();
                        CreateProduct_Product_Variant1 newProductVariasi = new CreateProduct_Product_Variant1()
                        {
                            st = 1,
                            stock = 1,
                            price_var = (float)item_var.HJUAL,
                            sku = item_var.BRG,
                            opt = new List<int>()
                        };
                        if (!string.IsNullOrWhiteSpace(item_var.Sort8))
                        {
                            var recnumVariasi = var_strukturVar.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item_var.Sort8).FirstOrDefault();
                            if (recnumVariasi != null)
                            {
                                newProductVariasi.opt.Add(Convert.ToInt32(recnumVariasi.RECNUM));
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(item_var.Sort9))
                        {
                            var recnumVariasi = var_strukturVar.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == item_var.Sort9).FirstOrDefault();
                            if (recnumVariasi != null)
                            {
                                newProductVariasi.opt.Add(Convert.ToInt32(recnumVariasi.RECNUM));
                            }
                        }
                        product_variant.product_variant.Add(newProductVariasi);
                    }

                    newDataProduct.product_variant = product_variant;
                }
                else if (brg_stf02.TYPE == "3")
                {
                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_1))
                    {
                        newDataProduct.images.Add(new CreateProduct_Images()
                        {
                            image_description = "",
                            image_file_name = "",
                            image_file_path = brg_stf02.LINK_GAMBAR_1
                        });
                    }

                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_2))
                    {
                        newDataProduct.images.Add(new CreateProduct_Images()
                        {
                            image_description = "",
                            image_file_name = "",
                            image_file_path = brg_stf02.LINK_GAMBAR_2
                        });
                    }

                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_3))
                    {
                        newDataProduct.images.Add(new CreateProduct_Images()
                        {
                            image_description = "",
                            image_file_name = "",
                            image_file_path = brg_stf02.LINK_GAMBAR_3
                        });
                    }
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

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), urll);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", iden.token);
                request.Content = new StringContent(myData, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response;
                response = await client.SendAsync(request);
                responseFromServer = await response.Content.ReadAsStringAsync();

                //client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + iden.token));
                //var content = new StringContent(myData, Encoding.UTF8, "application/json");
                //content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json");
                //HttpResponseMessage clientResponse = await client.PutAsync(
                //    urll, content);

                //using (HttpContent responseContent = clientResponse.Content)
                //{
                //    using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                //    {
                //        responseFromServer = await reader.ReadToEndAsync();
                //    }
                //};
                //}
                //catch (Exception ex)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //}

                if (responseFromServer != "")
                {
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokpedCreateProductResult)) as TokpedCreateProductResult;
                    if (result.header.error_code == 0)
                    {
                        //change by calvin 9 juni 2019
                        //await EditProductGetStatus(iden, brg, result.data.upload_id, currentLog.REQUEST_ID, product_id);
                        string EDBConnID = EDB.GetConnectionString("ConnId");
                        var sqlStorage = new SqlServerStorage(EDBConnID);

                        var Jobclient = new BackgroundJobClient(sqlStorage);
                        Jobclient.Enqueue<TokopediaControllerJob>(x => x.EditProductGetStatus(iden.DatabasePathErasoft, brg, log_CUST, "Barang", "Edit Produk Get Status", iden, brg, result.data.upload_id, currentLog.REQUEST_ID, product_id));
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

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Tokopedia Gagal.")]
        public async Task<string> CreateProduct(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, string brg)
        {
            string ret = "";
            var token = SetupContext(iden);
            iden.token = token;
            var brg_stf02 = ErasoftDbContext.STF02.Where(p => p.BRG == brg).SingleOrDefault();
            if (brg_stf02 != null)
            {
                var brg_stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == brg && p.IDMARKET == iden.idmarket).SingleOrDefault();
                string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/create?shop_id=" + Uri.EscapeDataString(iden.API_secret_key);


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
                    status = 1,
                    minimum_order = 1,
                    weight = Convert.ToInt32(brg_stf02.BERAT),
                    weight_unit = 1,
                    condition = 1,
                    description = brg_stf02.Deskripsi,
                    must_insurance = false,
                    returnable = false,
                    sku = brg_stf02.BRG,
                    stock = 1, //1 - 10000.Stock should be 1 if want to add variant product. 0 indicates always availabl
                    product_wholesale_price = null,
                    product_preorder = null,
                    product_video = null,
                    images = new List<CreateProduct_Images>()
                };

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
                    etalase_id = etalase_id,
                    etalase_name = ""
                };

                if (brg_stf02.TYPE == "4") // punya variasi
                {
                    CreateProduct_Product_Variant product_variant = new CreateProduct_Product_Variant()
                    {
                        product_variant = new List<CreateProduct_Product_Variant1>(),
                        variant = new List<CreateProduct_Variant>()
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
                        int variant_id = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_JUDUL_VAR);
                        int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 1).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                        int unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                        CreateProduct_Variant newVariasi = new CreateProduct_Variant()
                        {
                            v = variant_id,
                            vu = unit_id,
                            pos = 1,
                            opt = new List<CreateProduct_Opt>()
                        };

                        foreach (var fe_record in var_strukturVar.Where(p => p.LEVEL_VAR == 1))
                        {
                            #region cek duplikat variant_id, unit_id, value_id
                            bool add = true;
                            if (product_variant.variant.Count > 0)
                            {
                                foreach (var variant in product_variant.variant.Where(p => p.v == variant_id && p.vu == unit_id))
                                {
                                    var added_value_id = variant.opt.Select(p => p.vuv).ToList();
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
                                CreateProduct_Image gambarVariant = new CreateProduct_Image()
                                {
                                    file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                    file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                    x = 128,
                                    y = 128
                                };
                                CreateProduct_Opt newOpt = new CreateProduct_Opt()
                                {
                                    vuv = Convert.ToInt32(fe_record.MP_VALUE_VAR),
                                    t_id = fe_record.RECNUM,
                                    cstm = var_stf20.Where(p => p.LEVEL_VAR == fe_record.LEVEL_VAR && p.KODE_VAR == fe_record.KODE_VAR).FirstOrDefault()?.KET_VAR,
                                    image = new List<CreateProduct_Image>()
                                };
                                newOpt.image.Add(gambarVariant);

                                newVariasi.opt.Add(newOpt);

                                if (newDataProduct.images.Count() == 0)
                                {
                                    newDataProduct.images.Add(new CreateProduct_Images()
                                    {
                                        image_file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                        image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                        image_description = ""
                                    });
                                }
                            }
                        }
                        product_variant.variant.Add(newVariasi);
                    }
                    #endregion

                    #region variant lv 2
                    if (var_strukturVar.Where(p => p.LEVEL_VAR == 2).Count() > 0)
                    {
                        int variant_id = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_JUDUL_VAR);
                        int first_value = Convert.ToInt32(var_strukturVar.Where(p => p.LEVEL_VAR == 2).FirstOrDefault().MP_VALUE_VAR); // untuk dapatkan unit id
                        int unit_id = AttributeOptTokped.Where(p => p.VARIANT_ID == variant_id && p.VALUE_ID == first_value).FirstOrDefault().UNIT_ID;
                        CreateProduct_Variant newVariasi = new CreateProduct_Variant()
                        {
                            v = variant_id,
                            vu = unit_id,
                            pos = 2,
                            opt = new List<CreateProduct_Opt>()
                        };

                        foreach (var fe_record in var_strukturVar.Where(p => p.LEVEL_VAR == 2))
                        {
                            #region cek duplikat variant_id, unit_id, value_id
                            bool add = true;
                            if (product_variant.variant.Count > 0)
                            {
                                foreach (var variant in product_variant.variant.Where(p => p.v == variant_id && p.vu == unit_id))
                                {
                                    var added_value_id = variant.opt.Select(p => p.vuv).ToList();
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
                                CreateProduct_Image gambarVariant = new CreateProduct_Image()
                                {
                                    file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                    file_path = var_stf02.Where(p => p.Sort9 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                    x = 128,
                                    y = 128
                                };
                                CreateProduct_Opt newOpt = new CreateProduct_Opt()
                                {
                                    vuv = Convert.ToInt32(fe_record.MP_VALUE_VAR),
                                    t_id = fe_record.RECNUM,
                                    cstm = var_stf20.Where(p => p.LEVEL_VAR == fe_record.LEVEL_VAR && p.KODE_VAR == fe_record.KODE_VAR).FirstOrDefault()?.KET_VAR,
                                    image = new List<CreateProduct_Image>()
                                };
                                newOpt.image.Add(gambarVariant);

                                newVariasi.opt.Add(newOpt);

                                if (newDataProduct.images.Count() == 0)
                                {
                                    newDataProduct.images.Add(new CreateProduct_Images()
                                    {
                                        image_file_name = "Image " + Convert.ToString(fe_record.RECNUM),
                                        image_file_path = var_stf02.Where(p => p.Sort8 == fe_record.KODE_VAR).FirstOrDefault().LINK_GAMBAR_1,
                                        image_description = ""
                                    });
                                }
                            }
                        }
                        product_variant.variant.Add(newVariasi);
                    }
                    #endregion

                    //product variasi
                    foreach (var item_var in var_stf02)
                    {
                        var price_var = var_stf02h.Where(p => p.BRG == item_var.BRG).FirstOrDefault();
                        CreateProduct_Product_Variant1 newProductVariasi = new CreateProduct_Product_Variant1()
                        {
                            st = 1,
                            stock = 1,
                            price_var = (float)item_var.HJUAL,
                            sku = item_var.BRG,
                            opt = new List<int>()
                        };
                        if (!string.IsNullOrWhiteSpace(item_var.Sort8))
                        {
                            var recnumVariasi = var_strukturVar.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == item_var.Sort8).FirstOrDefault();
                            if (recnumVariasi != null)
                            {
                                newProductVariasi.opt.Add(Convert.ToInt32(recnumVariasi.RECNUM));
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(item_var.Sort9))
                        {
                            var recnumVariasi = var_strukturVar.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == item_var.Sort9).FirstOrDefault();
                            if (recnumVariasi != null)
                            {
                                newProductVariasi.opt.Add(Convert.ToInt32(recnumVariasi.RECNUM));
                            }
                        }
                        product_variant.product_variant.Add(newProductVariasi);
                    }

                    newDataProduct.product_variant = product_variant;
                }
                else if (brg_stf02.TYPE == "3")
                {
                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_1))
                    {
                        newDataProduct.images.Add(new CreateProduct_Images()
                        {
                            image_description = "",
                            image_file_name = "",
                            image_file_path = brg_stf02.LINK_GAMBAR_1
                        });
                    }

                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_2))
                    {
                        newDataProduct.images.Add(new CreateProduct_Images()
                        {
                            image_description = "",
                            image_file_name = "",
                            image_file_path = brg_stf02.LINK_GAMBAR_2
                        });
                    }

                    if (!string.IsNullOrEmpty(brg_stf02.LINK_GAMBAR_3))
                    {
                        newDataProduct.images.Add(new CreateProduct_Images()
                        {
                            image_description = "",
                            image_file_name = "",
                            image_file_path = brg_stf02.LINK_GAMBAR_3
                        });
                    }
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
                //}
                //catch (Exception ex)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //}

                if (responseFromServer != "")
                {
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokpedCreateProductResult)) as TokpedCreateProductResult;
                    if (result.header.error_code == 0)
                    {
                        //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
                        //change by calvin 9 juni 2019
                        //await CreateProductGetStatus(iden, brg, result.data.upload_id, currentLog.REQUEST_ID);
                        string EDBConnID = EDB.GetConnectionString("ConnId");
                        var sqlStorage = new SqlServerStorage(EDBConnID);

                        var Jobclient = new BackgroundJobClient(sqlStorage);
                        Jobclient.Enqueue<TokopediaControllerJob>(x => x.CreateProductGetStatus(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, "Link Produk (Tahap 1 / 2 )", iden, brg, result.data.upload_id, currentLog.REQUEST_ID));
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
                        //change by calvin 9 juni 2019
                        //Task.Run(() => CreateProductGetStatus(data, item.BRG, Convert.ToInt32(item.BRG_MP.Split(';')[1]), item.BRG_MP.Split(';')[2]).Wait());
                        string EDBConnID = EDB.GetConnectionString("ConnId");
                        var sqlStorage = new SqlServerStorage(EDBConnID);

                        var Jobclient = new BackgroundJobClient(sqlStorage);
                        Jobclient.Enqueue<TokopediaControllerJob>(x => x.CreateProductGetStatus(data.DatabasePathErasoft, item.BRG, arf01inDB.CUST, "Barang", "Link Produk (Tahap 1 / 2 )", data, item.BRG, Convert.ToInt32(item.BRG_MP.Split(';')[1]), Convert.ToString(Convert.ToInt32(item.BRG_MP.Split(';')[2]))));
                        //end change by calvin 9 juni 2019
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

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = milis.ToString(),
            //    REQUEST_ACTION = "Request PIckup",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = "fs : " + iden.merchant_code,
            //    REQUEST_ATTRIBUTE_2 = "orderNo : " + NO_BUKTI_SOT01A,
            //    REQUEST_ATTRIBUTE_3 = "NoRef : " + NO_REFERENSI_SOT01A,
            //    REQUEST_STATUS = "Pending",
            //};

            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            RequestPickup newData = new RequestPickup()
            {
                order_id = Convert.ToInt32(NO_REFERENSI_SOT01A),
                shop_id = Convert.ToInt32(iden.API_secret_key),
                request_time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss +0000 UTC")
            };
            List<RequestPickup> newDataList = new List<RequestPickup>();
            newDataList.Add(newData);
            string myData = JsonConvert.SerializeObject(newDataList.ToArray());

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
            //try
            //{
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
            //}
            //catch (Exception ex)
            //{

            //}

            if (responseFromServer != null)
            {
                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Berhasil Request Pickup Pesanan " + Convert.ToString(namaPemesan) + " ke Tokopedia.");
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

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = milis.ToString(),
            //    REQUEST_ACTION = "Accept Order",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = "fs : " + iden.merchant_code,
            //    REQUEST_ATTRIBUTE_2 = "orderNo : " + ordNo,
            //    REQUEST_ATTRIBUTE_3 = "NoRef : " + splitNoRef[0],
            //    REQUEST_STATUS = "Pending",
            //};

            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
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
            //try
            //{
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
            //}
            //catch (Exception ex)
            //{

            //}

            if (responseFromServer != null)
            {
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

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrderList(TokopediaAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            //if merchant code diisi. barulah GetOrderList
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
                    status = "610";
                    break;
                default:
                    break;
            }
            long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
            string urll = "https://fs.tokopedia.net/v1/order/list?fs_id=" + Uri.EscapeDataString(iden.merchant_code) + "&from_date=" + Convert.ToString(unixTimestampFrom) + "&to_date=" + Convert.ToString(unixTimestampTo) + "&page=" + Convert.ToString(page) + "&per_page=100&shop_id=" + Uri.EscapeDataString(iden.API_secret_key);

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
            int rowCount = 0;
            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                TokopediaOrders result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaOrders)) as TokopediaOrders;
                var orderPaid = result.data.Where(p => p.order_status == 220).ToList();
                var orderAccepted = result.data.Where(p => p.order_status == 400).ToList();
                var orderTokpedInDb = ErasoftDbContext.TEMP_TOKPED_ORDERS.Where(p => p.fs_id == iden.merchant_code);

                var last21days = DateTimeOffset.UtcNow.AddHours(7).AddDays(-21).DateTime;
                System.DateTime datetimeisnull = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST && (p.TGL ?? datetimeisnull) > last21days).Select(p => p.NO_REFERENSI).ToList();

                var connIdARF01C = Guid.NewGuid().ToString();
                rowCount = result.data.Count();
                foreach (var order in orderPaid)
                {
                    if (!OrderNoInDb.Contains(order.order_id + ";" + order.invoice_ref_num))
                    {
                        List<TEMP_TOKPED_ORDERS> ListNewOrders = new List<TEMP_TOKPED_ORDERS>();
                        ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_TOKPED_ORDERS");

                        string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                        insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                        insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                        var kabKot = "3174";
                        var prov = "31";
                        insertPembeli += "('" + order.recipient.name.Replace("'", "`") + "','" + order.recipient.address.address_full.Replace("'", "`") + "','" + order.recipient.phone + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
                        insertPembeli += "1, 'IDR', '01', '" + order.recipient.address.address_full.Replace("'", "`") + "', 0, 0, 0, 0, '1', 0, 0, ";
                        insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.recipient.address.postal_code.Replace("'", "`") + "', '', '" + kabKot + "', '" + prov + "', '', '','" + connIdARF01C + "'),";

                        var order_order_id = Convert.ToString(order.order_id);
                        if (orderTokpedInDb.Where(p => p.order_id == order_order_id).Count() == 0)
                        {
                            //belum ada di temp
                            foreach (var product in order.products)
                            {
                                TEMP_TOKPED_ORDERS newOrder = new TEMP_TOKPED_ORDERS()
                                {
                                    fs_id = order.fs_id,
                                    order_id = Convert.ToString(order.order_id),
                                    accept_partial = order.accept_partial,
                                    invoice_ref_num = order.invoice_ref_num,
                                    product_id = product.id,
                                    product_name = product.name,
                                    product_quantity = product.quantity,
                                    product_notes = product.notes,
                                    product_weight = product.weight,
                                    product_total_weight = product.total_weight,
                                    product_price = product.price,
                                    product_total_price = product.total_price,
                                    product_currency = product.currency,
                                    product_sku = product.sku,
                                    products_fulfilled_product_id = 0,
                                    products_fulfilled_quantity_deliver = 0,
                                    products_fulfilled_quantity_reject = 0,
                                    device_type = order.device_type,
                                    buyer_id = order.buyer.id,
                                    buyer_name = order.buyer.name,
                                    buyer_email = order.buyer.email,
                                    buyer_phone = order.buyer.phone,
                                    shop_id = order.shop_id,
                                    payment_id = order.payment_id,
                                    recipient_name = order.recipient.name,
                                    recipient_address_address_full = order.recipient.address.address_full,
                                    recipient_address_district = order.recipient.address.district,
                                    recipient_address_district_id = order.recipient.address.district_id,
                                    recipient_address_city = order.recipient.address.city,
                                    recipient_address_city_id = order.recipient.address.city_id,
                                    recipient_address_province = order.recipient.address.province,
                                    recipient_address_province_id = order.recipient.address.province_id,
                                    recipient_address_country = order.recipient.address.country,
                                    recipient_address_geo = order.recipient.address.geo,
                                    recipient_address_postal_code = order.recipient.address.postal_code,
                                    recipient_phone = order.recipient.phone,
                                    logistics_shipping_id = order.logistics.shipping_id,
                                    logistics_shipping_agency = order.logistics.shipping_agency,
                                    logistics_service_type = order.logistics.service_type,
                                    amt_ttl_product_price = order.amt.ttl_product_price,
                                    amt_shipping_cost = order.amt.shipping_cost,
                                    amt_insurance_cost = order.amt.insurance_cost,
                                    amt_ttl_amount = order.amt.ttl_amount,
                                    amt_voucher_amount = order.amt.voucher_amount,
                                    amt_toppoints_amount = order.amt.toppoints_amount,
                                    dropshipper_info_name = order.dropshipper_info.name,
                                    dropshipper_info_phone = order.dropshipper_info.phone,
                                    voucher_info_voucher_code = order.voucher_info.voucher_code,
                                    voucher_info_voucher_type = order.voucher_info.voucher_type,
                                    order_status = order.order_status,
                                    create_time = DateTimeOffset.FromUnixTimeSeconds(order.create_time).UtcDateTime,
                                    custom_fields_awb = order.custom_fields.awb,
                                    conn_id = connId,
                                    CUST = CUST,
                                    NAMA_CUST = NAMA_CUST
                                };
                                var product_fulfilled = order.products_fulfilled.SingleOrDefault(p => p.product_id == product.id);
                                if (product_fulfilled != null)
                                {
                                    newOrder.products_fulfilled_product_id = product_fulfilled.product_id;
                                    newOrder.products_fulfilled_quantity_deliver = product_fulfilled.quantity_deliver;
                                    newOrder.products_fulfilled_quantity_reject = product_fulfilled.quantity_reject;
                                }
                                ListNewOrders.Add(newOrder);
                            }
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
                            CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                            EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
                            jmlhNewOrder++;
                        }
                    }
                }

                foreach (var order in orderAccepted)
                {
                    if (!OrderNoInDb.Contains(order.order_id + ";" + order.invoice_ref_num))
                    {
                        List<TEMP_TOKPED_ORDERS> ListNewOrders = new List<TEMP_TOKPED_ORDERS>();

                        ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_TOKPED_ORDERS");

                        string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                        insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                        insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                        var kabKot = "3174";
                        var prov = "31";
                        insertPembeli += "('" + order.recipient.name + "','" + order.recipient.address.address_full + "','" + order.recipient.phone + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
                        insertPembeli += "1, 'IDR', '01', '" + order.recipient.address.address_full + "', 0, 0, 0, 0, '1', 0, 0, ";
                        insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.recipient.address.postal_code + "', '', '" + kabKot + "', '" + prov + "', '', '','" + connIdARF01C + "'),";

                        var order_order_id = Convert.ToString(order.order_id);
                        if (orderTokpedInDb.Where(p => p.order_id == order_order_id).Count() == 0)
                        {
                            //belum ada di temp
                            foreach (var product in order.products)
                            {
                                TEMP_TOKPED_ORDERS newOrder = new TEMP_TOKPED_ORDERS()
                                {
                                    fs_id = order.fs_id,
                                    order_id = Convert.ToString(order.order_id),
                                    accept_partial = order.accept_partial,
                                    invoice_ref_num = order.invoice_ref_num,
                                    product_id = product.id,
                                    product_name = product.name,
                                    product_quantity = product.quantity,
                                    product_notes = product.notes,
                                    product_weight = product.weight,
                                    product_total_weight = product.total_weight,
                                    product_price = product.price,
                                    product_total_price = product.total_price,
                                    product_currency = product.currency,
                                    product_sku = product.sku,
                                    products_fulfilled_product_id = 0,
                                    products_fulfilled_quantity_deliver = 0,
                                    products_fulfilled_quantity_reject = 0,
                                    device_type = order.device_type,
                                    buyer_id = order.buyer.id,
                                    buyer_name = order.buyer.name,
                                    buyer_email = order.buyer.email,
                                    buyer_phone = order.buyer.phone,
                                    shop_id = order.shop_id,
                                    payment_id = order.payment_id,
                                    recipient_name = order.recipient.name,
                                    recipient_address_address_full = order.recipient.address.address_full,
                                    recipient_address_district = order.recipient.address.district,
                                    recipient_address_district_id = order.recipient.address.district_id,
                                    recipient_address_city = order.recipient.address.city,
                                    recipient_address_city_id = order.recipient.address.city_id,
                                    recipient_address_province = order.recipient.address.province,
                                    recipient_address_province_id = order.recipient.address.province_id,
                                    recipient_address_country = order.recipient.address.country,
                                    recipient_address_geo = order.recipient.address.geo,
                                    recipient_address_postal_code = order.recipient.address.postal_code,
                                    recipient_phone = order.recipient.phone,
                                    logistics_shipping_id = order.logistics.shipping_id,
                                    logistics_shipping_agency = order.logistics.shipping_agency,
                                    logistics_service_type = order.logistics.service_type,
                                    amt_ttl_product_price = order.amt.ttl_product_price,
                                    amt_shipping_cost = order.amt.shipping_cost,
                                    amt_insurance_cost = order.amt.insurance_cost,
                                    amt_ttl_amount = order.amt.ttl_amount,
                                    amt_voucher_amount = order.amt.voucher_amount,
                                    amt_toppoints_amount = order.amt.toppoints_amount,
                                    dropshipper_info_name = order.dropshipper_info.name,
                                    dropshipper_info_phone = order.dropshipper_info.phone,
                                    voucher_info_voucher_code = order.voucher_info.voucher_code,
                                    voucher_info_voucher_type = order.voucher_info.voucher_type,
                                    order_status = order.order_status,
                                    create_time = DateTimeOffset.FromUnixTimeSeconds(order.create_time).UtcDateTime,
                                    custom_fields_awb = order.custom_fields.awb,
                                    conn_id = connId,
                                    CUST = CUST,
                                    NAMA_CUST = NAMA_CUST
                                };
                                var product_fulfilled = order.products_fulfilled.SingleOrDefault(p => p.product_id == product.id);
                                if (product_fulfilled != null)
                                {
                                    newOrder.products_fulfilled_product_id = product_fulfilled.product_id;
                                    newOrder.products_fulfilled_quantity_deliver = product_fulfilled.quantity_deliver;
                                    newOrder.products_fulfilled_quantity_reject = product_fulfilled.quantity_reject;
                                }
                                ListNewOrders.Add(newOrder);
                            }
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
                            CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                            EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
                            jmlhNewOrder++;
                        }
                    }
                }

            }
            if (rowCount > 99)
            {
                await GetOrderList(iden, stat, CUST, NAMA_CUST, (page + 1), jmlhNewOrder);
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
        public async Task<string> GetOrderListCompleted(TokopediaAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhOrderComplete)
        {
            //if merchant code diisi. barulah GetOrderList
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
            long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

            ////untuk perbaiki data
            //unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-106).ToUnixTimeSeconds();

            string urll = "https://fs.tokopedia.net/v1/order/list?fs_id=" + Uri.EscapeDataString(iden.merchant_code) + "&from_date=" + Convert.ToString(unixTimestampFrom) + "&to_date=" + Convert.ToString(unixTimestampTo) + "&page=" + Convert.ToString(page) + "&per_page=100&shop_id=" + Uri.EscapeDataString(iden.API_secret_key);

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
            int rowCount = 0;
            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                TokopediaOrders result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(TokopediaOrders)) as TokopediaOrders;
                var orderCompleted = result.data.Where(p => p.order_status == 700).ToList();
                var order701 = result.data.Where(p => p.order_status == 701).ToList(); // order yang dianggap selesai tetapi barang tidak sampai ke buyer

                var connIdARF01C = Guid.NewGuid().ToString();
                rowCount = result.data.Count();

                string ordersn = "";
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
            if (rowCount > 99)
            {
                await GetOrderListCompleted(iden, stat, CUST, NAMA_CUST, (page + 1), jmlhOrderComplete);
            }
            else
            {
                //add by calvin 1 april 2019
                //notify user
                if (jmlhOrderComplete > 0)
                {
                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderComplete) + " Pesanan dari Tokopedia sudah selesai.");
                }
                //end add by calvin 1 april 2019
            }
            return ret;
        }

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

        public async Task<string> UpdateStock(TokopediaAPIData iden, int product_id, int stok)
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

        public async Task<string> UpdatePrice(TokopediaAPIData iden, int product_id, float price)
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
        [NotifyOnFailed("Create Product {obj} ke Tokopedia Berhasil. Link Produk Gagal.")]
        public async Task<BindingBase> GetActiveItemListBySKU(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, TokopediaAPIData iden, int page, int recordCount, int recnumArf01, string SKU, string log_request_id)
        {
            var connId = Guid.NewGuid().ToString();

            var token = SetupContext(iden);
            iden.token = token;
            BindingBase ret = new BindingBase();
            ret.message = "";// jika perlu lanjut recursive, isi
            long milis = CurrentTimeMillis();
            DateTime milisBack = DateTimeOffset.FromUnixTimeMilliseconds(milis).UtcDateTime.AddHours(7);
            string status = "";

            long unixTimestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            long unixTimestampTo = (long)DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
            int rows = 50;
            int Rowsstart = page * rows;
            //order by name
            string urll = "https://fs.tokopedia.net/inventory/v1/fs/" + Uri.EscapeDataString(iden.merchant_code) + "/product/list?";
            string queryParam = "";
            queryParam = "shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&rows=" + Uri.EscapeDataString(Convert.ToString(rows)) + "&start=" + Uri.EscapeDataString(Convert.ToString(Rowsstart)) + "&product_id=&order_by=9&keyword=&exclude_keyword=&sku=&price_min=1&price_max=500000000&preorder=false&free_return=false&wholesale=false";
            //queryParam = "shop_id=" + Uri.EscapeDataString(iden.API_secret_key) + "&rows=" + Uri.EscapeDataString(Convert.ToString(rows)) + "&start=" + Uri.EscapeDataString(Convert.ToString(Rowsstart)) + "&order_by=9";
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
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + iden.token));
            HttpResponseMessage clientResponse = await client.GetAsync(
                urll + queryParam);

            using (HttpContent responseContent = clientResponse.Content)
            {
                using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
                {
                    responseFromServer = await reader.ReadToEndAsync();
                }
            };

            //HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll + queryParam);
            //myReq.Method = "GET";
            //myReq.Headers.Add("Authorization", ("Bearer " + iden.token));
            //myReq.Accept = "application/x-www-form-urlencoded";
            //myReq.ContentType = "application/json";
            //try
            //{
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
            //    //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}
            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ActiveProductListResult)) as ActiveProductListResult;
                if (result.header.error_code == 0)
                {
                    var found = false;
                    foreach (var item in result.data.products)
                    {
                        if (!found)
                        {
                            if (item.sku == SKU)
                            {
                                found = true;
                                if (item.childs != null)
                                {
                                    if (item.childs.Count() > 0)
                                    {
                                        //change by calvin 9 juni 2019
                                        //await GetActiveItemVariantByProductID(iden, SKU, recnumArf01, Convert.ToString(item.id));
                                        string EDBConnID = EDB.GetConnectionString("ConnId");
                                        var sqlStorage = new SqlServerStorage(EDBConnID);

                                        var Jobclient = new BackgroundJobClient(sqlStorage);
                                        Jobclient.Enqueue<TokopediaControllerJob>(x => x.GetActiveItemVariantByProductID(iden.DatabasePathErasoft, SKU, log_CUST, "Barang", "Link Variasi Produk", iden, SKU, recnumArf01, Convert.ToString(item.id), log_request_id));
                                        //end change by calvin 9 juni 2019
                                    }
                                    else
                                    {
                                        var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(item.id) + "' WHERE BRG = '" + Convert.ToString(SKU) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                                        MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                                        {
                                            REQUEST_ID = log_request_id
                                        };
                                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                                    }
                                }
                                else
                                {
                                    var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(item.id) + "' WHERE BRG = '" + Convert.ToString(SKU) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
                                    MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                                    {
                                        REQUEST_ID = log_request_id
                                    };
                                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                                }
                            }
                        }
                    }
                }
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

            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ActiveProductVariantResult)) as ActiveProductVariantResult;
                if (result.header.error_code == 200)
                {
                    List<TEMP_BRG_MP> listNewData = new List<TEMP_BRG_MP>();
                    var success_induk = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(product_id) + "' WHERE BRG = '" + Convert.ToString(brg) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");

                    foreach (var item in result.data.children)
                    {
                        var success = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(item.product_id) + "' WHERE BRG = '" + Convert.ToString(item.sku) + "' AND IDMARKET = '" + Convert.ToString(iden.idmarket) + "'");
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
            catch (Exception ex)
            {
                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            }

            if (!string.IsNullOrWhiteSpace(responseFromServer))
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ActiveProductVariantResult)) as ActiveProductVariantResult;
                if (result.header.error_code == 200)
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

            SetupContext(data);

            var arf01inDB = ErasoftDbContext.ARF01.Where(p => (p.RecNum ?? 0) == data.idmarket).SingleOrDefault();
            if (arf01inDB != null)
            {
                if (!string.IsNullOrWhiteSpace(arf01inDB.API_KEY))
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

            if (responseFromServer != null)
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

                if (responseFromServer != null)
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

            ManageController.GetAttributeTokpedReturn ret = new ManageController.GetAttributeTokpedReturn() {
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

            if (responseFromServer != null)
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(GetVariantResult)) as GetVariantResult;
                if (string.IsNullOrEmpty(result.header.reason))
                {
                    try
                    {
                        string a = "";
                        int i = 0;
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
            public int id { get; set; }
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
            public int product_id { get; set; }
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
            public int error_code { get; set; }
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
            public int id { get; set; }
            public string name { get; set; }
            public string uri { get; set; }
            public string location { get; set; }
        }

        public class ActiveProductListResultProduct
        {
            public int id { get; set; }
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
            public int department_id { get; set; }
            public object[] labels { get; set; }
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
            public int id { get; set; }
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
            public int error_code { get; set; }
        }

        public class ActiveProductVariantResultData
        {
            public int parent_id { get; set; }
            public int default_child { get; set; }
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
            public int product_id { get; set; }
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
            public int product_id { get; set; }
            public string name { get; set; }
            public int shop_id { get; set; }
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
            public int product_id { get; set; }
            public int new_stock { get; set; }

        }
        public class UpdatePriceData
        {
            public string sku { get; set; }
            public int product_id { get; set; }
            public float new_price { get; set; }

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
            public int error_code { get; set; }
        }

        public class GetVariantResultData
        {
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

        public class EditProduct_Product
        {
            public int product_id { get; set; }
            public string name { get; set; }
            public int? category_id { get; set; }
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
            public CreateProduct_Etalase etalase { get; set; }
            public CreateProduct_Product_Wholesale_Price[] product_wholesale_price { get; set; }
            public CreateProduct_Product_Preorder product_preorder { get; set; }
            public List<CreateProduct_Images> images { get; set; }
            public CreateProduct_Product_Video[] product_video { get; set; }
            public CreateProduct_Product_Variant product_variant { get; set; }
        }

        public class CreateProductTokpedData
        {
            public List<CreateProduct_Product> products { get; set; }
        }

        public class CreateProduct_Product
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
            public CreateProduct_Etalase etalase { get; set; }
            public CreateProduct_Product_Wholesale_Price[] product_wholesale_price { get; set; }
            public CreateProduct_Product_Preorder product_preorder { get; set; }
            public List<CreateProduct_Images> images { get; set; }
            public CreateProduct_Product_Video[] product_video { get; set; }
            public CreateProduct_Product_Variant product_variant { get; set; }
        }

        public class CreateProduct_Etalase
        {
            public int etalase_id { get; set; }
            public string etalase_name { get; set; }
        }

        public class CreateProduct_Product_Preorder
        {
            public int preorder_process_time { get; set; }
            public int preorder_time_unit { get; set; }
            public int preorder_status { get; set; }
        }

        public class CreateProduct_Product_Variant
        {
            public List<CreateProduct_Variant> variant { get; set; }
            public List<CreateProduct_Product_Variant1> product_variant { get; set; }
        }

        public class CreateProduct_Variant
        {
            public int v { get; set; }
            public int vu { get; set; }
            public int pos { get; set; }
            public List<CreateProduct_Opt> opt { get; set; }
        }

        public class CreateProduct_Opt
        {
            public int vuv { get; set; }
            public int t_id { get; set; }
            public string cstm { get; set; }
            public List<CreateProduct_Image> image { get; set; }
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
            public int st { get; set; }
            public int stock { get; set; }
            public float price_var { get; set; }
            public string sku { get; set; }
            public List<int> opt { get; set; }
        }

        public class CreateProduct_Product_Wholesale_Price
        {
            public int qty_min { get; set; }
            public int qty_max { get; set; }
            public int prd_prc { get; set; }
        }

        public class CreateProduct_Images
        {
            public string image_description { get; set; }
            public string image_file_path { get; set; }
            public string image_file_name { get; set; }
        }

        public class CreateProduct_Product_Video
        {
            public string url { get; set; }
            public string type { get; set; }
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

        public class TokpedCreateProductResultHeader
        {
            public float process_time { get; set; }
            public string messages { get; set; }
            public string reason { get; set; }
            public int error_code { get; set; }
        }

        public class TokpedCreateProductResultData
        {
            public int upload_id { get; set; }
        }

    }
}