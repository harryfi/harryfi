using Erasoft.Function;
using MasterOnline.Models;
using MasterOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MasterOnline.Controllers
{
    public class BCAController : Controller
    {
        //static string client_id = "60fb0683-44b0-49a5-bc6b-e6a4ceb0c46b";
        //static string client_secret = "4d362ca3-f89b-450f-b1ea-d713f4a1441c";
        //static string api_key = "a604e42c-7a01-4117-84eb-8c367394b059";
        //static string api_secret = "83778479-63f8-4b61-921a-d956695c2a36";
        static string urlBCAApi = "https://sandbox.bca.co.id";
        AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
        // GET: BCA
        public ActionResult Index()
        {
            return View();
        }

        //public BCA_Auth getAuth()
        //{
        //    var ret = new BCA_Auth();
        //    var client_id = string.Empty;
        //    var client_secret = string.Empty;
        //    string urll = urlBCAApi + "/api/oauth/token";
        //    WebRequest myReq = WebRequest.Create(urll);
        //    myReq.Method = "POST";
        //    DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
        //    var dsSIFSYS = EDB.GetDataSet("", "SIFSYS", "SELECT TOP 1 * FROM SIFSYS");
        //    if (dsSIFSYS.Tables[0].Rows.Count > 0)
        //    {
        //        client_id = dsSIFSYS.Tables[0].Rows[0]["BCA_CLIENT_ID"].ToString();
        //        client_secret = dsSIFSYS.Tables[0].Rows[0]["BCA_CLIENT_SECRET"].ToString();
        //    }
        //    myReq.Headers.Add("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(client_id + ":" + client_secret))));
        //    string myData = "grant_type=client_credentials";
        //    myReq.ContentType = "application/x-www-form-urlencoded";
        //    try
        //    {
        //        Stream dataStream = myReq.GetRequestStream();
        //        dataStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Length);
        //        dataStream.Close();

        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        //        WebResponse response = myReq.GetResponse();
        //        Console.WriteLine(((HttpWebResponse)response).StatusDescription);
        //        dataStream = response.GetResponseStream();
        //        StreamReader reader = new StreamReader(dataStream);
        //        string responseFromServer = reader.ReadToEnd();

        //        ret = Newtonsoft.Json.JsonConvert.DeserializeObject(responseFromServer, typeof(BCA_Auth)) as BCA_Auth;
        //        reader.Close();
        //        dataStream.Close();
        //        response.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        ret = new BCA_Auth();
        //        ret.access_token = ex.ToString();
        //    }
        //    return ret;
        //}
        public async Task<BCA_Auth> getAuth()
        {
            var ret = new BCA_Auth();
            ret.ErrorMessage = new Errormessage();

            string Myprod = "grant_type=client_credentials";
            string urll = urlBCAApi + "/api/oauth/token";
            string client_id = "";
            string client_secret = "";
            DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
            var dsSIFSYS = EDB.GetDataSet("MOConnectionString", "SIFSYS", "SELECT TOP 1 * FROM SIFSYS");
            if (dsSIFSYS.Tables[0].Rows.Count > 0)
            {
                client_id = dsSIFSYS.Tables[0].Rows[0]["BCA_CLIENT_ID"].ToString();
                client_secret = dsSIFSYS.Tables[0].Rows[0]["BCA_CLIENT_SECRET"].ToString();
            }

            try
            {
                var client = new System.Net.Http.HttpClient();
                string url = string.Format(urll, string.Empty);
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", ("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(client_id + ":" + client_secret))));
                System.Net.Http.HttpResponseMessage response = null;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                var content = new System.Net.Http.StringContent(Myprod, Encoding.UTF8, "application/json");
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded");
                response = await client.PostAsync(url, content);


                if (response != null)
                {
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var contentR = await response.Content.ReadAsStringAsync();
                        client.Dispose();
                        response.Dispose();
                        ret = Newtonsoft.Json.JsonConvert.DeserializeObject(contentR, typeof(BCA_Auth)) as BCA_Auth;
                    }
                    //else
                    //{
                    //    return response.ToString();
                    //}
                }
                //return "";

            }
            catch (Exception ex)
            {
                //return ex.ToString();
                ret.ErrorMessage.English = ex.ToString();
            }

            return ret;
        }
        [Route("BCA/TransferBCA")]
        [HttpGet]
        public async Task<ActionResult> TransferBCA(string amount, string corpId, string formRek, string reffID, string supp)
        {
            string urll = "/banking/corporates/transfers";
            var api_secret = string.Empty;
            var api_key = string.Empty;
            WebRequest myReq = WebRequest.Create(urlBCAApi + urll);
            var ret = new BindingBase();
            ret.status = 0;
            var auth = await getAuth();
            if (auth.ErrorMessage == null)
            {
                DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);

                var dsSIFSYS = EDB.GetDataSet("", "SIFSYS", "SELECT TOP 1 * FROM SIFSYS");
                if (dsSIFSYS.Tables[0].Rows.Count > 0)
                {
                    api_key = dsSIFSYS.Tables[0].Rows[0]["BCA_API_KEY"].ToString();
                    api_secret = dsSIFSYS.Tables[0].Rows[0]["BCA_API_SECRET"].ToString();
                    if (!string.IsNullOrEmpty(api_secret) || !string.IsNullOrEmpty(api_key))
                    {
                        if (!string.IsNullOrEmpty(auth.access_token))
                        {
                            string timeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
                            TransferData data = new TransferData
                            {
                                Amount = amount,
                                CorporateID = corpId,
                                //BeneficiaryAccountNumber = "0201245681",
                                CurrencyCode = "IDR",
                                ReferenceID = reffID,
                                //Remark1 = "Transfer Test",
                                //Remark2 = "Online Transfer",
                                SourceAccountNumber = formRek,
                                TransactionDate = DateTime.Today.ToString("yyyy-MM-dd"),
                                TransactionID = DateTime.Now.ToString("HHmmssff")
                            };
                            var beneficiaryAccountNumber = EDB.GetFieldValue("MOConnectionString", "APF01", "SUPP = '" + supp + "'", "RekBca").ToString();
                            if (!string.IsNullOrEmpty(beneficiaryAccountNumber))
                            {
                                data.BeneficiaryAccountNumber = beneficiaryAccountNumber;
                                Utils.HttpRequest req = new Utils.HttpRequest();
                                string dataPost = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                                string signature = CreateSignature("POST", urll, auth.access_token, dataPost, timeStamp, api_secret);
                                var bindTransfer = Newtonsoft.Json.JsonConvert.DeserializeObject(await req.RequestJSONObjectBCA(Utils.HttpRequest.METHOD.POST, urlBCAApi + urll, auth.access_token, api_key, timeStamp, signature, dataPost), typeof(TransferResponse)) as TransferResponse;
                                if (bindTransfer != null)
                                {
                                    if (!string.IsNullOrEmpty(bindTransfer.ErrorCode))
                                    {
                                        ret.message = bindTransfer.ErrorMessage.Indonesian;
                                    }
                                    else
                                    {
                                        ret.status = 1;
                                        //update TGIRO = 1
                                        EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "UPDATE APT03A SET TGIRO = 1 WHERE BUKTI = '" + reffID + "'");
                                    }
                                }
                                else
                                {
                                    ret.message = "Failed to call BCA API";
                                }
                            }
                            else
                            {
                                ret.message = "Rekening BCA Supplier belum diisi";
                            }
                        }
                        else
                        {
                            ret.message = "Failed to get BCA Token";
                        }
                    }
                    else
                    {
                        ret.message = "anda belum mengisi data Rekening BCA.";
                    }
                }
                else
                {
                    ret.message = "anda belum melakukan setting.";
                }

            }
            else
            {
                ret.message = auth.ErrorMessage.English;
            }


            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        #region encript signature
        private string CreateSignature(string method, string url, string token, string body, string timeStamp, string api_secret)
        {
            api_secret = api_secret ?? "";
            string stringToSign = method + ":" + url + ":" + token + ":" + SHA256HexHashString(body) + ":" + timeStamp;
            //string stringToSign = method + ":" + url + ":" + token + ":" + body + ":" + timeStamp;
            //string stringToSign = "POST:/banking/corporates/transfers:lIWOt2p29grUo59bedBUrBY3pnzqQX544LzYPohcGHOuwn8AUEdUKS:e3cf5797ac4ac02f7dad89ed2c5f5615c9884b2d802a504e4aebb76f45b8bdfb:2016-02-03T10:00:00.000+07:00";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(api_secret);
            byte[] messageBytes = encoding.GetBytes(stringToSign);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                //return Convert.ToBase64String(hashmessage);
                return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
            }
        }
        private static string SHA256HexHashString(string StringIn)
        {
            string hashString;
            var encoding = new System.Text.ASCIIEncoding();

            using (var sha256 = SHA256Managed.Create())
            {
                var hash = sha256.ComputeHash(encoding.GetBytes(Regex.Replace(StringIn, @"\s", "")));
                hashString = ToHex(hash, false);
            }

            return hashString;
        }
        private static string ToHex(byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
            return result.ToString();
        }
        #endregion

        public ActionResult GetDataTransfer(string nobuk)
        {
            var ret = new BindingBCA();
            ret.status = 0;
            DatabaseSQL EDB = new DatabaseSQL(sessionData.Account.UserId);
            var dsAPT03 = EDB.GetDataSet("MOConnectionString", "APT03A", "SELECT * FROM APT03A WHERE BUKTI = '" + nobuk + "'");
            if (dsAPT03.Tables[0].Rows.Count > 0)
            {
                var dsAPF01 = EDB.GetDataSet("MOConnectionString", "APF01", "SELECT * FROM APF01 WHERE SUPP = '" + dsAPT03.Tables[0].Rows[0]["SUPP"].ToString() + "'");
                if (dsAPF01.Tables[0].Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(dsAPF01.Tables[0].Rows[0]["REKBCA"].ToString()))
                    {
                        var dsSIFSYS = EDB.GetDataSet("", "SIFSYS", "SELECT TOP 1 * FROM SIFSYS");
                        if (dsSIFSYS.Tables[0].Rows.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(dsSIFSYS.Tables[0].Rows[0]["CORPORATE_ID"].ToString()) || !string.IsNullOrEmpty(dsSIFSYS.Tables[0].Rows[0]["REKBCA"].ToString()))
                            {
                                ret.status = 1;
                                ret.CorporateID = dsSIFSYS.Tables[0].Rows[0]["CORPORATE_ID"].ToString();
                                ret.SourceAccountName = dsSIFSYS.Tables[0].Rows[0]["NAMAREKBCA"].ToString();
                                ret.SourceAccountNumber = dsSIFSYS.Tables[0].Rows[0]["REKBCA"].ToString();

                                ret.BeneficiaryAccountName = dsAPF01.Tables[0].Rows[0]["NAMAREKBCA"].ToString();
                                ret.BeneficiaryAccountNumber = dsAPF01.Tables[0].Rows[0]["REKBCA"].ToString();
                            }
                            else
                            {
                                ret.message = "anda belum mengisi data Rekening BCA.";
                            }
                        }
                        else
                        {
                            ret.message = "anda belum melakukan setting.";
                        }

                    }
                    else
                    {
                        ret.message = "anda belum mengisi data Rekening BCA untuk supplier ini.";
                    }

                }
                else
                {
                    ret.message = "Supplier not found.";
                }
            }
            else
            {
                ret.message = "No Bukti not found.";
            }

            //return ret;
            return Json(ret, JsonRequestBehavior.AllowGet);

        }
    }
}