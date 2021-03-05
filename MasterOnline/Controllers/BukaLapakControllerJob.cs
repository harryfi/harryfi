using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MasterOnline.Models;
using System.Text;
using System.Data;
using MasterOnline.ViewModels;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Erasoft.Function;
using System.Data.SqlClient;
using Hangfire;
using RestSharp;
using System.Threading.Tasks;

namespace MasterOnline.Controllers
{
    public class BukaLapakControllerJob : Controller
    {
        // GET: BukaLapak
        //AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        private DatabaseSQL EDB;
        private MoDbContext MoDbContext;
        private ErasoftContext ErasoftDbContext;
        private string username;

#if AWS
        private static string callBackUrl = "https://masteronline.co.id/bukalapak/auth";
        private static string client_id = "GovVusRdl0QwJCXu1F0th5lezoFYvVIW4XHv4U1M05U";
        private static string client_secret = "osqzx8n3y3YRJ0vydm_8qOZ9N9f95EvrZSvTFtKQCzM";
#else
        private static string callBackUrl = "https://dev.masteronline.co.id/bukalapak/auth";
        private static string client_id = "laJXb5jh91BelPQg2VmE2ooa58UVJmlJkNq98EPJc6s";
        private static string client_secret = "AXe5u7JcYiSNLvOsGW92Dzc4li6mbrWpN9qjlLD4OxI";
#endif
        public BukaLapakControllerJob()
        {
            //MoDbContext = new MoDbContext();
            //if (sessionData?.Account != null)
            //{
            //    if (sessionData.Account.UserId == "admin_manage")
            //        ErasoftDbContext = new ErasoftContext();
            //    else
            //        ErasoftDbContext = new ErasoftContext(sessionData.Account.DatabasePathErasoft);
            //    EDB = new DatabaseSQL(sessionData.Account.DatabasePathErasoft);

            //}
            //else
            //{
            //    if (sessionData?.User != null)
            //    {
            //        var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
            //        ErasoftDbContext = new ErasoftContext(accFromUser.DatabasePathErasoft);
            //        EDB = new DatabaseSQL(accFromUser.DatabasePathErasoft);
            //    }
            //}
        }
        protected void SetupContext(string DatabasePathErasoft, string uname)
        {
            //string ret = "";
            MoDbContext = new MoDbContext("");
            EDB = new DatabaseSQL(DatabasePathErasoft);
            string EraServerName = EDB.GetServerName("sConn");
            ErasoftDbContext = new ErasoftContext(EraServerName, DatabasePathErasoft);
            username = uname;
            //var arf01inDB = ErasoftDbContext.ARF01.Where(p => p.RecNum == idmarket).SingleOrDefault();
            //if (arf01inDB != null)
            //{
            //    ret = arf01inDB.TOKEN;
            //}
            //return ret;
        }
        public BukaLapakKey RefreshToken(BukaLapakKey data)
        {
            SetupContext(data.dbPathEra, "");
            var ret = data;
            if (data.tgl_expired < DateTime.UtcNow.AddHours(7).AddMinutes(30))
            {
                var cekInDB = ErasoftDbContext.ARF01.Where(m => m.CUST == data.cust).FirstOrDefault();
                if(cekInDB != null)
                {
                    if(data.token != cekInDB.TOKEN)
                    {
                        data.code = cekInDB.API_KEY;
                        data.refresh_token = cekInDB.REFRESH_TOKEN;
                        data.tgl_expired = cekInDB.TGL_EXPIRED.Value;
                        data.token = cekInDB.TOKEN;

                        if (cekInDB.TGL_EXPIRED.Value.AddMinutes(-30) > DateTime.UtcNow.AddHours(7))
                        {
                            return data;
                        }
                    }
                }
                var urll = ("https://accounts.bukalapak.com/oauth/token");
                var client = new RestClient(urll);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AlwaysMultipartFormData = true;
                request.AddParameter("grant_type", "refresh_token");
                request.AddParameter("client_id", client_id);
                request.AddParameter("client_secret", client_secret);
                request.AddParameter("refresh_token", data.refresh_token);
                string stringRet = "";
                try
                {
                    IRestResponse response = client.Execute(request);
                    stringRet = response.Content;
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
                    //ret = "error : " + err;
                }

                if (!string.IsNullOrEmpty(stringRet))
                {
                    AccessKeyBL retObj = JsonConvert.DeserializeObject(stringRet, typeof(AccessKeyBL)) as AccessKeyBL;
                    if (retObj != null)
                    {
                        DateTime tglExpired = DateTimeOffset.FromUnixTimeSeconds(retObj.created_at).UtcDateTime.AddHours(7).AddSeconds(retObj.expires_in);
                        var a = EDB.ExecuteSQL("ARConnectionString", CommandType.Text, "UPDATE ARF01 SET REFRESH_TOKEN='" + retObj.refresh_token + "', TGL_EXPIRED='" + tglExpired.ToString("yyyy-MM-dd HH:mm:ss") + "', TOKEN='" + retObj.access_token + "', STATUS_API = '1' WHERE CUST ='" + data.cust + "'");
                        ret.token = retObj.access_token;
                        ret.tgl_expired = tglExpired;
                        ret.refresh_token = retObj.refresh_token;
                    }
                    else
                    {
                    }
                }

            }
            return ret;
        }
        [HttpPost]
        public BindingBase GetAccessKey(string cust, string email, string password)
        {
            var ret = new BindingBase();
            ret.status = 0;

            var urll = ("https://api.bukalapak.com/v2/authenticate.json");

            var myReq = HttpWebRequest.Create(urll);

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Get API Key",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = cust,
                REQUEST_ATTRIBUTE_2 = email,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, "", currentLog);

