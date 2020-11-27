using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Security;
using Erasoft.Function;
using MasterOnline.Models.Api;
using MasterOnline.Utils;
using MasterOnline.ViewModels;

namespace MasterOnline.Controllers
{
    public class WebApiController : ApiController
    {
        public MoDbContext MoDbContext { get; set; }
        public ErasoftContext ErasoftDbContext { get; set; }
        private AccountUserViewModel _viewModel;

        public WebApiController()
        {
            MoDbContext = new MoDbContext("");
            _viewModel = new AccountUserViewModel();
        }

        protected override void Dispose(bool disposing)
        {
            MoDbContext.Dispose();
        }

        private string GenerateNumber()
        {
            Random random = new Random();
            string r = "";
            int i;
            for (i = 0; i < 6; i++)
            {
                r += random.Next(0, 9).ToString();
            }
            return r;
        }

        [System.Web.Http.Route("api/lupapassword")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public async Task<IHttpActionResult> LupaPassword([FromBody]JsonData data)
        {
            try
            {
                JsonApi result;
                string apiKey = "";

                var re = Request;
                var headers = re.Headers;

                if (headers.Contains("X-API-KEY"))
                {
                    apiKey = headers.GetValues("X-API-KEY").First();
                }

                if (apiKey != "M@STERONLINE4P1K3Y")
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "Wrong API KEY!",
                        data = null
                    };

                    return Json(result);
                }

                var accInDb = MoDbContext.Account.SingleOrDefault(a => a.Email == data.Email);

                if (accInDb == null)
                {
                    result = new JsonApi()
                    {
                        code = 400,
                        message = "Email tidak terdaftar!",
                        data = null
                    };

                    return Json(result);
                }

                var randPassword = GenerateNumber();
                var encNewPassword = Helper.EncodePassword(randPassword, accInDb.VCode);
                var email = new MailAddress(data.Email);
                var body = "<p>Password baru Anda adalah {0}</p>" +
                           "<p>Untuk keamanan data Anda, silakan segera ubah password Anda.</p>" +
                           "Salam sukses!" +
                           "<p>Best regards,</p>" +
                           "<p>CS MasterOnline.</p>";

                var message = new MailMessage();
                message.To.Add(email);
                message.From = new MailAddress("csmasteronline@gmail.com");
                message.Subject = "Password Akun MasterOnline";
                message.Body = string.Format(body, randPassword);
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = "csmasteronline@gmail.com",
                        Password = "kmblwexkeretrwxv"
                    };
                    smtp.Credentials = credential;
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(message);
                }

                accInDb.Password = encNewPassword;
                accInDb.ConfirmPassword = encNewPassword;
                MoDbContext.SaveChanges();

                result = new JsonApi()
                {
                    code = 200,
                    message = "Success",
                    data = null
                };

                return Json(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [System.Web.Http.Route("api/refreshstokmp")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public async Task<IHttpActionResult> RefreshStokMP([FromBody]JsonData data)
        {

            try
            {
                JsonApi result;
                string apiKey = "";
                string dbPathEra = "";
                string userName = "";

                var re = Request;
                var headers = re.Headers;

                if (headers.Contains("X-API-KEY"))
                {
                    apiKey = headers.GetValues("X-API-KEY").First();
                }

                if (apiKey != "REFRESHSTOKMP_M@STERONLINE4P1K3Y")
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "Wrong API KEY!",
                        data = null
                    };

                    return Json(result);
                }

                if (headers.Contains("DBPATHERA"))
                {
                    dbPathEra = headers.GetValues("DBPATHERA").First();
                }
                else
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "DBPATHERA can not be empty!",
                        data = null
                    };

                    return Json(result);
                }

