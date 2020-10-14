using Erasoft.Function;
using Hangfire;
using MasterOnline.Models;
using MasterOnline.ViewModels;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web;
using System.Web.Mvc;

namespace MasterOnline.Controllers
{
    public class JDIDControllerJob : Controller
    {
        //AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        public string imageUrl = "https://img20.jd.id/Indonesia/s300x300_/";
        public string ServerUrl = "https://open.jd.id/api";
        public string AccessToken = "";
        public string AppKey = "";
        public string AppSecret = "";
        public string Version = "1.0";
        public string Format = "json";
        public string SignMethod = "md5";
        private string Charset_utf8 = "UTF-8";
        public string Method;
        public string ParamJson;
        public string ParamFile;
        protected List<long> listCategory = new List<long>();
        public List<Model_BrandJob> listBrand = new List<Model_BrandJob>();

        private MoDbContext MoDbContext { get; set; }
        private ErasoftContext ErasoftDbContext { get; set; }
        private DatabaseSQL EDB;
        private string username;

        public JDIDControllerJob()
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

        #region jdid tools
        private string getCurrentTimeFormatted()
        {
            var dt = System.DateTime.Now.ToLocalTime();
            return dt.ToString("yyyy-MM-dd HH:mm:ss.fff") + dt.ToString("zzzz").Replace(":", "");
        }

        public string Call(string sappKey, string saccessToken, string sappSecret, string sMethod, string sParamJson)
        {
            //construct system parameters
            var sysParams = new Dictionary<string, string>();
            //sysParams.Add("app_key", this.AppKey);
            sysParams.Add("app_key", sappKey);
            sysParams.Add("v", this.Version);
            sysParams.Add("format", this.Format);
            sysParams.Add("sign_method", this.SignMethod);
            //sysParams.Add("method", this.Method);
            sysParams.Add("method", sMethod);
            sysParams.Add("timestamp", this.getCurrentTimeFormatted());
            //sysParams.Add("access_token", this.AccessToken);
            sysParams.Add("access_token", saccessToken);

            //get business parameters
            if (sParamJson != null && sParamJson.Length > 0)
            {
                sysParams.Add("param_json", sParamJson);
            }
            else
            {
                sysParams.Add("param_json", "{}");
            }
            //sign
            sysParams.Add("sign", this.generateSign(sysParams, sappSecret));

            //send http post request
            var content = this.curl(this.ServerUrl, null, sysParams);
            return content;
        }

        public string Call4BigData(string sappSecret)
        {
            //construct system parameters
            var sysParams = new Dictionary<string, string>();
            sysParams.Add("app_key", this.AppKey);
            sysParams.Add("v", this.Version);
            sysParams.Add("format", this.Format);
            sysParams.Add("sign_method", this.SignMethod);
            sysParams.Add("method", this.Method);
            sysParams.Add("timestamp", this.getCurrentTimeFormatted());
            sysParams.Add("access_token", this.AccessToken);

            //get business parameters
            if (null != this.ParamJson && this.ParamJson.Length > 0)
            {
                sysParams.Add("param_json", this.ParamJson);
            }
            else
            {
                sysParams.Add("param_json", "{}");
            }

            //get business file which would upload
            if (null != this.ParamFile && this.ParamFile.Length > 0)
            {
                sysParams.Add("param_file_md5", this.GetMD5HashFromFile(this.ParamFile));
            }
            else
            {
                throw new ArgumentException("paramter ParamFile is required");
            }

            //sign
            sysParams.Add("sign", this.generateSign(sysParams, sappSecret));

            //send http post request
            var postDatas = new NameValueCollection();
            foreach (var item in sysParams)
            {
                postDatas.Add(item.Key, item.Value);
            }
            var content = this.curl(this.ServerUrl, new string[] { this.ParamFile }, sysParams);
            return content;
        }

