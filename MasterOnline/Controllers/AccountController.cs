using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;
using MasterOnline.Models;
using MasterOnline.Utils;
using MasterOnline.ViewModels;

namespace MasterOnline.Controllers
{
    public class AccountController : Controller
    {
        private MoDbContext MoDbContext;
        private AccountUserViewModel _viewModel;

        public AccountController()
        {
            MoDbContext = new MoDbContext();
            _viewModel = new AccountUserViewModel();
        }

        protected override void Dispose(bool disposing)
        {
            MoDbContext.Dispose();
        }

        // Route ke halaman login
        [Route("login")]
        public ActionResult Login()
        {
            return View();
        }

        // Proses Logging In dari Acc / User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoggingIn(Account account)
        {
            ModelState.Remove("NamaTokoOnline");

            if (!ModelState.IsValid)
                return View("Login", account);

            //Configuration connectionConfiguration = WebConfigurationManager.OpenWebConfiguration("~");
            var accFromDb = MoDbContext.Account.SingleOrDefault(a => a.Email == account.Email);

            if (accFromDb == null)
            {
                var userFromDb = MoDbContext.User.SingleOrDefault(a => a.Email == account.Email);


                if (userFromDb == null)
                {
                    ModelState.AddModelError(string.Empty, @"Username / Email tidak ditemukan!");
                    return View("Login", account);
                }

                var accInDb = MoDbContext.Account.Single(ac => ac.AccountId == userFromDb.AccountId);
                var key = accInDb.VCode;
                var originPassword = account.Password;
                var encodedPassword = Helper.EncodePassword(originPassword, key);
                var pass = userFromDb.Password;

                if (!encodedPassword.Equals(pass))
                {
                    ModelState.AddModelError(string.Empty, @"Password salah!");
                    return View("Login", account);
                }

                if (!userFromDb.Status)
                {
                    ModelState.AddModelError(string.Empty, @"Akun tidak aktif!");
                    return View("Login", account);
                }

                _viewModel.User = userFromDb;
                //var accByUser = MoDbContext.Account.Single(a => a.AccountId == userFromDb.AccountId);
                //connectionConfiguration.ConnectionStrings.ConnectionStrings["PerAccContext"].ConnectionString = $"Server=202.67.14.92\\SQLEXPRESS, 1433;initial catalog=ERASOFT_{accByUser.UserId};user id=masteronline;password=M@ster123;multipleactiveresultsets=True;application name=EntityFramework";
            }
            else
            {
                var pass = accFromDb.Password;
                var hashCode = accFromDb.VCode;
                var encodingPassString = Helper.EncodePassword(account.Password, hashCode);

                if (!encodingPassString.Equals(pass))
                {
                    ModelState.AddModelError(string.Empty, @"Password salah!");
                    return View("Login", account);
                }

                if (!accFromDb.Status)
                {
                    ModelState.AddModelError(string.Empty, @"Akun tidak aktif!");
                    return View("Login", account);
                }

                _viewModel.Account = accFromDb;
                //connectionConfiguration.ConnectionStrings.ConnectionStrings["PerAccContext"].ConnectionString = $"Server=202.67.14.92\\SQLEXPRESS, 1433;initial catalog=ERASOFT_{accFromDb.UserId};user id=masteronline;password=M@ster123;multipleactiveresultsets=True;application name=EntityFramework";
            }

            Session["SessionInfo"] = _viewModel;

            ErasoftContext erasoftContext = null;
            if (_viewModel?.Account != null)
            {
                erasoftContext = _viewModel.Account.UserId == "admin_manage" ? new ErasoftContext() : new ErasoftContext(_viewModel.Account.UserId);
            }
            else
            {
                var accFromUser = MoDbContext.Account.Single(a => a.AccountId == _viewModel.User.AccountId);
                erasoftContext = new ErasoftContext(accFromUser.UserId);
            }

            var dataUsahaInDb = erasoftContext.SIFSYS.Single(p => p.BLN == 1);
            var jumlahAkunMarketplace = erasoftContext.ARF01.Count();

            if (dataUsahaInDb?.NAMA_PT != "PT ERAKOMP INFONUSA" && jumlahAkunMarketplace > 0)
            {
                SyncMarketplace(erasoftContext);
                return RedirectToAction("Index", "Manage", "SyncMarketplace");
            }

            return RedirectToAction("Bantuan", "Manage");
        }