                if (headers.Contains("USERNAME"))
                {
                    userName = headers.GetValues("USERNAME").First();
                }
                else
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "USERNAME can not be empty!",
                        data = null
                    };

                    return Json(result);
                }

                await Task.Run(() => new StokControllerJob().updateStockMarketPlace_ForItemInSTF08A("", dbPathEra, userName));

                result = new JsonApi()
                {
                    code = 200,
                    message = "Success",
                    data = null
                };

                return Json(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [System.Web.Http.Route("api/updatestokmp")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public async Task<IHttpActionResult> UpdateStokMP([FromBody]JsonData data)
        {

            try
            {
                JsonApi result;
                string apiKey = "";
                string dbPathEra = "";
                string userName = "";

                var re = Request;
                var headers = re.Headers;

                if (headers.Contains("X-API-KEY"))
                {
                    apiKey = headers.GetValues("X-API-KEY").First();
                }

                if (apiKey != "UPDATESTOKMP_M@STERONLINE4P1K3Y")
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "Wrong API KEY!",
                        data = null
                    };

                    return Json(result);
                }

                if (headers.Contains("DBPATHERA"))
                {
                    dbPathEra = headers.GetValues("DBPATHERA").First();
                }
                else
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "DBPATHERA can not be empty!",
                        data = null
                    };

                    return Json(result);
                }

                if (headers.Contains("USERNAME"))
                {
                    userName = headers.GetValues("USERNAME").First();
                }
                else
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "USERNAME can not be empty!",
                        data = null
                    };

                    return Json(result);
                }

                if (data == null)
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "Kode Barang can not be empty!",
                        data = null
                    };

                    return Json(result);
                }

                var connID = "[UPDATESTOK_API_WH][" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddhhmmss") + "]";

                var EDB = new DatabaseSQL(dbPathEra);
                EDB.ExecuteSQL("MOConnectionString", System.Data.CommandType.Text, "INSERT INTO TEMP_ALL_MP_ORDER_ITEM (BRG, CONN_ID) VALUES ('" + data.brg + "', '" + connID + "')");

                await Task.Run(() => new StokControllerJob().updateStockMarketPlace(connID, dbPathEra, userName));

                result = new JsonApi()
                {
                    code = 200,
                    message = "Success",
                    data = null
                };

                return Json(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [System.Web.Http.Route("api/updatestatusmp")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        public async Task<IHttpActionResult> UpdateStatusMP([FromBody]JsonData data)
        {

            try
            {
                JsonApi result;
                string apiKey = "";
                string dbPathEra = "";
                string userName = "";

                var re = Request;
                var headers = re.Headers;

                if (headers.Contains("X-API-KEY"))
                {
                    apiKey = headers.GetValues("X-API-KEY").First();
                }

                if (apiKey != "UPDATESTATUSMP_M@STERONLINE4P1K3Y")
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "Wrong API KEY!",
                        data = null
                    };

                    return Json(result);
                }

                if (headers.Contains("DBPATHERA"))
                {
                    dbPathEra = headers.GetValues("DBPATHERA").First();
                }
                else
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "DBPATHERA can not be empty!",
                        data = null
                    };

                    return Json(result);
                }

                if (headers.Contains("USERNAME"))
                {
                    userName = headers.GetValues("USERNAME").First();
                }
                else
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "USERNAME can not be empty!",
                        data = null
                    };

                    return Json(result);
                }

                if (data.no_bukti == null)
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "No bukti pesanan can not be empty!",
                        data = null
                    };

                    return Json(result);
                }

                if (data.status_pesanan == null)
                {
                    result = new JsonApi()
                    {
                        code = 401,
                        message = "Status pesanan can not be empty!",
                        data = null
                    };

                    return Json(result);
                }

                //var connID = "[UPDATESTATUS_API_WH][" + DateTime.UtcNow.AddHours(7).ToString("yyyyMMddhhmmss") + "]";

                result = new JsonApi();

                var EDB = new DatabaseSQL(dbPathEra);
                string EraServerName = EDB.GetServerName("sConn");
                var ErasoftDbContext = new ErasoftContext(EraServerName, dbPathEra);

                var pesanan = ErasoftDbContext.SOT01A.AsNoTracking().Single(p => p.NO_BUKTI == data.no_bukti);
                var marketPlace = ErasoftDbContext.ARF01.AsNoTracking().Single(p => p.CUST == pesanan.CUST);
                var mp = MoDbContext.Marketplaces.AsNoTracking().Single(p => p.IdMarket.ToString() == marketPlace.NAMA);

                if (marketPlace.TIDAK_HIT_UANG_R == true)
                {
                    if (mp.NamaMarket.ToUpper().Contains("TOKOPEDIA"))
                    {
                        if (!string.IsNullOrEmpty(marketPlace.Sort1_Cust))
                        {
                            TokopediaControllerJob.TokopediaAPIData iden = new TokopediaControllerJob.TokopediaAPIData()
                            {
                                merchant_code = marketPlace.Sort1_Cust, //FSID
                                API_client_password = marketPlace.API_CLIENT_P, //Client ID
                                API_client_username = marketPlace.API_CLIENT_U, //Client Secret
                                API_secret_key = marketPlace.API_KEY, //Shop ID 
                                token = marketPlace.TOKEN,
                                idmarket = marketPlace.RecNum.Value,
                                DatabasePathErasoft = dbPathEra,
                                username = userName
                            };
                            var tokpedController = new TokopediaControllerJob();
                            Task.Run(() => tokpedController.PostAckOrder(dbPathEra, pesanan.NO_BUKTI, marketPlace.CUST, "Pesanan", "Accept Order", iden, pesanan.NO_BUKTI, pesanan.NO_REFERENSI).Wait());

                            result.code = 200;
                            result.message = "Success";
                            result.data = null;
                        }
                        else
                        {
                            result.code = 401;
                            result.message = "Apikey can not be empty ";
                            result.data = null;
                        }
                    }
                    else
                    {
                        result.code = 401;
                        result.message = "Marketplace is not Tokopedia";
                        result.data = null;
                    }
                }
                else
                {
                    result.code = 401;
                    result.message = "Marketplace not Active";
                    result.data = null;
                }

                return Json(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }
}
