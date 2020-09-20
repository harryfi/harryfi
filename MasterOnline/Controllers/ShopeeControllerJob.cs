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
    public class ShopeeControllerJob : Controller
    {
        //AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);//string auth = Base64Encode();
#if AWS
        string shpCallbackUrl = "https://masteronline.co.id/shp/code?user=";
#else
        string shpCallbackUrl = "https://dev.masteronline.co.id/shp/code?user=";
#endif

        protected int MOPartnerID = 841371;
        protected string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
        private MoDbContext MoDbContext { get; set; }
        private ErasoftContext ErasoftDbContext { get; set; }
        private DatabaseSQL EDB;
        private string username;
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

        public ShopeeControllerJob()
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

        protected void SetupContext(ShopeeAPIData data)
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

        [Route("shp/code")]
        [HttpGet]
        public ActionResult ShopeeCode(string user, string shop_id)
        {
            var param = user.Split(new string[] { "_param_" }, StringSplitOptions.None);
            if (param.Count() == 2)
            {
                ShopeeControllerJob.ShopeeAPIData dataSp = new ShopeeControllerJob.ShopeeAPIData()
                {
                    merchant_code = shop_id,
                    DatabasePathErasoft = param[0],
                    no_cust = param[1],
                };
                Task.Run(() => GetTokenShopee(dataSp, true)).Wait();
            }
            return View("ShopeeAuth");
        }

        public async Task<BindingBase> GetItemsList(ShopeeAPIData iden, int IdMarket, int page, int recordCount)
        {
            //int MOPartnerID = 841371;
            //string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            //string ret = "";
            var ret = new BindingBase
            {
                status = 0,
                recordCount = recordCount,
            };

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = seconds.ToString() + "_" + page,
                REQUEST_ACTION = "Get Item List",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/items/get";

            ShopeeGetItemListData HttpBody = new ShopeeGetItemListData
            {
                partner_id = MOPartnerID, //MasterOnline Partner ID
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                pagination_offset = page * 10,
                pagination_entries_per_page = 10,

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
                    var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetItemListResult)) as ShopeeGetItemListResult;
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    //add 13 Feb 2019, tuning
                    var stf02h_local = ErasoftDbContext.STF02H.Where(m => m.IDMARKET == IdMarket).ToList();
                    var tempBrg_local = ErasoftDbContext.TEMP_BRG_MP.Where(m => m.IDMARKET == IdMarket).ToList();
                    //end add 13 Feb 2019, tuning
                    ret.status = 1;
                    if (listBrg.items.Length == 10)
                        ret.message = (page + 1).ToString();
                    foreach (var item in listBrg.items)
                    {
                        if (item.status.ToUpper() != "BANNED" && item.status.ToUpper() != "DELETED")
                        {
                            string kdBrg = string.IsNullOrEmpty(item.item_sku) ? item.item_id.ToString() : item.item_sku;
                            string brgMp = item.item_id.ToString() + ";0";
                            //change 13 Feb 2019, tuning
                            //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.ToUpper().Equals(brgMp.ToUpper()) && t.IDMARKET == IdMarket).FirstOrDefault();
                            //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(brgMp) && t.IDMARKET == IdMarket).FirstOrDefault();
                            var tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
                            var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
                            //end change 13 Feb 2019, tuning

                            if ((tempbrginDB == null && brgInDB == null) || item.variations.Length > 1)
                            {
                                //var getDetailResult = await GetItemDetail(iden, item.item_id);
                                var getDetailResult = await GetItemDetail(iden, item.item_id, tempBrg_local, stf02h_local, IdMarket);
                                if (getDetailResult.status == 1)
                                {
                                    ret.recordCount += getDetailResult.recordCount;
                                }
                                else
                                {
                                    currentLog.REQUEST_EXCEPTION = ret.message;
                                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                                }
                            }
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
        public async Task<BindingBase> GetItemDetail(ShopeeAPIData iden, long item_id, List<TEMP_BRG_MP> tempBrg_local, List<STF02H> stf02h_local, int IdMarket)
        {
            //    int MOPartnerID = 841371;
            //    string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            //string ret = "";
            var ret = new BindingBase
            {
                status = 0,
                recordCount = 0,
            };

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/item/get";

            ShopeeGetItemDetailData HttpBody = new ShopeeGetItemDetailData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                item_id = item_id
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
                ret.message = ex.Message;
            }

            if (responseFromServer != null)
            {
                try
                {
                    var detailBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetItemDetailResult)) as ShopeeGetItemDetailResult;

                    //string IdMarket = ErasoftDbContext.ARF01.Where(c => c.Sort1_Cust.Equals(iden.merchant_code)).FirstOrDefault().RecNum.ToString();
                    string cust = ErasoftDbContext.ARF01.Where(c => c.Sort1_Cust == iden.merchant_code).FirstOrDefault().CUST.ToString();
                    string categoryCode = detailBrg.item.category_id.ToString();
                    string categoryName = MoDbContext.CategoryShopee.Where(p => p.CATEGORY_CODE == categoryCode).FirstOrDefault().CATEGORY_NAME;
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

                    if (detailBrg.item.has_variation)
                    {
                        //insert brg induk
                        string brgMpInduk = Convert.ToString(detailBrg.item.item_id) + ";";
                        //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.ToUpper().Equals(brgMpInduk.ToUpper()) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
                        //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(brgMpInduk) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
                        var tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMpInduk.ToUpper()).FirstOrDefault();
                        var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMpInduk.ToUpper()).FirstOrDefault();
                        if (tempbrginDB == null && brgInDB == null)
                        {
                            //ret.recordCount++;
                            var ret1 = await proses_Item_detail(detailBrg, categoryCode, categoryName, cust, IdMarket, brgMpInduk, detailBrg.item.name, detailBrg.item.variations[0].status, detailBrg.item.original_price, string.IsNullOrEmpty(detailBrg.item.item_sku) ? brgMpInduk : detailBrg.item.item_sku, 1, "", iden);
                            ret.recordCount += ret1.status;
                        }
                        else if (brgInDB != null)
                        {
                            brgMpInduk = brgInDB.BRG;
                        }
                        //end insert brg induk

                        foreach (var item in detailBrg.item.variations)
                        {
                            sellerSku = item.variation_sku;
                            if (string.IsNullOrEmpty(sellerSku))
                            {
                                sellerSku = item.variation_id.ToString();
                            }
                            string brgMp = Convert.ToString(detailBrg.item.item_id) + ";" + Convert.ToString(item.variation_id);
                            //tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.ToUpper().Equals(brgMp.ToUpper()) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
                            //brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(brgMp) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
                            tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
                            brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
                            if (tempbrginDB == null && brgInDB == null)
                            {
                                //ret.recordCount++;
                                var ret2 = await proses_Item_detail(detailBrg, categoryCode, categoryName, cust, IdMarket, brgMp, detailBrg.item.name + " " + item.name, item.status, item.original_price, sellerSku, 2, brgMpInduk, iden);
                                ret.recordCount += ret2.status;
                            }
                        }
                    }
                    else
                    {
                        sellerSku = string.IsNullOrEmpty(detailBrg.item.item_sku) ? detailBrg.item.item_id.ToString() : detailBrg.item.item_sku;
                        //ret.recordCount++;
                        var ret0 = await proses_Item_detail(detailBrg, categoryCode, categoryName, cust, IdMarket, Convert.ToString(detailBrg.item.item_id) + ";0", detailBrg.item.name, detailBrg.item.status, detailBrg.item.original_price, sellerSku, 0, "", iden);
                        ret.recordCount += ret0.status;
                    }
                }
                catch (Exception ex2)
                {
                    ret.message = ex2.Message;
                }
            }
            return ret;
        }
        protected async Task<BindingBase> proses_Item_detail(ShopeeGetItemDetailResult detailBrg, string categoryCode, string categoryName, string cust, int IdMarket, string barang_id, string barang_name, string barang_status, float barang_price, string sellerSku, int typeBrg, string kdBrgInduk, ShopeeAPIData iden)
        {
            // typeBrg : 0 = barang tanpa varian; 1 = barang induk; 2 = barang varian
            var ret = new BindingBase();
            string brand = "OEM";
            string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, NAMA3, BERAT, PANJANG, LEBAR, TINGGI, CUST, ";
            sSQL += "Deskripsi, IDMARKET, HJUAL, HJUAL_MP, DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, KODE_BRG_INDUK, TYPE, AVALUE_45,";
            sSQL += "ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
            sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20, ";
            sSQL += "ACODE_21, ANAME_21, AVALUE_21, ACODE_22, ANAME_22, AVALUE_22, ACODE_23, ANAME_23, AVALUE_23, ACODE_24, ANAME_24, AVALUE_24, ACODE_25, ANAME_25, AVALUE_25, ACODE_26, ANAME_26, AVALUE_26, ACODE_27, ANAME_27, AVALUE_27, ACODE_28, ANAME_28, AVALUE_28, ACODE_29, ANAME_29, AVALUE_29, ACODE_30, ANAME_30, AVALUE_30) VALUES ";

            string namaBrg = barang_name;
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
            if (detailBrg.item.images.Count() > 0)
            {
                urlImage = detailBrg.item.images[0];
                if (detailBrg.item.images.Count() >= 2)
                {
                    urlImage2 = detailBrg.item.images[1];
                    if (detailBrg.item.images.Count() >= 3)
                    {
                        urlImage3 = detailBrg.item.images[2];
                    }
                }
            }
            sSQL += "('" + barang_id + "' , '" + sellerSku + "' , '" + nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' , '" + nama3.Replace('\'', '`') + "' ,";
            sSQL += detailBrg.item.weight * 1000 + "," + detailBrg.item.package_length + "," + detailBrg.item.package_width + "," + detailBrg.item.package_height + ", '";
            sSQL += cust + "' , '" + detailBrg.item.description.Replace('\'', '`') + "' , " + IdMarket + " , " + barang_price + " , " + barang_price;
            sSQL += " , " + (barang_status.Contains("NORMAL") ? "1" : "0") + " , '" + categoryCode + "' , '" + categoryName + "' , '" + "REPLACE_MEREK" + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "'";
            //add kode brg induk dan type brg
            sSQL += ", '" + (typeBrg == 2 ? kdBrgInduk : "") + "' , '" + (typeBrg == 1 ? "4" : "3") + "'";
            //end add kode brg induk dan type brg
            sSQL += ",'" + (namaBrg.Length > 250 ? namaBrg.Substring(0, 250) : namaBrg) + "'"; //request by Calvin, 19 maret 2019

            //var attributeShopee = MoDbContext.AttributeShopee.Where(a => a.CATEGORY_CODE == categoryCode).FirstOrDefault();
            var GetAttributeShopee = await GetAttributeToList(iden, categoryCode, categoryName);
            var attributeShopee = GetAttributeShopee.attributes.FirstOrDefault();

            #region set attribute
            if (attributeShopee != null)
            {
                string attrVal = "";
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_1))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_1.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`').Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_1 + "' , '" + attributeShopee.ANAME_1.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_2))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_2.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_2 + "' , '" + attributeShopee.ANAME_2.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }

                if (!string.IsNullOrEmpty(attributeShopee.ACODE_3))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_3.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_3 + "' , '" + attributeShopee.ANAME_3.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_4))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_4.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_4 + "' , '" + attributeShopee.ANAME_4.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }

                if (!string.IsNullOrEmpty(attributeShopee.ACODE_5))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_5.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_5 + "' , '" + attributeShopee.ANAME_5.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_6))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_6.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_6 + "' , '" + attributeShopee.ANAME_6.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_7))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_7.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_7 + "' , '" + attributeShopee.ANAME_7.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_8))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_8.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_8 + "' , '" + attributeShopee.ANAME_8.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_9))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_9.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_9 + "' , '" + attributeShopee.ANAME_9.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_10))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_10.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_10 + "' , '" + attributeShopee.ANAME_10.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_11))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_11.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_11 + "' , '" + attributeShopee.ANAME_11.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_12))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_12.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_12 + "' , '" + attributeShopee.ANAME_12.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_13))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_13.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_13 + "' , '" + attributeShopee.ANAME_13.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_14))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_14.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_14 + "' , '" + attributeShopee.ANAME_14.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_15))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_15.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_15 + "' , '" + attributeShopee.ANAME_15.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_16))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_16.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_16 + "' , '" + attributeShopee.ANAME_16.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_17))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_17.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_17 + "' , '" + attributeShopee.ANAME_17.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_18))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_18.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_18 + "' , '" + attributeShopee.ANAME_18.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_19))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_19.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_19 + "' , '" + attributeShopee.ANAME_19.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_20))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_20.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_20 + "' , '" + attributeShopee.ANAME_20.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_21))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_21.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_21 + "' , '" + attributeShopee.ANAME_21.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_22))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_22.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_22 + "' , '" + attributeShopee.ANAME_22.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_23))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_23.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_23 + "' , '" + attributeShopee.ANAME_23.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_24))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_24.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_24 + "' , '" + attributeShopee.ANAME_24.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_25))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_25.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_25 + "' , '" + attributeShopee.ANAME_25.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_26))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_26.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_26 + "' , '" + attributeShopee.ANAME_26.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_27))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_27.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_27 + "' , '" + attributeShopee.ANAME_27.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_28))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_28.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_28 + "' , '" + attributeShopee.ANAME_28.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_29))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_29.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_29 + "' , '" + attributeShopee.ANAME_29.Replace("\'", "\'\'") + "' , '" + attrVal + "'";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', ''";
                }
                if (!string.IsNullOrEmpty(attributeShopee.ACODE_30))
                {
                    foreach (var property in detailBrg.item.attributes)
                    {
                        string tempCode = property.attribute_id.ToString();
                        if (attributeShopee.ACODE_30.ToUpper().Equals(tempCode.ToString()))
                        {
                            if (!string.IsNullOrEmpty(attrVal))
                                attrVal += ";";
                            attrVal += Convert.ToString(property.attribute_value).Replace('\'', '`');
                            if (property.attribute_name.ToUpper() == "MEREK")
                            {
                                brand = property.attribute_value;
                            }
                        }
                    }
                    sSQL += ", '" + attributeShopee.ACODE_30 + "' , '" + attributeShopee.ANAME_30.Replace("\'", "\'\'") + "' , '" + attrVal + "')";
                    attrVal = "";
                }
                else
                {
                    sSQL += ", '', '', '')";
                }
            }
            #endregion
            sSQL = sSQL.Replace("REPLACE_MEREK", brand);
            var retRec = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
            ret.status = retRec;
            return ret;
        }

        public async Task<string> GetCategory(ShopeeAPIData iden)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //ganti
            string urll = "https://partner.shopeemobile.com/api/v1/item/categories/get";

            //ganti
            ShopeeGetCategoryData HttpBody = new ShopeeGetCategoryData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetCategoryResult)) as ShopeeGetCategoryResult;
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
        protected void RecursiveInsertCategory(SqlCommand oCommand, ShopeeGetCategoryCategory[] categories, int parent, int master_category_code)
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

        public async Task<ATTRIBUTE_SHOPEE_AND_OPT> GetAttributeToList(ShopeeAPIData iden, string category_code, string category_name)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            ATTRIBUTE_SHOPEE_AND_OPT ret = new ATTRIBUTE_SHOPEE_AND_OPT();

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //ganti
            string urll = "https://partner.shopeemobile.com/api/v1/item/attributes/get";

            //ganti
            ShopeeGetAttributeData HttpBody = new ShopeeGetAttributeData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetAttributeResult)) as ShopeeGetAttributeResult;
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

        public async Task<ATTRIBUTE_SHOPEE_AND_OPT> GetAttributeToList(ShopeeAPIData iden, CATEGORY_SHOPEE category)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            ATTRIBUTE_SHOPEE_AND_OPT ret = new ATTRIBUTE_SHOPEE_AND_OPT();

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //ganti
            string urll = "https://partner.shopeemobile.com/api/v1/item/attributes/get";

            //ganti
            ShopeeGetAttributeData HttpBody = new ShopeeGetAttributeData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetAttributeResult)) as ShopeeGetAttributeResult;
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

        //add by fauzi 21 Februari 2020 fungsi untuk ambil token dari api shopee agar tgl expired dari akun marketplace termonitor.
        [AutomaticRetry(Attempts = 3)]
        [Queue("2_get_token")]
        public async Task<string> GetTokenShopee(ShopeeAPIData dataAPI, bool bForceRefresh)
        {
            string ret = "";
            DateTime dateNow = DateTime.UtcNow.AddHours(7);
            bool TokenExpired = false;
            if (!string.IsNullOrWhiteSpace(dataAPI.tgl_expired.ToString()))
            {
                if (dateNow >= dataAPI.tgl_expired)
                {
                    TokenExpired = true;
                }
            }
            else
            {
                TokenExpired = true;
            }

            if (TokenExpired || bForceRefresh)
            {
                int MOPartnerID = 841371;
                string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";

                SetupContext(dataAPI);

                long seconds = CurrentTimeSecond();
                DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

                MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
                {
                    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
                    REQUEST_ACTION = "Refresh Token Shopee", //ganti
                    REQUEST_DATETIME = milisBack,
                    REQUEST_ATTRIBUTE_1 = dataAPI.merchant_code,
                    REQUEST_STATUS = "Pending",
                };

                //ganti
                string urll = "https://partner.shopeemobile.com/api/v1/shop/get_partner_shop";

                //ganti
                ShopeeGetTokenShop HttpBody = new ShopeeGetTokenShop
                {
                    partner_id = MOPartnerID,
                    shopid = Convert.ToInt32(dataAPI.merchant_code),
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
                    currentLog.REQUEST_RESULT = "Process Get API Token Shopee";
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
                        var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetTokenShopResult)) as ShopeeGetTokenShopResult;
                        if (result.error == null && !string.IsNullOrWhiteSpace(result.ToString()))
                        {
                            if (result.authed_shops.Length > 0)
                            {
                                foreach (var item in result.authed_shops)
                                {
                                    if (item.shopid.ToString() == dataAPI.merchant_code.ToString())
                                    {
                                        var dateExpired = DateTimeOffset.FromUnixTimeSeconds(item.expire_time).UtcDateTime.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                                        DatabaseSQL EDB = new DatabaseSQL(dataAPI.DatabasePathErasoft);
                                        var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '1', Sort1_Cust = '" + dataAPI.merchant_code + "', TGL_EXPIRED = '" + dateExpired + "' WHERE CUST = '" + dataAPI.no_cust + "'");
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

        public async Task<string> GetAttribute(ShopeeAPIData iden)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";
            var categories = MoDbContext.CategoryShopee.Where(p => p.IS_LAST_NODE.Equals("1")).ToList();
            foreach (var category in categories)
            {
                long seconds = CurrentTimeSecond();
                DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

                //ganti
                string urll = "https://partner.shopeemobile.com/api/v1/item/attributes/get";

                //ganti
                ShopeeGetAttributeData HttpBody = new ShopeeGetAttributeData
                {
                    partner_id = MOPartnerID,
                    shopid = Convert.ToInt32(iden.merchant_code),
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
                        var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetAttributeResult)) as ShopeeGetAttributeResult;
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
                                var AttributeInDb = MoDbContext.AttributeShopee.Where(p => p.CATEGORY_CODE.ToUpper().Equals(category.CATEGORY_CODE)).ToList();
                                //cek jika belum ada di database, insert
                                var cari = AttributeInDb;
                                if (cari.Count() == 0)
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
                                            var AttributeOptInDb = MoDbContext.AttributeOptShopee.AsNoTracking().ToList();
                                            foreach (var option in attribs.options)
                                            {
                                                var cariOpt = AttributeOptInDb.Where(p => p.ACODE == Convert.ToString(attribs.attribute_id) && p.OPTION_VALUE == option);
                                                if (cariOpt.Count() == 0)
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
            return ret;
        }
        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrderByStatus(ShopeeAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            long seconds = CurrentTimeSecond();
            //change by nurul 10/12/2019, change create_time_from
            //long timestamp7Days = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
            long timestamp7Days = (long)DateTimeOffset.UtcNow.AddDays(-3).ToUnixTimeSeconds();
            //change add by nurul 10/12/2019, change create_time_from

            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/orders/get";

            ShopeeGetOrderByStatusData HttpBody = new ShopeeGetOrderByStatusData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                pagination_offset = page,
                pagination_entries_per_page = 50,
                create_time_from = timestamp7Days,
                create_time_to = seconds,
                order_status = stat.ToString()
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

            if (responseFromServer != null)
            {
                //try
                //{
                var listOrder = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetOrderByStatusResult)) as ShopeeGetOrderByStatusResult;
                if(listOrder.orders != null)
                {
                    if(listOrder.orders.Length > 0)
                    {
                        if (stat == StatusOrder.READY_TO_SHIP || stat == StatusOrder.UNPAID)
                        {
                            string[] ordersn_list = listOrder.orders.Select(p => p.ordersn).ToArray();
                            //add by calvin 4 maret 2019, filter
                            var dariTgl = DateTimeOffset.UtcNow.AddDays(-30).DateTime;
                            var SudahAdaDiMO = ErasoftDbContext.SOT01A.Where(p => p.USER_NAME == "Auto Shopee" && p.CUST == CUST && p.TGL >= dariTgl).Select(p => p.NO_REFERENSI).ToList();
                            //end add by calvin
                            var filtered = ordersn_list.Where(p => !SudahAdaDiMO.Contains(p));
                            if (filtered.Count() > 0)
                            {
                                await GetOrderDetails(iden, filtered.ToArray(), connID, CUST, NAMA_CUST, stat);
                                jmlhNewOrder = filtered.Count();
                            }

                            //add by calvin 29 mei 2019
                            if (stat == StatusOrder.READY_TO_SHIP)
                            {
                                string ordersn = "";
                                var filteredSudahAda = ordersn_list.Where(p => SudahAdaDiMO.Contains(p));
                                foreach (var item in filteredSudahAda)
                                {
                                    ordersn = ordersn + "'" + item + "',";
                                }
                                if (!string.IsNullOrEmpty(ordersn))
                                {
                                    ordersn = ordersn.Substring(0, ordersn.Length - 1);
                                    var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '0'");
                                    if (rowAffected > 0)
                                    {
                                        jmlhPesananDibayar += rowAffected;
                                    }
                                }
                            }
                            //end add by calvin 29 mei 2019

                            if (listOrder.more)
                            {
                                //add by Tri 4 Mei 2020, update stok di jalankan per batch karena batch berikutnya akan memiliki connID yg berbeda
                                new StokControllerJob().updateStockMarketPlace(connID, iden.DatabasePathErasoft, iden.username);
                                //end add by Tri 4 Mei 2020, update stok di jalankan per batch karena batch berikutnya akan memiliki connID yg berbeda

                                await GetOrderByStatus(iden, stat, CUST, NAMA_CUST, page + 50, jmlhNewOrder, jmlhPesananDibayar);
                            }
                            else
                            {
                                if (jmlhNewOrder > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari Shopee.");
                                    new StokControllerJob().updateStockMarketPlace(connID, iden.DatabasePathErasoft, iden.username);
                                }
                                if (jmlhPesananDibayar > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhPesananDibayar) + " Pesanan terbayar dari Shopee.");
                                }
                            }
                        }
                    }
                }
               
                //}
                //catch (Exception ex2)
                //{
                //}
            }


            // tunning untuk tidak duplicate
            var queryStatus = "";
            if (stat == StatusOrder.UNPAID)
            {
                //queryStatus = "\"}\"" + "," + "\"6\"" + "," + "\"";
                queryStatus = "\\\"}\"" + "," + "\"6\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","6","\"000003\""
            }
            else if (stat == StatusOrder.READY_TO_SHIP)
            {
                //queryStatus = "\"}\"" + "," + "\"3\"" + "," + "\"";
                queryStatus = "\\\"}\"" + "," + "\"3\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","3","\"000003\""
            }
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + iden.no_cust + "%' and arguments like '%" + queryStatus + "%' and invocationdata like '%shopee%' and invocationdata like '%GetOrderByStatus%' and statename like '%Enque%' and invocationdata not like '%resi%' and invocationdata not like '%GetOrderByStatusCompleted%' and invocationdata not like '%GetOrderByStatusCancelled%' ");
            // end tunning untuk tidak duplicate

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrderByStatusCancelled(ShopeeAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            long seconds = CurrentTimeSecond();
            long timeStampFrom = (long)DateTimeOffset.UtcNow.AddDays(-10).ToUnixTimeSeconds();
            long timeStampTo = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/orders/get";

            ShopeeGetOrderByStatusData HttpBody = new ShopeeGetOrderByStatusData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                pagination_offset = page,
                pagination_entries_per_page = 50,
                create_time_from = timeStampFrom,
                create_time_to = timeStampTo,
                order_status = stat.ToString()
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

            if (responseFromServer != null)
            {
                //try
                //{
                var listOrder = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetOrderByStatusResult)) as ShopeeGetOrderByStatusResult;

                if (listOrder.orders != null)
                {
                    if (listOrder.orders.Length > 0)
                    {
                        string[] ordersn_list = listOrder.orders.Where(p => p.order_status == stat.ToString()).Select(p => p.ordersn).ToArray();
                        string ordersn = "";
                        foreach (var item in ordersn_list)
                        {
                            ordersn = ordersn + "'" + item + "',";
                        }
                        if (ordersn_list.Count() > 0)
                        {
                            ordersn = ordersn.Substring(0, ordersn.Length - 1);
                            var brgAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID) SELECT DISTINCT BRG,'" + connID + "' AS CONN_ID FROM SOT01A A INNER JOIN SOT01B B ON A.NO_BUKTI = B.NO_BUKTI WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI <> '11' AND BRG <> 'NOT_FOUND' AND CUST = '" + CUST + "'");
                            var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2', STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI <> '11' AND CUST = '" + CUST + "'");
                            if (rowAffected > 0)
                            {
                                //add by Tri 1 sep 2020, hapus packing list
                                var delPL = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03B WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN (" + ordersn + ")  AND STATUS_TRANSAKSI = '11' AND CUST = '" + CUST + "')");
                                var delPLDetail = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03C WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN (" + ordersn + ")  AND STATUS_TRANSAKSI = '11' AND CUST = '"+CUST+"')");
                                //end add by Tri 1 sep 2020, hapus packing list
                                //add by Tri 4 Des 2019, isi cancel reason
                                var sSQL1 = "";
                                var sSQL2 = "SELECT * INTO #TEMP FROM (";
                                var listReason = new Dictionary<string, string>();
                                //if (ordersn_list.Count() > 50)
                                //{
                                //    var order50 = new string[50];
                                //    int i = 0;
                                //    foreach (var order in listOrder.orders)
                                //    {
                                //        order50[i] = order.ordersn;
                                //        i++;
                                //        if (i > 50 || order == listOrder.orders.Last())
                                //        {
                                //            var list2 = await GetOrderDetailsForCancelReason(iden, ordersn_list);
                                //            listReason = AddDictionary(listReason, list2);
                                //            i = 0;
                                //            order50 = new string[50];
                                //        }
                                //    }
                                //}
                                //else
                                //{
                                listReason = await GetOrderDetailsForCancelReason(iden, ordersn_list);
                                //}
                                foreach (var order in listOrder.orders)
                                {
                                    string reasonValue;
                                    if (listReason.TryGetValue(order.ordersn, out reasonValue))
                                    {
                                        if (!string.IsNullOrEmpty(sSQL1))
                                        {
                                            sSQL1 += " UNION ALL ";
                                        }
                                        sSQL1 += " SELECT '" + order.ordersn + "' NO_REFERENSI, '" + listReason[order.ordersn] + "' ALASAN ";
                                    }
                                }
                                sSQL2 += sSQL1 + ") as qry; INSERT INTO SOT01D (NO_BUKTI, CATATAN_1, USERNAME) ";
                                sSQL2 += " SELECT A.NO_BUKTI, ALASAN, 'AUTO_SHOPEE' FROM SOT01A A INNER JOIN #TEMP T ON A.NO_REFERENSI = T.NO_REFERENSI ";
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

                                //var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN (" + ordersn + ") AND STATUS <> '2' AND ST_POSTING = 'T'");
                                string qry_Retur = "SELECT F.NO_REF FROM SIT01A F LEFT JOIN SIT01A R ON R.NO_REF = F.NO_BUKTI AND R.JENIS_FORM = '3' AND F.JENIS_FORM = '2' ";
                                qry_Retur += "WHERE F.NO_REF IN (" + ordersn + ") AND ISNULL(R.NO_BUKTI, '') = '' AND F.CUST = '" + CUST + "'";
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
                                new StokControllerJob().updateStockMarketPlace(connID, iden.DatabasePathErasoft, iden.username);
                            }
                            jmlhNewOrder = jmlhNewOrder + rowAffected;
                            if (listOrder.more)
                            {
                                await GetOrderByStatusCancelled(iden, stat, CUST, NAMA_CUST, page + 50, jmlhNewOrder);
                            }
                            else
                            {
                                if (jmlhNewOrder > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhNewOrder) + " Pesanan dari Shopee dibatalkan.");
                                }
                            }
                        }
                    }
                }
                //}
                //catch (Exception ex2)
                //{
                //}
            }

            // tunning untuk tidak duplicate
            var queryStatus = "\\\"}\"" + "," + "\"2\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","2","\"000003\""
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + queryStatus + "%' and arguments like '%" + iden.no_cust + "%' and invocationdata like '%shopee%' and invocationdata like '%GetOrderByStatusCancelled%' and statename like '%Enque%' and invocationdata not like '%resi%'");
            // end tunning untuk tidak duplicate

            return ret;
        }

        //public Dictionary<TKey, TValue> AddDictionary<TKey, TValue>(this Dictionary<TKey, TValue> target, Dictionary<TKey, TValue> source)
        //{
        //    if (source == null) throw new ArgumentNullException("source");
        //    if (target == null) throw new ArgumentNullException("target");

        //    foreach (var keyValuePair in source)
        //    {
        //        target.Add(keyValuePair.Key, keyValuePair.Value);
        //    }

        //    return target;
        //}


        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrderByStatusCompleted(ShopeeAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";
            string connID = Guid.NewGuid().ToString();
            SetupContext(iden);

            long seconds = CurrentTimeSecond();
            //long timeStampFrom = (long)DateTimeOffset.UtcNow.AddDays(-10).ToUnixTimeSeconds();
            long timeStampFrom = (long)DateTimeOffset.UtcNow.AddDays(-14).ToUnixTimeSeconds();//change by Tri 25 aug 2020, pesanan bisa selesai lebih dari 10hari
            long timeStampTo = (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/orders/get";

            ShopeeGetOrderByStatusData HttpBody = new ShopeeGetOrderByStatusData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                pagination_offset = page,
                pagination_entries_per_page = 50,
                create_time_from = timeStampFrom,
                create_time_to = timeStampTo,
                order_status = stat.ToString()
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

            if (responseFromServer != null)
            {
                //try
                //{
                var listOrder = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetOrderByStatusResult)) as ShopeeGetOrderByStatusResult;
                if(listOrder.orders != null)
                {
                    if(listOrder.orders.Length > 0)
                    {
                        string[] ordersn_list = listOrder.orders.Where(p => p.order_status == stat.ToString()).Select(p => p.ordersn).ToArray();
                        string ordersn = "";
                        foreach (var item in ordersn_list)
                        {
                            ordersn = ordersn + "'" + item + "',";
                        }
                        if (ordersn_list.Count() > 0)
                        {
                            ordersn = ordersn.Substring(0, ordersn.Length - 1);
                            var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '03'");
                            jmlhNewOrder = jmlhNewOrder + rowAffected;
                            if (listOrder.more)
                            {
                                await GetOrderByStatusCompleted(iden, stat, CUST, NAMA_CUST, page + 50, jmlhNewOrder);
                            }
                            else
                            {
                                if (jmlhNewOrder > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("" + Convert.ToString(jmlhNewOrder) + " Pesanan dari Shopee sudah selesai.");
                                }
                            }
                        }
                    }
                }
                

                //}
                //catch (Exception ex2)
                //{
                //}
            }


            // tunning untuk tidak duplicate
            var queryStatus = "\\\"}\"" + "," + "\"4\"" + "," + "\"\\\"" + CUST + "\\\"\"";  //     \"}","4","\"000003\""
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + queryStatus + "%' and arguments like '%" + iden.no_cust + "%' and invocationdata like '%shopee%' and invocationdata like '%GetOrderByStatusCompleted%' and statename like '%Enque%' and invocationdata not like '%resi%'");
            // end tunning untuk tidak duplicate

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Resi Pesanan {obj} ke Shopee Gagal.")]
        public async Task<string> GetOrderDetailsForTrackNo(ShopeeAPIData iden, string[] ordersn_list, int retry)
        {
            SetupContext(iden);
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/orders/detail";

            GetOrderDetailsData HttpBody = new GetOrderDetailsData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                ordersn_list = ordersn_list
                //ordersn_list = ordersn_list_test.ToArray()
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            string responseFromServer = "";

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

            if (responseFromServer != "")
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetOrderDetailsResult)) as ShopeeGetOrderDetailsResult;
                var connIdARF01C = Guid.NewGuid().ToString();

                foreach (var order in result.orders)
                {
                    var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.NO_REFERENSI == order.ordersn);

                    if (order.tracking_no != null && order.tracking_no != "")
                    {
                        ret = order.tracking_no;
                        if (pesananInDb != null)
                        {
                            pesananInDb.TRACKING_SHIPMENT = order.tracking_no;
                            ErasoftDbContext.SaveChanges();
                        }
                    }
                    else
                    {
                        //var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        //contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Update Resi Pesanan " + Convert.ToString(pesananInDb.NO_BUKTI) + " ke Shopee gagal.");
                        //List<string> list_ordersn = new List<string>();
                        //list_ordersn.Add(ordersn);

                        var cekRetry = retry;
                        if (cekRetry >= 0 && cekRetry < 2)
                        {
                            cekRetry = retry + 1;
                            string EDBConnID = EDB.GetConnectionString("ConnId");
                            var sqlStorage = new SqlServerStorage(EDBConnID);

                            var client = new BackgroundJobClient(sqlStorage);
#if (DEBUG || Debug_AWS)
                            GetOrderDetailsForTrackNo(iden, ordersn_list.ToArray(), cekRetry);
#else
                            client.Enqueue<ShopeeControllerJob>(x => x.GetOrderDetailsForTrackNo(iden, ordersn_list.ToArray(), cekRetry));
#endif
                        }
                        else
                        {
                            var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                            contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Update Resi Pesanan " + Convert.ToString(pesananInDb.NO_BUKTI) + " ke Shopee gagal.");
                        }
                    }
                }
                //}
                //catch (Exception ex2)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}
            }
            return ret;
        }
        //add by Tri 4 Des 2019
        public async Task<Dictionary<string, string>> GetOrderDetailsForCancelReason(ShopeeAPIData iden, string[] ordersn_list)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            if (ordersn_list.Count() > 50)
            {
                var arrayLength = ordersn_list.Count();
                int skip = 0;
                while (arrayLength > 0)
                {
                    var take = arrayLength;
                    if (take > 50)
                        take = 50;
                    var listOrder = ordersn_list.Skip(skip).Take(take).ToList().ToArray();
                    ret = await GetOrderDetailsForCancelReasonAPI(iden, listOrder, ret);
                    skip = skip + take;
                    arrayLength = arrayLength - take;
                }
            }
            else
            {
                ret = await GetOrderDetailsForCancelReasonAPI(iden, ordersn_list, ret);
            }
            return ret;
        }
        public async Task<Dictionary<string, string>> GetOrderDetailsForCancelReasonAPI(ShopeeAPIData iden, string[] ordersn_list, Dictionary<string, string> ret)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            //Dictionary<string, string> ret = new Dictionary<string, string>();

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/orders/detail";

            GetOrderDetailsData HttpBody = new GetOrderDetailsData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                ordersn_list = ordersn_list
                //ordersn_list = ordersn_list_test.ToArray()
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            string responseFromServer = "";

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

            if (responseFromServer != "")
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetOrderDetailsResult)) as ShopeeGetOrderDetailsResult;
                var connIdARF01C = Guid.NewGuid().ToString();

                foreach (var order in result.orders)
                {
                    //ret = order.tracking_no;
                    ret.Add(order.ordersn, order.cancel_reason);
                }
                //}
                //catch (Exception ex2)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}
            }
            return ret;
        }
        //end add by Tri 4 Des 2019

        //add by nurul 17/3/2020, hangfire update resi job 
        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrderDetailsForUpdateResiJOB(ShopeeAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST)
        {
            SetupContext(iden);
            string ret = "";
            //add by nurul 16/6/2020
            List<string> orderToProcess = new List<string>();
            string responseFromServer = "";
            //end add by nurul 16/6/2020
            var list_ordersn = ErasoftDbContext.SOT01A.Where(a => (a.TRACKING_SHIPMENT == null || a.TRACKING_SHIPMENT == "-" || a.TRACKING_SHIPMENT == "") && a.NO_PO_CUST.Contains("SH") && a.CUST == CUST && (a.STATUS_TRANSAKSI.Contains("03") || a.STATUS_TRANSAKSI.Contains("04"))).Select(a => a.NO_REFERENSI).ToList();
            if (list_ordersn.Count() > 0)
            {
                //add by nurul 16/6/2020
                if (list_ordersn.Count() > 50)
                {
                    var hitungOrder = 0;
                    hitungOrder = list_ordersn.Count();
                    foreach (var order1 in list_ordersn)
                    {
                        if (orderToProcess.Count() == 49 || orderToProcess.Count() + 1 == hitungOrder)
                        {
                            orderToProcess.Add(order1);

                            var ordersn_list = orderToProcess.ToArray();

                            int MOPartnerID = 841371;
                            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";

                            long seconds = CurrentTimeSecond();
                            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

                            string urll = "https://partner.shopeemobile.com/api/v1/orders/detail";

                            GetOrderDetailsData HttpBody = new GetOrderDetailsData
                            {
                                partner_id = MOPartnerID,
                                shopid = Convert.ToInt32(iden.merchant_code),
                                timestamp = seconds,
                                ordersn_list = ordersn_list
                            };

                            string myData = JsonConvert.SerializeObject(HttpBody);

                            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

                            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                            myReq.Method = "POST";
                            myReq.Headers.Add("Authorization", signature);
                            myReq.Accept = "application/json";
                            myReq.ContentType = "application/json";
                            //string responseFromServer = "";

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

                            if (responseFromServer != "")
                            {
                                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetOrderDetailsResult)) as ShopeeGetOrderDetailsResult;
                                var connIdARF01C = Guid.NewGuid().ToString();

                                foreach (var order in result.orders)
                                {
                                    if (order.tracking_no != null || order.tracking_no != "")
                                    {
                                        var dTrackNo = order.tracking_no;
                                        var noref = order.ordersn;
                                        var tempCust = CUST;
                                        var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.NO_REFERENSI == noref && p.CUST == tempCust);
                                        if (pesananInDb != null)
                                        {
                                            pesananInDb.TRACKING_SHIPMENT = dTrackNo;
                                            ErasoftDbContext.SaveChanges();
                                        }

                                    }
                                    else
                                    {
                                        throw new Exception("Update Resi JOB Gagal. Tracking Number Null.");
                                    }
                                }
                            }

                            hitungOrder = hitungOrder - orderToProcess.Count();
                            orderToProcess.Clear();
                        }
                        else
                        {
                            orderToProcess.Add(order1);
                        }
                    }
                }
                else
                {
                    //end add by nurul 16/6/2020

                    var ordersn_list = list_ordersn.ToArray();

                    int MOPartnerID = 841371;
                    string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
                    //string ret = "";

                    long seconds = CurrentTimeSecond();
                    DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

                    string urll = "https://partner.shopeemobile.com/api/v1/orders/detail";

                    GetOrderDetailsData HttpBody = new GetOrderDetailsData
                    {
                        partner_id = MOPartnerID,
                        shopid = Convert.ToInt32(iden.merchant_code),
                        timestamp = seconds,
                        ordersn_list = ordersn_list
                        //ordersn_list = ordersn_list.ToArray()
                    };

                    string myData = JsonConvert.SerializeObject(HttpBody);

                    string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                    myReq.Method = "POST";
                    myReq.Headers.Add("Authorization", signature);
                    myReq.Accept = "application/json";
                    myReq.ContentType = "application/json";
                    //string responseFromServer = "";

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

                    if (responseFromServer != "")
                    {
                        var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetOrderDetailsResult)) as ShopeeGetOrderDetailsResult;
                        var connIdARF01C = Guid.NewGuid().ToString();

                        foreach (var order in result.orders)
                        {
                            //ret = order.tracking_no;
                            if (order.tracking_no != null || order.tracking_no != "")
                            {
                                var dTrackNo = order.tracking_no;
                                var noref = order.ordersn;
                                var tempCust = CUST;
                                var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.NO_REFERENSI == noref && p.CUST == tempCust);
                                if (pesananInDb != null)
                                {
                                    pesananInDb.TRACKING_SHIPMENT = dTrackNo;
                                    ErasoftDbContext.SaveChanges();
                                }

                            }
                            else
                            {
                                throw new Exception("Update Resi JOB Gagal. Tracking Number Null.");
                            }
                        }


                        //}
                        //catch (Exception ex2)
                        //{
                        //    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                        //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                        //}
                    }
                }
            }
            return ret;
        }
        //end add by nurul, hangfire update resi job 

        public async Task<string> FixPemesanNullSOT01A(ShopeeAPIData iden, string[] ordersn_list, string CUST, string NAMA_CUST)
        {
            SetupContext(iden);
            //var MoDbContext = new MoDbContext();
            //var ErasoftDbContext = new ErasoftContext(iden.DatabasePathErasoft);
            //var EDB = new DatabaseSQL(iden.DatabasePathErasoft);
            //var username = iden.username;
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/orders/detail";

            GetOrderDetailsData HttpBody = new GetOrderDetailsData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
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

            if (responseFromServer != "")
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetOrderDetailsResult)) as ShopeeGetOrderDetailsResult;
                var connIdARF01C = Guid.NewGuid().ToString();
                TEMP_SHOPEE_ORDERS batchinsert = new TEMP_SHOPEE_ORDERS();
                List<TEMP_SHOPEE_ORDERS_ITEM> batchinsertItem = new List<TEMP_SHOPEE_ORDERS_ITEM>();
                string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                var kabKot = "3174";
                var prov = "31";

                foreach (var order in result.orders)
                {
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

                using (SqlCommand CommandSQL = new SqlCommand())
                {
                    //call sp to insert buyer data
                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIdARF01C;

                    EDB.ExecuteSQL("Con", "MoveARF01CFromTempTable", CommandSQL);
                };

                string updateSOT01A = "";
                foreach (var order in result.orders)
                {
                    try
                    {
                        updateSOT01A += "UPDATE SOT01A SET PEMESAN = (SELECT TOP 1 BUYER_CODE FROM ARF01C WHERE TLP = '" + order.recipient_address.phone + "' AND EMAIL = '') WHERE CUST='" + CUST + "' AND NO_REFERENSI = '" + order.ordersn + "';";
                    }
                    catch (Exception ex3)
                    {

                    }
                }

                if (updateSOT01A != "")
                {
                    EDB.ExecuteSQL("Constring", CommandType.Text, updateSOT01A);
                }
            }
            return ret;
        }

        public async Task<string> GetOrderDetails(ShopeeAPIData iden, string[] ordersn_list, string connID, string CUST, string NAMA_CUST, StatusOrder stat)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = seconds.ToString(),
            //    REQUEST_ACTION = "Get Order List", //ganti
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = iden.merchant_code,
            //    REQUEST_STATUS = "Pending",
            //};

            string urll = "https://partner.shopeemobile.com/api/v1/orders/detail";

            GetOrderDetailsData HttpBody = new GetOrderDetailsData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                ordersn_list = ordersn_list
                //ordersn_list = ordersn_list_test.ToArray()
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
            //    manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}

            if (responseFromServer != null)
            {
                //try
                //{
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetOrderDetailsResult)) as ShopeeGetOrderDetailsResult;
                var connIdARF01C = Guid.NewGuid().ToString();
                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
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
                    string tlp = !string.IsNullOrEmpty(order.recipient_address.phone) ? order.recipient_address.phone.Replace('\'', '`') : "";
                    if (tlp.Length > 30)
                    {
                        tlp = tlp.Substring(0, 30);
                    }
                    string AL_KIRIM1 = !string.IsNullOrEmpty(order.recipient_address.full_address) ? order.recipient_address.full_address.Replace('\'', '`') : "";
                    if (AL_KIRIM1.Length > 30)
                    {
                        AL_KIRIM1 = AL_KIRIM1.Substring(0, 30);
                    }
                    string KODEPOS = !string.IsNullOrEmpty(order.recipient_address.zipcode) ? order.recipient_address.zipcode.Replace('\'', '`') : "";
                    if (KODEPOS.Length > 7)
                    {
                        KODEPOS = KODEPOS.Substring(0, 7);
                    }

                    insertPembeli += string.Format("('{0}','{1}','{2}','{3}',0,0,'0','01',1, 'IDR', '01', '{4}', 0, 0, 0, 0, '1', 0, 0,'FP', '{5}', '{6}', '{7}', '', '{8}', '{9}', '', '','{10}'),",
                        ((nama ?? "").Replace("'", "`")),
                        ((order.recipient_address.full_address ?? "").Replace("'", "`")),
                        (tlp),
                        //(NAMA_CUST.Replace(',', '.')),
                        (NAMA_CUST.Length > 30 ? NAMA_CUST.Substring(0, 30) : NAMA_CUST),
                        (AL_KIRIM1),
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        (username),
                        (KODEPOS),
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
                        #region cut max length dan ubah '
                        string payment_method = !string.IsNullOrEmpty(order.payment_method) ? order.payment_method.Replace('\'', '`') : "";
                        if (payment_method.Length > 100)
                        {
                            payment_method = payment_method.Substring(0, 100);
                        }
                        string shipping_carrier = !string.IsNullOrEmpty(order.shipping_carrier) ? order.shipping_carrier.Replace('\'', '`') : "";
                        if (shipping_carrier.Length > 300)
                        {
                            shipping_carrier = shipping_carrier.Substring(0, 300);
                        }
                        string currency = !string.IsNullOrEmpty(order.currency) ? order.currency.Replace('\'', '`') : "";
                        if (currency.Length > 50)
                        {
                            currency = currency.Substring(0, 50);
                        }
                        string Recipient_Address_town = !string.IsNullOrEmpty(order.recipient_address.town) ? order.recipient_address.town.Replace('\'', '`') : "";
                        if (Recipient_Address_town.Length > 300)
                        {
                            Recipient_Address_town = Recipient_Address_town.Substring(0, 300);
                        }
                        string Recipient_Address_city = !string.IsNullOrEmpty(order.recipient_address.city) ? order.recipient_address.city.Replace('\'', '`') : "";
                        if (Recipient_Address_city.Length > 300)
                        {
                            Recipient_Address_city = Recipient_Address_city.Substring(0, 300);
                        }
                        string Recipient_Address_name = !string.IsNullOrEmpty(order.recipient_address.name) ? order.recipient_address.name.Replace('\'', '`') : "";
                        if (Recipient_Address_name.Length > 300)
                        {
                            Recipient_Address_name = Recipient_Address_name.Substring(0, 300);
                        }
                        string Recipient_Address_district = !string.IsNullOrEmpty(order.recipient_address.district) ? order.recipient_address.district.Replace('\'', '`') : "";
                        if (Recipient_Address_district.Length > 300)
                        {
                            Recipient_Address_district = Recipient_Address_district.Substring(0, 300);
                        }
                        string Recipient_Address_country = !string.IsNullOrEmpty(order.recipient_address.country) ? order.recipient_address.country.Replace('\'', '`') : "";
                        if (Recipient_Address_country.Length > 300)
                        {
                            Recipient_Address_country = Recipient_Address_country.Substring(0, 300);
                        }
                        string Recipient_Address_zipcode = !string.IsNullOrEmpty(order.recipient_address.zipcode) ? order.recipient_address.zipcode.Replace('\'', '`') : "";
                        if (Recipient_Address_zipcode.Length > 300)
                        {
                            Recipient_Address_zipcode = Recipient_Address_zipcode.Substring(0, 300);
                        }
                        string Recipient_Address_phone = !string.IsNullOrEmpty(order.recipient_address.phone) ? order.recipient_address.phone.Replace('\'', '`') : "";
                        if (Recipient_Address_phone.Length > 50)
                        {
                            Recipient_Address_phone = Recipient_Address_phone.Substring(0, 50);
                        }
                        string Recipient_Address_state = !string.IsNullOrEmpty(order.recipient_address.state) ? order.recipient_address.state.Replace('\'', '`') : "";
                        if (Recipient_Address_state.Length > 300)
                        {
                            Recipient_Address_state = Recipient_Address_state.Substring(0, 300);
                        }
                        string tracking_no = !string.IsNullOrEmpty(order.tracking_no) ? order.tracking_no.Replace('\'', '`') : "";
                        if (tracking_no.Length > 100)
                        {
                            tracking_no = tracking_no.Substring(0, 100);
                        }
                        string order_status = !string.IsNullOrEmpty(order.order_status) ? order.order_status.Replace('\'', '`') : "";
                        if (order_status.Length > 100)
                        {
                            order_status = order_status.Substring(0, 100);
                        }
                        string service_code = !string.IsNullOrEmpty(order.service_code) ? order.service_code.Replace('\'', '`') : "";
                        if (service_code.Length > 100)
                        {
                            service_code = service_code.Substring(0, 100);
                        }
                        string ordersn = !string.IsNullOrEmpty(order.ordersn) ? order.ordersn.Replace('\'', '`') : "";
                        if (ordersn.Length > 100)
                        {
                            ordersn = ordersn.Substring(0, 100);
                        }
                        string country = !string.IsNullOrEmpty(order.country) ? order.country.Replace('\'', '`') : "";
                        if (country.Length > 100)
                        {
                            country = country.Substring(0, 100);
                        }
                        string dropshipper = !string.IsNullOrEmpty(order.dropshipper) ? order.dropshipper.Replace('\'', '`') : "";
                        if (dropshipper.Length > 300)
                        {
                            dropshipper = dropshipper.Substring(0, 300);
                        }
                        string buyer_username = !string.IsNullOrEmpty(order.buyer_username) ? order.buyer_username.Replace('\'', '`') : "";
                        if (buyer_username.Length > 300)
                        {
                            buyer_username = buyer_username.Substring(0, 300);
                        }
                        if (NAMA_CUST.Length > 50)
                        {
                            NAMA_CUST = NAMA_CUST.Substring(0, 50);
                        }
                        #endregion
                        TEMP_SHOPEE_ORDERS newOrder = new TEMP_SHOPEE_ORDERS()
                        {
                            actual_shipping_cost = order.actual_shipping_cost,
                            buyer_username = buyer_username,
                            cod = order.cod,
                            country = country,
                            create_time = DateTimeOffset.FromUnixTimeSeconds(order.create_time).UtcDateTime,
                            currency = currency,
                            days_to_ship = order.days_to_ship,
                            dropshipper = dropshipper,
                            escrow_amount = order.escrow_amount,
                            estimated_shipping_fee = order.estimated_shipping_fee,
                            goods_to_declare = order.goods_to_declare,
                            message_to_seller = (order.message_to_seller ?? "").Replace('\'', '`'),
                            note = (order.note ?? "").Replace('\'', '`'),
                            note_update_time = DateTimeOffset.FromUnixTimeSeconds(order.note_update_time).UtcDateTime,
                            ordersn = ordersn,
                            order_status = order_status,
                            payment_method = payment_method,
                            //change by nurul 5/12/2019, local time 
                            //pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime,
                            pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime.AddHours(7),
                            //end change by nurul 5/12/2019, local time 
                            Recipient_Address_country = Recipient_Address_country,
                            Recipient_Address_state = Recipient_Address_state,
                            Recipient_Address_city = Recipient_Address_city,
                            Recipient_Address_town = Recipient_Address_town,
                            Recipient_Address_district = Recipient_Address_district,
                            Recipient_Address_full_address = (order.recipient_address.full_address ?? "").Replace('\'', '`'),
                            Recipient_Address_name = Recipient_Address_name,
                            Recipient_Address_phone = Recipient_Address_phone,
                            Recipient_Address_zipcode = Recipient_Address_zipcode,
                            service_code = service_code,
                            shipping_carrier = shipping_carrier,
                            total_amount = order.total_amount,
                            tracking_no = tracking_no,
                            update_time = DateTimeOffset.FromUnixTimeSeconds(order.update_time).UtcDateTime,
                            CONN_ID = connID,
                            CUST = CUST,
                            NAMA_CUST = NAMA_CUST
                        };
                        var ShippingFeeData = await GetShippingFee(iden, order.ordersn);
                        if (ShippingFeeData != null)
                        {
                            newOrder.estimated_shipping_fee = (ShippingFeeData.order_income.buyer_paid_shipping_fee + ShippingFeeData.order_income.shopee_shipping_rebate - ShippingFeeData.order_income.actual_shipping_fee).ToString();
                        }
                        var listPromo = new Dictionary<long, double>();//add 6 juli 2020
                        foreach (var item in order.items)
                        {
                            string item_name = !string.IsNullOrEmpty(item.item_name) ? item.item_name.Replace('\'', '`') : "";
                            if (item_name.Length > 400)
                            {
                                item_name = item_name.Substring(0, 400);
                            }
                            string item_sku = !string.IsNullOrEmpty(item.item_sku) ? item.item_sku.Replace('\'', '`') : "";
                            if (item_sku.Length > 400)
                            {
                                item_sku = item_sku.Substring(0, 400);
                            }
                            string variation_name = !string.IsNullOrEmpty(item.variation_name) ? item.variation_name.Replace('\'', '`') : "";
                            if (variation_name.Length > 400)
                            {
                                variation_name = variation_name.Substring(0, 400);
                            }
                            string variation_sku = !string.IsNullOrEmpty(item.variation_sku) ? item.variation_sku.Replace('\'', '`') : "";
                            if (variation_sku.Length > 400)
                            {
                                variation_sku = variation_sku.Substring(0, 400);
                            }

                            TEMP_SHOPEE_ORDERS_ITEM newOrderItem = new TEMP_SHOPEE_ORDERS_ITEM()
                            {
                                ordersn = ordersn,
                                is_wholesale = item.is_wholesale,
                                item_id = item.item_id,
                                item_name = item_name,
                                item_sku = item_sku,
                                variation_discounted_price = item.variation_discounted_price,
                                variation_id = item.variation_id,
                                variation_name = variation_name,
                                variation_original_price = item.variation_original_price,
                                variation_quantity_purchased = item.variation_quantity_purchased,
                                variation_sku = variation_sku,
                                weight = item.weight,
                                pay_time = DateTimeOffset.FromUnixTimeSeconds(order.pay_time ?? order.create_time).UtcDateTime,
                                CONN_ID = connID,
                                CUST = CUST,
                                NAMA_CUST = NAMA_CUST
                            };
                            if (!string.IsNullOrEmpty(item.promotion_type))
                            {
                                if (item.promotion_type == "bundle_deal")
                                {
                                    var discount = 0d;
                                    if (!listPromo.ContainsKey(item.promotion_id))
                                    {
                                        discount = await GetEscrowDetail(iden, order.ordersn, item.item_id, item.variation_id, item.promotion_id);
                                        listPromo.Add(item.promotion_id, discount);
                                    }
                                    else
                                    {
                                        discount = listPromo[item.promotion_id];
                                    }
                                    newOrderItem.variation_discounted_price = item.variation_original_price;
                                    newOrderItem.DISC = discount;
                                    newOrderItem.N_DISC = Convert.ToInt64(newOrderItem.variation_discounted_price) * newOrderItem.variation_quantity_purchased * discount / 100;
                                }
                            }
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
                            CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 1;
                            CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                            CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                            EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
                        }
                    }
                    catch (Exception ex3)
                    {

                    }
                }
                //}
                //catch (Exception ex2)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}
            }
            return ret;
        }
        //add by Tri 14 Apr 2020, api untuk ambil shipping fee
        public async Task<GetShippingFeeResult> GetShippingFee(ShopeeAPIData iden, string ordersn)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            var ret = new GetShippingFeeResult();

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/orders/income";

            GetEscrowDetailsData HttpBody = new GetEscrowDetailsData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                ordersn = ordersn
                //ordersn_list = ordersn_list_test.ToArray()
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
            if (!string.IsNullOrEmpty(responseFromServer))
            {
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(GetShippingFeeResult)) as GetShippingFeeResult;
                if (result != null)
                {
                    if (string.IsNullOrEmpty(result.error))
                    {
                        ret = result;
                        return ret;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(result.msg))
                        {
                            throw new Exception(result.msg);
                        }
                        else
                        {
                            throw new Exception(result.error);
                        }
                    }
                }
            }
            return null;
        }
        //end add by Tri 14 Apr 2020, api untuk ambil shipping fee 
        public async Task<double> GetEscrowDetail(ShopeeAPIData iden, string ordersn, long itemId, long variationId, long activityId)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            var ret = 0d;

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/orders/my_income";

            GetEscrowDetailsData HttpBody = new GetEscrowDetailsData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                ordersn = ordersn
                //ordersn_list = ordersn_list_test.ToArray()
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
            if (!string.IsNullOrEmpty(responseFromServer))
            {
                //GetEscrowDetailResult
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(GetEscrowDetailResult)) as GetEscrowDetailResult;
                if (result != null)
                {
                    if (result.order != null)
                    {
                        if (result.order.activity != null)
                        {
                            foreach (var act in result.order.activity)
                            {
                                //foreach (var item in act.items)
                                //{
                                //if (item.item_id == itemId && item.variation_id == variationId)
                                //{
                                //    var hargapromo = Convert.ToInt64(act.discounted_price) / item.quantity_purchased;
                                //    return hargapromo.ToString();
                                //}
                                //}
                                if (act.activity_id == activityId)
                                {
                                    double discount = (Convert.ToInt64(act.original_price) - Convert.ToInt64(act.discounted_price)) * 100 / Convert.ToInt64(act.original_price);
                                    return discount;
                                }

                            }
                        }
                    }
                }
            }
            return ret;
        }
        public class GetAirwayBillsData
        {
            public bool is_batch { get; set; }
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public string[] ordersn_list { get; set; }
            //add by nurul 28/2/2020, untuk job
            public object extinfo { get; set; }
            //end add by nurul 28/2/2020, untuk job
        }


        public class GetAirwayBillsRootResult
        {
            public GetAirwayBillsResult result { get; set; }
            public string request_id { get; set; }
        }

        public class GetAirwayBillsResult
        {
            public int total_count { get; set; }
            public GetAirwayBillsError[] errors { get; set; }
            public GetAirwayBillsAirway_Bills[] airway_bills { get; set; }
        }

        public class GetAirwayBillsError
        {
            public string ordersn { get; set; }
            public string error_description { get; set; }
            public string error_code { get; set; }
        }

        public class GetAirwayBillsAirway_Bills
        {
            public string ordersn { get; set; }
            public string airway_bill { get; set; }
        }

        public class GetAirwayBillsBatchResult
        {
            public GetAirwayBillsBatchResultBatch_Result batch_result { get; set; }
            public string request_id { get; set; }
        }

        public class GetAirwayBillsBatchResultBatch_Result
        {
            public int total_count { get; set; }
            public List<GetAirwayBillsBatchResultError> errors { get; set; }
            public string[] airway_bills { get; set; }
        }

        public class GetAirwayBillsBatchResultError
        {
            public string ordersn { get; set; }
            public string error_description { get; set; }
            public string error_code { get; set; }
        }

        public async Task<string> GetAirwayBillsJOB(ShopeeAPIData iden, string[] ordersn_list, getJOBShopee temp_job)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            //var result = new GetAirwayBillsBatchResult();
            var result = "";
            //result.batch_result = new GetAirwayBillsBatchResultBatch_Result();
            //result.batch_result.errors = new List<GetAirwayBillsBatchResultError>();
            //result.batch_result.errors.Add(new GetAirwayBillsBatchResultError { error_code = "MO_Internal", error_description = "Internal Server Error.", ordersn = "" });

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/airway_bill/get_mass";

            GetAirwayBillsData HttpBody = new GetAirwayBillsData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                ordersn_list = ordersn_list,
                is_batch = true,
                //ordersn_list = ordersn_list_test.ToArray()
                //add by nurul 28/2/2020, untuk job
                extinfo = temp_job
                //end add by nurul 28/2/2020, untuk job
            };

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";
            string responseFromServer = "";

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

            if (responseFromServer != "")
            {
                //result = JsonConvert.DeserializeObject(responseFromServer, typeof(GetAirwayBillsBatchResult)) as GetAirwayBillsBatchResult;
                result = responseFromServer;

                //var connIdARF01C = Guid.NewGuid().ToString();

                //foreach (var order in result.orders)
                //{
                //    ret = order.tracking_no;
                //}
                //}
                //catch (Exception ex2)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}
            }
            return result;
        }
        public async Task<GetAirwayBillsBatchResult> GetAirwayBills(ShopeeAPIData iden, string[] ordersn_list, getJOBShopee temp_job, bool adaJOB)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            var result = new GetAirwayBillsBatchResult();
            result.batch_result = new GetAirwayBillsBatchResultBatch_Result();
            result.batch_result.errors = new List<GetAirwayBillsBatchResultError>();
            result.batch_result.errors.Add(new GetAirwayBillsBatchResultError { error_code = "MO_Internal", error_description = "Internal Server Error.", ordersn = "" });

            var result1 = new GetAirwayBillsBatchResult();
            result1.batch_result = new GetAirwayBillsBatchResultBatch_Result();
            result1.batch_result.errors = new List<GetAirwayBillsBatchResultError>();
            result1.batch_result.errors.Add(new GetAirwayBillsBatchResultError { error_code = "MO_Internal", error_description = "Internal Server Error.", ordersn = "" });

            var result2 = new GetAirwayBillsBatchResult();
            result2.batch_result = new GetAirwayBillsBatchResultBatch_Result();
            result2.batch_result.errors = new List<GetAirwayBillsBatchResultError>();
            result2.batch_result.errors.Add(new GetAirwayBillsBatchResultError { error_code = "MO_Internal", error_description = "Internal Server Error.", ordersn = "" });

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/airway_bill/get_mass";

            GetAirwayBillsData HttpBody = new GetAirwayBillsData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                //ordersn_list = ordersn_list,
                is_batch = true,
                //extinfo = new getJOBShopee(),
            };
            var NewListOrder = new List<string>();
            var order_sn = new List<string>();
            var job_sn = new List<string>();
            getJOBShopee kirim_job = new getJOBShopee() { };
            string responseFromServer1 = "";
            string responseFromServer = "";
            if (adaJOB == false)
            {
                HttpBody.ordersn_list = ordersn_list;
                HttpBody.extinfo = new getJOBShopee();

                string myData = JsonConvert.SerializeObject(HttpBody);

                string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
                myReq.Method = "POST";
                myReq.Headers.Add("Authorization", signature);
                myReq.Accept = "application/json";
                myReq.ContentType = "application/json";


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
            else
            {

                if (ordersn_list.Count() == temp_job.job_ordersn_list.Count())
                {
                    for (int i = 0; i < temp_job.job_ordersn_list.Count(); i++)
                    {
                        if (temp_job.job_ordersn_list[i] != "")
                        {
                            order_sn.Add(ordersn_list[i]);
                            job_sn.Add(temp_job.job_ordersn_list[i]);
                            kirim_job.job_ordersn_list.Add(temp_job.job_ordersn_list[i]);
                        }
                        else
                        {
                            NewListOrder.Add(ordersn_list[i]);
                        }
                    }
                }
            }

            if (NewListOrder.Count() > 0)
            {
                HttpBody.ordersn_list = NewListOrder.ToArray();
                HttpBody.extinfo = new getJOBShopee();
                string myData1 = JsonConvert.SerializeObject(HttpBody);

                string signature1 = CreateSign(string.Concat(urll, "|", myData1), MOPartnerKey);

                HttpWebRequest myReq1 = (HttpWebRequest)WebRequest.Create(urll);
                myReq1.Method = "POST";
                myReq1.Headers.Add("Authorization", signature1);
                myReq1.Accept = "application/json";
                myReq1.ContentType = "application/json";


                myReq1.ContentLength = myData1.Length;
                using (var dataStream = myReq1.GetRequestStream())
                {
                    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData1), 0, myData1.Length);
                }
                using (WebResponse response = await myReq1.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer1 = reader.ReadToEnd();
                    }
                }
            }

            var ret = "";
            if (order_sn.Count() > 0)
            {
                ret = await GetAirwayBillsJOB(iden, order_sn.ToArray(), kirim_job);
                if (ret != "")
                {
                    result2 = JsonConvert.DeserializeObject(ret, typeof(GetAirwayBillsBatchResult)) as GetAirwayBillsBatchResult;
                }
            }
            if (responseFromServer1 != "")
            {
                if (responseFromServer1 != "")
                {
                    result1 = JsonConvert.DeserializeObject(responseFromServer1, typeof(GetAirwayBillsBatchResult)) as GetAirwayBillsBatchResult;
                }
            }

            if (responseFromServer != "")
            {

                result = JsonConvert.DeserializeObject(responseFromServer, typeof(GetAirwayBillsBatchResult)) as GetAirwayBillsBatchResult;
                if (responseFromServer1 != "")
                {
                    result.batch_result.total_count += result1.batch_result.total_count;
                    var temp1 = result.batch_result.airway_bills.ToList();
                    foreach (var AB in result1.batch_result.airway_bills)
                    {
                        temp1.Add(AB);
                    }
                    result.batch_result.airway_bills = temp1.ToArray();
                    result.batch_result.errors.AddRange(result1.batch_result.errors.ToList());
                }
                if (ret != "")
                {
                    result.batch_result.total_count += result2.batch_result.total_count;
                    var temp1 = result.batch_result.airway_bills.ToList();
                    foreach (var AB in result2.batch_result.airway_bills)
                    {
                        temp1.Add(AB);
                    }
                    result.batch_result.airway_bills = temp1.ToArray();
                    result.batch_result.errors.AddRange(result2.batch_result.errors.ToList());
                }

                //var connIdARF01C = Guid.NewGuid().ToString();

                //foreach (var order in result.orders)
                //{
                //    ret = order.tracking_no;
                //}
                //}
                //catch (Exception ex2)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}
            }
            else if (responseFromServer1 != "" || ret != "")
            {
                if (ret != "") //result2
                {
                    result = result2;
                    if (responseFromServer1 != "")
                    {
                        result.batch_result.total_count += result1.batch_result.total_count;
                        var temp1 = result.batch_result.airway_bills.ToList();
                        foreach (var AB in result1.batch_result.airway_bills)
                        {
                            temp1.Add(AB);
                        }
                        result.batch_result.airway_bills = temp1.ToArray();
                        result.batch_result.errors.AddRange(result1.batch_result.errors.ToList());
                    }
                }
                if (responseFromServer != "") //result1
                {
                    result = result1;
                    if (ret != "")
                    {
                        result.batch_result.total_count += result2.batch_result.total_count;
                        var temp1 = result.batch_result.airway_bills.ToList();
                        foreach (var AB in result2.batch_result.airway_bills)
                        {
                            temp1.Add(AB);
                        }
                        result.batch_result.airway_bills = temp1.ToArray();
                        result.batch_result.errors.AddRange(result2.batch_result.errors.ToList());
                    }
                }

            }
            return result;
        }

        public async Task<ShopeeGetParameterForInitLogisticResult> GetParameterForInitLogistic(ShopeeAPIData iden, string ordersn)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            ShopeeGetParameterForInitLogisticResult ret = null;
            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/init_parameter/get";

            ShopeeGetParameterForInitLogisticData HttpBody = new ShopeeGetParameterForInitLogisticData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetParameterForInitLogisticResult)) as ShopeeGetParameterForInitLogisticResult;
                    ret = result;
                }
                catch (Exception ex2)
                {

                }
            }
            return ret;
        }
        public async Task<ShopeeGetParameterForInitLogisticResult> GetLogisticInfo(ShopeeAPIData iden, string ordersn)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            ShopeeGetParameterForInitLogisticResult ret = null;
            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/init_info/get";

            ShopeeGetParameterForInitLogisticData HttpBody = new ShopeeGetParameterForInitLogisticData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetParameterForInitLogisticResult)) as ShopeeGetParameterForInitLogisticResult;
                    ret = result;
                }
                catch (Exception ex2)
                {

                }
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Proses Dropoff/JOB Pesanan {obj} ke Shopee Gagal.")]
        public async Task<string> InitLogisticDropOff(string dbPathEra, string namaPemesan, string log_CUST, string log_ActionCategory, string log_ActionName, ShopeeAPIData iden, string ordersn, ShopeeInitLogisticDropOffDetailData data, int recnum, string dBranch, string dSender, string dTrackNo, string set_job)
        {
            SetupContext(iden);
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = seconds.ToString(),
                REQUEST_ACTION = "Update No Resi",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = ordersn,
                REQUEST_ATTRIBUTE_2 = dTrackNo,
                REQUEST_ATTRIBUTE_3 = "dropoff",
                REQUEST_ATTRIBUTE_4 = dSender + "[;]" + dBranch,
                REQUEST_STATUS = "Pending",
            };

            if (set_job == "1")
            {
                currentLog.REQUEST_ACTION = "Generate JOB";
                currentLog.REQUEST_ATTRIBUTE_3 = "JOB";
            }

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/init";
            ShopeeInitLogisticDropOffData HttpBody = new ShopeeInitLogisticDropOffData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
            //try
            //{
            myReq.ContentLength = myData.Length;
            //System.Threading.Thread.Sleep(5000);
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
            if (set_job == "1")
            {
                manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            }
            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}

            if (responseFromServer != "")
            {
                //try
                //{


                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeInitLogisticResult)) as ShopeeInitLogisticResult;
                if ((result.error == null ? "" : result.error) == "")
                {
                    //add by nurul 6/3/2020, u/ handle pertama kali proses dropoff berhasil tapi tracking_no null 
                    if ((string.IsNullOrWhiteSpace(result.tracking_number)) && result.request_id != "")
                    {
                        //DIGANTI PAKE THROW UNTUK RETRY NYA 
                        if (set_job == "1")
                        {
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        }
                        throw new Exception("Tracking Number Null");
                        //myData = JsonConvert.SerializeObject(HttpBody);

                        //signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);
                        //responseFromServer = "";

                        //myReq = (HttpWebRequest)WebRequest.Create(urll);
                        //myReq.Method = "POST";
                        //myReq.Headers.Add("Authorization", signature);
                        //myReq.Accept = "application/json";
                        //myReq.ContentType = "application/json";

                        //myReq.ContentLength = myData.Length;
                        //using (var dataStream = myReq.GetRequestStream())
                        //{
                        //    dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, myData.Length);
                        //}
                        //System.Threading.Thread.Sleep(5000);
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
                        //    result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeInitLogisticResult)) as ShopeeInitLogisticResult;
                        //    if ((result.error == null ? "" : result.error) == "")
                        //    {
                        //        if (!string.IsNullOrWhiteSpace(result.tracking_no) || !string.IsNullOrWhiteSpace(result.tracking_number))
                        //        {
                        //            var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.RecNum == recnum);
                        //            if (pesananInDb != null)
                        //            {
                        //                if (dTrackNo == "")
                        //                {
                        //                    dTrackNo = string.IsNullOrEmpty(result.tracking_no) ? result.tracking_number : result.tracking_no;
                        //                }
                        //                string nilaiTRACKING_SHIPMENT = "D[;]" + dBranch + "[;]" + dSender + "[;]" + dTrackNo;
                        //                if (nilaiTRACKING_SHIPMENT == "D[;][;][;]")
                        //                {
                        //                    nilaiTRACKING_SHIPMENT = "";
                        //                }

                        //                pesananInDb.TRACKING_SHIPMENT = dTrackNo;
                        //                pesananInDb.status_kirim = "2";
                        //                if (string.IsNullOrWhiteSpace(pesananInDb.TRACKING_SHIPMENT))
                        //                {
                        //                    pesananInDb.status_kirim = "1";
                        //                }

                        //                ErasoftDbContext.SaveChanges();
                        //                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        //                contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Berhasil Update Resi Pesanan " + Convert.ToString(pesananInDb.NO_BUKTI) + " ke Shopee.");
                        //            }
                        //        }
                        //        else
                        //        {
                        //            List<string> list_ordersn = new List<string>();
                        //            list_ordersn.Add(ordersn);
                        //            var trackno = await GetOrderDetailsForTrackNo(iden, list_ordersn.ToArray());

                        //            var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.RecNum == recnum);
                        //            if (pesananInDb != null)
                        //            {
                        //                if (dTrackNo == "")
                        //                {
                        //                    dTrackNo = trackno;
                        //                }
                        //                string nilaiTRACKING_SHIPMENT = "D[;]" + dBranch + "[;]" + dSender + "[;]" + dTrackNo;
                        //                if (nilaiTRACKING_SHIPMENT == "D[;][;][;]")
                        //                {
                        //                    nilaiTRACKING_SHIPMENT = "";
                        //                }
                        //                pesananInDb.TRACKING_SHIPMENT = dTrackNo;
                        //                pesananInDb.status_kirim = "2";
                        //                if (string.IsNullOrWhiteSpace(pesananInDb.TRACKING_SHIPMENT))
                        //                {
                        //                    pesananInDb.status_kirim = "1";
                        //                }

                        //                ErasoftDbContext.SaveChanges();
                        //                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        //                contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Berhasil Update Resi Pesanan " + Convert.ToString(pesananInDb.NO_BUKTI) + " ke Shopee.");
                        //            }
                        //        }
                        //    }
                        //    else
                        //    {
                        //        throw new Exception(result.msg);

                        //    }
                        //}
                        //end DIGANTI PAKE THROW UNTUK RETRY NYA
                    }
                    //end add by nurul 6/3/2020, u/ handle pertama kali proses dropoff berhasil tapi tracking_no null 
                    else if (string.IsNullOrWhiteSpace(result.tracking_no) && string.IsNullOrWhiteSpace(result.tracking_number))
                    {
                        List<string> list_ordersn = new List<string>();
                        list_ordersn.Add(ordersn);
                        //var trackno = await GetOrderDetailsForTrackNo(iden, list_ordersn.ToArray());

                        string EDBConnID = EDB.GetConnectionString("ConnId");
                        var sqlStorage = new SqlServerStorage(EDBConnID);

                        var client = new BackgroundJobClient(sqlStorage);
#if (DEBUG || Debug_AWS)
                        GetOrderDetailsForTrackNo(iden, list_ordersn.ToArray(), 0);
#else
                            client.Enqueue<ShopeeControllerJob>(x => x.GetOrderDetailsForTrackNo(iden, list_ordersn.ToArray(), 0));
#endif

                        var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.RecNum == recnum);
                        if (pesananInDb != null)
                        {
                            if (dTrackNo == "")
                            {
                                //dTrackNo = trackno;
                            }
                            //string nilaiTRACKING_SHIPMENT = "D[;]" + dBranch + "[;]" + dSender + "[;]" + dTrackNo;
                            //if (nilaiTRACKING_SHIPMENT == "D[;][;][;]")
                            //{
                            //    nilaiTRACKING_SHIPMENT = "";
                            //}
                            //                            pesananInDb.TRACKING_SHIPMENT = nilaiTRACKING_SHIPMENT;
                            if (set_job == "1")
                            {
                                pesananInDb.NO_PO_CUST = dTrackNo;
                            }
                            else
                            {
                                pesananInDb.TRACKING_SHIPMENT = dTrackNo;
                            }
                            pesananInDb.status_kirim = "2";
                            //if (string.IsNullOrWhiteSpace(pesananInDb.TRACKING_SHIPMENT) && set_job != "1")
                            //{
                            //    pesananInDb.status_kirim = "1";
                            //}
                            if (string.IsNullOrWhiteSpace(pesananInDb.NO_PO_CUST) && set_job == "1")
                            {
                                pesananInDb.status_kirim = "1";
                            }

                            ErasoftDbContext.SaveChanges();
                            if (set_job != "1")
                            {
                                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Berhasil Proses Dropoff/JOB Pesanan " + Convert.ToString(pesananInDb.NO_BUKTI) + " ke Shopee.");
                            }
                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                        }



                    }
                    else
                    {
                        var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.RecNum == recnum);
                        if (pesananInDb != null)
                        {
                            if (dTrackNo == "")
                            {
                                dTrackNo = string.IsNullOrEmpty(result.tracking_no) ? result.tracking_number : result.tracking_no;
                            }
                            string nilaiTRACKING_SHIPMENT = "D[;]" + dBranch + "[;]" + dSender + "[;]" + dTrackNo;
                            if (nilaiTRACKING_SHIPMENT == "D[;][;][;]")
                            {
                                nilaiTRACKING_SHIPMENT = "";
                            }

                            //                            pesananInDb.TRACKING_SHIPMENT = nilaiTRACKING_SHIPMENT;
                            if (set_job == "1")
                            {
                                pesananInDb.NO_PO_CUST = dTrackNo;
                            }
                            else
                            {
                                pesananInDb.TRACKING_SHIPMENT = dTrackNo;
                            }
                            pesananInDb.status_kirim = "2";
                            //if (string.IsNullOrWhiteSpace(pesananInDb.TRACKING_SHIPMENT) && set_job != "1")
                            //{
                            //    pesananInDb.status_kirim = "1";
                            //}
                            if (string.IsNullOrWhiteSpace(pesananInDb.NO_PO_CUST) && set_job == "1")
                            {
                                pesananInDb.status_kirim = "1";
                            }

                            ErasoftDbContext.SaveChanges();
                            if (set_job != "1")
                            {
                                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Berhasil Proses Dropoff/JOB Pesanan " + Convert.ToString(pesananInDb.NO_BUKTI) + " ke Shopee.");
                            }
                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                        }
                        //List<string> list_ordersn = new List<string>();
                        //list_ordersn.Add(ordersn);
                        //var trackno = await GetOrderDetailsForTrackNo(iden, list_ordersn.ToArray());

                        //var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.RecNum == recnum);
                        //if (pesananInDb != null)
                        //{
                        //    if (dTrackNo == "")
                        //    {
                        //        dTrackNo = trackno;
                        //    }
                        //    string nilaiTRACKING_SHIPMENT = "D[;]" + dBranch + "[;]" + dSender + "[;]" + dTrackNo;
                        //    if (nilaiTRACKING_SHIPMENT == "D[;][;][;]")
                        //    {
                        //        nilaiTRACKING_SHIPMENT = "";
                        //    }
                        //    //                            pesananInDb.TRACKING_SHIPMENT = nilaiTRACKING_SHIPMENT;
                        //    if (set_job == "1")
                        //    {
                        //        pesananInDb.NO_PO_CUST = dTrackNo;
                        //    }
                        //    else
                        //    {
                        //        pesananInDb.TRACKING_SHIPMENT = dTrackNo;
                        //    }
                        //    pesananInDb.status_kirim = "2";
                        //    if (string.IsNullOrWhiteSpace(pesananInDb.TRACKING_SHIPMENT) && set_job != "1")
                        //    {
                        //        pesananInDb.status_kirim = "1";
                        //    }
                        //    if (string.IsNullOrWhiteSpace(pesananInDb.NO_PO_CUST) && set_job == "1")
                        //    {
                        //        pesananInDb.status_kirim = "1";
                        //    }

                        //    ErasoftDbContext.SaveChanges();
                        //    if (set_job != "1")
                        //    {
                        //        var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        //        contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Berhasil Update Resi Pesanan " + Convert.ToString(pesananInDb.NO_BUKTI) + " ke Shopee.");
                        //    }
                        //    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                        //}
                    }
                }
                else
                {
                    var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.RecNum == recnum);
                    if (pesananInDb != null)
                    {
                        pesananInDb.status_kirim = "1";
                        ErasoftDbContext.SaveChanges();
                    }
                    throw new Exception(result.msg);

                    //currentLog.REQUEST_EXCEPTION = result.msg;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                }
                //}
                //catch (Exception ex2)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}
            }
            return ret;
        }
        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Resi Pesanan {obj} ke Shopee Gagal.")]
        public async Task<string> InitLogisticNonIntegrated(string dbPathEra, string namaPemesan, string log_CUST, string log_ActionCategory, string log_ActionName, ShopeeAPIData iden, string ordersn, ShopeeInitLogisticNotIntegratedDetailData data, int recnum, string savedParam)
        {
            SetupContext(iden);
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = seconds.ToString(),
            //    REQUEST_ACTION = "Update No Resi",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = ordersn,
            //    REQUEST_ATTRIBUTE_2 = "",
            //    REQUEST_ATTRIBUTE_3 = "NonIntegrated",
            //    REQUEST_ATTRIBUTE_4 = savedParam,
            //    REQUEST_STATUS = "Pending",
            //};

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/init";
            ShopeeInitLogisticNonIntegratedData HttpBody = new ShopeeInitLogisticNonIntegratedData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
            //    manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}

            if (responseFromServer != null)
            {
                //try
                //{
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeInitLogisticResult)) as ShopeeInitLogisticResult;

                var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Berhasil Update Resi Pesanan " + Convert.ToString(namaPemesan) + " ke Shopee.");
                //    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                //}
                //catch (Exception ex2)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Request Pickup Pesanan {obj} ke Shopee Gagal.")]
        public async Task<string> InitLogisticPickup(string dbPathEra, string namaPemesan, string log_CUST, string log_ActionCategory, string log_ActionName, ShopeeAPIData iden, string ordersn, ShopeeInitLogisticPickupDetailData data, int recnum, string savedParam)
        {
            SetupContext(iden);
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = seconds.ToString(),
            //    REQUEST_ACTION = "Update No Resi",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = ordersn,
            //    REQUEST_ATTRIBUTE_2 = "",
            //    REQUEST_ATTRIBUTE_3 = "Pickup",
            //    REQUEST_ATTRIBUTE_4 = savedParam,
            //    REQUEST_STATUS = "Pending",
            //};

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/init";
            ShopeeInitLogisticPickupData HttpBody = new ShopeeInitLogisticPickupData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
            //    manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}

            if (responseFromServer != "")
            {
                //try
                //{
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeInitLogisticResult)) as ShopeeInitLogisticResult;
                if (result.error == null)
                {
                    if (string.IsNullOrWhiteSpace(result.tracking_no) && string.IsNullOrWhiteSpace(result.tracking_number))
                    {
                        List<string> list_ordersn = new List<string>();
                        list_ordersn.Add(ordersn);
                        //var trackno = await GetOrderDetailsForTrackNo(iden, list_ordersn.ToArray());
                        string EDBConnID = EDB.GetConnectionString("ConnId");
                        var sqlStorage = new SqlServerStorage(EDBConnID);

                        var client = new BackgroundJobClient(sqlStorage);
#if (DEBUG || Debug_AWS)
                        GetOrderDetailsForTrackNo(iden, list_ordersn.ToArray(), 0);
#else
                            client.Enqueue<ShopeeControllerJob>(x => x.GetOrderDetailsForTrackNo(iden, list_ordersn.ToArray(), 0));
#endif

                        var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.RecNum == recnum);
                        if (pesananInDb != null)
                        {
                            //string nilaiTRACKING_SHIPMENT = "P[;]" + data.address_id + "[;]" + data.pickup_time_id + "[;]" + trackno;

                            //                            pesananInDb.TRACKING_SHIPMENT = nilaiTRACKING_SHIPMENT;
                            //pesananInDb.TRACKING_SHIPMENT = trackno;
                            pesananInDb.status_kirim = "2";
                            //if (string.IsNullOrWhiteSpace(pesananInDb.TRACKING_SHIPMENT))
                            //{
                            //    pesananInDb.status_kirim = "1";
                            //}

                            ErasoftDbContext.SaveChanges();
                            var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                            //contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Berhasil Update Resi Pesanan " + Convert.ToString(namaPemesan) + " ke Shopee.");
                            contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Berhasil Request Pickup Pesanan " + Convert.ToString(pesananInDb.NO_BUKTI) + " ke Shopee.");
                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                        }
                    }
                    else
                    {
                        var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.RecNum == recnum);
                        if (pesananInDb != null)
                        {
                            //                            pesananInDb.TRACKING_SHIPMENT = savedParam;
                            string dTrackNo = "";
                            if (dTrackNo == "")
                            {
                                dTrackNo = string.IsNullOrEmpty(result.tracking_no) ? result.tracking_number : result.tracking_no;
                            }
                            pesananInDb.TRACKING_SHIPMENT = dTrackNo;
                            pesananInDb.status_kirim = "2";
                            //if (string.IsNullOrWhiteSpace(dTrackNo))
                            //{
                            //    pesananInDb.status_kirim = "1";
                            //}
                            ErasoftDbContext.SaveChanges();

                            var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                            //contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Berhasil Request Pickup Pesanan " + Convert.ToString(namaPemesan) + " ke Shopee.");
                            contextNotif.Clients.Group(iden.DatabasePathErasoft).monotification("Berhasil Request Pickup Pesanan " + Convert.ToString(pesananInDb.NO_BUKTI) + " ke Shopee.");
                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                        }
                    }
                }
                else
                {
                    var pesananInDb = ErasoftDbContext.SOT01A.SingleOrDefault(p => p.RecNum == recnum);
                    if (pesananInDb != null)
                    {
                        pesananInDb.status_kirim = "1";
                        ErasoftDbContext.SaveChanges();
                    }
                    throw new Exception(result.msg);
                    //currentLog.REQUEST_EXCEPTION = result.msg;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                }
                //}
                //catch (Exception ex2)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}
            }
            return ret;
        }

        public async Task<string> UpdateStock(ShopeeAPIData iden, string brg_mp, int qty)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = seconds.ToString(),
                REQUEST_ACTION = "Update QOH",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/items/update_stock";
            string[] brg_mp_split = brg_mp.Split(';');
            ShopeeUpdateStockData HttpBody = new ShopeeUpdateStockData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
        public async Task<string> UpdateVariationStock(ShopeeAPIData iden, string brg_mp, int qty)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = seconds.ToString(),
                REQUEST_ACTION = "Update QOH",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/items/update_variation_stock";
            string[] brg_mp_split = brg_mp.Split(';');
            ShopeeUpdateVariationStockData HttpBody = new ShopeeUpdateVariationStockData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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

        public async Task<ShopeeGetLogisticsResult> GetLogistics(ShopeeAPIData iden)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            ShopeeGetLogisticsResult ret = null;

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/channel/get";

            ShopeeGetLogisticsData HttpBody = new ShopeeGetLogisticsData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetLogisticsResult)) as ShopeeGetLogisticsResult;
                    ret = result;
                }
                catch (Exception ex2)
                {
                }
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Status Cancel Pesanan {obj} ke Shopee Gagal.")]
        public async Task<string> AcceptBuyerCancellation(string dbPathEra, string namaPembeli, string log_CUST, string log_ActionCategory, string log_ActionName, ShopeeAPIData iden, string ordersn)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";
            SetupContext(iden);

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = seconds.ToString(),
            //    REQUEST_ACTION = "Accept Buyer Cancel", //ganti
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = iden.merchant_code,
            //    REQUEST_STATUS = "Pending",
            //};

            string urll = "https://partner.shopeemobile.com/api/v1/orders/buyer_cancellation/accept";

            ShopeeAcceptBuyerCancelOrderData HttpBody = new ShopeeAcceptBuyerCancelOrderData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}

            if (responseFromServer != null)
            {
                //try
                //{
                //    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                //}
                //catch (Exception ex2)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Status Cancel Pesanan {obj} ke Shopee Gagal.")]
        public async Task<string> CancelOrder(string dbPathEra, string namaPembeli, string log_CUST, string log_ActionCategory, string log_ActionName, ShopeeAPIData iden, string ordersn, string cancelReason, string listVariable)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";
            SetupContext(iden);
            //var MoDbContext = new MoDbContext();
            //var ErasoftDbContext = new ErasoftContext(dbPathEra);
            //var EDB = new DatabaseSQL(dbPathEra);
            //var username = iden.username;

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = seconds.ToString(),
            //    REQUEST_ACTION = "Cancel Order",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = iden.merchant_code,
            //    REQUEST_STATUS = "Pending",
            //};

            string urll = "https://partner.shopeemobile.com/api/v1/orders/cancel";

            ShopeeCancelOrderData HttpBody = new ShopeeCancelOrderData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                ordersn = ordersn,
                //cancel_reason = "CUSTOMER_REQUEST"
                cancel_reason = cancelReason
            };
            if (cancelReason.Contains("STOCK"))
            {
                var listBrg = listVariable.Split('|');
                foreach (var brg in listBrg)
                {
                    var kodeBrg = brg.Split(';');
                    if (kodeBrg.Length > 1)
                    {
                        if (kodeBrg[1] == "0" || string.IsNullOrEmpty(kodeBrg[1]))
                        {
                            HttpBody.item_id = Convert.ToInt64(kodeBrg[0]);
                        }
                        else
                        {
                            HttpBody.item_id = Convert.ToInt64(kodeBrg[0]);
                            HttpBody.variation_id = Convert.ToInt64(kodeBrg[1]);
                        }
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
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}

            if (responseFromServer != null)
            {
                //try
                //{
                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeCancelOrderResult)) as ShopeeCancelOrderResult;
                if (result.error != null)
                {
                    if (result.error != "")
                    {
                        //await AcceptBuyerCancellation(iden, ordersn);
                    }
                }
                //}
                //catch (Exception ex2)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 0)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Product {obj} ke Shopee Gagal.")]
        public async Task<string> CreateProduct(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, ShopeeAPIData iden, string brg, string cust, List<ShopeeLogisticsClass> logistics)
        {
            string ret = "";
            SetupContext(iden);

            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == brg.ToUpper()).FirstOrDefault();
            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == cust.ToUpper()).FirstOrDefault();
            if (brgInDb == null || marketplace == null)
                return "invalid passing data";
            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == brg.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
            if (detailBrg == null)
                return "invalid passing data";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/item/add";

            //add by calvin 21 desember 2018, default nya semua logistic enabled
            var ShopeeGetLogisticsResult = await GetLogistics(iden);

            foreach (var log in ShopeeGetLogisticsResult.logistics.Where(p => p.enabled == true && p.fee_type.ToUpper() != "CUSTOM_PRICE" && p.fee_type.ToUpper() != "SIZE_SELECTION"))
            {
                bool lolosValidLogistic = true;
                if (log.weight_limits != null)
                {
                    if (log.weight_limits.item_max_weight < (brgInDb.BERAT / 1000))
                    {
                        lolosValidLogistic = false;
                    }
                    if (log.weight_limits.item_min_weight > (brgInDb.BERAT / 1000))
                    {
                        lolosValidLogistic = false;
                    }
                }

                if (log.item_max_dimension != null)
                {
                    if (log.item_max_dimension.length > 0)
                    {
                        if (log.item_max_dimension.length < (Convert.ToInt32(brgInDb.PANJANG) == 0 ? 1 : Convert.ToInt32(brgInDb.PANJANG)))
                        {
                            lolosValidLogistic = false;
                        }
                    }
                    if (log.item_max_dimension.height > 0)
                    {
                        if (log.item_max_dimension.height < (Convert.ToInt32(brgInDb.TINGGI) == 0 ? 1 : Convert.ToInt32(brgInDb.TINGGI)))
                        {
                            lolosValidLogistic = false;
                        }
                    }
                    if (log.item_max_dimension.width > 0)
                    {
                        if (log.item_max_dimension.width < (Convert.ToInt32(brgInDb.LEBAR) == 0 ? 1 : Convert.ToInt32(brgInDb.LEBAR)))
                        {
                            lolosValidLogistic = false;
                        }
                    }
                }

                if (lolosValidLogistic)
                {
                    logistics.Add(new ShopeeLogisticsClass()
                    {
                        enabled = log.enabled,
                        is_free = false,
                        logistic_id = log.logistic_id,
                    });
                }
            }
            //end add by calvin 21 desember 2018, default nya semua logistic enabled

            ShopeeProductData HttpBody = new ShopeeProductData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                item_sku = brg,
                category_id = Convert.ToInt64(detailBrg.CATEGORY_CODE),
                condition = "NEW",
                name = (brgInDb.NAMA + " " + brgInDb.NAMA2).Trim().Replace("’", "`"),
                description = brgInDb.Deskripsi.Replace("’", "`"),
                package_height = Convert.ToInt32(brgInDb.TINGGI) == 0 ? 1 : Convert.ToInt32(brgInDb.TINGGI),
                package_length = Convert.ToInt32(brgInDb.PANJANG) == 0 ? 1 : Convert.ToInt32(brgInDb.PANJANG),
                package_width = Convert.ToInt32(brgInDb.LEBAR) == 0 ? 1 : Convert.ToInt32(brgInDb.LEBAR),
                weight = brgInDb.BERAT / 1000,
                price = detailBrg.HJUAL,
                stock = 1,//create product min stock = 1
                images = new List<ShopeeImageClass>(),
                attributes = new List<ShopeeAttributeClass>(),
                variations = new List<ShopeeVariationClass>(),
                logistics = logistics
            };

            //add by calvin 10 mei 2019
            HttpBody.description = new StokControllerJob().RemoveSpecialCharacters(HttpBody.description);
            //end add by calvin 10 mei 2019

            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
            HttpBody.description = HttpBody.description.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
            HttpBody.description = HttpBody.description.Replace("<li>", "- ").Replace("</li>", "\r\n");
            HttpBody.description = HttpBody.description.Replace("<ul>", "").Replace("</ul>", "\r\n");
            HttpBody.description = HttpBody.description.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
            HttpBody.description = HttpBody.description.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
            HttpBody.description = HttpBody.description.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");
            //end add by nurul 20/1/2020, handle <p> dan enter double di shopee

            //add by calvin 10 september 2019
            HttpBody.description = HttpBody.description.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
            HttpBody.description = HttpBody.description.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
            HttpBody.description = HttpBody.description.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
            HttpBody.description = HttpBody.description.Replace("<p>", "\r\n").Replace("</p>", "\r\n");
            //HttpBody.description = HttpBody.description.Replace("<li>", "- ").Replace("</li>", "\r\n");
            //HttpBody.description = HttpBody.description.Replace("&nbsp;", "");

            HttpBody.description = System.Text.RegularExpressions.Regex.Replace(HttpBody.description, "<.*?>", String.Empty);
            //end add by calvin 10 september 2019

            //add by calvin 1 mei 2019
            var qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(brg, "ALL");
            if (qty_stock > 0)
            {
                HttpBody.stock = Convert.ToInt32(qty_stock);
            }
            //end add by calvin 1 mei 2019
            int jmlPic = 0;
            //add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
            //List<string> byteGambarUploaded = new List<string>();
            //end add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat

            if (jmlPic < 9)
            {
                //if (!byteGambarUploaded.Contains(brgInDb.Sort5))
                {
                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
                    {
                        HttpBody.images.Add(new ShopeeImageClass { url = brgInDb.LINK_GAMBAR_1 });
                        jmlPic++;
                        //byteGambarUploaded.Add(brgInDb.Sort5);
                    }
                }
            }
            if (jmlPic < 9)
            {
                //if (!byteGambarUploaded.Contains(brgInDb.Sort6))
                {
                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
                    {
                        HttpBody.images.Add(new ShopeeImageClass { url = brgInDb.LINK_GAMBAR_2 });
                        jmlPic++;
                        //byteGambarUploaded.Add(brgInDb.Sort6);
                    }
                }
            }
            if (jmlPic < 9)
            {
                //if (!byteGambarUploaded.Contains(brgInDb.Sort7))
                {
                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
                    {
                        HttpBody.images.Add(new ShopeeImageClass { url = brgInDb.LINK_GAMBAR_3 });
                        jmlPic++;
                        //byteGambarUploaded.Add(brgInDb.Sort7);
                    }
                }
            }
            if (jmlPic < 9)
            {
                //if (!byteGambarUploaded.Contains(brgInDb.SIZE_GAMBAR_4))
                {
                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_4))
                    {
                        HttpBody.images.Add(new ShopeeImageClass { url = brgInDb.LINK_GAMBAR_4 });
                        jmlPic++;
                        //byteGambarUploaded.Add(brgInDb.SIZE_GAMBAR_4);
                    }
                }
            }
            if (jmlPic < 9)
            {
                //if (!byteGambarUploaded.Contains(brgInDb.SIZE_GAMBAR_5))
                {
                    if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_5))
                    {
                        HttpBody.images.Add(new ShopeeImageClass { url = brgInDb.LINK_GAMBAR_5 });
                        jmlPic++;
                        //byteGambarUploaded.Add(brgInDb.SIZE_GAMBAR_5);
                    }
                }
            }
            //remark 15/10/2019, tidak ambil gambar varian untuk barang induk
            //if (brgInDb.TYPE == "4")
            //{
            //    var ListVariant = ErasoftDbContext.STF02.Where(p => p.PART == brg).ToList();
            //    var ListSettingVariasi = ErasoftDbContext.STF02I.Where(p => p.BRG == brg).ToList();

            //    foreach (var item in ListVariant)
            //    {
            //        if (jmlPic < 9)
            //        {
            //            //List<string> Duplikat = HttpBody.variations.Select(p => p.name).ToList();
            //            //add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
            //            if (!byteGambarUploaded.Contains(item.Sort5))
            //            {
            //                if (!string.IsNullOrEmpty(item.LINK_GAMBAR_1))
            //                {
            //                    HttpBody.images.Add(new ShopeeImageClass { url = item.LINK_GAMBAR_1 });
            //                    jmlPic++;
            //                    byteGambarUploaded.Add(item.Sort5);
            //                }
            //            }
            //        }
            //        //end add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
            //    }
            //    //foreach (var item in ListVariant)
            //    //{
            //    //    if (jmlPic < 9)
            //    //    {
            //    //        //List<string> Duplikat = HttpBody.variations.Select(p => p.name).ToList();
            //    //        //add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
            //    //        if (!byteGambarUploaded.Contains(item.Sort6))
            //    //        {
            //    //            if (!string.IsNullOrEmpty(item.LINK_GAMBAR_2))
            //    //            {
            //    //                HttpBody.images.Add(new ShopeeImageClass { url = item.LINK_GAMBAR_2 });
            //    //                jmlPic++;
            //    //                byteGambarUploaded.Add(item.Sort6);
            //    //            }
            //    //        }
            //    //    }
            //    //    //end add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
            //    //}
            //}
            //end remark 15/10/2019, tidak ambil gambar varian untuk barang induk

            //if (brgInDb.TYPE == "3")
            //{
            //if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
            //    HttpBody.images.Add(new ShopeeImageClass { url = brgInDb.LINK_GAMBAR_1 });
            //if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
            //    HttpBody.images.Add(new ShopeeImageClass { url = brgInDb.LINK_GAMBAR_2 });
            //if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
            //    HttpBody.images.Add(new ShopeeImageClass { url = brgInDb.LINK_GAMBAR_3 });
            ////add 6/9/2019, 5 gambar
            //if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_4))
            //    HttpBody.images.Add(new ShopeeImageClass { url = brgInDb.LINK_GAMBAR_4 });
            //if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_5))
            //    HttpBody.images.Add(new ShopeeImageClass { url = brgInDb.LINK_GAMBAR_5 });
            //end add 6/9/2019, 5 gambar
            //}
            //if (brgInDb.TYPE == "4")
            //{
            //    var ListVariant = ErasoftDbContext.STF02.Where(p => p.PART == brg).ToList();
            //    var ListSettingVariasi = ErasoftDbContext.STF02I.Where(p => p.BRG == brg).ToList();
            //    //add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
            //    List<string> byteGambarUploaded = new List<string>();
            //    //end add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
            //    foreach (var item in ListVariant)
            //    {
            //        List<string> Duplikat = HttpBody.variations.Select(p => p.name).ToList();
            //        //add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
            //        if (!byteGambarUploaded.Contains(item.Sort5))
            //        {
            //            byteGambarUploaded.Add(item.Sort5);
            //            if (!string.IsNullOrEmpty(item.LINK_GAMBAR_1))
            //                HttpBody.images.Add(new ShopeeImageClass { url = item.LINK_GAMBAR_1 });
            //        }
            //        if (!byteGambarUploaded.Contains(item.Sort6))
            //        {
            //            byteGambarUploaded.Add(item.Sort6);
            //            if (!string.IsNullOrEmpty(item.LINK_GAMBAR_2))
            //                HttpBody.images.Add(new ShopeeImageClass { url = item.LINK_GAMBAR_2 });
            //        }
            //        //end add by calvin 13 februari 2019, untuk compare size gambar, agar saat upload barang, tidak perlu upload gambar duplikat
            //    }
            //}
            try
            {
                for (int i = 1; i <= 30; i++)
                {
                    string attribute_id = Convert.ToString(detailBrg["ACODE_" + i.ToString()]);
                    string value = Convert.ToString(detailBrg["AVALUE_" + i.ToString()]);
                    if (!string.IsNullOrWhiteSpace(attribute_id) && !string.IsNullOrWhiteSpace(value))
                    {
                        if (value != "null")
                        {
                            HttpBody.attributes.Add(new ShopeeAttributeClass
                            {
                                attributes_id = Convert.ToInt64(attribute_id),
                                value = value.Trim()
                            });
                        }

                    }
                }

            }
            catch (Exception ex)
            {

            }

            string myData = JsonConvert.SerializeObject(HttpBody);

            string signature = CreateSign(string.Concat(urll, "|", myData), MOPartnerKey);

            string responseFromServer = "";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "POST";
            myReq.Headers.Add("Authorization", signature);
            myReq.Accept = "application/json";
            myReq.ContentType = "application/json";

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = seconds.ToString(),
                REQUEST_ACTION = "Create Product",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = brg,
                REQUEST_ATTRIBUTE_2 = brgInDb.NAMA,
                REQUEST_STATUS = "Pending",
            };

            //try
            //{
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);

            string sSQL = "UPDATE S SET LINK_STATUS='Buat Produk Pending', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', ";
            //jobid;request_action;request_result;request_exception
            string Link_Error = "0;Buat Produk;;";
            sSQL += "LINK_ERROR = '" + Link_Error + "' FROM STF02H S INNER JOIN ARF01 A ON S.IDMARKET = A.RECNUM AND A.CUST = '" + log_CUST + "' WHERE S.BRG = '" + kodeProduk + "' ";
            EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);

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
                //try
                //{
                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeCreateProdResponse)) as ShopeeCreateProdResponse;
                if (resServer != null)
                {
                    if (string.IsNullOrEmpty(resServer.error))
                    {
                        var item = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == brg.ToUpper() && b.IDMARKET == marketplace.RecNum).SingleOrDefault();
                        if (item != null)
                        {
                            item.BRG_MP = Convert.ToString(resServer.item_id) + ";0";
                            item.LINK_STATUS = "Buat Produk Berhasil";
                            item.LINK_DATETIME = DateTime.UtcNow.AddHours(7);
                            item.LINK_ERROR = "0;Buat Produk;;";
                            ErasoftDbContext.SaveChanges();

                            if (brgInDb.TYPE == "4")
                            {
                                //await InitTierVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, resServer.item_id, marketplace);
                                string EDBConnID = EDB.GetConnectionString("ConnId");
                                var sqlStorage = new SqlServerStorage(EDBConnID);
                                var client = new BackgroundJobClient(sqlStorage);

                                //delay 1 menit, karena API shopee ada delay saat create barang.
                                //client.Enqueue<ShopeeControllerJob>(x => x.InitTierVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, "Buat Variasi Produk", iden, brgInDb, resServer.item_id, marketplace, currentLog));
#if (DEBUG || Debug_AWS)
                                await InitTierVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, "Buat Variasi Produk", iden, brgInDb, resServer.item_id, marketplace, currentLog);
#else
                                client.Schedule<ShopeeControllerJob>(x => x.InitTierVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, "Buat Variasi Produk", iden, brgInDb, resServer.item_id, marketplace, currentLog), TimeSpan.FromSeconds(30));
#endif
                            }

                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            //add 21 Nov 2019, check create product duplicate
                            if (dbPathEra == "ERASOFT_120157")
                            {
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            }
                            //end add 21 Nov 2019, check create product duplicate

                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = "item not found";
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                            throw new Exception("item not found");
                        }
                    }
                    else
                    {
                        currentLog.REQUEST_RESULT = resServer.msg;
                        currentLog.REQUEST_EXCEPTION = resServer.error + ";" + resServer.msg;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        throw new Exception(currentLog.REQUEST_EXCEPTION);
                    }
                }

                //}
                //catch (Exception ex2)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Variasi Product {obj} ke Shopee Berhasil. Link Produk Gagal.")]
        public async Task<string> GetVariation(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, ShopeeAPIData iden, STF02 brgInDb, long item_id, ARF01 marketplace, Dictionary<string, int> mapSTF02HRecnum_IndexVariasi, List<ShopeeVariation> MOVariation, List<ShopeeTierVariation> tier_variation, API_LOG_MARKETPLACE currentLog)
        {
            var MOVariationNew = MOVariation.ToList();

            string ret = "";
            string brg = brgInDb.BRG;
            SetupContext(iden);

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/item/tier_var/get";

            ShopeeGetVariation HttpBody = new ShopeeGetVariation
            {
                partner_id = MOPartnerID,
                item_id = item_id,
                shopid = Convert.ToInt32(iden.merchant_code),
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
            if (currentLog != null)
            {
                manageAPI_LOG_MARKETPLACE(api_status.RePending, ErasoftDbContext, iden, currentLog);
            }
            //}
            //catch (Exception ex)
            //{
            //    //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}


            if (responseFromServer != "")
            {
                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(GetVariationResult)) as GetVariationResult;
                var new_tier_variation = new List<ShopeeUpdateVariation>();
                bool adaPerbaikanBrgMP = false;
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
                    //change by Tri 16 Des 2019, isi link status
                    //var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(resServer.item_id) + ";" + Convert.ToString(variasi.variation_id) + "' WHERE RECNUM = '" + Convert.ToString(recnum_stf02h_var) + "' AND ISNULL(BRG_MP,'') = '' ");
                    string Link_Error = "0;Buat Produk;;";//jobid;request_action;request_result;request_exception                     
                    var sSQL = "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(resServer.item_id) + ";" + Convert.ToString(variasi.variation_id);
                    sSQL += "',LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '" + Link_Error;
                    sSQL += "' WHERE RECNUM = '" + Convert.ToString(recnum_stf02h_var) + "' AND ISNULL(BRG_MP,'') = '' ";
                    var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, sSQL);
                    //end change by Tri 16 Des 2019, isi link status
                    if (result > 0)
                    {
                        adaPerbaikanBrgMP = true;
                    }
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
                            new_tier_variation.Add(new ShopeeUpdateVariation()
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
                if (adaPerbaikanBrgMP && currentLog != null)
                {
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                }

                if (MOVariationNew.Count() > 0)
                {
                    //foreach (var variasi in mapSTF02HRecnum_IndexVariasi)
                    //{
                    //    await AddVariation(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariation, MOVariationNew);
                    //}
                    //await UpdateTierVariationIndex(iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariationNew, tier_variation, new_tier_variation);
#if (Debug_AWS || DEBUG)
                    await UpdateTierVariationList(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariationNew, tier_variation, new_tier_variation, MOVariation);
#else
                    string EDBConnID = EDB.GetConnectionString("ConnId");
                    var sqlStorage = new SqlServerStorage(EDBConnID);

                    var client = new BackgroundJobClient(sqlStorage);
                    client.Enqueue<ShopeeControllerJob>(x => x.UpdateTierVariationList(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariationNew, tier_variation, new_tier_variation, MOVariation));
#endif
                }
                else //update image only
                {
                    //#if (Debug_AWS || DEBUG)
                    if (tier_variation != null)
                        await UpdateImageTierVariationList(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariationNew, tier_variation, new_tier_variation, MOVariation);
                    //#else
                    //                    string EDBConnID = EDB.GetConnectionString("ConnId");
                    //                    var sqlStorage = new SqlServerStorage(EDBConnID);

                    //                    var client = new BackgroundJobClient(sqlStorage);
                    //                    client.Enqueue<ShopeeControllerJob>(x => x.UpdateTierVariationList(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariationNew, tier_variation, new_tier_variation, MOVariation));
                    //#endif

                    //add by Tri 10 Des 2019 , update harga 
                    //var ShopeeApi = new ShopeeController();
                    var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == log_CUST).FirstOrDefault();
                    if (customer != null)
                    {
                        //var data = new ShopeeController.ShopeeAPIData
                        //{
                        //    API_client_password = iden.API_client_password,
                        //    API_client_username = iden.API_client_username,
                        //    API_secret_key = iden.API_secret_key,
                        //    merchant_code = iden.merchant_code,
                        //    mta_password_password_merchant = iden.mta_password_password_merchant,
                        //    mta_username_email_merchant = iden.mta_username_email_merchant,
                        //    token = iden.token
                        //};
                        var listBrg = EDB.GetDataSet("MOConnectionString", "STF02", "SELECT A.BRG, BRG_MP, B.HJUAL FROM STF02 A INNER JOIN STF02H B ON A.BRG = B.BRG WHERE PART = '" + brg + "' AND IDMARKET = " + customer.RecNum + " AND ISNULL(BRG_MP, '') <> ''");
                        if (listBrg.Tables[0].Rows.Count > 0)
                        {
                            string EDBConnID = EDB.GetConnectionString("ConnId");
                            var sqlStorage = new SqlServerStorage(EDBConnID);

                            var client = new BackgroundJobClient(sqlStorage);

                            for (int i = 0; i < listBrg.Tables[0].Rows.Count; i++)
                            {
#if (Debug_AWS || DEBUG)
                                await UpdateVariationPrice(dbPathEra, listBrg.Tables[0].Rows[i]["BRG"].ToString(), log_CUST, log_ActionCategory, log_ActionName, iden, listBrg.Tables[0].Rows[i]["BRG_MP"].ToString(), float.Parse(listBrg.Tables[0].Rows[i]["HJUAL"].ToString()));
#else
                                client.Enqueue<ShopeeControllerJob>(x => x.UpdateVariationPrice(dbPathEra, listBrg.Tables[0].Rows[i]["BRG"].ToString(), log_CUST, log_ActionCategory, log_ActionName, iden, listBrg.Tables[0].Rows[i]["BRG_MP"].ToString(), float.Parse(listBrg.Tables[0].Rows[i]["HJUAL"].ToString())));
#endif
                            }
                        }

                        //add by nurul 27/1/2020, tambah update deskripsi dll
                        if (!string.IsNullOrEmpty(customer.Sort1_Cust))
                        {
                            var stf02h = ErasoftDbContext.STF02H.Where(p => p.BRG == brg && p.IDMARKET == customer.RecNum).FirstOrDefault();
                            if (stf02h != null)
                            {
                                if (!string.IsNullOrEmpty(stf02h.BRG_MP))
                                {
                                    //add 6 aug 2020, hapus error log lama sebelum panggil update produk
                                    string sSQL = "DELETE FROM API_LOG_MARKETPLACE WHERE REQUEST_ATTRIBUTE_5 = 'HANGFIRE' AND REQUEST_ACTION = 'Update Produk' AND CUST = '" + customer.CUST + "' AND CUST_ATTRIBUTE_1 = '" + brg + "'";
                                    EDB.ExecuteSQL("sConn", CommandType.Text, sSQL);
                                    //end add 6 aug 2020, hapus error log lama sebelum panggil update produk

                                    iden.merchant_code = customer.Sort1_Cust;
                                    //Task.Run(() => shoAPI.UpdateProduct(iden, (string.IsNullOrEmpty(dataBarang.Stf02.BRG) ? barangInDb.BRG : dataBarang.Stf02.BRG), tblCustomer.CUST, new List<ShopeeController.ShopeeLogisticsClass>()).Wait());
                                    //Task.Run(() => shoAPI.UpdateProduct(idenNew, brg, customer.CUST, new List<ShopeeController.ShopeeLogisticsClass>()).Wait());
                                    //await UpdateProduct(iden, brg, customer.CUST, new List<ShopeeLogisticsClass>());
                                    string EDBConnID = EDB.GetConnectionString("ConnId");
                                    var sqlStorage = new SqlServerStorage(EDBConnID);
                                    var client = new BackgroundJobClient(sqlStorage);
#if (Debug_AWS || DEBUG)
                                    await UpdateProduct(dbPathEra, brg, customer.CUST, "Barang", "Update Produk", iden, brg, customer.CUST, new List<ShopeeLogisticsClass>());
#else
                                    client.Enqueue<ShopeeControllerJob>(x => x.UpdateProduct(dbPathEra, brg, customer.CUST, "Barang", "Update Produk", iden, brg, customer.CUST, new List<ShopeeLogisticsClass>()));
#endif
                                }
                            }
                        }
                        //end add by nuurl 27/1/2020, tambah update deskripsi dll

                    }
                    //end add by Tri 10 Des 2019 , update harga 

                }
            }

            return ret;
        }
        public async Task<GetVariationResult> CekVariationShopee(ShopeeAPIData iden, long item_id)
        {
            var ret = new GetVariationResult();
            SetupContext(iden);

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/item/tier_var/get";

            ShopeeGetVariation HttpBody = new ShopeeGetVariation
            {
                partner_id = MOPartnerID,
                item_id = item_id,
                shopid = Convert.ToInt32(iden.merchant_code),
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
            //if (currentLog != null)
            //{
            //    manageAPI_LOG_MARKETPLACE(api_status.RePending, ErasoftDbContext, iden, currentLog);
            //}
            //}
            //catch (Exception ex)
            //{
            //    //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}


            if (responseFromServer != "")
            {
                ret = JsonConvert.DeserializeObject(responseFromServer, typeof(GetVariationResult)) as GetVariationResult;

            }

            return ret;
        }
        public class ShopeeUpdateVariation
        {
            public int[] tier_index { get; set; }
            public int stock { get; set; }
            public float price { get; set; }
            public string variation_sku { get; set; }
            public long? variation_id { get; set; }
        }
        public class ShopeeAddTierVariation
        {
            public long item_id { get; set; }
            public ShopeeVariation[] variation { get; set; }
            public long shopid { get; set; }
            public long partner_id { get; set; }
            public long timestamp { get; set; }

        }

        public class ShopeeUpdateTierVariationResult : ShopeeError
        {
            public long item_id { get; set; }
            //public string request_id { get; set; }
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Variasi Product {obj} ke Shopee Gagal.")]
        public async Task<string> UpdateTierVariationList(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, ShopeeAPIData iden, STF02 brgInDb, long item_id, ARF01 marketplace, Dictionary<string, int> mapSTF02HRecnum_IndexVariasi, List<ShopeeVariation> MOVariationNew, List<ShopeeTierVariation> tier_variation, List<ShopeeUpdateVariation> new_tier_variation, List<ShopeeVariation> MOVariation)
        {
            //Use this api to update tier-variation list or upload variation image of a tier-variation item
            string ret = "";
            string brg = brgInDb.BRG;
            SetupContext(iden);

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/item/tier_var/update_list";

            ShopeeUpdateTierVariationList HttpBody = new ShopeeUpdateTierVariationList
            {
                partner_id = MOPartnerID,
                item_id = item_id,
                shopid = Convert.ToInt32(iden.merchant_code),
                tier_variation = tier_variation.ToArray(),
                timestamp = seconds,
            };

            string myData = JsonConvert.SerializeObject(HttpBody);
            myData = myData.Replace(",\"images_url\":null", " ");//remove images_url from tier 2

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
                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeUpdateTierVariationResult)) as ShopeeUpdateTierVariationResult;
                if (string.IsNullOrEmpty(resServer.error))
                {
                    if (resServer.item_id == item_id)
                    {
#if (Debug_AWS || DEBUG)
                        await AddTierVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariation, MOVariationNew);
#else
                    string EDBConnID = EDB.GetConnectionString("ConnId");
                    var sqlStorage = new SqlServerStorage(EDBConnID);

                    var client = new BackgroundJobClient(sqlStorage);
                    client.Enqueue<ShopeeControllerJob>(x => x.AddTierVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariation, MOVariationNew));
#endif
                    }
                }
                //add by Tri 16 Des 2019, return exception jika ada error
                else
                {
                    throw new Exception(resServer.msg);
                }
                //end add by Tri 16 Des 2019, return exception jika ada error
            }

            return ret;
        }

        //[AutomaticRetry(Attempts = 2)]
        //[Queue("1_create_product")]
        //[NotifyOnFailed("Update Variasi Product {obj} ke Shopee Gagal.")]
        public async Task<string> UpdateImageTierVariationList(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, ShopeeAPIData iden, STF02 brgInDb, long item_id, ARF01 marketplace, Dictionary<string, int> mapSTF02HRecnum_IndexVariasi, List<ShopeeVariation> MOVariationNew, List<ShopeeTierVariation> tier_variation, List<ShopeeUpdateVariation> new_tier_variation, List<ShopeeVariation> MOVariation)
        {
            //Use this api to update tier-variation list or upload variation image of a tier-variation item
            string ret = "";
            string brg = brgInDb.BRG;
            SetupContext(iden);

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/item/tier_var/update_list";

            ShopeeUpdateTierVariationList HttpBody = new ShopeeUpdateTierVariationList
            {
                partner_id = MOPartnerID,
                item_id = item_id,
                shopid = Convert.ToInt32(iden.merchant_code),
                tier_variation = tier_variation.ToArray(),
                timestamp = seconds,
            };

            string myData = JsonConvert.SerializeObject(HttpBody);
            myData = myData.Replace(",\"images_url\":null", " ");//remove images_url from tier 2

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
                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeUpdateTierVariationResult)) as ShopeeUpdateTierVariationResult;
                if (string.IsNullOrEmpty(resServer.error))
                {
                    //                    if (resServer.item_id == item_id)
                    //                    {
                    //#if (Debug_AWS || DEBUG)
                    //                        await AddTierVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariation, MOVariationNew);
                    //#else
                    //                    string EDBConnID = EDB.GetConnectionString("ConnId");
                    //                    var sqlStorage = new SqlServerStorage(EDBConnID);

                    //                    var client = new BackgroundJobClient(sqlStorage);
                    //                    client.Enqueue<ShopeeControllerJob>(x => x.AddTierVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariation, MOVariationNew));
                    //#endif
                    //}
                }
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Variasi Product {obj} ke Shopee Gagal.")]
        public async Task<string> AddTierVariation(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, ShopeeAPIData iden, STF02 brgInDb, long item_id, ARF01 marketplace, Dictionary<string, int> mapSTF02HRecnum_IndexVariasi, List<ShopeeVariation> MOVariation, List<ShopeeVariation> MOVariationNew)
        {
            string ret = "";
            string brg = brgInDb.BRG;
            SetupContext(iden);

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/item/tier_var/add";
            ShopeeAddTierVariation HttpBody = new ShopeeAddTierVariation
            {
                partner_id = MOPartnerID,
                item_id = item_id,
                shopid = Convert.ToInt32(iden.merchant_code),
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
#if (Debug_AWS || DEBUG)
                    await GetVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariation, null, null);
#else
                    string EDBConnID = EDB.GetConnectionString("ConnId");
                    var sqlStorage = new SqlServerStorage(EDBConnID);

                    var client = new BackgroundJobClient(sqlStorage);
                    client.Enqueue<ShopeeControllerJob>(x => x.GetVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, MOVariation, null, null));
