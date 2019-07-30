using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Security;
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
            MoDbContext = new MoDbContext();
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
    }
}
