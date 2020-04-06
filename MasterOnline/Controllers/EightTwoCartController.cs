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

        public ActionResult Index()
        {
            return View();
        }

        //Get All Products.
        public async Task<BindingBase> GetProductsList(E2CartAPIData iden, int IdMarket, int page, int recordCount, int totalData)
        {
            var ret = new BindingBase
            {
                status = 0,
                recordCount = recordCount,
                exception = 0,
                totalData = totalData//add 18 Juli 2019, show total record
            };

            string urll = string.Format("https://{0}/api/v1/getProduct?apiKey={1}&apiCredential={2}", url, iden.API_key, iden.API_credential);

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
                try {
                    var resultAPI = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(E2CartProductListResult)) as E2CartProductListResult;

                    ret.status = 1;
                    if (resultAPI != null)
                    {
                        if (resultAPI.data != null)
                        {

                            var listBrgMP = new List<string>();
                            foreach (var lis in resultAPI.data)
                            {
                                listBrgMP.Add(lis.id_product.ToString() + ";0");
                                if (lis.combinations.Length > 0)
                                {
                                    foreach (var listVar in lis.combinations)
                                    {
                                        listBrgMP.Add(lis.id_product.ToString() + ";" + listVar.id_product_attribute.ToString());
                                    }
                                }
                            }
                            var stf02h_local = (from a in ErasoftDbContext.STF02H where a.IDMARKET == IdMarket && listBrgMP.Contains(a.BRG_MP) select new stf02h_local { BRG = a.BRG, BRG_MP = a.BRG_MP, IDMARKET = a.IDMARKET }).ToList();
                            var tempBrg_local = (from a in ErasoftDbContext.TEMP_BRG_MP where a.IDMARKET == IdMarket && listBrgMP.Contains(a.BRG_MP) select new tempBrg_local { BRG_MP = a.BRG_MP, IDMARKET = a.IDMARKET }).ToList();
                            //if (listBrg.items.Length == 10)
                            //if (listBrg.product_listings)
                            //ret.message = (page + 1).ToString();
                            //ret.nextPage = 1;
                            if (listBrgMP.Count == (stf02h_local.Count + tempBrg_local.Count))
                            {
                                ret.totalData += listBrgMP.Count;
                                return ret;
                            }
                            ret.totalData += resultAPI.data.Count();//add 18 Juli 2019, show total record
                            foreach (var item in resultAPI.data)
                            {
                                //if (item.status.ToUpper() != "BANNED" && item.status.ToUpper() != "DELETED")
                                //{
                                //if (item.item_id == 1512392638 || item.item_id == 1790099887 || item.item_sku == "1660" || item.item_sku == "51")
                                //{

                                //}
                                //string kdBrg = string.IsNullOrEmpty(item.item_sku) ? item.item_id.ToString() : item.item_sku;
                                string kdBrg = item.id_product.ToString();
                                string brgMp = item.id_product.ToString() + ";0";
                                //change 13 Feb 2019, tuning
                                //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.ToUpper().Equals(brgMp.ToUpper()) && t.IDMARKET == IdMarket).FirstOrDefault();
                                //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.Equals(brgMp) && t.IDMARKET == IdMarket).FirstOrDefault();
                                //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.IDMARKET == IdMarket && (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
                                //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.IDMARKET == IdMarket && (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
                                var tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
                                var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brgMp.ToUpper()).FirstOrDefault();
                                //end change 13 Feb 2019, tuning

                                if ((tempbrginDB == null && brgInDB == null) || item.combinations.Length > 1)
                                {
                                    //var getDetailResult = await GetItemDetail(iden, item.item_id);
                                    //var getDetailResult = await GetItemDetail(iden, item.item_id, new List<tempBrg_local>(), new List<stf02h_local>(), IdMarket);
                                    
                                    //var getDetailResult = await GetItemDetail(iden, item.id_product, tempBrg_local, stf02h_local, IdMarket);
                                    //ret.totalData += getDetailResult.totalData;//add 18 Juli 2019, show total record
                                    //if (getDetailResult.exception == 1)
                                    //    ret.exception = 1;
                                    //if (getDetailResult.status == 1)
                                    //{
                                    //    ret.recordCount += getDetailResult.recordCount;
                                    //}
                                    //else
                                    //{
                                    //    currentLog.REQUEST_EXCEPTION = getDetailResult.message;
                                    //    manageAPI_LOG_MARKETPLACE(api_status.Exception, ErasoftDbContext, iden, currentLog);
                                    //}
                                }
                                //}
                            }
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        ret.nextPage = 0;
                    }
                }
                catch(Exception ex)
                {

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

        //Add or Edit Product.
        public ActionResult PostProduct(E2CartAPIData iden, string prodId)
        {

            string urll = string.Format("https://{0}/api/v1/addProduct", url);

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
        public ActionResult PostProductImage(E2CartAPIData iden)
        {

            string urll = string.Format("https://{0}/api/v1/addProductImage", url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(apiKey);
            postData += "&apiCredential=" + Uri.EscapeDataString(apiCred);
            postData += "&id_product=" + Uri.EscapeDataString("134");
            postData += "&image_url=" + Uri.EscapeDataString("https://www.jualalatmusik.com/image/data/guitars/rockwell-rg-212.jpg");

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

        //Add or Edit Product Attribute.
        public ActionResult PostProductAtt(E2CartAPIData iden)
        {

            string urll = string.Format("https://{0}/api/v1/addProductAttribute", url);

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

                ViewBag.response = resServer;
            }


            return View();
        }

        //Delete Product Image.
        public ActionResult DelProductImage(E2CartAPIData iden)
        {

            string urll = string.Format("https://{0}/api/v1/deleteProductImage", url);

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
        public ActionResult GetProductCategory(E2CartAPIData iden)
        {

            string urll = string.Format("https://{0}/api/v1/getCategory?apiKey={1}&apiCredential={2}", url, apiKey, apiCred);

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
                var lProdCat = Newtonsoft.Json.JsonConvert.DeserializeObject(responseServer, typeof(E2CartProductCategoryResult)) as E2CartProductCategoryResult;

                ViewBag.ProdCat = lProdCat.data;
            }

            return View();

        }

        //Add or Edit Product Category.
        public ActionResult PostProductCategory(E2CartAPIData iden, string idCat)
        {

            string urll = string.Format("https://{0}/api/v1/addCategory", url);

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

        //Get All Manufactures.
        public ActionResult GetManufacture(E2CartAPIData iden)
        {

            string urll = string.Format("https://{0}/api/v1/getManufacturer?apiKey={1}&apiCredential={2}", url, apiKey, apiCred);

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
        public ActionResult PostProdManufacture(E2CartAPIData iden, string idMan)
        {

            string urll = string.Format("https://{0}/api/v1/addManufacturer", url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(apiKey);
            postData += "&apiCredential=" + Uri.EscapeDataString(apiCred);
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
        public ActionResult GetInventory(E2CartAPIData iden)
        {

            string urll = string.Format("https://{0}/api/v1/getInventory?apiKey={1}&apiCredential={2}", url, apiKey, apiCred);

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

        //Update Inventory (Stock).
        public ActionResult PutInventory(E2CartAPIData iden, string idMan)
        {

            string urll = string.Format("https://{0}/api/v1/editInventory", url);

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);

            //Required parameters, other parameters can be add
            var postData = "apiKey=" + Uri.EscapeDataString(apiKey);
            postData += "&apiCredential=" + Uri.EscapeDataString(apiCred);
            postData += "&id_product=" + Uri.EscapeDataString("134");
            postData += "&id_product_attribute=" + Uri.EscapeDataString("6");
            postData += "&stock=" + Uri.EscapeDataString("22");

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

        //Get All Attributes.
        public ActionResult GetAttribute(E2CartAPIData iden)
        {

            string urll = string.Format("https://{0}/api/v1/getAttribute?apiKey={1}&apiCredential={2}", url, apiKey, apiCred);

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
        public ActionResult PostProdAttGroup(E2CartAPIData iden, string idAttGrp)
        {

            string urll = string.Format("https://{0}/api/v1/addAttributeGroup", url);

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
        public ActionResult PostProdAttribute(E2CartAPIData iden, string idAtt)
        {

            string urll = string.Format("https://{0}/api/v1/addAttribute", url);

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
        public async Task<string> GetCustomer(E2CartAPIData iden)
        {
            string ret = "";
            var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();

            string urll = string.Format("https://{0}/api/v1/getCustomer?apiKey={1}&apiCredential={2}", url, iden.API_key, iden.API_credential);

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
                if(resultApi != null)
                {
                    if (resultApi.error == "none" && resultApi.data.Length > 0)
                    {
                        foreach (var data in resultApi.data)
                        {
                            if (data.email == iden.email)
                            {
                                //var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '1', AL = '" + result.shop.address1 + "', Sort1_Cust = '" + result.shop.id + "', TLP = '" + result.shop.phone + "' WHERE CUST = '" + dataAPI.no_cust + "'");
                                var resultquery = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE ARF01 SET STATUS_API = '1' WHERE CUST = '" + iden.no_cust + "'");
                                if (resultquery == 1)
                                {
                                    contextNotif.Clients.Group(iden.DatabasePathErasoft).notifTransaction("Akun marketplace " + iden.email.ToString() + " (82Cart) berhasil aktif", true);
                                    //currentLog.REQUEST_RESULT = "Update Status API Complete";
                                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, dataAPI, currentLog);
                                }
                                else
                                {
                                    contextNotif.Clients.Group(iden.DatabasePathErasoft).notifTransaction("Akun marketplace (82Cart) gagal diaktifkan", true);
                                    //currentLog.REQUEST_RESULT = "Update Status API Failed";
                                    //currentLog.REQUEST_EXCEPTION = "Failed Update Table";
                                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, dataAPI, currentLog);
                                }
                            }
                        }
                    }
                    else
                    {
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
        public ActionResult GetOrders(E2CartAPIData iden)
        {

            string urll = string.Format("https://{0}/api/v1/getOrder?apiKey={1}&apiCredential={2}", url, apiKey, apiCred);


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
        public ActionResult GetOrdersState(E2CartAPIData iden)
        {

            string urll = string.Format("https://{0}/api/v1/getOrderStates?apiKey={1}&apiCredential={2}", url, apiKey, apiCred);


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
        
        public class E2CartAPIData
        {
            public string no_cust { get; set; }
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
            public string wholesale_price { get; set; }
            public string price { get; set; }
            public string on_sale { get; set; }
            public string meta_title { get; set; }
            public string meta_description { get; set; }
            public string link_rewrite { get; set; }
            public string id_category_default { get; set; }
            public string category_default { get; set; }
            public E2CartProductCategory[] category { get; set; }
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
            public ProductCombinations[] combinations { get; set; }
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
    }
}