#endif
                }
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Variasi Product {obj} ke Shopee Gagal.")]
        public async Task<string> InitTierVariation(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, ShopeeAPIData iden, STF02 brgInDb, long item_id, ARF01 marketplace, API_LOG_MARKETPLACE currentLog)
        {
            string ret = "";
            string brg = brgInDb.BRG;
            SetupContext(iden);

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

            ShopeeInitTierVariation HttpBody = new ShopeeInitTierVariation
            {
                partner_id = MOPartnerID,
                item_id = item_id,
                shopid = Convert.ToInt32(iden.merchant_code),
                tier_variation = new List<ShopeeTierVariation>().ToArray(),
                variation = new List<ShopeeVariation>().ToArray(),
                timestamp = seconds
            };
            List<ShopeeTierVariation> tier_variation = new List<ShopeeTierVariation>();
            List<ShopeeVariation> variation = new List<ShopeeVariation>();
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
                ShopeeTierVariation tier1 = new ShopeeTierVariation();
                ShopeeTierVariation tier2 = new ShopeeTierVariation();
                List<string> tier1_options = new List<string>();
                List<string> tier2_options = new List<string>();
                List<string> tier1_images = new List<string>();
                //List<string> tier2_images = new List<string>();
                List<string> tier1_code = new List<string>();
                string sSQL = "SELECT A.BRG, SORT8, SORT9, ISNULL(BRG_MP, '') BRG_MP, LINK_GAMBAR_1 ";
                sSQL += "FROM STF02 A INNER JOIN STF02H B ON A.BRG = B.BRG ";
                sSQL += "WHERE PART = '" + brg + "' AND IDMARKET = " + marketplace.RecNum;
                var dsBarang = EDB.GetDataSet("CString", "STF02", sSQL);
                var dataBrg = await CekVariationShopee(iden, item_id);
                if (dataBrg != null)
                {
                    #region remark cek tier_variation
                    //if (dataBrg.tier_variation != null)
                    //{
                    //    if (dataBrg.tier_variation.Length > 0)
                    //    {
                    //        var tempData = new List<STF02>();
                    //        var newData = ListVariant;
                    //        foreach (var opsiLv1 in dataBrg.tier_variation[0].options)
                    //        {
                    //            var getNamaVar = ListSettingVariasi.Where(p => p.LEVEL_VAR == 1 && p.MARKET == "SHOPEE" && p.MP_JUDUL_VAR.ToUpper() == dataBrg.tier_variation[0].name.ToUpper() && p.MP_VALUE_VAR.ToUpper() == opsiLv1.ToUpper()).FirstOrDefault();
                    //            var filterLv1 = ListVariant.Where(p => (p.Sort8 ?? "") == getNamaVar.KODE_VAR).ToList();
                    //            if(filterLv1.Count == 0)//varian lvl 1 kosong di MO
                    //            {
                    //                var filterLv2 = ListVariant.Where(p => (p.Sort9 ?? "") == getNamaVar.KODE_VAR).FirstOrDefault();
                    //                if (filterLv2 != null)
                    //                {
                    //                    tempData.Add(filterLv2);
                    //                    newData.Remove(filterLv2);
                    //                }
                    //            }
                    //            else
                    //            {
                    //                var cekLvl2 = filterLv1.Where(p => (p.Sort9 ?? "") == getNamaVar.KODE_VAR).ToList();
                    //                if(cekLvl2.Count == 0)// mo dan shopee memiliki 1 lvl varian
                    //                {
                    //                    var filterLv2 = filterLv1.Where(p => (p.Sort8 ?? "") == getNamaVar.KODE_VAR).FirstOrDefault();
                    //                    if (filterLv2 != null)
                    //                    {
                    //                        tempData.Add(filterLv2);
                    //                        newData.Remove(filterLv2);
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    if (dataBrg.tier_variation.Length > 1)// mo dan shopee memiliki 2 lvl varian
                    //                    {
                    //                        foreach (var opsiLv2 in dataBrg.tier_variation[1].options)
                    //                        {
                    //                            var getNamaVar2 = ListSettingVariasi.Where(p => p.LEVEL_VAR == 2 && p.MARKET == "SHOPEE" && p.MP_JUDUL_VAR.ToUpper() == dataBrg.tier_variation[1].name.ToUpper() && p.MP_VALUE_VAR.ToUpper() == opsiLv2.ToUpper()).FirstOrDefault();
                    //                            var filterLv2 = filterLv1.Where(p => (p.Sort9 ?? "") == getNamaVar.KODE_VAR).FirstOrDefault();
                    //                            if (filterLv2 != null)
                    //                            {
                    //                                tempData.Add(filterLv2);
                    //                                newData.Remove(filterLv2);
                    //                            }
                    //                        }
                    //                    }
                    //                    else//mo 2 lvl varian. shopee 1 lvl
                    //                    {
                    //                        foreach (var lvl2MO in cekLvl2.OrderBy(p => p.ID))
                    //                        {
                    //                            tempData.Add(lvl2MO);
                    //                            newData.Remove(lvl2MO);
                    //                        }
                    //                    }
                    //                }

                    //            }

                    //        }
                    //        ListVariant = tempData;
                    //        ListVariant.AddRange(newData);
                    //    }
                    //}
                    #endregion
                    if (dataBrg.variations != null)
                    {
                        if (dataBrg.variations.Length > 0)
                        {
                            var tempData = new List<STF02>();
                            var newData = ListVariant;
                            //var cekTier1 = dataBrg.variations.Where(p => p.tier_index[0] == 0).ToList();
                            if (dataBrg.tier_variation.Length == 1)// shopee 1 tier
                            {
                                foreach (var tier1_s in dataBrg.variations.OrderBy(p => p.tier_index[0]))
                                {
                                    var brgMPVar = dataBrg.item_id + ";" + tier1_s.variation_id;
                                    var dStf02h = ListStf02hVariasi.Where(p => (p.BRG_MP ?? "") == brgMPVar).FirstOrDefault();
                                    if (dStf02h != null)
                                    {
                                        var currentBrg = dStf02h.BRG;
                                        var sorted = ListVariant.Where(p => p.BRG == currentBrg).FirstOrDefault();
                                        tempData.Add(sorted);
                                        newData.Remove(sorted);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var tier1_s in dataBrg.variations.OrderBy(p => p.tier_index[0]).ThenBy(p => p.tier_index[1]))
                                {
                                    var brgMPVar = dataBrg.item_id + ";" + tier1_s.variation_id;
                                    var dStf02h = ListStf02hVariasi.Where(p => (p.BRG_MP ?? "") == brgMPVar).FirstOrDefault();
                                    if (dStf02h != null)
                                    {
                                        var currentBrg = dStf02h.BRG;
                                        var sorted = ListVariant.Where(p => p.BRG == currentBrg).FirstOrDefault();
                                        tempData.Add(sorted);
                                        newData.Remove(sorted);
                                    }
                                }
                            }
                            ListVariant = tempData;
                            ListVariant.AddRange(newData);
                        }
                    }
                }
                //foreach (var item in ListVariant.OrderBy(p => p.ID))
                foreach (var item in ListVariant)
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

                        ShopeeVariation adaVariant = new ShopeeVariation()
                        {
                            tier_index = ListTierIndex.ToArray(),
                            price = (float)stf02h.HJUAL,
                            stock = 1,//create product min stock = 1
                            variation_sku = item.BRG
                        };

                        //add by calvin 1 mei 2019
                        var qty_stock = new StokControllerJob(dbPathEra, username).GetQOHSTF08A(item.BRG, "ALL");
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

                        if (!tier1_code.Contains(item.Sort8))
                        {
                            if (!string.IsNullOrEmpty(item.LINK_GAMBAR_1))
                                //tier1_images.Add(new ShopeeImageClass { url = item.LINK_GAMBAR_1 });
                                tier1_images.Add(item.LINK_GAMBAR_1);
                            //if (!string.IsNullOrEmpty(item.LINK_GAMBAR_2))
                            //    //tier1_images.Add(new ShopeeImageClass { url = item.LINK_GAMBAR_1 });
                            //    tier1_images.Add(item.LINK_GAMBAR_2);
                            tier1_code.Add(item.Sort8);
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(tier1.name))
                {
                    tier1.options = tier1_options.ToArray();
                    tier1.images_url = tier1_images.ToArray();
                    //tier1.images_url = new string[1];
                    //tier1.images_url[0] = "";
                    tier_variation.Add(tier1);
                }
                if (!string.IsNullOrWhiteSpace(tier2.name))
                {
                    tier2.options = tier2_options.ToArray();
                    //tier2.images_url = new string[0];
                    tier_variation.Add(tier2);
                }
            }
            HttpBody.variation = variation.ToArray();
            HttpBody.tier_variation = tier_variation.ToArray();

            string myData = JsonConvert.SerializeObject(HttpBody);
            myData = myData.Replace(",\"images_url\":null", " ");//remove images_url from tier 2

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
            if (currentLog != null)
            {
                manageAPI_LOG_MARKETPLACE(api_status.RePending, ErasoftDbContext, iden, currentLog);
            }
            //}
            //catch (Exception ex)
            //{
            //    //currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    //manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}


            if (responseFromServer != "")
            {
                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(InitTierVariationResult)) as InitTierVariationResult;
                if (resServer.error != null)
                {
                    if (resServer.msg.Contains("there is no tier_variation level change")) //add by calvin 14 november 2019, req by pak richard
                    {
                        //do nothing
                        //add by Tri 4 Des 2019, case user tambah varian tanpa ubah tier
#if (DEBUG || Debug_AWS)
                        await GetVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, variation, tier_variation, currentLog);
#else
                    string EDBConnID = EDB.GetConnectionString("ConnId");
                    var sqlStorage = new SqlServerStorage(EDBConnID);

                    var client = new BackgroundJobClient(sqlStorage);
                    client.Enqueue<ShopeeControllerJob>(x => x.GetVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, variation, tier_variation, currentLog));
#endif
                        //end add by Tri 4 Des 2019, case user tambah varian tanpa ubah tier
                    }
                    else
                    {
                        throw new Exception(resServer.msg);
                    }
                }
                else
                {
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
                                string Link_Error = "0;Buat Produk;;";//jobid;request_action;request_result;request_exception
                                var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + Convert.ToString(resServer.item_id) + ";" + Convert.ToString(variasi.variation_id) + "',LINK_STATUS='Buat Produk Berhasil', LINK_DATETIME = '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "',LINK_ERROR = '" + Link_Error + "' WHERE RECNUM = '" + Convert.ToString(recnum_stf02h_var) + "'");

                                //var barang = ErasoftDbContext.STF02H.Where(m => m.RecNum == recnum_stf02h_var).FirstOrDefault();
                                //await UpdateImage(iden, barang.BRG, Convert.ToString(resServer.item_id) + ";" + Convert.ToString(variasi.variation_id));
                            }

                            if (currentLog != null)
                            {
                                manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                            }
                        }
                    }
                    else
                    {
#if (DEBUG || Debug_AWS)
                        await GetVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, variation, tier_variation, currentLog);