        protected void SyncMarketplace(ErasoftContext LocalErasoftDbContext)
        {
            //MoDbContext = new MoDbContext();
            AccountUserViewModel sessionData = System.Web.HttpContext.Current.Session["SessionInfo"] as AccountUserViewModel;
            //if (sessionData?.Account != null)
            //{
            //    if (sessionData.Account.UserId == "admin_manage")
            //        ErasoftDbContext = new ErasoftContext();
            //    else
            //        ErasoftDbContext = new ErasoftContext(sessionData.Account.UserId);
            //}
            //else
            //{
            //    if (sessionData?.User != null)
            //    {
            //        var accFromUser = MoDbContext.Account.Single(a => a.AccountId == sessionData.User.AccountId);
            //        ErasoftDbContext = new ErasoftContext(accFromUser.UserId);
            //    }
            //}
            var connectionID = Guid.NewGuid().ToString();
            //string username = sessionData.Account.Username;

            //#region bukalapak
            //var kdBL = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "BUKALAPAK");
            //var listBLShop = LocalErasoftDbContext.ARF01.Where(m => m.NAMA == kdBL.IdMarket.ToString()).ToList();
            //var blApi = new BukaLapakController();
            //if (listBLShop.Count > 0)
            //{
            //    foreach (ARF01 tblCustomer in listBLShop)
            //    {
            //        if (!string.IsNullOrEmpty(tblCustomer.TOKEN))
            //        {
            //            var stf02hinDB = LocalErasoftDbContext.STF02H.Where(p => !string.IsNullOrEmpty(p.BRG_MP) && p.IDMARKET == tblCustomer.RecNum).ToList();
            //            foreach (var item in stf02hinDB)
            //            {
            //                var barangInDb = LocalErasoftDbContext.STF02.SingleOrDefault(b => b.BRG == item.BRG);
            //                if (barangInDb != null)
            //                {
            //                    var qtyOnHand = 0d;
            //                    {
            //                        object[] spParams = {
            //                                new SqlParameter("@BRG", barangInDb.BRG),
            //                                new SqlParameter("@GD","ALL"),
            //                                new SqlParameter("@Satuan", "2"),
            //                                new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
            //                                new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}
            //                            };

            //                        LocalErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
            //                        qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
            //                    }
            //                    blApi.updateProduk(item.BRG_MP, "", (qtyOnHand > 0 ? qtyOnHand.ToString() : "1"), tblCustomer.API_KEY, tblCustomer.TOKEN);
            //                }

            //            }
            //        }
            //    }
            //}
            //#endregion

            #region lazada
            var kdLazada = MoDbContext.Marketplaces.SingleOrDefault(m => m.NamaMarket.ToUpper() == "LAZADA");
            var listLazadaShop = LocalErasoftDbContext.ARF01.Where(m => m.NAMA == kdLazada.IdMarket.ToString()).ToList();
            var lzdApi = new LazadaController();
            if (listLazadaShop.Count > 0)
            {
                foreach (ARF01 tblCustomer in listLazadaShop)
                {
                    if (!string.IsNullOrEmpty(tblCustomer.TOKEN))
                    {
                        #region refresh token lazada
                        lzdApi.GetRefToken(tblCustomer.CUST, tblCustomer.REFRESH_TOKEN);
                        lzdApi.GetShipment(tblCustomer.CUST, tblCustomer.TOKEN);
                        #endregion
                        //var stf02hinDB = LocalErasoftDbContext.STF02H.Where(p => !string.IsNullOrEmpty(p.BRG_MP) && p.IDMARKET == tblCustomer.RecNum).ToList();
                        //foreach (var item in stf02hinDB)
                        //{
                        //    var barangInDb = LocalErasoftDbContext.STF02.SingleOrDefault(b => b.BRG == item.BRG);
                        //    if (barangInDb != null)
                        //    {
                        //        var qtyOnHand = 0d;
                        //        {
                        //            object[] spParams = {
                        //                    new SqlParameter("@BRG", barangInDb.BRG),
                        //                    new SqlParameter("@GD","ALL"),
                        //                    new SqlParameter("@Satuan", "2"),
                        //                    new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                        //                    new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}
                        //                };

                        //            LocalErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                        //            qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                        //        }
                        //        lzdApi.UpdatePriceQuantity(item.BRG_MP, "", (qtyOnHand > 0 ? qtyOnHand.ToString() : "1"), tblCustomer.TOKEN);
                        //    }

                        //}
                    }
                }
            }
            #endregion

            #region Blibli
            var kdBli = MoDbContext.Marketplaces.Single(m => m.NamaMarket.ToUpper() == "BLIBLI");
            var listBLIShop = LocalErasoftDbContext.ARF01.Where(m => m.NAMA == kdBli.IdMarket.ToString()).ToList();
            if (listBLIShop.Count > 0)
            {
                var BliApi = new BlibliController();
                foreach (ARF01 tblCustomer in listBLIShop)
                {
                    if (!string.IsNullOrEmpty(tblCustomer.API_CLIENT_P) && !string.IsNullOrEmpty(tblCustomer.API_CLIENT_U))
                    {
                        BlibliController.BlibliAPIData data = new BlibliController.BlibliAPIData()
                        {
                            API_client_username = tblCustomer.API_CLIENT_U,
                            API_client_password = tblCustomer.API_CLIENT_P,
                            API_secret_key = tblCustomer.API_KEY,
                            mta_username_email_merchant = tblCustomer.EMAIL,
                            mta_password_password_merchant = tblCustomer.PASSWORD,
                        };
                        BliApi.GetToken(data, true);
                        BliApi.GetQueueFeedDetail(data, null);
                    }
                }
            }
            #endregion

            #region elevenia
            var kdEL = MoDbContext.Marketplaces.Single(m => m.NamaMarket.ToUpper() == "ELEVENIA");
            var listELShop = LocalErasoftDbContext.ARF01.Where(m => m.NAMA == kdEL.IdMarket.ToString()).ToList();
            if (listELShop.Count > 0)
            {
                var elApi = new EleveniaController();
                foreach (ARF01 tblCustomer in listELShop)
                {
                    //isi delivery temp
                    elApi.GetDeliveryTemp(Convert.ToString(tblCustomer.RecNum), Convert.ToString(tblCustomer.API_KEY));

                    ////cari yang brg_mp tidak null, per market
                    //var stf02hinDB = LocalErasoftDbContext.STF02H.Where(p => !string.IsNullOrEmpty(p.BRG_MP) && p.IDMARKET == tblCustomer.RecNum).ToList();
                    //foreach (var item in stf02hinDB)
                    //{
                    //    var barangInDb = LocalErasoftDbContext.STF02.SingleOrDefault(b => b.BRG == item.BRG);
                    //    if (barangInDb != null)
                    //    {
                    //        {
                    //            #region getUrlImage
                    //            string[] imgID = new string[3];
                    //            for (int i = 0; i < 3; i++)
                    //            {
                    //                imgID[i] = "http://masteronline.co.id/ele/image?id=" + $"FotoProduk-{barangInDb.USERNAME}-{barangInDb.BRG}-foto-{i + 1}.jpg";
                    //                imgID[i] = Convert.ToString(imgID[i]).Replace(" ", "%20");
                    //            }
                    //            #endregion
                    //            var qtyOnHand = 0d;
                    //            {
                    //                object[] spParams = {
                    //                        new SqlParameter("@BRG", barangInDb.BRG),
                    //                        new SqlParameter("@GD","ALL"),
                    //                        new SqlParameter("@Satuan", "2"),
                    //                        new SqlParameter("@THN", Convert.ToInt16(DateTime.Now.ToString("yyyy"))),
                    //                        new SqlParameter("@QOH", SqlDbType.Decimal) {Direction = ParameterDirection.Output}
                    //                    };

                    //                LocalErasoftDbContext.Database.ExecuteSqlCommand("exec [GetQOH_STF08A] @BRG, @GD, @Satuan, @THN, @QOH OUTPUT", spParams);
                    //                qtyOnHand = Convert.ToDouble(((SqlParameter)spParams[4]).Value);
                    //            }
                    //            EleveniaController.EleveniaProductData data = new EleveniaController.EleveniaProductData
                    //            {
                    //                api_key = tblCustomer.API_KEY,
                    //                kode = barangInDb.BRG,
                    //                nama = barangInDb.NAMA,
                    //                berat = (barangInDb.BERAT / 1000).ToString(),//MO save dalam Gram, Elevenia dalam Kilogram
                    //                imgUrl = imgID,
                    //                Keterangan = barangInDb.Deskripsi,
                    //                Qty = Convert.ToString(qtyOnHand),
                    //                DeliveryTempNo = item.DeliveryTempElevenia,
                    //                IDMarket = tblCustomer.RecNum.ToString(),
                    //            };
                    //            data.Brand = LocalErasoftDbContext.STF02E.SingleOrDefault(m => m.KODE == barangInDb.Sort2 && m.LEVEL == "2").KET;
                    //            data.Price = item.HJUAL.ToString();
                    //            data.kode_mp = item.BRG_MP;
                    //            elApi.UpdateProductQOH_Price(data);
                    //        }
                    //    }
                    //}

                    //var elApi = new EleveniaController();
                    //elApi.GetOrder(tblCustomer.API_KEY, EleveniaController.StatusOrder.Paid, connectionID, tblCustomer.CUST, tblCustomer.PERSO);
                }
            }
            #endregion
        }