        private string GetMD5HashFromFile(string fileName)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(fileName))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        public string curl(string url, string[] files, Dictionary<string, string> formFields = null)
        {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "multipart/form-data; boundary=" +
                                    boundary;
            request.Method = "POST";
            request.KeepAlive = true;

            Stream memStream = new System.IO.MemoryStream();

            var boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" +
                                                                    boundary + "\r\n");
            var endBoundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" +
                                                                        boundary + "--");


            string formdataTemplate = "\r\n--" + boundary +
                                        "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";
            try
            {
                if (formFields != null)
                {
                    foreach (string key in formFields.Keys)
                    {
                        string formitem = string.Format(formdataTemplate, key, formFields[key]);
                        byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                        memStream.Write(formitembytes, 0, formitembytes.Length);
                    }
                }


                //file
                if (files != null)
                {
                    string headerTemplate =
                        "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                        "Content-Type: application/octet-stream\r\n\r\n";
                    for (int i = 0; i < files.Length; i++)
                    {
                        memStream.Write(boundarybytes, 0, boundarybytes.Length);
                        var header = string.Format(headerTemplate, "param_file", files[i]);
                        var headerbytes = System.Text.Encoding.UTF8.GetBytes(header);

                        memStream.Write(headerbytes, 0, headerbytes.Length);

                        using (var fileStream = new FileStream(files[i], FileMode.Open, FileAccess.Read))
                        {
                            var buffer = new byte[1024];
                            var bytesRead = 0;
                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                memStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
                //~:end file


                memStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
                request.ContentLength = memStream.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    memStream.Position = 0;
                    byte[] tempBuffer = new byte[memStream.Length];
                    memStream.Read(tempBuffer, 0, tempBuffer.Length);
                    memStream.Close();
                    requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                }

                using (var response = request.GetResponse())
                {
                    Stream stream2 = response.GetResponseStream();
                    StreamReader reader2 = new StreamReader(stream2);
                    return reader2.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }

        }

        private string generateSign(Dictionary<string, string> sysDataDictionary, string sappSecret)
        {
            var dic = sysDataDictionary.OrderBy(key => key.Key).ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);
            var sb = new System.Text.StringBuilder();
            foreach (var item in dic)
            {
                if (!"".Equals(item.Key) && !"".Equals(item.Value))
                {
                    sb.Append(item.Key).Append(item.Value);
                }

            }
            //prepend and append appsecret   
            //sb.Insert(0, this.AppSecret);
            //sb.Append(this.AppSecret);
            sb.Insert(0, sappSecret);
            sb.Append(sappSecret);
            var signValue = this.calculateMD5Hash(sb.ToString());
            //Console.WriteLine("raw string=" + sb.ToString());
            //Console.WriteLine("signValue=" + signValue);
            return signValue;
        }


        private string calculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();

        }
        #endregion

        protected void SetupContext(string DatabasePathErasoft, string uname)
        {
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, DatabasePathErasoft);
            username = uname;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke JDID Gagal.")]
        public async Task<string> JD_CreateProduct(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, JDIDAPIData data)
        {
            SetupContext(data.DatabasePathErasoft, data.username);

            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper()).FirstOrDefault();
            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == log_CUST.ToUpper()).FirstOrDefault();
            if (brgInDb == null || marketplace == null)
                return "invalid passing data";
            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum && b.DISPLAY == true).FirstOrDefault();
            if (detailBrg == null)
                return "invalid passing data";

            var categoryID = "";
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
            }

            //Required parameters, other parameters can be add
            //var postData = "apiKey=" + Uri.EscapeDataString(iden.API_key);
            //postData += "&apiCredential=" + Uri.EscapeDataString(iden.API_credential);
            //postData += "&name=" + Uri.EscapeDataString(brgInDb.NAMA);
            //postData += "&reference=" + Uri.EscapeDataString(brgInDb.BRG);
            //postData += "&active=" + Uri.EscapeDataString("1");
            //postData += "&visibility=" + Uri.EscapeDataString("both");
            //postData += "&available_for_order=" + Uri.EscapeDataString("1");
            //postData += "&show_price=" + Uri.EscapeDataString("1");
            //postData += "&online_only=" + Uri.EscapeDataString("0");
            //postData += "&condition=" + Uri.EscapeDataString("new");
            //postData += "&wholesale_price=" + Uri.EscapeDataString("0");
            //postData += "&price=" + Uri.EscapeDataString(detailBrg.HJUAL.ToString());
            //postData += "&on_sale=" + Uri.EscapeDataString("1");
            //postData += "&link_rewrite=" + Uri.EscapeDataString(brgInDb.NAMA.Replace(" ", "-").ToLower());
            //postData += "&width=" + Uri.EscapeDataString(brgInDb.LEBAR.ToString());
            //postData += "&height=" + Uri.EscapeDataString(brgInDb.TINGGI.ToString());
            //postData += "&depth=" + Uri.EscapeDataString("0");
            //postData += "&weight=" + Uri.EscapeDataString(weight.ToString());
            //postData += "&additional_shipping_cost=" + Uri.EscapeDataString("0");
            //postData += "&minimal_quantity=" + Uri.EscapeDataString(brgInDb.MINI.ToString());
            //postData += "&out_of_stock=" + Uri.EscapeDataString("0");
            //postData += "&id_category_default=" + Uri.EscapeDataString(finalCategory.ToString());
            //postData += "&category=" + Uri.EscapeDataString("[" + categoryID.ToString() + "]");
            //postData += "&id_manufacturer=" + Uri.EscapeDataString(detailBrg.AVALUE_38.ToString());


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

            //postData += "&short_description=" + Uri.EscapeDataString(vDescription);
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
            qty_stock = brgInDb.ISI;
            //end handle stock


            categoryID = categoryID.Substring(0, categoryID.Length - 1);
            string[] splitCat = categoryID.Split(',');
            var finalCategory = splitCat.Last();
            var weight = Convert.ToDouble(brgInDb.BERAT / 1000);

            //buatkan selection merk (brandId) get dari API xxx
            //buatkan selection jaminan Asuransi Yes | No (isQuality)
            //pelajari (commonAttributeIds)
            //

            string sMethod = "epi.ware.openapi.SpuApi.publishWare";
            string sParamJson = "{\"spuInfo\":{\"appDescription\":\"" + vDescription + "\", \"brandId\":30830, \"catId\":" + finalCategory + ", \"commonAttributeIds\":\"9192:2258575;9233:2262793;9191:2262808;9180:2259386;9175:2259395;9223:2262814;9514:2262804;9193:2262797\", " +
                "\"description\":\"" + vDescription + "\", \"isQuality\":1, \"isSequenceNumber\":1, \"keywords\":\"kew1,ke2\", \"packageInfo\":\"pack\", \"productArea\":\"xinjiang\", " +
                "\"qualityDays\":12, \"spuName\":\"test spu name for api 4\", \"subtitle\":\"sub\", \"subtitleHref\":\"http://xxxx.com\", \"subtitleHrefM\":\"http://xxxxm.com\", \"transportId\":42, " +
                "\"warrantyPeriod\":3, \"whetherCod\":0,  \"weight\":\"1\",  \"netWeight\":\"1.5\",   \"packHeight\":\"1\",   \"packLong\":\"1\",   \"packWide\":\"1\",   \"piece\":1}, " +
                "\"skuList\":[ {\"costPrice\":1000, \"imgFlag\":\"https://indomedia.com.au/wp-content/uploads/2019/02/vvip-warpan.jpg\", \"jdPrice\":1000, \"sellerSkuId\":\"sellerSkuId 0\", " +
                "\"skuName\":\"sku name for api 0\", \"stock\":10, \"upc\":\"upc\" }]}";


            var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
            var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
            if (ret != null)
            {
                if (ret.openapi_msg.ToLower() == "success")
                {
                    var retPrice = JsonConvert.DeserializeObject(ret.openapi_data, typeof(Data_UpPriceJob)) as Data_UpPriceJob;
                    if (retPrice != null)
                    {
                        if (retPrice.success)
                        {
                            try
                            {
                                //                                if (!string.IsNullOrEmpty(responseFromServer))
                                //                                {
                                //                                    var resultAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(ResultCreateProduct82Cart)) as ResultCreateProduct82Cart;

                                //                                    if (resultAPI != null)
                                //                                    {
                                //                                        if (resultAPI.error == "none" && resultAPI.results == "success")
                                //                                        {
                                //                                            if (resultAPI.data.data[0].id_product != null)
                                //                                            {
                                //                                                var item = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == kodeProduk.ToUpper() && b.IDMARKET == marketplace.RecNum).SingleOrDefault();
                                //                                                if (item != null)
                                //                                                {
                                //                                                    item.BRG_MP = Convert.ToString(resultAPI.data.data[0].id_product) + ";0";
                                //                                                    item.LINK_STATUS = "Buat Produk Berhasil";
                                //                                                    item.LINK_DATETIME = DateTime.UtcNow.AddHours(7);
                                //                                                    item.LINK_ERROR = "0;Buat Produk;;";
                                //                                                    ErasoftDbContext.SaveChanges();
                                //                                                }

                                //                                                if (brgInDb.TYPE == "4") // punya variasi
                                //                                                {
                                //                                                    //handle variasi product
                                //                                                    #region variasi product
                                //                                                    var var_stf02 = ErasoftDbContext.STF02.Where(p => p.PART == kodeProduk).ToList();
                                //                                                    var var_strukturVar = ErasoftDbContext.STF02I.Where(p => p.BRG == kodeProduk && p.MARKET == "82CART").ToList().OrderBy(p => p.RECNUM);

                                //                                                    foreach (var itemData in var_stf02)
                                //                                                    {
                                //                                                        #region varian LV1
                                //                                                        if (!string.IsNullOrEmpty(itemData.Sort8))
                                //                                                        {
                                //                                                            var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 1 && p.KODE_VAR == itemData.Sort8).FirstOrDefault();
                                //                                                            listattributeIDGroup = variant_id_group.MP_JUDUL_VAR + ",";
                                //                                                            listattributeIDItems = variant_id_group.MP_VALUE_VAR + ",";
                                //                                                            //foreach (var itemVar in variant_id_items)
                                //                                                            //{
                                //                                                            //var dataBRGItem = var_stf02.Where(p => p.Sort8 == itemVar.KODE_VAR).FirstOrDefault();
                                //                                                            //c82CartController.E2Cart_AddAttributeProduct(dataLocal, itemData.BRG, item.BRG_MP, attributeIDGroup, attributeIDItems, weight.ToString(), detailBrg.HJUAL.ToString(), Convert.ToInt32(qty_stock), dataBRGItem.LINK_GAMBAR_1.ToString());

                                //                                                            //}
                                //                                                        }
                                //                                                        #endregion

                                //                                                        #region varian LV2
                                //                                                        if (!string.IsNullOrEmpty(itemData.Sort9))
                                //                                                        {
                                //                                                            var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 2 && p.KODE_VAR == itemData.Sort9).FirstOrDefault();
                                //                                                            listattributeIDGroup = listattributeIDGroup + variant_id_group.MP_JUDUL_VAR + ",";
                                //                                                            listattributeIDItems = listattributeIDItems + variant_id_group.MP_VALUE_VAR + ",";
                                //                                                            //foreach (var itemVar in variant_id_items)
                                //                                                            //{
                                //                                                            //var dataBRGItem = var_stf02.Where(p => p.Sort8 == itemVar.KODE_VAR).FirstOrDefault();
                                //                                                            //c82CartController.E2Cart_AddAttributeProduct(dataLocal, itemData.BRG, item.BRG_MP, attributeIDGroup, attributeIDItems, weight.ToString(), detailBrg.HJUAL.ToString(), Convert.ToInt32(qty_stock), dataBRGItem.LINK_GAMBAR_1.ToString());

                                //                                                            //}
                                //                                                        }
                                //                                                        #endregion

                                //                                                        #region varian LV3
                                //                                                        if (!string.IsNullOrEmpty(itemData.Sort10))
                                //                                                        {
                                //                                                            var variant_id_group = var_strukturVar.Where(p => p.LEVEL_VAR == 3 && p.KODE_VAR == itemData.Sort10).FirstOrDefault();
                                //                                                            listattributeIDGroup = listattributeIDGroup + variant_id_group.MP_JUDUL_VAR + ",";
                                //                                                            listattributeIDItems = listattributeIDItems + variant_id_group.MP_VALUE_VAR + ",";
                                //                                                            //foreach (var itemVar in variant_id_items)
                                //                                                            //{
                                //                                                            //var dataBRGItem = var_stf02.Where(p => p.Sort8 == itemVar.KODE_VAR).FirstOrDefault();
                                //                                                            //c82CartController.E2Cart_AddAttributeProduct(dataLocal, itemData.BRG, item.BRG_MP, attributeIDGroup, attributeIDItems, weight.ToString(), detailBrg.HJUAL.ToString(), Convert.ToInt32(qty_stock), dataBRGItem.LINK_GAMBAR_1.ToString());
                                //                                                            //}
                                //                                                        }
                                //                                                        #endregion

                                //                                                        listattributeIDGroup = listattributeIDGroup.Substring(0, listattributeIDGroup.Length - 1);
                                //                                                        listattributeIDItems = listattributeIDItems.Substring(0, listattributeIDItems.Length - 1);

                                //                                                        c82CartController.E2Cart_AddAttributeProduct(dataLocal, itemData.BRG, item.BRG_MP, listattributeIDGroup, listattributeIDItems, weight.ToString(), detailBrg.HJUAL.ToString(), Convert.ToInt32(qty_stock), itemData.LINK_GAMBAR_1.ToString());

                                //                                                    }



                                //                                                    #endregion
                                //                                                    //end handle variasi product
                                //                                                }

                                //                                                //handle all image was uploaded
                                //                                                foreach (var images in lGambarUploaded)
                                //                                                {
                                //                                                    Task.Run(() => c82CartController.E2Cart_AddImageProduct(dataLocal, item.BRG_MP, images)).Wait();
                                //                                                }
                                //                                                //end handle all image was uploaded

                                //                                                //start handle update stock for default product

                                //#if (DEBUG || Debug_AWS)
                                //                                                Task.Run(() => c82CartController.E2Cart_UpdateStock_82Cart(dbPathEra, item.BRG, iden.no_cust, "Stock", "Update Stok", dataLocal, item.BRG_MP, Convert.ToInt32(qty_stock), iden.username)).Wait();
                                //#else
                                //                                var EDB = new DatabaseSQL(dbPathEra);
                                //                                string EDBConnID = EDB.GetConnectionString("ConnId");
                                //                                var sqlStorage = new SqlServerStorage(EDBConnID);
                                //                                var client = new BackgroundJobClient(sqlStorage);
                                //                                client.Enqueue<EightTwoCartControllerJob>(x => x.E2Cart_UpdateStock_82Cart(dbPathEra, item.BRG, iden.no_cust, "Stock", "Update Stok", dataLocal, item.BRG_MP, Convert.ToInt32(qty_stock), iden.username));
                                //                                //end handle update stock for default product
                                //#endif
                                //                                                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                                //                                            }
                                //                                        }
                                //                                        else
                                //                                        {
                                //                                            if (resultAPI.error != null && resultAPI.error != "none")
                                //                                            {

                                //                                            }
                                //                                        }
                                //                                    }
                                //                                }
                            }
                            catch (Exception ex)
                            {
                                //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                                //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                            }
                        }
                        else
                        {
                            //currentLog.REQUEST_EXCEPTION = retStok.message;
                            //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        }
                    }
                    else
                    {
                        //currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                        //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    }
                }
                else
                {
                    //currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                }
            }
            else
            {
                //currentLog.REQUEST_EXCEPTION = response;
                //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
            }



            return "";
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Harga Jual Produk {obj} ke JD.ID gagal.")]
        public async Task<string> JD_updatePrice(string DatabasePathErasoft, string stf02_brg, string log_CUST, string log_ActionCategory, string log_ActionName, JDIDAPIDataJob data, string id, int price, string uname)
        {
            SetupContext(DatabasePathErasoft, uname);

            try {

                string sMethod = "epi.ware.openapi.SkuApi.updateSkuInfo";
                string sParamJson = "{\"skuInfo\":{\"skuId\":" + id + ", \"jdPrice\":" + price + "}}";

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (ret != null)
                {
                    if (ret.openapi_msg.ToLower() == "success")
                    {
                        var retPrice = JsonConvert.DeserializeObject(ret.openapi_data, typeof(Data_UpPriceJob)) as Data_UpPriceJob;
                        if (retPrice != null)
                        {
                            if (retPrice.success)
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
                            }
                            else
                            {
                                throw new Exception(retPrice.message.ToString());
                            }
                        }
                        else
                        {
                            throw new Exception(ret.openapi_msg.ToString());
                        }
                    }
                    else
                    {
                        throw new Exception(ret.openapi_msg.ToString());
                    }
                }
                else
                {
                    throw new Exception("Tidak ada respon dari API.");
                }
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception(msg);
            }

            return "";
        }

        [System.Web.Mvc.HttpGet]
        public void getCategory(JDIDAPIDataJob data)
        {
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Category",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = data.accessToken,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            //var mgrApiManager = new JDIDControllerJob();
            //mgrApiManager.AppKey = data.appKey;
            //mgrApiManager.AppSecret = data.appSecret;
            //mgrApiManager.AccessToken = data.accessToken;
            //mgrApiManager.Method = "epi.ware.openapi.CategoryApi.getAllCategoryTree";
            //mgrApiManager.ParamJson = "";

            string sMethod = "epi.ware.openapi.CategoryApi.getAllCategoryTree";
            string sParamJson = "";

            try
            {
                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (ret != null)
                {
                    if (ret.openapi_code == 0)
                    {
                        var listKategori = JsonConvert.DeserializeObject(ret.openapi_data, typeof(DATA_CATJob)) as DATA_CATJob;
                        if (listKategori != null)
                        {
                            if (listKategori.success)
                            {
                                EDB.ExecuteSQL("CString", CommandType.Text, "Update ARF01 SET STATUS_API = '1' WHERE TOKEN = '" + data.accessToken + "' AND API_KEY = '" + data.appKey + "'");
                                string dbPath = "";
                                var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
                                if (sessionData?.Account != null)
                                {
                                    dbPath = sessionData.Account.DatabasePathErasoft;
                                }
                                else
                                {
                                    if (sessionData?.User != null)
                                    {
                                        var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                                        dbPath = accFromUser.DatabasePathErasoft;
                                    }
                                }

                                #region connstring
#if AWS
                    string con = "Data Source=localhost;Initial Catalog=" + dbPath + ";Persist Security Info=True;User ID=sa;Password=admin123^";
#elif Debug_AWS
                                string con = "Data Source=13.250.232.74;Initial Catalog=" + dbPath + ";Persist Security Info=True;User ID=sa;Password=admin123^";
#else
                                string con = "Data Source=13.251.222.53;Initial Catalog=" + dbPath + ";Persist Security Info=True;User ID=sa;Password=admin123^";
#endif
                                #endregion
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
                                        oCommand.CommandText = "INSERT INTO [CATEGORY_JDID] ([CATEGORY_CODE], [CATEGORY_NAME], [CATE_STATE], [TYPE], [LEAF], [PARENT_CODE]) VALUES (@CATEGORY_CODE, @CATEGORY_NAME, @CATE_STATE, @TYPE, @LEAF, @PARENT_CODE)";
                                        //oCommand.Parameters.Add(new SqlParameter("@ARF01_SORT1_CUST", SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@CATEGORY_CODE", SqlDbType.NVarChar, 50));
                                        oCommand.Parameters.Add(new SqlParameter("@CATEGORY_NAME", SqlDbType.NVarChar, 250));
                                        oCommand.Parameters.Add(new SqlParameter("@CATE_STATE", SqlDbType.NVarChar, 3));
                                        oCommand.Parameters.Add(new SqlParameter("@TYPE", SqlDbType.NVarChar, 3));
                                        oCommand.Parameters.Add(new SqlParameter("@LEAF", SqlDbType.NVarChar, 1));
                                        oCommand.Parameters.Add(new SqlParameter("@PARENT_CODE", SqlDbType.NVarChar, 50));

                                        //try
                                        //{
                                        foreach (var item in listKategori.model) //foreach parent level top
                                        {
                                            oCommand.Parameters[0].Value = item.cateId;
                                            oCommand.Parameters[1].Value = item.cateName;
                                            oCommand.Parameters[2].Value = item.cateState;
                                            oCommand.Parameters[3].Value = item.type;
                                            oCommand.Parameters[4].Value = "1";
                                            if (item.parentCateVo != null)
                                            {
                                                oCommand.Parameters[5].Value = item.parentCateVo.cateId;
                                            }
                                            else
                                            {
                                                oCommand.Parameters[5].Value = "";
                                            }
                                            if (oCommand.ExecuteNonQuery() > 0)
                                            {
                                                listCategory.Add(item.cateId);
                                                if (item.parentCateVo != null)
                                                {
                                                    if (!listCategory.Contains(item.parentCateVo.cateId))
                                                    {
                                                        RecursiveInsertCategory(oCommand, item.parentCateVo);
                                                    }
                                                }
                                            }
                                            else
                                            {

                                            }
                                        }
                                        //oTransaction.Commit();
                                        //}
                                        //catch (Exception ex)
                                        //{
                                        //    //oTransaction.Rollback();
                                        //}
                                    }
                                }
                            }
                            else
                            {
                                currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            }
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        }
                    }
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = response;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                }
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
            }


        }
        protected void RecursiveInsertCategory(SqlCommand oCommand, Model_CatJob item)
        {
            //foreach (var child in categories.Where(p => p.parent_id == parent))
            //{

            oCommand.Parameters[0].Value = item.cateId;
            oCommand.Parameters[1].Value = item.cateName;
            oCommand.Parameters[2].Value = item.cateState;
            oCommand.Parameters[3].Value = item.type;
            oCommand.Parameters[4].Value = "0";
            if (item.parentCateVo != null)
            {
                oCommand.Parameters[5].Value = item.parentCateVo.cateId;

            }
            else
            {
                oCommand.Parameters[5].Value = "";
            }

            if (oCommand.ExecuteNonQuery() > 0)
            {
                listCategory.Add(item.cateId);
                if (item.parentCateVo != null)
                {
                    if (!listCategory.Contains(item.parentCateVo.cateId))
                    {
                        RecursiveInsertCategory(oCommand, item.parentCateVo);
                    }
                }
            }
            else
            {

            }
            //}
        }

        public ATTRIBUTE_JDID getAttribute(JDIDAPIDataJob data, string catId)
        {
            var retAttr = new ATTRIBUTE_JDID();
            //var mgrApiManager = new JDIDControllerJob();

            //mgrApiManager.AppKey = data.appKey;
            //mgrApiManager.AppSecret = data.appSecret;
            //mgrApiManager.AccessToken = data.accessToken;

            //mgrApiManager.Method = "epi.ware.openapi.WareAttributeApi.getAttributesByCatId";
            //mgrApiManager.ParamJson = "{ \"catId\":\"" + catId + "\"}";

            string sMethod = "epi.ware.openapi.WareAttributeApi.getAttributesByCatId";
            string sParamJson = "{ \"catId\":\"" + catId + "\"}";

            var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
            var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
            if (ret != null)
            {
                if (ret.openapi_code == 0)
                {
                    var listAttr = JsonConvert.DeserializeObject(ret.openapi_data, typeof(AttrDataJob)) as AttrDataJob;
                    if (listAttr != null)
                    {
                        if (listAttr.model.Count > 0)
                        {
                            string a = "";
                            int i = 0;
                            retAttr.CATEGORY_CODE = catId;
                            foreach (var attr in listAttr.model)
                            {

                                a = Convert.ToString(i + 1);
                                retAttr["ACODE_" + a] = Convert.ToString(attr.propertyId);
                                retAttr["ATYPE_" + a] = attr.type.ToString();
                                retAttr["ANAME_" + a] = attr.nameEn;
                                i = i + 1;
                            }
                            for (int j = i; j < 20; j++)
                            {
                                a = Convert.ToString(j + 1);
                                retAttr["ACODE_" + a] = "";
                                retAttr["ATYPE_" + a] = "";
                                retAttr["ANAME_" + a] = "";
                            }
                        }

                    }
                }
            }
            //return response;
            //return ret.openapi_data;
            return retAttr;
        }

        public List<ATTRIBUTE_OPT_JDID> getAttributeOpt(JDIDAPIDataJob data, string catId, string attrId, int page)
        {
            //var mgrApiManager = new JDIDControllerJob();
            var listOpt = new List<ATTRIBUTE_OPT_JDID>();
            //mgrApiManager.AppKey = data.appKey;
            //mgrApiManager.AppSecret = data.appSecret;
            //mgrApiManager.AccessToken = data.accessToken;

            //mgrApiManager.Method = "epi.ware.openapi.WareAttributeApi.getAttrValuesByCatIdAndAttrId";
            //mgrApiManager.ParamJson = "{ \"catId\":\"" + catId + "\", \"attrId\":\"" + attrId + "\", \"currentPage\":" + page + ", \"pageSize\":20}";

            string sMethod = "epi.ware.openapi.WareAttributeApi.getAttrValuesByCatIdAndAttrId";
            string sParamJson = "{ \"catId\":\"" + catId + "\", \"attrId\":\"" + attrId + "\", \"currentPage\":" + page + ", \"pageSize\":20}";

            var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
            var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
            if (ret != null)
            {
                if (ret.openapi_code == 0)
                {
                    var retOpt = JsonConvert.DeserializeObject(ret.openapi_data, typeof(JDID_ATTRIBUTE_OPTJob)) as JDID_ATTRIBUTE_OPTJob;
                    if (retOpt != null)
                    {
                        if (retOpt.model != null)
                        {
                            if (retOpt.model.data.Count > 0)
                            {
                                foreach (var opt in retOpt.model.data)
                                {
                                    var newOpt = new ATTRIBUTE_OPT_JDID()
                                    {
                                        ACODE = opt.attributeValueId.ToString(),
                                        OPTION_VALUE = opt.nameEn
                                    };
                                    listOpt.Add(newOpt);
                                }
                                //if(retOpt.model.data.Count == 20)
                                //{
                                var cursiveOpt = getAttributeOpt(data, catId, attrId, page + 1);
                                if (cursiveOpt.Count > 0)
                                {
                                    foreach (var opt2 in cursiveOpt)
                                    {
                                        listOpt.Add(opt2);
                                    }
                                }
                                //}
                            }
                        }

                    }
                }
            }


            return listOpt;
        }

        public BindingBase getListProduct(JDIDAPIDataJob data, int page, string cust, int recordCount)
        {
            var ret = new BindingBase
            {
                status = 0,
                recordCount = recordCount,
            };

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get Item List",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = (page + 1).ToString(),
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);

            try
            {
                if (listBrand.Count == 0)
                {
                    getShopBrand(data);
                }
                //var mgrApiManager = new JDIDControllerJob();
                //mgrApiManager.AppKey = data.appKey;
                //mgrApiManager.AppSecret = data.appSecret;
                //mgrApiManager.AccessToken = data.accessToken;
                //mgrApiManager.Method = "com.jd.eptid.warecenter.api.ware.WarePlusClient.getWareTinyInfoListByVenderId";
                //mgrApiManager.ParamJson = (page + 1) + ",10";

                string sMethod = "com.jd.eptid.warecenter.api.ware.WarePlusClient.getWareTinyInfoListByVenderId";
                string sParamJson = (page + 1) + ",10";

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var retData = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (retData != null)
                {
                    if (retData.openapi_msg.ToLower() == "success")
                    {
                        var listProd = JsonConvert.DeserializeObject(retData.openapi_data, typeof(Data_ListProdJob)) as Data_ListProdJob;
                        if (listProd != null)
                        {
                            if (listProd.success)
                            {
                                string msg = "";
                                bool adaError = false;
                                if (listProd.model.spuInfoVoList.Count > 0)
                                {
                                    ret.status = 1;
                                    int IdMarket = ErasoftDbContext.ARF01.Where(c => c.CUST == cust).FirstOrDefault().RecNum.Value;
                                    if (listProd.model.spuInfoVoList.Count == 10)
                                        ret.message = (page + 1).ToString();

                                    foreach (var item in listProd.model.spuInfoVoList)
                                    {
                                        //product status: 1.online,2.offline,3.punish,4.deleted
                                        if (item.wareStatus == 1 || item.wareStatus == 2)
                                        {
                                            var retProd = GetProduct(data, item, IdMarket, cust);
                                            if (retProd.status == 1)
                                            {
                                                ret.recordCount += retProd.recordCount;
                                            }
                                            else
                                            {
                                                adaError = true;
                                                msg += item.spuId + ":" + retProd.message + "___||___";
                                            }
                                        }

                                    }
                                }
                                if (adaError)
                                {
                                    currentLog.REQUEST_EXCEPTION = msg;
                                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                                }
                                else
                                {
                                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data, currentLog);
                                }
                            }
                            else
                            {
                                ret.message = retData.openapi_data;
                                currentLog.REQUEST_EXCEPTION = ret.message;
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            }
                        }
                        else
                        {
                            ret.message = retData.openapi_data;
                            currentLog.REQUEST_EXCEPTION = ret.message;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        }
                    }
                    else
                    {
                        ret.message = retData.openapi_msg;
                        currentLog.REQUEST_EXCEPTION = ret.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    }
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
            }
            return ret;
        }

        public BindingBase GetProduct(JDIDAPIDataJob data, SpuinfovolistJob itemFromList, int IdMarket, string cust)
        {
            var ret = new BindingBase
            {
                status = 0,
            };

            try
            {
                //var mgrApiManager = new JDIDControllerJob();
                //mgrApiManager.AppKey = data.appKey;
                //mgrApiManager.AppSecret = data.appSecret;
                //mgrApiManager.AccessToken = data.accessToken;
                //mgrApiManager.Method = "com.jd.eptid.warecenter.api.ware.WarePlusClient.getSkuInfoBySpuId";
                //mgrApiManager.ParamJson = "[" + itemFromList.spuId + "]";

                string sMethod = "com.jd.eptid.warecenter.api.ware.WarePlusClient.getSkuInfoBySpuId";
                string sParamJson = "[" + itemFromList.spuId + "]";

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var retProd = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (retProd != null)
                {
                    if (retProd.openapi_msg.ToLower() == "success")
                    {
                        var dataProduct = JsonConvert.DeserializeObject(retProd.openapi_data, typeof(ProductDataJob)) as ProductDataJob;
                        if (dataProduct != null)
                        {
                            if (dataProduct.success)
                            {
                                var stf02h_local = ErasoftDbContext.STF02H.Where(m => m.IDMARKET == IdMarket).ToList();
                                var tempBrg_local = ErasoftDbContext.TEMP_BRG_MP.Where(m => m.IDMARKET == IdMarket).ToList();

                                var haveVarian = false;
                                if (dataProduct.model.Count > 1)
                                {
                                    haveVarian = true;
                                }

                                foreach (var item in dataProduct.model)
                                {
                                    var tempbrginDB = new TEMP_BRG_MP();
                                    var brgInDB = new STF02H();

                                    if (haveVarian)
                                    {
                                        //handle parent
                                        string kdBrgInduk = item.spuId.ToString();
                                        bool createParent = false;
                                        tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == kdBrgInduk).FirstOrDefault();
                                        brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP) == kdBrgInduk).FirstOrDefault();
                                        if (tempbrginDB == null && brgInDB == null)
                                        {
                                            if (item.skuId == dataProduct.model[0].skuId)
                                                createParent = true;
                                        }
                                        else if (brgInDB != null)
                                        {
                                            kdBrgInduk = brgInDB.BRG;
                                        }
                                        //end handle parent

                                        //foreach (var varian in item.skuIds)
                                        //{
                                        //    tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == varian.ToString().ToUpper()).FirstOrDefault();
                                        //    brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == varian.ToString().ToUpper()).FirstOrDefault();
                                        //    if (tempbrginDB == null && brgInDB == null)
                                        //    {

                                        //    }
                                        //}
                                        //for (int i = 0; i < item.skuIds.Count; i++)
                                        //{
                                        tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        if (tempbrginDB == null && brgInDB == null)
                                        {
                                            var retData = getProductDetail(data, item, kdBrgInduk, createParent, item.skuId.ToString(), cust, IdMarket, itemFromList);
                                            if (retData.status == 1)
                                            {
                                                ret.recordCount += retData.recordCount;
                                                //createParent = false;
                                            }
                                        }
                                        //}

                                    }
                                    else
                                    {
                                        tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == item.skuId.ToString().ToUpper()).FirstOrDefault();
                                        if (tempbrginDB == null && brgInDB == null)
                                        {
                                            var retData = getProductDetail(data, item, "", false, item.skuId.ToString(), cust, IdMarket, itemFromList);
                                            if (retData.status == 1)
                                            {
                                                ret.recordCount += retData.recordCount;
                                            }
                                        }
                                    }

                                }

                                ret.status = 1;
                            }
                            else
                            {
                                ret.message = retProd.openapi_data;
                            }
                        }
                        else
                        {
                            ret.message = retProd.openapi_data;
                        }
                    }
                    else
                    {
                        ret.message = retProd.openapi_msg;
                    }
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }

            return ret;
        }

        public BindingBase getProductDetail(JDIDAPIDataJob data, Model_ProductJob item, string kdBrgInduk, bool createParent, string skuId, string cust, int IdMarket, SpuinfovolistJob itemFromList)
        {
            var ret = new BindingBase
            {
                status = 0,
            };

            try
            {
                //var mgrApiManager = new JDIDControllerJob();
                //mgrApiManager.AppKey = data.appKey;
                //mgrApiManager.AppSecret = data.appSecret;
                //mgrApiManager.AccessToken = data.accessToken;
                //mgrApiManager.Method = "epi.ware.openapi.SkuApi.getSkuBySkuIds";
                ////mgrApiManager.ParamJson = "{ \"page\":" + page + ", \"pageSize\":10}";
                //mgrApiManager.ParamJson = "{\"skuIds\" : \"" + skuId + "\"}";

                string sMethod = "epi.ware.openapi.SkuApi.getSkuBySkuIds";
                string sParamJson = "{\"skuIds\" : \"" + skuId + "\"}";

                string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, BERAT, PANJANG, LEBAR, TINGGI, CUST, Deskripsi, IDMARKET, HJUAL, HJUAL_MP, ";
                sSQL += "DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, KODE_BRG_INDUK, TYPE, ";
                sSQL += "ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
                sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20) VALUES ";

                string sSQLVal = "";

                //if (!string.IsNullOrEmpty(kdBrgInduk))
                //{
                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var retProd = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (retProd != null)
                {
                    if (retProd.openapi_msg.ToLower() == "success")
                    {
                        var detailData = JsonConvert.DeserializeObject(retProd.openapi_data, typeof(Data_Detail_ProductJob)) as Data_Detail_ProductJob;
                        if (detailData != null)
                        {
                            if (detailData.success)
                            {
                                if (!string.IsNullOrEmpty(kdBrgInduk))
                                {
                                    if (createParent)
                                    {
                                        var retSQL = CreateSQLValue(item, detailData.model[0], kdBrgInduk, "", cust, IdMarket, 1, itemFromList);
                                        if (retSQL.status == 1)
                                            sSQLVal += retSQL.message;
                                    }

                                    var retSQL2 = CreateSQLValue(item, detailData.model[0], kdBrgInduk, skuId, cust, IdMarket, 2, itemFromList);
                                    if (retSQL2.status == 1)
                                        sSQLVal += retSQL2.message;
                                }
                                else
                                {
                                    var retSQL = CreateSQLValue(item, detailData.model[0], "", skuId, cust, IdMarket, 0, itemFromList);
                                    if (retSQL.status == 1)
                                        sSQLVal += retSQL.message;
                                }
                            }
                            else
                            {
                                ret.message = string.IsNullOrEmpty(detailData.message) ? retProd.openapi_data : detailData.message;
                            }
                        }
                        else
                        {
                            ret.message = retProd.openapi_data;
                        }
                    }
                    else
                    {
                        ret.message = retProd.openapi_msg;
                    }
                }
                else
                {
                    ret.message = response;
                }

                if (!string.IsNullOrEmpty(sSQLVal))
                {
                    sSQL = sSQL + sSQLVal;
                    sSQL = sSQL.Substring(0, sSQL.Length - 1);
                    var a = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                    ret.recordCount += a;
                    ret.status = 1;
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }

        protected BindingBase CreateSQLValue(Model_ProductJob item, Model_Detail_ProductJob detItem, string kdBrgInduk, string skuId, string cust, int IdMarket, int typeBrg, SpuinfovolistJob itemFromList)
        {
            // typeBrg : 0 = barang tanpa varian; 1 = barang induk; 2 = barang varian
            var ret = new BindingBase
            {
                status = 0,
            };

            string sSQL_Value = "";
            try
            {

                string[] attrVal;
                string value = "";
                var brgAttribute = new Dictionary<string, string>();
                string namaBrg = item.skuName;
                string nama, nama2, urlImage, urlImage2, urlImage3;
                urlImage = "";
                urlImage2 = "";
                urlImage3 = "";
                var categoryCode = itemFromList.fullCategoryId.Split('/');
                var categoryName = itemFromList.fullCategoryName.Split('/');
                double price = Convert.ToDouble(item.jdPrice);
                //var statusBrg = detItem != null ? detItem.status : 1;
                var statusBrg = detItem.status;
                string brand = itemFromList.brandId.ToString();
                //var display = statusBrg.Equals("active") ? 1 : 0;
                string deskripsi = itemFromList.description;

                if (typeBrg != 1)
                {
                    sSQL_Value += " ( '" + skuId + "' , '" + skuId + "' , '";
                }
                else
                {
                    namaBrg = itemFromList.spuName;
                    sSQL_Value += " ( '" + kdBrgInduk + "' , '" + kdBrgInduk + "' , '";
                }

                if (typeBrg == 2)
                {
                    //namaBrg += " " + detItem.skuName;
                    urlImage = imageUrl + detItem.mainImgUri;
                }
                else
                {
                    urlImage = imageUrl + item.mainImgUri;
                }

                if (namaBrg.Length > 30)
                {
                    nama = namaBrg.Substring(0, 30);
                    if (namaBrg.Length > 285)
                    {
                        nama2 = namaBrg.Substring(30, 255);
                    }
                    else
                    {
                        nama2 = namaBrg.Substring(30);
                    }
                }
                else
                {
                    nama = namaBrg;
                    nama2 = "";
                }


                sSQL_Value += nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' ,";

                //var attrVal = detItem.saleAttributeIds.Split(';');
                //if (detItem == null)
                //{
                //    attrVal = item.saleAttributeIds.Split(';');
                //    foreach (Newtonsoft.Json.Linq.JProperty property in item.saleAttributeNameMap)
                //    {
                //        brgAttribute.Add(property.Name, property.Value.ToString());
                //    }
                //}
                //else
                //{
                attrVal = detItem.saleAttributeIds.Split(';');
                foreach (Newtonsoft.Json.Linq.JProperty property in detItem.saleAttributeNameMap)
                {
                    brgAttribute.Add(property.Name, property.Value.ToString());
                }
                //price = brgAttribute.TryGetValue("jdPrice", out value) ? Convert.ToDouble(value) : Convert.ToDouble(item.jdPrice);
                if (Convert.ToDouble(detItem.jdPrice) > 0)
                    price = Convert.ToDouble(detItem.jdPrice);

                //}
                if (listBrand.Count > 0)
                {
                    var a = listBrand.Where(m => m.brandId == itemFromList.brandId).FirstOrDefault();
                    if (a != null)
                        brand = a.brandName;
                }
                //sSQL_Value += Convert.ToDouble(detItem != null ? detItem.netWeight : item.netWeight) * 1000 + " , " + Convert.ToDouble(detItem != null ? detItem.packLong : item.packLong) + " , ";
                //sSQL_Value += Convert.ToDouble(detItem != null ? detItem.packWide : item.packWide) + " , " + Convert.ToDouble(detItem != null ? detItem.packHeight : item.packHeight);
                sSQL_Value += Convert.ToDouble(detItem.netWeight) * 1000 + " , " + Convert.ToDouble(detItem.packLong) + " , ";
                sSQL_Value += Convert.ToDouble(detItem.packWide) + " , " + Convert.ToDouble(detItem.packHeight);
                sSQL_Value += " , '" + cust + "' , '" + deskripsi.Replace('\'', '`') + "' , " + IdMarket + " , " + price + " , " + price + " , ";
                sSQL_Value += statusBrg + " , '" + categoryCode[categoryCode.Length - 1] + "' , '" + categoryName[categoryName.Length - 1] + "' , '";
                sSQL_Value += brand + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "' , '" + (typeBrg == 2 ? kdBrgInduk : "") + "' , '" + (typeBrg == 1 ? "4" : "3") + "'";
                int i;
                for (i = 0; i < attrVal.Length; i++)
                {
                    var attr = attrVal[i].Split(':');
                    if (attr.Length == 2)
                    {
                        var attrName = (brgAttribute.TryGetValue(attrVal[i], out value) ? value : "").Split(':');

                        sSQL_Value += ",'" + attr[0] + "','" + attrName[0] + "','" + attr[1] + "'";
                    }
                    else
                    {
                        sSQL_Value += ",'','',''";
                    }
                }

                for (int j = i; j < 20; j++)
                {
                    sSQL_Value += ",'','',''";
                }

                sSQL_Value += "),";
                //if (typeBrg == 1)
                //    sSQL_Value += ",";
                ret.status = 1;
                ret.message = sSQL_Value;
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }

        protected void getShopBrand(JDIDAPIDataJob data)
        {
            try
            {

                //var mgrApiManager = new JDIDControllerJob();
                //mgrApiManager.AppKey = data.appKey;
                //mgrApiManager.AppSecret = data.appSecret;
                //mgrApiManager.AccessToken = data.accessToken;
                //mgrApiManager.Method = "epi.popShop.getShopBrandList";

                string sMethod = "epi.popShop.getShopBrandList";
                string sParamJson = "";

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var retBrand = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (retBrand != null)
                {
                    if (retBrand.openapi_msg.ToLower() == "success")
                    {
                        var dataBrand = JsonConvert.DeserializeObject(retBrand.openapi_data, typeof(Data_BrandJob)) as Data_BrandJob;
                        if (dataBrand.success)
                        {
                            listBrand = dataBrand.model;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<string> JD_printLabelJDID(JDIDAPIDataJob data, string noref)
        {

            string ret = "";

            try
            {
                string sMethod = "epi.pop.order.print.uat1";
                string sParamJson = noref + ",1,1,\"PDF\""; //orderId, printType, packageNum, imageType //example: "1027577635,1,1,\"PDF\""

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var result = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (result != null)
                {
                    if (result.openapi_msg.ToLower() == "success")
                    {
                        var listPrintLabel = JsonConvert.DeserializeObject(result.openapi_data, typeof(Data_PrintLabel)) as Data_PrintLabel;
                        var str = "{\"data\":" + listPrintLabel.model + "}";
                        foreach(var dataDetail in listPrintLabel.model.data)
                        {
                            ret = dataDetail.PDF.ToString();
                        }
                        //var listDetails = JsonConvert.DeserializeObject(str, typeof(ModelOrderJob)) as ModelOrderJob;

                        //var test = result;
                    }
                    else
                    {
                        ret = "error";
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return ret;
        }

        public async Task<string> JD_sendGoodJDID(JDIDAPIDataJob data, string noref, string noresi)
        {

            string ret = "";

            try
            {
                string sMethod = "epi.popOrder.sendGoods.uat";
                string sParamJson = "{\"orderId\":"+ noref + ", \"expressNo\":\""+ noresi +"\"}";

                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var result = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (result != null)
                {
                    if (result.openapi_msg.ToLower() == "success")
                    {
                        var listRTS = JsonConvert.DeserializeObject(result.openapi_data, typeof(Data_ReadyToShip)) as Data_ReadyToShip;
                        if(listRTS.success == true)
                        {
                            ret = listRTS.message.ToString();
                        }
                        else
                        {
                            ret = listRTS.message.ToString();
                        }
                    }
                    else
                    {
                        ret = "error";
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return ret;
        }

        public void UpdateStock(JDIDAPIDataJob data, string id, int stok)
        {
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Stock",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = id,
                REQUEST_ATTRIBUTE_2 = stok.ToString(),
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data, currentLog);
            //var mgrApiManager = new JDIDControllerJob();

            //mgrApiManager.AppKey = data.appKey;
            //mgrApiManager.AppSecret = data.appSecret;
            //mgrApiManager.AccessToken = data.accessToken;
            //mgrApiManager.Method = "epi.ware.openapi.warestock.updateWareStock";
            //mgrApiManager.ParamJson = "{\"jsonStr\":[{\"skuId\":" + id + ", \"realNum\": " + stok + "}]}";

            string sMethod = "epi.ware.openapi.warestock.updateWareStock";
            string sParamJson = "{\"jsonStr\":[{\"skuId\":" + id + ", \"realNum\": " + stok + "}]}";

            try
            {
                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var ret = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (ret != null)
                {
                    if (ret.openapi_msg.ToLower() == "success")
                    {
                        var retStok = JsonConvert.DeserializeObject(ret.openapi_data, typeof(Data_UpStokJob)) as Data_UpStokJob;
                        if (retStok != null)
                        {
                            if (retStok.success)
                            {

                            }
                            else
                            {
                                currentLog.REQUEST_EXCEPTION = retStok.message;
                                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                            }
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                        }
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = ret.openapi_data;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                    }
                }
                else
                {
                    currentLog.REQUEST_EXCEPTION = response;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data, currentLog);
                }
            }
            catch (Exception ex)
            {
                currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, data, currentLog);
            }

        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> JD_GetOrderByStatusPaid(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            SetupContext(iden.DatabasePathErasoft, iden.username);

            var daysFrom = -1;
            var daysTo = 1;

            while (daysFrom > -13)
            {
                await JD_GetOrderByStatusPaidList3Days(iden, stat, CUST, NAMA_CUST, 1, 0, 0, daysFrom, daysTo);
                daysFrom -= 3;
                daysTo -= 3;
            }

            // tunning untuk tidak duplicate
            var queryStatus = "";
            if (stat == StatusOrder.PAID)
            {
                queryStatus = "\"1\"" + "," + "\"\\\"" + CUST + "\\\"\"" + "," + "\"\\\"" + NAMA_CUST + "\\\"\"";  // "1","\"000011\"","\"Echoboomers\""
            }
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust + "%' and arguments like '%" + queryStatus + "%' and invocationdata like '%JD_GetOrderByStatusPaid%' and statename like '%Enque%' and invocationdata not like '%resi%' and invocationdata not like '%JD_GetOrderByStatusComplete%' and invocationdata not like '%JD_GetOrderByStatusCancel%' ");
            // end tunning untuk tidak duplicate

            return ret;
        }

        public async Task<string> JD_GetOrderByStatusPaidList3Days(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar, int daysFrom, int daysTo)
        {
            //1:Waiting for delivery, 2:Shipped, 3:Waiting_Cancel, 4:Waiting_Refuse, 5:Canceled, 6:Completed, 7:Ready to Ship

            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden.DatabasePathErasoft, iden.username);

            var listOrderId = new List<long>();
            listOrderId.AddRange(GetOrderList(iden, "1", 0, daysFrom));

            string connectionID = Guid.NewGuid().ToString();
            var connIdARF01C = Guid.NewGuid().ToString();

            if (listOrderId.Count > 0)
            {
                var ord = new List_Order_JD { orderIds = new List<string>() };
                string order_10 = "";
                int i = 1;
                foreach (var id in listOrderId)
                {
                    order_10 += id.ToString() + ",";
                    i++;
                    if (i >= 10)
                    {
                        i = 0;
                        order_10 = order_10.Substring(0, order_10.Length - 1);
                        ord.orderIds.Add(order_10);
                        order_10 = "";
                    }
                }
                if (!string.IsNullOrEmpty(order_10))
                {
                    order_10 = order_10.Substring(0, order_10.Length - 1);
                    ord.orderIds.Add(order_10);
                }

                //bool callSP = false;
                int newRecord = 0;
                foreach (var listOrder in ord.orderIds)
                {
                    var insertTemp = GetOrderDetail(iden, listOrder, iden.no_cust, connIdARF01C, connectionID);
                    if (insertTemp.status == 1)
                    {
                        //callSP = true;
                        if (insertTemp.recordCount > 0)
                            newRecord += insertTemp.recordCount;
                    }
                }

                //if (callSP)
                //{
                //    SqlCommand CommandSQL = new SqlCommand();

                //    //add by Tri call sp to insert buyer data
                //    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                //    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;

                //    EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                //    //end add by Tri call sp to insert buyer data

                //    CommandSQL = new SqlCommand();
                //    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                //    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                //    CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                //    CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //    CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 1;
                //    CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = iden.no_cust;

                //    EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);

                //    if (newRecord > 0)
                //    {
                //        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                //        contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(newRecord) + " Pesanan baru dari JD.ID.");

                //        new StokControllerJob().updateStockMarketPlace(connectionID, iden.DatabasePathErasoft, iden.username);
                //    }
                //}
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> JD_GetOrderByStatusRTS(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            SetupContext(iden.DatabasePathErasoft, iden.username);

            var daysFrom = -1;
            var daysTo = 1;

            while (daysFrom > -13)
            {
                await JD_GetOrderByStatusRTSList3Days(iden, stat, CUST, NAMA_CUST, 1, 0, 0, daysFrom, daysTo);
                daysFrom -= 3;
                daysTo -= 3;
            }

            // tunning untuk tidak duplicate
            var queryStatus = "";
            if (stat == StatusOrder.READY_TO_SHIP)
            {
                queryStatus = "\"7\"" + "," + "\"\\\"" + CUST + "\\\"\"" + "," + "\"\\\"" + NAMA_CUST + "\\\"\"";  // "7","\"000011\"","\"Echoboomers\""
            }
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust + "%' and arguments like '%" + queryStatus + "%' and invocationdata like '%JD_GetOrderByStatusRTS%' and statename like '%Enque%' and invocationdata not like '%resi%' and invocationdata not like '%JD_GetOrderByStatusComplete%' and invocationdata not like '%JD_GetOrderByStatusCancel%' ");
            // end tunning untuk tidak duplicate

            return ret;
        }

        public async Task<string> JD_GetOrderByStatusRTSList3Days(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar, int daysFrom, int daysTo)
        {
            //1:Waiting for delivery, 2:Shipped, 3:Waiting_Cancel, 4:Waiting_Refuse, 5:Canceled, 6:Completed, 7:Ready to Ship

            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden.DatabasePathErasoft, iden.username);

            var listOrderId = new List<long>();
            listOrderId.AddRange(GetOrderList(iden, "7", 0, daysFrom));

            string connectionID = Guid.NewGuid().ToString();
            var connIdARF01C = Guid.NewGuid().ToString();

            if (listOrderId.Count > 0)
            {
                var ord = new List_Order_JD { orderIds = new List<string>() };
                string order_10 = "";
                int i = 1;
                foreach (var id in listOrderId)
                {
                    order_10 += id.ToString() + ",";
                    i++;
                    if (i >= 10)
                    {
                        i = 0;
                        order_10 = order_10.Substring(0, order_10.Length - 1);
                        ord.orderIds.Add(order_10);
                        order_10 = "";
                    }
                }
                if (!string.IsNullOrEmpty(order_10))
                {
                    order_10 = order_10.Substring(0, order_10.Length - 1);
                    ord.orderIds.Add(order_10);
                }

                bool callSP = false;
                int newRecord = 0;
                foreach (var listOrder in ord.orderIds)
                {
                    var insertTemp = GetOrderDetail(iden, listOrder, iden.no_cust, connIdARF01C, connectionID);
                    if (insertTemp.status == 1)
                    {
                        //callSP = true;
                        if (insertTemp.recordCount > 0)
                            newRecord += insertTemp.recordCount;
                    }
                }

                //if (callSP)
                //{
                //    SqlCommand CommandSQL = new SqlCommand();

                //    //add by Tri call sp to insert buyer data
                //    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                //    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;

                //    EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                //    //end add by Tri call sp to insert buyer data

                //    CommandSQL = new SqlCommand();
                //    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                //    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                //    CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                //    CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //    CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 1;
                //    CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = iden.no_cust;

                //    EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);

                //    if (newRecord > 0)
                //    {
                //        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                //        contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(newRecord) + " Pesanan baru dari JD.ID.");

                //        //new StokControllerJob().updateStockMarketPlace(connectionID, iden.DatabasePathErasoft, iden.username);
                //    }
                //}
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> JD_GetOrderByStatusCancel(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            SetupContext(iden.DatabasePathErasoft, iden.username);

            var daysFrom = -1;
            var daysTo = 1;

            while (daysFrom > -13)
            {
                await JD_GetOrderByStatusCancelList3Days(iden, stat, CUST, NAMA_CUST, 1, 0, 0, daysFrom, daysTo);
                daysFrom -= 3;
                daysTo -= 3;
            }

            // tunning untuk tidak duplicate
            var queryStatus = "";
            if (stat == StatusOrder.CANCELLED)
            {
                queryStatus = "\"5\"" + "," + "\"\\\"" + CUST + "\\\"\"" + "," + "\"\\\"" + NAMA_CUST + "\\\"\"";  // "5","\"000011\"","\"Echoboomers\""
            }
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust + "%' and arguments like '%" + queryStatus + "%' and invocationdata like '%JD_GetOrderByStatusCancel%' and statename like '%Enque%' and invocationdata not like '%resi%' and invocationdata not like '%JD_GetOrderByStatusComplete%' ");
            // end tunning untuk tidak duplicate

            return ret;
        }

        public async Task<string> JD_GetOrderByStatusCancelList3Days(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar, int daysFrom, int daysTo)
        {
            //1:Waiting for delivery, 2:Shipped, 3:Waiting_Cancel, 4:Waiting_Refuse, 5:Canceled, 6:Completed, 7:Ready to Ship

            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden.DatabasePathErasoft, iden.username);

            var listOrderId = new List<long>();
            listOrderId.AddRange(GetOrderList(iden, "5", 0, daysFrom));

            string connectionID = Guid.NewGuid().ToString();
            var connIdARF01C = Guid.NewGuid().ToString();

            if (listOrderId.Count > 0)
            {
                var ord = new List_Order_JD { orderIds = new List<string>() };
                string order_10 = "";
                int i = 1;
                foreach (var id in listOrderId)
                {
                    order_10 += id.ToString() + ",";
                    i++;
                    if (i >= 10)
                    {
                        i = 0;
                        order_10 = order_10.Substring(0, order_10.Length - 1);
                        ord.orderIds.Add(order_10);
                        order_10 = "";
                    }
                }
                if (!string.IsNullOrEmpty(order_10))
                {
                    order_10 = order_10.Substring(0, order_10.Length - 1);
                    ord.orderIds.Add(order_10);
                }

                bool callSP = false;
                int cancelRecord = 0;
                foreach (var listOrder in ord.orderIds)
                {
                    var insertTemp = GetOrderDetail(iden, listOrder, iden.no_cust, connIdARF01C, connectionID);
                    if (insertTemp.status == 1)
                    {
                        callSP = true;
                        if (insertTemp.recordCount > 0)
                            cancelRecord += insertTemp.recordCount;
                    }
                }

                //if (cancelRecord > 0)
                //{
                //    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                //    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(cancelRecord) + " Pesanan dari JD.ID dibatalkan.");

                //    new StokControllerJob().updateStockMarketPlace(connectionID, iden.DatabasePathErasoft, iden.username);
                //}

                //if (callSP)
                //{
                //    SqlCommand CommandSQL = new SqlCommand();

                //    //add by Tri call sp to insert buyer data
                //    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                //    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;

                //    EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                //    //end add by Tri call sp to insert buyer data

                //    CommandSQL = new SqlCommand();
                //    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                //    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                //    CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                //    CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //    CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 1;
                //    CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                //    CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = iden.no_cust;

                //    EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);

                //    if (newRecord > 0)
                //    {
                //        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                //        contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(newRecord) + " Pesanan baru dari JD.ID.");

                //        new StokControllerJob().updateStockMarketPlace(connectionID, iden.DatabasePathErasoft, iden.username);
                //    }
                //}
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> JD_GetOrderByStatusComplete(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            string ret = "";
            SetupContext(iden.DatabasePathErasoft, iden.username);

            var daysFrom = -1;
            var daysTo = 1;

            while (daysFrom > -13)
            {
                await JD_GetOrderByStatusCompleteList3Days(iden, stat, CUST, NAMA_CUST, 1, 0, 0, daysFrom, daysTo);
                daysFrom -= 3;
                daysTo -= 3;
            }

            // tunning untuk tidak duplicate
            var queryStatus = "";
            if (stat == StatusOrder.COMPLETED)
            {
                queryStatus = "\"6\"" + "," + "\"\\\"" + CUST + "\\\"\"" + "," + "\"\\\"" + NAMA_CUST + "\\\"\"";  // "6","\"000011\"","\"Echoboomers\""
            }
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust + "%' and arguments like '%" + queryStatus + "%' and invocationdata like '%JD_GetOrderByStatusComplete%' and statename like '%Enque%' and invocationdata not like '%resi%' and invocationdata not like '%JD_GetOrderByStatusCancel%' ");
            // end tunning untuk tidak duplicate

            return ret;
        }


        public async Task<string> JD_GetOrderByStatusCompleteList3Days(JDIDAPIDataJob iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar, int daysFrom, int daysTo)
        {
            //1:Waiting for delivery, 2:Shipped, 3:Waiting_Cancel, 4:Waiting_Refuse, 5:Canceled, 6:Completed, 7:Ready to Ship

            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden.DatabasePathErasoft, iden.username);

            var listOrderId = new List<long>();
            listOrderId.AddRange(GetOrderList(iden, "6", 0, daysFrom));

            string connectionID = Guid.NewGuid().ToString();
            var connIdARF01C = Guid.NewGuid().ToString();

            if (listOrderId.Count > 0)
            {
                var ord = new List_Order_JD { orderIds = new List<string>() };
                string order_10 = "";
                int i = 1;
                foreach (var id in listOrderId)
                {
                    order_10 += id.ToString() + ",";
                    i++;
                    if (i >= 10)
                    {
                        i = 0;
                        order_10 = order_10.Substring(0, order_10.Length - 1);
                        ord.orderIds.Add(order_10);
                        order_10 = "";
                    }
                }
                if (!string.IsNullOrEmpty(order_10))
                {
                    order_10 = order_10.Substring(0, order_10.Length - 1);
                    ord.orderIds.Add(order_10);
                }

                bool callSP = false;
                int newRecord = 0;
                foreach (var listOrder in ord.orderIds)
                {
                    var insertTemp = GetOrderDetail(iden, listOrder, iden.no_cust, connIdARF01C, connectionID);
                    if (insertTemp.status == 1)
                    {
                        callSP = true;
                        if (insertTemp.recordCount > 0)
                            newRecord += insertTemp.recordCount;
                    }
                }

            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public void Order_JD(JDIDAPIDataJob data, string uname)
        {
            //1:Waiting for delivery, 2:Shipped, 3:Waiting_Cancel, 4:Waiting_Refuse, 5:Canceled, 6:Completed, 7:Ready to Ship
            var listOrderId = new List<long>();
            SetupContext(data.DatabasePathErasoft, uname);
            listOrderId.AddRange(GetOrderList(data, "1", 1, 1));
            listOrderId.AddRange(GetOrderList(data, "2", 1, 1));
            listOrderId.AddRange(GetOrderList(data, "3", 1, 1));
            listOrderId.AddRange(GetOrderList(data, "4", 1, 1));
            listOrderId.AddRange(GetOrderList(data, "5", 0, 1));
            listOrderId.AddRange(GetOrderList(data, "6", 1, 1));
            string connectionID = Guid.NewGuid().ToString();
            var connIdARF01C = Guid.NewGuid().ToString();

            if (listOrderId.Count > 0)
            {
                var ord = new List_Order_JD { orderIds = new List<string>() };
                string order_10 = "";
                int i = 1;
                foreach (var id in listOrderId)
                {
                    order_10 += id.ToString() + ",";
                    i++;
                    if (i >= 10)
                    {
                        i = 0;
                        order_10 = order_10.Substring(0, order_10.Length - 1);
                        ord.orderIds.Add(order_10);
                        order_10 = "";
                    }
                }
                if (!string.IsNullOrEmpty(order_10))
                {
                    order_10 = order_10.Substring(0, order_10.Length - 1);
                    ord.orderIds.Add(order_10);
                }

                bool callSP = false;
                int newRecord = 0;
                foreach (var listOrder in ord.orderIds)
                {
                    var insertTemp = GetOrderDetail(data, listOrder, data.no_cust, connIdARF01C, connectionID);
                    if (insertTemp.status == 1)
                    {
                        callSP = true;
                        if (insertTemp.recordCount > 0)
                            newRecord += insertTemp.recordCount;
                    }
                }

                if (callSP)
                {
                    SqlCommand CommandSQL = new SqlCommand();

                    //add by Tri call sp to insert buyer data
                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;

                    EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                    //end add by Tri call sp to insert buyer data

                    CommandSQL = new SqlCommand();
                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                    CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                    CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 1;
                    CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                    CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = data.no_cust;

                    EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);

                    if (newRecord > 0)
                    {
                        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(newRecord) + " Pesanan baru dari Lazada.");

                        //new StokControllerJob().updateStockMarketPlace(connectionID, data.DatabasePathErasoft, uname);
                    }
                }
            }
        }

        public List<long> GetOrderList(JDIDAPIDataJob data, string status, int page, int addDays)
        {
            var ret = new List<long>();
            //var mgrApiManager = new JDIDControllerJob();
            //mgrApiManager.AppKey = data.appKey;
            //mgrApiManager.AppSecret = data.appSecret;
            //mgrApiManager.AccessToken = data.accessToken;

            //mgrApiManager.Method = "epi.popOrder.getOrderIdListByCondition";
            //mgrApiManager.ParamJson = "{\"orderStatus\":" + status + ", \"startRow\": " + page * 20 + ", \"bookTimeBegin\": "
            //    + DateTimeOffset.Now.AddDays(-14).ToUnixTimeSeconds() + "}";

            string sMethod = "epi.popOrder.getOrderIdListByCondition";
            string sParamJson = "{\"orderStatus\":" + status + ", \"startRow\": " + page * 20 + ", \"bookTimeBegin\": "
                + DateTimeOffset.Now.AddDays(addDays).AddHours(7).ToUnixTimeSeconds() + "000 }";
            //string sParamJson = "{\"orderStatus\":" + status + ", \"startRow\": " + page * 20 + "}";

            try
            {
                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var retData = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (retData.openapi_code == 0)
                {
                    var listOrderId = JsonConvert.DeserializeObject(retData.openapi_data, typeof(Data_OrderIdsJob)) as Data_OrderIdsJob;
                    if (listOrderId.success)
                    {
                        ret = listOrderId.model;
                        if (listOrderId.model.Count == 20)
                        {
                            var nextOrders = GetOrderList(data, status, page + 1, addDays);
                            ret.AddRange(nextOrders);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return ret;
        }

        public BindingBase GetOrderDetail(JDIDAPIDataJob data, string listOrderIds, string cust, string conn_id_arf01c, string conn_id_order)
        {
            //var ret = new List<long>();
            var ret = new BindingBase();
            ret.status = 0;
            bool adaInsert = false;
            //var mgrApiManager = new JDIDControllerJob();
            //mgrApiManager.AppKey = data.appKey;
            //mgrApiManager.AppSecret = data.appSecret;
            //mgrApiManager.AccessToken = data.accessToken;

            //mgrApiManager.Method = "epi.popOrder.getOrderInfoListForBatch";
            //mgrApiManager.ParamJson = "[" + listOrderIds + "]";

            string sMethod = "epi.popOrder.getOrderInfoListForBatch";
            string sParamJson = "[" + listOrderIds + "]";

            var jmlhNewOrder = 0;
            try
            {
                var response = Call(data.appKey, data.accessToken, data.appSecret, sMethod, sParamJson);
                var retData = JsonConvert.DeserializeObject(response, typeof(JDID_RESJob)) as JDID_RESJob;
                if (retData.openapi_code == 0)
                {
                    var listOrderId = JsonConvert.DeserializeObject(retData.openapi_data, typeof(Data_OrderDetailJob)) as Data_OrderDetailJob;
                    if (listOrderId.success)
                    {
                        var str = "{\"data\":" + listOrderId.model + "}";
                        var listDetails = JsonConvert.DeserializeObject(str, typeof(ModelOrderJob)) as ModelOrderJob;
                        if (listDetails != null)
                        {

                            string insertQ = "INSERT INTO TEMP_ORDER_JD ([ADDRESS_CUSTOMER],[AREA],[BOOKTIME],[CITY],[COUPON_AMOUNT],[CUSTOMER_NAME],";
                            insertQ += "[DELIVERY_ADDR],[DELIVERY_TYPE],[EMAIL],[FREIGHT_AMOUNT],[FULL_CUT_AMMOUNT],[INSTALLMENT_FEE],[ORDER_COMPLETE_TIME],";
                            insertQ += "[ORDER_ID],[ORDER_SKU_NUM],[ORDER_STATE],[ORDER_TYPE],[PAY_SUBTOTAL],[PAYMENT_TYPE],[PHONE],[POSTCODE],[PROMOTION_AMOUNT],";
                            insertQ += "[SENDPAY],[STATE_CUSTOMER],[TOTAL_PRICE],[USER_PIN],[CUST],[USERNAME],[CONN_ID],[KET_CUSTOMER],[KODE_KURIR],[NAMA_KURIR],[NO_RESI],[NAMA_CUST]) VALUES ";

                            string insertOrderItems = "INSERT INTO TEMP_ORDERITEMS_JD ([ORDER_ID],[COMMISSION],[COST_PRICE],[COUPON_AMOUNT],[FULL_CUT_AMMOUNT]";
                            insertOrderItems += ",[HAS_PROMO],[JDPRICE],[PROMOTION_AMOUNT],[SKUID],[SKU_NAME],[SKU_NUMBER],[SPUID],[WEIGHT],[USERNAME],[CONN_ID],[BOOKTIME],[CUST],[NAMA_CUST]) VALUES ";

                            string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                            insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                            insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";

                            var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == cust).Select(p => p.NO_REFERENSI).ToList();
                            string idOrderCancel = ""; //untuk cancel
                            string idOrderComplete = ""; //untuk completed
                            string idOrderRTS = ""; //untuk readytoship
                            int jmlhOrderCancel = 0;
                            int jmlhOrderCompleted = 0;
                            int jmlhOrderReadytoShip = 0;
                            int jmlhOrderNew = 0;

                            foreach (var order in listDetails.data)
                            {
                                //1: waiting for delivery, 2: shipped, 3: Waiting_Cancel, 4: Waiting_Refuse, 5: canceled, 6: Completed, 7: Ready to ship
                                bool doInsert = true;
                                if (OrderNoInDb.Contains(Convert.ToString(order.orderId)) && order.orderState.ToString() == "1")
                                {
                                    doInsert = false;
                                }
                                else if (order.orderState.ToString() == "5") //CANCELED
                                {
                                    if (OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        doInsert = false;
                                        idOrderCancel = idOrderCancel + "'" + order.orderId + "',";
                                        jmlhOrderCancel++;
                                        //tidak ubah status menjadi selesai jika belum diisi faktur
                                        var dsSIT01A = EDB.GetDataSet("CString", "SIT01A", "SELECT NO_REFERENSI, O.NO_BUKTI, O.STATUS_TRANSAKSI FROM SIT01A I INNER JOIN SOT01A O ON I.NO_SO = O.NO_BUKTI WHERE NO_REFERENSI = '" + order.orderId + "'");
                                        if (dsSIT01A.Tables[0].Rows.Count == 0)
                                        {
                                            doInsert = false;
                                        }
                                    }
                                    else
                                    {
                                        //tidak diinput jika order sudah selesai sebelum masuk MO
                                        doInsert = false;
                                    }
                                }
                                else if (order.orderState.ToString() == "6") // COMPLETED
                                {
                                    if (OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        idOrderComplete = idOrderComplete + "'" + order.orderId + "',";                                
                                    }
                                    doInsert = false;
                                }
                                else if (order.orderState.ToString() == "7") // READY TO SHIP
                                {
                                    if (OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        //jmlhOrderReadytoShip++;
                                        doInsert = false;
                                    }
                                    else
                                    {
                                        idOrderRTS = idOrderRTS + "'" + order.orderId + "',";
                                        doInsert = true;
                                    }
                                }
                                else if (order.orderState.ToString() == "3" || order.orderState.ToString() == "4")
                                {
                                    if (!OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        doInsert = false;
                                    }
                                    else {
                                        doInsert = false;
                                    }
                                }

                                if (doInsert)
                                {
                                    ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_ORDER_JD");
                                    ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_ORDERITEMS_JD");

                                    var dtNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    adaInsert = true;
                                    var statusEra = "";
                                    switch (order.orderState.ToString())
                                    {
                                        //1:Waiting for delivery, 2:Shipped, 3:Waiting_Cancel, 4:Waiting_Refuse, 5:Canceled, 6:Completed, 7:Ready to Ship
                                        case "1":
                                            statusEra = "01";
                                            break;
                                        case "2":
                                            statusEra = "03";
                                            break;
                                        case "3":
                                        case "4":
                                        case "5":
                                            statusEra = "11";
                                            break;
                                        case "6":
                                            statusEra = "04";
                                            break;
                                        case "7":
                                            statusEra = "01";
                                            break;
                                        default:
                                            statusEra = "99";
                                            break;
                                    }

                                    var nama = order.customerName != null ? order.customerName.Replace('\'', '`').Replace("'", "") : "";
                                    if (nama.Length > 30)
                                        nama = nama.Substring(0, 30);

                                    var vOrderAddress = order.address != null ? order.address.Replace('\'', '`').Replace("'", "") : "";
                                    var vDeliveryAddress = order.deliveryAddr != null ? order.deliveryAddr.Replace('\'', '`').Replace("'", "") : "";
                                    var vArea = order.area != null ? order.area.Replace('\'', '`').Replace("'", "") : "";
                                    var vCity = order.city != null ? order.city.Replace('\'', '`').Replace("'", "") : "";
                                    var vState = order.state != null ? order.state.Replace('\'', '`').Replace("'", "") : "";
                                    var messageCustomer = order.buyerMessage != null ? order.buyerMessage.Replace("'", "") : "";
                                    var vEmail = order.email != null ? order.email.Replace('\'', '`').Replace("'", "") : "";

                                    //insertQ += "('" + order.address.Replace('\'', '`') + "','" + order.area.Replace('\'', '`') + "','" + DateTimeOffset.FromUnixTimeSeconds(order.bookTime / 1000).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss") + "','" + order.city.Replace('\'', '`') + "'," + order.couponAmount + ",'" + order.customerName + "','";
                                    //var insertQValue = "('" + vOrderAddress + "','" + vArea + "','" + DateTimeOffset.FromUnixTimeSeconds(order.bookTime / 1000).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd hh:mm:ss") + "','" + vCity + "'," + order.couponAmount + ",'" + nama + "','";
                                    var insertQValue = "('" + vOrderAddress + "','" + vArea + "','" + DateTimeOffset.FromUnixTimeSeconds(order.bookTime / 1000).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "','" + vCity + "'," + order.couponAmount + ",'" + nama + "','";
                                    insertQValue += vDeliveryAddress + "'," + order.deliveryType + ",'" + vEmail + "'," + order.freightAmount + "," + order.fullCutAmount + "," + order.installmentFee + ",'" + DateTimeOffset.FromUnixTimeSeconds(order.orderCompleteTime / 1000).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "','";
                                    insertQValue += order.orderId + "'," + order.orderSkuNum + "," + statusEra + "," + order.orderType + "," + order.paySubtotal + "," + order.paymentType + ",'" + order.phone + "','" + order.postCode + "'," + order.promotionAmount + ",'";
                                    insertQValue += order.sendPay + "','" + vState + "'," + order.totalPrice + ",'" + order.userPin + "','" + data.no_cust + "','" + username + "','" + conn_id_order + "', '" + messageCustomer + "', '" + order.carrierCode + "', '" + order.carrierCompany + "', '" + order.expressNo + "', '" + data.nama_cust + "') ,";

                                    var insertOrderItemsValue = "";

                                    if (order.orderSkuinfos != null)
                                    {
                                        foreach (var ordItem in order.orderSkuinfos)
                                        {
                                            insertOrderItemsValue += "('" + order.orderId + "'," + ordItem.commission + "," + ordItem.costPrice + "," + ordItem.couponAmount + "," + ordItem.fullCutAmount + ",";
                                            insertOrderItemsValue += ordItem.hasPromo + "," + ordItem.jdPrice + "," + ordItem.promotionAmount + ",'" + ordItem.skuId + "','" + ordItem.skuName.Replace("'", "") + "',";
                                            insertOrderItemsValue += ordItem.skuNumber + ",'" + ordItem.spuId + "'," + ordItem.weight + ",'" + username + "','" + conn_id_order + "','" + DateTimeOffset.FromUnixTimeSeconds(order.bookTime / 1000).AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "','" + data.no_cust + "','" + data.nama_cust + "') ,";
                                        }
                                    }

                                    var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + vCity + "%'");
                                    var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + vState + "%'");

                                    var kabKot = "3174";//set default value jika tidak ada di db
                                    var prov = "31";//set default value jika tidak ada di db

                                    if (tblProv.Tables[0].Rows.Count > 0)
                                        prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
                                    if (tblKabKot.Tables[0].Rows.Count > 0)
                                        kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();


                                    var vAddress = order.address != null ? order.address.Replace('\'', '`').Replace("'", "") : "";
                                    
                                    var vPostCode = order.postCode != null ? order.postCode.Replace('\'', '`') : "";

                                    //insertPembeli += "('" + order.customerName.Replace('\'', '`') + "','" + order.address.Replace('\'', '`') + "','" + order.phone + "','" + order.email.Replace('\'', '`') + "',0,0,'0','01',";
                                    var insertPembeliValue = "('" + nama + "','" + vAddress + "','" + order.phone + "','" + nama + "',0,0,'0','01',";
                                    insertPembeliValue += "1, 'IDR', '01', '" + vAddress + "', 0, 0, 0, 0, '1', 0, 0, ";
                                    insertPembeliValue += "'FP', '" + dtNow + "', '" + username + "', '" + vPostCode + "', '" + vEmail + "', '" + kabKot + "', '" + prov + "', '" + vCity + "', '" + vState + "', '" + conn_id_arf01c + "') ,";

                                    if (!OrderNoInDb.Contains(Convert.ToString(order.orderId)))
                                    {
                                        if(string.IsNullOrEmpty(idOrderRTS))
                                        {
                                            jmlhNewOrder++;
                                        }
                                        insertQValue = insertQValue.Substring(0, insertQValue.Length - 2);
                                        EDB.ExecuteSQL(username, CommandType.Text, insertQ + insertQValue);


                                        insertOrderItemsValue = insertOrderItemsValue.Substring(0, insertOrderItemsValue.Length - 2);
                                        EDB.ExecuteSQL(username, CommandType.Text, insertOrderItems + insertOrderItemsValue);


                                        insertPembeliValue = insertPembeliValue.Substring(0, insertPembeliValue.Length - 2);
                                        EDB.ExecuteSQL(username, CommandType.Text, insertPembeli + insertPembeliValue);

                                        using (SqlCommand CommandSQL = new SqlCommand())
                                        {
                                            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = conn_id_arf01c;

                                            EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                                        }

                                        using (SqlCommand CommandSQL = new SqlCommand())
                                        {
                                            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = conn_id_order;
                                            CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                                            CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                            CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 1;
                                            CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                                            CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = data.no_cust;

                                            EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);
                                        }
                                    }
                                }
                            }

                            if (adaInsert)
                            {
                                ret.status = 1;

                                if (jmlhNewOrder > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari JD.ID.");

                                    new StokControllerJob().updateStockMarketPlace(conn_id_order, data.DatabasePathErasoft, data.username);
                                }

                            }

                            if (!string.IsNullOrEmpty(idOrderCancel))
                            {
                                idOrderCancel = idOrderCancel.Substring(0, idOrderCancel.Length - 1);
                                var brgAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + conn_id_order + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + idOrderCancel + ") AND STATUS_TRANSAKSI <> '11' AND BRG <> 'NOT_FOUND' AND CUST = '" + cust + "'");
                                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2', STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN (" + idOrderCancel + ") AND STATUS_TRANSAKSI <> '11' AND CUST = '" + cust + "'");
                                if (rowAffected > 0)
                                {
                                    //add by Tri 1 sep 2020, hapus packing list
                                    var delPL = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03B WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN (" + idOrderCancel + ")  AND STATUS_TRANSAKSI = '11' AND CUST = '" + cust + "')");
                                    var delPLDetail = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03C WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN (" + idOrderCancel + ")  AND STATUS_TRANSAKSI = '11' AND CUST = '" + cust + "')");
                                    //end add by Tri 1 sep 2020, hapus packing list
                                    var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN (" + idOrderCancel + ") AND STATUS <> '2' AND ST_POSTING = 'T' AND CUST = '" + cust + "'");
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderCancel) + " Pesanan dari JD.ID dibatalkan.");
                                    new StokControllerJob().updateStockMarketPlace(conn_id_order, data.DatabasePathErasoft, data.username);
                                }
                            }

                            if (!string.IsNullOrEmpty(idOrderComplete))
                            {
                                idOrderComplete = idOrderComplete.Substring(0, idOrderComplete.Length - 1);
                                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI IN (" + idOrderComplete + ") AND STATUS_TRANSAKSI = '03'");
                                jmlhOrderCompleted = jmlhOrderCompleted + rowAffected;
                                if (jmlhOrderCompleted > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderCompleted) + " Pesanan dari JD.ID sudah selesai.");

                                    //add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                    if (!string.IsNullOrEmpty(idOrderComplete))
                                    {
                                        var dateTimeNow = Convert.ToDateTime(DateTime.Now.AddHours(7).ToString("yyyy-MM-dd"));
                                        string sSQLUpdateDatePesananSelesai = "UPDATE SIT01A SET TGL_KIRIM = '" + dateTimeNow + "' WHERE NO_REF IN (" + idOrderComplete + ")";
                                        var resultUpdateDatePesanan = EDB.ExecuteSQL("CString", CommandType.Text, sSQLUpdateDatePesananSelesai);
                                    }
                                    //end add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                }
                            }

                            if (!string.IsNullOrEmpty(idOrderRTS))
                            {
                                idOrderRTS = idOrderRTS.Substring(0, idOrderRTS.Length - 1);
                                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI IN (" + idOrderRTS + ") AND STATUS_TRANSAKSI = '0'");
                                jmlhOrderReadytoShip = jmlhOrderReadytoShip + rowAffected;
                                if (jmlhOrderReadytoShip > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(data.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhOrderReadytoShip) + " Pesanan dari JD.ID Ready To Ship.");
                                }
                                new StokControllerJob().updateStockMarketPlace(conn_id_order, data.DatabasePathErasoft, data.username);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            //return adaInsert;
            ret.recordCount = jmlhNewOrder;
            return ret;
        }

        protected enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4
        }
        protected void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, JDIDAPIDataJob iden, API_LOG_MARKETPLACE data)
        {
            switch (action)
            {
                case api_status.Pending:
                    {
                        var arf01 = ErasoftDbContext.ARF01.Where(p => p.TOKEN == iden.accessToken).FirstOrDefault();
                        var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                        {
                            CUST = arf01 != null ? arf01.CUST : iden.accessToken,
                            CUST_ATTRIBUTE_1 = iden.accessToken,
                            CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                            CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                            CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                            CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                            MARKETPLACE = "JD",
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

        public enum StatusOrder
        {
            PAID = 1,
            SHIPPED = 2,
            IN_CANCEL = 3,
            WAITING_REFUSE = 4,
            CANCELLED = 5,
            COMPLETED = 6,
            READY_TO_SHIP = 7
        }

        #region jdid data class

        public class Model_PromoJob
        {
            public bool success { get; set; }
            public int code { get; set; }
            public string description { get; set; }
            public string order_id { get; set; }
        }

        public class Data_OrderDetailJob
        {
            public string message { get; set; }
            public string model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
        }

        public class Data_ReadyToShip
        {
            public int code { get; set; }
            public bool success { get; set; }
            public string message { get; set; }
        }

        public class Data_PrintLabel
        {
            public int code { get; set; }
            public bool success { get; set; }
            public Model_PrintLabel model { get; set; }
        }

        public class Model_PrintLabel
        {
            public Data_DetailPrintLabel[] data { get; set; }
            public int templateId { get; set; }
        }

        public class Data_DetailPrintLabel
        {
            public string PDF { get; set; }
            public int orderId { get; set; }
        }


        public class ModelOrderJob
        {
            public List<DetailOrder_JDJob> data { get; set; }
        }

        public class DetailOrder_JDJob
        {
            public string address { get; set; }
            public string area { get; set; }
            public long bookTime { get; set; }
            public string city { get; set; }
            public float couponAmount { get; set; }
            public string customerName { get; set; }
            public string deliveryAddr { get; set; }
            public int deliveryType { get; set; }
            public string email { get; set; }
            public float freightAmount { get; set; }
            public float fullCutAmount { get; set; }
            public float installmentFee { get; set; }
            public long orderCompleteTime { get; set; }
            public long orderId { get; set; }
            public long orderSkuNum { get; set; }
            public Orderskuinfo[] orderSkuinfos { get; set; }
            public int orderState { get; set; }
            public int orderType { get; set; }
            public float paySubtotal { get; set; }
            public int paymentType { get; set; }
            public string phone { get; set; }
            public string postCode { get; set; }
            public float promotionAmount { get; set; }
            public string sendPay { get; set; }
            public string state { get; set; }
            public float totalPrice { get; set; }
            public string userPin { get; set; }
            public string buyerMessage { get; set; }
            public string carrierCode { get; set; }
            public string carrierCompany { get; set; }
            public string expressNo { get; set; }
        }

        public class ModelPrintLabel
        {
            //public List<DetailPrintLabel_JD> data { get; set; }
        }



        public class OrderskuinfoJob
        {
            public float commission { get; set; }
            public float costPrice { get; set; }
            public float couponAmount { get; set; }
            public float fullCutAmount { get; set; }
            public int hasPromo { get; set; }
            public float jdPrice { get; set; }
            public float promotionAmount { get; set; }
            public long skuId { get; set; }
            public string skuName { get; set; }
            public int skuNumber { get; set; }
            public long spuId { get; set; }
            public int weight { get; set; }
            public string popSkuId { get; set; }
        }


        public class List_Order_JDJob
        {
            public List<string> orderIds { get; set; }
        }

        public class Data_OrderIdsJob
        {
            public string message { get; set; }
            public List<long> model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
        }

        public class Data_Detail_ProductJob
        {
            public int code { get; set; }
            public string message { get; set; }
            public List<Model_Detail_ProductJob> model { get; set; }
            public bool success { get; set; }
        }

        public class Model_Detail_ProductJob
        {
            public string skuName { get; set; }
            public long skuId { get; set; }
            public object upc { get; set; }
            public object sellerSkuId { get; set; }
            public string weight { get; set; }
            public string netWeight { get; set; }
            public string packLong { get; set; }
            public string packWide { get; set; }
            public string packHeight { get; set; }
            public int piece { get; set; }
            public string mainImgUri { get; set; }
            public int status { get; set; }
            public long spuId { get; set; }
            public string saleAttributeIds { get; set; }
            public dynamic saleAttributeNameMap { get; set; }
            public float jdPrice { get; set; }
            public object maxQuantity { get; set; }
        }

        public class JDIDAPIDataJob
        {
            public string appKey { get; set; }
            public string appSecret { get; set; }
            public string accessToken { get; set; }
            public string no_cust { get; set; }
            public string nama_cust { get; set; }
            public string username { get; set; }
            public string email { get; set; }
            public string DatabasePathErasoft { get; set; }
        }


        public class Data_UpStokJob
        {
            public int code { get; set; }
            public string message { get; set; }
            public dynamic model { get; set; }
            public bool success { get; set; }
        }

        public class Data_UpPriceJob
        {
            public int code { get; set; }
            public string message { get; set; }
            public dynamic model { get; set; }
            public bool success { get; set; }
        }


        public class Data_BrandJob
        {
            public List<Model_BrandJob> model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
        }

        public class Model_BrandJob
        {
            public long id { get; set; }
            public int isForever { get; set; }
            public string logo { get; set; }
            public long shopId { get; set; }
            public int state { get; set; }
            public int brandState { get; set; }
            public long brandId { get; set; }
            public string brandName { get; set; }
        }


        public class JDID_RESJob
        {
            public string openapi_data { get; set; }
            public string error { get; set; }
            public int openapi_code { get; set; }
            public string openapi_msg { get; set; }
        }


        public class DATA_CATJob
        {
            public List<Model_CatJob> model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
        }
        public class Model_CatJob
        {
            public long id { get; set; }
            public int cateState { get; set; }
            public Model_CatJob parentCateVo { get; set; }
            public long cate1Id { get; set; }
            public string cateNameEn { get; set; }
            public long shopId { get; set; }
            public int state { get; set; }
            public long cate3Id { get; set; }
            public int type { get; set; }
            public long cate2Id { get; set; }
            public long cateId { get; set; }
            public string cateName { get; set; }
        }


        public class AttrDataJob
        {
            public int code { get; set; }
            public string message { get; set; }
            public List<Model_AttrJob> model { get; set; }
            public bool success { get; set; }
        }

        public class Model_AttrJob
        {
            public long propertyId { get; set; }
            public int type { get; set; }
            public string name { get; set; }
            public string nameEn { get; set; }
        }

        public class JDID_ATTRIBUTE_OPTJob
        {
            public int code { get; set; }
            public string message { get; set; }
            public Model_OptJob model { get; set; }
            public bool success { get; set; }
        }

        public class Model_OptJob
        {
            public int pageSize { get; set; }
            public int totalCount { get; set; }
            public List<JDOptJob> data { get; set; }
        }

        public class JDOptJob
        {
            public long attributeValueId { get; set; }
            public string name { get; set; }
            public string nameEn { get; set; }
            public long attributeId { get; set; }
        }

        public class Data_ListProdJob
        {
            public Model_ListProdJob model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
        }

        public class Model_ListProdJob
        {
            public int totalNum { get; set; }
            public string _class { get; set; }
            public List<SpuinfovolistJob> spuInfoVoList { get; set; }
            public int pageNum { get; set; }
        }

        public class SpuinfovolistJob
        {
            public long transportId { get; set; }
            public string mainImgUri { get; set; }
            public long spuId { get; set; }
            public string fullCategoryId { get; set; }
            public string _class { get; set; }
            public string fullCategoryName { get; set; }
            public long brandId { get; set; }
            public long warrantyPeriod { get; set; }
            public string description { get; set; }
            public long shopId { get; set; }
            //public int afterSale { get; set; }
            public string spuName { get; set; }
            //public string appDescription { get; set; }
            public int wareStatus { get; set; }
        }
        public class ProductDataJob
        {
            public List<Model_ProductJob> model { get; set; }
            public int code { get; set; }
            public bool success { get; set; }
        }

        public class Model_ProductJob
        {
            public string packWide { get; set; }
            public string weight { get; set; }
            public string mainImgUri { get; set; }
            public long spuId { get; set; }
            public float jdPrice { get; set; }
            public string skuName { get; set; }
            public dynamic saleAttributeNameMap { get; set; }
            public string saleAttributeIds { get; set; }
            public string netWeight { get; set; }
            public long skuId { get; set; }
            public string packHeight { get; set; }
            public string packLong { get; set; }
            public int piece { get; set; }
        }
        public class DataProdJob
        {
            public int code { get; set; }
            public object message { get; set; }
            public List<Model_Product_2Job> model { get; set; }
            public bool success { get; set; }
        }

        public class Model_Product_2Job
        {
            public object wareTypeId { get; set; }
            //public object sellerType { get; set; }
            public object catId { get; set; }
            public object wareStatus { get; set; }
            public string mainImgUri { get; set; }
            //public int shopId { get; set; }
            public long spuId { get; set; }
            public string spuName { get; set; }
            public string fullCategoryId { get; set; }
            public string fullCategoryName { get; set; }
            public string shopName { get; set; }
            public long transportId { get; set; }
            public object propertyRight { get; set; }
            public string brandName { get; set; }
            public string brandLogo { get; set; }
            public List<long> skuIds { get; set; }
            public string appDescription { get; set; }
            public string commonAttributeIds { get; set; }
            public dynamic commonAttributeNameMap { get; set; }
            public long modified { get; set; }
            public object imgUris { get; set; }
            public long brandId { get; set; }
            public object productArea { get; set; }
            public string description { get; set; }
            public int auditStatus { get; set; }
            public int minQuantity { get; set; }
            public int afterSale { get; set; }
            public object crossProductType { get; set; }
            public object clearanceType { get; set; }
            public object taxesType { get; set; }
            public object countryId { get; set; }
        }
        #endregion
    }
}