#else
                    string EDBConnID = EDB.GetConnectionString("ConnId");
                    var sqlStorage = new SqlServerStorage(EDBConnID);

                    var client = new BackgroundJobClient(sqlStorage);
                    client.Enqueue<ShopeeControllerJob>(x => x.GetVariation(dbPathEra, kodeProduk, log_CUST, log_ActionCategory, log_ActionName, iden, brgInDb, item_id, marketplace, mapSTF02HRecnum_IndexVariasi, variation, tier_variation, currentLog));
#endif

                    }
                }
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Product {obj} ke Shopee Gagal.")]
        public async Task<string> UpdateProduct(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, ShopeeAPIData iden, string brg, string cust, List<ShopeeLogisticsClass> logistics)
        {
            string ret = "";
            //add by nurul 28/1/2020
            SetupContext(iden);
            //end add by nurul 28/1/2020

            var brgInDb = ErasoftDbContext.STF02.Where(b => b.BRG.ToUpper() == brg.ToUpper()).FirstOrDefault();
            var marketplace = ErasoftDbContext.ARF01.Where(c => c.CUST.ToUpper() == cust.ToUpper()).FirstOrDefault();
            if (brgInDb == null || marketplace == null)
                return "invalid passing data";
            var detailBrg = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == brg.ToUpper() && b.IDMARKET == marketplace.RecNum).FirstOrDefault();
            if (detailBrg == null)
                return "invalid passing data";

            long item_id = 0;
            string[] brg_mp = detailBrg.BRG_MP.Split(';');
            if (brg_mp.Count() == 2)
            {
                item_id = Convert.ToInt64(brg_mp[0]);
            }

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //change by nurul 28/1/2020
            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    //REQUEST_ID = seconds.ToString(),
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssffff"),
            //    REQUEST_ACTION = "Update Product",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = iden.merchant_code,
            //    REQUEST_STATUS = "Pending",
            //};

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = seconds.ToString(),
                REQUEST_ACTION = "Update Product",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = brg,
                REQUEST_ATTRIBUTE_2 = brgInDb.NAMA,
                REQUEST_STATUS = "Pending",
            };
            //end change by nurul 28/1/2020

            string urll = "https://partner.shopeemobile.com/api/v1/item/update";

            //add by calvin 21 desember 2018, default nya semua logistic enabled
            var ShopeeGetLogisticsResult = await GetLogistics(iden);

            foreach (var log in ShopeeGetLogisticsResult.logistics.Where(p => p.enabled == true && p.fee_type.ToUpper() != "CUSTOM_PRICE" && p.fee_type.ToUpper() != "SIZE_SELECTION"))
            {
                logistics.Add(new ShopeeLogisticsClass()
                {
                    enabled = log.enabled,
                    is_free = false,
                    logistic_id = log.logistic_id,
                });
            }
            //end add by calvin 21 desember 2018, default nya semua logistic enabled

            ShopeeUpdateProductData HttpBody = new ShopeeUpdateProductData
            {
                item_id = item_id,
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                item_sku = brg,
                category_id = Convert.ToInt64(detailBrg.CATEGORY_CODE),
                condition = "NEW",
                name = (brgInDb.NAMA + " " + brgInDb.NAMA2).Trim().Replace("’", "`"),
                description = brgInDb.Deskripsi.Replace("’", "`"),
                package_height = Convert.ToInt32(brgInDb.TINGGI) == 0 ? 1 : Convert.ToInt32(brgInDb.TINGGI),
                package_length = Convert.ToInt32(brgInDb.PANJANG) == 0 ? 1 : Convert.ToInt32(brgInDb.PANJANG),
                package_width = Convert.ToInt32(brgInDb.LEBAR) == 0 ? 1 : Convert.ToInt32(brgInDb.LEBAR),
                weight = (brgInDb.BERAT / 1000),
                attributes = new List<ShopeeAttributeClass>(),
                logistics = logistics
            };

            //add by nurul 20/1/2020, handle <p> dan enter double di shopee
            HttpBody.description = new StokControllerJob().RemoveSpecialCharacters(HttpBody.description);

            HttpBody.description = HttpBody.description.Replace("<p>", "").Replace("</p>", "").Replace("\r", "\r\n").Replace("strong", "b");
            HttpBody.description = HttpBody.description.Replace("<li>", "- ").Replace("</li>", "\r\n");
            HttpBody.description = HttpBody.description.Replace("<ul>", "").Replace("</ul>", "\r\n");
            HttpBody.description = HttpBody.description.Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ");
            HttpBody.description = HttpBody.description.Replace("</em>&nbsp;", " ").Replace("&nbsp;", " ").Replace("</em>", "");
            HttpBody.description = HttpBody.description.Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");

            //HttpBody.description = HttpBody.description.Replace("<p>", "").Replace("</p>", "").Replace("&nbsp;\r\n\r\n", "\n").Replace("&nbsp;<em>", " ").Replace("</em>&nbsp;", " ");
            //HttpBody.description = HttpBody.description.Replace("&nbsp;", " ").Replace("</em>", "").Replace("<br />\r\n", "\n").Replace("\r\n\r\n", "\n").Replace("\r\n", "");

            //add by calvin 10 september 2019
            HttpBody.description = HttpBody.description.Replace("<h1>", "\r\n").Replace("</h1>", "\r\n");
            HttpBody.description = HttpBody.description.Replace("<h2>", "\r\n").Replace("</h2>", "\r\n");
            HttpBody.description = HttpBody.description.Replace("<h3>", "\r\n").Replace("</h3>", "\r\n");
            HttpBody.description = HttpBody.description.Replace("<p>", "\r\n").Replace("</p>", "\r\n");

            //HttpBody.description = HttpBody.description.Replace("&nbsp;", "");
            HttpBody.description = System.Text.RegularExpressions.Regex.Replace(HttpBody.description, "<.*?>", String.Empty);
            //end add by nurul 20/1/2020, handle <p> dan enter double di shopee

            try
            {
                //change 27 mei 2020, ambil dari stf02h saja, karena attribut tidak ambil lg dari db MO
                //string sSQL = "SELECT * FROM (";
                //for (int i = 1; i <= 30; i++)
                //{
                //    sSQL += "SELECT A.ACODE_" + i.ToString() + " AS CATEGORY_CODE,A.ANAME_" + i.ToString() + " AS CATEGORY_NAME,B.ATYPE_" + i.ToString() + " AS CATEGORY_TYPE,A.AVALUE_" + i.ToString() + " AS VALUE FROM STF02H A INNER JOIN MO.DBO.ATTRIBUTE_SHOPEE B ON A.CATEGORY_CODE = B.CATEGORY_CODE WHERE A.BRG='" + brg + "' AND A.IDMARKET = '" + marketplace.RecNum + "' " + System.Environment.NewLine;
                //    if (i < 30)
                //    {
                //        sSQL += "UNION ALL " + System.Environment.NewLine;
                //    }
                //}

                //DataSet dsFeature = EDB.GetDataSet("sCon", "STF02H", sSQL + ") ASD WHERE ISNULL(CATEGORY_CODE,'') <> '' ");

                //for (int i = 0; i < dsFeature.Tables[0].Rows.Count; i++)
                //{
                //    //HttpBody.attributes.Add(new ShopeeAttributeClass
                //    //{
                //    //    attributes_id = Convert.ToInt64(dsFeature.Tables[0].Rows[i]["CATEGORY_CODE"]),
                //    //    value = Convert.ToString(dsFeature.Tables[0].Rows[i]["VALUE"]).Trim()
                //    //});
                //    if (Convert.ToString(dsFeature.Tables[0].Rows[i]["VALUE"]).Trim() != "null")
                //    {
                //        HttpBody.attributes.Add(new ShopeeAttributeClass
                //        {
                //            attributes_id = Convert.ToInt64(dsFeature.Tables[0].Rows[i]["CATEGORY_CODE"]),
                //            value = Convert.ToString(dsFeature.Tables[0].Rows[i]["VALUE"]).Trim()
                //        });
                //    }
                //}
                for (int i = 1; i <= 30; i++)
                {
                    string attribute_id = Convert.ToString(detailBrg["ACODE_" + i.ToString()]);
                    string value = Convert.ToString(detailBrg["AVALUE_" + i.ToString()]);
                    if (!string.IsNullOrWhiteSpace(attribute_id) && !string.IsNullOrWhiteSpace(value))
                    {
                        if (value != "null")
                        {
                            HttpBody.attributes.Add(new ShopeeAttributeClass
                            {
                                attributes_id = Convert.ToInt64(attribute_id),
                                value = value.Trim()
                            });
                        }

                    }
                }
                //end change 27 mei 2020, ambil dari stf02h saja, karena attribut tidak ambil lg dari db MO
            }
            catch (Exception ex)
            {

            }


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
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
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
            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}

            if (responseFromServer != "")
            {
                //try
                //{
                //change by nurul 28/1/2020, tampilin log error
                //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeCreateProdResponse)) as ShopeeCreateProdResponse;
                if (resServer != null)
                {
                    if (string.IsNullOrEmpty(resServer.error))
                    {
                        await UpdateProductDisplay(iden, brg, cust);//add by Tri 6 aug 2020, update display product
                        var item = ErasoftDbContext.STF02H.Where(b => b.BRG.ToUpper() == brg.ToUpper() && b.IDMARKET == marketplace.RecNum).SingleOrDefault();
                        if (item != null)
                        {
                            manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                        }
                        else
                        {
                            currentLog.REQUEST_EXCEPTION = "item not found";
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                            throw new Exception("item not found");
                        }
                    }
                    else
                    {
                        if (resServer.msg.Contains("weight") || resServer.msg.Contains("package_height") || resServer.msg.Contains("package_length") || resServer.msg.Contains("package_width"))
                        {
                            //currentLog.REQUEST_RESULT = "Update Product " + brg + " ke Shopee Gagal.";
                            currentLog.REQUEST_RESULT = "Update Product " + brg + " ke Shopee Gagal.";
                            currentLog.REQUEST_EXCEPTION = resServer.error + ";" + resServer.msg + "\n Update barang Shopee memiliki ketentuan panjang, lebar dan tinggi max 40cm dan berat max 5 kg.";
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                            throw new Exception(currentLog.REQUEST_EXCEPTION);
                        }
                        else
                        {
                            currentLog.REQUEST_RESULT = "Update Product " + brg + " ke Shopee Gagal.";
                            currentLog.REQUEST_EXCEPTION = resServer.error + ";" + resServer.msg;
                            manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                            throw new Exception(currentLog.REQUEST_EXCEPTION);
                        }
                    }
                }
                //end change by nurul 28/1/2020, tampilin log error

                //var resserver = jsonconvert.deserializeobject(responsefromserver, typeof(shopeecreateprodresponse)) as shopeecreateprodresponse;
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

                //}
                //catch (Exception ex2)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}
            }
            return ret;
        }

        public async Task<string> UpdateProductDisplay(ShopeeAPIData iden, string brg, string cust)
        {
            SetupContext(iden);

            var customer = ErasoftDbContext.ARF01.Where(m => m.CUST == cust).FirstOrDefault();
            var stf02h_brg = ErasoftDbContext.STF02H.Where(m => m.BRG == brg && m.IDMARKET == customer.RecNum).FirstOrDefault();
            string urll = "https://partner.shopeemobile.com/api/v1/items/unlist";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string itemId = "";
            if (!string.IsNullOrEmpty(stf02h_brg.BRG_MP))
            {
                var splitCode = stf02h_brg.BRG_MP.Split(';');
                if (splitCode[0] != "0")
                {
                    itemId = splitCode[0];
                }
            }

            if (!string.IsNullOrEmpty(itemId))
            {
                string myData = "{ \"shopid\":" + iden.merchant_code + ",\"partner_id\":" + MOPartnerID + ",\"timestamp\":" + seconds + ",";
                myData += "\"items\": [{\"item_id\":" + itemId + ", \"unlist\":" + (stf02h_brg.DISPLAY ? "false" : "true") + " }] }";

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
                //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
                //}
                //catch (Exception ex)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}

                if (responseFromServer != "")
                {
                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeUnlistProductResponse)) as ShopeeUnlistProductResponse;
                    if (resServer != null)
                    {
                        if(resServer.failed != null)
                        {
                            if(resServer.failed.Length > 0)
                            {
                                string errMsg = "Error Unlist Product : ";
                                foreach (var er in resServer.failed)
                                {
                                    errMsg += er.item_id + " : " + er.error_description + "\n";
                                }
                                throw new Exception(errMsg);
                            }
                            
                        }
                    }

                }
            }

            return "";
        }
        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Harga Jual Produk {obj} ke Shopee gagal.")]
        public async Task<string> UpdatePrice_Job(string dbPathEra, string kdbrgMO, string log_CUST, string log_ActionCategory, string log_ActionName, string brg_mp, ShopeeAPIData iden, float price)
        {
            SetupContext(iden);
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                //REQUEST_ID = seconds.ToString(),
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Price", //ganti
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/items/update_price";

            string[] brg_mp_split = brg_mp.Split(';');
            ShopeeUpdatePriceData HttpBody = new ShopeeUpdatePriceData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                item_id = Convert.ToInt64(brg_mp_split[0]),
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
                throw new Exception(currentLog.REQUEST_EXCEPTION);
            }

            if (responseFromServer != null)
            {
                try
                {
                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(GetPickupTimeSlotError)) as GetPickupTimeSlotError;
                    if (!string.IsNullOrEmpty(resServer.error))
                    {
                        currentLog.REQUEST_EXCEPTION = resServer.msg;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        throw new Exception(resServer.msg);
                    }
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
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                    throw new Exception(currentLog.REQUEST_EXCEPTION);
                }
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Create Variasi Product {obj} ke Shopee Berhasil. Update Harga Produk.")]
        public async Task<string> UpdateVariationPrice(string dbPathEra, string kodeProduk, string log_CUST, string log_ActionCategory, string log_ActionName, ShopeeAPIData iden, string brg_mp, float price)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = seconds.ToString(),
            //    REQUEST_ACTION = "Update Price",
            //    REQUEST_DATETIME = milisBack,
            //    REQUEST_ATTRIBUTE_1 = iden.merchant_code,
            //    REQUEST_STATUS = "Pending",
            //};

            string urll = "https://partner.shopeemobile.com/api/v1/items/update_variation_price";

            string[] brg_mp_split = brg_mp.Split(';');
            ShopeeUpdateVariantionPriceData HttpBody = new ShopeeUpdateVariantionPriceData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
            //    manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, iden, currentLog);
            //}
            //catch (Exception ex)
            //{
            //    currentLog.REQUEST_EXCEPTION = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
            //}

            if (responseFromServer != null)
            {
                //add by Tri 16 Des 2019, return exception jika ada error
                var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(GetPickupTimeSlotError)) as GetPickupTimeSlotError;
                if (!string.IsNullOrEmpty(resServer.error))
                {
                    throw new Exception(resServer.msg);
                }
                //end add by Tri 16 Des 2019, return exception jika ada error

                //try
                //{
                //    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                //}
                //catch (Exception ex2)
                //{
                //    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                //}
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 2)]
        [Queue("1_create_product")]
        [NotifyOnFailed("Update Harga Jual Produk Varian {obj} ke Shopee gagal.")]
        public async Task<string> UpdateVariationPrice_Job(string dbPathEra, string kdbrgMO, string log_CUST, string log_ActionCategory, string log_ActionName, string brg_mp, ShopeeAPIData iden, float price)
        {
            SetupContext(iden);
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
                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/items/update_variation_price";

            string[] brg_mp_split = brg_mp.Split(';');
            ShopeeUpdateVariantionPriceData HttpBody = new ShopeeUpdateVariantionPriceData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(GetPickupTimeSlotError)) as GetPickupTimeSlotError;
                    if (!string.IsNullOrEmpty(resServer.error))
                    {
                        currentLog.REQUEST_EXCEPTION = resServer.msg;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, iden, currentLog);
                        throw new Exception(resServer.msg);
                    }
                    //add 19 sept 2020, update harga massal
                    if (log_ActionName.Contains("UPDATE_MASSAL"))
                    {
                        var dataLog = log_ActionName.Split('_');
                        if(dataLog.Length >= 4)
                        {
                            var nobuk = dataLog[2];
                            var indexData = Convert.ToInt32(dataLog[3]);
                            var log_b = ErasoftDbContext.LOG_HARGAJUAL_B.Where(m => m.NO_BUKTI == nobuk && m.NO_FILE == indexData).FirstOrDefault();
                            if (log_b != null)
                            {
                                var currentProgress = log_b.KET.Split('/');
                                if(currentProgress.Length == 2)
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
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
            }
            return ret;
        }

        public async Task<string> UpdateImage(ShopeeAPIData iden, string brg, string brg_mp)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";
            SetupContext(iden);
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
                REQUEST_ID = seconds.ToString(),
                REQUEST_ACTION = "Update Product Image",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
                REQUEST_ATTRIBUTE_2 = brg,
                REQUEST_ATTRIBUTE_3 = brg_mp,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/item/img/update";

            List<string> imagess = new List<string>();

            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_1))
                imagess.Add(brgInDb.LINK_GAMBAR_1);
            //if (brgInDb.TYPE == "3")
            //{
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_2))
                imagess.Add(brgInDb.LINK_GAMBAR_2);
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_3))
                imagess.Add(brgInDb.LINK_GAMBAR_3);
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_4))
                imagess.Add(brgInDb.LINK_GAMBAR_4);
            if (!string.IsNullOrEmpty(brgInDb.LINK_GAMBAR_5))
                imagess.Add(brgInDb.LINK_GAMBAR_5);
            //}
            //remark 14 Nov 2019, gambar varian tidak masuk ke gambar induk
            //if (brgInDb.TYPE == "4")
            //{
            //    var brgVarian = ErasoftDbContext.STF02.Where(m => m.PART == brg).ToList();
            //    foreach (var brgVar in brgVarian)
            //    {
            //        if (!string.IsNullOrEmpty(brgVar.LINK_GAMBAR_1))
            //            imagess.Add(brgVar.LINK_GAMBAR_1);
            //        if (!string.IsNullOrEmpty(brgVar.LINK_GAMBAR_2))
            //            imagess.Add(brgVar.LINK_GAMBAR_2);
            //    }
            //}
            //end remark 14 Nov 2019, gambar varian tidak masuk ke gambar induk
            string[] brg_mp_split = brg_mp.Split(';');
            ShopeeUpdateImageData HttpBody = new ShopeeUpdateImageData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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

        public async Task<string> AddDiscount(ShopeeAPIData iden, int recNumPromosi)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";
            double promoPrice;
            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.merchant_code).FirstOrDefault();
            var varPromo = ErasoftDbContext.PROMOSI.Where(p => p.RecNum == recNumPromosi).FirstOrDefault();
            var varPromoDetail = ErasoftDbContext.DETAILPROMOSI.Where(p => p.RecNumPromosi == recNumPromosi).ToList();
            long starttime = ((DateTimeOffset)varPromo.TGL_MULAI).ToUnixTimeSeconds();
            long endtime = ((DateTimeOffset)varPromo.TGL_AKHIR).ToUnixTimeSeconds();

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = seconds.ToString(),
                REQUEST_ACTION = "Add Discount",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/discount/add";

            ShopeeAddDiscountData HttpBody = new ShopeeAddDiscountData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                discount_name = varPromo.NAMA_PROMOSI,
                start_time = starttime,
                end_time = endtime,
                items = new List<ShopeeAddDiscountDataItems>()
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
                                ShopeeAddDiscountDataItems item = new ShopeeAddDiscountDataItems()
                                {
                                    item_id = Convert.ToInt64(brg_mp[0]),
                                    item_promotion_price = (float)promoPrice,
                                    purchase_limit = (UInt32)(promoDetail.MAX_QTY == 0 ? 2 : promoDetail.MAX_QTY),
                                    variations = new List<ShopeeAddDiscountDataItemsVariation>()
                                };
                                //item.variations.Add(new ShopeeAddDiscountDataItemsVariation()
                                //{
                                //    variation_id = 0,
                                //    variation_promotion_price = (float)0.1
                                //});
                                HttpBody.items.Add(item);
                            }
                            else /*if (brg_mp[1] == "0")*/
                            {
                                ShopeeAddDiscountDataItems item = new ShopeeAddDiscountDataItems()
                                {
                                    item_id = Convert.ToInt64(brg_mp[0]),
                                    item_promotion_price = (float)promoPrice,
                                    //purchase_limit = 10,
                                    purchase_limit = (UInt32)(promoDetail.MAX_QTY == 0 ? 2 : promoDetail.MAX_QTY),
                                    variations = new List<ShopeeAddDiscountDataItemsVariation>()
                                };
                                item.variations.Add(new ShopeeAddDiscountDataItemsVariation()
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
                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeCreatePromoRes)) as ShopeeCreatePromoRes;
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

        public async Task<string> AddDiscountItem(ShopeeAPIData iden, long discount_id, DetailPromosi detilPromosi)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";
            double promoPrice;
            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.merchant_code).FirstOrDefault();

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = seconds.ToString(),
                REQUEST_ACTION = "Add Discount Item",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/discount/items/add";

            ShopeeAddDiscountItemData HttpBody = new ShopeeAddDiscountItemData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                discount_id = discount_id,
                items = new List<ShopeeAddDiscountDataItems>()
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
                        ShopeeAddDiscountDataItems item = new ShopeeAddDiscountDataItems()
                        {
                            item_id = Convert.ToInt64(brg_mp[0]),
                            //item_promotion_price = (float)detilPromosi.HARGA_PROMOSI,
                            item_promotion_price = (float)promoPrice,
                            //purchase_limit = 10,
                            purchase_limit = (UInt32)(detilPromosi.MAX_QTY == 0 ? 2 : detilPromosi.MAX_QTY),
                            variations = new List<ShopeeAddDiscountDataItemsVariation>()
                        };
                        //item.variations.Add(new ShopeeAddDiscountDataItemsVariation()
                        //{
                        //    variation_id = 0,
                        //    variation_promotion_price = 0
                        //});
                        HttpBody.items.Add(item);
                    }
                    else/* if (brg_mp[1] == "0")*/
                    {
                        ShopeeAddDiscountDataItems item = new ShopeeAddDiscountDataItems()
                        {
                            item_id = Convert.ToInt64(brg_mp[0]),
                            //item_promotion_price = (float)detilPromosi.HARGA_PROMOSI,
                            item_promotion_price = (float)promoPrice,
                            //purchase_limit = 10,
                            purchase_limit = (UInt32)(detilPromosi.MAX_QTY == 0 ? 2 : detilPromosi.MAX_QTY),
                            variations = new List<ShopeeAddDiscountDataItemsVariation>()
                        };
                        item.variations.Add(new ShopeeAddDiscountDataItemsVariation()
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
                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeCreatePromoRes)) as ShopeeCreatePromoRes;
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
        public async Task<string> DeleteDiscount(ShopeeAPIData iden, long discount_id)
        {
            //Use this api to delete one discount activity BEFORE it starts.
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.merchant_code).FirstOrDefault();

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = seconds.ToString(),
                REQUEST_ACTION = "Delete Discount",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/discount/delete";

            ShopeeDeleteDiscountData HttpBody = new ShopeeDeleteDiscountData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeDeleteDiscountData)) as ShopeeDeleteDiscountData;
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
        public async Task<string> DeleteDiscountItem(ShopeeAPIData iden, long discount_id, DetailPromosi detilPromosi)
        {
            //Use this api to delete items of the discount activity
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            var arf01 = ErasoftDbContext.ARF01.Where(p => p.Sort1_Cust == iden.merchant_code).FirstOrDefault();

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = seconds.ToString(),
                REQUEST_ACTION = "Delete Discount Item",
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
                REQUEST_STATUS = "Pending",
            };

            string urll = "https://partner.shopeemobile.com/api/v1/discount/item/delete";

            ShopeeDeleteDiscountItemData HttpBody = new ShopeeDeleteDiscountItemData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
                    var resServer = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeDeleteDiscountItemData)) as ShopeeDeleteDiscountItemData;
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

        public async Task<ShopeeGetAddressResult> GetAddress(ShopeeAPIData iden)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            var ret = new ShopeeGetAddressResult();

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/address/get";

            ShopeeGetAddressData HttpBody = new ShopeeGetAddressData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
                    ret = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetAddressResult)) as ShopeeGetAddressResult;

                }
                catch (Exception ex2)
                {
                }
            }
            return ret;
        }
        public async Task<ShopeeGetTimeSlotResult> GetTimeSlot(ShopeeAPIData iden, long address_id, string ordersn)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            var ret = new ShopeeGetTimeSlotResult();

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/timeslot/get";

            ShopeeGetTimeSlotData HttpBody = new ShopeeGetTimeSlotData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
                    ret = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetTimeSlotResult)) as ShopeeGetTimeSlotResult;
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
                        ShopeeGetTimeSlotResultPickup_Time errItem = new ShopeeGetTimeSlotResultPickup_Time()
                        {
                            pickup_time_id = "-1",
                            date_string = "Order sudah Expired."
                        };
                        ret.pickup_time = new ShopeeGetTimeSlotResultPickup_Time[1];
                        ret.pickup_time[0] = (errItem);
                    }
                }
                catch (Exception ex2)
                {
                }
            }
            return ret;
        }
        public async Task<string> GetBranch(ShopeeAPIData iden, string ordersn)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/logistics/branch/get";

            ShopeeGetBranchData HttpBody = new ShopeeGetBranchData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
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
                    //var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetItemListResult)) as ShopeeGetItemListResult;

                }
                catch (Exception ex2)
                {
                }
            }
            return ret;
        }

        public async Task<string> CekBrutoOrderCompleted(ShopeeAPIData iden, StatusOrder stat, string CUST, string NAMA_CUST, int page, int jmlhNewOrder, int jmlhPesananDibayar)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";
            //string connID = Guid.NewGuid().ToString();
            string connID = "FixBruto";
            SetupContext(iden);

            long seconds = CurrentTimeSecond();
            long timestampFrom = (long)DateTimeOffset.UtcNow.AddDays(-23).ToUnixTimeSeconds();
            long timestampTo = (long)DateTimeOffset.UtcNow.AddDays(-21).ToUnixTimeSeconds();

            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/orders/get";

            ShopeeGetOrderByStatusData HttpBody = new ShopeeGetOrderByStatusData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                pagination_offset = page,
                pagination_entries_per_page = 50,
                create_time_from = timestampFrom,
                create_time_to = timestampTo,
                order_status = stat.ToString()
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

            if (responseFromServer != null)
            {
                //try
                //{
                var listOrder = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetOrderByStatusResult)) as ShopeeGetOrderByStatusResult;
                if (stat == StatusOrder.COMPLETED)
                {
                    string[] ordersn_list = listOrder.orders.Select(p => p.ordersn).ToArray();
                    ////add by calvin 4 maret 2019, filter
                    //var dariTgl = DateTimeOffset.UtcNow.AddDays(-30).DateTime;
                    //var SudahAdaDiMO = ErasoftDbContext.SOT01A.Where(p => p.USER_NAME == "Auto Shopee" && p.CUST == CUST && p.TGL >= dariTgl).Select(p => p.NO_REFERENSI).ToList();
                    ////end add by calvin
                    //var filtered = ordersn_list.Where(p => !SudahAdaDiMO.Contains(p));
                    //if (filtered.Count() > 0)
                    //{
                    foreach (var item in ordersn_list)
                    {
                        if (item == "190917235708RJN")
                        {
                            await GetOrderDetails(iden, ordersn_list.Where(p => p == "190917235708RJN").ToArray(), connID, CUST, NAMA_CUST, stat);
                        }
                    }
                    //jmlhNewOrder = filtered.Count();
                    //}

                    ////add by calvin 29 mei 2019
                    //if (stat == StatusOrder.READY_TO_SHIP)
                    //{
                    //    string ordersn = "";
                    //    var filteredSudahAda = ordersn_list.Where(p => SudahAdaDiMO.Contains(p));
                    //    foreach (var item in filteredSudahAda)
                    //    {
                    //        ordersn = ordersn + "'" + item + "',";
                    //    }
                    //    ordersn = ordersn.Substring(0, ordersn.Length - 1);
                    //    var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI IN (" + ordersn + ") AND STATUS_TRANSAKSI = '0'");
                    //    if (rowAffected > 0)
                    //    {
                    //        jmlhPesananDibayar += rowAffected;
                    //    }
                    //}
                    ////end add by calvin 29 mei 2019

                    if (listOrder.more)
                    {
                        await CekBrutoOrderCompleted(iden, stat, CUST, NAMA_CUST, page + 50, jmlhNewOrder, jmlhPesananDibayar);
                    }
                    else
                    {
                        //if (jmlhNewOrder > 0)
                        //{
                        //    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        //    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari Shopee.");
                        //    new StokControllerJob().updateStockMarketPlace(connID, iden.DatabasePathErasoft, iden.username);
                        //}
                        //if (jmlhPesananDibayar > 0)
                        //{
                        //    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                        //    contextNotif.Clients.Group(iden.DatabasePathErasoft).moNewOrder("Terdapat " + Convert.ToString(jmlhPesananDibayar) + " Pesanan terbayar dari Shopee.");
                        //}
                    }
                }
                //}
                //catch (Exception ex2)
                //{
                //}
            }
            return ret;
        }

        public async Task<string> Template(ShopeeAPIData iden)
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
                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
                REQUEST_STATUS = "Pending",
            };

            //ganti
            string urll = "https://partner.shopeemobile.com/api/v1/items/get";

            //ganti
            ShopeeGetItemListData HttpBody = new ShopeeGetItemListData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                pagination_offset = 0,
                pagination_entries_per_page = 100
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
                    //ganti
                    var listBrg = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetItemListResult)) as ShopeeGetItemListResult;
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
            Exception = 4,
            RePending = 5,
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
        protected void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, ShopeeAPIData iden, API_LOG_MARKETPLACE data)
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
        }
        public class ShopeeGetAttributeData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public string language { get; set; }
            public int category_id { get; set; }
        }
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

        public class GetEscrowDetailsData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
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

        public class ShopeeGetItemListResult
        {
            public ShopeeGetItemListItem[] items { get; set; }
            public string request_id { get; set; }
            public bool more { get; set; }
        }

        public class ShopeeGetItemListItem
        {
            public string status { get; set; }
            public int update_time { get; set; }
            public string item_sku { get; set; }
            public ShopeeGetItemListVariation[] variations { get; set; }
            public int shopid { get; set; }
            public long item_id { get; set; }
        }

        public class ShopeeGetItemListVariation
        {
            public string variation_sku { get; set; }
            public long variation_id { get; set; }
        }

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
            public int shopid { get; set; }
            public string currency { get; set; }
            public int create_time { get; set; }
            public int likes { get; set; }
            public string[] images { get; set; }
            public int days_to_ship { get; set; }
            public float package_length { get; set; }
            public int stock { get; set; }
            public string status { get; set; }
            public int update_time { get; set; }
            public string description { get; set; }
            public int views { get; set; }
            public float price { get; set; }
            public int sales { get; set; }
            public int discount_id { get; set; }
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
            public int category_id { get; set; }
        }

        public class ShopeeGetItemDetailLogistic
        {
            public string logistic_name { get; set; }
            public bool is_free { get; set; }
            public float estimated_shipping_fee { get; set; }
            public int logistic_id { get; set; }
            public bool enabled { get; set; }
        }

        public class ShopeeGetItemDetailVariation
        {
            public string status { get; set; }
            public float original_price { get; set; }
            public int update_time { get; set; }
            public int create_time { get; set; }
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
            public int attribute_id { get; set; }
            public string attribute_value { get; set; }
            public string attribute_type { get; set; }
        }


        public class ShopeeGetCategoryResult
        {
            public ShopeeGetCategoryCategory[] categories { get; set; }
            public string request_id { get; set; }
        }

        public class ShopeeGetCategoryCategory
        {
            public int parent_id { get; set; }
            public bool has_children { get; set; }
            public int category_id { get; set; }
            public string category_name { get; set; }
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
            public int attribute_id { get; set; }
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

        public class ShopeeGetTokenShop
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
        }

        public class ShopeeGetTokenShopResult
        {
            public string error { get; set; }
            public string msg { get; set; }
            public ShopeeGetTokenShopResultAtribute[] authed_shops { get; set; }
            public string request_id { get; set; }
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
            public int update_time { get; set; }
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
            public int create_time { get; set; }
            public int? pay_time { get; set; }
            public ShopeeGetOrderDetailsResultRecipient_Address recipient_address { get; set; }
            public int days_to_ship { get; set; }
            public string tracking_no { get; set; }
            public string order_status { get; set; }
            public int note_update_time { get; set; }
            public int update_time { get; set; }
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
            public string cancel_reason { get; set; }//add by Tri 9 Des 2019
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
            public long promotion_id { get; set; }
            public int variation_quantity_purchased { get; set; }
            public string variation_sku { get; set; }
            public string variation_original_price { get; set; }
            public string promotion_type { get; set; }
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
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
            public object dropoff { get; set; }
        }
        public class ShopeeInitLogisticPickupData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
            public object pickup { get; set; }
        }
        public class ShopeeInitLogisticNonIntegratedData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
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
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public int stock { get; set; }
        }

        public class ShopeeUpdateVariationStockData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
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
            public int partner_id { get; set; }
            public int shopid { get; set; }
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
            public int logistic_id { get; set; }
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
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
            public string cancel_reason { get; set; }
            public long item_id { get; set; }
            public long variation_id { get; set; }
        }
        public class ShopeeCancelOrderResult
        {
            public int modified_time { get; set; }
            public string request_id { get; set; }
            public string msg { get; set; }
            public string error { get; set; }
        }
        public class ShopeeGetVariation
        {
            public long item_id { get; set; }
            public int shopid { get; set; }
            public int partner_id { get; set; }
            public long timestamp { get; set; }
        }
        public class ShopeeInitTierVariation
        {
            public long item_id { get; set; }
            public ShopeeTierVariation[] tier_variation { get; set; }
            public ShopeeVariation[] variation { get; set; }
            public int shopid { get; set; }
            public int partner_id { get; set; }
            public long timestamp { get; set; }

        }
        public class ShopeeUpdateTierVariationList
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


        public class ShopeeTierVariation
        {
            public string name { get; set; }
            public string[] options { get; set; }
            public string[] images_url { get; set; }
        }
        public class ShopeeVariation
        {
            public int[] tier_index { get; set; }
            public int stock { get; set; }
            public float price { get; set; }
            public string variation_sku { get; set; }
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

        public class ShopeeUnlistProductResponse
        {
            public ShopeeUnlistProductFailed[] failed { get; set; }
            public ShopeeUnlistProductSuccess[] success { get; set; }
            public string request_id { get; set; }
        }

        public class ShopeeUnlistProductFailed
        {
            public long item_id { get; set; }
            public string error_description { get; set; }
        }

        public class ShopeeUnlistProductSuccess
        {
            public long item_id { get; set; }
            public bool unlist { get; set; }
        }

        public class ShopeeCreateProdResponse : ShopeeError
        {
            public long item_id { get; set; }
            public Item item { get; set; }
        }

        public class ShopeeCreatePromoRes : ShopeeError
        {
            public int discount_id { get; set; }
            public int count { get; set; }
        }
        public class ShopeeDeletePromo : ShopeeError
        {
            public UInt64 discount_id { get; set; }
            public DateTime modify_time { get; set; }
        }
        public class Item
        {
            public List<ShopeeLogisticsClass> logistics { get; set; }
            public double original_price { get; set; }
            public double package_width { get; set; }
            public int cmt_count { get; set; }
            public double weight { get; set; }
            public int shopid { get; set; }
            public string currency { get; set; }
            public int create_time { get; set; }
            public int likes { get; set; }
            public List<string> images { get; set; }
            public int days_to_ship { get; set; }
            public double package_length { get; set; }
            public int stock { get; set; }
            public string status { get; set; }
            public int update_time { get; set; }
            public string description { get; set; }
            public int views { get; set; }
            public double price { get; set; }
            public int sales { get; set; }
            public int discount_id { get; set; }
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
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public string ordersn { get; set; }
        }
        public class ShopeeUpdatePriceData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public float price { get; set; }
        }
        public class ShopeeUpdateImageData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public string[] images { get; set; }
        }
        public class ShopeeUpdateVariantionPriceData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public long item_id { get; set; }
            public long variation_id { get; set; }
            public float price { get; set; }
        }
        public class ShopeeAddDiscountData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
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
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public long discount_id { get; set; }
            public List<ShopeeAddDiscountDataItems> items { get; set; }
        }
        public class ShopeeDeleteDiscountItemData : ShopeeError
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public long discount_id { get; set; }
            public long item_id { get; set; }
            public long variation_id { get; set; }
        }
        public class ShopeeDeleteDiscountData : ShopeeError
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public long discount_id { get; set; }
        }

        public class ShopeeGetAddressData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
        }
        public class ShopeeGetTimeSlotData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
            public long timestamp { get; set; }
            public long address_id { get; set; }
            public string ordersn { get; set; }
        }
        public class ShopeeGetBranchData
        {
            public int partner_id { get; set; }
            public int shopid { get; set; }
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
            public int address_id { get; set; }
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
            public int date { get; set; }
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
            public int update_time { get; set; }
            public int create_time { get; set; }
            public int discount_id { get; set; }
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
            public string msg { get; set; }
            public string error { get; set; }
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

        //add by nurul 28/2/2020
        public class getJOBShopee
        {
            public List<string> job_ordersn_list = new List<string>();
        }
        //end add by nurul 28/2/2020

        //add by nurul 28/2/2020
        //public class getJOBShopee
        //{
        //    public List<string> job_ordersn_list = new List<string>();
        //}
        //end add by nurul 28/2/2020

        public class GetEscrowDetailResult
        {
            public Order order { get; set; }
            public string request_id { get; set; }
        }

        public class GetShippingFeeResult
        {
            public string error { get; set; }
            public string msg { get; set; }
            public string request_id { get; set; }
            public string ordersn { get; set; }
            public string buyer_user_name { get; set; }
            public object[] returnsn_list { get; set; }
            public object[] refund_id_list { get; set; }
            public Order_Income order_income { get; set; }
        }

        public class Order_Income
        {
            public double escrow_amount { get; set; }
            public double buyer_total_amount { get; set; }
            public double original_price { get; set; }
            public double seller_discount { get; set; }
            public double shopee_discount { get; set; }
            public double voucher_from_seller { get; set; }
            public double voucher_from_shopee { get; set; }
            public double coins { get; set; }
            public double buyer_paid_shipping_fee { get; set; }
            public double buyer_transaction_fee { get; set; }
            public double cross_border_tax { get; set; }
            public double payment_promotion { get; set; }
            public double commission_fee { get; set; }
            public double service_fee { get; set; }
            public double seller_transaction_fee { get; set; }
            public double seller_lost_compensation { get; set; }
            public double seller_coin_cash_back { get; set; }
            public double escrow_tax { get; set; }
            public double final_shipping_fee { get; set; }
            public double actual_shipping_fee { get; set; }
            public double shopee_shipping_rebate { get; set; }
            public double shipping_fee_discount_from_3pl { get; set; }
            public double seller_shipping_discount { get; set; }
            public double estimated_shipping_fee { get; set; }
            //public object[] seller_voucher_code { get; set; }
            public double drc_adjustable_refund { get; set; }
        }

        public class Order
        {
            public Activity[] activity { get; set; }
            public string payee_id { get; set; }
            public string shipping_carrier { get; set; }
            public string exchange_rate { get; set; }
            public Bank_Account bank_account { get; set; }
            public Income_Details income_details { get; set; }
            public string country { get; set; }
            public string escrow_currency { get; set; }
            public string escrow_channel { get; set; }
            public Item1[] items { get; set; }
            public string ordersn { get; set; }
            public string fm_tn { get; set; }
        }

        public class Bank_Account
        {
            public string bank_name { get; set; }
            public string bank_account_number { get; set; }
            public string bank_account_country { get; set; }
        }

        public class Income_Details
        {
            public string seller_return_refund_amount { get; set; }
            public bool is_completed { get; set; }
            public string voucher_name { get; set; }
            public string voucher_type { get; set; }
            public string final_shipping_fee { get; set; }
            public string voucher { get; set; }
            public string seller_coin_cash_back { get; set; }
            public string coin { get; set; }
            public string seller_rebate { get; set; }
            public string cross_border_tax { get; set; }
            public string commission_fee { get; set; }
            public string buyer_shopee_kredit { get; set; }
            public string voucher_seller { get; set; }
            public string escrow_amount { get; set; }
            public string shipping_fee_rebate { get; set; }
            public string service_fee { get; set; }
            public string voucher_code { get; set; }
            public string local_currency { get; set; }
            public string credit_card_transaction_fee { get; set; }
            public string total_amount { get; set; }
            public string credit_card_promotion { get; set; }
            public string escrow_tax { get; set; }
            public string actual_shipping_cost { get; set; }
        }

        public class Activity
        {
            public string discounted_price { get; set; }
            public string original_price { get; set; }
            //public int activity_id { get; set; }
            public long activity_id { get; set; }
            public Item2[] items { get; set; }
            public string activity_type { get; set; }
        }

        public class Item2
        {
            public long item_id { get; set; }
            public string original_price { get; set; }
            public int quantity_purchased { get; set; }
            public long variation_id { get; set; }
        }

        public class Item1
        {
            public float original_price { get; set; }
            public int quantity_purchased { get; set; }
            public object deal_price { get; set; }
            public object credit_card_promotion { get; set; }
            public string item_name { get; set; }
            public object discount_from_coin { get; set; }
            public string item_sku { get; set; }
            public long variation_id { get; set; }
            public string variation_name { get; set; }
            public int add_on_deal_id { get; set; }
            public bool is_add_on_deal { get; set; }
            public long item_id { get; set; }
            public object discounted_price { get; set; }
            public int discount_from_voucher_seller { get; set; }
            public string variation_sku { get; set; }
            public int discount_from_voucher { get; set; }
            public bool is_main_item { get; set; }
            public object seller_rebate { get; set; }
        }

    }
}