        // Route ke halaman register
        [Route("register")]
        public ActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CekEmailPengguna(string emailPengguna)
        {
            var res = new CekKetersediaanData()
            {
                Email = emailPengguna
            };

            var accInDb = MoDbContext.Account.SingleOrDefault(a => a.Email.Contains(emailPengguna));
            if (accInDb != null)
            {
                res.Available = false;
                res.CekNull = accInDb.Username;
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        // Proses saving data account
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveAccount(Account account)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", account);
            }

            if (account.Password != account.ConfirmPassword)
            {
                ModelState.AddModelError("", @"Password konfirmasi tidak sama");
                return View("Register", account);
            }

            var accInDb = MoDbContext.Account.SingleOrDefault(a => a.Email == account.Email);

            if (accInDb != null)
            {
                ModelState.AddModelError("", @"Email sudah terdaftar!");
                return View("Register", account);
            }

            var keyNew = Helper.GeneratePassword(10);
            var originPassword = account.Password;
            var password = Helper.EncodePassword(account.Password, keyNew);

            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Uploaded/"), fileName);
                    account.PhotoKtpUrl = "~/Content/Uploaded/" + fileName;
                    file.SaveAs(path);
                }
            }

            if (String.IsNullOrWhiteSpace(account.PhotoKtpUrl))
            {
                ModelState.AddModelError("", @"Harap sertakan foto / scan KTP Anda!");
                return View("Register", account);
            }