            myReq.Method = "POST";
            myReq.ContentType = "application/json";
            myReq.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(email + ":" + password)));
            var myData = "{\"version\":\"1.1\",\"method\":\"\",\"params\":{\"account\":{\"type\":\"\",\"value\":\"0\"}}}";
            myReq.GetRequestStream().Write(Encoding.UTF8.GetBytes(myData), 0, Encoding.UTF8.GetBytes(myData).Count());
            var myResp = myReq.GetResponse();
            var myreader = new System.IO.StreamReader(myResp.GetResponseStream());
            //Dim myText As String;
            var stringRet = myreader.ReadToEnd();
            if (!string.IsNullOrEmpty(stringRet))
            {
                AccessKeyBL retObj = JsonConvert.DeserializeObject(stringRet, typeof(AccessKeyBL)) as AccessKeyBL;
                if (retObj != null)
                {
                    if (retObj.status.Equals("OK"))
                    {
                        //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                        //string username = sessionData?.Account != null ? sessionData.Account.Username : sessionData.User.Username;

                        //var a = EDB.ExecuteSQL("ARConnectionString", CommandType.Text, "UPDATE ARF01 SET API_KEY='" + retObj.user_id + "', TOKEN='" + retObj.token + "', STATUS_API = '1' WHERE CUST ='" + cust + "'");
                        ////var a = EDB.GetDataSet("ARConnectionString", "ARF01", "SELECT * FROM ARF01");
                        //if (a == 1)
                        //{
                        //    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, "", currentLog);
                        //    ret.status = 1;
                        //}
                        //else
                        //{
                        //    currentLog.REQUEST_EXCEPTION = "failed to update api_key;execute result=" + a;
                        //    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, "", currentLog);
                        //}
                    }
                    else
                    {
                        var a = EDB.ExecuteSQL("ARConnectionString", CommandType.Text, "UPDATE ARF01 SET STATUS_API = '0' WHERE CUST ='" + cust + "'");

                        ret.message = retObj.message;
                        currentLog.REQUEST_EXCEPTION = ret.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, "", currentLog);
                    }
                }
                else
                {
                    ret.message = "Failed to call Buka Lapak API";
                    currentLog.REQUEST_EXCEPTION = ret.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, "", currentLog);
                }
            }

            return ret;
        }
        [HttpPost]
        public BindingBase CreateProduct(BrgViewModel data)
        {
            var ret = new BindingBase();
            ret.status = 0;
            //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);

            var dataProduct = new BindingBukaLapakProduct
            {
                images = data.imageId,
                product = new ProductBukaLpk
                {
                    category_id = "564",
                    name = data.nama,
                    @new = "true",
                    price = data.harga,
                    negotiable = "false",
                    weight = data.weight,//weight in gram
                    stock = data.qty,
                    description_bb = data.deskripsi,
                    product_detail_attributes = new Product_Detail_Attributes
                    {
                        bahan = "baku",
                        merek = data.merk,
                        tipe = "formal",
                        //type = "",
                        ukuran = "S",
                    }
                }
            };
            if (!string.IsNullOrEmpty(data.nama2))
            {
                dataProduct.product.name += " " + data.nama2;
            }
            if (!string.IsNullOrEmpty(data.imageId2))
            {
                if (!string.IsNullOrEmpty(dataProduct.images))
                    dataProduct.images += ",";
                dataProduct.images += data.imageId2;
            }
            if (!string.IsNullOrEmpty(data.imageId3))
            {
                if (!string.IsNullOrEmpty(dataProduct.images))
                    dataProduct.images += ",";
                dataProduct.images += "," + data.imageId3;
            }
            //string hargaMarket = EDB.GetFieldValue("", "STF02H", "BRG = '" + data.kdBrg + "' AND AKUNMARKET = '" + data.akunMarket + "'", "HJUAL").ToString();
            //if (!string.IsNullOrEmpty(hargaMarket))
            //    dataProduct.product.price = hargaMarket;
            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Create Product",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = data.kdBrg,
                REQUEST_ATTRIBUTE_2 = data.token,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, data.key, currentLog);

            string dataPost = JsonConvert.SerializeObject(dataProduct);
            Utils.HttpRequest req = new Utils.HttpRequest();
            var bindResponse = req.CallBukaLapakAPI("POST", "products.json", dataPost, data.key, data.token, typeof(CreateProductBukaLapak)) as CreateProductBukaLapak;
            if (bindResponse != null)
            {
                if (bindResponse.status.Equals("OK"))
                {
                    ret.status = 1;
                    ret.message = bindResponse.product_detail.id;
                    var a = EDB.ExecuteSQL("", CommandType.Text, "UPDATE STF02H SET BRG_MP = '" + bindResponse.product_detail.id + "' WHERE BRG = '" + data.kdBrg + "' AND IDMARKET = '" + data.idMarket + "'");
                    if (a == 1)
                    {
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, data.key, currentLog);
                    }
                    else
                    {
                        currentLog.REQUEST_EXCEPTION = "failed to update brg_mp;execute result=" + a;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data.key, currentLog);
                    }
                }
                else
                {
                    ret.message = bindResponse.message;
                    currentLog.REQUEST_EXCEPTION = ret.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data.key, currentLog);
                }
            }
            else
            {
                ret.message = "Failed to call Buka Lapak API";
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, data.key, currentLog);
            }
            return ret;
        }

        [HttpGet]
        public BindingBase uploadGambar(string imagePath, string userId, string token)
        {
            //ukuran minimum gambar adalah 300x300
            BindingBase ret = new BindingBase();
            ret.status = 0;

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Upload Image Product",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = token,
                REQUEST_ATTRIBUTE_2 = imagePath,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, userId, currentLog);

            string URL = "https://api.bukalapak.com/v2/images.json";
            string post_data = imagePath;//"D:\\kaos.jpg"; //alamat file

            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

            WebRequest myReq = WebRequest.Create(URL);
            myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(userId + ":" + token))));

            myReq.Method = "POST";
            myReq.ContentType = "multipart/form-data; boundary=" + boundary;

            Stream postDataStream = GetPostStream(post_data, boundary);

            myReq.ContentLength = postDataStream.Length;
            Stream reqStream = myReq.GetRequestStream();

            postDataStream.Position = 0;

            byte[] buffer = new byte[1024];
            int bytesRead = 0;

            while ((bytesRead = postDataStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                reqStream.Write(buffer, 0, bytesRead);
            }

            postDataStream.Close();
            reqStream.Close();
            try
            {
                StreamReader sr = new StreamReader(myReq.GetResponse().GetResponseStream());
                var stringRes = JsonConvert.DeserializeObject(sr.ReadToEnd(), typeof(BukaLapakRes)) as BukaLapakRes;
                if (stringRes != null)
                {
                    if (stringRes.status.Equals("OK"))
                    {
                        ret.status = 1;
                        ret.message = stringRes.id;
                        manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);
                    }
                    else
                    {
                        ret.message = stringRes.message;
                        currentLog.REQUEST_EXCEPTION = ret.message;
                        manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                    }
                }
                else
                {
                    ret.message = "Failed to call Buka Lapak API";
                    currentLog.REQUEST_EXCEPTION = ret.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                }
            }
            catch (Exception ex)
            {
                ret.message = ex.ToString();
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                return ret;
            }

            return ret;
        }

        private static Stream GetPostStream(string filePath, string boundary)
        {
            Stream postDataStream = new System.IO.MemoryStream();
            System.Uri fileUrl = new Uri(filePath);

            //change by calvin 16 nov 2018
            //FileInfo fileInfo = new FileInfo(filePath);
            String filename = fileUrl.PathAndQuery.Replace('/', Path.DirectorySeparatorChar);
            FileInfo fileInfo = new FileInfo(filename);
            //end change by calvin 16 nov 2018

            string fileHeaderTemplate = Environment.NewLine + "--" + boundary + Environment.NewLine +
            "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" +
            Environment.NewLine + "Content-Type: multipart/form-data" + Environment.NewLine + Environment.NewLine;

            byte[] fileHeaderBytes = System.Text.Encoding.UTF8.GetBytes(string.Format(fileHeaderTemplate,
            "file", fileInfo.FullName));

            postDataStream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);

            //change by calvin 16 nov 2018
            //FileStream fileStream = fileInfo.OpenRead();
            var req = System.Net.WebRequest.Create(filePath);
            Stream stream = req.GetResponse().GetResponseStream();
            //end change by calvin 16 nov 2018

            byte[] buffer = new byte[1024];

            int bytesRead = 0;

            //while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                postDataStream.Write(buffer, 0, bytesRead);
            }

            //fileStream.Close();
            stream.Close();

            byte[] endBoundaryBytes = System.Text.Encoding.UTF8.GetBytes("--" + boundary + "--");
            postDataStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);

            return postDataStream;
        }

        [HttpGet]
        public CreateProductBukaLapak updateProduk(string brg, string brgMp, string price, string stock, string userId, string token)
        {

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Update Price/Stock Product",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = brg,
                REQUEST_ATTRIBUTE_2 = price,
                REQUEST_ATTRIBUTE_3 = stock,
                REQUEST_ATTRIBUTE_4 = token,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, userId, currentLog);

            var ret = new CreateProductBukaLapak();
            string Myprod = "{\"product\": {";
            if (!string.IsNullOrEmpty(price))
            {
                Myprod += "\"price\":\"" + price + "\"";
            }
            if (!string.IsNullOrEmpty(price) && !string.IsNullOrEmpty(stock))
                Myprod += ",";
            if (!string.IsNullOrEmpty(stock))
            {
                Myprod += "\"stock\":\"" + stock + "\"";
            }
            Myprod += "}}";
            Utils.HttpRequest req = new Utils.HttpRequest();
            ret = req.CallBukaLapakAPI("PUT", "products/" + brgMp + ".json", Myprod, userId, token, typeof(CreateProductBukaLapak)) as CreateProductBukaLapak;
            if (ret != null)
            {
                if (ret.status.ToString().Equals("OK"))
                {
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);

                    //add by calvin 8 nov 2018
                    if (!string.IsNullOrEmpty(stock))
                    {
                        //jika stok di bukalapak 0, di bukalapak akan menjadi non display, MO disamakan
                        if (Convert.ToDouble(stock) == 0)
                        {
                            var arf01Bukalapak = ErasoftDbContext.ARF01.Where(p => p.NAMA == "8").ToList();
                            foreach (var akun in arf01Bukalapak)
                            {
                                string sSQL = "UPDATE STF02H SET DISPLAY = '0' WHERE IDMARKET = '" + Convert.ToString(akun.RecNum) + "' AND BRG = '" + brg + "'";
                                var a = EDB.ExecuteSQL(sSQL, CommandType.Text, sSQL);
                                if (a <= 0)
                                {

                                }
                            }
                        }
                    }
                    //end add by calvin 8 nov 2018
                }
                else
                {
                    ret.message = ret.message;
                    currentLog.REQUEST_EXCEPTION = ret.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                }
            }
            else
            {
                ret = new CreateProductBukaLapak();
                ret.message = "Failed to call Buka Lapak API";
                currentLog.REQUEST_EXCEPTION = "Failed to call Buka Lapak API";
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
            }
            return ret;
        }

        [HttpGet]
        public BindingBase updateProdukStat(string kdBrg, string id, string akunMarket, bool stat, string userId, string token)
        {
            var ret = new BindingBase { status = 0 };
            Utils.HttpRequest req = new Utils.HttpRequest();
            BindingProductBL response = req.CallBukaLapakAPI("", "products/" + id + ".json", "", userId, token, typeof(BindingProductBL)) as BindingProductBL;
            if (response != null)
            {
                if (response.status.ToString().Equals("OK"))
                {
                    if (stat != Convert.ToBoolean(response.product.active))
                    {
                        if (stat)
                        {
                            var res = prodAktif(kdBrg, id, userId, token);
                            ret.status = res.status;
                            ret.message = res.message;

                        }
                        else
                        {
                            var res = prodNonAktif(kdBrg, id, userId, token);
                            ret.status = res.status;
                            ret.message = res.message;
                        }
                    }
                }
                else//no product on bukalapak > create new
                {
                    //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                    var dsSTF02 = EDB.GetDataSet("MOConnectionString", "STF02", "SELECT * FROM STF02 WHERE BRG = '" + kdBrg + "'");
                    if (dsSTF02.Tables[0].Rows.Count > 0)
                    {
                        dsSTF02 = EDB.GetDataSet("MOConnectionString", "STF02H", "SELECT * FROM STF02H WHERE BRG = '" + kdBrg + "' AND AKUN_MARKET = '" + akunMarket + "'");

                        BrgViewModel dataBrg = new BrgViewModel
                        {
                            nama = dsSTF02.Tables["STF02"].Rows[0]["NAMA"].ToString(),
                            weight = dsSTF02.Tables["STF02"].Rows[0]["NAMA"].ToString(),
                            qty = "1",
                            deskripsi = dsSTF02.Tables["STF02"].Rows[0]["DESKRIPSI"].ToString(),
                            merk = dsSTF02.Tables["STF02"].Rows[0]["KET_SORT2"].ToString(),

                        };
                        if (dsSTF02.Tables["STF02H"].Rows.Count > 0)
                        {
                            dataBrg.harga = dsSTF02.Tables["STF02H"].Rows[0]["HJUAL"].ToString();
                        }
                        else
                        {
                            //tidak ada data barang dengan brg dan marketplace tsb, gunakan harga brg dr marketplace lain
                            dsSTF02 = EDB.GetDataSet("", "STF02H", "SELECT * FROM STF02H WHERE BRG = '" + kdBrg + "'");
                            if (dsSTF02.Tables["STF02H"].Rows.Count > 0)
                            {
                                dataBrg.harga = dsSTF02.Tables["STF02H"].Rows[0]["HJUAL"].ToString();
                            }
                        }
                        string path = "C:\\MasterOnline\\Content\\Uploaded\\";
                        string fileName = "FotoProduk-" + dsSTF02.Tables["STF02"].Rows[0]["USERNAME"].ToString() + "-" + kdBrg + "-foto-";
                        string[] files = System.IO.Directory.GetFiles(path, "*-" + kdBrg + "-foto-1.*", System.IO.SearchOption.TopDirectoryOnly);
                        if (files.Length > 0)
                        {
                            var a = uploadGambar(path + fileName + "1", userId, token);
                            dataBrg.imageId = a.message;
                        }
                        files = System.IO.Directory.GetFiles(path, "*-" + kdBrg + "-foto-2.*", System.IO.SearchOption.TopDirectoryOnly);
                        if (files.Length > 0)
                        {
                            var a = uploadGambar(path + fileName + "2", userId, token);
                            dataBrg.imageId2 = a.message;
                        }
                        files = System.IO.Directory.GetFiles(path, "*-" + kdBrg + "-foto-3.*", System.IO.SearchOption.TopDirectoryOnly);
                        if (files.Length > 0)
                        {
                            var a = uploadGambar(path + fileName + "3", userId, token);
                            dataBrg.imageId3 = a.message;
                        }
                        var res = CreateProduct(dataBrg);
                    }

                }
            }
            else
            {
                ret.message = "Failed to call Buka Lapak API";
            }

            return ret;
        }

        public BindingBase prodNonAktif(string kdBrg, string id, string userId, string token)
        {
            var ret = new BindingBase { status = 0 };
            Utils.HttpRequest req = new Utils.HttpRequest();

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Hide Product",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = kdBrg,
                REQUEST_ATTRIBUTE_2 = id,
                REQUEST_ATTRIBUTE_3 = token,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, userId, currentLog);

            ResProduct response = req.CallBukaLapakAPI("PUT", "products/" + id + "/sold.json", "", userId, token, typeof(ResProduct)) as ResProduct;
            if (response != null)
            {
                if (response.status.ToString().Equals("OK"))
                {
                    ret.status = 1;
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);
                }
                else
                {
                    ret.message = response.message;
                    currentLog.REQUEST_EXCEPTION = response.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                }
            }
            else
            {
                ret.message = "Failed to call Buka Lapak API";
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
            }
            return ret;
        }


        public BindingBase prodAktif(string brg, string id, string userId, string token)
        {
            var ret = new BindingBase { status = 0 };
            Utils.HttpRequest req = new Utils.HttpRequest();

            MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            {
                REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                REQUEST_ACTION = "Show Product",
                REQUEST_DATETIME = DateTime.Now,
                REQUEST_ATTRIBUTE_1 = brg,
                REQUEST_ATTRIBUTE_2 = id,
                REQUEST_ATTRIBUTE_3 = token,
                REQUEST_STATUS = "Pending",
            };
            manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, userId, currentLog);

            ResProduct response = req.CallBukaLapakAPI("PUT", "products/" + id + "/relist.json", "", userId, token, typeof(ResProduct)) as ResProduct;
            if (response != null)
            {
                if (response.status.ToString().Equals("OK"))
                {
                    ret.status = 1;
                    manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);
                    //call api to set product stock, buka lapak non active product = 0 stock

                    var qtyOnHand = 0d;
                    {
                        object[] spParams = {
                                                    new SqlParameter("@BRG", brg),
                                                    new SqlParameter("@GD","ALL"),
                                                    new SqlParameter("@Satuan", "2"),
                                                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                                                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}
                                                };

                        ErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                        qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                    }
                    updateProduk(brg, id, "", qtyOnHand > 0 ? qtyOnHand.ToString() : "1", userId, token);
                }
                else
                {
                    ret.message = response.message;
                    currentLog.REQUEST_EXCEPTION = response.message;
                    manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                }
            }
            else
            {
                ret.message = "Failed to call Buka Lapak API";
                currentLog.REQUEST_EXCEPTION = ret.message;
                manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
            }
            return ret;
        }

        [HttpGet]
        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrdersNew(BukaLapakKey data, string CUST, string NAMA_CUST, string username, int day)
        {
            string ret = "";
            SetupContext(data.dbPathEra, username);

            var delQry = "delete a from sot01a a left join sot01b b on a.no_bukti = b.no_bukti where isnull(b.no_bukti, '') = '' and tgl >= '";
            delQry += DateTime.UtcNow.AddHours(7).AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss") + "' and cust = '" + CUST + "'";

            //var resultDel = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, delQry);

            //add by nurul 20/1/2021, bundling 
            var AdaKomponen = false;
            List<string> tempConnId = new List<string>() { };
            //end add by nurul 20/1/2021, bundling 

            var dtNow = DateTime.UtcNow.AddHours(7);
            var loop = true;
            var page = 0;
            while (loop)
            {
                data = RefreshToken(data);
                var retOrder = await GetOrdersLoop(data, CUST, NAMA_CUST, username, page, dtNow.AddDays(day).ToString("yyyy-MM-ddTHH:mm:ss"), dtNow.ToString("yyyy-MM-ddTHH:mm:ss"), 0);
                
                if (retOrder.AdaKomponen)
                {
                    AdaKomponen = retOrder.AdaKomponen;
                }

                if (retOrder.status >= 50)
                {
                    page = page + 1;
                }
                else
                {
                    loop = false;
                }
            }
            if (AdaKomponen)
            {
                new StokControllerJob().getQtyBundling(data.dbPathEra, username);
            }

            var queryStatus = "\"\\\"" + CUST + "\\\"\",\"\\";    // "\"000001\"","\
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + queryStatus + "%' and invocationdata like '%bukalapak%' and invocationdata like '%GetOrdersNew%' and statename like '%Enque%' and invocationdata not like '%resi%' ");

            return ret;
        }

        public async Task<BindingBase> GetOrdersLoop(BukaLapakKey data, string CUST, string NAMA_CUST, string username, int page, string fromDt, string toDt, int retry)
        {
            var ret = new BindingBase();
            ret.status = 0;
            var conn_id = Guid.NewGuid().ToString();
            int jmlhNewOrder = 0;
            //data = RefreshToken(data);

            //add by nurul 19/1/2021, bundling 
            ret.ConnId = conn_id;
            //end add by nurul 19/1/2021, bundling

            string urll = "https://api.bukalapak.com/transactions?limit=50&offset=" + (page * 50) + "&context=sale"
                + "&start_time=" + Uri.EscapeDataString(fromDt) + "&end_time=" + Uri.EscapeDataString(toDt) 
                + "&states[]=pending&states[]=paid&states[]=accepted";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", "Bearer " + data.token);
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
            catch(WebException e)
            {
                string err = "";
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse resp = e.Response;
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        err = sr.ReadToEnd();
                    }
                    var response = e.Response as HttpWebResponse;
                    var status = (int)response.StatusCode;
                    if (status == 401)
                    {
                        if (retry == 0)
                        {
                            data = RefreshToken(data);
                            await GetOrdersLoop(data, CUST, NAMA_CUST, username, page, fromDt, toDt, 1);
                        }
                        else
                        {
                            throw new Exception(err);
                        }
                    }
                    else
                    {
                        throw new Exception(err);
                    }
                }
                else
                {
                    throw new Exception(e.Message);
                }
            }
            if (responseFromServer != "")
            {
                GetOrdersResponse retObj = JsonConvert.DeserializeObject(responseFromServer, typeof(GetOrdersResponse)) as GetOrdersResponse;
                if (retObj != null)
                {
                    if(retObj.meta.http_status == 200)
                    {
                        if(retObj.data != null)
                        {
                            if(retObj.data.Length > 0)
                            {
                                ret.status = retObj.data.Length;
                                var cekDate = DateTime.UtcNow.AddHours(7).AddDays(-4);
                                var orderInDB = ErasoftDbContext.SOT01A.Where(m => m.CUST == CUST && m.TGL >= cekDate).Select(m => m.NO_REFERENSI).ToList();
                                foreach(var order in retObj.data)
                                {
                                    if (orderInDB.Contains(order.transaction_id))
                                    {
                                        if(order.state != "pending")
                                        {
                                            EDB.ExecuteSQL("CString", CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI = '" + order.transaction_id + "' AND CUST = '" + CUST+"' AND STATUS_TRANSAKSI = '0'");
                                        }

                                        if (!order.delivery.consignee.phone.Contains("Terkunci"))
                                        {
                                            var ordID = order.transaction_id;
                                            var currentOrder = ErasoftDbContext.SOT01A.Where(m => m.CUST == CUST && m.NO_REFERENSI == ordID).FirstOrDefault();
                                            if(currentOrder != null)
                                            {
                                                if (string.IsNullOrEmpty(currentOrder.PEMESAN))
                                                {
                                                    string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                                    insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                                    insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";
                                                    
                                                    var kabKot = "3174";//set default value jika tidak ada di db
                                                    var prov = "31";//set default value jika tidak ada di db
#region cut max length pembeli
                                                    var nama = order.buyer.name.Replace('\'', '`');
                                                    if (nama.Length > 30)
                                                        nama = nama.Substring(0, 30);
                                                    string tlp = !string.IsNullOrEmpty(order.delivery.consignee.phone) ? order.delivery.consignee.phone.Replace('\'', '`') : "";
                                                    if (tlp.Length > 30)
                                                    {
                                                        tlp = tlp.Substring(0, 30);
                                                    }
                                                    string AL_KIRIM1 = !string.IsNullOrEmpty(order.delivery.consignee.address) ? order.delivery.consignee.address.Replace('\'', '`') : "";
                                                    if (AL_KIRIM1.Length > 30)
                                                    {
                                                        AL_KIRIM1 = AL_KIRIM1.Substring(0, 30);
                                                    }
                                                    string KODEPOS = !string.IsNullOrEmpty(order.delivery.consignee.postal_code) ? order.delivery.consignee.postal_code.Replace('\'', '`') : "";
                                                    if (KODEPOS.Length > 7)
                                                    {
                                                        KODEPOS = KODEPOS.Substring(0, 7);
                                                    }

                                                    string namaKabkot = (string.IsNullOrEmpty(order.delivery.consignee.city) ? "" : order.delivery.consignee.city.Replace("'", "`"));
                                                    if (namaKabkot.Length > 50)
                                                        namaKabkot = namaKabkot.Substring(0, 50);

                                                    string namaProv = string.IsNullOrEmpty(order.delivery.consignee.province) ? "" : order.delivery.consignee.province.Replace("'", "`");
                                                    if (namaProv.Length > 50)
                                                        namaProv = namaProv.Substring(0, 50);
#endregion
                                                    insertPembeli += "('" + nama + "','" + order.delivery.consignee.address.Replace('\'', '`') + "','" + tlp + "','',0,0,'0','01',";
                                                    insertPembeli += "1, 'IDR', '01', '" + AL_KIRIM1.Replace('\'', '`') + "', 0, 0, 0, 0, '1', 0, 0, ";
                                                    insertPembeli += "'FP', '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '"
                                                        + KODEPOS + "', '', '" + kabKot + "', '" + prov + "', '" + namaKabkot
                                                        + "', '" + namaProv.Replace('\'', '`') + "', '" + conn_id + "')";

                                                    EDB.ExecuteSQL("MOConnectionString", CommandType.Text, insertPembeli);

                                                    SqlCommand CommandSQL = new SqlCommand();
                                                    CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                                    CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = conn_id;

                                                    EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);

                                                    var pembeli = ErasoftDbContext.ARF01C.Where(m => m.TLP == order.delivery.consignee.phone).FirstOrDefault();
                                                    if(pembeli != null)
                                                    {
                                                        EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "UPDATE SOT01A SET PEMESAN = '"+pembeli.BUYER_CODE+"' WHERE NO_BUKTI = '"+currentOrder.NO_BUKTI+"'");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_BL_ORDER");
                                        ErasoftDbContext.Database.ExecuteSqlCommand("DELETE FROM TEMP_BL_ORDERITEMS");

                                        string insertQ = "INSERT INTO TEMP_BL_ORDER ([ID],[INVOICE_ID],[STATE],[TRANSACTION_ID],[AMOUNT],[QUANTITY],[COURIER],[BUYERS_NOTE],[SHIPPING_FEE],";
                                        insertQ += "[SHIPPING_ID],[SHIPPING_CODE],[SHIPPING_SERVICE],[SUBTOTAL_AMOUNT],[TOTAL_AMOUNT],[PAYMENT_AMOUNT],[CREATED_AT],[UPDATED_AT],";
                                        insertQ += "[BUYER_EMAIL],[BUYER_ID],[BUYER_NAME],[BUYER_USERNAME],[BUYER_LOGISTIC_CHOICE],[CONSIGNEE_ADDRESS],[CONSIGNEE_AREA],[CONSIGNEE_CITY],";
                                        insertQ += "[CONSIGNEE_NAME],[CONSIGNEE_PHONE],[CONSIGNEE_POSTCODE],[CONSIGNEE_PROVICE],[CUST],[USERNAME],[CONNECTION_ID]) VALUES ";

                                        string insertOrderItems = "INSERT INTO TEMP_BL_ORDERITEMS ([ID],[TRANSACTION_ID],[PRODUCT_ID],[CATEGORY],[CATEGORY_ID],[NAME]";
                                        insertOrderItems += ",[PRICE],[WEIGHT],[DESC],[CONDITON],[STOCK],[QTY], [CREATED_AT], [UPDATED_AT], [USERNAME], [CONNECTION_ID]) VALUES ";

                                        string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                        insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                        insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";


                                        var nama = order.buyer.name.Replace('\'', '`');
                                        if (nama.Length > 30)
                                            nama = nama.Substring(0, 30);
                                        var nama2 = order.delivery.consignee.name.Replace('\'', '`');
                                        if (nama2.Length > 30)
                                            nama2 = nama2.Substring(0, 30);
#region cut max length pembeli
                                        string tlp = !string.IsNullOrEmpty(order.delivery.consignee.phone) ? order.delivery.consignee.phone.Replace('\'', '`') : "";
                                        if (tlp.Length > 30)
                                        {
                                            tlp = tlp.Substring(0, 30);
                                        }
                                        string AL_KIRIM1 = !string.IsNullOrEmpty(order.delivery.consignee.address) ? order.delivery.consignee.address.Replace('\'', '`') : "";
                                        if (AL_KIRIM1.Length > 30)
                                        {
                                            AL_KIRIM1 = AL_KIRIM1.Substring(0, 30);
                                        }
                                        string KODEPOS = !string.IsNullOrEmpty(order.delivery.consignee.postal_code) ? order.delivery.consignee.postal_code.Replace('\'', '`') : "";
                                        if (KODEPOS.Length > 7)
                                        {
                                            KODEPOS = KODEPOS.Substring(0, 7);
                                        }

                                        string namaKabkot = (string.IsNullOrEmpty(order.delivery.consignee.city) ? "" : order.delivery.consignee.city.Replace("'", "`"));
                                        if (namaKabkot.Length > 50)
                                            namaKabkot = namaKabkot.Substring(0, 50);

                                        string namaProv = string.IsNullOrEmpty(order.delivery.consignee.province) ? "" : order.delivery.consignee.province.Replace("'", "`");
                                        if (namaProv.Length > 50)
                                            namaProv = namaProv.Substring(0, 50);
#endregion
#region cut max length header
                                        string transId = (string.IsNullOrEmpty(order.transaction_id) ? "" : order.transaction_id.Replace("'", "`"));
                                        if (transId.Length > 50)
                                            transId = transId.Substring(0, 50);
                                        string courier = (string.IsNullOrEmpty(order.delivery.carrier) ? "" : order.delivery.carrier.Replace("'", "`"));
                                        if (courier.Length > 50)
                                            courier = courier.Substring(0, 50);
                                        string shippingService = (string.IsNullOrEmpty(order.delivery.requested_carrier) ? "" : order.delivery.requested_carrier.Replace("'", "`"));
                                        if (shippingService.Length > 50)
                                            shippingService = shippingService.Substring(0, 50);
                                        string buyerLogistic = (string.IsNullOrEmpty(order.delivery.buyer_logistic_choice) ? "" : order.delivery.buyer_logistic_choice.Replace("'", "`"));
                                        if (buyerLogistic.Length > 100)
                                            buyerLogistic = buyerLogistic.Substring(0, 100);
                                        string consigneeArea = (string.IsNullOrEmpty(order.delivery.consignee.district) ? "" : order.delivery.consignee.district.Replace("'", "`"));
                                        if (consigneeArea.Length > 100)
                                            consigneeArea = consigneeArea.Substring(0, 100);
                                        var ketPembeli = "";
                                        if(order.options != null)
                                        {
                                            if (!string.IsNullOrEmpty(order.options.buyer_note))
                                            {
                                                ketPembeli = order.options.buyer_note.Replace('\'', '`');
                                            }
                                        }
                                        string paymentType = (string.IsNullOrEmpty(order.payment_method) ? "" : order.payment_method.Replace("'", "`"));
                                        if (paymentType.Length > 50)
                                            paymentType = paymentType.Substring(0, 50);
                                        #endregion
                                        string statusEra = "0";
                                        if (order.state != "pending")
                                        {
                                            statusEra = "01";
                                        }
                                        string sTipe_pesanan = "";
                                        if(order.sla != null)
                                        {
                                            if(order.sla.type == "preorder")
                                            {
                                                sTipe_pesanan = "Preorder";
                                            }
                                        }
                                        insertQ += "(" + order.id + "," + order.invoice_id + ",'" + statusEra + "','" + transId + "'," + order.amount.buyer.total + ",0,'" 
                                            + courier + "','" + ketPembeli + "'," + order.amount.buyer.details.delivery + ",";
                                        insertQ += "0,'" + sTipe_pesanan + "','" + shippingService + "'," + order.amount.buyer.coded_amount + "," + order.amount.seller.total + "," 
                                            + order.amount.buyer.payment_amount + ",'" +  order.created_at.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.state_changed_at.refund_at.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "','";
                                        insertQ += "','" + order.buyer.id + "','" + nama + "','"+paymentType+"','" + buyerLogistic + "','" + order.delivery.consignee.address.Replace('\'', '`') + "','" 
                                            + consigneeArea.Replace('\'', '`') + "','" + namaKabkot + "','";
                                        insertQ += nama2 + "','" + tlp + "','" + KODEPOS + "','" + namaProv + "','" + CUST + "','" + username + "','" + conn_id + "')";

                                        if (order.items != null)
                                        {
                                            foreach (var items in order.items)
                                            {
#region cut max length details
                                                string katName = (string.IsNullOrEmpty(items.category.name) ? "" : items.category.name.Replace("'", "`"));
                                                if (katName.Length > 50)
                                                    katName = katName.Substring(0, 50);
                                                var items_name = string.IsNullOrEmpty(items.name) ? "" : items.name.Replace('\'', '`');
                                                if (!string.IsNullOrEmpty(items.stuff.variant_name))
                                                {
                                                    items_name += " " + items.stuff.variant_name.Replace('\'', '`');
                                                }
                                                if (items_name.Length > 285)
                                                    items_name = items_name.Substring(0, 285);
                                                string condition = (string.IsNullOrEmpty(items.stuff.product.condition) ? "" : items.stuff.product.condition.Replace("'", "`"));
                                                if (condition.Length > 50)
                                                    condition = condition.Substring(0, 50);

#endregion
                                                var brgmp = items.stuff.product.id + ";" + items.stuff.id;
                                                insertOrderItems += "(" + order.id + ", '" + transId + "','" +  brgmp  + "','" + katName + "',0,'" + items_name + "',";
                                                insertOrderItems += items.price + "," + items.stuff.product.weight + ",'','" + condition + "',0," 
                                                    + items.quantity + ",'" + order.created_at.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.updated_at.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "','" 
                                                    + username + "','" + conn_id + "')";
                                                insertOrderItems += ",";
                                            }
                                        }

                                        //var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + order.consignee.city + "%'");
                                        //var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + order.consignee.province + "%'");

                                        var kabKot = "3174";//set default value jika tidak ada di db
                                        var prov = "31";//set default value jika tidak ada di db

                                        //if (tblProv.Tables[0].Rows.Count > 0)
                                        //    prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
                                        //if (tblKabKot.Tables[0].Rows.Count > 0)
                                        //    kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();

                                        //insertPembeli += "('" + order.buyer.name.Replace('\'', '`') + "','" + order.consignee.address.Replace('\'', '`') + "','" + order.consignee.phone + "','" + order.buyer.email.Replace('\'', '`') + "',0,0,'0','01',";
                                        insertPembeli += "('" + nama + "','" + order.delivery.consignee.address.Replace('\'', '`') + "','" + tlp + "','',0,0,'0','01',";
                                        insertPembeli += "1, 'IDR', '01', '" + AL_KIRIM1 + "', 0, 0, 0, 0, '1', 0, 0, ";
                                        insertPembeli += "'FP', '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '" 
                                            + KODEPOS + "', '', '" + kabKot + "', '" + prov + "', '" + namaKabkot + "', '" + namaProv + "', '" + conn_id + "')";

                                        //if (i < bindOrder.transactions.Length)
                                        insertQ += ",";
                                        insertPembeli += ",";

                                        insertQ = insertQ.Substring(0, insertQ.Length - 1);
                                        var a = EDB.ExecuteSQL(username, CommandType.Text, insertQ);
                                       
                                        insertOrderItems = insertOrderItems.Substring(0, insertOrderItems.Length - 1);
                                        a = EDB.ExecuteSQL(username, CommandType.Text, insertOrderItems);

                                        if (!order.delivery.consignee.phone.Contains("Terkunci"))
                                        {
                                            insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 1);
                                            a = EDB.ExecuteSQL(username, CommandType.Text, insertPembeli);
                                        }

#region call sp
                                        SqlCommand CommandSQL = new SqlCommand();
                                        if (!order.delivery.consignee.phone.Contains("Terkunci"))
                                        {
                                            //add by Tri call sp to insert buyer data
                                            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = conn_id;

                                            EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                                            //end add by Tri call sp to insert buyer data
                                        }

                                        CommandSQL = new SqlCommand();
                                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = conn_id;
                                        CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.UtcNow.AddHours(7).AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                                        CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss");
                                        CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 1;
                                        CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                                        CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = CUST;

                                        EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);
#endregion
                                        jmlhNewOrder++;
                                    }
                                }

                                if(jmlhNewOrder > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(data.dbPathEra).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari Bukalapak.");

                                    //add by nurul 25/1/2021, bundling
                                    var listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in ('" + conn_id + "')").ToList();
                                    if (listBrgKomponen.Count() > 0)
                                    {
                                        ret.AdaKomponen = true;
                                    }
                                    //end add by nurul 25/1/2021, bundling

                                    new StokControllerJob().updateStockMarketPlace(conn_id, data.dbPathEra, username);

                                    //add by nurul 19/1/2021, bundling 
                                    ret.AdaPesanan = true;
                                    //end add by nurul 19/1/2021, bundling
                                }
                            }
                        }
                    }
                }

            }
            return ret;
        }

        [HttpGet]
        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrdersCompleted(BukaLapakKey data, string CUST, string NAMA_CUST, string username)
        {
            string ret = "";
            SetupContext(data.dbPathEra, username);
            var dtNow = DateTime.UtcNow.AddHours(7);
            var loop = true;
            var page = 0;
            while (loop)
            {
                data = RefreshToken(data);
                var retOrder = await GetOrdersCompletedLoop(data, CUST, NAMA_CUST, username, page, dtNow.AddDays(-10).ToString("yyyy-MM-ddTHH:mm:ss"), dtNow.ToString("yyyy-MM-ddTHH:mm:ss"), 0);
                if (retOrder >= 50)
                {
                    page = page + 1;
                }
                else
                {
                    loop = false;
                }
            }

            var queryStatus = "\"\\\"" + CUST + "\\\"\",\"\\";    // "\"000001\"","\
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + queryStatus + "%' and invocationdata like '%bukalapak%' and invocationdata like '%GetOrdersCompleted%' and statename like '%Enque%' and invocationdata not like '%resi%' ");

            return ret;
        }

        public async Task<int> GetOrdersCompletedLoop(BukaLapakKey data, string CUST, string NAMA_CUST, string username, int page, string fromDt, string toDt, int retry)
        {
            var ret = 0;
            var conn_id = Guid.NewGuid().ToString();
            //int jmlhNewOrder = 0;
            //data = RefreshToken(data);
            var list_04 = new List<string>();

            string urll = "https://api.bukalapak.com/transactions?limit=50&offset=" + (page * 50) + "&context=sale"
                + "&start_time=" + Uri.EscapeDataString(fromDt) + "&end_time=" + Uri.EscapeDataString(toDt)
                + "&states[]=received&states[]=remitted";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", "Bearer " + data.token);
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
                    var response = e.Response as HttpWebResponse;
                    var status = (int)response.StatusCode;
                    if (status == 401)
                    {
                        if (retry == 0)
                        {
                            data = RefreshToken(data);
                            await GetOrdersCompletedLoop(data, CUST, NAMA_CUST, username, page, fromDt, toDt, 1);
                        }
                        else
                        {
                            throw new Exception(err);
                        }
                    }
                    else
                    {
                        throw new Exception(err);
                    }
                }
                else
                {
                    throw new Exception(e.Message);
                }
            }
            if (responseFromServer != "")
            {
                GetOrdersResponse retObj = JsonConvert.DeserializeObject(responseFromServer, typeof(GetOrdersResponse)) as GetOrdersResponse;
                if (retObj != null)
                {
                    if (retObj.meta.http_status == 200)
                    {
                        if (retObj.data != null)
                        {
                            if (retObj.data.Length > 0)
                            {
                                ret = retObj.data.Length;
                                var cekfromDt = DateTime.UtcNow.AddHours(-7).AddDays(-10);
                                //var untukCekdiSO = retObj.data.Select(p => new { id = p.id }).AsEnumerable().Select(t => t.id.ToString()).ToList();
                                var untukCekdiSO = retObj.data.Select(p => p.transaction_id).ToList();
                                var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == CUST && p.TGL.Value >= cekfromDt && untukCekdiSO.Contains(p.NO_REFERENSI)).
                                    Select(p => new { NO_REFERENSI = p.NO_REFERENSI, NO_BUKTI = p.NO_BUKTI, PEMESAN = p.PEMESAN, STATUS_TRANSAKSI = p.STATUS_TRANSAKSI }).
                                    ToList();
                                var listNoRefInSO = OrderNoInDb.Select(p => p.NO_REFERENSI).ToList();
                                var orderWithOrderIdInSO = retObj.data.Where(p => listNoRefInSO.Contains(p.transaction_id)).ToList();
                                if (orderWithOrderIdInSO.Count > 0)
                                {
                                    var listNoRefUntukCekSIT01a = orderWithOrderIdInSO.Select(p => p.transaction_id).ToList();
                                    var getSIT01A = ErasoftDbContext.SIT01A.Where(p => listNoRefUntukCekSIT01a.Contains(p.NO_REF)).Select(p => p.NO_REF).ToList();
                                    foreach (var order in orderWithOrderIdInSO)
                                    {
                                        var orderMO = OrderNoInDb.Where(p => p.NO_REFERENSI == order.transaction_id).FirstOrDefault();
                                        if(orderMO != null)
                                        {
                                            if (string.IsNullOrEmpty(orderMO.PEMESAN) && order.delivery != null)
                                            {
                                                if (!order.delivery.consignee.phone.Contains("Terkunci"))
                                                {
                                                    InsertPembeli(order, conn_id, data.dbPathEra, username);
                                                    var pembeliInDB = ErasoftDbContext.ARF01C.Where(m => m.TLP == order.delivery.consignee.phone).FirstOrDefault();
                                                    if (pembeliInDB != null)
                                                    {
                                                        var rowAffected2 = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET PEMESAN = '" + pembeliInDB.BUYER_CODE + "' WHERE NO_BUKTI = '" + orderMO.NO_BUKTI + "'");
                                                    }
                                                    else
                                                    {
                                                        var adaPembeliGagalInsert = true;
                                                    }
                                                }
                                            }

                                            if(order.state == "remitted" || order.state == "received")
                                            {
                                                if (orderMO.STATUS_TRANSAKSI != "04")
                                                {
                                                    if (getSIT01A.Contains(order.transaction_id))
                                                    {
                                                        list_04.Add(orderMO.NO_BUKTI);
                                                    }
                                                }

                                            }
                                        }
                                    }

                                    if (list_04.Count > 0)
                                    {
                                        string noBuktiSO = "";
                                        string sSQL = "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_BUKTI IN (";
                                        foreach (var nobuk in list_04)
                                        {
                                            sSQL += "'" + nobuk + "' ,";
                                            noBuktiSO += "'" + nobuk + "' ,";
                                        }
                                        sSQL = sSQL.Substring(0, sSQL.Length - 2) + ")";
                                        var result = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);

                                        //add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                        var dateTimeNow = Convert.ToDateTime(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"));
                                        noBuktiSO = noBuktiSO.Substring(0, noBuktiSO.Length - 2) + ")";
                                        string sSQLUpdateDatePesananSelesai = "UPDATE SIT01A SET TGL_KIRIM = '" + dateTimeNow + "' WHERE NO_SO IN (" + noBuktiSO;
                                        var resultUpdateDatePesanan = EDB.ExecuteSQL("CString", CommandType.Text, sSQLUpdateDatePesananSelesai);
                                        //end add by fauzi 23/09/2020 update tanggal pesanan untuk fitur upload faktur FTP
                                    }
                                }
                            }
                        }
                    }
                }
            }
                return ret;
        }

        [HttpGet]
        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public async Task<string> GetOrdersCanceled(BukaLapakKey data, string CUST, string NAMA_CUST, string username)
        {
            string ret = "";
            SetupContext(data.dbPathEra, username);
            var dtNow = DateTime.UtcNow.AddHours(7).AddDays(-7);
            var loop = true;
            var page = 0;

            //add by nurul 20/1/2021, bundling 
            var AdaKomponen = false;
            List<string> tempConnId = new List<string>() { };
            //end add by nurul 20/1/2021, bundling 

            var orderList = (from a in ErasoftDbContext.SOT01A
                                   where a.USER_NAME == "Auto Bukalapak" && a.STATUS_TRANSAKSI != "11" && a.CUST == CUST && a.TGL >= dtNow
                                    && a.STATUS_TRANSAKSI != "12"
                             select a.NO_REFERENSI).ToList();

            while (loop)
            {
                data = RefreshToken(data);
                var retOrder = await GetOrdersCanceledLoop(data, CUST, NAMA_CUST, username, page, dtNow.ToString("yyyy-MM-ddTHH:mm:ss"), dtNow.AddDays(7).ToString("yyyy-MM-ddTHH:mm:ss"), orderList, 0);
                if (retOrder.AdaKomponen)
                {
                    AdaKomponen = retOrder.AdaKomponen;
                }
                if (retOrder.status >= 50)
                {
                    page = page + 1;
                }
                else
                {
                    loop = false;
                }
            }
            if (AdaKomponen)
            {
                new StokControllerJob().getQtyBundling(data.dbPathEra, username);
            }
            var queryStatus = "\"\\\"" + CUST + "\\\"\",\"\\";    // "\"000001\"","\
            var execute = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "delete from hangfire.job where arguments like '%" + queryStatus + "%' and invocationdata like '%GetOrdersCanceled%' and invocationdata like '%GetOrdersCompleted%' and statename like '%Enque%' and invocationdata not like '%resi%' ");

            return ret;
        }
        public async Task<BindingBase> GetOrdersCanceledLoop(BukaLapakKey data, string CUST, string NAMA_CUST, string username, int page, string fromDt, string toDt, List<string> orderList, int retry)
        {
            var ret = new BindingBase();
            ret.status = 0;
            var conn_id = Guid.NewGuid().ToString();
            int jmlhOrder = 0;
            //data = RefreshToken(data);
            var brgCancelled = new List<TEMP_ALL_MP_ORDER_ITEM>();

            string urll = "https://api.bukalapak.com/transactions?limit=50&offset=" + (page * 50) + "&context=sale"
                + "&start_time=" + Uri.EscapeDataString(fromDt) + "&end_time=" + Uri.EscapeDataString(toDt)
                + "&states[]=cancelled&states[]=expired";
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "GET";
            myReq.Headers.Add("Authorization", "Bearer " + data.token);
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
                    var response = e.Response as HttpWebResponse;
                    var status = (int)response.StatusCode;
                    if (status == 401)
                    {
                        if (retry == 0)
                        {
                            data = RefreshToken(data);
                            await GetOrdersCanceledLoop(data, CUST, NAMA_CUST, username, page, fromDt, toDt, orderList, 1);
                        }
                        else
                        {
                            throw new Exception(err);
                        }
                    }
                    else
                    {
                        throw new Exception(err);
                    }
                }
                else
                {
                    throw new Exception(e.Message);
                }
            }
            if (responseFromServer != "")
            {
                GetOrdersResponse retObj = JsonConvert.DeserializeObject(responseFromServer, typeof(GetOrdersResponse)) as GetOrdersResponse;
                if (retObj != null)
                {
                    if (retObj.meta.http_status == 200)
                    {
                        if (retObj.data != null)
                        {
                            if (retObj.data.Length > 0)
                            {
                                ret.status = retObj.data.Length;
                                string sSQL = "INSERT INTO SOT01D (NO_BUKTI, CATATAN_1, USERNAME) VALUES ";
                                string sSQL2 = "";
                                foreach (var order in retObj.data)
                                {
                                    if (orderList.Contains(order.transaction_id))
                                    {
                                        var dsOrder = EDB.GetDataSet("MOConnectionString", "ORDER", "SELECT P.NO_BUKTI, ISNULL(F.NO_BUKTI, '') NO_FAKTUR, ISNULL(TIPE_KIRIM,0) TIPE_KIRIM "
                                            + ",ISNULL(F.NO_FA_OUTLET, '-') NO_FA_OUTLET FROM SOT01A (NOLOCK) P LEFT JOIN SIT01A (NOLOCK) F ON P.NO_BUKTI = F.NO_SO "
                                            + "WHERE NO_REFERENSI = '" + order.transaction_id + "' AND CUST = '" + CUST + "'");
                                        int rowAffected = 0;
                                        var nobuk = "";
                                        bool cekSudahKirimCOD = false;
                                        //change by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                                        //var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2',STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN ('" + order.transaction_id + "') AND STATUS_TRANSAKSI <> '11' AND CUST = '" + CUST + "'");
                                        //var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS='2',STATUS_TRANSAKSI = '11', STATUS_KIRIM='5' WHERE NO_REFERENSI IN ('" + order.transaction_id + "') AND STATUS_TRANSAKSI <> '11' AND CUST = '" + CUST + "'");
                                        //END change by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus

                                        if (dsOrder.Tables[0].Rows.Count > 0)
                                        {
                                            nobuk = dsOrder.Tables[0].Rows[0]["NO_BUKTI"].ToString();
                                            if (dsOrder.Tables[0].Rows[0]["TIPE_KIRIM"].ToString() != "1")
                                            {
                                                rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text,
                                                    "UPDATE SOT01A SET STATUS='2',STATUS_TRANSAKSI = '11', STATUS_KIRIM='5' WHERE NO_REFERENSI IN ('"
                                                    + order.transaction_id + "') AND STATUS_TRANSAKSI <> '11' AND CUST = '" + CUST + "'");
                                            }
                                            else//pesanan cod
                                            {
                                                if (order.state_changed_at.delivered_at.HasValue)
                                                {
                                                    rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text,
                                                        "UPDATE SOT01A SET STATUS_TRANSAKSI = '12', STATUS_KIRIM='5' WHERE NO_REFERENSI IN ('"
                                                        + order.transaction_id + "') AND STATUS_TRANSAKSI <> '12' AND CUST = '" + CUST + "'");
                                                    cekSudahKirimCOD = true;
                                                }
                                                else
                                                {
                                                    rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text,
                                                        "UPDATE SOT01A SET STATUS='2',STATUS_TRANSAKSI = '11', STATUS_KIRIM='5' WHERE NO_REFERENSI IN ('"
                                                        + order.transaction_id + "') AND STATUS_TRANSAKSI <> '11' AND CUST = '" + CUST + "'");
                                                }

                                            }
                                        }
                                        if (rowAffected > 0)
                                        {
                                            //add by Tri 1 sep 2020, hapus packing list
                                            //remark by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                                            //var delPL = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03B WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN ('" + order.transaction_id + "')  AND STATUS_TRANSAKSI = '11' AND CUST = '" + CUST + "')");
                                            //var delPLDetail = EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "DELETE FROM SOT03C WHERE NO_PESANAN IN (SELECT NO_BUKTI FROM SOT01A WHERE NO_REFERENSI IN ('" + order.transaction_id + "')  AND STATUS_TRANSAKSI = '11' AND CUST = '" + CUST + "')");
                                            //END remark by nurul 16/2/2021, status kirim aja yg diubah jd batal, packing tidak dihapus
                                            //end add by Tri 1 sep 2020, hapus packing list
                                            jmlhOrder = jmlhOrder + rowAffected;
                                            //add by Tri 4 Des 2019, isi cancel reason
                                            if (nobuk == "")
                                                nobuk = ErasoftDbContext.SOT01A.Where(m => m.NO_REFERENSI == order.transaction_id && m.CUST == CUST).Select(m => m.NO_BUKTI).FirstOrDefault();
                                            if (!string.IsNullOrEmpty(nobuk))
                                            {
                                                var sot01d = ErasoftDbContext.SOT01D.Where(m => m.NO_BUKTI == nobuk).FirstOrDefault();
                                                if (sot01d == null)
                                                {
                                                    string cancelReason = "";
                                                    if (!string.IsNullOrEmpty(order.options.reject_reason))
                                                    {
                                                        cancelReason = order.options.reject_reason;
                                                    }
                                                    if (!string.IsNullOrEmpty(order.options.cancel_reason))
                                                    {
                                                        cancelReason = order.options.cancel_reason;
                                                        if (!string.IsNullOrEmpty(order.options.cancel_notes))
                                                            cancelReason += " : " + order.options.cancel_notes;
                                                    }
                                                    sSQL2 += "('" + nobuk + "','" + cancelReason + "','AUTO_BukaLapak'),";
                                                }
                                            }
                                            //end add by Tri 4 Des 2019, isi cancel reason
                                            //var fakturInDB = ErasoftDbContext.SIT01A.Where(m => m.CUST == CUST && m.NO_REF == order.transaction_id).FirstOrDefault();
                                            //if (fakturInDB != null)
                                            if (!cekSudahKirimCOD)
                                            {
                                                if (!string.IsNullOrEmpty(dsOrder.Tables[0].Rows[0]["NO_FAKTUR"].ToString()))
                                                {
                                                    //var returFaktur = ErasoftDbContext.SIT01A.Where(m => m.JENIS_FORM == "3" && m.NO_REF == fakturInDB.NO_BUKTI).FirstOrDefault();
                                                    //if (returFaktur == null)
                                                    var no_retur = dsOrder.Tables[0].Rows[0]["NO_FA_OUTLET"].ToString();
                                                    if (no_retur.Contains("-"))
                                                    {
                                                        var rowAffectedSI = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SIT01A SET STATUS='2' WHERE NO_REF IN ('" + order.transaction_id + "') AND STATUS <> '2' AND ST_POSTING = 'T' AND CUST = '" + CUST + "'");
                                                    }

                                                }
                                            }
                                            var orderDetail = (from a in ErasoftDbContext.SOT01A
                                                               join b in ErasoftDbContext.SOT01B on a.NO_BUKTI equals b.NO_BUKTI
                                                               //where a.NO_REFERENSI == order.order_id
                                                               where a.NO_REFERENSI == order.transaction_id && b.BRG != "NOT_FOUND" && a.CUST == CUST
                                                               select new { b.BRG }).ToList();
                                            foreach (var item in orderDetail)
                                            {
                                                if (brgCancelled.Where(p => p.BRG == item.BRG).Count() <= 0)
                                                {
                                                    brgCancelled.Add(new TEMP_ALL_MP_ORDER_ITEM() { BRG = item.BRG, CONN_ID = conn_id });
                                                }
                                            }
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(sSQL2))
                                {
                                    sSQL += sSQL2.Substring(0, sSQL2.Length - 1);
                                    EDB.ExecuteSQL("MOConnectionString", CommandType.Text, sSQL);
                                }
                                if (jmlhOrder > 0)
                                {
                                    var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                                    contextNotif.Clients.Group(data.dbPathEra).moNewOrder("" + Convert.ToString(jmlhOrder) + " Pesanan dari BukaLapak dibatalkan.");
                                }
                            }
                        }
                    }
                }
            }
            var itemCount = brgCancelled.Count();
            if (itemCount > 0)
            {
                string sSQL = "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG,CONN_ID)" + System.Environment.NewLine;
                int indexCount = 0;
                foreach (var item in brgCancelled)
                {
                    indexCount = indexCount + 1;
                    //sSQL += "SELECT '' AS BRG, '' AS CONN_ID " + System.Environment.NewLine;
                    sSQL += "SELECT '" + item.BRG + "' AS BRG, '" + item.CONN_ID + "' AS CONN_ID " + System.Environment.NewLine;
                    if (indexCount < itemCount)
                    {
                        sSQL += "UNION ALL " + System.Environment.NewLine;
                    }
                }
                var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, sSQL);

                //add by nurul 25/1/2021, bundling
                var listBrgKomponen = ErasoftDbContext.Database.SqlQuery<string>("select distinct a.brg from TEMP_ALL_MP_ORDER_ITEM a(nolock) inner join stf03 b(nolock) on a.brg=b.brg where a.CONN_ID in ('" + conn_id + "')").ToList();
                if (listBrgKomponen.Count() > 0)
                {
                    ret.AdaKomponen = true;
                }
                //end add by nurul 25/1/2021, bundling
                new StokControllerJob().updateStockMarketPlace(conn_id, data.dbPathEra, username);
            }
            return ret;
        }
        public void InsertPembeli(GetOrdersDatum order, string conn_id, string dbPathEra, string username)
        {
            string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
            insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
            insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";

            var kabKot = "3174";//set default value jika tidak ada di db
            var prov = "31";//set default value jika tidak ada di db
#region cut max length pembeli
            var nama = order.buyer.name.Replace('\'', '`');
            if (nama.Length > 30)
                nama = nama.Substring(0, 30);
            string tlp = !string.IsNullOrEmpty(order.delivery.consignee.phone) ? order.delivery.consignee.phone.Replace('\'', '`') : "";
            if (tlp.Length > 30)
            {
                tlp = tlp.Substring(0, 30);
            }
            string AL_KIRIM1 = !string.IsNullOrEmpty(order.delivery.consignee.address) ? order.delivery.consignee.address.Replace('\'', '`') : "";
            if (AL_KIRIM1.Length > 30)
            {
                AL_KIRIM1 = AL_KIRIM1.Substring(0, 30);
            }
            string KODEPOS = !string.IsNullOrEmpty(order.delivery.consignee.postal_code) ? order.delivery.consignee.postal_code.Replace('\'', '`') : "";
            if (KODEPOS.Length > 7)
            {
                KODEPOS = KODEPOS.Substring(0, 7);
            }

            string namaKabkot = (string.IsNullOrEmpty(order.delivery.consignee.city) ? "" : order.delivery.consignee.city.Replace("'", "`"));
            if (namaKabkot.Length > 50)
                namaKabkot = namaKabkot.Substring(0, 50);

            string namaProv = string.IsNullOrEmpty(order.delivery.consignee.province) ? "" : order.delivery.consignee.province.Replace("'", "`");
            if (namaProv.Length > 50)
                namaProv = namaProv.Substring(0, 50);
#endregion
            insertPembeli += "('" + nama + "','" + order.delivery.consignee.address.Replace('\'', '`') + "','" + tlp + "','',0,0,'0','01',";
            insertPembeli += "1, 'IDR', '01', '" + AL_KIRIM1.Replace('\'', '`') + "', 0, 0, 0, 0, '1', 0, 0, ";
            insertPembeli += "'FP', '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '"
                + KODEPOS + "', '', '" + kabKot + "', '" + prov + "', '" + namaKabkot
                + "', '" + namaProv.Replace('\'', '`') + "', '" + conn_id + "')";

            SqlCommand CommandSQL = new SqlCommand();
            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = conn_id;

            EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);

        }
        [HttpGet]
        [AutomaticRetry(Attempts = 2)]
        [Queue("3_general")]
        public BindingBase cekTransaksi(/*string transId,*/ string Cust, string email, string userId, string token, string dbPathEra, string uname)
        {
            //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
            var ret = new BindingBase();
            ret.status = 0;
            SetupContext(dbPathEra, uname);
            string connectionID = Guid.NewGuid().ToString();

            Utils.HttpRequest req = new Utils.HttpRequest();
            string url = "transactions.json?seller=1&created_since=" + DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd");
            //if (!string.IsNullOrEmpty(transId))
            //    url = "transactions/" + transId + ".json";
            var jmlhNewOrder = 0;//add by calvin 1 april 2019
            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //    REQUEST_ACTION = "Get Order",
            //    REQUEST_DATETIME = DateTime.Now,
            //    REQUEST_ATTRIBUTE_1 = token,
            //    REQUEST_ATTRIBUTE_2 = email,
            //    REQUEST_ATTRIBUTE_3 = connectionID,
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, userId, currentLog);

            var bindOrder = req.CallBukaLapakAPI("", url + "?", "", userId, token, typeof(BukaLapakOrder)) as BukaLapakOrder;
            if (bindOrder != null)
            {
                //ret = bindOrder;
                if (bindOrder.status.Equals("OK"))
                {
                    var OrderNoInDb = ErasoftDbContext.SOT01A.Where(p => p.CUST == Cust).Select(p => p.NO_REFERENSI).ToList();
                    bool adaInsert = false;

                    string insertQ = "INSERT INTO TEMP_BL_ORDER ([ID],[INVOICE_ID],[STATE],[TRANSACTION_ID],[AMOUNT],[QUANTITY],[COURIER],[BUYERS_NOTE],[SHIPPING_FEE],";
                    insertQ += "[SHIPPING_ID],[SHIPPING_CODE],[SHIPPING_SERVICE],[SUBTOTAL_AMOUNT],[TOTAL_AMOUNT],[PAYMENT_AMOUNT],[CREATED_AT],[UPDATED_AT],";
                    insertQ += "[BUYER_EMAIL],[BUYER_ID],[BUYER_NAME],[BUYER_USERNAME],[BUYER_LOGISTIC_CHOICE],[CONSIGNEE_ADDRESS],[CONSIGNEE_AREA],[CONSIGNEE_CITY],";
                    insertQ += "[CONSIGNEE_NAME],[CONSIGNEE_PHONE],[CONSIGNEE_POSTCODE],[CONSIGNEE_PROVICE],[CUST],[USERNAME],[CONNECTION_ID]) VALUES ";

                    string insertOrderItems = "INSERT INTO TEMP_BL_ORDERITEMS ([ID],[TRANSACTION_ID],[PRODUCT_ID],[CATEGORY],[CATEGORY_ID],[NAME]";
                    insertOrderItems += ",[PRICE],[WEIGHT],[DESC],[CONDITON],[STOCK],[QTY], [CREATED_AT], [UPDATED_AT], [USERNAME], [CONNECTION_ID]) VALUES ";

                    string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                    insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                    insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";
                    //int i = 1;
                    var connIDARF01C = Guid.NewGuid().ToString();
                    //string username = sessionData?.Account != null ? sessionData.Account.Username : sessionData.User.Username;
                    var dtNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    foreach (Transaction order in bindOrder.transactions)
                    {
                        if (Convert.ToString(order.id) == "192164002916")
                        {

                        }
                        if (!order.buyer.email.Equals(email))//cek email pembeli != email user untuk mendapatkan order penjualan
                        {
                            bool doInsert = true;
                            //change by calvin 11 juni 2019
                            //if (OrderNoInDb.Contains(Convert.ToString(order.id)) && order.state.ToString().ToLower() == "paid")
                            //{
                            //    doInsert = false;
                            //}
                            //else if (order.state.ToString().ToLower() == "received" || order.state.ToString().ToLower() == "remitted")
                            //{
                            //    if (OrderNoInDb.Contains(Convert.ToString(order.id)))
                            //    {
                            //        //tidak ubah status menjadi selesai jika belum diisi faktur
                            //        var dsSIT01A = EDB.GetDataSet("CString", "SIT01A", "SELECT NO_REFERENSI, O.NO_BUKTI, O.STATUS_TRANSAKSI FROM SIT01A I INNER JOIN SOT01A O ON I.NO_SO = O.NO_BUKTI WHERE NO_REFERENSI = '" + order.id + "'");
                            //        if (dsSIT01A.Tables[0].Rows.Count == 0)
                            //        {
                            //            doInsert = false;
                            //        }
                            //    }
                            //    else
                            //    {
                            //        //tidak diinput jika order sudah selesai sebelum masuk MO
                            //        doInsert = false;
                            //    }
                            //}
                            if (OrderNoInDb.Contains(Convert.ToString(order.id)))
                            {
                                //tidak ubah status menjadi selesai jika belum diisi faktur
                                if (order.state.ToString().ToLower() == "received" || order.state.ToString().ToLower() == "remitted")
                                {
                                    if (order.state.ToString().ToLower() == "remitted")
                                    {
                                        doInsert = false;
                                    }
                                    else
                                    {
                                        var dsSIT01A = EDB.GetDataSet("CString", "SIT01A", "SELECT NO_REFERENSI, O.NO_BUKTI, O.STATUS_TRANSAKSI FROM SIT01A I INNER JOIN SOT01A O ON I.NO_SO = O.NO_BUKTI WHERE NO_REFERENSI = '" + order.id + "'");
                                        if (dsSIT01A.Tables[0].Rows.Count == 0)
                                        {
                                            doInsert = false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (order.state.ToString().ToLower() == "paid")
                                    {
                                        //tidak perlu insert karena pesanan sudah ada di MO pada saat statusnya masih pending / addressed / payment_chosen / confirm_payment, 
                                        //update status transaksi jadi 01 dan bila perlu update juga ongkir dll
                                        doInsert = false;
                                        var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '01' WHERE NO_REFERENSI IN ('" + order.id + "') AND STATUS_TRANSAKSI = '0'");
                                    }
                                    if (order.state.ToString().ToLower() == "rejected" || order.state.ToString().ToLower() == "expired" || order.state.ToString().ToLower() == "cancelled" || order.state.ToString().ToLower() == "refunded")
                                    {
                                        //tidak perlu insert karena pesanan sudah ada di MO, 
                                        //update status transaksi jadi cancel
                                        doInsert = false;
                                        var rowAffected = EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI IN ('" + order.id + "')");
                                    }
                                }
                            }
                            else
                            {
                                if (order.state.ToString().ToLower() == "received" || order.state.ToString().ToLower() == "remitted")
                                {
                                    //tidak diinput jika order sudah selesai sebelum masuk MO
                                    doInsert = false;
                                }
                            }
                            //end change by calvin 11 juni 2019

                            if (doInsert)
                            {
                                adaInsert = true;
                                var statusEra = "";
                                switch (order.state.ToString().ToLower())
                                {
                                    case "pending":
                                    case "addressed":
                                    case "payment_chosen":
                                    case "confirm_payment":
                                        statusEra = "0";
                                        break;
                                    case "paid":
                                        statusEra = "01";
                                        break;
                                    case "accepted":
                                        statusEra = "02";
                                        break;
                                    case "delivered":
                                        statusEra = "03";
                                        break;
                                    case "received":
                                    //statusEra = "03";
                                    //break;
                                    case "remitted":
                                        statusEra = "04";
                                        break;
                                    case "rejected":
                                    case "expired":
                                    case "cancelled":
                                    //statusEra = "11";
                                    //break;
                                    case "refunded":
                                        statusEra = "11";
                                        break;
                                    default:
                                        statusEra = "99";
                                        break;
                                }
                                //jika status pesanan sudah diubah di mo, dari 01 -> 02/03, status tidak dikembalikan ke 01
                                if (statusEra == "01")
                                {
                                    var currentStatus = EDB.GetFieldValue("", "SOT01A", "NO_REFERENSI = '" + order.id + "'", "STATUS_TRANSAKSI").ToString();
                                    if (!string.IsNullOrEmpty(currentStatus))
                                        if (currentStatus == "02" || currentStatus == "03")
                                            statusEra = currentStatus;
                                }
                                //end jika status pesanan sudah diubah di mo, dari 01 -> 02/03, status tidak dikembalikan ke 01
                                //jmlhNewOrder++;

                                var nama = order.buyer.name.Replace('\'', '`');
                                if (nama.Length > 30)
                                    nama = nama.Substring(0, 30);
                                var nama2 = order.consignee.name.Replace('\'', '`');
                                if (nama2.Length > 30)
                                    nama2 = nama2.Substring(0, 30);

                                insertQ += "(" + order.id + "," + order.invoice_id + ",'" + statusEra + "','" + order.transaction_id + "'," + order.amount + "," + order.quantity + ",'" + order.courier.Replace('\'', '`') + "','" + order.buyer_notes.Replace('\'', '`') + "'," + order.shipping_fee + ",";
                                insertQ += order.shipping_id + ",'" + order.shipping_code + "','" + order.shipping_service.Replace('\'', '`') + "'," + order.subtotal_amount + "," + order.total_amount + "," + order.payment_amount + ",'" + /*Convert.ToDateTime(order.created_at).ToString("yyyy-MM-dd HH:mm:ss")*/ order.created_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + /*Convert.ToDateTime(order.updated_at).ToString("yyyy-MM-dd HH:mm:ss")*/ order.updated_at.ToString("yyyy-MM-dd HH:mm:ss") + "','";
                                //insertQ += order.buyer.email.Replace('\'', '`') + "','" + order.buyer.id + "','" + order.buyer.name.Replace('\'', '`') + "','" + order.buyer.username.Replace('\'', '`') + "','" + order.buyer_logistic_choice.Replace('\'', '`') + "','" + order.consignee.address.Replace('\'', '`') + "','" + order.consignee.area.Replace('\'', '`') + "','" + order.consignee.city.Replace('\'', '`') + "','";
                                insertQ += order.buyer.email.Replace('\'', '`') + "','" + order.buyer.id + "','" + nama + "','" + order.buyer.username.Replace('\'', '`') + "','" + order.buyer_logistic_choice.Replace('\'', '`') + "','" + order.consignee.address.Replace('\'', '`') + "','" + order.consignee.area.Replace('\'', '`') + "','" + order.consignee.city.Replace('\'', '`') + "','";
                                //insertQ += order.consignee.name.Replace('\'', '`') + "','" + order.consignee.phone + "','" + order.consignee.post_code.Replace('\'', '`') + "','" + order.consignee.province.Replace('\'', '`') + "','" + Cust + "','" + username + "','" + connectionID + "')";
                                insertQ += nama2 + "','" + order.consignee.phone + "','" + order.consignee.post_code.Replace('\'', '`') + "','" + order.consignee.province.Replace('\'', '`') + "','" + Cust + "','" + username + "','" + connectionID + "')";

                                if (order.products != null)
                                {
                                    foreach (ProductBukaLapak items in order.products)
                                    {
                                        string namaBrg = "";
                                        //CHANGE BY CALVIN 19 DESEMBER 2018, NAMA BARANG DIISI DARI MARKETPLACE, UNTUK DISIMPAN DI CATATAN
                                        //var ds = EDB.GetDataSet("MOConnectionString", "0", "SELECT STF02.NAMA AS NAMA_BRG FROM STF02H S INNER JOIN ARF01 A ON S.AKUNMARKET = A.PERSO INNER JOIN STF02 ON S.BRG = STF02.BRG WHERE BRG_MP = '" + items.id + "' AND CUST = '" + Cust + "'");
                                        //if (ds.Tables[0].Rows.Count > 0)
                                        //{
                                        //    namaBrg = ds.Tables[0].Rows[0]["NAMA_BRG"].ToString();
                                        //}
                                        //END CHANGE BY CALVIN 19 DESEMBER 2018, NAMA BARANG DIISI DARI MARKETPLACE, UNTUK DISIMPAN DI CATATAN
                                        namaBrg = items.name + " " + items.current_variant_name;
                                        insertOrderItems += "(" + order.id + ", '" + order.transaction_id + "','" + (string.IsNullOrEmpty(items.current_product_sku_id.ToString()) ? items.id.ToString() : items.current_product_sku_id.ToString()) + "','" + items.category + "'," + items.category_id + ",'" + namaBrg.Replace('\'', '`') + "',";
                                        insertOrderItems += items.accepted_price + "," + items.weight + ",'" + items.desc + "','" + items.condition.Replace('\'', '`') + "'," + items.stock + "," + items.order_quantity + ",'" + order.created_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + order.updated_at.ToString("yyyy-MM-dd HH:mm:ss") + "','" + username + "','" + connectionID + "')";
                                        insertOrderItems += " ,";
                                    }
                                }

                                var tblKabKot = EDB.GetDataSet("MOConnectionString", "KabupatenKota", "SELECT TOP 1 * FROM KabupatenKota WHERE NamaKabKot LIKE '%" + order.consignee.city + "%'");
                                var tblProv = EDB.GetDataSet("MOConnectionString", "Provinsi", "SELECT TOP 1 * FROM Provinsi WHERE NamaProv LIKE '%" + order.consignee.province + "%'");

                                var kabKot = "3174";//set default value jika tidak ada di db
                                var prov = "31";//set default value jika tidak ada di db

                                if (tblProv.Tables[0].Rows.Count > 0)
                                    prov = tblProv.Tables[0].Rows[0]["KodeProv"].ToString();
                                if (tblKabKot.Tables[0].Rows.Count > 0)
                                    kabKot = tblKabKot.Tables[0].Rows[0]["KodeKabKot"].ToString();

                                //insertPembeli += "('" + order.buyer.name.Replace('\'', '`') + "','" + order.consignee.address.Replace('\'', '`') + "','" + order.consignee.phone + "','" + order.buyer.email.Replace('\'', '`') + "',0,0,'0','01',";
                                insertPembeli += "('" + nama + "','" + order.consignee.address.Replace('\'', '`') + "','" + order.consignee.phone + "','" + order.buyer.email.Replace('\'', '`') + "',0,0,'0','01',";
                                insertPembeli += "1, 'IDR', '01', '" + order.consignee.address.Replace('\'', '`') + "', 0, 0, 0, 0, '1', 0, 0, ";
                                insertPembeli += "'FP', '" + dtNow + "', '" + username + "', '" + order.consignee.post_code.Replace('\'', '`') + "', '" + order.buyer.email.Replace('\'', '`') + "', '" + kabKot + "', '" + prov + "', '" + order.consignee.city.Replace('\'', '`') + "', '" + order.consignee.province.Replace('\'', '`') + "', '" + connIDARF01C + "')";

                                //if (i < bindOrder.transactions.Length)
                                insertQ += " ,";
                                insertPembeli += " ,";

                                if (!OrderNoInDb.Contains(Convert.ToString(order.id)))
                                    jmlhNewOrder++;
                            }
                        }

                        //i = i + 1;
                    }
                    string errorMsg = "";
                    if (adaInsert)
                    {
                        insertQ = insertQ.Substring(0, insertQ.Length - 2);
                        var a = EDB.ExecuteSQL(username, CommandType.Text, insertQ);
                        if (a <= 0)
                        {
                            errorMsg = "failed to insert order to temp table;";
                        }

                        insertOrderItems = insertOrderItems.Substring(0, insertOrderItems.Length - 2);
                        a = EDB.ExecuteSQL(username, CommandType.Text, insertOrderItems);
                        if (a <= 0)
                        {
                            errorMsg += "failed to insert order item to temp table;";
                        }

                        insertPembeli = insertPembeli.Substring(0, insertPembeli.Length - 2);
                        a = EDB.ExecuteSQL(username, CommandType.Text, insertPembeli);
                        if (a <= 0)
                        {
                            errorMsg += "failed to insert pembeli to temp table;";
                        }
                        if (!string.IsNullOrEmpty(errorMsg))
                        {
                            throw new Exception(errorMsg);
                            //currentLog.REQUEST_EXCEPTION = errorMsg;
                            //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                        }
                        else
                        {
                            //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);
                        }

                        ret.status = 1;
                        ret.message = a.ToString();

#region call sp
                        SqlCommand CommandSQL = new SqlCommand();

                        //add by Tri call sp to insert buyer data
                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connIDARF01C;

                        EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);
                        //end add by Tri call sp to insert buyer data

                        CommandSQL = new SqlCommand();
                        CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;

                        CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = connectionID;
                        CommandSQL.Parameters.Add("@DR_TGL", SqlDbType.DateTime).Value = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd HH:mm:ss");
                        CommandSQL.Parameters.Add("@SD_TGL", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        CommandSQL.Parameters.Add("@Lazada", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@bukalapak", SqlDbType.Int).Value = 1;
                        CommandSQL.Parameters.Add("@Elevenia", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@Blibli", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@Tokped", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@Shopee", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@JD", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@82Cart", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@Shopify", SqlDbType.Int).Value = 0;
                        CommandSQL.Parameters.Add("@Cust", SqlDbType.VarChar, 50).Value = Cust;

                        EDB.ExecuteSQL("MOConnectionString", "MoveOrderFromTempTable", CommandSQL);
#endregion

                        if (jmlhNewOrder > 0)
                        {
                            var contextNotif = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<MasterOnline.Hubs.MasterOnlineHub>();
                            contextNotif.Clients.Group(dbPathEra).moNewOrder("Terdapat " + Convert.ToString(jmlhNewOrder) + " Pesanan baru dari Bukalapak.");

                            new StokControllerJob().updateStockMarketPlace(connectionID, dbPathEra, uname);
                        }
                    }
                    else
                    {
                        //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);
                    }
                }
                else
                {
                    ret.message = bindOrder.message;
                    throw new Exception(ret.message);
                    //currentLog.REQUEST_EXCEPTION = bindOrder.message;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                }
            }
            else
            {
                ret.message = "failed to call buka lapak api";
                throw new Exception(ret.message);
                //currentLog.REQUEST_EXCEPTION = ret.message;
                //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
            }


            return ret;
        }

        [HttpGet]
        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Konfirmasi Pengiriman Pesanan {obj} ke Bukalapak Gagal.")]
        public BindingBase KonfirmasiPengiriman(/*string noBukti,*/string dbPathEra, string namaPemesan, string log_CUST, string log_ActionCategory, string log_ActionName, string uname, string shipCode, string transId, string courier, string userId, string token)
        {
            var ret = new BindingBase();
            ret.status = 0;
            SetupContext(dbPathEra, uname);

            var data = new BindingShipBL
            {
                payment_shipping = new ShippingBukaLapak
                {
                    shipping_code = shipCode,
                    transaction_id = transId,
                }
            };
            if (courier.ToUpper().Contains("POS") || courier.ToUpper().Contains("TIKI") || courier.ToUpper().Contains("JNE"))
            {
                //tidak perlu ditambahkan nama courier
            }
            else
            {
                data.payment_shipping.new_courier = courier;
            }
            //MasterOnline.API_LOG_MARKETPLACE currentLog = new API_LOG_MARKETPLACE
            //{
            //    REQUEST_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //    REQUEST_ACTION = "Confirm Shipment",
            //    REQUEST_DATETIME = DateTime.Now,
            //    REQUEST_ATTRIBUTE_1 = token,
            //    REQUEST_ATTRIBUTE_2 = shipCode,
            //    REQUEST_ATTRIBUTE_3 = transId,
            //    REQUEST_STATUS = "Pending",
            //};
            //manageAPI_LOG_MARKETPLACE(api_status.Pending, ErasoftDbContext, userId, currentLog);

            string dataPost = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            Utils.HttpRequest req = new Utils.HttpRequest();
            var bindStatus = req.CallBukaLapakAPI("POST", "transactions/confirm_shipping.json", dataPost, userId, token, typeof(BukaLapakRes)) as BukaLapakRes;
            if (bindStatus != null)
            {
                if (bindStatus.status.Equals("OK"))
                {
                    //string username = sessionData.Account.Username;
                    //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                    ret.status = 1;
                    //change status menjadi  04 => shipped
                    //EDB.ExecuteSQL("", CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '04' WHERE NO_REFERENSI = '" + noBukti + "'");
                    //manageAPI_LOG_MARKETPLACE(api_status.Success, ErasoftDbContext, userId, currentLog);

                }
                else
                {
                    throw new Exception(bindStatus.message);
                    //ret.message = bindStatus.message;
                    //currentLog.REQUEST_EXCEPTION = bindStatus.message;
                    //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
                }
            }
            else
            {
                throw new Exception("failed to call Buka Lapak api");
                //ret.message = "failed to call Buka Lapak api";
                //currentLog.REQUEST_EXCEPTION = ret.message;
                //manageAPI_LOG_MARKETPLACE(api_status.Failed, ErasoftDbContext, userId, currentLog);
            }

            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Status Accept Pesanan {obj} ke BukaLapak Gagal.")]
        public async Task<BindingBase> Bukalapak_AcceptOrder(string DatabasePathErasoft, string namaPemesan, string log_CUST, string log_ActionCategory, string log_ActionName, BukaLapakKey data, string noref, string username)
        {
            SetupContext(DatabasePathErasoft, username);
            data = new BukaLapakControllerJob().RefreshToken(data);
            var ret = new BindingBase();
            string transid = noref.Substring(2, noref.Length - 2);

            string urll = "https://api.bukalapak.com/transactions/" + transid + "/status";

            string myData = "{\"state\":\"accepted\"}";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "PUT";
            myReq.Headers.Add("Authorization", "Bearer " + data.token);
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
            if (responseFromServer != "")
            {
                var resp = JsonConvert.DeserializeObject(responseFromServer, typeof(ChangeOrderStatusResponse)) as ChangeOrderStatusResponse;
                if (resp != null)
                {
                    if (resp.meta != null)
                    {
                        if (resp.meta.http_status != 200)
                        {
                            if (resp.errors != null)
                            {
                                if (resp.errors.Length > 0)
                                {
                                    string errMsg = "";
                                    foreach (var error in resp.errors)
                                    {
                                        errMsg += error.code + ":" + error.message + "\n";
                                    }
                                    throw new Exception(errMsg);
                                }
                            }
                            throw new Exception(responseFromServer);
                        }
                        else
                        {
#region set pembeli
                            if (!resp.data.delivery.consignee.phone.Contains("Terkunci"))
                            {
                                var ordID = resp.data.transaction_id;
                                var currentOrder = ErasoftDbContext.SOT01A.Where(m => m.CUST == log_CUST && m.NO_REFERENSI == ordID).FirstOrDefault();
                                if (currentOrder != null)
                                {
                                    if (string.IsNullOrEmpty(currentOrder.PEMESAN))
                                    {
                                        var pembeli = ErasoftDbContext.ARF01C.Where(m => m.TLP == resp.data.delivery.consignee.phone).FirstOrDefault();
                                        if (pembeli == null)
                                        {
                                            string conn_id = Guid.NewGuid().ToString();
                                            string insertPembeli = "INSERT INTO TEMP_ARF01C (NAMA, AL, TLP, PERSO, TERM, LIMIT, PKP, KLINK, ";
                                            insertPembeli += "KODE_CABANG, VLT, KDHARGA, AL_KIRIM1, DISC_NOTA, NDISC_NOTA, DISC_ITEM, NDISC_ITEM, STATUS, LABA, TIDAK_HIT_UANG_R, ";
                                            insertPembeli += "No_Seri_Pajak, TGL_INPUT, USERNAME, KODEPOS, EMAIL, KODEKABKOT, KODEPROV, NAMA_KABKOT, NAMA_PROV, CONNECTION_ID) VALUES ";

                                            var kabKot = "3174";//set default value jika tidak ada di db
                                            var prov = "31";//set default value jika tidak ada di db
#region cut max length pembeli
                                            var nama = resp.data.buyer.name.Replace('\'', '`');
                                            if (nama.Length > 30)
                                                nama = nama.Substring(0, 30);
                                            string tlp = !string.IsNullOrEmpty(resp.data.delivery.consignee.phone) ? resp.data.delivery.consignee.phone.Replace('\'', '`') : "";
                                            if (tlp.Length > 30)
                                            {
                                                tlp = tlp.Substring(0, 30);
                                            }
                                            string AL_KIRIM1 = !string.IsNullOrEmpty(resp.data.delivery.consignee.address) ? resp.data.delivery.consignee.address.Replace('\'', '`') : "";
                                            if (AL_KIRIM1.Length > 30)
                                            {
                                                AL_KIRIM1 = AL_KIRIM1.Substring(0, 30);
                                            }
                                            string KODEPOS = !string.IsNullOrEmpty(resp.data.delivery.consignee.postal_code) ? resp.data.delivery.consignee.postal_code.Replace('\'', '`') : "";
                                            if (KODEPOS.Length > 7)
                                            {
                                                KODEPOS = KODEPOS.Substring(0, 7);
                                            }

                                            string namaKabkot = (string.IsNullOrEmpty(resp.data.delivery.consignee.city) ? "" : resp.data.delivery.consignee.city.Replace("'", "`"));
                                            if (namaKabkot.Length > 50)
                                                namaKabkot = namaKabkot.Substring(0, 50);

                                            string namaProv = string.IsNullOrEmpty(resp.data.delivery.consignee.province) ? "" : resp.data.delivery.consignee.province.Replace("'", "`");
                                            if (namaProv.Length > 50)
                                                namaProv = namaProv.Substring(0, 50);
#endregion
                                            insertPembeli += "('" + nama + "','" + resp.data.delivery.consignee.address.Replace('\'', '`') + "','" + tlp + "','',0,0,'0','01',";
                                            insertPembeli += "1, 'IDR', '01', '" + AL_KIRIM1.Replace('\'', '`') + "', 0, 0, 0, 0, '1', 0, 0, ";
                                            insertPembeli += "'FP', '" + DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm:ss") + "', '" + username + "', '"
                                                + KODEPOS + "', '', '" + kabKot + "', '" + prov + "', '" + namaKabkot
                                                + "', '" + namaProv.Replace('\'', '`') + "', '" + conn_id + "')";

                                            EDB.ExecuteSQL("MOConnectionString", CommandType.Text, insertPembeli);

                                            SqlCommand CommandSQL = new SqlCommand();
                                            CommandSQL.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                                            CommandSQL.Parameters.Add("@Conn_id", SqlDbType.VarChar, 50).Value = conn_id;

                                            EDB.ExecuteSQL("MOConnectionString", "MoveARF01CFromTempTable", CommandSQL);

                                            pembeli = ErasoftDbContext.ARF01C.Where(m => m.TLP == resp.data.delivery.consignee.phone).FirstOrDefault();
                                        }
                                        //var pembeli = ErasoftDbContext.ARF01C.Where(m => m.TLP == resp.data.delivery.consignee.phone).FirstOrDefault();
                                        if (pembeli != null)
                                        {
                                            EDB.ExecuteSQL("MOConnectionString", CommandType.Text, "UPDATE SOT01A SET PEMESAN = '" + pembeli.BUYER_CODE + "' WHERE NO_BUKTI = '" + currentOrder.NO_BUKTI + "'");
                                        }
                                    }
                                }
                            }
#endregion
                        }
                    }
                }
            }
            return ret;
        }

        [AutomaticRetry(Attempts = 3)]
        [Queue("1_manage_pesanan")]
        [NotifyOnFailed("Update Status Cancel Pesanan {obj} ke BukaLapak Gagal.")]
        public async Task<BindingBase> Bukalapak_CancelOrder(string DatabasePathErasoft, string namaPemesan, string log_CUST, string log_ActionCategory, string log_ActionName, BukaLapakKey data, string noref, string username, string cancelReason)
        {
            SetupContext(DatabasePathErasoft, username);
            data = new BukaLapakControllerJob().RefreshToken(data);
            var ret = new BindingBase();

            string transid = noref.Substring(2, noref.Length - 2);
            string urll = "https://api.bukalapak.com/transactions/" + transid + "/status";

            string myData = "{\"state\":\"rejected\", \"state_options\": {\"reject_reason\":  \"" + cancelReason + "\" } }";

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(urll);
            myReq.Method = "PUT";
            myReq.Headers.Add("Authorization", "Bearer " + data.token);
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
            if (responseFromServer != "")
            {
                var resp = JsonConvert.DeserializeObject(responseFromServer, typeof(ChangeOrderStatusResponse)) as ChangeOrderStatusResponse;
                if (resp != null)
                {
                    if (resp.meta != null)
                    {
                        if (resp.meta.http_status != 200)
                        {
                            if (resp.errors != null)
                            {
                                if (resp.errors.Length > 0)
                                {
                                    string errMsg = "";
                                    foreach (var error in resp.errors)
                                    {
                                        errMsg += error.code + ":" + error.message + "\n";
                                    }
                                    throw new Exception(errMsg);
                                }
                            }
                            throw new Exception(responseFromServer);
                        }
                        
                    }
                }
            }
            return ret;
        }
        public BindingBase getListProduct(string cust, string userId, string token, int page, bool display, int recordCount)
        {
            var ret = new BindingBase();
            ret.status = 0;
            ret.recordCount = recordCount;

            Utils.HttpRequest req = new Utils.HttpRequest();
            string nonaktifUrl = "&not_for_sale_only=1";
            ProdBL resListProd = req.CallBukaLapakAPI("", "products/mylapak.json?page=" + page + "&per_page=10" + (display ? "" : nonaktifUrl), "", userId, token, typeof(ProdBL)) as ProdBL;
            if (resListProd != null)
            {
                if (resListProd.status.Equals("OK") && resListProd.products != null)
                {
                    if (resListProd.products.Count == 0)
                    {
                        if (display)
                        {
                            ret.status = 1;
                            ret.message = "MOVE_TO_INACTIVE_PRODUCTS";
                        }
                        else
                        {
                            return ret;
                        }

                    }
                    ret.status = 1;
                    if (resListProd.products.Count == 10)
                    {
                        ret.message = (page + 1).ToString();
                        if (!display)
                            ret.message = "MOVE_TO_INACTIVE_PRODUCTS";
                    }
                    else
                    {
                        if (display)
                            ret.message = "MOVE_TO_INACTIVE_PRODUCTS";
                    }
                    int IdMarket = ErasoftDbContext.ARF01.Where(c => c.CUST.Equals(cust)).FirstOrDefault().RecNum.Value;
                    var stf02h_local = ErasoftDbContext.STF02H.Where(m => m.IDMARKET == IdMarket).ToList();
                    var tempBrg_local = ErasoftDbContext.TEMP_BRG_MP.Where(m => m.IDMARKET == IdMarket).ToList();

                    string sSQL = "INSERT INTO TEMP_BRG_MP (BRG_MP, SELLER_SKU, NAMA, NAMA2, NAMA3, BERAT, PANJANG, LEBAR, TINGGI, CUST, ";
                    sSQL += "Deskripsi, IDMARKET, HJUAL, HJUAL_MP, DISPLAY, CATEGORY_CODE, CATEGORY_NAME, MEREK, IMAGE, IMAGE2, IMAGE3, KODE_BRG_INDUK, TYPE";
                    //sSQL += ", ACODE_1, ANAME_1, AVALUE_1, ACODE_2, ANAME_2, AVALUE_2, ACODE_3, ANAME_3, AVALUE_3, ACODE_4, ANAME_4, AVALUE_4, ACODE_5, ANAME_5, AVALUE_5, ACODE_6, ANAME_6, AVALUE_6, ACODE_7, ANAME_7, AVALUE_7, ACODE_8, ANAME_8, AVALUE_8, ACODE_9, ANAME_9, AVALUE_9, ACODE_10, ANAME_10, AVALUE_10, ";
                    //sSQL += "ACODE_11, ANAME_11, AVALUE_11, ACODE_12, ANAME_12, AVALUE_12, ACODE_13, ANAME_13, AVALUE_13, ACODE_14, ANAME_14, AVALUE_14, ACODE_15, ANAME_15, AVALUE_15, ACODE_16, ANAME_16, AVALUE_16, ACODE_17, ANAME_17, AVALUE_17, ACODE_18, ANAME_18, AVALUE_18, ACODE_19, ANAME_19, AVALUE_19, ACODE_20, ANAME_20, AVALUE_20, ";
                    //sSQL += "ACODE_21, ANAME_21, AVALUE_21, ACODE_22, ANAME_22, AVALUE_22, ACODE_23, ANAME_23, AVALUE_23, ACODE_24, ANAME_24, AVALUE_24, ACODE_25, ANAME_25, AVALUE_25, ACODE_26, ANAME_26, AVALUE_26, ACODE_27, ANAME_27, AVALUE_27, ACODE_28, ANAME_28, AVALUE_28, ACODE_29, ANAME_29, AVALUE_29, ACODE_30, ANAME_30, AVALUE_30, ";
                    //sSQL += "ACODE_31, ANAME_31, AVALUE_31, ACODE_32, ANAME_32, AVALUE_32, ACODE_33, ANAME_33, AVALUE_33, ACODE_34, ANAME_34, AVALUE_34, ACODE_35, ANAME_35, AVALUE_35, ACODE_36, ANAME_36, AVALUE_36, ACODE_37, ANAME_37, AVALUE_37, ACODE_38, ANAME_38, AVALUE_38, ACODE_39, ANAME_39, AVALUE_39, ACODE_40, ANAME_40, AVALUE_40, ";
                    //sSQL += "ACODE_41, ANAME_41, AVALUE_41, ACODE_42, ANAME_42, AVALUE_42, ACODE_43, ANAME_43, AVALUE_43, ACODE_44, ANAME_44, AVALUE_44, ACODE_45, ANAME_45, AVALUE_45, ACODE_46, ANAME_46, AVALUE_46, ACODE_47, ANAME_47, AVALUE_47, ACODE_48, ANAME_48, AVALUE_48, ACODE_49, ANAME_49, AVALUE_49, ACODE_50, ANAME_50, AVALUE_50) VALUES ";
                    sSQL += ") VALUES ";
                    string sSQL_Value = "";
                    foreach (var brg in resListProd.products)
                    {
                        bool haveVarian = false;
                        string kdBrgInduk = "";
                        if (brg.product_sku.Count > 0)
                        {
                            haveVarian = true;
                            kdBrgInduk = brg.id;
                            var tempbrginDBInduk = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == kdBrgInduk.ToUpper()).FirstOrDefault();
                            var brgInDBInduk = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == kdBrgInduk.ToUpper()).FirstOrDefault();
                            if (tempbrginDBInduk == null && brgInDBInduk == null)
                            {
                                var insert1 = CreateTempQry(brg, cust, IdMarket, display, 1, "", 0);
                                if (insert1.status == 1)
                                    sSQL_Value += insert1.message;
                            }
                            else if (brgInDBInduk != null)
                            {
                                kdBrgInduk = brgInDBInduk.BRG;
                            }
                        }
                        //var tempbrginDB = ErasoftDbContext.TEMP_BRG_MP.Where(t => t.BRG_MP.ToUpper().Equals(brg.id.ToUpper()) && t.IDMARKET == IdMarket).FirstOrDefault();
                        //var brgInDB = ErasoftDbContext.STF02H.Where(t => t.BRG_MP.ToUpper().Equals(brg.id.ToUpper()) && t.IDMARKET == IdMarket).FirstOrDefault();
                        var tempbrginDB = tempBrg_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brg.id.ToUpper()).FirstOrDefault();
                        var brgInDB = stf02h_local.Where(t => (t.BRG_MP == null ? "" : t.BRG_MP).ToUpper() == brg.id.ToUpper()).FirstOrDefault();
                        if (tempbrginDB == null && brgInDB == null)
                        {
#region remark
                            //ret.recordCount++;
                            //string nama, nama2, nama3, urlImage, urlImage2, urlImage3;
                            //urlImage = "";
                            //urlImage2 = "";
                            //urlImage3 = "";
                            //if (brg.name.Length > 30)
                            //{
                            //    nama = brg.name.Substring(0, 30);
                            //    //change by calvin 15 januari 2019
                            //    //if (brg.name.Length > 60)
                            //    //{
                            //    //    nama2 = brg.name.Substring(30, 30);
                            //    //    nama3 = (brg.name.Length > 90) ? brg.name.Substring(60, 30) : brg.name.Substring(60);
                            //    //}
                            //    if (brg.name.Length > 285)
                            //    {
                            //        nama2 = brg.name.Substring(30, 255);
                            //        nama3 = "";
                            //    }
                            //    //end change by calvin 15 januari 2019
                            //    else
                            //    {
                            //        nama2 = brg.name.Substring(30);
                            //        nama3 = "";
                            //    }
                            //}
                            //else
                            //{
                            //    nama = brg.name;
                            //    nama2 = "";
                            //    nama3 = "";
                            //}

                            //if (brg.images != null)
                            //{
                            //    urlImage = brg.images[0];
                            //    if (brg.images.Length >= 2)
                            //    {
                            //        urlImage2 = brg.images[1];
                            //        if (brg.images.Length >= 3)
                            //        {
                            //            urlImage3 = brg.images[2];
                            //        }
                            //    }
                            //}

                            //sSQL_Value += "('" + brg.id + "' , '" + brg.id + "' , '";
                            ////if (brg.name.Length > 30)
                            ////{
                            ////    sSQL += brg.name.Substring(0, 30) + "' , '" + brg.name.Substring(30) + "' , ";
                            ////}
                            ////else
                            ////{
                            ////    sSQL += brg.name + "' , '' , ";
                            ////}
                            //sSQL_Value += nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' , '" + nama3.Replace('\'', '`') + "' ,";
                            //sSQL_Value += brg.weight + " , 1, 1, 1, '" + cust + "' , '" + brg.desc.Replace("<br/>", "\r\n").Replace("<br />", "\r\n").Replace('\'', '`') + "' , " + ErasoftDbContext.ARF01.Where(c => c.CUST.Equals(cust)).FirstOrDefault().RecNum;
                            //sSQL_Value += " , " + brg.price + " , " + brg.price + " , " + (display ? "1" : "0") + ", '";
                            //sSQL_Value += brg.category_id + "' , '" + brg.category + "' , '" + (string.IsNullOrEmpty(brg.specs.merek) ? brg.specs.brand : brg.specs.merek);
                            //sSQL_Value += "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "') ,";
#endregion
                            if (haveVarian)
                            {
                                for (int i = 0; i < brg.product_sku.Count; i++)
                                {
                                    var insert2 = CreateTempQry(brg, cust, IdMarket, display, 2, kdBrgInduk, i);
                                    if (insert2.status == 1)
                                        sSQL_Value += insert2.message;
                                }
                            }
                            else
                            {
                                var insert2 = CreateTempQry(brg, cust, IdMarket, display, 0, "", 0);
                                if (insert2.status == 1)
                                    sSQL_Value += insert2.message;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(sSQL_Value))
                    {
                        sSQL = sSQL + sSQL_Value;
                        sSQL = sSQL.Substring(0, sSQL.Length - 1);
                        var a = EDB.ExecuteSQL("CString", CommandType.Text, sSQL);
                        ret.recordCount += a;
                    }

                }
                else
                {
                    ret.message = resListProd.message;
                }
            }
            else
            {
                ret.message = "failed to call Buka Lapak api";
            }

            return ret;
        }

        public BindingBase CreateTempQry(ListProduct brg, string cust, int idMarket, bool display, int type, string kdBrgInduk, int i)
        {
            // typeBrg : 0 = barang tanpa varian; 1 = barang induk; 2 = barang varian
            var ret = new BindingBase();
            ret.status = 0;
            string sSQL_Value = "";
            try
            {
                string nama, nama2, nama3, urlImage, urlImage2, urlImage3;
                urlImage = "";
                urlImage2 = "";
                urlImage3 = "";
                string namaBrg = brg.name;
                long itemPrice = brg.price;
                if (type == 2)
                {
                    namaBrg += " " + brg.product_sku[i].variant_name;
                    itemPrice = brg.product_sku[i].price;
                }
                if (namaBrg.Length > 30)
                {
                    nama = namaBrg.Substring(0, 30);
                    //change by calvin 15 januari 2019
                    //if (brg.name.Length > 60)
                    //{
                    //    nama2 = brg.name.Substring(30, 30);
                    //    nama3 = (brg.name.Length > 90) ? brg.name.Substring(60, 30) : brg.name.Substring(60);
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

                if (brg.images != null)
                {
                    if (type != 2)
                    {
                        urlImage = brg.images[0];
                        if (brg.images.Length >= 2)
                        {
                            urlImage2 = brg.images[1];
                            if (brg.images.Length >= 3)
                            {
                                urlImage3 = brg.images[2];
                            }
                        }
                    }
                    else
                    {
                        if (brg.product_sku[i].images != null)
                        {
                            urlImage = brg.product_sku[i].images[0];
                            if (brg.product_sku[i].images.Length >= 2)
                            {
                                urlImage2 = brg.product_sku[i].images[1];
                                if (brg.product_sku[i].images.Length >= 3)
                                {
                                    urlImage3 = brg.product_sku[i].images[2];
                                }
                            }
                        }
                    }

                }
                if (type != 2)
                {
                    sSQL_Value += "('" + brg.id + "' , '" + brg.id + "' , '";
                }
                else
                {
                    sSQL_Value += "('" + brg.product_sku[i].id + "' , '" + brg.product_sku[i].sku_name + "' , '";
                }
                string brand = "";
                if (brg.specs != null)
                {
                    brand = brg.specs.merek;
                    if (string.IsNullOrEmpty(brand))
                    {
                        brand = brg.specs.brand;
                    }
                }

                sSQL_Value += nama.Replace('\'', '`') + "' , '" + nama2.Replace('\'', '`') + "' , '" + nama3.Replace('\'', '`') + "' ,";
                sSQL_Value += brg.weight + " , 1, 1, 1, '" + cust + "' , '" + brg.desc.Replace("<br/>", "\r\n").Replace("<br />", "\r\n").Replace('\'', '`') + "' , " + idMarket;
                sSQL_Value += " , " + itemPrice + " , " + itemPrice + " , " + (display ? "1" : "0") + ", '";
                sSQL_Value += brg.category_id + "' , '" + brg.category + "' , '" + brand;
                sSQL_Value += "' , '" + urlImage + "' , '" + urlImage2 + "' , '" + urlImage3 + "','";
                sSQL_Value += (type == 2 ? kdBrgInduk : "") + "','" + (type == 1 ? "4" : "3") + "') ,";
                ret.status = 1;
                ret.message = sSQL_Value;
            }
            catch (Exception ex)
            {
                ret.message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return ret;
        }
        [HttpGet]
        public BindingBase CancelOrder(string noBukti, string transId, string userId, string token)
        {
            var ret = new BindingBase();
            string Myprod = "{ \"data\": { \"id\":\"" + transId + "\", \"payment_rejection[reason]\":\"Stok Habis\" } }";
            /* REASON (case sensitive) :
             * Stok Habis
             * Harga barang/biaya kirim tidak sesuai
             * Ada kesibukan lain yang sifatnya mendadak
             * Permintaan pembeli tidak dapat dilayani
            */
            Utils.HttpRequest req = new Utils.HttpRequest();
            var bindCancel = req.CallBukaLapakAPI("PUT", "transactions/reject.json", Myprod, userId, token, typeof(BukaLapakRes)) as BukaLapakRes;
            if (bindCancel != null)
            {
                if (bindCancel.status.Equals("OK"))
                {
                    //string username = sessionData.Account.Username;
                    //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                    ret.status = 1;
                    //change status menjadi  11 => cancelled
                    //EDB.ExecuteSQL("", CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI = '" + noBukti + "'");
                }
                else
                {
                    ret.message = bindCancel.message;
                }
            }
            else
            {
                ret.message = "failed to call Buka Lapak api";
            }

            return ret;
        }

        public BindingBase GetCategoryBL(string noBukti, string transId, string userId, string token)
        {
            var ret = new BindingBase();
            string Myprod = "{ \"data\": { \"id\":\"" + transId + "\", \"payment_rejection[reason]\":\"Stok Habis\" } }";
            /* REASON (case sensitive) :
             * Stok Habis
             * Harga barang/biaya kirim tidak sesuai
             * Ada kesibukan lain yang sifatnya mendadak
             * Permintaan pembeli tidak dapat dilayani
            */
            Utils.HttpRequest req = new Utils.HttpRequest();
            var bindCancel = req.CallBukaLapakAPI("PUT", "transactions/reject.json", Myprod, userId, token, typeof(BukaLapakRes)) as BukaLapakRes;
            if (bindCancel != null)
            {
                if (bindCancel.status.Equals("OK"))
                {
                    //string username = sessionData.Account.Username;
                    //DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
                    ret.status = 1;
                    //change status menjadi  11 => cancelled
                    //EDB.ExecuteSQL("", CommandType.Text, "UPDATE SOT01A SET STATUS_TRANSAKSI = '11' WHERE NO_REFERENSI = '" + noBukti + "'");
                }
                else
                {
                    ret.message = bindCancel.message;
                }
            }
            else
            {
                ret.message = "failed to call Buka Lapak api";
            }

            return ret;
        }

        public enum api_status
        {
            Pending = 1,
            Success = 2,
            Failed = 3,
            Exception = 4
        }
        public void manageAPI_LOG_MARKETPLACE(api_status action, ErasoftContext db, string iden, API_LOG_MARKETPLACE data)
        {
            switch (action)
            {
                case api_status.Pending:
                    {
                        var arf01 = ErasoftDbContext.ARF01.Where(p => p.API_KEY == iden).FirstOrDefault();
                        var apiLog = new MasterOnline.API_LOG_MARKETPLACE
                        {
                            CUST = arf01 != null ? arf01.CUST : "",
                            CUST_ATTRIBUTE_1 = arf01 != null ? arf01.PERSO : "",
                            CUST_ATTRIBUTE_2 = data.CUST_ATTRIBUTE_2 != null ? data.CUST_ATTRIBUTE_2 : "",
                            CUST_ATTRIBUTE_3 = data.CUST_ATTRIBUTE_3 != null ? data.CUST_ATTRIBUTE_3 : "",
                            CUST_ATTRIBUTE_4 = data.CUST_ATTRIBUTE_4 != null ? data.CUST_ATTRIBUTE_4 : "",
                            CUST_ATTRIBUTE_5 = data.CUST_ATTRIBUTE_5 != null ? data.CUST_ATTRIBUTE_5 : "",
                            MARKETPLACE = "Buka Lapak",
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
    }

#region get order response
    public class SetOrderCourrierResponse : BLErrorResponse
    {
        public SetCourrierDatum data { get; set; }
        public GetOrdersMeta meta { get; set; }

    }
    public class SetCourrierDatum
    {
        public long id { get; set; }
        public string transaction_id { get; set; }
        public long shipping_id { get; set; }
        public string courier { get; set; }
        public string courier_name { get; set; }
        public string booking_code { get; set; }
        public string partner_label_url { get; set; }
    }
    public class ChangeOrderStatusResponse : BLErrorResponse
    {
        public GetOrdersDatum data { get; set; }
        public GetOrdersMeta meta { get; set; }
    }
    public class GetOrdersResponse : BLErrorResponse
    {
        public GetOrdersDatum[] data { get; set; }
        public GetOrdersMeta meta { get; set; }
    }

    public class GetOrdersMeta
    {
        public int http_status { get; set; }
    }

    public class GetOrdersDatum
    {
        public long id { get; set; }
        public string type { get; set; }
        public long invoice_id { get; set; }
        public string transaction_id { get; set; }
        public string state { get; set; }
        public string payment_method { get; set; }
        public State_Changed_At state_changed_at { get; set; }
        //public State_Changed_By state_changed_by { get; set; }
        public bool actionable { get; set; }
        public string created_on { get; set; }
        public GetOrdersItem[] items { get; set; }
        public GetOrdersBuyer buyer { get; set; }
        //public GetOrdersStore store { get; set; }
        public GetOrdersAmount amount { get; set; }
        public GetOrdersCashback[] cashback { get; set; }
        public GetOrdersDelivery delivery { get; set; }
        public GetOrdersDropship dropship { get; set; }
        //public GetOrdersFeedback feedback { get; set; }
        public GetOrdersOptions options { get; set; }
        public bool on_hold { get; set; }
        public bool deal { get; set; }
        //public long claim_id { get; set; }
        public GetOrdersSla sla { get; set; }
        //public DateTime last_printed_at { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        //public GetOrdersPromotion promotion { get; set; }
        //public string virtual_transaction_serial_number { get; set; }
        public bool can_claim_assurance { get; set; }
        //public int merchant_return_insurance_id { get; set; }
    }

    public class State_Changed_At
    {
        public DateTime pending_at { get; set; }
        //public DateTime paid_at { get; set; }
        //public DateTime accepted_at { get; set; }
        //public DateTime rejected_at { get; set; }
        //public DateTime cancelled_at { get; set; }
        public DateTime? delivered_at { get; set; }
        //public DateTime expired_at { get; set; }
        //public DateTime received_at { get; set; }
        //public DateTime remitted_at { get; set; }
        public DateTime refund_at { get; set; }
        //public DateTime refunded_at { get; set; }
    }

    public class State_Changed_By
    {
        //public long cancelled_by { get; set; }
    }

    public class GetOrdersBuyer
    {
        public long id { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string avatar { get; set; }
    }

    public class GetOrdersStore
    {
        public long id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string avatar { get; set; }
        public long address_id { get; set; }
        public bool brand { get; set; }
        public bool official { get; set; }
    }

    public class GetOrdersAmount
    {
        public GetOrdersBuyer1 buyer { get; set; }
        public GetOrdersSeller seller { get; set; }
    }

    public class GetOrdersBuyer1
    {
        public double total { get; set; }
        public double payment_amount { get; set; }
        public double refund_amount { get; set; }
        public double coded_amount { get; set; }
        public GetOrdersDetails details { get; set; }
    }

    public class GetOrdersDetails
    {
        public double item { get; set; }
        public double delivery { get; set; }
        public double insurance { get; set; }
        //public double axinan_insurance_amount { get; set; }
        //public double logistic_insurance_amount { get; set; }
        //public double gadget_insurance_amount { get; set; }
        //public double goods_insurance_amount { get; set; }
        //public double cosmetic_insurance_amount { get; set; }
        //public double fmcg_insurance_amount { get; set; }
        //public double return_insurance_amount { get; set; }
        public double administration { get; set; }
        public double tipping_amount { get; set; }
        public double negotiation { get; set; }
        public double vat { get; set; }
        public double flash_deal_discount { get; set; }
        public double priority_buyer { get; set; }
        public double voucher_discount { get; set; }
        public double service_fee_dynamic_charging { get; set; }
        public double retarget_discount_amount { get; set; }
    }

    public class GetOrdersSeller
    {
        public double total { get; set; }
        public GetOrdersDetails1 details { get; set; }
    }

    public class GetOrdersDetails1
    {
        public double items { get; set; }
        public double delivery { get; set; }
        public double insurance { get; set; }
        public GetOrdersShipping_Reductions[] shipping_reductions { get; set; }
        public GetOrdersReduction[] reductions { get; set; }
        public GetOrdersSuper_Seller[] super_seller { get; set; }
    }

    public class GetOrdersShipping_Reductions
    {
        public string name { get; set; }
        public double amount { get; set; }
        public string description { get; set; }
        public bool remit_excluded { get; set; }
    }

    public class GetOrdersReduction
    {
        public string name { get; set; }
        public double amount { get; set; }
        public string description { get; set; }
    }

    public class GetOrdersSuper_Seller
    {
        public string name { get; set; }
        public double amount { get; set; }
    }

    public class GetOrdersDelivery
    {
        public GetOrdersConsignee consignee { get; set; }
        public string tracking_number { get; set; }
        public string requested_carrier { get; set; }
        public string carrier { get; set; }
        public bool white_label_courier { get; set; }
        public GetOrdersHistory[] history { get; set; }
        public long? id { get; set; }
        public bool force_awb { get; set; }
        public bool force_find_driver { get; set; }
        //public string shipping_receipt_state { get; set; }
        public bool allow_different_courier { get; set; }
        public bool allow_manual_receipt_voucher { get; set; }
        public double manual_switch_fee { get; set; }
        public bool allow_redeliver { get; set; }
        //public GetOrdersBooking booking { get; set; }
        //public GetOrdersPickup_Time pickup_time { get; set; }
        public bool force_awb_voucher { get; set; }
        //public DateTime estimated_received_at { get; set; }
        //public GetOrdersConvenience_Store convenience_store { get; set; }
        //public GetOrdersAvailable_Shipping_Service available_shipping_service { get; set; }
        public string buyer_logistic_choice { get; set; }
        //public bool receipt_validity { get; set; }
    }

    public class GetOrdersConsignee
    {
        public string name { get; set; }
        public string phone { get; set; }
        public string country { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string district { get; set; }
        public string address { get; set; }
        public string postal_code { get; set; }
        //public float latitude { get; set; }
        //public float longitude { get; set; }
    }

    public class GetOrdersBooking
    {
        public long id { get; set; }
        public string booking_code { get; set; }
        public string state { get; set; }
        public bool invoicing { get; set; }
        public GetOrdersDriver driver { get; set; }
        //public DateTime created_at { get; set; }
    }

    public class GetOrdersDriver
    {
        public string name { get; set; }
        public string phone { get; set; }
        public string pin { get; set; }
        public string photo { get; set; }
        public string live_tracking { get; set; }
    }

    //public class GetOrdersPickup_Time
    //{
    //    public DateTime from { get; set; }
    //    public DateTime to { get; set; }
    //}

    public class GetOrdersConvenience_Store
    {
        public string id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        //public GetOrdersCoordinate coordinate { get; set; }
        public string unique_code { get; set; }
    }

    //public class GetOrdersCoordinate
    //{
    //    public float latitude { get; set; }
    //    public float longitude { get; set; }
    //}

    public class GetOrdersAvailable_Shipping_Service
    {
        public string type { get; set; }
        public string name { get; set; }
    }

    public class GetOrdersHistory
    {
        public DateTime date { get; set; }
        public string status { get; set; }
    }

    public class GetOrdersDropship
    {
        public string name { get; set; }
        public string note { get; set; }
    }

    public class GetOrdersFeedback
    {
        public GetOrdersStore1 store { get; set; }
        public GetOrdersBuyer2 buyer { get; set; }
    }

    public class GetOrdersStore1
    {
        public long id { get; set; }
        public string content { get; set; }
        public bool positive { get; set; }
        public bool editable { get; set; }
        public GetOrdersReply[] replies { get; set; }
    }

    public class GetOrdersReply
    {
        public long sender_id { get; set; }
        public string content { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class GetOrdersBuyer2
    {
        public long id { get; set; }
        public string content { get; set; }
        public bool positive { get; set; }
        public bool editable { get; set; }
        public GetOrdersReply1[] replies { get; set; }
    }

    public class GetOrdersReply1
    {
        public long sender_id { get; set; }
        public string content { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class GetOrdersOptions
    {
        public string buyer_note { get; set; }
        public string reject_reason { get; set; }
        public string cancel_reason { get; set; }
        public string cancel_notes { get; set; }
        //public DateTime cancel_request_at { get; set; }
        public string reject_cancel_reason { get; set; }
        public string reject_cancel_notes { get; set; }
    }

    public class GetOrdersSla
    {
        public string type { get; set; }
        public int value { get; set; }
    }

    public class GetOrdersPromotion
    {
        public bool promoted_push { get; set; }
        public bool push { get; set; }
        public bool voucher { get; set; }
    }

    public class GetOrdersItem
    {
        public long id { get; set; }
        public string name { get; set; }
        public double price { get; set; }
        public double total_price { get; set; }
        public double flash_deal_discount { get; set; }
        public int quantity { get; set; }
        public GetOrdersCategory category { get; set; }
        public double agent_commission { get; set; }
        public GetOrdersStuff stuff { get; set; }
    }

    public class GetOrdersCategory
    {
        public string name { get; set; }
    }

    public class GetOrdersStuff
    {
        public string reference_type { get; set; }
        public long id { get; set; }
        public GetOrdersProduct product { get; set; }
        //public GetOrdersStore2 store { get; set; }
        public long price { get; set; }
        public GetOrdersImage image { get; set; }
        public string variant_name { get; set; }
        public string sku_name { get; set; }
        public double discount { get; set; }
        public GetOrdersUnit[] units { get; set; }
    }

    public class GetOrdersProduct
    {
        public string id { get; set; }
        public double price { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string condition { get; set; }
        public double weight { get; set; }
        public GetOrdersShipping shipping { get; set; }
        public bool assurance { get; set; }
        public string url { get; set; }
        public string _operator { get; set; }
        public double nominal { get; set; }
        public string partner { get; set; }
    }

    public class GetOrdersShipping
    {
        public bool force_insurance { get; set; }
        //public object[] free_shipping_coverage { get; set; }
    }

    public class GetOrdersStore2
    {
        public long id { get; set; }
        public string name { get; set; }
        public string term_and_condition { get; set; }
        //public DateTime term_and_condition_updated_at { get; set; }
        public GetOrdersAddress address { get; set; }
    }

    public class GetOrdersAddress
    {
        public string city { get; set; }
    }

    public class GetOrdersImage
    {
        public string[] large_urls { get; set; }
        public string[] small_urls { get; set; }
        //public object[] large_urlshttpswwwbukalapakcomimagesmobilelogo_xl_squarepng { get; set; }
        //public object[] small_urlshttpswwwbukalapakcomimagesvirtual_productphonexlpng { get; set; }
    }

    public class GetOrdersUnit
    {
        public long id { get; set; }
        public GetOrdersProduct1 product { get; set; }
        public double price { get; set; }
        public GetOrdersImage1 image { get; set; }
        public string variant_name { get; set; }
        public string sku_name { get; set; }
        public double discount { get; set; }
    }

    public class GetOrdersProduct1
    {
        public string id { get; set; }
        public int price { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string condition { get; set; }
        public double weight { get; set; }
        public GetOrdersShipping1 shipping { get; set; }
        public bool assurance { get; set; }
        public string url { get; set; }
    }

    public class GetOrdersShipping1
    {
        public bool force_insurance { get; set; }
        //public object[] free_shipping_coverage { get; set; }
    }

    public class GetOrdersImage1
    {
        public string[] large_urls { get; set; }
        public string[] small_urls { get; set; }
    }

    public class GetOrdersCashback
    {
        public string type { get; set; }
        public double amount { get; set; }
    }
#endregion

}