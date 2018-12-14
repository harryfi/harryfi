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

namespace MasterOnline.Controllers
{
    public class ShopeeController : Controller
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
        public ShopeeController()
        {
            MoDbContext = new MoDbContext();
            var sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            if (sessionData?.Account != null)
            {
                if (sessionData.Account.UserId == "admin_manage")
                    ErasoftDbContext = new ErasoftContext();
                else
                    ErasoftDbContext = new ErasoftContext(sessionData.Account.DatabasePathErasoft);

                EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);
                username = sessionData.Account.Username;
            }
            else
            {
                if (sessionData?.User != null)
                {
                    var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
                    ErasoftDbContext = new ErasoftContext(accFromUser.DatabasePathErasoft);
                    EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
                    username = accFromUser.Username;
                }
            }
        }
        [Route("shp/code")]
        [HttpGet]
        public ActionResult ShopeeCode(string user, string shop_id)
        {
            var param = user.Split(new string[] { "_param_" }, StringSplitOptions.None);
            if (param.Count() == 2)
            {
                DatabaseSQL EDB = new DatabaseSQL(param[0]);
                var result = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET Sort1_Cust = '" + shop_id + "' WHERE CUST = '" + param[1] + "'");
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
                REQUEST_ID = seconds.ToString(),
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
                pagination_offset = page,
                pagination_entries_per_page = 10
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
                    ret.status = 1;
                    if (listBrg.items.Length == 10)
                        ret.message = (page + 1).ToString();
                    foreach (var item in listBrg.items)
                    {
                        string kdBrg = string.IsNullOrEmpty(item.item_sku) ? item.item_id.ToString() : item.item_sku;
                        var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.SELLER_SKU.ToUpper().Equals(kdBrg.ToUpper()) && t.IDMARKET == IdMarket).FirstOrDefault();
                        var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(item.item_id) && t.IDMARKET == IdMarket).FirstOrDefault();

                        if ((tempbrginDB == null && brgInDB == null) || item.variations.Length > 1)
                        {
                            var getDetailResult = await GetItemDetail(iden, item.item_id);
                            if (getDetailResult.status == 1)
                            {
                                ret.recordCount += getDetailResult.recordCount;
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
        public async Task<BindingBase> GetItemDetail(ShopeeAPIData iden, int item_id)
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

                    string IdMarket = ErasoftDbContext.ARF01.Where(c => c.Sort1_Cust.Equals(iden.merchant_code)).FirstOrDefault().RecNum.ToString();
                    string cust = ErasoftDbContext.ARF01.Where(c => c.Sort1_Cust.Equals(iden.merchant_code)).FirstOrDefault().CUST.ToString();
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
                        foreach (var item in detailBrg.item.variations)
                        {
                            sellerSku = item.variation_sku;
                            if (string.IsNullOrEmpty(sellerSku))
                            {
                                sellerSku = item.variation_id.ToString();
                            }

                            var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.SELLER_SKU.ToUpper().Equals(sellerSku.ToUpper()) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
                            var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(Convert.ToString(detailBrg.item.item_id) + ";" + Convert.ToString(item.variation_id)) && t.IDMARKET.ToString() == IdMarket).FirstOrDefault();
                            if (tempbrginDB == null && brgInDB == null)
                            {
                                ret.recordCount++;
                                proses_Item_detail(detailBrg, categoryCode, categoryName, cust, IdMarket, Convert.ToString(detailBrg.item.item_id) + ";" + Convert.ToString(item.variation_id), item.variation_sku, detailBrg.item.name + " " + item.name, item.status, item.price, sellerSku);
                            }
                        }
                    }
                    else
                    {
                        sellerSku = string.IsNullOrEmpty(detailBrg.item.item_sku) ? detailBrg.item.item_id.ToString() : detailBrg.item.item_sku;
                        ret.recordCount++;
                        proses_Item_detail(detailBrg, categoryCode, categoryName, cust, IdMarket, detailBrg.item.item_id.ToString(), detailBrg.item.item_sku, detailBrg.item.name, detailBrg.item.status, detailBrg.item.price, sellerSku);
                    }
                }
                catch (Exception ex2)
                {
                    ret.message = ex2.Message;
                }
            }
            return ret;
        }
        protected void proses_Item_detail(ShopeeGetItemDetailResult detailBrg, string categoryCode, string categoryName, string cust, string IdMarket, string barang_id, string barang_sku, string barang_name, string barang_status, float barang_price, string sellerSku)
        {
            string brand = "OEM";
            string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, NAMA3, BERAT, PANJANG, LEBAR, TINGGI, CUST, ";
            sSQL += "Deskripsi, IDMARKET, HJUAL, HJUAL_MP, DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3,";
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
            sSQL += Convert.ToDouble(detailBrg.item.weight) * 1000 + "," + detailBrg.item.package_length + "," + detailBrg.item.package_width + "," + detailBrg.item.package_height + ", '";
            sSQL += cust + "' , '" + detailBrg.item.description + "' , " + IdMarket + " , " + barang_price + " , " + barang_price;
            sSQL += " , " + (barang_status.Contains("NORMAL") ? "1" : "0") + " , '" + categoryCode + "' , '" + categoryName + "' , '" + "REPLACE_MEREK" + "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "'";

            var attributeShopee = MoDbContext.AttributeShopee.Where(a => a.CATEGORY_CODE.Equals(categoryCode)).FirstOrDefault();
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
                            attrVal += Convert.ToString(property.attribute_value);
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
            EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
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
                                    if (oCommand.ExecuteNonQuery() == 1)
                                    {
                                        if (item.has_children)
                                        {
                                            RecursiveInsertCategory(oCommand, result.categories, item.category_id, item.category_id);
                                        }
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
                oCommand.Parameters[0].Value = child.category_id;
                oCommand.Parameters[1].Value = child.category_name;
                oCommand.Parameters[2].Value = parent;
                oCommand.Parameters[3].Value = child.has_children ? "0" : "1";
                oCommand.Parameters[4].Value = master_category_code;

                if (oCommand.ExecuteNonQuery() == 1)
                {
                    if (child.has_children)
                    {
                        RecursiveInsertCategory(oCommand, categories, child.category_id, master_category_code);
                    }
                }
            }
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
                                var AttributeInDb = MoDbContext.AttributeShopee.ToList();
                                //cek jika belum ada di database, insert
                                var cari = AttributeInDb.Where(p => p.CATEGORY_CODE.ToUpper().Equals(category.CATEGORY_CODE));
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
        public async Task<string> GetOrderByStatus(ShopeeAPIData iden, StatusOrder stat, string connID, string CUST, string NAMA_CUST)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            long timestamp7Days = (long)DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();

            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            string urll = "https://partner.shopeemobile.com/api/v1/orders/get";

            ShopeeGetOrderByStatusData HttpBody = new ShopeeGetOrderByStatusData
            {
                partner_id = MOPartnerID,
                shopid = Convert.ToInt32(iden.merchant_code),
                timestamp = seconds,
                pagination_offset = 0,
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

                    var listOrder = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetOrderByStatusResult)) as ShopeeGetOrderByStatusResult;
                    if (stat == StatusOrder.READY_TO_SHIP)
                    {
                        string[] ordersn_list = listOrder.orders.Select(p => p.ordersn).ToArray();
                        await GetOrderDetails(iden, ordersn_list, connID, CUST, NAMA_CUST);
                    }

                }
                catch (Exception ex2)
                {
                }
            }
            return ret;
        }
        public async Task<string> GetOrderDetails(ShopeeAPIData iden, string[] ordersn_list, string connID, string CUST, string NAMA_CUST)
        {
            int MOPartnerID = 841371;
            string MOPartnerKey = "94cb9bc805355256df8b8eedb05c941cb7f5b266beb2b71300aac3966318d48c";
            string ret = "";

            long seconds = CurrentTimeSecond();
            DateTime milisBack = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime.AddHours(7);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = seconds.ToString(),
                REQUEST_ACTION = "Get Order List", //ganti
                REQUEST_DATETIME = milisBack,
                REQUEST_ATTRIBUTE_1 = iden.merchant_code,
                REQUEST_STATUS = "Pending",
            };

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
                    var result = JsonConvert.DeserializeObject(responseFromServer, typeof(ShopeeGetOrderDetailsResult)) as ShopeeGetOrderDetailsResult;
                    var connIdARF01C = Guid.NewGuid().ToString();
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, iden, currentLog);
                    List<TEMP_SHOPEE_ORDERS> batchinsert = new List<TEMP_SHOPEE_ORDERS>();
                    List<TEMP_SHOPEE_ORDERS_ITEM> batchinsertItem = new List<TEMP_SHOPEE_ORDERS_ITEM>();
                    string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                    insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                    insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV,CONNECTION_ID) VALUES ";
                    var kabKot = "3174";
                    var prov = "31";
                    foreach (var order in result.orders)
                    {
                        TEMP_SHOPEE_ORDERS newOrder = new TEMP_SHOPEE_ORDERS()
                        {
                            actual_shipping_cost = order.actual_shipping_cost,
                            buyer_username = order.buyer_username,
                            cod = order.cod,
                            country = order.country,
                            create_time = order.create_time,
                            currency = order.currency,
                            days_to_ship = order.days_to_ship,
                            dropshipper = order.dropshipper,
                            escrow_amount = order.escrow_amount,
                            estimated_shipping_fee = order.estimated_shipping_fee,
                            goods_to_declare = order.goods_to_declare,
                            message_to_seller = order.message_to_seller,
                            note = order.note,
                            note_update_time = order.note_update_time,
                            ordersn = order.ordersn,
                            order_status = order.order_status,
                            payment_method = order.payment_method,
                            pay_time = order.pay_time,
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
                            update_time = order.update_time
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
                                weight = item.weight
                            };
                            batchinsertItem.Add(newOrderItem);
                        }
                        insertPembeli += "('" + order.recipient_address.name + "','" + order.recipient_address.full_address + "','" + order.recipient_address.phone + "','" + NAMA_CUST.Replace(',', '.') + "',0,0,'0','01',";
                        insertPembeli += "1, 'IDR', '01', '" + order.recipient_address.full_address + "', 0, 0, 0, 0, '1', 0, 0, ";
                        insertPembeli += "'FP', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" + order.recipient_address.zipcode + "', '', '" + kabKot + "', '" + prov + "', '', '','" + connIdARF01C + "'),";

                        batchinsert.Add(newOrder);
                    }
                    ErasoftDbContext.TEMP_SHOPEE_ORDERS.AddRange(batchinsert);
                    ErasoftDbContext.TEMP_SHOPEE_ORDERS_ITEM.AddRange(batchinsertItem);
                    insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
                    EDB.ExecuteSQL("Constring", CommandType.Text, insertPembeli);
                    ErasoftDbContext.SaveChanges();
                    using (SqlCommand CommandSQL = new SqlCommand())
                    {
                        //call sp to insert buyer data
                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIdARF01C;

                        EDB.ExecuteSQL("Con", "MoveARF01CFromTempTable", CommandSQL);

                        //CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                        //CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connID;
                        //CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                        //CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        //CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                        //CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 0;
                        //CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                        //CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                        //CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                        //CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 1;

                        //EDB.ExecuteSQL("Con", "MoveOrderFromTempTable", CommandSQL);
                    };
                }
                catch (Exception ex2)
                {
                    currentLog.REQUEST_EXCEPTION = ex2.InnerException == null ? ex2.Message : ex2.InnerException.Message;
                    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                }
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

        [HttpGet]
        public string ShopeeUrl(string cust)
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
            }
        }
        protected void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, ShopeeAPIData iden, API_LOG_MARKETPLACE data)
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
        public static long CurrentTimeSecond()
        {
            //        return (long)DateTime.Now.ToUniversalTime().Subtract(
            //new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            //).TotalMilliseconds;
            return (long)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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
            public int item_id { get; set; }
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
            public int item_id { get; set; }
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
            public int item_id { get; set; }
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
            public int pay_time { get; set; }
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
            public int item_id { get; set; }
            public int variation_quantity_purchased { get; set; }
            public string variation_sku { get; set; }
            public string variation_original_price { get; set; }
        }

    }
}