            var email = new MailAddress(account.Email);
            account.UserId = email.User;
            account.Status = false; //User tidak aktif untuk pertama kali

            account.KODE_SUBSCRIPTION = "01"; //Free account
            account.TGL_SUBSCRIPTION = DateTime.Today.Date; //First time subs
            account.Password = password;
            account.ConfirmPassword = password;
            account.VCode = keyNew;

            MoDbContext.Account.Add(account);
            MoDbContext.SaveChanges();
            ModelState.Clear();

            //remark by calvin 2 oktober 2018, untuk testing dlu
            var body = "<p>Selamat, akun Anda berhasil didaftarkan pada sistem kami&nbsp;<img src=\"https://html-online.com/editor/tinymce4_6_5/plugins/emoticons/img/smiley-laughing.gif\" alt=\"laughing\" /></p>" +
                "<p>&nbsp;</p>" +
                "<p>Detail akun Anda ialah sebagai berikut,</p>" +
                "<p>Email: {0}</p>" +
                "<p>Password: {1}</p>" +
                "<p>Semoga sukses selalu dalam bisnis Anda di MasterOnline.</p><p>&nbsp;</p>" +
                "<p>Best regards,</p>" +
                "<p>CS MasterOnline.</p>";

            var message = new MailMessage();
            message.To.Add(email);
            message.From = new MailAddress("csmasteronline@gmail.com");
            message.Subject = "Pendaftaran MasterOnline berhasil!";
            message.Body = string.Format(body, account.Email, originPassword);
            message.IsBodyHtml = true;
#if AWS
            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "AKIAIXN2D33JPSDL7WEQ",
                    Password = "ApBddkFZF8hwJtbo+s4Oq31MqDtWOpzYKDhyVGSHGCEl"
                };
                smtp.Credentials = credential;
                smtp.Host = "email-smtp.us-east-1.amazonaws.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
            }
#else
            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "csmasteronline@gmail.com",
                    Password = "erasoft123"
                };
                smtp.Credentials = credential;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
            }
#endif
            //end remark by calvin 2 oktober 2018, untuk testing dlu

            //ViewData["SuccessMessage"] = $"Selamat, akun Anda berhasil didaftarkan! Klik <a href=\"{Url.Action("Login")}\">di sini</a> untuk login!";
            ViewData["SuccessMessage"] = $"Kami telah menerima pendaftaran Anda. Silakan menunggu <i>approval</i> dari admin kami, terima kasih.";

            return View("Register");
        }

        // Proses Logging Out + Hapus Session
        public ActionResult LoggingOut()
        {
            Session["SessionInfo"] = null;
            //add by Tri, clear session id from cookies
            Session.Abandon();
            Response.Cookies.Add(new System.Web.HttpCookie("ASP.NET_SessionId", ""));
            //end add by Tri, clear session id from cookies
            return RedirectToAction("Index", "Home");
        }

        // Route ke halaman lupa password
        [Route("remind")]
        public ActionResult Remind()
        {
            return View();
        }
    }
}